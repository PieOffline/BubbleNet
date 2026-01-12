using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BubbleNet.Models;

namespace BubbleNet.Services
{
    /// <summary>
    /// Core networking service for BubbleNet.
    /// Handles TCP connections for sending and receiving transfers between local network peers.
    /// Uses a custom binary protocol for efficient data transfer.
    /// </summary>
    public class NetworkService : IDisposable
    {
        // ===== Port Configuration =====
        // BubbleNet uses specific ports to avoid conflicts with common services
        public const int DEFAULT_PORT = 16741;  // Primary port (16741 = "1 bubble" in leet speak)
        public const int ALT_PORT = 6741;       // Fallback port if primary is in use

        // ===== Network State =====
        private TcpListener? _listener;              // TCP listener for incoming connections
        private CancellationTokenSource? _cts;       // Token source for cancelling operations
        private bool _isRunning;                     // Whether the service is actively listening
        private int _activePort;                     // Currently bound port

        // ===== Events =====
        /// <summary>Fired when a transfer is received from another peer</summary>
        public event EventHandler<TransferItem>? TransferReceived;

        /// <summary>Fired when the service status changes (for UI updates)</summary>
        public event EventHandler<string>? StatusChanged;

        /// <summary>Fired when an error occurs during network operations</summary>
        public event EventHandler<string>? ErrorOccurred;

        // ===== Public Properties =====
        /// <summary>Whether the network service is currently running and accepting connections</summary>
        public bool IsRunning => _isRunning;

        /// <summary>The port number currently being used for listening</summary>
        public int ActivePort => _activePort;

        /// <summary>
        /// Gets the local IP address of this machine on the local network.
        /// Uses a UDP socket trick to find the primary network interface.
        /// </summary>
        public string LocalIP
        {
            get
            {
                try
                {
                    // Create a UDP socket and "connect" to a public IP
                    // This doesn't actually send data, but allows us to determine
                    // which local interface would be used for internet traffic
                    using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
                    socket.Connect("8.8.8.8", 65530);  // Google DNS, arbitrary port
                    var endPoint = socket.LocalEndPoint as IPEndPoint;
                    return endPoint?.Address.ToString() ?? "127.0.0.1";
                }
                catch
                {
                    // Fallback to enumerating network interfaces
                    return GetFallbackLocalIP();
                }
            }
        }

        /// <summary>
        /// Fallback method to get local IP by enumerating network interfaces.
        /// Used when the UDP socket method fails.
        /// </summary>
        private string GetFallbackLocalIP()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                // Find the first IPv4 address that isn't loopback
                if (ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip))
                {
                    return ip.ToString();
                }
            }
            return "127.0.0.1";  // Last resort fallback
        }

        /// <summary>
        /// Gets the last three octets of the local IP address.
        /// Used for generating word codes.
        /// </summary>
        /// <returns>Tuple of (octet2, octet3, octet4) from the IP address</returns>
        public (byte octet2, byte octet3, byte octet4) GetIPOctets()
        {
            var parts = LocalIP.Split('.');
            if (parts.Length != 4) return (0, 0, 0);

            return (
                byte.TryParse(parts[1], out var o2) ? o2 : (byte)0,
                byte.TryParse(parts[2], out var o3) ? o3 : (byte)0,
                byte.TryParse(parts[3], out var o4) ? o4 : (byte)0
            );
        }

        /// <summary>
        /// Gets the word code for this machine's IP address.
        /// Word codes are easier to remember and share than raw IP addresses.
        /// </summary>
        public string GetWordCode()
        {
            var octets = GetIPOctets();
            return FoodWords.GenerateWordCode(octets.octet2, octets.octet3, octets.octet4);
        }

        /// <summary>
        /// Starts listening for incoming connections on either the default or alternate port.
        /// Tries the alternate port first, then the default port if that fails.
        /// </summary>
        /// <returns>True if successfully started listening, false otherwise</returns>
        public Task<bool> StartListeningAsync()
        {
            // Don't restart if already running
            if (_isRunning) return Task.FromResult(true);

            // Create cancellation token for clean shutdown
            _cts = new CancellationTokenSource();

            // Try each port until one works
            foreach (var port in new[] { ALT_PORT, DEFAULT_PORT })
            {
                try
                {
                    // Create and start TCP listener
                    _listener = new TcpListener(IPAddress.Any, port);
                    _listener.Start();
                    _activePort = port;
                    _isRunning = true;

                    // Notify status change
                    StatusChanged?.Invoke(this, $"Listening on port {port}");

                    // Start accepting connections in background
                    _ = AcceptConnectionsAsync(_cts.Token);
                    return Task.FromResult(true);
                }
                catch (SocketException)
                {
                    // Port in use, try next one
                    _listener?.Stop();
                    continue;
                }
            }

            // All ports failed
            ErrorOccurred?.Invoke(this, "Failed to open ports 16741 or 6741");
            return Task.FromResult(false);
        }

        /// <summary>
        /// Stops listening for connections and cleans up resources.
        /// </summary>
        public void StopListening()
        {
            _cts?.Cancel();           // Signal cancellation
            _listener?.Stop();        // Stop accepting connections
            _isRunning = false;
            _activePort = 0;
            StatusChanged?.Invoke(this, "Stopped listening");
        }

        /// <summary>
        /// Main loop for accepting incoming TCP connections.
        /// Runs continuously until cancelled.
        /// </summary>
        private async Task AcceptConnectionsAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested && _listener != null)
            {
                try
                {
                    // Register cancellation to break out of AcceptTcpClientAsync
                    using var registration = ct.Register(() => _listener?.Stop());

                    // Wait for incoming connection
                    var client = await _listener.AcceptTcpClientAsync();

                    // Check if we were cancelled during the wait
                    if (ct.IsCancellationRequested)
                    {
                        client.Close();
                        break;
                    }

                    // Handle the client connection on a separate task
                    // This allows us to accept multiple connections concurrently
                    _ = HandleClientAsync(client, ct);
                }
                catch (ObjectDisposedException)
                {
                    // Listener was stopped - normal shutdown
                    break;
                }
                catch (SocketException) when (ct.IsCancellationRequested)
                {
                    // Socket exception due to cancellation - normal shutdown
                    break;
                }
                catch (Exception ex)
                {
                    // Unexpected error - log and continue
                    if (!ct.IsCancellationRequested)
                    {
                        ErrorOccurred?.Invoke(this, $"Accept error: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Handles an individual client connection, reading the transfer data.
        /// Protocol:
        /// 1. Read header length (4 bytes, int32)
        /// 2. Read header JSON (variable length)
        /// 3. Read content based on transfer type
        /// </summary>
        private async Task HandleClientAsync(TcpClient client, CancellationToken ct)
        {
            try
            {
                using var stream = client.GetStream();
                using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);

                // ===== Step 1: Read Header =====
                // Header contains metadata about the transfer
                var headerLength = reader.ReadInt32();
                var headerBytes = reader.ReadBytes(headerLength);
                var headerJson = Encoding.UTF8.GetString(headerBytes);
                var header = JsonSerializer.Deserialize<TransferHeader>(headerJson);

                if (header == null) return;

                // ===== Step 2: Create Transfer Item =====
                var item = new TransferItem
                {
                    Type = header.Type,
                    Name = header.Name,
                    FileName = header.FileName,
                    FileSize = header.FileSize,
                    SenderWordCode = header.SenderWordCode,
                    SenderIP = ((IPEndPoint?)client.Client.RemoteEndPoint)?.Address.ToString() ?? "Unknown",
                    ReceivedAt = DateTime.Now,
                    // Mubble properties
                    IsMubbleEncrypted = header.IsMubbleEncrypted,
                    MubbleCode = header.MubbleCode ?? "",
                    // StreamShare properties
                    StreamId = header.StreamId,
                    StreamSequence = header.StreamSequence,
                    // FileBubble properties
                    FileBubbleId = header.FileBubbleId
                };

                // ===== Step 3: Read Content Based on Type =====
                if (header.Type == TransferType.File ||
                    header.Type == TransferType.Image ||
                    header.Type == TransferType.Screenshot ||
                    header.Type == TransferType.StreamShare ||
                    header.Type == TransferType.FileBubble)
                {
                    // Binary data transfer - read length prefix then data
                    var dataLength = reader.ReadInt64();
                    item.FileData = new byte[dataLength];

                    // Read in chunks for large files
                    var totalRead = 0;
                    while (totalRead < dataLength)
                    {
                        var chunkSize = (int)Math.Min(8192, dataLength - totalRead);
                        var read = await stream.ReadAsync(item.FileData.AsMemory(totalRead, chunkSize), ct);
                        if (read == 0) break;  // Connection closed
                        totalRead += read;
                    }
                    item.FileSize = totalRead;
                }
                else
                {
                    // Text-based transfer (Text or Link)
                    var contentLength = reader.ReadInt32();
                    var contentBytes = reader.ReadBytes(contentLength);
                    item.Content = Encoding.UTF8.GetString(contentBytes);
                }

                // ===== Step 4: Notify Listeners =====
                TransferReceived?.Invoke(this, item);
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, $"Receive error: {ex.Message}");
            }
            finally
            {
                // Always close the client connection
                client.Close();
            }
        }

        /// <summary>
        /// Sends a transfer to the specified target word code.
        /// Tries both ports to maximize connection success.
        /// </summary>
        /// <param name="targetWordCode">The recipient's word code</param>
        /// <param name="item">The transfer item to send</param>
        /// <param name="mubbleEnabled">Whether to encrypt with Mubble</param>
        /// <param name="mubbleCode">The Mubble encryption code</param>
        /// <returns>True if send was successful</returns>
        public async Task<bool> SendTransferAsync(string targetWordCode, TransferItem item,
            bool mubbleEnabled = false, string mubbleCode = "")
        {
            try
            {
                // ===== Validate Word Code =====
                if (!FoodWords.IsValidWordCode(targetWordCode))
                {
                    ErrorOccurred?.Invoke(this, "Invalid word code - please check the format (Word1/Word2/Word3)");
                    return false;
                }

                // ===== Parse Target Word Code to IP =====
                var octets = FoodWords.ParseWordCode(targetWordCode);
                var localParts = LocalIP.Split('.');
                if (localParts.Length != 4) return false;

                // Fill in missing octets from local IP (assumes same subnet)
                int o2 = octets.octet2 > 0 ? octets.octet2 : int.Parse(localParts[1]);
                int o3 = octets.octet3 > 0 ? octets.octet3 : int.Parse(localParts[2]);
                int o4 = octets.octet4 > 0 ? octets.octet4 : int.Parse(localParts[3]);

                // Construct target IP using local subnet
                var targetIP = $"{localParts[0]}.{o2}.{o3}.{o4}";

                // ===== Apply Mubble Encryption if Enabled =====
                byte[]? fileDataToSend = item.FileData;
                string contentToSend = item.Content;

                if (mubbleEnabled && !string.IsNullOrEmpty(mubbleCode))
                {
                    // Encrypt file data
                    if (fileDataToSend != null)
                    {
                        fileDataToSend = MubbleService.Encrypt(fileDataToSend, mubbleCode);
                    }
                    // Encrypt text content
                    if (!string.IsNullOrEmpty(contentToSend))
                    {
                        contentToSend = MubbleService.EncryptString(contentToSend, mubbleCode);
                    }
                }

                // ===== Try Each Port =====
                foreach (var port in new[] { DEFAULT_PORT, ALT_PORT })
                {
                    try
                    {
                        using var client = new TcpClient();

                        // Connect with timeout
                        var connectTask = client.ConnectAsync(targetIP, port);
                        if (await Task.WhenAny(connectTask, Task.Delay(3000)) != connectTask)
                        {
                            continue;  // Connection timed out, try next port
                        }
                        await connectTask;  // Ensure connection completed

                        using var stream = client.GetStream();
                        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);

                        // ===== Create and Send Header =====
                        var header = new TransferHeader
                        {
                            Type = item.Type,
                            Name = item.Name,
                            FileName = item.FileName,
                            FileSize = item.FileSize,
                            SenderWordCode = GetWordCode(),
                            // Mubble properties
                            IsMubbleEncrypted = mubbleEnabled,
                            MubbleCode = mubbleCode,  // Sent in plaintext for matching
                            // StreamShare properties
                            StreamId = item.StreamId,
                            StreamSequence = item.StreamSequence,
                            // FileBubble properties
                            FileBubbleId = item.FileBubbleId
                        };

                        var headerJson = JsonSerializer.Serialize(header);
                        var headerBytes = Encoding.UTF8.GetBytes(headerJson);

                        // Write header length and header
                        writer.Write(headerBytes.Length);
                        writer.Write(headerBytes);

                        // ===== Send Content Based on Type =====
                        if (item.Type == TransferType.File ||
                            item.Type == TransferType.Image ||
                            item.Type == TransferType.Screenshot ||
                            item.Type == TransferType.StreamShare ||
                            item.Type == TransferType.FileBubble)
                        {
                            // Binary data - write length prefix then data
                            writer.Write((long)(fileDataToSend?.Length ?? 0));
                            if (fileDataToSend != null)
                            {
                                await stream.WriteAsync(fileDataToSend);
                            }
                        }
                        else
                        {
                            // Text data
                            var contentBytes = Encoding.UTF8.GetBytes(contentToSend);
                            writer.Write(contentBytes.Length);
                            writer.Write(contentBytes);
                        }

                        // Flush to ensure all data is sent
                        await stream.FlushAsync();
                        StatusChanged?.Invoke(this, $"Sent to {targetWordCode}");
                        return true;
                    }
                    catch
                    {
                        continue;  // Try next port
                    }
                }

                // All connection attempts failed
                ErrorOccurred?.Invoke(this, $"Could not connect to {targetWordCode}");
                return false;
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, $"Send error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Sends a transfer to multiple recipients (DoubleBubble).
        /// </summary>
        /// <param name="targetWordCodes">Array of recipient word codes</param>
        /// <param name="item">The transfer item to send</param>
        /// <param name="mubbleEnabled">Whether to encrypt with Mubble</param>
        /// <param name="mubbleCode">The Mubble encryption code</param>
        /// <returns>Number of successful sends</returns>
        public async Task<int> SendToMultipleAsync(string[] targetWordCodes, TransferItem item,
            bool mubbleEnabled = false, string mubbleCode = "")
        {
            int successCount = 0;

            // Send to each target concurrently
            var tasks = targetWordCodes.Select(async target =>
            {
                var success = await SendTransferAsync(target, item, mubbleEnabled, mubbleCode);
                if (success) Interlocked.Increment(ref successCount);
            });

            await Task.WhenAll(tasks);
            return successCount;
        }

        /// <summary>
        /// Cleans up network resources.
        /// </summary>
        public void Dispose()
        {
            StopListening();
            _cts?.Dispose();
        }

        /// <summary>
        /// Internal class for transfer header serialization.
        /// Contains all metadata about a transfer.
        /// </summary>
        private class TransferHeader
        {
            public TransferType Type { get; set; }
            public string Name { get; set; } = string.Empty;
            public string? FileName { get; set; }
            public long FileSize { get; set; }
            public string SenderWordCode { get; set; } = string.Empty;

            // Mubble encryption properties
            public bool IsMubbleEncrypted { get; set; }
            public string? MubbleCode { get; set; }

            // StreamShare properties
            public string? StreamId { get; set; }
            public int StreamSequence { get; set; }

            // FileBubble properties
            public string? FileBubbleId { get; set; }
        }
    }
}

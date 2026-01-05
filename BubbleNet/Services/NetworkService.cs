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
    public class NetworkService : IDisposable
    {
        public const int DEFAULT_PORT = 16741;
        public const int ALT_PORT = 6741;
        
        private TcpListener? _listener;
        private CancellationTokenSource? _cts;
        private bool _isRunning;
        private int _activePort;

        public event EventHandler<TransferItem>? TransferReceived;
        public event EventHandler<string>? StatusChanged;
        public event EventHandler<string>? ErrorOccurred;

        public bool IsRunning => _isRunning;
        public int ActivePort => _activePort;

        public string LocalIP
        {
            get
            {
                try
                {
                    using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
                    socket.Connect("8.8.8.8", 65530);
                    var endPoint = socket.LocalEndPoint as IPEndPoint;
                    return endPoint?.Address.ToString() ?? "127.0.0.1";
                }
                catch
                {
                    return GetFallbackLocalIP();
                }
            }
        }

        private string GetFallbackLocalIP()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip))
                {
                    return ip.ToString();
                }
            }
            return "127.0.0.1";
        }

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

        public string GetWordCode()
        {
            var octets = GetIPOctets();
            return FoodWords.GenerateWordCode(octets.octet2, octets.octet3, octets.octet4);
        }

        public async Task<bool> StartListeningAsync()
        {
            if (_isRunning) return true;

            _cts = new CancellationTokenSource();

            // Try default port first, then alternative
            foreach (var port in new[] { DEFAULT_PORT, ALT_PORT })
            {
                try
                {
                    _listener = new TcpListener(IPAddress.Any, port);
                    _listener.Start();
                    _activePort = port;
                    _isRunning = true;
                    StatusChanged?.Invoke(this, $"Listening on port {port}");
                    
                    _ = AcceptConnectionsAsync(_cts.Token);
                    return true;
                }
                catch (SocketException)
                {
                    _listener?.Stop();
                    continue;
                }
            }

            ErrorOccurred?.Invoke(this, "Failed to open ports 16741 or 6741");
            return false;
        }

        public void StopListening()
        {
            _cts?.Cancel();
            _listener?.Stop();
            _isRunning = false;
            _activePort = 0;
            StatusChanged?.Invoke(this, "Stopped listening");
        }

        private async Task AcceptConnectionsAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested && _listener != null)
            {
                try
                {
                    // Register cancellation to stop accepting
                    using var registration = ct.Register(() => _listener?.Stop());
                    var client = await _listener.AcceptTcpClientAsync();
                    if (ct.IsCancellationRequested)
                    {
                        client.Close();
                        break;
                    }
                    _ = HandleClientAsync(client, ct);
                }
                catch (ObjectDisposedException)
                {
                    // Listener was stopped
                    break;
                }
                catch (SocketException) when (ct.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    if (!ct.IsCancellationRequested)
                    {
                        ErrorOccurred?.Invoke(this, $"Accept error: {ex.Message}");
                    }
                }
            }
        }

        private async Task HandleClientAsync(TcpClient client, CancellationToken ct)
        {
            try
            {
                using var stream = client.GetStream();
                using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
                
                // Read header length and header
                var headerLength = reader.ReadInt32();
                var headerBytes = reader.ReadBytes(headerLength);
                var headerJson = Encoding.UTF8.GetString(headerBytes);
                var header = JsonSerializer.Deserialize<TransferHeader>(headerJson);
                
                if (header == null) return;

                var item = new TransferItem
                {
                    Type = header.Type,
                    Name = header.Name,
                    FileName = header.FileName,
                    FileSize = header.FileSize,
                    SenderWordCode = header.SenderWordCode,
                    SenderIP = ((IPEndPoint?)client.Client.RemoteEndPoint)?.Address.ToString() ?? "Unknown",
                    ReceivedAt = DateTime.Now
                };

                // Read content based on type
                if (header.Type == TransferType.File || header.Type == TransferType.Image || header.Type == TransferType.Screenshot)
                {
                    var dataLength = reader.ReadInt64();
                    item.FileData = new byte[dataLength];
                    var totalRead = 0;
                    while (totalRead < dataLength)
                    {
                        var read = await stream.ReadAsync(item.FileData.AsMemory(totalRead, (int)Math.Min(8192, dataLength - totalRead)), ct);
                        if (read == 0) break;
                        totalRead += read;
                    }
                    item.FileSize = totalRead;
                }
                else
                {
                    // Text or Link
                    var contentLength = reader.ReadInt32();
                    var contentBytes = reader.ReadBytes(contentLength);
                    item.Content = Encoding.UTF8.GetString(contentBytes);
                }

                TransferReceived?.Invoke(this, item);
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, $"Receive error: {ex.Message}");
            }
            finally
            {
                client.Close();
            }
        }

        public async Task<bool> SendTransferAsync(string targetWordCode, TransferItem item)
        {
            try
            {
                if (!FoodWords.IsValidWordCode(targetWordCode))
                {
                    ErrorOccurred?.Invoke(this, "Invalid word code - please check the format (Word1/Word2/Word3)");
                    return false;
                }
                
                var octets = FoodWords.ParseWordCode(targetWordCode);

                // Get local IP parts
                var localParts = LocalIP.Split('.');
                if (localParts.Length != 4) return false;

                // If ParseWordCode returned -1 for an octet, fill it from local IP
                int o2 = octets.octet2 > 0 ? octets.octet2 : int.Parse(localParts[1]);
                int o3 = octets.octet3 > 0 ? octets.octet3 : int.Parse(localParts[2]);
                int o4 = octets.octet4 > 0 ? octets.octet4 : int.Parse(localParts[3]);

                var targetIP = $"{localParts[0]}.{o2}.{o3}.{o4}";
                
                // Try both ports
                foreach (var port in new[] { DEFAULT_PORT, ALT_PORT })
                {
                    try
                    {
                        using var client = new TcpClient();
                        var connectTask = client.ConnectAsync(targetIP, port);
                        if (await Task.WhenAny(connectTask, Task.Delay(3000)) != connectTask)
                        {
                            continue;
                        }
                        await connectTask;

                        using var stream = client.GetStream();
                        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);

                        // Create header
                        var header = new TransferHeader
                        {
                            Type = item.Type,
                            Name = item.Name,
                            FileName = item.FileName,
                            FileSize = item.FileSize,
                            SenderWordCode = GetWordCode()
                        };

                        var headerJson = JsonSerializer.Serialize(header);
                        var headerBytes = Encoding.UTF8.GetBytes(headerJson);
                        
                        writer.Write(headerBytes.Length);
                        writer.Write(headerBytes);

                        // Write content
                        if (item.Type == TransferType.File || item.Type == TransferType.Image || item.Type == TransferType.Screenshot)
                        {
                            writer.Write((long)(item.FileData?.Length ?? 0));
                            if (item.FileData != null)
                            {
                                await stream.WriteAsync(item.FileData);
                            }
                        }
                        else
                        {
                            var contentBytes = Encoding.UTF8.GetBytes(item.Content);
                            writer.Write(contentBytes.Length);
                            writer.Write(contentBytes);
                        }

                        await stream.FlushAsync();
                        StatusChanged?.Invoke(this, $"Sent to {targetWordCode}");
                        return true;
                    }
                    catch
                    {
                        continue;
                    }
                }

                ErrorOccurred?.Invoke(this, $"Could not connect to {targetWordCode}");
                return false;
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, $"Send error: {ex.Message}");
                return false;
            }
        }

        public void Dispose()
        {
            StopListening();
            _cts?.Dispose();
        }

        private class TransferHeader
        {
            public TransferType Type { get; set; }
            public string Name { get; set; } = string.Empty;
            public string? FileName { get; set; }
            public long FileSize { get; set; }
            public string SenderWordCode { get; set; } = string.Empty;
        }
    }
}

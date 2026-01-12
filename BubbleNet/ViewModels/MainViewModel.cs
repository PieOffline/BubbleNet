using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using BubbleNet.Models;
using BubbleNet.Services;
using BubbleNet.Views;
using Microsoft.Win32;

namespace BubbleNet.ViewModels
{
    /// <summary>
    /// Main view model for the BubbleNet application.
    /// Handles all user interactions, network operations, and state management.
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged, IDisposable
    {
        // ===== Services =====
        private readonly NetworkService _networkService;      // Handles network communication
        private readonly SoundService _soundService;          // Plays audio feedback
        private readonly StreamShareService _streamService;   // Manages screen streaming
        private readonly FileBubbleService _fileBubbleService; // Manages file collections

        // ===== Application Settings =====
        private Settings _settings;  // Persisted user preferences

        // ===== URL Validation =====
        // Regex pattern for validating URLs before opening/sending
        // Allows http/https protocols with proper domain format
        private static readonly Regex SafeUrlPattern = new(
            @"^https?://[a-zA-Z0-9][-a-zA-Z0-9]*(\.[a-zA-Z0-9][-a-zA-Z0-9]*)+(:[0-9]+)?(/.*)?$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public event PropertyChangedEventHandler? PropertyChanged;

        #region Connection Properties

        private bool _isConnected;
        /// <summary>Whether the application is connected to the mesh network</summary>
        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                _isConnected = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ConnectionStatus));
            }
        }

        /// <summary>Display string showing connection status with emoji indicator</summary>
        public string ConnectionStatus => IsConnected ? "🟢 Connected" : "🔴 Disconnected";

        private string _wordCode = "---/---/---";
        /// <summary>This device's word code for receiving transfers</summary>
        public string WordCode
        {
            get => _wordCode;
            set { _wordCode = value; OnPropertyChanged(); }
        }

        private string _localIP = "Getting IP...";
        /// <summary>This device's local IP address</summary>
        public string LocalIP
        {
            get => _localIP;
            set { _localIP = value; OnPropertyChanged(); }
        }

        private int _activePort;
        /// <summary>The port number currently in use for listening</summary>
        public int ActivePort
        {
            get => _activePort;
            set
            {
                _activePort = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PortDisplay));
            }
        }

        /// <summary>Display string showing active port</summary>
        public string PortDisplay => ActivePort > 0 ? $"Port: {ActivePort}" : "Port: ---";

        #endregion

        #region Notification Properties

        private string _notificationMessage = "Welcome to BubbleNet! Press 'Mesh Me' to start sharing.";
        /// <summary>Current notification/status message displayed to user</summary>
        public string NotificationMessage
        {
            get => _notificationMessage;
            set { _notificationMessage = value; OnPropertyChanged(); }
        }

        #endregion

        #region Toggle Properties

        private bool _autoDeny;
        /// <summary>When enabled, automatically rejects all incoming transfers</summary>
        public bool AutoDeny
        {
            get => _autoDeny;
            set
            {
                _autoDeny = value;
                OnPropertyChanged();
                _soundService.PlayToggle();
                _settings.AutoDeny = value;
                NotificationMessage = value
                    ? "Auto-deny is ON - All incoming transfers will be rejected"
                    : "Auto-deny is OFF - Ready to receive transfers";
            }
        }

        private bool _autoOpen = true;
        /// <summary>When enabled, automatically opens received links in browser</summary>
        public bool AutoOpen
        {
            get => _autoOpen;
            set
            {
                _autoOpen = value;
                OnPropertyChanged();
                _soundService.PlayToggle();
                _settings.AutoOpen = value;
                NotificationMessage = value
                    ? "Auto-open is ON - Links will open automatically in browser"
                    : "Auto-open is OFF";
            }
        }

        #endregion

        #region Send Properties

        private string _targetWordCode = "";
        /// <summary>The recipient's word code for sending transfers</summary>
        public string TargetWordCode
        {
            get => _targetWordCode;
            set { _targetWordCode = value; OnPropertyChanged(); }
        }

        private string _textToSend = "";
        /// <summary>Text content to send (for Text transfer type)</summary>
        public string TextToSend
        {
            get => _textToSend;
            set { _textToSend = value; OnPropertyChanged(); }
        }

        private string _linkToSend = "";
        /// <summary>URL to send (for Link transfer type)</summary>
        public string LinkToSend
        {
            get => _linkToSend;
            set { _linkToSend = value; OnPropertyChanged(); }
        }

        private TransferType _selectedPayloadType = TransferType.File;
        /// <summary>Currently selected payload type for the redesigned send menu</summary>
        public TransferType SelectedPayloadType
        {
            get => _selectedPayloadType;
            set
            {
                _selectedPayloadType = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedPayloadIcon));
                OnPropertyChanged(nameof(SelectedPayloadName));
            }
        }

        /// <summary>Icon for the currently selected payload type</summary>
        public string SelectedPayloadIcon => SelectedPayloadType switch
        {
            TransferType.File => "📁",
            TransferType.Text => "📝",
            TransferType.Link => "🔗",
            TransferType.Screenshot => "📸",
            TransferType.StreamShare => "📺",
            TransferType.FileBubble => "📦",
            _ => "📦"
        };

        /// <summary>Name of the currently selected payload type</summary>
        public string SelectedPayloadName => SelectedPayloadType switch
        {
            TransferType.File => "File",
            TransferType.Text => "Text",
            TransferType.Link => "Link",
            TransferType.Screenshot => "Screenshot",
            TransferType.StreamShare => "Stream Share",
            TransferType.FileBubble => "FileBubble",
            _ => "Unknown"
        };

        private bool _isDoubleBubbleEnabled;
        /// <summary>Whether DoubleBubble (multi-recipient) mode is enabled</summary>
        public bool IsDoubleBubbleEnabled
        {
            get => _isDoubleBubbleEnabled;
            set
            {
                _isDoubleBubbleEnabled = value;
                OnPropertyChanged();
                _soundService.PlayToggle();
            }
        }

        private string _doubleBubbleTargets = "";
        /// <summary>Comma-separated word codes for DoubleBubble recipients</summary>
        public string DoubleBubbleTargets
        {
            get => _doubleBubbleTargets;
            set { _doubleBubbleTargets = value; OnPropertyChanged(); }
        }

        #endregion

        #region Stream Properties

        private bool _isStreaming;
        /// <summary>Whether a stream share session is currently active</summary>
        public bool IsStreaming
        {
            get => _isStreaming;
            set { _isStreaming = value; OnPropertyChanged(); }
        }

        #endregion

        #region Received Items

        private TransferItem? _selectedReceivedItem;
        /// <summary>Currently selected item in the received items list</summary>
        public TransferItem? SelectedReceivedItem
        {
            get => _selectedReceivedItem;
            set { _selectedReceivedItem = value; OnPropertyChanged(); }
        }

        /// <summary>Collection of received transfer items</summary>
        public ObservableCollection<TransferItem> ReceivedItems { get; } = new();

        /// <summary>Collection of active stream sessions (for viewing received streams)</summary>
        public ObservableCollection<StreamSession> ActiveStreams { get; } = new();

        /// <summary>Collection of FileBubble sessions</summary>
        public ObservableCollection<FileBubbleSession> FileBubbleSessions { get; } = new();

        #endregion

        #region Commands

        public ICommand MeshMeCommand { get; }
        public ICommand DisconnectCommand { get; }
        public ICommand SendCommand { get; }
        public ICommand SendFileCommand { get; }
        public ICommand SendTextCommand { get; }
        public ICommand SendLinkCommand { get; }
        public ICommand SendScreenshotCommand { get; }
        public ICommand StartStreamCommand { get; }
        public ICommand StopStreamCommand { get; }
        public ICommand OpenItemCommand { get; }
        public ICommand DownloadItemCommand { get; }
        public ICommand CopyItemCommand { get; }
        public ICommand CopyWordCodeCommand { get; }
        public ICommand OpenSettingsCommand { get; }
        public ICommand SelectPayloadTypeCommand { get; }
        public ICommand DownloadFileBubbleCommand { get; }

        #endregion

        #region Settings Access

        /// <summary>Application settings (for data binding)</summary>
        public Settings Settings => _settings;

        #endregion

        /// <summary>
        /// Initializes the MainViewModel with all services and commands.
        /// </summary>
        public MainViewModel()
        {
            // ===== Load Settings =====
            _settings = Settings.Load();

            // ===== Initialize Services =====
            _networkService = new NetworkService();
            _soundService = SoundService.Instance;
            _streamService = new StreamShareService();
            _fileBubbleService = new FileBubbleService();

            // Apply loaded settings
            _soundService.SoundEnabled = _settings.SoundEnabled;
            _autoDeny = _settings.AutoDeny;
            _autoOpen = _settings.AutoOpen;
            _doubleBubbleTargets = _settings.DoubleBubbleTargets;

            // ===== Subscribe to Network Events =====
            _networkService.TransferReceived += OnTransferReceived;
            _networkService.StatusChanged += (s, msg) => NotificationMessage = msg;
            _networkService.ErrorOccurred += (s, err) =>
            {
                NotificationMessage = $"⚠️ {err}";
                _soundService.PlayError();
            };

            // ===== Subscribe to Stream Events =====
            _streamService.StreamImageCaptured += OnStreamImageCaptured;

            // ===== Initialize Commands =====
            MeshMeCommand = new RelayCommand(async () => await ConnectAsync(), () => !IsConnected);
            DisconnectCommand = new RelayCommand(Disconnect, () => IsConnected);

            // Generic send command that routes based on selected payload type
            SendCommand = new RelayCommand(async () => await SendCurrentPayloadAsync(),
                () => IsConnected && !string.IsNullOrWhiteSpace(TargetWordCode));

            // Specific send commands
            SendFileCommand = new RelayCommand(async () => await SendFileAsync(),
                () => IsConnected && !string.IsNullOrWhiteSpace(TargetWordCode));
            SendTextCommand = new RelayCommand(async () => await SendTextAsync(),
                () => IsConnected && !string.IsNullOrWhiteSpace(TargetWordCode) && !string.IsNullOrWhiteSpace(TextToSend));
            SendLinkCommand = new RelayCommand(async () => await SendLinkAsync(),
                () => IsConnected && !string.IsNullOrWhiteSpace(TargetWordCode) && !string.IsNullOrWhiteSpace(LinkToSend));
            SendScreenshotCommand = new RelayCommand(async () => await SendScreenshotAsync(),
                () => IsConnected && !string.IsNullOrWhiteSpace(TargetWordCode));

            // Stream commands
            StartStreamCommand = new RelayCommand(async () => await StartStreamAsync(),
                () => IsConnected && !string.IsNullOrWhiteSpace(TargetWordCode) && !IsStreaming);
            StopStreamCommand = new RelayCommand(async () => await StopStreamAsync(),
                () => IsStreaming);

            // Item action commands
            OpenItemCommand = new RelayCommand<TransferItem>(OpenItem);
            DownloadItemCommand = new RelayCommand<TransferItem>(DownloadItem);
            CopyItemCommand = new RelayCommand<TransferItem>(CopyItem);
            CopyWordCodeCommand = new RelayCommand(CopyWordCode, () => IsConnected);

            // Settings command
            OpenSettingsCommand = new RelayCommand(OpenSettings);

            // Payload type selection command
            SelectPayloadTypeCommand = new RelayCommand<TransferType>(SelectPayloadType);

            // FileBubble download command
            DownloadFileBubbleCommand = new RelayCommand<FileBubbleSession>(DownloadFileBubble);
        }

        #region Connection Methods

        /// <summary>
        /// Connects to the mesh network by starting the TCP listener.
        /// </summary>
        private async Task ConnectAsync()
        {
            _soundService.PlayClick();
            NotificationMessage = "Connecting to mesh network...";

            var success = await _networkService.StartListeningAsync();
            if (success)
            {
                IsConnected = true;
                LocalIP = _networkService.LocalIP;
                WordCode = _networkService.GetWordCode();
                ActivePort = _networkService.ActivePort;

                // Show abbreviated word code in notification (just the last word)
                var lastWord = WordCode.Substring(WordCode.LastIndexOf('/') + 1);
                NotificationMessage = $"✅ Connected! Share your word code: {lastWord}";
                _soundService.PlayConnect();
            }
        }

        /// <summary>
        /// Disconnects from the mesh network.
        /// </summary>
        private void Disconnect()
        {
            _soundService.PlayClick();

            // Stop any active stream
            if (IsStreaming)
            {
                _streamService.StopStreamAsync().Wait();
                IsStreaming = false;
            }

            _networkService.StopListening();
            IsConnected = false;
            WordCode = "---/---/---";
            ActivePort = 0;
            NotificationMessage = "Disconnected from mesh network.";
            _soundService.PlayDisconnect();
        }

        #endregion

        #region Receive Methods

        /// <summary>
        /// Handles incoming transfers from the network service.
        /// Applies Mubble decryption if needed and adds to received items.
        /// </summary>
        private void OnTransferReceived(object? sender, TransferItem item)
        {
            // Check if auto-deny is enabled
            if (AutoDeny)
            {
                NotificationMessage = $"⛔ Auto-denied transfer from {item.SenderWordCode}";
                return;
            }

            // Handle on UI thread
            Application.Current.Dispatcher.Invoke(() =>
            {
                // ===== Handle Mubble Decryption =====
                if (item.IsMubbleEncrypted)
                {
                    // Check if we have Mubble enabled with matching code
                    if (_settings.MubbleEnabled && MubbleService.CodesMatch(item.MubbleCode, _settings.MubbleCode))
                    {
                        // Decrypt the payload
                        if (item.FileData != null)
                        {
                            item.FileData = MubbleService.Decrypt(item.FileData, _settings.MubbleCode);
                        }
                        if (!string.IsNullOrEmpty(item.Content))
                        {
                            item.Content = MubbleService.DecryptString(item.Content, _settings.MubbleCode);
                        }
                        NotificationMessage = $"🔓 Decrypted {item.Type} from {item.SenderWordCode}";
                    }
                    else
                    {
                        NotificationMessage = $"🔐 Encrypted transfer from {item.SenderWordCode} - codes don't match";
                        // Still add to list but mark as encrypted
                    }
                }

                // ===== Handle StreamShare =====
                if (item.Type == TransferType.StreamShare && !string.IsNullOrEmpty(item.StreamId))
                {
                    HandleStreamShareReceived(item);
                    return;
                }

                // ===== Handle FileBubble =====
                if (item.Type == TransferType.FileBubble && _settings.FileBubbleEnabled)
                {
                    HandleFileBubbleReceived(item);
                }

                // Add to received items list
                ReceivedItems.Insert(0, item);
                NotificationMessage = $"📥 Received {item.Type} from {item.SenderWordCode}";
                _soundService.PlayReceive();

                // ===== Auto-open Links =====
                if (AutoOpen && item.Type == TransferType.Link && !string.IsNullOrWhiteSpace(item.Content))
                {
                    if (IsValidAndSafeUrl(item.Content))
                    {
                        try
                        {
                            Process.Start(new ProcessStartInfo(item.Content) { UseShellExecute = true });
                        }
                        catch { /* Ignore open errors */ }
                    }
                    else
                    {
                        NotificationMessage = $"⚠️ Auto-open skipped: URL did not pass validation";
                    }
                }
            });
        }

        /// <summary>
        /// Handles received stream share images.
        /// </summary>
        private void HandleStreamShareReceived(TransferItem item)
        {
            // Find or create stream session
            var session = ActiveStreams.FirstOrDefault(s => s.StreamId == item.StreamId);
            if (session == null)
            {
                session = new StreamSession
                {
                    StreamId = item.StreamId!,
                    TargetWordCode = item.SenderWordCode,
                    IsReceiving = true,
                    StartTime = DateTime.Now
                };
                ActiveStreams.Add(session);
                _streamService.RegisterReceivedStream(item.StreamId!, item.SenderWordCode);
            }

            session.ImageCount = item.StreamSequence;

            // Also add to received items for viewing
            ReceivedItems.Insert(0, item);
            NotificationMessage = $"📺 Stream image #{item.StreamSequence} from {item.SenderWordCode}";
        }

        /// <summary>
        /// Handles received FileBubble files.
        /// </summary>
        private void HandleFileBubbleReceived(TransferItem item)
        {
            var sessionId = _fileBubbleService.AddFileAutoSession(item.SenderWordCode, item);

            // Update UI collection
            var existingSession = FileBubbleSessions.FirstOrDefault(s => s.SessionId == sessionId);
            if (existingSession == null)
            {
                var newSession = _fileBubbleService.GetSession(sessionId);
                if (newSession != null)
                {
                    FileBubbleSessions.Add(newSession);
                }
            }
        }

        #endregion

        #region Send Methods

        /// <summary>
        /// Sends the currently selected payload type.
        /// Routes to the appropriate send method based on SelectedPayloadType.
        /// </summary>
        private async Task SendCurrentPayloadAsync()
        {
            switch (SelectedPayloadType)
            {
                case TransferType.File:
                    await SendFileAsync();
                    break;
                case TransferType.Text:
                    await SendTextAsync();
                    break;
                case TransferType.Link:
                    await SendLinkAsync();
                    break;
                case TransferType.Screenshot:
                    await SendScreenshotAsync();
                    break;
                case TransferType.StreamShare:
                    if (IsStreaming)
                        await StopStreamAsync();
                    else
                        await StartStreamAsync();
                    break;
            }
        }

        /// <summary>
        /// Opens file dialog and sends selected file.
        /// Supports DoubleBubble (multi-recipient) mode.
        /// </summary>
        private async Task SendFileAsync()
        {
            var dialog = new OpenFileDialog
            {
                Title = "Select File to Send",
                Filter = "All Files (*.*)|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var fileInfo = new FileInfo(dialog.FileName);
                    var item = new TransferItem
                    {
                        Type = TransferType.File,
                        Name = fileInfo.Name,
                        FileName = fileInfo.Name,
                        FileData = await File.ReadAllBytesAsync(dialog.FileName),
                        FileSize = fileInfo.Length
                    };

                    await SendItemAsync(item, $"📤 Sending {item.FileName}...", $"✅ Sent {item.FileName}");
                }
                catch (Exception ex)
                {
                    NotificationMessage = $"⚠️ Error: {ex.Message}";
                    _soundService.PlayError();
                }
            }
        }

        /// <summary>
        /// Sends the text content in TextToSend.
        /// </summary>
        private async Task SendTextAsync()
        {
            // Prevent sending to self (causes issues)
            if (ConvertToWordCode(TargetWordCode) == _networkService.GetWordCode())
            {
                NotificationMessage = "⚠️ Please don't send text to yourself. It bugs out.";
                _soundService.PlayError();
                return;
            }

            var item = new TransferItem
            {
                Type = TransferType.Text,
                Name = "Text Message",
                Content = TextToSend
            };

            var success = await SendItemAsync(item, "📤 Sending text...", "✅ Sent text");
            if (success)
            {
                TextToSend = "";  // Clear input on success
            }
        }

        /// <summary>
        /// Sends the URL in LinkToSend.
        /// Validates URL format and adds https:// if needed.
        /// </summary>
        private async Task SendLinkAsync()
        {
            var url = LinkToSend.Trim();

            // Add https:// prefix if no protocol specified
            if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                url = "https://" + url;
            }

            // Validate URL before sending
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
                (uri.Scheme != "http" && uri.Scheme != "https"))
            {
                NotificationMessage = "⚠️ Invalid URL format. Please enter a valid http or https URL.";
                _soundService.PlayError();
                return;
            }

            var item = new TransferItem
            {
                Type = TransferType.Link,
                Name = "Link",
                Content = url
            };

            var success = await SendItemAsync(item, "📤 Sending link...", "✅ Sent link");
            if (success)
            {
                LinkToSend = "";  // Clear input on success
            }
        }

        /// <summary>
        /// Captures a screenshot and sends it.
        /// Saves locally as PNG if SaveScreenshotsLocally is enabled.
        /// </summary>
        private async Task SendScreenshotAsync()
        {
            try
            {
                // Minimize window to capture clean screenshot
                var mainWindow = Application.Current.MainWindow;
                var previousState = mainWindow.WindowState;
                mainWindow.WindowState = WindowState.Minimized;
                await Task.Delay(300);  // Wait for window animation

                // Capture screen dimensions
                var screenWidth = (int)SystemParameters.PrimaryScreenWidth;
                var screenHeight = (int)SystemParameters.PrimaryScreenHeight;

                // Capture screen using System.Drawing
                using var bitmap = new System.Drawing.Bitmap(screenWidth, screenHeight);
                using var graphics = System.Drawing.Graphics.FromImage(bitmap);
                graphics.CopyFromScreen(0, 0, 0, 0, new System.Drawing.Size(screenWidth, screenHeight));

                // Convert to PNG byte array
                using var ms = new MemoryStream();
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                var screenshotData = ms.ToArray();

                // Generate filename with timestamp
                var filename = $"Screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png";

                // ===== Save Screenshot Locally as PNG =====
                if (_settings.SaveScreenshotsLocally)
                {
                    try
                    {
                        // Ensure screenshot directory exists
                        if (!Directory.Exists(_settings.ScreenshotSavePath))
                        {
                            Directory.CreateDirectory(_settings.ScreenshotSavePath);
                        }

                        // Save as PNG file directly
                        var savePath = Path.Combine(_settings.ScreenshotSavePath, filename);
                        await File.WriteAllBytesAsync(savePath, screenshotData);
                        NotificationMessage = $"📸 Screenshot saved to {savePath}";
                    }
                    catch (Exception ex)
                    {
                        NotificationMessage = $"⚠️ Could not save screenshot locally: {ex.Message}";
                    }
                }

                // Restore window
                mainWindow.WindowState = previousState;
                mainWindow.Activate();

                // Create transfer item
                var item = new TransferItem
                {
                    Type = TransferType.Screenshot,
                    Name = filename,
                    FileName = filename,
                    FileData = screenshotData,
                    FileSize = screenshotData.Length
                };

                await SendItemAsync(item, "📤 Sending screenshot...", "✅ Sent screenshot");
            }
            catch (Exception ex)
            {
                NotificationMessage = $"⚠️ Screenshot error: {ex.Message}";
                _soundService.PlayError();
            }
        }

        /// <summary>
        /// Helper method to send a transfer item.
        /// Handles Mubble encryption and DoubleBubble routing.
        /// </summary>
        private async Task<bool> SendItemAsync(TransferItem item, string sendingMessage, string successMessage)
        {
            NotificationMessage = sendingMessage;

            // Get targets (single or DoubleBubble)
            string[] targets;
            if (IsDoubleBubbleEnabled && !string.IsNullOrWhiteSpace(DoubleBubbleTargets))
            {
                // Parse comma-separated targets
                targets = DoubleBubbleTargets.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim())
                    .Where(t => !string.IsNullOrEmpty(t))
                    .ToArray();

                // Add the primary target if not already included
                if (!string.IsNullOrWhiteSpace(TargetWordCode) && !targets.Contains(TargetWordCode))
                {
                    targets = targets.Prepend(TargetWordCode).ToArray();
                }
            }
            else
            {
                targets = new[] { TargetWordCode };
            }

            // Send to all targets
            int successCount = 0;
            foreach (var target in targets)
            {
                var success = await _networkService.SendTransferAsync(
                    target,
                    item,
                    _settings.MubbleEnabled,
                    _settings.MubbleCode);

                if (success) successCount++;
            }

            if (successCount > 0)
            {
                var targetInfo = successCount > 1 ? $"{successCount} recipients" : TargetWordCode;
                NotificationMessage = $"{successMessage} to {targetInfo}";
                _soundService.PlaySend();
                return true;
            }

            return false;
        }

        #endregion

        #region Stream Methods

        /// <summary>
        /// Starts a screen streaming session.
        /// </summary>
        private async Task StartStreamAsync()
        {
            try
            {
                var streamId = await _streamService.StartStreamAsync(
                    TargetWordCode,
                    _settings.StreamShareInterval);

                IsStreaming = true;

                // Show streaming popup window
                var streamWindow = new StreamShareWindow(_streamService, streamId, TargetWordCode);
                streamWindow.Owner = Application.Current.MainWindow;
                streamWindow.StopRequested += async (s, e) => await StopStreamAsync();
                streamWindow.Show();

                NotificationMessage = $"📺 Started streaming to {TargetWordCode}";
            }
            catch (Exception ex)
            {
                NotificationMessage = $"⚠️ Stream error: {ex.Message}";
                _soundService.PlayError();
            }
        }

        /// <summary>
        /// Stops the current streaming session.
        /// </summary>
        private async Task StopStreamAsync()
        {
            await _streamService.StopStreamAsync();
            IsStreaming = false;
            NotificationMessage = "📺 Streaming stopped";
        }

        /// <summary>
        /// Handles captured stream images, sending them to the target.
        /// </summary>
        private async void OnStreamImageCaptured(object? sender, StreamShareEventArgs e)
        {
            if (e.ImageData == null) return;

            var item = new TransferItem
            {
                Type = TransferType.StreamShare,
                Name = $"Stream_{e.ImageIndex}",
                FileName = $"stream_{e.StreamId}_{e.ImageIndex}.png",
                FileData = e.ImageData,
                FileSize = e.ImageData.Length,
                StreamId = e.StreamId,
                StreamSequence = e.ImageIndex
            };

            await _networkService.SendTransferAsync(
                e.TargetWordCode,
                item,
                _settings.MubbleEnabled,
                _settings.MubbleCode);
        }

        #endregion

        #region Item Action Methods

        /// <summary>
        /// Opens a received item based on its type.
        /// </summary>
        private void OpenItem(TransferItem? item)
        {
            if (item == null) return;

            try
            {
                switch (item.Type)
                {
                    case TransferType.Link:
                        if (!string.IsNullOrWhiteSpace(item.Content))
                        {
                            // Validate URL before opening
                            if (IsValidAndSafeUrl(item.Content))
                            {
                                Process.Start(new ProcessStartInfo(item.Content) { UseShellExecute = true });
                            }
                            else
                            {
                                NotificationMessage = "⚠️ Cannot open: URL did not pass security validation";
                                _soundService.PlayError();
                                return;
                            }
                        }
                        break;

                    case TransferType.File:
                    case TransferType.Image:
                    case TransferType.Screenshot:
                    case TransferType.StreamShare:
                        // Save to temp and open
                        if (item.FileData != null)
                        {
                            var tempPath = Path.Combine(Path.GetTempPath(), item.FileName ?? "bubblenet_file");
                            File.WriteAllBytes(tempPath, item.FileData);
                            Process.Start(new ProcessStartInfo(tempPath) { UseShellExecute = true });
                        }
                        break;

                    case TransferType.Text:
                        // Copy to clipboard
                        Clipboard.SetText(item.Content);
                        NotificationMessage = "📋 Text copied to clipboard";
                        break;
                }

                item.IsOpened = true;
                _soundService.PlayClick();
            }
            catch (Exception ex)
            {
                NotificationMessage = $"⚠️ Error opening: {ex.Message}";
                _soundService.PlayError();
            }
        }

        /// <summary>
        /// Downloads a file item to user-selected location.
        /// </summary>
        private void DownloadItem(TransferItem? item)
        {
            if (item == null || item.FileData == null) return;

            var dialog = new SaveFileDialog
            {
                FileName = item.FileName ?? "download",
                Filter = "All Files (*.*)|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    File.WriteAllBytes(dialog.FileName, item.FileData);
                    NotificationMessage = $"✅ Downloaded to {dialog.FileName}";
                    _soundService.PlayClick();
                }
                catch (Exception ex)
                {
                    NotificationMessage = $"⚠️ Download error: {ex.Message}";
                    _soundService.PlayError();
                }
            }
        }

        /// <summary>
        /// Copies item content to clipboard.
        /// </summary>
        private void CopyItem(TransferItem? item)
        {
            if (item == null) return;

            try
            {
                var textToCopy = item.Type switch
                {
                    TransferType.Text => item.Content,
                    TransferType.Link => item.Content,
                    _ => item.DisplayInfo
                };

                Clipboard.SetText(textToCopy);
                NotificationMessage = "📋 Copied to clipboard";
                _soundService.PlayClick();
            }
            catch (Exception ex)
            {
                NotificationMessage = $"⚠️ Copy error: {ex.Message}";
            }
        }

        /// <summary>
        /// Copies the local word code to clipboard.
        /// </summary>
        private void CopyWordCode()
        {
            if (!IsConnected) return;
            Clipboard.SetText(WordCode);
            NotificationMessage = $"📋 Word code copied: {WordCode}";
            _soundService.PlayClick();
        }

        /// <summary>
        /// Downloads all files from a FileBubble session.
        /// </summary>
        private void DownloadFileBubble(FileBubbleSession? session)
        {
            if (session == null) return;

            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Select folder to download FileBubble contents"
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var count = _fileBubbleService.DownloadSessionFiles(session.SessionId, dialog.SelectedPath);
                NotificationMessage = $"✅ Downloaded {count} files to {dialog.SelectedPath}";
                _soundService.PlayClick();
            }
        }

        #endregion

        #region Settings Methods

        /// <summary>
        /// Opens the settings window.
        /// </summary>
        private void OpenSettings()
        {
            _soundService.PlayClick();

            var settingsWindow = new SettingsWindow(_settings);
            settingsWindow.Owner = Application.Current.MainWindow;

            if (settingsWindow.ShowDialog() == true)
            {
                // Reload settings after save
                _settings = Settings.Load();
                _soundService.SoundEnabled = _settings.SoundEnabled;
                OnPropertyChanged(nameof(Settings));
                NotificationMessage = "✅ Settings saved";
            }
        }

        /// <summary>
        /// Selects a payload type (for the redesigned send menu).
        /// </summary>
        private void SelectPayloadType(TransferType type)
        {
            SelectedPayloadType = type;
            _soundService.PlayClick();
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Validates that a URL is safe to open (http/https only, proper format).
        /// </summary>
        private static bool IsValidAndSafeUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            // Must be http or https
            if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                return false;

            // Validate URL format using Uri.TryCreate
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                return false;

            // Only allow http and https schemes
            if (uri.Scheme != "http" && uri.Scheme != "https")
                return false;

            // Additional validation with regex pattern
            return SafeUrlPattern.IsMatch(url);
        }

        /// <summary>
        /// Converts a partial word code to full format using local IP for missing octets.
        /// </summary>
        private string ConvertToWordCode(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            var parsed = FoodWords.ParseWordCode(input.Trim());
            if (parsed.octet2 == -1 && parsed.octet3 == -1 && parsed.octet4 == -1)
                return string.Empty;

            try
            {
                var local = _networkService.GetIPOctets();

                int o2 = parsed.octet2 > 0 ? parsed.octet2 : local.octet2;
                int o3 = parsed.octet3 > 0 ? parsed.octet3 : local.octet3;
                int o4 = parsed.octet4 > 0 ? parsed.octet4 : local.octet4;

                // Clamp values to valid range
                o2 = Math.Clamp(o2, 1, 255);
                o3 = Math.Clamp(o3, 1, 255);
                o4 = Math.Clamp(o4, 1, 255);

                return FoodWords.GenerateWordCode((byte)o2, (byte)o3, (byte)o4);
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Raises PropertyChanged event for data binding.
        /// </summary>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Cleans up resources when disposing.
        /// </summary>
        public void Dispose()
        {
            // Stop streaming if active
            if (IsStreaming)
            {
                _streamService.StopStreamAsync().Wait();
            }

            // Save settings
            _settings.Save();

            // Dispose services
            _streamService.Dispose();
            _networkService.Dispose();
        }

        #endregion
    }

    #region Command Implementations

    /// <summary>
    /// Simple ICommand implementation for async and sync operations.
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Func<Task>? _executeAsync;
        private readonly Action? _execute;
        private readonly Func<bool>? _canExecute;

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public RelayCommand(Func<Task> executeAsync, Func<bool>? canExecute = null)
        {
            _executeAsync = executeAsync;
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

        public async void Execute(object? parameter)
        {
            if (_executeAsync != null)
                await _executeAsync();
            else
                _execute?.Invoke();
        }
    }

    /// <summary>
    /// Generic ICommand implementation for commands with parameters.
    /// </summary>
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T?> _execute;
        private readonly Func<T?, bool>? _canExecute;

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public RelayCommand(Action<T?> execute, Func<T?, bool>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke((T?)parameter) ?? true;
        public void Execute(object? parameter) => _execute((T?)parameter);
    }

    #endregion
}

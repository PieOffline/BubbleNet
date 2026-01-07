using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using BubbleNet.Models;
using BubbleNet.Services;
using Microsoft.Win32;

namespace BubbleNet.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly NetworkService _networkService;
        private readonly SoundService _soundService;
        
        // URL validation pattern - allows http, https, and common safe protocols
        private static readonly Regex SafeUrlPattern = new(
            @"^https?://[a-zA-Z0-9][-a-zA-Z0-9]*(\.[a-zA-Z0-9][-a-zA-Z0-9]*)+(:[0-9]+)?(/.*)?$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public event PropertyChangedEventHandler? PropertyChanged;

        #region Properties

        private bool _isConnected;
        public bool IsConnected
        {
            get => _isConnected;
            set { _isConnected = value; OnPropertyChanged(); OnPropertyChanged(nameof(ConnectionStatus)); }
        }

        public string ConnectionStatus => IsConnected ? "🟢 Connected" : "🔴 Disconnected";

        private string _wordCode = "---/---/---";
        public string WordCode
        {
            get => _wordCode;
            set { _wordCode = value; OnPropertyChanged(); }
        }

        private string _localIP = "Getting IP...";
        public string LocalIP
        {
            get => _localIP;
            set { _localIP = value; OnPropertyChanged(); }
        }

        private int _activePort;
        public int ActivePort
        {
            get => _activePort;
            set { _activePort = value; OnPropertyChanged(); OnPropertyChanged(nameof(PortDisplay)); }
        }

        public string PortDisplay => ActivePort > 0 ? $"Port: {ActivePort}" : "Port: ---";

        private string _notificationMessage = "Welcome to BubbleNet! Press 'Mesh Me' to start sharing.";
        public string NotificationMessage
        {
            get => _notificationMessage;
            set { _notificationMessage = value; OnPropertyChanged(); }
        }

        private bool _autoDeny;
        public bool AutoDeny
        {
            get => _autoDeny;
            set 
            { 
                _autoDeny = value; 
                OnPropertyChanged();
                _soundService.PlayToggle();
                NotificationMessage = value ? "Auto-deny is ON - All incoming transfers will be rejected" : "Auto-deny is OFF - Ready to receive transfers";
            }
        }

        private bool _autoOpen;
        public bool AutoOpen
        {
            get => _autoOpen;
            set 
            { 
                _autoOpen = value; 
                OnPropertyChanged();
                _soundService.PlayToggle();
                NotificationMessage = value ? "Auto-open is ON - Links will open automatically in browser" : "Auto-open is OFF";
            }
        }

        private string _targetWordCode = "";
        public string TargetWordCode
        {
            get => _targetWordCode;
            set { _targetWordCode = value; OnPropertyChanged(); }
        }

        private string _textToSend = "";
        public string TextToSend
        {
            get => _textToSend;
            set { _textToSend = value; OnPropertyChanged(); }
        }

        private string _linkToSend = "";
        public string LinkToSend
        {
            get => _linkToSend;
            set { _linkToSend = value; OnPropertyChanged(); }
        }

        private TransferItem? _selectedReceivedItem;
        public TransferItem? SelectedReceivedItem
        {
            get => _selectedReceivedItem;
            set { _selectedReceivedItem = value; OnPropertyChanged(); }
        }

        public ObservableCollection<TransferItem> ReceivedItems { get; } = new();

        #endregion

        #region Commands

        public ICommand MeshMeCommand { get; }
        public ICommand DisconnectCommand { get; }
        public ICommand SendFileCommand { get; }
        public ICommand SendTextCommand { get; }
        public ICommand SendLinkCommand { get; }
        public ICommand SendScreenshotCommand { get; }
        public ICommand OpenItemCommand { get; }
        public ICommand DownloadItemCommand { get; }
        public ICommand CopyItemCommand { get; }
        public ICommand CopyWordCodeCommand { get; }

        #endregion

        public MainViewModel()
        {
            _networkService = new NetworkService();
            _soundService = SoundService.Instance;

            _networkService.TransferReceived += OnTransferReceived;
            _networkService.StatusChanged += (s, msg) => NotificationMessage = msg;
            _networkService.ErrorOccurred += (s, err) =>
            {
                NotificationMessage = $"⚠️ {err}";
                _soundService.PlayError();
            };

            MeshMeCommand = new RelayCommand(async () => await ConnectAsync(), () => !IsConnected);
            DisconnectCommand = new RelayCommand(Disconnect, () => IsConnected);
            SendFileCommand = new RelayCommand(async () => await SendFileAsync(), () => IsConnected && !string.IsNullOrWhiteSpace(TargetWordCode));
            SendTextCommand = new RelayCommand(async () => await SendTextAsync(), () => IsConnected && !string.IsNullOrWhiteSpace(TargetWordCode) && !string.IsNullOrWhiteSpace(TextToSend));
            SendLinkCommand = new RelayCommand(async () => await SendLinkAsync(), () => IsConnected && !string.IsNullOrWhiteSpace(TargetWordCode) && !string.IsNullOrWhiteSpace(LinkToSend));
            SendScreenshotCommand = new RelayCommand(async () => await SendScreenshotAsync(), () => IsConnected && !string.IsNullOrWhiteSpace(TargetWordCode));
            OpenItemCommand = new RelayCommand<TransferItem>(OpenItem);
            DownloadItemCommand = new RelayCommand<TransferItem>(DownloadItem);
            CopyItemCommand = new RelayCommand<TransferItem>(CopyItem);
            CopyWordCodeCommand = new RelayCommand(CopyWordCode, () => IsConnected);
        }

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
                EndOfWordCode = WordCode?
                    .Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries)
                    .LastOrDefault()?
                    .Trim() ?? string.Empty;

                NotificationMessage = $"✅ Connected! Share your word code: {EndOfWordCode}";
                _soundService.PlayConnect();
            }
        }

        private void Disconnect()
        {
            _soundService.PlayClick();
            _networkService.StopListening();
            IsConnected = false;
            WordCode = "---/---/---";
            ActivePort = 0;
            NotificationMessage = "Disconnected from mesh network.";
            _soundService.PlayDisconnect();
        }

        private void OnTransferReceived(object? sender, TransferItem item)
        {
            if (AutoDeny)
            {
                NotificationMessage = $"⛔ Auto-denied transfer from {item.SenderWordCode}";
                return;
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                ReceivedItems.Insert(0, item);
                NotificationMessage = $"📥 Received {item.Type} from {item.SenderWordCode}";
                _soundService.PlayReceive();

                // Auto-open links if enabled - only for validated URLs
                if (AutoOpen && item.Type == TransferType.Link && !string.IsNullOrWhiteSpace(item.Content))
                {
                    if (IsValidAndSafeUrl(item.Content))
                    {
                        try
                        {
                            Process.Start(new ProcessStartInfo(item.Content) { UseShellExecute = true });
                        }
                        catch { }
                    }
                    else
                    {
                        NotificationMessage = $"⚠️ Auto-open skipped: URL did not pass validation";
                    }
                }
            });
        }

        /// <summary>
        /// Validates that a URL is safe to open (http/https only, proper format)
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

                    NotificationMessage = $"📤 Sending {item.FileName}...";
                    var success = await _networkService.SendTransferAsync(TargetWordCode, item);
                    if (success)
                    {
                        NotificationMessage = $"✅ Sent {item.FileName} to {TargetWordCode}";
                        _soundService.PlaySend();
                    }
                }
                catch (Exception ex)
                {
                    NotificationMessage = $"⚠️ Error: {ex.Message}";
                    _soundService.PlayError();
                }
            }
        }

        private async Task SendTextAsync()
        {
            var item = new TransferItem
            {
                Type = TransferType.Text,
                Name = "Text Message",
                Content = TextToSend
            };

            NotificationMessage = $"📤 Sending text...";
            var success = await _networkService.SendTransferAsync(TargetWordCode, item);
            if (success)
            {
                NotificationMessage = $"✅ Sent text to {TargetWordCode}";
                TextToSend = "";
                _soundService.PlaySend();
            }
        }

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

            NotificationMessage = $"📤 Sending link...";
            var success = await _networkService.SendTransferAsync(TargetWordCode, item);
            if (success)
            {
                NotificationMessage = $"✅ Sent link to {TargetWordCode}";
                LinkToSend = "";
                _soundService.PlaySend();
            }
        }

        private async Task SendScreenshotAsync()
        {
            try
            {
                // Minimize window, take screenshot, restore
                var mainWindow = Application.Current.MainWindow;
                var previousState = mainWindow.WindowState;
                mainWindow.WindowState = WindowState.Minimized;
                await Task.Delay(300);

                // Capture screen
                var screenWidth = (int)SystemParameters.PrimaryScreenWidth;
                var screenHeight = (int)SystemParameters.PrimaryScreenHeight;
                
                using var bitmap = new System.Drawing.Bitmap(screenWidth, screenHeight);
                using var graphics = System.Drawing.Graphics.FromImage(bitmap);
                graphics.CopyFromScreen(0, 0, 0, 0, new System.Drawing.Size(screenWidth, screenHeight));
                
                using var ms = new MemoryStream();
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                var screenshotData = ms.ToArray();

                mainWindow.WindowState = previousState;
                mainWindow.Activate();

                var item = new TransferItem
                {
                    Type = TransferType.Screenshot,
                    Name = $"Screenshot_{DateTime.Now:yyyyMMdd_HHmmss}",
                    FileName = $"Screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png",
                    FileData = screenshotData,
                    FileSize = screenshotData.Length
                };

                NotificationMessage = $"📤 Sending screenshot...";
                var success = await _networkService.SendTransferAsync(TargetWordCode, item);
                if (success)
                {
                    NotificationMessage = $"✅ Sent screenshot to {TargetWordCode}";
                    _soundService.PlaySend();
                }
            }
            catch (Exception ex)
            {
                NotificationMessage = $"⚠️ Screenshot error: {ex.Message}";
                _soundService.PlayError();
            }
        }

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

        private void CopyWordCode()
        {
            if (!IsConnected) return;
            Clipboard.SetText(WordCode);
            NotificationMessage = $"📋 Word code copied: {WordCode}";
            _soundService.PlayClick();
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            _networkService.Dispose();
        }
    }

    // Simple RelayCommand implementation
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
}

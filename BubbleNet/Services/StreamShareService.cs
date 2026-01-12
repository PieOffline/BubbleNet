using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace BubbleNet.Services
{
    /// <summary>
    /// StreamShare service - enables continuous screen capture streaming to other BubbleNet users.
    /// Captures screenshots at configurable intervals and sends them as a stream.
    /// </summary>
    public class StreamShareService : INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<StreamShareEventArgs>? StreamImageCaptured;
        public event EventHandler<StreamShareEventArgs>? StreamStarted;
        public event EventHandler<StreamShareEventArgs>? StreamStopped;

        // ===== Active Streams =====
        private readonly ConcurrentDictionary<string, StreamSession> _activeSessions = new();
        private CancellationTokenSource? _streamingCts;

        // ===== Current Session Properties =====
        private string? _currentStreamId;
        public string? CurrentStreamId => _currentStreamId;

        private bool _isStreaming;
        /// <summary>Whether this client is currently streaming</summary>
        public bool IsStreaming
        {
            get => _isStreaming;
            private set { _isStreaming = value; OnPropertyChanged(); }
        }

        private int _imagesSent;
        /// <summary>Number of images sent in the current stream session</summary>
        public int ImagesSent
        {
            get => _imagesSent;
            private set { _imagesSent = value; OnPropertyChanged(); }
        }

        private TimeSpan _streamDuration;
        /// <summary>Duration of the current streaming session</summary>
        public TimeSpan StreamDuration
        {
            get => _streamDuration;
            private set { _streamDuration = value; OnPropertyChanged(); }
        }

        private string _streamTarget = "";
        /// <summary>Target word code(s) for the current stream</summary>
        public string StreamTarget
        {
            get => _streamTarget;
            private set { _streamTarget = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Starts a new stream share session.
        /// </summary>
        /// <param name="targetWordCode">The target recipient's word code</param>
        /// <param name="intervalSeconds">Capture interval in seconds (0.25 to 10)</param>
        /// <returns>The unique stream session ID</returns>
        public async Task<string> StartStreamAsync(string targetWordCode, double intervalSeconds = 1.0)
        {
            // Stop any existing stream
            await StopStreamAsync();

            // Generate unique stream ID using full GUID for better uniqueness
            // Using 16 characters (half of GUID) provides ~2^64 combinations
            _currentStreamId = Guid.NewGuid().ToString("N")[..16].ToUpper();
            _streamingCts = new CancellationTokenSource();

            IsStreaming = true;
            ImagesSent = 0;
            StreamTarget = targetWordCode;

            var startTime = DateTime.Now;

            // Create session tracking
            var session = new StreamSession
            {
                StreamId = _currentStreamId,
                StartTime = startTime,
                TargetWordCode = targetWordCode,
                IntervalSeconds = intervalSeconds
            };
            _activeSessions[_currentStreamId] = session;

            // Notify stream started
            StreamStarted?.Invoke(this, new StreamShareEventArgs
            {
                StreamId = _currentStreamId,
                TargetWordCode = targetWordCode,
                ImageIndex = 0
            });

            // Start the streaming loop
            var ct = _streamingCts.Token;
            _ = Task.Run(async () =>
            {
                try
                {
                    while (!ct.IsCancellationRequested)
                    {
                        // Capture screenshot
                        var imageData = CaptureScreen();
                        if (imageData != null)
                        {
                            ImagesSent++;
                            StreamDuration = DateTime.Now - startTime;

                            // Raise event for sending
                            StreamImageCaptured?.Invoke(this, new StreamShareEventArgs
                            {
                                StreamId = _currentStreamId,
                                TargetWordCode = targetWordCode,
                                ImageData = imageData,
                                ImageIndex = ImagesSent
                            });
                        }

                        // Wait for interval
                        await Task.Delay(TimeSpan.FromSeconds(intervalSeconds), ct);
                    }
                }
                catch (OperationCanceledException)
                {
                    // Normal cancellation
                }
                catch (Exception)
                {
                    // Handle streaming errors
                }
            }, ct);

            return _currentStreamId;
        }

        /// <summary>
        /// Stops the current streaming session.
        /// </summary>
        public Task StopStreamAsync()
        {
            if (_streamingCts != null)
            {
                _streamingCts.Cancel();
                _streamingCts.Dispose();
                _streamingCts = null;
            }

            if (_currentStreamId != null)
            {
                _activeSessions.TryRemove(_currentStreamId, out _);

                // Notify stream stopped
                StreamStopped?.Invoke(this, new StreamShareEventArgs
                {
                    StreamId = _currentStreamId,
                    TargetWordCode = StreamTarget,
                    ImageIndex = ImagesSent
                });

                _currentStreamId = null;
            }

            IsStreaming = false;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Captures the current screen as PNG data.
        /// </summary>
        /// <returns>PNG image data as byte array</returns>
        private byte[]? CaptureScreen()
        {
            try
            {
                // Get screen dimensions
                var screenWidth = (int)SystemParameters.PrimaryScreenWidth;
                var screenHeight = (int)SystemParameters.PrimaryScreenHeight;

                // Capture screen using System.Drawing
                using var bitmap = new Bitmap(screenWidth, screenHeight);
                using var graphics = Graphics.FromImage(bitmap);
                graphics.CopyFromScreen(0, 0, 0, 0, new System.Drawing.Size(screenWidth, screenHeight));

                // Convert to PNG byte array
                using var ms = new MemoryStream();
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                return ms.ToArray();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Registers a received stream session for tracking on the receiver side.
        /// </summary>
        public void RegisterReceivedStream(string streamId, string senderWordCode)
        {
            if (!_activeSessions.ContainsKey(streamId))
            {
                _activeSessions[streamId] = new StreamSession
                {
                    StreamId = streamId,
                    StartTime = DateTime.Now,
                    TargetWordCode = senderWordCode,
                    IsReceiving = true
                };
            }
        }

        /// <summary>
        /// Unregisters a stream session (stopped by sender or receiver).
        /// </summary>
        public void UnregisterStream(string streamId)
        {
            _activeSessions.TryRemove(streamId, out _);
        }

        /// <summary>
        /// Gets information about an active stream session.
        /// </summary>
        public StreamSession? GetSession(string streamId)
        {
            _activeSessions.TryGetValue(streamId, out var session);
            return session;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            StopStreamAsync().Wait();
            _streamingCts?.Dispose();
        }
    }

    /// <summary>
    /// Event arguments for StreamShare events.
    /// </summary>
    public class StreamShareEventArgs : EventArgs
    {
        public string StreamId { get; set; } = "";
        public string TargetWordCode { get; set; } = "";
        public byte[]? ImageData { get; set; }
        public int ImageIndex { get; set; }
    }

    /// <summary>
    /// Represents an active stream session (sending or receiving).
    /// </summary>
    public class StreamSession
    {
        public string StreamId { get; set; } = "";
        public DateTime StartTime { get; set; }
        public string TargetWordCode { get; set; } = "";
        public double IntervalSeconds { get; set; } = 1.0;
        public bool IsReceiving { get; set; }
        public int ImageCount { get; set; }

        public TimeSpan Duration => DateTime.Now - StartTime;
    }
}

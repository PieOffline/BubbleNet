using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using BubbleNet.Models;

namespace BubbleNet.Services
{
    /// <summary>
    /// FileBubble service - automatic file collection from a sender.
    /// When enabled, files received from a sender are automatically grouped
    /// into a "bubble" that can be downloaded as a folder.
    /// </summary>
    public class FileBubbleService : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<FileBubbleEventArgs>? FileAdded;
        public event EventHandler<FileBubbleEventArgs>? SessionCreated;
        public event EventHandler<FileBubbleEventArgs>? SessionClosed;

        // ===== Active FileBubble Sessions =====
        // Key: FileBubbleId, Value: Session containing all collected files
        private readonly ConcurrentDictionary<string, FileBubbleSession> _sessions = new();

        // ===== Current Session (for UI binding) =====
        private string? _activeSessionId;
        public string? ActiveSessionId => _activeSessionId;

        private bool _isActive;
        /// <summary>Whether FileBubble collection is currently active</summary>
        public bool IsActive
        {
            get => _isActive;
            set { _isActive = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Gets all active FileBubble sessions.
        /// </summary>
        public IReadOnlyDictionary<string, FileBubbleSession> Sessions => _sessions;

        /// <summary>
        /// Creates a new FileBubble session for collecting files.
        /// </summary>
        /// <param name="senderWordCode">The word code of the sender</param>
        /// <returns>The unique FileBubble session ID</returns>
        public string CreateSession(string senderWordCode)
        {
            // Generate unique session ID
            var sessionId = Guid.NewGuid().ToString("N")[..8].ToUpper();

            var session = new FileBubbleSession
            {
                SessionId = sessionId,
                SenderWordCode = senderWordCode,
                StartTime = DateTime.Now
            };

            _sessions[sessionId] = session;
            _activeSessionId = sessionId;
            IsActive = true;

            SessionCreated?.Invoke(this, new FileBubbleEventArgs
            {
                SessionId = sessionId,
                SenderWordCode = senderWordCode
            });

            return sessionId;
        }

        /// <summary>
        /// Adds a file to a FileBubble session.
        /// </summary>
        /// <param name="sessionId">The session ID</param>
        /// <param name="item">The transfer item containing the file</param>
        public void AddFile(string sessionId, TransferItem item)
        {
            if (_sessions.TryGetValue(sessionId, out var session))
            {
                session.Files.Add(item);

                FileAdded?.Invoke(this, new FileBubbleEventArgs
                {
                    SessionId = sessionId,
                    SenderWordCode = session.SenderWordCode,
                    FileName = item.FileName,
                    FileCount = session.Files.Count
                });
            }
        }

        /// <summary>
        /// Adds a file to the current active session, or creates a new session if needed.
        /// </summary>
        /// <param name="senderWordCode">The sender's word code</param>
        /// <param name="item">The transfer item containing the file</param>
        /// <returns>The session ID the file was added to</returns>
        public string AddFileAutoSession(string senderWordCode, TransferItem item)
        {
            // Find an existing session from this sender, or create new
            var existingSession = _sessions.Values
                .FirstOrDefault(s => s.SenderWordCode == senderWordCode && !s.IsClosed);

            string sessionId;
            if (existingSession != null)
            {
                sessionId = existingSession.SessionId;
            }
            else
            {
                sessionId = CreateSession(senderWordCode);
            }

            AddFile(sessionId, item);
            return sessionId;
        }

        /// <summary>
        /// Gets a FileBubble session by ID.
        /// </summary>
        public FileBubbleSession? GetSession(string sessionId)
        {
            _sessions.TryGetValue(sessionId, out var session);
            return session;
        }

        /// <summary>
        /// Closes a FileBubble session (stops collecting files).
        /// </summary>
        public void CloseSession(string sessionId)
        {
            if (_sessions.TryGetValue(sessionId, out var session))
            {
                session.IsClosed = true;
                session.EndTime = DateTime.Now;

                SessionClosed?.Invoke(this, new FileBubbleEventArgs
                {
                    SessionId = sessionId,
                    SenderWordCode = session.SenderWordCode,
                    FileCount = session.Files.Count
                });

                if (_activeSessionId == sessionId)
                {
                    _activeSessionId = null;
                    IsActive = _sessions.Values.Any(s => !s.IsClosed);
                }
            }
        }

        /// <summary>
        /// Downloads all files from a session to a specified directory.
        /// </summary>
        /// <param name="sessionId">The session ID</param>
        /// <param name="targetDirectory">The directory to save files to</param>
        /// <returns>Number of files saved</returns>
        public int DownloadSessionFiles(string sessionId, string targetDirectory)
        {
            if (!_sessions.TryGetValue(sessionId, out var session))
                return 0;

            // Create directory if needed
            if (!Directory.Exists(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
            }

            int savedCount = 0;
            var errors = new System.Collections.Generic.List<string>();

            foreach (var file in session.Files)
            {
                if (file.FileData != null && !string.IsNullOrEmpty(file.FileName))
                {
                    var filePath = Path.Combine(targetDirectory, file.FileName);

                    // Handle duplicate filenames
                    int counter = 1;
                    var originalPath = filePath;
                    while (File.Exists(filePath))
                    {
                        var ext = Path.GetExtension(originalPath);
                        var nameWithoutExt = Path.GetFileNameWithoutExtension(originalPath);
                        filePath = Path.Combine(targetDirectory, $"{nameWithoutExt}_{counter++}{ext}");
                    }

                    try
                    {
                        File.WriteAllBytes(filePath, file.FileData);
                        savedCount++;
                    }
                    catch (Exception ex)
                    {
                        // Track failed files but continue with others
                        errors.Add($"{file.FileName}: {ex.Message}");
                    }
                }
            }

            // If there were errors, throw an aggregate exception with details
            if (errors.Count > 0 && savedCount < session.Files.Count)
            {
                throw new IOException($"Failed to save {errors.Count} file(s): {string.Join("; ", errors)}");
            }

            return savedCount;
        }

        /// <summary>
        /// Downloads a single file from a session.
        /// </summary>
        public bool DownloadFile(string sessionId, Guid fileId, string targetPath)
        {
            if (!_sessions.TryGetValue(sessionId, out var session))
                return false;

            var file = session.Files.FirstOrDefault(f => f.Id == fileId);
            if (file?.FileData == null)
                return false;

            File.WriteAllBytes(targetPath, file.FileData);
            return true;
        }

        /// <summary>
        /// Removes a session completely.
        /// </summary>
        public void RemoveSession(string sessionId)
        {
            _sessions.TryRemove(sessionId, out _);
            if (_activeSessionId == sessionId)
            {
                _activeSessionId = null;
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// Event arguments for FileBubble events.
    /// </summary>
    public class FileBubbleEventArgs : EventArgs
    {
        public string SessionId { get; set; } = "";
        public string SenderWordCode { get; set; } = "";
        public string? FileName { get; set; }
        public int FileCount { get; set; }
    }

    /// <summary>
    /// Represents a FileBubble session containing collected files.
    /// </summary>
    public class FileBubbleSession : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public string SessionId { get; set; } = "";
        public string SenderWordCode { get; set; } = "";
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public bool IsClosed { get; set; }

        /// <summary>Collection of files received in this session</summary>
        public ObservableCollection<TransferItem> Files { get; } = new();

        /// <summary>Total size of all files in bytes</summary>
        public long TotalSize => Files.Sum(f => f.FileSize);

        /// <summary>Formatted total size string</summary>
        public string TotalSizeDisplay
        {
            get
            {
                var bytes = TotalSize;
                string[] sizes = { "B", "KB", "MB", "GB" };
                int order = 0;
                double size = bytes;
                while (size >= 1024 && order < sizes.Length - 1)
                {
                    order++;
                    size /= 1024;
                }
                return $"{size:0.##} {sizes[order]}";
            }
        }

        /// <summary>Duration of the session</summary>
        public TimeSpan Duration => (EndTime ?? DateTime.Now) - StartTime;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

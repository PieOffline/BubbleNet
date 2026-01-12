using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BubbleNet.Models
{
    /// <summary>
    /// Defines the types of transfers supported by BubbleNet.
    /// Each type determines how the payload is processed, displayed, and handled.
    /// </summary>
    public enum TransferType
    {
        /// <summary>Standard file transfer - any file type</summary>
        File,
        /// <summary>Plain text message transfer</summary>
        Text,
        /// <summary>URL/hyperlink transfer - can auto-open in browser</summary>
        Link,
        /// <summary>Screen capture image transfer</summary>
        Screenshot,
        /// <summary>Image file transfer (distinct from screenshot for display purposes)</summary>
        Image,
        /// <summary>Stream share - continuous screen capture stream</summary>
        StreamShare,
        /// <summary>FileBubble - automatic file collection session</summary>
        FileBubble
    }

    /// <summary>
    /// Represents a single transfer item (sent or received) in BubbleNet.
    /// Contains all metadata and payload data for the transfer.
    /// Implements INotifyPropertyChanged for UI binding updates.
    /// </summary>
    public class TransferItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>Unique identifier for this transfer item</summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>The type of transfer (File, Text, Link, Screenshot, etc.)</summary>
        public TransferType Type { get; set; }

        /// <summary>Display name for the transfer item</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Text content, link URL, or other string-based payload data</summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>Binary file data for file-based transfers</summary>
        public byte[]? FileData { get; set; }

        /// <summary>Original filename for file-based transfers</summary>
        public string? FileName { get; set; }

        /// <summary>Size of the file in bytes</summary>
        public long FileSize { get; set; }

        /// <summary>Timestamp when the transfer was received</summary>
        public DateTime ReceivedAt { get; set; } = DateTime.Now;

        /// <summary>Food word code of the sender (e.g., "Apple/Banana/Cherry")</summary>
        public string SenderWordCode { get; set; } = string.Empty;

        /// <summary>IP address of the sender</summary>
        public string SenderIP { get; set; } = string.Empty;

        /// <summary>Whether the item has been opened/viewed by the user</summary>
        public bool IsOpened { get; set; } = false;

        // ===== Mubble (encryption) properties =====
        /// <summary>Indicates if this payload is Mubble-encrypted</summary>
        public bool IsMubbleEncrypted { get; set; } = false;

        /// <summary>The Mubble code used for encryption (sent in plaintext for receiver matching)</summary>
        public string MubbleCode { get; set; } = string.Empty;

        // ===== StreamShare properties =====
        /// <summary>Unique stream session ID for StreamShare transfers</summary>
        public string? StreamId { get; set; }

        /// <summary>For StreamShare: sequence number of this image in the stream</summary>
        public int StreamSequence { get; set; }

        /// <summary>Whether this is an active stream (for receiver display)</summary>
        private bool _isStreamActive;
        public bool IsStreamActive
        {
            get => _isStreamActive;
            set { _isStreamActive = value; OnPropertyChanged(); }
        }

        // ===== FileBubble properties =====
        /// <summary>Unique FileBubble session ID</summary>
        public string? FileBubbleId { get; set; }

        /// <summary>Number of files collected in this FileBubble session</summary>
        public int FileBubbleFileCount { get; set; }

        // ===== DoubleBubble properties =====
        /// <summary>List of target word codes for DoubleBubble (multi-recipient) sends</summary>
        public string[]? DoubleBubbleTargets { get; set; }

        /// <summary>
        /// Returns the appropriate emoji icon for this transfer type.
        /// Used in the UI to quickly identify the transfer type.
        /// </summary>
        public string TypeIcon => Type switch
        {
            TransferType.File => "📁",           // File folder icon
            TransferType.Text => "📝",           // Text/note icon
            TransferType.Link => "🔗",           // Link icon
            TransferType.Screenshot => "📸",     // Camera icon
            TransferType.Image => "🖼️",          // Framed picture icon
            TransferType.StreamShare => "📺",    // TV/stream icon
            TransferType.FileBubble => "📦",     // Package/collection icon
            _ => "📦"                            // Default package icon
        };

        /// <summary>
        /// Returns a user-friendly display string for the transfer item.
        /// Truncates long content and formats file sizes appropriately.
        /// </summary>
        public string DisplayInfo => Type switch
        {
            TransferType.File => $"{FileName} ({FormatFileSize(FileSize)})",
            TransferType.Text => Content.Length > 50 ? Content.Substring(0, 50) + "..." : Content,
            TransferType.Link => Content,
            TransferType.Screenshot => "Screenshot",
            TransferType.Image => FileName ?? "Image",
            TransferType.StreamShare => $"Stream ({StreamSequence} images)",
            TransferType.FileBubble => $"FileBubble ({FileBubbleFileCount} files)",
            _ => "Unknown"
        };

        /// <summary>Time portion of when the transfer was received (HH:mm:ss format)</summary>
        public string TimeDisplay => ReceivedAt.ToString("HH:mm:ss");

        /// <summary>Date portion of when the transfer was received (MMM dd, yyyy format)</summary>
        public string DateDisplay => ReceivedAt.ToString("MMM dd, yyyy");

        /// <summary>
        /// Formats a byte count into a human-readable string with appropriate units.
        /// Automatically scales from B to KB, MB, GB, TB as needed.
        /// </summary>
        /// <param name="bytes">The number of bytes to format</param>
        /// <returns>Formatted string like "1.5 MB" or "256 KB"</returns>
        private static string FormatFileSize(long bytes)
        {
            // Define size unit labels in ascending order
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            double size = bytes;

            // Keep dividing by 1024 until we reach an appropriate unit
            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size /= 1024;
            }

            // Return formatted string with up to 2 decimal places
            return $"{size:0.##} {sizes[order]}";
        }

        /// <summary>
        /// Raises the PropertyChanged event for UI binding updates.
        /// </summary>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

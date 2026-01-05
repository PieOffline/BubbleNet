using System;

namespace BubbleNet.Models
{
    public enum TransferType
    {
        File,
        Text,
        Link,
        Screenshot,
        Image
    }

    public class TransferItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public TransferType Type { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;  // Text content, link URL, or base64 data
        public byte[]? FileData { get; set; }
        public string? FileName { get; set; }
        public long FileSize { get; set; }
        public DateTime ReceivedAt { get; set; } = DateTime.Now;
        public string SenderWordCode { get; set; } = string.Empty;
        public string SenderIP { get; set; } = string.Empty;
        public bool IsOpened { get; set; } = false;

        public string TypeIcon => Type switch
        {
            TransferType.File => "📁",
            TransferType.Text => "📝",
            TransferType.Link => "🔗",
            TransferType.Screenshot => "📸",
            TransferType.Image => "🖼️",
            _ => "📦"
        };

        public string DisplayInfo => Type switch
        {
            TransferType.File => $"{FileName} ({FormatFileSize(FileSize)})",
            TransferType.Text => Content.Length > 50 ? Content.Substring(0, 50) + "..." : Content,
            TransferType.Link => Content,
            TransferType.Screenshot => "Screenshot",
            TransferType.Image => FileName ?? "Image",
            _ => "Unknown"
        };

        public string TimeDisplay => ReceivedAt.ToString("HH:mm:ss");
        public string DateDisplay => ReceivedAt.ToString("MMM dd, yyyy");

        private static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
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
}

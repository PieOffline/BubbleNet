using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace BubbleNet.Models
{
    /// <summary>
    /// Application settings model that persists user preferences.
    /// Includes Mubble encryption settings, StreamShare settings, and other configuration options.
    /// </summary>
    public class Settings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        // ===== File Paths =====
        /// <summary>Path to the settings file in user's AppData folder</summary>
        private static readonly string SettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "BubbleNet",
            "settings.json");

        /// <summary>Path to save screenshots</summary>
        private static readonly string DefaultScreenshotPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
            "BubbleNet Screenshots");

        // ===== Mubble Settings =====
        // Mubble = "Muddled Bubble" - provides encryption/scrambling of payloads

        private bool _mubbleEnabled = false;
        /// <summary>Whether Mubble encryption is enabled for outgoing payloads</summary>
        public bool MubbleEnabled
        {
            get => _mubbleEnabled;
            set { _mubbleEnabled = value; OnPropertyChanged(); }
        }

        private string _mubbleCode = "";
        /// <summary>
        /// The Mubble encryption/decryption code.
        /// This code is sent in plaintext with encrypted payloads for matching on the receiver side.
        /// </summary>
        public string MubbleCode
        {
            get => _mubbleCode;
            set { _mubbleCode = value; OnPropertyChanged(); }
        }

        // ===== StreamShare Settings =====

        private double _streamShareInterval = 1.0;
        /// <summary>
        /// Interval between stream share captures in seconds.
        /// Valid range: 0.25 to 10 seconds.
        /// </summary>
        public double StreamShareInterval
        {
            get => _streamShareInterval;
            set
            {
                // Clamp value to valid range
                _streamShareInterval = Math.Max(0.25, Math.Min(10.0, value));
                OnPropertyChanged();
            }
        }

        // ===== FileBubble Settings =====

        private bool _fileBubbleEnabled = false;
        /// <summary>Whether FileBubble auto-collection is enabled</summary>
        public bool FileBubbleEnabled
        {
            get => _fileBubbleEnabled;
            set { _fileBubbleEnabled = value; OnPropertyChanged(); }
        }

        private string _fileBubbleDownloadPath = "";
        /// <summary>Default download path for FileBubble collections</summary>
        public string FileBubbleDownloadPath
        {
            get => string.IsNullOrEmpty(_fileBubbleDownloadPath)
                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "BubbleNet Downloads")
                : _fileBubbleDownloadPath;
            set { _fileBubbleDownloadPath = value; OnPropertyChanged(); }
        }

        // ===== Screenshot Settings =====

        private string _screenshotSavePath = "";
        /// <summary>Path where screenshots are saved directly as PNG files</summary>
        public string ScreenshotSavePath
        {
            get => string.IsNullOrEmpty(_screenshotSavePath) ? DefaultScreenshotPath : _screenshotSavePath;
            set { _screenshotSavePath = value; OnPropertyChanged(); }
        }

        private bool _saveScreenshotsLocally = true;
        /// <summary>Whether to save screenshots locally before sending</summary>
        public bool SaveScreenshotsLocally
        {
            get => _saveScreenshotsLocally;
            set { _saveScreenshotsLocally = value; OnPropertyChanged(); }
        }

        // ===== Sound Settings =====

        private bool _soundEnabled = true;
        /// <summary>Whether sound effects are enabled</summary>
        public bool SoundEnabled
        {
            get => _soundEnabled;
            set { _soundEnabled = value; OnPropertyChanged(); }
        }

        // ===== DoubleBubble Settings =====

        private string _doubleBubbleTargets = "";
        /// <summary>
        /// Comma-separated list of saved DoubleBubble targets.
        /// Format: "WordCode1,WordCode2,WordCode3"
        /// </summary>
        public string DoubleBubbleTargets
        {
            get => _doubleBubbleTargets;
            set { _doubleBubbleTargets = value; OnPropertyChanged(); }
        }

        // ===== General UI Settings =====

        private bool _autoOpen = true;
        /// <summary>Whether to auto-open received links in browser</summary>
        public bool AutoOpen
        {
            get => _autoOpen;
            set { _autoOpen = value; OnPropertyChanged(); }
        }

        private bool _autoDeny = false;
        /// <summary>Whether to auto-deny all incoming transfers</summary>
        public bool AutoDeny
        {
            get => _autoDeny;
            set { _autoDeny = value; OnPropertyChanged(); }
        }

        // ===== Persistence Methods =====

        /// <summary>
        /// Saves the current settings to the settings file.
        /// Creates the directory if it doesn't exist.
        /// </summary>
        public void Save()
        {
            try
            {
                // Ensure directory exists
                var directory = Path.GetDirectoryName(SettingsPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Serialize and save settings
                var json = JsonSerializer.Serialize(this, new JsonSerializerOptions
                {
                    WriteIndented = true  // Make the file human-readable
                });
                File.WriteAllText(SettingsPath, json);
            }
            catch (Exception)
            {
                // Silently fail - settings are not critical
            }
        }

        /// <summary>
        /// Loads settings from the settings file.
        /// Returns default settings if file doesn't exist or can't be read.
        /// </summary>
        /// <returns>Settings instance with loaded or default values</returns>
        public static Settings Load()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    var json = File.ReadAllText(SettingsPath);
                    var settings = JsonSerializer.Deserialize<Settings>(json);
                    return settings ?? new Settings();
                }
            }
            catch (Exception)
            {
                // Return default settings on any error
            }
            return new Settings();
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

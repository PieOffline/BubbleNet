using System.Windows;
using BubbleNet.Models;
using Microsoft.Win32;

namespace BubbleNet.Views
{
    /// <summary>
    /// Settings window for configuring BubbleNet preferences.
    /// Includes Mubble encryption, StreamShare, FileBubble, and other settings.
    /// </summary>
    public partial class SettingsWindow : Window
    {
        // Local copy of settings for editing
        private readonly Settings _settings;

        // Result indicating if settings were saved
        public bool SettingsSaved { get; private set; }

        /// <summary>
        /// Creates a new settings window with the provided settings.
        /// </summary>
        /// <param name="settings">The settings object to edit</param>
        public SettingsWindow(Settings settings)
        {
            InitializeComponent();

            // Create a working copy to allow cancel
            _settings = settings;
            DataContext = _settings;
        }

        /// <summary>
        /// Opens the Mubble configuration page/dialog.
        /// </summary>
        private void OpenMubble_Click(object sender, RoutedEventArgs e)
        {
            // Show Mubble information dialog
            var mubbleWindow = new MubbleWindow(_settings);
            mubbleWindow.Owner = this;
            mubbleWindow.ShowDialog();
        }

        /// <summary>
        /// Opens folder browser for FileBubble download path.
        /// </summary>
        private void BrowseFileBubblePath_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Select FileBubble Download Folder",
                ShowNewFolderButton = true
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _settings.FileBubbleDownloadPath = dialog.SelectedPath;
            }
        }

        /// <summary>
        /// Opens folder browser for screenshot save path.
        /// </summary>
        private void BrowseScreenshotPath_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Select Screenshot Save Folder",
                ShowNewFolderButton = true
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _settings.ScreenshotSavePath = dialog.SelectedPath;
            }
        }

        /// <summary>
        /// Saves settings and closes the window.
        /// </summary>
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // Save settings to file
            _settings.Save();
            SettingsSaved = true;
            DialogResult = true;
            Close();
        }

        /// <summary>
        /// Cancels changes and closes the window.
        /// </summary>
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}

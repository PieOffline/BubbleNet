using System.Windows;
using BubbleNet.Models;

namespace BubbleNet.Views
{
    /// <summary>
    /// Mubble configuration window.
    /// Provides detailed explanation and configuration of Mubble encryption.
    /// "Mubble" = "Muddled Bubble" - encrypts payloads using a shared code.
    /// </summary>
    public partial class MubbleWindow : Window
    {
        private readonly Settings _settings;

        /// <summary>
        /// Creates a new Mubble window with the provided settings.
        /// </summary>
        /// <param name="settings">The settings object containing Mubble configuration</param>
        public MubbleWindow(Settings settings)
        {
            InitializeComponent();

            _settings = settings;
            DataContext = _settings;

            UpdateStatusText();
        }

        /// <summary>
        /// Updates the status text based on current Mubble settings.
        /// </summary>
        private void UpdateStatusText()
        {
            if (_settings.MubbleEnabled)
            {
                if (string.IsNullOrWhiteSpace(_settings.MubbleCode))
                {
                    StatusText.Text = "⚠️ Enabled but no code set";
                    StatusText.Foreground = (System.Windows.Media.Brush)FindResource("WarningBrush");
                }
                else
                {
                    StatusText.Text = "🔒 Active & Protected";
                    StatusText.Foreground = (System.Windows.Media.Brush)FindResource("SuccessBrush");
                }
            }
            else
            {
                StatusText.Text = "🔓 Disabled";
                StatusText.Foreground = (System.Windows.Media.Brush)FindResource("SecondaryTextBrush");
            }
        }

        /// <summary>
        /// Handles Mubble toggle state changes.
        /// </summary>
        private void MubbleToggle_Changed(object sender, RoutedEventArgs e)
        {
            UpdateStatusText();
        }

        /// <summary>
        /// Copies the Mubble code to clipboard.
        /// </summary>
        private void CopyCode_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(_settings.MubbleCode))
            {
                Clipboard.SetText(_settings.MubbleCode);
                MessageBox.Show("Mubble code copied to clipboard!", "Copied", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("No code to copy. Please enter a Mubble code first.", "No Code", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Closes the window.
        /// </summary>
        private void Done_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

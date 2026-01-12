using System.Windows;
using BubbleNet.ViewModels;

namespace BubbleNet.Views
{
    /// <summary>
    /// Main application window for BubbleNet.
    /// Uses MainViewModel for all data binding and logic.
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Initializes the main window and sets up event handlers.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            // Subscribe to window closing event to properly dispose resources
            Closing += MainWindow_Closing;
        }

        /// <summary>
        /// Handles window closing to ensure proper cleanup.
        /// Disposes the ViewModel which stops network services and saves settings.
        /// </summary>
        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            // Dispose the ViewModel to clean up network resources and save settings
            if (DataContext is MainViewModel vm)
            {
                vm.Dispose();
            }
        }
    }
}

using System.Windows;

namespace BubbleNet
{
    /// <summary>
    /// Main application class for BubbleNet.
    /// Handles application lifecycle events and global configuration.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Called when the application starts.
        /// Initializes global resources and configuration.
        /// </summary>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Application-wide exception handling could be added here
            // DispatcherUnhandledException += OnDispatcherUnhandledException;
        }
    }
}

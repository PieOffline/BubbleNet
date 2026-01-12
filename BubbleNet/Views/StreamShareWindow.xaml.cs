using System;
using System.Windows;
using System.Windows.Threading;
using BubbleNet.Services;

namespace BubbleNet.Views
{
    /// <summary>
    /// Popup window shown while streaming is active.
    /// Displays stream statistics and provides a stop button.
    /// </summary>
    public partial class StreamShareWindow : Window
    {
        private readonly StreamShareService _streamService;
        private readonly DispatcherTimer _updateTimer;
        private readonly DateTime _startTime;

        /// <summary>Event raised when user requests to stop the stream</summary>
        public event EventHandler? StopRequested;

        /// <summary>
        /// Creates a new stream share window.
        /// </summary>
        /// <param name="streamService">The stream share service to monitor</param>
        /// <param name="streamId">The unique stream ID</param>
        /// <param name="targetWordCode">The target recipient</param>
        public StreamShareWindow(StreamShareService streamService, string streamId, string targetWordCode)
        {
            InitializeComponent();

            _streamService = streamService;
            _startTime = DateTime.Now;

            // Set initial values
            StreamIdText.Text = streamId;
            TargetText.Text = targetWordCode;
            ImageCountText.Text = "0";
            DurationText.Text = "00:00:00";

            // Create timer to update stats
            _updateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            _updateTimer.Tick += UpdateStats;
            _updateTimer.Start();

            // Handle window closing
            Closing += (s, e) =>
            {
                _updateTimer.Stop();
            };
        }

        /// <summary>
        /// Updates the stream statistics display.
        /// </summary>
        private void UpdateStats(object? sender, EventArgs e)
        {
            // Update image count
            ImageCountText.Text = _streamService.ImagesSent.ToString();

            // Update duration
            var duration = DateTime.Now - _startTime;
            DurationText.Text = duration.ToString(@"hh\:mm\:ss");

            // Check if streaming stopped externally
            if (!_streamService.IsStreaming)
            {
                _updateTimer.Stop();
                Close();
            }
        }

        /// <summary>
        /// Handles the stop streaming button click.
        /// </summary>
        private void StopStream_Click(object sender, RoutedEventArgs e)
        {
            _updateTimer.Stop();
            StopRequested?.Invoke(this, EventArgs.Empty);
            Close();
        }
    }
}

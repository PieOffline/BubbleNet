using System;
using System.Media;
using System.Threading.Tasks;

namespace BubbleNet.Services
{
    /// <summary>
    /// Defines the types of sound effects available in BubbleNet.
    /// Each type corresponds to a different user action or system event.
    /// </summary>
    public enum SoundType
    {
        /// <summary>Played when successfully connecting to the mesh network</summary>
        Connect,
        /// <summary>Played when disconnecting from the mesh network</summary>
        Disconnect,
        /// <summary>Played when successfully sending a transfer</summary>
        Send,
        /// <summary>Played when receiving a new transfer</summary>
        Receive,
        /// <summary>Played when an error occurs</summary>
        Error,
        /// <summary>Played for general UI clicks</summary>
        Click,
        /// <summary>Played when toggling a switch</summary>
        Toggle
    }

    /// <summary>
    /// Singleton service for playing sound effects throughout the application.
    /// Uses Windows system sounds as they're universally available and don't require
    /// additional assets to be bundled with the application.
    /// </summary>
    public class SoundService
    {
        // ===== Singleton Pattern =====
        // Ensures only one instance exists throughout the application lifetime
        private static readonly Lazy<SoundService> _instance = new(() => new SoundService());

        /// <summary>
        /// Gets the singleton instance of the SoundService.
        /// </summary>
        public static SoundService Instance => _instance.Value;

        // ===== Sound Settings =====
        private bool _soundEnabled = true;

        /// <summary>
        /// Gets or sets whether sound effects are enabled.
        /// When false, all PlayXxx methods become no-ops.
        /// </summary>
        public bool SoundEnabled
        {
            get => _soundEnabled;
            set => _soundEnabled = value;
        }

        /// <summary>
        /// Private constructor to enforce singleton pattern.
        /// </summary>
        private SoundService() { }

        /// <summary>
        /// Plays the specified sound type using Windows system sounds.
        /// Sound playback occurs on a background thread to avoid blocking the UI.
        /// </summary>
        /// <param name="soundType">The type of sound to play</param>
        /// <remarks>
        /// We use Windows system sounds as a fallback since custom sound files
        /// would require additional resources and handling. The system sounds
        /// provide adequate audio feedback for the application's needs.
        /// </remarks>
        public void PlaySound(SoundType soundType)
        {
            // Skip if sounds are disabled
            if (!_soundEnabled) return;

            // Play sound on background thread to prevent UI lag
            Task.Run(() =>
            {
                try
                {
                    // Map sound type to appropriate Windows system sound
                    SystemSound? sound = soundType switch
                    {
                        SoundType.Connect => SystemSounds.Asterisk,      // Pleasant notification
                        SoundType.Disconnect => SystemSounds.Hand,       // Warning-like sound
                        SoundType.Send => SystemSounds.Exclamation,      // Action completed
                        SoundType.Receive => SystemSounds.Beep,          // New item notification
                        SoundType.Error => SystemSounds.Hand,            // Error indication
                        SoundType.Click => SystemSounds.Beep,            // Subtle click feedback
                        SoundType.Toggle => SystemSounds.Beep,           // Toggle switch feedback
                        _ => null
                    };

                    // Play the sound if mapping found
                    sound?.Play();
                }
                catch
                {
                    // Silently ignore sound errors - sound is not critical functionality
                    // This can happen if audio device is unavailable or system sounds are disabled
                }
            });
        }

        // ===== Convenience Methods =====
        // These provide a cleaner API for common sound operations

        /// <summary>Plays the connection established sound</summary>
        public void PlayConnect() => PlaySound(SoundType.Connect);

        /// <summary>Plays the disconnection sound</summary>
        public void PlayDisconnect() => PlaySound(SoundType.Disconnect);

        /// <summary>Plays the send success sound</summary>
        public void PlaySend() => PlaySound(SoundType.Send);

        /// <summary>Plays the receive notification sound</summary>
        public void PlayReceive() => PlaySound(SoundType.Receive);

        /// <summary>Plays the error sound</summary>
        public void PlayError() => PlaySound(SoundType.Error);

        /// <summary>Plays the UI click sound</summary>
        public void PlayClick() => PlaySound(SoundType.Click);

        /// <summary>Plays the toggle switch sound</summary>
        public void PlayToggle() => PlaySound(SoundType.Toggle);
    }
}

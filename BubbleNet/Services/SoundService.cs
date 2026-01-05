using System;
using System.IO;
using System.Media;
using System.Threading.Tasks;

namespace BubbleNet.Services
{
    public enum SoundType
    {
        Connect,
        Disconnect,
        Send,
        Receive,
        Error,
        Click,
        Toggle
    }

    public class SoundService
    {
        private static readonly Lazy<SoundService> _instance = new(() => new SoundService());
        public static SoundService Instance => _instance.Value;

        private bool _soundEnabled = true;
        public bool SoundEnabled
        {
            get => _soundEnabled;
            set => _soundEnabled = value;
        }

        private SoundService() { }

        public void PlaySound(SoundType soundType)
        {
            if (!_soundEnabled) return;

            Task.Run(() =>
            {
                try
                {
                    // Use system sounds as fallback since we can't embed custom sounds easily
                    SystemSound? sound = soundType switch
                    {
                        SoundType.Connect => SystemSounds.Asterisk,
                        SoundType.Disconnect => SystemSounds.Hand,
                        SoundType.Send => SystemSounds.Exclamation,
                        SoundType.Receive => SystemSounds.Beep,
                        SoundType.Error => SystemSounds.Hand,
                        SoundType.Click => SystemSounds.Beep,
                        SoundType.Toggle => SystemSounds.Beep,
                        _ => null
                    };
                    sound?.Play();
                }
                catch
                {
                    // Silently ignore sound errors
                }
            });
        }

        public void PlayConnect() => PlaySound(SoundType.Connect);
        public void PlayDisconnect() => PlaySound(SoundType.Disconnect);
        public void PlaySend() => PlaySound(SoundType.Send);
        public void PlayReceive() => PlaySound(SoundType.Receive);
        public void PlayError() => PlaySound(SoundType.Error);
        public void PlayClick() => PlaySound(SoundType.Click);
        public void PlayToggle() => PlaySound(SoundType.Toggle);
    }
}

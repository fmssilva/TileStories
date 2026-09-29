namespace TileStories
{
    // The Android fallback of the work plan's Stage 2 item 3, as pure logic: Unity's own AudioSettings.OnAudioConfigurationChanged is
    // reported not to fire reliably on Android when Bluetooth earbuds disconnect, so while audio plays a poll asks the platform whether
    // a Bluetooth A2DP output is on (about every PollSeconds). A true -> false change is a disconnect the callback missed. This class
    // only judges the readings; the poll itself is CardAudioPlayer's (#if UNITY_ANDROID, behind card_settings.container.audio_android_output_poll).
    public sealed class OutputRouteWatcher
    {
        // How often the platform is asked while audio plays (work plan: every 1-2 seconds)
        public const float PollSeconds = 1.5f;

        private bool _known;
        private bool _last;

        // A new reading (`bluetoothOutputOn`). True when it is a disconnect: the output was Bluetooth and is not any more
        public bool Observe(bool bluetoothOutputOn)
        {
            bool dropped = _known && _last && !bluetoothOutputOn;
            _known = true;
            _last = bluetoothOutputOn;
            return dropped;
        }

        // Forget what was read (audio stopped: the next reading starts a new comparison)
        public void Reset() => _known = false;
    }
}

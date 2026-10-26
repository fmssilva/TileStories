using System;
using UnityEngine;

namespace TileStories
{
    // The MonoBehaviour side of the card's audio (_3.1 step 9A), thin on purpose: it owns the ONE AudioSource, gives the service its
    // per-frame Tick, and forwards the three things only a MonoBehaviour can hear -- OnApplicationPause, AudioSettings'
    // OnAudioConfigurationChanged and, on Android with the wall's flag on, a poll of the platform for a Bluetooth disconnect the callback
    // missed (the work plan's Stage 2 item 3; OutputRouteWatcher judges the readings). Every decision is CardAudioService's.
    [RequireComponent(typeof(AudioSource))]
    public sealed class CardAudioPlayer : MonoBehaviour
    {
        // The service this player ticks and forwards to (PoiCardHost builds it; a test may install its own over a ManualAudioOutput)
        public CardAudioService Service { get; set; }

        // Whether to poll the platform for a Bluetooth disconnect (card_settings.container.audio_android_output_poll now)
        public Func<bool> PollForOutputLoss { get; set; } = () => false;

        private readonly OutputRouteWatcher _route = new();
        private float _sincePoll;

        // The AudioSource wrapped as the service's sound device
        public UnityAudioOutput CreateOutput() => new UnityAudioOutput(GetComponent<AudioSource>());

        private void OnEnable() => AudioSettings.OnAudioConfigurationChanged += OnConfigurationChanged;

        private void OnDisable() => AudioSettings.OnAudioConfigurationChanged -= OnConfigurationChanged;

        private void Update()
        {
            if (Service == null) return;
            Service.Tick();
            PollTheOutputRoute();
        }

        private void OnApplicationPause(bool paused) => Service?.OnApplicationPause(paused);

        private void OnConfigurationChanged(bool deviceWasChanged) => Service?.OnAudioConfigurationChanged(deviceWasChanged);

        // While audio plays and the wall asks for it: read whether Bluetooth is the output every PollSeconds; a drop is a device change
        private void PollTheOutputRoute()
        {
            if (Service == null || Service.State != CardAudioState.Playing || !PollForOutputLoss())
            {
                _route.Reset();
                _sincePoll = 0f;
                return;
            }
            _sincePoll += Time.unscaledDeltaTime;
            if (_sincePoll < OutputRouteWatcher.PollSeconds) return;
            _sincePoll = 0f;
            if (TryReadBluetoothOutput(out bool bluetoothOn) && _route.Observe(bluetoothOn))
                Service.OnAudioConfigurationChanged(true);
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        // Ask Android's AudioManager whether a Bluetooth A2DP output is on (the work plan's documented AndroidJavaObject fallback)
        private static bool TryReadBluetoothOutput(out bool on)
        {
            on = false;
            try
            {
                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                using (var audioManager = activity.Call<AndroidJavaObject>("getSystemService", "audio"))
                {
                    on = audioManager.Call<bool>("isBluetoothA2dpOn");
                    return true;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("[Audio] output route poll failed: " + e.Message);
                return false;
            }
        }
#else
        // - no platform to ask outside an Android player: nothing to read (the flag does nothing there)
        private static bool TryReadBluetoothOutput(out bool on)
        {
            on = false;
            return false;
        }
#endif
    }
}

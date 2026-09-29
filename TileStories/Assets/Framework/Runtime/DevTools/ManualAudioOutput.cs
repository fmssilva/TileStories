using UnityEngine;

namespace TileStories
{
    // A silent sound device whose clip time moves only when a caller says (_3.1 step 9A): what the Phase A gallery card plays through,
    // and what tests hand the card's audio service so audio time is a number they set, never a real-time wait. It behaves like
    // AudioSource where the service depends on it: a paused output keeps its time, a clip that runs out stops playing and its time
    // goes back to 0, a speed above 1 moves the clip faster, and DropPlayback is the engine losing the clip (a Bluetooth disconnect).
    public sealed class ManualAudioOutput : IAudioOutput
    {
        private AudioClip _clip;
        private float _time;

        public bool IsPlaying { get; private set; }
        public float Length => _clip != null ? _clip.length : 0f;
        public float Volume { get; set; } = 1f;
        public float Speed { get; set; } = 1f;
        public AudioClip Clip => _clip;
        // How the service used the device (tests read these)
        public int LoadCount { get; private set; }
        public int PlayCount { get; private set; }

        public float Time
        {
            get => _time;
            set => _time = Mathf.Clamp(value, 0f, Length);
        }

        public void Load(AudioClip clip)
        {
            _clip = clip;
            _time = 0f;
            IsPlaying = false;
            LoadCount++;
        }

        public void Play()
        {
            if (_clip == null) return;
            IsPlaying = true;
            PlayCount++;
        }

        public void Pause() => IsPlaying = false;

        public void Stop()
        {
            IsPlaying = false;
            _time = 0f;
        }

        // `seconds` of real time pass: a playing clip moves on by that much at its speed, and stops (time 0) when it runs out
        public void Advance(float seconds)
        {
            if (!IsPlaying || _clip == null) return;
            _time += seconds * Speed;
            if (_time >= Length)
            {
                IsPlaying = false;
                _time = 0f;
            }
        }

        // The engine lost the clip (an output device change): nothing plays, the time is gone, the clip is still "loaded"
        public void DropPlayback()
        {
            IsPlaying = false;
            _time = 0f;
        }
    }
}

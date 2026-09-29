using UnityEngine;

namespace TileStories
{
    // IAudioOutput over the ONE AudioSource of the card's audio (_3.1 step 9A): 2D (a narration is not placed in the room), no loop, no
    // play-on-awake. CardAudioPlayer creates it; nothing else in the app owns an AudioSource for the card.
    public sealed class UnityAudioOutput : IAudioOutput
    {
        private readonly AudioSource _source;

        public UnityAudioOutput(AudioSource source)
        {
            _source = source;
            _source.playOnAwake = false;
            _source.loop = false;
            _source.spatialBlend = 0f;
        }

        public bool IsPlaying => _source.isPlaying;

        public float Time
        {
            get => _source.time;
            // - AudioSource.time throws a warning past the clip's end: keep a hair inside it
            set { if (_source.clip != null) _source.time = Mathf.Clamp(value, 0f, Mathf.Max(0f, _source.clip.length - 0.01f)); }
        }

        public float Length => _source.clip != null ? _source.clip.length : 0f;

        public float Volume
        {
            get => _source.volume;
            set => _source.volume = Mathf.Clamp01(value);
        }

        public float Speed
        {
            get => _source.pitch;
            set => _source.pitch = value;
        }

        public void Load(AudioClip clip)
        {
            _source.Stop();
            _source.clip = clip;
            _source.time = 0f;
        }

        public void Play() => _source.Play();

        public void Pause() => _source.Pause();

        public void Stop() => _source.Stop();
    }
}

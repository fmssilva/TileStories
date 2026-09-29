using UnityEngine;
using UnityEngine.Video;

namespace TileStories
{
    // A video device that decodes nothing and whose clip time moves only when a caller says (_3.1 step 9B): what the Phase A gallery card
    // plays through, and what tests hand the card's video service so video time is a number they set, never a real-time wait. It behaves like
    // the real player where the service and the views depend on it: a loaded clip has NO frame until time first moves after Play (a real
    // player prepares first -- the poster shows meanwhile), a paused clip keeps its time, a clip that does not loop stops at its end and says
    // Ended, a looping one starts over. Its Texture is a small RenderTexture painted a flat colour per whole second of the clip
    // (FrameColourAt), so a render pixel tells which second of the video a view shows.
    public sealed class ManualVideoOutput : IVideoOutput
    {
        private VideoClip _clip;
        private float _time;
        private RenderTexture _texture;

        public bool IsPlaying { get; private set; }
        public bool Ended { get; private set; }
        public bool HasFrame { get; private set; }
        public bool Muted { get; set; }
        public bool Looping { get; set; }
        public VideoClip Clip => _clip;
        public float Length => _clip != null ? (float)_clip.length : 0f;
        public Texture Texture => _texture;
        // How the service used the device (tests read these)
        public int LoadCount { get; private set; }
        public int PlayCount { get; private set; }

        public float Time
        {
            get => _time;
            set
            {
                _time = Mathf.Clamp(value, 0f, Length);
                if (HasFrame) Paint();
            }
        }

        // The flat colour the texture shows for second `second` of a clip (six colours in turn)
        public static Color FrameColourAt(int second)
        {
            switch (((second % 6) + 6) % 6)
            {
                case 0: return new Color(0.80f, 0.20f, 0.20f);
                case 1: return new Color(0.20f, 0.70f, 0.25f);
                case 2: return new Color(0.20f, 0.35f, 0.85f);
                case 3: return new Color(0.85f, 0.75f, 0.15f);
                case 4: return new Color(0.70f, 0.25f, 0.75f);
                default: return new Color(0.15f, 0.75f, 0.80f);
            }
        }

        public void Load(VideoClip clip)
        {
            _clip = clip;
            _time = 0f;
            IsPlaying = false;
            Ended = false;
            HasFrame = false;
            LoadCount++;
            if (_texture == null)
            {
                _texture = new RenderTexture(64, 36, 0) { name = "ManualVideo" };
                _texture.Create();
            }
        }

        public void Play()
        {
            if (_clip == null) return;
            IsPlaying = true;
            Ended = false;
            PlayCount++;
        }

        public void Pause() => IsPlaying = false;

        public void Stop()
        {
            _clip = null;
            _time = 0f;
            IsPlaying = false;
            Ended = false;
            HasFrame = false;
        }

        // `seconds` of real time pass: a playing clip moves on (its first frame arrives now), a looping one wraps, another stops at its end
        public void Advance(float seconds)
        {
            if (!IsPlaying || _clip == null) return;
            _time += seconds;
            if (_time >= Length)
            {
                if (Looping && Length > 0f) _time %= Length;
                else
                {
                    _time = Length;
                    IsPlaying = false;
                    Ended = true;
                }
            }
            HasFrame = true;
            Paint();
        }

        // Free the texture (the gallery harness and tests call it when they are done)
        public void Release()
        {
            if (_texture == null) return;
            _texture.Release();
            if (Application.isPlaying) Object.Destroy(_texture);
            else Object.DestroyImmediate(_texture);
            _texture = null;
        }

        private void Paint()
        {
            if (_texture == null) return;
            var previous = RenderTexture.active;
            RenderTexture.active = _texture;
            GL.Clear(true, true, FrameColourAt(Mathf.FloorToInt(_time)));
            RenderTexture.active = previous;
        }
    }
}

using System;
using UnityEngine;
using UnityEngine.Video;

namespace TileStories
{
    // IVideoOutput over the ONE VideoPlayer of the card's video (_3.1 step 9B): it renders into its own RenderTexture (sized to the clip), plays
    // the clip's sound straight to the device (Direct; muted for a header loop), never plays on awake. A seek asked before the clip is prepared
    // is applied once it is. CardVideoPlayer creates it; nothing else in the app owns a VideoPlayer for the card.
    public sealed class UnityVideoOutput : IVideoOutput, IDisposable
    {
        private readonly VideoPlayer _player;
        private RenderTexture _texture;
        private bool _ended;
        private bool _muted;
        private float? _pendingSeek;

        public UnityVideoOutput(VideoPlayer player)
        {
            _player = player;
            _player.playOnAwake = false;
            _player.isLooping = false;
            _player.renderMode = VideoRenderMode.RenderTexture;
            _player.audioOutputMode = VideoAudioOutputMode.Direct;
            _player.skipOnDrop = true;
            _player.waitForFirstFrame = true;
            _player.loopPointReached += _ => { if (!_player.isLooping) _ended = true; };
            _player.prepareCompleted += _ =>
            {
                if (!_pendingSeek.HasValue) return;
                _player.time = _pendingSeek.Value;
                _pendingSeek = null;
            };
        }

        public bool IsPlaying => _player.isPlaying;
        public bool Ended => _ended;
        public bool HasFrame => _player.clip != null && _player.isPrepared && _player.frame >= 0;
        public Texture Texture => _texture;

        public float Time
        {
            get => _pendingSeek ?? (float)_player.time;
            set
            {
                if (_player.clip == null) return;
                float at = Mathf.Clamp(value, 0f, (float)_player.clip.length);
                if (_player.isPrepared) _player.time = at;
                else _pendingSeek = at;
            }
        }

        public bool Muted
        {
            get => _muted;
            set
            {
                _muted = value;
                for (ushort t = 0; t < _player.controlledAudioTrackCount; t++) _player.SetDirectAudioMute(t, value);
            }
        }

        public bool Looping
        {
            get => _player.isLooping;
            set => _player.isLooping = value;
        }

        public void Load(VideoClip clip)
        {
            _player.Stop();
            _ended = false;
            _pendingSeek = null;
            _player.clip = clip;
            if (clip == null) return;
            EnsureTexture((int)clip.width, (int)clip.height);
            _player.targetTexture = _texture;
            _player.controlledAudioTrackCount = clip.audioTrackCount;
            for (ushort t = 0; t < clip.audioTrackCount; t++)
            {
                _player.EnableAudioTrack(t, true);
                _player.SetDirectAudioMute(t, _muted);
            }
            _player.Prepare();
        }

        public void Play()
        {
            if (_player.clip == null) return;
            _ended = false;
            _player.Play();
        }

        public void Pause() => _player.Pause();

        public void Stop()
        {
            _player.Stop();
            _player.clip = null;
            _ended = false;
            _pendingSeek = null;
        }

        public void Dispose()
        {
            if (_texture == null) return;
            _texture.Release();
            if (Application.isPlaying) UnityEngine.Object.Destroy(_texture);
            else UnityEngine.Object.DestroyImmediate(_texture);
            _texture = null;
        }

        // One texture reused for every clip of the same size: a new one only when the size changes
        private void EnsureTexture(int width, int height)
        {
            if (_texture != null && _texture.width == width && _texture.height == height) return;
            Dispose();
            _texture = new RenderTexture(Mathf.Max(1, width), Mathf.Max(1, height), 0) { name = "CardVideo" };
            _texture.Create();
        }
    }
}

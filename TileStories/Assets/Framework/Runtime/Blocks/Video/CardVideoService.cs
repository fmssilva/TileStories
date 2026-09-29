using System;
using UnityEngine;
using UnityEngine.Video;

namespace TileStories
{
    // The card's ONE video owner (_3.1 step 9B): plain C#, no MonoBehaviour, beside the audio owner (CardAudioService). One player for the
    // whole card, never one per view: it decides what holds the player -- the visitor's video (a tap; with its sound) or a header's loop
    // (muted, looping; only while no visitor video holds the player) -- loads the clip through the card's media source and gives it back when
    // the video ends, is replaced or the card closes. The frames go through an IVideoOutput (a VideoPlayer in the app), so every rule here is a
    // plain test and a test moves video time by moving the output, never by waiting. What starting a video does to the card's audio is not
    // decided here: CardSoundCoordinator watches both owners (CardSoundRule).
    public sealed class CardVideoService : ICardVideo
    {
        private readonly IVideoOutput _output;
        private readonly Func<IMediaSource> _media;

        private VideoTrack _current;
        private CardVideoState _state = CardVideoState.Idle;
        private bool _playingLoop;
        // The loop a header asked for (it takes the player whenever no visitor video holds it)
        private VideoTrack _loopRequest;
        // What the output holds now: the media source + path it came from (given back when the video is over) and the clip's length
        private IMediaSource _held;
        private string _heldPath;
        private float _length;
        private float _lastPosition;
        // Paused by the app going to the background (not by the visitor): it resumes when the app comes back
        private bool _heldByApp;

        // `media`: the card's media source now (asked at every start: the wall's media folder may change)
        public CardVideoService(IVideoOutput output, Func<IMediaSource> media)
        {
            _output = output;
            _media = media;
        }

        public event Action Changed;

        public CardVideoState State => _state;
        public VideoTrack Current => _current;
        public bool PlayingLoop => _playingLoop;
        public float Length => _current != null ? _length : 0f;
        public float Position => _state == CardVideoState.Idle ? 0f : _state == CardVideoState.Playing && !_heldByApp ? _output.Time : _lastPosition;
        public bool HasFrame => _current != null && _output.HasFrame;
        public Texture Texture => _output.Texture;

        public bool IsCurrent(VideoTrack track) => _current != null && _current.SameVideo(track);

        public void Toggle(VideoTrack track)
        {
            if (track == null || string.IsNullOrWhiteSpace(track.ClipPath)) return;
            if (IsCurrent(track) && !_playingLoop)
            {
                if (_state == CardVideoState.Playing) Pause();
                else if (_state == CardVideoState.Paused) Resume();
                return;
            }
            // - a loop or another video gives way: one player, one picture
            if (_current != null) LetGoOfCurrent();
            if (!Start(track, false)) StartLoopIfWanted();
        }

        public void Pause()
        {
            if (_state != CardVideoState.Playing) return;
            _lastPosition = _output.Time;
            _output.Pause();
            _heldByApp = false;
            _state = CardVideoState.Paused;
            Changed?.Invoke();
        }

        public void Resume()
        {
            if (_state != CardVideoState.Paused) return;
            _output.Time = _lastPosition;
            _output.Play();
            _state = CardVideoState.Playing;
            Changed?.Invoke();
        }

        // Move the visitor's video to `seconds` (clamped to the clip), playing or paused; a loop is not the visitor's to move
        public void Seek(float seconds)
        {
            if (_current == null || _playingLoop || float.IsNaN(seconds)) return;
            float at = Mathf.Clamp(seconds, 0f, Mathf.Max(0f, _length - 0.05f));
            _output.Time = at;
            _lastPosition = at;
            Changed?.Invoke();
        }

        public void Stop()
        {
            if (_current == null || _playingLoop) return;
            LetGoOfCurrent();
            Changed?.Invoke();
            StartLoopIfWanted();
        }

        public void PlayLoop(VideoTrack loop)
        {
            if (loop == null || string.IsNullOrWhiteSpace(loop.ClipPath)) return;
            bool same = _loopRequest != null && _loopRequest.SameVideo(loop);
            _loopRequest = loop;
            if (_current == null) StartLoopIfWanted();
            // - another header's loop still holds the player: the new header's takes over
            else if (_playingLoop && !same)
            {
                LetGoOfCurrent();
                StartLoopIfWanted();
            }
        }

        public void StopLoop(VideoTrack loop)
        {
            if (_loopRequest == null || !_loopRequest.SameVideo(loop)) return;
            _loopRequest = null;
            if (!_playingLoop) return;
            LetGoOfCurrent();
            Changed?.Invoke();
        }

        public void CardShown(string poiId)
        {
            if (_current != null && !_playingLoop && _current.PoiId != poiId) Stop();
        }

        public void CardClosed()
        {
            _loopRequest = null;
            if (_current == null) return;
            LetGoOfCurrent();
            Changed?.Invoke();
        }

        // One step of time, called every frame by whoever owns the service (CardVideoPlayer; a test after moving its output): notices a video
        // that ran out (a loop never does) and announces the progress and the new frame to the views
        public void Tick()
        {
            if (_state != CardVideoState.Playing || _heldByApp) return;
            if (_output.Ended && !_playingLoop)
            {
                LetGoOfCurrent();
                Changed?.Invoke();
                StartLoopIfWanted();
                return;
            }
            _lastPosition = _output.Time;
            Changed?.Invoke();
        }

        // The app went to the background or came back (MonoBehaviour.OnApplicationPause): hold what plays, resume it at the same place
        public void OnApplicationPause(bool paused)
        {
            if (paused)
            {
                if (_state != CardVideoState.Playing || _heldByApp) return;
                _lastPosition = _output.Time;
                _output.Pause();
                _heldByApp = true;
            }
            else
            {
                if (!_heldByApp) return;
                _heldByApp = false;
                _output.Time = _lastPosition;
                _output.Play();
            }
            Changed?.Invoke();
        }

        // Load `track`'s clip through the card's media source and start it (a loop muted and looping); false (and Idle) when it cannot be loaded
        private bool Start(VideoTrack track, bool loop)
        {
            var media = _media?.Invoke();
            var clip = media?.Load<VideoClip>(track.ClipPath);
            if (clip == null)
            {
                Changed?.Invoke();
                return false;
            }
            _held = media;
            _heldPath = track.ClipPath;
            _length = (float)clip.length;
            _output.Load(clip);
            _output.Muted = loop;
            _output.Looping = loop;
            _output.Play();
            _current = track;
            _playingLoop = loop;
            _state = CardVideoState.Playing;
            _lastPosition = 0f;
            _heldByApp = false;
            Changed?.Invoke();
            return true;
        }

        private void StartLoopIfWanted()
        {
            if (_loopRequest != null && _current == null) Start(_loopRequest, true);
        }

        // Stop and forget whatever holds the player, and give its clip back
        private void LetGoOfCurrent()
        {
            _output.Stop();
            if (_held != null) _held.Release(_heldPath);
            _held = null;
            _heldPath = null;
            _current = null;
            _playingLoop = false;
            _state = CardVideoState.Idle;
            _length = 0f;
            _lastPosition = 0f;
            _heldByApp = false;
        }
    }
}

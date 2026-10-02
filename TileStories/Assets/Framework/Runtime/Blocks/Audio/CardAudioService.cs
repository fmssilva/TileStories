using System;
using System.Collections.Generic;
using UnityEngine;

namespace TileStories
{
    // The card's ONE audio owner (_3.1 step 9A): plain C#, no MonoBehaviour. It decides what plays (never two audios at once: a new one
    // switches -- the playing one fades out over half a second -- or queues, by card_settings.container.audio_when_another_starts), loads
    // the clip through the card's media source and gives it back when the audio ends or is replaced, and survives the card: closing the
    // card does not stop it (the host decides that with Keep Audio Playing; the mini-player shows it). The sound itself goes through an
    // IAudioOutput (an AudioSource in the app), the time of a fade through an injectable clock, and the interruptions of the work plan's
    // Stage 2 item 3 (the app in the background, the output device changing) through AudioInterruptionRule -- so every rule here is a plain
    // test, and a test moves audio time by moving the output, never by waiting.
    public sealed class CardAudioService : ICardAudio
    {
        // A clip that stops by itself within this many seconds of its end (scaled by the speed) has ended; earlier, the engine lost it
        private const float EndToleranceSeconds = 0.75f;
        // How many times in a row a lost clip is loaded and started again before the service gives up and holds it paused
        private const int MaxRestarts = 3;
        // Seconds of real progress after a restart that count as "playing again": the restart budget is refilled
        private const float ProgressThatRefillsRestarts = 5f;

        private readonly IAudioOutput _output;
        private readonly Func<IMediaSource> _media;
        private readonly Func<float> _clock;
        private readonly Func<string> _mode;
        private readonly AudioInterruptionRule _interruption = new();
        private readonly List<AudioTrack> _queue = new();

        private AudioTrack _current;
        private CardAudioState _state = CardAudioState.Idle;
        private float _speed = AudioSpeedRule.Normal;
        // What the output holds now: the clip and the media source + path it came from (given back when the audio is over)
        private AudioClip _clip;
        private IMediaSource _held;
        private string _heldPath;
        // The audio that is fading out while the new current one waits to start (its media is given back when the fade ends)
        private bool _fading;
        private float _fadeStart;
        private IMediaSource _outgoingHeld;
        private string _outgoingPath;
        // A pause asked while the switch fades (the visitor's tap, or a video starting): the new audio then starts paused when the fade ends.
        // The state already reads Paused, so the players and CardSoundCoordinator see the pause the moment it is asked
        private bool _pauseAfterFade;
        // The last position seen while playing: where a lost clip is put back, and what tells "ended" from "lost"
        private float _lastPosition;
        private int _restarts;
        private float _restartAt;

        // `media`: the card's media source now (asked at every start: the wall's media folder may change); `clock`: seconds, for the
        // fade; `mode`: the wall's audio_when_another_starts now (a live edit reaches the next start)
        public CardAudioService(IAudioOutput output, Func<IMediaSource> media, Func<float> clock, Func<string> mode)
        {
            _output = output;
            _media = media;
            _clock = clock ?? (() => Time.unscaledTime);
            _mode = mode;
        }

        public event Action Changed;

        public CardAudioState State => _state;
        public AudioTrack Current => _current;
        public float Speed => _speed;
        public IReadOnlyList<AudioTrack> Queue => _queue;
        public float Length => _clip != null && !_fading ? _clip.length : 0f;

        // Seconds into the current audio (0 while none plays or a switch is still fading the old one out)
        public float Position => _state == CardAudioState.Idle || _fading ? 0f : _output.IsPlaying ? _output.Time : _lastPosition;

        public bool IsCurrent(AudioTrack track) => _current != null && _current.SameAudio(track);

        public bool IsQueued(AudioTrack track) => QueueIndex(track) >= 0;

        // Play a new audio (now, switching or queued), pause a playing one, resume a paused one, or take a queued one out of the queue
        public void Toggle(AudioTrack track)
        {
            if (track == null || string.IsNullOrWhiteSpace(track.ClipPath)) return;
            if (IsCurrent(track))
            {
                // - during a switch fade too: the pause (or the resume) is kept for the moment the new audio starts
                if (_state == CardAudioState.Playing) Pause();
                else if (_state == CardAudioState.Paused) Resume();
                return;
            }
            int queued = QueueIndex(track);
            if (queued >= 0)
            {
                _queue.RemoveAt(queued);
                Changed?.Invoke();
                return;
            }
            bool playing = _current != null && _state == CardAudioState.Playing;
            switch (AudioSwitchRule.Decide(_mode?.Invoke(), playing))
            {
                case AudioSwitchRule.Decision.StartNow:
                    // - a paused audio the visitor moved on from is simply let go
                    if (_current != null) LetGoOfCurrent();
                    StartTrack(track, 0f);
                    break;
                case AudioSwitchRule.Decision.FadeThenStart:
                    BeginFade(track);
                    break;
                default:
                    _queue.Add(track);
                    Changed?.Invoke();
                    break;
            }
        }

        public void Pause()
        {
            if (_state != CardAudioState.Playing) return;
            if (_fading)
            {
                // - nothing of the new audio plays yet: remember the pause, the fade-out of the old one goes on
                _pauseAfterFade = true;
                _state = CardAudioState.Paused;
                Changed?.Invoke();
                return;
            }
            _lastPosition = _output.Time;
            _output.Pause();
            _interruption.Forget();
            _state = CardAudioState.Paused;
            Changed?.Invoke();
        }

        public void Resume()
        {
            if (_state != CardAudioState.Paused) return;
            if (_fading)
            {
                // - a pause asked during this fade is taken back: the new audio starts playing when the fade ends, as first asked
                _pauseAfterFade = false;
                _state = CardAudioState.Playing;
                Changed?.Invoke();
                return;
            }
            _output.Time = _lastPosition;
            _output.Play();
            _interruption.Forget();
            _restarts = 0;
            _state = CardAudioState.Playing;
            Changed?.Invoke();
        }

        // Move to `seconds` of the current audio (clamped to the clip), playing or paused
        public void Seek(float seconds)
        {
            if (_current == null || _clip == null || _fading || float.IsNaN(seconds)) return;
            float at = Mathf.Clamp(seconds, 0f, Mathf.Max(0f, _clip.length - 0.05f));
            _output.Time = at;
            _lastPosition = at;
            _restarts = 0;
            Changed?.Invoke();
        }

        public void SetSpeed(float speed)
        {
            if (!(speed > 0f)) return;
            _speed = speed;
            _output.Speed = speed;
            Changed?.Invoke();
        }

        public void UpdateTitle(AudioTrack track)
        {
            if (!IsCurrent(track) || track.Title == _current.Title) return;
            _current = track;
            Changed?.Invoke();
        }

        // End the current audio, give its clip back and forget the queue
        public void Stop()
        {
            bool had = _current != null || _queue.Count > 0;
            _queue.Clear();
            if (_current != null || _fading) LetGoOfCurrent();
            if (had) Changed?.Invoke();
        }

        // One step of time, called every frame by whoever owns the service (CardAudioPlayer; a test after moving its output and clock):
        // moves a fade along, notices an audio that ended (and starts the next queued one) or was lost by the engine, and announces the
        // progress to the players
        public void Tick()
        {
            if (_fading)
            {
                float elapsed = _clock() - _fadeStart;
                _output.Volume = AudioSwitchRule.FadeVolume(elapsed);
                if (AudioSwitchRule.FadeFinished(elapsed)) FinishFade();
                Changed?.Invoke();
                return;
            }
            if (_state != CardAudioState.Playing || _interruption.HeldByApp) return;
            if (_output.IsPlaying)
            {
                _lastPosition = _output.Time;
                if (_restarts > 0 && _lastPosition - _restartAt >= ProgressThatRefillsRestarts) _restarts = 0;
                Changed?.Invoke();
                return;
            }
            // - the output stopped by itself: the clip ran out, or the engine dropped it (a device change no callback announced)
            bool ended = _clip == null || _lastPosition >= _clip.length - EndToleranceSeconds * Mathf.Max(1f, _speed);
            if (ended)
            {
                EndCurrent();
                return;
            }
            if (_restarts >= MaxRestarts)
            {
                _state = CardAudioState.Paused;
                Changed?.Invoke();
                return;
            }
            _restarts++;
            _restartAt = _lastPosition;
            ReloadOutput(_lastPosition, true);
            Changed?.Invoke();
        }

        // The app went to the background or came back (MonoBehaviour.OnApplicationPause): pause explicitly, resume at the same place
        public void OnApplicationPause(bool paused)
        {
            if (_current == null || _fading) return;
            float position = _output.IsPlaying ? _output.Time : _lastPosition;
            Apply(_interruption.AppPaused(paused, _state == CardAudioState.Playing, position));
        }

        // AudioSettings.OnAudioConfigurationChanged: the output device changed and the engine forgot what it was playing
        public void OnAudioConfigurationChanged(bool deviceWasChanged)
        {
            if (_current == null || _fading) return;
            float position = _output.IsPlaying ? _output.Time : _lastPosition;
            Apply(_interruption.DeviceChanged(deviceWasChanged, _state == CardAudioState.Playing, position));
        }

        private void Apply(AudioInterruptionRule.Result result)
        {
            switch (result.Step)
            {
                case AudioInterruptionRule.Step.Pause:
                    _lastPosition = result.Position;
                    _output.Pause();
                    break;
                case AudioInterruptionRule.Step.Resume:
                    _output.Time = result.Position;
                    _output.Play();
                    _lastPosition = result.Position;
                    break;
                case AudioInterruptionRule.Step.Reload:
                    ReloadOutput(result.Position, result.ResumePlaying);
                    break;
                default:
                    return;
            }
            Changed?.Invoke();
        }

        // Load the current clip into the output again at `position` (after the engine lost it), playing when it was
        private void ReloadOutput(float position, bool play)
        {
            if (_clip == null) return;
            _output.Load(_clip);
            _output.Speed = _speed;
            _output.Volume = 1f;
            _output.Time = position;
            _lastPosition = position;
            if (play) _output.Play();
        }

        // Load `track`'s clip through the card's media source and start it at `startAt` -- or hold it there paused (`play` false: a pause asked
        // during a switch fade); false (and Idle) when the clip cannot be loaded
        private bool StartTrack(AudioTrack track, float startAt, bool play = true)
        {
            var media = _media?.Invoke();
            var clip = media?.Load<AudioClip>(track.ClipPath);
            if (clip == null)
            {
                _current = null;
                _state = CardAudioState.Idle;
                Changed?.Invoke();
                return false;
            }
            _held = media;
            _heldPath = track.ClipPath;
            _clip = clip;
            _output.Load(clip);
            _output.Volume = 1f;
            _output.Speed = _speed;
            _output.Time = startAt;
            if (play) _output.Play();
            _current = track;
            _state = play ? CardAudioState.Playing : CardAudioState.Paused;
            _lastPosition = startAt;
            _restarts = 0;
            _interruption.Forget();
            Changed?.Invoke();
            return true;
        }

        // The playing audio starts to fade out; `next` is the current audio from now on and starts when the fade is over
        private void BeginFade(AudioTrack next)
        {
            if (!_fading)
            {
                _outgoingHeld = _held;
                _outgoingPath = _heldPath;
                _held = null;
                _heldPath = null;
                _clip = null;
                _fading = true;
                _fadeStart = _clock();
            }
            _current = next;
            _state = CardAudioState.Playing;
            // - a new audio asked to play: a pause asked for the one it replaces no longer applies
            _pauseAfterFade = false;
            Changed?.Invoke();
        }

        // The old audio is silent: stop it, give its clip back, start the new one -- paused at its start when a pause was asked meanwhile
        private void FinishFade()
        {
            _fading = false;
            bool paused = _pauseAfterFade;
            _pauseAfterFade = false;
            _output.Stop();
            ReleaseOutgoing();
            var next = _current;
            _current = null;
            _state = CardAudioState.Idle;
            StartTrack(next, 0f, play: !paused);
        }

        // The current audio ran out: give its clip back and go on with the queue
        private void EndCurrent()
        {
            _output.Stop();
            ReleaseHeld();
            _current = null;
            _clip = null;
            _state = CardAudioState.Idle;
            _lastPosition = 0f;
            _interruption.Forget();
            Changed?.Invoke();
            if (_queue.Count == 0) return;
            var next = _queue[0];
            _queue.RemoveAt(0);
            StartTrack(next, 0f);
        }

        // Silence and forget whatever is current (a paused audio replaced, or Stop)
        private void LetGoOfCurrent()
        {
            _fading = false;
            _pauseAfterFade = false;
            _output.Stop();
            _output.Volume = 1f;
            ReleaseHeld();
            ReleaseOutgoing();
            _current = null;
            _clip = null;
            _state = CardAudioState.Idle;
            _lastPosition = 0f;
            _interruption.Forget();
        }

        private void ReleaseHeld()
        {
            if (_held != null) _held.Release(_heldPath);
            _held = null;
            _heldPath = null;
        }

        private void ReleaseOutgoing()
        {
            if (_outgoingHeld != null) _outgoingHeld.Release(_outgoingPath);
            _outgoingHeld = null;
            _outgoingPath = null;
        }

        private int QueueIndex(AudioTrack track)
        {
            for (int i = 0; i < _queue.Count; i++)
                if (_queue[i].SameAudio(track)) return i;
            return -1;
        }
    }
}

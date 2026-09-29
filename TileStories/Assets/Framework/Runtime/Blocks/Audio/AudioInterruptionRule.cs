namespace TileStories
{
    // What a narration does when the phone interrupts it (_3.1 step 9A; the work plan's Stage 2 item 3 -- two DIFFERENT triggers, two
    // different callbacks). Pure: the service asks it what to do and does it.
    //  1. The app goes to the background (a call, another app, the lock screen): OnApplicationPause(true). The narration is paused
    //     explicitly, and resumes at the SAME position when the app returns -- but only if it was playing: a narration the visitor had
    //     paused stays paused.
    //  2. The audio OUTPUT changes (earbuds connect or disconnect mid-narration): AudioSettings.OnAudioConfigurationChanged with
    //     deviceWasChanged = true. The engine re-initialises and forgets what it was playing: the clip is loaded again, put back at
    //     the position it had and started again if it was playing.
    // A device change while the app is in the background is handled when the app returns (resume reloads too), never twice.
    public sealed class AudioInterruptionRule
    {
        public enum Step
        {
            None,
            // Pause the output now (the app went to the background while playing)
            Pause,
            // Start the output again at Position (the app came back and had been playing)
            Resume,
            // Load the clip again, seek to Position, and play it if ResumePlaying (the output device changed)
            Reload,
        }

        public readonly struct Result
        {
            public readonly Step Step;
            public readonly float Position;
            public readonly bool ResumePlaying;

            public Result(Step step, float position, bool resumePlaying)
            {
                Step = step;
                Position = position;
                ResumePlaying = resumePlaying;
            }
        }

        private bool _heldByApp;
        private float _heldPosition;
        private bool _deviceChangedWhileAway;

        // Whether the narration is paused because the app is in the background
        public bool HeldByApp => _heldByApp;

        // The app's focus changed (`paused` = went to the background). `playing`: the narration is playing now; `position`: where it is
        public Result AppPaused(bool paused, bool playing, float position)
        {
            if (paused)
            {
                if (!playing || _heldByApp) return default;
                _heldByApp = true;
                _heldPosition = position;
                return new Result(Step.Pause, position, false);
            }
            if (!_heldByApp) return default;
            _heldByApp = false;
            // - the output changed while we were away: the engine was rebuilt, so resume means load again, not just play
            bool reload = _deviceChangedWhileAway;
            _deviceChangedWhileAway = false;
            return new Result(reload ? Step.Reload : Step.Resume, _heldPosition, true);
        }

        // AudioSettings.OnAudioConfigurationChanged: `deviceWasChanged` false is a settings change that keeps playing (nothing to do)
        public Result DeviceChanged(bool deviceWasChanged, bool playing, float position)
        {
            if (!deviceWasChanged) return default;
            if (_heldByApp)
            {
                _deviceChangedWhileAway = true;
                return default;
            }
            return new Result(Step.Reload, position, playing);
        }

        // The visitor's own action (a new audio, a stop) ends any interruption bookkeeping
        public void Forget()
        {
            _heldByApp = false;
            _deviceChangedWhileAway = false;
        }
    }
}

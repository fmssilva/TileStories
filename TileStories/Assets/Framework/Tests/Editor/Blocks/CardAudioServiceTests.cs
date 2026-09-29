using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace TileStories.Editor.Tests
{
    // The card's ONE audio owner (_3.1 step 9A) on REAL clips: the LivingRoom fixture's 20 s tone and its two long narrations are loaded
    // through the real ResourcesMediaSource, so a clip really is held and given back. Only the sound DEVICE is a stand-in (a silent
    // ManualAudioOutput whose time moves when the test says) and the clock of a fade is a number: no audio time here is ever a wait.
    public class CardAudioServiceTests
    {
        private const string Root = "LivingRoom/CardMedia";
        private const string TonePath = "audio/lamp_tone.wav";
        private const string GuidePath = "audio/castelo_s_jorge_guide_pt.mp3";

        private ManualAudioOutput _out;
        private ResourcesMediaSource _media;
        private CardAudioService _svc;
        private float _now;
        private string _mode;

        [SetUp]
        public void Build()
        {
            _out = new ManualAudioOutput();
            _media = new ResourcesMediaSource(Root);
            _now = 0f;
            _mode = CardOptions.AudioSwitch;
            _svc = new CardAudioService(_out, () => _media, () => _now, () => _mode);
        }

        private static AudioTrack Tone(string poi = "a") => new(poi, TonePath, "Tone");
        private static AudioTrack Guide(string poi = "b") => new(poi, GuidePath, "Guide");

        // Audio time passes: the device's clip moves, the fade's clock too, and the service takes its step (like one frame)
        private void Advance(float seconds)
        {
            _now += seconds;
            _out.Advance(seconds);
            _svc.Tick();
        }

        // A fade's clock moves with the device silent (the old audio is being faded out, its time does not matter)
        private void FadeStep(float seconds)
        {
            _now += seconds;
            _svc.Tick();
        }

        [Test]
        public void Toggle_StartsTheClip_HoldsItThroughTheSource_AndTheEndGivesItBack()
        {
            Assert.AreEqual(CardAudioState.Idle, _svc.State);
            _svc.Toggle(Tone());
            Assert.AreEqual(CardAudioState.Playing, _svc.State);
            Assert.IsTrue(_svc.IsCurrent(Tone()), "identity is the point and the clip path");
            Assert.IsTrue(_out.IsPlaying);
            Assert.AreEqual("lamp_tone", _out.Clip.name, "the real fixture clip");
            Assert.AreEqual(20f, _svc.Length, 0.05f, "the generated tone is 20 s");
            Assert.AreEqual(1, _media.RefCount(TonePath), "the service holds the clip through the card's media source");
            Advance(5f);
            Assert.AreEqual(5f, _svc.Position, 0.01f);

            // - the clip runs out (frames of half a second): the audio ends, the clip is given back, nothing plays on
            int steps = 0;
            while (_svc.State != CardAudioState.Idle && steps++ < 60) Advance(0.5f);
            Assert.AreEqual(CardAudioState.Idle, _svc.State, "it ended");
            Assert.Less(steps, 40, "at the clip's own end, not later");
            Assert.IsNull(_svc.Current);
            Assert.AreEqual(0, _media.RefCount(TonePath));
            Assert.AreEqual(0, _media.LoadedCount, "the ended clip is unloaded");
            Assert.AreEqual(0f, _svc.Position);
        }

        [Test]
        public void PauseKeepsThePosition_ResumeContinues_ASecondToggleOfTheSameAudioPausesAndResumes()
        {
            _svc.Toggle(Tone());
            Advance(5f);
            _svc.Pause();
            Assert.AreEqual(CardAudioState.Paused, _svc.State);
            Assert.IsFalse(_out.IsPlaying);
            Advance(3f);
            Assert.AreEqual(5f, _svc.Position, 0.01f, "paused: the time stands still");
            Assert.AreEqual(CardAudioState.Paused, _svc.State, "a paused clip is not an ended one");
            _svc.Resume();
            Advance(2f);
            Assert.AreEqual(7f, _svc.Position, 0.01f);

            // - the same audio asked for by another block (a different object, a different title): the same audio, so it toggles
            _svc.Toggle(new AudioTrack("a", TonePath, "The chip's title"));
            Assert.AreEqual(CardAudioState.Paused, _svc.State, "a playing audio asked again pauses");
            _svc.Toggle(Tone());
            Assert.AreEqual(CardAudioState.Playing, _svc.State, "and asked once more resumes");
            Assert.AreEqual(1, _out.LoadCount, "one load for all of it: never a second copy of the same audio");
        }

        [Test]
        public void Seek_MovesPlayingOrPausedAudio_ClampsToTheClip_AndNeedsSomethingLoaded()
        {
            Assert.DoesNotThrow(() => _svc.Seek(5f), "nothing loaded: nothing to do");
            _svc.Toggle(Tone());
            _svc.Seek(10f);
            Assert.AreEqual(10f, _svc.Position, 0.01f);
            Advance(1f);
            Assert.AreEqual(11f, _svc.Position, 0.01f, "it plays on from there");
            _svc.Seek(999f);
            Assert.Less(_svc.Position, 20f);
            Assert.Greater(_svc.Position, 19.8f, "clamped to the very end of the clip, never past it");
            _svc.Seek(-5f);
            Assert.AreEqual(0f, _svc.Position, 0.01f);
            _svc.Pause();
            _svc.Seek(3f);
            Assert.AreEqual(3f, _svc.Position, 0.01f, "seeking a paused clip");
            _svc.Resume();
            Advance(1f);
            Assert.AreEqual(4f, _svc.Position, 0.01f, "and it resumes from the sought place");
        }

        [Test]
        public void UpdateTitle_RenamesTheCurrentAudioOnly_WithoutRestartingIt()
        {
            _svc.Toggle(new AudioTrack("a", TonePath, "Old title"));
            Advance(4f);
            int changes = 0;
            _svc.Changed += () => changes++;
            _svc.UpdateTitle(new AudioTrack("a", TonePath, "Old title"));
            Assert.AreEqual(0, changes, "the same words: nothing changed");
            _svc.UpdateTitle(new AudioTrack("a", TonePath, "New title"));
            Assert.AreEqual("New title", _svc.Current.Title, "a renamed block is still the same audio, with its new words");
            Assert.AreEqual(1, changes, "the players are told");
            Assert.AreEqual(4f, _svc.Position, 0.01f, "the sound went on");
            Assert.AreEqual(1, _out.LoadCount, "and was not loaded again");
            _svc.UpdateTitle(new AudioTrack("b", TonePath, "Another point's"));
            _svc.UpdateTitle(new AudioTrack("a", GuidePath, "Another clip"));
            Assert.AreEqual("New title", _svc.Current.Title, "another audio's words are not the current one's");
        }

        [Test]
        public void Speed_MovesTheClipFaster_AndStaysForTheNextAudio()
        {
            _svc.Toggle(Tone());
            _svc.SetSpeed(2f);
            Assert.AreEqual(2f, _out.Speed);
            Advance(3f);
            Assert.AreEqual(6f, _svc.Position, 0.01f, "three seconds at double speed");
            _svc.SetSpeed(0f);
            Assert.AreEqual(2f, _svc.Speed, "a speed of zero is refused");
            _svc.Stop();
            _svc.Toggle(Tone());
            Assert.AreEqual(2f, _out.Speed, "the chosen speed is the visitor's, not the clip's");
        }

        [Test]
        public void ANewAudio_WhileOneIsPlaying_InSwitchMode_FadesTheOldOneOutInHalfASecondThenStartsTheNewOne()
        {
            _mode = CardOptions.AudioSwitch;
            _svc.Toggle(Tone("a"));
            Advance(2f);
            _svc.Toggle(Guide("b"));
            Assert.IsTrue(_svc.IsCurrent(Guide()), "the new audio is the current one at once (its block draws playing)");
            Assert.AreEqual(CardAudioState.Playing, _svc.State);
            Assert.AreEqual(0f, _svc.Position);
            Assert.AreEqual(0f, _svc.Length, "nothing of the new clip is loaded while the old one fades");
            Assert.AreEqual("lamp_tone", _out.Clip.name, "the old sound is still what the device plays");
            Assert.IsTrue(_out.IsPlaying);
            Assert.AreEqual(1, _media.RefCount(TonePath), "the old clip is held until it is silent");
            Assert.AreEqual(0, _media.RefCount(GuidePath));

            FadeStep(0.25f);
            Assert.AreEqual(0.5f, _out.Volume, 0.001f, "halfway through the fade");
            Assert.AreEqual("lamp_tone", _out.Clip.name);

            FadeStep(0.25f);
            Assert.AreEqual("castelo_s_jorge_guide_pt", _out.Clip.name, "the fade is over: the new clip is what the device plays");
            Assert.IsTrue(_out.IsPlaying);
            Assert.AreEqual(1f, _out.Volume, "back at full for the new audio");
            Assert.AreEqual(0, _media.RefCount(TonePath), "the old clip was given back");
            Assert.AreEqual(1, _media.RefCount(GuidePath));
            Assert.AreEqual(200.39f, _svc.Length, 0.1f);
            Assert.AreEqual(0f, _svc.Position, 0.01f);
        }

        [Test]
        public void ANewAudio_WhileOneIsPlaying_InQueueMode_WaitsItsTurn_AndStartsWhenTheFirstEnds()
        {
            _mode = CardOptions.AudioQueue;
            _svc.Toggle(Tone("a"));
            _svc.Toggle(Guide("b"));
            Assert.IsTrue(_svc.IsCurrent(Tone("a")), "the playing audio is not interrupted");
            Assert.IsTrue(_svc.IsQueued(Guide("b")));
            Assert.AreEqual(1, _svc.Queue.Count);
            Assert.AreEqual(0, _media.RefCount(GuidePath), "a queued clip is not loaded yet");

            _svc.Toggle(Guide("b"));
            Assert.IsFalse(_svc.IsQueued(Guide("b")), "asking for a queued audio again takes it out");
            _svc.Toggle(Guide("b"));
            Assert.IsTrue(_svc.IsQueued(Guide("b")));

            int steps = 0;
            while (_svc.IsCurrent(Tone("a")) && steps++ < 80) Advance(0.5f);
            Assert.IsTrue(_svc.IsCurrent(Guide("b")), "the first ended: the queued one is the current audio");
            Assert.AreEqual(CardAudioState.Playing, _svc.State);
            Assert.AreEqual("castelo_s_jorge_guide_pt", _out.Clip.name);
            Assert.AreEqual(0, _svc.Queue.Count);
            Assert.AreEqual(0, _media.RefCount(TonePath), "the ended clip was given back");
            Assert.AreEqual(1, _media.RefCount(GuidePath));
        }

        [Test]
        public void ANewAudio_WhileTheOldOneIsPausedOnly_StartsAtOnce_NoFadeNoQueue()
        {
            _mode = CardOptions.AudioQueue;
            _svc.Toggle(Tone("a"));
            Advance(1f);
            _svc.Pause();
            _svc.Toggle(Guide("b"));
            Assert.IsTrue(_svc.IsCurrent(Guide("b")));
            Assert.AreEqual(CardAudioState.Playing, _svc.State);
            Assert.AreEqual("castelo_s_jorge_guide_pt", _out.Clip.name);
            Assert.AreEqual(0, _svc.Queue.Count, "a paused audio the visitor moved on from never blocks a new one");
            Assert.AreEqual(0, _media.RefCount(TonePath));
        }

        [Test]
        public void Stop_EndsTheAudio_ClearsTheQueue_AndGivesTheClipsBack()
        {
            _mode = CardOptions.AudioQueue;
            _svc.Toggle(Tone("a"));
            _svc.Toggle(Guide("b"));
            _svc.Stop();
            Assert.AreEqual(CardAudioState.Idle, _svc.State);
            Assert.IsNull(_svc.Current);
            Assert.AreEqual(0, _svc.Queue.Count);
            Assert.IsFalse(_out.IsPlaying);
            Assert.AreEqual(0, _media.LoadedCount);
            Assert.DoesNotThrow(() => _svc.Stop(), "stopping nothing is fine");
        }

        [Test]
        public void TheAppInTheBackground_PausesPlayingAudio_ResumesAtTheSamePlace_ButNeverWakesAPausedOne()
        {
            _svc.Toggle(Tone());
            Advance(6f);
            _svc.OnApplicationPause(true);
            Assert.IsFalse(_out.IsPlaying, "the phone call: the sound stops");
            Assert.AreEqual(CardAudioState.Playing, _svc.State, "...but the narration is not ended or forgotten");
            _svc.Tick();
            Assert.AreEqual(CardAudioState.Playing, _svc.State, "a held narration is not mistaken for one that ran out");
            Assert.AreEqual(6f, _svc.Position, 0.01f);
            _svc.OnApplicationPause(false);
            Assert.IsTrue(_out.IsPlaying, "back in the app: it goes on");
            Assert.AreEqual(6f, _out.Time, 0.01f, "from the same place");

            _svc.Pause();
            _svc.OnApplicationPause(true);
            _svc.OnApplicationPause(false);
            Assert.IsFalse(_out.IsPlaying, "a narration the visitor paused stays paused");
            Assert.AreEqual(CardAudioState.Paused, _svc.State);
        }

        [Test]
        public void TheOutputDeviceChanging_ReloadsTheClipAtThePosition_ASettingsChangeDoesNothing()
        {
            _svc.Toggle(Tone());
            Advance(6f);
            int loads = _out.LoadCount;
            _svc.OnAudioConfigurationChanged(false);
            Assert.AreEqual(loads, _out.LoadCount, "deviceWasChanged false: nothing to reload");

            _out.DropPlayback();
            _svc.OnAudioConfigurationChanged(true);
            Assert.AreEqual(loads + 1, _out.LoadCount, "the engine forgot the clip: it is loaded again");
            Assert.IsTrue(_out.IsPlaying, "and started again");
            Assert.AreEqual(6f, _out.Time, 0.01f, "at the position it had");
            Assert.AreEqual(1, _media.RefCount(TonePath), "still one hold on the clip");
        }

        [Test]
        public void AClipTheEngineLostWithNoCallback_IsLoadedAgainAtItsPosition_ThreeTimes_ThenHeldPausedNotLoopedForever()
        {
            _svc.Toggle(Tone());
            Advance(7f);
            int loads = _out.LoadCount;
            for (int attempt = 1; attempt <= 3; attempt++)
            {
                _out.DropPlayback();
                _svc.Tick();
                Assert.IsTrue(_out.IsPlaying, "restart " + attempt);
                Assert.AreEqual(7f, _out.Time, 0.01f, "at the position it was lost at");
            }
            Assert.AreEqual(loads + 3, _out.LoadCount);
            _out.DropPlayback();
            _svc.Tick();
            Assert.AreEqual(CardAudioState.Paused, _svc.State, "the fourth loss with no progress: the service gives up, visibly");
            Assert.IsFalse(_out.IsPlaying);
            _svc.Resume();
            Assert.IsTrue(_out.IsPlaying, "the visitor can still press play");
        }

        [Test]
        public void AClipThatStopsAtItsEnd_IsAnEndedAudio_NotALostOne()
        {
            _svc.Toggle(Tone());
            for (int i = 0; i < 39; i++) Advance(0.5f);
            Assert.AreEqual(19.5f, _svc.Position, 0.01f);
            _out.DropPlayback();
            _svc.Tick();
            Assert.AreEqual(CardAudioState.Idle, _svc.State, "half a second from the end: it ended, it is not restarted");
            Assert.AreEqual(1, _out.LoadCount);
        }

        [Test]
        public void AClipThatIsNotThere_LeavesTheServiceIdle_WithOneWarning_AndNeverAnException()
        {
            LogAssert.Expect(LogType.Warning, new Regex(@"\[Card\] media not found: Resources/LivingRoom/CardMedia/audio/ghost"));
            Assert.DoesNotThrow(() => _svc.Toggle(new AudioTrack("a", "audio/ghost.wav", "Ghost")));
            Assert.AreEqual(CardAudioState.Idle, _svc.State);
            Assert.IsNull(_svc.Current);
            Assert.AreEqual(0, _out.PlayCount, "nothing was played");
            Assert.DoesNotThrow(() => _svc.Toggle(new AudioTrack("a", "", "Empty")), "an empty path is simply nothing");
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void Changed_IsRaisedForEveryChangeAPlayerDraws_AndEveryTickWhilePlaying()
        {
            int changes = 0;
            _svc.Changed += () => changes++;
            _svc.Toggle(Tone());
            int afterStart = changes;
            Assert.Greater(afterStart, 0, "starting");
            Advance(1f);
            Assert.Greater(changes, afterStart, "a tick while playing: the progress");
            int afterTick = changes;
            _svc.Seek(3f);
            _svc.SetSpeed(1.5f);
            _svc.Pause();
            Assert.GreaterOrEqual(changes, afterTick + 3, "a seek, a speed, a pause");
            int paused = changes;
            _svc.Tick();
            Assert.AreEqual(paused, changes, "a paused audio has no progress to announce");
        }
    }
}

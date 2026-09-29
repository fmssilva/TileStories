using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // The card's video rules and owner (_3.1 step 9B), no scene: TimeCodeRule, ChapterRule, CardSoundRule, and the REAL CardVideoService and
    // CardAudioService joined by the REAL CardSoundCoordinator, over the manual devices (video time and audio time are numbers the test moves)
    // and the Phase A gallery's real generated clips and silent audio (CardGalleryMedia).
    public class CardVideoRulesTests
    {
        private const string Poi = "poi_1";
        private CardGalleryMedia _media;
        private ManualVideoOutput _videoOut;
        private CardVideoService _video;
        private ManualAudioOutput _audioOut;
        private CardAudioService _audio;
        private CardSoundCoordinator _sound;
        private int _changes;

        private static VideoTrack Visitor(string clip = "short.mp4", string poi = Poi) => new VideoTrack(poi, clip, "The castle film");
        private static VideoTrack Loop(string poi = Poi) => new VideoTrack(poi, "long.mp4", "");

        [SetUp]
        public void SetUp()
        {
            _media = new CardGalleryMedia();
            _videoOut = new ManualVideoOutput();
            _video = new CardVideoService(_videoOut, () => _media);
            _audioOut = new ManualAudioOutput();
            _audio = new CardAudioService(_audioOut, () => _media, () => 0f, () => CardOptions.AudioSwitch);
            _sound = new CardSoundCoordinator(_audio, _video);
            _changes = 0;
            _video.Changed += () => _changes++;
        }

        [TearDown]
        public void TearDown()
        {
            _sound.Dispose();
            _video.CardClosed();
            _audio.Stop();
            _videoOut.Release();
        }

        private void AdvanceVideo(float seconds)
        {
            _videoOut.Advance(seconds);
            _video.Tick();
        }

        // ---------------- pure rules ----------------

        [Test]
        public void TimeCodeRule_ReadsSecondsMinutesAndHours_AndNothingElse()
        {
            var good = new Dictionary<string, float>
            {
                ["90"] = 90f, ["12.5"] = 12.5f, ["0"] = 0f, ["1:30"] = 90f, ["01:30"] = 90f, [" 2:05 "] = 125f, ["1:02:03"] = 3723f, ["0:59.5"] = 59.5f,
            };
            foreach (var pair in good)
            {
                Assert.IsTrue(TimeCodeRule.TryParse(pair.Key, out float s), "'" + pair.Key + "' is a time");
                Assert.AreEqual(pair.Value, s, 0.0001f, pair.Key);
            }
            foreach (string bad in new[] { "", "  ", null, "1:75", "1:2:60", "1.5:30", "-5", "+5", "1:", ":30", "1:2:3:4", "abc", "1,5", "1:30s" })
                Assert.IsFalse(TimeCodeRule.TryParse(bad, out _), "'" + bad + "' is not a time");
        }

        [Test]
        public void ChapterRule_KeepsRowsWithALabelAndATimeInsideTheClip_InTimeOrder_AndLightsTheOneAtATime()
        {
            var rows = new List<(string, string)>
            {
                ("1:30", "Third"), ("0:00", "First"), ("0:45", "Second"), ("1:30", "Same time as the third"),
                ("bad", "Unreadable time"), ("0:20", ""), ("5:00", "Past the end"),
            };
            var chapters = ChapterRule.From(rows, 120f);
            CollectionAssert.AreEqual(new[] { "First", "Second", "Third" }, chapters.Select(c => c.Label).ToList());
            CollectionAssert.AreEqual(new[] { 1, 2, 0 }, chapters.Select(c => c.Row).ToList(), "each chip remembers its written row");
            Assert.AreEqual(-1, ChapterRule.IndexAt(ChapterRule.From(new List<(string, string)> { ("0:10", "Later") }, 0f), 5f), "before the first chapter: none");
            Assert.AreEqual(0, ChapterRule.IndexAt(chapters, 0f));
            Assert.AreEqual(0, ChapterRule.IndexAt(chapters, 44.9f));
            Assert.AreEqual(1, ChapterRule.IndexAt(chapters, 45f), "a chapter lights from its own start");
            Assert.AreEqual(2, ChapterRule.IndexAt(chapters, 119f));
            Assert.AreEqual(1, ChapterRule.From(new List<(string, string)> { ("5:00", "Anywhere") }, 0f).Count, "an unknown length keeps every row");
        }

        [Test]
        public void CardSoundRule_OnlyWhatJustStartedWins_TheVideoOnATie_AndNothingPausesWhatIsAlreadyQuiet()
        {
            Assert.AreEqual(CardSoundRule.Step.PauseAudio, CardSoundRule.Decide(true, true, false, true), "a video starts over playing audio");
            Assert.AreEqual(CardSoundRule.Step.PauseVideo, CardSoundRule.Decide(false, true, true, true), "audio starts over a playing video");
            Assert.AreEqual(CardSoundRule.Step.PauseAudio, CardSoundRule.Decide(false, true, false, true), "both in one step: the video wins");
            Assert.AreEqual(CardSoundRule.Step.None, CardSoundRule.Decide(false, false, false, true), "a video alone");
            Assert.AreEqual(CardSoundRule.Step.None, CardSoundRule.Decide(false, true, false, false), "audio alone");
            Assert.AreEqual(CardSoundRule.Step.None, CardSoundRule.Decide(true, true, true, true), "nothing new started: nothing decided");
            Assert.AreEqual(CardSoundRule.Step.None, CardSoundRule.Decide(true, false, false, true), "the audio stopped as the video started: nothing to pause");
        }

        // ---------------- the owner ----------------

        [Test]
        public void AVideo_PlaysFromItsClip_ShowsNoFrameUntilTheFirstStep_PausesResumesAndSeeks_AndGivesTheClipBackAtItsEnd()
        {
            var track = Visitor();
            _video.Toggle(track);
            Assert.AreEqual(CardVideoState.Playing, _video.State);
            Assert.IsTrue(_video.IsCurrent(track));
            Assert.IsFalse(_video.PlayingLoop);
            Assert.AreEqual(12f, _video.Length, 0.1f, "the generated clip's real length");
            Assert.IsFalse(_videoOut.Muted, "the visitor's video has its sound");
            Assert.IsFalse(_videoOut.Looping);
            Assert.IsFalse(_video.HasFrame, "still preparing: the poster shows");
            Assert.AreEqual(1, _media.RefCount("short.mp4"), "the clip is held while it plays");
            AdvanceVideo(2.5f);
            Assert.IsTrue(_video.HasFrame, "the first frame arrived");
            Assert.AreEqual(2.5f, _video.Position, 0.001f);

            _video.Toggle(track);
            Assert.AreEqual(CardVideoState.Paused, _video.State, "the same button pauses");
            AdvanceVideo(3f);
            Assert.AreEqual(2.5f, _video.Position, 0.001f, "a paused video keeps its place");
            _video.Seek(8f);
            Assert.AreEqual(8f, _video.Position, 0.001f, "a seek while paused");
            _video.Toggle(track);
            Assert.AreEqual(CardVideoState.Playing, _video.State, "and resumes");
            _video.Seek(500f);
            Assert.AreEqual(11.95f, _video.Position, 0.01f, "a seek is kept inside the clip");
            _video.Seek(-3f);
            Assert.AreEqual(0f, _video.Position, 0.001f);

            AdvanceVideo(13f);
            Assert.AreEqual(CardVideoState.Idle, _video.State, "the clip ran out");
            Assert.IsNull(_video.Current);
            Assert.AreEqual(0, _media.HeldCount, "and was given back");
            Assert.Greater(_changes, 5, "every step told the views");
        }

        [Test]
        public void AHeadersLoop_PlaysMutedAndLooping_GivesWayToTheVisitorsVideo_AndComesBackWhenItStops()
        {
            var loop = Loop();
            _video.PlayLoop(loop);
            Assert.IsTrue(_video.PlayingLoop, "nothing else holds the player: the loop plays");
            Assert.IsTrue(_videoOut.Muted, "muted");
            Assert.IsTrue(_videoOut.Looping, "and looping");
            AdvanceVideo(100f);
            Assert.AreEqual(CardVideoState.Playing, _video.State, "95 s of clip later it is still playing: it looped");
            Assert.AreEqual(5f, _video.Position, 0.01f);
            _video.Seek(40f);
            Assert.AreEqual(5f, _video.Position, 0.01f, "a loop is not the visitor's to move");

            var film = Visitor();
            _video.Toggle(film);
            Assert.IsTrue(_video.IsCurrent(film), "the visitor's video takes the ONE player");
            Assert.IsFalse(_video.PlayingLoop);
            Assert.IsFalse(_videoOut.Muted, "with its sound");
            Assert.AreEqual(0, _media.RefCount("long.mp4"), "the loop's clip was given back");
            _video.Stop();
            Assert.IsTrue(_video.PlayingLoop, "the visitor's video ended: the header's loop comes back");
            Assert.IsTrue(_videoOut.Muted);

            _video.StopLoop(Loop("another_poi"));
            Assert.IsTrue(_video.PlayingLoop, "another header's StopLoop does not touch this one");
            _video.StopLoop(loop);
            Assert.AreEqual(CardVideoState.Idle, _video.State, "its own header went away");
            Assert.AreEqual(0, _media.HeldCount);
        }

        [Test]
        public void ANewCard_StopsTheVideoOfAnotherPoint_KeepsItsOwn_AndClosingTheCardStopsEverything()
        {
            var film = Visitor();
            _video.Toggle(film);
            _video.CardShown(Poi);
            Assert.AreEqual(CardVideoState.Playing, _video.State, "the same point's card again (a live edit): it plays on");
            _video.CardShown("poi_2");
            Assert.AreEqual(CardVideoState.Idle, _video.State, "another point's card: the video belonged to the old card");

            _video.PlayLoop(Loop("poi_2"));
            _video.CardShown("poi_2");
            Assert.IsTrue(_video.PlayingLoop, "a new card never stops its own header's loop");
            _video.CardClosed();
            Assert.AreEqual(CardVideoState.Idle, _video.State, "closed: nothing plays with no card to show it");
            Assert.AreEqual(0, _media.HeldCount);
            _video.Stop();
            Assert.AreEqual(CardVideoState.Idle, _video.State, "and the loop request went with the card: nothing comes back");
        }

        [Test]
        public void TheAppInTheBackground_HoldsTheVideo_AndResumesItAtTheSamePlace_AVisitorsPauseStaysAPause()
        {
            _video.Toggle(Visitor());
            AdvanceVideo(4f);
            _video.OnApplicationPause(true);
            Assert.IsFalse(_videoOut.IsPlaying, "held while the app is away");
            AdvanceVideo(3f);
            Assert.AreEqual(4f, _video.Position, 0.001f);
            _video.OnApplicationPause(false);
            Assert.IsTrue(_videoOut.IsPlaying, "back: it plays on");
            Assert.AreEqual(4f, _video.Position, 0.001f, "from the same place");

            _video.Pause();
            _video.OnApplicationPause(true);
            _video.OnApplicationPause(false);
            Assert.AreEqual(CardVideoState.Paused, _video.State, "a video the visitor paused is not started by the app coming back");
        }

        [Test]
        public void AClipThatCannotBeLoaded_LeavesThePlayerIdle_AndTheLoopKeepsTheScreen()
        {
            _video.PlayLoop(Loop());
            _video.Toggle(Visitor("ghost.mp4"));
            Assert.IsTrue(_video.PlayingLoop, "the missing video never took the player from the loop for good");
            _video.CardClosed();
            _video.Toggle(Visitor("ghost.mp4"));
            Assert.AreEqual(CardVideoState.Idle, _video.State);
            Assert.AreEqual(0, _media.HeldCount);
        }

        // ---------------- the two owners together ----------------

        [Test]
        public void StartingTheVideo_PausesTheCardsAudio_StartingTheAudio_PausesTheVideo_AndAMutedLoopPausesNothing()
        {
            var guide = new AudioTrack(Poi, "short.mp3", "Guide");
            _audio.Toggle(guide);
            Assert.AreEqual(CardAudioState.Playing, _audio.State, "precondition: the audio guide plays");

            _video.PlayLoop(Loop());
            Assert.AreEqual(CardAudioState.Playing, _audio.State, "a header's muted loop never pauses the audio");

            _video.Toggle(Visitor());
            Assert.AreEqual(CardVideoState.Playing, _video.State);
            Assert.AreEqual(CardAudioState.Paused, _audio.State, "the video started: the audio paused (not stopped)");
            Assert.IsTrue(_audio.IsCurrent(guide), "it keeps its place for later");

            _audio.Toggle(guide);
            Assert.AreEqual(CardAudioState.Playing, _audio.State, "the visitor resumes the audio");
            Assert.AreEqual(CardVideoState.Paused, _video.State, "the audio started: the video paused");

            _video.Toggle(Visitor());
            Assert.AreEqual(CardVideoState.Playing, _video.State);
            Assert.AreEqual(CardAudioState.Paused, _audio.State, "and back again: never both at once");
        }
    }
}

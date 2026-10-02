using System.Globalization;
using System.Linq;
using NUnit.Framework;

namespace TileStories.Editor.Tests
{
    // The pure rules of the audio guide (_3.1 step 9A): captions (VttRule), the speed list, how a time is written and placed, what
    // starting an audio does while another plays, the fade, the interruptions of the work plan's Stage 2 item 3, the Android output
    // watcher and the mini-player's visibility. Every case is a plain `new`-free static call: no scene, no clip, no clock.
    public class AudioRulesTests
    {
        // ---------------- VttRule ----------------

        [Test]
        public void Vtt_TheGalleryFile_ParsesItsThreeCues_WithTheirTimesAndWords()
        {
            var cues = VttRule.Parse(CardGalleryDefinitions.Captions["short.vtt"]);
            Assert.AreEqual(3, cues.Count);
            Assert.AreEqual(0f, cues[0].Start);
            Assert.AreEqual(4f, cues[0].End);
            Assert.AreEqual(CardGalleryDefinitions.ShortCaptionOne, cues[0].Text);
            Assert.AreEqual(CardGalleryDefinitions.ShortCaptionTwo, cues[1].Text);
            Assert.AreEqual(8.5f, cues[2].Start);
            Assert.AreEqual(12f, cues[2].End);
        }

        [Test]
        public void Vtt_AFileSavedWithAByteOrderMark_ParsesExactlyLikeOneWithout()
        {
            // - many editors save .vtt as UTF-8 with a BOM: the mark sits before "WEBVTT" and must not cost a cue
            string plain = CardGalleryDefinitions.Captions["short.vtt"];
            var expected = VttRule.Parse(plain);
            var withMark = VttRule.Parse((char)0xFEFF + plain);
            Assert.AreEqual(expected.Count, withMark.Count, "precondition: the gallery file has cues, and the marked copy as many");
            Assert.Greater(expected.Count, 0);
            for (int i = 0; i < expected.Count; i++)
            {
                Assert.AreEqual(expected[i].Start, withMark[i].Start, "cue " + i + " start");
                Assert.AreEqual(expected[i].End, withMark[i].End, "cue " + i + " end");
                Assert.AreEqual(expected[i].Text, withMark[i].Text, "cue " + i + " words");
            }
        }

        [Test]
        public void Vtt_TextAt_StartIsInclusive_EndIsExclusive_AndAGapIsEmpty()
        {
            var cues = VttRule.Parse(CardGalleryDefinitions.Captions["short.vtt"]);
            Assert.AreEqual(CardGalleryDefinitions.ShortCaptionOne, VttRule.TextAt(cues, 0f), "the first instant");
            Assert.AreEqual(CardGalleryDefinitions.ShortCaptionOne, VttRule.TextAt(cues, 3.99f));
            Assert.AreEqual(CardGalleryDefinitions.ShortCaptionTwo, VttRule.TextAt(cues, 4f), "the end of one is the start of the next");
            Assert.AreEqual("", VttRule.TextAt(cues, 8.2f), "between the second and the third: no caption");
            Assert.AreEqual(CardGalleryDefinitions.ShortCaptionThree, VttRule.TextAt(cues, 8.5f));
            Assert.AreEqual("", VttRule.TextAt(cues, 12f), "the last cue's end is exclusive");
            Assert.AreEqual("", VttRule.TextAt(cues, -1f));
            Assert.AreEqual(-1, VttRule.IndexAt(cues, 100f));
            Assert.AreEqual("", VttRule.TextAt(null, 1f), "no cues: nothing");
        }

        [Test]
        public void Vtt_ReadsHoursCommasSettingsTagsEntitiesAndSeveralLines()
        {
            const string text = "WEBVTT\n\n" +
                                "01:02:03.500 --> 01:02:05.000 align:start position:10%\n<v Ana>Hello <i>there</i> &amp; welcome</v>\n\n" +
                                "00:01,500 --> 00:02,000\nfirst line\nsecond line\n";
            var cues = VttRule.Parse(text);
            Assert.AreEqual(2, cues.Count);
            // - sorted by start: the short one (1.5 s) comes first
            Assert.AreEqual(1.5f, cues[0].Start, 0.001f, "a comma for the dot, minutes and seconds only");
            Assert.AreEqual("first line second line", cues[0].Text, "the lines of a cue joined with a space");
            Assert.AreEqual(3723.5f, cues[1].Start, 0.001f, "hours");
            Assert.AreEqual(3725f, cues[1].End, 0.001f, "the settings after the end time are ignored");
            Assert.AreEqual("Hello there & welcome", cues[1].Text, "tags dropped, the entity decoded");
        }

        [Test]
        public void Vtt_SkipsCommentsBrokenCuesAndBackwardsTimes_AndKeepsTheRest_NeverThrows()
        {
            const string text = "﻿WEBVTT - a title\r\n\r\nNOTE this is a comment\r\n\r\nSTYLE\r\n::cue { color: red }\r\n\r\n" +
                                "good\r\n00:00.000 --> 00:02.000\r\nkept\r\n\r\n" +
                                "00:xx.000 --> 00:04.000\r\nbroken time\r\n\r\n" +
                                "00:06.000 --> 00:05.000\r\nbackwards\r\n\r\n" +
                                "just an id and words\r\n\r\n" +
                                "00:08.000 --> 00:09.000\r\nalso kept\r\n";
            var cues = VttRule.Parse(text);
            CollectionAssert.AreEqual(new[] { "kept", "also kept" }, cues.Select(c => c.Text).ToArray(), "a BOM, CRLF, comments, an id, a bad cue: only the two good cues remain");
        }

        [Test]
        public void Vtt_TextThatIsNotWebVtt_OrIsEmpty_HasNoCues_OutOfOrderCuesAreSorted_OverlapsShowTheLatest()
        {
            Assert.AreEqual(0, VttRule.Parse("00:00.000 --> 00:01.000\nno header").Count);
            Assert.AreEqual(0, VttRule.Parse("").Count);
            Assert.AreEqual(0, VttRule.Parse(null).Count);
            var cues = VttRule.Parse("WEBVTT\n\n00:05.000 --> 00:10.000\nlate\n\n00:00.000 --> 00:08.000\nearly, long\n");
            CollectionAssert.AreEqual(new[] { "early, long", "late" }, cues.Select(c => c.Text).ToArray(), "ordered by start");
            Assert.AreEqual("early, long", VttRule.TextAt(cues, 3f));
            Assert.AreEqual("late", VttRule.TextAt(cues, 6f), "where two overlap the one that started last is shown");
        }

        // ---------------- speed, time ----------------

        [Test]
        public void Speed_ThePresets_ListTheirSpeeds_TheDefaultIsNarration_AndOffHasNoChoice()
        {
            CollectionAssert.AreEqual(new[] { 0.75f, 1f, 1.25f, 1.5f }, AudioSpeedRule.OptionsOf(AudioSpeedRule.Narration).ToArray());
            CollectionAssert.AreEqual(new[] { 0.5f, 0.75f, 1f, 1.25f, 1.5f, 2f }, AudioSpeedRule.OptionsOf(AudioSpeedRule.Wide).ToArray());
            CollectionAssert.AreEqual(new[] { 1f }, AudioSpeedRule.OptionsOf(AudioSpeedRule.Off).ToArray());
            CollectionAssert.AreEqual(AudioSpeedRule.OptionsOf(AudioSpeedRule.Narration).ToArray(), AudioSpeedRule.OptionsOf("").ToArray(), "no preset picked");
            CollectionAssert.AreEqual(AudioSpeedRule.OptionsOf(AudioSpeedRule.Narration).ToArray(), AudioSpeedRule.OptionsOf("nonsense").ToArray(), "an unknown name");
            Assert.IsTrue(AudioSpeedRule.OffersChoice(AudioSpeedRule.Narration));
            Assert.IsFalse(AudioSpeedRule.OffersChoice(AudioSpeedRule.Off), "one speed: no chip");
            Assert.AreEqual(AudioSpeedRule.Presets.Count, AudioSpeedRule.PresetLabels.Count, "every preset has its Editor words");
            foreach (string preset in AudioSpeedRule.Presets)
                Assert.IsTrue(AudioSpeedRule.OptionsOf(preset).Contains(AudioSpeedRule.Normal), preset + " always contains normal speed");
        }

        [Test]
        public void Speed_Next_CyclesAndWraps_AUnknownSpeedGoesToNormal_AndLabelsAreInvariant()
        {
            var options = AudioSpeedRule.OptionsOf(AudioSpeedRule.Narration);
            Assert.AreEqual(1.25f, AudioSpeedRule.Next(options, 1f));
            Assert.AreEqual(1.5f, AudioSpeedRule.Next(options, 1.25f));
            Assert.AreEqual(0.75f, AudioSpeedRule.Next(options, 1.5f), "after the last: the first");
            Assert.AreEqual(1f, AudioSpeedRule.Next(options, 0.9f), "a speed the list lacks: normal");
            Assert.AreEqual(1f, AudioSpeedRule.Next(null, 2f));
            Assert.AreEqual("1x", AudioSpeedRule.Label(1f));
            Assert.AreEqual("1.25x", AudioSpeedRule.Label(1.25f));
            Assert.AreEqual("0.75x", AudioSpeedRule.Label(0.75f));
            var culture = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("pt-PT");
                Assert.AreEqual("1.25x", AudioSpeedRule.Label(1.25f), "the machine's language never turns the dot into a comma");
            }
            finally { CultureInfo.CurrentCulture = culture; }
        }

        [Test]
        public void Time_IsWrittenAsMinutesAndSeconds_HoursFromAnHour_AndBadValuesAreZero()
        {
            Assert.AreEqual("0:00", AudioTimeRule.Format(0f));
            Assert.AreEqual("0:59", AudioTimeRule.Format(59.9f), "the seconds are cut down, never rounded up to a minute");
            Assert.AreEqual("1:00", AudioTimeRule.Format(60f));
            Assert.AreEqual("3:07", AudioTimeRule.Format(187.4f));
            Assert.AreEqual("10:15", AudioTimeRule.Format(615f));
            Assert.AreEqual("1:00:00", AudioTimeRule.Format(3600f));
            Assert.AreEqual("1:02:03", AudioTimeRule.Format(3723f));
            Assert.AreEqual("0:00", AudioTimeRule.Format(-5f));
            Assert.AreEqual("0:00", AudioTimeRule.Format(float.NaN));
            Assert.AreEqual("0:00", AudioTimeRule.Format(float.PositiveInfinity));
        }

        [Test]
        public void Time_FractionAndTimeAt_AreClamped_AndZeroWithoutALength()
        {
            Assert.AreEqual(0.25f, AudioTimeRule.Fraction(5f, 20f));
            Assert.AreEqual(0f, AudioTimeRule.Fraction(-1f, 20f));
            Assert.AreEqual(1f, AudioTimeRule.Fraction(25f, 20f));
            Assert.AreEqual(0f, AudioTimeRule.Fraction(5f, 0f), "no length yet: the bar is empty");
            Assert.AreEqual(0f, AudioTimeRule.Fraction(float.NaN, 20f));
            Assert.AreEqual(10f, AudioTimeRule.TimeAt(0.5f, 20f));
            Assert.AreEqual(20f, AudioTimeRule.TimeAt(1.7f, 20f));
            Assert.AreEqual(0f, AudioTimeRule.TimeAt(-0.3f, 20f));
            Assert.AreEqual(0f, AudioTimeRule.TimeAt(0.5f, 0f));
        }

        // ---------------- switch, fade ----------------

        [Test]
        public void Switch_Decide_ANewAudioStartsAtOnceUnlessSomethingPlays_ThenTheSettingDecides()
        {
            Assert.AreEqual(AudioSwitchRule.Decision.StartNow, AudioSwitchRule.Decide(CardOptions.AudioSwitch, false));
            Assert.AreEqual(AudioSwitchRule.Decision.StartNow, AudioSwitchRule.Decide(CardOptions.AudioQueue, false), "nothing playing: never queued");
            Assert.AreEqual(AudioSwitchRule.Decision.FadeThenStart, AudioSwitchRule.Decide(CardOptions.AudioSwitch, true));
            Assert.AreEqual(AudioSwitchRule.Decision.Queue, AudioSwitchRule.Decide(CardOptions.AudioQueue, true));
            Assert.AreEqual(AudioSwitchRule.Decision.FadeThenStart, AudioSwitchRule.Decide(null, true), "no setting: the default is to switch");
            Assert.AreEqual(AudioSwitchRule.Decision.FadeThenStart, AudioSwitchRule.Decide("nonsense", true));
            CollectionAssert.AreEqual(new[] { CardOptions.AudioSwitch, CardOptions.AudioQueue }, CardOptions.AudioModes);
            Assert.AreEqual(CardOptions.AudioModes.Length, CardOptions.AudioModeLabels.Length);
            Assert.AreEqual(CardOptions.AudioSwitch, new CardContainerSettings().audio_when_another_starts, "the shipped default");
        }

        [Test]
        public void Fade_GoesStraightFromFullToSilent_InHalfASecond()
        {
            Assert.AreEqual(0.5f, AudioSwitchRule.FadeSeconds, "the work plan's 500 ms");
            Assert.AreEqual(1f, AudioSwitchRule.FadeVolume(0f));
            Assert.AreEqual(1f, AudioSwitchRule.FadeVolume(-1f));
            Assert.AreEqual(0.5f, AudioSwitchRule.FadeVolume(0.25f), 0.0001f);
            Assert.AreEqual(0f, AudioSwitchRule.FadeVolume(0.5f));
            Assert.AreEqual(0f, AudioSwitchRule.FadeVolume(3f));
            Assert.IsFalse(AudioSwitchRule.FadeFinished(0.49f));
            Assert.IsTrue(AudioSwitchRule.FadeFinished(0.5f));
        }

        // ---------------- interruptions ----------------

        [Test]
        public void Interruption_ThePhoneCall_PausesWhilePlaying_AndResumesAtTheSamePlace()
        {
            var rule = new AudioInterruptionRule();
            var pause = rule.AppPaused(true, playing: true, position: 42.5f);
            Assert.AreEqual(AudioInterruptionRule.Step.Pause, pause.Step);
            Assert.IsTrue(rule.HeldByApp);
            Assert.AreEqual(AudioInterruptionRule.Step.None, rule.AppPaused(true, true, 43f).Step, "asked twice: held once");
            var resume = rule.AppPaused(false, playing: false, position: 0f);
            Assert.AreEqual(AudioInterruptionRule.Step.Resume, resume.Step);
            Assert.AreEqual(42.5f, resume.Position, "the position it had when the call came, whatever the output says now");
            Assert.IsTrue(resume.ResumePlaying);
            Assert.IsFalse(rule.HeldByApp);
        }

        [Test]
        public void Interruption_ANarrationTheVisitorHadPaused_StaysPausedAfterTheApp_ComesBack()
        {
            var rule = new AudioInterruptionRule();
            Assert.AreEqual(AudioInterruptionRule.Step.None, rule.AppPaused(true, playing: false, position: 10f).Step, "paused already: nothing to hold");
            Assert.AreEqual(AudioInterruptionRule.Step.None, rule.AppPaused(false, playing: false, position: 10f).Step, "and nothing to resume");
            Assert.AreEqual(AudioInterruptionRule.Step.None, rule.AppPaused(false, true, 5f).Step, "coming back with nothing held does nothing");
        }

        [Test]
        public void Interruption_TheEarbudsChange_ReloadsAtThePositionAndPlaysOnlyIfItWasPlaying()
        {
            var rule = new AudioInterruptionRule();
            var playing = rule.DeviceChanged(true, playing: true, position: 12.5f);
            Assert.AreEqual(AudioInterruptionRule.Step.Reload, playing.Step);
            Assert.AreEqual(12.5f, playing.Position);
            Assert.IsTrue(playing.ResumePlaying);
            var paused = rule.DeviceChanged(true, playing: false, position: 30f);
            Assert.AreEqual(AudioInterruptionRule.Step.Reload, paused.Step, "a paused narration is loaded again too (the engine lost it)");
            Assert.IsFalse(paused.ResumePlaying, "...but stays paused");
            Assert.AreEqual(AudioInterruptionRule.Step.None, rule.DeviceChanged(false, true, 5f).Step, "a settings change that keeps the device: nothing to do");
        }

        [Test]
        public void Interruption_ADeviceChangeWhileTheAppIsAway_IsHandledOnce_WhenItReturns()
        {
            var rule = new AudioInterruptionRule();
            rule.AppPaused(true, true, 20f);
            Assert.AreEqual(AudioInterruptionRule.Step.None, rule.DeviceChanged(true, false, 0f).Step, "away: nothing to reload into yet");
            var back = rule.AppPaused(false, false, 0f);
            Assert.AreEqual(AudioInterruptionRule.Step.Reload, back.Step, "the engine was rebuilt meanwhile: coming back loads again, not just plays");
            Assert.AreEqual(20f, back.Position);
            Assert.IsTrue(back.ResumePlaying);
            Assert.AreEqual(AudioInterruptionRule.Step.None, rule.AppPaused(false, false, 0f).Step, "and only once");
        }

        [Test]
        public void Interruption_TheVisitorsOwnAction_ForgetsAHeldNarration()
        {
            var rule = new AudioInterruptionRule();
            rule.AppPaused(true, true, 8f);
            rule.Forget();
            Assert.IsFalse(rule.HeldByApp);
            Assert.AreEqual(AudioInterruptionRule.Step.None, rule.AppPaused(false, false, 0f).Step);
        }

        [Test]
        public void OutputRoute_OnlyBluetoothGoingFromOnToOff_IsADisconnect()
        {
            var watcher = new OutputRouteWatcher();
            Assert.IsFalse(watcher.Observe(false), "the first reading has nothing to compare with");
            Assert.IsFalse(watcher.Observe(true), "connecting is not a drop");
            Assert.IsFalse(watcher.Observe(true));
            Assert.IsTrue(watcher.Observe(false), "on, then off: the earbuds went");
            Assert.IsFalse(watcher.Observe(false), "still off: reported once");
            watcher.Observe(true);
            watcher.Reset();
            Assert.IsFalse(watcher.Observe(false), "after a reset the next reading starts a new comparison");
            Assert.That(OutputRouteWatcher.PollSeconds, Is.InRange(1f, 2f), "the work plan: every one to two seconds");
        }

        // ---------------- mini-player ----------------

        [Test]
        public void MiniPlayer_IsVisibleOnlyForActiveAudio_KeptOnTheWall_WithNoCardOpen()
        {
            Assert.IsTrue(MiniPlayerRule.IsVisible(true, true, false));
            Assert.IsFalse(MiniPlayerRule.IsVisible(false, true, false), "no audio");
            Assert.IsFalse(MiniPlayerRule.IsVisible(true, false, false), "Keep Audio Playing off");
            Assert.IsFalse(MiniPlayerRule.IsVisible(true, true, true), "a card is open: it holds its own player");
        }

        [Test]
        public void MiniPlayer_AudioStopsWithItsCard_OnlyWhenKeepIsOff_AndNeverForTheCardShownNow()
        {
            Assert.IsTrue(MiniPlayerRule.StopsWithCard(false, "lamp", null), "card closed, keep off");
            Assert.IsTrue(MiniPlayerRule.StopsWithCard(false, "lamp", "lamp_military"), "another point's card opens, keep off");
            Assert.IsFalse(MiniPlayerRule.StopsWithCard(false, "lamp", "lamp"), "the audio belongs to the card shown now");
            Assert.IsFalse(MiniPlayerRule.StopsWithCard(true, "lamp", null), "keep on: it plays on");
            Assert.IsFalse(MiniPlayerRule.StopsWithCard(false, null, null), "no audio: nothing to stop");
        }
    }
}

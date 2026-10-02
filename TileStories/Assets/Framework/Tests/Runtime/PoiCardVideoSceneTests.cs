using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // Phase B of the video (_3.1 step 9B): the REAL LivingRoomScene, its shipped config and the fixture's real castle clip loaded through the
    // card's own media source. The Lamp holds an inline video (captions) and a chapters video in the takeover display; Lamp - Military's header
    // is the video loop. Every action is a real tap on the panel. Only the video DEVICE is a stand-in (a ManualVideoOutput that decodes nothing)
    // and the audio device a silent one, so the 200 s clip never costs 200 s of waiting; PoiCardRealVideoTests puts the real VideoPlayer under
    // the same card.
    public class PoiCardVideoSceneTests : SearchSceneFixture
    {
        private const string ClipPath = "video/castelo_s_jorge_video.mp4";

        private PoiCardSheetView Sheet => Card.Sheet;
        private CardSettings Live => Session.CardSettings;
        private ManualVideoOutput _videoOut;
        private CardVideoService _video;
        private ManualAudioOutput _audioOut;
        private CardAudioService _audio;
        private float _now;

        [UnitySetUp]
        public IEnumerator UseStandInDevices()
        {
            _now = 0f;
            _audioOut = new ManualAudioOutput();
            _audio = new CardAudioService(_audioOut, () => Card.Media, () => _now, () => Live.container.audio_when_another_starts);
            Card.UseAudioService(_audio);
            _videoOut = new ManualVideoOutput();
            _video = new CardVideoService(_videoOut, () => Card.Media);
            Card.UseVideoService(_video);
            Card.State = new CardLocalState(new MemoryCardStateStore(), Session.SearchConfig.wall_id);
            yield break;
        }

        [UnityTearDown]
        public IEnumerator ReleaseTheStandInTexture()
        {
            _video.CardClosed();
            _videoOut.Release();
            yield break;
        }

        private List<VideoBlockView> Videos => Sheet.Stack.BoundViews.OfType<VideoBlockView>().ToList();
        private VideoBlockView Inline => Videos.Single(v => v.Display == CardOptions.DisplayInline);
        private VideoBlockView Teaser => Videos.Single(v => v.Display == CardOptions.DisplayTakeover);
        private AudioGuideBlockView Chip => Sheet.Stack.BoundViews.OfType<AudioGuideBlockView>().Single(v => v.Variant == BuiltInBlocks.AudioGuideHeroChip);
        private HeaderBlockView Header => (HeaderBlockView)Sheet.Stack.BoundViews[0];

        // Video (and audio) time pass in steps the test chooses: the devices move, the owners take their step
        private void Advance(float seconds)
        {
            _now += seconds;
            _videoOut.Advance(seconds);
            _video.Tick();
            _audioOut.Advance(seconds);
            _audio.Tick();
        }

        private IEnumerator Select(string poiId)
        {
            SelectionEventBus.Select(poiId);
            yield return CardTestInput.Settle();
        }

        private IEnumerator OpenFull(string poiId)
        {
            if (SelectionEventBus.CurrentPoiId != poiId) yield return Select(poiId);
            Sheet.SetStop(SheetStopRule.Stop.Full);
            yield return CardTestInput.Settle();
        }

        private static IEnumerator Tap(VisualElement e)
        {
            yield return CardTestInput.Tap(e.panel, e.worldBound.center);
        }

        [UnityTest]
        public IEnumerator TheLamp_InlineVideo_FromTheWallsFolder_ARealTapPlaysIt_ThePosterUntilTheFirstFrame_ThenItsFramesAndCaptions()
        {
            yield return OpenFull("lamp");
            Assert.AreEqual(2, Videos.Count, "The Lamp holds two videos");
            var inline = Inline;
            Assert.AreEqual(BuiltInBlocks.VideoInline, inline.Variant);
            Assert.AreEqual("On film", Sheet.Stack.HeadingOf(inline).text, "the authored heading");
            Assert.AreEqual("0:00 / 3:20", inline.Panel.TimeText.text, "the real clip's length: 200.3 s");
            Assert.AreEqual("castelo_s_jorge_2", inline.Panel.Poster.Texture.name, "the real photograph as its poster, from the wall's folder");
            Assert.AreEqual(20, inline.Panel.Cues.Count, "the captions file loaded and parsed");
            Assert.AreEqual(DisplayStyle.None, inline.Panel.ChapterRow.resolvedStyle.display, "the inline look: no chapter buttons");
            Assert.AreEqual(CardVideoState.Idle, _video.State, "opening a card plays nothing by itself");

            // - each tap first scrolls its own button into view and checks it is there (a tap outside the view once left the captions
            //   off and read as a caption-timing failure, 2026-09-29): the timing itself is the stand-in device's, exact to the second
            yield return CardTestInput.TapInView(Sheet.Stack.Scroll, inline.Panel.PlayOverlay);
            Assert.AreEqual(CardVideoState.Playing, _video.State, "a real tap started it");
            Assert.AreEqual("castelo_s_jorge_video", _videoOut.Clip.name, "the real clip, through the card's media source");
            Assert.IsFalse(_videoOut.Muted, "with its sound");
            Assert.IsFalse(inline.Panel.ShowsFrames, "no frame yet: the poster");
            yield return Capture("Lamp_Video_Inline_Poster");
            Advance(1f);
            yield return null;
            Assert.IsTrue(inline.Panel.ShowsFrames, "the first frame arrived");
            // - the class was set during the tick: one more panel update resolves its style
            yield return null;
            Assert.AreEqual(DisplayStyle.None, inline.Panel.Poster.Root.resolvedStyle.display, "the poster stepped back");

            yield return CardTestInput.TapInView(Sheet.Stack.Scroll, inline.Panel.CaptionsButton);
            Assert.IsTrue(inline.Panel.CaptionsShown, "a real tap switched the captions on");
            Advance(11f);
            yield return null;
            Assert.AreEqual("Legenda de teste 2 de 20: texto provisório, não é a transcrição.", inline.Panel.CaptionLine.text,
                "at 0:12 the second placeholder caption (10 s to 20 s), Portuguese letters intact");
            Assert.AreEqual("0:12 / 3:20", inline.Panel.TimeText.text);
            // - the caption line sits under the bar: bring it on screen for the capture
            Sheet.Stack.Scroll.ScrollTo(inline.Panel.CaptionLine);
            yield return CardTestInput.Settle(0.2f);
            Assert.IsTrue(CardTestInput.IsShown(inline.Panel.CaptionLine, Sheet.Root), "the caption line is on screen");
            yield return Capture("Lamp_Video_Inline_Playing_Captions");
        }

        [UnityTest]
        public IEnumerator TheAudioGuidePauses_WhenARealTapStartsTheVideo_AndTheVideoPauses_WhenARealTapStartsTheAudio()
        {
            yield return Select("lamp");
            yield return Tap(Chip.PlayButton);
            Assert.AreEqual(CardAudioState.Playing, _audio.State, "precondition: the audio guide plays");
            Advance(5f);

            yield return OpenFull("lamp");
            yield return CardTestInput.TapInView(Sheet.Stack.Scroll, Inline.Panel.PlayOverlay);
            Assert.AreEqual(CardVideoState.Playing, _video.State);
            Assert.AreEqual(CardAudioState.Paused, _audio.State, "the video started: the audio guide paused, not stopped");
            Assert.AreEqual(5f, _audio.Position, 0.01f, "at its place");
            Assert.AreEqual(CardIcons.Shape.Play, Chip.PlayGlyph.Kind, "the chip offers play again");

            // - scrolled far down, the header collapses and hides its pinned chip: back to the top, as a visitor does, and the chip is there
            Sheet.Stack.Scroll.scrollOffset = Vector2.zero;
            yield return CardTestInput.Settle(0.2f);
            Assert.IsTrue(CardTestInput.IsShown(Chip.PlayButton, Sheet.Root), "precondition: the chip is on screen again");
            yield return Tap(Chip.PlayButton);
            Assert.AreEqual(CardAudioState.Playing, _audio.State, "a real tap resumed the audio");
            Assert.AreEqual(CardVideoState.Paused, _video.State, "and the video paused");
            Assert.AreEqual(CardIcons.Shape.Play, Inline.Panel.PlayGlyph.Kind);
        }

        // _3.1 [9B follow-up], step 15.1: the visitor starts the Lamp's narration while another point's still plays (the wall's Audio Overlap
        // is Switch: the other one fades out for half a second), then taps the video inside that half-second. The narration used to start when
        // the fade ended, under the video; now the pause the video asked for is kept
        [UnityTest]
        public IEnumerator AVideoTappedWhileAnotherPointsNarrationFadesIntoTheLamps_LeavesTheLampsNarrationPaused_AndTheVideoPlays()
        {
            Assert.AreEqual(CardOptions.AudioSwitch, Live.container.audio_when_another_starts, "precondition: the wall switches audio with a fade");
            Assert.IsTrue(Live.container.keep_audio_on_close, "precondition: an audio plays on when another point's card opens");
            yield return Select("lamp_military");
            yield return Tap(Chip.PlayButton);
            Assert.AreEqual(CardAudioState.Playing, _audio.State, "precondition: Lamp - Military's narration plays");
            Assert.AreEqual("lamp_tone", _audioOut.Clip.name);
            Advance(3f);

            yield return Select("lamp");
            Assert.AreEqual(CardAudioState.Playing, _audio.State, "precondition: it plays on under the Lamp's card");
            yield return Tap(Chip.PlayButton);
            Assert.AreEqual("lamp", _audio.Current.PoiId, "a real tap asked for the Lamp's narration");
            Assert.AreEqual("lamp_tone", _audioOut.Clip.name, "the other narration is fading out: the switch has begun");

            yield return OpenFull("lamp");
            yield return CardTestInput.TapInView(Sheet.Stack.Scroll, Inline.Panel.PlayOverlay);
            Assert.AreEqual(CardVideoState.Playing, _video.State, "a real tap inside the fade started the video");
            Assert.AreEqual(CardAudioState.Paused, _audio.State, "and the narration-to-be is paused at once");

            Advance(0.6f);
            yield return null;
            Assert.AreEqual("castelo_s_jorge_guide_pt", _audioOut.Clip.name, "the fade is over: the Lamp's narration is loaded");
            Assert.IsFalse(_audioOut.IsPlaying, "but it does NOT play under the video");
            Assert.AreEqual(CardAudioState.Paused, _audio.State, "the narration ends the fade paused");
            Assert.AreEqual(CardVideoState.Playing, _video.State, "the video plays on");
            Sheet.Stack.Scroll.scrollOffset = Vector2.zero;
            yield return CardTestInput.Settle(0.2f);
            Assert.AreEqual(CardIcons.Shape.Play, Chip.PlayGlyph.Kind, "the chip offers Play: the visitor starts it when they want");
        }

        [UnityTest]
        public IEnumerator TheChaptersVideo_IsATeaser_ARealTapPlaysItFullScreen_AChapterJumps_AndBackKeepsTheStopAndTheScroll()
        {
            yield return OpenFull("lamp");
            var teaser = Teaser;
            Assert.AreEqual(BuiltInBlocks.VideoChapters, teaser.Variant);
            Assert.AreEqual(DisplayStyle.None, teaser.Panel.Root.resolvedStyle.display, "the takeover display: no player on the card");
            Assert.AreEqual("3:20", teaser.TeaserLength.text);
            Assert.AreEqual("castelo_s_jorge_1", teaser.TeaserPoster.Texture.name);
            // - to the teaser's own button (then the tap below finds it in view and scrolls no further: the offset Back must give back)
            Sheet.Stack.Scroll.ScrollTo(teaser.TeaserButton);
            yield return CardTestInput.Settle(0.2f);
            float offset = Sheet.Stack.Scroll.scrollOffset.y;
            Assert.Greater(offset, 500f, "precondition: the card is scrolled far down");
            yield return Capture("Lamp_Video_Teaser");

            yield return CardTestInput.TapInView(Sheet.Stack.Scroll, teaser.TeaserButton);
            yield return CardTestInput.Settle(0.2f);
            Assert.IsTrue(Sheet.Takeover.IsOpen, "a real tap opened it full screen");
            Assert.AreEqual(Header.TitleText + " > The castle, chapter by chapter", Sheet.Takeover.Crumb.text);
            Assert.AreEqual(CardVideoState.Playing, _video.State, "and started it");
            var full = teaser.FullScreenPanel;
            CollectionAssert.AreEqual(new[] { "0:00  Opening", "0:50  Part two", "1:40  Part three", "2:30  Closing" },
                full.ChapterButtons.Select(b => b.text).ToList(), "the four chapters, EN");
            yield return Tap(full.ChapterButtons[2]);
            Advance(1f);
            yield return null;
            Assert.AreEqual(101f, _video.Position, 0.01f, "a real tap on Part three jumped to 1:40");
            Assert.IsTrue(full.ChapterButtons[2].ClassListContains("card-video__chapter--current"), "and lights it");
            Assert.IsTrue(full.ShowsFrames);
            yield return Capture("Lamp_Video_Takeover_Chapters");

            yield return Tap(Sheet.Takeover.Back);
            yield return CardTestInput.Settle(0.2f);
            Assert.IsFalse(Sheet.Takeover.IsOpen, "Back");
            Assert.AreEqual(SheetStopRule.Stop.Full, Sheet.Stop, "the card at its stop");
            Assert.AreEqual(offset, Sheet.Stack.Scroll.scrollOffset.y, 1f, "and at its scroll");
            Assert.AreEqual(CardVideoState.Playing, _video.State, "the video plays on");
            Assert.IsNull(teaser.FullScreenPanel, "the full-screen panel was let go");
        }

        [UnityTest]
        public IEnumerator InPortuguese_TheVideosSpeakPortuguese_HeadingsChaptersAndButtons()
        {
            Live.languages = new List<string> { "pt", "en" };
            yield return OpenFull("lamp");
            Assert.AreEqual("Em vídeo", Sheet.Stack.HeadingOf(Inline).text);
            Assert.AreEqual("Capítulo a capítulo", Sheet.Stack.HeadingOf(Teaser).text);
            Assert.AreEqual("Ecrã inteiro", Inline.Panel.FullScreenButton.tooltip);
            Assert.AreEqual("Legendas", Inline.Panel.CaptionsButton.text);
            Assert.AreEqual("Reproduzir", Inline.Panel.PlayButton.tooltip);
            yield return CardTestInput.TapInView(Sheet.Stack.Scroll, Teaser.TeaserButton);
            yield return CardTestInput.Settle(0.2f);
            StringAssert.EndsWith("O castelo, capítulo a capítulo", Sheet.Takeover.Crumb.text);
            CollectionAssert.AreEqual(new[] { "0:00  Abertura", "0:50  Segunda parte", "1:40  Terceira parte", "2:30  Encerramento" },
                Teaser.FullScreenPanel.ChapterButtons.Select(b => b.text).ToList());
            Assert.AreEqual("Pausar", Teaser.FullScreenPanel.PlayButton.tooltip);
            Advance(1f);
            yield return null;
            yield return Capture("Lamp_Video_Takeover_PT");
        }

        [UnityTest]
        public IEnumerator LampMilitary_HeaderLoopsTheClipMuted_ItsPosterFirst_AndReduceMotionShowsOnlyThePoster()
        {
            yield return OpenFull("lamp_military");
            var header = Header;
            Assert.IsTrue(header.HasHero, "the video loop look has its hero");
            Assert.AreEqual(ClipPath, header.LoopTrack.ClipPath);
            Assert.AreEqual("castelo_s_jorge_1", header.Picture.Texture.name, "the poster, from the wall's folder");
            Assert.IsTrue(_video.PlayingLoop, "the header asked for its loop");
            Assert.IsTrue(_videoOut.Muted, "muted");
            Assert.IsTrue(_videoOut.Looping, "looping");
            Assert.IsFalse(header.LoopShowsFrames, "no frame yet: the poster shows first");
            Sheet.Stack.Scroll.scrollOffset = Vector2.zero;
            yield return CardTestInput.Settle(0.15f);
            yield return Capture("Military_Header_Loop_Poster");
            Advance(0.5f);
            yield return null;
            Assert.IsTrue(header.LoopShowsFrames, "then the loop's frames");
            Assert.AreEqual(CardAudioState.Idle, _audio.State, "a muted loop starts no sound and pauses none");
            Advance(250f);
            Assert.IsTrue(_video.PlayingLoop, "still playing past the clip's end: it loops");
            yield return Capture("Military_Header_Loop_Playing");

            Live.container.reduce_motion = true;
            yield return Select("lamp");
            yield return OpenFull("lamp_military");
            Assert.AreEqual(CardVideoState.Idle, _video.State, "Reduce Motion: no loop is asked for");
            Assert.IsFalse(Header.LoopShowsFrames);
            Assert.AreEqual("castelo_s_jorge_1", Header.Picture.Texture.name, "the poster only");
            Sheet.Stack.Scroll.scrollOffset = Vector2.zero;
            yield return CardTestInput.Settle(0.15f);
            yield return Capture("Military_Header_Loop_ReduceMotion");
        }

        [UnityTest]
        public IEnumerator ClosingTheCard_StopsTheVideoAndGivesTheClipBack_SoDoesAnotherPointsCard()
        {
            yield return OpenFull("lamp");
            yield return CardTestInput.TapInView(Sheet.Stack.Scroll, Inline.Panel.PlayOverlay);
            Advance(2f);
            yield return Tap(Sheet.CloseButton);
            Assert.IsFalse(Sheet.IsOpen);
            Assert.AreEqual(CardVideoState.Idle, _video.State, "a video belongs to its card: closed, it stops");
            Assert.AreEqual(0, Card.Media.RefCount(ClipPath), "and the clip was given back");

            yield return OpenFull("lamp");
            yield return CardTestInput.TapInView(Sheet.Stack.Scroll, Inline.Panel.PlayOverlay);
            Assert.AreEqual(CardVideoState.Playing, _video.State);
            yield return Select("lamp_economic");
            Assert.AreEqual(CardVideoState.Idle, _video.State, "another point's card: the video stopped");
        }
    }
}

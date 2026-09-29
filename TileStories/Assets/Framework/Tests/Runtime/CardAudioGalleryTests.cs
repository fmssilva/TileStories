using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace TileStories.Tests
{
    // Phase A of the audio guide (_3.1 step 9A, 40-testing.md 4.4): both looks on the real gallery card, driven with REAL pointer events
    // (a tap on play / pause / the speed chip / the captions switch, a drag along the scrubber) over the card's real audio service. The
    // sound device is the gallery's silent ManualAudioOutput and audio time moves only through AdvanceAudio, so no assertion waits
    // for a clip. The generic fit / contrast / tap-target / heading / render checks of every entry are CardGalleryTests' own
    // (the audio entries are in the same list).
    public class CardAudioGalleryTests
    {
        private const string ScenePath = "Assets/Dev/CardGallery/CardGalleryScene.unity";
        private CardGalleryHarness _harness;

        private static int IndexOf(string name) => CardGalleryDefinitions.All.ToList().FindIndex(e => e.Name == name);

        [UnitySetUp]
        public IEnumerator SetUp()
        {
#if UNITY_EDITOR
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
#else
            Assert.Ignore("Needs the Editor to load the gallery scene by path.");
#endif
            for (int i = 0; i < 60 && _harness == null; i++)
            {
                _harness = Object.FindFirstObjectByType<CardGalleryHarness>();
                yield return null;
            }
            Assert.IsNotNull(_harness, "the gallery scene holds CardGalleryHarness");
            _harness.EnsureBuilt();
            // - audio time is a number the test sets: real time never moves the gallery's clip
            _harness.AutoAdvanceAudio = false;
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            var scene = SceneManager.GetSceneByPath(ScenePath);
            SceneManager.SetActiveScene(SceneManager.CreateScene("AfterCardAudioGalleryTest"));
            if (scene.IsValid() && scene.isLoaded) yield return SceneManager.UnloadSceneAsync(scene);
        }

        private CardAudioService Audio => _harness.AudioService;
        private ManualAudioOutput Output => _harness.AudioOutput;

        // Show a gallery entry and hand back its audio block's view (scrolled into view unless it is pinned under the header)
        private IEnumerator Show(string name, System.Action<AudioGuideBlockView> got)
        {
            _harness.Show(IndexOf(name));
            IBlockView view = null;
            yield return CardGalleryChecks.SettledBlock(_harness, name, v => view = v);
            got((AudioGuideBlockView)view);
        }

        // Show a block over a POI the test builds (an entry that is not in the gallery list), then hand back the audio views in stack order
        private IEnumerator ShowCustom(CardGalleryDefinitions.Entry entry, System.Action<System.Collections.Generic.List<AudioGuideBlockView>> got)
        {
            CardGalleryChecks.ShowEntry(_harness, entry);
            yield return CardTestInput.Settle();
            got(_harness.Sheet.Stack.BoundViews.OfType<AudioGuideBlockView>().ToList());
        }

        // A real tap at the centre of `target`
        private static IEnumerator Tap(VisualElement target)
        {
            yield return CardTestInput.Tap(target.panel, target.worldBound.center);
        }

        private static string Words(AudioGuideBlockView v) => v.TimeText.text;

        // ---------------- the player ----------------

        [UnityTest]
        public IEnumerator ThePlayer_ARealTapOnPlay_StartsTheAudio_TheButtonBecomesPause_ATapPauses_ATapResumes()
        {
            AudioGuideBlockView view = null;
            yield return Show("audio_guide_player_short", v => view = v);
            Assert.AreEqual(CardIcons.Shape.Play, view.PlayGlyph.Kind, "idle: the play shape");
            Assert.AreEqual("0:00 / 0:12", Words(view), "the clip's real length is known before it plays");
            Assert.AreEqual(0f, view.Scrubber.Fraction);
            Assert.AreEqual("1x", view.SpeedChip.text);
            Assert.AreEqual(CardGalleryDefinitions.AudioTitle, view.Title.text);
            Assert.AreEqual("Play", view.PlayButton.tooltip, "the button's name comes from the card's texts");
            Assert.IsTrue(UIAccessibility.MeetsMinTapTarget(view.PlayButton.worldBound.width, view.PlayButton.worldBound.height), "a finger-sized button");
            Assert.AreEqual(CardAudioState.Idle, Audio.State, "nothing plays until asked");

            yield return Tap(view.PlayButton);
            Assert.AreEqual(CardAudioState.Playing, Audio.State, "a real tap started it");
            Assert.IsTrue(Audio.IsCurrent(view.Track));
            Assert.IsTrue(Output.IsPlaying);
            Assert.AreEqual(CardIcons.Shape.Pause, view.PlayGlyph.Kind, "the button now offers pause");
            Assert.AreEqual("Pause", view.PlayButton.tooltip);
            Assert.IsTrue(view.Root.ClassListContains("card-audio--playing"));

            _harness.AdvanceAudio(3f);
            Assert.AreEqual("0:03 / 0:12", Words(view), "the time follows the clip");
            Assert.AreEqual(0.25f, view.Scrubber.Fraction, 0.01f, "and so does the bar");
            yield return null;
            Assert.AreEqual(0.25f, view.Scrubber.Fill.resolvedStyle.width / view.Scrubber.Track.resolvedStyle.width, 0.02f, "drawn as a quarter of the track");

            yield return Tap(view.PlayButton);
            Assert.AreEqual(CardAudioState.Paused, Audio.State);
            Assert.IsFalse(Output.IsPlaying);
            Assert.AreEqual(CardIcons.Shape.Play, view.PlayGlyph.Kind);
            _harness.AdvanceAudio(2f);
            Assert.AreEqual("0:03 / 0:12", Words(view), "paused: the time stands still");

            yield return Tap(view.PlayButton);
            _harness.AdvanceAudio(1f);
            Assert.AreEqual("0:04 / 0:12", Words(view), "resumed from where it paused");
            Assert.GreaterOrEqual(CardTestInput.Contrast(view.PlayGlyph.resolvedStyle.color, view.PlayButton.resolvedStyle.backgroundColor), UIAccessibility.MinRatioLargeTextOrUIComponent,
                "the glyph reads on the button");
            yield return CardGalleryChecks.Render("Card_audio_player_playing");
        }

        [UnityTest]
        public IEnumerator TheScrubber_ARealPressJumpsThere_ARealDragFollowsTheFinger_AndItDoesNothingBeforeThePlay()
        {
            AudioGuideBlockView view = null;
            yield return Show("audio_guide_player_short", v => view = v);
            Rect bar = view.Scrubber.Root.worldBound;
            Assert.GreaterOrEqual(bar.height, 48f - 0.5f, "the bar's hit area is finger-tall");
            var panel = view.Root.panel;

            // - nothing loaded yet: a press on the bar seeks nothing (and starts nothing)
            yield return CardTestInput.Tap(panel, new Vector2(bar.x + bar.width * 0.5f, bar.center.y));
            Assert.AreEqual(CardAudioState.Idle, Audio.State);
            Assert.AreEqual(0f, view.Scrubber.Fraction, "idle: the bar stays empty");

            yield return Tap(view.PlayButton);
            yield return CardTestInput.Tap(panel, new Vector2(bar.x + bar.width * 0.25f, bar.center.y));
            Assert.AreEqual(3f, Audio.Position, 0.2f, "a press at a quarter of the bar: a quarter of the twelve seconds");
            Assert.AreEqual(0.25f, view.Scrubber.Fraction, 0.02f);

            yield return CardTestInput.DragFrom(panel, new Vector2(bar.x + bar.width * 0.25f, bar.center.y), new Vector2(bar.width * 0.55f, 0f), 6);
            yield return null;
            Assert.AreEqual(9.6f, Audio.Position, 0.2f, "dragged to four fifths: nine and a half seconds");
            Assert.AreEqual("0:09 / 0:12", Words(view));
            Assert.IsFalse(view.Scrubber.IsDragging, "the finger lifted: the drag is over");
            Assert.AreEqual(0.8f, view.Scrubber.Knob.resolvedStyle.left / view.Scrubber.Track.resolvedStyle.width, 0.02f, "the knob sits at four fifths of the track");
            Assert.AreEqual(CardAudioState.Playing, Audio.State, "scrubbing does not pause");

            yield return CardTestInput.DragFrom(panel, new Vector2(bar.x + bar.width * 0.9f, bar.center.y), new Vector2(bar.width * 0.5f, 0f), 6);
            Assert.Greater(Audio.Position, 11.5f, "dragged past the end: clamped to the end of the clip, never beyond");
            Assert.Less(Audio.Position, 12f);
        }

        [UnityTest]
        public IEnumerator TheSpeedChip_RealTapsCycleTheSpeeds_TheClipMovesFaster_AndOffHasNoChip()
        {
            AudioGuideBlockView view = null;
            yield return Show("audio_guide_player_short", v => view = v);
            Assert.IsTrue(UIAccessibility.MeetsMinTapTarget(view.SpeedChip.worldBound.width, view.SpeedChip.worldBound.height), "the chip is a finger-sized target");
            Assert.GreaterOrEqual(CardTestInput.Contrast(view.SpeedChip.resolvedStyle.color, CardTestInput.EffectiveBackground(view.SpeedChip)), UIAccessibility.MinRatioNormalText, "chip contrast");
            yield return Tap(view.PlayButton);
            foreach (var (label, speed) in new[] { ("1.25x", 1.25f), ("1.5x", 1.5f), ("0.75x", 0.75f), ("1x", 1f) })
            {
                yield return Tap(view.SpeedChip);
                Assert.AreEqual(label, view.SpeedChip.text, "the next speed of the Narration list");
                Assert.AreEqual(speed, Output.Speed, "the device plays at it");
                Assert.AreEqual(speed, Audio.Speed);
            }
            yield return Tap(view.SpeedChip);
            float before = Audio.Position;
            _harness.AdvanceAudio(4f);
            Assert.AreEqual(before + 5f, Audio.Position, 0.05f, "four seconds at 1.25x move the clip five");

            yield return Show("audio_guide_player_nospeed", v => view = v);
            Assert.AreEqual(DisplayStyle.None, view.SpeedChip.resolvedStyle.display, "the Speeds field says Off: no chip");
            Assert.AreEqual(DisplayStyle.None, view.SpeedChip.style.display.value);
        }

        [UnityTest]
        public IEnumerator TheCaptions_ASwitchShowsTheLineOfTheMoment_ItFollowsTheClipTime_AndAGapHasNone()
        {
            AudioGuideBlockView view = null;
            yield return Show("audio_guide_player_short", v => view = v);
            Assert.AreEqual(DisplayStyle.Flex, view.CaptionsButton.style.display.value, "a captions file: the switch is there");
            Assert.AreEqual("Captions", view.CaptionsButton.text);
            Assert.IsTrue(UIAccessibility.MeetsMinTapTarget(view.CaptionsButton.worldBound.width, view.CaptionsButton.worldBound.height));
            Assert.IsFalse(view.CaptionsShown, "off until the visitor switches them on (Captions On By Default is off)");
            Assert.AreEqual(DisplayStyle.None, view.CaptionLine.resolvedStyle.display, "no line while off");
            Assert.AreEqual(3, view.Cues.Count, "the block parsed the file through VttRule");

            yield return Tap(view.PlayButton);
            yield return Tap(view.CaptionsButton);
            Assert.IsTrue(view.CaptionsShown, "a real tap switched them on");
            Assert.IsTrue(view.CaptionsButton.ClassListContains("card-audio__chip--on"));
            _harness.AdvanceAudio(1f);
            Assert.AreEqual(CardGalleryDefinitions.ShortCaptionOne, view.CaptionLine.text, "at 0:01 the first line");
            _harness.AdvanceAudio(3.5f);
            Assert.AreEqual(CardGalleryDefinitions.ShortCaptionTwo, view.CaptionLine.text, "at 0:04.5 the second");
            yield return CardGalleryChecks.Render("Card_audio_player_captions_on");
            _harness.AdvanceAudio(3.7f);
            Assert.AreEqual("", view.CaptionLine.text, "at 0:08.2, between two cues: no caption");
            _harness.AdvanceAudio(0.3f);
            Assert.AreEqual(CardGalleryDefinitions.ShortCaptionThree, view.CaptionLine.text, "at 0:08.5 the third");

            // - a seek moves the line with the audio
            Rect bar = view.Scrubber.Root.worldBound;
            yield return CardTestInput.Tap(view.Root.panel, new Vector2(bar.x + bar.width * 0.1f, bar.center.y));
            Assert.AreEqual(CardGalleryDefinitions.ShortCaptionOne, view.CaptionLine.text, "scrubbed back to the start: the first line again");

            yield return Tap(view.CaptionsButton);
            Assert.IsFalse(view.CaptionsShown);
            Assert.AreEqual(DisplayStyle.None, view.CaptionLine.resolvedStyle.display, "switched off again");

            yield return Show("audio_guide_player_nocaptions", v => view = v);
            Assert.AreEqual(0, view.Cues.Count);
            Assert.AreEqual(DisplayStyle.None, view.CaptionsButton.style.display.value, "no captions file: no switch");
        }

        [UnityTest]
        public IEnumerator TheCaptionLine_OfALongClip_WrapsInsideTheCard_WithoutMovingTheButtonsAbove()
        {
            AudioGuideBlockView view = null;
            yield return Show("audio_guide_player_long", v => view = v);
            Assert.AreEqual("0:00 / 10:15", Words(view), "a ten-minute clip's time");
            Assert.AreEqual(CardGalleryDefinitions.AudioLongTitle, view.Title.text, "the long title stays on one line (ellipsis), never widening the row");
            Assert.LessOrEqual(view.Title.worldBound.xMax, _harness.Sheet.Root.worldBound.xMax + 0.5f);
            yield return Tap(view.PlayButton);
            yield return Tap(view.CaptionsButton);
            float playY = view.PlayButton.worldBound.y, chipY = view.CaptionsButton.worldBound.y;
            _harness.AdvanceAudio(1f);
            Assert.AreEqual(CardGalleryDefinitions.LongCaptionOne, view.CaptionLine.text);
            yield return CardTestInput.Settle(0.15f);
            Assert.Greater(view.CaptionLine.worldBound.height, view.CaptionLine.resolvedStyle.fontSize * 2f, "a long caption wraps over several lines");
            Assert.LessOrEqual(view.CaptionLine.worldBound.xMax, _harness.Sheet.Root.worldBound.xMax + 0.5f, "inside the card");
            Assert.AreEqual(playY, view.PlayButton.worldBound.y, 0.5f, "the play button did not move");
            Assert.AreEqual(chipY, view.CaptionsButton.worldBound.y, 0.5f, "nor the chips");
            Assert.GreaterOrEqual(CardTestInput.Contrast(view.CaptionLine.resolvedStyle.color, CardTestInput.EffectiveBackground(view.CaptionLine)), UIAccessibility.MinRatioNormalText);
        }

        [UnityTest]
        public IEnumerator ACaptionsOnByDefaultBlock_OpensWithTheLineShown()
        {
            var entry = new CardGalleryDefinitions.Entry(BuiltInBlocks.AudioGuideKind, BuiltInBlocks.AudioGuidePlayer, "captionson",
                CardGalleryDefinitions.AudioBlock(BuiltInBlocks.AudioGuidePlayer, "short.mp3", "short.vtt", "Captioned", captionsOn: true));
            System.Collections.Generic.List<AudioGuideBlockView> views = null;
            yield return ShowCustom(entry, v => views = v);
            Assert.IsTrue(views[0].CaptionsShown);
            Assert.AreEqual(DisplayStyle.Flex, views[0].CaptionLine.resolvedStyle.display);
        }

        // ---------------- the hero chip ----------------

        [UnityTest]
        public IEnumerator TheHeroChip_SitsPinnedUnderTheHeader_CountsInThePeekStop_AndARealTapPlaysAndPauses()
        {
            AudioGuideBlockView chip = null;
            yield return Show("audio_guide_hero_chip_short", v => chip = v);
            var stack = _harness.Sheet.Stack;
            Assert.AreSame(stack.PinnedTop, stack.SlotOf(chip).parent, "a hero chip is pinned, not in the scroll");
            var header = stack.HeaderView;
            Assert.GreaterOrEqual(stack.PinnedTop.worldBound.yMin, header.PeekPart.worldBound.yMax - 0.5f, "under the title and chip");
            Assert.LessOrEqual(stack.PinnedTop.worldBound.yMax, stack.Scroll.worldBound.yMin + 0.5f, "above what scrolls");
            // - this entry's header has no subtitle text (SubtitleShown false: the gallery's generic block entries don't author one), so the
            //   subtitle-stays-hidden-at-peek claim is proven on the real Lamp instead (it has one): PoiCardAudioSceneTests.TheLamp_HoldsAHeroChipUnderItsTitle_...
            Assert.IsFalse(header.SubtitleShown, "precondition: nothing to check here");
            Assert.AreEqual(DisplayStyle.None, chip.Root.Q<VisualElement>(className: "card-audio__chips").resolvedStyle.display, "no speed chip or captions switch on the chip");
            Assert.AreEqual(DisplayStyle.None, chip.CaptionLine.resolvedStyle.display, "no caption line either");
            Assert.AreEqual(PickingMode.Ignore, chip.Scrubber.Root.pickingMode, "its bar is a progress line, not a scrubber");

            // - the peek stop shows the whole chip
            _harness.Sheet.SetStop(SheetStopRule.Stop.Peek);
            yield return CardTestInput.Settle(0.3f);
            Rect card = _harness.Sheet.Root.worldBound;
            Assert.LessOrEqual(chip.Root.worldBound.yMax, card.yMax + 1f, "the chip is inside the peeking card");
            Assert.LessOrEqual(stack.PeekPart.worldBound.yMax, card.yMax + 1f);
            Assert.AreSame(stack.PinnedTop, stack.PeekPart, "the peek stop ends where the pinned strip ends");

            yield return Tap(chip.PlayButton);
            Assert.AreEqual(CardAudioState.Playing, Audio.State, "a tap on the chip at the peek stop plays");
            Assert.AreEqual(SheetStopRule.Stop.Peek, _harness.Sheet.Stop, "the card did not open");
            _harness.AdvanceAudio(6f);
            Assert.AreEqual("0:06 / 0:12", Words(chip));
            Assert.AreEqual(0.5f, chip.Scrubber.Fraction, 0.01f, "the thin line shows the progress");
            Assert.AreEqual(CardIcons.Shape.Pause, chip.PlayGlyph.Kind);
            yield return CardGalleryChecks.Render("Card_audio_hero_chip_peek_playing");
            yield return Tap(chip.PlayButton);
            Assert.AreEqual(CardAudioState.Paused, Audio.State);
            Assert.AreEqual(CardIcons.Shape.Play, chip.PlayGlyph.Kind);
        }

        [UnityTest]
        public IEnumerator AChipAndAPlayerOfTheSameClip_AreOneAudio_WhicheverIsTapped_BothShowIt()
        {
            var entry = new CardGalleryDefinitions.Entry(BuiltInBlocks.AudioGuideKind, BuiltInBlocks.AudioGuideHeroChip, "twoblocks",
                CardGalleryDefinitions.AudioBlock(BuiltInBlocks.AudioGuideHeroChip, "short.mp3", "short.vtt", "Two views"),
                setup: poi =>
                {
                    var second = CardGalleryDefinitions.AudioBlock(BuiltInBlocks.AudioGuidePlayer, "short.mp3", "short.vtt", "Two views");
                    second.key = "block_3";
                    poi.card.blocks.Add(second);
                });
            System.Collections.Generic.List<AudioGuideBlockView> views = null;
            yield return ShowCustom(entry, v => views = v);
            Assert.AreEqual(2, views.Count);
            var chip = views.Single(v => v.Variant == BuiltInBlocks.AudioGuideHeroChip);
            var player = views.Single(v => v.Variant == BuiltInBlocks.AudioGuidePlayer);
            _harness.Sheet.Stack.Scroll.ScrollTo(_harness.Sheet.Stack.SlotOf(player));
            yield return CardTestInput.Settle(0.15f);

            yield return Tap(chip.PlayButton);
            Assert.AreEqual(CardIcons.Shape.Pause, player.PlayGlyph.Kind, "the chip started it: the player shows it playing");
            _harness.AdvanceAudio(4f);
            Assert.AreEqual(Words(chip), Words(player), "the same time on both");
            yield return Tap(player.PlayButton);
            Assert.AreEqual(CardIcons.Shape.Play, chip.PlayGlyph.Kind, "the player paused it: the chip shows it paused");
            Assert.AreEqual(1, Output.LoadCount, "one audio, one load");
        }

        // ---------------- states and words ----------------

        [UnityTest]
        public IEnumerator AClipThatIsNotThere_ShowsItsNote_TheButtonIsOffAndATapPlaysNothing()
        {
            AudioGuideBlockView view = null;
            yield return Show("audio_guide_player_unavailable", v => view = v);
            Assert.IsFalse(view.ClipAvailable);
            Assert.AreEqual("Audio unavailable", view.Note.text);
            Assert.AreEqual(DisplayStyle.Flex, view.Note.resolvedStyle.display);
            Assert.IsFalse(view.PlayButton.enabledSelf, "nothing to play");
            Assert.AreEqual(DisplayStyle.None, view.SpeedChip.style.display.value);
            yield return Tap(view.PlayButton);
            Assert.AreEqual(CardAudioState.Idle, Audio.State);
            Assert.AreEqual(0, Output.PlayCount);
            Assert.IsTrue(view.Root.ClassListContains("card-audio--unavailable"));
        }

        [UnityTest]
        public IEnumerator InPortuguese_TheButtonsCarryTheirPortugueseNames()
        {
            _harness.Language = "pt";
            AudioGuideBlockView view = null;
            yield return Show("audio_guide_player_short", v => view = v);
            Assert.AreEqual("Reproduzir", view.PlayButton.tooltip);
            Assert.AreEqual("Legendas", view.CaptionsButton.text);
            Assert.AreEqual("Velocidade de reprodução", view.SpeedChip.tooltip);
            Assert.AreEqual("Posição no áudio", view.Scrubber.Root.tooltip);
            yield return Tap(view.PlayButton);
            Assert.AreEqual("Pausar", view.PlayButton.tooltip);
            yield return Show("audio_guide_player_unavailable", v => view = v);
            Assert.AreEqual("Áudio indisponível", view.Note.text);
            yield return CardGalleryChecks.Render("Card_audio_player_unavailable_pt");
        }

        // ---------------- one audio at a time ----------------

        [UnityTest]
        public IEnumerator AnotherCardsAudio_InSwitchMode_FadesTheFirstOutInHalfASecond_ThenPlaysTheSecond()
        {
            _harness.AudioMode = CardOptions.AudioSwitch;
            AudioGuideBlockView first = null, second = null;
            yield return Show("audio_guide_player_short", v => first = v);
            yield return Tap(first.PlayButton);
            _harness.AdvanceAudio(2f);
            var firstTrack = first.Track;

            yield return Show("audio_guide_player_long", v => second = v);
            Assert.IsTrue(Audio.IsCurrent(firstTrack), "opening another card does not stop the first audio (Keep Audio Playing)");
            yield return Tap(second.PlayButton);
            Assert.IsTrue(Audio.IsCurrent(second.Track), "the tapped audio is the current one at once");
            Assert.AreEqual(CardIcons.Shape.Pause, second.PlayGlyph.Kind, "its button already says pause");
            Assert.AreEqual("short.mp3", Output.Clip.name, "the first sound is still what plays, fading");
            _harness.AdvanceAudio(0.25f);
            Assert.AreEqual(0.5f, Output.Volume, 0.05f, "halfway through the fade");
            _harness.AdvanceAudio(0.25f);
            Assert.AreEqual("long.mp3", Output.Clip.name, "the fade over: the second sound plays");
            Assert.AreEqual(1f, Output.Volume);
            Assert.AreEqual("0:00 / 10:15", Words(second));
            Assert.AreEqual(2, _harness.Media.RefCount("long.mp3"), "held by the open card's block (its length) and by the playing audio");
            Assert.AreEqual(0, _harness.Media.RefCount("short.mp3"), "the first block released its own load on unbind, the service its own when the fade ended");
        }

        [UnityTest]
        public IEnumerator AnotherCardsAudio_InQueueMode_ShowsUpNext_AndStartsWhenTheFirstEnds()
        {
            _harness.AudioMode = CardOptions.AudioQueue;
            AudioGuideBlockView first = null, second = null;
            yield return Show("audio_guide_player_short", v => first = v);
            yield return Tap(first.PlayButton);
            yield return Show("audio_guide_player_long", v => second = v);
            yield return Tap(second.PlayButton);
            Assert.IsTrue(Audio.IsQueued(second.Track), "the second audio waits");
            Assert.AreEqual("short.mp3", Output.Clip.name, "the first keeps playing");
            Assert.AreEqual("Up next", second.Note.text, "the block says it waits its turn");
            Assert.AreEqual(DisplayStyle.Flex, second.Note.resolvedStyle.display);
            Assert.AreEqual(CardIcons.Shape.Play, second.PlayGlyph.Kind, "it is not playing yet");
            Assert.IsTrue(second.Root.ClassListContains("card-audio--queued"));

            yield return Tap(second.PlayButton);
            Assert.IsFalse(Audio.IsQueued(second.Track), "a second tap takes it out of the queue");
            Assert.AreEqual(DisplayStyle.None, second.Note.resolvedStyle.display);
            yield return Tap(second.PlayButton);
            Assert.IsTrue(Audio.IsQueued(second.Track));

            for (int i = 0; i < 40 && Audio.IsCurrent(second.Track) == false; i++) _harness.AdvanceAudio(0.5f);
            Assert.IsTrue(Audio.IsCurrent(second.Track), "the first ended: the queued audio plays by itself");
            Assert.AreEqual("long.mp3", Output.Clip.name);
            Assert.AreEqual(CardIcons.Shape.Pause, second.PlayGlyph.Kind);
            Assert.AreEqual(DisplayStyle.None, second.Note.resolvedStyle.display, "no longer waiting");
        }

        // ---------------- the mini-player ----------------

        [UnityTest]
        public IEnumerator TheMiniPlayer_AppearsWhenTheCardClosesOverPlayingAudio_HasItsOwnButton_AndATapOnItsTitleShowsTheCardAgain()
        {
            AudioGuideBlockView view = null;
            yield return Show("audio_guide_player_short", v => view = v);
            var mini = _harness.AudioCoordinator.Mini;
            Assert.IsFalse(mini.IsShown, "a card is open: it holds its own player");
            yield return Tap(view.PlayButton);
            _harness.AdvanceAudio(2f);
            Assert.IsFalse(mini.IsShown, "playing with the card open: still no mini-player");

            yield return Tap(_harness.Sheet.CloseButton);
            Assert.IsFalse(_harness.Sheet.IsOpen, "a real tap on the X closed the card");
            Assert.AreEqual(CardAudioState.Playing, Audio.State, "the audio plays on (Keep Audio Playing)");
            Assert.IsTrue(mini.IsShown, "the mini-player takes over");
            yield return CardTestInput.Settle(0.15f);
            Assert.AreEqual(CardGalleryDefinitions.AudioTitle, mini.Title.text);
            Assert.AreEqual("0:02 / 0:12", mini.TimeText.text);
            Assert.AreEqual(CardIcons.Shape.Pause, mini.PlayGlyph.Kind);
            Assert.IsTrue(UIAccessibility.MeetsMinTapTarget(mini.PlayButton.worldBound.width, mini.PlayButton.worldBound.height), "its button is finger-sized");
            Assert.IsTrue(UIAccessibility.MeetsMinTapTarget(mini.OpenButton.worldBound.width, mini.OpenButton.worldBound.height), "so is its title");
            Assert.GreaterOrEqual(CardTestInput.Contrast(mini.Title.resolvedStyle.color, CardTestInput.EffectiveBackground(mini.Title)), UIAccessibility.MinRatioNormalText);
            Assert.AreEqual("Pause", mini.PlayButton.tooltip);
            _harness.AdvanceAudio(1f);
            Assert.AreEqual("0:03 / 0:12", mini.TimeText.text, "it follows the clip");
            yield return CardGalleryChecks.Render("Card_audio_miniplayer");

            yield return Tap(mini.PlayButton);
            Assert.AreEqual(CardAudioState.Paused, Audio.State, "its button pauses");
            Assert.IsTrue(mini.IsShown, "a paused audio is still the mini-player's");
            Assert.AreEqual(CardIcons.Shape.Play, mini.PlayGlyph.Kind);
            yield return Tap(mini.PlayButton);
            Assert.AreEqual(CardAudioState.Playing, Audio.State, "and resumes");

            yield return Tap(mini.OpenButton);
            yield return CardTestInput.Settle(0.2f);
            Assert.IsTrue(_harness.Sheet.IsOpen, "a tap on the title showed the card again");
            Assert.IsFalse(mini.IsShown, "the card holds the player again");
            var again = _harness.Sheet.Stack.BoundViews.OfType<AudioGuideBlockView>().Single();
            Assert.AreEqual(CardIcons.Shape.Pause, again.PlayGlyph.Kind, "the reopened card shows the audio that kept playing");
            Assert.AreEqual(CardAudioState.Playing, Audio.State);
        }

        [UnityTest]
        public IEnumerator WithKeepAudioPlayingOff_TheAudioStopsWithItsCard_AndNoMiniPlayerAppears()
        {
            var entry = new CardGalleryDefinitions.Entry(BuiltInBlocks.AudioGuideKind, BuiltInBlocks.AudioGuidePlayer, "keepoff",
                CardGalleryDefinitions.AudioBlock(BuiltInBlocks.AudioGuidePlayer, "short.mp3", null, "No keeping"),
                wallSetup: wall => wall.card_settings.container.keep_audio_on_close = false);
            System.Collections.Generic.List<AudioGuideBlockView> views = null;
            yield return ShowCustom(entry, v => views = v);
            _harness.Sheet.Stack.Scroll.ScrollTo(_harness.Sheet.Stack.SlotOf(views[0]));
            yield return CardTestInput.Settle(0.15f);
            yield return Tap(views[0].PlayButton);
            Assert.AreEqual(CardAudioState.Playing, Audio.State);
            yield return Tap(_harness.Sheet.CloseButton);
            Assert.AreEqual(CardAudioState.Idle, Audio.State, "the card closed: its audio stopped with it");
            Assert.IsFalse(Output.IsPlaying);
            Assert.IsFalse(_harness.AudioCoordinator.Mini.IsShown, "nothing left for a mini-player to show");
            Assert.AreEqual(0, _harness.Media.HeldCount, "and every clip and caption file was given back");
        }

        [UnityTest]
        public IEnumerator ClosingTheCardAndTheAudio_LeavesNoClipOrCaptionsHeld()
        {
            AudioGuideBlockView view = null;
            yield return Show("audio_guide_player_short", v => view = v);
            Assert.AreEqual(2, _harness.Media.HeldCount, "an open card holds its clip (for the length) and its captions");
            yield return Tap(view.PlayButton);
            Assert.AreEqual(2, _harness.Media.HeldCount, "the service shares the clip's one asset: the same load, counted twice");
            Assert.AreEqual(2, _harness.Media.RefCount("short.mp3"), "the block's load and the service's");
            _harness.Sheet.Hide();
            Assert.AreEqual(1, _harness.Media.RefCount("short.mp3"), "the card closed: only the playing audio holds the clip");
            Assert.AreEqual(0, _harness.Media.RefCount("short.vtt"), "the captions went with the card");
            Audio.Stop();
            Assert.AreEqual(0, _harness.Media.HeldCount, "stopped: nothing is held at all");
        }
    }
}

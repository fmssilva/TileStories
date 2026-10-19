using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // Phase B of step 12 (_3.2 section 6.2: the compact row, Display Expandable, the full view of any block) on the REAL LivingRoomScene, with
    // real taps on the real card and the card's REAL owners (the app's preview stage loading the real glb, the AR placement on the mock-
    // localised wall). Only the sound and picture DEVICES are stand-ins (ManualAudioOutput / ManualVideoOutput, as in every audio / video
    // scene test): time passes when the test says. Everything is asserted on the OWNER and the counts, never on pixels:
    //   - The Lamp with every block Expandable binds no view and holds no file in its 59 rows; a real tap opens a row (its view bound, the
    //     preview owner holds its slot), a real tap closes it (the slot and every file given back)
    //   - each owner that serves ONE block at a time, used in the full view and handed back: audio, video, preview, AR placement -- open
    //     the full view by a real tap on the open row's button, use the owner there, close it with a real tap on Back, and the card's own
    //     view of the block works again
    //   - a language switch, a live edit and Reduce Motion with the full view open: it stays, redrawn; another point closes it
    //   - renders at 390 px in English and Portuguese: the rows, one open, the full view
    public class PoiCardExpandableSceneTests : SearchSceneFixture
    {
        private const string Lamp = "lamp";
        private const string LampMilitary = "lamp_military";

        private PoiCardSheetView Sheet => Card.Sheet;
        private BlockStackView Stack => Sheet.Stack;
        private CardSettings Live => Session.CardSettings;
        private ManualAudioOutput _audioOut;
        private CardAudioService _audio;
        private ManualVideoOutput _videoOut;
        private CardVideoService _video;
        private float _now;

        [UnitySetUp]
        public IEnumerator UseStandInSoundAndPictureDevices()
        {
            _now = 0f;
            _audioOut = new ManualAudioOutput();
            _audio = new CardAudioService(_audioOut, () => Card.Media, () => _now, () => Live.container.audio_when_another_starts);
            Card.UseAudioService(_audio);
            _videoOut = new ManualVideoOutput();
            _video = new CardVideoService(_videoOut, () => Card.Media);
            Card.UseVideoService(_video);
            // - a language picked on the chip is remembered in this throwaway state, never in the developer's own PlayerPrefs
            Card.State = new CardLocalState(new MemoryCardStateStore(), Session.SearchConfig.wall_id);
            yield break;
        }

        [UnityTearDown]
        public IEnumerator LetTheStandInsGo()
        {
            Card.ArPlacement?.Remove();
            _video.CardClosed();
            _videoOut.Release();
            yield break;
        }

        // ---------------- helpers ----------------

        // The Lamp's blocks `keys` (all of them when none is named) shown Expandable, pushed through the live seam (display is content)
        private IEnumerator Expandable(params string[] keys)
        {
            var copy = ConfigCopy();
            foreach (var block in copy.pois.Single(p => p.id == Lamp).card.blocks)
                if (keys.Length == 0 || keys.Contains(block.key)) block.display = CardOptions.DisplayExpandable;
            Session.ApplyCardSettings(copy);
            yield return CardTestInput.Settle(0.3f);
        }

        private IEnumerator Push(Action<WallConfigData> edit)
        {
            var copy = ConfigCopy();
            edit(copy);
            Session.ApplyCardSettings(copy);
            yield return CardTestInput.Settle(0.3f);
        }

        private IEnumerator OpenFull(string poi)
        {
            if (SelectionEventBus.CurrentPoiId != poi)
            {
                SelectionEventBus.Select(poi);
                yield return CardTestInput.Settle();
            }
            Sheet.SetStop(SheetStopRule.Stop.Full);
            yield return CardTestInput.Settle();
            Assert.AreEqual(SheetStopRule.Stop.Full, Sheet.Stop, "precondition: the card rests at full");
        }

        // Scroll `target` into the card's view and wait until it holds still there: a long scroll collapses the header, which moves the
        // content for a few frames after it -- a tap sent while it moves can land its finger-up on another element (the click is lost), and
        // a scroll asked before the stack's new range is laid out is clamped short. Found by this fixture (two taps lost in one run, both
        // green alone): the precondition is a target in view AND still for three frames, not a fixed wait
        private IEnumerator InViewAndStill(VisualElement target)
        {
            var scroll = Stack.Scroll;
            Rect last = default;
            int still = 0;
            for (int frame = 0; frame < 240 && still < 3; frame++)
            {
                if (!scroll.contentViewport.worldBound.Contains(target.worldBound.center)) scroll.ScrollTo(target);
                yield return null;
                var now = target.worldBound;
                still = now == last && scroll.contentViewport.worldBound.Contains(now.center) ? still + 1 : 0;
                last = now;
            }
            Assert.IsTrue(scroll.contentViewport.worldBound.Contains(target.worldBound.center),
                "precondition: " + target.name + " is inside the visible part of the card: " + target.worldBound + " in " + scroll.contentViewport.worldBound);
            Assert.AreEqual(3, still, "precondition: " + target.name + " holds still before the tap");
        }

        // A real tap on an element of the card's stack, once it is in view and still
        private IEnumerator TapInStack(VisualElement target)
        {
            yield return InViewAndStill(target);
            yield return CardTestInput.Tap(target.panel, target.worldBound.center);
            yield return null;
        }

        // A real tap on a compact row's line (scrolled into view first)
        private IEnumerator TapRow(string key)
        {
            var row = Stack.CompactRowOf(key);
            Assert.IsNotNull(row, "precondition: " + key + " is drawn as a compact row");
            yield return TapInStack(row.Toggle);
        }

        // A real tap on an open row's full-view button: the block in the full view
        private IEnumerator TapFullView(string key)
        {
            var row = Stack.CompactRowOf(key);
            Assert.IsTrue(row.IsExpanded, "precondition: " + key + "'s row is open (its button shows only then)");
            yield return TapInStack(row.FullView);
            Assert.IsTrue(Sheet.FullView.IsOpen, "a real tap on the button opened the full view");
            Assert.AreEqual(key, Sheet.FullView.BlockKey);
        }

        // A real tap on something in the full view (it is not in the card's scroll)
        private static IEnumerator Tap(VisualElement e)
        {
            Assert.IsTrue(e.panel != null && e.worldBound.width > 0f, "precondition: " + e.name + " is laid out on the panel");
            yield return CardTestInput.Tap(e.panel, e.worldBound.center);
            yield return null;
        }

        private IEnumerator TapBack()
        {
            yield return Tap(Sheet.Takeover.Back);
            Assert.IsFalse(Sheet.FullView.IsOpen, "a real tap on Back closed the full view");
        }

        private void Advance(float seconds)
        {
            _now += seconds;
            _audioOut.Advance(seconds);
            _audio.Tick();
            _videoOut.Advance(seconds);
            _video.Tick();
        }

        private static IEnumerator WaitFor(Func<bool> condition, string what, int frames = 900)
        {
            for (int i = 0; i < frames && !condition(); i++) yield return null;
            Assert.IsTrue(condition(), "timed out waiting for " + what);
        }

        // ---------------- the acceptance: count what is bound ----------------

        [UnityTest]
        public IEnumerator TheLamp_AllExpandable_HoldsNoMediaUntilARealTapOpensARow_AndARealTapClosesIt_GivingTheOwnerItsSlotBack()
        {
            yield return Expandable();
            yield return OpenFull(Lamp);
            var keys = LampKeys();
            var rows = keys.Where(k => Stack.CompactRowOf(k) != null).ToList();
            Assert.AreEqual(59, rows.Count, "every scrolling block of The Lamp is a row (62 less the header, the pinned chip, the footer CTA)");
            Assert.AreEqual(3, Stack.BoundViews.Count, "only the header, the hero chip and the sticky CTA bind a view");
            foreach (string key in rows)
            {
                Assert.IsNull(Stack.ViewOfBlock(key), key + ": no view");
                Assert.AreEqual(0, Stack.HeldMediaOf(key), key + ": no file");
            }
            CollectionAssert.IsEmpty(Card.PreviewService.HeldKeys, "no 3D / 360 slot held: the two models and two panoramas wait in their rows");
            Assert.AreEqual(CardVideoState.Idle, _video.State, "no video");
            Assert.IsNull(_videoOut.Clip, "and no clip handed to the device");
            int files = Card.Media.LoadedCount;
            Assert.AreEqual(Stack.HeldMedia, files, "every file the card's media source holds belongs to the three whole blocks");

            // - a real tap on the inline model's row: its view, its preview slot, its fallback picture
            yield return TapRow("block_59");
            Assert.IsTrue(Stack.IsExpanded("block_59"), "the tap opened it");
            var model = Stack.ViewOfBlock("block_59") as ModelTurntableBlockView;
            Assert.IsNotNull(model, "the model's own view, bound under the row");
            CollectionAssert.AreEquivalent(new[] { "block_59" }, Card.PreviewService.HeldKeys, "the preview owner holds the block's slot");
            yield return WaitFor(() => model.ShowsSubject, "the real glb to load into the slot");

            // - a real tap closes it: the owner and the card's media give everything back
            yield return TapRow("block_59");
            Assert.IsFalse(Stack.IsExpanded("block_59"), "the second tap closed it");
            CollectionAssert.IsEmpty(Card.PreviewService.HeldKeys, "the slot released");
            Assert.AreEqual(0, Stack.HeldMediaOf("block_59"));
            Assert.AreEqual(files, Card.Media.LoadedCount, "the card holds exactly what it held before the row opened");
        }

        // ---------------- one owner at a time: handed over to the full view and back ----------------

        [UnityTest]
        public IEnumerator Audio_PlayedInTheFullView_IsWhatTheCardsPlayerShowsAfter_AndTheCardsPlayerStillControlsIt()
        {
            yield return Expandable("block_55");
            yield return OpenFull(Lamp);
            yield return TapRow("block_55");
            var cards = (AudioGuideBlockView)Stack.ViewOfBlock("block_55");
            yield return TapFullView("block_55");
            var full = (AudioGuideBlockView)Sheet.FullView.View;
            Assert.AreNotSame(cards, full, "a second view of the player");
            Assert.IsTrue(full.Track.SameAudio(cards.Track), "of the same audio");
            Assert.AreEqual(CardAudioState.Idle, _audio.State);

            yield return Tap(full.PlayButton);
            Assert.AreEqual(CardAudioState.Playing, _audio.State, "a real tap in the full view plays it");
            Assert.IsTrue(_audio.IsCurrent(cards.Track));
            Advance(3f);
            yield return TapBack();
            Assert.AreEqual(CardAudioState.Playing, _audio.State, "closing the full view does not stop the audio: the card's player shows it");
            Assert.GreaterOrEqual(_audio.Position, 3f, "where it was");
            Assert.AreSame(cards, Stack.ViewOfBlock("block_55"), "the card's player was bound all along");

            yield return TapInStack(cards.PlayButton);
            Assert.AreEqual(CardAudioState.Paused, _audio.State, "the card's own player answers: a real tap pauses it");
            yield return TapInStack(cards.PlayButton);
            Assert.AreEqual(CardAudioState.Playing, _audio.State, "and resumes it");
        }

        [UnityTest]
        public IEnumerator Video_PlayedInTheFullView_IsWhatTheCardsPanelShowsAfter_AndTheCardsPanelStillControlsIt()
        {
            yield return Expandable("block_56");
            yield return OpenFull(Lamp);
            yield return TapRow("block_56");
            var cards = (VideoBlockView)Stack.ViewOfBlock("block_56");
            yield return TapFullView("block_56");
            var full = (VideoBlockView)Sheet.FullView.View;
            Assert.AreNotSame(cards, full);
            Assert.AreEqual(DisplayStyle.None, full.Panel.FullScreenButton.style.display.value, "no full screen to open from the full view");

            yield return Tap(full.Panel.PlayOverlay);
            Assert.AreEqual(CardVideoState.Playing, _video.State, "a real tap in the full view plays it");
            Advance(2f);
            yield return null;
            Assert.IsTrue(full.Panel.ShowsFrames, "the full view draws the playback");
            yield return TapBack();
            Assert.AreEqual(CardVideoState.Playing, _video.State, "closing the full view keeps the one playback");
            Assert.IsTrue(_video.IsCurrent(cards.Track));
            Advance(1f);
            yield return null;
            Assert.IsTrue(cards.Panel.ShowsFrames, "the card's own panel shows the same video now");
            Assert.GreaterOrEqual(_video.Position, 3f, "at the moment it reached");

            yield return TapInStack(cards.Panel.PlayButton);
            Assert.AreEqual(CardVideoState.Paused, _video.State, "the card's panel answers: a real tap pauses it");
            yield return TapInStack(cards.Panel.PlayButton);
            Assert.AreEqual(CardVideoState.Playing, _video.State);
            // - closing its row: nothing left to watch it on
            yield return TapRow("block_56");
            Assert.AreEqual(CardVideoState.Paused, _video.State, "a real tap closing the row paused the video");
        }

        [UnityTest]
        public IEnumerator Preview_TheFullViewTakesASlotOfItsOwn_ClosingGivesOnlyThatBack_AndTheCardsModelStillTurns()
        {
            yield return Expandable("block_59");
            yield return OpenFull(Lamp);
            yield return TapRow("block_59");
            var cards = (ModelTurntableBlockView)Stack.ViewOfBlock("block_59");
            yield return WaitFor(() => cards.ShowsSubject, "the card's model");
            // - The Lamp's other 3D / 360 blocks are inline and hold their own slots: the owner's whole set is compared, before and after
            var held = Card.PreviewService.HeldKeys.ToList();
            CollectionAssert.Contains(held, "block_59", "precondition: the open row holds its slot");
            yield return TapFullView("block_59");
            var full = (ModelTurntableBlockView)Sheet.FullView.View;
            CollectionAssert.AreEquivalent(held.Append("block_59::takeover"), Card.PreviewService.HeldKeys,
                "the block bound twice holds TWO slots: the card's untouched, the full view's own");
            yield return WaitFor(() => full.ShowsSubject, "the full view's model");
            Assert.IsTrue(full.Root.ClassListContains(PreviewBlockView.FullClass), "its stage takes the page's height");
            Assert.Greater(full.Frame.worldBound.height, cards.Frame.worldBound.height, "taller than the card's stage");

            var before = cards.State;
            float yaw = full.State.Yaw;
            yield return CardTestInput.DragFrom(full.Frame.panel, full.Frame.worldBound.center, new Vector2(120f, 0f));
            Assert.AreNotEqual(yaw, full.State.Yaw, "a real drag turns the full view's model");
            Assert.AreEqual(before.Yaw, cards.State.Yaw, "and never the card's");

            yield return TapBack();
            CollectionAssert.AreEquivalent(held, Card.PreviewService.HeldKeys, "closing gave back ONLY the full view's slot");
            Assert.IsTrue(cards.ShowsSubject, "the card's model is still drawn");
            yield return InViewAndStill(cards.Frame);
            float cardYaw = cards.State.Yaw;
            yield return CardTestInput.DragFrom(cards.Frame.panel, cards.Frame.worldBound.center, new Vector2(120f, 0f));
            Assert.AreNotEqual(cardYaw, cards.State.Yaw, "and a real drag turns it: the card's own view works again");
        }

        [UnityTest]
        public IEnumerator ArPlacement_PlacedFromTheFullView_LowersTheCardToTheRoom_AndTheCardsButtonRemovesIt()
        {
            Assert.IsTrue(Session.GetComponent<MockLocalizationProvider>().IsLocalised, "precondition: the mock has localised the wall");
            yield return Expandable("block_62");
            yield return OpenFull(Lamp);
            yield return TapRow("block_62");
            var cards = (PlaceInArBlockView)Stack.ViewOfBlock("block_62");
            yield return TapFullView("block_62");
            var full = (PlaceInArBlockView)Sheet.FullView.View;
            Assert.AreNotSame(cards, full);

            yield return Tap(full.Button);
            Assert.IsNotNull(Card.ArPlacementService.Placed, "a real tap in the full view placed the model in the room");
            Assert.IsFalse(Sheet.FullView.IsOpen, "the full view went with the card: the room is what to look at");
            Assert.AreEqual(SheetStopRule.Stop.Peek, Sheet.Stop, "the card lowered to its title");
            Assert.IsTrue(Card.ArPlacement.IsPlaced(Lamp, "block_62"));

            Sheet.SetStop(SheetStopRule.Stop.Full);
            yield return CardTestInput.Settle();
            Assert.AreSame(cards, Stack.ViewOfBlock("block_62"), "the card's own view was bound all along");
            Assert.AreEqual("Remove from room", cards.ButtonLabel.text, "it shows the one placed model");
            yield return TapInStack(cards.Button);
            Assert.IsNull(Card.ArPlacementService.Placed, "the card's button removes it: the owner answers the card again");
        }

        // ---------------- the full view survives a rebind ----------------

        [UnityTest]
        public IEnumerator TheFullView_SurvivesTheLanguageSwitch_ALiveEdit_AndReduceMotion_WithItsVideoPlaying_AndAnotherPointClosesIt()
        {
            yield return Expandable("block_56");
            yield return OpenFull(Lamp);
            yield return TapRow("block_56");
            yield return TapFullView("block_56");
            yield return Tap(((VideoBlockView)Sheet.FullView.View).Panel.PlayOverlay);
            Advance(2f);
            Assert.AreEqual(CardVideoState.Playing, _video.State, "precondition: playing in the full view");
            StringAssert.EndsWith("> On film", Sheet.Takeover.Crumb.text);

            // - the language chip (covered by the full view for a finger: pressed through the panel, the submit path a tap ends in)
            Press(Sheet.LanguageButton);
            yield return CardTestInput.Settle(0.3f);
            Assert.IsTrue(Sheet.FullView.IsOpen, "the language switch kept the full view");
            Assert.AreEqual("block_56", Sheet.FullView.BlockKey);
            StringAssert.EndsWith("> Em vídeo", Sheet.Takeover.Crumb.text, "redrawn in Portuguese");
            Assert.AreEqual(CardVideoState.Playing, _video.State, "the video plays on");
            Assert.IsTrue(_video.IsCurrent(((VideoBlockView)Sheet.FullView.View).Track), "the full view draws it again");
            Assert.IsTrue(Stack.IsExpanded("block_56"), "and the card's row is still open under it");

            yield return Push(c =>
            {
                var heading = c.pois.Single(p => p.id == Lamp).card.blocks.Single(b => b.key == "block_56").fields.Single(f => f.key == BlockKindDefinition.HeadingField);
                heading.text.Single(t => t.lang == "pt").value = "Em filme";
            });
            Assert.IsTrue(Sheet.FullView.IsOpen, "a live edit kept it");
            StringAssert.EndsWith("> Em filme", Sheet.Takeover.Crumb.text, "with the edited words");

            yield return Push(c => c.card_settings.container.reduce_motion = true);
            Assert.IsTrue(Sheet.FullView.IsOpen, "Reduce Motion switched on live kept it");
            Assert.AreEqual(CardVideoState.Playing, _video.State, "the visitor's own video plays on (Reduce Motion stills only the header's loop)");

            SelectionEventBus.Select(LampMilitary);
            yield return CardTestInput.Settle();
            Assert.IsFalse(Sheet.FullView.IsOpen, "another point closes the old card's full view");
            Assert.IsFalse(Sheet.Takeover.IsOpen);
            Assert.AreNotEqual(CardVideoState.Playing, _video.State, "and its video stops with the old card");
        }

        // ---------------- what it looks like ----------------

        [UnityTest]
        public IEnumerator Renders_TheRows_OneOpen_AndTheFullView_InEnglishAndPortuguese()
        {
            yield return Expandable();
            foreach (string language in new[] { "en", "pt" })
            {
                if (language == "pt")
                {
                    Press(Sheet.LanguageButton);
                    yield return CardTestInput.Settle(0.3f);
                }
                yield return OpenFull(Lamp);
                Stack.Scroll.scrollOffset = Vector2.zero;
                yield return CardTestInput.Settle(0.2f);
                foreach (var row in LampKeys().Select(Stack.CompactRowOf).Where(r => r != null))
                {
                    if (!row.Root.worldBound.Overlaps(Stack.Scroll.contentViewport.worldBound)) continue;
                    Assert.IsTrue(UIAccessibility.MeetsMinTapTarget(row.Toggle.worldBound.width, row.Toggle.worldBound.height), row.Words.text + ": a whole line to tap");
                    Assert.LessOrEqual(row.Words.worldBound.xMax, row.Chevron.worldBound.xMin + 0.5f, row.Words.text + ": the words end before the chevron");
                    Assert.GreaterOrEqual(CardTestInput.Contrast(row.Words.resolvedStyle.color, CardTestInput.EffectiveBackground(row.Words)),
                        UIAccessibility.MinRatioNormalText, row.Words.text + ": the words read");
                    Assert.GreaterOrEqual(CardTestInput.Contrast(row.Meta.resolvedStyle.color, CardTestInput.EffectiveBackground(row.Meta)),
                        UIAccessibility.MinRatioNormalText, row.Meta.text + ": the format reads");
                }
                yield return Capture("Expandable_Lamp_rows_" + language);

                yield return TapRow("block_2");
                var open = Stack.CompactRowOf("block_2");
                Assert.IsTrue(UIAccessibility.MeetsMinTapTarget(open.FullView.worldBound.width, open.FullView.worldBound.height), "the full-view button is a whole target");
                Stack.Scroll.ScrollTo(Stack.SlotOfBlock("block_2"));
                yield return CardTestInput.Settle(0.2f);
                yield return Capture("Expandable_Lamp_open_" + language);

                yield return TapFullView("block_2");
                yield return CardTestInput.Settle(0.2f);
                yield return Capture("Expandable_Lamp_fullview_" + language);
                yield return TapBack();
                yield return TapRow("block_2");
            }
        }

        // The Lamp's block keys in the stack's order
        private List<string> LampKeys()
        {
            var keys = new List<string>();
            foreach (var block in ConfigCopy().pois.Single(p => p.id == Lamp).card?.blocks ?? new List<BlockInstanceData>())
                if (Stack.SlotOfBlock(block.key) != null) keys.Add(block.key);
            return keys;
        }
    }
}

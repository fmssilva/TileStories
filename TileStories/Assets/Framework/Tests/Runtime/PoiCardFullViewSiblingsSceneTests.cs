using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // Phase B of step 16 (_3.2 section 5.7: the full view's breadcrumb up the ontology and the sibling swipe, F4) on the REAL LivingRoomScene with
    // real touches, a real Escape and the card's REAL owners (the preview stage loading the real glb and panorama; the video owner over a stand-in
    // picture device, ManualVideoOutput: time passes when the test says). Asked of the OWNERS, per owner, as step 12 did:
    //   - VIDEO: Lamp - Residential's castle video played in the full view, a REAL swipe to the next video -- the first pauses and its full-view binding is
    //     gone, the owner holds nothing new; a real tap plays the second: the owner FOLLOWED (the second current, the first not)
    //   - PREVIEW: the 3D model in the full view holds its own `::takeover` slot; a real tap on the next chip hands it to the next model's own
    //     slot (the first given back, the card's own slots untouched), and on to the panorama's
    //   - a real swipe and a real chip tap move between siblings and the breadcrumb follows; a REAL Escape returns to the CARD at the block the
    //     visitor came from (the scroll unmoved), never to the previous sibling (NN/g, Budiu 2017)
    //   - the breadcrumb's section pill, tapped for real, closes the full view and moves the card to that section (one nav_jump, "breadcrumb")
    //   - a family of one (Listen) draws no chips, and a real swipe there turns nothing
    //   - renders: the Media full view with the breadcrumb and the chips, EN and PT, and Listen's with none
    public class PoiCardFullViewSiblingsSceneTests : SearchSceneFixture
    {
        private const string Lamp = "lamp";
        // - the point with two DIFFERENT clips in one family (The Lamp's two videos share one clip: to the one owner they are the same video)
        private const string Residential = "lamp_residential";

        private PoiCardSheetView Sheet => Card.Sheet;
        private BlockStackView Stack => Sheet.Stack;
        private NavController Nav => Sheet.Nav;
        private ScrollView Scroll => Stack.Scroll;
        private TakeoverView Takeover => Sheet.Takeover;
        private float OnePixel => Sheet.Layer.panel.visualTree.layout.height / Screen.height;

        private sealed class RecordingCardEvents : ICardEvents
        {
            public readonly List<CardEvent> Raised = new();
            public void Raise(CardEvent cardEvent) => Raised.Add(cardEvent);
        }

        private RecordingCardEvents _events;
        private ManualVideoOutput _videoOut;
        private CardVideoService _video;
        private BackgroundSafeInput _backgroundInput;
        private Keyboard _keyboard;
        private List<string> _shippedLanguages;
        private List<CardEvent> NavJumps => _events.Raised.Where(e => e.Kind == CardEventKinds.NavJump).ToList();

        [UnitySetUp]
        public IEnumerator UseStandInDevicesAndRecordTheEvents()
        {
            _videoOut = new ManualVideoOutput();
            _video = new CardVideoService(_videoOut, () => Card.Media);
            Card.UseVideoService(_video);
            Card.State = new CardLocalState(new MemoryCardStateStore(), Session.SearchConfig.wall_id);
            _events = new RecordingCardEvents();
            Card.Events = _events;
            yield break;
        }

        [UnityTearDown]
        public IEnumerator LetTheStandInsGo()
        {
            _video.CardClosed();
            _videoOut.Release();
            if (_keyboard != null) InputSystem.RemoveDevice(_keyboard);
            _keyboard = null;
            _backgroundInput?.Dispose();
            _backgroundInput = null;
            yield break;
        }

        // ---------------- helpers ----------------

        // The shipped wall pushed through the live seam in `language` (the wall's first language)
        private IEnumerator UseLanguage(string language)
        {
            var copy = ConfigCopy();
            _shippedLanguages ??= new List<string>(ConfigCopy().card_settings.languages);
            copy.card_settings.languages = language == "en" ? new List<string>(_shippedLanguages) : new List<string> { language, "en" };
            Session.ApplyCardSettings(copy);
            yield return CardTestInput.Settle(0.3f);
        }

        private IEnumerator OpenFull(string poi)
        {
            if (SelectionEventBus.CurrentPoiId != poi)
            {
                SelectionEventBus.Clear();
                yield return CardTestInput.Settle();
                SelectionEventBus.Select(poi);
                yield return CardTestInput.Settle();
            }
            Sheet.SetStop(SheetStopRule.Stop.Full);
            yield return CardTestInput.Settle();
            Assert.AreEqual(SheetStopRule.Stop.Full, Sheet.Stop, "precondition: the card rests at full");
            float rest = Sheet.Stops.HeightOf(Sheet.Stop);
            for (int frame = 0; frame < 600 && Mathf.Abs(Sheet.Root.layout.height - rest) > OnePixel; frame++) yield return null;
            yield return CardTestInput.Settle(0.2f);
        }

        // Scroll the CARD until `target` is inside its visible part and still for three frames
        private IEnumerator InViewAndStill(VisualElement target)
        {
            Rect last = default;
            int still = 0;
            for (int frame = 0; frame < 240 && still < 3; frame++)
            {
                if (!Scroll.contentViewport.worldBound.Contains(target.worldBound.center)) Scroll.ScrollTo(target);
                yield return null;
                var now = target.worldBound;
                still = now == last && Scroll.contentViewport.worldBound.Contains(now.center) ? still + 1 : 0;
                last = now;
            }
            Assert.AreEqual(3, still, "precondition: " + target.name + " is in view and still");
        }

        // Open `key` in the full view through the host seam a bento tile and an open row's button use (IBlockHost.OpenFullView)
        private IEnumerator OpenFullViewOf(string key)
        {
            Sheet.OpenFullView(key);
            yield return CardTestInput.Settle(0.3f);
            Assert.IsTrue(Sheet.FullView.IsOpen, "precondition: " + key + " is open in the full view");
            Assert.AreEqual(key, Sheet.FullView.BlockKey);
        }

        // A real sideways swipe across the full view's page, from 75% of its width to 15% (the finger moving LEFT brings the next sibling)
        private IEnumerator SwipeThePage(int direction, float atHeight = 0.5f)
        {
            var page = Sheet.FullView.Scroll.contentViewport.worldBound;
            float from = direction > 0 ? 0.75f : 0.25f;
            var start = new Vector2(page.xMin + page.width * from, page.yMin + page.height * atHeight);
            yield return CardTestInput.TouchDrag(Sheet.Layer.panel, start, new Vector2(-direction * page.width * 0.6f, 0f), frames: 12);
            yield return CardTestInput.Settle(0.3f);
        }

        // A real tap on the chip of page `page`, brought into the strip's view first
        private IEnumerator TapChip(int page)
        {
            var chip = Takeover.Chips[page];
            for (int frame = 0; frame < 60 && !Takeover.ChipStrip.worldBound.Contains(chip.worldBound.center); frame++)
            {
                // - the strip keeps the CURRENT chip in view; step towards the target through the chips that are, as a visitor would
                int step = page > Takeover.PageIndex ? 1 : -1;
                var visible = Takeover.Chips.Children().Where(c => Takeover.ChipStrip.worldBound.Contains(c.worldBound.center)).ToList();
                var towards = step > 0 ? visible.Last() : visible.First();
                yield return CardTestInput.Tap(towards.panel, towards.worldBound.center);
                yield return CardTestInput.Settle(0.2f);
            }
            Assert.IsTrue(Takeover.ChipStrip.worldBound.Contains(chip.worldBound.center), "precondition: chip " + page + " is in the strip's view");
            yield return CardTestInput.Tap(chip.panel, chip.worldBound.center);
            yield return CardTestInput.Settle(0.3f);
        }

        private IEnumerator PressEscape()
        {
            if (_keyboard == null)
            {
                _backgroundInput = new BackgroundSafeInput();
                _keyboard = BackgroundSafeInput.AddEnabled<Keyboard>();
            }
            InputSystem.QueueStateEvent(_keyboard, new KeyboardState(Key.Escape));
            yield return null;
            InputSystem.QueueStateEvent(_keyboard, new KeyboardState());
            yield return CardTestInput.Settle(0.3f);
        }

        private List<string> Siblings(string key) => FullViewSiblingRule.Siblings(Nav.Model, BlockRegistry.Shared, key).ToList();
        private string MediaWords => Sheet.Takeover.CrumbSectionWords.text;

        // ---------------- tests ----------------

        // THE RISK of the step, on the video owner: a block played in the full view, a REAL swipe hands the page to the next video
        [UnityTest]
        public IEnumerator Video_PlayedInTheFullView_ARealSwipeToTheNextVideo_PausesTheFirst_AndTheOwnerFollowsTheSecond()
        {
            yield return OpenFull(Residential);
            var siblings = Siblings("block_4");
            Assert.AreEqual(siblings.IndexOf("block_4") + 1, siblings.IndexOf("block_7"), "precondition: the second video is the next sibling");
            // - the visitor reads the first video on the card, then opens it full screen
            yield return InViewAndStill(Stack.SlotOfBlock("block_4"));
            float cardOffset = Scroll.scrollOffset.y;
            yield return OpenFullViewOf("block_4");

            var first = (VideoBlockView)Sheet.FullView.View;
            var firstTrack = first.Track;
            yield return CardTestInput.Tap(first.Panel.PlayOverlay.panel, first.Panel.PlayOverlay.worldBound.center);
            yield return null;
            Assert.AreEqual(CardVideoState.Playing, _video.State, "a real tap played the first video in the full view");
            Assert.IsTrue(_video.IsCurrent(firstTrack));

            yield return SwipeThePage(+1);
            Assert.AreEqual("block_7", Sheet.FullView.BlockKey, "a real swipe brought the next video");
            StringAssert.EndsWith(" > " + MediaWords + " > " + Nav.RowOf("block_7").Words, Takeover.CrumbText, "the breadcrumb follows");
            Assert.AreEqual(CardVideoState.Paused, _video.State, "the first PAUSED: the next sibling covers it, nothing would show or stop it");
            Assert.IsTrue(_video.IsCurrent(firstTrack), "the one owner holds the one paused clip (section 7.1), nothing new");
            var second = (VideoBlockView)Sheet.FullView.View;
            var secondTrack = second.Track;
            Assert.IsNotNull(secondTrack, "the second video's view is bound");
            Assert.IsFalse(firstTrack.SameVideo(secondTrack), "precondition: two different clips (the castle, the framework's tile pattern)");

            yield return CardTestInput.Tap(second.Panel.PlayOverlay.panel, second.Panel.PlayOverlay.worldBound.center);
            yield return null;
            Assert.AreEqual(CardVideoState.Playing, _video.State, "a real tap played the second video");
            Assert.IsTrue(_video.IsCurrent(secondTrack), "the owner FOLLOWED: the second is current");
            Assert.IsFalse(_video.IsCurrent(firstTrack), "and the first released");

            // - Back: the CARD, at the block the visitor came from -- never the previous sibling
            yield return PressEscape();
            Assert.IsFalse(Takeover.IsOpen, "a real Escape closed the full view");
            Assert.AreEqual(SheetStopRule.Stop.Full, Sheet.Stop, "the card did not move down");
            Assert.AreEqual(cardOffset, Scroll.scrollOffset.y, OnePixel, "the card's scroll is where it was");
            Assert.IsTrue(Scroll.contentViewport.worldBound.Contains(Stack.SlotOfBlock("block_4").worldBound.center),
                "the block the visitor came from is still in view");
        }

        // The preview owner: each 3D / 360 block in the full view takes its OWN `::takeover` slot; a real chip tap hands it on
        [UnityTest]
        public IEnumerator Preview_ARealChipTapHandsTheFullViewSlotToTheNextModel_AndOnToThePanorama_TheCardsOwnSlotsUntouched()
        {
            yield return OpenFull(Lamp);
            var siblings = Siblings("block_58");
            var held = Card.PreviewService.HeldKeys.ToList();
            Assert.IsFalse(held.Any(k => k.EndsWith("::takeover")), "precondition: no full-view slot yet");
            yield return OpenFullViewOf("block_58");
            CollectionAssert.AreEquivalent(held.Append("block_58::takeover"), Card.PreviewService.HeldKeys, "the model's own full-view slot");

            int next = siblings.IndexOf("block_59");
            Assert.AreEqual(siblings.IndexOf("block_58") + 1, next, "precondition: the second model is the next sibling");
            yield return TapChip(next);
            Assert.AreEqual("block_59", Sheet.FullView.BlockKey, "a real tap on its chip");
            CollectionAssert.AreEquivalent(held.Append("block_59::takeover"), Card.PreviewService.HeldKeys,
                "the slot handed on: the next model's own, the first given back, the card's slots untouched");
            Assert.IsTrue(Takeover.Chips[next].ClassListContains(TakeoverView.CurrentChipClass), "its chip marked");

            int panorama = siblings.IndexOf("block_60");
            yield return TapChip(panorama);
            Assert.AreEqual("block_60", Sheet.FullView.BlockKey);
            CollectionAssert.AreEquivalent(held.Append("block_60::takeover"), Card.PreviewService.HeldKeys, "and on to the panorama's own slot");

            yield return PressEscape();
            Assert.IsFalse(Takeover.IsOpen);
            CollectionAssert.AreEquivalent(held, Card.PreviewService.HeldKeys, "Back gave back ONLY the full view's slot");
        }

        // The breadcrumb up the ontology, and its section pill: a real tap moves the card to the section through the card's one jump
        [UnityTest]
        public IEnumerator TheBreadcrumbsSectionPill_ARealTap_ClosesTheFullView_AndMovesTheCardToThatSection()
        {
            yield return OpenFull(Lamp);
            int media = Nav.Model.Sections.ToList().FindIndex(s => s.Key == BuiltInFamilies.Media);
            yield return OpenFullViewOf("block_60");
            Assert.AreEqual(Stack.HeaderView.TitleText + " > " + MediaWords + " > " + Nav.RowOf("block_60").Words, Takeover.CrumbText,
                "the card, its section, the block (\"Castle > Media > 360\")");
            var pill = Takeover.CrumbSection;
            Assert.IsTrue(UIAccessibility.MeetsMinTapTarget(pill.worldBound.width, pill.worldBound.height), "the pill is a whole touch target");
            _events.Raised.Clear();
            yield return CardTestInput.Tap(pill.panel, pill.worldBound.center);
            yield return CardTestInput.Settle(0.6f);
            Assert.IsFalse(Takeover.IsOpen, "a real tap on the pill closed the full view");
            var jump = NavJumps.Single();
            Assert.AreEqual(TakeoverView.CrumbElementKey, jump.Variant, "one nav_jump, reported by the breadcrumb");
            Assert.AreEqual(BuiltInFamilies.Media, jump.Value, "to Media");
            Assert.AreEqual(media, Nav.Current, "the card is at Media");
            string firstOfMedia = Nav.Model.Sections[media].Targets[0];
            Assert.AreEqual(Scroll.contentViewport.worldBound.yMin + Stack.LandingInset, Stack.LandingElement(Stack.SlotOfBlock(firstOfMedia)).worldBound.yMin, OnePixel,
                "Media's chapter (its sheet, its first block's landing: _3.3 P4.4c1) at the top of the card");
        }

        // _3.2 step 17a: every icon of the card's set at three times its size, once on the surface of an unread chip and once on the accent fill of
        // the chip being read -- a contact sheet for the eye (the new kind icons: do they draw, do they read, do they survive the on-accent colour)
        [UnityTest]
        public IEnumerator TheCardsIcons_RenderAsOneSheet_OnAnUnreadChipAndOnTheChipBeingRead()
        {
            var sheet = new VisualElement { name = "icon-contact-sheet" };
            sheet.AddToClassList("ts-card");
            sheet.AddToClassList("ts-theme-default");
            sheet.AddToClassList("card-takeover");
            sheet.style.position = Position.Absolute;
            sheet.style.left = sheet.style.top = sheet.style.right = sheet.style.bottom = 0f;
            sheet.style.flexDirection = FlexDirection.Row;
            sheet.style.flexWrap = Wrap.Wrap;
            sheet.style.alignContent = Align.FlexStart;
            sheet.style.paddingTop = 40f;
            Sheet.Layer.Add(sheet);
            try
            {
                foreach (bool current in new[] { false, true })
                    foreach (string key in CardIcons.All)
                    {
                        var cell = new VisualElement();
                        cell.style.width = 54f;
                        cell.style.alignItems = Align.Center;
                        cell.style.marginBottom = 8f;
                        var chip = new VisualElement();
                        chip.AddToClassList("card-takeover__chip");
                        chip.AddToClassList(TakeoverView.IconChipClass);
                        if (current) chip.AddToClassList(TakeoverView.CurrentChipClass);
                        chip.style.width = chip.style.height = 46f;
                        var icon = CardIcons.Create();
                        CardIcons.SetKey(icon, key);
                        icon.style.scale = new Scale(new Vector3(1.6f, 1.6f, 1f));
                        chip.Add(icon);
                        var label = new Label(key) { pickingMode = PickingMode.Ignore };
                        label.style.fontSize = 9f;
                        label.style.color = Color.white;
                        cell.Add(chip);
                        cell.Add(label);
                        sheet.Add(cell);
                    }
                yield return CardTestInput.Settle(0.3f);
                yield return CardGalleryChecks.Render("Card_icons_sheet_390");
            }
            finally
            {
                sheet.RemoveFromHierarchy();
            }
        }

        [UnityTest]
        public IEnumerator AFamilyOfOne_DrawsNoChips_AndARealSwipeThereTurnsNothing_AndTheMediaFullViewRendersInBothLanguages()
        {
            foreach (string language in new[] { "en", "pt" })
            {
                yield return UseLanguage(language);
                yield return OpenFull(Lamp);
                yield return OpenFullViewOf("block_58");
                Assert.Greater(Takeover.PageCount, 1, "precondition: Media has siblings");
                // - every opening after the first reuses the chips: the current one must still be centred in view (the PT render found it out
                //   of view, measured against the chips' stale layout), with its neighbours a tap away
                var strip = Takeover.ChipStrip.worldBound;
                int at = Takeover.PageIndex;
                foreach (int i in new[] { at - 1, at, at + 1 })
                    Assert.IsTrue(strip.Contains(Takeover.Chips[i].worldBound.center), language + ": chip " + i + " in view around the current " + at
                        + " (" + Takeover.Chips[i].worldBound + " in " + strip + ")");
                // - _3.2 step 17a: a chip is its block's KIND icon, not a number; the one being read says what it is, its words WHOLE at 390 px
                //   (no ellipsis for The Lamp's headings), and every chip is still a whole touch target. Named by the block's words for a reader
                var siblingKeys = Siblings("block_58");
                for (int i = 0; i < Takeover.PageCount; i++)
                {
                    var chip = Takeover.Chips[i];
                    Assert.IsTrue(Sheet.Stack.TryGetEntryOfBlock(siblingKeys[i], out var entry), "precondition: the sibling is on the card");
                    BlockRegistry.Shared.TryGet(entry.Instance.kind, out var kind);
                    Assert.AreEqual(kind.IconKey, CardIcons.KeyOf(Takeover.ChipIcon(i)), language + ": chip " + i + " wears its kind's icon");
                    Assert.AreEqual(Nav.RowOf(siblingKeys[i]).Words, chip.tooltip, language + ": chip " + i + " is named by its block's words");
                    Assert.IsTrue(UIAccessibility.MeetsMinTapTarget(chip.worldBound.width, chip.worldBound.height),
                        language + ": chip " + i + " is a whole touch target (" + chip.worldBound.size + ")");
                }
                var words = Takeover.ChipWords(at);
                Assert.AreEqual(DisplayStyle.Flex, words.resolvedStyle.display, language + ": the chip being read shows its words");
                Assert.AreEqual(Nav.RowOf(siblingKeys[at]).Words, words.text);
                float natural = words.MeasureTextSize(words.text, 0f, VisualElement.MeasureMode.Undefined, 0f, VisualElement.MeasureMode.Undefined).x;
                Assert.LessOrEqual(natural, words.layout.width + 1f, language + ": the current chip's words are whole, cut by no ellipsis (" + natural
                    + " px of text in a " + words.layout.width + " px label)");
                Assert.IsTrue(strip.Contains(Takeover.Chips[at].worldBound.min) && strip.Contains(Takeover.Chips[at].worldBound.max),
                    language + ": the whole current chip is inside the strip at 390 px (" + Takeover.Chips[at].worldBound + " in " + strip + ")");
                yield return CardGalleryChecks.Render("FullView_Lamp_media_" + language + "_390");
                Assert.IsTrue(Sheet.Back());
                yield return CardTestInput.Settle(0.2f);
            }

            yield return UseLanguage("en");
            yield return OpenFull(Lamp);
            var listen = Nav.Model.Sections.Single(s => s.Key == BuiltInFamilies.Guides);
            Assert.AreEqual(1, listen.Count, "precondition: Listen holds one block");
            yield return OpenFullViewOf(listen.Targets[0]);
            Assert.AreEqual(1, Takeover.PageCount, "one page");
            Assert.AreEqual(DisplayStyle.None, Takeover.ChipStrip.resolvedStyle.display, "no chips for a family of one");
            Assert.IsTrue(Takeover.SectionShown, "its breadcrumb still names Listen");
            yield return SwipeThePage(+1, atHeight: 0.9f);
            Assert.IsTrue(Takeover.IsOpen, "a swipe there turns nothing and closes nothing");
            Assert.AreEqual(listen.Targets[0], Sheet.FullView.BlockKey);
            yield return CardGalleryChecks.Render("FullView_Lamp_listen_en_390");
        }
    }
}

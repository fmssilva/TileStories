using System;
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
    // Phase B of step 15b (_3.2 section 6.1, the bento LAYOUT) on the REAL LivingRoomScene with real touches, a real Escape and the card's
    // REAL owners (the preview stage loading the real glb; the video owner over a stand-in picture device):
    //   - The Lamp in bento at 390 px, EN and PT: a tile per section where its blocks were, drawn where BentoLayoutRule says for the measured
    //     room, filling the Full card (About leads; Listen keeps its minimum width beside Media; Community its minimum height), every word whole
    //     at 4.5:1, every tile a whole touch target; at Half every row keeps its minimum and the tiles scroll
    //   - a REAL tap on a tile opens the right section in the full view (its blocks, in order, under their headings), one nav_jump named
    //     bento; a real Escape closes it to the BENTO -- the card where it was, not lowered -- and the next one lowers the card
    //   - media released when the full view closes, asked of the OWNERS: the model's full-view slot goes back, a video played there stops,
    //     the card's media holds what the overview held
    //   - no second sheet: a real tap on the Drawer's line closes the drawer and opens the section alone; a real tap on an Open Block button
    //     inside the full view REPLACES the page (one takeover), never stacks one
    //   - Show After Reading holds in bento: every section read in the full view and the overview read to its end reveal the waiting question
    //     (the Play tile counts it); one section left unread does not
    //   - no Top Bar over the tiles (step 16: the tiles are the map); a short card (Lamp - Military) scrolls
    public class PoiCardBentoLayoutSceneTests : SearchSceneFixture
    {
        private const string Lamp = "lamp";
        private const string LampMilitary = "lamp_military";
        private const string WaitingQuestion = "block_44";

        private PoiCardSheetView Sheet => Card.Sheet;
        private BlockStackView Stack => Sheet.Stack;
        private NavController Nav => Sheet.Nav;
        private ScrollView Scroll => Stack.Scroll;
        private CardBentoView Bento => Sheet.Bento;
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
        public IEnumerator UseStandInDevicesAnInMemoryCardStateAndRecordTheEvents()
        {
            _videoOut = new ManualVideoOutput();
            _video = new CardVideoService(_videoOut, () => Card.Media);
            Card.UseVideoService(_video);
            // - a fresh reading memory every test: the waiting question starts hidden
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

        // The wall pushed through the live seam with the bento layout (and `edit`), in `language`
        private IEnumerator UseBento(Action<CardNavigationSettings> edit = null, string language = "en")
        {
            var copy = ConfigCopy();
            _shippedLanguages ??= new List<string>(ConfigCopy().card_settings.languages);
            copy.card_settings.languages = language == "en" ? new List<string>(_shippedLanguages) : new List<string> { language, "en" };
            copy.card_settings.navigation.layout = CardOptions.LayoutBento;
            edit?.Invoke(copy.card_settings.navigation);
            Session.ApplyCardSettings(copy);
            yield return CardTestInput.Settle(0.3f);
        }

        private IEnumerator Open(string poi, SheetStopRule.Stop stop)
        {
            if (SelectionEventBus.CurrentPoiId != poi)
            {
                SelectionEventBus.Clear();
                yield return CardTestInput.Settle();
                SelectionEventBus.Select(poi);
                yield return CardTestInput.Settle();
            }
            Scroll.scrollOffset = Vector2.zero;
            Sheet.SetStop(stop);
            yield return CardTestInput.Settle();
            Assert.AreEqual(stop, Sheet.Stop, "precondition: the card rests at " + stop);
            float rest = Sheet.Stops.HeightOf(Sheet.Stop);
            for (int frame = 0; frame < 600 && Mathf.Abs(Sheet.Root.layout.height - rest) > OnePixel; frame++) yield return null;
            Assert.AreEqual(rest, Sheet.Root.layout.height, OnePixel, "precondition: the sheet rests at its " + Sheet.Stop + " height");
            yield return CardTestInput.Settle(0.2f);
        }

        // Scroll `scroll` until `target` is inside its visible part AND still for three frames (a picture loading moves it)
        private static IEnumerator InViewAndStill(ScrollView scroll, VisualElement target)
        {
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
                "precondition: " + target.name + " is inside the visible part: " + target.worldBound + " in " + scroll.contentViewport.worldBound);
        }

        // A real tap by a touch pointer on the tile of `section`, the tile first in view and still
        private IEnumerator TouchTapTile(int section)
        {
            var tile = Bento.Tiles[section];
            yield return InViewAndStill(Scroll, tile.Button);
            yield return CardTestInput.TouchDrag(Sheet.Layer.panel, tile.Button.worldBound.center, Vector2.zero, frames: 1);
            yield return CardTestInput.Settle(0.3f);
        }

        // A real press and release of Escape on an Input System keyboard (where Android's Back arrives), under BackgroundSafeInput
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

        private static IEnumerator WaitFor(Func<bool> condition, string what, int frames = 900)
        {
            for (int i = 0; i < frames && !condition(); i++) yield return null;
            Assert.IsTrue(condition(), "timed out waiting for " + what);
        }

        private int SectionIndex(string family) => Nav.Model.Sections.ToList().FindIndex(s => s.Key == family);

        // The overview scrolled so the tiles' top meets the viewport's top (the hero picture of The Lamp's header above them scrolled off)
        private IEnumerator TilesAtTheTop()
        {
            for (int frame = 0; frame < 120; frame++)
            {
                float offset = NavScrollRule.TargetOffset(Bento.Host.layout.y, Scroll.contentContainer.layout.height, Scroll.contentViewport.layout.height);
                Scroll.scrollOffset = new Vector2(0f, offset);
                yield return null;
                if (Mathf.Abs(Bento.Host.worldBound.yMin - Scroll.contentViewport.worldBound.yMin) <= OnePixel) break;
            }
            yield return CardTestInput.Settle(0.3f);
            Assert.AreEqual(Scroll.contentViewport.worldBound.yMin, Bento.Host.worldBound.yMin, OnePixel, "precondition: the tiles' top at the viewport's top");
        }

        // ---------------- the tests ----------------

        [UnityTest]
        public IEnumerator TheLamp_IsATilePerSection_FillingTheFullCardWhereTheRuleSays_WordsWholeInEnglishAndPortuguese_AndAtHalfTheTilesScroll()
        {
            foreach (string language in new[] { "en", "pt" })
            {
                yield return UseBento(language: language);
                SelectionEventBus.Clear();
                yield return CardTestInput.Settle();
                yield return Open(Lamp, SheetStopRule.Stop.Full);
                var model = Nav.Model;
                Assert.AreEqual(CardOptions.LayoutBento, Sheet.Layout.Effective, language);
                Assert.AreEqual(8, Bento.Tiles.Count, language + ": a tile per section of The Lamp");
                foreach (string target in model.Targets) Assert.IsNull(Stack.ViewOfBlock(target), target + ": no view on the overview");
                yield return TilesAtTheTop();

                var placed = Bento.Placed;
                var host = Bento.Host.worldBound;
                float viewport = Scroll.contentViewport.layout.height;
                Assert.IsTrue(placed.Fills, language + ": eight sections fill a Full card");
                Assert.AreEqual(viewport, host.height, OnePixel, language + ": the tiles are exactly the visible part (" + viewport + " px)");
                Assert.AreEqual(0f, placed.RoomLeft, OnePixel, language + ": a long card's tiles use all of it (no room left under them)");
                int about = SectionIndex(BuiltInFamilies.About);
                Assert.AreEqual(about, BentoLayoutRule.Lead(model.Sections.Select(s => s.Count).ToArray()), "About leads (18 blocks)");
                Assert.AreEqual(host.width, Bento.Tiles[about].Button.worldBound.width, OnePixel, "the lead spans the card");
                float touch = CardTestInput.TokenPx("--ts-touch-target");
                float minHeight = CardTestInput.TokenPx("--ts-bento-tile-min-height");
                foreach (var tile in Bento.Tiles)
                {
                    var drawn = tile.Button.worldBound;
                    var rule = placed.Tiles[tile.Index];
                    Assert.AreEqual(rule.X, drawn.xMin - host.xMin, OnePixel, tile.Words.text + ": where the rule says");
                    Assert.AreEqual(rule.Y, drawn.yMin - host.yMin, OnePixel, tile.Words.text);
                    Assert.AreEqual(rule.Width, drawn.width, OnePixel, tile.Words.text);
                    Assert.AreEqual(rule.Height, drawn.height, OnePixel, tile.Words.text);
                    Assert.GreaterOrEqual(Mathf.Min(drawn.width, drawn.height), touch - OnePixel, tile.Words.text + ": a whole touch target");
                    Assert.GreaterOrEqual(drawn.height, minHeight - OnePixel, tile.Words.text + ": at least the tile minimum");
                    var words = tile.Words.MeasureTextSize(tile.Words.text, 0, VisualElement.MeasureMode.Undefined, 0, VisualElement.MeasureMode.Undefined);
                    Assert.LessOrEqual(words.x, tile.Words.layout.width + 0.5f, "'" + tile.Words.text + "': the whole word, never cut");
                    Assert.LessOrEqual(tile.Words.worldBound.xMax, drawn.xMax + OnePixel, "'" + tile.Words.text + "': inside its tile");
                    foreach (var label in new[] { tile.Words, tile.Count })
                    {
                        float contrast = CardTestInput.Contrast(label.resolvedStyle.color, CardTestInput.EffectiveBackground(label));
                        Assert.GreaterOrEqual(contrast, UIAccessibility.MinRatioNormalText, "'" + label.text + "' reads at 4.5:1 (" + contrast.ToString("F2") + ")");
                    }
                    Assert.AreEqual(model.Sections[tile.Index].Count.ToString(), tile.Count.text, tile.Words.text + ": its count");
                    // - the names of its blocks, in order, as many as the tile holds whole -- above the count, inside the tile
                    var shown = tile.ShownLines.ToList();
                    var targets = model.Sections[tile.Index].Targets;
                    for (int k = 0; k < shown.Count; k++)
                    {
                        Assert.AreEqual(Nav.RowOf(targets[k]).Words, shown[k].text, tile.Words.text + ": name " + k + " is its block's words");
                        Assert.LessOrEqual(shown[k].worldBound.yMax, tile.Count.worldBound.yMin + OnePixel, "'" + shown[k].text + "': above the count");
                        Assert.LessOrEqual(shown[k].worldBound.xMax, drawn.xMax + OnePixel, "'" + shown[k].text + "': inside the tile");
                    }
                    Assert.LessOrEqual(tile.Count.worldBound.yMax, drawn.yMax + OnePixel, tile.Words.text + ": the count whole inside the tile");
                    TestContext.WriteLine(language + " " + tile.Words.text + " (" + model.Sections[tile.Index].Count + "): " + drawn.width.ToString("F1") + " x "
                        + drawn.height.ToString("F1") + " row " + rule.Row);
                }
                // - the target wins (the brief's question): Listen (1 block) beside Media (14) keeps its words' width, Community's row its height
                int listen = SectionIndex(BuiltInFamilies.Guides);
                int community = SectionIndex(BuiltInFamilies.Community);
                Assert.Greater(Bento.Tiles[listen].Button.worldBound.width, host.width * 1f / 15f + OnePixel, "Listen is wider than its 1-of-15 share: the minimum won");
                // - its height is the rule's share of the room, never under the minimum (how much above it depends on Full's height: 844 since S4a)
                float communityHeight = Bento.Tiles[community].Button.worldBound.height;
                Assert.GreaterOrEqual(communityHeight, minHeight - OnePixel, "Community's row keeps at least the minimum height");
                Assert.AreEqual(placed.Tiles.First(t => t.Section == community).Height, communityHeight, OnePixel, "...and the height the rule gives its row");
                Assert.IsEmpty(Bento.Tiles[community].ShownLines.ToList(), "a tile at its minimum shows no names: its title and count only");
                var leadNames = Bento.Tiles[about].ShownLines.ToList();
                Assert.GreaterOrEqual(leadNames.Count, 3, "the lead shows several of its blocks' names (" + leadNames.Count + ")");
                Assert.Less(leadNames.Count, model.Sections[about].Count, "but not all 18: the rest stay hidden in place");
                int play = SectionIndex(BuiltInFamilies.Play);
                Assert.Less(Bento.Tiles[play].Button.worldBound.width, 140f, "precondition: Play's tile is narrow beside Media");
                Assert.IsEmpty(Bento.Tiles[play].ShownLines.ToList(), "a tile too narrow to read names shows none (no names cut to a few letters)");
                TestContext.WriteLine(language + ": viewport " + viewport.ToString("F1") + ", rows " + placed.Rows.Count + ", at minimum " + placed.RowsAtMinimum);
                yield return CardGalleryChecks.Render("Nav_Lamp_bento_" + language + "_390");
            }

            // - Half: the visible part is too low for the rows' minimums -- every row keeps it and the tiles scroll
            yield return Open(Lamp, SheetStopRule.Stop.Half);
            yield return CardTestInput.Settle(0.3f);
            Assert.IsFalse(Bento.Placed.Fills, "at Half the tiles do not fit (" + Scroll.contentViewport.layout.height + " px)");
            Assert.AreEqual(Bento.Placed.Rows.Count, Bento.Placed.RowsAtMinimum, "every row at its minimum");
            Assert.Greater(Bento.Host.layout.height, Scroll.contentViewport.layout.height, "the tiles are taller than the room: the card scrolls");
        }

        [UnityTest]
        public IEnumerator ARealTapOnATile_OpensThatSectionInTheFullView_AndARealEscapeClosesItToTheBento_NotTheCardBelowIt()
        {
            yield return UseBento();
            yield return Open(Lamp, SheetStopRule.Stop.Full);
            int stories = SectionIndex(BuiltInFamilies.Stories);
            var section = Nav.Model.Sections[stories];
            float offset = Scroll.scrollOffset.y;
            _events.Raised.Clear();
            yield return TouchTapTile(stories);
            offset = Scroll.scrollOffset.y;

            Assert.IsTrue(Sheet.FullView.IsOpen, "a real tap on the Stories tile opened the full view");
            Assert.AreEqual(section.Key, Sheet.FullView.SectionKey, "the right section");
            CollectionAssert.AreEqual(section.Targets, Sheet.FullView.SectionBlocks.ToList(), "its blocks, in order");
            foreach (string target in section.Targets)
            {
                var slot = Sheet.FullView.SectionSlotOf(target);
                Assert.IsNotNull(Sheet.FullView.SectionViewOf(target), target + ": its view on the page");
                Assert.AreEqual(Stack.HeadingOfBlock(target), slot.Q<Label>(className: "card-block-heading").text, target + ": under the card's own heading");
            }
            Assert.AreEqual(1, NavJumps.Count, "one nav_jump");
            Assert.AreEqual(CardBentoView.ElementKey, NavJumps[0].Variant);
            Assert.AreEqual(section.Key, NavJumps[0].Value);
            Assert.AreEqual(stories, Nav.Current, "the spy names the open section");
            StringAssert.EndsWith(" > " + Bento.Tiles[stories].Words.text, Sheet.Takeover.CrumbText, "the breadcrumb ends with the tile's words");
            // - ONE sheet over the card: what a finger meets at the screen's middle is the full view
            var middle = Sheet.Layer.worldBound.center;
            var picked = Sheet.Layer.panel.Pick(middle);
            Assert.IsNotNull(picked);
            Assert.IsTrue(Sheet.Takeover.Root.Contains(picked), "the full view is on top (picked " + picked.name + ")");
            Assert.IsFalse(Sheet.Overlay.IsOpen, "nothing else is open");
            yield return CardGalleryChecks.Render("Nav_Lamp_bento_open_en_390");

            yield return PressEscape();
            Assert.IsFalse(Sheet.FullView.IsOpen, "a real Escape closed the full view");
            Assert.AreEqual(SheetStopRule.Stop.Full, Sheet.Stop, "to the BENTO: the card is still at Full");
            Assert.AreEqual(offset, Scroll.scrollOffset.y, OnePixel, "the tiles where they were");
            Assert.IsTrue(Scroll.contentViewport.worldBound.Contains(Bento.Tiles[stories].Button.worldBound.center), "the tile tapped is on screen again");
            Assert.AreEqual(-1, Nav.Current, "the overview: no section");
            yield return PressEscape();
            Assert.AreEqual(SheetStopRule.Stop.Half, Sheet.Stop, "the next Escape lowers the card");
        }

        // The brief's acceptance on the OWNERS: Media opened from its tile binds its blocks in the full view -- the model takes its full-view
        // slot (the card's own is set aside, so it holds none), a real tap plays the video -- and closing the full view gives everything back
        [UnityTest]
        public IEnumerator ClosingTheFullView_ReleasesItsMedia_TheModelsSlotGoesBack_AndAVideoPlayingThereStops()
        {
            yield return UseBento();
            yield return Open(Lamp, SheetStopRule.Stop.Full);
            CollectionAssert.DoesNotContain(Card.PreviewService.HeldKeys, "block_59", "precondition: on the overview the model holds no slot");
            var heldBefore = Card.PreviewService.HeldKeys.ToList();
            int files = Card.Media.LoadedCount;
            yield return TouchTapTile(SectionIndex(BuiltInFamilies.Media));
            Assert.IsTrue(Sheet.FullView.IsOpen);
            Assert.AreEqual(BuiltInFamilies.Media, Sheet.FullView.SectionKey);
            yield return WaitFor(() => Card.PreviewService.HeldKeys.Contains("block_59::takeover"), "the model's full-view slot");
            Assert.Greater(Sheet.Takeover.HeldMedia, 0, "the page loads its files through the takeover's scope");

            var video = Sheet.FullView.SectionViewOf("block_56") as VideoBlockView;
            Assert.IsNotNull(video, "precondition: the inline video is on the page");
            yield return InViewAndStill(Sheet.FullView.Scroll, video.Panel.PlayOverlay);
            yield return CardTestInput.Tap(video.Panel.PlayOverlay.panel, video.Panel.PlayOverlay.worldBound.center);
            yield return null;
            Assert.AreEqual(CardVideoState.Playing, _video.State, "a real tap played the video in the full view");
            // - the track is read before the page leaves: an unbound view forgets it (IsCurrent(null) would be false whatever the owner did)
            var track = video.Track;
            Assert.IsTrue(_video.IsCurrent(track), "precondition: the owner plays this block's video");
            Assert.GreaterOrEqual(Card.Media.RefCount(track.ClipPath), 2, "precondition: the clip is held by the owner AND the page's scope");

            yield return PressEscape();
            Assert.IsFalse(Sheet.FullView.IsOpen);
            Assert.AreEqual(CardVideoState.Paused, _video.State, "the video stopped playing as its page left (ICollapsibleBlockView)");
            CollectionAssert.AreEquivalent(heldBefore, Card.PreviewService.HeldKeys, "the model's full-view slot went back");
            Assert.AreEqual(0, Sheet.Takeover.HeldMedia, "the page holds nothing");
            // - what is left is the ONE video owner's paused clip -- the rule of step 12 for every view that leaves the screen (a row closed, a
            //   section put away, a page turned): kept until another video plays or the card closes. Every file of the page went back
            Assert.IsTrue(_video.IsCurrent(track), "the owner keeps the paused video (step 12's rule)");
            Assert.AreEqual(files + 1, Card.Media.LoadedCount, "the overview's files plus the owner's one paused clip, nothing else");
            Assert.AreEqual(1, Card.Media.RefCount(track.ClipPath), "the page's references went back with its scope: only the owner's is left");
            _video.CardClosed();
            Assert.AreEqual(files, Card.Media.LoadedCount, "the card closing lets that clip go too");
        }

        // No sheet on a sheet (NN/g bottom sheets, Laubheimer 2023), through the overlay owner of step 8: a real tap on the Drawer's line
        // closes the drawer as the section opens; a real tap on an Open Block button inside the full view replaces its page
        [UnityTest]
        public IEnumerator NoSecondSheet_ADrawerPickClosesTheDrawer_AndAnOpenBlockInsideTheFullViewReplacesThePage()
        {
            yield return UseBento(nav => nav.menu = BuiltInNavElements.DrawerTree);
            yield return Open(Lamp, SheetStopRule.Stop.Full);
            yield return CardTestInput.Tap(Sheet.MenuButton.panel, Sheet.MenuButton.worldBound.center);
            yield return CardTestInput.Settle(0.3f);
            Assert.IsTrue(Sheet.Overlay.IsOpen, "precondition: a real tap opened the drawer");
            var drawer = Sheet.Slots.MenuView as DrawerTreeNavView;
            Assert.IsNotNull(drawer);
            int about = SectionIndex(BuiltInFamilies.About);
            var line = drawer.SectionRows.Single(r => r.Index == about).Button;
            yield return CardTestInput.Tap(line.panel, line.worldBound.center);
            yield return CardTestInput.Settle(0.3f);
            Assert.IsFalse(Sheet.Overlay.IsOpen, "the drawer closed");
            Assert.IsTrue(Sheet.FullView.IsOpen, "the section opened");
            Assert.AreEqual(BuiltInFamilies.About, Sheet.FullView.SectionKey);

            // - block_18's Open Block buttons: one to the Listen player (another section), one to the sources (no section)
            var actions = Sheet.FullView.SectionViewOf("block_18") as ActionsBlockView;
            Assert.IsNotNull(actions, "precondition: The Lamp's actions block with its Open Block buttons is in About");
            var toListen = actions.Actions.Single(a => a.Target == "block_55").Button;
            yield return InViewAndStill(Sheet.FullView.Scroll, toListen);
            _events.Raised.Clear();
            yield return CardTestInput.Tap(toListen.panel, toListen.worldBound.center);
            yield return CardTestInput.Settle(0.3f);
            Assert.IsTrue(Sheet.Takeover.IsOpen && Sheet.FullView.IsOpen, "still ONE full view");
            Assert.AreEqual(BuiltInFamilies.Guides, Sheet.FullView.SectionKey, "the page now holds Listen: replaced, not stacked");
            Assert.IsNull(Sheet.FullView.SectionViewOf("block_18"), "About's blocks left the page");
            Assert.AreEqual(1, Sheet.Layer.Query(className: "card-takeover").ToList().Count(e => e.resolvedStyle.display == DisplayStyle.Flex),
                "one takeover layer shown");
            Assert.AreEqual(1, NavJumps.Count, "one nav_jump");
            Assert.AreEqual("block_55", NavJumps[0].BlockKey);

            yield return PressEscape();
            Assert.IsFalse(Sheet.Takeover.IsOpen, "ONE Escape: back on the tiles");
            Assert.AreEqual(SheetStopRule.Stop.Full, Sheet.Stop);
        }

        // Show After Reading in every layout (_3.2 step 14's one rule): in bento a section is read in its full view; the waiting question
        // shows once EVERY section was read to its end there and the overview to its end -- one section left unread keeps it hidden
        [UnityTest]
        public IEnumerator EverySectionReadInTheFullView_RevealsTheWaitingQuestion_AndOneLeftUnreadDoesNot()
        {
            yield return UseBento();
            yield return Open(Lamp, SheetStopRule.Stop.Full);
            int play = SectionIndex(BuiltInFamilies.Play);
            int playCount = Nav.Model.Sections[play].Count;
            Assert.IsFalse(Stack.IsRevealed(WaitingQuestion), "precondition: the question waits");
            CollectionAssert.DoesNotContain(Nav.Model.Targets, WaitingQuestion);

            IEnumerator ReadSection(int index)
            {
                Assert.IsTrue(Nav.JumpTo(index, CardBentoView.ElementKey));
                yield return CardTestInput.Settle(0.3f);
                var page = Sheet.FullView.Scroll;
                // - read down to the end in steps of a quarter viewport: under the third the spy line sits at, so no block's top can be stepped
                //   over without crossing the line on screen (a finger scrolls continuously; a half-viewport step skipped a short postcard)
                for (int step = 0; step < 200; step++)
                {
                    float range = NavScrollRule.Range(page.contentContainer.layout.height, page.contentViewport.layout.height);
                    if (page.scrollOffset.y >= range - OnePixel) break;
                    page.scrollOffset = new Vector2(0f, Mathf.Min(range, page.scrollOffset.y + page.contentViewport.layout.height * 0.25f));
                    yield return null;
                    yield return null;
                }
                yield return CardTestInput.Settle(0.2f);
                foreach (string target in Nav.Model.Sections[index].Targets)
                {
                    var slot = Sheet.FullView.SectionSlotOf(target);
                    Assert.IsTrue(Nav.HasViewed(target), target + " read on the page -- slot " + (slot == null ? "none" : slot.layout.ToString()) + ", offset " + page.scrollOffset.y +
                        ", content " + page.contentContainer.layout.height + ", viewport " + page.contentViewport.layout.height + ", section " + Sheet.FullView.SectionKey +
                        ", highValue " + page.verticalScroller.highValue + ", range " +
                        NavScrollRule.Range(page.contentContainer.layout.height, page.contentViewport.layout.height) + ", scroll " + page.worldBound);
                }
                yield return PressEscape();
            }

            // - every section but the last
            for (int i = 0; i < Nav.Model.Sections.Count - 1; i++) yield return ReadSection(i);
            Scroll.scrollOffset = new Vector2(0f, NavScrollRule.Range(Scroll.contentContainer.layout.height, Scroll.contentViewport.layout.height));
            yield return CardTestInput.Settle(0.3f);
            Assert.IsFalse(Stack.IsRevealed(WaitingQuestion), "one section never opened: the card is not read");

            yield return ReadSection(Nav.Model.Sections.Count - 1);
            Scroll.scrollOffset = new Vector2(0f, NavScrollRule.Range(Scroll.contentContainer.layout.height, Scroll.contentViewport.layout.height));
            yield return CardTestInput.Settle(0.3f);
            Assert.IsTrue(Stack.IsRevealed(WaitingQuestion), "every section read and the overview at its end: the question shows");
            Assert.AreEqual(playCount + 1, Nav.Model.Sections[SectionIndex(BuiltInFamilies.Play)].Count, "the model counts it");
            Assert.AreEqual((playCount + 1).ToString(), Bento.Tiles[SectionIndex(BuiltInFamilies.Play)].Count.text, "and the Play tile says so");
        }

        [UnityTest]
        public IEnumerator UnderBento_NoTopBarIsDrawnOverTheTiles_TheyStartUnderTheTitle_AndAShortCardScrolls()
        {
            // - step 16 (the 15b review): the tiles ARE the map -- the wall's Text Tabs are not drawn over them (an inert duplicate); the value
            //   stays for another layout, and the first tile sits right under the header with no strip between
            yield return UseBento(nav => nav.top = BuiltInNavElements.TextTabs);
            yield return Open(Lamp, SheetStopRule.Stop.Full);
            Assert.IsTrue(Bento.IsOn, "precondition: the bento is drawn");
            Assert.AreEqual(BuiltInNavElements.TextTabs, Session.CardSettings.navigation.top, "precondition: the wall still says Text Tabs");
            Assert.IsNull(Sheet.Slots.TopKey, "no element in the top slot");
            Assert.AreEqual(DisplayStyle.None, Sheet.Slots.Top.resolvedStyle.display, "nothing drawn there");
            Assert.LessOrEqual(Scroll.worldBound.yMin, Stack.HeaderSlot.worldBound.yMax + OnePixel, "the stack (and its tiles) starts right under the title");
            var first = Bento.Tiles[0].Button;
            // - _3.3 S4a: the hero is Full's whole view and the tiles come after it -- tap the tile where a finger reaches it
            yield return CardTestInput.TapInReach(Sheet, first);
            yield return CardTestInput.Settle(0.3f);
            Assert.AreEqual(Nav.Model.Sections[0].Key, Sheet.FullView.SectionKey, "a real tap on a tile still opens its section");
            yield return PressEscape();

            yield return Open(LampMilitary, SheetStopRule.Stop.Full);
            Assert.AreEqual(CardOptions.LayoutScroll, Sheet.Layout.Effective, "Lamp - Military: nothing to switch between, it scrolls");
            Assert.IsFalse(Bento.IsOn);
            Assert.IsNull(Bento.Host.parent, "no tiles: the host is not in the scroll");
            foreach (string target in Nav.Model.Targets) Assert.IsNotNull(Stack.ViewOfBlock(target), target + " drawn whole");
        }
    }
}

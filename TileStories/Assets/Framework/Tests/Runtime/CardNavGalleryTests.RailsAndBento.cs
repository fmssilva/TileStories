using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // Phase A of the two in-flow nav blocks of step 15a (_3.2 section 5.6): nav_rails and nav_bento over the five fixture cases at 390 px,
    // read from CardNavGalleryDefinitions like every other entry. The rails come in through Auto Nav Block, the bento is AUTHORED into the
    // card (its tiles are content: the builder never inserts it) -- both right after the header.
    public partial class CardNavGalleryTests
    {
        // The Swipes To The End a rail has with no field stored (BuiltInBlocks.NavRails' NumberDefault)
        private const int DefaultSwipes = 4;

        private static string[] RailsEntryNames => CardNavGalleryDefinitions.All
            .Where(e => e.IsBlock && e.ElementKey == BuiltInBlocks.NavRailsKind).Select(e => e.Name).ToArray();

        private static string[] BentoEntryNames => CardNavGalleryDefinitions.All
            .Where(e => e.IsBlock && e.ElementKey == BuiltInBlocks.NavBentoKind).Select(e => e.Name).ToArray();

        private static string WordsOf(CardStrings strings, string key, string fallback)
        {
            string words = key != null ? strings.Get(key) : "";
            return string.IsNullOrEmpty(words) ? fallback : words;
        }

        // A real tap (panel events) at the centre of `target`, after asserting that is what the finger would land on, not something over it
        private static IEnumerator RealTap(PoiCardSheetView sheet, VisualElement target)
        {
            var at = target.worldBound.center;
            var picked = sheet.Root.panel.Pick(at);
            Assert.IsTrue(picked != null && (picked == target || target.Contains(picked)),
                "precondition: the tap lands on " + target.name + ", but the panel picks " + (picked != null ? picked.name + " (" + picked.GetType().Name + ")" : "nothing") + " at " + at);
            yield return CardTestInput.Tap(sheet.Root.panel, at);
        }

        // How far the card's stack can scroll (a short fixture card barely does: a landing there is clamped, not measured)
        private static float ScrollRange(PoiCardSheetView sheet) =>
            NavScrollRule.Range(sheet.Stack.Scroll.contentContainer.layout.height, sheet.Stack.Scroll.contentViewport.layout.height);

        // Every word of `root` a visitor sees: whole (a wrapping label fits its longest word, any other its whole text) and at 4.5:1
        private static void AssertWordsWholeAndReadable(VisualElement root, VisualElement layer, string what)
        {
            foreach (var label in root.Query<Label>().ToList().Where(l => l.text.Length > 0 && CardTestInput.IsShown(l, layer)))
            {
                bool wraps = label.resolvedStyle.whiteSpace == WhiteSpace.Normal;
                string widest = wraps ? label.text.Split(' ').OrderByDescending(w => label.MeasureTextSize(w, 0, VisualElement.MeasureMode.Undefined, 0,
                    VisualElement.MeasureMode.Undefined).x).First() : label.text;
                var size = label.MeasureTextSize(widest, 0, VisualElement.MeasureMode.Undefined, 0, VisualElement.MeasureMode.Undefined);
                Assert.GreaterOrEqual(label.layout.width + 0.5f, size.x, what + " '" + widest + "': the whole word fits, never cut");
                float contrast = CardTestInput.Contrast(label.resolvedStyle.color, CardTestInput.EffectiveBackground(label));
                Assert.GreaterOrEqual(contrast, UIAccessibility.MinRatioNormalText, what + " '" + label.text + "' reads at 4.5:1 (" + contrast.ToString("F2") + ")");
            }
        }

        // The rails block over every case: Auto Nav Block puts it right after the header; one rail per SECTION (it lists blocks, so a one-family
        // card keeps its rail: rule 2.5.7 does not apply) under its words and count; the cards the rail draws are exactly NavRailRule's plan
        // for the strip as laid out; no rail takes more than the swipes allowed to reach its last cell; every card a full touch target with
        // whole words at 4.5:1 inside the card; with Peek Next Card the first cell past the whole ones shows exactly --ts-track-peek of itself
        // at the strip's right edge (the cut card IS the cue), and a rail that fits shows no slice. Rendered Nav_nav_rails_<case>.png
        [UnityTest]
        public IEnumerator EveryRailsEntry_IsRightAfterTheHeader_ARailPerSection_WithinTheSwipes_TheNextCardCutAsTheCue_WholeAndTappable_AndRenders(
            [ValueSource(nameof(RailsEntryNames))] string name)
        {
            int index = CardNavGalleryDefinitions.All.ToList().FindIndex(e => e.Name == name);
            var entry = CardNavGalleryDefinitions.All[index];
            // - the recorder first: the card's context takes the harness's events when the card is SHOWN
            var events = new RecordingCardEvents();
            _harness.Gallery.Events = events;
            _harness.Show(index);
            yield return CardTestInput.Settle();
            var sheet = _harness.Gallery.Sheet;
            var model = sheet.Nav.Model;
            string autoKey = BlockStackBuilder.AutoNavBlockKey(entry.ElementKey);
            Assert.IsNull(sheet.Slots.TopView, "no element in any slot: the block judged alone");
            CollectionAssert.DoesNotContain(model.Targets, autoKey, "the rails are no target of the card's own navigation");
            var slot = sheet.Stack.SlotOfBlock(autoKey);
            Assert.IsNotNull(slot, "Auto Nav Block put the rails in the stack");
            Assert.AreEqual(0, sheet.Stack.Scroll.contentContainer.IndexOf(slot), "the first block of the scroll: right after the header");
            var view = sheet.Stack.BoundViews.OfType<NavRailsBlockView>().Single();
            Assert.AreEqual(model.Sections.Count, view.Rails.Count, "a rail per section (it lists blocks: a one-family card keeps its rail)");

            var layer = sheet.Layer;
            float onePixel = CardGalleryChecks.OnePixel(view.Root);
            float touch = CardTestInput.TokenPx("--ts-touch-target");
            float narrowest = CardTestInput.TokenPx("--ts-rail-card");
            var card = sheet.Root.worldBound;
            Assert.GreaterOrEqual(view.Root.worldBound.xMin, card.xMin - onePixel, "the block starts inside the card");
            Assert.LessOrEqual(view.Root.worldBound.xMax, card.xMax + onePixel, "and ends inside it");
            var strings = Strings(_harness, entry);
            var jumps = new List<string>();

            for (int i = 0; i < model.Sections.Count; i++)
            {
                var section = model.Sections[i];
                var rail = view.Rails[i];
                string what = section.Key + " (" + section.Targets.Count + " blocks)";
                Assert.AreEqual(WordsOf(strings, section.LabelKey, section.Key), rail.TitleWords.text, what + ": the section's words in " + entry.Case.Language);
                Assert.AreEqual(section.Count.ToString(), rail.TitleCount.text, what + ": its block count");
                float viewport = rail.Strip.VisibleWidth;
                float peek = CardTrackFit.PeekOf(rail.Strip.Viewport, true);
                Assert.Greater(peek, 0f, what + ": precondition: Peek Next Card is on and its token resolves");
                var cards = rail.ShownCards.ToList();
                Assert.GreaterOrEqual(cards.Count, 1, what + ": a rail with blocks draws a card");

                // - the cards are the plan's, from the numbers the strip really has
                float gap = cards[0].Button.resolvedStyle.marginRight;
                float width = cards[0].Button.layout.width;
                Assert.AreEqual(CardTrackRule.CellWidth(viewport, narrowest, gap, peek, section.Targets.Count), width, onePixel,
                    what + ": the cards are CardTrackRule's width for this strip");
                var plan = NavRailRule.Of(section.Targets.Count, width + gap, viewport, DefaultSwipes);
                Assert.AreEqual(plan.Cards, cards.Count, what + ": the block cards NavRailRule keeps");
                Assert.AreEqual(plan.HasAll, rail.AllCard.Button.resolvedStyle.display == DisplayStyle.Flex, what + ": the All card exactly when blocks are left out");
                int swipes = NavRailRule.SwipesToEnd(plan.Cells, width + gap, viewport);
                Assert.LessOrEqual(swipes, DefaultSwipes, what + ": " + plan.Cells + " cells reach the end in " + swipes + " swipes");
                for (int c = 0; c < cards.Count; c++)
                    Assert.AreEqual(section.Targets[c], cards[c].BlockKey, what + ": card " + c + " leads to the section's block " + c);

                // - the row is as long as its cells: nothing is cut at the END of the swipe
                var cells = cards.Select(c => c.Button).ToList();
                if (plan.HasAll) cells.Add(rail.AllCard.Button);
                Assert.AreEqual(plan.Cells * (width + gap), rail.Strip.Row.layout.width, onePixel * 2f, what + ": the row is its cells' width: the swipe ends on a whole card");

                // - the cue: with more cells than fit whole, the next one shows --ts-track-peek of itself at the strip's edge; else none
                int whole = CardTrackRule.WholeCells(viewport - peek, narrowest, gap);
                float edge = rail.Strip.Viewport.worldBound.xMax;
                if (plan.Cells > whole)
                {
                    var cut = cells[whole].worldBound;
                    Assert.AreEqual(peek, edge - cut.xMin, onePixel * 2f, what + ": the next card shows exactly the peek token of itself (" + (edge - cut.xMin) + " of " + peek + ")");
                    Assert.Greater(cut.xMax, edge + onePixel, what + ": and is cut by the strip's edge, not whole");
                }
                else
                    Assert.LessOrEqual(cells[cells.Count - 1].worldBound.xMax, edge + onePixel, what + ": a rail that fits shows no slice");

                // - every cell a touch target, its words whole at 4.5:1, its family's icon
                foreach (var button in cells)
                {
                    Assert.GreaterOrEqual(button.layout.height, touch - onePixel, button.name + ": tall enough to tap");
                    Assert.GreaterOrEqual(button.layout.width, touch - onePixel, button.name + ": wide enough to tap");
                    AssertWordsWholeAndReadable(button, layer, button.name);
                }
                foreach (var c in cards)
                    Assert.AreEqual(section.IconKey, CardIcons.KeyOf(c.Icon), what + ": every card wears its family's icon");
                if (plan.HasAll)
                    Assert.AreEqual(string.Format(WordsOf(strings, CardStrings.Keys.NavRailsAll, "{0}"), section.Targets.Count), rail.AllCard.Words.text,
                        what + ": the All card says how many blocks the section holds");
            }
            yield return CardGalleryChecks.Render(name);

            // - a real tap on the first card of the first rail lands its block, one nav_jump named nav_rails; a tap on the All card of a rail that has
            //   one lands the section's first block
            var first = view.Rails[0];
            sheet.Stack.Scroll.scrollOffset = Vector2.zero;
            yield return CardTestInput.Settle();
            yield return RealTap(sheet, first.ShownCards.First().Button);
            yield return Landed(sheet.Nav);
            if (ScrollRange(sheet) > 100f) AssertLanded(sheet, first.ShownCards.First().BlockKey, "the first card of the first rail");
            var jump = events.Raised.Single(e => e.Kind == CardEventKinds.NavJump);
            Assert.AreEqual(NavRailsBlockView.ElementKey, jump.Variant, "named by the rails block");
            Assert.AreEqual(first.ShownCards.First().BlockKey, jump.BlockKey);
            Assert.AreEqual(model.Sections[0].Key, jump.Value, "and the section it landed in");

            var withAll = view.Rails.FirstOrDefault(r => r.Plan.HasAll);
            if (withAll != null)
            {
                sheet.Stack.Scroll.scrollOffset = Vector2.zero;
                yield return CardTestInput.Settle();
                withAll.Strip.Show(withAll.AllCard.Button);
                yield return CardTestInput.Settle();
                events.Raised.Clear();
                var at = withAll.AllCard.Button.worldBound.center;
                Assert.IsTrue(withAll.Strip.Viewport.worldBound.Contains(at), "precondition: the All card was brought into the strip (" + at + ")");
                yield return RealTap(sheet, withAll.AllCard.Button);
                yield return Landed(sheet.Nav);
                if (ScrollRange(sheet) > 100f) AssertLanded(sheet, model.Sections[withAll.SectionIndex].Targets[0], "the All card");
                Assert.AreEqual(1, events.Raised.Count(e => e.Kind == CardEventKinds.NavJump), "one nav_jump");
            }
        }

        // The wall's Peek Next Card setting governs the rails too: off, a whole number of cards fills the strip and none shows a slice
        [UnityTest]
        public IEnumerator PeekNextCardOff_TheRailsShowWholeCardsOnly_NoSliceOfTheNext()
        {
            var entry = CardNavGalleryDefinitions.All.First(e => e.IsBlock && e.ElementKey == BuiltInBlocks.NavRailsKind && e.Case.Key == "big");
            _harness.Gallery.EnsureBuilt();
            _harness.Gallery.Language = entry.Case.Language;
            var wall = CardNavGalleryDefinitions.Wall(entry);
            wall.card_settings.container.peek_next_card = false;
            _harness.Gallery.ShowPoi(CardNavGalleryDefinitions.Poi(entry), wall, null, SheetStopRule.Stop.Full);
            yield return CardTestInput.Settle();
            var sheet = _harness.Gallery.Sheet;
            var view = sheet.Stack.BoundViews.OfType<NavRailsBlockView>().Single();
            float onePixel = CardGalleryChecks.OnePixel(view.Root);
            float narrowest = CardTestInput.TokenPx("--ts-rail-card");
            int railsWithMore = 0;
            foreach (var rail in view.Rails)
            {
                var cards = rail.ShownCards.ToList();
                float viewport = rail.Strip.VisibleWidth, gap = cards[0].Button.resolvedStyle.marginRight, width = cards[0].Button.layout.width;
                Assert.AreEqual(CardTrackRule.CellWidth(viewport, narrowest, gap), width, onePixel, rail.SectionKey + ": whole cards fill the strip");
                int whole = CardTrackRule.WholeCells(viewport, narrowest, gap);
                var cells = cards.Select(c => c.Button).ToList();
                if (rail.Plan.HasAll) cells.Add(rail.AllCard.Button);
                float edge = rail.Strip.Viewport.worldBound.xMax;
                Assert.LessOrEqual(cells.Take(whole).Last().worldBound.xMax, edge + onePixel, rail.SectionKey + ": the whole cards end inside the strip");
                if (cells.Count > whole)
                {
                    railsWithMore++;
                    Assert.GreaterOrEqual(cells[whole].worldBound.xMin, edge - onePixel, rail.SectionKey + ": the next card starts at the strip's edge: no slice shows");
                }
            }
            Assert.Greater(railsWithMore, 0, "precondition: some rail has cards past the first screen (it is The Lamp's size)");
        }

        // The bento block over every case: AUTHORED into the card right after the header (the builder never inserts it); four tiles for the four
        // blocks it leads to, in the author's order, each with the author's words or the block's own (the card's compact-row words), the first
        // with a picture, the third marked as opening the full view; the first and the lonely last tile span the row, the others sit two to a
        // row; every tile a full touch target, every word whole at 4.5:1, inside the card. A real tap on a jump tile lands its block (+-1 px)
        // with one nav_jump named nav_bento; a real tap on the full-view tile opens that block over the card and a real Back closes it.
        // Rendered Nav_nav_bento_<case>.png
        [UnityTest]
        public IEnumerator EveryBentoEntry_IsAuthoredRightAfterTheHeader_OneTilePerBlock_WholeAndTappable_ATapJumpsOrOpensTheFullView_AndRenders(
            [ValueSource(nameof(BentoEntryNames))] string name)
        {
            int index = CardNavGalleryDefinitions.All.ToList().FindIndex(e => e.Name == name);
            var entry = CardNavGalleryDefinitions.All[index];
            var events = new RecordingCardEvents();
            _harness.Gallery.Events = events;
            _harness.Show(index);
            yield return CardTestInput.Settle();
            var sheet = _harness.Gallery.Sheet;
            var model = sheet.Nav.Model;
            string key = CardNavGalleryDefinitions.BentoKey;
            Assert.AreEqual(CardOptions.NavNone, CardNavGalleryDefinitions.Wall(entry).card_settings.navigation.auto_nav_block, "the wall inserts nothing: the card authors the block");
            Assert.IsNull(sheet.Slots.TopView, "no element in any slot: the block judged alone");
            CollectionAssert.DoesNotContain(model.Targets, key, "the bento is no target of the card's own navigation");
            var slot = sheet.Stack.SlotOfBlock(key);
            Assert.IsNotNull(slot, "the authored bento is in the stack");
            Assert.AreEqual(0, sheet.Stack.Scroll.contentContainer.IndexOf(slot), "the first block of the scroll: right after the header");
            var view = sheet.Stack.BoundViews.OfType<NavBentoBlockView>().Single();
            var strings = Strings(_harness, entry);
            bool portuguese = entry.Case.Language == "pt";

            var tiles = view.Tiles;
            Assert.AreEqual(CardNavGalleryDefinitions.BentoTargets.Count, tiles.Count, "a tile per block it leads to (every one is reachable)");
            CollectionAssert.AreEqual(CardNavGalleryDefinitions.BentoTargets, tiles.Select(t => t.Target), "in the author's order");
            Assert.AreEqual(portuguese ? "O castelo" : "The castle", tiles[0].Words.text, "the author's words, in " + entry.Case.Language);
            Assert.AreEqual(portuguese ? "Ver de perto" : "Look closer", tiles[1].Words.text);
            for (int i = 2; i < tiles.Count; i++)
            {
                Assert.IsTrue(sheet.Nav.TryGetEntry(tiles[i].Target, out _), "precondition: tile " + i + "'s block is in the stack");
                Assert.AreEqual(sheet.Nav.RowOf(tiles[i].Target).Words, tiles[i].Words.text, "tile " + i + ": no words of its own: the block's own words");
                Assert.IsNotEmpty(tiles[i].Words.text);
            }
            Assert.IsTrue(tiles[0].HasPicture && tiles[0].Picture.Texture != null, "the first tile shows its picture");
            for (int i = 1; i < tiles.Count; i++) Assert.IsFalse(tiles[i].HasPicture, "tile " + i + " has none");
            // - a tile with no picture shows its block's family icon (in the family's colour) at its top, so it is never an empty box beside a picture
            //   tile; a tile with a picture shows the picture instead
            for (int i = 0; i < tiles.Count; i++)
            {
                var row = sheet.Nav.RowOf(tiles[i].Target);
                Assert.AreEqual(!tiles[i].HasPicture, tiles[i].Icon.resolvedStyle.display == DisplayStyle.Flex, "tile " + i + ": the icon shows exactly where there is no picture");
                if (!tiles[i].HasPicture) Assert.AreEqual(row.IconKey, CardIcons.KeyOf(tiles[i].Icon), "tile " + i + ": its block's family icon");
            }
            Assert.AreEqual(BentoTileRule.Opens.FullView, tiles[2].Opens, "the third tile opens the full view");
            Assert.AreEqual(BentoTileRule.Opens.Jump, tiles[0].Opens);
            Assert.AreEqual(DisplayStyle.Flex, tiles[2].FullViewGlyph.resolvedStyle.display, "and wears the full-view glyph");
            Assert.AreEqual(DisplayStyle.None, tiles[0].FullViewGlyph.resolvedStyle.display);

            var layer = sheet.Layer;
            float onePixel = CardGalleryChecks.OnePixel(view.Root);
            float touch = CardTestInput.TokenPx("--ts-touch-target");
            var card = sheet.Root.worldBound;
            var grid = view.Root.worldBound;
            Assert.GreaterOrEqual(grid.xMin, card.xMin - onePixel, "the block starts inside the card");
            Assert.LessOrEqual(grid.xMax, card.xMax + onePixel, "and ends inside it");
            var wide = BentoTileRule.Wide(tiles.Count);
            for (int i = 0; i < tiles.Count; i++)
            {
                var t = tiles[i];
                Assert.GreaterOrEqual(t.Button.layout.height, touch - onePixel, "tile " + i + ": tall enough to tap");
                Assert.GreaterOrEqual(t.Button.layout.width, touch - onePixel, "tile " + i + ": wide enough to tap");
                Assert.LessOrEqual(t.Button.worldBound.xMax, grid.xMax + onePixel, "tile " + i + ": inside the block");
                // - the cells span as the rule says: a wide one is the grid's width (less its own padding), the others about half
                float share = t.Cell.worldBound.width / grid.width;
                Assert.AreEqual(wide[i] ? 1f : 0.5f, share, 0.02f, "tile " + i + ": " + (wide[i] ? "spans the row" : "half a row") + " (share " + share.ToString("F2") + ")");
                AssertWordsWholeAndReadable(t.Button, layer, "tile " + i);
            }
            yield return CardGalleryChecks.Render(name);

            // - a real tap on the first tile (a jump): its block lands, one nav_jump named nav_bento
            sheet.Stack.Scroll.scrollOffset = Vector2.zero;
            yield return CardTestInput.Settle();
            yield return CardTestInput.Tap(sheet.Root.panel, tiles[0].Button.worldBound.center);
            yield return Landed(sheet.Nav);
            AssertLanded(sheet, tiles[0].Target, "the first tile");
            var jump = events.Raised.Single(e => e.Kind == CardEventKinds.NavJump);
            Assert.AreEqual(NavBentoBlockView.ElementKey, jump.Variant, "named by the bento block");
            Assert.AreEqual(tiles[0].Target, jump.BlockKey);
            Assert.IsFalse(sheet.Takeover.IsOpen, "a jump tile opens no full view");

            // - a real tap on the full-view tile: that block opens over the card, the card does not move, and a real Back closes it
            sheet.Stack.Scroll.scrollOffset = Vector2.zero;
            yield return CardTestInput.Settle();
            float offset = sheet.Stack.Scroll.scrollOffset.y;
            events.Raised.Clear();
            yield return CardTestInput.Tap(sheet.Root.panel, tiles[2].Button.worldBound.center);
            yield return CardTestInput.Settle();
            Assert.IsTrue(sheet.Takeover.IsOpen, "the full-view tile opened its block over the card");
            Assert.AreEqual(offset, sheet.Stack.Scroll.scrollOffset.y, onePixel, "and the card behind did not move");
            Assert.IsEmpty(events.Raised.Where(e => e.Kind == CardEventKinds.NavJump), "it is no jump");
            Assert.IsTrue(sheet.Back(), "Back closes the full view first");
            yield return CardTestInput.Settle();
            Assert.IsFalse(sheet.Takeover.IsOpen, "and the card is back");
        }
    }
}

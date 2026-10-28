using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // Phase B of step 11a (_3.2 section 5.6, nav_icon_grid, and section 3, Auto Nav Block) on the REAL LivingRoomScene with real touches:
    //   - Auto Nav Block = Icon Grid, pushed through the live seam: The Lamp (56 targets) shows the grid as the first block of its scroll,
    //     right under its header; Lamp - Military (4 targets, under Show Navigation From) gets none; None takes it away again, live
    //   - a REAL finger tap on every tile lands its family's first block (+-1 px, or the end clamps it) with ONE nav_jump named nav_icon_grid
    //   - the spy and the scroll are unaffected: the nav model is the same with and without the grid, a real drag names the section
    //     under the spy line with it in, and a Text Tabs tap still lands its block +-1 px
    //   - the stops do not move with it (195.00 / 337.60 / 796.00)
    //   - renders of the real grid at 390 px in English and Portuguese
    [Ignore(TestSkips.OldNavigationElements)]
    public class PoiCardNavBlockSceneTests : SearchSceneFixture
    {
        private const string Lamp = "lamp";
        private const string LampMilitary = "lamp_military";
        private static readonly string AutoKey = BlockStackBuilder.AutoNavBlockKey(BuiltInBlocks.NavIconGridKind);

        private PoiCardSheetView Sheet => Card.Sheet;
        private NavController Nav => Sheet.Nav;
        private CardNavModel Model => Nav.Model;
        private ScrollView Scroll => Sheet.Stack.Scroll;

        private sealed class RecordingCardEvents : ICardEvents
        {
            public readonly List<CardEvent> Raised = new();
            public void Raise(CardEvent cardEvent) => Raised.Add(cardEvent);
        }

        private RecordingCardEvents _events;
        private List<string> _shippedLanguages;

        [UnitySetUp]
        public IEnumerator UseAnInMemoryCardStateAndRecordTheEvents()
        {
            Card.State = new CardLocalState(new MemoryCardStateStore(), Session.SearchConfig.wall_id);
            _events = new RecordingCardEvents();
            Card.Events = _events;
            yield break;
        }

        // ---------------- helpers ----------------

        private float OnePixel => Sheet.Layer.panel.visualTree.layout.height / Screen.height;

        private float Range => NavScrollRule.Range(Scroll.contentContainer.layout.height, Scroll.contentViewport.layout.height);

        private List<CardEvent> NavJumps => _events.Raised.Where(e => e.Kind == CardEventKinds.NavJump).ToList();

        private VisualElement GridSlot => Sheet.Stack.SlotOfBlock(AutoKey);

        private List<Button> Tiles(VisualElement slot) =>
            slot.Q<VisualElement>("card-nav-icon-grid").Query<Button>(className: "nav-icon-grid__tile").ToList()
                .Where(b => b.resolvedStyle.display == DisplayStyle.Flex).ToList();

        // The wall's navigation for a test: Auto Nav Block `auto`, the card in `language`, pushed through the live seam
        private IEnumerator UseAuto(string auto, string language = "en")
        {
            var copy = ConfigCopy();
            copy.card_settings.navigation.auto_nav_block = auto;
            _shippedLanguages ??= new List<string>(ConfigCopy().card_settings.languages);
            copy.card_settings.languages = language == "en" ? new List<string>(_shippedLanguages) : new List<string> { language, "en" };
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
            yield return Rested();
        }

        // The sheet's height animates in real time: every measure waits for it to rest at its stop's height
        private IEnumerator Rested()
        {
            float rest = Sheet.Stops.HeightOf(Sheet.Stop);
            for (int frame = 0; frame < 600 && Mathf.Abs(Sheet.Root.layout.height - rest) > OnePixel; frame++) yield return null;
            Assert.AreEqual(rest, Sheet.Root.layout.height, OnePixel, "precondition: the sheet rests at its " + Sheet.Stop + " height");
            yield return null;
            yield return null;
        }

        private IEnumerator Landed()
        {
            for (int frame = 0; frame < 600 && Nav.IsJumping; frame++) yield return null;
            Assert.IsFalse(Nav.IsJumping, "the jump came to rest");
            yield return CardTestInput.Settle(0.3f);
        }

        // A block's top lands at the viewport's top (relative, one pixel), or the card's end clamps it
        private void AssertLanded(string block, string name)
        {
            var slot = Sheet.Stack.SlotOfBlock(block);
            if (Sheet.Stack.LandingTop(slot) <= Range - OnePixel)
                Assert.AreEqual(Sheet.Stack.LandingInset, Sheet.Stack.LandingElement(slot).worldBound.yMin - Scroll.contentViewport.worldBound.yMin, OnePixel, name + ": its top meets the viewport's top");
            else
                Assert.AreEqual(Range, Scroll.scrollOffset.y, OnePixel, name + ": a late block clamps at the end of the range");
        }

        // The section the reader is in, read from the SCREEN: the spy's own rule over each section's first slot as drawn
        private int SectionUnderTheSpyLine() =>
            CardTestInput.SectionUnderTheSpyLine(Scroll, Model, Sheet.Stack.SlotOfBlock);

        // A real tap by a touch pointer on a point of the card layer
        private IEnumerator TouchTap(Vector2 at)
        {
            yield return CardTestInput.TouchDrag(Sheet.Layer.panel, at, Vector2.zero, frames: 1);
            yield return CardTestInput.Settle();
        }

        private static string Print(CardNavModel model) =>
            string.Join(" / ", model.Sections.Select(s => s.Key + ":" + string.Join(",", s.Targets)));

        // ---------------- the tests ----------------

        [UnityTest]
        public IEnumerator AutoNavBlock_PutsTheGridFirstOnTheLampsScroll_NotOnLampMilitary_AndNoneTakesItAway_Live()
        {
            yield return UseAuto(CardOptions.NavNone);
            yield return Open(Lamp, SheetStopRule.Stop.Full);
            Assert.IsNull(GridSlot, "the shipped default (None): no grid");
            int blocksWithout = Scroll.contentContainer.childCount;

            yield return UseAuto(BuiltInBlocks.NavIconGridKind);
            yield return CardTestInput.Settle(0.3f);
            Assert.IsNotNull(GridSlot, "Icon Grid, live: The Lamp's open card gains the grid");
            // - a picture header's hero opens the scroll (it is the header's own part), so "first" = before the slot of every block of the card
            int gridAt = Scroll.contentContainer.IndexOf(Sheet.Stack.Sheets.SheetOf(GridSlot).Root);
            foreach (string target in Model.Targets)
                Assert.Less(gridAt, Scroll.contentContainer.IndexOf(Sheet.Stack.Sheets.SheetOf(Sheet.Stack.SlotOfBlock(target)).Root), "the grid comes before " + target);
            Assert.AreEqual(Scroll.contentContainer.IndexOf(Sheet.Stack.Sheets.SheetOf(Sheet.Stack.SlotOfBlock(Model.Targets[0])).Root), gridAt + 1,
                "right before the card's first block");
            Assert.AreEqual(blocksWithout + 1, Scroll.contentContainer.childCount, "exactly one block more");
            var tiles = Tiles(GridSlot);
            Assert.AreEqual(Model.Sections.Count, tiles.Count, "one tile per family of The Lamp");
            Assert.AreEqual(8, tiles.Count, "precondition: The Lamp's eight families that navigate");

            yield return Open(LampMilitary, SheetStopRule.Stop.Full);
            Assert.Less(Model.TargetCount, NavOntologyRule.MinBlocks(ConfigCopy().card_settings.navigation),
                "precondition: Lamp - Military is under Show Navigation From");
            Assert.IsNull(GridSlot, "Lamp - Military (" + Model.TargetCount + " targets, under Show Navigation From) gets no grid");

            yield return Open(Lamp, SheetStopRule.Stop.Full);
            Assert.IsNotNull(GridSlot, "back on The Lamp: the grid");
            yield return UseAuto(CardOptions.NavNone);
            Assert.IsNull(GridSlot, "None, live: gone again");
            Assert.AreEqual(blocksWithout, Scroll.contentContainer.childCount, "and the card is the one it was");
        }

        [UnityTest]
        public IEnumerator ARealTapOnEveryTile_LandsItsFamilysFirstBlock_WithOneNavJumpNamedNavIconGrid()
        {
            yield return UseAuto(BuiltInBlocks.NavIconGridKind);
            yield return Open(Lamp, SheetStopRule.Stop.Full);
            var tiles = Tiles(GridSlot);
            Assert.AreEqual(Model.Sections.Count, tiles.Count, "precondition: a tile per family");
            for (int i = tiles.Count - 1; i >= 0; i--)
            {
                Scroll.scrollOffset = Vector2.zero;
                yield return CardTestInput.Settle(0.3f);
                // - _3.3 S4a: the hero is Full's whole view and the grid comes after it -- bring the tile to the middle of what a finger reaches
                yield return CardTestInput.IntoReach(Sheet, Tiles(GridSlot)[i]);
                var tile = Tiles(GridSlot)[i];
                _events.Raised.Clear();
                yield return TouchTap(tile.worldBound.center);
                yield return Landed();
                var section = Model.Sections[i];
                AssertLanded(section.Targets[0], section.Key + "'s first block, from a real tap on its tile");
                Assert.AreEqual(1, NavJumps.Count, section.Key + ": one nav_jump");
                Assert.AreEqual(BuiltInBlocks.NavIconGridKind, NavJumps[0].Variant, "named by the grid");
                Assert.AreEqual(section.Targets[0], NavJumps[0].BlockKey, "to the family's first block");
                Assert.AreEqual(section.Key, NavJumps[0].Value, "in that family");
            }
        }

        [UnityTest]
        public IEnumerator TheSpyAndTheScroll_AreUnaffectedByTheInsertedGrid()
        {
            yield return UseAuto(CardOptions.NavNone);
            yield return Open(Lamp, SheetStopRule.Stop.Full);
            string without = Print(Model);

            yield return UseAuto(BuiltInBlocks.NavIconGridKind);
            yield return Open(Lamp, SheetStopRule.Stop.Full);
            Assert.IsNotNull(GridSlot, "precondition: the grid is in");
            Assert.AreEqual(without, Print(Model), "the same sections and targets with the grid as without it");
            CollectionAssert.DoesNotContain(Model.Targets, AutoKey, "the grid is no target");
            Assert.AreEqual(0, Nav.Current, "at the top the spy names the first family");

            // - a real finger drag: the spy names the section under its line, the grid in the stack above it
            var viewport = Scroll.contentViewport.worldBound;
            for (int drag = 0; drag < 3; drag++)
            {
                yield return CardTestInput.TouchDrag(Scroll.panel, viewport.center, new Vector2(0f, -viewport.height * 0.6f));
                yield return CardTestInput.Settle(0.6f);
                Assert.Greater(Scroll.scrollOffset.y, OnePixel, "precondition: the drag scrolls");
                Assert.AreEqual(SectionUnderTheSpyLine(), Nav.Current, "offset " + Scroll.scrollOffset.y.ToString("F0") + ": the section on screen");
            }

            // - a tap on the Navigation Bar (the card's navigation by default) lands its group's first block the same way with the grid as
            //   without it: a fresh card at Full each time, a real tap on every group
            foreach (string auto in new[] { CardOptions.NavNone, BuiltInBlocks.NavIconGridKind })
            {
                yield return UseAuto(auto);
                SelectionEventBus.Clear();
                yield return CardTestInput.Settle();
                yield return Open(Lamp, SheetStopRule.Stop.Full);
                Assert.AreEqual(auto != CardOptions.NavNone, GridSlot != null, "precondition: the grid is " + (auto == CardOptions.NavNone ? "out" : "in"));
                Assert.AreEqual(NavBarView.ElementKey, Sheet.Slots.BottomKey, "precondition: the default Navigation Bar is drawn at Full");
                var bar = Sheet.Slots.Bar;
                yield return CardTestInput.PastTheHero(Sheet);
                for (int target = 1; target < bar.Items.Count; target++)
                {
                    Assert.IsFalse(Sheet.BottomArea.ClassListContains(CardChromeView.BottomHiddenClass), "precondition: the bar shows before group " + target + "'s tap");
                    yield return TouchTap(bar.Items[target].Button.worldBound.center);
                    yield return Landed();
                    AssertLanded(bar.Entries[target].FirstTarget, bar.Entries[target].FirstTarget + " from its group (" + auto + ")");
                }
            }
        }

        [UnityTest]
        public IEnumerator TheStops_DoNotMove_WithTheGrid()
        {
            var measured = new Dictionary<string, float[]>();
            foreach (string auto in new[] { CardOptions.NavNone, BuiltInBlocks.NavIconGridKind })
            {
                yield return UseAuto(auto);
                var heights = new List<float>();
                foreach (var stop in new[] { SheetStopRule.Stop.Peek, SheetStopRule.Stop.Half, SheetStopRule.Stop.Full })
                {
                    yield return Open(Lamp, stop);
                    heights.Add(Sheet.Root.layout.height);
                }
                measured[auto] = heights.ToArray();
            }
            CollectionAssert.AreEqual(measured[CardOptions.NavNone], measured[BuiltInBlocks.NavIconGridKind], "peek / half / full: " +
                string.Join(", ", measured[BuiltInBlocks.NavIconGridKind].Select(h => h.ToString("F2"))));
        }

        [UnityTest]
        public IEnumerator TheRealGrid_ReadsWholeInEnglishAndPortuguese_AtThePhoneWidth_AndRenders([Values("en", "pt")] string language)
        {
            yield return UseAuto(BuiltInBlocks.NavIconGridKind, language);
            yield return Open(Lamp, SheetStopRule.Stop.Full);
            var grid = GridSlot.Q<VisualElement>("card-nav-icon-grid");
            var card = Sheet.Root.worldBound;
            Assert.GreaterOrEqual(grid.worldBound.xMin, card.xMin - OnePixel, "the grid starts inside the card");
            Assert.LessOrEqual(grid.worldBound.xMax, card.xMax + OnePixel, "and ends inside it");
            var words = new CardStrings(Card.StringTable.Entries(), CardStringSources.Shared.Entries(), null, language, "en");
            var tiles = Tiles(GridSlot);
            for (int i = 0; i < tiles.Count; i++)
            {
                var label = tiles[i].Q<Label>(className: "nav-tabs__label");
                Assert.AreEqual(words.Get(Model.Sections[i].LabelKey), label.text, "tile " + i + " in " + language);
                var size = label.MeasureTextSize(label.text, 0, VisualElement.MeasureMode.Undefined, 0, VisualElement.MeasureMode.Undefined);
                Assert.GreaterOrEqual(label.layout.width + 0.5f, size.x, "'" + label.text + "': whole");
                Assert.GreaterOrEqual(tiles[i].layout.height, CardTestInput.TokenPx("--ts-touch-target") - OnePixel, label.text + ": a full touch target");
            }
            yield return Capture("NavIconGrid_Lamp_full_" + language);
        }
    }
}

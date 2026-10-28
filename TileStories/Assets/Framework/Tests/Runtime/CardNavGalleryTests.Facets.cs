using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // Phase A of the in-card facets (_3.2 step 18, section 5.8): the two menus that hold the chips, drawer_tree and grid_overlay, over the
    // five fixture cases. Each menu's chip row is exactly the chips NavFacetRule offers for the case's blocks (the row is hidden when the
    // rule offers none); every chip is a full touch target with its words whole; a REAL tap (panel pointer events) on a chip narrows the
    // menu to exactly the blocks the rule keeps -- the sections and kinds listed, their counts -- marks the chip by a border AND bolder
    // words and brings a Clear chip; a real tap on Clear gives the whole list back. Rendered Nav_<key>_<case>_filtered.png.
    public partial class CardNavGalleryTests
    {
        private static string[] MenuEntryNames => CardNavGalleryDefinitions.All
            .Where(e => !e.IsBlock && e.Layout == null && (e.ElementKey == BuiltInNavElements.DrawerTree || e.ElementKey == BuiltInNavElements.GridOverlay) && IsElementCase(e))
            .Select(e => e.Name).ToArray();

        // Whether a button lies wholly inside the strip's visible part (one pixel of slack: Rect.Contains drops the far edges)
        private static bool WhollyIn(Rect visible, VisualElement button, float slack)
        {
            var box = button.worldBound;
            return box.xMin >= visible.xMin - slack && box.xMax <= visible.xMax + slack && box.yMin >= visible.yMin - slack && box.yMax <= visible.yMax + slack;
        }

        private static NavFacetBar FacetsOf(INavElementView menu) => menu is DrawerTreeNavView drawer ? drawer.Facets : ((GridOverlayNavView)menu).Facets;

        // What the menu must list under these facets: per section (the model's index) the blocks that pass, per kind too
        private static List<(int Section, List<string> Passing)> ExpectedSections(CardNavModel model, NavFacetBar bar, NavFacets facets) =>
            Enumerable.Range(0, model.Sections.Count)
                .Select(s => (Section: s, Passing: NavFacetRule.Passing(model.Sections[s].Targets, bar.FactsOf, facets)))
                .Where(x => x.Passing.Count > 0).ToList();

        private static void AssertTheMenuListsExactly(INavElementView menu, CardNavModel model, NavFacetBar bar, NavFacets facets, string when)
        {
            var expected = ExpectedSections(model, bar, facets);
            if (menu is DrawerTreeNavView drawer)
            {
                Assert.AreEqual(expected.Count, drawer.SectionRows.Count, when + ": a line per section that still has a block that passes, no other");
                for (int i = 0; i < expected.Count; i++)
                {
                    Assert.AreEqual(expected[i].Section, drawer.SectionOf(i), when + ": line " + i + " is the right section");
                    Assert.AreEqual(expected[i].Passing.Count.ToString(), drawer.SectionRows[i].Count.text, when + ": its count follows the filter");
                }
                int kinds = expected.Sum(x =>
                {
                    int left = model.Sections[x.Section].Kinds.Count(k => NavFacetRule.Passing(k.Targets, bar.FactsOf, facets).Count > 0);
                    return NavOntologyRule.OffersAKindChoice(left) ? left : 0;
                });
                Assert.AreEqual(kinds, drawer.KindRows.Count, when + ": the kinds listed are the kinds with a block that passes");
                foreach (var (row, section, kind) in drawer.KindRows)
                    Assert.AreEqual(NavFacetRule.Passing(kind.Targets, bar.FactsOf, facets).Count.ToString(), row.Count.text, when + ": a kind's count follows the filter");
                return;
            }
            var grid = (GridOverlayNavView)menu;
            Assert.AreEqual(expected.Count, grid.Tiles.Count, when + ": a tile per section that still has a block that passes");
            for (int i = 0; i < expected.Count; i++)
                Assert.AreEqual(expected[i].Passing.Count.ToString(), grid.Tiles[i].Count.text, when + ": tile " + i + " counts only what passes");
        }

        [UnityTest]
        public IEnumerator EveryMenuEntry_OffersTheChipsItsCardSupports_ARealTapNarrowsItToTheRulesBlocks_ClearBringsItBack_AndRenders(
            [ValueSource(nameof(MenuEntryNames))] string name)
        {
            int index = CardNavGalleryDefinitions.All.ToList().FindIndex(e => e.Name == name);
            var entry = CardNavGalleryDefinitions.All[index];
            _harness.Show(index);
            yield return CardTestInput.Settle();
            var sheet = _harness.Gallery.Sheet;
            var menu = sheet.Slots.MenuView;
            if (menu == null)
            {
                // - rule 2.5.7: one section, no menu (and so no chips): nothing to filter
                Assert.IsFalse(sheet.Overlay.IsModal, name + ": no menu is open for a card of one section");
                yield return CardGalleryChecks.Render(name + "_filtered");
                yield break;
            }
            var model = sheet.Nav.Model;
            var bar = FacetsOf(menu);
            float touch = CardTestInput.TokenPx("--ts-touch-target");
            float onePixel = CardGalleryChecks.OnePixel(menu.Root);

            // - the chip row is the rule's chips for this card's blocks, in its order, nothing else
            var offered = NavFacetRule.Chips(model.Targets.Select(bar.FactsOf).ToList(), new NavFacets());
            Assert.AreEqual(offered.Count, bar.Offered.Count, name + ": as many chips as the rule offers");
            for (int i = 0; i < offered.Count; i++)
            {
                Assert.AreEqual(offered[i].Dimension, bar.Offered[i].Dimension, name + ": chip " + i + " dimension");
                Assert.AreEqual(offered[i].Value, bar.Offered[i].Value, name + ": chip " + i + " value");
            }
            Assert.AreEqual(offered.Count, bar.Chips.Count, name + ": a button per offered chip, no Clear while nothing is on");
            AssertTheMenuListsExactly(menu, model, bar, new NavFacets(), name + " unfiltered");
            if (offered.Count == 0)
            {
                Assert.AreEqual(DisplayStyle.None, bar.Root.resolvedStyle.display, name + ": a card with nothing to narrow shows no chip row at all");
                yield return CardGalleryChecks.Render(name + "_filtered");
                yield break;
            }

            // - every chip a full touch target inside the menu, its words whole, marked by nothing yet
            var visible = bar.Strip.Viewport.worldBound;
            foreach (var chip in bar.Chips)
            {
                Assert.GreaterOrEqual(chip.Button.layout.height, touch - onePixel, chip.Label.text + ": tall enough to tap");
                Assert.GreaterOrEqual(chip.Button.layout.width, touch - onePixel, chip.Label.text + ": wide enough to tap");
                var words = chip.Label.MeasureTextSize(chip.Label.text, 0, VisualElement.MeasureMode.Undefined, 0, VisualElement.MeasureMode.Undefined);
                Assert.GreaterOrEqual(chip.Label.layout.width + 0.5f, words.x, "'" + chip.Label.text + "': the whole word fits");
                Assert.IsFalse(chip.Button.ClassListContains(NavChip.CurrentClass), chip.Label.text + ": not on yet");
            }

            // - a REAL tap on the first chip wholly inside the strip's visible part
            int pick = bar.Chips.ToList().FindIndex(c => WhollyIn(visible, c.Button, onePixel));
            Assert.GreaterOrEqual(pick, 0, name + ": precondition: a chip is wholly inside the strip " + visible);
            var tapped = bar.Offered[pick];
            yield return CardTestInput.TouchDrag(sheet.Layer.panel, bar.Chips[pick].Button.worldBound.center, Vector2.zero, frames: 1);
            yield return CardTestInput.Settle();
            var facets = new NavFacets();
            tapped.Toggle(facets);
            Assert.AreEqual(facets.Format, bar.Facets.Format, name + ": the tap switched the chip's format");
            Assert.AreEqual(facets.MaxMinutes, bar.Facets.MaxMinutes, name + ": ... its time");
            CollectionAssert.AreEqual(facets.Without, bar.Facets.Without, name + ": ... its without");
            AssertTheMenuListsExactly(menu, model, bar, facets, name + " filtered by " + tapped.Dimension + " " + tapped.Value);
            Assert.IsTrue(bar.HasClear, name + ": a Clear chip appears while a facet is on");
            var on = bar.Chips.Single(c => c.Button.ClassListContains(NavChip.CurrentClass));
            Assert.AreEqual(FontStyle.Bold, on.Label.resolvedStyle.unityFontStyleAndWeight, name + ": the chip that is on is marked by weight, not colour alone");
            Assert.Greater(on.Button.Q(className: "nav-chip__pill").resolvedStyle.borderTopWidth, 0f, name + ": and by a border");
            if (menu is GridOverlayNavView grid)
            {
                grid.ShowAxis(CardOptions.GroupByFormat);
                var byFormat = ExpectedSections(model.By(CardOptions.GroupByFormat), bar, facets);
                Assert.AreEqual(byFormat.Count, grid.Tiles.Count, name + ": the Format side is filtered by the same chips");
                grid.ShowAxis(CardOptions.GroupByFamily);
            }
            yield return CardGalleryChecks.Render(name + "_filtered");

            // - a REAL tap on Clear (the last chip) gives the whole list back
            var clear = bar.Chips[bar.Chips.Count - 1];
            Assert.IsTrue(clear.Button.ClassListContains(NavFacetBar.ClearClass), name + ": Clear is the last chip");
            Assert.IsTrue(sheet.Layer.worldBound.Contains(clear.Button.worldBound.center), name + ": precondition: Clear is on screen");
            if (!WhollyIn(visible, clear.Button, onePixel)) bar.Strip.Show(clear.Button);
            yield return CardTestInput.Settle(0.2f);
            yield return CardTestInput.TouchDrag(sheet.Layer.panel, clear.Button.worldBound.center, Vector2.zero, frames: 1);
            yield return CardTestInput.Settle();
            Assert.IsFalse(bar.Facets.IsActive, name + ": Clear switched every facet off");
            Assert.IsFalse(bar.HasClear, name + ": and Clear went");
            AssertTheMenuListsExactly(menu, model, bar, new NavFacets(), name + " cleared");
        }
    }
}

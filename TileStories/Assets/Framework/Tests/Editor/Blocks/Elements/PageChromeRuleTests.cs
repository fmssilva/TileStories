using NUnit.Framework;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // The Page Chrome's pure rules (_3.3 S2c, R17.5): which parts a block page shows (the wall's defaults, the block's own override), and
    // which part of the one-line breadcrumb gives way first when the line is too long.
    public class PageChromeRuleTests
    {
        private const float Floor = 10f;
        private static readonly float[] Floors = { Floor, Floor, Floor };

        // ---------------- PageCrumbFitRule ----------------

        [Test]
        public void ALineThatFits_KeepsEveryPartWhole()
        {
            var widths = PageCrumbFitRule.Widths(new[] { 100f, 60f, 120f }, 300f, Floors);
            CollectionAssert.AreEqual(new[] { 100f, 60f, 120f }, widths);
        }

        [Test]
        public void TheMiddlePart_IsCutFirst_AndOnlyAsMuchAsTheLineIsOver()
        {
            // - 280 natural in 250: 30 over, the section (60) gives 30 and nothing else moves
            var widths = PageCrumbFitRule.Widths(new[] { 100f, 60f, 120f }, 250f, Floors);
            CollectionAssert.AreEqual(new[] { 100f, 30f, 120f }, widths);
        }

        [Test]
        public void ThePointIsCutOnlyOnceTheMiddleIsAtItsFloor_AndTheNameLast()
        {
            // - 280 in 190: 90 over; the section gives 50 (to its floor), the point the other 40; the name is whole
            var cutPoint = PageCrumbFitRule.Widths(new[] { 100f, 60f, 120f }, 190f, Floors);
            CollectionAssert.AreEqual(new[] { 60f, Floor, 120f }, cutPoint);
            // - 280 in 100: the section and the point both at their floors, then the name takes the rest of the cut
            var cutName = PageCrumbFitRule.Widths(new[] { 100f, 60f, 120f }, 100f, Floors);
            CollectionAssert.AreEqual(new[] { Floor, Floor, 80f }, cutName);
        }

        [Test]
        public void WhenEvenTheFloorsDoNotFit_EveryPartIsAtItsFloor_NeverNegative()
        {
            var widths = PageCrumbFitRule.Widths(new[] { 100f, 60f, 120f }, 5f, Floors);
            CollectionAssert.AreEqual(new[] { Floor, Floor, Floor }, widths);
        }

        [Test]
        public void APartThatIsNotThere_StaysZero_AndNeverTakesTheCut()
        {
            // - no section (a page of no section): the point is cut before the name, the missing part stays 0
            var widths = PageCrumbFitRule.Widths(new[] { 150f, 0f, 150f }, 200f, Floors);
            CollectionAssert.AreEqual(new[] { 50f, 0f, 150f }, widths);
        }

        [Test]
        public void APartNarrowerThanItsFloor_IsNeverWidened()
        {
            var widths = PageCrumbFitRule.Widths(new[] { 4f, 4f, 300f }, 100f, Floors);
            Assert.AreEqual(4f, widths[PageCrumbFitRule.Point]);
            Assert.AreEqual(4f, widths[PageCrumbFitRule.Section]);
            Assert.AreEqual(92f, widths[PageCrumbFitRule.Name], 0.001f);
        }

        // ---------------- PageChromeRule ----------------

        [Test]
        public void AWallWithNoSettings_ShowsBothParts_AndTheFrameworkDefaultIsBoth()
        {
            Assert.IsTrue(PageChromeRule.Wall(null).Breadcrumb);
            Assert.IsTrue(PageChromeRule.Wall(null).PrevNext);
            var defaults = new CardComponentSettings();
            Assert.IsTrue(defaults.page_chrome_breadcrumb, "the schema default: on");
            Assert.IsTrue(defaults.page_chrome_prev_next, "the schema default: on");
        }

        [TestCase(true, true)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(false, false)]
        public void TheWallsDefault_IsWhatABlockWithNoOverrideShows(bool breadcrumb, bool prevNext)
        {
            var wall = new CardComponentSettings { page_chrome_breadcrumb = breadcrumb, page_chrome_prev_next = prevNext };
            var chrome = PageChromeRule.Of(wall, new BlockInstanceData { key = "block_1", kind = "story" });
            Assert.AreEqual(breadcrumb, chrome.Breadcrumb);
            Assert.AreEqual(prevNext, chrome.PrevNext);
        }

        [Test]
        public void ABlocksOverrideThatIsOn_ReplacesTheWallsValues_AndOneThatIsOffIsIgnoredWithItsValuesKept()
        {
            var wall = new CardComponentSettings { page_chrome_breadcrumb = true, page_chrome_prev_next = true };
            var block = new BlockInstanceData { key = "block_1", kind = "story" };
            var row = new BlockItemData();
            row.fields.Add(new BlockItemFieldValue { key = BlockSurfaces.ComponentKey, value = ElementOptions.ComponentPageChrome });
            row.fields.Add(new BlockItemFieldValue { key = BlockSurfaces.OverrideKey, flag = false });
            row.fields.Add(new BlockItemFieldValue { key = BlockSurfaces.BreadcrumbKey, value = ElementOptions.ChromeHidden });
            row.fields.Add(new BlockItemFieldValue { key = BlockSurfaces.PrevNextKey, value = ElementOptions.ChromeHidden });
            block.fields.Add(new BlockFieldValue { key = BlockSurfaces.ComponentOverridesField });
            block.fields[0].items.Add(row);

            var off = PageChromeRule.Of(wall, block);
            Assert.IsTrue(off.Breadcrumb && off.PrevNext, "Override off: the wall's, the block's values kept");

            row.fields[1].flag = true;
            var on = PageChromeRule.Of(wall, block);
            Assert.IsFalse(on.Breadcrumb, "Override on: the block's own Breadcrumb");
            Assert.IsFalse(on.PrevNext, "Override on: the block's own Previous / Next");

            // - an override row that names one part only: the other reads its default (shown), never the wall's
            row.fields.RemoveAt(3);
            wall.page_chrome_prev_next = false;
            var partial = PageChromeRule.Of(wall, block);
            Assert.IsFalse(partial.Breadcrumb);
            Assert.IsTrue(partial.PrevNext, "a part the override row does not store reads Shown");
        }

        [Test]
        public void AnOverrideForAnotherComponent_DoesNotChangeThePageChrome()
        {
            var wall = new CardComponentSettings { page_chrome_breadcrumb = false, page_chrome_prev_next = false };
            var block = new BlockInstanceData { key = "block_1", kind = "story" };
            var row = new BlockItemData();
            row.fields.Add(new BlockItemFieldValue { key = BlockSurfaces.ComponentKey, value = ElementOptions.ComponentOpenRow });
            row.fields.Add(new BlockItemFieldValue { key = BlockSurfaces.OverrideKey, flag = true });
            block.fields.Add(new BlockFieldValue { key = BlockSurfaces.ComponentOverridesField });
            block.fields[0].items.Add(row);
            var chrome = PageChromeRule.Of(wall, block);
            Assert.IsFalse(chrome.Breadcrumb);
            Assert.IsFalse(chrome.PrevNext);
        }

        [Test]
        public void AConfigWrittenBeforeThePageChrome_ReadsBothPartsOn()
        {
            // - a card_settings.components block that holds only the Open Row's emphasis: JsonUtility keeps the schema defaults for the rest
            var settings = JsonUtility.FromJson<CardSettings>("{\"components\":{\"open_row_emphasis\":\"shine\"}}");
            Assert.AreEqual("shine", settings.components.open_row_emphasis);
            Assert.IsTrue(settings.components.page_chrome_breadcrumb);
            Assert.IsTrue(settings.components.page_chrome_prev_next);
        }

        [Test]
        public void ThePageChromeComponent_IsOfferedToABlockAndItsTwoPartFieldsExistOnlyUnderIt()
        {
            CollectionAssert.Contains(ElementOptions.Components, ElementOptions.ComponentPageChrome);
            Assert.AreEqual(ElementOptions.Components.Length, ElementOptions.ComponentLabels.Length);
            var fields = BlockSurfaces.ComponentOverridesDefinition.ItemFields;
            int shownWhenPageChrome = 0;
            foreach (var f in fields)
                if (f.Key == BlockSurfaces.BreadcrumbKey || f.Key == BlockSurfaces.PrevNextKey)
                {
                    Assert.IsNotNull(f.ShownWhen, f.Key);
                    shownWhenPageChrome++;
                }
            Assert.AreEqual(2, shownWhenPageChrome);
        }
    }
}

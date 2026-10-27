using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories.Editor.Tests
{
    // The Page Chrome (_3.3 S2c) on a REAL sheet with the shipped wall's real cards, no panel (Phase B taps and looks for real):
    //   - the top bar: a Back arrow that draws no word, its tooltip the card's own words (EN + PT), the breadcrumb on one line's parts
    //   - the foot: Previous / Next naming the neighbouring pages in the card's words, one card at either end, none for a page alone
    //   - the wall's two switches hide the breadcrumb / the foot, and a block's own override replaces them for that block's page only
    public class PageChromeSheetTests
    {
        private const string FrameworkStringsPath = "Assets/Framework/Runtime/UI/Screen/Cards/CardStrings.asset";

        private static WallConfigData Shipped() => JsonUtility.FromJson<WallConfigData>(File.ReadAllText("Assets/Apps/LivingRoom/config.json"));

        // Every sheet a test opened: closed in TearDown, so a page turned onto a gyro panorama gives the device's sensor back (its count is
        // static and the Panorama tests that run after read it)
        private readonly System.Collections.Generic.List<PoiCardSheetView> _sheets = new();

        private PoiCardSheetView Sheet()
        {
            var sheet = new PoiCardSheetView(new VisualElement(), BlockRegistry.Shared, Array.Empty<StyleSheet>());
            _sheets.Add(sheet);
            return sheet;
        }

        [TearDown]
        public void CloseEverySheet()
        {
            foreach (var sheet in _sheets) sheet.Hide();
            _sheets.Clear();
        }

        private static BlockBindContext ShowCard(PoiCardSheetView sheet, WallConfigData wall, string poiId, string language = "en")
        {
            var media = new ResourcesMediaSource(wall.card_settings.media_resources_path);
            var poi = wall.pois.Single(p => p.id == poiId);
            var stack = BlockStackBuilder.Build(poi, wall.card_settings, BlockRegistry.Shared, wall.pois);
            var context = CardContextBuilder.Build(new BlockBindContext { Media = media }, poi, wall, wall.card_settings, language,
                AssetDatabase.LoadAssetAtPath<CardStringTable>(FrameworkStringsPath).Entries(), CardStringSources.Shared.Entries());
            sheet.Show(stack.Entries, context, SheetStopRule.Stop.Full, wall.card_settings.container.half_max_ratio);
            return context;
        }

        private static bool FootIsOnThePage(PoiCardSheetView sheet) => sheet.FullView.Foot.hierarchy.parent == sheet.FullView.Scroll.contentContainer;

        [TestCase("en")]
        [TestCase("pt")]
        public void TheBackArrow_DrawsNoWord_ItsTooltipIsTheCardsOwnWords_AndItIsAWholeTarget(string language)
        {
            var wall = Shipped();
            var sheet = Sheet();
            var context = ShowCard(sheet, wall, "lamp", language);
            sheet.OpenFullView("block_58");
            var back = sheet.Takeover.Back;
            Assert.AreEqual("", back.text, "no word on the arrow");
            Assert.AreEqual(context.Strings.Get(CardStrings.Keys.PageBack), back.tooltip, "its accessible name, in the card's language");
            Assert.IsNotEmpty(back.tooltip);
            if (language != "en")
            {
                var english = Sheet();
                ShowCard(english, wall, "lamp", "en");
                english.OpenFullView("block_58");
                Assert.AreNotEqual(english.Takeover.Back.tooltip, back.tooltip, "the card speaks the visitor's language, the name too");
            }
        }

        [Test]
        public void TheFoot_NamesTheNeighbouringPages_OneCardAtEitherEnd_AndEveryTurnMovesIt()
        {
            var wall = Shipped();
            var sheet = Sheet();
            var context = ShowCard(sheet, wall, "lamp");
            var model = sheet.Nav.Model;
            var siblings = FullViewSiblingRule.Siblings(model, BlockRegistry.Shared, "block_58").ToList();
            sheet.OpenFullView("block_58");
            var takeover = sheet.Takeover;
            var foot = sheet.FullView.Foot;
            int at = siblings.IndexOf("block_58");
            Assert.IsTrue(FootIsOnThePage(sheet), "the foot is at the end of the page's scroll");
            Assert.AreSame(sheet.FullView.Scroll.contentContainer[sheet.FullView.Scroll.contentContainer.childCount - 1], foot, "the LAST thing on the page");
            Assert.AreEqual(context.Strings.Get(CardStrings.Keys.PagePrevious), foot.PreviousLabel.text);
            Assert.AreEqual(context.Strings.Get(CardStrings.Keys.PageNext), foot.NextLabel.text);
            Assert.AreEqual(sheet.Nav.RowOf(siblings[at - 1]).Words, foot.PreviousName.text, "Previous names the block before");
            Assert.AreEqual(sheet.Nav.RowOf(siblings[at + 1]).Words, foot.NextName.text, "Next names the block after");
            Assert.IsTrue(foot.Previous.visible && foot.Next.visible);

            takeover.Turn(+1);
            Assert.AreEqual(sheet.Nav.RowOf(siblings[at]).Words, foot.PreviousName.text, "after a turn Previous names the block just left");
            Assert.AreEqual(sheet.Nav.RowOf(siblings[at + 2]).Words, foot.NextName.text, "and Next the one after the new page");
            Assert.IsTrue(FootIsOnThePage(sheet), "still one foot on the new page");

            takeover.ShowPage(0);
            Assert.IsFalse(foot.Previous.visible, "no page before the first: no Previous card");
            Assert.IsTrue(foot.Next.visible, "Next stays");
            takeover.ShowPage(siblings.Count - 1);
            Assert.IsTrue(foot.Previous.visible);
            Assert.IsFalse(foot.Next.visible, "no page after the last: no Next card");
            Assert.AreEqual("", foot.Next.tooltip, "an absent card names nothing");
        }

        [Test]
        public void APageAlone_HasNoFoot_ABackThatClosesLeavesNoFootBehind()
        {
            var wall = Shipped();
            var sheet = Sheet();
            ShowCard(sheet, wall, "lamp");
            var listen = sheet.Nav.Model.Sections.Single(s => s.Key == BuiltInFamilies.Guides);
            sheet.OpenFullView(listen.Targets[0]);
            Assert.AreEqual(1, sheet.Takeover.PageCount, "precondition: Listen is a family of one");
            Assert.IsFalse(FootIsOnThePage(sheet), "nothing to turn to: no foot");
            sheet.OpenFullView("block_58");
            Assert.IsTrue(FootIsOnThePage(sheet));
            Assert.IsTrue(sheet.Back());
            Assert.IsFalse(FootIsOnThePage(sheet), "closed: the foot left the page");
        }

        [Test]
        public void TheWallsSwitches_HideTheBreadcrumbAndTheFoot_EachOnItsOwn()
        {
            var wall = Shipped();
            var sheet = Sheet();
            ShowCard(sheet, wall, "lamp");
            sheet.OpenFullView("block_58");
            Assert.AreNotEqual("", sheet.Takeover.CrumbText, "precondition: the breadcrumb reads");
            Assert.AreNotEqual(DisplayStyle.None, sheet.Takeover.Crumb.style.display.value);
            sheet.Hide();

            wall.card_settings.components.page_chrome_breadcrumb = false;
            ShowCard(sheet, wall, "lamp");
            sheet.OpenFullView("block_58");
            Assert.AreEqual("", sheet.Takeover.CrumbText, "breadcrumb off: nothing reads");
            Assert.AreEqual(DisplayStyle.None, sheet.Takeover.Crumb.style.display.value);
            Assert.IsNotNull(sheet.Takeover.Back.parent, "the Back arrow stands alone");
            Assert.IsTrue(FootIsOnThePage(sheet), "the foot is its own switch");
            sheet.Hide();

            wall.card_settings.components.page_chrome_breadcrumb = true;
            wall.card_settings.components.page_chrome_prev_next = false;
            ShowCard(sheet, wall, "lamp");
            sheet.OpenFullView("block_58");
            Assert.IsFalse(FootIsOnThePage(sheet), "Previous / Next off: no foot");
            Assert.AreNotEqual("", sheet.Takeover.CrumbText, "the breadcrumb is its own switch");
            Assert.IsTrue(sheet.FullView.TurnsBetweenBlocks, "the sibling swipe and the chips are unchanged");
            Assert.AreEqual(sheet.FullView.Scroll.contentContainer.childCount, 1, "just the page");
        }

        // The S2c review: a page that shows a big title of its own (the story) is named by its CONTEXT, "point > section"; a neighbour
        // without one (a rich text) ends with its name again, on the same open view
        [Test]
        public void TheStorysPage_IsNamedByItsContextAlone_ANeighbourWithoutATitleEndsWithItsName()
        {
            var wall = Shipped();
            var painting = wall.pois.Single(p => p.id == PaintingStory.PaintingId);
            string storyKey = painting.card.blocks.Single(b => b.kind == "story").key;
            var sheet = Sheet();
            ShowCard(sheet, wall, PaintingStory.PaintingId);
            var siblings = FullViewSiblingRule.Siblings(sheet.Nav.Model, BlockRegistry.Shared, storyKey).ToList();
            Assert.GreaterOrEqual(siblings.Count, 2, "precondition: the story has neighbours to turn to");
            sheet.OpenFullView(storyKey);
            var bar = sheet.Takeover.Bar;
            Assert.IsTrue(sheet.Takeover.SectionShown, "precondition: the story's page names its section");
            Assert.AreEqual(bar.Point.text + " > " + bar.SectionWords.text, sheet.Takeover.CrumbText, "the story: point > section, nothing after");
            Assert.IsFalse(bar.NameShown, "the page's name is left out");
            Assert.AreEqual(DisplayStyle.None, bar.Name.style.display.value, "and not drawn");

            int at = siblings.IndexOf(storyKey);
            sheet.Takeover.ShowPage(at == 0 ? 1 : at - 1);
            string neighbour = sheet.FullView.BlockKey;
            Assert.AreNotEqual(storyKey, neighbour);
            Assert.IsTrue(bar.NameShown, "a neighbour with no title of its own is named again");
            StringAssert.EndsWith(sheet.Takeover.NameOf(sheet.Takeover.PageIndex), sheet.Takeover.CrumbText, "its trail ends with its name");
        }

        [Test]
        public void ABlocksOwnOverride_ReplacesTheWallsChoiceForThatBlocksPageOnly()
        {
            var wall = Shipped();
            var painting = wall.pois.Single(p => p.id == PaintingStory.PaintingId);
            var story = painting.card.blocks.Single(b => b.kind == "story");
            string storyKey = story.key;
            var row = new BlockItemData();
            row.fields.Add(new BlockItemFieldValue { key = BlockSurfaces.ComponentKey, value = ElementOptions.ComponentPageChrome });
            row.fields.Add(new BlockItemFieldValue { key = BlockSurfaces.OverrideKey, flag = true });
            row.fields.Add(new BlockItemFieldValue { key = BlockSurfaces.BreadcrumbKey, value = ElementOptions.ChromeHidden });
            row.fields.Add(new BlockItemFieldValue { key = BlockSurfaces.PrevNextKey, value = ElementOptions.ChromeHidden });
            var overrides = story.fields.Find(f => f.key == BlockSurfaces.ComponentOverridesField);
            if (overrides == null)
            {
                overrides = new BlockFieldValue { key = BlockSurfaces.ComponentOverridesField };
                story.fields.Add(overrides);
            }
            overrides.items.Add(row);

            var sheet = Sheet();
            ShowCard(sheet, wall, PaintingStory.PaintingId);
            var siblings = FullViewSiblingRule.Siblings(sheet.Nav.Model, BlockRegistry.Shared, storyKey).ToList();
            Assert.GreaterOrEqual(siblings.Count, 2, "precondition: the story has neighbours to turn to");
            sheet.OpenFullView(storyKey);
            Assert.AreEqual(storyKey, sheet.FullView.BlockKey, "precondition: the story's page is open");
            Assert.AreEqual("", sheet.Takeover.CrumbText, "the story's override hides its breadcrumb");
            Assert.IsFalse(FootIsOnThePage(sheet), "and its Previous / Next row");

            // - turn to a neighbour that has no override: the wall's choice (both on) is back, on the same open view
            int at = siblings.IndexOf(storyKey);
            sheet.Takeover.ShowPage(at == 0 ? 1 : at - 1);
            Assert.AreNotEqual(storyKey, sheet.FullView.BlockKey);
            Assert.AreNotEqual("", sheet.Takeover.CrumbText, "another block follows the wall: the breadcrumb is back");
            Assert.IsTrue(FootIsOnThePage(sheet), "and the foot");

            // - Override off: the story follows the wall again, its values kept in the row
            row.fields.Find(f => f.key == BlockSurfaces.OverrideKey).flag = false;
            sheet.Hide();
            ShowCard(sheet, wall, PaintingStory.PaintingId);
            sheet.OpenFullView(storyKey);
            Assert.AreNotEqual("", sheet.Takeover.CrumbText, "Override off: the wall's breadcrumb");
            Assert.IsTrue(FootIsOnThePage(sheet), "Override off: the wall's foot");
            Assert.AreEqual(ElementOptions.ChromeHidden, row.fields.Find(f => f.key == BlockSurfaces.BreadcrumbKey).value, "its values are kept");
        }
    }
}

using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories.Editor.Tests
{
    // "Open in full view" for any block (_3.2 step 12, BlockFullView) on a REAL sheet with The Lamp's real card, the shipped words and the
    // wall's real media folder: the full view binds a SECOND pooled view of the block's kind through the takeover's own scope while the card's
    // own view stays bound and keeps its files; leaving unbinds that view BEFORE the takeover gives its files back and returns it to the pool;
    // the full view survives a rebind of the same point (a language switch, a live edit, reduce motion) and closes for another point, Back,
    // a lowered card and a block not on the card. The owners handed over and back with real input are PoiCardExpandableSceneTests'.
    public class BlockFullViewTests
    {
        private const string FrameworkStringsPath = "Assets/Framework/Runtime/UI/Cards/CardStrings.asset";
        private const string Lamp = "lamp";
        private const string LampMilitary = "lamp_military";

        private static WallConfigData Shipped() =>
            JsonUtility.FromJson<WallConfigData>(File.ReadAllText("Assets/Apps/LivingRoom/config.json"));

        private static PoiCardSheetView Sheet() => new(new VisualElement(), BlockRegistry.Shared, Array.Empty<StyleSheet>());

        private static void ShowCard(PoiCardSheetView sheet, WallConfigData wall, IMediaSource media, string poiId = Lamp, string language = "en")
        {
            var poi = wall.pois.Single(p => p.id == poiId);
            var stack = BlockStackBuilder.Build(poi, wall.card_settings, BlockRegistry.Shared, wall.pois);
            var context = CardContextBuilder.Build(new BlockBindContext { Media = media }, poi, wall, wall.card_settings, language,
                AssetDatabase.LoadAssetAtPath<CardStringTable>(FrameworkStringsPath).Entries(), CardStringSources.Shared.Entries());
            sheet.Show(stack.Entries, context, SheetStopRule.Stop.Full, wall.card_settings.container.half_max_ratio);
        }

        [OneTimeSetUp]
        public void Preconditions()
        {
            Assert.IsTrue(BlockRegistry.Shared.TryGet("size_comparison", out _), "precondition: the LivingRoom app's kind is registered in Edit Mode");
        }

        [Test]
        public void TheFullView_BindsASecondPooledViewOfTheKind_ThroughItsOwnScope_WhileTheCardsViewStaysBound()
        {
            var wall = Shipped();
            var media = new ResourcesMediaSource(wall.card_settings.media_resources_path);
            var sheet = Sheet();
            ShowCard(sheet, wall, media);
            var cards = sheet.Stack.ViewOfBlock("block_29");
            int cardFiles = sheet.Stack.HeldMediaOf("block_29");
            Assert.IsInstanceOf<GalleryBlockView>(cards, "precondition: The Lamp's carousel gallery, inline");
            Assert.Greater(cardFiles, 0, "precondition: the card's gallery holds its pictures");
            int loaded = media.LoadedCount;

            sheet.OpenFullView("block_29");
            Assert.IsTrue(sheet.FullView.IsOpen);
            Assert.AreEqual("block_29", sheet.FullView.BlockKey);
            var full = sheet.FullView.View;
            Assert.IsInstanceOf<GalleryBlockView>(full, "a view of the same kind");
            Assert.AreNotSame(cards, full, "a SECOND view: the card's own is not moved");
            Assert.AreSame(cards, sheet.Stack.ViewOfBlock("block_29"), "the card's view is still bound in its slot");
            // - parent is the LOGICAL parent in UI Toolkit: the ScrollView, whose content container holds it (hierarchy.parent)
            Assert.AreSame(sheet.FullView.Scroll, full.Root.parent, "drawn in the full view's own scroll...");
            Assert.AreSame(sheet.FullView.Scroll.contentContainer, full.Root.hierarchy.parent);
            Assert.AreSame(sheet.Takeover.Page, sheet.FullView.Scroll.parent, "...on the takeover's page");
            Assert.IsTrue(full.Root.ClassListContains(BlockFullView.BlockClass), "growing to the page's height");
            Assert.AreEqual(cardFiles, sheet.Stack.HeldMediaOf("block_29"), "the card's files are untouched");
            Assert.Greater(sheet.Takeover.HeldMedia, 0, "the full view loads its own through its own scope");
            // - step 16: the breadcrumb climbs the card's own structure -- the title, the block's section (Media: the shipped English words of
            //   the family, _3.2 section 2.2), then the words its row would show
            Assert.AreEqual(sheet.Stack.HeaderView.TitleText + " > Media > In pictures", sheet.Takeover.CrumbText,
                "the breadcrumb: the card's title, its section, then the words its row would show");

            Assert.IsTrue(sheet.Back(), "Back closes the full view first");
            Assert.IsFalse(sheet.FullView.IsOpen);
            Assert.IsNull(sheet.FullView.View);
            Assert.IsNull(full.Root.parent, "the second view left the page");
            Assert.IsFalse(full.Root.ClassListContains(BlockFullView.BlockClass), "and the full view's class with it (the pool hands it to the card next)");
            Assert.AreEqual(0, sheet.Takeover.HeldMedia, "every file of the full view given back");
            Assert.AreEqual(loaded, media.LoadedCount, "the card holds exactly what it held before");
            Assert.AreSame(cards, sheet.Stack.ViewOfBlock("block_29"), "and its view is still bound");
            Assert.AreEqual(SheetStopRule.Stop.Full, sheet.Stop, "the card did not move");

            int created = sheet.Stack.CreatedViewCount;
            sheet.OpenFullView("block_29");
            Assert.AreSame(full, sheet.FullView.View, "opened again: the pooled second view");
            Assert.AreEqual(created, sheet.Stack.CreatedViewCount, "nothing new built");
            sheet.Hide();
            Assert.AreEqual(0, media.LoadedCount, "closing the card with the full view open holds nothing");
        }

        [Test]
        public void LeavingThePage_UnbindsTheViewFirst_ThenTheTakeoverGivesItsFilesBack()
        {
            var wall = Shipped();
            var media = new ResourcesMediaSource(wall.card_settings.media_resources_path);
            var poi = wall.pois.Single(p => p.id == Lamp);
            string picture = new BlockFieldReader(poi.card.blocks.Single(b => b.key == "block_33"), "en", "en").ValidAsset(BuiltInBlocks.BeforeAfterBeforeField, MediaKind.Image);
            Assert.Greater(picture.Length, 0, "precondition: a real picture of The Lamp");
            var takeover = new TakeoverView(new VisualElement());
            int heldWhenLeaving = -1, shown = 0;
            takeover.Open("crumb", _ => "", 2, 0, media, (view, container, scope, page) =>
            {
                shown++;
                Assert.IsNotNull(scope.Load<Texture2D>(picture));
                view.WhenPageLeaves(() => heldWhenLeaving = view.HeldMedia);
            }, null);
            Assert.AreEqual(1, takeover.HeldMedia);
            takeover.ShowPage(1);
            Assert.AreEqual(1, heldWhenLeaving, "the page left while its file was still held: release ran first");
            Assert.AreEqual(1, takeover.HeldMedia, "the new page holds its own");
            heldWhenLeaving = -1;
            takeover.Close();
            Assert.AreEqual(1, heldWhenLeaving, "on close too");
            Assert.AreEqual(0, takeover.HeldMedia);
            Assert.AreEqual(0, media.LoadedCount);
            Assert.AreEqual(2, shown);
        }

        [Test]
        public void TheFullView_SurvivesARebindOfTheSamePoint_AndClosesForAnotherPoint_ALoweredCard_OrABlockNotOnTheCard()
        {
            var wall = Shipped();
            var media = new ResourcesMediaSource(wall.card_settings.media_resources_path);
            var sheet = Sheet();
            ShowCard(sheet, wall, media);
            sheet.OpenFullView("block_2");

            // - the language switch: the same point bound again -- the full view stays, drawn in Portuguese
            ShowCard(sheet, wall, media, language: "pt");
            Assert.IsTrue(sheet.FullView.IsOpen, "still open after the language switch");
            Assert.AreEqual("block_2", sheet.FullView.BlockKey);
            StringAssert.EndsWith("> A história", sheet.Takeover.CrumbText, "its breadcrumb in the new language");

            // - a live edit that turns Reduce Motion on: the same point again, the full view bound with it
            wall.card_settings.container.reduce_motion = true;
            ShowCard(sheet, wall, media, language: "pt");
            Assert.IsTrue(sheet.FullView.IsOpen, "still open after a live edit");
            Assert.IsInstanceOf<RichTextBlockView>(sheet.FullView.View, "a view of the kind bound again");
            CollectionAssert.DoesNotContain(sheet.Stack.BoundViews, sheet.FullView.View, "never one the card itself is showing");

            // - a live edit that deletes the block: nothing to reopen
            var poi = wall.pois.Single(p => p.id == Lamp);
            var removed = poi.card.blocks.Single(b => b.key == "block_2");
            poi.card.blocks.Remove(removed);
            ShowCard(sheet, wall, media, language: "pt");
            Assert.IsFalse(sheet.FullView.IsOpen, "the block is gone: no full view");
            poi.card.blocks.Insert(0, removed);

            ShowCard(sheet, wall, media);
            sheet.OpenFullView("block_2");
            ShowCard(sheet, wall, media, LampMilitary);
            Assert.IsFalse(sheet.FullView.IsOpen, "another point closes it");

            ShowCard(sheet, wall, media);
            sheet.OpenFullView("block_2");
            sheet.ShowHeaderAtPeek();
            Assert.IsFalse(sheet.FullView.IsOpen, "a model placed from it lowers the card: the full view goes with it");
            Assert.AreEqual(SheetStopRule.Stop.Peek, sheet.Stop);
            sheet.SetStop(SheetStopRule.Stop.Full);
            sheet.OpenFullView("block_2");
            sheet.ShowOnWall();
            Assert.IsFalse(sheet.FullView.IsOpen, "show on wall: the wall, not the full view");

            sheet.SetStop(SheetStopRule.Stop.Full);
            sheet.OpenFullView("block_no_such");
            Assert.IsFalse(sheet.Takeover.IsOpen, "a block not on this card opens nothing");
            sheet.Hide();
            Assert.AreEqual(0, media.LoadedCount);
        }

        [Test]
        public void AVideoInTheFullView_IsTheWholePlayer_WithNoFullScreenOfItsOwnToOffer()
        {
            var wall = Shipped();
            var sheet = Sheet();
            ShowCard(sheet, wall, new ResourcesMediaSource(wall.card_settings.media_resources_path));
            var cards = (VideoBlockView)sheet.Stack.ViewOfBlock("block_57");
            Assert.AreEqual(CardOptions.DisplayTakeover, cards.Display, "precondition: The Lamp's chapters video is a teaser on the card");

            sheet.OpenFullView("block_57");
            var full = (VideoBlockView)sheet.FullView.View;
            Assert.AreEqual(CardOptions.DisplayInline, full.Display, "the whole player, not a second teaser");
            Assert.AreEqual(DisplayStyle.Flex, full.Panel.Root.style.display.value);
            Assert.AreEqual(DisplayStyle.None, full.Panel.FullScreenButton.style.display.value, "full screen already: no button to open one again");
            Assert.AreEqual(DisplayStyle.None, full.Teaser.style.display.value, "no teaser");
            Assert.IsTrue(full.Panel.Root.ClassListContains("card-video--full"), "the player takes the page's height");
            Assert.IsFalse(cards.Panel.Root.ClassListContains("card-video--full"), "the card's own does not");
            Assert.AreEqual(DisplayStyle.Flex, cards.Teaser.style.display.value, "the card keeps its own teaser");
            sheet.Hide();
        }
    }
}

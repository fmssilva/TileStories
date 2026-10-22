using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories.Editor.Tests
{
    // CardFullViewOpener (_3.2 chunk D: what opens in the full view, out of PoiCardSheetView) on a REAL sheet with The Lamp's real card and
    // the shipped words -- the edges the sheet-level full view tests (BlockFullViewTests, FullViewSiblingsSheetTests, BentoLayoutSheetTests)
    // do not reach:
    //   - a same-point rebind that DELETED the open block (a live edit) reopens nothing, and the card stays where it was
    //   - a same-point rebind closes a gallery's lightbox for good: only a block's full view or a bento section is reopened
    //   - a closed card opens nothing, by either door; a section outside the card's map opens no page and says so
    public class CardFullViewOpenerTests
    {
        private const string FrameworkStringsPath = "Assets/Framework/Runtime/UI/Cards/CardStrings.asset";
        private const string Lamp = "lamp";
        private const string OpenBlock = "block_58";

        private static WallConfigData Shipped() => JsonUtility.FromJson<WallConfigData>(File.ReadAllText("Assets/Apps/LivingRoom/config.json"));

        private static PoiCardSheetView Sheet() => new(new VisualElement(), BlockRegistry.Shared, Array.Empty<StyleSheet>());

        private static void ShowCard(PoiCardSheetView sheet, WallConfigData wall, string language = "en")
        {
            var poi = wall.pois.Single(p => p.id == Lamp);
            var stack = BlockStackBuilder.Build(poi, wall.card_settings, BlockRegistry.Shared, wall.pois);
            var context = CardContextBuilder.Build(new BlockBindContext { Media = new ResourcesMediaSource(wall.card_settings.media_resources_path) },
                poi, wall, wall.card_settings, language, AssetDatabase.LoadAssetAtPath<CardStringTable>(FrameworkStringsPath).Entries(),
                CardStringSources.Shared.Entries());
            sheet.Show(stack.Entries, context, SheetStopRule.Stop.Full, wall.card_settings.container.half_max_ratio);
        }

        [Test]
        public void ASamePointRebindThatDeletedTheOpenBlock_ReopensNothing_AndTheCardStaysWhereItWas()
        {
            var wall = Shipped();
            var sheet = Sheet();
            ShowCard(sheet, wall);
            sheet.FullViewOpener.OpenFullView(OpenBlock);
            Assert.IsTrue(sheet.FullView.IsOpen, "precondition: " + OpenBlock + " opens in the full view");
            Assert.AreEqual(OpenBlock, sheet.FullView.BlockKey);

            // - the live edit: the same point, without the block that was open
            var edited = JsonUtility.FromJson<WallConfigData>(JsonUtility.ToJson(wall));
            var lamp = edited.pois.Single(p => p.id == Lamp);
            int removed = lamp.card.blocks.RemoveAll(b => b.key == OpenBlock);
            Assert.AreEqual(1, removed, "precondition: the edit deleted exactly the open block");
            ShowCard(sheet, edited);

            Assert.IsFalse(sheet.Stack.TryGetEntryOfBlock(OpenBlock, out _), "precondition: the rebound card has no " + OpenBlock);
            Assert.IsFalse(sheet.FullView.IsOpen, "a deleted block is not reopened");
            Assert.IsFalse(sheet.Takeover.IsOpen, "and nothing else opens in its place");
            Assert.IsTrue(sheet.IsOpen, "the card itself stays open");
            Assert.AreEqual(SheetStopRule.Stop.Full, sheet.Stop, "at the stop it had");

            // - the next same-point rebind (a language switch) has nothing to reopen either
            ShowCard(sheet, edited, "pt");
            Assert.IsFalse(sheet.Takeover.IsOpen, "a later rebind reopens nothing");
            sheet.Hide();
        }

        [Test]
        public void ASamePointRebind_ClosesAGallerysLightbox_ButReopensABlocksFullView()
        {
            var wall = Shipped();
            var sheet = Sheet();
            ShowCard(sheet, wall);
            sheet.FullViewOpener.OpenTakeover("Gallery", 3, 1, (view, container, media, page) => { });
            Assert.IsTrue(sheet.Takeover.IsOpen, "precondition: the lightbox is open");
            Assert.IsFalse(sheet.FullView.IsOpen, "precondition: a lightbox is not a block's full view");

            ShowCard(sheet, wall, "pt");
            Assert.IsFalse(sheet.Takeover.IsOpen, "the lightbox closes over the rebind and is not reopened (its block draws its own pages again)");

            // - the same rebind with a block's full view open brings it back on that block
            sheet.FullViewOpener.OpenFullView(OpenBlock);
            ShowCard(sheet, wall, "en");
            Assert.IsTrue(sheet.FullView.IsOpen, "a block's full view is reopened");
            Assert.AreEqual(OpenBlock, sheet.FullView.BlockKey, "on the same block");
            sheet.Hide();
        }

        [Test]
        public void AClosedCard_OpensNothing_AndASectionOutsideTheMap_OpensNoPage()
        {
            var wall = Shipped();
            var sheet = Sheet();
            ShowCard(sheet, wall);
            int sections = sheet.Nav.Model.Sections.Count;
            Assert.Greater(sections, 1, "precondition: The Lamp's map has several sections");

            // - the bento door (CardLayoutView asks through SectionOpener): an index past either end opens nothing and answers false
            Assert.IsFalse(sheet.Layout.SectionOpener(sections, null), "one past the last section");
            Assert.IsFalse(sheet.Layout.SectionOpener(-1, null), "before the first");
            Assert.IsFalse(sheet.Takeover.IsOpen, "no page drawn for either");
            Assert.IsTrue(sheet.Layout.SectionOpener(0, null), "precondition: a real section does open");
            Assert.IsTrue(sheet.FullView.IsOpen);

            // - closed: neither a block nor a lightbox opens, and the section door answers false
            sheet.Hide();
            sheet.FullViewOpener.OpenFullView(OpenBlock);
            Assert.IsFalse(sheet.Takeover.IsOpen, "a closed card opens no block");
            sheet.FullViewOpener.OpenTakeover("Gallery", 3, 0, (view, container, media, page) => { });
            Assert.IsFalse(sheet.Takeover.IsOpen, "nor a lightbox");
            Assert.IsFalse(sheet.Layout.SectionOpener(0, null), "nor a section");
        }
    }
}

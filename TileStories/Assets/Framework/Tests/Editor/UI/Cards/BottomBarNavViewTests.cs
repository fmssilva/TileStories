using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories.Editor.Tests
{
    // The bottom_bar element (_3.2 step 7a, section 5.2) on a REAL sheet with The Lamp's real card (Grouped By Family: 8 sections) and the
    // shipped words: the bar shows Bar Slots - 1 sections and More, More's list holds EXACTLY the overflow in the model's order -- for 3, 4
    // and 5 slots -- and a card with fewer sections than slots gets one slot per section and no More. The bar sits in the sheet's bottom
    // area, under the sticky footer. What a real finger does with it is PoiCardBottomBarSceneTests'.
    public class BottomBarNavViewTests
    {
        private const string FrameworkStringsPath = "Assets/Framework/Runtime/UI/Cards/CardStrings.asset";

        private static WallConfigData Shipped() =>
            JsonUtility.FromJson<WallConfigData>(File.ReadAllText("Assets/Apps/LivingRoom/config.json"));

        // The shipped wall with the bar as its Bottom Bar, no Top Bar, the blocks Grouped By Family
        private static WallConfigData WithBar(int maxSlots)
        {
            var wall = Shipped();
            var nav = wall.card_settings.navigation;
            nav.bottom = BuiltInNavElements.BottomBar;
            nav.top = CardOptions.NavNone;
            nav.block_order = CardOptions.BlockOrderByFamily;
            nav.max_slots = maxSlots;
            return wall;
        }

        private static PoiCardSheetView Sheet() => new(new VisualElement(), BlockRegistry.Shared, Array.Empty<StyleSheet>());

        private static CardStrings Words(string language) =>
            new(AssetDatabase.LoadAssetAtPath<CardStringTable>(FrameworkStringsPath).Entries(), CardStringSources.Shared.Entries(), null, language, "en");

        private static void ShowCard(PoiCardSheetView sheet, WallConfigData wall, string poiId)
        {
            var poi = wall.pois.Single(p => p.id == poiId);
            var stack = BlockStackBuilder.Build(poi, wall.card_settings, BlockRegistry.Shared, wall.pois);
            var context = CardContextBuilder.Build(new BlockBindContext(), poi, wall, wall.card_settings, "en",
                AssetDatabase.LoadAssetAtPath<CardStringTable>(FrameworkStringsPath).Entries(), CardStringSources.Shared.Entries());
            sheet.Show(stack.Entries, context, SheetStopRule.Stop.Full, wall.card_settings.container.half_max_ratio);
        }

        [OneTimeSetUp]
        public void TheAppsKindIsRegistered()
        {
            Assert.IsTrue(BlockRegistry.Shared.TryGet("size_comparison", out _), "precondition: the LivingRoom app's kind is registered in Edit Mode");
            Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<CardStringTable>(FrameworkStringsPath), "precondition: the framework's words");
        }

        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        public void TheLamp_ShowsBarSlotsMinusOneSections_AndMoreListsExactlyTheOverflow_InOrder(int slots)
        {
            var sheet = Sheet();
            ShowCard(sheet, WithBar(slots), "lamp");
            var model = sheet.Nav.Model;
            var words = Words("en");
            Assert.AreEqual(8, model.Sections.Count, "precondition: The Lamp has more sections than any bar holds");
            Assert.AreEqual(BuiltInNavElements.BottomBar, sheet.Slots.BottomKey, "the wall's Bottom Bar is in the bottom slot");
            Assert.IsNull(sheet.Slots.TopView, "precondition: no Top Bar");
            var bar = (BottomBarNavView)sheet.Slots.BottomView;

            var inBar = model.Sections.Take(slots - 1).ToList();
            var overflow = model.Sections.Skip(slots - 1).ToList();
            CollectionAssert.AreEqual(inBar.Select(s => words.Get(s.LabelKey)), bar.Items.Select(t => t.Label.text), slots + " slots: the first sections");
            CollectionAssert.AreEqual(Enumerable.Range(0, slots - 1), bar.Items.Select(t => t.Index), "each slot is its section's index");
            Assert.IsTrue(bar.HasMore, "the rest do not fit: More");
            Assert.AreSame(bar.Bar, bar.More.Button.parent, "More is in the bar");
            Assert.AreEqual(bar.Bar.childCount - 1, bar.Bar.IndexOf(bar.More.Button), "as its LAST slot");
            Assert.AreEqual(slots, bar.Bar.childCount, "Bar Slots slots in all");
            Assert.AreEqual("More", bar.More.Label.text, "More's own word");
            Assert.AreEqual(CardIcons.More, CardIcons.KeyOf(bar.More.Icon), "and its three dots");

            CollectionAssert.AreEqual(overflow.Select(s => words.Get(s.LabelKey)), bar.MoreRows.Select(r => r.Label.text),
                "More lists EXACTLY the overflow sections, in the model's order");
            CollectionAssert.AreEqual(overflow.Select(s => s.IconKey), bar.MoreRows.Select(r => CardIcons.KeyOf(r.Icon)), "each with its family's icon");
            CollectionAssert.AreEqual(model.MoreSections.Select(s => s.Key), overflow.Select(s => s.Key), "the model's own overflow (NavOntologyRule)");
            Assert.AreEqual(overflow.Count, bar.MoreList.childCount, "nothing else in the list");

            Assert.IsFalse(bar.MoreOpen, "the list starts closed");
            Assert.AreEqual(DisplayStyle.None, bar.MoreList.style.display.value);
            bar.OpenMore();
            Assert.IsTrue(bar.MoreOpen, "a tap on More opens it");
            Assert.AreEqual(DisplayStyle.Flex, bar.MoreList.style.display.value);
            Assert.AreSame(bar.MoreList, bar.Root[0], "above the bar");
            bar.OpenMore();
            Assert.IsFalse(bar.MoreOpen, "a second tap closes it");
            bar.OpenMore();
            ShowCard(sheet, WithBar(slots), "lamp");
            Assert.IsFalse(((BottomBarNavView)sheet.Slots.BottomView).MoreOpen, "a rebind (a live edit, another point) closes it");
        }

        // A card whose sections all fit: one slot per section, no More and nothing to open
        [Test]
        public void ACardWithFewerSectionsThanSlots_ShowsOneSlotPerSection_AndNoMore()
        {
            var wall = WithBar(5);
            wall.card_settings.navigation.min_blocks = 2;
            var sheet = Sheet();
            ShowCard(sheet, wall, "lamp_military");
            var model = sheet.Nav.Model;
            Assert.That(model.Sections.Count, Is.InRange(2, 4), "precondition: Lamp - Military has fewer sections than 5 slots (and two to choose from)");
            var bar = (BottomBarNavView)sheet.Slots.BottomView;
            Assert.IsNotNull(bar, "the bar shows from Show Navigation From 2");
            var words = Words("en");
            CollectionAssert.AreEqual(model.Sections.Select(s => words.Get(s.LabelKey)), bar.Items.Select(t => t.Label.text), "every section its own slot");
            Assert.IsFalse(bar.HasMore, "no More");
            Assert.IsNull(bar.More.Button.parent, "More is not in the bar");
            Assert.AreEqual(0, bar.MoreRows.Count, "and lists nothing");
            bar.OpenMore();
            Assert.IsFalse(bar.MoreOpen, "nothing to open");
        }

        // Where the bar sits: the sheet's bottom area closes the sheet under the stack, the sticky footer first, the bottom slot under it
        [Test]
        public void TheBottomSlot_ClosesTheBottomArea_UnderTheStickyFooter_UnderTheStack()
        {
            var sheet = Sheet();
            ShowCard(sheet, WithBar(5), "lamp");
            Assert.AreSame(sheet.Root, sheet.BottomArea.parent, "the area is a part of the sheet");
            Assert.AreEqual(sheet.Root.IndexOf(sheet.Stack.Scroll) + 1, sheet.Root.IndexOf(sheet.BottomArea), "right under the stack");
            Assert.AreSame(sheet.Stack.Footer, sheet.BottomArea[0], "the footer first: the CTA stays nearest the content");
            Assert.AreSame(sheet.Slots.Bottom, sheet.BottomArea[1], "the bottom slot under it: the thumb zone");
            Assert.Greater(sheet.Stack.Footer.childCount, 0, "precondition: The Lamp has its sticky footer");
            Assert.AreSame(sheet.Slots.Bottom, sheet.Slots.BottomView.Root.parent, "the bar sits in the bottom slot");
            Assert.AreEqual(DisplayStyle.Flex, sheet.Slots.Bottom.style.display.value, "a held element is in the sheet");

            sheet.Hide();
            Assert.IsNull(sheet.Slots.BottomView, "closing the card empties the slot");
            Assert.AreEqual(DisplayStyle.None, sheet.Slots.Bottom.style.display.value);
        }
    }
}

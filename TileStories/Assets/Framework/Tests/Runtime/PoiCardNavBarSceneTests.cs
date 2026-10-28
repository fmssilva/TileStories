using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // The Navigation Bar on the real scene (_3.3 S4a review, NavBarView): the Ribeira Palace's card with the bar on -- its groups (one per
    // family the card has blocks of), floating under the title bar with no jump of the stack when it comes, after the hero; a tap lands the
    // group's first block under the floating chrome and marks the group (the indicator under it); each indicator, the full-width shape and
    // the bottom position; a merged group; the bar off again -> the old elements. Renders in TestEvidence/NavBar/.
    [Category("Core")]
    public class PoiCardNavBarSceneTests : SearchSceneFixture
    {
        private PoiCardSheetView Sheet => Card.Sheet;
        private BlockStackView Stack => Sheet.Stack;
        private ScrollView Scroll => Stack.Scroll;
        private NavBarView Bar => Sheet.Slots.Bar;
        private float OnePixel => Sheet.Layer.panel.visualTree.layout.height / Screen.height;
        private const string Domain = "NavBar";
        private const string Painting = "painting";

        private IEnumerator Push(System.Action<WallConfigData> edit)
        {
            var copy = ConfigCopy();
            edit(copy);
            Session.ApplyCardSettings(copy);
            yield return CardTestInput.Settle(0.3f);
        }

        private IEnumerator OpenWithTheBar(System.Action<CardNavBarSettings> look = null)
        {
            CardTestInput.HoldTheWallStill(Sheet);
            yield return Push(c =>
            {
                c.card_settings.navigation.bar.enabled = true;
                look?.Invoke(c.card_settings.navigation.bar);
            });
            SelectionEventBus.Select(Painting);
            yield return CardTestInput.Settle();
            Assert.IsTrue(Sheet.IsOpen, "precondition: the Ribeira Palace's card is open");
            Sheet.SetStop(SheetStopRule.Stop.Full);
            yield return CardTestInput.Settle(0.6f);
            // - the hero's pan and zoom stand at time 0, so a render of the card holds still
            Stack.HeaderView.Clock = () => 0f;
            Stack.HeaderView.Animate();
            yield return null;
        }

        // Scroll the stack to `y` and let the chrome decide
        private IEnumerator ScrollTo(float y)
        {
            Scroll.scrollOffset = new Vector2(0f, y);
            yield return null;
            yield return null;
            yield return CardTestInput.Settle(0.4f);
        }

        private bool TopShown => !Sheet.Slots.Top.ClassListContains(CardChromeView.TopHiddenClass) && Sheet.Slots.Top.resolvedStyle.display == DisplayStyle.Flex;

        [UnityTest]
        public IEnumerator TheBar_HoldsTheCardsGroups_FloatsUnderTheTitleBar_AndComesAfterTheHero_WithNoJump()
        {
            yield return OpenWithTheBar();
            Assert.AreEqual(NavBarView.ElementKey, Sheet.Slots.TopKey, "the bar is the top slot's element");
            Assert.AreSame(Sheet.Slots.Top, Bar.Root.parent, "...in the top host, under the title bar");
            Assert.GreaterOrEqual(Bar.Items.Count, NavBarRule.MinEntries, "the card's groups");
            Assert.AreEqual(Bar.Entries.Count, Bar.Items.Count);
            var families = Sheet.Nav.Model.Targets.Select(t => { Sheet.Nav.TryGetEntry(t, out var e); return e.Definition.Family; }).Distinct().ToList();
            foreach (var entry in Bar.Entries) CollectionAssert.Contains(families, entry.Item.Key, "a group of a family the card has blocks of");
            Assert.IsNull(Sheet.Slots.MenuKey, "no menu while the bar is on");
            Assert.IsNull(Sheet.Slots.FloatingKey, "...no floating button");
            Assert.IsNull(Sheet.Slots.EdgeKey, "...no edge");

            Assert.IsFalse(TopShown, "at rest the hero is the screen: the bar waits (After The Hero)");
            float viewport = Scroll.layout.height;
            yield return LookMeasure.Render(Domain, "bar", "rest_full", Sheet.Root);
            yield return ScrollTo(Stack.HeaderView.HeroHeight * 0.5f);
            Assert.IsTrue(TopShown, "the visitor scrolled the picture away: the bar comes");
            Assert.AreEqual(viewport, Scroll.layout.height, OnePixel, "it floats: the stack keeps its size, nothing jumps");
            var bar = Bar.Root.worldBound;
            var title = Stack.HeaderSlot.worldBound;
            Assert.GreaterOrEqual(bar.yMin, title.yMax - OnePixel, "it sits under the title bar");
            float margin = NavBarRule.SideMargin(Bar.Settings);
            Assert.AreEqual(Sheet.Root.worldBound.xMin + margin, bar.xMin, OnePixel, "floating: inset by its side margin");
            yield return LookMeasure.Render(Domain, "bar", "scrolled_pill", Sheet.Root);
        }

        [UnityTest]
        public IEnumerator ATap_LandsTheGroupsFirstBlockUnderTheChrome_AndMarksIt_TheIndicatorUnderIt()
        {
            yield return OpenWithTheBar();
            yield return ScrollTo(Stack.HeaderView.HeroHeight);
            int last = Bar.Items.Count - 1;
            Bar.Tap(last);
            yield return CardTestInput.Settle(1.2f);
            Assert.AreEqual(last, Bar.ChosenIndex, "the group tapped is the chosen one");
            Assert.IsTrue(Bar.Items[last].Button.ClassListContains(NavBarView.ChosenClass));
            Assert.AreEqual(1, Bar.Items.Count(i => i.Button.ClassListContains(NavBarView.ChosenClass)), "...and no other");
            string first = Bar.Entries[last].FirstTarget;
            var slot = Stack.SlotOfBlock(first);
            Assert.AreEqual(Stack.LandingTop(slot), Scroll.scrollOffset.y, 2f, "the jump rests on the group's first block");
            float under = Sheet.Slots.Top.worldBound.yMax;
            Assert.GreaterOrEqual(slot.worldBound.yMin + 2f, Stack.Scroll.worldBound.yMin + Stack.LandingInset - OnePixel,
                "...under the floating bars and the strip, in sight");
            Assert.AreEqual(Bar.Items[last].Button.worldBound.center.x, Bar.Indicator.worldBound.center.x, 1f, "the indicator under the chosen group");
            var label = Bar.Items[last].Label;
            Assert.AreEqual(FontStyle.Bold, label.resolvedStyle.unityFontStyleAndWeight, "the chosen words bolder (never colour alone)");
            yield return LookMeasure.Render(Domain, "bar", "tapped_last", Sheet.Root);
        }

        [UnityTest]
        public IEnumerator EachIndicator_AndTheFullWidthShape_AndTheBottomPosition_DrawAsSet(
            [Values(CardOptions.NavBarIndicatorPill, CardOptions.NavBarIndicatorUnderline, CardOptions.NavBarIndicatorRaised, CardOptions.NavBarIndicatorNone)] string indicator)
        {
            yield return OpenWithTheBar(b =>
            {
                b.indicator = indicator;
                b.show = CardOptions.NavBarShowAlways;
            });
            Assert.IsTrue(TopShown, "Show = Always: the bar is there at rest, over the hero");
            Bar.Tap(1);
            yield return CardTestInput.Settle(1.2f);
            Assert.IsTrue(Bar.Root.ClassListContains("nav-bar--indicator-" + indicator));
            bool drawn = indicator != CardOptions.NavBarIndicatorNone;
            Assert.AreEqual(drawn ? DisplayStyle.Flex : DisplayStyle.None, Bar.Indicator.resolvedStyle.display, "the indicator drawn unless None");
            if (indicator == CardOptions.NavBarIndicatorRaised)
                Assert.Less(Bar.Indicator.worldBound.yMin, Bar.Root.worldBound.yMin - 4f, "raised: the disc rises past the bar's top edge");
            if (indicator == CardOptions.NavBarIndicatorUnderline)
                Assert.LessOrEqual(Bar.Indicator.worldBound.height, 4f, "underline: a short line");
            yield return LookMeasure.Render(Domain, "indicators", indicator, Sheet.Root);

            yield return Push(c =>
            {
                var b = c.card_settings.navigation.bar;
                b.enabled = true;
                b.indicator = indicator;
                b.show = CardOptions.NavBarShowAlways;
                b.shape = CardOptions.NavBarFullWidth;
            });
            yield return CardTestInput.Settle(0.4f);
            Assert.AreEqual(Sheet.Root.worldBound.width, Bar.Root.worldBound.width, OnePixel, "full width: edge to edge");
            Assert.AreEqual(0f, Bar.Ground.resolvedStyle.borderTopLeftRadius, 0.01f, "...square");
            yield return LookMeasure.Render(Domain, "indicators", indicator + "_full_width", Sheet.Root);

            yield return Push(c =>
            {
                var b = c.card_settings.navigation.bar;
                b.enabled = true;
                b.indicator = indicator;
                b.show = CardOptions.NavBarShowAlways;
                b.position = CardOptions.NavBarBottom;
            });
            yield return CardTestInput.Settle(0.4f);
            Assert.AreEqual(NavBarView.ElementKey, Sheet.Slots.BottomKey, "the bar in the bottom host");
            Assert.IsNull(Sheet.Slots.TopKey, "...and none at the top");
            Assert.Greater(Bar.Root.worldBound.center.y, Sheet.Root.worldBound.center.y, "at the bottom of the card");
            yield return LookMeasure.Render(Domain, "indicators", indicator + "_bottom", Sheet.Root);
        }

        [UnityTest]
        public IEnumerator AMergedGroup_LeadsToTheFirstBlockOfAnyOfItsFamilies_AndTheBarOffBringsTheOldElementsBack()
        {
            yield return OpenWithTheBar(b =>
            {
                b.show = CardOptions.NavBarShowAlways;
                b.items = new List<CardNavBarItem>
                {
                    new() { key = "group_1", families = new List<string> { BuiltInFamilies.Media, BuiltInFamilies.Guides },
                        label = new List<LocalizedEntry> { new() { lang = "en", value = "Watch" }, new() { lang = "pt", value = "Ver" } } },
                    new() { key = BuiltInFamilies.About, families = new List<string> { BuiltInFamilies.About } },
                };
            });
            Assert.AreEqual(2, Bar.Items.Count, "the two groups the wall made");
            Assert.That(Bar.Items[0].Label.text, Is.EqualTo("Watch").Or.EqualTo("Ver"), "the group's own words");
            string first = Sheet.Nav.Model.Targets.First(t =>
            {
                Sheet.Nav.TryGetEntry(t, out var e);
                return e.Definition.Family == BuiltInFamilies.Media || e.Definition.Family == BuiltInFamilies.Guides;
            });
            Assert.AreEqual(first, Bar.Entries[0].FirstTarget, "the merged group lands on the first block of either family");
            yield return LookMeasure.Render(Domain, "bar", "merged", Sheet.Root);

            yield return Push(c => c.card_settings.navigation.bar.enabled = false);
            yield return CardTestInput.Settle(0.4f);
            Assert.AreNotEqual(NavBarView.ElementKey, Sheet.Slots.TopKey, "off: the bar is gone");
            Assert.IsNull(Bar.Root.parent, "...out of the card");
        }
    }
}

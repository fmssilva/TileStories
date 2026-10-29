using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // The Locate Button ("Show me where it is", _3.3 P2 review e, P3d; LocateFabView + LocateFabRule) on the REAL scene with real input:
    //   - the default: a 56 pt disc in the accent at the bottom right, 12 above the bar's highest point (the raised disc's top), over the
    //     hero at Half and Full and after it; a whole touch target; named by the Show On Wall words
    //   - its rect never meets the bar's items or the disc whichever group is chosen (Bottom and Top), nor the hero's words (Half, Full)
    //   - a REAL tap at Half and at Full lowers the card to Peek with the point still selected: its marker on the wall
    //   - it follows the bar: at Top it rests over the sheet's foot; Hide While Scrolling slides it away and back with REAL strokes
    //   - the locate mark >= 4.5:1 on its disc on the render, over the hero and after it, light and dark (TestEvidence/Premium/P3d/)
    //   - the old sticky action (The Lamp's): off under the bottom bar by default, back with Sticky Action Shown or the bar at Top
    public class PoiCardLocateFabSceneTests : SearchSceneFixture
    {
        private const string Domain = "Premium/P3d";
        private const string Painting = "painting";
        private const string Lamp = "lamp";

        private PoiCardSheetView Sheet => Card.Sheet;
        private BlockStackView Stack => Sheet.Stack;
        private ScrollView Scroll => Stack.Scroll;
        private NavBarView Bar => Sheet.Slots.Bar;
        private LocateFabView Fab => Sheet.Locate;
        private float OnePixel => Sheet.Layer.panel.visualTree.layout.height / Screen.height;

        [UnityTearDown]
        public IEnumerator BackToLight()
        {
            ThemeController.SetMode(CardOptions.ModeLight);
            yield return null;
        }

        // `poi`'s card at `stop`, scrolled to its top, the bar as `look` sets it on the framework's default
        private IEnumerator Open(System.Action<CardNavBarSettings> look = null, SheetStopRule.Stop stop = SheetStopRule.Stop.Full, string poi = Painting)
        {
            CardTestInput.HoldTheWallStill(Sheet);
            var copy = ConfigCopy();
            copy.card_settings.navigation.bar = new CardNavBarSettings();
            look?.Invoke(copy.card_settings.navigation.bar);
            Session.ApplyCardSettings(copy);
            yield return CardTestInput.Settle(0.3f);
            if (!Sheet.IsOpen || Card.ShownPoiId != poi) SelectionEventBus.Select(poi);
            yield return CardTestInput.Settle();
            Assert.IsTrue(Sheet.IsOpen && Card.ShownPoiId == poi, "precondition: the card of " + poi + " is open");
            Sheet.SetStop(stop);
            yield return CardTestInput.Settle(0.6f);
            Scroll.scrollOffset = Vector2.zero;
            Stack.HeaderView.Clock = () => 0f;
            Stack.HeaderView.Animate();
            yield return CardTestInput.UntilStill(Sheet);
            yield return CardTestInput.Settle(0.3f);
        }

        private static bool Meets(Rect a, Rect b) => a.xMin < b.xMax && b.xMin < a.xMax && a.yMin < b.yMax && b.yMin < a.yMax;

        // The bar's highest point as drawn: its band, or the chosen disc's top when that rises past the band
        private float BarTop => Bar.Disc.resolvedStyle.display == DisplayStyle.Flex ? Mathf.Min(Bar.Row.worldBound.yMin, Bar.Disc.worldBound.yMin) : Bar.Row.worldBound.yMin;

        // Every shown word and action of the hero's foot (the boxes as laid out: a label's box is its column's width)
        private List<VisualElement> HeroWords()
        {
            var foot = Stack.HeaderView.Foot;
            var words = foot.Query<Label>().ToList().Where(l => l.resolvedStyle.display == DisplayStyle.Flex && !string.IsNullOrWhiteSpace(l.text)
                && l.worldBound.width > 0f).Cast<VisualElement>().ToList();
            words.AddRange(Stack.HeaderView.Actions.Children().Where(c => c.resolvedStyle.display == DisplayStyle.Flex));
            return words;
        }

        [UnityTest]
        public IEnumerator TheDefault_AnAccentDiscAtTheBottomRight_12AboveTheBar_NeverMeetingItsItemsOrTheHerosWords([Values("half", "full")] string at)
        {
            var stop = at == "half" ? SheetStopRule.Stop.Half : SheetStopRule.Stop.Full;
            yield return Open(stop: stop);
            Assert.AreEqual(NavBarView.ElementKey, Sheet.Slots.BottomKey, "precondition: the default bar at the bottom");
            Assert.IsTrue(Fab.IsShown, "shown at " + at);
            var fab = Fab.Root.worldBound;
            float size = CardTestInput.TokenPx("--ts-fab-size");
            Assert.AreEqual(56f, size, "precondition: the card's 56");
            Assert.AreEqual(size, fab.width, 0.5f, "a 56 pt disc");
            Assert.AreEqual(size, fab.height, 0.5f);
            Assert.GreaterOrEqual(fab.width, 44f, "a whole touch target");
            Assert.AreEqual(size * 0.5f, Fab.Root.resolvedStyle.borderTopLeftRadius, 0.5f, "round");
            AreSame(Theme("--ts-accent"), Fab.Root.resolvedStyle.backgroundColor, "the accent");
            Assert.AreEqual("Show me where it is", Fab.Root.tooltip, "named by the Show On Wall words");
            var sheet = Sheet.Root.worldBound;
            Assert.AreEqual(CardTestInput.TokenPx("--ts-navbar-inset"), sheet.xMax - fab.xMax, OnePixel, "at the right, the bar's own inset from the side");
            Assert.AreEqual(BarTop - 12f, fab.yMax, OnePixel, "12 above the bar's highest point (" + BarTop + ")");
            // - whichever group is chosen: a real tap on each, the disc slid there, never under the button
            for (int i = 0; i < Bar.Items.Count; i++)
            {
                if (i != Bar.ChosenIndex)
                {
                    yield return CardTestInput.TouchDrag(Sheet.Root.panel, Bar.Items[i].Button.worldBound.center, Vector2.zero, frames: 1);
                    yield return CardTestInput.Settle(0.8f);
                    for (int f = 0; f < 120 && Bar.Sliding; f++) yield return null;
                }
                fab = Fab.Root.worldBound;
                foreach (var item in Bar.Items)
                    Assert.IsFalse(Meets(fab, item.Button.worldBound), "group " + i + " chosen: the button meets '" + item.Label.text + "' " + item.Button.worldBound + " / " + fab);
                Assert.IsFalse(Meets(fab, Bar.Disc.worldBound), "group " + i + " chosen: ...or the disc " + Bar.Disc.worldBound);
            }
            // - the hero's words at rest at this stop (a jump above moved the card: open it again here)
            yield return Open(stop: stop);
            fab = Fab.Root.worldBound;
            var words = HeroWords();
            Assert.Greater(words.Count, 2, "precondition: the hero's words are drawn");
            foreach (var w in words)
                Assert.IsFalse(Meets(fab, w.worldBound), at + ": the button meets the hero's '" + (w as Label)?.text + "' " + w.worldBound + " / " + fab);
            // - each row of the words (the text, the actions) that reaches down beside the button ends 12 before it (its box less its own
            //   gutter: no word stands in a padding); a row wholly above it keeps the hero's whole width (a title narrowed for nothing wraps)
            int beside = 0;
            foreach (var row in new[] { Stack.HeaderView.Text, Stack.HeaderView.Actions })
            {
                if (row.resolvedStyle.display == DisplayStyle.None) continue;
                float end = row.worldBound.xMax - row.resolvedStyle.paddingRight;
                if (row.worldBound.yMax > fab.yMin + OnePixel)
                {
                    beside++;
                    Assert.LessOrEqual(end, fab.xMin - 12f + OnePixel, row.name + " beside the button: its column ends 12 before it");
                }
                else Assert.AreEqual(sheet.xMax - CardTestInput.TokenPx("--ts-space-5"), end, OnePixel, row.name + " above the button: the whole width");
            }
            Assert.Greater(beside, 0, "precondition: a row of the words lies beside the button at " + at);
            yield return Save("default_" + at);
        }

        // A REAL tap shows the point on the wall: the card lowers to Peek, the point still selected (its marker stays), at Half and at Full;
        // at Peek the button is gone (the wall is in view)
        [UnityTest]
        public IEnumerator ARealTap_ShowsThePointOnTheWall_AtHalfAndFull([Values("half", "full")] string at)
        {
            yield return Open(stop: at == "half" ? SheetStopRule.Stop.Half : SheetStopRule.Stop.Full);
            Assert.IsTrue(Fab.IsShown, "precondition: the button");
            yield return CardTestInput.TouchDrag(Sheet.Root.panel, Fab.Root.worldBound.center, Vector2.zero, frames: 1);
            yield return CardTestInput.Settle(0.8f);
            Assert.AreEqual(SheetStopRule.Stop.Peek, Sheet.Stop, "the card lowered to Peek: the wall in view");
            Assert.IsTrue(Sheet.IsOpen, "...the card still open");
            Assert.AreEqual(Painting, SelectionEventBus.CurrentPoiId, "...the point still selected: its marker on the wall");
            Assert.IsFalse(Fab.IsShown, "at Peek: no button");
        }

        // It follows the bar: at Top it rests 12 over the sheet's foot (no bar under it), meeting neither the bar nor the hero's words
        [UnityTest]
        public IEnumerator WithTheBarAtTop_ItRests12OverTheFoot_ClearOfTheBarAndTheWords()
        {
            yield return Open(b => b.position = CardOptions.NavBarTop, SheetStopRule.Stop.Half);
            Assert.AreEqual(NavBarView.ElementKey, Sheet.Slots.TopKey, "precondition: the bar at Top");
            Assert.IsTrue(Fab.IsShown, "shown");
            var fab = Fab.Root.worldBound;
            Assert.AreEqual(Sheet.Root.worldBound.yMax - 12f, fab.yMax, OnePixel, "12 over the sheet's foot");
            foreach (var item in Bar.Items) Assert.IsFalse(Meets(fab, item.Button.worldBound), "never on the bar's items");
            foreach (var w in HeroWords()) Assert.IsFalse(Meets(fab, w.worldBound), "never on the hero's words");
            yield return Save("top_half");
        }

        // Hide While Scrolling: REAL strokes down the card slide the bar away and the button with it (no finger on it then); a stroke back up
        // brings both again
        [UnityTest]
        public IEnumerator HideWhileScrolling_RealStrokesDown_SlideItAwayWithTheBar_AndUpBringItBack()
        {
            yield return Open(b => b.hide_on_scroll = CardOptions.NavBarHideDown);
            Assert.IsTrue(Fab.IsShown, "precondition: shown at the card's top");
            for (int s = 0; s < 3; s++)
            {
                var reach = CardTestInput.ReachableStack(Sheet);
                yield return CardTestInput.TouchDrag(Scroll.panel, new Vector2(reach.center.x, reach.center.y), new Vector2(0f, -120f));
                yield return CardTestInput.UntilStill(Sheet);
            }
            Assert.IsTrue(Sheet.BottomArea.ClassListContains(CardChromeView.BottomHiddenClass), "precondition: reading down slid the bar away");
            Assert.IsFalse(Fab.IsShown, "...and the button with it");
            Assert.IsTrue(Fab.Root.ClassListContains(LocateFabView.HiddenClass), "...slid, not dropped");
            Assert.AreEqual(PickingMode.Ignore, Fab.Root.pickingMode, "...taking no finger while away");
            var back = CardTestInput.ReachableStack(Sheet);
            yield return CardTestInput.TouchDrag(Scroll.panel, new Vector2(back.center.x, back.center.y), new Vector2(0f, 40f));
            yield return CardTestInput.UntilStill(Sheet);
            yield return CardTestInput.Settle(0.4f);
            Assert.IsFalse(Sheet.BottomArea.ClassListContains(CardChromeView.BottomHiddenClass), "precondition: a move up brought the bar back");
            Assert.IsTrue(Fab.IsShown, "...and the button");
            Assert.AreEqual(BarTop - 12f, Fab.Root.worldBound.yMax, OnePixel, "...at its place over the bar");
        }

        // The locate mark on its disc, measured on the render (the mark drawn, then cleared: the ink against the disc round its strokes),
        // over the hero and after it, light and dark: >= 4.5:1
        [UnityTest]
        public IEnumerator TheLocateMark_Reads4Point5To1_OnTheRender_OverTheHeroAndAfterIt([Values("light", "dark")] string mode)
        {
            yield return Open();
            ThemeController.SetMode(mode);
            yield return CardTestInput.Settle(0.3f);
            Assert.AreEqual(mode, ThemeController.CurrentMode, "precondition: the card shows " + mode);
            var report = new List<string>();
            foreach (bool overHero in new[] { true, false })
            {
                if (!overHero)
                {
                    Scroll.scrollOffset = new Vector2(0f, Stack.HeaderView.HeroBottom() + 240f);
                    yield return CardTestInput.Settle(0.4f);
                    yield return CardTestInput.UntilStill(Sheet);
                }
                CardTestInput.HoldSectionsStill(Sheet);
                Assert.IsTrue(Fab.IsShown, "precondition: shown " + (overHero ? "over the hero" : "after it"));
                Assert.IsTrue(CardTokenColours.TryResolve(Fab.Root, "--ts-on-accent", out Color ink), "precondition: the mark's ink");
                string place = overHero ? "OverTheHero" : "AfterTheHero";
                Texture2D drawn = null, under = null;
                try
                {
                    yield return ThemePixelSet.GrabStill($"fab_{mode}_{place}", null, r => drawn = r);
                    Fab.Mark.style.visibility = Visibility.Hidden;
                    yield return null;
                    yield return ThemePixelSet.GrabStill($"fab_under_{mode}_{place}", null, r => under = r);
                    var (ratio, worst, inked) = ThemePixelSet.RingContrast(drawn, under, ThemePixelSet.ScreenRectOf(Fab.Mark, drawn), ink);
                    Assert.Greater(inked, 0, "precondition: the mark draws pixels");
                    report.Add($"{mode} {place}: {ratio:0.00}:1 (worst {(Color32)worst})");
                    Assert.GreaterOrEqual(ratio, 4.5f, $"the locate mark on its disc, {mode} {place}: {ratio:0.00}:1");
                    System.IO.File.WriteAllBytes(TestEvidence.PathFor(Domain, $"fab_{mode}_{place}.png"), drawn.EncodeToPNG());
                }
                finally
                {
                    Fab.Mark.style.visibility = StyleKeyword.Null;
                    if (drawn != null) Object.Destroy(drawn);
                    if (under != null) Object.Destroy(under);
                }
            }
            System.IO.File.WriteAllText(TestEvidence.PathFor(Domain, $"fab_ratios_{mode}.txt"), string.Join("\n", report) + "\n");
        }

        // The old sticky action (The Lamp's Show On Wall in the Sticky look): off by default under the bottom bar -- no footer, no pill on the
        // hero -- and kept as options: Sticky Action Shown brings both back under the bottom bar; with the bar at Top, Automatic draws them
        [UnityTest]
        public IEnumerator TheOldStickyAction_IsOffUnderTheBottomBar_ByDefault_AndComesBackAsAnOption()
        {
            int HeroPrimaries() => Stack.HeaderView.Actions.Children().Count(c => c.ClassListContains(BlockStackView.HeroPrimaryClass));

            yield return Open(poi: Lamp);
            Assert.AreEqual(0, Stack.Footer.childCount, "the default: no sticky footer");
            Assert.AreEqual(0, HeroPrimaries(), "...and no pill on the hero");
            Assert.IsTrue(Fab.IsShown, "the Locate Button stands for it");

            yield return Open(b => b.sticky_action = CardOptions.NavBarStickyShown, poi: Lamp);
            Assert.AreEqual(1, Stack.Footer.childCount, "Sticky Action Shown: the footer back");
            Assert.AreEqual(1, HeroPrimaries(), "...and the hero's pill");

            yield return Open(b => b.position = CardOptions.NavBarTop, poi: Lamp);
            Assert.AreEqual(1, Stack.Footer.childCount, "the bar at Top, Automatic: the footer drawn");
            Assert.AreEqual(1, HeroPrimaries(), "...and the hero's pill");

            yield return Open(b => b.sticky_action = CardOptions.NavBarStickyHidden, poi: Lamp);
            Assert.AreEqual(0, Stack.Footer.childCount, "Hidden: none");
        }

        private Color Theme(string token)
        {
            Assert.IsTrue(CardTokenColours.TryResolve(Fab.Root, token, out Color c), "precondition: the theme has " + token);
            return c;
        }

        private static void AreSame(Color expected, Color actual, string what) =>
            Assert.Less(((Vector4)expected - (Vector4)actual).magnitude, 0.01f, what + ": expected " + (Color32)expected + ", drawn " + (Color32)actual);

        private IEnumerator Save(string name)
        {
            CardTestInput.HoldSectionsStill(Sheet);
            Texture2D shot = null;
            yield return ThemePixelSet.GrabStill("fab_" + name, null, r => shot = r);
            if (shot == null) yield break;
            System.IO.File.WriteAllBytes(TestEvidence.PathFor(Domain, "fab_" + name + ".png"), shot.EncodeToPNG());
            Object.Destroy(shot);
        }
    }
}

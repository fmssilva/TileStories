using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // The Navigation Bar on the REAL scene (_3.3 S4 third round, v5; NavBarView) with the Ribeira Palace's card and real input:
    //   - the framework's default bar: floating at the bottom, Solid with its shadow, Raised (the edge bent into a bump under the chosen
    //     group, its icon riding up in a filled accent disc), over the hero from the start -- the hero's words rise clear of it
    //   - a real tap on a group jumps there; the bump and the disc slide to it with an overshoot, and follow real drags of the stack
    //   - every option value changes what the visitor sees (one case per value), and the pairs that interact: Raised x Top (the bump hangs
    //     down), Fill x over the hero (a non-solid bar turns white on dark glass, Clear transparent there and solid after it)
    //   - the labels read at 7:1 on the real render on every fill, over the hero and after it, light and dark, Bottom and Top
    //   - an old config's "under_title" reads as Top on the running card; a merged group; the bar off
    // Renders in TestEvidence/Premium/P2/.
    [Category("Core")]
    public class PoiCardNavBarSceneTests : SearchSceneFixture
    {
        private const string Domain = "Premium/P2";
        private const string Painting = "painting";
        private const float Aaa = 7f;

        private PoiCardSheetView Sheet => Card.Sheet;
        private BlockStackView Stack => Sheet.Stack;
        private ScrollView Scroll => Stack.Scroll;
        private NavBarView Bar => Sheet.Slots.Bar;
        private float OnePixel => Sheet.Layer.panel.visualTree.layout.height / Screen.height;

        [UnityTearDown]
        public IEnumerator BackToLight()
        {
            ThemeController.SetMode(CardOptions.ModeLight);
            yield return null;
        }

        private IEnumerator Push(System.Action<CardNavBarSettings> look)
        {
            var copy = ConfigCopy();
            copy.card_settings.navigation.bar = new CardNavBarSettings();
            look?.Invoke(copy.card_settings.navigation.bar);
            Session.ApplyCardSettings(copy);
            yield return CardTestInput.Settle(0.3f);
        }

        // The Ribeira Palace's card at Full, scrolled to the top (the hero), the bar as `look` sets it on the framework's default
        private IEnumerator Open(System.Action<CardNavBarSettings> look = null)
        {
            CardTestInput.HoldTheWallStill(Sheet);
            yield return Push(look);
            // - a card already open on the point is rebound by the push (selecting it again would close it)
            if (!Sheet.IsOpen || Card.ShownPoiId != Painting) SelectionEventBus.Select(Painting);
            yield return CardTestInput.Settle();
            Scroll.scrollOffset = Vector2.zero;
            Assert.IsTrue(Sheet.IsOpen, "precondition: the Ribeira Palace's card is open");
            Sheet.SetStop(SheetStopRule.Stop.Full);
            yield return CardTestInput.Settle(0.6f);
            // - the hero's pan and zoom stand at time 0, so a render of the card holds still
            Stack.HeaderView.Clock = () => 0f;
            Stack.HeaderView.Animate();
            yield return CardTestInput.UntilStill(Sheet);
            Assert.IsTrue(Sheet.Slots.TopKey == NavBarView.ElementKey || Sheet.Slots.BottomKey == NavBarView.ElementKey, "precondition: the bar is on the card");
            yield return UntilSettled();
        }

        // Scroll the stack to `y` and let the chrome decide
        private IEnumerator ScrollTo(float y)
        {
            Scroll.scrollOffset = new Vector2(0f, y);
            yield return null;
            yield return CardTestInput.Settle(0.4f);
            yield return UntilSettled();
        }

        // Past the hero: the bar no longer lies on its picture
        private IEnumerator PastTheHero() => ScrollTo(Stack.HeaderView.HeroBottom() + 240f);

        // The indicators at rest (the slide over)
        private IEnumerator UntilSettled()
        {
            for (int i = 0; i < 120 && Bar.Sliding; i++) yield return null;
            Assert.IsFalse(Bar.Sliding, "precondition: the indicator came to rest");
        }

        private bool BottomShown => !Sheet.BottomArea.ClassListContains(CardChromeView.BottomHiddenClass);
        private Rect Band => Bar.Row.worldBound;
        private float Highest => Bar.PaintedPath.SelectMany(PointsOf).Min(p => p.y);
        private float Lowest => Bar.PaintedPath.SelectMany(PointsOf).Max(p => p.y);

        private static IEnumerable<Vector2> PointsOf(NavBarShapeRule.Command c)
        {
            if (c.Op == NavBarShapeRule.Op.Close) yield break;
            yield return c.P1;
            if (c.Op == NavBarShapeRule.Op.Quad || c.Op == NavBarShapeRule.Op.Cubic) yield return c.P2;
            if (c.Op == NavBarShapeRule.Op.Cubic) yield return c.P3;
        }

        private static Color Theme(VisualElement from, string token)
        {
            Assert.IsTrue(CardTokenColours.TryResolve(from, token, out Color c), "precondition: the theme has " + token);
            return c;
        }

        private static void AreSame(Color expected, Color actual, string what) =>
            Assert.Less(((Vector4)expected - (Vector4)actual).magnitude, 0.01f, what + ": expected " + (Color32)expected + ", drawn " + (Color32)actual);

        [UnityTest]
        public IEnumerator TheDefaultBar_FloatsAtTheBottom_SolidRaisedFilled_OverTheHero_TheHerosWordsClearOfIt()
        {
            yield return Open();
            Assert.AreEqual(NavBarView.ElementKey, Sheet.Slots.BottomKey, "the default bar is the bottom slot's element");
            Assert.IsNull(Sheet.Slots.TopKey, "...no top element (the old Top Bar defaults to none)");
            Assert.IsNull(Sheet.Slots.MenuKey);
            Assert.IsNull(Sheet.Slots.FloatingKey);
            Assert.IsNull(Sheet.Slots.EdgeKey);
            Assert.GreaterOrEqual(Bar.Items.Count, NavBarRule.MinEntries, "the card's groups");
            Assert.IsTrue(BottomShown, "Show Always: there at rest");
            Assert.IsTrue(Bar.OverHero, "at rest it lies on the hero's picture");
            Assert.IsFalse(Bar.OverImage, "Solid stays solid over the hero");
            // - it floats: inset by its side gap, over the stack's foot (the stack runs under it)
            var sheet = Sheet.Root.worldBound;
            Assert.Greater(Band.xMin, sheet.xMin + 4f, "floating: inset from the side");
            Assert.AreEqual(sheet.xMax - Band.xMax, Band.xMin - sheet.xMin, OnePixel, "...the same on both sides");
            Assert.Less(Band.yMax, sheet.yMax - 4f, "...a gap above the screen's foot");
            Assert.Greater(Scroll.worldBound.yMax, Band.yMin, "the stack runs under the bar (it floats, out of the flow)");
            // - Solid: the shadow, the opaque bar colour
            Assert.IsTrue(Bar.PaintedShadow, "Solid + Floating: the shadow");
            AreSame(Theme(Bar.Root, "--ts-bar"), Bar.PaintedFill, "the fill");
            // - Raised + Filled: the bump under the chosen group and the disc riding up in it
            Assert.GreaterOrEqual(Bar.ChosenIndex, 0, "a group is chosen");
            Assert.Less(Highest, Bar.Room - 1f, "the edge bends up into a bump");
            Assert.AreEqual(DisplayStyle.Flex, Bar.Disc.resolvedStyle.display, "the disc");
            Assert.Less(Bar.Disc.worldBound.yMin, Band.yMin - 4f, "...rides up past the bar's edge");
            Assert.AreEqual(Bar.Items[Bar.ChosenIndex].Button.worldBound.center.x, Bar.Disc.worldBound.center.x, 1f, "...over the chosen group");
            AreSame(Theme(Bar.Root, "--ts-accent"), Bar.Disc.resolvedStyle.backgroundColor, "the disc in the accent");
            Assert.AreEqual(0f, Bar.Items[Bar.ChosenIndex].Icon.resolvedStyle.opacity, 0.01f, "the chosen icon rides in the disc, not twice");
            // - the hero's words (and its actions) finish above the bar and its disc
            var header = Stack.HeaderView;
            Assert.LessOrEqual(header.Foot.worldBound.yMax, Bar.Disc.worldBound.yMin + OnePixel, "the hero's words rise clear of the bar (v5)");
            yield return Save("default_bottom_light_OverTheHero");
        }

        [UnityTest]
        public IEnumerator ARealTap_JumpsToTheGroup_TheBumpSlidesThereWithAnOvershoot_AndFollowsRealDrags()
        {
            yield return Open();
            yield return PastTheHero();
            int from = Bar.ChosenIndex, last = Bar.Items.Count - 1;
            Assert.Less(from, last, "precondition: the last group is not the chosen one");
            float target = Bar.Items[last].Button.worldBound.center.x - Bar.Root.worldBound.xMin;
            var button = Bar.Items[last].Button;
            // - a real tap on the last group's button, then every frame of the slide sampled
            float furthest = float.MinValue;
            yield return CardTestInput.Tap(button.panel, button.worldBound.center);
            for (int i = 0; i < 90 && (Bar.Sliding || i < 2); i++)
            {
                furthest = Mathf.Max(furthest, Bar.Centre);
                yield return null;
            }
            yield return CardTestInput.Settle(1f);
            yield return UntilSettled();
            Assert.AreEqual(last, Bar.ChosenIndex, "the tapped group is the chosen one");
            Assert.Greater(furthest, target + 1f, "the bump passed its target on the way (v5's overshoot)");
            Assert.AreEqual(target, Bar.Centre, 0.5f, "...and settled on it");
            string first = Bar.Entries[last].FirstTarget;
            Assert.AreEqual(Stack.LandingTop(Stack.SlotOfBlock(first)), Scroll.scrollOffset.y, 2f, "the jump rests on the group's first block");
            Assert.AreEqual(button.worldBound.center.x, Bar.Disc.worldBound.center.x, 1f, "the disc over the tapped group");
            var peak = Bar.PaintedPath[2].P3;
            Assert.AreEqual(Highest, peak.y, 0.01f, "precondition: the path's highest point is the bump's peak");
            Assert.Less(Mathf.Abs(peak.x - target), button.worldBound.width * 0.5f, "the bump's peak under the tapped group (held clear of the corner)");
            Assert.AreEqual(FontStyle.Bold, Bar.Items[last].Label.resolvedStyle.unityFontStyleAndWeight, "the chosen words bolder (never colour alone)");
            yield return Save("tapped_last_light");

            // - real drags back up the card: the chosen group follows the scroll, and the disc follows the chosen group
            for (int i = 0; i < 8 && Bar.ChosenIndex == last; i++)
            {
                var reach = CardTestInput.ReachableStack(Sheet);
                yield return CardTestInput.TouchDrag(Scroll.panel, new Vector2(reach.center.x, reach.yMin + 40f), new Vector2(0f, reach.height * 0.6f));
                yield return CardTestInput.UntilStill(Sheet);
            }
            yield return CardTestInput.Settle(1f);
            yield return UntilSettled();
            Assert.Less(Bar.ChosenIndex, last, "after real drags up the card, an earlier group is the one being read");
            Assert.AreEqual(Bar.Items[Bar.ChosenIndex].Button.worldBound.center.x, Bar.Disc.worldBound.center.x, 1f, "the disc followed it");
        }

        // The bar's clear room is the card's: its host is full width and the bar draws its own margins, so the gutters beside the bar, the gap
        // under it and the room above its edge show the card -- a finger there lands on the card (a real stroke scrolls the stack), while the
        // bar's band and its disc still take their taps (the disc = the chosen group)
        [UnityTest]
        public IEnumerator TheBarsClearRoom_IsTheCards_ARealStrokeThereScrollsTheStack_TheBandAndTheDiscStillTakeTheirTaps()
        {
            yield return Open();
            yield return PastTheHero();
            Assert.IsTrue(BottomShown, "precondition: the bar shows");
            var panel = Sheet.Layer.panel;
            var band = Band;
            int chosen = Bar.ChosenIndex;
            Assert.GreaterOrEqual(chosen, 0, "precondition: a group is chosen");
            // - above the group furthest from the chosen one: no bump rises there
            int far = chosen < Bar.Items.Count / 2 ? Bar.Items.Count - 1 : 0;
            var spots = new (string name, Vector2 at)[]
            {
                ("the gutter left of the bar", new Vector2(band.xMin - 8f, band.center.y)),
                ("the gutter right of the bar", new Vector2(band.xMax + 8f, band.center.y)),
                ("the room above the bar's edge", new Vector2(Bar.Items[far].Button.worldBound.center.x, band.yMin - 12f)),
                ("the gap under the bar", new Vector2(band.center.x, band.yMax + 8f)),
            };
            foreach (var (name, at) in spots)
            {
                Assert.IsTrue(Scroll.contentViewport.worldBound.Contains(at), "precondition: " + name + " " + at + " lies over the stack " + Scroll.contentViewport.worldBound);
                var picked = panel.Pick(at);
                Assert.IsTrue(picked == Scroll.contentContainer || Scroll.contentContainer.Contains(picked), name + ": a finger lands on the card, not on " + picked?.name);
                // - a real stroke started there, up the card: the stack scrolls
                float before = Scroll.scrollOffset.y;
                yield return CardTestInput.TouchDrag(panel, at, new Vector2(0f, -120f));
                yield return CardTestInput.UntilStill(Sheet);
                Assert.Greater(Scroll.scrollOffset.y, before + 20f, name + ": a real stroke from there scrolled the stack (" + before + " -> " + Scroll.scrollOffset.y + ")");
                yield return PastTheHero();
                yield return CardTestInput.Settle(0.6f);
            }

            // - the band: a group's button under the finger
            var button = Bar.Items[far].Button;
            var onBand = panel.Pick(button.worldBound.center);
            Assert.IsTrue(onBand == button || button.Contains(onBand), "the band: the group's own button takes the finger, not " + onBand?.name);
            // - the disc where it rises past the band's edge: a real tap there is a tap on the chosen group
            chosen = Bar.ChosenIndex;
            var discTop = new Vector2(Bar.Disc.worldBound.center.x, (Bar.Disc.worldBound.yMin + band.yMin) * 0.5f);
            Assert.Less(Bar.Disc.worldBound.yMin, band.yMin - 4f, "precondition: the disc rises past the band's edge");
            var onDisc = panel.Pick(discTop);
            Assert.IsTrue(onDisc == Bar.Disc || Bar.Disc.Contains(onDisc), "the disc above the band takes the finger, not " + onDisc?.name);
            string first = Bar.Entries[chosen].FirstTarget;
            yield return CardTestInput.Tap(panel, discTop);
            yield return CardTestInput.Settle(1f);
            yield return UntilSettled();
            Assert.AreEqual(Stack.LandingTop(Stack.SlotOfBlock(first)), Scroll.scrollOffset.y, 2f, "a real tap on the disc jumps to the chosen group's first block");
        }

        // The P2 review's bug (_3.3 fourth round > P2 review (c)): the bump and the disc follow the chosen group on every scroll, not only the
        // first. The card is pushed live twice before reading (a live push rebinds: the bar is taken off its slot and put back), then real
        // finger strokes walk it down group by group; at each new group, once the slide rests, the bump's peak is where the rule puts it for
        // that group's centre and the disc sits on the group. Bottom and Top, light and dark
        [UnityTest]
        public IEnumerator RealDragsToEachGroupInTurn_TheBumpAndTheDiscFollowTheChosenGroup(
            [Values(CardOptions.NavBarBottom, CardOptions.NavBarTop)] string position, [Values(CardOptions.ModeLight, CardOptions.ModeDark)] string mode)
        {
            yield return Open(b => b.position = position);
            yield return Push(b => b.position = position);
            ThemeController.SetMode(mode);
            yield return CardTestInput.Settle(0.2f);
            Assert.AreEqual(mode, ThemeController.CurrentMode, "precondition: the card shows " + mode);
            Assert.AreEqual(0, Bar.ChosenIndex, "precondition: at the top of the card the first group is chosen (" + position + ")");
            var visited = new List<int> { 0 };
            var checkedAtRest = new HashSet<int>();
            for (int stroke = 0; stroke < 100 && Bar.ChosenIndex < Bar.Items.Count - 1; stroke++)
            {
                // - short strokes, and the chosen group read on every frame of the stroke and of its fling: a short section (one
                //   block) passed within a stroke was chosen on the way -- a reading only at rest missed it
                var reach = CardTestInput.ReachableStack(Sheet);
                var drag = CardTestInput.TouchDrag(Scroll.panel, new Vector2(reach.center.x, reach.yMax - 40f), new Vector2(0f, -reach.height * 0.15f));
                while (drag.MoveNext())
                {
                    if (!visited.Contains(Bar.ChosenIndex)) visited.Add(Bar.ChosenIndex);
                    yield return drag.Current;
                }
                var rest = CardTestInput.UntilStill(Sheet);
                while (rest.MoveNext())
                {
                    if (!visited.Contains(Bar.ChosenIndex)) visited.Add(Bar.ChosenIndex);
                    yield return rest.Current;
                }
                if (!visited.Contains(Bar.ChosenIndex)) visited.Add(Bar.ChosenIndex);
                // - the indicators judged on each group the strokes come to REST on (once each)
                if (!checkedAtRest.Add(Bar.ChosenIndex)) continue;
                yield return UntilSettled();
                var button = Bar.Items[Bar.ChosenIndex].Button;
                float centre = button.worldBound.center.x - Bar.Root.worldBound.xMin;
                string what = $"{position} {mode} '{Bar.Items[Bar.ChosenIndex].Label.text}'";
                Assert.AreEqual(centre, Bar.Centre, 0.5f, what + ": the indicators' centre came to the chosen group");
                Assert.AreEqual(button.worldBound.center.x, Bar.Disc.worldBound.center.x, 1f, what + ": the disc sits on the chosen group");
                float peak = Bar.PaintedPath[2].P3.x;
                float expected = NavBarShapeRule.BumpX(centre, Bar.Root.layout.width, Bar.CornerRadius, Bar.BumpHalf);
                Assert.AreEqual(expected, peak, 0.5f, what + ": the bump's peak is drawn under the chosen group (held clear of the corners)");
                yield return Save($"follows_{position}_{mode}_{Bar.ChosenIndex}", "Premium/P3");
            }
            visited.Sort();
            CollectionAssert.AreEqual(Enumerable.Range(0, Bar.Items.Count).ToList(), visited,
                $"{position} {mode}: real strokes down the card chose every group in turn");
            Assert.GreaterOrEqual(checkedAtRest.Count, Bar.Items.Count - 1, $"{position} {mode}: the indicators judged at rest on almost every group (" + checkedAtRest.Count + ")");
        }

        // _3.3 P3a: Raised is a CUT-OUT round the disc on the render, Bottom and Top -- at the chosen group's centre the bar's edge rises (the
        // bar's fill shows in the ring between the disc and the bump's peak, the hero beyond the peak), while well beside the bump the edge
        // stays flat (the hero shows where the ring is over the bump). Read on the solid default bar over the hero (its fill opaque, the
        // picture behind it never the bar's colour)
        [UnityTest]
        public IEnumerator TheRaisedBump_IsACutOutRoundTheDisc_TheEdgeRisesAtTheChosenGroup_OnTheRender([Values("bottom", "top")] string position)
        {
            yield return Open(b => b.position = position);
            Assert.IsTrue(Bar.OverHero, "precondition: the bar lies on the hero's picture");
            Assert.AreEqual(DisplayStyle.Flex, Bar.Disc.resolvedStyle.display, "precondition: the raised disc");
            bool bottom = position == CardOptions.NavBarBottom;
            var box = Bar.Surface.worldBound;
            float edge = bottom ? Bar.Room : Bar.BarHeight, rise = bottom ? -1f : 1f;
            float cx = box.xMin + Bar.Centre;
            float peak = box.yMin + edge + rise * Bar.BumpHeight;
            var disc = Bar.Disc.worldBound;
            float discEnd = bottom ? disc.yMin : disc.yMax;
            Assert.AreEqual(cx, disc.center.x, 1f, "precondition: the disc on the chosen group");
            float ring = Mathf.Abs(discEnd - peak);
            Assert.GreaterOrEqual(ring, 4f, "the disc sits a ring of bar inside the bump's peak (" + ring + " pt)");
            Texture2D shot = null;
            yield return ThemePixelSet.GrabStill("raised_cutout_" + position, null, r => shot = r);
            try
            {
                var origin = RuntimePanelUtils.ScreenToPanel(Bar.Root.panel, Vector2.zero);
                float unit = ThemePixelSet.PanelUnitsPerPixel(Bar.Root);
                Color At(float x, float y) => shot.GetPixel(Mathf.RoundToInt((x - origin.x) / unit), shot.height - 1 - Mathf.RoundToInt((y - origin.y) / unit));
                float Off(Color a, Color b) => ((Vector4)a - (Vector4)b).magnitude;
                var fill = Bar.PaintedFill;
                // - in the ring, half way between the bump's peak and the disc: the bar's own fill
                var inRing = At(cx, (peak + discEnd) * 0.5f);
                Assert.Less(Off(inRing, fill), 0.04f, position + ": the bar's fill in the ring over the disc " + (Color32)inRing + " (fill " + (Color32)fill + ")");
                // - 3 pt beyond the peak: no bar (the hero)
                var beyond = At(cx, peak + rise * 3f);
                Assert.Greater(Off(beyond, fill), 0.04f, position + ": beyond the bump's peak the bar ends " + (Color32)beyond);
                // - the same height, well beside the bump (its foot plus 20): the edge is flat there, the ring's height is the hero's
                float side = Bar.Centre > box.width * 0.5f ? cx - Bar.BumpHalf - 20f : cx + Bar.BumpHalf + 20f;
                var flat = At(side, (peak + discEnd) * 0.5f);
                Assert.Greater(Off(flat, fill), 0.04f, position + ": beside the bump the edge stays flat " + (Color32)flat);
                System.IO.File.WriteAllBytes(TestEvidence.PathFor("Premium/P3", "raised_cutout_" + position + ".png"), shot.EncodeToPNG());
            }
            finally { Object.Destroy(shot); }
        }

        // One case per option value: what each value draws, read off the running card
        private static IEnumerable<string> OptionValues() => new[]
        {
            "position=bottom", "position=top", "shape=floating", "shape=full_width", "fill=solid", "fill=glass", "fill=clear",
            "indicator=raised", "indicator=pill", "indicator=underline", "indicator=none", "chosen_icon=plain", "chosen_icon=filled",
            "labels=icon_and_text", "labels=icon", "labels=text", "labels=selected_only", "selected_color=accent", "selected_color=ink",
            "selected_color=group", "bold=on", "bold=off", "show=always", "show=after_hero", "hide=off", "hide=on",
        };

        [UnityTest]
        public IEnumerator EveryOptionValue_ChangesWhatTheVisitorSees([ValueSource(nameof(OptionValues))] string option)
        {
            string key = option.Split('=')[0], value = option.Split('=')[1];
            // - the indicator / chosen icon / labels / colour cases on the pill, so the chosen icon is in its own place to read
            bool onPill = key == "chosen_icon" || key == "bold" || key == "selected_color";
            yield return Open(b =>
            {
                if (onPill) b.indicator = CardOptions.NavBarIndicatorPill;
                switch (key)
                {
                    case "position": b.position = value; break;
                    case "shape": b.shape = value; break;
                    case "fill": b.fill = value; break;
                    case "indicator": b.indicator = value; break;
                    case "chosen_icon": b.chosen_icon = value; break;
                    case "labels": b.labels = value; break;
                    case "selected_color": b.selected_color = value; break;
                    case "bold":
                        b.bold_selected = value == "on";
                        // - the chosen icon in its own place, where its lines are measured
                        b.chosen_icon = CardOptions.NavBarChosenIconPlain;
                        break;
                    case "show": b.show = value; break;
                    case "hide": b.hide_on_scroll = value == "on" ? CardOptions.NavBarHideDown : CardOptions.NavBarHideNever; break;
                }
            });
            var sheet = Sheet.Root.worldBound;
            var chosen = Bar.Items[Bar.ChosenIndex];
            switch (option)
            {
                case "position=bottom":
                    Assert.AreEqual(NavBarView.ElementKey, Sheet.Slots.BottomKey);
                    Assert.Greater(Band.center.y, sheet.center.y, "at the bottom of the card");
                    break;
                case "position=top":
                    Assert.AreEqual(NavBarView.ElementKey, Sheet.Slots.TopKey, "in the top host");
                    Assert.Less(Band.center.y, sheet.center.y, "at the top of the card");
                    // - the chrome row is the X and the title's row (the Fade band runs on below it, fading out over what lies there)
                    Assert.GreaterOrEqual(Band.yMin, Mathf.Max(Sheet.CloseButton.worldBound.yMax, Stack.HeaderSlot.worldBound.yMax) - OnePixel,
                        "under the chrome row");
                    break;
                case "shape=floating":
                    Assert.AreEqual(sheet.width - 2f * (Band.xMin - sheet.xMin), Band.width, OnePixel, "inset the same on both sides");
                    Assert.Greater(Band.xMin - sheet.xMin, 4f);
                    break;
                case "shape=full_width":
                    Assert.AreEqual(sheet.width, Band.width, OnePixel, "edge to edge");
                    Assert.IsTrue(Bar.PaintedPath.Any(c => c.Op == NavBarShapeRule.Op.Move && Mathf.Abs(c.P1.x) < 0.01f), "square corners: the path starts at x 0");
                    Assert.IsFalse(Bar.PaintedShadow, "full width: no shadow");
                    break;
                case "fill=solid":
                    Assert.IsTrue(Bar.PaintedShadow, "the shadow only under Solid");
                    Assert.AreEqual(1f, Bar.PaintedFill.a, 0.01f, "opaque");
                    break;
                case "fill=glass":
                    Assert.IsFalse(Bar.PaintedShadow, "no shadow under glass (it shows through as a grey haze)");
                    Assert.Less(Bar.PaintedFill.a, 1f, "see-through");
                    Assert.IsTrue(Bar.OverImage, "over the hero: white on dark glass");
                    AreSame(Theme(Bar.Root, "--ts-chrome-glass-hero"), Bar.PaintedFill, "the dark glass over the hero");
                    yield return PastTheHero();
                    AreSame(Theme(Bar.Root, "--ts-navbar-glass"), Bar.PaintedFill, "the bar's glass after it");
                    break;
                case "fill=clear":
                    Assert.IsFalse(Bar.PaintedShadow);
                    Assert.AreEqual(0f, Bar.PaintedFill.a, 0.001f, "transparent over the hero");
                    AreSame(Theme(Bar.Root, "--ts-on-scrim"), chosen.Label.resolvedStyle.color, "...its words white on the picture");
                    yield return PastTheHero();
                    AreSame(Theme(Bar.Root, "--ts-bar"), Bar.PaintedFill, "solid after the hero");
                    break;
                case "indicator=raised":
                    Assert.Less(Highest, Bar.Room - 1f, "the bump");
                    Assert.Less(Bar.Disc.worldBound.yMin, Band.yMin, "the disc rides up past the edge");
                    break;
                case "indicator=pill":
                    Assert.AreEqual(Bar.Room, Highest, 0.01f, "a flat edge");
                    Assert.AreEqual(DisplayStyle.Flex, Bar.Pill.resolvedStyle.display, "the pill");
                    Assert.AreEqual(chosen.Button.worldBound.center.x, Bar.Pill.worldBound.center.x, 1f, "...behind the chosen group");
                    Assert.AreEqual(DisplayStyle.None, Bar.Underline.resolvedStyle.display);
                    break;
                case "indicator=underline":
                    Assert.AreEqual(Bar.Room, Highest, 0.01f, "a flat edge");
                    Assert.AreEqual(DisplayStyle.Flex, Bar.Underline.resolvedStyle.display, "the underline");
                    Assert.AreEqual(chosen.Button.worldBound.center.x, Bar.Underline.worldBound.center.x, 1f, "...under the chosen group");
                    Assert.Greater(Bar.Underline.worldBound.center.y, chosen.Label.worldBound.yMax - OnePixel, "...below its words");
                    break;
                case "indicator=none":
                    Assert.AreEqual(Bar.Room, Highest, 0.01f, "a flat edge");
                    Assert.AreEqual(DisplayStyle.None, Bar.Pill.resolvedStyle.display);
                    Assert.AreEqual(DisplayStyle.None, Bar.Underline.resolvedStyle.display);
                    Assert.AreEqual(FontStyle.Bold, chosen.Label.resolvedStyle.unityFontStyleAndWeight, "still marked by its bolder words");
                    break;
                case "chosen_icon=plain":
                    Assert.AreEqual(DisplayStyle.None, Bar.Disc.resolvedStyle.display, "no disc");
                    Assert.AreEqual(1f, chosen.Icon.resolvedStyle.opacity, 0.01f, "the chosen icon in its place");
                    break;
                case "chosen_icon=filled":
                    Assert.AreEqual(DisplayStyle.Flex, Bar.Disc.resolvedStyle.display, "an accent disc");
                    Assert.AreEqual(chosen.Icon.worldBound.center.y, Bar.Disc.worldBound.center.y, 1f, "...at the icon's own place");
                    Assert.AreEqual(chosen.Icon.worldBound.center.x, Bar.Disc.worldBound.center.x, 1f);
                    Assert.Less(Bar.Disc.worldBound.width, Bar.BarHeight, "Filled's smaller disc, inside the bar");
                    break;
                case "labels=icon_and_text":
                    Assert.IsTrue(Bar.Items.All(i => i.Label.resolvedStyle.display == DisplayStyle.Flex), "every word");
                    Assert.IsTrue(Bar.Items.Where(i => i != chosen).All(i => i.Icon.resolvedStyle.display == DisplayStyle.Flex), "every icon");
                    break;
                case "labels=icon":
                    Assert.IsTrue(Bar.Items.All(i => i.Label.resolvedStyle.display == DisplayStyle.None), "no words");
                    Assert.AreEqual(Bar.BarHeight, Band.height, OnePixel, "the shorter bar (no row of words)");
                    break;
                case "labels=text":
                    Assert.IsTrue(Bar.Items.All(i => i.Icon.resolvedStyle.display == DisplayStyle.None), "no icons");
                    Assert.IsTrue(Bar.Items.All(i => i.Label.resolvedStyle.display == DisplayStyle.Flex), "every word");
                    Assert.AreEqual(DisplayStyle.Flex, Bar.Disc.resolvedStyle.display, "Raised still rides its disc");
                    break;
                case "labels=selected_only":
                    Assert.AreEqual(DisplayStyle.Flex, chosen.Label.resolvedStyle.display, "the chosen group's words");
                    Assert.IsTrue(Bar.Items.Where(i => i != chosen).All(i => i.Label.resolvedStyle.display == DisplayStyle.None), "...only");
                    break;
                case "selected_color=accent":
                    AreSame(Theme(Bar.Root, "--ts-accent"), chosen.Label.resolvedStyle.color, "the chosen words in the accent");
                    break;
                case "selected_color=ink":
                    AreSame(Theme(Bar.Root, "--ts-ink"), chosen.Label.resolvedStyle.color, "the chosen words in the ink");
                    AreSame(Theme(Bar.Root, "--ts-ink"), Bar.Disc.resolvedStyle.backgroundColor, "...and the disc");
                    break;
                case "selected_color=group":
                    AreSame(Theme(Bar.Root, Bar.Entries[Bar.ChosenIndex].Item.ColorToken), Bar.Disc.resolvedStyle.backgroundColor, "the disc in the group's colour");
                    AreSame(Theme(Bar.Root, "--ts-ink"), chosen.Label.resolvedStyle.color, "...its words stay text colour");
                    break;
                case "bold=on":
                case "bold=off":
                    Assert.AreEqual(value == "on", Bar.Root.ClassListContains("nav-bar--bold"));
                    // - the heavier stroke is pixels: the chosen icon's ink on the render, bold against plain
                    yield return CompareIconInk(value == "on");
                    break;
                case "show=always":
                    Assert.IsTrue(BottomShown, "there at rest, over the hero");
                    break;
                case "show=after_hero":
                    Assert.IsFalse(BottomShown, "at rest the hero is the screen: the bar waits");
                    yield return ScrollTo(Stack.HeaderView.HeroHeight * 0.7f);
                    Assert.IsTrue(BottomShown, "the hero scrolled away: the bar comes");
                    break;
                case "hide=off":
                case "hide=on":
                    yield return PastTheHero();
                    for (int i = 0; i < 3; i++)
                    {
                        // - real finger strokes down the card, inside the part of the stack a finger reaches
                        var reach = CardTestInput.ReachableStack(Sheet);
                        yield return CardTestInput.TouchDrag(Scroll.panel, new Vector2(reach.center.x, reach.yMax - 40f), new Vector2(0f, -reach.height * 0.3f));
                        if (value == "on" && !BottomShown) break;
                    }
                    yield return CardTestInput.Settle(0.6f);
                    Assert.AreEqual(value == "off", BottomShown, value == "on" ? "reading down: it slides away" : "off: it stays while reading down");
                    break;
            }
            yield return Save("option_" + key + "_" + value);
        }

        // The chosen icon's ink on the render: bold draws more of it than plain (both rendered, the same card, the same group)
        private IEnumerator CompareIconInk(bool boldNow)
        {
            int Ink(Texture2D t, RectInt r, Color ground)
            {
                int count = 0;
                for (int y = r.yMin; y < r.yMax; y++)
                    for (int x = r.xMin; x < r.xMax; x++)
                        if (((Vector4)t.GetPixel(x, y) - (Vector4)ground).magnitude > 0.25f) count++;
                return count;
            }
            Texture2D a = null, b = null;
            var icon = Bar.Items[Bar.ChosenIndex].Icon;
            try
            {
                yield return ThemePixelSet.GrabStill("bold_" + boldNow, Sheet.Root, r => a = r);
                Bar.Root.EnableInClassList("nav-bar--bold", !boldNow);
                yield return null;
                yield return null;
                yield return ThemePixelSet.GrabStill("bold_" + !boldNow, Sheet.Root, r => b = r);
                var area = ThemePixelSet.ScreenRectOf(icon, a);
                var ground = a.GetPixel(area.xMin, area.yMin);
                int now = Ink(a, area, ground), other = Ink(b, area, ground);
                Assert.Greater(now + other, 0, "precondition: the icon draws pixels");
                if (boldNow) Assert.Greater(now, other * 1.1f, "bold draws heavier lines than plain (" + now + " vs " + other + " px)");
                else Assert.Less(now * 1.1f, other, "plain draws thinner lines than bold (" + now + " vs " + other + " px)");
            }
            finally
            {
                Bar.Root.EnableInClassList("nav-bar--bold", boldNow);
                if (a != null) Object.Destroy(a);
                if (b != null) Object.Destroy(b);
            }
        }

        [UnityTest]
        public IEnumerator RaisedAtTop_TheBumpHangsDown_TheDiscBelowTheBarsFoot()
        {
            yield return Open(b => b.position = CardOptions.NavBarTop);
            yield return PastTheHero();
            Assert.AreEqual(Bar.BarHeight + Bar.BumpHeight, Lowest, 0.01f, "the path's lowest point is the bump, its height below the bar's foot");
            Assert.AreEqual(0f, Highest, 0.01f, "...its top edge straight");
            float gap = CardTestInput.TokenPx("--ts-navbar-disc-gap");
            Assert.AreEqual(Band.yMax + Bar.BumpHeight - gap, Bar.Disc.worldBound.yMax, 1f, "the disc hangs in the bump, its foot a ring of bar above the bump's lowest point");
            Assert.Greater(Bar.Disc.worldBound.yMax, Band.yMax + 4f, "...below the bar's foot");
            Assert.AreEqual(Bar.Items[Bar.ChosenIndex].Button.worldBound.center.x, Bar.Disc.worldBound.center.x, 1f, "...under the chosen group");
            yield return Save("raised_top_light_AfterTheHero");
        }

        [UnityTest]
        public IEnumerator TheLabels_ReadAt7To1_OnTheRealRender_OnEveryFill_OverTheHeroAndAfterIt(
            [Values(CardOptions.NavBarBottom, CardOptions.NavBarTop)] string position, [Values("light", "dark")] string mode)
        {
            ThemeController.SetMode(mode);
            Session.CardSettings.container.reduce_motion = true;
            var report = new List<string>();
            float worst = float.MaxValue;
            string worstWhat = "";
            foreach (string fill in CardOptions.NavBarFills)
            {
                yield return Open(b =>
                {
                    b.position = position;
                    b.fill = fill;
                    // - Labels Text On The Chosen would leave words out: every word read
                    b.labels = CardOptions.NavBarLabelsIconAndText;
                });
                // - the push re-applies the wall's Default Mode: the mode measured is set after it, and proven
                ThemeController.SetMode(mode);
                yield return CardTestInput.Settle(0.2f);
                Assert.AreEqual(mode, ThemeController.CurrentMode, "precondition: the card shows " + mode);
                foreach (bool overHero in new[] { true, false })
                {
                    if (!overHero) yield return PastTheHero();
                    else yield return ScrollTo(0f);
                    if (overHero && !Bar.OverHero)
                    {
                        // - the Top bar lies on the hero only while the picture reaches past its foot: at the top of the card it does
                        Assert.Fail("precondition: at the top of the card the " + position + " bar lies on the hero (" + fill + ")");
                    }
                    Assert.AreEqual(overHero, Bar.OverHero, "precondition: the bar is " + (overHero ? "" : "not ") + "over the hero");
                    string place = overHero ? "OverTheHero" : "AfterTheHero";
                    var labels = Bar.Items.Select(i => i.Label).Where(l => l.resolvedStyle.display == DisplayStyle.Flex).ToList();
                    var colours = labels.ToDictionary(l => l, l => l.resolvedStyle.color);
                    Texture2D drawn = null, under = null;
                    try
                    {
                        yield return ThemePixelSet.GrabStill($"labels_{position}_{fill}_{mode}_{place}", null, r => drawn = r);
                        foreach (var l in labels) l.style.color = Color.clear;
                        yield return null;
                        yield return ThemePixelSet.GrabStill($"labels_under_{position}_{fill}_{mode}_{place}", null, r => under = r);
                        foreach (var l in labels)
                        {
                            var (ratio, worstPixel, inked) = ThemePixelSet.RingContrast(drawn, under, ThemePixelSet.ScreenRectOf(l, drawn), colours[l]);
                            Assert.Greater(inked, 0, "precondition: the words '" + l.text + "' draw pixels");
                            report.Add($"{position} {fill} {mode} {place} '{l.text}': {ratio:0.00}:1 (worst {(Color32)worstPixel}, words {(Color32)colours[l]})");
                            if (ratio < worst) { worst = ratio; worstWhat = $"'{l.text}' {position} {fill} {mode} {place}"; }
                        }
                        System.IO.File.WriteAllBytes(TestEvidence.PathFor(Domain, $"labels_{position}_{fill}_{mode}_{place}.png"), drawn.EncodeToPNG());
                    }
                    finally
                    {
                        foreach (var l in labels) l.style.color = colours[l];
                        if (drawn != null) Object.Destroy(drawn);
                        if (under != null) Object.Destroy(under);
                    }
                }
            }
            System.IO.File.AppendAllText(TestEvidence.PathFor(Domain, "labels_ratios.txt"), string.Join("\n", report) + "\n");
            Assert.GreaterOrEqual(worst, Aaa, "every group's words keep 7:1 against what they are drawn over -- worst: " + worstWhat + "\n" + string.Join("\n", report));
        }

        [UnityTest]
        public IEnumerator AnOldConfigsUnderTitle_ShowsTheBarAtTop_AMergedGroupLandsOnEitherFamily_AndOffTakesTheBarAway()
        {
            yield return Open(b =>
            {
                b.position = CardOptions.NavBarUnderTitle;
                b.items = new List<CardNavBarItem>
                {
                    new() { key = "group_1", families = new List<string> { BuiltInFamilies.Media, BuiltInFamilies.Guides },
                        label = new List<LocalizedEntry> { new() { lang = "en", value = "Watch" }, new() { lang = "pt", value = "Ver" } } },
                    new() { key = BuiltInFamilies.About, families = new List<string> { BuiltInFamilies.About } },
                };
            });
            Assert.AreEqual(NavBarView.ElementKey, Sheet.Slots.TopKey, "an old config's under_title is Top on the running card");
            Assert.AreEqual(2, Bar.Items.Count, "the two groups the wall made");
            Assert.That(Bar.Items[0].Label.text, Is.EqualTo("Watch").Or.EqualTo("Ver"), "the group's own words");
            string first = Sheet.Nav.Model.Targets.First(t =>
            {
                Sheet.Nav.TryGetEntry(t, out var e);
                return e.Definition.Family == BuiltInFamilies.Media || e.Definition.Family == BuiltInFamilies.Guides;
            });
            Assert.AreEqual(first, Bar.Entries[0].FirstTarget, "the merged group lands on the first block of either family");

            yield return Push(b => b.enabled = false);
            yield return CardTestInput.Settle(0.4f);
            Assert.IsNull(Bar.Root.parent, "off: the bar is out of the card");
            Assert.IsFalse(Sheet.Root.ClassListContains(NavSlots.SheetBarBottomClass), "...and the stack's end no longer keeps clear of it");
        }

        // The card as the visitor sees it now -> TestEvidence/Premium/P2/<name>.png
        private IEnumerator Save(string name, string domain = Domain)
        {
            Texture2D shot = null;
            yield return ThemePixelSet.GrabStill(name, null, r => shot = r);
            if (shot == null) yield break;
            System.IO.File.WriteAllBytes(TestEvidence.PathFor(domain, name + ".png"), shot.EncodeToPNG());
            Object.Destroy(shot);
        }
    }
}

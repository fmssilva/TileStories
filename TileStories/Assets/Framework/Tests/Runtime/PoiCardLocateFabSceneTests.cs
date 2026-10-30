using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // The Locate Button ("Show me where it is", _3.3 P3d review b; LocateFabView + LocatePin + LocateFabRule) on the REAL scene with real input:
    //   - the default: Pin + Label in AR Teal, the pin as big as the bar's chosen disc (Size 1), floating in its touch target at the bottom right,
    //     12 above the bar's highest point (the raised disc's top), "Find it on the wall" on its chip; named by the Show On Wall words
    //   - neither the button nor its chip meets the bar's items or the disc whichever group is chosen, nor the hero's words (Half, Full)
    //   - a REAL tap at Half and at Full lowers the card to Peek with the point still selected: its marker on the wall
    //   - a REAL drag folds the label away for this card, the pin staying; another point shows it again
    //   - Float: the pin rises Bob pt and its shadow shrinks on the motion's own clock; Float off and Reduce Motion keep it still
    //   - Size: the pin's height the bar's chosen disc times Size (0.6, 2), the touch target never under 44 pt, the float inside it
    //   - it floats in its own layer after the sheet: the hero's words stand exactly where they stand with the button off
    //   - every Style x Colour rendered, light and dark: the rim >= 4.5:1 on the paper (over the hero measured and reported), the label
    //     >= 7:1 on its chip; on every hero picture its dark halo's edge or its white disc >= 3:1 (WCAG 1.4.11), no halo on the paper
    //   - it follows the bar: at Top over the sheet's foot; Hide While Scrolling slides it away and back with REAL strokes
    // Renders in TestEvidence/Premium/P3e/.
    public class PoiCardLocateFabSceneTests : SearchSceneFixture
    {
        private const string Domain = "Premium/P3e";
        private const string Painting = "painting";
        private const string Lamp = "lamp";

        private PoiCardSheetView Sheet => Card.Sheet;
        private BlockStackView Stack => Sheet.Stack;
        private ScrollView Scroll => Stack.Scroll;
        private NavBarView Bar => Sheet.Slots.Bar;
        private LocateFabView Fab => Sheet.Locate;
        private float OnePixel => Sheet.Layer.panel.visualTree.layout.height / Screen.height;
        // - the motion's clock, the test's: frozen at 0 unless a test moves it
        private double _now;

        [UnityTearDown]
        public IEnumerator BackToLight()
        {
            ThemeController.SetMode(CardOptions.ModeLight);
            yield return null;
        }

        // `poi`'s card at `stop`, scrolled to its top, the bar and the Locate Button as `edit` sets them over the framework's defaults
        private IEnumerator Open(System.Action<CardSettings> edit = null, SheetStopRule.Stop stop = SheetStopRule.Stop.Full, string poi = Painting)
        {
            CardTestInput.HoldTheWallStill(Sheet);
            _now = 0d;
            Fab.Clock = () => _now;
            var copy = ConfigCopy();
            copy.card_settings.navigation.bar = new CardNavBarSettings();
            copy.card_settings.locate = new CardLocateSettings();
            edit?.Invoke(copy.card_settings);
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

        // What the visitor sees of the button: its touch target and, while shown, its chip
        private List<Rect> FabRects()
        {
            var rects = new List<Rect> { Fab.Root.worldBound };
            if (Fab.LabelShown) rects.Add(Fab.Chip.worldBound);
            return rects;
        }

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
        public IEnumerator TheDefault_APinWithItsLabel_12AboveTheBar_NeverMeetingItsItemsOrTheHerosWords([Values("half", "full")] string at)
        {
            var stop = at == "half" ? SheetStopRule.Stop.Half : SheetStopRule.Stop.Full;
            yield return Open(stop: stop);
            Assert.AreEqual(NavBarView.ElementKey, Sheet.Slots.BottomKey, "precondition: the default bar at the bottom");
            Assert.IsTrue(Fab.IsShown, "shown at " + at);
            Assert.AreEqual(CardOptions.LocatePinLabel, Fab.Style, "the default style: Pin + Label");
            Assert.IsTrue(Fab.LabelShown, "...its words shown at rest");
            Assert.AreEqual("Find it on the wall", Fab.Chip.text, "...the card's words");
            Assert.AreEqual("Show me where it is", Fab.Root.tooltip, "named by the Show On Wall words");
            // - it floats: its own layer in the card layer right after the sheet (drawn over it, never inside it), under the floating slot
            Assert.AreEqual(Sheet.Layer, Fab.Layer.parent, "the button's layer is a layer of the card");
            Assert.AreEqual(Sheet.Layer.IndexOf(Sheet.Root) + 1, Sheet.Layer.IndexOf(Fab.Layer), "...right after the sheet: drawn over all of it");
            Assert.Less(Sheet.Layer.IndexOf(Fab.Layer), Sheet.Layer.IndexOf(Sheet.Floating.Root), "...under the floating slot, the menus and the full view");
            Assert.IsFalse(Sheet.Root.Contains(Fab.Root), "...never inside the sheet (no room taken, never clipped)");
            Assert.AreEqual(1f, CardLocateSettings.SizeDefault, "precondition: Size's default is the chosen disc itself");
            float icon = CardTestInput.TokenPx("--ts-navbar-disc-raised") * CardLocateSettings.SizeDefault;
            Assert.AreEqual(icon, Fab.Pin.worldBound.height, 0.5f, "the pin: as big as the chosen group's disc (" + icon + ")");
            Assert.AreEqual(Bar.Disc.worldBound.height, Fab.Pin.worldBound.height, 0.5f, "...the disc as drawn");
            Assert.AreEqual(icon * LocateFabRule.PinAspect, Fab.Pin.worldBound.width, 0.5f, "...in the study's proportion");
            var fab = Fab.Root.worldBound;
            float touch = LocateFabRule.Touch(icon, false);
            Assert.GreaterOrEqual(touch, 44f, "a whole touch target");
            Assert.AreEqual(touch, fab.width, 0.5f, "the touch target round the pin and its float");
            Assert.AreEqual(touch, fab.height, 0.5f);
            AreSame(Theme("--ts-locate-teal-rim"), Fab.Pin.Rim, "AR Teal: the rim's token");
            var sheet = Sheet.Root.worldBound;
            Assert.AreEqual(CardTestInput.TokenPx("--ts-navbar-inset"), sheet.xMax - fab.xMax, OnePixel, "at the right, the bar's own inset from the side");
            Assert.AreEqual(BarTop - 12f, fab.yMax, OnePixel, "12 above the bar's highest point (" + BarTop + ")");
            Assert.Less(Fab.Chip.worldBound.xMax, Fab.Pin.worldBound.xMin, "the chip at the pin's left");
            Assert.AreEqual(Fab.Pin.worldBound.yMin + Fab.Pin.worldBound.height * 23f / 64f, Fab.Chip.worldBound.center.y, 1f, "...level with the pin's disc");
            // - whichever group is chosen: a real tap on each, the disc slid there, never under the button or its chip
            for (int i = 0; i < Bar.Items.Count; i++)
            {
                if (i != Bar.ChosenIndex)
                {
                    yield return CardTestInput.TouchDrag(Sheet.Root.panel, Bar.Items[i].Button.worldBound.center, Vector2.zero, frames: 1);
                    yield return CardTestInput.Settle(0.8f);
                    for (int f = 0; f < 120 && Bar.Sliding; f++) yield return null;
                }
                foreach (var r in FabRects())
                {
                    foreach (var item in Bar.Items)
                        Assert.IsFalse(Meets(r, item.Button.worldBound), "group " + i + " chosen: the button meets '" + item.Label.text + "' " + item.Button.worldBound + " / " + r);
                    Assert.IsFalse(Meets(r, Bar.Disc.worldBound), "group " + i + " chosen: ...or the disc " + Bar.Disc.worldBound);
                }
            }
            // - the hero's words at rest at this stop (a jump above moved the card and folded the chip: the card opened again fresh), on both
            //   heroes: every row of words EXACTLY where it is with the button off -- the floating pin never narrows, wraps or moves them
            foreach (string poi in new[] { Lamp, Painting })
            {
                yield return Open(c => c.navigation.bar.locate_button = false, stop, poi);
                Assert.IsFalse(Fab.IsShown, "precondition: the button off");
                var without = HeroRows();
                SelectionEventBus.Clear();
                yield return CardTestInput.Settle();
                yield return Open(stop: stop, poi: poi);
                Assert.IsTrue(Fab.IsShown && Fab.LabelShown, "precondition: " + poi + " opened fresh shows the button and its words (shown " + Fab.IsShown
                    + ", no room " + Fab.Chip.ClassListContains(LocateFabView.NoRoomClass) + ", folded " + Fab.Chip.ClassListContains(LocateFabView.FoldedClass)
                    + ", chip " + Fab.Chip.worldBound + ")");
                var with = HeroRows();
                Assert.AreEqual(without.Count, with.Count, "precondition: the same rows of words");
                Assert.Greater(with.Count, 2, "precondition: the hero's rows are drawn");
                for (int r = 0; r < with.Count; r++)
                {
                    Assert.AreEqual(without[r].Name, with[r].Name, "precondition: the same row in the same place of the list");
                    // - a float epsilon only: the same layout gives the same numbers
                    Vector4 a = new(without[r].Box.x, without[r].Box.y, without[r].Box.width, without[r].Box.height);
                    Vector4 b = new(with[r].Box.x, with[r].Box.y, with[r].Box.width, with[r].Box.height);
                    Assert.Less((a - b).magnitude, 0.01f, at + " " + poi + ": '" + with[r].Name + "' with the button on " + with[r].Box + ", off " + without[r].Box);
                }
                // - the button's own touch target is clear of the words at rest on these heroes (an overlap is accepted in general, never
                //   expected here), and the chip shows only clear of every word's INK
                foreach (var w in HeroWords())
                    Assert.IsFalse(Meets(Fab.Pin.worldBound, w.worldBound), at + " " + poi + ": the pin meets the hero's '" + (w as Label)?.text + "' " + w.worldBound);
                var ink = new List<Ink>();
                InkRects.Collect(Stack.HeaderView.Foot, Sheet.Root, ink);
                Assert.Greater(ink.Count, 2, "precondition: the hero's ink measured");
                var chip = Fab.Chip.worldBound;
                var chipInSheet = new Rect(chip.x - Sheet.Root.worldBound.x, chip.y - Sheet.Root.worldBound.y, chip.width, chip.height);
                foreach (var i in ink)
                    Assert.IsFalse(i.Rect.Overlaps(chipInSheet), at + " " + poi + ": the chip meets the hero's ink " + i.Rect + " / " + chipInSheet);
                yield return Save("default_" + poi + "_" + at);
            }
        }

        // Every row of the hero's words in order -- its texts and actions rows with their padding (what a narrowing would change), then each
        // word and action in them -- named, with its box as laid out
        private List<(string Name, Rect Box)> HeroRows()
        {
            var rows = new List<(string, Rect)>();
            var header = Stack.HeaderView;
            foreach (var row in new[] { header.Text, header.Actions })
                if (row.resolvedStyle.display == DisplayStyle.Flex)
                    rows.Add((row.name + " padding-right " + row.resolvedStyle.paddingRight, row.worldBound));
            foreach (var w in HeroWords())
                rows.Add((w.name + ":" + ((w as Label)?.text ?? ""), w.worldBound));
            return rows;
        }

        // A REAL tap on the pin shows the point on the wall: the card lowers to Peek, the point still selected (its marker stays), at Half and at
        // Full; at Peek the button is gone (the wall is in view). A tap on the chip does the same. Core: the shipped default's "show me where
        // it is" (the old sticky action is off under the default bar)
        [Category("Core")]
        [UnityTest]
        public IEnumerator ARealTap_ShowsThePointOnTheWall_AtHalfAndFull([Values("half", "full")] string at, [Values("pin", "chip")] string on)
        {
            yield return Open(stop: at == "half" ? SheetStopRule.Stop.Half : SheetStopRule.Stop.Full);
            Assert.IsTrue(Fab.IsShown && Fab.LabelShown, "precondition: the button and its words");
            var target = on == "pin" ? Fab.Pin.worldBound.center : Fab.Chip.worldBound.center;
            yield return CardTestInput.TouchDrag(Sheet.Root.panel, target, Vector2.zero, frames: 1);
            yield return CardTestInput.Settle(0.8f);
            Assert.AreEqual(SheetStopRule.Stop.Peek, Sheet.Stop, "the card lowered to Peek: the wall in view");
            Assert.IsTrue(Sheet.IsOpen, "...the card still open");
            Assert.AreEqual(Painting, SelectionEventBus.CurrentPoiId, "...the point still selected: its marker on the wall");
            Assert.IsFalse(Fab.IsShown, "at Peek: no button");
            // - its layer is not the sheet's: closing the card takes it away too
            SelectionEventBus.Clear();
            yield return CardTestInput.Settle();
            Assert.IsFalse(Sheet.IsOpen, "precondition: the card closed");
            Assert.AreEqual(DisplayStyle.None, Fab.Root.resolvedStyle.display, "the card closed: the button gone with it");
        }

        // A REAL drag down the card folds the words away, the pin staying (and still a tap away); scrolling back to the top keeps them folded;
        // the next point's card shows them again
        [UnityTest]
        public IEnumerator ARealDrag_FoldsTheLabelAway_ThePinStays_TheNextPointShowsItAgain()
        {
            yield return Open();
            Assert.IsTrue(Fab.LabelShown, "precondition: the words at rest");
            var reach = CardTestInput.ReachableStack(Sheet);
            yield return CardTestInput.TouchDrag(Scroll.panel, reach.center, new Vector2(0f, -120f));
            yield return CardTestInput.UntilStill(Sheet);
            yield return CardTestInput.Settle(0.4f);
            Assert.Greater(Scroll.scrollOffset.y, 1f, "precondition: the drag scrolled the card");
            Assert.IsFalse(Fab.LabelShown, "the words folded away");
            Assert.AreEqual(0f, Fab.Chip.resolvedStyle.opacity, 0.01f, "...faded out");
            Assert.AreEqual(PickingMode.Ignore, Fab.Chip.pickingMode, "...taking no finger");
            Assert.IsTrue(Fab.IsShown, "the pin stays");
            Scroll.scrollOffset = Vector2.zero;
            yield return CardTestInput.Settle(0.3f);
            Assert.IsFalse(Fab.LabelShown, "back at the top: still folded for this card");
            SelectionEventBus.Select(Lamp);
            yield return CardTestInput.Settle();
            Sheet.SetStop(SheetStopRule.Stop.Full);
            yield return CardTestInput.Settle(0.6f);
            Assert.IsTrue(Fab.LabelShown, "another point's card: the words again");
        }

        // Float on: on the motion's own clock the pin rises Bob pt half way through its period and its shadow shrinks; back at rest at a
        // whole period. Float off and Reduce Motion: no motion at all, whatever the clock
        [UnityTest]
        public IEnumerator Float_MovesThePinOnItsClock_FloatOffAndReduceMotionKeepItStill()
        {
            yield return Open();
            Assert.IsTrue(Fab.Moving, "Float on: its motion runs");
            float rest = Fab.Pin.worldBound.yMin, shadowRest = Fab.Shadow.worldBound.width;
            _now = LocateFabRule.BobPeriod * 0.5;
            yield return null;
            yield return null;
            Assert.AreEqual(rest - LocateFabRule.Bob, Fab.Pin.worldBound.yMin, OnePixel, "half way: the pin up by " + LocateFabRule.Bob + " pt");
            Assert.AreEqual(shadowRest * 0.7f, Fab.Shadow.worldBound.width, 0.5f, "...its shadow shrunk to 0.7");
            Assert.AreEqual(0.5f, Fab.Shadow.resolvedStyle.opacity, 0.01f, "...and faded");
            _now = LocateFabRule.BobPeriod;
            yield return null;
            yield return null;
            Assert.AreEqual(rest, Fab.Pin.worldBound.yMin, OnePixel, "a whole period: back at rest");

            foreach (var (name, edit) in new (string, System.Action<CardSettings>)[]
            {
                ("Float off", c => c.locate.float_motion = false),
                ("Reduce Motion", c => c.container.reduce_motion = true),
            })
            {
                yield return Open(edit);
                Assert.IsFalse(Fab.Moving, name + ": no motion runs");
                rest = Fab.Pin.worldBound.yMin;
                _now = LocateFabRule.BobPeriod * 0.5;
                for (int f = 0; f < 3; f++) yield return null;
                Assert.AreEqual(rest, Fab.Pin.worldBound.yMin, 0.01f, name + ": the pin still");
            }
        }

        // Size: the pin's height the chosen group's disc times Size (the Editor's ends, 0.6 and 2), the touch target round it, never under 44
        // pt, and everything that floats inside it at rest AND at the top of its float (the pin, or the glass disc holding it, and the shadow);
        // still 12 above the bar
        [UnityTest]
        public IEnumerator Size_ScalesThePinFromTheChosenDisc_ItsFloatStaysInsideItsTouchTarget([Values(0.6f, 2f)] float size, [Values("pin", "glass_ping")] string style)
        {
            yield return Open(c => { c.locate.size = size; c.locate.style = style; });
            float icon = CardTestInput.TokenPx("--ts-navbar-disc-raised");
            Assert.AreEqual(icon * size, Fab.Pin.worldBound.height, 0.5f, size + "x: the pin's height");
            var fab = Fab.Root.worldBound;
            Assert.GreaterOrEqual(fab.width, 44f, "a whole touch target");
            Assert.GreaterOrEqual(fab.height, 44f);
            Assert.AreEqual(LocateFabRule.Touch(icon * size, style == CardOptions.LocateGlassPing), fab.height, 0.5f, "the rule's touch target");
            if (style == CardOptions.LocateGlassPing)
                Assert.AreEqual(icon * size * LocateFabRule.GlassToPin, Fab.Glass.worldBound.width, 0.5f, "the glass disc round the pin");
            Assert.AreEqual(BarTop - 12f, fab.yMax, OnePixel, "12 above the bar's highest point");
            // - inside at rest, then half way through the float (its top: Bob pt up)
            Assert.IsTrue(Fab.Moving, "precondition: Float on");
            foreach (double at in new[] { 0d, LocateFabRule.BobPeriod * 0.5 })
            {
                _now = at;
                yield return null;
                yield return null;
                foreach (var part in new[] { Fab.Float, Fab.Pin, Fab.Shadow })
                {
                    var r = part.worldBound;
                    Assert.IsTrue(r.xMin >= fab.xMin - 0.01f && r.xMax <= fab.xMax + 0.01f && r.yMin >= fab.yMin - 0.01f && r.yMax <= fab.yMax + 0.01f,
                        $"{size}x {style}, float {at:0.0} s: {part.name} {r} inside its touch target {fab}");
                }
            }
            Assert.AreEqual(Fab.Float.worldBound.yMin, Fab.Root.worldBound.yMin + (fab.height - LocateFabRule.Standing(icon * size, style == CardOptions.LocateGlassPing)), 0.5f,
                "at the float's top the float box reaches the rule's standing height");
            yield return Save($"size_{style}_{size:0.0}");
        }

        // Every Style x Colour, light and dark, after the hero and over it: rendered; the rim (its token, as painted) >= 4.5:1 against what lies
        // round the pin on the render (the pin and its shadow hidden: the ring 2-3 px out from its silhouette), the label's words >= 7:1 on the
        // render round their letters, Glass + Ping's disc drawn round the pin
        [UnityTest]
        public IEnumerator EveryStyleAndColour_Renders_TheRim4Point5To1_TheLabel7To1_LightAndDark([Values("light", "dark")] string mode)
        {
            var report = new List<string>();
            foreach (string style in CardOptions.LocateStyles)
                foreach (string colour in CardOptions.LocateColours)
                {
                    yield return Open(c => { c.locate.style = style; c.locate.colour = colour; });
                    ThemeController.SetMode(mode);
                    yield return CardTestInput.Settle(0.3f);
                    foreach (bool overHero in new[] { false, true })
                    {
                        if (!overHero)
                        {
                            // - "on the paper": a scroll where nothing of the content lies under the floating pin (it floats over whatever
                            //   scrolls by -- a word in the accent under it is the content's, not the paper the rule speaks of)
                            yield return ScrollToBarePaper();
                        }
                        else
                        {
                            Scroll.scrollOffset = Vector2.zero;
                            yield return CardTestInput.Settle(0.4f);
                            yield return CardTestInput.UntilStill(Sheet);
                        }
                        CardTestInput.HoldSectionsStill(Sheet);
                        string place = overHero ? "hero" : "paper";
                        Assert.IsTrue(Fab.IsShown, $"precondition: shown ({style} {colour} {mode} {place})");
                        AreSame(Theme(LocateFabRule.Tokens(colour).Rim), Fab.Pin.Rim, $"{colour}: the rim painted from its token");
                        Texture2D drawn = null, under = null, bare = null;
                        try
                        {
                            yield return ThemePixelSet.GrabStill($"locate_{style}_{colour}_{mode}_{place}", null, r => drawn = r);
                            var inline = Fab.Chip.style.color;
                            Fab.Chip.style.color = Color.clear;
                            yield return null;
                            yield return ThemePixelSet.GrabStill("locate_under", null, r => under = r);
                            Fab.Chip.style.color = inline;
                            Fab.Float.style.visibility = Visibility.Hidden;
                            Fab.Shadow.style.visibility = Visibility.Hidden;
                            yield return null;
                            yield return ThemePixelSet.GrabStill("locate_bare", null, r => bare = r);
                            var pinArea = ThemePixelSet.ScreenRectOf(Fab.Pin, drawn);
                            var (ground, groundPixels) = WorstRound(drawn, bare, pinArea, Fab.Pin.Rim);
                            Assert.Greater(groundPixels, 20, "precondition: ground pixels round the pin");
                            float rim = UIAccessibility.ContrastRatio(Fab.Pin.Rim, ground);
                            string line = $"{mode} {place} {style} {colour}: rim {rim:0.00}:1 on {(Color32)ground}";
                            if (Fab.LabelShown)
                            {
                                Assert.IsTrue(CardTokenColours.TryResolve(Fab.Root, "--ts-locate-chip-ink", out Color ink), "precondition: the chip's ink");
                                var (ratio, worst, inked) = ThemePixelSet.RingContrast(drawn, under, ThemePixelSet.ScreenRectOf(Fab.Chip, drawn), ink);
                                Assert.Greater(inked, 0, "precondition: the label draws its words");
                                line += $", label {ratio:0.00}:1";
                                Assert.GreaterOrEqual(ratio, 7f, $"{mode} {place} {style} {colour}: the label {ratio:0.00}:1 (worst {(Color32)worst})");
                            }
                            report.Add(line);
                            // - on the paper the rim holds 4.5:1; over the hero's picture the ground is the picture: measured and reported
                            //   (the pin's glow and drop shadow part it from a picture, the developer's look decides)
                            if (!overHero) Assert.GreaterOrEqual(rim, 4.5f, line);
                            if (style == CardOptions.LocateGlassPing)
                                Assert.Greater(Fab.Glass.worldBound.width, Fab.Pin.worldBound.width, "Glass + Ping: the disc round the pin");
                            System.IO.File.WriteAllBytes(TestEvidence.PathFor(Domain, $"locate_{style}_{colour}_{mode}_{place}.png"), drawn.EncodeToPNG());
                        }
                        finally
                        {
                            Fab.Float.style.visibility = StyleKeyword.Null;
                            Fab.Shadow.style.visibility = StyleKeyword.Null;
                            if (drawn != null) Object.Destroy(drawn);
                            if (under != null) Object.Destroy(under);
                            if (bare != null) Object.Destroy(bare);
                        }
                    }
                }
            System.IO.File.WriteAllText(TestEvidence.PathFor(Domain, $"locate_ratios_{mode}.txt"), string.Join("\n", report) + "\n");
        }

        private static IEnumerable<string> HeroPictures() => new[] { "lamp", "painting", "lamp_military", "lamp_religious" };

        // On every hero picture of the wall, light / dark, Half / Full (the pin on the picture, its halo on): the pin is a non-text control, so
        // its outline reads >= 3:1 against the picture beside it (WCAG 1.4.11). Measured on the render along rays round the pin's head (the
        // tip's side and the chip left out): the darkest drawn pixel in the band at the body's edge (the dark halo) or the white inner disc,
        // against the picture just past the glow on the render with the pin hidden -- one of the two >= 3:1 on EVERY ray. On the paper the halo
        // is off (TestEvidence/Premium/P3e/locate_hero_ratios_<poi>_<mode>.txt)
        [UnityTest]
        public IEnumerator OnEveryHeroPicture_ThePinsEdgeOrItsDisc_Reads3To1_TheHaloOffOnThePaper(
            [ValueSource(nameof(HeroPictures))] string poi, [Values("light", "dark")] string mode)
        {
            var report = new List<string>();
            // - measured still: Reduce Motion rests a looping video hero on its poster and the turntable (and the pin; its halo is drawn all
            //   the same); opened ONCE, only the stop changes after (a re-applied card rebinds and reloads its 3D preview)
            yield return Open(c => c.container.reduce_motion = true, SheetStopRule.Stop.Half, poi);
            ThemeController.SetMode(mode);
            foreach (var stop in new[] { SheetStopRule.Stop.Half, SheetStopRule.Stop.Full })
            {
                Scroll.scrollOffset = Vector2.zero;
                Sheet.SetStop(stop);
                yield return CardTestInput.Settle();
                yield return CardTestInput.UntilStill(Sheet);
                CardTestInput.HoldSectionsStill(Sheet);
                Assert.IsTrue(Fab.IsShown, $"precondition: shown ({poi} {mode} {stop})");
                Assert.IsTrue(Stack.HeaderView.HasPicture, "precondition: " + poi + "'s hero is a picture");
                Assert.IsTrue(Fab.Pin.OverPicture, $"{poi} {mode} {stop}: the pin on the hero's picture wears its halo");
                Texture2D drawn = null, bare = null;
                try
                {
                    // - still where it is measured (the pin and the picture round it): the header's turntable keeps turning above it
                    yield return GrabStillAround($"locate_hero_{poi}_{mode}_{stop}", r => drawn = r);
                    Fab.Float.style.visibility = Visibility.Hidden;
                    Fab.Shadow.style.visibility = Visibility.Hidden;
                    yield return null;
                    yield return GrabStillAround("locate_hero_bare", r => bare = r);
                    var (worst, rays) = WorstEdgeOrDisc(drawn, bare);
                    Assert.Greater(rays, 20, "precondition: rays round the pin's head on the picture");
                    string line = $"{poi} {mode} {stop}: edge or disc >= {worst:0.00}:1 on every one of {rays} rays";
                    report.Add(line);
                    Assert.GreaterOrEqual(worst, 3f, line);
                    System.IO.File.WriteAllBytes(TestEvidence.PathFor(Domain, $"locate_hero_{poi}_{mode}_{stop}.png"), drawn.EncodeToPNG());
                }
                finally
                {
                    Fab.Float.style.visibility = StyleKeyword.Null;
                    Fab.Shadow.style.visibility = StyleKeyword.Null;
                    if (drawn != null) Object.Destroy(drawn);
                    if (bare != null) Object.Destroy(bare);
                }
            }
            // - scrolled past the hero (a card long enough to leave it), the pin on the paper: no halo
            if (Scroll.verticalScroller.highValue > Stack.HeaderView.HeroBottom() + 40f)
            {
                yield return ScrollToBarePaper();
                Assert.IsFalse(Fab.Pin.OverPicture, poi + ": past the hero the pin is on the paper: no halo");
            }
            System.IO.File.WriteAllText(TestEvidence.PathFor(Domain, $"locate_hero_ratios_{poi}_{mode}.txt"), string.Join("\n", report) + "\n");
        }

        // The whole frame, once the pixels within MeasuredReach of the pin have stood the same for StillFrames frames (ThemePixelSet.GrabStill
        // asks it of the whole screen: a turning header model never lets that happen, and it lies away from what is measured here)
        private IEnumerator GrabStillAround(string name, System.Action<Texture2D> done)
        {
            const int StillFrames = 10, Attempts = 30;
            // - the rays' farthest picture point (head + glow + 3) and some room: 32 pt round the pin's box
            const float MeasuredReach = 32f;
            Texture2D render = null;
            for (int attempt = 0; attempt < Attempts; attempt++)
            {
                if (attempt > 0) for (int f = 0; f < StillFrames; f++) yield return null;
                yield return new WaitForEndOfFrame();
                var next = ScreenCapture.CaptureScreenshotAsTexture();
                var pin = Fab.Pin.worldBound;
                var area = new Rect(pin.x - MeasuredReach, pin.y - MeasuredReach, pin.width + 2f * MeasuredReach, pin.height + 2f * MeasuredReach);
                bool still = render != null && SameIn(render, next, area);
                if (render != null) Object.Destroy(render);
                render = next;
                if (still) break;
                Assert.Less(attempt, Attempts - 1, name + ": the picture round the pin never held still for " + StillFrames + " frames");
            }
            done(render);
        }

        // Whether two renders hold the same pixels inside `area` (panel space)
        private bool SameIn(Texture2D a, Texture2D b, Rect area)
        {
            var origin = RuntimePanelUtils.ScreenToPanel(Fab.Pin.panel, Vector2.zero);
            float unit = OnePixel;
            int x0 = Mathf.Clamp(Mathf.FloorToInt((area.xMin - origin.x) / unit), 0, a.width), x1 = Mathf.Clamp(Mathf.CeilToInt((area.xMax - origin.x) / unit), 0, a.width);
            int yTop = Mathf.Clamp(Mathf.FloorToInt((area.yMin - origin.y) / unit), 0, a.height), yBottom = Mathf.Clamp(Mathf.CeilToInt((area.yMax - origin.y) / unit), 0, a.height);
            Color32[] pa = a.GetPixels32(), pb = b.GetPixels32();
            for (int y = a.height - yBottom; y < a.height - yTop; y++)
                for (int x = x0; x < x1; x++)
                {
                    int i = y * a.width + x;
                    if (pa[i].r != pb[i].r || pa[i].g != pb[i].g || pa[i].b != pb[i].b) return false;
                }
            return true;
        }

        // The worst ray round the pin's head: per ray, the better of (the darkest drawn pixel in the band at the body's edge -- the study's head
        // circle, centre (24, 23.5) of 64, radius 20.5 -- against the picture past the glow) and (the white inner disc at 0.8 of its radius
        // against the same picture). Rays pointing down to the tip and rays whose picture point lies on the chip are left out
        private (float Worst, int Rays) WorstEdgeOrDisc(Texture2D drawn, Texture2D bare)
        {
            var panel = Fab.Pin.panel;
            var origin = RuntimePanelUtils.ScreenToPanel(panel, Vector2.zero);
            float unit = OnePixel;
            Color At(Texture2D t, Vector2 p) => t.GetPixel(Mathf.RoundToInt((p.x - origin.x) / unit), t.height - 1 - Mathf.RoundToInt((p.y - origin.y) / unit));
            var pin = Fab.Pin.worldBound;
            var chip = Fab.LabelShown ? Fab.Chip.worldBound : Rect.zero;
            float s = pin.height / 64f, head = 20.5f * s, disc = 11.5f * s;
            Vector2 centre = pin.min + new Vector2(24f, 23.5f) * s, discCentre = pin.min + new Vector2(24f, 23f) * s;
            float worst = float.MaxValue;
            int rays = 0;
            for (int a = 0; a < 360; a += 5)
            {
                var d = new Vector2(Mathf.Cos(a * Mathf.Deg2Rad), Mathf.Sin(a * Mathf.Deg2Rad));
                // - not towards the tip (the body narrows there: the head circle is not its edge)
                if (d.y > 0.55f) continue;
                var beyond = centre + d * (head + LocatePin.GlowPoints + 3f);
                if (chip.Contains(beyond) || beyond.x >= Sheet.Root.worldBound.xMax - unit) continue;
                rays++;
                Color picture = At(bare, beyond);
                float edge = 1f;
                for (float r = head - 2f; r <= head + 4f; r += unit * 0.5f)
                {
                    Color seen = At(drawn, centre + d * r);
                    // - the halo is the DARK side of the edge: only a pixel darker than the picture counts as it
                    if (seen.grayscale < picture.grayscale)
                        edge = Mathf.Max(edge, UIAccessibility.ContrastRatio(seen, picture));
                }
                float inner = UIAccessibility.ContrastRatio(At(drawn, discCentre + d * (disc * 0.8f)), picture);
                worst = Mathf.Min(worst, Mathf.Max(edge, inner));
            }
            return (worst, rays);
        }

        // Scroll past the hero to the first place where no shown text, picture or icon of the stack lies within 8 pt of the button's touch
        // target (from layout, every 24 pt of scroll): the pin then stands on the paper alone
        private IEnumerator ScrollToBarePaper()
        {
            float range = Scroll.verticalScroller.highValue;
            for (float y = Stack.HeaderView.HeroBottom() + 40f; y < range; y += 24f)
            {
                Scroll.scrollOffset = new Vector2(0f, y);
                yield return null;
                yield return null;
                var zone = Fab.Root.worldBound;
                zone = new Rect(zone.x - 8f, zone.y - 8f, zone.width + 16f, zone.height + 16f);
                bool bare = !Scroll.contentContainer.Query<VisualElement>().ToList().Any(e => e.childCount == 0 && e.worldBound.Overlaps(zone)
                    && CardTestInput.IsShown(e, Sheet.Root) && e.resolvedStyle.visibility == Visibility.Visible
                    && (e is TextElement t && !string.IsNullOrWhiteSpace(t.text) || e.resolvedStyle.backgroundImage != default(Background)
                        || e is CardIcons.VectorGlyph));
                if (!bare) continue;
                yield return CardTestInput.UntilStill(Sheet);
                yield break;
            }
            Assert.Fail("precondition: a scroll where the pin stands on bare paper");
        }

        // The worst ground (nearest in contrast to `rim`) in the ring 2-3 px out from the pin's silhouette, read on `bare` (the pin and its
        // shadow hidden); the silhouette: what changed between `drawn` and `bare` inside `area`
        private static (Color Worst, int Count) WorstRound(Texture2D drawn, Texture2D bare, RectInt area, Color rim)
        {
            Color32[] a = drawn.GetPixels32(), b = bare.GetPixels32();
            int pad = 12, w = area.width + 2 * pad, h = area.height + 2 * pad, x0 = area.x - pad, y0 = area.y - pad;
            var pin = new bool[w * h];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    int sx = x0 + x, sy = y0 + y;
                    if (sx < 0 || sy < 0 || sx >= drawn.width || sy >= drawn.height) continue;
                    int i = sy * drawn.width + sx;
                    pin[y * w + x] = Mathf.Abs(a[i].r - b[i].r) + Mathf.Abs(a[i].g - b[i].g) + Mathf.Abs(a[i].b - b[i].b) > 24;
                }
            bool Near(int x, int y, int reach)
            {
                for (int dy = -reach; dy <= reach; dy++)
                    for (int dx = -reach; dx <= reach; dx++)
                    {
                        int nx = x + dx, ny = y + dy;
                        if (nx >= 0 && ny >= 0 && nx < w && ny < h && pin[ny * w + nx]) return true;
                    }
                return false;
            }
            Color worst = Color.clear;
            float worstRatio = float.MaxValue;
            int count = 0;
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    int sx = x0 + x, sy = y0 + y;
                    if (sx < 0 || sy < 0 || sx >= drawn.width || sy >= drawn.height || pin[y * w + x] || Near(x, y, 1) || !Near(x, y, 3)) continue;
                    Color g = b[sy * drawn.width + sx];
                    count++;
                    float r = UIAccessibility.ContrastRatio(rim, g);
                    if (r < worstRatio) { worstRatio = r; worst = g; }
                }
            return (worst, count);
        }

        // It follows the bar: at Top it rests 12 over the sheet's foot (no bar under it), never on the bar; the hero's words (lower with no
        // bar under them) never give way to it -- no row narrowed -- and its chip shows only clear of their ink
        [Category("CardSmoke")]
        [UnityTest]
        public IEnumerator WithTheBarAtTop_ItRests12OverTheFoot_ClearOfTheBarAndTheWords()
        {
            yield return Open(c => c.navigation.bar.position = CardOptions.NavBarTop, SheetStopRule.Stop.Half);
            Assert.AreEqual(NavBarView.ElementKey, Sheet.Slots.TopKey, "precondition: the bar at Top");
            Assert.IsTrue(Fab.IsShown, "shown");
            Assert.AreEqual(Sheet.Root.worldBound.yMax - 12f, Fab.Root.worldBound.yMax, OnePixel, "12 over the sheet's foot");
            foreach (var r in FabRects())
                foreach (var item in Bar.Items) Assert.IsFalse(Meets(r, item.Button.worldBound), "never on the bar's items");
            foreach (var row in new[] { Stack.HeaderView.Text, Stack.HeaderView.Actions })
                Assert.AreEqual(StyleKeyword.Null, row.style.paddingRight.keyword, row.name + ": no inline room taken from the words for the button");
            if (Fab.LabelShown)
            {
                var ink = new List<Ink>();
                InkRects.Collect(Stack.HeaderView.Foot, Sheet.Root, ink);
                var chip = Fab.Chip.worldBound;
                var chipInSheet = new Rect(chip.x - Sheet.Root.worldBound.x, chip.y - Sheet.Root.worldBound.y, chip.width, chip.height);
                foreach (var i in ink) Assert.IsFalse(i.Rect.Overlaps(chipInSheet), "the chip shown only clear of the hero's ink " + i.Rect);
            }
            yield return Save("top_half");
        }

        // Hide While Scrolling: REAL strokes down the card slide the bar away and the button with it (no finger on it then); a stroke back up
        // brings both again
        [UnityTest]
        public IEnumerator HideWhileScrolling_RealStrokesDown_SlideItAwayWithTheBar_AndUpBringItBack()
        {
            yield return Open(c => c.navigation.bar.hide_on_scroll = CardOptions.NavBarHideDown);
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
            Assert.IsFalse(Fab.Moving, "...its motion stopped while away");
            var back = CardTestInput.ReachableStack(Sheet);
            yield return CardTestInput.TouchDrag(Scroll.panel, new Vector2(back.center.x, back.center.y), new Vector2(0f, 40f));
            yield return CardTestInput.UntilStill(Sheet);
            yield return CardTestInput.Settle(0.4f);
            Assert.IsFalse(Sheet.BottomArea.ClassListContains(CardChromeView.BottomHiddenClass), "precondition: a move up brought the bar back");
            Assert.IsTrue(Fab.IsShown, "...and the button");
            Assert.AreEqual(BarTop - 12f, Fab.Root.worldBound.yMax, OnePixel, "...at its place over the bar");
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
            yield return ThemePixelSet.GrabStill("locate_" + name, null, r => shot = r);
            if (shot == null) yield break;
            System.IO.File.WriteAllBytes(TestEvidence.PathFor(Domain, "locate_" + name + ".png"), shot.EncodeToPNG());
            Object.Destroy(shot);
        }
    }
}

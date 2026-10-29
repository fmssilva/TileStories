using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // Tier 0 of the Locate Button (_3.3 P2 review e, P3d): LocateFabRule -- shown at Half and Full only while the bar and its Locate Button
    // are on and the scroll is not hiding the bar; its bottom over the bar's highest point (the raised disc's top) or a sticky footer or the
    // foot, at the card's numbers; the touch target holding everything that floats in it at every size -- and NavBarRule's Locate Button / Sticky Action defaults and matrix, an old config,
    // the touch target and the mark's 4.5:1 on the accent from the theme's own values, light and dark
    public class LocateFabRuleTests
    {
        private static CardNavigationSettings Nav(System.Action<CardNavBarSettings> edit = null)
        {
            var nav = new CardNavigationSettings { bar = new CardNavBarSettings() };
            edit?.Invoke(nav.bar);
            return nav;
        }

        // A plain-number token of the card's own tokens (CardTokens.uss), read from the file the card reads
        private static float CardToken(string name)
        {
            string uss = System.IO.File.ReadAllText("Assets/Framework/Runtime/UI/Screen/Cards/CardTokens.uss");
            var m = Regex.Match(uss, Regex.Escape(name) + @":\s*([0-9.]+)\s*;");
            Assert.IsTrue(m.Success, "precondition: CardTokens.uss defines " + name);
            return float.Parse(m.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture);
        }

        [Test]
        public void ItShows_AtHalfAndFull_WhileTheBarAndItsLocateButtonAreOn_AndNotWhileTheScrollHidesTheBar()
        {
            var nav = Nav();
            Assert.IsTrue(NavBarRule.LocateButton(nav.bar), "on by default");
            Assert.IsFalse(LocateFabRule.Shown(nav, SheetStopRule.Stop.Peek, false), "Peek: the wall is already in view");
            Assert.IsTrue(LocateFabRule.Shown(nav, SheetStopRule.Stop.Half, false), "Half");
            Assert.IsTrue(LocateFabRule.Shown(nav, SheetStopRule.Stop.Full, false), "Full");
            Assert.IsFalse(LocateFabRule.Shown(nav, SheetStopRule.Stop.Dismissed, false), "no card");
            Assert.IsFalse(LocateFabRule.Shown(nav, SheetStopRule.Stop.Full, true), "Hide While Scrolling slid the bar away: it goes with it");
            Assert.IsFalse(LocateFabRule.Shown(Nav(b => b.locate_button = false), SheetStopRule.Stop.Full, false), "Locate Button off");
            Assert.IsFalse(LocateFabRule.Shown(Nav(b => b.enabled = false), SheetStopRule.Stop.Full, false), "the bar off: its rows (this one) with it");
            Assert.IsFalse(LocateFabRule.Shown(null, SheetStopRule.Stop.Full, false), "no navigation settings");
            Assert.IsTrue(LocateFabRule.Shown(Nav(b => b.position = CardOptions.NavBarTop), SheetStopRule.Stop.Half, false), "the bar at Top: still there");
            // - a config written before P3d has no locate_button: it loads on
            var old = JsonUtility.FromJson<CardNavBarSettings>("{\"position\":\"bottom\"}");
            Assert.IsTrue(NavBarRule.LocateButton(old), "an old config: on");
            Assert.AreEqual(CardOptions.NavBarStickyAuto, NavBarRule.StickyAction(old), "...Sticky Action Automatic");
        }

        // Its bottom at the card's numbers: 12 over the raised disc's top (gap 20 + bar 62 + the disc's radius 20 = 102 above the foot), over
        // the band alone without Raised (82), over a sticky footer, or 12 over the foot
        [Test]
        public void ItRestsAGapOverTheBarsHighestPoint_OrAFooter_OrTheFoot_AtTheCardsNumbers()
        {
            float gap = CardToken("--ts-fab-gap"), bar = CardToken("--ts-navbar-height"), under = CardToken("--ts-navbar-gap");
            float disc = CardToken("--ts-navbar-disc-raised");
            Assert.AreEqual(12f, gap, "12 pt above (the brief)");
            float raised = under + bar + disc * 0.5f;
            Assert.AreEqual(raised + gap, LocateFabRule.Bottom(raised, 0f, gap), 1e-4f, "Raised: over the disc's top, whichever group is chosen");
            Assert.AreEqual(under + bar + gap, LocateFabRule.Bottom(under + bar, 0f, gap), 1e-4f, "Pill / Underline: over the band");
            Assert.AreEqual(56f + gap, LocateFabRule.Bottom(float.NaN, 56f, gap), 1e-4f, "no bottom bar, a sticky footer shown: over the footer");
            Assert.AreEqual(gap, LocateFabRule.Bottom(float.NaN, 0f, gap), 1e-4f, "nothing under it: over the foot");
            Assert.AreEqual(56f + raised + gap, LocateFabRule.Bottom(56f + raised, 56f, gap), 1e-4f, "a bar over a footer: over the bar");
            // - the default pin (the chosen group's 40 pt disc) in its touch target: its shadow (40 x 0.75 x 0.8 x 0.28 = 6.72), the lift
            //   (1.2), the pin and its 7 pt float = 54.92
            float touch = LocateFabRule.Touch(LocateFabRule.PinHeight(disc, new CardLocateSettings()), false);
            Assert.AreEqual(6.72f + 1.2f + disc + LocateFabRule.Bob, touch, 1e-3f, "the default pin's touch target");
        }

        // Every option read with its default: an old config (no locate block), an unknown value, Size clamped to 0.6..2; the Editor's words
        [Test]
        public void ItsOptions_ReadWithTheirDefaults_AnOldConfig_AnUnknownValue_SizeClamped()
        {
            var old = JsonUtility.FromJson<CardSettings>("{\"enabled\":true}");
            Assert.IsNotNull(old.locate, "an old config gets the defaults");
            Assert.AreEqual(CardOptions.LocatePinLabel, LocateFabRule.Style(old.locate), "Style: Pin + Label");
            Assert.AreEqual(CardOptions.LocateTeal, LocateFabRule.Colour(old.locate), "Colour: AR Teal");
            Assert.AreEqual(1f, CardLocateSettings.SizeDefault, "the developer's: the pin as big as the chosen group's disc");
            Assert.AreEqual(CardLocateSettings.SizeDefault, LocateFabRule.Size(old.locate), "Size: the default");
            Assert.IsTrue(LocateFabRule.Floats(old.locate, false), "Float on");
            Assert.IsFalse(LocateFabRule.Floats(old.locate, true), "Reduce Motion: still");
            Assert.IsFalse(LocateFabRule.Floats(new CardLocateSettings { float_motion = false }, false), "Float off: still");
            var odd = new CardLocateSettings { style = "neon", colour = "pink", size = 7f };
            Assert.AreEqual(CardOptions.LocatePinLabel, LocateFabRule.Style(odd), "unknown style = the default");
            Assert.AreEqual(CardOptions.LocateTeal, LocateFabRule.Colour(odd), "unknown colour = the default");
            Assert.AreEqual(2f, CardLocateSettings.SizeMax, "up to twice the chosen disc");
            Assert.AreEqual(0.6f, CardLocateSettings.SizeMin, "down to 0.6 of it (24 of the disc's 40)");
            Assert.AreEqual(CardLocateSettings.SizeMax, LocateFabRule.Size(odd), "Size clamped to its top");
            Assert.AreEqual(CardLocateSettings.SizeMin, LocateFabRule.Size(new CardLocateSettings { size = 0.2f }), "...and to its foot");
            Assert.AreEqual(CardLocateSettings.SizeDefault, LocateFabRule.Size(new CardLocateSettings { size = float.NaN }), "a NaN: the default");
            CollectionAssert.AreEqual(new[] { "Floating Pin", "Pin + Label", "Glass + Ping" }, CardOptions.LocateStyleLabels);
            CollectionAssert.AreEqual(new[] { "AR Teal", "Fresh Green", "Cyan", "Accent" }, CardOptions.LocateColourLabels);
            // - the label: only Pin + Label, only until the first scroll
            Assert.IsTrue(LocateFabRule.LabelShown(old.locate, false));
            Assert.IsFalse(LocateFabRule.LabelShown(old.locate, true), "folded after the visitor's first scroll");
            Assert.IsFalse(LocateFabRule.LabelShown(new CardLocateSettings { style = CardOptions.LocatePin }, false), "Floating Pin: no words");
            Assert.IsFalse(LocateFabRule.LabelShown(new CardLocateSettings { style = CardOptions.LocateGlassPing }, false), "Glass + Ping: no words");
            Assert.IsTrue(LocateFabRule.HasGlass(new CardLocateSettings { style = CardOptions.LocateGlassPing }));
        }

        // The chip's room: shown only where no word's ink lies within the gap of it (the words never give way to the button)
        [Test]
        public void TheLabel_FitsOnlyClearOfTheHerosWords()
        {
            var chip = new Rect(200f, 700f, 138f, 32f);
            Assert.IsTrue(LocateFabRule.LabelFits(chip, new Rect[0], 12f), "no words: room");
            Assert.IsTrue(LocateFabRule.LabelFits(chip, new[] { new Rect(40f, 705f, 40f, 16f) }, 12f), "a short row far to its left: room");
            Assert.IsTrue(LocateFabRule.LabelFits(chip, new[] { new Rect(40f, 660f, 140f, 20f) }, 12f), "a word just past the gap: room");
            Assert.IsFalse(LocateFabRule.LabelFits(chip, new[] { new Rect(40f, 690f, 170f, 16f) }, 12f), "a word reaching under the chip: no room");
            Assert.IsFalse(LocateFabRule.LabelFits(chip, new[] { new Rect(40f, 670f, 200f, 20f) }, 12f), "a word within the gap above: no room");
        }

        // Over a picture (its halo on): any part of the pin on the hero's picture as drawn -- the whole hero at Full, its foot just reaching
        // the pin's top, the hero scrolled away above it, no hero
        [Test]
        public void OverPicture_WhileAnyPartOfThePinLiesOnTheHerosPicture()
        {
            var pin = new Rect(332f, 682f, 30f, 40f);
            Assert.IsTrue(LocateFabRule.OverPicture(pin, new Rect(0f, 0f, 390f, 844f)), "the hero the whole screen: on it");
            Assert.IsTrue(LocateFabRule.OverPicture(pin, new Rect(0f, -150f, 390f, 840f)), "the hero's foot 8 pt into the pin's top: on it");
            Assert.IsFalse(LocateFabRule.OverPicture(pin, new Rect(0f, -1043f, 390f, 844f)), "the hero scrolled away above it: on the paper");
            Assert.IsFalse(LocateFabRule.OverPicture(pin, new Rect(0f, -162f, 390f, 844f)), "the hero's foot exactly at the pin's top: on the paper");
            Assert.IsFalse(LocateFabRule.OverPicture(pin, Rect.zero), "no hero picture: on the paper");
        }

        // Its sizes: the pin's height is the chosen group's disc times Size (0.6..2), the touch target never under 44 pt whatever the size and
        // holding everything that floats in it at the float's top (shadow, lift, the pin or its 1.5x glass disc, the Bob), as wide as it too
        [Test]
        public void ItsSizes_FollowTheChosenDisc_TheTouchTargetNeverUnder44()
        {
            float icon = CardToken("--ts-navbar-disc-raised");
            Assert.AreEqual(40f, icon, "precondition: the chosen group's 40 pt disc");
            foreach (float s in new[] { CardLocateSettings.SizeMin, 1f, CardLocateSettings.SizeMax })
                foreach (bool glass in new[] { false, true })
                {
                    var locate = new CardLocateSettings { size = s, style = glass ? CardOptions.LocateGlassPing : CardOptions.LocatePin };
                    float pin = LocateFabRule.PinHeight(icon, locate);
                    Assert.AreEqual(icon * s, pin, 1e-4f, "the pin's height at " + s + "x");
                    float touch = LocateFabRule.Touch(pin, glass);
                    Assert.GreaterOrEqual(touch, 44f, s + "x " + (glass ? "glass" : "pin") + ": a whole touch target");
                    float box = glass ? pin * LocateFabRule.GlassToPin : pin;
                    Assert.AreEqual(box, LocateFabRule.FloatBox(pin, glass), 1e-4f, "what floats: the pin or its glass disc");
                    float standing = LocateFabRule.ShadowHeight(pin, glass) + pin * 0.03f + box + LocateFabRule.Bob;
                    Assert.AreEqual(standing, LocateFabRule.Standing(pin, glass), 1e-4f, "the height standing in the target at the float's top");
                    // - a float epsilon: the sum here adds the same parts in another order than the rule
                    Assert.GreaterOrEqual(touch + 1e-4f, standing, s + "x: the float box at its top inside the target (" + standing + " in " + touch + ")");
                    Assert.GreaterOrEqual(touch, box, "...and as wide as it");
                    Assert.Less(LocateFabRule.ShadowWidth(pin, glass), box, "the shadow narrower than what floats over it");
                }
        }

        // The motion's shapes: the float an eased sine (0 at rest, the whole 7 pt half way through its 2.6 s, back at 2.6), the shadow
        // shrinking and fading as the pin rises, the ping growing and fading over 60 % of its 3.2 s then nothing; still = 0 / no ring
        [Test]
        public void TheFloatTheShadowAndThePing_FollowTheStudysShapes_AndStandStillWithoutMotion()
        {
            Assert.AreEqual(7f, LocateFabRule.Bob);
            Assert.AreEqual(2.6f, LocateFabRule.BobPeriod);
            Assert.AreEqual(0f, LocateFabRule.Rise(0f, true), 1e-5f, "at rest at the start");
            Assert.AreEqual(1f, LocateFabRule.Rise(1.3f, true), 1e-5f, "the top half way");
            Assert.AreEqual(0f, LocateFabRule.Rise(2.6f, true), 1e-4f, "back down: one period");
            Assert.AreEqual(0.5f, LocateFabRule.Rise(0.65f, true), 1e-4f, "eased: half up at a quarter");
            float last = -1f;
            for (float t = 0f; t <= 1.3f; t += 0.1f)
            {
                float r = LocateFabRule.Rise(t, true);
                Assert.GreaterOrEqual(r, last - 1e-6f, "rising through the first half (" + t + " s)");
                last = r;
            }
            Assert.AreEqual(0f, LocateFabRule.Rise(1.3f, false), "still: never rises");
            Assert.AreEqual(1f, LocateFabRule.ShadowScale(0f));
            Assert.AreEqual(0.7f, LocateFabRule.ShadowScale(1f), 1e-5f, "the shadow shrinks as the pin rises");
            Assert.AreEqual(0.9f, LocateFabRule.ShadowAlpha(0f), 1e-5f);
            Assert.AreEqual(0.5f, LocateFabRule.ShadowAlpha(1f), 1e-5f, "...and fades");
            var (s0, a0) = LocateFabRule.Ping(0f, true);
            Assert.AreEqual(1f, s0, 1e-5f, "a ring starts at the disc");
            Assert.AreEqual(0.8f, a0, 1e-5f);
            var (sEnd, aEnd) = LocateFabRule.Ping(3.2f * 0.6f - 1e-4f, true);
            Assert.AreEqual(1.7f, sEnd, 0.01f, "it grows to 1.7x");
            Assert.AreEqual(0f, aEnd, 0.01f, "...fading out");
            Assert.AreEqual(0f, LocateFabRule.Ping(2.5f, true).Alpha, "then nothing until the next");
            Assert.AreEqual(0.8f, LocateFabRule.Ping(3.2f, true).Alpha, 1e-4f, "the next ring every 3.2 s");
            Assert.AreEqual(0f, LocateFabRule.Ping(0.5f, false).Alpha, "still: no ring");
        }

        // Sticky Action: Automatic leaves the old sticky action out only while the bar floats at the bottom; Shown / Hidden as picked; with the
        // bar off it is drawn as before the bar; an unknown value reads Automatic
        [Test]
        public void TheStickyAction_IsOffUnderABottomBar_ByDefault_KeptAsAnOption()
        {
            Assert.IsFalse(NavBarRule.StickyActionShown(Nav()), "the default (bar at the Bottom, Automatic): off");
            Assert.IsTrue(NavBarRule.StickyActionShown(Nav(b => b.position = CardOptions.NavBarTop)), "the bar at Top, Automatic: drawn");
            Assert.IsTrue(NavBarRule.StickyActionShown(Nav(b => b.sticky_action = CardOptions.NavBarStickyShown)), "Shown under a bottom bar");
            Assert.IsFalse(NavBarRule.StickyActionShown(Nav(b => { b.position = CardOptions.NavBarTop; b.sticky_action = CardOptions.NavBarStickyHidden; })), "Hidden at Top");
            Assert.IsTrue(NavBarRule.StickyActionShown(Nav(b => { b.enabled = false; b.sticky_action = CardOptions.NavBarStickyHidden; })), "the bar off: drawn as before");
            Assert.IsTrue(NavBarRule.StickyActionShown(null), "no navigation settings: drawn");
            Assert.AreEqual(CardOptions.NavBarStickyAuto, NavBarRule.StickyAction(new CardNavBarSettings { sticky_action = "sometimes" }), "unknown = Automatic");
            CollectionAssert.AreEqual(new[] { "Automatic", "Shown", "Hidden" }, CardOptions.NavBarStickyActionLabels);
        }

        // Every Colour's tokens exist in both modes; its rim >= 4.5:1 on the paper; the chip's words >= 7:1 on the chip laid over black and
        // over white (its light ground is a little see-through), read from the theme's own values
        [Test]
        public void EveryColoursRim_Reads4Point5To1OnThePaper_AndTheChipsWords7To1_LightAndDark()
        {
            var theme = UnityEditor.AssetDatabase.LoadAssetAtPath<ThemeAsset>("Assets/Framework/Runtime/Resources/TileStories/Themes/Theme.default.asset");
            Assert.IsNotNull(theme, "precondition: the framework theme");
            foreach (string mode in CardOptions.Modes)
            {
                var r = theme.ResolverFor(mode);
                Assert.IsTrue(r.TryColour("--ts-sheet-open", out Color paper), mode + ": the paper");
                foreach (string colour in CardOptions.LocateColours)
                {
                    var (hi, lo, rim) = LocateFabRule.Tokens(colour);
                    Assert.IsTrue(r.TryColour(hi, out _), mode + " " + colour + ": " + hi);
                    Assert.IsTrue(r.TryColour(lo, out _), mode + " " + colour + ": " + lo);
                    Assert.IsTrue(r.TryColour(rim, out Color rimColour), mode + " " + colour + ": " + rim);
                    float ratio = UIAccessibility.ContrastRatio(rimColour, paper);
                    Assert.GreaterOrEqual(ratio, 4.5f, mode + " " + colour + ": the rim on the paper " + ratio.ToString("0.00") + ":1");
                }
                Assert.IsTrue(r.TryColour("--ts-locate-chip", out Color chip), mode + ": the chip");
                Assert.IsTrue(r.TryColour("--ts-locate-chip-ink", out Color words), mode + ": its words");
                foreach (var under in new[] { Color.black, Color.white })
                {
                    var ground = Color.Lerp(under, new Color(chip.r, chip.g, chip.b, 1f), chip.a);
                    float ratio = UIAccessibility.ContrastRatio(words, ground);
                    Assert.GreaterOrEqual(ratio, 7f, mode + ": the chip's words over " + under + " " + ratio.ToString("0.00") + ":1");
                }
                foreach (string token in new[] { "--ts-locate-disc", "--ts-locate-disc-edge", "--ts-locate-shadow", "--ts-locate-glass", "--ts-locate-glass-line" })
                    Assert.IsTrue(r.TryColour(token, out _), mode + ": " + token);
            }
        }
    }
}

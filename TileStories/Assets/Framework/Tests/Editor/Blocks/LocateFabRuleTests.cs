using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // Tier 0 of the Locate Button (_3.3 P2 review e, P3d): LocateFabRule -- shown at Half and Full only while the bar and its Locate Button
    // are on and the scroll is not hiding the bar; its bottom over the bar's highest point (the raised disc's top) or a sticky footer or the
    // foot, at the card's numbers; the hero words' room -- and NavBarRule's Locate Button / Sticky Action defaults and matrix, an old config,
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
            // - the hero's words leave its column: inset 16 + disc 56 + gap 12 from the hero's right edge
            float size = CardToken("--ts-fab-size"), inset = CardToken("--ts-navbar-inset");
            Assert.AreEqual(56f, size, "the brief's 56 pt disc");
            Assert.GreaterOrEqual(size, 44f, "...a whole touch target (WCAG 2.5.5)");
            Assert.AreEqual(inset + size + gap, LocateFabRule.WordsClear(true, size, inset, gap), 1e-4f);
            Assert.AreEqual(0f, LocateFabRule.WordsClear(false, size, inset, gap), "not shown: the words keep their width");
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

        // The locate mark is the accent's own ink (--ts-on-accent) on the accent disc: >= 4.5:1 in both modes, read from the theme
        [Test]
        public void TheLocateMark_On_TheAccentDisc_Reads4Point5To1_LightAndDark()
        {
            var theme = UnityEditor.AssetDatabase.LoadAssetAtPath<ThemeAsset>("Assets/Framework/Runtime/Resources/TileStories/Themes/Theme.default.asset");
            Assert.IsNotNull(theme, "precondition: the framework theme");
            foreach (string mode in CardOptions.Modes)
            {
                var r = theme.ResolverFor(mode);
                Assert.IsTrue(r.TryColour("--ts-accent", out Color disc), mode + ": the theme has the accent");
                Assert.IsTrue(r.TryColour("--ts-on-accent", out Color ink), mode + ": ...and its ink");
                float ratio = UIAccessibility.ContrastRatio(ink, disc);
                Assert.GreaterOrEqual(ratio, 4.5f, mode + ": the mark on the disc " + ratio.ToString("0.00") + ":1");
            }
        }
    }
}

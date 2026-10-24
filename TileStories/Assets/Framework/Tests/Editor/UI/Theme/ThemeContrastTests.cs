using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace TileStories.Tests
{
    // The measured palettes (_3.3 R12.6, D1): every pair of the theme asset that holds text or meaning, in every mode and under every accent
    // option, by arithmetic -- text >= 7:1 (WCAG AAA) on every surface it sits on, icons / borders / focus >= 4.5:1, body text APCA Lc >= 75.
    // The render half (glass, rim, scrim over a photo) is ThemeFrameSceneTests / PoiCardHeroSceneTests.
    internal static class ThemeContrastAudit
    {
        // The grounds text is drawn on: a block, a button / chip, the app ground, the chrome bar
        public static readonly string[] Surfaces = { "--ts-surface", "--ts-surface-2", "--ts-bg", "--ts-bar" };
        // Every colour a text is written in (the card's sheets: color: var(...)); accent is used as a text colour too
        public static readonly string[] TextRoles = { "--ts-ink", "--ts-ink-2", "--ts-ink-3", "--ts-accent-text", "--ts-accent", "--ts-correct", "--ts-wrong" };
        // The body text roles APCA judges
        public static readonly string[] BodyRoles = { "--ts-ink", "--ts-ink-2" };
        // Marks that carry meaning without words: a meaningful border, the focus ring, an accent outline, the status fallback rings
        public static readonly string[] MarkRoles =
            { "--ts-line", "--ts-focus", "--ts-accent", "--ts-status-0", "--ts-status-1", "--ts-status-2", "--ts-status-3", "--ts-status-4" };
        public static readonly string[] Families = { "meta", "about", "media", "guides", "stories", "ar", "play", "visit", "community", "navigation" };

        // One measured pair: what, its value, the bar it must clear
        public readonly struct Pair
        {
            public readonly string Name; public readonly float Value; public readonly float Bar;
            public Pair(string name, float value, float bar) { Name = name; Value = value; Bar = bar; }
            public bool Passes => Value >= Bar;
            public override string ToString() => $"{Name} {Value:0.00} (needs {Bar:0.#})";
        }

        // Every pair of `mode` (with `accent`'s values laid over it, or none), named "<mode>[/<accent>]: <fg> on <bg> <kind>"
        public static List<Pair> Measure(ThemeAsset theme, string mode, ThemeKnobOption accent = null)
        {
            var colours = new Dictionary<string, Color>();
            foreach (var t in theme.Mode(mode).tokens.colours) colours[t.name] = t.value;
            var over = accent?.modes.Find(m => m.key == mode);
            if (over != null) foreach (var t in over.tokens.colours) colours[t.name] = t.value;
            string tag = mode + (accent != null ? "/" + accent.option : "") + ": ";
            Color C(string name) { Assert.IsTrue(colours.ContainsKey(name), tag + "the theme defines " + name); return colours[name]; }

            var pairs = new List<Pair>();
            foreach (string ink in TextRoles)
                foreach (string ground in Surfaces)
                    pairs.Add(new Pair(tag + ink + " on " + ground, UIAccessibility.ContrastRatio(C(ink), C(ground)), UIAccessibility.MinRatioEnhancedText));
            // - a poll's results bar tints a block: the words over it read on the blend (the project blends in LINEAR light)
            foreach (string ground in new[] { "--ts-surface", "--ts-surface-2" })
                pairs.Add(new Pair(tag + "--ts-ink on --ts-result-fill over " + ground,
                    UIAccessibility.ContrastRatio(C("--ts-ink"), LinearOver(C("--ts-result-fill"), C(ground))), UIAccessibility.MinRatioEnhancedText));
            pairs.Add(new Pair(tag + "--ts-on-accent on --ts-accent", UIAccessibility.ContrastRatio(C("--ts-on-accent"), C("--ts-accent")), UIAccessibility.MinRatioEnhancedText));
            // - the hero's words over its scrim over the WORST pixel any picture can have, pure white (P4.4b part 2: a guarantee for every
            //   picture, not a measure of today's; the render half is PoiCardHeroSceneTests' worst-pixel scan)
            foreach (string words in new[] { "--ts-on-scrim", "--ts-on-scrim-2" })
                pairs.Add(new Pair(tag + words + " on --ts-scrim-hero over white", UIAccessibility.ContrastRatio(C(words), LinearOver(C("--ts-scrim-hero"), Color.white)),
                    UIAccessibility.MinRatioEnhancedText));
            foreach (string body in BodyRoles)
                foreach (string ground in new[] { "--ts-surface", "--ts-surface-2" })
                    pairs.Add(new Pair(tag + body + " on " + ground + " APCA Lc", UIAccessibility.ApcaLc(C(body), C(ground)), 75f));
            foreach (string mark in MarkRoles)
                foreach (string ground in new[] { "--ts-surface", "--ts-surface-2" })
                    pairs.Add(new Pair(tag + mark + " on " + ground, UIAccessibility.ContrastRatio(C(mark), C(ground)), 4.5f));
            foreach (string family in Families)
            {
                string fill = "--ts-family-" + family;
                foreach (string ground in new[] { "--ts-surface", "--ts-surface-2" })
                    pairs.Add(new Pair(tag + fill + " on " + ground, UIAccessibility.ContrastRatio(C(fill), C(ground)), 4.5f));
                pairs.Add(new Pair(tag + fill + "-ink on " + fill, UIAccessibility.ContrastRatio(C(fill + "-ink"), C(fill)), UIAccessibility.MinRatioEnhancedText));
            }
            return pairs;
        }

        // `top` (with its alpha) laid over an opaque `under`, blended in linear light as the project renders
        public static Color LinearOver(Color top, Color under) =>
            Color.Lerp(under.linear, new Color(top.r, top.g, top.b, 1f).linear, top.a).gamma;

        // The theme's accent options (knob "accent")
        public static List<ThemeKnobOption> Accents(ThemeAsset theme) => theme.Knobs.Where(k => k.knob == "accent").ToList();
    }

    public class ThemeContrastTests
    {
        private const string AssetPath = "Assets/Framework/Runtime/Resources/TileStories/Themes/Theme.default.asset";

        private static ThemeAsset Theme()
        {
            var theme = AssetDatabase.LoadAssetAtPath<ThemeAsset>(AssetPath);
            Assert.IsNotNull(theme, "the default theme lives at " + AssetPath);
            return theme;
        }

        private static string Failures(IEnumerable<ThemeContrastAudit.Pair> pairs) =>
            string.Join("\n", pairs.Where(p => !p.Passes).Select(p => p.ToString()));

        // Every mode: every text >= 7:1 on every ground, body APCA >= 75, marks and family fills >= 4.5:1, ink on a fill >= 7:1. The measured
        // table is written to TestEvidence/Theme/contrast_<mode>.txt (the guide's Build log quotes it)
        [Test]
        public void EveryMode_EveryPairThatHoldsTextOrMeaning_ClearsItsBar()
        {
            var theme = Theme();
            CollectionAssert.AreEquivalent(CardOptions.Modes, theme.ModeKeys, "precondition: the theme has the light and dark modes");
            foreach (string mode in theme.ModeKeys)
            {
                var pairs = ThemeContrastAudit.Measure(theme, mode);
                Assert.Greater(pairs.Count, 80, "precondition: the audit measured the whole palette of " + mode);
                File.WriteAllLines(TestEvidence.PathFor("Theme", "contrast_" + mode + ".txt"), pairs.Select(p => p.ToString()));
                string failed = Failures(pairs);
                Assert.IsEmpty(failed, mode + " fails:\n" + failed);
            }
        }

        // Every accent option of the curated list (R12.4: a wall picks one, never a free colour), in every mode
        [Test]
        public void EveryAccentOption_InEveryMode_ClearsEveryBar()
        {
            var theme = Theme();
            var accents = ThemeContrastAudit.Accents(theme);
            Assert.GreaterOrEqual(accents.Count, 3, "the curated accent list holds at least 3 options (R12.4)");
            foreach (var accent in accents)
            {
                CollectionAssert.AreEquivalent(theme.ModeKeys, accent.modes.Select(m => m.key), accent.option + ": a value for every mode");
                foreach (string mode in theme.ModeKeys)
                {
                    string failed = Failures(ThemeContrastAudit.Measure(theme, mode, accent));
                    Assert.IsEmpty(failed, accent.option + " in " + mode + " fails:\n" + failed);
                }
            }
            Assert.AreEqual(theme.Mode(CardOptions.ModeLight).tokens.colours.Find(t => t.name == "--ts-accent").value,
                accents[0].modes.Find(m => m.key == CardOptions.ModeLight).tokens.colours.Find(t => t.name == "--ts-accent").value,
                "the theme's own accent is the first option's (heritage): the modes and the list cannot drift apart");
        }

        // Mutation proof: a failing value turns the audit red, naming the pair -- for a mode's text, a mark, a family ink and an accent option
        [Test]
        public void AFailingValue_IsCaught_NamingThePair()
        {
            var copy = Object.Instantiate(Theme());
            try
            {
                Set(copy.Mode(CardOptions.ModeLight).tokens.colours, "--ts-ink-2", new Color(0.6f, 0.6f, 0.6f));
                Set(copy.Mode(CardOptions.ModeDark).tokens.colours, "--ts-line", new Color(0.2f, 0.2f, 0.2f));
                Set(copy.Mode(CardOptions.ModeDark).tokens.colours, "--ts-family-media-ink", new Color(0.6f, 0.7f, 0.9f));
                var terracotta = ThemeContrastAudit.Accents(copy).Find(a => a.option == "terracotta");
                Assert.IsNotNull(terracotta, "precondition: the list holds terracotta");
                Set(terracotta.modes.Find(m => m.key == CardOptions.ModeLight).tokens.colours, "--ts-on-accent", new Color(0.8f, 0.5f, 0.4f));

                string light = Failures(ThemeContrastAudit.Measure(copy, CardOptions.ModeLight));
                StringAssert.Contains("light: --ts-ink-2 on --ts-surface ", light, "a grey secondary text is named with its ground");
                StringAssert.Contains("--ts-ink-2 on --ts-surface APCA Lc", light, "and fails APCA as body text");
                string dark = Failures(ThemeContrastAudit.Measure(copy, CardOptions.ModeDark));
                StringAssert.Contains("dark: --ts-line on --ts-surface ", dark, "a faint border is named");
                StringAssert.Contains("dark: --ts-family-media-ink on --ts-family-media ", dark, "a family ink lost on its fill is named");
                string accent = Failures(ThemeContrastAudit.Measure(copy, CardOptions.ModeLight, terracotta));
                StringAssert.Contains("light/terracotta: --ts-on-accent on --ts-accent", accent, "an accent option's button words are named");
            }
            finally { Object.DestroyImmediate(copy); }
        }

        // The APCA function against the algorithm's known values (APCA-W3 0.0.98G-4g)
        [Test]
        public void ApcaLc_MatchesTheReferenceValues()
        {
            Assert.AreEqual(106.04f, UIAccessibility.ApcaLc(Color.black, Color.white), 0.05f, "black text on white");
            Assert.AreEqual(107.88f, UIAccessibility.ApcaLc(Color.white, Color.black), 0.05f, "white text on black (reverse polarity)");
            Assert.AreEqual(0f, UIAccessibility.ApcaLc(Color.grey, Color.grey), "a colour on itself has no contrast");
            float mid = UIAccessibility.ApcaLc(new Color32(0x88, 0x88, 0x88, 255), Color.white);
            Assert.AreEqual(63.06f, mid, 0.1f, "#888 on white");
        }

        private static void Set(List<ThemeToken<Color>> colours, string name, Color value)
        {
            int at = colours.FindIndex(t => t.name == name);
            Assert.GreaterOrEqual(at, 0, "precondition: the set holds " + name);
            var token = colours[at];
            token.value = value;
            colours[at] = token;
        }
    }
}

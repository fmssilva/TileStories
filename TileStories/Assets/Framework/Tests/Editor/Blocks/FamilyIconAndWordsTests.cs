using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // What a navigation element needs to draw a family and a kind (_3.2 step 6): every family registered in the app's registry has an icon
    // the card's set really draws and a colour the app theme really holds, and every kind has words in English and Portuguese.
    // Read over BlockRegistry.Shared -- the framework's families and kinds AND the app's -- so a tenth family or a new kind cannot be
    // registered without them. Icons: the stylesheets read as text. Colours: the theme asset, through the resolver C# paints with.
    public class FamilyIconAndWordsTests
    {
        private static string AllStylesheets() =>
            string.Join("\n", Directory.GetFiles(Application.dataPath, "*.uss", SearchOption.AllDirectories).Select(File.ReadAllText));

        private static ThemeAsset Theme()
        {
            var theme = ThemeController.Load(ThemeController.DefaultThemeKey);
            Assert.IsNotNull(theme, "precondition: the default theme loads");
            return theme;
        }

        [Test]
        public void EveryRegisteredFamily_HasAnIconTheSetDraws_AndAColourTheThemeHolds()
        {
            string uss = AllStylesheets();
            var theme = Theme().ResolverFor(Theme().DefaultMode);
            var families = BlockRegistry.Shared.Families;
            Assert.GreaterOrEqual(families.Count, 9, "precondition: the nine built-in families are registered");
            foreach (var family in families)
            {
                Assert.IsFalse(string.IsNullOrEmpty(family.IconKey), family.Key + ": has an icon");
                CollectionAssert.Contains(CardIcons.All, family.IconKey, family.Key + ": its icon is one the set has");
                StringAssert.Contains(".card-icon--" + family.IconKey + " ", uss, family.Key + ": a stylesheet draws " + family.IconKey);
                StringAssert.StartsWith("--ts-", family.ColorToken ?? "", family.Key + ": its colour is a token");
                Assert.IsTrue(theme.TryColour(family.ColorToken, out _), family.Key + ": the theme holds " + family.ColorToken);
            }
            CollectionAssert.AllItemsAreUnique(families.Select(f => f.ColorToken), "one colour per family");
            Assert.AreEqual(families.Count, families.Select(f => f.IconKey).Distinct().Count(), "one icon per family");
        }

        // The family colours are never shown alone, but they still must read against the card (WCAG 1.4.11, non-text 3:1) -- checked on the
        // theme's values, in every mode, against that mode's surface
        [Test]
        public void EveryFamilyColour_ReadsAtThreeToOne_OnTheCardsSurface()
        {
            var theme = Theme();
            Assert.Greater(theme.Modes.Count, 0, "precondition: the theme has a mode");
            foreach (var mode in theme.Modes)
            {
                var colours = theme.ResolverFor(mode.key);
                Assert.IsTrue(colours.TryColour("--ts-surface", out var surface), mode.key + ": the theme holds --ts-surface");
                foreach (var family in BlockRegistry.Shared.Families)
                {
                    Assert.IsTrue(colours.TryColour(family.ColorToken, out var colour), mode.key + ": the theme holds " + family.ColorToken);
                    float ratio = UIAccessibility.ContrastRatio(colour, surface);
                    Assert.GreaterOrEqual(ratio, 3f, mode.key + ": " + family.ColorToken + " on --ts-surface: " + ratio.ToString("F2"));
                }
            }
        }

        // A section sheet (_3.3 P4.4c1) paints its family's colours BY NAME: the pill's ink and the sheet's glass are derived from the family's
        // colour token (SectionSheetRule.InkOf / GlassOf). A name the theme lacks would paint transparent with no warning (R8 S1), so every
        // registered family -- an app's own included -- must find both in every mode: the ink >= 7:1 on its fill (the opener's words), the
        // glass translucent (the camera stays present, D8)
        [Test]
        public void EveryRegisteredFamily_FindsItsSectionColours_InEveryMode()
        {
            var theme = Theme();
            foreach (var mode in theme.Modes)
            {
                var colours = theme.ResolverFor(mode.key);
                foreach (string part in new[] { "--ts-glass-edge", "--ts-glass-hi", "--ts-glass-shadow" })
                    Assert.IsTrue(colours.TryColour(part, out _), mode.key + ": the theme holds " + part);
                foreach (var family in BlockRegistry.Shared.Families)
                {
                    Assert.IsTrue(colours.TryColour(family.ColorToken, out var fill), mode.key + ": the theme holds " + family.ColorToken);
                    string ink = SectionSheetRule.InkOf(family.ColorToken), glass = SectionSheetRule.GlassOf(family.ColorToken);
                    Assert.IsTrue(colours.TryColour(ink, out var words), mode.key + ": the theme holds " + ink);
                    Assert.IsTrue(colours.TryColour(glass, out var sheet), mode.key + ": the theme holds " + glass);
                    float ratio = UIAccessibility.ContrastRatio(words, fill);
                    Assert.GreaterOrEqual(ratio, UIAccessibility.MinRatioEnhancedText, mode.key + ": " + ink + " on " + family.ColorToken + ": " + ratio.ToString("F2"));
                    Assert.That(sheet.a, Is.GreaterThan(0f).And.LessThan(1f), mode.key + ": " + glass + " is translucent glass, alpha " + sheet.a);
                }
            }
        }

        // The living glass and the rim light (_3.3 P4.4c2) read their colours BY NAME too (SectionSheetRule.LightOf / Light2Of / SweepOf), and
        // a missing one paints nothing with no warning: every registered family finds all three in every mode, the lights translucent (the
        // glass stays glass). R11 v3: the rim light is full strength on light metal and about half, and narrower, on dark
        [Test]
        public void EveryRegisteredFamily_FindsItsLights_InEveryMode_TheRimLightHalfAndNarrowerOnDark()
        {
            var theme = Theme();
            var light = theme.ResolverFor(CardOptions.ModeLight);
            var dark = theme.ResolverFor(CardOptions.ModeDark);
            Assert.IsNotNull(light, "precondition: a light mode");
            Assert.IsNotNull(dark, "precondition: a dark mode");
            foreach (var mode in theme.Modes)
            {
                var colours = theme.ResolverFor(mode.key);
                Assert.IsTrue(colours.TryNumber(CardLight.SweepArcToken, out float arc), mode.key + ": the theme holds " + CardLight.SweepArcToken);
                Assert.That(arc, Is.GreaterThan(0f).And.LessThan(360f), mode.key + ": the rim light's arc is part of a turn");
                foreach (var family in BlockRegistry.Shared.Families)
                {
                    foreach (string token in new[] { SectionSheetRule.LightOf(family.ColorToken), SectionSheetRule.Light2Of(family.ColorToken) })
                    {
                        Assert.IsTrue(colours.TryColour(token, out var glow), mode.key + ": the theme holds " + token);
                        Assert.That(glow.a, Is.GreaterThan(0f).And.LessThan(1f), mode.key + ": " + token + " is a translucent light, alpha " + glow.a);
                    }
                    Assert.IsTrue(colours.TryColour(SectionSheetRule.SweepOf(family.ColorToken), out _), mode.key + ": the theme holds " + SectionSheetRule.SweepOf(family.ColorToken));
                }
            }
            foreach (var family in BlockRegistry.Shared.Families)
            {
                light.TryColour(SectionSheetRule.SweepOf(family.ColorToken), out var onLight);
                dark.TryColour(SectionSheetRule.SweepOf(family.ColorToken), out var onDark);
                Assert.AreEqual(1f, onLight.a, family.Key + ": full strength on light metal");
                Assert.That(onDark.a, Is.InRange(0.35f * onLight.a, 0.55f * onLight.a), family.Key + ": about half strength on dark metal");
            }
            light.TryNumber(CardLight.SweepArcToken, out float lightArc);
            dark.TryNumber(CardLight.SweepArcToken, out float darkArc);
            Assert.Less(darkArc, lightArc, "the rim light is narrower on dark metal");
        }

        [Test]
        public void EveryIconOfTheSet_IsDrawnByTheStylesheet()
        {
            string uss = AllStylesheets();
            foreach (string key in CardIcons.All) StringAssert.Contains(".card-icon--" + key + " ", uss, key + ": the set's stylesheet draws it");
        }

        [Test]
        public void EveryRegisteredKind_HasWordsForAVisitor_InEnglishAndPortuguese()
        {
            var entries = CardStringTableChecks.FrameworkEntries().Concat(CardStringSources.Shared.Entries()).ToList();
            var kinds = BlockRegistry.Shared.All;
            Assert.Greater(kinds.Count, 30, "precondition: the framework's kinds and the app's are registered");
            foreach (var kind in kinds)
            {
                Assert.IsFalse(string.IsNullOrEmpty(kind.LabelKey), kind.Key + ": names its words");
                foreach (string lang in new[] { "en", "pt" })
                    Assert.IsFalse(string.IsNullOrWhiteSpace(CardStrings.Find(entries, kind.LabelKey, lang)), kind.Key + ": " + kind.LabelKey + " has " + lang + " words");
                Assert.AreNotEqual(kind.DisplayName, kind.LabelKey, kind.Key + ": a CardStrings key, not the Editor's name");
            }
        }
    }
}

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

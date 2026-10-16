using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // What a navigation element needs to draw a family and a kind (_3.2 step 6): every family registered in the app's registry has an icon
    // the card's set really draws and a colour token a stylesheet really defines, and every kind has words in English and Portuguese.
    // Read over BlockRegistry.Shared -- the framework's families and kinds AND the app's -- so a tenth family or a new kind cannot be
    // registered without them. The stylesheets are read as text: a token is defined where a USS file declares it.
    public class FamilyIconAndWordsTests
    {
        private static string AllStylesheets() =>
            string.Join("\n", Directory.GetFiles(Application.dataPath, "*.uss", SearchOption.AllDirectories).Select(File.ReadAllText));

        [Test]
        public void EveryRegisteredFamily_HasAnIconTheSetDraws_AndAColourTokenAStylesheetDefines()
        {
            string uss = AllStylesheets();
            var families = BlockRegistry.Shared.Families;
            Assert.GreaterOrEqual(families.Count, 9, "precondition: the nine built-in families are registered");
            foreach (var family in families)
            {
                Assert.IsFalse(string.IsNullOrEmpty(family.IconKey), family.Key + ": has an icon");
                CollectionAssert.Contains(CardIcons.All, family.IconKey, family.Key + ": its icon is one the set has");
                StringAssert.Contains(".card-icon--" + family.IconKey + " ", uss, family.Key + ": a stylesheet draws " + family.IconKey);
                StringAssert.StartsWith("--ts-", family.ColorToken ?? "", family.Key + ": its colour is a card token");
                Assert.IsTrue(Regex.IsMatch(uss, Regex.Escape(family.ColorToken) + @"\s*:"), family.Key + ": a stylesheet defines " + family.ColorToken);
            }
            CollectionAssert.AllItemsAreUnique(families.Select(f => f.ColorToken), "one colour per family");
            Assert.AreEqual(families.Count, families.Select(f => f.IconKey).Distinct().Count(), "one icon per family");
        }

        // The family colours are never shown alone, but they still must read against the card (WCAG 1.4.11, non-text 3:1) -- checked on the
        // token values of the default preset against its surface
        [Test]
        public void EveryFamilyColour_ReadsAtThreeToOne_OnTheCardsSurface()
        {
            string tokens = File.ReadAllText(Path.Combine(Application.dataPath, "Framework/Runtime/UI/Cards/CardTokens.uss"));
            Color Token(string name)
            {
                var m = Regex.Match(tokens, Regex.Escape(name) + @"\s*:\s*rgb\((\d+),\s*(\d+),\s*(\d+)\)");
                Assert.IsTrue(m.Success, name + " is an rgb() token of the default preset");
                return new Color(int.Parse(m.Groups[1].Value) / 255f, int.Parse(m.Groups[2].Value) / 255f, int.Parse(m.Groups[3].Value) / 255f);
            }
            var surface = Token("--ts-surface");
            foreach (var family in BlockRegistry.Shared.Families.Where(f => f.ColorToken.StartsWith("--ts-family-")))
            {
                float ratio = UIAccessibility.ContrastRatio(Token(family.ColorToken), surface);
                Assert.GreaterOrEqual(ratio, 3f, family.ColorToken + " on --ts-surface: " + ratio.ToString("F2"));
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

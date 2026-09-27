using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;

namespace TileStories.Editor.Tests
{
    // The card's UI texts (_3.1 step 5b): the lookup order of CardStrings, and the SHIPPED framework table
    // (CardStrings.asset) against every key the card's code reads -- so a view can never show a raw key.
    public class CardStringsTests
    {
        private const string AssetPath = "Assets/Framework/Runtime/UI/Cards/CardStrings.asset";

        private static CardStringEntry Entry(string key, params (string Lang, string Value)[] texts) =>
            new() { key = key, text = texts.Select(t => new LocalizedEntry { lang = t.Lang, value = t.Value }).ToList() };

        private static readonly List<CardStringEntry> Framework = new()
        {
            Entry("close", ("en", "Close"), ("pt", "Fechar")),
            Entry("hint", ("en", "Tap to reveal")),
        };

        [Test]
        public void TheWallsWording_WinsInTheVisitorsLanguage()
        {
            var wall = new List<CardStringEntry> { Entry("close", ("pt", "Sair")) };
            Assert.AreEqual("Sair", new CardStrings(Framework, wall, "pt", "en").Get("close"));
        }

        [Test]
        public void AWallsFallbackWording_NeverHidesTheFrameworksTextInTheVisitorsLanguage()
        {
            var wall = new List<CardStringEntry> { Entry("close", ("en", "Dismiss")) };
            Assert.AreEqual("Fechar", new CardStrings(Framework, wall, "pt", "en").Get("close"), "framework [pt] before wall [en]");
            Assert.AreEqual("Dismiss", new CardStrings(Framework, wall, "en", "en").Get("close"), "wall [en] for an English visitor");
        }

        [Test]
        public void AMissingLanguage_FallsBack_WallFirst_ThenFramework_ThenTheKey()
        {
            Assert.AreEqual("Tap to reveal", new CardStrings(Framework, null, "pt", "en").Get("hint"), "framework fallback");
            var wall = new List<CardStringEntry> { Entry("hint", ("en", "Tap it")) };
            Assert.AreEqual("Tap it", new CardStrings(Framework, wall, "pt", "en").Get("hint"), "wall fallback before framework fallback");
            Assert.AreEqual("nowhere", new CardStrings(Framework, wall, "pt", "en").Get("nowhere"), "an unknown key shows as itself");
        }

        [Test]
        public void ABlankWallField_KeepsTheFrameworksWording()
        {
            var wall = new List<CardStringEntry> { Entry("close", ("en", "  ")) };
            Assert.AreEqual("Close", new CardStrings(Framework, wall, "en", "en").Get("close"), "an emptied Card Texts field is not a wording");
        }

        [Test]
        public void TheShippedTable_HasEveryKeyTheCardReads_InEnglishAndPortuguese_WithAWhereNote()
        {
            var table = AssetDatabase.LoadAssetAtPath<CardStringTable>(AssetPath);
            Assert.IsNotNull(table, AssetPath + " exists");
            foreach (string key in CardStrings.Keys.All)
            {
                var row = table.RowOf(key);
                Assert.IsNotNull(row, "the table has a row for '" + key + "'");
                foreach (string lang in new[] { "en", "pt" })
                    Assert.IsNotNull(CardStrings.Find(table.Entries(), key, lang), key + " has " + lang + " text");
                Assert.IsFalse(string.IsNullOrWhiteSpace(row.where), key + " says where the card shows it (the Editor (i))");
                Assert.IsTrue((row.where + string.Concat(row.text.Select(t => t.value))).All(c => c < 128), key + ": ASCII only");
            }
            CollectionAssert.AreEquivalent(CardStrings.Keys.All, table.rows.Select(r => r.key), "no row the code never reads, no key without a row");
        }

        [Test]
        public void EveryKeyTheCardCodeReads_IsInKeysAll()
        {
            // - a view that reads CardStrings.Keys.X for an X missing from All would never be guarded above
            var used = new HashSet<string>();
            foreach (string file in Directory.GetFiles("Assets/Framework/Runtime", "*.cs", SearchOption.AllDirectories))
                foreach (Match m in Regex.Matches(File.ReadAllText(file), @"CardStrings\.Keys\.(\w+)"))
                    if (m.Groups[1].Value != "All") used.Add(m.Groups[1].Value);
            Assert.IsNotEmpty(used, "the scan found the card's reads (not vacuous)");
            var constants = typeof(CardStrings.Keys).GetFields().Where(f => f.IsLiteral).ToDictionary(f => f.Name, f => (string)f.GetRawConstantValue());
            foreach (string name in used)
            {
                Assert.IsTrue(constants.ContainsKey(name), name + " is a key constant");
                CollectionAssert.Contains(CardStrings.Keys.All, constants[name], name + " is listed in Keys.All");
            }
        }
    }
}

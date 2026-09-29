using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace TileStories.Editor.Tests
{
    // The two checks every card string table must pass, written ONCE for the framework's table and for an app's (_3.1 step 11-fix): the
    // table holds a row for exactly the keys the code reads, each in English and Portuguese with a "where" note; and the code reads no key
    // its Keys list lacks. Public: an app's test assembly reaches them, so an app's table gets the same guard with no copy of it.
    public static class CardStringTableChecks
    {
        // The framework's own card texts as the card reads them (the asset Detail Card > Card Texts lists first). The window's helper is
        // internal to the Editor assembly, which names no app, so an app's tests reach it through here.
        public static List<CardStringEntry> FrameworkEntries() => POIEditorToolWindow.FrameworkCardStrings().Entries();

        // This wall's own wording of one text in one language ("" when it has none): what typing in Card Texts writes
        public static string WallWording(CardSettings settings, string key, string language) => POIEditorToolWindow.CardTextOverride(settings, key, language);

        // The table has a row for every key (en + pt text that is valid visitor text, an ASCII where note) and no row the code never reads
        public static void AssertTableHasEveryKey(CardStringTable table, IReadOnlyCollection<string> keys, string owner)
        {
            Assert.IsNotNull(table, owner + ": the table asset exists");
            foreach (string key in keys)
            {
                var row = table.RowOf(key);
                Assert.IsNotNull(row, owner + ": the table has a row for '" + key + "'");
                foreach (string lang in new[] { "en", "pt" })
                    Assert.IsNotNull(CardStrings.Find(table.Entries(), key, lang), owner + ": " + key + " has " + lang + " text");
                Assert.IsFalse(string.IsNullOrWhiteSpace(row.where), owner + ": " + key + " says where the card shows it (the Editor (i))");
                // - the where-note is an Editor text (ASCII); the row's words are what a visitor reads (proper letters, valid text)
                EditorTextChecks.AssertAscii(row.where, owner + ": " + key + " where-note");
                foreach (var t in row.text) VisitorTextChecks.AssertValid(t.value, owner + ": " + key + " [" + t.lang + "]");
            }
            CollectionAssert.AreEquivalent(keys, table.rows.Select(r => r.key), owner + ": no row the code never reads, no key without a row");
        }

        // Every key the code under `folder` reads through `accessPrefix` (for example "CardStrings.Keys.") is a constant of `keysType` and
        // is listed in `allKeys`: a view that reads a key missing from the list would never be guarded by the table check above
        public static void AssertEveryKeyReadInSourceIsListed(string folder, string accessPrefix, Type keysType, IReadOnlyCollection<string> allKeys, string owner)
        {
            var used = new HashSet<string>();
            foreach (string file in Directory.GetFiles(folder, "*.cs", SearchOption.AllDirectories))
                foreach (Match m in Regex.Matches(File.ReadAllText(file), Regex.Escape(accessPrefix) + @"(\w+)"))
                    if (m.Groups[1].Value != "All") used.Add(m.Groups[1].Value);
            Assert.IsNotEmpty(used, owner + ": the scan found the code's reads (not vacuous)");
            var constants = keysType.GetFields().Where(f => f.IsLiteral).ToDictionary(f => f.Name, f => (string)f.GetRawConstantValue());
            foreach (string name in used)
            {
                Assert.IsTrue(constants.ContainsKey(name), owner + ": " + name + " is a key constant");
                CollectionAssert.Contains(allKeys, constants[name], owner + ": " + name + " is listed in Keys.All");
            }
        }
    }
}

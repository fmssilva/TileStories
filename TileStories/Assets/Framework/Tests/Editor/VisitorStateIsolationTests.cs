using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace TileStories.Editor.Tests
{
    // The visitor's saved state on this device (answers, votes, language, light / dark, recent searches, the last view) lives in ONE store,
    // DeviceStateStore, and a test run never reads or clears the developer's own (the S2c review: a "dark" picked while playing by hand
    // coloured a test's card). Proven three ways: the run's store is not the app's; the source names PlayerPrefs only in the store itself
    // (production) and in the store's own test (tests, on fresh wall ids); and the scan is proven to catch a planted use.
    public class VisitorStateIsolationTests
    {
        private const string Root = "Assets";

        // Where code may name PlayerPrefs, each with its reason: everything else reads the device through DeviceStateStore
        private static readonly Dictionary<string, string> Allowed = new()
        {
            ["Assets/Framework/Runtime/Blocks/State/PlayerPrefsCardStateStore.cs"] = "the app's store itself: the one place that touches PlayerPrefs",
            ["Assets/Framework/Runtime/Blocks/State/DeviceStateStore.cs"] = "builds the app's store (App)",
            ["Assets/Framework/Tests/Editor/Blocks/CardLocalStateTests.cs"] = "its subject IS the PlayerPrefs store: every key under a fresh Guid wall id, removed after",
            ["Assets/Framework/Tests/Editor/VisitorStateIsolationTests.cs"] = "this scan's own planted samples (strings, never run)",
        };

        // A code use of PlayerPrefs (the API, or building the PlayerPrefs store) on a line that is not a comment
        private static readonly Regex Use = new(@"\bPlayerPrefs\s*\.|new\s+PlayerPrefsCardStateStore\s*\(");

        // Every non-comment line of `files` (path -> text) that uses PlayerPrefs outside the allow-list, as "path:line"
        private static List<string> Violations(IEnumerable<(string Path, string Text)> files)
        {
            var found = new List<string>();
            foreach (var (path, text) in files)
            {
                if (Allowed.ContainsKey(path)) continue;
                string[] lines = text.Split('\n');
                for (int i = 0; i < lines.Length; i++)
                {
                    string code = lines[i];
                    int comment = code.IndexOf("//", System.StringComparison.Ordinal);
                    if (comment >= 0) code = code.Substring(0, comment);
                    if (Use.IsMatch(code)) found.Add(path + ":" + (i + 1));
                }
            }
            return found;
        }

        private static IEnumerable<(string Path, string Text)> SourceFiles() =>
            Directory.GetFiles(Root, "*.cs", SearchOption.AllDirectories)
                .Select(p => p.Replace('\\', '/'))
                .Where(p => p.StartsWith("Assets/Framework/") || p.StartsWith("Assets/Apps/"))
                .Select(p => (p, File.ReadAllText(p)));

        [Category("Core")]
        [Test]
        public void ThisRun_KeepsTheVisitorsStateInAStoreOfItsOwn_NeverTheAppsPlayerPrefs()
        {
            Assert.IsInstanceOf<PlayerPrefsCardStateStore>(DeviceStateStore.App, "the app keeps it in PlayerPrefs");
            Assert.AreNotSame(DeviceStateStore.App, DeviceStateStore.Current, "the run's SetUpFixture handed the device a store of its own");
            Assert.IsInstanceOf<MemoryCardStateStore>(DeviceStateStore.Current);
        }

        [Test]
        public void TheSource_NamesPlayerPrefsOnlyInTheStore_AndInTheStoresOwnTest()
        {
            foreach (string path in Allowed.Keys) FileAssert.Exists(path, "an allow-list line names a file that is gone: remove the line");
            var violations = Violations(SourceFiles());
            CollectionAssert.IsEmpty(violations, "read or write the visitor's saved state through DeviceStateStore.Current, never PlayerPrefs: " +
                string.Join(", ", violations));
        }

        [Test]
        public void TheScan_CatchesAPlantedUse_ButNotAComment()
        {
            var planted = new[]
            {
                ("Assets/Framework/Tests/Runtime/Planted.cs", "class A {\n    void B() { PlayerPrefs.DeleteKey(\"x\"); }\n}"),
                ("Assets/Framework/Runtime/Planted2.cs", "var s = new PlayerPrefsCardStateStore();"),
                ("Assets/Framework/Runtime/Planted3.cs", "// PlayerPrefs.GetString is mentioned in a comment only"),
            };
            CollectionAssert.AreEqual(new[] { "Assets/Framework/Tests/Runtime/Planted.cs:2", "Assets/Framework/Runtime/Planted2.cs:1" }, Violations(planted));
        }
    }
}

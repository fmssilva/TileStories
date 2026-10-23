using System.IO;
using System.Linq;
using NUnit.Framework;
using TileStories.Editor;
using UnityEditor;
using UnityEngine;

namespace TileStories.Tests
{
    // The theme stylesheet is GENERATED from the theme asset and committed (_3.3 R12.1): a stale one would ship colours the asset no
    // longer holds, and C# (ThemeResolver, reading the asset) would then disagree with what the screen draws. These tests hold the
    // committed file to what the generator makes of the committed asset NOW, and prove the check can fail.
    public class ThemeUssStaleTests
    {
        private const string AssetPath = "Assets/Framework/Runtime/Resources/TileStories/Themes/Theme.default.asset";

        private static ThemeAsset Theme()
        {
            var theme = AssetDatabase.LoadAssetAtPath<ThemeAsset>(AssetPath);
            Assert.IsNotNull(theme, "the default theme lives at " + AssetPath + " (ThemeController loads it from Resources)");
            return theme;
        }

        private static string Committed(ThemeAsset theme)
        {
            string path = ThemeUssGenerator.PathFor(theme);
            Assert.IsTrue(File.Exists(path), path + " is committed (a build reads it through TileStoriesTheme.tss)");
            // - git may hand the file back with CRLF on Windows: the generator writes LF
            return File.ReadAllText(path).Replace("\r\n", "\n");
        }

        [Test]
        public void TheCommittedThemeStylesheet_IsWhatTheGeneratorMakesOfTheAsset()
        {
            var theme = Theme();
            string committed = Committed(theme), now = ThemeUssGenerator.Generate(theme, AssetPath);
            if (committed == now) return;
            string[] a = committed.Split('\n'), b = now.Split('\n');
            int line = Enumerable.Range(0, Mathf.Min(a.Length, b.Length)).FirstOrDefault(i => a[i] != b[i]);
            Assert.Fail(ThemeUssGenerator.PathFor(theme) + " is STALE: press Regenerate USS on " + AssetPath + ". First difference at line " +
                        (line + 1) + ":\n  committed: " + (line < a.Length ? a[line] : "<end>") + "\n  asset now: " + (line < b.Length ? b[line] : "<end>"));
        }

        // - not vacuous: a value changed in the asset changes exactly its own line of the output (a generator that ignored the asset, or
        //   wrote a constant, would pass the test above forever)
        [Test]
        public void AValueChangedInTheAsset_ChangesExactlyItsOwnLine()
        {
            var copy = Object.Instantiate(Theme());
            try
            {
                var colours = copy.Mode(copy.DefaultMode).tokens.colours;
                int accent = colours.FindIndex(t => t.name == "--ts-accent");
                Assert.GreaterOrEqual(accent, 0, "precondition: the default mode holds --ts-accent");
                var token = colours[accent];
                token.value = new Color(1f, 0f, 1f, 0.5f);
                colours[accent] = token;

                string[] before = ThemeUssGenerator.Generate(Theme(), AssetPath).Split('\n'), after = ThemeUssGenerator.Generate(copy, AssetPath).Split('\n');
                Assert.AreEqual(before.Length, after.Length, "the same lines");
                var changed = Enumerable.Range(0, before.Length).Where(i => before[i] != after[i]).ToList();
                Assert.AreEqual(1, changed.Count, "exactly one line changes");
                Assert.AreEqual("    --ts-accent: rgba(255, 0, 255, 0.5);", after[changed[0]], "the accent's line, with the new value");
            }
            finally { Object.DestroyImmediate(copy); }
        }

        [Test]
        public void TheDefaultTheme_OpensInItsDarkMode_OneRuleOnTheMarkedAppRoot()
        {
            var theme = Theme();
            Assert.AreEqual(ThemeController.DefaultThemeKey, theme.Key, "the asset is the framework's default theme");
            Assert.IsNotNull(theme.Mode(theme.DefaultMode), "its default mode is one of its modes");
            Assert.AreEqual("dark", theme.DefaultMode, "today's Stone & Gold dark (_3.3 P4.1: no visual change)");
            string text = Committed(theme);
            StringAssert.StartsWith("/* GENERATED from " + AssetPath, text, "the header names the asset it was generated from");
            foreach (var mode in theme.Modes)
                Assert.AreEqual(1, text.Split('\n').Count(l => l == ".ts-app.ts-theme-" + theme.Key + ".ts-mode-" + mode.key + " {"),
                    mode.key + ": one rule on the root ThemeController marks");
        }

        [Test]
        public void TheGenerator_RefusesATokenDefinedTwice_AndANameThatIsNotACustomProperty()
        {
            var copy = Object.Instantiate(Theme());
            try
            {
                var colours = copy.Mode(copy.DefaultMode).tokens.colours;
                colours.Add(colours[0]);
                var twice = Assert.Throws<System.ArgumentException>(() => ThemeUssGenerator.Generate(copy, AssetPath));
                StringAssert.Contains(colours[0].name + " is defined twice", twice.Message);
                colours.RemoveAt(colours.Count - 1);
                colours.Add(new ThemeToken<Color> { name = "ts-no-dashes", value = Color.white });
                var badName = Assert.Throws<System.ArgumentException>(() => ThemeUssGenerator.Generate(copy, AssetPath));
                StringAssert.Contains("'ts-no-dashes' is not a USS custom property name", badName.Message);
            }
            finally { Object.DestroyImmediate(copy); }
        }
    }
}

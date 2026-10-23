using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace TileStories.Tests
{
    // A var() whose token nobody defines paints TRANSPARENT, silently -- no warning, no error (_3.3 R8 S1). Since the theme pipeline
    // (_3.3 P4.1) a token lives in one of two places: the app theme (generated from the theme asset, on the app root) or the component's
    // own sheets (CardTokens.uss for the card: its sizes and the numbers C# reads). These tests hold every name a screen's stylesheets
    // use to one of those places, and every name to ONE of them (a component re-declaring a theme token would shadow every mode).
    public class ThemeTokenGuardTests
    {
        private const string ThemeFolder = "Assets/Framework/Runtime/UI/Theme/Generated";
        private const string CardTokens = "Assets/Framework/Runtime/UI/Cards/CardTokens.uss";

        private static readonly Regex Comment = new(@"/\*.*?\*/", RegexOptions.Singleline);
        private static readonly Regex Use = new(@"var\(\s*(--[a-z0-9-]+)");
        private static readonly Regex Declaration = new(@"(--[a-z0-9-]+)\s*:");

        // The screens on the shared panel and the stylesheets each is drawn with: the card (the framework's sheets and every app's block
        // sheets, which the card loads after its own), the search UI, the zoom buttons
        private static Dictionary<string, string[]> Components() => new()
        {
            ["card"] = Directory.GetFiles("Assets/Framework/Runtime/UI/Cards", "*.uss", SearchOption.AllDirectories)
                .Concat(Directory.GetFiles("Assets/Apps", "*.uss", SearchOption.AllDirectories)).ToArray(),
            ["search"] = Directory.GetFiles("Assets/Framework/Runtime/UI/Navigation", "*.uss", SearchOption.AllDirectories),
            ["zoom"] = Directory.GetFiles("Assets/Framework/Runtime/UI/Zoom", "*.uss", SearchOption.AllDirectories),
        };

        private static string Text(string path) => Comment.Replace(File.ReadAllText(path), "");

        private static HashSet<string> Used(IEnumerable<string> files) =>
            new(files.SelectMany(f => Use.Matches(Text(f)).Cast<Match>().Select(m => m.Groups[1].Value)));

        // - a declaration is a name followed by ':' that is not inside var( ... ) -- strip the uses first
        private static HashSet<string> Declared(IEnumerable<string> files) =>
            new(files.SelectMany(f => Declaration.Matches(Use.Replace(Text(f), "")).Cast<Match>().Select(m => m.Groups[1].Value)));

        private static HashSet<string> ThemeTokens()
        {
            var files = Directory.GetFiles(ThemeFolder, "*.uss");
            Assert.Greater(files.Length, 0, "precondition: the generated theme is in " + ThemeFolder);
            return Declared(files);
        }

        [Test]
        public void EveryTokenAScreenUses_IsDefinedByTheThemeOrByItsOwnSheets()
        {
            var theme = ThemeTokens();
            foreach (var (component, files) in Components())
            {
                Assert.Greater(files.Length, 0, "precondition: " + component + " has stylesheets");
                var used = Used(files);
                var undefined = used.Where(t => !theme.Contains(t) && !Declared(files).Contains(t)).OrderBy(t => t).ToList();
                Assert.IsEmpty(undefined, component + ": these tokens are used but defined nowhere (they paint transparent): " + string.Join(", ", undefined));
            }
            // - not vacuous: the card really is written with tokens of both kinds
            var card = Used(Components()["card"]);
            Assert.Greater(card.Count(theme.Contains), 30, "the card uses the theme's tokens");
            Assert.Greater(card.Count(Declared(new[] { CardTokens }).Contains), 30, "the card uses its own tokens");
        }

        [Test]
        public void NoTokenIsDefinedBothByTheThemeAndByAScreensOwnSheets()
        {
            var theme = ThemeTokens();
            foreach (var (component, files) in Components())
            {
                var twice = Declared(files).Where(theme.Contains).OrderBy(t => t).ToList();
                Assert.IsEmpty(twice, component + ": these re-declare a theme token (they would hide every mode and knob of the theme): " + string.Join(", ", twice));
            }
        }

        private static readonly Regex FontProperty = new(@"-unity-font(-definition)?\s*:");

        // The app's faces are the THEME's (_3.3 P4.3, R12.3): only the generated theme names a font. A sheet that set one itself would
        // fork the type (a theme or knob change would skip it), and a TMP asset given by USS blanks a label silently (R8 S2). The
        // dev-only spike (Assets/Dev) is not the app and keeps its own bake-off sheets
        [Test]
        public void NoStylesheetOutsideTheGeneratedTheme_SetsAFont()
        {
            var sheets = new[] { "Assets/Framework", "Assets/Apps" }
                .SelectMany(root => new[] { "*.uss", "*.uxml", "*.tss" }.SelectMany(p => Directory.GetFiles(root, p, SearchOption.AllDirectories)))
                .Select(p => p.Replace('\\', '/')).Where(p => !p.StartsWith(ThemeFolder + "/")).ToList();
            var screens = Components().Values.SelectMany(f => f).Select(p => p.Replace('\\', '/')).ToList();
            CollectionAssert.IsSubsetOf(screens, sheets, "precondition: every stylesheet a screen is drawn with is scanned");
            CollectionAssert.Contains(sheets, "Assets/Framework/Runtime/UI/Theme/TileStoriesTheme.tss", "precondition: ...and the panel's theme");
            var offenders = sheets.Where(p => FontProperty.IsMatch(Text(p))).ToList();
            Assert.IsEmpty(offenders, "these set a font themselves (the theme's type roles decide every face): " + string.Join(", ", offenders));
            // - not vacuous: the generated theme does set the faces, and the pattern finds it there
            Assert.IsTrue(Directory.GetFiles(ThemeFolder, "*.uss").Any(p => FontProperty.IsMatch(Text(p))), "the generated theme sets the faces");
        }

        [Test]
        public void TheCardsOwnTokens_HoldNoColour()
        {
            string text = Text(CardTokens);
            StringAssert.DoesNotMatch(@"rgba?\(|#[0-9a-fA-F]{3,8}\b", text, "every colour is the theme's (it changes with the mode; C# reads it through ThemeResolver)");
            Assert.Greater(Declared(new[] { CardTokens }).Count, 50, "precondition: the card's own sizes are here");
        }
    }
}

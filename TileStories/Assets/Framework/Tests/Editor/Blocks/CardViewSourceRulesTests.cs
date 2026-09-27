using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace TileStories.Editor.Tests
{
    // The card's two hard rules (_3.1 section 3, 30-ui-content.md), enforced on the source of every card view
    // (Runtime/UI/Cards/**/*.cs) so a later edit cannot slip past them:
    //   - no literal colour or size: views add CLASSES, PoiCard.uss + CardTokens.uss hold every value (_3.3 section 1)
    //   - no visitor string in code: a text a visitor reads comes from the config or the card strings table
    public class CardViewSourceRulesTests
    {
        private const string CardViews = "Assets/Framework/Runtime/UI/Cards";

        private static string[] Files() => Directory.GetFiles(CardViews, "*.cs", SearchOption.AllDirectories);

        // Source without comments (a comment may describe a colour or quote a visitor text)
        private static string Code(string file) =>
            Regex.Replace(Regex.Replace(File.ReadAllText(file), @"//.*", ""), @"/\*.*?\*/", "", RegexOptions.Singleline);

        [Test]
        public void TheScanSeesTheCardViews()
        {
            var names = Files().Select(Path.GetFileName).ToList();
            foreach (string view in new[] { "PoiCardSheetView.cs", "RichTextBlockView.cs", "StatusBlockView.cs", "ActionsBlockView.cs", "CardTextView.cs" })
                CollectionAssert.Contains(names, view, "not vacuous");
        }

        [Test]
        public void NoCardView_WritesALiteralColourOrSize()
        {
            var literal = new Regex(
                @"new\s+Color\s*\(|Color\.(white|black|red|green|blue|yellow|cyan|magenta|gray|grey|clear)\b|" +
                @"ColorUtility\.|\brgba?\s*\(|" +
                @"style\.(width|height|min\w*|max\w*|fontSize|margin\w*|padding\w*|border\w*Width|border\w*Radius|left|right|top|bottom)\s*=\s*-?\d");
            foreach (string file in Files())
                foreach (Match m in literal.Matches(Code(file)))
                    Assert.Fail(Path.GetFileName(file) + " writes a literal look in C#: '" + m.Value + "' (put it in PoiCard.uss / CardTokens.uss)");
        }

        [Test]
        public void NoCardView_WritesAVisitorStringLiteral()
        {
            // - a non-empty literal given to a text, a tooltip or a Label/Button text initialiser
            var visitorText = new Regex(@"(\.text|\btext|\.tooltip|\btooltip)\s*=\s*""[^""]+""");
            foreach (string file in Files())
                foreach (Match m in visitorText.Matches(Code(file)))
                    Assert.Fail(Path.GetFileName(file) + " writes a visitor string in code: '" + m.Value + "' (use the config or CardStrings)");
        }
    }
}

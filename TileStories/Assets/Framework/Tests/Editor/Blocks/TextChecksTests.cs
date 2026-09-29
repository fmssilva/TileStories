using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // The two text rules of _3.1 step 9-pre: the Editor's texts stay ASCII (and an accent still fails there), a visitor's text is
    // valid language. The rules are proven on hand-made strings, then on the shipped content.
    public class TextChecksTests
    {
        private const string Accented = "Descrição útil, comentário, Tão largo como...";

        [Test]
        public void AnEditorText_WithAnAccent_StillFails_AndPlainAsciiPasses()
        {
            Assert.IsNotNull(EditorTextChecks.FirstProblem(Accented), "an accented Editor help text is refused");
            Assert.Throws<NUnit.Framework.AssertionException>(() => EditorTextChecks.AssertAscii("Card language: Português", "an Editor help"),
                "the assertion the Editor tests use throws for an accent");
            Assert.Throws<NUnit.Framework.AssertionException>(() => EditorTextChecks.AssertAscii("dash – here", "an Editor label"), "a typographic dash too");
            Assert.IsNull(EditorTextChecks.FirstProblem("Open At: peek -> half\nSecond line."), "ASCII with a newline is fine");
            EditorTextChecks.AssertAscii("Half Height Max (0.25-0.40)", "an Editor label");
        }

        [Test]
        public void AVisitorText_WithProperPortuguese_IsValid_AndBrokenTextIsNot()
        {
            VisitorTextChecks.AssertValid(Accented, "proper Portuguese");
            VisitorTextChecks.AssertValid("Primeiro parágrafo.\nSegundo parágrafo.", "two paragraphs");
            Assert.IsNotNull(VisitorTextChecks.Problem("bell\u0007here"), "a control character");
            Assert.IsNotNull(VisitorTextChecks.Problem("tab\there"), "a tab is a control character too");
            Assert.IsNotNull(VisitorTextChecks.Problem("Descri��o"), "a replacement character is a sign of the wrong encoding");
            Assert.IsNotNull(VisitorTextChecks.Problem("a" + "\uD83D" + "b"), "a lone high surrogate");
            Assert.IsNotNull(VisitorTextChecks.Problem("caça"), "a c and its cedilla stored apart (not NFC)");
            Assert.IsNotNull(VisitorTextChecks.Problem("Sẽo"), "a tilde stored apart");
            Assert.IsNotNull(VisitorTextChecks.Problem(null), "null");
        }

        [Test]
        public void EveryVisitorText_OfTheShippedLivingRoomCards_IsValid_InBothConfigCopies()
        {
            foreach (string path in new[] { "Assets/Apps/LivingRoom/config.json", "Assets/StreamingAssets/LivingRoom/config.json" })
            {
                var config = JsonUtility.FromJson<WallConfigData>(File.ReadAllText(path));
                var values = VisitorTextChecks.LocalizedValues(config.card_settings, "card_settings");
                for (int i = 0; i < config.pois.Count; i++) values.AddRange(VisitorTextChecks.LocalizedValues(config.pois[i].card, "pois[" + i + "].card"));
                Assert.Greater(values.Count, 300, path + ": the walk found the card content (not vacuous)");
                Assert.IsTrue(values.Any(v => v.Lang == "pt"), path + ": Portuguese content is there");
                foreach (var v in values) VisitorTextChecks.AssertValid(v.Value, path + " " + v.Where + " [" + v.Lang + "]");
            }
        }
    }
}

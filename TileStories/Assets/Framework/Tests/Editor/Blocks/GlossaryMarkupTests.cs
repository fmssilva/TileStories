using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace TileStories.Editor.Tests
{
    // The card's long-text markup and glossary (_3.1 section 6): paragraphs at blank lines, [[term]] and
    // [[shown words|term]], the author's own characters never read as tags, definitions by language.
    public class GlossaryMarkupTests
    {
        [Test]
        public void Paragraphs_SplitAtBlankLines_TrimmedAndNeverEmpty()
        {
            CollectionAssert.AreEqual(new[] { "One.", "Two, still two.\nSame paragraph.", "Three." },
                GlossaryMarkup.Paragraphs("  One.\n\nTwo, still two.\nSame paragraph.\r\n  \r\nThree.\n\n\n"));
            CollectionAssert.IsEmpty(GlossaryMarkup.Paragraphs("   \n\n  "));
            CollectionAssert.IsEmpty(GlossaryMarkup.Paragraphs(null));
        }

        [Test]
        public void Parse_ReadsBothTermForms_AndKeepsTheTextAroundThem()
        {
            var runs = GlossaryMarkup.Parse("The [[keep]] and the [[Torre de menagem|keep]] fell.");
            CollectionAssert.AreEqual(new[] { "The ", "keep", " and the ", "Torre de menagem", " fell." }, runs.Select(r => r.Text));
            CollectionAssert.AreEqual(new[] { null, "keep", null, "keep", null }, runs.Select(r => r.Term));
            Assert.AreEqual("The keep and the Torre de menagem fell.", GlossaryMarkup.PlainText("The [[keep]] and the [[Torre de menagem|keep]] fell."));
        }

        [Test]
        public void BrokenMarkup_IsShownAsTheAuthorTypedIt()
        {
            Assert.AreEqual("a [[|x]] b", GlossaryMarkup.PlainText("a [[|x]] b"), "no shown words");
            Assert.AreEqual("a [[x|]] b", GlossaryMarkup.PlainText("a [[x|]] b"), "no term");
            Assert.AreEqual("a [[x b", GlossaryMarkup.PlainText("a [[x b"), "unclosed");
        }

        [Test]
        public void Terms_EachOnce_IgnoringCase_AcrossParagraphs()
        {
            CollectionAssert.AreEqual(new[] { "keep", "curtain wall" }, GlossaryMarkup.Terms("[[keep]] x [[Keep]]\n\n[[a|curtain wall]] [[x|KEEP]]"));
        }

        [Test]
        public void RichText_AKnownTermIsALink_AnUnknownOnePlain_AndTheAuthorsTagsAreNeverObeyed()
        {
            var runs = GlossaryMarkup.Parse("<b>bold?</b> [[keep]] [[moat]]");
            string rich = GlossaryMarkup.ToRichText(runs, t => t == "keep");
            StringAssert.Contains("<noparse><b>bold?</b> </noparse>", rich, "the author's <b> is shown literally");
            StringAssert.Contains("<link=\"keep\"><u><noparse>keep</noparse></u></link>", rich, "a known term links to its entry");
            StringAssert.DoesNotContain("<link=\"moat\"", rich, "an unknown term is not a link");
            StringAssert.Contains("<noparse>moat</noparse>", rich, "...it shows as plain text");
        }

        private static readonly List<GlossaryEntry> Entries = new()
        {
            new GlossaryEntry
            {
                term = "Keep",
                definition = new List<LocalizedEntry> { new() { lang = "en", value = "The strongest tower." }, new() { lang = "pt", value = "A torre mais forte." } },
            },
            new GlossaryEntry { term = "moat", definition = new List<LocalizedEntry> { new() { lang = "en", value = "A ditch of water." } } },
            new GlossaryEntry { term = "empty", definition = new List<LocalizedEntry> { new() { lang = "en", value = "  " } } },
        };

        [Test]
        public void TheGlossary_MatchesIgnoringCase_InTheCardsLanguage_WithTheFallback()
        {
            Assert.AreEqual("A torre mais forte.", new CardGlossary(Entries, "pt", "en").Definition(" keep "));
            Assert.AreEqual("A ditch of water.", new CardGlossary(Entries, "pt", "en").Definition("MOAT"), "no pt text: the fallback");
            Assert.IsNull(new CardGlossary(Entries, "en", "en").Definition("tower"), "no entry");
            Assert.IsFalse(new CardGlossary(Entries, "en", "en").Knows("empty"), "an entry with no definition text links nothing");
        }
    }
}

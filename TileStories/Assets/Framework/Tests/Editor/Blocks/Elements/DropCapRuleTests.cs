using NUnit.Framework;

namespace TileStories.Editor.Tests
{
    // DropCapRule (_3.3 S2a): where a drop-cap paragraph is cut -- the words filling N lines beside the raised letter, the rest under it. The
    // measure here is 1 unit per character, so every expected cut is arithmetic on the words (the real face's widths are the paragraph
    // component's, checked on the render by CardStoryGalleryTests).
    public class DropCapRuleTests
    {
        private static float Chars(string run) => run.Length;

        [Test]
        public void TheWordsFillingTheLinesGoBeside_TheRestUnder()
        {
            // - width 10: "hen King" (8) | "Manuel I" (8) | "found the" ... -> two lines beside, the rest under
            var cut = DropCapRule.Split("hen King Manuel I found the old castle", 10f, 2, Chars);
            Assert.AreEqual("hen King Manuel I", cut.Beside);
            Assert.AreEqual("found the old castle", cut.Under);
            // - three lines beside: one more line moves up
            cut = DropCapRule.Split("hen King Manuel I found the old castle", 10f, 3, Chars);
            Assert.AreEqual("hen King Manuel I found the", cut.Beside);
            Assert.AreEqual("old castle", cut.Under);
        }

        [Test]
        public void AShortParagraph_StaysWholeBesideTheLetter_AndNoRoomToJudge_KeepsItWhole()
        {
            Assert.AreEqual("", DropCapRule.Split("hen it rained", 20f, 2, Chars).Under, "it fits the lines: nothing under");
            var unlaid = DropCapRule.Split("hen King Manuel I found", 0f, 2, Chars);
            Assert.AreEqual("hen King Manuel I found", unlaid.Beside, "no width yet (the first layout pass): the whole text beside");
            Assert.AreEqual("", unlaid.Under);
            Assert.AreEqual("", DropCapRule.Split("", 10f, 2, Chars).Beside);
        }

        // A glossary mark is one word, measured as its SHOWN words, and never cut in two; spaces inside it do not split
        [Test]
        public void AGlossaryMark_IsOneWord_MeasuredAsItsShownWords()
        {
            CollectionAssert.AreEqual(new[] { "the", "[[curtain wall|curtain wall]]", "fell" }, DropCapRule.Words("the [[curtain wall|curtain wall]] fell"));
            // - "the curtain wall" is 16 characters shown (the mark's 30 would not fit 16): it stays on line one
            var cut = DropCapRule.Split("the [[curtain wall|curtain wall]] fell down", 16f, 1, Chars);
            Assert.AreEqual("the [[curtain wall|curtain wall]]", cut.Beside);
            Assert.AreEqual("fell down", cut.Under);
        }

        // A word wider than the line takes its own line (the engine breaks it); the words after it follow on the next line
        [Test]
        public void AWordWiderThanTheLine_TakesALineOfItsOwn()
        {
            var cut = DropCapRule.Split("a Constantinopolitan tower stood", 10f, 2, Chars);
            Assert.AreEqual("a",DropCapRule.Split("a Constantinopolitan tower", 10f, 1, Chars).Beside, "line 1 holds 'a', the long word opens line 2");
            Assert.AreEqual("a Constantinopolitan", cut.Beside, "line 2 is the long word alone");
            Assert.AreEqual("tower stood", cut.Under);
        }
    }
}

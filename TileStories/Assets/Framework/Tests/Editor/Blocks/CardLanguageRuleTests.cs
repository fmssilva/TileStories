using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace TileStories.Editor.Tests
{
    // _3.1 15.3.1 / 15.3.2: which language a card speaks and which a text is read in. The rule is pure, so every case is a plain call; the
    // text pick (BlockFieldReader.Pick), the card strings and the glossary are driven with the SAME language + fallback the host hands them.
    public class CardLanguageRuleTests
    {
        private static List<LocalizedEntry> Texts(params (string Lang, string Value)[] texts) =>
            texts.Select(t => new LocalizedEntry { lang = t.Lang, value = t.Value }).ToList();

        private static CardStringEntry StringRow(string key, params (string Lang, string Value)[] texts) =>
            new() { key = key, text = Texts(texts) };

        private static readonly string[] EnPt = { "en", "pt" };

        [Test]
        public void Choices_KeepTheWallsOrder_DropBlanksAndRepeats()
        {
            CollectionAssert.AreEqual(new[] { "en", "pt", "es" }, CardLanguageRule.Choices(new[] { "en", " ", null, "pt", "en", " es " }));
            CollectionAssert.IsEmpty(CardLanguageRule.Choices(null));
            CollectionAssert.IsEmpty(CardLanguageRule.Choices(new string[0]));
        }

        [Test]
        public void TheFallback_IsTheWallsFirstLanguage_AndEmptyWhenThereIsNone()
        {
            Assert.AreEqual("en", CardLanguageRule.Fallback(EnPt));
            Assert.AreEqual("pt", CardLanguageRule.Fallback(new[] { "pt", "en" }), "reordering Languages moves the fallback");
            Assert.AreEqual("", CardLanguageRule.Fallback(null));
            Assert.AreEqual("", CardLanguageRule.Fallback(new[] { " " }));
        }

        [Test]
        public void TheShownLanguage_IsTheVisitorsPick_ThenThePreview_ThenTheFirst()
        {
            Assert.AreEqual("en", CardLanguageRule.Shown(EnPt, "", "", true), "nothing picked: the wall's first");
            Assert.AreEqual("pt", CardLanguageRule.Shown(EnPt, "pt", "", true), "the visitor's pick");
            Assert.AreEqual("pt", CardLanguageRule.Shown(EnPt, "", "pt", true), "the developer's preview while no pick");
            Assert.AreEqual("en", CardLanguageRule.Shown(EnPt, "en", "pt", true), "the visitor's pick wins over the preview");
        }

        [Test]
        public void ThePreview_IsIgnoredWhereTheBuildDoesNotAllowDeveloperFeatures()
        {
            Assert.AreEqual("en", CardLanguageRule.Shown(EnPt, "", "pt", false), "a release build ignores a preview left in the config");
            Assert.AreEqual("pt", CardLanguageRule.Shown(EnPt, "pt", "", false), "but a visitor's own pick works everywhere");
        }

        [Test]
        public void ALanguageTheWallDoesNotOffer_IsNeverShown()
        {
            Assert.AreEqual("en", CardLanguageRule.Shown(EnPt, "fr", "", true), "a saved pick from an older Languages list");
            Assert.AreEqual("en", CardLanguageRule.Shown(EnPt, "", "fr", true), "a preview that is no longer listed");
            Assert.AreEqual("", CardLanguageRule.Shown(null, "pt", "pt", true), "a wall with no language shows none");
        }

        [Test]
        public void Next_StepsThroughTheChoices_AndWraps_AndIsEmptyWithOnlyOne()
        {
            Assert.AreEqual("pt", CardLanguageRule.Next(EnPt, "en"));
            Assert.AreEqual("en", CardLanguageRule.Next(EnPt, "pt"), "wraps round");
            var three = new[] { "en", "pt", "es" };
            Assert.AreEqual("es", CardLanguageRule.Next(three, "pt"));
            Assert.AreEqual("en", CardLanguageRule.Next(three, "es"));
            Assert.AreEqual("", CardLanguageRule.Next(new[] { "en" }, "en"), "one language: nothing to switch to");
            Assert.AreEqual("", CardLanguageRule.Next(new[] { "en", "en" }, "en"), "a repeated language is still one");
        }

        // ---- the text pick: shown language -> the wall's FIRST -> the first language the field has

        [Test]
        public void AnEnglishOnlyField_OnAPortugueseCard_ReadsEnglish()
        {
            var field = Texts(("en", "The keep"));
            Assert.AreEqual("The keep", BlockFieldReader.Pick(field, "pt", CardLanguageRule.Fallback(EnPt)));
        }

        [Test]
        public void APortugueseOnlyField_OnAnEnglishCard_ReadsPortuguese()
        {
            var field = Texts(("pt", "A torre"));
            Assert.AreEqual("A torre", BlockFieldReader.Pick(field, "en", CardLanguageRule.Fallback(EnPt)), "the field has no en and no fallback text: the one it has");
        }

        [Test]
        public void AFieldWithNoText_ShowsNothing()
        {
            Assert.AreEqual("", BlockFieldReader.Pick(Texts(("en", " "), ("pt", "")), "pt", "en"));
            Assert.AreEqual("", BlockFieldReader.Pick(new List<LocalizedEntry>(), "pt", "en"));
            Assert.AreEqual("", BlockFieldReader.Pick(null, "pt", "en"));
        }

        [Test]
        public void TheWallsFirstLanguage_BeatsAnotherLanguageTheFieldHappensToListFirst()
        {
            var field = Texts(("es", "La torre"), ("en", "The keep"));
            Assert.AreEqual("The keep", BlockFieldReader.Pick(field, "pt", "en"), "the fallback is the wall's first (en), not whichever the field lists first (es)");
            Assert.AreEqual("La torre", BlockFieldReader.Pick(field, "pt", "pt"), "the old behaviour (fallback = shown): the field's first language, which is the bug");
        }

        [Test]
        public void TheShownLanguage_StillWinsOverTheFallback()
        {
            var field = Texts(("en", "The keep"), ("pt", "A torre"));
            Assert.AreEqual("A torre", BlockFieldReader.Pick(field, "pt", "en"));
            Assert.AreEqual("The keep", BlockFieldReader.Pick(field, "en", "en"));
        }

        [Test]
        public void ABlockFieldReader_AppliesTheSameRuleToTextAndItemText()
        {
            var block = new BlockInstanceData
            {
                fields = new List<BlockFieldValue>
                {
                    new() { key = "heading", text = Texts(("en", "Heading")) },
                    new() { key = "rows", items = new List<BlockItemData> { new() { fields = new List<BlockItemFieldValue> { new() { key = "label", text = Texts(("en", "Row")) } } } } },
                },
            };
            var read = new BlockFieldReader(block, "pt", CardLanguageRule.Fallback(EnPt));
            Assert.AreEqual("Heading", read.Text("heading"));
            Assert.AreEqual("Row", read.ItemText(read.Items("rows")[0], "label"));
        }

        // ---- the card's own texts and the glossary read through the same fallback

        [Test]
        public void ACardString_FallsBackToTheWallsFirstLanguage_ThenToAnyLanguageItHas_ThenTheKey()
        {
            var table = new List<CardStringEntry>
            {
                StringRow("en_only", ("en", "Only English")),
                StringRow("es_only", ("es", "Solo espanol")),
                StringRow("both", ("en", "Both"), ("pt", "Ambos")),
            };
            var pt = new CardStrings(table, null, null, "pt", "en");
            Assert.AreEqual("Only English", pt.Get("en_only"), "no pt text: the wall's first language");
            Assert.AreEqual("Solo espanol", pt.Get("es_only"), "neither pt nor en: the language the key has");
            Assert.AreEqual("Ambos", pt.Get("both"));
            Assert.AreEqual("nowhere", pt.Get("nowhere"), "an unknown key shows as itself");
        }

        [Test]
        public void AGlossaryDefinition_FallsBackTheSameWay()
        {
            var entries = new List<GlossaryEntry>
            {
                new() { term = "keep", definition = Texts(("en", "The strongest tower.")) },
                new() { term = "moat", definition = Texts(("pt", "Um fosso de agua.")) },
            };
            var glossary = new CardGlossary(entries, "pt", "en");
            Assert.AreEqual("The strongest tower.", glossary.Definition("keep"));
            Assert.AreEqual("Um fosso de agua.", glossary.Definition("moat"));
            Assert.AreEqual("Um fosso de agua.", new CardGlossary(entries, "en", "en").Definition("moat"), "pt-only on an English card");
        }

        // ---- the visitor's pick lives in the card's state, per wall

        [Test]
        public void TheVisitorsLanguage_IsRemembered_PerWall_AndForgottenByTheReset()
        {
            var store = new MemoryCardStateStore();
            var wall = new CardLocalState(store, "living_room");
            Assert.AreEqual("", wall.Language(), "never picked");
            wall.SetLanguage("pt");
            Assert.AreEqual("pt", new CardLocalState(store, "living_room").Language(), "a new object over the same store reads it back");
            Assert.AreEqual("", new CardLocalState(store, "chafariz").Language(), "another wall keeps its own");
            Assert.AreEqual(1, wall.ResetAll(), "the reset forgets exactly the entry written");
            Assert.AreEqual("", wall.Language());
        }

        [Test]
        public void ABlankLanguage_ForgetsThePick()
        {
            var wall = new CardLocalState(new MemoryCardStateStore(), "living_room");
            wall.SetLanguage("pt");
            wall.SetLanguage(" ");
            Assert.AreEqual("", wall.Language());
        }

        [Test]
        public void ThePick_DoesNotClashWithAnAnswerOrAVote()
        {
            var wall = new CardLocalState(new MemoryCardStateStore(), "living_room");
            wall.SetLanguage("pt");
            wall.SetAnswer("lamp", "block_1", 0, 2);
            Assert.AreEqual("pt", wall.Language());
            Assert.AreEqual(2, wall.Answer("lamp", "block_1", 0));
            Assert.AreEqual(2, wall.ResetAll(), "both entries were indexed");
        }
    }
}

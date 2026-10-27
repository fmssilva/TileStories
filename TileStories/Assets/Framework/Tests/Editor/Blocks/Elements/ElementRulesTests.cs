using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // ElementRules and ElementMotionRule on The Painting's story (_3.3 R14.3, R14.8, R17.3, R17.4), and the seams they feed: the card's
    // own ReadingTimeRule / BlockSummaryRule / CompactRowRule / BlockStackBuilder read a composed block's rows with no code of their own.
    // Every expected number is counted here from The Painting's shipped texts (a word = a run of non-spaces), never typed in.
    public class ElementRulesTests
    {
        private PaintingStory _s;

        [SetUp]
        public void BuildTheStory() => _s = PaintingStory.Build();

        // The story's reading time in `language` as the guide defines it: its words at 200 a minute plus 10 s for the picture and 6 s for each
        // of the carousel's four, rounded up to whole minutes
        private int ExpectedMinutes(string language)
        {
            int words = new[] { _s.Eyebrow, _s.Title, _s.Lede, _s.Paragraph }.Sum(r => Words(r, language));
            float seconds = words * 60f / 200f + 1 * BuiltInElements.PictureSeconds + 4 * BuiltInElements.CarouselPictureSeconds;
            return Mathf.CeilToInt(seconds / 60f);
        }

        private static int Words(ElementRow row, string language) =>
            Regex.Matches(GlossaryMarkup.PlainText(BlockFieldReader.Pick(row.Field(BuiltInElements.TextField).text, language, language)), @"\S+").Count;

        [Test]
        public void Minutes_AreThePagesWordsAndMediaWeights_RoundedUp_InEachLanguage()
        {
            Assert.Greater(Words(_s.Paragraph, "en"), 50, "precondition: the rich text's later paragraphs made a real paragraph");
            Assert.AreEqual(ExpectedMinutes("en"), ElementRules.Minutes(_s.Scope, _s.Elements, "en", "en"));
            Assert.AreEqual(ExpectedMinutes("pt"), ElementRules.Minutes(_s.Scope, _s.Elements, "pt", "en"));
        }

        [Test]
        public void Minutes_SetByHand_Win_AndANothingPageIsZero()
        {
            PaintingStory.Value(_s.Block, BlockKindDefinition.MinutesField).number = 7f;
            Assert.AreEqual(7, ElementRules.Minutes(_s.Scope, _s.Elements, "en", "en"));
            PaintingStory.Value(_s.Block, BlockKindDefinition.MinutesField).number = 0f;
            _s.PageValue.items.Clear();
            Assert.AreEqual(0, ElementRules.Minutes(_s.Scope, _s.Elements, "en", "en"), "0 = Auto, and an empty Page takes no time");
            // - a single divider takes no time either; a broken link counts as nothing
            _s.AddPage(BuiltInElements.Divider);
            var link = _s.AddPage(BuiltInElements.Spacer);
            link.SetRef(ElementRef.ToPage("el_99"));
            Assert.AreEqual(0, ElementRules.Minutes(_s.Scope, _s.Elements, "en", "en"));
        }

        [Test]
        public void FirstWords_AreTheTitlesWords_ElseThePagesFirstWords_InTheLanguage_GlossaryMarksAsTheirWords()
        {
            // - what a story is called: its title, even after the eyebrow above it
            Assert.AreEqual(BlockFieldReader.Pick(PaintingStory.TextOf(_s.Header, BlockStackBuilder.HeaderTitleField), "en", "en"),
                ElementRules.FirstWords(_s.Scope, _s.Elements, "en", "en"), "the title row names the story");
            Assert.AreEqual(BlockFieldReader.Pick(PaintingStory.TextOf(_s.Header, BlockStackBuilder.HeaderTitleField), "pt", "en"),
                ElementRules.FirstWords(_s.Scope, _s.Elements, "pt", "en"));

            // - no title row: the first words (the picture has none: the eyebrow's)
            _s.PageValue.items.Remove(_s.Title.Data);
            Assert.AreEqual(BlockFieldReader.Pick(PaintingStory.TextOf(_s.Header, "eyebrow"), "en", "en"), ElementRules.FirstWords(_s.Scope, _s.Elements, "en", "en"),
                "with no title, the first row with words");
            _s.Eyebrow.Field(BuiltInElements.TextField).text[0].value = "[[Ribeira|ribeira palace]]  by\nthe river";
            Assert.AreEqual("Ribeira by the river", ElementRules.FirstWords(_s.Scope, _s.Elements, "en", "en"));
        }

        // The page breadcrumb's question (the S2c review): does the Page show a title of its own? A title-like row with words in the
        // language (or its fallback) -- a heading counts, an empty one does not, a link plays its target's part
        [Test]
        public void HasOwnTitle_IsATitleLikeRowWithWords_InTheLanguageOrItsFallback()
        {
            Assert.IsTrue(ElementRules.HasOwnTitle(_s.Scope, _s.Elements, "en", "en"), "the story's title row");
            Assert.IsTrue(ElementRules.HasOwnTitle(_s.Scope, _s.Elements, "pt", "en"), "in Portuguese too");
            var titleText = _s.Title.Field(BuiltInElements.TextField).text;
            titleText.RemoveAll(e => e.lang == "pt");
            Assert.IsTrue(ElementRules.HasOwnTitle(_s.Scope, _s.Elements, "pt", "en"), "a missing language falls back to the wall's first");

            _s.PageValue.items.Remove(_s.Title.Data);
            Assert.IsFalse(BlockSurfaces.PageRows(_s.Block, _s.Kind).Any(r => new ElementRow(r).Kind == BuiltInElements.Heading),
                "precondition: the fixture has no heading");
            Assert.IsFalse(ElementRules.HasOwnTitle(_s.Scope, _s.Elements, "en", "en"), "no title row: the page needs its name");

            var heading = _s.AddPage(BuiltInElements.Heading);
            Assert.IsFalse(ElementRules.HasOwnTitle(_s.Scope, _s.Elements, "en", "en"), "an empty heading is no title");
            PaintingStory.SetText(heading, BuiltInElements.TextField, new List<LocalizedEntry> { new() { lang = "en", value = "Two centuries" } });
            Assert.IsTrue(ElementRules.HasOwnTitle(_s.Scope, _s.Elements, "en", "en"), "a heading with words is one");

            _s.PageValue.items.Remove(heading.Data);
            _s.PageValue.items.Add(_s.Title.Data);
            var link = _s.AddPage(BuiltInElements.Spacer);
            link.SetRef(ElementRef.ToPage(_s.Title.Id));
            _s.PageValue.items.Remove(_s.Title.Data);
            Assert.IsFalse(ElementRules.HasOwnTitle(_s.Scope, _s.Elements, "en", "en"), "a link to a row that is gone shows nothing");
        }

        [Test]
        public void SearchWords_AreEveryLanguagesTextAndCaptions_NeverCredits_NoRepeats_ReferencesAddNothing()
        {
            var words = ElementRules.SearchWords(_s.Scope, _s.Elements);
            foreach (var entry in PaintingStory.TextOf(_s.Header, BlockStackBuilder.HeaderTitleField)) CollectionAssert.Contains(words, entry.value, "the title in " + entry.lang);
            foreach (var picture in _s.GalleryPictures())
                foreach (var caption in picture.fields.Single(f => f.key == BuiltInBlocks.GalleryCaptionField).text)
                    CollectionAssert.Contains(words, BlockSummaryRule.OneLine(caption.value), "a gallery caption in " + caption.lang);
            foreach (var media in _s.Carousel.Media)
                if (media.Credit.Length > 0) CollectionAssert.DoesNotContain(words, media.Credit, "a credit is not what a visitor searches for");
            Assert.AreEqual(words.Count, words.Distinct().Count(), "no repeats");

            // - a link adds nothing (its words are its target's); an own preview row adds its words; the same words twice are one entry
            int before = words.Count;
            _s.AddPreview(ElementRow.CreateRef(_s.Block, ElementRef.ToPage(_s.Title.Id)));
            var again = _s.AddPage(BuiltInElements.Title);
            PaintingStory.SetText(again, BuiltInElements.TextField, PaintingStory.TextOf(_s.Header, BlockStackBuilder.HeaderTitleField));
            Assert.AreEqual(before, ElementRules.SearchWords(_s.Scope, _s.Elements).Count);
            var teaser = ElementRow.Create(_s.Block, Element(BuiltInElements.Lede));
            PaintingStory.SetText(teaser, BuiltInElements.TextField, new List<LocalizedEntry> { new() { lang = "en", value = "A palace the sea took back." } });
            _s.AddPreview(teaser);
            CollectionAssert.Contains(ElementRules.SearchWords(_s.Scope, _s.Elements), "A palace the sea took back.");
        }

        [Test]
        public void TapOf_IsTheStoredTapWhenOffered_ElseTheElementsDefault_ElseNone()
        {
            var video = _s.AddPage(BuiltInElements.Video);
            Assert.AreEqual(ElementOptions.TapPlayInPlace, ElementRules.TapOf(video, Element(BuiltInElements.Video)), "born with the element's first tap");
            video.SetValue(ElementRow.TapKey, ElementOptions.TapOpenPlayer);
            Assert.AreEqual(ElementOptions.TapOpenPlayer, ElementRules.TapOf(video, Element(BuiltInElements.Video)));
            video.SetValue(ElementRow.TapKey, ElementOptions.TapOpenViewer);
            Assert.AreEqual(ElementOptions.TapPlayInPlace, ElementRules.TapOf(video, Element(BuiltInElements.Video)), "a tap the video does not offer");
            Assert.AreEqual(ElementOptions.TapNone, ElementRules.TapOf(_s.Carousel, Element(BuiltInElements.Carousel)), "a carousel only swipes");
        }

        [Test]
        public void Motion_AutoPlaysTheElementsOwn_AnUnofferedEffectToo_AndReduceMotionIsARule()
        {
            Assert.AreEqual(ElementOptions.MotionParallax, ElementMotionRule.EffectOf(_s.Picture, Element(BuiltInElements.Picture)), "a picture: a subtle parallax");
            Assert.AreEqual(ElementOptions.MotionReveal, ElementMotionRule.EffectOf(_s.Paragraph, Element(BuiltInElements.Paragraph)), "text: a reveal");
            Assert.AreEqual(ElementOptions.MotionNone, ElementMotionRule.EffectOf(_s.AddPage(BuiltInElements.Spacer), Element(BuiltInElements.Spacer)));
            _s.Picture.SetMotion(ElementOptions.MotionSlowZoom, ElementOptions.StrengthStrong);
            Assert.AreEqual(ElementOptions.MotionSlowZoom, ElementMotionRule.EffectOf(_s.Picture, Element(BuiltInElements.Picture)));
            Assert.AreEqual(ElementOptions.StrengthStrong, _s.Picture.Strength);
            _s.Paragraph.SetMotion(ElementOptions.MotionSlowZoom, ElementOptions.StrengthStrong);
            Assert.AreEqual(ElementOptions.MotionReveal, ElementMotionRule.EffectOf(_s.Paragraph, Element(BuiltInElements.Paragraph)), "text never zooms");
            _s.Paragraph.SetMotion("wobble", "huge");
            Assert.AreEqual(ElementOptions.MotionAuto, _s.Paragraph.Motion, "an unknown stored effect reads as Auto");
            Assert.AreEqual(ElementOptions.StrengthSubtle, _s.Paragraph.Strength);

            // - Reduce Motion: every effect still, the reveal included (the row is simply there: identical frames, _3.3 S2b)
            foreach (string moving in new[] { ElementOptions.MotionNone, ElementOptions.MotionReveal, ElementOptions.MotionSlideIn, ElementOptions.MotionParallax, ElementOptions.MotionSlowZoom, ElementOptions.MotionPan })
                Assert.AreEqual(ElementMotionRule.Reduced.Still, ElementMotionRule.ReducedOf(moving), moving);
        }

        // ---------------- the seams: the card's own rules read the rows ----------------

        [Test]
        public void TheCardsReadingTimeAndSummaryRules_ReadAComposedBlocksRows()
        {
            Assert.AreEqual(ExpectedMinutes("en"), ReadingTimeRule.Minutes(_s.Kind, _s.Block, 0f), "the contents list's and section's minutes");
            Assert.AreEqual(BlockFieldReader.Pick(PaintingStory.TextOf(_s.Header, BlockStackBuilder.HeaderTitleField), "pt", "en"), BlockSummaryRule.FirstWords(_s.Block, _s.Kind, "pt", "en"));
            PaintingStory.Value(_s.Block, BlockKindDefinition.HeadingField).text = new List<LocalizedEntry> { new() { lang = "en", value = "The story" } };
            Assert.AreEqual("The story", BlockSummaryRule.FirstWords(_s.Block, _s.Kind, "en", "en"), "a heading still comes first");

            // - an Open Row's words describe a button, never the block: a Page with no words gives no summary, whatever the override says
            var bare = new BlockInstanceData { key = "block_bare", kind = PaintingStory.StoryKindKey };
            var open = new BlockItemData();
            open.fields.Add(new BlockItemFieldValue { key = BlockSurfaces.WordsKey, text = new List<LocalizedEntry> { new() { lang = "en", value = "Read the story" } } });
            PaintingStory.Value(bare, BlockSurfaces.ComponentOverridesField).items.Add(open);
            Assert.AreEqual("", BlockSummaryRule.FirstWords(bare, _s.Kind, "en", "en"));
        }

        [Test]
        public void TheStackBuilder_KeepsTheStory_AndItsCompactRowSaysItsFirstWordsAndMinutes()
        {
            var built = BlockStackBuilder.Build(_s.Painting, _s.Config.card_settings, _s.Blocks, _s.Config.pois);
            var entry = built.Entries.Single(e => e.Instance.key == PaintingStory.StoryBlockKey);
            var table = AssetDatabase.LoadAssetAtPath<CardStringTable>("Assets/Framework/Runtime/UI/Screen/Cards/CardStrings.asset").Entries();
            var row = CompactRowRule.Of(entry, _s.Blocks, "en", "en", new CardStrings(table, null, null, "en", "en").Get);
            Assert.AreEqual(BlockFieldReader.Pick(PaintingStory.TextOf(_s.Header, BlockStackBuilder.HeaderTitleField), "en", "en"), row.Words, "the compact row names the story by its title");
            Assert.IsTrue(row.FromFirstWords);
            Assert.AreEqual(ExpectedMinutes("en"), row.Minutes);
            StringAssert.Contains(ExpectedMinutes("en").ToString(), row.Meta, "the row's second line names the minutes");
        }

        [Test]
        public void TheStackBuilder_SkipsAStoryWithNoRowItCanShow_AndResolvesLinksOnThePoi()
        {
            // - no rows: the required Page is empty
            _s.PageValue.items.Clear();
            AssertSkipped(BlockStackBuilder.SkipReason.MissingRequired);
            // - rows, none complete: a picture with no picture, a paragraph with no words, a link to a row that is gone
            _s.AddPage(BuiltInElements.Picture);
            _s.AddPage(BuiltInElements.Paragraph);
            _s.AddPage(BuiltInElements.Spacer).SetRef(ElementRef.ToPage("el_99"));
            AssertSkipped(BlockStackBuilder.SkipReason.NoCompleteRow);
            // - a link to the second block's heading IS a row the card can show: resolved on The Painting's own card
            _s.AddPage(BuiltInElements.Spacer).SetRef(ElementRef.ToBlock(PaintingStory.SecondBlockKey, _s.SecondHeading.Id));
            var built = BlockStackBuilder.Build(_s.Painting, _s.Config.card_settings, _s.Blocks, _s.Config.pois);
            Assert.IsTrue(built.Entries.Any(e => e.Instance.key == PaintingStory.StoryBlockKey));
        }

        private void AssertSkipped(BlockStackBuilder.SkipReason reason)
        {
            var built = BlockStackBuilder.Build(_s.Painting, _s.Config.card_settings, _s.Blocks, _s.Config.pois);
            Assert.IsFalse(built.Entries.Any(e => e.Instance.key == PaintingStory.StoryBlockKey), "the story is not shown");
            var skipped = built.Skipped.Single(k => k.Instance.key == PaintingStory.StoryBlockKey);
            Assert.AreEqual(reason, skipped.Reason);
            Assert.AreEqual(PaintingStory.PageField, skipped.FieldKey);
        }

        private static ElementKindDefinition Element(string key) => ElementRegistry.Shared.TryGet(key, out var e) ? e : null;
    }
}

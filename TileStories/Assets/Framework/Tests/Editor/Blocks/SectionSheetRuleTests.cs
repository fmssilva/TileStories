using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;

namespace TileStories.Tests
{
    // The card's chapters (_3.3 P4.4c1, R10.7, SectionSheetRule): runs of one family, bare runs for the navigation family, which runs show
    // their opener / are laid over the one before when a layout hides some, and the opener's meta line in the card's real words (EN + PT).
    // The real card's sheets (The Lamp's interleaved families, both modes, the layouts) are SectionSheetSceneTests.
    public class SectionSheetRuleTests
    {
        private List<CardStringEntry> _table;

        [SetUp]
        public void LoadTheCardsWords() =>
            _table = AssetDatabase.LoadAssetAtPath<CardStringTable>("Assets/Framework/Runtime/UI/Screen/Cards/CardStrings.asset").Entries();

        private System.Func<string, string> Words(string language) => new CardStrings(_table, null, null, language, "en").Get;

        // The registry's own answer: the built-in families, as registered
        private static bool OpensSection(string family) =>
            !BlockRegistry.Shared.TryGetFamily(family, out var definition) || definition.OpensSection;

        private static List<SectionSheetRule.Run> Runs(params string[] families) => SectionSheetRule.Runs(families, OpensSection);

        [Test]
        public void ConsecutiveBlocksOfOneFamily_AreOneRun_ANewRunStartsWhereverTheFamilyChanges()
        {
            var runs = Runs("about", "about", "about", "stories", "stories", "media");
            Assert.AreEqual(new[] { "about", "stories", "media" }, runs.Select(r => r.Family).ToArray(), "one run per family, in the stack's order");
            Assert.AreEqual(new[] { 0, 3, 5 }, runs.Select(r => r.Start).ToArray(), "each run starts where its family does");
            Assert.AreEqual(new[] { 3, 2, 1 }, runs.Select(r => r.Count).ToArray(), "and holds every consecutive block of it");
            // - interleaved families (a block order that is not by family): The Lamp's visit -> media -> visit comes back as its own run
            var interleaved = Runs("stories", "visit", "media", "media", "visit", "visit", "about");
            Assert.AreEqual(new[] { "stories", "visit", "media", "visit", "about" }, interleaved.Select(r => r.Family).ToArray(),
                "a family that comes back opens a new run");
            Assert.AreEqual(7, interleaved.Sum(r => r.Count), "every block is in exactly one run");
            Assert.IsEmpty(Runs(), "no blocks, no runs");
        }

        [Test]
        public void TheNavigationFamily_IsABareRun_EveryContentFamilyOpensASection()
        {
            Assert.IsTrue(BlockRegistry.Shared.TryGetFamily(BuiltInFamilies.Navigation, out var navigation) && !navigation.OpensSection,
                "precondition: the navigation family opens no section (its blocks ARE the card's navigation)");
            var runs = Runs("navigation", "about", "meta");
            Assert.AreEqual(new[] { true, false, false }, runs.Select(r => r.Bare).ToArray(),
                "the nav blocks sit on the plain ground; About and the Sources (meta) are sections");
            foreach (var family in BlockRegistry.Shared.Families.Where(f => f.Key != BuiltInFamilies.Navigation))
                Assert.IsTrue(family.OpensSection, family.Key + ": a content family opens a section");
        }

        [Test]
        public void AShownRun_ShowsItsOpener_AndIsLaidOverTheOneBefore_UnlessBareOrContinuingItsFamily()
        {
            var runs = Runs("navigation", "about", "stories", "about", "media");
            var all = SectionSheetRule.Looks(runs, new[] { true, true, true, true, true });
            Assert.AreEqual(new[] { false, true, true, true, true }, all.Select(l => l.Opener).ToArray(), "every section opens with its pill; the bare nav run has none");
            Assert.AreEqual(new[] { false, false, true, true, true }, all.Select(l => l.Overlaps).ToArray(),
                "a sheet is laid over the sheet before it -- never over the bare nav run, and the first sheet over nothing");
            // - a layout that sets aside the stories run (tabs / pages: one section shown) leaves About's two runs next to each other: the
            //   second CONTINUES the first -- no second opener, not laid over it
            var aside = SectionSheetRule.Looks(runs, new[] { true, true, false, true, true });
            Assert.IsFalse(aside[2].Shown, "a run with no shown block is not shown");
            Assert.IsTrue(aside[3].Continues, "About after About reads as one chapter");
            Assert.IsFalse(aside[3].Opener, "so no second About opener");
            Assert.IsFalse(aside[3].Overlaps, "and no sheet laid over its own family");
            Assert.IsTrue(aside[4].Opener && aside[4].Overlaps, "Media after it opens and lays over as usual");
            // - nothing shown before: the first shown run lays over nothing
            var late = SectionSheetRule.Looks(runs, new[] { false, false, false, true, true });
            Assert.IsTrue(late[3].Opener && !late[3].Overlaps && !late[3].Continues, "the first shown run opens, over nothing");
        }

        [Test]
        public void TheOpenersMetaLine_CountsTheBlocks_AndAddsTheirMinutes_InTheCardsWords()
        {
            Assert.AreEqual("1 block", SectionSheetRule.Meta(1, 0, Words("en")), "one block, no minutes given");
            Assert.AreEqual("4 blocks", SectionSheetRule.Meta(4, 0, Words("en")));
            Assert.AreEqual("4 blocks · 9 min", SectionSheetRule.Meta(4, 9, Words("en")), "the minutes joined by the compact row's own separator");
            Assert.AreEqual("1 bloco", SectionSheetRule.Meta(1, 0, Words("pt")), "Portuguese");
            Assert.AreEqual("3 blocos · 2 min", SectionSheetRule.Meta(3, 2, Words("pt")));
            Assert.AreEqual("", SectionSheetRule.Meta(2, 0, null), "no words: nothing is invented");
        }

        [Test]
        public void ASectionsColours_AreNamedFromItsFamilysColourToken()
        {
            Assert.IsTrue(BlockRegistry.Shared.TryGetFamily("media", out var media), "precondition: the media family");
            Assert.AreEqual("--ts-family-media-ink", SectionSheetRule.InkOf(media.ColorToken), "the words on the pill");
            Assert.AreEqual("--ts-family-media-glass", SectionSheetRule.GlassOf(media.ColorToken), "the sheet's glass");
        }
    }
}

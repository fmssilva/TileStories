using System.Linq;
using NUnit.Framework;
using TileStories.Tests;

namespace TileStories.Editor.Tests
{
    // The golden card stays whole (_Test_Suite_2.md 1.3; 00-process "Real fixtures, one home"), read from the SHIPPED config through the
    // real registry and builder: every registered kind on the Ribeira Palace (save the named ones), no kind + look twice, every block
    // drawn; BlockOf finds each kind's block, a look among several, and names what is missing; the edge cards are what their names say.
    // A kind registered tomorrow fails here until the golden card holds it.
    public class ReferenceCardTests
    {
        private WallConfigData _config;
        private POIData _golden;

        [SetUp]
        public void LoadTheShippedWall()
        {
            _config = ReferenceCard.Config();
            _golden = ReferenceCard.GoldenPoi(_config);
        }

        [Test]
        public void EveryRegisteredKind_IsOnTheGoldenCard_SaveTheNamedOnes_AndNoNamedOneIsStale()
        {
            var onCard = _golden.card.blocks.Select(b => b.kind).ToHashSet();
            var registered = BlockRegistry.Shared.All.Select(k => k.Key).ToList();
            Assert.Greater(registered.Count, 30, "precondition: the framework's kinds (and the app's) are registered");
            var missing = registered.Where(k => !onCard.Contains(k) && !ReferenceCard.KeptOffTheGoldenCard.ContainsKey(k)).ToList();
            CollectionAssert.IsEmpty(missing, "every registered kind is on the golden card: add a block of each to the Ribeira Palace");
            foreach (var named in ReferenceCard.KeptOffTheGoldenCard)
            {
                Assert.IsTrue(registered.Contains(named.Key), named.Key + " is no longer registered: drop its line from KeptOffTheGoldenCard");
                Assert.IsFalse(onCard.Contains(named.Key), named.Key + " is on the golden card now: drop its line from KeptOffTheGoldenCard");
            }
        }

        [Test]
        public void NoKindAndLookIsOnTheGoldenCardTwice()
        {
            var twice = _golden.card.blocks.GroupBy(b => b.kind + ":" + ReferenceCard.LookOf(b)).Where(g => g.Count() > 1)
                .Select(g => g.Key + " x" + g.Count()).ToList();
            CollectionAssert.IsEmpty(twice, "one block per kind and look: a look matrix belongs to the Phase A gallery");
        }

        [Test]
        public void EveryBlockOfTheGoldenCard_IsDrawn_NoneSkipped()
        {
            var result = BlockStackBuilder.Build(_golden, _config.card_settings, BlockRegistry.Shared, _config.pois);
            CollectionAssert.IsEmpty(result.Skipped.Select(s => $"{s.Instance.key} ({s.Instance.kind}): {s.Reason} {s.FieldKey}"),
                "the golden card's content is complete: no block skipped");
            Assert.AreEqual(_golden.card.blocks.Count, result.Entries.Count, "every block an entry of the stack");
        }

        [Test]
        public void BlockOf_FindsEachKindsBlock_ALookAmongSeveral_AndNamesWhatIsMissing()
        {
            foreach (var kind in _golden.card.blocks.Select(b => b.kind).Distinct())
            {
                string key = ReferenceCard.BlockOf(kind);
                Assert.AreEqual(kind, _golden.card.blocks.Single(b => b.key == key).kind, kind + ": BlockOf hands that kind's block");
            }
            // - three looks of the quiz: the default without a look, each look by its name
            var quizzes = _golden.card.blocks.Where(b => b.kind == BuiltInBlocks.KnowledgeCheckKind).ToList();
            Assert.Greater(quizzes.Count, 1, "precondition: the golden card carries several looks of the quiz");
            Assert.AreEqual(BuiltInBlocks.KnowledgeCheck.DefaultVariant,
                ReferenceCard.LookOf(quizzes.Single(b => b.key == ReferenceCard.BlockOf(BuiltInBlocks.KnowledgeCheckKind))), "no look named: the default look");
            foreach (var quiz in quizzes)
                Assert.AreEqual(quiz.key, ReferenceCard.BlockOf(BuiltInBlocks.KnowledgeCheckKind, ReferenceCard.LookOf(quiz)), "named: that look's block");
            var missing = Assert.Throws<AssertionException>(() => ReferenceCard.BlockOf(BuiltInBlocks.NavContentsKind));
            StringAssert.Contains("Ribeira Palace has 0 nav_contents", missing.Message, "a missing kind is named in the failure");
        }

        [Test]
        public void TheShowAfterReadingCase_LivesOnTheGoldenCard()
        {
            var quiz = _golden.card.blocks.Single(b => b.key == ReferenceCard.BlockOf(BuiltInBlocks.KnowledgeCheckKind, BuiltInBlocks.KnowledgeCheckTrueFalseSwipe));
            Assert.IsTrue(new BlockFieldReader(quiz, null, null).Flag(BuiltInBlocks.KnowledgeCheckShowAfterViewedField),
                "the true / false quiz waits until the card is read");
        }

        [Test]
        public void TheSecondAndEdgeCards_AreWhatTheirNamesSay()
        {
            POIData Point(string id) => _config.pois.Single(p => p.id == id);
            int Count(string id, string kind) => Point(id).card?.blocks.Count(b => b.kind == kind) ?? 0;
            Assert.Greater(Point(ReferenceCard.SecondCard).card.blocks.Count, 1, "the second card is a real card");
            Assert.AreEqual(BuiltInBlocks.HeaderVideoLoop, Point(ReferenceCard.Edge.VideoLoopHero).card.blocks.Single(b => b.kind == BuiltInBlocks.HeaderKind).variant);
            Assert.AreEqual(BuiltInBlocks.HeaderModelTurntable, Point(ReferenceCard.Edge.TurntableHero).card.blocks.Single(b => b.kind == BuiltInBlocks.HeaderKind).variant);
            Assert.AreEqual(0, Count(ReferenceCard.Edge.NoHeader, BuiltInBlocks.HeaderKind), "no header");
            Assert.AreEqual(1, Count(ReferenceCard.Edge.NoHeader, BuiltInBlocks.NavContentsKind), "and the kept Contents block");
            Assert.AreEqual(1, Count(ReferenceCard.Edge.AudioOnly, BuiltInBlocks.AudioGuideKind), "one audio guide");
            Assert.AreEqual(0, Point(ReferenceCard.Edge.NoCard).card?.blocks.Count ?? 0, "no card");
            Assert.IsTrue(Point(ReferenceCard.Edge.NoCard).status_unknown, "its status unknown");
            Assert.AreEqual(0, Point(ReferenceCard.Edge.EmptyPoint).card?.blocks.Count ?? 0, "the empty point");
        }
    }
}

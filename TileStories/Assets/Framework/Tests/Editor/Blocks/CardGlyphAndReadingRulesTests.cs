using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // The pure rules of step 8A-fix (_3.1): the swipe's grab rule, the vector glyphs' outlines, and the show-after-reading placement
    // rule with the Editor warning that uses it.
    public class CardGlyphAndReadingRulesTests
    {
        // ---------------- SwipeGrabRule ----------------

        [Test]
        public void SwipeGrab_NeedsMoreThanTheSlop_AndAClearlySidewaysDrag()
        {
            Assert.IsFalse(SwipeGrabRule.ClaimsPointer(Vector2.zero), "a finger that has not moved");
            Assert.IsFalse(SwipeGrabRule.ClaimsPointer(new Vector2(SwipeGrabRule.Slop, 0f)), "exactly the slop: not yet");
            Assert.IsTrue(SwipeGrabRule.ClaimsPointer(new Vector2(SwipeGrabRule.Slop + 1f, 0f)), "sideways past the slop");
            Assert.IsTrue(SwipeGrabRule.ClaimsPointer(new Vector2(-SwipeGrabRule.Slop - 1f, 3f)), "sideways to the left, a little off level");
            Assert.IsFalse(SwipeGrabRule.ClaimsPointer(new Vector2(0f, 200f)), "straight down: the stack's to scroll");
            Assert.IsFalse(SwipeGrabRule.ClaimsPointer(new Vector2(40f, 40f)), "as far down as across: not clearly sideways");
            Assert.IsFalse(SwipeGrabRule.ClaimsPointer(new Vector2(30f, -60f)), "mostly up: the stack's to scroll");
            Assert.IsTrue(SwipeGrabRule.ClaimsPointer(new Vector2(41f, 40f)), "just more across than down");
        }

        // ---------------- CardIcons vector glyphs ----------------

        [Test]
        public void EveryVectorShape_HasOutlines_InsideTheUnitSquare()
        {
            foreach (CardIcons.Shape shape in System.Enum.GetValues(typeof(CardIcons.Shape)))
            {
                var outlines = CardIcons.OutlinesOf(shape);
                Assert.IsNotEmpty(outlines, shape + " draws something");
                Assert.Greater(CardIcons.StrokeUnits(shape), 0f, shape + " has a stroke");
                foreach (var outline in outlines)
                {
                    Assert.GreaterOrEqual(outline.Points.Length, 2, shape + ": an outline joins points");
                    foreach (var p in outline.Points)
                    {
                        Assert.That(p.x, Is.InRange(0f, 1f), shape + " x in 0..1");
                        Assert.That(p.y, Is.InRange(0f, 1f), shape + " y in 0..1");
                    }
                }
            }
        }

        [Test]
        public void TheStar_HasFivePoints_TheTickAndCrossAreOpenLines_TheThumbsAreClosedAndMirrored()
        {
            var star = CardIcons.OutlinesOf(CardIcons.Shape.Star).Single();
            Assert.IsTrue(star.Closed, "a star can be filled");
            Assert.AreEqual(10, star.Points.Length, "five outer and five inner points");
            Assert.AreEqual(0.5f, star.Points[0].x, 1e-4f, "the first point is straight up");
            Assert.Less(star.Points[0].y, star.Points[1].y, "...and it is the highest");

            Assert.IsFalse(CardIcons.OutlinesOf(CardIcons.Shape.Tick).Single().Closed, "a tick is a line");
            var cross = CardIcons.OutlinesOf(CardIcons.Shape.Cross);
            Assert.AreEqual(2, cross.Count, "a cross is two lines");
            Assert.IsTrue(cross.All(o => !o.Closed));

            var up = CardIcons.OutlinesOf(CardIcons.Shape.ThumbUp);
            var down = CardIcons.OutlinesOf(CardIcons.Shape.ThumbDown);
            Assert.AreEqual(2, up.Count, "the hand and the cuff");
            Assert.IsTrue(up.All(o => o.Closed));
            Assert.AreEqual(up.Count, down.Count);
            for (int i = 0; i < up.Count; i++)
            {
                Assert.AreEqual(up[i].Points.Length, down[i].Points.Length);
                for (int k = 0; k < up[i].Points.Length; k++)
                {
                    Assert.AreEqual(up[i].Points[k].x, down[i].Points[k].x, 1e-5f, "the thumb down is the thumb up turned over: same x");
                    Assert.AreEqual(1f - up[i].Points[k].y, down[i].Points[k].y, 1e-5f, "...mirrored in y");
                }
            }
            float upTop = up.SelectMany(o => o.Points).Min(p => p.y);
            Assert.Less(upTop, 0.2f, "the thumb up points up");
        }

        // ---------------- show after reading ----------------

        [Test]
        public void ABlockThatWaitsForTheReading_IsInView_OnlyWhenNothingButMetaFollowsIt()
        {
            Assert.IsTrue(ContentSeenRule.RevealsInView(new string[0]), "the last block of the card");
            Assert.IsTrue(ContentSeenRule.RevealsInView(new[] { "meta" }), "only sources after it");
            Assert.IsTrue(ContentSeenRule.RevealsInView(new[] { "meta", "meta" }));
            Assert.IsFalse(ContentSeenRule.RevealsInView(new[] { "about" }), "content after it");
            Assert.IsFalse(ContentSeenRule.RevealsInView(new[] { "meta", "play" }), "content after a meta block");
            Assert.IsFalse(ContentSeenRule.RevealsInView(new[] { "" }), "a kind nobody registered counts as content");
        }

        private static BlockInstanceData Block(string key, string kind, bool showAfterReading = false)
        {
            var block = new BlockInstanceData { key = key, kind = kind, variant = "", display = "inline" };
            if (showAfterReading) block.fields.Add(new BlockFieldValue { key = BuiltInBlocks.KnowledgeCheckShowAfterViewedField, flag = true });
            return block;
        }

        [Test]
        public void TheCardContentWarns_OfAShowAfterReadingBlock_ThatIsNotTheLastContentBlockBeforeMeta()
        {
            var settings = new CardSettings();
            var poi = new POIData { id = "p", name = "Tower" };
            var header = Block("block_1", BuiltInBlocks.HeaderKind);
            var quiz = Block("block_2", BuiltInBlocks.KnowledgeCheckKind, showAfterReading: true);
            var facts = Block("block_3", BuiltInBlocks.QuickFactsKind);
            var sources = Block("block_4", BuiltInBlocks.SourcesKind);
            poi.card.blocks.AddRange(new[] { header, quiz, facts, sources });

            var warnings = POIEditorToolWindow.CardBlockWarnings(quiz, BuiltInBlocks.KnowledgeCheck, settings, poi);
            Assert.AreEqual(POIEditorToolWindow.CardShowAfterReadingMidCardNote, warnings.Single(), "quick facts follow it: warned");

            poi.card.blocks.Clear();
            poi.card.blocks.AddRange(new[] { header, facts, quiz, sources });
            CollectionAssert.IsEmpty(POIEditorToolWindow.CardBlockWarnings(quiz, BuiltInBlocks.KnowledgeCheck, settings, poi), "only sources follow it: fine");

            poi.card.blocks.Clear();
            poi.card.blocks.AddRange(new[] { header, facts, quiz });
            CollectionAssert.IsEmpty(POIEditorToolWindow.CardBlockWarnings(quiz, BuiltInBlocks.KnowledgeCheck, settings, poi), "the very last block: fine");

            var plain = Block("block_2", BuiltInBlocks.KnowledgeCheckKind);
            poi.card.blocks.Clear();
            poi.card.blocks.AddRange(new[] { header, plain, facts });
            CollectionAssert.IsEmpty(POIEditorToolWindow.CardBlockWarnings(plain, BuiltInBlocks.KnowledgeCheck, settings, poi), "Show After Reading is off: nothing to say");
        }

        [Test]
        public void TheShowAfterReadingWarning_NamesNoInternalId()
        {
            StringAssert.DoesNotContain("block_", POIEditorToolWindow.CardShowAfterReadingMidCardNote);
            StringAssert.Contains("Show After Reading", POIEditorToolWindow.CardShowAfterReadingMidCardNote, "the toggle's own label");
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace TileStories.Tests
{
    // Phase A of the swipe tracks (_3.1 15.3.4, 40-testing.md 4.4): the horizontal timeline and the related carousel on the real gallery
    // card. A track must never show a card cut in half at rest or at the end of the swipe (the audit read "1755 / The" and "Lamp -" as
    // broken), so the strip ends on a card edge: the cells are stretched to a whole number (CardTrackRule) and every title is whole.
    public class CardTrackGalleryTests
    {
        private const string ScenePath = "Assets/Dev/CardGallery/CardGalleryScene.unity";
        private CardGalleryHarness _harness;

        private static int IndexOf(string name) => CardGalleryDefinitions.All.ToList().FindIndex(e => e.Name == name);

        [UnitySetUp]
        public IEnumerator SetUp()
        {
#if UNITY_EDITOR
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
#else
            Assert.Ignore("Needs the Editor to load the gallery scene by path.");
#endif
            for (int i = 0; i < 60 && _harness == null; i++)
            {
                _harness = Object.FindFirstObjectByType<CardGalleryHarness>();
                yield return null;
            }
            Assert.IsNotNull(_harness, "the gallery scene holds CardGalleryHarness");
            _harness.EnsureBuilt();
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            StatusRamp.ResetToDefaults();
            CategoryPalette.ClearOverrides();
            MarkerHierarchyResolver.ResetToDefaults();
            var scene = SceneManager.GetSceneByPath(ScenePath);
            SceneManager.SetActiveScene(SceneManager.CreateScene("AfterCardTrackTest"));
            if (scene.IsValid() && scene.isLoaded) yield return SceneManager.UnloadSceneAsync(scene);
        }

        // No cell may overlap the strip's edge without being inside it: a cell is wholly in view or wholly out
        private static void AssertNoneCut(ScrollView track, IReadOnlyList<VisualElement> cells, string when)
        {
            Rect strip = track.contentViewport.worldBound;
            float tol = CardGalleryChecks.OnePixel(track) * 2f + 0.5f;
            for (int i = 0; i < cells.Count; i++)
            {
                Rect r = cells[i].worldBound;
                bool inView = r.xMax > strip.xMin + tol && r.xMin < strip.xMax - tol;
                if (inView)
                    Assert.IsTrue(r.xMin >= strip.xMin - tol && r.xMax <= strip.xMax + tol, when + ": cell " + i + " is cut by the strip: " + r + " in " + strip);
            }
        }

        // Some cell ends exactly where the strip does (the gap after a card is part of the strip it fills)
        private static void AssertStripEndsOnACard(ScrollView track, IReadOnlyList<VisualElement> cells, float gap, string when)
        {
            Rect strip = track.contentViewport.worldBound;
            float tol = CardGalleryChecks.OnePixel(track) * 2f + 0.5f;
            Assert.IsTrue(cells.Any(c => Mathf.Abs(c.worldBound.xMax + gap - strip.xMax) <= tol),
                when + ": the strip's right edge (" + strip.xMax + ") is a card's edge; the cards end at " + string.Join(", ", cells.Select(c => c.worldBound.xMax)));
        }

        private static IEnumerator ScrollToEnd(ScrollView track)
        {
            track.scrollOffset = new Vector2(track.horizontalScroller.highValue, 0f);
            yield return CardTestInput.Settle(0.2f);
        }

        // ---------------- timeline ----------------

        [UnityTest]
        public IEnumerator TheHorizontalTimeline_NeverShowsAHalfEvent_AtRestOrAtTheEndOfTheSwipe([Values("timeline_horizontal_long", "timeline_horizontal_short")] string name)
        {
            TimelineBlockView timeline = null;
            _harness.Show(IndexOf(name));
            yield return CardGalleryChecks.SettledBlock(_harness, name, v => timeline = (TimelineBlockView)v);
            var boxes = timeline.Events.Select(e => e.Box).ToList();
            var track = timeline.Track;
            bool scrolls = track.horizontalScroller.highValue > 0f;
            if (name.EndsWith("long")) Assert.IsTrue(scrolls, name + ": precondition: the long line is longer than the strip (it scrolls)");
            else Assert.GreaterOrEqual(boxes.Count, 2, name + ": precondition: a short line still has events to lay out");

            float narrowest = CardTestInput.TokenPx("--ts-timeline-cell");
            float onePixel = CardGalleryChecks.OnePixel(track);
            Assert.GreaterOrEqual(boxes[0].worldBound.width, narrowest - onePixel, "an event is never narrower than the cell token");
            Assert.Less(boxes[0].worldBound.width, narrowest * 2f, "and not absurdly wide");
            Assert.AreEqual(boxes[0].worldBound.width, boxes[1].worldBound.width, onePixel + 0.01f, "every event the same width");

            AssertNoneCut(track, boxes, "at rest");
            AssertStripEndsOnACard(track, boxes, 0f, "at rest");
            Assert.Greater(boxes.Count(b => b.worldBound.xMax <= track.contentViewport.worldBound.xMax + 1f && b.worldBound.xMin >= track.contentViewport.worldBound.xMin - 1f), 0, "at least one whole event");
            yield return CardGalleryChecks.Render("Card_" + name + "_rest");

            if (!scrolls) yield break;
            yield return ScrollToEnd(track);
            AssertNoneCut(track, boxes, "at the end of the swipe");
            AssertStripEndsOnACard(track, boxes, 0f, "at the end of the swipe");
            Rect strip = track.contentViewport.worldBound;
            Assert.AreEqual(strip.xMax, boxes.Last().worldBound.xMax, CardGalleryChecks.OnePixel(track) * 2f + 0.5f, "the swipe ends on the last event, whole");
            yield return CardGalleryChecks.Render("Card_" + name + "_end");
        }

        // ---------------- related ----------------

        [UnityTest]
        public IEnumerator TheRelatedCarousel_NeverShowsAHalfCard_AtRestOrAtTheEnd_AndEveryTitleIsWhole([Values("related_carousel_nearest", "related_carousel_manual")] string name)
        {
            RelatedBlockView related = null;
            _harness.Show(IndexOf(name));
            yield return CardGalleryChecks.SettledBlock(_harness, name, v => related = (RelatedBlockView)v);
            var boxes = related.Cards.Select(c => c.Box).ToList();
            Assert.GreaterOrEqual(boxes.Count, 2, name + ": precondition: a strip of cards");
            var track = related.Track;
            float gap = boxes[0].resolvedStyle.marginRight;
            Assert.Greater(gap, 0f, "precondition: the cards have a gap");

            float narrowest = CardTestInput.TokenPx("--ts-related-card");
            Assert.GreaterOrEqual(boxes[0].worldBound.width, narrowest - CardGalleryChecks.OnePixel(track), "a card is never narrower than the card token");

            AssertNoneCut(track, boxes, "at rest");
            AssertStripEndsOnACard(track, boxes, gap, "at rest");
            AssertTitlesWhole(related);
            yield return CardGalleryChecks.Render("Card_" + name + "_rest");

            if (track.horizontalScroller.highValue > 0f)
            {
                yield return ScrollToEnd(track);
                AssertNoneCut(track, boxes, "at the end of the swipe");
                AssertStripEndsOnACard(track, boxes, gap, "at the end of the swipe");
                AssertTitlesWhole(related);
                yield return CardGalleryChecks.Render("Card_" + name + "_end");
            }
        }

        // A picture-less card shows its whole title: the label lies inside its card, and its text is laid out in full (no clipped line)
        private static void AssertTitlesWhole(RelatedBlockView related)
        {
            foreach (var card in related.Cards)
            {
                Rect box = card.Box.worldBound, title = card.Title.worldBound;
                float tol = CardGalleryChecks.OnePixel(card.Box) * 2f + 0.5f;
                Assert.IsFalse(string.IsNullOrEmpty(card.Title.text), "precondition: the card has a title");
                Assert.GreaterOrEqual(title.xMin, box.xMin - tol, card.Title.text + ": inside its card (left)");
                Assert.LessOrEqual(title.xMax, box.xMax + tol, card.Title.text + ": inside its card (right)");
                Assert.GreaterOrEqual(title.yMin, box.yMin - tol, card.Title.text + ": inside its card (top)");
                Assert.LessOrEqual(title.yMax, box.yMax + tol, card.Title.text + ": inside its card (bottom)");
                var needs = card.Title.MeasureTextSize(card.Title.text, title.width, VisualElement.MeasureMode.Exactly, 0f, VisualElement.MeasureMode.Undefined);
                Assert.GreaterOrEqual(title.height + tol, needs.y, card.Title.text + ": the label is tall enough for all its lines: " + title.height + " vs " + needs.y);
            }
        }
    }
}

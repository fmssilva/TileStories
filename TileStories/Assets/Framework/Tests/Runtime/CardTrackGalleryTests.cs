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
    // Phase A of the swipe tracks (_3.1 15.3.4, 15.4.6, 40-testing.md 4.4): the horizontal timeline and the related carousel on the real
    // gallery card, both ways of Peek Next Card (card_settings.container.peek_next_card). OFF: a track never shows a card cut in half at rest
    // or at the end of the swipe (the audit read "1755 / The" and "Lamp -" as broken), so the strip ends on a card edge: the cells are
    // stretched to a whole number (CardTrackRule) and every title is whole. ON (the default): at rest the strip shows its whole cells and a
    // slice of the next one exactly as wide as the --ts-track-peek token, and the swipe still ends on a whole last card.
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

        // Show a gallery entry with Peek Next Card set: the entry's own fabricated wall and its setup, then the option
        private IEnumerator ShowWithPeek(string name, bool peek, System.Action<IBlockView> got)
        {
            var entry = CardGalleryDefinitions.All[IndexOf(name)];
            var wall = CardGalleryDefinitions.Taxonomy();
            entry.WallSetup?.Invoke(wall);
            wall.card_settings.container.peek_next_card = peek;
            _harness.ShowPoi(CardGalleryDefinitions.Poi(entry), wall, entry.Viewer, entry.Stop);
            yield return CardGalleryChecks.SettledBlock(_harness, name, got);
        }

        // At rest with Peek Next Card: every cell wholly in view except the one the strip's right edge cuts, and of that one exactly the
        // token's slice shows
        private static void AssertSliceOfTheNext(ScrollView track, IReadOnlyList<VisualElement> cells, string when)
        {
            Rect strip = track.contentViewport.worldBound;
            float tol = CardGalleryChecks.OnePixel(track) * 2f + 0.5f;
            var cut = cells.Where(c => c.worldBound.xMin < strip.xMax - tol && c.worldBound.xMax > strip.xMax + tol).ToList();
            Assert.AreEqual(1, cut.Count, when + ": exactly one card is cut by the strip's right edge (the next one): " + string.Join(", ", cells.Select(c => c.worldBound.xMin + ".." + c.worldBound.xMax)) + " in " + strip);
            Assert.AreEqual(CardTestInput.TokenPx("--ts-track-peek"), strip.xMax - cut[0].worldBound.xMin, tol, when + ": the slice shown is the --ts-track-peek token");
            foreach (var c in cells.Where(c => c != cut[0] && c.worldBound.xMax > strip.xMin + tol && c.worldBound.xMin < strip.xMax - tol))
                Assert.IsTrue(c.worldBound.xMin >= strip.xMin - tol && c.worldBound.xMax <= strip.xMax + tol, when + ": every other card in view is whole: " + c.worldBound);
            Assert.GreaterOrEqual(cells.Count(c => c.worldBound.xMin >= strip.xMin - tol && c.worldBound.xMax <= strip.xMax + tol), 1, when + ": at least one whole card");
        }

        // ---------------- timeline ----------------

        [UnityTest]
        public IEnumerator TheHorizontalTimeline_NeverShowsAHalfEvent_AtRestOrAtTheEndOfTheSwipe([Values("timeline_horizontal_long", "timeline_horizontal_short")] string name)
        {
            TimelineBlockView timeline = null;
            yield return ShowWithPeek(name, false, v => timeline = (TimelineBlockView)v);
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
            yield return ShowWithPeek(name, false, v => related = (RelatedBlockView)v);
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

        // ---------------- Peek Next Card (on: the default) ----------------

        [UnityTest]
        public IEnumerator WithPeekNextCard_ATrackShowsASliceOfTheNextCardAtRest_AndTheSwipeEndsOnAWholeLastCard(
            [Values("timeline_horizontal_long", "related_carousel_nearest")] string name)
        {
            IBlockView view = null;
            yield return ShowWithPeek(name, true, v => view = v);
            var (track, cells, gap) = TrackOf(view);
            Assert.Greater(track.horizontalScroller.highValue, 0f, name + ": precondition: more cards than fit (the track scrolls)");
            float narrowest = view is TimelineBlockView ? CardTestInput.TokenPx("--ts-timeline-cell") : CardTestInput.TokenPx("--ts-related-card");
            float onePixel = CardGalleryChecks.OnePixel(track);
            Assert.GreaterOrEqual(cells[0].worldBound.width, narrowest - onePixel, "a card is never narrower than its token");
            Assert.AreEqual(cells[0].worldBound.width, cells[1].worldBound.width, onePixel + 0.01f, "every card the same width");

            AssertSliceOfTheNext(track, cells, "at rest");
            if (view is RelatedBlockView related) AssertTitlesWhole(related);
            yield return CardGalleryChecks.Render("Card_" + name + "_peek_rest");

            yield return ScrollToEnd(track);
            Rect strip = track.contentViewport.worldBound;
            Assert.AreEqual(strip.xMax, cells.Last().worldBound.xMax + gap, onePixel * 2f + 0.5f, "the swipe ends on the last card, whole");
            yield return CardGalleryChecks.Render("Card_" + name + "_peek_end");
        }

        [UnityTest]
        public IEnumerator WithPeekNextCard_ATrackWhoseCardsAllFit_ShowsNoSlice([Values("timeline_horizontal_short", "related_carousel_manual")] string name)
        {
            IBlockView view = null;
            yield return ShowWithPeek(name, true, v => view = v);
            var (track, cells, gap) = TrackOf(view);
            Assert.AreEqual(0f, track.horizontalScroller.highValue, 0.5f, name + ": precondition: every card fits (nothing to swipe to)");
            AssertNoneCut(track, cells, "at rest");
            AssertStripEndsOnACard(track, cells, gap, "at rest");
        }

        [UnityTest]
        public IEnumerator TheGallery_ShowsEachTrackBothWays_ThePeekEntryWithASlice_TheNoPeekEntryWholeCardsOnly(
            [Values("timeline_horizontal_long_nopeek", "related_carousel_nearest_nopeek")] string name)
        {
            IBlockView view = null;
            _harness.Show(IndexOf(name));
            yield return CardGalleryChecks.SettledBlock(_harness, name, v => view = v);
            var (track, cells, gap) = TrackOf(view);
            Assert.Greater(track.horizontalScroller.highValue, 0f, name + ": precondition: more cards than fit, so with Peek Next Card on it would show a slice");
            AssertNoneCut(track, cells, "the no-peek entry at rest");
            AssertStripEndsOnACard(track, cells, gap, "the no-peek entry at rest");
            yield return CardGalleryChecks.Render("Card_" + name);
        }

        // The track, its cells and the gap after each, of a timeline or a related block
        private static (ScrollView Track, List<VisualElement> Cells, float Gap) TrackOf(IBlockView view)
        {
            if (view is TimelineBlockView timeline) return (timeline.Track, timeline.Events.Select(e => e.Box).ToList(), 0f);
            var related = (RelatedBlockView)view;
            var boxes = related.Cards.Select(c => c.Box).ToList();
            return (related.Track, boxes, boxes[0].resolvedStyle.marginRight);
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

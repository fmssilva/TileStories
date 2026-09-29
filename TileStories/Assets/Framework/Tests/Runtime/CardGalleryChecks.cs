using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace TileStories.Tests
{
    // The generic Phase A checks of ONE block entry on the gallery card (_3.1 section 7, 40-testing.md 4.4), shared by the
    // framework's gallery tests and by an app's own (step 11: an app kind's entry goes through exactly the same fit / contrast /
    // tap-target / heading / gap checks as a built-in kind, with no copy of them). Every member is public: an app's test assembly
    // reaches them, the framework's internals it cannot.
    public static class CardGalleryChecks
    {
        // One screen pixel in panel units: layout snaps each edge to a physical pixel, so a width and a height set by the
        // same token may differ by up to that much (the honest tolerance whatever the Game view's size)
        public static float OnePixel(VisualElement e) =>
            RuntimePanelUtils.ScreenToPanel(e.panel, Vector2.right).x - RuntimePanelUtils.ScreenToPanel(e.panel, Vector2.zero).x;

        // Save the frame on screen as TestEvidence/Card/<name>.png (the vision pass reads it)
        public static IEnumerator Render(string name)
        {
            yield return new WaitForEndOfFrame();
            var tex = ScreenCapture.CaptureScreenshotAsTexture();
            try { File.WriteAllBytes(TestEvidence.PathFor("Card", name + ".png"), tex.EncodeToPNG()); }
            finally { Object.Destroy(tex); }
        }

        // The colour a real render shows at a panel position (which picture a frame shows): the darkest pixel of the 7x7
        // patch there -- every gallery picture is its flat colour crossed by LIGHTER grid lines (CardGalleryMedia), so the
        // darkest pixel is the picture's own colour wherever the point falls
        public static IEnumerator PixelAt(VisualElement inPanel, Vector2 panelPoint, System.Action<Color> got)
        {
            yield return new WaitForEndOfFrame();
            var tex = ScreenCapture.CaptureScreenshotAsTexture();
            try
            {
                var origin = RuntimePanelUtils.ScreenToPanel(inPanel.panel, Vector2.zero);
                float unit = RuntimePanelUtils.ScreenToPanel(inPanel.panel, Vector2.right).x - origin.x;
                var screen = (panelPoint - origin) / unit;
                int cx = Mathf.RoundToInt(screen.x), cy = tex.height - 1 - Mathf.RoundToInt(screen.y);
                Color darkest = Color.white;
                for (int dy = -3; dy <= 3; dy++)
                    for (int dx = -3; dx <= 3; dx++)
                    {
                        var c = tex.GetPixel(cx + dx, cy + dy);
                        if (c.grayscale < darkest.grayscale) darkest = c;
                    }
                got(darkest);
            }
            finally { Object.Destroy(tex); }
        }

        // Show a block entry that is NOT in CardGalleryDefinitions.All (an app's own), exactly as the harness shows one of its own
        public static void ShowEntry(CardGalleryHarness harness, CardGalleryDefinitions.Entry entry)
        {
            harness.EnsureBuilt();
            var wall = CardGalleryDefinitions.Taxonomy();
            entry.WallSetup?.Invoke(wall);
            harness.ShowPoi(CardGalleryDefinitions.Poi(entry), wall, entry.Viewer, entry.Stop);
        }

        // Wait for the shown entry to settle, scroll its block (the card's second view, under the header) into view and hand it back
        public static IEnumerator SettledBlock(CardGalleryHarness harness, string name, System.Action<IBlockView> got)
        {
            yield return CardTestInput.Settle();
            var views = harness.Sheet.Stack.BoundViews;
            Assert.AreEqual(2, views.Count, name + ": the header + the block");
            // - a footer block (sticky) is pinned outside the scroll: nothing to scroll to
            if (harness.Sheet.Stack.SlotOf(views[1]).parent == harness.Sheet.Stack.Scroll.contentContainer) harness.Sheet.Stack.Scroll.ScrollTo(harness.Sheet.Stack.SlotOf(views[1]));
            yield return CardTestInput.Settle(0.15f);
            got(views[1]);
        }

        // Every block entry, whatever its kind: it builds at the full stop, every shown text sits inside the phone-width
        // card, reads against what is really behind it (>= 4.5:1, 3:1 for large text), every tap target is >= 44x44, and its
        // heading and the gap under it are the card's one look
        public static void AssertBlockEntry(CardGalleryHarness harness, CardGalleryDefinitions.Entry entry, IBlockView view)
        {
            string name = entry.Name;
            var sheet = harness.Sheet;
            Assert.AreEqual(SheetStopRule.Stop.Full, sheet.Stop);
            Assert.AreEqual(BlockRegistry.Shared.CreateView(entry.Kind).GetType(), view.GetType(), "the entry's kind drew it");
            Assert.Greater(view.Root.worldBound.height, 0f, "the block takes space on the card");
            Assert.AreEqual(DisplayStyle.None, sheet.Stack.Scroll.verticalScroller.resolvedStyle.display, "no desktop scroll bar on the phone card");

            Rect card = sheet.Root.worldBound;
            int texts = 0;
            foreach (var label in view.Root.Query<Label>().ToList())
            {
                if (string.IsNullOrEmpty(label.text) || !CardTestInput.IsShown(label, view.Root)) continue;
                texts++;
                string what = name + " '" + label.text.Substring(0, System.Math.Min(24, label.text.Length)) + "'";
                // - a label in a horizontal swipe track may lie beyond the card's edge until swiped to: it must fit the
                //   track's viewport (reachable whole), and the track itself must sit inside the card
                var track = label.GetFirstAncestorOfType<ScrollView>();
                if (track != null && track.mode == ScrollViewMode.Horizontal)
                {
                    Assert.LessOrEqual(label.worldBound.width, track.contentViewport.worldBound.width + 0.5f, what + " fits the swipe track's view");
                    Assert.GreaterOrEqual(track.worldBound.xMin, card.xMin - 0.5f, what + ": its track starts inside the card");
                    Assert.LessOrEqual(track.worldBound.xMax, card.xMax + 0.5f, what + ": its track ends inside the card");
                }
                else
                {
                    Assert.GreaterOrEqual(label.worldBound.xMin, card.xMin - 0.5f, what + " starts inside the card");
                    Assert.LessOrEqual(label.worldBound.xMax, card.xMax + 0.5f, what + " wraps inside the card");
                }
                bool large = label.resolvedStyle.fontSize >= 24f || (label.resolvedStyle.fontSize >= 18.66f && label.resolvedStyle.unityFontStyleAndWeight != FontStyle.Normal);
                float ratio = CardTestInput.Contrast(label.resolvedStyle.color, CardTestInput.EffectiveBackground(label));
                Assert.GreaterOrEqual(ratio, large ? UIAccessibility.MinRatioLargeTextOrUIComponent : UIAccessibility.MinRatioNormalText, what + " contrast");
            }
            // - a picture-only look (a grid of pictures) shows no text: a loaded picture counts as content too
            int pictures = view.Root.Query(className: "card-image").ToList().Count(e => CardTestInput.IsShown(e, view.Root) && !e.ClassListContains("card-image--unavailable"));
            Assert.Greater(texts + pictures, 0, name + ": the block shows text or a picture (not vacuous)");
            foreach (var tap in view.Root.Query(className: "card-tap").ToList().Where(e => CardTestInput.IsShown(e, view.Root)))
                Assert.IsTrue(UIAccessibility.MeetsMinTapTarget(tap.worldBound.width, tap.worldBound.height), name + ": a tap target is " + tap.worldBound.size);

            AssertHeadingAndGap(harness, entry, view);
        }

        // _3.1 step 6C: the stack draws the block's heading (authored, else the kind's default, else none) above the
        // block in ONE look, and the ONE gap under every scrolling block is --ts-block-gap -- no block adds its own margin
        public static void AssertHeadingAndGap(CardGalleryHarness harness, CardGalleryDefinitions.Entry entry, IBlockView view)
        {
            var stack = harness.Sheet.Stack;
            var heading = stack.HeadingOf(view);
            var slot = stack.SlotOf(view);
            var definition = BlockRegistry.Shared.TryGet(entry.Kind, out var d) ? d : null;
            string expected = CardGalleryDefinitions.HasHeading(entry)
                ? CardGalleryDefinitions.HeadingPrefix + entry.Kind.Replace('_', ' ')
                : definition?.DefaultHeadingKey != null ? new CardStrings(harness.StringTable.Entries(), harness.StringSources.Entries(), null, harness.Language, harness.Language).Get(definition.DefaultHeadingKey) : "";
            Assert.AreEqual(expected, heading.text, entry.Name + ": the heading the stack draws");
            if (expected.Length > 0)
            {
                Assert.IsTrue(CardTestInput.IsShown(heading, slot), entry.Name + ": the heading shows");
                Assert.LessOrEqual(heading.worldBound.yMax, view.Root.worldBound.yMin + 0.5f, entry.Name + ": above its block");
                // - the same look in the scroll, the footer and a block pinned under the header (only the header's OWN heading --
                //   inside HeaderSlot but not inside the pinned strip nested in it -- styles its kicker its own way)
                var reference = harness.Sheet.Root.Query<Label>(className: "card-block-heading").ToList()
                    .First(l => !stack.HeaderSlot.Contains(l) || stack.PinnedTop.Contains(l));
                Assert.AreEqual(reference.resolvedStyle.fontSize, heading.resolvedStyle.fontSize, "one heading look");
                Assert.GreaterOrEqual(CardTestInput.Contrast(heading.resolvedStyle.color, CardTestInput.EffectiveBackground(heading)), UIAccessibility.MinRatioNormalText,
                    entry.Name + ": heading contrast");
            }
            else Assert.AreEqual(DisplayStyle.None, heading.resolvedStyle.display, entry.Name + ": no heading, no empty line");

            if (slot.parent != stack.Scroll.contentContainer) return;
            Assert.AreEqual(0f, view.Root.resolvedStyle.marginTop, 0.01f, entry.Name + ": the block adds no top margin of its own");
            Assert.AreEqual(0f, view.Root.resolvedStyle.marginBottom, 0.01f, entry.Name + ": ...nor a bottom one");
            Assert.AreEqual(CardTestInput.TokenPx("--ts-block-gap"), slot.resolvedStyle.marginBottom, OnePixel(slot) + 0.01f, entry.Name + ": the gap under it is the token (snapped to a screen pixel)");
            Assert.AreEqual(slot.worldBound.yMax, view.Root.worldBound.yMax, OnePixel(slot) + 0.01f, entry.Name + ": the block ends where its slot ends");
        }
    }
}

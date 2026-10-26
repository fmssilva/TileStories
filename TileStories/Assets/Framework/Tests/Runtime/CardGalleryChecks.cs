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

        // The ONE pixel a real render shows at a panel position -- for a small shape whose parts are a few pixels apart (a star's hollow,
        // ~3 px to its stroke): PixelAt's 7x7 "darkest" patch would reach the stroke there, and on a light ground read it instead
        public static IEnumerator PixelExactlyAt(VisualElement inPanel, Vector2 panelPoint, System.Action<Color> got)
        {
            yield return new WaitForEndOfFrame();
            var tex = ScreenCapture.CaptureScreenshotAsTexture();
            try
            {
                var origin = RuntimePanelUtils.ScreenToPanel(inPanel.panel, Vector2.zero);
                float unit = RuntimePanelUtils.ScreenToPanel(inPanel.panel, Vector2.right).x - origin.x;
                var screen = (panelPoint - origin) / unit;
                got(tex.GetPixel(Mathf.FloorToInt(screen.x), tex.height - 1 - Mathf.FloorToInt(screen.y)));
            }
            finally { Object.Destroy(tex); }
        }

        // The pixel a screen capture holds at a panel position (one pixel, no patch)
        public static Color ScreenPixel(Texture2D shot, VisualElement inPanel, Vector2 panelPoint)
        {
            var origin = RuntimePanelUtils.ScreenToPanel(inPanel.panel, Vector2.zero);
            float unit = RuntimePanelUtils.ScreenToPanel(inPanel.panel, Vector2.right).x - origin.x;
            var screen = (panelPoint - origin) / unit;
            return shot.GetPixel(Mathf.FloorToInt(screen.x), shot.height - 1 - Mathf.FloorToInt(screen.y));
        }

        // A text's colour against its REAL ground on a render (_3.3 D1, the section opener's method): the ground just outside the words'
        // box -- above, below, before, after, `margin` panel units away, where their letters' edges sit -- each read on the capture; the
        // worst ratio and where it was. A point outside the capture or outside `within` (the visible part) is not judged
        public static (float Ratio, string At) WordsOnRender(Texture2D shot, Label words, Rect within, float margin = 2f)
        {
            float worst = float.MaxValue;
            string at = "";
            var box = words.worldBound;
            foreach (var p in new[] { new Vector2(box.center.x, box.yMin - margin), new Vector2(box.center.x, box.yMax + margin),
                         new Vector2(box.xMin - margin, box.center.y), new Vector2(box.xMax + margin, box.center.y) })
            {
                if (!within.Contains(p)) continue;
                var ground = ScreenPixel(shot, words, p);
                float ratio = UIAccessibility.ContrastRatio(ground, words.resolvedStyle.color);
                if (ratio >= worst) continue;
                worst = ratio;
                at = p + " ground " + (Color32)ground + " words " + (Color32)words.resolvedStyle.color;
            }
            return (worst, at);
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
            // - a look pinned to the top (the audio guide's hero_chip) is drawn only in the hero's action row (_3.3 P4.4a): it has no slot
            if (views.Count == 1 && harness.Sheet.Stack.HeroActionViews.Count == 1)
            {
                yield return CardTestInput.Settle(0.15f);
                got(harness.Sheet.Stack.HeroActionViews[0]);
                yield break;
            }
            Assert.AreEqual(2, views.Count, name + ": the header + the block");
            // - a footer block (sticky) is pinned outside the scroll: nothing to scroll to
            if (harness.Sheet.Stack.IsInScroll(harness.Sheet.Stack.SlotOf(views[1]))) harness.Sheet.Stack.Scroll.ScrollTo(harness.Sheet.Stack.SlotOf(views[1]));
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
                // - a label nobody can see is not judged: out of the layout (display none) or laid out but invisible (the story chapters not shown,
                //   kept beside the shown one for their height, _3.2 step 14)
                if (string.IsNullOrEmpty(label.text) || !CardTestInput.IsShown(label, view.Root) || label.resolvedStyle.visibility == Visibility.Hidden) continue;
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
            // - the hero's Listen (a hero_chip look, _3.3 P4.4a) sits in the hero's action row: no slot, no heading, no gap of its own
            if (slot == null)
            {
                Assert.IsTrue(stack.HeaderView.Actions.Contains(view.Root), entry.Name + ": a block with no slot is one of the hero's actions");
                return;
            }
            var definition = BlockRegistry.Shared.TryGet(entry.Kind, out var d) ? d : null;
            string expected = CardGalleryDefinitions.HasHeading(entry)
                ? CardGalleryDefinitions.HeadingFor(entry.Kind)
                : definition?.DefaultHeadingKey != null ? new CardStrings(harness.StringTable.Entries(), harness.StringSources.Entries(), null, harness.Language, harness.Language).Get(definition.DefaultHeadingKey) : "";
            Assert.AreEqual(expected, heading.text, entry.Name + ": the heading the stack draws");
            // - a composed block draws its own title (_3.3 S2b): its slot heading keeps the words for navigation and is never drawn
            if (expected.Length > 0 && BlockSurfaces.Composes(definition))
                Assert.IsFalse(CardTestInput.IsShown(heading, slot), entry.Name + ": a composed block's own title is its heading: the slot's is not drawn");
            else if (expected.Length > 0)
            {
                Assert.IsTrue(CardTestInput.IsShown(heading, slot), entry.Name + ": the heading shows");
                Assert.LessOrEqual(heading.worldBound.yMax, view.Root.worldBound.yMin + 0.5f, entry.Name + ": above its block");
                // - the same look in the scroll and the footer (the header's own heading is hidden in the bar)
                var reference = harness.Sheet.Root.Query<Label>(className: "card-block-heading").ToList()
                    .First(l => !stack.HeaderSlot.Contains(l));
                Assert.AreEqual(reference.resolvedStyle.fontSize, heading.resolvedStyle.fontSize, "one heading look");
                Assert.GreaterOrEqual(CardTestInput.Contrast(heading.resolvedStyle.color, CardTestInput.EffectiveBackground(heading)), UIAccessibility.MinRatioNormalText,
                    entry.Name + ": heading contrast");
            }
            else Assert.AreEqual(DisplayStyle.None, heading.resolvedStyle.display, entry.Name + ": no heading, no empty line");

            // - only a block of the scroll (inside its section sheet since _3.3 P4.4c1): the header and the footer are chrome
            if (!stack.IsInScroll(slot)) return;
            Assert.AreEqual(0f, view.Root.resolvedStyle.marginTop, 0.01f, entry.Name + ": the block adds no top margin of its own");
            Assert.AreEqual(0f, view.Root.resolvedStyle.marginBottom, 0.01f, entry.Name + ": ...nor a bottom one");
            Assert.AreEqual(CardTestInput.TokenPx("--ts-block-gap"), slot.resolvedStyle.marginBottom, OnePixel(slot) + 0.01f, entry.Name + ": the gap under it is the token (snapped to a screen pixel)");
            // - a block of the scroll sits in its frame (_3.3 P4.4b): it ends where the frame's CONTENT ends, inside its padding and rim
            float contentEnd = slot.worldBound.yMax - slot.resolvedStyle.paddingBottom - slot.resolvedStyle.borderBottomWidth;
            Assert.AreEqual(contentEnd, view.Root.worldBound.yMax, OnePixel(slot) + 0.01f, entry.Name + ": the block ends where its frame's content ends");
        }

        // _3.2 step 12: `entry`'s block shown Display Expandable on the gallery card, compact and then opened by a real tap, for ANY kind (the
        // framework walks its whole catalogue with it, an app its own kind). Compact: ONE tappable line (a whole touch target) with words
        // that sit inside the card and read (4.5:1), its format, the family icon, the chevron, and NO view bound and no file held; a real tap
        // binds the kind's own view in the row's slot and shows the full-view button; a second real tap closes it and gives every file back.
        // Renders <render>_compact.png and <render>_expanded.png
        public static IEnumerator AssertCompactAndExpanded(CardGalleryHarness harness, CardGalleryDefinitions.Entry entry, string render)
        {
            var block = JsonUtility.FromJson<BlockInstanceData>(JsonUtility.ToJson(entry.Block));
            if (string.IsNullOrEmpty(block.key)) block.key = "block_2";
            block.display = CardOptions.DisplayExpandable;
            ShowEntry(harness, new CardGalleryDefinitions.Entry(entry.Kind, entry.Variant, entry.Content, block, entry.Setup, entry.WallSetup, entry.Viewer));
            yield return CardTestInput.Settle();
            var stack = harness.Sheet.Stack;
            string key = block.key;
            var row = stack.CompactRowOf(key);
            Assert.IsNotNull(row, entry.Kind + ": drawn as a compact row (was it skipped? " + string.Join(", ", stack.BoundKinds) + ")");
            Assert.IsNull(stack.ViewOfBlock(key), entry.Kind + ": a closed row binds no view");
            Assert.AreEqual(0, stack.HeldMediaOf(key), entry.Kind + ": and holds no file");
            Assert.AreEqual(1, stack.BoundViews.Count, entry.Kind + ": the header's is the only view on the card");

            Rect card = harness.Sheet.Root.worldBound;
            Assert.Greater(row.Words.text.Length, 0, entry.Kind + ": the row has words");
            Assert.Greater(row.Meta.text.Length, 0, entry.Kind + ": and its kind of content");
            Assert.AreEqual(DisplayStyle.Flex, row.Icon.resolvedStyle.display, entry.Kind + ": the family icon");
            Assert.IsTrue(UIAccessibility.MeetsMinTapTarget(row.Toggle.worldBound.width, row.Toggle.worldBound.height),
                entry.Kind + ": the line is a whole touch target (" + row.Toggle.worldBound.size + ")");
            Assert.GreaterOrEqual(row.Words.worldBound.xMin, card.xMin - 0.5f, entry.Kind + ": its words start inside the card");
            Assert.LessOrEqual(row.Words.worldBound.xMax, row.Chevron.worldBound.xMin + 0.5f, entry.Kind + ": and end before the chevron");
            Assert.LessOrEqual(row.Chevron.worldBound.xMax, card.xMax + 0.5f, entry.Kind + ": the chevron inside the card");
            foreach (var label in new[] { row.Words, row.Meta })
                Assert.GreaterOrEqual(CardTestInput.Contrast(label.resolvedStyle.color, CardTestInput.EffectiveBackground(label)), UIAccessibility.MinRatioNormalText,
                    entry.Kind + " '" + label.text + "' reads");
            yield return Render(render + "_compact");

            yield return CardTestInput.TapInView(stack.Scroll, row.Toggle);
            yield return CardTestInput.Settle(0.2f);
            var view = stack.ViewOfBlock(key);
            Assert.IsNotNull(view, entry.Kind + ": a real tap opened it");
            Assert.AreEqual(BlockRegistry.Shared.CreateView(entry.Kind).GetType(), view.GetType(), entry.Kind + ": its own view");
            Assert.AreSame(stack.SlotOfBlock(key), view.Root.parent, entry.Kind + ": under its row, in the same slot");
            Assert.IsTrue(row.IsExpanded, entry.Kind + ": the row shows it open");
            // - one full-screen control per job (_3.2 step 13): a kind whose own view offers a full screen (video) gets no row button beside it
            BlockRegistry.Shared.TryGet(entry.Kind, out var kind);
            bool ownFullView = kind?.OffersOwnFullView ?? false;
            Assert.AreEqual(ownFullView ? DisplayStyle.None : DisplayStyle.Flex, row.FullView.resolvedStyle.display,
                entry.Kind + (ownFullView ? ": no row full-view button, its own view offers one" : ": with its full-view button"));
            if (!ownFullView)
                Assert.IsTrue(UIAccessibility.MeetsMinTapTarget(row.FullView.worldBound.width, row.FullView.worldBound.height), entry.Kind + ": a whole touch target");
            stack.Scroll.ScrollTo(stack.SlotOfBlock(key));
            yield return CardTestInput.Settle(0.2f);
            yield return Render(render + "_expanded");

            yield return CardTestInput.TapInView(stack.Scroll, row.Toggle);
            Assert.IsNull(stack.ViewOfBlock(key), entry.Kind + ": a second real tap closed it");
            Assert.AreEqual(0, stack.HeldMediaOf(key), entry.Kind + ": every file given back");
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TileStories.Tests
{
    // The block frame and the two modes on the REAL card (_3.3 P4.4b, R13.1, R12.5-6, D2): every block of the scroll sits in an opaque
    // surface inside its rim, the bar and the footer do not; Emphasis quiet / featured; and the RENDER half of the measured palettes -- each
    // block's edge >= 3:1 against the sheet ground over the camera stand-in (P0 S5's sunny azulejo wall), in light and in dark, at the worst
    // sampled point; the glare render (35 % white, P0 S2c) of the card in both modes.
    public class ThemeFrameSceneTests : SearchSceneFixture
    {
        // The P0 S5 camera stand-in: the shared gallery backdrop, a sunny azulejo wall
        private const string PhotoPath = "Assets/Dev/MarkerGallery/Backdrops/backdrop.png";
        // How far outside / inside a block's edge a sample is taken: past the 1 px outer line and the rim band (two points of the panel)
        private const float SampleOffset = 3f;

        private VisualElement _photo;

        [UnityTearDown]
        public IEnumerator RestoreTheMode()
        {
            _photo?.RemoveFromHierarchy();
            ThemeController.SetMode("");
            yield return null;
        }

        // Open a card at Full
        private IEnumerator OpenAtFull(string poi)
        {
            SelectionEventBus.Select(poi);
            yield return CardTestInput.Settle();
            Card.Sheet.SetStop(SheetStopRule.Stop.Full);
            yield return CardTestInput.Settle();
            Assert.IsTrue(Card.Sheet.IsOpen, "precondition: " + poi + "'s card is open");
        }

        // The hero's height (the scroll's first part): scrolling past it brings the blocks into view
        private float HeroHeight() => Card.Sheet.Stack.HeaderView?.HeroPart.layout.height ?? 0f;

        // The framed slots of the stack (the scroll's blocks), in order
        private List<VisualElement> FramedSlots() =>
            Card.Sheet.Stack.Scroll.contentContainer.Query(className: BlockFrame.FramedClass).ToList();

        [UnityTest]
        public IEnumerator EveryBlockOfTheScroll_SitsInAnOpaqueFrame_TheBarAndTheFooterDoNot()
        {
            yield return OpenAtFull("lamp");
            var stack = Card.Sheet.Stack;
            var scrollSlots = stack.Scroll.contentContainer.Query(className: "card-block-slot").ToList();
            Assert.Greater(scrollSlots.Count, 20, "precondition: The Lamp's scroll holds its blocks");
            float padding = CardTestInput.TokenPx("--ts-space-4");
            Color surface = default;
            Assert.IsTrue(ThemeController.TryResolverFor(stack.Scroll, out var theme) && theme.TryColour("--ts-surface", out surface),
                "precondition: the card sits under the theme");
            foreach (var slot in scrollSlots)
            {
                Assert.IsTrue(slot.ClassListContains(BlockFrame.FramedClass), slot.name + ": every block of the scroll is framed");
                Assert.AreEqual(surface, slot.resolvedStyle.backgroundColor, "the frame is the theme's opaque surface");
                Assert.AreEqual(1f, slot.resolvedStyle.backgroundColor.a, "opaque: no text of a block sits on the glass");
                Assert.AreEqual(padding, slot.resolvedStyle.paddingLeft, 0.01f, "padding 16 on the 8 grid (--ts-space-4)");
                Assert.AreEqual(padding, slot.resolvedStyle.paddingTop, 0.01f);
                Assert.Greater(slot.resolvedStyle.borderLeftWidth, 0f, "the rim's outer line");
                Assert.AreEqual(DisplayStyle.Flex, slot.Q(className: BlockFrame.RimClass).resolvedStyle.display, "the rim band shows");
                Assert.AreEqual(DisplayStyle.Flex, slot.Q(className: BlockFrame.ShadowClass).resolvedStyle.display, "the shadow part shows");
            }
            foreach (var chrome in new[] { stack.HeaderSlot, stack.Footer })
                Assert.IsFalse(chrome.Query(className: BlockFrame.FramedClass).ToList().Any(), chrome.name + ": the bar and the footer are chrome, never framed");
            // - the words of a block read on its frame: the first block's heading's composited background IS the surface
            var heading = scrollSlots.Select(s => s.Q<Label>(className: "card-block-heading")).First(h => h != null && h.resolvedStyle.display == DisplayStyle.Flex);
            Assert.AreEqual(surface, CardTestInput.EffectiveBackground(heading), "a heading's ground is its frame, never the glass behind it");
        }

        // P4.4b part 2 (Architect, reversible): words keep the frame's 16 pt padding; a picture, carousel, strip or track runs edge to edge to
        // the frame's INNER edge (inside its rim line), App-Store-card style -- so a short timeline and a two-card carousel fit again
        [UnityTest]
        public IEnumerator PicturesAndTracks_RunToTheFramesInnerEdge_WordsKeepThePadding()
        {
            yield return OpenAtFull("lamp");
            float padding = CardTestInput.TokenPx("--ts-space-4");
            var regions = FramedSlots().SelectMany(slot => slot.Query(className: BlockFrame.BleedClass).ToList()
                    .Where(r => r.resolvedStyle.display == DisplayStyle.Flex && r.worldBound.width > 0f).Select(r => (slot, r))).ToList();
            var kinds = regions.Select(p => p.r.parent?.GetClasses().FirstOrDefault(c => c != "card-block") ?? "").Distinct().ToList();
            Assert.GreaterOrEqual(regions.Count, 5, "precondition: The Lamp holds several pictures and tracks in frames: " + string.Join(", ", kinds));
            foreach (var (slot, region) in regions)
            {
                float one = CardGalleryChecks.OnePixel(slot);
                string what = region.name + "." + string.Join(".", region.GetClasses()) + " in " + string.Join(".", region.parent.GetClasses());
                // - --ts-frame-bleed undoes exactly the frame's side padding (a USS length cannot be negated: the pair is checked here)
                Assert.AreEqual(-padding, region.resolvedStyle.marginLeft, 0.01f, what + ": the bleed is minus the frame's padding");
                Assert.AreEqual(-padding, region.resolvedStyle.marginRight, 0.01f, what + ": on both sides");
                float inner = slot.worldBound.xMin + slot.resolvedStyle.borderLeftWidth;
                float innerEnd = slot.worldBound.xMax - slot.resolvedStyle.borderRightWidth;
                Assert.AreEqual(inner, region.worldBound.xMin, one, what + ": starts at the frame's inner edge");
                Assert.AreEqual(innerEnd, region.worldBound.xMax, one, what + ": ends at the frame's inner edge");
            }
            // - and the words: a heading in the same frames still sits inside the padding
            foreach (var slot in regions.Select(p => p.slot).Distinct())
            {
                var heading = slot.Q<Label>(className: "card-block-heading");
                if (heading == null || heading.resolvedStyle.display != DisplayStyle.Flex) continue;
                Assert.AreEqual(slot.worldBound.xMin + slot.resolvedStyle.borderLeftWidth + padding, heading.worldBound.xMin, CardGalleryChecks.OnePixel(slot),
                    slot.name + ": its heading keeps the 16 pt padding");
            }
        }

        [UnityTest]
        public IEnumerator Emphasis_QuietIsAHairlineWithNoBandOrShadow_FeaturedDeepensTheShadow_LiveFromTheConfig()
        {
            var lamp = Session.SearchPois.First(p => p.id == "lamp");
            var blocks = lamp.card.blocks;
            var sources = blocks.First(b => b.kind == "sources");
            var story = blocks.First(b => b.kind == "rich_text");
            string sourcesBefore = sources.emphasis, storyBefore = story.emphasis;
            try
            {
                sources.emphasis = CardOptions.EmphasisQuiet;
                story.emphasis = CardOptions.EmphasisFeatured;
                yield return OpenAtFull("lamp");
                var quiet = Card.Sheet.Stack.SlotOfBlock(sources.key);
                var featured = Card.Sheet.Stack.SlotOfBlock(story.key);
                var normal = FramedSlots().First(s => s != quiet && s != featured && s.ClassListContains("card-block-slot--normal"));
                Assert.IsTrue(quiet.ClassListContains("card-block-slot--quiet"), "the quiet class is on its slot");
                Assert.AreEqual(DisplayStyle.None, quiet.Q(className: BlockFrame.RimClass).resolvedStyle.display, "quiet: no rim band");
                Assert.AreEqual(DisplayStyle.None, quiet.Q(className: BlockFrame.ShadowClass).resolvedStyle.display, "quiet: no shadow");
                Assert.AreEqual(normal.resolvedStyle.borderTopWidth, quiet.resolvedStyle.borderTopWidth, "quiet keeps the hairline edge");
                Assert.Less(quiet.Q<Label>(className: "card-block-heading").resolvedStyle.fontSize,
                    normal.Q<Label>(className: "card-block-heading").resolvedStyle.fontSize, "quiet: a smaller title");
                Color deep = default;
                Assert.IsTrue(ThemeController.TryResolverFor(featured, out var theme) && theme.TryColour("--ts-elevation-2", out deep), "precondition: the theme's deeper shadow");
                Assert.AreEqual(deep, featured.Q(className: BlockFrame.ShadowClass).resolvedStyle.unityBackgroundImageTintColor, "featured: the deeper shadow");
                Assert.Greater(featured.Q<Label>(className: "card-block-heading").resolvedStyle.fontSize,
                    normal.Q<Label>(className: "card-block-heading").resolvedStyle.fontSize, "featured: a larger title");
                // - an unknown value reads as normal (the first option), never as no frame
                sources.emphasis = "loud";
                Card.Rebind();
                yield return CardTestInput.Settle();
                Assert.IsTrue(Card.Sheet.Stack.SlotOfBlock(sources.key).ClassListContains("card-block-slot--normal"), "an unknown emphasis is normal");
            }
            finally { sources.emphasis = sourcesBefore; story.emphasis = storyBefore; }
        }

        // D2 on the render: every framed block in view, over the sunny wall, in both modes -- at each sampled point of its four edges the
        // better of (rim outer line vs the ground just outside, surface just inside vs that ground) clears 3:1; the worst point is reported
        [UnityTest]
        public IEnumerator EveryBlocksEdge_ClearsThreeToOne_AgainstTheGlassOverTheSunnyWall_InBothModes([Values("light", "dark")] string mode)
        {
            yield return OpenAtFull("lamp");
            PutTheWallBehind();
            ThemeController.SetMode(mode);
            // - past the hero: the blocks are what is measured
            Card.Sheet.Stack.Scroll.scrollOffset = new Vector2(0f, HeroHeight() + 40f);
            yield return CardTestInput.Settle();
            yield return new WaitForEndOfFrame();
            var shot = ScreenCapture.CaptureScreenshotAsTexture();
            try
            {
                var viewport = Card.Sheet.Stack.Scroll.contentViewport.worldBound;
                var slots = FramedSlots().Where(s => viewport.Contains(s.worldBound.min) && viewport.Contains(s.worldBound.max)).ToList();
                Assert.Greater(slots.Count, 0, "precondition: at least one whole block is in view to measure");
                float worst = float.MaxValue; string worstAt = "";
                foreach (var slot in slots)
                    foreach (var (outside, line, inside) in EdgeSamples(slot.worldBound))
                    {
                        // - _3.3 P4.4c1: the ground just outside a block is its OWN section sheet (its family's tinted glass) over the wall
                        var sheet = Card.Sheet.Stack.Sheets.SheetOf(slot);
                        Assert.IsNotNull(sheet, slot.name + ": precondition -- a block of the scroll sits on its section sheet");
                        Assert.IsTrue(sheet.Root.worldBound.Contains(outside), slot.name + ": the ground sampled at " + outside + " is its own sheet's glass");
                        Color ground = PixelAt(shot, outside), rim = PixelAt(shot, line), surface = PixelAt(shot, inside);
                        float edge = Mathf.Max(UIAccessibility.ContrastRatio(rim, ground), UIAccessibility.ContrastRatio(surface, ground));
                        if (edge < worst) { worst = edge; worstAt = $"{slot.name} at {line} (rim {(Color32)rim}, surface {(Color32)surface}, ground {(Color32)ground})"; }
                    }
                File.WriteAllBytes(TestEvidence.PathFor("Theme", "frame_over_wall_" + mode + ".png"), shot.EncodeToPNG());
                File.AppendAllText(TestEvidence.PathFor("Theme", "frame_edges.txt"), $"{mode}: worst edge {worst:0.00}:1 -- {worstAt}\n");
                Assert.GreaterOrEqual(worst, UIAccessibility.MinRatioLargeTextOrUIComponent, mode + ": the worst block edge over the sunny wall -- " + worstAt);
            }
            finally { Object.Destroy(shot); }
        }

        // The P0 glare layer (35 % white over the whole screen: sun on the glass) over the real card in both modes: a render for the eye
        [UnityTest]
        public IEnumerator TheCardUnderGlare_IsRendered_InBothModes([Values("light", "dark")] string mode)
        {
            yield return OpenAtFull("painting");
            PutTheWallBehind();
            ThemeController.SetMode(mode);
            var root = Card.Sheet.Root.panel.visualTree;
            var glare = new VisualElement { pickingMode = PickingMode.Ignore };
            glare.style.position = Position.Absolute;
            glare.style.left = 0; glare.style.top = 0; glare.style.right = 0; glare.style.bottom = 0;
            glare.style.backgroundColor = Color.white;
            glare.style.opacity = 0.35f;
            root.Add(glare);
            try
            {
                Card.Sheet.Stack.Scroll.scrollOffset = new Vector2(0f, HeroHeight());
                yield return CardTestInput.Settle();
                yield return new WaitForEndOfFrame();
                var shot = ScreenCapture.CaptureScreenshotAsTexture();
                File.WriteAllBytes(TestEvidence.PathFor("Theme", "glare_painting_" + mode + ".png"), shot.EncodeToPNG());
                Assert.Greater(shot.width, 0, "the render was taken");
                Object.Destroy(shot);
            }
            finally { glare.RemoveFromHierarchy(); }
        }

        // The sunny wall behind the card, where the AR camera feed is: a full-screen picture first in the card's own panel
        private void PutTheWallBehind()
        {
#if UNITY_EDITOR
            var photo = AssetDatabase.LoadAssetAtPath<Texture2D>(PhotoPath);
#else
            Texture2D photo = null;
#endif
            Assert.IsNotNull(photo, "the sunny-wall photo " + PhotoPath + " -- SETUP");
            var root = Card.Sheet.Layer.parent;
            _photo = new VisualElement { name = "test-camera-stand-in", pickingMode = PickingMode.Ignore };
            _photo.style.position = Position.Absolute;
            _photo.style.left = 0; _photo.style.top = 0; _photo.style.right = 0; _photo.style.bottom = 0;
            _photo.style.backgroundImage = photo;
            _photo.style.unityBackgroundScaleMode = ScaleMode.ScaleAndCrop;
            root.Insert(0, _photo);
        }

        // Points along a block's four edges: (in the ground just outside, on the outer line, on the surface just inside), in panel points
        private static IEnumerable<(Vector2 Outside, Vector2 Line, Vector2 Inside)> EdgeSamples(Rect box)
        {
            const int perSide = 6;
            for (int i = 1; i <= perSide; i++)
            {
                // - kept off the rounded corners: from 25 % to 75 % of each side
                float tx = Mathf.Lerp(box.xMin + box.width * 0.25f, box.xMax - box.width * 0.25f, (i - 1f) / (perSide - 1f));
                float ty = Mathf.Lerp(box.yMin + box.height * 0.25f, box.yMax - box.height * 0.25f, (i - 1f) / (perSide - 1f));
                yield return (new Vector2(tx, box.yMin - SampleOffset), new Vector2(tx, box.yMin + 0.5f), new Vector2(tx, box.yMin + SampleOffset));
                yield return (new Vector2(tx, box.yMax + SampleOffset), new Vector2(tx, box.yMax - 0.5f), new Vector2(tx, box.yMax - SampleOffset));
                yield return (new Vector2(box.xMin - SampleOffset, ty), new Vector2(box.xMin + 0.5f, ty), new Vector2(box.xMin + SampleOffset, ty));
                yield return (new Vector2(box.xMax + SampleOffset, ty), new Vector2(box.xMax - 0.5f, ty), new Vector2(box.xMax - SampleOffset, ty));
            }
        }

        // The rendered colour at a panel point (the capture is in screen pixels, bottom-up)
        private Color PixelAt(Texture2D shot, Vector2 panelPoint)
        {
            float scale = shot.width / Card.Sheet.Layer.panel.visualTree.layout.width;
            int x = Mathf.Clamp(Mathf.FloorToInt(panelPoint.x * scale), 0, shot.width - 1);
            int y = Mathf.Clamp(shot.height - 1 - Mathf.FloorToInt(panelPoint.y * scale), 0, shot.height - 1);
            return shot.GetPixel(x, y);
        }
    }
}

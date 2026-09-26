using NUnit.Framework;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // Pure math for the demo grids' scroll-zoom + WASD/mouse-drag pan (DevPreviewCameraDolly),
    // shared by EffectsPreviewFocus and OutlinePreviewFocus. No MonoBehaviour, no Input -- Input
    // polling (DevCameraInput) is proven live in EffectsPreviewSpawnerTests/OutlinePreviewRenderTests.
    public class DevPreviewCameraDollyTests
    {
        // ---------------- zoom ----------------

        [Test]
        public void ApplyZoom_WithNoScroll_ReturnsTheAutoFitDistanceUnchanged()
        {
            var dolly = new TileStories.DevPreviewCameraDolly();
            Assert.AreEqual(5f, dolly.ApplyZoom(5f), 1e-4f);
        }

        [Test]
        public void ApplyScroll_Positive_MovesCloser_Negative_MovesFarther()
        {
            var dolly = new TileStories.DevPreviewCameraDolly();
            dolly.ApplyScroll(1f);
            Assert.Less(dolly.ApplyZoom(5f), 5f, "scrolling in must reduce distance");

            var farther = new TileStories.DevPreviewCameraDolly();
            farther.ApplyScroll(-1f);
            Assert.Greater(farther.ApplyZoom(5f), 5f, "scrolling out must increase distance");
        }

        [Test]
        public void ApplyZoomDelta_Positive_MovesCloser_MirroringWSKeys()
        {
            var dolly = new TileStories.DevPreviewCameraDolly();
            dolly.ApplyZoomDelta(1f);
            Assert.Less(dolly.ApplyZoom(5f), 5f, "W (forward) must reduce distance, same direction as scrolling in");
        }

        [Test]
        public void ApplyScroll_Accumulates_AcrossMultipleCalls()
        {
            var dolly = new TileStories.DevPreviewCameraDolly();
            dolly.ApplyScroll(1f);
            dolly.ApplyScroll(1f);
            dolly.ApplyScroll(1f);
            Assert.AreEqual(-3f * TileStories.DevPreviewCameraDolly.MetresPerScrollClick, dolly.ZoomOffsetMetres, 1e-4f);
        }

        [Test]
        public void ApplyZoom_NeverGoesBelowOrAboveItsClampFractionOfTheAutoFitDistance()
        {
            var zoomedInFar = new TileStories.DevPreviewCameraDolly();
            zoomedInFar.ApplyScroll(1000f);
            Assert.GreaterOrEqual(zoomedInFar.ApplyZoom(10f), 10f * 0.15f - 1e-3f, "must never zoom through the grid");

            var zoomedOutFar = new TileStories.DevPreviewCameraDolly();
            zoomedOutFar.ApplyScroll(-1000f);
            Assert.LessOrEqual(zoomedOutFar.ApplyZoom(10f), 10f * 2.5f + 1e-3f, "must never retreat unboundedly");
        }

        // ---------------- pan ----------------

        [Test]
        public void ApplyPan_Accumulates_AndClampedPan_ReturnsItUnchanged_WhenWellInsideTheExtent()
        {
            var dolly = new TileStories.DevPreviewCameraDolly();
            dolly.ApplyPan(new Vector2(0.1f, 0.05f));
            dolly.ApplyPan(new Vector2(0.1f, 0.05f));

            var clamped = dolly.ClampedPan(new Vector2(10f, 10f));
            Assert.AreEqual(0.2f, clamped.x, 1e-4f);
            Assert.AreEqual(0.1f, clamped.y, 1e-4f);
        }

        [Test]
        public void ClampedPan_NeverExceedsAFractionOfTheGridsOwnExtent()
        {
            var dolly = new TileStories.DevPreviewCameraDolly();
            dolly.ApplyPan(new Vector2(1000f, -1000f));

            var extent = new Vector2(4f, 3f);
            var clamped = dolly.ClampedPan(extent);

            Assert.LessOrEqual(clamped.x, extent.x * 0.5f, "must never pan the grid entirely out of frame horizontally");
            Assert.GreaterOrEqual(clamped.y, -extent.y * 0.5f, "must never pan the grid entirely out of frame vertically");
        }

        // ---------------- reset ----------------

        [Test]
        public void Reset_ClearsBothZoomAndPan_SoARebuiltGridStartsFramedAgain()
        {
            var dolly = new TileStories.DevPreviewCameraDolly();
            dolly.ApplyScroll(5f);
            dolly.ApplyPan(new Vector2(1f, 1f));
            Assert.AreNotEqual(0f, dolly.ZoomOffsetMetres);
            Assert.AreNotEqual(Vector2.zero, dolly.PanOffsetMetres);

            dolly.Reset();

            Assert.AreEqual(0f, dolly.ZoomOffsetMetres);
            Assert.AreEqual(Vector2.zero, dolly.PanOffsetMetres);
            Assert.AreEqual(8f, dolly.ApplyZoom(8f), 1e-4f);
            Assert.AreEqual(Vector2.zero, dolly.ClampedPan(new Vector2(10f, 10f)));
        }

        [Test]
        public void EachFocusComponent_GetsItsOwnFreshDolly_NotAStaticSharedOne()
        {
            // A rebuilt grid gets a brand new EffectsPreviewFocus/OutlinePreviewFocus instance and,
            // with it, a brand new dolly at zero -- confirmed structurally: the dolly is an instance
            // field, never static. WallSession (2026-09-22, EffectsPreviewSpawnerTests.cs) now
            // explicitly carries the previous dolly's offset onto this fresh instance via
            // SetOffsets right after respawn, so a developer's camera position survives a rebuild --
            // but that is WallSession's job, not this class's; left alone, a fresh instance truly
            // does start at zero, which is exactly what SetOffsets below overwrites.
            var a = new TileStories.DevPreviewCameraDolly();
            a.ApplyScroll(10f);
            a.ApplyPan(new Vector2(1f, 1f));
            var b = new TileStories.DevPreviewCameraDolly();
            Assert.AreEqual(0f, b.ZoomOffsetMetres, "a new dolly instance must not see another instance's scroll");
            Assert.AreEqual(Vector2.zero, b.PanOffsetMetres, "a new dolly instance must not see another instance's pan");
        }

        // SetOffsets (2026-09-22): the absolute-set carry-over WallSession uses across a rebuild.
        [Test]
        public void SetOffsets_AbsoluteSet_OverwritesWhateverWasThereBefore()
        {
            var dolly = new TileStories.DevPreviewCameraDolly();
            dolly.ApplyScroll(10f);
            dolly.ApplyPan(new Vector2(1f, 1f));

            dolly.SetOffsets(new Vector2(0.3f, -0.2f), 0.75f);

            Assert.AreEqual(0.75f, dolly.ZoomOffsetMetres, 1e-4f);
            Assert.AreEqual(0.3f, dolly.PanOffsetMetres.x, 1e-4f);
            Assert.AreEqual(-0.2f, dolly.PanOffsetMetres.y, 1e-4f);
        }
    }
}

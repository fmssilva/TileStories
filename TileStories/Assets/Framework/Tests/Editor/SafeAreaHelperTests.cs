using UnityEngine;
using NUnit.Framework;
using TileStories;

namespace TileStories.Tests
{
    // Tier-0 pure-logic assertions for SafeAreaHelper.ComputePadding (no Screen read,
    // no panel) -- grounded directly against the math in SafeAreaHelper.cs.
    public class SafeAreaHelperTests
    {
        [Test]
        public void ComputePadding_fullscreen_safe_area_is_empty()
        {
            var pad = SafeAreaHelper.ComputePadding(new Rect(0, 0, 1920, 1080), new Vector2(1920, 1080));
            Assert.IsTrue(pad.IsEmpty);
        }

        // Phone held portrait, notch at TOP: the safe area is inset at the top (height
        // reduced, y still at the bottom edge). UI Toolkit is top-left origin, so this
        // must surface as TOP padding -- the non-obvious case the inversion handles.
        [Test]
        public void ComputePadding_top_notch_maps_to_top_padding()
        {
            var pad = SafeAreaHelper.ComputePadding(new Rect(0, 0, 1920, 990), new Vector2(1920, 1080));
            Assert.That(pad.top, Is.EqualTo(90f).Within(1e-5f));
            Assert.That(pad.bottom, Is.EqualTo(0f).Within(1e-5f));
            Assert.IsFalse(pad.IsEmpty);
        }

        // Bottom cutout (e.g. home-bar area): safe-area y is lifted off the bottom edge,
        // which must surface as BOTTOM padding, not top.
        [Test]
        public void ComputePadding_bottom_cutout_maps_to_bottom_padding()
        {
            var pad = SafeAreaHelper.ComputePadding(new Rect(0, 100, 1920, 980), new Vector2(1920, 1080));
            Assert.That(pad.bottom, Is.EqualTo(100f).Within(1e-5f));
            Assert.That(pad.top, Is.EqualTo(0f).Within(1e-5f));
        }
    }
}

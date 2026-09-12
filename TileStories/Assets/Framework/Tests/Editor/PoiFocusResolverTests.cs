using System.Collections.Generic;
using NUnit.Framework;
using TileStories.Editor;

namespace TileStories.Tests
{
    // Tier-0 tests for PoiFocusResolver: the pure name-resolution step behind
    // the per-POI "Focus in Scene" button. SceneView/Selection cannot run
    // headless, so this tests the real decision logic (null/unknown guards).
    public class PoiFocusResolverTests
    {
        [Test]
        public void NullId_ReturnsNull()
        {
            Assert.IsNull(PoiFocusResolver.ResolveFocusTargetName(
                null, true, new HashSet<string> { "lamp_01" }));
        }

        [Test]
        public void BlankId_ReturnsNull()
        {
            Assert.IsNull(PoiFocusResolver.ResolveFocusTargetName(
                "  ", true, new HashSet<string> { "lamp_01" }));
        }

        [Test]
        public void NoRig_ReturnsNull()
        {
            Assert.IsNull(PoiFocusResolver.ResolveFocusTargetName(
                "lamp_01", false, new HashSet<string> { "lamp_01" }));
        }

        [Test]
        public void UnknownId_ReturnsNull()
        {
            Assert.IsNull(PoiFocusResolver.ResolveFocusTargetName(
                "nope", true, new HashSet<string> { "lamp_01" }));
        }

        [Test]
        public void KnownId_ReturnsSameId()
        {
            Assert.AreEqual("lamp_01", PoiFocusResolver.ResolveFocusTargetName(
                "lamp_01", true, new HashSet<string> { "lamp_01", "painting_02" }));
        }

        [Test]
        public void FocusWidth_OneAndHalfMarkersAcross()
        {
            // Level_2 marker (20cm visual diameter) at 1.5-across = 30cm box:
            // marker fills ~2/3 of the view, sliver of context either side.
            Assert.AreEqual(0.3f, PoiFocusResolver.ComputeFocusWidth(0.2f), 1e-5f);
        }

        [Test]
        public void FocusWidth_LampLevel1_TighterThanTwoAcross()
        {
            // Real lamp data: level_1 = 30cm size_cm -> symbol diameter 0.30m,
            // GetVisualRadiusWorld = 0.15 * 1.18 ring = 0.177 -> focus diameter
            // 0.354m. At 1.5-across = 0.531m. This is the frame the user sees
            // when clicking Focus on the lamp; it must be tighter than the old
            // 2-across 0.708m they rejected.
            float lampFocusDiameter = 0.177f * 2f; // 0.354
            float lampFrame = PoiFocusResolver.ComputeFocusWidth(lampFocusDiameter);
            Assert.AreEqual(0.531f, lampFrame, 1e-3f);
            Assert.IsTrue(lampFrame < 0.7f, "lamp frame must be tighter than the rejected 2-across 0.708m");
        }

        [Test]
        public void FocusWidth_TinyMarker_HitsFloor()
        {
            // Near-zero uGUI-style diameter must not microscope: 0.2m floor.
            Assert.AreEqual(
                PoiFocusResolver.MinFocusWidth,
                PoiFocusResolver.ComputeFocusWidth(0.01f),
                1e-5f);
        }

        [Test]
        public void FocusWidth_ZeroDiameter_HitsFloor()
        {
            Assert.AreEqual(
                PoiFocusResolver.MinFocusWidth,
                PoiFocusResolver.ComputeFocusWidth(0f),
                1e-5f);
        }

        [Test]
        public void FocusWidth_TypicalMarker_NotMaskedByFloor()
        {
            // Level_5 marker (7cm symbol, 0.14m visual diameter with ring) at
            // 1.5-across = 0.21m, above the 0.2m floor: the floor must NOT
            // inflate it (the bug that read as "no zoom change").
            Assert.AreEqual(0.21f, PoiFocusResolver.ComputeFocusWidth(0.14f), 1e-5f);
        }
    }
}

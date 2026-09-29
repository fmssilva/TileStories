using NUnit.Framework;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // The pure preview rules (_3.1 step 10A.2): TurntableRule (model_3d) and PanoramaViewRule (panorama_360)
    public class PreviewRulesTests
    {
        [Test]
        public void Turntable_ADragTurnsYawAndPitch_WithinLimits()
        {
            var s = TurntableRule.Drag(TurntableState.Start, new Vector2(30f, 0f), degreesPerUnit: 1f);
            Assert.AreEqual(30f, s.Yaw, 0.01f);
            Assert.AreEqual(0f, s.Pitch, 0.01f);
            Assert.AreEqual(0f, s.IdleSeconds, "a drag resets the idle clock");

            s = TurntableRule.Drag(TurntableState.Start, new Vector2(0f, 20f), degreesPerUnit: 1f);
            Assert.AreEqual(-20f, s.Pitch, 0.01f, "dragging down looks from above: pitch decreases");
        }

        [Test]
        public void Turntable_YawWraps_PitchAndZoomClamp()
        {
            var s = TurntableRule.Drag(TurntableState.Start, new Vector2(370f, 0f), degreesPerUnit: 1f);
            Assert.AreEqual(10f, s.Yaw, 0.01f, "yaw wraps at 360");

            s = TurntableRule.Drag(TurntableState.Start, new Vector2(0f, -1000f), degreesPerUnit: 1f);
            Assert.AreEqual(TurntableRule.MaxPitch, s.Pitch, 0.01f, "pitch stops short of the pole");
            s = TurntableRule.Drag(TurntableState.Start, new Vector2(0f, 1000f), degreesPerUnit: 1f);
            Assert.AreEqual(TurntableRule.MinPitch, s.Pitch, 0.01f);

            s = TurntableRule.Pinch(TurntableState.Start, 100f);
            Assert.AreEqual(TurntableRule.MaxZoom, s.Zoom, 0.01f);
            s = TurntableRule.Pinch(TurntableState.Start, 0.001f);
            Assert.AreEqual(TurntableRule.MinZoom, s.Zoom, 0.01f);
        }

        [Test]
        public void Turntable_AutoSpin_OnlyResumesAfterAPause_ThenTurnsAtItsOwnSpeed()
        {
            var s = TurntableState.Start;
            s = TurntableRule.Idle(s, TurntableRule.AutoSpinResumeAfter - 0.1f);
            Assert.AreEqual(0f, s.Yaw, 0.01f, "not idle long enough yet: no spin");

            s = TurntableRule.Idle(s, 0.2f); // crosses the resume threshold
            Assert.AreEqual(TurntableRule.AutoSpinSpeed * 0.2f, s.Yaw, 0.01f, "spins only for the time past the threshold... ");

            // A fresh touch resets the idle clock and stops the spin at once
            var touched = TurntableRule.Drag(s, Vector2.zero, 1f);
            Assert.AreEqual(0f, touched.IdleSeconds, "a touch (even a zero-delta one) resets the idle clock");
        }

        [Test]
        public void Panorama_ADragLooksAround_YawWraps_PitchClamps()
        {
            var s = PanoramaViewRule.Drag(PanoramaViewState.Start, new Vector2(90f, 0f), degreesPerUnit: 1f);
            Assert.AreEqual(270f, s.Yaw, 0.01f, "dragging right turns the view left (yaw -90, wrapped into 0..360)");

            s = PanoramaViewRule.Drag(PanoramaViewState.Start, new Vector2(-450f, 0f), degreesPerUnit: 1f);
            Assert.AreEqual(90f, s.Yaw, 0.01f, "yaw wraps");

            s = PanoramaViewRule.Drag(PanoramaViewState.Start, new Vector2(0f, 1000f), degreesPerUnit: 1f);
            Assert.AreEqual(PanoramaViewRule.MaxPitch, s.Pitch, 0.01f, "pitch stops short of straight up");
            s = PanoramaViewRule.Drag(PanoramaViewState.Start, new Vector2(0f, -1000f), degreesPerUnit: 1f);
            Assert.AreEqual(PanoramaViewRule.MinPitch, s.Pitch, 0.01f, "and straight down");
        }

        [Test]
        public void Panorama_APinchChangesFov_WithinLimits()
        {
            var s = PanoramaViewRule.Pinch(PanoramaViewState.Start, 1.4f);
            Assert.AreEqual(PanoramaViewState.DefaultFov / 1.4f, s.Fov, 0.01f, "spreading fingers narrows the field of view");
            s = PanoramaViewRule.Pinch(PanoramaViewState.Start, 100f);
            Assert.AreEqual(PanoramaViewRule.MinFov, s.Fov, 0.01f);
            s = PanoramaViewRule.Pinch(PanoramaViewState.Start, 0.001f);
            Assert.AreEqual(PanoramaViewRule.MaxFov, s.Fov, 0.01f);
        }

        [Test]
        public void Panorama_AGyroReading_SetsYawPitchDirectly_WrappedAndClamped()
        {
            var s = PanoramaViewRule.Gyro(PanoramaViewState.Start, 400f, 200f);
            Assert.AreEqual(40f, s.Yaw, 0.01f);
            Assert.AreEqual(PanoramaViewRule.MaxPitch, s.Pitch, 0.01f);
        }

        [Test]
        public void Panorama_AheadMatchesEquirectRule_TheViewersZeroIsTheGeneratedPicturesMiddle()
        {
            // The viewer's sphere is built from EquirectRule: yaw 0 / pitch 0 must be the same direction the
            // generator calls "ahead" (u=0.5, v=0.5) -- proven once here so a future viewer cannot drift from it.
            Vector3 ahead = EquirectRule.DirectionOf(0.5f, 0.5f);
            Assert.AreEqual(Vector3.forward, ahead, "u=0.5,v=0.5 is +Z: yaw 0 pitch 0 must look the same way");
        }
    }
}

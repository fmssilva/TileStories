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

        // ---------------- ModelFitRule (10A.3-fix.2) ----------------

        [Test]
        public void ModelFit_TheSphereReachesTheFarthestCornerOfTheUnion_AndNoBoxes_IsAPoint()
        {
            var (centre, radius) = ModelFitRule.SphereOf(new[] { new Bounds(new Vector3(1f, 2f, 3f), new Vector3(2f, 4f, 4f)) });
            Assert.AreEqual(new Vector3(1f, 2f, 3f), centre);
            Assert.AreEqual(Mathf.Sqrt(1f + 4f + 4f), radius, 1e-4f, "half-extents (1, 2, 2): the corner is 3 away");

            var two = ModelFitRule.SphereOf(new[] { new Bounds(Vector3.zero, Vector3.one * 2f), new Bounds(new Vector3(10f, 0f, 0f), Vector3.one * 2f) });
            Assert.AreEqual(new Vector3(5f, 0f, 0f), two.Center, "the centre of the union of both boxes");
            Assert.AreEqual(Mathf.Sqrt(6f * 6f + 1f + 1f), two.Radius, 1e-4f, "the farthest corner of the farthest box");

            Assert.AreEqual(0f, ModelFitRule.SphereOf(new Bounds[0]).Radius);
            Assert.AreEqual(0f, ModelFitRule.SphereOf(null).Radius);
        }

        [Test]
        public void ModelFit_AtTheFittedDistance_TheSpheresSilhouetteSpansTheTargetOfTheShorterSide()
        {
            // The silhouette half-angle of a sphere seen from d is asin(r / d): at the fitted distance its tangent must be
            // `fill` times the shorter axis' half-FOV tangent, whatever the stage's shape.
            foreach (float aspect in new[] { 0.5f, 1f, 1.6f })
            {
                float distance = ModelFitRule.DistanceFor(2f, 60f, aspect);
                float silhouetteTan = Mathf.Tan(Mathf.Asin(2f / distance));
                float shorterAxisTan = Mathf.Tan(30f * Mathf.Deg2Rad) * Mathf.Min(1f, aspect);
                Assert.AreEqual(ModelFitRule.TargetFillOfShorterSide, silhouetteTan / shorterAxisTan, 1e-3f, "aspect " + aspect);
            }
        }

        [Test]
        public void ModelFit_ABiggerModelStandsFartherBack_AWiderStageDoesNotMoveItBecauseTheShorterSideDecides()
        {
            Assert.AreEqual(2f * ModelFitRule.DistanceFor(1f, 60f, 1f), ModelFitRule.DistanceFor(2f, 60f, 1f), 1e-3f, "distance is linear in the radius");
            Assert.AreEqual(ModelFitRule.DistanceFor(1f, 60f, 1.6f), ModelFitRule.DistanceFor(1f, 60f, 3f), 1e-4f, "landscape: the height is the shorter side, the width no longer matters");
            Assert.Greater(ModelFitRule.DistanceFor(1f, 60f, 0.5f), ModelFitRule.DistanceFor(1f, 60f, 1f), "a narrow (portrait) stage needs the camera farther back");
            Assert.Greater(ModelFitRule.DistanceFor(0f, 60f, 1f), 0f, "a degenerate model still gets a positive distance");
        }

        // ---------------- PanoramaSphereRule (10A.4.1) ----------------

        [Test]
        public void PanoramaSphere_EveryVertexSitsWhereItsUvLooks_AheadIsThePicturesMiddle()
        {
            var g = PanoramaSphereRule.Build(10f, 16, 8);
            Assert.AreEqual(17 * 9, g.Vertices.Length, "(segments + 1) x (rings + 1) vertices");
            Assert.AreEqual(g.Vertices.Length, g.Uvs.Length);
            Assert.AreEqual(16 * 8 * 6, g.Triangles.Length);
            for (int i = 0; i < g.Vertices.Length; i++)
            {
                Assert.AreEqual(10f, g.Vertices[i].magnitude, 1e-3f, "on the sphere");
                Vector3 expected = EquirectRule.DirectionOf(g.Uvs[i].x, g.Uvs[i].y) * 10f;
                Assert.AreEqual(0f, (g.Vertices[i] - expected).magnitude, 1e-3f, "the vertex looks where EquirectRule says its uv does");
            }
            int Find(float u, float v) { for (int i = 0; i < g.Uvs.Length; i++) if (Mathf.Approximately(g.Uvs[i].x, u) && Mathf.Approximately(g.Uvs[i].y, v)) return i; return -1; }
            Assert.AreEqual(0f, (g.Vertices[Find(0.5f, 0.5f)] - new Vector3(0f, 0f, 10f)).magnitude, 1e-3f, "the picture's middle is straight ahead (+Z)");
            Assert.AreEqual(0f, (g.Vertices[Find(0.75f, 0.5f)] - new Vector3(10f, 0f, 0f)).magnitude, 1e-3f, "a quarter right of the middle is +X: not mirrored");
            Assert.AreEqual(0f, (g.Vertices[Find(0.5f, 1f)] - new Vector3(0f, 10f, 0f)).magnitude, 1e-3f, "the picture's top is straight up");
        }

        [Test]
        public void PanoramaSphere_TheSeamRepeatsItsDirectionWithUZeroAndOne_SoNoTexelStretchesAcrossTheWrap()
        {
            var g = PanoramaSphereRule.Build(1f, 16, 8);
            int columns = 17;
            for (int row = 0; row <= 8; row++)
            {
                int first = row * columns, last = first + 16;
                Assert.AreEqual(0f, g.Uvs[first].x);
                Assert.AreEqual(1f, g.Uvs[last].x);
                Assert.AreEqual(0f, (g.Vertices[first] - g.Vertices[last]).magnitude, 1e-4f, "row " + row + ": the same direction at both edges of the picture");
            }
        }

        [Test]
        public void PanoramaSphere_EveryTriangleFacesTheCentre_ItIsDrawnFromInside()
        {
            var g = PanoramaSphereRule.Build(5f, 16, 8);
            int facing = 0;
            for (int t = 0; t < g.Triangles.Length; t += 3)
            {
                Vector3 a = g.Vertices[g.Triangles[t]], b = g.Vertices[g.Triangles[t + 1]], c = g.Vertices[g.Triangles[t + 2]];
                Vector3 normal = Vector3.Cross(b - a, c - a); // Unity: the normal of a clockwise triangle points at the viewer
                if (normal.sqrMagnitude < 1e-10f) continue;   // a sliver at a pole: nothing to face
                Assert.Less(Vector3.Dot(normal, (a + b + c) / 3f), 0f, "triangle " + t / 3 + " must face the camera at the centre");
                facing++;
            }
            Assert.Greater(facing, 16 * 8, "most triangles are real (the pole slivers aside)");
        }

        [Test]
        public void PanoramaSphere_AsksForAtLeastAUsableGrid()
        {
            var g = PanoramaSphereRule.Build(1f, 0, 0);
            Assert.AreEqual(4 * 3, g.Vertices.Length, "3 segments x 2 rings at the least");
        }

        // ---------------- PanoramaViewRule.DegreesPerUnit + PanoramaGyroRule (10A.4.2) ----------------

        [Test]
        public void Panorama_ADragAcrossTheStagesShorterSide_TurnsTheViewByExactlyTheFieldOfView()
        {
            Assert.AreEqual(70f / 200f, PanoramaViewRule.DegreesPerUnit(70f, 200f), 1e-5f);
            var s = PanoramaViewRule.Drag(PanoramaViewState.Start, new Vector2(0f, 200f), PanoramaViewRule.DegreesPerUnit(70f, 200f));
            Assert.AreEqual(70f, s.Pitch, 1e-3f, "a drag the stage's whole shorter side down looks up by one field of view: the scene follows the finger");
            Assert.Less(PanoramaViewRule.DegreesPerUnit(40f, 200f), PanoramaViewRule.DegreesPerUnit(100f, 200f), "zoomed in, the same drag turns the view less");
            Assert.AreEqual(70f, PanoramaViewRule.DegreesPerUnit(70f, 0f), "a stage not laid out yet counts as one unit, never a divide by zero");
            Assert.AreEqual(70f, PanoramaViewRule.DegreesPerUnit(70f, float.NaN));
        }

        [Test]
        public void Panorama_TheFieldOfViewIsMeasuredOnTheShorterSide_SoAZoomFeelsTheSameInAnyFrame()
        {
            Assert.AreEqual(70f, PanoramaViewRule.VerticalFov(70f, 1.6f), 1e-4f, "a wide stage: the vertical field IS the field");
            Assert.AreEqual(70f, PanoramaViewRule.VerticalFov(70f, 1f), 1e-4f);
            // - a tall stage (0.5 = half as wide as tall): the field spans its WIDTH, so the camera's vertical field is wider
            float tall = PanoramaViewRule.VerticalFov(70f, 0.5f);
            Assert.Greater(tall, 70f);
            float horizontal = 2f * Mathf.Atan(Mathf.Tan(tall * 0.5f * Mathf.Deg2Rad) * 0.5f) * Mathf.Rad2Deg;
            Assert.AreEqual(70f, horizontal, 1e-3f, "and across the stage's width it shows exactly the field of view");
            Assert.AreEqual(70f, PanoramaViewRule.VerticalFov(70f, 0f), "an unknown shape leaves the field as it is");
        }

        // A phone held upright, its back facing the horizon straight ahead: a right-handed +90 degrees about the sensor's x axis
        private static readonly Quaternion UprightAhead = Quaternion.AngleAxis(90f, Vector3.right);

        // The sensor frame's "up" is +z (x east, y north): the holder turning clockwise seen from above is a NEGATIVE turn about it
        private static Quaternion TurnedRight(float degrees) => Quaternion.AngleAxis(-degrees, Vector3.forward) * UprightAhead;

        [Test]
        public void Gyro_AFlatPhoneLooksDown_AnUprightOneLooksAhead()
        {
            var (_, flatPitch) = PanoramaGyroRule.ViewAngles(Quaternion.identity);
            // - asin is steepest at the poles: a float error of 1e-7 in the forward vector reads as 0.03 degrees there, the tolerance
            Assert.AreEqual(-90f, flatPitch, 0.05f, "flat on a table, its back camera looks straight down");
            var (yaw, pitch) = PanoramaGyroRule.ViewAngles(UprightAhead);
            Assert.AreEqual(0f, yaw, 0.01f);
            Assert.AreEqual(0f, pitch, 0.01f, "held upright it looks at the horizon");
        }

        [Test]
        public void Gyro_TurningThePhoneRight_TurnsTheViewRight_AndLeaningBackLooksUp()
        {
            Assert.AreEqual(30f, PanoramaGyroRule.ViewAngles(TurnedRight(30f)).Yaw, 0.01f, "yaw grows turning right, like a drag-free 'look right'");
            Assert.AreEqual(-30f, PanoramaGyroRule.ViewAngles(TurnedRight(-30f)).Yaw, 0.01f);
            // - further than upright (the top leaning away from the holder) the camera looks up at the ceiling
            var leaningBack = UprightAhead * Quaternion.AngleAxis(25f, Vector3.right);
            Assert.AreEqual(25f, PanoramaGyroRule.ViewAngles(leaningBack).Pitch, 0.01f, "looking up is a positive pitch, as the panorama's own");
            Assert.AreEqual(-25f, PanoramaGyroRule.ViewAngles(UprightAhead * Quaternion.AngleAxis(-25f, Vector3.right)).Pitch, 0.01f);
        }

        [Test]
        public void GyroFollower_TheFirstReadingIsTheStartHeading_ThenTheViewTurnsByHowFarThePhoneTurned()
        {
            var follower = new PanoramaGyroFollower(90f);
            Assert.IsFalse(follower.IsAnchored);
            // - the visitor happens to face the sensor's 40 degrees when the viewer opens: that direction IS the authored heading
            var first = follower.Follow(PanoramaViewState.Start, TurnedRight(40f));
            Assert.AreEqual(90f, first.Yaw, 0.01f);
            Assert.IsTrue(follower.IsAnchored);
            var later = follower.Follow(first, TurnedRight(70f));
            Assert.AreEqual(120f, later.Yaw, 0.01f, "30 degrees further right on the phone: 30 further right in the view");
            var back = follower.Follow(later, TurnedRight(10f));
            Assert.AreEqual(60f, back.Yaw, 0.01f, "and back past the start, wrapping through 0..360 like a drag");
            var wrapped = follower.Follow(back, TurnedRight(-60f));
            Assert.AreEqual(350f, wrapped.Yaw, 0.01f);
        }

        [Test]
        public void GyroFollower_KeepsTheVisitorsZoom_ClampsThePitch_AndResetsToAnotherAnchor()
        {
            var follower = new PanoramaGyroFollower(0f);
            var zoomed = new PanoramaViewState(0f, 0f, 45f);
            var s = follower.Follow(zoomed, UprightAhead);
            Assert.AreEqual(45f, s.Fov, "a pinch is the visitor's own: the phone never changes it");
            s = follower.Follow(s, Quaternion.identity);
            Assert.AreEqual(PanoramaViewRule.MinPitch, s.Pitch, 0.01f, "flat on a table the view stops short of straight down");
            follower.Reset();
            Assert.IsFalse(follower.IsAnchored);
            Assert.AreEqual(0f, follower.Follow(s, TurnedRight(75f)).Yaw, 0.01f, "after a Reset the next reading is the start heading again");
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

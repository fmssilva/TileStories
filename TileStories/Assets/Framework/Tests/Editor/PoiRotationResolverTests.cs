using NUnit.Framework;
using TileStories.Editor;
using UnityEngine;

namespace TileStories.Tests
{
    // Tier-0 tests for PoiRotationResolver: the pure yaw-normalise + quaternion
    // math behind the per-POI "Edit Rotation" slider. Runtime MarkerBillboard
    // always faces markers to the camera, so this yaw is editor-preview only --
    // but the mapping config-angle -> scene rotation must be exact and stable,
    // and the default (0) must be identity so a new/wall-config POI never
    // visually changes a marker. Pure math, no SceneView needed.
    public class PoiRotationResolverTests
    {
        [Test]
        public void Zero_IsIdentityRotation()
        {
            Assert.AreEqual(Quaternion.identity, PoiRotationResolver.ToYawQuaternion(0f));
        }

        [Test]
        public void NinetyDeg_IsQuarterTurnAroundY()
        {
            Quaternion q = PoiRotationResolver.ToYawQuaternion(90f);
            // eulerAngles returns in [0,360); expect yaw 90, no pitch/roll.
            Assert.AreEqual(90f, q.eulerAngles.y, 1e-4f);
            Assert.AreEqual(0f, q.eulerAngles.x, 1e-4f);
            Assert.AreEqual(0f, q.eulerAngles.z, 1e-4f);
        }

        [Test]
        public void Normalize_KeepInRange_360WrapsToZero()
        {
            Assert.AreEqual(0f, PoiRotationResolver.NormalizeYawDeg(360f), 1e-5f);
            Assert.AreEqual(180f, PoiRotationResolver.NormalizeYawDeg(180f), 1e-5f);
            Assert.AreEqual(720f % 360f, PoiRotationResolver.NormalizeYawDeg(720f), 1e-5f);
        }

        [Test]
        public void Normalize_Negative_FoldsPositive()
        {
            Assert.AreEqual(270f, PoiRotationResolver.NormalizeYawDeg(-90f), 1e-5f);
            Assert.AreEqual(350f, PoiRotationResolver.NormalizeYawDeg(-370f), 1e-5f);
        }

        [Test]
        public void DefaultConstant_IsZero()
        {
            Assert.AreEqual(0, PoiRotationResolver.DefaultEditorRotationDeg);
        }

        [Test]
        public void ConfigField_RoundTripsThroughJson()
        {
            var poi = new POIData { id = "lamp_01", editor_rotation_deg = 123f };
            string json = JsonUtility.ToJson(poi);
            var back = JsonUtility.FromJson<POIData>(json);
            Assert.AreEqual(123f, back.editor_rotation_deg, 1e-5f);
        }
    }
}
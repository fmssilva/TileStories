using NUnit.Framework;
using TileStories.Editor;
using UnityEngine;

namespace TileStories.Tests
{
    // Tier-0 tests for PoiRotationResolver: the pure angle-normalise + quaternion
    // math behind the per-POI Facing X/Y/Z sliders. The mapping config-angle ->
    // scene rotation must be exact and stable, and zero on every axis must be
    // identity so a new POI never visually changes a marker. Pure math, no
    // SceneView needed.
    public class PoiRotationResolverTests
    {
        [Test]
        public void Zero_IsIdentityRotation()
        {
            Assert.AreEqual(Quaternion.identity, PoiRotationResolver.ToEulerQuaternion(0f, 0f, 0f));
        }

        [Test]
        public void NinetyDeg_IsQuarterTurnAroundY()
        {
            Quaternion q = PoiRotationResolver.ToEulerQuaternion(0f, 90f, 0f);
            // eulerAngles returns in [0,360); expect yaw 90, no pitch/roll.
            Assert.AreEqual(90f, q.eulerAngles.y, 1e-4f);
            Assert.AreEqual(0f, q.eulerAngles.x, 1e-4f);
            Assert.AreEqual(0f, q.eulerAngles.z, 1e-4f);
        }

        [Test]
        public void Normalize_KeepInRange_360WrapsToZero()
        {
            Assert.AreEqual(0f, PoiRotationResolver.NormalizeAngleDeg(360f), 1e-5f);
            Assert.AreEqual(180f, PoiRotationResolver.NormalizeAngleDeg(180f), 1e-5f);
            Assert.AreEqual(720f % 360f, PoiRotationResolver.NormalizeAngleDeg(720f), 1e-5f);
        }

        [Test]
        public void Normalize_Negative_FoldsPositive()
        {
            Assert.AreEqual(270f, PoiRotationResolver.NormalizeAngleDeg(-90f), 1e-5f);
            Assert.AreEqual(350f, PoiRotationResolver.NormalizeAngleDeg(-370f), 1e-5f);
        }

        [Test]
        public void ConfigField_RoundTripsThroughJson()
        {
            var poi = new POIData { id = "lamp_01", editor_rotation_deg = 123f, editor_rotation_x_deg = 15f, editor_rotation_z_deg = 45f };
            string json = JsonUtility.ToJson(poi);
            var back = JsonUtility.FromJson<POIData>(json);
            Assert.AreEqual(123f, back.editor_rotation_deg, 1e-5f);
            Assert.AreEqual(15f, back.editor_rotation_x_deg, 1e-5f);
            Assert.AreEqual(45f, back.editor_rotation_z_deg, 1e-5f);
        }

        [Test]
        public void ToEulerQuaternion_Zero_IsIdentity()
        {
            Assert.AreEqual(Quaternion.identity, PoiRotationResolver.ToEulerQuaternion(0f, 0f, 0f));
        }

        [Test]
        public void ToEulerQuaternion_AppliesPitchYawRoll()
        {
            Quaternion q = PoiRotationResolver.ToEulerQuaternion(30f, 45f, 60f);
            Assert.AreEqual(30f, q.eulerAngles.x, 1e-4f);
            Assert.AreEqual(45f, q.eulerAngles.y, 1e-4f);
            Assert.AreEqual(60f, q.eulerAngles.z, 1e-4f);
        }

        [Test]
        public void ToEulerQuaternion_NormalizesEachAxisIntoRange()
        {
            Quaternion q = PoiRotationResolver.ToEulerQuaternion(720f, -90f, 370f);
            Assert.AreEqual(0f, q.eulerAngles.x, 1e-4f);
            Assert.AreEqual(270f, q.eulerAngles.y, 1e-4f);
            Assert.AreEqual(10f, q.eulerAngles.z, 1e-4f);
        }

        [Test]
        public void NormalizeAngleDeg_FoldsOutOfRangeAndNegative()
        {
            Assert.AreEqual(0f, PoiRotationResolver.NormalizeAngleDeg(360f), 1e-5f);
            Assert.AreEqual(270f, PoiRotationResolver.NormalizeAngleDeg(-90f), 1e-5f);
            Assert.AreEqual(10f, PoiRotationResolver.NormalizeAngleDeg(730f), 1e-5f);
        }

        [Test]
        public void SyncPoiRotationFromScene_WritesAllThreeAnglesAndReportsChange()
        {
            var poi = new POIData { editor_rotation_deg = 0f, editor_rotation_x_deg = 0f, editor_rotation_z_deg = 0f };
            bool changed = POIEditorToolWindow.SyncPoiRotationFromScene(poi, new Vector3(15f, 90f, 45f));

            Assert.IsTrue(changed);
            Assert.AreEqual(15f, poi.editor_rotation_x_deg, 1e-4f);
            Assert.AreEqual(90f, poi.editor_rotation_deg, 1e-4f);
            Assert.AreEqual(45f, poi.editor_rotation_z_deg, 1e-4f);
        }

        [Test]
        public void SyncPoiRotationFromScene_NormalizesBeforeWriting()
        {
            var poi = new POIData { editor_rotation_deg = 0f, editor_rotation_x_deg = 0f, editor_rotation_z_deg = 0f };
            POIEditorToolWindow.SyncPoiRotationFromScene(poi, new Vector3(0f, -90f, 0f));

            Assert.AreEqual(270f, poi.editor_rotation_deg, 1e-4f, "-90 scene yaw should normalize to 270.");
        }

        [Test]
        public void SyncPoiRotationFromScene_NoChangeWhenAlreadyInSync()
        {
            var poi = new POIData { editor_rotation_deg = 45f, editor_rotation_x_deg = 30f, editor_rotation_z_deg = 60f };
            bool changed = POIEditorToolWindow.SyncPoiRotationFromScene(poi, new Vector3(30f, 45f, 60f));

            Assert.IsFalse(changed, "Matching angles should be a no-op (avoid dirty spam during Scene repaints).");
        }
    }
}
using NUnit.Framework;
using UnityEngine;

namespace TileStories.Tests
{
    public class EditorCameraLookTests
    {
        // Fixed deltaTime to keep assertions deterministic.
        private const float Dt = 0.02f;

        [Test]
        public void ApplyDelta_ZeroDelta_ReturnsIdentity()
        {
            var result = EditorCameraLook.ApplyDelta(Quaternion.identity, Vector2.zero, Dt);

            Assert.AreEqual(0f, result.eulerAngles.x, 0.001f);
            Assert.AreEqual(0f, result.eulerAngles.y, 0.001f);
            Assert.AreEqual(0f, result.eulerAngles.z, 0.001f);
        }

        [Test]
        public void ApplyDelta_PositiveDeltaX_IncreasesYaw()
        {
            // yaw grows: 0 + 1.0 * 0.02 * 10 = 0.2 degrees
            var result = EditorCameraLook.ApplyDelta(Quaternion.identity,
                new Vector2(1.0f, 0f), Dt);

            Assert.AreEqual(0.2f, result.eulerAngles.y, 0.01f);
        }

        [Test]
        public void ApplyDelta_PositiveDeltaY_DecreasesPitch()
        {
            // pitch -= 1.0 * 0.02 * 10 = -0.2 (looking up); wraps to 359.8
            var result = EditorCameraLook.ApplyDelta(Quaternion.identity,
                new Vector2(0f, 1.0f), Dt);

            Assert.AreEqual(359.8f, result.eulerAngles.x, 0.01f);
        }

        // THE KEY TEST: the original bug snapped back to a hardcoded (0,0) origin
        // instead of using the camera's current rotation.
        [Test]
        public void ApplyDelta_FromNonZeroOrigin_RotatesRelativeToCurrentRotation()
        {
            // Camera already faces 45 degrees right (e.g. from prior mouse-look).
            Quaternion startRotation = Quaternion.Euler(0f, 45f, 0f);

            // Small left-turn delta.
            var result = EditorCameraLook.ApplyDelta(startRotation,
                new Vector2(-1.0f, 0f), Dt);

            // Yaw should be 45 - 0.2 = 44.8, NOT -0.2 (which the bug produced).
            Assert.AreEqual(44.8f, result.eulerAngles.y, 0.01f);
        }

        [Test]
        public void ApplyDelta_LargeNegativeDeltaY_ClampsPitchDownAt80()
        {
            var result = EditorCameraLook.ApplyDelta(Quaternion.identity,
                new Vector2(0f, -1000f), Dt);

            // pitch = 0 - (-1000 * 0.02 * 10) = 200 -> clamped to 80
            Assert.AreEqual(80f, result.eulerAngles.x, 0.1f);
        }

        [Test]
        public void ApplyDelta_LargePositiveDeltaY_ClampsPitchUpAtMinus80()
        {
            var result = EditorCameraLook.ApplyDelta(Quaternion.identity,
                new Vector2(0f, 1000f), Dt);

            // pitch = 0 - (1000 * 0.02 * 10) = -200 -> clamped to -80 -> wraps to 280
            Assert.AreEqual(280f, result.eulerAngles.x, 0.1f);
        }

        [Test]
        public void ApplyDelta_PitchAlreadyNegative_ClampsCorrectly()
        {
            // Camera looking 70 degrees up (pitch = -70, stored as 290 in eulerAngles).
            Quaternion startRotation = Quaternion.Euler(-70f, 0f, 0f);

            // Additional large up-delta should clamp at -80 (wraps to 280).
            var result = EditorCameraLook.ApplyDelta(startRotation,
                new Vector2(0f, 1000f), Dt);

            // 290 unwrapped -> -70; -70 - 200 = -270 -> clamped to -80 -> 280
            Assert.AreEqual(280f, result.eulerAngles.x, 0.1f);
        }
    }
}

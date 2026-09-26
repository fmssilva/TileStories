using NUnit.Framework;
using TileStories;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // DevCameraInput reads real global Input System / EditorWindow state, so a headless test run
    // (no real mouse/keyboard events, no real mouse-over-window) can only prove the safe, always-true
    // baseline: with nothing pressed and no window under the mouse, every axis is exactly zero, and
    // the call never throws. The gated (Game-view-only) and per-key branches are exercised live by a
    // human per _2.3_Marker_Hierarchy.md's Playmode guide -- Input System device state cannot be
    // synthesized from an Editor test without a real OS input event.
    public class DevCameraInputTests
    {
        [Test]
        public void ReadThisFrame_WithNothingPressed_ReturnsExactlyNone()
        {
            var frame = DevCameraInput.ReadThisFrame();

            Assert.AreEqual(Vector2.zero, frame.LookDelta);
            Assert.AreEqual(0f, frame.RollDelta);
            Assert.AreEqual(Vector3.zero, frame.MoveDelta);
        }

        [Test]
        public void None_IsTheSameZeroValue_ReadThisFrameFallsBackTo()
        {
            var none = DevCameraInputFrame.None;
            Assert.AreEqual(Vector2.zero, none.LookDelta);
            Assert.AreEqual(0f, none.RollDelta);
            Assert.AreEqual(Vector3.zero, none.MoveDelta);
        }
    }
}

using UnityEngine;

namespace TileStories
{
    // One frame's worth of free-fly camera input, read from the Input System. Shared by every
    // Editor/dev-build "fly around and look" camera in this project: MockLocalizationProvider
    // (moves the real Camera.main to test wall tracking) and the demo grid cameras'
    // EffectsPreviewFocus/OutlinePreviewFocus (pan/zoom their own dedicated camera via
    // DevPreviewCameraDolly). One input reader, several different appliers -- each consumer
    // decides HOW to use the numbers (move a Transform directly vs. offset a constrained dolly),
    // but WHAT the player pressed is read in exactly one place.
    public readonly struct DevCameraInputFrame
    {
        // Mouse/arrow-key yaw-pitch delta this frame (matches EditorCameraLook.ApplyDelta's
        // existing delta convention: x = yaw, y = pitch, NOT yet scaled by sensitivity/deltaTime).
        public readonly Vector2 LookDelta;

        // Z/C roll delta this frame, in degrees (NOT yet scaled by rollSpeed * deltaTime).
        public readonly float RollDelta;

        // WASD + Q/E movement, in camera-local axes (x = right, y = up, z = forward), unit
        // vector components only (NOT yet scaled by moveSpeed * deltaTime).
        public readonly Vector3 MoveDelta;

        public DevCameraInputFrame(Vector2 lookDelta, float rollDelta, Vector3 moveDelta)
        {
            LookDelta = lookDelta;
            RollDelta = rollDelta;
            MoveDelta = moveDelta;
        }

        public static readonly DevCameraInputFrame None = new DevCameraInputFrame(Vector2.zero, 0f, Vector3.zero);
    }

    public static class DevCameraInput
    {
#if UNITY_EDITOR
        // Only the Game view (where Play Mode actually renders) should drive a free-fly camera.
        // Without this, scrolling or WASD-ing while the mouse is over the POI Editor window (or
        // any other Editor panel) still moves the camera, because Mouse/Keyboard.current reflect
        // OS-level state, not which EditorWindow has focus -- the exact bug reported for the demo
        // grid's scroll zoom (it moved the grid AND scrolled the POI Editor's own GUI at once).
        private static bool MouseIsOverGameView()
        {
            var window = UnityEditor.EditorWindow.mouseOverWindow;
            return window != null && window.GetType().Name == "GameView";
        }
#endif

        // Reads this frame's WASD/look/roll input. Returns DevCameraInputFrame.None outside the
        // Editor, when no mouse/keyboard is present, or when the mouse is not over the Game view.
        public static DevCameraInputFrame ReadThisFrame()
        {
#if UNITY_EDITOR
            if (!MouseIsOverGameView()) return DevCameraInputFrame.None;

            var mouse = UnityEngine.InputSystem.Mouse.current;
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (mouse == null && kb == null) return DevCameraInputFrame.None;

            Vector2 look = Vector2.zero;
            bool lookActive = false;

            // 1. Right mouse button + mouse.
            if (mouse != null && mouse.rightButton.isPressed)
            {
                look = mouse.delta.ReadValue();
                lookActive = true;
            }

            // 2. Alt + left mouse button.
            if (!lookActive && mouse != null && mouse.leftButton.isPressed &&
                kb != null && (kb.leftAltKey.isPressed || kb.rightAltKey.isPressed))
            {
                look = mouse.delta.ReadValue();
            }

            // 3. Arrow keys (additive with the two above -- rare to combine, harmless if so).
            if (kb != null)
            {
                float arrowYaw = 0f, arrowPitch = 0f;
                if (kb.leftArrowKey.isPressed) arrowYaw -= 1f;
                if (kb.rightArrowKey.isPressed) arrowYaw += 1f;
                if (kb.upArrowKey.isPressed) arrowPitch -= 1f;
                if (kb.downArrowKey.isPressed) arrowPitch += 1f;
                if (arrowYaw != 0f || arrowPitch != 0f)
                    look += new Vector2(arrowYaw, -arrowPitch);
            }

            float roll = 0f;
            if (kb != null)
            {
                if (kb.zKey.isPressed) roll -= 1f;
                if (kb.cKey.isPressed) roll += 1f;
            }

            Vector3 move = Vector3.zero;
            if (kb != null)
            {
                if (kb.wKey.isPressed) move += Vector3.forward;
                if (kb.sKey.isPressed) move -= Vector3.forward;
                if (kb.aKey.isPressed) move -= Vector3.right;
                if (kb.dKey.isPressed) move += Vector3.right;
                if (kb.eKey.isPressed) move += Vector3.up;
                if (kb.qKey.isPressed) move -= Vector3.up;
            }

            return new DevCameraInputFrame(look, roll, move);
#else
            return DevCameraInputFrame.None;
#endif
        }
    }
}

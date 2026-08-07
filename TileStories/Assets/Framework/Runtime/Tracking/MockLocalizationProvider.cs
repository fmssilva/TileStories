using System;
using UnityEngine;

namespace TileStories
{
    /// <summary>
    /// Editor / dev-device mock. Immediately fires OnWallLocalised so POIs appear
    /// without needing a real Immersal scan. Adds WASD + mouse-look in the Editor.
    /// Attach this instead of ImmersalWallTracker when testing locally.
    /// In the Editor, look is available via: right mouse, Alt + left mouse, or arrow keys.
    /// Movement is via WASD (horizontal) and Q/E (vertical). Look is via arrows, mouse, or Alt+click.
    /// </summary>
    public class MockLocalizationProvider : MonoBehaviour, IWallTracker
    {
        [Tooltip("Offset applied to Pose.identity so the 'wall' sits in front of the camera.")]
        [SerializeField] private Vector3 wallOffset = new Vector3(0f, 0f, 2f);

        [Tooltip("How fast the camera moves with WASD / QE in the Editor (metres/sec).")]
        [SerializeField] private float moveSpeed = 2f;

        [Tooltip("Mouse-look sensitivity in the Editor.")]
        [SerializeField] private float lookSensitivity = 2f;

        public bool IsLocalised { get; private set; }
        public Pose CurrentPose { get; private set; }

        public event Action<Pose> OnWallLocalised;
        public event Action OnTrackingLost;

        private void Start()
        {
            var pose = new Pose(wallOffset, Quaternion.identity);
            CurrentPose = pose;
            IsLocalised = true;
            OnWallLocalised?.Invoke(pose);
        }

#if UNITY_EDITOR
        private void Update()
        {
            var cam = Camera.main.transform;
            if (cam == null) return;

            var mouse = UnityEngine.InputSystem.Mouse.current;
            var kb = UnityEngine.InputSystem.Keyboard.current;

            bool lookActive = false;

            // --- LOOK INPUTS (all additive, non-exclusive) ---

            // 1. Right mouse button + mouse (existing)
            if (mouse != null && mouse.rightButton.isPressed)
            {
                ApplyLookDelta(mouse.delta.ReadValue());
                lookActive = true;
            }

            // 2. Alt + left mouse button (new)
            if (!lookActive && mouse != null && mouse.leftButton.isPressed &&
                kb != null && (kb.leftAltKey.isPressed || kb.rightAltKey.isPressed))
            {
                ApplyLookDelta(mouse.delta.ReadValue());
            }

            // 3. Arrow keys for look (new)
            if (kb != null)
            {
                float arrowYaw = 0f, arrowPitch = 0f;
                if (kb.leftArrowKey.isPressed)  arrowYaw -= 1f;
                if (kb.rightArrowKey.isPressed) arrowYaw += 1f;
                if (kb.upArrowKey.isPressed)    arrowPitch -= 1f;
                if (kb.downArrowKey.isPressed)  arrowPitch += 1f;

                if (arrowYaw != 0f || arrowPitch != 0f)
                {
                    float step = lookSensitivity * Time.deltaTime * 100f;
                    ApplyLookDelta(new Vector2(arrowYaw * step, -arrowPitch * step));
                }
            }

            // --- MOVEMENT (unchanged) ---
            if (kb == null) return;
            var move = Vector3.zero;
            if (kb.wKey.isPressed) move += cam.forward;
            if (kb.sKey.isPressed) move -= cam.forward;
            if (kb.aKey.isPressed) move -= cam.right;
            if (kb.dKey.isPressed) move += cam.right;
            if (kb.eKey.isPressed) move += Vector3.up;      // E = ascend
            if (kb.qKey.isPressed) move -= Vector3.up;      // Q = descend
            cam.position += move * (moveSpeed * Time.deltaTime);
        }

        // Delegate to EditorCameraLook for the pure rotation math; this keeps
        // the MonoBehaviour thin and the math independently unit-testable.
        private void ApplyLookDelta(Vector2 delta)
        {
            var cam = Camera.main.transform;
            cam.localRotation = EditorCameraLook.ApplyDelta(cam.localRotation, delta, Time.deltaTime);
        }
#endif
    }
}

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

        [Tooltip("Roll speed for Z/C keys in the Editor (degrees/sec).")]
        [SerializeField] private float rollSpeed = 45f;

        private float _rollDeg;

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
        // Reads WASD/look/roll input through the shared DevCameraInput (Runtime/DevTools) --
        // the same reader the demo grid cameras use (EffectsPreviewFocus/OutlinePreviewFocus) --
        // so this project has exactly one place that knows which keys/mouse buttons mean what,
        // gated on the mouse actually being over the Game view.
        private void Update()
        {
            // - check the camera BEFORE touching .transform: with no main camera (a scene unloading,
            //   a test that owns its own camera) the old order threw every frame
            var main = Camera.main;
            if (main == null) return;
            var cam = main.transform;

            var input = DevCameraInput.ReadThisFrame();

            // lookSensitivity now scales every look source uniformly (mouse-drag and arrow keys
            // alike) -- previously mouse-drag silently ignored this field entirely, which is why
            // "Mouse-look sensitivity" not doing anything to the mouse was a latent bug, not
            // intentional behaviour, fixed here as part of unifying onto DevCameraInput.
            if (input.LookDelta != Vector2.zero)
                ApplyLookDelta(input.LookDelta * lookSensitivity);

            if (input.RollDelta != 0f)
            {
                _rollDeg += input.RollDelta * rollSpeed * Time.deltaTime;
                ApplyLookDelta(Vector2.zero);
            }

            if (input.MoveDelta != Vector3.zero)
            {
                Vector3 move = input.MoveDelta.z * cam.forward + input.MoveDelta.x * cam.right + input.MoveDelta.y * Vector3.up;
                cam.position += move * (moveSpeed * Time.deltaTime);
            }
        }

        // Delegate to EditorCameraLook for the pure rotation math; this keeps
        // the MonoBehaviour thin and the math independently unit-testable.
        private void ApplyLookDelta(Vector2 delta)
        {
            var main = Camera.main;
            if (main == null) return;
            var cam = main.transform;
            cam.localRotation = EditorCameraLook.ApplyDelta(cam.localRotation, delta, Time.deltaTime, _rollDeg);
        }
#endif
    }
}

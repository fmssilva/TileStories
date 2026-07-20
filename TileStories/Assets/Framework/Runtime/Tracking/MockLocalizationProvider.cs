using System;
using UnityEngine;

namespace TileStories
{
    /// <summary>
    /// Editor / dev-device mock. Immediately fires OnWallLocalised so POIs appear
    /// without needing a real Immersal scan. Adds WASD + mouse-look in the Editor.
    /// Attach this instead of ImmersalWallTracker when testing locally.
    /// </summary>
    public class MockLocalizationProvider : MonoBehaviour, IWallTracker
    {
        [Tooltip("Offset applied to Pose.identity so the 'wall' sits in front of the camera.")]
        [SerializeField] private Vector3 wallOffset = new Vector3(0f, 0f, 2f);

        [Tooltip("How fast the camera moves with WASD in the Editor (metres/sec).")]
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
        private float _pitch;
        private float _yaw;

        private void Update()
        {
            var cam = Camera.main.transform;

            // Mouse look — hold right-mouse button
            if (UnityEngine.InputSystem.Mouse.current != null &&
                UnityEngine.InputSystem.Mouse.current.rightButton.isPressed)
            {
                var delta = UnityEngine.InputSystem.Mouse.current.delta.ReadValue();
                _yaw   += delta.x * lookSensitivity * Time.deltaTime * 10f;
                _pitch -= delta.y * lookSensitivity * Time.deltaTime * 10f;
                _pitch  = Mathf.Clamp(_pitch, -80f, 80f);
                cam.localRotation = Quaternion.Euler(_pitch, _yaw, 0f);
            }

            // WASD movement
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb == null) return;
            var move = Vector3.zero;
            if (kb.wKey.isPressed) move += cam.forward;
            if (kb.sKey.isPressed) move -= cam.forward;
            if (kb.aKey.isPressed) move -= cam.right;
            if (kb.dKey.isPressed) move += cam.right;
            cam.position += move * (moveSpeed * Time.deltaTime);
        }
#endif
    }
}

using System;
using Immersal.XR;
using UnityEngine;

namespace TileStories
{
    /// <summary>
    /// Wraps the Immersal Localizer for use with our IWallTracker interface.
    /// Assign the Localizer and XRSpace references in the Inspector.
    /// On device: fires OnWallLocalised when Immersal first locks.
    /// In Editor: use MockLocalizationProvider instead.
    /// </summary>
    public class ImmersalWallTracker : MonoBehaviour, IWallTracker
    {
        [Tooltip("The Immersal Localizer component in the scene.")]
        [SerializeField] private Localizer localizer;

        [Tooltip("The XRSpace root whose transform is updated by Immersal after lock.")]
        [SerializeField] private XRSpace xrSpace;

        [Tooltip("Consecutive localization failures before OnTrackingLost fires.")]
        [SerializeField] private int failureThreshold = 10;

        public bool IsLocalised { get; private set; }
        public Pose CurrentPose { get; private set; }

        public event Action<Pose> OnWallLocalised;
        public event Action OnTrackingLost;

        private int _consecutiveFailures;

        private void OnEnable()
        {
            if (localizer == null)
            {
                Debug.LogError("[ImmersalWallTracker] Localizer reference is not set.");
                return;
            }
            localizer.OnFirstSuccessfulLocalization.AddListener(HandleFirstLock);
            localizer.OnSuccessfulLocalizations.AddListener(HandleSuccess);
            localizer.OnFailedLocalizations.AddListener(HandleFailure);
        }

        private void OnDisable()
        {
            if (localizer == null) return;
            localizer.OnFirstSuccessfulLocalization.RemoveListener(HandleFirstLock);
            localizer.OnSuccessfulLocalizations.RemoveListener(HandleSuccess);
            localizer.OnFailedLocalizations.RemoveListener(HandleFailure);
        }

        private void HandleFirstLock()
        {
            IsLocalised = true;
            _consecutiveFailures = 0;
            var pose = GetXRSpacePose();
            CurrentPose = pose;
            Debug.Log("[ImmersalWallTracker] First lock achieved.");
            OnWallLocalised?.Invoke(pose);
        }

        private void HandleSuccess(int[] mapIds)
        {
            _consecutiveFailures = 0;
            if (!IsLocalised) return;
            CurrentPose = GetXRSpacePose();
        }

        private void HandleFailure()
        {
            _consecutiveFailures++;
            if (IsLocalised && _consecutiveFailures >= failureThreshold)
            {
                IsLocalised = false;
                Debug.LogWarning("[ImmersalWallTracker] Tracking lost.");
                OnTrackingLost?.Invoke();
            }
        }

        private Pose GetXRSpacePose()
        {
            if (xrSpace == null) return Pose.identity;
            var t = xrSpace.transform;
            return new Pose(t.position, t.rotation);
        }
    }
}

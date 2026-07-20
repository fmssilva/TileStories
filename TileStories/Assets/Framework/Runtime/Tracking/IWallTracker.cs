using System;
using UnityEngine;

namespace TileStories
{
    public interface IWallTracker
    {
        bool IsLocalised { get; }
        Pose CurrentPose { get; }

        event Action<Pose> OnWallLocalised;
        event Action OnTrackingLost;
    }
}

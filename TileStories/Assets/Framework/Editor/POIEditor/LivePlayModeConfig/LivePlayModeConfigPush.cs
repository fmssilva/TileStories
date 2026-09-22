using UnityEngine;

namespace TileStories.Editor
{
    // The one entry point the POI Editor window calls after a config change: while Play Mode runs,
    // send the window's config to the running wall. Registers every domain applier in one place.
    public static class LivePlayModeConfigPush
    {
        private static readonly LivePlayModeConfigDispatcher Dispatcher = new(new ILivePlayModeApplier[]
        {
            new LivePlayModeEffectsApplier(),
            new LivePlayModeOrientationApplier(),
            new LivePlayModeMarkerApplier(),
        });

        // Push the config to the running WallSession, if there is one (does nothing in Edit Mode)
        public static void PushToRunningWall(WallConfigData authoringConfig)
        {
            if (!Application.isPlaying || authoringConfig == null) return;

            var session = Object.FindFirstObjectByType<WallSession>();
            var applied = Dispatcher.Push(session, authoringConfig);
            if (applied.Count > 0)
                Debug.Log($"[LivePlayModeConfig] applied: {string.Join(", ", applied)}");
        }
    }
}

using UnityEngine;

namespace TileStories.Editor
{
    // Live Play Mode updates for the Zoom domain (_2.4 section 3.9): the running ARZoomController and the
    // on-screen buttons read the wall's zoom settings every frame (WallSession.ApplyZoomSettings).
    public class LivePlayModeZoomApplier : ILivePlayModeApplier
    {
        public string Name => "zoom";

        // Every zoom_settings field, nothing else
        public string Fingerprint(WallConfigData config) => JsonUtility.ToJson(config.zoom_settings, false);

        // Hand the running wall its own copy so it can never mutate the authoring config
        public void Apply(WallSession session, WallConfigData configCopy)
        {
            session.ApplyZoomSettings(configCopy.zoom_settings);
        }
    }
}

using UnityEngine;

namespace TileStories.Editor
{
    // Live Play Mode updates for the LOD domain (_2.4): distance bands, crowding response, clusters,
    // transitions and culling. The running LODController picks up the new settings object on its
    // next frame and re-evaluates from scratch (WallSession.ApplyLodSettings).
    public class LivePlayModeLodApplier : ILivePlayModeApplier
    {
        public string Name => "lod";

        // Every lod_settings field (bands included), nothing else
        public string Fingerprint(WallConfigData config) => JsonUtility.ToJson(config.lod_settings, false);

        // Hand the running wall its own copy so it can never mutate the authoring config
        public void Apply(WallSession session, WallConfigData configCopy)
        {
            session.ApplyLodSettings(configCopy.lod_settings);
        }
    }
}

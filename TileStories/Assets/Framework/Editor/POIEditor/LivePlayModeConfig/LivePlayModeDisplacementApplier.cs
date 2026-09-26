using UnityEngine;

namespace TileStories.Editor
{
    // Live Play Mode updates for the Displacement domain (_2.5): overlap distance, what moves, algorithm,
    // who gives way, max move and leader lines. The running LODController picks up the new settings
    // object on its next frame, puts every marker back and displaces with the new rules
    // (WallSession.ApplyDisplacementSettings).
    public class LivePlayModeDisplacementApplier : ILivePlayModeApplier
    {
        public string Name => "displacement";

        // Every displacement_settings field, nothing else
        public string Fingerprint(WallConfigData config) => JsonUtility.ToJson(config.displacement_settings, false);

        // Hand the running wall its own copy so it can never mutate the authoring config
        public void Apply(WallSession session, WallConfigData configCopy)
        {
            session.ApplyDisplacementSettings(configCopy.displacement_settings);
        }
    }
}

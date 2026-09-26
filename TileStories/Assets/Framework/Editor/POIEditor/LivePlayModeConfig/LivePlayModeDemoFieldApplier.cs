using UnityEngine;

namespace TileStories.Editor
{
    // Live Play Mode updates for the dev-only LOD demo field: any change (switch, counts, box, clump,
    // labels, Reshuffle) rebuilds the field in front of the current camera (WallSession.ApplyDemoField).
    public class LivePlayModeDemoFieldApplier : ILivePlayModeApplier
    {
        public string Name => "demo field";

        // Every demo_field field, nothing else
        public string Fingerprint(WallConfigData config) => JsonUtility.ToJson(config.demo_field, false);

        // Hand the running wall its own copy so it can never mutate the authoring config
        public void Apply(WallSession session, WallConfigData configCopy)
        {
            session.ApplyDemoField(configCopy.demo_field);
        }
    }
}

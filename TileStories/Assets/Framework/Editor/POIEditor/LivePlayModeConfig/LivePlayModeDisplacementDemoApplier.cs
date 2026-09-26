using UnityEngine;

namespace TileStories.Editor
{
    // Live Play Mode updates for the dev-only displacement demo: any change (switch, group size, spread,
    // distance, labels, reference copies, Run LOD) rebuilds the demo on its stage
    // (WallSession.ApplyDisplacementDemo).
    public class LivePlayModeDisplacementDemoApplier : ILivePlayModeApplier
    {
        public string Name => "displacement demo";

        // Every displacement_demo field, nothing else
        public string Fingerprint(WallConfigData config) => JsonUtility.ToJson(config.displacement_demo, false);

        // Hand the running wall its own copy so it can never mutate the authoring config
        public void Apply(WallSession session, WallConfigData configCopy)
        {
            session.ApplyDisplacementDemo(configCopy.displacement_demo);
        }
    }
}

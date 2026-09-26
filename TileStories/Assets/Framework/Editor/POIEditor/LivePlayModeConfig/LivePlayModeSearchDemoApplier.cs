using UnityEngine;

namespace TileStories.Editor
{
    // Live Play Mode updates for the dev-only search & filter demo: any change (switch, markers per category,
    // test cases, labels, distance, spacing, Run LOD) rebuilds the demo on its stage (WallSession.ApplySearchDemo).
    public class LivePlayModeSearchDemoApplier : ILivePlayModeApplier
    {
        public string Name => "search demo";

        // Every search_demo field, nothing else
        public string Fingerprint(WallConfigData config) => JsonUtility.ToJson(config.search_demo, false);

        // Hand the running wall its own copy so it can never mutate the authoring config
        public void Apply(WallSession session, WallConfigData configCopy) => session.ApplySearchDemo(configCopy.search_demo);
    }
}

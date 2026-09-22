using System.Collections.Generic;
using UnityEngine;

namespace TileStories.Editor
{
    // Live Play Mode updates for the Orientation domain (_2.1): the wall's orientation_settings
    // (vertical alignment, facing, update cost) and each hierarchy level's facing override.
    public class LivePlayModeOrientationApplier : ILivePlayModeApplier
    {
        public string Name => "orientation";

        // orientation_settings plus only the levels' facing override (their other columns belong to other domains)
        public string Fingerprint(WallConfigData config)
        {
            var overrides = new FacingOverrides();
            foreach (var level in config.hierarchy_levels)
                overrides.entries.Add(level.key + "=" + level.facing_mode_override);
            return JsonUtility.ToJson(config.orientation_settings, false) + "|" + JsonUtility.ToJson(overrides, false);
        }

        // Re-point every spawned marker, then the clusters currently on screen (LODController owns those)
        public void Apply(WallSession session, WallConfigData configCopy)
        {
            session.ApplyOrientationSettings(configCopy.orientation_settings, configCopy.hierarchy_levels);
            var lod = Object.FindFirstObjectByType<LODController>();
            if (lod != null) lod.ReapplyClusterOrientation();
        }

        [System.Serializable]
        private class FacingOverrides
        {
            public List<string> entries = new();
        }
    }
}

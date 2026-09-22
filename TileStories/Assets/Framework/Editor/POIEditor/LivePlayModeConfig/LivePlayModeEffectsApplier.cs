using UnityEngine;

namespace TileStories.Editor
{
    // Live Play Mode updates for the Effects domain (_2.2.4): the effect definitions, the master and
    // per-effect switches, the preview grid settings, and which effects each hierarchy level runs.
    public class LivePlayModeEffectsApplier : ILivePlayModeApplier
    {
        public string Name => "effects";

        // effect_defaults plus only the levels' effect columns (their size, reveal or facing edits belong elsewhere)
        public string Fingerprint(WallConfigData config)
        {
            var columns = new LevelEffectColumns();
            foreach (var level in config.hierarchy_levels)
                columns.entries.Add(level.key + "=" + level.ripple_effect + "/" + level.halo_effect + "/" + level.pulse);
            return JsonUtility.ToJson(config.effect_defaults, false) + "|" + JsonUtility.ToJson(columns, false);
        }

        // Hand the running wall its own copies so it can never mutate the authoring config
        public void Apply(WallSession session, WallConfigData configCopy)
        {
            session.ApplyEffectSettings(configCopy.effect_defaults, configCopy.hierarchy_levels);
        }

        [System.Serializable]
        private class LevelEffectColumns
        {
            public System.Collections.Generic.List<string> entries = new();
        }
    }
}

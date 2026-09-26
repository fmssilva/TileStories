using System.Collections.Generic;

namespace TileStories.Editor
{
    // Pure rules of the LOD section of the POI Editor: which fields matter for the chosen Response
    // (so the window only shows rows that do something) and what is wrong with a Distance Bands table.
    // No GUI, no config writes: the window draws, this decides. Mirrors LODController's own rules
    // (ComputeTargetDensityState / ShouldAggregate / SafetyNet), and LodEditorRulesTests pins that.
    public static class LodEditorRules
    {
        // The Shrink Starts At / Shrink Floor rows: only modes that run the shrink ramp
        public static bool UsesShrink(string mode) => mode == "shrink_and_fade" || mode == "hybrid";

        // The Crowded At row: every mode that reacts to a crowded marker
        public static bool UsesCrowdedCount(string mode) => mode == "select_hide" || mode == "cluster" || mode == "shrink_and_fade" || mode == "hybrid";

        // The Safety Net rows: only modes that would otherwise NEVER cluster (Cluster and Hybrid already
        // merge every crowded marker, and "none" is a total opt-out)
        public static bool UsesSafetyNet(string mode) => mode == "select_hide" || mode == "shrink_and_fade";

        // The Clusters sub-foldout: clusters are built by cluster/hybrid, or by the safety net
        public static bool CanBuildClusters(LodSettings s) =>
            s != null && (s.density_response_mode == "cluster" || s.density_response_mode == "hybrid"
                          || (s.density_safety_escalation_enabled && UsesSafetyNet(s.density_response_mode)));

        // What is wrong with a band table, in the words of the Editor Tab (empty = fine)
        public static List<string> BandProblems(List<LodBandEntry> bands)
        {
            var problems = new List<string>();
            if (bands == null || bands.Count == 0)
            {
                problems.Add("No distance bands: every marker is shown at every distance. Click Suggest Values or + Add band.");
                return problems;
            }
            for (int i = 0; i < bands.Count; i++)
            {
                if (bands[i].max_distance_m <= 0f)
                    problems.Add("Band " + (i + 1) + ": 'Up to (m)' must be above 0.");
                if (bands[i].max_visible_count < -1 || bands[i].max_visible_count == 0)
                    problems.Add("Band " + (i + 1) + ": 'Max markers' must be -1 (all) or at least 1.");
                if (i > 0 && bands[i].max_distance_m <= bands[i - 1].max_distance_m)
                    problems.Add("Band " + (i + 1) + ": 'Up to (m)' must be larger than the band above it (rows go from near to far).");
            }
            return problems;
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace TileStories
{
    // What the card's status block shows for a POI's condition (_3.1 step 5b): the colour and line style of its
    // marker's outline ring, resolved by the MARKERS' OWN RULE (MarkerVisualResolver: outline mode uniform / per type /
    // same hue, the Outline Types row the POI picked, the unknown row) -- never a second copy of it -- so the card and
    // the marker can never disagree. Only when the wall draws no ring for it (no Outline Types rows, outline mode
    // "none") does the card fall back to its own status tokens (--ts-status-0..4, CardTokens.uss). Pure; the static
    // palettes it reads are the ones WallSession configured for the running markers.
    public static class CardStatusRule
    {
        // The token step used for an unknown condition (--ts-status-4, the neutral grey)
        public const int UnknownTokenStep = 4;

        public readonly struct Status
        {
            // The POI has a condition to show at all (has_status)
            public readonly bool Shows;
            public readonly bool Unknown;
            // True: Color is the wall's own ring colour. False: use the token class of TokenStep.
            public readonly bool FromWall;
            public readonly Color Color;
            // The ring's line style key ("solid", "dash_long", "dash_medium", "dash_short", "dotted")
            public readonly string LineStyle;
            public readonly int TokenStep;
            // The Outline Types row's name ("Partial Damage"), or "" when the wall has no such row (never a key)
            public readonly string LevelName;
            public readonly float Pct;

            public Status(bool shows, bool unknown, bool fromWall, Color color, string lineStyle, int tokenStep, string levelName, float pct)
            {
                Shows = shows;
                Unknown = unknown;
                FromWall = fromWall;
                Color = color;
                LineStyle = lineStyle;
                TokenStep = tokenStep;
                LevelName = levelName;
                Pct = pct;
            }
        }

        // The condition the card shows for `poi`
        public static Status Resolve(POIData poi, MarkerVisualSettings look, WallConfigData taxonomy)
        {
            if (poi == null || !poi.has_status) return default;
            bool unknown = poi.status_unknown;
            string levelKey = !string.IsNullOrWhiteSpace(poi.status_level_key) ? poi.status_level_key
                : unknown ? MarkerVisualResolver.FallbackUnknownStatusLevelKey : null;
            string name = RowName(taxonomy, levelKey);

            var marker = MarkerVisualResolver.Resolve(poi, look ?? MarkerVisualSettings.Default());
            if (marker.ShowRing)
            {
                var color = marker.RingUsesCategoryHue ? marker.RingHueColor : marker.RingLevel.RingColor;
                return new Status(true, unknown, true, color, marker.RingLevel.RingSpriteKey, TokenStepOf(poi), name, marker.RingLevel.Pct);
            }
            // - the wall draws no ring for this POI: the card's own ramp, solid for a known condition, dotted for unknown
            return new Status(true, unknown, false, default, unknown ? "dotted" : "solid", TokenStepOf(poi), name, poi.status_pct);
        }

        // Every known Outline Types row of the wall, in order of damage, as the card's scale shows them: each row resolved
        // by the markers' rule as if this POI stood at that row (so same-hue shades this POI's own category)
        public static List<(string Key, Status Step)> Scale(POIData poi, MarkerVisualSettings look, WallConfigData taxonomy)
        {
            var steps = new List<(string, Status)>();
            var rows = taxonomy?.outline_levels;
            if (poi == null || rows == null) return steps;
            var known = new List<OutlineLevelEntry>();
            foreach (var row in rows)
                if (row != null && !string.IsNullOrWhiteSpace(row.key) && row.key != MarkerVisualResolver.FallbackUnknownStatusLevelKey) known.Add(row);
            known.Sort((a, b) => a.pct.CompareTo(b.pct));
            foreach (var row in known)
            {
                var probe = new POIData
                {
                    id = poi.id, category = poi.category, has_status = true, status_unknown = false,
                    status_level_key = row.key, status_pct = row.pct,
                };
                steps.Add((row.key, Resolve(probe, look, taxonomy)));
            }
            return steps;
        }

        // --ts-status-0..3 from the damage percentage (0 intact .. 3 destroyed), 4 for unknown
        public static int TokenStepOf(POIData poi) =>
            poi.status_unknown ? UnknownTokenStep : Mathf.Clamp(Mathf.RoundToInt(poi.status_pct / 100f * 3f), 0, 3);

        private static string RowName(WallConfigData taxonomy, string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return "";
            var row = taxonomy?.outline_levels?.Find(e => e != null && e.key == key);
            return row != null && !string.IsNullOrWhiteSpace(row.label) ? row.label : "";
        }
    }
}

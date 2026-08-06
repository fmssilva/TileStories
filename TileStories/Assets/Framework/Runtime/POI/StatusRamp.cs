using System.Collections.Generic;
using UnityEngine;

namespace TileStories
{
    public readonly struct StatusLevel
    {
        public readonly float Pct;
        public readonly Color RingColor;
        public readonly string RingSpriteKey; // solid/dash_long/dash_medium/dash_short/dotted
        public readonly float RingWidth;

        public StatusLevel(float pct, Color ringColor, string ringSpriteKey, float ringWidth)
        {
            Pct = pct;
            RingColor = ringColor;
            RingSpriteKey = ringSpriteKey;
            RingWidth = ringWidth;
        }
    }

    // The single condition palette used everywhere a marker shows destruction
    // status: OutlineGold's ring, OutlineSameHue's ring+fill, and Badge's badge
    // fill all read from this same table -- the three styles never disagree about
    // colour. Every level, including 100%, is fully opaque (see _2_2_Marker_Design.md
    // §4 principle 3 for why).
    public static class StatusRamp
    {
        private const string UnknownStatusLevelKey = "unknown";

        // Deliberately a cool, neutral grey -- outside the warm gold/rust family used
        // for known status levels, so "unknown" reads as a genuinely different kind of
        // signal ("we don't know") rather than an extra point on the destruction scale.
        public static readonly Color UnknownColor = new Color(0.55f, 0.56f, 0.58f);

        // Default representation for semantic unknown when no config-provided
        // outline level is available. Keeps unknown distinct from known destruction.
        public static readonly StatusLevel UnknownFallbackLevel =
            new StatusLevel(100f, UnknownColor, "dotted", 1.8f);

        public static readonly StatusLevel[] Levels =
        {
            new StatusLevel(0,   new Color(0.890f, 0.741f, 0.447f), "solid",       3.2f),
            new StatusLevel(20,  new Color(0.812f, 0.624f, 0.369f), "dash_long",   2.8f),
            new StatusLevel(40,  new Color(0.733f, 0.522f, 0.322f), "dash_medium", 2.4f),
            new StatusLevel(60,  new Color(0.651f, 0.416f, 0.271f), "dash_short",  2.0f),
            new StatusLevel(80,  new Color(0.549f, 0.302f, 0.235f), "dotted",      1.7f),
            new StatusLevel(100, new Color(0.431f, 0.200f, 0.169f), "dotted",      1.8f),
        };

        private static StatusLevel[] _activeLevels = Levels;
        private static Dictionary<string, StatusLevel> _activeLevelsByKey = BuildDefaultLevelKeyLookup();

        public static IReadOnlyList<StatusLevel> ActiveLevels => _activeLevels;

        public static void Configure(IEnumerable<OutlineLevelEntry> entries)
        {
            if (entries == null)
            {
                _activeLevels = Levels;
                return;
            }

            var configured = new List<StatusLevel>();
            var configuredByKey = new Dictionary<string, StatusLevel>();
            int index = 0;
            foreach (var entry in entries)
            {
                if (entry == null)
                    continue;

                var fallback = Levels[Mathf.Clamp(index, 0, Levels.Length - 1)];
                var lineStyle = MarkerVisualsParser.NormalizeLineStyle(entry.line_style);
                var color = fallback.RingColor;
                if (!string.IsNullOrWhiteSpace(entry.color_hex) &&
                    ColorUtility.TryParseHtmlString(entry.color_hex, out var parsedColor))
                {
                    color = parsedColor;
                }

                float ringWidth = entry.ring_width > 0f ? entry.ring_width : fallback.RingWidth;
                var level = new StatusLevel(entry.pct, color, lineStyle, ringWidth);
                configured.Add(level);

                if (!string.IsNullOrWhiteSpace(entry.key))
                {
                    configuredByKey[entry.key.Trim()] = level;
                }

                index++;
            }

            _activeLevels = configured.Count > 0 ? configured.ToArray() : Levels;
            _activeLevelsByKey = configuredByKey.Count > 0 ? configuredByKey : BuildDefaultLevelKeyLookup();
        }

        public static void ResetToDefaults()
        {
            _activeLevels = Levels;
            _activeLevelsByKey = BuildDefaultLevelKeyLookup();
        }

        public static bool TryResolveByKey(string key, out StatusLevel level)
        {
            level = default;
            if (string.IsNullOrWhiteSpace(key) || _activeLevelsByKey == null)
                return false;

            return _activeLevelsByKey.TryGetValue(key.Trim(), out level);
        }

        // Snaps an arbitrary 0-100 destruction percentage to the nearest authored
        // level. Callers MUST check POIData.has_status before calling this at all.
        public static StatusLevel Resolve(float pct)
        {
            var levels = _activeLevels;
            var closest = levels[0];
            float bestDist = float.MaxValue;
            foreach (var level in levels)
            {
                float dist = Mathf.Abs(level.Pct - pct);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    closest = level;
                }
            }
            return closest;
        }

        // Used by MarkerStyle.OutlineSameHue: drains saturation/value toward a
        // near-black neutral as status worsens, never reaching pure black.
        public static Color ShadeTowardBlack(Color baseColor, float pct)
        {
            Color.RGBToHSV(baseColor, out float h, out float s, out float v);
            float t = Mathf.Clamp01(pct / 100f);
            float s2 = s * (1f - 0.72f * t);
            float v2 = Mathf.Lerp(v, 0.09f, t * 0.92f);
            return Color.HSVToRGB(h, Mathf.Max(s2, 0.04f), Mathf.Max(v2, 0.07f));
        }

        private static Dictionary<string, StatusLevel> BuildDefaultLevelKeyLookup()
        {
            // The stock ramp is percentage-based and does not encode semantic keys.
            // We still expose well-known keys for safety fallbacks.
            return new Dictionary<string, StatusLevel>
            {
                { "intact", Levels[0] },
                { "partial_damage", Levels[1] },
                { "destroyed", Levels[Levels.Length - 1] },
                { UnknownStatusLevelKey, UnknownFallbackLevel },
            };
        }
    }
}
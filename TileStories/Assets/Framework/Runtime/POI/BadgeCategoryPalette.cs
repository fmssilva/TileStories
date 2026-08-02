using System.Collections.Generic;
using UnityEngine;

namespace TileStories
{
    // Resolves optional badge taxonomy keys into badge icon/tint settings.
    public static class BadgeCategoryPalette
    {
        public readonly struct BadgeDefinition
        {
            public readonly Color Color;
            public readonly string IconKey;

            public BadgeDefinition(Color color, string iconKey)
            {
                Color = color;
                IconKey = iconKey;
            }
        }

        private static Dictionary<string, BadgeDefinition> _definitions;

        public static void Configure(IEnumerable<BadgeCategoryEntry> entries)
        {
            _definitions = new Dictionary<string, BadgeDefinition>();
            if (entries == null)
                return;

            foreach (var entry in entries)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.key))
                    continue;

                Color color = new Color(0.70f, 0.70f, 0.70f, 1f);
                if (!string.IsNullOrWhiteSpace(entry.color_hex) &&
                    ColorUtility.TryParseHtmlString(entry.color_hex, out var parsed))
                {
                    color = parsed;
                }

                _definitions[entry.key] = new BadgeDefinition(color, entry.icon_key);
            }
        }

        public static void Clear()
        {
            _definitions = null;
        }

        public static bool TryResolve(string key, out BadgeDefinition definition)
        {
            definition = default;
            if (string.IsNullOrWhiteSpace(key) || _definitions == null)
                return false;

            return _definitions.TryGetValue(key, out definition);
        }
    }
}

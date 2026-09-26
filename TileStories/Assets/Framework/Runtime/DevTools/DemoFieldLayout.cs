using System.Collections.Generic;
using UnityEngine;

namespace TileStories
{
    // Pure placement of the dev-only LOD demo field (DemoFieldSettings): which generated markers
    // exist, at which hierarchy level and category, and where. No scene, no MonoBehaviour: the
    // spawner (DemoFieldSpawner) and the tests read the same list, so they cannot drift apart.
    //
    // Local frame of the field: +x right, +y up, +z away from the camera. Markers of each hierarchy
    // level are scattered through a box that starts distance_m in front of the camera and reaches
    // depth_m further (so one field spans several distance bands), plus one dense clump a quarter of
    // the way in, packed inside dense_clump_radius_m (so crowding / clusters always happen).
    // A given seed always gives the same field.
    public static class DemoFieldLayout
    {
        // One generated marker of the field
        public readonly struct Entry
        {
            public readonly string Id;
            public readonly string Name;
            public readonly string LevelKey;
            public readonly string Category;
            public readonly Vector3 LocalPosition;
            public readonly bool InClump;

            public Entry(string id, string name, string levelKey, string category, Vector3 localPosition, bool inClump)
            {
                Id = id;
                Name = name;
                LevelKey = levelKey;
                Category = category;
                LocalPosition = localPosition;
                InClump = inClump;
            }
        }

        // How many demo markers a level asks for (0 when the level has no row), clamped to the limit
        public static int CountFor(DemoFieldSettings settings, string levelKey)
        {
            if (settings?.level_counts == null) return 0;
            foreach (var row in settings.level_counts)
                if (row != null && row.level_key == levelKey)
                    return Mathf.Clamp(row.count, 0, DemoFieldSettings.MaxCountPerLevel);
            return 0;
        }

        // Build the whole field: every level's scattered markers (in level order), then the clump
        public static List<Entry> Build(DemoFieldSettings settings, IReadOnlyList<HierarchyLevelEntry> levels,
            IReadOnlyList<string> categories)
        {
            var entries = new List<Entry>();
            if (settings == null) return entries;

            var rng = new System.Random(settings.seed);
            float width = Mathf.Max(0f, settings.width_m);
            float height = Mathf.Max(0f, settings.height_m);
            float depth = Mathf.Max(0f, settings.depth_m);
            float near = Mathf.Max(0f, settings.distance_m);
            int categoryCursor = 0;

            string NextCategory() => categories == null || categories.Count == 0
                ? ""
                : categories[categoryCursor++ % categories.Count];
            float Range(float min, float max) => min + (float)rng.NextDouble() * (max - min);

            if (levels != null)
            {
                foreach (var level in levels)
                {
                    if (level == null) continue;
                    int count = CountFor(settings, level.key);
                    string label = string.IsNullOrWhiteSpace(level.level_name) ? level.key : level.level_name;
                    for (int i = 0; i < count; i++)
                    {
                        var pos = new Vector3(Range(-width * 0.5f, width * 0.5f), Range(-height * 0.5f, height * 0.5f), near + Range(0f, depth));
                        entries.Add(new Entry($"demo_{level.key}_{i + 1}", $"{label} {i + 1}", level.key, NextCategory(), pos, false));
                    }
                }
            }

            int clump = Mathf.Clamp(settings.dense_clump_count, 0, DemoFieldSettings.MaxClumpCount);
            float radius = Mathf.Max(0f, settings.dense_clump_radius_m);
            var clumpCentre = new Vector3(0f, 0f, near + depth * 0.25f);
            for (int i = 0; i < clump; i++)
            {
                // - even spread inside a disc facing the camera (sqrt keeps the centre from bunching)
                float angle = Range(0f, Mathf.PI * 2f);
                float r = radius * Mathf.Sqrt((float)rng.NextDouble());
                var pos = clumpCentre + new Vector3(Mathf.Cos(angle) * r, Mathf.Sin(angle) * r, Range(-radius, radius) * 0.25f);
                string levelKey = levels != null && levels.Count > 0 ? levels[i % levels.Count]?.key ?? "" : "";
                entries.Add(new Entry($"demo_clump_{i + 1}", $"Clump {i + 1}", levelKey, NextCategory(), pos, true));
            }
            return entries;
        }
    }
}

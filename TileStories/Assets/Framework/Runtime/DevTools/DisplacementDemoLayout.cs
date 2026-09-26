using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TileStories
{
    // Pure placement of the dev-only displacement demo (DisplacementDemoSettings): which generated markers
    // exist, at which hierarchy level and category, and where on the flat demo wall. No scene, no
    // MonoBehaviour: the spawner (DisplacementDemoSpawner) and the tests read the same list.
    //
    // Local frame: +x right, +y up, +z away from the camera; the demo wall stands at z = distance_m. Four
    // scenario cells in a 2 x 2 grid, spaced in proportion to the distance so the whole demo stays in view
    // while the markers keep their real size (farther = smaller on screen = more crowding):
    //   top-left  "Same"  -- every member on the same (middle) level: only the algorithm decides
    //   top-right "Mixed" -- members on different levels: shows who gives way (tiebreak)
    //   bottom-left "Big/Small" -- one top-level marker among the smallest level
    //   bottom-right "Lone" -- one marker with no neighbour: must never move (control)
    // Each live marker can have a faded reference copy at its true place (never displaced): beside the
    // group (side_by_side), on top of it (overlay) or none (off).
    public static class DisplacementDemoLayout
    {
        public const string SameLevel = "same_level";
        public const string MixedLevels = "mixed_levels";
        public const string BigAndSmall = "big_and_small";
        public const string Lone = "lone";

        public static readonly string[] ReferenceModes = { "side_by_side", "overlay", "off" };

        // Opacity of a reference copy, so the live marker reads as the real one
        public const float ReferenceAlpha = 0.35f;

        // Cell and half-cell spacing as a fraction of the distance (keeps the demo framed at any distance)
        public const float CellHalfWidthPerMetre = 0.4f;
        public const float CellHalfHeightPerMetre = 0.25f;
        public const float ReferenceShiftPerMetre = 0.15f;

        // One generated marker of the demo
        public readonly struct Entry
        {
            public readonly string Id;
            public readonly string Name;
            public readonly string LevelKey;
            public readonly string Category;
            public readonly Vector3 LocalPosition;
            public readonly string Scenario;
            public readonly bool IsReference;

            public Entry(string id, string name, string levelKey, string category, Vector3 localPosition, string scenario, bool isReference)
            {
                Id = id;
                Name = name;
                LevelKey = levelKey;
                Category = category;
                LocalPosition = localPosition;
                Scenario = scenario;
                IsReference = isReference;
            }
        }

        public static int MembersPerGroup(DisplacementDemoSettings s) =>
            Mathf.Clamp(s.markers_per_group, DisplacementDemoSettings.MinMarkersPerGroup, DisplacementDemoSettings.MaxMarkersPerGroup);

        public static float SpreadMetres(DisplacementDemoSettings s) =>
            Mathf.Clamp(s.spread_cm, 0f, DisplacementDemoSettings.MaxSpreadCm) / 100f;

        public static float DistanceMetres(DisplacementDemoSettings s) =>
            Mathf.Clamp(s.distance_m, DisplacementDemoSettings.MinDistanceM, DisplacementDemoSettings.MaxDistanceM);

        // Unknown or blank = side_by_side, the clearest view
        public static string ReferenceMode(DisplacementDemoSettings s) =>
            System.Array.IndexOf(ReferenceModes, s.reference_copies) >= 0 ? s.reference_copies : "side_by_side";

        // Golden angle (radians): consecutive spiral points never line up, so no two groups look alike
        public const float GoldenAngle = 2.39996f;

        // Where a group member sits around its cell centre: a golden-angle spiral reaching `spread` at the
        // last member -- irregular like a real wall (a perfectly symmetric ring cancels every push in the
        // Force Directed algorithm and hides its behaviour). spread 0 = all exactly on top of each other.
        public static Vector2 MemberOffset(int index, int count, float spread)
        {
            if (index == 0 || count < 2) return Vector2.zero;
            float radius = spread * Mathf.Sqrt((float)index / (count - 1));
            float angle = index * GoldenAngle;
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        }

        // Build the whole demo: the four scenarios' live markers, each followed by its reference copy
        public static List<Entry> Build(DisplacementDemoSettings settings, IReadOnlyList<HierarchyLevelEntry> levels,
            IReadOnlyList<string> categories)
        {
            var entries = new List<Entry>();
            if (settings == null) return entries;

            // - most important first (lower priority number); same priority keeps the table order
            var ordered = (levels ?? new List<HierarchyLevelEntry>()).Where(l => l != null)
                .Select((l, i) => (l, i)).OrderBy(p => p.l.priority).ThenBy(p => p.i).Select(p => p.l.key).ToList();
            string top = ordered.Count > 0 ? ordered[0] : "";
            string bottom = ordered.Count > 0 ? ordered[ordered.Count - 1] : "";
            string middle = ordered.Count > 0 ? ordered[ordered.Count / 2] : "";

            int n = MembersPerGroup(settings);
            float spread = SpreadMetres(settings);
            float d = DistanceMetres(settings);
            string mode = ReferenceMode(settings);
            float cx = CellHalfWidthPerMetre * d, cy = CellHalfHeightPerMetre * d;
            // - side by side: the two halves move apart by the spread too, so a wide group never runs into its copy
            float liveShift = mode == "side_by_side" ? ReferenceShiftPerMetre * d + spread : 0f;
            int categoryCursor = 0;
            string NextCategory() => categories == null || categories.Count == 0 ? "" : categories[categoryCursor++ % categories.Count];

            void AddGroup(string scenario, string shortName, Vector2 cell, int count, System.Func<int, string> levelOf, System.Func<int, string> nameOf)
            {
                for (int k = 0; k < count; k++)
                {
                    Vector2 member = cell + MemberOffset(k, count, spread);
                    string level = levelOf(k);
                    string category = NextCategory();
                    string id = $"ddemo_{shortName}_{k + 1}";
                    entries.Add(new Entry(id, nameOf(k), level, category, new Vector3(member.x + liveShift, member.y, d), scenario, false));
                    if (mode != "off")
                        entries.Add(new Entry(id + "_ref", nameOf(k), level, category,
                            new Vector3(member.x - liveShift, member.y, d), scenario, true));
                }
            }

            AddGroup(SameLevel, "same", new Vector2(-cx, cy), n, _ => middle, k => $"Same {k + 1}");
            AddGroup(MixedLevels, "mixed", new Vector2(cx, cy), n, k => ordered.Count > 0 ? ordered[k % ordered.Count] : "", k => $"Mixed {k + 1}");
            AddGroup(BigAndSmall, "bigsmall", new Vector2(-cx, -cy), n, k => k == 0 ? top : bottom, k => k == 0 ? "Big" : $"Small {k}");
            AddGroup(Lone, "lone", new Vector2(cx, -cy), 1, _ => middle, _ => "Lone");
            return entries;
        }
    }
}

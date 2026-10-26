using System.Collections.Generic;
using UnityEngine;

namespace TileStories
{
    // Pure placement of the minimap's dots (spec _2.6 section 8): which two world axes the map shows
    // (projection) and which world rectangle fills it (bounds), then POI position -> 0..1 map position.
    //   projection "wall"  = x right, y up    (a flat wall seen from the front)
    //   projection "floor" = x right, z up    (a room seen from above, far = top)
    //   projection "auto"  = the two axes along which the POIs spread most (ties prefer "wall")
    //   bounds "auto" = every POI plus a margin; "manual" = bounds_min / bounds_max (world metres)
    public readonly struct MinimapLayout
    {
        // Share of the fitted size added on each side in auto bounds, so no dot sits on the border
        public const float AutoMargin = 0.1f;
        // Smallest map extent (metres): a single POI or a line of POIs still gets a real rectangle
        public const float MinExtentM = 0.5f;

        public readonly bool Floor;
        public readonly Vector2 Min;
        public readonly Vector2 Max;

        private MinimapLayout(bool floor, Vector2 min, Vector2 max)
        {
            Floor = floor;
            Min = min;
            Max = max;
        }

        // The layout for these POI positions under these settings
        public static MinimapLayout Compute(IReadOnlyList<Vector3> positions, MinimapSettings settings)
        {
            bool floor = ResolveFloor(positions, settings?.projection);
            Vector2 min, max;
            if (settings != null && settings.bounds_mode == SelectFilterSearchOptions.BoundsManual)
            {
                min = Project(settings.bounds_min, floor);
                max = Project(settings.bounds_max, floor);
                if (min.x > max.x) (min.x, max.x) = (max.x, min.x);
                if (min.y > max.y) (min.y, max.y) = (max.y, min.y);
            }
            else
            {
                FitAuto(positions, floor, out min, out max);
            }
            GrowToMinExtent(ref min, ref max);
            return new MinimapLayout(floor, min, max);
        }

        // Whether the map shows the floor plane (x/z) rather than the wall plane (x/y)
        public static bool ResolveFloor(IReadOnlyList<Vector3> positions, string projection)
        {
            if (projection == SelectFilterSearchOptions.ProjectionWall) return false;
            if (projection == SelectFilterSearchOptions.ProjectionFloor) return true;
            if (positions == null || positions.Count < 2) return false;

            Vector3 lo = positions[0], hi = positions[0];
            foreach (var p in positions) { lo = Vector3.Min(lo, p); hi = Vector3.Max(hi, p); }
            Vector3 spread = hi - lo;
            // - the axis the POIs spread least along is the one the map drops
            return spread.y < spread.z;
        }

        // A POI's position on the map, (0,0) bottom-left to (1,1) top-right, clamped to the map
        public Vector2 Normalize(Vector3 world)
        {
            Vector2 p = Project(world, Floor);
            return new Vector2(
                Mathf.Clamp01(Mathf.InverseLerp(Min.x, Max.x, p.x)),
                Mathf.Clamp01(Mathf.InverseLerp(Min.y, Max.y, p.y)));
        }

        private static Vector2 Project(Vector3 world, bool floor) => floor ? new Vector2(world.x, world.z) : new Vector2(world.x, world.y);

        private static void FitAuto(IReadOnlyList<Vector3> positions, bool floor, out Vector2 min, out Vector2 max)
        {
            if (positions == null || positions.Count == 0)
            {
                min = Vector2.zero;
                max = Vector2.zero;
                return;
            }
            min = max = Project(positions[0], floor);
            foreach (var p in positions)
            {
                Vector2 q = Project(p, floor);
                min = Vector2.Min(min, q);
                max = Vector2.Max(max, q);
            }
            Vector2 margin = (max - min) * AutoMargin;
            min -= margin;
            max += margin;
        }

        private static void GrowToMinExtent(ref Vector2 min, ref Vector2 max)
        {
            for (int axis = 0; axis < 2; axis++)
            {
                float size = max[axis] - min[axis];
                if (size >= MinExtentM) continue;
                float centre = (max[axis] + min[axis]) * 0.5f;
                min[axis] = centre - MinExtentM * 0.5f;
                max[axis] = centre + MinExtentM * 0.5f;
            }
        }
    }
}

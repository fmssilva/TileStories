using System.Collections.Generic;
using UnityEngine;

namespace TileStories
{
    // The wall's main axis: a line on the floor plane through the POIs, pointing "right"
    public readonly struct WallAxis
    {
        // A point on the axis (the POIs' centre) and the unit direction it runs in (horizontal: y = 0)
        public readonly Vector3 Origin;
        public readonly Vector3 Direction;

        public WallAxis(Vector3 origin, Vector3 direction)
        {
            Origin = origin;
            Direction = direction;
        }

        // How far along the wall a position is (metres, negative = left of the centre)
        public float Along(Vector3 position) => Vector3.Dot(position - Origin, Direction);
    }

    // The wall's positioned POIs laid along its main axis (WallAxisRule.Places)
    public sealed class WallPlaces
    {
        public readonly List<POIData> Pois = new();
        public readonly List<string> Ids = new();
        // Each POI's place along the wall (metres from the POIs' centre, left negative), in the order of Pois
        public readonly List<float> Along = new();
        public WallAxis Axis;

        // The index of this POI id, or -1 (not on the wall, or no position)
        public int IndexOf(string id) => id == null ? -1 : Ids.IndexOf(id);

        // The nearest positioned POI on each side of `id` along the wall (null where there is none)
        public (POIData Left, POIData Right) NeighboursOf(string id)
        {
            var (left, right) = WallAxisRule.Neighbours(Along, Ids, IndexOf(id));
            return (left >= 0 ? Pois[left] : null, right >= 0 ? Pois[right] : null);
        }
    }

    // "Along the wall" (_3.1 Tier 2 group B), pure and defined ONCE: wall_locator's strip and neighbours and related's
    // next_along_wall all read it. A wall's POIs spread along it more than across it, so the wall's main axis is the
    // direction the POIs spread most on the floor plane (x / z: the principal axis of their positions, a 2x2 fit), and a
    // POI's place along the wall is its projection on that axis. Height (y) never counts: a POI high on the panel and one
    // low under it are at the same place along the wall.
    // The direction points "right" by the minimap's convention (its wall projection draws world x to the right): its x is
    // positive, and when it runs exactly along z, its z is. (Not MinimapLayout itself: that keeps world x as its horizontal
    // axis, so a wall standing at an angle to world x would fold onto itself; this finds the wall's own axis.)
    public static class WallAxisRule
    {
        // Below this spread (metres^2) the POIs are one point: the axis falls back to world x
        private const float FlatSpread = 1e-6f;

        // The main axis of these positions (0 or 1 position, or all in one spot: world x through them)
        public static WallAxis Of(IReadOnlyList<Vector3> positions)
        {
            int n = positions?.Count ?? 0;
            if (n == 0) return new WallAxis(Vector3.zero, Vector3.right);
            Vector3 centre = Vector3.zero;
            foreach (var p in positions) centre += p;
            centre /= n;
            float xx = 0f, zz = 0f, xz = 0f;
            foreach (var p in positions)
            {
                float dx = p.x - centre.x, dz = p.z - centre.z;
                xx += dx * dx;
                zz += dz * dz;
                xz += dx * dz;
            }
            if (xx + zz < FlatSpread) return new WallAxis(centre, Vector3.right);
            // - the principal direction of a 2x2 covariance: the angle that diagonalises it
            float angle = 0.5f * Mathf.Atan2(2f * xz, xx - zz);
            var direction = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
            if (direction.x < -1e-6f || (Mathf.Abs(direction.x) <= 1e-6f && direction.z < 0f)) direction = -direction;
            return new WallAxis(centre, direction.normalized);
        }

        // The order of `ids` along the wall, left to right: by place along the axis, ties by id (a total, stable order)
        public static List<int> Order(IReadOnlyList<float> along, IReadOnlyList<string> ids)
        {
            var order = new List<int>(along.Count);
            for (int i = 0; i < along.Count; i++) order.Add(i);
            order.Sort((a, b) =>
            {
                int byPlace = along[a].CompareTo(along[b]);
                return byPlace != 0 ? byPlace : string.CompareOrdinal(ids[a], ids[b]);
            });
            return order;
        }

        // The wall's POIs at their positions (POIPositionResolver: no position = the wall's origin; an invalid one -- NaN,
        // infinite -- is left out), the axis through them and each one's place along it
        public static WallPlaces Places(IReadOnlyList<POIData> pois)
        {
            var places = new WallPlaces();
            var positions = new List<Vector3>();
            if (pois != null)
                foreach (var poi in pois)
                {
                    if (poi == null || !POIPositionResolver.TryResolvePosition(poi, out var position, logErrors: false)) continue;
                    places.Pois.Add(poi);
                    places.Ids.Add(poi.id);
                    positions.Add(position);
                }
            places.Axis = Of(positions);
            foreach (var position in positions) places.Along.Add(places.Axis.Along(position));
            return places;
        }

        // The nearest POI on each side of `self` along the wall (indexes into the lists, -1 where there is none)
        public static (int Left, int Right) Neighbours(IReadOnlyList<float> along, IReadOnlyList<string> ids, int self)
        {
            var order = Order(along, ids);
            int at = order.IndexOf(self);
            if (at < 0) return (-1, -1);
            return (at > 0 ? order[at - 1] : -1, at < order.Count - 1 ? order[at + 1] : -1);
        }
    }
}

using UnityEngine;

namespace TileStories
{
    // The geometry of fab_radial (_3.2 section 5.4, canvas C2): a pie / marking menu (Callahan et al. 1988; Kurtenbach & Buxton 1994) on an
    // ARC around its button. Pure, in panel units (x right, y DOWN) and degrees (0 = right, 90 = up, 180 = left, 270 = down).
    //   - the button sits at the screen's right edge, so the arc lies in the LEFT half-plane, from straight up (90) through left (180) to
    //     straight down (270): the first item on top, in reading order
    //   - an item is a RECTANGLE (its round button and its word under it: wider than tall); two items are apart when they are a width apart
    //     across OR a height apart down -- so neither their buttons nor their words ever overlap, without a circle's waste
    //   - Layout finds the smallest radius at or above the minimum whose arc keeps EVERY item inside the layer (on the right too: an item
    //     straight above or below the button reaches half a width right of its centre), the items apart, at most MaxStepDegrees apart (a
    //     sector wider than a marking menu's eighth helps nobody), the arc centred on "left" as far as the room allows
    //   - Capacity: the most items (2 at least, `wanted` at most) an arc holds in the room -- at Full the button rests in the screen's top
    //     corner and only a quarter arc is left, so fewer fit there than at peek; the element pages through the rest behind More
    //   - ItemAt reads a press-slide-release: the direction of the finger's travel picks the item whose sector holds it; a release inside the
    //     dead zone (the finger never really left the button) or outside the arc (past half a sector beyond its first or last item) picks
    //     nothing
    public static class RadialMenuRule
    {
        // The widest gap between two neighbours (a marking menu of 8 items gives each 45 degrees)
        public const float MaxStepDegrees = 45f;
        // Where the arc lies: the left half-plane
        public const float HighestDegrees = 90f;
        public const float LowestDegrees = 270f;
        // An arc of fewer than two items is no choice (one section and More is the least a page holds)
        public const int MinItems = 2;
        private const float Left = 180f;

        // The room around the button's CENTRE: to the layer's top, bottom, left and right edge
        public readonly struct Room
        {
            public readonly float Above, Below, LeftOf, RightOf;

            public Room(float above, float below, float leftOf, float rightOf)
            {
                Above = above;
                Below = below;
                LeftOf = leftOf;
                RightOf = rightOf;
            }
        }

        // One item's size: its round button and its word under it
        public readonly struct ItemSize
        {
            public readonly float Width, Height;

            public ItemSize(float width, float height)
            {
                Width = width;
                Height = height;
            }
        }

        public readonly struct Arc
        {
            public readonly int Count;
            public readonly float Radius;
            // The first (top) item's direction and the step between two neighbours, in degrees
            public readonly float FirstDegrees;
            public readonly float StepDegrees;
            // Every item inside the room and apart (false: the best arc there is, items may crowd)
            public readonly bool Fits;

            public Arc(int count, float radius, float firstDegrees, float stepDegrees, bool fits)
            {
                Count = count;
                Radius = radius;
                FirstDegrees = firstDegrees;
                StepDegrees = stepDegrees;
                Fits = fits;
            }

            // Item i's direction from the button's centre
            public float DegreesOf(int i) => FirstDegrees + i * StepDegrees;

            // Item i's centre relative to the button's centre, in panel units (y down)
            public Vector2 Offset(int i)
            {
                float radians = DegreesOf(i) * Mathf.Deg2Rad;
                return new Vector2(Radius * Mathf.Cos(radians), -Radius * Mathf.Sin(radians));
            }
        }

        // The arc of `count` items in this room: the smallest radius that fits, else the best there is (Fits false)
        public static Arc Layout(int count, Room room, ItemSize item, float minRadius)
        {
            if (count <= 0) return new Arc(0, minRadius, Left, 0f, true);
            float maxRadius = Mathf.Max(minRadius, room.LeftOf - item.Width / 2f);
            Arc best = default;
            float bestApart = -1f;
            for (float radius = minRadius; radius <= maxRadius + 0.5f; radius += 1f)
            {
                if (!Span(radius, room, item, out float top, out float bottom)) continue;
                float step = count > 1 ? Mathf.Min(MaxStepDegrees, (bottom - top) / (count - 1)) : 0f;
                float used = step * (count - 1);
                // - centred on "left" as far as the room allows: a short arc at peek sits beside the button, not above it
                float first = Mathf.Clamp(Left - used / 2f, top, bottom - used);
                var arc = new Arc(count, radius, first, step, true);
                float apart = Apart(arc, item);
                if (apart >= 1f - 1e-4f && LeftmostReach(arc) + item.Width / 2f <= room.LeftOf + 0.01f) return arc;
                if (apart > bestApart)
                {
                    bestApart = apart;
                    best = new Arc(count, radius, first, step, false);
                }
            }
            return bestApart >= 0f ? best : new Arc(count, minRadius, Left - MaxStepDegrees * (count - 1) / 2f, MaxStepDegrees, false);
        }

        // The most items an arc holds in this room: `wanted` when they fit, else fewer, never under MinItems
        public static int Capacity(int wanted, Room room, ItemSize item, float minRadius)
        {
            for (int count = wanted; count > MinItems; count--)
                if (Layout(count, room, item, minRadius).Fits) return count;
            return Mathf.Min(wanted, MinItems);
        }

        // The directions an item centre may take at this radius so the item stays inside the room above and below its centre's row and right of
        // it (an item straight up or down sits on the button's own column): false when there is none
        private static bool Span(float radius, Room room, ItemSize item, out float top, out float bottom)
        {
            float above = room.Above - item.Height / 2f, below = room.Below - item.Height / 2f, right = room.RightOf - item.Width / 2f;
            top = Mathf.Max(HighestDegrees, Left - Mathf.Asin(Mathf.Clamp(above / radius, -1f, 1f)) * Mathf.Rad2Deg);
            bottom = Mathf.Min(LowestDegrees, Left + Mathf.Asin(Mathf.Clamp(below / radius, -1f, 1f)) * Mathf.Rad2Deg);
            // - an item at direction d reaches radius * cos(d) right of the button: keep that within `right` (cos falls from 0 at straight up to
            //   -1 at left, and rises back to 0 at straight down)
            float rightmost = Mathf.Acos(Mathf.Clamp(right / radius, -1f, 1f)) * Mathf.Rad2Deg;
            top = Mathf.Max(top, rightmost);
            bottom = Mathf.Min(bottom, 360f - rightmost);
            return bottom >= top;
        }

        // How far apart the closest two items are, as a share of what keeps them apart: every pair is a width apart across or a height apart
        // down when it is at least 1
        public static float Apart(Arc arc, ItemSize item)
        {
            float nearest = float.MaxValue;
            for (int i = 1; i < arc.Count; i++)
                for (int j = 0; j < i; j++)
                {
                    var gap = arc.Offset(i) - arc.Offset(j);
                    nearest = Mathf.Min(nearest, Mathf.Max(Mathf.Abs(gap.x) / item.Width, Mathf.Abs(gap.y) / item.Height));
                }
            return arc.Count > 1 ? nearest : 1f;
        }

        // How far left of the button the arc's furthest item centre lies
        private static float LeftmostReach(Arc arc)
        {
            float reach = 0f;
            for (int i = 0; i < arc.Count; i++) reach = Mathf.Max(reach, -arc.Offset(i).x);
            return reach;
        }

        // The direction of a finger's travel (panel units, y down) in degrees, 0 = right, 90 = up, in [0, 360)
        public static float DegreesOf(Vector2 travel)
        {
            float degrees = Mathf.Atan2(-travel.y, travel.x) * Mathf.Rad2Deg;
            return degrees < 0f ? degrees + 360f : degrees;
        }

        // The item a release after this travel picks, or -1: inside the dead zone, or outside the arc. A direction exactly between two
        // items picks the first of them
        public static int ItemAt(Vector2 travel, Arc arc, float deadZone)
        {
            if (arc.Count <= 0 || travel.magnitude < deadZone) return -1;
            float degrees = DegreesOf(travel);
            float halfSector = (arc.Count > 1 ? arc.StepDegrees : MaxStepDegrees) / 2f;
            int nearest = -1;
            float nearestGap = float.MaxValue;
            for (int i = 0; i < arc.Count; i++)
            {
                float gap = Mathf.Abs(degrees - arc.DegreesOf(i));
                // - a later item must be CLEARLY nearer: a direction on the border between two stays with the first, whatever float noise says
                if (gap < nearestGap - 1e-3f)
                {
                    nearestGap = gap;
                    nearest = i;
                }
            }
            // - a hair of float slack, so a direction exactly on a sector's outer edge still counts
            return nearestGap <= halfSector + 1e-3f ? nearest : -1;
        }
    }
}

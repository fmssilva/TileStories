using UnityEngine;

namespace TileStories
{
    // Where a place_in_ar model stands (_3.1 step 10B.1), pure and in the WALL's frame (the POI positions' frame, WallSession.MarkerSpawnRoot):
    //   anchor poi_on_wall -- the POI's position pushed out along the wall's normal by Offset From Wall. The normal is the POI's own
    //                         authored facing: a marker's forward points INTO the wall (the side a world-space canvas is read from,
    //                         _2.1), so the outward normal is its back, flipped when the viewer stands on the other side.
    //   anchor surface     -- a surface found by AR plane detection: named for the Editor, not built yet (falls back to poi_on_wall).
    //   scale real_size    -- the model's own metres; height_cm -- scaled so it is this tall; marker_multiple -- this many times the
    //                         POI marker's diameter tall.
    // The model stands upright (the wall frame's up) and shows the viewer the side the card's preview shows at rest (its -Z, where the
    // preview camera stands), with the centre of its bounds on the pushed-out point (the point the preview turns it about).
    public static class ArPlacementRule
    {
        public const string AnchorPoiOnWall = "poi_on_wall";
        public const string AnchorSurface = "surface";
        public const string ScaleRealSize = "real_size";
        public const string ScaleHeightCm = "height_cm";
        public const string ScaleMarkerMultiple = "marker_multiple";

        // Whether a placed model goes when the card's selection becomes `selectedPoiId` (null: the card closed). Keep On Switch keeps it through
        // every change; otherwise it stays only while the point it was placed for stays selected (a live edit shows the same point again)
        public static bool RemovesOnSelection(string placedPoiId, string selectedPoiId, bool keepOnSwitch) =>
            !keepOnSwitch && placedPoiId != null && placedPoiId != selectedPoiId;

        // A flat model (no height) is measured as this tall, so no scale divides by zero
        public const float MinModelHeightMetres = 0.001f;
        // Below this length a horizontal direction is no direction (a POI facing straight up or down)
        private const float FlatDirection = 1e-4f;

        // What the rule needs: the POI's place and facing on the wall, the viewer (null when unknown), the block's choices and the
        // model's bounds at scale 1 in its own frame. Lengths in the wall frame are metres times WallScale (the frame's own scale).
        public struct Input
        {
            public Vector3 PoiPosition;
            public Quaternion PoiRotation;
            public Vector3? Viewer;
            public float OffsetCm;
            public string ScaleMode;
            public float HeightCm;
            public float MarkerMultiple;
            // The POI marker's diameter in the wall frame (its hierarchy level's size)
            public float MarkerDiameter;
            // The wall frame's uniform scale in the world (1 in a normal scene)
            public float WallScale;
            public Bounds ModelBounds;
        }

        // Where the model's root goes, in the wall frame, and the outward normal and target point it was worked out from
        public readonly struct Placement
        {
            public readonly Vector3 LocalPosition;
            public readonly Quaternion LocalRotation;
            public readonly float LocalScale;
            public readonly Vector3 Normal;
            public readonly Vector3 Target;

            public Placement(Vector3 localPosition, Quaternion localRotation, float localScale, Vector3 normal, Vector3 target)
            {
                LocalPosition = localPosition;
                LocalRotation = localRotation;
                LocalScale = localScale;
                Normal = normal;
                Target = target;
            }
        }

        // The wall's outward normal at a POI: the back of its authored facing, turned to the viewer's side when the viewer is known
        public static Vector3 OutwardNormal(Quaternion poiRotation, Vector3 poiPosition, Vector3? viewer)
        {
            var normal = -(poiRotation * Vector3.forward);
            if (viewer.HasValue && Vector3.Dot(normal, viewer.Value - poiPosition) < 0f) normal = -normal;
            return normal.normalized;
        }

        // The model's uniform scale in the wall frame for a scale mode (an unknown mode reads as real_size)
        public static float ScaleOf(string mode, float modelHeightMetres, float heightCm, float markerMultiple, float markerDiameter, float wallScale)
        {
            float height = Mathf.Max(MinModelHeightMetres, modelHeightMetres);
            float frame = wallScale > 0f ? wallScale : 1f;
            switch (mode)
            {
                case ScaleHeightCm: return heightCm / 100f / height / frame;
                // - the marker's diameter is already a wall-frame length, as is the model's height once scaled by it
                case ScaleMarkerMultiple: return markerMultiple * markerDiameter / height;
                default: return 1f / frame;
            }
        }

        public static Placement Place(Input input)
        {
            float frame = input.WallScale > 0f ? input.WallScale : 1f;
            var normal = OutwardNormal(input.PoiRotation, input.PoiPosition, input.Viewer);
            var target = input.PoiPosition + normal * (input.OffsetCm / 100f / frame);
            // - upright: the model faces along the normal's horizontal part (a POI on a floor or ceiling faces the viewer instead)
            var facing = Vector3.ProjectOnPlane(normal, Vector3.up);
            if (facing.sqrMagnitude < FlatDirection && input.Viewer.HasValue) facing = Vector3.ProjectOnPlane(input.Viewer.Value - target, Vector3.up);
            if (facing.sqrMagnitude < FlatDirection) facing = Vector3.back;
            // - its -Z towards the viewer's side: the side the card's preview shows at rest
            var rotation = Quaternion.LookRotation(-facing.normalized, Vector3.up);
            float scale = ScaleOf(input.ScaleMode, input.ModelBounds.size.y, input.HeightCm, input.MarkerMultiple, input.MarkerDiameter, frame);
            var position = target - rotation * (input.ModelBounds.center * scale);
            return new Placement(position, rotation, scale, normal, target);
        }
    }
}

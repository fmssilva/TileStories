using System.Collections.Generic;
using UnityEngine;

namespace TileStories
{
    // How far back the preview camera stands so a model fills its stage (_3.1 10A.3-fix.2). The model is fitted by its
    // bounding SPHERE, not its box: a turn (a drag, auto-spin) never changes a sphere, so whatever angle the visitor turns
    // it to, the model stays inside the picture. Pure, so the maths is a unit test; CardPreviewStage only feeds it bounds.
    public static class ModelFitRule
    {
        // The model's bounding sphere spans this share of the stage's SHORTER side at zoom 1
        public const float TargetFillOfShorterSide = 0.8f;

        // The sphere around the combined boxes: centred on their union's centre, reaching the farthest corner of any box
        // (tighter than the union box's own diagonal when the boxes do not fill it). An empty list is a point at the origin.
        public static (Vector3 Center, float Radius) SphereOf(IReadOnlyList<Bounds> boxes)
        {
            if (boxes == null || boxes.Count == 0) return (Vector3.zero, 0f);
            var union = boxes[0];
            for (int i = 1; i < boxes.Count; i++) union.Encapsulate(boxes[i]);
            float farthest = 0f;
            foreach (var box in boxes)
            {
                var e = box.extents;
                for (int corner = 0; corner < 8; corner++)
                {
                    var offset = new Vector3((corner & 1) == 0 ? -e.x : e.x, (corner & 2) == 0 ? -e.y : e.y, (corner & 4) == 0 ? -e.z : e.z);
                    farthest = Mathf.Max(farthest, (box.center + offset - union.center).magnitude);
                }
            }
            return (union.center, farthest);
        }

        // The camera distance from the sphere's centre at which the sphere's silhouette spans `fill` of the stage's shorter
        // side. `verticalFovDegrees` is the camera's field of view, `aspect` the stage's width / height. The silhouette of a
        // sphere of radius r seen from distance d has half-angle asin(r / d), so d = r / sin(atan(fill * tan(halfFov of the
        // shorter axis))).
        public static float DistanceFor(float radius, float verticalFovDegrees, float aspect, float fill = TargetFillOfShorterSide)
        {
            aspect = Mathf.Max(aspect, 0.01f);
            float halfFovTan = Mathf.Tan(verticalFovDegrees * 0.5f * Mathf.Deg2Rad);
            float shorterAxisTan = halfFovTan * Mathf.Min(1f, aspect);
            float halfAngle = Mathf.Atan(Mathf.Clamp(fill, 0.05f, 1f) * shorterAxisTan);
            return Mathf.Max(radius / Mathf.Sin(halfAngle), 0.01f);
        }
    }
}

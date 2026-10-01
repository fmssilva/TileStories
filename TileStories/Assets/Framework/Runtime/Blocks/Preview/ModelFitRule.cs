using System.Collections.Generic;
using UnityEngine;

namespace TileStories
{
    // How a model preview is framed (_3.1 10B-pre.1): a developer CHOICE, since each option trades size against safety
    public enum ModelFitMode
    {
        // Every yaw at the current pitch inside the frame: the camera pulls back only as far as the tilt needs (default)
        YawSafe,
        // The bounding sphere: inside at ANY rotation, the smallest picture
        Sphere,
        // The bounds at the start angle: the largest picture, may clip once turned
        AtRest,
        // Like YawSafe, but counting only the triangles that face the camera from each direction (what a one-sided scan really
        // draws): a hollow room scan fills the frame instead of its empty box
        Visible,
    }

    // How far back the preview camera stands so a model fills its stage (_3.1 10A.3-fix.2, 10B-pre.1). The camera ORBITS the model
    // (CardPreviewStage: rotation Euler(pitch, yaw, 0) about the pivot), so a "turn" is a camera direction. Three fits:
    //   sphere   -- the bounding sphere spans TargetFillOfShorterSide of the stage's shorter side; no turn ever changes it.
    //   yaw_safe -- the renderers' box corners fit FrameFill of the frame for EVERY yaw at the pitch the visitor holds now. A fixed
    //               distance safe for every yaw AND every pitch the turntable allows (+-80) is the sphere again (every box corner lies
    //               on the bounding sphere, and a steep tilt brings one to the top), so the distance follows the pitch: at rest a long
    //               model fills the frame, tilting pulls the camera back just enough, and nothing ever clips.
    //   at_rest  -- the box corners fit FrameFill at the start angle only (yaw 0, pitch 0): largest, may clip once turned.
    // Pure, so the maths is a unit test; CardPreviewStage only feeds it bounds.
    public static class ModelFitRule
    {
        // Config values of the Fit choice (BuiltInBlocks.Model3DFitField / HeaderModelFitField)
        public const string FitYawSafe = "yaw_safe";
        public const string FitSphere = "sphere";
        public const string FitAtRest = "at_rest";
        public const string FitVisible = "visible";
        // The visible fit samples the yaw circle this finely (degrees): a face that turns toward the camera between two samples is
        // inside the frame margin, never past the edge, at this step
        public const float VisibleYawStepDegrees = 10f;

        // The model's bounding sphere spans this share of the stage's SHORTER side at zoom 1
        public const float TargetFillOfShorterSide = 0.8f;
        // The box fits keep every corner within this share of the frame on each axis: a 5 % margin on each edge (the pixel tests'
        // margin) plus about one rasteriser pixel on a phone-size stage
        public const float FrameFill = 0.88f;
        // The camera never stands closer than this in front of the nearest corner (the near plane's own room)
        private const float MinDepthMetres = 0.02f;

        // The mode a stored Fit value names; anything else (empty, a stale value) is the default
        public static ModelFitMode ModeOf(string value) =>
            value == FitSphere ? ModelFitMode.Sphere : value == FitAtRest ? ModelFitMode.AtRest : value == FitVisible ? ModelFitMode.Visible : ModelFitMode.YawSafe;

        // The sphere around the combined boxes: centred on their union's centre, reaching the farthest corner of any box
        // (tighter than the union box's own diagonal when the boxes do not fill it). An empty list is a point at the origin.
        public static (Vector3 Center, float Radius) SphereOf(IReadOnlyList<Bounds> boxes)
        {
            if (boxes == null || boxes.Count == 0) return (Vector3.zero, 0f);
            var centre = UnionCentre(boxes);
            float farthest = 0f;
            foreach (var corner in CornersAround(boxes, centre)) farthest = Mathf.Max(farthest, corner.magnitude);
            return (centre, farthest);
        }

        // Every corner of every box, relative to `pivot` (the points the box fits keep inside the frame)
        public static List<Vector3> CornersAround(IReadOnlyList<Bounds> boxes, Vector3 pivot)
        {
            var corners = new List<Vector3>();
            if (boxes == null) return corners;
            foreach (var box in boxes)
            {
                var e = box.extents;
                for (int corner = 0; corner < 8; corner++)
                    corners.Add(box.center + new Vector3((corner & 1) == 0 ? -e.x : e.x, (corner & 2) == 0 ? -e.y : e.y, (corner & 4) == 0 ? -e.z : e.z) - pivot);
            }
            return corners;
        }

        // The centre of the boxes' union: the pivot every fit orbits
        public static Vector3 UnionCentre(IReadOnlyList<Bounds> boxes)
        {
            if (boxes == null || boxes.Count == 0) return Vector3.zero;
            var union = boxes[0];
            for (int i = 1; i < boxes.Count; i++) union.Encapsulate(boxes[i]);
            return union.center;
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

        // yaw_safe: the distance at which every corner stays inside FrameFill of the frame for EVERY yaw at `pitchDegrees`.
        // Per corner v (relative to the pivot) the orbit's yaw sweeps it round a circle of horizontal radius rho at height y; in
        // camera space x = rho sin t, y' = y cos p + rho cos t sin p, depth = d - y sin p + rho cos t cos p. Inside means
        // |x| <= kh * depth and |y'| <= kv * depth, so d >= |x| / kh - z (z = depth - d). Over the whole circle:
        //   horizontal: y sin p + rho * sqrt(1 / kh^2 + cos^2 p)                         (the max of a sine plus a cosine)
        //   vertical:   the larger of the two ends cos t = +1 / -1                        (|linear| + linear is convex in cos t)
        public static float YawSafeDistance(IReadOnlyList<Vector3> corners, float pitchDegrees, float verticalFovDegrees, float aspect, float fill = FrameFill)
        {
            var (kh, kv) = FrameTangents(verticalFovDegrees, aspect, fill);
            float p = pitchDegrees * Mathf.Deg2Rad, sin = Mathf.Sin(p), cos = Mathf.Cos(p);
            float distance = 0f;
            foreach (var v in corners)
            {
                float rho = new Vector2(v.x, v.z).magnitude, y = v.y;
                distance = Mathf.Max(distance, y * sin + rho * Mathf.Sqrt(1f / (kh * kh) + cos * cos));
                for (int end = -1; end <= 1; end += 2)
                {
                    float z = -y * sin + end * rho * cos;
                    distance = Mathf.Max(distance, Mathf.Abs(y * cos + end * rho * sin) / kv - z, MinDepthMetres - z);
                }
            }
            return Mathf.Max(distance, 0.01f);
        }

        // at_rest: the distance at which every corner stays inside FrameFill of the frame at the start angle (yaw 0, pitch 0: the
        // camera looks along +z, so a corner's x / y / z are already camera space)
        public static float AtRestDistance(IReadOnlyList<Vector3> corners, float verticalFovDegrees, float aspect, float fill = FrameFill)
        {
            var (kh, kv) = FrameTangents(verticalFovDegrees, aspect, fill);
            float distance = 0f;
            foreach (var v in corners)
                distance = Mathf.Max(distance, Mathf.Abs(v.x) / kh - v.z, Mathf.Abs(v.y) / kv - v.z, MinDepthMetres - v.z);
            return Mathf.Max(distance, 0.01f);
        }

        // visible: the distance at which, for every sampled yaw at `pitchDegrees`, every corner of a triangle that FACES the camera
        // (its normal toward the camera, standing `referenceDistance` back) stays inside FrameFill of the frame. `triangles` are
        // corner triples relative to the pivot (a, b, c in the mesh's winding: Unity's front face is clockwise seen from the
        // camera, so its normal is cross(b - a, c - a)). Backfaces are culled by the renderer, so they never count.
        public static float VisibleDistance(IReadOnlyList<Vector3> triangles, float pitchDegrees, float referenceDistance, float verticalFovDegrees, float aspect, float fill = FrameFill)
        {
            var (kh, kv) = FrameTangents(verticalFovDegrees, aspect, fill);
            float distance = 0f;
            for (float yaw = 0f; yaw < 360f; yaw += VisibleYawStepDegrees)
            {
                var toCamera = Quaternion.Inverse(Quaternion.Euler(pitchDegrees, yaw, 0f));
                var camera = Quaternion.Euler(pitchDegrees, yaw, 0f) * Vector3.back * referenceDistance;
                for (int t = 0; t + 2 < triangles.Count; t += 3)
                {
                    Vector3 a = triangles[t], b = triangles[t + 1], c = triangles[t + 2];
                    if (Vector3.Dot(Vector3.Cross(b - a, c - a), camera - a) <= 0f) continue; // facing away: culled, never drawn
                    for (int k = 0; k < 3; k++)
                    {
                        var p = toCamera * triangles[t + k];
                        distance = Mathf.Max(distance, Mathf.Abs(p.x) / kh - p.z, Mathf.Abs(p.y) / kv - p.z, MinDepthMetres - p.z);
                    }
                }
            }
            return Mathf.Max(distance, 0.01f);
        }

        // Half the frame's width and height as tangents, shrunk to `fill` (width follows the stage's aspect)
        private static (float Horizontal, float Vertical) FrameTangents(float verticalFovDegrees, float aspect, float fill)
        {
            float vertical = Mathf.Tan(verticalFovDegrees * 0.5f * Mathf.Deg2Rad) * Mathf.Clamp(fill, 0.05f, 1f);
            return (vertical * Mathf.Max(aspect, 0.01f), vertical);
        }
    }
}

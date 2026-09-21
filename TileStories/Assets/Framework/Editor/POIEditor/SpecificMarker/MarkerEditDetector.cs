using UnityEngine;

namespace TileStories.Editor
{
    // Pure "did the selected marker's pose just change?" tracker. The window feeds it the
    // selected rig marker's local pose on every observation; it answers true only when the
    // SAME marker was seen before with a different pose. Selecting a different marker (or
    // nothing) starts fresh, so merely clicking a marker is never mistaken for an edit.
    public sealed class MarkerEditDetector
    {
        private const float PositionEpsilonSqr = 1e-8f;

        private string _id;
        private Vector3 _position;
        private Quaternion _rotation;

        // Record this observation and report whether the marker moved or rotated since
        // the previous observation of the same id.
        public bool Observe(string id, Vector3 localPosition, Quaternion localRotation)
        {
            bool sameMarker = id != null && id == _id;
            bool changed = sameMarker
                && ((localPosition - _position).sqrMagnitude > PositionEpsilonSqr
                    || !PoiRotationResolver.IsSameOrientation(localRotation, _rotation));

            _id = id;
            _position = localPosition;
            _rotation = localRotation;
            return changed;
        }

        // True while a Move/Rotate/Rect handle owns the mouse mid-drag. Camera orbit/pan/zoom
        // also grab the hot control, but they set Tools.viewToolActive, so they are excluded.
        // This is the only edit signal when Edit-Mode preview rewrites the marker's pose
        // every repaint (a pose diff can no longer tell the developer's drag from the preview).
        public static bool IsGizmoEditGesture(bool isMouseDrag, int hotControl, bool viewToolActive, UnityEditor.Tool tool)
        {
            if (!isMouseDrag || hotControl == 0 || viewToolActive)
                return false;

            return tool == UnityEditor.Tool.Move
                || tool == UnityEditor.Tool.Rotate
                || tool == UnityEditor.Tool.Rect
                || tool == UnityEditor.Tool.Transform;
        }

        // Forget the current marker (nothing selected): the next observation is a first sight.
        public void Reset()
        {
            _id = null;
        }
    }
}

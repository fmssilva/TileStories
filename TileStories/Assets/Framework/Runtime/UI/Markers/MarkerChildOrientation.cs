using UnityEngine;

namespace TileStories
{
    // One instance on the Label and one on the Badge (_2.1_Marker_Orientation.md section 9.2).
    // Counter-rotates a child (pure Z) so its up matches label_orientation_mode /
    // badge_orientation_mode independently of the root's own orientation, and, for a
    // badge with badge_corner_mode == "screen_fixed", holds its screen corner as the
    // root rolls. Does nothing at all when its mode is "inherit".
    public class MarkerChildOrientation : MonoBehaviour
    {
        private string _mode = "inherit";
        private string _badgeCornerMode = "inherit";
        private RectTransform _rect;
        private Vector2 _baseAnchoredPosition;
        private bool _baseCaptured;

        // Called once by MarkerBillboard.Configure - never per frame.
        public void Configure(string mode, string badgeCornerMode)
        {
            _mode = string.IsNullOrEmpty(mode) ? "inherit" : mode;
            _badgeCornerMode = string.IsNullOrEmpty(badgeCornerMode) ? "inherit" : badgeCornerMode;
            _rect = GetComponent<RectTransform>();
        }

        // Called by MarkerBillboard.LateUpdate after the root's rotation for this frame is
        // final, so the roll measured against it is correct.
        public void Tick(Quaternion rootWorldRotation, Vector3 screenUpWorld, Vector3 upReference)
        {
            if (_rect == null) _rect = GetComponent<RectTransform>();
            if (_rect == null) return;

            if (!_baseCaptured)
            {
                _baseAnchoredPosition = _rect.anchoredPosition;
                _baseCaptured = true;
            }

            transform.localRotation = MarkerOrientationResolver.ResolveChildLocalRotation(_mode, rootWorldRotation, screenUpWorld, upReference);

            if (_badgeCornerMode == "screen_fixed")
            {
                float rollDeg = MarkerOrientationResolver.RootRollDeg(rootWorldRotation, screenUpWorld);
                _rect.anchoredPosition = MarkerOrientationResolver.Rotate2D(_baseAnchoredPosition, -rollDeg);
            }
        }
    }
}

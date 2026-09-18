using UnityEngine;

namespace TileStories
{
    // One instance on the Label and one on the Badge (_2.1_Marker_Orientation.md section 9.2).
    // Counter-rotates a child (pure Z) so its up matches its own Vertical Alignment mode
    // independently of the root's, and does nothing at all when that mode is "inherit".
    // The badge's screen position is never independently held (_2.1_Marker_Orientation.md
    // v4: v3's "screen_fixed" badge-corner mechanism was removed) -- it simply goes
    // wherever the root's own rotation puts it, like every other child.
    public class MarkerChildOrientation : MonoBehaviour
    {
        private string _verticalAlignmentMode = "inherit";

        // Called once by MarkerBillboard.Configure - never per frame.
        public void Configure(string verticalAlignmentMode)
        {
            _verticalAlignmentMode = string.IsNullOrEmpty(verticalAlignmentMode) ? "inherit" : verticalAlignmentMode;
        }

        // Called by MarkerBillboard.LateUpdate after the root's rotation for this frame is
        // final, so the roll measured against it is correct.
        public void Tick(Quaternion rootWorldRotation, Vector3 screenUpWorld, Vector3 upReference)
        {
            transform.localRotation = MarkerOrientationResolver.ResolveChildLocalRotation(
                _verticalAlignmentMode, rootWorldRotation, screenUpWorld, upReference);
        }
    }
}

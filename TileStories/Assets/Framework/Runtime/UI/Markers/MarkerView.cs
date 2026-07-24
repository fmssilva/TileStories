using TMPro;
using UnityEngine;

namespace TileStories
{
    // Visual representation of a POI marker in AR space (uGUI World Space Canvas prefab).
    // Shows a truncated label and handles basic tap detection.
    public class MarkerView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI labelText;
        [SerializeField] private float overlapOffsetAmount = 0.15f; // 15cm vertical nudge when markers overlap

        private POIAnchor _anchor;
        private Vector3 _baseLocalPosition;
        private bool _hasBasePosition;

        // Expose the POI id for deterministic sorting in overlap resolution
        public string PoiId { get; private set; }

        public void Initialise(POIAnchor anchor)
        {
            _anchor = anchor;
            PoiId = anchor?.Data?.id ?? string.Empty;
            if (labelText != null && anchor.Data != null)
            {
                // Truncate to 29 chars to show the distinguishing number in overlap test names
                // e.g., "The Painting (overlap test 1)" -> full name (29 chars)
                // This ensures markers like painting_1, painting_2, etc. have distinct labels
                string name = anchor.Data.name;
                if (name.Length > 29)
                    name = name.Substring(0, 26) + "...";
                labelText.text = name;
            }
        }

        // Nudge this marker up by overlapOffsetAmount * offsetIndex so it doesn't sit
        // on top of another marker that resolved to a similar screen position.
        // Idempotent: sets an absolute offset from the stored base position,
        // never adds to the current position.
        public void ApplyOverlapOffset(float offsetIndex)
        {
            if (_anchor == null) return;

            // Capture the base position on first call (spawn-time position)
            if (!_hasBasePosition)
            {
                _baseLocalPosition = _anchor.transform.localPosition;
                _hasBasePosition = true;
            }

            // Set position to base + offset, never add to current position
            Vector3 newPos = _baseLocalPosition;
            newPos.y += offsetIndex * overlapOffsetAmount;
            _anchor.transform.localPosition = newPos;
        }
    }
}
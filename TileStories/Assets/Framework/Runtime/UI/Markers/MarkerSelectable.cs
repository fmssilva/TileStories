using UnityEngine;
using UnityEngine.EventSystems;

namespace TileStories
{
    // Thin tap target on POI_Marker.prefab. Converts a uGUI pointer click (served by
    // the prefab Canvas' GraphicRaycaster -- no Physics.Raycast, no custom raycasting)
    // into a SelectionEventBus event carrying this marker's PoiId, and nothing else.
    // Does NOT own highlight/zoom logic (that lives in SelectionHighlightController /
    // ZoomOnSelectController) so it stays a thin MonoBehaviour per the "MonoBehaviours
    // stay thin" rule. MarkerView exposes PoiId (set in Initialise), so no getter
    // change is needed on MarkerView.
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MarkerView))]
    public sealed class MarkerSelectable : MonoBehaviour, IPointerClickHandler
    {
        private MarkerView _markerView;

        private void Awake()
        {
            // MarkerView sits on the same GameObject as this component.
            _markerView = GetComponent<MarkerView>();
        }

        // Raised by uGUI's GraphicRaycaster when the marker is tapped. Publishes the
        // POI id to any SelectionEventBus listener; a marker with no id is ignored
        // (authoring-time guard, should not happen at runtime).
        public void OnPointerClick(PointerEventData eventData)
        {
            // Lazy resolve so the handler is correct even if Awake has not run (e.g.
            // EditMode AddComponent), where _markerView would still be null.
            var mv = _markerView != null ? _markerView : GetComponent<MarkerView>();
            if (mv != null && !string.IsNullOrEmpty(mv.PoiId))
                SelectionEventBus.RaiseMarkerSelected(mv.PoiId);
        }
    }
}

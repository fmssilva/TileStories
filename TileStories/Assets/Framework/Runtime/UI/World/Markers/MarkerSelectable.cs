using UnityEngine;
using UnityEngine.EventSystems;

namespace TileStories
{
    // Thin tap target on POI_Marker.prefab: a uGUI pointer click (served by the prefab Canvas'
    // GraphicRaycaster, no Physics.Raycast) selects this marker's POI through SelectionEventBus, and
    // nothing else -- tapping the selected marker again deselects it (the bus decides). Highlight, zoom
    // and the detail card are listeners of the bus.
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

        // Raised by uGUI's GraphicRaycaster when the marker is tapped
        public void OnPointerClick(PointerEventData eventData)
        {
            // - lazy resolve: in EditMode AddComponent tests Awake may not have run
            var mv = _markerView != null ? _markerView : GetComponent<MarkerView>();
            if (mv != null)
                SelectionEventBus.Select(mv.PoiId);
        }
    }
}

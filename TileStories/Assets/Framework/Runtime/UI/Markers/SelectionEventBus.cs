using System;

namespace TileStories
{
    // Lightweight, allocation-free pub/sub relay for marker selection events.
    // Decouples the tap target (MarkerSelectable on the POI_Marker prefab) from the
    // systems that react to a selection (SelectionHighlightController,
    // ZoomOnSelectController) -- the prefab never needs a WallSession or controller
    // reference. Static, mirroring ARZoomState's one-source-of-truth pattern.
    // Callers MUST unsubscribe (via their Dispose) to avoid stale-listener leaks
    // across scene reloads.
    public static class SelectionEventBus
    {
        // Raised when a POI marker is tapped. Argument is the POI id (MarkerView.PoiId).
        public static event Action<string> OnMarkerSelected;

        // Raised to clear any active selection (empty-space tap, search filter clear,
        // spec block 3). Block 2 only raises OnMarkerSelected; the handler still
        // listens so a future clear path needs no wiring change.
        public static event Action OnSelectionCleared;

        // Raise a selection event for a tapped POI id.
        public static void RaiseMarkerSelected(string poiId) => OnMarkerSelected?.Invoke(poiId);

        // Clear the active selection.
        public static void RaiseSelectionCleared() => OnSelectionCleared?.Invoke();
    }
}

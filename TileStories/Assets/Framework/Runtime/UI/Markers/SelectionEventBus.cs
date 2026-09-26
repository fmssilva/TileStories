using System;
using System.Collections.Generic;

namespace TileStories
{
    // The one selection state of the app and its events (spec _2.6 section 11). Every input surface --
    // a marker tap, a minimap dot, a results-list row, a cluster tap -- goes through here, and every
    // reaction (highlight dim, zoom-on-select, detail card, list/minimap highlight) listens here, so
    // no surface needs to know another exists. Static like ARZoomState: one app, one selection.
    // Listeners MUST unsubscribe (their Dispose/OnDestroy) so a reloaded scene keeps no stale ones.
    public static class SelectionEventBus
    {
        // Raised with the POI id when a POI becomes the selection
        public static event Action<string> OnMarkerSelected;

        // Raised when the selection is cleared (re-tap, the card's close button, a new wall)
        public static event Action OnSelectionCleared;

        // Raised with the member POI ids when a cluster aggregate is tapped (it is not a selection)
        public static event Action<IReadOnlyList<string>> OnClusterSelected;

        // The selected POI id, or null
        public static string CurrentPoiId { get; private set; }

        // Select a POI. Selecting the POI that is already selected clears the selection instead
        // (tap again to deselect): decided HERE, before any listener runs, so every listener sees
        // the same single event.
        public static void Select(string poiId)
        {
            if (string.IsNullOrEmpty(poiId))
                return;
            if (poiId == CurrentPoiId)
            {
                Clear();
                return;
            }
            CurrentPoiId = poiId;
            OnMarkerSelected?.Invoke(poiId);
        }

        // Clear the selection (no event when nothing is selected)
        public static void Clear()
        {
            if (CurrentPoiId == null)
                return;
            CurrentPoiId = null;
            OnSelectionCleared?.Invoke();
        }

        // A cluster aggregate was tapped
        public static void SelectCluster(IReadOnlyList<string> memberPoiIds)
        {
            if (memberPoiIds != null && memberPoiIds.Count > 0)
                OnClusterSelected?.Invoke(memberPoiIds);
        }

        // Forget the selection without raising events (a wall session starting or ending)
        public static void ResetState() => CurrentPoiId = null;
    }
}

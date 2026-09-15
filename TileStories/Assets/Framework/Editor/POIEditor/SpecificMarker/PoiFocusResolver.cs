namespace TileStories.Editor
{
    // Pure focus-target resolution for the per-POI "Focus in Scene" button.
    // Editor windows cannot be unit-tested headless (no SceneView, no Selection),
    // so the decision logic lives here as a plain static class: given a POI id,
    // the rig root, and the list of known POI ids, decide which GameObject name
    // to focus. The window itself only does Selection + SceneView framing.
    // No UnityEditor dependency: operates on names/ids only, Tier-0 testable.
    public static class PoiFocusResolver
    {
        // How many marker-widths should fit across the framed view. 1.5 makes the
        // focused POI fill ~2/3 of the view with a sliver of neighbour context
        // either side. Float (not int) so fine steps below 2 are possible --
        // the lamp (level_1, 30cm) frames identically at 2-across and 3-across
        // once its 0.7m box is reached, which is what read as "no zoom change".
        public const float DefaultMarkersAcross = 1.5f;

        // Floor so tiny markers still show a usable patch, not a microscope
        // view. Kept well BELOW the typical 1.5-across width so it only catches
        // degenerate/zero readings instead of masking the zoom constant.
        // In metres of view width at the marker's depth.
        public const float MinFocusWidth = 0.2f;

        // Returns the rig-child name to focus, or null when there is nothing
        // sensible to focus (empty id, no rig, unknown id).
        public static string ResolveFocusTargetName(
            string poiId, bool hasRig, System.Collections.Generic.ICollection<string> knownPoiIds)
        {
            if (string.IsNullOrWhiteSpace(poiId))
                return null;
            if (!hasRig)
                return null;
            if (knownPoiIds == null || !knownPoiIds.Contains(poiId))
                return null;
            return poiId;
        }

        // Width of the context box to frame around the marker, in world units:
        // the marker's own visual diameter times how many markers should fit
        // across, floored so tiny markers still show surrounding context.
        // Pure math, Tier-0 testable. SceneView.Frame(bounds) then keeps the
        // current view direction and only dollies to fit this width.
        public static float ComputeFocusWidth(
            float visualDiameter,
            float markersAcross = DefaultMarkersAcross,
            float minWidth = MinFocusWidth)
        {
            if (markersAcross < 0.25f)
                markersAcross = 0.25f;
            if (minWidth <= 0f)
                minWidth = MinFocusWidth;
            float width = visualDiameter * markersAcross;
            return width < minWidth ? minWidth : width;
        }
    }
}

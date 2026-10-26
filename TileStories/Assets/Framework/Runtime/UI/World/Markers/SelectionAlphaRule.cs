namespace TileStories
{
    // The one rule for a marker's selection-channel opacity (MarkerView.SetSelectionAlpha), combining
    // the tap selection and the active search / filter result set (spec _2.6 sections 7 and 11). Pure,
    // so the whole truth table is testable; SelectionHighlightController is its only caller and the
    // only writer of that channel, so selection and filtering can never overwrite each other.
    public static class SelectionAlphaRule
    {
        // - the selected marker is always full: selecting is a stronger signal than filtering
        // - outside an active result set: the filter's mismatch opacity (0 = hidden), selection or not
        // - otherwise, while another marker is selected and the highlight is on: the selection dim
        // - otherwise full
        public static float AlphaFor(bool isSelected, bool anySelected, bool resultSetActive, bool inResultSet,
            bool highlightEnabled, float selectionDimAlpha, float mismatchAlpha)
        {
            if (isSelected)
                return 1f;
            if (resultSetActive && !inResultSet)
                return mismatchAlpha;
            if (anySelected && highlightEnabled)
                return selectionDimAlpha;
            return 1f;
        }
    }
}

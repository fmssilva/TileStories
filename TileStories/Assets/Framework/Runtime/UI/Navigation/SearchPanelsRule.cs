namespace TileStories
{
    // Which search UI panels are on screen (spec _2.6 sections 7-10, 14), as one pure rule so the whole
    // table is testable. The camera view stays clear until the visitor searches or filters:
    //   - results are "active" while there is query text or at least one filter
    //   - the view switch (List / Minimap / Highlight) shows only while results are active
    //   - the list shows in List view while results are active and nothing is selected
    //   - a selection shows the detail card instead of the list
    //   - the minimap shows while it is enabled and either set to "always", opened with its button, or
    //     the Minimap view is showing active results
    // "Highlight" view shows no panel at all: the markers themselves show the result set.
    public readonly struct SearchPanels
    {
        public readonly bool ViewModes;
        public readonly bool List;
        public readonly bool Minimap;
        public readonly bool Card;
        public readonly bool MinimapButton;

        public SearchPanels(bool viewModes, bool list, bool minimap, bool card, bool minimapButton)
        {
            ViewModes = viewModes;
            List = list;
            Minimap = minimap;
            Card = card;
            MinimapButton = minimapButton;
        }
    }

    public static class SearchPanelsRule
    {
        public static SearchPanels Resolve(bool resultsActive, ViewMode view, bool selectionActive,
            bool minimapEnabled, bool minimapAlways, bool minimapToggledOpen)
        {
            bool list = resultsActive && view == ViewMode.List && !selectionActive;
            bool minimap = minimapEnabled
                && (minimapAlways || minimapToggledOpen || (resultsActive && view == ViewMode.Minimap));
            bool button = minimapEnabled && !minimapAlways;
            return new SearchPanels(resultsActive, list, minimap, selectionActive, button);
        }
    }

    // Where the result set is shown (select_filter_search.results.default_view)
    public enum ViewMode
    {
        List,
        Minimap,
        CameraHighlight,
    }
}

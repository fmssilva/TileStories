namespace TileStories
{
    // Which search UI panels are on screen (spec _2.6 sections 7-10, 14), as one pure rule so the whole
    // table is testable. The camera view stays clear until the visitor searches or filters:
    //   - results are "active" while there is query text or at least one filter
    //   - the view switch (List / Minimap / Highlight) shows only while results are active
    //   - the list shows in List view while results are active and nothing is selected
    //   - a selection hides the list (the POI Detail Card, PoiCardHost, shows the selected POI)
    //   - the minimap shows while it is enabled and either set to "always", opened with its button, or
    //     the Minimap view is showing active results
    //   - the top (search bar, filter tray, view switch) hides while something covers the top of the screen --
    //     the POI Detail Card at its full stop (_3.1 step 6C): the visitor is reading, and a bar half under the card
    //     could be neither read nor tapped
    // "Highlight" view shows no panel at all: the markers themselves show the result set.
    public readonly struct SearchPanels
    {
        public readonly bool Top;
        public readonly bool ViewModes;
        public readonly bool List;
        public readonly bool Minimap;
        public readonly bool MinimapButton;

        public SearchPanels(bool top, bool viewModes, bool list, bool minimap, bool minimapButton)
        {
            Top = top;
            ViewModes = viewModes;
            List = list;
            Minimap = minimap;
            MinimapButton = minimapButton;
        }
    }

    public static class SearchPanelsRule
    {
        public static SearchPanels Resolve(bool resultsActive, ViewMode view, bool selectionActive,
            bool minimapEnabled, bool minimapAlways, bool minimapToggledOpen, bool topCovered)
        {
            bool list = resultsActive && view == ViewMode.List && !selectionActive;
            bool minimap = minimapEnabled
                && (minimapAlways || minimapToggledOpen || (resultsActive && view == ViewMode.Minimap));
            bool button = minimapEnabled && !minimapAlways;
            return new SearchPanels(!topCovered, resultsActive && !topCovered, list, minimap, button);
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

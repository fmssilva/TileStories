using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories
{
    // The result view switch (spec _2.6 section 10): List | Map | Highlight. "Map" is offered only while
    // the minimap is enabled. The starting view is the wall's default_result_view, or -- when
    // remember_last_view is on -- the view this device chose last time (PlayerPrefs).
    // Plain C#: built into the parent the search UI hands it; styled by SearchUI.uss.
    public sealed class ViewModeControl
    {
        public const string LastViewPrefsKey = "TileStories.last_result_view";

        public VisualElement Root { get; }
        public ViewMode Mode { get; private set; } = ViewMode.List;

        // Raised after the visitor chose another view
        public event Action<ViewMode> Changed;

        private readonly Button _list;
        private readonly Button _map;
        private readonly Button _highlight;
        private ResultsSettings _results;

        public ViewModeControl(VisualElement parent)
        {
            Root = new VisualElement { name = "view-modes" };
            Root.AddToClassList("view-modes");
            _list = AddSegment("List", ViewMode.List);
            _map = AddSegment("Map", ViewMode.Minimap);
            _highlight = AddSegment("Highlight", ViewMode.CameraHighlight);
            parent.Add(Root);
        }

        public bool IsShown => Root.style.display != DisplayStyle.None;
        public void SetShown(bool shown) => Root.style.display = shown ? DisplayStyle.Flex : DisplayStyle.None;
        public bool MapOffered => _map.style.display != DisplayStyle.None;

        // Apply the wall's settings and pick the starting view
        public void Configure(ResultsSettings results, bool minimapEnabled)
        {
            _results = results ?? new ResultsSettings();
            _map.style.display = minimapEnabled ? DisplayStyle.Flex : DisplayStyle.None;
            string start = _results.remember_last_view
                ? PlayerPrefs.GetString(LastViewPrefsKey, _results.default_view)
                : _results.default_view;
            Mode = StartingView(ViewModeParser.Parse(start), minimapEnabled);
            RefreshSegments();
        }

        // The view to start in: the chosen one, or List when that is the map and the map is off
        public static ViewMode StartingView(ViewMode chosen, bool minimapEnabled) =>
            chosen == ViewMode.Minimap && !minimapEnabled ? ViewMode.List : chosen;

        // Switch view as if the visitor tapped its segment
        public void Choose(ViewMode mode)
        {
            if (mode == Mode) return;
            Mode = mode;
            if (_results != null && _results.remember_last_view)
            {
                PlayerPrefs.SetString(LastViewPrefsKey, ViewModeParser.ToString(mode));
                PlayerPrefs.Save();
            }
            RefreshSegments();
            Changed?.Invoke(mode);
        }

        private Button AddSegment(string label, ViewMode mode)
        {
            var b = new Button(() => Choose(mode)) { name = "view-mode-" + mode, text = label, tooltip = label + " view" };
            b.AddToClassList("view-mode");
            Root.Add(b);
            return b;
        }

        private void RefreshSegments()
        {
            _list.EnableInClassList("view-mode--on", Mode == ViewMode.List);
            _map.EnableInClassList("view-mode--on", Mode == ViewMode.Minimap);
            _highlight.EnableInClassList("view-mode--on", Mode == ViewMode.CameraHighlight);
        }
    }
}

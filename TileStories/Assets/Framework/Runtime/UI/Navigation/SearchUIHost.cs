using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories
{
    // The visitor's search, filter and select UI in a wall scene (spec _2.6): the ONE place its views are
    // composed. It builds the search bar, the filter tray, the view switch, the results list, the minimap
    // and the detail card into its own UIDocument, rebuilds them whenever the wall's searchable data
    // changes (WallSession.SearchDataChanged) and pushes the one result set to all of them and to the
    // markers (SelectionHighlightController). Every decision is a pure rule (ResultSetCoordinator,
    // SearchPanelsRule, FilterTrayView.BuildOptions); this class only wires and applies them.
    [RequireComponent(typeof(UIDocument))]
    public sealed class SearchUIHost : MonoBehaviour
    {
        [Tooltip("The wall whose POIs this UI searches")]
        [SerializeField] private WallSession wallSession;

        [Tooltip("SearchUI.uss: every layout and colour of the search UI")]
        [SerializeField] private StyleSheet styleSheet;

        private UIDocument _document;
        private VisualElement _root;
        private bool _subscribed;

        public SearchOverlayView SearchBar { get; private set; }
        public FilterTrayView Tray { get; private set; }
        public ViewModeControl ViewModes { get; private set; }
        public ResultsListView List { get; private set; }
        public MinimapView Minimap { get; private set; }
        public DetailCardView Card { get; private set; }

        // The last computed result set (what every surface shows)
        public ResultSetState State { get; private set; } = new();
        public string Query { get; private set; } = "";
        public SearchPanels Panels { get; private set; }
        public bool MinimapToggledOpen { get; private set; }
        public VoiceSearchController Voice { get; private set; }

        // The UI's root element (null before it is built), for tests and screenshots
        public VisualElement Root => _root;

        // Wire to a wall (scene setup, tests)
        public void Bind(WallSession session, StyleSheet sheet = null)
        {
            Unsubscribe();
            wallSession = session;
            if (sheet != null) styleSheet = sheet;
            if (isActiveAndEnabled) Subscribe();
        }

        private void OnEnable() => Subscribe();

        private void OnDisable() => Unsubscribe();

        private void Subscribe()
        {
            if (_subscribed || wallSession == null) return;
            _subscribed = true;
            wallSession.SearchDataChanged += Rebuild;
            SelectionEventBus.OnMarkerSelected += OnSelectionChanged;
            SelectionEventBus.OnSelectionCleared += OnSelectionCleared;
            if (wallSession.SearchIndex != null) Rebuild();
        }

        private void Unsubscribe()
        {
            if (!_subscribed) return;
            _subscribed = false;
            if (wallSession != null) wallSession.SearchDataChanged -= Rebuild;
            SelectionEventBus.OnMarkerSelected -= OnSelectionChanged;
            SelectionEventBus.OnSelectionCleared -= OnSelectionCleared;
            List?.Dispose();
            Minimap?.Dispose();
            Card?.Dispose();
            Voice?.Dispose();
            _root?.RemoveFromHierarchy();
            _root = null;
            SearchBar = null;
        }

        // Search a query as if the visitor typed it (suggestions, voice, the Editor's "Try a query")
        public void SetQuery(string query)
        {
            if (SearchBar == null) return;
            SearchBar.SetQuery(query);
        }

        // Rebuild every view for the wall's current searchable data and settings
        public void Rebuild()
        {
            if (wallSession == null) return;
            BuildOnce();
            var settings = wallSession.SelectFilterSearch ?? new SelectFilterSearchSettings();
            _root.style.display = settings.enabled ? DisplayStyle.Flex : DisplayStyle.None;
            if (!settings.enabled) return;

            var config = wallSession.SearchConfig;
            Tray.Rebuild(FilterTrayView.BuildOptions(config, settings.filter));
            Minimap.Rebuild(MarkerPoints(), settings.minimap, wallSession.WallIconLibrary);
            ViewModes.Configure(settings.results, settings.minimap.enabled);
            Card.Configure(FindPoi, config);

            Voice?.Dispose();
            Voice = settings.voice.enabled
                ? new VoiceSearchController(TranscriberFactory.Create(Application.isEditor), SetQuery)
                : null;
            var indicator = new VoiceActivityIndicatorView(settings.voice.indicator_style);
            if (Voice != null) Voice.StateChanged += s => SearchBar.ShowVoiceState(indicator, s);
            SearchBar.Configure(settings.search, Voice != null && Voice.IsAvailable);

            _recent = new RecentSearchesManager(settings.results.recent_count);
            var suggestions = new SuggestedSearchesManager { Source = SuggestedSearchesManager.ParseSource(settings.results.suggestion_source) };
            SearchBar.SetSuggestions(settings.results.suggestions_enabled
                ? suggestions.BuildSuggestions(config, _recent)
                : new List<string>());

            Refresh();
        }

        private RecentSearchesManager _recent;

        private void BuildOnce()
        {
            if (_root != null) return;
            _document = GetComponent<UIDocument>();
            _root = new VisualElement { name = "search-ui", pickingMode = PickingMode.Ignore };
            _root.AddToClassList("search-ui");
            if (styleSheet != null) _root.styleSheets.Add(styleSheet);

            var top = new VisualElement { name = "search-top", pickingMode = PickingMode.Ignore };
            top.AddToClassList("search-top");
            var bottom = new VisualElement { name = "search-bottom", pickingMode = PickingMode.Ignore };
            bottom.AddToClassList("search-bottom");

            // - draw order = child order: the map under the bottom panels (the card's X stays tappable),
            //   the top bar (and its filter tray) over everything
            Minimap = new MinimapView(_root);
            _root.Add(bottom);
            _root.Add(top);

            SearchBar = new SearchOverlayView(top);
            ViewModes = new ViewModeControl(top);
            Tray = new FilterTrayView(top);
            List = new ResultsListView(bottom);
            Card = new DetailCardView(bottom);

            SearchBar.QueryChanged += q => { Query = q; Refresh(); };
            SearchBar.Submitted += q => _recent?.Add(q);
            SearchBar.FiltersPressed += () => Tray.SetOpen(!Tray.IsOpen);
            SearchBar.MapPressed += () => { MinimapToggledOpen = !MinimapToggledOpen; ApplyPanels(); };
            SearchBar.MicPressed += () => Voice?.StartVoiceSearch();
            Tray.Changed += Refresh;
            ViewModes.Changed += _ => ApplyPanels();

            _document.rootVisualElement.Add(_root);
            _root.RegisterCallback<GeometryChangedEvent>(_ => ApplySafeArea());
        }

        // Keep the whole search UI inside the device's safe area (notch, home indicator). The panels are
        // absolutely positioned, which parent padding does not move, so the root's own offsets take the
        // insets -- converted from screen pixels to panel units (the panel scales to its reference size).
        private void ApplySafeArea()
        {
            if (_root?.panel == null) return;
            var insets = SafeAreaHelper.GetCurrent();
            float scale = RuntimePanelUtils.ScreenToPanel(_root.panel, new Vector2(1000f, 0f)).x
                        - RuntimePanelUtils.ScreenToPanel(_root.panel, Vector2.zero).x;
            scale = scale > 0f ? scale / 1000f : 1f;
            SetOffset(_root.style.left, insets.left * scale, v => _root.style.left = v);
            SetOffset(_root.style.top, insets.top * scale, v => _root.style.top = v);
            SetOffset(_root.style.right, insets.right * scale, v => _root.style.right = v);
            SetOffset(_root.style.bottom, insets.bottom * scale, v => _root.style.bottom = v);
        }

        // - write only a changed value: a style write inside GeometryChanged would otherwise loop
        private static void SetOffset(StyleLength current, float value, System.Action<StyleLength> set)
        {
            if (current.keyword == StyleKeyword.Undefined && Mathf.Approximately(current.value.value, value)) return;
            set(value);
        }

        // Recompute the one result set and push it to every surface
        public void Refresh()
        {
            if (_root == null || wallSession == null) return;
            var settings = wallSession.SelectFilterSearch ?? new SelectFilterSearchSettings();
            State = ResultSetCoordinator.Compute(wallSession.SearchIndex, wallSession.SearchPois, Query, Tray.Selection, settings);

            wallSession.SelectionHighlight?.SetResultSet(State.Active ? State.Ids : null);
            Minimap.SetVisibleIds(State.Active ? State.Ids : null);
            SearchBar.SetFilterCount(Tray.Selection.Count);

            var rows = new List<ResultsListView.Row>();
            foreach (var r in State.Results)
                rows.Add(new ResultsListView.Row { PoiId = r.PoiId, Name = r.Poi.name, Subtitle = DetailCardView.Subtitle(r.Poi, wallSession.SearchConfig) });
            string relaxText = null;
            System.Action relaxAction = null;
            if (State.Relax.HasValue)
            {
                var s = State.Relax.Value;
                relaxText = ResultSetCoordinator.RelaxText(s, FilterTrayView.BuildOptions(wallSession.SearchConfig, settings.filter));
                relaxAction = () => Tray.SetFacet(s.Group, s.Key, false);
            }
            List.ShowRows(rows, State.EmptyMessage, relaxText, relaxAction);
            ApplyPanels();
        }

        private void ApplyPanels()
        {
            if (_root == null) return;
            var minimap = wallSession.SelectFilterSearch?.minimap ?? new MinimapSettings();
            Panels = SearchPanelsRule.Resolve(State.Active, ViewModes.Mode, SelectionEventBus.CurrentPoiId != null,
                minimap.enabled, minimap.visibility == SelectFilterSearchOptions.VisibilityAlways, MinimapToggledOpen);
            ViewModes.SetShown(Panels.ViewModes);
            List.SetShown(Panels.List);
            Minimap.SetShown(Panels.Minimap);
            Card.Root.style.display = Panels.Card ? DisplayStyle.Flex : DisplayStyle.None;
            SearchBar.SetMapButton(Panels.MinimapButton, Panels.Minimap);
        }

        private void OnSelectionChanged(string poiId)
        {
            // - picking a result while searching is the moment a query proved useful: remember it
            if (!string.IsNullOrEmpty(Query)) _recent?.Add(Query);
            ApplyPanels();
        }

        private void OnSelectionCleared() => ApplyPanels();

        // Every running marker's POI and where the marker really stands (its true place, before any
        // displacement nudge): the minimap draws what is on the wall, a demo's stage included
        private List<(POIData, Vector3)> MarkerPoints()
        {
            var points = new List<(POIData, Vector3)>();
            foreach (var marker in wallSession.SpawnedMarkers)
            {
                var poi = marker != null ? marker.GetComponentInParent<POIAnchor>()?.Data : null;
                if (poi != null) points.Add((poi, marker.UndisplacedWorldPosition));
            }
            return points;
        }

        private POIData FindPoi(string id)
        {
            var pois = wallSession.SearchPois;
            for (int i = 0; i < pois.Count; i++)
                if (pois[i].id == id) return pois[i];
            return null;
        }
    }
}

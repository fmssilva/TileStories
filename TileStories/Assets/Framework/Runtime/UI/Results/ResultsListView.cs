using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories
{
    // Scrollable list of search/filter results rendered via UI Toolkit.
    // Binds to POISearchIndex.Search() results, displays each match's name,
    // category, and relevance score, and raises SelectionEventBus events on tap
    // so the same selection pipeline serves list, minimap, and marker inputs.
    // (spec _2.6 section 9)
    public class ResultsListView : MonoBehaviour
    {
        private WallConfigData _config;
        private POISearchIndex _searchIndex;
        private UIDocument _uiDocument;
        private VisualElement _root;
        private ListView _listView;
        private Label _emptyStateLabel;
        private string _selectedPoiId = null;
        private bool _isEnabled = true;

        // One row in the results list.
        public class ResultRow
        {
            public string poiId;
            public string displayName;
            public string categoryLabel;
            public string summary;
            public float score;
        }

        // Initialise with wall config and search index.
        public void Initialize(WallConfigData config, POISearchIndex searchIndex)
        {
            _config = config;
            _searchIndex = searchIndex;

            if (_uiDocument == null)
            {
                _uiDocument = FindFirstObjectByType<UIDocument>();
                if (_uiDocument != null)
                {
                    _root = _uiDocument.rootVisualElement;
                    CreateUI(_root);
                }
            }

            if (_listView != null)
                RefreshResults("");
        }

        // Build the UI Toolkit list view and attach it to the root visual tree.
        private void CreateUI(VisualElement root)
        {
            _listView = new ListView()
            {
                name = "results-list-view",
                showAlternatingRowBackgrounds = AlternatingRowBackground.All,
                showBorder = false,
                reorderable = false,
            };

            _listView.style.flexGrow = 1;
            _listView.style.position = Position.Absolute;
            _listView.style.left = 12;
            _listView.style.right = 12;
            _listView.style.top = 80;
            _listView.style.bottom = 80;

            // Define the row template
            _listView.makeItem += MakeResultItem;
            _listView.bindItem += BindResultItem;
            _listView.selectionChanged += OnSelectionChanged;

            root.Add(_listView);

            // Empty state label
            _emptyStateLabel = new Label();
            _emptyStateLabel.name = "results-empty-state";
            _emptyStateLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            _emptyStateLabel.style.flexGrow = 1;
            _emptyStateLabel.style.fontSize = 14;
            _emptyStateLabel.style.color = new StyleColor(new Color(0.7f, 0.7f, 0.7f));
            _emptyStateLabel.style.display = DisplayStyle.None;
            root.Add(_emptyStateLabel);

            // Subscribe to selection events
            SelectionEventBus.OnMarkerSelected += OnExternalSelection;
            SelectionEventBus.OnSelectionCleared += OnExternalClear;
        }

        // Factory for each result row VisualElement.
        private VisualElement MakeResultItem()
        {
            var row = new VisualElement()
            {
                name = "result-row",
                tooltip = "Search result -- click to select",
            };
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.paddingLeft = 8;
            row.style.paddingRight = 8;
            row.style.paddingTop = 6;
            row.style.paddingBottom = 6;

            var nameLabel = new Label()
            {
                name = "result-name",
                pickingMode = PickingMode.Ignore,
            };
            nameLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
            nameLabel.style.flexGrow = 1;
            nameLabel.style.fontSize = 14;
            row.Add(nameLabel);

            var categoryLabel = new Label()
            {
                name = "result-category",
                pickingMode = PickingMode.Ignore,
            };
            categoryLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
            categoryLabel.style.fontSize = 12;
            categoryLabel.style.color = new StyleColor(new Color(0.7f, 0.7f, 0.7f));
            categoryLabel.style.marginLeft = 8;
            row.Add(categoryLabel);

            return row;
        }

        // Bind data to a result row VisualElement.
        private void BindResultItem(VisualElement element, int index)
        {
            if (index < 0 || _listView.itemsSource == null || index >= _listView.itemsSource.Count)
                return;

            var item = _listView.itemsSource[index] as ResultRow;
            if (item == null) return;

            element.tooltip = $"{item.displayName} ({item.categoryLabel})";

            var nameLabel = element.Q<Label>("result-name");
            var categoryLabel = element.Q<Label>("result-category");

            if (nameLabel != null)
                nameLabel.text = item.displayName;

            if (categoryLabel != null)
                categoryLabel.text = item.categoryLabel;

            // Highlight if this is the currently selected item
            bool isSelected = _selectedPoiId == item.poiId;
            element.style.backgroundColor = isSelected
                ? new StyleColor(new Color(0.3f, 0.5f, 0.8f, 0.2f))
                : new StyleColor(Color.clear);
        }

        // Search query changed -- rebuild the results list.
        // matchMode (default Any) is forwarded to POISearchIndex.Search so voice
        // search can enforce a stricter token-coverage policy than typed input
        // without the index needing to know who called it.
        public void RefreshResults(string query, SearchMatchMode matchMode = SearchMatchMode.Any)
        {
            if (_searchIndex == null || _listView == null)
                return;

            var results = _searchIndex.Search(query, matchMode);
            var rows = new List<ResultRow>();

            foreach (var result in results)
            {
                POIData poi = null;
                if (_config != null && result.POIIndex >= 0 && result.POIIndex < _config.pois.Count)
                    poi = _config.pois[result.POIIndex];

                string poiId = poi?.id ?? result.POIId;
                if (string.IsNullOrEmpty(poiId))
                    poiId = FindIdBySearchResult(result);

                rows.Add(new ResultRow
                {
                    poiId = poiId,
                    displayName = poi?.name ?? $"POI_{result.POIIndex}",
                    categoryLabel = string.IsNullOrEmpty(poi?.category) ? "" : poi.category,
                    summary = poi?.summary ?? "",
                    score = result.Score,
                });
            }

            _listView.itemsSource = rows;
            _listView.Rebuild();

            // Show/hide empty state
            if (rows.Count == 0)
            {
                ShowEmptyState(query);
            }
            else
            {
                HideEmptyState();
            }
        }

        // Look up POI id by matching SearchResult index to config.pois.
        private string FindIdBySearchResult(POISearchIndex.SearchResult result)
        {
            if (_config == null || _config.pois == null) return "";
            if (result.POIIndex >= 0 && result.POIIndex < _config.pois.Count)
                return _config.pois[result.POIIndex].id;
            return "";
        }

        private void ShowEmptyState(string query)
        {
            if (_emptyStateLabel == null) return;

            if (_config?.no_results_message != null)
            {
                string message = _config.no_results_message.Replace("{query}", query);
                _emptyStateLabel.text = message;
            }
            else
            {
                _emptyStateLabel.text = query.Length > 0
                    ? $"No matches for \"{query}\""
                    : "No search results";
            }

            _emptyStateLabel.style.display = DisplayStyle.Flex;
            _listView.style.display = DisplayStyle.None;
        }

        private void HideEmptyState()
        {
            if (_emptyStateLabel != null)
                _emptyStateLabel.style.display = DisplayStyle.None;
            if (_listView != null)
                _listView.style.display = DisplayStyle.Flex;
        }

        // Called when a row is selected in the list view.
        private void OnSelectionChanged(IEnumerable<object> selectedItems)
        {
            if (!_isEnabled || _searchIndex == null) return;

            foreach (var item in selectedItems)
            {
                if (item is ResultRow row)
                {
                    SelectionEventBus.RaiseMarkerSelected(row.poiId);
                    break;
                }
            }
        }

        // Called when a selection happens externally (marker tap or minimap tap).
        private void OnExternalSelection(string poiId)
        {
            _selectedPoiId = poiId;
            RefreshSelectionHighlight();
        }

        private void OnExternalClear()
        {
            _selectedPoiId = null;
            RefreshSelectionHighlight();
        }

        private void RefreshSelectionHighlight()
        {
            if (_listView?.itemsSource == null) return;
            _listView.Rebuild();
        }

        // Update the results list visibility.
        public void SetVisible(bool visible)
        {
            _isEnabled = visible;
            if (_root != null)
                _root.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        // Update when wall config changes.
        public void Refresh(WallConfigData newConfig)
        {
            _config = newConfig;
            RefreshResults("");
        }

        private void OnDestroy()
        {
            SelectionEventBus.OnMarkerSelected -= OnExternalSelection;
            SelectionEventBus.OnSelectionCleared -= OnExternalClear;
        }
    }
}

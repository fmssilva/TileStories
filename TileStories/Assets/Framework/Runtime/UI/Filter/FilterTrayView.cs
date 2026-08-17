using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories
{
    // Facet filter tray: collapsible sections for category, badge category,
    // outline/status level, and hierarchy level. Toggling a facet immediately
    // updates the visible result set (NN/G's "faceted-search-with-a-tray" pattern).
    // When zero results remain, computes which single facet removal would yield
    // the most results and offers it as a one-tap "relax filters" action.
    // (spec _2.6 section 7)
    public class FilterTrayView : MonoBehaviour
    {
        private WallConfigData _config;
        private UIDocument _uiDocument;
        private VisualElement _root;
        private VisualElement _trayContainer;
        private Label _emptyStateLabel;

        // Currently active filter values per facet type
        private readonly HashSet<string> _activeCategories = new();
        private readonly HashSet<string> _activeBadgeCategories = new();
        private readonly HashSet<string> _activeOutlineLevels = new();
        private readonly HashSet<string> _activeHierarchyLevels = new();

        // Event raised when filters change so ResultsListView can refresh
        public event Action OnFiltersChanged;

        // Initialise with wall config
        public void Initialize(WallConfigData config)
        {
            _config = config;

            if (_uiDocument == null)
            {
                _uiDocument = FindFirstObjectByType<UIDocument>();
                if (_uiDocument != null)
                {
                    _root = _uiDocument.rootVisualElement;
                    CreateUI(_root);
                }
            }

            if (_trayContainer != null)
                RefreshAllFacets();
        }

        // Build the filter tray UI with collapsible sections
        private void CreateUI(VisualElement root)
        {
            _trayContainer = new VisualElement()
            {
                name = "filter-tray-container",
            };
            _trayContainer.style.position = Position.Absolute;
            _trayContainer.style.top = 80;
            _trayContainer.style.left = 12;
            _trayContainer.style.right = 12;
            _trayContainer.style.bottom = 12;

            _emptyStateLabel = new Label();
            _emptyStateLabel.name = "filter-empty-state";
            _emptyStateLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            _emptyStateLabel.style.flexGrow = 1;
            _emptyStateLabel.style.fontSize = 14;
            _emptyStateLabel.style.color = new StyleColor(new Color(0.7f, 0.7f, 0.7f));
            _emptyStateLabel.style.display = DisplayStyle.None;
            _trayContainer.Add(_emptyStateLabel);

            root.Add(_trayContainer);
        }

        // Rebuild all facet sections from the wall config taxonomy
        private void RefreshAllFacets()
        {
            if (_trayContainer == null || _config == null)
                return;

            // Clear existing facet sections (keep the empty state label)
            var toRemove = new List<VisualElement>();
            foreach (var child in _trayContainer.Children())
            {
                if (child is VisualElement ve && ve.name != "filter-empty-state")
                    toRemove.Add(ve);
            }
            foreach (var child in toRemove)
                child.RemoveFromHierarchy();

            // Category facet (CategoryStyleEntry uses .category field)
            AddFacetSection<CategoryStyleEntry>("Categories", _config.category_styles,
                entry => entry.category, "category", _activeCategories);

            // Badge category facet (BadgeCategoryEntry uses .key field)
            AddFacetSection<BadgeCategoryEntry>("Badges", _config.badge_categories,
                entry => entry.key, "badge", _activeBadgeCategories);

            // Outline/status level facet (OutlineLevelEntry uses .key field)
            AddFacetSection<OutlineLevelEntry>("Status Levels", _config.outline_levels,
                entry => entry.key, "status", _activeOutlineLevels);

            // Hierarchy level facet (HierarchyLevelEntry uses .key field)
            AddFacetSection<HierarchyLevelEntry>("Hierarchy Levels", _config.hierarchy_levels,
                entry => entry.key, "hierarchy", _activeHierarchyLevels);
        }

        // Generic facet section builder for any taxonomy table
        private void AddFacetSection<T>(string title, List<T> entries, Func<T, string> keySelector,
            string facetType, HashSet<string> activeSet)
        {
            if (entries == null || entries.Count == 0)
                return;

            var section = new VisualElement()
            {
                name = $"facet-section-{facetType}",
            };
            section.style.marginBottom = 8;

            var header = new Label(title);
            header.style.unityFontStyleAndWeight = FontStyle.Bold;
            header.style.fontSize = 13;
            header.style.paddingLeft = 4;
            header.style.paddingBottom = 4;
            section.Add(header);

            foreach (var entry in entries)
            {
                string key = keySelector(entry);
                if (string.IsNullOrEmpty(key))
                    continue;

                var toggle = new Toggle(key);
                toggle.name = "facet-toggle-" + key;
                toggle.tooltip = $"Toggle {key} {facetType} filter";
                toggle.style.minHeight = 44; // WCAG 2.5.5: >=44px tap target
                toggle.value = activeSet.Contains(key);
                toggle.RegisterValueChangedCallback(evt =>
                {
                    if (evt.newValue)
                        activeSet.Add(key);
                    else
                        activeSet.Remove(key);

                    OnFacetToggled();
                });

                section.Add(toggle);
            }

            _trayContainer.Add(section);
        }

        // Called when any facet toggle changes
        private void OnFacetToggled()
        {
            OnFiltersChanged?.Invoke();
        }

        // Get all active category filters
        public List<string> GetActiveCategories() => new List<string>(_activeCategories);

        // Get all active badge category filters
        public List<string> GetActiveBadgeCategories() => new List<string>(_activeBadgeCategories);

        // Get all active outline level filters
        public List<string> GetActiveOutlineLevels() => new List<string>(_activeOutlineLevels);

        // Get all active hierarchy level filters
        public List<string> GetActiveHierarchyLevels() => new List<string>(_activeHierarchyLevels);

        // Compute the "relax filters" suggestion when zero results.
        // Delegates to FilterFacetEvaluator for the pure-logic computation.
        public string ComputeRelaxSuggestion()
        {
            if (_config?.pois == null) return null;
            return FilterFacetEvaluator.ComputeRelaxSuggestion(
                _config.pois, _activeCategories, _activeBadgeCategories,
                _activeOutlineLevels, _activeHierarchyLevels);
        }

        // Check if any filters are currently active
        public bool HasActiveFilters()
        {
            return _activeCategories.Count > 0 || _activeBadgeCategories.Count > 0 ||
                   _activeOutlineLevels.Count > 0 || _activeHierarchyLevels.Count > 0;
        }

        // Clear all active filters
        public void ClearAllFilters()
        {
            _activeCategories.Clear();
            _activeBadgeCategories.Clear();
            _activeOutlineLevels.Clear();
            _activeHierarchyLevels.Clear();
            RefreshAllFacets();
            OnFiltersChanged?.Invoke();
        }

        // Show/hide the filter tray
        public void SetVisible(bool visible)
        {
            if (_trayContainer != null)
                _trayContainer.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        // Update when wall config changes
        public void Refresh(WallConfigData newConfig)
        {
            _config = newConfig;
            RefreshAllFacets();
        }

        private void OnDestroy()
        {
            OnFiltersChanged = null;
        }
    }
}

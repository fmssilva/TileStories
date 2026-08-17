using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories
{
    // Segmented control for switching between result views:
    // - List: shows ResultsListView as a scrollable list of search results
    // - Minimap: shows MinimapView overlay with POI dots
    // - Highlight: dims non-selected markers, keeps AR view prominent
    // Persists user's preferred view mode to PlayerPrefs.
    // (spec _2.6 section 10)
    public class ViewModeControl : MonoBehaviour
    {
        private const string PREF_KEY = "TileStories.default_result_view";

        private WallConfigData _config;
        private UIDocument _uiDocument;
        private VisualElement _root;
        private VisualElement _segmentedControl;
        private MinimapView _minimapView;
        private ResultsListView _resultsListView;
        private FilterTrayView _filterTrayView;

        // Current active view mode
        private ViewMode _currentMode = ViewMode.List;

        public enum ViewMode
        {
            List,
            Minimap,
            CameraHighlight
        }

        // Event raised when the view mode changes
        public event Action<ViewMode> OnViewModeChanged;

        // Initialise with wall config and references to the view components
        public void Initialize(WallConfigData config, UIDocument uiDocument,
            MinimapView minimapView, ResultsListView resultsListView,
            FilterTrayView filterTrayView)
        {
            _config = config;
            _uiDocument = uiDocument;
            _minimapView = minimapView;
            _resultsListView = resultsListView;
            _filterTrayView = filterTrayView;

            if (_uiDocument != null)
            {
                _root = _uiDocument.rootVisualElement;
                CreateUI(_root);
            }

            // Load or default the view mode
            string defaultMode = config?.default_result_view ?? "list";
            string savedMode = PlayerPrefs.GetString(PREF_KEY, defaultMode);
            ApplyViewMode(ViewModeParser.Parse(savedMode), instant: true);
        }

        // Build the segmented control UI
        private void CreateUI(VisualElement root)
        {
            _segmentedControl = new VisualElement()
            {
                name = "view-mode-control",
            };
            _segmentedControl.style.position = Position.Absolute;
            _segmentedControl.style.top = 12;
            _segmentedControl.style.left = 12;
            _segmentedControl.style.right = 12;
            _segmentedControl.style.height = 36;
            _segmentedControl.style.flexDirection = FlexDirection.Row;
            _segmentedControl.style.justifyContent = Justify.Center;
            _segmentedControl.style.borderLeftWidth = 1;
            _segmentedControl.style.borderRightWidth = 1;
            _segmentedControl.style.borderTopWidth = 1;
            _segmentedControl.style.borderBottomWidth = 1;
            _segmentedControl.style.borderLeftColor = new StyleColor(new Color(1f, 1f, 1f, 0.2f));
            _segmentedControl.style.borderRightColor = new StyleColor(new Color(1f, 1f, 1f, 0.2f));
            _segmentedControl.style.borderTopColor = new StyleColor(new Color(1f, 1f, 1f, 0.2f));
            _segmentedControl.style.borderBottomColor = new StyleColor(new Color(1f, 1f, 1f, 0.2f));

            AddSegmentButton("List", ViewMode.List);
            AddSegmentButton("Minimap", ViewMode.Minimap);
            AddSegmentButton("Highlight", ViewMode.CameraHighlight);

            root.Add(_segmentedControl);
        }

        // Add a single button to the segmented control
        private void AddSegmentButton(string label, ViewMode mode)
        {
            var button = new Button(() => OnSegmentClicked(mode))
            {
                text = label,
                name = $"view-mode-{mode}",
            };
            UIAccessibility.SetRoleAndLabel(button, "button", label + " view");
            button.style.minWidth = 100;
            button.style.minHeight = 34;
            button.style.unityTextAlign = TextAnchor.MiddleCenter;
            button.style.fontSize = 13;
            button.style.unityFontStyleAndWeight = FontStyle.Bold;
            button.userData = mode;
            UpdateSegmentButtonVisual(button, mode == _currentMode);

            _segmentedControl.Add(button);
        }

        // Handle a segment button click
        private void OnSegmentClicked(ViewMode mode)
        {
            ApplyViewMode(mode);
        }

        // Apply the selected view mode, updating all view components
        private void ApplyViewMode(ViewMode mode, bool instant = false)
        {
            if (_currentMode == mode && !instant)
                return;

            _currentMode = mode;

            // Update segment button visuals
            if (_segmentedControl != null)
            {
                foreach (var child in _segmentedControl.Children())
                {
                    if (child is Button button)
                    {
                        ViewMode buttonMode = (ViewMode)button.userData;
                        UpdateSegmentButtonVisual(button, buttonMode == mode);
                    }
                }
            }

            // Show/hide the appropriate view components
            if (_minimapView != null)
                _minimapView.SetVisible(mode == ViewMode.Minimap);

            if (_resultsListView != null)
                _resultsListView.SetVisible(mode == ViewMode.List);

            if (_filterTrayView != null)
                _filterTrayView.SetVisible(mode == ViewMode.List || mode == ViewMode.Minimap);

            // In CameraHighlight mode, SelectionHighlightController handles dimming
            // (already wired in Block 2)

            // Persist preference
            PlayerPrefs.SetString(PREF_KEY, ViewModeParser.ToString(mode));
            PlayerPrefs.Save();

            OnViewModeChanged?.Invoke(mode);
        }

        // Update a segment button's appearance based on selected state
        private void UpdateSegmentButtonVisual(Button button, bool selected)
        {
            if (selected)
            {
                button.style.backgroundColor = new StyleColor(new Color(0.3f, 0.5f, 0.8f, 0.8f));
                button.style.color = new StyleColor(Color.white);
            }
            else
            {
                button.style.backgroundColor = new StyleColor(Color.clear);
                button.style.color = new StyleColor(new Color(0.8f, 0.8f, 0.8f));
            }
        }

        // Get the current view mode
        public ViewMode GetCurrentMode() => _currentMode;

        // Update when wall config changes
        public void Refresh(WallConfigData newConfig)
        {
            _config = newConfig;
        }

        private void OnDestroy()
        {
            OnViewModeChanged = null;
        }
    }
}

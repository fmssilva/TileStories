using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories
{
    // 2D minimap overlay that renders POI positions as dots on a flat panel.
    // Deliberately not a second RenderTexture camera -- this project already has
    // normalized wall coordinates (POIData.x_norm / y_norm), so the minimap is
    // a scatter-plot of positions already in the schema (spec _2.6 section 8).
    // Tap a dot -> raises the same selection event as tapping the real marker
    // (SelectionEventBus), so there is one selection system with two input surfaces.
    public class MinimapView : MonoBehaviour
    {
        private const float DOT_SIZE_PX = 20f;
        private const float SELECTED_DOT_SCALE = 1.5f;
        private const float DIM_ALPHA = 0.3f;
        private WallConfigData _config;
        private POISearchIndex _searchIndex;
        private UIDocument _uiDocument;
        private VisualElement _container;
        private VisualElement _background;
        private float _width = 200f;
        private float _height = 200f;
        private bool _isEnabled = true;

        // Maps POI id to its dot VisualElement for lookup on tap.
        private readonly Dictionary<string, VisualElement> _dots = new();

        // Initialise with wall config and search index.
        public void Initialize(WallConfigData config, POISearchIndex searchIndex)
        {
            _config = config;
            _searchIndex = searchIndex;
            CategoryPalette.Configure(config?.category_styles);

            if (_uiDocument == null)
            {
                _uiDocument = FindFirstObjectByType<UIDocument>();
                if (_uiDocument != null)
                    CreateUI(_uiDocument.rootVisualElement);
            }

            if (_background != null)
                RefreshDots();
        }

        // Build the minimap background container and subscribe to events.
        private void CreateUI(VisualElement root)
        {
            _container = new VisualElement()
            {
                name = "minimap-container",
            };
            _container.style.position = Position.Absolute;
            _container.style.top = 12;
            _container.style.right = 12;
            _container.style.width = 200;
            _container.style.height = 200;

            _background = new VisualElement()
            {
                name = "minimap-background",
            };
            _background.style.borderLeftWidth = 1;
            _background.style.borderRightWidth = 1;
            _background.style.borderTopWidth = 1;
            _background.style.borderBottomWidth = 1;
            _background.style.borderLeftColor = new StyleColor(Color.white);
            _background.style.borderRightColor = new StyleColor(Color.white);
            _background.style.borderTopColor = new StyleColor(Color.white);
            _background.style.borderBottomColor = new StyleColor(Color.white);
            _background.style.backgroundColor = new StyleColor(new Color(0f, 0f, 0f, 0.5f));
            _background.style.position = Position.Absolute;
            _background.style.top = 0;
            _background.style.left = 0;
            _background.style.right = 0;
            _background.style.bottom = 0;
            _background.style.borderTopLeftRadius = 8;
            _background.style.borderTopRightRadius = 8;
            _background.style.borderBottomLeftRadius = 8;
            _background.style.borderBottomRightRadius = 8;

            _container.Add(_background);
            root.Add(_container);

            SelectionEventBus.OnMarkerSelected += OnSelectionChanged;
            SelectionEventBus.OnSelectionCleared += OnSelectionCleared;
        }

        // Render each POI as a dot positioned by normalized wall coordinates.
        private void RefreshDots()
        {
            if (_config == null || _config.pois == null || _background == null)
                return;

            // Clear existing dots
            foreach (var dot in _dots.Values)
                dot.RemoveFromHierarchy();
            _dots.Clear();

            foreach (var poi in _config.pois)
            {
                if (string.IsNullOrEmpty(poi.id))
                    continue;

                var dot = CreateDot(poi);
                _background.Add(dot);
                _dots[poi.id] = dot;
            }
        }

        // Create a visual dot element for a POI.
        private VisualElement CreateDot(POIData poi)
        {
            var dot = new VisualElement();
            dot.name = $"minimap-dot-{poi.id}";
            dot.tooltip = $"{poi.name} ({poi.category})";
            dot.style.position = Position.Absolute;
            dot.style.width = DOT_SIZE_PX;
            dot.style.height = DOT_SIZE_PX;
            dot.style.borderTopLeftRadius = DOT_SIZE_PX / 2f;
            dot.style.borderTopRightRadius = DOT_SIZE_PX / 2f;
            dot.style.borderBottomLeftRadius = DOT_SIZE_PX / 2f;
            dot.style.borderBottomRightRadius = DOT_SIZE_PX / 2f;
            dot.userData = poi.id;

            // Use the coordinate converter for position
            Vector2 pos = MinimapCoordinateConverter.ConvertToPixel(
                MinimapCoordinateConverter.ClampNorm(poi.x_norm),
                MinimapCoordinateConverter.ClampNorm(poi.y_norm),
                _width, _height, DOT_SIZE_PX);

            dot.style.left = pos.x;
            dot.style.top = pos.y;

            Color dotColor = ResolveDotColor(poi);
            dot.style.backgroundColor = new StyleColor(dotColor);

            // Tap handler -- same selection path as real markers
            dot.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (!_isEnabled) return;
                evt.StopPropagation();
                SelectionEventBus.RaiseMarkerSelected(poi.id);
            });

            return dot;
        }

        // Resolve the dot color based on minimap icon style and POI data.
        private Color ResolveDotColor(POIData poi)
        {
            string iconStyle = _config?.minimap_icon_style ?? "category_colored_dots";

            if (iconStyle == "dots_only")
                return Color.white;

            // category_colored_dots: use the category palette color
            if (_config != null && !string.IsNullOrEmpty(poi.category))
                return CategoryPalette.ResolveColor(poi.category);

            return Color.white;
        }

        // Update the minimap visibility based on config setting.
        public void SetVisible(bool visible)
        {
            _isEnabled = visible;
            if (_container != null)
                _container.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        // Highlight the selected POI dot, dim all others.
        private void OnSelectionChanged(string poiId)
        {
            if (_background == null) return;

            foreach (var kvp in _dots)
            {
                if (kvp.Key == poiId)
                {
                    kvp.Value.style.scale = new StyleScale(new Vector2(SELECTED_DOT_SCALE, SELECTED_DOT_SCALE));
                    kvp.Value.style.opacity = 1f;
                }
                else
                {
                    kvp.Value.style.scale = new StyleScale(Vector2.one);
                    kvp.Value.style.opacity = new StyleFloat(DIM_ALPHA);
                }
            }
        }

        private void OnSelectionCleared()
        {
            if (_background == null) return;

            foreach (var dot in _dots.Values)
            {
                dot.style.scale = new StyleScale(Vector2.one);
                dot.style.opacity = 1f;
            }
        }

        // Update minimap when the wall config changes (e.g. after a new wall loads).
        public void Refresh(WallConfigData newConfig)
        {
            _config = newConfig;
            CategoryPalette.Configure(_config?.category_styles);

            // Rebuild dots with new config
            foreach (var dot in _dots.Values)
                dot.RemoveFromHierarchy();
            _dots.Clear();

            if (_background != null)
                RefreshDots();
        }

        private void OnDestroy()
        {
            SelectionEventBus.OnMarkerSelected -= OnSelectionChanged;
            SelectionEventBus.OnSelectionCleared -= OnSelectionCleared;
        }
    }
}

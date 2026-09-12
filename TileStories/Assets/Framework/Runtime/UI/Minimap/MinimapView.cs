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
        // Fallbacks used only when no wall config is loaded yet; the authored
        // values live on WallConfigData (developer-tunable in the Editor Tab).
        private const float DEFAULT_DOT_SIZE_PX = 20f;
        private const float DEFAULT_TAP_TARGET_PX = 44f;
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

        // Maps POI id to its visible dot element (highlight scaling/dimming target).
        private readonly Dictionary<string, VisualElement> _dots = new();

        // Maps POI id to its invisible hit-zone container (removal on refresh).
        private readonly Dictionary<string, VisualElement> _hitZones = new();

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
        // Internal (not private) so the EditMode accessibility suite can build the
        // real UI and assert authored hit-zone sizes (Runtime grants
        // InternalsVisibleTo the editor test assembly -- same seam as DetailCardView).
        internal void CreateUI(VisualElement root)
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

            // One background-level tap handler routes every dot tap by nearest-dot-center
            // (spec _2.6 section 8 + _2.7 Decision 3.3): adjacent 44px hit-zones overlap at
            // high density by design -- nearest center resolves the ambiguity deterministically.
            _background.RegisterCallback<MouseDownEvent>(OnBackgroundTap);

            SelectionEventBus.OnMarkerSelected += OnSelectionChanged;
            SelectionEventBus.OnSelectionCleared += OnSelectionCleared;
        }

        // Visual dot diameter from config (developer-tunable), with sane floors.
        private float DotSizePx => Mathf.Max(4f, _config != null ? _config.minimap_dot_size_px : DEFAULT_DOT_SIZE_PX);

        // Invisible hit-zone diameter; never smaller than the visual dot itself.
        private float TapTargetPx => Mathf.Max(DotSizePx,
            Mathf.Max(4f, _config != null ? _config.minimap_dot_tap_target_px : DEFAULT_TAP_TARGET_PX));

        // Route a tap on the minimap background to the POI whose DOT CENTER is nearest
        // to the tap point. Hit-zones may overlap at density; nearest-center wins.
        private void OnBackgroundTap(MouseDownEvent evt)
        {
            if (!_isEnabled || _dots.Count == 0)
                return;

            string nearestId = null;
            float nearestDistSq = float.MaxValue;
            foreach (var kvp in _dots)
            {
                if (kvp.Value == null)
                    continue;
                float distSq = (kvp.Value.worldBound.center - evt.mousePosition).sqrMagnitude;
                if (distSq < nearestDistSq)
                {
                    nearestDistSq = distSq;
                    nearestId = kvp.Key;
                }
            }

            if (nearestId == null)
                return;
            evt.StopPropagation();
            SelectionEventBus.RaiseMarkerSelected(nearestId);
        }

        // Render each POI as a dot positioned by normalized wall coordinates.
        private void RefreshDots()
        {
            if (_config == null || _config.pois == null || _background == null)
                return;

            // Clear existing dots (remove the hit-zone containers; the visual
            // dot children come with them).
            foreach (var zone in _hitZones.Values)
                zone.RemoveFromHierarchy();
            _hitZones.Clear();
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

        // Create one POI marker on the minimap: a small VISUAL dot centred inside a larger
        // INVISIBLE tap-catching zone (_2.7 Decision 3.3). Visual size and touch target are
        // separate concerns -- compact look, WCAG-sized interaction -- and both come from
        // WallConfigData so each wall tunes density vs. reachability.
        private VisualElement CreateDot(POIData poi)
        {
            float dotSize = DotSizePx;
            float tapSize = TapTargetPx;

            // The container is the hit target: transparent, sized by minimap_dot_tap_target_px.
            var hit = new VisualElement();
            hit.name = $"minimap-hit-{poi.id}";
            hit.tooltip = $"{poi.name} ({poi.category})";
            hit.style.position = Position.Absolute;
            hit.style.width = tapSize;
            hit.style.height = tapSize;

            // Use captured_position and wall_bounds for position.
            // Fallback to (0.5, 0.5) if no captured position or bounds available.
            Vector2 normPos = new Vector2(0.5f, 0.5f);
            if (poi.has_captured_position && poi.captured_position != null && _config != null && _config.wall_bounds != null && _config.wall_bounds.IsValid())
            {
                Vector3 worldPos = new Vector3(poi.captured_position.x, poi.captured_position.y, poi.captured_position.z);
                normPos = _config.wall_bounds.WorldToNormalized(worldPos);
            }

            // Use the coordinate converter for position (centres the hit zone on the POI).
            Vector2 pos = MinimapCoordinateConverter.ConvertToPixel(
                MinimapCoordinateConverter.ClampNorm(normPos.x),
                MinimapCoordinateConverter.ClampNorm(normPos.y),
                _width, _height, tapSize);
            hit.style.left = pos.x;
            hit.style.top = pos.y;

            // The visible dot, centred inside the hit zone.
            var dot = new VisualElement();
            dot.name = $"minimap-dot-{poi.id}";
            dot.userData = poi.id;
            dot.style.position = Position.Absolute;
            dot.style.width = dotSize;
            dot.style.height = dotSize;
            dot.style.left = (tapSize - dotSize) / 2f;
            dot.style.top = (tapSize - dotSize) / 2f;
            dot.style.borderTopLeftRadius = dotSize / 2f;
            dot.style.borderTopRightRadius = dotSize / 2f;
            dot.style.borderBottomLeftRadius = dotSize / 2f;
            dot.style.borderBottomRightRadius = dotSize / 2f;

            Color dotColor = ResolveDotColor(poi);
            dot.style.backgroundColor = new StyleColor(dotColor);

            hit.Add(dot);

            // Selection highlight scales/dims this element (OnSelectionChanged).
            _dots[poi.id] = dot;
            _hitZones[poi.id] = hit;
            return hit;
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

        // Show/hide the minimap visibility based on config setting.
        public void SetVisible(bool visible)
        {
            _isEnabled = visible;
            if (_container != null)
                _container.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        // Filter the rendered dots to the active facet's passing-id set (_2.6-i):
        // non-matching dots (and their hit zones) are hidden; null/empty set shows all.
        public void SetFilterCandidateIds(ICollection<string> candidateIds)
        {
            bool unrestricted = candidateIds == null || candidateIds.Count == 0;
            foreach (var kvp in _hitZones)
            {
                if (kvp.Value == null) continue;
                bool show = unrestricted || candidateIds.Contains(kvp.Key);
                kvp.Value.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
            }
            foreach (var kvp in _dots)
            {
                if (kvp.Value == null) continue;
                bool show = unrestricted || candidateIds.Contains(kvp.Key);
                kvp.Value.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
            }
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
            foreach (var zone in _hitZones.Values)
                zone.RemoveFromHierarchy();
            _hitZones.Clear();
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

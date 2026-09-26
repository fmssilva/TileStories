using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories
{
    // The minimap (spec _2.6 section 8): a flat overview of every POI, one dot per POI placed by
    // MinimapLayout at where its marker really stands -- a UI element, not a second camera. Dots follow the result set (outside it they are
    // hidden) and the selection (the selected dot grows, the others fade). Tapping the map selects the
    // POI whose dot centre is nearest, so overlapping hit zones in a crowd resolve deterministically.
    // Plain C#: built into the parent the search UI hands it; styled by SearchUI.uss.
    public sealed class MinimapView : IDisposable
    {
        public const float SelectedScale = 1.5f;
        public const float FadedOpacity = 0.35f;

        public VisualElement Root { get; }
        private readonly VisualElement _map;
        private readonly Dictionary<string, VisualElement> _dots = new();
        private readonly Dictionary<string, Vector2> _positions = new();
        private HashSet<string> _visibleIds;

        public MinimapView(VisualElement parent)
        {
            Root = new VisualElement { name = "minimap" };
            Root.AddToClassList("search-panel");
            Root.AddToClassList("minimap");
            _map = new VisualElement { name = "minimap-map" };
            _map.AddToClassList("minimap-map");
            _map.RegisterCallback<ClickEvent>(evt =>
            {
                var size = _map.contentRect.size;
                if (size.x <= 0f || size.y <= 0f) return;
                Vector2 local = _map.WorldToLocal(evt.position);
                TapAt(new Vector2(local.x / size.x, 1f - local.y / size.y));
            });
            Root.Add(_map);
            parent.Add(Root);

            SelectionEventBus.OnMarkerSelected += OnSelectionChanged;
            SelectionEventBus.OnSelectionCleared += OnSelectionCleared;
        }

        public bool IsShown => Root.style.display != DisplayStyle.None;
        public void SetShown(bool shown) => Root.style.display = shown ? DisplayStyle.Flex : DisplayStyle.None;

        // Every dot's map position (0..1, bottom-left origin), for tests and the readout
        public IReadOnlyDictionary<string, Vector2> Positions => _positions;

        // The dot element of a POI (null if it has none)
        public VisualElement DotOf(string poiId) => _dots.TryGetValue(poiId, out var d) ? d : null;

        // Whether a POI's dot is on the map right now (inside the result set, or no result set)
        public bool IsDotShown(string poiId) => _dots.ContainsKey(poiId) && (_visibleIds == null || _visibleIds.Contains(poiId));

        // Rebuild every dot: one per POI at its world position; `icons` is only used for the mini_icons style
        public void Rebuild(IReadOnlyList<(POIData poi, Vector3 world)> points, MinimapSettings settings, SpriteKeyLibrary icons)
        {
            _map.Clear();
            _dots.Clear();
            _positions.Clear();
            settings ??= new MinimapSettings();

            var placed = new List<POIData>();
            var worlds = new List<Vector3>();
            if (points != null)
                foreach (var (poi, world) in points)
                    if (poi != null && !string.IsNullOrEmpty(poi.id))
                    {
                        placed.Add(poi);
                        worlds.Add(world);
                    }

            var layout = MinimapLayout.Compute(worlds, settings);
            float tap = Mathf.Clamp(settings.tap_target_px, MinimapSettings.TapTargetMin, MinimapSettings.TapTargetMax);
            float dot = Mathf.Clamp(settings.dot_size_px, MinimapSettings.DotSizeMin, Mathf.Min(MinimapSettings.DotSizeMax, tap));
            for (int i = 0; i < placed.Count; i++)
            {
                var poi = placed[i];
                Vector2 p = layout.Normalize(worlds[i]);
                _positions[poi.id] = p;
                _dots[poi.id] = AddDot(poi, p, dot, tap, settings.icon_style, icons);
            }
            ApplyVisibility();
            ApplySelection(SelectionEventBus.CurrentPoiId);
        }

        // Show only these POIs' dots (null = every dot)
        public void SetVisibleIds(ICollection<string> ids)
        {
            _visibleIds = ids == null ? null : new HashSet<string>(ids);
            ApplyVisibility();
        }

        // A tap at a map position (0..1, bottom-left origin): selects the nearest shown dot
        public void TapAt(Vector2 normalized)
        {
            string nearest = null;
            float best = float.MaxValue;
            foreach (var kvp in _positions)
            {
                if (!IsDotShown(kvp.Key)) continue;
                float d = (kvp.Value - normalized).sqrMagnitude;
                if (d < best) { best = d; nearest = kvp.Key; }
            }
            if (nearest != null) SelectionEventBus.Select(nearest);
        }

        // One hit zone (the tap target, invisible) centred on the POI, with the visible dot inside it
        private VisualElement AddDot(POIData poi, Vector2 p, float dotPx, float tapPx, string iconStyle, SpriteKeyLibrary icons)
        {
            var hit = new VisualElement { name = "minimap-hit-" + poi.id, tooltip = poi.name, pickingMode = PickingMode.Ignore };
            hit.AddToClassList("minimap-hit");
            hit.style.left = Length.Percent(p.x * 100f);
            hit.style.top = Length.Percent((1f - p.y) * 100f);
            hit.style.width = tapPx;
            hit.style.height = tapPx;
            hit.style.translate = new Translate(Length.Percent(-50f), Length.Percent(-50f));

            var dot = new VisualElement { name = "minimap-dot-" + poi.id, pickingMode = PickingMode.Ignore };
            dot.AddToClassList("minimap-dot");
            dot.style.width = dotPx;
            dot.style.height = dotPx;
            dot.style.borderTopLeftRadius = dot.style.borderTopRightRadius =
                dot.style.borderBottomLeftRadius = dot.style.borderBottomRightRadius = dotPx * 0.5f;

            // - the dot's colour and icon are the POI's own data (its category), not a design value
            if (iconStyle != SelectFilterSearchOptions.IconDotsOnly)
                dot.style.backgroundColor = CategoryPalette.ResolveColor(poi.category);
            if (iconStyle == SelectFilterSearchOptions.IconMini)
            {
                dot.AddToClassList("minimap-dot--icon");
                Sprite sprite = icons != null ? icons.Get(CategoryPalette.ResolveIconKey(poi.category)) : null;
                if (sprite != null)
                {
                    var icon = new VisualElement { name = "minimap-icon-" + poi.id, pickingMode = PickingMode.Ignore };
                    icon.AddToClassList("minimap-icon");
                    icon.style.backgroundImage = new StyleBackground(sprite);
                    dot.Add(icon);
                }
            }

            hit.Add(dot);
            _map.Add(hit);
            return dot;
        }

        private void ApplyVisibility()
        {
            foreach (var kvp in _dots)
                kvp.Value.parent.style.display = IsDotShown(kvp.Key) ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void ApplySelection(string selectedId)
        {
            foreach (var kvp in _dots)
            {
                bool isSelected = kvp.Key == selectedId;
                kvp.Value.style.scale = new Scale(Vector2.one * (isSelected ? SelectedScale : 1f));
                kvp.Value.style.opacity = selectedId == null || isSelected ? 1f : FadedOpacity;
            }
        }

        private void OnSelectionChanged(string poiId) => ApplySelection(poiId);
        private void OnSelectionCleared() => ApplySelection(null);

        public void Dispose()
        {
            SelectionEventBus.OnMarkerSelected -= OnSelectionChanged;
            SelectionEventBus.OnSelectionCleared -= OnSelectionCleared;
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace TileStories
{
    // Minimal detail card (spec _2.6 section 14): proves selection -> detail end to end, nothing more.
    // Shows the selected POI's name, "category - level" and summary; the X clears the selection through
    // SelectionEventBus, so markers, list and minimap all return to normal together. The future content
    // card (blocks, scroll, animation) is another domain's work.
    // Plain C#: built into the parent the search UI hands it; styled by SearchUI.uss.
    public sealed class DetailCardView : IDisposable
    {
        public VisualElement Root { get; }
        private readonly Label _name;
        private readonly Label _subtitle;
        private readonly Label _summary;
        private Func<string, POIData> _findPoi;
        private WallConfigData _taxonomy;

        public DetailCardView(VisualElement parent)
        {
            Root = new VisualElement { name = "detail-card" };
            Root.AddToClassList("search-panel");
            Root.AddToClassList("detail-card");

            var close = new Button(() => SelectionEventBus.Clear()) { name = "detail-card-close", text = "X", tooltip = "Close" };
            close.AddToClassList("detail-card-close");
            _name = new Label { name = "detail-card-name" };
            _name.AddToClassList("detail-card-name");
            _subtitle = new Label { name = "detail-card-subtitle" };
            _subtitle.AddToClassList("detail-card-subtitle");
            _summary = new Label { name = "detail-card-summary" };
            _summary.AddToClassList("detail-card-summary");
            Root.Add(close);
            Root.Add(_name);
            Root.Add(_subtitle);
            Root.Add(_summary);
            parent.Add(Root);

            SelectionEventBus.OnMarkerSelected += Show;
        }

        public string NameText => _name.text;
        public string SubtitleText => _subtitle.text;

        // Where the card finds a POI by id and the taxonomy it reads labels from
        public void Configure(Func<string, POIData> findPoi, WallConfigData taxonomy)
        {
            _findPoi = findPoi;
            _taxonomy = taxonomy;
            if (SelectionEventBus.CurrentPoiId != null) Show(SelectionEventBus.CurrentPoiId);
        }

        private void Show(string poiId)
        {
            var poi = _findPoi?.Invoke(poiId);
            if (poi == null) return;
            _name.text = poi.name;
            _subtitle.text = Subtitle(poi, _taxonomy);
            _summary.text = poi.summary ?? "";
            _summary.style.display = string.IsNullOrEmpty(poi.summary) ? DisplayStyle.None : DisplayStyle.Flex;
        }

        // "category - level name" (either part left out when the POI has none)
        public static string Subtitle(POIData poi, WallConfigData taxonomy)
        {
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(poi.category)) parts.Add(poi.category);
            var level = taxonomy?.hierarchy_levels?.Find(l => l != null && l.key == poi.hierarchy_level_key);
            if (level != null && !string.IsNullOrWhiteSpace(level.level_name)) parts.Add(level.level_name);
            return string.Join(" - ", parts);
        }

        public void Dispose() => SelectionEventBus.OnMarkerSelected -= Show;
    }
}

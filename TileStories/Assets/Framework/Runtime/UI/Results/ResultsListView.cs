using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace TileStories
{
    // The results list (spec _2.6 section 9): UI Toolkit's virtualised ListView (only visible rows are
    // built), one row per result -- the POI's name and "category - level" -- in ranked order. Tapping a
    // row selects that POI through SelectionEventBus, like tapping its marker. With no results it shows
    // the wall's no-results message and, when there is one, a one-tap "remove this filter" button.
    // Plain C#: built into the parent the search UI hands it; styled by SearchUI.uss.
    public sealed class ResultsListView : IDisposable
    {
        public sealed class Row
        {
            public string PoiId;
            public string Name;
            public string Subtitle;
        }

        public VisualElement Root { get; }
        private readonly ListView _list;
        private readonly VisualElement _empty;
        private readonly Label _emptyMessage;
        private readonly Button _relax;
        private readonly List<Row> _rows = new();
        private Action _relaxAction;

        public ResultsListView(VisualElement parent)
        {
            Root = new VisualElement { name = "results-panel" };
            Root.AddToClassList("search-panel");
            Root.AddToClassList("results-panel");

            _list = new ListView
            {
                name = "results-list",
                itemsSource = _rows,
                selectionType = SelectionType.None,
                virtualizationMethod = CollectionVirtualizationMethod.FixedHeight,
                fixedItemHeight = 56,   // = .result-row height in SearchUI.uss
                makeItem = MakeRow,
                bindItem = BindRow,
            };
            _list.AddToClassList("results-list");
            Root.Add(_list);

            _empty = new VisualElement { name = "results-empty" };
            _empty.AddToClassList("results-empty");
            _emptyMessage = new Label { name = "results-empty-message" };
            _emptyMessage.AddToClassList("results-empty-message");
            _relax = new Button(() => _relaxAction?.Invoke()) { name = "results-relax" };
            _relax.AddToClassList("search-button");
            _empty.Add(_emptyMessage);
            _empty.Add(_relax);
            Root.Add(_empty);

            parent.Add(Root);
            SelectionEventBus.OnMarkerSelected += OnSelectionChanged;
            SelectionEventBus.OnSelectionCleared += OnSelectionCleared;
            ShowRows(new List<Row>(), "", null, null);
        }

        public IReadOnlyList<Row> Rows => _rows;
        public string EmptyMessage => _emptyMessage.text;
        public bool EmptyShown => _empty.style.display != DisplayStyle.None;
        public string RelaxText => _relax.style.display == DisplayStyle.None ? null : _relax.text;

        public void SetShown(bool shown) => Root.style.display = shown ? DisplayStyle.Flex : DisplayStyle.None;
        public bool IsShown => Root.style.display != DisplayStyle.None;

        // Show these rows; with none, `emptyMessage` and (when relaxText is set) a button running relaxAction
        public void ShowRows(IReadOnlyList<Row> rows, string emptyMessage, string relaxText, Action relaxAction)
        {
            _rows.Clear();
            _rows.AddRange(rows);
            _list.RefreshItems();

            bool empty = _rows.Count == 0;
            _list.style.display = empty ? DisplayStyle.None : DisplayStyle.Flex;
            _empty.style.display = empty ? DisplayStyle.Flex : DisplayStyle.None;
            _emptyMessage.text = emptyMessage ?? "";
            _relaxAction = relaxAction;
            _relax.text = relaxText ?? "";
            _relax.style.display = empty && relaxText != null ? DisplayStyle.Flex : DisplayStyle.None;
        }

        // Tap a row by POI id, exactly as a visitor's tap would
        public void TapRow(string poiId) => SelectionEventBus.Select(poiId);

        private VisualElement MakeRow()
        {
            var row = new VisualElement();
            row.AddToClassList("result-row");
            var name = new Label { name = "result-name" };
            name.AddToClassList("result-name");
            var subtitle = new Label { name = "result-subtitle" };
            subtitle.AddToClassList("result-subtitle");
            row.Add(name);
            row.Add(subtitle);
            row.RegisterCallback<ClickEvent>(_ =>
            {
                if (row.userData is string id) TapRow(id);
            });
            return row;
        }

        private void BindRow(VisualElement element, int index)
        {
            var row = _rows[index];
            element.userData = row.PoiId;
            element.name = "result-row-" + row.PoiId;
            element.tooltip = row.Name;
            element.Q<Label>("result-name").text = row.Name;
            element.Q<Label>("result-subtitle").text = row.Subtitle;
            element.EnableInClassList("result-row--selected", row.PoiId == SelectionEventBus.CurrentPoiId);
        }

        private void OnSelectionChanged(string _) => _list.RefreshItems();
        private void OnSelectionCleared() => _list.RefreshItems();

        public void Dispose()
        {
            SelectionEventBus.OnMarkerSelected -= OnSelectionChanged;
            SelectionEventBus.OnSelectionCleared -= OnSelectionCleared;
        }
    }
}

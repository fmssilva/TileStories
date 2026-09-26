using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace TileStories
{
    // One facet group of the tray and its choices (taxonomy key + the label the visitor reads)
    public sealed class FacetGroupOptions
    {
        public FacetGroup Group;
        public string Title;
        public List<(string key, string label)> Choices = new();
    }

    // The facet filter tray (spec _2.6 section 7, NN/G's "faceted search with a tray"): one chip per
    // taxonomy row of each enabled facet group, toggling a chip updates the results at once (no Apply).
    // Plain C#: built into the parent element the search UI hands it; styled by SearchUI.uss.
    public sealed class FilterTrayView
    {
        public VisualElement Root { get; }
        public FacetSelection Selection { get; } = new();

        // Raised after the visitor changed any filter
        public event Action Changed;

        private readonly ScrollView _groups;

        public FilterTrayView(VisualElement parent)
        {
            Root = new VisualElement { name = "filter-tray" };
            Root.AddToClassList("search-panel");
            Root.AddToClassList("filter-tray");

            // - a scroll view: a wall with many rows gets a scrolling tray, never overlapping groups
            _groups = new ScrollView(ScrollViewMode.Vertical) { name = "filter-groups" };
            _groups.AddToClassList("filter-groups");
            Root.Add(_groups);

            var clear = new Button(ClearAll) { name = "filter-clear-all", text = "Clear filters", tooltip = "Clear every filter" };
            clear.AddToClassList("search-button");
            Root.Add(clear);

            parent.Add(Root);
            SetOpen(false);
        }

        public bool IsOpen => Root.style.display != DisplayStyle.None;

        public void SetOpen(bool open) => Root.style.display = open ? DisplayStyle.Flex : DisplayStyle.None;

        // Rebuild the chips for this taxonomy; active filters whose row still exists stay on
        public void Rebuild(IReadOnlyList<FacetGroupOptions> groups)
        {
            _groups.Clear();
            var stillThere = new FacetSelection();
            foreach (var g in groups)
            {
                var section = new VisualElement { name = "facet-group-" + g.Group };
                section.AddToClassList("facet-group");
                var title = new Label(g.Title);
                title.AddToClassList("facet-title");
                section.Add(title);

                var chips = new VisualElement();
                chips.AddToClassList("facet-chips");
                foreach (var (key, label) in g.Choices)
                {
                    bool on = Selection.IsActive(g.Group, key);
                    if (on) stillThere.Set(g.Group, key, true);
                    var chip = new Toggle(label) { name = $"facet-{g.Group}-{key}", value = on, tooltip = $"Filter by {label}" };
                    chip.AddToClassList("facet-chip");
                    var group = g.Group;
                    chip.RegisterValueChangedCallback(evt =>
                    {
                        if (Selection.Set(group, key, evt.newValue)) Changed?.Invoke();
                    });
                    chips.Add(chip);
                }
                section.Add(chips);
                _groups.Add(section);
            }

            Selection.Clear();
            foreach (var group in stillThere.ActiveGroups)
                foreach (var key in stillThere.Active(group))
                    Selection.Set(group, key, true);
        }

        // Turn one filter value on or off (the relax button, tests): the same result as tapping its chip
        public void SetFacet(FacetGroup group, string key, bool on)
        {
            Root.Q<Toggle>($"facet-{group}-{key}")?.SetValueWithoutNotify(on);
            if (Selection.Set(group, key, on)) Changed?.Invoke();
        }

        public void ClearAll()
        {
            if (!Selection.Any) return;
            Selection.Clear();
            _groups.Query<Toggle>().ForEach(t => t.SetValueWithoutNotify(false));
            Changed?.Invoke();
        }

        // The tray's groups for a wall: only enabled facet groups whose table has rows; each chip reads
        // the row's label (a level's name, a badge's label), the key only when the row has no label. Then one
        // group per Keyword Field marked Filter, whose chips are the keywords the POIs hold in it (A-Z).
        public static List<FacetGroupOptions> BuildOptions(WallConfigData config, FilterSettings filter)
        {
            var groups = new List<FacetGroupOptions>();
            if (config == null) return groups;
            filter ??= new FilterSettings();

            if (filter.category_facet)
                AddGroup(groups, FacetGroup.Category, "Category", config.category_styles, e => e.category, e => e.category);
            if (filter.badge_facet)
                AddGroup(groups, FacetGroup.Badge, "Badge", config.badge_categories, e => e.key, e => e.label);
            if (filter.status_facet)
                AddGroup(groups, FacetGroup.Status, "Status", config.outline_levels, e => e.key, e => e.label);
            if (filter.hierarchy_facet)
                AddGroup(groups, FacetGroup.Hierarchy, "Level", config.hierarchy_levels, e => e.key, e => e.level_name);

            if (config.search_fields != null)
                foreach (var field in config.search_fields)
                    if (field != null && field.filterable && !string.IsNullOrWhiteSpace(field.key))
                        AddFieldGroup(groups, field, config.pois);
            return groups;
        }

        // One chip per distinct keyword (case and spaces ignored, first spelling kept) the POIs hold in this field
        private static void AddFieldGroup(List<FacetGroupOptions> groups, SearchFieldDefinition field, List<POIData> pois)
        {
            var labels = new SortedDictionary<string, string>(StringComparer.Ordinal);
            if (pois != null)
                foreach (var poi in pois)
                {
                    var own = poi?.search_keyword_fields?.Find(f => f != null && f.field_key == field.key);
                    if (own?.keywords == null) continue;
                    foreach (var keyword in own.keywords)
                    {
                        string key = FilterFacetEvaluator.FieldValueKey(keyword);
                        if (key.Length > 0 && !labels.ContainsKey(key)) labels[key] = keyword.Trim();
                    }
                }
            if (labels.Count == 0) return;

            var options = new FacetGroupOptions
            {
                Group = FacetGroup.Field(field.key),
                Title = string.IsNullOrWhiteSpace(field.label) ? field.key : field.label,
            };
            foreach (var kvp in labels) options.Choices.Add((kvp.Key, kvp.Value));
            groups.Add(options);
        }

        private static void AddGroup<T>(List<FacetGroupOptions> groups, FacetGroup group, string title, List<T> rows,
            Func<T, string> key, Func<T, string> label) where T : class
        {
            if (rows == null) return;
            var options = new FacetGroupOptions { Group = group, Title = title };
            foreach (var row in rows)
            {
                if (row == null || string.IsNullOrEmpty(key(row))) continue;
                string l = label(row);
                options.Choices.Add((key(row), string.IsNullOrWhiteSpace(l) ? key(row) : l));
            }
            if (options.Choices.Count > 0) groups.Add(options);
        }
    }
}

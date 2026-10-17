using System.Collections.Generic;
using UnityEngine.UIElements;

namespace TileStories
{
    // The drawer_tree navigation element (_3.2 section 5.3, canvas C3, L1 + L2): the card's whole ontology as an OUTLINE in a drawer from
    // the side -- every section (a family, or a format with Group By = format) with its block count, and under each the kinds of block it
    // holds on this card, each with its count. A tap on a section moves the card to its first block; a tap on a kind, to that kind's first
    // block (any block in two levels, rule 2.1). A section with one kind lists no kinds under it (the row would only repeat the section --
    // NavOntologyRule.OffersAKindChoice, the rule icon_tabs_chips and bar_sheet use). The section and the kind being read are marked: a bar at
    // the row's start AND bolder words (NavTab.CurrentClass), never colour alone.
    // The least discoverable element and the most complete one (NN/g, hamburger menus): never a default -- it lives behind the header's menu
    // button, opened by the card's overlay owner (CardMenuOverlay) on a scrim, and closes there on a pick (a jump), the X, the scrim and Back.
    // Views only: built from NavTab, NavTint, NavMenuHeader and the model.
    public sealed class DrawerTreeNavView : INavElementView
    {
        // On a kind's row (indented under its section)
        public const string KindRowClass = "nav-drawer__kind";

        public VisualElement Root { get; }
        public NavMenuHeader Header { get; }
        public ScrollView List { get; }
        // One row per section of the model, in its order (Index = the section's index)
        public IReadOnlyList<NavTab> SectionRows => _sections.Shown;
        // One row per kind listed, in the outline's order (each with the section and kind it stands for)
        public IReadOnlyList<(NavTab Row, int Section, CardNavModel.Kind Kind)> KindRows => _kindRows;

        private readonly ElementPool<NavTab> _sections;
        private readonly ElementPool<NavTab> _kinds;
        private readonly List<(NavTab Row, int Section, CardNavModel.Kind Kind)> _kindRows = new();
        private readonly NavTint _tint;
        private CardNavModel _model;
        private NavController _nav;
        private CardMenuOverlay _overlay;

        public DrawerTreeNavView()
        {
            Root = new VisualElement { name = "nav-drawer-tree" };
            Root.AddToClassList("nav-drawer");
            Header = new NavMenuHeader("nav-drawer", () => _overlay?.Close());
            // - a finger scrolls it: no desktop scroller, like every scroll of the card
            List = new ScrollView(ScrollViewMode.Vertical)
            {
                name = "nav-drawer-list",
                verticalScrollerVisibility = ScrollerVisibility.Hidden,
                horizontalScrollerVisibility = ScrollerVisibility.Hidden,
            };
            List.AddToClassList("nav-drawer__list");
            Root.Add(Header.Root);
            Root.Add(List);
            _sections = new ElementPool<NavTab>(index => Row(NavTab.Create("nav-drawer-section-", index, true, () => PickSection(index)), "nav-drawer__section"),
                tab => tab.Button.RemoveFromHierarchy());
            _kinds = new ElementPool<NavTab>(index => Row(NavTab.Create("nav-drawer-kind-", index, false, () => PickKind(index)), KindRowClass),
                tab => tab.Button.RemoveFromHierarchy());
            _tint = new NavTint(Root);
        }

        public void Bind(CardNavModel model, NavController nav, BlockBindContext context)
        {
            Unbind();
            _model = model;
            _nav = nav;
            _overlay = context?.Overlay;
            var strings = context?.Strings;
            Header.Words(strings);
            for (int s = 0; s < model.Sections.Count; s++)
            {
                var section = model.Sections[s];
                var row = _sections.Take();
                row.Label.text = Words(section.LabelKey, section.Key, strings);
                row.Button.tooltip = row.Label.text;
                CardIcons.SetKey(row.Icon, section.IconKey);
                row.Icon.style.display = section.IconKey != null ? DisplayStyle.Flex : DisplayStyle.None;
                _tint.Icon(row.Icon, section.ColorToken);
                // - an outline says how much is behind each line: the counts are its point, so they are always shown
                NavCount.Set(row.Count, true, section.Count);
                List.Add(row.Button);
                if (!NavOntologyRule.OffersAKindChoice(section.Kinds.Count)) continue;
                foreach (var kind in section.Kinds)
                {
                    var kindRow = _kinds.Take();
                    kindRow.Label.text = Words(kind.LabelKey, kind.Key, strings);
                    kindRow.Button.tooltip = kindRow.Label.text;
                    NavCount.Set(kindRow.Count, true, kind.Count);
                    List.Add(kindRow.Button);
                    _kindRows.Add((kindRow, s, kind));
                }
            }
            nav.CurrentBlockChanged += OnCurrentBlockChanged;
            OnCurrentBlockChanged(nav.CurrentBlock);
        }

        public void Unbind()
        {
            if (_nav != null) _nav.CurrentBlockChanged -= OnCurrentBlockChanged;
            _nav = null;
            _model = null;
            _overlay = null;
            _sections.ReleaseAll();
            _kinds.ReleaseAll();
            _kindRows.Clear();
            _tint.Clear();
        }

        // A tap on a section's row: its first block (the overlay owner hears the jump and closes the drawer)
        public void PickSection(int index)
        {
            if (_model == null || index < 0 || index >= _model.Sections.Count) return;
            _nav?.JumpTo(index, BuiltInNavElements.DrawerTree);
        }

        // A tap on a kind's row (its place among the kind rows): that kind's first block
        public void PickKind(int index)
        {
            if (index < 0 || index >= _kindRows.Count) return;
            _nav?.JumpToBlock(_kindRows[index].Kind.Targets[0], BuiltInNavElements.DrawerTree);
        }

        // The reader moved: mark the section and the kind of the block being read
        private void OnCurrentBlockChanged(string block)
        {
            if (_model == null) return;
            int section = _model.SectionOf(block);
            var kind = _model.KindOf(block);
            if (section < 0) section = _nav?.Current ?? -1;
            foreach (var row in _sections.Shown) row.Button.EnableInClassList(NavTab.CurrentClass, row.Index == section);
            foreach (var (row, s, k) in _kindRows) row.Button.EnableInClassList(NavTab.CurrentClass, s == section && ReferenceEquals(k, kind));
        }

        // A label key's words in the card's language (the key itself when none are found)
        private static string Words(string labelKey, string key, CardStrings strings)
        {
            string words = labelKey != null && strings != null ? strings.Get(labelKey) : "";
            return string.IsNullOrEmpty(words) ? key : words;
        }

        private static NavTab Row(NavTab tab, string rowClass)
        {
            tab.Button.AddToClassList("nav-drawer__row");
            tab.Button.AddToClassList(rowClass);
            return tab;
        }
    }
}

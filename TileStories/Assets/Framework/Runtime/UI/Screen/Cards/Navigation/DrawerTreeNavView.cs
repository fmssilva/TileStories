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
        // The facet chips under the title (_3.2 step 18): they narrow the outline in place
        public NavFacetBar Facets { get; }
        public ScrollView List { get; }
        // One row per section of the outline now, in the model's order: every section of the model, or with a facet on only the sections
        // that still hold a block that passes (the row's place = its index here; SectionOf gives the model's index)
        public IReadOnlyList<NavTab> SectionRows => _sections.Shown;
        // One row per kind listed, in the outline's order (each with the model's section and kind it stands for)
        public IReadOnlyList<(NavTab Row, int Section, CardNavModel.Kind Kind)> KindRows => _kindRows;

        private readonly ElementPool<NavTab> _sections;
        private readonly ElementPool<NavTab> _kinds;
        private readonly List<(NavTab Row, int Section, CardNavModel.Kind Kind)> _kindRows = new();
        // What a row stands for in the facets' list: the model's section of each section row, the blocks that pass in each section row and kind row
        private readonly List<int> _sectionOfRow = new();
        private readonly List<List<string>> _passingOfSection = new();
        private readonly List<List<string>> _passingOfKind = new();
        private readonly NavTint _tint;
        private CardNavModel _model;
        private NavController _nav;
        private CardMenuOverlay _overlay;
        private CardStrings _strings;

        public DrawerTreeNavView()
        {
            Root = new VisualElement { name = "nav-drawer-tree" };
            Root.AddToClassList("nav-drawer");
            Header = new NavMenuHeader("nav-drawer", () => _overlay?.Close());
            Facets = new NavFacetBar("nav-drawer-facets");
            Facets.Changed += Draw;
            // - a finger scrolls it: no desktop scroller, like every scroll of the card
            List = new ScrollView(ScrollViewMode.Vertical)
            {
                name = "nav-drawer-list",
                verticalScrollerVisibility = ScrollerVisibility.Hidden,
                horizontalScrollerVisibility = ScrollerVisibility.Hidden,
            };
            List.AddToClassList("nav-drawer__list");
            Root.Add(Header.Root);
            Root.Add(Facets.Root);
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
            _strings = context?.Strings;
            Header.Words(_strings);
            // - every facet off for a new card; the bar reads the card's blocks and draws its chips
            Facets.Bind(model, nav, context);
            nav.CurrentBlockChanged += OnCurrentBlockChanged;
            Draw();
        }

        public void Unbind()
        {
            if (_nav != null) _nav.CurrentBlockChanged -= OnCurrentBlockChanged;
            Facets.Unbind();
            _nav = null;
            _model = null;
            _overlay = null;
            _strings = null;
            ClearRows();
        }

        // The outline: a line per section that has a block passing the facets on (all of them with none on), its kinds under it. The counts
        // are the blocks that pass, so a chip narrows the numbers too; the other sections and kinds are not listed at all
        private void Draw()
        {
            if (_model == null) return;
            ClearRows();
            for (int s = 0; s < _model.Sections.Count; s++)
            {
                var section = _model.Sections[s];
                var passing = Facets.Passing(section.Targets);
                if (passing.Count == 0) continue;
                var row = _sections.Take();
                _sectionOfRow.Add(s);
                _passingOfSection.Add(passing);
                row.Label.text = Words(section.LabelKey, section.Key, _strings);
                row.Button.tooltip = row.Label.text;
                CardIcons.SetKey(row.Icon, section.IconKey);
                row.Icon.style.display = section.IconKey != null ? DisplayStyle.Flex : DisplayStyle.None;
                _tint.Icon(row.Icon, section.ColorToken);
                // - an outline says how much is behind each line: the counts are its point, so they are always shown
                NavCount.Set(row.Count, true, passing.Count);
                List.Add(row.Button);
                var kinds = new List<(CardNavModel.Kind Kind, List<string> Passing)>();
                foreach (var kind in section.Kinds)
                {
                    var ofKind = Facets.Passing(kind.Targets);
                    if (ofKind.Count > 0) kinds.Add((kind, ofKind));
                }
                if (!NavOntologyRule.OffersAKindChoice(kinds.Count)) continue;
                foreach (var (kind, ofKind) in kinds)
                {
                    var kindRow = _kinds.Take();
                    kindRow.Label.text = Words(kind.LabelKey, kind.Key, _strings);
                    kindRow.Button.tooltip = kindRow.Label.text;
                    NavCount.Set(kindRow.Count, true, ofKind.Count);
                    List.Add(kindRow.Button);
                    _kindRows.Add((kindRow, s, kind));
                    _passingOfKind.Add(ofKind);
                }
            }
            List.scrollOffset = UnityEngine.Vector2.zero;
            OnCurrentBlockChanged(_nav?.CurrentBlock);
        }

        private void ClearRows()
        {
            _sections.ReleaseAll();
            _kinds.ReleaseAll();
            _kindRows.Clear();
            _sectionOfRow.Clear();
            _passingOfSection.Clear();
            _passingOfKind.Clear();
            _tint.Clear();
        }

        // The model's section a section row (its place among the rows shown) stands for, -1 when there is none
        public int SectionOf(int row) => row >= 0 && row < _sectionOfRow.Count ? _sectionOfRow[row] : -1;

        // A tap on a section's row (its place among the rows shown): its first block that passes the facets (the overlay owner hears the jump
        // and closes the drawer). With no facet on that is the section's first block, as ever
        public void PickSection(int index)
        {
            if (_model == null || index < 0 || index >= _sectionOfRow.Count) return;
            if (Facets.Facets.IsActive) _nav?.JumpToBlock(_passingOfSection[index][0], BuiltInNavElements.DrawerTree);
            else _nav?.JumpTo(_sectionOfRow[index], BuiltInNavElements.DrawerTree);
        }

        // A tap on a kind's row (its place among the kind rows): that kind's first block that passes the facets
        public void PickKind(int index)
        {
            if (index < 0 || index >= _kindRows.Count) return;
            _nav?.JumpToBlock(_passingOfKind[index][0], BuiltInNavElements.DrawerTree);
        }

        // The reader moved: mark the section and the kind of the block being read
        private void OnCurrentBlockChanged(string block)
        {
            if (_model == null) return;
            int section = _model.SectionOf(block);
            var kind = _model.KindOf(block);
            if (section < 0) section = _nav?.Current ?? -1;
            for (int i = 0; i < _sections.Shown.Count; i++)
                _sections.Shown[i].Button.EnableInClassList(NavTab.CurrentClass, _sectionOfRow[i] == section);
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

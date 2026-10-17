using System.Collections.Generic;
using UnityEngine.UIElements;

namespace TileStories
{
    // The grid_overlay navigation element (_3.2 section 5.3, canvas C4, L1 -> L2 in place): BIG tiles over the card, two to a row -- one per
    // section with its icon (a family's, in its colour), its words and its block count -- under a Purpose / Format switch. Kiosk-like: every
    // tile is a big thumb target. A tile whose section holds two kinds or more opens those kinds IN PLACE (the same grid, each kind a tile
    // with its count, and an "All sections" way back); a tile with one kind moves the card straight to its first block (rule 2.5.4: no level
    // with one choice). A kind's tile moves the card to that kind's first block.
    // The switch pivots the tiles between the visitor's two axes, which the model answers since step 2: Purpose = the families, Format = the
    // formats (read, look, watch...). It opens on the wall's Group By and reads the other axis from CardNavModel.By -- the same targets
    // regrouped by the model, never by this view. The tile being read is marked: an outline AND bolder words, never colour alone.
    // Opened by the header's menu button (or a bar's More) through the card's overlay owner, on a scrim; it closes there on a pick (a jump),
    // the X, the scrim and Back, and opens again on all sections (the axis the visitor chose stays for this card).
    public sealed class GridOverlayNavView : INavElementView
    {
        // On the tile of the section / kind being read; on the switch option shown now
        public const string CurrentClass = "nav-grid__tile--current";
        public const string OptionCurrentClass = "nav-grid__option--current";

        // One big tile: the whole button is the target; a family's icon, the words, the count
        public sealed class Tile
        {
            public readonly int Index;
            public readonly Button Button;
            public readonly VisualElement Icon;
            public readonly Label Label;
            public readonly Label Count;

            public Tile(int index, Button button, VisualElement icon, Label label, Label count)
            {
                Index = index;
                Button = button;
                Icon = icon;
                Label = label;
                Count = count;
            }
        }

        public VisualElement Root { get; }
        public NavMenuHeader Header { get; }
        // The Purpose / Format switch
        public Button ByPurpose { get; }
        public Button ByFormat { get; }
        // The way back from one section's kinds to every section, and the open section's words beside it (hidden on the sections level)
        public VisualElement BackRow { get; }
        public Button All { get; }
        public Label OpenTitle { get; }
        public ScrollView Scroll { get; }
        public VisualElement Grid { get; }
        public IReadOnlyList<Tile> Tiles => _tiles.Shown;

        // The axis the tiles show (CardOptions.GroupByFamily = Purpose, GroupByFormat = Format) and the section whose kinds are open (-1 = all)
        public string Axis { get; private set; } = CardOptions.GroupByFamily;
        public int OpenSection { get; private set; } = -1;
        // The model the tiles are drawn from now: the card's map on the chosen axis
        public CardNavModel Shown => _model?.By(Axis);

        private readonly ElementPool<Tile> _tiles;
        private readonly NavTint _tint;
        private CardNavModel _model;
        private NavController _nav;
        private CardMenuOverlay _overlay;
        private CardStrings _strings;

        public GridOverlayNavView()
        {
            Root = new VisualElement { name = "nav-grid-overlay" };
            Root.AddToClassList("nav-grid");
            Header = new NavMenuHeader("nav-grid", () => _overlay?.Close());
            var switchRow = new VisualElement { name = "nav-grid-switch" };
            switchRow.AddToClassList("nav-grid__switch");
            ByPurpose = Option("nav-grid-by-purpose", () => ShowAxis(CardOptions.GroupByFamily));
            ByFormat = Option("nav-grid-by-format", () => ShowAxis(CardOptions.GroupByFormat));
            switchRow.Add(ByPurpose);
            switchRow.Add(ByFormat);
            BackRow = new VisualElement { name = "nav-grid-back" };
            BackRow.AddToClassList("nav-grid__back");
            All = new Button(() => OpenKinds(-1)) { name = "nav-grid-all" };
            All.AddToClassList("nav-grid__all");
            var allIcon = CardIcons.Create();
            CardIcons.SetKey(allIcon, CardIcons.OpenBlock);
            allIcon.AddToClassList("nav-grid__all-icon");
            All.Add(allIcon);
            OpenTitle = new Label { name = "nav-grid-open-title", pickingMode = PickingMode.Ignore };
            OpenTitle.AddToClassList("nav-grid__open-title");
            BackRow.Add(All);
            BackRow.Add(OpenTitle);
            // - a finger scrolls it: no desktop scroller, like every scroll of the card
            Scroll = new ScrollView(ScrollViewMode.Vertical)
            {
                name = "nav-grid-scroll",
                verticalScrollerVisibility = ScrollerVisibility.Hidden,
                horizontalScrollerVisibility = ScrollerVisibility.Hidden,
            };
            Scroll.AddToClassList("nav-grid__scroll");
            Grid = new VisualElement { name = "nav-grid-tiles" };
            Grid.AddToClassList("nav-grid__tiles");
            Scroll.Add(Grid);
            Root.Add(Header.Root);
            Root.Add(switchRow);
            Root.Add(BackRow);
            Root.Add(Scroll);
            _tiles = new ElementPool<Tile>(CreateTile, tile => tile.Button.RemoveFromHierarchy());
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
            ByPurpose.text = _strings?.Get(CardStrings.Keys.NavMenuByPurpose) ?? "";
            ByFormat.text = _strings?.Get(CardStrings.Keys.NavMenuByFormat) ?? "";
            All.text = _strings?.Get(CardStrings.Keys.NavMenuAll) ?? "";
            // - it opens on the wall's own axis
            Axis = model.GroupBy;
            OpenSection = -1;
            nav.CurrentBlockChanged += OnCurrentBlockChanged;
            if (_overlay != null) _overlay.Closed += OnOverlayClosed;
            Redraw();
        }

        public void Unbind()
        {
            if (_nav != null) _nav.CurrentBlockChanged -= OnCurrentBlockChanged;
            if (_overlay != null) _overlay.Closed -= OnOverlayClosed;
            _nav = null;
            _model = null;
            _overlay = null;
            _strings = null;
            OpenSection = -1;
            _tiles.ReleaseAll();
            _tint.Clear();
        }

        // The switch: show the tiles on this axis, from all its sections
        public void ShowAxis(string axis)
        {
            if (_model == null) return;
            Axis = axis == CardOptions.GroupByFormat ? CardOptions.GroupByFormat : CardOptions.GroupByFamily;
            OpenSection = -1;
            Redraw();
        }

        // A tap on a tile (its place in the grid): a section with a choice of kinds opens them in place, any other moves the card
        public void PickTile(int index)
        {
            var shown = Shown;
            if (shown == null || index < 0) return;
            if (OpenSection < 0)
            {
                if (index >= shown.Sections.Count) return;
                var section = shown.Sections[index];
                if (NavOntologyRule.OffersAKindChoice(section.Kinds.Count)) OpenKinds(index);
                else Jump(section.Targets[0]);
                return;
            }
            var kinds = shown.Sections[OpenSection].Kinds;
            if (index < kinds.Count) Jump(kinds[index].Targets[0]);
        }

        // Show one section's kinds in place (-1: back to every section)
        public void OpenKinds(int section)
        {
            OpenSection = Shown != null && section >= 0 && section < Shown.Sections.Count ? section : -1;
            Redraw();
        }

        // The card moves (the overlay owner hears the jump and closes the grid)
        private void Jump(string block) => _nav?.JumpToBlock(block, BuiltInNavElements.GridOverlay);

        // The menu closed (whichever way): it opens again on every section
        private void OnOverlayClosed()
        {
            if (OpenSection < 0) return;
            OpenKinds(-1);
        }

        // Draw the tiles of the level shown now: every section on the axis, or one section's kinds
        private void Redraw()
        {
            _tiles.ReleaseAll();
            _tint.Clear();
            var shown = Shown;
            if (shown == null) return;
            ByPurpose.EnableInClassList(OptionCurrentClass, Axis == CardOptions.GroupByFamily);
            ByFormat.EnableInClassList(OptionCurrentClass, Axis == CardOptions.GroupByFormat);
            BackRow.style.display = OpenSection >= 0 ? DisplayStyle.Flex : DisplayStyle.None;
            if (OpenSection < 0)
            {
                foreach (var section in shown.Sections)
                    Fill(_tiles.Take(), section.LabelKey, section.Key, section.IconKey, section.ColorToken, section.Count);
            }
            else
            {
                var open = shown.Sections[OpenSection];
                OpenTitle.text = Words(open.LabelKey, open.Key);
                // - kinds have no icon of their own: their words name them
                foreach (var kind in open.Kinds) Fill(_tiles.Take(), kind.LabelKey, kind.Key, null, null, kind.Count);
            }
            Scroll.scrollOffset = UnityEngine.Vector2.zero;
            OnCurrentBlockChanged(_nav?.CurrentBlock);
        }

        private void Fill(Tile tile, string labelKey, string key, string iconKey, string colorToken, int count)
        {
            tile.Label.text = Words(labelKey, key);
            tile.Button.tooltip = tile.Label.text;
            CardIcons.SetKey(tile.Icon, iconKey);
            tile.Icon.style.display = iconKey != null ? DisplayStyle.Flex : DisplayStyle.None;
            _tint.Icon(tile.Icon, colorToken);
            NavCount.Set(tile.Count, true, count);
            Grid.Add(tile.Button);
        }

        // The reader moved: mark the tile of the section (or kind) being read, on the axis shown
        private void OnCurrentBlockChanged(string block)
        {
            var shown = Shown;
            if (shown == null) return;
            int current = OpenSection < 0 ? shown.SectionOf(block) : KindIndex(shown, block);
            foreach (var tile in _tiles.Shown) tile.Button.EnableInClassList(CurrentClass, tile.Index == current);
        }

        // The place of the block's kind among the open section's kinds, -1 when it is in another section
        private int KindIndex(CardNavModel shown, string block)
        {
            if (shown.SectionOf(block) != OpenSection) return -1;
            var kind = shown.KindOf(block);
            var kinds = shown.Sections[OpenSection].Kinds;
            for (int i = 0; i < kinds.Count; i++)
                if (ReferenceEquals(kinds[i], kind)) return i;
            return -1;
        }

        private string Words(string labelKey, string key)
        {
            string words = labelKey != null && _strings != null ? _strings.Get(labelKey) : "";
            return string.IsNullOrEmpty(words) ? key : words;
        }

        private Tile CreateTile(int index)
        {
            var button = new Button(() => PickTile(index)) { name = "nav-grid-tile-" + index };
            button.AddToClassList("nav-grid__tile");
            var icon = CardIcons.Create();
            icon.AddToClassList("nav-grid__icon");
            var label = new Label { pickingMode = PickingMode.Ignore };
            label.AddToClassList("nav-grid__label");
            var count = NavCount.Create();
            count.AddToClassList("nav-grid__count");
            button.Add(icon);
            button.Add(label);
            button.Add(count);
            return new Tile(index, button, icon, label, count);
        }

        private static Button Option(string name, System.Action clicked)
        {
            var option = new Button(clicked) { name = name };
            option.AddToClassList("nav-grid__option");
            return option;
        }
    }
}

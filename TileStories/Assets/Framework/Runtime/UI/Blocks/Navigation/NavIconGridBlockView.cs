using UnityEngine.UIElements;

namespace TileStories
{
    // The nav_icon_grid block (_3.2 section 5.6, D1): the card's sections as a grid of tiles -- the family's icon in its colour, the
    // section's words and how many blocks it holds -- right after the header, so the visitor sees what the card offers before reading.
    // A tap jumps to the section's first block through the card's NavController (nav_jump "nav_icon_grid"), exactly as a tab does.
    // It computes no structure: the tiles are NavController.Model's sections, drawn when the controller is Bound -- the model is built
    // from the shown stack, so it exists only AFTER the stack bound this block, and a reveal rebuilds it without rebinding the block.
    // With fewer than two sections it draws nothing (rule 2.5.7: one tile is no choice). Tiles are NavTabs (NavItems), tinted by NavTint;
    // Navigation.uss lays them out.
    public sealed class NavIconGridBlockView : IBlockView
    {
        // What nav_jump names as the element that asked
        public const string ElementKey = BuiltInBlocks.NavIconGridKind;

        public VisualElement Root { get; }
        public System.Collections.Generic.IReadOnlyList<NavTab> Tiles => _tiles.Shown;

        private readonly ElementPool<NavTab> _tiles;
        private readonly NavTint _tint;
        private NavController _nav;
        private CardStrings _strings;

        public NavIconGridBlockView()
        {
            Root = new VisualElement { name = "card-nav-icon-grid" };
            Root.AddToClassList("card-block");
            Root.AddToClassList("nav-icon-grid");
            _tiles = new ElementPool<NavTab>(NewTile, tile => tile.Button.RemoveFromHierarchy());
            _tint = new NavTint(Root);
        }

        public void Bind(BlockInstanceData instance, BlockBindContext context)
        {
            Unbind();
            _strings = context.Strings;
            _nav = context.Nav;
            // - bound with the stack: nothing to draw yet, the sheet binds the controller to THIS card's model right after it (Bound). Bound
            //   later -- an expandable row opened -- the model is this card's already: draw at once
            Root.style.display = DisplayStyle.None;
            if (_nav == null) return;
            _nav.Bound += Draw;
            if (_nav.HasCurrentModel) Draw();
        }

        public void Unbind()
        {
            if (_nav != null) _nav.Bound -= Draw;
            _nav = null;
            _strings = null;
            _tiles.ReleaseAll();
            _tint.Clear();
        }

        // One tile per section of the bound model, in its order; none (and no grid) with fewer than two
        private void Draw()
        {
            _tiles.ReleaseAll();
            _tint.Clear();
            var model = _nav?.Model;
            bool shown = model != null && NavOntologyRule.OffersASectionChoice(model.Sections.Count);
            Root.style.display = shown ? DisplayStyle.Flex : DisplayStyle.None;
            if (!shown) return;
            foreach (var section in model.Sections)
            {
                var tile = _tiles.Take();
                Root.Add(tile.Button);
                string words = section.LabelKey != null && _strings != null ? _strings.Get(section.LabelKey) : "";
                tile.Label.text = string.IsNullOrEmpty(words) ? section.Key : words;
                tile.Button.tooltip = tile.Label.text;
                // - a format section has no icon: its words say it alone (NavLabelRule)
                CardIcons.SetKey(tile.Icon, section.IconKey);
                tile.Icon.style.display = section.IconKey != null ? DisplayStyle.Flex : DisplayStyle.None;
                _tint.Icon(tile.Icon, section.ColorToken);
                // - a grid of the card's contents always says how much each part holds: the count is its point, not a wall option
                NavCount.Set(tile.Count, true, section.Count);
            }
        }

        private NavTab NewTile(int index)
        {
            var tile = NavTab.Create("nav-icon-grid-tile-", index, withIcon: true, () => Tap(index));
            tile.Button.AddToClassList("nav-icon-grid__tile");
            return tile;
        }

        // A tile jumps to its section's first block, as the tab of that section would
        public void Tap(int index) => _nav?.JumpTo(index, ElementKey);
    }
}

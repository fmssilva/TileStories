using System.Collections.Generic;
using UnityEngine.UIElements;

namespace TileStories
{
    // The nav_bento block (_3.2 section 5.6, D4): a few big tiles the author picked, each leading to one block of THIS card -- the tile's own
    // words and picture when given, else the block's own words (the card's compact-row words, NavController.RowOf). A tap either moves the
    // card to the block (IBlockHost.ScrollToBlock, nav_jump "nav_bento") or opens it in the full view (IBlockHost.OpenFullView: a second view
    // of the block, its media handed over and given back through the card's owners), as the tile says. BentoTileRule decides which tiles
    // exist: a tile whose block the card cannot reach (BlockRefRule's jump targets) is not drawn, so a visitor never taps a tile that does
    // nothing; BentoTileRule.Wide decides which tiles span the row. Drawn when the controller is Bound (the words come from the shown stack,
    // which binds first), like nav_icon_grid.
    public sealed class NavBentoBlockView : IBlockView
    {
        // What nav_jump names as the element that asked
        public const string ElementKey = BuiltInBlocks.NavBentoKind;
        // On a tile that opens the full view (its glyph shows)
        public const string FullViewClass = "nav-bento__tile--full-view";
        // On a tile with a picture
        public const string PictureClass = "nav-bento__tile--picture";
        // On a cell that spans the whole row
        public const string WideClass = "nav-bento__cell--wide";

        // One tile: its cell in the grid, the tap target and its parts
        public sealed class Tile
        {
            public readonly VisualElement Cell;
            public readonly Button Button;
            public readonly CardImage Picture;
            // The block's family icon in its colour: what fills the top of a tile with no picture (a tile with one shows the picture instead)
            public readonly VisualElement Icon;
            public readonly Label Words;
            public readonly Label Meta;
            public readonly CardIcons.VectorGlyph FullViewGlyph;
            public string Target { get; internal set; } = "";
            public BentoTileRule.Opens Opens { get; internal set; }
            public bool HasPicture { get; internal set; }

            internal Tile(VisualElement cell, Button button, CardImage picture, VisualElement icon, Label words, Label meta, CardIcons.VectorGlyph glyph)
            {
                Cell = cell;
                Button = button;
                Picture = picture;
                Icon = icon;
                Words = words;
                Meta = meta;
                FullViewGlyph = glyph;
            }
        }

        public VisualElement Root { get; }
        public IReadOnlyList<Tile> Tiles => _tiles.Shown;

        private readonly ElementPool<Tile> _tiles;
        private readonly NavTint _tint;
        private NavController _nav;
        private BlockBindContext _context;
        private BlockInstanceData _instance;
        private IBlockHost _host;

        public NavBentoBlockView()
        {
            Root = new VisualElement { name = "card-nav-bento" };
            Root.AddToClassList("card-block");
            Root.AddToClassList("nav-bento");
            _tiles = new ElementPool<Tile>(NewTile, ReleaseTile);
            _tint = new NavTint(Root);
        }

        public void Bind(BlockInstanceData instance, BlockBindContext context)
        {
            Unbind();
            _context = context;
            _instance = instance;
            _host = context.Host;
            _nav = context.Nav;
            // - the words of the blocks it leads to come from the shown stack, which binds first: draw on Bound (or at once, bound later)
            Root.style.display = DisplayStyle.None;
            if (_nav == null) return;
            _nav.Bound += Draw;
            if (_nav.HasCurrentModel) Draw();
        }

        public void Unbind()
        {
            if (_nav != null) _nav.Bound -= Draw;
            _nav = null;
            _context = null;
            _instance = null;
            _host = null;
            _tiles.ReleaseAll();
            _tint.Clear();
        }

        // One tile per row the card can lead somewhere from, in the author's order; no tile at all, no block
        private void Draw()
        {
            _tiles.ReleaseAll();
            _tint.Clear();
            var read = new BlockFieldReader(_instance, _context?.Language, _context?.FallbackLanguage);
            var rowFields = BuiltInBlocks.NavBento.Field(BuiltInBlocks.NavBentoTilesField).ItemFields;
            var rows = new List<BentoTileRule.Tile>();
            foreach (var item in read.Items(BuiltInBlocks.NavBentoTilesField))
            {
                if (!BlockFieldReader.ItemIsComplete(item, rowFields)) continue;
                rows.Add(new BentoTileRule.Tile(
                    read.ItemValue(item, BuiltInBlocks.NavBentoTargetField),
                    read.ItemText(item, BuiltInBlocks.NavBentoWordsField),
                    read.ItemValidAsset(item, BuiltInBlocks.NavBentoPictureField, MediaKind.Image),
                    BentoTileRule.OpensOf(read.ItemValue(item, BuiltInBlocks.NavBentoOpensField))));
            }
            var tiles = BentoTileRule.Tiles(rows, _instance?.key, _context?.JumpTargets);
            Root.style.display = tiles.Count > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            var wide = BentoTileRule.Wide(tiles.Count);
            for (int i = 0; i < tiles.Count; i++)
            {
                var data = tiles[i];
                var tile = _tiles.Take();
                Root.Add(tile.Cell);
                tile.Cell.EnableInClassList(WideClass, wide[i]);
                tile.Target = data.Target;
                tile.Opens = data.Opens;
                var row = _nav.RowOf(data.Target);
                tile.Words.text = data.Words.Length > 0 ? data.Words : row.Words;
                tile.Meta.text = row.Meta;
                tile.Meta.style.display = row.Meta.Length > 0 ? DisplayStyle.Flex : DisplayStyle.None;
                tile.HasPicture = data.Picture.Length > 0;
                tile.Button.EnableInClassList(PictureClass, tile.HasPicture);
                tile.Button.EnableInClassList(FullViewClass, data.Opens == BentoTileRule.Opens.FullView);
                tile.FullViewGlyph.style.display = data.Opens == BentoTileRule.Opens.FullView ? DisplayStyle.Flex : DisplayStyle.None;
                tile.Picture.Root.style.display = tile.HasPicture ? DisplayStyle.Flex : DisplayStyle.None;
                if (tile.HasPicture) tile.Picture.Show(_context.Media, data.Picture, _context.Strings);
                // - no picture: the block's family icon in its colour fills the top of the tile and says what kind of block it leads to
                CardIcons.SetKey(tile.Icon, row.IconKey);
                tile.Icon.style.display = !tile.HasPicture && row.IconKey != null ? DisplayStyle.Flex : DisplayStyle.None;
                _tint.Icon(tile.Icon, row.ColorToken);
                tile.Button.tooltip = tile.Words.text;
            }
        }

        private Tile NewTile(int index)
        {
            var cell = new VisualElement { name = "nav-bento-cell-" + index, pickingMode = PickingMode.Ignore };
            cell.AddToClassList("nav-bento__cell");
            Tile tile = null;
            var button = new Button(() => Tap(tile)) { name = "nav-bento-tile-" + index };
            button.AddToClassList("nav-bento__tile");
            button.AddToClassList("card-tap");
            var picture = new CardImage("nav-bento__picture");
            picture.Root.pickingMode = PickingMode.Ignore;
            var icon = CardIcons.Create();
            icon.pickingMode = PickingMode.Ignore;
            icon.AddToClassList("nav-bento__icon");
            var body = new VisualElement { pickingMode = PickingMode.Ignore };
            body.AddToClassList("nav-bento__body");
            var words = new Label { pickingMode = PickingMode.Ignore };
            words.AddToClassList("nav-bento__words");
            var meta = new Label { pickingMode = PickingMode.Ignore };
            meta.AddToClassList("nav-bento__meta");
            var glyph = CardIcons.CreateVector(CardIcons.Shape.FullScreen);
            glyph.pickingMode = PickingMode.Ignore;
            glyph.AddToClassList("nav-bento__full-view-glyph");
            var texts = new VisualElement { pickingMode = PickingMode.Ignore };
            texts.AddToClassList("nav-bento__texts");
            texts.Add(words);
            texts.Add(meta);
            body.Add(texts);
            body.Add(glyph);
            button.Add(picture.Root);
            button.Add(icon);
            button.Add(body);
            cell.Add(button);
            tile = new Tile(cell, button, picture, icon, words, meta, glyph);
            return tile;
        }

        private static void ReleaseTile(Tile tile)
        {
            tile.Cell.RemoveFromHierarchy();
            tile.Picture.Clear(null);
        }

        // A tap: the card moves to the block, or the block opens in the full view over the card -- as the tile says
        private void Tap(Tile tile)
        {
            if (tile == null || string.IsNullOrEmpty(tile.Target)) return;
            if (tile.Opens == BentoTileRule.Opens.FullView) _host?.OpenFullView(tile.Target);
            else _host?.ScrollToBlock(tile.Target, ElementKey);
        }

        // What a test taps: the tile at `index`
        public void TapTile(int index) => Tap(_tiles.Shown[index]);
    }
}

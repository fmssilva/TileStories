using System.Collections.Generic;

namespace TileStories
{
    // What the nav_bento block draws (_3.2 section 5.6, D4), pure: the tiles an author listed, less the ones that lead nowhere. A tile is a
    // BlockRef (the block it leads to), optional words, an optional picture and what a tap does with the block -- move the card to it, or
    // open it in the full view. BlockRefRule's JumpTargets decide reachability for BOTH (a full view of a block the card does not show
    // would be a page with nothing to bind), so a visitor never gets a tile that does nothing and the Editor warns about the same pick
    // by the same words as every other BlockRef holder.
    public static class BentoTileRule
    {
        // What a tap on a tile does with its block
        public enum Opens
        {
            Jump,
            FullView,
        }

        // One tile to draw: the block it leads to, the words and picture the author gave it ("" = none), and what a tap does
        public readonly struct Tile
        {
            public readonly string Target;
            public readonly string Words;
            public readonly string Picture;
            public readonly Opens Opens;

            public Tile(string target, string words, string picture, Opens opens)
            {
                Target = target;
                Words = words;
                Picture = picture;
                Opens = opens;
            }
        }

        // The stored value of the Opens choice as an Opens: only "full_view" opens the full view, a blank or unknown value moves the card
        public static Opens OpensOf(string value) => value == BuiltInBlocks.NavBentoOpensFullView ? Opens.FullView : Opens.Jump;

        // The tiles of a bento block, in the author's order: a row with no block picked, one that points at the bento block itself, or one the
        // card cannot take the visitor to (`jumpTargets`, BlockBindContext.JumpTargets: a deleted block, the header, a pinned or footer look, a
        // block waiting for Show After Reading) is not a tile. `rows` are the Tiles rows read one by one by the caller (the view's reader)
        public static List<Tile> Tiles(IEnumerable<Tile> rows, string selfKey, ICollection<string> jumpTargets)
        {
            var tiles = new List<Tile>();
            if (rows == null || jumpTargets == null) return tiles;
            foreach (var row in rows)
            {
                if (string.IsNullOrWhiteSpace(row.Target) || row.Target == selfKey || !jumpTargets.Contains(row.Target)) continue;
                tiles.Add(row);
            }
            return tiles;
        }

        // Which tiles span the whole row of the grid (two tiles to a row otherwise): the first one, which leads the block, and a last one that
        // would sit alone in its row -- so the grid never has a hole
        public static bool[] Wide(int tileCount)
        {
            var wide = new bool[tileCount];
            for (int i = 0; i < tileCount; i++)
                // - after the first, tiles pair up as (1, 2), (3, 4)...: the last one is alone when it starts a pair
                wide[i] = i == 0 || (i == tileCount - 1 && (i - 1) % 2 == 0);
            return wide;
        }
    }
}

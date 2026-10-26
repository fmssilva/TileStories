using System.Collections.Generic;

namespace TileStories
{
    // Where references resolve (_3.3 R17.8): the holder block's Page, the other blocks of the same POI (each one's Page, found through its
    // kind) and the POI's shared elements. The shared pool is passed in: it has no stored home before phase 2's Editor, so today it is empty.
    public sealed class ElementScope
    {
        public readonly BlockInstanceData Holder;
        public readonly BlockKindDefinition HolderKind;
        private readonly IReadOnlyList<BlockInstanceData> _poiBlocks;
        private readonly BlockRegistry _blocks;
        public readonly IReadOnlyList<BlockItemData> Shared;

        public ElementScope(BlockInstanceData holder, BlockKindDefinition holderKind, IReadOnlyList<BlockInstanceData> poiBlocks,
            BlockRegistry blocks, IReadOnlyList<BlockItemData> shared)
        {
            Holder = holder;
            HolderKind = holderKind;
            _poiBlocks = poiBlocks ?? System.Array.Empty<BlockInstanceData>();
            _blocks = blocks;
            Shared = shared ?? System.Array.Empty<BlockItemData>();
        }

        // The scope of `holder` on `poi`'s card, its kind and the other blocks' kinds from `blocks`, with no shared elements yet
        public static ElementScope Of(POIData poi, BlockInstanceData holder, BlockRegistry blocks)
        {
            BlockKindDefinition kind = null;
            blocks?.TryGet(holder?.kind, out kind);
            return new ElementScope(holder, kind, poi?.card?.blocks, blocks, null);
        }

        // Whether the POI's card has a block with this key (whatever its kind)
        public bool HasBlock(string blockKey)
        {
            foreach (var block in _poiBlocks)
                if (block != null && block.key == blockKey) return true;
            return false;
        }

        // The Page rows of the POI's block with this key; false when the POI has no such block or its kind composes nothing
        public bool TryGetPageOf(string blockKey, out IReadOnlyList<BlockItemData> rows)
        {
            rows = null;
            if (string.IsNullOrEmpty(blockKey)) return false;
            foreach (var block in _poiBlocks)
            {
                if (block == null || block.key != blockKey) continue;
                if (_blocks == null || !_blocks.TryGet(block.kind, out var kind) || !BlockSurfaces.Composes(kind)) return false;
                rows = BlockSurfaces.PageRows(block, kind);
                return true;
            }
            return false;
        }
    }

    // What a reference row shows (_3.3 R17.8), pure: its target row in its scope -- one level only (a reference to a reference is invalid:
    // no chains, no cycles), and a target that cannot be found is Broken with the reason. The visitor sees nothing for a broken reference;
    // the Editor shows a warning row with Unlink / Remove (S3).
    public static class ElementRefRule
    {
        public enum Problem
        {
            None,
            // The scope is not page / block / shared
            UnknownScope,
            // Scope block: the POI has no block with that key
            MissingBlock,
            // Scope block: that block's kind composes nothing (it has no Page)
            NoPage,
            // No row with that id where the scope points
            MissingRow,
            // The target row is itself a reference
            RefToRef,
        }

        public readonly struct Result
        {
            public readonly BlockItemData Target;
            public readonly Problem Problem;

            public Result(BlockItemData target, Problem problem)
            {
                Target = target;
                Problem = problem;
            }

            public bool IsBroken => Problem != Problem.None;
        }

        // The row `reference` points at in `scope`, or Broken with the reason
        public static Result Resolve(ElementRef reference, ElementScope scope)
        {
            IReadOnlyList<BlockItemData> rows;
            switch (reference.Scope)
            {
                case ElementOptions.ScopePage:
                    rows = BlockSurfaces.PageRows(scope?.Holder, scope?.HolderKind);
                    break;
                case ElementOptions.ScopeBlock:
                    if (scope == null || !scope.TryGetPageOf(reference.Block, out rows))
                        return new Result(null, HasBlock(scope, reference.Block) ? Problem.NoPage : Problem.MissingBlock);
                    break;
                case ElementOptions.ScopeShared:
                    rows = scope?.Shared ?? System.Array.Empty<BlockItemData>();
                    break;
                default:
                    return new Result(null, Problem.UnknownScope);
            }
            var target = Find(rows, reference.Row);
            if (target == null) return new Result(null, Problem.MissingRow);
            if (new ElementRow(target).IsRef) return new Result(null, Problem.RefToRef);
            return new Result(target, Problem.None);
        }

        // The row itself for an element row, its target for a reference row (Broken when that fails)
        public static Result ContentOf(BlockItemData row, ElementScope scope)
        {
            var element = new ElementRow(row);
            return element.IsRef ? Resolve(element.Ref, scope) : new Result(row, Problem.None);
        }

        private static BlockItemData Find(IReadOnlyList<BlockItemData> rows, string id)
        {
            if (rows == null || string.IsNullOrEmpty(id)) return null;
            foreach (var row in rows)
                if (row != null && new ElementRow(row).Id == id) return row;
            return null;
        }

        private static bool HasBlock(ElementScope scope, string key)
        {
            if (scope == null || string.IsNullOrEmpty(key)) return false;
            return scope.HasBlock(key);
        }
    }
}

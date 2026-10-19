using System.Collections.Generic;

namespace TileStories
{
    // The card's block views, pooled per kind (work plan rule 2: never instantiate-and-destroy per tap; _3.1 section 3). Take hands out a
    // free view of a kind -- one made before, else a new one from the registry -- and Return gives it back for the next bind. Since _3.2 step
    // 12 two owners take from the ONE pool: the stack (a block's view in its slot, an expandable block's only once it is opened) and the
    // full view (a SECOND view of a kind the stack may be showing at the same time), so the pool never grows past what is on screen at once.
    public sealed class BlockViewPool
    {
        private readonly BlockRegistry _registry;
        private readonly Dictionary<string, Stack<IBlockView>> _free = new();

        public BlockViewPool(BlockRegistry registry) => _registry = registry;

        // How many views were ever built (pooling makes this stop growing once every kind was seen)
        public int CreatedCount { get; private set; }

        // A free view of `kind` (null when the registry has no such kind)
        public IBlockView Take(string kind)
        {
            if (kind != null && _free.TryGetValue(kind, out var stack) && stack.Count > 0) return stack.Pop();
            var view = _registry.CreateView(kind);
            if (view != null) CreatedCount++;
            return view;
        }

        // Give back a view of `kind` the caller has unbound and taken off the card
        public void Return(string kind, IBlockView view)
        {
            if (view == null || kind == null) return;
            if (!_free.TryGetValue(kind, out var stack)) _free[kind] = stack = new Stack<IBlockView>();
            stack.Push(view);
        }
    }
}

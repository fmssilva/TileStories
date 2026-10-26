using System.Collections.Generic;

namespace TileStories
{
    // A composed block's element components, pooled per element kind (_3.3 R14.2, the BlockViewPool pattern one level down): Take hands back
    // a component given back earlier, else makes one through the registry; Return keeps it for the next bind. Rows are never made and thrown
    // away per bind, so scrolling past the same story twice builds nothing new.
    public sealed class ElementComponentPool
    {
        private readonly ElementRegistry _registry;
        private readonly Dictionary<string, Stack<IElementComponent>> _free = new();

        // How many components this pool has made in all (what a test reads to see reuse)
        public int CreatedCount { get; private set; }

        public ElementComponentPool(ElementRegistry registry) => _registry = registry;

        // A component of `kind`, or null for an element that has none
        public IElementComponent Take(string kind)
        {
            if (kind != null && _free.TryGetValue(kind, out var stack) && stack.Count > 0) return stack.Pop();
            var made = _registry?.CreateComponent(kind);
            if (made != null) CreatedCount++;
            return made;
        }

        // Give a component back (unbound and out of the hierarchy) for the next Take of its kind
        public void Return(string kind, IElementComponent component)
        {
            if (kind == null || component == null) return;
            if (!_free.TryGetValue(kind, out var stack)) _free[kind] = stack = new Stack<IElementComponent>();
            stack.Push(component);
        }
    }
}

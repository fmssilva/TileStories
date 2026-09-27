using System.Collections.Generic;
using UnityEngine.UIElements;

namespace TileStories
{
    // The blocks of the open card (_3.1 section 3): the header in its own slot, pinned at the top of the sheet
    // (the title stays in view while the rest scrolls; peek shows exactly its top part), and every other block
    // in order in a ScrollView. Views are pooled per kind: switching POIs rebinds, it never instantiates and
    // destroys a view per tap (work plan performance rule 2). Each bound view gets its own ScopedMediaSource over
    // the card's media source: it loads lazily in Bind, and the stack releases the whole scope on unbind.
    public sealed class BlockStackView
    {
        public VisualElement HeaderSlot { get; }
        public ScrollView Scroll { get; }
        // Pinned under the scroll: the blocks whose variant is a footer (BlockKindDefinition.FooterVariants)
        public VisualElement Footer { get; }

        private readonly BlockRegistry _registry;
        private readonly Dictionary<string, Stack<IBlockView>> _pool = new();
        private readonly List<(string Kind, IBlockView View, ScopedMediaSource Media)> _bound = new();

        // How many views were ever built (pooling makes this stop growing once every kind was seen)
        public int CreatedViewCount { get; private set; }

        // The bound views in stack order (the header first)
        public IReadOnlyList<IBlockView> BoundViews
        {
            get
            {
                var views = new List<IBlockView>(_bound.Count);
                foreach (var b in _bound) views.Add(b.View);
                return views;
            }
        }

        // The element the peek stop must show in full: the header's top part (its whole root for another header view)
        public VisualElement PeekPart
        {
            get
            {
                if (_bound.Count == 0 || _bound[0].Kind != BuiltInBlocks.HeaderKind) return HeaderSlot;
                return _bound[0].View is HeaderBlockView header ? header.PeekPart : _bound[0].View.Root;
            }
        }

        public BlockStackView(VisualElement headerParent, VisualElement scrollParent, BlockRegistry registry)
        {
            _registry = registry;
            HeaderSlot = new VisualElement { name = "poi-card-header" };
            HeaderSlot.AddToClassList("poi-card__header");
            headerParent.Add(HeaderSlot);
            // - a phone card scrolls by dragging its content: no desktop scroll bar (it took width and ignored the tokens)
            Scroll = new ScrollView(ScrollViewMode.Vertical)
            {
                name = "poi-card-stack",
                verticalScrollerVisibility = ScrollerVisibility.Hidden,
                horizontalScrollerVisibility = ScrollerVisibility.Hidden,
            };
            Scroll.AddToClassList("poi-card__stack");
            scrollParent.Add(Scroll);
            Footer = new VisualElement { name = "poi-card-footer" };
            Footer.AddToClassList("poi-card__footer");
            scrollParent.Add(Footer);
        }

        // Show these entries (BlockStackBuilder's output), each bound with `context` and its own variant
        public void Bind(IReadOnlyList<BlockStackBuilder.Entry> entries, BlockBindContext context)
        {
            UnbindAll();
            foreach (var entry in entries)
            {
                var view = Take(entry.Definition.Key);
                if (view == null) continue;
                var media = new ScopedMediaSource(context.Media);
                view.Bind(entry.Instance, new BlockBindContext
                {
                    Poi = context.Poi,
                    Taxonomy = context.Taxonomy,
                    Variant = entry.Variant,
                    Language = context.Language,
                    FallbackLanguage = context.FallbackLanguage,
                    Media = media,
                    Strings = context.Strings,
                    MarkerLook = context.MarkerLook,
                    Glossary = context.Glossary,
                    Host = context.Host,
                });
                bool header = _bound.Count == 0 && entry.Definition.Key == BuiltInBlocks.HeaderKind;
                var slot = header ? HeaderSlot : entry.Definition.IsFooter(entry.Variant) ? Footer : Scroll.contentContainer;
                slot.Add(view.Root);
                _bound.Add((entry.Definition.Key, view, media));
            }
            Scroll.scrollOffset = UnityEngine.Vector2.zero;
        }

        // Unbind every view and put it back in its kind's pool
        public void UnbindAll()
        {
            foreach (var (kind, view, media) in _bound)
            {
                view.Unbind();
                media.ReleaseAll();
                view.Root.RemoveFromHierarchy();
                if (!_pool.TryGetValue(kind, out var stack)) _pool[kind] = stack = new Stack<IBlockView>();
                stack.Push(view);
            }
            _bound.Clear();
        }

        private IBlockView Take(string kind)
        {
            if (_pool.TryGetValue(kind, out var stack) && stack.Count > 0) return stack.Pop();
            var view = _registry.CreateView(kind);
            if (view != null) CreatedViewCount++;
            return view;
        }
    }
}

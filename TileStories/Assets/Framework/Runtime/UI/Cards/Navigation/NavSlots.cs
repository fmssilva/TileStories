using System.Collections.Generic;
using UnityEngine.UIElements;

namespace TileStories
{
    // The navigation slots of the card's sheet (_3.2 section 4.2) and the element each one shows. Today the TOP slot only: a strip in
    // the sheet's root between the header (with its pinned top part) and the stack's ScrollView -- outside the scroll, so a jump
    // lands a block at the viewport's top with no offset for it, and under the header's peek part, so it never moves a stop. Which
    // element a slot holds is NavSlotRule's; WHEN a held element shows (the stop, the scroll, the room) is CardChromeRule's, applied
    // by the sheet's CardChromeView. The element is made by the NavElementRegistry and pooled per key (ElementPool), so a card switch
    // rebinds the same view. The sheet owns one, next to its NavController.
    public sealed class NavSlots
    {
        public VisualElement Top { get; }
        // The element bound in the top slot now (null = none) and its key
        public INavElementView TopView { get; private set; }
        public string TopKey { get; private set; }

        private readonly NavElementRegistry _registry;
        private readonly Dictionary<string, ElementPool<INavElementView>> _pools = new();

        // `sheetRoot`: the sheet's root; `stackScroll`: the stack's ScrollView inside it, which the top slot goes right before
        public NavSlots(VisualElement sheetRoot, VisualElement stackScroll, NavElementRegistry registry)
        {
            _registry = registry;
            Top = new VisualElement { name = "poi-card-nav-top" };
            Top.AddToClassList("poi-card__nav-top");
            sheetRoot.Insert(sheetRoot.IndexOf(stackScroll), Top);
            Top.style.display = DisplayStyle.None;
        }

        // Show this card's element in each slot (none when the rule says so), bound to its model and controller
        public void Bind(CardNavModel model, NavController nav, BlockBindContext context)
        {
            Unbind();
            if (model == null) return;
            string key = NavSlotRule.TopElement(context?.Settings?.navigation, model, TopElementOf);
            if (key == null) return;
            var view = PoolOf(key).Take();
            if (view == null) return;
            view.Bind(model, nav, context);
            Top.Add(view.Root);
            TopView = view;
            TopKey = key;
            Top.style.display = DisplayStyle.Flex;
        }

        // Every slot empty again (the card closed, or is rebinding)
        public void Unbind()
        {
            if (TopKey != null) _pools[TopKey].ReleaseAll();
            TopView = null;
            TopKey = null;
            Top.style.display = DisplayStyle.None;
        }

        // The element registered under this key for the top slot, or null
        private NavElementDefinition TopElementOf(string key) =>
            _registry.TryGet(key, out var definition) && definition.Slot == NavSlotKeys.Top ? definition : null;

        private ElementPool<INavElementView> PoolOf(string key)
        {
            if (!_pools.TryGetValue(key, out var pool))
                _pools[key] = pool = new ElementPool<INavElementView>(_ => _registry.CreateView(key), view =>
                {
                    view.Unbind();
                    view.Root.RemoveFromHierarchy();
                });
            return pool;
        }
    }
}

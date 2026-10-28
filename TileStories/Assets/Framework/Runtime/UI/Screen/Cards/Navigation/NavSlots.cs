using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace TileStories
{
    // The navigation slots of the card's sheet (_3.2 section 4.2) and the element each one shows:
    //   - TOP: a strip in the sheet's root between the header (with its pinned top part) and the stack's ScrollView -- outside the scroll,
    //     so a jump lands a block at the viewport's top with no offset for it, and under the header's peek part, so it never moves a stop
    //   - BOTTOM (step 7a): the last part of the sheet's bottom area, under the sticky CTA footer -- the CTA stays nearest the content, the
    //     bar sits in the thumb zone. Below the header too, so it never moves a stop either
    //   - MENU (step 8): no place in the sheet -- the element is bound here like the others and the card's overlay owner (CardMenuOverlay)
    //     draws it over the card, on a scrim, when the header's menu button (or a bar's More) opens it
    //   - FLOATING (step 9): the right end of the card's FloatingSlot, a row in the card layer just above the sheet's top-right edge at every
    //     stop (outside the sheet, which clips): it costs no reading space, so the chrome never hides it
    //   - EDGE (step 10): a host INSIDE the stack's viewport (the ScrollView's contentViewport), absolute over its right edge: its box is
    //     exactly what the visitor can scroll, so it follows the chrome with no geometry of its own, takes no room in the flow, and the
    //     element decides for itself whether the viewport is tall enough to draw (EdgeScrubberRule.Fits)
    // Which element a slot holds is NavSlotRule's; WHEN a held element shows (the stop, the scroll, the room) is CardChromeRule's, applied
    // by the sheet's CardChromeView. An element is made by the NavElementRegistry and pooled per key (ElementPool), so a card switch
    // rebinds the same view. The sheet owns one, next to its NavController.
    public sealed class NavSlots
    {
        public VisualElement Top { get; }
        public VisualElement Bottom { get; }
        // The element bound in each slot now (null = none) and its key
        public INavElementView TopView { get; private set; }
        public string TopKey { get; private set; }
        public INavElementView BottomView { get; private set; }
        public string BottomKey { get; private set; }
        // The MENU element (step 8): bound like the others, but never parented here -- the card's overlay owner (CardMenuOverlay) draws its
        // root on the scrim while it is open, and the header's menu button exists only while one is bound
        public INavElementView MenuView { get; private set; }
        public string MenuKey { get; private set; }
        // The FLOATING element (step 9), drawn in the floating slot's element host
        public VisualElement Floating { get; }
        public INavElementView FloatingView { get; private set; }
        public string FloatingKey { get; private set; }
        // The EDGE element (step 10), drawn in the host over the stack viewport's right edge
        public VisualElement Edge { get; }
        public INavElementView EdgeView { get; private set; }
        public string EdgeKey { get; private set; }
        // The Navigation Bar (_3.3 S4a review): one view, bound into the top or the bottom host while card_settings.navigation.bar is on --
        // the card then draws no other navigation element (their settings are kept). TopView / BottomView name it while it is there
        public NavBarView Bar { get; } = new NavBarView();
        public const string TopBarClass = "poi-card__nav-top--bar";
        public const string BottomBarClass = "poi-card__nav-bottom--bar";
        // On the sheet's root while the bar floats at the bottom: the stack keeps its end clear of it (PoiCard.uss)
        public const string SheetBarBottomClass = "poi-card--bar-bottom";

        private readonly NavElementRegistry _registry;
        private readonly VisualElement _sheetRoot;
        private readonly Dictionary<string, ElementPool<INavElementView>> _pools = new();

        // `sheetRoot`: the sheet's root; `stackScroll`: the stack's ScrollView inside it, which the top slot goes right before and whose
        // viewport the edge slot's host lies over;
        // `bottomArea`: the sheet's bottom area (the footer first), which the bottom slot closes; `floating`: the floating slot's element host
        public NavSlots(VisualElement sheetRoot, ScrollView stackScroll, VisualElement bottomArea, VisualElement floating, NavElementRegistry registry)
        {
            _registry = registry;
            _sheetRoot = sheetRoot;
            Floating = floating;
            Top = new VisualElement { name = "poi-card-nav-top" };
            Top.AddToClassList("poi-card__nav-top");
            sheetRoot.Insert(sheetRoot.IndexOf(stackScroll), Top);
            Top.style.display = DisplayStyle.None;
            Bottom = new VisualElement { name = "poi-card-nav-bottom" };
            Bottom.AddToClassList("poi-card__nav-bottom");
            bottomArea.Add(Bottom);
            Bottom.style.display = DisplayStyle.None;
            // - after the content container, so it paints and picks over the blocks; never takes a finger itself, only the element in it does
            Edge = new VisualElement { name = "poi-card-nav-edge", pickingMode = PickingMode.Ignore };
            Edge.AddToClassList("poi-card__nav-edge");
            stackScroll.contentViewport.Add(Edge);
            Edge.style.display = DisplayStyle.None;
        }

        // Show this card's element in each slot (none when the rule says so), bound to its model and controller
        public void Bind(CardNavModel model, NavController nav, BlockBindContext context)
        {
            Unbind();
            if (model == null) return;
            var settings = context?.Settings?.navigation;
            if (NavBarRule.Enabled(settings))
            {
                BindBar(model, nav, context, settings);
                return;
            }
            var top = BindSlot(Top, NavSlotRule.TopElement(settings, model, ElementOf(NavSlotKeys.Top)), model, nav, context);
            TopView = top.View;
            TopKey = top.Key;
            var bottom = BindSlot(Bottom, NavSlotRule.BottomElement(settings, model, ElementOf(NavSlotKeys.Bottom)), model, nav, context);
            BottomView = bottom.View;
            BottomKey = bottom.Key;
            var floating = BindSlot(Floating, NavSlotRule.FloatingElement(settings, model, ElementOf(NavSlotKeys.Floating)), model, nav, context);
            FloatingView = floating.View;
            FloatingKey = floating.Key;
            var edge = BindSlot(Edge, NavSlotRule.EdgeElement(settings, model, ElementOf(NavSlotKeys.Edge)), model, nav, context);
            EdgeView = edge.View;
            EdgeKey = edge.Key;
            string menuKey = NavSlotRule.MenuElement(settings, model, ElementOf(NavSlotKeys.Menu));
            if (menuKey != null && PoolOf(menuKey).Take() is { } menu)
            {
                menu.Bind(model, nav, context);
                MenuView = menu;
                MenuKey = menuKey;
            }
        }

        // The bar in its host (the top one under the chrome row, or the bottom one, floating over the stack's foot), when the card shows
        // navigation and two groups at least
        private void BindBar(CardNavModel model, NavController nav, BlockBindContext context, CardNavigationSettings settings)
        {
            Bar.Bind(model, nav, context);
            if (!NavBarRule.Shows(settings, model.ShowsNavigation, Bar.Entries.Count))
            {
                Bar.Unbind();
                return;
            }
            bool bottom = NavBarRule.AtBottom(settings.bar);
            var host = bottom ? Bottom : Top;
            host.Add(Bar.Root);
            host.style.display = DisplayStyle.Flex;
            Top.EnableInClassList(TopBarClass, !bottom);
            Bottom.EnableInClassList(BottomBarClass, bottom);
            _sheetRoot.EnableInClassList(SheetBarBottomClass, bottom);
            if (bottom)
            {
                BottomView = Bar;
                BottomKey = NavBarView.ElementKey;
            }
            else
            {
                TopView = Bar;
                TopKey = NavBarView.ElementKey;
            }
        }

        // Every slot empty again (the card closed, or is rebinding)
        public void Unbind()
        {
            bool barBound = TopKey == NavBarView.ElementKey || BottomKey == NavBarView.ElementKey;
            if (barBound)
            {
                Bar.Unbind();
                Bar.Root.RemoveFromHierarchy();
                if (TopKey == NavBarView.ElementKey) TopKey = null;
                if (BottomKey == NavBarView.ElementKey) BottomKey = null;
            }
            Top.RemoveFromClassList(TopBarClass);
            Bottom.RemoveFromClassList(BottomBarClass);
            _sheetRoot.RemoveFromClassList(SheetBarBottomClass);
            if (TopKey != null) _pools[TopKey].ReleaseAll();
            if (BottomKey != null) _pools[BottomKey].ReleaseAll();
            if (MenuKey != null) _pools[MenuKey].ReleaseAll();
            if (FloatingKey != null) _pools[FloatingKey].ReleaseAll();
            if (EdgeKey != null) _pools[EdgeKey].ReleaseAll();
            TopView = null;
            TopKey = null;
            BottomView = null;
            BottomKey = null;
            MenuView = null;
            MenuKey = null;
            FloatingView = null;
            FloatingKey = null;
            EdgeView = null;
            EdgeKey = null;
            Top.style.display = DisplayStyle.None;
            Bottom.style.display = DisplayStyle.None;
            Floating.style.display = DisplayStyle.None;
            Edge.style.display = DisplayStyle.None;
        }

        // One slot: a view of the element `key` names (null = none), bound and shown in `slot`
        private (INavElementView View, string Key) BindSlot(VisualElement slot, string key, CardNavModel model, NavController nav, BlockBindContext context)
        {
            if (key == null) return (null, null);
            var view = PoolOf(key).Take();
            if (view == null) return (null, null);
            view.Bind(model, nav, context);
            slot.Add(view.Root);
            slot.style.display = DisplayStyle.Flex;
            return (view, key);
        }

        // The element registered under a key for this slot, or null
        private Func<string, NavElementDefinition> ElementOf(string slot) => key =>
            _registry.TryGet(key, out var definition) && definition.Slot == slot ? definition : null;

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

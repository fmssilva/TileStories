using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories
{
    // The Navigation Bar (_3.3 S4a review): ONE bar of the card's groups, as card_settings.navigation.bar configures it -- its shape (floating
    // or full width), its fill (glass, solid, clear, and how much of it), its hairline and shadow, and how the chosen group shows (a pill, an
    // underline or a raised disc that slides between groups; a bolder or larger icon, its own chosen icon; its colour). NOT a registry
    // element: NavSlots binds it into the top host (under the title bar) or the bottom host when the bar is on, and the card draws no other
    // navigation element then. The groups (NavBarRule.Entries) and the chosen one (the controller's CurrentBlock) are the rule's; a tap
    // jumps to the group's first block through the controller. The chosen group is marked by its shape AND its bolder words, never colour alone.
    public sealed class NavBarView : INavElementView
    {
        public const string ElementKey = "nav_bar";
        public const string ItemClass = "nav-bar__item";
        public const string ChosenClass = "nav-bar__item--chosen";

        // One group's button: its icon and its words
        public sealed class Item
        {
            public readonly int Index;
            public readonly Button Button;
            public readonly VisualElement Icon;
            public readonly Label Label;

            public Item(int index, Button button, VisualElement icon, Label label)
            {
                Index = index;
                Button = button;
                Icon = icon;
                Label = label;
            }
        }

        public VisualElement Root { get; }
        public VisualElement Shadow { get; }
        public VisualElement Row { get; }
        public VisualElement Indicator { get; }
        public IReadOnlyList<Item> Items => _items.Shown;
        public IReadOnlyList<NavBarRule.Entry> Entries => _entries;
        // The chosen group (-1 = none)
        public int ChosenIndex { get; private set; } = -1;
        public CardNavBarSettings Settings { get; private set; }

        private readonly ElementPool<Item> _items;
        private List<NavBarRule.Entry> _entries = new();
        private NavController _nav;
        private CardStrings _strings;

        public NavBarView()
        {
            Root = new VisualElement { name = "nav-bar" };
            Root.AddToClassList("nav-bar");
            Shadow = new VisualElement { name = "nav-bar-shadow", pickingMode = PickingMode.Ignore };
            Shadow.AddToClassList("nav-bar__shadow");
            Root.Add(Shadow);
            var ground = new VisualElement { name = "nav-bar-ground", pickingMode = PickingMode.Ignore };
            ground.AddToClassList("nav-bar__ground");
            Root.Add(ground);
            Ground = ground;
            Row = new VisualElement { name = "nav-bar-row" };
            Row.AddToClassList("nav-bar__row");
            Root.Add(Row);
            // - the one indicator: out of the flow, under the items, moved to the chosen one (sliding when the bar says so)
            Indicator = new VisualElement { name = "nav-bar-indicator", pickingMode = PickingMode.Ignore };
            Indicator.AddToClassList("nav-bar__indicator");
            Row.Add(Indicator);
            _items = new ElementPool<Item>(NewItem, item => item.Button.RemoveFromHierarchy());
            // - colours painted inline follow the mode, and the indicator follows the items' layout
            Root.RegisterCallback<AttachToPanelEvent>(_ => { ThemeController.ModeChanged -= Paint; ThemeController.ModeChanged += Paint; Paint(); });
            Root.RegisterCallback<DetachFromPanelEvent>(_ => ThemeController.ModeChanged -= Paint);
            Row.RegisterCallback<GeometryChangedEvent>(_ => PlaceIndicator());
        }

        // The bar's fill: the ground under the items (the raised disc takes its colour)
        public VisualElement Ground { get; }

        public void Bind(CardNavModel model, NavController nav, BlockBindContext context)
        {
            Unbind();
            _nav = nav;
            _strings = context?.Strings;
            var navSettings = context?.Settings?.navigation;
            Settings = navSettings?.bar ?? new CardNavBarSettings();
            var registry = BlockRegistry.Shared;
            var items = NavBarRule.Items(navSettings, registry);
            _entries = NavBarRule.Entries(items, model?.Targets, block =>
                nav != null && nav.TryGetEntry(block, out var entry) ? entry.Definition.Family : null);
            ApplyLook();
            for (int i = 0; i < _entries.Count; i++)
            {
                var item = _items.Take();
                item.Label.text = NavBarRule.Words(_entries[i].Item, _strings);
                // - an icon alone still names its group (tooltip: what a screen reader and a long press read)
                item.Button.tooltip = item.Label.text;
                Row.Add(item.Button);
            }
            if (nav != null)
            {
                nav.CurrentBlockChanged += OnCurrentBlockChanged;
                OnCurrentBlockChanged(nav.CurrentBlock);
            }
            else Choose(-1);
        }

        public void Unbind()
        {
            if (_nav != null) _nav.CurrentBlockChanged -= OnCurrentBlockChanged;
            _nav = null;
            _items.ReleaseAll();
            _entries = new List<NavBarRule.Entry>();
            ChosenIndex = -1;
        }

        // A tap on a group: the card moves to its first block
        public bool Tap(int index)
        {
            if (index < 0 || index >= _entries.Count || _nav == null) return false;
            return _nav.JumpToBlock(_entries[index].FirstTarget, ElementKey);
        }

        private Item NewItem(int index)
        {
            var button = new Button(() => Tap(index)) { name = "nav-bar-item-" + index };
            button.AddToClassList(ItemClass);
            var icon = CardIcons.Create();
            icon.AddToClassList("nav-bar__icon");
            button.Add(icon);
            var label = new Label { pickingMode = PickingMode.Ignore };
            label.AddToClassList("nav-bar__label");
            button.Add(label);
            return new Item(index, button, icon, label);
        }

        private void OnCurrentBlockChanged(string block)
        {
            int index = NavBarRule.EntryOf(_entries, block);
            // - a block of no group (a family the bar leaves out) keeps the last mark: the visitor is still under that group's map
            if (index < 0 && block != null && ChosenIndex >= 0) return;
            Choose(index);
        }

        // Mark one group: its class, its chosen icon, its words' display, its colour, and the indicator under it
        private void Choose(int index)
        {
            ChosenIndex = index;
            string labels = NavBarRule.Labels(Settings);
            foreach (var item in _items.Shown)
            {
                bool chosen = item.Index == index;
                var entry = _entries[item.Index];
                item.Button.EnableInClassList(ChosenClass, chosen);
                string key = chosen ? entry.Item.SelectedIcon : entry.Item.Icon;
                bool hasIcon = !string.IsNullOrEmpty(key);
                CardIcons.SetKey(item.Icon, key);
                item.Icon.style.display = NavBarRule.ShowsIcon(labels, hasIcon) ? DisplayStyle.Flex : DisplayStyle.None;
                item.Label.style.display = NavBarRule.ShowsWords(labels, hasIcon, chosen) ? DisplayStyle.Flex : DisplayStyle.None;
            }
            Paint();
            PlaceIndicator();
        }

        // The bar's look from its settings: classes for what USS draws, inline numbers for what the developer sets
        private void ApplyLook()
        {
            var bar = Settings;
            string shape = NavBarRule.Shape(bar), fill = NavBarRule.Fill(bar), indicator = NavBarRule.Indicator(bar);
            foreach (string option in CardOptions.NavBarShapes) Root.EnableInClassList("nav-bar--" + option, option == shape);
            foreach (string option in CardOptions.NavBarFills) Root.EnableInClassList("nav-bar--fill-" + option, option == fill);
            foreach (string option in CardOptions.NavBarIndicators) Root.EnableInClassList("nav-bar--indicator-" + option, option == indicator);
            Root.EnableInClassList("nav-bar--bottom", NavBarRule.AtBottom(bar));
            Root.EnableInClassList("nav-bar--border", bar.border && fill != CardOptions.NavBarClear);
            Root.EnableInClassList("nav-bar--shadow", bar.shadow && fill != CardOptions.NavBarClear);
            Root.EnableInClassList("nav-bar--slide", bar.slide_indicator);
            Root.EnableInClassList("nav-bar--bold", bar.bold_selected);
            Root.EnableInClassList("nav-bar--enlarge", bar.enlarge_selected);
            float radius = NavBarRule.CornerRadius(bar), margin = NavBarRule.SideMargin(bar), height = NavBarRule.Height(bar);
            Root.style.marginLeft = margin;
            Root.style.marginRight = margin;
            Row.style.height = height;
            foreach (var part in new[] { Root, Ground, Shadow })
            {
                part.style.borderTopLeftRadius = radius;
                part.style.borderTopRightRadius = radius;
                part.style.borderBottomLeftRadius = radius;
                part.style.borderBottomRightRadius = radius;
            }
        }

        // The colours: the ground (the theme's bar colour for the fill, its opacity kept by the setting's share), each group's icon and words
        // (chosen: its colour as the setting says; the others muted or their own), the indicator (a soft wash of the chosen colour)
        private void Paint()
        {
            var bar = Settings;
            if (bar == null) return;
            string fill = NavBarRule.Fill(bar);
            string groundToken = fill == CardOptions.NavBarSolid ? "--ts-bar" : fill == CardOptions.NavBarGlass ? "--ts-bar-glass" : null;
            if (groundToken != null && CardTokenColours.TryResolve(Root, groundToken, out Color ground))
            {
                ground.a *= NavBarRule.FillOpacity(bar);
                Ground.style.backgroundColor = ground;
            }
            else Ground.style.backgroundColor = StyleKeyword.Null;

            string selected = NavBarRule.SelectedColour(bar), idle = NavBarRule.IdleColour(bar);
            Color? chosenColour = null;
            foreach (var item in _items.Shown)
            {
                bool chosen = item.Index == ChosenIndex;
                string token = chosen ? SelectedToken(selected, _entries[item.Index].Item)
                    : idle == CardOptions.NavBarColourGroup ? _entries[item.Index].Item.ColorToken : "--ts-ink-2";
                bool resolved = CardTokenColours.TryResolve(Root, token, out Color colour);
                CardIcons.Tint(item.Icon, resolved ? colour : (Color?)null);
                // - the words stay ink (>= 7:1 on the bar); only the chosen group's take its colour when that is the ink or the accent text
                item.Label.style.color = chosen && resolved && selected != CardOptions.NavBarColourGroup ? new StyleColor(colour) : new StyleColor(StyleKeyword.Null);
                if (chosen && resolved) chosenColour = colour;
            }
            string indicator = NavBarRule.Indicator(bar);
            if (indicator == CardOptions.NavBarIndicatorRaised)
            {
                // - the raised disc is the bar lifting under the chosen icon: the bar's own ground, opaque enough to read as one piece
                var disc = Ground.resolvedStyle.backgroundColor;
                if (CardTokenColours.TryResolve(Root, fill == CardOptions.NavBarClear ? "--ts-bar-glass" : groundToken, out Color fillColour)) disc = fillColour;
                disc.a = Mathf.Max(disc.a, 0.92f);
                Indicator.style.backgroundColor = disc;
            }
            else if (chosenColour.HasValue)
            {
                var wash = chosenColour.Value;
                wash.a = indicator == CardOptions.NavBarIndicatorUnderline ? 1f : 0.16f;
                Indicator.style.backgroundColor = wash;
            }
            else Indicator.style.backgroundColor = StyleKeyword.Null;
        }

        private static string SelectedToken(string selected, NavBarRule.Item item) =>
            selected == CardOptions.NavBarColourAccent ? "--ts-accent" : selected == CardOptions.NavBarColourInk ? "--ts-ink" : item.ColorToken ?? "--ts-accent";

        // The indicator under the chosen group's button (none chosen, or the None indicator: hidden). Its box per kind is USS's (height,
        // radius); its place and width are the chosen button's
        private void PlaceIndicator()
        {
            string indicator = NavBarRule.Indicator(Settings);
            bool shown = indicator != CardOptions.NavBarIndicatorNone && ChosenIndex >= 0 && ChosenIndex < _items.Shown.Count;
            Indicator.style.display = shown ? DisplayStyle.Flex : DisplayStyle.None;
            if (!shown) return;
            var box = _items.Shown[ChosenIndex].Button.layout;
            if (float.IsNaN(box.width) || box.width <= 0f) return;
            float width, left;
            if (indicator == CardOptions.NavBarIndicatorUnderline)
            {
                width = Mathf.Min(box.width * 0.4f, 28f);
                left = box.center.x - width * 0.5f;
            }
            else if (indicator == CardOptions.NavBarIndicatorRaised)
            {
                width = Mathf.Min(box.width, NavBarRule.Height(Settings));
                left = box.center.x - width * 0.5f;
                Indicator.style.height = width;
                Indicator.style.borderTopLeftRadius = Indicator.style.borderTopRightRadius =
                    Indicator.style.borderBottomLeftRadius = Indicator.style.borderBottomRightRadius = width * 0.5f;
            }
            else
            {
                width = box.width;
                left = box.x;
            }
            if (indicator != CardOptions.NavBarIndicatorRaised)
            {
                // - the raised disc's own box let go: the pill and the underline take theirs from USS
                Indicator.style.height = StyleKeyword.Null;
                Indicator.style.borderTopLeftRadius = Indicator.style.borderTopRightRadius =
                    Indicator.style.borderBottomLeftRadius = Indicator.style.borderBottomRightRadius = StyleKeyword.Null;
            }
            Indicator.style.left = left;
            Indicator.style.width = width;
        }
    }
}

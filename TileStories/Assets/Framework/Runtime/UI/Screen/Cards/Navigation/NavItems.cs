using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories
{
    // One tab of a tab element (text_tabs, icon_tabs): the button (its whole area is the tap target), an optional family icon, its words,
    // an optional block count and the underline that marks the current one. The current tab is marked twice -- underline AND bolder words
    // (NavTab.CurrentClass) -- never by colour alone.
    public sealed class NavTab
    {
        public const string CurrentClass = "nav-tabs__tab--current";

        public readonly int Index;
        public readonly Button Button;
        // Null on a text tab
        public readonly VisualElement Icon;
        public readonly Label Label;
        public readonly Label Count;
        public readonly VisualElement Underline;

        private NavTab(int index, Button button, VisualElement icon, Label label, Label count, VisualElement underline)
        {
            Index = index;
            Button = button;
            Icon = icon;
            Label = label;
            Count = count;
            Underline = underline;
        }

        // A new tab named "<namePrefix><index>"; `withIcon` gives it the icon above its words (icon_tabs)
        public static NavTab Create(string namePrefix, int index, bool withIcon, Action clicked)
        {
            var button = new Button(clicked) { name = namePrefix + index };
            button.AddToClassList("nav-tabs__tab");
            VisualElement icon = null;
            if (withIcon)
            {
                button.AddToClassList("nav-tabs__tab--icon");
                icon = CardIcons.Create();
                icon.AddToClassList("nav-tabs__icon");
                button.Add(icon);
            }
            var words = new VisualElement { pickingMode = PickingMode.Ignore };
            words.AddToClassList("nav-tabs__words");
            var label = new Label { pickingMode = PickingMode.Ignore };
            label.AddToClassList("nav-tabs__label");
            var count = NavCount.Create();
            words.Add(label);
            words.Add(count);
            var underline = new VisualElement { pickingMode = PickingMode.Ignore };
            underline.AddToClassList("nav-tabs__underline");
            button.Add(words);
            button.Add(underline);
            return new NavTab(index, button, icon, label, count, underline);
        }
    }

    // One chip of a chip row (kind_chips, icon_tabs_chips): a kind's words with its family's colour as a DOT beside them (never the only
    // signal: the words name the kind) and an optional block count. The current chip has a border and bolder words (CurrentClass).
    public sealed class NavChip
    {
        public const string CurrentClass = "nav-chip--current";

        public readonly int Index;
        public readonly Button Button;
        public readonly VisualElement Dot;
        public readonly Label Label;
        public readonly Label Count;
        // The kind this chip stands for now (CardNavModel.Kind) -- set by the row on every bind
        public CardNavModel.Kind Kind { get; set; }

        private NavChip(int index, Button button, VisualElement dot, Label label, Label count)
        {
            Index = index;
            Button = button;
            Dot = dot;
            Label = label;
            Count = count;
        }

        public static NavChip Create(string namePrefix, int index, Action clicked)
        {
            // - the button is the whole tap target (the touch token both ways); the pill inside it is what the chip looks like, so a
            //   chip reads compact while a finger still gets a full target
            var button = new Button(clicked) { name = namePrefix + index };
            button.AddToClassList("nav-chip");
            var pill = new VisualElement { pickingMode = PickingMode.Ignore };
            pill.AddToClassList("nav-chip__pill");
            var dot = new VisualElement { pickingMode = PickingMode.Ignore };
            dot.AddToClassList("nav-chip__dot");
            var label = new Label { pickingMode = PickingMode.Ignore };
            label.AddToClassList("nav-chip__label");
            var count = NavCount.Create();
            pill.Add(dot);
            pill.Add(label);
            pill.Add(count);
            button.Add(pill);
            return new NavChip(index, button, dot, label, count);
        }
    }

    // The top of a menu over the card (drawer_tree; _3.2 step 8): its title and a visible close X -- a menu on a scrim keeps a
    // way out the visitor can see, not only Back and the scrim (NN/g bottom sheets, Laubheimer 2023). The X is the card's own (two crossed
    // bars, PoiCard.uss), named by CardStrings' Close
    public sealed class NavMenuHeader
    {
        public readonly VisualElement Root;
        public readonly Label Title;
        public readonly Button Close;

        public NavMenuHeader(string namePrefix, Action close)
        {
            Root = new VisualElement { name = namePrefix + "-header" };
            Root.AddToClassList("nav-menu__header");
            Title = new Label { name = namePrefix + "-title", pickingMode = PickingMode.Ignore };
            Title.AddToClassList("nav-menu__title");
            Close = new Button(close) { name = namePrefix + "-close" };
            Close.AddToClassList("nav-menu__close");
            foreach (string bar in new[] { "poi-card__close-bar--a", "poi-card__close-bar--b" })
            {
                var stroke = new VisualElement { pickingMode = PickingMode.Ignore };
                stroke.AddToClassList("poi-card__close-bar");
                stroke.AddToClassList(bar);
                Close.Add(stroke);
            }
            Root.Add(Title);
            Root.Add(Close);
        }

        // The menu's title and the X's name, in the card's language
        public void Words(CardStrings strings)
        {
            Title.text = strings?.Get(CardStrings.Keys.NavMenu) ?? "";
            Close.tooltip = strings?.Get(CardStrings.Keys.Close) ?? "";
        }
    }

    // The block count an item shows with Show Counts on (a number: no visitor words)
    public static class NavCount
    {
        public static Label Create()
        {
            var count = new Label { pickingMode = PickingMode.Ignore };
            count.AddToClassList("nav-item__count");
            return count;
        }

        public static void Set(Label count, bool shown, int n)
        {
            count.text = shown ? n.ToString() : "";
            count.style.display = shown ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    // Paints parts of an element in colours named by theme TOKENS (a family's ColorToken), read from the theme the element sits under
    // (CardTokenColours) -- so the values stay data. A token resolves only once the element sits under the app's marked root, so the
    // paint is (re)applied when the element is attached and every time it is laid out.
    public sealed class NavTint
    {
        private enum Paint { Icon, Fill, Ink }

        private readonly VisualElement _root;
        private readonly List<(VisualElement Target, string Token, Paint How)> _parts = new();

        public NavTint(VisualElement root)
        {
            _root = root;
            // - a colour painted inline does not follow a mode class: it paints again when the mode changes (only while on a panel)
            root.RegisterCallback<AttachToPanelEvent>(_ => { ThemeController.ModeChanged -= Apply; ThemeController.ModeChanged += Apply; Apply(); });
            root.RegisterCallback<DetachFromPanelEvent>(_ => ThemeController.ModeChanged -= Apply);
            if (root.panel != null) ThemeController.ModeChanged += Apply;
            root.RegisterCallback<GeometryChangedEvent>(_ => Apply());
        }

        public void Clear() => _parts.Clear();

        // An icon made by CardIcons.Create, in the token's colour (null token = the stylesheet's own). Painting an icon again replaces its
        // colour (a bar's More slot takes the colour of the section it stands for, and back)
        public void Icon(VisualElement icon, string token)
        {
            _parts.RemoveAll(part => part.Target == icon);
            _parts.Add((icon, token, Paint.Icon));
            Apply();
        }

        // An element's background, in the token's colour (a chip's dot, a progress segment's bar)
        public void Fill(VisualElement element, string token)
        {
            _parts.Add((element, token, Paint.Fill));
            Apply();
        }

        // A text's colour, in the token's colour (the words on a family's filled pill: the section opener)
        public void Ink(VisualElement text, string token)
        {
            _parts.Add((text, token, Paint.Ink));
            Apply();
        }

        public void Apply()
        {
            foreach (var (target, token, how) in _parts)
            {
                bool resolved = CardTokenColours.TryResolve(_root, token, out Color colour);
                var paint = resolved ? new StyleColor(colour) : new StyleColor(StyleKeyword.Null);
                if (how == Paint.Icon) CardIcons.Tint(target, resolved ? colour : (Color?)null);
                else if (how == Paint.Ink) target.style.color = paint;
                else target.style.backgroundColor = paint;
            }
        }
    }
}

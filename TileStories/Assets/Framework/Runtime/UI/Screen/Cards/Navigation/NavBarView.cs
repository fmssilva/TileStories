using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories
{
    // The Navigation Bar (_3.3 S4 third round, v5): ONE bar of the card's groups, as card_settings.navigation.bar configures it. NOT a registry
    // element: NavSlots binds it into the bottom host (floating over the screen's foot) or the top host (under the chrome row) while the bar
    // is on, and the card draws no other navigation element then.
    //   - the SURFACE paints the bar as ONE path (NavBarShapeRule, Painter2D in generateVisualContent): its fill, its hairline and -- under
    //     Solid and Floating only -- its shadow; under Raised its edge rises into one smooth bell at the chosen group, its peak the disc's
    //     top (hanging down at Top)
    //   - the DISC carries the chosen icon: riding up in the bump (Raised), or at the icon's own place (Filled); the pill / underline sit
    //     behind / under the chosen group; all of them follow ONE animated centre that slides with v5's overshoot (Reduce Motion: at once)
    //   - over the hero a non-solid bar turns white on dark glass (SetOverHero, the sheet's call); Clear is transparent there, solid after it
    // The groups (NavBarRule.Entries) and the chosen one (the controller's CurrentBlock) are the rule's; a tap jumps to the group's first block
    // through the controller. The chosen group is marked by its shape AND its bolder words, never colour alone.
    public sealed class NavBarView : INavElementView
    {
        public const string ElementKey = "nav_bar";
        public const string ItemClass = "nav-bar__item";
        public const string ChosenClass = "nav-bar__item--chosen";
        public const string OverHeroClass = "nav-bar--over-hero";

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
        // The painted bar (fill, hairline, shadow, the bump)
        public VisualElement Surface { get; }
        public VisualElement Row { get; }
        public VisualElement Pill { get; }
        public VisualElement Underline { get; }
        public VisualElement Disc { get; }
        public VisualElement DiscIcon { get; }
        public IReadOnlyList<Item> Items => _items.Shown;
        public IReadOnlyList<NavBarRule.Entry> Entries => _entries;
        // The chosen group (-1 = none)
        public int ChosenIndex { get; private set; } = -1;
        public CardNavBarSettings Settings { get; private set; }
        // Whether the bar sits over the hero's picture now (the sheet decides, SetOverHero)
        public bool OverHero { get; private set; }

        // What was painted last, for tests and the look: the path (in the bar's box), its fill and hairline, the shadow, the bump's centre
        public IReadOnlyList<NavBarShapeRule.Command> PaintedPath { get; private set; } = new List<NavBarShapeRule.Command>();
        public Color PaintedFill { get; private set; }
        public Color PaintedLine { get; private set; }
        public bool PaintedShadow { get; private set; }
        // The chosen group's centre the indicators follow (in the bar's box), now and where it slides to
        public float Centre { get; private set; } = float.NaN;
        public float CentreTarget { get; private set; } = float.NaN;
        public bool Sliding => _slide != null && _sliding;
        // Reduce Motion: the indicators jump; the time the slide reads (real time unless a test hands its own)
        public bool ReduceMotion { get; set; }
        public System.Func<float> Clock { get; set; } = () => Time.unscaledTime;

        // The bar's numbers (CardTokens.uss --ts-navbar-*, read on the card root)
        public float BarHeight { get; private set; }
        public float Room { get; private set; }
        public float BarWidth => Root.layout.width;
        public float DiscSize { get; private set; }
        // How far the edge rises at the chosen group (or hangs, at Top): Raised Depth's share of the raised disc's own size -- below 0 a
        // cradle dipping into the bar (NavBarShapeRule.EdgeRise)
        public float BumpHeight => NavBarShapeRule.EdgeRise(NavBarRule.RaisedDepth(Settings), RaisedDiscRadius, Token(CradleGapToken));
        // Half the bump's foot: how far its soft shoulders reach from the disc's centre
        public float BumpHalf => Token(BumpHalfToken);
        // The raised disc's radius: the bell's crest is its arc
        public float RaisedDiscRadius => Token(RaisedDiscToken) * 0.5f;
        public float CornerRadius => NavBarShapeRule.Radius(NavBarRule.Floating(Settings), NavBarRule.Raised(Settings), BarHeight, Token(RaisedRadiusToken));

        private static readonly CustomStyleProperty<float> HeightToken = new("--ts-navbar-height");
        private static readonly CustomStyleProperty<float> CompactHeightToken = new("--ts-navbar-height-compact");
        private static readonly CustomStyleProperty<float> InsetToken = new("--ts-navbar-inset");
        private static readonly CustomStyleProperty<float> GapToken = new("--ts-navbar-gap");
        private static readonly CustomStyleProperty<float> RoomToken = new("--ts-navbar-room");
        private static readonly CustomStyleProperty<float> BumpHalfToken = new("--ts-navbar-bump-half");
        private static readonly CustomStyleProperty<float> RaisedRadiusToken = new("--ts-navbar-raised-radius");
        private static readonly CustomStyleProperty<float> RaisedDiscToken = new("--ts-navbar-disc-raised");
        private static readonly CustomStyleProperty<float> FilledDiscToken = new("--ts-navbar-disc-filled");
        private static readonly CustomStyleProperty<float> CradleGapToken = new("--ts-navbar-cradle-gap");

        private readonly ElementPool<Item> _items;
        private List<NavBarRule.Entry> _entries = new();
        private NavController _nav;
        private CardStrings _strings;
        private IVisualElementScheduledItem _slide;
        private bool _sliding;
        private float _slideFrom, _slideStart;

        public NavBarView()
        {
            // - the root spans the bump's room and the bar: only the bar's band (the row) and the disc take a finger, the clear room above
            //   the bar's edge is the card's
            Root = new VisualElement { name = "nav-bar", pickingMode = PickingMode.Ignore };
            Root.AddToClassList("nav-bar");
            Surface = new VisualElement { name = "nav-bar-surface", pickingMode = PickingMode.Ignore };
            Surface.AddToClassList("nav-bar__surface");
            Surface.generateVisualContent += PaintSurface;
            Root.Add(Surface);
            Row = new VisualElement { name = "nav-bar-row" };
            Row.AddToClassList("nav-bar__row");
            Root.Add(Row);
            // - the pill and the underline: out of the flow, under the items, at the chosen one
            Pill = new VisualElement { name = "nav-bar-pill", pickingMode = PickingMode.Ignore };
            Pill.AddToClassList("nav-bar__pill");
            Row.Add(Pill);
            Underline = new VisualElement { name = "nav-bar-underline", pickingMode = PickingMode.Ignore };
            Underline.AddToClassList("nav-bar__underline");
            Row.Add(Underline);
            // - the disc over everything: under Raised it rises past the bar's edge; a finger on it is a tap on the chosen group, as its
            //   button below would be
            Disc = new VisualElement { name = "nav-bar-disc" };
            Disc.AddToClassList("nav-bar__disc");
            Disc.AddManipulator(new Clickable(() => Tap(ChosenIndex)));
            DiscIcon = CardIcons.Create();
            DiscIcon.AddToClassList("nav-bar__disc-icon");
            Disc.Add(DiscIcon);
            Root.Add(Disc);
            _items = new ElementPool<Item>(NewItem, item => item.Button.RemoveFromHierarchy());
            // - colours follow the mode, numbers the card's tokens, the indicators the items' layout
            Root.RegisterCallback<AttachToPanelEvent>(_ => { ThemeController.ModeChanged -= OnModeChanged; ThemeController.ModeChanged += OnModeChanged; Refresh(); });
            Root.RegisterCallback<DetachFromPanelEvent>(_ => ThemeController.ModeChanged -= OnModeChanged);
            Row.RegisterCallback<GeometryChangedEvent>(_ => Place(false));
            // - the card's tokens resolve with its first layout: read them then; a new width redraws the path
            Root.RegisterCallback<GeometryChangedEvent>(_ => { if (BarHeight <= 0f) Refresh(); else Draw(); });
            // - a slide still under way when the bar is re-hosted (every bind takes it off its slot and puts it back) starts again on its new
            //   panel: its scheduled frames belonged to the old one
            Root.RegisterCallback<AttachToPanelEvent>(_ => { if (_sliding) RunSlide(); });
        }

        public void Bind(CardNavModel model, NavController nav, BlockBindContext context)
        {
            Unbind();
            _nav = nav;
            _strings = context?.Strings;
            ReduceMotion = context?.ReduceMotion ?? false;
            var navSettings = context?.Settings?.navigation;
            Settings = navSettings?.bar ?? new CardNavBarSettings();
            var items = NavBarRule.Items(navSettings, BlockRegistry.Shared);
            _entries = NavBarRule.Entries(items, model?.Targets, block =>
                nav != null && nav.TryGetEntry(block, out var entry) ? entry.Definition.Family : null);
            for (int i = 0; i < _entries.Count; i++)
            {
                var item = _items.Take();
                item.Label.text = NavBarRule.Words(_entries[i].Item, _strings);
                // - an icon alone still names its group (tooltip: what a screen reader and a long press read)
                item.Button.tooltip = item.Label.text;
                Row.Add(item.Button);
            }
            Disc.BringToFront();
            Refresh();
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
            Centre = CentreTarget = float.NaN;
            StopSlide();
        }

        // A tap on a group: the card moves to its first block
        public bool Tap(int index)
        {
            if (index < 0 || index >= _entries.Count || _nav == null) return false;
            return _nav.JumpToBlock(_entries[index].FirstTarget, ElementKey);
        }

        // The sheet says whether the bar is over the hero's picture (NavBarRule.OverHero): a non-solid bar turns white on dark glass there
        public void SetOverHero(bool overHero)
        {
            if (overHero == OverHero) return;
            OverHero = overHero;
            Root.EnableInClassList(OverHeroClass, overHero);
            Paint();
        }

        // A non-solid bar over the hero: white words on dark glass (or on the hero itself, Clear)
        public bool OverImage => OverHero && NavBarRule.Fill(Settings) != CardOptions.NavBarSolid;


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

        private void OnModeChanged() => Paint();

        private void OnCurrentBlockChanged(string block)
        {
            int index = NavBarRule.EntryOf(_entries, block);
            // - a block of no group (a family the bar leaves out) keeps the last mark: the visitor is still under that group's map
            if (index < 0 && block != null && ChosenIndex >= 0) return;
            Choose(index);
        }

        // Mark one group: its class, its chosen icon, its words' display; the indicators slide to it
        private void Choose(int index)
        {
            bool first = ChosenIndex < 0 || float.IsNaN(Centre);
            ChosenIndex = index;
            string labels = NavBarRule.Labels(Settings);
            bool disc = NavBarRule.ChosenDisc(Settings);
            foreach (var item in _items.Shown)
            {
                bool chosen = item.Index == index;
                var entry = _entries[item.Index];
                item.Button.EnableInClassList(ChosenClass, chosen);
                string key = chosen ? entry.Item.SelectedIcon : entry.Item.Icon;
                bool hasIcon = !string.IsNullOrEmpty(key);
                CardIcons.SetKey(item.Icon, key);
                item.Icon.style.display = NavBarRule.ShowsIcon(labels, hasIcon) ? DisplayStyle.Flex : DisplayStyle.None;
                // - the disc carries the chosen icon: its own place stays (the words do not move), the icon only goes unseen
                item.Icon.style.opacity = chosen && disc ? 0f : 1f;
                item.Label.style.display = NavBarRule.ShowsWords(labels, hasIcon, chosen) ? DisplayStyle.Flex : DisplayStyle.None;
            }
            if (index >= 0) CardIcons.SetKey(DiscIcon, _entries[index].Item.SelectedIcon);
            Paint();
            Place(!first);
        }

        // The settings and the card's numbers: classes for what USS draws, the box for the path
        private void Refresh()
        {
            var bar = Settings ?? new CardNavBarSettings();
            string labels = NavBarRule.Labels(bar);
            BarHeight = Token(labels == CardOptions.NavBarLabelsIconAndText ? HeightToken : CompactHeightToken);
            Room = Token(RoomToken);
            bool bottom = NavBarRule.AtBottom(bar), floating = NavBarRule.Floating(bar);
            foreach (string option in CardOptions.NavBarShapes) Root.EnableInClassList("nav-bar--" + option, option == NavBarRule.Shape(bar));
            foreach (string option in CardOptions.NavBarFills) Root.EnableInClassList("nav-bar--fill-" + option, option == NavBarRule.Fill(bar));
            foreach (string option in CardOptions.NavBarIndicators) Root.EnableInClassList("nav-bar--indicator-" + option, option == NavBarRule.Indicator(bar));
            foreach (string option in CardOptions.NavBarLabels) Root.EnableInClassList("nav-bar--labels-" + option, option == labels);
            Root.EnableInClassList("nav-bar--bottom", bottom);
            Root.EnableInClassList("nav-bar--top", !bottom);
            Root.EnableInClassList("nav-bar--bold", NavBarRule.BoldChosen(bar));
            Root.EnableInClassList("nav-bar--filled", NavBarRule.ChosenIcon(bar) == CardOptions.NavBarChosenIconFilled);
            float inset = floating ? Token(InsetToken) : 0f;
            Root.style.marginLeft = inset;
            Root.style.marginRight = inset;
            Root.style.marginBottom = floating && bottom ? Token(GapToken) : 0f;
            Root.style.height = Room + BarHeight;
            // - the items' row is the bar itself: under the room at the bottom (the bump rises into it), on top at Top (it hangs below)
            Row.style.top = bottom ? Room : 0f;
            Row.style.height = BarHeight;
            DiscSize = NavBarRule.Raised(bar) ? Token(RaisedDiscToken) : Token(FilledDiscToken);
            Disc.style.width = Disc.style.height = DiscSize;
            Disc.style.borderTopLeftRadius = Disc.style.borderTopRightRadius =
                Disc.style.borderBottomLeftRadius = Disc.style.borderBottomRightRadius = DiscSize * 0.5f;
            Paint();
            Place(false);
        }

        // The colours: the surface's fill and hairline (by fill, over the hero or after it), each group's icon and words, the disc and pill
        private void Paint()
        {
            var bar = Settings;
            if (bar == null) return;
            string fill = NavBarRule.Fill(bar);
            bool overImage = OverImage;
            // - Clear over the hero: transparent at the Bottom, its labels with the hero words' halo (Navigation.uss); at Top it lies
            //   in the dark scrim -- a stronger shade, never weaker words (the words' 7:1 rule, measured on the render)
            string fillToken = fill == CardOptions.NavBarSolid ? "--ts-bar"
                : fill == CardOptions.NavBarGlass ? (OverHero ? "--ts-chrome-glass-hero" : "--ts-navbar-glass")
                : !OverHero ? "--ts-bar" : NavBarRule.AtBottom(bar) ? null : "--ts-scrim-hero";
            string lineToken = fill == CardOptions.NavBarGlass && OverHero ? "--ts-chrome-line-hero" : fill == CardOptions.NavBarClear && OverHero ? null : "--ts-hair";
            PaintedFill = Colour(fillToken);
            PaintedLine = Colour(lineToken);
            PaintedShadow = NavBarRule.Floating(bar) && fill == CardOptions.NavBarSolid;

            string selected = NavBarRule.SelectedColour(bar), iconColour = NavBarRule.IconColour(bar);
            Color white = Colour("--ts-on-scrim");
            Color chosenColour = overImage ? white : Colour(ChosenToken(selected, ChosenIndex));
            Color idle = overImage ? white : Colour("--ts-ink-2");
            foreach (var item in _items.Shown)
            {
                bool chosen = item.Index == ChosenIndex;
                // - an idle icon in Icon Colour's readable tone (its group's -text by default); the words stay ink
                Color icon = overImage ? white : Colour(NavBarRule.IconToken(iconColour, _entries[item.Index].Item.ColorToken));
                CardIcons.Tint(item.Icon, chosen ? chosenColour : icon);
                // - the words stay ink on a group colour (a family colour is a mark, not a text colour); accent / ink words are text colours
                Color words = chosen ? (overImage ? white : selected == CardOptions.NavBarColourGroup ? Colour("--ts-ink") : chosenColour) : idle;
                item.Label.style.color = words;
            }
            // - the disc: the chosen colour with the ink made for it on it -- a group colour's own -ink, the accent's on-accent (white over
            //   the hero with the dark scrim's colour on it, both modes)
            Color discInk = overImage ? Opaque(Colour("--ts-scrim-hero"))
                : selected == CardOptions.NavBarColourInk ? Colour("--ts-surface") : Colour(NavBarRule.InkOn(ChosenToken(selected, ChosenIndex)));
            Disc.style.backgroundColor = chosenColour;
            CardIcons.Tint(DiscIcon, discInk);
            var wash = chosenColour;
            wash.a = overImage ? 0.2f : 0.12f;
            Pill.style.backgroundColor = wash;
            Underline.style.backgroundColor = chosenColour;
            Surface.MarkDirtyRepaint();
        }

        // The token of the chosen group's colour: the accent, the ink, or its row's colour
        private string ChosenToken(string selected, int index) =>
            selected == CardOptions.NavBarColourInk ? "--ts-ink"
            : selected == CardOptions.NavBarColourGroup && index >= 0 && index < _entries.Count && !string.IsNullOrEmpty(_entries[index].Item.ColorToken)
                ? _entries[index].Item.ColorToken
                : "--ts-accent";

        // The indicators to the chosen group's centre: sliding there (v5's overshoot) or at once (Reduce Motion, the first mark)
        private void Place(bool slide)
        {
            bool shown = ChosenIndex >= 0 && ChosenIndex < _items.Shown.Count;
            var box = shown ? _items.Shown[ChosenIndex].Button.layout : default;
            if (!shown || float.IsNaN(box.width) || box.width <= 0f)
            {
                Draw();
                return;
            }
            CentreTarget = Row.layout.x + box.center.x;
            if (!slide || ReduceMotion || float.IsNaN(Centre))
            {
                StopSlide();
                Centre = CentreTarget;
            }
            else if (!Mathf.Approximately(Centre, CentreTarget))
            {
                _slideFrom = Centre;
                _slideStart = Clock();
                _sliding = true;
                RunSlide();
            }
            Draw();
        }

        // One frame of the slide
        private void SlideFrame()
        {
            if (!_sliding) return;
            float t = (Clock() - _slideStart) / NavBarShapeRule.SlideSeconds;
            Centre = Mathf.LerpUnclamped(_slideFrom, CentreTarget, NavBarShapeRule.Slide(t));
            if (t >= 1f) StopSlide();
            Draw();
        }

        // The slide's frames, made fresh for each slide on the panel the bar sits on now: never ONE item kept paused across the re-hosts every
        // bind makes (a frozen slide leaves the bump and the disc where it began -- the first group)
        private void RunSlide()
        {
            _slide?.Pause();
            _slide = Root.schedule.Execute(SlideFrame).Every(16);
        }

        private void StopSlide()
        {
            _sliding = false;
            _slide?.Pause();
            _slide = null;
            if (!float.IsNaN(CentreTarget)) Centre = CentreTarget;
        }

        // Everything the centre moves: the path (the bump), the disc, the pill and the underline
        private void Draw()
        {
            var bar = Settings ?? new CardNavBarSettings();
            string indicator = NavBarRule.Indicator(bar);
            bool chosen = ChosenIndex >= 0 && !float.IsNaN(Centre);
            bool bottom = NavBarRule.AtBottom(bar), raised = indicator == CardOptions.NavBarIndicatorRaised;
            float width = BarWidth;
            // - the bell is centred on the raised disc, the disc held inside the bar: one centre for both, so they never part near a side
            float bellX = float.IsNaN(width) ? Centre : NavBarShapeRule.BumpX(chosen ? Centre : width * 0.5f, width, RaisedDiscRadius);
            if (!float.IsNaN(width) && width > 0f)
            {
                float radius = NavBarShapeRule.Radius(NavBarRule.Floating(bar), raised, BarHeight, Token(RaisedRadiusToken));
                float rise = raised && chosen ? BumpHeight : 0f;
                var path = NavBarShapeRule.Path(width, BarHeight, Room, radius, rise, Token(BumpHalfToken),
                    bellX, NavBarShapeRule.CrestRadius(rise, RaisedDiscRadius, Token(CradleGapToken)));
                PaintedPath = bottom ? path : NavBarShapeRule.Mirror(path, Room + BarHeight);
                Surface.MarkDirtyRepaint();
            }

            bool disc = chosen && NavBarRule.ChosenDisc(bar);
            Disc.style.display = disc ? DisplayStyle.Flex : DisplayStyle.None;
            if (disc)
            {
                Disc.style.left = (raised ? bellX : Centre) - DiscSize * 0.5f;
                if (raised)
                {
                    // - its centre on the flat edge whatever the depth (at +1 its top is the bell's peak, the crest its own outline; mirrored
                    //   at Top: its foot the bell's lowest point)
                    float discTop = NavBarShapeRule.DiscTop(Room, NavBarShapeRule.BellHeight(RaisedDiscRadius));
                    Disc.style.top = bottom ? discTop : Room + BarHeight - discTop - DiscSize;
                }
                else
                {
                    var icon = _items.Shown[ChosenIndex].Icon.layout;
                    var button = _items.Shown[ChosenIndex].Button.layout;
                    float iconCentre = Row.layout.y + button.y + icon.center.y;
                    Disc.style.top = float.IsNaN(iconCentre) ? Row.layout.y : iconCentre - DiscSize * 0.5f;
                }
            }
            bool pill = chosen && indicator == CardOptions.NavBarIndicatorPill;
            Pill.style.display = pill ? DisplayStyle.Flex : DisplayStyle.None;
            bool underline = chosen && indicator == CardOptions.NavBarIndicatorUnderline;
            Underline.style.display = underline ? DisplayStyle.Flex : DisplayStyle.None;
            if (chosen && (pill || underline))
            {
                float itemWidth = _items.Shown[ChosenIndex].Button.layout.width;
                float local = Centre - Row.layout.x;
                if (pill)
                {
                    Pill.style.left = local - itemWidth * 0.5f + 3f;
                    Pill.style.width = Mathf.Max(0f, itemWidth - 6f);
                }
                else Underline.style.left = local;
            }
        }

        // The surface: the shadow (offset copies, faint), the fill, the hairline -- one path
        private void PaintSurface(MeshGenerationContext mgc)
        {
            if (PaintedPath.Count == 0) return;
            var painter = mgc.painter2D;
            if (PaintedShadow && PaintedFill.a > 0f)
            {
                // - no blur in Painter2D: a few faint copies a little lower read as a soft shadow under the bar's foot
                var shade = Colour("--ts-elevation-1");
                shade.a *= 0.3f;
                for (int i = 1; i <= 3; i++) Trace(painter, PaintedPath, new Vector2(0f, i * 2f), shade, null);
            }
            Trace(painter, PaintedPath, Vector2.zero, PaintedFill.a > 0f ? PaintedFill : (Color?)null, PaintedLine.a > 0f ? PaintedLine : (Color?)null);
        }

        private static void Trace(Painter2D painter, IReadOnlyList<NavBarShapeRule.Command> path, Vector2 offset, Color? fill, Color? line)
        {
            if (fill == null && line == null) return;
            painter.BeginPath();
            foreach (var c in path)
            {
                switch (c.Op)
                {
                    case NavBarShapeRule.Op.Move: painter.MoveTo(c.P1 + offset); break;
                    case NavBarShapeRule.Op.Line: painter.LineTo(c.P1 + offset); break;
                    case NavBarShapeRule.Op.Quad: painter.QuadraticCurveTo(c.P1 + offset, c.P2 + offset); break;
                    case NavBarShapeRule.Op.Cubic: painter.BezierCurveTo(c.P1 + offset, c.P2 + offset, c.P3 + offset); break;
                    case NavBarShapeRule.Op.Close: painter.ClosePath(); break;
                }
            }
            if (fill.HasValue)
            {
                painter.fillColor = fill.Value;
                painter.Fill();
            }
            if (line.HasValue)
            {
                painter.strokeColor = line.Value;
                painter.lineWidth = 1f;
                painter.Stroke();
            }
        }

        // A theme colour by name; nothing (fully transparent) for no name or when the bar sits under no theme yet
        private Color Colour(string token) =>
            token != null && CardTokenColours.TryResolve(Root, token, out Color colour) ? colour : default;

        private static Color Opaque(Color colour)
        {
            colour.a = 1f;
            return colour;
        }

        // A plain-number token read on the card root (.ts-card), 0 when the bar sits under none
        private float Token(CustomStyleProperty<float> token)
        {
            for (var e = Root.parent; e != null; e = e.parent)
                if (e.ClassListContains("ts-card")) return e.customStyle.TryGetValue(token, out float value) ? value : 0f;
            return 0f;
        }
    }
}

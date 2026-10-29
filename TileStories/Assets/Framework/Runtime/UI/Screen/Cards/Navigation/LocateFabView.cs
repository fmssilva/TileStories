using TileStories.LookKit;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories
{
    // The card's Locate Button ("Show me where it is", _3.3 P3d review b; study _3.3.1_Prototype/Locate Button Study.html): a 3D-looking map
    // pin (LocatePin) floating at the bottom right of the sheet, over the hero and after it -- where and when LocateFabRule says, placed by
    // the sheet. Detail Card > Locate Button picks its Style (Floating Pin / Pin + Label -- "Find it on the wall" on a light chip at rest,
    // folded away at the visitor's first scroll of the card, the pin staying / Glass + Ping -- a light glass disc holding the pin, a soft ring
    // every PingPeriod), its Colour (theme tokens), its Size (the Navigation Bar's chosen disc times Size) and Float (a slow bob, its shadow
    // on the bar breathing with it; Reduce Motion = still). The touch target is never under 44 pt; the chip is part of it. A tap lowers the
    // card to Peek with the point's marker in view (IBlockHost.ShowOnWall); its name is the Show On Wall words (CardStrings). Hide While
    // Scrolling slides it away with the bar (HiddenClass, PoiCard.uss), and while away it takes no finger. The sheet owns one.
    //   - it FLOATS: Layer is its own layer in the card layer, right after the sheet (UI Toolkit draws children in order, no z-index), so it
    //     is drawn over the sheet, never clipped by it, and takes no room in it -- the hero's words and the blocks never move for it. The sheet
    //     stands at the layer's foot, full width, so "above the sheet's foot" is the same number in the layer
    public sealed class LocateFabView
    {
        public const string LayerClass = "poi-card-locate";
        public const string RootClass = "poi-card__fab";
        // Slid away with the bar (Hide While Scrolling): below its place and faded out, over the chrome's own transition
        public const string HiddenClass = RootClass + "--hidden";
        public const string ChipClass = "poi-card__fab-chip";
        // The chip folded away (the visitor scrolled): faded, slid towards the pin, no finger
        public const string FoldedClass = ChipClass + "--folded";
        // No room for the chip beside the pin (the hero's words reach across it): laid out but not shown, no finger
        public const string NoRoomClass = ChipClass + "--no-room";
        public const string FloatClass = "poi-card__fab-float";
        public const string ShadowClass = "poi-card__fab-shadow";
        public const string GlassClass = "poi-card__fab-glass";
        public const string PingClass = "poi-card__fab-ping";

        // Its layer over the sheet (no finger of its own: a tap beside the button reaches the card under it)
        public VisualElement Layer { get; }
        public Button Root { get; }
        public LocatePin Pin { get; }
        public Label Chip { get; }
        public VisualElement Float { get; }
        public VisualElement Shadow { get; }
        public VisualElement Glass { get; }
        public VisualElement Ping { get; }
        // The motion's clock (tests hand their own) and whether it runs now
        public System.Func<double> Clock { get => _ticker.Clock; set => _ticker.Clock = value; }
        public bool Moving => _ticker.Running;
        // Placed now (display) and not slid away: what the visitor sees and can tap
        public bool IsShown => Root.style.display == DisplayStyle.Flex && !Root.ClassListContains(HiddenClass);
        public bool LabelShown => Chip.style.display == DisplayStyle.Flex && !Chip.ClassListContains(FoldedClass) && !Chip.ClassListContains(NoRoomClass);
        public string Style { get; private set; } = CardOptions.LocatePinLabel;
        // The pin's height and the touch target's side (pt), as last laid out
        public float PinHeight { get; private set; }
        public float Touch { get; private set; } = LocateFabRule.MinTouch;

        // Its numbers (CardTokens.uss, read on the card root): the gap above what lies under it, its inset from the side, the bar's chosen disc
        public float Gap => Token(GapToken);
        // The chip's own gap: from the pin, and the least room it keeps from any word
        public float ChipGap => Token(ChipGapToken);
        public float Inset => Token(InsetToken);
        public float ChosenDisc => Token(ChosenDiscToken);

        private static readonly CustomStyleProperty<float> GapToken = new("--ts-fab-gap");
        private static readonly CustomStyleProperty<float> ChipGapToken = new("--ts-fab-chip-gap");
        private static readonly CustomStyleProperty<float> ChipHeightToken = new("--ts-fab-chip-height");
        private static readonly CustomStyleProperty<float> InsetToken = new("--ts-navbar-inset");
        private static readonly CustomStyleProperty<float> ChosenDiscToken = new("--ts-navbar-disc-raised");

        private readonly KitTicker _ticker;
        private CardLocateSettings _settings;
        private bool _reduceMotion, _scrolled, _laidOutGlass;

        public LocateFabView(System.Action tap)
        {
            // - the card's own tokens live on these classes (CardTokens.uss: the numbers read below), the theme's arrive from the app root
            Layer = new VisualElement { name = "poi-card-locate", pickingMode = PickingMode.Ignore };
            Layer.AddToClassList("ts-card");
            Layer.AddToClassList("ts-theme-default");
            Layer.AddToClassList(LayerClass);
            Root = new Button(tap) { name = "poi-card-fab" };
            Root.AddToClassList(RootClass);
            Root.style.display = DisplayStyle.None;
            Chip = new Label { name = "poi-card-fab-chip", pickingMode = PickingMode.Position };
            Chip.AddToClassList(ChipClass);
            Shadow = Part("poi-card-fab-shadow", ShadowClass);
            Float = Part("poi-card-fab-float", FloatClass);
            Glass = Part("poi-card-fab-glass", GlassClass);
            Ping = Part("poi-card-fab-ping", PingClass);
            Pin = new LocatePin { name = "poi-card-fab-pin" };
            Glass.Add(Ping);
            Float.Add(Glass);
            Float.Add(Pin);
            Root.Add(Chip);
            Root.Add(Shadow);
            Root.Add(Float);
            Layer.Add(Root);
            _ticker = new KitTicker(Root, Move);
            Shadow.generateVisualContent += DrawShadow;
            // - the colours follow the mode (a painted part does not follow a mode class)
            Root.RegisterCallback<AttachToPanelEvent>(_ => { ThemeController.ModeChanged -= Paint; ThemeController.ModeChanged += Paint; Paint(); });
            Root.RegisterCallback<DetachFromPanelEvent>(_ => { ThemeController.ModeChanged -= Paint; _ticker.Stop(); });
            // - its sizes come from the card's tokens: laid out again once they resolve
            Root.RegisterCallback<CustomStyleResolvedEvent>(_ => { Paint(); Layout(); });
        }

        private static VisualElement Part(string name, string className)
        {
            var part = new VisualElement { name = name, pickingMode = PickingMode.Ignore };
            part.AddToClassList(className);
            return part;
        }

        // The open card: its words (the button's name, the chip's words), its look and whether its motion runs; a new card shows the chip again
        public void Bind(CardStrings strings, CardLocateSettings settings, bool reduceMotion, bool newCard)
        {
            Root.tooltip = strings?.Get(CardStrings.Keys.ShowOnWallButton) ?? "";
            Chip.text = strings?.Get(CardStrings.Keys.LocateLabel) ?? "";
            _settings = settings;
            _reduceMotion = reduceMotion;
            if (newCard)
            {
                _scrolled = false;
                Chip.RemoveFromClassList(FoldedClass);
            }
            Style = LocateFabRule.Style(settings);
            Chip.style.display = Style == CardOptions.LocatePinLabel ? DisplayStyle.Flex : DisplayStyle.None;
            Glass.style.display = LocateFabRule.HasGlass(settings) ? DisplayStyle.Flex : DisplayStyle.None;
            Root.EnableInClassList("poi-card__fab--instant", reduceMotion);
            Pin.Paint(LocateFabRule.Colour(settings));
            Paint();
            Layout();
            Move(0f);
        }

        // Whether the chip has room beside the pin now (LocateFabRule.LabelFits, judged by the sheet over the hero's words)
        public void SetLabelRoom(bool room)
        {
            Chip.EnableInClassList(NoRoomClass, !room);
            Chip.pickingMode = IsShown && LabelShown ? PickingMode.Position : PickingMode.Ignore;
        }

        // The visitor scrolled the card: the chip folds away for this card (the pin stays)
        public void Scrolled()
        {
            if (_scrolled) return;
            _scrolled = true;
            Chip.AddToClassList(FoldedClass);
            Chip.pickingMode = PickingMode.Ignore;
        }

        // Placed (`placed`: the bar and its Locate Button on, at Half or Full) `bottom` above the sheet's foot, or slid away while the scroll
        // hides the bar (`shown` false while placed); the motion runs only while it is seen and moves
        public void Apply(bool placed, bool shown, float bottom)
        {
            Root.style.display = placed ? DisplayStyle.Flex : DisplayStyle.None;
            Root.EnableInClassList(HiddenClass, placed && !shown);
            Root.pickingMode = placed && shown ? PickingMode.Position : PickingMode.Ignore;
            Chip.pickingMode = placed && shown && LabelShown ? PickingMode.Position : PickingMode.Ignore;
            Layout();
            // - only a real move is written: the same value again would raise another geometry change for nothing
            if (Root.style.bottom.keyword != StyleKeyword.Undefined || Mathf.Abs(Root.style.bottom.value.value - bottom) > 0.01f) Root.style.bottom = bottom;
            float inset = Inset;
            if (Root.style.right.keyword != StyleKeyword.Undefined || Mathf.Abs(Root.style.right.value.value - inset) > 0.01f) Root.style.right = inset;
            bool moves = placed && shown && LocateFabRule.Floats(_settings, _reduceMotion);
            if (moves && !_ticker.Running) _ticker.Start();
            else if (!moves && _ticker.Running) { _ticker.Stop(); Move(0f); }
        }

        // The parts' sizes from the pin's height (the bar's chosen disc times Size): the touch target round all of it, the pin centred in it
        // standing on its shadow, the glass disc round the pin
        private void Layout()
        {
            float chosenDisc = ChosenDisc;
            if (!(chosenDisc > 0f)) return;
            bool glass = LocateFabRule.HasGlass(_settings);
            float pin = LocateFabRule.PinHeight(chosenDisc, _settings);
            float touch = LocateFabRule.Touch(pin, glass);
            // - laid out again only when something it is laid out from changed (the same values again raise a geometry change for nothing)
            if (Mathf.Approximately(pin, PinHeight) && Mathf.Approximately(touch, Touch) && glass == _laidOutGlass) return;
            _laidOutGlass = glass;
            PinHeight = pin;
            Touch = touch;
            float pinWide = pin * LocateFabRule.PinAspect;
            float disc = LocateFabRule.FloatBox(pin, glass);
            float shadowWide = LocateFabRule.ShadowWidth(pin, glass), shadowHigh = LocateFabRule.ShadowHeight(pin, glass);
            Root.style.width = Root.style.height = touch;
            Shadow.style.width = shadowWide;
            Shadow.style.height = shadowHigh;
            Shadow.style.left = (touch - shadowWide) * 0.5f;
            // - the float box: the pin (or its disc) standing on the shadow's middle, centred across
            Float.style.width = disc;
            Float.style.height = glass ? disc : pin;
            Float.style.left = (touch - disc) * 0.5f;
            // - the tip a little above its shadow: it floats even at rest
            float floatBottom = LocateFabRule.FloatBottom(pin, glass);
            Float.style.bottom = floatBottom;
            Pin.style.width = pinWide;
            Pin.style.height = pin;
            Pin.style.left = (disc - pinWide) * 0.5f;
            Pin.style.top = glass ? (disc - pin) * 0.5f : 0f;
            Glass.style.width = Glass.style.height = disc;
            Glass.style.borderTopLeftRadius = Glass.style.borderTopRightRadius =
                Glass.style.borderBottomLeftRadius = Glass.style.borderBottomRightRadius = disc * 0.5f;
            Ping.style.borderTopLeftRadius = Ping.style.borderTopRightRadius =
                Ping.style.borderBottomLeftRadius = Ping.style.borderBottomRightRadius = disc * 0.5f;
            // - the chip beside the pin (its gap from the pin's own side), its middle level with the pin's inner disc (the drawing's 23 of 64)
            float chipHigh = Token(ChipHeightToken);
            Chip.style.height = chipHigh;
            // - a true capsule: half its height exactly (a radius past half a side is scaled down per corner pair -- an oval)
            Chip.style.borderTopLeftRadius = Chip.style.borderTopRightRadius =
                Chip.style.borderBottomLeftRadius = Chip.style.borderBottomRightRadius = chipHigh * 0.5f;
            Chip.style.right = touch + Token(ChipGapToken) - (touch - pinWide) * 0.5f;
            Chip.style.bottom = floatBottom + (glass ? disc * 0.5f : pin * (1f - 23f / 64f)) - chipHigh * 0.5f;
        }

        // One frame of the motion at `time` (s): the pin's float, the shadow breathing with it, the ping going out
        private void Move(float time)
        {
            bool floats = LocateFabRule.Floats(_settings, _reduceMotion) && _ticker.Running;
            float rise = LocateFabRule.Rise(time, floats);
            Float.style.translate = new Translate(0f, -LocateFabRule.Bob * rise);
            Shadow.style.scale = new Scale(Vector2.one * LocateFabRule.ShadowScale(rise));
            Shadow.style.opacity = LocateFabRule.ShadowAlpha(rise);
            var (scale, alpha) = LocateFabRule.Ping(time, floats && LocateFabRule.HasGlass(_settings));
            Ping.style.scale = new Scale(Vector2.one * scale);
            Ping.style.opacity = alpha;
        }

        // The colours of the parts drawn in C# (the pin paints itself): the shadow, the ping ring in the pin's deep colour
        private void Paint()
        {
            Shadow.MarkDirtyRepaint();
            if (CardTokenColours.TryResolve(Root, LocateFabRule.Tokens(LocateFabRule.Colour(_settings)).Lo, out Color lo))
            {
                // - the study's: the ring at 0.6 of the pin's deep colour, the chip's edge at 0.45
                lo.a = 0.6f;
                Ping.style.borderTopColor = Ping.style.borderRightColor = Ping.style.borderBottomColor = Ping.style.borderLeftColor = lo;
            }
            // - the chip's edge: the pin's own light colour, a button's coloured border
            if (CardTokenColours.TryResolve(Root, LocateFabRule.Tokens(LocateFabRule.Colour(_settings)).Hi, out Color hi)
                && CardTokenColours.TryResolve(Root, "--ts-locate-chip", out Color chip))
            {
                // - a third of the way to white: a soft coloured edge, the button's, never louder than the pin
                chip.a = 1f;
                var edge = Color.Lerp(hi, chip, 0.35f);
                Chip.style.borderTopColor = Chip.style.borderRightColor = Chip.style.borderBottomColor = Chip.style.borderLeftColor = edge;
            }
            Pin.MarkDirtyRepaint();
        }

        // The shadow on the bar: a soft black ellipse -- a few nested ellipses, each faint, so its edge fades (no blur in UI Toolkit)
        private void DrawShadow(MeshGenerationContext context)
        {
            var box = Shadow.contentRect;
            if (!(box.width > 0f) || !CardTokenColours.TryResolve(Root, "--ts-locate-shadow", out Color shade)) return;
            var p = context.painter2D;
            const int layers = 7;
            // - light: the study's grey ellipse, not a dark blot
            float alpha = shade.a * 0.6f / layers;
            for (int i = 0; i < layers; i++)
            {
                float s = 1f - i * 0.12f;
                float rx = box.width * 0.5f * s, ry = box.height * 0.5f * s;
                var c = box.center;
                const float k = 0.5523f;
                p.BeginPath();
                p.MoveTo(c + new Vector2(-rx, 0f));
                p.BezierCurveTo(c + new Vector2(-rx, -ry * k), c + new Vector2(-rx * k, -ry), c + new Vector2(0f, -ry));
                p.BezierCurveTo(c + new Vector2(rx * k, -ry), c + new Vector2(rx, -ry * k), c + new Vector2(rx, 0f));
                p.BezierCurveTo(c + new Vector2(rx, ry * k), c + new Vector2(rx * k, ry), c + new Vector2(0f, ry));
                p.BezierCurveTo(c + new Vector2(-rx * k, ry), c + new Vector2(-rx, ry * k), c + new Vector2(-rx, 0f));
                p.ClosePath();
                var layer = shade;
                layer.a = alpha;
                // - fillColor: a gradient fill ignores alpha
                p.fillColor = layer;
                p.Fill();
            }
        }

        // A plain-number token read on the card root (.ts-card), 0 when it sits under none
        private float Token(CustomStyleProperty<float> token)
        {
            for (var e = Root.parent; e != null; e = e.parent)
                if (e.ClassListContains("ts-card")) return e.customStyle.TryGetValue(token, out float value) ? value : 0f;
            return 0f;
        }
    }
}

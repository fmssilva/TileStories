using System.Collections.Generic;
using TileStories.LookKit;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories
{
    // One OPEN section's own parts (_3.3 fourth round, v6's Sections; Look Blocks = Open): what stands before it (the Divider), how it begins
    // (the Start: a tile row with rounded top corners, a wave, or nothing but its tint), its ground's tint (a Fade over its height, or a flat
    // band), and its eyebrow -- the 28 x 3 rule in its group's colour over the group's name in that colour's words. Every piece is the Look
    // Kit's (Fade, Tile Row, Wave, Strip, Reveal, Drift); this view only resolves the theme's colours by name (repainting on a mode switch)
    // and places them. The section's blocks are the sheet's, after the eyebrow. Plain C#, owned by its SectionSheets.Sheet.
    //   - the Reveal starts once, the first time the section enters the view (Entered: SectionSheets asks on every scroll); Drift slides the
    //     divider while it is shown (the Start rests); Reduce Motion = still and whole
    public sealed class OpenSectionView
    {
        public const string RootClass = "card-open-section";
        public const string EdgeClass = RootClass + "__edge";
        public const string GroundClass = RootClass + "__ground";
        public const string DividerClass = RootClass + "__divider";
        public const string StartClass = RootClass + "__start";
        public const string EyebrowClass = RootClass + "__eyebrow";
        public const string RuleClass = RootClass + "__rule";
        public const string NameClass = RootClass + "__name";
        // The divider's own looks and the gap a drawn start keeps under a divider
        public const string GapClass = RootClass + "__gap";

        // The parts, in the sheet: the ground at the back, the edge first in its flow, the eyebrow after it
        public VisualElement Ground { get; }
        public VisualElement Edge { get; }
        public VisualElement Divider { get; }
        public VisualElement DividerTiles { get; }
        public VisualElement DividerStrip { get; }
        // The ornament's two lines either side of its one tile
        public VisualElement OrnamentLeft { get; }
        public VisualElement OrnamentRight { get; }
        public VisualElement Gap { get; }
        public VisualElement StartTiles { get; }
        public VisualElement StartTilesRow { get; }
        public VisualElement StartWave { get; }
        public VisualElement Eyebrow { get; }
        public VisualElement Rule { get; }
        public Label Name { get; }

        // What it shows now, for the tests and the look
        public SectionStyleRule.Style Style { get; private set; }
        public int Position { get; private set; }
        public bool Revealed { get; private set; }
        public float RevealElapsed => _reveal.Elapsed;
        public bool RevealRunning => _reveal.Running;
        public bool Drifting => _drift.Running;
        public float DriftOffset { get; private set; }
        public Color TintColour { get; private set; }
        public Color TextColour { get; private set; }
        public TileRowOptions StartTileOptions => _startTiles.Options;
        public TileRowOptions DividerTileOptions => _dividerTiles.Options;
        public WaveOptions WaveOptions => _wave.Options;
        public FadeOptions GroundOptions => _ground.Options;

        // The injectable clock of its motion (tests hand their own)
        public System.Func<double> Clock
        {
            get => _reveal.Clock;
            set { _reveal.Clock = value; _drift.Clock = value; }
        }

        private readonly VisualElement _host;
        private readonly FadePainter _ground = new(new FadeOptions(null));
        private readonly TileRowPainter _startTiles = new(new TileRowOptions());
        private readonly TileRowPainter _dividerTiles = new(new TileRowOptions());
        private readonly WavePainter _wave = new(new WaveOptions());
        private readonly KitTicker _reveal;
        private readonly KitTicker _drift;
        private readonly RevealOptions _revealOptions = new();
        private readonly DriftOptions _driftOptions = new();
        private string _tintToken;
        private string _textToken;
        private Texture2D _image;
        private bool _reduceMotion;
        // Where the ground's tint begins: under the edge (the divider and the start's shape), measured, never written back as a style
        private float _groundTop;
        private string _groupToken;

        public OpenSectionView(VisualElement host)
        {
            _host = host;
            Ground = Part("card-open-section-ground", GroundClass);
            Ground.AddManipulator(_ground);
            Edge = Part("card-open-section-edge", EdgeClass);
            Divider = Part("card-open-section-divider", DividerClass);
            DividerTiles = Part("card-open-section-divider-tiles", DividerClass + "-tiles");
            DividerTiles.AddManipulator(_dividerTiles);
            DividerStrip = Part("card-open-section-divider-strip", DividerClass + "-strip");
            OrnamentLeft = Part("card-open-section-ornament-left", DividerClass + "-line");
            OrnamentRight = Part("card-open-section-ornament-right", DividerClass + "-line");
            Divider.Add(OrnamentLeft);
            Divider.Add(DividerTiles);
            Divider.Add(OrnamentRight);
            Divider.Add(DividerStrip);
            Gap = Part("card-open-section-gap", GapClass);
            StartTiles = Part("card-open-section-tiles", StartClass + "-tiles");
            StartTilesRow = Part("card-open-section-tiles-row", StartClass + "-tiles-row");
            StartTilesRow.AddManipulator(_startTiles);
            StartTiles.Add(StartTilesRow);
            StartWave = Part("card-open-section-wave", StartClass + "-wave");
            StartWave.AddManipulator(_wave);
            Edge.Add(Divider);
            Edge.Add(Gap);
            Edge.Add(StartTiles);
            Edge.Add(StartWave);
            Eyebrow = Part("card-open-section-eyebrow", EyebrowClass);
            Rule = Part("card-open-section-rule", RuleClass);
            Name = new Label { name = "card-open-section-name", pickingMode = PickingMode.Ignore };
            Name.AddToClassList(NameClass);
            Eyebrow.Add(Rule);
            Eyebrow.Add(Name);
            // - the ground fills the section; its tint starts under the edge (the divider and the start's shape) -- the Fade's stops follow the
            //   edge's height, a repaint only (a style written from a geometry event costs the panel another layout pass per section)
            Edge.RegisterCallback<GeometryChangedEvent>(_ => PlaceGround());
            _reveal = new KitTicker(Edge, RevealFrame);
            _drift = new KitTicker(Edge, DriftFrame);
            // - painted colours do not follow a mode class: paint again on a mode switch, while on a panel
            Edge.RegisterCallback<AttachToPanelEvent>(_ => { ThemeController.ModeChanged -= Paint; ThemeController.ModeChanged += Paint; Paint(); });
            Edge.RegisterCallback<DetachFromPanelEvent>(_ => { ThemeController.ModeChanged -= Paint; _drift.Stop(); });
        }

        private static VisualElement Part(string name, string cls)
        {
            var e = new VisualElement { name = name, pickingMode = PickingMode.Ignore };
            e.AddToClassList(cls);
            return e;
        }

        // Show the section `position` of the card as `style` says, in its group's colour token and words; `image` = Divider Image's picture
        public void Show(SectionSheetRule.Run run, SectionStyleRule.Style style, int position, string groupColourToken, string name, Texture2D image,
            bool reduceMotion, bool words)
        {
            Style = style;
            Position = position;
            _image = image;
            _reduceMotion = reduceMotion;
            _revealOptions.ReduceMotion = reduceMotion;
            _driftOptions.ReduceMotion = reduceMotion;
            _tintToken = SectionStyleRule.TintToken(style.Colour, groupColourToken);
            _groupToken = string.IsNullOrEmpty(groupColourToken) ? SectionStyleRule.AccentToken : groupColourToken;
            _textToken = SectionStyleRule.TextOf(string.IsNullOrEmpty(groupColourToken) ? SectionStyleRule.AccentToken : groupColourToken);
            // - the eyebrow's tracked capitals (UI Toolkit has no text-transform)
            Name.text = (name ?? "").ToUpperInvariant();
            Name.style.display = words && Name.text.Length > 0 ? DisplayStyle.Flex : DisplayStyle.None;

            string divider = style.Divider;
            bool tiles = divider == CardOptions.SectionDividerAzulejo || divider == CardOptions.SectionDividerOrnament;
            bool strip = divider == CardOptions.SectionDividerMosaic || divider == CardOptions.SectionDividerImage && image != null;
            Divider.style.display = divider == CardOptions.SectionDividerNone || divider == CardOptions.SectionDividerImage && image == null
                ? DisplayStyle.None : DisplayStyle.Flex;
            foreach (string option in CardOptions.SectionDividers) Divider.EnableInClassList(DividerClass + "--" + option, option == divider);
            DividerTiles.style.display = tiles ? DisplayStyle.Flex : DisplayStyle.None;
            bool ornament = divider == CardOptions.SectionDividerOrnament;
            OrnamentLeft.style.display = OrnamentRight.style.display = ornament ? DisplayStyle.Flex : DisplayStyle.None;
            DividerStrip.style.display = strip ? DisplayStyle.Flex : DisplayStyle.None;
            bool drawnStart = style.Start == CardOptions.SectionStartTiles || style.Start == CardOptions.SectionStartWave;
            Gap.style.display = drawnStart && Divider.style.display == DisplayStyle.Flex ? DisplayStyle.Flex : DisplayStyle.None;
            StartTiles.style.display = style.Start == CardOptions.SectionStartTiles ? DisplayStyle.Flex : DisplayStyle.None;
            StartWave.style.display = style.Start == CardOptions.SectionStartWave ? DisplayStyle.Flex : DisplayStyle.None;

            Revealed = false;
            _reveal.Stop();
            _drift.Stop();
            DriftOffset = 0f;
            Paint();
        }

        // The section entered the view (SectionSheets, on a scroll): the Reveal starts the first time; Drift runs from the first show
        public void Entered()
        {
            if (Revealed) return;
            Revealed = true;
            if (Reveals) _reveal.Start();
            else Finish();
            if (Style.Motion == CardOptions.SectionMotionDrift && !_reduceMotion) _drift.Start();
        }

        // Whether the section's pieces turn in as it enters: Reveal, and Drift (which then slides its divider), never under Reduce Motion
        private bool Reveals => (Style.Motion == CardOptions.SectionMotionReveal || Style.Motion == CardOptions.SectionMotionDrift) && !_reduceMotion;

        // Every piece whole now (tests; Reduce Motion)
        public void Finish()
        {
            _reveal.Stop();
            ApplyReveal(float.PositiveInfinity);
        }

        public void Stop()
        {
            _reveal.Stop();
            _drift.Stop();
        }

        // The colours from the theme the section sits under (none -- clear -- before it sits under a theme): the tint over the sheet's own colour, the start's and divider's tiles, the
        // eyebrow's rule and words
        private void Paint()
        {
            if (_tintToken == null) return;
            Color sheet = Colour("--ts-sheet-open", default);
            sheet.a = 1f;
            Color tint = Colour(_tintToken, default);
            tint.a = 1f;
            TintColour = tint;
            TextColour = Colour(_textToken, Colour("--ts-ink", default));
            Color hair = Colour("--ts-hair", default);

            var stops = new List<FadeStop>();
            foreach (var (offset, amount) in SectionStyleRule.Stops(Style, Position, _groundTop))
                stops.Add(new FadeStop(offset, SectionStyleRule.Tint(sheet, tint, amount)));
            _ground.Options = new FadeOptions(stops);
            _wave.Options = new WaveOptions { Colour = SectionStyleRule.Tint(sheet, tint, Style.Intensity), Rise = _wave.Options.Rise };
            var startTiles = new TileRowOptions { Colour = tint, Paper = sheet, Size = TileRule.Unit, Joint = 1f };
            _startTiles.Options = startTiles;
            var dividerTiles = new TileRowOptions { Colour = tint, Paper = sheet, Size = Style.Divider == CardOptions.SectionDividerOrnament ? 22f : TileRule.Unit, Joint = 1f };
            _dividerTiles.Options = dividerTiles;
            // - a hairline divider is the ground's hairline colour; the ornament's lines its tint (v6: at 0.6)
            Divider.style.backgroundColor = Style.Divider == CardOptions.SectionDividerHairline ? new StyleColor(hair) : StyleKeyword.Null;
            var line = tint;
            line.a = 0.6f;
            OrnamentLeft.style.backgroundColor = OrnamentRight.style.backgroundColor = line;
            if (Style.Divider == CardOptions.SectionDividerMosaic)
            {
                Strip.Apply(DividerStrip, new StripOptions { Texture = MosaicRule.Texture(), Height = MosaicRule.Height, TileWidth = MosaicRule.Width });
                DividerStrip.style.unityBackgroundImageTintColor = tint;
            }
            else if (Style.Divider == CardOptions.SectionDividerImage && _image != null)
            {
                Strip.Apply(DividerStrip, new StripOptions { Texture = _image, Height = 24f });
                DividerStrip.style.unityBackgroundImageTintColor = StyleKeyword.Null;
            }
            // - the rule keeps the GROUP's colour whatever Colour tints the start (v6: the eyebrow is the group's)
            Rule.style.backgroundColor = Colour(_groupToken, tint);
            Name.style.color = TextColour;
            ApplyReveal(Revealed && !_reveal.Running ? float.PositiveInfinity : _reveal.Elapsed);
        }

        private void PlaceGround()
        {
            float top = Edge.resolvedStyle.display == DisplayStyle.None || float.IsNaN(Edge.layout.height) ? 0f : Edge.layout.height;
            if (Mathf.Approximately(top, _groundTop)) return;
            _groundTop = top;
            PaintGround();
        }

        // Only the ground's stops (a repaint, never a style: this runs from a geometry event)
        private void PaintGround()
        {
            if (_tintToken == null) return;
            Color sheet = Colour("--ts-sheet-open", default);
            sheet.a = 1f;
            var stops = new List<FadeStop>();
            foreach (var (offset, amount) in SectionStyleRule.Stops(Style, Position, _groundTop))
                stops.Add(new FadeStop(offset, SectionStyleRule.Tint(sheet, TintColour, amount)));
            _ground.Options = new FadeOptions(stops);
        }

        private void RevealFrame(float elapsed)
        {
            ApplyReveal(elapsed);
            if (elapsed >= RevealRule.Length(TileRule.CountFor(Edge.layout.width, TileRule.Unit, 1f), _revealOptions)) _reveal.Stop();
        }

        // Each piece at `elapsed` of the Reveal: the start's tiles one after the other, the divider's tiles too, the wave rising, a strip fading
        // in; before the section entered (Reveal) everything waits hidden
        private void ApplyReveal(float elapsed)
        {
            float at = !Reveals ? float.PositiveInfinity : Revealed ? elapsed : float.NegativeInfinity;
            System.Func<int, float> turn = i => float.IsPositiveInfinity(at) ? 1f : float.IsNegativeInfinity(at) ? 0f : RevealRule.Turn(i, at, _revealOptions);
            System.Func<int, float> alpha = i => float.IsPositiveInfinity(at) ? 1f : float.IsNegativeInfinity(at) ? 0f : RevealRule.Alpha(i, at, _revealOptions);
            _startTiles.Options.ScaleOf = turn;
            _startTiles.Options.AlphaOf = alpha;
            _dividerTiles.Options.ScaleOf = turn;
            _dividerTiles.Options.AlphaOf = alpha;
            _startTiles.Repaint();
            _dividerTiles.Repaint();
            _wave.Options.Rise = Mathf.Lerp(0.15f, 1f, Mathf.Clamp01(turn(0)));
            _wave.Repaint();
            DividerStrip.style.opacity = alpha(0);
        }

        // Drift slides the DIVIDER only (its tiles, its mosaic or picture strip); the Start reveals once, then rests
        private void DriftFrame(float elapsed)
        {
            DriftOffset = DriftRule.Offset(elapsed, TileRule.Unit + 1f, _driftOptions);
            _dividerTiles.Options.Offset = DriftOffset;
            _dividerTiles.Repaint();
            if (Style.Divider == CardOptions.SectionDividerMosaic)
                Strip.Apply(DividerStrip, new StripOptions { Texture = MosaicRule.Texture(), Height = MosaicRule.Height, TileWidth = MosaicRule.Width, Offset = DriftRule.Offset(elapsed, MosaicRule.Width, _driftOptions) });
            else if (Style.Divider == CardOptions.SectionDividerImage && _image != null)
            {
                var o = new StripOptions { Texture = _image, Height = 24f };
                o.Offset = DriftRule.Offset(elapsed, o.RepeatWidth, _driftOptions);
                Strip.Apply(DividerStrip, o);
            }
        }

        private Color Colour(string token, Color fallback) => CardTokenColours.TryResolve(_host, token, out Color c) ? c : fallback;
    }
}

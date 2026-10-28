using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories
{
    // The card's moving light (_3.3 P4.4c2, R11, R8 S4): the ONE owner of the UI Shader Graph materials that paint it -- each section
    // sheet's living glass (SectionGlass: soft lights of the family's hues drifting inside the glass) and the rim light (RimSweep: the
    // highlight that sweeps the rim of the block being read). Each painted element gets its own copy of the base material, because its
    // size and colours are its own; the element is always a SIBLING layer behind the content (a material on a parent paints its children).
    //   - Colours are the THEME's, read BY NAME through ThemeResolver (CardTokenColours) and painted again on a mode switch; the graphs
    //     type no colour. They go in linear, as UI Toolkit hands its own colours to a shader in a linear project.
    //   - Time is the card's Clock (a test sets it), pushed into every material's _Clock each tick; Reduce Motion sets _Animate 0 on all,
    //     the still frame (container.reduce_motion; the visitor's switch and the OS flag join it later).
    // Plain C#: BlockStackView owns one; SectionSheets asks it for the glass, RimLightView for the sweep.
    public sealed class CardLight
    {
        public const string GlassMaterialPath = "TileStories/Card/SectionGlass";
        public const string SweepMaterialPath = "TileStories/Card/RimSweep";
        // The rim light's width in degrees of its turn, per mode (the theme's number: narrower on dark metal)
        public const string SweepArcToken = "--ts-rim-sweep-arc";
        // The rim's metal band per mode (the theme's number: 3 on light metal, a hairline on dark), the band the sweep lights
        public const string RimBandToken = "--ts-rim-band";
        // One turn of the rim light, in seconds (the prototype's 5 s spin)
        public const float SweepPeriod = 5f;
        private const long TickMs = 33;

        public static readonly int SizeId = Shader.PropertyToID("_Size");
        public static readonly int HueId = Shader.PropertyToID("_Hue");
        public static readonly int Hue2Id = Shader.PropertyToID("_Hue2");
        public static readonly int SweepId = Shader.PropertyToID("_Sweep");
        public static readonly int ArcId = Shader.PropertyToID("_Arc");
        public static readonly int ClockId = Shader.PropertyToID("_Clock");
        public static readonly int AnimateId = Shader.PropertyToID("_Animate");
        public static readonly int SeedId = Shader.PropertyToID("_Seed");
        public static readonly int PeriodId = Shader.PropertyToID("_Period");

        // One painted element: its material copy and the family colour token it reads now (null = paints nothing)
        private sealed class Layer
        {
            public VisualElement Element;
            public Material Material;
            public string Token;
            public bool Sweep;
        }

        private readonly VisualElement _host;
        private readonly Material _glassBase;
        private readonly Material _sweepBase;
        private readonly List<Layer> _layers = new();

        public CardLight(VisualElement host)
        {
            _host = host;
            _glassBase = Resources.Load<Material>(GlassMaterialPath);
            _sweepBase = Resources.Load<Material>(SweepMaterialPath);
            Debug.Assert(_glassBase != null && _sweepBase != null, "[Card] the light materials are missing from Resources/" + GlassMaterialPath);
            // - a colour painted from the theme does not follow a mode class: paint again when the mode changes (only while on a panel)
            host.RegisterCallback<AttachToPanelEvent>(_ => { ThemeController.ModeChanged -= PaintColours; ThemeController.ModeChanged += PaintColours; });
            host.RegisterCallback<DetachFromPanelEvent>(_ => ThemeController.ModeChanged -= PaintColours);
            host.schedule.Execute(Tick).Every(TickMs);
        }

        // The card's time in seconds; every material reads it as _Clock
        public Func<float> Clock { get; set; } = () => Time.unscaledTime;

        // Whether the light stands still (Reduce Motion): every material's _Animate 0
        public bool ReduceMotion { get; private set; }

        public void SetReduceMotion(bool on)
        {
            ReduceMotion = on;
            foreach (var layer in _layers) layer.Material.SetFloat(AnimateId, on ? 0f : 1f);
        }

        // Paint `element` as a section's living glass in the family colour `colorToken`'s lights (null = a sheet with no glass light);
        // `seed` sets where its lights start their drift, so two sheets never move in step
        public Material Glass(VisualElement element, string colorToken, float seed)
        {
            var layer = LayerOf(element, sweep: false);
            layer.Token = colorToken;
            layer.Material.SetFloat(SeedId, seed);
            PaintColours(layer);
            return layer.Material;
        }

        // Paint `element` as the rim light, in the colour of the family `colorToken` (the lit block's)
        public Material Sweep(VisualElement element, string colorToken)
        {
            var layer = LayerOf(element, sweep: true);
            layer.Token = colorToken;
            PaintColours(layer);
            return layer.Material;
        }

        // Push the clock into every material now (the schedule does it every tick)
        internal void Tick()
        {
            float now = Clock();
            foreach (var layer in _layers) layer.Material.SetFloat(ClockId, now);
        }

        // The element's layer, made once: its own material copy, sized by its layout
        private Layer LayerOf(VisualElement element, bool sweep)
        {
            foreach (var existing in _layers)
                if (existing.Element == element) return existing;
            var layer = new Layer { Element = element, Sweep = sweep, Material = new Material(sweep ? _sweepBase : _glassBase) };
            layer.Material.SetFloat(AnimateId, ReduceMotion ? 0f : 1f);
            layer.Material.SetFloat(ClockId, Clock());
            if (sweep) layer.Material.SetFloat(PeriodId, SweepPeriod);
            element.style.unityMaterial = new StyleMaterialDefinition(layer.Material);
            element.RegisterCallback<GeometryChangedEvent>(_ => Resize(layer));
            // - painted again once on a panel: before that the element sits under no themed root (CardTokenColours finds nothing)
            element.RegisterCallback<AttachToPanelEvent>(_ => PaintColours(layer));
            _layers.Add(layer);
            return layer;
        }

        // The material's size in points; the sweep's also its rounded edge (the block's radius inside the rim's outer line) and the band it
        // lights: the rim's metal band, just inside that line (the outer line is the block's measured edge and never lit) -- as wide as the
        // band the theme gives the mode shown (--ts-rim-band), never under two lines (_3.3 S4a)
        private static void Resize(Layer layer)
        {
            var rect = layer.Element.layout;
            // - not laid out yet (painted before its first layout pass): its geometry change sizes it
            if (float.IsNaN(rect.width) || float.IsNaN(rect.height)) return;
            if (!layer.Sweep)
            {
                layer.Material.SetVector(SizeId, new Vector4(rect.width, rect.height, 0f, 0f));
                return;
            }
            var slot = layer.Element.parent;
            var block = slot?.resolvedStyle;
            float line = block?.borderTopWidth ?? 1f;
            float radius = Mathf.Max(0f, (block?.borderTopLeftRadius ?? 0f) - line);
            float band = Mathf.Max(2f * line, RimBand(layer.Element, slot));
            layer.Material.SetVector(SizeId, new Vector4(rect.width, rect.height, radius, band));
        }

        // The rim's metal band in the mode shown NOW: the theme's number, never the rim part's resolved border -- that one is the last
        // style pass's, so right after a mode switch (and in a slot the light just moved into) it still holds the old mode's width
        private static float RimBand(VisualElement element, VisualElement slot)
        {
            if (ThemeController.TryResolverFor(element, out var theme) && theme.TryPixels(RimBandToken, out float band)) return band;
            float drawn = slot?.Q(className: BlockFrame.RimClass)?.resolvedStyle.borderTopWidth ?? 0f;
            return float.IsNaN(drawn) ? 0f : drawn;
        }

        // The band the sweep on `element` lights now (points; 0 when it is no sweep layer): what the frame tests read
        public float SweepBand(VisualElement element)
        {
            foreach (var layer in _layers)
                if (layer.Element == element && layer.Sweep) return layer.Material.GetVector(SizeId).w;
            return 0f;
        }

        private void PaintColours()
        {
            foreach (var layer in _layers) PaintColours(layer);
        }

        // The layer's colours in the theme its element sits under now (nothing found = transparent: it paints nothing)
        private static void PaintColours(Layer layer)
        {
            var element = layer.Element;
            if (layer.Sweep)
            {
                layer.Material.SetVector(SweepId, Linear(element, SectionSheetRule.SweepOf(layer.Token)));
                layer.Material.SetFloat(ArcId, Arc(element));
                // - a mode switch changes the rim's band too (wider on light metal), and a move into a slot of the same size raises no
                //   geometry change: the band is set here, from the theme, every time the sweep is painted
                Resize(layer);
                return;
            }
            layer.Material.SetVector(HueId, Linear(element, layer.Token == null ? null : SectionSheetRule.LightOf(layer.Token)));
            layer.Material.SetVector(Hue2Id, Linear(element, layer.Token == null ? null : SectionSheetRule.Light2Of(layer.Token)));
        }

        // The rim light's arc in the mode the element is shown in (0 = no light: the theme holds no arc)
        private static float Arc(VisualElement element) =>
            ThemeController.TryResolverFor(element, out var theme) && theme.TryNumber(SweepArcToken, out float arc) ? arc : 0f;

        // A theme colour as a shader reads it: linear rgb, its own alpha; transparent when the token is unknown here
        private static Vector4 Linear(VisualElement element, string token)
        {
            if (token == null || !CardTokenColours.TryResolve(element, token, out Color colour)) return Vector4.zero;
            var linear = colour.linear;
            return new Vector4(linear.r, linear.g, linear.b, colour.a);
        }
    }
}

using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories
{
    // The Open Row (_3.3 R14.8, R17.5): the SHARED last line of a preview that opens a page -- verb + object + reading time + a chevron
    // ("Read the story · 4 min ›"), a whole touch target (--ts-touch-target tall, Components.uss). Its emphasis comes from the block's
    // override, else the wall's (Detail Card > Card Components): plain; nudge -- the chevron steps towards the page; shine -- the nudge plus
    // a light crossing the words, a per-glyph tint sweep (TextElement.PostProcessTextVertices) from the words' colour to --ts-accent-shine,
    // both >= 7:1 (ThemeContrastTests), so the brightest frame still reads (D1). Reduce Motion: plain (OpenRowMotionRule). Time is Clock's (a
    // test sets it); it ticks only while the row is on a panel and moving. Host-agnostic: whoever owns the row decides what a tap opens.
    public sealed class OpenRowView : VisualElement
    {
        public const string RootClass = "ts-open-row";
        // The shine's colour: the row's own token, the theme's --ts-accent-shine (Components.uss)
        private static readonly CustomStyleProperty<Color> ShineProperty = new("--ts-open-row-shine");

        public Label Words { get; }
        public CardIcons.VectorGlyph Chevron { get; }
        // The emphasis drawn now (plain / nudge / shine)
        public string Emphasis { get; private set; } = ElementOptions.EmphasisPlain;
        // The time the motion reads, in seconds (the row's own clock unless a test hands one)
        public Func<float> Clock { get; set; } = () => Time.realtimeSinceStartup;
        // The tint each glyph took on the last repaint (what a test reads to find the brightest frame)
        public Color32[] LastGlyphTints { get; private set; } = Array.Empty<Color32>();

        private readonly IVisualElementScheduledItem _tick;
        private Color _shine;

        public OpenRowView()
        {
            name = "ts-open-row";
            AddToClassList(RootClass);
            AddToClassList("card-tap");
            pickingMode = PickingMode.Ignore;
            Words = new Label { name = "ts-open-row-words", pickingMode = PickingMode.Ignore };
            Words.AddToClassList(RootClass + "__words");
            Chevron = CardIcons.CreateVector(CardIcons.Shape.Chevron);
            Chevron.name = "ts-open-row-chevron";
            Chevron.AddToClassList(RootClass + "__chevron");
            Add(Words);
            Add(Chevron);
            Words.PostProcessTextVertices = Shine;
            RegisterCallback<CustomStyleResolvedEvent>(_ =>
            {
                if (customStyle.TryGetValue(ShineProperty, out var shine)) _shine = shine;
            });
            _tick = schedule.Execute(Tick).Every(33);
            _tick.Pause();
            RegisterCallback<AttachToPanelEvent>(_ => Run());
            RegisterCallback<DetachFromPanelEvent>(_ => _tick.Pause());
        }

        // Show `words` ("Read the story · 4 min") with `emphasis`, still under `reduceMotion`
        public void Show(string words, string emphasis, bool reduceMotion)
        {
            Words.text = words ?? "";
            Emphasis = OpenRowMotionRule.Effective(emphasis, reduceMotion);
            Run();
            Tick();
        }

        // One frame of the motion at Clock's time (also what a test calls after moving the clock)
        public void Tick()
        {
            float time = Clock();
            Chevron.style.translate = OpenRowMotionRule.Nudges(Emphasis)
                ? new Translate(OpenRowMotionRule.NudgeOffset(time), 0f)
                : new StyleTranslate(StyleKeyword.Null);
            Words.MarkDirtyRepaint();
        }

        private void Run()
        {
            if (panel != null && Emphasis != ElementOptions.EmphasisPlain) _tick.Resume();
            else _tick.Pause();
        }

        // Every glyph's tint: the words' colour, lit towards the shine colour under the band (shine only; plain / nudge keep the words' own)
        private void Shine(TextElement.GlyphsEnumerable glyphs)
        {
            if (LastGlyphTints.Length != glyphs.Count) LastGlyphTints = new Color32[glyphs.Count];
            bool shines = OpenRowMotionRule.Shines(Emphasis);
            float minX = float.MaxValue, maxX = float.MinValue;
            if (shines)
                foreach (var glyph in glyphs)
                    for (int v = 0; v < glyph.vertices.Length; v++)
                    {
                        minX = Mathf.Min(minX, glyph.vertices[v].position.x);
                        maxX = Mathf.Max(maxX, glyph.vertices[v].position.x);
                    }
            float centre = OpenRowMotionRule.ShineCentre(Clock());
            int i = 0;
            foreach (var glyph in glyphs)
            {
                var vertices = glyph.vertices;
                if (vertices.Length == 0)
                {
                    i++;
                    continue;
                }
                Color32 tint = vertices[0].tint;
                if (shines && maxX > minX)
                {
                    float x = 0f;
                    for (int v = 0; v < vertices.Length; v++) x += vertices[v].position.x;
                    float share = (x / vertices.Length - minX) / (maxX - minX);
                    float amount = OpenRowMotionRule.ShineAmount(share, centre);
                    // - from the words' own resolved colour: the shine colour is a theme token, never typed here
                    tint = Color.Lerp(Words.resolvedStyle.color, _shine, amount);
                    for (int v = 0; v < vertices.Length; v++)
                    {
                        var vertex = vertices[v];
                        vertex.tint = tint;
                        vertices[v] = vertex;
                    }
                }
                if (i < LastGlyphTints.Length) LastGlyphTints[i] = tint;
                i++;
            }
        }
    }
}

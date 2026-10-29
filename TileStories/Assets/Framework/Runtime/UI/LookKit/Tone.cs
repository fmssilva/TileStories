using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories.LookKit
{
    // The two shapes a Tone takes: Radial -- an ellipse of darkening round an anchor (light falling off from a corner); Edge -- a darkening
    // rising from a line (an edge scrim: dense at the anchor's height and below, fading upwards)
    public enum ToneShape { Radial, Edge }

    // A Tone's options (_3.3 P3c addendum, "Words On Picture"): its shape, where it is anchored, its reach (Radial: the ellipse's two radii;
    // Edge: only y, how far up it fades), its stops as a CSS gradient writes them (0 = at the anchor, 1 = at the reach; alpha of Colour, in
    // the gamma blend a page uses), its colour, the strength it is drawn at (a multiplier of every stop) and the drawn grid's cell
    public sealed class ToneOptions
    {
        public ToneShape Shape = ToneShape.Radial;
        public Vector2 Anchor;
        public Vector2 Reach = Vector2.one;
        public IReadOnlyList<(float At, float Alpha)> Stops = new[] { (0f, 0f) };
        public Color Colour = Color.black;
        public float Strength = 1f;
        public float Cell = 6f;

        // The strongest stop (the most any point gets at strength 1)
        public float Peak
        {
            get
            {
                float most = 0f;
                foreach (var s in Stops) most = Mathf.Max(most, s.Alpha);
                return most;
            }
        }
    }

    // The Tone's rule, pure: how far a point is along the gradient, the stops' alpha there (straight between stops, the first stop's before
    // it, nothing past the last), and that alpha DRAWN: a page blends a dark colour in gamma, the panel in linear light, so the gamma blend's
    // darkening is reproduced exactly (1 - (1 - a)^2.2 for a colour at black) -- the prototype's look, not a paler one. And the strength
    // that brings a given darkening to a point
    public static class ToneRule
    {
        // How far `p` lies along the tone (0 at the anchor, 1 at its reach; Edge: 0 at the anchor's height and below)
        public static float Distance(ToneOptions o, Vector2 p)
        {
            var d = p - o.Anchor;
            if (o.Shape == ToneShape.Edge) return Mathf.Max(0f, -d.y / Mathf.Max(1e-4f, o.Reach.y));
            float x = d.x / Mathf.Max(1e-4f, o.Reach.x), y = d.y / Mathf.Max(1e-4f, o.Reach.y);
            return Mathf.Sqrt(x * x + y * y);
        }

        // The stops' alpha at `r` along them
        public static float StopAlpha(IReadOnlyList<(float At, float Alpha)> stops, float r)
        {
            if (stops == null || stops.Count == 0) return 0f;
            if (r <= stops[0].At) return stops[0].Alpha;
            for (int i = 1; i < stops.Count; i++)
                if (r <= stops[i].At)
                {
                    float span = stops[i].At - stops[i - 1].At;
                    return span <= 0f ? stops[i].Alpha : Mathf.Lerp(stops[i - 1].Alpha, stops[i].Alpha, (r - stops[i - 1].At) / span);
                }
            return 0f;
        }

        // The gamma-blend alpha at `p` (the stops times the strength, never past 1)
        public static float PageAlpha(ToneOptions o, Vector2 p) => o == null ? 0f : Mathf.Clamp01(o.Strength * StopAlpha(o.Stops, Distance(o, p)));

        // The alpha the panel draws at `p` (its linear blend) for the page's gamma darkening
        public static float AlphaAt(ToneOptions o, Vector2 p) => Drawn(PageAlpha(o, p));

        // A gamma-blend alpha as the panel's linear one, and back
        public static float Drawn(float pageAlpha) => 1f - Mathf.Pow(1f - Mathf.Clamp01(pageAlpha), 2.2f);
        public static float Page(float drawnAlpha) => 1f - Mathf.Pow(1f - Mathf.Clamp01(drawnAlpha), 1f / 2.2f);

        // The relative luminance (WCAG) of a colour given in gamma
        public static float Luminance(Color c) => 0.2126f * c.linear.r + 0.7152f * c.linear.g + 0.0722f * c.linear.b;

        // The least drawn alpha (0..1) of a colour of luminance `shadeY` over a ground whose brightest luminance is `groundY` that keeps an ink
        // of luminance `inkY` at `ratio` (linear blend: ground (1 - A) + shade A); 0 when the ground is dark enough, 1 when even full cannot
        public static float NeededAlpha(float groundY, float inkY, float shadeY, float ratio)
        {
            if (float.IsNaN(groundY)) return 1f;
            float most = (inkY + 0.05f) / Mathf.Max(1f, ratio) - 0.05f;
            if (groundY <= most) return 0f;
            if (most <= shadeY) return 1f;
            return Mathf.Clamp01((groundY - most) / (groundY - shadeY));
        }

        // The strength that draws `needed` (a linear alpha) where the stops give `stopAlpha` at strength 1 -- never past what makes the peak
        // full (1 / Peak): a point the stops barely reach cannot be bought by turning the anchor black
        public static float StrengthFor(float needed, float stopAlpha, float peak)
        {
            if (!(needed > 0f)) return 0f;
            float cap = peak > 0f ? 1f / peak : 1f;
            return stopAlpha > 0f ? Mathf.Min(cap, Page(needed) / stopAlpha) : cap;
        }
    }

    // The Tone drawn on an element: a grid of cells over its whole box, each vertex tinted with the rule's drawn alpha -- the panel blends
    // between them, an exact soft gradient at any size, no texture. Add it with element.AddManipulator(new TonePainter()) and hand it its
    // options as they change (Options)
    public sealed class TonePainter : Manipulator
    {
        // UI Toolkit indexes a mesh with 16 bits: the grid grows its cells rather than pass that
        private const int MaxVertices = 60000;

        private ToneOptions _options;

        public ToneOptions Options
        {
            get => _options;
            set
            {
                _options = value;
                target?.MarkDirtyRepaint();
            }
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.generateVisualContent += Draw;
            target.MarkDirtyRepaint();
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.generateVisualContent -= Draw;
            target.MarkDirtyRepaint();
        }

        private void Draw(MeshGenerationContext mgc)
        {
            if (_options == null || !(_options.Strength > 0f)) return;
            var area = target.contentRect;
            if (!(area.width > 0f) || !(area.height > 0f)) return;
            float cell = _options.Cell;
            int cols = Mathf.CeilToInt(area.width / cell), rows = Mathf.CeilToInt(area.height / cell);
            while ((cols + 1) * (rows + 1) > MaxVertices)
            {
                cell *= 1.5f;
                cols = Mathf.CeilToInt(area.width / cell);
                rows = Mathf.CeilToInt(area.height / cell);
            }
            var mesh = mgc.Allocate((cols + 1) * (rows + 1), cols * rows * 6);
            for (int r = 0; r <= rows; r++)
            {
                float y = Mathf.Min(area.yMin + r * cell, area.yMax);
                for (int c = 0; c <= cols; c++)
                {
                    float x = Mathf.Min(area.xMin + c * cell, area.xMax);
                    var tint = _options.Colour;
                    tint.a = ToneRule.AlphaAt(_options, new Vector2(x, y));
                    mesh.SetNextVertex(new Vertex { position = new Vector3(x, y, Vertex.nearZ), tint = tint });
                }
            }
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                {
                    // - clockwise on the panel (y grows down): top-left, top-right, foot-right / top-left, foot-right, foot-left
                    ushort v = (ushort)(r * (cols + 1) + c), right = (ushort)(v + 1), below = (ushort)(v + cols + 1), corner = (ushort)(v + cols + 2);
                    mesh.SetNextIndex(v);
                    mesh.SetNextIndex(right);
                    mesh.SetNextIndex(corner);
                    mesh.SetNextIndex(v);
                    mesh.SetNextIndex(corner);
                    mesh.SetNextIndex(below);
                }
        }
    }
}

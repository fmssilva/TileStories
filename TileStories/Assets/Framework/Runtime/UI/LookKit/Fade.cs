using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories.LookKit
{
    // One stop of a Fade: how far down the element it sits (panel units from the element's top) and the colour there -- its alpha is the
    // fade's amount at that line
    public readonly struct FadeStop
    {
        public readonly float Offset;
        public readonly Color Colour;

        public FadeStop(float offset, Color colour)
        {
            Offset = offset;
            Colour = colour;
        }
    }

    // A Fade's options (the Look Kit's first effect, _3.3 fourth round): its stops, top to bottom. Two stops at one offset make a hard edge
    // (the colour changes there); above the first stop and below the last the end colours hold
    public sealed class FadeOptions
    {
        public readonly IReadOnlyList<FadeStop> Stops;

        public FadeOptions(IReadOnlyList<FadeStop> stops) => Stops = stops ?? new List<FadeStop>();

        // A plain ramp: clear above `start`, `colour` at `start`, fading linearly to clear over `length`
        public static FadeOptions Ramp(Color colour, float start, float length)
        {
            var clear = colour;
            clear.a = 0f;
            return new FadeOptions(new List<FadeStop>
            {
                new(start, clear), new(start, colour), new(start + Mathf.Max(0f, length), clear),
            });
        }
    }

    // The Fade's rule, pure: the colour at a line of the element -- straight lines between the stops (each channel and the alpha), the end
    // colours held past them. `below`: on a hard edge (two stops at one offset), the colour just under the line (else just above it)
    public static class FadeRule
    {
        public static Color ColourAt(IReadOnlyList<FadeStop> stops, float y, bool below = true)
        {
            if (stops == null || stops.Count == 0) return Color.clear;
            if (y < stops[0].Offset || (!below && y <= stops[0].Offset)) return stops[0].Colour;
            for (int i = 0; i < stops.Count - 1; i++)
            {
                var a = stops[i];
                var b = stops[i + 1];
                bool inside = below ? y >= a.Offset && y < b.Offset : y > a.Offset && y <= b.Offset;
                if (!inside) continue;
                float span = b.Offset - a.Offset;
                return span <= 0f ? b.Colour : Color.LerpUnclamped(a.Colour, b.Colour, (y - a.Offset) / span);
            }
            return stops[stops.Count - 1].Colour;
        }

        // The lines a drawn fade is cut at inside a `height`-tall element: its top, every stop inside it, its foot -- in order, each once
        public static List<float> Cuts(IReadOnlyList<FadeStop> stops, float height)
        {
            var cuts = new List<float> { 0f };
            if (stops != null)
                foreach (var stop in stops)
                    if (stop.Offset > 0f && stop.Offset < height && !cuts.Contains(stop.Offset)) cuts.Add(stop.Offset);
            if (height > 0f) cuts.Add(height);
            cuts.Sort();
            return cuts;
        }
    }

    // The Fade drawn on an element: one band per pair of cuts, a quad whose top and foot take the rule's colours (the panel blends between
    // them) -- an exact gradient at any length, colour and alpha, no texture. Add it with element.AddManipulator(new FadePainter(options))
    public sealed class FadePainter : Manipulator
    {
        private FadeOptions _options;

        public FadePainter(FadeOptions options) => _options = options;

        public FadeOptions Options
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
            var stops = _options?.Stops;
            var box = target.contentRect;
            if (stops == null || stops.Count == 0 || box.width <= 0f || box.height <= 0f) return;
            var cuts = FadeRule.Cuts(stops, box.height);
            int bands = cuts.Count - 1;
            if (bands <= 0) return;
            var mesh = mgc.Allocate(bands * 4, bands * 6);
            for (int i = 0; i < bands; i++)
            {
                float top = cuts[i], foot = cuts[i + 1];
                Color32 upper = FadeRule.ColourAt(stops, top, below: true), lower = FadeRule.ColourAt(stops, foot, below: false);
                // - clockwise on the panel (y grows down): top-left, top-right, foot-right, foot-left
                mesh.SetNextVertex(new Vertex { position = new Vector3(box.xMin, box.yMin + top, Vertex.nearZ), tint = upper });
                mesh.SetNextVertex(new Vertex { position = new Vector3(box.xMax, box.yMin + top, Vertex.nearZ), tint = upper });
                mesh.SetNextVertex(new Vertex { position = new Vector3(box.xMax, box.yMin + foot, Vertex.nearZ), tint = lower });
                mesh.SetNextVertex(new Vertex { position = new Vector3(box.xMin, box.yMin + foot, Vertex.nearZ), tint = lower });
                ushort v = (ushort)(i * 4);
                mesh.SetNextIndex(v);
                mesh.SetNextIndex((ushort)(v + 1));
                mesh.SetNextIndex((ushort)(v + 2));
                mesh.SetNextIndex(v);
                mesh.SetNextIndex((ushort)(v + 2));
                mesh.SetNextIndex((ushort)(v + 3));
            }
        }
    }
}

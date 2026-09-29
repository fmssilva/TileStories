using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories
{
    // The card's ONE icon set (_3.1 Tier 1): small line icons drawn by CardParts.uss from four generic parts -- a frame (a
    // ring or a box), two bars and a dot -- inside an 18 px glyph box; each key's class shows and places the parts. No
    // picture and no glyph font: the icons follow the card's tokens (colour, stroke, size) like every other element.
    // Used by the actions buttons (their action's icon) and practical_info (a row's icon). A look may resize the icon's
    // root (the actions' circles) -- the glyph box keeps the drawing its own size, centred.
    public static class CardIcons
    {
        public const string ShowOnWall = "show_on_wall";
        public const string Time = "time";
        public const string Access = "access";
        public const string Location = "location";
        public const string Info = "info";
        public const string Ticket = "ticket";
        public const string Light = "light";

        // Every key the set draws
        public static readonly IReadOnlyList<string> All = new[] { ShowOnWall, Time, Access, Location, Info, Ticket, Light };

        // A new icon element (no key yet: SetKey picks the drawing)
        public static VisualElement Create()
        {
            var icon = new VisualElement { pickingMode = PickingMode.Ignore };
            icon.AddToClassList("card-icon");
            var glyph = new VisualElement { pickingMode = PickingMode.Ignore };
            glyph.AddToClassList("card-icon__glyph");
            foreach (string part in new[] { "card-icon__frame", "card-icon__bar card-icon__bar--a", "card-icon__bar card-icon__bar--b", "card-icon__dot" })
            {
                var e = new VisualElement { pickingMode = PickingMode.Ignore };
                foreach (string c in part.Split(' ')) e.AddToClassList(c);
                glyph.Add(e);
            }
            icon.Add(glyph);
            return icon;
        }

        // Draw `key` on an icon made by Create (a key the set does not have draws nothing)
        public static void SetKey(VisualElement icon, string key)
        {
            foreach (string known in All)
                icon.EnableInClassList("card-icon--" + known, known == key);
        }

        // The key an icon draws now, or null
        public static string KeyOf(VisualElement icon)
        {
            foreach (string known in All)
                if (icon.ClassListContains("card-icon--" + known)) return known;
            return null;
        }

        // ---- vector glyphs (8A-fix): shapes USS borders cannot draw, painted with Painter2D ----

        // The shapes a VectorGlyph draws
        public enum Shape { Tick, Cross, Star, ThumbUp, ThumbDown, Play, Pause }

        // One outline of a shape, in 0..1 units of the glyph's square. `Points` are joined in order; a `Closed` outline can be filled.
        public readonly struct Outline
        {
            public readonly Vector2[] Points;
            public readonly bool Closed;

            public Outline(Vector2[] points, bool closed)
            {
                Points = points;
                Closed = closed;
            }
        }

        // The line width of a shape as a share of the glyph's side: a stroke that scales with the glyph, so it needs no token
        public static float StrokeUnits(Shape shape) => shape == Shape.Tick || shape == Shape.Cross ? 0.13f : 0.07f;

        // The outlines of `shape` in 0..1 units (pure, so a test can check every point stays inside the glyph)
        public static IReadOnlyList<Outline> OutlinesOf(Shape shape)
        {
            switch (shape)
            {
                case Shape.Tick:
                    return new[] { new Outline(new[] { new Vector2(0.14f, 0.54f), new Vector2(0.40f, 0.80f), new Vector2(0.86f, 0.22f) }, false) };
                case Shape.Cross:
                    return new[]
                    {
                        new Outline(new[] { new Vector2(0.2f, 0.2f), new Vector2(0.8f, 0.8f) }, false),
                        new Outline(new[] { new Vector2(0.8f, 0.2f), new Vector2(0.2f, 0.8f) }, false),
                    };
                case Shape.Star:
                    return new[] { new Outline(StarPoints(), true) };
                case Shape.Play:
                    // - a triangle pointing right, a hair right of centre so it reads centred (its weight sits at the left)
                    return new[] { new Outline(new[] { new Vector2(0.30f, 0.16f), new Vector2(0.84f, 0.50f), new Vector2(0.30f, 0.84f) }, true) };
                case Shape.Pause:
                    return new[]
                    {
                        new Outline(new[] { new Vector2(0.24f, 0.16f), new Vector2(0.42f, 0.16f), new Vector2(0.42f, 0.84f), new Vector2(0.24f, 0.84f) }, true),
                        new Outline(new[] { new Vector2(0.58f, 0.16f), new Vector2(0.76f, 0.16f), new Vector2(0.76f, 0.84f), new Vector2(0.58f, 0.84f) }, true),
                    };
                case Shape.ThumbUp:
                    return ThumbOutlines(false);
                default:
                    return ThumbOutlines(true);
            }
        }

        // A five-pointed star: ten points alternating the outer and the inner radius, the first straight up
        private static Vector2[] StarPoints()
        {
            const float centreY = 0.55f;
            const float outer = 0.46f;
            const float inner = 0.19f;
            var points = new Vector2[10];
            for (int i = 0; i < points.Length; i++)
            {
                float radius = i % 2 == 0 ? outer : inner;
                float angle = (-90f + 36f * i) * Mathf.Deg2Rad;
                points[i] = new Vector2(0.5f + radius * Mathf.Cos(angle), centreY + radius * Mathf.Sin(angle));
            }
            return points;
        }

        // A thumb: the cuff (a bar at the left) and the hand with its thumb pointing up; the thumb down is the same turned over
        private static Outline[] ThumbOutlines(bool down)
        {
            var hand = new List<Vector2> { new Vector2(0.34f, 0.46f), new Vector2(0.47f, 0.13f) };
            AddCurve(hand, hand[1], new Vector2(0.50f, 0.05f), new Vector2(0.58f, 0.09f));
            AddCurve(hand, new Vector2(0.58f, 0.09f), new Vector2(0.66f, 0.13f), new Vector2(0.62f, 0.26f));
            hand.Add(new Vector2(0.58f, 0.40f));
            hand.Add(new Vector2(0.86f, 0.40f));
            AddCurve(hand, new Vector2(0.86f, 0.40f), new Vector2(0.96f, 0.40f), new Vector2(0.96f, 0.50f));
            hand.Add(new Vector2(0.96f, 0.80f));
            AddCurve(hand, new Vector2(0.96f, 0.80f), new Vector2(0.96f, 0.90f), new Vector2(0.86f, 0.90f));
            hand.Add(new Vector2(0.34f, 0.90f));
            var cuff = new List<Vector2> { new Vector2(0.06f, 0.44f), new Vector2(0.26f, 0.44f), new Vector2(0.26f, 0.90f), new Vector2(0.06f, 0.90f) };
            if (down)
            {
                for (int i = 0; i < hand.Count; i++) hand[i] = new Vector2(hand[i].x, 1f - hand[i].y);
                for (int i = 0; i < cuff.Count; i++) cuff[i] = new Vector2(cuff[i].x, 1f - cuff[i].y);
            }
            return new[] { new Outline(hand.ToArray(), true), new Outline(cuff.ToArray(), true) };
        }

        // Append the points of a quadratic curve from `from` (already in the list) through control `control` to `to`
        private static void AddCurve(List<Vector2> points, Vector2 from, Vector2 control, Vector2 to)
        {
            const int steps = 6;
            for (int i = 1; i <= steps; i++)
            {
                float t = i / (float)steps;
                points.Add((1f - t) * (1f - t) * from + 2f * (1f - t) * t * control + t * t * to);
            }
        }

        // A new vector glyph of `shape` (size and colour come from the element's USS: `width` / `height` and `color`)
        public static VectorGlyph CreateVector(Shape shape) => new VectorGlyph(shape);

        // One shape painted with Painter2D into the largest centred square of its content box. The colour is the element's resolved
        // `color` (a --ts-* token in USS), the stroke a share of the side; `Filled` fills the closed outlines (a star / thumb "on"),
        // else they are only outlined. Repaints when the style, the size or `Filled` change.
        public sealed class VectorGlyph : VisualElement
        {
            private Shape _shape;
            private bool _filled;

            public Shape Kind
            {
                get => _shape;
                set
                {
                    _shape = value;
                    MarkDirtyRepaint();
                }
            }

            public bool Filled
            {
                get => _filled;
                set
                {
                    if (_filled == value) return;
                    _filled = value;
                    MarkDirtyRepaint();
                }
            }

            public VectorGlyph(Shape shape)
            {
                _shape = shape;
                pickingMode = PickingMode.Ignore;
                AddToClassList("card-glyph");
                generateVisualContent += Draw;
                RegisterCallback<CustomStyleResolvedEvent>(_ => MarkDirtyRepaint());
                RegisterCallback<GeometryChangedEvent>(_ => MarkDirtyRepaint());
            }

            private void Draw(MeshGenerationContext context)
            {
                Rect box = contentRect;
                float side = Mathf.Min(box.width, box.height);
                if (!(side > 0f)) return;
                var painter = context.painter2D;
                Color colour = resolvedStyle.color;
                painter.strokeColor = colour;
                painter.fillColor = colour;
                painter.lineWidth = side * StrokeUnits(_shape);
                painter.lineJoin = LineJoin.Round;
                painter.lineCap = LineCap.Round;
                Vector2 origin = new Vector2(box.x + (box.width - side) * 0.5f, box.y + (box.height - side) * 0.5f);
                foreach (var outline in OutlinesOf(_shape))
                {
                    painter.BeginPath();
                    for (int i = 0; i < outline.Points.Length; i++)
                    {
                        Vector2 at = origin + outline.Points[i] * side;
                        if (i == 0) painter.MoveTo(at);
                        else painter.LineTo(at);
                    }
                    if (outline.Closed) painter.ClosePath();
                    if (outline.Closed && _filled) painter.Fill();
                    painter.Stroke();
                }
            }
        }
    }
}

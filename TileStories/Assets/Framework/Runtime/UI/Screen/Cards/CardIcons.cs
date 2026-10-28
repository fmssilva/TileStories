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
        // The Open Block action (_3.2 step 4): an arrow down to a line -- "go to that part of this card"
        public const string OpenBlock = "open_block";
        // The families' icons (_3.2 step 6, FamilyDefinition.IconKey; about, ar and visit reuse info, show_on_wall and location):
        // a framed picture with a sun, headphones, an open book, a die, a person, a page of text
        public const string Picture = "picture";
        public const string Listen = "listen";
        public const string Book = "book";
        public const string Play = "play";
        public const string People = "people";
        public const string Sources = "sources";
        // A bar's More slot (_3.2 step 7a): three dots in a row
        public const string More = "more";
        // The header's menu button (_3.2 step 8): three bars, one above the other
        public const string Menu = "menu";
        // The navigation family (_3.2 step 11a): a box split in four -- the nav blocks' grid of the card's sections
        public const string Grid = "grid";
        // The kinds' icons (_3.2 step 17a, BlockKindDefinition.IconKey): what a sibling chip of the full view shows, so a visitor tells a 360
        // panorama from a gallery without opening either. A kind with no drawing of its own among the ones above gets one here:
        // a box split in two (before / after, a comparison), a magnifier, a cube (a 3D model), a wide strip (a panorama), a screen
        // (a video), a line with a milestone (a timeline), a speech bubble (a dialogue, a quote, feedback), upright bars (a poll)
        public const string Split = "split";
        public const string Zoom = "zoom";
        public const string Cube = "cube";
        public const string Panorama = "panorama";
        public const string Video = "video";
        public const string Timeline = "timeline";
        public const string Chat = "chat";
        public const string Chart = "chart";

        // Every key the set draws
        public static readonly IReadOnlyList<string> All = new[]
        {
            ShowOnWall, Time, Access, Location, Info, Ticket, Light, OpenBlock, Picture, Listen, Book, Play, People, Sources, More, Menu, Grid,
            Split, Zoom, Cube, Panorama, Video, Timeline, Chat, Chart,
        };

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

        // Paint an icon made by Create in `colour` instead of the accent (a family's colour, read from its token by CardTokenColours):
        // the frame's lines and the bars and the dot. Null puts the stylesheet's accent back
        public static void Tint(VisualElement icon, Color? colour)
        {
            foreach (var part in icon.Query(className: "card-icon__frame").ToList())
            {
                var border = colour.HasValue ? new StyleColor(colour.Value) : new StyleColor(StyleKeyword.Null);
                part.style.borderTopColor = border;
                part.style.borderRightColor = border;
                part.style.borderBottomColor = border;
                part.style.borderLeftColor = border;
            }
            foreach (string fill in new[] { "card-icon__bar", "card-icon__dot" })
                foreach (var part in icon.Query(className: fill).ToList())
                    part.style.backgroundColor = colour.HasValue ? new StyleColor(colour.Value) : new StyleColor(StyleKeyword.Null);
        }

        // ---- vector glyphs (8A-fix): shapes USS borders cannot draw, painted with Painter2D ----

        // The shapes a VectorGlyph draws
        public enum Shape { Tick, Cross, Star, ThumbUp, ThumbDown, Play, Pause, Stop, FullScreen, Chevron, Gear, Azulejo, QuoteOpen, Close, Sliders }

        // The card's chrome marks (Close, Sliders): the premium target's thin 1.5 stroke on a 16 mark (_3.3 S4 third round, v5)
        public const float ChromeStrokeUnits = 1.5f / 16f;

        // One outline of a shape, in 0..1 units of the glyph's square. `Points` are joined in order; a `Closed` outline can be filled; a
        // `Solid` one is always filled and never stroked (the azulejo's centre dot, a quotation mark)
        public readonly struct Outline
        {
            public readonly Vector2[] Points;
            public readonly bool Closed;
            public readonly bool Solid;

            public Outline(Vector2[] points, bool closed, bool solid = false)
            {
                Points = points;
                Closed = closed || solid;
                Solid = solid;
            }
        }

        // The line width of a shape as a share of the glyph's side: a stroke that scales with the glyph, so it needs no token. The ornament
        // is drawn fine, like the prototype's tile (1.6 of 34)
        public static float StrokeUnits(Shape shape) =>
            shape == Shape.Close || shape == Shape.Sliders ? ChromeStrokeUnits
            : shape == Shape.Tick || shape == Shape.Cross || shape == Shape.FullScreen || shape == Shape.Chevron ? 0.13f
            : shape == Shape.Azulejo ? 0.05f : 0.07f;

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
                case Shape.Stop:
                    // - a plain filled square, the universal stop glyph, same box as Play/Pause
                    return new[] { new Outline(new[] { new Vector2(0.22f, 0.22f), new Vector2(0.78f, 0.22f), new Vector2(0.78f, 0.78f), new Vector2(0.22f, 0.78f) }, true) };
                case Shape.FullScreen:
                    // - four open corner brackets pointing outwards: "make this fill the screen"
                    return new[]
                    {
                        new Outline(new[] { new Vector2(0.14f, 0.40f), new Vector2(0.14f, 0.14f), new Vector2(0.40f, 0.14f) }, false),
                        new Outline(new[] { new Vector2(0.60f, 0.14f), new Vector2(0.86f, 0.14f), new Vector2(0.86f, 0.40f) }, false),
                        new Outline(new[] { new Vector2(0.86f, 0.60f), new Vector2(0.86f, 0.86f), new Vector2(0.60f, 0.86f) }, false),
                        new Outline(new[] { new Vector2(0.40f, 0.86f), new Vector2(0.14f, 0.86f), new Vector2(0.14f, 0.60f) }, false),
                    };
                case Shape.Chevron:
                    // - an open "v" pointing down: "there is more below, tap to open" (a compact row turns it over once open)
                    return new[] { new Outline(new[] { new Vector2(0.18f, 0.36f), new Vector2(0.50f, 0.68f), new Vector2(0.82f, 0.36f) }, false) };
                case Shape.Gear:
                    return GearOutlines();
                case Shape.Close:
                    // - the card's X (v5: 6.5 .. 17.5 of 24): two crossed strokes, smaller than the Cross so the thin line reads as a mark
                    return new[]
                    {
                        new Outline(new[] { new Vector2(6.5f / 24f, 6.5f / 24f), new Vector2(17.5f / 24f, 17.5f / 24f) }, false),
                        new Outline(new[] { new Vector2(17.5f / 24f, 6.5f / 24f), new Vector2(6.5f / 24f, 17.5f / 24f) }, false),
                    };
                case Shape.Sliders:
                    return SlidersOutlines();
                case Shape.Azulejo:
                    return AzulejoOutlines();
                case Shape.QuoteOpen:
                    return new[] { new Outline(QuoteMark(0.30f), true, solid: true), new Outline(QuoteMark(0.66f), true, solid: true) };
                case Shape.ThumbUp:
                    return ThumbOutlines(false);
                default:
                    return ThumbOutlines(true);
            }
        }

        // The star's own centre in 0..1 units of the glyph's square: a hair below the box's middle (its points weigh the top). A pixel read
        // of an outlined star's hollow is taken here, never at the box's middle (that is 0.05 of a side off, near the stroke)
        public static readonly Vector2 StarCentre = new(0.5f, 0.55f);

        // A five-pointed star: ten points alternating the outer and the inner radius, the first straight up
        private static Vector2[] StarPoints()
        {
            const float outer = 0.46f;
            const float inner = 0.19f;
            var points = new Vector2[10];
            for (int i = 0; i < points.Length; i++)
            {
                float radius = i % 2 == 0 ? outer : inner;
                float angle = (-90f + 36f * i) * Mathf.Deg2Rad;
                points[i] = new Vector2(StarCentre.x + radius * Mathf.Cos(angle), StarCentre.y + radius * Mathf.Sin(angle));
            }
            return points;
        }

        // The card's settings (_3.3 P4.4b part 2, the universal "settings" sign): a wheel of eight teeth -- each a trapezoid from the root
        // circle out to the tip, its base wider than its tip -- around a round hole, both centred; outlined like the other line icons
        private static Outline[] GearOutlines()
        {
            const int teeth = 8;
            const float root = 0.33f, tip = 0.45f, hole = 0.14f;
            const float baseHalf = 13f, tipHalf = 8f;
            var wheel = new Vector2[teeth * 4];
            for (int i = 0; i < teeth; i++)
            {
                float centre = i * 360f / teeth - 90f;
                wheel[i * 4] = GearPoint(centre - baseHalf, root);
                wheel[i * 4 + 1] = GearPoint(centre - tipHalf, tip);
                wheel[i * 4 + 2] = GearPoint(centre + tipHalf, tip);
                wheel[i * 4 + 3] = GearPoint(centre + baseHalf, root);
            }
            var circle = new Vector2[16];
            for (int i = 0; i < circle.Length; i++) circle[i] = GearPoint(i * 360f / circle.Length, hole);
            return new[] { new Outline(wheel, true), new Outline(circle, true) };
        }

        // The card's settings (the premium target, v5: "sliders", not a gear -- the sign of things to adjust): two tracks, each broken by
        // its round knob, the upper knob right of centre, the lower one left (v5's path on a 24 grid)
        private static Outline[] SlidersOutlines()
        {
            const float u = 1f / 24f;
            Outline Line(float x0, float x1, float y) => new(new[] { new Vector2(x0 * u, y * u), new Vector2(x1 * u, y * u) }, false);
            Outline Knob(float cx, float cy)
            {
                var ring = new Vector2[16];
                for (int i = 0; i < ring.Length; i++)
                {
                    float a = i * Mathf.PI * 2f / ring.Length;
                    ring[i] = new Vector2((cx + 2f * Mathf.Cos(a)) * u, (cy + 2f * Mathf.Sin(a)) * u);
                }
                return new Outline(ring, true);
            }
            return new[] { Line(4f, 13f, 8f), Line(18f, 20f, 8f), Knob(15.5f, 8f), Line(4f, 7f, 16f), Line(11f, 20f, 16f), Knob(9f, 16f) };
        }

        // A small tile ornament (_3.3 S2a, the story's divider; the prototype's azulejo): a square tile, a four-pointed flower inside it whose
        // petals curve in towards the centre, and a dot at the centre
        private static Outline[] AzulejoOutlines()
        {
            var tile = new[] { new Vector2(0.03f, 0.03f), new Vector2(0.97f, 0.03f), new Vector2(0.97f, 0.97f), new Vector2(0.03f, 0.97f) };
            // - the prototype's path on a 34 grid: from each point to the next through two control points that pull the side inwards
            var flower = new List<Vector2> { new(0.50f, 0.12f) };
            AddCubic(flower, new(0.50f, 0.12f), new(0.62f, 0.32f), new(0.68f, 0.38f), new(0.88f, 0.50f));
            AddCubic(flower, new(0.88f, 0.50f), new(0.68f, 0.62f), new(0.62f, 0.68f), new(0.50f, 0.88f));
            AddCubic(flower, new(0.50f, 0.88f), new(0.38f, 0.68f), new(0.32f, 0.62f), new(0.12f, 0.50f));
            AddCubic(flower, new(0.12f, 0.50f), new(0.32f, 0.38f), new(0.38f, 0.32f), new(0.50f, 0.12f));
            var dot = new Vector2[12];
            for (int i = 0; i < dot.Length; i++) dot[i] = GearPoint(i * 360f / dot.Length, 0.09f);
            return new[] { new Outline(tile, true), new Outline(flower.ToArray(), true), new Outline(dot, true, solid: true) };
        }

        // One opening quotation mark's comma, its round head centred at x `cx` low in the box and its tail rising to the right
        private static Vector2[] QuoteMark(float cx)
        {
            const float r = 0.13f, cy = 0.62f;
            var points = new List<Vector2>();
            // - the head: a circle from its right side round the bottom and the left to its top (y grows downwards)
            for (int i = 0; i <= 9; i++)
            {
                float a = (i * 30f) * Mathf.Deg2Rad;
                points.Add(new Vector2(cx + r * Mathf.Cos(a), cy + r * Mathf.Sin(a)));
            }
            // - the tail: from the head's top up and to the right, then back down its inner edge to the head
            AddCurve(points, points[points.Count - 1], new Vector2(cx - 0.02f, cy - 0.32f), new Vector2(cx + 0.14f, cy - 0.40f));
            AddCurve(points, new Vector2(cx + 0.14f, cy - 0.40f), new Vector2(cx + 0.05f, cy - 0.28f), new Vector2(cx + r, cy));
            return points.ToArray();
        }

        // Append the points of a cubic curve from `from` (already in the list) through `c1`, `c2` to `to`
        private static void AddCubic(List<Vector2> points, Vector2 from, Vector2 c1, Vector2 c2, Vector2 to)
        {
            const int steps = 8;
            for (int i = 1; i <= steps; i++)
            {
                float t = i / (float)steps, u = 1f - t;
                points.Add(u * u * u * from + 3f * u * u * t * c1 + 3f * u * t * t * c2 + t * t * t * to);
            }
        }

        private static Vector2 GearPoint(float degrees, float radius) =>
            new(0.5f + radius * Mathf.Cos(degrees * Mathf.Deg2Rad), 0.5f + radius * Mathf.Sin(degrees * Mathf.Deg2Rad));

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
                // - a filled gear is ONE solid with its hub punched out (the wheel and the hole in one path, odd-even), stroked round
                //   so its teeth read soft, like the system's settings mark
                if (_filled && _shape == Shape.Gear)
                {
                    painter.BeginPath();
                    foreach (var outline in OutlinesOf(_shape)) Trace(painter, outline, origin, side);
                    painter.Fill(FillRule.OddEven);
                    painter.Stroke();
                    return;
                }
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
                    if ((outline.Closed && _filled) || outline.Solid) painter.Fill();
                    if (!outline.Solid) painter.Stroke();
                }
            }

            private static void Trace(Painter2D painter, Outline outline, Vector2 origin, float side)
            {
                for (int i = 0; i < outline.Points.Length; i++)
                {
                    Vector2 at = origin + outline.Points[i] * side;
                    if (i == 0) painter.MoveTo(at);
                    else painter.LineTo(at);
                }
                if (outline.Closed) painter.ClosePath();
            }
        }
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories.LookKit
{
    // The azulejo tile, pure (v6's tile, drawn by us): in a 24-unit square, the light ground, four quarter discs at the corners, four petals
    // round the centre and the centre dot -- each part one closed path of cubic curves, so a tile scales (and squeezes, for the Reveal) as a
    // vector, crisp at any size. One colour tints it; the dot takes the paper's.
    public static class TileRule
    {
        public const float Unit = 24f;
        public const float CornerRadius = 7f;
        public const float DotRadius = 1.7f;
        // Each part's alpha of the tile's one colour (v6)
        public const float GroundAlpha = 0.16f;
        public const float CornerAlpha = 0.55f;
        public const float PetalAlpha = 1f;

        public static readonly Vector2 Centre = new(12f, 12f);
        // A quarter circle's handle length for a radius of 1 (four cubics draw a circle to within 0.03 %)
        private const float Kappa = 0.5522848f;

        public enum Part { Ground, Corner, Petal, Dot }

        // One closed path: a start point, then cubic segments (control, control, end) back to it
        public readonly struct Shape
        {
            public readonly Part Part;
            public readonly Vector2 Start;
            public readonly IReadOnlyList<Vector2> Cubics;

            public Shape(Part part, Vector2 start, IReadOnlyList<Vector2> cubics)
            {
                Part = part;
                Start = start;
                Cubics = cubics;
            }
        }

        // The petal pointing up (v6: "M12 3.2C14.8 6.6 14.8 9.4 12 12C9.2 9.4 9.2 6.6 12 3.2Z"); the other three are it turned about the centre
        private static readonly Vector2[] UpPetal =
        {
            new(14.8f, 6.6f), new(14.8f, 9.4f), new(12f, 12f),
            new(9.2f, 9.4f), new(9.2f, 6.6f), new(12f, 3.2f),
        };

        // Every part of the tile in drawing order: ground, the four corners (top-left, top-right, bottom-right, bottom-left), the four petals
        // (up, right, down, left: each a quarter turn clockwise of the last), the dot
        public static List<Shape> Shapes()
        {
            var shapes = new List<Shape> { Rect(Part.Ground, 0f, 0f, Unit, Unit) };
            var corners = new[] { new Vector2(0f, 0f), new Vector2(Unit, 0f), new Vector2(Unit, Unit), new Vector2(0f, Unit) };
            for (int q = 0; q < 4; q++) shapes.Add(QuarterDisc(corners[q], q));
            for (int q = 0; q < 4; q++)
            {
                var cubics = new List<Vector2>(UpPetal.Length);
                foreach (var p in UpPetal) cubics.Add(Turn(p, q));
                shapes.Add(new Shape(Part.Petal, Turn(new Vector2(12f, 3.2f), q), cubics));
            }
            shapes.Add(Circle(Part.Dot, Centre, DotRadius));
            return shapes;
        }

        // A point turned `quarters` quarter turns clockwise on the panel (y down) about the tile's centre
        public static Vector2 Turn(Vector2 p, int quarters)
        {
            var d = p - Centre;
            for (int i = 0; i < ((quarters % 4) + 4) % 4; i++) d = new Vector2(-d.y, d.x);
            return Centre + d;
        }

        public static float AlphaOf(Part part) => part switch
        {
            Part.Ground => GroundAlpha,
            Part.Corner => CornerAlpha,
            _ => PetalAlpha,
        };

        // How many tiles of `size` with `joint` between them cover `width` (one more is drawn by a row that slides: Drift)
        public static int CountFor(float width, float size, float joint) =>
            width <= 0f || size <= 0f ? 0 : Mathf.CeilToInt((width + joint) / (size + joint));

        // A tile's point in panel units: unit point `u` of the tile whose left edge is at `left`, its top at `top`, `size` wide, squeezed to
        // `scaleX` of its width about its vertical centre line (the Reveal's turn)
        public static Vector2 Place(Vector2 u, float left, float top, float size, float scaleX) =>
            new(left + size * 0.5f + (u.x - Unit * 0.5f) / Unit * size * scaleX, top + u.y / Unit * size);

        private static Shape Rect(Part part, float x, float y, float w, float h)
        {
            // - straight sides as cubics whose handles lie on them, so every part is one kind of path
            Vector2 a = new(x, y), b = new(x + w, y), c = new(x + w, y + h), d = new(x, y + h);
            return new Shape(part, a, new List<Vector2> { a, b, b, b, c, c, c, d, d, d, a, a });
        }

        // The quarter of a disc of radius CornerRadius centred on a tile corner (q: 0 top-left, 1 top-right, 2 bottom-right, 3 bottom-left),
        // inside the tile: the corner, along one side, round the arc, back along the other side
        private static Shape QuarterDisc(Vector2 corner, int q)
        {
            float r = CornerRadius, k = Kappa * r;
            // - the two directions INTO the tile from this corner (clockwise order)
            Vector2 u, v;
            switch (q)
            {
                case 0: u = Vector2.right; v = Vector2.up; break;
                case 1: u = Vector2.up; v = Vector2.left; break;
                case 2: u = Vector2.left; v = Vector2.down; break;
                default: u = Vector2.down; v = Vector2.right; break;
            }
            // - Vector2.up is +y, which is DOWN the panel: into the tile from the top edge
            Vector2 p1 = corner + u * r, p2 = corner + v * r;
            return new Shape(Part.Corner, corner, new List<Vector2>
            {
                corner, p1, p1,
                p1 + v * k, p2 + u * k, p2,
                p2, corner, corner,
            });
        }

        private static Shape Circle(Part part, Vector2 c, float r)
        {
            float k = Kappa * r;
            Vector2 top = c + new Vector2(0f, -r), right = c + new Vector2(r, 0f), foot = c + new Vector2(0f, r), left = c + new Vector2(-r, 0f);
            return new Shape(part, top, new List<Vector2>
            {
                top + new Vector2(k, 0f), right + new Vector2(0f, -k), right,
                right + new Vector2(0f, k), foot + new Vector2(k, 0f), foot,
                foot + new Vector2(-k, 0f), left + new Vector2(0f, k), left,
                left + new Vector2(0f, -k), top + new Vector2(-k, 0f), top,
            });
        }
    }

    // A Tile Row's options: the tile's colour and the paper its dot takes, the tile's size and the joint between tiles, how far the row has
    // slid (Drift: wraps every tile and joint), and -- for the Reveal -- each tile's squeeze and alpha (null = whole)
    public sealed class TileRowOptions
    {
        public Color Colour = Color.blue;
        public Color Paper = Color.white;
        public float Size = TileRule.Unit;
        public float Joint = 1f;
        public float Offset;
        public System.Func<int, float> ScaleOf;
        public System.Func<int, float> AlphaOf;

        public float Pitch => Size + Joint;
    }

    // A row of tiles drawn across its element with Painter2D, from the left edge (less the Drift's offset) to the right. A rounded or clipped
    // edge is the HOST's (put the row in an element with border radius and overflow hidden). element.AddManipulator(new TileRowPainter(o))
    public sealed class TileRowPainter : Manipulator
    {
        private static readonly List<TileRule.Shape> Shapes = TileRule.Shapes();
        private TileRowOptions _options;

        public TileRowPainter(TileRowOptions options) => _options = options ?? new TileRowOptions();

        public TileRowOptions Options
        {
            get => _options;
            set
            {
                _options = value ?? new TileRowOptions();
                Repaint();
            }
        }

        // Draw again (an option changed in place: the Reveal's or the Drift's frame)
        public void Repaint() => target?.MarkDirtyRepaint();

        protected override void RegisterCallbacksOnTarget()
        {
            target.generateVisualContent += Draw;
            target.MarkDirtyRepaint();
        }

        protected override void UnregisterCallbacksFromTarget() => target.generateVisualContent -= Draw;

        private void Draw(MeshGenerationContext mgc)
        {
            var o = _options;
            var box = target.contentRect;
            if (box.width <= 0f || o.Size <= 0f) return;
            float pitch = o.Pitch;
            // - the slide wraps by one tile and joint: the row looks the same every pitch
            float shift = pitch > 0f ? Mathf.Repeat(o.Offset, pitch) : 0f;
            float first = box.xMin - shift;
            int count = TileRule.CountFor(box.width + shift, o.Size, o.Joint);
            var painter = mgc.painter2D;
            for (int i = 0; i < count; i++)
            {
                float scale = o.ScaleOf?.Invoke(i) ?? 1f;
                float alpha = o.AlphaOf?.Invoke(i) ?? 1f;
                if (scale <= 0.001f || alpha <= 0.001f) continue;
                float left = first + i * pitch;
                foreach (var shape in Shapes)
                {
                    var colour = shape.Part == TileRule.Part.Dot ? o.Paper : o.Colour;
                    colour.a *= (shape.Part == TileRule.Part.Dot ? 1f : TileRule.AlphaOf(shape.Part)) * alpha;
                    painter.fillColor = colour;
                    painter.BeginPath();
                    painter.MoveTo(TileRule.Place(shape.Start, left, box.yMin, o.Size, scale));
                    for (int c = 0; c + 2 < shape.Cubics.Count; c += 3)
                        painter.BezierCurveTo(TileRule.Place(shape.Cubics[c], left, box.yMin, o.Size, scale),
                            TileRule.Place(shape.Cubics[c + 1], left, box.yMin, o.Size, scale),
                            TileRule.Place(shape.Cubics[c + 2], left, box.yMin, o.Size, scale));
                    painter.ClosePath();
                    painter.Fill();
                }
            }
        }
    }
}

using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories
{
    // The Locate Button's map pin (_3.3 P3d review b; the study's 48 x 64 drawing), a vector drawn into its own box with Painter2D: a
    // teardrop body in a gradient from its light top to its deep foot, a thin rim in a deeper shade of its colour (>= 4.5:1 on the paper),
    // a soft glow of its own colour round it and a soft black drop shadow under it (layered faint strokes: no blur in UI Toolkit), a white inner
    // disc lit from the top left and the deep dot at its centre; while it lies on a picture (OverPicture) a dark halo round its body gives it
    // an edge on any picture. Every colour is a THEME token read by name (LocateFabRule.Tokens), so a
    // mode switch or another Colour repaints it; nothing here is a literal colour
    public sealed class LocatePin : VisualElement
    {
        public const string ClassName = "poi-card__fab-pin";
        // The drawing's own units (the study's viewBox) and its rim's width in them
        private const float UnitsWide = 48f, UnitsHigh = 64f, RimUnits = 1f;

        // The tokens it is painted from now (hi, lo, rim), and the colours they resolved to on the last paint (what tests read)
        public (string Hi, string Lo, string Rim) Tokens { get; private set; } = LocateFabRule.Tokens(CardOptions.LocateTeal);
        public Color Hi { get; private set; }
        public Color Lo { get; private set; }
        public Color Rim { get; private set; }
        public Color Disc { get; private set; }
        public Color Drop { get; private set; }
        public Color Halo { get; private set; }
        // The drop shadow's fall (pt) and the glow's reach past the body (pt) and strength per layer
        public const float DropPoints = 3f;
        public const float GlowPoints = 8f;
        public const float GlowAlpha = 0.1f;
        // Over a picture: the dark halo's dense core hugging the body (pt, at --ts-locate-halo's strength) and its soft reach past it (pt)
        public const float HaloCorePoints = 3f;
        public const float HaloPoints = 7f;

        // Whether the pin lies on a picture now (the hero's): there its rim alone reads 1-2:1, so a dark halo gives it its edge (>= 3:1,
        // WCAG 1.4.11); on the paper the rim reads on its own and no halo is drawn
        public bool OverPicture
        {
            get => _overPicture;
            set
            {
                if (_overPicture == value) return;
                _overPicture = value;
                MarkDirtyRepaint();
            }
        }
        private bool _overPicture;

        public LocatePin()
        {
            AddToClassList(ClassName);
            pickingMode = PickingMode.Ignore;
            generateVisualContent += Draw;
            RegisterCallback<CustomStyleResolvedEvent>(_ => MarkDirtyRepaint());
        }

        // Paint it in `colour` (a CardOptions.Locate* key)
        public void Paint(string colour)
        {
            Tokens = LocateFabRule.Tokens(colour);
            MarkDirtyRepaint();
        }

        private Color Colour(string token)
        {
            CardTokenColours.TryResolve(this, token, out Color c);
            return c;
        }

        private void Draw(MeshGenerationContext context)
        {
            var box = contentRect;
            if (!(box.width > 0f) || !(box.height > 0f)) return;
            Hi = Colour(Tokens.Hi);
            Lo = Colour(Tokens.Lo);
            Rim = Colour(Tokens.Rim);
            Disc = Colour("--ts-locate-disc");
            Drop = Colour("--ts-locate-shadow");
            var discEdge = Colour("--ts-locate-disc-edge");
            // - the drawing scaled into the box whole (its own proportion kept), centred
            float scale = Mathf.Min(box.width / UnitsWide, box.height / UnitsHigh);
            var origin = new Vector2(box.x + (box.width - UnitsWide * scale) * 0.5f, box.y + (box.height - UnitsHigh * scale) * 0.5f);
            Vector2 P(float x, float y) => origin + new Vector2(x, y) * scale;

            var p = context.painter2D;
            p.lineJoin = LineJoin.Round;
            // - the body's path: the study's teardrop on its point at (24, 62), `down` points lower
            void Body(float down)
            {
                var d = new Vector2(0f, down);
                p.BeginPath();
                p.MoveTo(P(24f, 62f) + d);
                p.BezierCurveTo(P(24f, 62f) + d, P(4f, 38.5f) + d, P(4f, 23.5f) + d);
                p.BezierCurveTo(P(4f, 12.2f) + d, P(12.9f, 3f) + d, P(24f, 3f) + d);
                p.BezierCurveTo(P(35.1f, 3f) + d, P(44f, 12.2f) + d, P(44f, 23.5f) + d);
                p.BezierCurveTo(P(44f, 38.5f) + d, P(24f, 62f) + d, P(24f, 62f) + d);
                p.ClosePath();
            }
            // - softness without a blur (UI Toolkit has none): a few widening strokes, each faint -- they add up densest at the edge
            void Soft(Color colour, float alpha, float widest, int layers)
            {
                colour.a = alpha;
                for (int i = 0; i < layers; i++)
                {
                    p.strokeColor = colour;
                    p.lineWidth = widest * (layers - i) / layers;
                    p.Stroke();
                }
            }
            // - 1: its drop shadow, black, a little below it (the study's 0 6 8 at 0.18)
            Body(DropPoints);
            var dropFill = Drop;
            dropFill.a *= 0.3f;
            // - a translucent flat fill is fillColor: a gradient fill ignores alpha (drawn first, before any gradient on this painter)
            p.fillColor = dropFill;
            p.Fill();
            Soft(Drop, Drop.a * 0.22f, DropPoints * 2.5f, 3);
            // - 2: a glow in its own deep colour round the body (the study's 0 0 10 at 0.28): it reads as a lit object, never a cut-out
            Body(0f);
            Soft(Lo, GlowAlpha, GlowPoints * 2f, 4);
            // - 2b: over a picture, the dark halo after the glow (so the glow never lightens it): a dense core round the body, then a short
            //   soft fall -- a stroke reaches half its width past the path
            Halo = Colour("--ts-locate-halo");
            if (OverPicture)
            {
                Soft(Halo, Halo.a * 0.15f, HaloPoints * 2f, 4);
                p.strokeColor = Halo;
                p.lineWidth = HaloCorePoints * 2f;
                p.Stroke();
            }
            // - 3: the body, lit from its top left, and its rim: a deeper shade of its own colour
            p.fillGradient = FillGradient.MakeLinearGradient(Hi, Lo, P(0f, 0f), P(UnitsWide * 0.35f, UnitsHigh));
            p.Fill();
            p.strokeColor = Rim;
            p.lineWidth = Mathf.Max(1.25f, RimUnits * scale * 1.5f);
            p.Stroke();
            // - the inner disc, lit from its top left, and the dot
            p.BeginPath();
            p.Arc(P(24f, 23f), 11.5f * scale, 0f, 360f);
            p.ClosePath();
            p.fillGradient = FillGradient.MakeRadialGradient(Disc, discEdge, P(24f, 23f), 11.5f * 0.7f * 2f * scale, P(24f - 1.15f, 23f - 3.45f));
            p.Fill();
            p.BeginPath();
            p.Arc(P(24f, 23f), 3.6f * scale, 0f, 360f);
            p.ClosePath();
            p.fillGradient = Flat(Lo, P(20f, 23f), P(28f, 23f));
            p.Fill();
        }

        // A flat fill as a "gradient" of one colour: once a painter fills with a gradient it keeps doing so
        private static FillGradient Flat(Color colour, Vector2 from, Vector2 to) => FillGradient.MakeLinearGradient(colour, colour, from, to);
    }
}

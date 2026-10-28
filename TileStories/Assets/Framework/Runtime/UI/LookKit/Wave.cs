using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories.LookKit
{
    // The Wave, pure: v6's one path ("M0 30V17C48 4 98 2 150 12S262 30 318 20S372 6 390 10V30Z" in a 390 x 30 box) -- a soft swell filled to
    // the box's foot. The S segments' first handles are the previous handles reflected through their points, worked out once here. The path
    // stretches to any width; its height grows from the foot (`rise`: 0 = flat on the foot, 1 = whole -- the Reveal's)
    public static class WaveRule
    {
        public const float BoxWidth = 390f;
        public const float BoxHeight = 30f;

        // The swell's points in v6's box: the start, then three cubics (control, control, end)
        public static readonly Vector2 Start = new(0f, 17f);
        public static readonly IReadOnlyList<Vector2> Cubics = new[]
        {
            new Vector2(48f, 4f), new Vector2(98f, 2f), new Vector2(150f, 12f),
            new Vector2(202f, 22f), new Vector2(262f, 30f), new Vector2(318f, 20f),
            new Vector2(374f, 10f), new Vector2(372f, 6f), new Vector2(390f, 10f),
        };

        // A point of v6's box placed in a box `width` x `height`, risen `rise` of its height from the foot
        public static Vector2 Place(Vector2 p, float width, float height, float rise) =>
            new(p.x / BoxWidth * width, height - (BoxHeight - p.y) / BoxHeight * height * Mathf.Clamp01(rise));
    }

    // A Wave's options: its colour (the alpha is the wave's), how far it has risen
    public sealed class WaveOptions
    {
        public Color Colour = new(0f, 0f, 0f, 0.12f);
        public float Rise = 1f;
    }

    // The Wave drawn across its element with Painter2D: the swell, down the right side, along the foot, up the left side
    public sealed class WavePainter : Manipulator
    {
        private WaveOptions _options;

        public WavePainter(WaveOptions options) => _options = options ?? new WaveOptions();

        public WaveOptions Options
        {
            get => _options;
            set
            {
                _options = value ?? new WaveOptions();
                Repaint();
            }
        }

        public void Repaint() => target?.MarkDirtyRepaint();

        protected override void RegisterCallbacksOnTarget()
        {
            target.generateVisualContent += Draw;
            target.MarkDirtyRepaint();
        }

        protected override void UnregisterCallbacksFromTarget() => target.generateVisualContent -= Draw;

        private void Draw(MeshGenerationContext mgc)
        {
            var box = target.contentRect;
            if (box.width <= 0f || box.height <= 0f || _options.Colour.a <= 0f || _options.Rise <= 0f) return;
            Vector2 At(Vector2 p) => box.position + WaveRule.Place(p, box.width, box.height, _options.Rise);
            var painter = mgc.painter2D;
            painter.fillColor = _options.Colour;
            painter.BeginPath();
            painter.MoveTo(new Vector2(box.xMin, box.yMax));
            painter.LineTo(At(WaveRule.Start));
            var c = WaveRule.Cubics;
            for (int i = 0; i + 2 < c.Count; i += 3) painter.BezierCurveTo(At(c[i]), At(c[i + 1]), At(c[i + 2]));
            painter.LineTo(new Vector2(box.xMax, box.yMax));
            painter.ClosePath();
            painter.Fill();
        }
    }
}

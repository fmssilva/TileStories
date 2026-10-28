using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TileStories.LookKit
{
    // A Strip's options: a tileable texture repeated sideways at `Height` (its width kept in proportion unless `TileWidth` is set), slid
    // `Offset` to the left (Drift), at `Alpha`
    public sealed class StripOptions
    {
        public Texture2D Texture;
        public float Height = 24f;
        public float TileWidth;
        public float Offset;
        public float Alpha = 1f;

        // One repeat's width: the set one, else the texture's proportion at Height
        public float RepeatWidth => TileWidth > 0f ? TileWidth
            : Texture != null && Texture.height > 0 ? Height * Texture.width / Texture.height : Height;
    }

    // The Strip: the element's background is the texture, repeated along x at the strip's height, its position the slide (wrapping every
    // repeat) -- the panel draws it, nothing per frame but a style. Apply again after any option changed
    public static class Strip
    {
        public static void Apply(VisualElement element, StripOptions options)
        {
            if (element == null) return;
            if (options?.Texture == null)
            {
                element.style.backgroundImage = StyleKeyword.Null;
                return;
            }
            float repeat = options.RepeatWidth;
            element.style.height = options.Height;
            element.style.backgroundImage = new StyleBackground(options.Texture);
            element.style.backgroundRepeat = new BackgroundRepeat(Repeat.Repeat, Repeat.NoRepeat);
            element.style.backgroundSize = new BackgroundSize(new Length(repeat), new Length(options.Height));
            element.style.backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Left, -Mathf.Repeat(options.Offset, repeat));
            element.style.opacity = Mathf.Clamp01(options.Alpha);
        }
    }

    // The Mosaic, pure (v6's divider): tesserae of 6 units on a 7-unit pitch, 4 rows high, a band weaving along a sine of 14 columns -- the
    // cells on the band nearly solid, beside it half, the rest faint. One period (14 columns, 98 x 28 units) is drawn into a texture the
    // Strip repeats, so it tiles without a seam
    public static class MosaicRule
    {
        public const int Rows = 4;
        public const int Period = 14;
        public const float Pitch = 7f;
        public const float Cell = 6f;
        public const float CellRadius = 1.5f;
        public const float Width = Period * Pitch;
        public const float Height = Rows * Pitch;

        // A cell's alpha (of the strip's one colour): on the band 0.92, near it 0.42, else 0.14
        public static float AlphaOf(int column, int row)
        {
            float middle = 1.5f + 1.45f * Mathf.Sin(2f * Mathf.PI * column / Period);
            float d = Mathf.Abs(row - middle);
            return d < 0.8f ? 0.92f : d < 1.7f ? 0.42f : 0.14f;
        }

        // How much of a pixel at (x, y) in units a rounded cell covers (an anti-aliased edge one pixel wide at `unitsPerPixel`)
        public static float Coverage(float x, float y, float unitsPerPixel)
        {
            float cx = Mathf.Repeat(x, Pitch), cy = Mathf.Repeat(y, Pitch);
            float half = Cell * 0.5f, r = CellRadius;
            float qx = Mathf.Abs(cx - half) - (half - r), qy = Mathf.Abs(cy - half) - (half - r);
            float outside = new Vector2(Mathf.Max(qx, 0f), Mathf.Max(qy, 0f)).magnitude + Mathf.Min(Mathf.Max(qx, qy), 0f) - r;
            return Mathf.Clamp01(0.5f - outside / Mathf.Max(1e-4f, unitsPerPixel));
        }

        // One period as pixels (row 0 at the TOP), white, the alpha each cell's: `pixelsPerUnit` sets the texture's sharpness
        public static Color32[] Pixels(int pixelsPerUnit, out int width, out int height)
        {
            width = Mathf.RoundToInt(Width * pixelsPerUnit);
            height = Mathf.RoundToInt(Height * pixelsPerUnit);
            var pixels = new Color32[width * height];
            float unit = 1f / pixelsPerUnit;
            for (int py = 0; py < height; py++)
                for (int px = 0; px < width; px++)
                {
                    float x = (px + 0.5f) * unit, y = (py + 0.5f) * unit;
                    int column = Mathf.FloorToInt(x / Pitch), row = Mathf.FloorToInt(y / Pitch);
                    float a = Coverage(x, y, unit) * AlphaOf(column, row);
                    // - a texture's first row is its BOTTOM: the top row of cells goes last
                    pixels[(height - 1 - py) * width + px] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(a * 255f));
                }
            return pixels;
        }

        // The period as a texture, made once per sharpness and kept (a tint colours it: background-image-tint-color)
        public static Texture2D Texture(int pixelsPerUnit = 4)
        {
            if (Cache.TryGetValue(pixelsPerUnit, out var made) && made != null) return made;
            var pixels = Pixels(pixelsPerUnit, out int w, out int h);
            var texture = new Texture2D(w, h, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Repeat, filterMode = FilterMode.Bilinear, name = "LookKit Mosaic" };
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            Cache[pixelsPerUnit] = texture;
            return texture;
        }

        private static readonly Dictionary<int, Texture2D> Cache = new();
    }
}

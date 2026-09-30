using NUnit.Framework;
using UnityEngine;

namespace TileStories.Tests
{
    // Reads what a preview's RenderTexture really drew (the stage clears to transparent black, so "drawn" means alpha > 0):
    // the pixel tests of model_3d / panorama_360 judge the model's own pixels, never the state that says where it looks
    public static class PreviewPixels
    {
        // The box (in texture pixels, y up) around every pixel the stage drew; width 0 when nothing was drawn
        public static RectInt DrawnBounds(RenderTexture texture)
        {
            var pixels = Read(texture);
            int w = texture.width, h = texture.height;
            int minX = w, maxX = -1, minY = h, maxY = -1;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (pixels[y * w + x].a <= 0) continue;
                    if (x < minX) minX = x;
                    if (x > maxX) maxX = x;
                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;
                }
            }
            return maxX < minX ? new RectInt(0, 0, 0, 0) : new RectInt(minX, minY, maxX - minX + 1, maxY - minY + 1);
        }

        // The share of the shorter side every edge of a preview keeps clear of the model (a sphere fit spans 80 %, so the untouched
        // margin is 10 % per edge; 5 % leaves room for the rasteriser's edge pixel only)
        public const float EdgeMarginOfShorterSide = 0.05f;

        // Every pixel the stage drew stays EdgeMarginOfShorterSide away from all four edges; `what` names the case
        public static RectInt AssertDrawnInsideWithMargin(RenderTexture texture, string what)
        {
            var drawn = DrawnBounds(texture);
            int w = texture.width, h = texture.height;
            int margin = Mathf.CeilToInt(Mathf.Min(w, h) * EdgeMarginOfShorterSide);
            string message = what + ": drawn " + drawn + " in " + w + "x" + h + " (margin " + margin + "px)";
            Assert.Greater(drawn.width, 0, message + " -- nothing rendered");
            Assert.GreaterOrEqual(drawn.xMin, margin, message + " -- touches the left edge");
            Assert.GreaterOrEqual(drawn.yMin, margin, message + " -- touches the bottom edge");
            Assert.LessOrEqual(drawn.xMax, w - margin, message + " -- touches the right edge");
            Assert.LessOrEqual(drawn.yMax, h - margin, message + " -- touches the top edge");
            return drawn;
        }

        // The colour of one pixel (texture pixels, y up)
        public static Color32 At(RenderTexture texture, int x, int y) => Read(texture)[y * texture.width + x];

        // The average colour of every pixel, alpha included
        public static Color Average(RenderTexture texture)
        {
            long r = 0, g = 0, b = 0, a = 0;
            var pixels = Read(texture);
            foreach (var p in pixels) { r += p.r; g += p.g; b += p.b; a += p.a; }
            float n = 255f * pixels.Length;
            return new Color(r / n, g / n, b / n, a / n);
        }

        private static Color32[] Read(RenderTexture texture)
        {
            var previous = RenderTexture.active;
            RenderTexture.active = texture;
            var copy = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
            copy.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
            copy.Apply();
            RenderTexture.active = previous;
            var pixels = copy.GetPixels32();
            Object.Destroy(copy);
            return pixels;
        }
    }
}

using UnityEngine;

namespace TileStories
{
    // A picture's brightness, read once (_3.3 P3c review: the Local Shade is only as dark as the picture under the words needs): the
    // picture drawn into a small linear RenderTexture and read back -- a card's pictures are not CPU-readable -- into a grid of relative
    // luminances (WCAG, linear light), row 0 the picture's TOP. MaxIn answers "how bright is the brightest spot here" for a part of it.
    // Plain C#; a few ms once per picture and size, never per frame
    public sealed class PictureLuminance
    {
        public Texture Source { get; }
        public int Width { get; }
        public int Height { get; }
        private readonly float[] _y;

        private PictureLuminance(Texture source, int width, int height, float[] y)
        {
            Source = source;
            Width = width;
            Height = height;
            _y = y;
        }

        // `texture` read at `width` x `height` cells (each at least 1; null for no texture)
        public static PictureLuminance Read(Texture texture, int width, int height)
        {
            if (texture == null) return null;
            width = Mathf.Clamp(width, 1, 4096);
            height = Mathf.Clamp(height, 1, 4096);
            var rt = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            var active = RenderTexture.active;
            var read = new Texture2D(width, height, TextureFormat.RGBA32, false, true);
            try
            {
                Graphics.Blit(texture, rt);
                RenderTexture.active = rt;
                read.ReadPixels(new Rect(0, 0, width, height), 0, 0, false);
            }
            finally
            {
                RenderTexture.active = active;
                RenderTexture.ReleaseTemporary(rt);
            }
            var pixels = read.GetPixels32();
            if (Application.isPlaying) Object.Destroy(read);
            else Object.DestroyImmediate(read);
            // - in a Linear project the blit stored linear values; a Gamma project's are gamma: brought to linear here
            bool gamma = QualitySettings.activeColorSpace == ColorSpace.Gamma;
            var y = new float[width * height];
            for (int row = 0; row < height; row++)
                for (int x = 0; x < width; x++)
                {
                    // - a read-back texture's row 0 is the picture's foot
                    Color c = pixels[(height - 1 - row) * width + x];
                    if (gamma) c = c.linear;
                    y[row * width + x] = 0.2126f * c.r + 0.7152f * c.g + 0.0722f * c.b;
                }
            return new PictureLuminance(texture, width, height, y);
        }

        // The brightest luminance inside `uv` (0..1, x right, y DOWN from the picture's top; clamped to the picture); NaN when it misses it
        public float MaxIn(Rect uv)
        {
            int x0 = Mathf.Clamp(Mathf.FloorToInt(uv.xMin * Width), 0, Width), x1 = Mathf.Clamp(Mathf.CeilToInt(uv.xMax * Width), 0, Width);
            int y0 = Mathf.Clamp(Mathf.FloorToInt(uv.yMin * Height), 0, Height), y1 = Mathf.Clamp(Mathf.CeilToInt(uv.yMax * Height), 0, Height);
            if (x1 <= x0 || y1 <= y0) return float.NaN;
            float most = 0f;
            for (int row = y0; row < y1; row++)
                for (int x = x0; x < x1; x++)
                    most = Mathf.Max(most, _y[row * Width + x]);
            return most;
        }
    }
}

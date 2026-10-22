using UnityEngine;

namespace TileStories.Dev.StyleSpike
{
    // DEV-ONLY (_3.3 P0 S7): a soft rounded-rect shadow texture for 9-slice use -- black with an alpha falloff, the corner + blur
    // region inside the slices so any block size stretches only the flat middle.
    public static class SpikeShadowSprite
    {
        // Paint a (2 * Slice + 2) square shadow: a rounded box of `radius` blurred over `softness`, inset by `pad`
        public static Texture2D Make(int pad, int radius, float softness, float opacity, out int slice)
        {
            slice = pad + radius;
            int size = 2 * slice + 2;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { name = "StyleSpike Shadow", wrapMode = TextureWrapMode.Clamp };
            float half = size * 0.5f;
            float inner = half - pad;                                   // the box's half size
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    var q = new Vector2(Mathf.Abs(x + 0.5f - half), Mathf.Abs(y + 0.5f - half)) - new Vector2(inner - radius, inner - radius);
                    float d = Vector2.Max(q, Vector2.zero).magnitude + Mathf.Min(Mathf.Max(q.x, q.y), 0f) - radius;
                    float a = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(-softness * 0.5f, softness, d));
                    tex.SetPixel(x, y, new Color(0f, 0f, 0f, a * opacity));
                }
            tex.Apply();
            return tex;
        }
    }
}

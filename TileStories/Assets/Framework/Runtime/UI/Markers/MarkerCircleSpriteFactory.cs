using System.Collections.Generic;
using UnityEngine;

namespace TileStories
{
    // Single shared source of runtime-generated circle/ring sprites for every
    // marker accent effect (MarkerSunEffect, MarkerAccentEffect). Centralising
    // this means the domain-reload-safety fix from section 18.12 only has to
    // exist in one place -- do not reimplement sprite generation or its caching
    // in any individual effect file; call in here instead.
    public static class MarkerCircleSpriteFactory
    {
        private const int TextureSize = 256;

        private static readonly Dictionary<(int, int), Sprite> s_ringCache = new();
        private static readonly Dictionary<int, Sprite> s_filledCache = new();

        // section 18.12: explicit null checks (not ??=) -- UnityEngine.Object's
        // overloaded == correctly reports a destroyed sprite as null; ??='s raw
        // CLR check does not, which is exactly how the original square-render
        // bug happened.
        public static Sprite GetRing(float outerScale, float innerScale)
        {
            var key = (Quantize(outerScale), Quantize(innerScale));
            if (s_ringCache.TryGetValue(key, out var sprite) && sprite != null)
                return sprite;

            sprite = BuildRingSprite(TextureSize, outerScale, innerScale);
            s_ringCache[key] = sprite;
            return sprite;
        }

        public static Sprite GetFilled(float radiusScale)
        {
            var key = Quantize(radiusScale);
            if (s_filledCache.TryGetValue(key, out var sprite) && sprite != null)
                return sprite;

            sprite = BuildFilledCircleSprite(TextureSize, radiusScale);
            s_filledCache[key] = sprite;
            return sprite;
        }

        // Forces the cache empty at the start of every logical run -- fires on
        // every Play Mode entry and process start, whether or not domain reload
        // ran. This is the actual fix for section 18.12's stale-static-cache bug;
        // the explicit null checks above are defence in depth for the same class
        // of problem within a single session.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetCaches()
        {
            s_ringCache.Clear();
            s_filledCache.Clear();
        }

        private static int Quantize(float scale) => Mathf.RoundToInt(scale * 1000f);

        private static Sprite BuildRingSprite(int size, float outerRadiusScale, float innerRadiusScale)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false, true);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;

            float center = (size - 1) * 0.5f;
            float outer = center * outerRadiusScale;
            float inner = center * innerRadiusScale;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    float alpha = d <= outer && d >= inner ? 1f : 0f;
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            texture.Apply(false, false);
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        private static Sprite BuildFilledCircleSprite(int size, float radiusScale)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false, true);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;

            float center = (size - 1) * 0.5f;
            float radius = center * radiusScale;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    float alpha = d <= radius ? 1f : 0f;
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            texture.Apply(false, false);
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }
    }
}

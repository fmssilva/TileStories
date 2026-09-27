using System.Collections.Generic;
using UnityEngine;

namespace TileStories
{
    // The Phase A card gallery's media (_3.1 step 7): pictures made in memory, one per name in CardGalleryDefinitions'
    // Pictures list -- no wall folder, no files (Phase A has no config and the Framework never names a wall). Each picture
    // is its own flat colour with a lighter grid every GridStep px, so a render pixel tells which picture a frame shows and
    // a moving grid shows a picture moving. Counted like the real source (ResourcesMediaSource): loads per name, the last
    // release destroys the texture -- the gallery tests check that a block gives back everything it took.
    public sealed class CardGalleryMedia : IMediaSource
    {
        public const int GridStep = 32;

        private readonly Dictionary<string, (Texture2D Texture, int Count)> _held = new();

        // How many different pictures are held now, and how many loads of one name are not given back
        public int HeldCount => _held.Count;
        public int RefCount(string path) => _held.TryGetValue(path ?? "", out var h) ? h.Count : 0;
        public int TotalLoads { get; private set; }

        public T Load<T>(string path) where T : Object
        {
            string key = path?.Trim() ?? "";
            if (!CardGalleryDefinitions.Pictures.TryGetValue(key, out var picture)) return null;
            TotalLoads++;
            if (_held.TryGetValue(key, out var held))
            {
                _held[key] = (held.Texture, held.Count + 1);
                return held.Texture as T;
            }
            var texture = Make(picture.Size, picture.Colour);
            texture.name = key;
            _held[key] = (texture, 1);
            return texture as T;
        }

        public void Release(string path)
        {
            string key = path?.Trim() ?? "";
            if (!_held.TryGetValue(key, out var held)) return;
            if (held.Count > 1)
            {
                _held[key] = (held.Texture, held.Count - 1);
                return;
            }
            _held.Remove(key);
            Object.Destroy(held.Texture);
        }

        private static Texture2D Make(Vector2Int size, Color colour)
        {
            var texture = new Texture2D(size.x, size.y, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            var line = Color.Lerp(colour, Color.white, 0.5f);
            var pixels = new Color32[size.x * size.y];
            for (int y = 0; y < size.y; y++)
                for (int x = 0; x < size.x; x++)
                    pixels[y * size.x + x] = x % GridStep == 3 || y % GridStep == 3 ? line : colour;
            texture.SetPixels32(pixels);
            texture.Apply();
            return texture;
        }
    }
}

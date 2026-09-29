using System.Collections.Generic;
using UnityEngine;

namespace TileStories
{
    // The Phase A card gallery's media (_3.1 steps 7 and 9A): pictures, silent audio clips and captions made in memory, one per name in
    // CardGalleryDefinitions' Pictures / Clips / Captions lists -- no wall folder, no files (Phase A has no config and the Framework
    // never names a wall). Each picture is its own flat colour with a lighter grid every GridStep px, so a render pixel tells which
    // picture a frame shows and a moving grid shows a picture moving; a clip is silence of the length its name is given, a captions file
    // is the WebVTT text its name is given. Counted like the real source (ResourcesMediaSource): loads per name, the last release destroys
    // the asset -- the gallery tests check that a block gives back everything it took.
    public sealed class CardGalleryMedia : IMediaSource
    {
        public const int GridStep = 32;
        // A gallery clip's sample rate: the lowest that keeps a ten-minute clip small (nothing is heard)
        private const int ClipRate = 2000;

        private readonly Dictionary<string, (Object Asset, int Count)> _held = new();

        // How many different assets are held now, and how many loads of one name are not given back
        public int HeldCount => _held.Count;
        public int RefCount(string path) => _held.TryGetValue(path ?? "", out var h) ? h.Count : 0;
        public int TotalLoads { get; private set; }

        public T Load<T>(string path) where T : Object
        {
            string key = path?.Trim() ?? "";
            if (_held.TryGetValue(key, out var held) && held.Asset is T cached)
            {
                TotalLoads++;
                _held[key] = (cached, held.Count + 1);
                return cached;
            }
            var asset = Make(key, typeof(T));
            if (asset == null) return null;
            TotalLoads++;
            asset.name = key;
            _held[key] = (asset, 1);
            return asset as T;
        }

        public void Release(string path)
        {
            string key = path?.Trim() ?? "";
            if (!_held.TryGetValue(key, out var held)) return;
            if (held.Count > 1)
            {
                _held[key] = (held.Asset, held.Count - 1);
                return;
            }
            _held.Remove(key);
            Object.Destroy(held.Asset);
        }

        // The asset the gallery has under `key` for the type asked, or null (a missing file)
        private static Object Make(string key, System.Type type)
        {
            // - the type asked for is the asset's type or one of its bases (a view may ask for a Texture as well as a Texture2D)
            if (type.IsAssignableFrom(typeof(Texture2D)) && CardGalleryDefinitions.Pictures.TryGetValue(key, out var picture)) return MakePicture(picture.Size, picture.Colour);
            if (type.IsAssignableFrom(typeof(AudioClip)) && CardGalleryDefinitions.Clips.TryGetValue(key, out float seconds))
                return AudioClip.Create(key, Mathf.CeilToInt(seconds * ClipRate), 1, ClipRate, false);
            if (type.IsAssignableFrom(typeof(TextAsset)) && CardGalleryDefinitions.Captions.TryGetValue(key, out string vtt)) return new TextAsset(vtt);
            return null;
        }

        private static Texture2D MakePicture(Vector2Int size, Color colour)
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

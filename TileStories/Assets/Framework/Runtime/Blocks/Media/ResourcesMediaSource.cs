using System.Collections.Generic;
using UnityEngine;

namespace TileStories
{
    // IMediaSource over Resources.Load, the same pattern the marker icon and font libraries use: card media lives
    // in a folder inside a Resources folder of the wall (card_settings.media_resources_path). Loads are counted per
    // path; the last Release unloads the asset (Resources.UnloadAsset), so a closed card holds nothing.
    public sealed class ResourcesMediaSource : IMediaSource
    {
        private readonly string _root;
        private readonly Dictionary<string, (Object Asset, int Count)> _held = new();
        private readonly HashSet<string> _reportedMissing = new();

        // `resourcesRoot`: the Resources-relative folder ("" = the Resources root itself)
        public ResourcesMediaSource(string resourcesRoot) => _root = (resourcesRoot ?? "").Trim().Trim('/');

        public string Root => _root;

        // How many different assets are held right now
        public int LoadedCount => _held.Count;

        // How many Loads of this path are not released yet
        public int RefCount(string path) => _held.TryGetValue(Key(path), out var h) ? h.Count : 0;

        public T Load<T>(string path) where T : Object
        {
            string key = Key(path);
            if (key.Length == 0) return null;
            if (_held.TryGetValue(key, out var held) && held.Asset is T cached)
            {
                _held[key] = (cached, held.Count + 1);
                return cached;
            }

            var asset = Resources.Load<T>(ResourcesPath(key));
            if (asset == null)
            {
                // - one line per missing file, not one per bind: the block shows its "media unavailable" state
                if (_reportedMissing.Add(key)) Debug.LogWarning("[Card] media not found: Resources/" + ResourcesPath(key));
                return null;
            }
            _held[key] = (asset, 1);
            return asset;
        }

        public void Release(string path)
        {
            string key = Key(path);
            if (!_held.TryGetValue(key, out var held)) return;
            if (held.Count > 1)
            {
                _held[key] = (held.Asset, held.Count - 1);
                return;
            }
            _held.Remove(key);
            // - a GameObject (a prefab) cannot be unloaded one by one; everything a card shows today can
            if (!(held.Asset is GameObject)) Resources.UnloadAsset(held.Asset);
        }

        // "castle/hero.png" -> "castle/hero": Resources paths have no extension
        private static string Key(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return "";
            string p = path.Trim().Replace('\\', '/').Trim('/');
            int dot = p.LastIndexOf('.');
            int slash = p.LastIndexOf('/');
            return dot > slash ? p.Substring(0, dot) : p;
        }

        private string ResourcesPath(string key) => _root.Length == 0 ? key : _root + "/" + key;
    }
}

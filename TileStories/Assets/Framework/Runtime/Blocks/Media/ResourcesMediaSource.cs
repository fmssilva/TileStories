using System.Collections.Generic;
using UnityEngine;

namespace TileStories
{
    // IMediaSource over Resources.Load, the same pattern the marker icon and font libraries use: card media lives
    // in a folder inside a Resources folder of the wall (card_settings.media_resources_path). Loads are counted per
    // asset; the last Release unloads it (Resources.UnloadAsset), so a closed card holds nothing.
    // An asset is one (path without its extension, asset type) pair: "guide.mp3" and "guide.vtt" share a Resources path but are
    // two assets (an AudioClip and a TextAsset), so each is counted and released on its own (_3.1 step 9A).
    public sealed class ResourcesMediaSource : IMediaSource
    {
        private readonly string _root;
        private readonly Dictionary<(string Key, System.Type Type), (Object Asset, int Count)> _held = new();
        private readonly HashSet<string> _reportedMissing = new();

        // `resourcesRoot`: the Resources-relative folder ("" = the Resources root itself)
        public ResourcesMediaSource(string resourcesRoot) => _root = (resourcesRoot ?? "").Trim().Trim('/');

        public string Root => _root;

        // A "default:<key>" value resolves through these instead of Resources.Load (_3.1 step 13): the wall's own
        // override first, then the Framework's shipped library. Both null = every default key is unresolved (the
        // block's "media unavailable" state, same as a missing file). Never reference-counted: a library asset is
        // a static Framework/wall reference, never loaded or unloaded by this source.
        public CardMediaLibrary WallDefaults { get; set; }
        public CardMediaLibrary FrameworkDefaults { get; set; }

        // How many different assets are held right now
        public int LoadedCount => _held.Count;

        // How many Loads of this path are not released yet (the asset of the kind its extension names; else the first with that path)
        public int RefCount(string path) => Find(path, out var slot) ? _held[slot].Count : 0;

        public T Load<T>(string path) where T : Object
        {
            if (MediaPathRule.IsDefaultKey(path))
            {
                string defaultKey = MediaPathRule.DefaultKeyOf(path);
                // - a texture may be a picture or a panorama entry: the first kind that holds the key wins
                T found = null;
                foreach (var kind in MediaPathRule.KindsOfAssetType(typeof(T)))
                    if ((found = CardMediaLibraryLookup.Resolve(defaultKey, kind, WallDefaults, FrameworkDefaults) as T) != null) break;
                if (found == null && _reportedMissing.Add("default:" + defaultKey + "|" + typeof(T).Name))
                    Debug.LogWarning("[Card] default media key not found: " + defaultKey);
                return found;
            }

            string key = Key(path);
            if (key.Length == 0) return null;
            var slot = (key, typeof(T));
            if (_held.TryGetValue(slot, out var held) && held.Asset is T cached)
            {
                _held[slot] = (cached, held.Count + 1);
                return cached;
            }

            var asset = Resources.Load<T>(ResourcesPath(key));
            if (asset == null)
            {
                // - one line per missing file, not one per bind: the block shows its "media unavailable" state
                if (_reportedMissing.Add(key + "|" + typeof(T).Name)) Debug.LogWarning("[Card] media not found: Resources/" + ResourcesPath(key));
                return null;
            }
            _held[slot] = (asset, 1);
            return asset;
        }

        public void Release(string path)
        {
            // - a default-library asset is never counted or unloaded here (it is a static Framework/wall reference)
            if (MediaPathRule.IsDefaultKey(path)) return;
            if (!Find(path, out var slot)) return;
            var held = _held[slot];
            if (held.Count > 1)
            {
                _held[slot] = (held.Asset, held.Count - 1);
                return;
            }
            _held.Remove(slot);
            // - a GameObject (a prefab) cannot be unloaded one by one; everything a card shows today can
            if (!(held.Asset is GameObject)) Resources.UnloadAsset(held.Asset);
        }

        // The held asset `path` names: the one whose type fits the path's extension (.mp3 = an AudioClip, .vtt = a TextAsset, a
        // picture = a texture); a path with no extension names the first asset held under it
        private bool Find(string path, out (string Key, System.Type Type) slot)
        {
            string key = Key(path);
            var kind = MediaPathRule.KindOfExtension(path);
            foreach (var pair in _held)
                if (pair.Key.Key == key && MediaPathRule.IsAssetOfKind(pair.Value.Asset, kind))
                {
                    slot = pair.Key;
                    return true;
                }
            slot = default;
            return false;
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

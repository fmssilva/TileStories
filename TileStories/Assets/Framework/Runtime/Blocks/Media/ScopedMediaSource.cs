using System.Collections.Generic;

namespace TileStories
{
    // One bound block view's window onto the card's media source: it remembers every Load the view made, and
    // ReleaseAll gives each one back. BlockStackView hands every view its own scope and calls ReleaseAll when it
    // unbinds the view, so a block never has to remember to release (and cannot leak) what it loaded.
    public sealed class ScopedMediaSource : IMediaSource
    {
        private readonly IMediaSource _source;
        private readonly List<string> _loaded = new();

        public ScopedMediaSource(IMediaSource source) => _source = source;

        // How many Loads this scope still holds
        public int HeldCount => _loaded.Count;

        public T Load<T>(string path) where T : UnityEngine.Object
        {
            var asset = _source?.Load<T>(path);
            if (asset != null) _loaded.Add(path);
            return asset;
        }

        // Give back ONE of this scope's Loads of `path` (a view that swaps an image early)
        public void Release(string path)
        {
            if (_loaded.Remove(path)) _source?.Release(path);
        }

        // Give back everything this scope loaded
        public void ReleaseAll()
        {
            foreach (string path in _loaded) _source?.Release(path);
            _loaded.Clear();
        }
    }
}

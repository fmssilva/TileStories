using System;
using System.Collections.Generic;
using UnityEngine;

namespace TileStories
{
    // The card's ONE preview owner (_3.1 step 10A.2): plain C#, no MonoBehaviour, the same "one owner beside the card"
    // discipline as CardAudioService/CardVideoService, but shaped as a SLOT manager since several previews (a model_3d
    // here, a panorama_360 there) can be bound in the stack at once, unlike audio/video where only one ever plays. A
    // block asks for its own slot by its block key when it binds, gets it back unchanged on a later bind with the same
    // kind/path (a rebind never reloads what it already has), and releases it on unbind; the card closing releases
    // whatever no block gave back on its own.
    public sealed class CardPreviewService : ICardPreview
    {
        private sealed class Slot : ICardPreviewSlot
        {
            public string Key { get; }
            public MediaKind Kind { get; }
            public string Path { get; }
            public bool IsLoading { get; set; } = true;
            public bool Failed { get; set; }
            public RenderTexture Texture => Handle?.Texture;
            public IPreviewHandle Handle;

            public void RenderNow(TurntableState turntable, PanoramaViewState panorama) => Handle?.RenderNow(turntable, panorama);

            public Slot(string key, MediaKind kind, string path)
            {
                Key = key;
                Kind = kind;
                Path = path;
            }
        }

        private readonly IPreviewStage _stage;
        private readonly Func<IMediaSource> _media;
        private readonly Dictionary<string, Slot> _slots = new();

        public CardPreviewService(IPreviewStage stage, Func<IMediaSource> media)
        {
            _stage = stage;
            _media = media;
        }

        public event Action Changed;

        public ICardPreviewSlot Request(string key, MediaKind kind, string path)
        {
            if (string.IsNullOrWhiteSpace(key)) return null;
            if (_slots.TryGetValue(key, out var existing))
            {
                if (existing.Kind == kind && existing.Path == path) return existing;
                ReleaseSlot(existing);
                _slots.Remove(key);
            }
            var slot = new Slot(key, kind, path);
            _slots[key] = slot;
            if (string.IsNullOrWhiteSpace(path))
            {
                slot.IsLoading = false;
                slot.Failed = true;
                return slot;
            }
            var media = _media?.Invoke();
            slot.Handle = _stage.Load(kind, media, path,
                onReady: () => { slot.IsLoading = false; Changed?.Invoke(); },
                onFailed: () => { slot.IsLoading = false; slot.Failed = true; Changed?.Invoke(); });
            return slot;
        }

        public void Release(string key)
        {
            if (string.IsNullOrWhiteSpace(key) || !_slots.TryGetValue(key, out var slot)) return;
            ReleaseSlot(slot);
            _slots.Remove(key);
            Changed?.Invoke();
        }

        public void ReleaseAll()
        {
            if (_slots.Count == 0) return;
            foreach (var slot in _slots.Values) ReleaseSlot(slot);
            _slots.Clear();
            Changed?.Invoke();
        }

        private static void ReleaseSlot(Slot slot) => slot.Handle?.Release();
    }
}

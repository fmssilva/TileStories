using System;
using UnityEngine;

namespace TileStories
{
    // A fake preview stage that never touches the scene (_3.1 step 10A.2): what Phase A's gallery card previews through,
    // and what tests hand CardPreviewService so "a model loaded" is a fact they can count, never a real glTFast import or
    // a real RenderTexture camera. Loads (and fails) synchronously, unlike the real stage's async glTFast import, so a
    // test never waits a frame for it; a path containing MissingMarker behaves like a missing or wrong-type file.
    public sealed class ManualPreviewStage : IPreviewStage
    {
        public const string MissingMarker = "missing";

        private sealed class Handle : IPreviewHandle
        {
            private readonly ManualPreviewStage _owner;
            public RenderTexture Texture { get; }
            public bool Released { get; private set; }
            public int RenderCount { get; private set; }

            public Handle(ManualPreviewStage owner, RenderTexture texture)
            {
                _owner = owner;
                Texture = texture;
            }

            public void RenderNow(TurntableState turntable, PanoramaViewState panorama) => RenderCount++;

            public void Release()
            {
                if (Released) return;
                Released = true;
                _owner.AliveHandles--;
                if (Texture != null) UnityEngine.Object.DestroyImmediate(Texture);
            }
        }

        public int LoadCount { get; private set; }
        public int FailCount { get; private set; }
        // How many loaded handles have not been released yet -- what a real stage's "no GameObject/texture left behind"
        // memory check reduces to for this fake: it must be 0 once every slot that was loaded has been released.
        public int AliveHandles { get; private set; }

        public IPreviewHandle Load(MediaKind kind, IMediaSource media, string path, Action onReady, Action onFailed)
        {
            LoadCount++;
            if (string.IsNullOrWhiteSpace(path) || path.Contains(MissingMarker, StringComparison.OrdinalIgnoreCase))
            {
                FailCount++;
                onFailed?.Invoke();
                return null;
            }
            var texture = new RenderTexture(64, 64, 0);
            AliveHandles++;
            onReady?.Invoke();
            return new Handle(this, texture);
        }
    }
}

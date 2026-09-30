using UnityEngine;

namespace TileStories
{
    // The handles CardPreviewStage gives back (_3.1 step 10A.4.1): what one slot keeps for as long as it is requested. A handle
    // owns everything its subject made (the root GameObject with the model or sphere, the RenderTexture, the mesh and material of
    // a panorama) and Release leaves none of it behind. The stage does the drawing; a handle only says what to draw.
    public sealed partial class CardPreviewStage
    {
        // Where the model camera stands: how far back, what it orbits, and the model's bounding-sphere radius (the far plane's reach)
        private readonly struct ModelFit
        {
            public readonly float Distance;
            public readonly Vector3 Pivot;
            public readonly float Radius;

            public ModelFit(float distance, Vector3 pivot, float radius)
            {
                Distance = distance;
                Pivot = pivot;
                Radius = radius;
            }
        }

        // What every handle shares: its slot's root GameObject, the RenderTexture the view draws, resizing that texture to the
        // view's own aspect, and giving everything back
        private abstract class HandleBase : IPreviewHandle
        {
            protected readonly CardPreviewStage Stage;
            private readonly IMediaSource _media;
            private readonly string _path;
            private GameObject _root;
            private RenderTexture _texture;

            protected bool Released { get; private set; }
            protected GameObject Root => _root;

            protected HandleBase(CardPreviewStage stage, IMediaSource media, string path, GameObject root, RenderTexture texture)
            {
                Stage = stage;
                _media = media;
                _path = path;
                _root = root;
                _texture = texture;
            }

            public RenderTexture Texture => _texture;

            // The stage element resized: recreate the texture at its real aspect and let the subject refit to it (a no-op if the
            // size did not actually change -- a GeometryChangedEvent can fire with the same rect).
            public void Resize(int width, int height)
            {
                if (Released || _root == null) return;
                width = Mathf.Max(1, width);
                height = Mathf.Max(1, height);
                if (_texture != null && _texture.width == width && _texture.height == height) return;

                var old = _texture;
                _texture = NewTexture(old != null ? old.name : "CardPreview", width, height);
                DestroyTexture(old);
                OnResized((float)width / height);
            }

            // The texture has a new aspect: a model refits its camera distance to it
            protected virtual void OnResized(float aspect) { }

            public void RenderNow(TurntableState turntable, PanoramaViewState panorama)
            {
                if (Released || _root == null || _texture == null) return;
                Draw(_texture, turntable, panorama);
            }

            // Render this subject into `texture` the way the state that belongs to its kind says
            protected abstract void Draw(RenderTexture texture, TurntableState turntable, PanoramaViewState panorama);

            public void Release()
            {
                if (Released) return;
                Released = true;
                ReleaseSubject();
                if (_root != null) UnityEngine.Object.Destroy(_root);
                _root = null;
                DestroyTexture(_texture);
                _texture = null;
                _media?.Release(_path);
            }

            // Whatever the subject made besides the root (a panorama's mesh and material)
            protected virtual void ReleaseSubject() { }

            // A camera render leaves its target as RenderTexture.active, and releasing the active texture is a Unity warning
            private static void DestroyTexture(RenderTexture texture)
            {
                if (texture == null) return;
                if (RenderTexture.active == texture) RenderTexture.active = null;
                texture.Release();
                UnityEngine.Object.Destroy(texture);
            }
        }

        private sealed class ModelHandle : HandleBase
        {
            private readonly GameObject _model;
            private ModelFit _fit;

            public ModelHandle(CardPreviewStage stage, IMediaSource media, string path, GameObject root, RenderTexture texture, GameObject model, ModelFit fit)
                : base(stage, media, path, root, texture)
            {
                _model = model;
                _fit = fit;
            }

            protected override void OnResized(float aspect)
            {
                if (_model != null) _fit = FitFor(_model, aspect);
            }

            protected override void Draw(RenderTexture texture, TurntableState turntable, PanoramaViewState panorama) =>
                Stage.RenderModel(texture, turntable, _fit);
        }

        // The inside-out sphere: the camera stands at the root (the sphere's centre) and turns with the view state
        private sealed class PanoramaHandle : HandleBase
        {
            private Mesh _mesh;
            private Material _material;

            public PanoramaHandle(CardPreviewStage stage, IMediaSource media, string path, GameObject root, RenderTexture texture, Mesh mesh, Material material)
                : base(stage, media, path, root, texture)
            {
                _mesh = mesh;
                _material = material;
            }

            protected override void Draw(RenderTexture texture, TurntableState turntable, PanoramaViewState panorama) =>
                Stage.RenderPanorama(texture, Root.transform.position, panorama);

            protected override void ReleaseSubject()
            {
                if (_mesh != null) UnityEngine.Object.Destroy(_mesh);
                if (_material != null) UnityEngine.Object.Destroy(_material);
                _mesh = null;
                _material = null;
            }
        }

        // A handle whose media never resolved: Resize/RenderNow/Release are all no-ops (onFailed already fired once)
        private sealed class FailedHandle : IPreviewHandle
        {
            public RenderTexture Texture => null;
            public void Resize(int width, int height) { }
            public void RenderNow(TurntableState turntable, PanoramaViewState panorama) { }
            public void Release() { }
        }
    }
}

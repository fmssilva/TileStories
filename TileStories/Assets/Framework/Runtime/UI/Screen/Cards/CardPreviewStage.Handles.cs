using System.Collections.Generic;
using UnityEngine;

namespace TileStories
{
    // The handles CardPreviewStage gives back (_3.1 step 10A.4.1): what one slot keeps for as long as it is requested. A handle
    // owns everything its subject made (the root GameObject with the model or sphere, the RenderTexture, the mesh and material of
    // a panorama) and Release leaves none of it behind. The stage does the drawing; a handle only says what to draw.
    public sealed partial class CardPreviewStage
    {
        // What the model camera orbits and how far back it stands (ModelFitRule): the fit mode, the pivot, the bounding-sphere radius
        // (also the far plane's reach), the box corners around the pivot, the stage's aspect and -- for the visible fit only -- a sample
        // of the model's triangles. yaw_safe and visible follow the pitch, so their distance is asked per render (DistanceAt); visible
        // keeps what it worked out per whole degree of pitch, since it walks the triangles at every sampled yaw.
        private sealed class ModelFit
        {
            public readonly ModelFitMode Mode;
            public readonly Vector3 Pivot;
            public readonly float Radius;
            public readonly float Aspect;
            private readonly List<Vector3> _corners;
            private readonly List<Vector3> _triangles;
            private readonly float _fixedDistance;
            private readonly Dictionary<int, float> _visibleByPitch = new();

            public ModelFit(ModelFitMode mode, Vector3 pivot, float radius, List<Vector3> corners, List<Vector3> triangles, float aspect)
            {
                // - no triangle can be read (a mesh not marked readable in a build): the box of yaw_safe instead
                Mode = mode == ModelFitMode.Visible && (triangles == null || triangles.Count == 0) ? ModelFitMode.YawSafe : mode;
                Pivot = pivot;
                Radius = radius;
                Aspect = aspect;
                _corners = corners;
                _triangles = triangles;
                _fixedDistance = corners.Count == 0 ? 2f
                    : Mode == ModelFitMode.Sphere ? ModelFitRule.DistanceFor(radius, ModelFieldOfView, aspect)
                    : Mode == ModelFitMode.AtRest ? ModelFitRule.AtRestDistance(corners, ModelFieldOfView, aspect)
                    : 0f;
            }

            // The camera distance for a camera pitched `pitchDegrees` (yaw_safe and visible depend on it)
            public float DistanceAt(float pitchDegrees)
            {
                if (_corners.Count == 0 || (Mode != ModelFitMode.YawSafe && Mode != ModelFitMode.Visible)) return _fixedDistance;
                if (Mode == ModelFitMode.YawSafe) return ModelFitRule.YawSafeDistance(_corners, pitchDegrees, ModelFieldOfView, Aspect);
                int degree = Mathf.RoundToInt(pitchDegrees);
                if (!_visibleByPitch.TryGetValue(degree, out float distance))
                {
                    // - the faces are judged from where the box fit would stand: never closer than the camera ends up
                    float reference = ModelFitRule.YawSafeDistance(_corners, degree, ModelFieldOfView, Aspect);
                    distance = ModelFitRule.VisibleDistance(_triangles, degree, reference, ModelFieldOfView, Aspect);
                    _visibleByPitch[degree] = distance;
                }
                return distance;
            }

            // The same fit for another stage aspect or another mode (the triangles are kept; they depend on neither)
            public ModelFit With(float aspect, ModelFitMode mode, List<Vector3> triangles) =>
                new(mode, Pivot, Radius, _corners, triangles ?? _triangles, aspect);

            public bool HasTriangles => _triangles != null && _triangles.Count > 0;
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

            // How a model is framed (a panorama ignores it)
            public virtual void SetFit(ModelFitMode mode) { }

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
            private ModelFit _fit;
            // What the block asked for (the fit falls back to yaw_safe while the visible fit has no triangles to read)
            private ModelFitMode _requested = ModelFitMode.YawSafe;

            public ModelHandle(CardPreviewStage stage, IMediaSource media, string path, GameObject root, RenderTexture texture, ModelFit fit)
                : base(stage, media, path, root, texture)
            {
                _fit = fit;
            }

            protected override void OnResized(float aspect) => _fit = _fit.With(aspect, _requested, null);

            // - the visible fit reads the model's triangles the first time it is picked, never for the other fits
            public override void SetFit(ModelFitMode mode)
            {
                _requested = mode;
                bool needsTriangles = mode == ModelFitMode.Visible && !_fit.HasTriangles && Root != null;
                _fit = _fit.With(_fit.Aspect, mode, needsTriangles ? TrianglesAround(Root, _fit.Pivot) : null);
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
            public void SetFit(ModelFitMode mode) { }
            public void RenderNow(TurntableState turntable, PanoramaViewState panorama) { }
            public void Release() { }
        }
    }
}

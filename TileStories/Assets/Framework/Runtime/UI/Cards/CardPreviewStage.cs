using System;
using UnityEngine;

namespace TileStories
{
    // The real IPreviewStage (_3.1 step 10A.2b.1): a disabled camera, far from the wall on its own layer, rendering
    // ONE model at a time on demand into a RenderTexture per handle. A block's media resolves through the media
    // source exactly like any other asset (10A.1 already wired MediaKind.Model to a GameObject: the .glb is imported
    // by glTFast's own ScriptedImporter at asset-import time, so this stage never touches the glTFast runtime API --
    // it Instantiates an ordinary prefab, same as any other Resources asset). Lives on the same GameObject as
    // PoiCardHost (GetComponent-or-AddComponent, same pattern as CardAudioPlayer/CardVideoPlayer).
    public sealed class CardPreviewStage : MonoBehaviour, IPreviewStage
    {
        // A different height than the other far-away dev rigs (EffectsPreviewSpawner 5000, OutlinePreviewSpawner
        // 6500) so none of them ever coincide.
        public const float FarOffsetMetres = 8000f;
        // Square, small enough to stay cheap when several blocks hold a live handle at once, large enough that a
        // half-card-width turntable does not show resampling.
        public const int TextureSize = 512;
        // Spacing between concurrently-held handles along the stage's own X axis: generous enough that a model's
        // bounds (the fitted camera distance) never lets one handle's render bleed into a neighbour's.
        private const float HandleSpacingMetres = 50f;

        private sealed class Handle : IPreviewHandle
        {
            private readonly CardPreviewStage _stage;
            private readonly IMediaSource _media;
            private readonly string _path;
            private GameObject _root;
            private GameObject _model;
            private RenderTexture _texture;
            private float _fitDistance = 2f;
            private bool _released;

            public Handle(CardPreviewStage stage, IMediaSource media, string path, GameObject root, GameObject model, RenderTexture texture, float fitDistance)
            {
                _stage = stage;
                _media = media;
                _path = path;
                _root = root;
                _model = model;
                _texture = texture;
                _fitDistance = fitDistance;
            }

            public RenderTexture Texture => _texture;

            public void RenderNow(TurntableState turntable, PanoramaViewState panorama)
            {
                if (_released || _model == null || _texture == null) return;
                _stage.RenderModel(_model, _texture, turntable, _fitDistance);
            }

            public void Release()
            {
                if (_released) return;
                _released = true;
                if (_root != null) UnityEngine.Object.Destroy(_root);
                _root = null;
                _model = null;
                if (_texture != null)
                {
                    _texture.Release();
                    UnityEngine.Object.Destroy(_texture);
                    _texture = null;
                }
                _media?.Release(_path);
            }
        }

        // A handle whose media never resolved: RenderNow/Release are both no-ops (onFailed already fired once)
        private sealed class FailedHandle : IPreviewHandle
        {
            public RenderTexture Texture => null;
            public void RenderNow(TurntableState turntable, PanoramaViewState panorama) { }
            public void Release() { }
        }

        private Camera _camera;
        private Light _key;
        private Light _fill;
        private Transform _rig;
        private int _nextHandleSlot;
        private int _previewLayer = -1;

        public IPreviewHandle Load(MediaKind kind, IMediaSource media, string path, Action onReady, Action onFailed)
        {
            if (kind != MediaKind.Model)
                throw new NotSupportedException("CardPreviewStage does not build " + kind + " yet (panorama_360 is 10A.4)");

            EnsureRig();
            var prefab = media?.Load<GameObject>(path);
            if (prefab == null)
            {
                onFailed?.Invoke();
                return new FailedHandle();
            }

            int slot = _nextHandleSlot++;
            var root = new GameObject("CardPreviewHandle_" + slot);
            root.transform.SetParent(_rig, false);
            root.transform.localPosition = new Vector3(slot * HandleSpacingMetres, 0f, 0f);

            var model = UnityEngine.Object.Instantiate(prefab, root.transform);
            model.transform.localPosition = Vector3.zero;
            SetLayerRecursively(model, _previewLayer);

            float fitDistance = FitDistanceFor(model, _camera.fieldOfView, _camera.aspect);

            var texture = new RenderTexture(TextureSize, TextureSize, 16) { name = "CardPreview_" + slot };
            texture.Create();

            onReady?.Invoke();
            return new Handle(this, media, path, root, model, texture, fitDistance);
        }

        private void RenderModel(GameObject model, RenderTexture texture, TurntableState turntable, float fitDistance)
        {
            var pivot = model.transform.position;
            var rotation = Quaternion.Euler(turntable.Pitch, turntable.Yaw, 0f);
            float distance = fitDistance / Mathf.Max(TurntableRule.MinZoom, turntable.Zoom);
            _camera.transform.position = pivot - rotation * Vector3.forward * distance;
            _camera.transform.rotation = rotation;
            _camera.targetTexture = texture;
            _camera.Render();
        }

        private void EnsureRig()
        {
            if (_rig != null) return;
            _previewLayer = LayerMask.NameToLayer("CardPreview");
            if (_previewLayer < 0)
                throw new InvalidOperationException("The 'CardPreview' layer is missing from this project's TagManager (see 10A.2b.1)");

            var rigGo = new GameObject("CardPreviewRig");
            rigGo.transform.SetParent(transform, false);
            rigGo.transform.position = new Vector3(0f, FarOffsetMetres, 0f);
            _rig = rigGo.transform;

            var cameraGo = new GameObject("CardPreviewCamera");
            cameraGo.transform.SetParent(_rig, false);
            _camera = cameraGo.AddComponent<Camera>();
            _camera.enabled = false; // rendered only on demand, from RenderNow -- never every frame
            _camera.cullingMask = 1 << _previewLayer;
            _camera.clearFlags = CameraClearFlags.SolidColor;
            _camera.backgroundColor = default; // transparent black -- not a "look" (CardViewSourceRulesTests), just an empty clear
            _camera.nearClipPlane = 0.05f;
            _camera.farClipPlane = 100f;
            _camera.allowHDR = false;
            _camera.allowMSAA = false;

            var keyGo = new GameObject("CardPreviewKeyLight");
            keyGo.transform.SetParent(_rig, false);
            keyGo.transform.rotation = Quaternion.Euler(40f, -30f, 0f);
            _key = keyGo.AddComponent<Light>();
            _key.type = LightType.Directional;
            _key.cullingMask = 1 << _previewLayer;
            _key.intensity = 1.2f;

            var fillGo = new GameObject("CardPreviewFillLight");
            fillGo.transform.SetParent(_rig, false);
            fillGo.transform.rotation = Quaternion.Euler(30f, 150f, 0f);
            _fill = fillGo.AddComponent<Light>();
            _fill.type = LightType.Directional;
            _fill.cullingMask = 1 << _previewLayer;
            _fill.intensity = 0.4f;
        }

        // The camera distance that frames the model's own bounds fully, at its own field of view/aspect (the same
        // fit-to-bounds idea as DevPreviewGridLayout.FitDistance, but off one renderer's real bounds instead of a
        // fixed grid extent, since a model_3d asset can be any size)
        private static float FitDistanceFor(GameObject model, float fovDegrees, float aspect)
        {
            var renderers = model.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return 2f;
            var bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            float radius = Mathf.Max(bounds.extents.magnitude, 0.01f);
            float halfFov = fovDegrees * 0.5f * Mathf.Deg2Rad;
            float verticalDistance = radius / Mathf.Sin(halfFov);
            float horizontalFov = 2f * Mathf.Atan(Mathf.Tan(halfFov) * Mathf.Max(aspect, 0.01f));
            float horizontalDistance = radius / Mathf.Sin(horizontalFov * 0.5f);
            return Mathf.Max(verticalDistance, horizontalDistance) * 1.15f;
        }

        private static void SetLayerRecursively(GameObject go, int layer)
        {
            go.layer = layer;
            var t = go.transform;
            for (int i = 0; i < t.childCount; i++) SetLayerRecursively(t.GetChild(i).gameObject, layer);
        }
    }
}

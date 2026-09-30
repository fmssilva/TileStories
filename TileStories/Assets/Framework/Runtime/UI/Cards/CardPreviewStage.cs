using System;
using System.Collections.Generic;
using UnityEngine;

namespace TileStories
{
    // The real IPreviewStage (_3.1 steps 10A.2b.1 and 10A.4.1): a disabled camera, far from the wall on its own layer, rendering
    // ONE subject at a time on demand into a RenderTexture per handle -- a model (Instantiated from the prefab glTFast's own
    // importer made at asset-import time, so this stage never touches the glTFast runtime API) or a 360 panorama (an inside-out
    // sphere wearing the picture, the camera at its centre). A block's media resolves through the media source exactly like any
    // other asset. Lives on the same GameObject as PoiCardHost (GetComponent-or-AddComponent, same pattern as
    // CardAudioPlayer/CardVideoPlayer). The handle classes are in CardPreviewStage.Handles.cs.
    public sealed partial class CardPreviewStage : MonoBehaviour, IPreviewStage
    {
        // A different height than the other far-away dev rigs (EffectsPreviewSpawner 5000, OutlinePreviewSpawner
        // 6500) so none of them ever coincide.
        public const float FarOffsetMetres = 8000f;
        // The initial guess before the view's own GeometryChangedEvent reports its real pixel size (Resize then
        // recreates this at the stage's real aspect): square, small enough to stay cheap, large enough that the
        // very first frame or two are not visibly blocky.
        public const int TextureSize = 512;
        // Spacing between concurrently-held handles along the stage's own X axis. Each render's far plane stops at its own
        // subject (see RenderModel / RenderPanorama), so a neighbour this far away is never drawn behind it.
        private const float HandleSpacingMetres = 50f;
        // The model camera's field of view: the fit (ModelFitRule) and every model render use this one value, because the
        // panorama renders change the shared camera's field of view to the visitor's own zoom
        private const float ModelFieldOfView = 60f;
        // The panorama sphere's radius: far enough that the near plane never touches it, small enough to sit inside one slot
        private const float PanoramaRadiusMetres = 10f;
        private const float NearPlaneMetres = 0.01f;
        // The far plane reaches this far past the subject's own sphere: room for its back, never a neighbour
        private const float SubjectMarginRatio = 1.05f;
        private const string PanoramaMaterialResource = "TileStories/PanoramaUnlit";

        private Camera _camera;
        private Light _key;
        private Light _fill;
        private Transform _rig;
        private int _nextHandleSlot;
        private int _previewLayer = -1;
        private Material _panoramaMaterial;

        public IPreviewHandle Load(MediaKind kind, IMediaSource media, string path, Action onReady, Action onFailed)
        {
            switch (kind)
            {
                case MediaKind.Model: return LoadModel(media, path, onReady, onFailed);
                case MediaKind.Panorama: return LoadPanorama(media, path, onReady, onFailed);
                default: throw new NotSupportedException("CardPreviewStage does not build " + kind);
            }
        }

        private IPreviewHandle LoadModel(IMediaSource media, string path, Action onReady, Action onFailed)
        {
            EnsureRig();
            var prefab = media?.Load<GameObject>(path);
            if (prefab == null)
            {
                onFailed?.Invoke();
                return new FailedHandle();
            }

            var root = NewHandleRoot(out int slot);
            var model = UnityEngine.Object.Instantiate(prefab, root.transform);
            model.transform.localPosition = Vector3.zero;
            SetLayerRecursively(model, _previewLayer);

            var texture = NewTexture("CardPreview_" + slot, TextureSize, TextureSize);
            var fit = FitFor(model, 1f); // TextureSize is square; Resize refits once the real stage size is known

            onReady?.Invoke();
            return new ModelHandle(this, media, path, root, texture, model, fit);
        }

        private IPreviewHandle LoadPanorama(IMediaSource media, string path, Action onReady, Action onFailed)
        {
            EnsureRig();
            var picture = media?.Load<Texture2D>(path);
            if (picture == null || _panoramaMaterial == null)
            {
                if (picture != null) media.Release(path);
                onFailed?.Invoke();
                return new FailedHandle();
            }

            var root = NewHandleRoot(out int slot);
            var geometry = PanoramaSphereRule.Build(PanoramaRadiusMetres);
            var mesh = new Mesh { name = "CardPanoramaSphere_" + slot, vertices = geometry.Vertices, uv = geometry.Uvs, triangles = geometry.Triangles };
            mesh.RecalculateBounds();
            // - its own copy of the shared material: the picture is this handle's, the shader and settings are the asset's
            var material = new Material(_panoramaMaterial) { name = "CardPanorama_" + slot, mainTexture = picture };
            var sphere = new GameObject("CardPanoramaSphere", typeof(MeshFilter), typeof(MeshRenderer));
            sphere.transform.SetParent(root.transform, false);
            sphere.layer = _previewLayer;
            sphere.GetComponent<MeshFilter>().sharedMesh = mesh;
            var meshRenderer = sphere.GetComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = material;
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;

            var texture = NewTexture("CardPreview_" + slot, TextureSize, TextureSize);
            onReady?.Invoke();
            return new PanoramaHandle(this, media, path, root, texture, mesh, material);
        }

        // One slot along the rig's X axis, for the next handle's model or sphere
        private GameObject NewHandleRoot(out int slot)
        {
            slot = _nextHandleSlot++;
            var root = new GameObject("CardPreviewHandle_" + slot);
            root.transform.SetParent(_rig, false);
            root.transform.localPosition = new Vector3(slot * HandleSpacingMetres, 0f, 0f);
            return root;
        }

        private static RenderTexture NewTexture(string name, int width, int height)
        {
            var texture = new RenderTexture(Mathf.Max(1, width), Mathf.Max(1, height), 16) { name = name };
            texture.Create();
            return texture;
        }

        private void RenderModel(RenderTexture texture, TurntableState turntable, ModelFit fit)
        {
            var rotation = Quaternion.Euler(turntable.Pitch, turntable.Yaw, 0f);
            float distance = fit.Distance / Mathf.Max(TurntableRule.MinZoom, turntable.Zoom);
            float reach = fit.Radius * SubjectMarginRatio;
            _camera.fieldOfView = ModelFieldOfView;
            _camera.nearClipPlane = Mathf.Max(NearPlaneMetres, distance - reach);
            _camera.farClipPlane = distance + reach;
            _camera.transform.SetPositionAndRotation(fit.Pivot - rotation * Vector3.forward * distance, rotation);
            Render(texture);
        }

        private void RenderPanorama(RenderTexture texture, Vector3 centre, PanoramaViewState view)
        {
            _camera.fieldOfView = PanoramaViewRule.VerticalFov(view.Fov, (float)texture.width / texture.height);
            _camera.nearClipPlane = NearPlaneMetres;
            _camera.farClipPlane = PanoramaRadiusMetres * SubjectMarginRatio;
            // - Euler x turns DOWN when positive, the rule's pitch looks UP when positive
            _camera.transform.SetPositionAndRotation(centre, Quaternion.Euler(-view.Pitch, view.Yaw, 0f));
            Render(texture);
        }

        private void Render(RenderTexture texture)
        {
            _camera.aspect = (float)texture.width / texture.height;
            _camera.targetTexture = texture;
            _camera.Render();
        }

        private void EnsureRig()
        {
            if (_rig != null) return;
            _previewLayer = LayerMask.NameToLayer("CardPreview");
            if (_previewLayer < 0)
                throw new InvalidOperationException("The 'CardPreview' layer is missing from this project's TagManager (see 10A.2b.1)");
            _panoramaMaterial = Resources.Load<Material>(PanoramaMaterialResource);

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
            _camera.fieldOfView = ModelFieldOfView;
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

        // Where the camera stands and what it orbits so the model's bounding SPHERE fills ModelFitRule.TargetFillOfShorterSide of
        // the STAGE's shorter axis (_3.1 10A.3-fix.2). A sphere never changes when the model turns, so a drag or auto-spin to any
        // yaw/pitch keeps every pixel of the model inside the picture (the box fitted at rest let a turned, wide model touch the
        // frame's edge). `aspect` is the stage's own width / height.
        private static ModelFit FitFor(GameObject model, float aspect)
        {
            var boxes = new List<Bounds>();
            foreach (var renderer in model.GetComponentsInChildren<Renderer>()) boxes.Add(renderer.bounds);
            if (boxes.Count == 0) return new ModelFit(2f, model.transform.position, 1f);
            var (centre, radius) = ModelFitRule.SphereOf(boxes);
            return new ModelFit(ModelFitRule.DistanceFor(radius, ModelFieldOfView, aspect), centre, radius);
        }

        private static void SetLayerRecursively(GameObject go, int layer)
        {
            go.layer = layer;
            var t = go.transform;
            for (int i = 0; i < t.childCount; i++) SetLayerRecursively(t.GetChild(i).gameObject, layer);
        }
    }
}

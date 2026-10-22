using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TileStories.Dev.StyleSpike
{
    // DEV-ONLY (_3.3 P0 S5): the AR camera stand-in for the glass test. No AR feed in the Editor, so a PHOTO of a sunny wall is drawn
    // by the stage camera on a quad that fills its view (camera-rendered, BEHIND the UI panel, like the AR feed), and a second camera
    // renders the same view into a small RenderTexture (1/Downscale of the screen): the frosted sheet samples that copy.
    public sealed class SpikeGlassRig : IDisposable
    {
        public const string UnlitShader = "Universal Render Pipeline/Unlit";
        public const float PhotoDistance = 10f;

        public RenderTexture LowRes { get; private set; }
        public Camera LowResCamera { get; private set; }

        private readonly GameObject _quad;
        private readonly Material _photoMaterial;
        private readonly GameObject _lowResObject;

        // Put the photo behind `camera` (scaled to COVER its view) and, when downscale > 0, a low-res copy camera beside it
        public SpikeGlassRig(Camera camera, Texture2D photo, int downscale)
        {
            Debug.Assert(photo != null, "[StyleSpike] the glass rig needs a photo");
            var shader = Shader.Find(UnlitShader);
            Debug.Assert(shader != null, "[StyleSpike] URP Unlit shader not found");
            _photoMaterial = new Material(shader);
            _photoMaterial.SetTexture("_BaseMap", photo);

            camera.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            float viewH = 2f * PhotoDistance * Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            float viewW = viewH * camera.aspect;
            float photoAspect = (float)photo.width / photo.height;
            float h = Mathf.Max(viewH, viewW / photoAspect);          // cover: no empty band at any screen proportion
            _quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            _quad.name = "StyleSpike Photo";
            Object.Destroy(_quad.GetComponent<Collider>());
            _quad.transform.SetPositionAndRotation(new Vector3(0, 0, PhotoDistance), Quaternion.identity);
            _quad.transform.localScale = new Vector3(h * photoAspect, h, 1f);
            _quad.GetComponent<MeshRenderer>().sharedMaterial = _photoMaterial;

            if (downscale <= 0) return;
            LowRes = new RenderTexture(Mathf.Max(1, Screen.width / downscale), Mathf.Max(1, Screen.height / downscale), 16)
            {
                name = "StyleSpike LowRes", filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp
            };
            _lowResObject = new GameObject("StyleSpike LowRes Camera");
            LowResCamera = _lowResObject.AddComponent<Camera>();
            LowResCamera.CopyFrom(camera);
            LowResCamera.tag = "Untagged";
            LowResCamera.depth = camera.depth - 1;
            LowResCamera.targetTexture = LowRes;
        }

        // Tear down the photo, the copy camera and its texture
        public void Dispose()
        {
            if (_lowResObject != null) Object.Destroy(_lowResObject);
            if (LowRes != null) { LowRes.Release(); Object.Destroy(LowRes); }
            if (_quad != null) Object.Destroy(_quad);
            if (_photoMaterial != null) Object.Destroy(_photoMaterial);
        }
    }
}

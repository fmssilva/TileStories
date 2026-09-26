using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TileStories.Tests
{
    // Shared set-up of the Displacement PlayMode suites (_2.5): the shipped LivingRoom config spawned by a
    // real WallSession through the real POI_Marker prefab, a real LODController running step 8, and a real
    // MainCamera rendering into a fixed 1280 x 720 target (pixel distances are deterministic). Scenarios
    // come from the real dev-only displacement demo, switched on through WallSession.ApplyDisplacementDemo
    // -- the same seam the POI Editor's live push calls -- so every test also exercises the demo.
    public abstract class DisplacementDemoFixture
    {
        private const string MarkerPrefabPath = "Assets/Framework/Runtime/UI/Markers/POI_Marker.prefab";
        private const string ClusterPrefabPath = "Assets/Framework/Runtime/UI/Markers/POI_Cluster.prefab";
        protected const int Width = 1280;
        protected const int Height = 720;

        private readonly List<Object> _tracked = new();
        protected WallConfigData Config;
        protected WallSession Session;
        protected LODController Lod;
        protected Camera Cam;
        protected GameObject[] WallPois;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            WallConfigData config = null;
            yield return WallConfigLoader.LoadFromStreamingAssets("LivingRoom/config.json", c => config = c);
            Assert.IsNotNull(config, "StreamingAssets/LivingRoom/config.json must load for this test.");
            Config = config;
            // - every other demo view off: the displacement demo is the only thing on screen
            Config.effect_defaults.preview = new EffectDefaults.EffectPreviewSettings();
            Config.outline_preview = new OutlinePreviewSettings();
            Config.demo_field = new DemoFieldSettings();
            Config.displacement_demo = new DisplacementDemoSettings();
            ARZoomState.SetZoom(1f, 1f, 4f);

            var camGO = new GameObject("DisplacementTestCamera", typeof(Camera)) { tag = "MainCamera" };
            Cam = camGO.GetComponent<Camera>();
            Cam.fieldOfView = 60f;
            Cam.clearFlags = CameraClearFlags.SolidColor;
            Cam.backgroundColor = Color.black;
            var target = new RenderTexture(Width, Height, 24);
            Cam.targetTexture = target;
            _tracked.Add(camGO);
            _tracked.Add(target);

            var anchorGO = new GameObject("PlacementCorrectionAnchor");
            var wsGO = new GameObject("WallSessionHolder");
            wsGO.SetActive(false); // no Awake: no config reload, no tracker lookup
            Session = wsGO.AddComponent<WallSession>();
            _tracked.Add(anchorGO);
            _tracked.Add(wsGO);
#if UNITY_EDITOR
            SetField(Session, "_config", Config);
            SetField(Session, "poiAnchorPrefab", AssetDatabase.LoadAssetAtPath<GameObject>(MarkerPrefabPath));
            SetField(Session, "correctionAnchor", anchorGO.transform);
            Session.GetType().GetMethod("SpawnPOIs", BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(Session, null);

            var lodGO = new GameObject("LODController", typeof(LODController));
            _tracked.Add(lodGO);
            Lod = lodGO.GetComponent<LODController>();
            SetField(Lod, "_wallSession", Session);
            SetField(Lod, "_camera", Cam);
            SetField(Lod, "_clusterPrefab", AssetDatabase.LoadAssetAtPath<GameObject>(ClusterPrefabPath));
#else
            Assert.Fail("Needs the Editor (AssetDatabase).");
#endif
            WallPois = Session.SpawnedMarkers.Select(m => m.gameObject).ToArray();
            Assert.AreEqual(Config.pois.Count, WallPois.Length, "precondition: every wall POI spawned");
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            MarkerHierarchyResolver.ResetToDefaults();
            StatusRamp.ResetToDefaults();
            ARZoomState.SetZoom(1f, 1f, 4f);
            foreach (var o in _tracked)
                if (o != null) Object.DestroyImmediate(o);
            _tracked.Clear();
            yield return null;
        }

        // ---- driving the real seams ----

        // Switch the demo on (or change it) and let displacement settle on it
        protected IEnumerator Demo(DisplacementDemoSettings demo)
        {
            demo.enabled = true;
            Session.ApplyDisplacementDemo(demo);
            SkipReveals();
            yield return Settle();
        }

        // Swap the wall's displacement settings live and let them settle
        protected IEnumerator Displacement(DisplacementSettings settings)
        {
            Session.ApplyDisplacementSettings(settings);
            yield return Settle();
        }

        // A frame for LODController.Update to pick up new settings objects, then three evaluations (the
        // 2-cycle hysteresis commits on the second), each followed by a frame so LateUpdate draws lines
        protected IEnumerator Settle()
        {
            yield return null;
            for (int i = 0; i < 3; i++)
            {
                Lod.Evaluate();
                yield return null;
            }
        }

        // Default settings with a few fields changed
        protected static DisplacementSettings D(System.Action<DisplacementSettings> edit = null)
        {
            var s = new DisplacementSettings();
            edit?.Invoke(s);
            return s;
        }

        private void SkipReveals()
        {
            if (Session.DisplacementDemoRoot == null) return;
            foreach (var reveal in Session.DisplacementDemoRoot.GetComponentsInChildren<MarkerRevealEffect>())
                reveal.SkipToEnd();
        }

        // ---- reading what the visitor sees ----

        protected DisplacementStats Stats => Lod.LastDisplacementStats;

        protected List<MarkerView> LiveMarkers => Session.SpawnedMarkers.ToList();

        protected List<MarkerView> ReferenceMarkers => Session.DisplacementDemoRoot == null
            ? new List<MarkerView>()
            : Session.DisplacementDemoRoot.GetComponentsInChildren<MarkerView>(true).Where(m => m.PoiId.EndsWith("_ref")).ToList();

        protected MarkerView Live(string id)
        {
            var m = Session.SpawnedMarkers.FirstOrDefault(v => v.PoiId == id);
            Assert.IsNotNull(m, "live demo marker " + id);
            return m;
        }

        protected MarkerView Reference(string id)
        {
            var m = ReferenceMarkers.FirstOrDefault(v => v.PoiId == id + "_ref");
            Assert.IsNotNull(m, "reference copy of " + id);
            return m;
        }

        protected MarkerView[] Group(string shortName) =>
            LiveMarkers.Where(m => m.PoiId.StartsWith("ddemo_" + shortName + "_")).ToArray();

        protected Vector2 ScreenOf(Vector3 world)
        {
            Vector3 s = Cam.WorldToScreenPoint(world);
            return new Vector2(s.x, s.y);
        }

        protected Vector2 LabelScreen(MarkerView m) => ScreenOf(m.LabelRect.position);

        protected static MarkerLeaderLine Line(MarkerView m) => m.GetComponent<MarkerLeaderLine>();

        protected static LineRenderer Renderer(MarkerView m) => m.GetComponent<LineRenderer>();

        protected int ShownLines => LiveMarkers.Count(m => Line(m).IsShown);

        protected Texture2D Render()
        {
            Cam.Render();
            var previous = RenderTexture.active;
            RenderTexture.active = Cam.targetTexture;
            var tex = new Texture2D(Width, Height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
            tex.Apply();
            RenderTexture.active = previous;
            _tracked.Add(tex);
            return tex;
        }

        // Evidence for the vision pass: Assets/Screenshots/<name>.png
        protected static void SavePng(Texture2D tex, string name)
        {
            string dir = Path.Combine(Application.dataPath, "Screenshots");
            Directory.CreateDirectory(dir);
            File.WriteAllBytes(Path.Combine(dir, name + ".png"), tex.EncodeToPNG());
        }

        protected static void SetField(object target, string name, object value)
        {
            var f = target.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(f, target.GetType().Name + "." + name + " must exist");
            f.SetValue(target, value);
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace TileStories.Editor.Tests
{
    // Live Play Mode sync, FIELD BY FIELD, for the LOD, Zoom and LOD demo field settings (the companion
    // of LiveSyncFieldMatrixTests, which covers the marker-visual domains). Every public field of
    // LodSettings / ZoomSettings / DemoFieldSettings is walked by reflection -- a new field gets a case
    // automatically -- edited in the authoring config and pushed through the SAME applier list the window
    // uses (LivePlayModeConfigPush.CreateDispatcher) onto a REAL WallSession with REAL markers. Each case
    // asserts the running wall now reads the new value, from its own copy (never the authoring object),
    // and that only the owning domain's applier fired. What the runtime DOES with a swapped settings
    // object is proven separately: LodRealPipelineTests (LOD off restores, clusters), the demo-field tests
    // below, and ARZoom tests.
    public class LodZoomLiveSyncTests
    {
        private const string MarkerPrefabPath = "Assets/Framework/Runtime/UI/Markers/POI_Marker.prefab";
        private readonly List<UnityEngine.Object> _created = new();

        // Main cameras of the scene the EditMode run happens in, switched off while a test owns Camera.main
        private readonly List<Camera> _silencedCameras = new();

        [TearDown]
        public void TearDown()
        {
            foreach (var o in _created) if (o != null) UnityEngine.Object.DestroyImmediate(o);
            _created.Clear();
            foreach (var c in _silencedCameras) if (c != null) c.enabled = true;
            _silencedCameras.Clear();
            MarkerHierarchyResolver.ResetToDefaults();
            StatusRamp.ResetToDefaults();
        }

        public sealed class Row
        {
            public string Block;         // lod_settings / zoom_settings / demo_field
            public string Applier;       // the applier that must fire
            public FieldInfo Field;
            public override string ToString() => Block + "." + Field.Name;
        }

        private static readonly (string block, Type type, string applier)[] Blocks =
        {
            ("lod_settings", typeof(LodSettings), "lod"),
            ("zoom_settings", typeof(ZoomSettings), "zoom"),
            ("demo_field", typeof(DemoFieldSettings), "demo field"),
        };

        // Alternative values for the string fields (each is a dropdown in the Editor Tab)
        private static readonly Dictionary<string, string> StringAlternatives = new()
        {
            { "density_response_mode", "select_hide" },
            { "cluster_icon_mode", "count_only" },
            { "cluster_band_source", "farthest_member" },
        };

        public static IEnumerable<TestCaseData> AllRows()
        {
            foreach (var (block, type, applier) in Blocks)
                foreach (var f in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
                    yield return new TestCaseData(new Row { Block = block, Applier = applier, Field = f })
                        .SetName("LiveSync_" + block + "." + f.Name);
        }

        private static object BlockOf(WallConfigData c, string block) =>
            typeof(WallConfigData).GetField(block).GetValue(c);

        // Edit one field to a different, valid value; returns a text form of the new value
        private static string Edit(object owner, FieldInfo field)
        {
            var t = field.FieldType;
            object value = field.GetValue(owner);
            if (t == typeof(bool)) field.SetValue(owner, !(bool)value);
            else if (t == typeof(float)) field.SetValue(owner, (float)value + 0.5f);
            else if (t == typeof(int)) field.SetValue(owner, (int)value + 1);
            else if (t == typeof(string))
            {
                Assert.IsTrue(StringAlternatives.ContainsKey(field.Name), "no live-sync value for " + field.Name);
                field.SetValue(owner, StringAlternatives[field.Name]);
            }
            else if (t == typeof(List<LodBandEntry>))
                ((List<LodBandEntry>)value)[0].max_distance_m = 1.25f;          // the Distance Bands table
            else if (t == typeof(List<DemoFieldLevelCount>))
                ((List<DemoFieldLevelCount>)value).Add(new DemoFieldLevelCount { level_key = "level_1", count = 3 });
            else Assert.Fail("no live-sync edit for " + t + " " + field.Name);
            return JsonUtility.ToJson(owner);
        }

        [TestCaseSource(nameof(AllRows))]
        public void LiveEdit_ReachesTheRunningWall(Row row)
        {
            var authoring = LoadShippedConfig();
            var session = NewSessionOn(Copy(authoring));
            var dispatcher = LivePlayModeConfigPush.CreateDispatcher();
            dispatcher.Push(session, authoring);   // first push of a Play session: every domain once

            string expected = Edit(BlockOf(authoring, row.Block), row.Field);
            var applied = dispatcher.Push(session, authoring);

            CollectionAssert.AreEqual(new[] { row.Applier }, applied, row + ": exactly the owning domain's applier fires");
            var running = typeof(WallSession).GetField("_config", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(session) as WallConfigData;
            var runningBlock = BlockOf(running, row.Block);
            Assert.AreEqual(expected, JsonUtility.ToJson(runningBlock), row + ": the running wall reads the edited value");
            Assert.AreNotSame(BlockOf(authoring, row.Block), runningBlock, row + ": the wall got a private copy, not the authoring object");
        }

        [Test]
        public void DemoField_SwitchedOnLive_SpawnsItsMarkers_AndTheyReplaceTheWallsOwn()
        {
            var cam = NewOnlyMainCamera();
            var wallPose = new Pose(new Vector3(-1.2f, 0.3f, 0.4f), Quaternion.Euler(10f, 283f, 0f));
            cam.transform.SetPositionAndRotation(wallPose.position, wallPose.rotation);
            var authoring = LoadShippedConfig();
            var session = NewSessionOn(Copy(authoring));
            var wallIds = session.SpawnedMarkers.Select(m => m.PoiId).ToList();
            var dispatcher = LivePlayModeConfigPush.CreateDispatcher();
            dispatcher.Push(session, authoring);
            Assert.IsNull(session.DemoFieldRoot, "precondition: the switch is off in the shipped config");

            var demo = authoring.demo_field;
            demo.enabled = true;
            demo.dense_clump_count = 4;
            demo.level_counts = authoring.hierarchy_levels.Select(l => new DemoFieldLevelCount { level_key = l.key, count = 2 }).ToList();
            dispatcher.Push(session, authoring);

            Assert.IsNotNull(session.DemoFieldRoot, "the field spawns live");
            _created.Add(session.DemoFieldRoot);
            Assert.AreEqual(DemoFieldStage.EditorStagePosition, session.DemoFieldRoot.transform.position, "the field stands on the empty stage");
            Assert.AreEqual(DemoFieldStage.EditorStagePosition, cam.transform.position, "the camera moved to the stage's start");

            // any other edit rebuilds the field in the SAME place, wherever the camera has walked since
            cam.transform.position += new Vector3(0f, 0f, 5f);
            demo.seed++;
            dispatcher.Push(session, authoring);
            _created.Add(session.DemoFieldRoot);
            Assert.AreEqual(DemoFieldStage.EditorStagePosition, session.DemoFieldRoot.transform.position, "a rebuild never moves the field");
            int expected = authoring.hierarchy_levels.Count * 2 + 4;
            Assert.AreEqual(expected, session.SpawnedMarkers.Count, "the wall now runs exactly the demo markers");
            Assert.IsTrue(session.SpawnedMarkers.All(m => m.PoiId.StartsWith("demo_")), "no wall POI is evaluated while hidden");
            foreach (var id in wallIds)
                Assert.IsFalse(GameObject.Find("LiveLodAnchor").transform.Find(id).gameObject.activeSelf, id + " is paused while the field hides the wall");

            demo.enabled = false;
            dispatcher.Push(session, authoring);
            Assert.IsNull(session.DemoFieldRoot, "switching it off removes the field");
            Assert.AreEqual(wallPose.position, cam.transform.position, "the camera is back where it was before the stage");
            Assert.Less(Quaternion.Angle(wallPose.rotation, cam.transform.rotation), 0.01f, "facing the same way");
            CollectionAssert.AreEquivalent(wallIds, session.SpawnedMarkers.Select(m => m.PoiId), "the wall's own POIs are back");
            foreach (var id in wallIds)
                Assert.IsTrue(GameObject.Find("LiveLodAnchor").transform.Find(id).gameObject.activeSelf, id + " active again");
        }

        [Test]
        public void DemoField_ShowLabelsOff_DemoMarkersHaveNoText_EvenForLevelsThatShowOne()
        {
            NewOnlyMainCamera();
            var config = LoadShippedConfig();
            var labelled = config.hierarchy_levels.First(l => l.show_label);
            config.demo_field = new DemoFieldSettings
            {
                enabled = true, show_labels = false, dense_clump_count = 0,
                level_counts = new List<DemoFieldLevelCount> { new() { level_key = labelled.key, count = 3 } },
            };
            var session = NewSessionOn(config);
            _created.Add(session.DemoFieldRoot);

            Assert.AreEqual(3, session.SpawnedMarkers.Count);
            foreach (var m in session.SpawnedMarkers)
                Assert.IsFalse(m.transform.Find("Label").gameObject.activeSelf, m.PoiId + ": no label while Show labels is off");
        }

        // ---------------- harness ----------------

        // A camera that IS Camera.main for this test: EditMode tests run inside the open scene, whose own
        // main camera would otherwise be the one the demo field moves to its stage
        private GameObject NewOnlyMainCamera()
        {
            foreach (var other in UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))
                if (other.enabled && other.CompareTag("MainCamera")) { other.enabled = false; _silencedCameras.Add(other); }
            var cam = new GameObject("DemoCam", typeof(Camera)) { tag = "MainCamera" };
            _created.Add(cam);
            Assert.AreSame(cam.GetComponent<Camera>(), Camera.main, "precondition: the test's camera is the main camera");
            return cam;
        }

        private static WallConfigData LoadShippedConfig()
        {
            string path = Path.Combine(Application.streamingAssetsPath, "LivingRoom/config.json");
            var config = JsonUtility.FromJson<WallConfigData>(File.ReadAllText(path));
            Assert.IsNotNull(config, "StreamingAssets/LivingRoom/config.json must load.");
            config.effect_defaults.preview = new EffectDefaults.EffectPreviewSettings();
            config.outline_preview = new OutlinePreviewSettings();
            return config;
        }

        private static WallConfigData Copy(WallConfigData c) => JsonUtility.FromJson<WallConfigData>(JsonUtility.ToJson(c));

        private WallSession NewSessionOn(WallConfigData config)
        {
            var go = new GameObject("LiveLodSession");
            go.SetActive(false);
            _created.Add(go);
            var ws = go.AddComponent<WallSession>();
            var anchor = new GameObject("LiveLodAnchor");
            _created.Add(anchor);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(MarkerPrefabPath);
            Assert.IsNotNull(prefab, "POI_Marker prefab must exist.");
            Set(ws, "_config", config);
            Set(ws, "_effectDefaults", config.effect_defaults);
            Set(ws, "poiAnchorPrefab", prefab);
            Set(ws, "correctionAnchor", anchor.transform);
            typeof(WallSession).GetMethod("SpawnPOIs", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(ws, null);
            return ws;
        }

        private static void Set(object obj, string field, object value) =>
            obj.GetType().GetField(field, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(obj, value);
    }
}

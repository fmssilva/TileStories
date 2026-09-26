using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TestTools;
using TileStories;

namespace TileStories.Tests
{
    // Automated Phase-A (spec 4.4/4.5 Tier 0/0.5) companion to ClusterGalleryHarness:
    // drives the SAME ClusterGalleryDefinitions.Entries list through the real
    // POI_Cluster.prefab + MarkerClusterView code path and asserts visual state
    // programmatically (never eyeballed). Grows automatically as entries are added.
    // Mirrors MarkerGalleryTests' fixture+Spawn/TearDown shape.
    internal static class ClusterGalleryTestFixture
    {
        public const string ClusterPrefabPath = "Assets/Framework/Runtime/UI/Markers/POI_Cluster.prefab";
        public const string IconLibraryPath   = "Assets/Framework/Runtime/UI/Markers/IconLibrary.asset";

        public static GameObject LoadClusterPrefab()
        {
#if UNITY_EDITOR
            var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(ClusterPrefabPath);
            Assert.IsNotNull(prefab, "POI_Cluster.prefab missing at " + ClusterPrefabPath);
            return prefab;
#else
            Assert.Fail("ClusterGalleryTests requires the Unity Editor (AssetDatabase).");
            return null;
#endif
        }

        public const string MarkerPrefabPath = "Assets/Framework/Runtime/UI/Markers/POI_Marker.prefab";

        public static GameObject LoadMarkerPrefab()
        {
#if UNITY_EDITOR
            var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(MarkerPrefabPath);
            Assert.IsNotNull(prefab, "POI_Marker.prefab missing at " + MarkerPrefabPath);
            return prefab;
#else
            Assert.Fail("ClusterGalleryTests requires the Unity Editor (AssetDatabase).");
            return null;
#endif
        }

        public static SpriteKeyLibrary LoadIconLibrary()
        {
#if UNITY_EDITOR
            var lib = UnityEditor.AssetDatabase.LoadAssetAtPath<SpriteKeyLibrary>(IconLibraryPath);
            Assert.IsNotNull(lib, "IconLibrary.asset missing at " + IconLibraryPath);
            return lib;
#else
            Assert.Fail("ClusterGalleryTests requires the Unity Editor (AssetDatabase).");
            return null;
#endif
        }
    }

    public class ClusterGalleryTests
    {
        private readonly List<GameObject> _tracked = new();

        [SetUp]
        public void SetUp()
        {
            CategoryPalette.Configure(ClusterGalleryDefinitions.Overrides);
        }

        [TearDown]
        public void TearDown()
        {
            CategoryPalette.ClearOverrides();
            foreach (var go in _tracked) if (go) Object.DestroyImmediate(go);
            _tracked.Clear();
        }

        private static T GetPrivate<T>(object target, string name)
        {
            var f = target.GetType().GetField(name,
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            Assert.IsNotNull(f, "field '" + name + "' not found on " + target.GetType().Name);
            return (T)f.GetValue(target);
        }

        // Member fabrication with REAL markers: the real POI_Marker prefab, initialised like any POI,
        // so the cluster reads a real symbol size and the category BuildCategoryCounts looks up.
        // Same recipe as ClusterGalleryHarness.FabricateMembers.
        private List<MarkerView> FabricateMembers(ClusterGalleryEntry entry, Transform parent)
        {
            var markerPrefab = ClusterGalleryTestFixture.LoadMarkerPrefab();
            var members = new List<MarkerView>();
            int idx = 0;
            foreach (var cc in entry.CategoryPlan)
            {
                for (int c = 0; c < cc.Count; c++, idx++)
                {
                    var mgo = Object.Instantiate(markerPrefab, parent);
                    mgo.name = "member_" + idx;
                    var anchor = mgo.GetComponent<POIAnchor>() ?? mgo.AddComponent<POIAnchor>();
                    anchor.Initialise(new POIData { id = entry.Label + "_m" + idx, name = cc.Category, category = cc.Category });
                    var view = mgo.GetComponentInChildren<MarkerView>();
                    view.Initialise(anchor, MarkerVisualSettings.Default(), MarkerEffectFlags.None);
                    members.Add(view);
                }
            }
            return members;
        }

        private static int CountDistinctCategories(ClusterGalleryEntry entry)
        {
            var set = new HashSet<string>();
            if (entry.CategoryPlan != null)
                foreach (var cc in entry.CategoryPlan)
                    if (!string.IsNullOrEmpty(cc.Category)) set.Add(cc.Category);
            return set.Count;
        }

        [UnityTest]
        public IEnumerator ClusterGallery_variants_render_correctly()
        {
            var prefab = ClusterGalleryTestFixture.LoadClusterPrefab();
            var lib = ClusterGalleryTestFixture.LoadIconLibrary();

            foreach (var entry in ClusterGalleryDefinitions.Entries)
            {
                var go = Object.Instantiate(prefab);
                go.name = "Cluster_" + entry.Label.Replace(" ", "_").Replace("/", "_").Replace("|", "_");
                _tracked.Add(go);

                var mcv = go.GetComponent<MarkerClusterView>();
                Assert.IsNotNull(mcv, "MarkerClusterView missing on spawned cluster (" + entry.Label + ")");

                var members = FabricateMembers(entry, go.transform);
                var settings = new LodSettings
                {
                    density_response_mode = "cluster",
                    cluster_icon_mode = entry.IconMode,
                };
                mcv.Initialize(members, lib, settings);
                yield return null; // let layout/scale settle

                int n = entry.MemberCount;
                float largestMember = 0f;
                foreach (var m in members) largestMember = Mathf.Max(largestMember, m.SymbolDiameterMetres);
                Assert.Greater(largestMember, 0f, entry.Label + ": precondition, members have a real symbol size");
                float expectedSize = MarkerClusterView.ComputeDiameterMetres(largestMember, settings.cluster_size_ratio, n);

                // 6.5: "+N" count label matches member count.
                // Read via reflection (test asmdef does not reference TMPro) -- same
                // private-field pattern ClusterReconcileTests uses for MarkerClusterView fields.
                var countLabel = GetPrivate<object>(mcv, "countLabel");
                var countTextProp = countLabel.GetType().GetProperty("text");
                Assert.IsNotNull(countTextProp, entry.Label + ": countLabel has no 'text' property");
                Assert.AreEqual("+" + n, countTextProp.GetValue(countLabel),
                    entry.Label + ": count label should be +N");

                // 6.2: a WORLD-space size (metres), from the largest member x cluster_size_ratio,
                // never smaller than that member and never the old 48..112 "px" (= metres) giant.
                var pieRt = GetPrivate<RectTransform>(mcv, "pieContainer");
                Assert.AreEqual(expectedSize, pieRt.sizeDelta.x, 1e-4f,
                    entry.Label + ": cluster size follows the largest member's symbol");
                Assert.GreaterOrEqual(pieRt.sizeDelta.x, largestMember,
                    entry.Label + ": a cluster is never smaller than its largest member");
                Assert.Less(pieRt.sizeDelta.x, 2f,
                    entry.Label + ": a cluster is a marker-sized disc, not tens of metres wide");
                foreach (RectTransform slice in pieRt)
                    Assert.AreEqual(pieRt.rect.width, slice.rect.width, 1e-4f,
                        entry.Label + ": every pie slice fills the pie container exactly");

                // 6.3: DominantIcon active iff dominant_category mode; hidden otherwise.
                Assert.AreEqual(entry.IconMode == "dominant_category",
                    mcv.DominantIcon.gameObject.activeSelf,
                    entry.Label + ": DominantIcon active state by icon mode");

                // 6.1/6.3: pie slices == distinct categories for pie_and_count; 0 otherwise.
                int expectedSlices = entry.IconMode == "pie_and_count" ? CountDistinctCategories(entry) : 0;
                Assert.AreEqual(expectedSlices, pieRt.childCount,
                    entry.Label + ": pie slice count (== distinct categories in pie_and_count, 0 otherwise)");

                if (entry.IconMode == "dominant_category")
                {
                    Assert.IsNotNull(mcv.DominantIcon.sprite,
                        entry.Label + ": dominant_category must resolve an icon sprite end-to-end");
                }

                foreach (var m in members) if (m) Object.DestroyImmediate(m.gameObject);
            }
        }
    }
}
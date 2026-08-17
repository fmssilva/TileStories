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
        // Contract values mirrored from MarkerClusterView (MinSizePx/SizePerMember/MaxSizePx).
        private const float MinSizePx = 48f;
        private const float SizePerMember = 9f;
        private const float MaxSizePx = 112f;

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

        // Lightweight member fabrication: a MarkerView + POIAnchor carries the category
        // that MarkerClusterView.BuildCategoryCounts reads via GetComponentInParent.
        // Mirrors ClusterGalleryHarness.FabricateMembers but self-contained (no scene GO).
        private List<MarkerView> FabricateMembers(ClusterGalleryEntry entry, Transform parent)
        {
            var members = new List<MarkerView>();
            int idx = 0;
            foreach (var cc in entry.CategoryPlan)
            {
                for (int c = 0; c < cc.Count; c++, idx++)
                {
                    var mgo = new GameObject("member_" + idx, typeof(MarkerView), typeof(CanvasGroup));
                    mgo.transform.SetParent(parent, false);
                    var anchor = mgo.AddComponent<POIAnchor>();
                    anchor.Initialise(new POIData { id = entry.Label + "_m" + idx, category = cc.Category });
                    members.Add(mgo.GetComponent<MarkerView>());
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
                float expectedSize = Mathf.Clamp(MinSizePx + n * SizePerMember, MinSizePx, MaxSizePx);

                // 6.5: "+N" count label matches member count.
                // Read via reflection (test asmdef does not reference TMPro) -- same
                // private-field pattern ClusterReconcileTests uses for MarkerClusterView fields.
                var countLabel = GetPrivate<object>(mcv, "countLabel");
                var countTextProp = countLabel.GetType().GetProperty("text");
                Assert.IsNotNull(countTextProp, entry.Label + ": countLabel has no 'text' property");
                Assert.AreEqual("+" + n, countTextProp.GetValue(countLabel),
                    entry.Label + ": count label should be +N");

                // 6.5: tap target >= 44px; 6.2: rect spans 48..112px scaled by member count.
                var pieRt = GetPrivate<RectTransform>(mcv, "pieContainer");
                Assert.IsTrue(pieRt.sizeDelta.x >= MinSizePx,
                    entry.Label + ": cluster rect must be >= MinSizePx (" + MinSizePx + "px)");
                Assert.IsTrue(pieRt.sizeDelta.x <= MaxSizePx,
                    entry.Label + ": cluster rect must be <= MaxSizePx (" + MaxSizePx + "px)");
                Assert.AreEqual(expectedSize, pieRt.sizeDelta.x, 0.5f,
                    entry.Label + ": cluster rect size matches member-count scaling");

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
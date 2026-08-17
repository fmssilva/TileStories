using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using NUnit.Framework;
using TileStories;

namespace TileStories.Tests
{
    // Verifies the ONE shared cluster aggregate asset (POI_Cluster.prefab) -- the contract
    // that LODController's cluster creation will Instantiate in Phase 2 / Block 4.
    // This is a shell-only asset here: no member wiring logic is exercised, only the
    // component/structure contract.
    public class ClusterPrefabTests
    {
        private const string PrefabPath = "Assets/Framework/Runtime/UI/Markers/POI_Cluster.prefab";

        [Test]
        public void ClusterPrefab_declares_required_components()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            Assert.IsNotNull(prefab, "POI_Cluster.prefab missing");

            var canvas = prefab.GetComponentInChildren<Canvas>();
            Assert.IsNotNull(canvas, "cluster needs a Canvas");
            Assert.AreEqual(RenderMode.WorldSpace, canvas.renderMode, "cluster canvas must be world-space (matches POI_Marker.prefab)");

                                    Assert.IsNotNull(prefab.GetComponentInChildren<CanvasGroup>(), "cluster needs a CanvasGroup for alpha/interaction");
            Assert.IsNotNull(prefab.GetComponentInChildren<MarkerClusterView>(), "cluster needs MarkerClusterView");
            Assert.IsNotNull(prefab.GetComponentInChildren<RectTransform>(), "cluster pie container needs a RectTransform");
        }

        [Test]
        public void Initialize_on_empty_members_no_NullRef_and_SetVisible_toggles_canvas_group()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            Assert.IsNotNull(prefab);

            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            try
            {
                var mcv = go.GetComponent<MarkerClusterView>();
                var cg = go.GetComponent<CanvasGroup>();
                Assert.IsNotNull(mcv);
                Assert.IsNotNull(cg);

                var settings = new LodSettings { cluster_icon_mode = "count_only" };
                var lib = ScriptableObject.CreateInstance<SpriteKeyLibrary>();

                // Empty member list must not deref anything; this pins the BuildPie/
                // ResolveAccentColor null-safe paths (Decision 6: empty -> fallback accent).
                Assert.DoesNotThrow(() => mcv.Initialize(new List<MarkerView>(), lib, settings));

                Assert.DoesNotThrow(() => mcv.SetVisible(true));
                Assert.AreEqual(1f, cg.alpha);

                Assert.DoesNotThrow(() => mcv.SetVisible(false));
                Assert.AreEqual(0f, cg.alpha);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        // 3-4 contract: the authoring step (ClusterPrefabWiring) embeds a DominantIcon
        // child with a REAL (non-zero) fileID and leaves it INACTIVE in the asset so the
        // pie/count_only modes never leak it. Runtime-only BuildDominantIcon activates it
        // for dominant_category mode. This is the automated gate for that contract.
        [Test]
        public void DominantIcon_child_is_inactive_in_asset()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            Assert.IsNotNull(prefab, "POI_Cluster.prefab missing at " + PrefabPath);

            var mcv = prefab.GetComponentInChildren<MarkerClusterView>(true);
            Assert.IsNotNull(mcv, "MarkerClusterView missing on cluster prefab");

            // Serialized reference must point at a real persisted child, not fileID:0.
            var so = new SerializedObject(mcv);
            var prop = so.FindProperty("dominantIcon");
            Assert.IsNotNull(prop, "serialized property 'dominantIcon' missing on MarkerClusterView");
            Assert.AreNotEqual(0, prop.objectReferenceInstanceIDValue,
                "dominantIcon reference is fileID:0 -- DominantIcon child was never persisted (re-run 'TileStories/Cluster/Wire DominantIcon')");

            Image domIcon = mcv.DominantIcon;
            Assert.IsNotNull(domIcon, "DominantIcon Image child not wired on MarkerClusterView");
            Assert.IsFalse(domIcon.gameObject.activeSelf,
                "DominantIcon child must be inactive in the asset (active only in dominant_category mode)");
        }

        // 3-4 contract (runtime half): BuildDominantIcon activates the child + resolves its
        // sprite ONLY in dominant_category mode; count_only / pie_and_count must hide it.
        // Uses the REAL IconLibrary.asset + ClusterGalleryDefinitions.Overrides so the
        // category -> icon_key -> sprite chain is exercised end-to-end (religious -> temple -> IconTemple),
        // not a mock.
        [Test]
        public void DominantIcon_only_active_in_dominant_category_mode()
        {
            var iconLibrary = AssetDatabase.LoadAssetAtPath<SpriteKeyLibrary>(
                "Assets/Framework/Runtime/UI/Markers/IconLibrary.asset");
            Assert.IsNotNull(iconLibrary, "IconLibrary.asset missing (expected at Assets/Framework/Runtime/UI/Markers/IconLibrary.asset)");

            CategoryPalette.Configure(ClusterGalleryDefinitions.Overrides);
            try
            {
                // 3 religious members -> dominant category "religious" -> icon_key "temple".
                var members = new List<MarkerView>();
                for (int i = 0; i < 3; i++)
                {
                    var go = new GameObject("domtest_member_" + i, typeof(MarkerView), typeof(CanvasGroup));
                    var anchor = go.AddComponent<POIAnchor>();
                    anchor.Initialise(new POIData { id = "domtest_m" + i, category = "religious" });
                    members.Add(go.GetComponent<MarkerView>());
                }

                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                try
                {
                    var mcv = instance.GetComponent<MarkerClusterView>();

                    var domSettings = new LodSettings { cluster_icon_mode = "dominant_category" };
                    mcv.Initialize(members, iconLibrary, domSettings);
                    Assert.IsTrue(mcv.DominantIcon.gameObject.activeSelf,
                        "DominantIcon must be active in dominant_category mode");
                    Assert.IsNotNull(mcv.DominantIcon.sprite,
                        "DominantIcon sprite must resolve end-to-end (religious -> temple -> IconTemple)");

                    var countSettings = new LodSettings { cluster_icon_mode = "count_only" };
                    mcv.Initialize(members, iconLibrary, countSettings);
                    Assert.IsFalse(mcv.DominantIcon.gameObject.activeSelf,
                        "DominantIcon must stay inactive in count_only mode");

                    var pieSettings = new LodSettings { cluster_icon_mode = "pie_and_count" };
                    mcv.Initialize(members, iconLibrary, pieSettings);
                    Assert.IsFalse(mcv.DominantIcon.gameObject.activeSelf,
                        "DominantIcon must stay inactive in pie_and_count mode");
                }
                finally
                {
                    Object.DestroyImmediate(instance);
                }

                foreach (var m in members) if (m) Object.DestroyImmediate(m.gameObject);
            }
            finally
            {
                CategoryPalette.ClearOverrides();
            }
        }
    }
}

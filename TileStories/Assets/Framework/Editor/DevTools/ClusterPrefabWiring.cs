using UnityEngine;
using UnityEngine.UI;          // Image
using UnityEditor;

namespace TileStories
{
    // Idempotent editor utility that authors the DominantIcon child on
    // POI_Cluster.prefab and wires MarkerClusterView.dominantIcon to it. Keeping
    // the child GameObject inactive in the asset is what lets BuildDominantIcon's
    // mode-gated SetActive(true) (dominant_category only) actually HIDE it in
    // pie_and_count / count_only. Run via "TileStories/Cluster/Wire DominantIcon".
    //
    // This is a one-off authoring action, not runtime code: it has no place in
    // POI_Cluster.prefab's behaviour and is kept here so the wiring can be
    // re-applied idempotently if the prefab is ever re-baked from scratch.
    internal static class ClusterPrefabWiring
    {
        private const string PrefabPath = "Assets/Framework/Runtime/UI/Markers/POI_Cluster.prefab";

        [MenuItem("TileStories/Cluster/Wire DominantIcon")]
        public static void WireDominantIcon()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null)
            {
                Debug.LogError("[ClusterPrefabWiring] POI_Cluster.prefab not found at " + PrefabPath);
                return;
            }

            var mcv = prefab.GetComponentInChildren<MarkerClusterView>(true);
                        if (mcv == null)
            {
                Debug.LogError("[ClusterPrefabWiring] MarkerClusterView not found on prefab");
                return;
            }

            // Idempotency guard (3-4 Decision C): if a previous successful run already
            // embedded a DominantIcon child and assigned a real (non-zero fileID) reference,
            // skip the authoring pass so re-running the menu does not re-parent/re-save.
            // Unity returns null for objectReferenceValue when the reference is fileID:0,
            // which is precisely the "still broken" case we want to (re)repair.
            var guardSo = new SerializedObject(mcv);
            var guardProp = guardSo.FindProperty("dominantIcon");
            if (guardProp != null && guardProp.objectReferenceValue != null)
            {
                Debug.Log("[3-4] already wired (dominantIcon fileID non-zero), skipping");
                return;
            }

            // Edit the prefab via LoadPrefabContents (Unity 6 supported path). Children of
            // the loaded contents root are persisted as REAL sub-assets on
            // SaveAsPrefabAsset, so the DominantIcon child receives a real fileID -- fixing
            // the original bug where AssetDatabase.AddObjectToAsset on a GameObject threw
            // in Unity 6 ("Couldn't add object to asset file because 'DominantIcon' is a
            // GameObject! Use the PrefabUtility class instead"). Ref: Unity docs
            // PrefabUtility.LoadPrefabContents.
            GameObject contentsRoot = PrefabUtility.LoadPrefabContents(PrefabPath);
            mcv = contentsRoot.GetComponentInChildren<MarkerClusterView>(true);
            if (mcv == null)
            {
                Debug.LogError("[ClusterPrefabWiring] MarkerClusterView not found on prefab contents");
                PrefabUtility.UnloadPrefabContents(contentsRoot);
                return;
            }

            // Phase 1: ensure the DominantIcon child exists and is configured. Parenting it
            // under the loaded contents root is what persists it as a real sub-asset on save.
            Transform existing = FindDescendant(mcv.transform, "DominantIcon");
            GameObject domGO = existing != null ? existing.gameObject : null;
            if (domGO == null)
            {
                domGO = new GameObject("DominantIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                domGO.transform.SetParent(mcv.transform, false);
                var rt = domGO.GetComponent<RectTransform>();
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = Vector2.zero;
                rt.sizeDelta = new Vector2(48f, 48f);
                rt.localScale = Vector3.one;
            }

            // Inactive in the asset; MarkerClusterView.BuildDominantIcon flips this on
            // only inside the dominant_category branch.
            domGO.SetActive(false);
            EditorUtility.SetDirty(domGO);
            var img = domGO.GetComponent<Image>();
            if (img == null)
            {
                Debug.LogError("[ClusterPrefabWiring] DominantIcon child has no Image component");
                PrefabUtility.UnloadPrefabContents(contentsRoot);
                return;
            }

            // Phase 2: wire the serialized reference while contents are loaded, then save.
            var so = new SerializedObject(mcv);
            var prop = so.FindProperty("dominantIcon");
            if (prop == null)
            {
                Debug.LogError("[ClusterPrefabWiring] SerializedProperty 'dominantIcon' not found");
                PrefabUtility.UnloadPrefabContents(contentsRoot);
                return;
            }
            prop.objectReferenceValue = img;
            so.ApplyModifiedProperties();
            PrefabUtility.SaveAsPrefabAsset(contentsRoot, PrefabPath);
            PrefabUtility.UnloadPrefabContents(contentsRoot);
            AssetDatabase.Refresh();
            Debug.Log("[3-4] OK: DominantIcon child=" + domGO.name + " activeSelf=" + domGO.activeSelf
                      + " wired to dominantIcon (fileID non-zero)");
        }

        /// <summary>
        /// Recursive whole-prefab descendant search by name. Robust to the child landing at
        /// the root when SetParent onto a Prefab-Asset transform is blocked by Unity.
        /// </summary>
        private static Transform FindDescendant(Transform parent, string name)
        {
            if (parent == null) return null;
            for (int i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                if (child.name == name) return child;
                var found = FindDescendant(child, name);
                if (found != null) return found;
            }
            return null;
        }
}
}

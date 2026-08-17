using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TileStories
{
    // Phase A (spec 4.4) isolated cluster gallery. Instantiates every
    // ClusterGalleryDefinitions.Entries through the real POI_Cluster.prefab +
    // MarkerClusterView path with NO AR/tracking/config.json -- fabricated members
    // only. One data list (ClusterGalleryDefinitions) drives both this harness and
    // ClusterGalleryTests, so the gallery and the asserts can never drift apart.
    // Dev-only (AssetDatabase calls are Editor-guarded); mirrored at runtime by
    // ClusterGalleryTests which drive the same path without AssetDatabase.
    public class ClusterGalleryHarness : MonoBehaviour
    {
        [Header("Assets (Editor-resolved if left null)")]
        [SerializeField] private GameObject poiClusterPrefab;
        [SerializeField] private GameObject poiMarkerPrefab;
        [SerializeField] private SpriteKeyLibrary iconLibrary;

        [Header("Layout (world-space metres)")]
        [SerializeField] private float columnSpacing = 1.7f;
        [SerializeField] private float rowSpacing = 2.2f;
        [SerializeField] private int columns = 3;

        private const string ClusterPrefabPath = "Assets/Framework/Runtime/UI/Markers/POI_Cluster.prefab";
        private const string MarkerPrefabPath  = "Assets/Framework/Runtime/UI/Markers/POI_Marker.prefab";
        private const string IconLibraryPath   = "Assets/Framework/Runtime/UI/Markers/IconLibrary.asset";

        // Configure the shared category palette once so dominant-category icon
        // resolution matches the Authoring Tool / WallSession path (spec 2.2 Marker
        // Design 4-5).
        [ContextMenu("Populate Cluster Gallery")]
        private void Start()
        {
            CategoryPalette.Configure(ClusterGalleryDefinitions.Overrides);
            Populate();
        }

        private void Populate()
        {
            var clusterPrefab = ResolveClusterPrefab();
            var markerPrefab  = ResolveMarkerPrefab();
            var lib = ResolveIconLibrary();
            if (clusterPrefab == null || lib == null)
            {
                Debug.LogError("[ClusterGallery] missing cluster prefab or icon library");
                return;
            }

            int row = 0, col = 0;
            foreach (var entry in ClusterGalleryDefinitions.Entries)
            {
                SpawnEntry(clusterPrefab, markerPrefab, lib, entry, row, col);
                if (++col >= columns) { col = 0; row++; }
            }
            Debug.Log("[ClusterGallery] Spawned " + ClusterGalleryDefinitions.Entries.Count + " entries");
        }

        private void SpawnEntry(GameObject clusterPrefab, GameObject markerPrefab,
                                SpriteKeyLibrary lib, ClusterGalleryEntry entry, int row, int col)
        {
            var go = Instantiate(clusterPrefab, transform);
            go.name = "Cluster_" + Sanitize(entry.Label);
            var mcv = go.GetComponentInChildren<MarkerClusterView>(true);
            if (mcv == null)
            {
                Debug.LogError("[ClusterGallery] MarkerClusterView missing on prefab");
                return;
            }

            var members = FabricateMembers(markerPrefab, entry, go.transform);
            var settings = new LodSettings { density_response_mode = "cluster", cluster_icon_mode = entry.IconMode };
            mcv.Initialize(members, lib, settings);

            Vector3 pos = new Vector3(col * columnSpacing, 0f, -row * rowSpacing);
            mcv.PositionAt(pos, transform);
        }

        // Fabricate lightweight member markers (POIAnchor + MarkerView) under `parent`,
        // one per CategoryCount, mirroring MarkerGalleryTests.Spawn.
        private List<MarkerView> FabricateMembers(GameObject markerPrefab, ClusterGalleryEntry entry, Transform parent)
        {
            var members = new List<MarkerView>();
            int idx = 0;
            foreach (var cc in entry.CategoryPlan)
            {
                for (int c = 0; c < cc.Count; c++, idx++)
                {
                    var mgo = Instantiate(markerPrefab, parent);
                    mgo.name = "member_" + idx;
                    var poiData = new POIData { id = entry.Label + "_m" + idx, name = cc.Category, category = cc.Category };
                    var anchor = mgo.GetComponent<POIAnchor>() ?? mgo.AddComponent<POIAnchor>();
                    anchor.Initialise(poiData);
                    var view = mgo.GetComponentInChildren<MarkerView>();
                    if (view == null) view = mgo.AddComponent<MarkerView>();
                    view.Initialise(anchor, MarkerStyle.OutlineGold, MarkerShape.Circle, MarkerEffectFlags.None);
                    members.Add(view);
                }
            }
            return members;
        }

        private GameObject ResolveClusterPrefab()
        {
            if (poiClusterPrefab != null) return poiClusterPrefab;
#if UNITY_EDITOR
            return AssetDatabase.LoadAssetAtPath<GameObject>(ClusterPrefabPath);
#else
            return null;
#endif
        }

        private GameObject ResolveMarkerPrefab()
        {
            if (poiMarkerPrefab != null) return poiMarkerPrefab;
#if UNITY_EDITOR
            return AssetDatabase.LoadAssetAtPath<GameObject>(MarkerPrefabPath);
#else
            return null;
#endif
        }

        private SpriteKeyLibrary ResolveIconLibrary()
        {
            if (iconLibrary != null) return iconLibrary;
#if UNITY_EDITOR
            return AssetDatabase.LoadAssetAtPath<SpriteKeyLibrary>(IconLibraryPath);
#else
            return null;
#endif
        }

        private static string Sanitize(string s)
        {
            foreach (char c in System.IO.Path.GetInvalidFileNameChars()) s = s.Replace(c.ToString(), "_");
            return s;
        }
    }
}

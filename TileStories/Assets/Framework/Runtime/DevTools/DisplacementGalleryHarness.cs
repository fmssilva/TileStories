using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TileStories
{
    // Phase A (spec 4.4) isolated displacement gallery.
    // Instantiates DisplacementGalleryDefinitions.Entries through the real POI_Marker.prefab
    // and exercises MarkerOverlapResolver.ApplyDisplacement without requiring full AR/tracking.
    // One data list (DisplacementGalleryDefinitions) drives both this harness and DisplacementGalleryTests.
    public class DisplacementGalleryHarness : MonoBehaviour
    {
        [Header("Assets (Editor-resolved if left null)")]
        [SerializeField] private GameObject poiMarkerPrefab;
        [SerializeField] private Camera galleryCamera;

        [Header("Layout (world-space metres)")]
        [SerializeField] private float columnSpacing = 2.0f;
        [SerializeField] private float rowSpacing = 2.5f;
        [SerializeField] private int columns = 2;

        private const string MarkerPrefabPath = "Assets/Framework/Runtime/UI/Markers/POI_Marker.prefab";

        [ContextMenu("Populate Displacement Gallery")]
        private void Start()
        {
            CategoryPalette.Configure(DisplacementGalleryDefinitions.Overrides);
            if (galleryCamera == null) galleryCamera = Camera.main;
            Populate();
        }

        private void Populate()
        {
            var markerPrefab = ResolveMarkerPrefab();
            if (markerPrefab == null)
            {
                Debug.LogError("[DisplacementGallery] missing marker prefab");
                return;
            }

            int row = 0, col = 0;
            foreach (var entry in DisplacementGalleryDefinitions.Entries)
            {
                SpawnEntry(markerPrefab, entry, row, col);
                if (++col >= columns) { col = 0; row++; }
            }
            Debug.Log("[DisplacementGallery] Spawned " + DisplacementGalleryDefinitions.Entries.Count + " entries");
        }

        private void SpawnEntry(GameObject markerPrefab, DisplacementGalleryEntry entry, int row, int col)
        {
            var groupGO = new GameObject("Group_" + Sanitize(entry.Label));
            groupGO.transform.SetParent(transform, false);
            Vector3 centerPos = new Vector3(col * columnSpacing, 0f, -row * rowSpacing);
            groupGO.transform.localPosition = centerPos;

            var spawnedUnits = new List<VisualUnit>();
            for (int i = 0; i < entry.GroupSize; i++)
            {
                var mgo = Instantiate(markerPrefab, groupGO.transform);
                mgo.name = $"Marker_{i}";
                // Slightly clustered around the group center
                mgo.transform.localPosition = new Vector3(Random.Range(-0.01f, 0.01f), Random.Range(-0.01f, 0.01f), 0f);

                var poiData = new POIData
                {
                    id = $"{entry.Label}_poi_{i}",
                    name = $"POI {i}",
                    category = "religious",
                    has_status = false
                };
                var anchor = mgo.GetComponent<POIAnchor>() ?? mgo.AddComponent<POIAnchor>();
                anchor.Initialise(poiData);

                var view = mgo.GetComponentInChildren<MarkerView>();
                if (view == null) view = mgo.AddComponent<MarkerView>();
                view.Initialise(anchor, MarkerVisualSettings.Default(), MarkerEffectFlags.None);

                var reveal = mgo.GetComponent<MarkerRevealEffect>();
                reveal?.SetFullAlphaAndScale();

                spawnedUnits.Add(new VisualUnit
                {
                    marker = view,
                    poiId = poiData.id,
                    worldPosition = mgo.transform.position
                });
            }

            if (galleryCamera != null)
            {
                var settings = new DisplacementSettings
                {
                    enabled = true,
                    overlap_threshold_px = 40f,
                    max_displacement_px = 120f,
                    displace_target = entry.DisplaceTarget,
                    displacement_algorithm = entry.Algorithm,
                    leader_lines_enabled = entry.LeaderLinesEnabled,
                    leader_line_style = entry.LeaderLineStyle,
                    displacement_tiebreak = "symmetric"
                };
                var stab = new Dictionary<string, DisplacementStabilityState>();
                MarkerOverlapResolver.ApplyDisplacement(spawnedUnits, galleryCamera, settings, stab); // section-9 cycle 1: provisional
                MarkerOverlapResolver.ApplyDisplacement(spawnedUnits, galleryCamera, settings, stab); // section-9 cycle 2: commits
            }
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

        private static string Sanitize(string input)
        {
            if (string.IsNullOrEmpty(input)) return "entry";
            return input.Replace(" ", "_").Replace("/", "_");
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TileStories
{
    // Phase A (spec 4.4) isolated orientation gallery. Instantiates
    // OrientationGalleryDefinitions.Entries through the real POI_Marker.prefab and drives
    // each through MarkerBillboard.Configure, so it exercises the real components rather
    // than the resolver in isolation. One data list drives both this harness and
    // OrientationGalleryTests. The camera rig can orbit/pitch/roll (serialized fields), so
    // a human can sweep it by hand and a test can set a pose deterministically -- the
    // existing MarkerGalleryHarness only ever views a flat grid head-on, the one pose at
    // which a billboard is indistinguishable from no billboard.
    public class OrientationGalleryHarness : MonoBehaviour
    {
        [Header("Assets (Editor-resolved if left null)")]
        [SerializeField] private GameObject poiMarkerPrefab;
        [SerializeField] private Camera galleryCamera;

        [Header("Layout (world-space metres)")]
        [SerializeField] private float rowSpacing = 1.0f;

        [Header("Camera rig sweep (degrees) - for hand testing in the editor")]
        [SerializeField] private float orbitYawDeg = 0f;
        [SerializeField] private float orbitPitchDeg = 0f;
        [SerializeField] private float orbitRollDeg = 0f;
        [SerializeField] private float orbitDistance = 3f;

        private const string MarkerPrefabPath = "Assets/Framework/Runtime/UI/Markers/POI_Marker.prefab";

        [ContextMenu("Populate Orientation Gallery")]
        private void Start()
        {
            if (galleryCamera == null) galleryCamera = Camera.main;
            Populate();
            ApplyCameraRig();
        }

        private void Update()
        {
            ApplyCameraRig();
        }

        private void ApplyCameraRig()
        {
            if (galleryCamera == null) return;
            Vector3 center = new Vector3(0f, -rowSpacing * (OrientationGalleryDefinitions.Entries.Count - 1) / 2f, 0f);
            Quaternion orbitRotation = Quaternion.Euler(orbitPitchDeg, orbitYawDeg, 0f);
            galleryCamera.transform.position = center - orbitRotation * Vector3.forward * orbitDistance;
            galleryCamera.transform.rotation = Quaternion.LookRotation(center - galleryCamera.transform.position, Vector3.up) * Quaternion.Euler(0f, 0f, orbitRollDeg);
        }

        private void Populate()
        {
            var markerPrefab = ResolveMarkerPrefab();
            if (markerPrefab == null)
            {
                Debug.LogError("[OrientationGallery] missing marker prefab");
                return;
            }

            int row = 0;
            foreach (var entry in OrientationGalleryDefinitions.Entries)
            {
                SpawnEntry(markerPrefab, entry, row);
                row++;
            }
            Debug.Log("[OrientationGallery] Spawned " + OrientationGalleryDefinitions.Entries.Count + " entries");
        }

        // Instantiates one gallery row and configures it. Exposed (internal) so
        // OrientationGalleryTests can reuse the exact same spawn/configure path instead
        // of duplicating it -- the composition guarantee 40-testing.md 4.2.1 asks for.
        internal static GameObject SpawnEntry(GameObject markerPrefab, OrientationGalleryEntry entry, int row, Transform parent = null)
        {
            var mgo = Object.Instantiate(markerPrefab, parent);
            mgo.name = "Entry_" + Sanitize(entry.Label);
            mgo.transform.localPosition = new Vector3(0f, -row * 1.0f, 0f);

            var poiData = new POIData { id = entry.Label, name = entry.Label, category = "religious" };
            var anchor = mgo.GetComponent<POIAnchor>() ?? mgo.AddComponent<POIAnchor>();
            anchor.Initialise(poiData);

            var view = mgo.GetComponentInChildren<MarkerView>();
            if (view == null) view = mgo.AddComponent<MarkerView>();
            view.Initialise(anchor, MarkerStyle.OutlineGold, MarkerShape.Circle);

            var reveal = mgo.GetComponent<MarkerRevealEffect>();
            if (reveal != null)
            {
                // MarkerView.Initialise above already started the fade/scale-in coroutine
                // (reveal_duration_s > 0 even for the Fallback hierarchy style). Without
                // stopping it first, it keeps running and overwrites this override on the
                // very next frame, making the marker's scale (and therefore every
                // screen-space measurement) depend on frame timing instead of being
                // deterministic - matches the same fix DisplacementGalleryTests.cs already
                // applies for exactly this reason.
                reveal.StopAllCoroutines();
                reveal.SetFullAlphaAndScale();
            }

            // MarkerChildOrientation is baked into POI_Marker.prefab's Label/Badge as of
            // Block 4 -- this defensive add only matters if an older cached prefab
            // instance without it is ever passed in.
            var labelChild = mgo.transform.Find("Label");
            if (labelChild != null && labelChild.GetComponent<MarkerChildOrientation>() == null)
                labelChild.gameObject.AddComponent<MarkerChildOrientation>();
            var badgeChild = mgo.transform.Find("Badge");
            if (badgeChild != null && badgeChild.GetComponent<MarkerChildOrientation>() == null)
                badgeChild.gameObject.AddComponent<MarkerChildOrientation>();

            var settings = new OrientationSettings
            {
                vertical_alignment_mode = entry.VerticalAlignmentMode,
                facing_mode = entry.FacingMode,
                facing_basis = entry.FacingBasis,
                up_reference = entry.UpReference,
                label_vertical_alignment_mode = entry.LabelMode,
                badge_vertical_alignment_mode = entry.BadgeMode,
            };

            var billboard = mgo.GetComponent<MarkerBillboard>();
            if (billboard == null) billboard = mgo.AddComponent<MarkerBillboard>();
            billboard.Configure(settings, "", parent);

            return mgo;
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
            return input.Replace(" ", "_").Replace("/", "_").Replace(".", "_");
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TileStories
{
    // Orchestrates a wall session: load config, wait for localisation, spawn POI anchors.
    // No position-deciding logic of its own — delegates to POIPositionResolver.
    public class WallSession : MonoBehaviour
    {
        [Tooltip("Path inside StreamingAssets, e.g. 'LivingRoom/config.json'")]
        [SerializeField] private string configPath = "LivingRoom/config.json";

        [Tooltip("Prefab with a POIAnchor component. Required for runtime spawn.")]
        [SerializeField] private GameObject poiAnchorPrefab;

        [Tooltip("Parent transform for all spawned POIs (PlacementCorrectionAnchor).")]
        [SerializeField] private Transform correctionAnchor;

        private IWallTracker _tracker;
        private WallConfigData _config;
        private readonly List<GameObject> _spawnedPOIs = new();
        private bool _didSpawn;
        private bool _configLoaded;

        private void Awake()
        {
            _tracker = GetComponent<IWallTracker>();
            if (_tracker == null)
                Debug.LogError("[WallSession] No IWallTracker component found on this GameObject. Add MockLocalizationProvider or ImmersalWallTracker.");

            if (poiAnchorPrefab == null)
                Debug.LogWarning("[WallSession] POI anchor prefab is not assigned. Spawning anchor-only objects until marker view is added.");

            if (correctionAnchor == null)
                Debug.LogWarning("[WallSession] PlacementCorrectionAnchor not assigned. POIs will be parented to this object's transform.");

            StartCoroutine(LoadConfigCoroutine());
        }

        private void OnEnable()
        {
            if (_tracker != null)
                _tracker.OnWallLocalised += HandleWallLocalised;
        }

        private void OnDisable()
        {
            if (_tracker != null)
                _tracker.OnWallLocalised -= HandleWallLocalised;
        }

        private IEnumerator LoadConfigCoroutine()
        {
            yield return WallConfigLoader.LoadFromStreamingAssets(configPath, loaded => _config = loaded);

            if (_config == null)
                yield break;

            _configLoaded = true;
            Debug.Log($"[WallSession] Loaded '{_config.wall_name}' — {_config.pois?.Count ?? 0} POIs.");

            // If tracking already has a lock by the time config finishes, spawn immediately
            if (!_didSpawn && _tracker != null && _tracker.IsLocalised)
            {
                HandleWallLocalised(_tracker.CurrentPose);
            }
        }

        private void HandleWallLocalised(UnityEngine.Pose wallPose)
        {
            if (_didSpawn) return;

            if (!_configLoaded || _config == null || _config.pois == null)
            {
                Debug.Log("[WallSession] Localized before config loaded. Waiting for config...");
                return;
            }

            Debug.Assert(_config.pois.Count > 0, "[WallSession] POI list is empty — nothing to spawn.");
            Debug.Assert(_config.pois.TrueForAll(p => !string.IsNullOrEmpty(p.id)),
                         "[WallSession] One or more POIs have empty ids.");

            Debug.Log($"[WallSession] Wall localised. Spawning {_config.pois.Count} POIs.");
            SpawnPOIs();
            _didSpawn = true;
        }

        private void SpawnPOIs()
        {
            // Resolve calibration anchors once
            CalibrationAnchor[] anchors = _config.calibration_anchors?.ToArray() ?? System.Array.Empty<CalibrationAnchor>();

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Collect spawned MarkerViews for overlap detection
            var spawnedMarkerViews = new List<MarkerView>();

            foreach (var poi in _config.pois)
            {
                // Resolve position via the dedicated resolver (no position logic in this class)
                if (!POIPositionResolver.TryResolvePosition(poi, anchors, out Vector3 localPos))
                {
                    Debug.LogWarning($"[WallSession] Skipping POI '{poi.id}' — position could not be resolved.");
                    continue;
                }

                // Instantiate new marker from prefab (always fresh, no reuse logic)
                var go = poiAnchorPrefab != null
                    ? Instantiate(poiAnchorPrefab, correctionAnchor != null ? correctionAnchor : transform)
                    : CreateAnchorOnlyObject(correctionAnchor != null ? correctionAnchor : transform, poi.id);

                go.transform.localPosition = localPos;
                go.transform.localRotation = Quaternion.identity;
                go.name = poi.id;

                var anchor = go.GetComponent<POIAnchor>() ?? go.AddComponent<POIAnchor>();
                anchor.Initialise(poi);

                // Initialize MarkerView to set the label text from POI data
                var markerView = go.GetComponentInChildren<MarkerView>();
                if (markerView != null)
                {
                    markerView.Initialise(anchor);
                    spawnedMarkerViews.Add(markerView);
                }

                _spawnedPOIs.Add(go);

                var cam = Camera.main;
                if (cam != null)
                {
                    Vector3 worldPos = go.transform.position;
                    var toMarker = (worldPos - cam.transform.position);
                    var distance = toMarker.magnitude;
                    var forwardDot = Vector3.Dot(cam.transform.forward, toMarker.normalized);
                    var inFront = forwardDot > 0f;
                    Debug.Log($"[WallSession] POI ready id={poi.id} localPos={localPos} dist={distance:F2}m inFront={inFront} dot={forwardDot:F3}");
                }
                else
                {
                    Debug.Log($"[WallSession] POI ready id={poi.id} localPos={localPos} (no Camera.main found)");
                }
            }

            // Apply near-overlap detection after all markers are spawned
            MarkerOverlapResolver.ApplyOverlapOffsets(spawnedMarkerViews, Camera.main);

            stopwatch.Stop();
            Debug.Log($"[WallSession] Ready {_spawnedPOIs.Count}/{_config.pois.Count} POIs in {stopwatch.ElapsedMilliseconds}ms.");
        }

        private static GameObject CreateAnchorOnlyObject(Transform parent, string poiId)
        {
            var go = new GameObject($"POI_{poiId}_Anchor");
            go.transform.SetParent(parent);
            return go;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TileStories
{
    /// <summary>
    /// Coordinates a wall session: load config, wait for localisation, spawn POI anchors.
    /// </summary>
    public class WallSession : MonoBehaviour
    {
        [Tooltip("Path inside StreamingAssets, e.g. 'LivingRoom/config.json'")]
        [SerializeField] private string configPath = "LivingRoom/config.json";

        [Tooltip("Prefab with a POIAnchor component. Required for runtime spawn.")]
        [SerializeField] private GameObject poiAnchorPrefab;

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

        private void Start() { }

        private IEnumerator LoadConfigCoroutine()
        {
            yield return WallConfigLoader.LoadFromStreamingAssets(configPath, loaded => _config = loaded);

            if (_config == null)
                yield break;

            _configLoaded = true;
            Debug.Log($"[WallSession] Loaded '{_config.wall_name}' — {_config.pois?.Count ?? 0} POIs.");

            // Keep flow simple: if tracking already has a lock by the time config finishes,
            // spawn immediately from the current pose.
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

            Debug.Log($"[WallSession] Wall localised. Spawning {_config.pois.Count} POIs.");
            SpawnPOIs(wallPose);
            _didSpawn = true;
        }

        private void SpawnPOIs(UnityEngine.Pose wallPose)
        {
            foreach (var poi in _config.pois)
            {
                Vector3 worldPos;

                var hasCapturedPosition = poi.captured_position != null &&
                                          (Mathf.Abs(poi.captured_position.x) > 0.0001f ||
                                           Mathf.Abs(poi.captured_position.y) > 0.0001f ||
                                           Mathf.Abs(poi.captured_position.z) > 0.0001f);

                if (hasCapturedPosition)
                {
                    // Use the exact captured position (offset from wall origin)
                    worldPos = wallPose.position + wallPose.rotation * new Vector3(
                        poi.captured_position.x,
                        poi.captured_position.y,
                        poi.captured_position.z);
                }
                else
                {
                    // Fallback: map x_norm/y_norm onto a 4m × 3m plane in front of the wall
                    const float wallWidth = 4f;
                    const float wallHeight = 3f;
                    var local = new Vector3(
                        (poi.x_norm - 0.5f) * wallWidth,
                        (poi.y_norm - 0.5f) * wallHeight,
                        0.5f);
                    worldPos = wallPose.position + wallPose.rotation * local;
                }

                var go = poiAnchorPrefab != null
                    ? Instantiate(poiAnchorPrefab, worldPos, Quaternion.identity)
                    : CreateAnchorOnlyObject(worldPos, poi.id);

                var anchor = go.GetComponent<POIAnchor>() ?? go.AddComponent<POIAnchor>();
                anchor.Initialise(poi);
                _spawnedPOIs.Add(go);

                var cam = Camera.main;
                if (cam != null)
                {
                    var toMarker = (worldPos - cam.transform.position);
                    var distance = toMarker.magnitude;
                    var forwardDot = Vector3.Dot(cam.transform.forward, toMarker.normalized);
                    var inFront = forwardDot > 0f;
                    Debug.Log($"[WallSession] POI spawned id={poi.id} pos={worldPos} dist={distance:F2}m inFront={inFront} dot={forwardDot:F3}");
                }
                else
                {
                    Debug.Log($"[WallSession] POI spawned id={poi.id} pos={worldPos} (no Camera.main found)");
                }
            }
        }

        private static GameObject CreateAnchorOnlyObject(Vector3 position, string poiId)
        {
            var go = new GameObject($"POI_{poiId}_Anchor");
            go.transform.position = position;
            return go;
        }
    }
}

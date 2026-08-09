using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TileStories
{
    // Orchestrates a wall session: load config, wait for localisation, spawn POI anchors.
    // No position-deciding logic of its own -- delegates to POIPositionResolver.
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
        private EffectDefaults _effectDefaults;
        private readonly List<GameObject> _spawnedPOIs = new();
        private bool _didSpawn;
        private bool _configLoaded;

        // Resolved once from config, then passed to every marker on spawn
        private MarkerShape _markerShape;
        private MarkerShape _badgeShape = MarkerShape.Circle;
        private MarkerOutlineMode _markerOutlineMode;
        private bool _markerUseBadge;
        private bool _hasShapeFromConfig;
        private bool _hasCategoryDefinitions;
        private bool _hasOutlineLevels;
        private SpriteKeyLibrary _wallIconLibrary;

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

            // Runtime reads only authored config and does no visual design fallbacks.
            // Missing/invalid optional fields degrade to no-op visual behavior.
            _hasShapeFromConfig = MarkerVisualsParser.TryParseShape(_config.marker_shape, out _markerShape);
            if (!_hasShapeFromConfig)
                Debug.LogWarning("[WallSession] marker_shape missing/invalid - leaving prefab symbol shape unchanged.");

            // badge_shape is independent of marker_shape (section 20.2). Missing/
            // invalid falls back to Circle, matching the MarkerView default.
            if (!MarkerVisualsParser.TryParseShape(_config.badge_shape, out _badgeShape))
                _badgeShape = MarkerShape.Circle;

            if (!string.IsNullOrWhiteSpace(_config.marker_outline_mode))
            {
                if (!MarkerVisualsParser.TryParseOutlineMode(_config.marker_outline_mode, out _markerOutlineMode))
                {
                    _markerOutlineMode = MarkerOutlineMode.None;
                    Debug.LogWarning("[WallSession] marker_outline_mode missing/invalid - disabling outline at runtime.");
                }

                _markerUseBadge = _config.marker_use_badge;
            }
            else if (MarkerVisualsParser.TryParseStyle(_config.marker_style, out var legacyStyle))
            {
                // Legacy marker_style is still accepted as explicit authored config.
                MarkerVisualsParser.DeriveOutlineAndBadgeFromLegacyStyle(
                    legacyStyle == MarkerStyle.Badge ? "badge" :
                    legacyStyle == MarkerStyle.OutlineSameHue ? "outline_same_hue" : "outline_gold",
                    out _markerOutlineMode,
                    out _markerUseBadge);
            }
            else
            {
                _markerOutlineMode = MarkerOutlineMode.None;
                _markerUseBadge = false;
            }

            _hasCategoryDefinitions = _config.category_styles != null && _config.category_styles.Count > 0;
            if (_hasCategoryDefinitions) CategoryPalette.Configure(_config.category_styles);
            else CategoryPalette.ClearOverrides();

            _wallIconLibrary = null;
            if (!string.IsNullOrWhiteSpace(_config.marker_icon_library_resources_path))
            {
                _wallIconLibrary = Resources.Load<SpriteKeyLibrary>(_config.marker_icon_library_resources_path.Trim());
                if (_wallIconLibrary == null)
                    Debug.LogWarning($"[WallSession] marker_icon_library_resources_path '{_config.marker_icon_library_resources_path}' could not be loaded from Resources. Using prefab default icon library.");
            }

            BadgeCategoryPalette.Configure(_config.badge_categories);

            _hasOutlineLevels = _config.outline_levels != null && _config.outline_levels.Count > 0;
            if (_hasOutlineLevels) StatusRamp.Configure(_config.outline_levels);

            MarkerHierarchyResolver.Configure(_config.hierarchy_levels);

            // Capture effect defaults once; passed to every marker on spawn.
            _effectDefaults = _config.effect_defaults;

            Debug.Log($"[WallSession] Loaded '{_config.wall_name}' -- {_config.pois?.Count ?? 0} POIs.");

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

            Debug.Assert(_config.pois.Count > 0, "[WallSession] POI list is empty -- nothing to spawn.");
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
                    Debug.LogWarning($"[WallSession] Skipping POI '{poi.id}' -- position could not be resolved.");
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

                // Initialize MarkerView with style/shape/effects from config
                var markerView = go.GetComponentInChildren<MarkerView>();
                if (markerView != null)
                {
                    var effects = MarkerEffectFlags.None;
                    markerView.Initialise(
                        anchor,
                        _markerOutlineMode,
                        _markerUseBadge,
                        _markerShape,
                        effects,
                        _hasCategoryDefinitions,
                        _hasShapeFromConfig,
                        _hasOutlineLevels,
                        _wallIconLibrary,
                        _badgeShape,
                        _effectDefaults);
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
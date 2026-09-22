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

        // Resolved from config (RefreshVisualSettings), then passed to every marker
        private MarkerVisualSettings _visualSettings = MarkerVisualSettings.Default();
        private SpriteKeyLibrary _wallIconLibrary;
                private POISearchIndex _searchIndex;

                // Exposed after SpawnPOIs completes so LODController and other systems
        // can enumerate spawned markers without reaching into WallSession internals.
        public IReadOnlyList<MarkerView> SpawnedMarkers { get; private set; } = System.Array.Empty<MarkerView>();

        // Root of the dev-only effects preview grid, or null when it was not spawned.
        public GameObject EffectsPreviewRoot { get; private set; }

        // Root of the dev-only outline preview grid, or null when it was not spawned.
        public GameObject OutlinePreviewRoot { get; private set; }

                // Read-only access to the wall's LOD settings, used by LODController.
        // May be null until config finishes loading in LoadConfigCoroutine.
        public LodSettings LodSettings => _config?.lod_settings;
        // Read-only access to the wall's displacement/overlap settings, consumed by the
        // overlap resolver (Block 1) and the label/marker displacement algorithms (Block 2-4).
        // Mirrors the LodSettings accessor; null only if WallConfigData fails to construct.
        public DisplacementSettings DisplacementSettings => _config?.displacement_settings;
        // Read-only access to the wall's marker/label/badge/cluster orientation settings
        // (_2.1_Marker_Orientation.md section 7), consumed by MarkerBillboard.Configure
        // and by LODController for the cluster spawn path. Mirrors DisplacementSettings.
        public OrientationSettings OrientationSettings => _config?.orientation_settings;

// Read-only access to the wall's resolved icon library + calibrated AR spawn
// root, so the cluster system can place aggregates without reaching into
// WallSession internals (mirrors the SpawnedMarkers pattern, spec §6.1).
public SpriteKeyLibrary WallIconLibrary => _wallIconLibrary;
public Transform MarkerSpawnRoot => correctionAnchor != null ? correctionAnchor : transform;

        // Read-only access to the wall's search index, consumed by SearchOverlayView / ResultsListView / MinimapView.
        // Built once in LoadConfigCoroutine from WallConfigData.pois; null only if WallConfigData fails to construct.
        public POISearchIndex SearchIndex => _searchIndex;

        // Block 2 selection infrastructure (spec _2.6 section 11): highlight/dim
        // and zoom-on-select. Wired once in SpawnPOIs after markers exist,
        // disposed in OnDisable so the static SelectionEventBus holds no
        // stale listeners across scene reloads.
        private SelectionHighlightController _selectionHighlight;
        private ZoomOnSelectController _zoomOnSelect;

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

            // Block 2: release bus subscriptions and restore all markers to full.
            _selectionHighlight?.Dispose();
            _zoomOnSelect?.Dispose();
        }

        private IEnumerator LoadConfigCoroutine()
        {
            yield return WallConfigLoader.LoadFromStreamingAssets(configPath, loaded => _config = loaded);

            if (_config == null)
                yield break;

            _configLoaded = true;

            // Runtime reads only authored config and does no visual design fallbacks.
            // Missing/invalid optional fields degrade to no-op visual behavior.
            RefreshVisualSettings();

            // Build the search index from config POIs (_2.6-al wiring).
            // Created once here, then exposed via SearchIndex for SearchOverlayView / ResultsListView / MinimapView.
            _searchIndex = new POISearchIndex();
            _searchIndex.Build(_config);
            if (_config.synonym_groups != null && _config.synonym_groups.Count > 0)
                _searchIndex.ConfigureWithSynonyms(_config.synonym_groups);

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

        // Build the dev-only effects preview grid from the current config (null when off or not allowed)
        private void SpawnEffectsPreview()
        {
            EffectsPreviewRoot = EffectsPreviewSpawner.TrySpawn(_config, poiAnchorPrefab, Camera.main,
                (view, anchor, style) => view.Initialise(anchor, _visualSettings, MarkerEffectFlags.None, _effectDefaults, style));
        }

        // Build the dev-only outline preview grid from the current config (null when off or not allowed)
        private void SpawnOutlinePreview()
        {
            OutlinePreviewRoot = OutlinePreviewSpawner.TrySpawn(_config, poiAnchorPrefab, Camera.main,
                (view, anchor, style) => view.Initialise(anchor, _visualSettings, MarkerEffectFlags.None, _effectDefaults, style));
        }

        // Keep the config and the static level resolver in step (live Play Mode edits)
        private void ReplaceHierarchyLevels(List<HierarchyLevelEntry> hierarchyLevels)
        {
            if (hierarchyLevels == null) return;
            _config.hierarchy_levels = hierarchyLevels;
            MarkerHierarchyResolver.Configure(hierarchyLevels);
        }

        // Point the palettes at the config, reload the wall icon library, and resolve the wall-level
        // marker look. Called at load, at spawn (so a config injected without loading still spawns
        // exactly as configured) and on every live marker change.
        private void RefreshVisualSettings()
        {
            _wallIconLibrary = null;
            if (!string.IsNullOrWhiteSpace(_config.marker_icon_library_resources_path))
            {
                _wallIconLibrary = Resources.Load<SpriteKeyLibrary>(_config.marker_icon_library_resources_path.Trim());
                if (_wallIconLibrary == null)
                    Debug.LogWarning($"[WallSession] marker_icon_library_resources_path '{_config.marker_icon_library_resources_path}' could not be loaded from Resources. Using prefab default icon library.");
            }

            MarkerVisualSettings.ApplyPalettes(_config);
            _visualSettings = MarkerVisualSettings.Resolve(_config, _wallIconLibrary);
            if (!_visualSettings.HasShapeFromConfig)
                Debug.LogWarning("[WallSession] marker_shape missing/invalid - leaving prefab symbol shape unchanged.");
        }

        // Swap in new marker/badge/outline settings on a running wall: every spawned marker re-applies
        // its visuals (no reveal restart). `source` is the caller's own copy of the config, never the
        // authoring object; its POIs replace each marker's data so per-POI edits show up too.
        public void ApplyMarkerSettings(WallConfigData source)
        {
            if (_config == null || source == null) return;

            _config.marker_shape = source.marker_shape;
            _config.badge_shape = source.badge_shape;
            _config.marker_outline_mode = source.marker_outline_mode;
            _config.outline_uniform_color_hex = source.outline_uniform_color_hex;
            _config.marker_use_badge = source.marker_use_badge;
            _config.badge_corner = source.badge_corner;
            _config.badge_size_ratio = source.badge_size_ratio;
            _config.ring_size_ratio = source.ring_size_ratio;
            _config.contour_spin_deg_per_s = source.contour_spin_deg_per_s;
            _config.marker_icon_library_resources_path = source.marker_icon_library_resources_path;
            _config.category_styles = source.category_styles;
            _config.badge_categories = source.badge_categories;
            _config.outline_levels = source.outline_levels;
            _config.outline_preview = source.outline_preview;
            ReplaceHierarchyLevels(source.hierarchy_levels);
            RefreshVisualSettings();

            foreach (var marker in SpawnedMarkers)
            {
                if (marker == null) continue;
                var anchor = marker.GetComponentInParent<POIAnchor>();
                if (anchor == null) continue;
                var poi = source.pois?.Find(p => p.id == anchor.Data?.id);
                if (poi != null) anchor.Initialise(poi);
                marker.ReapplyVisuals(_visualSettings);
            }

            // The outline demo grid reflects the wall's current levels/colours/mode: rebuild it
            // whenever any of that changes live, same pattern as the effects grid.
            if (OutlinePreviewRoot != null) Destroy(OutlinePreviewRoot);
            OutlinePreviewRoot = null;
            SpawnOutlinePreview();
        }

        // Swap in new orientation settings on a running wall: every spawned marker is re-pointed at
        // them, with its own level's facing override. Clusters are LODController's job (its
        // ReapplyClusterOrientation). The caller passes its own copy, never the authoring object.
        public void ApplyOrientationSettings(OrientationSettings settings, List<HierarchyLevelEntry> hierarchyLevels)
        {
            if (_config == null || settings == null) return;

            _config.orientation_settings = settings;
            ReplaceHierarchyLevels(hierarchyLevels);

            foreach (var marker in SpawnedMarkers)
            {
                if (marker == null) continue;
                var billboard = marker.GetComponentInChildren<MarkerBillboard>();
                var anchor = marker.GetComponentInParent<POIAnchor>();
                if (billboard == null || anchor?.Data == null) continue;
                billboard.ReapplySettings(settings, MarkerHierarchyResolver.ResolveFacingModeOverride(anchor.Data.hierarchy_level_key));
            }
        }

        // Swap in new effect settings on a running wall: every spawned marker re-applies its effects
        // and the preview grid is rebuilt. The caller passes its own copy, never the authoring object.
        public void ApplyEffectSettings(EffectDefaults defaults, List<HierarchyLevelEntry> hierarchyLevels)
        {
            if (_config == null || defaults == null) return;

            _config.effect_defaults = defaults;
            _effectDefaults = defaults;
            ReplaceHierarchyLevels(hierarchyLevels);

            foreach (var marker in SpawnedMarkers)
                if (marker != null) marker.ReapplyEffects(defaults);

            if (EffectsPreviewRoot != null) Destroy(EffectsPreviewRoot);
            EffectsPreviewRoot = null;
            SpawnEffectsPreview();
        }

        private void SpawnPOIs()
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            _effectDefaults = _config.effect_defaults;
            RefreshVisualSettings();

            // Collect spawned MarkerViews for overlap detection
            var spawnedMarkerViews = new List<MarkerView>();

            foreach (var poi in _config.pois)
            {
                // Resolve position via the dedicated resolver (no position logic in this class)
                if (!POIPositionResolver.TryResolvePosition(poi, out Vector3 localPos))
                {
                    Debug.LogWarning($"[WallSession] Skipping POI '{poi.id}' -- position could not be resolved.");
                    continue;
                }

                // Instantiate new marker from prefab (always fresh, no reuse logic)
                var go = poiAnchorPrefab != null
                    ? Instantiate(poiAnchorPrefab, correctionAnchor != null ? correctionAnchor : transform)
                    : CreateAnchorOnlyObject(correctionAnchor != null ? correctionAnchor : transform, poi.id);

                go.transform.localPosition = localPos;
                // The POI's authored editor rotation, so MarkerBillboard.Configure (below)
                // captures it as the "authored" rotation wall_fixed uses fully and yaw_only
                // uses partially (X/Z) at runtime (_2.1_Marker_Orientation.md section 13
                // point 2). always_facing_camera ignores this value entirely, so setting it
                // unconditionally here changes nothing for that facing mode.
                go.transform.localRotation = Quaternion.Euler(poi.editor_rotation_x_deg, poi.editor_rotation_deg, poi.editor_rotation_z_deg);
                go.name = poi.id;

                var anchor = go.GetComponent<POIAnchor>() ?? go.AddComponent<POIAnchor>();
                anchor.Initialise(poi);

                // Initialize MarkerView with style/shape/effects from config
                var markerView = go.GetComponentInChildren<MarkerView>();
                if (markerView != null)
                {
                    markerView.Initialise(anchor, _visualSettings, MarkerEffectFlags.None, _effectDefaults);
                    spawnedMarkerViews.Add(markerView);
                }

                // Orient this marker per the wall's OrientationSettings, next to the
                // MarkerView.Initialise call above (_2.1_Marker_Orientation.md section 13).
                // The level override is read through the same hierarchy-key lookup
                // MarkerView already uses for size/effects/reveal timing.
                var billboard = go.GetComponentInChildren<MarkerBillboard>();
                if (billboard != null)
                {
                    string facingModeOverride = MarkerHierarchyResolver.ResolveFacingModeOverride(poi.hierarchy_level_key);
                    billboard.Configure(_config.orientation_settings, facingModeOverride, MarkerSpawnRoot);
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

                        // Displacement is no longer a one-shot spawn-time step. MarkerOverlapResolver.ApplyDisplacement
            // is invoked per-cycle by LODController (step 8 of the §2.4 pipeline) over the surviving
            // VisualUnits, so screen-space grouping stays consistent with LOD/density state.
            SpawnedMarkers = spawnedMarkerViews;

            stopwatch.Stop();
            Debug.Log($"[WallSession] Ready {_spawnedPOIs.Count}/{_config.pois.Count} POIs in {stopwatch.ElapsedMilliseconds}ms.");

            // Dev-only effects preview grid (effect_defaults.preview; Editor + development builds only).
            // The spawner decides everything; this only tells it how this wall initialises a MarkerView.
            SpawnEffectsPreview();

            // Dev-only outline preview grid (outline_preview; Editor + development builds only), same rule.
            SpawnOutlinePreview();

            // Block 2 selection infrastructure (spec _2.6 section 11). Markers exist
            // now (SpawnedMarkers populated above), so wire the bus-driven highlight
            // and zoom-on-select responders. Both are idempotent against re-entry;
            // config is the resolved wall config. ARZoomController/LODController are
            // scene singletons resolved here (no hard dependency if absent).
            // Master gate (_2.7 entry 2.6-d): a wall that disables the whole
            // Select/Filter/Search domain gets no selection responders at all.
            if (ShouldWireSelectionResponders(_config))
            {
                if (_selectionHighlight == null)
                    _selectionHighlight = new SelectionHighlightController(this, _config);

                var lod = GetComponent<LODController>();
                var zoom = GetComponent<ARZoomController>();
                if (_zoomOnSelect == null && lod != null && zoom != null)
                    _zoomOnSelect = new ZoomOnSelectController(this, _config, zoom, lod);
            }
        }

        // Single decision point for the 2.6-d master toggle: pure and Tier-0 testable,
        // so WallSession itself stays wiring-only (no inline domain policy).
        internal static bool ShouldWireSelectionResponders(WallConfigData config)
        {
            return config == null || config.search_filter_select_enabled;
        }

        private static GameObject CreateAnchorOnlyObject(Transform parent, string poiId)
        {
            var go = new GameObject($"POI_{poiId}_Anchor");
            go.transform.SetParent(parent);
            return go;
        }
    }
}
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
        private FontKeyLibrary _wallFontLibrary;
        // Exposed after SpawnPOIs completes so LODController and other systems
        // can enumerate spawned markers without reaching into WallSession internals.
        public IReadOnlyList<MarkerView> SpawnedMarkers { get; private set; } = System.Array.Empty<MarkerView>();

        // Root of the dev-only effects preview grid, or null when it was not spawned.
        public GameObject EffectsPreviewRoot { get; private set; }

        // Root of the dev-only outline preview grid, or null when it was not spawned.
        public GameObject OutlinePreviewRoot { get; private set; }

        // Root of the dev-only LOD demo field, or null when it was not spawned.
        public GameObject DemoFieldRoot => _demoField.Root;

        // The dev-only LOD demo field: where it lives, its markers, the Editor camera's trip to its stage
        private readonly DemoFieldStage _demoField = new();

        // Root of the dev-only displacement demo, or null when it was not spawned.
        public GameObject DisplacementDemoRoot => _displacementDemo.Root;

        // The dev-only displacement demo, on its own stage
        private readonly DemoFieldStage _displacementDemo = new(DemoFieldStage.DisplacementDemoStagePosition);

        // Root of the dev-only search & filter demo, or null when it was not spawned.
        public GameObject SearchDemoRoot => _searchDemo.Root;

        // The dev-only search & filter demo, on its own stage
        private readonly DemoFieldStage _searchDemo = new(DemoFieldStage.SearchDemoStagePosition);

        // LOD switched off, handed to LODController while the displacement demo pauses LOD. One fixed
        // object: LODController resets itself whenever the settings object it reads changes.
        private readonly LodSettings _lodPausedForDemo = new LodSettings { enabled = false };

        // The wall's own POI markers; SpawnedMarkers is these, or a demo's while one is on.
        private List<MarkerView> _wallMarkers = new();

        // Read-only access to the wall's LOD settings, used by LODController.
        // May be null until config finishes loading in LoadConfigCoroutine.
        public LodSettings LodSettings =>
            (_displacementDemo.IsOn && DisplacementDemoSpawner.PausesLod(_config?.displacement_demo))
            || (_searchDemo.IsOn && SearchDemoSpawner.PausesLod(_config?.search_demo))
                ? _lodPausedForDemo
                : _config?.lod_settings;
        // Read-only access to the wall's AR zoom settings (ARZoomController, ARZoomGestureInput).
        public ZoomSettings ZoomSettings => _config?.zoom_settings;
        // Read-only access to the wall's displacement settings (LODController's step 8 reads them every
        // frame, so a new object from ApplyDisplacementSettings takes effect at once).
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

        // The Select, Filter & Search settings (_2.6), read by the selection responders and the search UI
        public SelectFilterSearchSettings SelectFilterSearch => _config?.select_filter_search;

        // The searchable set: exactly the POIs whose markers run (the wall's own, or a demo's while one
        // is on), so every search result has a marker to show. Rebuilt with the index.
        public IReadOnlyList<POIData> SearchPois => _searchPois;
        private readonly List<POIData> _searchPois = new();

        // The search index over SearchPois (null until the wall has spawned)
        public POISearchIndex SearchIndex { get; private set; }

        // The config the index was built from: the wall's taxonomy and vocabulary with SearchPois as its
        // POIs (plus the demo synonym group while the search demo runs). The search UI reads facets here.
        public WallConfigData SearchConfig { get; private set; }

        // Raised whenever what can be searched changed (spawn, a demo on/off, a live search or marker
        // edit): the search UI rebuilds its facets, minimap and results
        public event System.Action SearchDataChanged;

        // Selection responders (spec _2.6 section 11): the highlight / filter dim and zoom-on-select.
        // Created after spawning, disposed in OnDisable so the static SelectionEventBus keeps no stale
        // listeners across scene reloads.
        private SelectionHighlightController _selectionHighlight;
        private ZoomOnSelectController _zoomOnSelect;

        // The highlight controller (null while the domain is switched off), for the search UI and tests
        public SelectionHighlightController SelectionHighlight => _selectionHighlight;

        // The zoom-on-select responder (null without a zoom controller in the scene)
        public ZoomOnSelectController ZoomOnSelect => _zoomOnSelect;

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

            // release bus subscriptions and restore all markers to full
            _selectionHighlight?.Dispose();
            _selectionHighlight = null;
            _zoomOnSelect?.Dispose();
            _zoomOnSelect = null;
            SelectionEventBus.ResetState();
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

        // Destroy and respawn the effects/hierarchy demo grid, carrying the developer's current
        // camera pan/zoom across the rebuild (2026-09-22) -- without this, EVERY marker/hierarchy
        // edit made while Play Mode runs snapped the grid camera back to its auto-fit framing,
        // since a rebuild gives the grid a brand new DevPreviewCameraDolly at zero. Object.Destroy
        // only schedules destruction for end-of-frame, so reading the outgoing dolly's offset here
        // (before the Destroy call) is still safe.
        private void RebuildEffectsPreview()
        {
            Vector2 pan = Vector2.zero;
            float zoom = 0f;
            if (EffectsPreviewRoot != null)
            {
                var outgoing = EffectsPreviewRoot.GetComponent<EffectsPreviewFocus>();
                if (outgoing != null) { pan = outgoing.Dolly.PanOffsetMetres; zoom = outgoing.Dolly.ZoomOffsetMetres; }
                Destroy(EffectsPreviewRoot);
            }
            EffectsPreviewRoot = null;
            SpawnEffectsPreview();
            EffectsPreviewRoot?.GetComponent<EffectsPreviewFocus>()?.Dolly.SetOffsets(pan, zoom);
        }

        // Same as RebuildEffectsPreview, for the outline demo grid.
        private void RebuildOutlinePreview()
        {
            Vector2 pan = Vector2.zero;
            float zoom = 0f;
            if (OutlinePreviewRoot != null)
            {
                var outgoing = OutlinePreviewRoot.GetComponent<OutlinePreviewFocus>();
                if (outgoing != null) { pan = outgoing.Dolly.PanOffsetMetres; zoom = outgoing.Dolly.ZoomOffsetMetres; }
                Destroy(OutlinePreviewRoot);
            }
            OutlinePreviewRoot = null;
            SpawnOutlinePreview();
            OutlinePreviewRoot?.GetComponent<OutlinePreviewFocus>()?.Dolly.SetOffsets(pan, zoom);
        }

        // (Re)build the dev-only demos (the LOD demo field, the displacement demo) and decide which markers
        // the wall runs (LOD, displacement, selection): a demo's alone while one is on, the wall's own
        // otherwise. LOD forgets every decision about the markers that are replaced.
        private void RebuildDemoField()
        {
            var lod = FindLodController();
            if (lod != null) { lod.RestoreAllMarkers(); lod.ClearDisplacement(); }

            var cam = Camera.main;
            bool fieldOn = DemoFieldSpawner.ShouldSpawn(_config, poiAnchorPrefab, cam);
            bool displacementOn = DisplacementDemoSpawner.ShouldSpawn(_config, poiAnchorPrefab, cam);
            bool searchOn = SearchDemoSpawner.ShouldSpawn(_config, poiAnchorPrefab, cam);
            // - a stage that turns off goes first: it gives the camera back before another stage takes it
            if (!fieldOn) _demoField.Rebuild(false, cam, null);
            if (!displacementOn) _displacementDemo.Rebuild(false, cam, null);
            if (!searchOn) _searchDemo.Rebuild(false, cam, null);
            if (fieldOn) _demoField.Rebuild(true, cam, (pose, markers) =>
                DemoFieldSpawner.Spawn(_config, poiAnchorPrefab, pose, MarkerSpawnRoot, InitialiseDemoMarker, markers));
            if (displacementOn) _displacementDemo.Rebuild(true, cam, (pose, markers) =>
                DisplacementDemoSpawner.Spawn(_config, poiAnchorPrefab, pose, MarkerSpawnRoot, InitialiseDemoMarker, markers));
            if (searchOn) _searchDemo.Rebuild(true, cam, (pose, markers) =>
                SearchDemoSpawner.Spawn(_config, poiAnchorPrefab, pose, MarkerSpawnRoot, InitialiseDemoMarker, markers));

            var demo = _demoField.IsOn ? _demoField : _displacementDemo.IsOn ? _displacementDemo : _searchDemo.IsOn ? _searchDemo : null;
            foreach (var go in _spawnedPOIs)
                if (go != null) go.SetActive(demo == null);
            SpawnedMarkers = demo != null ? new List<MarkerView>(demo.Markers) : _wallMarkers;
            if (lod != null) lod.RestoreAllMarkers();

            // - a new marker set is a new search set: nothing stays selected across the swap
            SelectionEventBus.Clear();
            RebuildSearchIndex();
        }

        private bool AnyDemoOn => _demoField.IsOn || _displacementDemo.IsOn || _searchDemo.IsOn;

        // Rebuild the search index over the running markers' POIs, with the wall's taxonomy and keyword
        // vocabulary, then tell the search UI and the highlight
        private void RebuildSearchIndex()
        {
            if (_config == null) return;

            _searchPois.Clear();
            foreach (var marker in SpawnedMarkers)
            {
                var poi = marker != null ? marker.GetComponentInParent<POIAnchor>()?.Data : null;
                if (poi != null) _searchPois.Add(poi);
            }

            var fields = new List<SearchFieldDefinition>(_config.search_fields ?? new List<SearchFieldDefinition>());
            var synonyms = new List<SynonymGroup>(_config.synonym_groups ?? new List<SynonymGroup>());
            if (_searchDemo.IsOn)
                SearchDemoLayout.AddDemoVocabulary(_config.search_demo, fields, synonyms);

            SearchConfig = new WallConfigData
            {
                wall_id = _config.wall_id,
                category_styles = _config.category_styles,
                badge_categories = _config.badge_categories,
                outline_levels = _config.outline_levels,
                hierarchy_levels = _config.hierarchy_levels,
                search_fields = fields,
                synonym_groups = synonyms,
                select_filter_search = _config.select_filter_search,
                pois = new List<POIData>(_searchPois),
            };
            SearchIndex ??= new POISearchIndex();
            SearchIndex.Build(SearchConfig);

            _selectionHighlight?.SetResultSet(null);
            SearchDataChanged?.Invoke();
        }

        // Swap in new Select, Filter & Search settings and keyword vocabulary on a running wall: the index
        // is rebuilt (each running POI takes its searchable fields -- summary, keywords -- from `source`)
        // and the search UI rebuilds. `source` is the caller's own copy, never the authoring object.
        public void ApplySearchSettings(WallConfigData source)
        {
            if (_config == null || source == null) return;

            _config.select_filter_search = source.select_filter_search ?? new SelectFilterSearchSettings();
            _config.search_fields = source.search_fields;
            _config.synonym_groups = source.synonym_groups;
            foreach (var poi in _config.pois)
            {
                var from = source.pois?.Find(p => p != null && p.id == poi.id);
                if (from == null) continue;
                poi.summary = from.summary;
                poi.search_keywords = from.search_keywords;
                poi.search_keyword_fields = from.search_keyword_fields;
            }

            if (!_config.select_filter_search.enabled)
                SelectionEventBus.Clear();
            UpdateSelectionResponders();
            _selectionHighlight?.Refresh();
            RebuildSearchIndex();
        }

        // Swap in new search demo settings on a running wall: the demo is rebuilt (or removed)
        public void ApplySearchDemo(SearchDemoSettings settings)
        {
            if (_config == null || settings == null) return;
            _config.search_demo = settings;
            RebuildDemoField();
        }

        // How a demo marker is set up: exactly like a wall POI (same visual settings, orientation)
        private void InitialiseDemoMarker(MarkerView view, POIAnchor anchor, HierarchyStyle? styleOverride)
        {
            view.Initialise(anchor, _visualSettings, MarkerEffectFlags.None, _effectDefaults, styleOverride);
            ConfigureBillboard(view.gameObject, anchor.Data.hierarchy_level_key);
        }

        // The scene's LOD pipeline: on this object in a wall scene, anywhere as a fallback
        private LODController FindLodController()
        {
            var lod = GetComponent<LODController>();
            if (lod == null) lod = FindFirstObjectByType<LODController>();
            return lod;
        }

        // Swap in new LOD settings on a running wall. LODController notices the new object on its
        // next frame, forgets every band/crowding/cluster decision and evaluates with the new rules.
        public void ApplyLodSettings(LodSettings settings)
        {
            if (_config == null || settings == null) return;
            _config.lod_settings = settings;
        }

        // Swap in new zoom settings on a running wall (ARZoomController reads them every frame)
        public void ApplyZoomSettings(ZoomSettings settings)
        {
            if (_config == null || settings == null) return;
            _config.zoom_settings = settings;
        }

        // Swap in new displacement settings on a running wall. LODController notices the new object on its
        // next frame, puts every marker back where it belongs and displaces with the new rules.
        public void ApplyDisplacementSettings(DisplacementSettings settings)
        {
            if (_config == null || settings == null) return;
            _config.displacement_settings = settings;
        }

        // Swap in new displacement demo settings on a running wall: the demo is rebuilt (or removed)
        public void ApplyDisplacementDemo(DisplacementDemoSettings settings)
        {
            if (_config == null || settings == null) return;
            _config.displacement_demo = settings;
            RebuildDemoField();
        }

        // Swap in new demo field settings on a running wall: the field is rebuilt (or removed)
        public void ApplyDemoField(DemoFieldSettings settings)
        {
            if (_config == null || settings == null) return;
            _config.demo_field = settings;
            RebuildDemoField();
        }

        // Orient one marker per the wall's OrientationSettings and its level's facing override
        private void ConfigureBillboard(GameObject marker, string hierarchyLevelKey)
        {
            var billboard = marker.GetComponentInChildren<MarkerBillboard>();
            if (billboard == null) return;
            string facingModeOverride = MarkerHierarchyResolver.ResolveFacingModeOverride(hierarchyLevelKey);
            billboard.Configure(_config.orientation_settings, facingModeOverride, MarkerSpawnRoot);
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

            _wallFontLibrary = null;
            if (!string.IsNullOrWhiteSpace(_config.label_font_library_resources_path))
            {
                _wallFontLibrary = Resources.Load<FontKeyLibrary>(_config.label_font_library_resources_path.Trim());
                if (_wallFontLibrary == null)
                    Debug.LogWarning($"[WallSession] label_font_library_resources_path '{_config.label_font_library_resources_path}' could not be loaded from Resources. Using prefab default font library.");
            }

            MarkerVisualSettings.ApplyPalettes(_config);
            _visualSettings = MarkerVisualSettings.Resolve(_config, _wallIconLibrary, _wallFontLibrary);
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
            _config.icon_color_hex = source.icon_color_hex;
            _config.icon_size_ratio = source.icon_size_ratio;
            _config.label_gap_ratio = source.label_gap_ratio;
            _config.label_font_size_ratio = source.label_font_size_ratio;
            _config.label_font_key = source.label_font_key;
            _config.label_font_library_resources_path = source.label_font_library_resources_path;
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
            RebuildOutlinePreview();

            // The effects/hierarchy demo grid's per-level cells are built from a HierarchyStyle
            // (size_cm, show_label, rotate_contour, reveal delay/duration) that THIS applier owns,
            // not LivePlayModeEffectsApplier's -- ApplyEffectSettings only rebuilds it on an
            // effect_defaults or Ripple/Halo/Pulse column edit. Without this, a live Size/Text
            // Label/Spin Ring/Reveal edit reaches every REAL marker (via SpawnedMarkers above) but
            // leaves the demo grid showing stale values, which is exactly what "Add Hierarchy demo
            // grid" exists to preview (_2.3_Marker_Hierarchy.md section 8).
            RebuildEffectsPreview();

            // The demo field's markers copy each level's look at spawn: rebuild so they follow too.
            if (AnyDemoOn) RebuildDemoField();
            // Names, taxonomy labels and taxonomy keywords are searchable: re-index
            else RebuildSearchIndex();
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

        // Swap in each spawned POI's authored facing (Facing X/Y/Z) on a running wall. Only the three
        // editor_rotation fields are read from `pois` (the caller's own copy); wall_fixed markers turn
        // to the new angle, yaw_only ones take its X/Z tilt, always_facing_camera ignores it.
        public void ApplyPoiFacing(List<POIData> pois)
        {
            if (_config == null || pois == null) return;

            foreach (var marker in SpawnedMarkers)
            {
                if (marker == null) continue;
                var anchor = marker.GetComponentInParent<POIAnchor>();
                var source = anchor?.Data == null ? null : pois.Find(p => p != null && p.id == anchor.Data.id);
                var billboard = marker.GetComponentInChildren<MarkerBillboard>();
                if (source == null || billboard == null) continue;

                anchor.Data.editor_rotation_x_deg = source.editor_rotation_x_deg;
                anchor.Data.editor_rotation_deg = source.editor_rotation_deg;
                anchor.Data.editor_rotation_z_deg = source.editor_rotation_z_deg;
                billboard.SetAuthoredLocalRotation(AuthoredRotationOf(source));
            }
        }

        // A POI's authored facing as a local rotation: the one formula spawn and live edits share
        private static Quaternion AuthoredRotationOf(POIData poi) =>
            Quaternion.Euler(poi.editor_rotation_x_deg, poi.editor_rotation_deg, poi.editor_rotation_z_deg);

        // Swap which hierarchy level each spawned POI uses, on a running wall: size, label, label
        // style, effects and facing all follow the new level at once (no reveal restart). Only
        // hierarchy_level_key is read from `pois` (the caller's own copy); every other POI field keeps
        // its own domain's live path. Re-applies every matched marker, not only changed ones, so the
        // result never depends on which other live applier ran first in the same push.
        public void ApplyPoiHierarchyLevels(List<POIData> pois)
        {
            if (_config == null || pois == null) return;

            foreach (var marker in SpawnedMarkers)
            {
                if (marker == null) continue;
                var anchor = marker.GetComponentInParent<POIAnchor>();
                var source = anchor?.Data == null ? null : pois.Find(p => p != null && p.id == anchor.Data.id);
                if (source == null) continue;

                anchor.Data.hierarchy_level_key = source.hierarchy_level_key;
                marker.ReapplyVisuals(_visualSettings);
                var billboard = marker.GetComponentInChildren<MarkerBillboard>();
                if (billboard != null)
                    billboard.ReapplySettings(_config.orientation_settings,
                        MarkerHierarchyResolver.ResolveFacingModeOverride(source.hierarchy_level_key));
            }
            // A POI's level is searchable and filterable: re-index
            RebuildSearchIndex();
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

            RebuildEffectsPreview();
            if (AnyDemoOn) RebuildDemoField();
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
                go.transform.localRotation = AuthoredRotationOf(poi);
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
                ConfigureBillboard(go, poi.hierarchy_level_key);

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
            _wallMarkers = spawnedMarkerViews;
            SpawnedMarkers = spawnedMarkerViews;

            // Selection responders first: rebuilding the demos rebuilds the search index, which tells the
            // search UI -- the highlight must already be there to take its result sets
            SelectionEventBus.ResetState();
            UpdateSelectionResponders();

            // Dev-only LOD demo field (demo_field; Editor + development builds only): while it is on,
            // its markers replace the wall's own in SpawnedMarkers.
            RebuildDemoField();

            stopwatch.Stop();
            Debug.Log($"[WallSession] Ready {_spawnedPOIs.Count}/{_config.pois.Count} POIs in {stopwatch.ElapsedMilliseconds}ms.");

            // Dev-only effects preview grid (effect_defaults.preview; Editor + development builds only).
            // The spawner decides everything; this only tells it how this wall initialises a MarkerView.
            SpawnEffectsPreview();

            // Dev-only outline preview grid (outline_preview; Editor + development builds only), same rule.
            SpawnOutlinePreview();

        }

        // Create the selection responders while the Select, Filter & Search domain is on, dispose them
        // while it is off (the master switch, also live)
        private void UpdateSelectionResponders()
        {
            if (!ShouldWireSelectionResponders(_config))
            {
                _selectionHighlight?.Dispose();
                _selectionHighlight = null;
                _zoomOnSelect?.Dispose();
                _zoomOnSelect = null;
                return;
            }

            _selectionHighlight ??= new SelectionHighlightController(this);
            if (_zoomOnSelect == null)
            {
                // - scene singleton on its own rig (explicit != null, never ??: a missing component can be Unity's "fake null")
                var zoom = GetComponent<ARZoomController>();
                if (zoom == null) zoom = FindFirstObjectByType<ARZoomController>();
                if (zoom != null) _zoomOnSelect = new ZoomOnSelectController(this, zoom);
            }
        }

        // The master switch: pure so the rule is testable without a scene
        internal static bool ShouldWireSelectionResponders(WallConfigData config) =>
            config?.select_filter_search == null || config.select_filter_search.enabled;

        private static GameObject CreateAnchorOnlyObject(Transform parent, string poiId)
        {
            var go = new GameObject($"POI_{poiId}_Anchor");
            go.transform.SetParent(parent);
            return go;
        }
    }
}
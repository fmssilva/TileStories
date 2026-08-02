using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TileStories.Editor
{
    // Editor window for POI marker placement + wall-level marker visual authoring.
    // Key behavior:
    // - Config JSON is edited in-memory, then written explicitly by Save.
    // - Scene rig mutations use Unity Undo (objects/components).
    // - Config data mutations use a local snapshot history for Ctrl+Z / Ctrl+Y.
    public class POIAuthoringToolWindow : EditorWindow
    {
        private static readonly string[] OutlineModeOptions = { "gold", "same_hue" };
        private static readonly string[] OutlineModeLabels = { "Gold", "Same Hue" };
        private static readonly string[] LineStyleOptions = { "solid", "dash_long", "dash_medium", "dash_short", "dotted" };
        private static readonly string[] LineStyleLabels = { "Continuous", "Big Dashed", "Medium Dashed", "Small Dashed", "Dots" };
        private static readonly string[] ShapeOptions = { "circle", "rounded_square", "hexagon", "diamond", "star" };

        private const string DefaultConfigPath = "Assets/Apps/LivingRoom/config.json";
        private const string DefaultStreamingConfigPath = "Assets/StreamingAssets/LivingRoom/config.json";
        private const string DefaultPrefabPath = "Assets/Framework/Runtime/UI/Markers/POI_Marker.prefab";
        private const string DefaultIconLibraryPath = "Assets/Framework/Runtime/UI/Markers/IconLibrary.asset";
        private const float SyncPositionTolerance = 0.001f;
        private const int MaxOutlineLevels = 6;

        [SerializeField] private WallConfigData _config;
        [SerializeField] private Transform _correctionAnchor;
        [SerializeField] private GameObject _wallMesh;
        [SerializeField] private string _configPath = DefaultConfigPath;
        [SerializeField] private string _streamingConfigPath = DefaultStreamingConfigPath;
        [SerializeField] private string _prefabPath = DefaultPrefabPath;
        [SerializeField] private Vector2 _scrollPos;

        [SerializeField] private bool _showGlobalSceneOptions = true;
        [SerializeField] private bool _showSpecificMarkerOptions = true;
        [SerializeField] private bool _showGlobalMarker = true;
        [SerializeField] private bool _showGlobalBadge = true;
        [SerializeField] private bool _showGlobalOutline = true;
        [SerializeField] private bool _showPoiPosition = true;
        [SerializeField] private bool _showPoiMarkerStyle = true;
        [SerializeField] private bool _showPoiBadgeStyle = true;
        [SerializeField] private bool _showPoiOutline = true;
        [SerializeField] private bool _showPoiEffects = true;

        [SerializeField] private SpriteKeyLibrary _defaultIconLibrary;
        [SerializeField] private SpriteKeyLibrary _wallIconLibrary;
        [SerializeField] private bool _hasUnsavedChanges;

        private readonly Dictionary<string, bool> _poiFoldouts = new();
        private readonly List<string> _configHistory = new();
        private int _configHistoryIndex = -1;
        private bool _isApplyingHistory;

        [MenuItem("TileStories/POI Authoring Tool #P")]
        private static void ShowWindow()
        {
            var w = GetWindow<POIAuthoringToolWindow>();
            w.titleContent = new GUIContent("POI Authoring");
            w.Show();
        }

        private void OnEnable()
        {
            TryResolveSceneReferences();
            EnsureDefaultIconLibraryLoaded();
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private void OnGUI()
        {
            HandleUndoShortcuts();

            DrawToolbar();
            DrawTopConfigAndActions();
            DrawSyncAndWarnings();

            if (_config == null)
            {
                EditorGUILayout.HelpBox("No config loaded. Click Load Config.", MessageType.Info);
                return;
            }

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            _showGlobalSceneOptions = EditorGUILayout.Foldout(_showGlobalSceneOptions, "Global Scene Options", true);
            if (_showGlobalSceneOptions)
            {
                DrawConfigMutationScope(DrawGlobalSceneOptions, refreshRigOnChange: true);
            }

            EditorGUILayout.Space(8f);

            _showSpecificMarkerOptions = EditorGUILayout.Foldout(_showSpecificMarkerOptions, "Specific Marker Options", true);
            if (_showSpecificMarkerOptions)
            {
                DrawConfigMutationScope(DrawSpecificMarkerOptions, refreshRigOnChange: true);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawToolbar()
        {
            GUILayout.Label("POI Authoring Tool", EditorStyles.boldLabel);
            EditorGUILayout.Space(4f);
        }

        private void DrawTopConfigAndActions()
        {
            TryResolveSceneReferences();

            DrawPathRow("Config path", ref _configPath, "json");
            DrawPathRow("Streaming path", ref _streamingConfigPath, "json");
            DrawPathRow("Marker prefab", ref _prefabPath, "prefab");

            _correctionAnchor = (Transform)EditorGUILayout.ObjectField("Correction anchor", _correctionAnchor, typeof(Transform), true);
            _wallMesh = (GameObject)EditorGUILayout.ObjectField("Wall mesh (reference)", _wallMesh, typeof(GameObject), true);

            EditorGUILayout.Space(6f);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Load Config", GUILayout.Height(26f)))
                    LoadConfig();

                if (GUILayout.Button("Populate Rig from JSON", GUILayout.Height(26f)))
                    PopulateRig();

                bool hasRigChildren = GetRigChildCount() > 0;
                var previousColor = GUI.color;
                if (hasRigChildren)
                    GUI.color = new Color(0.95f, 0.55f, 0.25f);

                if (GUILayout.Button("Clear Rig", GUILayout.Height(26f)))
                    ClearRig();

                GUI.color = previousColor;
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                var previousColor = GUI.color;
                if (_hasUnsavedChanges)
                    GUI.color = new Color(0.96f, 0.78f, 0.25f);

                if (GUILayout.Button("Save All to JSON", GUILayout.Height(26f)))
                    SaveAllToJson();

                GUI.color = previousColor;

                if (GUILayout.Button("Copy to StreamingAssets", GUILayout.Height(26f)))
                    CopyToStreamingAssets();

                using (new EditorGUI.DisabledScope(!CanUndoConfigChange()))
                {
                    if (GUILayout.Button("Undo", GUILayout.Height(26f), GUILayout.Width(85f)))
                        UndoConfigChange();
                }

                using (new EditorGUI.DisabledScope(!CanRedoConfigChange()))
                {
                    if (GUILayout.Button("Redo", GUILayout.Height(26f), GUILayout.Width(85f)))
                        RedoConfigChange();
                }
            }
        }

        private void DrawSyncAndWarnings()
        {
            if (_correctionAnchor == null)
            {
                EditorGUILayout.HelpBox(
                    "Populate/Capture need PlacementCorrectionAnchor. Assign it manually if auto-find fails.",
                    MessageType.Warning);
            }

            int rigCount = GetRigChildCount();
            if (rigCount > 0)
            {
                EditorGUILayout.HelpBox(
                    "Rig currently contains generated markers. Clear Rig before final play/runtime checks to avoid duplicates.",
                    MessageType.Warning);
            }

            if (_hasUnsavedChanges)
            {
                EditorGUILayout.HelpBox(
                    "Config has unsaved changes. Use Save All to JSON to persist category/badge/outline/marker edits.",
                    MessageType.Warning);
            }

            Transform rig = GetExistingRig();
            if (rig != null && rig.childCount > 0)
            {
                bool inSync = IsRigInSyncWithConfig(out int outOfSyncCount);
                var originalColor = GUI.color;
                GUI.color = inSync ? Color.green : Color.red;
                string label = inSync
                    ? "Rig matches config.json"
                    : $"Rig OUT OF SYNC - {outOfSyncCount} marker(s) differ from config.json";
                EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
                GUI.color = originalColor;
            }

            EditorGUILayout.Space(8f);
        }

        private void DrawGlobalSceneOptions()
        {
            _showGlobalMarker = EditorGUILayout.Foldout(_showGlobalMarker, "Marker", true);
            if (_showGlobalMarker)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    DrawMarkerGlobalSection();
                }
            }

            EditorGUILayout.Space(4f);

            _showGlobalBadge = EditorGUILayout.Foldout(_showGlobalBadge, "Badge", true);
            if (_showGlobalBadge)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    _config.marker_use_badge = EditorGUILayout.Toggle("Enable badge", _config.marker_use_badge);
                    if (_config.marker_use_badge)
                        DrawGlobalBadgeSection();
                    else
                        EditorGUILayout.HelpBox("Enable badge to edit badge symbol taxonomy.", MessageType.Info);
                }
            }

            EditorGUILayout.Space(4f);

            _showGlobalOutline = EditorGUILayout.Foldout(_showGlobalOutline, "Outline", true);
            if (_showGlobalOutline)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    DrawGlobalOutlineSection();
                }
            }
        }

        private void DrawMarkerGlobalSection()
        {
            int shapeIdx = Array.IndexOf(ShapeOptions, _config.marker_shape);
            if (shapeIdx < 0) shapeIdx = 0;
            shapeIdx = EditorGUILayout.Popup("Marker shape", shapeIdx, ShapeOptions);
            _config.marker_shape = ShapeOptions[shapeIdx];

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Category Symbols", EditorStyles.boldLabel);

            DrawWallIconLibrarySelector();

            if (_config.category_styles == null)
                _config.category_styles = new List<CategoryStyleEntry>();

            if (_config.category_styles.Count == 0)
            {
                if (GUILayout.Button("Seed categories from current POIs"))
                {
                    var seen = new HashSet<string>();
                    foreach (var poi in _config.pois)
                    {
                        if (poi == null || string.IsNullOrWhiteSpace(poi.category) || !seen.Add(poi.category))
                            continue;

                        _config.category_styles.Add(new CategoryStyleEntry
                        {
                            category = poi.category,
                            icon_key = "unknown",
                            color_hex = string.Empty
                        });
                    }
                }
            }

            for (int i = 0; i < _config.category_styles.Count; i++)
            {
                var entry = _config.category_styles[i] ?? new CategoryStyleEntry();
                using (new EditorGUILayout.HorizontalScope())
                {
                    entry.category = EditorGUILayout.TextField(entry.category, GUILayout.MinWidth(130f));

                    Sprite current = ResolveSpriteForKey(entry.icon_key);
                    Sprite chosen = (Sprite)EditorGUILayout.ObjectField(current, typeof(Sprite), false, GUILayout.Width(140f));
                    if (chosen != current)
                        entry.icon_key = AssignSpriteToLibraryAndGetKey(chosen, entry.category);

                    string symbolName = chosen != null ? chosen.name : (current != null ? current.name : "(none)");
                    EditorGUILayout.LabelField(symbolName, GUILayout.MinWidth(120f));
                    DrawSpritePreview(chosen != null ? chosen : current);
                    DrawColorHexCompact(ref entry.color_hex);

                    if (GUILayout.Button("-", GUILayout.Width(28f)))
                    {
                        _config.category_styles.RemoveAt(i);
                        i--;
                        continue;
                    }
                }

                _config.category_styles[i] = entry;
            }

            if (GUILayout.Button("+ Add category"))
            {
                _config.category_styles.Add(new CategoryStyleEntry
                {
                    category = "new_category",
                    icon_key = "unknown",
                    color_hex = string.Empty
                });
            }
        }

        private void DrawGlobalBadgeSection()
        {
            if (_config.badge_categories == null)
                _config.badge_categories = new List<BadgeCategoryEntry>();

            if (_config.badge_categories.Count == 0)
            {
                if (GUILayout.Button("Seed sample badge categories"))
                {
                    _config.badge_categories.Add(new BadgeCategoryEntry { key = "info", label = "Info", icon_key = "unknown", color_hex = "#9FB3C8" });
                    _config.badge_categories.Add(new BadgeCategoryEntry { key = "warning", label = "Warning", icon_key = "unknown", color_hex = "#C8A66A" });
                    _config.badge_categories.Add(new BadgeCategoryEntry { key = "danger", label = "Danger", icon_key = "unknown", color_hex = "#B57373" });
                }
            }

            for (int i = 0; i < _config.badge_categories.Count; i++)
            {
                var entry = _config.badge_categories[i] ?? new BadgeCategoryEntry();
                using (new EditorGUILayout.HorizontalScope())
                {
                    entry.key = EditorGUILayout.TextField(entry.key, GUILayout.MinWidth(90f));
                    entry.label = EditorGUILayout.TextField(entry.label, GUILayout.MinWidth(110f));

                    Sprite current = ResolveSpriteForKey(entry.icon_key);
                    Sprite chosen = (Sprite)EditorGUILayout.ObjectField(current, typeof(Sprite), false, GUILayout.Width(140f));
                    if (chosen != current)
                        entry.icon_key = AssignSpriteToLibraryAndGetKey(chosen, string.IsNullOrWhiteSpace(entry.key) ? entry.label : entry.key);

                    string symbolName = chosen != null ? chosen.name : (current != null ? current.name : "(none)");
                    EditorGUILayout.LabelField(symbolName, GUILayout.MinWidth(120f));
                    DrawSpritePreview(chosen != null ? chosen : current);
                    DrawColorHexCompact(ref entry.color_hex);

                    if (GUILayout.Button("-", GUILayout.Width(28f)))
                    {
                        _config.badge_categories.RemoveAt(i);
                        i--;
                        continue;
                    }
                }

                _config.badge_categories[i] = entry;
            }

            if (GUILayout.Button("+ Add badge category"))
            {
                _config.badge_categories.Add(new BadgeCategoryEntry
                {
                    key = "new_badge",
                    label = "New Badge",
                    icon_key = "unknown",
                    color_hex = "#B3B3B3"
                });
            }
        }

        private void DrawGlobalOutlineSection()
        {
            bool useOutline = !string.Equals(_config.marker_outline_mode, "none", StringComparison.OrdinalIgnoreCase);
            useOutline = EditorGUILayout.Toggle("Enable outline", useOutline);

            if (!useOutline)
            {
                _config.marker_outline_mode = "none";
                EditorGUILayout.HelpBox("Outline disabled. Outline levels are ignored at runtime.", MessageType.Info);
                return;
            }

            string normalizedOutlineMode = MarkerVisualsParser.TryParseOutlineMode(_config.marker_outline_mode, out var parsedOutlineMode)
                ? (parsedOutlineMode == MarkerOutlineMode.SameHue ? "same_hue" : parsedOutlineMode == MarkerOutlineMode.Gold ? "gold" : "gold")
                : "gold";
            int idx = Array.IndexOf(OutlineModeOptions, normalizedOutlineMode);
            if (idx < 0) idx = 0;
            idx = EditorGUILayout.Popup("Outline mode", idx, OutlineModeLabels);
            _config.marker_outline_mode = OutlineModeOptions[idx];

            if (_config.outline_levels == null)
                _config.outline_levels = new List<OutlineLevelEntry>();

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Outline levels", EditorStyles.boldLabel);

            if (_config.outline_levels.Count == 0)
            {
                EditorGUILayout.HelpBox("No levels configured. Runtime will suppress status visuals until levels exist.", MessageType.Warning);
                if (GUILayout.Button("Seed default outline levels"))
                {
                    for (int i = 0; i < StatusRamp.Levels.Length; i++)
                    {
                        var level = StatusRamp.Levels[i];
                        _config.outline_levels.Add(new OutlineLevelEntry
                        {
                            key = "lvl_" + i,
                            label = level.Pct.ToString("0") + "%",
                            pct = level.Pct,
                            line_style = level.RingSpriteKey,
                            color_hex = string.Empty,
                            ring_width = level.RingWidth
                        });
                    }
                }
            }

            for (int i = 0; i < _config.outline_levels.Count; i++)
            {
                var entry = _config.outline_levels[i] ?? new OutlineLevelEntry();
                using (new EditorGUILayout.HorizontalScope())
                {
                    entry.label = EditorGUILayout.TextField(entry.label, GUILayout.MinWidth(120f));
                    entry.pct = EditorGUILayout.Slider(entry.pct, 0f, 100f, GUILayout.MinWidth(140f));

                    int styleIndex = Array.IndexOf(LineStyleOptions, MarkerVisualsParser.NormalizeLineStyle(entry.line_style));
                    if (styleIndex < 0) styleIndex = 0;
                    styleIndex = EditorGUILayout.Popup(styleIndex, LineStyleLabels, GUILayout.Width(115f));
                    entry.line_style = LineStyleOptions[styleIndex];

                    if (GUILayout.Button("-", GUILayout.Width(28f)))
                    {
                        _config.outline_levels.RemoveAt(i);
                        i--;
                        continue;
                    }
                }

                entry.key = string.IsNullOrWhiteSpace(entry.key) ? $"level_{i + 1}" : entry.key;
                _config.outline_levels[i] = entry;
            }

            bool canAdd = _config.outline_levels.Count < Mathf.Max(MaxOutlineLevels, LineStyleOptions.Length + 1);
            using (new EditorGUI.DisabledScope(!canAdd))
            {
                if (GUILayout.Button("+ Add outline level"))
                {
                    _config.outline_levels.Add(new OutlineLevelEntry
                    {
                        key = "level_" + (_config.outline_levels.Count + 1),
                        label = "Level " + (_config.outline_levels.Count + 1),
                        pct = Mathf.Clamp(_config.outline_levels.Count * 20f, 0f, 100f),
                        line_style = "solid",
                        color_hex = string.Empty
                    });
                }
            }

            if (!canAdd)
                EditorGUILayout.HelpBox("Reached outline-level cap for current line-style set.", MessageType.Info);
        }

        private void DrawSpecificMarkerOptions()
        {
            if (_config.pois == null || _config.pois.Count == 0)
            {
                EditorGUILayout.HelpBox("No POI data loaded.", MessageType.Info);
                return;
            }

            for (int i = 0; i < _config.pois.Count; i++)
            {
                var poi = _config.pois[i];
                if (poi == null)
                    continue;

                string foldoutKey = string.IsNullOrWhiteSpace(poi.id) ? $"poi_{i}" : poi.id;
                bool expanded = GetPoiFoldout(foldoutKey);
                expanded = EditorGUILayout.Foldout(expanded, $"{i + 1}. {poi.name} ({poi.id})", true);
                _poiFoldouts[foldoutKey] = expanded;
                if (!expanded)
                    continue;

                using (new EditorGUI.IndentLevelScope())
                {
                    _showPoiPosition = EditorGUILayout.Foldout(_showPoiPosition, "Position", true);
                    if (_showPoiPosition)
                    {
                        using (new EditorGUI.IndentLevelScope())
                            DrawPoiPositionFields(poi);
                    }

                    _showPoiMarkerStyle = EditorGUILayout.Foldout(_showPoiMarkerStyle, "Marker Style", true);
                    if (_showPoiMarkerStyle)
                    {
                        using (new EditorGUI.IndentLevelScope())
                            DrawPoiMarkerStyleFields(poi);
                    }

                    if (_config.marker_use_badge)
                    {
                        _showPoiBadgeStyle = EditorGUILayout.Foldout(_showPoiBadgeStyle, "Badge Style", true);
                        if (_showPoiBadgeStyle)
                        {
                            using (new EditorGUI.IndentLevelScope())
                                DrawPoiBadgeStyleFields(poi);
                        }

                        _showPoiOutline = EditorGUILayout.Foldout(_showPoiOutline, "Outline", true);
                        if (_showPoiOutline)
                        {
                            using (new EditorGUI.IndentLevelScope())
                                DrawPoiOutlineFields(poi);
                        }
                    }

                    _showPoiEffects = EditorGUILayout.Foldout(_showPoiEffects, "Effects", true);
                    if (_showPoiEffects)
                    {
                        using (new EditorGUI.IndentLevelScope())
                            DrawPoiEffectsFields(poi);
                    }
                }

                EditorGUILayout.Space(6f);
            }
        }

        private void DrawPoiPositionFields(POIData poi)
        {
            poi.x_norm = EditorGUILayout.Slider("X norm", poi.x_norm, 0f, 1f);
            poi.y_norm = EditorGUILayout.Slider("Y norm", poi.y_norm, 0f, 1f);

            bool hasCaptured = poi.has_captured_position;
            bool wantsCaptured = EditorGUILayout.Toggle("Use captured position", hasCaptured);

            if (wantsCaptured && !hasCaptured)
            {
                poi.captured_position = new CapturedPosition();
                poi.has_captured_position = true;
                poi.captured_position_source = "manual";
                poi.captured_position_timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            }
            else if (!wantsCaptured && hasCaptured)
            {
                poi.captured_position = null;
                poi.has_captured_position = false;
                poi.captured_position_source = null;
            }

            if (poi.captured_position != null)
            {
                var cp = poi.captured_position;
                cp.x = EditorGUILayout.FloatField("X", cp.x);
                cp.y = EditorGUILayout.FloatField("Y", cp.y);
                cp.z = EditorGUILayout.FloatField("Z", cp.z);
            }
        }

        private void DrawPoiMarkerStyleFields(POIData poi)
        {
            poi.name = EditorGUILayout.TextField("Name", poi.name);
            poi.category = DrawCategoryDropdown("Category", poi.category);
            poi.is_hero = EditorGUILayout.Toggle("Is hero", poi.is_hero);
        }

        private void DrawPoiBadgeStyleFields(POIData poi)
        {
            poi.badge_category = DrawBadgeCategoryDropdown("Badge category", poi.badge_category);
        }

        private void DrawPoiOutlineFields(POIData poi)
        {
            bool hasStatus = poi.has_status;
            bool wantsStatus = EditorGUILayout.Toggle("Has status", hasStatus);

            if (wantsStatus && !hasStatus)
            {
                poi.has_status = true;
                poi.status_pct = 0f;
            }
            else if (!wantsStatus && hasStatus)
            {
                poi.has_status = false;
                poi.status_pct = 0f;
                poi.status_unknown = false;
                poi.status_level_key = null;
            }

            if (poi.has_status)
            {
                if (_config.outline_levels != null && _config.outline_levels.Count > 0)
                    DrawStatusLevelDropdown(poi);
                else
                    poi.status_pct = EditorGUILayout.Slider("Status %", poi.status_pct, 0f, 100f);

                poi.status_unknown = EditorGUILayout.Toggle("Status unknown", poi.status_unknown);
            }

            poi.rotate_contour = EditorGUILayout.Toggle("Rotate contour", poi.rotate_contour);
        }

        private void DrawPoiEffectsFields(POIData poi)
        {
            var current = MarkerVisualsParser.ParseEffectFlags(poi.effect_mode);
            var tokens = new List<string>();

            if (EditorGUILayout.Toggle("Pulse", current.HasFlag(MarkerEffectFlags.Pulse))) tokens.Add("pulse");
            if (EditorGUILayout.Toggle("Sun Contours", current.HasFlag(MarkerEffectFlags.SunContours))) tokens.Add("sun_contours");
            if (EditorGUILayout.Toggle("Sun Circles", current.HasFlag(MarkerEffectFlags.SunCircles))) tokens.Add("sun_circles");
            if (EditorGUILayout.Toggle("Ring Pulse", current.HasFlag(MarkerEffectFlags.RingPulse))) tokens.Add("ring_pulse");
            if (EditorGUILayout.Toggle("Simple Sun", current.HasFlag(MarkerEffectFlags.SimpleSun))) tokens.Add("simple_sun");
            if (EditorGUILayout.Toggle("Beacon", current.HasFlag(MarkerEffectFlags.Beacon))) tokens.Add("beacon");

            poi.effect_mode = string.Join(",", tokens);
        }

        private void DrawWallIconLibrarySelector()
        {
            EnsureDefaultIconLibraryLoaded();

            using (new EditorGUILayout.HorizontalScope())
            {
                _wallIconLibrary = (SpriteKeyLibrary)EditorGUILayout.ObjectField("Wall icon library", _wallIconLibrary, typeof(SpriteKeyLibrary), false);

                if (GUILayout.Button("Create", GUILayout.Width(70f)))
                {
                    CreateOrAssignWallIconLibrary();
                }
            }

            if (_wallIconLibrary != null)
            {
                string assetPath = AssetDatabase.GetAssetPath(_wallIconLibrary);
                string resourcesPath = AssetPathToResourcesPath(assetPath);
                if (string.IsNullOrWhiteSpace(resourcesPath))
                {
                    EditorGUILayout.HelpBox("Wall icon library must be inside a Resources folder for runtime loading.", MessageType.Warning);
                }
                else
                {
                    EditorGUILayout.LabelField("Resources path", resourcesPath);
                    _config.marker_icon_library_resources_path = resourcesPath;
                }
            }
            else
            {
                EditorGUILayout.HelpBox("No wall icon library assigned. Symbol picker uses default icons; create a wall library to add new symbols cleanly.", MessageType.Info);
            }
        }

        private void CreateOrAssignWallIconLibrary()
        {
            string directory = GetWallLibraryDirectory();
            string wallName = _config != null && !string.IsNullOrWhiteSpace(_config.wall_id) ? _config.wall_id : "Wall";
            string fileName = SanitizeFileName(wallName) + "_IconLibrary.asset";
            string assetPath = directory + "/" + fileName;

            EnsureAssetDirectory(directory);

            var existing = AssetDatabase.LoadAssetAtPath<SpriteKeyLibrary>(assetPath);
            if (existing != null)
            {
                _wallIconLibrary = existing;
            }
            else
            {
                var created = CreateInstance<SpriteKeyLibrary>();
                if (_defaultIconLibrary != null)
                    created.CopyFrom(_defaultIconLibrary);

                AssetDatabase.CreateAsset(created, assetPath);
                AssetDatabase.SaveAssets();
                _wallIconLibrary = created;
            }

            string resourcesPath = AssetPathToResourcesPath(assetPath);
            if (_config != null && !string.IsNullOrWhiteSpace(resourcesPath))
                _config.marker_icon_library_resources_path = resourcesPath;

            _hasUnsavedChanges = true;
            EditorUtility.SetDirty(this);
            Repaint();
        }

        private string AssignSpriteToLibraryAndGetKey(Sprite sprite, string suggestedName)
        {
            if (sprite == null)
                return null;

            var library = GetEditableIconLibrary();
            if (library == null)
                return null;

            Undo.RecordObject(library, "Assign Marker Symbol");
            string key = library.EnsureKeyForSprite(sprite, suggestedName);
            EditorUtility.SetDirty(library);
            AssetDatabase.SaveAssets();
            return key;
        }

        private Sprite ResolveSpriteForKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return null;

            var wall = _wallIconLibrary != null ? _wallIconLibrary.Get(key) : null;
            if (wall != null)
                return wall;

            EnsureDefaultIconLibraryLoaded();
            return _defaultIconLibrary != null ? _defaultIconLibrary.Get(key) : null;
        }

        private SpriteKeyLibrary GetEditableIconLibrary()
        {
            if (_wallIconLibrary != null)
                return _wallIconLibrary;

            EnsureDefaultIconLibraryLoaded();
            return _defaultIconLibrary;
        }

        private void DrawSpritePreview(Sprite sprite)
        {
            Texture preview = null;
            if (sprite != null)
                preview = AssetPreview.GetAssetPreview(sprite) ?? AssetPreview.GetMiniThumbnail(sprite);

            GUILayout.Box(preview ?? Texture2D.grayTexture, GUILayout.Width(20f), GUILayout.Height(20f));
        }

        private static void DrawColorHexCompact(ref string colorHex)
        {
            Color parsed = TryParseHexColor(colorHex, out var c) ? c : Color.white;

            EditorGUI.BeginChangeCheck();
            Color picked = EditorGUILayout.ColorField(GUIContent.none, parsed, false, true, false, GUILayout.Width(28f));
            if (EditorGUI.EndChangeCheck())
            {
                colorHex = ToHexRgb(picked);
            }

            colorHex = EditorGUILayout.TextField(colorHex ?? string.Empty, GUILayout.Width(90f));
        }

        private static bool TryParseHexColor(string hex, out Color color)
        {
            color = default;
            if (string.IsNullOrWhiteSpace(hex))
                return false;

            string normalized = hex.Trim();
            if (!normalized.StartsWith("#", StringComparison.Ordinal))
                normalized = "#" + normalized;

            return ColorUtility.TryParseHtmlString(normalized, out color);
        }

        private static string ToHexRgb(Color color)
        {
            Color32 c32 = color;
            return $"#{c32.r:X2}{c32.g:X2}{c32.b:X2}";
        }

        private void DrawConfigMutationScope(Action drawContent, bool refreshRigOnChange)
        {
            if (_config == null)
            {
                drawContent?.Invoke();
                return;
            }

            string before = JsonUtility.ToJson(_config, prettyPrint: false);
            drawContent?.Invoke();
            string after = JsonUtility.ToJson(_config, prettyPrint: false);

            if (before == after)
                return;

            RecordConfigChange(before, after);
            _hasUnsavedChanges = true;

            if (refreshRigOnChange)
                RefreshRigVisuals();
        }

        private void RecordConfigChange(string before, string after)
        {
            if (_isApplyingHistory)
                return;

            if (_configHistory.Count == 0)
            {
                _configHistory.Add(before);
                _configHistoryIndex = 0;
            }

            if (_configHistoryIndex < _configHistory.Count - 1)
                _configHistory.RemoveRange(_configHistoryIndex + 1, _configHistory.Count - (_configHistoryIndex + 1));

            if (!string.Equals(_configHistory[_configHistoryIndex], before, StringComparison.Ordinal))
            {
                _configHistory.Add(before);
                _configHistoryIndex = _configHistory.Count - 1;
            }

            if (!string.Equals(_configHistory[_configHistoryIndex], after, StringComparison.Ordinal))
            {
                _configHistory.Add(after);
                _configHistoryIndex = _configHistory.Count - 1;
            }
        }

        private void InitializeConfigHistory()
        {
            _configHistory.Clear();
            _configHistoryIndex = -1;

            if (_config == null)
                return;

            _configHistory.Add(JsonUtility.ToJson(_config, prettyPrint: false));
            _configHistoryIndex = 0;
        }

        private bool CanUndoConfigChange() => _configHistoryIndex > 0;
        private bool CanRedoConfigChange() => _configHistoryIndex >= 0 && _configHistoryIndex < _configHistory.Count - 1;

        private void UndoConfigChange()
        {
            if (!CanUndoConfigChange())
                return;

            _configHistoryIndex--;
            ApplyConfigSnapshot(_configHistory[_configHistoryIndex]);
            _hasUnsavedChanges = true;
            RefreshRigVisuals();
        }

        private void RedoConfigChange()
        {
            if (!CanRedoConfigChange())
                return;

            _configHistoryIndex++;
            ApplyConfigSnapshot(_configHistory[_configHistoryIndex]);
            _hasUnsavedChanges = true;
            RefreshRigVisuals();
        }

        private void ApplyConfigSnapshot(string snapshot)
        {
            if (string.IsNullOrWhiteSpace(snapshot))
                return;

            _isApplyingHistory = true;
            _config = JsonUtility.FromJson<WallConfigData>(snapshot);
            TryResolveWallIconLibraryFromConfig();
            _isApplyingHistory = false;
            Repaint();
        }

        private void HandleUndoShortcuts()
        {
            var e = Event.current;
            if (e == null || e.type != EventType.KeyDown)
                return;

            bool ctrl = e.control || e.command;
            if (!ctrl)
                return;

            if (e.keyCode == KeyCode.Z && !e.shift)
            {
                UndoConfigChange();
                e.Use();
            }
            else if ((e.keyCode == KeyCode.Z && e.shift) || e.keyCode == KeyCode.Y)
            {
                RedoConfigChange();
                e.Use();
            }
        }

        private void DrawPathRow(string label, ref string path, string extension)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                path = EditorGUILayout.TextField(label, path);
                if (GUILayout.Button("...", GUILayout.Width(30f)))
                {
                    string abs = EditorUtility.OpenFilePanel($"Select {label}", Application.dataPath, extension);
                    if (!string.IsNullOrWhiteSpace(abs))
                    {
                        string rel = AbsoluteToAssetPath(abs);
                        if (!string.IsNullOrWhiteSpace(rel))
                            path = rel;
                        else
                            EditorUtility.DisplayDialog("Invalid path", "Please choose a file inside this Unity project.", "OK");
                    }
                }
            }
        }

        private static string AbsoluteToAssetPath(string absolutePath)
        {
            string normalizedAbs = absolutePath.Replace("\\", "/");
            string normalizedAssets = Application.dataPath.Replace("\\", "/");
            if (!normalizedAbs.StartsWith(normalizedAssets, StringComparison.OrdinalIgnoreCase))
                return null;

            return "Assets" + normalizedAbs.Substring(normalizedAssets.Length);
        }

        private static string AssetPathToResourcesPath(string assetPath)
        {
            if (string.IsNullOrWhiteSpace(assetPath))
                return null;

            string normalized = assetPath.Replace("\\", "/");
            int resourcesIndex = normalized.IndexOf("/Resources/", StringComparison.OrdinalIgnoreCase);
            if (resourcesIndex < 0)
                return null;

            string relative = normalized.Substring(resourcesIndex + "/Resources/".Length);
            if (relative.EndsWith(".asset", StringComparison.OrdinalIgnoreCase))
                relative = relative.Substring(0, relative.Length - ".asset".Length);
            return relative;
        }

        private string GetWallLibraryDirectory()
        {
            string configDir = Path.GetDirectoryName(_configPath)?.Replace("\\", "/");
            if (string.IsNullOrWhiteSpace(configDir) || !configDir.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
                configDir = "Assets/Apps/LivingRoom";

            return configDir + "/MarkerAssets/Resources/MarkerSymbols";
        }

        private static void EnsureAssetDirectory(string assetDirectory)
        {
            string relative = assetDirectory.Replace("\\", "/");
            if (!relative.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
                return;

            string root = Directory.GetParent(Application.dataPath)?.FullName?.Replace("\\", "/");
            if (string.IsNullOrWhiteSpace(root))
                return;

            string absolute = root + "/" + relative;
            if (!Directory.Exists(absolute))
                Directory.CreateDirectory(absolute);

            AssetDatabase.Refresh();
        }

        private static string SanitizeFileName(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return "Wall";

            var invalid = Path.GetInvalidFileNameChars();
            var chars = raw.Trim().ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                if (invalid.Contains(chars[i]) || char.IsWhiteSpace(chars[i]))
                    chars[i] = '_';
            }

            return new string(chars);
        }

        private void EnsureDefaultIconLibraryLoaded()
        {
            if (_defaultIconLibrary == null)
                _defaultIconLibrary = AssetDatabase.LoadAssetAtPath<SpriteKeyLibrary>(DefaultIconLibraryPath);
        }

        private void TryResolveWallIconLibraryFromConfig()
        {
            _wallIconLibrary = null;
            if (_config == null || string.IsNullOrWhiteSpace(_config.marker_icon_library_resources_path))
                return;

            string target = _config.marker_icon_library_resources_path.Trim();
            string[] guids = AssetDatabase.FindAssets("t:SpriteKeyLibrary");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.Equals(AssetPathToResourcesPath(path), target, StringComparison.Ordinal))
                {
                    _wallIconLibrary = AssetDatabase.LoadAssetAtPath<SpriteKeyLibrary>(path);
                    break;
                }
            }
        }

        private string DrawCategoryDropdown(string label, string current)
        {
            var options = CollectCategoryOptions();
            int idx = Mathf.Max(0, options.IndexOf(current));
            int next = EditorGUILayout.Popup(label, idx, options.ToArray());
            return options[next];
        }

        private string DrawBadgeCategoryDropdown(string label, string current)
        {
            var options = new List<string> { "" };
            if (_config?.badge_categories != null)
            {
                foreach (var entry in _config.badge_categories)
                {
                    if (entry == null || string.IsNullOrWhiteSpace(entry.key))
                        continue;
                    if (!options.Contains(entry.key))
                        options.Add(entry.key);
                }
            }

            int idx = Mathf.Max(0, options.IndexOf(current));
            int next = EditorGUILayout.Popup(label, idx, options.ToArray());
            return options[next];
        }

        private void DrawStatusLevelDropdown(POIData poi)
        {
            var levels = _config.outline_levels;
            if (levels == null || levels.Count == 0)
            {
                poi.status_pct = EditorGUILayout.Slider("Status %", poi.status_pct, 0f, 100f);
                return;
            }

            var labels = new string[levels.Count];
            int selectedIndex = 0;
            for (int i = 0; i < levels.Count; i++)
            {
                var level = levels[i];
                string levelLabel = !string.IsNullOrWhiteSpace(level.label) ? level.label : (level.key ?? $"Level {i + 1}");
                labels[i] = levelLabel + " (" + level.pct.ToString("0") + "%)";

                if (!string.IsNullOrWhiteSpace(poi.status_level_key) && poi.status_level_key == level.key)
                    selectedIndex = i;
            }

            int next = EditorGUILayout.Popup("Status level", selectedIndex, labels);
            next = Mathf.Clamp(next, 0, levels.Count - 1);
            poi.status_level_key = levels[next].key;
            poi.status_pct = levels[next].pct;
            EditorGUILayout.LabelField("Resolved status %", poi.status_pct.ToString("0.0"));
        }

        private List<string> CollectCategoryOptions()
        {
            var options = new List<string>();

            if (_config?.category_styles != null)
            {
                foreach (var entry in _config.category_styles)
                {
                    if (entry == null || string.IsNullOrWhiteSpace(entry.category))
                        continue;
                    if (!options.Contains(entry.category))
                        options.Add(entry.category);
                }
            }

            if (options.Count == 0)
                options.Add("unknown");

            return options;
        }

        private bool GetPoiFoldout(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return true;

            if (!_poiFoldouts.TryGetValue(key, out bool expanded))
            {
                expanded = true;
                _poiFoldouts[key] = true;
            }

            return expanded;
        }

        private void SaveAllToJson()
        {
            CapturePositions(silentWhenRigMissing: true);
            SaveConfig();
        }

        private void SaveConfig()
        {
            if (_config == null)
            {
                Debug.LogWarning("[POIAuthoring] Nothing to save.");
                return;
            }

            string json = JsonUtility.ToJson(_config, prettyPrint: true);
            File.WriteAllText(_configPath, json);
            AssetDatabase.Refresh();
            _hasUnsavedChanges = false;
            Debug.Log($"[POIAuthoring] Saved config to {_configPath}");
        }

        private void LoadConfig()
        {
            if (!File.Exists(_configPath))
            {
                Debug.LogError($"[POIAuthoring] Config not found at {_configPath}");
                return;
            }

            string json = File.ReadAllText(_configPath);
            _config = JsonUtility.FromJson<WallConfigData>(json);

            if (_config == null)
            {
                Debug.LogError("[POIAuthoring] Failed to parse config.");
                return;
            }

            if (_config.category_styles == null) _config.category_styles = new List<CategoryStyleEntry>();
            if (_config.badge_categories == null) _config.badge_categories = new List<BadgeCategoryEntry>();
            if (_config.outline_levels == null) _config.outline_levels = new List<OutlineLevelEntry>();
            if (_config.pois == null) _config.pois = new List<POIData>();

            EnsureDefaultIconLibraryLoaded();
            TryResolveWallIconLibraryFromConfig();
            InitializeConfigHistory();
            _hasUnsavedChanges = false;

            Debug.Log($"[POIAuthoring] Loaded {_config.pois.Count} POIs from {_configPath}");
            Repaint();
        }

        private void CopyToStreamingAssets()
        {
            if (!File.Exists(_configPath))
            {
                Debug.LogError("[POIAuthoring] Source config not found.");
                return;
            }

            string dir = Path.GetDirectoryName(_streamingConfigPath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            File.Copy(_configPath, _streamingConfigPath, overwrite: true);
            AssetDatabase.Refresh();
            Debug.Log($"[POIAuthoring] Copied to StreamingAssets: {_streamingConfigPath}");
        }

        private void RefreshRigVisuals()
        {
            if (_config == null || _config.pois == null) return;

            Transform rig = GetExistingRig();
            if (rig == null) return;

            bool hasCategoryDefinitions = _config.category_styles != null && _config.category_styles.Count > 0;
            if (hasCategoryDefinitions) CategoryPalette.Configure(_config.category_styles);
            else CategoryPalette.ClearOverrides();

            BadgeCategoryPalette.Configure(_config.badge_categories);

            bool hasOutlineLevels = _config.outline_levels != null && _config.outline_levels.Count > 0;
            if (hasOutlineLevels) StatusRamp.Configure(_config.outline_levels);

            bool hasShapeFromConfig = MarkerVisualsParser.TryParseShape(_config.marker_shape, out var shape);

            MarkerOutlineMode outlineMode;
            bool useBadge;
            if (!string.IsNullOrWhiteSpace(_config.marker_outline_mode))
            {
                if (!MarkerVisualsParser.TryParseOutlineMode(_config.marker_outline_mode, out outlineMode))
                    outlineMode = MarkerOutlineMode.None;
                useBadge = _config.marker_use_badge;
            }
            else if (MarkerVisualsParser.TryParseStyle(_config.marker_style, out var legacyStyle))
            {
                MarkerVisualsParser.DeriveOutlineAndBadgeFromLegacyStyle(
                    legacyStyle == MarkerStyle.Badge ? "badge" :
                    legacyStyle == MarkerStyle.OutlineSameHue ? "outline_same_hue" : "outline_gold",
                    out outlineMode,
                    out useBadge);
            }
            else
            {
                outlineMode = MarkerOutlineMode.None;
                useBadge = false;
            }

            var runtimeLibrary = _wallIconLibrary;

            foreach (var poi in _config.pois)
            {
                var child = rig.Find(poi.id);
                if (child == null) continue;

                var anchor = child.GetComponentInChildren<POIAnchor>() ?? child.gameObject.AddComponent<POIAnchor>();
                anchor.Initialise(poi);

                var markerView = child.GetComponentInChildren<MarkerView>();
                var effects = MarkerVisualsParser.ParseEffectFlags(poi.effect_mode);
                markerView?.Initialise(anchor, outlineMode, useBadge, shape, effects,
                    hasCategoryDefinitions,
                    hasShapeFromConfig,
                    hasOutlineLevels,
                    runtimeLibrary);
            }
        }

        internal bool IsRigInSyncWithConfig(out int outOfSyncCount)
        {
            outOfSyncCount = 0;

            Transform rig = GetExistingRig();
            if (rig == null || _config == null || _config.pois == null)
                return true;

            foreach (Transform child in rig)
            {
                var poi = _config.pois.Find(p => p.id == child.name);

                if (poi == null)
                {
                    outOfSyncCount++;
                    continue;
                }

                if (!poi.has_captured_position)
                {
                    outOfSyncCount++;
                    continue;
                }

                Vector3 savedPos = new Vector3(
                    poi.captured_position.x,
                    poi.captured_position.y,
                    poi.captured_position.z);

                float distance = Vector3.Distance(child.localPosition, savedPos);
                if (distance > SyncPositionTolerance)
                    outOfSyncCount++;
            }

            return outOfSyncCount == 0;
        }

        private void ClearRig()
        {
            Transform rig = GetExistingRig();

            if (rig == null || rig.childCount == 0)
            {
                EditorUtility.DisplayDialog(
                    "Nothing to clear",
                    "POIAuthoringRig has no children.",
                    "OK");
                return;
            }

            bool inSync = IsRigInSyncWithConfig(out int outOfSyncCount);
            bool proceed = inSync || EditorUtility.DisplayDialog(
                "Uncaptured or unsynced positions",
                $"{outOfSyncCount} marker(s) in the rig don't match config.json (never captured, or moved since the last capture). Clearing now will lose that placement work. Clear anyway?",
                "Clear anyway",
                "Cancel");

            if (!proceed)
                return;

            for (int i = rig.childCount - 1; i >= 0; i--)
                Undo.DestroyObjectImmediate(rig.GetChild(i).gameObject);

            Debug.Log("[POIAuthoring] Cleared POIAuthoringRig children.");
        }

        private Transform GetExistingRig()
        {
            TryResolveSceneReferences();
            if (_correctionAnchor == null)
                return null;

            for (int i = 0; i < _correctionAnchor.childCount; i++)
            {
                var child = _correctionAnchor.GetChild(i);
                if (child.name == "POIAuthoringRig")
                    return child;
            }

            return null;
        }

        private Transform GetOrCreateRig()
        {
            Transform existing = GetExistingRig();
            if (existing != null)
                return existing;

            if (_correctionAnchor == null)
                return null;

            var go = new GameObject("POIAuthoringRig");
            Undo.RegisterCreatedObjectUndo(go, "Create POIAuthoringRig");
            go.transform.SetParent(_correctionAnchor);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            return go.transform;
        }

        private int GetRigChildCount()
        {
            Transform rig = GetExistingRig();
            return rig != null ? rig.childCount : 0;
        }

        private void TryResolveSceneReferences()
        {
            if (_correctionAnchor == null)
            {
                var correctionAnchorObject = GameObject.Find("PlacementCorrectionAnchor");
                if (correctionAnchorObject != null)
                    _correctionAnchor = correctionAnchorObject.transform;
            }

            if (_wallMesh == null)
                _wallMesh = GameObject.Find("146267-LivingRoom2-tex");
        }

        private void PopulateRig()
        {
            if (!File.Exists(_configPath))
            {
                Debug.LogError($"[POIAuthoring] Source config not found at {_configPath}");
                return;
            }

            LoadConfig();

            if (_config == null || _config.pois == null)
            {
                Debug.LogWarning("[POIAuthoring] No config loaded.");
                return;
            }

            var rig = GetOrCreateRig();
            if (rig == null)
            {
                Debug.LogError("[POIAuthoring] Cannot find or create POIAuthoringRig. Assign a correction anchor.");
                return;
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(_prefabPath);
            if (prefab == null)
            {
                Debug.LogError($"[POIAuthoring] Prefab not found at {_prefabPath}");
                return;
            }

            if (rig.childCount > 0)
            {
                bool clear = EditorUtility.DisplayDialog(
                    "POI Authoring Rig",
                    $"POIAuthoringRig already has {rig.childCount} object(s). Clear existing rig first?",
                    "Clear and repopulate",
                    "Cancel");

                if (!clear)
                {
                    Debug.Log("[POIAuthoring] Populate cancelled by user.");
                    return;
                }

                var children = new List<GameObject>();
                for (int i = 0; i < rig.childCount; i++)
                    children.Add(rig.GetChild(i).gameObject);

                foreach (var child in children)
                    Undo.DestroyObjectImmediate(child);
            }

            var anchors = _config.calibration_anchors?.ToArray() ?? Array.Empty<CalibrationAnchor>();

            bool hasCategoryDefinitions = _config.category_styles != null && _config.category_styles.Count > 0;
            if (hasCategoryDefinitions) CategoryPalette.Configure(_config.category_styles);
            else CategoryPalette.ClearOverrides();

            BadgeCategoryPalette.Configure(_config.badge_categories);

            bool hasOutlineLevels = _config.outline_levels != null && _config.outline_levels.Count > 0;
            if (hasOutlineLevels) StatusRamp.Configure(_config.outline_levels);

            bool hasShapeFromConfig = MarkerVisualsParser.TryParseShape(_config.marker_shape, out var shape);

            MarkerOutlineMode outlineMode;
            bool useBadge;
            if (!string.IsNullOrWhiteSpace(_config.marker_outline_mode))
            {
                if (!MarkerVisualsParser.TryParseOutlineMode(_config.marker_outline_mode, out outlineMode))
                    outlineMode = MarkerOutlineMode.None;
                useBadge = _config.marker_use_badge;
            }
            else if (MarkerVisualsParser.TryParseStyle(_config.marker_style, out var legacyStyle))
            {
                MarkerVisualsParser.DeriveOutlineAndBadgeFromLegacyStyle(
                    legacyStyle == MarkerStyle.Badge ? "badge" :
                    legacyStyle == MarkerStyle.OutlineSameHue ? "outline_same_hue" : "outline_gold",
                    out outlineMode,
                    out useBadge);
            }
            else
            {
                outlineMode = MarkerOutlineMode.None;
                useBadge = false;
            }

            foreach (var poi in _config.pois)
            {
                if (!POIPositionResolver.TryResolvePosition(poi, anchors, out Vector3 localPos))
                {
                    Debug.LogWarning($"[POIAuthoring] Skipping POI '{poi.id}' - position could not be resolved.");
                    continue;
                }

                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, rig);
                instance.name = poi.id;
                instance.transform.localPosition = localPos;
                instance.transform.localRotation = Quaternion.identity;

                Undo.RegisterCreatedObjectUndo(instance, $"Populate marker for {poi.id}");

                var anchor = instance.GetComponentInChildren<POIAnchor>() ?? instance.AddComponent<POIAnchor>();
                anchor.Initialise(poi);

                var markerView = instance.GetComponentInChildren<MarkerView>();
                if (markerView != null)
                {
                    var effects = MarkerVisualsParser.ParseEffectFlags(poi.effect_mode);
                    markerView.Initialise(anchor, outlineMode, useBadge, shape, effects,
                        hasCategoryDefinitions,
                        hasShapeFromConfig,
                        hasOutlineLevels,
                        _wallIconLibrary);
                }
            }

            SelectRigObjects();
            Debug.Log($"[POIAuthoring] Populated {rig.childCount} markers under POIAuthoringRig.");
        }

        private void CapturePositions(bool silentWhenRigMissing = false)
        {
            if (_config == null || _config.pois == null)
            {
                Debug.LogWarning("[POIAuthoring] No config loaded.");
                return;
            }

            Transform rig = GetExistingRig();
            if (rig == null || rig.childCount == 0)
            {
                if (!silentWhenRigMissing)
                    Debug.LogWarning("[POIAuthoring] No POIAuthoringRig with children found. Populate first.");
                return;
            }

            int captured = 0;
            int skipped = 0;
            var sceneObjects = new Dictionary<string, Transform>();

            for (int i = 0; i < rig.childCount; i++)
            {
                var child = rig.GetChild(i);
                sceneObjects[child.name] = child;
            }

            foreach (var poi in _config.pois)
            {
                if (!sceneObjects.TryGetValue(poi.id, out var markerTransform))
                {
                    skipped++;
                    continue;
                }

                Vector3 localPos;
                if (_correctionAnchor != null)
                    localPos = _correctionAnchor.InverseTransformPoint(markerTransform.position);
                else
                    localPos = markerTransform.localPosition;

                poi.captured_position = new CapturedPosition
                {
                    x = localPos.x,
                    y = localPos.y,
                    z = localPos.z
                };
                poi.has_captured_position = true;
                poi.captured_position_source = "workflow_a_editor";
                poi.captured_position_timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                captured++;
            }

            _hasUnsavedChanges = true;
            Debug.Log($"[POIAuthoring] Captured {captured} positions (skipped {skipped} missing scene objects).");
            Repaint();
        }

        private void SelectRigObjects()
        {
            Transform rig = GetExistingRig();
            if (rig == null || rig.childCount == 0)
                return;

            var gos = new List<GameObject>();
            for (int i = 0; i < rig.childCount; i++)
                gos.Add(rig.GetChild(i).gameObject);

            Selection.objects = gos.ToArray();
        }

        private void OnSceneGUI(SceneView sv)
        {
            if (_config == null || _config.pois == null)
                return;

            var anchors = _config.calibration_anchors?.ToArray() ?? Array.Empty<CalibrationAnchor>();

            Handles.color = Color.cyan;
            foreach (var poi in _config.pois)
            {
                if (!POIPositionResolver.TryResolvePosition(poi, anchors, out Vector3 localPos))
                    continue;

                Vector3 worldPos;
                if (_correctionAnchor != null)
                    worldPos = _correctionAnchor.TransformPoint(localPos);
                else
                    worldPos = localPos;

                float handleSize = HandleUtility.GetHandleSize(worldPos) * 0.1f;
                Handles.SphereHandleCap(0, worldPos, Quaternion.identity, handleSize, EventType.Repaint);
            }
        }
    }
}

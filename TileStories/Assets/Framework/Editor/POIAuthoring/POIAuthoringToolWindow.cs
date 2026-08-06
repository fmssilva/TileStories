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
    public partial class POIAuthoringToolWindow : EditorWindow
    {
        private const string DefaultConfigPath = "Assets/Apps/LivingRoom/config.json";
        private const string DefaultStreamingConfigPath = "Assets/StreamingAssets/LivingRoom/config.json";
        private const string DefaultPrefabPath = "Assets/Framework/Runtime/UI/Markers/POI_Marker.prefab";
        private const string DefaultIconLibraryPath = "Assets/Framework/Runtime/UI/Markers/IconLibrary.asset";
        private const float SyncPositionTolerance = 0.001f;

        [SerializeField] private WallConfigData _config;
        [SerializeField] private Transform _correctionAnchor;
        [SerializeField] private GameObject _wallMesh;
        [SerializeField] private string _configPath = DefaultConfigPath;
        [SerializeField] private string _streamingConfigPath = DefaultStreamingConfigPath;
        [SerializeField] private string _prefabPath = DefaultPrefabPath;
        [SerializeField] private Vector2 _scrollPos;

        [SerializeField] private bool _showTopConfig = true;

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

            _showGlobalSceneOptions = DrawFramedFoldout(_showGlobalSceneOptions, "Global Scene Options", GlobalSectionColor,
                () => DrawConfigMutationScope(DrawGlobalSceneOptions, refreshRigOnChange: true));

            EditorGUILayout.Space(8f);

            _showSpecificMarkerOptions = DrawFramedFoldout(_showSpecificMarkerOptions, "Specific Marker Options", GlobalSectionColor,
                () => DrawConfigMutationScope(DrawSpecificMarkerOptions, refreshRigOnChange: true));

            EditorGUILayout.EndScrollView();
        }

        // Draw a foldout with a coloured left border so the two top-level groups
        // (Global / Specific) read as visually distinct sections, not just plain
        // foldouts. Inner Marker/Badge/Outline foldouts use the same helper at a
        // lighter accent colour so the nesting reads without a second method.
        private bool DrawFramedFoldout(bool expanded, string title, Color accentColor, Action drawContent)
        {
            var headerStyle = new GUIStyle(EditorStyles.foldoutHeader)
            {
                normal = { textColor = accentColor },
                onNormal = { textColor = accentColor }
            };
            expanded = EditorGUILayout.Foldout(expanded, title, true, headerStyle);

            if (expanded)
            {
                var rect = EditorGUILayout.BeginVertical(GUI.skin.box);
                EditorGUI.DrawRect(new Rect(rect.x, rect.y, 2f, rect.height), accentColor);
                using (new EditorGUI.IndentLevelScope())
                    drawContent?.Invoke();
                EditorGUILayout.EndVertical();
            }

            return expanded;
        }

        private void DrawToolbar()
        {
            GUILayout.Label("POI Authoring Tool", EditorStyles.boldLabel);
            EditorGUILayout.Space(4f);
        }

        private void DrawTopConfigAndActions()
        {
            TryResolveSceneReferences();

            _showTopConfig = EditorGUILayout.Foldout(_showTopConfig, "Scene Configuration", true);
            if (_showTopConfig)
            {
                DrawPathRow("Config path", ref _configPath, "json");
                DrawPathRow("Streaming path", ref _streamingConfigPath, "json");
                DrawPathRow("Marker prefab", ref _prefabPath, "prefab");

                _correctionAnchor = (Transform)EditorGUILayout.ObjectField("Correction anchor", _correctionAnchor, typeof(Transform), true);
                _wallMesh = (GameObject)EditorGUILayout.ObjectField("Wall mesh (reference)", _wallMesh, typeof(GameObject), true);
            }

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
    }
}

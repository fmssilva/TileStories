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

        // EditorPrefs key for the "Don't show again" toggle on safety prompts.
        internal const string SkipPromptPrefKey = "TileStories.RigSafetySkipPrompt";

        // ---- Static safety infrastructure ----
        // PromptBeforePlayOrBuild is called by POIAuthoringRigSafetyCheck (Play
        // Mode) and POIAuthoringRigBuildCheck (Build). The event subscription and
        // play-mode hook live in the dedicated safety-check class instead, keeping
        // this window class focused on authoring logic.

        // Shows the rig-safety dialog and returns true to proceed (Play or Build),
        // false to abort. When isBuild is true the "Continue Without Clearing"
        // option is hidden because a build is visitor-facing.
        internal static bool PromptBeforePlayOrBuild(bool isBuild)
        {
            int childCount = GetRigChildCountStatic();
            if (childCount == 0)
                return true; // Nothing to warn about.

            if (EditorPrefs.GetBool(SkipPromptPrefKey, false))
                return true; // User opted out via "Don't show again".

            return ShowRigSafetyDialog(childCount, isBuild);
        }

        // Pure dialog logic. Returns true to proceed, false to abort.
        private static bool ShowRigSafetyDialog(int childCount, bool isBuild)
        {
            string button1 = isBuild ? "Save, Clear & Build" : "Save, Clear & Play";

            string message = isBuild
                ? $"POIAuthoringRig has {childCount} marker(s). These are Edit-Mode authoring stand-ins and must not ship. Save positions to config.json and clear the rig before building."
                : $"POIAuthoringRig has {childCount} marker(s) in the scene. If you have not captured positions to JSON, you will get duplicate markers at runtime. Save and clear now?";

            // DisplayDialogComplex returns 0 = left button, 1 = middle, 2 = right.
            // Layout (play):  [Save, Clear & Play]  [Continue Without Clearing]  [Cancel]
            // Layout (build): [Save, Clear & Build]  [Cancel]

            int choice;
            if (isBuild)
            {
                // Two-button dialog: button3 must be "" for the right button to
                // be button2 (return 1), not button3 (return 2).
                choice = EditorUtility.DisplayDialogComplex(
                    "POIAuthoringRig Safety Check",
                    message,
                    button1,    // 0 = left (Save, Clear & Build)
                    "Cancel",   // 1 = right (Cancel)
                    "");        // no middle button
            }
            else
            {
                choice = EditorUtility.DisplayDialogComplex(
                    "POIAuthoringRig Safety Check",
                    message,
                    button1,                              // 0 = left (Save, Clear & Play)
                    "Continue Without Clearing",          // 1 = middle
                    "Cancel");                            // 2 = right
            }

            if (choice == 0) // Save, Clear & Continue
            {
                SaveAndClearRig();
                return true;
            }

            if (!isBuild && choice == 1) // Continue Without Clearing (play only)
                return true;

            // Cancel (any context) or -1 (closed via X)
            return false;
        }

        // Finds the open POIAuthoringToolWindow instance (if any) and calls
        // its SaveAllToJson + ClearRig. If the window is not open we still
        // clear the rig to prevent duplicates, but warn that config was not saved.
        private static void SaveAndClearRig()
        {
            var windows = Resources.FindObjectsOfTypeAll<POIAuthoringToolWindow>();
            if (windows.Length > 0)
            {
                var tool = windows[0];
                tool.SaveAllToJson();  // CapturePositions + SaveConfig
                tool.ClearRig();
            }
            else
            {
                // Window is closed -- just clear the rig to prevent duplicates.
                var rig = GameObject.Find("POIAuthoringRig");
                if (rig != null)
                {
                    for (int i = rig.transform.childCount - 1; i >= 0; i--)
                        Undo.DestroyObjectImmediate(rig.transform.GetChild(i).gameObject);
                }
                Debug.LogWarning("[POIAuthoringRigSafety] Tool window was not open. Rig was cleared but config.json was NOT saved. Open the POI Authoring Tool and click 'Save All to JSON' to persist your work.");
            }
        }

        // Safe static accessor -- does NOT require a window instance.
        private static int GetRigChildCountStatic()
        {
            var rig = GameObject.Find("POIAuthoringRig");
            if (rig == null)
                return 0;

            // Only count children under a rig that is a child of PlacementCorrectionAnchor.
            if (rig.transform.parent == null || rig.transform.parent.name != "PlacementCorrectionAnchor")
                return 0;

            return rig.transform.childCount;
        }

        // ---- Menu item for "Don't show again" toggle ----

        [MenuItem("TileStories/POI Authoring/Rig Safety Prompt on Play/Build")]
        private static void ToggleRigSafetyPrompt()
        {
            bool currentlySkipping = EditorPrefs.GetBool(SkipPromptPrefKey, false);
            EditorPrefs.SetBool(SkipPromptPrefKey, !currentlySkipping);
        }

        [MenuItem("TileStories/POI Authoring/Rig Safety Prompt on Play/Build", true)]
        private static void ValidateToggleRigSafetyPrompt()
        {
            Menu.SetChecked(
                "TileStories/POI Authoring/Rig Safety Prompt on Play/Build",
                !EditorPrefs.GetBool(SkipPromptPrefKey, false));
        }

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
        [SerializeField] private bool _showGlobalEffects = true;
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

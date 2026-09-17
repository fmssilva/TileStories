using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TileStories.Editor
{
    // Editor window for POI marker placement + wall-level marker editing.
    // Key behavior:
    // - Config JSON is edited in-memory, then written explicitly by Save.
    // - Scene rig mutations use Unity Undo (objects/components).
    // - Config data mutations use a local snapshot history for Ctrl+Z / Ctrl+Y.
    public partial class POIEditorToolWindow : EditorWindow
    {

        private enum TabSelection
        {
            GlobalScene,
            SpecificMarker
        }

        private TabSelection _selectedTab = TabSelection.GlobalScene;

        private const string DefaultConfigPath = "Assets/Apps/LivingRoom/config.json";
        private const string DefaultStreamingConfigPath = "Assets/StreamingAssets/LivingRoom/config.json";
        private const string DefaultPrefabPath = "Assets/Framework/Runtime/UI/Markers/POI_Marker.prefab";
        private const string DefaultIconLibraryPath = "Assets/Framework/Runtime/UI/Markers/IconLibrary.asset";
        private const float SyncPositionTolerance = 0.001f;

        // EditorPrefs key for the "Don't show again" toggle on safety prompts.
        internal const string SkipPromptPrefKey = "TileStories.RigSafetySkipPrompt";

        // EditorPrefs key for the "Don't show again" toggle on un-verifying confirmed positions.
        internal const string SkipUnverifyPromptPrefKey = "TileStories.SkipUnverifyConfirmation";
        internal const string VerifiedPositionLockedMessage = "Those positions are already verified. If you want to change them, click the Verified button to enable editing.";

        private readonly Dictionary<string, Vector3> _lastVerifiedPositions = new();

        private Vector3 GetLastVerifiedPositionForPoi(POIData poi)
        {
            if (poi == null)
                return Vector3.zero;

            if (_lastVerifiedPositions.TryGetValue(poi.id, out var verifiedPosition))
                return verifiedPosition;

            if (poi.position != null)
                return new Vector3(poi.position.x, poi.position.y, poi.position.z);

            var rig = GetExistingRig();
            if (rig != null)
            {
                var child = rig.Find(poi.id);
                if (child != null)
                    return child.localPosition;
            }

            return Vector3.zero;
        }

        internal static bool ShouldBlockVerifiedPositionMove(POIData poi, Vector3 currentLocalPosition, out Vector3 correctedLocalPosition, out string message)
        {
            return ShouldBlockVerifiedPositionMove(poi, currentLocalPosition, null, out correctedLocalPosition, out message);
        }

        internal static bool ShouldBlockVerifiedPositionMove(POIData poi, Vector3 currentLocalPosition, Vector3? lastVerifiedLocalPosition, out Vector3 correctedLocalPosition, out string message)
        {
            correctedLocalPosition = currentLocalPosition;
            message = string.Empty;

            if (poi == null || !poi.position_verified)
                return false;

            Vector3 lockPosition = lastVerifiedLocalPosition ?? (poi.position != null
                ? new Vector3(poi.position.x, poi.position.y, poi.position.z)
                : currentLocalPosition);

            if (Vector3.Distance(currentLocalPosition, lockPosition) <= 0.0001f)
                return false;

            correctedLocalPosition = lockPosition;
            message = VerifiedPositionLockedMessage;
            return true;
        }

        private void HandleSceneGui(SceneView sceneView)
        {
            if (_config == null || _config.pois == null || Event.current == null)
                return;

            if (Selection.activeGameObject == null)
                return;

            Transform target = Selection.activeGameObject.transform;
            while (target != null && target.parent != null && target.parent.name != "POIEditorRig")
                target = target.parent;

            if (target == null || target.parent == null || target.parent.name != "POIEditorRig")
                return;

            var poi = _config.pois.FirstOrDefault(p => p.id == target.name);
            if (poi == null)
                return;

            if (!poi.position_verified)
            {
                // Live two-way sync: while the developer rotates a marker with Unity's
                // Rotate tool, read the child's euler angles back into config so the
                // "Edit Rotation" (Y) slider and the persisted X/Z stay in lockstep.
                // Runs in OnSceneGUI -- outside the window's DrawConfigMutationScope --
                // so it never feeds back through the scope's JSON diff (which would spam
                // undo history and refresh the rig on every Scene repaint).
                if (Event.current.type == EventType.MouseDrag || Event.current.type == EventType.MouseUp)
                {
                    if (SyncPoiRotationFromScene(poi, target.localRotation.eulerAngles))
                    {
                        _hasUnsavedChanges = true;
                        Repaint();
                    }
                }
                return;
            }

            if (Event.current.type != EventType.MouseDrag && Event.current.type != EventType.MouseUp)
                return;

            Vector3 lastVerifiedPosition = GetLastVerifiedPositionForPoi(poi);
            if (!ShouldBlockVerifiedPositionMove(poi, target.localPosition, lastVerifiedPosition, out var correctedPosition, out var message))
                return;

            Undo.RecordObject(target, "Revert verified POI position");
            target.localPosition = correctedPosition;
            target.localRotation = PoiRotationResolver.ToEulerQuaternion(poi.editor_rotation_x_deg, poi.editor_rotation_deg, poi.editor_rotation_z_deg);

            if (sceneView != null)
                sceneView.ShowNotification(new GUIContent(message));
        }

        // Write a rig child's scene euler angles back into the POI's rotation fields,
        // returning true when anything actually changed. Y maps to editor_rotation_deg
        // (the Edit Rotation slider), X/Z to the pitch/roll fields. Normalizes each axis
        // into [0, 360) so dragging past a full turn or through negative wraps cleanly.
        // Pure (no SceneView), so it is Tier-0 testable.
        internal static bool SyncPoiRotationFromScene(POIData poi, Vector3 euler)
        {
            if (poi == null)
                return false;

            float x = PoiRotationResolver.NormalizeAngleDeg(euler.x);
            float y = PoiRotationResolver.NormalizeAngleDeg(euler.y);
            float z = PoiRotationResolver.NormalizeAngleDeg(euler.z);

            if (Mathf.Abs(x - poi.editor_rotation_x_deg) < 0.001f &&
                Mathf.Abs(y - poi.editor_rotation_deg) < 0.001f &&
                Mathf.Abs(z - poi.editor_rotation_z_deg) < 0.001f)
                return false;

            poi.editor_rotation_x_deg = x;
            poi.editor_rotation_deg = y;
            poi.editor_rotation_z_deg = z;
            return true;
        }

        // ---- Static safety infrastructure ----
        // PromptBeforePlayOrBuild is called by POIEditorRigSafetyCheck (Play
        // Mode) and POIEditorRigBuildCheck (Build). The event subscription and
        // play-mode hook live in the dedicated safety-check class instead, keeping
        // this window class focused on editor logic.

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
                ? $"POIEditorRig has {childCount} marker(s). These are Edit-Mode editor stand-ins and must not ship. Save positions to config.json and clear the rig before building."
                : $"POIEditorRig has {childCount} marker(s) in the scene. If you have not captured positions to JSON, you will get duplicate markers at runtime. Save and clear now?";

            // DisplayDialogComplex returns 0 = left button, 1 = middle, 2 = right.
            // Layout (play):  [Save, Clear & Play]  [Continue Without Clearing]  [Cancel]
            // Layout (build): [Save, Clear & Build]  [Cancel]

            int choice;
            if (isBuild)
            {
                // Two-button dialog: button3 must be "" for the right button to
                // be button2 (return 1), not button3 (return 2).
                choice = EditorUtility.DisplayDialogComplex(
                    "POIEditorRig Safety Check",
                    message,
                    button1,    // 0 = left (Save, Clear & Build)
                    "Cancel",   // 1 = right (Cancel)
                    "");        // no middle button
            }
            else
            {
                choice = EditorUtility.DisplayDialogComplex(
                    "POIEditorRig Safety Check",
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

        // Finds the open POIEditorToolWindow instance (if any) and calls
        // its SaveAllToJson + ClearRig. If the window is not open we still
        // clear the rig to prevent duplicates, but warn that config was not saved.
        private static void SaveAndClearRig()
        {
            var windows = Resources.FindObjectsOfTypeAll<POIEditorToolWindow>();
            if (windows.Length > 0)
            {
                var tool = windows[0];
                tool.SaveAllToJson();  // CapturePositions + SaveConfig
                tool.ClearRig();
            }
            else
            {
                // Window is closed -- just clear the rig to prevent duplicates.
                var rig = GameObject.Find("POIEditorRig");
                if (rig != null)
                {
                    for (int i = rig.transform.childCount - 1; i >= 0; i--)
                        Undo.DestroyObjectImmediate(rig.transform.GetChild(i).gameObject);
                }
                Debug.LogWarning("[POIEditorRigSafety] Tool window was not open. Rig was cleared but config.json was NOT saved. Open the POI Editor and click 'Save All to JSON' to persist your work.");
            }
        }

        // Safe static accessor -- does NOT require a window instance.
        private static int GetRigChildCountStatic()
        {
            var rig = GameObject.Find("POIEditorRig");
            if (rig == null)
                return 0;

            // Only count children under a rig that is a child of PlacementCorrectionAnchor.
            if (rig.transform.parent == null || rig.transform.parent.name != "PlacementCorrectionAnchor")
                return 0;

            return rig.transform.childCount;
        }

        // ---- Menu item for "Don't show again" toggle ----

        // Deliberately NOT under "TileStories/POI Editor": Unity gives a submenu
        // precedence over a same-named command, so sharing that branch silently
        // removed the POI Editor entry from the menu (only Shift+P still opened it).
        [MenuItem("TileStories/Rig Safety Prompt on Play/Build")]
        private static void ToggleRigSafetyPrompt()
        {
            bool currentlySkipping = EditorPrefs.GetBool(SkipPromptPrefKey, false);
            EditorPrefs.SetBool(SkipPromptPrefKey, !currentlySkipping);
        }

        // Must return bool: a void validator leaves the item permanently greyed
        // out. Menu.SetEnabled is gone in Unity 6, so the return value is the only
        // way to report "this item is usable" -- and a validator that returns
        // nothing is read as "disabled".
        [MenuItem("TileStories/Rig Safety Prompt on Play/Build", true)]
        private static bool ValidateToggleRigSafetyPrompt()
        {
            Menu.SetChecked(
                "TileStories/Rig Safety Prompt on Play/Build",
                !EditorPrefs.GetBool(SkipPromptPrefKey, false));
            return true;
        }

        [SerializeField] private WallConfigData _config;
        [SerializeField] private GameObject _wallMesh;
        [SerializeField] private string _configPath = DefaultConfigPath;
        [SerializeField] private string _streamingConfigPath = DefaultStreamingConfigPath;
        [SerializeField] private string _prefabPath = DefaultPrefabPath;
        [SerializeField] private Vector2 _scrollPos;

        [SerializeField] private bool _showTopConfig = true;

        [SerializeField] private bool _showGlobalMarker = true;
        [SerializeField] private bool _showGlobalBadge = true;
        [SerializeField] private bool _showGlobalOutline = true;
        [SerializeField] private bool _showGlobalHierarchy = true;
        [SerializeField] private bool _showGlobalEffects = true;
        // Block 2 (_2.4 rows 5b/12/13): LOD + AR-zoom editor foldouts.
        [SerializeField] private bool _showGlobalLod = true;
        [SerializeField] private bool _showGlobalZoom = true;
        // Block 8 (_2.5 section 11): Displacement editor foldout.
        [SerializeField] private bool _showGlobalDisplacement = true;
        // Block 5 (_2.6 section 3): Search & Filter editor foldout.
        [SerializeField] private bool _showGlobalSearchFilter = true;

        [SerializeField] private bool _showPoiPosition = true;
        [SerializeField] private bool _showPoiMarkerStyle = true;
        [SerializeField] private bool _showPoiBadgeStyle = true;
        [SerializeField] private bool _showPoiOutline = true;
        [SerializeField] private bool _showPoiSearchKeywords = true;

        [SerializeField] private SpriteKeyLibrary _defaultIconLibrary;
        [SerializeField] private SpriteKeyLibrary _wallIconLibrary;
        [SerializeField] private bool _hasUnsavedChanges;

        private readonly Dictionary<string, bool> _poiFoldouts = new();
        private readonly List<string> _configHistory = new();
        private int _configHistoryIndex = -1;
        private bool _isApplyingHistory;

        [MenuItem("TileStories/POI Editor #P")]
        private static void ShowWindow()
        {
            var w = GetWindow<POIEditorToolWindow>();
            w.titleContent = new GUIContent("POI Editor");
            w.Show();
        }

        private void OnEnable()
        {
            SceneView.duringSceneGui -= HandleSceneGui;
            SceneView.duringSceneGui += HandleSceneGui;
            EnsureDefaultIconLibraryLoaded();
        }

        private void OnDisable()
        {
        }

        private void OnGUI()
        {
            HandleUndoShortcuts();

            DrawTopConfigAndActions();

            if (_config == null)
            {
                EditorGUILayout.HelpBox("No config loaded. Click Load & Populate Rig.", MessageType.Info);
                return;
            }

            // Tab buttons with constant base colors (Global Scene = blue, Specific Marker = green)
            var originalBgColor = GUI.backgroundColor;

            using (new EditorGUILayout.HorizontalScope())
            {
                // Global Scene tab button (always blue)
                var globalTabStyle = new GUIStyle(GUI.skin.button);
                globalTabStyle.fontStyle = FontStyle.Bold;
                globalTabStyle.normal.textColor = TabTextColor;
                globalTabStyle.onNormal.textColor = TabTextColor;
                GUI.backgroundColor = GlobalSceneTabColor;
                if (GUILayout.Button("Global Scene", globalTabStyle, GUILayout.Height(26f), GUILayout.ExpandWidth(false)))
                    _selectedTab = TabSelection.GlobalScene;

                // Specific Marker tab button (always green)
                var specificTabStyle = new GUIStyle(GUI.skin.button);
                specificTabStyle.fontStyle = FontStyle.Bold;
                specificTabStyle.normal.textColor = TabTextColor;
                specificTabStyle.onNormal.textColor = TabTextColor;
                GUI.backgroundColor = SpecificMarkerTabColor;
                if (GUILayout.Button("Specific Marker", specificTabStyle, GUILayout.Height(26f), GUILayout.ExpandWidth(false)))
                    _selectedTab = TabSelection.SpecificMarker;
            }

            GUI.backgroundColor = originalBgColor;

            // Tab buttons are a fixed header above; this scroll wraps only the
            // active tab's content (each rendered inside DrawTabContentContainer's
            // colored border) so the tab row never scrolls away. No spacer between.
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            switch (_selectedTab)
            {
                case TabSelection.GlobalScene:
                    DrawConfigMutationScope(() => DrawTabContentContainer(DrawGlobalSceneOptions, GlobalSectionColor), refreshRigOnChange: true);
                    break;
                case TabSelection.SpecificMarker:
                    DrawConfigMutationScope(() => DrawTabContentContainer(DrawSpecificMarkerOptions, SpecificMarkerTabColor), refreshRigOnChange: true);
                    break;
            }

            EditorGUILayout.EndScrollView();
        }

        /// Create a bold GUIStyle for foldout headers with the specified text color.
        private static GUIStyle CreateFoldoutStyle(Color textColor)
        {
            var style = new GUIStyle(EditorStyles.foldout);
            style.fontStyle = FontStyle.Bold;
            style.normal.textColor = textColor;
            style.onNormal.textColor = textColor;
            return style;
        }

        /// Create a bold LABEL style carrying the same header color.
        /// Used for the POI header NAME text. A foldout style must not be handed to
        /// EditorGUI.LabelField: its normal/onNormal state carries the foldout-arrow
        /// texture as a background, so the label paints a SECOND arrow next to the real
        /// foldout arrow. A boldLabel-derived style has no such background.
        private static GUIStyle CreateHeaderLabelStyle(Color textColor)
        {
            var style = new GUIStyle(EditorStyles.boldLabel);
            style.normal.textColor = textColor;
            return style;
        }

        /// Draw a foldout with a bold colored title and its content when expanded.
        /// When drawHeaderTrailing is provided it is drawn on the SAME row, right of the
        /// title (e.g. a help "(i)" button), so a section can carry its own help inline.
        private static bool DrawFramedFoldout(ref bool expanded, Action content, string title, Color titleColor, Action drawHeaderTrailing = null)
        {
            var boldStyle = CreateFoldoutStyle(titleColor);
            if (drawHeaderTrailing == null)
            {
                expanded = EditorGUILayout.Foldout(expanded, title, true, boldStyle);
            }
            else
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    // Size the foldout to its title with a FIXED left-anchored rect so it
                    // does NOT auto-expand: laying a default EditorGUILayout.Foldout out
                    // directly eats the whole row, which would push the trailing help to
                    // the far right edge regardless of the Space below. EditorGUILayout's
                    // Foldout overload takes no GUILayoutOption, so the width is fixed via
                    // an explicit GUI rect instead (matching IMGUI's left/width rules).
                    // Width = measured title text + the arrow affordance. The trailing help
                    // then sits a fixed step after the title, inside the heading zone.
                    float titleWidth = boldStyle.CalcSize(new GUIContent(title)).x + 20f;
                    Rect foldoutRect = GUILayoutUtility.GetRect(
                        new GUIContent(title), boldStyle, GUILayout.Width(titleWidth), GUILayout.ExpandWidth(false));
                    expanded = EditorGUI.Foldout(foldoutRect, expanded, title, true, boldStyle);
                    GUILayout.Space(40f);
                    drawHeaderTrailing();
                }
            }

            if (expanded)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    content?.Invoke();
                }
            }

            return expanded;
        }

        /// Draw a content container with colored top and left borders for visual section grouping.
        private static void DrawTabContentContainer(Action content, Color containerColor, string label = "", GUIStyle labelStyle = null)
        {
            using (new EditorGUI.IndentLevelScope())
            {
                // Capture a valid start rect: when a label precedes the content we
                // take its rect; otherwise reserve a zero-height control so GetLastRect
                // is legal (calling it immediately after beginning a group throws
                // "You cannot call GetLast immediately after beginning a group").
                Rect startRect;
                if (!string.IsNullOrEmpty(label))
                {
                    EditorGUILayout.LabelField(label, labelStyle ?? EditorStyles.boldLabel);
                    startRect = GUILayoutUtility.GetLastRect();
                }
                else
                {
                    startRect = EditorGUILayout.GetControlRect(false, 0f, GUILayout.ExpandWidth(true));
                }
                float startY = startRect.yMax;

                content?.Invoke();

                Rect endRect = GUILayoutUtility.GetLastRect();
                float height = Mathf.Max(0f, endRect.yMax - startY);

                // Colored top border
                EditorGUI.DrawRect(new Rect(startRect.x, startY, startRect.width, 2f), containerColor);
                // Colored left border
                EditorGUI.DrawRect(new Rect(startRect.x, startY, 3f, height), containerColor);
            }
        }

        private void DrawTopConfigAndActions()
        {
            _showTopConfig = EditorGUILayout.Foldout(_showTopConfig, "Scene Configuration", true, CreateFoldoutStyle(SceneConfigSectionColor));
            if (_showTopConfig)
            {
                DrawTabContentContainer(() =>
                {
                    DrawPathRow("Config path", ref _configPath, "json", out _, out _, out _);
                    DrawPathRow("Streaming path", ref _streamingConfigPath, "json", out _, out _, out _);
                    DrawPathRow("Marker prefab", ref _prefabPath, "prefab", out _, out _, out _);

                    // Shared row: transparent indent spacer + width capped to
                    // max(MinRowWidth, min(visible panel, MaxRowWidth)). Here the
                    // stretchy element is the ObjectField; it takes the full rowWidth.
                    DrawEditorRow(out float meshRowWidth, out _);
                    _wallMesh = (GameObject)EditorGUILayout.ObjectField("Wall mesh (reference)", _wallMesh, typeof(GameObject), true,
                        GUILayout.Width(meshRowWidth), GUILayout.ExpandWidth(false));
                    EditorRowEnd();
                }, SceneConfigSectionColor);
            }

            EditorGUILayout.Space(6f);

            // Shared row: transparent indent spacer + width capped to
            // max(MinRowWidth, min(visible panel, MaxRowWidth)). Two buttons
            // share the capped row; each gets ~half minus the inter-button gap.
            DrawEditorRow(out float rigRowWidth, out _);
            {
                float rigShare = Mathf.Max(0f, (rigRowWidth - 4f) / 2f);
                if (GUILayout.Button("Load & Populate Rig", GUILayout.Height(26f), GUILayout.Width(rigShare), GUILayout.ExpandWidth(false)))
                    LoadAndPopulateRig();

                bool hasRigChildren = GetRigChildCount() > 0;
                var previousColor = GUI.color;
                if (hasRigChildren)
                    GUI.color = new Color(0.95f, 0.55f, 0.25f);

                if (GUILayout.Button("Clear Rig", GUILayout.Height(26f), GUILayout.Width(rigShare), GUILayout.ExpandWidth(false)))
                    ClearRig();

                GUI.color = previousColor;
            }
            EditorRowEnd();

            // Shared row: transparent indent spacer + width capped to
            // max(MinRowWidth, min(visible panel, MaxRowWidth)). Undo/Redo
            // keep their fixed 85px; Save and Copy share the remaining width.
            DrawEditorRow(out float saveRowWidth, out _);
            {
                float fixedButtons = 85f + 85f + 12f; // Undo + Redo + three ~4px gaps
                float actionShare = Mathf.Max(0f, (saveRowWidth - fixedButtons) / 2f);
                var previousColor = GUI.color;
                if (_hasUnsavedChanges)
                    GUI.color = new Color(0.96f, 0.78f, 0.25f);

                if (GUILayout.Button("Save All to JSON", GUILayout.Height(26f), GUILayout.Width(actionShare), GUILayout.ExpandWidth(false)))
                    SaveAllToJson();

                GUI.color = previousColor;

                if (GUILayout.Button("Copy to StreamingAssets", GUILayout.Height(26f), GUILayout.Width(actionShare), GUILayout.ExpandWidth(false)))
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
            EditorRowEnd();

        }
    }
}

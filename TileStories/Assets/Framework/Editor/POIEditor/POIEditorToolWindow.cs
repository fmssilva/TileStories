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
        private const string DefaultFontLibraryPath = "Assets/Framework/Runtime/UI/Markers/FontLibrary.asset";
        private const float SyncPositionTolerance = 0.001f;

        // EditorPrefs key for the "Don't show again" toggle on un-verifying confirmed positions.
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

        // Scene-view mouse gestures on the selected rig marker (drag = move/rotate, up =
        // gesture end). Everything else in the scene GUI stream is ignored here.
        private void HandleSceneGui(SceneView sceneView)
        {
            if (_config == null || _config.pois == null || Event.current == null)
                return;

            if (Event.current.type != EventType.MouseDrag && Event.current.type != EventType.MouseUp)
                return;

            // A drag while a Move/Rotate/Rect handle owns the mouse (not a camera-orbit or
            // pan) is the developer acting on the marker, even when the transform itself
            // is then overwritten (Edit-Mode preview) or reverted (Verified lock).
            bool gizmoGesture = MarkerEditDetector.IsGizmoEditGesture(
                Event.current.type == EventType.MouseDrag, GUIUtility.hotControl, Tools.viewToolActive, Tools.current);

            HandleSelectedMarkerEdit(revealOnEdit: true, gizmoGesture);
        }

        // Runs ~10x/s while the window is open: keeps the sliders in sync with edits that
        // never produce a Scene-view mouse event (the Inspector's Transform fields, undo).
        // Never reveals -- only a real Scene-view gesture may move the window's scroll.
        private void OnInspectorUpdate()
        {
            RepaintLiveReadoutsWhilePlaying();
            if (_config == null || _config.pois == null)
                return;

            HandleSelectedMarkerEdit(revealOnEdit: false, gizmoGesture: false);
        }

        // Write a rig child's scene euler angles back into the POI's rotation fields,
        // returning true when anything actually changed. Y maps to editor_rotation_deg
        // (the Facing Y slider), X/Z to the pitch/roll fields. Normalizes each axis
        // into [0, 360) so dragging past a full turn or through negative wraps cleanly.
        // Pure (no SceneView), so it is Tier-0 testable.
        internal static bool SyncPoiRotationFromScene(POIData poi, Vector3 euler)
        {
            if (poi == null)
                return false;

            // Compare orientations, not raw triples: the scene reports a canonical euler
            // triple that can differ from the stored one for the very same rotation.
            Quaternion stored = PoiRotationResolver.ToEulerQuaternion(poi.editor_rotation_x_deg, poi.editor_rotation_deg, poi.editor_rotation_z_deg);
            if (PoiRotationResolver.IsSameOrientation(Quaternion.Euler(euler), stored))
                return false;

            poi.editor_rotation_x_deg = PoiRotationResolver.NormalizeAngleDeg(euler.x);
            poi.editor_rotation_deg = PoiRotationResolver.NormalizeAngleDeg(euler.y);
            poi.editor_rotation_z_deg = PoiRotationResolver.NormalizeAngleDeg(euler.z);
            return true;
        }

        // ---- Static safety infrastructure ----
        // PromptBeforePlayOrBuild is called by POIEditorRigSafetyCheck (Play
        // Mode) and POIEditorRigBuildCheck (Build). The event subscription and
        // play-mode hook live in the dedicated safety-check class instead, keeping
        // this window class focused on editor logic.

        // The gate before Play or a build while the POI Editor rig still holds its stand-in markers.
        // True = go on, false = stop. Uses the open POI Editor (the one holding a config) to save.
        // There is deliberately NO opt-out: an opt-out once let a build ship the stand-ins.
        internal static bool PromptBeforePlayOrBuild(bool isBuild) =>
            PromptBeforePlayOrBuild(isBuild, FindOpenEditorWithConfig());

        // Same, with the window that saves passed in (null = no POI Editor open: nothing can be saved)
        internal static bool PromptBeforePlayOrBuild(bool isBuild, POIEditorToolWindow tool)
        {
            int markerCount = GetRigChildCountStatic();
            if (markerCount == 0)
                return true; // Nothing to warn about.

            bool canSave = tool != null && tool._config != null;
            int movedCount = 0;
            if (canSave)
                tool.IsRigInSyncWithConfig(out movedCount);
            bool hasUnsavedEdits = canSave && tool._hasUnsavedChanges;

            var answer = EditorDecision.Ask(BuildRigSafetyQuestion(markerCount, isBuild, canSave, movedCount, hasUnsavedEdits));
            if (answer == DecisionAnswer.Confirm)
            {
                ClearRigBeforePlayOrBuild(canSave && (movedCount > 0 || hasUnsavedEdits) ? tool : null);
                return true;
            }
            return answer == DecisionAnswer.Alternative; // Play With Duplicates (never offered for a build)
        }

        // Pure: the gate's question from the real state, so every label says exactly what its button does.
        //  - something to save and a POI Editor to save it: "Save, Clear & Play/Build"
        //  - nothing to save, or no POI Editor open (then the message says moves are lost): "Clear & Play/Build"
        //  - Play only: "Play With Duplicates" leaves the stand-ins in (each marker then shows twice)
        internal static DecisionRequest BuildRigSafetyQuestion(int markerCount, bool isBuild, bool canSave,
            int movedCount, bool hasUnsavedEdits)
        {
            string run = isBuild ? "Build" : "Play";
            bool save = canSave && (movedCount > 0 || hasUnsavedEdits);

            var lines = new List<string>
            {
                isBuild
                    ? $"The scene still holds {markerCount} POI Editor stand-in marker(s). They exist only for editing and must not ship in a build."
                    : $"The scene still holds {markerCount} POI Editor stand-in marker(s). Play spawns its own markers from the config, so each of these would show twice."
            };
            if (save)
            {
                if (movedCount > 0) lines.Add($"- {movedCount} of them were moved since the last save.");
                if (hasUnsavedEdits) lines.Add("- The config has unsaved edits.");
                lines.Add($"Save, Clear & {run} saves everything to config.json AND its StreamingAssets copy (the copy {run} reads), then removes the stand-ins.");
            }
            else if (canSave)
            {
                lines.Add($"Everything is already saved. Clear & {run} removes the stand-ins.");
            }
            else
            {
                lines.Add($"The POI Editor window is closed, so nothing can be saved from here: Clear & {run} removes the stand-ins, and a marker moved since the last save loses its new place. To keep it, press Cancel, open the POI Editor and press Save All to JSON.");
            }
            if (!isBuild)
                lines.Add("Play With Duplicates leaves the stand-ins in the scene, e.g. to compare them with the real markers.");

            return new DecisionRequest
            {
                Title = "POI Editor stand-in markers in the scene",
                Message = string.Join("\n\n", lines),
                ConfirmLabel = (save ? "Save, Clear & " : "Clear & ") + run,
                AlternativeLabel = isBuild ? null : "Play With Duplicates",
            };
        }

        // What the confirm button does: save + copy to StreamingAssets (only when there is something
        // to save and a window to save it), then remove every stand-in. The developer already
        // answered, so the rig is cleared directly -- Clear Rig's own question is not asked again.
        private static void ClearRigBeforePlayOrBuild(POIEditorToolWindow saveWith)
        {
            if (saveWith != null)
            {
                saveWith.SaveAllToJson();          // CapturePositions + SaveConfig
                saveWith.CopyToStreamingAssets();  // Play and builds read this copy
            }

            var rig = GameObject.Find("POIEditorRig");
            if (rig != null)
            {
                for (int i = rig.transform.childCount - 1; i >= 0; i--)
                    Undo.DestroyObjectImmediate(rig.transform.GetChild(i).gameObject);
            }
            Debug.Log(saveWith != null
                ? "[POIEditorRigSafety] Saved, copied to StreamingAssets and cleared the rig."
                : "[POIEditorRigSafety] Cleared the rig (nothing saved).");
        }

        // The POI Editor window that holds a config, if one is open (it is the one that can save)
        private static POIEditorToolWindow FindOpenEditorWithConfig()
        {
            foreach (var window in Resources.FindObjectsOfTypeAll<POIEditorToolWindow>())
                if (window != null && window._config != null)
                    return window;
            return null;
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

        [SerializeField] private WallConfigData _config;
        [SerializeField] private GameObject _wallMesh;
        [SerializeField] private string _configPath = DefaultConfigPath;
        [SerializeField] private string _streamingConfigPath = DefaultStreamingConfigPath;
        [SerializeField] private string _prefabPath = DefaultPrefabPath;
        [SerializeField] private Vector2 _scrollPos;

        [SerializeField] private bool _showTopConfig = true;

        // Default collapsed: matches the Specific Marker tab's default-collapsed POI
        // sections, so a wall with many sections opens as a scannable list of closed
        // foldouts instead of dumping every Global Scene section open at once.
        // Labels, Text & Fonts (_2.0_Labels_And_Fonts_Design.md, 2026-09-22): wall-default label
        // typography, placed first since it will also host future UI Toolkit text/font config.
        [SerializeField] private bool _showGlobalLabelsAndFonts = false;
        [SerializeField] private bool _showGlobalMarker = false;
        // Block 6 (_2.1_Marker_Orientation.md): Orientation editor foldout, between Marker and Badge.
        [SerializeField] private bool _showGlobalOrientation = false;
        [SerializeField] private bool _showGlobalBadge = false;
        [SerializeField] private bool _showGlobalOutline = false;
        [SerializeField] private bool _showGlobalHierarchy = false;
        [SerializeField] private bool _showGlobalEffects = false;
        // Block 2 (_2.4 rows 5b/12/13): LOD + AR-zoom editor foldouts.
        [SerializeField] private bool _showGlobalLod = false;
        [SerializeField] private bool _showGlobalZoom = false;
        // Block 8 (_2.5 section 11): Displacement editor foldout.
        [SerializeField] private bool _showGlobalDisplacement = false;
        // Global Scene > Select, Filter & Search (_2.6)
        [SerializeField] private bool _showGlobalSearchFilter = false;

        // Default collapsed: a POI's inner sections open one at a time, on request,
        // instead of dumping all five at once every time a POI is expanded.
        [SerializeField] private bool _showPoiPosition = false;
        [SerializeField] private bool _showPoiMarkerStyle = false;
        [SerializeField] private bool _showPoiBadgeStyle = false;
        [SerializeField] private bool _showPoiOutline = false;
        [SerializeField] private bool _showPoiSearchKeywords = false;

        [SerializeField] private SpriteKeyLibrary _defaultIconLibrary;
        [SerializeField] private SpriteKeyLibrary _wallIconLibrary;
        [SerializeField] private FontKeyLibrary _wallFontLibrary;
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
            SceneView.duringSceneGui -= HandleOrientationPreviewSceneGui;
            SceneView.duringSceneGui += HandleOrientationPreviewSceneGui;
            EnsureDefaultIconLibraryLoaded();
        }

        // Unsubscribe BOTH scene handlers. HandleSceneGui used to stay subscribed after the
        // window closed, so a dead window kept reacting to Scene-view drags with its stale
        // config snapshot (e.g. a POI still "verified" there) -- reverting rotations and
        // showing the verified warning while the live window said unverified.
        private void OnDisable()
        {
            SceneView.duringSceneGui -= HandleSceneGui;
            SceneView.duringSceneGui -= HandleOrientationPreviewSceneGui;
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
        /// By default the trailing content sits a fixed step after the title, and the row
        /// spans the container's own (often much wider than 480) width. Pass
        /// rightAlignTrailing: true to instead cap the WHOLE header row to the same shared
        /// row-width convention every other row in this window uses (see
        /// EditorRowWidthForIndent / the file-level "Reusable Row-Layout Command" note):
        /// the title occupies the available space up to that cap, and the trailing content
        /// (e.g. Position's Verified + help buttons) sits right-aligned at the capped row's
        /// own right edge, not the container's.
        private static bool DrawFramedFoldout(ref bool expanded, Action content, string title, Color titleColor, Action drawHeaderTrailing = null, bool rightAlignTrailing = false)
        {
            var boldStyle = CreateFoldoutStyle(titleColor);
            if (drawHeaderTrailing == null)
            {
                expanded = EditorGUILayout.Foldout(expanded, title, true, boldStyle);
            }
            else if (rightAlignTrailing)
            {
                // Same indent + capped-width formula as DrawEditorRow, but that helper's
                // own BeginHorizontal is deliberately UNconstrained (every other caller
                // sizes its individual controls to rowWidth instead) -- here the row needs
                // GUILayout.FlexibleSpace() to fill exactly up to a capped boundary, which
                // only works if the enclosing horizontal group itself is width-constrained.
                //
                // NO manual indent spacer here (unlike DrawEditorRow's real spacer Button):
                // this row's first control is EditorGUI.Foldout, which -- unlike a plain
                // GUILayout.Button -- already lands at the same x as a sibling plain
                // EditorGUILayout.Foldout row (e.g. "Marker Style" right below) with zero
                // extra spacing; GUILayoutUtility.GetRect here is not itself indent-aware,
                // but neither is the sibling's implicit position, so the two already agree.
                // Adding GUILayout.Space(indent) in front (a previous version of this code
                // did) double-shifted this row's arrow past its siblings' -- confirmed by a
                // real screenshot showing Position's arrow sitting well right of Marker
                // Style's. The capped WIDTH still needs to reserve indent+rowWidth (not just
                // rowWidth) so the trailing content's right edge doesn't shift left along
                // with this fix -- only the leading spacer was ever the bug.
                float indent = EditorGUI.IndentedRect(new Rect(0f, 0f, 0f, 0f)).x;
                float rowWidth = EditorRowWidthForIndent(EditorGUIUtility.currentViewWidth, AddButtonRowRightMargin, indent);
                using (new EditorGUILayout.HorizontalScope(GUILayout.Width(indent + rowWidth)))
                {
                    float titleWidth = boldStyle.CalcSize(new GUIContent(title)).x + 20f;
                    Rect foldoutRect = GUILayoutUtility.GetRect(
                        new GUIContent(title), boldStyle, GUILayout.Width(titleWidth), GUILayout.ExpandWidth(false));
                    expanded = EditorGUI.Foldout(foldoutRect, expanded, title, true, boldStyle);
                    GUILayout.FlexibleSpace();
                    drawHeaderTrailing();
                }
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

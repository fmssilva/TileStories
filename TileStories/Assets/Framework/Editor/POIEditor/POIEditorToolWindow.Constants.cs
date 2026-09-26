using UnityEditor;
using UnityEngine;

namespace TileStories.Editor
{
    public partial class POIEditorToolWindow
    {
        // Taxonomy table layout (marker / badge / outline tables).
        // One rhythm for the whole window: a single gap between every logical
        // column group, and a single gap between sibling controls inside one
        // group. Tune both here instead of hunting per-table magic numbers.
        private const float TableGapBetweenGroups = 6f;
        private const float TableGapWithinGroup = 4f;

        // A centered variant of miniBoldLabel, for header titles sitting over a column of
        // dropdown/popup controls (those read center-ish, not left-aligned like a text field).
        // Lazily built once, not per-frame -- EditorStyles.miniBoldLabel isn't safely readable
        // before the editor GUI system is initialized, so this can't be a plain static readonly.
        private static GUIStyle _centeredMiniBoldLabel;
        private static GUIStyle CenteredMiniBoldLabel => _centeredMiniBoldLabel ?? (_centeredMiniBoldLabel = new GUIStyle(EditorStyles.miniBoldLabel) { alignment = TextAnchor.MiddleCenter });

        // The Hierarchy table's "Aa" button when its level overrides the wall label style (created
        // lazily: GUI.skin only exists inside OnGUI).
        private static GUIStyle _boldTableButton;
        private static GUIStyle BoldTableButton => _boldTableButton ?? (_boldTableButton = new GUIStyle(GUI.skin.button) { fontStyle = FontStyle.Bold });

        // Extra breathing room before the Color group (Marker/Badge tables only) --
        // developer screenshot feedback (2026-09-18) confirmed the standard
        // between-groups gap read as visually too tight once the Symbol group's preview
        // thumbnail sits directly next to the (now correctly narrow) color swatch.
        private const float TableGapBeforeColor = 20f;

        // Extra breathing room before the Marker Table's destructive delete button --
        // bigger than the standard between-groups gap so the danger affordance reads as
        // clearly separated from the (non-destructive) keyword group next to it, not
        // just another column boundary. Widened 16f -> 28f (2026-09-18, developer
        // screenshot feedback): 16f still read as visually too close to the standard gap.
        private const float TableGapBeforeDelete = 28f;

        // Row layout for non-table rows (buttons, stand-alone controls).
        // SectionRowIndent: the left pad that makes a bare GUILayout row sit at the
        // same x as EditorGUILayout content inside a foldout (GUILayout ignores
        // EditorGUI.indentLevel; EditorGUILayout does not -- this constant bridges
        // that gap so buttons align with the section's table rows).
        // MaxRowWidth: cap for non-table rows (sliders, plain fields) so they never
        // stretch across the whole window just because the taxonomy tables are wide.
        // Narrow window -> narrower row; wide window -> wider row, capped here.
        private const float SectionRowIndent = 15f;
        private const float MaxRowWidth = 480f;

        // INDENT LEVELS: the one vocabulary for how far a row sits to the right (_5.1_Editor_Tab.md,
        // subject "Indentation"). Pass one of these as the `extraIndentPixels` argument of the shared
        // field drawers (DrawScalarField/DrawIntField/DrawToggleField/DrawPopupField/DrawSliderField/
        // DrawColorField) or of DrawEditorRow. The level of a row is decided by WHERE IT IS IN THE
        // TREE, not by taste: a child sits one level right of its parent container.
        //   IndentLevel0: direct child of a section (DrawFramedFoldout content) -- the ambient indent
        //   IndentLevel1: child of a sub-foldout inside a section (Orientation's Vertical Alignment...)
        //   IndentLevel2: child of a sub-foldout of a sub-foldout, or the text block of an open guide
        //   IndentLevel3: one level deeper still
        // Raw pixels, never EditorGUI.IndentLevelScope: a whole scope step also makes the row's own
        // labelled control re-apply the ambient indent a second time (double-indent trap).
        private const float IndentStepPixels = 16f;
        private const float IndentLevel0 = 0f;
        private const float IndentLevel1 = IndentStepPixels;
        private const float IndentLevel2 = IndentStepPixels * 2f;
        private const float IndentLevel3 = IndentStepPixels * 3f;

        // CONDITIONAL ADVANCE: a row shown only when another field has a value (e.g. "Up Reference"
        // only when something is World Up) sits this much right of the level of its parent field. Add
        // it to the level (IndentLevel1 + ConditionalAdvance); a doubly conditional row adds it twice.
        // Half a level, a raw nudge (a full step reads as too far forward). Never fake indentation with
        // leading spaces in a label: that shifts only the glyphs, not the row.
        private const float ConditionalAdvance = 8f;

        // Floor for non-table rows: even on a very narrow panel a button/row must
        // stay readable, so the width clamp is max(MinRowWidth, min(panel, MaxRowWidth)).
        private const float MinRowWidth = 180f;

        // Right spacing between a row's controls and the visible panel edge, when the
        // panel is narrow. This must clear the ScrollView's VERTICAL SCROLLBAR (~15px in
        // the Pro skin): with the old 6f the row's right edge landed underneath the
        // scrollbar whenever the list was long enough to scroll, so the last few pixels
        // of every row were hidden. Used by the row width rule and must be stable across
        // the Layout and Repaint IMGUI passes.
        private const float AddButtonRowRightMargin = 20f;

        // Header pad for the Symbol/Style column: only the single within-group
        // gap that also separates the ObjectField from the preview in the body
        // rows. That puts the info button DIRECTLY OVER the preview thumbnail
        // (the preview is the curated picker now -- there is no separate select
        // button to make room for).
        private const float SymbolColumnPad = TableGapWithinGroup;

        // Color group widths (marker / badge / outline tables).
        // The first cell is the REAL working EditorGUILayout.ColorField, forced
        // wide (ColorPickerWidth) so it is easy to click -- no flat draw-only
        // swatch in front of it, which read as a dead cell. The hex field is the
        // "color name". The pair sits flush (GUILayout's default inter-control
        // spacing is already there), so the group width is just picker + hex.
        // Narrowed from 60f (2026-09-18, developer screenshot feedback): 60f x
        // singleLineHeight (~18f) is a 3.3:1 bar, not the "square-like" swatch a color
        // picker cell should read as. 32f keeps it above the ColorGroup_WidthConstants_
        // DerivedCorrectly test's 30f floor while getting close to square against the
        // fixed row height. Note this constant itself was NEVER widened by the
        // indent-doubling fix (Lesson 4) -- that bug was clipping the RENDERED width
        // down to roughly a third of this constant, so fixing it made the picker jump
        // from a broken ~20px back UP to whatever this constant said, which is why it
        // suddenly looked "too wide" right after the gap fix landed.
        private const float ColorPickerWidth = 32f;
        private const float ColorHexFieldWidth = 90f;
        private const float ColorGroupWidth = ColorPickerWidth + ColorHexFieldWidth;

        private static readonly string[] OutlineModeOptions = { "uniform", "same_hue", "per_type" };
        private static readonly string[] OutlineModeLabels = { "Uniform color", "Same hue as marker", "Per outline type" };
        private static readonly string[] LineStyleOptions = { "solid", "dash_long", "dash_medium", "dash_short", "dotted" };
        private static readonly string[] LineStyleLabels = { "Continuous", "Big Dashed", "Medium Dashed", "Small Dashed", "Dots" };
        private static readonly string[] ShapeOptions = { "circle", "rounded_square", "hexagon", "diamond", "star", "none" };
        private static readonly string[] ShapeLabels = { "Circle", "Rounded Square", "Hexagon", "Diamond", "Star", "None" };


                // Show-label options (explicit wording per Â§6 of 2.3 doc, clearer than bare checkbox).

        // --- Orientation editor constants (_2.1_Marker_Orientation.md v4) ---
        // Vertical Alignment domain.
        private static readonly string[] VerticalAlignmentModeOptions = { "world_up", "screen_up" };
        private static readonly string[] VerticalAlignmentModeLabels = { "World Up", "Screen Up" };
        private static readonly string[] ChildVerticalAlignmentModeOptions = { "inherit", "world_up", "screen_up" };
        private static readonly string[] ChildVerticalAlignmentModeLabels = { "Inherit", "World Up", "Screen Up" };
        private static readonly string[] UpReferenceOptions = { "world_gravity", "spawn_root", "custom" };
        private static readonly string[] UpReferenceLabels = { "World Gravity", "Spawn Root", "Custom" };

        // Facing Options domain.
        private static readonly string[] FacingModeOptions = { "wall_fixed", "yaw_only", "always_facing_camera" };
        private static readonly string[] FacingModeLabels = { "Wall Fixed", "Y Rotation Only", "Always Facing Camera" };
        private static readonly string[] FacingBasisOptions = { "view_plane", "camera_position" };
        private static readonly string[] FacingBasisLabels = { "View Plane", "Camera Position" };
        // Hierarchy Levels table's per-level override column (section 4.3): "" = inherit the wall setting.
        private static readonly string[] FacingModeOverrideOptions = { "", "wall_fixed", "yaw_only", "always_facing_camera" };
        private static readonly string[] FacingModeOverrideLabels = { "Inherit", "Wall Fixed", "Y Rotation Only", "Always Facing Camera" };

        // Update Cost domain.
        private static readonly string[] OrientationUpdateModeOptions = { "every_frame", "interval", "on_camera_delta" };
        private static readonly string[] OrientationUpdateModeLabels = { "Every Frame", "Interval", "On Camera Delta" };

        private static readonly string VerticalAlignmentModeHelp = "Which way is 'up' for the whole marker (Symbol, Ring and everything that doesn't override it below). World Up (default): stays upright against real-world gravity, so turning the phone does not spin it. Screen Up: always aligned to the phone's own screen edges instead.";
        private static readonly string ChildVerticalAlignmentHelp = "Inherit (default): follows the marker's own Vertical Alignment above, rigidly. World Up / Screen Up: overrides just this element's vertical alignment independently of the marker root - this is how 'marker stays upright in the world, but the label always reads screen-horizontal' (or the reverse) is achieved without moving the root.";
        private static readonly string UpReferenceHelp = "The real-world 'up' direction used wherever Vertical Alignment is World Up. World Gravity (default): true real-world up - no gyroscope needed, since AR world space is already gravity-aligned. Spawn Root: the wall's own placement anchor up instead - use only if this wall's map was scanned at a tilt and isn't gravity-aligned. Custom: an authored vector below.";
        private static readonly string CustomUpHelp = "The custom up-reference vector, used only when Up Reference is set to Custom.";
        private static readonly string FacingModeHelp = "What the marker points at. Wall Fixed: painted flat onto the wall using this POI's own Facing X / Y / Z (Specific Marker > POI > Position) - never moves at runtime. Y Rotation Only: keeps the authored X/Z wall tilt fixed, but continuously turns left/right (yaw) to face the visitor. Always Facing Camera (default): ignores the authored angles entirely and always looks straight at the visitor, like a classic billboard.";
        private static readonly string FacingBasisHelp = "How the marker's forward direction is chosen (Always Facing Camera only). View Plane (default): every marker parallel to the camera's near plane, no perspective skew anywhere on screen. Camera Position: each marker's forward points away from the camera individually, which reads as more physical for large markers but introduces slight skew off-centre.";
        private static readonly string OrientationUpdateModeHelp = "Cost control for how often orientation is re-resolved. Every Frame (default, recommended): always up to date, avoids a subtle stale-rotation mismatch with the label/badge displacement system (see the Update Mode help below) - the CPU cost is negligible even at 150 markers. Interval: re-resolves at most every Update Interval seconds. On Camera Delta: re-resolves only once the camera has rotated past Camera Delta degrees since the last resolve. Both non-default modes can make label/badge offsets lag the marker's own rotation for a moment after a fast camera turn - use them only if a real profiling pass shows a need.";
        private static readonly string OrientationUpdateIntervalHelp = "Seconds between orientation re-resolves, used only when Update Mode is Interval.";
        private static readonly string OrientationCameraDeltaHelp = "Degrees the camera must rotate before orientation re-resolves, used only when Update Mode is On Camera Delta.";
        private static readonly string EditModePreviewHelp = "Scene view ONLY. Shows the resolved Vertical Alignment / Facing Options directly on the Edit-Mode marker rig, without entering Play Mode. It has no effect in Play Mode or on a device, where the real orientation always runs. Nothing animates here (Edit Mode does not tick per-frame) - this checks the static result at the Scene camera's current angle only. The Scene camera cannot roll, so roll-dependent behaviour is tested in Play Mode. See the three 'How to ... Test' guides below for the step-by-step tests.";

        // Three test guides, one per test area, each in its own collapsible foldout. One block per
        // Orientation sub-section (Vertical Alignment, Facing Options, Update Cost), NOT per field:
        // the fields under a sub-section are tested together. Terse slide-style bullets.
        // ASCII only (20-code-quality.md 2.1); a test asserts that and that every sub-section is covered.
        private static readonly string OrientationSceneTestGuide =
            "SETUP\n" +
            "- Scene-Mode Preview ON (above), then top bar 'Load & Populate Rig'.\n" +
            "- Scene view: RMB + mouse = look, RMB + WASD = fly, Alt + LMB = orbit.\n" +
            "- Changes show live. Nothing is saved until 'Save All to JSON'.\n\n" +
            "VERTICAL ALIGNMENT\n" +
            "- Not possible in Scene test: needs camera roll and the Scene camera cannot roll.\n" +
            "- Test it in Play Mode.\n\n" +
            "FACING OPTIONS\n" +
            "- Wall Fixed: orbit the Scene camera, markers must NOT move.\n" +
            "  - Specific Marker > pick any one POI > its own Facing X/Y/Z sliders rotate the marker live.\n" +
            "  - Or Rotate tool (E) on the marker: the sliders follow. Verified POIs are locked.\n" +
            "- Y Rotation Only: orbit left/right, markers turn to you. Orbit up/down, they do not tip.\n" +
            "  - Facing X/Z sliders apply, Y does nothing (a notice explains it).\n" +
            "- Always Facing Camera: markers face the Scene camera from any angle. Facing sliders do nothing (notice).\n" +
            "  - Facing Basis: View Plane = all parallel to screen. Camera Position = slight skew at the screen edge.\n" +
            "- Hierarchy Levels > Facing Override: set one level to Wall Fixed. POIs at that level stay still, the rest follow you. Reset to Inherit after.\n\n" +
            "UPDATE COST\n" +
            "- Not possible in Scene test: needs frames and time. Test it in Play Mode.\n\n" +
            "WHEN DONE\n" +
            "- Scene-Mode Preview OFF: rig returns to the saved angles, config untouched.";

        private static readonly string OrientationPlaymodeTestGuide =
            "SETUP\n" +
            "- 'Save All to JSON' then 'Copy to StreamingAssets' (Play reads the copy).\n" +
            "- LIVE: once Play is running, change any Orientation value here (a level's Facing Override, or a POI's own Facing X / Y / Z) and the running markers and clusters follow at once, no restart. Nothing is saved by that: stop Play and your edits stay in this window; Save All to JSON (then Copy to StreamingAssets) only to keep them.\n" +
            "- Open your wall's scene, press Play, click into the Game view.\n" +
            "- Editor mock camera (Editor only, not Unity's):\n" +
            "  - W/A/S/D = move. E = up, Q = down.\n" +
            "  - RMB + mouse, Alt + LMB + mouse, or arrow keys = look.\n" +
            "  - Z / C = tilt the phone left / right. What you SEE: the Game window never rotates, the world (wall, markers) rotates around you. (Under the hood the camera rolls.)\n\n" +
            "VERTICAL ALIGNMENT\n" +
            "- Hold Z or C and watch the wall rotate:\n" +
            "  - World Up: markers rotate WITH the wall, they stay upright relative to the real world.\n" +
            "  - Screen Up: markers do NOT rotate, they stay level with the window edges while the wall turns behind them.\n" +
            "- Marker, Label and Badge are independent: e.g. Marker World Up + Label Screen Up = text stays screen-horizontal.\n" +
            "- Clusters: view a dense group of POIs from far away, then roll with Z / C.\n" +
            "- Up Reference: World Gravity = upright. Custom = leans by your vector. Spawn Root = leans with the wall's root.\n\n" +
            "FACING OPTIONS\n" +
            "- Wall Fixed: walk and look around, markers never move.\n" +
            "- Y Rotation Only: circle a marker with A / D, it turns to you, never tips. Z / C has no effect.\n" +
            "- Always Facing Camera: faces you everywhere, also above and below (E / Q). Never flips edge-on close up.\n" +
            "  - Facing Basis: View Plane = parallel to screen. Camera Position = skew at the screen edge.\n" +
            "- Hierarchy Levels > Facing Override: set one level to Wall Fixed, the others follow you.\n\n" +
            "UPDATE COST\n" +
            "- Every Frame: smooth. Interval (e.g. 1 s): markers update in steps.\n" +
            "- On Camera Delta (e.g. 20 deg): markers re-aim only after the camera turns past it.\n\n" +
            "AUTOMATED\n" +
            "- Automated: Test Runner, EditMode + PlayMode, zero failures.";

        private static readonly string OrientationDeviceTestGuide =
            "SETUP (Android)\n" +
            "- USB: enable USB debugging, plug in, accept the prompt, 'adb devices' lists it.\n" +
            "- Wi-Fi: Developer options > Wireless debugging > Pair with code.\n" +
            "  - 'adb pair <ip>:<pair-port>' + code, then 'adb connect <ip>:<port>'.\n" +
            "  - Or 'npx adb-qr-connect' and scan the QR code.\n" +
            "- 'Save All to JSON' + 'Copy to StreamingAssets', then Build Settings > Android > Build And Run.\n" +
            "- No mock camera on device: you move and tilt the phone.\n\n" +
            "LOGS\n" +
            "- 'adb logcat -c', then 'adb logcat -s Unity > __logcat.txt' and read the file.\n" +
            "- Look for [Config], [POI], [Marker] lines and any Exception / Error.\n\n" +
            "VERTICAL ALIGNMENT (main device test: real auto-rotation)\n" +
            "- Point at the wall, rotate the phone to landscape, hold 5 s.\n" +
            "- World Up: markers stay level with the world. Screen Up: they turn with the screen.\n" +
            "- Check Marker, Label, Badge, Clusters: labels horizontal? symbols still circular? Did the app screen rotate?\n" +
            "- Up Reference: only Spawn Root / Custom if the wall's map is tilted.\n\n" +
            "FACING OPTIONS\n" +
            "- Wall Fixed: walk left/right, markers stay glued to the wall.\n" +
            "- Y Rotation Only: markers turn to you, keep the wall tilt.\n" +
            "- Always Facing Camera: faces you, also crouching or very close. Facing Basis: View Plane vs Camera Position, check markers at the screen edge.\n" +
            "- Hierarchy Levels > Facing Override: one level Wall Fixed, the others follow you.\n\n" +
            "UPDATE COST\n" +
            "- Every Frame is the default: 5 min session on the densest wall, watch for lag or heat.\n" +
            "- Interval / On Camera Delta only if too slow; check label offset lag after fast turns.";

        private static readonly Color GlobalSectionColor = new Color(0.35f, 0.55f, 0.95f);

        // Tab button and container colors for the enhanced visual hierarchy.
        private static readonly Color GlobalSceneTabColor = new Color(0.15f, 0.50f, 0.95f); // vivid blue
        private static readonly Color SpecificMarkerTabColor = new Color(0.00f, 0.78f, 0.38f); // vivid green
        private static readonly Color TabTextColor = Color.white;
        private static readonly Color SceneConfigSectionColor = new Color(0.45f, 0.55f, 0.85f);
        private static readonly Color LabelsAndFontsSectionColor = new Color(0.90f, 0.45f, 0.55f);
        private static readonly Color MarkerSectionColor = new Color(0.30f, 0.80f, 0.40f);
        private static readonly Color OrientationSectionColor = new Color(0.50f, 0.50f, 0.95f);
        private static readonly Color BadgeSectionColor = new Color(0.95f, 0.60f, 0.20f);
        private static readonly Color OutlineSectionColor = new Color(0.60f, 0.35f, 0.90f);
        private static readonly Color EffectsSectionColor = new Color(0.20f, 0.65f, 0.90f);
        private static readonly Color HierarchySectionColor = new Color(0.95f, 0.80f, 0.15f);
        private static readonly Color LodSectionColor = new Color(0.00f, 0.70f, 0.70f);
                private static readonly Color ZoomSectionColor = new Color(0.80f, 0.20f, 0.70f);

        // Displacement: its option arrays and texts live in GlobalScene/POIEditorToolWindow.DisplacementHelp.cs
        private static readonly Color DisplacementSectionColor = new Color(0.85f, 0.30f, 0.95f);
        private static readonly Color SearchFilterSectionColor = new Color(0.60f, 0.40f, 0.20f);
        // Specific Marker tab: per-POI header foldouts pick a stable color from
        // PoiHeaderPalette (deterministic FNV-1a hash of the POI id), so concrete
        // POI header titles look varied instead of sharing one crimson. The five
        // inner sub-section titles stay bold but render with the editor's default
        // (uncolored) text via FoldoutDefaultColor.
        private static readonly Color[] PoiHeaderPalette = new Color[]
        {
            new Color(0.95f, 0.25f, 0.25f), // red
            new Color(0.95f, 0.55f, 0.10f), // orange
            new Color(0.95f, 0.78f, 0.15f), // gold
            new Color(0.45f, 0.80f, 0.20f), // lime
            new Color(0.10f, 0.72f, 0.45f), // green
            new Color(0.00f, 0.70f, 0.70f), // teal
            new Color(0.15f, 0.60f, 0.95f), // sky blue
            new Color(0.20f, 0.45f, 0.90f), // blue
            new Color(0.45f, 0.45f, 0.95f), // indigo
            new Color(0.70f, 0.40f, 0.95f), // violet
            new Color(0.90f, 0.30f, 0.70f), // magenta
            new Color(0.95f, 0.20f, 0.55f), // hot pink
        };
        private static Color FoldoutDefaultColor => EditorStyles.foldout.normal.textColor;

        // Resolve a stable per-POI header color by hashing the POI key; the index
        // seed is used as the hash source when the key is null/empty. Deterministic
        // across repaints and editor sessions (no flicker), and reuses
        // CategoryPalette's canonical FNV-1a StableHash so there is one hash
        // implementation site shared with runtime category coloring.
        private static Color PoiHeaderColorFor(string poiKey, int fallbackSeed)
        {
            Color[] p = PoiHeaderPalette;
            if (p == null || p.Length == 0)
                return new Color(0.80f, 0.22f, 0.28f); // defensive fallback = old crimson
            string seed = string.IsNullOrEmpty(poiKey) ? fallbackSeed.ToString() : poiKey;
            int idx = (CategoryPalette.StableHash(seed) & 0x7FFFFFFF) % p.Length;
            return p[idx];
        }

        // The Marker Table's trash glyph -- also the default icon for the shared
        // DeleteButton (Shared/POIEditorToolWindow.DeleteButton.cs), so every delete
        // affordance in the window uses the same glyph by construction.
        private static GUIContent _trashIcon;
        internal static GUIContent TrashIcon => _trashIcon ?? (_trashIcon = EditorGUIUtility.IconContent("d_TreeEditor.Trash"));

        // Info glyph reused for read-only explanation buttons. Unity native
        // icon first, bundled PNG fallback -- mirrors the pencil pattern so it
        // survives domain reloads and never depends on editor skin names.
        public const string InfoIconAssetPath = "Assets/Framework/Editor/POIEditor/Shared/Icons/info-icon.png";

        private static GUIContent _infoIcon;
        public static GUIContent InfoIcon
        {
            get
            {
                if (_infoIcon != null)
                    return _infoIcon;

                var native = EditorGUIUtility.IconContent("_Help");
                _infoIcon = native != null && native.image != null
                    ? native
                    : new GUIContent(AssetDatabase.LoadAssetAtPath<Texture2D>(InfoIconAssetPath));
                return _infoIcon;
            }
        }

        // Select glyph for the curated "Choose existing" picker button.
        // Unity native assign icon first, bundled PNG fallback -- same caching
        // pattern as TrashIcon / InfoIcon so it survives domain reloads.
        // NOTE: the native probe uses FindTexture, NOT IconContent: on Unity 6
        // (6000.x) "ProjectAssign" is not a registered skin name, and
        // IconContent(name) LOGS "Unable to load the icon" for unknown names on
        // every call before falling back. FindTexture returns null silently.
        public const string SelectIconAssetPath = "Assets/Framework/Editor/POIEditor/Shared/Icons/select.png";

        private static GUIContent _selectIcon;
        public static GUIContent SelectIcon
        {
            get
            {
                if (_selectIcon != null)
                    return _selectIcon;

                var nativeTexture = EditorGUIUtility.FindTexture("ProjectAssign");
                _selectIcon = nativeTexture != null
                    ? new GUIContent(nativeTexture)
                    : new GUIContent(AssetDatabase.LoadAssetAtPath<Texture2D>(SelectIconAssetPath));
                return _selectIcon;
            }
        }

        // Details glyph for the per-row edit-note buttons. Uses the bundled PNG
        // (explicitly requested asset) with a text fallback if it ever goes
        // missing -- unlike Info/Select there is no reliable native "three dots".
        public const string DetailsIconAssetPath = "Assets/Framework/Editor/POIEditor/Shared/Icons/details.png";

        private static GUIContent _detailsIcon;
        public static GUIContent DetailsIcon
        {
            get
            {
                if (_detailsIcon != null)
                    return _detailsIcon;

                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(DetailsIconAssetPath);
                _detailsIcon = tex != null ? new GUIContent(tex, "Edit details") : new GUIContent("...");
                return _detailsIcon;
            }
        }

        // Pencil glyph for the per-POI header rename button. Loaded by asset
        // path (not EditorGUIUtility.Load) so it survives domain reloads and
        // never depends on the stray top-level Assets/Editor copy.
        internal const string EditIconAssetPath = "Assets/Framework/Editor/POIEditor/SpecificMarker/Icons/edit-icon.png";

        // Crosshair glyph for the per-POI header focus button (selects + frames the marker).
        internal const string FocusIconAssetPath = "Assets/Framework/Editor/POIEditor/SpecificMarker/Icons/focus-icon.png";

        // Green plus glyph for the per-POI header ADD-NEAR button (adds a new POI below
        // this one, using it as the template).
        internal const string AddIconAssetPath = "Assets/Framework/Editor/POIEditor/SpecificMarker/Icons/add-icon.png";

        private static GUIContent _addIcon;
        internal static GUIContent AddIcon
        {
            get
            {
                if (_addIcon != null)
                    return _addIcon;

                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(AddIconAssetPath);
                _addIcon = new GUIContent(tex, "Add a new POI below this one, copied from it");
                return _addIcon;
            }
        }

        // POI header row icon-cluster help ((i) button between "+" and delete).
        internal static readonly string PoiHeaderIconsHelpBody =
            "What each icon on this row does, left to right after the name:\n\n" +
            "Crosshair (focus): selects this POI's marker and moves the Scene view camera " +
            "to frame it, without expanding this POI's foldout.\n\n" +
            "Pencil: renames this POI (edits its display name only).\n\n" +
            "Up / Down arrows: reorders this POI earlier or later in the list. This is the " +
            "config's own save order, not a spatial position.\n\n" +
            "+ (add near): adds a new POI directly below this one, copied from it (category, " +
            "hierarchy level, rotation, etc). Use the crosshair on the new row, then Unity's " +
            "normal Move tool, to place it where you actually want it.\n\n" +
            "Trash (delete): permanently removes this POI from the config and from the Scene " +
            "rig, after a confirmation prompt.";

        // Position & rotation setup help (Position foldout title row, (i) button).
        internal static readonly string PositionSetupHelpBody =
            "How to place this marker, step by step:\n\n" +
            "1. Select it. A newly added POI is auto-focused already; otherwise click the " +
            "crosshair icon on this POI's title row to select it and frame it in the Scene view.\n\n" +
            "2. Move it. Use Unity's normal Move tool (same as any other GameObject) to drag the " +
            "marker in the Scene view to where it should sit on the wall.\n\n" +
            "3. Lock it in. Once the position looks right, click 'Verified' (next to this help " +
            "button) to lock it -- turns green. A verified position is protected: if the marker " +
            "gets bumped in the Scene view afterward, it snaps back automatically until you " +
            "unlock it again by clicking Verified a second time.\n\n" +
            "Verified also locks facing: while green, the three Facing sliders are disabled and " +
            "rotating the marker in the Scene view (or the Inspector) snaps it back with a notice. " +
            "Click Verified again to unlock. Whether the angles matter at runtime depends on the " +
            "wall's global Facing Options mode (Global Scene > Orientation) -- see the Facing " +
            "row's own help button for which mode reads which axis.";

        // Facing Options row help ((i) button next to the slider itself).
        internal static readonly string EditRotationHelpBody =
            "Three sliders set this marker's authored facing: X (pitch), Y (yaw) and Z (roll), in " +
            "degrees. They are always identical -- you decide which ones matter for your wall.\n\n" +
            "Which angles are used at runtime depends on the wall's Facing Options mode " +
            "(Global Scene > Orientation > Facing Options):\n" +
            "- Always Facing Camera: none of the three are used -- the marker always fully faces " +
            "the visitor. They only tilt the marker in the Scene view.\n" +
            "- Y Rotation Only: X and Z (tilt) are used as authored; Y is replaced every frame by " +
            "the live camera-facing yaw, so Y only sets a starting yaw for editor preview.\n" +
            "- Wall Fixed: all three (X, Y, Z) are used exactly as authored -- this marker never " +
            "turns toward the visitor, so set all three carefully (e.g. glued flat to a wall).\n\n" +
            "Sync: the sliders, Unity's Rotate tool and the Inspector Transform all stay in sync, " +
            "in both directions. Angles live in memory until you click Save, which writes them to " +
            "config.json.\n\n" +
            "Preview warnings: with Scene-Mode Preview ON (Global Scene > Orientation) the Scene view " +
            "shows what the runtime would show, so a notice appears over the Scene view when an axis " +
            "you change is overridden: every axis in Always Facing Camera, only Y in Y Rotation Only, " +
            "none in Wall Fixed. With the preview OFF you always see the raw authored angles. Tick the dialog's \"do not show again\" checkbox to hide it; TileStories > Reset Hidden Notices brings it back.\n\n" +
            "Lock: once this POI is Verified (green) the sliders are disabled and any rotation " +
            "made in the Scene view snaps back with a notice. Click Verified to unlock.\n\n" +
            "Defaults: the first POI starts at (0, 0, 0). A POI added with the + icon copies all " +
            "three angles from the POI it was added from.\n\n" +
            "Auto-open: moving or rotating a marker in the Scene view opens its section here " +
            "and scrolls to it.";
    }
}

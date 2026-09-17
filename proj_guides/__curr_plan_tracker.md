# Current Task Tracker - POI Editor Layout + Full X/Y/Z Editor Rotation

Date: 2026-09-17. Status: RESOLVED. Compile clean (zero error CS); EditMode 703/703;
PlayMode 64/64. Durable write-up: proj_guides/_5.1_Editor_Tab.md (2026-09-17 blocks).

## Pass 1 - layout + rotation (all verified green)

a) Add-POI button wording: top and bottom buttons are two GENUINELY different operations
   (insert-before-next vs insert-after-previous), not duplicated logic. Only the wording
   was confusing: now `+ Add POI near next v` / `+ Add POI near previous ^` /
   `+ Add first POI` for the empty case. The separator pair already used "near".

b) Focus duplicated into the POI title row as an icon (focus-icon.png), between the
   rename pencil and the reorder arrows. Same CanFocusPoiInScene guard. A COLLAPSED POI
   is now focusable in one click.

c) The big "Focus in Scene" row was removed; its (i) help moved onto the Position foldout
   title row and rewritten to the full placement flow (focus -> Move -> Rotate ->
   Verified). DrawFramedFoldout gained an optional drawHeaderTrailing action. Implementation
   note: EditorGUILayout.Foldout accepts NO GUILayoutOption, so the slot reserves a
   fixed-width rect via GUILayoutUtility.GetRect and draws with EditorGUI.Foldout.

d) Full X/Y/Z editor rotation, synced with the Scene rotate gizmo.
   - POIData gained editor_rotation_x_deg + editor_rotation_z_deg; editor_rotation_deg is
     KEPT as Y (not renamed) because JsonUtility does no name mapping. Old configs load x/z=0.
   - PoiRotationResolver.ToEulerQuaternion(x,y,z) applies all three; ToYawQuaternion(y) is a
     thin wrapper so existing callers/tests keep working.
   - All 6 apply sites use the Euler form.
   - Two-way sync: HandleSceneGui (on SceneView.duringSceneGui) reads the child's
     localRotation.eulerAngles back into config for unverified POIs via
     SyncPoiRotationFromScene -- deliberately in OnSceneGUI, OUTSIDE the window's
     DrawConfigMutationScope, so it cannot feed back through the JSON diff and spam undo
     history on every Scene repaint.
   - CapturePositions also reads localRotation.eulerAngles into all three fields, so
     scene-rotate edits persist to JSON on Save.
   - Verified POIs keep the old lock (rotation reverts with position).

e) Invisible X/Y/Z coordinate letters -- root cause: they were rendered but effectively
   invisible (miniBoldLabel in a DisabledGroup in the dark theme) and later the row
   overflowed the panel, squeezing the label to a sliver.

f) Verified button indented 2 levels deeper (IndentLevelScope(2)) so it reads as the
   final commit action rather than a 4th coordinate line.

## Pass 2 - refinement follow-ups (verified green)

1. Delete button: same headerRect.height as sibling buttons (dropped fixedHeight=24f);
   danger affordance moved from the dark-red FILL to the red ICON ITSELF
   (SpecificMarker/Icons/delete-icon.png via DeleteIcon, ImageOnly). Confirm dialog kept.
   The taxonomy tables still use the native TrashIcon.
2. Coordinates compacted into ONE row: `x [val]  y [val]  z [val]` -- each axis letter
   drawn at a fixed 14px immediately left of its own value field, the three pairs sharing
   the row width equally (value = max(60, (rowWidth - 3*14 - 2*6)/3)). No letter can be
   stranded or squeezed, and the position reads as a single line.
3. Row width rule now counts the INDENT: EditorRowWidthForIndent(viewWidth, rightMargin,
   indent) computes rightLimit = min(visible panel, MaxRowWidth) measured from x=0 and
   returns max(MinRowWidth, rightLimit - indent). So a deeper indent NARROWS the content
   while every row in the window ends at the same x. The pure EditorRowWidth helper keeps
   its signature (guarded by EditorRowWidth_PureRule) and the three render tests now call
   the shipped formula instead of re-deriving the old arithmetic.
   FULL SCAN RESULT: all 52 DrawEditorRow call sites (Global Scene fields + add buttons,
   LOD/Zoom, Search & Filter, AssetPaths path row, Specific Marker header + per-POI
   dropdowns/status rows, keyword rows, Position tab rows) go through DrawEditorRow, and
   DrawEditorRow is the ONLY place the width is computed. No call site overrides the
   computed rowWidth with a hard-coded MaxRowWidth-style cap, so the indent-aware budget
   now applies to every row, including the indented Global Scene rows.
4. (i) after "Position": left-aligned a fixed 40f after the title. Root cause of the
   earlier right-pinning: a default EditorGUILayout.Foldout eats the whole row, so the
   Space was irrelevant. Fixed by reserving a fixed-width rect (title + 20f) via
   GUILayoutUtility.GetRect and drawing with EditorGUI.Foldout; the (i) then lands a
   fixed step after the title. Caught as CS1501 on the first attempt (EditorGUILayout
   overload has no options parameter) and corrected.
5. Second foldout arrow removed: the name label was drawn with the foldout STYLE, whose
   normal/onNormal background IS the foldout-arrow texture, so LabelField painted a second
   arrow next to the real one. Now a separate CreateHeaderLabelStyle (boldLabel + header
   color) is used for the name text; the foldout keeps CreateFoldoutStyle.

## Files touched (both passes)
- Editor/POIEditor/SpecificMarker/PoiRotationResolver.cs  (ToEulerQuaternion, NormalizeAngleDeg)
- Editor/POIEditor/SpecificMarker/POIEditorToolWindow.SpecificMarker.cs  (header focus + delete icons, removed focus row, Position (i), add-button labels, header label style, x/z inherit on copy)
- Editor/POIEditor/SpecificMarker/POIEditorToolWindow.PositionTabs.cs  (one-row coordinates, verified indent)
- Editor/POIEditor/POIEditorToolWindow.cs  (DrawFramedFoldout trailing slot + fixed-width foldout rect, CreateHeaderLabelStyle, HandleSceneGui sync, SyncPoiRotationFromScene)
- Editor/POIEditor/POIEditorToolWindow.Constants.cs  (FocusIconAssetPath, DeleteIconAssetPath/DeleteIcon, PositionSetupHelpBody)
- Editor/POIEditor/Shared/POIEditorToolWindow.RowLayout.cs  (EditorRowWidthForIndent)
- Editor/POIEditor/RigLifecycle/POIEditorToolWindow.RigLifecycle.cs  (3 apply sites + CapturePositions rotation capture)
- Runtime/Core/WallConfigData.cs  (editor_rotation_x_deg / editor_rotation_z_deg)
- Tests/Editor/PoiRotationResolverTests.cs  (+7: euler math, normalize, sync round-trip, no-op)
- Tests/Editor/POIEditorTableLayoutTests.cs  (+1 right-edge invariant test)
- Tests/Editor/POIEditorAddButtonRowRenderTests.cs  (3 render expectations now call the shipped formula)
- Tests/Editor/POIEditorVisualHierarchyTests.cs  (DrawFramedFoldout signature guard 4 -> 5)
- Assets: SpecificMarker/Icons/focus-icon.png + delete-icon.png (supplied by dev)
- Docs: .clinerules/10-structure.md, proj_guides/_5.1_Editor_Tab.md (two 2026-09-17 blocks)

## Test evidence
- Compile: zero error CS (one CS1501 guess was caught by the compiler and fixed).
- Pass 1: EditMode 699 green; PlayMode 64 green. One guard caught the foldout signature
  change (4 -> 5 params) and was updated.
- Pass 2: EditMode 700 green (added EditorRowWidthForIndent_RightEdgeStaysAlignedAcrossIndents:
  right edge identical at 0/15/30px indents on wide and narrow panels, and content
  actually shrinks as the indent deepens); PlayMode 64 green.
- Caveat: Tier-0 only -- the rendered IMGUI layout still needs a human glance.

## Pass 3 - the invisible X/Y/Z letters (root cause) + row right margin (RESOLVED)
Compile clean; EditMode 703/703 (700 + 3 new render tests); PlayMode 64/64.

Letter-invisibility ROOT CAUSE: the coordinate row was the ONLY row in the window that
drew its label inside EditorGUI.BeginDisabledGroup(true). Every other label in that same
foldout (Edit Rotation, Category, Has status) uses the SAME EditorStyles.boldLabel but is
un-disabled and renders fine -- so the disabled scope tinted the letters invisible.
Fix: DrawCoordinateRow keeps axis letters as ordinary UN-disabled GUILayout.Labels and
wraps ONLY the value fields in EditorGUI.DisabledScope(true). Row stays one compact line
x [val]  y [val]  z [val] (letters 16px, adjacent; three pairs share row width equally).
Factored out as DrawCoordinateRow(x,y,z, out firstLabel, out firstValue, out lastValue)
so a real OnGUI render test can drive it.

NEW render test POIEditorCoordinateRowRenderTests.cs: drives production DrawCoordinateRow
in a real EditorWindow OnGUI pass and asserts the ACTUAL GUILayoutUtility rects:
label width >8px (not a sliver), label <10px left of its value field, whole row fits a
900px and a 300px panel. Geometry now mechanically proven.

Row-right-margin: AddButtonRowRightMargin 6f -> 20f so rows clear the ScrollView vertical
scrollbar (~15px). Before, the row edge landed at view-6 = under the scrollbar, hiding the
last pixels of every row when the list scrolled. This is the right-spacing inside the
shared row budget (indent + elements + right margin), applied once for every row -- no
per-row trailing spacer needed. Render tests read the const via reflection so they
self-adjust.

## Previous task (RESOLVED 2026-09-16): taxonomy identity rename single-source-of-truth
Generalized IdentityRenameResolver + IdentityRenameEditState; wired badge-key and
search-field-key renames to propagate; added delete guards; removed redundant dirty flags.
Full write-up lives in proj_guides/_5.1_Editor_Tab.md (2026-09-16 block).
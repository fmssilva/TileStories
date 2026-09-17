# Current Task Tracker - Specific Marker Layout + Full X/Y/Z Editor Rotation

Date: 2026-09-17. Status: RESOLVED. Compile ExitCode 0; EditMode 700/700; PlayMode 64/64.

## What was asked, and what each item actually was (checked on disk first)

a) Add-POI button wording. The top (+ Add POI before next) and bottom (+ Add POI after
   previous) buttons are two GENUINELY different operations: insert-before-next vs
   insert-after-previous -- NOT a duplicated button. They only READ confusingly, so the
   fix was naming only: `+ Add POI near next v`, `+ Add POI near previous ^`, and
   `+ Add first POI` for the empty case. The separator pair already used "near".

b) Focus duplicated into the POI title row as a simple icon (focus-icon.png), placed
   between the rename pencil and the up/down reorder arrows. Same CanFocusPoiInScene
   disabled-guard as before. A COLLAPSED POI is now focusable in one click.

c) The big "Focus in Scene" row was removed entirely, and its (i) help moved onto the
   Position foldout title row. Help text rewritten from "what the button does" to the
   full placement flow: focus via crosshair -> Move tool to place -> Rotate tool to tilt
   -> Verified to lock. DrawFramedFoldout gained an optional drawHeaderTrailing action.

d) Full X/Y/Z editor rotation, synced with the Scene rotate gizmo.
   - POIData gained editor_rotation_x_deg + editor_rotation_z_deg. editor_rotation_deg is
     KEPT as the Y axis (not renamed) because JsonUtility does no name mapping: renaming
     it would silently drop yaw from every existing config.json. Old configs load x/z=0.
   - PoiRotationResolver.ToEulerQuaternion(x,y,z) applies all three; ToYawQuaternion(y) is
     now a thin wrapper so existing callers/tests keep working.
   - All 6 apply sites switched to the Euler form (RefreshRigVisuals x2, PopulateRig,
     SpawnAndFocusNewPoi, ApplyPoiEditorRotation, verified-lock revert).
   - Two-way sync: HandleSceneGui (already on SceneView.duringSceneGui) reads the child's
     localRotation.eulerAngles back into config for unverified POIs via
     SyncPoiRotationFromScene. Deliberately in OnSceneGUI -- OUTSIDE the window's
     DrawConfigMutationScope -- so it cannot feed back through the JSON diff and spam undo
     history / rig refreshes on every Scene repaint.
   - CapturePositions also reads localRotation.eulerAngles into all three fields, so
     scene-rotate edits persist to JSON on Save.
   - Verified POIs keep the old lock: rotation reverts together with position.

e) Invisible X/Y/Z coordinate labels -- root cause found. They were rendered with
   EditorStyles.miniBoldLabel at 28px, effectively invisible in the dark theme (matches the
   screenshot: three value boxes, no visible axis letters). Fixed to boldLabel at 34px with
   the value width adjusted (coordRow - 40f).

f) Verified button indented 2 levels deeper than the coordinate rows (IndentLevelScope(2)
   around its DrawEditorRow) so it reads as the final commit action, not a 4th coordinate.

## Files touched
- Editor/POIEditor/SpecificMarker/PoiRotationResolver.cs  (ToEulerQuaternion, NormalizeAngleDeg)
- Editor/POIEditor/SpecificMarker/POIEditorToolWindow.SpecificMarker.cs  (header focus icon, removed focus row, Position (i), add-button labels, x/z inherit on copy)
- Editor/POIEditor/SpecificMarker/POIEditorToolWindow.PositionTabs.cs  (bold axis labels, verify indent)
- Editor/POIEditor/POIEditorToolWindow.cs  (DrawFramedFoldout trailing slot, HandleSceneGui sync, SyncPoiRotationFromScene)
- Editor/POIEditor/POIEditorToolWindow.Constants.cs  (FocusIconAssetPath, PositionSetupHelpBody)
- Editor/POIEditor/RigLifecycle/POIEditorToolWindow.RigLifecycle.cs  (3 apply sites + CapturePositions rotation capture)
- Runtime/Core/WallConfigData.cs  (editor_rotation_x_deg / editor_rotation_z_deg)
- Tests/Editor/PoiRotationResolverTests.cs  (+7 tests: euler math, normalize, sync round-trip, no-op)
- Tests/Editor/POIEditorVisualHierarchyTests.cs  (DrawFramedFoldout signature guard 4 -> 5)
- Assets/Framework/Editor/POIEditor/SpecificMarker/Icons/focus-icon.png  (new, supplied by dev)
- Docs: .clinerules/10-structure.md, proj_guides/_5.1_Editor_Tab.md

## Test evidence
- First layout pass: EditMode 699 green / PlayMode 64 green. The only failure on the
  way was a signature-guard test asserting DrawFramedFoldout had exactly 4 parameters
  (the guard doing its job after the optional trailing-help param was added); updated
  to 5 and re-ran green.
- Follow-up refinement pass: EditMode 700 passed / 0 failed (699 + 1 new
  `EditorRowWidthForIndent_RightEdgeStaysAlignedAcrossIndents`). The three moved
  render-test expectations now call the shipped `EditorRowWidthForIndent` instead of
  re-deriving the old arithmetic. PlayMode 64 passed / 0 failed.

## Previous task (RESOLVED 2026-09-16): taxonomy identity rename single-source-of-truth
Generalized IdentityRenameResolver + IdentityRenameEditState; wired badge-key and
search-field-key renames to propagate; added delete guards; removed redundant dirty flags.
Full write-up lives in proj_guides/_5.1_Editor_Tab.md (2026-09-16 block) and the git history
for that session.
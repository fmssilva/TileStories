# Project Structure and Organizing Principles

Read this file to get oriented on the project's structure. It is the map: what
exists, where it lives, and what each piece does in one line. Full tree verified
against disk on 2026-09-18.

## 0. How to read and edit this file

- The tree in section 5 uses ONLY plain `-` bullets and two-space indentation.
  No box-drawing characters, no tables, no unicode. Any agent editing it must
  keep it that way so weaker tools cannot corrupt it.
- Every folder and every first-party file carries a one-line description after
  ` -- `. Names alone are not enough: the description is what lets an agent
  decide whether to open a file without opening it.
- Third-party and Unity-generated folders (TextMesh Pro, UI Toolkit, XR,
  Settings, Plugins) are summarised at folder level only. Do not expand them
  file by file; nothing in this project edits them by hand.
- ASCII only, everywhere.

## 1. Organizing principles

### 1.1 Two top-level areas: Framework and Apps

- `Assets/Framework/` contains code that behaves the same for every wall: AR
  session bootstrap, tracking abstractions, POI data and rendering, UI shells,
  content-card rendering, analytics, and the guide character system.
- `Assets/Apps/<WallName>/` contains one self-contained wall folder per app: POI
  list, category taxonomy, localization/map files, and wall media.
- A system moves from an app-specific folder into Framework only when a second
  wall needs the exact same thing unchanged.
- Nothing in `Framework/` may reference anything inside a specific `Apps/<WallName>/` folder.
- No wall folder references another wall folder.

### 1.2 Editor code is physically separated from Runtime code

- `Assets/Framework/Runtime/` is shipped in the build.
- `Assets/Framework/Editor/` only runs in the Unity Editor.
- Assembly Definition files enforce this split.
- Runtime code must never reference Editor code.
- If runtime code needs editor-only behavior, move that logic into the Editor
  assembly instead of adding a runtime dependency.

### 1.3 Menu-item and tool visibility

- Developer-facing per-wall configuration belongs in the POI Editor window.
- Framework-internal repair tools should not appear as ambiguous top-level menu items.
- One-time asset scaffolding can be a menu item, but it must be clearly scoped.
- When adding a `[MenuItem("TileStories/...")]`, ask whether a wall developer
  would actually need to click it.

### 1.4 Domain-centered folders, not type-centered folders

- Group files by what they do, not by generic technical buckets.
- Do not create catch-all utility files that accumulate unrelated methods.
- Test folders live alongside the code they test inside `Framework/Tests/`.

### 1.5 The UI system boundary is spatial

- World-space, physically anchored to the AR wall -> uGUI World-Space Canvas.
  In practice this is `Runtime/UI/Markers/` and nothing else.
- Everything screen-space -> UI Toolkit (`UIDocument` + `.uxml` + `.uss`).
  Every other folder under `Runtime/UI/` follows this.

## 2. How to read this structure

- The key boundary is `Framework/` versus `Apps/`: Framework never knows about a
  specific wall.
- The second key boundary is `Framework/Runtime/` versus `Framework/Editor/`:
  runtime ships to devices, editor code does not.
- Several folders exist but are still empty: they are deliberate scaffolds for
  later stages, marked `[EMPTY SCAFFOLD]` in the tree. An empty folder is not a
  bug and must not be deleted.

## 3. Stage-by-stage construction sequence

- Stage 0: docs, mock deliverables, sketches
- Stage 1: Core, Tracking, POI, UI/Scanning, UI/Cards scaffold, UI/Onboarding, Telemetry scaffold, first wall app
- Stage 2: Blocks, UI/Markers, LOD, Zoom, Lapse timeline, state manager
- Stage 3: Circuits, UI/Circuits, UI/Gamification, UI/GuideCharacter, UI/Navigation, UI/Sharing, optional AI
- Stage 4: Editor/Validation, Editor/Baker, Tests
- Stage 5: Editor/Wizard
- Stage 6: package extraction

## 4. Standing facts about the project

- `LivingRoom` is the only wall with real, working content. `Chafariz/`, `Mural/`,
  `Panorama/` and `TestWall/` are empty scaffolds.
- `Assets/StreamingAssets/LivingRoom/config.json` is the runtime-readable copy of
  `Assets/Apps/LivingRoom/config.json`. The POI Editor writes the first and
  copies to the second.
- `Assets/Dev/` is intentional development infrastructure, not garbage. It holds
  the Phase A isolated gallery scenes (`40-testing.md` section 4.4).
- Assemblies: `TileStories` (runtime), `TileStories.Editor`,
  `TileStories.Tests.Runtime` (PlayMode), `TileStories.Editor.Tests` (EditMode).
- The five `Assets/InitTestScene<guid>.unity` files at the project root are Unity
  Test Framework leftovers, not authored content. Safe to delete; do not build on them.

## 5. Complete project file structure

- Assets/
  - Apps/  -- one self-contained folder per wall; no wall references another
    - LivingRoom/  -- the only wall with real content; the dev + test wall
      - config.json  -- authoring source of truth: wall settings, taxonomies, POI list
      - config.json.backup  -- manual safety copy of the above
      - LivingRoomScene.unity  -- the playable AR scene for this wall
      - 146267-LivingRoom2.bytes  -- Immersal map; filename prefix IS the map id
      - 146267-LivingRoom2-tex.glb  -- textured mesh of the mapped space, for visual reference
      - generate_config.py  -- one-off script that seeded the initial POI list
      - MarkerAssets/
        - Resources/
          - MarkerSymbols/  -- must stay under Resources/ so runtime Resources.Load finds it
            - Wall_IconLibrary.asset  -- per-wall sprite-key to sprite lookup
            - living_room_IconLibrary.asset  -- same, for the living-room POI set
            - test_wall_IconLibrary.asset  -- same, for the test wall
      - MediaAssets/  [EMPTY SCAFFOLD]  -- Audio/ Images/ Models3D/ Videos/ for POI content
    - Chafariz/  [EMPTY SCAFFOLD]  -- circular outdoor wall, primary evaluation site
    - Mural/  [EMPTY SCAFFOLD]  -- flat outdoor wildlife mural, third test wall
    - Panorama/  [EMPTY SCAFFOLD]  -- U-shaped indoor tile panorama
    - TestWall/  [EMPTY SCAFFOLD]  -- throwaway wall for editor experiments
  - Dev/  -- Phase A isolated harness scenes; excluded from Build Settings
    - MarkerGallery/
      - MarkerGalleryScene.unity  -- grid of every marker visual variant, no AR, no config
      - Backdrops/
        - backdrop.png  -- the ONE shared backdrop every uGUI gallery scene reuses
      - Screenshots/
        - marker_gallery_phase3.png  -- captured Tier 1 vision-pass reference
    - ClusterGallery/
      - ClusterGalleryScene.unity  -- grid of cluster-aggregate variants
    - OrientationGallery/
      - OrientationGalleryScene.unity  -- rotatable camera rig (orbit/pitch/roll) for hand-testing every Vertical Alignment / Facing Options combination live; drives OrientationGalleryHarness, excluded from Build Settings
    - PoiEditorLayout/  [EMPTY SCAFFOLD]  -- saved POI Editor window layouts
  - Framework/
    - Runtime/  -- ships to device
      - TileStories.asmdef  -- runtime assembly; refs Unity.InputSystem, Immersal.Core, Unity.TextMeshPro
      - AssemblyInfo.cs  -- InternalsVisibleTo so test assemblies can reach internals
      - Core/  -- config model, loading, and the session that boots a wall
        - WallConfigData.cs  -- the whole config.json object model: WallConfigData, POIData, PositionData, taxonomy entries, LodSettings, DisplacementSettings, EffectDefaults
        - WallConfigLoader.cs  -- reads config.json from StreamingAssets and deserializes it
        - WallSession.cs  -- boots one wall: load config, wait for tracking, spawn markers, expose settings to LOD/zoom. Delegates every decision; holds none
        - MarkerShape.cs  -- enum of symbol silhouettes (circle, rounded square, hexagon, diamond, star, none)
        - MarkerOutlineMode.cs  -- enum for whether and how the status contour ring renders
        - MarkerStyle.cs  -- LEGACY combined style enum, kept for old configs
        - MarkerVisualsParser.cs  -- parses config strings into the enums above with safe fallbacks
      - Tracking/  -- localisation, the editor mock, and AR zoom
        - IWallTracker.cs  -- the tracking seam: OnWallLocalised(Pose)
        - ImmersalWallTracker.cs  -- real implementation wrapping the Immersal Localizer
        - MockLocalizationProvider.cs  -- Tier A editor mock: fires localised immediately, adds WASD + mouse look
        - EditorCameraLook.cs  -- pure yaw/pitch math for the mock camera; no scene needed
        - ARZoomState.cs  -- current zoom factor and the effective-distance rule it implies
        - ARZoomMath.cs  -- pure pinch, double-tap and step-cycle math
        - ARZoomGestureInput.cs  -- reads touch input and calls the controller
        - ARZoomController.cs  -- applies zoom to the camera FOV; the only writer of zoom state
      - POI/  -- pure resolvers and the LOD pipeline; mostly plain C# classes
        - POIPositionResolver.cs  -- POI to local Vector3; a null position resolves to origin
        - CategoryPalette.cs  -- category string to color + icon key
        - BadgeCategoryPalette.cs  -- badge key to badge icon + tint
        - StatusRamp.cs  -- status percentage to ring color and dash style
        - MarkerHierarchyResolver.cs  -- hierarchy level key to size, label visibility, effects, reveal timing
        - MarkerOverlapResolver.cs  -- screen-space overlap detection and the three displacement algorithms
        - DisplacementTieBreakStrategy.cs  -- decides which marker of an overlapping pair moves
        - ClusterGrouping.cs  -- deterministic grouping of dense markers into cluster aggregates
        - LODController.cs  -- the per-cycle pipeline: distance banding, culling, density response, clustering, displacement
        - POISearchIndex.cs  -- builds and queries the POI keyword index
        - SearchTokenizer.cs  -- shared tokenisation for index build and query time
        - SynonymGroup.cs  -- synonym expansion, stored as plain config data
      - UI/
        - Markers/  -- the ONLY uGUI World-Space Canvas domain in the project
          - POI_Marker.prefab  -- the marker outer container. Root holds Canvas + POIAnchor + MarkerView + MarkerBillboard + MarkerRevealEffect + MarkerSelectable + MarkerLeaderLine; children are Label, Symbol, Ring, Badge. Label and Badge each also carry MarkerChildOrientation (see _2.1)
          - POI_Cluster.prefab  -- LOD aggregate marker; children PieContainer, CountLabel, BackgroundImage, DominantIcon. Root also carries MarkerBillboard (see _2.1 section 6.4)
          - POIAnchor.cs  -- holds this marker's POIData so other components can read it
          - MarkerView.cs  -- applies all visual state to the prefab's children; owns label and marker displacement write-back
          - MarkerBillboard.cs  -- orientation of the marker root, Vertical Alignment + Facing Options (see _2.1). Referenced from POI_Marker.prefab by script GUID -- do NOT rename
          - MarkerOrientationResolver.cs  -- pure, stateless orientation math (see _2.1): ScreenUpWorld, ResolveVerticalUp, ResolveRootRotation, ResolveChildLocalRotation, RootRollDeg, Rotate2D, ClampPitch, ShouldUpdate, ResolveUpReference
          - MarkerChildOrientation.cs  -- per-child (Label/Badge) independent vertical-alignment counter-rotation (see _2.1); baked onto POI_Marker.prefab's Label and Badge children
          - MarkerLayout.cs  -- symbol/label/badge sizing ratios and ScreenPixelsToWorld conversion
          - MarkerCircleGlyphView.cs  -- draws one circular glyph (symbol or badge) from a sprite + tint
          - MarkerCircleSpriteFactory.cs  -- generates circle and ring textures once, shared by every effect
          - MarkerRingView.cs  -- the status contour ring, including its optional continuous spin
          - MarkerClusterView.cs  -- builds a cluster's pie chart, count label and dominant icon
          - MarkerLeaderLine.cs  -- world-space LineRenderer from a displaced marker back to its true position
          - MarkerSelectable.cs  -- tap target; raises selection through the event bus
          - SelectionEventBus.cs  -- static relay so selection has one source of truth across scenes
          - SelectionHighlightController.cs  -- single writer of marker highlight alpha, so nothing fights over CanvasGroup
          - ZoomOnSelectController.cs  -- zooms the camera toward a newly selected marker
          - MarkerEffect.cs  -- base class for optional per-marker animated effects
          - MarkerEffectFlags.cs  -- bitflags naming which effects a hierarchy level enables
          - MarkerPulseEffect.cs  -- gentle scale pulse
          - MarkerGlowEffect.cs  -- soft glow behind the symbol; hero tier only
          - MarkerSunEffect.cs  -- radiating sun burst, ring or filled variants
          - MarkerAccentEffect.cs  -- accent halo with breathe and beacon modes
          - MarkerParticleEffect.cs  -- particle emission around a marker
          - MarkerRevealEffect.cs  -- delayed fade and scale-in on spawn; snaps to full in Edit Mode
          - SpriteKeyLibrary.cs  -- the sprite-key lookup contract shared by icon and shape libraries
          - IconLibrary.asset  -- framework default category and badge icons
          - ShapeLibrary.asset  -- framework default symbol silhouettes
          - SymbolCircle.png  -- the default circular symbol sprite
          - Icons/  -- 14 default category and damage-state icon PNGs
          - Rings/  -- 5 contour ring PNGs: solid, long/medium/short dash, dotted
          - Shapes/  -- 4 non-circle symbol silhouettes: diamond, hexagon, rounded square, star
        - Cards/  -- the POI detail bottom sheet (UI Toolkit)
          - DetailCardView.cs  -- binds a selected POI into the card; proves selection to detail
          - DetailCard.uxml  -- card layout
          - DetailCard.uss  -- card styling
        - Filter/  -- facet filter tray (UI Toolkit)
          - FilterFacetEvaluator.cs  -- pure facet matching and the relax-filters suggestion
          - FilterTrayView.cs  -- the tray UI and its toggles
          - FilterTrayView.uxml  -- tray layout
          - FilterTrayView.uss  -- tray styling
        - Results/  -- search results list (UI Toolkit)
          - ResultsListView.cs  -- maps SearchResult into displayable rows
          - ResultsListView.uxml  -- list layout
          - ResultsListView.uss  -- list styling
        - Navigation/  -- search overlay, voice search and view-mode switching (UI Toolkit)
          - SearchOverlayView.cs  -- the search overlay's input policy and presentation
          - SearchOverlayView.uxml  -- overlay layout
          - SearchOverlayView.uss  -- overlay styling
          - SearchInputGuard.cs  -- stateless input validation and debounce policy
          - RecentSearchesManager.cs  -- persists and replays recent queries
          - SuggestedSearchesManager.cs  -- derives suggestions from the live POI set so they never go stale
          - ResultSetCoordinator.cs  -- the one seam joining search and filter into a single result set
          - ViewModeControl.cs  -- AR / map / list mode switcher
          - ViewModeControl.uxml  -- switcher layout
          - ViewModeControl.uss  -- switcher styling
          - ViewModeParser.cs  -- pure string to view-mode parsing
          - ITranscriber.cs  -- the speech-to-text seam
          - TranscriberFactory.cs  -- picks the real or debug transcriber per platform
          - DebugTranscriber.cs  -- editor stand-in that fakes transcription without a device
          - VoiceSearchController.cs  -- plain C# voice search orchestration, fully EditMode testable
          - VoiceStateMachine.cs  -- idle/listening/processing states and their transitions
          - VoiceActivityIndicatorView.cs  -- the listening indicator bar
        - Minimap/  -- wall overview map (UI Toolkit)
          - MinimapCoordinateConverter.cs  -- pure wall-space to minimap-pixel conversion
          - MinimapView.cs  -- renders POIs on the minimap; shares the selection bus with markers
          - MinimapView.uxml  -- minimap layout
          - MinimapView.uss  -- minimap styling
        - Zoom/  -- on-screen zoom buttons (UI Toolkit)
          - ZoomControlView.cs  -- zoom in/out/reset buttons routed to ARZoomController
          - ZoomControlView.uxml  -- button layout
          - ZoomControlView.uss  -- button styling
        - Shared/  -- cross-screen UI Toolkit support
          - PanelSettings.asset  -- the one runtime panel; 390x844 reference resolution
          - SafeAreaHelper.cs  -- notch and home-indicator padding; pure math exposed for testing
          - UIAccessibility.cs  -- contrast and tap-target helpers used by the accessibility tests
          - UIPalette.cs  -- shared color tokens, in the runtime assembly so views need no Editor reference
        - Scanning/  [EMPTY SCAFFOLD]  -- looking-for-the-wall state
        - Onboarding/  [EMPTY SCAFFOLD]  -- first-run flow and profile selection
        - Circuits/  [EMPTY SCAFFOLD]  -- guided route UI
        - Gamification/  [EMPTY SCAFFOLD]  -- badges, toasts, discovery counter
        - GuideCharacter/  [EMPTY SCAFFOLD]  -- the guide character overlay
        - Sharing/  [EMPTY SCAFFOLD]  -- screenshot and social sharing
      - DevTools/  -- Phase A harnesses and their data lists; one list drives both harness and tests
        - MarkerGalleryDefinitions.cs  -- data list of every marker visual variant
        - MarkerGalleryHarness.cs  -- spawns that list into the gallery scene, no AR or config
        - ClusterGalleryDefinitions.cs  -- data list of cluster member-count and category mixes
        - ClusterGalleryHarness.cs  -- spawns cluster variants
        - DisplacementGalleryDefinitions.cs  -- data list of overlap group sizes, angles and algorithms
        - DisplacementGalleryHarness.cs  -- spawns displacement scenarios
        - OrientationGalleryDefinitions.cs  -- data list of the 11 orientation gallery entries (see _2.1 section 11)
        - OrientationGalleryHarness.cs  -- orbit/pitch/roll camera rig + SpawnEntry (shared by the visual harness and OrientationGalleryTests)
      - Blocks/  [EMPTY SCAFFOLD]  -- the per-POI content block system (text/image/audio/video/model/map)
      - Circuits/  [EMPTY SCAFFOLD]  -- circuit state machine and entry-point resolution
      - Telemetry/  [EMPTY SCAFFOLD]  -- analytics events and consent
      - AI/  [EMPTY SCAFFOLD]  -- optional AI features, feature-flagged
    - Editor/  -- never ships; Editor-only tooling
      - TileStories.Editor.asmdef  -- editor assembly
      - AssemblyInfo.cs  -- InternalsVisibleTo for the editor test assembly
      - POIEditorRigSafetyCheck.cs  -- warns on scene save or Play Mode if the editor rig still has markers in it
      - POIEditorRigBuildCheck.cs  -- hard-blocks a build while the rig has children, since a build is visitor-facing
      - POIEditor/  -- the POI Editor window; one partial class across many files (see _5.1_Editor_Tab.md)
        - POIEditorToolWindow.cs  -- shell: fields, menu item, OnGUI orchestration, DrawFramedFoldout, action bar
        - POIEditorToolWindow.Constants.cs  -- option and label arrays, section colors, layout constants, help strings
        - MarkerSymbolTexturePostprocessor.cs  -- forces correct import settings on marker symbol textures
        - GlobalScene/  -- wall-wide settings sections
          - POIEditorToolWindow.GlobalScene.cs  -- section dispatch plus Marker, Badge, Outline, Effects and Hierarchy Levels (now also carries the per-level Facing override column, see _2.1 section 4.3)
          - POIEditorToolWindow.Orientation.cs  -- Orientation section, 4 sub-foldouts: Vertical Alignment, Facing Options, Update Cost, Test (see _2.1 section 8)
          - POIEditorToolWindow.LodZoom.cs  -- LOD and Zoom sections, plus the four shared field-row helpers every section uses
          - POIEditorToolWindow.Displacement.cs  -- displacement algorithm, thresholds and leader-line settings
          - POIEditorToolWindow.SearchFilter.cs  -- search keyword fields and synonym groups
        - SpecificMarker/  -- per-POI editing
          - POIEditorToolWindow.SpecificMarker.cs  -- the POI list, header row (focus, rename, reorder, add, help, delete) and per-POI style sections
          - POIEditorToolWindow.PositionTabs.cs  -- the Position foldout: three Facing X/Y/Z sliders (disabled while Verified), XYZ readout, Verified toggle
          - POIEditorToolWindow.MarkerSceneEdit.cs  -- selected-marker edit handling: facing sync from Scene view/Inspector, Verified lock on facing and position, auto-reveal of the POI's section
          - MarkerEditDetector.cs  -- pure tracker: did the selected marker's pose change since last seen, plus the gizmo-drag gesture check (drives the auto-reveal)
          - FacingEditAdvice.cs  -- pure: which facing edits are invisible under Edit-Mode preview for the effective facing mode, and the Scene-view warning text for them
          - PoiFocusResolver.cs  -- pure math for framing a marker in the Scene view
          - PoiRenameKeys.cs  -- Enter/ESC commit handling for the rename field
          - PoiRotationResolver.cs  -- pure Facing X/Y/Z math: angles to quaternion, angle normalisation, IsSameOrientation (compare rotations as orientations, never euler triples)
          - Icons/  -- add, delete, edit and focus button PNGs
        - RigLifecycle/
          - POIEditorToolWindow.RigLifecycle.cs  -- creates, refreshes, captures from and clears the Edit-Mode marker rig; calls the real Initialise methods; also owns the orientation preview (ApplyOrientationPreview, RestoreRigRotationsFromConfig, see _2.1 section 9)
        - ConfigData/
          - POIEditorToolWindow.ConfigFileIO.cs  -- load, save and copy-to-StreamingAssets
          - POIEditorToolWindow.ConfigHistory.cs  -- JSON-snapshot undo/redo, separate from Unity's Undo
          - POIEditorToolWindow.ConfigValidation.cs  -- non-blocking warnings: hierarchy keys, size ranges
        - AssetPaths/
          - POIEditorToolWindow.AssetPaths.cs  -- browse rows, absolute-to-Assets path conversion, Resources path rules
        - Shared/  -- reusable pieces every section draws through
          - POIEditorToolWindow.RowLayout.cs  -- DrawEditorRow / EditorRowEnd: the one shared indent + width-capped row
          - POIEditorToolWindow.IconButton.cs  -- the one icon-only button look, used by every icon and help button
          - POIEditorToolWindow.DeleteButton.cs  -- the one shared destructive-delete-button look (Marker Table's trash glyph, IconButtonSize default), used by every delete affordance in the window
          - POIEditorToolWindow.SymbolTable.cs  -- the shared taxonomy table renderer and sprite preview/picker
          - HelpInfoPopup.cs  -- read-only framework help popup plus HelpInfoButton.Draw
          - EntryDetailsPopup.cs  -- editable per-row notes persisted into config; NOT the same as help
          - ExistingSymbolPickerPopup.cs  -- curated sprite picker limited to wall and framework icons
          - EditorAlertItem.cs  -- one config-validation finding plus the plain-text report builder (first 6 findings + guidance)
          - EditorNotice.cs  -- the one queue for short informational messages (locked edit, hidden facing change, nothing to clear, rejected rename, validation report); an editor-update pump shows each as a native OK dialog, debounced for drag gestures. Advisory notices carry Unity's native do-not-show-again checkbox (undone by TileStories > Reset Hidden Notices); refusal explanations never do. Yes/No decisions stay EditorUtility.DisplayDialog
          - IdentityRenameResolver.cs  -- the one rename rule for every identity key; rewrites all referencing POIs
          - IdentityRenameEditState.cs  -- commit-style edit session for identity cells; survives undo swaps
          - IdentityDeleteGuard.cs  -- confirm dialog before deleting a row POIs still reference
          - SearchFieldReferenceResolver.cs  -- the one nested identity: search field key inside each POI's keyword list
          - DefaultCategoryStyles.cs  -- seeds 6 heritage categories into an empty config
          - DefaultBadgeCategories.cs  -- seeds 4 damage levels
          - DefaultOutlineLevels.cs  -- seeds the 4-level destruction ramp
          - LodAutoSuggest.cs  -- computes sensible LOD band values from the current POI set
          - Icons/  -- details, info and select button PNGs
      - DevTools/
        - ClusterPrefabWiring.cs  -- one-shot re-wiring of POI_Cluster.prefab's dominant icon reference
      - Validation/  [EMPTY SCAFFOLD]  -- Stage 4 config schema validator
      - Baker/  [EMPTY SCAFFOLD]  -- Stage 4 JSON to ScriptableObject bake step
      - Wizard/  [EMPTY SCAFFOLD]  -- Stage 5 new-wall onboarding wizard
      - Dev/  [EMPTY SCAFFOLD]  -- scratch editor utilities
    - Tests/
      - Editor/  -- EditMode suite: pure logic, config round-trips, IMGUI layout measurement
        - TileStories.Editor.Tests.asmdef
        - ARZoomMathTests.cs, ARZoomStateTests.cs  -- zoom math and state clamping
        - CategoryPaletteTests.cs, StatusRampTests.cs, MarkerVisualsParserTests.cs  -- taxonomy resolution
        - MarkerHierarchyResolverTests.cs, MarkerLayoutTests.cs, MarkerLayoutPxConversionTests.cs  -- size and layout math
        - ClusterGroupingTests.cs, ClusterReconcileTests.cs, ClusterPrefabTests.cs  -- cluster grouping and prefab contract
        - DisplacementComputeTests.cs, DisplacementAlgorithmTest.cs, DisplacementHysteresisTests.cs, DisplacementTieBreakStrategyTests.cs, DisplacementSettingsDefaultsTests.cs, DisplacementAuthoringRoundTripTests.cs  -- displacement math, defaults and authoring round-trip
        - LODControllerTests.cs, LodAutoSuggestTests.cs, LodAuthoringRoundTripTests.cs, DensityThresholdValidationTests.cs, LeaderLineVisibilityTests.cs  -- LOD logic and authoring
        - POIPositionResolverTests.cs  -- null position falls back to origin
        - MarkerOrientationResolverTests.cs  -- Tier-0 tests for the orientation resolver (_2.1 section 5): vertical alignment, facing modes, degenerate/pitch-clamp guards, update gating
        - POISearchIndexTests.cs, POISearchIndexMatchModeTests.cs, SearchTokenizerTests.cs, SearchSynonymGroupsTests.cs  -- search index behaviour
        - EditorCameraLookTests.cs  -- mock-camera look math including roll (_2.1 section 13.3)
        - PoiRotationResolverTests.cs, PoiFocusResolverTests.cs, PositionTabsTests.cs  -- per-POI editor math and Position foldout shape
        - PoiFacingLockAndSyncTests.cs  -- Facing X/Y/Z: orientation-based scene sync, Verified lock on facing, marker-edit detector, auto-reveal state, real IMGUI slider rows, LivingRoom config round trip
        - EditorNoticeTests.cs  -- notice queue timing (discrete vs drag), validation report text, hide/reset of notices, converted call sites (ClearRig, config validation, facing warnings)
        - TestDialogGuard.cs  -- SetUpFixture: switches real notice dialogs off for the whole EditMode run so no test can hang on a modal
        - PoiFacingModesAndLifecycleTests.cs  -- per-mode/per-axis visibility on a real POI_Marker prefab under preview, preview warnings vs real pipeline, gizmo under preview, verify/unverify wiring, leaked scene-handler check, reveal incl. blocked edits
        - IdentityRenameResolverTests.cs, IdentityRenameCompositionTests.cs  -- rename rule and its composition across tables
        - HierarchyLevelKeyValidationTests.cs, HierarchyLevelSizeRangeTests.cs  -- config validation
        - OrientationEditorRoundTripTests.cs  -- OrientationSettings JSON round-trip, no-block defaults, editor foldout field-existence, deleted-field absence checks (_2.1 section 7)
        - OrientationEditModePreviewTests.cs  -- Edit-Mode orientation preview: non-destructive guarantee, on/off cycle byte-identical, CapturePositions guard, wall_fixed Edit/Play-Mode agreement (_2.1 section 9)
        - DefaultCategoryStylesTests.cs, DefaultBadgeCategoriesTests.cs, DefaultOutlineLevelsTests.cs  -- seeding defaults
        - POIEditorToolWriteBackTests.cs, POIEditorAddPoiTests.cs, POIEditorToolSearchRoundTripTests.cs  -- editor data flow; ReloadGuardChoiceTests.cs  -- pure dialog-result mappings (reload guards, rig safety) incl. Esc/X must cancel
        - POIEditorTableLayoutTests.cs, POIEditorAddButtonRowRenderTests.cs, POIEditorCoordinateRowRenderTests.cs, POIEditorColorGroupRenderTests.cs, POIEditorVisualHierarchyTests.cs, POIEditorWindowChromeTests.cs  -- real IMGUI geometry measurement
        - MarkerSelectionEditModeTest.cs, MarkerSymbolTexturePostprocessorTests.cs, PanelSettingsTests.cs, SafeAreaHelperTests.cs, SearchFilterSelectToggleTests.cs, ZoomControlViewEditModeTests.cs  -- assorted contracts
        - UI/  -- UI Toolkit view tests, mirroring Runtime/UI's folder names
          - Cards/DetailCardViewTests.cs
          - Filter/FilterFacetEvaluatorTests.cs, FilterTrayViewTests.cs, FilterCompositionTests.cs
          - Minimap/MinimapCoordinateConverterTests.cs, MinimapViewTests.cs
          - Navigation/ViewModeControlTests.cs, ViewModeParserTests.cs
          - Results/ResultsListViewTests.cs
          - Search/SearchOverlayViewTests.cs, SearchInputGuardTests.cs, RecentSearchesManagerTests.cs, SuggestedSearchesManagerTests.cs, VoiceSearchControllerTests.cs, VoiceStateMachineTests.cs, VoiceActivityIndicatorViewTests.cs
          - Shared/UIAccessibilityTests.cs, SelectFilterAccessibilityTests.cs
        - MarkerAssets/  [EMPTY SCAFFOLD]  -- fixtures for marker asset tests
      - Runtime/  -- PlayMode suite: anything needing a scene, a camera, coroutines or Destroy
        - TileStories.Tests.asmdef
        - MarkerGalleryTests.cs, ClusterGalleryTests.cs, DisplacementGalleryTests.cs, OrientationGalleryTests.cs  -- Phase A gallery assertions, driven by the same definition lists as the harnesses
        - MarkerOverlapResolverTests.cs  -- the marker billboard rotation contract
        - MarkerViewRuntimeTests.cs, MarkerRevealEffectTests.cs, MarkerIconLibraryRuntimeTests.cs  -- marker visuals at runtime
        - LODControllerEvaluateTests.cs, ClusterPipelineIntegrationTests.cs, ClusterReconcilePlayModeTests.cs  -- the real LOD pipeline end to end
        - DisplacementLabelTests.cs  -- label displacement against real transforms
        - FilterCompositionPlayModeTests.cs  -- filter fades, which need coroutines
        - ARZoomRoutingTests.cs, ARZoomRoutingTests_additions.cs, ARZoomCameraFovTests.cs  -- zoom routing and real FOV change
        - LivingRoomConfigIntegrationTests.cs  -- editor to StreamingAssets to runtime config contract
        - OrientationWallSessionIntegrationTests.cs  -- real WallSession.SpawnPOIs orientation wiring + hierarchy override + real-marker occlusion check (_2.1 section 6)
        - OrientationClusterIntegrationTests.cs  -- real LODController cluster spawn path gets MarkerBillboard.Configure; cluster rotation changes when camera rotates (_2.1 section 6.4)
        - Search/SearchOverlayRuntimeTests.cs  -- search overlay with a real UIDocument
      - EditMode/  [EMPTY SCAFFOLD]  -- legacy folder, superseded by Tests/Editor
  - StreamingAssets/
    - LivingRoom/
      - config.json  -- runtime-readable copy; written by the editor's Copy to StreamingAssets
      - config.json.backup  -- safety copy
  - Plugins/
    - Android/
      - AndroidManifest.xml  -- camera permission and AR feature declarations
  - Settings/  -- URP render pipeline assets and the default scene template (Unity-generated)
  - XR/  -- AR Foundation, ARCore, ARKit, OpenXR and XR Simulation loader/settings assets (Unity-generated)
  - TextMesh Pro/  -- TMP package resources: fonts, materials, shaders, sprite assets (third-party)
  - UI Toolkit/  -- UnityThemes/UnityDefaultRuntimeTheme.tss (Unity-generated)
  - Screenshots/  -- captured dev and vision-pass PNGs; evidence artifacts, not shipped content
  - Resources/  [EMPTY SCAFFOLD]  -- project-wide runtime-loadable assets
  - DefaultVolumeProfile.asset  -- URP post-processing defaults
  - InitTestScene<guid>.unity (x5)  -- Unity Test Framework leftovers; not authored content
- proj_guides/  -- domain design docs (_0_work_plan.md, _2.1 through _2.6, _5.1)
- .clinerules/  -- the guideline files (00 process, 10 structure, 20 code quality, 30 UI, 40 testing, 50 tools, 60 finishing)
- report/  -- thesis report sources

## 6. Notes for future edits

- Keep the tree ASCII-only and `-`-indented (section 0).
- The tree describes what EXISTS on disk right now, never what is planned. A file
  a domain doc says will be created belongs in that domain doc until it actually
  exists; add it here in the same session you create it.
- Any task that creates, deletes, moves or renames a file under `Assets/` updates
  the tree in the same session, with a one-line description for anything new.
- Update only the entries your task touched. Do not re-audit the whole tree every
  session; a full re-scan is an occasional, deliberate task, not routine work.
- If a second wall needs the same thing, move the shared logic to Framework
  rather than copying it into another wall folder.
- In the POI editor, destructive row actions must have a clear danger affordance:
  a red delete icon next to the edit/move controls, and a confirmation dialog
  before removing a POI from the config and the scene rig.
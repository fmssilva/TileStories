# Project Structure & Organizing Principles

Read this file to get oriented on the project's structure.

## 1. Organizing principles

### 1.1 Two top-level areas: Framework and Apps

- `Assets/Framework/` contains code that behaves the same for every wall: AR session bootstrap, tracking abstractions, POI data and rendering, UI shells, content-card rendering, analytics, and the guide character system.
- `Assets/Apps/<WallName>/` contains one self-contained wall folder per app: POI list, category taxonomy, localization/map files, and wall media.
- A system moves from an app-specific folder into Framework only when a second wall needs the exact same thing unchanged.
- Nothing in `Framework/` may reference anything inside a specific `Apps/<WallName>/` folder.
- No wall folder references another wall folder.

### 1.2 Editor code is physically separated from Runtime code

- `Assets/Framework/Runtime/` is shipped in the build.
- `Assets/Framework/Editor/` only runs in the Unity Editor.
- Assembly Definition files enforce this split.
- Runtime code must never reference Editor code.
- If runtime code needs editor-only behavior, move that logic into the Editor assembly instead of adding a runtime dependency.

### 1.3 Menu-item and tool visibility

- Developer-facing per-wall configuration belongs in the POI Editor window.
- Framework-internal repair tools should not appear as ambiguous top-level menu items.
- One-time asset scaffolding can be a menu item, but it must be clearly scoped.
- When adding a `[MenuItem("TileStories/...")]`, ask whether a wall developer would actually need to click it.

### 1.4 Domain-centered folders, not type-centered folders

- Group files by what they do, not by generic technical buckets.
- Do not create catch-all utility files that accumulate unrelated methods.
- Test folders live alongside the code they test inside `Framework/Tests/`.

## 2. How to read this structure

- The key boundary is `Framework/` versus `Apps/`: Framework never knows about a specific wall.
- The second key boundary is `Framework/Runtime/` versus `Framework/Editor/`: runtime ships to devices, editor code does not.
- The `AI/` folder is optional and guarded by a feature flag.

## 3. Stage-by-stage construction sequence

- Stage 0: docs, mock deliverables, sketches
- Stage 1: Core, Tracking, POI, UI/Scanning, UI/Cards scaffold, UI/Onboarding, Telemetry scaffold, first wall app
- Stage 2: Blocks, UI/Markers, LOD, Zoom, Lapse timeline, state manager
- Stage 3: Circuits, UI/Circuits, UI/Gamification, UI/GuideCharacter, UI/Navigation, UI/Sharing, optional AI
- Stage 4: Editor/Validation, Editor/Baker, Tests
- Stage 5: Editor/Wizard
- Stage 6: package extraction

## 4. Standing facts about the project

- `Assets/Apps/Chafariz/`, `Mural/`, and `Panorama/` are scaffold-only walls.
- `LivingRoom` is the only wall with real, working content.
- `Assets/StreamingAssets/LivingRoom/config.json` is the runtime-readable copy of `Assets/Apps/LivingRoom/config.json`.
- `Assets/Dev/` is intentional development infrastructure, not garbage.

## 5. Complete project file structure

This is the authoritative ASCII outline for authored content. It uses only simple characters so it stays stable in editors and agent tools.

TileStories/
- Assets/
  - Framework/
    - Runtime/
      - Core/
        - WallSession.cs
        - WallConfigLoader.cs
        - WallConfigData.cs
        - MarkerStyle.cs
        - MarkerShape.cs
        - MarkerOutlineMode.cs
        - MarkerVisualsParser.cs
        - WallConfigAsset.cs
        - TileStoriesSettings.cs
        - FeatureFlags.cs
      - Tracking/
        - IWallTracker.cs
        - ImmersalWallTracker.cs
        - MockLocalizationProvider.cs
        - EditorCameraLook.cs
        - ARZoomState.cs
        - ARZoomController.cs
        - ARZoomMath.cs
        - ARZoomGestureInput.cs
        - TrackingJitterSmoother.cs
        - TrackingMetrics.cs
      - POI/
        - POIPositionResolver.cs
        - CategoryPalette.cs
        - BadgeCategoryPalette.cs
        - StatusRamp.cs
        - MarkerHierarchyResolver.cs
        - DisplacementTieBreakStrategy.cs
        - MarkerOverlapResolver.cs
        - POISearchIndex.cs
        - SearchTokenizer.cs
        - SynonymGroup.cs
      - DevTools/
        - MarkerGalleryDefinitions.cs
        - MarkerGalleryHarness.cs
        - ClusterGalleryDefinitions.cs
        - ClusterGalleryHarness.cs
        - DisplacementGalleryDefinitions.cs
        - DisplacementGalleryHarness.cs
      - Blocks/
        - TileStoriesUIBlock.cs
        - TileStoriesBlockRegistry.cs
        - TextBlock.cs
        - ImageBlock.cs
        - AudioBlock.cs
        - VideoBlock.cs
        - ModelBlock.cs
        - MapBlock.cs
        - ProfileResolver.cs
      - Circuits/
        - CircuitStateMachine.cs
        - CircuitState.cs
        - EntryPointResolver.cs
        - AmbientPivotMonitor.cs
        - ButterflyPromptController.cs
        - CircuitLookupTable.cs
      - UI/
        - Markers/
        - Zoom/
        - Minimap/
        - Results/
        - Filter/
        - Cards/
        - Circuits/
        - Onboarding/
        - Gamification/
        - GuideCharacter/
        - Navigation/
        - Sharing/
        - Telemetry/
        - AI/
    - Editor/
      - POIEditor/
        - POIEditorToolWindow.cs
        - POIEditorToolWindow.Constants.cs
        - GlobalScene/
        - SpecificMarker/
        - Shared/
          - POIEditorToolWindow.RowLayout.cs  // EditorRowWidth + DrawEditorRow/EditorRowEnd: reusable non-table row (indent + width cap + ExpandWidth(false)); indent measured at call time so nested scopes keep deeper spacing
        - ConfigData/
        - AssetPaths/
          - POIEditorToolWindow.AssetPaths.cs  // DrawPathRow: non-button shared row (transparent indent + labelled TextField capped to rowWidth - 36f + fixed browse button + EditorRowEnd), AbsoluteToAssetPath, GetWallLibraryDirectory
        - RigLifecycle/
      - DevTools/
        - ClusterPrefabWiring.cs
      - Validation/
      - Baker/
      - Wizard/
      - POIEditorRigSafetyCheck.cs
      - POIEditorRigBuildCheck.cs
    - Tests/
      - Editor/
      - Runtime/
  - Apps/
    - LivingRoom/
      - config.json
      - config.json.backup
      - LivingRoomScene.unity
      - 146267-LivingRoom2.bytes
      - 146267-LivingRoom2-tex.glb
      - generate_config.py
      - MarkerAssets/
      - MediaAssets/
    - Chafariz/
      - MediaAssets/
    - Mural/
      - MediaAssets/
    - Panorama/
      - MediaAssets/
  - Dev/
    - MarkerGallery/
  - proj_guides/
  - .clinerules/
  - report/

## 6. Notes for future edits

- Keep the hierarchy outline ASCII-only.
- If a new folder or system is added, place it in the correct top-level area first, then expand the relevant subtree.
- If a second wall needs the same thing, move the shared logic to Framework rather than copying it into another wall folder.
- In the POI editor, destructive row actions must have a clear danger affordance: a larger dark-red delete button, placed next to edit/move controls, and a confirmation dialog before removing a POI from the config and the scene rig.

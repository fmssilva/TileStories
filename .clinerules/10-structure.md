# Project Structure and Organizing Principles

Read this file to get oriented on the project's structure. It is the map: what
exists, where it lives, and what each piece does in one line. Tree verified
against disk on 2026-09-18; the Effects, Orientation and Live Play Mode entries
were re-verified on 2026-09-21; the Marker/Badge/Outline entries (MarkerVisualSettings,
MarkerVisualResolver, MarkerDesign.cs, MarkerDesignHelp.cs, LivePlayModeMarkerApplier,
the new test files, MarkerStyle.cs removal) were re-verified on 2026-09-22; the Outline
follow-up (P5, same day: MarkerOutlineMode.Uniform/PerType rename+split, outline_uniform_color_hex,
DevPreviewGridLayout.cs, OutlinePreviewSpawner.cs/OutlinePreviewFocus.cs, OutlinePreviewRenderTests.cs)
was verified same day too, as was a further same-day fix to POIEditorToolWindow.MarkerDesign.cs's
outline table (a double-gap regression when the Color column is hidden). The Hierarchy Levels
domain (new HierarchyHelp.cs, GlobalScene.cs's table header row and Search Keywords column, new
HierarchyDesignAuthoringTests.cs) was verified on 2026-09-22. A further same-day pass added the
demo grids' mutual exclusion (DemoGridExclusivity.cs), shared free-fly camera controls
(DevCameraInput.cs, DevPreviewCameraDolly.cs, MockLocalizationProvider.cs's refactor), the
Hierarchy table's Label/Show Text Label? reorder, and the wall-level label_gap_ratio/
label_font_size_ratio fields -- all verified against disk the same day. A further same-day
pass split label typography out into its own domain doc, `_2.0_Labels_And_Fonts_Design.md`, and added
a real font-type system (FontKeyLibrary.cs, FontLibrary.asset, Fonts/) and per-hierarchy-level
label overrides (HierarchyLevelEntry's 3 `_override` fields, LevelLabelOverrideWindow.cs, the
Hierarchy table's "Aa" window) -- verified against disk the same day. A follow-up same-day pass
renamed the confusing `HierarchyLevelEntry.label` field to `level_name`, reorganized the
Hierarchy Levels table into 3 clearer groups, moved "Show Text Label?" to a plain Toggle,
promoted the wall-default label fields into their own top-level "Labels, Text & Fonts" Global
Scene section (LabelsAndFonts.cs/LabelsAndFontsHelp.cs), and fixed the demo grids' camera
resetting on every live editor edit (DevPreviewCameraDolly.SetOffsets,
WallSession.RebuildEffectsPreview/RebuildOutlinePreview) -- all verified against disk the same day.
A 2026-09-24 pass on the Labels & Fonts domain added a UI-driven wall font library flow
(AssetPaths.cs's TryResolveWallFontLibraryFromConfig/CreateOrAssignWallFontLibrary/
GetAvailableFontKeyOptions, FontKeyLibrary.CopyFrom), fixed the Scene-view rig preview never
resolving a wall's font library (RigLifecycle.cs), widened the Gap/Font-size slider ranges,
made "Marker Label" its own collapsible sub-foldout, added an "Add Labels demo grid" switch
sharing effect_defaults.preview, and added LabelsAndFontsAuthoringTests.cs -- verified against
disk the same day. A 2026-09-24 Hierarchy Levels domain review replaced the per-level label
"_override" sentinels with one override_label_style switch (MarkerHierarchyResolver.StyleOf), rebuilt
LevelLabelOverrideWindow.cs as a thin "Marker Label Style" shell, added TableRowScope (RowLayout.cs),
the shared font rows/Add font flow, LivePlayModePoiLevelApplier.cs, HierarchyTableClickTests.cs and
MarkerLabelStyleWindowTests.cs -- verified against disk the same day.
A 2026-09-24 Marker / Badge / Outline / Effects / Orientation domain review added the wall icon fields, the shared
DomainTest.cs, ReferencePopupOptions.cs, the popup-edit factories, live POI name + facing, the yaw_only facing-side fix
and seven test files (LiveSyncFieldMatrixTests.cs and siblings) -- verified against disk the same day.
A 2026-09-25 LOD / Zoom domain review (_2.4_Marker_LOD.md) wired LODController and the zoom buttons into
LivingRoomScene, split ZoomSettings out of LodSettings, added the dev-only LOD demo field (DemoFieldLayout/
DemoFieldSpawner), the LOD/Zoom/demo-field live appliers, LodZoomHelp.cs/LodEditorRules.cs and five test files
(LodRealPipelineTests.cs and siblings) -- verified against disk the same day. A same-day round 2 ("the demo field
does nothing") added DemoFieldStage.cs, LodStats.cs, Shared/FieldRows.cs (the shared drawers moved out of LodZoom.cs),
DemoFieldLayoutTests.cs, LodStatsTests.cs and LodWallSceneTests.cs, and fixed yaw_only, Select & Hide and the zoom
buttons -- verified against disk the same day. A 2026-09-25 Displacement domain review (_2.5_Marker_Displacement.md)
added DisplacementStats.cs, LeaderLine.mat, DisplacementDemoLayout.cs/DisplacementDemoSpawner.cs (the dev-only displacement
demo on a generalised DemoFieldStage), DisplacementHelp.cs, the two displacement live appliers and eight test files; it deleted
DisplacementGalleryHarness.cs (no scene, no users) and LeaderLineVisibilityTests.cs -- verified against disk the same day.
A 2026-09-25 Select, Filter & Search review (_2.6_Select_Filter_Search.md) moved the domain into one select_filter_search
block (SelectFilterSearchSettings.cs), rebuilt the engine (SearchTextDistance.cs), made the visitor UI plain view classes composed
by SearchUIHost on the scene's new SearchUI object (SearchUI.uss replaced every domain .uxml/.uss and UIPalette.cs), added the
dev-only search demo (SearchDemoLayout/SearchDemoSpawner), SelectionAlphaRule.cs, MarkerClusterSelectable.cs, MinimapLayout.cs,
SearchPanelsRule.cs, the Search live appliers, SearchFilterHelp.cs, KeywordListText.cs and six test files; it deleted
SearchInputGuard.cs, MinimapCoordinateConverter.cs and the replaced tests -- verified against disk the same day.
A 2026-09-26 Select, Filter & Search round 2 added SearchKeywordSources.cs (the one keyword list: index + Editor "Found by"),
SearchDemoCheck.cs (the demo's self-judging test cases) and SearchSettingsFieldTests.cs (one real-scene test per field +
a coverage guard), made FacetGroup a value type (Keyword Fields as filters) and capped zoom-on-select to keep the tapped
point on screen -- verified against disk the same day.
A 2026-09-26 popups pass moved every popup of the POI Editor into Shared/Popups/ (EditorPopup + EditorDecision +
NoticePopup new; HelpInfoPopup, EntryDetailsPopup, ExistingSymbolPickerPopup, EditorNotice moved; LevelLabelOverrideWindow
became LevelLabelStylePopup), replaced ReloadGuardChoiceTests.cs by EditorDecisionTests.cs and added EditorPopupTests.cs --
verified against disk the same day.

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
- `TileStories/MarkerGalleryScreenshots/` (next to `Assets/`, not inside it) is written by
  `MarkerGalleryTests` on every PlayMode run: generated evidence PNGs, not authored content.
- The five `Assets/InitTestScene<guid>.unity` files at the project root are Unity
  Test Framework leftovers, not authored content. Safe to delete; do not build on them.

## 5. Complete project file structure

- Assets/
  - Apps/  -- one self-contained folder per wall; no wall references another
    - LivingRoom/  -- the only wall with real content; the dev + test wall
      - config.json  -- authoring source of truth: wall settings, taxonomies, POI list
      - config.json.backup  -- manual safety copy of the above
      - LivingRoomScene.unity  -- the playable AR scene for this wall; WallSession carries LODController, ZoomRig carries ARZoomController + ARZoomGestureInput + UIDocument/ZoomControlView (LivingRoomSceneWiringTests); SearchUI carries UIDocument (sort order 1) + SearchUIHost bound to WallSession and SearchUI.uss (2026-09-25)
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
        - WallConfigData.cs  -- the whole config.json object model: WallConfigData, POIData, PositionData, taxonomy entries, LodSettings, DisplacementSettings, EffectDefaults, OutlinePreviewSettings (P5, outline_preview); label_gap_ratio/label_font_size_ratio/label_font_key/label_font_library_resources_path (2026-09-22, _2.0_Labels_And_Fonts_Design.md) join ring_size_ratio/badge_size_ratio's symbol-diameter-ratio convention; HierarchyLevelEntry's override_label_style + label_gap_ratio/label_font_size_ratio/label_font_key (2026-09-24) are the level's own "Marker Label Style", used only while the switch is on; icon_color_hex / icon_size_ratio (2026-09-24, _2.2.1): the icon colour of every symbol and badge and the symbol icon size; ZoomSettings (2026-09-25, zoom_settings: its own block, no longer inside LodSettings); LodSettings.DefaultBands + shrink_min_factor / cluster_size_ratio (2026-09-25); DemoFieldSettings + DemoFieldLevelCount (2026-09-25, demo_field: the dev-only LOD demo field); DisplacementDemoSettings (2026-09-25, displacement_demo: the dev-only displacement demo) and DisplacementSettings' slider-limit constants (force_directed_iterations removed: no visible effect, _2.5 Design history); (2026-09-25) select_filter_search (the Select, Filter & Search block, SelectFilterSearchSettings.cs), search_fields (SearchFieldDefinition.filterable = the Keyword Field's Filter column, 2026-09-26) + synonym_groups (keyword vocabulary), SearchDemoSettings (search_demo, dev-only); the old flat search/minimap/voice/selection fields and the global WallBounds are gone
        - SelectFilterSearchSettings.cs  -- 2026-09-25. The Select, Filter & Search block: SelectionSettings (+ ZoomOnSelectSettings), SearchSettings, FilterSettings, ResultsSettings, MinimapSettings, VoiceSettings, each field's limits; SelectFilterSearchOptions = every option string + parser (TriggerAllows, ParseMatchMode, ParsePrefix, ToSearchOptions, MismatchAlpha), shared by the runtime and the Editor Tab dropdowns
        - WallConfigLoader.cs  -- reads config.json from StreamingAssets and deserializes it
        - WallSession.cs  -- boots one wall: load config, wait for tracking, spawn markers, expose settings to LOD/zoom. Delegates every decision; holds none. Also the live Play Mode seams ApplyEffectSettings / ApplyOrientationSettings; RefreshVisualSettings loads the wall's optional font library (label_font_library_resources_path) alongside the icon library; RebuildEffectsPreview/RebuildOutlinePreview (2026-09-22) destroy+respawn a demo grid while carrying its camera pan/zoom across the gap via DevPreviewCameraDolly.SetOffsets; ApplyPoiHierarchyLevels (2026-09-24) moves running POIs to another hierarchy level live (size/label/effects/facing); ApplyPoiFacing (2026-09-24) swaps each running POI's authored Facing X/Y/Z (AuthoredRotationOf is the one spawn+live formula); ApplyLodSettings / ApplyZoomSettings / ApplyDemoField (2026-09-25) swap those blocks on a running wall; RebuildDemoField hands the LOD demo field and the displacement demo to their DemoFieldStages (a stage turning off goes first, so the camera gets home before another stage takes it) and runs a demo's markers alone while one is on; ApplyDisplacementSettings / ApplyDisplacementDemo (2026-09-25) are the Displacement live seams; LodSettings hands LODController a fixed LOD-off object while the displacement demo pauses LOD; ConfigureBillboard is the one marker-orientation call for wall and demo markers; LOD/zoom controllers are found anywhere in the scene; (2026-09-25) SearchPois / SearchConfig / SearchIndex rebuilt over the RUNNING markers' POIs (RebuildSearchIndex, after every marker swap and live marker/level edit), SearchDataChanged, SelectFilterSearch, the selection responders (UpdateSelectionResponders, master switch live), ApplySearchSettings / ApplySearchDemo, a third DemoFieldStage for the search demo (LOD paused unless Run LOD)
        - MarkerShape.cs  -- enum of symbol silhouettes (circle, rounded square, hexagon, diamond, star, none)
        - MarkerOutlineMode.cs  -- enum for whether and how the status contour ring renders (Uniform, SameHue, PerType, None; P5: Gold renamed to Uniform, PerType added)
        - MarkerVisualsParser.cs  -- parses config strings into the enums above with safe fallbacks, plus TryParseBadgeCorner
      - Tracking/  -- localisation, the editor mock, and AR zoom
        - IWallTracker.cs  -- the tracking seam: OnWallLocalised(Pose)
        - ImmersalWallTracker.cs  -- real implementation wrapping the Immersal Localizer
        - MockLocalizationProvider.cs  -- Tier A editor mock: fires localised immediately, drives Camera.main via the shared DevCameraInput reader (Runtime/DevTools) for WASD/mouse look/roll (2026-09-22); does nothing while there is no main camera
        - EditorCameraLook.cs  -- pure yaw/pitch math for the mock camera; no scene needed
        - ARZoomState.cs  -- current zoom factor and the effective-distance rule it implies
        - ARZoomMath.cs  -- pure pinch, double-tap and step-cycle math; AnimatedZoom eases over exactly Transition (s), frame-rate independent
        - ARZoomGestureInput.cs  -- reads touch input and calls the controller
        - ARZoomController.cs  -- applies zoom to the camera FOV; the only writer of zoom state; reads WallSession.ZoomSettings every frame (Enable Zoom off returns to 1x, new Min/Max Zoom apply at once, 2026-09-25); a step starts from the running animation's target (a quick second click goes a level further)
      - POI/  -- pure resolvers and the LOD pipeline; mostly plain C# classes
        - POIPositionResolver.cs  -- POI to local Vector3; a null position resolves to origin
        - CategoryPalette.cs  -- category string to color + icon key
        - BadgeCategoryPalette.cs  -- badge key to badge icon + tint
        - StatusRamp.cs  -- status percentage to ring color and dash style; Configure(entries, mode, uniformColor) is mode-aware since P5 (Uniform shares one colour, Per outline type honours each row's own)
        - MarkerHierarchyResolver.cs  -- hierarchy level key to size, label visibility, effects, reveal timing, Marker Label Style; StyleOf(entry) is the ONE row-to-HierarchyStyle conversion, shared by Configure and the demo grid's level cells (2026-09-24)
        - MarkerVisualSettings.cs  -- the wall-level marker/badge/outline look, resolved ONCE from WallConfigData (Resolve) and applied to every static palette (ApplyPalettes); WallSession, the editor Scene rig and the live Play Mode applier all build it the same way (_2.2.1 section 3.1); FontLibrary/LabelFontKey added 2026-09-22 (_2.0_Labels_And_Fonts_Design.md); the shared label limits LabelGapRatioMin/Max, LabelFontSizeRatioMin/Max, DefaultLabelFontKey + Clamp/Resolve helpers (2026-09-24) used by the runtime clamp and every label slider; IconColor / IconSizeRatio + IconSizeRatioMin/Max (2026-09-24), shared with the Icon size slider
        - MarkerVisualResolver.cs  -- pure: POIData + MarkerVisualSettings -> MarkerVisualState (background/fill/icon/ring/badge decisions as plain data); MarkerView only draws the result (_2.2.1 section 3.1); a known status resolves its Outline Types row by status_level_key first (pct only without a key), and no status = no badge (2026-09-24, _2.2.2/_2.2.3)
        - MarkerOverlapResolver.cs  -- displacement (step 8 of LODController): TakesPart (visible individual markers only), union-find grouping (BuildOverlapGroups), the three pure algorithms (ComputeOffsets; Force Directed up to ForceDirectedMaxSteps), hysteresis (CommitGroupMembership), ApplyDisplacement returning a DisplacementStats, ClearAll (everything back in place)
        - DisplacementTieBreakStrategy.cs  -- decides which marker of an overlapping pair moves
        - ClusterGrouping.cs  -- deterministic grouping of dense markers into cluster aggregates; RepresentativeDistance (2026-09-25) is the Band Source rule (centroid / nearest / farthest member)
        - LODController.cs  -- the per-cycle pipeline: distance banding, culling, density response, clustering, displacement; ReapplyClusterOrientation re-points on-screen clusters when orientation changes live; (2026-09-25) applies the crowding factor to markers, honours Band Source, measures every marker at its UndisplacedWorldPosition, follows WallSession.LodSettings every frame and RestoreAllMarkers when LOD is switched off or its settings are replaced; (round 2) SelectAndHide keeps a crowd's most important markers, LastStats publishes each evaluation's LodStats; (2026-09-25) LastDisplacementStats, and ClearDisplacement puts every marker back once when Displacement is switched off or its settings object is replaced
        - DisplacementStats.cs  -- 2026-09-25. What one displacement cycle decided (looked at, crowded groups, moved, longest move, labels hidden, leader lines) for the Live Displacement Readout and the tests
        - LodStats.cs  -- 2026-09-25 (round 2). Pure: counts one LOD evaluation's final units (shown, shrunk, hidden by crowding / Max markers / out of view, clusters, per band) for the POI Editor's Live LOD Readout
        - POISearchIndex.cs  -- inverted index token -> (POI, rank, source); Build indexes exactly SearchKeywordSources.Collect per POI (2026-09-26); Search(query, SearchOptions, candidates): exact / forward prefix / typo matching, Match Words, filters by id (empty set = nothing), results carry the POI and a Reason; SearchMatchMode, SearchPrefixScope, SearchOptions (2026-09-25 rewrite)
        - SearchKeywordSources.cs  -- 2026-09-26. Pure: the ONE list of every word a POI is found by (name, Keyword Fields + Others, summary, category / badge / status / level row name + Search Keywords, synonyms of all of those), each with source, rank and origin; used by POISearchIndex.Build and the Editor's "Found by"
        - SearchTokenizer.cs  -- shared tokenisation for index build and query time (accent folding, lowercase, split, dedup)
        - SearchTextDistance.cs  -- 2026-09-25. Bounded optimal-string-alignment edit distance (a swapped pair = one edit) for typo tolerance
        - SynonymGroup.cs  -- one synonym group (key + synonyms, all equal members), stored in WallConfigData.synonym_groups
      - UI/
        - Markers/  -- the ONLY uGUI World-Space Canvas domain in the project
          - POI_Marker.prefab  -- the marker outer container. Root holds Canvas + POIAnchor + MarkerView + MarkerBillboard + MarkerRevealEffect + MarkerSelectable + MarkerLeaderLine; children are Label, Symbol, Ring, Badge. Label and Badge each also carry MarkerChildOrientation (see _2.1); the root's LineRenderer (leader line) uses LeaderLine.mat, shadows off
          - POI_Cluster.prefab  -- LOD aggregate marker; children PieContainer, CountLabel, BackgroundImage, DominantIcon. Root also carries MarkerBillboard (see _2.1 section 6.4); root carries MarkerClusterSelectable (2026-09-25)
          - POIAnchor.cs  -- holds this marker's POIData so other components can read it
          - MarkerView.cs  -- applies all visual state to the prefab's children; owns label and marker displacement write-back; ReapplyEffects re-applies only the effects; label gap/font-size/font come from the level while its override_label_style is on, else the wall default, in ApplyVisuals (_2.0_Labels_And_Fonts_Design.md); (2026-09-25) alpha = three independent channels (SetVisible for LOD, SetSelectionAlpha for selection/filter dim, SetDensityFactor for Shrink & Fade, which also scales the root) composed by ComposeAlpha; SymbolDiameterMetres; UndisplacedWorldPosition; RefreshLeaderLine / TryGetLeaderLineEnds (where the leader line runs: symbol rim -> label text edge for Label only, true point -> moved marker otherwise) and ShowsLabel (Displacement review); (2026-09-25) tap raycasts take their final state at the START of a fade (a marker on its way out stops taking taps)
          - MarkerBillboard.cs  -- orientation of the marker root, Vertical Alignment + Facing Options (see _2.1); ReapplySettings swaps settings on a running marker without losing the authored rotation. Referenced from POI_Marker.prefab by script GUID -- do NOT rename; SetAuthoredLocalRotation (2026-09-24) swaps the authored rotation on a running marker (live Facing X/Y/Z)
          - MarkerOrientationResolver.cs  -- pure, stateless orientation math (see _2.1): ScreenUpWorld, ResolveVerticalUp, ResolveRootRotation, ResolveChildLocalRotation, RootRollDeg, Rotate2D, ClampPitch, ShouldUpdate, ResolveUpReference; yaw_only points the marker forward AWAY from the camera like always_facing_camera (2026-09-24 fix: it showed every marker from behind) and measures the yaw in the parent's frame (2026-09-25: a rotated parent's yaw was applied twice)
          - MarkerChildOrientation.cs  -- per-child (Label/Badge) independent vertical-alignment counter-rotation (see _2.1); baked onto POI_Marker.prefab's Label and Badge children
          - MarkerLayout.cs  -- symbol/ring/badge/label sizing ratios (labelGapRatio, 2026-09-22 -- symbol-diameter-relative, was a fixed world-units gap) and ScreenPixelsToWorld conversion
          - FontKeyLibrary.cs  -- 2026-09-22. {key -> TMP_FontAsset} lookup ScriptableObject, same shape as SpriteKeyLibrary (_2.0_Labels_And_Fonts_Design.md); CopyFrom (2026-09-24) seeds a new wall library from the framework default, mirroring SpriteKeyLibrary.CopyFrom; EnsureKeyForFont registers a font and returns its key (reuses an existing entry), mirroring EnsureKeyForSprite
          - FontLibrary.asset  -- framework default FontKeyLibrary: liberation_sans / roboto_bold / oswald_bold
          - Fonts/  -- 2026-09-22. Roboto-Bold SDF.asset, Oswald Bold SDF.asset, their .ttf sources and OFL license text (Google Fonts, sourced from Unity's own bundled TMP Examples & Extras package)
          - MarkerCircleGlyphView.cs  -- draws one circular glyph (symbol or badge) from a sprite + tint; SetIconSizeRatio (2026-09-24, the wall icon_size_ratio)
          - MarkerCircleSpriteFactory.cs  -- generates circle and ring textures once, shared by every effect
          - MarkerRingView.cs  -- the status contour ring, including its optional continuous spin
          - MarkerClusterView.cs  -- builds a cluster's pie chart, count label and dominant icon; world-space size from its largest member x cluster_size_ratio (ComputeDiameterMetres, 2026-09-25); fixed draw order (pie, dark centre disc, icon, '+N') and a clockwise-closed pie
          - MarkerLeaderLine.cs  -- the leader line of a displaced label or marker, recomputed every LateUpdate from MarkerView.TryGetLeaderLineEnds; straight / dashed (dash texture tiled via textureScale, MaterialPropertyBlock) / elbow (screen-space corner, ElbowPoint); EdgeDistance stops it at the label's text edge
          - LeaderLine.mat  -- 2026-09-25. Sprites/Default material of every leader line (unlit, alpha-blended, vertex colour = category colour); without it URP drew magenta
          - MarkerSelectable.cs  -- tap target on POI_Marker: SelectionEventBus.Select(PoiId) (re-tap deselects)
          - MarkerClusterSelectable.cs  -- 2026-09-25. Tap target on POI_Cluster: SelectionEventBus.SelectCluster(member ids) (zoom-on-select); a cluster is never the selection
          - SelectionEventBus.cs  -- the one selection state: Select (re-selecting clears, decided before any listener), Clear, SelectCluster, CurrentPoiId, ResetState; events OnMarkerSelected / OnSelectionCleared / OnClusterSelected
          - SelectionHighlightController.cs  -- the ONLY writer of the selection alpha channel: selected id (bus) + result set (SetResultSet) through SelectionAlphaRule, settings read live from the wall
          - SelectionAlphaRule.cs  -- 2026-09-25. Pure: a marker's selection-channel alpha from (selected, any selected, result set active, in it, highlight on, dim, mismatch alpha)
          - ZoomOnSelectController.cs  -- zoom-on-select: counts the visible markers within Crowd Radius of the tapped one at their true places (CountNeighbours, independent of LOD), a cluster tap is a crowd; ComputeZoomTarget pure, capped by MaxZoomKeepingOnScreen so the tapped point (a cluster's centre) stays on screen (2026-09-26); LastNeighbourCount
          - MarkerEffect.cs  -- base class for optional per-marker animated effects
          - MarkerEffectFlags.cs  -- the six selectable effects as bitflags, plus MarkerEffectNames (display names shared by the editor and the preview grid)
          - MarkerPulseEffect.cs  -- gentle scale pulse
          - MarkerRippleEffect.cs  -- three staggered waves flowing outward, rings or discs, each style with its own parameter block
          - MarkerHaloEffect.cs  -- one extra layer behind the symbol: halo ring or halo disc (breathing) or beacon (grow and fade), each variant with its own parameter block
          - MarkerRevealEffect.cs  -- delayed fade and scale-in on spawn toward the marker's current resting look (SetRest, so a reveal never overrides a LOD decision); SkipToEnd for galleries/tests; snaps to rest in Edit Mode
          - SpriteKeyLibrary.cs  -- the sprite-key lookup contract shared by icon and shape libraries
          - IconLibrary.asset  -- framework default category and badge icons
          - ShapeLibrary.asset  -- framework default symbol silhouettes
          - SymbolCircle.png  -- the default circular symbol sprite
          - Icons/  -- 14 default category and damage-state icon PNGs
          - Rings/  -- 5 contour ring PNGs: solid, long/medium/short dash, dotted
          - Shapes/  -- 4 non-circle symbol silhouettes: diamond, hexagon, rounded square, star
        - Cards/  -- the POI detail bottom sheet (UI Toolkit)
          - DetailCardView.cs  -- plain C# view: the selected POI's name, "category - level" (Subtitle) and summary; X clears through the bus
        - Filter/  -- facet filter tray (UI Toolkit)
          - FilterFacetEvaluator.cs  -- pure: FacetGroup (value type: Category / Badge / Status / Hierarchy + Field(key) for a filterable Keyword Field, 2026-09-26), FacetSelection (the active values, ActiveGroups), PoiPasses (OR within a group, AND across; status only with has_status; a field passes on any held keyword, FieldValueKey), CandidateIds, ComputeRelaxSuggestion -> RelaxSuggestion
          - FilterTrayView.cs  -- plain C# view: the tray (a ScrollView of facet groups, one chip per row by its label), SetFacet, ClearAll, Changed; BuildOptions (pure: enabled groups, labels, then one group per Keyword Field marked Filter with a chip per distinct keyword the POIs hold)
        - Results/  -- search results list (UI Toolkit)
          - ResultsListView.cs  -- plain C# view: virtualised ListView rows (name + "category - level"), a row tap selects through the bus, the no-results message + one relax button
        - Navigation/  -- the search UI host and stylesheet, the search bar, the result-set rules, view switching, voice (UI Toolkit)
          - SearchOverlayView.cs  -- plain C# view: the search bar (field with placeholder, Mic, Filters (n), Map), suggestions under an empty field, the listen bar; dynamic = scheduler debounce, explicit = Enter; QueryChanged / Submitted / button events
          - RecentSearchesManager.cs  -- persists and replays recent queries
          - SuggestedSearchesManager.cs  -- derives suggestions from the live POI set so they never go stale
          - ResultSetCoordinator.cs  -- pure: Compute(index, pois, query, facets, settings) -> ResultSetState (active, ranked results, ids, no-results message, relax suggestion) -- the ONE set every surface shows; RelaxText
          - SearchPanelsRule.cs  -- 2026-09-25. Pure: which panels show (view switch, list, minimap, card, Map button) + the ViewMode enum
          - SearchUIHost.cs  -- 2026-09-25. The one MonoBehaviour of the search UI (scene object SearchUI): builds the six views into its own UIDocument, rebuilds them on WallSession.SearchDataChanged, applies ResultSetCoordinator / SearchPanelsRule, pushes the result set to SelectionHighlightController, keeps the UI in the safe area; SetQuery (suggestions, voice, the Editor's Try a Query)
          - SearchUI.uss  -- 2026-09-25. Every size and colour of the search UI (colours as USS variables); replaced the six unused per-view .uxml/.uss files
          - ViewModeControl.cs  -- plain C# view: List | Map | Highlight segments; the start view from default_view or (remember_last_view) PlayerPrefs; Map only with a minimap
          - ViewModeParser.cs  -- pure config string <-> ViewMode
          - ITranscriber.cs  -- the speech-to-text seam
          - TranscriberFactory.cs  -- the Editor gets DebugTranscriber, a device none (no fake mic) until a real backend is added
          - DebugTranscriber.cs  -- editor stand-in that fakes transcription without a device; hears EditorPhrase (the POI Editor's Try a Query text, 2026-09-26)
          - VoiceSearchController.cs  -- plain C#: listen once, hand the transcript to the typed-query path; IsAvailable needs a backend
          - VoiceStateMachine.cs  -- idle/listening/processing states and their transitions
          - VoiceActivityIndicatorView.cs  -- the listening indicator bar
        - Minimap/  -- wall overview map (UI Toolkit)
          - MinimapLayout.cs  -- 2026-09-25. Pure: projection (wall x/y, floor x/z, auto = widest spread) and bounds (auto fit + margin, manual), world -> 0..1 map position
          - MinimapView.cs  -- plain C# view: one dot per running marker at its true world position (percent placement), three dot styles, result-set filtering, selection scale/fade, TapAt nearest shown dot
        - Zoom/  -- on-screen zoom buttons (UI Toolkit)
          - ZoomControlView.cs  -- zoom in/out/reset buttons routed to ARZoomController; mounts into its UIDocument on Start and shows only while Enable Zoom and Show On-Screen Buttons are on (ShouldShowButtons), 2026-09-25
          - ZoomControlView.uxml  -- button layout; links ZoomControlView.uss with a ui:Style element
          - ZoomControlView.uss  -- button styling, UI Toolkit USS only (no web-CSS gap/border/cursor/calc: UI Toolkit drops them with an import warning)
        - Shared/  -- cross-screen UI Toolkit support
          - PanelSettings.asset  -- the one runtime panel; 390x844 reference resolution
          - SafeAreaHelper.cs  -- notch and home-indicator padding; pure math exposed for testing
          - UIAccessibility.cs  -- contrast and tap-target helpers used by the accessibility tests
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
        - DisplacementGalleryDefinitions.cs  -- data list of overlap group sizes, angles and algorithms; drives DisplacementGalleryTests
        - DisplacementDemoLayout.cs  -- 2026-09-25. Pure placement of the dev-only displacement demo: four scenarios (Same level, Mixed levels, Big/Small, Lone) on a flat demo wall, members on a golden-angle spiral, each with an optional faded reference copy (side_by_side / overlay / off)
        - DisplacementDemoSpawner.cs  -- 2026-09-25. Dev-only "Add displacement demo": spawns the layout as real POI_Marker instances; live markers go to WallSession (LOD/displacement run on them), reference copies do not (never move); IsAllowed / ShouldSpawn (the LOD demo field wins) / PausesLod
        - OrientationGalleryDefinitions.cs  -- data list of the 11 orientation gallery entries (see _2.1 section 11)
        - OrientationGalleryHarness.cs  -- orbit/pitch/roll camera rig + SpawnEntry (shared by the visual harness and OrientationGalleryTests)
        - EffectsPreviewSpawner.cs  -- dev-only "Add effects demo grid" (effect_defaults.preview): 3 stacked blocks -- quick row (No effect/Pulse/Spin Ring), combo row (2 Ripple variants + a blank spacer cell + 3 Halo variants), one cell per hierarchy level using that level's real StyleOf style (size, Show Marker Label?, Marker Label Style, effects, reveal timing), labelled with the Base marker's own name (never the level name; PlainCircleName for the default base) -- 5000 m above the scene; layout maths delegated to DevPreviewGridLayout.cs (P5, now N-block); Editor and development builds only; called from WallSession
        - EffectsPreviewFocus.cs  -- lives on the grid root: owns the dedicated grid camera (main camera FOV/target, depth +100, neutral clear), copies the main camera rotation each frame, refits on aspect/FOV change, and applies a DevPreviewCameraDolly (scroll-zoom + WASD/mouse-drag pan, driven by DevCameraInput)
        - DevPreviewGridLayout.cs  -- Shared pure N-block grid layout maths (GridExtent/FitDistance/ChooseColumns/CellPositions; each block starts its own row) used by both EffectsPreviewSpawner and OutlinePreviewSpawner (2-block convenience overloads kept for the latter), so the camera-fit logic exists once
        - DevPreviewCameraDolly.cs  -- shared "move closer to a marker" manual zoom/pan offset on top of a demo grid's own auto-fit framing, clamped and reset on rebuild; deliberately pan-only, never independent rotation (every cell's MarkerBillboard always faces Camera.main, not a grid's own camera)
        - DevCameraInput.cs  -- shared free-fly input reader (WASD/RMB-drag/Alt+LMB-drag/arrows/scroll), gated on the mouse being over the Game view; used by both demo-grid Focus components and by MockLocalizationProvider (Runtime/Tracking) for the real wall camera
        - OutlinePreviewSpawner.cs  -- P5. Dev-only "Add outline demo grid" (outline_preview): one no-outline control cell + one cell per outline level (real colour/dash/spin, the wall's own "unknown" row reused if present rather than duplicated), 6500 m above the scene (different height than the effects grid); Editor and development builds only; called from WallSession
        - OutlinePreviewFocus.cs  -- Same shape as EffectsPreviewFocus (dolly zoom/pan included), for the outline grid's own camera
        - DemoFieldLayout.cs  -- 2026-09-25. Pure, seeded placement of the dev-only LOD demo field: markers per hierarchy level scattered through a box in front of the camera plus one dense clump (shared by the spawner and its tests)
        - DemoFieldSpawner.cs  -- 2026-09-25. Dev-only "Add LOD demo field" (demo_field): spawns the layout as real POI_Marker instances at a given pose in the wall's own space (no separate camera: LOD needs the real one); IsAllowed / ShouldSpawn = switch on + Editor or development build
        - DemoFieldStage.cs  -- 2026-09-25 (round 2). Plain C#: where a real-camera demo stands (Editor: an empty stage, 3500 m up for the LOD field and 4200 m for the displacement demo, the camera moved there and back; device: the start pose), kept while it is on; owns the root and its markers; Rebuild(shouldExist, camera, spawn) -- the demo's spawner decides what exists; WallSession holds one per demo; SearchDemoStagePosition (5700 m) for the search demo
        - SearchDemoLayout.cs  -- 2026-09-25. Pure: the dev-only search demo -- one column per wall category, levels / badges / outline types and the demo Material keyword field cycling, a column of named test POIs, a 5-point clump on the camera's line of sight; TestCases (2026-09-26: query, POI, the setting it depends on, Expected(settings)); DemoSynonymGroup / DemoMaterialField / AddDemoVocabulary
        - SearchDemoCheck.cs  -- 2026-09-26. Pure: runs the demo's test cases on a real index with the current search settings -> Verdicts ([ok] / [!!] lines) for the Live Search Readout and the tests
        - SearchDemoSpawner.cs  -- 2026-09-25. Dev-only "Add search demo": spawns the layout as real POI_Markers handed to WallSession (which then searches them); IsAllowed / ShouldSpawn (the LOD field and the displacement demo win) / PausesLod
      - Blocks/  [EMPTY SCAFFOLD]  -- the per-POI content block system (text/image/audio/video/model/map)
      - Circuits/  [EMPTY SCAFFOLD]  -- circuit state machine and entry-point resolution
      - Telemetry/  [EMPTY SCAFFOLD]  -- analytics events and consent
      - AI/  [EMPTY SCAFFOLD]  -- optional AI features, feature-flagged
    - Editor/  -- never ships; Editor-only tooling
      - TileStories.Editor.asmdef  -- editor assembly; refs Unity.TextMeshPro (2026-09-24, TMP_FontAsset for "Add font")
      - AssemblyInfo.cs  -- InternalsVisibleTo for the editor test assembly
      - POIEditorRigSafetyCheck.cs  -- warns on scene save or Play Mode if the editor rig still has markers in it
      - POIEditorRigBuildCheck.cs  -- hard-blocks a build while the rig has children, since a build is visitor-facing
      - DevFeatureBuildGuard.cs  -- pure: registry of developer-only config switches (name, how to turn off, works in release?) and which are ON and would take effect in a given kind of build; includes "Add LOD demo field" and "Add displacement demo" (2026-09-25); and "Add search demo"
      - DevFeatureBuildCheck.cs  -- IPreprocessBuildWithReport: reads every StreamingAssets config before a build; ConfirmOrStopBuild asks Build anyway / Cancel (EditorDecision) when a registered dev-only switch is ON (batch mode only warns)
      - POIEditor/  -- the POI Editor window; one partial class across many files (see _5.1_Editor_Tab.md)
        - POIEditorToolWindow.cs  -- shell: fields, menu item, OnGUI orchestration, DrawFramedFoldout, action bar; the Play / Build gate PromptBeforePlayOrBuild + BuildRigSafetyQuestion (labels from the real state; Save also copies to StreamingAssets, 2026-09-26)
        - POIEditorToolWindow.Constants.cs  -- option and label arrays, section colors, layout constants, help strings
        - MarkerSymbolTexturePostprocessor.cs  -- forces correct import settings on marker symbol textures
        - GlobalScene/  -- wall-wide settings sections
          - POIEditorToolWindow.GlobalScene.cs  -- section dispatch plus the Hierarchy Levels table (every row a TableRowScope): Hierarchy Level Name + Priority + Details, Marker Size (cm), Marker Label (Show Marker Label? + "Aa" Marker Label Style button), Effects, Spin Ring, Reveal, Facing Override, Search Keywords, Remove; NextFreeHierarchyLevelKey; HierarchyCheckboxRectProbe test seam; ends in its own Test sub-foldout (_2.3_Marker_Hierarchy.md)
          - POIEditorToolWindow.HierarchyHelp.cs  -- Hierarchy Levels domain (_2.3_Marker_Hierarchy.md): the "Add Hierarchy demo grid" and Search Keywords (i) help, the three Scene/Playmode/Device Test guides (app-agnostic, Editor Tab controls only)
          - POIEditorToolWindow.LabelsAndFonts.cs  -- 2026-09-22. "Labels, Text & Fonts" domain (_2.0_Labels_And_Fonts_Design.md): its own top-level Global Scene section, placed first; wall-default label Gap/Font size fields inside a collapsible "Marker Label" sub-foldout, the shared DrawLabelFontRows (Font popup + "Add font"), the wall font list row, a Test sub-foldout with an "Add Labels demo grid" switch sharing effect_defaults.preview, and DrawLevelLabelStyleEditor (the Marker Label Style window's body, run in this window's DrawConfigMutationScope; LevelLabelStyleProbe test seam)
          - POIEditorToolWindow.LabelsAndFontsHelp.cs  -- framework font key options, (i) help texts (incl. Add font and the Marker Label Style override) and the three Scene/Playmode/Device Test guides for the Labels, Text & Fonts domain; every text names an Editor Tab control, never a project doc or code file (_5.1_Editor_Tab.md, "Domain Manual Tests")
          - POIEditorToolWindow.MarkerDesign.cs  -- Marker (Label gap/font-size sliders + Font popup since 2026-09-22, _2.0_Labels_And_Fonts_Design.md), Badge and Outline sections (_2.2.1/_2.2.2/_2.2.3), each on the shared field-drawer helpers and ending in a Test sub-foldout; Icon color / Icon size rows, a "Badge Categories" title, the Outline Types (i) and TableRowScope on the Outline table (2026-09-24)
          - POIEditorToolWindow.MarkerDesignHelp.cs  -- Marker/Badge/Outline option arrays, (i) help texts (incl. Label gap/font-size/font key, 2026-09-22), and the nine Scene/Playmode/Device test guides (three domains x three tiers); the per-POI (Specific Marker) (i) texts and Icon color/size, Outline Types help (2026-09-24)
          - POIEditorToolWindow.DemoGridExclusivity.cs  -- MakeThisTheOnlyActiveDemoView: only one of the effects/hierarchy grid, the outline grid, the LOD demo field, the displacement demo and the search demo can be seen at once; ticking one unticks the others
          - POIEditorToolWindow.Orientation.cs  -- Orientation section, 4 sub-foldouts: Vertical Alignment, Facing Options, Update Cost, Test (Scene-Mode Preview toggle + three collapsed guides: Scene, Playmode, Device; see _2.1 section 8); its Test is the shared DrawDomainTestSubSection since 2026-09-24
          - POIEditorToolWindow.Effects.cs  -- Effects section: master toggle (hides everything when off), one foldout per effect with its own enabled checkbox, used-by line, (i) buttons and parameter rows
          - POIEditorToolWindow.EffectsHelp.cs  -- every Effects constant: level-table option arrays, (i) help texts, the three Test guides
          - EffectUsageSummary.cs  -- pure text/list logic of the Effects page: used-by and per-level effects lines, level-table dropdown filtering, preview base-marker options
          - POIEditorToolWindow.EffectsTest.cs  -- the Effects part of its Test sub-foldout: _effectsTest state and the "Add effects demo grid" switch + Base marker (DrawEffectsPreviewSwitch); the foldout itself is the shared DrawDomainTestSubSection, drawn even while Enable effects is off (see _2.2.4)
          - POIEditorToolWindow.LodZoom.cs  -- LOD section (Enable LOD; sub-foldouts Distance Bands with its TableRowScope table + Suggest Values, Crowding, Clusters, Transitions & Performance; Test with the LOD demo field controls and the Live LOD Readout) and Zoom section (Zoom Range, Gestures & Buttons, Test), rebuilt 2026-09-25; LodLiveReadout (the readout's text) and LodBandCellRectProbe test seams
          - POIEditorToolWindow.LodZoomHelp.cs  -- 2026-09-25. LOD / Zoom option arrays, every (i) help text and the six Scene/Playmode/Device Test guides (app-agnostic, Editor Tab controls only)
          - LodEditorRules.cs  -- 2026-09-25. Pure: which LOD rows matter for the chosen Response (mirrors LODController's own rules) and what is wrong with a Distance Bands table
          - POIEditorToolWindow.Displacement.cs  -- Displacement section (_2.5, rebuilt 2026-09-25): Enable Displacement, sub-foldouts Overlap Detection, Resolution (What Moves, Algorithm, Who Gives Way, Max Move), Leader Lines, Test (Add displacement demo + its rows, Live Displacement Readout / DisplacementLiveReadout, the three guides)
          - POIEditorToolWindow.DisplacementHelp.cs  -- 2026-09-25. Displacement option arrays, every (i) text and the three Scene/Playmode/Device Test guides (app-agnostic, Editor Tab controls only)
          - POIEditorToolWindow.SearchFilter.cs  -- Select, Filter & Search section (_2.6, rebuilt 2026-09-25): Enable, sub-foldouts Selection / Search / Filters / Results & Views / Minimap / Voice (one per select_filter_search sub-block), Keywords & Synonyms (Keyword Fields table with a Filter column (2026-09-26) + Synonym Groups table, TableRowScope), Test ("Add search demo" + rows, Try a Query (also the Editor mic's phrase), Demo Query, Live Search Readout / SearchLiveReadout ending in the demo's "Demo Test Cases", the three guides); NextFreeSearchFieldKey
          - POIEditorToolWindow.SearchFilterHelp.cs  -- 2026-09-25. Option labels (values come from SelectFilterSearchOptions), every (i) text (incl. the per-POI Summary & Keywords texts since 2026-09-26) and the three Scene/Playmode/Device Test guides (app-agnostic, Editor Tab controls only)
        - LivePlayModeConfig/  -- Editor-assembly only, never ships: pushes window edits to the running wall while Play Mode runs (see _2.2.4 section 3.7)
          - ILivePlayModeApplier.cs  -- one domain's live re-apply contract: Name, Fingerprint(config), Apply(session, configCopy)
          - LivePlayModeConfigDispatcher.cs  -- plain C#: remembers each domain's last fingerprint per running wall, applies only changed domains, hands each a private config copy
          - LivePlayModeEffectsApplier.cs  -- Effects domain applier: fingerprint = effect_defaults + hierarchy levels; Apply calls WallSession.ApplyEffectSettings
          - LivePlayModeOrientationApplier.cs  -- Orientation domain applier: fingerprint = orientation_settings + each level's facing override; Apply calls WallSession.ApplyOrientationSettings then LODController.ReapplyClusterOrientation (see _2.1 section 13.5); plus each POI's Facing X/Y/Z -> WallSession.ApplyPoiFacing (2026-09-24)
          - LivePlayModeMarkerApplier.cs  -- Marker/Badge/Outline domain applier: fingerprint = every wall-level marker field + the taxonomy tables + each POI's own marker/badge/status fields + each hierarchy level's non-effect, non-facing columns (level_name/priority/details/size/show_label/rotate_contour/reveal timing/search keywords/Marker Label Style); Apply calls WallSession.ApplyMarkerSettings (see _2.2.1 section 3.3, _2.3 section 8, _2.0_Labels_And_Fonts_Design.md); each POI's name (label text) and the icon fields since 2026-09-24
          - LivePlayModePoiLevelApplier.cs  -- 2026-09-24. A POI's own hierarchy_level_key: fingerprint = each POI's id=key; Apply calls WallSession.ApplyPoiHierarchyLevels (own applier because that one field drives several domains, _2.3 section 8)
          - LivePlayModeLodApplier.cs  -- 2026-09-25. LOD domain: fingerprint = lod_settings; Apply calls WallSession.ApplyLodSettings
          - LivePlayModeZoomApplier.cs  -- 2026-09-25. Zoom domain: fingerprint = zoom_settings; Apply calls WallSession.ApplyZoomSettings
          - LivePlayModeDemoFieldApplier.cs  -- 2026-09-25. LOD demo field: fingerprint = demo_field; Apply calls WallSession.ApplyDemoField (rebuilds the field)
          - LivePlayModeDisplacementApplier.cs  -- 2026-09-25. Displacement domain: fingerprint = displacement_settings; Apply calls WallSession.ApplyDisplacementSettings
          - LivePlayModeDisplacementDemoApplier.cs  -- 2026-09-25. Displacement demo: fingerprint = displacement_demo; Apply calls WallSession.ApplyDisplacementDemo (rebuilds the demo)
          - LivePlayModeSearchApplier.cs  -- 2026-09-25. Select, Filter & Search: fingerprint = select_filter_search + search_fields + synonym_groups + each POI's summary / keywords; Apply calls WallSession.ApplySearchSettings
          - LivePlayModeSearchDemoApplier.cs  -- 2026-09-25. Search demo: fingerprint = search_demo; Apply calls WallSession.ApplySearchDemo
          - LivePlayModeConfigPush.cs  -- the one entry point the window calls after a change; registers every domain applier; does nothing outside Play Mode; CreateDispatcher is the one applier registration list, also used by LiveSyncFieldMatrixTests
        - SpecificMarker/  -- per-POI editing
          - POIEditorToolWindow.SpecificMarker.cs  -- the POI list, header row (focus, rename, reorder, add, help, delete) and per-POI style sections; per-POI Marker Style / Badge Style / Outline rows on the shared drawers with (i) help and stale-safe reference popups; a first POI gets the first hierarchy level; ticking Status unknown selects the unknown outline type AND badge (2026-09-24); the "Summary & Keywords" section (2026-09-26: Summary text area, one list per Keyword Field, Others, read-only Found by = SearchKeywordSources via FoundByText) never writes the config by drawing (a keyword-field list is created only when typed into)
          - POIEditorToolWindow.PositionTabs.cs  -- the Position foldout: three Facing X/Y/Z sliders (disabled while Verified), XYZ readout, Verified toggle
          - POIEditorToolWindow.MarkerSceneEdit.cs  -- selected-marker edit handling: facing sync from Scene view/Inspector, Verified lock on facing and position, auto-reveal of the POI's section
          - MarkerEditDetector.cs  -- pure tracker: did the selected marker's pose change since last seen, plus the gizmo-drag gesture check (drives the auto-reveal)
          - FacingEditAdvice.cs  -- pure: which facing edits are invisible under Scene-Mode preview for the effective facing mode, and the Scene-view warning text for them
          - PoiFocusResolver.cs  -- pure math for framing a marker in the Scene view
          - PoiRenameKeys.cs  -- Enter/ESC commit handling for the rename field
          - PoiRotationResolver.cs  -- pure Facing X/Y/Z math: angles to quaternion, angle normalisation, IsSameOrientation (compare rotations as orientations, never euler triples)
          - ReferencePopupOptions.cs  -- 2026-09-24. Pure: the option list of a taxonomy-reference popup ("(none)", the rows, a stale key kept as "<key> (missing)") and its index <-> key mapping
          - Icons/  -- add, delete, edit and focus button PNGs
        - RigLifecycle/
          - POIEditorToolWindow.RigLifecycle.cs  -- creates, refreshes, captures from and clears the Edit-Mode marker rig; calls the real Initialise methods; also owns the orientation preview (ApplyOrientationPreview, RestoreRigRotationsFromConfig, see _2.1 section 9); PrepareRigVisuals now also resolves the wall's font library (2026-09-24 fix, _2.0_Labels_And_Fonts_Design.md), so the Scene-view rig matches Play Mode's font resolution; Load & Populate asks ONE question (BuildReloadQuestion: Save & Reload / Discard & Reload) only when work is at stake (2026-09-26)
        - ConfigData/
          - POIEditorToolWindow.ConfigFileIO.cs  -- load, save and copy-to-StreamingAssets
          - POIEditorToolWindow.ConfigHistory.cs  -- JSON-snapshot undo/redo, separate from Unity's Undo; CreateSymbolPickerPopup / CreateDetailsPopup (2026-09-24): every config-writing popup is built here so its write-back runs in a mutation scope (undo, unsaved, rig refresh, live push); IsStillEditing closes it once an undo/reload replaced the config (2026-09-26)
          - POIEditorToolWindow.ConfigValidation.cs  -- non-blocking warnings: hierarchy keys, size ranges, the LOD crowding thresholds and the Distance Bands table (ValidateLodBands, 2026-09-25); every Select, Filter & Search option string against SelectFilterSearchOptions (ValidateSearchEnumFields) and required keyword fields
        - AssetPaths/
          - POIEditorToolWindow.AssetPaths.cs  -- browse rows, absolute-to-Assets path conversion, Resources path rules; TryResolveWallFontLibraryFromConfig/EnsureWallFontLibrary/AddFontToWallLibraryAndGetKey/GetAvailableFontKeyOptions -- the wall font library's locate/create/register/list-keys logic, same home as the equivalent icon-library methods
        - Shared/  -- reusable pieces every section draws through
          - POIEditorToolWindow.FieldRows.cs  -- 2026-09-25 (round 2). The shared labelled-row drawers every section uses (DrawScalarField, DrawIntField, DrawToggleField, DrawPopupField, DrawReferencePopupField, DrawSliderField, DrawIntSliderField, DrawColorField); each one DrawEditorRow...EditorRowEnd, never touching indentLevel (moved out of LodZoom.cs)
          - POIEditorToolWindow.RowLayout.cs  -- DrawEditorRow / EditorRowEnd: the one shared indent + width-capped row; TableRowScope (2026-09-24): a table row that pays the indent once and zeroes indentLevel so EditorGUI Rect cells are not shifted by IndentedRect
          - POIEditorToolWindow.DomainTest.cs  -- 2026-09-24. The ONE Test sub-foldout every Global Scene domain ends with: TestGuideState, DrawDomainTestSubSection (optional preview switch + three collapsed guides) and DrawTestGuideFoldout
          - POIEditorToolWindow.IconButton.cs  -- the one icon-only button look, used by every icon and help button
          - POIEditorToolWindow.DeleteButton.cs  -- the one shared destructive-delete-button look (Marker Table's trash glyph, IconButtonSize default), used by every delete affordance in the window
          - POIEditorToolWindow.SymbolTable.cs  -- the shared taxonomy table renderer and sprite preview/picker; rows in TableRowScope and a TableCellRectProbe test seam (2026-09-24)
          - EditorAlertItem.cs  -- one config-validation finding plus the plain-text report builder (first 6 findings + guidance)
          - Popups/  -- 2026-09-26. EVERY popup of the POI Editor, two kinds by intent (_5.1_Editor_Tab.md, "Popups: the two kinds and when to use which")
            - EditorDecision.cs  -- the ONE blocking kind: Ask(title, message, confirm, alternative?, dontAskAgainKey?) -> DecisionAnswer over the native modal dialogs; Resolve (Esc / X / unexpected = Cancel), Responder (tests click a button without a dialog), don't-ask-again
            - EditorPopup.cs  -- the ONE non-blocking kind: a floating draggable utility window (title bar X, Esc closes, one open per kind, ShowAt next to the clicked rect) + EditorPopupContent (Kind, Title, InitialSize, IsAlive, Draw, OnClosed)
            - HelpInfoPopup.cs  -- content: read-only framework help text, plus HelpInfoButton.Draw (every (i) button)
            - EntryDetailsPopup.cs  -- content: a developer-editable note persisted into config (built by CreateDetailsPopup); NOT the same as help
            - ExistingSymbolPickerPopup.cs  -- content: curated sprite picker limited to wall and framework icons (built by CreateSymbolPickerPopup; a pick closes it)
            - LevelLabelStylePopup.cs  -- content: the "Aa" Marker Label Style of one hierarchy level, held by key; its body is the owner's DrawLevelLabelStyleEditor, run in the owner's DrawConfigMutationScope (functional config, not a free-text note -- _2.0_Labels_And_Fonts_Design.md section 4)
            - EditorNotice.cs  -- the one queue for short informational messages (locked edit, hidden facing change, nothing to clear, rejected rename, validation report); an editor-update pump shows each as a non-blocking NoticePopup, debounced for drag gestures; NoticeKeys (every hideable notice/question, reset by TileStories > Reset Hidden Messages)
            - NoticePopup.cs  -- content: one notice as a message box -- the text, then one bottom row: "Don't show this again" on the left (advisory notices only), the default OK on the right (Enter presses it); the checkbox is committed when it closes, however it closes (OnClosed)
          - IdentityRenameResolver.cs  -- the one rename rule for every identity key; rewrites all referencing POIs
          - IdentityRenameEditState.cs  -- commit-style edit session for identity cells; survives undo swaps
          - IdentityDeleteGuard.cs  -- EditorDecision question before deleting a row POIs still reference
          - SearchFieldReferenceResolver.cs  -- the one nested identity: search field key inside each POI's keyword list
          - KeywordListText.cs  -- 2026-09-25. Pure Parse / Join of a comma-separated keyword list: the one text form of every Search Keywords cell, keyword field and synonym row (DrawKeywordListField)
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
        - TileStories.Editor.Tests.asmdef  -- refs Unity.TextMeshPro added 2026-09-22 (TMP_FontAsset assertions, _2.0_Labels_And_Fonts_Design.md)
        - ARZoomMathTests.cs, ARZoomStateTests.cs  -- zoom math and state clamping
        - CategoryPaletteTests.cs, StatusRampTests.cs, MarkerVisualsParserTests.cs  -- taxonomy resolution
        - MarkerHierarchyResolverTests.cs (incl. Marker Label Style resolution and StyleOf clamping, 2026-09-24), MarkerLayoutTests.cs, MarkerLayoutPxConversionTests.cs  -- size and layout math
        - ClusterGroupingTests.cs, ClusterReconcileTests.cs, ClusterPrefabTests.cs  -- cluster grouping and prefab contract
        - DisplacementComputeTests.cs, DisplacementAlgorithmTest.cs, DisplacementHysteresisTests.cs, DisplacementTieBreakStrategyTests.cs, DisplacementSettingsDefaultsTests.cs  -- displacement math and defaults
        - DisplacementAuthoringRoundTripTests.cs  -- every displacement_settings and displacement_demo field survives a JSON round trip (walked by reflection); a config without the blocks gets the defaults, demo off
        - DisplacementEditorTabTests.cs  -- 2026-09-25. Global Scene > Displacement on the real window: help/guide contract (ASCII, app-agnostic, no code names; a source scan makes every drawn row appear in the Playmode guide), option arrays vs runtime values, undo/redo of every field, live fingerprints (reflection) + registration, real OnGUI in every branch, readout wording
        - DisplacementDemoLayoutTests.cs  -- 2026-09-25. Each displacement demo control on the pure layout (group size + clamp, spread incl. irregular spiral, distance scaling, reference modes, levels by Priority), the spawner's allow/pause/LOD-field-wins rules, TakesPart
        - LeaderLineGeometryTests.cs  -- 2026-09-25. Pure leader-line geometry: the screen-space elbow corner, the stop at a label's text edge
        - LODControllerTests.cs, LodAutoSuggestTests.cs, LodAuthoringRoundTripTests.cs, DensityThresholdValidationTests.cs  -- LOD logic and authoring
        - LodZoomEditorTabTests.cs  -- 2026-09-25. LOD/Zoom sections: a real click on a band's trash button + real Ctrl+Z; Suggest Values keeps every other field; LodEditorRules agree with LODController's own rules per Response; help/guide contract (ASCII, app-agnostic, every option and every drawn control explained); Test foldout defaults
        - DemoFieldLayoutTests.cs  -- 2026-09-25 (round 2). Each LOD demo field control one by one on the pure layout: per-level counts + clamp, clump count/radius, Field Distance/Depth/Width/Height bounds, Reshuffle, categories
        - LodStatsTests.cs  -- 2026-09-25 (round 2). LodStats counts every outcome once (clusters, capped clusters, culled), the readout's wording, DemoFieldStage's pure Editor/device placement rules
        - LodZoomLiveSyncTests.cs  -- 2026-09-25. Every LodSettings / ZoomSettings / DemoFieldSettings field (walked by reflection) pushed live through the real applier list onto a real WallSession; the demo field switched on/off live, Show labels off
        - LivingRoomSceneWiringTests.cs  -- 2026-09-25. The SAVED wall scene holds LODController (+POI_Cluster) on the WallSession object, and ARZoomController / ARZoomGestureInput / ZoomControlView wired to it
        - POIPositionResolverTests.cs  -- null position falls back to origin
        - SearchTokenizerTests.cs  -- tokenisation: accent folding, splitting, dedup
        - SearchEngineTests.cs  -- 2026-09-25. POISearchIndex + SearchTextDistance on a fabricated wall: every source at its rank, each option (Match Words, Partial Words, Typo Tolerance), synonyms both ways and capped, filters by id, the review's regressions, the shipped wall's names; (2026-09-26) SearchKeywordSources: every "Found by" word finds its POI at its rank, origins, FoundByText
        - SearchFilterEditorTabTests.cs  -- 2026-09-25. The Select, Filter & Search section: help/guide contract, every drawn row and option explained, labels fit, undo/redo + JSON round trip + live fingerprints of every field (reflection), shipped configs, validation, build guard, real OnGUI in every branch, drawing never writes
        - KeywordListFieldTypingTests.cs  -- 2026-09-25. Typing "castle, tower" into the REAL keyword cell (real click + key events) gives two keywords
        - MarkerOrientationResolverTests.cs  -- Tier-0 tests for the orientation resolver (_2.1 section 5): vertical alignment, facing modes, degenerate/pitch-clamp guards, update gating
        - EditorCameraLookTests.cs  -- mock-camera look math including roll (_2.1 section 13.3)
        - PoiRotationResolverTests.cs, PoiFocusResolverTests.cs, PositionTabsTests.cs  -- per-POI editor math and Position foldout shape
        - PoiFacingLockAndSyncTests.cs  -- Facing X/Y/Z: orientation-based scene sync, Verified lock on facing, marker-edit detector, auto-reveal state, real IMGUI slider rows, LivingRoom config round trip
        - EditorNoticeTests.cs  -- notice queue timing (discrete vs drag), validation report text, hide/reset of notices, converted call sites (ClearRig, config validation, facing warnings)
        - TestDialogGuard.cs  -- SetUpFixture OUTSIDE any namespace (so it covers both test namespaces): EditorDecision answers Cancel and notice popups stay off for the whole EditMode run, so no test can hang on a modal
        - PoiFacingModesAndLifecycleTests.cs  -- per-mode/per-axis visibility on a real POI_Marker prefab under preview, preview warnings vs real pipeline, gizmo under preview, verify/unverify wiring, leaked scene-handler check, reveal incl. blocked edits
        - IdentityRenameResolverTests.cs, IdentityRenameCompositionTests.cs  -- rename rule and its composition across tables
        - HierarchyLevelKeyValidationTests.cs, HierarchyLevelSizeRangeTests.cs  -- config validation
        - HierarchyDesignAuthoringTests.cs  -- Hierarchy Levels: guide-contract (ASCII, app-agnostic, every column group named), a forbidden-terms scan over every hierarchy + labels help/guide text (no docs, code, wall names or stale column names), NextFreeHierarchyLevelKey never repeating a key, default Test-foldout state, Search Keywords JSON round trip, Live Play Mode fingerprint ownership, Ripple/Halo/Facing Override dropdown-to-runtime-parse checks, real OnGUI smoke tests across every conditional-column branch
        - HierarchyTableClickTests.cs  -- 2026-09-24. Hosts the REAL POIEditorToolWindow.OnGUI and sends real mouse clicks to the Show Marker Label?/Pulse/Spin Ring checkboxes (found via HierarchyCheckboxRectProbe); proves the IndentedRect fix (TableRowScope) and that a switched-off Pulse stays read-only; a real click on a row's trash while a POI uses the level: Cancel keeps it, Delete anyway deletes it (2026-09-26)
        - MarkerLabelStyleWindowTests.cs  -- 2026-09-24. The REAL Marker Label Style popup (LevelLabelStylePopup in the shared EditorPopup) on a real POIEditorToolWindow: fixed title + single instance, override-off rows show/follow the wall default, a real click on the override seeds from the wall default and marks unsaved, a real Ctrl+Z undoes it and the window keeps editing the live config, the window closes when its level is deleted
        - OrientationEditorRoundTripTests.cs  -- OrientationSettings JSON round-trip, no-block defaults, editor foldout field-existence, deleted-field absence checks, test-guide contract (ASCII, every field and option named, collapsed by default) (_2.1 sections 7, 13)
        - EffectsAuthoringTests.cs  -- Effects authoring: editor dropdown strings vs MarkerHierarchyResolver, JSON round trip and undo/redo of EVERY effect field (walked by reflection) through the real window history, shipped LivingRoom config, guide and help-text contract (_2.2.4)
        - EffectsUsageAndPreviewLayoutTests.cs  -- pure Effects logic: per-effect switches (FilterEnabled), usage and per-level lines, dropdown filtering, preview cell list and placement (quick row/combo row/spacer/level row, N-block geometry; level cells show the base marker name and carry the level's Marker Label Style, 2026-09-24), release-build guard
        - MarkerDesignAuthoringTests.cs  -- Marker/Badge/Outline authoring: every editor-offered option (Outline Color, Background shape) parses at runtime (the free_colors bug regression), Ring/Badge raycastTarget=false regression (_2.2.1); Labels & Fonts guide-contract test (2026-09-22, _2.0_Labels_And_Fonts_Design.md)
        - MarkerDomainsHelpTextTests.cs  -- 2026-09-24. Every Marker/Badge/Outline/Effects/Orientation/per-POI help, guide and validation notice: ASCII, no doc/code/wall/dev-scene names, no stale control names, no claim the runtime contradicts
        - LiveSyncFieldMatrixTests.cs  -- 2026-09-24. One test per Editor Tab field of these domains and every POI field: real WallSession + real markers, pushed through LivePlayModeConfigPush.CreateDispatcher, the running marker must change; plus a completeness guard (every config field is a row or a reasoned exclusion)
        - MarkerVisualResolverTests.cs  -- 2026-09-24. Ring row by status_level_key (two rows at the same pct), pct fallback, fallback-badge colour, same-hue shading, no badge without a status
        - POIEditorPopupEditTests.cs, PoiReferencePopupTests.cs, TaxonomyTableClickTests.cs, DomainTestFoldoutTests.cs  -- 2026-09-24. Popup edits get undo/unsaved (+ source scan for the factories); a stale per-POI key survives the real OnGUI; a real click at the left edge of a Category/Badge Key/Outline label cell starts editing (TableRowScope); every domain Test foldout open with collapsed guides
        - MarkerVisualSettingsTests.cs  -- MarkerVisualSettings.Resolve: label_gap_ratio/label_font_size_ratio read-through and clamp (widened to 0-1/0.08-1.5, 2026-09-24), label_font_key read-through/blank-fallback, font library pass-through/null-fallback, null-config framework defaults (_2.0_Labels_And_Fonts_Design.md)
        - LabelsAndFontsAuthoringTests.cs  -- 2026-09-24. GetAvailableFontKeyOptions unions the wall's own FontKeyLibrary keys with the 3 framework keys; AddFontToWallLibraryAndGetKey registers a font (key from its name, idempotent, suffixed on a clash) and the popups offer it; PrepareRigVisuals resolves the wall font library; the label slider limits are the exact limits the runtime clamps to (_2.0_Labels_And_Fonts_Design.md)
        - DemoGridExclusivityTests.cs  -- MakeThisTheOnlyActiveDemoView: each of the five demo views turned on leaves only itself on; an unrelated edit touches none
        - DevPreviewCameraDollyTests.cs  -- pure zoom/pan math: scroll/WASD accumulation, clamp ranges, Reset, SetOffsets absolute-set (2026-09-22, the carry-over WallSession uses across a rebuild)
        - DevCameraInputTests.cs  -- DevCameraInput baseline (returns exactly None with nothing pressed); the gated/per-key branches need real OS input, covered live instead (2026-09-22)
        - MarkerTaxonomyReferenceValidationTests.cs  -- ValidateMarkerTaxonomyReferences: stale category/badge/status-level/custom-symbol references, empty-taxonomy no-op case (_2.2.1)
        - DevFeatureBuildGuardTests.cs  -- the dev-only switch build guard: reported in development builds with the how-to text, ignored in release builds (matches EffectsPreviewSpawner.IsAllowed), nothing when OFF, registry entries complete
        - LivePlayModeConfigTests.cs  -- live Play Mode config: dispatcher rules (first push, unchanged, one domain, new wall, private copy); the Effects, Orientation and Marker appliers driving a real WallSession and real markers (every orientation field walked by reflection for the fingerprint, level facing override, effects untouched; marker fingerprint covers every wall+POI marker field, real markers recolour/hide live; hierarchy-level Size/Text Label/Spin Ring/Search Keywords; label_gap_ratio/label_font_size_ratio reaching a real marker's Label RectTransform/TMP font size, 2026-09-22; label_font_key reaching a real marker's real TMP font asset; a level's Marker Label Style on/off and wall edits on real markers; a POI moved to another level live via LivePlayModePoiLevelApplier -- size, label, effect and facing on real markers, 2026-09-24)
        - OrientationEditModePreviewTests.cs  -- Scene-Mode orientation preview: non-destructive guarantee, on/off cycle byte-identical, CapturePositions guard, wall_fixed Edit/Play-Mode agreement, Label/Badge tick and restore on a real POI_Marker (_2.1 section 9)
        - DefaultCategoryStylesTests.cs, DefaultBadgeCategoriesTests.cs, DefaultOutlineLevelsTests.cs  -- seeding defaults
        - POIEditorToolWriteBackTests.cs, POIEditorAddPoiTests.cs  -- editor data flow
        - EditorDecisionTests.cs  -- 2026-09-26 (replaced ReloadGuardChoiceTests.cs). EditorDecision: the one result mapping (Esc / X / unexpected = Cancel), the responder, don't-ask-again + reset; the gate / reload question builders' labels per state; ONE test per button of every question on real files under Temp/ and a real rig (Clear Rig, Load & Populate's Save & Reload / Discard & Reload / Cancel, Delete POI, IdentityDeleteGuard, unverify, the Play gate's Save, Clear & Play / Clear & Play / Play With Duplicates / Cancel, the Build gate, the developer-only switch build check); a source scan for any other dialog / PopupWindow
        - EditorPopupTests.cs  -- 2026-09-26. EditorPopup with real windows and real events: a click on (i) opens a floating draggable popup, a second (i) retargets it, Esc closes, no Close button on a working popup, real typing in a Details note is undoable and an undo closes the note, a real click on a picker row picks + closes; a notice opens without blocking with its checkbox left and OK right on one row, OK / Enter close it, the checkbox counts on OK and on X, a refusal has OK and no checkbox
        - POIEditorTableLayoutTests.cs, POIEditorAddButtonRowRenderTests.cs, POIEditorCoordinateRowRenderTests.cs, POIEditorColorGroupRenderTests.cs, POIEditorVisualHierarchyTests.cs, POIEditorWindowChromeTests.cs  -- real IMGUI geometry measurement
        - MarkerSymbolTexturePostprocessorTests.cs, PanelSettingsTests.cs, SafeAreaHelperTests.cs, ZoomControlViewEditModeTests.cs  -- assorted contracts
        - UI/  -- UI Toolkit view tests, mirroring Runtime/UI's folder names
          - Navigation/SearchUiRulesTests.cs  -- 2026-09-25. The rules and views without a scene: bus toggle, alpha truth table, zoom rules (+ the keep-on-screen cap, 2026-09-26), panels, view start, result set + relax, facets + tray (+ Keyword Field filter groups), list, card, bar, voice, minimap layout + view, the demo layout (every filter value covered), every demo test case's verdict right for all 18 search-setting combinations, demo rules
          - Search/RecentSearchesManagerTests.cs, SuggestedSearchesManagerTests.cs, VoiceStateMachineTests.cs, VoiceActivityIndicatorViewTests.cs
          - Shared/UIAccessibilityTests.cs
        - MarkerAssets/  [EMPTY SCAFFOLD]  -- fixtures for marker asset tests
      - Runtime/  -- PlayMode suite: anything needing a scene, a camera, coroutines or Destroy
        - TileStories.Tests.asmdef  -- refs TileStories, nunit, UnityEngine.UI (EventSystem taps, 2026-09-25)
        - MarkerGalleryTests.cs, ClusterGalleryTests.cs, DisplacementGalleryTests.cs, OrientationGalleryTests.cs  -- Phase A gallery assertions, driven by the same definition lists as the harnesses
        - MarkerOverlapResolverTests.cs  -- the marker billboard rotation contract
        - MarkerViewRuntimeTests.cs, MarkerRevealEffectTests.cs, MarkerIconLibraryRuntimeTests.cs  -- marker visuals at runtime
        - MarkerConfigDrivesMarkerTests.cs  -- real WallSession + real prefab + the real shipped LivingRoom config's 4 dev marker-design fixtures: no status (no ring/badge), no badge category (status-coloured fallback badge), no hierarchy level (framework fallback size), custom symbol (icon overridden, category fill untouched) (_2.2.1 section 6)
        - OutlinePreviewRenderTests.cs  -- P5. Real WallSession + real prefab + real shipped config: the outline demo grid's cell count/naming (control + one per level, the wall's own "unknown" row not duplicated), per-mode ring colour (Uniform shared, Per outline type per-row), only level cells spin, live rebuild on ApplyMarkerSettings (_2.2.3 section 6)
        - MarkerEffectConfigTests.cs  -- every effect config value drives a real marker: each effect uses its OWN block (no leaking), parameters vs the spec formula each frame, all 24 ripple x halo x pulse combinations, LivingRoom levels, master and per-effect switches, period <= 0 safety, no base-scale drift, level reveal timing (_2.2.4)
        - EffectsPreviewSpawnerTests.cs  -- the effects preview grid through the real WallSession.SpawnPOIs: off = nothing, quick/combo/level rows with the right effects/size/labels (2026-09-22: level cells use the real table level_name + show_label, not the key), all inside the camera view, plain grey circle vs a copied real POI, switches respected; plus WallSession.ApplyEffectSettings turning the grid on/off/rebuilding, ApplyMarkerSettings rebuilding it live on a hierarchy Size edit, DevPreviewCameraDolly scroll/pan actually moving the real grid camera, and (2026-09-22 follow-up) a live rebuild carrying the developer's zoom/pan across it instead of resetting; (2026-09-24) level cells labelled with the Base POI's own name, and a level's Marker Label Style reaching its grid cell's real TMP font size/offset live
        - EffectsPreviewRenderTests.cs  -- render-level proof of the focus grid: real camera renders to a portrait texture with a wall 50 cm in front of the main camera; every cell drawn, main camera never draws the grid, every effect visible against an effects-off render and animated, effects keep their look on 7/20/30 cm markers, labels stay readable while the main camera rotates and rolls, refit on a resize; saves a PNG under Assets/Screenshots
        - LODControllerEvaluateTests.cs, ClusterPipelineIntegrationTests.cs, ClusterReconcilePlayModeTests.cs  -- the real LOD pipeline end to end
        - LodRealPipelineTests.cs  -- 2026-09-25. Real WallSession + real prefabs + real LODController + real POI_Cluster: a hidden marker comes back, Shrink & Fade scales/fades to the floor, a marker-sized tappable cluster (projected px), Band Source decides the band, LOD off live restores everything, selection dim survives a hide; the LOD demo field (zoom reveals more markers, the clump clusters) and per-mode renders with pixel checks (dark centre disc, closed pie) saved under Assets/Screenshots
        - LodWallSceneTests.cs  -- 2026-09-25 (round 2). Loads the REAL LivingRoomScene (room mesh, mock camera, shipped config) and unloads it after: the demo field on its stage, unoccluded, facing, drawn (pixel diff), each control, the camera and wall back when off; the Live LOD Readout against real markers; the zoom buttons styled + docked + following Show On-Screen Buttons; two quick zoom-in clicks = two levels
        - DisplacementLabelTests.cs  -- label displacement against real transforms
        - DisplacementDemoFixture.cs  -- 2026-09-25. Shared set-up: shipped config, real WallSession + POI_Marker + LODController, MainCamera into a fixed 1280x720 target; drives the displacement demo and settings through WallSession's live seams; renders PNGs
        - DisplacementDemoTests.cs  -- 2026-09-25. Each displacement demo control one by one on real markers: switch (stage, camera, wall paused and back), Markers per Group, Spread, Distance, Show labels, Reference copies (a copy never moves), Run LOD, the LOD demo field wins
        - DisplacementSettingsRealMarkerTests.cs  -- 2026-09-25. Every Displacement setting one by one on real markers (enable/off restores, overlap distance, what moves incl. live switching, each algorithm + renders, who gives way, max move + label hiding, leader lines in Label only and Marker, styles, min length, width/opacity, no magenta pixels), hidden markers never take part
        - ARZoomRoutingTests.cs, ARZoomCameraFovTests.cs  -- zoom routing and real FOV change
        - LivingRoomConfigIntegrationTests.cs  -- editor to StreamingAssets to runtime config contract
        - OrientationWallSessionIntegrationTests.cs  -- real WallSession.SpawnPOIs orientation wiring + hierarchy override + real-marker occlusion check (_2.1 section 6)
        - OrientationLiveUpdateTests.cs  -- live orientation changes on a RUNNING wall: real rotations of real markers and a real cluster after real frames (world_up/screen_up, Label alone, wall_fixed round trip keeps the authored rotation, level override, on_camera_delta change not delayed) (_2.1 section 13.5)
        - OrientationClusterIntegrationTests.cs  -- real LODController cluster spawn path gets MarkerBillboard.Configure; cluster rotation changes when camera rotates (_2.1 section 6.4)
        - SearchSceneFixture.cs  -- 2026-09-25. Shared set-up: the REAL LivingRoomScene with its SearchUI, real raycast taps (TapScreen), live panel presses (Press), ApplySearch, Game-view renders (Search_*.png)
        - SearchSceneTests.cs  -- 2026-09-25. On the real scene: one set on list / map / markers, chip taps + relax, the view switch, real marker taps (select, dim, card, re-tap), selection x filter, every setting pushed live, minimap, voice, recent -> suggestion, contrast + tap targets
        - SearchDemoTests.cs  -- 2026-09-25. The search demo on the real scene: stage + camera + give-back, every test case's verdict (SearchDemoCheck) on the running index and through the UI, each demo control ([CoversField] search_demo.*), filters on the demo (category chip, Material field chip), zoom-on-select on the clump, a real cluster tap
        - SearchSettingsFieldTests.cs  -- 2026-09-26. ONE real-scene test per select_filter_search field on the demo (through ApplySearchSettings, the real effect asserted, a Search_Field_*.png render), CoversFieldAttribute, and CoverageGuard_EveryFieldHasARealTest (reflection: a field no test names fails)
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
- proj_guides/  -- domain design docs (_0_work_plan.md, _2.1 through _2.6 (+ _2.6.1 vision, _2.6.2 human, _2.6.4 archive), _2.2.4 effects, _5.1)
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
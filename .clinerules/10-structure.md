# 1. Project Structure & Organizing Principles

**Read this file to get oriented on the project's structure**


## 1. Organizing principles (apply these when adding anything new)

### 1.1 Two top-level areas: Framework and Apps

- `Assets/Framework/` — everything that behaves identically no matter which
  heritage wall is loaded: AR session bootstrap, tracking abstractions, the
  POI data model and rendering, UI shells, content-card rendering,
  analytics, the guide character system.
- `Assets/Apps/<WallName>/` — one self-contained folder per wall. Each
  contains only that wall's data: its POI list, category taxonomy,
  map/localization files, and media (images, audio, 3D models, video).
- A system moves from an app-specific folder into Framework only once a
  **second** wall needs the exact same thing, unchanged. If only one wall
  currently needs it, it stays local to that wall's folder, even if it
  looks reusable. Do not generalize ahead of actual need.
- Nothing in `Framework/` may ever reference anything inside a specific
  `Apps/<WallName>/` folder. If Framework code needs wall-specific
  information, that information is passed in through a data contract (a
  ScriptableObject base class or interface) the wall folder implements —
  never a direct reference the other way.
- No wall folder references another wall folder. If two walls need the same
  thing, that thing belongs in Framework, not copy-pasted between wall
  folders.

### 1.2 Editor code is physically separated from Runtime code

- `Assets/Framework/Runtime/` — code that ships in the built app.
- `Assets/Framework/Editor/` — code that only runs inside the Unity Editor
  (custom inspectors, menu-item tools, validation scripts, wall-setup
  wizards). Must never end up in a device build.
- Enforced via **Assembly Definition files (.asmdef)**, not just folder
  naming — an assembly reference rule is enforced by the compiler and fails
  loudly if violated; folder naming alone can be silently broken by mistake.
  **Verified 2026-08-06**: `TileStories.asmdef` (Runtime) and
  `TileStories.Editor.asmdef` (`includePlatforms: ["Editor"]`) exist exactly
  this way, plus separate `TileStories.Tests.asmdef` /
  `TileStories.Editor.Tests.asmdef` for tests. Correctly followed, not just
  aspirational.
- The Runtime assembly must never reference the Editor assembly. If a
  runtime script needs editor-only functionality, that's a sign the code
  belongs in the Editor assembly instead, wired through a build step or
  menu tool, not a runtime call.
- Wrap any code that must exist in a runtime file but only makes sense in
  the editor in `#if UNITY_EDITOR` — but prefer physically moving it to the
  Editor assembly whenever possible, since that's caught at compile time
  rather than relying on a preprocessor directive someone might forget.

### 1.3 Domain-centered folders, not type-centered folders

- Group files by what they do (`Tracking/`, `POI/`, `Content/`,
  `Analytics/`), never by generic technical category (`Scripts/`,
  `Prefabs/`, `Managers/`). A reader should understand what the project
  does from the folder tree alone, without reading a single file.
- Never create a `Utils.cs`, `Helpers.cs`, or `Common.cs` file that
  accumulates unrelated static methods over time. If a file starts doing
  more than one clearly nameable job, split it into separate files named
  after each job.
- Test folders live alongside the code they test, inside
  `Framework/Tests/`. **Correction 2026-08-06**: the actual subfolder names
  are `Framework/Tests/Editor/` and `Framework/Tests/Runtime/` (matching
  which assembly/test-runner mode each belongs to), not `EditMode/`/
  `PlayMode/` as an earlier version of this rule said — verified directly,
  the actual folders don't match what was previously written here.

---
**How to read this structure:**

The most important boundary in the entire project is the line between `Framework/` and `Apps/`. Framework code **never** imports, references, or knows about any specific wall — it only knows about `WallConfigAsset`, the abstract data type any wall produces. Each wall in `Apps/` loads one `WallConfigAsset` and calls `WallSession.StartSession(asset)`. This is what makes the generalisability claim concrete rather than rhetorical — **if adding a fourth wall requires touching any file in `Framework/`, something is architecturally wrong**.

The second important boundary is between `Framework/Runtime/` and `Framework/Editor/`. Runtime code ships to end users' phones. Editor code is development tooling that never reaches a device. Unity's own assembly-definition system enforces this: the `.Editor.asmdef` references the `Runtime.asmdef` (allowed); the `Runtime.asmdef` never references the Editor assembly (enforced by the asmdef configuration — a compile error if violated). Never blur this boundary by writing `#if UNITY_EDITOR` blocks inside Runtime files — put the code in Editor/ instead.

The third boundary is the `AI/` folder, gated entirely by `FeatureFlags.AIGuideEnabled`. Every other subsystem ignores its existence when the flag is false. This is what makes "any heritage wall or deployment without AI budget simply leaves the flag off" a true statement rather than an aspiration.

**Stage-by-stage construction sequence in this structure:**
- **Stage 0**: no files created; mock deliverables live in Docs/ as PNGs/sketches
- **Stage 1**: Core/, Tracking/, POI/ (partial), UI/Scanning/, UI/Cards/ (scaffold only), UI/Onboarding/, Telemetry/ (wired but empty events), Apps/[first wall]/
- **Stage 2**: Blocks/ (all 6 types, in 2A→2B→2C order), UI/Markers/, UI/Gamification/LapseTimelineView.cs, LODController.cs, LapseStateManager.cs
- **Stage 3**: Circuits/ (all files), UI/Circuits/ (all files), UI/Gamification/ (remaining), UI/GuideCharacter/, UI/Navigation/, UI/Sharing/, AI/ (optional)
- **Stage 4**: Editor/Validation/, Editor/Baker/, Tests/ (all files)
- **Stage 5**: Editor/Wizard/
- **Stage 6**: Final UPM package extraction; a `TileStoriesFramework/` package folder appears alongside the Unity project, mirroring Framework/ with the standard package.json/README/CHANGELOG/LICENSE root files

---


## 2. Standing facts about the project — read before starting any task

- `Assets/Apps/Chafariz/`, `Mural/`, `Panorama/` each have only the
  `MediaAssets/{Audio,Images,Models3D,Videos}/` subfolder scaffold — no
  `config.json`, no scene, no actual content. `LivingRoom` is the only wall
  with real, working content. This is intentional: these are still-future walls.
  Do not mistake scaffold presence for "in progress."
- `Assets/Framework/Editor/AssemblyInfo.cs` exists at the root of `Editor/` —
  grants `InternalsVisibleTo("TileStories.Editor.Tests")` so edit-mode tests
  can access `internal` members. Not a feature file.
- `Assets/Dev/` holds the MarkerGallery dev harness scene and assets.
  `Assets/Screenshots/` holds development PNG captures. Both are intentional
  dev infrastructure, not garbage.
- `Assets/StreamingAssets/LivingRoom/config.json` is the runtime-readable copy
  of `Apps/LivingRoom/config.json`, synced via POI Authoring Tool → "Copy to
  StreamingAssets". Both copies must always be kept in sync.
- Everything under `Assets/Settings/`, `Assets/XR/`, `Assets/TextMesh Pro/`,
  `Assets/Plugins/` is standard Unity/package-installed boilerplate — not
  itemized in the tree below, which only covers authored project content.

---

## A. Complete Project File Structure

This section is the single authoritative reference for every folder and file in the
project. Read this before reading §0–§13.
Every file listed here is described in one sentence stating its responsibility — what
it does, not what it contains.

The Unity project is a monorepo. Unity's own generated folders (`Library/`, `Temp/`,
`Logs/`, `obj/`) are excluded from version control via `.gitignore` and are not listed
here. Only authored content is listed.


**How to read this structure:**

The most important boundary in the entire project is the line between `Framework/` and `Apps/`. Framework code **never** imports, references, or knows about any specific wall — it only knows about `WallConfigAsset`, the abstract data type any wall produces. Each wall in `Apps/` loads one `WallConfigAsset` and calls `WallSession.StartSession(asset)`. This is what makes the generalisability claim concrete rather than rhetorical — **if adding a fourth wall requires touching any file in `Framework/`, something is architecturally wrong**.

The second important boundary is between `Framework/Runtime/` and `Framework/Editor/`. Runtime code ships to end users' phones. Editor code is development tooling that never reaches a device. Unity's own assembly-definition system enforces this: the `.Editor.asmdef` references the `Runtime.asmdef` (allowed); the `Runtime.asmdef` never references the Editor assembly (enforced by the asmdef configuration — a compile error if violated). Never blur this boundary by writing `#if UNITY_EDITOR` blocks inside Runtime files — put the code in Editor/ instead.

The third boundary is the `AI/` folder, gated entirely by `FeatureFlags.AIGuideEnabled`. Every other subsystem ignores its existence when the flag is false. This is what makes "any heritage wall or deployment without AI budget simply leaves the flag off" a true statement rather than an aspiration.

**Stage-by-stage construction sequence in this structure:**
- **Stage 0**: no files created; mock deliverables live in Docs/ as PNGs/sketches
- **Stage 1**: Core/, Tracking/, POI/ (partial), UI/Scanning/, UI/Cards/ (scaffold only), UI/Onboarding/, Telemetry/ (wired but empty events), Apps/[first wall]/
- **Stage 2**: Blocks/ (all 6 types, in 2A→2B→2C order), UI/Markers/, UI/Gamification/LapseTimelineView.cs, LODController.cs, LapseStateManager.cs
- **Stage 3**: Circuits/ (all files), UI/Circuits/ (all files), UI/Gamification/ (remaining), UI/GuideCharacter/, UI/Navigation/, UI/Sharing/, AI/ (optional)
- **Stage 4**: Editor/Validation/, Editor/Baker/, Tests/ (all files)
- **Stage 5**: Editor/Wizard/
- **Stage 6**: Final UPM package extraction; a `TileStoriesFramework/` package folder appears alongside the Unity project, mirroring Framework/ with the standard package.json/README/CHANGELOG/LICENSE root files

---


```
TileStories/                          ← Unity project root (open this in Unity Hub)
│
├── Assets/                            ← Everything below is inside the actual Unity project
│   │
│   ├── Framework/                    ← The reusable engine. NEVER wall-specific code here.
│   │   │
│   │   ├── Runtime/                  ← Ships in the UPM package. No Editor references.
│   │   │   ├── TileStories.asmdef    ← Assembly def: Runtime assembly, no editor dependency.
│   │   │   │
│   │   │   ├── Core/                  ← Config loading, wall session lifecycle, project-wide settings/feature flags
│   │   │   │   ├── WallSession.cs              ← Owns the active wall's lifecycle: load config,
│   │   │   │   │                                  fire OnWallReady, coordinate all subsystems.
│   │   │   │   │                                  **Wiring only, no business logic**: this class
│   │   │   │   │                                  calls other objects that decide things (the
│   │   │   │   │                                  resolver, the registry, the state machines) — it
│   │   │   │   │                                  must never itself contain an `if`/`switch` that
│   │   │   │   │                                  decides *what* should happen, only sequencing of
│   │   │   │   │                                  *when* the thing that decides gets called. This
│   │   │   │   │                                  is the one class in the whole project most prone
│   │   │   │   │                                  to quietly absorbing logic over time — watch it.
│   │   │   │   ├── WallConfigLoader.cs         ← Loads config.json from StreamingAssets at runtime;
│   │   │   │   │                                  deserializes into WallConfigData for use by
│   │   │   │   │                                  WallSession and other systems.
│   │   │   │   ├── WallConfigData.cs           ← Plain C# classes mirroring config.json schema
│   │   │   │   │                                  (POIData with captured_position, CalibrationAnchor,
│   │   │   │   │                                  CategoryStyle, OutlineLevel, BadgeCategoryData,
│   │   │   │   │                                  etc.); also holds the full config at runtime.
│   │   │   │   ├── MarkerStyle.cs              ← Enum: OutlineGold / OutlineSameHue / Badge —
│   │   │   │   │                                  the three ways a marker encodes its status axis.
│   │   │   │   ├── MarkerShape.cs              ← Enum: Circle / Diamond / Hexagon / RoundedSquare /
│   │   │   │   │                                  Star — the base silhouette for Symbol and Badge.
│   │   │   │   ├── MarkerOutlineMode.cs        ← Enum: None / Contour / ContourRotating —
│   │   │   │   │                                  controls whether and how the status contour renders.
│   │   │   │   ├── MarkerVisualsParser.cs      ← Parses the wall config's free-form marker_style /
│   │   │   │   │                                  marker_shape / marker_outline_mode strings into
│   │   │   │   │                                  their enums; logs once and falls back to sane
│   │   │   │   │                                  defaults on unrecognised values.
│   │   │   │   ├── WallConfigAsset.cs          ← [NOT YET BUILT] ScriptableObject baked from
│   │   │   │   │                                  config.json; planned as the only data type runtime
│   │   │   │   │                                  code reads. Currently WallConfigData is used directly.
│   │   │   │   ├── TileStoriesSettings.cs      ← [NOT YET BUILT] ScriptableObject holding
│   │   │   │   │                                  per-project settings (LOD thresholds, feature flags).
│   │   │   │   └── FeatureFlags.cs             ← [NOT YET BUILT] Static class exposing bool flags
│   │   │   │   │                                  (AI guide enabled, social sharing enabled) read by
│   │   │   │   │                                  every subsystem to skip entire code paths at runtime.
│   │   │   │
│   │   │   ├── Tracking/              ← AR tracking abstraction (IWallTracker) + Immersal/mock implementations
│   │   │   │   ├── IWallTracker.cs             ← Interface: ExposesPose, IsLocalised, events
│   │   │   │   │                                  OnWallLocalised(Pose) and OnTrackingLost().
│   │   │   │   ├── ImmersalWallTracker.cs      ← Implements IWallTracker using Immersal SDK;
│   │   │   │   │                                  wraps ImmersalSDK localisation callbacks,
│   │   │   │   │                                  emits OnWallLocalised when confidence threshold
│   │   │   │   │                                  is met, measures time-to-first-lock metric.
│   │   │   │   ├── MockLocalizationProvider.cs ← Detects #if UNITY_EDITOR (or a debug flag);
│   │   │   │   │                                  immediately fires OnWallLocalised(Pose.identity)
│   │   │   │   │                                  with configurable offset; instantiates POIs
│   │   │   │   │                                  relative to a flat in-scene reference plane;
│   │   │   │   │                                  adds WASD + editor camera controls (right mouse,
│   │   │   │   │                                  Alt + left mouse, or arrow keys for look) to the
│   │   │   │   │                                  Main Camera for editor walk-through. Build first
│   │   │   │   │                                  in Stage 1, before any other feature -- this is
│   │   │   │   │                                  Tier 1 of the three-tier testing pipeline.
>>>>>>>

│   │   │   │   ├── EditorCameraLook.cs          ← Static helper: pure yaw/pitch rotation math
│   │   │   │   │                                  extracted from MockLocalizationProvider so the
│   │   │   │   │                                  editor look-delta can be unit-tested without a
│   │   │   │   │                                  scene or camera.
│   │   │   │   ├── TrackingJitterSmoother.cs   ← [NOT YET BUILT] Planned: smooths Immersal pose
│   │   │   │   │                                  updates via SmoothDamp + Slerp to prevent visual
│   │   │   │   │                                  snap on re-localisation.
│   │   │   │   └── TrackingMetrics.cs          ← [NOT YET BUILT] Planned: records per-session
│   │   │   │                                      time-to-first-lock, zone-switch, drift measurements.
│   │   │   │
│   │   │   ├── POI/                   ← POI position resolution, category/badge palettes, status ramp, overlap resolution
│   │   │   │   ├── POIPositionResolver.cs      ← Converts (x_norm, y_norm) → 3D position under
│   │   │   │   │                                  the XR Space using piecewise linear interpolation
│   │   │   │   │                                  between calibration anchors; bypassed if the POI
│   │   │   │   │                                  has a captured_position value.
│   │   │   │   ├── MarkerOverlapResolver.cs    ← Static; called once after spawning. Snapshots
│   │   │   │   │                                  every marker's screen position, unions markers
│   │   │   │   │                                  within ~40px into groups (union-find, spawn-order
│   │   │   │   │                                  independent), assigns each group a stable vertical
│   │   │   │   │                                  offset. Idempotent: calling twice produces the same
│   │   │   │   │                                  result.
│   │   │   │   ├── CategoryPalette.cs          ← Resolves a wall-defined `category` string into a
│   │   │   │   │                                  fill colour and optional icon key. Optional
│   │   │   │   │                                  Configure() call (made by WallSession) maps specific
│   │   │   │   │                                  categories to colours/icons; unknown categories fall
│   │   │   │   │                                  through to a deterministic hash + KnownIcons lookup.
│   │   │   │   ├── BadgeCategoryPalette.cs     ← Resolves the optional `badge_category` key into
│   │   │   │   │                                  a BadgeDefinition (color + icon key). Used by
│   │   │   │   │                                  MarkerView when the wall uses Badge-style markers.
│   │   │   │   ├── StatusRamp.cs               ← Maps `status_pct` float → StatusLevel (ring color,
│   │   │   │   │                                  sprite key, ring width). Defines the four built-in
│   │   │   │   │                                  destruction levels (Intact/PartialDamage/Destroyed/
│   │   │   │   │                                  Unknown). Consumed by MarkerRingView.
│   │   │   │   ├── MarkerHierarchyResolver.cs  ← Resolves a wall's hierarchy_levels table into
│   │   │   │   │                                  per-POI HierarchyStyle (size, label, effects,
│   │   │   │   │                                  reveal delay/duration). Static Configure/
│   │   │   │   │                                  TryResolveByKey/Fallback, same pattern as StatusRamp.
│   │   │   │   ├── POIPool.cs                  ← [NOT YET BUILT] Object pool of recycled POIAnchor
│   │   │   │   │                                  instances; acquire/return on show/hide.
│   │   │   │   ├── LODController.cs            ← [NOT YET BUILT] Scene-wide distance-based LOD:
│   │   │   │   │                                  SetActive(false) on entire POIAnchor GameObjects
│   │   │   │   │                                  beyond 7m; top-5/15/all tiers inside that range.
│   │   │   │   ├── LapseStateManager.cs        ← [NOT YET BUILT] POI-level state machine for
│   │   │   │   │                                  temporal-lapse walls; gates visibility per epoch.
│   │   │   │   └── NearestPOIFinder.cs         ← [NOT YET BUILT] Given a list of POI IDs, returns
│   │   │   │                                      the one whose resolved position is nearest to a
│   │   │   │                                      Vector3; used by circuit entry-point resolution.
│   │   │   │
│   │   │   ├── DevTools/              ← Dev-only harness; included in Runtime assembly so PlayMode
│   │   │   │   │                        tests can instantiate it, but gated by #if UNITY_EDITOR
│   │   │   │   │                        for any file-write or AssetDatabase calls. Not shipped.
│   │   │   │   ├── MarkerGalleryDefinitions.cs ← Defines `MarkerGalleryEntry` (one marker variant
│   │   │   │   │                                  to render and assert on) and the full list of
│   │   │   │   │                                  gallery entries built up phase by phase.
│   │   │   │   └── MarkerGalleryHarness.cs     ← MonoBehaviour that instantiates and configures
│   │   │   │                                      every MarkerGalleryEntry in the gallery scene;
│   │   │   │                                      used by both the Dev/MarkerGallery scene and
│   │   │   │                                      MarkerGalleryTests for automated visual assertions.
│   │   │   │
│   │   │   ├── Blocks/                ← NOT YET BUILT. Planned: detail-card content block system (text/image/audio/video/3D/map)
│   │   │   │   ├── TileStoriesUIBlock.cs       ← Abstract base class for all six registered block
│   │   │   │   │                                  types; exposes Populate(POIData, profile) and
│   │   │   │   │                                  abstract void Render().
│   │   │   │   ├── TileStoriesBlockRegistry.cs ← Static registry: maps "type" string → factory
│   │   │   │   │                                  func; exposes public Register(typeKey, factory)
│   │   │   │   │                                  so downstream projects add block types without
│   │   │   │   │                                  forking; auto-registers the six built-in types
│   │   │   │   │                                  via [RuntimeInitializeOnLoadMethod].
│   │   │   │   ├── TextBlock.cs                ← Renders name + summary or content_by_profile
│   │   │   │   │                                  text into TextMeshPro labels in the detail card.
│   │   │   │   ├── ImageBlock.cs               ← Lazy-loads image from MediaAssets/Images/ via
│   │   │   │   │                                  UnityWebRequest, displays in a RawImage; unloads
│   │   │   │   │                                  texture when card closes.
│   │   │   │   ├── AudioBlock.cs               ← Downloads/plays TTS clip from MediaAssets/Audio/;
│   │   │   │   │                                  owns playback queue, pause/resume, progress bar;
│   │   │   │   │                                  handles OnApplicationPause and
│   │   │   │   │                                  AudioSettings.OnAudioConfigurationChanged.
│   │   │   │   ├── VideoBlock.cs               ← Streams video from MediaAssets/Videos/ via
│   │   │   │   │                                  VideoPlayer; lazy-loads, releases VideoClip on
│   │   │   │   │                                  close; never pre-loads at scene start.
│   │   │   │   ├── ModelBlock.cs               ← Loads .glb from MediaAssets/Models3D/ via
│   │   │   │   │                                  glTFast at card-open time; renders in a dedicated
│   │   │   │   │                                  RT camera inset; unloads on close.
│   │   │   │   ├── MapBlock.cs                 ← "See where it is today": (a) opens native Maps
│   │   │   │   │                                  app via geo:/maps URL from coordinates_today, and
│   │   │   │   │                                  (b) optionally fetches a Maps Static API thumbnail
│   │   │   │   │                                  PNG via UnityWebRequest and shows in a RawImage.
│   │   │   │   └── ProfileResolver.cs          ← Not a block — resolves content_by_profile[key]
│   │   │   │                                      for the active visitor profile; called by every
│   │   │   │                                      block's Populate() before rendering.
│   │   │   │
│   │   │   ├── Circuits/              ← NOT YET BUILT. Planned: guided multi-POI tour/route state machine
│   │   │   │   ├── CircuitStateMachine.cs      ← Central state machine: owns currentIndex,
│   │   │   │   │                                  visited-set, skipped-set, and a stack of paused
│   │   │   │   │                                  CircuitState snapshots; exposes StartCircuit,
│   │   │   │   │                                  MarkVisited, Pause, Resume, Abandon.
│   │   │   │   ├── CircuitState.cs             ← Plain data class: circuitId, currentIndex, visited
│   │   │   │   │                                  HashSet, skipped HashSet — one instance per paused
│   │   │   │   │                                  or active circuit; pushed/popped by state machine.
│   │   │   │   ├── EntryPointResolver.cs       ← Finds nearest poi_sequence member to visitor's
│   │   │   │   │                                  current position at circuit start; uses
│   │   │   │   │                                  NearestPOIFinder; returns starting index.
│   │   │   │   ├── AmbientPivotMonitor.cs      ← Polls visitor position every 2s during an active
│   │   │   │   │                                  sequential circuit; starts a 10s engagement timer
│   │   │   │   │                                  when visitor dwells near an out-of-sequence POI;
│   │   │   │   │                                  fires OnDeviationConfirmed after the threshold.
│   │   │   │   ├── ButterflyPromptController.cs← Listens for OnDeviationConfirmed; checks if the
│   │   │   │   │                                  engaged POI belongs to a different circuit;
│   │   │   │   │                                  shows dismissible toast via GuideCharacterView;
│   │   │   │   │                                  on accept, pauses active circuit and starts new;
│   │   │   │   │                                  enforces a 60s suppress-cooldown after any
│   │   │   │   │                                  dismissal (prevents cognitive fatigue from
│   │   │   │   │                                  repeated prompts along dense wall sections).
│   │   │   │   └── CircuitLookupTable.cs       ← Precomputed at bake time (Stage 4): maps every
│   │   │   │                                      POI ID → list of circuit IDs that contain it,
│   │   │   │                                      used by ButterflyPromptController for cross-
│   │   │   │                                      circuit detection without runtime JSON scanning.
│   │   │   │
│   │   │   ├── UI/                             ← Two distinct rendering systems co-exist here —
│   │   │   │   │                                  see §2 for the full decision rationale. Read this
│   │   │   │   │                                  comment before writing any UI code:
│   │   │   │   │                                  • Markers/ and NextStopArrow.cs → uGUI (World Space
│   │   │   │   │                                    Canvas, GameObject-based, physically placed in AR)
│   │   │   │   │                                  • Everything else in UI/ → UI Toolkit (UIDocument,
│   │   │   │   │                                    UXML + USS, screen overlay, PanelSettings asset).
│   │   │   │   │                                  The two systems do not conflict in Unity 6.3 LTS.
│   │   │   │   │
│   │   │   │   ├── Shared/                     ← Design foundation — create Stage 0, before any UXML
│   │   │   │   │   ├── DesignTokens.uss        ← :root CSS variables for every visual constant:
│   │   │   │   │   │                              --ts-color-primary, --ts-radius-card, --ts-spacing-md,
│   │   │   │   │   │                              --ts-font-body, --ts-motion-spring — one change here
│   │   │   │   │   │                              rethemes the entire app. Import first in every .uss.
│   │   │   │   │   ├── Typography.uss          ← .ts-h1/.ts-body/.ts-caption class definitions;
│   │   │   │   │   │                              references --ts-font-* tokens.
│   │   │   │   │   ├── PanelSettings.asset     ← UI Toolkit Panel Settings: Scale With Screen Size,
│   │   │   │   │   │                              reference resolution 390×844 (iPhone 14 Pro base),
│   │   │   │   │   │                              safe-area insets applied here once for all panels.
│   │   │   │   │   └── SafeAreaHelper.cs       ← Reads Screen.safeArea (with the top-left origin
│   │   │   │   │                                  inversion needed for UI Toolkit per Unity docs);
│   │   │   │   │                                  applies as padding to the root VisualElement of
│   │   │   │   │                                  every UIDocument panel. Called once at panel init.
│   │   │   │   │
│   │   │   │   ├── Scanning/                   ← UI Toolkit (screen overlay, UXML/USS)
│   │   │   │   │   ├── ScanningStateView.uxml  ← UXML structure: animated scan-line overlay shown
│   │   │   │   │   │                              while ImmersalWallTracker searches.
│   │   │   │   │   ├── ScanningStateView.uss   ← USS: scan-line animation via CSS transition.
│   │   │   │   │   ├── ScanningStateView.cs    ← Controller: subscribes to IWallTracker events,
│   │   │   │   │   │                              shows/hides the UIDocument panel.
│   │   │   │   │   └── LockSuccessAnimation.cs ← Controller: plays the gold-ring-pulse and
│   │   │   │   │                                  marker-appear transition on first wall lock;
│   │   │   │   │                                  uses USS transition for the ring pulse,
│   │   │   │   │                                  DOTween for the marker-appear if more control needed.
│   │   │   │   │
│   │   │   │   ├── Markers/                    ← uGUI ONLY — World Space Canvas, 3D-positioned.
│   │   │   │   │   │                              **Update 2026-08: this subtree grew substantially
│   │   │   │   │   │                              during Stage 2.3 beyond what was originally sketched
│   │   │   │   │   │                              here — full current file-by-file detail lives in
│   │   │   │   │   │                              `_2_2_Marker_Design.md` §2 (current architecture
│   │   │   │   │   │                              reference); this entry is now just an accurate
│   │   │   │   │   │                              top-level list, not the authoritative detail.**
│   │   │   │   │   ├── MarkerView.cs           ← Root orchestrator, not just a label renderer as first
│   │   │   │   │   │                              sketched — resolves category/hero-icon/status, wires
│   │   │   │   │   │                              every sub-component below.
│   │   │   │   │   ├── MarkerRevealEffect.cs       ← Handles the initial reveal animation: waits
│   │   │   │   │   │                                  revealDelaySeconds, fades alpha 0->1 + scales
│   │   │   │   │   │                                  0->1 over durationSeconds. Edit-Mode guard
│   │   │   │   │   │                                  sets full alpha/scale immediately (coroutines
│   │   │   │   │   │                                  don't tick in Edit Mode).
│   │   │   │   │   ├── MarkerCircleGlyphView.cs← One reusable "coloured shape + centred icon" element,
│   │   │   │   │   │                              used for both Symbol and Badge.
│   │   │   │   │   ├── MarkerRingView.cs       ← Status ring/contour (colour + dash pattern + optional
│   │   │   │   │   │                              rotation).
│   │   │   │   │   ├── MarkerLayout.cs         ← Pure-logic layout math, no Unity lifecycle.
│   │   │   │   │   ├── MarkerBillboard.cs      ← Faces the marker toward the camera.
│   │   │   │   │   ├── MarkerEffect.cs         ← Base class for the effect components below.
│   │   │   │   │   ├── MarkerPulseEffect.cs, MarkerSunEffect.cs, MarkerAccentEffect.cs  ← hero/accent effects, see below
│   │   │   │   │   │                            ← Hero/accent visual effects (breathing, concentric
│   │   │   │   │   │                              sun rings, single-layer ring-pulse/simple-sun/beacon)
│   │   │   │   │   │                              — driven by `MarkerEffectFlags`, independent of
│   │   │   │   │   │                              hero status. `MarkerGlowEffect.cs`/
│   │   │   │   │   │                              `MarkerParticleEffect.cs` also present, both
│   │   │   │   │   │                              deliberately unused/off in shipped runtime (kept as
│   │   │   │   │   │                              documented extension points, not dead code).
│   │   │   │   │   ├── MarkerCircleSpriteFactory.cs ← Shared runtime circle/ring sprite generation,
│   │   │   │   │   │                              used by the sun/accent effects above.
│   │   │   │   │   ├── MarkerEffectFlags.cs    ← `[Flags]` enum for the effects above.
│   │   │   │   │   ├── SpriteKeyLibrary.cs     ← Generic key→Sprite lookup, reused for icons, shapes,
│   │   │   │   │   │                              and ring line styles (3 separate `.asset` purposes,
│   │   │   │   │   │                              one class).
│   │   │   │   │   ├── POIAnchor.cs            ← Holds one POI's resolved data + position.
│   │   │   │   │   ├── POI_Marker.prefab       ← The one shared prefab every spawn path uses (gallery
│   │   │   │   │   │                              and real `WallSession` alike).
│   │   │   │   │   ├── IconLibrary.asset, ShapeLibrary.asset ← Framework-default `SpriteKeyLibrary`
│   │   │   │   │   │                              instances, referenced directly by the prefab.
│   │   │   │   │   ├── Icons/, Rings/, Shapes/ ← Default PNGs (must stay under Runtime/, not Editor/ —
│   │   │   │   │   │                              see `_5.1_Editor_Tab.md` §7 for why).
│   │   │   │   │   └── ClusterIndicator.cs     ← Not yet built — "N more" badge when LODController
│   │   │   │   │                                  (also not yet built) collapses markers past a
│   │   │   │   │                                  threshold. Genuinely future work, unlike the rest of
│   │   │   │   │                                  this list.
│   │   │   │   │
│   │   │   │   ├── Cards/                      ← UI Toolkit (screen overlay, UXML/USS)
│   │   │   │   │   ├── DetailCard.uxml         ← UXML: bottom-sheet structure (≤40% screen height),
│   │   │   │   │   │                              scroll view root, block-slot container, drag
│   │   │   │   │   │                              handle bar, dismiss button.
│   │   │   │   │   ├── DetailCard.uss          ← USS: card corner radii, shadow, spring-up
│   │   │   │   │   │                              transition, block spacing tokens.
│   │   │   │   │   ├── DetailCardView.cs       ← Controller: populates block-slot container from
│   │   │   │   │   │                              BlockRegistry, handles swipe-down and tap-
│   │   │   │   │   │                              outside dismissal.
│   │   │   │   │   └── SummaryLabelView.cs     ← Controller only (no UXML — simple 2-line tooltip
│   │   │   │   │                                  driven by UI Toolkit Label above the marker).
│   │   │   │   │   └── CardBlockContainer.cs   ← UI Toolkit ScrollView + flex-direction: column
│   │   │   │   │                                  scaffold hosting block renderers inside the
│   │   │   │   │                                  detail card; shared by all block types.
│   │   │   │   │                                  (Note: this replaces the earlier uGUI Vertical
│   │   │   │   │                                  Layout Group + Content Size Fitter + Scroll Rect
│   │   │   │   │                                  chain — the UI Toolkit ScrollView is simpler,
│   │   │   │   │                                  automatically virtualized for long lists, and
│   │   │   │   │                                  native momentum scrolling on iOS/Android.)
│   │   │   │   │
│   │   │   │   ├── Circuits/                   ← UI Toolkit (screen overlay); except NextStopArrow
│   │   │   │   │   ├── CircuitSelectionView.cs ← Shows circuit cards (title, time, POI count,
│   │   │   │   │   │                              profile-fit badge, Resume label if paused); routes
│   │   │   │   │   │                              taps to CircuitStateMachine.
│   │   │   │   │   ├── CircuitProgressRail.cs  ← "3 of 8 stops" progress indicator; renders filled
│   │   │   │   │   │                              dots (visited), hollow (skipped), empty (unvisited);
│   │   │   │   │   │                              updates on CircuitStateMachine state changes.
│   │   │   │   │   ├── CircuitCompletionView.cs← Consolidated end screen: summary, quiz questions
│   │   │   │   │   │                              if any, share button; replaces three scattered
│   │   │   │   │   │                              triggers that would otherwise be separate.
│   │   │   │   │   └── NextStopArrow.cs        ← **uGUI ONLY** — World Space Canvas element placed
│   │   │   │   │                                  in 3D between visitor and target POI; updates on
│   │   │   │   │                                  state machine change; shows distance + directional
│   │   │   │   │                                  label (e.g. "→ 4m"); only visible during an
│   │   │   │   │                                  active circuit, hidden otherwise.
│   │   │   │   │
│   │   │   │   ├── Onboarding/                 ← UI Toolkit (full-screen panels)
│   │   │   │   │   ├── OnboardingFlow.cs       ← Drives the multi-step onboarding sequence:
│   │   │   │   │   │                              analytics consent → profile selection →
│   │   │   │   │   │                              scanning-state hand-off; completes in <30s.
│   │   │   │   │   ├── ProfileSelectionView.cs ← Single-screen profile picker (tourist / student /
│   │   │   │   │   │                              academic / child) with brief description per
│   │   │   │   │   │                              option; writes selection to PlayerPrefs.
│   │   │   │   │   ├── ConsentNoticeView.cs    ← One-line analytics notice + toggle; calls
│   │   │   │   │   │                              FirebaseAnalytics.SetConsent() on change — never
│   │   │   │   │   │                              cosmetic, always functionally wired from Stage 1.
│   │   │   │   │   └── KnowledgeCheckView.cs   ← Pre/post factual-recall quiz (~5 questions);
│   │   │   │   │                                  shown at onboarding and again at exit survey;
│   │   │   │   │                                  result stored locally (Type A telemetry) for
│   │   │   │   │                                  learning-gain delta computation.
│   │   │   │   │
│   │   │   │   ├── Gamification/               ← UI Toolkit (screen overlays + toasts)
│   │   │   │   │   ├── BadgeSystem.cs          ← Evaluates badge trigger conditions on every
│   │   │   │   │   │                              POI visit and circuit completion; dispatches by
│   │   │   │   │   │                              trigger.type string — open set, add new types
│   │   │   │   │   │                              here without touching individual badge data.
│   │   │   │   │   ├── BadgeUnlockView.cs      ← Toast shown on badge earn: icon + title + brief
│   │   │   │   │   │                              description; dismisses automatically after 4s
│   │   │   │   │   │                              or on tap; triggers share sheet option.
│   │   │   │   │   ├── QuizBlockView.cs        ← Renders multiple-choice question from quiz[]
│   │   │   │   │   │                              array; shows explanation after every answer
│   │   │   │   │   │                              (required field, not optional); logs result
│   │   │   │   │   │                              to Type A telemetry.
│   │   │   │   │   ├── DidYouKnowView.cs       ← Surfaces a random fun_fact from wall-level
│   │   │   │   │   │                              fun_facts[] array on a timer or tap-count
│   │   │   │   │   │                              trigger; dismissed by tap.
│   │   │   │   │   ├── DiscoveryCounterView.cs ← "15/150 buildings discovered" progress display;
│   │   │   │   │   │                              reads visited-set count vs. total POI count.
│   │   │   │   │   └── LapseTimelineView.cs    ← Horizontal slider for epoch selection (e.g.
│   │   │   │   │                                  "Lisboa ~1700 / Terramoto 1755 / Reconstrução /
│   │   │   │   │                                  Hoje"); drives LapseStateManager; shows labelled
│   │   │   │   │                                  epoch names below each slider position.
│   │   │   │   │
│   │   │   │   ├── GuideCharacter/             ← UI Toolkit (persistent screen overlay)
│   │   │   │   │   ├── GuideCharacterView.cs   ← Persistent AR-chrome character overlay;
│   │   │   │   │   │                              switches between named sprite frames (idle /
│   │   │   │   │   │                              talking / pointing-left / pointing-right /
│   │   │   │   │   │                              surprised); listens to AudioBlock playback state.
│   │   │   │   │   └── ButterflyToastView.cs   ← Dismissible toast "You've found X — switch
│   │   │   │   │                                  circuit?" driven by ButterflyPromptController;
│   │   │   │   │                                  auto-dismisses after 8s if not tapped.
│   │   │   │   │
│   │   │   │   ├── Navigation/                 ← UI Toolkit (FAB, search overlay, settings)
│   │   │   │   │   ├── FabMenuView.cs          ← Floating action button that expands into:
│   │   │   │   │   │                              Circuits, Audio Guide, Achievements, AI Guide
│   │   │   │   │   │                              (if enabled via FeatureFlags).
│   │   │   │   │   ├── SearchOverlayView.cs    ← Full-screen search + filter panel; filters by
│   │   │   │   │   │                              category, status (if wall has one), and free-
│   │   │   │   │   │                              text name search; tapping a result pans view
│   │   │   │   │   │                              to that POI's marker.
│   │   │   │   │   └── SettingsView.cs         ← Language, profile, audio on/off, text size,
│   │   │   │   │                                  reset-progress toggle; writes to PlayerPrefs.
│   │   │   │   │
│   │   │   │   └── Sharing/                    ← UI Toolkit (share sheet trigger + capture)
│   │   │   │       ├── ARScreenCapture.cs      ← Captures watermarked/framed screenshot of
│   │   │   │       │                              the live AR view; saves to device camera roll
│   │   │   │       │                              (handles Android scoped-storage differences).
│   │   │   │       │                              **Critical implementation note**: use
│   │   │   │       │                              `ScreenCapture.CaptureScreenshotAsTexture()`
│   │   │   │       │                              (Unity's composited capture, includes all canvas
│   │   │   │       │                              layers) rather than `Texture2D.ReadPixels()`
│   │   │   │       │                              (reads only the camera/framebuffer, bypasses
│   │   │   │       │                              uGUI canvas entirely — the Immersal logo overlay,
│   │   │   │       │                              the guide character, and all UI chrome would be
│   │   │   │       │                              absent from the saved image). If `ReadPixels` is
│   │   │   │       │                              used for performance reasons, manually blit the
│   │   │   │       │                              Immersal attribution badge texture onto the
│   │   │   │       │                              captured `Texture2D` via `Graphics.CopyTexture`
│   │   │   │       │                              before saving — never distribute a screenshot
│   │   │   │       │                              that strips the required attribution logo.
│   │   │   │       └── ShareSheetController.cs ← Opens native OS share sheet with screenshot,
│   │   │   │                                      wall name, and deep-link; called by circuit
│   │   │   │                                      completion and badge unlock views.
│   │   │   │
│   │   │   ├── Telemetry/             ← NOT YET BUILT. Planned: analytics/usage event logging
│   │   │   │   ├── TelemetryService.cs         ← Central router: receives all event calls and
│   │   │   │   │                                  dispatches to Type A (local) and Type B (remote)
│   │   │   │   │                                  backends; never called directly — always via
│   │   │   │   │                                  TelemetryEvents static class below.
│   │   │   │   ├── TelemetryEvents.cs          ← Static facade with one method per event type:
│   │   │   │   │                                  POITapped(id), CircuitCompleted(id, duration),
│   │   │   │   │                                  AudioPlayed(id, completed), QuizAnswered(poi,
│   │   │   │   │                                  correct), SessionStarted(wallId, profile), etc.
│   │   │   │   │                                  All feature code calls this, never TelemetryService.
│   │   │   │   ├── LocalTelemetryBackend.cs    ← Type A: writes events to PlayerPrefs / local JSON
│   │   │   │   │                                  file; stores score, dwell time, completion %;
│   │   │   │   │                                  readable offline, no network needed.
│   │   │   │   └── RemoteTelemetryBackend.cs   ← Type B: stub implementing ITelemetryBackend;
│   │   │   │                                      wraps Firebase Analytics LogEvent(); real
│   │   │   │                                      Firebase calls added here — swapping to a
│   │   │   │                                      different analytics provider touches this file
│   │   │   │                                      only.
│   │   │   │
│   │   │   └── AI/                             ← Entire folder gated by FeatureFlags.AIGuideEnabled
│   │   │       ├── AIGuideController.cs        ← Manages the "Ask Guide" session: rate-limiting
│   │   │       │                                  (reads from Firebase Remote Config), session
│   │   │       │                                  start/stop, routes to either speech-to-speech
│   │   │       │                                  or text-only backend per config flag.
│   │   │       ├── RealtimeVoiceBackend.cs     ← Speech-to-speech path via OpenAI Realtime API
│   │   │       │                                  (verify model name at implementation time —
│   │   │       │                                  see Stage 7); sends structured POI context
│   │   │       │                                  (not raw camera frames) by default.
│   │   │       ├── TextChatBackend.cs          ← Cheaper fallback: device speech-to-text →
│   │   │       │                                  text-completion API call → response shown as
│   │   │       │                                  on-screen text; ~10× cheaper than voice path.
│   │   │       └── StructuredCameraContext.cs  ← Computes which POIs are in camera FoV from
│   │   │                                          wall_position data + camera frustum; buckets
│   │   │                                          each into an 8-region screen grid; returns
│   │   │                                          the structured list sent to AI instead of a
│   │   │                                          raw JPEG frame.
│   │   │
│   │   ├── Editor/                             ← Editor-only tools. Never referenced by Runtime/.
│   │   │   ├── TileStories.Editor.asmdef       ← Editor assembly def; references TileStories.asmdef.
│   │   │   ├── AssemblyInfo.cs                 ← Grants `InternalsVisibleTo("TileStories.Editor.Tests")`
│   │   │   │                                      so edit-mode tests can access `internal` members.
│   │   │   │                                      Not a feature file — purely an assembly attribute.
│   │   │   │
│   │   │   ├── Validation/            ← NOT YET BUILT. Planned: config.json schema validation tooling
│   │   │   │   ├── WallConfigValidator.cs      ← Reads config.json; validates required fields,
│   │   │   │   │                                  value ranges (x_norm 0–1), circuit POI ID
│   │   │   │   │                                  references, badge trigger type strings; returns
│   │   │   │   │                                  list of ValidationError with line/field context.
│   │   │   │   └── ValidationError.cs          ← Plain data class: field path, message, severity
│   │   │   │                                      (Error / Warning); displayed in Wizard and
│   │   │   │                                      surfaced as Unity Console messages.
│   │   │   │
│   │   │   ├── POIAuthoring/           ← `POIAuthoringToolWindow` split as a `partial class` across
│   │   │   │   │                          this folder (was one 1,750-line file; refactored 2026-08
│   │   │   │   │                          into per-concern partial files, zero behaviour change —
│   │   │   │   │                          see `_5.1_Editor_Tab.md` for the method-to-file map).
│   │   │   │   ├── POIAuthoringToolWindow.cs           ← Shell: fields, ShowWindow, OnEnable/OnDisable,
│   │   │   │   │                                          OnGUI, DrawFramedFoldout, DrawToolbar,
│   │   │   │   │                                          DrawTopConfigAndActions, DrawSyncAndWarnings.
│   │   │   │   │                                          ~235 lines.
│   │   │   │   ├── POIAuthoringToolWindow.Constants.cs ← Option/label arrays, colours, TrashIcon.
│   │   │   │   ├── MarkerSymbolTexturePostprocessor.cs ← AssetPostprocessor: auto-configures texture
│   │   │   │   │                                          import settings (Sprite type, alpha,
│   │   │   │   │                                          nPOTScale) for any PNG dropped under a
│   │   │   │   │                                          wall's `MarkerAssets/` folder.
│   │   │   │   ├── MARKER_ASSETS_CONVENTION.md         ← Documents the naming and folder convention
│   │   │   │   │                                          for wall-specific marker symbol assets.
│   │   │   │   ├── GlobalScene/
│   │   │   │   │   └── POIAuthoringToolWindow.GlobalScene.cs  ← DrawGlobalSceneOptions,
│   │   │   │   │                                                  DrawMarkerGlobalSection,
│   │   │   │   │                                                  DrawGlobalBadgeSection,
│   │   │   │   │                                                  DrawGlobalOutlineSection,
│   │   │   │   │                                                  RecomputeLevelPercentSpacing.
│   │   │   │   ├── SpecificMarker/
│   │   │   │   │   └── POIAuthoringToolWindow.SpecificMarker.cs ← DrawSpecificMarkerOptions,
│   │   │   │   │                                                    DrawPoiPositionFields,
│   │   │   │   │                                                    DrawPoiMarkerStyleFields,
│   │   │   │   │                                                    DrawPoiBadgeStyleFields,
│   │   │   │   │                                                    DrawPoiOutlineFields,
│   │   │   │   │                                                    DrawPoiEffectsFields,
│   │   │   │   │                                                    DrawCategoryDropdown,
│   │   │   │   │                                                    DrawBadgeCategoryDropdown,
│   │   │   │   │                                                    DrawStatusLevelDropdown,
│   │   │   │   │                                                    GetPoiFoldout.
│   │   │   │   ├── Shared/
│   │   │   │   │   ├── POIAuthoringToolWindow.SymbolTable.cs ← Generic symbol-table drawer reused by
│   │   │   │   │   │                                           category/badge/outline tables; plus
│   │   │   │   │   │                                           DrawWallIconLibrarySelector,
│   │   │   │   │   │                                           AssignSpriteToLibraryAndGetKey,
│   │   │   │   │   │                                           ResolveSpriteForKey, DrawSpritePreview,
│   │   │   │   │   │                                           DrawColorSwatchAndHex, TryParseHexColor.
│   │   │   │   │   ├── EntryDetailsPopup.cs                 ← Standalone PopupWindowContent (not
│   │   │   │   │   │                                           partial) for editing a single category/
│   │   │   │   │   │                                           badge/outline row in-place.
│   │   │   │   │   ├── ExistingSymbolPickerPopup.cs         ← Curated sprite picker that shows only
│   │   │   │   │   │                                           the wall's own icon library and the
│   │   │   │   │   │                                           framework default, not every Sprite in
│   │   │   │   │   │                                           the entire project.
│   │   │   │   │   ├── DefaultBadgeCategories.cs            ← Returns the four building-damage badge
│   │   │   │   │   │                                           defaults seeded into a new wall's config.
│   │   │   │   │   │                                           Editor-only; runtime reads config.json.
│   │   │   │   │   ├── DefaultCategoryStyles.cs             ← Returns the six heritage category style
│   │   │   │   │   │                                           defaults seeded into a new wall's config.
│   │   │   │   │   │                                           Editor-only; runtime reads config.json.
│   │   │   │   │   ├── DefaultOutlineLevels.cs              ← Returns the four destruction-status
│   │   │   │   │                                               outline level defaults seeded into a new
│   │   │   │   │                                               wall's config. Editor-only.
│   │   │   │   │   ├── HelpInfoPopup.cs                     ← Read-only popup for fixed, framework-authored
│   │   │   │   │   │                                          help text (info button). Distinct from
│   │   │   │   │   │                                          EntryDetailsPopup (which persists developer notes).
│   │   │   │   │   └── EditorAlertPopup.cs                  ← Non-blocking alert popup rendering a scrollable
│   │   │   │   │                                              list of validation warning items with fix guidance.
│   │   │   │   ├── ConfigData/
│   │   │   │   │   ├── POIAuthoringToolWindow.ConfigHistory.cs ← DrawConfigMutationScope,
│   │   │   │   │   │                                              RecordConfigChange, undo/redo stack,
│   │   │   │   │   │                                              HandleUndoShortcuts.
│   │   │   │   │   └── POIAuthoringToolWindow.ConfigFileIO.cs  ← SaveAllToJson, SaveConfig,
│   │   │   │   │                                                  LoadConfig, CopyToStreamingAssets.
│   │   │   │   ├── AssetPaths/
│   │   │   │   │   └── POIAuthoringToolWindow.AssetPaths.cs ← DrawPathRow, AbsoluteToAssetPath,
│   │   │   │   │                                               GetWallLibraryDirectory,
│   │   │   │   │                                               EnsureAssetDirectory, SanitizeFileName,
│   │   │   │   │                                               EnsureDefaultIconLibraryLoaded,
│   │   │   │   │                                               TryResolveWallIconLibraryFromConfig.
│   │   │   │   └── RigLifecycle/
│   │   │   │       └── POIAuthoringToolWindow.RigLifecycle.cs ← RefreshRigVisuals,
│   │   │   │                                                     IsRigInSyncWithConfig, ClearRig,
│   │   │   │                                                     TryResolveSceneReferences,
│   │   │   │                                                     PopulateRig, CapturePositions,
│   │   │   │                                                     SelectRigObjects, OnSceneGUI.
│   │   │   ├── POIAuthoringRigSafetyCheck.cs ← [InitializeOnLoad] static class:
│   │   │   │                                      non-blocking warning on scene save (dev may
│   │   │   │                                      be mid-placement); interactive dialog on
│   │   │   │                                      Play Mode entry (Save/Clear/Play, Continue,
│   │   │   │                                      Cancel) via PromptBeforePlayOrBuild(false).
│   │   │   └── POIAuthoringRigBuildCheck.cs ← IPreprocessBuildWithReport: delegates to the
│   │   │                                      same interactive dialog (Save/Clear/Build or
│   │   │                                      Cancel only -- no Continue without clearing);
│   │   │                                      throws BuildFailedException only if Cancel.
│   │   │   │
│   │   │   ├── Baker/                 ← NOT YET BUILT. Planned: config.json -> baked ScriptableObject build step
│   │   │   │   ├── WallConfigBaker.cs          ← Reads validated config.json → deserialises into
│   │   │   │   │                                  WallConfigData → populates WallConfigAsset
│   │   │   │   │                                  ScriptableObject → saves .asset file next to
│   │   │   │   │                                  config.json; also bakes CircuitLookupTable.
│   │   │   │   │                                  **Nullable serialization safety**: `status_pct`
│   │   │   │   │                                  must be backed by an explicit `has_status: bool`
│   │   │   │   │                                  in WallConfigData and WallConfigAsset — never a
│   │   │   │   │                                  float that silently reads as 0% ("Intact") when
│   │   │   │   │                                  the JSON key is absent. A wall with no `status`
│   │   │   │   │                                  axis (the mural) must bake to `has_status = false`;
│   │   │   │   │                                  MarkerView reads this flag before drawing any
│   │   │   │   │                                  ring/badge whatsoever. A third bool,
│   │   │   │   │                                  `status_unknown`, marks a POI whose fate is a real
│   │   │   │   │                                  historical unknown (distinct from `has_status ==
│   │   │   │   │                                  false`) — see `_2_2_Marker_Design.md` §4/§7.
│   │   │   │   └── BakerAssetPostprocessor.cs  ← AssetPostprocessor: auto-triggers baker when
│   │   │   │                                      any config.json is imported or changed; shows
│   │   │   │                                      progress bar for large POI counts.
│   │   │   │
│   │   │   └── Wizard/                ← LARGELY SUPERSEDED by Assets/Framework/Editor/POIAuthoring/, built early
│   │   │       ├── TileStoriesWizard.cs        ← EditorWindow entry point; auto-opens via
│   │   │       │                                  [InitializeOnLoad] on first import; three tabs:
│   │   │       │                                  Validate Config | Populate Scene | Documentation.
│   │   │       ├── ValidateConfigTab.cs        ← Tab: runs WallConfigValidator; shows results in
│   │   │       │                                  a scrollable list with severity icons; "Fix Now"
│   │   │       │                                  buttons for auto-fixable issues.
│   │   │       ├── PopulateSceneTab.cs         ← Tab: runs WallConfigBaker then instantiates one
│   │   │       │                                  POIAnchor prefab per POI from WallConfigAsset;
│   │   │       │                                  "Populate Scene Markers" is the one-button flow.
│   │   │       └── DocumentationTab.cs         ← Tab: inline documentation with copy-paste JSON
│   │   │                                          snippets per block type and per schema field;
│   │   │                                          written after Stage 2 when block shapes are
│   │   │                                          final, not speculatively before.
│   │   │
│   │   └── Tests/                     ← EditMode + PlayMode automated tests; 66 tests total as of
│   │       │                              2026-08-07. No TestFixtures/ folder — fixtures are inline.
│   │       ├── Editor/                ← EditMode tests (run without domain reload, fast)
│   │       │   ├── TileStories.Editor.Tests.asmdef ← Editor test assembly; references Runtime + Editor.
│   │       │   ├── CategoryPaletteTests.cs        ← Tests category→colour/icon resolution, hash
│   │       │   │                                     fallback, Configure() opt-in behaviour.
│   │       │   ├── DefaultBadgeCategoriesTests.cs  ← Tests that default badge seeding produces
│   │       │   │                                     correct 4-entry list.
│   │       │   ├── DefaultCategoryStylesTests.cs   ← Tests that default category style seeding
│   │       │   │                                     produces correct 6-entry list.
│   │       │   ├── DefaultOutlineLevelsTests.cs    ← Tests that default outline level seeding
│   │       │   │                                     produces correct 4-entry list.
│   │       │   ├── EditorCameraLookTests.cs        ← Tests that look-delta rotation is applied
│   │       │   │                                     relative to the camera's current rotation
│   │       │   │                                     (the bug where arrow keys snapped back
│   │       │   │                                     to a zero-origin is caught here).
│   │       │   ├── MarkerLayoutTests.cs            ← Tests pure-logic layout math (label offsets,
│   │       │   │                                     ring sizing) independent of Unity lifecycle.
│   │       │   ├── MarkerSymbolTexturePostprocessorTests.cs ← Tests that the AssetPostprocessor
│   │       │   │                                     correctly identifies MarkerAssets/ paths.
│   │       │   ├── MarkerVisualsParserTests.cs     ← Tests every string→enum parse path including
│   │       │   │                                     unknown/missing values and their fallbacks.
│   │       │   ├── POIAuthoringToolWriteBackTests.cs ← Tests that POI field edits round-trip
│   │       │   │                                     correctly through the authoring tool's write-
│   │       │   │                                     back path to WallConfigData.
│   │       │   ├── POIPositionResolverTests.cs     ← Tests piecewise interpolation from x_norm/
│   │       │   │                                     y_norm → 3D position with mock anchors.
│   │       │   └── StatusRampTests.cs              ← Tests StatusRamp levels, threshold ordering,
│   │       │                                          and UnknownColor distinctness.
│   │       └── Runtime/               ← PlayMode tests (require a running scene; use [UnityTest])
│   │           ├── TileStories.Tests.asmdef        ← Runtime test assembly.
│   │           ├── MarkerGalleryTests.cs            ← Instantiates every entry from MarkerGallery-
│   │           │                                       Definitions via MarkerGalleryHarness and
│   │           │                                       asserts visual state is correct.
│   │           ├── MarkerIconLibraryRuntimeTests.cs ← Tests that SpriteKeyLibrary lookups return
│   │           │                                       correct sprites at runtime.
│   │           ├── MarkerOverlapResolverTests.cs    ← Tests overlap offset assignment (clustered/
│   │           │                                       already-separated/idempotent scenarios) and
│   │           │                                       MarkerBillboard camera-facing rotation.
│   │           └── MarkerViewRuntimeTests.cs        ← Tests that MarkerView correctly wires its
│   │                                                   sub-components given various POI configs.
│   │
│   ├── Apps/                                   ← One sub-folder per wall. Framework has zero
│   │   │                                          knowledge these folders exist.
│   │   │
│   │   ├── LivingRoom/                         ← DEV-ONLY: home test wall. The only wall with
│   │   │   │                                      real, working content. Used for all iteration
│   │   │   │                                      and end-to-end testing before going to real
│   │   │   │                                      walls. Uses MockLocalizationProvider or a real
│   │   │   │                                      Immersal map. Never shipped to production.
│   │   │   ├── config.json                     ← Working POI data (20 POIs + heritage taxonomy).
│   │   │   ├── config.json.backup              ← Previous config snapshot (manual backup).
│   │   │   ├── LivingRoomScene.unity           ← The scene actually used for all dev iteration.
│   │   │   ├── 146267-LivingRoom2.bytes        ← Immersal VPS map file (naming from Immersal
│   │   │   │                                      Portal export; functionally equivalent to the
│   │   │   │                                      `map.bytes` name used in future wall docs).
│   │   │   ├── 146267-LivingRoom2-tex.glb      ← Immersal mesh export for this map (occlusion
│   │   │   │                                      geometry / visual reference).
│   │   │   ├── generate_config.py              ← Dev script: generated the initial config.json
│   │   │   │                                      with heritage taxonomy for 3 main + 15 satellite
│   │   │   │                                      POIs. Not run at build time; kept for reference.
│   │   │   ├── MarkerAssets/                   ← Wall-specific icon library assets.
│   │   │   │   └── Resources/MarkerSymbols/
│   │   │   │       ├── living_room_IconLibrary.asset  ← SpriteKeyLibrary for this wall's custom icons.
│   │   │   │       └── test_wall_IconLibrary.asset    ← Alternate test icon library.
│   │   │   └── MediaAssets/                    ← Per-POI content (currently mostly empty for dev).
│   │   │       ├── Audio/
│   │   │       ├── Images/
│   │   │       ├── Models3D/
│   │   │       └── Videos/
│   │   │
│   │   ├── Chafariz/                           ← [SCAFFOLDING ONLY] Chafariz Velho wall app.
│   │   │   │                                      Has MediaAssets/ subfolder structure but no
│   │   │   │                                      config.json, no scene, no content yet.
│   │   │   └── MediaAssets/
│   │   │       ├── Audio/
│   │   │       ├── Images/
│   │   │       ├── Models3D/
│   │   │       └── Videos/
│   │   │
│   │   ├── Mural/                              ← [SCAFFOLDING ONLY] Alto de Santa Catarina mural.
│   │   │   │                                      Has MediaAssets/ subfolder structure but no
│   │   │   │                                      config.json, no scene, no content yet.
│   │   │   └── MediaAssets/
│   │   │       ├── Audio/
│   │   │       ├── Images/
│   │   │       ├── Models3D/
│   │   │       └── Videos/
│   │   │
│   │   └── Panorama/                           ← [SCAFFOLDING ONLY] Grande Panorama de Lisboa.
│   │       │                                      Has MediaAssets/ subfolder structure but no
│   │       │                                      config.json, no scene, no content yet.
│   │       └── MediaAssets/
│   │           ├── Audio/
│   │           ├── Images/
│   │           ├── Models3D/
│   │           └── Videos/
│   │
│   ├── Dev/                                    ← Development harness assets. Not shipped.
│   │   └── MarkerGallery/                      ← Scene + assets for the marker gallery dev tool.
│   │       ├── MarkerGalleryScene.unity        ← Scene that MarkerGalleryHarness populates with
│   │       │                                      every variant from MarkerGalleryDefinitions;
│   │       │                                      used for visual inspection and PlayMode tests.
│   │       ├── Backdrops/backdrop.png          ← Background image used in the gallery scene.
│   │       └── Screenshots/                    ← PNG captures from gallery sessions (dev reference).
│   │
│   ├── StreamingAssets/                        ← Unity runtime-readable folder (no AssetDatabase
│   │   │                                          access needed); copied to device on build.
│   │   └── LivingRoom/
│   │       └── config.json                     ← Runtime-readable copy of Apps/LivingRoom/config.json.
│   │                                              Synced via "Copy to StreamingAssets" in the
│   │                                              POI Authoring Tool. Must always match the source.
│   │
│   ├── Screenshots/                            ← Development PNG screenshots captured during
│   │   └── (*.png)                                PlayMode sessions; not shipped. Named by date.
│   │
│   └── Plugins/                                ← Third-party SDKs imported by Package Manager
│       └── (Immersal SDK auto-extracted files  ← Do not edit. Regenerated on package update.)
│
├── Packages/                          ← Standard Unity package manifest folder, not project-authored content
│   └── manifest.json                           ← Lists all UPM packages; pinned versions from
│                                                  Stage 1 onward — never let Unity auto-upgrade
│                                                  a package mid-project.
│
└── Docs/                              ← Thesis-facing documentation, not shipped with the app
    ├── work-plan.md                            ← THIS FILE.
    ├── decisions.md                            ← Running log: date / decision / one-line reason.
    │                                              Append on every real design decision made during
    │                                              implementation; never reconstructed from memory.
    ├── field-notes/                  ← Raw observations from on-site testing sessions
    │   ├── panorama-capture-log.md             ← Immersal capture session notes.
    │   ├── chafariz-capture-log.md             ← Same for Chafariz.
    │   └── mural-capture-log.md                ← Same for the mural.
    └── evaluation/                   ← Thesis evaluation methodology + results
        ├── session-protocol.md                 ← Briefing script, task prompts, observer checklist.
        ├── sus-template.md                     ← SUS 10-item questionnaire in PT and EN.
        ├── ueqs-template.md                    ← UEQ-S 8-item questionnaire in PT and EN.
        ├── exit-survey.md                      ← 5-question exit survey.
        └── knowledge-check.md                  ← Pre/post factual-recall questions per wall.
```



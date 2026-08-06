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


## 2. Audit findings, 2026-08-06 — read before trusting the tree below blindly

- **Real cleanup debt found, not yet fixed**: `Assets/Framework/Editor/
  POIAuthor` (no extension) is a truncated leftover duplicate of the old
  single-file `POIAuthoringToolWindow.cs`, from whatever move/rename
  created the current `POIAuthoring/` folder split. It has its own `.meta`
  file (Unity is tracking it as an asset) but no `.cs` extension, so it
  doesn't compile — inert, but confusing clutter sitting in a shipped-looking
  location. **Delete `POIAuthor` and `POIAuthor.meta`.**
- `Assets/Framework/Editor/POIOutlineTableColumnFix.txt` is a scratch
  "here's the diff to make" note an implementing agent left behind in the
  actual Editor folder instead of its own working notes. Harmless (not
  compiled, `.txt`) but shouldn't be permanent project content — delete or
  move outside `Assets/`.
- `Assets/InitTestScene<guid>.unity` (a GUID-suffixed scene file sitting
  directly in `Assets/` root) looks like an accidental/temporary scene, not
  intentional content. Worth confirming and deleting if so.
- `Assets/Apps/LivingRoom/MediaAssets/Images/marker_symbols/` exists as an
  empty folder — harmless, but likely leftover scaffolding from before
  `MarkerAssets/` (the actually-used location) was settled on. Safe to
  delete if genuinely unused.
- **The three other planned test walls exist only as empty scaffolding** —
  `Assets/Apps/Chafariz/`, `Mural/`, `Panorama/` each have the
  `MediaAssets/{Audio,Images,Models3D,Videos}/` subfolder structure created
  but no `config.json`, no scene, no actual content. `LivingRoom` is the
  only wall with real content. This matches §1 "The three test walls" in
  `_0_work_plan.md` being still-future work for two of the three — not a
  problem, just confirmed-accurate status, not to be mistaken for "all
  three walls are underway."
- Everything under `Assets/Settings/`, `Assets/XR/`, `Assets/TextMesh Pro/`,
  `Assets/Plugins/` is standard Unity/package-installed boilerplate (URP
  settings, XR loaders, bundled TextMeshPro resources, Android manifest) —
  intentionally not itemized in the tree below, which only covers authored
  project content.

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
│   │   │   │   │                                  POIPositionResolver and other systems.
│   │   │   │   ├── WallConfigAsset.cs          ← ScriptableObject baked from config.json; the
│   │   │   │   │                                  only data type runtime code ever reads.
│   │   │   │   ├── WallConfigData.cs           ← Plain C# classes mirroring config.json schema
│   │   │   │   │                                  (POIData with captured_position, CalibrationAnchor,
│   │   │   │   │                                  CircuitData, BadgeData, etc.) used during baking;
│   │   │   │   │                                  not used at runtime directly.
│   │   │   │   ├── TileStoriesSettings.cs      ← ScriptableObject holding per-project settings
│   │   │   │   │                                  (e.g. default profile, LOD thresholds, feature
│   │   │   │   │                                  flags) — one instance per Unity project.
│   │   │   │   └── FeatureFlags.cs             ← Static class exposing bool flags (AI guide
│   │   │   │   │                                  enabled, social sharing enabled) read by every
│   │   │   │   │                                  subsystem to skip entire code paths at runtime.
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
│   │   │   │   │                                  adds WASD+mouse-look keyboard control to the
│   │   │   │   │                                  Main Camera for editor walk-through. Build first
│   │   │   │   │                                  in Stage 1, before any other feature — this is
│   │   │   │   │                                  Tier 1 of the three-tier testing pipeline.
│   │   │   │   ├── TrackingJitterSmoother.cs   ← Applied to the XR Space root transform;
│   │   │   │   │                                  intercepts each new Immersal pose before
│   │   │   │   │                                  applying it; smooths position via
│   │   │   │   │                                  Vector3.SmoothDamp (not raw Lerp — SmoothDamp
│   │   │   │   │                                  converges cleanly, raw Lerp approaches the
│   │   │   │   │                                  target asymptotically and overshoots); smooths
│   │   │   │   │                                  rotation via Quaternion.Slerp over 0.1–0.3s;
│   │   │   │   │                                  prevents visual "snap" when the VPS
│   │   │   │   │                                  re-localises on a new zone cluster.
│   │   │   │   └── TrackingMetrics.cs          ← Records per-session: time-to-first-lock,
│   │   │   │                                      zone-switch events, re-acquisition events,
│   │   │   │                                      drift measurements at 1m/3m/8m distance.
│   │   │   │
│   │   │   ├── POI/                   ← POI position resolution, category/badge palettes, status ramp, overlap resolution
│   │   │   │   ├── POIAnchor.cs                ← MonoBehaviour placed in AR space; holds one
│   │   │   │   │                                  POI's resolved 3D position (captured_position
│   │   │   │   │                                  if present, else interpolated from x_norm/y_norm)
│   │   │   │   │                                  and fires OnTapped when the marker is tapped.
│   │   │   │   ├── POIPositionResolver.cs      ← Converts (x_norm, y_norm) → 3D position under
│   │   │   │   │                                  the XR Space using piecewise linear interpolation
│   │   │   │   │                                  between calibration anchors; bypassed if the POI
│   │   │   │   │                                  has a captured_position value.
│   │   │   │   ├── MarkerOverlapResolver.cs    ← Static; called once from WallSession.SpawnPOIs()
│   │   │   │   │                                  after spawning. Snapshots every marker's screen
│   │   │   │   │                                  position once, unions markers within ~40px into
│   │   │   │   │                                  groups (union-find — never depends on spawn order),
│   │   │   │   │                                  assigns each group a stable vertical offset. Replaced
│   │   │   │   │                                  an earlier pairwise version with two real bugs (stale
│   │   │   │   │                                  comparison positions; a non-idempotent offset method
│   │   │   │   │                                  called multiple times per marker) — see Stage 1.2
│   │   │   │   │                                  plan §0.1 if this pattern looks tempting to redo.
│   │   │   │   ├── POIPool.cs                  ← Object pool of 5–8 recycled POIAnchor instances;
│   │   │   │   │                                  never instantiates/destroys per-tap — acquires
│   │   │   │   │                                  from pool and returns on hide.
│   │   │   │   ├── LODController.cs            ← Scene-wide; reads camera distance every 500ms,
│   │   │   │   │                                  shows top-5 markers beyond 5m, top-15 between
│   │   │   │   │                                  2–5m, all in viewport below 2m; never in Update().
│   │   │   │   │                                  **Critically: SetActive(false) on the entire
│   │   │   │   │                                  POIAnchor GameObject at distances beyond 7m
│   │   │   │   │                                  (not just the label — the whole GameObject),
│   │   │   │   │                                  and cull any POI whose screen-space projection
│   │   │   │   │                                  falls outside the camera's view frustum
│   │   │   │   │                                  regardless of distance. Keeping 100+ world-space
│   │   │   │   │                                  TextMesh/canvas components active across a 30m
│   │   │   │   │                                  wall degrades frame rate even when not visible
│   │   │   │   │                                  — SetActive(false) is the correct fix, not just
│   │   │   │   │                                  setting alpha to 0 or hiding the label.**
│   │   │   │   ├── LapseStateManager.cs        ← POI-level state machine: holds the active epoch
│   │   │   │   │                                  key (e.g. "pre_1755"), gates POIAnchor visibility
│   │   │   │   │                                  per lapse_states entries, broadcasts epoch change.
│   │   │   │   └── NearestPOIFinder.cs         ← Utility: given a list of POI IDs, returns the
│   │   │   │                                      one whose resolved position is nearest to a given
│   │   │   │                                      Vector3; used by circuit entry-point resolution
│   │   │   │                                      and by ambient pivot re-evaluation.
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
│   │   │   ├── POIAuthoring/           ← Actual folder name is `POIAuthoring/`, not `Authoring/` as
│   │   │   │   │                          first planned. `POIAuthoringToolWindow` is a `partial class`
│   │   │   │   │                          split across this folder (2026-08: was one 1,750-line file,
│   │   │   │   │                          refactored into per-concern partial files, zero behaviour
│   │   │   │   │                          change — see `_5.1_Editor_Tab.md` for the current map of
│   │   │   │   │                          which file owns which methods). Root file: window lifecycle,
│   │   │   │   │                          OnGUI shell. Subfolders: GlobalScene/, SpecificMarker/,
│   │   │   │   │                          Shared/ (incl. the generic symbol-table drawer reused by
│   │   │   │   │                          category/badge tables, and the curated existing-symbol
│   │   │   │   │                          picker popup), ConfigData/ (undo history + JSON I/O),
│   │   │   │   │                          AssetPaths/, RigLifecycle/. Also in this folder:
│   │   │   │   │                          `MarkerSymbolTexturePostprocessor.cs` (auto-configures
│   │   │   │   │                          texture import settings for anything dropped under a wall's
│   │   │   │   │                          `MarkerAssets/` folder).
│   │   │   │   ├── POIAuthoringRigSafetyCheck.cs ← Non-blocking warning: fires if scene is saved
│   │   │   │                                      or Play Mode entered while POIAuthoringRig still
│   │   │   │                                      has objects (normal mid-work iteration, so this
│   │   │   │                                      only warns, never blocks).
│   │   │   │   └── POIAuthoringRigBuildCheck.cs ← Hard block (IPreprocessBuildWithReport): fails any
│   │   │   │                                      build outright while POIAuthoringRig still has
│   │   │   │                                      objects. Stricter than the check above on purpose -
│   │   │   │                                      a build is visitor-facing, save/Play-mode aren't.
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
│   │   └── Tests/                     ← EditMode + PlayMode automated tests, one subfolder per mode
│   │       ├── Editor/                ← EditMode tests, mirrors Framework/Editor/ structure
│   │       │   ├── TileStories.Editor.Tests.asmdef ← Editor test assembly.
│   │       │   ├── WallConfigValidatorTests.cs ← Unity Test Framework tests for every validation
│   │       │   │                                  rule; uses fixture JSON files in TestFixtures/.
│   │       │   └── WallConfigBakerTests.cs     ← Tests that baked WallConfigAsset matches source
│   │       │                                      JSON field-for-field; catches silent bake errors.
│   │       ├── Runtime/               ← PlayMode tests, mirrors Framework/Runtime/ structure
│   │       │   ├── TileStories.Tests.asmdef    ← Runtime test assembly.
│   │       │   └── POIPositionResolverTests.cs ← Tests that piecewise interpolation produces
│   │       │                                      correct 3D positions from x_norm/y_norm inputs;
│   │       │                                      uses mock calibration anchor data.
│   │       └── TestFixtures/          ← Shared test data/prefabs reused across multiple test files
│   │           ├── valid_config.json           ← Minimal valid config used as positive test case.
│   │           ├── invalid_missing_id.json     ← Config missing required POI id field.
│   │           └── invalid_bad_xnorm.json      ← Config with x_norm > 1 (out-of-range).
│   │
│   ├── Apps/                                   ← One sub-folder per wall. Framework has zero
│   │   │                                          knowledge these folders exist.
│   │   │
│   │   ├── Panorama/                           ← Grande Panorama de Lisboa wall app.
│   │   │   ├── config.json                     ← This wall's full POI/circuit/badge data (§10).
│   │   │   ├── config.asset                    ← Auto-baked WallConfigAsset (generated, not edited).
│   │   │   ├── map.bytes                       ← Immersal VPS map file for this wall.
│   │   │   ├── mesh.glb                        ← OPTIONAL: occlusion mesh of wall geometry.
│   │   │   ├── PanoramaScene.unity             ← This wall's main scene; references WallSession,
│   │   │   │                                      ImmersalWallTracker, and the XR Space prefab.
│   │   │   └── MediaAssets/                    ← per-POI content, same 4 subfolders as Panorama
│   │   │       ├── Audio/                      ← castelo_pt.mp3, castelo_en.mp3, etc.
│   │   │       │   └── (audio clips per POI, named {poi_id}_{lang}.mp3)
│   │   │       ├── Images/                     ← castelo_1.jpg, etc.
│   │   │       │   └── (images per POI, named {poi_id}_{n}.jpg)
│   │   │       ├── Models3D/                   ← castelo.glb, etc.
│   │   │       │   └── (glTF models per POI, named {poi_id}.glb)
│   │   │       └── Videos/                     ← castelo_clip.mp4, etc.
│   │   │           └── (video clips per POI, named {poi_id}.mp4)
│   │   │
│   │   ├── Chafariz/                           ← Chafariz Velho wall app (primary dev surface).
│   │   │   ├── config.json                     ← (same shape as Panorama — different data)
│   │   │   ├── config.asset                    ← (auto-generated)
│   │   │   ├── map.bytes                       ← Immersal VPS map file (same purpose as Panorama's)
│   │   │   ├── mesh.glb                        ← OPTIONAL
│   │   │   ├── ChafarizScene.unity             ← This wall's scene (not yet built out — folder scaffolding only)
│   │   │   └── MediaAssets/                    ← per-POI content, same 4 subfolders as Panorama
│   │   │       ├── Audio/                      ← per-POI narration clips (same naming as Panorama)
│   │   │       ├── Images/                     ← per-POI photos
│   │   │       ├── Models3D/                   ← per-POI .glb models
│   │   │       └── Videos/                     ← per-POI video clips
│   │   │
│   │   ├── Mural/                              ← Alto de Santa Catarina mural wall app.
│   │   │   ├── config.json                     ← this wall's POI/circuit/badge data (not yet authored)
│   │   │   ├── config.asset                    ← (auto-generated)
│   │   │   ├── map.bytes                       ← Immersal VPS map file
│   │   │   ├── MuralScene.unity                ← this wall's scene (not yet built out — folder scaffolding only)
│   │   │   └── MediaAssets/                    ← per-POI content, same 4 subfolders as Panorama
│   │   │       ├── Audio/                      ← per-POI narration clips (same naming as Panorama)
│   │   │       ├── Images/                     ← per-POI photos
│   │   │       ├── Models3D/                   ← per-POI .glb models
│   │   │       └── Videos/                     ← per-POI video clips
│   │   │
│   │   └── LivingRoom/                         ← DEV-ONLY: home test wall. Used for fast
│   │       │                                      iteration and end-to-end testing before
│   │       │                                      going to real walls. Uses MockLocalization
│   │       │                                      or a real Immersal map of the living room.
│   │       │                                      Never shipped to production.
│   │       ├── config.json                     ← this wall's real, working POI data (20 POIs)
│   │       ├── config.asset                    ← (auto-generated)
│   │       ├── map.bytes                       ← Immersal scan of the living room (optional)
│   │       ├── LivingRoomScene.unity           ← this wall's scene — the one actually used for iteration
│   │       └── MediaAssets/                    ← per-POI content, same 4 subfolders as Panorama
│   │           ├── Audio/                      ← per-POI narration clips (dev wall, minimal content)
│   │           ├── Images/                     ← per-POI photos
│   │           ├── Models3D/                   ← per-POI .glb models
│   │           └── Videos/                     ← per-POI video clips
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
    │   ├── panorama-capture-log.md             ← Immersal capture session notes: date, conditions,
    │   │                                          image count, map quality metrics from Portal.
    │   ├── chafariz-capture-log.md             ← Same for Chafariz.
    │   └── mural-capture-log.md                ← Same for the mural.
    └── evaluation/                   ← Thesis evaluation methodology + results
        ├── session-protocol.md                 ← Printed/shared with evaluators: briefing script,
        │                                          task prompts, observer checklist.
        ├── sus-template.md                     ← SUS 10-item questionnaire in PT and EN.
        ├── ueqs-template.md                    ← UEQ-S 8-item questionnaire in PT and EN.
        ├── exit-survey.md                      ← 5-question exit survey (§3, Stage 7).
        └── knowledge-check.md                  ← Pre/post factual-recall questions per wall.
```



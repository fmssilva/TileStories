# TileStories Framework — Unified Work Plan (6 Months)


## A. Complete Project File Structure

This section is the single authoritative reference for every folder and file in the
project. Read this before reading §0–§13.
Every file listed here is described in one sentence stating its responsibility — what
it does, not what it contains.

The Unity project is a monorepo. Unity's own generated folders (`Library/`, `Temp/`,
`Logs/`, `obj/`) are excluded from version control via `.gitignore` and are not listed
here. Only authored content is listed.

```
TileStories/                          ← Unity project root (open this in Unity Hub)
│
├── Assets/
│   │
│   ├── Framework/                    ← The reusable engine. NEVER wall-specific code here.
│   │   │
│   │   ├── Runtime/                  ← Ships in the UPM package. No Editor references.
│   │   │   ├── TileStories.asmdef    ← Assembly def: Runtime assembly, no editor dependency.
│   │   │   │
│   │   │   ├── Core/
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
│   │   │   ├── Tracking/
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
│   │   │   ├── POI/
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
│   │   │   ├── Blocks/
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
│   │   │   ├── Circuits/
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
│   │   │   │   ├── Markers/                    ← uGUI ONLY — World Space Canvas, 3D-positioned
│   │   │   │   │   ├── MarkerView.cs           ← uGUI prefab: renders one POI's floating label
│   │   │   │   │   │                              (~40px, name truncated at ~15 chars, hero-tier
│   │   │   │   │   │                              only); fills colour from category (optionally
│   │   │   │   │   │                              overridden per-wall), draws status per the
│   │   │   │   │   │                              wall's marker_style (gold ring / same-hue fade /
│   │   │   │   │   │                              corner badge); no ring/badge if has_status ==
│   │   │   │   │   │                              false; a distinct "?" badge if status_unknown.
│   │   │   │   │   │                              Full design: `_2_2_Marker_Design.md`.
│   │   │   │   │   └── ClusterIndicator.cs     ← uGUI prefab: shows "N more" badge when
│   │   │   │   │                                  LODController collapses markers beyond threshold.
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
│   │   │   ├── Telemetry/
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
│   │   │   ├── Validation/
│   │   │   │   ├── WallConfigValidator.cs      ← Reads config.json; validates required fields,
│   │   │   │   │                                  value ranges (x_norm 0–1), circuit POI ID
│   │   │   │   │                                  references, badge trigger type strings; returns
│   │   │   │   │                                  list of ValidationError with line/field context.
│   │   │   │   └── ValidationError.cs          ← Plain data class: field path, message, severity
│   │   │   │                                      (Error / Warning); displayed in Wizard and
│   │   │   │                                      surfaced as Unity Console messages.
│   │   │   │
│   │   │   ├── Authoring/
│   │   │   │   ├── POIAuthoringToolWindow.cs ← Editor window for POI position capture: Populate Rig
│   │   │   │   │                                  from JSON (creates marker instances under
│   │   │   │   │                                  POIAuthoringRig) and Capture Positions to JSON
│   │   │   │   │                                  (writes captured_position back to config). Also
│   │   │   │   │                                  owns IsRigInSyncWithConfig(...) (rig-vs-JSON diff,
│   │   │   │   │                                  1mm tolerance) driving a live sync indicator and
│   │   │   │   │                                  the Clear Rig button.
│   │   │   │   ├── POIAuthoringRigSafetyCheck.cs ← Non-blocking warning: fires if scene is saved
│   │   │   │                                      or Play Mode entered while POIAuthoringRig still
│   │   │   │                                      has objects (normal mid-work iteration, so this
│   │   │   │                                      only warns, never blocks).
│   │   │   │   └── POIAuthoringRigBuildCheck.cs ← Hard block (IPreprocessBuildWithReport): fails any
│   │   │   │                                      build outright while POIAuthoringRig still has
│   │   │   │                                      objects. Stricter than the check above on purpose -
│   │   │   │                                      a build is visitor-facing, save/Play-mode aren't.
│   │   │   │
│   │   │   ├── Baker/
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
│   │   │   └── Wizard/
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
│   │   └── Tests/
│   │       ├── Editor/
│   │       │   ├── TileStories.Editor.Tests.asmdef ← Editor test assembly.
│   │       │   ├── WallConfigValidatorTests.cs ← Unity Test Framework tests for every validation
│   │       │   │                                  rule; uses fixture JSON files in TestFixtures/.
│   │       │   └── WallConfigBakerTests.cs     ← Tests that baked WallConfigAsset matches source
│   │       │                                      JSON field-for-field; catches silent bake errors.
│   │       ├── Runtime/
│   │       │   ├── TileStories.Tests.asmdef    ← Runtime test assembly.
│   │       │   └── POIPositionResolverTests.cs ← Tests that piecewise interpolation produces
│   │       │                                      correct 3D positions from x_norm/y_norm inputs;
│   │       │                                      uses mock calibration anchor data.
│   │       └── TestFixtures/
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
│   │   │   └── MediaAssets/
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
│   │   │   ├── map.bytes
│   │   │   ├── mesh.glb                        ← OPTIONAL
│   │   │   ├── ChafarizScene.unity
│   │   │   └── MediaAssets/
│   │   │       ├── Audio/
│   │   │       ├── Images/
│   │   │       ├── Models3D/
│   │   │       └── Videos/
│   │   │
│   │   ├── Mural/                              ← Alto de Santa Catarina mural wall app.
│   │   │   ├── config.json
│   │   │   ├── config.asset                    ← (auto-generated)
│   │   │   ├── map.bytes
│   │   │   ├── MuralScene.unity
│   │   │   └── MediaAssets/
│   │   │       ├── Audio/
│   │   │       ├── Images/
│   │   │       ├── Models3D/
│   │   │       └── Videos/
│   │   │
│   │   └── LivingRoom/                         ← DEV-ONLY: home test wall. Used for fast
│   │       │                                      iteration and end-to-end testing before
│   │       │                                      going to real walls. Uses MockLocalization
│   │       │                                      or a real Immersal map of the living room.
│   │       │                                      Never shipped to production.
│   │       ├── config.json
│   │       ├── config.asset                    ← (auto-generated)
│   │       ├── map.bytes                       ← Immersal scan of the living room (optional)
│   │       ├── LivingRoomScene.unity
│   │       └── MediaAssets/
│   │           ├── Audio/
│   │           ├── Images/
│   │           ├── Models3D/
│   │           └── Videos/
│   │
│   └── Plugins/                                ← Third-party SDKs imported by Package Manager
│       └── (Immersal SDK auto-extracted files  ← Do not edit. Regenerated on package update.)
│
├── Packages/
│   └── manifest.json                           ← Lists all UPM packages; pinned versions from
│                                                  Stage 1 onward — never let Unity auto-upgrade
│                                                  a package mid-project.
│
└── Docs/
    ├── work-plan.md                            ← THIS FILE.
    ├── decisions.md                            ← Running log: date / decision / one-line reason.
    │                                              Append on every real design decision made during
    │                                              implementation; never reconstructed from memory.
    ├── field-notes/
    │   ├── panorama-capture-log.md             ← Immersal capture session notes: date, conditions,
    │   │                                          image count, map quality metrics from Portal.
    │   ├── chafariz-capture-log.md             ← Same for Chafariz.
    │   └── mural-capture-log.md                ← Same for the mural.
    └── evaluation/
        ├── session-protocol.md                 ← Printed/shared with evaluators: briefing script,
        │                                          task prompts, observer checklist.
        ├── sus-template.md                     ← SUS 10-item questionnaire in PT and EN.
        ├── ueqs-template.md                    ← UEQ-S 8-item questionnaire in PT and EN.
        ├── exit-survey.md                      ← 5-question exit survey (§3, Stage 7).
        └── knowledge-check.md                  ← Pre/post factual-recall questions per wall.
```

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

## 0. Scope shift and calibration

Two decisions reorganise everything below relative to the earlier, more sprint-shaped plans:

1. **Functionality before content.** The 150-POI content set does not need to be real,
   verified, or final. Most POI text/images can be placeholder or AI-generated. Time
   saved on content-accuracy work is reinvested in building more of the framework
   properly. A small number of "hero" POIs per wall get real, careful content — enough
   to demo and to evaluate with — but this is not the bottleneck task. **Quality over
   quantity is a hard rule, not a preference**: 30 POIs with excellent content beats
   150 POIs where 120 have placeholder text — examiners and visitors both notice thin
   content, and a framework demo with a few deep examples is more convincing than a
   shallow one with many.
2. **Build the feature, then generalise it — not the reverse.** Implement each
   capability directly and concretely first (hard-coded where convenient), prove it
   works against real test walls, *then* extract it into config-driven, schema-validated,
   packaged form. The JSON validator, the ScriptableObject compiler, the Onboarding
   Wizard, and the final package/template exports all move to the back half of the
   timeline, deliberately.
3. **The "framework" claim rests on Stages 1–3, not on Stages 4–6 — say so explicitly,
   because the schedule will tempt you to forget it.** What actually supports this
   thesis's generalisability argument (thesis report §sec:pw-problem-rq,
   §subsec:pw-generalisability) is the same block code running unmodified across three
   structurally different walls; that evidence exists as soon as Stage 3 closes.
   Stages 4–6 (schema/compiler, Editor Wizard, UPM package) turn that already-proven
   core into something a *third party* could pick up without reading this thesis's
   source code — valuable, and worth doing, but packaging on top of a true claim, not
   the claim itself. §7's risk register and §8's RQ-mapping table both already note
   that Stages 4–6 are "not RQ-bearing"; the corollary, stated here so it is never lost
   under time pressure, is that **cutting Stages 4–6 if the schedule slips does not
   weaken the thesis's central contribution** — it only changes whether that
   contribution is demonstrated by an installable package or by three hand-integrated
   walls. Decide this trade-off consciously if Week 20 arrives behind schedule, rather
   than continuing to build tooling by default because it was next on the list.
4. **Scope the generalisability claim to walls, not rooms.** Every surface in §1, every
   coordinate in the schema below (`x_norm`/`y_norm` along a wall's *length*), and the
   architecture's whole framing assumes one continuous, traversable surface — curved or
   U-shaped, but one-dimensional in its navigation structure. A *room* is a different
   problem: multiple non-contiguous surfaces, volumetric wayfinding instead of movement
   along a line, and very likely several stitched Immersal maps with an explicit
   hand-off between them — none of which is designed, tested, or scheduled anywhere
   below. If room-scale support is genuinely a goal, this plan currently under-budgets
   it badly; the defensible move for a six-month thesis is to state the contribution as
   **"large, continuous heritage wall surfaces"** — matching both what §1 actually tests
   and the thesis report's own RQ wording, which already says "heritage wall surfaces"
   throughout — and name room-scale extension as acknowledged future work rather than a
   Stage 1–7 deliverable.
5. **Frame the tracking-technology contribution precisely: configuring and evaluating
   an existing VPS for a novel surface class, not inventing tracking from scratch —
   and that framing is the stronger, more honest claim, not a lesser one.** Using
   Immersal rather than building bespoke tracking does not eliminate the research gap
   identified in the thesis's related-work chapter; it relocates it. The gap was never
   "no tracking method exists" in the abstract — it's four narrower, still-unanswered
   gaps that Immersal's existence doesn't touch: a **configuration gap** (Immersal has
   never been published against a 30–40m curved/specular tile surface at this POI
   density — which capture protocol, which optimisation parameters, how the U-shaped
   join or the circular arc behaves, are all unanswered in the published literature);
   an **integration gap** (combining a VPS with 100+ persistent anchors, LOD
   management, and live visitor personalisation in one mobile app at a sustained
   frame rate is its own systems problem, regardless of whose tracking sits
   underneath); an **evaluation gap** (no published in-situ evaluation of
   VPS-based AR on a heritage wall exists with real visitor satisfaction and
   learning-gain data); and the **design gap** that motivated this thesis in the
   first place and is entirely unaffected by the tracking choice — personalised,
   non-linear, multi-epoch AR for a large wall has no published precedent either way.
   Concretely, this means the thesis should never claim to have "built a tracking
   solution" — it should claim to have *selected, configured, and empirically
   evaluated* a VPS-based AR framework for a technically demanding and previously
   unpublished surface class, and demonstrated a complete application combining
   spatial persistence, personalisation, and historical layering on that surface for
   the first time. Design Science Research, the thesis's own methodological framing,
   fully supports this: the contribution is the configured artefact plus its
   evaluation, not the algorithm underneath it — exactly the same principle already
   stated for the framework claim in item 3 above, applied here specifically to the
   tracking layer.

**Design methodology, stated once here since it governs how every stage below should
be approached, not just what each stage contains.** The right level of upfront design
is "design one stage ahead while implementing the current one," not "fully prototype
every stage before writing any code." A single comprehensive Figma/Whimsical
prototype of the entire six-month plan, tested once at the start, would mean Stage 3's
prototype decisions are built on Stage 1 UX assumptions that real-device testing will
already have overturned by the time Stage 3 starts. The one exception is the very
core interaction pattern this whole project hangs on — marker → tap → bottom-sheet
card, which §1's "Field-work protocol" and Stage 1's POI-card decisions below already
resolve with stated reasoning, so that upfront decision is effectively already made for
this plan rather than left to prototype from scratch. For everything not yet
specified this concretely below, apply the same per-stage rhythm used throughout this
plan: sketch the specific screens/flows that stage needs (a small, real number — two
or three screens, not a dozen), implement, run the informal usability check already
specified at the end of most stages, and let what's learned inform the *next* stage's
design rather than retrofitting the current one. This is also why each stage's
testing happens at the *end* of that stage rather than being batched into one big
testing phase at the end of the project — issues are cheapest to fix the stage they're
found in, and most expensive once three more stages have been built on top of an
unexamined assumption.

**Realism calibration, carried forward explicitly:** 6 months of real development time
maps to Stages 1–6 below (roughly equivalent to the old plan's "Sprints 0–4"); Stage 7's
stretch items are pursued only if time remains. Two specific features are flagged here
because each is independently large enough to consume 2–4 weeks on its own if
underestimated: the earthquake/disaster simulation sequence, and any 3D building-interior
content. Neither should be started until the core framework and evaluation are solid.
**On AI-assisted development specifically**: coding assistants (Cursor/Copilot/Claude)
realistically save roughly 30–40% of time on boilerplate and repetitive implementation,
but save close to nothing on architecture, schema design, or UX decisions — budget time
for those as if working unassisted, and only expect the speed-up on mechanical work.

The iterative method already established for this thesis (Design Science Research,
with Research-through-Design's allowance for problem framing and implementation to
co-evolve — thesis report §3.3) governs *how* each feature gets built: sketch →
prototype → implement → informal test → refine, repeated per feature, not once per phase.

**A unifying theoretical thread worth naming explicitly, even though it surfaces
concretely only later (§3, Stage 3's circuits item).** Falk & Dierking's Contextual
Model of Learning — visitor experience as continuously shaped by Personal,
Sociocultural, and Physical contexts — is, in retrospect, the academic articulation of
a separation this plan already makes throughout without naming it that way: the
*Physical* context is the wall itself (geometry, panel joins, the three-test-wall
comparison, §1); the *Personal* context is the profile system and, later, the
real-time engagement signals driving ambient pivot (§3, Stage 3); the *Sociocultural*
context is the family/children's-circuit design and the social-sharing feature (§3,
Stage 3, items 1 and 5). Worth stating this explicitly in the thesis's related-work or
methodology chapter as the framing this whole architecture sits inside, rather than
introducing the model only at the point the Butterfly Prompt makes it concrete — the
three-context separation was already organising this plan's structure well before any
of this document used Falk & Dierking's own terms for it.

**A note on reconciling this plan with the thesis chapters already drafted.** This
plan organises work into seven *Stages* (weeks); `chapter-proposed-solution.tex` and
`chapter-workplan.tex` organise the same project into five *Phases* (months: Phase 0
Core AR, Phase 1 Personalisation & Time, Phase 2 Voice & Engagement, Phase 3 Evaluation
Readiness, Phase 4+ Stretch), each with its own Gantt bar and feature list. The two
breakdowns do not nest cleanly — the chapters' Phase 1, for instance, bundles
onboarding, profile-adaptive content, and the timeline together, while this plan splits
that same material across Stage 2 item 9 (profile content inside existing blocks) and
Stage 3 item 1 (the onboarding flow itself), with the timeline (Stage 2 item 6) landing
earlier than either. The rough correspondence is:

| Stage(s) here | Roughly corresponds to (thesis chapter Phase) |
|---|---|
| 1 | Phase 0 — Core AR |
| 2 (items 1–8) | Spans the Phase 0/1 boundary — block system and LOD work mostly precede onboarding |
| 2 (item 9) + 3 (items 1–2) | Phase 1 — Personalisation and Time |
| 3 (items 3–7) | Phase 2 — Voice and Engagement |
| 3 (item 8) + 4–6 | Not named in the current Phase list at all — this is the framework-productisation work the chapters don't yet describe |
| 7 | Phase 3 — Evaluation Readiness, plus Phase 4+ Stretch |

Before this plan is executed, update the chapters to either adopt this Stage
breakdown directly or state plainly that Stages are an internal sub-decomposition of
each Phase — otherwise an examiner reading both documents finds two different project
plans for the same six months. The chapters also need a short addition introducing the
mural wall (Alto de Santa Catarina) as a third case study: as currently written,
`chapter-proposed-solution.tex` discusses only "Two Cases, One Surface Class" (the
Panorama and Chafariz). The mural is a welcome addition — it is, in fact, the concrete
filling of the dashed "Walls/[Future]" box already drawn in the architecture diagram
(`fig:architecture`) — but it needs its own paragraph in §sec:pw-domain and a row in the
case-comparison table, including an explicit statement of whether it is a
development-only proof case or also an evaluation site (the current evaluation plan in
§sec:pw-evaluation names only Chafariz, and opportunistically the Panorama).

---

## 1. The three test walls

| | Grande Panorama de Lisboa | Chafariz Velho | Alto de Santa Catarina mural |
|---|---|---|---|
| Content type | Built city — buildings | Historical scenes/figures | Wildlife/biodiversity illustration |
| Geometry | U-shaped, indoor | Circular, outdoor | Flat, outdoor |
| Date | c. 1700 | Structure 1755/1775; tiles 1950s | 2023 (artist: Tiago Hacke) |
| Scale | 23 m | ~30–40 m (est.) | 30 m × 6 m |
| Natural "category" axis | Religious/royal/military/civic/maritime/etc. | Wall-specific (events/figures, not buildings) | Species type: bird/mammal/amphibian/insect |
| Natural "status" axis | Earthquake fate — `status_pct: float (0–100)` + `has_status`/`status_unknown` bools (Stage 2.3 refined this from an earlier 4-state-enum sketch into a continuous destruction scale, richer for the ring/fade/badge rendering it drives; `status_unknown` keeps "this building's fate is a real historical unknown" distinct from "this wall has no status axis") | Needs its own axis — no earthquake fate applies | **None applies at all** — there is no equivalent "fate" dimension for a painted hedgehog |
| Access | Closed (PRR works) — opportunistic only | Guaranteed, primary dev surface | Guaranteed, public, freely accessible |

The third wall proves that **both** taxonomy axes (category *and* status), not just
category, must be wall-defined data, never framework-hardcoded fields. Source for the
third wall, verified directly: SIMAS/Câmara Municipal de Oeiras press coverage, *New in
Oeiras*, 19 Oct. 2023 [^w1].

**Mural wall — concrete specification (the third wall must be as concretely defined as
the other two, otherwise Stage 2 cannot prove the block system runs "unmodified" against
it):**

- **Hero animals (the mural's equivalent of the Panorama's 10 hero buildings):** based
  on the verified press coverage and publicly visible photographs of the mural, it
  depicts wildlife of the Oeiras/Lisbon coastal ecosystem painted by Tiago Hacke in
  2023. Provisional hero set to confirm on the first site visit: golden eagle
  (águia-real), white stork (cegonha-branca), European bee-eater (abelharuco), common
  kingfisher (guarda-rios), European hedgehog (ouriço-cacheiro), Iberian green frog
  (rã-verde), Atlantic puffin (papagaio-do-mar), and 1–2 large habitat/plant depictions.
  The specific animals depicted must be confirmed on site and each assigned a stable
  `"id"` — this is the mural's analogue of the Panorama's NOVA FCSH building list, and
  without it nothing can be config-named.

- **Category taxonomy for the mural** (`category` field, free-form, wall-defined
  data — not a framework enum): proposed values are `["bird"]`, `["mammal"]`,
  `["reptile_amphibian"]`, `["insect"]`, `["plant_habitat"]`. These drive the colour-
  coded marker system for this wall only; the framework assigns no meaning to them and
  the colour that represents "bird" on this wall has no relationship to what "religious"
  means on the Panorama.

- **Status axis for the mural: explicitly absent.** The `status` field is omitted from
  every mural POI — not set to `null`, not set to `""`, simply not present. This is the
  critical test case: the schema validator (Stage 4) must not flag a missing `status`
  field as an error for this wall. The marker renderer must show no border ring for a
  POI with no `status` field. Test this explicitly in Stage 1 from the first day the
  renderer is written.

- **Lapse-state axis for the mural: time-of-day.** The Panorama uses a historical-time
  lapse (`pre_1755` / `earthquake` / `pombaline` / `today`). The mural will use
  **time-of-day** (`dawn` / `day` / `dusk` / `night`) to show which animals are active
  at each time, with a sky-colour overlay change as the visual language. This is the
  explicit proof that the lapse mechanism generalises beyond historical time to any
  categorical dimension the wall author defines. Implement this by Stage 2, item 6.

- **Minimal mural config (for Stage 0 and Stage 1 starting point):**
```json
{
  "wall_id": "alto_santa_catarina_mural",
  "config_schema_version": "0.1",
  "geometry": { "type": "flat", "length_m": 30, "height_m": 6 },
  "pois": [
    {
      "id": "aguia_real",
      "name_pt": "Águia-real",
      "name_en": "Golden Eagle",
      "category": ["bird"],
      "wall_position": { "x_norm": 0.15, "y_norm": 0.60 },
      "summary_pt": "A maior ave de rapina da Península Ibérica.",
      "summary_en": "The largest bird of prey in the Iberian Peninsula.",
      "content_by_profile": {
        "tourist": "...", "student": "...", "academic": "...", "child": "..."
      },
      "lapse_states": {
        "dawn": { "visible": true, "marker_tint": "amber" },
        "day": { "visible": false },
        "dusk": { "visible": true, "marker_tint": "amber" },
        "night": { "visible": false }
      }
    }
  ]
}
```
Note: no `status` field anywhere. This is the test case for both the validator and
the renderer.



**Institutional and content-source tasks — easy to lose track of since they're not
code, but they block everything else if left undone:**
- **Formalise wall access in writing**, even informally by email, before relying on
  it in the thesis report — for the Panorama specifically, with whichever institution
  currently controls access during the PRR works; for Chafariz and the mural, this is
  lower-risk since both already have guaranteed public access, but get the same email
  confirmation regardless so the thesis can cite a concrete access basis rather than an
  assumption.
- **Obtain the actual list of POI names and approximate wall positions before
  treating "150 POIs" as a real number anywhere.** This plan refers to "all 150 POIs"
  throughout, but that count and the buildings it refers to come from an external
  source — the NOVA FCSH building-identification list for the Panorama — that has to
  be physically obtained, not assumed to exist in hand already. Do this in the first
  days of Stage 1: without this list, neither the 10 hero POIs nor the scale-proving
  remainder can be named at all, and everything downstream (content writing, the
  150-marker LOD/clustering test, the "15/150 buildings discovered" gamification
  counter) is blocked on it.

**Field-work protocol for capturing any wall (apply to all three; this is the repeatable
method, not a one-off task):**
- Photograph at 4–6 overlapping shots covering the full length, consistent distance,
  tripod if possible, morning light preferred (fewer visitor shadows for outdoor walls).
  This reference photography is distinct from, and lighter-weight than, the Immersal
  mapping capture below — it's for content/documentation purposes (planning marker
  positions, checking framing), not for building the VPS map itself.
- **Immersal mapping capture specifically — concrete numbers, not just "multiple
  angles":** walk the full length of the wall with the Immersal Mapper app capturing
  roughly 150–200 photos per wall, at three distances (~1m, ~2m, ~5m from the surface),
  including some shots angled up and down rather than only straight-on, to give the
  map enough vertical feature coverage. For outdoor walls (Chafariz, the mural),
  repeat at a second time of day if the 100-image free-tier cap (§3, Stage 1) allows
  it, to record lighting variation up front rather than discovering it as a tracking
  failure later. **After each capture, check the resulting map's quality metrics in
  the Immersal Developer Portal** (point count, coverage) before considering that
  wall's capture finished — a map that builds successfully but covers the wall
  unevenly is a worse outcome to discover at evaluation time than at capture time.
- Measure wall geometry directly: total length, average viewer-to-wall distance,
  approximate curvature/angle at panel joins (needed to decide x/y vs. x/y/z vs.
  x/y/z/yaw tracking, per the thesis report's own established distinction).
- Test on-site connectivity (WiFi/4G) before deciding whether cloud-dependent tracking
  is viable or the deployment must be offline-first.
- Identify and name a small set of "hero" POIs per wall before any content writing
  starts — for the Panorama, the suggested ten are: Castelo de São Jorge, Sé de
  Lisboa, Paço da Ribeira, Torre de Belém, Jerónimos, São Vicente de Fora, Convento do
  Carmo, Igreja de São Roque, Terreiro do Paço, Madre de Deus.

**Outstanding before Stage 1 closes:** the list above exists only for the Panorama.
Chafariz Velho and the mural wall each need their own hero-POI list, chosen by the
same method, before Stage 2's content work can start against all three walls — it
cannot be a different process per wall without weakening the cross-wall comparison
this whole plan depends on. Likewise, the mural wall's own field-work pass
(photography, Immersal mapping, on-site connectivity test) is not yet scheduled
anywhere in this plan — do it within Stage 1 or the very first days of Stage 2, so the
mural's reference data exists before Stage 2 item 6 needs it (§3, below), rather than
becoming a Stage-2 blocker discovered mid-stage. Both of these are half-day tasks, not
research problems, and there is no reason for either to drift past Stage 1. Before
writing detailed content, cross-reference the NOVA FCSH list and the Arquivo
Municipal de Lisboa (or equivalent sources for Chafariz/the mural) for **at least the
ten hero POIs per wall** — full source verification for the long tail of POIs is the
decoupled, non-thesis-critical editorial project already described in §5.

---

## 2. Architectural principles carried forward

- **No bespoke authoring paradigm.** Unity MARS, Unity's own first-party answer to
  this exact problem, is now deprecated (deprecated Unity 6.1, April 2025; end of
  support Unity 6.3, October 2025) [^w2][^w3]. The lesson is not "config-driven AR
  doesn't work" — it's *why this specific implementation didn't survive*: MARS asked
  developers to learn an entirely separate authoring paradigm (its own proxy system,
  its own rules UI) layered on top of, rather than built from, Unity's standard
  workflow. Tellingly, MARS's single best feature — environment simulation for
  testing without a physical device — survived the deprecation by being extracted
  into a lighter, free, standalone feature, **XR Simulation**, once decoupled from the
  rest of the bespoke ecosystem around it [^w3a]. Build on standard Unity primitives;
  let JSON config and the Wizard be a thin layer over that. Every Wizard feature stays
  optional — a developer must always be able to ignore it and hand-build a scene the
  normal way.
- **Prefer native extension points over a custom service locator.** MRTK's own v2→v3
  rewrite moved away from a heavyweight bespoke service locator toward Unity's native
  XR Subsystem infrastructure [^w4][^w5][^w6], after MRTK2's locator pattern was
  widely reported as confusing [^w7]. MRTK3 — the most mature, still-actively-
  maintained modular AR/MR framework that exists, surviving even Microsoft's 2023
  layoffs of the original team by moving to community maintenance [^w4][^w5] — also
  validates two specific patterns this framework leans on directly, not just the
  general "avoid a service locator" lesson: **profile assets that select which
  subsystems are active per deployment target** (the direct precedent for one
  `config.json` per wall selecting that wall's behaviour) and **explicit "data
  binding for branding, theming, dynamic data, and complex lists" shipped as a
  first-class feature** [^w6a] — confirming that a content-config-drives-UI
  architecture is a proven pattern in a shipped, mature framework, not an
  experimental risk unique to this project.
- **Data drives behaviour; views are swappable.** Yarn Spinner — a free, open-source
  dialogue/narrative engine shipped in commercially released titles including
  *Night in the Woods*, *Dredge*, *Escape Academy*, and *A Short Hike* [^w8] — is
  built on exactly this principle: content is
  data the engine consumes, views are swappable components that receive it, and
  localisation is designed in from the start rather than bolted on [^w8][^w9] — is the
  direct model for the POI block architecture (§3, Stage 2) and for this project's
  PT/EN (later ES/FR) requirement. This is not an untested toy precedent; it's a
  free, production-proven one. Yarn Spinner's own "Dialogue Views" — swappable
  presentation components fed by a content-driving "Dialogue Runner" that doesn't know
  what a view *looks like*, only that it can be asked to render [^w8][^w9] — is the same
  shape as this framework's `TileStoriesUIBlock` registry, not just an analogous idea;
  reading Yarn Spinner's own architecture docs is a legitimate, cheap way to
  sanity-check a block-registry design decision against a shipped precedent.
- **Decouple structure from entry point — the caller picks where to begin, the
  content never hardcodes it.** Both narrative engines already cited above validate
  this as a normal, production-grade pattern, not a special case invented for this
  framework: Yarn Spinner's `DialogueRunner.StartDialogue(nodeName)` accepts any
  named node as the starting point, with "Start" merely its default parameter value
  rather than a hardcoded requirement [^w24]; Ink addresses every knot and stitch
  individually and lets calling code divert to any of them by address, with no
  single entry point baked into the content format itself [^w25]. This is the direct
  precedent for resolving a circuit's actual starting POI from the visitor's current
  wall position at runtime (§3, Stage 3's circuits item) rather than always starting
  at index 0 of an authored sequence — the same "data drives behaviour, the engine
  doesn't assume a fixed starting state" principle already adopted above, applied to
  *where a sequence begins* rather than only to *what renders it*.
- **Name things using an existing taxonomy where one exists.** Sam Kabo Ashwell's
  branching-narrative taxonomy — Gauntlet, Branch-and-Bottleneck, Hub-and-Spoke,
  Parallel Tracks, Node Graph [^w10] — names the circuit/progression types better than
  inventing new terms would. **Use the taxonomy's own names as the literal
  `circuit_type` string values in the schema** — `gauntlet`, `branch_and_bottleneck`,
  `hub_and_spoke`, `parallel_tracks`, `node_graph` — rather than a project-specific
  set of synonyms; this makes the framework immediately legible to anyone with a
  game- or narrative-design background, and the taxonomy's own literature already
  describes which patterns combine sensibly (parallel tracks plus hub-and-spoke is
  common) versus which are powerful but error-prone (node graph, which is why §7's
  risk register and this plan's circuit work both treat it as the one type needing
  the strongest validation before shipping, or deferring entirely in the first
  version behind `gauntlet`/`branch_and_bottleneck`/`hub_and_spoke`). Worth noting
  even at this naming stage, since it surfaces concretely later (§3, Stage 3's
  circuits item): the taxonomy already encodes a spectrum between **authorial
  intent** (Gauntlet's designed, ordered sequence) and **agency** (Hub-and-Spoke's
  explicit any-order access) — a recognised tension in interactive-narrative design
  [^w30][^w31] this framework's `circuit_type` field operationalises rather than just
  references abstractly.
- **JSON is the portable source of truth; ScriptableObject is the fast runtime cache —
  not a binary choice, each format doing what it's good at.** The trade-off is worth
  stating explicitly rather than picking one by default, since MRTK's own precedent
  (profile *assets*, not JSON) genuinely cuts the other way:

  | | Raw JSON | ScriptableObject |
  |---|---|---|
  | Editable outside Unity | Yes — any text editor, spreadsheet export, future web tool | No — requires the Editor |
  | Type safety / autocomplete | None natively | Full (C# fields, enums) |
  | Version-control diffing | Clean, line-based | Noisy (binary or large `.asset` files) |
  | Inspector editing | None without custom tooling | Native |
  | Runtime parsing cost | Pay per load (mitigated by baking) | Zero — already deserialised |

  JSON wins on portability (a future content partner, or a script exporting from a
  spreadsheet of POI text, never needs Unity installed); ScriptableObject wins on
  runtime speed and safety. Using both, each where it's strong, gets both properties:
  author in JSON, validate it (Stage 4), then **bake it into a `WallConfigAsset`
  ScriptableObject** at import/build time — a generated, disposable cache, never
  hand-edited — so runtime code reads only the fast, GC-friendly ScriptableObject, and
  the JSON is re-parsed only when content actually changes. This is exactly the
  "compile a portable authoring format into a fast runtime asset" pattern Yarn Spinner
  itself uses (author in `.yarn`, compile to a runtime program) [^w9] — so this isn't a
  novel risk invented for this project either; it's a second instance of an already-
  proven pattern, from the same precedent already cited above for the block
  architecture.
- **This is closer to a headless CMS than to a no-code AR authoring tool — know which
  category you're in, since the two have very different "who is this for" answers.**
  A headless CMS (Strapi, Contentful: content authors edit structured data; the
  rendering layer is fixed code that doesn't change per piece of content) is the
  right comparison; a no-code AR tool aimed at non-programmers building one-off
  experiences through a visual editor (Adobe Aero, Zappar ZapWorks Studio) is not.
  This framework's primary user is *future-you or a hired developer*, not a museum
  curator with no Unity installed — which is the right call for a thesis-scale
  project, and it's exactly what resolves the JSON-vs-ScriptableObject question above
  in JSON's favour as the *authored* format (a developer comfortable with a text
  editor and version control) while still allowing the ScriptableObject bake for
  runtime performance.
- **Not competing with no-code museum-tour builders** (PandaSuite, STQRY, izi.TRAVEL,
  SmartGuide, Cuseum) [^w11] — those solve GPS/beacon-proximity triggering for
  non-technical curators; this framework solves precise AR registration on a single
  continuous surface for a technical user. Worth being explicit about *why* this
  isn't direct competition rather than just asserting it: every one of those
  platforms triggers content by proximity ("you are near this gallery"), not by
  millimetre-scale visual registration against a continuous surface with 100+
  adjacent points of interest — none of them attempt the specific AR-registration
  problem this framework and thesis actually solve. That existing commercial category
  is still useful context, though: it confirms config-driven content/rendering
  separation is a validated commercial pattern at scale, not a research risk unique
  to choosing it here.
- **`.glb` meshes load via Unity glTFast** (`com.unity.cloud.gltfast`) — first-party,
  faster than the community-maintained alternative, loads directly from `byte[]`, and
  has a defer-agent system that spreads mesh-loading cost across frames [^w13]. Note
  this covers two genuinely different `.glb` uses that should not be confused with
  each other: the **3D model block**'s per-POI content meshes (Stage 2, item 5 — a
  visible, tappable landmark model) and an **optional per-wall occlusion/visualisation
  mesh** (a captured or modelled approximation of the wall's own physical geometry,
  stored as `mesh.glb` alongside that wall's `config.json` and Immersal `map.bytes`,
  §10's schema) used so virtual content can be correctly occluded by the real wall's
  geometry rather than always rendering in front of it. The second is a real but
  lower-priority capability — note it in the per-wall asset folder structure now (§10,
  §12) so the schema doesn't need a breaking change to add it later, but treat
  building the occlusion behaviour itself as Stage 7 stretch scope at the earliest,
  not a Stage 1–3 commitment.
- **Performance and memory discipline, stated as hard constraints from Stage 1
  onward, not aspirations to clean up later.** Three rules, applying across every
  block type and every wall: (1) JSON parsing and object instantiation happen only at
  the load-phase boundary (scene load, or the Stage 4 import/bake step once it
  exists) — **never inside `Update()` or any per-frame code path**; (2) UI cards are
  **pooled** (5–8 recycled instances is enough for any realistic on-screen marker
  density) — never instantiated-and-destroyed per POI tap, which is the single most
  common cause of GC-spike frame drops in a UI-heavy mobile app; (3) media
  (audio/image/video/3D model) is referenced by path and **lazy-loaded on open,
  unloaded on close** — never pre-loaded for all 150+ POIs at scene start, which is
  the direct cause of the long cold-start time and memory pressure a naive
  implementation would hit immediately once scale-testing begins (§5's "demonstrate
  scale" requirement specifically exists to surface this early). Once Stage 4's
  JSON→ScriptableObject bake (above) exists, a fourth rule follows for free: **no
  JSON parsing at runtime at all**, only at import/build time — runtime code only
  ever reads the already-deserialised `WallConfigAsset`.
- **UI system: a deliberate hybrid — UI Toolkit for all screen-space UI, uGUI for
  world-space 3D elements only.** This replaces an earlier blanket "uGUI throughout"
  decision, which a deeper review found to be wrong for this project's specific goals.
  The correct decision, properly researched, is a precise one, and it is worth
  stating exactly so an implementing agent never has to re-derive it.

  **The boundary rule is spatial, not about complexity or screen size:**
  - **World-space (3D, physically anchored to the AR wall) → uGUI**, specifically
    a `Canvas` set to *World Space* render mode, attached to the XR Space container
    alongside the POIAnchor GameObjects. This is the only category where uGUI is
    genuinely superior: every floating marker label, every wayfinding arrow, every
    "tap-me" indicator must live as a `GameObject` in 3D space so Immersal's pose
    updates can move it correctly, depth-testing works against the camera, and the
    element can be shown/hidden by the LOD and frustum-culling system (which operates
    on `GameObject.SetActive()`). UI Toolkit's world-space story is genuinely limited
    — Unity's own documentation confirms it, confirmed-2025 developer consensus
    confirms it [^w36a] — and this category is the *only* place in the whole project
    where that limitation actually matters.
  - **Everything else — all screen-space UI → UI Toolkit.** "Screen-space" means:
    overlays that are positioned relative to the screen, not to a 3D point in the
    wall. This is *most* of the app's UI surface area: the onboarding flow, the
    profile selector, the consent notice, the detail card (bottom sheet), the circuit
    selection screen, the circuit progress rail, the completion screen, the
    gamification toasts (badge unlock, butterfly prompt, did-you-know), the guide
    character overlay, the FAB menu, the search/filter overlay, the settings screen,
    the knowledge check, the exit survey, the scanning-state overlay, the
    lock-success animation, the achievement list, the discovery counter. All of these.

  **Why UI Toolkit is the right choice for this goal, not just a nice-to-have:**
  The goal is "premium style, not a 2000-game look." That is precisely what UI
  Toolkit is optimised for in 2025–2026, and precisely where uGUI shows its age.
  Concretely: (1) **Responsiveness** — UI Toolkit uses a CSS Flexbox layout engine.
  A bottom sheet with a scrollable block stack, a two-column badge grid, a
  horizontally-centred profile card — these are trivial in Flexbox and require
  carefully nested `HorizontalLayoutGroup`/`VerticalLayoutGroup`/anchor-pivot
  combinations in uGUI that break whenever a new block type or screen size is added.
  UI Toolkit's `flex-direction`, `flex-wrap`, `justify-content`, and `align-items`
  directly produce responsive layouts that uGUI only approximates. (2) **Safe area /
  notch handling** — UI Toolkit exposes `Screen.safeArea` cleanly (with the
  coordinate inversion documented at [^w36a]) so the bottom sheet and the FAB never
  overlap a notch or the home indicator bar. In uGUI this requires a custom
  `SafeAreaLayout` script that resets `RectTransform` anchors at runtime — doable,
  but scaffolding that does not need to exist. (3) **Styling consistency** — a single
  `DesignTokens.uss` file defines all colours, corner radii, spacing, typography,
  and motion timing for every screen in one place. Changing the app's accent colour
  is editing one line of CSS. In uGUI, the same change requires hunting through
  dozens of prefabs, Shared Material assets, and hardcoded serialised fields. (4)
  **Performance** — UI Toolkit batches an entire panel into a single draw call.
  The detail card, with its 6-block stack, its scroll view, its multiple Text labels
  and RawImage textures, costs one draw call. The same UI in uGUI costs one draw
  call *per element*. On a thermal-constrained mobile device running Immersal's
  localisation pipeline in parallel, this is a real, measurable difference. (5)
  **Premium visual quality** — UI Toolkit's renderer produces pixel-precise, vector-
  quality borders, rounded corners, shadows, and gradients at any screen density
  without texture atlases. A card with a `border-radius: 16px; box-shadow: ...` looks
  like a native iOS/Android component. The same in uGUI requires a nine-slice sprite
  or a custom shader. (6) **Data binding** — Unity 6 shipped a production-ready
  runtime data binding system for UI Toolkit [^w36b] — a POIData object's name, text,
  and badge image can be bound directly to the UXML visual tree with one line, rather
  than calling `label.text = poi.name` in a script for every field. This is
  particularly valuable for the block stack, where each block type binds to a
  different subset of the same POI data object.

  **The practical hybrid in the file structure (updated from the earlier version):**
  Every file listed under `UI/` in §A that renders screen-space content uses
  UI Toolkit (`UIDocument` component + UXML + USS). Every file listed under
  `UI/Markers/` and the `NextStopArrow.cs` uses uGUI Canvas World-Space. That is
  the entire boundary — two files and their folder use uGUI; everything else uses
  UI Toolkit. Coexistence between the two systems is fully supported and stable in
  Unity 6.3 LTS [^w36a] — there is no rendering conflict, no event-system conflict,
  and no build-size overhead. The `UIDocument` component renders on a `PanelSettings`
  asset that sits in front of the AR camera as a screen overlay, completely
  independent of the World-Space Canvas that POI markers use.

  **One caveat worth building around from the start**: UI Toolkit has no native
  `Animator`/`Timeline` integration. For the handful of transitions that need to feel
  polished and "alive" — the bottom sheet sliding up, the badge toast flying in,
  the scanning-state pulse — use USS transitions (`transition: transform 0.3s
  ease-out;`) for simple ones, and for anything more complex (the lock-success
  animation, the lapse-epoch crossfade) either trigger a uGUI-based overlay element
  specifically for that animation, or use a lightweight tween library (DOTween works
  alongside UI Toolkit by tweening a `VisualElement`'s inline style properties
  directly). This is a known, documented gap in UI Toolkit, not a showstopper —
  every real project that uses UI Toolkit for screen UI has solved it the same way.

  **Stage 0's UX Design Sprint implication**: design tokens go into `DesignTokens.uss`
  on day one of Stage 0 — corner radius, spacing scale, colour palette, font scale,
  motion curve — before any UXML is written. Every component inherits from them.
  This is the step that makes "premium, consistent look" reliable rather than
  aspirational.

---

## 3. Stage-by-stage plan


### Stage 0 — UX Design Sprint (Week 0 — before any code is written) »» in fact we'll do first the very base of stage 1 and so we implement the prject with some immmersal map and we add some POIs... and when we have things working in terms of AR detecting some POIs with simple markers, then yes we'll go into design before implementing detail cards, etc. 

**Why this exists as its own numbered stage.** Three to five days of deliberate design
work before writing any code prevents weeks of refactoring later. The decisions resolved
here are the cheapest to change on paper and the most expensive to change once they are
embedded in a scene hierarchy, a prefab chain, and a block-registration system. This is
not a lengthy design process — it is one time-boxed sprint, never repeated, aimed at a
small set of high-consequence decisions that affect everything else.

**UX decisions to resolve in Stage 0:**

1. **Detail-card placement.** Three options were evaluated: floating card near the
   marker (immersive but unreadable with many POIs visible, occlusion risk, rejected);
   side panel (poor in portrait-format view against a tall wall, rejected); fixed bottom
   sheet ≤40% screen height plus a small floating label (~40px, name only, truncated at
   ~15 chars) anchored spatially to the marker. The bottom-sheet-plus-label is the
   confirmed choice (used by Bloomberg Connects and Smartify for exactly this trade-off).
   **Mock it against a real Chafariz photograph in Stage 0** — confirm the layout before
   building it. Non-negotiables: dismissable by swipe-down AND by tap-elsewhere; never
   covers more than 40% of screen height; if content is very long, make the sheet
   user-expandable, never auto-expanded.

2. **Marker shape and encoding system.** Originally sketched in the Flutter
   prototyping phase as: category encoded by fill colour + icon, earthquake fate
   encoded by border colour + style (solid-green for intact through progressively
   shorter dashes to dotted-red for vanished), importance hierarchy (large labelled
   icon for hero POIs, small colour-only dot for secondary scale-proving POIs).
   **Stage 2.3 refined the colour scheme**: the ring/badge status ramp ended up
   gold→rust (not green→red) specifically so it never shares a hue family with any
   category colour (a live problem the green/red sketch had — see
   `_2_2_Marker_Design.md` §4 for the full reasoning) — dash rhythm (solid→dotted)
   is unchanged from this sketch. Stage 2.3 also turned "one fixed encoding" into
   3 dev-selectable `marker_style` options (gold ring / same-hue fade / corner
   badge), of which this original sketch is one (`outline_gold`). The importance
   hierarchy is **partially resolved**: hero POIs get a persistent label (Stage
   2.3's `is_hero`), but "small colour-only dot for secondary" — an actual size/
   shape reduction, not just hiding the label — is not yet built; likely belongs
   with the LOD/marker-density system (§ below) rather than Stage 2.3's rendering
   work, since it's a distance-driven concern, not a per-POI content concern. The
   Stage 0 decision: **verify
   the no-status case renders cleanly** — a POI with no `status` field (such as every
   POI on the mural wall) must show no border ring at all, not a broken or empty ring.
   Mock this case explicitly in Stage 0 before the renderer is written. **Confirmed
   in Stage 2.3**: `has_status` guards this exactly as specified, plus a third
   `status_unknown` state for a documented-but-unresolved historical fate (see
   `_2_2_Marker_Design.md` §4 principle 3).

3. **Guide character implementation approach.** Choose between: (a) sprite-swap between
   a small set of named static frames (idle/talking/pointing-left/pointing-right/
   surprised) — no extra Unity packages, simpler, the visual difference is negligible in
   a 15-minute visitor session; (b) bone-rigged 2D animation via Unity's
   `com.unity.2d.animation` package — smoother transitions, adds a package dependency.
   **Recommendation: start with sprite-swap.** Install `com.unity.2d.animation` only if
   the sprite-swap version feels visually inadequate after Stage 3 user testing. This
   decision affects the package manifest from Week 1, so it must be made here, not
   deferred.

4. **Scanning-state UX.** What does the screen show while Immersal searches for a wall
   lock? Mock at least two options: (a) fullscreen camera feed with animated scan-line
   overlay and a text instruction ("Point camera at the tile wall"); (b) camera feed with
   a wall-silhouette guide overlay. Recommendation: option (a) for Stage 1, upgradable
   in Stage 7's UX polish pass. Non-negotiable: the transition from "scanning" to
   "locked" must be a rewarding moment — a brief animation (e.g. a gold ring pulse
   around the nearest marker) followed by markers appearing — not just the disappearance
   of a loading spinner.

5. **Minimal viable wall path (simplicity guarantee).** Design what a developer sees
   the first time they open the Wizard with a blank project. The Wizard's first screen
   must present only `wall_id`, `config_schema_version`, `geometry`, and one POI with
   `name_pt/en`, `category`, `wall_position`, and `summary_pt/en` — nothing else. Every
   other field in §10's full schema is an optional, addable-later increment. Sketch this
   Wizard first screen in Stage 0, even though the Wizard itself isn't built until
   Stage 5 — committing to the UX direction now means Stage 5 has a clear target.

**Stage 0 outputs (small deliverables, not code):**
- One static mock of the main AR view: markers visible, a detail card open, and the
  scanning-state indicator.
- Confirmed package manifest list (which packages will be in `package.json` from day
  one — see §11 for the full list).
- Confirmed guide-character approach (sprite-swap or bone-rigged).
- Minimal viable config sketch for all three walls: `wall_id`, `geometry`, category
  taxonomy per wall, and one hero POI per wall. This is the concrete starting point for
  Stage 1's first JSON files.

### Stage 1 — Foundations (Weeks 1–3)

<MY NOTES TO CHECK LATTER:> (besides these things, some tasks i did "by hand in unity" was to install the immersal sdk and then click in fix some errors that appeared automatically, and then drag the immersal sdk PREFAB "not the folder" to the hieaarchy scene root... and then immersal sdk » login with token...) so all these steps then we need to try with other acount and to make this a bit automatic or t create a untiy helper guide for the user to know exacly what to do... 
About your hierarchy screenshot:

ImmersalSDK at root is correct.
Do not move XR Origin or AR Session inside ImmersalSDK.
Keep ImmersalSDK as a sibling of AR Session, XR Origin, WallSession.
Delete duplicate top-level Main Camera (keep only XR Origin -> Camera Offset -> Main Camera).
Keep only one Directional Light.
)

(and then the agent dis many things like: Delta update I already applied:

I restored your map filenames so Immersal can parse the map id from the filename prefix.
I set the LivingRoom map id in config to 146267.... all these details should be present here to make sure we have everything with the correct path and configs and names... )

**Week 1 concrete bootstrap sequence — complete these in order before writing any
framework code.** The goal is a working end-to-end baseline: Immersal localises
against a real wall, at least one test marker appears correctly positioned in AR,
and the mock provider works in the Editor. Every later Stage 1 task builds on this
baseline being confirmed and solid. If any of these steps is blocked, resolve it
before continuing — later features all depend on the foundation.
</MY NOTES TO CHECK LATTER:>


1. **Create the Unity project**: Unity Hub → New Project → select the
   **3D (Mobile)** template → name it `TileStories` → confirm **Unity 6.3 LTS** is
   the selected editor version (it is the current LTS, supported until December 2027;
   do not use the default or latest tech-stream version). Add the Unity `.gitignore`
   (use GitHub's standard `Unity.gitignore` template) and make the first Git commit
   before adding any packages.

2. **Create the folder structure** (§A) before installing packages — do it now while
   `Assets/` is empty so the boundary is established before anything else exists:
   ```
   Assets/
     Framework/
       Runtime/
       Editor/
     Apps/
       LivingRoom/  (for first implementations and test we will implement a simple living room map to test evrything here and then yes we'll mve to the "real walls")
         MediaAssets/
           Audio/
           Images/
           Models3D/
           Videos/
   ```
   This structure is the architectural boundary made physical and explicit from day
   one. Framework code goes in `Framework/`; wall-specific content goes in `Apps/`;
   these two trees never import from each other in the wrong direction.

3. **Install packages** (§11, step 4): in the Package Manager
   (Window → Package Manager), install in this order — AR Foundation, ARCore XR
   Plugin, ARKit XR Plugin, TextMesh Pro, then the Immersal SDK via
   "Add package from git URL." Confirm each package installs without console errors
   before adding the next.

4. **Configure XR Plug-in Management**: go to **Edit → Project Settings →
   XR Plug-in Management**. On the **Android** tab, tick **ARCore**. On the **iOS**
   tab, tick **ARKit**. These checkboxes must be ticked or the AR session will not
   start on device. This is a per-project setting that is easy to forget.

5. **Create the first wall scene** (§11, step 4b): follow step 4b exactly —
   `AR Session`, `XR Origin`, `ImmersalSDK` with developer token pasted in,
   `AR Space` with `XR Map` component (map ID + `.bytes` file assigned),
   `Localizer`. Save the scene at `Assets/Apps/Chafariz/ChafarizScene.unity`.
   Place your downloaded `map.bytes` file at `Assets/Apps/Chafariz/map.bytes`.

6. **Add a single test POI marker as a child of AR Space**: create an empty
   GameObject child of `AR Space` named `TestPOI`. Add a `Sphere` primitive as its
   child as a visible placeholder. Initially position `TestPOI` at `(0, 0, -1)` in
   local space (1 metre in front of the map origin, facing the camera). This sphere
   is purely a test marker — it will be replaced by `POIAnchor` prefabs later.

7. **Build to device and confirm localisation at the physical wall**: build the
   project to an Android or iOS test device (Build Settings → correct platform →
   Build and Run). At the physical wall, point the camera. When Immersal localises
   (the `AR Space` transform moves, the `Localizer` component fires its event), the
   sphere should appear overlaid on the wall surface at the map origin. If it does:
   the entire AR foundation is confirmed working. Log the time from camera-active to
   first lock — this is the Stage 1 baseline for the time-to-first-lock metric.
   If the sphere does not appear: check the `ImmersalSDK` token is filled in, the
   map ID matches the `.bytes` filename prefix, and the XR settings from step 4 are
   ticked.

8. **Write `MockLocalizationProvider.cs`** in
   `Assets/Framework/Runtime/Tracking/` — this is the first Framework-domain file
   in the project. It detects `#if UNITY_EDITOR` (or a
   `TileStoriesSettings.UseMockTracking` flag) and immediately fires
   `OnWallLocalised(Pose.identity)`, bypassing all Immersal calls. Add a simple
   WASD + mouse-look keyboard controller to the AR Camera so you can "walk" along a
   flat reference plane in the Game View. From this point on, the primary development
   loop is the Editor with the mock active — device builds are reserved for
   hardware-specific tests only.

9. **Confirm the mock works in the Editor**: press Play. The test sphere from step 6
   should appear (it is a child of `AR Space`, which the mock places at
   `Pose.identity`). WASD should let you move toward and away from it. This
   confirms `IWallTracker` events fire correctly through the mock path. The
   framework/app wiring is sound. Begin Stage 1 feature work.

---

**Tracking.** AR tracking bootstrap: Immersal + AR Foundation wrapper exposing
`OnWallLocalised(Pose)`. Already substantially proven end-to-end on a real device
(localising in 2–3 seconds against a Chafariz test map) — this stage hardens it into a
reusable component, not building from scratch. **Decide and freeze the tech stack
before writing further code** — the Unity+Immersal decision is already made and tested;
the lesson worth keeping regardless is that re-deciding a framework mid-project costs
3–4 weeks, so any *future* major tooling choice (e.g. which glTF loader, which backend)
should follow the same test-first, decide-once discipline already used for the
tracking-platform decision (documented in the thesis report §3.5).

**Immersal account, licensing tier, and installation specifics — verified during this
review.** Register a free Immersal Developer Portal account at
`developers.immersal.com`; this is separate from, and a prerequisite to, the Immersal
Mapper app used for on-site capture. The **free tier caps each map at 100 source
images** and, as of SDK v1.18+, requires a **"Powered by Immersal" logo displayed
throughout the AR experience** — this is not a vague or unspecified requirement:
Immersal provides ready-made logo templates directly in the SDK Samples repository,
under `Assets/ImmersalSDK/Samples/ImmersalLogos` — download these at the start of
Stage 1 and design the AR view's UI chrome around a small, fixed-size badge in a
known corner from day one, rather than discovering the obligation later and having to
retrofit screen layouts around it. On the commercial-use question specifically: the
Pricing page is the most specific and most recently worded of Immersal's own
documentation pages and states plainly that **"the free license does not support the
development of commercial projects"** — treat this as the operative statement rather
than the older, vaguer "you can use it even for commercial projects" wording that
still appears elsewhere on their site, and **send a short email to
sales@immersal.com early in Stage 1** asking explicitly whether a freely-distributed
academic thesis project (no monetisation, no commercial publisher) qualifies as
"commercial" for their purposes — a five-minute email closes a real ambiguity their
own documentation doesn't resolve, and it's worth having the answer in writing before
any store submission in Stage 6. This affects the budget (the Pro tier is $99/month
for 500 images/map and explicit commercial-use rights) regardless of which way the
answer comes back. On the 100-image cap — a realistic estimate, not a vague "might be enough":
the field-work protocol specifies 150–200 photos per wall at multiple distances.
A realistic per-wall breakdown: 4 zones × 3 distances × 8–12 images per position
= 96–144 images for an indoor straight wall; add another 30–40% for a curved outdoor
wall (Chafariz) or a wall with significant height variation (the 30m × 6m mural, which
may need vertical angles to cover its upper half). **The honest assessment is: the Pro
tier is almost certainly required for all three walls if the field-work protocol above
is followed.** The free tier might be enough for a single-zone, single-distance proof-
of-concept in Stage 1 Week 1, but the full mapping passes will almost certainly exceed
100 images per wall. Budget Pro from the start ($99/month, 500 images/map, explicit
commercial-use rights) rather than discovering the cap mid-session. Use Immersal's own
map-stitching feature as a mitigation if needed (select multiple maps in the Developer
Portal and combine them into one), or confirm Pro is active before mapping sessions
begin. Immersal's own published sizing guidance suggests roughly 100–120m² per
100-image map for indoor areas and 200–500m² for open outdoor areas — at full wall
dimensions and recommended coverage density, most of these walls will push against
or over the free-tier cap. Installation, in order: install AR Foundation,
ARCore XR Plugin, ARKit XR Plugin, and TextMesh Pro via the Package Manager first (the
Immersal SDK depends on all four); then add the Immersal SDK core itself via Package
Manager → "Add package from git URL," pointing at the `imdk-unity` repository (the
current distribution method, replacing the older `.unitypackage` import). Real-time
on-device mapping requires an Enterprise license and is **not** needed for this
project's design — all three walls are mapped offline ahead of time with the Immersal
Mapper app, then the resulting map files are embedded in the app for on-device
localisation, which is fully supported on the free tier regardless of the
commercial-use question above.

**The TileStories framework is completely tier-agnostic.** The `.bytes` map file
works identically regardless of which Immersal tier was used to create it — the
on-device localisation algorithm, the `XRMap` component, and every API surface
`ImmersalWallTracker.cs` calls behave exactly the same whether the map was built on
the free tier (100 images) or Pro (500 images). The tier distinction affects only
how the map was built: image count during capture (which determines coverage and
quality) and whether commercial distribution is legally permitted. The framework
receives a `.bytes` file and passes it to the SDK — it has no knowledge of what tier
produced that file, and no code path differs between tiers. The Pro recommendation
in this section is about map quality for full-wall production deployment and legal
clearance; it is not a framework runtime requirement. A developer holding only a
free-tier map can run the full framework and exercise every feature with no
code-path difference.

**Map optimisation and a formal map-testing protocol — verified directly against
Immersal's own documentation, not just "test it and see."** Two named, documented
parameters govern the size/accuracy trade-off when refining a captured map:
`featureCount`/`featureCountMax` (raising these captures more features, at the cost of
a larger map) and `trackerLengthMin` (lowering this shrinks the map but can measurably
reduce localisation success rate) [^w23]. Don't tune these blind — Immersal documents
an actual **Map Testing** workflow worth adopting as-is rather than improvising a
weaker version: after the initial capture, use the Mapper app's manual mode to shoot a
*separate* held-out set of test images from realistic visitor viewpoints and distances
(not the same images used to build the map), export that test set, and re-run
localisation against it every time the map is re-optimised — the export gives a
concrete localisation-success-rate number per attempt, so successive optimisation
passes are compared against each other empirically rather than by feel [^w23]. This
test-set methodology is itself worth keeping as a named artefact in the thesis's
technical evaluation (§3.5), not discarded once a "good enough" map is found — it's
the actual evidence that the chosen optimisation settings were deliberate. Capture
guidance confirmed directly from Immersal's own how-to-map documentation: aim for
roughly 50% image overlap between adjacent shots, and make sure the same feature
points are visible in at least three different images, since the Mapper app's
image-connection algorithm depends on that overlap to link captures into one
coherent map [^w23]. For the multi-map stitching approach mentioned in §1 for a wall
too long for one free-tier map: the SDK's localisation requests currently support
querying up to 32 map IDs at once (raised from an earlier limit of 8) [^w23], so
splitting a 40m wall into several stitched free-tier maps is comfortably within that
ceiling.

**Tooling and accounts.** Unity 6.3 LTS specifically (supported through December 2027
[^w18] — pin the version now rather than drifting onto whatever the Hub recommends
mid-project; this is also the version against which §2's MARS end-of-support date is
cited), GitHub repo, Firebase project (free tier), Apple
Developer (€99, recurring annually — note this on the budget line in §6, not just
once) and Google Play (€25, one-time) accounts registered early — approval can take
days, don't block on it later. Monorepo structure: `/Apps` (per-wall content, see §3
folder layout below), `/Framework` (the package being extracted), `/Docs`.

**Config schema v0.** Hand-write `config.json` for Panorama and Chafariz — no
validator yet, just enough structure to build against. Concrete shape to start from
(fields will grow through Stage 2, but get the skeleton right now):
```json
{
  "wall_id": "grande_panorama",
  "config_schema_version": "0.1",
  "geometry": { "type": "u_shaped", "length_m": 23 },
  "pois": [
    {
      "id": "castelo_sao_jorge",
      "name_pt": "Castelo de São Jorge",
      "name_en": "St. George's Castle",
      "category": ["military", "power"],
      "status_pct": 60, "has_status": true, "status_unknown": false,
      "wall_position": { "x_norm": 0.72, "y_norm": 0.45 },
      "summary_pt": "...",
      "content_by_profile": {
        "tourist": "...", "student": "...", "academic": "...", "child": "..."
      },
      "coordinates_today": { "lat": 38.7139, "lng": -9.1334 },
      "media": { "images": ["castelo_1.jpg"], "audio_pt": "castelo.mp3" },
      "quiz": [{ "q": "...", "options": ["A","B","C"], "correct": 0 }]
    }
  ]
}
```
Note `category` and `status` are free-form/wall-defined from day one, not fixed enums —
this is the direct, practical consequence of the third wall's finding in §1: a status
field that defaults to "required" will break on the mural wall, so it must be optional
in the schema from the very first draft, not patched in later.

**POI rendering — design decisions, with the reasoning, not just the result:**
- *Detail-card placement*: evaluated three options — floating card near the marker
  (immersive but unreadable with many POIs visible, occlusion risk), fixed bottom
  sheet (readable, standard pattern, loses spatial connection), side panel. **Decision:
  fixed bottom sheet for full content, plus a small floating label (~40px, name only)
  anchored to the marker for spatial identification** — this is the pattern Bloomberg
  Connects and Smartify both use, and it resolves the immersion-vs-readability
  trade-off rather than picking a side.
- *Marker shape*: colour-coded circles, not custom per-category icons, chosen
  specifically to avoid multiplying asset-creation work across every category; icons
  can be added later within the same block-rendering approach without changing the
  marker system. **Resolved in Stage 2.3**: icons implemented as an opt-in
  `IconLibrary` lookup by category string, sphere-only-circle remaining the default
  for any category without an authored icon — the "avoid multiplying asset work"
  intent is preserved because icon authoring is additive, never required. Stage 2.3
  also added `marker_style` (3 dev-selectable status renderers: gold ring / same-hue
  fade / corner badge) and `marker_shape` (5 base silhouettes) as wall-level,
  dev-selectable config — the single fixed "colour-coded circle" approach sketched
  here was one option among those three, not the only one. See
  `_2_2_Marker_Design.md` for the full design.
- **Use a single Immersal map per wall — Immersal handles tracking stability
  automatically and the framework should not try to re-implement what the SDK already
  does.** One well-captured map per wall (per the field-work capture protocol in §1)
  is the correct approach. The SDK's on-device VPS handles pose updates, confidence
  monitoring, and re-localisation automatically within a single map — the framework's
  job is to consume the pose the SDK produces, not to manage map zones. There is no
  `ZoneManager.cs` in this framework (it was removed from scope). If a wall is too
  large for a single map (the 40m Chafariz arc is the most likely candidate), use
  Immersal's own **map-stitching feature** in the Developer Portal to merge multiple
  capture sessions into one combined map before downloading. The output is still a
  single `.bytes` file. The framework and app code never know how many captures went
  into it. This is a content-pipeline concern (handled during the Immersal mapping
  session), not an app-code concern.
- **POI coordinate system — what Immersal actually does for you, and the one thing it
  doesn't.** Worth being precise here, since it's easy to either overestimate or
  underestimate how much of this Immersal solves on its own. **What Immersal handles
  completely, automatically, continuously, with no extra code needed**: once
  localised, Immersal/AR Foundation transforms the scene's **XR Space** (called
  "AR Space" in older SDK samples — the parent container holding the map, its point
  cloud, and everything placed relative to it) so that its contents visually line up
  with the real world as seen through the camera; the AR camera itself is *not*
  moved, the content around it is [^w29]. This means: **anything placed at a fixed
  position under that XR Space, in the map's own coordinate system, from then on
  automatically appears in the correct real-world location** — there is no
  per-frame alignment code to write, no drift-correction logic, nothing. This is
  exactly the part of "POI positioning" that genuinely is already solved by the SDK,
  and it's worth saying so plainly rather than re-solving it.
  **What Immersal has no concept of at all, and therefore cannot do for you**: it
  doesn't know what a "POI" is, doesn't know about `x_norm`/`y_norm`, and doesn't know
  this wall has 150 named points of interest — it only ever answers "where is the
  camera, right now, in the map's coordinate system?" Converting an author-friendly
  `x_norm`/`y_norm` value (written by someone looking at a reference photo, with no
  Unity or Immersal knowledge required) into an actual 3D position under the XR Space
  is still this framework's job, not the SDK's. Two ways to do that conversion, and
  the right call is to use both, for different POIs:
  - **Direct field capture (the recommended primary method, and not a custom build —
    Immersal already ships this).** The SDK's own official "Content Placement
    Sample" includes a `ContentStorageManager` that does exactly this: while
    localised and standing at/near a real location, tap a button, and it records the
    current position under the XR Space, persisting it locally across app restarts
    [^w29]. Adapt this directly rather than building an equivalent tool from
    scratch: during the same field session used for Immersal mapping (§1), walk up
    to each POI that matters — at minimum the ten hero POIs per wall, ideally as many
    of the scale-proving ones as the session has time for — and capture its real
    position the same way. This needs no coordinate math, no interpolation, and has
    no curvature problem on the Panorama's U-shape or Chafariz's circle, because
    there's no formula involved at all — the position is simply wherever you
    physically stood. Store the result as a literal 3D position field on that POI
    (e.g. `captured_position: {x, y, z}`), separate from `x_norm`/`y_norm` (which stay
    useful as the human-readable authoring value content writers reason about before
    any field visit happens).
  - **Interpolation from a handful of calibration anchors (a fallback for content
    authored remotely, not yet field-captured — and never the primary method for
    anything a visitor will actually look at closely).** Place a small number of
    calibration anchors using the same capture method above — at minimum one at each
    end of a flat wall (the mural), and **one at each panel join** for the Panorama's
    U-shape and at several points around the arc for Chafariz's circle, not just two
    anchors at the far ends — a naive two-point linear interpolation across a curved
    or multi-panel wall would place interpolated POIs *inside* the wall or floating in
    front of it wherever the surface actually bends, since it would be treating a
    curved path as a straight line between its endpoints. With anchors at every
    panel join (the same join locations already measured during the field-work
    protocol's wall-geometry pass, §1), interpolate piecewise *within* each straight
    segment between consecutive anchors, never across a bend. This fallback exists so
    content drafting can happen away from the physical wall (most of Stage 1–2's work)
    without blocking on a site visit for every placeholder POI — but it is explicitly
    a secondary, lower-accuracy path, used for the bulk of the "scale-proving" POIs
    where approximate placement is acceptable, not for any POI a visitor will tap on
    and expect to be precisely positioned.
  Either way, the result — a real 3D position under the XR Space — is what actually
  gets projected to screen space at render time (still tested explicitly at 1 m, 3 m,
  and 8 m viewing distance, the core technical claim for RQ1). Once Stage 4's baker
  exists, this captured or interpolated position becomes just another field baked
  into the `WallConfigAsset`, with no runtime cost either way.
- **Marker rendering specifics**: small circle, category colour, name label
  truncated at ~15 characters to avoid overlap; when two markers are within ~40px on
  screen, apply a vertical offset rather than letting them overlap.
- **Tap behaviour**: opens the bottom sheet with name, a 2-sentence summary (tourist
  register), status badge, one thumbnail image, and a "See on Map" button. Cap the
  sheet at ≤40% of screen height so the wall stays visible behind it. Dismiss via
  swipe-down or tap-elsewhere-in-AR-view — both, not just one.
- **AR-failure fallback**: a searchable, non-AR "All Buildings" list view (name,
  category icon, status), where tapping an entry returns to AR view with that POI
  highlighted. This is not optional polish — it is the fallback path for when tracking
  simply doesn't lock, and should exist from Stage 1, not retrofitted later.
- **Persistent navigation**: AR view / Map / List / Profile, always visible.
- **Onboarding, minimal version for this stage**: one screen ("Point your camera at
  the tile wall to begin") plus the camera-permission request, handled per-platform
  (iOS requests at runtime; Android needs both a manifest entry and a runtime
  request) — test both explicitly, the two platforms fail differently if this is wrong.
  **Include a functional (not yet polished) "scanning" state from this very first
  build, not as a Stage 7 afterthought**: Immersal's VPS-based localisation takes a
  visible 1–3 seconds to lock once the camera is pointed at the wall — unlike
  single-image AR, which locks near-instantly, there is a real waiting period here
  that needs *some* on-screen acknowledgement (even a plain "Looking for the wall…"
  label is enough for Stage 1) or visitors will assume the app is broken. Log the
  elapsed time from camera-active to first successful lock as its own metric from
  Stage 1 onward — this becomes both a piece of the thesis's technical evaluation and
  the baseline Stage 7's polished version (the animated scan-line and celebratory
  "Found! 🎉" moment, §3 below) is measured as an improvement against, rather than a
  number invented retroactively at evaluation time. Add a one-line analytics-consent
  notice here too ("This app logs anonymous usage
  data to improve the experience — no personal information is collected"), even
  though no events are wired until Stage 3: this is the natural place for it
  structurally, and the thesis's own ethics section (§subsec:pw-ethics) already
  commits to exactly this disclosure. Wire the toggle to Firebase's actual
  `Firebase.Analytics.FirebaseAnalytics.SetConsent(...)` call (Firebase Unity SDK
  v13.x as of this review, June 2026) rather than treating consent as a cosmetic
  notice with no functional effect — Firebase's own GDPR-consent mechanism exists
  precisely for this, and using it now means Stage 3's analytics wiring has
  something real to respect from day one. Retrofitting consent UI after analytics
  already exist risks the same "too late to add" problem flagged for telemetry in
  §7's risk register.
- **Accessibility, minimal check for this stage**: confirm that whichever UI system
  renders the bottom sheet and labels exposes accessible text to TalkBack/VoiceOver
  from this very first screen, even though the full navigation pass waits until
  Stage 7 (§3, below). The thesis report commits to screen-reader support "planned
  from Phase 0" (`chapter-proposed-solution.tex`, §subsec:pw-core-concept); a
  five-minute smoke test now is what makes that sentence true on the day it's
  written, rather than leaving it unverified for six months.
- **Map integration — verified approach.** Google's own "Maps SDK for Unity" is
  deprecated and should not be used. Two genuinely different needs, two genuinely
  different mechanisms: (1) "Open in Maps app" needs no API key at all — build a
  `geo:` URI (Android) or an `https://maps.apple.com/?q=...` / Google Maps web URL
  (iOS) from the POI's `coordinates_today` and hand it to the OS via
  `Application.OpenURL`. (2) An inset *thumbnail* map inside the bottom sheet, if
  wanted, is not an embedded interactive map (Unity has no first-party plugin for
  that) but a single static image: call the Maps Static API
  (`https://maps.googleapis.com/maps/api/staticmap?...`) with a `UnityWebRequest`,
  display the returned PNG on a `RawImage`, and cache it locally since the POI's
  location never changes between sessions. This needs a Maps Static API key (free
  tier sufficient at this scale; the €20 billing alert mentioned below is the
  safety net, not an expected cost) — enable specifically the *Maps Static API* in
  the Google Cloud Console, not the general "Maps SDK," when creating the key.

**Testing for this stage — and a three-tier testing discipline that applies to every
stage thereafter, because "build and run to a real device" as the only testing loop
is the single largest productivity bottleneck in Unity AR development:**

**Tier 1 — MockLocalizationProvider (90% of development time, most iterations).**
Write `MockLocalizationProvider.cs` in `Framework/Runtime/Tracking/` at the very
start of Stage 1, before any other feature, because every feature above it depends
on localisation being "done." The mock detects `#if UNITY_EDITOR` (or a
`TileStoriesSettings.UseMockTracking` flag also readable at runtime for debug builds)
and immediately simulates a successful Immersal localisation by firing
`OnWallLocalised(Pose.identity)` with a configurable position offset, then
instantiating POIs relative to a flat reference plane in the scene rather than a
live Immersal map. Add a simple WASD+mouse-look keyboard controller on the Main
Camera so you can "walk" along the virtual 30-metre wall inside the Unity Editor
Game View to test LOD threshold transitions, lapse epoch switches, circuit
progression, and wayfinding arrow behaviour — all without building an APK, without
connecting a device, and without visiting a physical wall. A single USB deployment
cycle takes 3–5 minutes; a mock-based iteration takes 5 seconds. At 10 iterations
per feature, that is 50 minutes vs. ~8 seconds — across a 7-stage project, this
single file probably saves 30+ hours of waiting. The mock is **not** a simulation
of the AR camera — it bypasses Immersal's point-cloud matching entirely and
substitutes a known-good fixed pose, which is exactly what is wanted for testing
everything *above* the tracking layer. Test the actual tracking layer (Immersal) only
when there is something specifically tracking-layer-related to test, on a real device,
at a real wall — which should be a minority of all testing events, not the default.

**Tier 2 — Unity XR Simulation (AR Foundation plane detection, image tracking, device
movement without building to a device).** Enable it via `Project Settings → XR Plugin
Management → Simulation tab` — it is already part of AR Foundation and requires no
extra package install [^w35a]. Use the pre-built sample environments (`Window → XR →
AR Foundation → XR Environment → Install sample environments`) for testing
AR-Foundation-level features (device orientation, basic tracking, scene-level AR
session lifecycle) without building an APK. This does *not* simulate Immersal VPS
localisation — that is what Tier 1's mock is for. XR Simulation tests the AR session
plumbing; the mock tests the application logic above it. Use both, for different things.

**Tier 3 — USB Build and Run (~10% of development time).** Reserve for: verifying
device-specific hardware integrations (Bluetooth audio, camera permission flows,
storage access for telemetry JSONs), thermal degradation profiles over a 20-minute
session window, and final pre-evaluation validation at actual wall sites. Set up
TestFlight (iOS) and the Google Play Internal Testing track (Android) at the start of
Stage 1 (§3 Stage 1, below) so Tier-3 test builds are distributed to external testers
without ad-hoc APK/IPA sharing from the very beginning.

**Stage 1 specific tests (Tier 3 only — the items below require a real device):**
- Minimum 3-device matrix: one iOS (12 or newer), one Android flagship, one Android
  mid-range. Note explicitly that some budget/battery-optimised Android phones (e.g.
  certain Xiaomi models) ship without ARCore support at all — check this early, not
  after a test session fails mysteriously.
- Museum/site-condition AR testing: recognition time under 5 seconds is the target;
  test drift over a 5-minute session, **specifically logged at 1m, 3m, and 8m viewing
  distance** rather than as one undifferentiated number — drift behaves differently
  close to the wall than far from it, and averaging the two hides exactly the
  distance-dependent failure mode worth knowing about; test re-acquisition time after
  the phone is lowered and raised again.
- **Occlusion-specific re-localisation test, distinct from the lowered-phone test
  above**: have a second person walk through the camera's view for about five
  seconds while the app is actively tracking, then time how long the app takes to
  recover lock once they've passed. This simulates a real, common museum-visitor
  scenario (someone else walking between the visitor and the wall) that the
  simple lower-and-raise test doesn't cover, and the recovery time is worth knowing
  before, not during, a formal evaluation session.
- Log tracking confidence in a debug build: every zone switch, every recognition
  failure, every re-acquisition event. This data becomes direct evidence for the
  thesis's own §3.5 technical evaluation — instrumenting it now is free; reconstructing
  it later from memory is not possible at all.
- Informal usability check: 5 people unfamiliar with the app, told only "use the app,"
  observed without help; note the first point of confusion. Five participants is
  enough to surface most usability problems in a single round (the same principle
  already cited in the thesis's own evaluation methodology, §3.3). Specifically
  include at least one person who has never used any AR application before in this
  group — their reaction to the scanning state and first marker lock is the
  closest available baseline for an average museum visitor's experience, and a
  group of all AR-experienced testers will systematically miss confusion points an
  AR-naive visitor would hit.
- **Distribute test builds via TestFlight (iOS) and the Google Play Internal Testing
  track (Android)** rather than ad-hoc APK/IPA sharing — both are free, both handle
  installation friction for external testers automatically, and both stay useful all
  the way through Stage 4's wider 5–10 person test rounds (§3, below), so setting
  them up now in Stage 1 pays for itself repeatedly rather than being a one-off chore.

**Stage 1 exit criteria (MVP v0.1):** AR recognises the wall and shows 10 markers; tap
shows a card; the map link works; the app runs fully offline; sustained ≥25fps on an
iPhone-12-equivalent device.

---

### Stage 2 — POI-level features: the block system, one block at a time (Weeks 4–10)

**Internal sequencing within Stage 2 — critical constraint.** Seven weeks is realistic
for the nine items below, but ONLY if they are executed in a specific order. An agent
that works through items 1→9 numerically will discover in Week 9 that items 6 (lapse-
state) and 7 (LOD) are prerequisites for items 4 (video) and 5 (3D model) *looking
right at scale* — and will have to go back. The correct grouping is three
mini-milestones:

**Stage 2A — Weeks 4–5: scaffold + text + image + map + profile-variant**
Items 1, 2, 8, 9 from the list below. These establish the Vertical Layout Group /
Content Size Fitter / Scroll Rect rendering scaffold that everything else depends on.
They have no external dependencies, no async-loading risk, no platform-specific
behaviour. Profile-variant resolution (item 9) belongs here because it is a few lines
of logic inside already-built renderers, not a new block type. Get these working,
cross-wall-proven, and registered before touching anything with an external service or
an asset pipeline.

**Stage 2B — Week 6: LOD/marker-density + lapse-state**
Items 7 and 6 from the list below. Both are cross-cutting and must be built BEFORE
video/3D blocks, because: (a) the scale-proving requirement (100+ markers) requires the
LOD system to already exist — 100+ un-LOD'd markers will degrade performance before the
block content even loads; (b) a 3D model block may need lapse_state-based visibility
(show a pre-1755 reconstruction only in the `pre_1755` epoch), which cannot be tested
without a working lapse mechanism. Lapse-state must also be proven against all three
walls in Stage 2B specifically — including the mural's `dawn/day/dusk/night` axis —
before Stage 2C begins.

**Stage 2C — Weeks 7–10: audio + video + 3D model**
Items 3, 4, 5 from the list below. These are the most implementation-intensive and have
the most external dependencies: TTS pipeline and Android Bluetooth audio risk (audio);
asset-format concerns and lazy-loading (video and 3D). Build them last so the rendering
scaffold, LOD, and lapse-state are already stable and tested — these blocks add content
to a working system rather than simultaneously debugging foundational systems. Audio
first (3), then video (4), then 3D model (5) — each is simpler than the next.

**Architectural distinction that must be right from the start:** of the nine items
below, only six are genuine `TileStoriesUIBlock` registry entries (text/header, image,
audio, video, 3D model, map — items 1, 2, 3, 4, 5, 8). The other three are
cross-cutting mechanisms that act ON blocks: lapse-state (item 6) is a POI-level
state property that gates visibility; LOD (item 7) is a scene-wide rendering rule
across all markers; profile-variant (item 9) is resolver logic inside each existing
block's renderer. Forcing all nine into one registry is the wrong abstraction. Keep a
`TileStoriesUIBlock` base class with exactly six concrete subclasses, plus three
separate, smaller systems. Expose the registration call as public API so downstream
projects can add new block types without forking the package:
`TileStoriesBlockRegistry.Register(string typeKey, Func<UIBlockData, TileStoriesUIBlock> factory)`.

Each item in all three mini-milestones follows the same micro-cycle: sketch →
hard-code one working instance against Panorama → generalise into a registered type →
prove it runs unmodified against Chafariz → prove it runs against the mural for any
block that references category/status fields.

**An architectural precision worth getting right from the start, not discovered
mid-implementation**: of the nine items below, only **six are genuine
`TileStoriesUIBlock` registry entries** — text/header, image, audio, video, 3D model,
and map (items 1, 2, 3, 4, 5, and 8). The other three are **cross-cutting mechanisms
that act on blocks rather than being blocks themselves**, and implementing them as
registry entries would be a real design mistake, not just a style choice: "lapse"
(item 6) is a **POI-level state property** (`lapse_states` in §10's schema) that
changes *which* blocks and markers are currently visible for a given epoch — it
doesn't render its own content, so it has nothing to register. Level-of-detail (item
7) is a **scene-wide rendering rule** applied across every marker regardless of block
type, not a per-POI feature at all. Profile-variant content (item 9) is **resolution
logic inside each existing block's renderer** (reading `content_by_profile` and
picking the active profile's variant), not a seventh block type competing with the
other six. Keep this distinction explicit in the code (a `TileStoriesUIBlock` base
class with exactly six concrete subclasses, plus separate, smaller systems for
epoch-gating, LOD, and profile-resolution) rather than forcing all nine into one
registry for the sake of a tidy "nine block types" story — the tidy story is less
correct than the slightly-less-tidy truth, and an implementer who builds a
`LapseBlock : TileStoriesUIBlock` class the same shape as `AudioBlock` will have
built the wrong abstraction.

**Make the block registry itself a public extension point, not just an internal
organising pattern.** Expose the registration call (e.g.
`TileStoriesBlockRegistry.Register(string typeKey, Func<UIBlockData, TileStoriesUIBlock> factory)`)
as public API on the framework's `Runtime` assembly, specifically so a downstream
project can add a genuinely new block type (a future `"quiz"` or `"video_360"`, say)
**without forking the package** — registering a new factory function against a new
`"type"` string from their own code. This is the detail that actually makes the
architecture "extensible" in the sense the thesis claims, rather than merely
"organised internally": an Open/Closed-principle win that exists on paper is only
real once a project outside this one could exercise it without touching this
codebase's source.

1. **Text/header block** — name, short description. Build first; establishes the
   rendering scaffold (Vertical Layout Group + Content Size Fitter + Scroll Rect)
   every later block reuses.
2. **Image block** — single image, then a swipe/gallery variant.
3. **Audio block** — the scripted, always-on audio guide layer. Decisions to carry
   forward exactly: **Google Cloud TTS for the working version**, targeting PT-PT (not
   PT-BR) for a Lisbon heritage app. **Verified during this review, with a caveat to
   re-check**: Google's published voice lists show `pt-BR` consistently ahead of
   `pt-PT` in tier availability — the Chirp 3: HD rollout's published 31-language list
   includes `pt-BR` but not `pt-PT`, and documented Neural2 voice-map examples show
   only `pt-BR-Neural2-A/B`. This suggests `pt-PT` may currently sit at WaveNet/Standard
   tier rather than Neural2, but voice rollouts change monthly — **call
   `list_voices(language_code="pt-PT")` directly at implementation time and read the
   actual result** rather than trusting this note or the original "Neural2, PT-PT"
   instruction unverified. If no PT-PT Neural2 (or newer) voice exists, use the best
   available PT-PT tier (WaveNet over Standard) — do **not** substitute a `pt-BR`
   voice for a Lisbon-set app to chase a higher tier, since the Brazilian accent is a
   real mismatch for this content, not a cosmetic one. **English has no equivalent
   problem** — `en-GB` and `en-US` both appear in the current Chirp 3: HD list and have
   long-established Neural2 coverage — so once PT-PT content exists, English narration
   has a real choice of high-tier voices; lean towards a British-English voice
   (`en-GB-Neural2-*`, confirm the exact current voice ID via `list_voices()` rather
   than hardcoding one that may have been renamed) over a US accent, since it reads as
   the more natural register for European-tourist-facing content, and **pick one
   accent and stay consistent across every clip** — switching accents between clips
   reads as careless even if each individual clip is fine. Workflow order: **write and
   record the PT-PT script first, as the reference text**, then translate to English
   from that reference — translating into PT-PT from an English draft risks losing
   register and idiom that matter for a heritage-narration script. Pricing as last verified: Neural2
   at $16/million characters, WaveNet at $4/million characters, with a free allowance
   of 4M characters/month (Standard) or 1M characters/month (WaveNet) — at the ~12
   clips scoped below, this project will not come close to either free-tier ceiling
   regardless of which voice tier ends up available for PT-PT. A paid human voice actor
   (~€250 flat via a freelance platform) is a *possible* later upgrade, not a Stage-2
   requirement — a thesis evaluation does not need broadcast-quality narration to be valid. Script before generating audio, not
   the reverse. Target durations: ~90 seconds for a general intro narration, 45–60
   seconds per hero POI (written once per visitor profile register), 20–30 seconds
   per timeline-transition clip. **Do not attempt audio for all POIs in this stage** —
   ten heroes plus the intro is already 12+ clips to script, generate, and integrate
   correctly. Playback behaviour: auto-plays the intro narration on first wall lock;
   tapping a POI plays that POI's clip with a visible progress bar and pause/resume;
   audio continues playing if the card is closed (the visitor keeps exploring while
   listening); a volume control sits in the screen's top-right corner, reachable
   without opening any card, since some visitors will want silence in a shared
   space; a new tap on a different POI either fades out the current clip (500ms)
   and plays the new one, or queues it, per a user-facing setting — never play two
   clips simultaneously. **Audio interruption handling — two genuinely different
   triggers, needing two genuinely different Unity callbacks, not one.** Mobile
   operating systems interrupt audio aggressively, and a visitor who misses a key
   narrative beat because of a phone call or a disconnected earbud will reasonably
   blame the app, not their phone. (1) **App backgrounding** (a phone call, switching
   apps, the device locking) is the `OnApplicationPause(bool)` / `OnApplicationFocus
   (bool)` case — pause the narration explicitly here and resume from the same
   playback position on return, rather than assuming the platform's default behaviour
   is sufficient. (2) **Audio output device changes** (Bluetooth earbuds connecting or
   disconnecting mid-narration) are a *separate* case these two callbacks do **not**
   cover — that needs `AudioSettings.OnAudioConfigurationChanged`, Unity's own
   documented mechanism for this, which fires with `deviceWasChanged=true` and
   requires reloading audio objects and explicitly restarting playback state, since
   both are lost when the underlying audio engine re-initialises [^w34]. **A real,
   currently-open platform caveat worth testing for explicitly rather than assuming
   away**: Unity's own issue tracker confirms Android can lose all currently-playing
   audio when a Bluetooth device connects or disconnects, and multiple developer
   reports describe `OnAudioConfigurationChanged` not firing reliably on Android in
   practice even though it is the documented fix [^w34] — treat Bluetooth
   interruption recovery as something to physically test with real wireless
   headphones on the actual Android test devices in Stage 1's device matrix, not
   something the textbook callback can be assumed to solve correctly out of the box.
   **Concrete Android fallback, if testing shows `OnAudioConfigurationChanged`
   doesn't fire reliably**: add a small polling coroutine (every 1–2 seconds while
   audio is actively playing) that queries Android's `AudioManager` directly via
   `AndroidJavaObject` — specifically `audioManager.Call<bool>("isBluetoothA2dpOn")`
   transitioning from `true` to `false` signals a Bluetooth disconnect that Unity's
   callback missed. This is not a hypothetical — it's a known, documented gap in
   Unity's Android audio lifecycle wrapper that `AndroidJavaObject` accesses
   Android's own Java API to work around, the same mechanism already used for
   audio focus management in other Unity-on-Android contexts [^w34]. Keep this polling
   fallback gated by `#if UNITY_ANDROID` and behind a `FeatureFlags` flag so it
   can be disabled easily if the primary callback starts working in a future Unity
   patch, rather than ending up with two competing mechanisms both firing.
   Captions/subtitles: a timestamped caption file per clip,
   shown in sync with audio (accessibility and noisy-environment use). Localisation
   (PT/EN at minimum) is built into this block's data shape from the start, per the
   Yarn-Spinner-derived principle in §2 — do not add a language dimension later as a
   retrofit.
4. **Video block.**
5. **3D model block** — loads a referenced `.glb` via glTFast; this is also where
   pooling/lazy-loading discipline gets proven for the first time, since 3D models are
   the heaviest asset type per POI.
6. **"Lapse" state mechanism (the four-epoch timeline for the Panorama) — a POI-level
   property, not a seventh `TileStoriesUIBlock`, per the architectural note above.**
   Implemented
   generically enough that "lapse" need not mean historical time specifically.
   Concrete reference implementation for the Panorama: four states (pre-1755,
   earthquake, Pombaline reconstruction, today), a slider UI placed at the bottom of
   the screen above the navigation bar (tested against a collapsible top panel and
   rejected — a top panel gets covered by open info cards), 150ms transition
   animation between states, and distinct visual treatment per state: pre-1755 shows
   all markers in default colour; the earthquake state turns destroyed-status markers
   red with a brief shake/debris effect; the Pombaline state greys out destroyed
   markers, keeps survivors coloured, and introduces newly-built markers; the "today"
   state is, for this stage, a simplified pre-rendered comparison image rather than a
   live satellite overlay — a real-time map overlay is materially more complex and
   should not block this stage's completion. Marker visibility should change per
   selected epoch (not every POI exists in every era) — this is itself a form of the
   level-of-detail discipline below, just driven by time rather than distance.
   **Introduce the mural wall here**, where the natural "lapse" dimension is not
   historical time at all but something like season or time-of-day-the-animal-is-active
   — proving the mechanism generalises to a non-historical "criteria lapse" rather than
   being secretly hardcoded to mean "year." Show a short label under the slider at
   each position, naming the era concretely rather than leaving the visual state to
   speak for itself — for the Panorama: "Lisboa, ~1700" / "Terramoto, 1755" /
   "Reconstrução, 1760–1800" / "Lisboa Hoje."
7. **Level-of-detail / marker-density management** — with concrete thresholds, not
   just a principle: beyond roughly 5 metres from the wall, show only the
   **5 highest-priority markers per visible zone** plus a numbered cluster indicator
   for the rest (tapping it expands to individual markers); between 2–5 metres, widen
   that to roughly **15 markers** with smaller labels; under 2 metres, show every
   marker in the current viewport with full labels. These two numbers (5 and 15) are a
   starting point to tune against the real marker density once a wall's hero-plus-scale
   POIs are loaded, not a hard requirement — but pick concrete numbers now rather than
   a vague "fewer markers far away," since "fewer" with no number is not testable.
   **Never render all 100+ markers at once regardless of distance** — this
   is the single most load-bearing legibility decision in the whole project, directly
   tied to RQ2 and to the information-overload finding already established in the
   thesis's own literature review (Wang et al., among others, on cognitive overload in
   museum AR).
8. **Map block** — "See where it is today," linking each POI to its present-day
   location, building on Stage 1's map integration but now as a registered, reusable
   block rather than a one-off feature.
9. **Profile-variant content within blocks** — `"content_by_profile"` keys
   (tourist/student/academic/child) resolved by the existing block renderers, not new
   block types. Content taxonomy for status, concretely, for the Panorama: four
   *narrative* categories a content author reasons in — unchanged, modified
   (rebuilt/altered), destroyed, unknown. These map onto Stage 2.3's rendering schema
   as: unknown → `status_unknown: true`; the other three are narrative shorthand for
   a `status_pct` the author picks within that category's range (e.g. "modified"
   isn't one fixed number — a content writer can place it anywhere that reads right
   for that specific building, `status_pct` is the finer-grained field, the category
   name is how they think about it while writing). Closes out the
   POI-level work for this stage.

By the end of Stage 2: all three walls have a working set of fully-functional POIs
each (content still placeholder/light beyond the hero set), every block type proven
against at least two structurally different walls, and the mural wall specifically
proving the taxonomy-must-be-wall-defined finding in practice, not just in design intent.
**Stage 2 exit criteria, concretely:** all 6 registered block types (text/header,
image, audio, video, 3D model, map) and all 3 cross-cutting mechanisms (lapse-state
gating, LOD, profile-content resolution) above are implemented; each block type has
been run, unmodified, against at least two of the three walls;
hero-POI counts are met for all three walls per §1's note above; and the LOD
thresholds (5 m / 2 m breakpoints) have been tested at those distances on at least one
real device per platform, not just in the Editor.

---

### Stage 3 — Scene-level features (Weeks 11–16)

1. **Visitor profile onboarding** — a short, two-screen flow. Screen 1: "What brings
   you here today?" with four options (Tourist / Student / Historian / Family).
   Screen 2 (optional, skippable): "What interests you most?" Skipping is mandatory
   to support, not a nice-to-have — defaults to the Tourist profile on skip. Test the
   completion time explicitly; target under 30 seconds, since forcing a longer
   onboarding measurably reduces completion in comparable apps. Persist the chosen
   profile locally; surface it as a small, always-visible badge in the navigation
   bar; make "change profile" reachable directly from any POI card, not buried inside
   a settings menu.
2. **Circuits / progression state machine** — named per Ashwell's taxonomy (§2);
   start with **Gauntlet** and **Hub-and-Spoke** only; Node Graph is deferred unless
   time allows, given its higher validation burden (cycle detection, unreachable-node
   checks). **Implement this as one explicit, centralised state machine — not as
   implicit flags scattered across individual POIs.** This is the specific failure
   mode the dialogue-systems research this framework already borrows from warns about
   directly: branching/progression logic that scales linearly in authoring effort but
   *exponentially* in bugs once it isn't centralised [^w10]. Concretely: each circuit's
   config declares a `circuit_type` (one of the five taxonomy values above), a
   `poi_sequence` target list, and a `completion_rule`; the **state machine itself —
   not individual POIs — owns an `IsInteractive` flag per target**, which is what
   makes "later POIs in a sequential circuit stay inert until the visitor reaches
   them" trivial to implement correctly once, centrally, rather than re-implemented
   (and re-bugged) per circuit. This locking behaviour is itself circuit-type-specific,
   not universal: it's the right call for **Gauntlet** and **Branch-and-Bottleneck**
   (sequential types, where order is the point), actively wrong for **Hub-and-Spoke**
   and **Parallel Tracks** (explicitly any-order-allowed by the taxonomy's own
   definition, §2) — the state machine's locking behaviour should be a property of
   `circuit_type`, not a separate setting an author could mismatch against the type
   they picked.

   **A flexible starting point — the problem, why it's real, and how to solve it
   without over-building.** As designed above, a sequential circuit (Gauntlet,
   Branch-and-Bottleneck) has an unstated assumption: that the visitor begins at
   `poi_sequence[0]`. On a 23-metre continuous wall, that assumption is simply false
   in practice — a visitor can physically be standing in front of stop 5 of 8 when
   they choose to start the "Earthquake 1755" circuit, because that's wherever they
   happened to walk up to the wall. Forcing the system to treat stop 1 as "current"
   in that situation means the wayfinding arrow (item 3, below) points back along the
   wall at content the visitor is already standing next to — the AR equivalent of a
   book that insists on starting at page one even when the reader is already holding
   it open at page forty. Put plainly: **the system currently has an implicit
   entry-point bias — a fixed assumption (index 0) that prevents dropping a visitor
   into the middle of a sequence — and this needs to be decoupled from where the
   visitor physically is when they start.**

   Three architectural patterns are worth weighing for this, each with a real
   precedent, and the right choice here is *not* "pick one and apply it everywhere" —
   each fits a different part of this framework's actual circuit types:
   - **A pure adjacency-list graph** (every POI declares its own list of valid
     neighbours; a circuit is just whichever subgraph of nodes the author wires up;
     "Gauntlet" becomes nodes with exactly one forward neighbour, "Hub-and-Spoke"
     becomes one node connected to many that connect back to it) is the most general
     and mathematically clean solution — but it's the wrong default *here*, because
     it would mean abandoning the Ashwell-taxonomy-as-`circuit_type` decision already
     made in §2 specifically for its legibility, and it would inherit Node Graph's own
     validation burden (cycle detection, reachability checks) for circuit types that
     are structurally simple by design. **This pattern is exactly right, however, for
     the already-deferred Node Graph circuit type specifically** — when that type is
     eventually built, an explicit per-node neighbour list *is* its natural, correct
     representation, not a retrofit. Reserve it there; don't generalise everything to
     graphs now to solve a narrower problem.
   - **An entry tag plus a per-type traversal rule** (keep the named circuit
     structure; let the caller specify which target to start at; a traversal rule —
     forward-only, bidirectional, radial — determines what unlocks from that point)
     is the right fit for Gauntlet and Branch-and-Bottleneck, and it is *not* a novel
     risk: it is exactly what the two narrative engines already cited as this
     framework's direct architectural precedent (§2) do in production. Yarn Spinner's
     `DialogueRunner.StartDialogue(nodeName)` takes any named node as the starting
     point — "Start" is merely its *default* parameter value, not a hardcoded
     requirement [^w24]. Ink (inkle's narrative scripting language, shipped in *80
     Days*, *Heaven's Vault*, *Pendragon*, and *A Highland Song*) addresses every
     knot and stitch individually (`-> the_orient_express.in_third_class`) and lets
     calling code divert to any of them; one published account of using Ink for a
     conversation-driven game describes exactly this pattern — "each conversation...
     is identified by one knot... when you start an event, the game will set the Ink
     runtime's 'program counter' to this location" [^w25]. Both engines treat "where
     do we begin" as something the *caller* decides per-invocation, never as a fixed
     property of the content itself — which is precisely the decoupling this problem
     needs.
   - **A state/prerequisite evaluation matrix** (POIs unlock based on evaluated
     conditions — `StoryStage == 2`, `HasVisitedHub == true` — rather than direct
     graph edges) is more machinery than this specific problem needs, and building it
     from scratch would duplicate something this framework **already has**: the
     badge system's `trigger.type` dispatcher (§10's schema —
     `first_visit_of_category`, `first_visit_of_status`, `first_circuit_completed`,
     `poi_count_threshold`) *is* a small, named-condition evaluation engine. Don't
     build a second, parallel one for circuit-unlocking. If a genuinely
     prerequisite-shaped need shows up later (e.g. "this spoke unlocks only after the
     visitor passes the Hub's quiz" — a real condition, not a spatial one), extend
     the existing badge-trigger dispatcher with a new `trigger.type` rather than
     inventing a third mechanism. For the entry-point problem itself, this pattern is
     not the tool to reach for.

   **The recommended mechanism, concretely — and it needs less new machinery than it
   might look like, because the data it needs already exists.** Every POI already
   carries a `wall_position` (`x_norm`/`y_norm`, §10's schema) for marker rendering —
   or, where a field visit has already captured it, a more accurate
   `captured_position` (§3, Stage 1's coordinate-system note); use whichever is
   present per POI, following the same precedence rule given in §10. When a circuit
   starts, **do not default to `poi_sequence[0]`.** Instead, find
   whichever member of that circuit's `poi_sequence` has the position nearest
   to the visitor's current position, and initialise the state machine's
   `currentIndex` there — a nearest-neighbour search over an array of 6–8 items,
   not a pathfinding problem. This is not a new subsystem: it is the *same*
   nearest-POI computation already specified for free-roam wayfinding outside any
   circuit (item 3, below — "a simpler general-purpose version of the same arrow
   points toward the nearest not-yet-visited POI"), simply scoped down to membership
   in one circuit's `poi_sequence` at the moment that circuit is selected. A directly
   on-point precedent for this exact design exists in the museum-guide research
   literature: the CHIP project's Rijksmuseum tour guide implemented "a real-time
   routing system... for providing personalized tours tailored to the user position
   inside the museum," re-routing dynamically as the visitor's actual location changed
   rather than assuming a fixed starting room [^w26] — this framework's wall-position
   data plays the same role their spatial museum-room data played for them, just
   simpler, since a continuous wall is a one-dimensional case of the same problem.
   Two changes make this actually work, not just point the arrow differently:
   - **Add an `entry_point_strategy` field** per circuit — two values only:
     `nearest_flexible` (the default, recommended for most circuits: resolves the
     starting index from the nearest POI to the visitor's position, marks earlier
     stops as skippable/catch-up, uses `traversal_rule: "bidirectional"` to unlock
     both directions) and `ordered_strict` (always forces `currentIndex = 0` and
     uses the wayfinding arrow to guide the visitor to the physical start before
     unlocking any stop). **The `ordered_strict` option exists specifically to resolve
     a logic trap in the nearest_flexible default**: if a Gauntlet circuit is
     deliberately authored as a narrative climax whose *entire premise* is that stops
     build sequentially (e.g. a "10-Second Countdown to the Earthquake" circuit where
     stop 1 is the calm *before* and stop 6 is the collapse — starting at stop 4
     would destroy the narrative coherence), then skipping stops 1–3 is not a
     "catch-up opportunity," it's a broken experience. For that small category of
     circuits, `ordered_strict` is the correct author-declared override. For
     everything else — the "Earthquake 1755 Before & After" style retrospective that
     works fine viewed in any order — leave the default `nearest_flexible`. The two
     values together, per-circuit, give the framework the flexibility to avoid the
     "forced to walk to stop 1" problem for most content without silently
     mishandling the rare content that genuinely requires sequential order.
   - **Add a `traversal_rule` field**, defaulted per `circuit_type` but overridable
     per circuit: `bidirectional` for Gauntlet/Branch-and-Bottleneck (entering
     mid-sequence unlocks both forward *and* backward from that point — the right
     default, since a visitor who enters late shouldn't permanently lose access to
     earlier content the way a forward-only rule would silently cause); no locking at
     all for Hub-and-Spoke/Parallel Tracks (entry point is moot for them, since
     nothing is sequence-locked to begin with); for Node Graph, once built, the
     graph's own adjacency list (the first pattern above) determines reachability
     from the entry node directly, with no separate traversal-rule field needed.
   - **Fix `completion_rule` to a set-membership check, not an index-walk.** A
     circuit is complete when every POI in `poi_sequence` has been visited, in *any*
     order — not when POIs 0 through N have been visited in strictly increasing
     index order. This is the detail that actually makes entry-point flexibility
     functional: changing only where the arrow points while leaving an
     order-dependent completion check in place means a visitor who enters mid-sequence
     can walk the whole circuit and still never receive the "completed" badge/state.
     Name the two values explicitly — `visit_all` (order-independent; the new
     default) and `visit_in_order` (kept available, not removed, for the rare circuit
     deliberately authored as a single irreversible narrative climax where strict
     order is the actual point).
   Two implementation details worth getting right while building this, both cheap:
   show a brief, friendly on-screen acknowledgement when a circuit starts away from
   `poi_sequence[0]` ("Starting from Sé de Lisboa — stops 1–4 are also available
   behind you") rather than silently renumbering things, since a visitor who has
   previously seen circuits start at "1 of 8" and now sees "4 of 8" with no
   explanation could reasonably assume something is broken rather than working as
   intended; and resolve nearest-POI ties (the rare case of standing equidistant
   between two sequence members) with a simple, deterministic rule — the lower
   sequence index — rather than spending engineering time on a problem that doesn't
   need a clever solution.

   This whole design is also a small but genuine, citable methodological point for
   the thesis report, not just an engineering fix: it is well-documented in the
   museum-visitor-behaviour literature that visitors do not enter or move through
   exhibits in a fixed, predictable order — Véron and Levasseur's classic ethnographic
   typology (the "ant," "fish," "butterfly," and "grasshopper" visiting styles) has
   been re-validated across dozens of subsequent studies since 1983 [^w27], and
   Bitgood's analysis of museum circulation patterns specifically addresses how
   visitor movement and orientation interact with exhibit layout [^w28]. Treating a
   wall's circuits as always starting from one fixed edge was, in that light, an
   unexamined simplification carried over from the document/timeline metaphor this
   framework otherwise deliberately moved away from (§0) — and even the more formal
   academic museum-tour-planning literature often still assumes a tour begins at a
   fixed entrance, which suggests this fix is a genuine, if small, contribution
   specific to the continuous-wall case (a wall has no single "entrance" the way a
   museum building does — every metre along its length is an equally valid point of
   first contact). **Add Véron & Levasseur (1983), Bitgood (2006), and van Hage et
   al. (2010) to the thesis's own bibliography** (full citations in the new callout
   immediately below this item) — they belong in the related-work or methodology
   chapter's discussion of visitor movement and personalised wayfinding, not just in
   this plan's own technical reference list.

   **Deviation during an active circuit — the same problem, continuous rather than
   one-off, and the right framing is "ambient pivot," not "recalculating."** Entry-point
   resolution (above) solves "where do we start"; the same underlying assumption —
   that the visitor will follow `poi_sequence` in the authored order — can still be
   violated mid-circuit, if a visitor walks past the current target and lingers at a
   different POI instead. The car-navigation framing is the right way to think about
   the *wrong* answer here before settling on the right one: a rigid,
   "Recalculating…"-style interruption every time someone wanders is naggy and
   algorithmic in a way that actively fights the contemplative aesthetic of looking at
   historical art, but doing nothing at all risks leaving the wayfinding arrow
   stubbornly pointing at content the visitor has already silently decided to skip —
   breaking immersion from the opposite direction. The fix is **ambient, continuous
   re-evaluation with a deliberately slow trigger, not an event-driven
   "recalculating" interruption**:
   - **Scope this to sequential circuit types only** (Gauntlet, Branch-and-Bottleneck)
     — Hub-and-Spoke and Parallel Tracks have no enforced "current target" to deviate
     from in the first place (§3, above), so there is nothing here for them to react
     to.
   - **A sustained-engagement threshold, not a glance threshold — and the 10–15s
     figure is not an arbitrary guess, it sits squarely inside an established
     benchmark range from museum visitor-studies research, worth verifying rather
     than just trusting.** Re-evaluating on
     every momentary gaze shift would make the wayfinding arrow flicker erratically
     during entirely normal behaviour (chatting, taking a photo, glancing at a
     neighbouring POI for two seconds) — worse than the original rigid behaviour, not
     better. The field's own standard methodology treats two seconds as the minimum
     to even count as a deliberate "stop" at all (Serrell's widely-used tracking-and-
     timing protocol, still the field's standard reference for measuring exhibit
     dwell time [^w35]), while a separate, more contemporary study of a digital
     museum exhibit explicitly defines genuinely "hooked" engagement as **dwell time
     beyond a ten-second threshold**, distinct from a passing stop [^w36] — together,
     these support treating something in the 10–15-second range as a reasonable,
     evidence-grounded line between "glanced at" and "genuinely engaged with," not a
     number invented for this framework. Require that range of sustained gaze-lock or
     interaction with
     an out-of-sequence POI (reusing the same dwell/lock-on mechanic and its existing
     dead-zone already specified for gaze-lock interaction, item 3 below) before treating it
     as a genuine deviation rather than a glance. This is the same hysteresis
     principle already applied elsewhere in this plan, extended to a larger spatial
     scale, and it is worth citing Serrell (1997) directly in the thesis methodology
     or related-work chapter as the basis for this specific parameter choice, rather
     than presenting it as an unexplained constant in the code.
   - **On a confirmed deviation, recalculate `currentIndex` to the nearest unvisited
     member of `poi_sequence` ahead of the visitor's new position** — the identical
     nearest-neighbour mechanism used for entry-point resolution, simply re-run rather
     than run once. Any sequence member that's now behind the visitor and unvisited is
     marked **"Skipped"** on the progress rail (a hollow rather than filled circle,
     alongside the already-specified "3 of 8 stops" indicator) rather than silently
     dropped — visible, low-cost, and tells the visitor at a glance that catching it
     on the way back is still possible, consistent with the `completion_rule:
     "visit_all"` fix above actually allowing them to do so.
   - **The "Butterfly Prompt" — a deliberate, bounded injection of visitor agency
     into an otherwise authorial-intent-driven structure.** If sustained engagement is
     with a POI that belongs to a *different* circuit's `poi_sequence` (an
     entirely ordinary case once POIs are allowed to appear in more than one circuit,
     §10's schema — nothing here requires a new field, only a reverse POI→circuits
     lookup computed once at Stage 4's bake step, not at runtime), surface a single,
     dismissible toast through the guide character: "You've found a Maritime Lisbon
     landmark — want to switch to this tour instead?" This is worth naming precisely
     rather than just building it, because it is a genuine instance of a recognised
     tension in interactive-narrative design: Janet Murray's concept of **agency** —
     the player's felt capacity to take meaningful action and have the world respond
     — versus authorial intent, the designed shape of the experience [^w30]. Mateas
     and Stern's interactive drama *Façade* is built around exactly this tension,
     explicitly designing for "global agency" within authored dramatic structure
     rather than treating the two as opposites to pick one of [^w31]. This framework's
     own `circuit_type` system already encodes a version of this spectrum without
     naming it that way — a Gauntlet prioritises authorial intent (a designed,
     ordered sequence); Hub-and-Spoke maximises agency (explicit any-order access).
     The Butterfly Prompt is the moment a *sequential*, authorial-intent-leaning
     circuit deliberately makes a small, bounded offer of agency back to the visitor,
     rather than staying rigid — directly honouring the "butterfly" visitor type in
     Véron & Levasseur's typology, cited above [^w27]. Accepting the prompt should
     gracefully pause (not destroy) the original circuit's state — its progress rail
     and "Skipped" markers stay intact for resuming later — and run entry-point
     resolution for the new circuit at the visitor's current position, exactly as
     specified above; this is not a new code path, it's the existing mechanism invoked
     a second time. **Concretely, "pause not destroy" means keeping a small stack
     (not just one slot) of circuit states** — each entry holding that circuit's
     `currentIndex`, its visited-set, and its skipped-set — pushed when a Butterfly
     Prompt is accepted and popped when the visitor explicitly returns to a previous
     circuit (surfaced via the circuit-selection screen already specified above,
     showing "Resume" instead of "Start" for any circuit with a paused entry on the
     stack). A stack rather than a single slot costs almost nothing extra to build and
     correctly handles a visitor accepting a *second* Butterfly Prompt while already
     mid-detour — a real possibility worth designing for rather than discovering as a
     crash report, given that the whole point of this feature is to let visitors
     wander.
   - **This whole adaptive behaviour is itself an instance of Falk & Dierking's
     Contextual Model of Learning**, the museum-education framework describing visitor
     experience as continuously shaped by Personal, Sociocultural, and Physical
     contexts interacting with each other [^w32] — a Butterfly Prompt is literally the
     system detecting a real-time *Personal*-context signal (sustained interest,
     observed rather than declared at onboarding) and responding to it within the
     *Physical* context already given by the wall's layout. **Add Falk & Dierking
     (2000) to the bibliography callout below** alongside the others; it belongs in
     the same related-work discussion as the visitor-movement citations, since this is
     the clearest concrete instance in the whole framework of "context" actually
     driving a runtime decision rather than just being discussed abstractly.

   > **For the thesis's own academic bibliography — full citations, not just this
   > plan's informal footnotes:**
   > - Véron, E., & Levasseur, M. (1983). *Ethnographie de l'exposition: l'espace, le
   >   corps et le sens*. Paris: Bibliothèque publique d'Information, Centre Georges
   >   Pompidou. — *Relevance: the foundational, still widely-cited empirical typology
   >   of non-linear museum visitor movement (ant/fish/butterfly/grasshopper); direct
   >   evidence that visitors do not traverse exhibits in a fixed order, supporting
   >   the case for entry-point flexibility on methodological rather than purely
   >   engineering grounds.*
   > - Bitgood, S. (2006). An analysis of visitor circulation: Movement patterns and
   >   the General Value Principle. *Curator: The Museum Journal*, 49(4), 463–475.
   >   — *Relevance: circulation/wayfinding patterns specifically, complementing
   >   Véron & Levasseur with a design-oriented (rather than purely ethnographic)
   >   treatment of the same phenomenon.*
   > - Adams, E. (2014). *Fundamentals of Game Design* (3rd ed.), Chapter 12: General
   >   Principles of Level Design. Berkeley, CA: New Riders. — *Relevance: the
   >   spatial-layout counterpart to Ashwell's narrative-branching taxonomy already
   >   cited in §2 — Linear, Parallel, Ring, Network, and Hub-and-Spoke layouts.
   >   Notably, Chafariz Velho's circular geometry is a direct instance of Adams'
   >   "Ring layout," and Adams' own guidance on Network layouts — "stories must be
   >   able to tolerate the player experiencing events in any sequence" — is the
   >   game-design-literature equivalent of the `completion_rule` fix above.*
   > - van Hage, W. R., Stash, N., Wang, Y., & Aroyo, L. (2010). Finding your way
   >   through the Rijksmuseum with an adaptive mobile museum guide. In L. Aroyo et
   >   al. (Eds.), *The Semantic Web: Research and Applications* (ESWC 2010),
   >   *Lecture Notes in Computer Science*, vol. 6088, pp. 46–59. Springer.
   >   https://doi.org/10.1007/978-3-642-13486-9_4 — *Relevance: the closest direct
   >   precedent found for this exact mechanism — a mobile museum guide that re-routes
   >   a personalised tour in real time based on the visitor's actual current
   >   position, from the CHIP/Rijksmuseum research programme.*
   > - Falk, J. H., & Dierking, L. D. (2000). *Learning from Museums: Visitor
   >   Experiences and the Making of Meaning*. Walnut Creek, CA: AltaMira Press. —
   >   *Relevance: the Contextual Model of Learning (Personal, Sociocultural, and
   >   Physical contexts) — this entire app, and the Butterfly Prompt specifically,
   >   is a concrete software instance of a visitor's Personal context (real-time
   >   interest) being read and responded to within their Physical context (the
   >   wall). A strong fit for the related-work or methodology chapter's framing of
   >   what "personalisation" actually means here.*
   > - Murray, J. H. (1997). *Hamlet on the Holodeck: The Future of Narrative in
   >   Cyberspace*. New York: Free Press. — *Relevance: the foundational source for
   >   "agency" as a term of art in interactive narrative — the felt capacity to take
   >   meaningful action and have the world respond — directly underlying the
   >   agency/authorial-intent framing used to justify the Butterfly Prompt and the
   >   `circuit_type` spectrum (Gauntlet vs. Hub-and-Spoke) above.*
   > - Mateas, M., & Stern, A. (2003). Integrating plot, character and natural
   >   language processing in the interactive drama *Façade*. In *Proceedings of the
   >   1st International Conference on Technologies for Interactive Digital
   >   Storytelling and Entertainment (TIDSE 2003)*, pp. 139–151. — *Relevance: the
   >   concrete, shipped (if research-prototype) treatment of designing for agency
   >   within authored dramatic structure — "global agency" — rather than treating
   >   visitor choice and designed narrative as mutually exclusive; the direct
   >   precedent for how this framework's circuits deliberately mix the two.*
   > - Carson, D. (2000). Environmental storytelling: Creating immersive 3D worlds
   >   using lessons learned from the theme park industry. *Gamasutra*. — *Relevance:
   >   the practitioner-originated (and, via Henry Jenkins's "Game Design as
   >   Narrative Architecture" — originally presented c. 2002–2003, canonically cited
   >   as Jenkins, H. (2004), in N. Wardrip-Fruin & P. Harrigan (Eds.), *First Person:
   >   New Media as Story, Performance, and Game*, pp. 118–130, Cambridge, MA: MIT
   >   Press — subsequently academically formalised) concept that a physical/virtual
   >   space itself can carry narrative meaning, and that an experience must let a
   >   visitor answer "Where am I?" within the first moments — the explicit design
   >   goal behind this framework's Stage 1 onboarding screen and the "scanning
   >   state" indicator (§3, Stage 1). Worth noting directly in the thesis: Jenkins's
   >   own essay independently describes game worlds as becoming "a kind of
   >   information space, a memory palace" — the same method-of-loci framing the
   >   Krokos et al. citation below supports empirically, from a completely separate
   >   research tradition (game/narrative studies vs. HCI/cognitive science),
   >   reaching the same idea about spatially-organised information.*
   > - Krokos, E., Plaisant, C., & Varshney, A. (2019). Virtual memory palaces:
   >   Immersion aids recall. *Virtual Reality*, 23, 1–15.
   >   https://doi.org/10.1007/s10055-018-0346-3 — *Relevance: peer-reviewed evidence
   >   (8.8% higher recall, HMD vs. desktop) that immersive, spatially-anchored
   >   information recall — the *method of loci* this framework implements literally
   >   — outperforms flat presentation; a citable basis for expecting a learning-gain
   >   effect from the AR format itself (RQ5, §7's knowledge-check rationale).*
   > - Serrell, B. (1997). Paying attention: The duration and allocation of visitors'
   >   time in museum exhibitions. *Curator: The Museum Journal*, 40(2), 108–125.
   >   https://doi.org/10.1111/j.2151-6952.1997.tb01292.x — *Relevance: the
   >   field-standard methodology for what counts as a deliberate "stop" versus a
   >   passing glance in museum visitor studies, cited here as the evidentiary basis
   >   for the 10–15-second sustained-engagement threshold used to trigger ambient
   >   pivot above, rather than presenting that number as an unexplained constant.*

   Concrete reference circuits for the Panorama, useful as the worked
   examples that prove the engine actually works, not just compiles:
   - *"Earthquake 1755: Before & After"* — 8 POIs, ~15 minutes, contrasting destroyed
     vs. survived structures (Gauntlet-shaped).
   - *"Power & Religion"* — 8 POIs, ~12 minutes, castles, churches, and convents.
   - *"Children's Adventure"* — 6 POIs, ~10 minutes, simplified language, fun facts,
     more heavily gamified.
   - *"Vida Quotidiana"* (Daily Life) — markets, docks, trades, ordinary professions.
   - *"Lisboa Marítima"* (Maritime Lisbon) — ports, arsenals, ships, trade routes.
   Circuit selection screen: one card per circuit (title, description, estimated
   time, POI count, a profile-fit badge such as "Best for families"). In-circuit UI:
   a persistent progress indicator ("3 of 8 stops") and a "Next Stop" button. On
   reaching the final stop, show one consolidated **completion screen** rather than
   leaving the pieces scattered — a short summary of what was explored, the relevant
   knowledge-check questions if any of this circuit's hero POIs have one (item 4
   below), and the share button (item 5 below) together in one place, not as three
   separate, disconnected triggers the visitor has to notice independently.
3. **Wayfinding and targeting** — gaze/look-at raycasting layered on top of distance
   gating (distance alone cannot disambiguate POIs that sit close together at wall
   scale): a distance gate must pass before gaze-lock is even available; a
   screen-space wayfinding arrow is pinned along the line from screen-centre to the
   target at roughly 70% of that distance; a HUD crosshair changes visual state on
   lock; a radial lock-on charge (1–1.5 second hold) includes a dead-zone so small
   head movements don't cancel it accidentally, and fizzles out gracefully rather than
   cutting abruptly if the visitor looks away mid-charge. Within a circuit
   specifically, the directional hint additionally carries a readable label (e.g. "→
   Castelo, ~5m right"), computed from the camera's current facing direction relative
   to the target's wall-position — this was flagged in the original brainstorm as a
   genuinely valued, original idea and is worth the implementation effort it costs.
   **Auto-open vs. prompted-open is a per-circuit-type setting, not global**: a
   gamified/quiz context benefits from instant auto-open; a contemplative heritage
   context benefits from a tap-to-open prompt that doesn't yank attention away from
   the physical wall. Outside of any circuit, a simpler general-purpose version of the
   same arrow points toward the nearest not-yet-visited POI.
4. **Gamification** — a persistent discovery counter ("15 / 150 buildings
   discovered") with a checkmark on visited markers. Six concrete badges, with the
   explicit caution that badges should be earned through genuine content engagement,
   not handed out on arrival — the gamification literature already reviewed in this
   thesis (Mekler et al.; Souropetsis & Kyza) shows decorative, disconnected rewards
   can reduce intrinsic motivation rather than help it:
   - 🏛️ "Lisboa Religiosa" — first religious building visited
   - 👑 "Poder Real" — first power/palace building visited
   - 🚢 "Marinheiros" — first maritime building visited
   - ⚡ "Sobrevivente 1755" — first destroyed-in-the-earthquake building visited
   - 🗺️ "Explorador" — first circuit completed
   - ⭐ "Conhecedor" — 25 POIs visited; 🔍 "Investigador" — 50 POIs visited
   Per-hero-POI quiz: one question per hero POI, three options, single correct
   answer, shown *after* the content card has been viewed (not before, so it tests
   retention rather than guessing), with a concrete, verifiable question style (e.g.
   "Did this building survive the 1755 earthquake?") rather than trivia requiring
   outside knowledge. Immediate feedback: a green flash plus a short explanation for
   correct answers, a gentle "Actually…" plus explanation for incorrect ones, with no
   penalty either way. "Did you know?" fun facts: roughly twenty short facts,
   triggered every five minutes of active use or on the fifth POI tap (whichever comes
   first), shown as a small dismissable card that auto-disappears after ten seconds if
   not dismissed manually.
5. **Social sharing** — a dedicated capture button overlaid on the AR view, saving a
   watermarked/framed screenshot to the device's camera roll (Android versions before
   the modern scoped-storage model need the storage permission handled explicitly,
   newer ones do not); a share button appears on circuit completion and badge unlocks,
   opening the native share sheet. A leaderboard is explicitly **not** built at this
   stage — it would require backend infrastructure and raises privacy questions out
   of proportion to its value here; defer it entirely rather than build a half version.
6. **Guide/avatar character** — persistent UI chrome narrating the scripted audio
   layer. **Important correction from the original notes**: this is a 2D illustrated
   character with a small set of static poses/expressions (an azulejo-craftsman
   concept fits the period and the medium), not a 3D model and not lip-synced to
   generated speech — that is a *different, explicitly out-of-scope* feature (see §4).
   Earlier notes assumed a Flutter-specific animation tool (Rive) for this; since the
   project is Unity-only, implement it instead with Unity's standard 2D Animation
   package (sprite skinning) or, more simply, a small hand-authored sprite-sheet/frame
   animation — there is no need to introduce an external animation tool into the
   Unity pipeline for a handful of idle/talk/point poses.

   **Why this is worth real budget and not just a "nice to have" UI flourish:** a 2025
   *Advances in Culture* journal study of an Egyptian Museum (Cairo) mixed-reality
   experience found the virtual guide avatar was the single most highly rated element
   of the whole experience — rated above the content itself. The CHESS project
   (Roussou & Katifori, already in the thesis bibliography) found narrative
   characters increased engagement at the Acropolis Museum specifically. The Europeana
   XR project (wrapping up its own work around July 2026) is independently building an
   AI-powered Avatar Builder for heritage sites, including a monastery site in Cyprus —
   the underlying intuition that a character matters more than a disembodied voice is
   not unique to this project. Concretely, for this thesis: a character gives the audio
   guide a memory hook ("the painter who told me about the earthquake" is more
   memorable than an unattributed voice), differentiates from every comparable museum
   app currently on the market (Bloomberg Connects, Smartify, and Google Arts & Culture
   all use a disembodied voice or plain text, not a character), gives a distinctive,
   shareable image for the social-sharing feature above, and is close to non-negotiable
   for the children's profile specifically — children engage with characters far more
   reliably than with voice-over alone.

   **Character design brief, concrete enough to hand directly to an illustrator:**
   setting is Lisbon, c. 1700; offer 2–3 character concepts rather than committing to
   one sight-unseen — (a) a young sailor/explorer, (b) an elderly azulejo tile-painter,
   (c) a noblewoman from the Paço da Ribeira court. The tile-painter is the most
   thematically resonant of the three (literally the person who made what the visitor
   is looking at) and is the recommended default, but get the illustrator's read on
   feasibility across all three before deciding. Style: not cartoonish, not
   hyper-realistic — warm and illustrated, in the visual register of a historical
   picture book or the illustrations in a travel guide, not a game-asset style. Hard
   technical constraint to give the illustrator up front: the character must remain
   legible at roughly 120×200px on a phone screen while the AR camera view is active
   behind it, which rules out fine detail that only reads at large sizes. Commission
   through a freelance illustration platform (the budget in §6 assumes Fiverr-style
   pricing, €150–300 for the character concept plus the 4–6 base poses) rather than
   attempting the illustration in-house unless genuinely confident in that skill —
   this is the one piece of the whole project where outsourcing is clearly the right
   call given the time/quality trade-off for a non-illustrator.
7. **Search + filters, FAB menu** — secondary navigation chrome, built last in this
   stage since it depends on the marker/category system from Stages 1–2 already being
   stable. A floating action button expands into circuit/audio/achievements/AI-guide
   destinations; a search overlay supports filtering by category and by status/fate
   value (where the active wall defines one).
8. **Telemetry, fully wired** — every interaction from Stages 1–3 now flows through a
   two-tier pipeline, not built ad hoc per feature, named explicitly so the split is a
   referenceable architectural decision rather than an implicit one: **Type A** (local,
   on-device storage — `PlayerPrefs` or a local JSON file: score, time, percentage
   complete, no backend needed) and **Type B** (an abstract backend interface — a stub
   method such as `UploadScore(...)`, deliberately not connected to a real server yet)
   — this demonstrates extensibility without paying the cost of building real backend
   infrastructure this thesis doesn't need. **Route the Stage 7 analytics events
   (dwell time, POI taps, circuit completion) through this same Type A/B boundary too**,
   not through a separate, parallel pipeline built later — the entire point of having
   the boundary is that swapping in a real backend after the thesis touches one class
   (the Type B implementation), not every feature that emits an event. The
   *event taxonomy* itself is specified in full in Stage 7 (§3, below) — wire the
   pipeline now, populate every event listed there as each feature above ships, rather
   than retrofitting analytics after the features already exist.

**Stage 3 exit criteria:** all three walls support at least one full circuit;
onboarding completes in under 30 seconds for a first-time tester; every event in
Stage 7's taxonomy that corresponds to a feature shipped so far is actually firing
(verified in a debug log, not just code-reviewed); the guide character has at minimum
idle/talk/point poses wired to the audio block. If any of these is not true, do not
start Stage 4 — automation built on top of an unstable feature set just bakes the
instability in at the next layer down.

---

### Stage 4 — Automation: schema + compiler (Weeks 17–18)

Only now, once three walls' worth of hand-written JSON exists and the data's shape has
stabilised through real use:
- Write the actual JSON Schema (or a hand-rolled validator) covering every field used
  across all three walls so far — markedly easier and more accurate now than it would
  have been speculatively at Week 1. `Newtonsoft.Json.Schema` (or Unity's built-in
  `JsonUtility` plus a small hand-rolled set of field/range checks) is enough for
  this — the schema itself is small enough that pulling in a heavier validation
  dependency isn't justified.
- Build a "Validate Config" check: required fields, enum values where they genuinely
  are fixed (e.g. `circuit_type`), coordinate ranges, and a `config_schema_version`
  field for forward compatibility as the schema itself evolves later.
- Build the JSON→`WallConfigAsset` ScriptableObject baker; re-point all runtime code
  at the baked asset instead of parsed JSON, so there is zero JSON parsing at runtime,
  only at import/build time.

---

### Stage 5 — Editor tooling: the Onboarding Wizard (Weeks 19–20)

**Simplicity guarantee — the Wizard's first screen must present the minimal viable wall
path, not the full schema.** The full schema in §10 has many optional fields that are
genuinely useful but intimidating on a blank first encounter. The Wizard's opening
screen (the first thing a developer sees when adding a new wall) must present only:
`wall_id`, `config_schema_version`, `geometry`, and one POI with `name_pt/en`,
`category`, `wall_position`, and `summary_pt/en`. Nothing else. Every other field is
reachable via an "Add optional fields →" expandable section, or via a link to the full
schema reference in the documentation tab. The principle: a developer who just wants to
show 10 POIs with basic text should be able to do that in under 30 minutes using the
Wizard and the Samples~/reference implementation, without reading §10's full schema at
all. A developer who wants lapse-states, circuits, badges, and a conversational guide
can access all of that through the same Wizard in a later session. Design this UX
hierarchy in Stage 0 (§ Stage 0 item 5 above); implement it here.

**Every Wizard feature must remain strictly optional** (the MARS lesson, §2): a
developer must always be able to ignore the Wizard entirely, hand-edit a JSON file, and
build a scene the normal Unity way. The Wizard is a convenience layer, not a required
gate.


- `EditorWindow`, optionally auto-opened via `[InitializeOnLoad]` on first import.
- A documentation tab with copy-paste JSON snippets per block type — accurate now
  that block types were finalised through real use in Stage 2, rather than written
  speculatively against a schema that hadn't been tested yet.
- "Validate Config" and "Populate Scene Markers" buttons wired to Stage 4's
  validator/baker — the latter turns "add a wall" from manual scene-building into
  dropping in a folder and pressing one button.
- Every Wizard feature remains strictly optional, per §2's MARS lesson — a developer
  must be able to ignore the Wizard entirely.

---

### Stage 6 — Packaging for export (Weeks 21–22)

- Extract the proven, three-wall-tested core into a UPM package
  (`TileStoriesFramework`) using Unity's exact current package layout — this is not a
  matter of preference, Unity's own manual specifies it precisely [^w14][^w15]:
  ```
  TileStoriesFramework/
  ├── package.json
  ├── README.md
  ├── CHANGELOG.md
  ├── LICENSE.md
  ├── Editor/
  │   └── TileStories.Editor.asmdef
  ├── Runtime/
  │   └── TileStories.asmdef
  ├── Tests/
  │   ├── Editor/TileStories.Editor.Tests.asmdef
  │   └── Runtime/TileStories.Tests.asmdef
  ├── Samples~/          (tilde hides it from the Project window until installed)
  └── Documentation~/
  ```
  Two non-negotiable rules straight from Unity's own docs, already enforced by §12's
  asmdef-reference direction but worth restating here at the point of actual
  extraction: every package with code needs **at least one `.asmdef`**, and
  **runtime code can never reference editor code** (the reverse — editor referencing
  runtime — is fine and usually required) [^w14]. Note the Tests split specifically:
  editor-only tests and runtime tests get their *own* separate asmdefs nested under
  `Tests/Editor` and `Tests/Runtime` respectively, mirroring the same runtime/editor
  separation as the main code rather than one undifferentiated `Tests/` assembly.
- Extract the outer shell into a Unity Project Template, so starting a new wall is
  "New Project → TileStories Template," not manual folder surgery. Between these two,
  UPM (importing the reusable, versioned engine into an existing project) and the
  Project Template (starting a brand-new project already shaped correctly) answer two
  genuinely different "how do I get this" questions, and shipping both is what
  actually delivers on both of this project's original packaging goals at once. A
  third option, **`.unitypackage` export**, is worth naming explicitly even though it
  isn't the primary path: it's the lowest-friction way to hand a single
  `App_[Wall]`-style content folder to a collaborator one-off, but it's worse for
  ongoing versioning than either UPM or the Template — keep it available as a
  convenience export, never as the primary distribution method.

- **Time-to-new-wall measurement protocol** (directly produces citable evidence for
  the thesis's "easy to add a new wall" claim): add one new wall (re-add Chafariz from
  scratch to a clean project) as a timed, unassisted task using ONLY the Wizard,
  `Samples~/`, and `Documentation~/` — no source-code access. Record wall-clock time
  to three milestones: (1) first successful Immersal localisation; (2) first POI marker
  tappable; (3) first complete circuit working. Log every friction point (moment where
  the process was unclear or required an outside source). Report these times and friction-
  point count in the thesis as direct evidence of generalisability. Without this protocol
  specified and executed, the generalisability claim is a design assertion, not empirical
  evidence.
- One of the three test walls ships as the package's `Samples~` reference
  implementation — Chafariz Velho is the natural choice (fully public, no
  institutional dependency, hardest geometry already proven).
- **A "time-to-new-wall" measurement, not just a working package.** The strongest
  available evidence for the framework claim is not "the package exists" but "someone
  could use it quickly" — so once the package and template exist, deliberately
  integrate one more toy/test surface using *only* the packaged Wizard, the
  documentation tab, and the public template — no editing framework internals, no
  consulting the three walls' own code. **Protocol to make this result citable:**
  (1) Use Chafariz Velho as the test case — re-add it from scratch to a clean project,
  simulating a developer who has never seen this codebase. (2) Record wall-clock time
  to three defined milestones: first successful Immersal localisation; first POI marker
  tappable; first complete circuit working. (3) Log every moment where the process was
  unclear, blocked, or required consulting a source outside the package documentation —
  these are the friction points. (4) Report the elapsed times and friction-point count
  in the thesis as direct, quantitative evidence for the "ease of adding a new wall"
  claim. This is the single strongest piece of evidence available for the
  generalisability claim — considerably more convincing to an examiner than the
  package's existence alone, and it costs at most a day to produce.
- **Store-submission checklist**, prepared in parallel even if the actual submission
  happens later or not within the thesis window: ten screenshots, a 30-second preview
  video, a PT+EN localised store description, an age rating, and a privacy policy URL
  (host it on GitHub Pages — free, simple, sufficient). The privacy policy needs to
  state plainly what analytics are collected, that no personally identifying
  information is stored, and that Firebase is the provider. App Store Optimisation
  keywords to include in the title/subtitle: "Lisboa," "Azulejo," "Museu," "AR,"
  "Realidade Aumentada," "Tile," "Panorama," "1755." On Android, watch the final app
  size carefully: Google Play's Android App Bundle format caps the *compressed
  download size* at 150MB — a hard publishing requirement, not a soft review flag
  [^w17] — so use Unity's Build Profiles → Split Application Binary and Texture
  Compression Targeting (Player Settings → Publishing Settings) to stay under it,
  delivering wall content on-demand via the Addressables groups already decided in
  §sec:pw-architecture rather than bundling all three walls into the base install.

---

### Stage 7 — Evaluation readiness and stretch (Weeks 23–26)

**Analytics — the complete event taxonomy** (wire this in progressively through
Stages 1–3 as each feature ships; specified here in full so nothing is missed):
- *Session events*: `app_open`, `wall_detected`, `wall_lost`, `session_end` (with duration).
- *POI events*: `poi_tapped` (POI id, active profile, active timeline epoch),
  `poi_card_closed` (time spent on card).
- *Feature events*: `timeline_changed` (from/to state), `circuit_started`,
  `circuit_completed`, `circuit_abandoned` (at which stop), `audio_played`,
  `audio_skipped` (after how many seconds), `quiz_answered` (correct/incorrect, which
  POI), `badge_earned`, `screenshot_taken`, `share_tapped`.
- *Navigation events*: `tab_switched`, `list_searched`, `profile_changed`.
- *Every event*, regardless of type, carries: timestamp, an anonymous session UUID,
  active profile type, active timeline epoch, active circuit (if any), and an
  approximate running count of POIs visited that session.

**Quantitative instruments — SUS and UEQ-S, not improvised in their place.** The
thesis's own evaluation chapter (§subsec:pw-eval-instruments) already commits to the
System Usability Scale (ten items, scored against the standard <51/51–68/>68
poor/acceptable/good thresholds) and the Short User Experience Questionnaire (UEQ-S,
eight items covering pragmatic and hedonic quality) as the two instruments anchoring
the quantitative evaluation, benchmarked there against Sauter et al.'s comparable
mobile AR heritage app. Neither appeared anywhere in this plan before this revision —
administer both on-device immediately after each session's free-exploration and
prompted-task portions, before the exit survey below, using their standard published
wording rather than a paraphrase (both are short, established instruments; improvising
substitute questions forfeits the published-norm comparison that is the whole point of
using a standard instrument in the first place). Where time and a willing participant
allow, follow with the short semi-structured interview the same chapter commits to,
modelled on Cesário and Nisi's protocol for family/teen visitors — this is the only one
of the resulting five guaranteed-per-session instruments that is genuinely optional;
SUS, UEQ-S, the exit survey below, and the pre/post knowledge check are not.

**Exit survey — exactly five questions, no more, as the third of those instruments**
(completion rate collapses past
this length): overall experience (5-star); "Did the app help you understand the
history of Lisbon?" (Yes / Somewhat / No); favourite feature (multiple choice: AR
markers / Timeline / Audio guide / Circuits / Quiz); open difficulties (free text,
optional); a single recommend-to-a-friend item on a 1–5 agreement scale. Offer a small
incentive if feasible (a museum discount, a sticker) — it measurably improves
completion. A brief pre/post knowledge check (around five simple factual-recall
questions about the Panorama and the 1755 earthquake, shown unobtrusively at
onboarding and again at the exit survey) gives the thesis's learning-gain metric;
keep it short enough that it doesn't feel like a test. **A citable hypothesis worth
stating explicitly in the thesis discussion of this metric**: the *method of loci*
(the classical mnemonic of anchoring information to specific places, which this
framework does literally, by binding facts to fixed positions along a real wall) has
direct, peer-reviewed support for improving recall specifically when the spatial
encoding is immersive rather than flat — a 2019 study found participants recalled
8.8% more information using a head-mounted-display memory palace than an
equivalent desktop one [^w33]. This doesn't replace running the actual knowledge-check
data, but it gives a concrete, citable reason to *expect* a learning-gain effect from
the spatial AR format itself, independent of any single feature like personalisation
or gamification — worth adding to the related-work or discussion chapter alongside
the bibliography entries already flagged above.

**Performance and device testing**: profile with Instruments (iOS) / Android
Profiler; targets are under 200MB RAM in an active AR session, under 80MB initial app
download size, and a sustained 25fps or better. Apply the 80/20 rule — fix the three
worst offenders found, do not chase perfect optimisation. Test matrix: iPhone 12,
iPhone 14, a recent Samsung flagship, and a mid-range Android device (borrow one if
needed) — mid-range coverage matters because it's closer to a typical visitor's actual
phone than a developer's own device usually is. Battery: a 20-minute session should
not drain more than roughly 15% on a mid-range device. Offline: disable connectivity
entirely and confirm the full experience still works from the local bundle.
Accessibility: a VoiceOver/TalkBack navigation pass through the main flows (building
on the Stage 1 smoke check, §3 above), a check that
increasing system font size doesn't break card layouts, a text-contrast pass checked
against **WCAG 2.1 AA** thresholds specifically (a named, checkable standard, not a
vague "looks readable" judgement) and — specifically — a
colour-blind-mode check on the status-colour coding (e.g. the Panorama's
green/amber/red earthquake-fate scheme), adding a
border-style or icon redundancy rather than relying on colour alone, consistent with
the marker-encoding system already designed for exactly this reason.

**UX refinement**: upgrade Stage 1's functional-but-plain "Looking for the wall…"
state into a polished one — an
immediately-live camera view, an animated scan-line, and a short instruction; on
successful lock, a brief celebratory animation and a pulse on the first visible
marker to draw attention. Compare the resulting time-to-first-lock figure against
Stage 1's own baseline measurement of the same metric — an improvement here is a
legitimate, citable piece of evidence, not just a cosmetic change. Loading screens show rotating historical facts instead of a
generic spinner. A settings screen covers language, profile, audio on/off, text size,
and a reset-progress option. Localisation: build the framework so that adding a
language is "add a translated string table," never "find and replace hardcoded
strings" — get this structural property right as soon as the text/audio blocks exist
in Stage 2, not as a Stage-7 retrofit. **Language priority and why**: PT-PT and
English first, not as an arbitrary default but because together they cover roughly
80% of the realistic visitor base — Lisbon receives on the order of 7 million tourists
a year, and non-Portuguese visitors are predominantly English-speaking even when it
isn't their first language; Spanish and French are the next most common visitor
languages in Lisbon and add a further ~15% of coverage between them, which is real
but secondary. Minimum translation scope: the ten hero POIs
plus all UI strings, in PT-PT and English; Spanish/French are stretch only, pursued
after everything above is solid. A simple QR-code landing page (one static webpage,
device-detecting redirect to the right app store) supports a physical card at a wall's
entrance.

**Formal evaluation sessions**: recruit at least 20–30 participants where possible,
across three channels — real visitors at whichever wall is accessible at the time;
FCT NOVA students/researchers (easy to recruit, but note explicitly in the thesis that
this skews tech-savvy relative to a general visitor population); and family/friends
specifically to cover the children's circuit (aim for at least 3–4 child
participants). Session protocol: a two-minute briefing explaining the app is for the
wall and that the observer will watch but not help; 10–15 minutes of free,
unprompted exploration with a strict observe-don't-intervene rule; five minutes of
specific prompted tasks (e.g. "find out if this building survived the earthquake,"
"start the Earthquake circuit"); roughly ten minutes for SUS, UEQ-S, the exit survey,
and the knowledge check together — the original five-minute budget here did not yet
account for SUS and UEQ-S, both added above; an optional two-minute informal debrief
("what did you find most/least useful?"). Document every tracking failure with a
timestamp, which zone it occurred in, what the visitor was doing, and how long
re-acquisition took. Even twenty free-text responses are enough for a useful thematic
analysis — this does not require a large sample to be worthwhile. For the
family/children's sessions specifically, secure the parent or guardian's consent and
the child's own assent separately *before* the briefing — both already required by
the thesis's ethics section (§subsec:pw-ethics) — and build the few minutes this takes
into the protocol's timing rather than treating it as a formality squeezed in at the
door.

**Stretch features, in priority order, attempted only if the above is already solid:**
- *Conversational guide layer*: **architectural placement, decided before any
  implementation work**: build this as its own optional block type (`ai_guide`) or a
  Core-level toggle, kept deliberately distinct from every other block — not woven
  into the audio block's code path — specifically so it can be **omitted entirely**
  for a wall or deployment that doesn't want it (a future wall with no GPT budget, or
  one whose owning institution objects to a third-party AI API on principle, should be
  able to ship with this capability simply absent, not present-but-disabled). This
  architectural separation is what makes "any heritage wall" a credible claim for this
  specific feature — a framework where the AI layer is structurally inseparable from
  the rest of the experience would quietly force every future deployment into the same
  cost and API-dependency profile this thesis's own walls happen to use.
  **Correction as of this review (June 2026)** — the
  speech-to-speech model named in earlier notes, GPT-4o Realtime, has since been
  deprecated and removed from OpenAI's API entirely; the Realtime API itself moved
  from beta to a general-availability interface, and OpenAI's current realtime-class
  models (the `gpt-realtime` family, including a newer "GPT-Realtime-2" with
  configurable reasoning for speech-to-speech agents) are the closest equivalent at
  time of writing [^w16]. The underlying requirement this item is built around —
  speech-to-speech, low-latency, native interruption-handling, vision input for
  camera-aware narration — is unaffected by which specific model name is current, but
  given how fast this one API surface has moved even within the writing of this plan,
  **do not lock the model name now**: re-check OpenAI's current Realtime API docs
  (`platform.openai.com/docs`) and pricing at the point this stretch item is actually
  started (Stage 7), and re-estimate the per-session cost below rather than trusting a
  figure that may already be six months stale by then. This is exactly the same
  test-first, decide-late discipline this plan already applies to the AR-tracking
  stack in Stage 1 — a confident technology choice made early and never revisited is
  how the earlier GPT-4o Realtime recommendation went stale in the first place. With that
  caveat: realistic cost for thesis-scale usage, *as last verified*, is roughly
  €0.30–0.60 per 15-minute visitor session — treat this as a planning estimate to be
  re-confirmed, not a number to budget against without checking. Keep this strictly as
  an *additional*, user-initiated layer
  ("🎙️ Ask Guide" button) alongside, never replacing, the scripted audio block — the
  two must never play simultaneously. System-prompt design is where the quality
  actually comes from: feed it the active wall's context, its POI list, the visitor's
  profile, and the last-tapped POI; hard-constrain it to never fabricate historical
  detail and to say plainly when it's uncertain; test extensively before any real
  visitor uses it, since hallucination risk is real. Provide an offline fallback (a
  short list of pre-written answers to the most likely questions) and a hard rate
  limit (e.g. five questions per session) with a friendly redirect back to the content
  cards afterward, implemented as a value read from **Firebase Remote Config** rather
  than a hardcoded constant — this turns the rate limit and the entire feature's
  on/off switch into something that can be tightened or killed instantly from the
  Firebase console if costs or hallucination reports run higher than expected during
  a live evaluation session, with no app update required. Set a monthly spend alert
  (start around €10 as an illustrative trigger point, then adjust once a real per-session
  figure is confirmed) alongside the rate limit, not instead of it — the two catch
  different failure modes (a single runaway session vs. cumulative drift over many
  normal ones).

  **Camera-aware narration — two genuinely different implementations, with a clear
  recommendation between them.** The naive approach is to stream camera frames to the
  model directly: resized JPEGs (roughly 512–720px) at 2–4fps during motion, alongside
  the audio stream, letting the model say something like "I can see you're pointing
  toward the Castelo de São Jorge…". This works, but has three real costs worth
  weighing before committing to it: (1) **performance** — the camera is already busy
  feeding Immersal's localisation pipeline, and adding a second consumer of camera
  frames plus a continuous upload stream adds CPU/GPU and network load on top of
  tracking at the exact same time; test this on a real mid-range device before
  designing the UI around it, since a frame-rate drop during active tracking is a
  worse outcome than not having vision input at all; (2) **cost and bandwidth** —
  continuous image upload is the most expensive part of a realtime session by far;
  (3) **privacy** — frames may incidentally capture other visitors, not just the wall,
  which is a real consideration for a museum setting even though Anthropic-style data
  rules don't directly apply to OpenAI's API.

  **The recommended default is a different, lighter-weight design — send structured
  data about what's in view, not the raw image.** Since every POI's wall-relative
  position is already known data (the `wall_position.x_norm`/`y_norm` fields in
  §10's schema), it's straightforward to compute which POIs are currently inside the
  camera's field of view and which rough screen region each falls in (e.g. dividing
  the screen into an eight-part grid — upper-left/upper-centre/upper-right/etc. — and
  bucketing each visible POI's projected screen position into one of those regions).
  Sending that small, structured list ("currently visible: Castelo de São Jorge
  (upper-right), Sé de Lisboa (lower-centre)") to the model instead of a JPEG achieves
  the same "I can see what you're looking at" effect at a fraction of the bandwidth
  and cost, with no incidental-bystander privacy exposure, and — notably — **this
  same structured data is enough to build a fully scripted, non-AI version of
  camera-aware narration with no GPT dependency at all** ("You're currently looking
  toward the Castelo de São Jorge" triggered by simple geometry, not a model call).
  Build the geometry/region-bucketing logic once during Stage 2 or 3 when POI
  positions are first being projected to screen space anyway (§3, Stage 1's POI
  coordinate system work is the natural home for this), and treat sending raw camera
  frames to a vision-capable model as an optional, separately-tested upgrade on top of
  that foundation — not the only way to achieve the feature, and not the first one to
  attempt. If the raw-frame approach is attempted and does cause measurable
  performance problems, the documented fallback is to decouple it from continuous
  tracking entirely: pause AR localisation, capture a single snapshot, send that one
  frame with the question, get the answer, then resume tracking — a discrete
  "ask about what I'm looking at" action rather than a continuous video-like stream.

  **A cheaper, lower-risk alternative architecture, worth documenting even if not
  built**: drop speech-to-speech entirely and use device-native speech-to-text for
  input (free on both platforms) feeding a cheap, non-realtime text model, with the
  response displayed as on-screen text rather than synthesised back to speech. This
  is a fundamentally different cost profile — roughly an order of magnitude cheaper in
  aggregate (single-digit euros across many thousands of exchanges, rather than tens
  to hundreds of euros at the per-session rates above) because it avoids both speech
  synthesis and the premium realtime/speech-to-speech pricing tier entirely, at the
  cost of losing the natural-conversation feel and the native interruption-handling
  that make the speech-to-speech version distinctive. If Stage 7's time or budget is
  tight, this text-only version is a legitimate, much lower-risk way to still ship
  *some* conversational-guide capability rather than cutting the feature outright —
  decide between the two architectures based on time and budget remaining, not by
  default assuming the more expensive one.

  **Languages**: whichever architecture is used, OpenAI's models handle PT-PT, EN, ES,
  and FR natively with good accent handling, and a single, language-agnostic system
  prompt lets the model detect and respond in whichever language the visitor actually
  speaks — meaning the conversational guide ends up multilingual "for free" once
  built, in contrast to the scripted audio layer, where each language is real,
  separately-produced content. This is a genuine advantage of this layer worth noting
  in the thesis, not just a cost item.

  **An evaluation question this naturally enables**: if both the scripted audio layer
  and the conversational guide ship, add one optional item to Stage 7's interview or
  exit-survey protocol asking which visitors preferred — the scripted narrative, the
  conversational guide, or both — and why. Few published heritage-AR evaluations
  compare a scripted and a conversational layer head-to-head with the same visitors,
  so even a small amount of data here is a genuine, citable empirical finding, not
  just a satisfaction-rating footnote.
- *Earthquake/disaster simulation sequence* (flagged in §0 as a 2–4 week task on its
  own — scope it deliberately small): a 2D animated overlay rather than full 3D
  physics — screen shake, buildings cracking and fading via pre-composited imagery
  rather than simulated rubble, a fire/smoke particle overlay, a rumble-crack-crumble-
  silence sound design, and simple haptic feedback; 45–60 seconds, skippable. Narrative
  structure: a short "before" beat ("It's November 1, 1755…"), a numbers overlay during
  the event ("an estimated 30,000–40,000 deaths," "an estimated 85% of buildings
  destroyed"), and an "after" beat showing the Pombaline street grid where the old city
  stood.
- *360° interior views*, for a small named set of landmark buildings only: source
  imagery in order of effort — existing Google Street View indoor coverage where it
  exists, original photography where monument access can be arranged, and AI-generated
  renders stitched into a 360° panorama as a last resort, explicitly labelled in-app as
  an artistic reconstruction rather than a real photograph. Viewer: a standard 360°
  sphere-projection approach in Unity (a skybox or an inverted sphere with the panorama
  texture, gyroscope-controlled, pinch-to-zoom, tappable hotspots) — no special package
  is required for this.
- *3D building models*, only once tracking and evaluation are already complete and at
  least three weeks remain: 3–5 landmark buildings, sourced as a base asset (roughly
  €30–50 from the Unity Asset Store or a similar marketplace), customised in Blender,
  loaded via the same glTFast pipeline as the 3D model block. Target 60fps on an
  iPhone-13-equivalent device; reduce polygon count if that target isn't met. This is
  the single most time-expensive feature per unit of thesis value on this entire list —
  treat the "only if 3+ weeks spare" condition as a hard gate, not a suggestion.
- *Enhanced personalisation*: an implicit nudge (after three or more taps on the same
  category, e.g. maritime, offer a one-tap profile-switch suggestion); a favourites
  system (heart icon, saved list, reachable from the nav bar); an optional post-visit
  email summary of what was discovered, badges earned, and the quiz score.
- *Easter eggs* (small, low-cost, high-charm additions for a final-polish pass only):
  a hidden **"Found the Painter"** badge for finding **Gabriel del Barco's signature**
  on the panel — del Barco being the documented painter of the Grande Panorama tile
  panel itself, per the project's own prior research, which makes this egg a genuine
  historical detail rather than an invented one; a small animated fish easter egg when
  the camera is pointed at the depicted river; a "Secret Explorer" badge for finding
  five deliberately unlisted hidden hotspots.

---

## 4. Explicitly out of scope

Stated once, clearly, so none of it gets quietly re-attempted mid-project:

- **AR multiplayer / shared-tablet experiences** — network synchronisation cost is
  high relative to thesis value.
- **Machine-learning style-transfer effects** (e.g. via ML-Agents) — a visual gimmick,
  not a research contribution.
- **Body-pose interaction** (e.g. MediaPipe) — no clear value for this application.
- **Physical installations** (LED floors, smart lighting) — requires museum
  infrastructure partnership that doesn't exist.
- **Projection mapping** — a different project with a different technical core
  (already discussed at length in the thesis's own related-work chapter as a distinct
  category from mobile AR).
- **A web companion app** (e.g. via Unity WebGL) — a reasonable post-thesis idea, not
  a thesis-window task.
- **A 3D, AI-animated, lip-synced avatar guide** — explicitly different from, and not
  to be confused with, the 2D illustrated static-pose guide character that *is* in
  scope (Stage 3, item 6). This covers both a full real-time 3D animation pipeline
  (e.g. Unity Sentis plus a generative backend) and the lighter-weight alternative of
  a hosted lip-synced talking-head service (in the style of D-ID or HeyGen) generating
  short video clips on demand — both considered and rejected for the same reason: a
  cost-to-value ratio far worse than the 2D character plus the text/audio-driven GPT
  guide layer already planned, which together deliver most of the same perceived
  "character talks back to me" value at a fraction of the engineering cost and without
  a second, separate third-party API dependency and its own latency/cost profile to
  manage on top of the conversational layer's.
- **A day/night lighting cycle** — aesthetically interesting, no thesis value.
- **Room-scale or multi-surface tracking** (ceilings, multiple non-contiguous walls,
  volumetric wayfinding) — a different problem from the single continuous-surface case
  this thesis's schema and architecture are built around (§0, item 4). If room-scale
  support is wanted, it belongs to a follow-up project that redesigns the coordinate
  model from scratch, not to a late addition squeezed into this six-month window.

---

## 5. Content strategy

- Each wall needs a **small set of fully-realised "hero" POIs** (suggest 8–10 per
  wall) with real or carefully-written placeholder content across all profile
  variants — enough to demonstrate every block type convincingly and to run a
  meaningful evaluation session.
- Beyond the hero set, POIs exist primarily to prove **scale** (the marker/LOD/
  clustering system genuinely handling 100+ points), not content depth. Name,
  category, one short sentence is enough; AI-generated placeholder text is explicitly
  acceptable here and should be labelled as such internally so it is never later
  confused with verified content.
- Content-accuracy work (cross-referencing the NOVA FCSH building list, the Arquivo
  Municipal, and similar sources for full historical accuracy) is **decoupled from the
  framework timeline entirely**. It can proceed in parallel, later, or not at all
  within the thesis window, without blocking any stage above. Filling in the
  remaining content toward all ~150 POIs at minimal depth (name, status, one sentence
  only — not full per-profile content) is a long-term editorial project, explicitly
  not a thesis requirement.

---

## 6. Budget summary

Costs are scattered through the stages above; consolidated here so there is one place
to check against whatever personal or department budget actually exists, since this
plan does not currently state who pays for any of it.

| Item | Cost | When | Recurs? |
|---|---|---|---|
| Apple Developer Program | €99 | Stage 1 | Annually |
| Google Play Developer account | €25 | Stage 1 | One-time |
| Immersal Pro (only if the free tier's 100-images/map cap or commercial-use terms force an upgrade — see Stage 1) | $99/month | Contingent, decide in Stage 1 | Monthly while needed |
| Google Cloud TTS (WaveNet or Neural2, whichever tier PT-PT actually has — see Stage 2) | $4–16 per million characters; free allowance covers this project's ~12 scripted clips regardless of tier | Stage 2 onward | Pay-as-you-go, expect near-€0 at this scale |
| Google Maps Static API | Free tier; €20 billing alert | Stage 1 | Monthly, unlikely to trigger |
| Firebase | Free tier | Stage 1 | — |
| Human voice actor (optional upgrade) | ~€250 flat | Optional, not required for Stage 2 | One-time |
| Guide-character illustration (concept + 4–6 base poses, commissioned) | €150–300 | Stage 3 | One-time |
| GPT conversational layer (stretch) | ~€0.30–0.60 per 15-min session for the speech-to-speech version; an order of magnitude cheaper (~€5–10 total across many sessions) for the text-only fallback architecture — see Stage 7's stretch-features notes in §3 | Stage 7, if attempted | Per session — re-verify cost at implementation time |
| 3D landmark base assets (stretch) | ~€30–50 | Stage 7, if attempted | One-time |
| GitHub Pages (privacy policy host) | Free | Stage 6 | — |
| Mid-range Android test device | Borrow if possible, otherwise ~€100–150 | Stage 1 | One-time |

Running total if every guaranteed line is incurred: roughly €500–600 one-off (the
illustrator commission is the single largest discretionary line — confirm it's
actually wanted before committing), with real ongoing costs (TTS, Maps Static API)
expected to stay near €0/month at this
project's scale per the verified pricing above. The one line that could meaningfully
change this further is Immersal Pro ($99/month) if Stage 1's licensing check concludes the
free tier won't work — confirm that early, since it's the only contingent monthly
cost large enough to matter. Confirm the whole table against an actual funding source
(personal budget vs. department support) before Stage 1 starts, rather than
discovering a blocker at the Apple Developer sign-up step.

---

## 7. Risk register

| Risk | Likelihood | Impact | Mitigation |
|---|---|---|---|
| AR tracking proves unreliable | Medium | High | Implement a QR-code fallback as Plan B: place a small number of QR markers (e.g. four) at intervals along the wall purely to re-anchor the coordinate system when visual tracking loses confidence — a visitor scans the nearest code to recover, rather than the session failing outright |
| Block system scope creeps before Stage 4 freezes the schema | Medium | High | Hard rule: no new block type added after Stage 2 ends without a documented reason |
| Node Graph circuits introduced too early, with insufficient validation | Medium | Medium | Deferred explicitly to "if time allows"; ship Gauntlet/Hub-and-Spoke first |
| Mural wall's lack of a "status" axis breaks a UI element that assumed one exists everywhere | Low (now caught in design) | Medium | Status axis is optional and wall-defined from the very first schema draft, never assumed present |
| Wizard becomes load-bearing rather than optional | Low | High | Every Wizard feature must have a manual equivalent, restated at Stages 1 and 5 |
| Automation (Stage 4) starts before the schema has stabilised | Medium | Medium | Sequencing is the mitigation — Stage 4 is deliberately placed after Stages 2–3 |
| Grande Panorama access remains unavailable throughout | High | Low | Chafariz Velho and the mural wall are sufficient for every stage above; no claim in the thesis depends on Panorama access |
| Content quality silently degrades as POI count grows | Medium | High | No new POI is added until the existing ones' content is complete — a standing rule, not a one-time check |
| Analytics instrumentation is incomplete at evaluation time | Medium | High | Instrument every event as its feature ships (Stages 1–3); never retrofit analytics right before an evaluation session — by the time it's needed, it's too late to add |
| 3D model block (glTFast) underperforms on mid-range devices | Low–Medium | Medium | Defer-agent loading already planned; fall back to a 2D image per-POI if needed |
| GPT conversational layer costs more or behaves worse than expected | Low | Low | Hard cap at five questions/session, a monthly spend alert, and a kill switch; the scripted audio layer is fully sufficient on its own regardless |
| Stage 7 must fit both the formal evaluation sessions and every stretch feature into 4 weeks | High | High | Hard triage rule: the evaluation core (analytics, SUS/UEQ-S, exit survey, knowledge check, sessions) is never cut; stretch features are attempted strictly in the priority order already given, and any one not started by the start of week 25 is dropped outright, not compressed |
| University ethics/data-collection approval is not yet confirmed and gates real-visitor sessions | Medium | High | Submit the approval request as early as Stage 3, once the analytics taxonomy and the consent text from Stage 1 are both fixed — not at Stage 7, where approval lag would consume evaluation time directly |
| A named third-party API or model (e.g. the stretch GPT layer) is renamed, repriced, or deprecated before Stage 7 actually starts | Medium | Low–Medium | Treat the specific model name in Stage 7 as a placeholder to re-verify at implementation time, not a locked decision — the same discipline already applied to the tracking-platform choice in Stage 1 |
| Mural wall's hero-POI list and field-work remain undefined past Stage 1 | Medium | Medium | Close this out explicitly before Stage 2 starts (§1); it is a half-day task, not a research problem |
| Audio playback doesn't recover correctly after a Bluetooth headphone connect/disconnect on Android, a confirmed open platform issue rather than a hypothetical one | Medium | Medium | Test explicitly with real wireless headphones on the actual Android devices in Stage 1's matrix during Stage 2's audio-block work (§3); do not assume `AudioSettings.OnAudioConfigurationChanged` alone resolves it, since developer reports describe it not firing reliably on Android |
| Immersal free tier's commercial-use terms and 100-image/map cap turn out to block the project as designed | Low–Medium | Medium | Resolve directly against Immersal's current EULA in Stage 1 (see Stage 1's Immersal section); Pro is $99/month if needed and is a small, well-understood fallback, not a redesign |
| Wall access (especially the Panorama) is real but never confirmed in writing, leaving the thesis citing an assumption rather than a documented basis | Low–Medium | Low–Medium | Get even an informal email confirmation for each wall during Stage 1 (§1) and keep it on file — distinct from the access-availability risk above, this is about having a citable paper trail for whatever access level actually exists |
| The NOVA FCSH building-identification list (the actual source of "150 POIs") is not yet in hand, and everything content-related is blocked on it | Medium | High | Obtain this list in the first days of Stage 1 (§1) — treat it as a hard Stage 1 prerequisite, not a Stage 2 nice-to-have, since hero-POI selection, content writing, and the scale-proving LOD/clustering work in Stage 2 all depend on it existing first |
| Framework engineering drifts into building speculative generality the thesis doesn't actually need | Medium | Medium | Scope-discipline rule: every framework feature should be traceable to something already committed to in this work plan (§3's stage-by-stage features, §8's RQ mapping); anything that isn't — however technically interesting — is a framework feature for *after* the thesis, not during it |
| Entry-point resolution for circuits (§3, Stage 3) has an edge case (nearest-POI ties, a visitor standing between two sequence members) or the `completion_rule`/`traversal_rule` fix is implemented inconsistently across circuit types | Low–Medium | Medium | Test explicitly by physically starting each Gauntlet-shaped circuit from at least three different points along the wall (the actual start, the middle, near the end) during Stage 3's own testing, not only from the authored beginning — this is cheap to test and easy to silently get wrong since the "start from index 0" path will still work and mask a broken non-zero entry path in a quick smoke test |
| Ambient pivot's hysteresis (the 10–15s sustained-engagement threshold) is tuned wrong in practice — either too twitchy (arrow flickers during normal browsing) or too sluggish (feels unresponsive) — or the Butterfly Prompt's cross-circuit reverse lookup and the paused-circuit-state stack (§3, Stage 3) have a bug that only shows up after several detours in a row | Medium | Medium | Treat the threshold as a tunable value to adjust after the first few real walk-throughs, not a number to lock in from this document alone; specifically test accepting a *second* Butterfly Prompt while already mid-detour from a first one, since that is the edge case most likely to be skipped in a quick manual test but most likely to actually occur once real visitors are wandering freely |

---

## 8. Mapping back to the thesis's research questions

| Stage | RQ(s) served |
|---|---|
| 1 | RQ1 (tracking configuration) |
| 2 | RQ2 (scalability/legibility — block system, LOD, lapse mechanism) |
| 3 | RQ3 (engagement patterns), RQ4 (personalisation effectiveness) |
| 7 | RQ5 (immediate knowledge gain), plus the evaluation evidence for RQ3/RQ4 |
| 4–6 | Not RQ-bearing directly — these are the *generalisability* contribution itself (the three-wall proof, plus the time-to-new-wall measurement added to Stage 6 below), supporting the thesis's overall framework claim rather than any single RQ |

---

## 9. Glossary

Defined once here because the rest of this document uses these terms without
re-explaining them on every use.

- **Wall** — one continuous, traversable physical surface this framework targets
  (curved, U-shaped, or flat; not a room — see §0, item 4). The three in scope: the
  **Panorama** (Grande Panorama de Lisboa, U-shaped, opportunistic access), **Chafariz**
  (Chafariz Velho, circular, the guaranteed primary development surface), and the
  **mural** (Alto de Santa Catarina, flat, guaranteed public access).
- **POI** — Point of Interest: one addressable, taggable thing on a wall (a depicted
  building, a historical scene, a painted animal) with its own entry in `config.json`.
- **Hero POI** — one of the small set (8–10) of POIs per wall that get full, real
  content across every block type and every visitor profile, as opposed to the
  remaining POIs which exist mainly to prove scale (§5).
- **Block / block type** — a registered `TileStoriesUIBlock` renderer for one kind of
  POI content. Exactly six exist: text/header, image, audio, video, 3D model, map.
  "The audio block" means the block-type implementation; "an audio block" (lowercase)
  means one instance of it attached to one POI. Registration is public API
  (`TileStoriesBlockRegistry.Register(...)`, §3 Stage 2) so a downstream project can
  add a new block type without forking the framework.
- **"Lapse" state mechanism** — **not a block**, despite the casual shorthand "lapse
  block" appearing in earlier notes and still surfacing informally in conversation —
  a POI-level property (`lapse_states` in §10's schema) that changes *which* blocks
  and markers are currently visible for a given epoch/criteria state, concretely a
  four-epoch historical timeline on the Panorama but built generically enough to mean
  any criteria-driven state change (e.g. the mural's season/time-of-day variant) —
  "lapse" deliberately does not mean "year." See Stage 2's architectural note (§3) for
  why this distinction is implemented, not just terminological.
- **Category axis** — a wall-defined, free-form tag on a POI (e.g. the Panorama's
  religious/royal/military/etc.; the mural's bird/mammal/amphibian/insect). Never a
  fixed framework enum.
- **Status axis** — a second, *optional* wall-defined axis. For the Panorama this is
  earthquake destruction, represented as `status_pct: float (0–100)` guarded by
  `has_status: bool` — refined in Stage 2.3 from an earlier 4-state-enum sketch
  (unchanged/modified/destroyed/unknown) into a continuous scale, which renders more
  legibly across the ring/fade/badge marker styles Stage 2.3 built. The old enum's
  "unknown" value survives as its own bool, `status_unknown` — a POI can have
  `has_status: true` and a real historical unknown fate, which is a genuinely
  different state from `has_status: false` (this wall doesn't track destruction at
  all) and is rendered as its own distinct visual (a neutral "?" badge), never
  conflated with either the "no axis" case or a known percentage. Optional because
  the mural has no equivalent axis at all — this is the concrete reason status must
  never be assumed present (§1). See `_2_2_Marker_Design.md` for the full rendering
  design and the reasoning behind the enum→percentage change.
- **Zone-based tracking** — splitting one wall's reference imagery into 4–6 overlapping
  zone images so the tracker can fail over to an adjacent zone when confidence drops in
  the current one, rather than losing the lock entirely (Stage 1).
- **Scanning state** — the on-screen indicator shown while Immersal's VPS is actively
  trying to localise against the wall (a real 1–3 second wait, unlike near-instant
  single-image AR). Exists in a plain, functional form from Stage 1; upgraded to a
  polished animated version in Stage 7.
- **Time-to-first-lock** — the elapsed time from the camera becoming active to the
  first successful Immersal localisation. Logged from Stage 1 onward as both a
  technical-evaluation metric and the baseline Stage 7's UX polish is measured
  against.
- **Structured camera-awareness** — the recommended, lower-cost alternative to
  sending raw camera frames to a vision-capable model for "camera-aware narration":
  computing which POIs are currently in the camera's field of view from their already-
  known `wall_position` data and sending that small structured list (optionally
  bucketed into an eight-part screen-region grid) instead of an image (Stage 7).
- **LOD (level-of-detail) / marker-density management** — distance-based rules that
  reduce how many markers render at once as the viewer moves away from the wall (Stage
  2, item 7) — the single most load-bearing legibility decision in the project (RQ2).
- **Circuit** — a named, ordered sequence of POIs forming a guided sub-experience (e.g.
  "Earthquake 1755: Before & After"). Named per Ashwell's taxonomy (§2): **Gauntlet**
  (strict linear order), **Hub-and-Spoke** (return to a central point between
  branches), **Branch-and-Bottleneck**, **Parallel Tracks**, and **Node Graph**
  (free-form graph, deferred — highest validation burden).
- **Entry-point resolution** — finding which member of a circuit's `poi_sequence` is
  nearest the visitor's current wall position and starting the circuit's state
  machine there, instead of always at index 0. Reuses the same nearest-POI
  computation already used for free-roam wayfinding (Stage 3, item 3); the
  alternative considered and rejected was authoring a fixed starting index per
  circuit, which doesn't reflect where a visitor actually is on a 23-metre wall.
- **Traversal rule** — the per-`circuit_type` setting (`bidirectional` for
  Gauntlet/Branch-and-Bottleneck, irrelevant for Hub-and-Spoke/Parallel Tracks)
  governing what unlocks once entry-point resolution places a visitor mid-sequence.
  Works together with `completion_rule: "visit_all"` (set-membership, not an
  index-walk) — without that completion-rule fix, entry-point resolution only
  changes where the arrow points, not whether the circuit can actually be completed
  from a non-zero starting point.
- **Ambient pivot** — the continuous, hysteresis-gated version of entry-point
  resolution: re-running the same nearest-unvisited-target computation mid-circuit
  when a visitor sustains engagement (≥10–15s) with an out-of-sequence POI, rather
  than only once at the circuit's start. Deliberately *not* an event-driven
  "recalculating" interruption — the whole point is that it should not announce
  itself the way car-navigation rerouting does (§3, Stage 3's circuits item).
- **Butterfly Prompt** — a dismissible toast offering to switch circuits when
  sustained engagement is with a POI belonging to a *different* circuit's
  `poi_sequence`. Named after Véron & Levasseur's "butterfly" visitor type; a
  deliberate, bounded injection of visitor agency into an otherwise
  authorial-intent-leaning sequential circuit (§3, Stage 3).
- **`captured_position`** — a real 3D point under the XR Space, recorded by
  physically standing at a POI while Immersal-localised (adapting Immersal's own
  Content Placement Sample, §3 Stage 1). Takes precedence over the authored
  `wall_position` whenever both are present for the same POI (§10).
- **`content_by_profile`** — the JSON key holding profile-variant copy
  (tourist/student/academic/child) for a single POI, resolved by existing block
  renderers rather than separate block types per profile (Stage 2, item 9).
- **`wall_id` / `config_schema_version`** — the two fields every wall's `config.json`
  carries from the very first draft: which wall this is, and which version of the
  schema it was written against (for forward compatibility once Stage 4 formalises
  the schema).
- **`WallConfigAsset`** — the baked Unity `ScriptableObject` that Stage 4's compiler
  produces from a wall's JSON; at runtime, code reads this, never raw JSON.
- **The Wizard** — the custom `EditorWindow` built in Stage 5 that wraps the
  validator/compiler/documentation into one-click actions. Always optional, per §2's
  MARS lesson — never load-bearing.
- **UPM package** — the Unity Package Manager package (`TileStoriesFramework`)
  Stage 6 extracts the proven core into, installable via git URL or `.tgz`, with the
  standard `Editor`/`Runtime`/`Tests`/`Samples~`/`Documentation~` layout.
- **DSR / DBR / RtD** — Design Science Research / Design-Based Research /
  Research-through-Design: the thesis's own methodological framing (full citations in
  the thesis bibliography, not repeated here), under which an evaluated, working
  artifact is itself a valid research contribution.
- **SUS / UEQ-S** — System Usability Scale (10 items) / Short User Experience
  Questionnaire (8 items): the two standard, published evaluation instruments this
  plan administers in Stage 7 (§3), per the thesis's own evaluation chapter.
- **"Framework" vs. "app"** — this document's central distinction (§0, item 3): the
  *framework claim* (this architecture generalises across heritage walls) is proven by
  Stages 1–3 alone; the *framework product* (an installable package a third party
  could use) is what Stages 4–6 additionally build, and is valuable but not load-bearing
  for the thesis's central contribution.

---

## 10. Appendix: full config schema reference

Stage 1 introduces `config.json` with just enough structure to start (text/header
fields and one POI). This appendix shows **one wall's config with every block type,
plus circuits and badges, populated at once** — not a new design decision, just the
same fields from across Stage 2–3 assembled into a single concrete reference so an
implementing agent never has to guess a field name. Treat this as illustrative, not
literally Stage 1's starting file — build up to this shape incrementally, exactly as
§3 describes, and only formalise it into an actual schema in Stage 4. `config.json` is
one file among several inside each wall's own folder — it sits alongside that wall's
Immersal `map.bytes`, an optional occlusion `mesh.glb` (§2), and the `MediaAssets`
subfolders the `media` fields below reference by relative path; see §12 for the full
per-wall folder layout.

```json
{
  "wall_id": "grande_panorama",
  "config_schema_version": "0.3",
  "geometry": { "type": "u_shaped", "length_m": 23 },
  "pois": [
    {
      "id": "castelo_sao_jorge",
      "name_pt": "Castelo de São Jorge",
      "name_en": "St. George's Castle",
      "category": ["military", "power"],
      "status_pct": 60, "has_status": true, "status_unknown": false,
      "wall_position": { "x_norm": 0.72, "y_norm": 0.45 },
      "captured_position": { "x": 1.84, "y": 0.92, "z": -3.41 },
      "summary_pt": "Curta descrição de 2 frases, registo turístico.",
      "summary_en": "Two-sentence summary, tourist register.",
      "content_by_profile": {
        "tourist": "Texto curto e acessível...",
        "student": "Texto com mais contexto histórico e factos verificáveis...",
        "academic": "Texto com referências e nuance historiográfica...",
        "child": "Texto simples, frases curtas, um facto divertido..."
      },
      "coordinates_today": { "lat": 38.7139, "lng": -9.1334 },
      "media": {
        "images": ["castelo_1.jpg", "castelo_2.jpg"],
        "audio_pt": "castelo_pt.mp3",
        "audio_en": "castelo_en.mp3",
        "audio_captions_pt": "castelo_pt.vtt",
        "audio_captions_en": "castelo_en.vtt",
        "video": "castelo_clip.mp4",
        "model_3d": "castelo.glb"
      },
      "lapse_states": {
        "pre_1755": { "visible": true, "marker_colour": "default" },
        "earthquake": { "visible": true, "marker_colour": "red", "effect": "shake_debris" },
        "pombaline": { "visible": true, "marker_colour": "grey" },
        "today": { "visible": true, "marker_colour": "default" }
      },
      "quiz": [
        {
          "q_pt": "Este edifício sobreviveu ao terramoto de 1755?",
          "q_en": "Did this building survive the 1755 earthquake?",
          "options_pt": ["Sim, intacto", "Não, foi destruído", "Parcialmente"],
          "options_en": ["Yes, intact", "No, destroyed", "Partially"],
          "correct": 2,
          "explanation_pt": "O castelo sofreu danos significativos mas foi reconstruído.",
          "explanation_en": "The castle suffered significant damage but was rebuilt."
        }
      ]
    }
  ],
  "circuits": [
    {
      "id": "earthquake_1755",
      "type": "gauntlet",
      "title_pt": "Terramoto de 1755: Antes & Depois",
      "title_en": "Earthquake 1755: Before & After",
      "estimated_minutes": 15,
      "profile_fit": ["tourist", "student"],
      "poi_sequence": ["castelo_sao_jorge", "se_lisboa", "paco_ribeira"],
      "entry_point_strategy": "nearest_flexible",
      "completion_rule": "visit_all",
      "traversal_rule": "bidirectional",
      "auto_open_cards": false
    }
  ],
  "badges": [
    {
      "id": "lisboa_religiosa",
      "title_pt": "Lisboa Religiosa",
      "title_en": "Religious Lisbon",
      "icon": "🏛️",
      "trigger": { "type": "first_visit_of_category", "category": "religious" }
    },
    {
      "id": "sobrevivente_1755",
      "title_pt": "Sobrevivente 1755",
      "title_en": "Survivor of 1755",
      "icon": "⚡",
      "trigger": { "type": "status_threshold", "status_pct_gte": 80 }
    },
    {
      "id": "explorador",
      "title_pt": "Explorador",
      "title_en": "Explorer",
      "icon": "🗺️",
      "trigger": { "type": "first_circuit_completed" }
    },
    {
      "id": "conhecedor",
      "title_pt": "Conhecedor",
      "title_en": "Connoisseur",
      "icon": "⭐",
      "trigger": { "type": "poi_count_threshold", "count": 25 }
    }
  ],
  "fun_facts": [
    {
      "id": "fact_01",
      "text_pt": "A Praça do Comércio foi construída sobre escombros do terramoto.",
      "text_en": "Praça do Comércio was built on rubble from the earthquake."
    }
  ]
}
```

Field notes that matter more than they look:
- `category` and `status` are wall-defined, never validated against a
  framework-level fixed enum (§1's mural-wall finding). `status` is entirely
  **absent**, not empty, on walls where it doesn't apply (the mural). Note:
  `category` is sketched here as an array (multi-tag POIs, e.g. `["military",
  "power"]`) but the actual current implementation and Stage 2.3's
  `CategoryPalette` both use a single string — build up to the array form
  incrementally per this section's own philosophy; when it's built,
  `CategoryPalette.ResolveColor` needs a rule for which tag decides the fill
  colour when a POI has more than one.
  **Serialisation safety (Stage 4 bake implementation)**: `status_pct` in
  `WallConfigData` and `WallConfigAsset` must be backed by an explicit
  `has_status: bool`, never a plain float that silently reads as 0 ("Intact")
  when the JSON key is missing. Unity's serialiser silently initialises missing
  numeric fields to 0 — this would cause the mural's bird POIs to render as
  "0% Intact" (a fully-opaque gold ring, per Stage 2.3's actual ramp — see
  `_2_2_Marker_Design.md`), which is wrong, not from no ring/badge at all.
  `MarkerView` must check `has_status` before drawing any ring/badge whatsoever.
  A third bool, `status_unknown`, marks a POI whose fate is a genuine historical
  unknown — distinct from `has_status == false` — rendered as its own neutral
  "?" badge regardless of `marker_style`. Test this explicitly by running the
  mural's config through the baker and confirming `has_status == false` on
  every POI in the resulting `.asset` file — it is the cheapest bug to add a
  test for and the most embarrassing one to discover live.
- `wall_position` (`x_norm`/`y_norm`) is the human-authored, draftable-from-a-photo
  value content writers reason about before any field visit; `captured_position` (a
  real 3D point under the XR Space, §3's Stage 1 coordinate-system note) is the
  ground-truth value recorded during an actual field session with the
  Immersal-localised app. **When `captured_position` is present, the renderer must
  use it and ignore `wall_position`** — it exists specifically because interpolating
  from `wall_position` alone is a lower-accuracy fallback, never something to prefer
  once a precise capture exists for that POI. Expect `captured_position` to be
  missing on most of the "scale-proving" bulk POIs and present on all ten hero POIs
  per wall at minimum.
- `lapse_states` is keyed by whatever epoch/criteria names the wall actually uses — for
  the mural, these keys would be seasons or times-of-day, not `pre_1755`/`earthquake`/etc.
  This is a completely different mechanism from Stage 2.3's marker rendering system,
  despite both involving a "marker colour": `lapse_states` controls *whether a marker
  is visible at all*, for a given epoch; Stage 2.3's `marker_style`/`category_styles`
  govern *how one visible marker looks*. They compose (a marker can be visible in the
  `earthquake` epoch AND rendered at 60% status via `outline_same_hue`) — one doesn't
  replace or duplicate the other.
- `media` fields are all optional per POI — a POI with only a `summary` and no audio,
  video, or 3D model is valid; not every POI needs every block.
- `content_by_profile` keys are fixed (tourist/student/academic/child) since the
  profile *set* is a framework decision, even though the *content* per profile is
  wall-and-POI-specific data.
- Every quiz question carries its own `explanation_pt`/`explanation_en` — Stage 3's
  gamification design explicitly shows an explanation after every answer, correct or
  not, so this field is not optional in practice even though the schema doesn't
  enforce it until Stage 4.
- `circuits` and `badges` live at the wall level, not nested inside individual POIs,
  since a circuit references POIs by `id` rather than owning them. Note: `badges`
  here are gamification achievements (unlocked by a `trigger`, shown in a
  profile/collection screen) — unrelated to `MarkerStyle.Badge` (Stage 2.3's small
  corner-chip status renderer on a POI marker itself). Same English word, two
  unrelated concepts; consider renaming `MarkerStyle.Badge` to
  `MarkerStyle.CornerBadge` if this reads ambiguously once both systems exist
  side by side and get discussed together.
- The `sobrevivente_1755` badge's trigger changed from `first_visit_of_status:
  "destroyed"` (exact string match) to `status_threshold: {status_pct_gte: 80}`
  because Stage 2.3 changed `status` from a matchable enum string to a continuous
  `status_pct` — whichever stage actually builds the trigger engine needs a
  threshold-comparison rule type, not just equality matching, for any trigger keyed
  on status.
- `completion_rule: "visit_all"` and `traversal_rule: "bidirectional"` are not
  cosmetic — they are what makes flexible circuit entry points actually work rather
  than just relocating the wayfinding arrow. `completion_rule` must be a
  set-membership check (every `poi_sequence` member visited, any order) for this to
  function; `traversal_rule` controls whether entering mid-sequence unlocks both
  directions (`bidirectional`, the default for Gauntlet/Branch-and-Bottleneck) or is
  irrelevant because nothing is locked to begin with (Hub-and-Spoke/Parallel Tracks).
  See Stage 3's circuits item (§3) for the full design reasoning, the rejected
  alternatives, and why this is not authored as a starting-index field at all — the
  actual entry point is computed at runtime from the visitor's position against each
  target's `wall_position`, not declared in this file.
- `badges[].trigger.type` is an open set, not a single hardcoded check — the four
  shown above (`first_visit_of_category`, `first_visit_of_status`,
  `first_circuit_completed`, `poi_count_threshold`) already cover every badge named in
  Stage 3's gamification list; design the trigger-evaluation code as a small
  dispatcher over `type`, not a chain of if/else checks for one hardcoded badge at a
  time, so adding a fifth trigger type later doesn't mean touching every existing
  badge — this is also the dispatcher to extend, not duplicate, if a future
  prerequisite-style POI-unlock condition is ever needed (§3's circuits item explains
  why this is the right reuse rather than building a third mechanism).
- `fun_facts` is wall-level and POI-independent, since the "Did You Know?" feature
  (Stage 3) surfaces a random fact on a timer/tap-count basis rather than attaching to
  any specific POI — keep it as its own top-level array, not folded into any one POI's
  data, or the random-surfacing logic has nowhere clean to read from.
- This is the shape Stage 4 should write an actual JSON Schema / validator against —
  by then, real use across three walls will have surfaced any fields this illustrative
  version is still missing.

---

## 11. Appendix: environment setup, step by step

Stage 1's "Tooling and accounts" paragraph names what's needed; this appendix is the
literal order of operations for an implementing agent starting from zero, so no step
is skipped or done out of order.

1. **Install Unity Hub**, then install **Unity 6.3 LTS** through it specifically (not
   whatever version the Hub defaults to) — include the **Android Build Support**
   module (with OpenJDK and Android SDK & NDK Tools sub-modules ticked) and the
   **iOS Build Support** module at install time, not added later.
2. **Create accounts, in this order** (each is free to create; paid tiers are decided
   later, not now):
   - GitHub account + new private repo for the project.
   - Firebase account (console.firebase.google.com) + new Firebase project.
   - Google Cloud account (most easily the same Google identity as Firebase) + new
     Google Cloud project, with billing enabled (required even to stay on free tiers)
     and a billing alert set immediately, before enabling any paid API.
   - Immersal Developer Portal account (developers.immersal.com) — see Stage 1's
     Immersal section for the licensing decision to make alongside this.
   - Apple Developer Program enrolment (needs a payment method; approval can take
     days) and a Google Play Console developer account.
   - OpenAI account, **only when Stage 7's stretch conversational layer is actually
     started**, not before — there is no reason to hold an API key for a feature that
     may never be built.
3. **Set up the Unity project**: new 3D (Mobile) project on Unity 6.3 LTS; immediately
   add a `.gitignore` for Unity (the standard `Library/`, `Temp/`, `Obj/`, `Build/`,
   `Logs/`, `.vs/`, `*.csproj`, `*.sln` exclusions — use GitHub's own `Unity.gitignore`
   template rather than writing one from scratch) before the first commit, or the
   repo will fill with generated files.
4. **Install packages via Package Manager**, in this order (later ones sometimes
   depend on earlier ones being present): AR Foundation → ARCore XR Plugin → ARKit XR
   Plugin → TextMesh Pro → Immersal SDK (via "Add package from git URL," the
   `imdk-unity` repository) → Unity glTFast (`com.unity.cloud.gltfast`) → 2D Animation
   (`com.unity.2d.animation`), only if the guide character ends up using bone-rigged
   skinning rather than a simple sprite-swap (the simpler approach is preferred, per
   §3 Stage 3 item 6 — install this package only if that decision is actually made).

   **A note on Immersal SDK versioning**: the `imdk-unity` repository publishes no
   tagged releases — the package is always installed from the HEAD of the `main`
   branch (current version as of this writing: 2.3.0). To prevent a future
   Package Manager update from silently pulling a breaking change mid-project: after
   the first successful installation, open `Packages/packages-lock.json`, find the
   `com.immersal.core` entry, and copy its `hash` value into a comment in
   `Docs/decisions.md`. If a later update causes problems, pin back to that commit
   by editing `manifest.json` to use
   `"https://github.com/immersal/imdk-unity.git#<hash>"` instead of the bare URL.

4b. **Create the first AR scene and wire Immersal — do this once in Stage 1 Week 1,
   for Chafariz as the first development surface. This entire step is App domain
   (lives inside `Assets/Apps/Chafariz/`). Framework domain does not exist yet.**

   **Framework domain vs App domain — what each owns from day one:**
   - **App domain** (`Assets/Apps/[WallName]/`) owns everything specific to one wall:
     `config.json`, the Immersal `map.bytes`, the wall's Unity scene, and the Immersal
     scene hierarchy described below.
   - **Framework domain** (`Assets/Framework/`) owns the wrapper code on top of
     Immersal (`ImmersalWallTracker.cs`, `IWallTracker.cs`), all POI logic, all UI
     blocks, and all business logic. It never references a specific map file, map ID,
     wall name, or developer token.
   - **The developer token** is neither Framework nor App domain — it is
     personal to your Immersal account. It must never be committed to Git.
     The Stage 1 hand approach and the Stage 5 Wizard approach are both described
     below.

   **Step-by-step to create the first working Immersal AR scene:**

   i. In Unity's Project window, navigate to `Assets/Apps/Chafariz/`. Right-click
      inside it → **Create → Scene**. Name it `ChafarizScene`. Open it.
      Delete the default Main Camera and Directional Light GameObjects that Unity
      adds by default — AR Foundation provides its own camera.

   ii. In the Hierarchy, right-click → **Create Empty**, name it **`AR Session`**.
       In the Inspector, click **Add Component** → search `AR Session` → add it
       (from the AR Foundation package). This component manages the device AR
       session lifecycle (camera activation, tracking state).

   iii. Create an empty GameObject named **`XR Origin`**. Add the **`XR Origin`**
        component (AR Foundation). Unity automatically creates a child hierarchy:
        `Camera Offset → AR Camera`. Expand it in the Hierarchy. Confirm the
        `AR Camera` child has both **`AR Camera Manager`** and **`AR Camera
        Background`** components. If either is missing, select `AR Camera` and
        add it manually via Add Component.

   iv. Create an empty GameObject named **`ImmersalSDK`**. Add the **`ImmersalSDK`**
       component (Immersal Core package — search "ImmersalSDK" in Add Component).
       This is Immersal's own singleton MonoBehaviour. In its Inspector field
       **Developer Token**, paste your token string.

       To obtain your token: log in at `developers.immersal.com`, click your account
       name (top-right corner), and copy the **Developer Token** string shown there.
       This is the same token the Immersal Mapper mobile app uses to upload maps.

       **Keeping the token out of Git — two approaches:**
       - *Stage 1 hand approach (acceptable now)*: paste the token directly in the
         Inspector. Before every Git commit, clear this field, commit, then re-paste
         after checkout. Or: do not stage the scene file in commits where the token
         is present (`git add -p` to selectively stage without the scene). This is
         manual but fine for solo development through Stages 1–4.
       - *Stage 5 Wizard approach (the clean long-term solution)*: the
         `TileStoriesSettings` ScriptableObject (already planned in
         `Assets/Framework/Runtime/Core/TileStoriesSettings.cs`) gets a
         `public string ImmersalDeveloperToken` field. The **`.asset` instance** of
         this ScriptableObject is added to `.gitignore` (the `.cs` class file is
         committed normally; only the data file containing the actual token value is
         gitignored). A one-line bootstrapper reads the asset at startup and assigns
         `ImmersalSDK.Instance.developerToken = settings.ImmersalDeveloperToken`.
         The Stage 5 Wizard creates this asset and prompts for the token on first
         open. This is the correct long-term solution; the hand approach is fine
         until Stage 5.

   v. Place the Immersal map file for this wall: download the `.bytes` file from
      the Developer Portal (Maps → select your map → Download button). Place it at
      `Assets/Apps/Chafariz/map.bytes`. Also download the textured `.glb` mesh if
      available and place it at `Assets/Apps/Chafariz/mesh.glb` — it is not used for
      localisation but is useful for visually verifying map coverage. The `.ply`
      point cloud file is not needed and can be ignored.

   vi. Create an empty GameObject named **`AR Space`**. This is the container whose
       transform Immersal continuously updates to align virtual content with the
       physical wall — everything placed as a child of `AR Space` automatically
       appears at the correct real-world position once localised. Add the **`XR Map`**
       component (Immersal Core) to `AR Space`. In the `XR Map` Inspector:
       - **Map Id**: enter the integer map ID. This is the number at the start of
         the downloaded filename (e.g. if the file is named `146587-Chafariz.bytes`,
         the map ID is `146587`). It is also displayed on the map's detail page in
         the Developer Portal.
       - **Map File**: drag `Assets/Apps/Chafariz/map.bytes` into this slot.
       - **Localization Method**: choose **On Device**. This loads the map into
         device memory and performs all matching locally, enabling fully offline
         operation. Do not use Server mode — it requires internet connectivity and
         consumes Immersal cloud API quota.

   vii. Create an empty GameObject named **`Localizer`**. Add the **`Localizer`**
        component (Immersal Core). This drives the continuous localisation loop: it
        feeds camera frames to the on-device matcher and, on a successful match,
        fires the pose update that moves `AR Space` to align with the physical wall.
        Default settings are correct for a first test; leave them unchanged.

   The final scene hierarchy (save this as a reference):
   ```
   ChafarizScene
   ├── AR Session
   ├── XR Origin
   │   └── Camera Offset
   │       └── AR Camera        ← player's view; AR Foundation manages this
   ├── ImmersalSDK              ← developer token here — NEVER commit this value
   ├── AR Space                 ← Immersal continuously moves this to match the wall
   │   └── [POI Anchors]        ← children added here in Stage 1; move with the wall
   └── Localizer               ← drives the localisation loop
   ```

   **How the framework layer connects to this App-domain scene**: the framework's
   `ImmersalWallTracker.cs` finds the `Localizer` component at runtime via
   `FindFirstObjectByType<Localizer>()` and subscribes to its pose-update callback.
   It translates that callback into the framework's own `OnWallLocalised(Pose)` event
   that `WallSession.cs` listens to. Everything above that layer — POI placement, UI,
   block system — never references `ImmersalSDK`, map IDs, or scene names directly.
   The framework/app boundary is enforced here in practice: cross it only in
   `ImmersalWallTracker.cs`, and only via the `IWallTracker` interface.

5. **Wire up Firebase**: in the Firebase console, register the app's Android package
   name and iOS bundle ID, download `google-services.json` and
   `GoogleService-Info.plist`, and place them per Firebase's standard Unity
   instructions; then add the Firebase Unity SDK (Analytics at minimum) via the same
   Package Manager git-URL/`.tgz` flow as Immersal, not the older `.unitypackage`
   import unless a specific package isn't yet available that way.
6. **Generate API keys in Google Cloud Console**: enable the *Maps Static API*
   specifically (not the general "Maps SDK," which doesn't apply here — see Stage 1's
   map-integration note) and the *Cloud Text-to-Speech API*; create one API key per
   service, restrict each key to that specific API and to the app's package
   name/bundle ID, and store both in a `secrets.json` or equivalent that is in
   `.gitignore`, never committed in plaintext.
7. **First commit, first build**: commit the empty project with its `.gitignore` and
   package manifest before writing any project-specific code, so the baseline is
   recoverable; then do one empty Build & Run to both Android and iOS targets before
   any feature work, specifically to surface signing/provisioning problems while
   there is nothing else to debug alongside them.

---

## 12. Appendix: repository and coding conventions

Not creative decisions — picked once here so an implementing agent (or a returning
human) never has to guess or re-decide them mid-project.

**Repository layout** (expands on Stage 1's three top-level folders; this is the
*development-time* monorepo shape used while building and proving the framework
against three walls — once Stage 6 actually extracts `Framework/` into a standalone
UPM package, that package's own internal layout gains the root-level `package.json`,
`README.md`, `CHANGELOG.md`, `LICENSE.md`, and the split `Tests/Editor`/`Tests/Runtime`
asmdefs given in full in Stage 6, §3 — the simpler tree below is what exists before
that extraction, not a contradiction of it):
```
/Apps
  /Panorama        -- one folder per wall, fully swappable; each contains:
    config.json    -- this wall's content (§10's schema)
    map.bytes      -- the Immersal map for this wall (§3, Stage 1)
    mesh.glb       -- OPTIONAL: occlusion/visualisation mesh (§2) — most
                       walls won't have this in the thesis timeframe
    /MediaAssets
      /Audio  /Images  /Models3D  /Videos
    scene(s) and wall-specific prefabs
  /Chafariz        -- same internal shape as /Panorama
  /Mural           -- same internal shape as /Panorama
  /[FutureWall]    -- adding a wall is adding one folder in this exact
                       shape — the literal, mechanical test of the
                       generalisability claim (§0, item 3)
/Framework
  /Runtime         -- the code Stage 6 extracts into the UPM package; never
                       references Editor code
  /Editor          -- the Wizard (Stage 5), validator/compiler (Stage 4)
  /Tests
  /Samples~        -- the Chafariz reference implementation, post-Stage 6
  /Documentation~
/Docs
  work-plan.md      -- this file
  decisions.md       -- a running log of decisions made and why (see below)
```
The `/Framework` folder above has **zero knowledge that `/Apps/Panorama` (or any
other wall) exists** — it only knows how to consume *a* `config.json` that satisfies
the schema, the same boundary the thesis report's own §3.6 already argues for at the
app level; this repository layout just makes that boundary literal, physical, and
exportable rather than conceptual.

**Namespacing**: `TileStories.Runtime.*` for framework runtime code,
`TileStories.Editor.*` for editor-only tooling, `TileStories.Apps.<WallName>.*` for
anything genuinely wall-specific that doesn't belong in the framework. Assembly
definitions (`.asmdef`) mirror this: a `Runtime` asmdef and an `Editor` asmdef (the
latter referencing the former, never the reverse — this is what makes "runtime never
references editor" enforceable rather than just a convention).

**C# style**: standard Unity/.NET conventions — PascalCase for classes, methods, and
public fields/properties; camelCase for private fields and locals (a leading
underscore on private fields, `_wallConfig`, is acceptable and common in Unity
codebases — pick one and stay consistent project-wide); `I`-prefixed interfaces
(`IWallTracker`); one public type per file, file name matching the type name.

**Git workflow**: trunk-based with short-lived feature branches, since this is a
solo-researcher project where long-lived branches mainly create merge pain with no
offsetting review benefit. Branch naming: `feature/<short-name>` (e.g.
`feature/audio-block`, `feature/lapse-state-mechanism`), `fix/<short-name>` for bug
fixes.
Commit messages: a short imperative summary line (`Add audio block playback queue`),
a blank line, then optional detail — avoid vague messages like "fixes" or "wip" on
anything merged to the trunk branch. Tag the end of each Stage (`stage-1-complete`,
`stage-2-complete`, ...) — this gives a clean reference point if a later stage's
changes need to be compared against a known-good earlier state, and doubles as a
natural place to hang each stage's exit-criteria check (§13).

**A running decisions log (`/Docs/decisions.md`)**: append one short entry every time
a design decision in this plan is actually made or revised during implementation —
date, decision, one-line reason. This is cheap to maintain and is exactly the kind of
primary evidence the thesis's own methodology (DSR/RtD, §0) treats as a valid research
artifact in its own right — reconstructing this log from memory at the end of the
project is not possible, so it has to be kept live.

**Testing**: Unity Test Framework (`com.unity.test-framework`) for the
`Framework/Tests` folder. The selection rule for what earns a unit test: deterministic,
scene-free logic where a silent bug would do damage across all three walls at once, or
where a state machine has edge cases a quick manual pass would plausibly miss. The
Stage 4 validator/compiler was the first obvious case; by this same rule,
`CircuitStateMachine` (entry-point resolution, the pause/resume stack — both already
flagged as edge-case-prone in §7's risk register), `LapseStateManager`, and the badge
trigger evaluator meet the bar just as clearly and should get the same treatment, not
be left to informal testing. UI/gameplay-level testing beyond that stays informal (the
field-testing protocols already specified per stage) rather than trying to automate
AR-camera-dependent behaviour, which has a poor cost-to-value ratio for a six-month
solo thesis.

**Orchestrator/session classes carry no business logic.** Any class whose job is to
sequence a flow (`WallSession` being the primary example — see its entry in §A) calls
other objects that make decisions; it must never itself decide *what* happens, only
*when* the thing that decides gets called. These classes are the most common place
single-responsibility quietly erodes over a project's lifetime, since they already
touch everything and each new feature tempts "just one more line here." Catch this at
code-review time, not after the fact: if an orchestrator class's own decision logic
couldn't be deleted and replaced with calls to smaller classes without changing app
behaviour, that logic hasn't been extracted yet.

---

## 13. Appendix: master checklist

Stage 1.2 reality note (updated 2026-07-23, supersedes the 2026-07-22 note below): POI
markers are implemented, tested (18 EditMode + 3 PlayMode, all passing — the project's
first PlayMode tests), and confirmed on-device (2026-07-23). The one item still
genuinely open is Workflow B (on-site tap-to-place for ambiguous positions), which
LivingRoom never needed and stays deferred until a wall that actually needs it exists.
Stage 1.2's own document (`_1_2_POI_markers_plan.md` §0) is now the single current
status record for this stage — the separate status/todo/review/tasks/solver files that
used to track this have been retired and folded into it, plus `_1_2_future_notes.md`
for open follow-ups and developer-guide material gathered along the way.

Prior note, 2026-07-22 (kept for history): core POI-marker code path and scene
cleanup step are implemented (resolver, authoring tool, correction anchor, marker view,
tests authored, rig cleaned), but Stage 1 should still be treated as open until
explicit test-run and device-smoke evidence are recorded in the Stage 1.2 status files.

Every exit criterion and outstanding item named across this document, in one place,
in the order they become relevant. Nothing here is new — it is a consolidated,
tickable view of commitments already made above, with a section reference back to the
full reasoning each time.

**Before Stage 1 starts**
- [ ] Funding source for the budget in §6 confirmed (personal vs. department).
- [ ] Immersal free-tier commercial-use question emailed to sales@immersal.com, and
  the logo templates downloaded from the SDK Samples repo, before designing AR-view
  UI chrome around them (Stage 1's Immersal section, §3).
- [ ] All accounts created in the order given in §11.
- [ ] Wall access formalised in writing (even an informal email) for all three walls,
  Panorama included (§1, §7's risk register).
- [ ] NOVA FCSH building-identification list obtained — the actual source of "150
  POIs," and a hard blocker for everything content-related if missing (§1, §7).

**Stage 1 exit (MVP v0.1)** — §3
- [ ] AR recognises the wall and shows 10 markers; tap shows a card; the map link
  works; the app runs fully offline; sustained ≥25fps on an iPhone-12-equivalent device.
- [ ] Camera-permission flow tested explicitly on both iOS and Android.
- [ ] A functional (even if unpolished) scanning-state indicator exists, and
  time-to-first-lock is being logged (§3, Stage 1).
- [ ] Occlusion re-localisation tested (someone walks through frame for ~5s) in
  addition to the simple lower-and-raise test (§3, Stage 1).
- [ ] Drift logged separately at 1m/3m/8m, not as one averaged number (§3, Stage 1).
- [ ] TestFlight and Google Play Internal Testing track both set up (§3, Stage 1).
- [ ] Accessibility smoke check (TalkBack/VoiceOver reads the first screen) passed.
- [ ] Analytics-consent toggle wired to `FirebaseAnalytics.SetConsent()`.
- [ ] Mural wall's hero-POI list defined and its field-work pass scheduled or
  completed (§1) — do not let this drift into Stage 2.
- [ ] Chafariz's own hero-POI list defined, by the same method as the Panorama's (§1).
- [ ] Immersal map-quality metrics checked in the Developer Portal for every wall
  mapped so far, not just "the capture ran without an error" (§1).
- [ ] `captured_position` recorded (adapting Immersal's Content Placement Sample)
  for every hero POI on every wall mapped so far, not deferred to "later" (§3, Stage
  1's coordinate-system note; §10).
- [ ] For the Panorama specifically, a calibration anchor captured at each panel
  join, not only at the two far ends, so the curved/multi-panel geometry doesn't get
  flattened into a single straight-line interpolation (§3, Stage 1).

**Stage 2 exit** — §3
- [ ] All 6 registered block types implemented; all 3 cross-cutting mechanisms (lapse,
  LOD, profile-content) implemented per the architectural note in §3, Stage 2.
- [ ] Each block type run, unmodified, against at least two of the three walls.
- [ ] Hero-POI counts met for all three walls.
- [ ] LOD thresholds tested on a real device per platform, not just in the Editor.
- [ ] PT-PT TTS tier actually confirmed via `list_voices()`, not assumed (§3, Stage 2).
- [ ] Audio interruption tested with a real phone call and with real Bluetooth
  headphones connecting/disconnecting mid-narration, on the actual Android devices
  in the Stage 1 device matrix, not assumed safe from the documented API alone (§3,
  Stage 2; §7's risk register).

**Stage 3 exit** — §3
- [ ] All three walls support at least one full circuit.
- [ ] Onboarding completes in under 30 seconds for a first-time tester.
- [ ] Every shipped feature's analytics event verified firing in a debug log.
- [ ] Guide character has at minimum idle/talk/point poses wired to the audio block.
- [ ] Each Gauntlet-shaped circuit physically started from at least three different
  points along the wall (the authored start, the middle, near the end) — not only
  from the beginning — to confirm entry-point resolution and `completion_rule:
  "visit_all"` actually work together (§3, Stage 3; §7's risk register).
- [ ] Ambient pivot (mid-circuit deviation handling) and the Butterfly Prompt each
  tested at least once with a real walk-through, not only reasoned about on paper —
  including the edge case of accepting a second Butterfly Prompt while already
  mid-detour from a first one (§3, Stage 3; §7's risk register).

**Before Stage 4 starts**
- [ ] All Stage 3 exit criteria above are true — do not automate on top of an unstable
  feature set (§3).

**Before Stage 7's formal evaluation sessions**
- [ ] University ethics/data-collection approval submitted (ideally by Stage 3, not
  Stage 7 — §7's risk register) and confirmed before recruiting real visitors.
- [ ] SUS, UEQ-S, the exit survey, and the knowledge check are all ready and the
  session-timing budget accounts for all four (§3, Stage 7).
- [ ] Parent/guardian consent and child assent process ready for family sessions.

**Before considering the thesis's generalisability claim "demonstrated"**
- [ ] The same block code has run, unmodified, on all three structurally different
  walls (this is the actual evidence — true as soon as Stage 3 closes, §0 item 3).
- [ ] If Stages 4–6 were completed: the time-to-new-wall measurement (§3, Stage 6) has
  been run and its result and friction log are in the thesis write-up.
- [ ] If Stages 4–6 were cut for time: the thesis text says so plainly and frames the
  claim as resting on the three hand-integrated walls, not on an installable package
  that wasn't actually built (§0 item 3) — never let the write-up imply packaging
  happened if it didn't.

**Before the thesis chapters are finalised** (not part of this plan's own timeline,
but tracked here so it isn't lost — §0)
- [ ] Chapters updated to either adopt this plan's Stage breakdown or explicitly map
  Stages to the chapters' existing Phases.
- [ ] The mural wall introduced in the chapters as a third case study, with its
  evaluation-track status (or lack of it) stated explicitly.
- [ ] Every academic reference flagged throughout this plan as "add to the thesis
  bibliography" has actually been added to `bibliography_FINAL.bib` and cited
  somewhere in the related-work, methodology, or discussion chapters — not left only
  in this work plan's own callouts. Consolidated list, all introduced in §3 Stage 3's
  circuits item unless noted otherwise:
  - Véron, E., & Levasseur, M. (1983). *Ethnographie de l'exposition*.
  - Bitgood, S. (2006). An analysis of visitor circulation. *Curator*, 49(4), 463–475.
  - Adams, E. (2014). *Fundamentals of Game Design* (3rd ed.), Ch. 12.
  - van Hage, W. R., Stash, N., Wang, Y., & Aroyo, L. (2010). Finding your way
    through the Rijksmuseum with an adaptive mobile museum guide. *ESWC 2010*, LNCS
    6088, 46–59.
  - Falk, J. H., & Dierking, L. D. (2000). *Learning from Museums*.
  - Murray, J. H. (1997). *Hamlet on the Holodeck*.
  - Mateas, M., & Stern, A. (2003). Integrating plot, character and natural language
    processing in the interactive drama *Façade*. *TIDSE 2003*, 139–151.
  - Carson, D. (2000). Environmental storytelling. *Gamasutra*. (Optionally paired
    with Jenkins, H. (2004). Game design as narrative architecture, for the more
    academically-formalised companion treatment of the same idea.)
  - Serrell, B. (1997). Paying attention: The duration and allocation of visitors'
    time in museum exhibitions. *Curator*, 40(2), 108–125. — the evidentiary basis
    for the 10–15s ambient-pivot threshold, not just an unexplained constant.
  - Krokos, E., Plaisant, C., & Varshney, A. (2019). Virtual memory palaces:
    Immersion aids recall — introduced in §7's knowledge-check rationale, not §3.
    *Virtual Reality*, 23, 1–15.

---

## References

[^w1]: Sandra Pinto Barata, "Linda-a-Velha tem um novo miradouro e um mural de arte urbana que vale a pena conhecer," *New in Oeiras*, 19 Oct. 2023. https://newinoeiras.nit.pt/fora-de-casa/linda-a-velha-tem-um-novo-miradouro-e-um-mural-de-arte-urbana-que-vale-a-pena-conhecer
[^w2]: Unity Discussions, "Unity MARS is now deprecated," Apr. 2025. https://discussions.unity.com/t/unity-mars-is-now-deprecated/1630939
[^w3]: Unity Discussions, "Unity MARS End of Support," Oct. 2025. https://discussions.unity.com/t/unity-mars-end-of-support/1692536
[^w3a]: Unity Discussions, "What is the Future of Unity MARS?," 2024 — documents XR Simulation absorbing MARS's environment-simulation feature as a free, standalone tool independent of the rest of the deprecated MARS ecosystem. https://discussions.unity.com/t/what-is-the-future-of-unity-mars/943671
[^w4]: Wikipedia, "Mixed Reality Toolkit." https://en.wikipedia.org/wiki/Mixed_Reality_Toolkit
[^w5]: GitHub, mixedrealitytoolkit/mixedrealitytoolkit-unity. https://github.com/mixedrealitytoolkit/mixedrealitytoolkit-unity
[^w6]: Microsoft Learn / GitHub, "MRTK3 Architecture overview." https://github.com/MicrosoftDocs/mixed-reality/blob/docs/mrtk-unity/mrtk3-overview/architecture/architecture.md
[^w6a]: Microsoft Learn / GitHub, MRTK3 Subsystems documentation and feature list — profile assets selecting active subsystems per deployment target, and data binding for branding/theming/dynamic data/complex lists as a shipped first-class feature. https://github.com/MicrosoftDocs/mixed-reality/blob/docs/mrtk-unity/mrtk3-overview/architecture/subsystems.md ; https://github.com/mixedrealitytoolkit/mixedrealitytoolkit-unity
[^w7]: GitHub, microsoft/MixedRealityToolkit-Unity Issue #3545 (MRTK2 service-locator usability problems). https://github.com/microsoft/MixedRealityToolkit-Unity/issues/3545
[^w8]: Yarn Spinner, official site/features (data-driven dialogue, localisation, Dialogue Views). https://yarnspinner.dev/ ; https://yarnspinner.dev/features/
[^w9]: Yarn Spinner Features page, "Dialogue Views." https://yarnspinner.dev/features/
[^w10]: storyflow-editor.com, "Branching Dialogue Nightmare: Fix It in Unity, Unreal & Godot," Oct. 2025 (synthesising Sam Kabo Ashwell's 2015 taxonomy). https://storyflow-editor.com/blog/branching-dialogue-nightmare-how-to-fix/
[^w11]: Comparative coverage of museum-tour app builders (PandaSuite, STQRY, izi.TRAVEL, SmartGuide, Cuseum) — general market-context positioning, no single canonical source.
[^w13]: Unity glTFast (`com.unity.cloud.gltfast`) package documentation — runtime loading from `byte[]`, defer-agent frame-spreading. https://docs.unity3d.com/Packages/com.unity.cloud.gltfast@6.0/manual/index.html
[^w14]: Unity Manual, "Package layout for UPM packages." https://docs.unity3d.com/Manual/cus-layout.html
[^w15]: Unity Manual, "Create or edit the assembly definitions." https://docs.unity3d.com/Manual/cus-asmdef.html
[^w16]: OpenAI, API Deprecations page and Changelog — gpt-4o-realtime-preview deprecation (notified Sept. 2025, removed ~Mar. 2026) and Realtime API Beta removal (12 May 2026), superseded by the GA Realtime API and the `gpt-realtime` model family. Verified during this review, June 2026. https://developers.openai.com/api/docs/deprecations ; https://developers.openai.com/api/docs/changelog
[^w17]: Android Developers, "Android App Bundles — Compressed download size restriction" (150MB hard cap on compressed download size); Unity Manual, "Google Play delivery requirements." Verified during this review, June 2026. https://docs.unity3d.com/6000.3/Documentation/Manual/android-distribution-google-play.html
[^w18]: Unity, "Unity 6 Releases & Support" — Unity 6.3 LTS supported until December 2027 (two-year LTS window, plus an additional year for Enterprise/Industry subscribers). Verified during this review, June 2026. https://unity.com/releases/unity-6/support
[^w19]: Immersal Developer Documentation, "Pricing," "FAQ," and "Compatibility" pages, and the Developer Portal dashboard notice — free tier: 100 images/map, sufficient for roughly 100–120m² indoor or 200–500m² open outdoor areas; a "Powered by Immersal" logo required throughout the AR experience from SDK v1.18+, with ready-made templates provided in `Assets/ImmersalSDK/Samples/ImmersalLogos`; the Pricing page's specific wording ("the free license does not support the development of commercial projects") is treated as authoritative over older, vaguer wording elsewhere on Immersal's site stating free-tier use is permitted commercially with capacity limits only — confirm directly with sales@immersal.com for this project's specific case before relying on either statement. Pro tier: $99/month, 500 images/map. GitHub `immersal/imdk-unity` README — installation via Package Manager git-URL, dependency on AR Foundation/ARCore XR Plugin/ARKit XR Plugin/TextMesh Pro. Verified during this review, June 2026. https://developers.immersal.com/docs/immersal-sdk/pricing/ ; https://developers.immersal.com/docs/immersal-sdk/howdoesitwork/ ; https://developers.immersal.com/docs/immersal-sdk/faq/ ; https://github.com/immersal/imdk-unity
[^w20]: Google Cloud Text-to-Speech, "Supported voices and languages" and Chirp 3: HD documentation — `pt-BR` appears in the current Chirp 3: HD 31-language rollout and in documented Neural2 voice-map examples; `pt-PT` does not appear in either as of this review. Pricing: Neural2 $16/1M characters, WaveNet $4/1M characters, free allowance 4M characters/month (Standard) or 1M characters/month (WaveNet). Verified during this review, June 2026; voice-tier rollouts change frequently — re-verify via `list_voices(language_code="pt-PT")` at implementation time rather than relying on this note. https://docs.cloud.google.com/text-to-speech/docs/list-voices-and-types
[^w21]: Firebase, Unity SDK Release Notes and `firebase/firebase-unity-sdk` GitHub releases — current version in the 13.x series (Android BoM ~34.x) as of June 2026; Firebase Analytics for Unity remains fully supported (not deprecated); `FirebaseAnalytics.SetConsent()` is the SDK's GDPR-consent mechanism. Verified during this review, June 2026. https://firebase.google.com/support/release-notes/unity ; https://github.com/firebase/firebase-unity-sdk/releases
[^w22]: Google's first-party "Maps SDK for Unity" is deprecated (confirmed via Unity Discussions community reports, June 2026); the Maps Static API (`maps.googleapis.com/maps/api/staticmap`) fetched via `UnityWebRequest` onto a `RawImage` is the standard current substitute for an in-app thumbnail map, while "open in native Maps app" needs only a `geo:` / Maps-web URL via `Application.OpenURL`, no API key. Verified during this review, June 2026. https://discussions.unity.com/t/using-maps-sdk-for-android/920377
[^w23]: Immersal Developer Documentation, "Map Optimization," "Map Testing," and "How To Map" pages — `featureCount`/`featureCountMax` and `trackerLengthMin` as the documented map-size/accuracy trade-off parameters; the held-out test-image-set methodology for empirically comparing optimisation passes by localisation success rate; ~50% recommended image overlap and a 3-image minimum for feature-point matching during capture. `immersal/immersal-sdk-samples` CHANGELOG — confirms the per-localisation-request map ID limit is currently 32 (raised from 8). Verified during this review, June 2026. https://developers.immersal.com/docs/mapsmapping/advanced/mapoptimizationandcustomization/ ; https://developers.immersal.com/docs/mapsmapping/advanced/maptestingfeature/ ; https://developers.immersal.com/docs/mapsmapping/howtomap/ ; https://github.com/immersal/immersal-sdk-samples/blob/master/CHANGELOG.md
[^w24]: Yarn Spinner, `DialogueRunner.StartDialogue(string nodeName)` API reference — confirms the node to start from is a caller-supplied parameter, with "Start" only its default value when none is given. Verified during this review, June 2026. https://docs.yarnspinner.dev/api/csharp/yarn.unity/yarn.unity.dialoguerunner/yarn.unity.dialoguerunner.startdialogue
[^w25]: inkle, *ink* (open-source narrative scripting language, MIT licence; used in *80 Days*, *Heaven's Vault*, *Pendragon*, and *A Highland Song*) — knot/stitch addressing and the divert (`->`) operator, confirming any addressable knot or stitch can be a valid entry point, not only a single hardcoded start. The "Signs of the Sojourner" development blog gives a concrete, directly analogous example of a calling application choosing which knot to begin at based on external context. Verified during this review, June 2026. https://github.com/inkle/ink/blob/master/Documentation/WritingWithInk.md ; https://www.echodoggames.com/blog/2019/09/19/using-ink-for-conversations/
[^w26]: van Hage, W. R., Stash, N., Wang, Y., & Aroyo, L. (2010). Finding your way through the Rijksmuseum with an adaptive mobile museum guide. In L. Aroyo et al. (Eds.), *The Semantic Web: Research and Applications* (ESWC 2010), *Lecture Notes in Computer Science*, vol. 6088, pp. 46–59. Springer. https://doi.org/10.1007/978-3-642-13486-9_4 — a mobile museum guide (the CHIP/Rijksmuseum research programme) that re-routes a personalised tour in real time based on the visitor's current position, the closest direct precedent found for this mechanism. Verified during this review, June 2026.
[^w27]: Véron, E., & Levasseur, M. (1983). *Ethnographie de l'exposition: l'espace, le corps et le sens*. Paris: Bibliothèque publique d'Information, Centre Georges Pompidou — the foundational ant/fish/butterfly/grasshopper museum-visitor-movement typology, still cited and re-validated across museum visitor-studies research as of 2025–2026. Verified during this review, June 2026.
[^w28]: Bitgood, S. (2006). An analysis of visitor circulation: Movement patterns and the General Value Principle. *Curator: The Museum Journal*, 49(4), 463–475. Verified during this review, June 2026.
[^w29]: Immersal Developer Documentation, "The Content Placement Sample" and "Sample Scenes" pages — confirms that on a successful pose, the XR Space (not the AR camera) is transformed so its contents match the real world, and that `ContentStorageManager`/`MovableContent.cs` in the official sample already implement capture-and-persist of a position while localised. Verified during this review, June 2026. https://developers.immersal.com/docs/unitysdk/samplescenes/content-placement-sample/ ; https://developers.immersal.com/docs/unitysdk/samplescenes/
[^w30]: Murray, J. H. (1997). *Hamlet on the Holodeck: The Future of Narrative in Cyberspace*. New York: Free Press. Verified during this review, June 2026.
[^w31]: Mateas, M., & Stern, A. (2003). Integrating plot, character and natural language processing in the interactive drama *Façade*. In *Proceedings of the 1st International Conference on Technologies for Interactive Digital Storytelling and Entertainment (TIDSE 2003)*, pp. 139–151. Verified during this review, June 2026.
[^w32]: Falk, J. H., & Dierking, L. D. (2000). *Learning from Museums: Visitor Experiences and the Making of Meaning*. Walnut Creek, CA: AltaMira Press. Verified during this review, June 2026.
[^w33]: Krokos, E., Plaisant, C., & Varshney, A. (2019). Virtual memory palaces: Immersion aids recall. *Virtual Reality*, 23, 1–15. https://doi.org/10.1007/s10055-018-0346-3 — found an 8.8% higher recall rate using a head-mounted-display memory palace versus an equivalent desktop condition. Verified during this review, June 2026.
[^w34]: Unity Scripting API, `AudioSettings.OnAudioConfigurationChanged`, and Unity Manual, "Audio Settings" — the documented callback and required play-state-recovery pattern for audio device changes, distinct from `OnApplicationPause`/`OnApplicationFocus`. Unity Issue Tracker, "[Android] AudioClip stops playing when connecting the Bluetooth earbuds/earphones" (Unity's own confirmation that the audio engine re-initialises and loses playing state on Android device changes) and Unity Discussions, "OnAudioConfigurationChanged not called when change audio output device on Android" (developer reports of the callback not firing reliably on Android in practice). Verified during this review, June 2026. https://docs.unity3d.com/ScriptReference/AudioSettings.OnAudioConfigurationChanged.html ; https://docs.unity3d.com/Manual/class-AudioSettings.html ; https://issuetracker.unity3d.com/issues/android-audioclip-stops-playing-when-connecting-the-bluetooth-earbuds-slash-earphones ; https://discussions.unity.com/t/onaudioconfigurationchanged-not-called-when-change-audio-output-device-on-android/916993
[^w35]: Serrell, B. (1997). Paying attention: The duration and allocation of visitors' time in museum exhibitions. *Curator: The Museum Journal*, 40(2), 108–125. https://doi.org/10.1111/j.2151-6952.1997.tb01292.x — the field-standard tracking-and-timing methodology, still cited as the reference protocol in 2025 visitor-studies papers; treats roughly two seconds as the minimum to count as a deliberate "stop." Verified during this review, June 2026.
[^w35a]: Unity AR Foundation documentation, "XR Simulation" — confirmed first-party, included in AR Foundation 5.x, enabled via Project Settings → XR Plugin Management → Simulation tab; supports keyboard/mouse navigation through simulated environments in Play Mode without building to a device. Verified during this review, June 2026. https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@5.1/manual/xr-simulation/simulation.html
[^w36a]: Angry Shark Studio (2025, September). "Unity UI Toolkit vs UGUI: 2025 Developer Guide" — industry consensus September 2025: "VR/AR Projects: UGUI for world-space UI. UI Toolkit for screen overlays. UI Toolkit requires complex workarounds" for world-space. Unity official documentation (2026): "uGUI and IMGUI might be better choices if you need features that are not yet available in UI Toolkit." Verified during this review, June 2026. https://www.angry-shark-studio.com/blog/unity-ui-toolkit-vs-ugui-2025-guide/ ; https://docs.unity3d.com/6000.0/Documentation/Manual/UI-system-compare.html ; https://medium.com/@idimus/unity-ui-toolkit-safe-area-4dd35380b60d (safe-area coordinate-inversion for UI Toolkit specifically)
[^w36b]: Unity blog, "Unity 6 UI Toolkit: News and Updates" (November 2024) — confirms production-ready runtime data binding system, localization integration via bindings, built-in animation system added, expanded standard control library. Verified during this review, June 2026. https://unity.com/blog/unity-6-ui-toolkit-updates
[^w36]: A 2024 visualization-exhibit deployment study (arXiv:2404.01488, "DeLVE into Earth's Past") explicitly defines visitor engagement as "hooked" once dwell time exceeds a ten-second threshold, distinct from a passing stop. Verified during this review, June 2026. https://arxiv.org/abs/2404.01488

All other architectural reasoning (Core/Walls split, Martin Fowler's Rule of Three,
Wrong Abstraction, DSR/DBR/RtD methodology) is already cited with full bibliographic
detail in the thesis's own `bibliography_FINAL.bib` and is not duplicated here.
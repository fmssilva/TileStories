# PROJECT_GUIDE.md
> **This is the master implementation guide.** Read it before any session. It supersedes all older planning documents (`AR_IMPLEMENTATION_PLAN.md`, `AR_notes.md`, `FUTURE_ARCHITECTURE.md`, and the raw notes in `proj_notes`).
>
> **Stack**: Flutter 3.x · Dart ≥ 3.8.0 · Riverpod 3 · GoRouter 17 · Freezed 3 · Material 3
>
> **Updated**: 2025

---

## Table of Contents

1. [Project vision and scope](#1-project-vision-and-scope)
2. [AR layer abstraction — implementation switching](#2-ar-layer-abstraction--implementation-switching)
3. [Final folder structure — the target state](#3-final-folder-structure--the-target-state)
4. [Domain dependency rules](#4-domain-dependency-rules)
5. [Testing strategy](#5-testing-strategy)
6. [Implementation phases](#6-implementation-phases)
   - [Phase 0 — Cleanup and foundation](#phase-0--cleanup-and-foundation)
   - [Phase 1 — AR proof of concept](#phase-1--ar-proof-of-concept)
   - [Phase 2 — App design and integration](#phase-2--app-design-and-integration)
   - [Phase 3 — Experience Core](#phase-3--experience-core)
   - [Phase 4 — Wow Moments](#phase-4--wow-moments)
   - [Phase 5 — Excellence and release](#phase-5--excellence-and-release)
7. [Ongoing rules (every phase, every session)](#7-ongoing-rules-every-phase-every-session)
8. [Package decisions](#8-package-decisions)
9. [What to ignore from older documents](#9-what-to-ignore-from-older-documents)

---


## 1. Project vision and scope

**What this app is:** An AR-augmented museum companion for the Grande Panorama de Lisboa (c. 1700, 23m panel, Museu Nacional do Azulejo). Visitors point their phone at the panel and see interactive markers over historical buildings. They can explore by epoch, follow themed circuits, listen to audio narration, ask the AI guide questions, and witness the 1755 earthquake.

**What the user does before opening AR — the entry flow:**

This is the most underspecified part of the plan and must be decided before building Phase 1. The app is used inside a museum in front of a 23-metre painting. The user does not arrive knowing what they're looking at. The entry experience must:

1. **Welcome / context screen** (`home_page.dart`): A brief visual introduction to the Grande Panorama — what it is, when it was painted, why it matters. Single screen, no scroll wall. Design goal: 15 seconds to read, then proceed.
2. **Onboarding / profile** (Phase 2, but placeholder needed in Phase 1): "Who are you exploring with?" — architecture enthusiast, history buff, family with children, general visitor. Skippable in Phase 1 (defaults to `general`), required in Phase 2.
3. **Main entry choice** — two paths clearly presented:
   - "Explorar com AR" → requires camera permission → opens `PanoramaARPage`
   - "Explorar sem AR" → opens static panorama view with tap-to-reveal markers (offline-friendly fallback)
4. **AR mode** — user points phone at the physical panel; markers appear. This is the primary experience.

The home page flow must be designed before Phase 1 development begins. Without a clear entry flow, the "what does the user see first?" question gets answered accidentally (usually by whatever widget is top of the routing tree).

**Platform targets:**
- Android (primary — ARCore, minSdkVersion 24)
- iOS (secondary — ARKit, iOS 14.0+)
- Web (layout/dev reference only — AR not available on web)

**Languages:** Portuguese (default), English, Spanish — all three from Phase 0 onwards (UI strings) / Phase 4 (long-form content bulk translation)

**Museum partnership — open blockers (resolve before Phase 1 starts):**

These are not implementation details. They are external dependencies that can block delivery if left unresolved.

| Question                                                       | Why it blocks                                                                                                                                                                                            | Who resolves it                           |
| -------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------- |
| Who provides the panorama high-resolution scan?                | The app needs a gigapixel or multi-image mosaic of the 23m panel. Photographing it yourself is logistically hard (lighting, access, stitching). The museum may have a scan already.                      | Museum contact                            |
| What are the image rights?                                     | You cannot ship the panorama in the app bundle without rights clearance. The painting is historic (public domain) but the museum's specific photograph may have copyright.                               | Museum / your institution's legal advisor |
| Can 360° building photos be taken inside the museum?           | If using Option A for interior views, you need museum access to photograph interior spaces of the referenced buildings (not the museum itself, but Castelo etc.). Some are public; some require permits. | Museum / local government                 |
| Is there a formal data sharing agreement for thesis analytics? | If the museum is considered a research partner, their legal team may need to approve data collection methodology.                                                                                        | Museum / university ethics board          |
| What is the museum's wifi situation?                           | If the app relies on the AI assistant (HTTP calls), users need wifi or data. Is there museum wifi? Is it reliable? Affects the offline-first decision.                                                   | Museum contact                            |
| What device OS are museum visitors using?                      | `minSdkVersion 24` excludes ~5% of Android devices. Is the target demographic (museum visitors in Lisbon) within that range?                                                                             | Museum / your own observation             |
| QR code placement                                              | If the museum will place QR codes to download the app, where? This affects the onboarding design (how does the user know to point the phone at the painting?).                                           | Museum contact                            |

> **Action**: have all of these answered and documented in `PROJECT_GUIDES/MUSEUM_PARTNERSHIP.md` before Phase 2 development begins. Discovering blocked questions in Phase 3 delays the thesis timeline.

**Final deliverables (from pitch):**

| Deliverable                                | Phase               |
| ------------------------------------------ | ------------------- |
| AR panorama with 30 POIs (offline + live)  | Phase 1 POC         |
| Timeline slider (4 epochs)                 | Phase 1 POC         |
| Home page + entry flow + store release     | Phase 2             |
| User profiles (onboarding)                 | Phase 3             |
| Audio guide (20 clips, 5 circuits)         | Phase 3             |
| 5 themed circuits                          | Phase 3             |
| Quiz per building                          | Phase 3             |
| Achievements / gamification / leaderboard  | Phase 3             |
| Analytics for thesis                       | Phase 3             |
| Earthquake simulation (Flutter-native)     | Phase 4             |
| AI Q&A assistant (GPT-4o-mini via proxy)   | Phase 4             |
| 100 POIs                                   | Phase 4             |
| 360° interior views (TBD — see §6 Phase 4) | Phase 4             |
| Unity 3D walkthroughs (5 buildings)        | Phase 4b (parallel) |
| 150 POIs                                   | Phase 5             |
| Full ES translations (long-form content)   | Phase 5             |
| Performance optimization + size < 100MB    | Phase 5             |
| WCAG 2.1 AA final audit                    | Phase 5             |

---

## 2. AR layer abstraction — implementation switching

The codebase already has a clean 3-layer abstraction in `lib/ar_core/abstractions/`:

- `ARSessionManager` — manages AR session lifecycle (initialize, start, pause, resume, dispose)
- `ARImageTracker` — manages reference image detection and anchor tracking
- `ImageFrameProvider` — delivers frames to the AR pipeline (static image pan/zoom OR live camera)

Each has multiple concrete implementations in `lib/ar_core/implementations/`:

| Implementation             | What it does                                               | When used                          |
| -------------------------- | ---------------------------------------------------------- | ---------------------------------- |
| `MockARSessionManager`     | No-op session; always tracks; no hardware required         | Development, all tests, web        |
| `MockARImageTracker`       | Emits fake anchors after a configurable delay              | Development, all tests             |
| `ARCoreSessionManager`     | Real ARCore (Android) via `ar_flutter_plugin_plus`         | `--dart-define=USE_REAL_AR=true`   |
| `ARCoreImageTracker`       | Real ARCore augmented images                               | `--dart-define=USE_REAL_AR=true`   |
| `StaticImageFrameProvider` | Offline mode — InteractiveViewer around the panorama image | Always available, no camera needed |
| `CameraFrameProvider`      | Live mode — streams frames from device camera              | `--dart-define=USE_REAL_AR=true`   |

**Switching implementations:**

The compile-time flag `kUseRealAR = bool.fromEnvironment('USE_REAL_AR')` in `ar_infrastructure_providers.dart` controls which implementation the Riverpod providers return. The rest of the app (panorama domain, widgets, tests) depends only on the abstract interfaces — they are completely unaware of which backend is active.

**Panorama image — bundled with the app:**

The high-resolution panorama mosaic is shipped inside the app bundle (`assets/images/panorama/`), not downloaded at runtime. This decision is intentional: museum wifi may be unreliable, and offline exploration is a first-class feature. The same image serves two purposes:

1. **Offline mode** (`StaticImageFrameProvider`): displayed in `InteractiveViewer` — user pans/zooms and taps POI markers without camera.
2. **Live AR mode** (`ARCoreImageTracker`): used as the reference image for ARCore augmented image tracking. ARCore builds an on-device feature database from this image. **No internet connection is required** — ARCore augmented image tracking runs entirely on-device (confirmed in Google ARCore docs).

Consequence: the ARCore reference image in `panorama_reference_images.dart` must match the physical panel exactly as seen by visitors. If the museum moves or re-lights the panel, re-photograph and update the reference image.

```
Run with mock AR (default — works on any machine, no device needed):
  flutter run -d chrome
  flutter run -d <device>   (without --dart-define — kUseRealAR == false)

Run with real ARCore:
  flutter run --dart-define=USE_REAL_AR=true -d <android_device_id>
```

**Adding Unity AR later:** When a Unity project exists, add a `UnityARSessionManager` and `UnityARImageTracker` in `lib/ar_core/implementations/unity/`. Wire them into `ar_infrastructure_providers.dart` behind a `kUseUnityAR` compile-time flag. No other file needs to change — the panorama domain, providers, and widgets are already abstracted. The upgrade path `mock → Flutter ARCore → Unity AR` is fully supported by the current architecture with no refactoring required.

---

## 3. Final folder structure — the target state

Complete target structure for the finished app. Every new file goes here and nowhere else.

```
lib/
│
├── main.dart
│
├── design/                          # Design system — pure tokens and theme
│   ├── design_system.dart           # Single barrel export — always import this, never raw values
│   ├── tokens/
│   │   ├── spacing_tokens.dart      # Spacing.* — zero/xs/sm/md/lg/xl/xl2..xl7 + semantic aliases
│   │   ├── radius_tokens.dart       # RadiusTokens.* — component aliases
│   │   ├── animation_tokens.dart    # AnimationTokens.* — fast/medium/slow/verySlow + curves
│   │   ├── breakpoints.dart         # Breakpoints.mobile/tablet/desktop/wide/extraWide
│   │   ├── layout_tokens.dart       # LayoutTokens.* — maxContentWidth, readingWidth, etc.
│   │   ├── size_tokens.dart         # SizeTokens.* — icons, tapTarget, appBarHeight
│   │   ├── elevation_tokens.dart    # ElevationTokens.* — levels + getInteractiveElevation
│   │   ├── typography_tokens.dart   # Typography scale definitions
│   │   └── z_index_tokens.dart      # Z-index layering constants
│   ├── theme/
│   │   ├── app_theme.dart           # AppTheme.light() / dark() / highContrast()
│   │   ├── theme_extensions.dart    # context.primary, context.isMobile, etc.
│   │   └── theme_provider.dart      # themeModeProvider (NotifierProvider)
│   ├── branding/
│   │   ├── app_logo.dart
│   │   ├── app_name.dart
│   │   ├── app_brand.dart
│   │   └── app_assets.dart
│   └── layout/
│       ├── responsive_container.dart
│       ├── max_width_box.dart
│       ├── centered_content.dart
│       └── responsive_padding.dart
│
├── navigation/                      # Routing — single source of truth
│   ├── navigation.dart              # Barrel
│   ├── navConfig/
│   │   ├── nav_item.dart            # NavItem + NavMetadata (Freezed)
│   │   ├── nav_config.dart          # ALL routes declared here — add new routes here only
│   │   ├── router_config.dart       # createRouter(WidgetRef) — GoRouter setup
│   │   └── current_route_provider.dart
│   ├── histConfig/
│   │   ├── history_provider.dart    # navHistoryProvider (50-entry stack)
│   │   ├── is_navigating_provider.dart  # one-frame flag for push vs back/forward
│   │   ├── history_entry.dart
│   │   └── history_state.dart
│   └── widgets/
│       ├── nav_tabs_row.dart
│       └── hamburger/
│           ├── hamburger.dart
│           └── nav_accordion.dart
│
├── layout/                          # Layout system — every page uses LayoutManager
│   ├── layout_manager.dart          # Main orchestrator — ScrollRegistry + PageStateRegistry
│   ├── layout_slots.dart            # LayoutSlots data class
│   ├── layout_presets.dart          # LayoutPresets static factories
│   ├── platform_info.dart           # PlatformInfo.isApp(context)
│   ├── widgets/
│   │   ├── header.dart
│   │   ├── footer_app.dart
│   │   ├── footer_browser.dart
│   │   ├── back_to_top_button.dart
│   │   ├── breadcrumbs.dart         # renamed from breadcrums.dart (Phase 0)
│   │   ├── icons_group.dart         # language + theme switchers
│   │   ├── vertical_side_bar.dart
│   │   └── fab_wrapper.dart         # renamed from fab_wraper.dart (Phase 0)
│   ├── scrollController/
│   │   ├── scroll_registry.dart
│   │   └── scroll_registry_provider.dart
│   └── pageState/
│       ├── page_state_registry.dart
│       └── page_state_registry_provider.dart
│
├── ar_core/                         # AR infrastructure — domain-agnostic
│   ├── ar_core.dart                 # Barrel
│   ├── abstractions/
│   │   ├── ar_image_tracker.dart    # abstract interface
│   │   ├── ar_session_manager.dart  # abstract interface
│   │   └── image_frame_provider.dart  # abstract interface
│   ├── models/
│   │   ├── ar_tracking_state.dart   # enum + isTracking/hasError extensions
│   │   ├── ar_renderable.dart       # Freezed union: marker | model3D
│   │   ├── ar_availability.dart     # enum: available | unavailable | unknown
│   │   ├── ar_anchor.dart
│   │   ├── ar_reference_image.dart
│   │   ├── ar_session_config.dart
│   │   ├── ar_transform.dart
│   │   └── image_frame.dart
│   ├── nodes/                       # Phase 1: 3D node management abstraction (GLBs deferred to Phase 3)
│   │   ├── ar_node_manager.dart     # abstract interface: placeModel, removeModel, removeAll, onModelTapped
│   │   ├── ar_node_3d_config.dart   # plain Dart class: poiId, assetPath, scale, zOffset
│   │   └── mock_ar_node_manager.dart  # no-op implementation for tests + offline + Phase 1
│   ├── viewport/                    # Phase 1: viewport state abstraction for LOD system
│   │   ├── viewport_state.dart      # Freezed class: zoomLevel, centerNormalized, visibleRegion, LODTier, mode
│   │   ├── viewport_state_provider.dart  # NotifierProvider<ViewportStateNotifier, ViewportState>
│   │   └── frame_widget_provider.dart   # Phase 1: returns correct frame widget per ViewportMode (Layer A/B bridge)
│   ├── implementations/
│   │   ├── arcore/
│   │   │   ├── arcore_session_manager.dart
│   │   │   └── arcore_image_tracker.dart
│   │   ├── camera/
│   │   │   └── camera_frame_provider.dart
│   │   ├── mock/
│   │   │   ├── mock_ar_session_manager.dart
│   │   │   └── mock_ar_image_tracker.dart
│   │   └── static_image/
│   │       └── static_image_frame_provider.dart
│   ├── utils/
│   │   ├── anchor_stabilizer.dart
│   │   ├── calibrator_poi_model.dart  # remove in Phase 0 if only used by the calibrator page
│   │   └── coordinate_converter.dart
│   │   # poi_calibrator_page.dart → deleted in Phase 0; use PROJECT_GUIDES/ar_poi_calibrator.html instead
│   └── test/
│       ├── unit/
│       └── widgets/
│
├── components/                      # Shared UI — not domain-specific
│   ├── feedback/
│   │   ├── async_value_builder.dart  # Standardized AsyncValue.when() wrapper
│   │   ├── empty_state.dart
│   │   ├── error_display.dart
│   │   ├── error_page.dart           # go_router errorBuilder page
│   │   └── loading_indicator.dart    # + LoadingSize enum
│   └── ui/                           # Populated in Phase 1 as shared primitives emerge
│       ├── info_badge.dart           # Phase 1: colored pill badge (category, status)
│       ├── section_header.dart       # Phase 1: titled section divider with optional action
│       └── tappable_card.dart        # Phase 1: base card with InkWell + elevation + radius
│
├── utils/
│   ├── i18n/
│   │   ├── models/
│   │   │   ├── language.dart         # Language enum: portuguese | english | spanish
│   │   │   └── translatable_string.dart  # TranslatableString(pt, en, es?) + t() + ref.tr()
│   │   ├── providers/
│   │   │   └── language_provider.dart
│   │   ├── extensions/
│   │   │   └── context_extensions.dart  # I18nWidgetRef.language, .tr()
│   │   ├── utils/
│   │   │   └── language_storage.dart
│   │   └── widgets/
│   │       └── language_switcher.dart
│   └── zoom/
│
└── domains/
    │
    ├── home/                         # Welcome / landing page
    │   ├── pages/
    │   │   └── home_page.dart
    │   └── test/
    │       └── widgets/
    │
    ├── panorama/                     # Core AR experience — Phase 1
    │   ├── panorama_domain.dart      # Public barrel
    │   ├── models/
    │   │   └── poi.dart              # POI (Freezed), POICategory enum, POI extensions
    │   ├── repositories/
    │   │   └── pois_repository.dart  # loads assets/data/pois.json (41 POIs); in-memory cache; ES search added Phase 1
    │   ├── providers/
    │   │   └── panorama_providers.dart  # selectedPOIProvider, poiDisplayModeProvider, poisRepositoryProvider, poisProvider
    │   │   #   Dead providers filteredPOIsProvider/showPOIsProvider/selectedCategoryProvider DELETED Phase 1 Session 7A
    │   ├── pages/
    │   │   └── panorama_ar_page.dart    # thin wrapper -> PanoramaARView
    │   ├── services/
    │   │   └── poi_visibility_service.dart  # Phase 1: pure business logic; takes POIs+viewport+epoch → List<POIRenderSpec>
    │   ├── widgets/
    │   │   # poi_marker.dart — DELETED Phase 1 Session 4 (replaced by ar_poi_marker.dart in ar/widgets/)
    │   │   # poi_info_sheet.dart — DELETED Phase 1 Session 4 (renamed to poi_detail_sheet.dart in ar/widgets/)
    │   │   ├── poi_summary_card.dart    # Phase 1: compact overlay anchored near tapped marker (not bottom sheet)
    │   │   ├── poi_detail_sheet.dart    # Phase 1: renamed from poi_info_sheet.dart — full DraggableScrollableSheet
    │   │   └── poi_action_buttons.dart  # Phase 1: extracted action row reused by card + sheet
    │   ├── ar/
    │   │   ├── controllers/
    │   │   │   ├── panorama_ar_state.dart    # PanoramaARState (Freezed)
    │   │   │   └── panorama_ar_controller.dart  # PanoramaARController extends Notifier
    │   │   ├── providers/
    │   │   │   ├── panorama_ar_providers.dart
    │   │   │   └── ar_infrastructure_providers.dart  # kUseRealAR, mock/real switch; arNodeManagerProvider
    │   │   ├── config/
    │   │   │   ├── panorama_reference_images.dart
    │   │   │   └── panorama_3d_nodes_config.dart  # Phase 1: kAR3DNodes = []; Phase 3: populated with GLB paths
    │   │   ├── builders/
    │   │   │   └── # panorama_ar_scene_builder.dart — DELETED in Phase 1 Session 7A (scene management belongs in controller, not a builder class)
    │   │   └── widgets/
    │   │       ├── panorama_ar_view.dart     # Main AR view
    │   │       ├── panorama_top_bar.dart     # Phase 1: semi-transparent back+options (replaces browser header.dart)
    │   │       ├── ar_tracking_indicator.dart # Phase 1: searching/initializing/tracking states; auto-hides
    │   │       ├── ar_mode_toggle.dart       # Phase 1: compact pill at bottom-right (replaces top SegmentedButton)
    │   │       ├── ar_first_time_overlay.dart # Phase 1: 2-step coach mark, shown once per install in live AR mode
    │   │       ├── panorama_fab.dart         # Phase 2: expandable FAB — Progress sub-action real; Audio/Circuits/AI stubs
    │   │       ├── ar_not_available_widget.dart
    │   │       ├── ar_poi_marker.dart        # Phase 1: 4 LOD tiers; destruction spectrum; ARPOIMarkerIcon
    │   │       ├── poi_legend_button.dart    # Phase 1: 165-line file — 36dp ⓘ button; declares part 'poi_legend_sheet.dart'
    │   │       └── poi_legend_sheet.dart     # Phase 1: part of poi_legend_button.dart (~1440 lines); spectrum bar + 8-type grid
    │   └── test/
    │       ├── unit/
    │       ├── widgets/
    │       └── integration/
    │
    ├── timeline/                     # Epoch slider — Phase 1
    │   ├── timeline_domain.dart
    │   ├── models/
    │   │   └── time_period.dart      # plain Dart enum: pre1755|earthquake|pombalina|today
    │   │                              # Fields: year(int), label(TranslatableString),
    │   │                              #   description(TranslatableString)
    │   │                              # accentColor: NOT on enum — widget-layer extension only
    │   ├── providers/
    │   │   └── timeline_provider.dart  # NotifierProvider<TimelineNotifier, TimePeriod>
    │   ├── widgets/
    │   │   ├── timeline_slider.dart    # 4 labelled stops, design-system styled
    │   │   └── epoch_label.dart        # current epoch name + year
    │   └── test/
    │       ├── unit/
    │       └── widgets/
    │
    ├── onboarding/                   # User profile — Phase 2
    │   ├── onboarding_domain.dart
    │   ├── models/
    │   │   ├── profile_type.dart     # Freezed enum: architecture|history|child|general
    │   │   └── user_profile.dart     # Freezed: profileType, preferredLanguage
    │   ├── providers/
    │   │   └── onboarding_provider.dart  # AsyncNotifier<UserProfile?>
    │   ├── pages/
    │   │   ├── welcome_page.dart
    │   │   └── profile_setup_page.dart
    │   ├── widgets/
    │   │   └── profile_selector.dart
    │   └── test/
    │       ├── unit/
    │       └── widgets/
    │
    ├── analytics/                    # Local event logging — Phase 2 start
    │   │                              # NOTE: Phase 2 (not Phase 3) because gamification depends on it
    │   ├── analytics_domain.dart
    │   ├── models/
    │   │   ├── analytics_event.dart  # Freezed: eventType(enum), timestamp, metadata(Map)
    │   │   └── analytics_session.dart  # Freezed: sessionId, startedAt, endedAt
    │   ├── services/
    │   │   └── analytics_service.dart  # JSON-append via dart:io; exportSession(); clearOld()
    │   ├── providers/
    │   │   └── analytics_provider.dart  # Provider<AnalyticsService> singleton
    │   └── test/
    │       └── unit/
    │
    ├── audio_guide/                  # Audio narration — Phase 2
    │   ├── audio_guide_domain.dart
    │   ├── models/
    │   │   └── audio_clip.dart       # Freezed: id, poiId?, circuitId?, assetPath, duration, title
    │   ├── repositories/
    │   │   └── audio_clips_repository.dart
    │   ├── providers/
    │   │   ├── audio_guide_provider.dart    # NotifierProvider<AudioGuideNotifier, AudioGuideState>
    │   │   └── audio_player_provider.dart   # Provider<AudioPlayer> — just_audio instance
    │   ├── widgets/
    │   │   ├── audio_controls.dart          # play/pause/seek
    │   │   └── now_playing_bar.dart         # persistent mini-player
    │   └── test/
    │       ├── unit/
    │       └── widgets/
    │
    ├── circuits/                     # Themed routes — Phase 2
    │   ├── circuits_domain.dart
    │   ├── models/
    │   │   ├── circuit.dart          # Freezed: id, name, description, poiIds, recommendedTimePeriod?
    │   │   └── circuit_progress.dart  # Freezed: circuitId, visitedPoiIds, startedAt
    │   ├── repositories/
    │   │   └── circuits_repository.dart
    │   ├── providers/
    │   │   ├── circuits_provider.dart         # FutureProvider<List<Circuit>>
    │   │   └── active_circuit_provider.dart   # NotifierProvider<ActiveCircuitNotifier, CircuitProgress?>
    │   ├── pages/
    │   │   └── circuits_list_page.dart
    │   ├── widgets/
    │   │   ├── circuit_card.dart
    │   │   └── circuit_progress_bar.dart
    │   └── test/
    │       ├── unit/
    │       └── widgets/
    │
    ├── gamification/                 # Achievements — Phase 2
    │   ├── gamification_domain.dart
    │   ├── models/
    │   │   └── achievement.dart      # Freezed: id, name, description, earnedAt?(DateTime)
    │   ├── providers/
    │   │   └── gamification_provider.dart  # NotifierProvider — listens to analyticsProvider events
    │   ├── pages/
    │   │   └── achievements_page.dart
    │   ├── widgets/
    │   │   └── achievement_toast.dart  # animated overlay, shown via ref.listen in root
    │   └── test/
    │       ├── unit/
    │       └── widgets/
    │
    ├── earthquake/                   # 1755 simulation — Phase 3
    │   ├── earthquake_domain.dart
    │   ├── models/
    │   │   └── simulation_phase.dart  # plain Dart enum: idle|countdown|shaking|collapse|fire|tsunami|aftermath
    │   ├── providers/
    │   │   └── earthquake_provider.dart  # NotifierProvider<EarthquakeNotifier, SimulationPhase>
    │   ├── pages/
    │   │   └── earthquake_page.dart      # LayoutPresets.fullscreen()
    │   ├── widgets/
    │   │   ├── simulation_trigger.dart
    │   │   └── aftermath_stats.dart
    │   └── test/
    │       ├── unit/
    │       └── widgets/
    │
    ├── ai_assistant/                 # GPT Q&A — Phase 3
    │   ├── ai_assistant_domain.dart
    │   ├── models/
    │   │   ├── message.dart           # Freezed: role(enum user|assistant), content, timestamp
    │   │   ├── conversation.dart      # Freezed: messages, contextPoiId?, contextPeriod?
    │   │   └── assistant_context.dart  # Freezed: assembled from POI + timeline + profile
    │   ├── services/
    │   │   └── gpt_service.dart       # HTTP POST to OpenAI; key via dart-define or backend proxy
    │   ├── providers/
    │   │   └── assistant_provider.dart  # AsyncNotifier<Conversation>
    │   ├── pages/
    │   │   └── ai_chat_page.dart
    │   ├── widgets/
    │   │   ├── chat_bubble.dart
    │   │   └── voice_input_button.dart  # Phase 3b
    │   └── test/
    │       ├── unit/
    │       └── widgets/
    │
    ├── quiz/                         # Quiz per building — Phase 3
    │   ├── quiz_domain.dart
    │   ├── models/
    │   │   ├── quiz_question.dart    # Freezed: id, poiId, question, options, correctIndex
    │   │   └── quiz_result.dart      # Freezed: questionId, selectedIndex, answeredAt
    │   ├── repositories/
    │   │   └── quiz_repository.dart
    │   ├── providers/
    │   │   └── quiz_provider.dart    # NotifierProvider<QuizNotifier, QuizState>
    │   ├── pages/
    │   │   └── quiz_page.dart        # route /quiz/:poiId
    │   └── test/
    │       ├── unit/
    │       └── widgets/
    │
    └── unity_integration/            # Flutter wrapper for Unity — Phase 4b (parallel workstream)
        ├── unity_integration_domain.dart
        ├── models/
        │   └── unity_message.dart     # Typed Flutter <-> Unity message protocol
        ├── providers/
        │   └── unity_controller_provider.dart
        ├── pages/
        │   └── unity_viewer_page.dart  # Full-screen Unity view (buildings + tsunami)
        └── test/
            └── unit/
```

**Assets structure (complete target):**
```
assets/
├── 3d_models/
│   └── glasses/
├── audio/
│   └── guide/             # Phase 2: MP3 clips named by poi_id or circuit_id
├── branding/
│   └── Logo.png
├── data/
│   ├── pois.json          # grows: 10 -> 30 -> 100 -> 150
│   ├── circuits.json      # Phase 3: 5 circuits
│   ├── audio_clips.json   # Phase 3: audio clip metadata
│   └── quizzes.json       # Phase 3: quiz questions per POI
├── fonts/
│   └── NotoColorEmoji.ttf
└── images/
    └── panorama/          # mosaic image + period overlay images
```

---

## 4. Domain dependency rules

A domain may only import from layers **at the same level or above**. No exceptions.

```
Level 0 — no dependencies:
  ar_core/
  design/
  utils/i18n/
  utils/zoom/

Level 1 — depends on Level 0:
  layout/
  navigation/
  components/

Level 2 — depends on Level 0-1:
  domains/timeline/
  domains/onboarding/
  domains/analytics/       <- imports nothing from other domains
                              but receives events from domains via analyticsProvider.log()

Level 3 — depends on Level 0-2:
  domains/panorama/        <- depends on ar_core, timeline
  domains/audio_guide/     <- depends on panorama (POI IDs only)
  domains/circuits/        <- depends on panorama (POI IDs), timeline
  domains/quiz/            <- depends on panorama (POI IDs only)

Level 4 — depends on Level 0-3:
  domains/gamification/    <- depends on analytics only (NOT on domain state directly)
  domains/earthquake/      <- depends on timeline; unity_integration (optional upgrade)
  domains/ai_assistant/    <- depends on panorama (context), timeline (context), onboarding (profile)

Level 5 — top-level app domains:
  domains/home/            <- navigates everywhere but imports minimally
  domains/unity_integration/ <- parallel workstream; depends on design/i18n only
```

The `analytics` domain is special: it sits at Level 2 (imports nothing from other domains) but **receives events** from Level 3-4 domains via `analyticsProvider.log(...)`. This is not a circular dependency — analytics only reads; nothing imports from analytics except gamification (Level 4).

---

## 5. Testing strategy

Five layers. Each must be 100% green before moving to the next.

### Layer 1 — Unit tests

**What**: business logic, state management, utilities, providers in isolation  
**Location**: `lib/domains/<domain>/test/unit/`  
**Runner**: `flutter test lib/domains/<domain>/test/unit/ --reporter=compact`

Rules:
- Use `ProviderContainer` to test providers — no widgets, no MaterialApp, no routing
- Load real data (real JSON assets, real models) — do not mock your own code
- Mock only external I/O (HTTP, file system, hardware)
- Cover all state transitions, edge cases (empty, null, max values)

### Layer 2 — Widget tests

**What**: real widgets mounted, user interactions, provider integration with UI, lifecycle issues  
**Location**: `lib/domains/<domain>/test/widgets/`  
**Runner**: `flutter test lib/domains/<domain>/test/widgets/ --reporter=compact`

Rules:
- Mount real widgets with `UncontrolledProviderScope`
- Use real GoRouter configuration (stub page contents if needed)
- Simulate user actions: `tester.tap`, `tester.enterText`, `tester.drag`, `tester.scrollUntilVisible`
- Assert on: widget visibility, provider state, navigation location
- Use REAL widgets, REAL providers, REAL routes
- Only override providers that need hardware (camera, AR, GPS) — replace those with mock implementations

### Layer 3 — Integration tests (PC, no device)

**What**: full app mounted, critical user journeys end-to-end, error paths  
**Location**: `lib/domains/<domain>/test/integration/`  
**Runner**: `flutter test lib/domains/<domain>/test/integration/ --reporter=compact`

Write three types:
1. **Smoke tests** — confirm all main components render without errors; add concise single-line logs
2. **User journey tests** — 2-5 main flows (navigate → interact → assert)
3. **Error path tests** — what happens if repository throws? If data is empty? If network fails?

Rules:
- Full app mounted: `MaterialApp.router` + GoRouter + all real providers
- Mock only hardware providers (camera, AR session, GPS)
- Mock external HTTP (use fake HTTP responses — never call real OpenAI in tests)
- Tests are deterministic — no real API, no real network

**Run all 3 layers together:**
```
flutter test lib/ --reporter=compact
```

### Layer 4 — Device / browser integration tests

**What**: same as Layer 3 but running inside the real app process on a real device or browser  
**Location**: `integration_test/<feature_name>_test.dart`  
**Uses**: `package:integration_test/integration_test.dart`  
**Catches**: real rendering bugs (Impeller, Vulkan), real asset loading, real platform channels

Commands:
```
# Chrome (requires chromedriver matching your Chrome version, on port 4444):
#   1. Download chromedriver from https://googlechromelabs.github.io/chrome-for-testing/
#   2. Start: chromedriver --port=4444
#   3. Run with --profile to avoid DWDS AppConnectionException in debug mode:
flutter drive --driver=test_driver/integration_test.dart \
  --target=integration_test/<feature>_test.dart \
  -d chrome --profile

# Phase 2 integration test files (one command per domain):
flutter drive --driver=test_driver/integration_test.dart --target=integration_test/consent_onboarding_test.dart -d chrome --profile
flutter drive --driver=test_driver/integration_test.dart --target=integration_test/favourites_test.dart -d chrome --profile

# Real Android device:
flutter test integration_test/<feature>_test.dart -d <PHONE_DEVICE_ID>
```

**Known Chrome setup notes (Flutter 3.41):**
- `flutter test ... -d chrome` is NOT supported for `integration_test/` files — use `flutter drive`.
- `flutter drive -d chrome` in `--debug` mode raises `AppConnectionException` (DWDS issue). Use `--profile`.
- ChromeDriver must match your installed Chrome version exactly. Check Chrome version via:
  `reg query "HKEY_CURRENT_USER\Software\Google\Chrome\BLBeacon" /v version`
  Then download matching driver from `https://googlechromelabs.github.io/chrome-for-testing/`.
- The `--pwa-strategy` deprecation warning in output is benign — it does not affect test results.

### Layer 5 — Manual browser tests with live iteration

This layer is a continuous iterate-until-green loop. Each cycle:

a) Update floating widget task steps in `lib/test_utils/test_tasks_floating.dart` + `lib/test_utils/test_config.dart`

b) Add concise single-line debug logs to trace the flow. Rules:
   - Only log what helps identify failures — no noise
   - Single line, no emojis (terminal encoding issues)
   - Remove logs that are no longer needed before the next iteration

c) Run: `flutter run -d chrome > __out.txt 2>&1`

d) Pause: do not summarize, just wait for approval to proceed

e) (User action) — run test steps in browser, check each floating widget checkbox, approve

f) Read `__out.txt` — logs after each checkbox mark show what happened at each step. Check for `[TEST] !!!!!` notes.

g) Fix errors — think big picture first, consider 3 options, implement best one

h) Loop back to (a) immediately without announcing the next iteration. Iterate silently until 100% green.

**Layer order rule**: do not move to the next layer until the current one is 100% green.

---

## 6. Implementation phases

---

### Phase 0 — Cleanup and foundation

**Goal**: One clean, zero-debt codebase. No new features. **Estimated: 3-4 weeks.**

> **Why 3-4 weeks, not 1-2:** Each layout migration (old system → LayoutManager) requires reading the old implementation, understanding LayoutSlots, migrating, and verifying tests still pass. If there are 10+ pages on the old system, that alone is 1-2 weeks. Add: Freezed regeneration + regression checking after the TranslatableString model change, renaming files and updating every import, fixing design-system violations across multiple widgets, and validating the full test suite at the end. Underestimating Phase 0 is how technical debt doesn't get paid — people skip it because they're "already behind." Do not start Phase 1 early. The cleanup is the prerequisite, not an optional polish step.

All Phase 0 items are prerequisites. Nothing in Phase 1 starts until Phase 0 verification passes.

#### 0.1 Remove `photo_view` from pubspec

`photo_view` is listed in `pubspec.yaml` but has no imports in `lib/`. Dead weight — remove it.

- [ ] Remove `photo_view: ^0.15.0` from `pubspec.yaml`
- [ ] `flutter pub get` — confirm no missing reference errors

#### 0.2 Rename typo files (update all imports)

- [ ] `lib/layout/widgets/breadcrums.dart` → `breadcrumbs.dart`
- [ ] `lib/layout/widgets/fab_wraper.dart` → `fab_wrapper.dart`
- [ ] Search all `.dart` files for old names, update every import
- [ ] `flutter analyze` — 0 issues after rename

#### 0.3 Add `spanish` to `Language` enum and nullable `es` to `TranslatableString`

Do this now in Phase 0. Cost is zero now; cost is enormous in Phase 4 with 150 POIs.

- [ ] Add `spanish` value to `Language` enum in `language.dart`
- [ ] Add `final String? es;` to `TranslatableString` Freezed model (nullable, no default)
- [ ] Update `translate(Language language)` method: if `language == Language.spanish`, return `es ?? en` (fall back to English if `es` is null)
- [ ] Run `dart run build_runner build --delete-conflicting-outputs`
- [ ] Update `language_provider.dart` to handle Spanish (SharedPreferences key, initial load)
- [ ] Update `language_switcher.dart` to show ES option
- [ ] `flutter analyze` → 0 issues
- [ ] `flutter test` → all passing (no regressions — `es` is nullable so all existing code still works)
- [ ] Do NOT fill in `es` strings on existing code yet — that happens as each widget/model is touched

#### 0.4 Fix design-system violations in existing code

- [ ] `ar_poi_marker.dart`: replace `Colors.amber/purple/blue/cyan/grey` with `context.*` design token extensions (see `PHASE_0_PLAN.md §0.4a` for exact mappings)
- [ ] `language_switcher.dart`: replace `Colors.green` checkmark with `context.success`
- [ ] `components/feedback/` files (`empty_state.dart`, `error_display.dart`, `async_value_builder.dart`): **already clean** — they use `theme.colorScheme.*`. Verify and skip.
- [ ] `panorama_ar_view.dart`: `Colors.black`/`Colors.white` AR overlay colors are intentional — add comment, do not change (`poi_marker.dart` was deleted in Phase 1 Session 4)

#### 0.5 Remove demo routes from production nav

- [ ] In `nav_config.dart`, set `showInNav: false` + `showInBreadcrumb: false` on `demo-nav1..4`
- [ ] Gate `demo-nav*` routes behind `kDebugMode` — remove entirely from release builds
- [ ] Remove `poi-calibrator` route from `nav_config.dart` entirely — the calibrator is no longer an in-app page (see §0.7)

#### 0.6 Remove test marker from home page

- [ ] Delete "Hello from TileStories! 👋 (phone test v1)" string from `home_page.dart`

#### 0.7 Move calibrator HTML tool to PROJECT_GUIDES; remove in-app Dart page

The POI calibration workflow uses the HTML file directly in a browser — no Flutter integration needed. The Dart in-app `poi_calibrator_page.dart` adds complexity and a kDebugMode dependency without benefit.

- [ ] Move `lib/ar_core/utils/ar_poi_calibrator.html` → `PROJECT_GUIDES/ar_poi_calibrator.html`
- [ ] Add a clear comment block at the top of the HTML file explaining: what it is, how to use it (open in browser, load panorama image, click to record x/y coordinates, copy to pois.json), and that it is a standalone dev tool — not part of the Flutter app
- [ ] Delete `lib/ar_core/utils/poi_calibrator_page.dart` from the codebase
- [ ] Remove `calibrator_poi_model.dart` from `lib/ar_core/utils/` if it is only used by the calibrator page — but check first: it also contains viewport math utilities (`computeFitToViewport`, `computeMarkerScreenPosition`) that may be reusable. Extract those to `viewport_math.dart` before deleting (see `PHASE_0_PLAN.md §0.7d`)
- [ ] Remove the poi-calibrator route from `nav_config.dart` (see §0.5)
- [ ] Update any references to the calibrator in README or other docs

#### 0.8 Accessibility baseline

Accessibility is built into every widget from day 1. In Phase 0, set the baseline so all future work starts from a clean foundation.

- [ ] Verify design token color pairs meet WCAG AA contrast ratios:
  - `primary` on `surface`: ≥ 4.5:1
  - `onPrimary` on `primary`: ≥ 4.5:1
  - `error` on `surface`: ≥ 4.5:1
  - Large text (≥ 18pt / 14pt bold): ≥ 3:1
  - Check both light and dark themes
- [ ] Add `SizeTokens.tapTarget = 48.0` to `size_tokens.dart` if not already present
- [ ] Add `Semantics` wrappers to existing interactive elements in `nav_tabs_row.dart`, `hamburger.dart`, and `nav_accordion.dart`
- [ ] Ensure all existing `IconButton` widgets have `Tooltip`

#### 0.9 Phase 0 verification

- [ ] `flutter analyze` → 0 issues
- [ ] `flutter test` → all passing
- [ ] `dart run build_runner build --delete-conflicting-outputs` → 0 errors
- [ ] App runs on physical Android device (mock AR mode)
- [ ] Language switcher shows PT / EN / ES options
- [ ] All existing `IconButton` elements have `Tooltip`

---

### Phase 1 — AR Proof of Concept

**Goal**: Working AR panorama with 30 POIs, offline and live modes. Timeline slider. This is the proof-of-concept — prove the core feature works before designing the full app. **Estimated: 3 months.**

> **Prerequisite**: Phase 0 verification complete.

> **Why POC first:** The AR panorama is the unique value of the app. Everything else (home page, onboarding, gamification) can be added around it once it works. Design the app shell only after you know the core interaction is solid. Getting home page polish right before the AR works is wasted effort.

> **Accessibility rule (mandatory from here on)**: every widget written in this phase must satisfy the accessibility checklist in §7 before it is considered done. Not deferred.

> **i18n rule (mandatory from here on)**: every `t(pt: '...', en: '...')` must also include `es: '...'`. No exceptions.

#### 1.0 AR tracking validation gate

> **GATE: Do not write any AR or coordinate code until all four steps are resolved.**
> See `PROJECT_GUIDES/phase_1_plan.md` §Task 1.0 for full spec.

- [ ] Run `arcoreimg eval-img` on panorama scan — score >= 75 required (download from https://github.com/google-ar/arcoreimg)
- [ ] Physical panel field test on >= 2 Android devices — stable tracking within 10s
- [ ] If tracking fails: document fallback strategy (section-based or fiducial) in `AR_TRACKING_STRATEGY.md` and update coordinate plan before starting Task 1.1
- [ ] Panorama JPEG performance test on oldest target device — no OOM crash, render stall < 1s
- [ ] `AR_TRACKING_STRATEGY.md` written with results

#### 1.1 Expand POI data to 41 buildings ✅

**File:** `assets/data/pois.json`

Use `PROJECT_GUIDES/ar_poi_calibrator.html` (open in browser, no Flutter needed) to set accurate `x/y` coordinates. Do not guess.

- [x] Research additional buildings from the panel against museum sources
- [x] Use calibrator tool for accurate coordinates on each
- [x] Each entry: `id`, `name`(pt+en+es), `category`, `x`, `y`, `description`(pt+en+es), `survivalStatus` (string: `"intact"` | `"damaged"` | `"destroyed"`)
- [x] Validate: run app, confirm all 41 markers render in correct positions (expanded from original 30 target to 41 in Phase 1 Session 5)

#### 1.2 Timeline domain ✅

**New folder:** `lib/domains/timeline/`

> **Why top-level, not inside `panorama/`:** Timeline is consumed in Phase 1 by panorama (filter POIs), and later by earthquake (before-state) and AI assistant (context injection). Extracting it mid-project requires renaming files and updating provider paths across multiple domains. Revisit this decision at implementation time — if a Phase-1-only MVP is in scope, put it in `panorama/` instead and extract later. For this plan, it stays top-level.

`TimePeriod` is a **plain Dart enum** (not Freezed). Freezed is only justified for unions, JSON deserialization, or complex copyWith — a 4-value epoch enum with static data needs none of that. Each value carries `year`(int), `label`(TranslatableString), and `description`(TranslatableString). `accentColor` is NOT a field on the enum — it belongs in the widget layer (an extension method or a local switch in the widget) to keep the model free of UI concerns.

- [x] `TimePeriod` **plain Dart enum** with the four values: `pre1755`, `earthquake`, `pombalina`, `today`
- [x] `TimelineNotifier extends Notifier<TimePeriod>` — initial `pre1755`; `setEpoch(TimePeriod)` method
- [x] `timelineProvider = NotifierProvider<TimelineNotifier, TimePeriod>`
- [x] `TimelineSlider` widget — 4 labelled stops, design-system styled, full `Semantics` wrapper
- [x] `EpochLabel` widget — current epoch name + year
- [x] `timeline_domain.dart` barrel
- [x] Wire `timelineProvider` into `panorama_ar_view.dart`: filter visible POIs by `survivalStatus` (`'intact'`/`'damaged'`/`'destroyed'`) vs selected period
- [x] Unit tests (Layer 1): epoch transitions, label in PT / EN / ES
- [x] Widget tests (Layer 2): slider renders 4 stops, tap changes provider state

#### 1.3 ViewportState provider ✅

**New files:** `lib/ar_core/viewport/viewport_state.dart` + `viewport_state_provider.dart` + `frame_widget_provider.dart`

Abstraction over the current zoom level and visible region of the panorama — identical concept in offline mode (derived from `TransformationController`) and live AR mode (derived from ARCore anchor pose). Downstream code (LOD service, node manager) never reads `TransformationController` directly.

- [x] `ViewportState` Freezed class: `zoomLevel`, `centerNormalized`, `visibleRegionNormalized`, `ViewportMode` (offline | liveAR)
- [x] `LODTier` enum: `overview` (<2x), `mid` (<4x), `close` (<8x), `intimate` (8x+) — computed getter on `ViewportState` (if/else, not switch on double)
- [x] `ViewportStateNotifier extends Notifier<ViewportState>` — `updateFromMatrix(Matrix4, Size, Size)` for offline; `updateFromARPose(...)` stub sets `mode = liveAR`; `setMode(ViewportMode)` for toggle pill
- [x] `viewportStateProvider = NotifierProvider<ViewportStateNotifier, ViewportState>`
- [x] `frameWidgetProvider` — returns `StaticImageFrameProvider` or `ARCameraFrameProvider` based on `viewportStateProvider.mode` (Layer A/B bridge)
- [x] Wire `updateFromMatrix` into `StaticImageFrameProvider` on `TransformationController` change
- [x] Unit tests (Layer 1): identity matrix → zoom 1.0, center (0.5, 0.5); 2x scale → zoom 2.0; all `LODTier` thresholds; `updateFromARPose` sets liveAR mode

#### 1.4 POIVisibilityService ✅

**New file:** `lib/domains/panorama/services/poi_visibility_service.dart`

Pure business logic — no UI, no AR. Takes all POIs + viewport + epoch, returns ordered `List<POIRenderSpec>`. Single source of truth for "what renders and how." Architecture supports Phase 2 enriched markers and Phase 3 3D models without restructuring `PanoramaARController`.

- [x] `POIRenderMode` enum: `hidden`, `simpleMarker`, `enrichedMarker` (Phase 2), `model3DLow` (Phase 3), `model3DHigh` (Phase 3)
- [x] `POIRenderSpec` plain Dart class: `poi`, `mode`, `opacity` (1.0 normal, 0.45 for destroyed in earthquake epoch)
- [x] `POIVisibilityConfig.phase1()` const: `maxMarkersOverview = 30`, `maxMarkersMid = 30`, all 3D limits = 0
- [x] `POIVisibilityService.computeVisible()` pure function: epoch filtering, opacity logic, priority ordering
- [x] Wire into `PanoramaARController` — replaces inline POI filtering; stores result in `PanoramaARState.renderSpecs`
- [x] Unit tests (Layer 1): all 30 visible for pre1755; destroyed opacity 0.45 for earthquake; hidden for pombalina/today; empty list no crash

#### 1.5 ARNodeManager architecture stub ✅

**New files:** `lib/ar_core/nodes/ar_node_manager.dart` (abstract) + `mock_ar_node_manager.dart` + `lib/domains/panorama/ar/config/panorama_3d_nodes_config.dart`

**3D GLB model loading is deferred to Phase 3** (GLB sizes risk APK bloat; panel dimensions must be confirmed with museum before coordinate math; Phase 1 is the POC). Build the abstract interface now so Phase 3 swaps the implementation behind one provider change.

- [x] `ARNodeManager` abstract interface: `placeModel`, `removeModel`, `removeAll`, `enableFocusMode` (stub), `disableFocusMode` (stub), `onModelTapped` stream
- [x] `ARNode3DConfig` plain Dart class: `poiId`, `assetPath`, `scale`, `zOffset`
- [x] `MockARNodeManager` — all methods are no-ops, `onModelTapped` is empty stream
- [x] `kAR3DNodes = const []` in `panorama_3d_nodes_config.dart` — **empty in Phase 1**
- [x] `arNodeManagerProvider = Provider<ARNodeManager>((ref) => MockARNodeManager())` in `ar_infrastructure_providers.dart`
- [x] Wire into `PanoramaARController`: loop over `kAR3DNodes` on anchor detection (empty → no-op)
- [x] Unit tests (Layer 1): all mock methods no-throw; controller never calls `placeModel` when `kAR3DNodes` empty

#### 1.6 Panorama page chrome and navigation ✅

**Files:** `panorama_ar_view.dart` + new extracted widgets: `panorama_top_bar.dart`, `ar_tracking_indicator.dart`, `ar_first_time_overlay.dart` (update `ar_mode_toggle.dart`; `panorama_fab.dart` is Phase 2)

- [x] **_TopBar** (`panorama_top_bar.dart`): semi-transparent (`context.surface.withValues(alpha: 0.92)` + `ElevationTokens.level2` shadow — no `BackdropFilter`, banned in non-static contexts per `DESIGN/08_MOTION_AND_FEEL.md`) back button (`context.go('/')`) + options menu; replaces browser-style `header.dart` (not appropriate for full-screen immersive)
- [x] **ARTrackingIndicator** (`ar_tracking_indicator.dart`): 3 states (searching / initializing / tracking); auto-hides 3s after `tracking`; not shown in offline mode; `Semantics(liveRegion: true)`; no emoji in text
- [x] **ARModeToggle** (`ar_mode_toggle.dart`): redesigned as Variant-A animated track — lives inside `PanoramaTopBar` (not Positioned in Stack); `AnimatedContainer(72×40)` track, `AnimatedPositioned` 32dp thumb, 8dp blink dot (900ms repeat). See `phase_1_plan.md §Implementation notes` for full spec. **Test rule:** never use `pumpAndSettle()` in any test context that includes this widget — use bounded `pump(Duration(milliseconds: 500))` instead.
- [x] **POILegendButton** (`poi_legend_button.dart`): 36dp ⓘ button at `Stack[8] Positioned(top: kToolbarHeight, right: 0)`; opens `POILegendSheet` (`DraggableScrollableSheet` with destruction-spectrum bar + 8-type GridView). See `phase_1_plan.md §Implementation notes`.
- [x] **ARPOIMarker** (`ar_poi_marker.dart`) + **`marker_icons.dart`**: 4 LOD tiers (`MarkerScaleTier` enum — large 48dp / medium 36dp / small 24dp / micro 16dp); `_DashedBorderPainter` 6-stop destruction spectrum; `ARPOIMarkerIcon` 8 `CustomPainter` icons (large/medium only). See `phase_1_plan.md §Implementation notes`.
- [ ] **_ExpandableFAB DEFERRED TO PHASE 2** — a disabled/null FAB renders greyed in Material 3 and looks broken in a museum installation. `panorama_fab.dart` is created in Phase 2 when the Progress tracker sub-action is real. See Phase 2 §2.6.
- [ ] **ARFirstTimeOverlay** (`ar_first_time_overlay.dart`): 2-step coach mark shown once per install (SharedPreferences `ar_onboarding_shown`); only in live AR mode; step 1 = point at panel, step 2 = tap a marker; full Semantics; `Colors.black.withValues(alpha: 0.72)` backdrop
- [ ] **Epoch color wash**: `ColorFiltered` overlay on panorama tied to `timelineProvider` — subtle tint per epoch for temporal context
- [ ] **PopScope priority stack** in `panorama_ar_page.dart`: POISummaryCard open → close card; POIDetailSheet open → close sheet; nothing open → `context.go('/')`
- [ ] **Deep link**: `?poi=<id>` query param on `/panorama` route in `nav_config.dart`; sets `selectedPOIProvider` on load
- [x] Widget tests (Layer 2): back button navigates; PopScope closes card before navigating; tracking indicator states correct; auto-hide works. L1–L4 all green (1134 lib/ tests passing — Phase 1 final baseline).

#### 1.7 POISummaryCard and POIDetailSheet ✅

**New:** `poi_summary_card.dart`; **Rename + enrich:** `poi_info_sheet.dart` → `poi_detail_sheet.dart`; **New:** `poi_action_buttons.dart`

- [ ] **POISummaryCard**: compact overlay anchored near tapped marker (not bottom sheet); 280dp wide; scale+fade entrance (`AnimationTokens.fast`, `luxurySpring`); `InfoBadge` row; one-line description; dismiss X + tap-outside; `POIActionButtons` row (`[Mais Info]` / `[3D]` stub hidden until `kAR3DNodes` has entry / `[360]` Phase 4)
- [ ] **POIDetailSheet** (renamed from `poi_info_sheet.dart`): `DraggableScrollableSheet`; epoch context line that changes with `timelineProvider`; survival badge (`InfoBadge`); share button via `share_plus`; "Ver no Mapa" disabled stub
- [ ] Add `share_plus` to `pubspec.yaml`
- [ ] **POIActionButtons** extracted row reused by both card and sheet
- [ ] Rename `POIInfoSheet` → `POIDetailSheet`, update all imports
- [ ] Widget tests (Layer 2): card renders + dismisses; detail sheet epoch context updates; share fires; `[3D]` hidden when `kAR3DNodes` empty

#### 1.8 Shared UI primitives — `components/ui/` ✅

- [x] `info_badge.dart` — colored pill badge; `label`, `color` (context.* only); paired icon + text for survival (color never sole indicator)
- [x] `section_header.dart` — titled section divider with optional action widget
- [x] `tappable_card.dart` — base card: InkWell + ElevationToken + RadiusToken + press scale 0.97 + `Semantics`

#### 1.9 Home page placeholder ✅

**File:** `lib/domains/home/pages/home_page.dart` — minimal Phase 1 placeholder; full design is Phase 2.

- [x] `LayoutPresets.defaultPageApp()`, `AppLogo`, one-line description in PT/EN/ES
- [x] "Explorar com AR" `FilledButton`: camera permission → granted go `/panorama`; denied → dialog with "Explorar sem AR" fallback; permanently denied → direct to settings
- [x] "Explorar sem AR" `OutlinedButton`: no permission request → go `/panorama` (offline mode default)
- [x] Language + theme switcher visible (top-right, icon-only)
- [x] Widget tests (Layer 2): both CTAs render; permission denied dialog shows fallback

#### 1.10 Route stubs for Phase 2+ domains

**File:** `lib/navigation/navConfig/nav_config.dart` — `showInNav: false` stubs to prevent 404s on future deep links.

- [ ] `/circuits`, `/achievements`, `/ai-chat`, `/quiz/:poiId` — all point to inline `_ComingSoonPage` widget
- [ ] `flutter analyze → 0` after adding stubs

#### 1.11 Phase 1 verification (all 5 test layers)

> **Phase 1 Layers 1–2 are COMPLETE.** Final baseline: `flutter test lib/` → **+1134 ~3: All tests passed**, `flutter analyze` → **0 errors** (59 info-level deprecation warnings in `integration_test/` only — pre-existing). Sessions 7A+7B+7C resolved all MUST FIX and SHOULD FIX audit items.

- [x] Layer 1 unit tests: timeline, viewportState, poiVisibilityService, arNodeManager mock — all green
- [x] Layer 2 widget tests: panorama chrome, summaryCard, detailSheet, timelineSlider, homePage — all green
- [x] Layer 3 integration: home → panorama → tap POI → summary card → detail sheet → change epoch → share → back
- [x] `flutter test lib/ --reporter=compact` → all passing (+1134 ~3)
- [ ] Layer 4: `flutter test integration_test/panorama_ar_test.dart -d <device>` — Android + iOS (physical device step, pending museum visit)
- [ ] Layer 5: manual browser — all 21 scenarios green (see `phase_1_plan.md` §Testing Layer 5)
- [x] All strings PT + EN + ES — no hardcoded text, no English fallback in PT/ES mode
- [x] `flutter analyze` → 0 errors; no `StateProvider` in new code

---

### Phase 2 — App design and integration

**Goal**: Design the complete app shell. Implement the home page and entry flow. Wire analytics and GDPR consent. App store release. **Estimated: 2 months.**

> **See `PHASE_2_PLAN.md` for the canonical task tracker for this phase.**

> **Progress (as of Phase 2 Sessions 1–8):**
> - ✅ Task 2.1 — ES strings, `cached_network_image` removal, Tooltip fixes
> - ✅ Task 2.2 — Analytics domain (13 files, `AnalyticsService`, `LocalSQLiteBackend`, `CompositeBackend`)
> - ✅ Task 2.3 — Consent page + GoRouter redirect + `main.dart` wiring (Layers 1–4 green)
> - ✅ Task 2.4 — Onboarding domain: `ProfileType`, `UserProfile`, `OnboardingNotifier`, `ProfileCard`, `ProfileSelector`, `OnboardingPage` (Layers 1–4 green; 1300 `lib/` tests passing)
> - ✅ Task 2.5 — Favourites domain: `FavouritesNotifier`, `FavouritesPage`, `FavouriteToggleButton` (Layers 1–4 green; 1364 `lib/` tests passing; 15/15 Chrome integration tests)
> - Remaining: Tasks 2.6–2.11

> **Key technical discoveries made during Phase 2:**
> - `addTearDown(handle.dispose)` does NOT work for `SemanticsHandle` in Flutter 3.41. `_endOfTestVerifications()` runs before teardown callbacks. Always call `handle.dispose()` inline at the end of the test body.
> - `tester.ensureVisible(finder)` is required before tapping any widget that may be below the 800×600 test viewport. `warnIfMissed: false` is NOT equivalent — it silently skips the tap and the assertion then fails.
> - Layer 4 on Chrome requires `flutter drive --driver=test_driver/integration_test.dart --target=... -d chrome --profile`. The `--debug` mode raises `AppConnectionException` (DWDS). Use `--profile` instead.
> - ChromeDriver must match the installed Chrome version exactly.
> - **`defaultPageBrowser(scrollable: false)` is required for pages with `Expanded + ListView` body.** `LayoutManager` wraps the body in `SingleChildScrollView` by default. `Expanded` inside `SingleChildScrollView` = unbounded height = `RenderFlex` crash. Both in production AND in widget tests. `defaultPageBrowser()` now accepts `bool scrollable = true`; self-scrolling pages pass `scrollable: false`. This was a production bug discovered via Task 2.5 tests (see D21 in `PHASE_2_PLAN.md §2`).
> - **`ConsumerWidget` + `Dismissible`: capture `notifier` before the widget unmounts.** When `Dismissible` dismisses a tile, the `ConsumerWidget` unmounts. Any closure (e.g. SnackBarAction `onPressed`) that calls `ref.read(provider.notifier)` after unmount throws "ref used after unmount". Fix: `final notifier = ref.read(favouritesProvider.notifier)` captured before building the `Dismissible`.
> - **`semantics.flagsCollection.isToggled` returns `dart:ui.Tristate`, not `bool`.** The matchers `isFalse`/`isTrue` compare against `bool` and always fail on `Tristate`. Import `'dart:ui' show Tristate` and compare with `Tristate.isFalse`/`Tristate.isTrue`.
> - **Never-completing futures in tests**: use `Completer<T>()` (no timer) — not `Future.delayed(Duration(seconds: 60))` (creates a pending fake timer that fails `!timersPending` at teardown).
> - **`Dismissible` on Chrome (Layer 4)**: `tester.drag()` has no velocity — use `tester.fling(finder, Offset(-300, 0), 800)`.
> - **`SharedPreferencesAsync` in tests**: requires `SharedPreferencesAsyncPlatform.instance = InMemorySharedPreferencesAsync.empty()` in `setUp()`. `SharedPreferences.setMockInitialValues({})` only covers the legacy API.

> **Prerequisite**: Phase 1 verification complete. The core AR feature works — now build the app around it.

> **This is the design phase.** Phase 1 proved the AR POC works. Now stop and design the full app: what does the user see first? How do they navigate? What is the visual language? Answer these questions before adding more features. A well-designed shell makes Phase 3+ features easier to integrate.

> Accessibility and i18n rules continue. Every new widget: Semantics, Tooltip, tap target, `es` strings.

#### 2.1 Home page — entry flow

**File:** `lib/domains/home/pages/home_page.dart`

The entry experience must orient the museum visitor quickly (they are standing in front of a 23-metre painting). Design goal: 15 seconds from app open to first interaction.

- [ ] Context screen: painting title (Grande Panorama de Lisboa), date (c. 1700), museum name, one-paragraph description — single screen, no scroll wall
- [ ] Two entry paths clearly presented: "Explorar com AR" and "Explorar sem AR" (static image fallback)
- [ ] "Explorar com AR" path: camera permission request → if granted open `PanoramaARPage`; if denied show explanation and offer static mode
- [ ] Static image fallback: `InteractiveViewer` with panorama image + tappable POI markers — same info sheet, no camera required
- [ ] Phase 2 profile: `general` by default; profile selection available but not blocking (defer to §2.3)
- [ ] All strings: `t(pt: '...', en: '...', es: '...')`
- [ ] Widget tests (Layer 2): home renders correct CTAs, permission-denied path shows fallback

#### 2.2 Analytics domain

**New folder:** `lib/domains/analytics/`

> Build analytics infrastructure here (before gamification in Phase 3 depends on it).

- [ ] `AnalyticsEventType` enum: `sessionStart`, `sessionEnd`, `poiTapped`, `timelineChanged`, `circuitStarted`, `circuitCompleted`, `audioPlayed`, `earthquakeWatched`, `aiQuestionAsked`, `achievementEarned`, `profileSet`, `quizAnswered`
- [ ] `AnalyticsEvent` Freezed: `eventType`, `timestamp`, `metadata`(Map<String,dynamic>)
- [ ] `AnalyticsSession` Freezed: `sessionId`, `startedAt`, `endedAt`
- [ ] `AnalyticsService` — `logEvent()` appends JSON line to `dart:io` file; `exportSession()`; `clearOldSessions(int keepDays)`
- [ ] `analyticsProvider = Provider<AnalyticsService>` singleton
- [ ] Wire `sessionStart` / `sessionEnd` events in `main.dart`
- [ ] Wire `poiTapped` and `timelineChanged` events; other events wired as their domains are built
- [ ] Unit tests (Layer 1): event serialization, file append correctness, export, pruning

#### 2.3 GDPR consent and thesis ethics

> **This is not optional.** Portugal is EU jurisdiction. The app collects behavioral data (which POIs users tap, how long sessions last, which circuits they complete). Under GDPR, this requires explicit consent before collection begins. For a university thesis, the ethics board must approve the data collection methodology before you run the study. Ethics approval at Portuguese universities takes 4-12 weeks — **submit immediately at the start of Phase 2, not at the end.**

- [ ] **Ethics board submission** (submit at start of phase): what data is collected, how stored (local device only), how anonymized, how participants withdraw. Do not wait until Phase 3.
- [ ] **Consent screen**: hard gate before any analytics event — "Accept" enables logging, "Decline" means app works fully with no events logged
- [ ] `consentProvider = NotifierProvider<ConsentNotifier, ConsentState>` — `ConsentState`: `{notAsked | accepted | declined}`; persisted in SharedPreferences
- [ ] Wire into `main.dart` redirect: `consentState == notAsked` → redirect to `/consent`; analytics only logs if `consentState == accepted`
- [ ] `AnalyticsService.logEvent()` silent no-op if consent declined
- [ ] Consent text in PT, EN, ES; plain language
- [ ] "Data privacy" in settings: shows what is collected + "Delete my data" button (clears analytics file)

#### 2.4 App store preparation

> **Timeline note:** the original plan had store release in Phase 1. This plan moves it to Phase 2 — releasing after home page and GDPR consent are done, not just after the AR POC. This is the right call (releasing a POC without a proper home screen or consent flow is a bad first impression), but it means the first public release is 2 months later than the original plan. Plan accordingly for thesis timeline. The Phase 2 release includes the full Phase 1 AR POC — do not build a separate "Phase 2 only" release.

- [ ] Add `flutter_launcher_icons` as dev dependency; generate and place icons
- [ ] Set `applicationId` in `android/app/build.gradle.kts`
- [ ] Set `PRODUCT_BUNDLE_IDENTIFIER` in iOS
- [ ] `NSCameraUsageDescription` in `ios/Runner/Info.plist`
- [ ] `minSdkVersion 24` Android (ARCore)
- [ ] iOS deployment target 14.0
- [ ] Store descriptions PT, EN (ES in Phase 5)
- [ ] App Store screenshots + Google Play screenshots
- [ ] `flutter build apk --release` → verify < 100MB
- [ ] `flutter build ipa --release`
- [ ] TestFlight + Google Play Internal Testing; fix critical issues

#### 2.5 Phase 2 verification (all 5 test layers)

- [ ] Layer 1 unit tests: analytics, consent — all green
- [ ] Layer 2 widget tests: home_page entry flow, consent screen — all green
- [ ] Layer 3 integration tests: home → AR path; home → static path; consent gates analytics
- [ ] `flutter test lib/ --reporter=compact` → all passing
- [ ] Layer 4: device test — home page, both entry paths, consent flow
- [ ] Layer 5: manual browser test — home page correct, entry flow works, consent shown on first launch
- [ ] Consent screen shown on first launch; analytics events only log after consent
- [ ] All strings in PT, EN, ES
- [ ] Release APK < 100MB
- [ ] All Phase 2 widgets have Semantics + Tooltips + ≥ 48px tap targets

#### 2.6 Panorama page — Phase 2 enhancements (deferred from Phase 1)

These items were evaluated and deferred during Phase 1 implementation. Implement them in Phase 2 when the app shell design is stable.

**Expandable FAB (`panorama_fab.dart`):**
- [ ] Introduce `lib/domains/panorama/ar/widgets/panorama_fab.dart` with expandable pattern
- [ ] Phase 2: Progress sub-action is the first real action (wired to `gamification` progress summary)
- [ ] Audio Guide, Circuits, AI Guide sub-actions remain stubs (`Tooltip('Em breve')`) until their Phase 3/4 domains are built
- [ ] Staggered upward reveal animation (`AnimationTokens.r1`–`r4`); main button `Icons.explore_rounded`
- [ ] Position: `Positioned(bottom: Spacing.xl2 + 80, right: Spacing.lg)` above mode toggle pill
- [ ] `Semantics(label: t(pt: 'Accoes de exploracao', en: 'Exploration actions', es: 'Acciones de exploracion'))`
- [ ] Add to panorama Stack as layer `[6]`; update execution order

**POIVisibilityConfig promoted to provider:**
- [ ] Phase 1 hardcodes `const POIVisibilityConfig.phase1()` in `PanoramaARController.build()`. Phase 2: promote to `NotifierProvider<POIVisibilityConfigNotifier, POIVisibilityConfig>` so LOD config can vary (e.g. different maxMarkers per profile type)

**Marker clustering:**
- [ ] Phase 1 has a minimal "hide label if within 40dp" rule. Phase 2: implement full `ClusterMarker` grouping — markers within a cluster radius become a count badge (`Positioned` with number overlay). Tap expands to individual markers. Required for 100 POIs in Phase 4.

**Route stubs (deferred from Phase 1 Task 1.10):**
- [ ] Add `/circuits`, `/achievements`, `/ai-chat`, `/quiz/:poiId` routes to `nav_config.dart` (`showInNav: false`) pointing to `_ComingSoonPage` widget. Prevents 404 on future deep links. These were deferred from Phase 1 because dead routes in a museum installation are confusing.

**Deep link domain:**
- [ ] Replace `tilestories.app` placeholder domain in share URLs (`poi_detail_sheet.dart`) with the real domain before any store release. Configure GoRouter deep link handling in `nav_config.dart`.

---

### Phase 3 — Experience Core

**Goal**: User profiles. Audio guide. 5 themed circuits. Quiz per building. Achievements and leaderboard. **Estimated: 3 months.**

> **Prerequisite**: Phase 2 verification complete.

> Accessibility and i18n rules continue. Every new widget: Semantics, Tooltip, tap target, `es` strings.

#### 3.1 Onboarding domain

**New folder:** `lib/domains/onboarding/`

- [ ] `ProfileType` Freezed enum: `architecture | history | child | general` — each with `TranslatableString` label + description (all 3 languages)
- [ ] `UserProfile` Freezed: `profileType`, `preferredLanguage`
- [ ] `OnboardingNotifier extends AsyncNotifier<UserProfile?>` — loads from SharedPreferences in `build()`; `setProfile()` saves
- [ ] `welcome_page.dart`, `profile_setup_page.dart`, `profile_selector.dart`
- [ ] Wire redirect in `main.dart`: `onboardingProvider == null` → redirect to `/onboarding`
- [ ] `poi_info_sheet.dart`: adapt content depth per `ProfileType` — architecture gets proportions/style analysis; history gets dates/events; child gets simplified language
- [ ] Log `profileSet` analytics event when profile chosen
- [ ] Unit tests: load/save, null on first run
- [ ] Widget tests: 4 profile cards render, tap sets profile, redirect works

#### 3.2 Audio guide domain

**New folder:** `lib/domains/audio_guide/`  
**New package:** `just_audio: ^0.9.x`

- [ ] `AudioClip` Freezed: `id`, `poiId?`, `circuitId?`, `assetPath`, `durationSeconds`, `title`(TranslatableString all 3 langs)
- [ ] `assets/data/audio_clips.json` + `assets/audio/guide/` (20 MP3 clips, professional voice actor). **Confirm before publishing**: verify that the voice actor recording contract explicitly covers app store redistribution (iOS App Store + Google Play). Do not ship to stores without written confirmation.
- [ ] `AudioClipsRepository` — loads from JSON asset
- [ ] `AudioGuideNotifier extends Notifier<AudioGuideState>` — `play`, `pause`, `seek`, `stop`
- [ ] `AudioPlayerProvider` — `Provider<AudioPlayer>` (just_audio instance, disposed with provider)
- [ ] `AudioControls` widget — play/pause/seek bar; full Semantics
- [ ] `NowPlayingBar` widget — persistent mini-player. **Do not add a new LayoutSlot for this in Phase 3.** In Phase 2 (app design), the full LayoutSlots layout will be reviewed — adding a slot mid-project is a breaking change across all pages that create LayoutSlots. Until that design decision is made, implement NowPlayingBar as a standalone `Overlay` or `Stack` widget rendered above the page content, not via LayoutSlots. Revisit when Phase 2 layout design is finalised.
- [ ] Wire "Play audio" into `poi_info_sheet.dart` — visible only if clip exists for POI
- [ ] Log `audioPlayed` analytics event on play
- [ ] Unit tests: repository, notifier state transitions
- [ ] Widget tests: controls render correct states, NowPlayingBar shows when audio active

#### 3.3 Circuits domain

**New folder:** `lib/domains/circuits/`  
**New asset:** `assets/data/circuits.json`

Five circuits: `earthquake_1755`, `royal_power`, `religious_lisbon`, `daily_life`, `childrens_adventure`

- [ ] `Circuit` Freezed: `id`, `name`(TranslatableString), `description`(TranslatableString), `poiIds`, `recommendedTimePeriod?`(TimePeriod)
- [ ] `CircuitProgress` Freezed: `circuitId`, `visitedPoiIds`(Set<String>), `startedAt`
- [ ] `CircuitsRepository` — loads from JSON asset
- [ ] `circuitsProvider` — `FutureProvider<List<Circuit>>`
- [ ] `ActiveCircuitNotifier extends Notifier<CircuitProgress?>` — `startCircuit`, `markPoiVisited`, `abandonCircuit`
- [ ] `CircuitsListPage` — route `/circuits`
- [ ] `CircuitCard`, `CircuitProgressBar` widgets
- [ ] Wire into `panorama_ar_view.dart`: "Next stop: [name]" overlay when circuit active; highlight next unvisited marker
- [ ] Auto-mark POI visited when user taps the next circuit stop
- [ ] Log `circuitStarted` and `circuitCompleted` analytics events
- [ ] Unit tests: progress logic, completion detection
- [ ] Widget tests: card renders, progress bar fraction, circuit active overlay shows

#### 3.4 Quiz domain

**New folder:** `lib/domains/quiz/`

A short multiple-choice quiz unlocked after a user taps a POI and views the info sheet. Part of gamification and personalisation.

- [ ] `QuizQuestion` Freezed: `id`, `poiId`, `question`(TranslatableString), `options`(List<TranslatableString>), `correctIndex`
- [ ] `QuizResult` Freezed: `questionId`, `selectedIndex`, `answeredAt`
- [ ] `QuizRepository` — loads from `assets/data/quizzes.json`
- [ ] `QuizNotifier extends Notifier<QuizState>` — `startQuiz`, `answerQuestion`, `resetQuiz`
- [ ] `QuizPage` — route `/quiz/:poiId`
- [ ] Wire "Quiz" CTA in `poi_info_sheet.dart` — shown after user opens a building's sheet for the first time
- [ ] Log `quizAnswered` analytics event with `poiId`, `correct`(bool)
- [ ] Unit tests: correct/wrong logic, already-answered state
- [ ] Widget tests: 4 options render, selection shows correct/wrong feedback

#### 3.5 Gamification domain

**New folder:** `lib/domains/gamification/`

> Listens to `analyticsProvider` event stream — NOT directly to domain providers. This keeps gamification decoupled from domain internals.

> **Build order within Phase 3:** build gamification last — after circuits (§3.3), audio (§3.2), and quiz (§3.4) are complete. The `circuitCompleted` and `quizAnswered` events are only emitted by those domains, so gamification achievements that depend on them cannot be meaningfully tested until those domains exist.

Initial 10 achievements: `firstPoiViewed`, `circuitCompleted`, `10PoisViewed`, `30PoisViewed`, `allCategoriesSeen`, `audioPlayed`, `timelineExplored`, `arLiveModeUsed`, `earthquakeWatched`, `appOpened3Times`

- [ ] `Achievement` Freezed: `id`, `name`(TranslatableString), `description`(TranslatableString), `earnedAt?`(DateTime)
- [ ] `AchievementsRepository` — hardcoded definitions
- [ ] `GamificationNotifier extends Notifier<List<Achievement>>` — `build()` loads earned from SharedPreferences; listens to `analyticsProvider` event stream; awards on relevant event
- [ ] `AchievementToast` widget — animated overlay; shown via `ref.listen` near root widget
- [ ] `AchievementsPage` — route `/achievements`; locked achievements greyed
- [ ] **Personal progress summary** (local, no backend): circuits completed, buildings explored, quiz score, achievements earned. This is what "leaderboard" means in Phase 3 — a personal record, not a ranking.
- [ ] `LeaderboardRepository` abstract interface: `getProgress()`, `saveProgress()`. Phase 3 implementation: `LocalLeaderboardRepository` (SharedPreferences). This abstraction is the decision point — if a cloud backend is added later, implement `RemoteLeaderboardRepository` and swap the provider. No other file changes needed.
- [ ] Log `achievementEarned` analytics event when earned
- [ ] Unit tests: award logic, persistence, analytics event trigger → achievement fires
- [ ] Widget tests: toast animation, page locked/unlocked states

> **Future marketing potential (museum-facing):** once a `RemoteLeaderboardRepository` exists, the museum can run month-long challenges ("visit 20 buildings this month"), display a public ranking, and award prizes. This is a meaningful visitor engagement strategy for the museum — worth proposing. Requires: backend (database + API), privacy policy update, GDPR re-assessment. Do not implement until the museum confirms interest.

#### 3.5b User feedback widget (AR tracking quality)

> **Operational note:** if the museum moves or re-lights the panel, the ARCore reference image may fail to track. To detect this early, add a lightweight, opt-in feedback mechanism — users can volunteer as much or as little as they want.

- [ ] A small, unobtrusive "Did AR work well?" 👍/👎 widget visible in AR live mode (bottom corner, auto-dismisses after 5s, can be tapped)
- [ ] Positive tap: log `arFeedback: positive` analytics event (silent)
- [ ] Negative tap: offer optional one-sentence free-text field + send button; store in analytics JSON
- [ ] Progressive: if user dismisses, never show again for that session
- [ ] No backend required — feedback stored locally in analytics export. Later, if a backend is added, export can be transmitted.

#### 3.6 Phase 3 verification (all 5 test layers)

- [ ] `flutter test lib/ --reporter=compact` → all passing
- [ ] Onboarding on fresh install (clear app data)
- [ ] Audio plays on physical device; NowPlayingBar persists across routes
- [ ] Circuit: start → visit all stops → complete → achievement toast fires
- [ ] Quiz: open POI → tap quiz → answer → correct/wrong feedback → score logged
- [ ] Analytics events firing — verify in debug log
- [ ] All routes in `nav_config.dart`
- [ ] All strings PT / EN / ES — no fallback to English for Spanish user
- [ ] All Phase 3 widgets have Semantics + Tooltips + ≥ 48px tap targets
- [ ] Layer 4 device tests for audio and circuits

---

### Phase 4 — Wow Moments

**Goal**: Earthquake simulation. GPT Q&A. 100 POIs. 360° interior views. Unity 3D (parallel workstream). **Estimated: 3 months.**

> **Prerequisite**: Phase 3 verification complete.

> **360° interior views:** A 360° equirectangular photo shown in a spherical viewer — no Unity required. **Package risk:** the `panorama` package (pub.dev, v0.4.1) was last published 4 years ago, has only 140 pub points, and has reported gyroscope issues on newer iOS and rendering bugs on Android 13+. No well-maintained alternative exists on pub.dev as of early 2026. The three realistic options are: (a) `panorama` 0.4.1 — verify it works on current iOS/Android before committing; (b) custom `CustomPainter` equirectangular projection — ~200-300 lines of math, full control, no dependency; (c) WebView into the existing website if it already has 360° views — cheapest if the website is usable on mobile. Evaluate all three at implementation time and document the decision in `PROJECT_GUIDES/INTERIOR_VIEWS.md`.

> **Unity 3D walkthroughs (parallel workstream):** Interactive 3D scenes for Castelo São Jorge, Sé de Lisboa, Paço Ribeira, Jerónimos, Carmo — real-time geometry, user navigation, animated elements. This is a 3-6 month solo side project. Only start this if dedicated Unity dev time exists. The Flutter integration layer (`lib/domains/unity_integration/`) is ready when you are — see §2 for the upgrade path. Do not add `flutter_unity_widget` to pubspec until a Unity project has been tested on a physical device.

#### 4.1 Expand POI data to 100 buildings

- [ ] Use `PROJECT_GUIDES/ar_poi_calibrator.html` for 70 more buildings
- [ ] Historical research: names, descriptions (PT + EN + ES), survival status, category
- [ ] Validate all 100 render correctly at accurate positions

#### 4.2 360° interior views

> **Decision point at implementation time:** check if the existing website can be embedded via WebView or queried via API before re-implementing. Document the decision.

- [ ] Choose implementation approach: native `EquirectangularViewerPage`, WebView, or API integration
- [ ] If native: add `panorama` package or implement equirectangular `CustomPainter`
- [ ] Acquire 360° images for 5 buildings (Castelo, Sé, Paço Ribeira, Jerónimos, Carmo)
- [ ] Wire "Ver interior 360°" in `poi_info_sheet.dart` — enabled only for buildings with images
- [ ] Test on physical device (360° requires gyroscope/sensor access)

#### 4.3 Earthquake simulation

**New folder:** `lib/domains/earthquake/`

**Strategy:** Flutter-native first using `CustomPainter` for particles, `AnimationController` sequences, `HapticFeedback`, `just_audio` for sound. Unity upgrade is a separate parallel workstream.

- [ ] `SimulationPhase` plain Dart enum: `idle | countdown | shaking | collapse | fire | tsunami | aftermath`
- [ ] `EarthquakeNotifier extends Notifier<SimulationPhase>` — Timer-driven sequence; ~3 minute total runtime
- [ ] `EarthquakePage` — `LayoutPresets.fullscreen()`; route `/earthquake`
- [ ] `SimulationTrigger` widget — "Viver o Terramoto de 1755" CTA
- [ ] `AftermathStats` widget — death toll, count of `survived1755 == false` POIs
- [ ] `HapticFeedback.heavyImpact()` at shaking phase
- [ ] Audio via `just_audio`: crowd noise, rumble, bells sequence
- [ ] Wire: after simulation → optionally set `timelineProvider` epoch to `earthquake`
- [ ] Log `earthquakeWatched` analytics event on completion
- [ ] Unit tests: phase transition timer logic, aftermath stats calculation
- [ ] Widget tests: trigger states, aftermath numbers, phase-specific UI

#### 4.4 AI assistant domain

**New folder:** `lib/domains/ai_assistant/`  
**New package:** `http` (if not already a transitive dependency)

> **API key security — backend proxy first, before any other AI code.** `--dart-define` values are embedded in the compiled binary and can be extracted from the APK by anyone. Even a single thesis test distribution leaks the key. Deploy the proxy before writing any user-facing AI code.

> **Backend proxy (required):** Cloudflare Worker or Vercel serverless function — receives `{messages, context}` from app, calls OpenAI with server-side key, returns response. 20-30 lines of JavaScript, free at thesis scale. `GptService` calls `https://your-worker.workers.dev/ask`, never `api.openai.com` directly.

Model: `gpt-4o-mini`. System prompt: museum context + current POI + active epoch + user profile.

- [ ] **Deploy proxy first** — confirm it responds to a test POST before writing any Flutter code
- [ ] `Message` Freezed: `role`(enum user|assistant), `content`, `timestamp`
- [ ] `Conversation` Freezed: `messages`, `contextPoiId?`, `contextPeriod?`
- [ ] `AssistantContext` Freezed: assembled from POI + timeline + profile providers
- [ ] `GptService` — calls proxy URL; handles errors and rate limits
- [ ] `AssistantNotifier extends AsyncNotifier<Conversation>` — `askQuestion`, `resetConversation`, `setContext`
- [ ] `AiChatPage` — route `/ai-chat`
- [ ] `ChatBubble` widget — user right/primary, assistant left/surface
- [ ] Wire "Perguntar ao Guia" button in `poi_info_sheet.dart` — opens chat with POI context pre-loaded
- [ ] Log `aiQuestionAsked` analytics event per question
- [ ] Unit tests: service (mock HTTP), notifier states, context assembly
- [ ] Widget tests: messages render, input sends on submit, loading state

#### 4.5 Phase 4 verification (all 5 test layers)

- [ ] `flutter test lib/ --reporter=compact` → all passing
- [ ] 100 POIs visible and positioned correctly
- [ ] Earthquake runs full 3-minute sequence on physical device (vibration, audio, particle effects)
- [ ] 360° interior views open on physical device
- [ ] AI assistant responds with correct POI context; proxy is the only path to OpenAI
- [ ] Analytics events log correctly to JSON file
- [ ] App size ≤ 100MB
- [ ] All strings PT / EN / ES
- [ ] All Phase 4 widgets have Semantics + Tooltips + ≥ 48px tap targets
- [ ] Layer 4 device tests for earthquake and AI chat

---

### Phase 5 — Excellence and release

**Goal**: 150 POIs. Full ES content translations. Performance. Final accessibility audit. Final store release. 1000+ thesis visitors. **Estimated: 3 months.**

> **Prerequisite**: Phase 4 complete, thesis data collection begun.

#### 5.1 Complete to 150 POIs

- [ ] Use `PROJECT_GUIDES/ar_poi_calibrator.html` for remaining 50 buildings
- [ ] PT + EN + ES descriptions for all 150
- [ ] Verify all survival statuses against historical sources

#### 5.2 Full Spanish content translation

By now, all UI strings already have `es: '...'` from Phases 0-4. This phase covers **long-form content**:

- [ ] ES descriptions for all 150 POIs in `pois.json`
- [ ] ES for circuit names, descriptions, narration text in `circuits.json`
- [ ] ES audio clip titles in `audio_clips.json`
- [ ] ES achievement names and descriptions
- [ ] Audit: run app in ES — confirm no string falls back to English inadvertently

#### 5.3 Accessibility final audit

By now, all widgets built in Phases 1-4 already have Semantics, Tooltips, and correct tap targets. This phase is the **audit and edge-case fix**:

- [ ] Run TalkBack (Android) on all main user flows — fix any gaps
- [ ] Run VoiceOver (iOS) on all main user flows — fix any gaps
- [ ] WCAG AA contrast recheck: 4.5:1 text, 3:1 large text — both light and dark themes
- [ ] Add audio transcripts for all 20 audio guide clips (expandable text below audio controls)
- [ ] Add `AppTheme.highContrast()` in `app_theme.dart`; expose toggle in `icons_group.dart`
- [ ] Focus traversal explicit on complex screens: panorama view, AI chat, earthquake

#### 5.4 Performance optimization

- [ ] `flutter build apk --analyze-size` — identify largest contributors
- [ ] Panorama mosaic: optimized JPEG (not PNG) — significant APK size difference
- [ ] Lazy-load POI descriptions — load full text only when sheet opens, not at app start
- [ ] Profile AR overlay with Flutter DevTools Timeline — verify POI marker build < 16ms
- [ ] Profile cold start on mid-range 2021 Android device — target < 3s
- [ ] Verify no duplicate decoded image copies in memory
- [ ] Test full offline mode — AR + POIs + circuits + gamification must work with no internet
- [ ] Final APK size < 100MB confirmed

#### 5.5 App store preparation (code artefacts from Phase 2 — complete store submission here)

> **Note:** The code artefacts for app store preparation (launcher icons, `AndroidManifest.xml` labels, `AppConfig` constants class, `web/index.html` SEO metadata, `web/manifest.json`) were created during Phase 2 Task 2.11. The actual store submission process is Phase 5 work because it requires external dependencies that are only ready at this stage: developer accounts, privacy policy URL approved by the ethics board, final screenshots, real `tilestories.app` domain, and public release timing aligned with thesis study.

- ✅ (from Phase 2) `AppConfig` abstract final class — version, applicationId, bundleId, store URLs, webDomain
- ✅ (from Phase 2) Launcher icons generated for all Android densities and web
- ✅ (from Phase 2) `AndroidManifest.xml` label, applicationId, versionCode/versionName set
- ✅ (from Phase 2) `web/index.html` canonical + hreflang (pt/en/es/x-default) + OG tags + Twitter Card
- ✅ (from Phase 2) `web/manifest.json` name, description, lang, orientation, brand colors
- [ ] Replace `TODO(Phase2)` canonical URL placeholder in `web/index.html` with real `tilestories.app` domain
- [ ] `flutter build apk --release` → verify APK < 100MB
- [ ] `flutter build appbundle --release` → for Google Play upload
- [ ] iOS: add `ios/` folder when iOS target initialised (`flutter create --platforms=ios .`)
- [ ] L1/L2/L3 test suite for AppConfig and AboutSection: already present (35 tests from Phase 2)

#### 5.6 Store release and thesis study

- [ ] Store descriptions PT, EN, ES
- [ ] App Store screenshots (6 per locale, 3 locales)
- [ ] Google Play screenshots + 30-second demo video
- [ ] TestFlight + Google Play Internal Testing — 10 beta testers
- [ ] Fix critical issues from feedback
- [ ] Public release
- [ ] 1000+ museum visitors with analytics consent → thesis data collection
- [ ] `analyticsService.exportSession()` — CSV/JSON for analysis
- [ ] Heatmap data: most-tapped POIs, circuit completion rates, session drop-off points

#### 5.7 Final cleanup

- [ ] Remove all `kDebugMode`-only routes from release nav
- [ ] Remove `poi-calibrator` from nav (keep file for future content updates)

#### 5.8 Phase 5 verification

- [ ] `flutter test lib/ --reporter=compact` → all passing
- [ ] Accessibility audit passed on both platforms
- [ ] APK < 100MB on both platforms
- [ ] Cold start < 3s on mid-range device
- [ ] All features in PT + EN + ES
- [ ] TalkBack + VoiceOver full flow tests

---

## 7. Ongoing rules (every phase, every session)

### Before touching any file

1. Read `PROJECT_STATUS.md`
2. Read the file(s) to modify
3. Check if the type/widget/function already exists — do not duplicate
4. Identify which domain the new code belongs to — place it there only
5. **Create a comprehensive TODO list upfront** — list every step before starting. Do not skip steps or improvise mid-task.
6. **For every decision, consider 3 options** at two levels:
   - Architecture level: where does this fit? What are the 3 cleanest approaches?
   - Implementation level: how to write it? What are the 3 simplest correct implementations?
   Choose the cleanest. Document the decision with an inline comment if it's non-obvious.

### Terminal rules

- **PowerShell**: use `;` not `&&` to chain commands
- **Avoid pipes and filters** in commands — let output flow naturally to the terminal
- After running `flutter analyze`: wait ~10 seconds before checking results (analysis is async on slower machines)

### Code quality (non-negotiable)

- One file, one responsibility. > ~300 lines → split it
- All user-visible strings: `t(pt: '...', en: '...', es: '...')` wrapped in `ref.tr()` — **always include `es`**
- All colors: `context.primary`, `context.error`, etc. — never `Colors.*`
- All spacing: `Spacing.*` — never literal numbers
- All radius: `RadiusTokens.*` — never literal numbers
- All durations: `AnimationTokens.*` — never literal milliseconds
- All pages: `LayoutManager` + `LayoutSlots` (or `LayoutPresets` for standard patterns)
- Providers: `Notifier`/`NotifierProvider`, `AsyncNotifier`/`AsyncNotifierProvider`, `FutureProvider`, or `Provider` — **`StateProvider` is banned**

**Freezed — use only when justified.** Freezed adds a generated file, a `part` directive, and build runner overhead. For a simple class with 2-3 plain fields and no `copyWith` need, write it by hand. Use Freezed when the class genuinely benefits from it:
- **Use Freezed for**: union/sealed types (`when`/`map`), classes that need `copyWith` with many fields, models serialized from JSON (`json_serializable`)
- **Write by hand**: enums with no extra fields, simple value holders with ≤ 3 fields, classes with straightforward getters/setters that are not serialized
- **Never** add Freezed speculatively "in case we need `copyWith` later" — add it when the need is concrete

### Performance rules

These apply from Phase 1 onwards. Not deferred to Phase 5.

**App size budget (< 100MB APK):**
- Panorama mosaic: JPEG, not PNG. A 2393×817 PNG vs JPEG can differ by 3-5MB
- Audio clips: MP3 128kbps, not WAV or FLAC
- 3D assets: check sizes before adding; each Unity scene adds significant megabytes
- Images: use `flutter build apk --analyze-size` regularly to catch regressions

**Loading speed (< 3s cold start on mid-range 2021 device):**
- Do not decode the full panorama image at app start — decode on demand when the AR view opens
- Lazy-load POI descriptions from JSON only when a sheet opens, not at startup
- Do not eagerly load audio clips — load on first play only
- Keep `main.dart` initialization to: theme, routing, consent check, first route. Nothing else

**AR overlay performance (< 16ms per frame / 60fps):**
- `ar_poi_marker.dart` must use `RepaintBoundary` and `const` constructors where possible — no rebuilds per frame
- POI marker list renders inside `Stack` — do not trigger full `Stack` rebuild per marker update
- Profile with Flutter DevTools Timeline before declaring the AR view "done"

**Package discipline (simple code > heavy dependency):**
- Before adding a package, ask: can this be done with Flutter SDK in < 50 lines?
- A package that solves 90% of the problem in 10 lines is better than a framework that solves 100% but adds 2MB and requires a wrapper
- Packages with large native components (Unity, ARCore dependencies) significantly increase APK size — check before adding
- Do NOT add Firebase, cloud analytics, or any always-on background service — local-only is the rule
- Never add a package just because a tutorial used it. Check pub points, maintenance, and transitive dependency count

**Web/offline performance:**
- The app must work fully offline (no internet). AR + POIs + circuits + gamification all load from local assets
- AI assistant requires internet — degrade gracefully when offline (hide the "Ask Guide" button, show "requires internet" tooltip)
- AR live mode requires camera permission — offline mode (`StaticImageFrameProvider`) must always be the fallback when camera permission is denied or AR is unavailable on the device. ARCore augmented image tracking runs entirely on-device; no internet connection is required for image detection or tracking.

### Accessibility (every widget, from day 1)

Before a widget is considered done:

- [ ] Every interactive element has `Semantics(label: ref.tr(...))` or `Semantics(button: true, label: ...)`
- [ ] Every `IconButton` has a `Tooltip`
- [ ] All tap targets ≥ 48×48px (`SizeTokens.tapTarget`)
- [ ] Color is never the sole indicator of meaning — pair with icon or text
- [ ] On screens with > 5 interactive elements: explicit focus traversal order

These rules are checked per-widget as you write, not once per phase.

### After every task

- `flutter analyze` → 0 issues
- `flutter test` → all passing
- If Freezed file modified: `dart run build_runner build --delete-conflicting-outputs`

### File naming

- Files: `snake_case.dart`
- Classes: `PascalCase`
- Providers: `descriptiveNameProvider`
- Notifiers: `DescriptiveNameNotifier`
- Pages: `feature_name_page.dart`
- Widgets: `descriptive_name.dart`
- Barrels: `domain_name_domain.dart`

### Domain structure (every domain, no exceptions)

```
lib/domains/<name>/
├── models/        # data classes — Freezed for unions/JSON/copyWith; plain Dart for simple types
├── repositories/  # data access only (assets, network, local storage)
├── providers/     # one provider per file
├── pages/         # one file per route/screen
├── widgets/       # domain-specific UI only
├── services/      # external service calls (HTTP, platform channels)
├── test/
│   ├── unit/        # Layer 1
│   ├── widgets/     # Layer 2
│   └── integration/ # Layer 3
└── <name>_domain.dart  # barrel: public API of this domain
```

Do not put providers in pages. Do not put business logic in widgets. Do not put UI in repositories.

### Adding a new route

1. Add `NavItem` to `lib/navigation/navConfig/nav_config.dart`
2. Create the page in `lib/domains/<domain>/pages/`
3. `createRouter()` picks it up — no other file changes needed

### Adding a new package

1. Check `pubspec.yaml` first
2. Check if Flutter SDK provides it (zero-dep always preferred)
3. pub.dev: > 100 pub points, updated within 12 months, null-safe
4. Add with `^version` constraint + explanatory comment
5. `flutter pub get`

### Comments in code

Write comments for non-obvious design decisions and complex math. Not for what the code does — for why. Concise. No emojis.

```dart
// BoxFit.contain math: compute the actual rendered image rect within the
// viewport accounting for aspect ratio padding. Markers must align to image
// content, not the bounding box — hence the explicit offset calculation.
final Rect imageRect = _computeContainedImageRect(viewportSize, imageSize);
```

---

## 8. Package decisions

| Need                    | Package                                        | Rationale                                                                                                                                                                                                                                                                                                                                                                                           |
| ----------------------- | ---------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| State management        | `flutter_riverpod ^3.2.1`                      | In stack. Do not change.                                                                                                                                                                                                                                                                                                                                                                            |
| Routing                 | `go_router ^17.1.0`                            | In stack. Do not change.                                                                                                                                                                                                                                                                                                                                                                            |
| Code gen                | `freezed ^3.2.5` + `json_serializable ^6.12.0` | In stack. Do not change.                                                                                                                                                                                                                                                                                                                                                                            |
| AR                      | `ar_flutter_plugin_plus ^1.1.3`                | In stack. Do not reference the deprecated `flutterflow` fork.                                                                                                                                                                                                                                                                                                                                       |
| Image caching           | `cached_network_image ^3.3.0`                  | Currently no remote images — panorama and all POI assets ship in the bundle. Keep as a dependency only if a CMS or remote image source is added. Remove from pubspec if still unused by Phase 2 release.                                                                                                                                                                                            |
| Permissions             | `permission_handler ^12.0.1`                   | In stack.                                                                                                                                                                                                                                                                                                                                                                                           |
| Sharing                 | `share_plus`                                   | Add Phase 1. Native share sheet. Official Flutter package.                                                                                                                                                                                                                                                                                                                                          |
| Audio                   | `just_audio ^0.9.x`                            | Add Phase 3. Background audio, audio focus, stable platform channels. Better than `audioplayers`.                                                                                                                                                                                                                                                                                                   |
| App icons               | `flutter_launcher_icons` (dev)                 | Add Phase 2. Generates all icon sizes correctly.                                                                                                                                                                                                                                                                                                                                                    |
| Unity bridge            | `flutter_unity_widget`                         | Add Phase 4b ONLY when Unity project is ready. Never add speculatively.                                                                                                                                                                                                                                                                                                                             |
| HTTP                    | `http`                                         | Add Phase 4 for AI assistant. Never use a dedicated OpenAI package — they break on API updates.                                                                                                                                                                                                                                                                                                     |
| Interactive panorama    | `InteractiveViewer` (Flutter SDK)              | Already used. `photo_view` removed in Phase 0.                                                                                                                                                                                                                                                                                                                                                      |
| Local analytics storage | `dart:io` (built-in)                           | JSON-append file. Start here — no extra package. **Open question:** at thesis scale (1000+ visitors × ~200 events/session = 200k+ events), JSON-append files cannot be queried without loading the full file into memory. If live analytics during the study period is needed (heatmaps, drop-off rates), migrate to `sqflite`. Decide at the start of Phase 2 once study methodology is confirmed. |
| Cloud analytics         | —                                              | Do NOT add Firebase Analytics or any cloud SDK. Local-only for privacy + thesis ethics.                                                                                                                                                                                                                                                                                                             |

---

## 9. What to ignore from older documents

| Document                                                        | What to ignore                                                                                                                              |
| --------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------- |
| `AR_notes.md`                                                   | Superseded. Valid AR status is in `PROJECT_STATUS.md`.                                                                                      |
| `AR_IMPLEMENTATION_PLAN.md` / `AR_IMPLEMENTATION_PLAN_CLEAN.md` | Both superseded by this guide.                                                                                                              |
| `FUTURE_ARCHITECTURE.md`                                        | Early aspirational planning. Use §3 of this guide as the target structure.                                                                  |
| `proj_notes`                                                    | Replaced by this guide.                                                                                                                     |
| `DOMAIN_FEATURES_GUIDE/comando_initial.txt`                     | `@index-model` / `@index-provider` annotation tags — do not annotate source files with these. `PROJECT_STATUS.md` handles indexing cleanly. |
| References to `ar_flutter_plugin_flutterflow`                   | Deprecated. Use `ar_flutter_plugin_plus` only.                                                                                              |
| `StateProvider` or `StateNotifierProvider` in any doc           | Banned. Use `NotifierProvider` / `AsyncNotifierProvider`. See §7 Code quality rules.                                                        |
| `photo_view` references                                         | Removed in Phase 0.                                                                                                                         |
| `poi_calibrator_page.dart` in `lib/ar_core/utils/`              | Deleted in Phase 0. Calibration done with `PROJECT_GUIDES/ar_poi_calibrator.html` — open in browser, no Flutter required.                   |
| `ar_poi_calibrator_EXAMPLE.html` in `lib/ar_core/utils/`        | Actual filename is `ar_poi_calibrator.html` (no `_EXAMPLE` suffix). Moved to `PROJECT_GUIDES/ar_poi_calibrator.html` in Phase 0.            |
| "Spanish in Phase 3/4" in any older doc                         | Wrong. ES field added Phase 0, strings filled incrementally, long-form content Phase 5.                                                     |
| "Accessibility in Phase 4" in any older doc                     | Wrong. Accessibility is built from Phase 0 onwards. Phase 5 is audit only.                                                                  |

---

*End of PROJECT_GUIDE.md — update at the start of each new phase to reflect completed work and architectural decisions made during implementation.*

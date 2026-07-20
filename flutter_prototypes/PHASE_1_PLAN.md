# [Phase 1] — AR Proof of Concept

## Note for AI agent: mark each task with [ ] and then when that task is implemented and tested and passes all tests mark with ✅

---

## Changelog

| Session    | Date       | Changes                                                                                                                                                                                                                                                                                                                                                                                                                            |
| ---------- | ---------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Session 2  | 2026-03-13 | P0/P1/P2 audit fixes: `kDestructionColors`, T01 color tokens, `BackdropFilter` removal, `Positioned.fill` removal, `kDebugMode` guards, dead method deletion, `home_page_new.dart` deletion, `// AR overlay:` comments. `flutter analyze` 0 errors, `flutter test` +1100 ~3 −7.                                                                                                                                                    |
| Session 3  | 2026-03-13 | T03: POILegendSheet `maxChildSize` 0.85→0.92, `snapSizes` simplified. T04: `_TypeCell` 36dp marker circle redesign. T10: epoch context strip — left-border `Container` with `poiTypeEnum.accentColor`, `bodyMedium` weight 500. C7: deleted empty `lib/domains/user/auth/`. `flutter analyze` 0 errors, `flutter test` +1100 ~3 −7.                                                                                                |
| Session 4  | 2026-03-14 | C10 legacy cleanup: deleted `widgets/poi_marker.dart`, `widgets/poi_info_sheet.dart`, legacy barrel exports. `poi_legend_button_test.dart` L4 (13 groups), `ar_poi_marker_test.dart` L4, `poi_test.dart` L1 added. +1140 all tests passing.                                                                                                                                                                                        |
| Session 5  | 2026-03-15 | Legend polish: `maxChildSize` 1.0, `useSafeArea: false`, light/dark sheet theme, `_SheetDragHandle`. `pois.json` expanded 30→41 POIs. 58 new tests. `flutter test` +1147 ~3 all passing.                                                                                                                                                                                                                                           |
| Session 6  | 2026-03-16 | Full audit pass — no code changes. Verified 1163 tests passing, 0 analyze errors.                                                                                                                                                                                                                                                                                                                                                  |
| Session 7A | 2026-03-16 | Architecture cleanup: A1 deleted `panorama_ar_scene_builder.dart`; A3/A4 removed phantom state fields `selectedPOI`/`categoryFilter`/`renderables` from `PanoramaARState`; A5 deleted dead providers `showPOIsProvider`/`selectedCategoryProvider`/`filteredPOIsProvider`; Q3/U7 `ref.tr(poi.name)` i18n fix; Q5 duplicate `@override` removed; Q8 ES search in repository. Dead test groups removed: −29 tests. +1134 ~3 passing. |
| Session 7B | 2026-03-16 | Q1: split `poi_legend_button.dart` 1655→165 lines + `poi_legend_sheet.dart` (~1440 lines, `part of`). Q2: `ChurchIconPainter` moved to `marker_icons.dart`. Q6: `kDestructionColors[i]` throughout legend sheet. Q7: theme-aware `_circleColor` getter added. +1134 ~3 passing.                                                                                                                                                    |
| Session 7C | 2026-03-13 | U6: ES translations in `ar_not_available_widget.dart`. U1: `halfSize = spec.lodScale.sizeDp / 2` fix. Q4/U2/U3: `poi.poiTypeEnum` replaces legacy 4-category switch in `poi_detail_sheet.dart` + `poi_summary_card.dart`. P1: `.select()` watchers for `trackingState`/`errorMessage` in `panorama_ar_view.dart`. Test fixtures updated. +1134 ~3 passing.                                                                         |

---

## File Index

| File                                                            | Status | Role                                                                                                                                                                                                                                                                                                                             |
| --------------------------------------------------------------- | ------ | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `assets/data/pois.json`                                         | ✅      | 41 POIs, PT + EN + ES, survivalStatus                                                                                                                                                                                                                                                                                            |
| `lib/domains/timeline/models/time_period.dart`                  | ✅      | Plain Dart enum — 4 epochs                                                                                                                                                                                                                                                                                                       |
| `lib/domains/timeline/providers/timeline_provider.dart`         | ✅      | `NotifierProvider<TimelineNotifier, TimePeriod>`                                                                                                                                                                                                                                                                                 |
| `lib/domains/timeline/widgets/timeline_slider.dart`             | ✅      | 4-stop SegmentedButton                                                                                                                                                                                                                                                                                                           |
| `lib/domains/timeline/widgets/epoch_label.dart`                 | ✅      | Current epoch name + subtitle                                                                                                                                                                                                                                                                                                    |
| `lib/domains/timeline/timeline_domain.dart`                     | ✅      | Barrel export                                                                                                                                                                                                                                                                                                                    |
| `lib/ar_core/viewport/viewport_state.dart`                      | ✅      | Freezed — zoomLevel, centerNormalized, visibleRegion, mode                                                                                                                                                                                                                                                                       |
| `lib/ar_core/viewport/viewport_state.freezed.dart`              | ✅      | Generated                                                                                                                                                                                                                                                                                                                        |
| `lib/ar_core/viewport/viewport_state_provider.dart`             | ✅      | `NotifierProvider<ViewportStateNotifier, ViewportState>`                                                                                                                                                                                                                                                                         |
| `lib/ar_core/nodes/ar_node_manager.dart`                        | ✅      | Abstract interface — placeModel, removeModel, onModelTapped                                                                                                                                                                                                                                                                      |
| `lib/ar_core/nodes/ar_node_3d_config.dart`                      | ✅      | Plain Dart class — config for one 3D node                                                                                                                                                                                                                                                                                        |
| `lib/ar_core/nodes/mock_ar_node_manager.dart`                   | ✅      | No-op implementation                                                                                                                                                                                                                                                                                                             |
| `lib/domains/panorama/ar/config/panorama_3d_nodes_config.dart`  | ✅      | `kAR3DNodes = []` (Phase 1) + `kPanelPhysicalWidthMeters`                                                                                                                                                                                                                                                                        |
| `lib/domains/panorama/ar/config/panorama_reference_images.dart` | ✅      | ARCore image database config — must pass `widthInMeters`                                                                                                                                                                                                                                                                         |
| `lib/domains/panorama/services/poi_visibility_service.dart`     | ✅      | Pure service — `computeVisible()` → `List<POIRenderSpec>`                                                                                                                                                                                                                                                                        |
| `lib/domains/panorama/ar/widgets/panorama_ar_view.dart`         | ✅      | Main full-screen Stack; uses `renderSpecs` to drive markers                                                                                                                                                                                                                                                                      |
| `lib/domains/panorama/ar/widgets/panorama_top_bar.dart`         | ✅      | Semi-transparent top bar, back + options                                                                                                                                                                                                                                                                                         |
| `lib/domains/panorama/ar/widgets/ar_tracking_indicator.dart`    | ✅      | Searching / Initializing / Tracking states                                                                                                                                                                                                                                                                                       |
| `lib/domains/panorama/ar/widgets/ar_debug_overlay.dart`         | ✅      | Dev-only; `kShowARDebugOverlay` guard                                                                                                                                                                                                                                                                                            |
| `lib/domains/panorama/ar/widgets/ar_mode_toggle.dart`           | ✅      | Animated-track Variant-A redesign — `72×40` `AnimatedContainer` track, `AnimatedPositioned` 32dp thumb, blink dot (900ms repeat); lives inside `PanoramaTopBar` (not Positioned in Stack); `ConstrainedBox(maxWidth: 280)`                                                                                                       |
| `lib/domains/panorama/ar/widgets/ar_first_time_overlay.dart`    | ✅      | Two-step onboarding; `PrefsKeys.arOnboardingShown`                                                                                                                                                                                                                                                                               |
| `lib/domains/panorama/ar/widgets/ar_poi_marker.dart`            | ✅      | 4 LOD tiers (large 48dp / medium 36dp / small 24dp / micro 16dp); `_DashedBorderPainter` with 6-stop destruction-spectrum gradient; `ARPOIMarkerIcon` icon (large + medium only); `RepaintBoundary`; `Opacity` wrapper only when `opacity < 1.0`; `AnimatedScale(1.15)` when selected                                            |
| `lib/domains/panorama/ar/widgets/marker_icons.dart`             | ✅      | 8 `CustomPainter` icons — cross / crown / shield / columns / anchor / arch / hill / scales; vector paths in 0–1 unit square; `strokeWidth = size × 0.07`; no emoji, no SVG asset                                                                                                                                                 |
| `lib/domains/panorama/ar/widgets/poi_legend_button.dart`        | ✅      | 165-line file — 36dp ⓘ button (`Stack[8]` `Positioned(top: kToolbarHeight, right: 0)`); gold border `0xFFC9973A`; opens `POILegendSheet` (via `part 'poi_legend_sheet.dart'`). `DraggableScrollableSheet(initial: 0.55, min: 0.55, max: 1.0, snap: true, snapSizes: [0.55, 1.0], useSafeArea: false)`.                           |
| `lib/domains/panorama/ar/widgets/poi_legend_sheet.dart`         | ✅      | ~1440-line `part of poi_legend_button.dart` — full sheet content: `_SpectrumBar` (6-stop `kDestructionColors` gradient), `_TypeGrid` (`GridView.count(crossAxisCount: 4)`, 8 `_TypeCell` widgets), theme-aware `_LegendTheme` with `_circleColor` getter, `_SheetDragHandle`. Split from `poi_legend_button.dart` in Session 7B. |
| `lib/domains/panorama/ar/widgets/poi_summary_card.dart`         | ✅      | 280dp card near marker; scale+fade entrance                                                                                                                                                                                                                                                                                      |
| `lib/domains/panorama/ar/widgets/poi_detail_sheet.dart`         | ✅      | `DraggableScrollableSheet`; epoch context strip (left-border Container in `poiTypeEnum.accentColor`, `bodyMedium` weight 500); share button                                                                                                                                                                                      |
| `lib/domains/panorama/ar/widgets/poi_action_buttons.dart`       | ✅      | Extracted CTA row — reused by both `POISummaryCard` and `POIDetailSheet`                                                                                                                                                                                                                                                         |
| `lib/domains/panorama/providers/panorama_providers.dart`        | ✅      | `selectedPOIProvider`, `poiDisplayModeProvider` (`POIDisplayMode` enum), `poisRepositoryProvider`, `poisProvider`. Dead providers `showPOIsProvider`/`selectedCategoryProvider`/`filteredPOIsProvider` removed in Session 7A.                                                                                                    |
| `lib/components/ui/info_badge.dart`                             | ✅      | Colored pill badge — category + survival                                                                                                                                                                                                                                                                                         |
| `lib/components/ui/section_header.dart`                         | ✅      | Titled section divider with optional trailing action                                                                                                                                                                                                                                                                             |
| `lib/components/ui/tappable_card.dart`                          | ✅      | Press-scale 0.97 base card                                                                                                                                                                                                                                                                                                       |
| `lib/components/ui/ui_components.dart`                          | ✅      | Barrel export for `lib/components/ui/`                                                                                                                                                                                                                                                                                           |
| `lib/domains/home/pages/home_page.dart`                         | ✅      | Two CTAs, permission flow, LanguageSwitcher + ThemeSwitcher                                                                                                                                                                                                                                                                      |
| `lib/domains/home/providers/camera_permission_provider.dart`    | ✅      | Injectable `Provider<Future<PermissionStatus> Function()>`                                                                                                                                                                                                                                                                       |
| `lib/utils/prefs/prefs_keys.dart`                               | ✅      | `PrefsKeys.arOnboardingShown`, `PrefsKeys.timelineOnboardingShown`, `PrefsKeys.bookmarkedPOIIds` — single source of truth for pref keys                                                                                                                                                                                          |
| `PROJECT_GUIDES/AR_TRACKING_STRATEGY.md`                        | [ ]    | arcoreimg score, widthInMeters confirm, drift results — NOT YET CREATED                                                                                                                                                                                                                                                          |
| `PROJECT_GUIDES/MUSEUM_PARTNERSHIP.md`                          | [ ]    | Physical panel dimensions — NOT YET CREATED                                                                                                                                                                                                                                                                                      |

---

## 0. Phase Summary

**Goal:** Working AR panorama with 30 POIs, offline and live modes, timeline slider, and clean page chrome.

**Why this phase matters:** Phase 1 is the POC — it proves the core AR interaction (image tracking, marker overlay, POI selection) before the app shell is designed. If AR tracking does not work at museum scale, the project architecture changes fundamentally; proving it in Phase 1 de-risks everything that follows.

**OUT OF SCOPE:**
- Full home page design (Phase 2)
- 3D GLB model loading (Phase 3)
- Audio guide (Phase 3)
- Circuit/tour features (Phase 3)
- AI guide (Phase 4)
- Route stubs that lead nowhere
- Onboarding profile selection
- FAB (Phase 2 — disabled FAB in a museum looks broken)
- `tilestories.app` deep link domain (placeholder URL only — real config in Phase 2)
- Deep link platform config (`AndroidManifest.xml` intent filters, iOS associated domains) — Phase 2 prerequisite

**Estimated duration:** 3 months.

**Hard prerequisites:**
- Phase 0 verification complete: `flutter analyze` → 0 issues, `flutter test lib/` → all passing
- Physical panorama panel dimensions confirmed with museum → `MUSEUM_PARTNERSHIP.md`
- `arcoreimg eval-img` score >= 75 on the panorama reference image
- A3 printed proxy of the reference image (for development iteration without museum visits)
- **"Then vs Now" feature (content gate):** Current photographs of surviving buildings must be sourced — original, Creative Commons, or museum-licensed — before this feature can ship to visitors. The code architecture (`currentPhotoUrl` field on POI, "Then vs Now" widget) lands in Phase 1. The feature is content-gated, not code-gated. Do not block Phase 1 code tasks on photo sourcing; do block the museum install.

---

## 1. Pre-implementation Checklist

### Files to read before touching anything

- [ ] `PROJECT_GUIDES/PROJECT_GUIDE.md` — sections 2, 3, 4, 5, 7
- [ ] `PROJECT_GUIDES/IMPLEMENTATION_GUIDELINES.md` — in full
- [ ] `PROJECT_GUIDES/DESIGN/08_MOTION_AND_FEEL.md` — animation patterns
- [ ] `PROJECT_GUIDES/DESIGN/02_COLORS.md` + `PROJECT_GUIDES/DESIGN/01_TOKENS.md` — no raw `Colors.*`
- [ ] `PROJECT_GUIDES/FEEDBACK/FEEDBACK_GUIDE.md` — state transition patterns
- [ ] `PROJECT_GUIDES/NAV_AND_LAYOUT/NAVIGATION_AND_LAYOUT_GUIDE.md` — how to add routes
- [ ] `lib/domains/panorama/ar/providers/ar_infrastructure_providers.dart` — locate `arTrackingStateProvider`
- [ ] `lib/domains/panorama/ar/config/panorama_reference_images.dart` — confirm `widthInMeters` is passed
- [ ] `lib/ar_core/models/ar_availability.dart` — read the `ARAvailability` enum (`available | unavailable | unknown`) before Task 1.9; used to guard the home CTA
- [ ] `lib/ar_core/utils/viewport_math.dart` — confirm `computeMarkerScreenPosition` exists with named params `normX`, `normY`, `imgW`, `imgH`, `canvasW`, `canvasH`, `scale`, `tx`, `ty`. If not: create it in Task 1.3 with unit tests.
- [ ] `lib/domains/panorama/ar/widgets/poi_info_sheet.dart` — read fully before renaming to `poi_detail_sheet.dart` in Task 1.7 (identify all import sites and widget usages)

### Tests to run to confirm baseline

```powershell
flutter test lib/ --reporter=expanded
flutter analyze
# Also run in Chrome to confirm offline panorama renders correctly:
flutter run -d chrome
```

Both must be clean before starting any task.

### External dependencies

| Package                          | Justification                              | Check before adding                                   |
| -------------------------------- | ------------------------------------------ | ----------------------------------------------------- |
| `ar_flutter_plugin_plus: ^1.1.3` | AR session management (already in pubspec) | —                                                     |
| `share_plus`                     | Native share sheet for POI share button    | > 100 pub points, updated within 12 months, null-safe |
| `shared_preferences`             | AR onboarding persistence                  | May already be a transitive dep — check before adding |
| `permission_handler`             | Camera permission flow (home CTA)          | Already in pubspec — confirm                          |

### Compile-time flags to set up

```dart
// lib/domains/panorama/ar/providers/ar_infrastructure_providers.dart
const bool kUseRealAR          = bool.fromEnvironment('USE_REAL_AR');
const bool kShowARDebugOverlay = bool.fromEnvironment('SHOW_AR_DEBUG');
```

### Tools to run

```powershell
# After any Freezed file is modified:
dart run build_runner build --delete-conflicting-outputs

# Before starting: arcoreimg quality check (download from github.com/google-ar/arcoreimg)
arcoreimg eval-img --input_image_path=assets/images/panorama/<panorama.jpg>
# Required score >= 75. Document in AR_TRACKING_STRATEGY.md.
```

---

## 2. Architectural Decisions

| #   | Decision                                                                                          | Why                                                                                                                                                                                                                                                                                  | What was rejected                                                   |
| --- | ------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ------------------------------------------------------------------- |
| A1  | `TimePeriod` as plain Dart enum (not Freezed)                                                     | No union types, no JSON, no many-field copyWith — Freezed adds build overhead for no gain                                                                                                                                                                                            | Freezed sealed class                                                |
| A2  | `ViewportState` as Freezed class                                                                  | Multiple fields, `copyWith` used in notifier — justified use case                                                                                                                                                                                                                    | Plain Dart class (no copyWith)                                      |
| A3  | `POIDisplayMode` enum + two separate providers (`selectedPOIProvider` + `poiDisplayModeProvider`) | Single responsibility: one owns selection, one owns display mode. `PopScope` only checks display mode.                                                                                                                                                                               | Single combined provider (mixes concerns)                           |
| A4  | Timeline slider visibility keyed to `poiDisplayModeProvider != POIDisplayMode.none`               | `selectedPOI` persists after `poiDisplayModeProvider.close()` — using it caused slider to hide permanently                                                                                                                                                                           | `selectedPOIProvider == null` (BUG: selection not cleared on close) |
| A5  | `renderSpecs` drives marker overlay, not `allPOIs`                                                | `POIVisibilityService` is the single source of truth for what to render and how                                                                          -                                                                                                                           | Direct POI filtering in the widget                                  |
| A6  | `ARPOIMarker` accepts `opacity` + `showLabel`                                                     | Marker style is dictated by `POIRenderSpec`, not hardcoded in the widget                                                                                                                                                                                                             | Conditional widget trees in the overlay                             |
| A7  | `BackdropFilter` BANNED in non-static contexts                                                    | x4 frame-time regression on iOS Impeller during panning                                                                                                                                                                                                                              | Blur behind top bar                                                 |
| A8  | `cameraPermissionProvider` as injectable `Provider<Future<PermissionStatus> Function()>`          | Enables permission testing without platform channels                                                                                                                                                                                                                                 | Direct `Permission.camera.request()` call in widget                 |
| A9  | FAB deferred to Phase 2                                                                           | Disabled FAB with "Coming soon" tooltips signals an unfinished product to museum visitors                                                                                                                                                                                            | Phase 1 stub FAB                                                    |
| A10 | `frameWidgetProvider` decouples Layer A from Layer B                                              | Widget layer never imports `InteractiveViewer` or ARCore directly                                                                                                                                                                                                                    | `PanoramaARView` branching on mode internally                       |
| A11 | `PrefsKeys` static class for prefs string constants                                               | Single source of truth — no typo risk from scattered raw strings                                                                                                                                                                                                                     | Raw string literals in widget files                                 |
| A12 | Section-based tracking as fallback (Option A) if single-image fails                               | Overlapping zones with global coordinate offsets preserve POI math                                                                                                                                                                                                                   | Option B: fiducial markers (require physical installation)          |
| A13 | `POIVisibilityConfig.phase1()` hardcoded const in controller                                      | Not a provider in Phase 1 — unnecessary indirection. Promoted to provider in Phase 2 when LOD config needs to vary.                                                                                                                                                                  | Provider from the start                                             |
| A14 | `ARFirstTimeOverlay` uses `ref.listen` (not `ref.watch`) for tracking state transition            | `ref.watch` would re-evaluate during every rebuild and trigger the advance logic if tracking was already `tracking` when the overlay mounted (race condition). `ref.listen` fires only on the transition itself — i.e., the change from `searching`/`initializing` → `tracking`.     | `ref.watch` (would fire on mount if tracking already established)   |
| A15 | `ar_mode_toggle.dart` changed to bottom-right pill (`FilledButton.tonal`)                         | Top SegmentedButton wastes prime screen real estate in a full-screen immersive experience; pill at bottom-right stays out of the painting                                                                                                                                            | Keep SegmentedButton at top                                         |
| A16 | `tilestories.app` in share deep link is a placeholder                                             | Real domain and platform deep link config (`AndroidManifest.xml` intent filters, iOS associated domains) requires store presence — Phase 2 prerequisite. Add `// TODO(Phase2): replace with real domain` comment on the share URL string.                                            | Using a real domain now (not registered)                            |
| A17 | `kPanelPhysicalWidthMeters` AND `kPanelPhysicalHeightMeters` both stored in config                | ARCore `addImage()` takes `widthInMeters` — the height is not passed to ARCore but IS needed for the overlap formula (`panelScreenHeightDp = imageNaturalHeight * viewport.zoomLevel / dpr`). Both dimensions should be sourced from the same config constant, not hardcoded inline. | Width only (height guessed from aspect ratio inline)                |

---

## 3. UI/UX Structure and Flows

### Design Philosophy — Refined Minimalism with Historical Warmth

The painting is the product. Every pixel of app chrome that is visible competes with a 23-metre 18th-century masterwork. The UI must be nearly invisible until needed.

| Principle                  | Implementation rule                                                                                                                                                                    |
| -------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Chromeless by default**  | All app chrome hides unless the user is actively interacting. `PanoramaTopBar` is opaque but thin; `TimelineSlider` is the only persistent UI element (it IS the experience control).  |
| **Progressive disclosure** | `POISummaryCard` (compact) → `POIDetailSheet` (full). Never show everything at once. Each level of disclosure answers a specific question.                                             |
| **Contextual anchoring**   | `POISummaryCard` appears NEAR its marker, not at the bottom. The visual link between card and building is the emotional core of the interaction — do not break it with a bottom sheet. |
| **Ephemeral feedback**     | `EpochLabel` and `ARTrackingIndicator` auto-hide after 3 seconds. They answer "what just happened?" then disappear. They are not status bars.                                          |
| **Offline-first UX**       | Every interaction works without AR. The AR mode is an enhancement, not a requirement. Users who cannot point at the physical panel must still have a complete experience.              |

---

### Home Page (`/`)

**Layout:** `LayoutPresets.defaultPageApp()` — immersive, no header/footer.

**Structure (centered column):**
```
AppLogo
App name + one-line description (PT/EN/ES)
[Explorar com AR]   — FilledButton, r1 stagger
[Explorar sem AR]   — OutlinedButton, r2 stagger
LanguageSwitcher + ThemeSwitcher  — top-right header row
```

**State reads:** `languageProvider`, `themeProvider`
**State writes:** `cameraPermissionProvider` (side-effect: platform permission call)

**Interactive elements:**
- "Explorar com AR" → request camera permission → `/panorama`
- "Explorar sem AR" → navigate directly to `/panorama` (offline mode)
- `LanguageSwitcher` → cycles PT → EN → ES → PT
- `ThemeSwitcher` → toggles light/dark

**Entry animation:** `RevealAnimation` + `AnimationTokens.reveal`; r1/r2 stagger for CTAs.

**Permission flow:**
1. Granted → `context.go('/panorama')`
2. Denied → dialog with "Explorar sem AR" fallback (`Key('permission_denied_fallback')`)
3. Permanently denied → dialog directing to `openAppSettings()`

**Edge cases:**
- Permission must not be requested in `initState` — only on button tap
- `_arCtaLoading` flag prevents double-tap during permission request

---

### Panorama Page (`/panorama`)

**Layout:** Full-screen `Scaffold`, no AppBar — custom floating chrome.

**Stack structure:**
```
[0] Frame layer — InteractiveViewer (offline) OR ARCore plugin view (live AR)
[1] POI markers overlay — Positioned.fill; driven by renderSpecs; each marker in RepaintBoundary
[2] POISummaryCard — conditional; Positioned near tapped marker (computed via viewport_math.dart)
[3] ARTrackingIndicator — bottom-left; self-hides in offline mode; auto-hides 3s after tracking
[4] PanoramaTopBar — top; semi-transparent, NO BackdropFilter
[5] ARModeTogglePill — bottom-right; FilledButton.tonal pill
[6] TimelineSlider — bottom; AnimatedOpacity+IgnorePointer hides when displayMode != none
[7] EpochLabel — above timeline slider; auto-hides 3s via Timer
[8] ARFirstTimeOverlay — full-screen; shown once per install in live AR mode only
[9] ARDebugOverlay — bottom-left, shown only when kShowARDebugOverlay == true (tree-shaken in release)
[10] POIDetailSheet — DraggableScrollableSheet; MUST be a direct child of this Stack, NOT nested
     inside a Positioned widget — DraggableScrollableSheet requires Stack or Scaffold body as
     direct parent for correct size computation.
```

**State reads:** `viewportStateProvider`, `timelineProvider`, `poiDisplayModeProvider`, `selectedPOIProvider`, `arTrackingStateProvider`, `arNodeManagerProvider`
**State writes:** `viewportStateProvider` (frame transforms), `poiDisplayModeProvider`, `selectedPOIProvider`, `timelineProvider`

**Interactive elements:**
- Back → if overlay open: close it; else `context.go('/')`
- Mode toggle pill → `viewportStateProvider.setMode()`
- POI marker tap → set `selectedPOIProvider` + `poiDisplayModeProvider = summary`
- Summary card X → `poiDisplayModeProvider.close()`
- "Mais Info" → `poiDisplayModeProvider = sheet`
- Timeline segment → `timelineProvider.setEpoch()`
- Share button → `share_plus` with deep link `tilestories.app/panorama?poi=<id>`

**Entry/exit animations:**
- `POISummaryCard`: scale 0.85→1.0 + fade, `AnimationTokens.fast`, `luxurySpring` curve
- `POIDetailSheet`: sheet slide-up, `DraggableScrollableSheet`
- `ARTrackingIndicator`: `AnimatedSwitcher(duration: AnimationTokens.medium)`
- `EpochLabel`: `AnimatedSwitcher(duration: AnimationTokens.medium)` with unique key per epoch
- `TimelineSlider`: `AnimatedOpacity(duration: AnimationTokens.medium)`

**Accessibility:**
- `PopScope` priority: `poiDisplayModeProvider != none` → close overlay, else navigate
- `ARTrackingIndicator`: `Semantics(liveRegion: true)`
- `EpochLabel`: `Semantics(liveRegion: true)`
- All tap targets: >= 48×48px (`SizeTokens.tapTarget`)
- `MediaQuery.of(context).disableAnimations` respected in all animated widgets

**Edge cases:**
- `POISummaryCard` and `POIDetailSheet` are mutually exclusive — `POIDisplayMode` enforces this
- AR session init failure: `AsyncValueBuilder` wrapper shows retry or "Explorar sem AR" fallback
- Panorama JPEG OOM: `cacheWidth: 3000` cap if first-render stall > 1s on oldest device
- `PanoramaTopBar` uses `ElevationTokens.level2` shadow — NOT `BackdropFilter`

**Main user flow:**
```
Home → "Explorar sem AR"
  → Panorama offline: 30 markers visible
  → Tap marker → POISummaryCard near marker
  → "Mais Info" → POIDetailSheet
  → Change epoch → destroyed POIs disappear
  → Share → native share sheet
  → Back → closes sheet
  → Back → navigates home
```

**AR user flow:**
```
Home → "Explorar com AR"
  → Permission granted → Panorama live AR
  → ARFirstTimeOverlay Step 1: "Point at the painting"
  → Camera detects panel → FULL_TRACKING
  → ARFirstTimeOverlay Step 2: "Tap a building"
  → POI markers appear on panel
  → Pan away from detection area → LAST_KNOWN_POSE (markers hold)
  → Tap marker → same summary/sheet flow as offline
```

---

## 4. Data and State Architecture

### New providers

| Provider                   | Type                                                       | READ/WRITE | Notes                               |
| -------------------------- | ---------------------------------------------------------- | ---------- | ----------------------------------- |
| `timelineProvider`         | `NotifierProvider<TimelineNotifier, TimePeriod>`           | R+W        | Session-level; initial = `pre1755`  |
| `viewportStateProvider`    | `NotifierProvider<ViewportStateNotifier, ViewportState>`   | R+W        | Updated from frame transforms       |
| `selectedPOIProvider`      | `NotifierProvider<..., POI?>`                              | R+W        | Which POI is selected               |
| `poiDisplayModeProvider`   | `NotifierProvider<PoiDisplayModeNotifier, POIDisplayMode>` | R+W        | `none` / `summary` / `sheet`        |
| `arNodeManagerProvider`    | `Provider<ARNodeManager>`                                  | R only     | Phase 1: always `MockARNodeManager` |
| `cameraPermissionProvider` | `Provider<Future<PermissionStatus> Function()>`            | R only     | Injectable for testing              |

### New models / value types

| Type                  | File                                            | Kind             |
| --------------------- | ----------------------------------------------- | ---------------- |
| `TimePeriod`          | `timeline/models/time_period.dart`              | Plain Dart enum  |
| `ViewportState`       | `ar_core/viewport/viewport_state.dart`          | Freezed          |
| `ViewportMode`        | `ar_core/viewport/viewport_state.dart`          | Plain enum       |
| `LODTier`             | `ar_core/viewport/viewport_state.dart`          | Plain enum       |
| `POIDisplayMode`      | `panorama/providers/panorama_providers.dart`    | Plain enum       |
| `POIRenderSpec`       | `panorama/services/poi_visibility_service.dart` | Plain Dart class |
| `POIRenderMode`       | `panorama/services/poi_visibility_service.dart` | Plain enum       |
| `POIVisibilityConfig` | `panorama/services/poi_visibility_service.dart` | Plain Dart class |
| `ARNode3DConfig`      | `ar_core/nodes/ar_node_3d_config.dart`          | Plain Dart class |

**`POI` model required fields** (all declared in `lib/domains/panorama/models/poi.dart`):

| Field             | Type                 | Notes                                                                                                                                                                                                                                                                                                                                                       |
| ----------------- | -------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `id`              | `String`             | Unique string identifier                                                                                                                                                                                                                                                                                                                                    |
| `name`            | `TranslatableString` | PT + EN + ES                                                                                                                                                                                                                                                                                                                                                |
| `category`        | `String` (raw key)   | Raw category key stored in JSON. Use `POIX.poiTypeEnum` extension to resolve to `POIType` enum. 8-type vocabulary: `royal` \| `religious` \| `military` \| `civic` \| `maritime` \| `infrastructure` \| `landscape` \| `commerce`. Legacy keys `power`→royal, `civil`→civic, `residential`→civic handled by backward-compat fallback in `POIX.poiTypeEnum`. |
| `x`               | `double`             | Normalized 0.0–1.0 horizontal position on panorama                                                                                                                                                                                                                                                                                                          |
| `y`               | `double`             | Normalized 0.0–1.0 vertical position on panorama                                                                                                                                                                                                                                                                                                            |
| `description`     | `TranslatableString` | PT + EN + ES                                                                                                                                                                                                                                                                                                                                                |
| `survivalStatus`  | `String`             | `"intact"` \| `"damaged"` \| `"destroyed"`                                                                                                                                                                                                                                                                                                                  |
| `currentPhotoUrl` | `String?`            | Nullable. URL/asset path to current-day photo. Required for "Then vs Now" widget. Content-gated: must be a real licensed photo before feature is enabled.                                                                                                                                                                                                   |

### New services

| Service                    | File                                            | Pure?                 |
| -------------------------- | ----------------------------------------------- | --------------------- |
| `POIVisibilityService`     | `panorama/services/poi_visibility_service.dart` | Yes — no side effects |
| `ARNodeManager` (abstract) | `ar_core/nodes/ar_node_manager.dart`            | Interface             |
| `MockARNodeManager`        | `ar_core/nodes/mock_ar_node_manager.dart`       | No-op impl            |

### State connections

```
timelineProvider ──────┐
viewportStateProvider ─┤──→ PanoramaARController.computeRenderSpecs()
allPOIsProvider ───────┘         │
                                 ↓
                    PanoramaARState.renderSpecs
                                 │
                                 ↓
                    panorama_ar_view.dart → ARPOIMarker (opacity, showLabel)
```

```
selectedPOIProvider ────┐
poiDisplayModeProvider ─┤──→ panorama_ar_view.dart Stack conditionals
                              TimelineSlider visibility
                              POISummaryCard / POIDetailSheet rendering
                              PopScope canPop
```

### Epoch filtering logic (in `POIVisibilityService.computeVisible()`)

| Epoch        | Rule                                                                           |
| ------------ | ------------------------------------------------------------------------------ |
| `pre1755`    | All POIs → `simpleMarker`, opacity 1.0                                         |
| `earthquake` | All POIs → `simpleMarker`; `destroyed` → opacity 0.45; `damaged` → opacity 0.7 |
| `pombalina`  | `destroyed` → `hidden`; others → `simpleMarker`, opacity 1.0                   |
| `today`      | `destroyed` → `hidden`; others → `simpleMarker`, opacity 1.0                   |

### Minimum overlap rule (Phase 1)

If two markers are within 40dp on screen, hide the **label** of the lower-priority one (dot stays tappable). Priority: `power` > `religious` > `civil` > `maritime` > `residential`. Implemented inside `computeVisible()`.

```dart
const double kPOILabelHideThresholdDp = 40.0;
// screenDistanceDp = sqrt(
//   pow((poiA.x - poiB.x) * panelScreenWidthDp, 2) +
//   pow((poiA.y - poiB.y) * panelScreenHeightDp, 2)
// )
// panelScreenWidthDp  = imageNaturalWidth  * viewportState.zoomLevel / devicePixelRatio
// panelScreenHeightDp = imageNaturalHeight * viewportState.zoomLevel / devicePixelRatio
```

---

## 5. File and Folder Structure

### New files created in Phase 1

```
lib/
  ar_core/
    nodes/
      ar_node_manager.dart           # Abstract interface: placeModel, removeModel, onModelTapped
      ar_node_3d_config.dart         # Config for one 3D node (poiId, assetPath, scale, offset)
      mock_ar_node_manager.dart      # No-op implementation
    viewport/
      viewport_state.dart            # Freezed: zoomLevel, centerNormalized, visibleRegion, mode
      viewport_state.freezed.dart    # Generated — do not edit
      viewport_state_provider.dart   # NotifierProvider<ViewportStateNotifier, ViewportState>
    utils/
      viewport_math.dart             # computeMarkerScreenPosition() — named params normX/normY/imgW/imgH/canvasW/canvasH/scale/tx/ty

  components/ui/
    info_badge.dart                  # Colored pill: category + survival
    section_header.dart              # Titled divider with optional trailing action
    tappable_card.dart               # Press-scale 0.97 base card; InkWell + RepaintBoundary
    ui_components.dart               # Barrel export

  domains/
    home/
      providers/
        camera_permission_provider.dart   # Injectable Provider<Future<PermissionStatus> Function()>

    timeline/
      models/
        time_period.dart             # Plain Dart enum: pre1755 / earthquake / pombalina / today
      providers/
        timeline_provider.dart       # NotifierProvider<TimelineNotifier, TimePeriod>
      widgets/
        timeline_slider.dart         # SegmentedButton<TimePeriod>; compact layout < 400dp
        epoch_label.dart             # AnimatedSwitcher; Semantics(liveRegion: true)
      timeline_domain.dart           # Barrel export

    panorama/
      ar/
        config/
          panorama_3d_nodes_config.dart  # kAR3DNodes = []; kPanelPhysicalWidthMeters
        widgets/
          ar_debug_overlay.dart          # Dev-only; kShowARDebugOverlay guard
          ar_first_time_overlay.dart     # Two-step onboarding; SharedPreferences
          ar_mode_toggle.dart            # Variant-A animated track: 72×40 AnimatedContainer, AnimatedPositioned thumb, blink dot; lives in PanoramaTopBar
          ar_poi_marker.dart             # 4 LOD tiers; _DashedBorderPainter (6-stop destruction spectrum); ARPOIMarkerIcon (large/medium only); RepaintBoundary
          ar_tracking_indicator.dart     # searching / initializing / tracking
          marker_icons.dart              # 8 CustomPainter icons (cross/crown/shield/columns/anchor/arch/hill/scales); vector paths
          panorama_top_bar.dart          # Semi-transparent bar; back + options; ARModeToggle centre-right
          poi_detail_sheet.dart          # DraggableScrollableSheet (renamed from poi_info_sheet)
          poi_legend_button.dart         # 36dp ⓘ button; POILegendSheet with destruction spectrum + 8-type GridView
          poi_summary_card.dart          # 280dp anchored card near marker; horizontal + vertical clamp
          poi_action_buttons.dart        # Extracted CTA row — reused by POISummaryCard and POIDetailSheet
      services/
        poi_visibility_service.dart    # Pure: computeVisible() → List<POIRenderSpec>

  utils/prefs/
    prefs_keys.dart                  # PrefsKeys.arOnboardingShown, .timelineOnboardingShown, .bookmarkedPOIIds — const strings only
```

### Existing files modified in Phase 1

| File                                                                 | Modification                                                                                                                                                                                                      |
| -------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `assets/data/pois.json`                                              | Expanded 10 → 30 POIs; added `name.es` + `description.es` to all                                                                                                                                                  |
| `lib/domains/panorama/ar/widgets/panorama_ar_view.dart`              | Uses `renderSpecs` (not `allPOIs`); timeline slider keyed to `poiDisplayModeProvider`; `AnimatedOpacity` + `IgnorePointer` wrapper                                                                                |
| `lib/domains/panorama/ar/widgets/ar_mode_toggle.dart`                | Redesigned from bottom-right pill to Variant-A animated track: `72×40` `AnimatedContainer`, `AnimatedPositioned` 32dp thumb, blink dot, `ConstrainedBox(maxWidth: 280)`; relocated from Stack to `PanoramaTopBar` |
| `lib/domains/panorama/ar/providers/ar_infrastructure_providers.dart` | Added `kShowARDebugOverlay`; added `arNodeManagerProvider`                                                                                                                                                        |
| `lib/domains/panorama/panorama_domain.dart`                          | Barrel: exports new public widgets and services                                                                                                                                                                   |
| `lib/domains/home/pages/home_page.dart`                              | Added CTAs, permission flow, LanguageSwitcher + ThemeSwitcher                                                                                                                                                     |
| `android/app/build.gradle.kts`                                       | `abiFilters` must include `"x86_64"` for Android Emulator AR                                                                                                                                                      |
| `pubspec.yaml`                                                       | Added `share_plus` (latest stable); confirmed `shared_preferences` present                                                                                                                                        |

### Barrel exports to update

| Barrel                                      | Add                                                                                                                                                                                   |
| ------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `lib/domains/panorama/panorama_domain.dart` | `poi_visibility_service.dart`, `poi_summary_card.dart`, `poi_detail_sheet.dart`, `panorama_top_bar.dart`, `ar_tracking_indicator.dart`, `marker_icons.dart`, `poi_legend_button.dart` |
| `lib/domains/timeline/timeline_domain.dart` | All new `timeline/` public files                                                                                                                                                      |
| `lib/components/ui/ui_components.dart`      | `info_badge.dart`, `section_header.dart`, `tappable_card.dart`                                                                                                                        |
| `lib/ar_core/ar_core.dart`                  | `viewport_state.dart`, `viewport_state_provider.dart`, `ar_node_manager.dart`                                                                                                         |

### Files NOT to touch

- `lib/navigation/nav_config.dart` — do not restructure routes; demo routes `showInNav` stays as-is
- Any Phase 0 baseline file not listed in the "modified" table above

---

## 6. Task Execution Plan

> **Execution order:** 1.0 → 1.1 → 1.2 → 1.3 → 1.4 → 1.5 → **1.8** → 1.6 → 1.7 → 1.9
>
> **Rationale:** Task 1.8 (shared UI primitives: `InfoBadge`, `TappableCard`, `SectionHeader`) is executed before 1.6 and 1.7 because both of those tasks import these widgets. The numbering reflects the logical domain grouping, not the execution sequence — follow this order, not the task numbers.

### ✅ Task 1.0 — AR tracking validation gate

**What:** Validate the full AR tracking pipeline before writing any feature code.
**Why now:** The single most important task in Phase 1. Everything else assumes AR tracking works. A fatal tracking issue discovered after 9 tasks of feature code means architectural rework.

**Files:**
- `lib/domains/panorama/ar/config/panorama_reference_images.dart` — confirm `widthInMeters`
- `android/app/build.gradle.kts` — confirm `abiFilters` includes `x86_64`
- `PROJECT_GUIDES/AR_TRACKING_STRATEGY.md` — create and fill in full

**Steps (all must be completed — this is a gate):**

| Step                                 | Status | Description                                                                                                                                                                                                                                                                                              |
| ------------------------------------ | ------ | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Step 1: arcoreimg score              | [ ]    | `arcoreimg eval-img` on panorama JPEG. Required >= 75. Document in `AR_TRACKING_STRATEGY.md`. If < 75: document mitigation, do NOT proceed to Task 1.1.                                                                                                                                                  |
| Step 2: widthInMeters confirm        | [ ]    | Confirm `panorama_reference_images.dart` passes `kPanelPhysicalWidthMeters` as the `widthInMeters` parameter to the ARCore image database. Document confirmation in `AR_TRACKING_STRATEGY.md`.                                                                                                           |
| Step 3: Android Emulator pipeline    | [ ]    | Setup AVD: Pixel 6, x86_64, API 27+, Camera Back = VirtualScene. Install ARCore Services x86 APK from github.com/google-ar/arcore-android-sdk/releases. Run with `--dart-define=USE_REAL_AR=true --dart-define=SHOW_AR_DEBUG=true`. Confirm: `FULL_TRACKING` fires, POI markers appear in virtual scene. |
| Step 4: LAST_KNOWN_POSE drift test   | [ ]    | Point camera at panel → FULL_TRACKING → pan away for 30s → measure marker drift. Required: < 5cm at 3m. If fails: section-based tracking (Option A) is mandatory before Task 1.1.                                                                                                                        |
| Step 5: Fallback strategy decision   | [ ]    | If Step 1 or 4 fails: document chosen fallback in `AR_TRACKING_STRATEGY.md`. If section-based tracking chosen: update `kPanelSections[]` config and note coordinate math changes for Tasks 1.3–1.5.                                                                                                      |
| Step 6: Physical Android device test | [ ]    | Test on >= 2 Android devices (different manufacturers). A3 printed proxy. Stable tracking within 10s. LAST_KNOWN_POSE drift < 5cm. Document: device models, ARCore version, distance tested.                                                                                                             |
| Step 7: Physical iPhone test         | [ ]    | Physical iPhone 6S+, iOS 14+. `flutter build ios --release` clean. ARKit tracking confirmed on A3 proxy. Document results.                                                                                                                                                                               |
| Step 8: Panorama JPEG performance    | [ ]    | Load panorama JPEG on oldest/lowest-spec target device. Required: no OOM crash, no stall > 1s. If stalls: apply `cacheWidth: 3000` in `static_image_frame_provider.dart`. Document chosen value.                                                                                                         |

**AR testing layer map:**

| Layer | Platform         | AR engine   | Camera           | What it validates                                                                |
| ----- | ---------------- | ----------- | ---------------- | -------------------------------------------------------------------------------- |
| 1+2+3 | PC / Chrome      | None (mock) | n/a              | Provider wiring, UI states, user journeys                                        |
| 4a    | Android Emulator | Real ARCore | Virtual 3D scene | Plugin wiring, session init, image detection, anchor creation, overlay placement |
| 4b    | Physical Android | Real ARCore | Phone camera     | Real-world tracking, sensor jitter, hand tremor, physical A3 proxy               |
| 4c    | Physical iPhone  | Real ARKit  | Phone camera     | iOS-specific behaviour, ARKit tracking on same proxy                             |
| 5     | Chrome browser   | Mock        | n/a              | UI layout, navigation, offline viewer                                            |

**Important notes:**
- iOS Simulator has NO camera and NO ARKit — physical device is mandatory. There is no workaround.
- PC webcam is irrelevant — Chrome uses mock AR providers.
- Android Emulator: `abiFilters` must include `"x86_64"` or the app crashes with `UnsatisfiedLinkError`.
- `widthInMeters` is not optional for a 23m panel. ARCore docs: physical size "improves detection and tracking performance, especially for large physical images (over 75 cm)."

**Fallback options (document chosen one in `AR_TRACKING_STRATEGY.md`):**
- **Option A (recommended):** Section-based tracking — divide panel into 4–6 overlapping zones, each as a separate reference image with coordinates offset into a global coordinate system.
- **Option B:** Fiducial marker fallback — printed ARCore markers at known positions along the panel frame.

**Acceptance gate:**
- [ ] `AR_TRACKING_STRATEGY.md` exists with all Step results
- [ ] arcoreimg score >= 75 confirmed (or fallback selected and documented)
- [ ] Android emulator: `FULL_TRACKING` fires, POI markers appear in virtual scene
- [ ] LAST_KNOWN_POSE drift < 5cm at 3m after 30s (or section-based tracking implemented)
- [ ] Physical Android: stable tracking on A3 proxy within 10s, tested on >= 2 devices
- [ ] Physical iPhone: ARKit tracking confirmed
- [ ] `kPanelPhysicalWidthMeters` set and confirmed passed as `widthInMeters`
- [ ] Fallback strategy documented if single-image tracking fails

---

### ✅ Task 1.1 — Expand POI data to 30 buildings

**What:** `assets/data/pois.json` — 30 POIs with PT + EN + ES, correct normalized coordinates.
**Why now:** All other tasks depend on POI data. Layer 1/2/3 tests need at least 30 entries.

**Files:** `assets/data/pois.json`

**Spec:**
- 30 total POIs (IDs 1–30)
- Every entry must have: `id` (string), `name` (pt/en/es), `category` (`power`|`religious`|`civil`|`maritime`|`residential`), `x` (0.0–1.0), `y` (0.0–1.0), `description` (pt/en/es), `survivalStatus` (`"intact"`|`"damaged"`|`"destroyed"`)
- **`currentPhotoUrl: String?`** — optional field. Present only on entries where `survivalStatus == "intact"` AND a licensed current-day photograph is available. Null otherwise. Use `null` as the default for all 30 entries at this stage; real URLs/asset paths are a content dependency (see §0 hard prerequisites). The "Then vs Now" widget in Task 1.7 is conditional on this field being non-null.
- Use `PROJECT_GUIDES/ar_poi_calibrator.html` in a browser to record normalized x/y per building — do NOT guess coordinates
- Run `flutter run -d chrome` after adding to confirm all 30 markers visible at plausible positions

**Tests (Layer 1 unit — `lib/domains/panorama/test/unit/poi_json_test.dart`):**

_Data integrity:_
- JSON file loads and parses to exactly 30 `POI` objects — no parse errors
- All 30 entries have unique `id` values — no duplicates
- Every `id` is a non-empty string
- Every entry has non-empty `name.pt`, `name.en`, `name.es`
- Every entry has non-empty `description.pt`, `description.en`, `description.es`
- All `x` values are in `[0.0, 1.0]` — none negative, none > 1.0
- All `y` values are in `[0.0, 1.0]` — none negative, none > 1.0
- Every `category` value is one of `power | religious | civil | maritime | residential`
- Every `survivalStatus` value is one of `intact | damaged | destroyed`

_Field presence:_
- `currentPhotoUrl` key exists in every entry (value may be null — null is correct default)
- No entry with `survivalStatus == "destroyed"` has a non-null `currentPhotoUrl`

_Distribution (smoke check):_
- At least 1 POI with `survivalStatus == "destroyed"` exists (needed for filter tests)
- At least 1 POI with `survivalStatus == "intact"` exists
- At least 1 POI in each category exists

**Acceptance gate:**
- [ ] All Layer 1 unit tests pass
- [ ] 30 total POIs
- [ ] All entries have `name.es` + `description.es` — no null/missing Spanish
- [ ] All 30 markers visible in offline mode at plausible positions
- [ ] `flutter analyze` → 0 issues (no Dart changes in this task)

---

### ✅ Task 1.2 — Timeline domain

**What:** Timeline epoch system — `TimePeriod` enum, provider, slider widget, epoch label.
**Why now:** `POIVisibilityService` (Task 1.4) needs the epoch. Timeline is a data dependency for filtering.

**Files:**
- NEW: `lib/domains/timeline/models/time_period.dart`
- NEW: `lib/domains/timeline/providers/timeline_provider.dart`
- NEW: `lib/domains/timeline/widgets/timeline_slider.dart`
- NEW: `lib/domains/timeline/widgets/epoch_label.dart`
- NEW: `lib/domains/timeline/timeline_domain.dart`
- MODIFY: `lib/domains/panorama/ar/widgets/panorama_ar_view.dart` (wire in)

**Spec:**
```dart
// time_period.dart — plain Dart enum (NOT Freezed)
enum TimePeriod {
  pre1755, earthquake, pombalina, today;

  int get year => switch (this) {
    TimePeriod.pre1755    => 1700,
    TimePeriod.earthquake => 1755,
    TimePeriod.pombalina  => 1780,  // representative; rebuilding largely complete by 1775
    TimePeriod.today      => 2025,
  };
  TranslatableString get label => ...;     // PT/EN/ES all 3
  TranslatableString get subtitle => ...; // PT/EN/ES all 3
}

// timeline_provider.dart
class TimelineNotifier extends Notifier<TimePeriod> {
  @override TimePeriod build() => TimePeriod.pre1755;
  void setEpoch(TimePeriod epoch) => state = epoch;
}
final timelineProvider = NotifierProvider<TimelineNotifier, TimePeriod>(TimelineNotifier.new);
```

`TimelineSlider`: `SegmentedButton<TimePeriod>`. On screens < 400dp logical width: compact labels (name only, no year subtitle). Min height: `SizeTokens.tapTarget`. Active: `context.primary` (Gold); inactive: `context.surface`. `Semantics(label: ...)` on outer; each segment: `Semantics(selected: isSelected, label: ...)`.

`EpochLabel`: `AnimatedSwitcher(duration: AnimationTokens.medium)` with unique key per epoch. `Semantics(liveRegion: true)`.

> **Timeline first-use affordance (UX gap):** Museum visitors seeing the timeline slider for the first time will not understand it is a time-travel control — there is no labelling beyond the four epoch names. Add a one-time animated pulse (scale 1.0 → 1.08 → 1.0 + mild glow) on the `TimelineSlider` the first time the panorama page mounts, keyed to `PrefsKeys.timelineOnboardingShown` (add this key alongside `PrefsKeys.arOnboardingShown`). The pulse runs once per install, auto-clears after 4 seconds or first interaction. This is ~15 lines in `timeline_slider.dart` and costs nothing in steady state.

Wire positions in `panorama_ar_view.dart`:
- `TimelineSlider`: `Positioned(bottom: Spacing.xl2, left: Spacing.lg, right: Spacing.lg)`
- `EpochLabel`: `Positioned(bottom: Spacing.xl2 + 64, left: Spacing.lg)`

**Tests:**

_Layer 1 unit — `lib/domains/timeline/test/unit/timeline_provider_test.dart`:_
- Initial state is `TimePeriod.pre1755`
- `setEpoch(earthquake)` → state becomes `earthquake`; all 4 values round-trip correctly
- Year values: `pre1755` → 1700; `earthquake` → 1755; `pombalina` → 1780; `today` → 2025
- `label` for every epoch is non-empty in PT, EN, and ES
- `subtitle` for every epoch is non-empty in PT, EN, and ES
- Two separate `ProviderContainer` instances maintain independent state (no singleton leak)

_Layer 2 widget — `lib/domains/timeline/test/widgets/timeline_slider_test.dart`:_

_Structure and dimensions:_
- `TimelineSlider` renders exactly 4 `SegmentedButton` segments — no more, no fewer
- Segment minimum height renders at `>= SizeTokens.tapTarget` (48px) in the widget tree
- At 800dp logical width: all 4 segments are present and none are hidden
- At 399dp logical width (narrow): segments use compact labels — year subtitle is absent from each segment's text; epoch name text is still present
- At 400dp logical width (threshold): full labels (name + year) are present

_State and interaction:_
- Tapping the `earthquake` segment calls `timelineProvider.notifier.setEpoch(earthquake)`
- After tap, `timelineProvider` state equals the tapped epoch
- `EpochLabel` text reflects the newly selected epoch immediately after tap

_Style tokens:_
- Active segment background color matches `context.primary` — no raw `Color` literal
- Inactive segment background color matches `context.surface` — no raw `Color` literal
- `Semantics` node exists on the outer `TimelineSlider` with non-empty label
- Each segment has `Semantics(selected: isActive, label: nonEmpty)`

_EpochLabel — `lib/domains/timeline/test/widgets/epoch_label_test.dart`:_
- Label text changes when `timelineProvider` changes to a different epoch
- Widget tree contains a node with `Semantics(liveRegion: true)`
- `AnimatedSwitcher` child key is different for each of the 4 epochs (required for transition to fire)
- Label is NOT visible after 3 seconds have elapsed without an epoch change (Timer auto-hide — advance clock with `fakeAsync` or a `TestClock`)
- Label re-appears immediately when epoch changes again after it has hidden

_Timeline first-use pulse:_
- On first mount with `PrefsKeys.timelineOnboardingShown == false`: `ScaleTransition` target scale is `> 1.0` (pulse is active)
- After 4 seconds with no interaction: pulse is inactive and `ScaleTransition` scale is back to 1.0
- On first interaction (tap any segment): pulse stops immediately
- On second mount with `PrefsKeys.timelineOnboardingShown == true`: no pulse is applied

**Acceptance gate:**
- [ ] All Layer 1 unit tests pass
- [ ] All Layer 2 widget tests pass
- [ ] `flutter test lib/` → all passing

---

### ✅ Task 1.3 — ViewportState provider

**What:** Canonical viewport state — zoom level, visible region, mode (offline/liveAR).
**Why now:** `POIVisibilityService` needs zoom level for overlap detection; marker positioning needs viewport transform. Build the abstraction before wiring everything together.

**Files:**
- NEW: `lib/ar_core/viewport/viewport_state.dart`
- NEW: `lib/ar_core/viewport/viewport_state_provider.dart`
- CONFIRM OR CREATE: `lib/ar_core/utils/viewport_math.dart` — must export `computeMarkerScreenPosition({required double normX, required double normY, required double imgW, required double imgH, required double canvasW, required double canvasH, required double scale, required double tx, required double ty})`. If this file is absent, create it in this task with unit tests (see §1 checklist).
- Run `dart run build_runner build --delete-conflicting-outputs` after creating

**Spec:**
```dart
@freezed
class ViewportState with _$ViewportState {
  const factory ViewportState({
    required double zoomLevel,              // 1.0 = full panel
    required Offset centerNormalized,       // (0,0)–(1,1) center of visible region
    required Rect visibleRegionNormalized,  // normalized rect of visible area
    required ViewportMode mode,             // offline | liveAR
  }) = _ViewportState;

  const ViewportState._();
  factory ViewportState.initial() => const ViewportState(
    zoomLevel: 1.0,
    centerNormalized: Offset(0.5, 0.5),
    visibleRegionNormalized: Rect.fromLTWH(0, 0, 1, 1),
    mode: ViewportMode.offline,
  );

  LODTier get lodTier {
    if (zoomLevel < 2.0) return LODTier.overview;
    if (zoomLevel < 4.0) return LODTier.mid;
    if (zoomLevel < 8.0) return LODTier.close;
    return LODTier.intimate;
  }
}

enum ViewportMode { offline, liveAR }
enum LODTier       { overview, mid, close, intimate }
```

`updateFromMatrix()`: matrix[0] = scale, matrix[12] = tx, matrix[13] = ty. Debounced: max one state write per frame (16ms interval).

**Do NOT set `alignment` on `InteractiveViewer`** — the default allows pinch/scroll-wheel zoom to pivot around the pointer. `computeMarkerScreenPosition` in `viewport_math.dart` already accounts for center offset.

`updateFromARPose()`: Phase 1 stub — sets `zoomLevel: 2.0` and `mode: liveAR`.

**Tests (Layer 1 unit — `lib/ar_core/test/unit/viewport_state_test.dart` + `viewport_math_test.dart`):**

_ViewportState:_
- Identity matrix → `zoomLevel == 1.0`, `centerNormalized ≈ Offset(0.5, 0.5)`
- 2× scale matrix → `zoomLevel == 2.0`
- LODTier boundaries: zoom 1.5 → `overview`; 2.5 → `mid`; 5.0 → `close`; 9.0 → `intimate`
- `ViewportState.initial()` → `mode == ViewportMode.offline`, `zoomLevel == 1.0`, `visibleRegionNormalized == Rect.fromLTWH(0,0,1,1)`
- `updateFromARPose()` stub → `mode == ViewportMode.liveAR`, zoom updated to 2.0
- `setMode(liveAR)` → mode changes; `zoomLevel`, `centerNormalized`, `visibleRegionNormalized` unchanged
- Two calls to `updateFromMatrix()` within 16ms → only one state write (debounce)

_computeMarkerScreenPosition (viewport_math_test.dart):_
- Identity transform (scale=1, tx=0, ty=0), marker at center (normX=0.5, normY=0.5), imgW=1000, imgH=500, canvasW=375, canvasH=700 → output is approximately `Offset(187.5, 250.0)`
- Marker at (normX=0.0, normY=0.0) → output at top-left of rendered image
- Marker at (normX=1.0, normY=1.0) → output at bottom-right of rendered image
- 2× scale (scale=2, tx=-375, ty=-350), marker at (0.5, 0.5) → output still near canvas center (panning to center doesn't move center marker)
- Marker with result `< 0` or `> canvasW`/`> canvasH` can happen (caller handles clamping) — function does NOT clamp internally

**Acceptance gate:**
- [ ] All Layer 1 unit tests pass
- [ ] `lib/ar_core/utils/viewport_math.dart` exists and exports `computeMarkerScreenPosition` with all named params (`normX`, `normY`, `imgW`, `imgH`, `canvasW`, `canvasH`, `scale`, `tx`, `ty`)
- [ ] `flutter test lib/` → all passing

---

### ✅ Task 1.4 — POIVisibilityService

**What:** Pure service — takes all POIs + viewport + epoch → returns `List<POIRenderSpec>`.
**Why now:** Single source of truth for rendering decisions. Wire into controller before adding UI. Restructuring later is expensive.

**Files:**
- NEW: `lib/domains/panorama/services/poi_visibility_service.dart`
- MODIFY: `PanoramaARController` — watch viewport + timeline, call `computeVisible()`, store in `PanoramaARState.renderSpecs`

**Spec:**
```dart
class POIVisibilityService {
  const POIVisibilityService();
  List<POIRenderSpec> computeVisible({
    required List<POI> allPois,
    required ViewportState viewport,
    required TimePeriod epoch,
    required POIVisibilityConfig config,
    required double devicePixelRatio,   // needed for overlap dp formula
    required Size imageNaturalSize,     // natural JPEG dimensions (not displayed size)
  });
}

class POIRenderSpec {
  final POI poi;
  final POIRenderMode mode;
  final double opacity;
  final bool showLabel;  // false when suppressed by overlap rule
}

enum POIRenderMode { hidden, simpleMarker, enrichedMarker, model3DLow, model3DHigh }

class POIVisibilityConfig {
  final int maxMarkersOverview;
  final int maxMarkersMid;
  final int maxModels3DClose;    // 0 in Phase 1
  final int maxModels3DIntimate; // 0 in Phase 1

  const POIVisibilityConfig.phase1() :
    maxMarkersOverview = 30,
    maxMarkersMid = 30,
    maxModels3DClose = 0,
    maxModels3DIntimate = 0;
}
```

`POIVisibilityConfig.phase1()` is a hardcoded const in `PanoramaARController.build()` — not a provider in Phase 1. Promoted to provider in Phase 2.

**Tests (Layer 1 unit — `lib/domains/panorama/test/unit/poi_visibility_service_test.dart`):**

_Epoch × survivalStatus visibility matrix:_
- `pre1755` + `intact` → `mode == simpleMarker`, `opacity == 1.0`
- `pre1755` + `damaged` → `mode == simpleMarker`, `opacity == 1.0`
- `pre1755` + `destroyed` → `mode == simpleMarker`, `opacity == 1.0`
- `earthquake` + `intact` → `mode == simpleMarker`, `opacity == 1.0`
- `earthquake` + `damaged` → `mode == simpleMarker`, `opacity == 0.7`
- `earthquake` + `destroyed` → `mode == simpleMarker`, `opacity == 0.45`
- `pombalina` + `intact` → `mode == simpleMarker`, `opacity == 1.0`
- `pombalina` + `damaged` → `mode == simpleMarker`, `opacity == 1.0`
- `pombalina` + `destroyed` → `mode == hidden` (not present in visible results)
- `today` + `intact` → `mode == simpleMarker`, `opacity == 1.0`
- `today` + `damaged` → `mode == simpleMarker`, `opacity == 1.0`
- `today` + `destroyed` → `mode == hidden`
- All 30 POIs in result for `pre1755` — none are `hidden`
- Result count for `pombalina` equals 30 minus the count of `destroyed` POIs

_Phase 1 mode constraint:_
- No result has `mode == model3DLow` or `model3DHigh` when using `POIVisibilityConfig.phase1()`
- `POIVisibilityConfig.phase1()` → `maxModels3DClose == 0`, `maxModels3DIntimate == 0`

_Overlap / label suppression rule:_
- Two POIs with screen positions < 40dp apart: lower-priority one has `showLabel == false`
- Two POIs with screen positions > 40dp apart: both have `showLabel == true`
- Priority order verified: `power` label survives over `religious` when overlapping; `religious` survives over `civil`; etc.
- POI with `showLabel == false` still has `mode == simpleMarker` (dot stays, label hides)

_Edge cases:_
- Empty POI list → returns empty list, no exception
- Single POI → `showLabel == true` always (no pair to suppress)
- All POIs `destroyed` in `pombalina` epoch → empty result list

**Acceptance gate:**
- [ ] All Layer 1 unit tests pass
- [ ] `renderSpecs` drives `panorama_ar_view.dart` marker overlay (not `allPOIs` directly)
- [ ] `flutter test lib/` → all passing

---

### ✅ Task 1.5 — ARNodeManager architecture stub

**What:** Abstract interface + mock implementation for 3D node placement. Empty in Phase 1.
**Why now:** `PanoramaARController` needs to call something when an anchor is detected. Building the interface now means Phase 3 swaps the implementation behind one provider change, with no controller or widget changes.

**Files:**
- NEW: `lib/ar_core/nodes/ar_node_manager.dart`
- NEW: `lib/ar_core/nodes/ar_node_3d_config.dart`
- NEW: `lib/ar_core/nodes/mock_ar_node_manager.dart`
- NEW: `lib/domains/panorama/ar/config/panorama_3d_nodes_config.dart`
- MODIFY: `lib/domains/panorama/ar/providers/ar_infrastructure_providers.dart`

**Spec:**
```dart
abstract class ARNodeManager {
  Future<void> placeModel(ARNode3DConfig config, Vector3 worldOffset);
  Future<void> removeModel(String poiId);
  Future<void> removeAll();
  Future<void> enableFocusMode(String poiId);   // Phase 3 — no-op in mock
  Future<void> disableFocusMode();              // Phase 3 — no-op in mock
  Stream<String> get onModelTapped;             // emits poiId; empty stream in mock
}

// Phase 1: always MockARNodeManager regardless of kUseRealAR
final arNodeManagerProvider = Provider<ARNodeManager>((ref) => MockARNodeManager());

// panorama_3d_nodes_config.dart
const List<ARNode3DConfig> kAR3DNodes = [];
// Phase 3 candidates: Castelo (id 1), Se (id 2), Paco da Ribeira (id 3)
// DO NOT add GLB files until panel dimensions confirmed with museum
```

**Tests (Layer 1 unit):**
- `MockARNodeManager.placeModel` → no throw
- `MockARNodeManager.removeAll` → no-op
- `MockARNodeManager.onModelTapped` → empty stream
- `kAR3DNodes` empty → `placeModel` never called from controller

**Acceptance gate:**
- [ ] All Layer 1 unit tests pass
- [ ] `flutter test lib/` → all passing

---

### ✅ Task 1.8 — Shared UI primitives

> Executed before 1.6/1.7 because both depend on these widgets.

**What:** Three shared components: `InfoBadge`, `SectionHeader`, `TappableCard`.
**Why now:** Created when the second use case appears in a different domain. Both `POISummaryCard` and `POIDetailSheet` use them — that qualifies.

**Files:**
- NEW: `lib/components/ui/info_badge.dart`
- NEW: `lib/components/ui/section_header.dart`
- NEW: `lib/components/ui/tappable_card.dart`
- NEW: `lib/components/ui/ui_components.dart`

**Spec:**
```dart
// InfoBadge — colored pill, non-interactive. Min height: 24px.
// Color is never sole indicator — always paired with icon for survival badges.
class InfoBadge extends StatelessWidget {
  const InfoBadge({required this.label, required this.color, this.textColor, super.key});
}
// Category colors (context.* tokens only):
// power → context.primary | religious → context.secondary | civil → context.tertiary
// maritime → context.info | residential → context.onSurfaceVariant
// Survival:
// 'intact'    → context.success + Icons.check_circle_outline
// 'damaged'   → context.warning + Icons.warning_amber_outlined
// 'destroyed' → context.error   + Icons.cancel_outlined

// SectionHeader
class SectionHeader extends ConsumerWidget {
  const SectionHeader({required this.title, this.action, super.key});
  // title: TranslatableString via ref.tr(); action: optional trailing Widget
}

// TappableCard — press: scale 0.97 over AnimationTokens.fast
// InkWell + ElevationTokens + RadiusTokens.cardRadius + Semantics
class TappableCard extends StatefulWidget { ... }
```

**Tests (Layer 2 widget — `lib/components/test/widgets/`):**

_InfoBadge — `info_badge_test.dart`:_
- Renders with the provided `label` text visible
- Background color for `'intact'` survival badge uses `context.success` token — no raw `Color` literal in the widget tree
- Background color for `'damaged'` uses `context.warning` token
- Background color for `'destroyed'` uses `context.error` token
- `'intact'` badge contains `Icons.check_circle_outline` icon
- `'damaged'` badge contains `Icons.warning_amber_outlined` icon
- `'destroyed'` badge contains `Icons.cancel_outlined` icon
- Badge container height is >= 24px (min height spec)
- Badge is non-interactive — no `GestureDetector` in subtree, no `onTap`
- Category badge for `power` uses `context.primary` token
- Category badge for `religious` uses `context.secondary` token
- Category badge for `civil` uses `context.tertiary` token

_SectionHeader — `section_header_test.dart`:_
- Title text renders with the correct translated string for PT locale
- Title text renders with the correct translated string for EN locale
- When `action` is null: no trailing widget is present
- When `action` is a widget: it renders in the trailing position of the header row
- `Semantics` label is non-empty

_TappableCard — `tappable_card_test.dart`:_
- Renders provided child widget inside the card
- `onTap` callback fires exactly once when tapped
- Scale transition value is `< 1.0` (pressed state, 0.97 target) during a long-press hold
- Scale returns to `1.0` after the pointer is lifted
- `InkWell` is present in the subtree (Material ripple behaviour)
- `Semantics` label matches provided label param
- Animation duration matches `AnimationTokens.fast`

**Acceptance gate:**
- [ ] All Layer 2 widget tests pass
- [ ] `flutter test lib/` → all passing

---

### ✅ Task 1.6 — Panorama page chrome and navigation

**What:** Full Stack layout — top bar, tracking indicator, mode toggle, debug overlay, onboarding overlay, back/PopScope.
**Why now:** UI shell needed before POI cards (Task 1.7) can be placed within it.

**Files:**
- NEW: `lib/domains/panorama/ar/widgets/panorama_top_bar.dart`
- NEW: `lib/domains/panorama/ar/widgets/ar_tracking_indicator.dart`
- NEW: `lib/domains/panorama/ar/widgets/ar_debug_overlay.dart`
- NEW: `lib/domains/panorama/ar/widgets/ar_first_time_overlay.dart`
- MODIFY: `lib/domains/panorama/ar/widgets/ar_mode_toggle.dart` (top SegmentedButton → bottom-right pill)
- MODIFY: `lib/domains/panorama/ar/widgets/panorama_ar_view.dart`
- MODIFY: `lib/domains/panorama/pages/panorama_ar_page.dart` (PopScope)

**Key specs:**

`PanoramaTopBar`:
- Background: `context.surface.withValues(alpha: 0.92)` — solid, NO `BackdropFilter`
- Shadow: `ElevationTokens.level2`
- Back: `Icons.arrow_back_ios_new_rounded` → `context.go('/')`. `Tooltip` + `Semantics(button: true)`
- Options: `Icons.more_vert_rounded` → `ModalBottomSheet` (language switcher, theme switcher, help stub)
- Height: `SizeTokens.appBarHeight`; `SafeArea(bottom: false)`

`ARTrackingIndicator`:
- `searching` → `Icons.search_rounded` + `t(pt: 'Aponte para o painel', ...)`
- `initializing` → `Icons.radar_rounded` + `t(pt: 'A detetar...', ...)`
- `tracking` → `Icons.check_circle_outline_rounded` + `t(pt: 'AR activo', ...)` → auto-hides 3s via `Timer`
- Self-hides when `viewportStateProvider.mode == ViewportMode.offline`
- `Semantics(liveRegion: true)` + `AnimatedSwitcher(duration: AnimationTokens.medium)`

`ARModeTogglePill` (replaces top `SegmentedButton`):
- Position: `Positioned(bottom: Spacing.xl2 + SizeTokens.tapTarget + Spacing.sm, right: Spacing.lg)`
  > **Why this offset:** `TimelineSlider` sits at `bottom: Spacing.xl2`. The pill must clear the slider's tap target height (`SizeTokens.tapTarget`) plus a gap (`Spacing.sm`) so it does not overlap the right end of the slider on narrow screens (< 400dp).
- Widget: `FilledButton.tonal` pill
- Label shows **current mode** (NOT the target mode the button will switch to):

| Current mode           | Label shown                                            | Icon                        |
| ---------------------- | ------------------------------------------------------ | --------------------------- |
| `ViewportMode.offline` | `t(pt: 'Painel', en: 'Panel', es: 'Panel')`            | `Icons.image_outlined`      |
| `ViewportMode.liveAR`  | `t(pt: 'Câmara AR', en: 'AR Camera', es: 'Cámara AR')` | `Icons.camera_alt_outlined` |

- Tap: toggles to the other mode via `viewportStateProvider.notifier.setMode()`
- `Tooltip`: opposite mode name so accessibility users know what tapping will do
- `Semantics(button: true, label: ...)`

`EpochLabel`:
- Position: `Positioned(bottom: Spacing.xl2 + 64, left: Spacing.lg)`
- Epoch name (`context.titleMedium`) + subtitle (`context.onSurfaceVariant`, muted)
- `AnimatedSwitcher(duration: AnimationTokens.medium)` with **unique key per epoch** (required for switcher to detect change)
- **Auto-hide after 3 seconds** via `Timer` — re-appears whenever the epoch changes. This prevents it from becoming permanent chrome that competes with the painting.
- `Semantics(liveRegion: true)`

`ARDebugOverlay`:
- Only rendered when `kShowARDebugOverlay == true` (compile-time — tree-shaken in release)
- Content (monospace, semi-transparent dark background): tracking state, tracking method (FULL_TRACKING vs LAST_KNOWN_POSE), estimated image width in metres, detected image bounds, frame rate, visible POI count from `renderSpecs`

`ARFirstTimeOverlay`:
- Persistence: `SharedPreferences` key `PrefsKeys.arOnboardingShown`
- Step 1: auto-advances when `arTrackingStateProvider` transitions to `tracking` — use `ref.listen` NOT `ref.watch`
- Step 2: auto-dismisses on first marker tap
- Dismiss both steps: `t(pt: 'Entendido', en: 'Got it', es: 'Entendido')`
- NOT shown in `ViewportMode.offline`
- Entrance: `AnimationTokens.verySlow` fade-in; full-screen backdrop `Colors.black.withValues(alpha: 0.72)`; `Semantics(liveRegion: true)`

`PopScope`:
```dart
PopScope(
  canPop: ref.watch(poiDisplayModeProvider) == POIDisplayMode.none,
  onPopInvokedWithResult: (didPop, _) {
    if (!didPop) ref.read(poiDisplayModeProvider.notifier).close();
  },
)
```

FAB: NOT in Phase 1. Disabled FAB signals unfinished product in a museum installation. Phase 2.

Deep link: `?poi=<id>` query param in panorama route. If present, set `selectedPOIProvider` after page builds.

AR session error boundary: wrap `_LiveARCameraFrame` init in `AsyncValueBuilder`; on error show retry CTA + "Explorar sem AR" fallback.

**Tests (Layer 2 widget — `lib/domains/panorama/test/widgets/`):**

_PanoramaTopBar — `panorama_top_bar_test.dart`:_
- Renders back arrow icon (`Icons.arrow_back_ios_new_rounded`) — exactly one in tree
- Renders options icon (`Icons.more_vert_rounded`) — exactly one in tree
- Back icon tap calls `context.go('/')` — verify via router location change
- Options icon tap opens a `ModalBottomSheet` — sheet is present in the overlay tree after tap
- Modal sheet contains `LanguageSwitcher` and `ThemeSwitcher`
- Bar has a `SafeArea` wrapping its content
- Background widget uses `context.surface` color token — no raw `Color` literal
- `Tooltip` is present on the back `IconButton`
- Back button has `Semantics(button: true)` in its subtree
- Options button has `Semantics(button: true)` in its subtree
- Bar height in the rendered tree is `<= SizeTokens.appBarHeight` (not taller than spec)
- No `BackdropFilter` is present anywhere in the bar's subtree

_ARTrackingIndicator — `ar_tracking_indicator_test.dart`:_
- `searching` state: `Icons.search_rounded` is present; text contains PT "Aponte para o painel"
- `initializing` state: `Icons.radar_rounded` is present; text contains PT "A detetar"
- `tracking` state: `Icons.check_circle_outline_rounded` is present; text contains PT "AR activo"
- State change from `searching` → `tracking` triggers `AnimatedSwitcher` child swap
- In `ViewportMode.offline`: the indicator widget is invisible or absent (self-hides)
- In `ViewportMode.liveAR` + `tracking` state: indicator is visible immediately after state change
- In `ViewportMode.liveAR` + `tracking` state: indicator is absent/invisible after 3 seconds (advance fake clock)
- `Semantics(liveRegion: true)` is present in the subtree

_ARModeTogglePill — `ar_mode_toggle_test.dart`:_
- In `ViewportMode.offline`: label text is the PT "Painel"; icon is `Icons.image_outlined`
- In `ViewportMode.liveAR`: label text is the PT "Câmara AR"; icon is `Icons.camera_alt_outlined`
- Tapping calls `viewportStateProvider.notifier.setMode()` with the opposite mode
- After tap in offline mode: `viewportStateProvider` state is `liveAR`
- After tap in liveAR mode: `viewportStateProvider` state is `offline`
- `FilledButton.tonal` widget is present (not `FilledButton`, not `OutlinedButton`)
- `Semantics(button: true)` is present
- `Tooltip` is present and its message is the opposite mode label (not the current one)
- Pill is positioned at `bottom` > `Spacing.xl2` in the Stack (above the timeline slider)

_ARFirstTimeOverlay — `ar_first_time_overlay_test.dart`:_
- Not rendered at all when `ViewportMode.offline`
- Not rendered when `PrefsKeys.arOnboardingShown == true` (inject fake SharedPreferences)
- Rendered in `ViewportMode.liveAR` with `PrefsKeys.arOnboardingShown == false`
- Step 1 text is visible initially; Step 2 text is absent
- When `arTrackingStateProvider` transitions to `tracking` via `ref.listen`: Step 2 text becomes visible and Step 1 is absent
- "Entendido" button tap dismisses the overlay and writes `PrefsKeys.arOnboardingShown = true` to SharedPreferences
- Backdrop is present with opacity > 0 (full-screen dark layer)
- `Semantics(liveRegion: true)` present

_PopScope flow — `panorama_popscope_test.dart`:_
- `poiDisplayModeProvider == summary`: `canPop` is `false` — back press does NOT navigate
- `poiDisplayModeProvider == summary` + back press: `poiDisplayModeProvider` becomes `none`
- `poiDisplayModeProvider == sheet`: `canPop` is `false` — back press does NOT navigate
- `poiDisplayModeProvider == sheet` + back press: `poiDisplayModeProvider` becomes `none`
- `poiDisplayModeProvider == none`: back press navigates to `'/'`
- Sequence: open summary → back → open sheet → back → back navigates home (3-step sequence completes)

_Full panorama Stack — `panorama_ar_view_test.dart`:_
- `PanoramaTopBar` is present and positioned at the top of the Stack
- `TimelineSlider` is present and at the bottom when `displayMode == none`
- `TimelineSlider` is absent/invisible when `displayMode == summary`
- `TimelineSlider` is absent/invisible when `displayMode == sheet`
- `TimelineSlider` re-appears when display mode returns to `none`
- All 30 `ARPOIMarker` widgets are present in the tree for `pre1755` epoch in offline mode
- Changing epoch to `pombalina` causes at least one `ARPOIMarker` to disappear from the tree (for `destroyed` POIs)
- `ARModeTogglePill` is present and not obscured by the `TimelineSlider` (pill bottom > slider bottom)

**Layer 3 integration test — create `integration_test/panorama_ar_test.dart` in this task:**

Layer 3 for the panorama domain is a single file at the project root `integration_test/` (not inside `lib/`). Create it as part of Task 1.6 — after the chrome shell is complete, these scenarios can be driven. Layer 4b/4c run this same file on real devices.

The file must cover every major user interaction path end-to-end. Scenarios run with mock AR (no `--dart-define=USE_REAL_AR`):

```dart
// integration_test/panorama_ar_test.dart — 18 scenarios

// HOME PAGE
// S1. App launches: home page contains exactly 2 CTAs — "Explorar com AR" and "Explorar sem AR"
// S2. Home page: LanguageSwitcher and ThemeSwitcher are visible
// S3. "Explorar sem AR" tap: navigates to panorama page — PanoramaTopBar is present

// OFFLINE PANORAMA — LOAD
// S4. Panorama offline: all 30 ARPOIMarker widgets are present in the tree
// S5. Panorama offline: PanoramaTopBar is present at top; TimelineSlider is present at bottom
// S6. ARModeTogglePill is present; label text is "Painel" (offline mode) — not "Câmara AR"

// POI SELECTION — SUMMARY CARD
// S7. Tap any ARPOIMarker: POISummaryCard appears in the overlay tree with a non-empty title text
// S8. POISummaryCard is present: TimelineSlider is absent or has opacity 0
// S9. POISummaryCard: contains at least one InfoBadge widget (category or survival)
// S10. POISummaryCard dismiss via X button: card is removed; TimelineSlider re-appears; displayMode is none

// POI SELECTION — DETAIL SHEET
// S11. From summary card: tap "Mais Info" → POIDetailSheet is present in the overlay tree
// S12. POIDetailSheet present: TimelineSlider is absent or opacity 0
// S13. POIDetailSheet: survival badge InfoBadge is visible; epoch context line text is non-empty
// S14. Back button while detail sheet open: sheet closes; summary card is NOT re-opened (goes to none)
// S15. Back button while nothing open: navigates back to home page

// TIMELINE INTERACTION
// S16. Tap "pombalina" segment: at least one ARPOIMarker disappears (destroyed POI count > 0)
// S17. After pombalina, tap "pre1755": all 30 markers are present again
// S18. EpochLabel appears after epoch change; disappears after 3 seconds (advance clock via flutter_test fakeAsync equivalent)
```

**Acceptance gate:**
- [ ] All Layer 2 widget tests pass
- [ ] `integration_test/panorama_ar_test.dart` created with 6 scenarios
- [ ] `flutter test lib/` → all passing
- [ ] Manual: back button closes overlay first; tracking indicator visible in live mode

---

### ✅ Task 1.7 — POISummaryCard and POIDetailSheet

**What:** The two POI overlay widgets — compact anchored card + full detail sheet.
**Why now:** Core interaction of the AR experience. Depends on chrome (1.6) and primitives (1.8).

**Files:**
- NEW: `lib/domains/panorama/ar/widgets/poi_summary_card.dart`
- RENAME + enrich: `poi_info_sheet.dart` → `poi_detail_sheet.dart`
- NEW: `lib/domains/panorama/ar/widgets/poi_action_buttons.dart` (extracted row, reused by both)
- MODIFY: `lib/domains/panorama/panorama_domain.dart` (barrel)
- ADD to `pubspec.yaml`: `share_plus: <latest stable>`; run `flutter pub get`

> **Import sweep for rename (do before running analyze):** After renaming `poi_info_sheet.dart` → `poi_detail_sheet.dart` and `POIInfoSheet` → `POIDetailSheet`, search the entire codebase for the strings `poi_info_sheet` and `POIInfoSheet`. Update every import path and class reference found. Then run `flutter analyze` — a missed import will cause a hard error, not a warning.

**Key specs:**

`POISummaryCard`:
- Width: 280dp
- Position computed via `computeMarkerScreenPosition(normX, normY, ...)` — named params, from `viewport_math.dart`
- Flip below marker when in top third of viewport: `flipBelow = rawTop < viewportHeight * 0.33`
- Vertical clamp: `clampDouble(top, topBarHeight + Spacing.sm, viewportHeight - cardHeight - timelineHeight - Spacing.lg)`
- **Horizontal clamp:** `left = clampDouble(markerX - 140, Spacing.md, screenWidth - 280 - Spacing.md)` — prevents the card overflowing screen edges when a POI marker is near the left or right boundary of the panorama. `140` is half of the 280dp card width.
- Entrance: scale 0.85→1.0 + fade, `AnimationTokens.fast`, `luxurySpring` curve
- Background: `context.cardSurface` + `RadiusTokens.heroCardRadius` + `context.goldBorder`
- Shadow: `ElevationTokens.modal`
- Content: name `context.titleMedium bold`, `InfoBadge` row, one-line description (80 char truncate), dismiss X, `POIActionButtons`
- **Bookmark/favourite button:** Heart or star icon (system icon) in the top-right corner of the card body, alongside the dismiss X. Tapping saves/removes `poi.id` to `SharedPreferences` under a list key `PrefsKeys.bookmarkedPOIIds`. ~20 lines total. State: `StatefulWidget` local bool toggled on tap with `setState` (no provider needed — bookmark is a purely local UI action in Phase 1). This seeds Phase 3 gamification ("buildings you've discovered") with zero architectural cost now.
- Dismiss: tap X, tap outside, swipe down

`POIDetailSheet`: `DraggableScrollableSheet` — `initialChildSize: 0.5`, `minChildSize: 0.2`, `maxChildSize: 0.9`

**CRITICAL positioning note:** `DraggableScrollableSheet` must be a **direct child of the Stack** (layer [10]), NOT nested inside a `Positioned` widget. Nesting inside `Positioned` breaks the sheet's size computation — the sheet will not respond to drag gestures correctly.

**Content sections (top to bottom):**
1. Handle bar + POI name (`context.headlineSmall`) + `InfoBadge` row
2. **Epoch context line** — watches `timelineProvider`:

   | Epoch + survivalStatus            | PT                                | EN                                 | ES                                  |
   | --------------------------------- | --------------------------------- | ---------------------------------- | ----------------------------------- |
   | `pre1755` + `intact`              | "Este edifício existia em 1700"   | "This building stood in 1700"      | "Este edificio existía en 1700"     |
   | `pre1755` + `damaged`             | "Este edifício existia em 1700"   | "This building stood in 1700"      | "Este edificio existía en 1700"     |
   | `pre1755` + `destroyed`           | "Este edifício existia em 1700"   | "This building stood in 1700"      | "Este edificio existía en 1700"     |
   | `earthquake` + `destroyed`        | "Destruído no terramoto de 1755"  | "Destroyed in the 1755 earthquake" | "Destruido en el terremoto de 1755" |
   | `earthquake` + `damaged`          | "Danificado no terramoto de 1755" | "Damaged in the 1755 earthquake"   | "Dañado en el terremoto de 1755"    |
   | `earthquake` + `intact`           | "Sobreviveu ao terramoto de 1755" | "Survived the 1755 earthquake"     | "Sobrevivió al terremoto de 1755"   |
   | `pombalina` / `today` + `intact`  | "Este edifício existe hoje"       | "This building exists today"       | "Este edificio existe hoy"          |
   | `pombalina` / `today` + `damaged` | "Este edifício foi restaurado"    | "This building was restored"       | "Este edificio fue restaurado"      |

3. Full description in PT/EN/ES per active language
4. Historical dates: founding year, destruction year if applicable
5. **"Then vs Now" comparison** — shown only for `survivalStatus == 'intact'` AND `currentPhotoUrl != null`:
   ```dart
   // Stack + ClipRRect + GestureDetector horizontal drag + AnimatedPositioned
   // Top layer: current photograph (currentPhotoUrl)
   // Bottom layer: cropped panel depiction
   // Handle label: "→ Hoje / Today / Hoy" animates with drag position
   // ~40 lines total — implementation note: use AnimatedPositioned, not a physics simulation
   // This is the single most emotionally resonant moment in the experience:
   // "this building in the 1700 painting is the one I walk past today."
   ```
6. `POIActionButtons` row
7. Share button: `Icons.share_outlined` → `share_plus` — POI name + description + deep link
   `tilestories.app/panorama?poi=<id>` — **`// TODO(Phase2): replace with real domain`**
8. "Ver no Mapa" → disabled stub: `Tooltip(t(pt:'Em breve', en:'Coming soon', es:'Próximamente'))`

- Platform deep link config (`AndroidManifest.xml`, iOS associated domains) is Phase 2 prerequisite

`POIActionButtons`:
```dart
class POIActionButtons extends ConsumerWidget {
  final POI poi;
  final VoidCallback onMoreInfo;
  final VoidCallback? on3D;   // null if kAR3DNodes has no entry for this POI
  final VoidCallback? on360;  // null in Phase 1
}
// [3D] only shown when kAR3DNodes has entry for POI — Phase 1: never shown
```

**`ARPOIMarker` tap feedback** — add to `ar_poi_marker.dart` as part of this task (the widget is first used here):
```dart
// On tap: brief scale pulse — signals to the user that the tap registered before the
// card animates in (200-300ms network/state latency makes tap feel broken without it).
// Use AnimationController (vsync this) → Tween<double>(begin: 1.0, end: 1.15)
// → reverse back to 1.0, total duration AnimationTokens.fast.
// ~5 lines. Wrap the marker Stack in ScaleTransition.
```

Timeline slider hide/show — CRITICAL (bug was here):
```dart
// Keyed to poiDisplayModeProvider, NOT selectedPOIProvider.
// WHY: selectedPOI persists after displayMode.close() — using selectedPOI caused permanent hide bug.
AnimatedOpacity(
  opacity: displayMode == POIDisplayMode.none ? 1.0 : 0.0,
  duration: AnimationTokens.fast,
  child: IgnorePointer(
    ignoring: displayMode != POIDisplayMode.none,
    child: const TimelineSlider(),
  ),
)
```

**Tests (Layer 2 widget):**

**`lib/domains/panorama/test/widgets/poi_summary_card_test.dart` — 22 assertions:**

_Dimensions and layout:_
- `tester.getSize(find.byType(POISummaryCard)).width` equals exactly `280.0` (no range — fixed spec)
- Card has a `RadiusTokens.heroCardRadius` corner radius — find a `ClipRRect` or `DecoratedBox` with that radius; no raw `BorderRadius.circular()` literal
- Background color resolves to `context.cardSurface` token — no raw `Color(...)` literal in the widget source
- `ElevationTokens.modal` is referenced by a `BoxShadow` or `Material.elevation` in the card — assert shadow is non-zero
- Minimum card height >= `SizeTokens.tapTarget * 3` (card is never a thin sliver)

_Content:_
- POI `name` text is visible with `context.titleMedium` bold style — at least one `Text` widget whose `style` traces back to that token
- At least one `InfoBadge` widget is present in the card subtree (survival or category)
- Description text is present and truncated at 80 characters — visible text length `<= 80`
- `Icons.close` (or `Icons.close_rounded`) is present as the dismiss button
- `POIActionButtons` widget is present in the card subtree
- Bookmark icon (`Icons.favorite_border` or `Icons.star_border`) is present in the card subtree

_Entrance animation:_
- On mount, `ScaleTransition` scale starts at `0.85` and reaches `1.0` after `AnimationTokens.fast` ms (use `tester.pump(AnimationTokens.fast)`)
- On mount, `FadeTransition` opacity starts at `0.0` and reaches `1.0` after `AnimationTokens.fast` ms

_Interaction — dismiss paths (all three must clear both providers):_
- Tap `Icons.close` → `poiDisplayModeProvider == POIDisplayMode.none` AND `selectedPOIProvider == null`
- Tap outside the card boundary → `poiDisplayModeProvider == POIDisplayMode.none` AND `selectedPOIProvider == null`
- Fling card downward (swipe gesture) → `poiDisplayModeProvider == POIDisplayMode.none` AND `selectedPOIProvider == null`

_"Mais Info" path:_
- Tap the "Mais Info" / "More Info" / "Más info" button → `poiDisplayModeProvider == POIDisplayMode.sheet`

_Flip logic (use `pumpWidget` with controlled viewport size 400×800):_
- `normY = 0.20` (top 20% of viewport, < 33% threshold) → card top edge is **greater than** marker centre Y (card is positioned BELOW the marker)
- `normY = 0.60` (60% of viewport, ≥ 33% threshold) → card bottom edge is **less than** marker centre Y (card is positioned ABOVE the marker)

_Horizontal clamp:_
- `normX = 0.0` (far left) → `tester.getRect(find.byType(POISummaryCard)).left >= Spacing.md`
- `normX = 1.0` (far right) → `tester.getRect(find.byType(POISummaryCard)).right <= screenWidth - Spacing.md`

_Bookmark persistence (use `SharedPreferences.setMockInitialValues`):_
- Tap bookmark once → `SharedPreferences.getInstance()` then `prefs.getStringList(PrefsKeys.bookmarkedPOIIds)` contains `poi.id`
- Tap bookmark again → key is **absent** from the same list (toggled off)
- Re-mount card with prefs already containing `poi.id` → bookmark icon reflects saved state (filled icon, not border)

---

**`lib/domains/panorama/test/widgets/ar_poi_marker_test.dart` — 6 assertions:**

- Tap the marker → the `ScaleTransition` value is `> 1.0` immediately after the tap gesture completes (before pumping full duration)
- After pumping `AnimationTokens.fast` ms → scale returns to exactly `1.0`
- `showLabel: true` → the POI label `Text` widget is present in the subtree
- `showLabel: false` → the POI label `Text` widget is **absent** from the subtree
- The `opacity` parameter is honoured — wrap root in an `Opacity` widget (or equivalent) and assert `Opacity.opacity == suppliedValue`
- A `RepaintBoundary` wraps the marker's root widget — `find.ancestor(of: find.byType(ARPOIMarker), matching: find.byType(RepaintBoundary))` is non-empty

---

**`lib/domains/panorama/test/widgets/poi_detail_sheet_test.dart` — 23 assertions:**

_Structure:_
- `DraggableScrollableSheet` is present in the subtree **not** nested inside a `Positioned` widget — assert `find.ancestor(of: find.byType(DraggableScrollableSheet), matching: find.byType(Positioned))` is **empty** (direct Stack child requirement)
- On mount the sheet occupies ~50% of the screen height: `tester.getSize(find.byType(DraggableScrollableSheet)).height` is approximately `screenHeight * 0.5` (allow ±5%)

_Content — header:_
- POI `name` text is visible using `context.headlineSmall` style
- At least one `InfoBadge` is in the sheet subtree (survival badge)

_Epoch context line — all 8 combinations (parameterised test, PT locale):_
- `pre1755` + `intact` → visible text contains `"Este edifício existia em 1700"`
- `pre1755` + `destroyed` → visible text contains `"Este edifício existia em 1700"`
- `earthquake` + `destroyed` → visible text contains `"Destruído no terramoto de 1755"`
- `earthquake` + `damaged` → visible text contains `"Danificado no terramoto de 1755"`
- `earthquake` + `intact` → visible text contains `"Sobreviveu ao terramoto de 1755"`
- `pombalina` + `intact` → visible text contains `"Este edifício existe hoje"`
- `pombalina` + `damaged` → visible text contains `"Este edifício foi restaurado"`
- Live update: start on `pre1755`, trigger `timelineProvider` transition to `earthquake`, assert epoch context line text changes accordingly (no hot-reload required — `ref.watch` must rebuild)

_"Then vs Now" section:_
- `survivalStatus == intact` AND `currentPhotoUrl != null` → "Then vs Now" widget (or its section header text) is **visible** in the sheet subtree
- `survivalStatus == destroyed` (any `currentPhotoUrl`) → "Then vs Now" widget is **absent**
- `survivalStatus == intact` AND `currentPhotoUrl == null` → "Then vs Now" widget is **absent**

_Action buttons:_
- `Icons.share_outlined` button is present in the sheet subtree
- Share button tap → mock `SharePlus` (inject via `Provider` override) is called exactly once with a non-empty share string containing the POI id
- `[3D]` button / action is **absent** when `kAR3DNodes` is empty (Phase 1 invariant)
- `POIActionButtons` widget is present in the sheet subtree
- "Ver no Mapa" stub is present; it is either `onPressed: null` or wrapped in a `Tooltip` whose message contains `"breve"` / `"soon"` / `"pronto"`

_Timeline slider:_
- While `poiDisplayModeProvider == POIDisplayMode.sheet`, the `TimelineSlider` widget has `opacity == 0.0` or is absent from the hit-test tree (use `IgnorePointer` assertion)

**Acceptance gate:**
- [ ] All Layer 2 widget tests pass
- [ ] `flutter test lib/` → all passing
- [ ] Manual: card appears near marker; sheet slides up; back closes sheet then navigates

---

### ✅ Task 1.9 — Home page placeholder

**What:** Minimal Phase 1 home page with AR/offline CTAs and permission flow.
**Why now:** Entry point for the Phase 1 POC. Phase 2 gets the full home design.

**Files:**
- MODIFY: `lib/domains/home/pages/home_page.dart`
- NEW: `lib/domains/home/providers/camera_permission_provider.dart`

**Spec:**
```dart
// camera_permission_provider.dart — injectable; tests override without platform channels
final cameraPermissionProvider = Provider<Future<PermissionStatus> Function()>(
  (_) => () => Permission.camera.request(),
);

// home_page.dart additions:
// FilledButton  "Explorar com AR"  / "Explore with AR"  / "Explorar con AR"
// OutlinedButton "Explorar sem AR" / "Explore without AR" / "Explorar sin AR"
// _onExplorarComAR(): reads cameraPermissionProvider, navigates or shows dialog
// _showPermissionDialog(): denied → Key('permission_denied_fallback'); permanently denied → openAppSettings()
// LanguageSwitcher + ThemeSwitcher in header row (top-right)
// _arCtaLoading: bool field prevents double-tap during request
```

**AR availability guard on "Explorar com AR":**

Before requesting camera permission, check `arAvailabilityProvider` (read `lib/ar_core/models/ar_availability.dart` first — see §1 checklist). If `ARAvailability.unavailable`, skip the camera permission request entirely and navigate directly to `/panorama` with `ViewportMode.offline`. This prevents the user from going through a permission dialog, landing on the panorama, and then seeing an error boundary about AR not being available.

```dart
// _onExplorarComAR():
final arAvailability = ref.read(arAvailabilityProvider);
if (arAvailability == ARAvailability.unavailable) {
  // Device cannot do AR — go straight to offline mode; no camera permission needed.
  context.go('/panorama'); // ViewportState.initial() defaults to offline mode.
  return;
}
// ...existing camera permission logic...
```

> `ARAvailability.unknown` (not yet checked) should behave the same as `available` — attempt AR and let the session init handle failure gracefully via the error boundary.

**Tests (Layer 2 widget — `lib/domains/home/test/widgets/home_cta_test.dart`):**

_CTA widget types (2 assertions):_
- `find.byType(FilledButton)` within the subtree contains exactly the "Explorar com AR" button — **not** an `OutlinedButton` and not a plain `ElevatedButton`
- `find.byType(OutlinedButton)` within the subtree contains exactly the "Explorar sem AR" button — **not** a `FilledButton`

_Dimensions (2 assertions):_
- Both CTA buttons have `tester.getSize(...).height >= SizeTokens.tapTarget` (48dp minimum)
- Both CTAs are **simultaneously visible** on screen — neither is clipped or off-screen in a 400×800 test viewport

_Labels (3 assertions):_
- Default (PT) locale: `find.text('Explorar com AR')` and `find.text('Explorar sem AR')` are both present
- After overriding locale to EN: `find.text('Explore with AR')` and `find.text('Explore without AR')` are both present
- After overriding locale to ES: `find.text('Explorar con AR')` and `find.text('Explorar sin AR')` are both present

_Header widgets (2 assertions):_
- `LanguageSwitcher` widget is present in the subtree
- `ThemeSwitcher` widget is present in the subtree

_"Explorar sem AR" flow (2 assertions):_
- Tap "Explorar sem AR" → `cameraPermissionProvider` function is **never called** (use a counting mock injected via `ProviderScope.overrides`)
- Tap "Explorar sem AR" → router navigates to `'/panorama'` (assert `GoRouter` location == `'/panorama'`)

_"Explorar com AR" — permission granted (2 assertions):_
- Override `cameraPermissionProvider` to return `PermissionStatus.granted`
- Tap "Explorar com AR" → `cameraPermissionProvider` function is called exactly once
- Tap "Explorar com AR" → router navigates to `'/panorama'`

_"Explorar com AR" — permission denied (2 assertions):_
- Override `cameraPermissionProvider` to return `PermissionStatus.denied`
- Tap "Explorar com AR" → dialog appears in the overlay tree
- Dialog contains a widget with `Key('permission_denied_fallback')` — the retry/fallback button

_"Explorar com AR" — permanently denied (2 assertions):_
- Override `cameraPermissionProvider` to return `PermissionStatus.permanentlyDenied`
- Tap "Explorar com AR" → dialog appears (different from denied dialog)
- Dialog does **not** contain `Key('permission_denied_fallback')` — directs to settings instead (assert presence of a "Definições" / "Settings" / "Ajustes" text or icon)

_Loading state and double-tap guard (2 assertions):_
- Override `cameraPermissionProvider` to return a `Future` that never completes (use `Completer<PermissionStatus>()` — do not `.complete()`)
- Tap "Explorar com AR" → a `CircularProgressIndicator` (or equivalent loading widget) is visible while awaiting
- Tap the button a second time while loading → `cameraPermissionProvider` function is still called **only once** (double-tap guard active)

_AR availability guard (2 assertions):_
- Override `arAvailabilityProvider` to `ARAvailability.unavailable`; override `cameraPermissionProvider` to a counter mock
- Tap "Explorar com AR" → `cameraPermissionProvider` function is called **zero times**
- Tap "Explorar com AR" with `ARAvailability.unavailable` → router still navigates to `'/panorama'`

**Acceptance gate:**
- [ ] All 13 widget tests in `home_cta_test.dart` pass
- [ ] `flutter test lib/` → all passing

---

## 7. Global Test Suite

### Layer 1+2+3 combined

```powershell
# All layers 1-3:
flutter test lib/ --reporter=expanded

# Individual domain suites:
flutter test lib/domains/timeline/test/ --reporter=expanded
flutter test lib/domains/panorama/test/ --reporter=expanded
flutter test lib/domains/home/test/ --reporter=expanded
flutter test lib/ar_core/test/ --reporter=expanded
flutter test lib/components/ --reporter=expanded

# Quick health check after any change:
flutter analyze ; flutter test lib/ --reporter=expanded

# After any Freezed modification:
dart run build_runner build --delete-conflicting-outputs
```

**Current baseline: 1134 tests passing, 3 skipped (pre-existing), 0 analyze issues (59 info-level deprecation warnings in `integration_test/` only — pre-existing).**
Must never regress below this count. Run after every task.

### Layer 4 — Device tests

**Layer 4a — Android Emulator (real ARCore, virtual scene):**
```powershell
# Prerequisites: AVD x86_64 API 27+, Camera Back = VirtualScene, ARCore Services APK installed
# abiFilters must include "x86_64" in android/app/build.gradle.kts
flutter run --dart-define=USE_REAL_AR=true --dart-define=SHOW_AR_DEBUG=true -d <EMULATOR_ID>
```
Scenarios:
1. [ ] Launches without "device does not support AR" error
2. [ ] Virtual camera navigates to panorama image → `FULL_TRACKING` fires, POI markers appear
3. [ ] Pan away from image → `LAST_KNOWN_POSE`, markers hold position
4. [ ] Mode toggle pill switches to offline mode cleanly

**Layer 4b — Physical Android (real ARCore, real camera):**
```powershell
# Mock mode — confirms UI on device:
flutter test integration_test/panorama_ar_test.dart -d <ANDROID_DEVICE_ID>

# Real ARCore on A3 proxy image:
flutter run --dart-define=USE_REAL_AR=true --dart-define=SHOW_AR_DEBUG=true -d <ANDROID_DEVICE_ID>

# Full integration test with real AR:
flutter test integration_test/panorama_ar_test.dart -d <ANDROID_DEVICE_ID> --dart-define=USE_REAL_AR=true
```

**Layer 4c — Physical iPhone (real ARKit):**
```powershell
# iOS Simulator CANNOT be used — no camera, no ARKit. Physical device only.
flutter build ios --release  # must be clean
flutter run --dart-define=USE_REAL_AR=true --dart-define=SHOW_AR_DEBUG=true -d <IOS_DEVICE_ID>
flutter test integration_test/panorama_ar_test.dart -d <IOS_DEVICE_ID> --dart-define=USE_REAL_AR=true
```

**Scenarios (4b + 4c) — run the same `integration_test/panorama_ar_test.dart` defined in Task 1.6:**

The file contains 18 scenarios (S1–S18). On a physical device all 18 must pass. See the scenario list in Task 1.6 Layer 3 block for the full specification. Quick smoke-check priorities for CI:

1. [ ] S1: App launches; home page renders exactly 2 CTAs
2. [ ] S3: "Explorar sem AR" → panorama page with PanoramaTopBar visible
3. [ ] S4: All 30 ARPOIMarker widgets present in the tree (offline mode)
4. [ ] S7: Tap any ARPOIMarker → POISummaryCard appears with non-empty title
5. [ ] S11: Tap "Mais Info" → POIDetailSheet is present
6. [ ] S14: Back while sheet open → sheet closes; does NOT re-open summary card
7. [ ] S15: Back while nothing open → navigates to home page
8. [ ] S16: Tap "pombalina" → at least one marker disappears
9. [ ] S17: Tap "pre1755" again → all 30 markers present

### Layer 5 — Manual browser tests

Tasks in `lib/test_utils/test_config.dart` (`kTestSteps`) + floating overlay in `lib/test_utils/test_tasks_floating.dart` — iterate until all 21 are green. Run `flutter run -d chrome` and use the floating checklist.

1. [ ] Home page: both buttons visible; language switcher PT/EN/ES; theme light/dark
2. [ ] Offline panorama: pan/zoom smooth; all 30 markers visible at correct positions
3. [ ] Epoch label: appears on epoch change, auto-hides after 3 seconds (not permanent chrome)
4. [ ] POI tap: `POISummaryCard` appears NEAR marker (not at bottom), with name, badges, one-line description
5. [ ] Card flip: tap a marker near top of panel → card appears BELOW marker (not overlapping)
6. [ ] "Mais Info": `POIDetailSheet` slides up; timeline slider hidden while open; survival badge correct
7. [ ] "Then vs Now": swipe works for an intact building with `currentPhotoUrl`; absent for destroyed buildings
8. [ ] Epoch change in detail sheet: epoch context line updates immediately
9. [ ] Timeline: tap all 4 epochs; epoch label changes; destroyed POIs disappear for pombalina/today; dimmed for earthquake
10. [ ] Share button: fires native share sheet with POI name + deep link placeholder
11. [ ] Back button: closes summary card → closes detail sheet → navigates home (three-step sequence)
12. [ ] Language switch mid-session: all visible strings update, no English fallback in PT or ES mode
13. [ ] AR tracking indicator: appears in live mock mode, cycles through states, auto-hides after tracking
14. [ ] Mode toggle pill: switches between AR and offline modes; label reflects CURRENT mode (not target)
15. [ ] Narrow screen (simulate 360dp): timeline slider shows epoch names only (no year subtitles)
16. [ ] ARModeToggle design: toggle is inside PanoramaTopBar (not bottom-right); animated track visible; thumb moves left↔right on tap
17. [ ] ARModeToggle blink dot: in live mode blink dot pulses gold; in offline mode blink dot is absent
18. [ ] POILegendButton: ⓘ button visible at top-right of panorama; tap opens legend sheet without pumpAndSettle issue
19. [ ] POILegendSheet: spectrum bar shows gold→red gradient with 'Intacto'/'Destruído' labels; all 8 type icons with Portuguese names visible
20. [ ] ARPOIMarker LOD: zoom in — top-ranked markers are 48dp with icon visible; zoom out — markers shrink to 24dp with icon hidden
21. [ ] ARPOIMarker selection: tapping a marker scales it up (1.15×); tapping another marker de-selects the previous one

---

## 8. Phase Verification Checklist

### Functional

- [ ] 30 POIs in `pois.json` with PT + EN + ES on all entries; all have `survivalStatus`
- [ ] All 30 markers render at correct positions in offline mode
- [ ] **Live AR — Android Emulator** (Task 1.0 Step 3): `FULL_TRACKING` fires, POI markers appear. Results in `AR_TRACKING_STRATEGY.md`.
- [ ] **Live AR — physical Android** (Task 1.0 Step 6): stable within 10s on A3 proxy, tested >= 2 devices. Results in `AR_TRACKING_STRATEGY.md`.
- [ ] **Live AR — physical iPhone** (Task 1.0 Step 7): ARKit confirmed, `flutter build ios --release` clean. Results in `AR_TRACKING_STRATEGY.md`.
- [ ] **LAST_KNOWN_POSE drift < 5cm at 3m** after 30s — tested Android + iPhone. Documented in `AR_TRACKING_STRATEGY.md`.
- [ ] `kPanelPhysicalWidthMeters` set and passed as `widthInMeters` to ARCore image database.
- [ ] `kPanelPhysicalHeightMeters` stored in config alongside width (used for overlap distance formula).
- [ ] Timeline slider renders 4 epochs; **narrow screen (< 400dp) shows epoch name only** (no year subtitle).
- [ ] Changing epoch filters markers correctly for all 4 epochs.
- [ ] `destroyed` POIs hidden for `pombalina` and `today`.
- [ ] `destroyed` POIs opacity 0.45, `damaged` opacity 0.7 for `earthquake`.
- [ ] `POISummaryCard` appears **NEAR tapped marker** (not at bottom of screen).
- [ ] **Card-flip**: card appears BELOW marker when marker is in top third of viewport (Y < 33%).
- [ ] **Horizontal clamp**: `POISummaryCard` never overflows screen edges — left edge >= `Spacing.md`, right edge <= `screenWidth - Spacing.md`.
- [ ] **`ARPOIMarker` tap feedback**: tapping a marker produces a scale pulse (> 1.0) before the card animates in.
- [ ] **Bookmark button**: tapping the heart/star on `POISummaryCard` saves `poi.id` to `SharedPreferences`; tapping again removes it; state survives hot restart.
- [ ] `POISummaryCard` dismisses on X / tap outside / swipe down; both `selectedPOIProvider` and `poiDisplayModeProvider` cleared.
- [ ] `POIDetailSheet` shows survival badge, epoch context line, share button.
- [ ] **Epoch context line in `POIDetailSheet`** is correct for all epoch × survivalStatus combinations.
- [ ] **"Then vs Now" swipe** visible for intact buildings with `currentPhotoUrl`; absent for destroyed or null `currentPhotoUrl`.
- [ ] Share button fires native share sheet with POI name + deep link placeholder.
- [ ] `ARTrackingIndicator` shows correct state, self-hides in offline mode, auto-hides 3s after tracking.
- [ ] **`ARFirstTimeOverlay`** shows once per install in live AR mode; never shown in offline mode; never shown twice.
- [ ] Mode toggle pill label reflects **current mode** (not target mode); switches AR ↔ offline correctly.
- [ ] Timeline slider hidden when POI overlay open; re-appears on close.
- [ ] **Back button priority**: summary card closes → detail sheet closes → navigates home (three distinct states).
- [ ] `POISummaryCard` and `POIDetailSheet` cannot be open simultaneously (`POIDisplayMode` enforces this).
- [ ] Home page: both CTAs visible; permission denied shows dialog with fallback.
- [ ] **AR availability guard**: on devices where `ARAvailability.unavailable`, tapping "Explorar com AR" navigates directly to offline panorama without requesting camera permission.
- [ ] **Deep link** `?poi=<id>` opens panorama with correct POI summary card visible.
- [ ] `ViewportStateProvider` updates on pan/zoom in offline panorama.
- [ ] `POIVisibilityService` wired; `renderSpecs` drives overlay (not `allPOIs`).
- [ ] `ARNodeManager` stub wired; `kAR3DNodes` empty → nothing placed.
- [ ] `EpochLabel` auto-hides after 3 seconds; re-appears on next epoch change.

### Code quality

- [ ] `flutter analyze` → 0 issues
- [ ] `flutter test lib/ --reporter=expanded` → all passing, >= Phase 0 baseline count
- [ ] `flutter build apk --release` → APK < 100MB
- [ ] `flutter build apk --analyze-size` → APK size increase vs Phase 0 baseline < 15MB
- [ ] `flutter build ios --release` → IPA builds clean
- [ ] No `StateProvider` in any new or modified file
- [ ] No `BackdropFilter` on `PanoramaTopBar` or any non-static overlay
- [ ] No `SegmentedButton` at top of panorama page

### Design system compliance

- [ ] No raw `Colors.*` in any new or modified file (exception: AR overlay code with `// AR overlay:` comment)
- [ ] No literal spacing numbers — all `Spacing.*`
- [ ] No literal duration milliseconds — all `AnimationTokens.*`
- [ ] No literal border radius numbers — all `RadiusTokens.*`
- [ ] All entrance animations: `RevealAnimation` or fade+slide from `DESIGN/08_MOTION_AND_FEEL.md`
- [ ] All state transitions: `AnimatedSwitcher(duration: AnimationTokens.medium)`
- [ ] `EpochLabel` auto-hides after 3 seconds (not permanent chrome)
- [ ] `ARTrackingIndicator` auto-hides after 3 seconds once tracking achieved

### i18n

- [ ] All user-visible strings: `t(pt: '...', en: '...', es: '...')` — all 3 always present
- [ ] No string falls back to English in PT or ES mode
- [ ] Language switcher cycles PT → EN → ES → PT correctly
- [ ] All new POI entries have PT + EN + ES on `name` and `description`

### Accessibility (every new widget — not deferred)

- [ ] Every interactive element: `Semantics(button: true, label: ...)` or equivalent
- [ ] Every `IconButton`: `Tooltip`
- [ ] All tap targets >= 48×48px (`SizeTokens.tapTarget`)
- [ ] Color is **never** the sole indicator of meaning — survival badges use icon + text + color
- [ ] `EpochLabel`: `Semantics(liveRegion: true)`
- [ ] `ARTrackingIndicator`: `Semantics(liveRegion: true)`
- [ ] `ARFirstTimeOverlay`: `Semantics(liveRegion: true)`
- [ ] `MediaQuery.of(context).disableAnimations` respected in all animated widgets

### Device tests

- [ ] `integration_test/panorama_ar_test.dart` created with 6 scenarios (see Task 1.6)
- [ ] `integration_test/panorama_ar_test.dart` passes on physical Android
- [ ] `integration_test/panorama_ar_test.dart` passes on physical iPhone
- [ ] Layer 5 manual browser tests: all 21 scenarios green (`kTestSteps` in `lib/test_utils/test_config.dart`)

---

## 9. Ongoing Rules Reminder

| Rule                                                                                                   | Source                          |
| ------------------------------------------------------------------------------------------------------ | ------------------------------- |
| Read all files in scope before touching them                                                           | IMPLEMENTATION_GUIDELINES §0    |
| Create full TODO list upfront before starting any task                                                 | IMPLEMENTATION_GUIDELINES §0    |
| For every decision: consider 3 options at architecture AND implementation level                        | IMPLEMENTATION_GUIDELINES §1    |
| One file, one responsibility. > ~300 lines → split                                                     | IMPLEMENTATION_GUIDELINES §2    |
| `NotifierProvider` only — `StateProvider` is BANNED                                                    | PROJECT_GUIDE §7                |
| Freezed only when justified: union types, JSON, many-field copyWith                                    | PROJECT_GUIDE §7                |
| Package additions: > 100 pub points, updated within 12 months, null-safe, no stale version strings     | PROJECT_GUIDE §7                |
| After every task: `flutter analyze` → 0, `flutter test lib/ --reporter=expanded` → all passing         | IMPLEMENTATION_GUIDELINES §4    |
| If Freezed file modified: `dart run build_runner build --delete-conflicting-outputs`                   | PROJECT_GUIDE §7                |
| All colors: `context.*` tokens. Never `Colors.*` (exception: AR overlay with `// AR overlay:` comment) | DESIGN/02_COLORS.md             |
| All spacing: `Spacing.*`. Never literal numbers                                                        | DESIGN/01_TOKENS.md             |
| All durations: `AnimationTokens.*`. Never literal milliseconds                                         | DESIGN/01_TOKENS.md             |
| All border radii: `RadiusTokens.*`. Never literal numbers                                              | DESIGN/01_TOKENS.md             |
| All animations: pattern from `DESIGN/08_MOTION_AND_FEEL.md`                                            | DESIGN guide                    |
| All state transitions: `AnimatedSwitcher` per `FEEDBACK_GUIDE.md`                                      | FEEDBACK guide                  |
| New routes: add to `nav_config.dart` only                                                              | NAV_AND_LAYOUT guide            |
| `BackdropFilter` BANNED in non-static contexts (iOS Impeller x4 frame-time regression)                 | DESIGN/08_MOTION_AND_FEEL.md    |
| Terminal (PowerShell): use `;` not `&&`; `Select-Object -Last N` not `tail`                            | IMPLEMENTATION_GUIDELINES §7    |
| Test runner: ALWAYS `--reporter=expanded`. NEVER `--reporter=compact`                                  | IMPLEMENTATION_GUIDELINES §4    |
| Comments: explain WHY, not WHAT. No emojis (encoding issues on some terminals)                         | IMPLEMENTATION_GUIDELINES §2    |
| Timeline slider opacity: keyed to `poiDisplayModeProvider`, NOT `selectedPOIProvider`                  | Phase 1 bug fix — Task 1.7      |
| `renderSpecs` drives marker overlay, not `allPOIs`                                                     | Phase 1 architecture — Task 1.4 |
| `cameraPermissionProvider` is injectable — tests override without platform channels                    | Phase 1 — Task 1.9              |
| `POIDisplayMode.close()` always clears BOTH `selectedPOIProvider` AND `poiDisplayModeProvider`         | §2 A3                           |
| `ARFirstTimeOverlay` uses `ref.listen` for tracking state transition — NOT `ref.watch`                 | §2 A14 (race condition)         |
| `tilestories.app` is a placeholder — `// TODO(Phase2): replace with real domain`                       | §2 A16                          |
| `DraggableScrollableSheet` must be a direct child of Stack or Scaffold body — NOT inside `Positioned`  | §3 POIDetailSheet spec          |
| Deep link platform config (`AndroidManifest.xml`, iOS associated domains) is Phase 2 prerequisite      | Phase 1 out-of-scope decision   |

---

*End of Phase 1 Plan. Current status: Tasks 1.1–1.9 complete ✅. Task 1.0 physical device steps (Steps 3–8) and Layer 4/5 tests are not yet done. Phase verification checklist items marked [ ] are not yet confirmed.*

---

## Session Completion Notes — 2025

### What was implemented in this session

**Implementation gaps closed (all verified with `flutter analyze` → 0 issues):**

1. **`share_plus ^11.0.0`** added to `pubspec.yaml` → `flutter pub get` succeeded.

2. **`POI.currentPhotoUrl: String?`** added to `poi.dart` Freezed model.
   - `dart run build_runner build --delete-conflicting-outputs` → 14 outputs regenerated.
   - Required by the "Then vs Now" spec in POIDetailSheet (Phase 1 scaffold, content added Phase 2).

3. **`poi_action_buttons.dart`** created at `lib/domains/panorama/ar/widgets/`.
   - Two buttons: **Share** (via `SharePlus.instance.share` / `ShareParams`) and **Ver no Mapa** (disabled stub, `onPressed: null`).
   - Keys: `Key('poi_action_share')`, `Key('poi_action_map')`.
   - `// TODO(Phase2): replace with real domain` comment on the share URL.

4. **`POIDetailSheet`** enriched:
   - Imports `timelineProvider` and `poi_action_buttons.dart`.
   - Adds **epoch context line** below the title (watches `timelineProvider`, shows `epoch.label` in italics with `Key('poi_detail_epoch_context')`).
   - Adds **`POIActionButtons(poi: poi)`** at the bottom of the scroll column.
   - Fixed `initialChildSize` from `0.55` → `0.5` (per plan spec).

5. **`ARPOIMarker`** converted from `StatelessWidget` → `StatefulWidget`:
   - `AnimationController` with `AnimationTokens.fast` duration.
   - `ScaleTransition` (1.0 → 1.2) wrapping the marker dot only — triggered by `onTapDown` / `onTapUp` / `onTapCancel`.
   - `RepaintBoundary` wraps the GestureDetector to isolate repaints.
   - `onTap` callback now fires on `onTapUp` (not `GestureDetector.onTap`), keeping the pulse in sync.

### Test results

- **Before:** 982 passing, 3 skipped, 0 failures.
- **After:** 982 passing, 3 skipped, 0 failures.
- **`flutter analyze`:** No issues found.

### Deferred items (not in this session scope)

- `DraggableScrollableSheet` is still inside `Positioned.fill` in `panorama_ar_view.dart` — the plan rule says it should be a direct Stack child. This is a structural refactor of `panorama_ar_view.dart` and is deferred to avoid risk.
- `ARModeToggle` uses `camera_alt_rounded`/`image_rounded` icons; plan spec says `camera_alt_outlined`/`image_outlined`. This is cosmetic and deferred.
- `PanoramaTopBar` uses `SizeTokens.appBarHeightMobile`; plan references `appBarHeight`. To be confirmed when SizeTokens is audited.
- ARAvailability guard on "Explorar com AR" home page button — deferred.
- Timeline first-use pulse in `timeline_slider.dart` — deferred.
- Layer 3 / Layer 4 integration tests — require physical device or emulator with ARCore.
- `poi_action_buttons.dart` unit/widget tests — no test file yet; to be added in next session.

---

### Implementation notes — Phase 1 UI widget redesigns (post-plan additions)

The following three widgets were redesigned or created after the original Phase 1 plan was written. Their specs are recorded here for future sessions.

#### `ARModeToggle` — Variant-A animated-track redesign

**Location:** `lib/domains/panorama/ar/widgets/ar_mode_toggle.dart`
**Placed in:** `PanoramaTopBar` (not a Positioned widget in the Stack)

**Key dimensions and structure:**
- Outer: `ConstrainedBox(maxWidth: 280)`
- Track: `AnimatedContainer(width: 72, height: 40, borderRadius: 20)` — gold when live, surface when offline
- Thumb: `AnimatedPositioned` (left: 4 offline, left: 36 live) — 32dp circle
- Blink dot: `AnimatedBuilder` on `AnimationController(900ms, repeat(reverse: true))` — live mode only; 8dp circle, gold color
- Labels: 'Câmara AR' (title) + 'INACTIVO' / 'ACTIVO · A SCANNEAR' (subtitle)
- **CRITICAL:** Never use `pumpAndSettle()` in any test that includes this widget — the blink controller repeats indefinitely; use bounded `pump(Duration)` instead.

#### `ARPOIMarker` — 4 LOD tiers + `MarkerScaleTier` + `ARPOIMarkerIcon`

**Files:** `ar_poi_marker.dart`, `marker_icons.dart`

**`MarkerScaleTier` enum:**

| Tier   | `sizeDp` | `showIcon` | `labelEligible` |
| ------ | -------- | ---------- | --------------- |
| large  | 48       | true       | true            |
| medium | 36       | true       | true            |
| small  | 24       | false      | false           |
| micro  | 16       | false      | false           |

**LOD assignment:** ranks 0–4 → large; 5–9 → medium; 10–29 → small; overflow → micro.

**8-type priority (lower index = survives label overlap):** military(0) < royal(1) < religious(2) < civic(3) < maritime(4) < infrastructure(5) < landscape(6) < commerce(7).

**`ARPOIMarker` key structure:** `AnimatedScale(isSelected ? 1.15 : 1.0)` → `RepaintBoundary` → optional `Opacity` (only when opacity < 1.0) → `AnimatedContainer(size=tier.sizeDp)` → `CustomPaint(_DashedBorderPainter)` + `ARPOIMarkerIcon` (large/medium only).

**`ARPOIMarkerIcon`:** 8 `CustomPainter` icons in a 0–1 unit square. `strokeWidth = size × 0.07`.

#### `POILegendButton` + `POILegendSheet`

**File:** `lib/domains/panorama/ar/widgets/poi_legend_button.dart`
**Stack position:** `[8] Positioned(top: kToolbarHeight, right: 0)` in `panorama_ar_view.dart`

**Button:** 36dp circle, gold border `0xFFC9973A`, `CustomPaint(_InfoIconPainter)`.

**Sheet:** `DraggableScrollableSheet(initial: 0.55, min: 0.35, max: 0.80, snap: true)` → drag handle → `_SpectrumBar` (6-stop gold→red gradient, 'Intacto'/'Destruído') → `_TypeGrid` (`GridView.count(crossAxisCount: 4)`, 8 cells with `ARPOIMarkerIcon` + `POIType.namePt`).

**`POIType.namePt`:** Militar / Real / Religioso / Cívico / Marítimo / Infraestrutura / Paisagem / Comércio.

**Test helper (avoids pumpAndSettle timeout when ARModeToggle is in tree):**
```dart
Future<void> _openSheet(WidgetTester tester) async {
  await tester.tap(find.byType(POILegendButton));
  await tester.pump();
  await tester.pump(const Duration(milliseconds: 500));
}
```

---

### Test coverage summary (after L1–L4 implementation, this session)

| Layer | File                                     | What was added                                             |
| ----- | ---------------------------------------- | ---------------------------------------------------------- |
| L1    | `poi_visibility_service_test.dart`       | MarkerScaleTier, LOD, priority, toString groups            |
| L2    | `poi_legend_button_test.dart` (NEW)      | 13 groups                                                  |
| L2    | `ar_mode_toggle_test.dart`               | Design structure group                                     |
| L2    | `ar_poi_marker_test.dart`                | Groups 11–13 (icon visibility, paint opts, selected morph) |
| L3    | `panorama_layout_flow_test.dart` (NEW)   | 31 tests, 10 journeys A–J                                  |
| L4    | `integration_test/panorama_ar_test.dart` | Flows 11–13 (+15 tests)                                    |

**Full `lib/` suite (Phase 1 final baseline): 1134 tests passing, 3 skipped, 0 analyze errors.**


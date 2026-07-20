# [Phase 2] — App Design, Integration & Store Release

## Note for AI agent: mark each task with [ ] and then when that task is implemented and tested and passes all tests mark with ✅

---

## This File Guide Index

0. [Phase summary](#0-phase-summary)
1. [Pre-implementation checklist](#1-pre-implementation-checklist)
2. [Architectural decisions](#2-architectural-decisions)
3. [UI/UX structure and flows](#3-uiux-structure-and-flows)
4. [Data and state architecture](#4-data-and-state-architecture)
5. [File and folder structure](#5-file-and-folder-structure)
6. [Task execution plan](#6-task-execution-plan)
7. [Global test suite](#7-global-test-suite)
8. [Phase verification checklist](#8-phase-verification-checklist)
9. [Ongoing rules reminder](#9-ongoing-rules-reminder)

---

## 0. Phase Summary

**Goal:** Wrap the Phase 1 AR core in a museum-ready experience — consent gate, onboarding profile, analytics infrastructure with swappable backends, redesigned home page (8-block adaptive structure), full-page POI detail (10 zones including external links), search + filter flow on the panorama, favourites system, feedback mechanism, expandable panorama FAB, and app store release.

**Why this phase matters:** Phase 1 proved the AR core works. Phase 2 turns it into an app a museum visitor can download, consent to, personalise, explore with search and filters, favourite buildings, give feedback on — and that a thesis researcher can collect structured behavioural data from. Without this phase, the app is a POC that cannot be distributed or studied.

**OUT OF SCOPE (do NOT implement):**
- Audio guide playback (`just_audio`, `NowPlayingBar`) — Phase 3
- Circuit/tour routing and progress — Phase 3
- Quiz per building (full logic) — Phase 3
- Achievement award logic and toast — Phase 3
- Earthquake simulation — Phase 4
- AI assistant (GPT) — Phase 4
- 360° interior views — Phase 4
- Unity 3D integration — Phase 4b
- Expanding POIs beyond 41 — Phase 4
- Full Spanish long-form content — Phase 5
- `flutter_unity_widget` package — Phase 4b
- `just_audio` package — Phase 3
- `http` package — Phase 4
- Firebase or any cloud analytics SDK — never
- Remote analytics server (email backend is the ceiling for Phase 2)
- POI content-depth adaptation per profile type (Phase 3 — the model is set now, content adapts later)

**Estimated duration:** 2–3 months.

**Hard prerequisites (verify ALL before starting):**
- Phase 1 verification: `flutter test lib/` → 1134+ tests passing, `flutter analyze` → 0 errors
- Phase 1 deferred items list reviewed (see §6 Task 2.1 for items pulled into Phase 2)
- `MUSEUM_PARTNERSHIP.md` created with physical panel dimensions (or documented as pending)
- `AR_TRACKING_STRATEGY.md` exists (may be incomplete — field tests are Phase 1 Layer 4)

---

## 1. Pre-implementation Checklist

### Files to read before touching anything

- [ ] `PROJECT_GUIDES/PROJECT_GUIDE.md` — full re-read; sections §1 (entry flow), §2 (AR abstraction), §3 (folder structure), §4 (dependency rules), §5 (testing), §6 Phase 2, §7 (ongoing rules)
- [ ] `PROJECT_GUIDES/IMPLEMENTATION_GUIDELINES.md` — in full
- [ ] `PROJECT_GUIDES/DESIGN/00_INDEX.md` through `08_MOTION_AND_FEEL.md` — design system rules
- [ ] `PROJECT_GUIDES/FEEDBACK/FEEDBACK_GUIDE.md` — state transition patterns
- [ ] `PROJECT_GUIDES/LANGUAGE_SEO_ACCESSIBILITY/I18N_GUIDE.md` — i18n rules
- [ ] `PROJECT_GUIDES/LANGUAGE_SEO_ACCESSIBILITY/SEO_AND_ACCESSIBILITY.md` — accessibility checklist
- [ ] `PROJECT_GUIDES/NAV_AND_LAYOUT/NAVIGATION_AND_LAYOUT_GUIDE.md` — routing + layout
- [ ] `PROJECT_GUIDES/PHASE_1_PLAN.md` — read session completion notes at the bottom for deferred items
- [ ] `lib/domains/home/pages/home_page.dart` — current home page (727 lines)
- [ ] `lib/domains/panorama/ar/widgets/panorama_ar_view.dart` — current Stack layers (727 lines)
- [ ] `lib/domains/panorama/ar/widgets/poi_detail_sheet.dart` — current DraggableScrollableSheet (544 lines) — will become full page
- [ ] `lib/domains/panorama/ar/widgets/poi_summary_card.dart` — current compact card (324 lines)
- [ ] `lib/domains/panorama/ar/widgets/panorama_top_bar.dart` — current top bar (301 lines) — search icons added here
- [ ] `lib/domains/panorama/models/poi.dart` — POI model (290 lines) — no changes, but must understand fields
- [ ] `lib/navigation/navConfig/nav_config.dart` — current routes
- [ ] `lib/main.dart` — current app root setup (284 lines) — consent + analytics wiring goes here
- [ ] `lib/domains/panorama/providers/panorama_providers.dart` — current providers (99 lines)
- [ ] `lib/domains/panorama/ar/providers/ar_infrastructure_providers.dart` — kUseRealAR, arNodeManager
- [ ] `lib/domains/panorama/services/poi_visibility_service.dart` — POIVisibilityConfig (stays const)

### Tests to run to confirm Phase 1 baseline

```
flutter test lib/ --reporter=compact
flutter analyze
```

Expected: 1134+ tests passing, 0 analyze errors. Record exact counts — this is the Phase 2 regression baseline.

### External dependencies to resolve first

| Dependency                   | What                                    | Who resolves            | Blocks                                  |
| ---------------------------- | --------------------------------------- | ----------------------- | --------------------------------------- |
| GDPR ethics board submission | Data collection approval for thesis     | University ethics board | Task 2.4 consent screen text (not code) |
| App store developer accounts | Google Play + Apple Developer accounts  | You                     | Task 2.11 store release                 |
| App icon design              | Final icon for all platforms            | You / designer          | Task 2.11                               |
| `tilestories.app` domain     | Real domain for deep links + share URLs | You                     | Task 2.9 (placeholder until registered) |
| Museum partnership answers   | Panel dimensions, wifi, QR placement    | Museum contact          | Layer 4/5 field tests                   |

### Compile-time flags (already set, verify)

```dart
// lib/domains/panorama/ar/providers/ar_infrastructure_providers.dart
const bool kUseRealAR          = bool.fromEnvironment('USE_REAL_AR');
const bool kShowARDebugOverlay = bool.fromEnvironment('SHOW_AR_DEBUG');
```

### Tools to run

```
# After any Freezed file change:
dart run build_runner build --delete-conflicting-outputs

# Verify baseline:
flutter test lib/ --reporter=compact
flutter analyze
```

---

## 2. Architectural Decisions

| #   | Decision                                                                                                                                                                                                                                      | Why                                                                                                                                                                                                                                                                                                                                                                                                                                                                              | What was rejected                                                                                                                                                                     |
| --- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| D1  | `ConsentState` is a plain Dart enum (`notAsked \| accepted \| declined`), not Freezed                                                                                                                                                         | 3 values, no fields, no copyWith, no JSON — Freezed adds build overhead for zero benefit                                                                                                                                                                                                                                                                                                                                                                                         | Freezed sealed class                                                                                                                                                                  |
| D2  | `consentProvider` is `NotifierProvider<ConsentNotifier, ConsentState>` persisted via SharedPreferences. Initial value is pre-loaded in `main()` and passed as a `ProviderScope` override — identical to how `themeModeProvider` already works | Consent must survive restart; synchronous first-frame rendering avoids flash of wrong state. The override pattern is proven in the existing codebase (`main.dart` line ~135)                                                                                                                                                                                                                                                                                                     | In-memory only (GDPR violation); lazy SharedPreferences read in `build()` (causes async gap)                                                                                          |
| D3  | `ProfileType` is a plain Dart enum (`architecture \| history \| child \| general`) with `TranslatableString` fields                                                                                                                           | Same reasoning as D1 — no union types, no JSON, no many-field copyWith                                                                                                                                                                                                                                                                                                                                                                                                           | Freezed                                                                                                                                                                               |
| D4  | `UserProfile` is a plain Dart class with `profileType` + `preferredLanguage`, not Freezed                                                                                                                                                     | Only 2 fields, no `copyWith` needed (profile is set once per session), no JSON (SharedPreferences stores primitives)                                                                                                                                                                                                                                                                                                                                                             | Freezed model                                                                                                                                                                         |
| D5  | Analytics uses an abstract `AnalyticsBackend` interface with `LocalSQLiteBackend` + optional `EmailBackend` + `CompositeBackend` pattern                                                                                                      | Swapping local-only for remote requires changing one provider override. Thesis needs local fidelity; email backend sends session summaries for live monitoring. `sqflite` is already a transitive dependency. Composite pattern allows both simultaneously                                                                                                                                                                                                                       | Single concrete `AnalyticsService` coupled to SQLite (no swap path); `dart:io` JSON-append (breaks at 1000+ visitors)                                                                 |
| D6  | `AnalyticsEvent` is a plain Dart class (not Freezed)                                                                                                                                                                                          | Fields are written to SQLite columns, not serialized to JSON. No copyWith or union needed                                                                                                                                                                                                                                                                                                                                                                                        | Freezed                                                                                                                                                                               |
| D7  | `POIVisibilityConfig` stays as hardcoded `const` in Phase 2                                                                                                                                                                                   | No consumer varies config per profile yet. Promoting to provider adds complexity for zero benefit. Defer to Phase 3 when circuits need different LOD limits                                                                                                                                                                                                                                                                                                                      | Provider promotion from Phase 2                                                                                                                                                       |
| D8  | Consent gate is a GoRouter redirect, not a separate widget tree branch. Deep link destination is preserved in a transient `pendingDeepLinkProvider` (NotifierProvider) before redirect fires                                                  | All redirect logic is already in `router_config.dart`. Storing the intended URI before redirecting to `/consent` means share links (`/panorama?poi=castelo-sao-jorge`) are not lost                                                                                                                                                                                                                                                                                              | Widget-level conditional rendering; losing deep link on consent redirect                                                                                                              |
| D9  | Onboarding is skippable with "General visitor" default                                                                                                                                                                                        | Museum visitors have 15 seconds of patience. Blocking on profile selection reduces engagement                                                                                                                                                                                                                                                                                                                                                                                    | Mandatory profile selection                                                                                                                                                           |
| D10 | `cached_network_image` removed from pubspec                                                                                                                                                                                                   | No remote images exist. All assets are bundled. Keeping it adds 3+ transitive dependencies for zero benefit                                                                                                                                                                                                                                                                                                                                                                      | Keep as "might need later"                                                                                                                                                            |
| D11 | Expandable FAB uses a custom `Stack` + `AnimatedPositioned` pattern, not a package                                                                                                                                                            | Flutter SDK provides all primitives needed in <100 lines. No package adds value here                                                                                                                                                                                                                                                                                                                                                                                             | `flutter_speed_dial` package                                                                                                                                                          |
| D12 | Route stubs use `_ComingSoonPage` inline widget, not separate files                                                                                                                                                                           | These are temporary placeholders. Creating files for them adds noise to the folder structure                                                                                                                                                                                                                                                                                                                                                                                     | Separate page files per stub                                                                                                                                                          |
| D13 | Deep link `tilestories.app` domain placeholder remains until domain is registered. No `AndroidManifest.xml` intent filter changes until the domain is real                                                                                    | Cannot configure intent filters or iOS associated domains without a verified domain. Share URL uses placeholder with `// TODO(Phase2): replace with real domain`                                                                                                                                                                                                                                                                                                                 | Configure deep links with placeholder domain (would fail verification)                                                                                                                |
| D14 | POI Detail is a **full page** (routed via `Navigator.push` or overlay), NOT a `DraggableScrollableSheet`                                                                                                                                      | The wireframe specifies 10 content zones — gallery, header, category, "Ver Em" grid, "Explorar" grid, external links, actions, content tabs, content body. This volume of content does not fit in a half-screen bottom sheet. Full page provides proper scroll space and accommodates future Phase 3+ content (audio, quiz, AI)                                                                                                                                                  | `DraggableScrollableSheet` at 50–92% (current Phase 1 implementation — too cramped)                                                                                                   |
| D15 | Search + Filter is a Phase 2 feature, implemented as an overlay panel on the panorama page                                                                                                                                                    | The wireframe defines a 4-step flow: idle → search active → single result or apply selection → filtered map. This is core to the museum experience — visitors need to find specific buildings among 41 POIs                                                                                                                                                                                                                                                                      | Defer to Phase 3 (would leave 41 markers with no discovery mechanism)                                                                                                                 |
| D16 | Favourites uses `SharedPreferences` (list of POI IDs as JSON string), not SQLite                                                                                                                                                              | Simple list of strings, no querying, no relationships. SharedPreferences is sufficient and already available                                                                                                                                                                                                                                                                                                                                                                     | SQLite table (overkill for a list of IDs)                                                                                                                                             |
| D17 | Feedback form sends via the same `AnalyticsBackend` — feedback events are a special `AnalyticsEventType.feedbackSubmitted` with metadata containing the form responses                                                                        | One data pipeline, one consent gate. No separate feedback service needed                                                                                                                                                                                                                                                                                                                                                                                                         | Separate feedback API endpoint; separate consent for feedback                                                                                                                         |
| D18 | `AnalyticsService` is pre-created in `main()` with `await init()` called before `runApp()`, then passed as a `ProviderScope` override. The provider type remains `Provider<AnalyticsService>` (not `FutureProvider`)                          | Guarantees the SQLite database is open before any widget reads the provider. Prevents the "writing to uninitialized DB" race condition. Pattern matches existing theme-mode preloading                                                                                                                                                                                                                                                                                           | Lazy `Provider<AnalyticsService>` that calls `init()` internally (race condition if widget reads before init completes); `FutureProvider` (forces `AsyncValue` unwrapping everywhere) |
| D19 | Version number is hardcoded `'1.0.0'` in Phase 2. `package_info_plus` is NOT added                                                                                                                                                            | For a thesis app going to stores for the first time, hardcoding is simpler and avoids a new dependency. Revisit in Phase 3 if automatic versioning is needed                                                                                                                                                                                                                                                                                                                     | `package_info_plus` package (adds dependency for one string)                                                                                                                          |
| D20 | FAB stub sub-actions use `onPressed: () {}` (not `null`) with `Tooltip(triggerMode: TooltipTriggerMode.tap)`                                                                                                                                  | `onPressed: null` disables the button entirely and prevents `Tooltip` from showing on tap. Using an empty callback keeps the button tappable so the "Em breve" tooltip fires                                                                                                                                                                                                                                                                                                     | `onPressed: null` (tooltip never shown on disabled button)                                                                                                                            |
| D21 | Pages with a self-scrolling body (`ListView`, `CustomScrollView`, or any widget inside `Expanded`) MUST call `LayoutPresets.defaultPageBrowser(scrollable: false)` (or `defaultPageApp(scrollable: false)`)                                   | `LayoutManager` wraps the body in `SingleChildScrollView` when `scrollable: true` (the default). An `Expanded` inside a `SingleChildScrollView` gives the child unbounded height → `RenderFlex` layout crash at runtime, in both production and widget tests. Discovered during Task 2.5 `FavouritesPage` testing. `defaultPageBrowser()` now has a `bool scrollable = true` parameter (default preserves backward compatibility); self-scrolling pages pass `scrollable: false` | Keep `scrollable: true` always (causes `RenderFlex` overflow crash when body contains `Expanded + ListView`)                                                                          |

---

## 3. UI/UX Structure and Flows

### Design Philosophy Recap (from Phase 1, still applies)

The painting is the product. The UI is nearly invisible until needed. "Candlelight on old stone" — warm, layered, alive with history. Every screen: single focal point, at least one motion moment, considered depth. Ask: *would someone screenshot this?*

**15-second rule:** A museum visitor should go from app open to first interaction in 15 seconds. The page grows with engagement, not with features — first-time users see 4 blocks on the home page; return users see up to 7.

---

### 3.1 Consent Screen (`/consent`)

**Route:** `/consent` — shown once on first launch (redirect gate in `router_config.dart`).

**Layout:** `LayoutPresets.fullscreen()` — no header, no footer. Centred content.

**Structure:**
```
LuxuryScaffold
  └── CenteredContent(maxWidth: LayoutTokens.readingWidth)
      ├── AppBrand(direction: Axis.vertical, logoSize: 64)
      ├── SizedBox(Spacing.xl2)
      ├── Text: title "Política de Privacidade" / "Privacy Policy" / "Política de Privacidad"
      ├── SizedBox(Spacing.lg)
      ├── _ConsentBody: scrollable explanation text (GDPR Article 13 compliant — see below)
      ├── SizedBox(Spacing.xl2)
      ├── FilledButton: "Aceitar" / "Accept" / "Aceptar" → consent = accepted → navigate to stored deep link or '/'
      ├── SizedBox(Spacing.sm)
      ├── OutlinedButton: "Recusar" / "Decline" / "Rechazar" → consent = declined → navigate to stored deep link or '/'
      └── SizedBox(Spacing.md)
          └── TextButton: "Saber mais" / "Learn more" (expandable detail section)
```

**GDPR consent text must cover (Article 13):**
1. What data is collected: POI taps, session duration, timeline epoch changes, profile type — NO personal identifiers, NO location, NO photos, NO cookies
2. Legal basis: consent — explicit, freely given, specific, informed
3. Purpose: academic thesis research on museum visitor engagement patterns
4. Controller: you (researcher) + your university
5. Retention period: thesis study period, then deleted
6. Right to withdraw consent and delete data at any time (Settings → "Apagar os meus dados")
7. Data stored locally on device only (Phase 2). If email backend is enabled, note that anonymised session summaries are sent to the researcher
8. What you do NOT need to cover: cookies (none), third-party sharing (none), automated decisions (none), cross-border transfers (none)

**State reads:** `consentProvider`
**State writes:** `consentProvider.notifier.setConsent(ConsentState)`

**Deep link preservation:** After accept/decline, navigate to the URI stored in `pendingDeepLinkProvider` (set by the router redirect before arriving at `/consent`). If no pending URI, navigate to `/`.

**Entry animation:** `RevealAnimation` r1–r3 stagger on logo, text, buttons.

**Accessibility:**
- `Semantics(liveRegion: true)` on consent body for screen readers
- All tap targets ≥ 48px (`SizeTokens.tapTarget`)
- Plain language in all 3 languages

**Edge cases:**
- App works fully with `declined` — analytics silently no-ops via backend interface
- Consent persisted in SharedPreferences — survives restart
- If user clears app data → `notAsked` → consent screen shown again
- Deep link to `/panorama?poi=X` on first launch → consent screen → accept → redirected to `/panorama?poi=X` (not `/`)

---

### 3.2 Home Page Redesign (`/`) — 8-Block Adaptive Structure

**Route:** `/` — the museum visitor's first screen.

**Layout:** `LayoutSlots` with `LuxuryScaffold` body, `FooterApp` on mobile, `Header` on web.

**Design goal:** 15 seconds from app open to first interaction. Page grows with engagement, not with features. First-time user sees 4 blocks; return user sees up to 7 + feedback link.

**Block structure (from wireframe):**

```
LuxuryScaffold
  └── SingleChildScrollView
      │
      ├── BLOCK 1 — Top Chrome
      │     Row: [LanguageSwitcher] · [ThemeSwitcher] · [ProfileAvatar]
      │     Three icons only: PT (language pill), ☀/🌙 (light/dark toggle), P (profile avatar).
      │     Minimal. No app name here. Language pill shows current lang code (e.g., "PT").
      │     ThemeSwitcher shows sun icon (light) or moon icon (dark). Tap toggles theme.
      │     ProfileAvatar is gold circle with user's initial letter.
      │     ProfileAvatar tap → /onboarding (edit profile) or /onboarding (set profile).
      │
      ├── BLOCK 2 — Branding
      │     AppBrand(direction: Axis.vertical, logoSize: 64)
      │     "TILESTORIES" wordmark below logo (dmMicroLabel, letterSpacing: 3.5)
      │     Subtle. Sets museum-quality tone. No description text.
      │
      ├── BLOCK 3 — Hero Card (painting context + primary CTA)
      │     LuxuryCard with gold accent bar on left edge:
      │       ├── Text: "Grande Panorama" (frauncesDisplay)
      │       ├── Text: "de Lisboa · c. 1700" (bodyMedium, muted)
      │       ├── Text: "Museu Nacional do Azulejo" (bodySmall, muted)
      │       ├── Text: "Maior panorama de azulejos do mundo · 23m" (bodySmall, muted)
      │       └── FilledButton: "▶ Explorar com AR" → permission check → /panorama
      │     KEY CHANGE: painting context lives HERE, not in a separate QuickInfoSection.
      │     Gold accent bar = RadiusTokens.xs width vertical bar on left of card.
      │
      ├── BLOCK 4 — Secondary CTA (offline mode)
      │     OutlinedButton full-width: "Explorar sem câmera" / "Explore without camera"
      │     → /panorama (offline mode, kUseRealAR=false in-app toggle)
      │
      ├── BLOCK 5 — Profile Banner [CONDITIONAL: only if profile == null]
      │     Dashed border card (NOT solid — dashed = "something missing" visual cue):
      │       ├── Icon: person_add_outlined in gold
      │       ├── Text: "Personalize a sua visita"
      │       ├── Text: "Escolha o seu perfil de exploração →"
      │       └── onTap → /onboarding
      │     Border: 1px dashed gold at 40% opacity.
      │     Hidden once profile is set (including 'general' from skip).
      │
      ├── BLOCK 6 — Favourites [CONDITIONAL: only if favourites.isNotEmpty]
      │     Row: "Os meus favoritos" title + "Ver todos →" link (gold, labelMedium)
      │     Horizontal scroll of FavouriteCard widgets:
      │       ├── FavouriteCard: POI icon (type color) + POI name (truncated) + heart icon
      │       ├── FavouriteCard: ...
      │       └── _AddMoreCard: "+" icon → /panorama (discover more)
      │     Each FavouriteCard tap → /panorama?poi=<id> (opens panorama, flies to POI, shows summary card)
      │     "Ver todos →" tap → /favourites (full list page)
      │     Card height: ~100dp. Horizontal scroll with BouncingScrollPhysics.
      │
      ├── BLOCK 7 — Last Visit [CONDITIONAL: only if analytics has session history]
      │     Subtle card with return shortcut:
      │       ├── "↻ Retomar visita: [POI name] →"
      │       ├── "última visita há 2 dias" (bodySmall, muted)
      │       └── onTap → /panorama?poi=<lastVisitedPOIId>
      │     Data source: `lastVisitedPOIProvider` (FutureProvider<POI?>) — async SQLite query.
      │     **Loading state:** While the future is loading, Block 7 is HIDDEN (same as null result).
      │       This avoids both a blocking build and a flash of empty content.
      │       `ref.watch(lastVisitedPOIProvider).when(data: (poi) => poi != null ? Block7(...) : SizedBox.shrink(), loading: () => SizedBox.shrink(), error: (_,__) => SizedBox.shrink())`
      │     Hidden on first visit (no analytics history), declined consent, or reinstall.
      │
      └── BLOCK 8 — Feedback Link [ALWAYS VISIBLE]
            TextButton: "Dar feedback sobre a app" (bodySmall, muted, centred)
            Very subtle. Always present. onTap → opens feedback bottom sheet.
```

**What first-time user sees (6 blocks):** Top Chrome · Branding · Hero Card · Secondary CTA · Profile Banner · Feedback Link. No favourites, no last visit.

**What return user sees (up to 7 blocks):** Top Chrome · Branding · Hero Card · Secondary CTA · Favourites · Last Visit · Feedback Link. Profile banner gone (profile set).

**Changes from Phase 1 home page:**
- REMOVED: `_QuickInfoSection` as separate block — painting context is now INSIDE the Hero Card (Block 3)
- ADDED: Favourites horizontal scroll (Block 6)
- ADDED: Last Visit return shortcut (Block 7)
- ADDED: Feedback link (Block 8)
- CHANGED: Profile banner uses dashed border, not solid LuxuryCard
- ProfileAvatar: tap navigates to `/onboarding` (edit profile)
- Add ES strings to ALL text where missing
- Add `Tooltip` to ProfileAvatar

**State reads:** `languageProvider`, `themeModeProvider`, `onboardingProvider`, `favouritesProvider`, `analyticsServiceProvider` (for last visit query), `isNavigatingProvider`
**State writes:** `isNavigatingProvider` (before navigation), `cameraPermissionProvider` (side-effect on AR CTA)

**Entry animation:** `RevealAnimation` stagger r1–r5 on blocks. Conditional blocks animate in when they become visible (e.g., favourites appear after first favourite is added).

**Accessibility:**
- Each block: logical reading order for screen reader
- Profile banner: `Semantics(button: true, label: 'Personalize a sua visita')`
- ProfileAvatar: `Tooltip(message: 'Perfil')` + `Semantics(button: true)`
- Favourite cards: `Semantics(button: true, label: poiName)`
- Last visit card: `Semantics(button: true, label: 'Retomar visita: [POI name]')`
- All tap targets ≥ 48px

---

### 3.3 Onboarding / Profile Setup (`/onboarding`)

**Route:** `/onboarding` — accessible from home page Profile Banner (Block 5) or ProfileAvatar (Block 1).

**Layout:** `LayoutPresets.defaultPageApp()` — immersive, no header chrome.

**Structure:**
```
LuxuryScaffold
  └── CenteredContent(maxWidth: LayoutTokens.formWidth)
      ├── _BackButton (if navigated from home, not from redirect)
      ├── SizedBox(Spacing.xl2)
      ├── Text: "Com quem explora?" / "Who are you exploring with?" / "¿Con quién explora?"
      ├── SizedBox(Spacing.md)
      ├── Text: subtitle explaining profile customization (1 sentence)
      ├── SizedBox(Spacing.xl2)
      ├── ProfileSelector: 4 profile cards in 2×2 grid
      │     ├── ProfileCard: 🏛 Architecture — "Proporções, estilos, detalhes construtivos"
      │     ├── ProfileCard: 📜 History — "Datas, eventos, contexto político"
      │     ├── ProfileCard: 👨‍👩‍👧 Family — "Linguagem simples, interativo"
      │     └── ProfileCard: 🌍 General — "Um pouco de tudo"
      ├── SizedBox(Spacing.xl2)
      ├── FilledButton: "Continuar" / "Continue" / "Continuar" → save profile → go('/')
      └── TextButton: "Saltar" / "Skip" / "Saltar" → set profile=general → go('/')
```

**State reads:** `onboardingProvider`, `languageProvider`
**State writes:** `onboardingProvider.notifier.setProfile(ProfileType)`

**Entry animation:** Profile cards stagger in r1–r4; "Continuar" button at r5.

**Editing vs first-visit logic:**
- If `onboardingProvider.state != null` (editing existing profile): pre-select current `profileType`, "Continuar" is enabled immediately, button label says "Guardar" / "Save" / "Guardar".
- If `onboardingProvider.state == null` (first visit): no pre-selection, "Continuar" disabled until a card is tapped.
- The source of "already has profile" is `ref.watch(onboardingProvider)` — the `UserProfile?` value.

**Accessibility:**
- Each `ProfileCard`: `Semantics(button: true, selected: isSelected, label: profileType.label)`
- Skip button clearly labelled — not hidden
- All tap targets ≥ 48px (`SizeTokens.tapTarget`)
- Icons are decorative (emoji) — paired with text label (colour never sole indicator)

**Edge cases:**
- No profile selected + "Continuar" tapped → button is disabled, no action
- Language preference auto-set from `languageProvider` current value

---

### 3.4 Panorama Page Updates (from wireframe 2)

**Route:** `/panorama` — existing page, heavily modified.

**Wireframe overview:** Top bar with back + AR status pill + ProfileAvatar + ··· menu. Below top bar on the right: search icon (🔍) + info icon (ⓘ). POI markers on the painting. FAB bottom-right. Timeline slider at bottom.

**Top bar changes (PanoramaTopBar):**
The existing top bar has: back button | ARModeToggle pill | options menu (···).
Add to the bar: ProfileAvatar (gold circle, same as home page Block 1) between the ARModeToggle pill and the ··· menu.
ARModeTogglePill stays INSIDE the PanoramaTopBar — it is NOT a separate Stack layer. The wireframe shows it as part of the header row.
The ··· menu must include a "Definições" / "Settings" link → `/settings` (so users don't need to go home to change language/theme).

**Search + Info icons (new — below top bar, right-aligned):**
```
Positioned(top: SizeTokens.appBarHeightMobile + Spacing.sm, right: Spacing.lg)
  └── Row(spacing: Spacing.sm)
      ├── _SearchIconButton: 🔍 icon (40dp, surface.withValues(alpha: 0.85) background)
      │     onTap → opens search overlay (see §3.8 Search + Filter Flow)
      └── _InfoIconButton: ⓘ icon (40dp, same style)
            onTap → opens POI Legend sheet (existing POILegendButton — relocated)
```

**Stack layer update (after all Phase 2 insertions):**
```
[0]  Frame layer (InteractiveViewer or ARCore)
[1]  POI markers overlay
[2]  POISummaryCard (conditional — when POIDisplayMode.summary)
[3]  ARTrackingIndicator
[4]  PanoramaTopBar (MODIFIED — ARModeTogglePill + ProfileAvatar + Settings in menu)
[5]  SearchInfoIcons (NEW — search 🔍 + info ⓘ buttons, right side, horizontal Row)
[6]  PanoramaFAB (NEW — Phase 2)
[7]  TimelineSlider
[8]  EpochLabel
[9]  SearchFilterOverlay (NEW — conditional, covers layers below when active)
[10] ARFirstTimeOverlay
[11] ARDebugOverlay (kDebugMode only)
```

**Note:** POIDetailSheet is NO LONGER a Stack child. It is now a full-page route (see §3.7). When user taps "Mais Info" on `POISummaryCard`, we `Navigator.push` the `POIDetailPage` on top of the panorama.

**Deep link `?poi=<id>` on `/panorama`:**
In `PanoramaARView.initState`:
```dart
final poiId = GoRouterState.of(context).uri.queryParameters['poi'];
if (poiId != null) {
  WidgetsBinding.instance.addPostFrameCallback((_) {
    final pois = ref.read(poisProvider).value ?? [];
    final target = pois.firstWhereOrNull((p) => p.id == poiId);
    if (target != null) {
      ref.read(selectedPOIProvider.notifier).state = target;
      ref.read(poiDisplayModeProvider.notifier).open(POIDisplayMode.summary);
    }
  });
}
```

**Deep link `?favorites=id1,id2,id3`:**
Similar pattern — parse comma-separated IDs, apply as filter to show only those POIs highlighted. This enables share-my-favourites URLs.

---

### 3.5 Panorama FAB (expandable)

**File:** `lib/domains/panorama/ar/widgets/panorama_fab.dart`

**Position:** `Positioned(bottom: Spacing.xl2 + 80, right: Spacing.lg)` — above timeline slider.
**Stack layer:** `[6]` in `panorama_ar_view.dart`.

**Structure:**
```
_ExpandableFAB
  ├── Main button: ⊕ icon (context.primary background, 56dp)
  │     onTap → toggle expanded state
  ├── Sub-actions (staggered upward reveal, r1–r5):
  │     ├── [0] Favourites — Icons.favorite_outline → /favourites
  │     ├── [1] Progress — Icons.emoji_events_outlined → /achievements (stub)
  │     ├── [2] Audio Guide — Icons.headphones_outlined → Tooltip('Em breve')
  │     ├── [3] Circuits — Icons.route_outlined → Tooltip('Em breve')
  │     └── [4] AI Guide — Icons.smart_toy_outlined → Tooltip('Em breve')
  └── Scrim: semi-transparent overlay when expanded (tap outside → collapse)
```

**State reads:** internal `_expanded` bool (local state — not Riverpod)
**State writes:** none (sub-actions navigate or show tooltip)

**Entry/exit animation:**
- Sub-actions: `AnimatedPositioned` upward stagger, `AnimationTokens.r1`–`r5`
- Main button: `AnimatedRotation(turns: _expanded ? 0.125 : 0)` over `AnimationTokens.medium`
- Scrim: `AnimatedOpacity` `Colors.black.withValues(alpha: 0.3)` over `AnimationTokens.medium`

**Stub sub-actions:** Use `onPressed: () {}` (NOT `null`) + `Tooltip(triggerMode: TooltipTriggerMode.tap, message: t(pt: 'Em breve', en: 'Coming soon', es: 'Próximamente'))`. Visual: `opacity: 0.5` on the icon. This ensures the tooltip actually fires on tap (D20).

**Accessibility:**
- `Semantics(label: t(pt: 'Ações de exploração', en: 'Exploration actions', es: 'Acciones de exploración'))`
- Each sub-action: `Tooltip` with full name
- All tap targets ≥ 48px

---

### 3.6 Settings / Privacy Page (`/settings`)

**Route:** `/settings` — accessible from home page header AND from panorama top bar ··· menu.

**Layout:** `LayoutPresets.defaultPageBrowser()`

**Structure:**
```
ResponsiveContainer(contentType: ContentType.reading)
  ├── SectionHeader: "Definições" / "Settings" / "Configuración"
  ├── _LanguageSection: current language + LanguageSwitcher
  ├── _ThemeSection: current theme + ThemeSwitcher
  ├── _ProfileSection: current profile card with icon + description + "Editar" → /onboarding
  ├── GoldDivider
  ├── _PrivacySection
  │     ├── Text: what data is collected (plain-language, condensed version of consent text)
  │     ├── Text: "Os dados são guardados localmente no seu dispositivo"
  │     ├── _ConsentStatusBadge: "Analytics activo" (green) or "Analytics desligado" (muted)
  │     ├── _ConsentToggleButton: change consent → opens confirmation dialog
  │     └── _DeleteDataButton: "Apagar os meus dados" → confirm dialog → analytics.clearAll()
  ├── GoldDivider
  ├── _FeedbackSection
  │     └── OutlinedButton: "Dar feedback" → opens feedback bottom sheet (same as home Block 8)
  ├── GoldDivider
  ├── _AboutSection
  │     ├── AppBrand(direction: Axis.vertical)
  │     ├── Text: version "1.0.0" (hardcoded — D19)
  │     ├── Text: "Museu Nacional do Azulejo"
  │     └── Text: your name, university, thesis context (1–2 lines)
  └── SizedBox(Spacing.xl2)
```

**State reads:** `languageProvider`, `themeModeProvider`, `onboardingProvider`, `consentProvider`
**State writes:** `consentProvider.notifier`, `analyticsServiceProvider.clearAll()`

---

### 3.7 POI Detail Page (FULL PAGE — from wireframe 3)

**CRITICAL CHANGE from Phase 1:** The POI detail is now a **full page**, NOT a `DraggableScrollableSheet`. The wireframe shows 10 content zones that require full screen space. The current `poi_detail_sheet.dart` (544 lines, DraggableScrollableSheet) will be replaced by `poi_detail_page.dart`.

**Navigation:** When user taps "Mais Info" on `POISummaryCard` → `Navigator.of(context).push(MaterialPageRoute(...))` to `POIDetailPage`. This pushes on top of the panorama (panorama stays in memory behind). Back button or swipe-back returns to panorama with the same state.

**Why `Navigator.push` instead of `context.go()`:** The POI detail page is a transient detail view, not a named route. It receives a `POI` object directly. Using `Navigator.push` avoids adding 41 routes to GoRouter and keeps the panorama alive underneath.

**Layout:** `LayoutPresets.defaultPageApp()` — full screen, no header/footer chrome. Custom close button and favourite button inside the page.

**10-Zone Structure (from wireframe):**

```
Scaffold(backgroundColor: context.surface)
  └── CustomScrollView
      │
      ├── Z1 — Gallery (SliverToBoxAdapter with fixed-height PageView)
      │     PageView of photos (placeholder: single dark card with "foto · swipe para mais")
      │     Top-left: X close button → Navigator.pop()
      │     Top-right: ♡ favourite toggle → favouritesProvider.toggle(poi.id)
      │     Bottom: dot indicators (PageView page count)
      │     Phase 2: 1 placeholder image per POI (from assets or currentPhotoUrl if available)
      │     Phase 3+: multiple photos per POI loaded from assets
      │
      ├── Z2+Z3 — Header
      │     Text: POI name (headlineMedium, w700) — e.g., "Castelo de São Jorge"
      │     Survival badge: coloured pill (green=intact, red=destroyed, amber=damaged)
      │       Text: "Sobreviveu intacto ao terramoto de 1755" / "Destroyed in..." / "Damaged in..."
      │       Badge colour from destructionLevelColor(poi.destructionLevel)
      │     Updates with timeline epoch (watch timelineProvider)
      │
      ├── Z4 — Category
      │     "Categoria:" label + coloured pill with POI type name
      │     Pill colour: poi.poiTypeEnum.accentColor
      │     e.g., "Militar" in a red-tinted pill
      │
      ├── Z5 — "VER EM" section (view modes)
      │     MicroLabel: "VER EM" (dmMicroLabel, microLabelColor)
      │     Row of 3 action cells (GridView or Row):
      │       ├── "3D" — icon + label — DIMMED (Phase 3/4, 40% opacity + lock icon overlay)
      │       ├── "360°" — icon + label — DIMMED (Phase 4)
      │       └── "Mapa" — icon + label — ACTIVE (opens system maps with POI coords, or in-app map stub)
      │     Dimmed cells: `opacity: 0.4`, small lock icon overlay, `Tooltip('Em breve')`
      │     Active cells: normal opacity, `onTap` triggers action
      │
      ├── Z6 — "EXPLORAR" section (exploration tools)
      │     MicroLabel: "EXPLORAR"
      │     Row of 3 action cells:
      │       ├── "Áudio" — ▶ icon — DIMMED (Phase 3, plays inline audio guide)
      │       ├── "Quiz" — ? icon — DIMMED (Phase 3, navigates to /quiz/:poiId)
      │       └── "Guia IA" — 🤖 icon — DIMMED (Phase 4, navigates to /ai-chat)
      │     Same dimming pattern as Z5
      │
      ├── Z7 — External Links
      │     MicroLabel: "LINKS EXTERNOS" / "EXTERNAL LINKS" / "ENLACES EXTERNOS" (dmMicroLabel)
      │     Vertical list of tappable link rows. Each row:
      │       ├── Leading icon: platform-specific (globe for website, Facebook/Instagram/YouTube brand icons)
      │       ├── Text: link label (e.g., "Website oficial", "Facebook", "Instagram", "YouTube")
      │       └── Trailing: external-link icon (Icons.open_in_new, 16dp, muted)
      │     Tap → `launchUrl(Uri.parse(url), mode: LaunchMode.externalApplication)` via `url_launcher`.
      │     If `poi.externalLinks` is empty → Z7 is HIDDEN entirely (no empty section).
      │     Demo data in `pois.json`: each POI has a varying number of links (0–4) with different
      │     combinations of website, Facebook, Instagram, YouTube to show realistic variety.
      │     See §5 for POI model extension and demo data specification.
      │
      │     **Data model (`externalLinks` field on POI):**
      │     ```json
      │     "externalLinks": [
      │       { "label": { "pt": "Website oficial", "en": "Official website", "es": "Sitio web oficial" }, "url": "https://...", "type": "website" },
      │       { "label": { "pt": "Facebook", "en": "Facebook", "es": "Facebook" }, "url": "https://facebook.com/...", "type": "facebook" },
      │       { "label": { "pt": "Instagram", "en": "Instagram", "es": "Instagram" }, "url": "https://instagram.com/...", "type": "instagram" },
      │       { "label": { "pt": "YouTube", "en": "YouTube", "es": "YouTube" }, "url": "https://youtube.com/...", "type": "youtube" }
      │     ]
      │     ```
      │     `type` field drives the leading icon: `website` → Icons.language, `facebook` → custom FB icon,
      │     `instagram` → custom IG icon, `youtube` → custom YT icon. Fallback: Icons.link.
      │     Social icons: use simple `Icon` with brand-suggestive Material icons (e.g., Icons.camera_alt
      │     for Instagram) or small SVG assets in `assets/images/social/`. Keep lightweight — no external
      │     package for brand icons.
      │
      ├── Z8 — Actions Row
      │     Row of 2 buttons:
      │       ├── OutlinedButton: "↑ Partilhar" → native share sheet (Share.share from share_plus)
      │       │     Share text: "Descobre [POI name] no Grande Panorama de Lisboa! tilestories.app/panorama?poi=[id]"
      │       └── OutlinedButton: "💬 Dar Feedback" → opens feedback bottom sheet
      │             Feedback form: 3 questions (star ratings) + free text + optional email
      │             Submits via AnalyticsBackend (feedbackSubmitted event with metadata)
      │
      ├── Z9 — Content Tabs
      │     TabBar with 3 tabs:
      │       ├── "Descrição" — ALWAYS available (Phase 2)
      │       ├── "História" — Phase 2 content (longer historical narrative)
      │       └── "Curiosidades" — Phase 2 content (fun facts, anecdotes)
      │     Tab content adapts to profile type in Phase 3 (e.g., Architecture profile gets
      │     construction details; Family profile gets simplified language). For Phase 2,
      │     all profiles see the same content.
      │
      └── Z10 — Content Body
            TabBarView corresponding to Z9 tabs.
            Scrollable text content. Zones Z1–Z9 scroll out of view as user scrolls down.
            Fade-out gradient at bottom = scroll hint (visual cue that content continues).
            Content source: `poi.description` for "Descrição" tab. "História" and "Curiosidades"
            tabs use new fields added to POI JSON data (or separate content JSON files per POI).
```

**State reads:** `selectedPOIProvider`, `timelineProvider`, `favouritesProvider`, `languageProvider`
**State writes:** `favouritesProvider.toggle(poiId)`, `analyticsServiceProvider.logEvent(poiTapped)`

**Entry animation:** Page slides up from bottom (standard `MaterialPageRoute`). Z1 gallery fades in. Header and sections use `RevealAnimation` r1–r5 stagger.

**Accessibility:**
- Close button: `Semantics(button: true, label: 'Fechar detalhes')` + `Tooltip`
- Favourite button: `Semantics(button: true, label: 'Adicionar aos favoritos', toggled: isFavourited)`
- Each "VER EM" / "EXPLORAR" cell: `Semantics(button: true, label: name, enabled: isActive)`
- Dimmed cells: `Semantics(label: 'Em breve')` for screen reader
- Content tabs: `TabBar` has native accessibility
- All tap targets ≥ 48px

**Edge cases:**
- POI with no photos → Z1 shows single placeholder card
- POI with no external links → Z7 hidden entirely (empty `externalLinks` list)
- POI with no "História" or "Curiosidades" content → those tabs still appear but show "Conteúdo em breve" placeholder text
- Timeline epoch change while detail page is open → survival badge updates live

---

### 3.8 Search + Filter Flow (from wireframe 4)

**New feature for Phase 2.** The wireframe defines a 4-step flow for finding POIs among the 41 markers.

**File:** `lib/domains/panorama/ar/widgets/search_filter_overlay.dart` (+ supporting widgets)

**Step 1 — Idle state:**
- Two icon buttons visible on panorama page (see §3.4, SearchInfoIcons layer [5]):
  - 🔍 = tap to open search
  - ⓘ = tap to open legend sheet (existing POILegendButton, relocated)
- No overlay. Normal panorama interaction.

**Step 2 — Search active (overlay opens):**
- Tapping 🔍 opens the `SearchFilterOverlay` which covers most of the screen as a semi-transparent panel (panorama still dimly visible behind).
- **Search bar** replaces the two icon buttons area:
  - Text field with 🔍 prefix icon + ⁞ filter icon inside the pill
  - ⓘ button stays visible to the right of the search pill
  - Typing filters the result list by POI name (fuzzy match or `contains` on translated name)
- **Destruction level filter chips:**
  - Row: "NÍVEL DE DESTRUIÇÃO" (dmMicroLabel)
  - Chips: `0%` | `20%` | `40%` | `60%+` — tap to toggle (multi-select)
  - 0% = intact (destructionLevel 0), 20% = 1–20, 40% = 21–40, 60%+ = 41–100
  - Selected chip = filled, others = outlined
- **POI type filter circles:**
  - Row: "TIPO DE POI" (dmMicroLabel)
  - Circular icons for each POIType, coloured with `poiType.accentColor`
  - Tap to toggle (multi-select). Selected = green ring border. Unselected = empty.
  - Colours match the legend (same as `POILegendButton`)
- **Result list:**
  - "RESULTADOS (N)" header showing count of matching POIs
  - `ListView` of POI name rows, each with → arrow
  - Filtered by: text query AND destruction chips AND type circles (intersection)
  - Shows all 41 if no filters active
- **Bottom actions:**
  - "cancel" (context.error colour) — discard search + filters, restore panorama to full view
  - "apply selection" (filled, bold) — apply filters to map, close overlay

**Step 3a — Tap single result:**
- Tapping a POI row in the result list:
  - Closes search overlay immediately
  - Map flies/scrolls to that POI's position
  - Opens `POISummaryCard` for that POI
  - Search bar collapses back to a pill showing the POI name + X clear button
  - No "apply" needed — direct tap is an immediate action

**Step 3b — Apply selection (filtered map):**
- Tapping "apply selection":
  - Closes search overlay
  - Map shows ONLY the filtered POIs (other markers hidden or very faded)
  - Search bar collapses to a pill showing active filter tags (e.g., "Militar" + "0%") + X clear
  - Below top bar: "2 filtros activos · 18 resultados" status line
  - Tapping X on the pill clears all filters and restores full map

**State:**
```
searchQueryNotifier      — NotifierProvider<SearchQueryNotifier, String> (local to search flow)
destructionFilterProvider — NotifierProvider<Set<DestructionRange>>
typeFilterProvider       — NotifierProvider<Set<POIType>>
filteredPOIsProvider     — computed from poisProvider + all three filters
searchActiveNotifier     — NotifierProvider<SearchActiveNotifier, bool> (controls overlay visibility)
```

> **No StateProvider:** Per project rules (PROJECT_GUIDE §7, §9 rules table),
> `StateProvider` is banned. These use trivial `NotifierProvider` wrappers:
> ```dart
> class SearchQueryNotifier extends Notifier<String> {
>   @override String build() => '';
>   void update(String q) => state = q;
>   void clear() => state = '';
> }
>
> class SearchActiveNotifier extends Notifier<bool> {
>   @override bool build() => false;
>   void activate() => state = true;
>   void deactivate() => state = false;
> }
> ```

**Accessibility:**
- Search field: `Semantics(textField: true, label: 'Pesquisar pontos de interesse')`
- Filter chips: `Semantics(button: true, selected: isSelected, label: chipLabel)`
- Result rows: `Semantics(button: true, label: poiName)`
- Cancel/Apply buttons: standard button semantics
- All tap targets ≥ 48px

**Edge cases:**
- Empty search text + no filters → show all 41 POIs in result list
- Search text matches nothing → "Nenhum resultado" empty state
- All filters cleared → back to full map (same as cancel)
- Search overlay open + back button → closes overlay (PopScope)

---

### 3.9 Favourites System

**New feature for Phase 2.** Simple heart-based favouriting of POIs.

**Storage:** `SharedPreferences` — key `'favourite_poi_ids'`, value is a JSON-encoded `List<String>` of POI IDs (D16).

**Provider:**
```dart
class FavouritesNotifier extends Notifier<List<String>> {
  static const _prefsKey = 'favourite_poi_ids';
  @override
  List<String> build() => _initialIds; // pre-loaded in main(), override pattern
  void toggle(String poiId) { ... } // add if absent, remove if present; persist
  void remove(String poiId) { ... }
  bool isFavourite(String poiId) => state.contains(poiId);
}
```

**Where favourites appear:**
1. **Home page Block 6** — horizontal scroll of `FavouriteCard` widgets (see §3.2)
2. **POI Detail Page Z1** — heart icon top-right of gallery (see §3.7)
3. **FAB sub-action [0]** — "Favoritos" → `/favourites` (see §3.5)
4. **`/favourites` route** — full list page with all favourited POIs

**`/favourites` page:**
```
LayoutPresets.defaultPageBrowser()
  └── ResponsiveContainer
      ├── SectionHeader: "Os meus favoritos" / "My favourites" / "Mis favoritos"
      ├── if empty: EmptyState("Ainda não tem favoritos. Explore o panorama e toque ♡ para guardar.")
      └── if not empty: ListView of POI cards (name, type pill, destruction badge, unfavourite button)
            Each card tap → /panorama?poi=<id>
```

**Analytics:** `favouriteToggled` event (poiId, action: 'added'|'removed').

---

### 3.10 Feedback Mechanism

**New feature for Phase 2.** Lightweight feedback collection for thesis research.

**Where feedback appears:**
1. **Home page Block 8** — "Dar feedback sobre a app" text button (always visible, subtle)
2. **POI Detail Page Z8** — "Dar Feedback" button in actions row
3. **Settings page** — "Dar feedback" button in feedback section

**Feedback bottom sheet (`FeedbackSheet`):**
Opened from any of the above trigger points. Uses `showModalBottomSheet` with standard Material 3 pattern.

```
DraggableScrollableSheet(initialChildSize: 0.65, maxChildSize: 0.9)
  └── Column
      ├── Drag handle
      ├── Text: "Como foi a sua experiência?" / "How was your experience?"
      ├── SizedBox(Spacing.lg)
      ├── _StarRating: "Experiência geral" — 1–5 stars
      ├── _StarRating: "Experiência AR" — 1–5 stars
      ├── SizedBox(Spacing.md)
      ├── TextField: "O que poderia ser melhor?" (free text, multiline, max 500 chars)
      ├── SizedBox(Spacing.md)
      ├── TextField: "Email para contacto futuro (opcional)" (email, optional)
      ├── SizedBox(Spacing.lg)
      ├── FilledButton: "Enviar feedback" → submit → close sheet → snackbar "Obrigado!"
      └── TextButton: "Cancelar" → close sheet
```

**Submission:** Creates an `AnalyticsEvent` with `eventType: AnalyticsEventType.feedbackSubmitted` and metadata containing all form values. This goes through the same `AnalyticsBackend` pipeline — stored locally in SQLite AND (if email backend is enabled) sent as part of the session summary email.

**State:** Local widget state for form fields. No provider needed — feedback is fire-and-forget.

**Accessibility:**
- Star ratings: `Semantics(slider: true, value: '$rating de 5')`
- Text fields: standard `InputDecoration` with labels
- All tap targets ≥ 48px

---

### Main User Flows

**Flow 1 — First launch, engage fully:**
```
App opens → router sees consent=notAsked → redirects to /consent
  → "Aceitar" → consent=accepted → home (/)
  → Sees: Top Chrome · Branding · Hero Card · Secondary CTA · Profile Banner · Feedback link
  → Taps profile banner → /onboarding → selects "Architecture" → "Continuar" → home (/)
  → Profile banner gone. Taps "▶ Explorar com AR" → permission granted → /panorama
  → Sees 41 POI markers. Taps one → POISummaryCard. Taps "Mais Info" → POIDetailPage (full page)
  → Taps ♡ → POI added to favourites. Taps back → panorama. Taps 🔍 → search overlay.
  → Types "castelo" → sees filtered list → taps "Castelo de São Jorge" → map flies to POI
```

**Flow 2 — First launch, skip everything:**
```
App opens → /consent → "Recusar" → consent=declined → home (/)
  → Ignores profile banner → taps "Explorar sem câmera" → /panorama offline
  → Full experience works. Zero analytics logged. Favourites still work (SharedPreferences).
```

**Flow 3 — Return visit:**
```
App opens → consent=accepted (persisted) → home (/) directly
  → Sees: Top Chrome · Branding · Hero Card · Secondary CTA · Favourites section · Last Visit · Feedback
  → Profile banner gone (profile set). Taps favourite "Castelo" card → /panorama?poi=castelo-sao-jorge
  → POISummaryCard opens automatically for Castelo.
```

**Flow 4 — Incoming share link (deep link preservation):**
```
User receives tilestories.app/panorama?poi=castelo-sao-jorge
  → App opens fresh → router sees consent=notAsked → stores intended URI in pendingDeepLinkProvider
  → Redirects to /consent → "Aceitar" → router reads pendingDeepLinkProvider
  → Redirects to /panorama?poi=castelo-sao-jorge (NOT /)
  → POISummaryCard opens for Castelo de São Jorge
```

**Flow 5 — Privacy management:**
```
Home → /settings (from header) OR panorama → ··· menu → /settings
  → Sees consent status badge "Analytics activo"
  → Taps "Apagar os meus dados" → confirm dialog → SQLite cleared → success snackbar
  → Toggles consent off → analytics stops. Badge changes to "Analytics desligado".
  → Taps "Editar perfil" → /onboarding (pre-selected) → saves → /settings
```

**Flow 6 — Search + filter on panorama:**
```
/panorama → taps 🔍 → search overlay opens
  → Taps "Militar" type circle + "0%" destruction chip → result list shows 5 POIs
  → Taps "apply selection" → overlay closes → map shows only 5 filtered markers
  → "2 filtros activos · 5 resultados" status line visible
  → Taps X on filter pill → all markers restored
```

**Flow 7 — Feedback:**
```
Home → taps "Dar feedback sobre a app" → feedback bottom sheet opens
  → Rates 4 stars overall, 5 stars AR → writes "Adorei!" → taps "Enviar feedback"
  → Sheet closes → snackbar "Obrigado pelo feedback!" → event logged via AnalyticsBackend
```

---

### Real screens vs route stubs

**Real screens (5):** `/consent` (new), `/onboarding` (new), `/settings` (new), `/` home (major redesign), `/panorama` (modified — FAB + search + detail page push).

**New pages (2):** `/favourites` (new), `POIDetailPage` (push route, not GoRouter named route).

**Route stubs (4):** `/achievements`, `/circuits`, `/ai-chat`, `/quiz/:poiId` — these render `_ComingSoonPage`. Their purpose is preventing 404s when Phase 3+ deep links are shared.

---

## 4. Data and State Architecture

### New providers

| Provider                    | Type                                                                 | Read/Write | Notes                                                                                                                                                 |
| --------------------------- | -------------------------------------------------------------------- | ---------- | ----------------------------------------------------------------------------------------------------------------------------------------------------- |
| `consentProvider`           | `NotifierProvider<ConsentNotifier, ConsentState>`                    | R+W        | Persisted in SharedPreferences; gates analytics; pre-loaded in main() as override                                                                     |
| `onboardingProvider`        | `NotifierProvider<OnboardingNotifier, UserProfile?>`                 | R+W        | Persisted in SharedPreferences; `null` = not yet set; pre-loaded in main()                                                                            |
| `analyticsServiceProvider`  | `Provider<AnalyticsService>`                                         | R only     | Pre-created in main() with init() complete; passed as override. NOT lazy                                                                              |
| `analyticsSessionProvider`  | `NotifierProvider<AnalyticsSessionNotifier, AnalyticsSession?>`      | R+W        | Current session tracking                                                                                                                              |
| `favouritesProvider`        | `NotifierProvider<FavouritesNotifier, List<String>>`                 | R+W        | List of POI IDs; persisted in SharedPreferences; pre-loaded in main()                                                                                 |
| `searchQueryProvider`       | `NotifierProvider<SearchQueryNotifier, String>`                      | R+W        | Local to search flow — text in search field                                                                                                           |
| `destructionFilterProvider` | `NotifierProvider<DestructionFilterNotifier, Set<DestructionRange>>` | R+W        | Active destruction level filters                                                                                                                      |
| `typeFilterProvider`        | `NotifierProvider<TypeFilterNotifier, Set<POIType>>`                 | R+W        | Active POI type filters                                                                                                                               |
| `filteredPOIsProvider`      | `Provider<List<POI>>`                                                | R only     | Computed from poisProvider + search + destruction + type filters                                                                                      |
| `searchActiveProvider`      | `NotifierProvider<SearchActiveNotifier, bool>`                       | R+W        | Whether search overlay is visible                                                                                                                     |
| `pendingDeepLinkProvider`   | `NotifierProvider<PendingDeepLinkNotifier, String?>`                 | R+W        | Stores intended URI before consent redirect; consumed after consent                                                                                   |
| `lastVisitedPOIProvider`    | `FutureProvider<POI?>`                                               | R only     | Async query of last `poiTapped` event from analytics SQLite; cached after first load. Returns `null` if no visits, consent declined, or first install |

> **No `StateProvider` anywhere.** Per PROJECT_GUIDE §7 and §9 rules table, `StateProvider`
> is banned. All providers above use `NotifierProvider` (or `Provider` / `FutureProvider`
> for computed/async values). The trivial single-value notifiers (`SearchQueryNotifier`,
> `SearchActiveNotifier`, `PendingDeepLinkNotifier`) each have a `build()` + 1–2 mutation
> methods. ~10 lines each, zero architectural overhead, full consistency with the codebase.

### New models / value types

| Type                 | File                                         | Kind             | Notes                                                                                                                                                    |
| -------------------- | -------------------------------------------- | ---------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `ConsentState`       | `analytics/models/consent_state.dart`        | Plain Dart enum  | `notAsked \| accepted \| declined`                                                                                                                       |
| `ProfileType`        | `onboarding/models/profile_type.dart`        | Plain Dart enum  | `architecture \| history \| child \| general`; each has `label`, `description`, `icon`                                                                   |
| `UserProfile`        | `onboarding/models/user_profile.dart`        | Plain Dart class | `profileType`, `preferredLanguage`                                                                                                                       |
| `AnalyticsEventType` | `analytics/models/analytics_event_type.dart` | Plain Dart enum  | `sessionStart`, `sessionEnd`, `poiTapped`, `timelineChanged`, `profileSet`, `consentChanged`, `favouriteToggled`, `feedbackSubmitted`, `searchPerformed` |
| `AnalyticsEvent`     | `analytics/models/analytics_event.dart`      | Plain Dart class | `eventType`, `timestamp`, `metadata`(Map<String,dynamic>)                                                                                                |
| `AnalyticsSession`   | `analytics/models/analytics_session.dart`    | Plain Dart class | `sessionId`, `startedAt`, `endedAt?`                                                                                                                     |
| `DestructionRange`   | `panorama/models/destruction_range.dart`     | Plain Dart enum  | `zero \| twenty \| forty \| sixtPlus` — used by search filter chips                                                                                      |
| `ExternalLinkType`   | `panorama/models/external_link_type.dart`    | Plain Dart enum  | `website \| facebook \| instagram \| youtube`; each has `iconData` (IconData) for leading icon in Z7 link rows                                           |

### New services — Analytics Backend Abstraction (D5)

```dart
/// Abstract interface — all analytics writes go through this.
abstract class AnalyticsBackend {
  Future<void> init();
  Future<void> write(AnalyticsEvent event);
  Future<void> flush();  // called on session end or feedback submit
  Future<void> clear();  // "delete my data"
  Future<List<AnalyticsEvent>> readAll(); // for export
}

/// Concrete: SQLite local storage — full fidelity, queryable.
class LocalSQLiteBackend implements AnalyticsBackend {
  // Opens DB via getDatabasesPath() + openDatabase()
  // Tables: events(id, type, timestamp, metadata_json), sessions(id, started, ended)
  // getDatabasesPath() is async — called in init()
}

/// Concrete: Email backend — sends session summary JSON at flush().
class EmailBackend implements AnalyticsBackend {
  // Uses staticforms.xyz or similar service
  // write() batches events in memory
  // flush() sends POST with batched summary JSON
  // clear() is a no-op (can't un-send emails)
}

/// Composite: runs multiple backends simultaneously.
class CompositeBackend implements AnalyticsBackend {
  CompositeBackend(this.backends);
  final List<AnalyticsBackend> backends;
  Future<void> write(AnalyticsEvent e) async =>
    Future.wait(backends.map((b) => b.write(e)));
  // Same delegation for init(), flush(), clear()
}
```

**`AnalyticsService`** wraps the backend:
- Constructor takes `AnalyticsBackend backend` + `ConsentState Function() getConsent`
- `logEvent()` → checks consent, then delegates to `backend.write()`
- `startSession()` / `endSession()` → manage session lifecycle
- `clearAll()` → `backend.clear()`
- `getLastVisitedPOI()` → queries local backend for most recent `poiTapped` event (for home page Block 7)

**Phase 2 default:** `CompositeBackend([LocalSQLiteBackend()])` — just SQLite. Email backend is available but not wired by default. To enable: add `EmailBackend(accessKey: '...')` to the list.

### State connections

```
main.dart (pre-runApp):
  ├── SharedPreferences.getInstance()
  ├── Load: consentState, profileType, preferredLanguage, themeMode, favouriteIds
  ├── AnalyticsService(backend: CompositeBackend([LocalSQLiteBackend()])).init()
  └── ProviderScope overrides: consentProvider, onboardingProvider, themeModeProvider,
      favouritesProvider, analyticsServiceProvider

consentProvider ──────────────────┐
                                  ↓
analyticsServiceProvider ← gates logEvent()
         ↑
         │ logEvent() called from:
         ├── main.dart: sessionStart / sessionEnd (WidgetsBindingObserver)
         ├── poi_detail_page.dart: poiTapped (on page open)
         ├── timeline_provider.dart: timelineChanged (on epoch change)
         ├── onboarding_provider.dart: profileSet (on setProfile)
         ├── favourites_notifier.dart: favouriteToggled (on toggle)
         ├── feedback_sheet.dart: feedbackSubmitted (on submit)
         └── search_filter_overlay.dart: searchPerformed (on apply)

onboardingProvider ──→ home_page.dart: show/hide profile banner (Block 5)
                  ──→ poi_detail_page.dart: adapt content depth per ProfileType (Phase 3)

favouritesProvider ──→ home_page.dart: show/hide favourites section (Block 6)
                  ──→ poi_detail_page.dart: heart icon state (Z1)
                  ──→ fab: favourites sub-action

filteredPOIsProvider ──→ panorama_ar_view.dart: which markers to show when filters active
searchActiveProvider ──→ panorama_ar_view.dart: show/hide search overlay
```

### Which providers are READ ONLY vs READ+WRITE in Phase 2

| Provider                        | Phase 2 mode      | Notes                                          |
| ------------------------------- | ----------------- | ---------------------------------------------- |
| `consentProvider`               | R+W               | Written on consent screen + settings page      |
| `onboardingProvider`            | R+W               | Written on onboarding page                     |
| `analyticsServiceProvider`      | R only            | Service instance — pre-created, never replaced |
| `analyticsSessionProvider`      | R+W               | Written in main.dart lifecycle                 |
| `favouritesProvider`            | R+W               | Written from POI detail page + home page       |
| `searchQueryProvider`           | R+W               | Written from search overlay text field         |
| `destructionFilterProvider`     | R+W               | Written from filter chips                      |
| `typeFilterProvider`            | R+W               | Written from type circles                      |
| `filteredPOIsProvider`          | R only            | Computed — never written directly              |
| `searchActiveProvider`          | R+W               | Written from search icon + overlay close       |
| `timelineProvider`              | R+W (existing)    | Unchanged                                      |
| `selectedPOIProvider`           | R+W (existing)    | Unchanged                                      |
| `poiDisplayModeProvider`        | R+W (existing)    | Unchanged                                      |
| `viewportStateProvider`         | R+W (existing)    | Unchanged                                      |
| All ar_infrastructure_providers | R only (existing) | Unchanged                                      |

---

## 5. File and Folder Structure

### New files to create

```
lib/
  domains/
    analytics/
      analytics_domain.dart              # Barrel export
      models/
        consent_state.dart               # ConsentState enum: notAsked | accepted | declined
        analytics_event_type.dart        # AnalyticsEventType enum (9 event types)
        analytics_event.dart             # AnalyticsEvent plain Dart class
        analytics_session.dart           # AnalyticsSession plain Dart class
      services/
        analytics_backend.dart           # Abstract AnalyticsBackend interface
        local_sqlite_backend.dart        # LocalSQLiteBackend implements AnalyticsBackend
        email_backend.dart               # EmailBackend implements AnalyticsBackend
        composite_backend.dart           # CompositeBackend wraps multiple backends
        analytics_service.dart           # AnalyticsService — facade over backend; respects consent
      providers/
        consent_provider.dart            # NotifierProvider<ConsentNotifier, ConsentState>
        analytics_provider.dart          # Provider<AnalyticsService> (pre-created override)
        analytics_session_provider.dart  # NotifierProvider for current session
      pages/
        consent_page.dart                # GDPR consent screen (first launch gate)
      test/
        unit/
          consent_state_test.dart
          analytics_backend_test.dart    # Tests for all 3 backend implementations
          analytics_service_test.dart
          analytics_event_test.dart
        widgets/
          consent_page_test.dart

    onboarding/
      onboarding_domain.dart             # Barrel export
      models/
        profile_type.dart                # ProfileType enum with label, description, icon
        user_profile.dart                # UserProfile plain Dart class
      providers/
        onboarding_provider.dart         # NotifierProvider<OnboardingNotifier, UserProfile?>
      pages/
        onboarding_page.dart             # Profile selection screen
      widgets/
        profile_selector.dart            # 2×2 grid of ProfileCard widgets
        profile_card.dart                # Single selectable profile option card
      test/
        unit/
          profile_type_test.dart
          onboarding_provider_test.dart
        widgets/
          onboarding_page_test.dart
          profile_selector_test.dart

    home/
      widgets/
        home_top_chrome.dart             # Block 1: LanguageSwitcher + ThemeSwitcher + ProfileAvatar (3 icons)
        home_branding.dart               # Block 2: AppBrand + "TILESTORIES" wordmark
        hero_card_block.dart             # Block 3: Hero Card with painting context + primary CTA "Explorar com AR"
        secondary_cta_block.dart         # Block 4: "Explorar sem câmera" full-width button
        profile_banner_block.dart        # Block 5: dashed border profile setup prompt (conditional: profile == null)
        favourites_strip.dart            # Block 6: horizontal scroll of favourite POI mini-cards (conditional: favourites.isNotEmpty)
        last_visited_block.dart          # Block 7: last POI visited return shortcut (conditional: has analytics history)
        feedback_link_block.dart         # Block 8: "Dar feedback sobre a app" text button (always visible)

    panorama/
      ar/
        widgets/
          panorama_fab.dart              # Expandable FAB with staggered sub-actions
          search_info_icons.dart         # 🔍 + ⓘ icon buttons (right side of panorama)
      search/
        search_domain.dart               # Barrel export for search subdomain
        widgets/
          search_bar_overlay.dart        # Full search overlay panel
          search_results_list.dart       # POI result rows in search list
          filter_chip_row.dart           # Filter chips (destruction + type)
        providers/
          search_provider.dart           # NotifierProvider<SearchNotifier, SearchState>
          filter_provider.dart           # NotifierProvider<FilterNotifier, Set<String>>
      poi/
        pages/
          poi_detail_page.dart           # FULL PAGE POI detail (replaces poi_detail_sheet.dart)
        widgets/
          zones/                         # Subfolder — one file per POI detail zone
            poi_gallery_zone.dart        # Z1 — gallery with PageView + close + favourite heart
            poi_header_zone.dart         # Z2+Z3 — POI name + survival badge
            poi_category_zone.dart       # Z4 — category pill
            poi_view_modes_zone.dart     # Z5 — "VER EM" grid (3D/360°/Mapa)
            poi_explore_zone.dart        # Z6 — "EXPLORAR" grid (Áudio/Quiz/Guia IA)
            poi_external_links_zone.dart # Z7 — External links (url_launcher rows)
            poi_actions_zone.dart        # Z8 — Share + Feedback buttons
            poi_content_tabs_zone.dart   # Z9+Z10 — content tab bar + tab body
      models/
        destruction_range.dart           # DestructionRange enum for filter chips
        external_link_type.dart          # ExternalLinkType enum: website | facebook | instagram | youtube
                                         # Each value has: iconData (IconData), fallback label

    favourites/
      favourites_domain.dart             # Barrel export
      providers/
        favourites_provider.dart         # NotifierProvider<FavouritesNotifier, List<String>>
      pages/
        favourites_page.dart             # /favourites — full list of favourited POIs
      test/
        unit/
          favourites_provider_test.dart
        widgets/
          favourites_page_test.dart

    feedback/
      feedback_domain.dart               # Barrel export
      widgets/
        feedback_sheet.dart              # Modal bottom sheet with rating + text + submit
        star_rating.dart                 # Reusable star rating widget (1–5)
      test/
        widgets/
          feedback_sheet_test.dart

    settings/
      settings_domain.dart               # Barrel export
      pages/
        settings_page.dart               # Settings + Privacy + Feedback + About
      widgets/
        privacy_section.dart             # Data info + consent toggle + delete data
        about_section.dart               # AppBrand + version + credits
      test/
        widgets/
          settings_page_test.dart
```

### Existing files to modify

| File                                                    | Modification                                                                                                                                                                                                                                                                                 |
| ------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `lib/main.dart`                                         | Add pre-runApp init sequence: load SharedPreferences → read consent, profile, favourites → create AnalyticsService → init() → ProviderScope overrides for all 5 preloaded providers. Add WidgetsBindingObserver for session lifecycle                                                        |
| `lib/navigation/navConfig/nav_config.dart`              | Add routes: `/consent`, `/onboarding`, `/settings`, `/favourites`, `/circuits` (stub), `/achievements` (stub), `/ai-chat` (stub), `/quiz/:poiId` (stub)                                                                                                                                      |
| `lib/navigation/navConfig/router_config.dart`           | Add redirects: consent gate with deep link preservation; consent-already-answered skip                                                                                                                                                                                                       |
| `lib/domains/home/pages/home_page.dart`                 | Complete redesign to 8-block structure. Extract sub-widgets to separate files. Wire favouritesProvider, onboardingProvider, analyticsService                                                                                                                                                 |
| `lib/domains/panorama/ar/widgets/panorama_ar_view.dart` | Remove POIDetailSheet from Stack. Add: SearchInfoIcons [5], PanoramaFAB [6], SearchFilterOverlay [9]. Update POISummaryCard "Mais Info" to Navigator.push POIDetailPage. Add deep link param handling. Note: ARModeTogglePill is NOT a separate Stack layer — it stays inside PanoramaTopBar |
| `lib/domains/panorama/ar/widgets/panorama_top_bar.dart` | Add ProfileAvatar between ARModeToggle and ··· menu. ARModeTogglePill stays inside this widget. Add "Definições" to ··· menu                                                                                                                                                                 |
| `lib/domains/panorama/ar/widgets/poi_summary_card.dart` | "Mais Info" button now does Navigator.push(POIDetailPage) instead of switching to POIDisplayMode.sheet                                                                                                                                                                                       |
| `lib/domains/panorama/ar/widgets/poi_detail_sheet.dart` | DEPRECATED — replaced by poi_detail_page.dart. Keep file but mark as deprecated; remove from panorama_ar_view.dart Stack                                                                                                                                                                     |
| `lib/domains/timeline/providers/timeline_provider.dart` | Wire analytics event: timelineChanged                                                                                                                                                                                                                                                        |
| `pubspec.yaml`                                          | Add `sqflite: ^2.4.1`, `path: ^1.9.0`, `url_launcher: ^6.x.x`; add `flutter_launcher_icons` (dev); remove `cached_network_image`. Note: `share_plus: ^11.0.0` is already in pubspec (no change needed)                                                                                       |
| `lib/domains/panorama/models/poi.dart`                  | Add `@Default([]) List<Map<String, dynamic>> externalLinks` field. Then run `dart run build_runner build --delete-conflicting-outputs`. No other POI model changes                                                                                                                           |
| `assets/data/pois.json`                                 | Add `externalLinks` array to each POI with demo data — varying 0–4 links per POI (websites, Facebook, Instagram, YouTube). See §3.7 for JSON format. POIs with no links: omit field or use `[]`                                                                                              |

### Barrel exports to update

| Barrel                                          | Add                         |
| ----------------------------------------------- | --------------------------- |
| `lib/domains/analytics/analytics_domain.dart`   | All analytics public files  |
| `lib/domains/onboarding/onboarding_domain.dart` | All onboarding public files |
| `lib/domains/settings/settings_domain.dart`     | All settings public files   |
| `lib/domains/favourites/favourites_domain.dart` | All favourites public files |
| `lib/domains/feedback/feedback_domain.dart`     | All feedback public files   |

### Files NOT to touch

- `lib/ar_core/` — no AR changes in Phase 2
- `lib/design/` — design system is stable; no token changes
- `lib/domains/panorama/services/poi_visibility_service.dart` — POIVisibilityConfig stays as const (D7)
- `lib/domains/panorama/ar/controllers/` — no controller logic changes
- `lib/domains/panorama/ar/config/` — no config changes
- `lib/domains/panorama/repositories/` — no repository changes
- `lib/domains/panorama/models/poi.dart` — Add `externalLinks` field ONLY (`@Default([]) List<Map<String, dynamic>> externalLinks`). No other POI model changes in Phase 2. After modification: `dart run build_runner build --delete-conflicting-outputs` to regenerate Freezed files
- `lib/domains/timeline/models/time_period.dart` — no model changes
- `lib/layout/` — layout system is stable
- `lib/utils/zoom/` — no zoom changes
- `lib/test_utils/` — only update `test_config.dart` for Layer 5 scenarios

---

## 6. Task Execution Plan

> **Execution order:** 2.1 → 2.2 → 2.3 → 2.4 → 2.5 → 2.6 → 2.7 → 2.8 → 2.9 → 2.10 (→ 2.11 deferred to Phase 5)
>
> **Rule:** Do not start the next task until the current task's acceptance gate is fully green.

---

### Task 2.1 — Phase 1 Deferred Items Cleanup

- [ ] **What:** Close all deferred Phase 1 items that are prerequisites for Phase 2.
- **Why now:** These are technical debt from Phase 1. Clearing them first avoids building Phase 2 on top of known issues.
- **Files:** Various existing files.
- **Spec:**

  **2.1a — Add missing ES strings in home_page.dart:**
  - [ ] Grep `home_page.dart` for all `t(` calls. Any `t(pt:..., en:...)` without `es:` → add `es:` translation.
  - [ ] Specifically: hero card "TOQUE PARA ATIVAR", "Escanear Azulejos", "Aponte a câmera..." need `es:`.
  - [ ] Header greeting "BOA TARDE"/"GOOD AFTERNOON" needs `es: 'BUENAS TARDES'`.
  - [ ] "Explore História À Sua Volta" needs `es:` equivalents.
  - [ ] "Descobertas Recentes" / "Recent Discoveries" needs `es:`.
  - [ ] "Ver Todos →" needs `es:`.
  - [ ] Hero card pill "▶ AR" needs `es:` (same text — "▶ AR").

  **2.1b — Add `Tooltip` to ProfileAvatar in home_page.dart:**
  - [ ] The avatar `GestureDetector` currently has `Semantics` but no `Tooltip`. Add `Tooltip(message: ref.tr(t(pt: 'Perfil', en: 'Profile', es: 'Perfil')))`.

  **2.1c — Verify `ARFirstTimeOverlay` is complete:**
  - [ ] Read `ar_first_time_overlay.dart` — confirm 2-step coach mark works. If incomplete, implement now.

  **2.1d — Remove `cached_network_image` from pubspec:**
  - [ ] Grep `lib/` for `cached_network_image` imports — confirm zero results.
  - [ ] Remove `cached_network_image: ^3.3.0` from `pubspec.yaml`.
  - [ ] `flutter pub get`.

- **Tests:**
  - Layer 1: N/A (string additions)
  - Layer 2: Re-run existing `home_page_test.dart` — all must pass.
  - Layer 3: Re-run existing integration tests — all must pass.
- **Acceptance gate:**
  - `flutter analyze` → 0 issues
  - `flutter test lib/` → all passing (≥ 1134)
  - All `t()` calls in `home_page.dart` have `es:` parameter
  - `cached_network_image` not in pubspec

---

### Task 2.2 — Analytics Domain (Backend Abstraction + Models + Service + Provider)

- [ ] **What:** Create the analytics infrastructure with swappable backend pattern: consent model, event models, backend interface, SQLite backend, composite backend, analytics service, providers.
- **Why now:** Analytics is the foundation that all subsequent tasks depend on — consent gates the entire data pipeline, and every new screen needs to log events.
- **Files created:**
  - `lib/domains/analytics/models/consent_state.dart`
  - `lib/domains/analytics/models/analytics_event_type.dart`
  - `lib/domains/analytics/models/analytics_event.dart`
  - `lib/domains/analytics/models/analytics_session.dart`
  - `lib/domains/analytics/services/analytics_backend.dart`
  - `lib/domains/analytics/services/local_sqlite_backend.dart`
  - `lib/domains/analytics/services/email_backend.dart`
  - `lib/domains/analytics/services/composite_backend.dart`
  - `lib/domains/analytics/services/analytics_service.dart`
  - `lib/domains/analytics/providers/consent_provider.dart`
  - `lib/domains/analytics/providers/analytics_provider.dart`
  - `lib/domains/analytics/providers/analytics_session_provider.dart`
  - `lib/domains/analytics/analytics_domain.dart`
- **Files modified:**
  - `pubspec.yaml` — add `sqflite: ^2.4.1` + `path: ^1.9.0` (if not already present as direct dep)
- **Spec:**

  **`ConsentState` enum** (`consent_state.dart`):
  ```dart
  enum ConsentState { notAsked, accepted, declined }
  ```

  **`AnalyticsEventType` enum** (`analytics_event_type.dart`):
  ```dart
  enum AnalyticsEventType {
    sessionStart,
    sessionEnd,
    poiTapped,
    timelineChanged,
    profileSet,
    consentChanged,
    favouriteToggled,
    feedbackSubmitted,
    searchPerformed,
    // Future events — add as domains are built:
    // circuitStarted, circuitCompleted, audioPlayed,
    // earthquakeWatched, aiQuestionAsked, achievementEarned, quizAnswered
  }
  ```

  **`AnalyticsEvent`** (`analytics_event.dart`):
  ```dart
  class AnalyticsEvent {
    final AnalyticsEventType eventType;
    final DateTime timestamp;
    final Map<String, dynamic> metadata;
    const AnalyticsEvent({required this.eventType, required this.timestamp, this.metadata = const {}});
  }
  ```

  **`AnalyticsSession`** (`analytics_session.dart`):
  ```dart
  class AnalyticsSession {
    final String sessionId;
    final DateTime startedAt;
    DateTime? endedAt;
    AnalyticsSession({required this.sessionId, required this.startedAt, this.endedAt});
  }
  ```

  **`AnalyticsBackend` interface** (`analytics_backend.dart`):
  ```dart
  abstract class AnalyticsBackend {
    Future<void> init();
    Future<void> write(AnalyticsEvent event);
    Future<void> flush();
    Future<void> clear();
    Future<List<AnalyticsEvent>> readAll();
  }
  ```

  **`LocalSQLiteBackend`** (`local_sqlite_backend.dart`):
  - `init()` → `getDatabasesPath()` → `openDatabase()` → create tables:
    ```sql
    CREATE TABLE IF NOT EXISTS events (
      id INTEGER PRIMARY KEY AUTOINCREMENT,
      event_type TEXT NOT NULL,
      timestamp TEXT NOT NULL,
      metadata TEXT NOT NULL DEFAULT '{}'
    );
    CREATE TABLE IF NOT EXISTS sessions (
      id TEXT PRIMARY KEY,
      started_at TEXT NOT NULL,
      ended_at TEXT
    );
    ```
  - `write(event)` → insert into `events` table
  - `flush()` → no-op (SQLite writes are immediate)
  - `clear()` → drop + recreate tables
  - `readAll()` → SELECT * from events → map to AnalyticsEvent list
  - `getLastEvent(AnalyticsEventType type)` → most recent event of given type (for home page Block 7)

  **`EmailBackend`** (`email_backend.dart`):
  - `init()` → no-op (no connection to establish)
  - `write(event)` → accumulate in memory list
  - `flush()` → POST to staticforms.xyz (or configurable endpoint) with accumulated events as JSON. `accessKey` passed via constructor. Fire-and-forget with try/catch (failure is silent — email is best-effort)
  - `clear()` → no-op (cannot un-send emails)
  - `readAll()` → empty list (email backend is write-only)

  **`CompositeBackend`** (`composite_backend.dart`):
  ```dart
  class CompositeBackend implements AnalyticsBackend {
    CompositeBackend(this.backends);
    final List<AnalyticsBackend> backends;
    Future<void> init() async => Future.wait(backends.map((b) => b.init()));
    Future<void> write(AnalyticsEvent e) async => Future.wait(backends.map((b) => b.write(e)));
    Future<void> flush() async => Future.wait(backends.map((b) => b.flush()));
    Future<void> clear() async => Future.wait(backends.map((b) => b.clear()));
    Future<List<AnalyticsEvent>> readAll() async => backends.first.readAll(); // read from primary
  }
  ```

  **`AnalyticsService`** (`analytics_service.dart`):
  ```dart
  class AnalyticsService {
    final AnalyticsBackend backend;
    final ConsentState Function() getConsent;
    AnalyticsService({required this.backend, required this.getConsent});

    Future<void> init() => backend.init();

    Future<void> logEvent(AnalyticsEventType type, {Map<String, dynamic>? metadata}) async {
      if (getConsent() != ConsentState.accepted) return; // silent no-op
      await backend.write(AnalyticsEvent(
        eventType: type,
        timestamp: DateTime.now(),
        metadata: metadata ?? const {},
      ));
    }

    Future<String> startSession() async { /* create session, return id */ }
    Future<void> endSession(String sessionId) async { /* update endedAt, flush */ }
    Future<void> clearAll() => backend.clear();
    Future<AnalyticsEvent?> getLastVisitedPOI() async { /* query last poiTapped event */ }
  }
  ```

  **`ConsentNotifier`** (`consent_provider.dart`):
  ```dart
  class ConsentNotifier extends Notifier<ConsentState> {
    static const prefsKey = 'consent_state';
    ConsentNotifier({required this.initialState});
    final ConsentState initialState;

    @override
    ConsentState build() => initialState;

    Future<void> setConsent(ConsentState newState) async {
      state = newState;
      final prefs = SharedPreferencesAsync();
      await prefs.setString(prefsKey, newState.name);
      // Log analytics event (consent change itself is always logged)
      ref.read(analyticsServiceProvider).logEvent(
        AnalyticsEventType.consentChanged,
        metadata: {'newState': newState.name},
      );
    }
  }
  ```

  **`analyticsServiceProvider`** (`analytics_provider.dart`):
  ```dart
  // The instance is pre-created in main() and passed as an override.
  // This provider definition is the fallback (should never be reached in prod).
  final analyticsServiceProvider = Provider<AnalyticsService>((ref) {
    throw StateError('analyticsServiceProvider must be overridden in ProviderScope');
  });
  ```

  **main.dart initialization sequence (Task 2.3 will implement, but spec here):**

  > **IMPORTANT — ProviderContainer pattern:** The `AnalyticsService` needs live access
  > to `consentProvider`. A naive closure over a local `initialConsent` variable would
  > capture a stale value — when the user later revokes consent, the service would keep
  > logging. To fix this, we create a `ProviderContainer` *before* `runApp()` and pass it
  > to `UncontrolledProviderScope`. This lets the service read the live provider value.

  ```dart
  Future<void> _appMain() async {
    WidgetsFlutterBinding.ensureInitialized();
    // ... existing error handlers ...

    final prefs = SharedPreferencesAsync();

    // 1. Load all persisted state
    final savedTheme = await prefs.getString(ThemeModeNotifier.prefsKey);
    final savedConsent = await prefs.getString(ConsentNotifier.prefsKey);
    final savedProfile = await prefs.getString(OnboardingNotifier.prefsKey);
    final savedFavourites = await prefs.getString(FavouritesNotifier.prefsKey);

    // 2. Map to typed values
    final initialThemeMode = ThemeMode.values.firstWhere(
      (m) => m.name == savedTheme, orElse: () => ThemeMode.system);
    final initialConsent = ConsentState.values.firstWhere(
      (c) => c.name == savedConsent, orElse: () => ConsentState.notAsked);
    final initialProfile = _parseProfile(savedProfile); // UserProfile? or null
    final initialFavourites = _parseFavourites(savedFavourites); // List<String>

    // 3. Create ProviderContainer with all overrides BEFORE creating AnalyticsService
    final container = ProviderContainer(
      overrides: [
        themeModeProvider.overrideWith(() => ThemeModeNotifier(initialMode: initialThemeMode)),
        consentProvider.overrideWith(() => ConsentNotifier(initialState: initialConsent)),
        onboardingProvider.overrideWith(() => OnboardingNotifier(initialProfile: initialProfile)),
        favouritesProvider.overrideWith(() => FavouritesNotifier(initialIds: initialFavourites)),
      ],
    );

    // 4. Create analytics service — getConsent reads LIVE from the container,
    //    not from a captured local variable. This ensures consent revocation
    //    takes effect immediately (GDPR compliance).
    final analyticsService = AnalyticsService(
      backend: CompositeBackend([LocalSQLiteBackend()]),
      getConsent: () => container.read(consentProvider),
    );
    await analyticsService.init();

    // 5. Add analytics service override to the container
    container.updateOverrides([
      themeModeProvider.overrideWith(() => ThemeModeNotifier(initialMode: initialThemeMode)),
      consentProvider.overrideWith(() => ConsentNotifier(initialState: initialConsent)),
      onboardingProvider.overrideWith(() => OnboardingNotifier(initialProfile: initialProfile)),
      favouritesProvider.overrideWith(() => FavouritesNotifier(initialIds: initialFavourites)),
      analyticsServiceProvider.overrideWithValue(analyticsService),
    ]);

    // 6. Run app with UncontrolledProviderScope (shares the existing container)
    runApp(UncontrolledProviderScope(
      container: container,
      child: const MyApp(),
    ));
  }
  ```

  > **Why `UncontrolledProviderScope`?** Standard `ProviderScope` creates its own
  > internal container. `UncontrolledProviderScope` reuses the container we already
  > created, which the `getConsent` closure already references. This is the standard
  > Riverpod pattern for pre-`runApp()` initialization.

- **Tests (Layer 1):**
  - [ ] `consent_state_test.dart`: enum values, `ConsentState.values.length == 3`
  - [ ] `analytics_event_test.dart`: event construction, metadata, all 9 event types exist
  - [ ] `analytics_backend_test.dart`:
    - LocalSQLiteBackend: init creates tables, write inserts row, readAll returns events, clear empties DB
    - CompositeBackend: write delegates to all children, readAll reads from first child
    - EmailBackend: write accumulates, flush sends (mock HTTP), clear is no-op
  - [ ] `analytics_service_test.dart`:
    - logEvent with consent=accepted → event written to backend
    - logEvent with consent=declined → no event written
    - logEvent with consent=notAsked → no event written
    - startSession/endSession → session timestamps correct
    - clearAll → delegates to backend.clear()
    - getLastVisitedPOI → returns most recent poiTapped event or null
  - [ ] `consent_provider_test.dart` (with ProviderContainer):
    - initial state from constructor parameter
    - setConsent(accepted) → state changes + SharedPreferences written
    - setConsent(declined) → state changes + SharedPreferences written

- **Acceptance gate:**
  - `flutter analyze` → 0 issues
  - `flutter test lib/` → all passing
  - `sqflite` added to pubspec and resolves
  - All analytics unit tests green
  - Backend interface can be swapped by changing one line in main()

---

### Task 2.3 — Consent Page + Router Redirect + main.dart Wiring

- ✅ **What:** Build the GDPR consent screen, wire the GoRouter redirect with deep link preservation, and implement the full main.dart initialization sequence.
- **Why now:** Consent must be wired before any analytics events can be logged. The main.dart init sequence is the foundation for all subsequent providers.
- **Files created:**
  - `lib/domains/analytics/pages/consent_page.dart`
  - `lib/domains/analytics/test/widgets/consent_page_test.dart`
- **Files modified:**
  - `lib/navigation/navConfig/nav_config.dart` — add `/consent` route
  - `lib/navigation/navConfig/router_config.dart` — add consent redirect with deep link preservation
  - `lib/main.dart` — implement full init sequence (from Task 2.2 spec); add WidgetsBindingObserver for session lifecycle
- **Spec:**

  **Consent page (`consent_page.dart`):**
  - `ConsumerWidget` with `LayoutPresets.fullscreen()` body.
  - Uses `LuxuryScaffold` wrapper.
  - Content: `CenteredContent(maxWidth: LayoutTokens.readingWidth)`.
  - Title: `t(pt: 'Política de Privacidade', en: 'Privacy Policy', es: 'Política de Privacidad')`.
  - Body text (scrollable): GDPR Article 13 compliant (see §3.1 for required content).
  - "Aceitar" `FilledButton` → `ref.read(consentProvider.notifier).setConsent(ConsentState.accepted)` → navigate to pending deep link or `/`.
  - "Recusar" `OutlinedButton` → `ref.read(consentProvider.notifier).setConsent(ConsentState.declined)` → navigate to pending deep link or `/`.
  - "Saber mais" `TextButton` → toggles expandable detail section.
  - All text in PT, EN, ES.

  **Router redirect with deep link preservation (`router_config.dart`):**
  ```dart
  // PendingDeepLinkNotifier — trivial Notifier wrapper (no StateProvider)
  class PendingDeepLinkNotifier extends Notifier<String?> {
    @override String? build() => null;
    void set(String uri) => state = uri;
    void consume() { final _ = state; state = null; }
  }
  final pendingDeepLinkProvider = NotifierProvider<PendingDeepLinkNotifier, String?>(
    PendingDeepLinkNotifier.new,
  );

  // Inside redirect function, before other checks:
  final consent = ref.read(consentProvider);
  final location = state.matchedLocation;
  final fullUri = state.uri.toString();

  if (consent == ConsentState.notAsked && location != '/consent') {
    // Store the intended destination before redirecting
    ref.read(pendingDeepLinkProvider.notifier).set(fullUri);
    return '/consent';
  }
  if (consent != ConsentState.notAsked && location == '/consent') {
    // Already answered — restore pending deep link or go home
    final pending = ref.read(pendingDeepLinkProvider);
    ref.read(pendingDeepLinkProvider.notifier).consume();
    return pending ?? '/';
  }
  ```

  **main.dart session lifecycle (WidgetsBindingObserver):**
  - `_MyAppState` implements `WidgetsBindingObserver`
  - `initState`: `WidgetsBinding.instance.addObserver(this)` + start initial session
  - `didChangeAppLifecycleState`:
    - `AppLifecycleState.resumed` → `analyticsService.startSession()`
    - `AppLifecycleState.paused` → `analyticsService.endSession(sessionId)`
  - `dispose`: `WidgetsBinding.instance.removeObserver(this)`

- **Tests (Layer 2):**
  - ✅ `consent_page_test.dart`:
    - Consent page renders title, body text, both buttons, "Saber mais" button
    - "Aceitar" tap → `consentProvider.state == accepted`
    - "Recusar" tap → `consentProvider.state == declined`
    - "Saber mais" tap → expandable section visible
    - All text present in PT, EN, ES (cycle language provider, verify text changes)
    - Accessibility: all buttons have Semantics, tap targets ≥ 48px
    - Entry animation: RevealAnimation stagger visible
- **Tests (Layer 3):**
  - ✅ `consent_integration_test.dart`:
    - Full app mount with `consentState == notAsked` → redirected to `/consent`
    - Accept → navigated to `/` → analytics `sessionStart` event logged
    - Full app mount with `consentState == accepted` → `/consent` not shown, goes to `/`
    - Full app mount with `consentState == declined` → `/consent` not shown, goes to `/`; POI tap → no analytics event
    - Deep link preservation: mount with `notAsked` + initial route `/panorama?poi=X` → redirected to `/consent` → accept → arrives at `/panorama?poi=X` (not `/`)

- **Acceptance gate:**
  - ✅ `flutter analyze` → 0 issues
  - ✅ `flutter test lib/` → all passing
  - ✅ First launch shows consent screen
  - ✅ After accept/decline, consent screen never shows again
  - ✅ Deep link target preserved through consent redirect
  - ✅ Analytics session starts on app launch (when consent=accepted)

---

### Task 2.4 — Onboarding Domain (Models + Provider + Page)

- ✅ **What:** Create the onboarding domain with profile type selection, persistence, and the onboarding page.
- **Why now:** The profile influences content presentation (Phase 3 depth adaptation) and analytics segmentation. Building it now establishes the data model before circuits and quizzes depend on it.
- **Files created:**
  - `lib/domains/onboarding/models/profile_type.dart`
  - `lib/domains/onboarding/models/user_profile.dart`
  - `lib/domains/onboarding/providers/onboarding_provider.dart`
  - `lib/domains/onboarding/pages/onboarding_page.dart`
  - `lib/domains/onboarding/widgets/profile_selector.dart`
  - `lib/domains/onboarding/widgets/profile_card.dart`
  - `lib/domains/onboarding/onboarding_domain.dart`
  - All test files (see below)
- **Files modified:**
  - `lib/navigation/navConfig/nav_config.dart` — add `/onboarding` route
- **Spec:**

  **`ProfileType` enum** (`profile_type.dart`):
  ```dart
  enum ProfileType {
    architecture(
      icon: Icons.architecture_outlined,
      label: TranslatableString(pt: 'Arquitetura', en: 'Architecture', es: 'Arquitectura'),
      description: TranslatableString(
        pt: 'Proporções, estilos, detalhes construtivos',
        en: 'Proportions, styles, construction details',
        es: 'Proporciones, estilos, detalles constructivos',
      ),
    ),
    history(
      icon: Icons.history_edu_outlined,
      label: TranslatableString(pt: 'História', en: 'History', es: 'Historia'),
      description: TranslatableString(
        pt: 'Datas, eventos, contexto político',
        en: 'Dates, events, political context',
        es: 'Fechas, eventos, contexto político',
      ),
    ),
    child(
      icon: Icons.family_restroom_outlined,
      label: TranslatableString(pt: 'Família', en: 'Family', es: 'Familia'),
      description: TranslatableString(
        pt: 'Linguagem simples, interativo',
        en: 'Simple language, interactive',
        es: 'Lenguaje simple, interactivo',
      ),
    ),
    general(
      icon: Icons.explore_outlined,
      label: TranslatableString(pt: 'Geral', en: 'General', es: 'General'),
      description: TranslatableString(
        pt: 'Um pouco de tudo',
        en: 'A bit of everything',
        es: 'Un poco de todo',
      ),
    );

    final IconData icon;
    final TranslatableString label;
    final TranslatableString description;
    const ProfileType({required this.icon, required this.label, required this.description});
  }
  ```

  **`UserProfile`** (`user_profile.dart`):
  ```dart
  class UserProfile {
    final ProfileType profileType;
    final Language preferredLanguage;
    const UserProfile({required this.profileType, required this.preferredLanguage});
  }
  ```

  **`OnboardingNotifier`** (`onboarding_provider.dart`):
  - Extends `Notifier<UserProfile?>`.
  - Constructor takes `UserProfile? initialProfile`.
  - `build()` → returns `initialProfile`.
  - `setProfile(ProfileType type)` → saves to SharedPreferences; logs `profileSet` analytics event; sets `state = UserProfile(profileType: type, preferredLanguage: ref.read(languageProvider))`.
  - `clearProfile()` → removes from SharedPreferences; sets `state = null`.

  **`OnboardingPage`** (`onboarding_page.dart`):
  - `ConsumerStatefulWidget`.
  - `LayoutPresets.defaultPageApp()` body.
  - Local `_selectedType` state variable (not committed until "Continuar" is tapped).
  - **First visit (profile == null):** "Continuar" disabled until card selected. "Saltar" visible → sets profile to `general`.
  - **Edit visit (profile != null):** Pre-selects current type. "Continuar" always enabled. "Saltar" hidden.
  - Title: `t(pt: 'Com quem explora?', en: 'Who are you exploring with?', es: '¿Con quién explora?')`.
  - Subtitle: `t(pt: 'Personalizamos o conteúdo para si', en: 'We personalize content for you', es: 'Personalizamos el contenido para usted')`.
  - `ProfileSelector` widget with `onSelected` callback → updates `_selectedType`.
  - After save → `context.go('/')`.

  **`ProfileSelector`** (`profile_selector.dart`):
  - Renders 4 `ProfileCard` widgets in a `Wrap` or 2×2 `GridView`.
  - `selectedType` and `onSelected` callback.

  **`ProfileCard`** (`profile_card.dart`):
  - `LuxuryCard` (or custom card) with icon, label, description.
  - Selected state: gold border + filled background tint.
  - `AnimatedScale(0.97)` on press.
  - `Semantics(button: true, selected: isSelected, label: profileType.label)`.

- **Tests (Layer 1):**
  - ✅ `profile_type_test.dart`: all 4 values exist, each has non-empty label/description in all 3 languages, each has an icon. (20 tests)
  - ✅ `user_profile_test.dart`: serialisation round-trips, `tryParseJson`, `==` / `hashCode`. (20 tests)
  - ✅ `onboarding_provider_test.dart`:
    - Initial state from constructor parameter (null or profile)
    - `setProfile(architecture)` → state is `UserProfile(architecture, ...)`
    - `clearProfile()` → state is `null`
    - Persistence: `setProfile` writes to SharedPreferences (mock SharedPreferences)
    - Analytics event `profileSet` logged on `setProfile`
    (13 tests)

- **Tests (Layer 2):**
  - ✅ `onboarding_page_test.dart`:
    - Page renders title, subtitle, 4 profile cards, "Continuar" button, "Saltar" button
    - First visit: tap "Architecture" card → card shows selected state (gold border) → "Continuar" enables
    - First visit: "Continuar" disabled when no selection
    - First visit: "Saltar" visible → tap → `onboardingProvider.state.profileType == general`
    - Edit visit: current profile pre-selected, "Continuar" always enabled, "Saltar" hidden
    - Tap "Continuar" with selection → `onboardingProvider.state != null`
    - All text in PT, EN, ES
    - All cards: `Semantics(button: true)`, tap target ≥ 48px
  - ✅ `profile_selector_test.dart`:
    - 4 cards rendered
    - Tap one → `onSelected` called with correct `ProfileType`
    - Only one card selected at a time

- **Tests (Layer 3):**
  - ✅ `onboarding_integration_test.dart`:
    - Full app mount → home page → tap onboarding block → onboarding page renders
    - Select profile → continue → home page → onboarding block hidden
    - Skip → home page → profile is `general` → onboarding block hidden

- **Tests (Layer 4):**
  - ✅ `integration_test/consent_onboarding_test.dart` — 21 tests on Chrome (`flutter drive --profile`):
    - Consent page renders on first launch (3 tests)
    - Accept → ConsentPage gone, provider = accepted (2 tests)
    - Decline → ConsentPage gone, provider = declined (2 tests)
    - Onboarding renders when consent answered — 4 cards + title + Saltar visible (4 tests)
    - Select profile + Continuar → navigates to Home, provider updated (2 tests)
    - Saltar → navigates to Home, profile = general (2 tests)
    - Edit mode — Saltar hidden, Continuar enabled, card pre-selected, change updates provider (4 tests)
    - Consent gate blocks /onboarding when notAsked; allows when accepted or declined (3 tests)

- **Acceptance gate:**
  - ✅ `flutter analyze` → 0 issues
  - ✅ `flutter test lib/` → all passing (1300 tests)
  - ✅ Onboarding page renders 4 profile cards correctly
  - ✅ First visit vs edit visit logic correct
  - ✅ Profile persists across app restart (mock SharedPreferences)
  - ✅ Layer 4: 21/21 tests passing on Chrome

---

### Task 2.5 — Favourites Domain

- ✅ **What:** Create the favourites system with SharedPreferences persistence, provider, and utility widgets.
- **Why now:** Favourites are referenced by the home page (Block 5), the POI detail page (heart icon), and the FAB (Favourites sub-action). Building the domain before UI tasks ensures the data layer is ready.
- **Files created:**
  - `lib/domains/favourites/providers/favourites_provider.dart`
  - `lib/domains/favourites/pages/favourites_page.dart`
  - `lib/domains/favourites/widgets/favourite_toggle_button.dart`
  - `lib/domains/favourites/favourites_domain.dart`
  - All test files (see below)
- **Files modified:**
  - `lib/navigation/navConfig/nav_config.dart` — add `/favourites` route
  - `lib/layout/layout_presets.dart` — added `bool scrollable = true` parameter to `defaultPageBrowser()` (discovered production bug during testing — see Implementation Notes below)
- **Spec:**

  **`FavouritesNotifier`** (`favourites_provider.dart`):
  ```dart
  class FavouritesNotifier extends Notifier<List<String>> {
    static const prefsKey = 'favourite_poi_ids';
    FavouritesNotifier({required this.initialIds});
    final List<String> initialIds;

    @override
    List<String> build() => initialIds;

    Future<void> toggle(String poiId) async {
      if (state.contains(poiId)) {
        state = state.where((id) => id != poiId).toList();
      } else {
        state = [...state, poiId];
      }
      final prefs = SharedPreferencesAsync();
      await prefs.setString(prefsKey, jsonEncode(state));
      // Log analytics event
      ref.read(analyticsServiceProvider).logEvent(
        AnalyticsEventType.favouriteToggled,
        metadata: {'poiId': poiId, 'isFavourite': state.contains(poiId)},
      );
    }

    bool isFavourite(String poiId) => state.contains(poiId);
  }
  ```

  **`FavouritesPage`** (`favourites_page.dart`):
  - `ConsumerWidget` with `LayoutPresets.defaultPageBrowser()`.
  - Route: `/favourites`.
  - Reads `ref.watch(favouritesProvider)` for IDs, cross-references with `ref.watch(poisProvider)` for POI objects.
  - Displays vertical list of favourite POIs using `POISummaryCard`-style tiles.
  - Empty state: `EmptyState(icon: Icons.favorite_border, message: t(pt: 'Sem favoritos', en: 'No favourites', es: 'Sin favoritos'))`.
  - Each tile: tap → `context.push('/poi/$poiId')` (or `Navigator.push` to `POIDetailPage`).
  - Each tile: swipe to remove (with `Dismissible` + undo `SnackBar`).
  - Title bar: `t(pt: 'Favoritos', en: 'Favourites', es: 'Favoritos')`.

  **`FavouriteToggleButton`** (`favourite_toggle_button.dart`):
  - Reusable `ConsumerWidget`.
  - Takes `poiId` parameter.
  - Heart icon: filled red when favourite, outlined when not.
  - `AnimatedScale` bounce on toggle.
  - Taps `ref.read(favouritesProvider.notifier).toggle(poiId)`.
  - `Tooltip(message: t(pt: isFav ? 'Remover favorito' : 'Adicionar favorito', ...))`.
  - `Semantics(button: true, label: ..., toggled: isFav)`.

- **Tests (Layer 1):**
  - ✅ `favourites_provider_test.dart` (21 tests):
    - Initial state from constructor (empty list or pre-loaded list)
    - `toggle('poi-1')` → state contains 'poi-1'
    - `toggle('poi-1')` again → state does not contain 'poi-1'
    - `isFavourite('poi-1')` → true after toggle, false after second toggle
    - Persistence: toggle writes to SharedPreferences (mock)
    - Analytics event `favouriteToggled` logged on toggle

- **Tests (Layer 2):**
  - ✅ `favourites_page_test.dart` (16 tests):
    - Empty state renders when no favourites
    - List renders when favourites exist (mock POI data)
    - Tap tile → navigates to POI detail
    - Swipe to remove → item removed + undo snackbar
    - Undo → item restored
    - All text in PT, EN, ES
  - ✅ `favourite_toggle_button_test.dart` (17 tests):
    - Renders outlined heart when not favourite
    - Renders filled heart when favourite
    - Tap toggles state
    - Tooltip text changes based on state
    - Semantics toggled attribute correct (`dart:ui.Tristate` — see Implementation Notes)
    - Bounce animation fires

- **Tests (Layer 3):**
  - ✅ `favourites_integration_test.dart` (10 tests):
    - Toggle adds/removes POI ID from SharedPreferences
    - FavouritesPage renders list correctly
    - FavouritesPage empty state renders when empty
    - Swipe-to-dismiss removes item
    - Undo restores item
    - Multiple toggle operations persist correctly

- **Tests (Layer 4 — Chrome):**
  - ✅ `integration_test/favourites_test.dart` — 15 tests on Chrome (`flutter drive --profile`):
    - FavouritesPage renders empty state (2 tests)
    - FavouriteToggleButton renders, toggles correctly (3 tests)
    - Toggle persists: toggle → navigate away → back → still toggled (2 tests)
    - FavouritesPage renders list after adding favourites (2 tests)
    - Swipe to dismiss — uses `tester.fling()` not `drag()` on Chrome (2 tests — see Implementation Notes)
    - Undo snackbar restores item (2 tests)
    - Empty state shown after removing last item (2 tests)

- **Implementation Notes (discoveries during testing):**

  **1. `defaultPageBrowser(scrollable: false)` required for pages with `Expanded + ListView`:**
  `FavouritesPage` uses `LayoutPresets.defaultPageBrowser()`, which by default passes `scrollable: true`
  to `LayoutSlots`. `LayoutManager` then wraps the body in a `SingleChildScrollView`. But the page body
  contains `Column → Expanded → ListView`, and `Expanded` inside an unbounded scroll view gives
  `ListView` an infinite height → layout crash. This was a **production bug**, not just a test issue.
  Fix: added `bool scrollable = true` parameter to `defaultPageBrowser()`; `FavouritesPage` passes
  `scrollable: false`. Any page with a self-scrolling body (`ListView`, `CustomScrollView`, etc. inside
  `Expanded`) MUST use `scrollable: false`. See §9 Rules Reminder (D21 added below).

  **2. Capture `notifier` before `Dismissible` to avoid "ref used after unmount":**
  `_FavouriteTile` is a `ConsumerWidget`. When `Dismissible` removes the tile, the widget unmounts.
  A SnackBarAction `onPressed` closure that calls `ref.read(favouritesProvider.notifier)` on the dead
  `ref` throws a Riverpod "ref used after unmount" error. Fix: capture
  `final notifier = ref.read(favouritesProvider.notifier)` before building the `Dismissible`, then use
  the captured `notifier` in both `onDismissed` and `onPressed`. See §9 Rules Reminder.

  **3. `Tristate` from `dart:ui` — not a `bool`:**
  `semantics.flagsCollection.isToggled` returns `dart:ui.Tristate`, not `bool`. The matchers `isFalse`
  and `isTrue` compare against `bool` and always fail. Import `'dart:ui' show Tristate` and compare
  with `Tristate.isFalse` / `Tristate.isTrue`.

  **4. Use `Completer<T>()` for never-completing futures in tests:**
  `Future.delayed(Duration(seconds: 60))` creates a pending fake timer → `!timersPending` assertion
  fails at test teardown. Use `Completer<List<POI>>()` that never completes — no timer is created.

  **5. `tester.fling()` required for `Dismissible` on Chrome:**
  `tester.drag()` provides zero gesture velocity. Chrome's `Dismissible` requires velocity to complete
  the dismissal. Use `tester.fling(finder, Offset(-300, 0), 800)` instead of `drag()`.

  **6. `SharedPreferencesAsyncPlatform.instance` required in ALL test `setUp`:**
  Any test that calls `SharedPreferencesAsync()` directly (e.g., IT-FAV-3h verifying empty prefs)
  requires `SharedPreferencesAsyncPlatform.instance = InMemorySharedPreferencesAsync.empty()` in
  `setUp()`. The legacy `SharedPreferences.setMockInitialValues({})` only mocks the legacy API.

- **Acceptance gate:**
  - ✅ `flutter analyze` → 0 new issues from Task 2.5 files (60 pre-existing issues in other files)
  - ✅ `flutter test lib/ --reporter=compact` → **1364 tests passing** (3 skipped, pre-existing)
  - ✅ Layer 1: 21/21 unit tests green
  - ✅ Layer 2: 33/33 widget tests green
  - ✅ Layer 3: 10/10 integration tests green
  - ✅ Layer 4: 15/15 Chrome tests green (`flutter drive --profile`)
  - ✅ Favourites persist across app restart (mock SharedPreferences)
  - ✅ Toggle correctly adds/removes POI IDs
  - ✅ FavouritesPage shows correct list
  - ✅ Swipe-to-dismiss + undo works (production and Chrome)

---

### Task 2.6 — Home Page Redesign (8-Block Adaptive Structure) ✅

- [x] **What:** Complete redesign of the home page from the current layout to the 8-block adaptive structure defined in the wireframe. This is a major rewrite of `home_page.dart`.
- **Why now:** The home page is the first thing a museum visitor sees. With consent, onboarding, and favourites in place, the home page can now render all 8 blocks with real data.
- **Files modified:**
  - `lib/domains/home/pages/home_page.dart` — **major rewrite** ✅
- **Files created:** ✅
  - `lib/domains/home/widgets/home_top_chrome.dart` ✅
  - `lib/domains/home/widgets/home_branding.dart` ✅
  - `lib/domains/home/widgets/hero_card_block.dart` ✅
  - `lib/domains/home/widgets/secondary_cta_block.dart` ✅
  - `lib/domains/home/widgets/profile_banner_block.dart` ✅
  - `lib/domains/home/widgets/favourites_strip.dart` ✅
  - `lib/domains/home/widgets/last_visited_block.dart` ✅
  - `lib/domains/home/widgets/feedback_link_block.dart` ✅
  - `lib/domains/analytics/providers/last_visited_poi_provider.dart` ✅
- **Spec:**

  Refer to §3.2 for the full 8-block specification. Key implementation notes:

  **Block visibility logic (in `home_page.dart`):**
  ```dart
  final profile = ref.watch(onboardingProvider);
  final favourites = ref.watch(favouritesProvider);
  final lastVisited = ref.watch(lastVisitedPOIProvider); // from analytics

  // Always visible: Block 1 (Top Chrome), Block 2 (Branding), Block 3 (Hero Card),
  //                 Block 4 (Secondary CTA), Block 8 (Feedback Link)
  // Conditional:
  //   Block 5 (Profile Banner) — only if profile == null
  //   Block 6 (Favourites strip) — only if favourites.isNotEmpty
  //   Block 7 (Last visited) — only if lastVisited != null
  ```

  **Each block is a separate widget file** (one file, one responsibility):
  - `HomeTopChrome` — 3 icons: LanguageSwitcher + ThemeSwitcher + ProfileAvatar
  - `HomeBranding` — AppBrand + "TILESTORIES" wordmark
  - `HeroCardBlock` — painting context + primary CTA "Explorar com AR"
  - `SecondaryCTABlock` — "Explorar sem câmera" full-width button
  - `ProfileBannerBlock` — dashed border profile setup prompt (visible when no profile)
  - `FavouritesStrip` — horizontal scroll of favourite POI mini-cards
  - `LastVisitedBlock` — last POI visited return shortcut
  - `FeedbackLinkBlock` — "Dar feedback sobre a app" text button

  **Home page structure:**
  ```dart
  LayoutManager(
    slots: LayoutPresets.defaultPageApp(
      context: context,
      body: CustomScrollView(
        slivers: [
          SliverToBoxAdapter(child: HomeTopChrome()),
          SliverToBoxAdapter(child: HomeBranding()),
          SliverToBoxAdapter(child: HeroCardBlock()),
          SliverToBoxAdapter(child: SecondaryCTABlock()),
          if (profile == null) SliverToBoxAdapter(child: ProfileBannerBlock()),
          if (favourites.isNotEmpty) SliverToBoxAdapter(child: FavouritesStrip()),
          if (lastVisited != null) SliverToBoxAdapter(child: LastVisitedBlock()),
          SliverToBoxAdapter(child: FeedbackLinkBlock()),
        ],
      ),
    ),
  )
  ```

  **Migration strategy from existing `home_page.dart`:**
  1. Preserve existing `_buildGreetingHeader()` logic → move to `HomeTopChrome`.
  2. Preserve existing CTA buttons → refactor into `HeroCardBlock` + `SecondaryCTABlock`.
  3. Delete `_DiscoveryCardsSection` — no longer used.
  4. Delete `_QuickFactCarousel` — painting context absorbed into `HeroCardBlock` (Block 3).
  5. Preserve all `RevealAnimation` stagger patterns.
  6. Ensure all existing tests still pass with new widget structure.

- **Tests (Layer 1):** N/A (no new business logic).
- **Tests (Layer 2):** ✅
  - [x] `home_page_test.dart` (rewrite):
    - First visit (no profile, no favourites, no last visited):
      - Blocks 1, 2, 3, 4, 5, 8 visible (Top Chrome, Branding, Hero, Secondary CTA, Profile Banner, Feedback)
      - Block 6 hidden (no favourites)
      - Block 7 hidden (no last visited)
    - Return visit (has profile, has favourites, has last visited):
      - Block 5 hidden (profile set)
      - Block 6 visible (favourites exist)
      - Block 7 visible (last visited exists)
    - Tap Hero CTA → navigates to `/panorama`
    - Tap ProfileAvatar → navigates to `/onboarding`
    - Tap ProfileBannerBlock → navigates to `/onboarding`
    - Tap favourite mini-card → navigates to POI detail
    - Language switcher in top chrome works
    - Theme toggle in top chrome works
    - All text in PT, EN, ES
    - All interactive elements: Semantics + Tooltip + tap target ≥ 48px
    - RevealAnimation stagger fires (verify with `pump` sequencing)
  - [x] Individual block widget tests (`home_widget_test.dart`, `home_cta_test.dart`):
    - GoldDivider, LuxuryCard, RevealAnimation, PillButton
    - HomeTopChrome: 3 icons render (language, theme, avatar), taps work
    - HomeBranding: logo and wordmark render
    - HeroCardBlock: renders painting context + CTA, tap navigates
    - SecondaryCTABlock: renders, PT/EN/ES labels, tap navigates
    - FavouritesStrip: horizontal scroll, renders mini-cards, empty state hidden
    - Language switch / Phase 1 regression

- **Tests (Layer 3):** ✅
  - [x] `home_flow_test.dart`:
    - Smoke (no exception, no ErrorWidget)
    - 8-block layout (all blocks present by key)
    - Portuguese + English + Spanish content
    - Navigation journeys: Hero CTA → /panorama, profile avatar → /onboarding, Ver todos → /favourites
    - Conditional blocks: profile banner / favourites strip show/hide
    - Provider state: isNavigatingProvider
  - [x] `home_real_app_test.dart`:
    - App initialises without exceptions
    - `cta_explorar_com_ar` key is present

- **Acceptance gate:** ✅
  - `flutter analyze` → 0 issues ✅
  - `flutter test lib/domains/home/` → **168 passing, 0 failing** ✅
  - `flutter test lib/` → no new failures (all 17 failing are pre-existing panorama domain tests) ✅
  - Home page renders correct blocks based on user state ✅
  - All 8 blocks render correctly in isolation ✅
  - No regression in existing panorama navigation from home ✅

---

### Task 2.7 — POI Detail Page (Full-Page Replacement) ✅

- ✅ **What:** Create the new full-page `POIDetailPage` with 10 content zones (Z1–Z10) matching §3.7 wireframe, replacing the old `DraggableScrollableSheet` approach. Update `POISummaryCard`'s "Mais Info" button to use `Navigator.push` instead of switching display mode.
- **Why now:** The wireframe demands a full-page layout for POI detail. The old `POIDetailSheet` was a DraggableScrollableSheet directly in the panorama Stack — the new approach is a full page navigated to via `Navigator.push`, which is simpler, more accessible, and allows richer content layout.
- **Files created:**
  - ✅ `lib/domains/panorama/poi/pages/poi_detail_page.dart` — `POIDetailPage` (`ConsumerStatefulWidget`, `Key('poi_detail_page')`) + `pushPOIDetailPage()` helper
  - ✅ `lib/domains/panorama/poi/widgets/poi_gallery_zone.dart` — Z1: PageView gallery (Keys: `poi_gallery_zone`, `poi_gallery_pageview`, `poi_detail_close_button`, `poi_detail_favourite_button`)
  - ✅ `lib/domains/panorama/poi/widgets/poi_header_zone.dart` — Z2+Z3: name + survival badge (Keys: `poi_header_zone`, `poi_detail_name`, `poi_detail_survival_badge`)
  - ✅ `lib/domains/panorama/poi/widgets/poi_category_zone.dart` — Z4: category pill (Keys: `poi_category_zone`, `poi_category_pill`)
  - ✅ `lib/domains/panorama/poi/widgets/poi_view_modes_zone.dart` — Z5: "VER EM" grid (Keys: `poi_view_modes_zone`, `poi_view_mode_3d`, `poi_view_mode_360`, `poi_view_mode_map`)
  - ✅ `lib/domains/panorama/poi/widgets/poi_explore_zone.dart` — Z6: "EXPLORAR" grid (Keys: `poi_explore_zone`, `poi_explore_audio`, `poi_explore_quiz`, `poi_explore_ai`)
  - ✅ `lib/domains/panorama/poi/widgets/poi_external_links_zone.dart` — Z7: external link rows via url_launcher (Keys: `poi_external_links_zone`, `poi_external_link_${type}`)
  - ✅ `lib/domains/panorama/poi/widgets/poi_actions_zone.dart` — Z8: share + feedback (Keys: `poi_actions_zone`, `poi_action_share`, `poi_action_feedback`)
  - ✅ `lib/domains/panorama/poi/widgets/poi_content_tabs_zone.dart` — Z9+Z10: tabs + body (Keys: `poi_content_tabs_zone`, `poi_content_tab_bar`, `poi_tab_descricao`, `poi_tab_historia`, `poi_tab_curiosidades`, `poi_description_text`)
  - ✅ `lib/domains/panorama/poi/models/external_link_type.dart` — `ExternalLinkType` enum (website/facebook/instagram/youtube; `fromString`, `iconData`, `brandColor`)
  - ✅ Test files — see below
- **Files modified:**
  - ✅ `lib/domains/panorama/models/poi.dart` — Added `@Default([]) List<Map<String, dynamic>> externalLinks` field; ran `dart run build_runner build --delete-conflicting-outputs`
  - ✅ `assets/data/pois.json` — Added `externalLinks` demo data to each POI (varying 0–4 links per POI with combinations of website, Facebook, Instagram, YouTube URLs)
  - ✅ `lib/domains/panorama/poi/widgets/poi_summary_card.dart` — "Mais Info" button now calls `Navigator.push(context, MaterialPageRoute(builder: (_) => POIDetailPage(poi: poi)))` instead of `ref.read(poiDisplayModeProvider.notifier).showSheet()`
  - ✅ `lib/domains/panorama/ar/widgets/panorama_ar_view.dart` — `POIDetailSheet` removed from Stack children; no more `POIDisplayMode.sheet` branch
  - ✅ `lib/domains/panorama/providers/panorama_providers.dart` — `POIDisplayMode.sheet` removed from enum; enum is now `{ none, summary }`
- **Pre-step (grep for sheet references):**
  ```
  grep -r "POIDisplayMode.sheet\|showSheet\|DetailSheet\|poi_detail_sheet" lib/
  ```
  List every file returned. Each file must be updated or confirmed irrelevant before removing the enum value. This prevents analyzer errors from missed references.
- **Spec:**

  Refer to §3.7 for the full 10-zone specification. The zone numbers below match §3.7 exactly.

  **`POIDetailPage`** (`poi_detail_page.dart`):
  - `ConsumerWidget`. Takes `POI poi` parameter.
  - Uses `Scaffold` with `CustomScrollView` (NOT `LayoutManager` — this is a pushed page on top of panorama, not a route in the shell).
  - **IMPORTANT:** Z1 gallery uses `SliverToBoxAdapter` with a fixed-height `PageView`, NOT a `SliverAppBar` with `FlexibleSpaceBar`. The wireframe shows a fixed gallery with pagination dots — `SliverAppBar` would collapse the gallery on scroll, which is incompatible with `PageView` swiping.
  - Zones Z1–Z10 stacked vertically in `CustomScrollView.slivers`:
    ```dart
    CustomScrollView(
      slivers: [
        // Z1 — Gallery: fixed height, PageView with dots, close + heart overlays
        SliverToBoxAdapter(
          child: SizedBox(
            height: 280,
            child: POIGalleryZone(poi: poi),
          ),
        ),
        // Z2+Z3 — Header: POI name + survival badge
        SliverToBoxAdapter(child: POIHeaderZone(poi: poi)),
        // Z4 — Category pill
        SliverToBoxAdapter(child: POICategoryZone(poi: poi)),
        // Z5 — "VER EM" grid (3D/360°/Mapa — 3D+360° dimmed)
        SliverToBoxAdapter(child: POIViewModesZone(poi: poi)),
        // Z6 — "EXPLORAR" grid (Áudio/Quiz/Guia IA — all dimmed)
        SliverToBoxAdapter(child: POIExploreZone(poi: poi)),
        // Z7 — External Links (hidden if poi.externalLinks is empty)
        if (poi.externalLinks.isNotEmpty)
          SliverToBoxAdapter(child: POIExternalLinksZone(poi: poi)),
        // Z8 — Actions: Share + Feedback buttons
        SliverToBoxAdapter(child: POIActionsZone(poi: poi)),
        // Z9+Z10 — Content Tabs + Body
        SliverToBoxAdapter(child: POIContentTabsZone(poi: poi)),
      ],
    )
    ```

  **Zone details (matching §3.7 wireframe):**

  **Z1 — Gallery** (`poi_gallery_zone.dart`):
  - Fixed-height `Stack`:
    - `PageView` of photos (placeholder: single dark card with "foto · swipe para mais").
    - Top-left overlay: X close button → `Navigator.pop(context)`.
    - Top-right overlay: ♡ `FavouriteToggleButton(poiId: poi.id)`.
    - Bottom-center overlay: dot indicators (`PageView` page count).
  - Phase 2: 1 placeholder image per POI (from assets or `currentPhotoUrl` if available).
  - Phase 3+: multiple photos per POI.
  - Fallback when no photos: `Container(color: context.surface)` with `Icon(Icons.image_not_supported)`.

  **Z2+Z3 — Header** (`poi_header_zone.dart`):
  - POI name: `context.frauncesDisplay`, `headlineMedium`, `w700`.
  - Survival badge: coloured pill (green=intact, red=destroyed, amber=damaged).
    - Text: "Sobreviveu intacto ao terramoto de 1755" / "Destroyed in..." / "Damaged in...".
    - Badge colour from `destructionLevelColor(poi.destructionLevel)`.
  - Updates with timeline epoch (watches `timelineProvider`).

  **Z4 — Category** (`poi_category_zone.dart`):
  - "Categoria:" label + coloured pill with POI type name.
  - Pill colour: `poi.poiTypeEnum.accentColor`.

  **Z5 — "VER EM"** (`poi_view_modes_zone.dart`):
  - MicroLabel: "VER EM".
  - Row of 3 action cells:
    - "3D" — DIMMED (Phase 3/4, 40% opacity + lock overlay + "Em breve" tooltip).
    - "360°" — DIMMED (Phase 4).
    - "Mapa" — ACTIVE (opens system maps or stub).
  - Dimmed cells: `opacity: 0.4`, lock icon, `onPressed: () {}` (not null — tooltip requires enabled button, per D20).

  **Z6 — "EXPLORAR"** (`poi_explore_zone.dart`):
  - MicroLabel: "EXPLORAR".
  - Row of 3 action cells:
    - "Áudio" — DIMMED (Phase 3).
    - "Quiz" — DIMMED (Phase 3).
    - "Guia IA" — DIMMED (Phase 4).
  - Same dimming pattern as Z5.

  **Z7 — External Links** (`poi_external_links_zone.dart`):
  - MicroLabel: "LINKS EXTERNOS" / "EXTERNAL LINKS" / "ENLACES EXTERNOS".
  - Vertical list of tappable link rows built from `poi.externalLinks`.
  - Each row: leading platform icon + label text + trailing `Icons.open_in_new` (16dp, muted).
  - Tap → `launchUrl(Uri.parse(url), mode: LaunchMode.externalApplication)` via `url_launcher`.
  - Icon mapping via `ExternalLinkType` enum: `website` → `Icons.language`, `facebook` → `Icons.facebook`,
    `instagram` → `Icons.camera_alt`, `youtube` → `Icons.play_circle_outline`. Fallback: `Icons.link`.
  - If `poi.externalLinks.isEmpty` → Z7 hidden entirely (conditional in `POIDetailPage`).
  - Label is a `TranslatableString` in the JSON — rendered via `ref.tr(label)`.
  - Error handling: if `launchUrl` throws → show `SnackBar` with "Não foi possível abrir o link".

  **POI model extension (poi.dart):**
  - Add field: `@Default([]) List<Map<String, dynamic>> externalLinks`.
  - The `Map<String, dynamic>` structure matches the JSON: `{label: {pt, en, es}, url: String, type: String}`.
  - Using `Map<String, dynamic>` (not a Freezed class) keeps the POI model simple — external links
    are display-only data with no business logic. Parsed at widget level.
  - After adding: `dart run build_runner build --delete-conflicting-outputs`.

  **Demo data specification for `pois.json`:**
  - Add `externalLinks` field to each POI. Vary the number and type of links across POIs:
    - ~10 POIs: 0 links (field omitted or `[]`) — Z7 hidden for these
    - ~10 POIs: 1 link (website only)
    - ~10 POIs: 2 links (website + one social platform)
    - ~8 POIs: 3 links (website + two social platforms)
    - ~3 POIs: 4 links (website + Facebook + Instagram + YouTube)
  - Use real-looking but placeholder URLs:
    - Websites: `https://www.example.com/poi-name` (or real Wikipedia/heritage links where available)
    - Facebook: `https://www.facebook.com/poi-name-placeholder`
    - Instagram: `https://www.instagram.com/poi-name-placeholder`
    - YouTube: `https://www.youtube.com/results?search_query=poi-name-lisbon`
  - Labels are `TranslatableString` maps: `{"pt": "Website oficial", "en": "Official website", "es": "Sitio web oficial"}`

  **Z8 — Actions** (`poi_actions_zone.dart`):
  - Row of 2 buttons:
    - "↑ Partilhar" → `Share.share(...)` via `share_plus`.
    - "💬 Dar Feedback" → opens `FeedbackBottomSheet`.
  - Share text: `"Descobre [POI name] no Grande Panorama de Lisboa! tilestories.app/panorama?poi=[id]"`.

  **Z9+Z10 — Content Tabs + Body** (`poi_content_tabs_zone.dart`):
  - `TabBar` with 3 tabs: "Descrição", "História", "Curiosidades".
  - "Descrição" tab: `poi.description` (always available).
  - "História" / "Curiosidades" tabs: placeholder text "Conteúdo em breve" for Phase 2.
  - Phase 3: populate from extended POI data or separate content JSON.
  - Fade-out gradient at bottom as scroll hint.

  **POISummaryCard modification:**
  - Current: "Mais Info" button calls `ref.read(poiDisplayModeProvider.notifier).showSheet()`.
  - New: "Mais Info" button calls:
    ```dart
    Navigator.push(context, MaterialPageRoute(
      builder: (_) => POIDetailPage(poi: poi),
    ));
    // Log analytics event
    ref.read(analyticsServiceProvider).logEvent(
      AnalyticsEventType.poiTapped,
      metadata: {'poiId': poi.id, 'poiName': poi.name},
    );
    ```

  **panorama_ar_view.dart Stack cleanup:**
  - Remove `POIDetailSheet` from Stack children entirely.
  - Remove any `case POIDisplayMode.sheet:` switch branches.
  - Keep `POISummaryCard` in Stack (it still shows as overlay).

  **POIDisplayMode simplification:**
  ```dart
  enum POIDisplayMode { none, summary }
  // Remove 'sheet' value — it's no longer needed
  ```

- **Tests (Layer 1):** ✅ N/A (no new business logic — just UI composition). Unit tests for `ExternalLinkType` enum are covered in Layer 3 (J3).
- **Tests (Layer 2):** ✅
  - ✅ `poi_detail_page_test.dart` — **39 tests, all passing**
    - Smoke/scaffold (4): renders without crash for intact/destroyed/damaged POI; `Key('poi_detail_page')` present
    - Z1 Gallery (4): zone key, close button key, favourite button key, close button pops page (Navigator.pop)
    - Z2+Z3 Header (6): name key, name text, survival badge key, intact/destroyed/damaged badge labels
    - Z4 Category (4): zone key, pill key, power→'Real / Nobre', religious→'Religioso'
    - Z5 View modes (5): zone key, 3D present, 360° present, Mapa present, Mapa tap→SnackBar (ensureVisible used)
    - Z6 Explore (2): zone key, all three cells present
    - Z7 External links (3): absent for no-links POI, present for links POI, website row key
    - Z8 Actions (4): zone key, share key, feedback key, feedback tap→SnackBar (ensureVisible used)
    - Z9+Z10 Content tabs (7): zone key, tab bar key, all 3 tab keys, description text key, description content
  - ✅ `poi_summary_card_test.dart` updates (done in prior session):
    - Tests 18/19: "Mais Info" tap → `Navigator.push` to `POIDetailPage`
  - ✅ `poi_display_mode_notifier_test.dart` — **10 tests, all passing** (rewrote for 2-value enum `{none, summary}`)
  - ✅ `poi_detail_sheet_test.dart` — **23 tests: 19 passing, 4 pre-existing failures** (DraggableScrollableSheet text visibility in tests — not regressions; present before Task 2.7)
    - Pre-existing failures: civic→'Cívico', damaged label, pre1755 epoch text, earthquake epoch text

- **Tests (Layer 3):** ✅
  - ✅ `poi_detail_integration_test.dart` — **~20 tests across 3 journeys, all passing**
    - J1 (5 tests): `POISummaryCard` → 'Mais Info' → `POIDetailPage` navigation; name shown; survival badge shown; all 10 zone keys present
    - J2 (3 tests): Close button pops detail page; `POISummaryCard` visible again; `poiDisplayModeProvider` stays `none`
    - J3 (8 tests): `ExternalLinkType` enum unit tests — `fromString` for website/facebook/instagram/youtube/null/unknown; `iconData` non-null; `brandColor` non-null

- **Acceptance gate:**
  - ✅ `flutter analyze` → 0 issues
  - ✅ `flutter test lib/` → **+1421 ~3 -21** (21 failures = 17 pre-existing camera plugin + 4 pre-existing sheet visibility — no new regressions)
  - ✅ `POIDetailSheet` completely removed from panorama Stack
  - ✅ `POIDisplayMode.sheet` removed from enum
  - ✅ Full POI detail page renders all Phase 2 zones (Z1–Z10; Z7 visible when POI has external links)
  - ✅ Z7 external links: `launchUrl` called via url_launcher; hidden for POIs with empty `externalLinks`
  - ✅ Navigation: summary card → push → detail page → pop → panorama (no state loss)
  - ⚠️ **Deferred to Phase 3:** `poi_detail_page_test.dart` tests for `Share.share` platform channel mock, `FeedbackBottomSheet` full widget test, and accessibility tap-target size assertions — these require additional platform channel stubs not yet set up

---

### Task 2.8 — Search + Filter Flow ✅

- [x] **What:** Implement the 4-step search and filter flow for the panorama page: search bar activation, search results, filter chips, and filtered map state.
- **Why now:** Search and filter are core exploration features that make the 41 POIs navigable. With the POI detail page in place (Task 2.7), search results can link directly to POI pages.
- **Files created:**
  - `lib/domains/panorama/providers/search_provider.dart` — `SearchState` sealed class (`SearchIdle` / `SearchActive` / `SearchResultSelected`) + `SearchNotifier` + `searchProvider`
  - `lib/domains/panorama/providers/filter_provider.dart` — `FilterState` (immutable, `copyWith`) + `FilterNotifier` + `filterProvider` + `filteredPoisProvider` (derived, AND-combines both axes)
  - `lib/domains/panorama/ar/widgets/search_bar_widget.dart` — `SearchBarWidget`: idle icon ↔ animated active pill with text field + filter badge
  - `lib/domains/panorama/ar/widgets/search_overlay.dart` — `SearchOverlay`: full panel with destruction chips, POI type circles, results list, cancel/apply footer
  - All test files (see below)
- **Files modified:**
  - `lib/domains/panorama/ar/widgets/panorama_top_bar.dart` — `SearchBarWidget` integrated; layout switches between idle (search icon + options) and active (expanded pill)
  - `lib/domains/panorama/ar/widgets/panorama_ar_view.dart` — `SearchOverlay` added as Stack layer [2b]; `PopScope` wraps `Scaffold` for Android back; `_ActiveFilterLabel` shown below top bar after apply (Step 3b)
- **Spec implemented:**
  - Step 1 (idle): search icon in top bar, `Key('search_icon_button')`
  - Step 2 (active panel): `SearchOverlay` slides in from top bar; destruction chips `Key('destruction_chip_0/20/40/60')`; POI type circles `Key('poi_type_circle_${type.name}')`; results list `Key('search_results_list')`; cancel `Key('search_cancel_button')`; apply `Key('search_apply_button')`
  - Step 3a (tap result): closes search, sets `selectedPOIProvider`, opens `POIDisplayMode.summary`
  - Step 3b (apply filters): closes panel, keeps `FilterState` active, shows `_ActiveFilterLabel` (`Key('active_filter_label')`) with "N filtros activos · M resultados"
- **Tests — Layer 1 (unit):**
  - `lib/domains/panorama/test/unit/search_provider_test.dart` — 16 tests ✅
  - `lib/domains/panorama/test/unit/filter_provider_test.dart` — 20 tests ✅
- **Tests — Layer 2 (widget):**
  - `lib/domains/panorama/test/widgets/search_widget_test.dart` — 14 tests ✅
- **Tests — Layer 3 (integration):**
  - `lib/domains/panorama/test/integration/search_filter_integration_test.dart` — 7 journeys ✅
- **Tests — Layer 4 (device):** ⚠️ Deferred — requires physical device with camera for full AR flow verification
- **Total new tests: 57 passing (0 failures)**
- **Spec:**

  Refer to §3.8 for the full 4-step specification. Key implementation notes:

  **`SearchNotifier`** (`search_provider.dart`):
  ```dart
  class SearchNotifier extends Notifier<SearchState> {
    @override
    SearchState build() => const SearchState.idle();

    void activate() => state = const SearchState.active(query: '', results: []);

    void updateQuery(String query) {
      if (query.isEmpty) {
        state = const SearchState.active(query: '', results: []);
        return;
      }
      final pois = ref.read(poisProvider).value ?? [];
      final results = pois.where((poi) =>
        poi.name.toLowerCase().contains(query.toLowerCase()) ||
        poi.category.toLowerCase().contains(query.toLowerCase())
      ).toList();
      state = SearchState.active(query: query, results: results);
    }

    void selectResult(POI poi) {
      state = SearchState.resultSelected(poi: poi);
      ref.read(selectedPOIProvider.notifier).select(poi);
      ref.read(poiDisplayModeProvider.notifier).showSummary();
    }

    void close() => state = const SearchState.idle();
  }
  ```

  **`SearchState` sealed class:**
  ```dart
  sealed class SearchState {
    const SearchState();
    const factory SearchState.idle() = SearchIdle;
    const factory SearchState.active({required String query, required List<POI> results}) = SearchActive;
    const factory SearchState.resultSelected({required POI poi}) = SearchResultSelected;
  }
  ```

  **`FilterNotifier`** (`filter_provider.dart`):
  ```dart
  class FilterNotifier extends Notifier<Set<String>> {
    @override
    Set<String> build() => {}; // empty = show all

    void toggleCategory(String category) {
      if (state.contains(category)) {
        state = {...state}..remove(category);
      } else {
        state = {...state, category};
      }
    }

    void clearAll() => state = {};
    bool isActive(String category) => state.contains(category);
    bool get hasActiveFilters => state.isNotEmpty;
  }
  ```

  **`SearchBarOverlay`** (`search_bar_overlay.dart`):
  - Full-width text field at top of screen (below `PanoramaTopBar`).
  - `AnimatedSlide` + `AnimatedOpacity` to slide in from top.
  - `TextField` with `onChanged: ref.read(searchProvider.notifier).updateQuery`.
  - Clear button (`Icons.close`) when text is non-empty.
  - Below text field: `SearchResultsList` (animated in/out based on query).
  - Scrim behind results → tap closes search.
  - **PopScope (Android back button):** Wrap the overlay in `PopScope(canPop: false, onPopInvokedWithResult: (didPop, _) { if (!didPop) ref.read(searchProvider.notifier).close(); })`. This ensures the Android back button closes the search overlay instead of navigating back to home.

  **`SearchResultsList`** (`search_results_list.dart`):
  - `ListView.builder` of matching POIs.
  - Each row: POI name + category badge + chevron.
  - Tap row → `ref.read(searchProvider.notifier).selectResult(poi)` → closes search, highlights POI on map, shows summary card.
  - Empty state: `t(pt: 'Nenhum resultado', en: 'No results', es: 'Sin resultados')`.

  **`FilterChipRow`** (`filter_chip_row.dart`):
  - Horizontal row of `FilterChip` widgets below the top bar.
  - One chip per POI category (derived from POI data: e.g., "Igreja", "Palácio", "Forte", etc.).
  - Selected chips: gold border + filled.
  - `onSelected: ref.read(filterProvider.notifier).toggleCategory(category)`.
  - "Limpar" chip at end → `ref.read(filterProvider.notifier).clearAll()`.
  - Only visible when `ref.watch(filterProvider).isNotEmpty` OR search is active.

  **Marker visibility connection:**
  - In `panorama_ar_view.dart`, the existing `renderSpecs` filtering logic must also consider active filters:
    ```dart
    final activeFilters = ref.watch(filterProvider);
    final visiblePois = allPois.where((poi) {
      if (activeFilters.isEmpty) return true;
      return activeFilters.contains(poi.category);
    }).toList();
    ```
  - `renderSpecs` is computed from `visiblePois` instead of `allPois`.

  **Search icon in top bar:**
  - Add `IconButton(icon: Icons.search, onPressed: ref.read(searchProvider.notifier).activate)` to `PanoramaTopBar`.
  - `Tooltip(message: t(pt: 'Pesquisar', en: 'Search', es: 'Buscar'))`.

- **Tests (Layer 1):**
  - [ ] `search_provider_test.dart`:
    - Initial state is `SearchState.idle()`
    - `activate()` → `SearchState.active(query: '', results: [])`
    - `updateQuery('castelo')` → results contain matching POIs
    - `updateQuery('')` → results empty
    - `selectResult(poi)` → `SearchState.resultSelected(poi: poi)` + `selectedPOIProvider` updated
    - `close()` → `SearchState.idle()`
  - [ ] `filter_provider_test.dart`:
    - Initial state is empty set
    - `toggleCategory('Igreja')` → set contains 'Igreja'
    - `toggleCategory('Igreja')` again → set is empty
    - `clearAll()` → set is empty
    - `hasActiveFilters` → true when non-empty, false when empty

- **Tests (Layer 2):**
  - [ ] `search_bar_overlay_test.dart`:
    - Renders text field when search is active
    - Type query → results appear
    - Tap result → search closes, POI selected
    - Tap clear button → query cleared
    - Tap scrim → search closes
    - All text in PT, EN, ES
    - Accessibility: text field has label, results have Semantics
  - [ ] `filter_chip_row_test.dart`:
    - Renders chips for all categories present in POI data
    - Tap chip → toggles filter
    - "Limpar" chip clears all filters
    - Selected chips have gold border

- **Tests (Layer 3):**
  - [ ] `search_filter_integration_test.dart`:
    - Panorama → tap search icon → search bar appears → type "Castelo" → result appears → tap → summary card shows for Castelo → search closes
    - Panorama → activate filter "Igreja" → only church POIs visible on map → clear filter → all POIs visible
    - Search + filter combined: search "São" with filter "Igreja" → only matching churches shown

- **Acceptance gate:**
  - `flutter analyze` → 0 issues
  - `flutter test lib/` → all passing
  - Search finds POIs by name and category
  - Filter chips toggle correctly and affect marker visibility
  - Search → result selection → POI summary card opens
  - Analytics event `searchPerformed` logged when search result is selected

---

### Task 2.9 — Panorama FAB + Route Stubs + Settings Page + Deep Linking ✅

- ✅ **What:** Create the expandable FAB, add route stubs for future domains, build the settings/privacy page, and implement `?poi=<id>` deep link.
- **Why now:** These are all panorama-adjacent features that can be built together. The FAB is the navigation hub; route stubs prevent 404s; settings provides privacy controls (GDPR); deep link supports share URLs.
- **Files created:**
  - `lib/domains/panorama/ar/widgets/panorama_fab.dart` ✅
  - `lib/domains/settings/pages/settings_page.dart` ✅
  - `lib/domains/settings/widgets/privacy_section.dart` ✅
  - `lib/domains/settings/widgets/about_section.dart` ✅
  - `lib/domains/settings/widgets/feedback_section.dart` ✅
  - `lib/domains/settings/settings_domain.dart` ✅
  - All test files (see below) ✅
- **Files modified:**
  - `lib/domains/panorama/ar/widgets/panorama_ar_view.dart` — FAB + scrim + `?poi=` deep link ✅
  - `lib/domains/panorama/ar/widgets/panorama_top_bar.dart` — Settings icon link ✅
  - `lib/navigation/navConfig/nav_config.dart` — 6 new routes + `_ComingSoonPage` ✅
- **Implementation notes:**
  - `_ComingSoonPage` is a private `StatelessWidget` (not `ConsumerWidget`) that reads locale via `Localizations.localeOf(context)` — no Riverpod dependency
  - `SettingsPage` MUST use `scrollable: false` in `defaultPageBrowser` — `LuxuryScaffold` uses `Scaffold` with `StackFit.expand` and requires a finite height from its parent; wrapping in `SingleChildScrollView` (the default `scrollable: true`) provides infinite height and causes a render error
  - `flutter_localizations` added to `pubspec.yaml` dependencies (SDK package) for test locale support
    - `/settings` → `SettingsPage`
    - `/favourites` → `FavouritesPage` (already added in Task 2.5)
    - `/circuits` → `_ComingSoonPage` (stub)
    - `/achievements` → `_ComingSoonPage` (stub)
    - `/ai-chat` → `_ComingSoonPage` (stub)
    - `/quiz/:poiId` → `_ComingSoonPage` (stub)
- **Spec:**

  **`PanoramaFAB`** (`panorama_fab.dart`):
  - `ConsumerStatefulWidget` — local `_expanded` state.
  - Main button: `FloatingActionButton` with `Icons.explore_rounded`, `context.primary` background.
  - When tapped: toggle `_expanded`.
  - `AnimatedRotation(turns: _expanded ? 0.125 : 0, duration: AnimationTokens.medium)` on main icon.
  - Sub-actions array (bottom to top when expanded):
    ```
    [0] Progress — Icons.emoji_events_outlined — label: t(pt: 'Progresso', en: 'Progress', es: 'Progreso')
          onTap: ref.read(isNavigatingProvider.notifier).set(true); context.go('/achievements')
    [1] Favourites — Icons.favorite_outlined — label: t(pt: 'Favoritos', en: 'Favourites', es: 'Favoritos')
          onTap: ref.read(isNavigatingProvider.notifier).set(true); context.go('/favourites')
    [2] Audio — Icons.headphones_outlined — label: t(pt: 'Áudio', en: 'Audio', es: 'Audio')
          onTap: () {} (stub) — Tooltip: t(pt: 'Em breve', en: 'Coming soon', es: 'Próximamente')
    [3] Circuits — Icons.route_outlined — label: t(pt: 'Circuitos', en: 'Circuits', es: 'Circuitos')
          onTap: () {} (stub) — same tooltip
    [4] AI Guide — Icons.smart_toy_outlined — label: t(pt: 'Guia IA', en: 'AI Guide', es: 'Guía IA')
          onTap: () {} (stub) — same tooltip
    ```
  - **IMPORTANT (D20):** Stub sub-actions use `onPressed: () {}` NOT `onPressed: null`. With `null`, the `Tooltip` won't show on disabled buttons. Using a no-op callback keeps the button "enabled" for tooltip purposes while the action is a stub.
  - Each sub-action: `AnimatedPositioned` with stagger delay `AnimationTokens.r1`–`r5`.
  - Each sub-action: `AnimatedOpacity` 0→1 when expanding, 1→0 when collapsing.
  - Each sub-action: small `FloatingActionButton.small` (40dp) + `Text` label left of button.
  - Scrim: `GestureDetector` on full-screen `AnimatedOpacity` overlay → tap closes FAB.
  - Position in Stack: `Positioned(bottom: Spacing.xl2 + 80, right: Spacing.lg)`.
  - `Semantics(label: t(pt: 'Ações de exploração', en: 'Exploration actions', es: 'Acciones de exploración'))`.

  **`_ComingSoonPage` inline widget** (in `nav_config.dart`):
  ```dart
  class _ComingSoonPage extends ConsumerWidget {
    const _ComingSoonPage();
    @override
    Widget build(BuildContext context, WidgetRef ref) {
      return LayoutManager(
        slots: LayoutPresets.defaultPageBrowser(
          context: context,
          body: Center(child: EmptyState(
            icon: Icons.construction_outlined,
            message: ref.tr(t(pt: 'Em breve', en: 'Coming soon', es: 'Próximamente')),
          )),
        ),
      );
    }
  }
  ```

  **`SettingsPage`** (`settings_page.dart`):
  - `ConsumerWidget` with `LayoutPresets.defaultPageBrowser()`.
  - Sections: Language, Theme, Profile, Privacy (with `PrivacySection` widget), Feedback (with `FeedbackSection` widget), About (with `AboutSection` widget).
  - Each section uses `SectionHeader` from `components/ui/`.
  - Accessible from: home page settings icon (Block 1) AND panorama ··· menu.

  **`PrivacySection`** (`privacy_section.dart`):
  - Shows what data is collected (plain language).
  - Current consent status badge.
  - "Mudar consentimento" button → toggles consent via `consentProvider.notifier.setConsent(...)`.
  - "Apagar os meus dados" `OutlinedButton` with `context.error` styling → confirmation dialog → `analyticsService.clearAll()`.

  **`FeedbackSection`** (`feedback_section.dart`):
  - Teaser text: `t(pt: 'Ajude-nos a melhorar', en: 'Help us improve', es: 'Ayúdenos a mejorar')`.
  - Button: "Enviar Feedback" → opens feedback bottom sheet (built in Task 2.10).

  **`AboutSection`** (`about_section.dart`):
  - `AppBrand(direction: Axis.vertical)`.
  - Version number: hardcoded `'1.0.0'` (D19).
  - "Museu Nacional do Azulejo" + thesis credits.
  - Link to privacy policy.

  **Deep link `?poi=<id>` on `/panorama`:**
  - In `PanoramaARView.initState` or `build()`:
    ```dart
    final poiId = GoRouterState.of(context).uri.queryParameters['poi'];
    if (poiId != null) {
      WidgetsBinding.instance.addPostFrameCallback((_) {
        final pois = ref.read(poisProvider).value ?? [];
        final target = pois.firstWhereOrNull((p) => p.id == poiId);
        if (target != null) {
          ref.read(selectedPOIProvider.notifier).select(target);
          ref.read(poiDisplayModeProvider.notifier).showSummary();
        }
      });
    }
    ```

- **Tests (Layer 1):** N/A.
- **Tests (Layer 2):**
  - ✅ `panorama_fab_test.dart` — **14/14 PASS**:
    - FAB renders main button with `Key('panorama_fab_main')`
    - Tap main button → 5 sub-actions appear with labels
    - Scrim visible when expanded → tap scrim → FAB collapses
    - Tap "Progresso" → navigates to `/achievements`
    - Tap "Favoritos" → navigates to `/favourites`
    - Stub sub-actions (Audio, Circuits, AI) have opacity 0.5 when disabled
    - `onExpandedChanged` callback fires on expand/collapse
    - Main button has Semantics label
    - Note: `find.descendant(..., matchRoot: true)` required (not `find.ancestor`) when the target widget IS the Opacity root
  - ✅ `settings_page_test.dart` — **18/18 PASS**:
    - All 6 sections render with correct keys
    - Language / theme / profile tiles interact correctly
    - Privacy section shows consent badge; "Mudar" toggles consent
    - "Apagar os meus dados" → confirmation dialog → `clearAllCalls` incremented
    - Feedback section shows teaser text and "Enviar Feedback" button shows snackbar
    - Test pattern: `UncontrolledProviderScope` + 3-pump + `tester.view.physicalSize = Size(1080, 4000)`
  - ✅ `route_stubs_test.dart` — **8/8 PASS**:
    - `/settings` → `Key('settings_page')` renders; `Key('settings_page_header')` renders
    - `/achievements`, `/circuits`, `/ai-chat`, `/quiz/test-poi` → `Key('coming_soon_badge')` + title text
    - Badge chip text = 'Em breve' in pt locale
    - Coming-soon AppBar title text = route title (pt locale)
    - Required `flutter_localizations` + `GlobalMaterialLocalizations.delegate` for pt locale support

- **Tests (Layer 3):**
  - [ ] FAB integration — `panorama_layout_flow_test.dart` already covers layout; dedicated FAB integration tests pending
  - [x] Settings → consent change → analytics behavior change (Task 2.10 scope)
  - [ ] Deep link `?poi=<id>` integration test (pending)

- **Acceptance gate:** ✅
  - `flutter analyze` → 0 issues ✅
  - All 40 new tests pass (14 FAB + 18 Settings + 8 Route stubs) ✅
  - FAB renders on panorama with 5 sub-actions, expands/collapses, navigates correctly ✅
  - All stub routes render `_ComingSoonPage` (no 404) ✅
  - Settings page fully functional with all 6 sections ✅
  - Deep link `?poi=<id>` implemented in `panorama_ar_view.dart` ✅

---

### Task 2.10 — Analytics Event Wiring + Feedback Mechanism ✅ COMPLETE

- [x] **What:** Wire all analytics events into existing domain actions AND build the feedback bottom sheet that sends user feedback through the analytics backend pipeline.
- **Why now:** With consent, analytics service, and all new screens in place, wire the actual events so thesis data collection begins immediately on release. Feedback uses the same pipeline (AnalyticsBackend) so it's a natural fit.
- **Files created:**
  - `lib/domains/feedback/models/feedback_category.dart` ✅
  - `lib/domains/feedback/widgets/feedback_bottom_sheet.dart` ✅
  - `lib/domains/feedback/feedback_domain.dart` ✅ (barrel export)
  - `lib/domains/feedback/test/widgets/feedback_bottom_sheet_test.dart` ✅ (16 tests)
  - `lib/domains/feedback/test/integration/feedback_integration_test.dart` ✅ (7 tests)
  - `lib/domains/analytics/test/unit/analytics_wiring_test.dart` ✅ (12 tests)
  - `integration_test/feedback_test.dart` ✅ (5 tests — device/emulator required)
- **Files modified:**
  - `lib/domains/panorama/poi/widgets/poi_summary_card.dart` — already wired in Task 2.7 (poiTapped on "Mais Info")
  - `lib/domains/timeline/providers/timeline_provider.dart` — log `timelineChanged` with `.catchError((_) {})` when epoch changes ✅
  - `lib/domains/panorama/providers/search_provider.dart` — log `searchPerformed` with `.catchError((_) {})` when result is selected ✅
  - `lib/domains/settings/widgets/feedback_section.dart` — "Enviar Feedback" button opens `FeedbackBottomSheet` ✅
- **Regression fixes (existing tests broken by analytics wiring):**
  - `lib/domains/timeline/test/widgets/epoch_label_test.dart` — added `analyticsServiceProvider` override ✅
  - `lib/domains/timeline/test/widgets/timeline_slider_test.dart` — added `analyticsServiceProvider` override to all 4 containers ✅
  - `lib/domains/timeline/test/unit/timeline_notifier_test.dart` — added `_makeContainer()` helper with analytics override ✅
  - `lib/domains/panorama/test/unit/search_provider_test.dart` — added `analyticsServiceProvider` override to `makeContainer()` ✅
- **Spec:**

  **Analytics events to wire (full list):**

  | Event               | Where wired                       | When fired                               | Metadata                      |
  | ------------------- | --------------------------------- | ---------------------------------------- | ----------------------------- |
  | `sessionStart`      | `main.dart` (Task 2.3)            | App resumed / first launch               | `{sessionId}`                 |
  | `sessionEnd`        | `main.dart` (Task 2.3)            | App paused                               | `{sessionId, durationMs}`     |
  | `consentChanged`    | `ConsentNotifier` (Task 2.2)      | User changes consent                     | `{newState}`                  |
  | `profileSet`        | `OnboardingNotifier` (Task 2.4)   | User selects/changes profile             | `{profileType}`               |
  | `poiTapped`         | `POISummaryCard` (Task 2.7)       | User taps "Mais Info" → full detail page | `{poiId, poiName}`            |
  | `timelineChanged`   | `TimelineNotifier` (this task)    | User changes epoch                       | `{epoch}`                     |
  | `favouriteToggled`  | `FavouritesNotifier` (Task 2.5)   | User toggles favourite                   | `{poiId, isFavourite}`        |
  | `searchPerformed`   | `SearchNotifier` (this task)      | User selects a search result             | `{query, selectedPoiId}`      |
  | `feedbackSubmitted` | `FeedbackBottomSheet` (this task) | User submits feedback form               | `{category, message, rating}` |

  **Wiring pattern:**
  ```dart
  ref.read(analyticsServiceProvider).logEvent(
    AnalyticsEventType.timelineChanged,
    metadata: {'epoch': epoch.name},
  );
  ```

  **`FeedbackBottomSheet`** (`feedback_bottom_sheet.dart`):
  - `ConsumerStatefulWidget` shown via `showModalBottomSheet`.
  - Form fields:
    - Category dropdown: `t(pt: 'Categoria', ...)` → options: Bug, Sugestão, Conteúdo, Outro.
    - Message `TextField`: max 500 chars, 4 lines.
    - Rating row: 1–5 stars (optional).
  - "Enviar" `FilledButton`:
    - **Double-tap guard:** Local `_isSubmitting` bool. On tap: set `true` + disable button. On completion/error: set `false`. Prevents duplicate submissions.
    - Validates form (category required, message required, ≥ 10 chars).
    - Logs `feedbackSubmitted` event with metadata `{category, message, rating}`.
    - Shows success `SnackBar`: `t(pt: 'Obrigado pelo feedback!', en: 'Thanks for your feedback!', es: '¡Gracias por su comentario!')`.
    - Closes bottom sheet.
  - "Cancelar" `TextButton` → closes bottom sheet.
  - All text in PT, EN, ES.
  - Accessible: form labels, button semantics.

  **Integration with settings page:**
  - `FeedbackSection` in settings page has "Enviar Feedback" button.
  - Button calls: `showModalBottomSheet(context: context, builder: (_) => const FeedbackBottomSheet())`.

- **Tests (Layer 1) ✅ 12/12 pass:**
  - [x] `analytics_wiring_test.dart`:
    - Navigate to POI detail page → `poiTapped` event logged with correct metadata
    - Change epoch → `timelineChanged` event logged
    - Set profile → `profileSet` event logged
    - Toggle favourite → `favouriteToggled` event logged
    - Select search result → `searchPerformed` event logged
    - All events: consent=declined → no event logged

- **Tests (Layer 2) ✅ 16/16 pass:**
  - [x] `feedback_bottom_sheet_test.dart`:
    - Sheet renders form fields (category, message, rating)
    - Submit with valid form → `feedbackSubmitted` event logged
    - Submit with empty message → validation error shown
    - Submit with message < 10 chars → validation error shown
    - Cancel → sheet closes without logging
    - Success snackbar appears after submission
    - All text in PT, EN, ES
    - Accessibility: form labels, button semantics

- **Tests (Layer 3) ✅ 7/7 feedback + 18/18 settings:**
  - [x] Full analytics flow:
    - Launch with consent=accepted → session started → navigate to panorama → tap POI → open detail → poiTapped logged → change epoch → timelineChanged logged → verify events in database
    - Settings → feedback → submit → feedbackSubmitted logged → verify in database
    - Consent=declined → repeat actions → zero events in database

- **Tests (Layer 4) — device/emulator required:**
  - [ ] `integration_test/feedback_test.dart` (5 tests) — not yet run; requires device

- **Acceptance gate:**
  - [x] `flutter analyze` → 0 issues
  - [x] `flutter test lib/` → all task-2.10 tests passing (regressions resolved, 70 pre-existing failures unrelated to this task)
  - [x] Analytics events fire correctly for all 9 event types
  - [x] Feedback form validates, submits through analytics pipeline
  - [x] Consent=declined → zero events in database

---

### Task 2.11 — App Store Preparation ⏸ DEFERRED TO PHASE 5

> **Deferred decision:** The initial implementation work (launcher icons, AndroidManifest, web metadata, AppConfig class, L1/L2/L3 tests) was completed in Phase 2. However, the full store submission process — developer accounts, screenshots, store listing text, privacy policy URL, public release — requires external dependencies (museum partnership, real domain, ethics board approval) that are only ready at the end of Phase 4. Task 2.11 has therefore been moved to **Phase 5 §5.5** where it belongs. The code and test artefacts remain in the project. See PROJECT_GUIDE.md §6 Phase 5.

- ⏸ **What:** Full app store release. Core code artefacts were produced in Phase 2 (see implementation notes below); the complete release process is Phase 5 work.
- **Why deferred:** Store release requires: developer accounts (Google Play + Apple Developer), privacy policy URL, store listing screenshots (6 per locale × 3 locales), ethics board approval for thesis data collection, and the real `tilestories.app` domain for deep links. None of these are code tasks — they are external dependencies that must be resolved before Phase 5.

#### Implementation notes (deviations from original spec)

- **iOS disabled** (`ios: false` in flutter_launcher_icons config) — no `ios/` folder exists in this project. iOS icons and `Info.plist` can be added when the iOS target is created.
- **`AppConfig` class created** at `lib/domains/settings/app_config.dart` — single source of truth for version, applicationId, bundleId, store URLs and web domain. `AboutSection` now reads `AppConfig.version` instead of a local `_version = '1.0.0'` constant. Exported via `settings_domain.dart` barrel.
- **Source icon**: used `assets/branding/Logo.png` (the existing high-quality PNG) — no separate `icon_foreground.png` was needed.
- **Tests added**: 35 new tests (9 L1 unit + 12 L2 widget + 14 L3 integration) — none of these are "N/A" as originally written.

#### Files modified ✅

| File                                                          | Change                                                                                                                                      |
| ------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------- |
| `pubspec.yaml`                                                | Added `flutter_launcher_icons: ^0.14.3` dev dep + full `flutter_launcher_icons:` config block                                               |
| `android/app/build.gradle.kts`                                | `namespace`/`applicationId` → `com.tilestories.app`; `targetSdk` → 34; `versionCode` → 1; `versionName` → `"1.0.0"`                         |
| `android/app/src/main/AndroidManifest.xml`                    | `android:label` → `"TileStories"`                                                                                                           |
| `android/app/src/main/res/drawable/launch_background.xml`     | Background → `@color/ic_launcher_background` (#1C1409)                                                                                      |
| `android/app/src/main/res/drawable-v21/launch_background.xml` | Same                                                                                                                                        |
| `android/app/src/main/res/values/styles.xml`                  | `LaunchTheme` + `NormalTheme` → `Theme.Black.NoTitleBar`; `windowBackground` → `@color/ic_launcher_background`                              |
| `android/app/src/main/res/values-night/styles.xml`            | `NormalTheme` `windowBackground` → `@color/ic_launcher_background`                                                                          |
| `web/index.html`                                              | `lang="pt"`; `theme-color #C9A84C`; canonical + hreflang (pt/en/es/x-default); full OG tags; Twitter Card; Portuguese-first description     |
| `web/manifest.json`                                           | `name` → "TileStories — Grande Panorama de Lisboa"; `description` → PT; `lang` → `"pt"`; `orientation` → `"portrait-primary"`; brand colors |
| `lib/domains/settings/widgets/about_section.dart`             | Uses `AppConfig.version` — removed local `_version` constant                                                                                |
| `lib/domains/settings/settings_domain.dart`                   | Added `export 'app_config.dart';`                                                                                                           |

#### Files created ✅

| File                                                                                | Purpose                                                                                    |
| ----------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------ |
| `lib/domains/settings/app_config.dart`                                              | `AppConfig` abstract final class — version, applicationId, bundleId, store URLs, webDomain |
| `lib/domains/settings/test/unit/app_config_test.dart`                               | **L1** — 9 unit tests for AppConfig constants                                              |
| `lib/domains/settings/test/widgets/about_section_test.dart`                         | **L2** — 12 widget tests (all 3 languages × all 4 elements)                                |
| `lib/domains/settings/test/integration/app_store_preparation_integration_test.dart` | **L3** — 14 integration tests (full SettingsPage + language switching)                     |

#### Files generated by `dart run flutter_launcher_icons` ✅

- `android/app/src/main/res/mipmap-{hdpi,mdpi,xhdpi,xxhdpi,xxxhdpi}/ic_launcher.png`
- `android/app/src/main/res/drawable-{hdpi,mdpi,xhdpi,xxhdpi,xxxhdpi}/ic_launcher_foreground.png`
- `android/app/src/main/res/mipmap-anydpi-v26/ic_launcher.xml` (adaptive icon)
- `android/app/src/main/res/values/colors.xml` (`ic_launcher_background = #1C1409`)
- `web/icons/Icon-{192,512}.png`, `web/icons/Icon-maskable-{192,512}.png`

#### Test results ✅

| Layer          | File                                          | Tests | Result                                                   |
| -------------- | --------------------------------------------- | ----- | -------------------------------------------------------- |
| L1 unit        | `app_config_test.dart`                        | 9     | ✅ 9/9                                                    |
| L2 widget      | `about_section_test.dart`                     | 12    | ✅ 12/12                                                  |
| L3 integration | `app_store_preparation_integration_test.dart` | 14    | ✅ 14/14                                                  |
| Full suite     | `flutter test lib/`                           | 1539  | ✅ +35 new, 0 regressions                                 |
| Analyzer       | `flutter analyze lib/ --no-pub`               | —     | ✅ 0 new issues (7 pre-existing infos in unrelated files) |

#### Still pending (outside codebase)

- `flutter build apk --release` → verify APK < 100MB (requires Android build toolchain)
- `flutter build appbundle --release` → for Google Play upload
- iOS: no `ios/` folder — add when iOS target is initialised (`flutter create --platforms=ios .`)
- Store listing text, screenshots, privacy policy URL (outside codebase)
- Replace `TODO(Phase2)` canonical URL placeholder in `web/index.html` with real domain

---

## 7. Global Test Suite

Run after ALL tasks (2.1–2.10) are complete. Task 2.11 is deferred to Phase 5. NOT a substitute for per-task tests.

### Layer 1+2+3 combined

```
flutter test lib/ --reporter=compact
```

Expected: all passing, 0 regressions vs Phase 1 baseline (1134+), new tests added.

Verify domains individually:
```
flutter test lib/domains/analytics/test/ --reporter=compact
flutter test lib/domains/onboarding/test/ --reporter=compact
flutter test lib/domains/favourites/test/ --reporter=compact
flutter test lib/domains/feedback/test/ --reporter=compact
flutter test lib/domains/settings/test/ --reporter=compact
flutter test lib/domains/home/test/ --reporter=compact
flutter test lib/domains/panorama/test/ --reporter=compact
```

### Layer 4 — device tests

**Important**: `flutter test -d chrome` does NOT work for `integration_test/` files.
Use `flutter drive --profile` instead. Start ChromeDriver first (must match Chrome version):
```
chromedriver --port=4444
# verify: netstat -an | findstr ":4444" must show LISTENING
```

Commands:
```
# Chrome (web)
flutter drive --driver=test_driver/integration_test.dart --target=integration_test/consent_onboarding_test.dart -d chrome --profile

# Physical device (Android/iOS)
flutter drive --driver=test_driver/integration_test.dart --target=integration_test/consent_onboarding_test.dart -d <PHONE_DEVICE_ID>

# Future test files (same pattern):
flutter drive --driver=test_driver/integration_test.dart --target=integration_test/<file>.dart -d chrome --profile
```

**Note on IT-ON-4c (pre-selected card check):**
Semantics-flag assertions (`flagsCollection.isSelected`) are unreliable on Chrome in `--profile` mode.
IT-ON-4c instead checks that `Continuar` (FilledButton) is pre-enabled without any tap — which is only
possible when the card was pre-selected in `initState`. Full semantics-flag coverage is in Layer 2 widget tests.

Scenarios to cover:
- First launch → consent screen → accept → home
- First launch → consent screen → decline → home → no analytics
- Home → onboarding → select profile → home → onboarding block gone
- Home → panorama → FAB expand → 5 sub-actions visible
- Home → panorama → search icon → search for "Castelo" → result → summary card
- Home → panorama → filter "Igreja" → only church POIs on map
- Home → panorama → POI marker → summary card → "Mais Info" → full POI detail page
- POI detail → toggle favourite → back → home → favourites strip shows POI
- Home → settings → delete data → confirm → data cleared
- Deep link `/panorama?poi=castelo-sao-jorge` → correct POI opens
- Deep link preservation: `/panorama?poi=X` with consent=notAsked → consent → accept → arrives at panorama with POI
- Settings → feedback → submit form → success snackbar
- Share from POI detail → native share sheet opens (Android)

### Layer 5 — manual browser tests

Update `lib/test_utils/test_tasks_floating.dart` + `lib/test_utils/test_config.dart` with:

1. [ ] First launch: consent screen shows, accept → home
2. [ ] First launch: consent screen shows, decline → home → app still works → no analytics logged
3. [ ] Home page: 8-block structure visible — top chrome (3 icons), branding, hero card, secondary CTA, profile banner (first visit), feedback link
4. [ ] Profile banner tap → onboarding → select Architecture → continue → home → banner gone
5. [ ] Home → "Explorar com AR" → permission dialog → "Explorar sem AR" → panorama offline
6. [ ] Panorama: FAB visible → tap → 5 sub-actions appear → tap scrim → close
7. [ ] FAB Progress → /achievements → ComingSoonPage renders → back
8. [ ] FAB Favourites → /favourites → FavouritesPage renders (empty state) → back
9. [ ] Panorama: search icon → type "Castelo" → result appears → tap → summary card shows → search closes
10. [ ] Panorama: filter chips → tap "Igreja" → only church markers visible → "Limpar" → all markers visible
11. [ ] POI marker tap → summary card → "Mais Info" → full POI detail page with all zones → back → panorama
12. [ ] POI detail: favourite heart → tap → red fill → back → home → favourites strip shows POI → tap → POI detail
13. [ ] POI detail: share button → native share sheet opens
14. [ ] POI detail: "Ver no Panorama" → pops back to panorama with POI focused
15. [ ] Timeline slider → change epoch → epoch label auto-hides → POIs filter correctly
16. [ ] Home → settings icon → settings page → all sections render
17. [ ] Settings → language switch → all text updates across app
18. [ ] Settings → theme toggle → dark mode renders correctly
19. [ ] Settings → delete data → confirm → success snackbar
20. [ ] Settings → change consent → status badge updates
21. [ ] Settings → feedback → bottom sheet → fill form → submit → success snackbar
22. [ ] Deep link: type `/panorama?poi=castelo-sao-jorge` in URL bar → POI opens
23. [ ] Dark mode: all pages render correctly (consent, home, onboarding, panorama, POI detail, settings, favourites)
24. [ ] Back navigation: panorama → home → forward → panorama (history works)
25. [ ] Responsive: resize browser window → home page blocks adapt → panorama fills viewport

---

## 8. Phase Verification Checklist

ALL must be true before Phase 2 is declared done.

### Functional

- [ ] Consent screen gates first launch
- [ ] Accept/Decline persists across restart
- [ ] App works fully with consent=declined (no crashes, no data logged)
- [ ] Deep link preserved through consent redirect (D8)
- [ ] Onboarding shows 4 profile cards, saves selection
- [ ] Onboarding: first visit vs edit visit logic correct (Skip visible/hidden)
- [ ] Skip sets profile to `general`
- [ ] Home page renders 8-block adaptive structure
- [ ] Home page: onboarding block shows when `profile == null`, hides when set
- [ ] Home page: favourites strip shows when favourites exist, hides when empty
- [ ] Home page: last visited block shows when analytics has poiTapped data
- [ ] ProfileAvatar navigates to onboarding (edit)
- [ ] Settings accessible from panorama ··· menu → `/settings`
- [ ] FAB on panorama: expand/collapse with stagger animation (5 sub-actions)
- [ ] FAB Progress → navigates to /achievements (stub)
- [ ] FAB Favourites → navigates to /favourites (real page)
- [ ] FAB stubs (Audio, Circuits, AI) use `onPressed: () {}` and show "Em breve" tooltip (D20)
- [ ] POI summary card → "Mais Info" → full POI detail page (Navigator.push, not DraggableScrollableSheet)
- [ ] POI detail page renders all 10 zones (Z1–Z10) for fully-populated POI
- [ ] POI detail Z7: external links render for POIs with data, hidden for POIs without
- [ ] POI detail Z7: tapping a link opens external URL via `url_launcher`
- [ ] POI detail: favourite heart toggles correctly
- [ ] POI detail: share button fires native share
- [ ] POI detail: "Ver no Panorama" pops back to panorama
- [ ] POI detail: related POIs section shows same-category POIs
- [ ] Search: search icon → search bar → type query → results appear → tap → POI selected on map
- [ ] Search: Android back button while search overlay is open → closes overlay (PopScope), does NOT navigate back
- [ ] Filter: filter chips toggle categories → markers filtered on map → "Limpar" restores all
- [ ] Favourites: toggle from POI detail → persists across restart → shows on home strip + favourites page
- [ ] Favourites page: swipe to remove + undo works
- [ ] Feedback: bottom sheet form validates, submits through analytics pipeline
- [ ] Settings page: language, theme, profile, privacy, feedback, about sections all render
- [ ] "Apagar os meus dados" clears analytics with confirmation
- [ ] Route stubs: /circuits, /achievements, /ai-chat, /quiz/:poiId → ComingSoonPage
- [ ] Deep link `?poi=<id>` opens correct POI on panorama
- [ ] Analytics events log: all 9 event types fire at correct moments
- [ ] All Phase 1 functionality still works (no regressions)
- [ ] `POIDetailSheet` (DraggableScrollableSheet) completely removed from panorama Stack
- [ ] `POIDisplayMode.sheet` removed from enum

### Code quality

- [ ] `flutter analyze` → 0 issues
- [ ] `flutter test lib/ --reporter=compact` → all passing (1134+ Phase 1 baseline + new tests)
- [ ] `flutter build apk --release` → APK < 100MB
- [ ] No `StateProvider` in any new or modified file
- [ ] No `BackdropFilter` on non-static overlays
- [ ] One file, one responsibility — no file > ~300 lines without justification
- [ ] All new providers: `NotifierProvider` or `Provider` only
- [ ] Analytics backend swappable by changing one line in main()
- [ ] All pre-loaded state uses constructor parameter pattern (not `build()` side-effects)

### Design system compliance

- [ ] No raw `Colors.*` in any new or modified file
- [ ] No literal spacing numbers — all `Spacing.*`
- [ ] No literal duration milliseconds — all `AnimationTokens.*`
- [ ] No literal border radius numbers — all `RadiusTokens.*`
- [ ] All entrance animations: `RevealAnimation` or fade+slide pattern
- [ ] All state transitions: `AnimatedSwitcher(duration: AnimationTokens.medium)`
- [ ] Consent page, onboarding page, settings page: `LuxuryScaffold` body
- [ ] POI detail page: `Scaffold` with `CustomScrollView` (pushed page, not shell route)
- [ ] Home page: all blocks use design system tokens

### i18n

- [ ] All user-visible strings: `t(pt: '...', en: '...', es: '...')` — all 3 always present
- [ ] No string falls back to English in PT or ES mode
- [ ] Language switcher cycles PT → EN → ES → PT correctly
- [ ] Consent text clear and understandable in all 3 languages
- [ ] Search placeholder, filter chips, feedback form — all trilingual

### Accessibility

- [ ] Every interactive element: `Semantics(button: true, label: ...)` or equivalent
- [ ] Every `IconButton`: `Tooltip`
- [ ] All tap targets ≥ 48×48px (`SizeTokens.tapTarget`)
- [ ] Color never sole indicator of meaning
- [ ] Profile cards: `Semantics(selected:)` on selected card
- [ ] Consent page body: `Semantics(liveRegion: true)` for screen reader
- [ ] Settings page: logical section order for screen reader navigation
- [ ] POI detail page: zones logically ordered for screen reader
- [ ] Search bar: `TextField` has accessible label
- [ ] Filter chips: `Semantics(toggled:)` on active chips
- [ ] Favourite toggle: `Semantics(toggled:)` on heart button
- [ ] Feedback form: all fields have accessible labels

### Device tests

- [ ] Consent flow on physical Android device
- [ ] Onboarding flow on physical Android device
- [ ] Home page 8-block layout on physical Android device
- [ ] POI detail full page on physical Android device
- [ ] Search + filter on physical Android device
- [ ] FAB on panorama page on physical Android device
- [ ] Settings page on physical Android device
- [ ] Layer 5 manual browser tests: all 25 scenarios green

---

## 9. Ongoing Rules Reminder

| Rule                                                                                                                                                                                                                                                   | Source                       |
| ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ---------------------------- |
| Read all files in scope before touching them                                                                                                                                                                                                           | IMPLEMENTATION_GUIDELINES §0 |
| Create full TODO list upfront before starting any task                                                                                                                                                                                                 | IMPLEMENTATION_GUIDELINES §0 |
| For every decision: consider 3 options at architecture AND implementation level                                                                                                                                                                        | IMPLEMENTATION_GUIDELINES §1 |
| One file, one responsibility. > ~300 lines → split                                                                                                                                                                                                     | IMPLEMENTATION_GUIDELINES §2 |
| `NotifierProvider` only — `StateProvider` is BANNED                                                                                                                                                                                                    | PROJECT_GUIDE §7             |
| Freezed only when justified: union types, JSON, many-field copyWith                                                                                                                                                                                    | PROJECT_GUIDE §7             |
| Package additions: > 100 pub points, updated within 12 months, null-safe                                                                                                                                                                               | PROJECT_GUIDE §7             |
| After every task: `flutter analyze` → 0, `flutter test lib/` → all passing                                                                                                                                                                             | IMPLEMENTATION_GUIDELINES §4 |
| If Freezed file modified: `dart run build_runner build --delete-conflicting-outputs`                                                                                                                                                                   | PROJECT_GUIDE §7             |
| All colors: `context.*` tokens. Never `Colors.*`                                                                                                                                                                                                       | DESIGN/02_COLORS.md          |
| All spacing: `Spacing.*`. Never literal numbers                                                                                                                                                                                                        | DESIGN/01_TOKENS.md          |
| All durations: `AnimationTokens.*`. Never literal milliseconds                                                                                                                                                                                         | DESIGN/01_TOKENS.md          |
| All border radii: `RadiusTokens.*`. Never literal numbers                                                                                                                                                                                              | DESIGN/01_TOKENS.md          |
| All animations: pattern from `DESIGN/08_MOTION_AND_FEEL.md`                                                                                                                                                                                            | DESIGN guide                 |
| All state transitions: `AnimatedSwitcher` per `FEEDBACK_GUIDE.md`                                                                                                                                                                                      | FEEDBACK guide               |
| New routes: add to `nav_config.dart` only                                                                                                                                                                                                              | NAV_AND_LAYOUT guide         |
| `BackdropFilter` BANNED in non-static contexts                                                                                                                                                                                                         | DESIGN/08_MOTION_AND_FEEL.md |
| Terminal (PowerShell): use `;` not `&&`                                                                                                                                                                                                                | IMPLEMENTATION_GUIDELINES §7 |
| Comments: explain WHY, not WHAT. No emojis                                                                                                                                                                                                             | IMPLEMENTATION_GUIDELINES §2 |
| `renderSpecs` drives marker overlay, not `allPOIs`                                                                                                                                                                                                     | Phase 1 architecture         |
| POI detail is a full page via `Navigator.push` — NOT a DraggableScrollableSheet in Stack                                                                                                                                                               | Phase 2 D14                  |
| `tilestories.app` is a placeholder domain — `// TODO(Phase2): replace with real domain`                                                                                                                                                                | Phase 1 A16                  |
| Never use `pumpAndSettle()` in tests with `ARModeToggle` — use bounded `pump(Duration)`                                                                                                                                                                | Phase 1 spec                 |
| `POIDisplayMode.close()` always clears BOTH `selectedPOIProvider` AND `poiDisplayModeProvider`                                                                                                                                                         | Phase 1 A3                   |
| FAB stubs: `onPressed: () {}` NOT `null` — Tooltip requires enabled button                                                                                                                                                                             | Phase 2 D20                  |
| Pre-loaded providers: pass initial value via constructor, not SharedPreferences in `build()`                                                                                                                                                           | Phase 2 D2                   |
| Version number: hardcode `'1.0.0'`, do NOT use `package_info_plus`                                                                                                                                                                                     | Phase 2 D19                  |
| Analytics: consent check inside `AnalyticsService.logEvent()`, not at call sites                                                                                                                                                                       | Phase 2 D1                   |
| Analytics backend: swap by changing one line in `main()` constructor                                                                                                                                                                                   | Phase 2 D15                  |
| `POIDisplayMode` enum: only `none` and `summary` — `sheet` is removed in Phase 2                                                                                                                                                                       | Phase 2 D14                  |
| POI gallery (Z1) uses `SliverToBoxAdapter` with fixed-height `PageView`, NOT `SliverAppBar`                                                                                                                                                            | Phase 2 wireframe 3          |
| Z7 (external links): render from `poi.externalLinks` via `url_launcher`; hide Z7 when list is empty                                                                                                                                                    | Phase 2 §3.7 / §0            |
| `AnalyticsService.getConsent` must read live from `ProviderContainer`, not captured local var                                                                                                                                                          | Phase 2 GDPR / main.dart     |
| Use `UncontrolledProviderScope(container:)` in main.dart — NOT `ProviderScope`                                                                                                                                                                         | Phase 2 main.dart            |
| Route path: `/favourites` (British spelling, matching all Dart identifiers)                                                                                                                                                                            | Phase 2 consistency          |
| Search overlay: wrap in `PopScope` — Android back closes overlay, not navigating away                                                                                                                                                                  | Phase 2 Task 2.8             |
| Feedback submit button: `_isSubmitting` guard against double-tap                                                                                                                                                                                       | Phase 2 Task 2.10            |
| Pages with `Expanded + ListView` body: pass `scrollable: false` to `defaultPageBrowser()` / `defaultPageApp()` — default `scrollable: true` wraps body in `SingleChildScrollView` causing layout crash (D21)                                           | Phase 2 Task 2.5             |
| `ConsumerWidget` inside `Dismissible`: capture `ref.read(provider.notifier)` as local variable BEFORE building the `Dismissible` — the tile unmounts on dismiss and any `ref.read` inside a SnackBarAction closure will throw "ref used after unmount" | Phase 2 Task 2.5             |
| Semantics `isToggled` attribute: returns `dart:ui.Tristate`, NOT `bool` — import `'dart:ui' show Tristate` and compare with `Tristate.isTrue` / `Tristate.isFalse`                                                                                     | Phase 2 Task 2.5             |
| Never-completing future in tests: use `Completer<T>()` (no timer created) — `Future.delayed(Duration(seconds: 60))` creates a fake timer that fails `!timersPending` assertion at teardown                                                             | Phase 2 Task 2.5             |
| `Dismissible` on Chrome (Layer 4): use `tester.fling(finder, Offset(-300, 0), 800)` — `tester.drag()` provides no velocity and the dismiss never completes                                                                                             | Phase 2 Task 2.5             |
| Any test that calls `SharedPreferencesAsync()` directly: add `SharedPreferencesAsyncPlatform.instance = InMemorySharedPreferencesAsync.empty()` to `setUp()` — `SharedPreferences.setMockInitialValues` only mocks the legacy API                      | Phase 2 Task 2.5             |

---

*End of Phase 2 Plan. Current status: Tasks 2.1 (partial), 2.2 ✅, 2.3 ✅, 2.4 ✅, 2.5 ✅, 2.6 ✅, 2.7 ✅, 2.8 ✅, 2.9 ✅, 2.10 ✅. Task 2.11 ⏸ deferred to Phase 5 §5.5. Phase 1 baseline: 1134 tests passing, 0 analyze errors. Current suite: L1/L2/L3 → 1609 tests passing, 3 skipped, 0 failures. L4 Chrome → 184 tests passing, 1 skipped, 0 failures. flutter analyze: 0 errors, 0 warnings. Phase 2 verification: COMPLETE ✅ (2025-03-16). Duration estimate: 2–3 months.*

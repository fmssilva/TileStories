# PHASE_0_PLAN.md
> **Purpose**: Detailed work plan for Phase 0 — Cleanup and foundation.  
> Read `PROJECT_GUIDE.md` and `PROJECT_STATUS.md` in full before starting.  
> Complete all tasks in order. Do not start Phase 1 until §0.9 passes.

---

## STATUS SUMMARY (as of 2026-03-10)

| Task                                                                     | Status               | Notes                                                                                                                           |
| ------------------------------------------------------------------------ | -------------------- | ------------------------------------------------------------------------------------------------------------------------------- |
| **0.1** — Verify no `photo_view` references                              | ✅ DONE               | `photo_view` removed from pubspec, source, and all zoom files                                                                   |
| **0.2** — Rename typo files (`breadcrums`, `fab_wraper`, `simple_sroll`) | ✅ DONE               | Files renamed; imports and comment references updated                                                                           |
| **0.3a** — Uncomment Spanish in `Language` enum                          | ✅ DONE               | `spanish('es', 'Español', '🇪🇸')` active                                                                                          |
| **0.3b** — Add `es` field to `TranslatableString` + `t()`                | ✅ DONE               | `String? es` with English fallback; `translate()` and `t()` updated                                                             |
| **0.3c** — Run `build_runner`                                            | ✅ DONE               | `.freezed.dart` and `.g.dart` regenerated                                                                                       |
| **0.3d** — Verify `Language.fromCode('es')`                              | ✅ DONE               | Iterates `Language.values`; Spanish returned correctly                                                                          |
| **0.3e** — Fix `language_switcher.dart`                                  | ✅ DONE               | `Colors.green` → `context.success`; `Tooltip`+`Semantics` added; hardcoded `TextStyle` sizes replaced                           |
| **0.3f** — New i18n unit + widget tests                                  | ✅ DONE               | `language_test.dart` and `language_switcher_test.dart` created and green                                                        |
| **0.3g** — Update `I18N_GUIDE.md` for Spanish                            | ✅ DONE               | `es:` documented as optional with English fallback                                                                              |
| **0.4a** — Fix `ar_poi_marker.dart` raw `Colors.*`                       | ✅ DONE               | 5 category colors replaced with design tokens; `context` passed to `_getMarkerColor()`                                          |
| **0.4a** — Update `ar_poi_marker_test.dart` color assertions             | ✅ DONE               | Assertions updated to match new token values                                                                                    |
| **0.4d** — Grep `BrandColors.*` outside `lib/design/`                    | ✅ DONE               | 12 violations fixed: `nav_tabs_row.dart` (6), `nav_accordion.dart` (4), `base_accordion.dart` (1), `language_switcher.dart` (1) |
| **0.4e** — Spot-check hardcoded tokens in touched files                  | ✅ DONE               | `language_switcher.dart` `TextStyle` literals replaced                                                                          |
| **0.5a** — Remove calibrator route from nav_config                       | ✅ DONE               | Import and `NavItem` block deleted                                                                                              |
| **0.5b** — Gate demo routes behind `kDebugMode`                          | ✅ DONE               | `navigationConfig` is now a getter; demo routes in `_debugNavRoutes`                                                            |
| **0.5c** — Remove stale `lib/domains/demo_navigation/` folder            | ✅ DONE (pre-Phase 0) | Folder deleted; no dangling imports                                                                                             |
| **0.6** — Remove test banner from `home_page.dart`                       | ✅ DONE               | Green `Container` block deleted                                                                                                 |
| **0.7a** — Move `ar_poi_calibrator.html` to `PROJECT_GUIDES/`            | ✅ DONE               | HTML moved; original deleted                                                                                                    |
| **0.7b** — Add usage comment block to calibrator HTML                    | ✅ DONE               | Full comment at top of file                                                                                                     |
| **0.7c** — Delete `poi_calibrator_page.dart`                             | ✅ DONE               | No remaining imports; file deleted                                                                                              |
| **0.7d** — Extract `viewport_math.dart`; update barrel + tests           | ✅ DONE               | `FitToViewportResult`/`computeFitToViewport`/`computeMarkerScreenPosition` moved; `CalibratorPoi` deleted                       |
| **0.7e** — Delete empty `lib/components/ui/` folder                      | ✅ DONE               | Folder removed                                                                                                                  |
| **0.8a** — Verify `SizeTokens.tapTarget` exists                          | ✅ DONE               | `tapTarget = 48.0` confirmed                                                                                                    |
| **0.8b** — Contrast audit comment in `app_theme.dart`                    | ✅ DONE               | WCAG AA ratios documented in `color_scheme_builder.dart`; runtime validation in `main.dart`                                     |
| **0.8c** — Semantics on nav items                                        | ✅ DONE               | `language_switcher.dart` fixed in §0.3e; others already had Semantics                                                           |
| **0.8d** — Tap targets on nav interactive elements                       | ✅ DONE               | All use `IconButton` (48px default); no overrides found                                                                         |
| **0.8e** — Verify focus rings not suppressed                             | ✅ DONE               | No `focusColor: Colors.transparent` violations found                                                                            |
| **0.8f** — Keyboard nav on custom interactive widgets                    | ✅ DONE (audit only)  | `GestureDetector`/`InkWell` usage reviewed; TODOs left for Phase 1 where needed                                                 |
| **0.9** — Final `flutter analyze`                                        | ✅ DONE               | 0 errors, 0 warnings (ran in 5.3s)                                                                                              |
| **0.9** — Final `flutter test lib/`                                      | ✅ DONE               | 763 passed, 9 pre-existing failures (7 camera plugin + 2 SharedPreferences); 0 regressions introduced                           |
| **Zoom tests** (`zoom_gesture_detector` double-tap)                      | ⏭ DEFERRED           | 2 integration test failures in Journey 2 double-tap deferred to Phase 1 cleanup; does not block Phase 0                         |

> **Phase 0 is complete.** All cleanup tasks done. Phase 1 may begin.

---

---

## Before you start

- [ ] Run `flutter test lib/ --reporter=compact` and record the exact pass/fail counts. This is your regression baseline. Any test that was passing before Phase 0 must still pass at §0.9. Do not proceed if you cannot establish a clean baseline first.
- [ ] Run `flutter analyze` and record any pre-existing warnings/errors — you are not responsible for fixing issues that existed before Phase 0, but you must not introduce new ones.

---

## 0.1 — `photo_view` already removed (verify only)

`photo_view` is **not in `pubspec.yaml`** — it was already removed. Nothing to do except confirm:

- [ ] Grep for `photo_view` in `lib/` and `pubspec.yaml` — confirm zero results
- [ ] If found anywhere: remove the import/reference

---

## 0.2 — Rename typo files and fix all imports

Two files have typo names. Rename them and update every import site.

**Files to rename:**
```
lib/layout/widgets/breadcrums.dart  →  lib/layout/widgets/breadcrumbs.dart
lib/layout/widgets/fab_wraper.dart  →  lib/layout/widgets/fab_wrapper.dart
lib/navigation/test/simple_sroll/   →  lib/navigation/test/simple_scroll/
```

The `simple_sroll/` folder name is a typo. Rename the folder and update every import inside test files that reference it by relative path. The nested scroll test files reference it in comments — update those comments too.

**Steps:**
- [ ] Rename `breadcrums.dart` → `breadcrumbs.dart`
- [ ] Rename `fab_wraper.dart` → `fab_wrapper.dart`
- [ ] Rename folder `lib/navigation/test/simple_sroll/` → `lib/navigation/test/simple_scroll/`
- [ ] Search all `.dart` files for `breadcrums`, `fab_wraper`, and `simple_sroll` — update every import and every comment reference
- [ ] `flutter analyze` → 0 issues

---

## 0.3 — Add Spanish to Language enum and TranslatableString

**Context:** Spanish is a third-party locale for museum visitors. Adding `es` now (Phase 0) costs almost nothing — strings can be `null` and fall back to English. Adding it in Phase 4 would mean touching every model, every POI JSON entry, and re-running build_runner across a much larger codebase.

The current `Language` enum and `TranslatableString` have Spanish commented out. Uncomment and activate.

### 0.3a — `Language` enum

**File:** `lib/utils/i18n/models/language.dart`

First, check the actual state of the file before acting:
- If Spanish is **commented out** (lines beginning `//`): uncomment
- If Spanish is **entirely absent**: add it

From the last codebase read, it IS commented out — lines 25–26 contain `// @JsonValue('es')` and `// spanish('es', 'Español', '🇪🇸'),`. Uncomment both lines. Either way, the end result is:

```dart
@JsonValue('es')
spanish('es', 'Español', '🇪🇸'),
```

Remove the comment `// Future languages - uncomment when ready to add:` and the trailing `;` placeholder if present — the enum should just list all three values cleanly.

- [ ] Uncomment (or add) the Spanish entry in `Language` enum
- [ ] Clean up the comment block around it

### 0.3b — `TranslatableString` Freezed model

**File:** `lib/utils/i18n/models/translatable_string.dart`

```dart
// Change FROM:
const factory TranslatableString({
  required String pt,
  required String en,
  // String? es,
}) = _TranslatableString;

// Change TO:
const factory TranslatableString({
  required String pt,
  required String en,
  String? es,       // nullable — falls back to en if null
}) = _TranslatableString;
```

Update the `translate()` method — uncomment the Spanish case:
```dart
String translate(Language language) {
  switch (language) {
    case Language.portuguese:
      return pt;
    case Language.english:
      return en;
    case Language.spanish:
      return es ?? en;  // fall back to English if es is null
  }
}
```

Update the shorthand `t()` function — uncomment `es`:
```dart
TranslatableString t({
  required String pt,
  required String en,
  String? es,
}) {
  return TranslatableString(pt: pt, en: en, es: es);
}
```

- [ ] Uncomment `String? es` in factory constructor
- [ ] Uncomment `case Language.spanish: return es ?? en;` in `translate()`
- [ ] Uncomment `String? es` param and `es: es` in the `t()` shorthand
- [ ] Clean up comment blocks (`// Future:`, `// String? es,`)

### 0.3c — Regenerate Freezed files

```
dart run build_runner build --delete-conflicting-outputs
```

This will regenerate `translatable_string.freezed.dart` and `translatable_string.g.dart`. All existing call sites still compile because `es` is nullable — no existing `TranslatableString(pt: '...', en: '...')` calls break.

- [ ] Run `build_runner` — 0 errors
- [ ] `translatable_string.freezed.dart` and `translatable_string.g.dart` updated
- [ ] Load the app (`flutter run -d chrome` or on device) and navigate to any screen that loads `pois.json` — confirm no JSON parse error in the console. The added nullable `es` field must not break existing JSON deserialization of existing records that have no `es` key.

### 0.3d — `language_provider.dart`

**File:** `lib/utils/i18n/providers/language_provider.dart`

No structural change needed — `Language.values` already includes `spanish` after the enum change, and `Language.fromCode('es')` will work via the existing `fromCode()` method.

- [ ] Verify `Language.fromCode('es')` returns `Language.spanish` (read the `fromCode` implementation — it should iterate `Language.values` and match `.code`)
- [ ] If `fromCode` uses a hardcoded list or switch, add the Spanish case

### 0.3e — `language_switcher.dart`

**File:** `lib/utils/i18n/widgets/language_switcher.dart`

The switcher uses `Language.values.map(...)` — it will automatically include Spanish. However the checkmark icon uses `Colors.green` (raw color). Fix it now while touching this file:

```dart
// Change FROM:
const Icon(Icons.check, size: 16, color: Colors.green)

// Change TO:
Icon(Icons.check, size: 16, color: context.success)
```

Also add `Semantics` and `Tooltip` to the `PopupMenuButton`:
```dart
return Tooltip(
  message: ref.tr(t(pt: 'Selecionar idioma', en: 'Select language', es: 'Seleccionar idioma')),
  child: Semantics(
    label: ref.tr(t(pt: 'Seletor de idioma', en: 'Language selector', es: 'Selector de idioma')),
    child: PopupMenuButton<Language>(...)
  ),
);
```

- [ ] Fix `Colors.green` → `context.success`
- [ ] Add `Tooltip` + `Semantics` wrapper

### 0.3f — Verification and new tests

**Run existing tests:**
- [ ] `dart run build_runner build --delete-conflicting-outputs` → 0 errors
- [ ] `flutter analyze` → 0 issues
- [ ] `flutter test lib/ --reporter=compact` → no regressions (all tests that passed before still pass)
- [ ] Run app on device/Chrome: language switcher shows PT / EN / ES; switching works; preference saved across restart

**Write new tests for the Phase 0 Spanish changes:**

**Guide rule (`IMPLEMENTATION_GUIDELINES.md §5`):** Every code change in Phase 0 that modifies a model or widget requires new tests. Test location: co-located in `lib/utils/i18n/test/unit/` and `lib/utils/i18n/test/widgets/`.

Create `lib/utils/i18n/test/unit/language_test.dart` (if it doesn't already exist):
```dart
// Unit tests for Language enum and TranslatableString with Spanish
void main() {
  group('Language.fromCode', () {
    test('returns portuguese for pt', () {
      expect(Language.fromCode('pt'), Language.portuguese);
    });
    test('returns english for en', () {
      expect(Language.fromCode('en'), Language.english);
    });
    test('returns spanish for es', () {
      expect(Language.fromCode('es'), Language.spanish);
    });
    test('falls back to english for unknown code', () {
      expect(Language.fromCode('xx'), Language.english); // verify actual fallback behavior
    });
  });

  group('TranslatableString.translate', () {
    test('returns es when language is spanish and es is provided', () {
      final ts = TranslatableString(pt: 'Olá', en: 'Hello', es: 'Hola');
      expect(ts.translate(Language.spanish), 'Hola');
    });
    test('falls back to en when language is spanish and es is null', () {
      final ts = TranslatableString(pt: 'Olá', en: 'Hello');
      expect(ts.translate(Language.spanish), 'Hello');
    });
    test('returns pt when language is portuguese', () {
      final ts = TranslatableString(pt: 'Olá', en: 'Hello', es: 'Hola');
      expect(ts.translate(Language.portuguese), 'Olá');
    });
  });
}
```

Create `lib/utils/i18n/test/widgets/language_switcher_test.dart` (if it doesn't already exist):
```dart
// Widget test: switcher shows all 3 language options after Spanish is active
void main() {
  testWidgets('LanguageSwitcher shows PT, EN, ES options', (tester) async {
    await tester.pumpWidget(
      ProviderScope(
        child: MaterialApp(
          home: Scaffold(body: const LanguageSwitcher()),
        ),
      ),
    );
    // Open the popup
    await tester.tap(find.byType(PopupMenuButton<Language>));
    await tester.pumpAndSettle();
    // All 3 options must be visible
    expect(find.text('Português'), findsOneWidget);
    expect(find.text('English'), findsOneWidget);
    expect(find.text('Español'), findsOneWidget);
  });

  testWidgets('LanguageSwitcher has Semantics label', (tester) async {
    await tester.pumpWidget(
      ProviderScope(
        child: MaterialApp(
          home: Scaffold(body: const LanguageSwitcher()),
        ),
      ),
    );
    expect(
      find.bySemanticsLabel(RegExp(r'(Language selector|Seletor de idioma|Selector de idioma)')),
      findsOneWidget,
    );
  });
}
```

- [ ] Create `lib/utils/i18n/test/unit/language_test.dart` with the unit tests above  
- [ ] Create `lib/utils/i18n/test/widgets/language_switcher_test.dart` with the widget tests above  
- [ ] `flutter test lib/utils/i18n/test/ --reporter=compact` → all green  
- [ ] If `Language.fromCode` has a different fallback behavior than assumed: update the test to match the actual behavior (don't change the implementation to match the test)

### 0.3g — Update `I18N_GUIDE.md` to document Spanish

**Guide rule (`LANGUAGE_SEO_ACCESSIBILITY/I18N_GUIDE.md`):** The guide currently shows `t(pt: '...', en: '...')` as the canonical pattern. After Phase 0 adds Spanish, the guide is stale.

**File:** `PROJECT_GUIDES/LANGUAGE_SEO_ACCESSIBILITY/I18N_GUIDE.md`

- [ ] Find all `t(pt:` examples in the guide and add `es:` as an optional third parameter to at least the first full example, with a note explaining `es` is optional (falls back to `en`)
- [ ] Add a note near the `t()` function docs: "Spanish (`es:`) is optional — omit when not yet translated. The app falls back to English."
- [ ] The parameter order is fixed: `t(pt:, en:, es:)` — document this explicitly

> This is a documentation change only. No Dart code change needed.

> **Do NOT fill in `es` strings** on existing models or POI JSON yet. Leave them as `es: null` (omitted). Each domain fills `es` when touched in Phase 1+.

---

## 0.4 — Fix design-system violations in existing code

### 0.4a — `ar_poi_marker.dart`

**File:** `lib/domains/panorama/ar/widgets/ar_poi_marker.dart`

This file uses raw `Colors.*` for category marker colors. The design system has semantic colors (`context.primary`, `context.tertiary`, `context.warning`, etc.) plus a `SemanticColors` extension (`context.success`, `context.info`).

Map the categories to design tokens:

| Category    | Raw color used  | Replace with               | Rationale                        |
| ----------- | --------------- | -------------------------- | -------------------------------- |
| `power`     | `Colors.amber`  | `context.tertiary`         | Tertiary = Gold from logo        |
| `religious` | `Colors.purple` | `context.secondary`        | Secondary = Azulejo Blue         |
| `civil`     | `Colors.blue`   | `context.primary`          | Primary = Deep Blue              |
| `maritime`  | `Colors.cyan`   | `context.info`             | Info semantic = Sky blue         |
| default     | `Colors.grey`   | `context.onSurfaceVariant` | Muted token for unknown category |

**Before applying these mappings**, open `lib/design/theme/theme_extensions.dart` and `lib/design/theme/app_theme.dart` to confirm what each token actually resolves to. Verify the visual meaning makes sense for each POI category. The mappings above are based on the current token definitions (primary = Deep Blue #1E3A8A, secondary = Azulejo Blue #3B82F6, tertiary = Gold #D97706, info = Sky blue from SemanticColors). If a future token redesign changes these values, the mappings may need adjustment.

The widget also uses `Colors.white` and `Colors.black` for the label overlay (dark scrim + white text on top of AR camera feed). These are intentional contrast choices for legibility over live camera — **keep them as-is** and add a comment explaining why: `// AR overlay: literal black/white required for legibility over live camera — not a design system violation`. The rule "never use raw Colors.*" applies to UI theming; it does not apply to content that must remain legible regardless of theme.

Because `_getMarkerColor()` is a non-`build` method with no `BuildContext`, you need to pass `BuildContext` to it or move it inline into `build`. Preferred approach: pass `context` as a parameter.

```dart
// Change _getMarkerColor() signature:
Color _getMarkerColor(BuildContext context) { ... }

// Call site in build():
final markerColor = _getMarkerColor(context);
```

**Import rule (`DESIGN/00_INDEX.md`):** When adding `context.*` shortcuts (`context.tertiary`, `context.info`, `context.onSurfaceVariant`, etc.), ensure `ar_poi_marker.dart` imports the design system via the single barrel import — **not** individual token files:

```dart
// Correct — single import
import 'package:grande_panorama_ar/design/design_system.dart';

// Wrong — individual token file imports
import 'package:grande_panorama_ar/design/tokens/brand_colors.dart';
import 'package:grande_panorama_ar/design/theme/theme_extensions.dart';
```

If the file currently has individual design imports, replace them all with the single `design_system.dart` import.

- [ ] Replace the 5 raw `Colors.*` category colors with design tokens
- [ ] Pass `context` to `_getMarkerColor()`
- [ ] Confirm `Colors.white` / `Colors.black` label overlay lines are kept (intentional)
- [ ] `flutter analyze` → 0 issues
- [ ] `flutter test lib/domains/panorama/test/ --reporter=compact` → all green

> **Note on the existing tests:** `ar_poi_marker_test.dart` tests specific color values (e.g., `Colors.amber = 0xFFFFC107`). These tests will break after this change. Update them to assert on the design token colors instead. Read the test file first to understand what it currently asserts before editing the widget.

### 0.4b — `poi_marker.dart`

**File:** `lib/domains/panorama/widgets/poi_marker.dart`

`poi_marker.dart` line 37: `Colors.white` for the border when not selected. This is a deliberate contrast choice (white border on dark AR background) — **keep it**. Line 59: `Colors.black.withAlpha(...)` shadow — deliberate shadow — **keep it**.

- [ ] Read the file and confirm no other raw `Colors.*` exist besides the intentional overlay colors
- [ ] If any non-intentional `Colors.*` are found: replace with design tokens

### 0.4c — Verify `language_switcher.dart` fix applied

Already done in §0.3e. Confirm here.

- [ ] `Colors.green` checkmark already replaced with `context.success`

### 0.4d — Check for `BrandColors.*` direct usage in UI code

**Guide rule (`DESIGN/02_COLORS.md`):** "Never use raw `BrandColors.*` in UI — they don't adapt to dark mode and contrast isn't guaranteed." `BrandColors` is a raw palette. Only `lib/design/` internals are allowed to reference it. All other files must use `context.*` shortcuts.

```
grep -r "BrandColors\." lib/ --include="*.dart"
```

- [ ] Run the grep. If any result is outside `lib/design/`: replace with the appropriate `context.*` token.
- [ ] Common substitutions: `BrandColors.deepBlue500` → `context.primary`, `BrandColors.azulejoBlue500` → `context.secondary`, `BrandColors.gold500` → `context.tertiary`
- [ ] `flutter analyze` → 0 issues after any replacements

### 0.4e — Spot-check hardcoded token values in files being touched

**Guide rule (`DESIGN/00_INDEX.md`):** "Never hardcode a value — there is a token for everything."  
**Guide rule (`DESIGN/04_LAYOUT.md` ❌ Never):** `EdgeInsets.all(16)` → use `Spacing.lg` or `context.responsivePadding`  
**Guide rule (`DESIGN/03_TYPOGRAPHY.md` ❌ Never):** `TextStyle(fontSize: 24)` → use `context.headlineSmall`

**Scope: only files already modified in Phase 0 — do not scan the whole codebase.**

For each file you touch in Phase 0 (listed in §Summary), check for these patterns and fix them:

| Hardcoded value                                        | Replace with                                                  |
| ------------------------------------------------------ | ------------------------------------------------------------- |
| `EdgeInsets.all(16)` / `.symmetric(horizontal: 16)`    | `Spacing.lg` / `EdgeInsets.symmetric(horizontal: Spacing.lg)` |
| `EdgeInsets.all(8)` / `EdgeInsets.all(12)`             | `Spacing.sm` / `Spacing.md`                                   |
| `BorderRadius.circular(8)` etc.                        | `RadiusTokens.sm` / `.md` / `.lg` / `.cardRadius`             |
| `TextStyle(fontSize: 20)`                              | `context.titleLarge` or appropriate scale step                |
| `TextStyle(fontSize: 12, fontWeight: FontWeight.bold)` | `context.labelMedium.copyWith(fontWeight: FontWeight.bold)`   |
| `TextStyle(fontWeight: FontWeight.bold)`               | `.copyWith(fontWeight: FontWeight.bold)` on a context style   |
| `SizedBox(width: 4)` / `SizedBox(width: 12)`           | `SizedBox(width: Spacing.xs)` / `SizedBox(width: Spacing.md)` |

> **`language_switcher.dart` specific:** The current widget uses `const TextStyle(fontSize: 20)` for the flag and `const TextStyle(fontSize: 12, fontWeight: FontWeight.bold)` for the language code. Replace both — the flag can use `context.titleLarge` (22px is close to 20px flag display) and the code label can use `context.labelMedium.copyWith(fontWeight: FontWeight.bold)` (12px bold = labelMedium bold).

> **`home_page.dart` specific:** The test banner being deleted in §0.6 uses `TextStyle(color: Colors.white, fontWeight: FontWeight.bold, fontSize: 16)`. Since the whole block is deleted, no fix needed — just delete it.

- [ ] For each Phase 0 file: grep for `EdgeInsets`, `BorderRadius.circular`, `TextStyle(fontSize:`, `SizeBox(width:` with literal values
- [ ] Replace hardcoded values with the appropriate token
- [ ] `flutter analyze` → 0 issues

---

## 0.5 — Remove/gate demo routes and calibrator route

### 0.5a — Remove the calibrator route from nav_config

**File:** `lib/navigation/navConfig/nav_config.dart`

The `poi-calibrator` route is no longer an in-app page (the HTML calibrator replaces it — see §0.7). Remove it entirely:

- [ ] Delete the `NavItem` block for `id: 'poi-calibrator'` from `navigationConfig`
- [ ] Delete the import on line 5: `import '../../ar_core/utils/poi_calibrator_page.dart';`

### 0.5b — Gate demo routes behind `kDebugMode`

The 4 demo nav routes (`demo-nav1..4` with their children) are development-only. They should not exist in production builds.

**Strategy:** wrap the demo `NavItem` entries in a conditional so they are only included in debug builds.

The `navigationConfig` list is currently a top-level `final List<NavItem>`. Change it to a getter or a function so it can conditionally include items:

```dart
import 'package:flutter/foundation.dart' show kDebugMode;

List<NavItem> get navigationConfig => [
  // ... home, panorama (always present)

  // Debug-only routes — removed in release builds
  if (kDebugMode) ..._debugNavRoutes,
];

// Note: cannot be const because NavItem may not be const
final List<NavItem> _debugNavRoutes = [
  NavItem(id: 'demo-nav1', ...),
  NavItem(id: 'demo-nav2', ...),
  NavItem(id: 'demo-nav3', ...),
  NavItem(id: 'demo-nav4', ...),
];
```

> **Dart const note:** `kDebugMode` is a compile-time constant (`const bool.fromEnvironment('dart.vm.product')`), so `if (kDebugMode) ...list` inside a list literal is valid Dart — the list does NOT need to be const for this to work. However, `NavItem` likely has non-const fields (e.g., builder functions), which means the outer list cannot be `const` anyway. Use a regular `final` getter, not a `const` list. If making it a getter causes any compile error, convert to a regular function `getNavigationConfig()` and update all call sites.

> **Important:** check how `navigationConfig` is consumed in `router_config.dart` and `nav_tabs_row.dart`. If it is currently imported as a `final` variable, switching to a getter requires no call site changes (getters are accessed the same way as fields). If it was a `const`, all const usages must be updated.

- [ ] Change `navigationConfig` from a `final` list to a getter
- [ ] Move demo `NavItem` entries to `_debugNavRoutes` wrapped in `if (kDebugMode)`
- [ ] Confirm `router_config.dart`, `nav_tabs_row.dart`, and any other consumer compiles correctly
- [ ] `flutter analyze` → 0 issues
- [ ] Verify in debug build: demo routes visible in nav; in release build: not present

### 0.5c — ~~Resolve `lib/domains/demo_navigation/` folder~~ ✅ DONE

**Status: completed before Phase 0 execution — folder deleted manually.**

**What was there:** `lib/domains/demo_navigation/demo_nav2_page.dart` — a single file, a stale duplicate of `lib/navigation/test/demo_pages/demo_nav2_page.dart`.

**Why it was safe to delete immediately:**
- `nav_config.dart` already imported from `lib/navigation/test/demo_pages/demo_nav2_page.dart` (line 10) — the `domains/` copy was never imported by any production file.
- Grep confirmed 0 imports of the `domains/demo_navigation/` path anywhere in `lib/`.
- The `lib/navigation/test/demo_pages/` copy is the canonical version and is already wired up correctly.

**Where the demo navigation pages live (permanently):**  
`lib/navigation/test/demo_pages/` — all 7 pages (`demo_nav1_page.dart`, `demo_nav1_child_a/b/c_page.dart`, `demo_nav2_page.dart`, `demo_nav3_page.dart`, `demo_nav4_page.dart`). These are navigation stress-test fixtures, not domain features, so `lib/navigation/test/` is their correct home.

- [x] `lib/domains/demo_navigation/` deleted
- [x] `flutter analyze` → 0 issues confirmed (no dangling imports)

---

## 0.6 — Remove test marker from `home_page.dart`

**File:** `lib/domains/home/pages/home_page.dart`  
**Line ~140–159**

Delete the entire test-marker `Container` block:

```dart
// DELETE this entire block:
Container(
  margin: const EdgeInsets.only(bottom: Spacing.md),
  padding: const EdgeInsets.symmetric(
    horizontal: Spacing.lg,
    vertical: Spacing.sm,
  ),
  decoration: BoxDecoration(
    color: Colors.green,
    borderRadius: BorderRadius.circular(RadiusTokens.md),
  ),
  child: const Text(
    'Hello from TileStories! 👋  (phone test v1)',
    style: TextStyle(
      color: Colors.white,
      fontWeight: FontWeight.bold,
      fontSize: 16,
    ),
  ),
),
```

- [ ] Delete the block
- [ ] `flutter analyze` → 0 issues
- [ ] Run app and confirm green banner is gone from home page

---

## 0.7 — Move calibrator HTML; delete calibrator Dart files

### 0.7a — Move HTML tool

The `ar_poi_calibrator.html` is already in `lib/ar_core/utils/ar_poi_calibrator.html`.  
Move it to `PROJECT_GUIDES/`:

- [ ] Copy `lib/ar_core/utils/ar_poi_calibrator.html` → `PROJECT_GUIDES/ar_poi_calibrator.html`
- [ ] Delete `lib/ar_core/utils/ar_poi_calibrator.html`

### 0.7b — Add explanatory comment block to the HTML

**Before writing the comment, read the relevant JavaScript in the file** to verify the coordinate system. Specifically, find the `clientToNormalized()` function — it should show how x/y are computed from click position. From the current implementation: `x = (cx - rect.left) / rect.width`, which confirms coordinates are normalized fractions (0.0–1.0). Verify this is still true before writing the comment.

Open `PROJECT_GUIDES/ar_poi_calibrator.html` and add a comment at the very top (before `<!DOCTYPE html>`):

```html
<!--
  AR POI CALIBRATOR — Standalone browser tool
  ============================================
  Purpose:
    Click on the panorama image to record x/y coordinates for each POI.
    Exports a pois.json snippet you can paste into assets/data/pois.json.

  How to use:
    1. Open this file directly in any modern browser (no server needed).
    2. The panorama image loads from the path set in the JS config section.
       Adjust the image path if needed.
    3. Select a POI from the left panel, then click its position on the image.
    4. Repeat for all POIs.
    5. Click Export — copies JSON to clipboard or downloads pois_calibrated.json.
    6. Paste/merge into assets/data/pois.json in the Flutter project.

  This is NOT part of the Flutter app.
  It is a development utility — run it independently in the browser.
  The Flutter app reads the output (pois.json), not this file.

  Coordinate system:
    x and y are expressed as fractions of the image dimensions (0.0 – 1.0).
    x=0 is left edge, x=1 is right edge.
    y=0 is top edge, y=1 is bottom edge.
-->
```

- [ ] Add the comment block to `PROJECT_GUIDES/ar_poi_calibrator.html`

### 0.7c — Delete the Dart calibrator page

**File to delete:** `lib/ar_core/utils/poi_calibrator_page.dart`

Before deleting, check if anything else in the project imports it (the nav_config import was already removed in §0.5a):

```
grep -r "poi_calibrator_page" lib/
```

- [ ] Confirm no remaining imports of `poi_calibrator_page.dart`
- [ ] Delete `lib/ar_core/utils/poi_calibrator_page.dart`

### 0.7d — Decide what to do with `calibrator_poi_model.dart`

**File:** `lib/ar_core/utils/calibrator_poi_model.dart`

**Important context: this file is NOT a simple model file.** It contains two separate concerns:

1. `CalibratorPoi` class — a mutable data model used only by `POICalibratorPage` (the file being deleted in §0.7c). It has a `fromJson` / `toJson` that mirrors the `pois.json` schema. This part is calibrator-only.

2. `FitToViewportResult`, `computeFitToViewport()`, `computeMarkerScreenPosition()` — **pure viewport math functions**. These compute how to scale and position an image inside an `InteractiveViewer` canvas, and where POI markers appear in screen coordinates. These are domain-agnostic and may be useful for the real panorama view.

The file is also:
- Exported from `lib/ar_core/ar_core.dart` (the barrel file)
- Has its own unit test: `lib/ar_core/test/calibrator_poi_unit_test.dart` (933 lines)

**Do NOT blindly delete this file.** Decision process:

- [ ] Read `lib/ar_core/ar_core.dart` — confirm the barrel exports `calibrator_poi_model.dart`
- [ ] Search ALL of `lib/` (not just `lib/ar_core/`) for any import of `calibrator_poi_model` or `CalibratorPoi` or `FitToViewportResult` or `computeFitToViewport`
- [ ] Check if `lib/domains/panorama/` or any other domain uses the viewport math functions

**If the viewport math functions are unused outside the calibrator:**
- Extract `FitToViewportResult`, `computeFitToViewport()`, and `computeMarkerScreenPosition()` into a new file: `lib/ar_core/utils/viewport_math.dart`
- Update the barrel export in `ar_core.dart` to export `viewport_math.dart` instead
- Delete `calibrator_poi_model.dart`
- Update `calibrator_poi_unit_test.dart`: remove `CalibratorPoi` tests (no longer needed); keep viewport math tests if they have value, or delete the whole file
- Update the barrel to stop exporting the deleted file

**If the viewport math functions ARE used elsewhere:**
- Extract them to `viewport_math.dart` as above, then delete `calibrator_poi_model.dart`
- Update all importers to use the new path

**Either way:** `CalibratorPoi` goes with the deleted calibrator page. The viewport math stays if it has real value; otherwise it can also be deleted if the real AR view does not use it.

- [ ] `flutter analyze` → 0 issues after any deletes/moves

### 0.7e — Delete empty `lib/components/ui/` folder

`lib/components/ui/` exists but is completely empty. An empty folder in a structured project is confusing — it implies content that doesn't exist and makes future developers wonder what belongs there.

- [ ] Confirm `lib/components/ui/` is empty (no files, no subdirectories)
- [ ] Delete the empty folder
- [ ] If any future component clearly belongs in `components/ui/` (e.g., a shared `Button`, `Tag`, `Badge` widget), it will be added when that component is first needed — not as empty scaffolding

---

## 0.8 — Accessibility baseline

These are the minimum accessibility requirements that must be in place before Phase 1 adds any new widgets.

### 0.8a — Verify `SizeTokens.tapTarget` exists

**File:** `lib/design/tokens/size_tokens.dart`

Confirmed present: `static const double tapTarget = 48.0;`

- [ ] Read the file to confirm — no change needed if already there

### 0.8b — Verify contrast ratios meet WCAG AA

**File:** `lib/design/theme/app_theme.dart`

Read the light and dark theme definitions. For each key color pair, the contrast ratio must be:
- Regular text (< 18pt / 14pt bold): ≥ 4.5:1
- Large text (≥ 18pt / bold ≥ 14pt): ≥ 3:1

Check these pairs:
| Foreground         | Background                | Target ratio       |
| ------------------ | ------------------------- | ------------------ |
| `onPrimary`        | `primary`                 | ≥ 4.5:1            |
| `primary`          | `surface`                 | ≥ 3:1 (large text) |
| `onSurface`        | `surface`                 | ≥ 4.5:1            |
| `error`            | `surface`                 | ≥ 3:1              |
| `onError`          | `error`                   | ≥ 4.5:1            |
| `onSurfaceVariant` | `surfaceContainerHighest` | ≥ 4.5:1            |

Use a contrast checker (e.g., https://webaim.org/resources/contrastchecker/) with the hex values from `app_theme.dart`.

- [ ] Check each pair for light theme
- [ ] Check each pair for dark theme
- [ ] Document results as a comment in `app_theme.dart`:
  ```dart
  // Contrast audit (Phase 0):
  // onPrimary/primary: 7.2:1 ✓ | primary/surface: 4.8:1 ✓ | etc.
  ```
- [ ] If any pair **fails**: **do NOT change color values to arbitrary hex codes that happen to pass**. Document the failure clearly in the comment with a `FAIL` marker, and leave a note for the developer to resolve with design intent. Color values are part of the visual identity and cannot be changed without designer approval:
  ```dart
  // onSurface/surface: 3.8:1 FAIL — below WCAG AA 4.5:1 — needs design review
  ```

### 0.8c — Add Semantics to nav items that lack it

From the codebase audit:
- `hamburger.dart` — has `Semantics` ✓
- `nav_tabs_row.dart` — has `Semantics` ✓
- `nav_accordion.dart` — has `Semantics` ✓
- `language_switcher.dart` — no `Semantics` — fixed in §0.3e ✓

Check `icons_group.dart` (theme + language switchers group):

**File:** `lib/layout/widgets/icons_group.dart`

- [ ] Read `icons_group.dart` — confirm all interactive elements have `Tooltip` and `Semantics`
- [ ] If any `IconButton` is missing a `Tooltip`: add one with a translated label using `t(pt:, en:, es:)`
- [ ] If any interactive element is missing `Semantics`: add `Semantics(button: true, label: ...)`
- [ ] After each Semantics/Tooltip addition: run the widget tests for that specific file immediately (`flutter test lib/layout/widgets/ --reporter=compact` or the relevant path). Do not batch all accessibility changes and test at the very end — a broken Semantics wrapper can silently affect layout and cause test failures that are hard to trace.

### 0.8d — Verify tap targets on nav interactive elements

Check that `hamburger.dart`, `nav_tabs_row.dart`, and header icon buttons all meet ≥ 48×48px. `IconButton` in Flutter defaults to 48px hit area via `minTapSize` — verify this is not overridden anywhere.

- [ ] Read `header.dart` — verify UNDO/REDO `IconButton` widgets have `Tooltip` (they do: "UNDO - Go back" and "REDO - Go forward" — check the format is clean, no emojis in tooltip text since terminal encoding issues may affect debug logging)
- [ ] Fix tooltip text if it contains emojis: `'UNDO - Go back (⬅️)'` → `'Undo — go back'`

### 0.8e — Verify focus rings are not suppressed

**Guide rule (`LANGUAGE_SEO_ACCESSIBILITY/SEO_AND_ACCESSIBILITY.md`):** "Never suppress focus rings. Style them using `context.primary` with opacity."

Suppressed focus rings make keyboard navigation invisible — a WCAG 2.4.7 (Level AA) violation.

```
grep -r "focusColor.*transparent\|splashColor.*transparent\|overlayColor.*transparent\|focusColor: Colors.transparent\|splashColor: Colors.transparent" lib/ --include="*.dart"
```

Also check for `FocusNode` usage that ignores focus visuals:
```
grep -r "FocusNode\|focusedBorder.*none\|enabledBorder.*none" lib/ --include="*.dart"
```

- [ ] Run both greps
- [ ] For any widget suppressing `focusColor`: replace `Colors.transparent` with `context.primary.withValues(alpha: 0.12)` (or remove the override to let the theme handle it)
- [ ] For any `splashColor: Colors.transparent` / `highlightColor: Colors.transparent` combo that is hiding focus feedback: remove or replace — splash/highlight suppression on `InkWell` can mask keyboard focus indicator in some Flutter versions
- [ ] `flutter analyze` → 0 issues

> **Exception:** `Colors.transparent` is acceptable on decorative/non-interactive widgets that should genuinely have no ink effect (e.g., a static `Container` wrapped in `InkWell` just for cursor shape). Use judgment — if the widget is interactive and focusable, the focus ring must be visible.

### 0.8f — Verify keyboard navigation on custom interactive widgets

**Guide rule (`LANGUAGE_SEO_ACCESSIBILITY/SEO_AND_ACCESSIBILITY.md`):** Custom interactive widgets must use `Focus` + `onKeyEvent` to respond to Enter/Space keypresses. Standard Flutter buttons (`ElevatedButton`, `TextButton`, `IconButton`, `PopupMenuButton`) handle keyboard natively — no change needed for those.

```
grep -r "GestureDetector\|InkWell\|MouseRegion" lib/ --include="*.dart" -l
```

- [ ] List every file that uses `GestureDetector`, `InkWell`, or `MouseRegion` as an interactive element
- [ ] For each: check if it is a custom tappable (not wrapping a standard button) — if so, it must either be wrapped in `Focus` with `onKeyEvent` or use `InkWell.onTap` with a `FocusNode` that handles keyboard activation
- [ ] Standard check: press Tab to reach the element → press Enter/Space → verify it activates. If keyboard does nothing: it needs `onKeyEvent`.
- [ ] **PopupMenuButton**, **IconButton**, **ListTile**, **ElevatedButton**, **TextButton** in the existing codebase already pass this check — skip them.

> **Note:** Phase 0 is validation only. If keyboard nav issues are found in legacy widgets, document them in a comment (`// TODO Phase 1: add keyboard nav — Focus+onKeyEvent needed`) rather than fixing now, since fixing custom interactive widgets requires thorough widget testing and should be done alongside new feature work.

### 0.8g — `reducedMotion` — out of scope for Phase 0

**Guide rule (`LANGUAGE_SEO_ACCESSIBILITY/SEO_AND_ACCESSIBILITY.md`):** "Always respect the system setting: `MediaQuery.of(context).disableAnimations`."

Phase 0 adds no new animations, so this rule does not apply to Phase 0 work. No action needed now.

> **Phase 1+ rule (record it here so it isn't forgotten):** Every new animation introduced in Phase 1 or later must check `reducedMotion` before playing:
> ```dart
> final reducedMotion = MediaQuery.of(context).disableAnimations;
> final duration = reducedMotion ? Duration.zero : AnimationTokens.medium;
> ```
> This applies to `AnimatedSwitcher`, `AnimationController`, entrance animations in `LayoutManager`, and any custom `Tween`. The `AnimationTokens.*` values already exist — just skip them when `disableAnimations` is true.

---

## 0.9 — Phase 0 verification

Run all checks in order. Do not proceed to Phase 1 until every item is green.

### Tests

```
flutter test lib/ --reporter=compact
```
- [ ] All tests pass (or: any failing tests were already failing at the start of Phase 0 — document them)
- [ ] No new test failures introduced by Phase 0 changes

### Analysis

```
flutter analyze
```
Wait ~10 seconds after running before reading output.

- [ ] 0 errors, 0 warnings

### Build runner

```
dart run build_runner build --delete-conflicting-outputs
```
- [ ] 0 errors
- [ ] `translatable_string.freezed.dart` and `translatable_string.g.dart` are up to date

### Manual app checks

- [ ] App runs on physical Android device (`flutter run -d <device_id>`)
- [ ] App runs on Chrome (`flutter run -d chrome`)
- [ ] Language switcher shows PT / EN / ES options
- [ ] Switching language persists after app restart
- [ ] Home page shows no green test banner
- [ ] Demo routes are NOT visible in the nav (debug builds: they ARE visible — this is correct)
- [ ] Calibrator route is NOT in the nav
- [ ] AR POI markers show correct design-token colors (not raw amber/purple/blue/cyan)

### File checklist

- [ ] `lib/layout/widgets/breadcrumbs.dart` exists (correctly spelled)
- [ ] `lib/layout/widgets/fab_wrapper.dart` exists (correctly spelled)
- [ ] `lib/ar_core/utils/poi_calibrator_page.dart` does NOT exist
- [ ] `PROJECT_GUIDES/ar_poi_calibrator.html` exists with comment block
- [ ] `lib/ar_core/utils/ar_poi_calibrator.html` does NOT exist
- [ ] `photo_view` does NOT appear in `pubspec.yaml` or `lib/`

---

## Summary of all files changed

| File                                                                             | Change                                                                                                        |
| -------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------- |
| `lib/utils/i18n/models/language.dart`                                            | Uncomment/add `spanish` value                                                                                 |
| `lib/utils/i18n/models/translatable_string.dart`                                 | Add `String? es` field, update `translate()` and `t()`                                                        |
| `lib/utils/i18n/models/translatable_string.freezed.dart`                         | Regenerated (build_runner)                                                                                    |
| `lib/utils/i18n/models/translatable_string.g.dart`                               | Regenerated (build_runner)                                                                                    |
| `lib/utils/i18n/widgets/language_switcher.dart`                                  | Add Tooltip/Semantics, fix `Colors.green` → `context.success`, fix `const TextStyle(fontSize:)` → `context.*` |
| `lib/utils/i18n/test/unit/language_test.dart`                                    | **CREATE** — unit tests for `Language.fromCode('es')` + `TranslatableString` Spanish fallback                 |
| `lib/utils/i18n/test/widgets/language_switcher_test.dart`                        | **CREATE** — widget test: switcher shows PT/EN/ES; Semantics label present                                    |
| `PROJECT_GUIDES/LANGUAGE_SEO_ACCESSIBILITY/I18N_GUIDE.md`                        | Add `es:` to `t()` examples; document optional Spanish param                                                  |
| `lib/domains/panorama/ar/widgets/ar_poi_marker.dart`                             | Replace 5 raw category `Colors.*` with design tokens; add `design_system.dart` import                         |
| `lib/domains/panorama/test/widgets/ar_poi_marker_test.dart`                      | Update color assertions to match new tokens                                                                   |
| `lib/domains/home/pages/home_page.dart`                                          | Delete test marker Container block                                                                            |
| `lib/navigation/navConfig/nav_config.dart`                                       | Remove calibrator import+route; gate demo routes in `kDebugMode`                                              |
| `lib/layout/widgets/breadcrums.dart`                                             | Rename to `breadcrumbs.dart`                                                                                  |
| `lib/layout/widgets/fab_wraper.dart`                                             | Rename to `fab_wrapper.dart`                                                                                  |
| `lib/navigation/test/simple_sroll/` (folder)                                     | Rename to `simple_scroll/`; update all references                                                             |
| `lib/design/theme/app_theme.dart`                                                | Add contrast audit comment                                                                                    |
| `lib/layout/widgets/icons_group.dart`                                            | Add Tooltip/Semantics if missing                                                                              |
| `lib/layout/widgets/header.dart`                                                 | Clean tooltip text if it contains emojis                                                                      |
| `lib/ar_core/utils/poi_calibrator_page.dart`                                     | **DELETE**                                                                                                    |
| `lib/ar_core/utils/calibrator_poi_model.dart`                                    | **SPLIT**: extract viewport math → `viewport_math.dart`; delete rest                                          |
| `lib/ar_core/utils/viewport_math.dart`                                           | **CREATE** (extracted from `calibrator_poi_model.dart`)                                                       |
| `lib/ar_core/test/calibrator_poi_unit_test.dart`                                 | Update or delete (keep viewport math tests; remove CalibratorPoi tests)                                       |
| `lib/ar_core/ar_core.dart`                                                       | Update barrel: remove `calibrator_poi_model`, add `viewport_math`                                             |
| `lib/ar_core/utils/ar_poi_calibrator.html`                                       | **DELETE** (moved to PROJECT_GUIDES/)                                                                         |
| `PROJECT_GUIDES/ar_poi_calibrator.html`                                          | **CREATE** (moved from lib/) with usage comment block                                                         |
| `lib/domains/demo_navigation/` (folder)                                          | **DELETED** ✅ (stale duplicate — canonical copy lives in `lib/navigation/test/demo_pages/`)                   |
| `lib/components/ui/` (empty folder)                                              | **DELETE**                                                                                                    |
| All files importing renamed files                                                | Update import paths                                                                                           |
| Any file in `lib/` (outside `lib/design/`) using `BrandColors.*`                 | Replace with `context.*` tokens (§0.4d grep finds these)                                                      |
| Any Phase 0 file with hardcoded `EdgeInsets`/`BorderRadius`/`TextStyle` literals | Replace with `Spacing.*`/`RadiusTokens.*`/`context.*` (§0.4e)                                                 |

---

## Rules for this phase

- No new features. No new routes. No new providers. Cleanup only.
- Every edit: `flutter analyze` after — fix all issues before moving to next task.
- If a test breaks because of a legitimate code change (e.g., `ar_poi_marker_test.dart` color assertions): update the test to match the new correct behavior.
- If a test breaks for an unexpected reason: investigate before continuing — do not skip it.
- Freezed files are auto-generated — never edit them by hand.
- Do not change color values in `app_theme.dart` to fix contrast failures — flag them for designer review instead.

---

*End of PHASE_0_PLAN.md*

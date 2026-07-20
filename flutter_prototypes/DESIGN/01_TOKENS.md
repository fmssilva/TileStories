# Design Tokens Reference
> All raw values. Import: `design_system.dart`

---

## Spacing — `Spacing.*`
**File:** `tokens/spacing_tokens.dart`

```
zero = 0    xs = 4    sm = 8    md = 12   lg = 16
xl = 20     xl2 = 24  xl3 = 32  xl4 = 40  xl5 = 48
xl6 = 64    xl7 = 80
```

**Semantic aliases** (use these when meaning matters):
```dart
Spacing.padding = 16        // default element padding
Spacing.gap = 12            // default gap between elements
Spacing.section = 32        // between major sections
Spacing.pageHorizontal = 16 // page-level horizontal padding (unchanged)
Spacing.pageVertical = 24   // page-level vertical padding
Spacing.headerPaddingH = 24 // header / hero horizontal padding ← new
```

**Component shortcuts:**
```dart
Spacing.buttonHorizontal = 24   Spacing.buttonVertical = 16
Spacing.cardPadding = 16        Spacing.formFieldGap = 16
Spacing.listItemPadding = 12    Spacing.dialogPadding = 24
```

**Pre-built EdgeInsets** (save boilerplate):
```dart
Spacing.insetsXs    // EdgeInsets.all(4)
Spacing.insetsSm    // EdgeInsets.all(8)
Spacing.insetsMd    // EdgeInsets.all(12)
Spacing.insetsLg    // EdgeInsets.all(16)
Spacing.insetsCard  // symmetric card padding
Spacing.insetsButton // symmetric button padding
Spacing.insetsListItem
```

---

## Border Radius — `RadiusTokens.*`
**File:** `tokens/radius_tokens.dart`

```
none=0   xs=4   sm=8   md=12   lg=16   xl=20   xl2=24   full=9999
```

**Ready-to-use `BorderRadius` getters** (already `const`, zero allocation):
```dart
RadiusTokens.radiusNone   RadiusTokens.radiusSm    RadiusTokens.radiusMd
RadiusTokens.radiusLg     RadiusTokens.radiusXl    RadiusTokens.radiusFull
```

**Component aliases** (use these — they encode intent):
```dart
RadiusTokens.cardRadius       // lg = 16  ← updated from md=12
RadiusTokens.heroCardRadius   // xl = 20  ← new
RadiusTokens.gridCellRadius   // sm = 8   ← new (icon cells in cards)
RadiusTokens.buttonRadius     // xl = 20
RadiusTokens.inputRadius      // sm = 8
RadiusTokens.dialogRadius     // xl = 20
RadiusTokens.chipRadius       // full
RadiusTokens.pillButtonRadius // full (= chipRadius, explicit alias)
RadiusTokens.bottomSheetRadius // top corners only
RadiusTokens.drawerRadius     // right corners only
RadiusTokens.roundedRadius    // xl2 = 24, for hero cards
```

> ⚠️ `cardRadius` was `md=12` and is now `lg=16`. Run `grep -r "RadiusTokens.cardRadius"` before updating to audit all affected components.

---

## Elevation — `ElevationTokens.*`
**File:** `tokens/elevation_tokens.dart`

```
level0=0  level1=1  level2=3  level3=6  level4=8  level5=12
```

**Component aliases:**
```dart
ElevationTokens.card = 1        // resting card
ElevationTokens.cardHovered = 3 // on hover
ElevationTokens.appBar = 0      // flat (M3)
ElevationTokens.dialog = 6
ElevationTokens.fab = 6
ElevationTokens.snackBar = 6
ElevationTokens.button = 0      // ElevatedButton gains shadow on hover
ElevationTokens.drawer = 8
ElevationTokens.modal = 8
ElevationTokens.tooltip = 8
```

**Interactive state helper:**
```dart
// In StatefulWidget tracking hover/press:
final elevation = ElevationTokens.getInteractiveElevation(
  base: ElevationTokens.card,
  isHovered: _isHovered,
  isPressed: _isPressed,
);
```

---

## Animation — `AnimationTokens.*`
**File:** `tokens/animation_tokens.dart`

```dart
// Durations — existing
AnimationTokens.fast      // 100ms — button press, icon swap
AnimationTokens.medium    // 200ms — fade, scale hover, tooltip
AnimationTokens.slow      // 300ms — page element entrance
AnimationTokens.verySlow  // 500ms — hero enter, bottom sheet

// Durations — luxury additions
AnimationTokens.reveal    // 550ms — RevealAnimation (primary luxury entrance)
AnimationTokens.page      // 600ms — LuxuryPageTransition forward
AnimationTokens.float     // 4000ms — SlowFloat ambient drift
AnimationTokens.bar       // 1000ms — GoldProgressBar fill

// Stagger delay slots (use instead of magic numbers)
AnimationTokens.r1        // 100ms  AnimationTokens.r2  // 200ms
AnimationTokens.r3        // 300ms  AnimationTokens.r4  // 400ms
AnimationTokens.r5        // 500ms  (cap — group further items at r5)

// Curves — existing
AnimationTokens.easeOut    // enter
AnimationTokens.easeIn     // exit
AnimationTokens.easeInOut  // move
AnimationTokens.spring     // celebrate (Curves.elasticOut)

// Curves — luxury additions
AnimationTokens.luxurySpring  // Cubic(0.16, 1.0, 0.3, 1.0) — iOS spring feel
```

---

## Breakpoints — `Breakpoints.*`
**File:** `tokens/breakpoints.dart`

```
mobile=600   tablet=840   desktop=1200   wide=1600   extraWide=1920
smallPhone=360   largePhone=428   foldable=768
```

**Prefer context extensions** (see `05_THEME.md`):
```dart
context.isMobile    // < 600
context.isTablet    // 600–1199
context.isDesktop   // ≥ 1200
context.isWide      // ≥ 1600
context.layoutType  // LayoutType enum
```

**Grid helper:**
```dart
Breakpoints.getColumnCount(screenWidth, min: 1, max: 4)
Breakpoints.getCardColumns(screenWidth) // 1/2/3
```

---

## Layout Widths — `LayoutTokens.*`
**File:** `tokens/layout_tokens.dart`

```dart
LayoutTokens.maxContentWidth = 1200   // standard page
LayoutTokens.readingWidth    = 720    // articles
LayoutTokens.formWidth       = 600    // forms
LayoutTokens.narrowFormWidth = 400    // OTP, short inputs
LayoutTokens.dashboardWidth  = 1400   // tables/analytics
LayoutTokens.ultraWideWidth  = 1600   // marketing
```

Use via `ContentType` enum with `ResponsiveContainer` (see `04_LAYOUT.md`).

---

## Sizes — `SizeTokens.*`
**File:** `tokens/size_tokens.dart`

```dart
// Icons
SizeTokens.iconXs=16  SizeTokens.iconSm=20  SizeTokens.iconMd=24
SizeTokens.iconLg=32  SizeTokens.iconXl=48  SizeTokens.iconXl2=64

// Tap targets (accessibility)
SizeTokens.tapTarget = 48    // minimum accessible tap area
SizeTokens.tapTargetSm = 40
SizeTokens.tapTargetLg = 56

// Navigation
SizeTokens.appBarHeight = 64
SizeTokens.bottomNavHeight = 80
SizeTokens.navRailWidth = 80
SizeTokens.sidebarWidth = 280
```

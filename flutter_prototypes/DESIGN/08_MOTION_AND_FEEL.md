# Motion & Feel
> The difference between a correct app and a great one.
> Tokens: `AnimationTokens.*` from `design_system.dart`

---

## Core Principle

Motion is not decoration — it's communication. Every animation must answer: *what changed, where did it come from, where is it going?* If an animation doesn't answer one of those questions, remove it.

Restraint is as important as expressiveness. One well-choreographed entrance beats ten scattered micro-interactions.

---

## Token Reference

```dart
// Durations — existing (do not remove)
AnimationTokens.fast      // 100ms — press feedback, icon swap
AnimationTokens.medium    // 200ms — hover states, tooltips, badges
AnimationTokens.slow      // 300ms — element entrance, card expand
AnimationTokens.verySlow  // 500ms — page hero, bottom sheet, reveal

// Durations — luxury additions
AnimationTokens.reveal    // 550ms — RevealAnimation entrance (primary luxury entrance)
AnimationTokens.page      // 600ms — LuxuryPageTransition forward
AnimationTokens.float     // 4000ms — SlowFloat ambient drift
AnimationTokens.bar       // 1000ms — GoldProgressBar fill

// Stagger delay constants (use instead of magic numbers)
AnimationTokens.r1        // 100ms — first  staggered child
AnimationTokens.r2        // 200ms — second staggered child
AnimationTokens.r3        // 300ms — third
AnimationTokens.r4        // 400ms — fourth
AnimationTokens.r5        // 500ms — fifth  (cap; beyond r5 group remaining items)

// Curves — existing (do not remove)
AnimationTokens.easeOut   // entrances — fast start, gentle landing
AnimationTokens.easeIn    // exits — gentle start, fast end
AnimationTokens.easeInOut // repositioning — smooth both ends
AnimationTokens.spring    // celebration moments, AR recognition pulse (elasticOut)

// Curves — luxury additions
AnimationTokens.luxurySpring  // Cubic(0.16, 1.0, 0.3, 1.0) — iOS spring feel
                               // Equivalent to CSS cubic-bezier(0.16, 1, 0.3, 1)
                               // Use for: LuxuryPageTransition, RevealAnimation, card hover lift
```

---

## Entrance Animations — Every Widget That Appears

Nothing should snap into existence. Every new element enters with motion.

**Standard entrance — `RevealAnimation` wrapper (preferred):**
```dart
// Use RevealAnimation for all luxury-context entrances
RevealAnimation(
  delay: AnimationTokens.r1,   // stagger by slot
  child: MyWidget(),
)
// RevealAnimation: opacity 0→1 + translateY(+18px→0) over AnimationTokens.reveal (550ms)
// Curve: AnimationTokens.luxurySpring
// Uses SingleTickerProviderStateMixin with auto-forward on init
```

**Manual entrance — fade + translate up (where RevealAnimation is overkill):**
```dart
_opacity = Tween(begin: 0.0, end: 1.0).animate(
  CurvedAnimation(parent: _ctrl, curve: AnimationTokens.easeOut),
);
_slide = Tween(begin: const Offset(0, 0.06), end: Offset.zero).animate(
  CurvedAnimation(parent: _ctrl, curve: AnimationTokens.easeOut),
);
// duration: AnimationTokens.slow (300ms)
```

**Staggered list entrance — use r1–r5 constants, then group:**
```dart
// ✅ Named delay slots
RevealAnimation(delay: AnimationTokens.r1, child: HeroCard()),
RevealAnimation(delay: AnimationTokens.r2, child: FilterRow()),
RevealAnimation(delay: AnimationTokens.r3, child: SectionHeader()),
RevealAnimation(delay: AnimationTokens.r4, child: ListItem()),
RevealAnimation(delay: AnimationTokens.r5, child: ListItem()),
// Items beyond r5 all use r5 — no value in staggering beyond 500ms
```

Rules:
- Page body content: `reveal` (550ms), stagger with r1–r5 constants
- Modal/sheet content: `medium` (200ms), no stagger needed
- Inline feedback (snackbar, banner): `medium`, slide from edge
- Never stagger more than 5 distinct levels — group remaining items at r5

---

## Interaction Feedback — Every Tap Has a Physical Response

**Press scale (all tappable cards and buttons):**
```dart
AnimatedScale(
  scale: _pressed ? 0.97 : 1.0,
  duration: AnimationTokens.fast,
  curve: AnimationTokens.easeOut,
  child: ...,
)
```

**Hover elevation lift (desktop/web — luxury cards):**
```dart
// Transform: translateY(-4px), shadow grows, gold border brightens
AnimatedContainer(
  duration: AnimationTokens.medium,
  curve: AnimationTokens.luxurySpring,
  transform: Matrix4.translationValues(0, _hovered ? -4 : 0, 0),
  decoration: BoxDecoration(
    borderRadius: RadiusTokens.cardRadius,
    border: Border.all(
      color: context.goldBorder.withValues(
        alpha: _hovered ? 0.20 : 0.10,
      ),
    ),
    boxShadow: _hovered ? [
      BoxShadow(
        color: context.gold.withValues(alpha: 0.10),
        blurRadius: 24,
        offset: const Offset(0, 8),
      ),
      BoxShadow(
        color: Colors.black.withValues(alpha: 0.08),
        blurRadius: 8,
        offset: const Offset(0, 2),
      ),
    ] : ElevationTokens.shadowLevel1,
  ),
  child: ...,
)
```

**Focus rings (accessibility + feel):**
- Never suppress focus rings — style them with `context.gold.withValues(alpha: 0.4)` at 2px offset

---

## Page & Route Transitions — `LuxuryPageTransition`

> ⚠️ **BackdropFilter blur is BANNED from page transitions.** It causes a ×4 frame-time regression on iOS Impeller. Blur is only permitted in static overlays (modals, PillButton, bottom sheets).

**Forward transition (entering a new page):**
- Incoming page: `opacity 0→1` + `translateX(+30px→0)`, `luxurySpring`, `page` duration (600ms)
- Outgoing page: `scale(1.0→0.94)` + `translateX(0→-30%)` + `opacity 1→0.6`, `easeIn`, 400ms

```dart
// In GoRouter route definition — use LuxuryPageTransition widget from design_system.dart
CustomTransitionPage(
  child: const MyPage(),
  transitionsBuilder: (context, animation, secondaryAnimation, child) =>
      LuxuryPageTransition(
        animation: animation,
        secondaryAnimation: secondaryAnimation,
        child: child,
      ),
  transitionDuration: AnimationTokens.page,    // 600ms
  reverseTransitionDuration: AnimationTokens.slow, // 300ms
)
```

**Back/pop transition (reverse):**
- Outgoing page (going back): `opacity 1→0` + `translateX(0→+30px)`, `easeIn`, 300ms
- Returning page: `scale(0.94→1.0)` + `opacity 0.6→1` + `translateX(-15%→0)`, `luxurySpring`, 300ms

**Modal / bottom sheet:**
- Slide up from bottom + backdrop fades in simultaneously (`verySlow` = 500ms)
- No blur on backdrop — use `Colors.black.withValues(alpha: 0.54)` solid tint instead
- `BackdropFilter` blur IS permitted here because the sheet is static once open

**Dialog:**
- Scale from 0.92 + fade (`medium` = 200ms), `luxurySpring`

---

## State Transitions — Loading → Content → Empty → Error

**Skeleton loader — matches content shape exactly:**
```dart
// Shimmer: opacity pulses 0.3 → 0.7 → 0.3 over 1200ms, ease-in-out
// Color: context.onSurface.withValues(alpha: 0.08) to 0.16
```

- Every list has a skeleton version matching expected row count
- Never use `CircularProgressIndicator` for content areas — only for action confirmations

**Content arrival — crossfade:**
```dart
AnimatedSwitcher(
  duration: AnimationTokens.medium,
  switchInCurve: AnimationTokens.easeOut,
  switchOutCurve: AnimationTokens.easeIn,
  child: isLoading
      ? const SkeletonList(key: ValueKey('skeleton'))
      : RealContentList(key: ValueKey('content'), items: items),
)
```

**Empty state — animate in, never just appear:**
- Icon: fade + scale 0.8→1.0 over `slow`
- Text: fade in at `r2` delay after icon
- Action button: fade in at `r3`

---

## Visual Depth & Atmosphere

TileStories surfaces feel layered — like looking through a window into history.

**The z-axis stack:**
```
Background (DotTexture + GoldAura RadialGradient — no blur)
Page surface (context.surface — warm tint in dark mode)
Cards (LuxuryCard: gold border at 10%, card elevation 1dp)
Overlays (PillButton: BackdropFilter OK — static)
Modals (highest elevation, strong backdrop)
```

**GoldAura — background atmosphere:**
```dart
// Pure DecoratedBox with RadialGradient — NO BackdropFilter
DecoratedBox(
  decoration: BoxDecoration(
    gradient: RadialGradient(
      center: const Alignment(0.6, -0.7),
      radius: 0.9,
      colors: [
        context.goldGlow,   // Gold at 6–8% opacity
        Colors.transparent,
      ],
    ),
  ),
)
// Positioned behind DotTexture, behind page content
// Cost: near-zero (one shader pass, no compositing)
```

**DotTexture — subtle pattern layer:**
```dart
// CustomPainter drawing a grid of dots
// dot radius: 0.75px, spacing: 20px grid
// fill color: context.dotTextureTint (Gold at 2.5–3.5%)
// canvas.drawCircle(offset, 0.75, paint) in nested for-loop over canvas size
// Positioned above GoldAura, below page content
```

**LuxuryCard depth:**
```dart
BoxDecoration(
  color: context.cardSurface,         // white.4% (dark) / white (light)
  borderRadius: RadiusTokens.cardRadius, // 16px
  border: Border.all(
    color: context.goldBorder,        // Gold at 10–12%
    width: 1,
  ),
  boxShadow: [
    BoxShadow(
      color: Colors.black.withValues(alpha: 0.06),
      blurRadius: 2,
      offset: const Offset(0, 1),
    ),
    BoxShadow(
      color: Colors.black.withValues(alpha: 0.04),
      blurRadius: 8,
      offset: const Offset(0, 4),
    ),
  ],
)
```

---

## TileStories Signature Moments

**1. AR Tile Recognition**
When the camera recognizes a tile:
- A gold ring pulses outward (`spring` curve = elasticOut, 600ms, scale 1.0→1.6, opacity 1→0)
- 150ms later: discovery card slides up from bottom (`luxurySpring`, 550ms)
- Background dims: `Colors.black.withValues(alpha: 0→0.4)`, `medium` (200ms)
- No blur on the dim overlay — solid tint only (performance constraint)

**2. Discovery Card Expand → Detail View**
- `Hero` animation on tile image — travels from card to full-width header
- Title fades in at 100ms delay after hero settles
- Historical details stagger in with r1–r5 delays, `reveal` curve

**3. Saved Discoveries List**
- First load: items stagger in from bottom, r1–r5 delays
- Swipe-to-save: slides in from right + brief gold border flash (opacity 0.8→0, 400ms)
- Delete: `AnimatedList` slide-out left, gap closes with `easeOut`

**4. GoldShimmerText — Rarity / Magic Word**
When a `GoldShimmerText` word is on screen:
- `AnimationController` drives `Alignment(-2, 0)` → `Alignment(2, 0)`
- `LinearGradient.createShader` with Gold → white-gold → Gold stops
- Loop duration: `AnimationTokens.bar` (1000ms) with 2000ms rest between loops
- **Not** a continuous shimmer — fires once on entrance, then loops every few seconds

---

## Checklist — Before Shipping Any Screen

- [ ] Every element that appears has a `RevealAnimation` entrance (or manual fade+translate)
- [ ] Stagger delays use `r1`–`r5` tokens — no magic numbers like `Duration(milliseconds: 237)`
- [ ] Every tappable surface has press feedback (`AnimatedScale` to 0.97)
- [ ] Cards have hover lift on desktop/web (`luxurySpring`, translateY -4px)
- [ ] Loading state uses skeleton loader — not `CircularProgressIndicator`
- [ ] State transitions use `AnimatedSwitcher`
- [ ] Page transitions use `LuxuryPageTransition` — no instant cuts, no blur
- [ ] At least one signature "wow moment" is intentionally crafted per screen
- [ ] Background has `DotTexture` + `GoldAura` — not a flat color
- [ ] Dark mode cards have gold border via `context.goldBorder`
- [ ] Empty and error states are staggered and have recovery actions
- [ ] `BackdropFilter` is only in static overlays — never in animated transitions
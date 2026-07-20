# Colors
> File: `tokens/primitives/brand_colors.dart`, `tokens/semantic/semantic_colors.dart`, `theme/theme_extensions.dart`

---

## Brand Palette

Five palettes, each with a 50–900 tonal scale:

| Palette          | Core             | Hex       | M3 Role      | Used for                                     |
| ---------------- | ---------------- | --------- | ------------ | -------------------------------------------- |
| **Gold**         | `gold500`        | `#C9A84C` | **Primary**  | CTAs, active nav, shimmer, all brand moments |
| **Azulejo Blue** | `azulejoBlue500` | `#0EA5E9` | Secondary    | Water / sea accents, secondary interactive   |
| **Deep Blue**    | `deepBlue500`    | `#3B82F6` | Tertiary     | Supporting UI, links on parchment            |
| **Stone**        | `stone500`       | `#78716C` | Neutral      | Dark mode text, muted labels, card borders   |
| **Ivory / Ink**  | `ivory50`        | `#FAF8F4` | Surface/text | Light mode backgrounds; ink950 for dark text |

> ⚠️ **Hierarchy change**: Gold is now `primary` — promoted from tertiary. Deep Blue is now `tertiary` — demoted. Azulejo Blue remains `secondary`. All `context.primary` calls now resolve to Gold.

```dart
// Only access raw palette when building custom components.
// In all other cases, use theme/context shortcuts below.
GoldColors.gold500        // primary brand colour
AzulejoBlueColors.azulejoBlue500  // secondary
DeepBlueColors.deepBlue500        // tertiary (demoted)
StoneColors.stone500      // neutral dark text
IvoryColors.ivory50       // near-white surface
```

---

## Always Use Theme Colors (Not Raw Palette)

```dart
// ✅ Theme-aware (works in light + dark mode)
context.primary           // Gold — all primary brand moments
context.onPrimary         // Dark ink on Gold
context.secondary         // Azulejo Blue
context.tertiary          // Deep Blue
context.surface           // card/page surface
context.onSurface         // default text
context.onSurfaceVariant  // muted/secondary text
context.outline           // borders
context.error             // M3 error color

// ✅ Luxury-specific shortcuts (from theme_extensions.dart)
context.gold              // = context.primary (Gold)
context.goldDim           // Gold at 12–15% opacity — glow background tint
context.goldGlow          // Gold at 6–8% opacity — very subtle field wash
context.goldBorder        // Gold at 10–12% opacity — glass card borders
context.parchment         // F0E6CC (dark) / ink 1C1409 (light) — editorial text
context.muted             // parchment at 45% — secondary labels
context.microLabelColor   // Gold at 50% — EXPLORE · DISCOVER caps labels
context.dotTextureTint    // Gold at 2.5–3.5% — CustomPainter dot grid fill
context.cardSurface       // white.4% (dark) / white (light) — card backgrounds

// ❌ Raw — breaks in dark mode
BrandColors.deepBlue500
Color(0xFF3B82F6)
Colors.blue
```

---

## Semantic Colors

For feedback states (success, warning, info, error).

**Access via `context.*` shortcuts:**
```dart
context.success           // green — positive confirmation
context.successContainer  // green background for banners
context.onSuccess         // text on successContainer

context.warning           // orange — caution
context.warningContainer
context.onWarning         // (access via context.semanticColors.onWarning)

context.info              // blue — informational
context.infoContainer
context.onInfo            // (access via context.semanticColors.onInfo)

context.error             // from M3 ColorScheme
```

**Or full extension object:**
```dart
final s = context.semanticColors;
Container(color: s.successContainer, child: Text('Done', style: TextStyle(color: s.onSuccess)))
```

**Severity helper:**
```dart
// Renders the right color automatically for any severity
final s = context.semanticColors;
final color = s.colorForSeverity(MessageSeverity.warning);
final bg    = s.containerForSeverity(MessageSeverity.error);
```

`MessageSeverity` enum: `.success`, `.warning`, `.error`, `.info`

---

## Color Rules

| Situation              | Use                                                      |
| ---------------------- | -------------------------------------------------------- |
| Primary action button  | `context.primary` (Gold)                                 |
| Text on primary button | `context.onPrimary` (dark ink)                           |
| Brand moment / shimmer | `context.gold` / `context.goldDim`                       |
| Muted/hint text        | `context.muted` or `context.onSurfaceVariant`            |
| Micro-label caps       | `context.microLabelColor` + `context.dmMicroLabel` style |
| Glass card border      | `context.goldBorder`                                     |
| Card background        | `context.cardSurface` (automatic with `LuxuryCard`)      |
| Dividers / borders     | `context.outline`                                        |
| Disabled state         | `context.onSurface.withValues(alpha: 0.38)`              |
| Success state          | `context.success` / `context.successContainer`           |
| Warning state          | `context.warning` / `context.warningContainer`           |
| Error state            | `context.error` / `context.errorContainer`               |
| Info state             | `context.info` / `context.infoContainer`                 |

---

## Dark Mode

Dark mode is **automatic** — the theme switches all `ColorScheme` values.

- Never hardcode light-only or dark-only colors
- Always use `context.*` / `ColorScheme` — they adapt
- To branch on mode: `context.isDarkMode` (not `isDark` — that getter doesn't exist)

```dart
// Only branch if you truly need different assets/icons:
if (context.isDarkMode) { ... }
```

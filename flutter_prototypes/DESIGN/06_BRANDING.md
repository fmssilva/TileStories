# Branding Components
> Files: `design/branding/app_logo.dart`, `design/branding/app_name.dart`, `design/branding/app_brand.dart`  
> Import: `design/design_system.dart` (or `design/branding/branding.dart`)

---

## Components

| Widget     | Renders                            | Use when                    |
| ---------- | ---------------------------------- | --------------------------- |
| `AppLogo`  | Logo image only                    | Favicon, icon-only contexts |
| `AppName`  | "Tile**Stories**" styled text only | Text-only headings          |
| `AppBrand` | Logo + Name combined               | Header, splash, about page  |

---

## `AppBrand` — Logo + Name

```dart
// Default: horizontal, 48px logo
const AppBrand()

// Custom size
AppBrand(logoSize: 64, spacing: 16)

// Vertical stack
AppBrand(direction: Axis.vertical, logoSize: 80)

// Tappable (navigates home)
AppBrand(onTap: () => context.go('/'))

// Custom name style
AppBrand(nameStyle: context.headlineLarge)
```

**Params:**
```
logoSize      double   48.0    logo width/height
spacing       double   12.0    gap between logo and name
direction     Axis     horizontal
nameStyle     TextStyle? null  defaults to headlineMedium
onTap         VoidCallback?    wraps in Semantics(button:true) automatically
mainAxisAlignment              MainAxisAlignment.start
crossAxisAlignment             CrossAxisAlignment.center
```

---

## `AppLogo` — Image Only

```dart
// Default 48px
const AppLogo()

// Custom size with tap
AppLogo(size: 32, onTap: () => context.go('/'))

// No semantic label (when used inside AppBrand — already labelled)
AppLogo(size: 48, semanticLabel: null)
```

**Notes:**
- Uses `assets/branding/Logo.png` via `AppAssets.logo`
- `filterQuality: FilterQuality.high` — always crisp
- `cacheWidth`/`cacheHeight` — DPR-aware GPU memory hint (auto)
- When `onTap` provided: wraps in `Semantics(button: true, label: 'TileStories — go to home')`

---

## `AppName` — Text Only

```dart
// Default style (headlineMedium)
const AppName()

// Custom style
AppName(style: context.titleLarge)

// With tap
AppName(onTap: () => context.go('/'))
```

**Notes:**
- "Tile" renders in `context.primary` (Deep Blue)
- "Stories" renders in `context.tertiary` (Gold)
- Both colors adapt automatically in dark mode

---

## ❌ Never

```dart
// Don't hardcode brand colors
Text('TileStories', style: TextStyle(color: Colors.blue))

// Don't reference assets directly
Image.asset('assets/branding/Logo.png')  // use AppLogo

// Don't use raw BrandColors in branding widgets
Text('Tile', style: TextStyle(color: BrandColors.deepBlue500))
// ↑ breaks dark mode — use context.primary instead
```

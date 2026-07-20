# Theme System
> Files: `design/theme/app_theme.dart`, `design/theme/theme_extensions.dart`, `design/theme/theme_provider.dart`, `design/theme/widgets/theme_switcher.dart`

---

## Setup (already done in `main.dart`)

```dart
MaterialApp.router(
  theme: AppTheme.light(),
  darkTheme: AppTheme.dark(),
  themeMode: ref.watch(themeModeProvider),
)
```

You don't need to touch this. Focus on using the extensions below.

---

## `BuildContext` Extensions

Single import gives you everything: `import 'design/design_system.dart'`

### Colors
```dart
context.primary           context.onPrimary        // Gold / dark ink on Gold
context.secondary         context.onSecondary       // Azulejo Blue
context.tertiary          context.onTertiary        // Deep Blue (demoted)
context.surface           context.onSurface
context.onSurfaceVariant  // muted text
context.surfaceContainerHighest
context.outline           // border color
context.outlineVariant    // divider color
context.error             context.onError
context.inverseSurface    // snackbar background
context.onInverseSurface  // snackbar text
context.success           context.successContainer    context.onSuccess
context.warning           context.warningContainer    context.onWarning
context.info              context.infoContainer       context.onInfo

// ── Luxury additions (from theme_extensions.dart) ──
context.gold              // = context.primary (Gold)
context.goldDim           // Gold at 12–15% opacity — glow tint
context.goldGlow          // Gold at 6–8% opacity — field wash
context.goldBorder        // Gold at 10–12% opacity — glass card borders
context.parchment         // F0E6CC (dark) / ink 1C1409 (light) — editorial text
context.muted             // parchment at 45% — secondary labels
context.microLabelColor   // Gold at 50% — EXPLORE · DISCOVER caps
context.dotTextureTint    // Gold at 2.5–3.5% — dot grid fill color
context.cardSurface       // white.4% (dark) / white (light) — card fill
```

### Typography
```dart
context.displayLarge   context.headlineLarge  context.titleLarge
context.displayMedium  context.headlineMedium context.titleMedium
context.displaySmall   context.headlineSmall  context.titleSmall
                       context.bodyLarge      context.labelLarge
                       context.bodyMedium     context.labelMedium
                       context.bodySmall      context.labelSmall

// ── Luxury shortcuts ──
context.frauncesDisplay  // = context.displaySmall (Fraunces upright)
context.frauncesItalic   // = context.displaySmall italic (the "magic word")
context.dmMicroLabel     // labelSmall + letterSpacing 3.5 + w500 (ALL CAPS labels)
```

### Responsive
```dart
context.isMobile      // < 600
context.isTablet      // 600–1199
context.isDesktop     // ≥ 1200
context.isWide        // ≥ 1600
context.isExtraWide   // ≥ 1920
context.layoutType    // LayoutType enum
context.screenWidth
context.screenHeight
```

### Responsive Spacing
```dart
context.responsivePadding  // 16/24/32/48 by breakpoint
context.responsiveGap      // 12/16/20 by breakpoint
context.responsiveSection  // 32/40/48 by breakpoint
```

### Other
```dart
context.isDarkMode    context.isLightMode   // use isDarkMode — isDark does NOT exist
context.borderColor   // outline with opacity
context.dividerColor  // outlineVariant
context.theme         // ThemeData
context.colors        // ColorScheme
context.semanticColors // SemanticColors extension
```

### UI Helpers
```dart
// Snackbar
context.showSnackBar('Saved!');
context.showSnackBar('Error', action: SnackBarAction(...));

// Dialog
context.showThemedDialog(child: MyDialog());

// Bottom sheet (uses RadiusTokens.bottomSheetRadius automatically)
context.showThemedBottomSheet(child: MySheet());
```

---

## Theme Mode (Riverpod)

```dart
// Read current mode
final mode = ref.watch(themeModeProvider); // ThemeMode

// Toggle light ↔ dark
ref.read(themeModeProvider.notifier).toggle();

// Full 3-way cycle: light → dark → system → light
ref.read(themeModeProvider.notifier).cycle();

// Set explicitly
ref.read(themeModeProvider.notifier).setThemeMode(ThemeMode.dark);
```

**Widgets:**
```dart
// Simple toggle button (light ↔ dark only)
ThemeSwitcher()

// Full popup menu (light / dark / system)
ThemeSwitcherMenu()
```

---

## Direct Theme Access (when context extensions aren't enough)

```dart
final theme = Theme.of(context);
final colorScheme = theme.colorScheme;
final textTheme = theme.textTheme;
final semantic = theme.extension<SemanticColors>()!;
```

# SEO & Accessibility Guide

---

## Accessibility — Flutter Widgets

### Semantic Labels
Every interactive or meaningful widget needs a semantic label.

```dart
// Images
Semantics(
  image: true,
  label: 'Panoramic view of Torre de Belém',
  child: Image.asset(...),
)

// Custom tappable areas (non-button widgets)
Semantics(
  button: true,
  label: ref.tr(t(pt: 'Abrir detalhes', en: 'Open details')),
  child: GestureDetector(onTap: ..., child: ...),
)

// Expandable items
Semantics(
  button: true,
  label: item.label,
  expanded: isExpanded,
  child: ...,
)

// Loading states — announce to screen readers
Semantics(
  liveRegion: true,
  label: isLoading
      ? ref.tr(t(pt: 'A carregar...', en: 'Loading...'))
      : ref.tr(t(pt: 'Conteúdo carregado', en: 'Content loaded')),
  child: isLoading ? SkeletonLoader() : ContentWidget(),
)
```

### Keyboard Navigation
Any custom interactive widget that isn't a standard Flutter button needs keyboard support.

```dart
Focus(
  onKeyEvent: (node, event) {
    if (event is! KeyDownEvent) return KeyEventResult.ignored;
    if (event.logicalKey == LogicalKeyboardKey.enter ||
        event.logicalKey == LogicalKeyboardKey.space) {
      _activate();
      return KeyEventResult.handled;
    }
    if (event.logicalKey == LogicalKeyboardKey.escape) {
      _close();
      return KeyEventResult.handled;
    }
    return KeyEventResult.ignored;
  },
  child: ...,
)
```

### Tap Targets
- Minimum 48×48px for all interactive elements — `SizeTokens.tapTarget`
- `IconButton` enforces this via theme — verify any custom tap areas

### Focus Rings
Never suppress focus rings. Style them using `context.primary` with opacity.

```dart
// Visible focus indicator
Container(
  decoration: isFocused
      ? BoxDecoration(
          border: Border.all(
            color: context.primary.withValues(alpha: 0.5),
            width: 2,
          ),
          borderRadius: RadiusTokens.radiusMd,
        )
      : null,
  child: ...,
)
```

### Reduced Motion
Always respect the system setting.

```dart
final reduceMotion = MediaQuery.of(context).disableAnimations;

AnimatedRotation(
  turns: isExpanded ? 0.25 : 0.0,
  duration: reduceMotion ? Duration.zero : AnimationTokens.medium,
  child: Icon(Icons.chevron_right),
)
```

### Color Contrast
- Use `context.*` color tokens — they are tested for WCAG AA compliance
- Never use raw `BrandColors.*` in UI — they don't adapt to dark mode and contrast isn't guaranteed
- WCAG AA minimums: 4.5:1 for normal text, 3:1 for large text and UI components

---

## SEO — Flutter Web

### `web/index.html` — Required Meta Tags

```html
<head>
  <title>TileStories - Explorando o Grande Panorama de Lisboa</title>
  <meta name="description" content="Descubra a história de Lisboa através de azulejos e realidade aumentada.">
  
  <!-- Open Graph -->
  <meta property="og:title" content="TileStories">
  <meta property="og:description" content="AR panorama stories from Lisbon's azulejo tiles.">
  <meta property="og:image" content="https://tilestories.com/og-image.jpg">
  <meta property="og:url" content="https://tilestories.com">
  <meta property="og:type" content="website">

  <!-- Language -->
  <link rel="alternate" hreflang="pt" href="https://tilestories.com/pt">
  <link rel="alternate" hreflang="en" href="https://tilestories.com/en">
  <link rel="alternate" hreflang="x-default" href="https://tilestories.com">
  <link rel="canonical" href="https://tilestories.com/">
</head>

<!-- Visible content for crawlers (Flutter is client-side rendered) -->
<body>
  <h1>TileStories — Explorando o Grande Panorama de Lisboa</h1>
  <p>Descubra a história de Lisboa através de realidade aumentada e azulejos históricos.</p>
  <noscript>This app requires JavaScript.</noscript>
</body>
```

### Deep Linking (GoRouter)
```dart
GoRoute(
  path: '/discovery/:id',
  builder: (context, state) {
    final id = state.pathParameters['id']!;
    return DiscoveryDetailPage(id: id);
  },
)
```

### Flutter Web Build
```bash
flutter build web --split-debug-info --tree-shake-icons
```

---

## Checklist — Before Shipping Any Screen

- [ ] All interactive elements have `semanticLabel` or `Semantics(label:)`
- [ ] Custom tappable areas use `Semantics(button: true)`
- [ ] Tap targets ≥ 48px (`SizeTokens.tapTarget`)
- [ ] Focus rings visible and styled with `context.primary`
- [ ] Keyboard navigable (custom interactive widgets have `Focus` + `onKeyEvent`)
- [ ] Motion respects `MediaQuery.disableAnimations`
- [ ] No raw `BrandColors.*` in UI — only `context.*` tokens
- [ ] All user-facing strings translated with `ref.tr(t(...))`
- [ ] Expandable/collapsible elements use `Semantics(expanded:)`
- [ ] Loading states use `Semantics(liveRegion: true)`
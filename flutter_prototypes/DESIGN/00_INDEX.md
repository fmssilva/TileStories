# TileStories Design System — AI Agent Guide
> **Start here.** Pick the file for your task.

Single import: `import 'package:grande_panorama_ar/design/design_system.dart';`

---

## The One Rule

**Never hardcode a value.** There is a token for everything.

```dart
// ❌ Never
Padding(padding: EdgeInsets.all(16))
Container(color: Colors.blue)
Text('Title', style: TextStyle(fontSize: 24))
BorderRadius.circular(12)

// ✅ Always
Padding(padding: EdgeInsets.all(Spacing.lg))
Container(color: context.primary)
Text('Title', style: context.headlineMedium)
RadiusTokens.cardRadius
```

---

## Design Philosophy

TileStories is an AR discovery app rooted in Lisbon's azulejo tile culture. The design should feel like **candlelight on old stone** — warm, layered, alive with history. Not a generic dark app. Not a flat card grid. Something with texture, depth, and a sense that you're touching something real.

Every screen must have:
- A single clear focal point — the user's eye always knows where to go
- At least one moment of motion that rewards attention
- Considered use of depth — background, surface, card, overlay each feel distinct

Ask before shipping any screen: *would someone screenshot this?* That's the bar.

**Avoid at all costs:** flat hero + CTA layouts, symmetric boxy card grids, spinners instead of skeletons, instant state changes with no transition, hover states that only change color.

---

## Guide Files

| File                                           | Read when you need to…                                 |
| ---------------------------------------------- | ------------------------------------------------------ |
| [01_TOKENS.md](01_TOKENS.md)                   | Use spacing, radius, elevation, animation, breakpoints |
| [02_COLORS.md](02_COLORS.md)                   | Use colors, dark mode, semantic feedback colors        |
| [03_TYPOGRAPHY.md](03_TYPOGRAPHY.md)           | Use text styles, fonts, typography shortcuts           |
| [04_LAYOUT.md](04_LAYOUT.md)                   | Build page layouts, responsive containers              |
| [05_THEME.md](05_THEME.md)                     | Configure themes, access theme extensions              |
| [06_BRANDING.md](06_BRANDING.md)               | Use logo, app name, brand composite                    |
| [07_PATTERNS.md](07_PATTERNS.md)               | Quick patterns: card, form, list, states — with motion |
| [08_MOTION_AND_FEEL.md](08_MOTION_AND_FEEL.md) | Motion rules, entrance animations, signature moments   |

---

## Design System Location

```
lib/design/
├── design_system.dart         ← single barrel export
├── tokens/
│   ├── primitives/brand_colors.dart
│   ├── semantic/semantic_colors.dart
│   ├── spacing_tokens.dart
│   ├── radius_tokens.dart
│   ├── elevation_tokens.dart
│   ├── animation_tokens.dart
│   ├── breakpoints.dart
│   ├── layout_tokens.dart
│   ├── size_tokens.dart
│   ├── typography_tokens.dart
│   └── z_index_tokens.dart
├── theme/
│   ├── app_theme.dart
│   ├── theme_extensions.dart
│   ├── color_scheme_builder.dart
│   ├── theme_builder.dart
│   ├── theme_provider.dart
│   └── widgets/theme_switcher.dart
├── layout/
│   ├── responsive_container.dart
│   ├── responsive_padding.dart
│   ├── max_width_box.dart
│   └── centered_content.dart
└── branding/
    ├── app_logo.dart
    ├── app_name.dart
    └── app_brand.dart
```
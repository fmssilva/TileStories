# Typography
> File: `tokens/typography_tokens.dart`, `theme/theme_extensions.dart`

---

## Fonts

| Font         | Used for                                                     | Source                   |
| ------------ | ------------------------------------------------------------ | ------------------------ |
| **Fraunces** | Display + large headline styles; italic for the "magic word" | `GoogleFonts.fraunces()` |
| **DM Sans**  | All UI text (headline, title, body, label, micro-labels)     | `GoogleFonts.dmSans()`   |

Applied automatically in `buildTextTheme()` — you don't configure this manually. Both fonts are served by the `google_fonts` package already in `pubspec.yaml`.

> **Fraunces** is a variable optical-size font. It has a natural editorial warmth at large sizes and a crisp italic that makes the "magic word" feel alive. Use it **only** for display and large headline styles — never body or UI labels.
> **DM Sans** replaces Inter everywhere. It's more geometric and slightly wider, giving labels and body text better legibility at small sizes.

---

## Text Style Shortcuts

Access from any `BuildContext`. Never use `TextStyle(fontSize: ...)` directly.

```dart
// Display — Fraunces, editorial hero text
context.displayLarge   // 48px  ← hero titles, AR recognition moment
context.displayMedium  // 38px
context.displaySmall   // 30px  ← section hero / card hero text

// Headline — DM Sans, section headers
context.headlineLarge  // 28px
context.headlineMedium // 24px  ← most common page title
context.headlineSmall  // 20px

// Title — DM Sans, card/list headings
context.titleLarge     // 18px
context.titleMedium    // 15px  ← card titles
context.titleSmall     // 13px

// Body — DM Sans, content text
context.bodyLarge      // 16px
context.bodyMedium     // 14px  ← default body text
context.bodySmall      // 12px

// Label — DM Sans, buttons, chips, captions
context.labelLarge     // 14px  ← button label
context.labelMedium    // 12px
context.labelSmall     // 11px

// Luxury shortcuts from theme_extensions.dart
context.frauncesDisplay  // = context.displaySmall (Fraunces, upright)
context.frauncesItalic   // = context.displaySmall italic (the "magic word")
context.dmMicroLabel     // = context.labelSmall + letterSpacing 3.5 + w500 (EXPLORE · DISCOVER caps)
```

---

## The "Magic Word" Pattern

Every large editorial heading can have **one** word italicised in Fraunces to create emotional emphasis. This is the highest-power typographic tool — use it once per screen maximum.

```dart
// ✅ Correct — one italic Fraunces word in a RichText heading
RichText(
  text: TextSpan(
    style: context.displaySmall.copyWith(color: context.onSurface),
    children: [
      const TextSpan(text: 'The '),
      TextSpan(text: 'Golden', style: context.frauncesItalic.copyWith(color: context.gold)),
      const TextSpan(text: ' Age'),
    ],
  ),
)

// ❌ Wrong — italic used on whole heading
Text('The Golden Age', style: context.displaySmall.copyWith(fontStyle: FontStyle.italic))

// ❌ Wrong — italic used on body text
Text('This artefact dates to...', style: context.bodyMedium.copyWith(fontStyle: FontStyle.italic))
```

---

## Micro-Labels

CAPS track-spaced labels used for category tags and section-type markers (e.g. "EXPLORE · DISCOVER"):

```dart
Text(
  'HERITAGE SITE',
  style: context.dmMicroLabel.copyWith(color: context.microLabelColor),
)
```

Do not use `labelSmall` directly with manual letterSpacing — always go through `context.dmMicroLabel`.

---

## Common Usage

```dart
// Page title
Text('Explore Tiles', style: context.headlineMedium)

// Card heading
Text(tile.name, style: context.titleMedium)

// Body content
Text(tile.description, style: context.bodyMedium)

// Muted/secondary text
Text(tile.date, style: context.bodySmall.copyWith(color: context.muted))

// Caption / label
Text('NEW', style: context.labelSmall)

// Button label — automatic via FilledButton
```

---

## Muted / Disabled Text Helpers

```dart
// Muted: applies onSurfaceVariant color
Text('hint', style: context.textMuted(context.bodyMedium))

// Disabled: applies 38% opacity
Text('inactive', style: context.textDisabled(context.bodyMedium))
```

---

## Text Color Rules

| Text role             | Use                                           |
| --------------------- | --------------------------------------------- |
| Primary text          | inherit from style (defaults to `onSurface`)  |
| Secondary/muted       | `context.muted` or `context.onSurfaceVariant` |
| Parchment / editorial | `context.parchment`                           |
| Micro-label caps      | `context.microLabelColor`                     |
| On colored background | match the `on*` token: `context.onPrimary`    |
| Disabled              | `context.onSurface.withValues(alpha: 0.38)`   |
| Success text          | `context.onSuccess`                           |
| Error text            | `context.error` or `context.onErrorContainer` |

---

## ❌ Never

```dart
TextStyle(fontSize: 24)                  // hardcoded size
TextStyle(fontWeight: FontWeight.bold)   // use style from context
TextStyle(fontFamily: 'DM Sans')         // font is applied by theme
TextStyle(fontFamily: 'Fraunces')        // use context.frauncesDisplay/frauncesItalic
Colors.black87                           // use context.onSurface
.withOpacity(0.5)                        // use .withValues(alpha: 0.5) — withOpacity is deprecated
```

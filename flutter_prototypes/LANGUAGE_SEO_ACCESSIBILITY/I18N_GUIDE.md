# i18n Guide
> System: `lib/utils/i18n/` — PT first, always.

---

## Core Rules

1. **Portuguese first** — `t(pt: '...', en: '...')` — always in this order
2. **Spanish optional** — add `es: '...'` when providing a Spanish translation; omit to auto-fall back to English
3. **Inline translations** — no external files (ARB, JSON)
4. **ConsumerWidget required** — any widget using `ref.tr()` must extend `ConsumerWidget` or `ConsumerStatefulWidget`

---

## Usage

```dart
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:grande_panorama_ar/utils/i18n/models/translatable_string.dart';
import 'package:grande_panorama_ar/utils/i18n/extensions/context_extensions.dart';

class MyWidget extends ConsumerWidget {
  @override
  Widget build(BuildContext context, WidgetRef ref) {
    return Text(ref.tr(t(pt: 'Bem-vindo', en: 'Welcome')));
    // With Spanish:
    return Text(ref.tr(t(pt: 'Bem-vindo', en: 'Welcome', es: 'Bienvenido')));
    // Omitting es: falls back to English automatically
  }
}
```

---

## In Models

```dart
@freezed
class POI with _$POI {
  const factory POI({
    required String id,
    required TranslatableString name,
    required TranslatableString description,
  }) = _POI;
}

// JSON:
// { "name": { "pt": "Torre de Belém", "en": "Belém Tower" } }
// { "name": { "pt": "Torre de Belém", "en": "Belém Tower", "es": "Torre de Belém" } }

// Usage:
Text(ref.tr(poi.name))
```

---

## Domain String Constants

Create `lib/domains/<domain>/l10n/<domain>_strings.dart` only when a string is reused in 2+ places.

```dart
class PanoramaStrings {
  static final title = t(pt: 'Grande Panorama de Lisboa', en: 'Great Panorama of Lisbon');
  static final searchPlaceholder = t(pt: 'Pesquisar...', en: 'Search...');
}

// Usage:
Text(ref.tr(PanoramaStrings.title))
```

For single-use strings, always inline — no constants.

---

## Language Switching

```dart
// Read
final lang = ref.watch(languageProvider); // Language enum

// Set
ref.read(languageProvider.notifier).setLanguage(Language.english);

// Toggle
final current = ref.read(languageProvider);
ref.read(languageProvider.notifier).setLanguage(
  current == Language.portuguese ? Language.english : Language.portuguese,
);
```

Add `LanguageSwitcher()` to AppBar actions for the UI dropdown.

---

## ❌ Never

```dart
// StatelessWidget with translations
class MyWidget extends StatelessWidget {
  Widget build(BuildContext context) {
    return Text(ref.tr(...)); // ERROR — no ref
  }
}

// English first
t(en: 'Hello', pt: 'Olá') // wrong order

// Hardcoded strings
Text('Welcome') // must use ref.tr(t(...))

// Unnecessary constants for single-use strings
final greeting = t(pt: 'Olá', en: 'Hello');
return Text(ref.tr(greeting)); // just inline it
```

---

## Checklist

- [ ] PT text first in every `t()`
- [ ] Widget is `ConsumerWidget` or `ConsumerStatefulWidget`
- [ ] All user-facing strings use `ref.tr(t(...))`
- [ ] Spanish (`es:`) added when translation is available; omit to fall back to English
- [ ] Domain string constants only for strings used 2+ times
- [ ] Tested in PT, EN, and ES (if `es:` provided)
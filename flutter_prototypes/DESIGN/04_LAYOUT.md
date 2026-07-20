# Layout System
> Files: `lib/layout/`, `lib/design/layout/`

---

## Page Layout — `LayoutManager` + `LayoutSlots`

Every page returns a `LayoutManager`. Use a preset from `LayoutPresets`.

```dart
import 'package:grande_panorama_ar/layout/layout_manager.dart';
import 'package:grande_panorama_ar/layout/layout_presets.dart';

class MyPage extends ConsumerWidget {
  @override
  Widget build(BuildContext context, WidgetRef ref) {
    return LayoutManager(
      slots: LayoutPresets.defaultPageBrowser(
        context: context,
        body: _buildBody(context),
        fab: const MyFAB(), // optional
      ),
    );
  }
}
```

---

## Layout Presets

| Preset                                            | Use for                      | Nav                                   |
| ------------------------------------------------- | ---------------------------- | ------------------------------------- |
| `LayoutPresets.defaultPageBrowser(context, body)` | Standard content pages       | Header (web) or Footer (mobile), auto |
| `LayoutPresets.defaultPageApp(body)`              | Immersive / media viewer     | None — full screen                    |
| `LayoutPresets.minimal(context, body)`            | Login, signup, focused flows | Header only                           |
| `LayoutPresets.fullscreen(body)`                  | Splash, onboarding           | None — hides system UI                |

---

## Custom `LayoutSlots`

When a preset doesn't fit, build `LayoutSlots` directly:

```dart
return LayoutManager(
  slots: LayoutSlots(
    header: const Header(),       // optional AppBar
    body: MyContent(),            // required
    footer: const FooterApp(),    // optional BottomNav
    fab: const MyFAB(),           // optional FAB
    scrollable: true,             // default true
    safeArea: true,               // default true
    isLoading: _loading,          // loading overlay
    showBackToTop: true,          // for long pages
    backgroundColor: context.surface,
  ),
);
```

---

## Content Width Constraints — `ResponsiveContainer`

Inside the body, wrap content that needs width constraints:

```dart
import 'package:grande_panorama_ar/design/design_system.dart';

// Standard content page
ResponsiveContainer(
  child: MyContent(),
)

// Reading / article
ResponsiveContainer(
  contentType: ContentType.reading,   // 720px
  child: ArticleText(),
)

// Form
ResponsiveContainer(
  contentType: ContentType.form,      // 600px
  child: MyForm(),
)

// Custom width
ResponsiveContainer(
  maxWidth: 800,
  child: MyWidget(),
)
```

**`ContentType` options:**
```
standard   = 1200px   ← default
reading    =  720px   ← articles, docs
form       =  600px   ← login, signup
narrowForm =  400px   ← OTP, short flows
dashboard  = 1400px   ← tables, analytics
ultraWide  = 1600px   ← marketing, landing
```

---

## Atomic Layout Components

Build custom layouts by composing these:

```dart
// Width constraint only
MaxWidthBox(maxWidth: LayoutTokens.formWidth, child: MyForm())

// Centering only
CenteredContent(child: MyContent())

// Responsive padding only (adapts 16→24→32→48 by breakpoint)
ResponsivePadding(child: MyContent())

// Compose them:
ResponsivePadding(
  child: CenteredContent(
    child: MaxWidthBox(
      maxWidth: LayoutTokens.readingWidth,
      child: ArticleText(),
    ),
  ),
)
```

---

## Responsive Breakpoints

Access from context (preferred):
```dart
context.isMobile   // < 600px
context.isTablet   // 600–1199px
context.isDesktop  // ≥ 1200px
context.isWide     // ≥ 1600px
```

Responsive layout example:
```dart
// Different column count
final cols = context.isMobile ? 1 : context.isTablet ? 2 : 3;

// Different spacing
final padding = context.isMobile ? Spacing.md : Spacing.xl2;

// Different text style
final titleStyle = context.isMobile ? context.titleMedium : context.titleLarge;
```

---

## Responsive Padding Shortcut

```dart
// Correct padding for current screen width (16/24/32/48)
Padding(padding: EdgeInsets.symmetric(horizontal: context.responsivePadding))

// Gap between sections (adapts by screen)
SizedBox(height: context.responsiveSection)
```

---

## Scroll Restoration

`LayoutManager` auto-manages scroll position via `ScrollRegistry`.
To preserve custom scroll position across navigation:

```dart
// Inside body — get the controller registered for this page
final controller = ScrollRegistryProvider.of(context).controller('my-list');
ListView(controller: controller, ...)
```

---

## ❌ Never

```dart
Scaffold(...)         // Don't build Scaffold manually — use LayoutManager
MediaQuery.of(context).size.width < 600   // use context.isMobile
EdgeInsets.all(16)    // use Spacing.lg or context.responsivePadding
```

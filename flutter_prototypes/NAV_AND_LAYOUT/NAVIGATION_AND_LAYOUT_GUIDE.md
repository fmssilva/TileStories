# Navigation Guide

> **Audience**: AI agents and developers adding new pages or debugging navigation behaviour.  
> **Scope**: `lib/navigation/`, `lib/layout/`, `lib/main.dart`.

---

## 1. How to Add a New Page (5 steps)

### Step 1 — Create the page widget

```dart
// lib/domains/my_feature/pages/my_page.dart
import 'package:flutter/material.dart';
import '../../../layout/layout_manager.dart';
import '../../../layout/layout_presets.dart';

class MyPage extends StatelessWidget {
  const MyPage({super.key});

  @override
  Widget build(BuildContext context) {
    return LayoutManager(
      slots: LayoutPresets.defaultPageBrowser(
        context: context,
        body: const Center(child: Text('Hello!')),
      ),
    );
  }
}
```

### Step 2 — Register the route in `nav_config.dart`

```dart
// lib/navigation/navConfig/nav_config.dart
import '../../domains/my_feature/pages/my_page.dart';

final List<NavItem> navigationConfig = [
  // ...existing items...
  NavItem(
    id: 'my-feature',
    path: '/my-feature',
    label: const TranslatableString(pt: 'Funcionalidade', en: 'My Feature'),
    builder: (context) => const MyPage(),
    metadata: const NavMetadata(
      showInNav: true,    // appears in header tabs + hamburger
      requiresAuth: false,
      order: 9,           // position in nav menu (ascending)
      showInBreadcrumb: true,
    ),
  ),
];
```

That's it — go_router routes are auto-generated from this list.

### Step 3 — Navigate to it from any widget

```dart
// Always set the flag BEFORE context.go() so redirect knows this is a push
ref.read(isNavigatingProvider.notifier).set(true);
context.go('/my-feature');
```

### Step 4 (optional) — Add child pages

```dart
NavItem(
  id: 'my-feature',
  path: '/my-feature',
  // ...
  children: [
    NavItem(
      id: 'my-feature-detail',
      path: '/my-feature/detail',  // ← absolute path; go_router strips prefix automatically
      label: const TranslatableString(pt: 'Detalhe', en: 'Detail'),
      builder: (context) => const MyDetailPage(),
      metadata: const NavMetadata(showInNav: true, order: 91),
    ),
  ],
),
```

### Step 5 (optional) — Layout presets

| Preset                               | Use for                                                  |
| ------------------------------------ | -------------------------------------------------------- |
| `LayoutPresets.defaultPageBrowser()` | Most content pages (header nav on web, footer on mobile) |
| `LayoutPresets.defaultPageApp()`     | Full-screen / immersive content (no header/footer)       |
| `LayoutPresets.fullscreen()`         | Splash / onboarding (hides system UI)                    |

---

## 2. Navigation Architecture

### How forward navigation works

```
User taps tab / hamburger item
  → ref.read(isNavigatingProvider.notifier).set(true)   // ← flag set
  → context.go('/path')
    → go_router fires redirect(context, state)
      → redirect reads+consumes isNavigatingProvider (resets to false)
      → isNavigating == true → schedules postFrameCallback with push('/path')
    → postFrameCallback fires (after frame)
      → NavHistoryNotifier.push('/path') adds history entry
      → LayoutManager.initState postFrameCallback runs next:
          captures _myHistoryIndex, restores scroll+pageState
```

### How browser BACK/FORWARD works

```
User presses browser BACK
  → go_router fires redirect(context, state)
    → isNavigatingProvider is false (nobody set it)
    → schedules postFrameCallback with undoRedo('/prev-path')
  → postFrameCallback fires
    → NavHistoryNotifier.undoRedo() moves currentIndex back/forward
    → LayoutManager.initState postFrameCallback restores state for that index
```

### Key invariant

> `isNavigatingProvider` is `true` **only** for the ~1 frame between  
> `set(true)` and `redirect`'s `consume()`. It is **always** `false` during  
> browser-initiated BACK/FORWARD.

---

## 3. History Stack (`NavHistoryNotifier`)

Located in `lib/navigation/histConfig/history_provider.dart`.

| Method                              | When called                    | What it does                                                                                    |
| ----------------------------------- | ------------------------------ | ----------------------------------------------------------------------------------------------- |
| `push(path)`                        | User clicked a link            | Trims forward history, appends new `HistoryEntry`, increments currentIndex                      |
| `undoRedo(path)`                    | Browser BACK/FORWARD           | Moves `currentIndex` ±1; no new entries created                                                 |
| `stageSave(idx, scroll, pageState)` | `LayoutManager.dispose()`      | Writes to `_staged` map (no Riverpod `state =`); flushed at start of next `push()`/`undoRedo()` |
| `getScrollPositions()`              | `LayoutManager.initState` pfcb | Returns scroll positions for currentIndex (merged with staged)                                  |
| `getPageState()`                    | `LayoutManager.initState` pfcb | Returns page state for currentIndex (merged with staged)                                        |
| `saveScrollPositionsAt(idx, …)`     | Tests only                     | Direct write, bypasses staging — safe to use outside dispose()                                  |
| `savePageStateAt(idx, …)`           | Tests only                     | Direct write, bypasses staging                                                                  |

### Why staged saves exist

`LayoutManager.dispose()` fires inside Flutter's `buildScope`.  
Riverpod **throws** if `state =` is called during `buildScope` (debug mode).  
Solution: write to a plain Dart `Map` (`_staged`) instead. The staged data is  
flushed at the start of the **next** `push()`/`undoRedo()` call, which always  
runs from a `postFrameCallback` (safely outside `buildScope`).

---

## 4. Scroll Restoration (`ScrollRegistry`)

Located in `lib/layout/scrollController/`.

### How to register a scrollable widget

```dart
// Inside a widget that's a descendant of LayoutManager:
final registry = ScrollRegistryProvider.of(context);
final controller = registry.controller('my-widget-id');  // any unique string

return SingleChildScrollView(
  controller: controller,
  child: ...,
);
```

`LayoutManager` itself registers the main page scroll under the id `'page'`.

### Restoration flow

1. `LayoutManager.initState` creates `ScrollRegistry(savedPositions: {})`.
2. After first frame, `_registry.restorePositions(savedPositions)` calls  
   `jumpTo()` on each controller that has clients.
3. `LayoutManager.dispose` calls `_registry.cachedPositions` (snapshot of  
   last offsets) and passes them to `stageSave()`.
4. On next navigation, staged data is flushed into the history entry.
5. When the user navigates BACK, `getScrollPositions()` returns those offsets  
   and step 2 repeats.

---

## 5. Page State Restoration (`PageStateRegistry`)

Located in `lib/layout/pageState/`.

### How to save and restore arbitrary state

```dart
// Read initial value (after restoration)
@override
void didChangeDependencies() {
  super.didChangeDependencies();
  if (_restored) return;
  final registry = PageStateRegistryProvider.of(context);
  final saved = registry.get('tab');          // returns null on first visit
  if (saved != null) {
    _tabController.animateTo(saved as int);
    _restored = true;
  }
}

// Write on change
void _onTabChanged() {
  PageStateRegistryProvider.of(context).set('tab', _tabController.index);
}
```

### Keys

Any `String` key. Common conventions used in this project:

| Key            | Type     | Example             |
| -------------- | -------- | ------------------- |
| `'tab'`        | `int`    | active TabBar index |
| `'name'`       | `String` | text field value    |
| `'country'`    | `String` | dropdown selection  |
| `'newsletter'` | `bool`   | checkbox state      |

---


## 6. Common Pitfalls

| Symptom                                                            | Cause                                                                              | Fix                                                                 |
| ------------------------------------------------------------------ | ---------------------------------------------------------------------------------- | ------------------------------------------------------------------- |
| Browser BACK treated as push (history grows instead of going back) | Forgot to call `isNavigatingProvider.set(true)` before `context.go()`              | Always set the flag immediately before every `context.go()`         |
| Scroll not restored on BACK                                        | Controller not registered in `ScrollRegistry` (used `ScrollController()` directly) | Use `ScrollRegistryProvider.of(context).controller('id')`           |
| Page state not restored on BACK                                    | State read in `initState` (before history is updated)                              | Read state in `didChangeDependencies` with a `_restored` guard      |
| `_RenderTheater` assertion on web startup                          | Widget placed above Navigator's Overlay tree                                       | Use `LayoutManager` body (inside Scaffold) for any overlaid widgets |
| Tests fail with "Timer is still pending"                           | `addPostFrameCallback` called from `dispose()`                                     | Use `stageSave()` instead — it avoids the timer                     |

// ignore_for_file: depend_on_referenced_packages

// =============================================================================
// PAGE STATE — WIDGET TESTS
// =============================================================================
//
// These tests mount a real GoRouter + real LayoutManager + real NavHistory
// with stub pages that use PageStateRegistryProvider to read/write state.
// They prove that the full lifecycle works end-to-end WITHOUT a browser:
//
//   initState → restore() fires (postFrameCallback) → page reads saved state
//   user interaction → page calls registry.set('tab', n)
//   navigate away → LayoutManager.dispose() calls savePageStateAt()
//   UNDO → new LayoutManager creates fresh registry, restore() populates it
//          → page reads restored value via get()
//
// WHAT WE TEST:
//   1. PageStateRegistryProvider.of(context) is accessible inside LayoutManager
//   2. Tab index is saved when navigating away (dispose → savePageStateAt)
//   3. Tab index is RESTORED when returning via UNDO (restore → get())
//   4. Fresh navigation (new push) does NOT restore old state
//   5. Multiple UNDO/REDO cycles all restore correctly
//   6. Form field text is saved and restored
//   7. PageState and scroll positions coexist correctly
//   8. Pages that never write state produce empty pageState in history
//
// WHY STUB PAGES:
// Real pages have heavy deps (images, i18n, panorama). Stubs use real
// LayoutManager + real PageStateRegistryProvider — the full lifecycle.
//
// TIMING:
// redirect → addPostFrameCallback → push/undoRedo (Riverpod mutation)
// LayoutManager.initState → addPostFrameCallback → restore() + index capture
// LayoutManager.dispose → synchronous savePageStateAt
// → Use pumpAndSettle() to drain ALL pending callbacks before asserting.
//
// Run with:
//   flutter test lib/navigation/test/pageState/page_state_widget_test.dart
//   flutter test lib/navigation/test/
//
// =============================================================================

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:go_router/go_router.dart';
import 'package:grande_panorama_ar/layout/layout_manager.dart';
import 'package:grande_panorama_ar/layout/layout_slots.dart';
import 'package:grande_panorama_ar/layout/pageState/page_state_registry_provider.dart';
import 'package:grande_panorama_ar/navigation/navConfig/current_route_provider.dart';
import 'package:grande_panorama_ar/navigation/histConfig/history_provider.dart';
import 'package:grande_panorama_ar/navigation/histConfig/route_observer.dart';
import 'package:grande_panorama_ar/navigation/histConfig/is_navigating_provider.dart';

// =============================================================================
// STUB PAGES
//
// Each stub is wrapped in LayoutManager so the full page-state lifecycle fires.
// =============================================================================

/// A page that tracks a tab selection via PageStateRegistry.
///
/// On mount it reads the saved tab index (defaulting to 0).
/// When the user taps a tab button it calls registry.set('tab', n).
/// Keys used: 'tab' → int
class _TabbedPage extends StatefulWidget {
  final String label;

  const _TabbedPage({required this.label});

  @override
  State<_TabbedPage> createState() => _TabbedPageState();
}

class _TabbedPageState extends State<_TabbedPage> {
  int _currentTab = 0;
  // Guard: only restore saved state once. After the user changes tabs,
  // subsequent didChangeDependencies calls (from LayoutManager rebuilds)
  // must not overwrite the user's selection.
  bool _stateRestored = false;

  @override
  void didChangeDependencies() {
    super.didChangeDependencies();
    if (_stateRestored) return;
    // Read restored tab; PageStateRegistryProvider is set up by LayoutManager.
    // On first visit get() returns null (no saved state).
    // On UNDO, LayoutManager calls restore() then setState(), which re-fires
    // didChangeDependencies here with the populated registry.
    final saved = PageStateRegistryProvider.of(context).get('tab');
    if (saved != null) {
      setState(() => _currentTab = saved as int);
      _stateRestored = true;
    }
  }

  void _selectTab(BuildContext context, int index) {
    setState(() => _currentTab = index);
    PageStateRegistryProvider.of(context).set('tab', index);
  }

  @override
  Widget build(BuildContext context) {
    return Column(
      children: [
        Text(widget.label),
        Text('current_tab:$_currentTab'),
        Row(
          children: List.generate(
            3,
            (i) => TextButton(
              key: ValueKey('tab_btn_$i'),
              onPressed: () => _selectTab(context, i),
              child: Text('Tab $i'),
            ),
          ),
        ),
      ],
    );
  }
}

/// Wraps _TabbedPage in real LayoutManager so the page-state lifecycle fires.
class _TabbedPageRoute extends StatelessWidget {
  final String label;

  const _TabbedPageRoute({required this.label});

  @override
  Widget build(BuildContext context) {
    return LayoutManager(
      slots: LayoutSlots(
        body: _TabbedPage(label: label),
        scrollable: false,
        safeArea: false,
      ),
    );
  }
}

/// A page with a text field whose text is saved via PageStateRegistry.
///
/// Keys used: 'name' → String
class _FormPage extends StatefulWidget {
  final String label;

  const _FormPage({required this.label});

  @override
  State<_FormPage> createState() => _FormPageState();
}

class _FormPageState extends State<_FormPage> {
  late TextEditingController _ctrl;
  bool _stateRestored = false;

  @override
  void initState() {
    super.initState();
    _ctrl = TextEditingController();
  }

  @override
  void didChangeDependencies() {
    super.didChangeDependencies();
    if (_stateRestored) return;
    final saved = PageStateRegistryProvider.of(context).get('name');
    if (saved != null) {
      _ctrl.text = saved as String;
      _stateRestored = true;
    }
  }

  @override
  void dispose() {
    _ctrl.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Column(
      children: [
        Text(widget.label),
        TextField(
          key: const Key('name_field'),
          controller: _ctrl,
          onChanged: (v) =>
              PageStateRegistryProvider.of(context).set('name', v),
        ),
      ],
    );
  }
}

class _FormPageRoute extends StatelessWidget {
  final String label;

  const _FormPageRoute({required this.label});

  @override
  Widget build(BuildContext context) {
    return LayoutManager(
      slots: LayoutSlots(
        body: _FormPage(label: label),
        scrollable: false,
        safeArea: false,
      ),
    );
  }
}

/// A plain page that writes NOTHING to the registry.
class _SimplePage extends StatelessWidget {
  final String label;

  const _SimplePage({required this.label});

  @override
  Widget build(BuildContext context) {
    return LayoutManager(
      slots: LayoutSlots(
        body: Center(child: Text(label)),
        scrollable: false,
        safeArea: false,
      ),
    );
  }
}

// =============================================================================
// TEST ROUTER
// =============================================================================

GoRouter _buildRouter(ProviderContainer c) {
  return GoRouter(
    initialLocation: '/',
    redirect: (context, state) {
      final path = state.uri.toString();
      final isNavigating = c.read(isNavigatingProvider.notifier).consume();

      WidgetsBinding.instance.addPostFrameCallback((_) {
        final n = c.read(navHistoryProvider.notifier);
        final idx = c.read(navHistoryProvider).currentIndex;
        if (idx < 0) {
          n.push(path);
        } else if (isNavigating) {
          n.push(path);
        } else {
          n.undoRedo(path);
        }
      });
      return null;
    },
    observers: [
      NavObserver(
        onRouteChanged: (path) =>
            c.read(currentRouteProvider.notifier).update(path),
      ),
    ],
    routes: [
      GoRoute(
        path: '/',
        builder: (context, _) => const _SimplePage(label: '__home__'),
      ),
      GoRoute(
        path: '/tabs',
        builder: (context, _) => const _TabbedPageRoute(label: '__tabs__'),
      ),
      GoRoute(
        path: '/form',
        builder: (context, _) => const _FormPageRoute(label: '__form__'),
      ),
      GoRoute(
        path: '/other',
        builder: (context, _) => const _SimplePage(label: '__other__'),
      ),
    ],
    errorBuilder: (_, state) => Scaffold(body: Text('ERROR: ${state.uri}')),
  );
}

Widget _buildApp(GoRouter router, ProviderContainer c) {
  return UncontrolledProviderScope(
    container: c,
    child: MaterialApp.router(routerConfig: router, theme: ThemeData.light()),
  );
}

ProviderContainer _makeContainer(WidgetTester tester) {
  final c = ProviderContainer();
  addTearDown(c.dispose);
  return c;
}

// =============================================================================
// TESTS
// =============================================================================

void main() {
  // ---------------------------------------------------------------------------
  // GROUP 1: Provider wiring
  // ---------------------------------------------------------------------------

  group('PageStateRegistryProvider wiring', () {
    testWidgets(
      'PageStateRegistryProvider.of(context) is accessible inside LayoutManager',
      (tester) async {
        final c = _makeContainer(tester);
        final router = _buildRouter(c);

        await tester.pumpWidget(_buildApp(router, c));
        await tester.pumpAndSettle();

        c.read(isNavigatingProvider.notifier).set(true);
        router.go('/tabs');
        await tester.pumpAndSettle();

        // The page rendered without throwing — provider is accessible
        expect(find.text('__tabs__'), findsOneWidget);
        expect(find.text('current_tab:0'), findsOneWidget);
      },
    );

    testWidgets('page reads default tab=0 on first visit (no saved state)', (
      tester,
    ) async {
      final c = _makeContainer(tester);
      final router = _buildRouter(c);

      await tester.pumpWidget(_buildApp(router, c));
      await tester.pumpAndSettle();

      c.read(isNavigatingProvider.notifier).set(true);
      router.go('/tabs');
      await tester.pumpAndSettle();

      // No saved state → default 0
      expect(find.text('current_tab:0'), findsOneWidget);
    });
  });

  // ---------------------------------------------------------------------------
  // GROUP 2: Tab state saved on navigate away
  // ---------------------------------------------------------------------------

  group('Tab state — saved when leaving page', () {
    testWidgets('tab selection is stored in history after navigating away', (
      tester,
    ) async {
      final c = _makeContainer(tester);
      final router = _buildRouter(c);

      await tester.pumpWidget(_buildApp(router, c));
      await tester.pumpAndSettle();

      // Navigate to /tabs
      c.read(isNavigatingProvider.notifier).set(true);
      router.go('/tabs');
      await tester.pumpAndSettle();

      // Tap Tab 2
      await tester.tap(find.byKey(const ValueKey('tab_btn_2')));
      await tester.pumpAndSettle();

      expect(find.text('current_tab:2'), findsOneWidget);

      // Navigate away — dispose() fires savePageStateAt
      c.read(isNavigatingProvider.notifier).set(true);
      router.go('/');
      await tester.pumpAndSettle();

      // History entry for /tabs (index 1) must have tab=2
      final history = c.read(navHistoryProvider);
      expect(history.entries.length, greaterThanOrEqualTo(2));
      final tabsEntry = history.entries.firstWhere((e) => e.path == '/tabs');
      expect(tabsEntry.pageState['tab'], 2);
    });

    testWidgets('pages that never write state produce empty pageState', (
      tester,
    ) async {
      final c = _makeContainer(tester);
      final router = _buildRouter(c);

      await tester.pumpWidget(_buildApp(router, c));
      await tester.pumpAndSettle();

      // Home page (_SimplePage) writes nothing
      c.read(isNavigatingProvider.notifier).set(true);
      router.go('/other');
      await tester.pumpAndSettle();

      final history = c.read(navHistoryProvider);
      final homeEntry = history.entries.firstWhere((e) => e.path == '/');
      expect(homeEntry.pageState, isEmpty);
    });
  });

  // ---------------------------------------------------------------------------
  // GROUP 3: Tab state restored on UNDO
  // ---------------------------------------------------------------------------

  group('Tab state — restored on UNDO', () {
    testWidgets('tab index is restored when returning via UNDO', (
      tester,
    ) async {
      final c = _makeContainer(tester);
      final router = _buildRouter(c);

      await tester.pumpWidget(_buildApp(router, c));
      await tester.pumpAndSettle();

      // Navigate to /tabs and select Tab 2
      c.read(isNavigatingProvider.notifier).set(true);
      router.go('/tabs');
      await tester.pumpAndSettle();

      await tester.tap(find.byKey(const ValueKey('tab_btn_2')));
      await tester.pumpAndSettle();

      // Navigate away
      c.read(isNavigatingProvider.notifier).set(true);
      router.go('/');
      await tester.pumpAndSettle();

      // UNDO back to /tabs
      router.go('/tabs');
      await tester.pumpAndSettle();

      // Tab 2 must be restored
      expect(
        find.text('current_tab:2'),
        findsOneWidget,
        reason: 'UNDO must restore the previously selected tab',
      );
    });

    testWidgets('fresh navigation does NOT restore old tab (starts at 0)', (
      tester,
    ) async {
      final c = _makeContainer(tester);
      final router = _buildRouter(c);

      await tester.pumpWidget(_buildApp(router, c));
      await tester.pumpAndSettle();

      // Visit /tabs, select Tab 2, leave
      c.read(isNavigatingProvider.notifier).set(true);
      router.go('/tabs');
      await tester.pumpAndSettle();
      await tester.tap(find.byKey(const ValueKey('tab_btn_2')));
      await tester.pumpAndSettle();
      c.read(isNavigatingProvider.notifier).set(true);
      router.go('/');
      await tester.pumpAndSettle();

      // Navigate via /other to /tabs — this is a NEW push, not UNDO
      c.read(isNavigatingProvider.notifier).set(true);
      router.go('/other');
      await tester.pumpAndSettle();
      c.read(isNavigatingProvider.notifier).set(true);
      router.go('/tabs');
      await tester.pumpAndSettle();

      // Fresh visit — must start at tab 0
      expect(
        find.text('current_tab:0'),
        findsOneWidget,
        reason: 'Fresh navigation must not restore the old tab',
      );
    });

    testWidgets('multiple UNDO/REDO cycles all restore correctly', (
      tester,
    ) async {
      final c = _makeContainer(tester);
      final router = _buildRouter(c);

      await tester.pumpWidget(_buildApp(router, c));
      await tester.pumpAndSettle();

      // Visit /tabs, select Tab 1
      c.read(isNavigatingProvider.notifier).set(true);
      router.go('/tabs');
      await tester.pumpAndSettle();
      await tester.tap(find.byKey(const ValueKey('tab_btn_1')));
      await tester.pumpAndSettle();

      // Visit /other
      c.read(isNavigatingProvider.notifier).set(true);
      router.go('/other');
      await tester.pumpAndSettle();

      // Cycle 1: UNDO to /tabs — must see tab 1
      router.go('/tabs');
      await tester.pumpAndSettle();
      expect(find.text('current_tab:1'), findsOneWidget);

      // REDO to /other
      router.go('/other');
      await tester.pumpAndSettle();

      // Cycle 2: UNDO to /tabs again — must STILL see tab 1
      router.go('/tabs');
      await tester.pumpAndSettle();
      expect(
        find.text('current_tab:1'),
        findsOneWidget,
        reason: 'Second UNDO must also restore tab 1',
      );
    });
  });

  // ---------------------------------------------------------------------------
  // GROUP 4: Form text restored on UNDO
  // ---------------------------------------------------------------------------

  group('Form text — saved and restored on UNDO', () {
    testWidgets('text field content is saved and restored via UNDO', (
      tester,
    ) async {
      final c = _makeContainer(tester);
      final router = _buildRouter(c);

      await tester.pumpWidget(_buildApp(router, c));
      await tester.pumpAndSettle();

      // Navigate to /form and type a name
      c.read(isNavigatingProvider.notifier).set(true);
      router.go('/form');
      await tester.pumpAndSettle();

      await tester.enterText(find.byKey(const Key('name_field')), 'Alice');
      await tester.pumpAndSettle();

      // Navigate away
      c.read(isNavigatingProvider.notifier).set(true);
      router.go('/');
      await tester.pumpAndSettle();

      // History must have the form's page state saved
      final history = c.read(navHistoryProvider);
      final formEntry = history.entries.firstWhere((e) => e.path == '/form');
      expect(formEntry.pageState['name'], 'Alice');

      // UNDO back to /form
      router.go('/form');
      await tester.pumpAndSettle();

      // The text must be restored in the field
      final field = tester.widget<TextField>(
        find.byKey(const Key('name_field')),
      );
      expect(
        field.controller!.text,
        'Alice',
        reason: 'Form field must be restored after UNDO',
      );
    });

    testWidgets('empty form on fresh navigation (not restored)', (
      tester,
    ) async {
      final c = _makeContainer(tester);
      final router = _buildRouter(c);

      await tester.pumpWidget(_buildApp(router, c));
      await tester.pumpAndSettle();

      // Visit /form, type, leave
      c.read(isNavigatingProvider.notifier).set(true);
      router.go('/form');
      await tester.pumpAndSettle();
      await tester.enterText(find.byKey(const Key('name_field')), 'Bob');
      await tester.pumpAndSettle();
      c.read(isNavigatingProvider.notifier).set(true);
      router.go('/');
      await tester.pumpAndSettle();

      // New push to /form (not UNDO)
      c.read(isNavigatingProvider.notifier).set(true);
      router.go('/other');
      await tester.pumpAndSettle();
      c.read(isNavigatingProvider.notifier).set(true);
      router.go('/form');
      await tester.pumpAndSettle();

      final field = tester.widget<TextField>(
        find.byKey(const Key('name_field')),
      );
      expect(
        field.controller!.text,
        '',
        reason: 'Fresh navigation must not restore form text',
      );
    });
  });

  // ---------------------------------------------------------------------------
  // GROUP 5: Page state and scroll coexist
  // ---------------------------------------------------------------------------

  group('Page state coexists with scroll state', () {
    testWidgets('history entry stores both scroll position and page state', (
      tester,
    ) async {
      final c = _makeContainer(tester);
      final router = _buildRouter(c);

      await tester.pumpWidget(_buildApp(router, c));
      await tester.pumpAndSettle();

      // Navigate to /tabs, select Tab 1
      c.read(isNavigatingProvider.notifier).set(true);
      router.go('/tabs');
      await tester.pumpAndSettle();
      await tester.tap(find.byKey(const ValueKey('tab_btn_1')));
      await tester.pumpAndSettle();

      // Navigate away
      c.read(isNavigatingProvider.notifier).set(true);
      router.go('/');
      await tester.pumpAndSettle();

      // Entry must have pageState saved (scroll is {} since scrollable: false)
      final history = c.read(navHistoryProvider);
      final tabsEntry = history.entries.firstWhere((e) => e.path == '/tabs');
      expect(tabsEntry.pageState['tab'], 1);
      // scrollPositions is empty because the _TabbedPageRoute has scrollable:false
      expect(tabsEntry.scrollPositions, isEmpty);
    });
  });

  // ---------------------------------------------------------------------------
  // GROUP 6: Independent state per page
  // ---------------------------------------------------------------------------

  group('Independent page state per route', () {
    testWidgets('two different tab pages store state independently', (
      tester,
    ) async {
      final c = _makeContainer(tester);

      // Router with two separate tab pages at different routes
      final router = GoRouter(
        initialLocation: '/',
        redirect: (context, state) {
          final path = state.uri.toString();
          final isNavigating = c.read(isNavigatingProvider.notifier).consume();

          WidgetsBinding.instance.addPostFrameCallback((_) {
            final n = c.read(navHistoryProvider.notifier);
            final idx = c.read(navHistoryProvider).currentIndex;
            if (idx < 0) {
              n.push(path);
            } else if (isNavigating) {
              n.push(path);
            } else {
              n.undoRedo(path);
            }
          });
          return null;
        },
        observers: [
          NavObserver(
            onRouteChanged: (path) =>
                c.read(currentRouteProvider.notifier).update(path),
          ),
        ],
        routes: [
          GoRoute(
            path: '/',
            builder: (context, _) => const _SimplePage(label: '__home__'),
          ),
          GoRoute(
            path: '/page-x',
            builder: (context, _) =>
                const _TabbedPageRoute(label: '__page-x__'),
          ),
          GoRoute(
            path: '/page-y',
            builder: (context, _) =>
                const _TabbedPageRoute(label: '__page-y__'),
          ),
        ],
        errorBuilder: (_, state) => Scaffold(body: Text('ERROR: ${state.uri}')),
      );

      addTearDown(c.dispose);
      await tester.pumpWidget(
        UncontrolledProviderScope(
          container: c,
          child: MaterialApp.router(
            routerConfig: router,
            theme: ThemeData.light(),
          ),
        ),
      );
      await tester.pumpAndSettle();

      // Visit /page-x → select Tab 2
      c.read(isNavigatingProvider.notifier).set(true);
      router.go('/page-x');
      await tester.pumpAndSettle();
      await tester.tap(find.byKey(const ValueKey('tab_btn_2')));
      await tester.pumpAndSettle();

      // Visit /page-y → select Tab 1
      c.read(isNavigatingProvider.notifier).set(true);
      router.go('/page-y');
      await tester.pumpAndSettle();
      await tester.tap(find.byKey(const ValueKey('tab_btn_1')));
      await tester.pumpAndSettle();

      // Navigate away to save both
      c.read(isNavigatingProvider.notifier).set(true);
      router.go('/');
      await tester.pumpAndSettle();

      // Verify in history: each page has its own tab state
      final history = c.read(navHistoryProvider);
      final entryX = history.entries.firstWhere((e) => e.path == '/page-x');
      final entryY = history.entries.firstWhere((e) => e.path == '/page-y');

      expect(entryX.pageState['tab'], 2);
      expect(entryY.pageState['tab'], 1);
    });
  });
}

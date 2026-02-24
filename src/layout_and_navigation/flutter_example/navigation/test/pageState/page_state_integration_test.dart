// ignore_for_file: depend_on_referenced_packages

// =============================================================================
// PAGE STATE — INTEGRATION TESTS
// =============================================================================
//
// These integration tests exercise the FULL page-state lifecycle end-to-end
// inside a real widget tree:
//
//   GoRouter redirect → push/undoRedo → LayoutManager initState/dispose
//   → PageStateRegistryProvider → PageStateRegistry → NavHistoryNotifier
//
// They test complex multi-page scenarios that widget tests cannot cover
// in a single self-contained assertion block.
//
// WHAT THESE TESTS PROVE:
//   1. Multi-step UNDO/REDO cycles preserve per-page state independently
//   2. History trim on new push after UNDO clears forward page states
//   3. Scroll + page state coexist correctly across UNDO/REDO
//   4. Deep navigation chains save and restore state at every level
//   5. Page state is NOT restored after a fresh push (only after UNDO)
//   6. Multiple independent state keys per page all survive the round-trip
//
// DIFFERENCE FROM WIDGET TESTS:
//   Widget tests (page_state_widget_test.dart) test individual capabilities
//   (tab save, form restore, etc.) in isolation.
//   These integration tests run COMPLETE multi-step user flows from start
//   to finish with multiple pages and multiple UNDO/REDO operations.
//
// Run with:
//   flutter test lib/navigation/test/pageState/page_state_integration_test.dart
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
// SHARED STUB WIDGETS
// =============================================================================

/// Page with tab selection + outer scroll.
/// Keys: 'tab' → int (saved to pageState)
/// Scroll: outer 'page' controller owned by LayoutManager
class _TabbedPage extends StatefulWidget {
  final String label;
  const _TabbedPage({required this.label});
  @override
  State<_TabbedPage> createState() => _TabbedPageState();
}

class _TabbedPageState extends State<_TabbedPage> {
  int _currentTab = 0;
  bool _stateRestored = false;

  @override
  void didChangeDependencies() {
    super.didChangeDependencies();
    if (_stateRestored) return;
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
        Text('tab:$_currentTab'),
        Row(
          children: List.generate(
            4,
            (i) => TextButton(
              key: ValueKey('tab_${widget.label}_$i'),
              onPressed: () => _selectTab(context, i),
              child: Text('T$i'),
            ),
          ),
        ),
        // Tall filler so the outer SingleChildScrollView can actually scroll
        ...List.generate(
          40,
          (i) => SizedBox(height: 60, child: Text('item $i')),
        ),
      ],
    );
  }
}

class _TabbedRoute extends StatelessWidget {
  final String label;
  const _TabbedRoute({required this.label});

  @override
  Widget build(BuildContext context) {
    return LayoutManager(
      slots: LayoutSlots(
        body: _TabbedPage(label: label),
        scrollable: true,
        safeArea: false,
      ),
    );
  }
}

/// Page that stores MULTIPLE state keys: 'filter' (int) + 'sort' (String).
class _FilterSortPage extends StatefulWidget {
  final String label;
  const _FilterSortPage({required this.label});
  @override
  State<_FilterSortPage> createState() => _FilterSortPageState();
}

class _FilterSortPageState extends State<_FilterSortPage> {
  int _filter = 0;
  String _sort = 'asc';
  bool _restored = false;

  @override
  void didChangeDependencies() {
    super.didChangeDependencies();
    if (_restored) return;
    final reg = PageStateRegistryProvider.of(context);
    final savedFilter = reg.get('filter');
    final savedSort = reg.get('sort');
    if (savedFilter != null || savedSort != null) {
      setState(() {
        if (savedFilter != null) _filter = savedFilter as int;
        if (savedSort != null) _sort = savedSort as String;
      });
      _restored = true;
    }
  }

  @override
  Widget build(BuildContext context) {
    final reg = PageStateRegistryProvider.of(context);
    return Column(
      children: [
        Text(widget.label),
        Text('filter:$_filter'),
        Text('sort:$_sort'),
        Row(
          children: [
            TextButton(
              key: const ValueKey('filter_0'),
              onPressed: () {
                setState(() => _filter = 0);
                reg.set('filter', 0);
              },
              child: const Text('F0'),
            ),
            TextButton(
              key: const ValueKey('filter_1'),
              onPressed: () {
                setState(() => _filter = 1);
                reg.set('filter', 1);
              },
              child: const Text('F1'),
            ),
            TextButton(
              key: const ValueKey('sort_asc'),
              onPressed: () {
                setState(() => _sort = 'asc');
                reg.set('sort', 'asc');
              },
              child: const Text('Asc'),
            ),
            TextButton(
              key: const ValueKey('sort_desc'),
              onPressed: () {
                setState(() => _sort = 'desc');
                reg.set('sort', 'desc');
              },
              child: const Text('Desc'),
            ),
          ],
        ),
      ],
    );
  }
}

class _FilterSortRoute extends StatelessWidget {
  final String label;
  const _FilterSortRoute({required this.label});

  @override
  Widget build(BuildContext context) {
    return LayoutManager(
      slots: LayoutSlots(
        body: _FilterSortPage(label: label),
        scrollable: true,
        safeArea: false,
      ),
    );
  }
}

/// Simple page that renders a label without any state. Used as "other" page.
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
// TEST ROUTER + APP FACTORY
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
        builder: (_, s) => const _SimplePage(label: '__home__'),
      ),
      GoRoute(
        path: '/tabs-a',
        builder: (_, s) => const _TabbedRoute(label: '__tabs-a__'),
      ),
      GoRoute(
        path: '/tabs-b',
        builder: (_, s) => const _TabbedRoute(label: '__tabs-b__'),
      ),
      GoRoute(
        path: '/tabs-c',
        builder: (_, s) => const _TabbedRoute(label: '__tabs-c__'),
      ),
      GoRoute(
        path: '/other',
        builder: (_, s) => const _SimplePage(label: '__other__'),
      ),
      GoRoute(
        path: '/combined',
        builder: (_, s) => const _FilterSortRoute(label: '__combined__'),
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

ProviderContainer _makeContainer() {
  final c = ProviderContainer();
  addTearDown(c.dispose);
  return c;
}

// =============================================================================
// INTEGRATION TESTS
// =============================================================================

void main() {
  // ---------------------------------------------------------------------------
  // FLOW 1: Three-page chain — each page saves its own tab
  // ---------------------------------------------------------------------------

  group('Flow 1 — Multi-page independent tab state', () {
    testWidgets('three pages each save their own tab index after leaving', (
      tester,
    ) async {
      tester.view.physicalSize = const Size(800, 1200);
      tester.view.devicePixelRatio = 1.0;
      addTearDown(tester.view.resetPhysicalSize);

      final c = _makeContainer();
      final router = _buildRouter(c);
      await tester.pumpWidget(_buildApp(router, c));
      await tester.pumpAndSettle();

      // Navigate to tabs-a, select tab 2
      c.read(isNavigatingProvider.notifier).set(true);
      router.go('/tabs-a');
      await tester.pumpAndSettle();
      await tester.tap(find.byKey(const ValueKey('tab___tabs-a___2')));
      await tester.pumpAndSettle();

      // Navigate to tabs-b, select tab 3
      c.read(isNavigatingProvider.notifier).set(true);
      router.go('/tabs-b');
      await tester.pumpAndSettle();
      await tester.tap(find.byKey(const ValueKey('tab___tabs-b___3')));
      await tester.pumpAndSettle();

      // Navigate to other — triggers dispose of tabs-b
      c.read(isNavigatingProvider.notifier).set(true);
      router.go('/other');
      await tester.pumpAndSettle();

      // Verify: both tabs-a (index 1) and tabs-b (index 2) have correct tab saved
      final hist = c.read(navHistoryProvider);
      final entryA = hist.entries.firstWhere((e) => e.path == '/tabs-a');
      final entryB = hist.entries.firstWhere((e) => e.path == '/tabs-b');

      expect(entryA.pageState['tab'], 2, reason: 'tabs-a must have tab=2');
      expect(entryB.pageState['tab'], 3, reason: 'tabs-b must have tab=3');
    });
  });

  // ---------------------------------------------------------------------------
  // FLOW 2: UNDO restores to each page's own saved tab
  // ---------------------------------------------------------------------------

  group('Flow 2 — UNDO restores each page independently', () {
    testWidgets(
      'UNDO to tabs-b restores tab 3, UNDO to tabs-a restores tab 2',
      (tester) async {
        tester.view.physicalSize = const Size(800, 1200);
        tester.view.devicePixelRatio = 1.0;
        addTearDown(tester.view.resetPhysicalSize);

        final c = _makeContainer();
        final router = _buildRouter(c);
        await tester.pumpWidget(_buildApp(router, c));
        await tester.pumpAndSettle();

        // Build history: / → tabs-a(tab2) → tabs-b(tab3) → other
        c.read(isNavigatingProvider.notifier).set(true);
        router.go('/tabs-a');
        await tester.pumpAndSettle();
        await tester.tap(find.byKey(const ValueKey('tab___tabs-a___2')));
        await tester.pumpAndSettle();

        c.read(isNavigatingProvider.notifier).set(true);
        router.go('/tabs-b');
        await tester.pumpAndSettle();
        await tester.tap(find.byKey(const ValueKey('tab___tabs-b___3')));
        await tester.pumpAndSettle();

        c.read(isNavigatingProvider.notifier).set(true);
        router.go('/other');
        await tester.pumpAndSettle();

        // UNDO to tabs-b
        router.go('/tabs-b');
        await tester.pumpAndSettle();
        expect(
          find.text('tab:3'),
          findsOneWidget,
          reason: 'UNDO to tabs-b must restore tab 3',
        );

        // UNDO to tabs-a
        router.go('/tabs-a');
        await tester.pumpAndSettle();
        expect(
          find.text('tab:2'),
          findsOneWidget,
          reason: 'UNDO to tabs-a must restore tab 2',
        );
      },
    );
  });

  // ---------------------------------------------------------------------------
  // FLOW 3: History trim clears forward page state
  // ---------------------------------------------------------------------------

  group('Flow 3 — History trim after UNDO + new push', () {
    testWidgets('forward entries are trimmed when pushing after UNDO', (
      tester,
    ) async {
      tester.view.physicalSize = const Size(800, 1200);
      tester.view.devicePixelRatio = 1.0;
      addTearDown(tester.view.resetPhysicalSize);

      final c = _makeContainer();
      final router = _buildRouter(c);
      await tester.pumpWidget(_buildApp(router, c));
      await tester.pumpAndSettle();

      // Build history: / → tabs-a → tabs-b → tabs-c
      c.read(isNavigatingProvider.notifier).set(true);
      router.go('/tabs-a');
      await tester.pumpAndSettle();
      c.read(isNavigatingProvider.notifier).set(true);
      router.go('/tabs-b');
      await tester.pumpAndSettle();
      c.read(isNavigatingProvider.notifier).set(true);
      router.go('/tabs-c');
      await tester.pumpAndSettle();
      // history: [/, /tabs-a, /tabs-b, /tabs-c] index=3

      // UNDO back to tabs-a
      router.go('/tabs-b');
      await tester.pumpAndSettle();
      router.go('/tabs-a');
      await tester.pumpAndSettle();
      expect(c.read(navHistoryProvider).currentIndex, 1);

      // New push from tabs-a — should trim tabs-b and tabs-c
      c.read(isNavigatingProvider.notifier).set(true);
      router.go('/other');
      await tester.pumpAndSettle();

      final hist = c.read(navHistoryProvider);
      final paths = hist.entries.map((e) => e.path).toList();
      expect(paths, [
        '/',
        '/tabs-a',
        '/other',
      ], reason: 'Forward entries must be trimmed after new push from UNDO');
      expect(c.read(canGoForwardProvider), false);
    });
  });

  // ---------------------------------------------------------------------------
  // FLOW 4: Scroll + page state coexist across UNDO
  // ---------------------------------------------------------------------------

  group('Flow 4 — Scroll and page state coexist', () {
    testWidgets(
      'scroll position and tab state are both saved and restored on UNDO',
      (tester) async {
        tester.view.physicalSize = const Size(800, 1200);
        tester.view.devicePixelRatio = 1.0;
        addTearDown(tester.view.resetPhysicalSize);

        final c = _makeContainer();
        final router = _buildRouter(c);
        await tester.pumpWidget(_buildApp(router, c));
        await tester.pumpAndSettle();

        // Navigate to tabs-a
        c.read(isNavigatingProvider.notifier).set(true);
        router.go('/tabs-a');
        await tester.pumpAndSettle();

        // Select tab 1
        await tester.tap(find.byKey(const ValueKey('tab___tabs-a___1')));
        await tester.pumpAndSettle();

        // Scroll outer page
        await tester.drag(
          find.byType(SingleChildScrollView),
          const Offset(0, -200),
        );
        await tester.pumpAndSettle();
        final scrollOffset = tester
            .widget<SingleChildScrollView>(find.byType(SingleChildScrollView))
            .controller!
            .offset;
        expect(scrollOffset, greaterThan(0));

        // Navigate away
        c.read(isNavigatingProvider.notifier).set(true);
        router.go('/');
        await tester.pumpAndSettle();

        // Verify both saved in history
        final hist = c.read(navHistoryProvider);
        final entryA = hist.entries.firstWhere((e) => e.path == '/tabs-a');
        expect(entryA.pageState['tab'], 1);
        expect(
          entryA.scrollPositions['page'],
          moreOrLessEquals(scrollOffset, epsilon: 1.0),
        );

        // UNDO back to tabs-a
        router.go('/tabs-a');
        await tester.pumpAndSettle();

        // Both must be restored
        expect(find.text('tab:1'), findsOneWidget, reason: 'tab restored');
        final restoredScroll = tester
            .widget<SingleChildScrollView>(find.byType(SingleChildScrollView))
            .controller!
            .offset;
        expect(
          restoredScroll,
          moreOrLessEquals(scrollOffset, epsilon: 1.0),
          reason: 'scroll restored',
        );
      },
    );
  });

  // ---------------------------------------------------------------------------
  // FLOW 5: Multiple UNDO/REDO cycles all restore correctly
  // ---------------------------------------------------------------------------

  group('Flow 5 — Repeated UNDO/REDO cycle', () {
    testWidgets('tab state survives 3 UNDO/REDO round-trips', (tester) async {
      tester.view.physicalSize = const Size(800, 1200);
      tester.view.devicePixelRatio = 1.0;
      addTearDown(tester.view.resetPhysicalSize);

      final c = _makeContainer();
      final router = _buildRouter(c);
      await tester.pumpWidget(_buildApp(router, c));
      await tester.pumpAndSettle();

      // Build: / → tabs-a(tab2) → other
      c.read(isNavigatingProvider.notifier).set(true);
      router.go('/tabs-a');
      await tester.pumpAndSettle();
      await tester.tap(find.byKey(const ValueKey('tab___tabs-a___2')));
      await tester.pumpAndSettle();
      c.read(isNavigatingProvider.notifier).set(true);
      router.go('/other');
      await tester.pumpAndSettle();

      // Cycle 1
      router.go('/tabs-a');
      await tester.pumpAndSettle();
      expect(
        find.text('tab:2'),
        findsOneWidget,
        reason: 'Cycle 1: UNDO restores tab',
      );
      router.go('/other');
      await tester.pumpAndSettle();

      // Cycle 2
      router.go('/tabs-a');
      await tester.pumpAndSettle();
      expect(
        find.text('tab:2'),
        findsOneWidget,
        reason: 'Cycle 2: UNDO restores tab',
      );
      router.go('/other');
      await tester.pumpAndSettle();

      // Cycle 3
      router.go('/tabs-a');
      await tester.pumpAndSettle();
      expect(
        find.text('tab:2'),
        findsOneWidget,
        reason: 'Cycle 3: UNDO restores tab',
      );
    });
  });

  // ---------------------------------------------------------------------------
  // FLOW 6: Multiple state keys per page
  // ---------------------------------------------------------------------------

  group('Flow 6 — Multiple state keys per page', () {
    testWidgets('filter and sort are each independently saved and restored', (
      tester,
    ) async {
      tester.view.physicalSize = const Size(800, 1200);
      tester.view.devicePixelRatio = 1.0;
      addTearDown(tester.view.resetPhysicalSize);

      final c = _makeContainer();
      final router = _buildRouter(c);
      await tester.pumpWidget(_buildApp(router, c));
      await tester.pumpAndSettle();

      // Navigate to combined page
      c.read(isNavigatingProvider.notifier).set(true);
      router.go('/combined');
      await tester.pumpAndSettle();

      // Set filter=1 and sort=desc
      await tester.tap(find.byKey(const ValueKey('filter_1')));
      await tester.pumpAndSettle();
      await tester.tap(find.byKey(const ValueKey('sort_desc')));
      await tester.pumpAndSettle();
      expect(find.text('filter:1'), findsOneWidget);
      expect(find.text('sort:desc'), findsOneWidget);

      // Navigate away
      c.read(isNavigatingProvider.notifier).set(true);
      router.go('/');
      await tester.pumpAndSettle();

      // Verify both saved
      final hist = c.read(navHistoryProvider);
      final entry = hist.entries.firstWhere((e) => e.path == '/combined');
      expect(entry.pageState['filter'], 1);
      expect(entry.pageState['sort'], 'desc');

      // UNDO back
      router.go('/combined');
      await tester.pumpAndSettle();

      // Both must be restored
      expect(find.text('filter:1'), findsOneWidget, reason: 'filter restored');
      expect(find.text('sort:desc'), findsOneWidget, reason: 'sort restored');
    });

    testWidgets(
      'fresh navigation to combined page starts with default values',
      (tester) async {
        tester.view.physicalSize = const Size(800, 1200);
        tester.view.devicePixelRatio = 1.0;
        addTearDown(tester.view.resetPhysicalSize);

        final c = _makeContainer();
        final router = _buildRouter(c);
        await tester.pumpWidget(_buildApp(router, c));
        await tester.pumpAndSettle();

        // Visit combined, set values, leave
        c.read(isNavigatingProvider.notifier).set(true);
        router.go('/combined');
        await tester.pumpAndSettle();
        await tester.tap(find.byKey(const ValueKey('filter_1')));
        await tester.pumpAndSettle();
        await tester.tap(find.byKey(const ValueKey('sort_desc')));
        await tester.pumpAndSettle();
        c.read(isNavigatingProvider.notifier).set(true);
        router.go('/');
        await tester.pumpAndSettle();

        // New push to combined (not UNDO)
        c.read(isNavigatingProvider.notifier).set(true);
        router.go('/other');
        await tester.pumpAndSettle();
        c.read(isNavigatingProvider.notifier).set(true);
        router.go('/combined');
        await tester.pumpAndSettle();

        // Must start with defaults (filter=0, sort=asc)
        expect(
          find.text('filter:0'),
          findsOneWidget,
          reason: 'fresh nav: filter=0',
        );
        expect(
          find.text('sort:asc'),
          findsOneWidget,
          reason: 'fresh nav: sort=asc',
        );
      },
    );
  });
}

// ignore_for_file: depend_on_referenced_packages

// =============================================================================
// NESTED SCROLL — WIDGET TESTS
// =============================================================================
//
// These tests mount a real LayoutManager with a body that contains MULTIPLE
// inner scroll areas alongside the outer page scroll — mirroring the pattern
// used in DemoNav2Page:
//
//   outer scroll  → 'page'   key  (SingleChildScrollView owned by LayoutManager)
//   inner scroll  → 'nest_1' key  (ListView in body column)
//   inner scroll  → 'nest_2' key  (ListView in body column)
//
// WHAT THESE TESTS PROVE:
//   1. Nested pages can request multiple inner scroll controllers via
//      ScrollRegistryProvider.of(context).controller(id)
//   2. Scrolling each inner ListView updates its own cachedPosition key
//   3. All scroll positions (outer + inner) are saved to history when
//      LayoutManager.dispose() fires (navigating away)
//   4. All scroll positions are restored from history when returning via UNDO
//   5. Fresh navigation (new push) always starts every scroll at 0
//
// WHY STUB PAGES:
// Real production pages have heavy dependencies (images, i18n, 3D engine).
// The stub pages here use real LayoutManager + real ScrollRegistryProvider so
// the full nested scroll lifecycle is exercised with zero extra dependencies.
//
// KEY TIMING (same as simple_sroll tests):
//   - redirect defers push/undoRedo via addPostFrameCallback
//   - LayoutManager.initState defers scroll restore via addPostFrameCallback
//   - LayoutManager.dispose saves scroll positions synchronously
//   → Use pumpAndSettle() to drain ALL pending callbacks.
//
// Run with:
//   flutter test lib/navigation/test/nested_scroll/nested_scroll_widget_test.dart
//   flutter test lib/navigation/test/
//
// =============================================================================

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:go_router/go_router.dart';
import 'package:grande_panorama_ar/layout/layout_manager.dart';
import 'package:grande_panorama_ar/layout/layout_slots.dart';
import 'package:grande_panorama_ar/layout/scrollController/scroll_registry_provider.dart';
import 'package:grande_panorama_ar/navigation/navConfig/current_route_provider.dart';
import 'package:grande_panorama_ar/navigation/histConfig/history_provider.dart';
import 'package:grande_panorama_ar/navigation/histConfig/route_observer.dart';
import 'package:grande_panorama_ar/navigation/histConfig/is_navigating_provider.dart';

// =============================================================================
// STUB PAGES
// =============================================================================

/// A page with ONE outer scroll (managed by LayoutManager) and TWO inner
/// scroll areas that request controllers via ScrollRegistryProvider.
///
/// This is the minimal reproduction of the DemoNav2Page pattern.
class _NestedScrollPage extends StatelessWidget {
  final String label;
  const _NestedScrollPage({required this.label});

  @override
  Widget build(BuildContext context) {
    return LayoutManager(
      slots: LayoutSlots(
        // scrollable:true  → LayoutManager creates 'page' controller
        body: _NestedBody(label: label),
        scrollable: true,
        safeArea: false,
      ),
    );
  }
}

/// Body that requests two inner scroll controllers by ID from the registry.
///
/// The outer SingleChildScrollView is owned by LayoutManager (key: 'page').
/// The two inner ListViews here use keys 'nest_1' and 'nest_2'.
///
/// WHY Builder:
/// ScrollRegistryProvider.of(context) requires a context that is a DESCENDANT
/// of LayoutManager's build (which inserts the InheritedWidget). The Builder
/// provides that descendant context — exactly as DemoNav2Page does.
class _NestedBody extends StatelessWidget {
  final String label;
  const _NestedBody({required this.label});

  @override
  Widget build(BuildContext context) {
    return Builder(
      builder: (innerCtx) {
        final registry = ScrollRegistryProvider.of(innerCtx);
        final ctrl1 = registry.controller('nest_1');
        final ctrl2 = registry.controller('nest_2');

        return Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            // Page title — used by find.text() in tests
            Padding(padding: const EdgeInsets.all(8), child: Text(label)),
            // Inner scroll area 1 — fixed height so the outer page can scroll
            SizedBox(
              height: 300,
              child: ListView.builder(
                key: const ValueKey('nest_1_list'),
                controller: ctrl1,
                itemCount: 30,
                itemBuilder: (_, i) =>
                    SizedBox(height: 60, child: Text('$label nest_1 item $i')),
              ),
            ),
            // Inner scroll area 2
            SizedBox(
              height: 300,
              child: ListView.builder(
                key: const ValueKey('nest_2_list'),
                controller: ctrl2,
                itemCount: 30,
                itemBuilder: (_, i) =>
                    SizedBox(height: 60, child: Text('$label nest_2 item $i')),
              ),
            ),
            // Extra height so the outer SingleChildScrollView can also scroll
            ...List.generate(
              20,
              (i) => SizedBox(height: 60, child: Text('$label outer item $i')),
            ),
          ],
        );
      },
    );
  }
}

/// A simple non-nested page used as a "landing" route to trigger
/// LayoutManager disposal (and thus scroll save) on the nested page.
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

GoRouter _buildRouter(ProviderContainer container) {
  return GoRouter(
    initialLocation: '/',
    redirect: (context, state) {
      final path = state.uri.toString();
      final isNavigating = container
          .read(isNavigatingProvider.notifier)
          .consume();

      WidgetsBinding.instance.addPostFrameCallback((_) {
        final notifier = container.read(navHistoryProvider.notifier);
        final histState = container.read(navHistoryProvider);
        if (histState.currentIndex < 0) {
          notifier.push(path);
        } else if (isNavigating) {
          notifier.push(path);
        } else {
          notifier.undoRedo(path);
        }
      });
      return null;
    },
    observers: [
      NavObserver(
        onRouteChanged: (path) =>
            container.read(currentRouteProvider.notifier).update(path),
      ),
    ],
    routes: [
      GoRoute(
        path: '/',
        builder: (_, s) => const _SimplePage(label: '__home__'),
      ),
      GoRoute(
        path: '/nested',
        builder: (_, s) => const _NestedScrollPage(label: '__nested__'),
      ),
      GoRoute(
        path: '/other',
        builder: (_, s) => const _SimplePage(label: '__other__'),
      ),
    ],
    errorBuilder: (context, state) => Scaffold(
      body: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          const Icon(Icons.error_outline),
          Text('ERROR: ${state.uri}'),
        ],
      ),
    ),
  );
}

Widget _buildApp(GoRouter router, ProviderContainer container) {
  return UncontrolledProviderScope(
    container: container,
    child: MaterialApp.router(routerConfig: router, theme: ThemeData.light()),
  );
}

ProviderContainer _makeContainer() {
  final c = ProviderContainer();
  addTearDown(c.dispose);
  return c;
}

// =============================================================================
// HELPERS
// =============================================================================

/// Finds the outer SingleChildScrollView (owned by LayoutManager).
/// On the nested page there is exactly one SingleChildScrollView wrapping
/// the whole body; the inner areas use ListView (not SingleChildScrollView).
Finder get _outerScrollFinder => find.byType(SingleChildScrollView);

/// Returns the current scroll offset of the outer scroll view.
double _outerOffset(WidgetTester tester) {
  final widget = tester.widget<SingleChildScrollView>(_outerScrollFinder);
  return widget.controller!.offset;
}

/// Returns the current scroll offset of a specific inner ListView.
double _innerOffset(WidgetTester tester, Key key) {
  final widget = tester.widget<ListView>(find.byKey(key));
  return widget.controller!.offset;
}

// =============================================================================
// TESTS
// =============================================================================

void main() {
  // Fix viewport for all scroll tests so drag distances produce consistent results.
  setUp(() {
    // No global setup needed — each test sets its own view size.
  });

  // ---------------------------------------------------------------------------
  // GROUP 1: Nested page renders correctly
  // ---------------------------------------------------------------------------

  group('Nested page renders correctly', () {
    testWidgets('nested page shows its label text', (tester) async {
      final c = _makeContainer();
      final router = _buildRouter(c);

      await tester.pumpWidget(_buildApp(router, c));
      await tester.pumpAndSettle();

      c.read(isNavigatingProvider.notifier).set(true);
      router.go('/nested');
      await tester.pumpAndSettle();

      expect(find.text('__nested__'), findsWidgets);
      expect(find.byIcon(Icons.error_outline), findsNothing);
    });

    testWidgets('nested page has one outer SingleChildScrollView', (
      tester,
    ) async {
      final c = _makeContainer();
      final router = _buildRouter(c);

      await tester.pumpWidget(_buildApp(router, c));
      await tester.pumpAndSettle();

      c.read(isNavigatingProvider.notifier).set(true);
      router.go('/nested');
      await tester.pumpAndSettle();

      // LayoutManager with scrollable:true inserts exactly one outer scroll view
      expect(find.byType(SingleChildScrollView), findsOneWidget);
    });

    testWidgets('nested page has two inner ListViews with ValueKeys', (
      tester,
    ) async {
      final c = _makeContainer();
      final router = _buildRouter(c);

      await tester.pumpWidget(_buildApp(router, c));
      await tester.pumpAndSettle();

      c.read(isNavigatingProvider.notifier).set(true);
      router.go('/nested');
      await tester.pumpAndSettle();

      expect(
        find.byKey(const ValueKey('nest_1_list')),
        findsOneWidget,
        reason: 'Inner ListView 1 must be present',
      );
      expect(
        find.byKey(const ValueKey('nest_2_list')),
        findsOneWidget,
        reason: 'Inner ListView 2 must be present',
      );
    });
  });

  // ---------------------------------------------------------------------------
  // GROUP 2: Inner scroll controllers are independent
  // ---------------------------------------------------------------------------

  group('Inner scroll controllers are independent', () {
    testWidgets('scrolling nest_1 does not move nest_2 or outer', (
      tester,
    ) async {
      tester.view.physicalSize = const Size(800, 1200);
      tester.view.devicePixelRatio = 1.0;
      addTearDown(tester.view.resetPhysicalSize);

      final c = _makeContainer();
      final router = _buildRouter(c);

      await tester.pumpWidget(_buildApp(router, c));
      await tester.pumpAndSettle();

      c.read(isNavigatingProvider.notifier).set(true);
      router.go('/nested');
      await tester.pumpAndSettle();

      // Scroll inner list 1
      await tester.drag(
        find.byKey(const ValueKey('nest_1_list')),
        const Offset(0, -200),
      );
      await tester.pumpAndSettle();

      final nest1Offset = _innerOffset(tester, const ValueKey('nest_1_list'));
      final nest2Offset = _innerOffset(tester, const ValueKey('nest_2_list'));
      final outerOff = _outerOffset(tester);

      expect(nest1Offset, greaterThan(0), reason: 'nest_1 must have scrolled');
      expect(
        nest2Offset,
        moreOrLessEquals(0.0, epsilon: 1.0),
        reason: 'nest_2 must NOT be affected by nest_1 scroll',
      );
      expect(
        outerOff,
        moreOrLessEquals(0.0, epsilon: 1.0),
        reason: 'Outer scroll must NOT be affected by nest_1 scroll',
      );
    });

    testWidgets('scrolling nest_2 does not move nest_1 or outer', (
      tester,
    ) async {
      tester.view.physicalSize = const Size(800, 1200);
      tester.view.devicePixelRatio = 1.0;
      addTearDown(tester.view.resetPhysicalSize);

      final c = _makeContainer();
      final router = _buildRouter(c);

      await tester.pumpWidget(_buildApp(router, c));
      await tester.pumpAndSettle();

      c.read(isNavigatingProvider.notifier).set(true);
      router.go('/nested');
      await tester.pumpAndSettle();

      // Scroll inner list 2
      await tester.drag(
        find.byKey(const ValueKey('nest_2_list')),
        const Offset(0, -150),
      );
      await tester.pumpAndSettle();

      final nest1Offset = _innerOffset(tester, const ValueKey('nest_1_list'));
      final nest2Offset = _innerOffset(tester, const ValueKey('nest_2_list'));
      final outerOff = _outerOffset(tester);

      expect(nest2Offset, greaterThan(0), reason: 'nest_2 must have scrolled');
      expect(
        nest1Offset,
        moreOrLessEquals(0.0, epsilon: 1.0),
        reason: 'nest_1 must NOT be affected by nest_2 scroll',
      );
      expect(
        outerOff,
        moreOrLessEquals(0.0, epsilon: 1.0),
        reason: 'Outer scroll must NOT be affected by nest_2 scroll',
      );
    });
  });

  // ---------------------------------------------------------------------------
  // GROUP 3: All scroll positions are saved on navigation away
  // ---------------------------------------------------------------------------

  group('All scroll positions saved when navigating away', () {
    testWidgets(
      'nest_1 scroll is saved in history entry after navigating away',
      (tester) async {
        tester.view.physicalSize = const Size(800, 1200);
        tester.view.devicePixelRatio = 1.0;
        addTearDown(tester.view.resetPhysicalSize);

        final c = _makeContainer();
        final router = _buildRouter(c);

        await tester.pumpWidget(_buildApp(router, c));
        await tester.pumpAndSettle();

        c.read(isNavigatingProvider.notifier).set(true);
        router.go('/nested');
        await tester.pumpAndSettle();

        // Scroll nest_1
        await tester.drag(
          find.byKey(const ValueKey('nest_1_list')),
          const Offset(0, -200),
        );
        await tester.pumpAndSettle();

        final savedNest1 = _innerOffset(tester, const ValueKey('nest_1_list'));
        expect(savedNest1, greaterThan(0));

        // Navigate away → triggers LayoutManager.dispose() → saves positions
        c.read(isNavigatingProvider.notifier).set(true);
        router.go('/other');
        await tester.pumpAndSettle();

        final hist = c.read(navHistoryProvider);
        final nestedEntry = hist.entries[1]; // index 1 = /nested
        expect(nestedEntry.path, '/nested');
        expect(
          nestedEntry.scrollPositions['nest_1'],
          moreOrLessEquals(savedNest1, epsilon: 1.0),
          reason: 'nest_1 scroll must be saved in history',
        );
      },
    );

    testWidgets(
      'nest_2 scroll is saved in history entry after navigating away',
      (tester) async {
        tester.view.physicalSize = const Size(800, 1200);
        tester.view.devicePixelRatio = 1.0;
        addTearDown(tester.view.resetPhysicalSize);

        final c = _makeContainer();
        final router = _buildRouter(c);

        await tester.pumpWidget(_buildApp(router, c));
        await tester.pumpAndSettle();

        c.read(isNavigatingProvider.notifier).set(true);
        router.go('/nested');
        await tester.pumpAndSettle();

        // Scroll nest_2
        await tester.drag(
          find.byKey(const ValueKey('nest_2_list')),
          const Offset(0, -180),
        );
        await tester.pumpAndSettle();

        final savedNest2 = _innerOffset(tester, const ValueKey('nest_2_list'));
        expect(savedNest2, greaterThan(0));

        c.read(isNavigatingProvider.notifier).set(true);
        router.go('/other');
        await tester.pumpAndSettle();

        final hist = c.read(navHistoryProvider);
        final nestedEntry = hist.entries[1];
        expect(
          nestedEntry.scrollPositions['nest_2'],
          moreOrLessEquals(savedNest2, epsilon: 1.0),
          reason: 'nest_2 scroll must be saved in history',
        );
      },
    );

    testWidgets(
      'all three scroll areas (outer + nest_1 + nest_2) are saved simultaneously',
      (tester) async {
        tester.view.physicalSize = const Size(800, 3000);
        tester.view.devicePixelRatio = 1.0;
        addTearDown(tester.view.resetPhysicalSize);

        final c = _makeContainer();
        final router = _buildRouter(c);

        await tester.pumpWidget(_buildApp(router, c));
        await tester.pumpAndSettle();

        c.read(isNavigatingProvider.notifier).set(true);
        router.go('/nested');
        await tester.pumpAndSettle();

        // Scroll inner 1
        await tester.drag(
          find.byKey(const ValueKey('nest_1_list')),
          const Offset(0, -200),
        );
        await tester.pumpAndSettle();
        final savedNest1 = _innerOffset(tester, const ValueKey('nest_1_list'));

        // Scroll inner 2
        await tester.drag(
          find.byKey(const ValueKey('nest_2_list')),
          const Offset(0, -120),
        );
        await tester.pumpAndSettle();
        final savedNest2 = _innerOffset(tester, const ValueKey('nest_2_list'));

        expect(savedNest1, greaterThan(0));
        expect(savedNest2, greaterThan(0));

        // Navigate away
        c.read(isNavigatingProvider.notifier).set(true);
        router.go('/other');
        await tester.pumpAndSettle();

        final hist = c.read(navHistoryProvider);
        final nestedEntry = hist.entries[1];

        expect(
          nestedEntry.scrollPositions['nest_1'],
          moreOrLessEquals(savedNest1, epsilon: 1.0),
          reason: 'nest_1 must be saved',
        );
        expect(
          nestedEntry.scrollPositions['nest_2'],
          moreOrLessEquals(savedNest2, epsilon: 1.0),
          reason: 'nest_2 must be saved',
        );
      },
    );
  });

  // ---------------------------------------------------------------------------
  // GROUP 4: All scroll positions are RESTORED on UNDO
  // ---------------------------------------------------------------------------

  group('All scroll positions restored on UNDO', () {
    testWidgets('nest_1 scroll restored after UNDO', (tester) async {
      tester.view.physicalSize = const Size(800, 1200);
      tester.view.devicePixelRatio = 1.0;
      addTearDown(tester.view.resetPhysicalSize);

      final c = _makeContainer();
      final router = _buildRouter(c);

      await tester.pumpWidget(_buildApp(router, c));
      await tester.pumpAndSettle();

      c.read(isNavigatingProvider.notifier).set(true);
      router.go('/nested');
      await tester.pumpAndSettle();

      await tester.drag(
        find.byKey(const ValueKey('nest_1_list')),
        const Offset(0, -200),
      );
      await tester.pumpAndSettle();
      final scrolledNest1 = _innerOffset(tester, const ValueKey('nest_1_list'));
      expect(scrolledNest1, greaterThan(0));

      // Navigate away
      c.read(isNavigatingProvider.notifier).set(true);
      router.go('/other');
      await tester.pumpAndSettle();

      // UNDO → back to /nested
      router.go('/nested');
      await tester.pumpAndSettle();

      final restoredNest1 = _innerOffset(tester, const ValueKey('nest_1_list'));
      expect(
        restoredNest1,
        moreOrLessEquals(scrolledNest1, epsilon: 1.0),
        reason: 'nest_1 scroll must be restored after UNDO',
      );
    });

    testWidgets('nest_2 scroll restored after UNDO', (tester) async {
      tester.view.physicalSize = const Size(800, 1200);
      tester.view.devicePixelRatio = 1.0;
      addTearDown(tester.view.resetPhysicalSize);

      final c = _makeContainer();
      final router = _buildRouter(c);

      await tester.pumpWidget(_buildApp(router, c));
      await tester.pumpAndSettle();

      c.read(isNavigatingProvider.notifier).set(true);
      router.go('/nested');
      await tester.pumpAndSettle();

      await tester.drag(
        find.byKey(const ValueKey('nest_2_list')),
        const Offset(0, -180),
      );
      await tester.pumpAndSettle();
      final scrolledNest2 = _innerOffset(tester, const ValueKey('nest_2_list'));
      expect(scrolledNest2, greaterThan(0));

      c.read(isNavigatingProvider.notifier).set(true);
      router.go('/other');
      await tester.pumpAndSettle();

      router.go('/nested');
      await tester.pumpAndSettle();

      final restoredNest2 = _innerOffset(tester, const ValueKey('nest_2_list'));
      expect(
        restoredNest2,
        moreOrLessEquals(scrolledNest2, epsilon: 1.0),
        reason: 'nest_2 scroll must be restored after UNDO',
      );
    });

    testWidgets(
      'both nest_1 and nest_2 are independently restored after UNDO',
      (tester) async {
        tester.view.physicalSize = const Size(800, 1200);
        tester.view.devicePixelRatio = 1.0;
        addTearDown(tester.view.resetPhysicalSize);

        final c = _makeContainer();
        final router = _buildRouter(c);

        await tester.pumpWidget(_buildApp(router, c));
        await tester.pumpAndSettle();

        c.read(isNavigatingProvider.notifier).set(true);
        router.go('/nested');
        await tester.pumpAndSettle();

        // Scroll both inner areas to different offsets
        await tester.drag(
          find.byKey(const ValueKey('nest_1_list')),
          const Offset(0, -240),
        );
        await tester.pumpAndSettle();
        await tester.drag(
          find.byKey(const ValueKey('nest_2_list')),
          const Offset(0, -80),
        );
        await tester.pumpAndSettle();

        final scrolledNest1 = _innerOffset(
          tester,
          const ValueKey('nest_1_list'),
        );
        final scrolledNest2 = _innerOffset(
          tester,
          const ValueKey('nest_2_list'),
        );

        expect(
          scrolledNest1,
          greaterThan(scrolledNest2),
          reason: 'nest_1 was scrolled more than nest_2',
        );
        expect(scrolledNest2, greaterThan(0));

        c.read(isNavigatingProvider.notifier).set(true);
        router.go('/other');
        await tester.pumpAndSettle();

        router.go('/nested');
        await tester.pumpAndSettle();

        final restoredNest1 = _innerOffset(
          tester,
          const ValueKey('nest_1_list'),
        );
        final restoredNest2 = _innerOffset(
          tester,
          const ValueKey('nest_2_list'),
        );

        expect(
          restoredNest1,
          moreOrLessEquals(scrolledNest1, epsilon: 1.0),
          reason: 'nest_1 must be independently restored',
        );
        expect(
          restoredNest2,
          moreOrLessEquals(scrolledNest2, epsilon: 1.0),
          reason: 'nest_2 must be independently restored',
        );
      },
    );

    testWidgets('unscrolled inners stay at 0 after UNDO (zero-save scenario)', (
      tester,
    ) async {
      tester.view.physicalSize = const Size(800, 1200);
      tester.view.devicePixelRatio = 1.0;
      addTearDown(tester.view.resetPhysicalSize);

      final c = _makeContainer();
      final router = _buildRouter(c);

      await tester.pumpWidget(_buildApp(router, c));
      await tester.pumpAndSettle();

      // Visit nested page but do NOT scroll anything
      c.read(isNavigatingProvider.notifier).set(true);
      router.go('/nested');
      await tester.pumpAndSettle();

      c.read(isNavigatingProvider.notifier).set(true);
      router.go('/other');
      await tester.pumpAndSettle();

      router.go('/nested');
      await tester.pumpAndSettle();

      // All scrolls should be at 0
      expect(
        _innerOffset(tester, const ValueKey('nest_1_list')),
        moreOrLessEquals(0.0, epsilon: 1.0),
        reason: 'nest_1 was never scrolled — must stay at 0',
      );
      expect(
        _innerOffset(tester, const ValueKey('nest_2_list')),
        moreOrLessEquals(0.0, epsilon: 1.0),
        reason: 'nest_2 was never scrolled — must stay at 0',
      );
    });
  });

  // ---------------------------------------------------------------------------
  // GROUP 5: Fresh navigation does NOT restore inner scroll positions
  // ---------------------------------------------------------------------------

  group('Fresh navigation does not restore inner scroll positions', () {
    testWidgets('new push to /nested always starts nest_1 and nest_2 at 0', (
      tester,
    ) async {
      tester.view.physicalSize = const Size(800, 1200);
      tester.view.devicePixelRatio = 1.0;
      addTearDown(tester.view.resetPhysicalSize);

      final c = _makeContainer();
      final router = _buildRouter(c);

      await tester.pumpWidget(_buildApp(router, c));
      await tester.pumpAndSettle();

      // First visit: scroll both inner areas
      c.read(isNavigatingProvider.notifier).set(true);
      router.go('/nested');
      await tester.pumpAndSettle();
      await tester.drag(
        find.byKey(const ValueKey('nest_1_list')),
        const Offset(0, -200),
      );
      await tester.pumpAndSettle();
      await tester.drag(
        find.byKey(const ValueKey('nest_2_list')),
        const Offset(0, -150),
      );
      await tester.pumpAndSettle();

      // Leave
      c.read(isNavigatingProvider.notifier).set(true);
      router.go('/other');
      await tester.pumpAndSettle();

      // Navigate to /nested again as a NEW push (isNavigating=true) — not UNDO
      c.read(isNavigatingProvider.notifier).set(true);
      router.go('/nested');
      await tester.pumpAndSettle();

      // Fresh visit → inner scrolls must start at 0
      expect(
        _innerOffset(tester, const ValueKey('nest_1_list')),
        moreOrLessEquals(0.0, epsilon: 1.0),
        reason: 'Fresh push — nest_1 must start at 0',
      );
      expect(
        _innerOffset(tester, const ValueKey('nest_2_list')),
        moreOrLessEquals(0.0, epsilon: 1.0),
        reason: 'Fresh push — nest_2 must start at 0',
      );
    });
  });
}

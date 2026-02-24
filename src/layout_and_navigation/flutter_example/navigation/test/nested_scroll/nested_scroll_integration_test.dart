// ignore_for_file: depend_on_referenced_packages

// =============================================================================
// NESTED SCROLL — INTEGRATION TESTS
// =============================================================================
//
// These tests run the FULL stack end-to-end for a page with NESTED scroll areas:
//
//   GoRouter (real redirect + NavObserver)
//   + real Riverpod ProviderContainer (NavHistory, currentRoute)
//   + real LayoutManager (outer scroll lifecycle)
//   + real ScrollRegistryProvider (inner scroll lifecycle)
//   + nested-scroll stub page (outer 'page' + inner 'nest_1' + inner 'nest_2')
//
// WHAT THESE TESTS PROVE (end-to-end user flows):
//   1. Navigating to a nested-scroll page renders it without errors
//   2. Scrolling each inner area independently is possible
//   3. ALL scroll positions (outer + inner) are saved when leaving the page
//   4. ALL scroll positions are independently restored when returning via UNDO
//   5. Fresh navigation (new push) starts ALL scrolls at 0
//   6. Multi-page scenario: two different nested pages each save their own
//      complete set of scroll positions and restore independently
//
// DIFFERENCE FROM nested_scroll_widget_test.dart:
//   Widget tests assert individual mechanisms (save, restore, independence).
//   Integration tests assert COMPLETE USER FLOWS from app-launch to scroll restore.
//
// DIFFERENCE FROM simple_sroll/nav_integration_test.dart:
//   That file tests a single 'page' scroll key.
//   These tests specifically target 3+ simultaneous keys per page.
//
// Run with:
//   flutter test lib/navigation/test/nested_scroll/nested_scroll_integration_test.dart
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

/// Nested scroll page: outer scroll ('page') + two inner ListViews.
/// Keys used: 'page' (outer, managed by LayoutManager), 'nest_1', 'nest_2'.
class _NestedPage extends StatelessWidget {
  final String label;
  const _NestedPage(this.label);

  @override
  Widget build(BuildContext context) {
    return LayoutManager(
      slots: LayoutSlots(
        body: _NestedBody(label: label),
        scrollable: true,
        safeArea: false,
      ),
    );
  }
}

class _NestedBody extends StatelessWidget {
  final String label;
  const _NestedBody({required this.label});

  @override
  Widget build(BuildContext context) {
    return Builder(
      builder: (ctx) {
        final registry = ScrollRegistryProvider.of(ctx);
        final ctrl1 = registry.controller('nest_1');
        final ctrl2 = registry.controller('nest_2');

        return Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            Text(label),
            SizedBox(
              height: 300,
              child: ListView.builder(
                key: const ValueKey('nest_1_list'),
                controller: ctrl1,
                itemCount: 40,
                itemBuilder: (_, i) =>
                    SizedBox(height: 60, child: Text('$label nest1 $i')),
              ),
            ),
            SizedBox(
              height: 300,
              child: ListView.builder(
                key: const ValueKey('nest_2_list'),
                controller: ctrl2,
                itemCount: 40,
                itemBuilder: (_, i) =>
                    SizedBox(height: 60, child: Text('$label nest2 $i')),
              ),
            ),
            ...List.generate(
              20,
              (i) => SizedBox(height: 60, child: Text('$label outer $i')),
            ),
          ],
        );
      },
    );
  }
}

/// Simple page with no inner scrolls — used as a "landing" page to trigger
/// LayoutManager disposal (scroll save) on the nested page.
class _FlatPage extends StatelessWidget {
  final String label;
  const _FlatPage(this.label);

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
// TEST APP FACTORY
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
      GoRoute(path: '/', builder: (_, s) => const _FlatPage('__home__')),
      GoRoute(
        path: '/nested',
        builder: (_, s) => const _NestedPage('__nested__'),
      ),
      GoRoute(
        path: '/nested-b',
        builder: (_, s) => const _NestedPage('__nested-b__'),
      ),
      GoRoute(path: '/other', builder: (_, s) => const _FlatPage('__other__')),
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

double _innerOffset(WidgetTester tester, Key key) {
  final widget = tester.widget<ListView>(find.byKey(key));
  return widget.controller!.offset;
}

// =============================================================================
// INTEGRATION TESTS
// =============================================================================

void main() {
  // ---------------------------------------------------------------------------
  // FLOW 1: Nested page renders without errors
  // ---------------------------------------------------------------------------

  group('Flow 1 — Nested page launch', () {
    testWidgets('navigating to /nested shows the page without error', (
      tester,
    ) async {
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

    testWidgets('history has 2 entries after launch + navigate to /nested', (
      tester,
    ) async {
      final c = _makeContainer();
      final router = _buildRouter(c);

      await tester.pumpWidget(_buildApp(router, c));
      await tester.pumpAndSettle();

      c.read(isNavigatingProvider.notifier).set(true);
      router.go('/nested');
      await tester.pumpAndSettle();

      final hist = c.read(navHistoryProvider);
      expect(hist.entries.length, 2);
      expect(hist.entries[0].path, '/');
      expect(hist.entries[1].path, '/nested');
      expect(c.read(canGoBackProvider), true);
    });
  });

  // ---------------------------------------------------------------------------
  // FLOW 2: Inner scrolls are independent in a live app
  // ---------------------------------------------------------------------------

  group('Flow 2 — Independent inner scroll areas', () {
    testWidgets('scrolling nest_1 does not affect nest_2', (tester) async {
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

      final nest1 = _innerOffset(tester, const ValueKey('nest_1_list'));
      final nest2 = _innerOffset(tester, const ValueKey('nest_2_list'));

      expect(nest1, greaterThan(0), reason: 'nest_1 must have scrolled');
      expect(
        nest2,
        moreOrLessEquals(0.0, epsilon: 1.0),
        reason: 'nest_2 must be unaffected',
      );
    });

    testWidgets('both inner areas can scroll to different offsets', (
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

      await tester.drag(
        find.byKey(const ValueKey('nest_1_list')),
        const Offset(0, -300),
      );
      await tester.pumpAndSettle();

      await tester.drag(
        find.byKey(const ValueKey('nest_2_list')),
        const Offset(0, -100),
      );
      await tester.pumpAndSettle();

      final nest1 = _innerOffset(tester, const ValueKey('nest_1_list'));
      final nest2 = _innerOffset(tester, const ValueKey('nest_2_list'));

      expect(
        nest1,
        greaterThan(nest2),
        reason: 'nest_1 was dragged more than nest_2',
      );
      expect(nest2, greaterThan(0), reason: 'nest_2 must also have scrolled');
    });
  });

  // ---------------------------------------------------------------------------
  // FLOW 3: All scroll positions are saved when leaving nested page
  // ---------------------------------------------------------------------------

  group('Flow 3 — All scroll positions saved on navigation away', () {
    testWidgets('nest_1 and nest_2 positions are in history after leaving', (
      tester,
    ) async {
      tester.view.physicalSize = const Size(800, 1200);
      tester.view.devicePixelRatio = 1.0;
      addTearDown(tester.view.resetPhysicalSize);

      final c = _makeContainer();
      final router = _buildRouter(c);

      await tester.pumpWidget(_buildApp(router, c));
      await tester.pumpAndSettle();

      // Go to nested page
      c.read(isNavigatingProvider.notifier).set(true);
      router.go('/nested');
      await tester.pumpAndSettle();

      // Scroll both inners
      await tester.drag(
        find.byKey(const ValueKey('nest_1_list')),
        const Offset(0, -200),
      );
      await tester.pumpAndSettle();
      await tester.drag(
        find.byKey(const ValueKey('nest_2_list')),
        const Offset(0, -100),
      );
      await tester.pumpAndSettle();

      final savedNest1 = _innerOffset(tester, const ValueKey('nest_1_list'));
      final savedNest2 = _innerOffset(tester, const ValueKey('nest_2_list'));

      // Navigate away
      c.read(isNavigatingProvider.notifier).set(true);
      router.go('/other');
      await tester.pumpAndSettle();

      final hist = c.read(navHistoryProvider);
      final nestedEntry = hist.entries[1];
      expect(nestedEntry.path, '/nested');

      expect(
        nestedEntry.scrollPositions['nest_1'],
        moreOrLessEquals(savedNest1, epsilon: 1.0),
        reason: 'nest_1 must be in history after leaving',
      );
      expect(
        nestedEntry.scrollPositions['nest_2'],
        moreOrLessEquals(savedNest2, epsilon: 1.0),
        reason: 'nest_2 must be in history after leaving',
      );
    });

    testWidgets('unscrolled inners save as 0 (or absent) in history', (
      tester,
    ) async {
      final c = _makeContainer();
      final router = _buildRouter(c);

      await tester.pumpWidget(_buildApp(router, c));
      await tester.pumpAndSettle();

      c.read(isNavigatingProvider.notifier).set(true);
      router.go('/nested');
      await tester.pumpAndSettle();

      // No scrolling at all
      c.read(isNavigatingProvider.notifier).set(true);
      router.go('/other');
      await tester.pumpAndSettle();

      final hist = c.read(navHistoryProvider);
      final nestedEntry = hist.entries[1];

      // Either absent or 0 — both are acceptable
      final nest1Pos = nestedEntry.scrollPositions['nest_1'] ?? 0.0;
      final nest2Pos = nestedEntry.scrollPositions['nest_2'] ?? 0.0;

      expect(
        nest1Pos,
        moreOrLessEquals(0.0, epsilon: 1.0),
        reason: 'Unscrolled nest_1 must save 0',
      );
      expect(
        nest2Pos,
        moreOrLessEquals(0.0, epsilon: 1.0),
        reason: 'Unscrolled nest_2 must save 0',
      );
    });
  });

  // ---------------------------------------------------------------------------
  // FLOW 4: All scroll positions restored after UNDO (the core feature)
  // ---------------------------------------------------------------------------

  group('Flow 4 — Nested scroll UNDO restore (core feature)', () {
    testWidgets(
      'scroll nest_1 and nest_2, navigate away, UNDO → both restored',
      (tester) async {
        tester.view.physicalSize = const Size(800, 1200);
        tester.view.devicePixelRatio = 1.0;
        addTearDown(tester.view.resetPhysicalSize);

        final c = _makeContainer();
        final router = _buildRouter(c);

        await tester.pumpWidget(_buildApp(router, c));
        await tester.pumpAndSettle();

        // Step 1: Go to nested page
        c.read(isNavigatingProvider.notifier).set(true);
        router.go('/nested');
        await tester.pumpAndSettle();

        // Step 2: Scroll both inner areas
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

        expect(scrolledNest1, greaterThan(0));
        expect(scrolledNest2, greaterThan(0));

        // Step 3: Navigate away
        c.read(isNavigatingProvider.notifier).set(true);
        router.go('/other');
        await tester.pumpAndSettle();

        // Step 4: UNDO back to /nested
        router.go('/nested'); // no extra = undoRedo
        await tester.pumpAndSettle();

        // Step 5: Both inner positions must be restored
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
          reason: 'nest_1 must be restored after UNDO',
        );
        expect(
          restoredNest2,
          moreOrLessEquals(scrolledNest2, epsilon: 1.0),
          reason: 'nest_2 must be restored after UNDO',
        );
      },
    );

    testWidgets(
      'only nest_1 was scrolled — nest_1 restored, nest_2 stays at 0',
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

        // Only scroll nest_1
        await tester.drag(
          find.byKey(const ValueKey('nest_1_list')),
          const Offset(0, -300),
        );
        await tester.pumpAndSettle();

        final scrolledNest1 = _innerOffset(
          tester,
          const ValueKey('nest_1_list'),
        );
        expect(scrolledNest1, greaterThan(0));

        c.read(isNavigatingProvider.notifier).set(true);
        router.go('/other');
        await tester.pumpAndSettle();

        router.go('/nested');
        await tester.pumpAndSettle();

        expect(
          _innerOffset(tester, const ValueKey('nest_1_list')),
          moreOrLessEquals(scrolledNest1, epsilon: 1.0),
          reason: 'nest_1 must be restored',
        );
        expect(
          _innerOffset(tester, const ValueKey('nest_2_list')),
          moreOrLessEquals(0.0, epsilon: 1.0),
          reason: 'nest_2 was never scrolled — must stay at 0',
        );
      },
    );
  });

  // ---------------------------------------------------------------------------
  // FLOW 5: Fresh navigation does NOT restore inner positions
  // ---------------------------------------------------------------------------

  group('Flow 5 — Fresh navigation starts all inner scrolls at 0', () {
    testWidgets(
      'new push to /nested after having scrolled it — inners start at 0',
      (tester) async {
        tester.view.physicalSize = const Size(800, 1200);
        tester.view.devicePixelRatio = 1.0;
        addTearDown(tester.view.resetPhysicalSize);

        final c = _makeContainer();
        final router = _buildRouter(c);

        await tester.pumpWidget(_buildApp(router, c));
        await tester.pumpAndSettle();

        // First visit: scroll both inners, then leave
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
        c.read(isNavigatingProvider.notifier).set(true);
        router.go('/');
        await tester.pumpAndSettle();

        // Second visit via fresh push (isNavigating=true) — NOT an UNDO
        c.read(isNavigatingProvider.notifier).set(true);
        router.go('/nested');
        await tester.pumpAndSettle();

        expect(
          _innerOffset(tester, const ValueKey('nest_1_list')),
          moreOrLessEquals(0.0, epsilon: 1.0),
          reason: 'Fresh push: nest_1 must start at 0',
        );
        expect(
          _innerOffset(tester, const ValueKey('nest_2_list')),
          moreOrLessEquals(0.0, epsilon: 1.0),
          reason: 'Fresh push: nest_2 must start at 0',
        );
      },
    );
  });

  // ---------------------------------------------------------------------------
  // FLOW 6: Multi-page — two nested pages each save independent positions
  // ---------------------------------------------------------------------------

  group('Flow 6 — Two nested pages with independent scroll state', () {
    testWidgets(
      '/nested and /nested-b each save and restore their own positions',
      (tester) async {
        tester.view.physicalSize = const Size(800, 1200);
        tester.view.devicePixelRatio = 1.0;
        addTearDown(tester.view.resetPhysicalSize);

        final c = _makeContainer();
        final router = _buildRouter(c);

        await tester.pumpWidget(_buildApp(router, c));
        await tester.pumpAndSettle();

        // Visit /nested, scroll nest_1 a lot
        c.read(isNavigatingProvider.notifier).set(true);
        router.go('/nested');
        await tester.pumpAndSettle();
        await tester.drag(
          find.byKey(const ValueKey('nest_1_list')),
          const Offset(0, -300),
        );
        await tester.pumpAndSettle();
        final nestedNest1 = _innerOffset(tester, const ValueKey('nest_1_list'));
        expect(nestedNest1, greaterThan(0));

        // Navigate to /nested-b, scroll nest_1 a different amount
        c.read(isNavigatingProvider.notifier).set(true);
        router.go('/nested-b');
        await tester.pumpAndSettle();
        await tester.drag(
          find.byKey(const ValueKey('nest_1_list')),
          const Offset(0, -100),
        );
        await tester.pumpAndSettle();
        final nestedBNest1 = _innerOffset(
          tester,
          const ValueKey('nest_1_list'),
        );
        expect(nestedBNest1, greaterThan(0));

        // Both pages have saved their own nest_1 value — they must differ
        expect(
          nestedNest1,
          isNot(moreOrLessEquals(nestedBNest1, epsilon: 5.0)),
          reason: 'The two pages scrolled different amounts',
        );

        // Navigate away so /nested-b saves its positions
        c.read(isNavigatingProvider.notifier).set(true);
        router.go('/other');
        await tester.pumpAndSettle();

        // Verify history has both pages' positions saved
        final hist = c.read(navHistoryProvider);
        // entries: [/, /nested, /nested-b, /other]
        final nestedEntry = hist.entries[1];
        final nestedBEntry = hist.entries[2];

        expect(nestedEntry.path, '/nested');
        expect(nestedBEntry.path, '/nested-b');

        expect(
          nestedEntry.scrollPositions['nest_1'],
          moreOrLessEquals(nestedNest1, epsilon: 1.0),
          reason: '/nested nest_1 position must be saved correctly',
        );
        expect(
          nestedBEntry.scrollPositions['nest_1'],
          moreOrLessEquals(nestedBNest1, epsilon: 1.0),
          reason: '/nested-b nest_1 position must be saved independently',
        );

        // UNDO back to /nested-b
        router.go('/nested-b');
        await tester.pumpAndSettle();
        expect(
          _innerOffset(tester, const ValueKey('nest_1_list')),
          moreOrLessEquals(nestedBNest1, epsilon: 1.0),
          reason: '/nested-b nest_1 must be restored on UNDO',
        );

        // UNDO to /nested
        router.go('/nested');
        await tester.pumpAndSettle();
        expect(
          _innerOffset(tester, const ValueKey('nest_1_list')),
          moreOrLessEquals(nestedNest1, epsilon: 1.0),
          reason: '/nested nest_1 must be restored independently on UNDO',
        );
      },
    );
  });

  // ---------------------------------------------------------------------------
  // FLOW 7: UNDO/REDO cycle preserves nested scroll across multiple trips
  // ---------------------------------------------------------------------------

  group('Flow 7 — UNDO/REDO cycle preserves nested scroll state', () {
    testWidgets(
      'multiple UNDO/REDO trips all restore the same inner scroll positions',
      (tester) async {
        tester.view.physicalSize = const Size(800, 1200);
        tester.view.devicePixelRatio = 1.0;
        addTearDown(tester.view.resetPhysicalSize);

        final c = _makeContainer();
        final router = _buildRouter(c);

        await tester.pumpWidget(_buildApp(router, c));
        await tester.pumpAndSettle();

        // Scroll nested page
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
          const Offset(0, -120),
        );
        await tester.pumpAndSettle();
        final origNest1 = _innerOffset(tester, const ValueKey('nest_1_list'));
        final origNest2 = _innerOffset(tester, const ValueKey('nest_2_list'));

        // Leave
        c.read(isNavigatingProvider.notifier).set(true);
        router.go('/other');
        await tester.pumpAndSettle();

        // First UNDO
        router.go('/nested');
        await tester.pumpAndSettle();
        expect(
          _innerOffset(tester, const ValueKey('nest_1_list')),
          moreOrLessEquals(origNest1, epsilon: 1.0),
          reason: '1st UNDO — nest_1 restored',
        );
        expect(
          _innerOffset(tester, const ValueKey('nest_2_list')),
          moreOrLessEquals(origNest2, epsilon: 1.0),
          reason: '1st UNDO — nest_2 restored',
        );

        // REDO to /other
        router.go('/other');
        await tester.pumpAndSettle();

        // Second UNDO back to /nested
        router.go('/nested');
        await tester.pumpAndSettle();
        expect(
          _innerOffset(tester, const ValueKey('nest_1_list')),
          moreOrLessEquals(origNest1, epsilon: 1.0),
          reason: '2nd UNDO — nest_1 still restored correctly',
        );
        expect(
          _innerOffset(tester, const ValueKey('nest_2_list')),
          moreOrLessEquals(origNest2, epsilon: 1.0),
          reason: '2nd UNDO — nest_2 still restored correctly',
        );
      },
    );
  });
}

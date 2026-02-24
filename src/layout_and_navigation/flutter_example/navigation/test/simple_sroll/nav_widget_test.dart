// ignore_for_file: depend_on_referenced_packages

// =============================================================================
// NAVIGATION — WIDGET TESTS
// =============================================================================
//
// These tests mount real GoRouter + real LayoutManager + real NavHistory
// to catch problems that unit tests cannot detect:
//   - Router resolves routes correctly (no ErrorPage for valid paths)
//   - History provider is updated correctly by navigation actions
//   - Scroll positions are SAVED when leaving a page (in dispose)
//   - Scroll positions are RESTORED when returning via UNDO (addPostFrameCallback)
//
// WHY STUB PAGES WITH REAL LAYOUTMANAGER:
// Stub pages use plain widget bodies but are wrapped in the real LayoutManager
// so that the full scroll lifecycle (init → scroll → dispose → restore) is tested.
// Real production pages (HomePage, PanoramaPage) are NOT used because they have
// heavy dependencies (images, panorama engine) that don't work in widget tests.
//
// KEY TIMING (matching production):
// - redirect defers push/undoRedo via addPostFrameCallback
// - LayoutManager.initState defers scroll read via addPostFrameCallback
// - LayoutManager.dispose defers scroll save via addPostFrameCallback
// Therefore: use tester.pumpAndSettle() to drain ALL pending callbacks.
//
// Run with:
//   flutter test lib/navigation/test/nav_widget_test.dart
//   flutter test lib/navigation/test/
//
// =============================================================================

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:go_router/go_router.dart';
import 'package:grande_panorama_ar/layout/layout_manager.dart';
import 'package:grande_panorama_ar/layout/layout_slots.dart';
import 'package:grande_panorama_ar/navigation/histConfig/history_provider.dart';
import 'package:grande_panorama_ar/navigation/histConfig/route_observer.dart';
import 'package:grande_panorama_ar/navigation/navConfig/current_route_provider.dart';
import 'package:grande_panorama_ar/navigation/histConfig/is_navigating_provider.dart';

// =============================================================================
// STUB PAGES
//
// Each page is wrapped in a real LayoutManager with a scrollable body so that
// the full scroll lifecycle (ScrollRegistry init → scroll → save → restore) is
// exercised exactly as in production.
//
// The body content is intentionally tall so scroll tests can programmatically
// scroll to a meaningful offset.
// =============================================================================

/// A scrollable stub page backed by the real LayoutManager.
/// [label] is rendered as a unique Text widget for find.text() assertions.
class _ScrollablePage extends StatelessWidget {
  final String label;

  const _ScrollablePage({required this.label});

  @override
  Widget build(BuildContext context) {
    return LayoutManager(
      slots: LayoutSlots(
        body: _TallBody(label: label),
        scrollable: true,
        safeArea: false,
      ),
    );
  }
}

/// A tall body widget (>2000px) so scrolling has meaningful range.
class _TallBody extends StatelessWidget {
  final String label;

  const _TallBody({required this.label});

  @override
  Widget build(BuildContext context) {
    return Column(
      children: [
        Text(label),
        // Generate enough content to make the page scrollable
        ...List.generate(
          50,
          (i) => Container(
            height: 60,
            alignment: Alignment.center,
            child: Text('$label item $i'),
          ),
        ),
      ],
    );
  }
}

/// A non-scrollable stub page (for pages where scroll doesn't matter).
class _SimplePage extends StatelessWidget {
  final String label;

  const _SimplePage({required this.label});

  @override
  Widget build(BuildContext context) {
    return Scaffold(body: Center(child: Text(label)));
  }
}

// =============================================================================
// TEST ROUTER
//
// Mirrors production createRouter() exactly: same redirect logic with
// addPostFrameCallback deferral, same NavObserver, stub routes instead of real.
// =============================================================================

GoRouter _buildTestRouter(ProviderContainer container) {
  return GoRouter(
    initialLocation: '/',
    redirect: (context, state) {
      final path = state.uri.toString();
      final isNavigating = container
          .read(isNavigatingProvider.notifier)
          .consume();

      // Mirror production: defer to addPostFrameCallback so Riverpod mutations
      // never fire during the widget-build phase.
      WidgetsBinding.instance.addPostFrameCallback((_) {
        final notifier = container.read(navHistoryProvider.notifier);
        final currentIndex = container.read(navHistoryProvider).currentIndex;
        if (currentIndex < 0) {
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
        builder: (_, s) => const _ScrollablePage(label: '__home__'),
      ),
      GoRoute(
        path: '/page-a',
        builder: (_, s) => const _ScrollablePage(label: '__page-a__'),
      ),
      GoRoute(
        path: '/page-b',
        builder: (_, s) => const _ScrollablePage(label: '__page-b__'),
      ),
      GoRoute(
        path: '/simple',
        builder: (_, s) => const _SimplePage(label: '__simple__'),
        routes: [
          GoRoute(
            path: 'child',
            builder: (_, s) => const _SimplePage(label: '__simple-child__'),
          ),
        ],
      ),
    ],
    errorBuilder: (context, state) {
      return Scaffold(
        body: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            const Icon(Icons.error_outline),
            Text('ERROR: ${state.uri}'),
          ],
        ),
      );
    },
  );
}

Widget _buildTestApp(GoRouter router, ProviderContainer container) {
  return UncontrolledProviderScope(
    container: container,
    child: MaterialApp.router(routerConfig: router, theme: ThemeData.light()),
  );
}

/// Creates a [ProviderContainer] and registers a teardown that disposes it
/// synchronously, BEFORE Flutter's widget-tree teardown frame runs.
///
/// Why this matters:
/// LayoutManager.dispose() registers an addPostFrameCallback that calls
/// saveScrollPositionsAt → state= → notifies canGoBack/canGoForward watchers
/// → ProviderScheduler._scheduleTask() → Timer(Duration.zero, ...) [fake timer].
///
/// This fake timer outlives the test and causes flutter_test's _verifyInvariants
/// to throw "A Timer is still pending".
///
/// Fix: Riverpod's ProviderScheduler._scheduleTask() guards:
///   `if (_pendingTaskCompleter != null || _disposed) return;`
/// So if the container is already disposed when the callback fires, NO timer
/// is created. We dispose synchronously in addTearDown so it runs before
/// the next test's pumpWidget warmup frame fires the pending callback.
ProviderContainer _makeContainer(WidgetTester tester) {
  final container = ProviderContainer();
  addTearDown(container.dispose);
  return container;
}

// =============================================================================
// TESTS
// =============================================================================

void main() {
  // ---------------------------------------------------------------------------
  // GROUP 1: Routing — correct page is shown
  // ---------------------------------------------------------------------------

  group('Routing — correct page is shown', () {
    testWidgets('initial route / shows home page, not ErrorPage', (
      WidgetTester tester,
    ) async {
      final container = _makeContainer(tester);
      final router = _buildTestRouter(container);

      await tester.pumpWidget(_buildTestApp(router, container));
      await tester.pumpAndSettle();

      expect(
        find.text('__home__'),
        findsWidgets,
        reason: 'Home content must be visible for route /',
      );
      expect(
        find.byIcon(Icons.error_outline),
        findsNothing,
        reason: 'ErrorPage must not fire for valid route /',
      );
    });

    testWidgets('navigating to /page-a shows page-a, not ErrorPage', (
      WidgetTester tester,
    ) async {
      final container = _makeContainer(tester);
      final router = _buildTestRouter(container);

      await tester.pumpWidget(_buildTestApp(router, container));
      await tester.pumpAndSettle();

      container.read(isNavigatingProvider.notifier).set(true);
      router.go('/page-a');
      await tester.pumpAndSettle();

      expect(find.text('__page-a__'), findsWidgets);
      expect(find.byIcon(Icons.error_outline), findsNothing);
    });

    testWidgets('unknown path shows ErrorPage', (WidgetTester tester) async {
      final container = _makeContainer(tester);
      final router = _buildTestRouter(container);

      await tester.pumpWidget(_buildTestApp(router, container));
      await tester.pumpAndSettle();

      router.go('/does-not-exist');
      await tester.pumpAndSettle();

      expect(
        find.byIcon(Icons.error_outline),
        findsOneWidget,
        reason: 'ErrorPage must appear for unknown route',
      );
    });

    testWidgets('nested child route /simple/child resolves without ErrorPage', (
      WidgetTester tester,
    ) async {
      final container = _makeContainer(tester);
      final router = _buildTestRouter(container);

      await tester.pumpWidget(_buildTestApp(router, container));
      await tester.pumpAndSettle();

      container.read(isNavigatingProvider.notifier).set(true);
      router.go('/simple/child');
      await tester.pumpAndSettle();

      expect(
        find.text('__simple-child__'),
        findsOneWidget,
        reason: 'Relative path nested route must resolve correctly',
      );
      expect(find.byIcon(Icons.error_outline), findsNothing);
    });
  });

  // ---------------------------------------------------------------------------
  // GROUP 2: History provider — driven by navigation
  // ---------------------------------------------------------------------------

  group('History provider — state driven by router', () {
    testWidgets('initial navigation creates first entry', (
      WidgetTester tester,
    ) async {
      final container = _makeContainer(tester);
      final router = _buildTestRouter(container);

      await tester.pumpWidget(_buildTestApp(router, container));
      await tester.pumpAndSettle();

      final history = container.read(navHistoryProvider);
      expect(history.entries.length, 1);
      expect(history.current?.path, '/');
      expect(container.read(canGoBackProvider), false);
      expect(container.read(canGoForwardProvider), false);
    });

    testWidgets('isNavigating=true adds new history entry (push)', (
      WidgetTester tester,
    ) async {
      final container = _makeContainer(tester);
      final router = _buildTestRouter(container);

      await tester.pumpWidget(_buildTestApp(router, container));
      await tester.pumpAndSettle();

      container.read(isNavigatingProvider.notifier).set(true);
      router.go('/page-a');
      await tester.pumpAndSettle();

      final history = container.read(navHistoryProvider);
      expect(history.entries.length, 2, reason: 'push adds new entry');
      expect(history.entries[0].path, '/');
      expect(history.entries[1].path, '/page-a');
      expect(history.currentIndex, 1);
      expect(container.read(canGoBackProvider), true);
      expect(container.read(canGoForwardProvider), false);
    });

    testWidgets('no extra flag calls undoRedo(), does NOT add new entry', (
      WidgetTester tester,
    ) async {
      final container = _makeContainer(tester);
      final router = _buildTestRouter(container);

      await tester.pumpWidget(_buildTestApp(router, container));
      await tester.pumpAndSettle();

      container.read(isNavigatingProvider.notifier).set(true);
      router.go('/page-a');
      await tester.pumpAndSettle();

      // Simulate browser back (no extra)
      router.go('/');
      await tester.pumpAndSettle();

      final history = container.read(navHistoryProvider);
      expect(
        history.entries.length,
        2,
        reason: 'undoRedo does not add entries',
      );
      expect(history.currentIndex, 0);
    });

    testWidgets('canGoBack / canGoForward reflect navigation correctly', (
      WidgetTester tester,
    ) async {
      final container = _makeContainer(tester);
      final router = _buildTestRouter(container);

      await tester.pumpWidget(_buildTestApp(router, container));
      await tester.pumpAndSettle();
      expect(container.read(canGoBackProvider), false);
      expect(container.read(canGoForwardProvider), false);

      container.read(isNavigatingProvider.notifier).set(true);
      router.go('/page-a');
      await tester.pumpAndSettle();
      expect(container.read(canGoBackProvider), true);
      expect(container.read(canGoForwardProvider), false);

      router.go('/');
      await tester.pumpAndSettle();
      expect(container.read(canGoBackProvider), false);
      expect(container.read(canGoForwardProvider), true);
    });

    testWidgets('undo then push trims forward history', (
      WidgetTester tester,
    ) async {
      final container = _makeContainer(tester);
      final router = _buildTestRouter(container);

      await tester.pumpWidget(_buildTestApp(router, container));
      await tester.pumpAndSettle();

      container.read(isNavigatingProvider.notifier).set(true);
      router.go('/page-a');
      await tester.pumpAndSettle();
      container.read(isNavigatingProvider.notifier).set(true);
      router.go('/page-b');
      await tester.pumpAndSettle();
      // History: [/, /page-a, /page-b], index=2

      router.go('/page-a');
      await tester.pumpAndSettle();
      expect(container.read(navHistoryProvider).currentIndex, 1);

      container.read(isNavigatingProvider.notifier).set(true);
      router.go('/page-b');
      await tester.pumpAndSettle();

      final history = container.read(navHistoryProvider);
      final paths = history.entries.map((e) => e.path).toList();
      expect(paths, ['/', '/page-a', '/page-b']);
      expect(history.currentIndex, 2);
      expect(container.read(canGoForwardProvider), false);
    });
  });

  // ---------------------------------------------------------------------------
  // GROUP 3: Scroll save & restore — the core feature
  //
  // FLOW UNDER TEST:
  //   1. Home → page-a (push)
  //   2. Scroll page-a down to offset 300
  //   3. page-a → home (push)
  //   4. page-a's LayoutManager.dispose() fires → saves scroll 300 to page-a entry
  //   5. UNDO → back to page-a (undoRedo)
  //   6. page-a's LayoutManager.initState postFrameCallback fires →
  //      reads saved positions from history → calls restorePositions(300) →
  //      jumpTo(300) on the scroll controller
  //   7. Assert: scroll offset = 300
  // ---------------------------------------------------------------------------

  group('Scroll save and restore', () {
    testWidgets('scroll position on page-a is saved when navigating away', (
      WidgetTester tester,
    ) async {
      tester.view.physicalSize = const Size(800, 1200);
      tester.view.devicePixelRatio = 1.0;
      addTearDown(() => tester.view.resetPhysicalSize());

      final container = _makeContainer(tester);
      final router = _buildTestRouter(container);

      await tester.pumpWidget(_buildTestApp(router, container));
      await tester.pumpAndSettle();

      // Navigate to page-a
      container.read(isNavigatingProvider.notifier).set(true);
      router.go('/page-a');
      await tester.pumpAndSettle();

      // Scroll down on page-a
      await tester.drag(
        find.byType(SingleChildScrollView),
        const Offset(0, -300),
      );
      await tester.pumpAndSettle();

      // Navigate away (back to home)
      container.read(isNavigatingProvider.notifier).set(true);
      router.go('/');
      await tester.pumpAndSettle();

      // Verify: page-a's scroll position is saved in history entry 1
      final history = container.read(navHistoryProvider);
      final pageAEntry = history.entries[1]; // page-a is at index 1
      expect(pageAEntry.path, '/page-a');
      expect(
        pageAEntry.scrollPositions['page'],
        greaterThan(0),
        reason: 'Scroll position must be saved when leaving page-a',
      );
    });

    testWidgets('scroll position is restored when returning via UNDO', (
      WidgetTester tester,
    ) async {
      tester.view.physicalSize = const Size(800, 1200);
      tester.view.devicePixelRatio = 1.0;
      addTearDown(() => tester.view.resetPhysicalSize());

      final container = _makeContainer(tester);
      final router = _buildTestRouter(container);

      await tester.pumpWidget(_buildTestApp(router, container));
      await tester.pumpAndSettle();

      // Navigate to page-a
      container.read(isNavigatingProvider.notifier).set(true);
      router.go('/page-a');
      await tester.pumpAndSettle();

      // Scroll down on page-a
      await tester.drag(
        find.byType(SingleChildScrollView),
        const Offset(0, -300),
      );
      await tester.pumpAndSettle();

      // Capture the actual scroll offset achieved
      final scrollFinder = find.byType(SingleChildScrollView);
      final scrollWidget = tester.widget<SingleChildScrollView>(scrollFinder);
      final scrolledOffset = scrollWidget.controller!.offset;
      expect(
        scrolledOffset,
        greaterThan(0),
        reason: 'Must have actually scrolled',
      );

      // Navigate to home (push)
      container.read(isNavigatingProvider.notifier).set(true);
      router.go('/');
      await tester.pumpAndSettle();

      // UNDO — go back to page-a (undoRedo, no extra)
      router.go('/page-a');
      await tester.pumpAndSettle();

      // Verify: scroll position is restored
      final restoredScrollFinder = find.byType(SingleChildScrollView);
      final restoredWidget = tester.widget<SingleChildScrollView>(
        restoredScrollFinder,
      );
      expect(
        restoredWidget.controller!.offset,
        moreOrLessEquals(scrolledOffset, epsilon: 1.0),
        reason: 'Scroll position must be restored after UNDO',
      );
    });

    testWidgets('scroll is NOT restored when navigating fresh (new push)', (
      WidgetTester tester,
    ) async {
      tester.view.physicalSize = const Size(800, 1200);
      tester.view.devicePixelRatio = 1.0;
      addTearDown(() => tester.view.resetPhysicalSize());

      final container = _makeContainer(tester);
      final router = _buildTestRouter(container);

      await tester.pumpWidget(_buildTestApp(router, container));
      await tester.pumpAndSettle();

      // Visit page-a, scroll, leave
      container.read(isNavigatingProvider.notifier).set(true);
      router.go('/page-a');
      await tester.pumpAndSettle();
      await tester.drag(
        find.byType(SingleChildScrollView),
        const Offset(0, -300),
      );
      await tester.pumpAndSettle();
      container.read(isNavigatingProvider.notifier).set(true);
      router.go('/');
      await tester.pumpAndSettle();

      // Navigate to page-b then to page-a (new push, not UNDO)
      container.read(isNavigatingProvider.notifier).set(true);
      router.go('/page-b');
      await tester.pumpAndSettle();
      container.read(isNavigatingProvider.notifier).set(true);
      router.go('/page-a');
      await tester.pumpAndSettle();

      // This is a brand-new visit to page-a — scroll must start at 0
      final scrollFinder = find.byType(SingleChildScrollView);
      final scrollWidget = tester.widget<SingleChildScrollView>(scrollFinder);
      expect(
        scrollWidget.controller!.offset,
        moreOrLessEquals(0.0, epsilon: 1.0),
        reason: 'Fresh navigation must start at scroll offset 0',
      );
    });

    testWidgets('multiple scroll-and-return cycles work correctly', (
      WidgetTester tester,
    ) async {
      tester.view.physicalSize = const Size(800, 1200);
      tester.view.devicePixelRatio = 1.0;
      addTearDown(() => tester.view.resetPhysicalSize());

      final container = _makeContainer(tester);
      final router = _buildTestRouter(container);

      await tester.pumpWidget(_buildTestApp(router, container));
      await tester.pumpAndSettle();

      // Visit page-a, scroll to ~300
      container.read(isNavigatingProvider.notifier).set(true);
      router.go('/page-a');
      await tester.pumpAndSettle();
      await tester.drag(
        find.byType(SingleChildScrollView),
        const Offset(0, -300),
      );
      await tester.pumpAndSettle();
      final offsetA = tester
          .widget<SingleChildScrollView>(find.byType(SingleChildScrollView))
          .controller!
          .offset;

      // Go home
      container.read(isNavigatingProvider.notifier).set(true);
      router.go('/');
      await tester.pumpAndSettle();

      // UNDO to page-a
      router.go('/page-a');
      await tester.pumpAndSettle();

      final restoredA = tester
          .widget<SingleChildScrollView>(find.byType(SingleChildScrollView))
          .controller!
          .offset;
      expect(
        restoredA,
        moreOrLessEquals(offsetA, epsilon: 1.0),
        reason: 'First UNDO must restore scroll',
      );

      // REDO back to home
      router.go('/');
      await tester.pumpAndSettle();

      // UNDO again to page-a
      router.go('/page-a');
      await tester.pumpAndSettle();

      final restoredA2 = tester
          .widget<SingleChildScrollView>(find.byType(SingleChildScrollView))
          .controller!
          .offset;
      expect(
        restoredA2,
        moreOrLessEquals(offsetA, epsilon: 1.0),
        reason: 'Second UNDO must also restore scroll',
      );
    });
  });
}

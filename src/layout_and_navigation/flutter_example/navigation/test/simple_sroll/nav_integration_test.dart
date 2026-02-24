// ignore_for_file: depend_on_referenced_packages

// =============================================================================
// NAVIGATION — INTEGRATION TESTS
// =============================================================================
//
// These tests run the full navigation stack end-to-end in a widget environment:
//   GoRouter (same config as production redirect/observer logic)
//   + real Riverpod provider graph (ProviderContainer)
//   + real LayoutManager (scroll lifecycle)
//   + real NavObserver (currentRouteProvider tracking)
//
// Pages are intentionally stubbed (no real images / 3D assets), but the
// entire routing + history + scroll + route-tracking machinery is REAL.
//
// WHAT THESE TESTS PROVE:
//   1. End-to-end user flow: launch → navigate → scroll → UNDO → verify restored
//   2. currentRouteProvider tracks the active route in sync with navigation
//   3. canGoBack / canGoForward drive navigation buttons correctly
//   4. History is consistent after complex UNDO/REDO/push sequences
//   5. Scroll state survives multiple UNDO/REDO round-trips
//   6. Error page appears for unknown routes
//
// DIFFERENCE FROM UNIT TESTS:
//   Unit tests (nav_history_test.dart) call notifier methods directly.
//   These tests drive navigation via router.go(), which triggers the FULL
//   redirect → postFrameCallback → push/undoRedo chain, exactly as the
//   browser does.
//
// DIFFERENCE FROM WIDGET TESTS (nav_widget_test.dart):
//   Widget tests assert individual pieces (scroll saved, entry count, etc.).
//   Integration tests assert COMPLETE USER FLOWS from start to finish.
//
// Run with:
//   flutter test lib/navigation/test/nav_integration_test.dart
//   flutter test lib/navigation/test/
//
// =============================================================================

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:go_router/go_router.dart';
import 'package:grande_panorama_ar/layout/layout_manager.dart';
import 'package:grande_panorama_ar/layout/layout_slots.dart';
import 'package:grande_panorama_ar/navigation/navConfig/current_route_provider.dart';
import 'package:grande_panorama_ar/navigation/histConfig/history_provider.dart';
import 'package:grande_panorama_ar/navigation/histConfig/route_observer.dart';
import 'package:grande_panorama_ar/navigation/histConfig/is_navigating_provider.dart';

// =============================================================================
// STUB PAGES
// =============================================================================
// Scrollable stub with LayoutManager — exercises the full scroll lifecycle.

class _Page extends StatelessWidget {
  final String label;
  const _Page(this.label);

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

class _TallBody extends StatelessWidget {
  final String label;
  const _TallBody({required this.label});

  @override
  Widget build(BuildContext context) {
    return Column(
      children: [
        Text(label),
        ...List.generate(
          50,
          (i) => SizedBox(height: 60, child: Text('$label item $i')),
        ),
      ],
    );
  }
}

// =============================================================================
// TEST APP FACTORY
//
// Mirrors production createRouter() logic exactly:
//   - redirect defers push/undoRedo via addPostFrameCallback
//   - NavObserver updates currentRouteProvider
//   - Same extra flag semantics: isNavigating = push, otherwise = undoRedo
// =============================================================================

/// Builds the real GoRouter with redirect + NavObserver, wired to [container].
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
      GoRoute(path: '/', builder: (_, s) => const _Page('__home__')),
      GoRoute(path: '/explore', builder: (_, s) => const _Page('__explore__')),
      GoRoute(path: '/gallery', builder: (_, s) => const _Page('__gallery__')),
      GoRoute(path: '/about', builder: (_, s) => const _Page('__about__')),
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

/// Wraps a router in ProviderScope + MaterialApp.router for widget tests.
Widget _buildApp(GoRouter router, ProviderContainer container) {
  return UncontrolledProviderScope(
    container: container,
    child: MaterialApp.router(routerConfig: router, theme: ThemeData.light()),
  );
}

/// Creates a container with teardown for clean isolation.
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
  // FLOW 1: Fresh launch → first page is home
  // ---------------------------------------------------------------------------

  group('Flow 1 — App launch', () {
    testWidgets('home page is shown and history has one entry', (tester) async {
      final c = _makeContainer();
      final router = _buildRouter(c);
      await tester.pumpWidget(_buildApp(router, c));
      await tester.pumpAndSettle();

      expect(find.text('__home__'), findsWidgets);
      expect(c.read(navHistoryProvider).entries.length, 1);
      expect(c.read(navHistoryProvider).current?.path, '/');
      expect(c.read(canGoBackProvider), false);
      expect(c.read(canGoForwardProvider), false);
    });

    testWidgets('currentRouteProvider reflects home on launch', (tester) async {
      final c = _makeContainer();
      final router = _buildRouter(c);
      await tester.pumpWidget(_buildApp(router, c));
      await tester.pumpAndSettle();

      expect(c.read(currentRouteProvider), '/');
    });
  });

  // ---------------------------------------------------------------------------
  // FLOW 2: Navigate forward → history grows, canGoBack enables
  // ---------------------------------------------------------------------------

  group('Flow 2 — Forward navigation', () {
    testWidgets('navigate home → explore → gallery builds correct history', (
      tester,
    ) async {
      final c = _makeContainer();
      final router = _buildRouter(c);
      await tester.pumpWidget(_buildApp(router, c));
      await tester.pumpAndSettle();

      c.read(isNavigatingProvider.notifier).set(true);
      router.go('/explore');
      await tester.pumpAndSettle();

      c.read(isNavigatingProvider.notifier).set(true);
      router.go('/gallery');
      await tester.pumpAndSettle();

      final hist = c.read(navHistoryProvider);
      expect(hist.entries.map((e) => e.path).toList(), [
        '/',
        '/explore',
        '/gallery',
      ]);
      expect(hist.currentIndex, 2);
      expect(c.read(canGoBackProvider), true);
      expect(c.read(canGoForwardProvider), false);
    });

    testWidgets('currentRouteProvider tracks each navigation', (tester) async {
      final c = _makeContainer();
      final router = _buildRouter(c);
      await tester.pumpWidget(_buildApp(router, c));
      await tester.pumpAndSettle();
      expect(c.read(currentRouteProvider), '/');

      c.read(isNavigatingProvider.notifier).set(true);
      router.go('/explore');
      await tester.pumpAndSettle();
      expect(c.read(currentRouteProvider), '/explore');

      c.read(isNavigatingProvider.notifier).set(true);
      router.go('/gallery');
      await tester.pumpAndSettle();
      expect(c.read(currentRouteProvider), '/gallery');
    });
  });

  // ---------------------------------------------------------------------------
  // FLOW 3: UNDO → back in history, no new entries, canGoForward enables
  // ---------------------------------------------------------------------------

  group('Flow 3 — UNDO (browser back)', () {
    testWidgets('UNDO does not add new entries and moves currentIndex back', (
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
      router.go('/explore');
      await tester.pumpAndSettle();

      c.read(isNavigatingProvider.notifier).set(true);
      router.go('/gallery');
      await tester.pumpAndSettle();
      // history: [/, /explore, /gallery] index=2

      // UNDO to /explore
      router.go('/explore'); // no extra = undoRedo
      await tester.pumpAndSettle();

      final hist = c.read(navHistoryProvider);
      expect(hist.entries.length, 3, reason: 'UNDO must NOT add entries');
      expect(hist.currentIndex, 1);
      expect(hist.current?.path, '/explore');
      expect(c.read(canGoBackProvider), true);
      expect(c.read(canGoForwardProvider), true);
    });

    testWidgets(
      'UNDO all the way to start: canGoBack=false, canGoForward=true',
      (tester) async {
        final c = _makeContainer();
        final router = _buildRouter(c);
        await tester.pumpWidget(_buildApp(router, c));
        await tester.pumpAndSettle();

        c.read(isNavigatingProvider.notifier).set(true);
        router.go('/explore');
        await tester.pumpAndSettle();

        // UNDO back to /
        router.go('/'); // no extra
        await tester.pumpAndSettle();

        expect(c.read(canGoBackProvider), false);
        expect(c.read(canGoForwardProvider), true);
        expect(c.read(navHistoryProvider).currentIndex, 0);
      },
    );
  });

  // ---------------------------------------------------------------------------
  // FLOW 4: Scroll → UNDO → scroll restored  (the core product feature)
  // ---------------------------------------------------------------------------

  group('Flow 4 — Scroll save and UNDO restore (core feature)', () {
    testWidgets(
      'user scrolls on /explore, navigates away, UNDOs, scroll is restored',
      (tester) async {
        tester.view.physicalSize = const Size(800, 1200);
        tester.view.devicePixelRatio = 1.0;
        addTearDown(tester.view.resetPhysicalSize);

        final c = _makeContainer();
        final router = _buildRouter(c);
        await tester.pumpWidget(_buildApp(router, c));
        await tester.pumpAndSettle();

        // Step 1: Navigate to /explore
        c.read(isNavigatingProvider.notifier).set(true);
        router.go('/explore');
        await tester.pumpAndSettle();
        expect(find.text('__explore__'), findsWidgets);

        // Step 2: Scroll down on /explore
        await tester.drag(
          find.byType(SingleChildScrollView),
          const Offset(0, -300),
        );
        await tester.pumpAndSettle();

        final scrolledOffset = tester
            .widget<SingleChildScrollView>(find.byType(SingleChildScrollView))
            .controller!
            .offset;
        expect(
          scrolledOffset,
          greaterThan(0),
          reason: 'Must have actually scrolled',
        );

        // Step 3: Navigate to /gallery (forward)
        c.read(isNavigatingProvider.notifier).set(true);
        router.go('/gallery');
        await tester.pumpAndSettle();
        expect(find.text('__gallery__'), findsWidgets);

        // Verify: /explore entry has scroll saved
        final exploreEntry = c.read(navHistoryProvider).entries[1];
        expect(exploreEntry.path, '/explore');
        expect(
          exploreEntry.scrollPositions['page'],
          greaterThan(0),
          reason: 'Scroll must be saved when leaving /explore',
        );

        // Step 4: UNDO back to /explore
        router.go('/explore'); // no extra = undoRedo
        await tester.pumpAndSettle();
        expect(find.text('__explore__'), findsWidgets);

        // Step 5: Assert scroll restored
        final restoredOffset = tester
            .widget<SingleChildScrollView>(find.byType(SingleChildScrollView))
            .controller!
            .offset;
        expect(
          restoredOffset,
          moreOrLessEquals(scrolledOffset, epsilon: 1.0),
          reason: 'Scroll position must be restored after UNDO',
        );
      },
    );

    testWidgets(
      'fresh navigation to a previously-visited page does NOT restore scroll',
      (tester) async {
        tester.view.physicalSize = const Size(800, 1200);
        tester.view.devicePixelRatio = 1.0;
        addTearDown(tester.view.resetPhysicalSize);

        final c = _makeContainer();
        final router = _buildRouter(c);
        await tester.pumpWidget(_buildApp(router, c));
        await tester.pumpAndSettle();

        // Visit /explore, scroll, leave
        c.read(isNavigatingProvider.notifier).set(true);
        router.go('/explore');
        await tester.pumpAndSettle();
        await tester.drag(
          find.byType(SingleChildScrollView),
          const Offset(0, -300),
        );
        await tester.pumpAndSettle();
        c.read(isNavigatingProvider.notifier).set(true);
        router.go('/');
        await tester.pumpAndSettle();

        // Fresh push to /explore again (not UNDO)
        c.read(isNavigatingProvider.notifier).set(true);
        router.go('/explore');
        await tester.pumpAndSettle();

        // Scroll must be at 0 — this is a NEW visit, not UNDO
        final offset = tester
            .widget<SingleChildScrollView>(find.byType(SingleChildScrollView))
            .controller!
            .offset;
        expect(
          offset,
          moreOrLessEquals(0.0, epsilon: 1.0),
          reason: 'Fresh navigation must start at scroll offset 0',
        );
      },
    );
  });

  // ---------------------------------------------------------------------------
  // FLOW 5: UNDO then push → forward history is trimmed
  // ---------------------------------------------------------------------------

  group('Flow 5 — UNDO then push trims forward history', () {
    testWidgets(
      'browsing /explore → /gallery, then UNDO to /explore, then push /about — /gallery removed',
      (tester) async {
        final c = _makeContainer();
        final router = _buildRouter(c);
        await tester.pumpWidget(_buildApp(router, c));
        await tester.pumpAndSettle();

        c.read(isNavigatingProvider.notifier).set(true);
        router.go('/explore');
        await tester.pumpAndSettle();
        c.read(isNavigatingProvider.notifier).set(true);
        router.go('/gallery');
        await tester.pumpAndSettle();
        // history: [/, /explore, /gallery], index=2

        // UNDO to /explore
        router.go('/explore');
        await tester.pumpAndSettle();
        expect(c.read(navHistoryProvider).currentIndex, 1);

        // Push new page from /explore
        c.read(isNavigatingProvider.notifier).set(true);
        router.go('/about');
        await tester.pumpAndSettle();

        final hist = c.read(navHistoryProvider);
        expect(
          hist.entries.map((e) => e.path).toList(),
          ['/', '/explore', '/about'],
          reason: '/gallery must be trimmed after push from UNDO position',
        );
        expect(hist.currentIndex, 2);
        expect(c.read(canGoForwardProvider), false);
      },
    );
  });

  // ---------------------------------------------------------------------------
  // FLOW 6: Error page for unknown route
  // ---------------------------------------------------------------------------

  group('Flow 6 — Error page', () {
    testWidgets('unknown route shows error page with error icon', (
      tester,
    ) async {
      final c = _makeContainer();
      final router = _buildRouter(c);
      await tester.pumpWidget(_buildApp(router, c));
      await tester.pumpAndSettle();

      router.go('/this-does-not-exist');
      await tester.pumpAndSettle();

      expect(
        find.byIcon(Icons.error_outline),
        findsOneWidget,
        reason: 'ErrorPage must appear for unknown route',
      );
    });
  });

  // ---------------------------------------------------------------------------
  // FLOW 7: Multi-step scroll and UNDO/REDO cycle
  // ---------------------------------------------------------------------------

  group('Flow 7 — Multi-page scroll state across full UNDO/REDO cycle', () {
    testWidgets(
      'home and explore each save their own scroll, both restored correctly',
      (tester) async {
        tester.view.physicalSize = const Size(800, 1200);
        tester.view.devicePixelRatio = 1.0;
        addTearDown(tester.view.resetPhysicalSize);

        final c = _makeContainer();
        final router = _buildRouter(c);
        await tester.pumpWidget(_buildApp(router, c));
        await tester.pumpAndSettle();

        // Scroll home a little
        await tester.drag(
          find.byType(SingleChildScrollView),
          const Offset(0, -100),
        );
        await tester.pumpAndSettle();
        final homeOffset = tester
            .widget<SingleChildScrollView>(find.byType(SingleChildScrollView))
            .controller!
            .offset;
        expect(homeOffset, greaterThan(0));

        // Navigate to /explore
        c.read(isNavigatingProvider.notifier).set(true);
        router.go('/explore');
        await tester.pumpAndSettle();

        // Scroll /explore more
        await tester.drag(
          find.byType(SingleChildScrollView),
          const Offset(0, -300),
        );
        await tester.pumpAndSettle();
        final exploreOffset = tester
            .widget<SingleChildScrollView>(find.byType(SingleChildScrollView))
            .controller!
            .offset;
        expect(exploreOffset, greaterThan(homeOffset));

        // Navigate to /gallery
        c.read(isNavigatingProvider.notifier).set(true);
        router.go('/gallery');
        await tester.pumpAndSettle();

        // Verify both home and explore have saved their positions
        final hist = c.read(navHistoryProvider);
        expect(
          hist.entries[0].scrollPositions['page'],
          moreOrLessEquals(homeOffset, epsilon: 1.0),
          reason: 'Home scroll must be saved',
        );
        expect(
          hist.entries[1].scrollPositions['page'],
          moreOrLessEquals(exploreOffset, epsilon: 1.0),
          reason: 'Explore scroll must be saved',
        );

        // UNDO to /explore
        router.go('/explore');
        await tester.pumpAndSettle();
        final restoredExplore = tester
            .widget<SingleChildScrollView>(find.byType(SingleChildScrollView))
            .controller!
            .offset;
        expect(
          restoredExplore,
          moreOrLessEquals(exploreOffset, epsilon: 1.0),
          reason: 'Explore scroll restored',
        );

        // UNDO to home
        router.go('/');
        await tester.pumpAndSettle();
        final restoredHome = tester
            .widget<SingleChildScrollView>(find.byType(SingleChildScrollView))
            .controller!
            .offset;
        expect(
          restoredHome,
          moreOrLessEquals(homeOffset, epsilon: 1.0),
          reason: 'Home scroll restored',
        );
      },
    );
  });

  // ---------------------------------------------------------------------------
  // FLOW 8: History consistency — canGoBack/Forward after complex sequences
  // ---------------------------------------------------------------------------

  group('Flow 8 — canGoBack/canGoForward consistency', () {
    testWidgets('UNDO/REDO cycle keeps buttons in correct enabled state', (
      tester,
    ) async {
      final c = _makeContainer();
      final router = _buildRouter(c);
      await tester.pumpWidget(_buildApp(router, c));
      await tester.pumpAndSettle();

      // Initial: both disabled
      expect(c.read(canGoBackProvider), false);
      expect(c.read(canGoForwardProvider), false);

      c.read(isNavigatingProvider.notifier).set(true);
      router.go('/explore');
      await tester.pumpAndSettle();
      expect(c.read(canGoBackProvider), true); // can go back to /
      expect(c.read(canGoForwardProvider), false);

      c.read(isNavigatingProvider.notifier).set(true);
      router.go('/gallery');
      await tester.pumpAndSettle();
      expect(c.read(canGoBackProvider), true);
      expect(c.read(canGoForwardProvider), false);

      // UNDO to /explore
      router.go('/explore');
      await tester.pumpAndSettle();
      expect(c.read(canGoBackProvider), true);
      expect(c.read(canGoForwardProvider), true); // /gallery is forward

      // UNDO to /
      router.go('/');
      await tester.pumpAndSettle();
      expect(c.read(canGoBackProvider), false); // at start
      expect(c.read(canGoForwardProvider), true);

      // REDO to /explore
      router.go('/explore');
      await tester.pumpAndSettle();
      expect(c.read(canGoBackProvider), true);
      expect(c.read(canGoForwardProvider), true);

      // REDO to /gallery
      router.go('/gallery');
      await tester.pumpAndSettle();
      expect(c.read(canGoBackProvider), true);
      expect(c.read(canGoForwardProvider), false); // back at end
    });

    testWidgets('currentRouteProvider is consistent with canGoBack/Forward', (
      tester,
    ) async {
      final c = _makeContainer();
      final router = _buildRouter(c);
      await tester.pumpWidget(_buildApp(router, c));
      await tester.pumpAndSettle();

      c.read(isNavigatingProvider.notifier).set(true);
      router.go('/explore');
      await tester.pumpAndSettle();
      expect(c.read(currentRouteProvider), '/explore');
      expect(c.read(canGoBackProvider), true);

      router.go('/');
      await tester.pumpAndSettle();
      expect(c.read(currentRouteProvider), '/');
      expect(c.read(canGoBackProvider), false);
      expect(c.read(canGoForwardProvider), true);
    });
  });
}

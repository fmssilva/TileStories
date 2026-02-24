// ignore_for_file: depend_on_referenced_packages

// =============================================================================
// ROUTE TRACKING — UNIT TESTS
// =============================================================================
//
// Tests for the two components responsible for tracking the currently active
// route so navigation widgets (tabs, hamburger) can highlight the active item:
//
//   currentRouteProvider / _CurrentRouteNotifier
//     — Starts at '/', updates on every route change, reactive to watchers.
//
//   NavObserver
//     — Wraps NavigatorObserver callbacks (didPush, didPop, didReplace,
//       didRemove), deduplicates them, and calls onRouteChanged exactly once
//       per logical navigation via addPostFrameCallback.
//
// Both are tested WITHOUT mocking:
//   - currentRouteProvider is driven directly via its notifier.
//   - NavObserver is driven by calling its NavigatorObserver override methods
//     with lightweight fake Route objects.
//
// Run with:
//   flutter test lib/navigation/test/nav_route_tracking_test.dart
//   flutter test lib/navigation/test/
//
// =============================================================================

import 'package:flutter/widgets.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:grande_panorama_ar/navigation/navConfig/current_route_provider.dart';
import 'package:grande_panorama_ar/navigation/histConfig/route_observer.dart';

// =============================================================================
// HELPERS
// =============================================================================

/// Minimal fake [Route] that provides a [RouteSettings] with the given name.
/// Used to simulate Navigator callbacks without a real Navigator.
class _FakeRoute extends Route<void> {
  _FakeRoute(String name) : super(settings: RouteSettings(name: name));
}

/// Convenience: a route whose settings.name is null (unnamed route).
class _UnnamedRoute extends Route<void> {
  _UnnamedRoute() : super();
}

// =============================================================================
// TESTS
// =============================================================================

void main() {
  // ---------------------------------------------------------------------------
  // GROUP 1: currentRouteProvider
  //
  // Tests the Riverpod provider that stores the currently visible route path.
  // ---------------------------------------------------------------------------

  group('currentRouteProvider', () {
    test('initial state is "/"', () {
      final c = ProviderContainer();
      addTearDown(c.dispose);

      expect(c.read(currentRouteProvider), '/');
    });

    test('update() changes the state', () {
      final c = ProviderContainer();
      addTearDown(c.dispose);

      c.read(currentRouteProvider.notifier).update('/explore');
      expect(c.read(currentRouteProvider), '/explore');
    });

    test('update() with same path is idempotent', () {
      final c = ProviderContainer();
      addTearDown(c.dispose);

      c.read(currentRouteProvider.notifier).update('/about');
      c.read(currentRouteProvider.notifier).update('/about'); // second call
      expect(c.read(currentRouteProvider), '/about');
    });

    test('update() sequence tracks every change', () {
      final c = ProviderContainer();
      addTearDown(c.dispose);

      final notifier = c.read(currentRouteProvider.notifier);
      notifier.update('/home');
      expect(c.read(currentRouteProvider), '/home');

      notifier.update('/gallery');
      expect(c.read(currentRouteProvider), '/gallery');

      notifier.update('/home'); // back
      expect(c.read(currentRouteProvider), '/home');
    });

    test('provider is reactive — listener fires on update', () {
      final c = ProviderContainer();
      addTearDown(c.dispose);

      final received = <String>[];
      // Subscribe a listener
      final sub = c.listen<String>(currentRouteProvider, (prev, next) {
        received.add(next);
      });
      addTearDown(sub.close);

      c.read(currentRouteProvider.notifier).update('/explore');
      c.read(currentRouteProvider.notifier).update('/contact');

      expect(received, ['/explore', '/contact']);
    });

    test('different containers are fully isolated', () {
      final c1 = ProviderContainer();
      final c2 = ProviderContainer();
      addTearDown(c1.dispose);
      addTearDown(c2.dispose);

      c1.read(currentRouteProvider.notifier).update('/a');
      c2.read(currentRouteProvider.notifier).update('/b');

      expect(c1.read(currentRouteProvider), '/a');
      expect(c2.read(currentRouteProvider), '/b');
    });
  });

  // ---------------------------------------------------------------------------
  // GROUP 2: NavObserver — unit tests
  //
  // NavObserver defers onRouteChanged via addPostFrameCallback and deduplicates
  // multiple callbacks for the same path.
  //
  // We use testWidgets (not plain test) because:
  //   1. NavObserver calls WidgetsBinding.instance.addPostFrameCallback.
  //   2. WidgetsBinding.instance is only available in a widget-test environment.
  //   3. tester.pumpAndSettle() drains the postFrameCallback queue.
  //   4. Plain `test()` has no WidgetsBinding instance.
  // ---------------------------------------------------------------------------

  group('NavObserver', () {
    testWidgets('didPush fires onRouteChanged with the pushed path', (
      WidgetTester tester,
    ) async {
      await tester.pumpWidget(const SizedBox()); // activate widget tree
      final received = <String>[];
      final observer = NavObserver(onRouteChanged: received.add);

      observer.didPush(_FakeRoute('/home'), null);
      await tester.pump(); // drain one postFrameCallback frame

      expect(received, ['/home']);
    });

    testWidgets('didPop fires onRouteChanged with the previous route path', (
      WidgetTester tester,
    ) async {
      await tester.pumpWidget(const SizedBox());
      // didPop is called when the top route is removed; we expose the underlying page.
      // NavObserver passes `previousRoute` to _handleRoute (the revealed page).
      final received = <String>[];
      final observer = NavObserver(onRouteChanged: received.add);

      observer.didPop(_FakeRoute('/page-a'), _FakeRoute('/home'));
      await tester.pump();

      expect(received, ['/home']);
    });

    testWidgets('didReplace fires onRouteChanged with the new route path', (
      WidgetTester tester,
    ) async {
      await tester.pumpWidget(const SizedBox());
      final received = <String>[];
      final observer = NavObserver(onRouteChanged: received.add);

      observer.didReplace(
        newRoute: _FakeRoute('/explore'),
        oldRoute: _FakeRoute('/home'),
      );
      await tester.pump();

      expect(received, ['/explore']);
    });

    testWidgets('didRemove fires onRouteChanged with the previous route path', (
      WidgetTester tester,
    ) async {
      await tester.pumpWidget(const SizedBox());
      final received = <String>[];
      final observer = NavObserver(onRouteChanged: received.add);

      observer.didRemove(_FakeRoute('/modal'), _FakeRoute('/base'));
      await tester.pump();

      expect(received, ['/base']);
    });

    testWidgets('deduplication: two callbacks with same path only fire once', (
      WidgetTester tester,
    ) async {
      await tester.pumpWidget(const SizedBox());
      // GoRouter fires both didPush and didReplace for the same navigation.
      // NavObserver must deduplicate and call onRouteChanged exactly once.
      final received = <String>[];
      final observer = NavObserver(onRouteChanged: received.add);

      observer.didPush(_FakeRoute('/gallery'), null);
      observer.didReplace(
        newRoute: _FakeRoute('/gallery'),
        oldRoute: _FakeRoute('/home'),
      );
      await tester.pump();

      expect(
        received.length,
        1,
        reason: 'Duplicate callbacks must be deduplicated',
      );
      expect(received[0], '/gallery');
    });

    testWidgets('deduplication resets after a different path', (
      WidgetTester tester,
    ) async {
      await tester.pumpWidget(const SizedBox());
      // After navigating to /a, then /b, then /a again: each distinct nav fires once.
      final received = <String>[];
      final observer = NavObserver(onRouteChanged: received.add);

      observer.didPush(_FakeRoute('/a'), null);
      await tester.pump();

      observer.didPush(_FakeRoute('/b'), _FakeRoute('/a'));
      await tester.pump();

      observer.didPush(_FakeRoute('/a'), _FakeRoute('/b'));
      await tester.pump();

      expect(
        received,
        ['/a', '/b', '/a'],
        reason:
            'Each distinct navigation fires once; repeated paths at diff times are OK',
      );
    });

    testWidgets('unnamed route (null name) is ignored', (
      WidgetTester tester,
    ) async {
      await tester.pumpWidget(const SizedBox());
      final received = <String>[];
      final observer = NavObserver(onRouteChanged: received.add);

      observer.didPush(_UnnamedRoute(), null);
      await tester.pump();

      expect(
        received,
        isEmpty,
        reason: 'Routes with null name must be silently ignored',
      );
    });

    testWidgets('route with empty string name is ignored', (
      WidgetTester tester,
    ) async {
      await tester.pumpWidget(const SizedBox());
      final received = <String>[];
      final observer = NavObserver(onRouteChanged: received.add);

      observer.didPush(_FakeRoute(''), null);
      await tester.pump();

      expect(
        received,
        isEmpty,
        reason: 'Routes with empty name must be silently ignored',
      );
    });

    testWidgets('multiple distinct routes fire onRouteChanged in order', (
      WidgetTester tester,
    ) async {
      await tester.pumpWidget(const SizedBox());
      final received = <String>[];
      final observer = NavObserver(onRouteChanged: received.add);

      observer.didPush(_FakeRoute('/home'), null);
      await tester.pump();
      observer.didPush(_FakeRoute('/explore'), _FakeRoute('/home'));
      await tester.pump();
      observer.didPush(_FakeRoute('/about'), _FakeRoute('/explore'));
      await tester.pump();

      expect(received, ['/home', '/explore', '/about']);
    });

    testWidgets('onRouteChanged callback receives the exact path string', (
      WidgetTester tester,
    ) async {
      await tester.pumpWidget(const SizedBox());
      String? captured;
      final observer = NavObserver(onRouteChanged: (p) => captured = p);

      observer.didPush(_FakeRoute('/panorama/123?zoom=2'), null);
      await tester.pump();

      expect(captured, '/panorama/123?zoom=2');
    });
  });

  // ---------------------------------------------------------------------------
  // GROUP 3: NavObserver + currentRouteProvider — integrated
  //
  // These tests wire a NavObserver to a real currentRouteProvider so we can
  // verify the complete chain:
  //   NavigatorObserver.didPush → NavObserver → postFrameCallback
  //   → currentRouteProvider.notifier.update() → Riverpod state
  // ---------------------------------------------------------------------------

  group('NavObserver → currentRouteProvider integration', () {
    testWidgets('didPush updates currentRouteProvider', (
      WidgetTester tester,
    ) async {
      await tester.pumpWidget(const SizedBox());
      final c = ProviderContainer();
      addTearDown(c.dispose);

      final observer = NavObserver(
        onRouteChanged: (path) =>
            c.read(currentRouteProvider.notifier).update(path),
      );

      observer.didPush(_FakeRoute('/explore'), null);
      await tester.pump();

      expect(c.read(currentRouteProvider), '/explore');
    });

    testWidgets('navigation sequence correctly tracks active route', (
      WidgetTester tester,
    ) async {
      await tester.pumpWidget(const SizedBox());
      final c = ProviderContainer();
      addTearDown(c.dispose);

      final observer = NavObserver(
        onRouteChanged: (path) =>
            c.read(currentRouteProvider.notifier).update(path),
      );

      observer.didPush(_FakeRoute('/home'), null);
      await tester.pump();
      expect(c.read(currentRouteProvider), '/home');

      observer.didPush(_FakeRoute('/explore'), _FakeRoute('/home'));
      await tester.pump();
      expect(c.read(currentRouteProvider), '/explore');

      // Simulate going back
      observer.didPop(_FakeRoute('/explore'), _FakeRoute('/home'));
      await tester.pump();
      expect(c.read(currentRouteProvider), '/home');
    });

    testWidgets('deduplication does not double-update the provider', (
      WidgetTester tester,
    ) async {
      await tester.pumpWidget(const SizedBox());
      final c = ProviderContainer();
      addTearDown(c.dispose);

      int updateCount = 0;
      final observer = NavObserver(
        onRouteChanged: (path) {
          updateCount++;
          c.read(currentRouteProvider.notifier).update(path);
        },
      );

      // Simulate GoRouter firing both didPush + didReplace for same destination
      observer.didPush(_FakeRoute('/gallery'), null);
      observer.didReplace(
        newRoute: _FakeRoute('/gallery'),
        oldRoute: _FakeRoute('/home'),
      );
      await tester.pump();

      expect(updateCount, 1);
      expect(c.read(currentRouteProvider), '/gallery');
    });
  });
}

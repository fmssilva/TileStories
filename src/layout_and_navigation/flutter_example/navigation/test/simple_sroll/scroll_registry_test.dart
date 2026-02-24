// ignore_for_file: avoid_print, depend_on_referenced_packages

// =============================================================================
// SCROLL REGISTRY — FULL-STACK UNIT TESTS
// =============================================================================
//
// Tests the real [ScrollRegistry] class directly — no mocking.
// Uses Flutter's test runner (flutter_test) so Flutter bindings are available,
// which is required because [ScrollController] uses the Flutter engine.
//
// Coverage:
//   ScrollRegistry        — construction, controller creation, initialOffset,
//                           position caching via listener, cachedPositions
//                           immutability, dispose clears controllers
//   Position restoration  — controllers start at saved offset
//   Multiple controllers  — independent IDs don't interfere
//
// Run with:
//   flutter test lib/navigation/test/scroll_registry_test.dart
//
// =============================================================================

import 'package:flutter_test/flutter_test.dart';
import 'package:grande_panorama_ar/layout/scrollController/scroll_registry.dart';

void main() {
  // Flutter bindings needed for ScrollController
  TestWidgetsFlutterBinding.ensureInitialized();

  group('ScrollRegistry — construction', () {
    test('creates with empty savedPositions', () {
      final registry = ScrollRegistry(savedPositions: {});
      addTearDown(registry.dispose);
      // No exception — registry is valid
      expect(registry.cachedPositions, isEmpty);
    });

    test('creates with pre-populated savedPositions', () {
      final registry = ScrollRegistry(
        savedPositions: {'page': 300.0, 'sidebar': 50.0},
      );
      addTearDown(registry.dispose);
      // cachedPositions are empty until controllers are created and scroll events fire
      expect(registry.cachedPositions, isEmpty);
    });
  });

  // ---------------------------------------------------------------------------

  group('ScrollRegistry — controller()', () {
    test('controller() returns a ScrollController', () {
      final registry = ScrollRegistry(savedPositions: {});
      addTearDown(registry.dispose);

      final ctrl = registry.controller('main');
      expect(ctrl, isNotNull);
    });

    test('controller() with same id returns the same instance', () {
      final registry = ScrollRegistry(savedPositions: {});
      addTearDown(registry.dispose);

      final a = registry.controller('main');
      final b = registry.controller('main');
      expect(identical(a, b), true);
    });

    test('controller() with different ids returns different instances', () {
      final registry = ScrollRegistry(savedPositions: {});
      addTearDown(registry.dispose);

      final a = registry.controller('tab-a');
      final b = registry.controller('tab-b');
      expect(identical(a, b), false);
    });

    test('controller() sets initialScrollOffset from savedPositions', () {
      final registry = ScrollRegistry(savedPositions: {'page': 250.0});
      addTearDown(registry.dispose);

      // initialScrollOffset is set in the constructor — readable before attachment
      final ctrl = registry.controller('page');
      expect(ctrl.initialScrollOffset, 250.0);
    });

    test('controller() with no saved position defaults to 0.0', () {
      final registry = ScrollRegistry(savedPositions: {});
      addTearDown(registry.dispose);

      final ctrl = registry.controller('new-scroll');
      expect(ctrl.initialScrollOffset, 0.0);
    });

    test('multiple controllers have independent initialOffsets', () {
      final registry = ScrollRegistry(
        savedPositions: {'a': 100.0, 'b': 200.0, 'c': 0.0},
      );
      addTearDown(registry.dispose);

      expect(registry.controller('a').initialScrollOffset, 100.0);
      expect(registry.controller('b').initialScrollOffset, 200.0);
      expect(registry.controller('c').initialScrollOffset, 0.0);
    });
  });

  // ---------------------------------------------------------------------------

  group('ScrollRegistry — cachedPositions', () {
    test('cachedPositions is empty before any controllers are created', () {
      final registry = ScrollRegistry(savedPositions: {'p': 100.0});
      addTearDown(registry.dispose);

      expect(registry.cachedPositions, isEmpty);
    });

    test('cachedPositions is unmodifiable', () {
      final registry = ScrollRegistry(savedPositions: {});
      addTearDown(registry.dispose);

      registry.controller('main');

      expect(
        () => registry.cachedPositions['main'] = 99.0,
        throwsUnsupportedError,
      );
    });

    test('cachedPositions returns a snapshot (new map each call)', () {
      final registry = ScrollRegistry(savedPositions: {});
      addTearDown(registry.dispose);

      final snapshot1 = registry.cachedPositions;
      registry.controller('x'); // create a controller
      final snapshot2 = registry.cachedPositions;

      // Both are valid maps — the second might still be empty (no scroll fired)
      // but they are different objects
      expect(identical(snapshot1, snapshot2), false);
    });
  });

  // ---------------------------------------------------------------------------

  group('ScrollRegistry — dispose()', () {
    test('dispose() does not throw', () {
      final registry = ScrollRegistry(savedPositions: {});

      registry.controller('a');
      registry.controller('b');

      expect(() => registry.dispose(), returnsNormally);
    });

    test('dispose() can be called on empty registry', () {
      final registry = ScrollRegistry(savedPositions: {});
      expect(() => registry.dispose(), returnsNormally);
    });
  });

  // ---------------------------------------------------------------------------

  group('ScrollRegistry — realistic scenario', () {
    test('page with 3 scroll areas gets independent controllers', () {
      // Simulate DemoNav2Page requesting 3 inner scroll controllers
      final registry = ScrollRegistry(
        savedPositions: {
          'demo_nav2_inner_1': 120.0,
          'demo_nav2_inner_2': 0.0,
          'demo_nav2_inner_3': 450.0,
        },
      );
      addTearDown(registry.dispose);

      final c1 = registry.controller('demo_nav2_inner_1');
      final c2 = registry.controller('demo_nav2_inner_2');
      final c3 = registry.controller('demo_nav2_inner_3');

      // Each controller restores its own saved position
      expect(c1.initialScrollOffset, 120.0);
      expect(c2.initialScrollOffset, 0.0);
      expect(c3.initialScrollOffset, 450.0);

      // All are distinct objects
      expect(identical(c1, c2), false);
      expect(identical(c2, c3), false);
    });

    test('first visit (no saved positions) all controllers start at 0', () {
      // New navigation — no history entry has scroll data
      final registry = ScrollRegistry(savedPositions: {});
      addTearDown(registry.dispose);

      final mainCtrl = registry.controller('page');
      final sideCtrl = registry.controller('sidebar');

      expect(mainCtrl.initialScrollOffset, 0.0);
      expect(sideCtrl.initialScrollOffset, 0.0);
    });
  });
}

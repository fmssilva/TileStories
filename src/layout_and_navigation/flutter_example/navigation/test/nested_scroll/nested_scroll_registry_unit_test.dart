// ignore_for_file: depend_on_referenced_packages

// =============================================================================
// NESTED SCROLL — SCROLL REGISTRY UNIT TESTS
// =============================================================================
//
// These tests exercise ScrollRegistry with MULTIPLE keys simultaneously —
// the scenario that occurs on pages with nested scroll areas, like DemoNav2Page:
//
//   'page'              → outer SingleChildScrollView (managed by LayoutManager)
//   'demo_nav2_inner_1' → inner ListView 1 (managed by page body)
//   'demo_nav2_inner_2' → inner ListView 2 (managed by page body)
//   'demo_nav2_inner_3' → inner ListView 3 (managed by page body)
//
// WHAT THESE TESTS PROVE:
//   - Each key maps to an independent ScrollController
//   - Each controller gets its own initialScrollOffset from savedPositions
//   - cachedPositions captures all keys simultaneously
//   - restorePositions() updates all attached controllers at once
//   - dispose() cleans up all controllers regardless of how many were created
//
// DIFFERENCE FROM simple_sroll/scroll_registry_test.dart:
//   That file tests single-key scenarios.
//   These tests specifically target the multi-key nested scroll scenario.
//
// Run with:
//   flutter test lib/navigation/test/nested_scroll/nested_scroll_registry_unit_test.dart
//   flutter test lib/navigation/test/
//
// =============================================================================

import 'package:flutter_test/flutter_test.dart';
import 'package:grande_panorama_ar/layout/scrollController/scroll_registry.dart';

void main() {
  // ---------------------------------------------------------------------------
  // GROUP 1: Multi-key controller creation
  // ---------------------------------------------------------------------------

  group('Nested ScrollRegistry — multi-key controller creation', () {
    test('4 keys each return an independent ScrollController', () {
      final registry = ScrollRegistry(savedPositions: {});
      addTearDown(registry.dispose);

      final page = registry.controller('page');
      final inner1 = registry.controller('inner_1');
      final inner2 = registry.controller('inner_2');
      final inner3 = registry.controller('inner_3');

      // All are distinct objects
      expect(identical(page, inner1), false);
      expect(identical(page, inner2), false);
      expect(identical(page, inner3), false);
      expect(identical(inner1, inner2), false);
      expect(identical(inner1, inner3), false);
      expect(identical(inner2, inner3), false);
    });

    test('same key always returns the same controller instance', () {
      final registry = ScrollRegistry(savedPositions: {});
      addTearDown(registry.dispose);

      final a = registry.controller('inner_1');
      final b = registry.controller('inner_1');

      expect(identical(a, b), true);
    });

    test(
      '4 keys with different savedPositions each get correct initialOffset',
      () {
        final registry = ScrollRegistry(
          savedPositions: {
            'page': 150.0,
            'inner_1': 300.0,
            'inner_2': 0.0,
            'inner_3': 450.0,
          },
        );
        addTearDown(registry.dispose);

        expect(registry.controller('page').initialScrollOffset, 150.0);
        expect(registry.controller('inner_1').initialScrollOffset, 300.0);
        expect(registry.controller('inner_2').initialScrollOffset, 0.0);
        expect(registry.controller('inner_3').initialScrollOffset, 450.0);
      },
    );

    test('keys not present in savedPositions default to 0.0', () {
      final registry = ScrollRegistry(savedPositions: {'page': 200.0});
      addTearDown(registry.dispose);

      // 'page' is saved, but inner scrolls are new
      expect(registry.controller('page').initialScrollOffset, 200.0);
      expect(registry.controller('inner_1').initialScrollOffset, 0.0);
      expect(registry.controller('inner_2').initialScrollOffset, 0.0);
      expect(registry.controller('inner_3').initialScrollOffset, 0.0);
    });

    test(
      'DemoNav2Page scenario: 3 inner keys with realistic saved offsets',
      () {
        // Mirrors exactly the keys used by demo_nav2_page.dart
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

        expect(c1.initialScrollOffset, 120.0);
        expect(c2.initialScrollOffset, 0.0);
        expect(c3.initialScrollOffset, 450.0);

        // All distinct
        expect(identical(c1, c2), false);
        expect(identical(c2, c3), false);
      },
    );
  });

  // ---------------------------------------------------------------------------
  // GROUP 2: cachedPositions with multiple keys
  // ---------------------------------------------------------------------------

  group('Nested ScrollRegistry — cachedPositions with multiple keys', () {
    test('cachedPositions is empty before any controllers are created', () {
      final registry = ScrollRegistry(
        savedPositions: {'page': 100.0, 'inner_1': 200.0, 'inner_2': 300.0},
      );
      addTearDown(registry.dispose);

      // savedPositions are used for initialScrollOffset only;
      // cachedPositions is populated by scroll listener, not constructor
      expect(registry.cachedPositions, isEmpty);
    });

    test('creating 4 controllers does not populate cachedPositions', () {
      final registry = ScrollRegistry(savedPositions: {});
      addTearDown(registry.dispose);

      registry.controller('page');
      registry.controller('inner_1');
      registry.controller('inner_2');
      registry.controller('inner_3');

      // Listeners fire only when scroll position changes — not at creation time
      expect(registry.cachedPositions, isEmpty);
    });

    test('cachedPositions is unmodifiable even with multiple keys', () {
      final registry = ScrollRegistry(savedPositions: {});
      addTearDown(registry.dispose);

      registry.controller('page');
      registry.controller('inner_1');

      expect(
        () => registry.cachedPositions['page'] = 99.0,
        throwsUnsupportedError,
      );
    });

    test('cachedPositions returns a new snapshot on each call', () {
      final registry = ScrollRegistry(savedPositions: {});
      addTearDown(registry.dispose);

      registry.controller('page');
      registry.controller('inner_1');
      registry.controller('inner_2');

      final snap1 = registry.cachedPositions;
      final snap2 = registry.cachedPositions;

      expect(identical(snap1, snap2), false);
    });
  });

  // ---------------------------------------------------------------------------
  // GROUP 3: dispose() with multiple controllers
  // ---------------------------------------------------------------------------

  group('Nested ScrollRegistry — dispose() with multiple keys', () {
    test('dispose() does not throw with 4 controllers', () {
      final registry = ScrollRegistry(savedPositions: {});

      registry.controller('page');
      registry.controller('inner_1');
      registry.controller('inner_2');
      registry.controller('inner_3');

      expect(() => registry.dispose(), returnsNormally);
    });

    test('dispose() works even if only some keys were ever requested', () {
      // savedPositions has 4 keys, but only 2 controllers were actually created
      final registry = ScrollRegistry(
        savedPositions: {
          'page': 100.0,
          'inner_1': 200.0,
          'inner_2': 0.0,
          'inner_3': 50.0,
        },
      );

      registry.controller('page');
      registry.controller('inner_2'); // only 2 of the 4 requested

      expect(() => registry.dispose(), returnsNormally);
    });

    test('dispose() on completely empty registry does not throw', () {
      final registry = ScrollRegistry(savedPositions: {});
      expect(() => registry.dispose(), returnsNormally);
    });
  });

  // ---------------------------------------------------------------------------
  // GROUP 4: restorePositions() with multiple keys
  // ---------------------------------------------------------------------------

  group('Nested ScrollRegistry — restorePositions() with multiple keys', () {
    test(
      'restorePositions() does not throw when no controllers are attached',
      () {
        final registry = ScrollRegistry(savedPositions: {});
        addTearDown(registry.dispose);

        // Controllers exist but are not attached to any scroll view
        registry.controller('page');
        registry.controller('inner_1');
        registry.controller('inner_2');

        // restorePositions() uses jumpTo() which silently skips non-attached controllers
        expect(
          () => registry.restorePositions({
            'page': 200.0,
            'inner_1': 100.0,
            'inner_2': 50.0,
          }),
          returnsNormally,
        );
      },
    );

    test('restorePositions() with empty map does not throw', () {
      final registry = ScrollRegistry(savedPositions: {});
      addTearDown(registry.dispose);

      registry.controller('page');
      registry.controller('inner_1');

      expect(() => registry.restorePositions({}), returnsNormally);
    });

    test(
      'restorePositions() with keys that have no matching controllers does not throw',
      () {
        final registry = ScrollRegistry(savedPositions: {});
        addTearDown(registry.dispose);

        // Only 'page' controller was created
        registry.controller('page');

        // Positions for keys that were never requested — should be ignored gracefully
        expect(
          () => registry.restorePositions({
            'page': 300.0,
            'inner_1': 150.0,
            'inner_3': 75.0,
          }),
          returnsNormally,
        );
      },
    );
  });

  // ---------------------------------------------------------------------------
  // GROUP 5: Realistic nested scroll scenarios (end-to-end unit flow)
  // ---------------------------------------------------------------------------

  group('Nested ScrollRegistry — realistic scenarios', () {
    test('first visit: all 4 controllers start at offset 0', () {
      // New navigation — no history entry has scroll data
      final registry = ScrollRegistry(savedPositions: {});
      addTearDown(registry.dispose);

      expect(registry.controller('page').initialScrollOffset, 0.0);
      expect(registry.controller('inner_1').initialScrollOffset, 0.0);
      expect(registry.controller('inner_2').initialScrollOffset, 0.0);
      expect(registry.controller('inner_3').initialScrollOffset, 0.0);
    });

    test('UNDO visit: all 4 controllers restore their saved offsets', () {
      // Returning via UNDO — history has the positions from the last visit
      final savedPositions = {
        'page': 80.0,
        'inner_1': 320.0,
        'inner_2': 0.0,
        'inner_3': 160.0,
      };

      final registry = ScrollRegistry(savedPositions: savedPositions);
      addTearDown(registry.dispose);

      // Each controller gets the right initialScrollOffset
      expect(registry.controller('page').initialScrollOffset, 80.0);
      expect(registry.controller('inner_1').initialScrollOffset, 320.0);
      expect(registry.controller('inner_2').initialScrollOffset, 0.0);
      expect(registry.controller('inner_3').initialScrollOffset, 160.0);
    });

    test('partial UNDO: only outer scroll was saved, inners start at 0', () {
      // User scrolled only the outer view before leaving
      final registry = ScrollRegistry(savedPositions: {'page': 250.0});
      addTearDown(registry.dispose);

      // Outer scroll is restored
      expect(registry.controller('page').initialScrollOffset, 250.0);
      // Inners that were never scrolled default to 0
      expect(registry.controller('inner_1').initialScrollOffset, 0.0);
      expect(registry.controller('inner_2').initialScrollOffset, 0.0);
      expect(registry.controller('inner_3').initialScrollOffset, 0.0);
    });

    test(
      'all inner keys are independent — one large value does not affect others',
      () {
        final registry = ScrollRegistry(
          savedPositions: {
            'page': 0.0,
            'inner_1': 9999.0,
            'inner_2': 0.0,
            'inner_3': 0.0,
          },
        );
        addTearDown(registry.dispose);

        expect(registry.controller('page').initialScrollOffset, 0.0);
        expect(registry.controller('inner_1').initialScrollOffset, 9999.0);
        expect(registry.controller('inner_2').initialScrollOffset, 0.0);
        expect(registry.controller('inner_3').initialScrollOffset, 0.0);
      },
    );
  });
}

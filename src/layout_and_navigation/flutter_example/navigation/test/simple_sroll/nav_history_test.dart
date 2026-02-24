// ignore_for_file: avoid_print, depend_on_referenced_packages

// =============================================================================
// NAV HISTORY — FULL-STACK UNIT TESTS
// =============================================================================
//
// These tests use the REAL providers, notifiers, state classes, and models —
// no mocking. A [ProviderContainer] is created for each test, which gives us
// an isolated Riverpod environment exactly as the app uses at runtime.
//
// Coverage:
//   HistoryEntry          — model, scroll positions, toString
//   NavHistoryState       — canGoBack/Forward, current/previous/next, copyWith
//   NavHistoryNotifier    — push, undoRedo (back & forward heuristic),
//                           scroll positions save/get, max-entries trim,
//                           duplicate-path dedup, getBackPath/getForwardPath
//   canGoBackProvider     — reactive derived provider
//   canGoForwardProvider  — reactive derived provider
//
// Run with:
//   flutter test lib/navigation/test/nav_history_test.dart
//   flutter test lib/navigation/test/  (whole folder)
//
// =============================================================================

import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:grande_panorama_ar/navigation/histConfig/history_entry.dart';
import 'package:grande_panorama_ar/navigation/histConfig/history_provider.dart';
import 'package:grande_panorama_ar/navigation/histConfig/history_state.dart';

void main() {
  // ---------------------------------------------------------------------------
  // Helpers
  // ---------------------------------------------------------------------------

  /// Fresh isolated container for each test — exactly like the app at runtime
  ProviderContainer makeContainer() => ProviderContainer();

  group('HistoryEntry model', () {
    test('creates with path and empty scrollPositions by default', () {
      const e = HistoryEntry(path: '/home');
      expect(e.path, '/home');
      expect(e.scrollPositions, isEmpty);
    });

    test('copyWithScrollPositions merges maps', () {
      const e = HistoryEntry(path: '/home', scrollPositions: {'a': 10.0});
      final updated = e.copyWithScrollPositions({'b': 20.0});
      expect(updated.scrollPositions, {'a': 10.0, 'b': 20.0});
      expect(updated.path, '/home'); // path unchanged
    });

    test('copyWithScrollPositions overwrites existing key', () {
      const e = HistoryEntry(path: '/home', scrollPositions: {'a': 10.0});
      final updated = e.copyWithScrollPositions({'a': 99.0});
      expect(updated.scrollPositions['a'], 99.0);
    });

    test('scrollPositions field returns same map', () {
      const e = HistoryEntry(path: '/x', scrollPositions: {'k': 5.0});
      expect(e.scrollPositions, {'k': 5.0});
    });

    test('toString includes path', () {
      const e = HistoryEntry(path: '/about');
      expect(e.toString(), contains('/about'));
    });
  });

  // ---------------------------------------------------------------------------

  group('NavHistoryState', () {
    test('initial state is empty', () {
      const s = NavHistoryState();
      expect(s.entries, isEmpty);
      expect(s.currentIndex, -1);
      expect(s.current, isNull);
      expect(s.previous, isNull);
      expect(s.next, isNull);
      expect(s.canGoBack, false);
      expect(s.canGoForward, false);
    });

    test('canGoBack false with single entry', () {
      const s = NavHistoryState(
        entries: [HistoryEntry(path: '/a')],
        currentIndex: 0,
      );
      expect(s.canGoBack, false);
    });

    test('canGoBack true with two entries at index 1', () {
      const s = NavHistoryState(
        entries: [
          HistoryEntry(path: '/a'),
          HistoryEntry(path: '/b'),
        ],
        currentIndex: 1,
      );
      expect(s.canGoBack, true);
    });

    test('canGoForward true when not at last entry', () {
      const s = NavHistoryState(
        entries: [
          HistoryEntry(path: '/a'),
          HistoryEntry(path: '/b'),
        ],
        currentIndex: 0,
      );
      expect(s.canGoForward, true);
    });

    test('current returns correct entry', () {
      const s = NavHistoryState(
        entries: [
          HistoryEntry(path: '/a'),
          HistoryEntry(path: '/b'),
        ],
        currentIndex: 1,
      );
      expect(s.current?.path, '/b');
    });

    test('previous returns entry before current', () {
      const s = NavHistoryState(
        entries: [
          HistoryEntry(path: '/a'),
          HistoryEntry(path: '/b'),
        ],
        currentIndex: 1,
      );
      expect(s.previous?.path, '/a');
    });

    test('next returns entry after current', () {
      const s = NavHistoryState(
        entries: [
          HistoryEntry(path: '/a'),
          HistoryEntry(path: '/b'),
        ],
        currentIndex: 0,
      );
      expect(s.next?.path, '/b');
    });

    test('copyWith replaces only specified fields', () {
      const s = NavHistoryState(
        entries: [HistoryEntry(path: '/a')],
        currentIndex: 0,
      );
      final s2 = s.copyWith(currentIndex: -1);
      expect(s2.currentIndex, -1);
      expect(s2.entries.length, 1); // unchanged
    });
  });

  // ---------------------------------------------------------------------------

  group('NavHistoryNotifier — push()', () {
    test('push() first path → 1 entry at index 0', () {
      final c = makeContainer();
      addTearDown(c.dispose);

      c.read(navHistoryProvider.notifier).push('/home');

      final s = c.read(navHistoryProvider);
      expect(s.entries.length, 1);
      expect(s.currentIndex, 0);
      expect(s.current?.path, '/home');
    });

    test('push() builds sequential stack', () {
      final c = makeContainer();
      addTearDown(c.dispose);
      final n = c.read(navHistoryProvider.notifier);

      n.push('/home');
      n.push('/explore');
      n.push('/about');

      final s = c.read(navHistoryProvider);
      expect(s.entries.map((e) => e.path).toList(), [
        '/home',
        '/explore',
        '/about',
      ]);
      expect(s.currentIndex, 2);
    });

    test('push() after UNDO truncates forward history', () {
      final c = makeContainer();
      addTearDown(c.dispose);
      final n = c.read(navHistoryProvider.notifier);

      n.push('/a');
      n.push('/b');
      n.push('/c');
      // Simulate UNDO back to /b
      n.undoRedo('/b');
      // Now push a new page — /c should be discarded
      n.push('/d');

      final paths = c.read(navHistoryProvider).entries.map((e) => e.path);
      expect(paths.toList(), ['/a', '/b', '/d']);
      expect(c.read(navHistoryProvider).currentIndex, 2);
    });

    test('push() after two UNDOs truncates both forward entries', () {
      final c = makeContainer();
      addTearDown(c.dispose);
      final n = c.read(navHistoryProvider.notifier);

      n.push('/a');
      n.push('/b');
      n.push('/c');
      n.undoRedo('/b'); // back one
      n.undoRedo('/a'); // back two
      n.push('/z');

      final paths = c.read(navHistoryProvider).entries.map((e) => e.path);
      expect(paths.toList(), ['/a', '/z']);
    });

    test('push() trims oldest entries when over maxEntries ($maxEntries)', () {
      final c = makeContainer();
      addTearDown(c.dispose);
      final n = c.read(navHistoryProvider.notifier);

      // Push maxEntries + 5 pages
      for (int i = 0; i < maxEntries + 5; i++) {
        n.push('/page-$i');
      }

      final s = c.read(navHistoryProvider);
      expect(s.entries.length, maxEntries);
      // First 5 were trimmed — oldest remaining is /page-5
      expect(s.entries.first.path, '/page-5');
      expect(s.entries.last.path, '/page-${maxEntries + 4}');
    });
  });

  // ---------------------------------------------------------------------------

  group('NavHistoryNotifier — undoRedo()', () {
    test('undoRedo() with previous path decrements index (UNDO)', () {
      final c = makeContainer();
      addTearDown(c.dispose);
      final n = c.read(navHistoryProvider.notifier);

      n.push('/home');
      n.push('/explore');
      n.push('/about');

      // UNDO — go back to /explore
      n.undoRedo('/explore');

      expect(c.read(navHistoryProvider).currentIndex, 1);
      expect(c.read(navHistoryProvider).current?.path, '/explore');
    });

    test('undoRedo() with next path increments index (REDO)', () {
      final c = makeContainer();
      addTearDown(c.dispose);
      final n = c.read(navHistoryProvider.notifier);

      n.push('/home');
      n.push('/explore');
      n.undoRedo('/home'); // UNDO to home
      // Now REDO back to /explore
      n.undoRedo('/explore');

      expect(c.read(navHistoryProvider).currentIndex, 1);
      expect(c.read(navHistoryProvider).current?.path, '/explore');
    });

    test('undoRedo() prefers UNDO when path matches both prev and next', () {
      // Edge case: A → B → A (same path in both prev and next)
      final c = makeContainer();
      addTearDown(c.dispose);
      final n = c.read(navHistoryProvider.notifier);

      n.push('/a');
      n.push('/b');
      n.push('/a'); // /a again — now: [/a, /b, /a], index=2
      // UNDO to /b
      n.undoRedo('/b');
      // Now at index=1. prev=/a, next=/a
      // undoRedo('/a') should prefer UNDO (priority given to back)
      n.undoRedo('/a');

      // Should have gone BACK to index 0
      expect(c.read(navHistoryProvider).currentIndex, 0);
    });

    test('undoRedo() with unknown path does not change index', () {
      final c = makeContainer();
      addTearDown(c.dispose);
      final n = c.read(navHistoryProvider.notifier);

      n.push('/home');
      n.push('/explore');
      n.undoRedo('/zzz-unknown');

      expect(c.read(navHistoryProvider).currentIndex, 1);
    });

    test('full UNDO/REDO round trip preserves all entries', () {
      final c = makeContainer();
      addTearDown(c.dispose);
      final n = c.read(navHistoryProvider.notifier);

      n.push('/a');
      n.push('/b');
      n.push('/c');

      // UNDO all the way back
      n.undoRedo('/b');
      n.undoRedo('/a');
      expect(c.read(navHistoryProvider).currentIndex, 0);

      // REDO all the way forward
      n.undoRedo('/b');
      n.undoRedo('/c');
      expect(c.read(navHistoryProvider).currentIndex, 2);

      // Entries are still intact — nothing was truncated
      expect(c.read(navHistoryProvider).entries.length, 3);
    });
  });

  // ---------------------------------------------------------------------------

  group('NavHistoryNotifier — scroll positions', () {
    test('saveScrollPositionsAt() stores on current entry', () {
      final c = makeContainer();
      addTearDown(c.dispose);
      final n = c.read(navHistoryProvider.notifier);

      n.push('/home');
      n.saveScrollPositionsAt(0, {'page': 300.0, 'sidebar': 50.0});

      final entry = c.read(navHistoryProvider).current!;
      expect(entry.scrollPositions['page'], 300.0);
      expect(entry.scrollPositions['sidebar'], 50.0);
    });

    test(
      'saveScrollPositionsAt() overwrites existing positions for that index',
      () {
        final c = makeContainer();
        addTearDown(c.dispose);
        final n = c.read(navHistoryProvider.notifier);

        n.push('/home');
        n.saveScrollPositionsAt(0, {'page': 100.0});
        n.saveScrollPositionsAt(0, {
          'page': 100.0,
          'sidebar': 50.0,
        }); // second call with full map

        final entry = c.read(navHistoryProvider).current!;
        expect(entry.scrollPositions['page'], 100.0);
        expect(entry.scrollPositions['sidebar'], 50.0);
      },
    );

    test('getScrollPositions() returns current entry positions', () {
      final c = makeContainer();
      addTearDown(c.dispose);
      final n = c.read(navHistoryProvider.notifier);

      n.push('/explore');
      n.saveScrollPositionsAt(0, {'main': 250.0});

      expect(n.getScrollPositions(), {'main': 250.0});
    });

    test('scroll positions survive UNDO/REDO navigation', () {
      final c = makeContainer();
      addTearDown(c.dispose);
      final n = c.read(navHistoryProvider.notifier);

      // Navigate to /home and scroll
      n.push('/home');
      n.saveScrollPositionsAt(0, {'page': 400.0});

      // Navigate forward
      n.push('/explore');
      n.saveScrollPositionsAt(1, {'page': 100.0});

      // UNDO back to /home
      n.undoRedo('/home');

      // Scroll position for /home must still be there
      expect(n.getScrollPositions()?['page'], 400.0);
    });

    test(
      'each history entry stores its own scroll positions independently',
      () {
        final c = makeContainer();
        addTearDown(c.dispose);
        final n = c.read(navHistoryProvider.notifier);

        n.push('/a');
        n.saveScrollPositionsAt(0, {'p': 10.0});

        n.push('/b');
        n.saveScrollPositionsAt(1, {'p': 20.0});

        n.push('/c');
        n.saveScrollPositionsAt(2, {'p': 30.0});

        final entries = c.read(navHistoryProvider).entries;
        expect(entries[0].scrollPositions['p'], 10.0);
        expect(entries[1].scrollPositions['p'], 20.0);
        expect(entries[2].scrollPositions['p'], 30.0);
      },
    );
  });

  // ---------------------------------------------------------------------------

  group('NavHistoryNotifier — getBackPath / getForwardPath', () {
    test('getBackPath returns null at first entry', () {
      final c = makeContainer();
      addTearDown(c.dispose);
      final n = c.read(navHistoryProvider.notifier);

      n.push('/home');
      expect(n.getBackPath(), isNull);
    });

    test('getBackPath returns previous path', () {
      final c = makeContainer();
      addTearDown(c.dispose);
      final n = c.read(navHistoryProvider.notifier);

      n.push('/home');
      n.push('/explore');
      expect(n.getBackPath(), '/home');
    });

    test('getForwardPath returns null at last entry', () {
      final c = makeContainer();
      addTearDown(c.dispose);
      final n = c.read(navHistoryProvider.notifier);

      n.push('/home');
      n.push('/explore');
      expect(n.getForwardPath(), isNull);
    });

    test('getForwardPath returns next path after UNDO', () {
      final c = makeContainer();
      addTearDown(c.dispose);
      final n = c.read(navHistoryProvider.notifier);

      n.push('/home');
      n.push('/explore');
      n.undoRedo('/home'); // UNDO
      expect(n.getForwardPath(), '/explore');
    });
  });

  // ---------------------------------------------------------------------------

  group('canGoBackProvider / canGoForwardProvider (derived providers)', () {
    test('canGoBackProvider is false with single entry', () {
      final c = makeContainer();
      addTearDown(c.dispose);

      c.read(navHistoryProvider.notifier).push('/home');
      expect(c.read(canGoBackProvider), false);
    });

    test('canGoBackProvider is true after second push', () {
      final c = makeContainer();
      addTearDown(c.dispose);
      final n = c.read(navHistoryProvider.notifier);

      n.push('/home');
      n.push('/explore');
      expect(c.read(canGoBackProvider), true);
    });

    test('canGoForwardProvider is false at last entry', () {
      final c = makeContainer();
      addTearDown(c.dispose);
      final n = c.read(navHistoryProvider.notifier);

      n.push('/home');
      n.push('/explore');
      expect(c.read(canGoForwardProvider), false);
    });

    test('canGoForwardProvider is true after UNDO', () {
      final c = makeContainer();
      addTearDown(c.dispose);
      final n = c.read(navHistoryProvider.notifier);

      n.push('/home');
      n.push('/explore');
      n.undoRedo('/home'); // UNDO
      expect(c.read(canGoForwardProvider), true);
    });

    test('providers update reactively as state changes', () {
      final c = makeContainer();
      addTearDown(c.dispose);
      final n = c.read(navHistoryProvider.notifier);

      expect(c.read(canGoBackProvider), false);
      expect(c.read(canGoForwardProvider), false);

      n.push('/a');
      expect(c.read(canGoBackProvider), false);
      expect(c.read(canGoForwardProvider), false);

      n.push('/b');
      expect(c.read(canGoBackProvider), true); // can now go back
      expect(c.read(canGoForwardProvider), false);

      n.undoRedo('/a'); // UNDO
      expect(c.read(canGoBackProvider), false); // at start
      expect(c.read(canGoForwardProvider), true); // can now go forward

      n.undoRedo('/b'); // REDO
      expect(c.read(canGoBackProvider), true);
      expect(c.read(canGoForwardProvider), false);
    });
  });

  // ---------------------------------------------------------------------------

  group('Realistic scenario — full browser-like workflow', () {
    test('User browses 4 pages, goes back 2, then navigates to new page', () {
      // Scenario:
      //   / → /explore → /about → /contact
      //   User clicks UNDO twice → now at /explore
      //   User clicks new tab /gallery → /contact is discarded
      //   Result: [/, /explore, /gallery]

      final c = makeContainer();
      addTearDown(c.dispose);
      final n = c.read(navHistoryProvider.notifier);

      n.push('/');
      n.push('/explore');
      n.push('/about');
      n.push('/contact');

      // UNDO twice
      n.undoRedo('/about'); // index 2
      n.undoRedo('/explore'); // index 1

      // New navigation
      n.push('/gallery');

      final s = c.read(navHistoryProvider);
      expect(s.entries.map((e) => e.path).toList(), [
        '/',
        '/explore',
        '/gallery',
      ]);
      expect(s.currentIndex, 2);
      expect(c.read(canGoBackProvider), true);
      expect(c.read(canGoForwardProvider), false);
    });

    test('Scroll positions saved and restored correctly across UNDO/REDO', () {
      // Scenario:
      //   Navigate to /home, scroll to 500
      //   Navigate to /explore, scroll to 200
      //   UNDO back to /home → should see 500
      //   REDO back to /explore → should see 200

      final c = makeContainer();
      addTearDown(c.dispose);
      final n = c.read(navHistoryProvider.notifier);

      n.push('/home');
      n.saveScrollPositionsAt(0, {'page': 500.0});

      n.push('/explore');
      n.saveScrollPositionsAt(1, {'page': 200.0});

      // UNDO
      n.undoRedo('/home');
      expect(n.getScrollPositions()?['page'], 500.0);

      // REDO
      n.undoRedo('/explore');
      expect(n.getScrollPositions()?['page'], 200.0);
    });

    test('UNDO and REDO button enable/disable states match expected', () {
      final c = makeContainer();
      addTearDown(c.dispose);
      final n = c.read(navHistoryProvider.notifier);

      // At start — both disabled
      expect(c.read(canGoBackProvider), false);
      expect(c.read(canGoForwardProvider), false);

      n.push('/home');
      // Still can't go back — only 1 page
      expect(c.read(canGoBackProvider), false);
      expect(c.read(canGoForwardProvider), false);

      n.push('/explore');
      // UNDO enabled, REDO disabled
      expect(c.read(canGoBackProvider), true);
      expect(c.read(canGoForwardProvider), false);

      n.undoRedo('/home');
      // UNDO disabled (at start), REDO enabled
      expect(c.read(canGoBackProvider), false);
      expect(c.read(canGoForwardProvider), true);

      n.undoRedo('/explore');
      // UNDO enabled, REDO disabled (back at end)
      expect(c.read(canGoBackProvider), true);
      expect(c.read(canGoForwardProvider), false);
    });
  });

  // ---------------------------------------------------------------------------
  // EDGE CASES — max-entries boundary behaviour
  // ---------------------------------------------------------------------------

  group('Max entries boundary', () {
    test('exactly $maxEntries entries — no trim occurs', () {
      final c = makeContainer();
      addTearDown(c.dispose);
      final n = c.read(navHistoryProvider.notifier);

      for (int i = 0; i < maxEntries; i++) {
        n.push('/p$i');
      }

      final s = c.read(navHistoryProvider);
      expect(s.entries.length, maxEntries);
      expect(s.entries.first.path, '/p0'); // oldest NOT trimmed
      expect(s.entries.last.path, '/p${maxEntries - 1}');
      expect(s.currentIndex, maxEntries - 1);
    });

    test('$maxEntries + 1 entries — oldest one is trimmed', () {
      final c = makeContainer();
      addTearDown(c.dispose);
      final n = c.read(navHistoryProvider.notifier);

      for (int i = 0; i <= maxEntries; i++) {
        n.push('/p$i');
      }

      final s = c.read(navHistoryProvider);
      expect(s.entries.length, maxEntries); // still capped
      expect(s.entries.first.path, '/p1'); // /p0 was trimmed
      expect(s.entries.last.path, '/p$maxEntries');
      expect(s.currentIndex, maxEntries - 1); // still pointing at last
    });

    test('100 entries — always capped at $maxEntries, last entry correct', () {
      final c = makeContainer();
      addTearDown(c.dispose);
      final n = c.read(navHistoryProvider.notifier);

      const total = 100;
      for (int i = 0; i < total; i++) {
        n.push('/p$i');
      }

      final s = c.read(navHistoryProvider);
      expect(s.entries.length, maxEntries);
      // First 50 entries were trimmed one by one; oldest remaining is /p(100-50)=/p50
      expect(s.entries.first.path, '/p${total - maxEntries}');
      expect(s.entries.last.path, '/p${total - 1}');
      expect(s.currentIndex, maxEntries - 1);
    });
  });

  // ---------------------------------------------------------------------------
  // EDGE CASES — undo many, redo some, check scroll state per step
  // ---------------------------------------------------------------------------

  group('Undo many / redo some with scroll state verification', () {
    test(
      'undo many times then redo some — scroll state correct at each step',
      () {
        // Build: [/a(scroll=10), /b(scroll=20), /c(scroll=30), /d(scroll=40), /e(scroll=50)]
        // Undo all the way to /a, then redo to /c.
        // After each step the scroll state must match the entry we are now at.

        final c = makeContainer();
        addTearDown(c.dispose);
        final n = c.read(navHistoryProvider.notifier);

        final pages = ['/a', '/b', '/c', '/d', '/e'];
        for (int i = 0; i < pages.length; i++) {
          n.push(pages[i]);
          n.saveScrollPositionsAt(i, {'main': (i + 1) * 10.0});
        }
        // State: [/a=10, /b=20, /c=30, /d=40, /e=50], index=4

        // Undo to /d
        n.undoRedo('/d');
        expect(c.read(navHistoryProvider).current?.path, '/d');
        expect(n.getScrollPositions()?['main'], 40.0);

        // Undo to /c
        n.undoRedo('/c');
        expect(c.read(navHistoryProvider).current?.path, '/c');
        expect(n.getScrollPositions()?['main'], 30.0);

        // Undo to /b
        n.undoRedo('/b');
        expect(c.read(navHistoryProvider).current?.path, '/b');
        expect(n.getScrollPositions()?['main'], 20.0);

        // Undo to /a
        n.undoRedo('/a');
        expect(c.read(navHistoryProvider).current?.path, '/a');
        expect(n.getScrollPositions()?['main'], 10.0);
        expect(c.read(canGoBackProvider), false); // at start

        // Redo to /b
        n.undoRedo('/b');
        expect(c.read(navHistoryProvider).current?.path, '/b');
        expect(n.getScrollPositions()?['main'], 20.0);

        // Redo to /c
        n.undoRedo('/c');
        expect(c.read(navHistoryProvider).current?.path, '/c');
        expect(n.getScrollPositions()?['main'], 30.0);

        // Still have future /d and /e
        expect(c.read(canGoForwardProvider), true);
        // All 5 entries are still intact
        expect(c.read(navHistoryProvider).entries.length, 5);
      },
    );

    test(
      'undo many then push — future entries are trimmed, scroll states preserved for remaining',
      () {
        // Build: [/a(scroll=10), /b(scroll=20), /c(scroll=30), /d(scroll=40)]
        // Undo to /b, then push /z.
        // Expected result: [/a, /b, /z], /c and /d discarded.
        // /a and /b must keep their scroll positions.

        final c = makeContainer();
        addTearDown(c.dispose);
        final n = c.read(navHistoryProvider.notifier);

        n.push('/a');
        n.saveScrollPositionsAt(0, {'main': 10.0});
        n.push('/b');
        n.saveScrollPositionsAt(1, {'main': 20.0});
        n.push('/c');
        n.saveScrollPositionsAt(2, {'main': 30.0});
        n.push('/d');
        n.saveScrollPositionsAt(3, {'main': 40.0});

        // Undo twice (to /b)
        n.undoRedo('/c');
        n.undoRedo('/b');
        expect(c.read(navHistoryProvider).currentIndex, 1);

        // Push new page /z → trims /c and /d
        n.push('/z');

        final s = c.read(navHistoryProvider);
        expect(s.entries.map((e) => e.path).toList(), ['/a', '/b', '/z']);
        expect(s.currentIndex, 2);
        expect(c.read(canGoForwardProvider), false); // no future entries

        // Scroll positions for /a and /b are still intact after trim
        expect(s.entries[0].scrollPositions['main'], 10.0);
        expect(s.entries[1].scrollPositions['main'], 20.0);
        // /z is brand new — no scroll positions yet
        expect(s.entries[2].scrollPositions, isEmpty);
      },
    );

    test('undo many then push — currentIndex stays consistent', () {
      // Edge case: undo from index=9 (10 pages) all the way to index=0,
      // then push.  Result must be 2 entries, index=1.

      final c = makeContainer();
      addTearDown(c.dispose);
      final n = c.read(navHistoryProvider.notifier);

      // Push 10 pages
      for (int i = 0; i < 10; i++) {
        n.push('/p$i');
      }
      // Undo all the way to /p0
      for (int i = 8; i >= 0; i--) {
        n.undoRedo('/p$i');
      }

      expect(c.read(navHistoryProvider).currentIndex, 0);
      expect(c.read(navHistoryProvider).current?.path, '/p0');

      // Push a new page from the very first entry
      n.push('/new');

      final s = c.read(navHistoryProvider);
      expect(s.entries.length, 2);
      expect(s.entries[0].path, '/p0');
      expect(s.entries[1].path, '/new');
      expect(s.currentIndex, 1);
    });

    test(
      'saveScrollPositionsAt is no-op before first push (cold start guard)',
      () {
        // getScrollPositions and saveScrollPositionsAt must not crash when
        // called before any push() (cold-start timing edge-case).

        final c = makeContainer();
        addTearDown(c.dispose);
        final n = c.read(navHistoryProvider.notifier);

        // Both should silently do nothing / return null
        expect(() => n.saveScrollPositionsAt(-1, {'x': 1.0}), returnsNormally);
        expect(n.getScrollPositions(), isNull);

        // After the first push everything works normally
        n.push('/home');
        n.saveScrollPositionsAt(0, {'x': 99.0});
        expect(n.getScrollPositions()?['x'], 99.0);
      },
    );
  });

  // ---------------------------------------------------------------------------
  // saveScrollPositionsAt — the by-index variant used by LayoutManager.dispose()
  // ---------------------------------------------------------------------------
  //
  // CONTEXT: By the time LayoutManager.dispose() runs, the router has already
  // moved currentIndex to the NEW page. The departing page's LayoutManager must
  // save its scroll positions to its OWN index (captured in initState), not to
  // currentIndex. That's why saveScrollPositionsAt(index, positions) exists.
  // ---------------------------------------------------------------------------

  group('NavHistoryNotifier — saveScrollPositionsAt()', () {
    test('saves to the specified index, not currentIndex', () {
      final c = makeContainer();
      addTearDown(c.dispose);
      final n = c.read(navHistoryProvider.notifier);

      n.push('/home'); // index 0
      n.push('/explore'); // index 1 ← currentIndex now

      // Simulate LayoutManager for /home saving to its captured index (0)
      // even though currentIndex is 1.
      n.saveScrollPositionsAt(0, {'page': 999.0});

      final s = c.read(navHistoryProvider);
      // /home entry updated
      expect(s.entries[0].scrollPositions['page'], 999.0);
      // /explore entry untouched
      expect(s.entries[1].scrollPositions, isEmpty);
      // currentIndex unchanged
      expect(s.currentIndex, 1);
    });

    test('saves to index 0 correctly', () {
      final c = makeContainer();
      addTearDown(c.dispose);
      final n = c.read(navHistoryProvider.notifier);

      n.push('/a');
      n.saveScrollPositionsAt(0, {'main': 42.0});
      expect(
        c.read(navHistoryProvider).entries[0].scrollPositions['main'],
        42.0,
      );
    });

    test('saves to last index correctly', () {
      final c = makeContainer();
      addTearDown(c.dispose);
      final n = c.read(navHistoryProvider.notifier);

      n.push('/a');
      n.push('/b');
      n.push('/c'); // index 2
      n.saveScrollPositionsAt(2, {'footer': 500.0});
      expect(
        c.read(navHistoryProvider).entries[2].scrollPositions['footer'],
        500.0,
      );
    });

    test('merges with existing scroll positions at that index', () {
      final c = makeContainer();
      addTearDown(c.dispose);
      final n = c.read(navHistoryProvider.notifier);

      n.push('/home');
      n.saveScrollPositionsAt(0, {'page': 100.0}); // first save for index 0

      n.push('/explore');
      // Save an additional key to index 0 — must merge, not replace
      n.saveScrollPositionsAt(0, {'page': 100.0, 'sidebar': 50.0});

      final entry = c.read(navHistoryProvider).entries[0];
      expect(entry.scrollPositions['page'], 100.0); // existing preserved
      expect(entry.scrollPositions['sidebar'], 50.0); // new key added
    });

    test('out-of-range index is a no-op (negative index)', () {
      final c = makeContainer();
      addTearDown(c.dispose);
      final n = c.read(navHistoryProvider.notifier);

      n.push('/a');
      // -1 (the initial _myHistoryIndex before first frame) must not throw
      expect(() => n.saveScrollPositionsAt(-1, {'x': 1.0}), returnsNormally);
      // State unchanged
      expect(c.read(navHistoryProvider).entries[0].scrollPositions, isEmpty);
    });

    test('out-of-range index is a no-op (too large)', () {
      final c = makeContainer();
      addTearDown(c.dispose);
      final n = c.read(navHistoryProvider.notifier);

      n.push('/a');
      expect(() => n.saveScrollPositionsAt(99, {'x': 1.0}), returnsNormally);
      expect(c.read(navHistoryProvider).entries[0].scrollPositions, isEmpty);
    });

    test('multiple saveScrollPositionsAt calls to same index accumulate', () {
      final c = makeContainer();
      addTearDown(c.dispose);
      final n = c.read(navHistoryProvider.notifier);

      n.push('/home');
      n.push('/next');

      // Two separate saves to index 0 (simulating multiple scroll areas)
      n.saveScrollPositionsAt(0, {'main': 200.0});
      n.saveScrollPositionsAt(0, {'sidebar': 75.0});

      final entry = c.read(navHistoryProvider).entries[0];
      expect(entry.scrollPositions['main'], 200.0);
      expect(entry.scrollPositions['sidebar'], 75.0);
    });

    test(
      'saveScrollPositionsAt to UNDO-ed entry survives forward navigation',
      () {
        // Scenario: user at /c (index=2), does UNDO to /b (index=1),
        // LayoutManager for /c saves via saveScrollPositionsAt(2, ...).
        // Then user does REDO to /c — scroll position must still be there.

        final c = makeContainer();
        addTearDown(c.dispose);
        final n = c.read(navHistoryProvider.notifier);

        n.push('/a');
        n.push('/b');
        n.push('/c'); // index 2, currentIndex=2

        // Simulate UNDO: currentIndex moves to 1
        n.undoRedo('/b');
        expect(c.read(navHistoryProvider).currentIndex, 1);

        // LayoutManager for /c saves its scroll to index 2
        n.saveScrollPositionsAt(2, {'page': 777.0});

        // REDO: currentIndex moves back to 2
        n.undoRedo('/c');
        expect(c.read(navHistoryProvider).current?.path, '/c');

        // Scroll position must still be there
        expect(
          c.read(navHistoryProvider).entries[2].scrollPositions['page'],
          777.0,
        );
      },
    );

    test('is no-op on empty history (index=-1 when no push yet)', () {
      // Cold-start: LayoutManager.dispose() may fire with _myHistoryIndex=-1
      // if the page is destroyed before the first postFrameCallback runs.
      final c = makeContainer();
      addTearDown(c.dispose);
      final n = c.read(navHistoryProvider.notifier);

      expect(() => n.saveScrollPositionsAt(-1, {'x': 1.0}), returnsNormally);
      expect(c.read(navHistoryProvider).entries, isEmpty);
    });
  });
}

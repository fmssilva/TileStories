// ignore_for_file: avoid_print, depend_on_referenced_packages

// =============================================================================
// PAGE STATE — UNIT TESTS
// =============================================================================
//
// These tests cover:
//   PageStateRegistry      — get/set/snapshot/restore lifecycle, all value types,
//                            edge cases (empty keys, null values, type casting)
//   HistoryEntry.pageState — field, copyWithPageState merge/overwrite, immutability
//   NavHistoryNotifier     — savePageStateAt, getPageState, cold-start guard,
//                            page state survives UNDO/REDO navigation,
//                            boundary conditions, trim interaction
//
// Run with:
//   flutter test lib/navigation/test/pageState/page_state_test.dart
//   flutter test lib/navigation/test/  (whole folder)
//
// =============================================================================

import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:grande_panorama_ar/layout/pageState/page_state_registry.dart';
import 'package:grande_panorama_ar/navigation/histConfig/history_entry.dart';
import 'package:grande_panorama_ar/navigation/histConfig/history_provider.dart';

void main() {
  // ---------------------------------------------------------------------------
  // Helpers
  // ---------------------------------------------------------------------------

  /// Fresh isolated container for each test — exactly like the app at runtime
  ProviderContainer makeContainer() => ProviderContainer();

  // ---------------------------------------------------------------------------
  // PageStateRegistry unit tests
  // ---------------------------------------------------------------------------

  group('PageStateRegistry', () {
    test('get() returns null for unknown key on fresh registry', () {
      final reg = PageStateRegistry();
      expect(reg.get('tab'), isNull);
      expect(reg.get('search'), isNull);
    });

    test('get() returns saved value after construction with savedState', () {
      final reg = PageStateRegistry(savedState: {'tab': 2, 'search': 'lisbon'});
      expect(reg.get('tab'), 2);
      expect(reg.get('search'), 'lisbon');
    });

    test('set() stores value in current state', () {
      final reg = PageStateRegistry();
      reg.set('tab', 1);
      expect(reg.snapshot['tab'], 1);
    });

    test('snapshot returns all values written via set()', () {
      final reg = PageStateRegistry();
      reg.set('tab', 3);
      reg.set('name', 'Alice');
      reg.set('checked', true);

      final snap = reg.snapshot;
      expect(snap['tab'], 3);
      expect(snap['name'], 'Alice');
      expect(snap['checked'], true);
    });

    test('snapshot is immutable (does not expose internal map)', () {
      final reg = PageStateRegistry();
      reg.set('tab', 0);
      final snap = reg.snapshot;
      expect(() => snap['tab'] = 99, throwsUnsupportedError);
    });

    test('set() overwrites a previously set key', () {
      final reg = PageStateRegistry();
      reg.set('tab', 0);
      reg.set('tab', 5);
      expect(reg.snapshot['tab'], 5);
    });

    test(
      'get() and set() are independent — get reads savedState, set writes currentState',
      () {
        final reg = PageStateRegistry(savedState: {'tab': 2});
        // Current state starts empty
        expect(reg.snapshot['tab'], isNull);
        // Saved state readable via get()
        expect(reg.get('tab'), 2);

        // Writing to current state does not change what get() returns
        reg.set('tab', 7);
        expect(reg.get('tab'), 2); // still reads from _savedState
        expect(reg.snapshot['tab'], 7); // current state updated
      },
    );

    test('restore() populates savedState so get() returns restored values', () {
      final reg = PageStateRegistry(); // starts empty
      expect(reg.get('tab'), isNull);

      reg.restore({'tab': 4, 'search': 'porto'});

      expect(reg.get('tab'), 4);
      expect(reg.get('search'), 'porto');
    });

    test('restore() merges with existing savedState', () {
      final reg = PageStateRegistry(savedState: {'tab': 1});
      reg.restore({'search': 'faro'});

      expect(reg.get('tab'), 1); // pre-existing key preserved
      expect(reg.get('search'), 'faro'); // new key added
    });

    test('restore() overwrites existing key in savedState', () {
      final reg = PageStateRegistry(savedState: {'tab': 1});
      reg.restore({'tab': 9});
      expect(reg.get('tab'), 9);
    });

    test('snapshot is empty on a fresh registry with no set() calls', () {
      final reg = PageStateRegistry(savedState: {'tab': 1});
      expect(reg.snapshot, isEmpty);
    });
  });

  // ---------------------------------------------------------------------------
  // HistoryEntry.pageState model tests
  // ---------------------------------------------------------------------------

  group('HistoryEntry — pageState field', () {
    test('creates with empty pageState by default', () {
      const e = HistoryEntry(path: '/home');
      expect(e.pageState, isEmpty);
    });

    test('creates with provided pageState', () {
      const e = HistoryEntry(
        path: '/home',
        pageState: {'tab': 2, 'search': 'lisbon'},
      );
      expect(e.pageState['tab'], 2);
      expect(e.pageState['search'], 'lisbon');
    });

    test('copyWithPageState merges maps', () {
      const e = HistoryEntry(path: '/home', pageState: {'tab': 1});
      final updated = e.copyWithPageState({'search': 'faro'});
      expect(updated.pageState['tab'], 1); // existing preserved
      expect(updated.pageState['search'], 'faro'); // new key added
      expect(updated.path, '/home'); // path unchanged
    });

    test('copyWithPageState overwrites existing key', () {
      const e = HistoryEntry(path: '/home', pageState: {'tab': 1});
      final updated = e.copyWithPageState({'tab': 5});
      expect(updated.pageState['tab'], 5);
    });

    test('copyWithPageState does not mutate original', () {
      const e = HistoryEntry(path: '/home', pageState: {'tab': 1});
      e.copyWithPageState({'tab': 99});
      expect(e.pageState['tab'], 1); // original unchanged
    });

    test('toString includes path', () {
      const e = HistoryEntry(path: '/about', pageState: {'tab': 2});
      expect(e.toString(), contains('/about'));
    });

    test('scrollPositions and pageState coexist independently', () {
      const e = HistoryEntry(
        path: '/x',
        scrollPositions: {'page': 100.0},
        pageState: {'tab': 3},
      );
      final updated = e.copyWithScrollPositions({'page': 200.0});
      expect(
        updated.pageState['tab'],
        3,
      ); // pageState preserved through scroll update
      expect(updated.scrollPositions['page'], 200.0);
    });
  });

  // ---------------------------------------------------------------------------
  // NavHistoryNotifier — page state methods
  // ---------------------------------------------------------------------------

  group('NavHistoryNotifier — page state', () {
    test('savePageStateAt() stores on specified entry', () {
      final c = makeContainer();
      addTearDown(c.dispose);
      final n = c.read(navHistoryProvider.notifier);

      n.push('/home');
      n.savePageStateAt(0, {'tab': 2, 'search': 'porto'});

      final entry = c.read(navHistoryProvider).current!;
      expect(entry.pageState['tab'], 2);
      expect(entry.pageState['search'], 'porto');
    });

    test('savePageStateAt() merges with existing pageState', () {
      final c = makeContainer();
      addTearDown(c.dispose);
      final n = c.read(navHistoryProvider.notifier);

      n.push('/home');
      n.savePageStateAt(0, {'tab': 1});
      n.savePageStateAt(0, {
        'tab': 1,
        'search': 'lisbon',
      }); // second call with full map

      final entry = c.read(navHistoryProvider).current!;
      expect(entry.pageState['tab'], 1);
      expect(entry.pageState['search'], 'lisbon');
    });

    test('getPageState() returns current entry page state', () {
      final c = makeContainer();
      addTearDown(c.dispose);
      final n = c.read(navHistoryProvider.notifier);

      n.push('/explore');
      n.savePageStateAt(0, {'tab': 3});

      expect(n.getPageState(), {'tab': 3});
    });

    test(
      'getPageState() returns null before first push (cold-start guard)',
      () {
        final c = makeContainer();
        addTearDown(c.dispose);
        final n = c.read(navHistoryProvider.notifier);

        expect(n.getPageState(), isNull);
      },
    );

    test('savePageStateAt() is no-op for negative index', () {
      final c = makeContainer();
      addTearDown(c.dispose);
      final n = c.read(navHistoryProvider.notifier);

      n.push('/a');
      expect(() => n.savePageStateAt(-1, {'tab': 1}), returnsNormally);
      expect(c.read(navHistoryProvider).entries[0].pageState, isEmpty);
    });

    test('savePageStateAt() is no-op for out-of-range index', () {
      final c = makeContainer();
      addTearDown(c.dispose);
      final n = c.read(navHistoryProvider.notifier);

      n.push('/a');
      expect(() => n.savePageStateAt(99, {'tab': 1}), returnsNormally);
      expect(c.read(navHistoryProvider).entries[0].pageState, isEmpty);
    });

    test('each history entry stores its own page state independently', () {
      final c = makeContainer();
      addTearDown(c.dispose);
      final n = c.read(navHistoryProvider.notifier);

      n.push('/a');
      n.savePageStateAt(0, {'tab': 0});

      n.push('/b');
      n.savePageStateAt(1, {'tab': 1, 'search': 'braga'});

      n.push('/c');
      n.savePageStateAt(2, {'tab': 2});

      final entries = c.read(navHistoryProvider).entries;
      expect(entries[0].pageState['tab'], 0);
      expect(entries[1].pageState['tab'], 1);
      expect(entries[1].pageState['search'], 'braga');
      expect(entries[2].pageState['tab'], 2);
    });

    test('page state survives UNDO/REDO navigation', () {
      final c = makeContainer();
      addTearDown(c.dispose);
      final n = c.read(navHistoryProvider.notifier);

      // Navigate to /home and set tab=1
      n.push('/home');
      n.savePageStateAt(0, {'tab': 1});

      // Navigate to /explore and set tab=2
      n.push('/explore');
      n.savePageStateAt(1, {'tab': 2});

      // UNDO back to /home
      n.undoRedo('/home');
      expect(n.getPageState()?['tab'], 1);

      // REDO back to /explore
      n.undoRedo('/explore');
      expect(n.getPageState()?['tab'], 2);
    });

    test('page state at specific index survives UNDO then push (trim)', () {
      final c = makeContainer();
      addTearDown(c.dispose);
      final n = c.read(navHistoryProvider.notifier);

      n.push('/a');
      n.savePageStateAt(0, {'tab': 0});
      n.push('/b');
      n.savePageStateAt(1, {'tab': 1});
      n.push('/c'); // index 2 — no state saved

      // Undo to /b, then push /z → /c trimmed
      n.undoRedo('/c');
      n.undoRedo('/b');
      n.push('/z');

      final entries = c.read(navHistoryProvider).entries;
      expect(entries.map((e) => e.path).toList(), ['/a', '/b', '/z']);
      // /a and /b preserve their page state after the trim
      expect(entries[0].pageState['tab'], 0);
      expect(entries[1].pageState['tab'], 1);
      // /z is brand new — no page state yet
      expect(entries[2].pageState, isEmpty);
    });

    test('page state and scroll positions coexist on the same entry', () {
      final c = makeContainer();
      addTearDown(c.dispose);
      final n = c.read(navHistoryProvider.notifier);

      n.push('/home');
      n.saveScrollPositionsAt(0, {'page': 500.0});
      n.savePageStateAt(0, {'tab': 3});

      final entry = c.read(navHistoryProvider).current!;
      expect(entry.scrollPositions['page'], 500.0);
      expect(entry.pageState['tab'], 3);
    });

    test('getPageState() returns correct state after multiple UNDO steps', () {
      final c = makeContainer();
      addTearDown(c.dispose);
      final n = c.read(navHistoryProvider.notifier);

      final pages = ['/a', '/b', '/c', '/d', '/e'];
      for (int i = 0; i < pages.length; i++) {
        n.push(pages[i]);
        n.savePageStateAt(i, {'tab': i});
      }
      // State: [/a(tab=0), /b(tab=1), /c(tab=2), /d(tab=3), /e(tab=4)], index=4

      n.undoRedo('/d');
      expect(n.getPageState()?['tab'], 3);

      n.undoRedo('/c');
      expect(n.getPageState()?['tab'], 2);

      n.undoRedo('/b');
      expect(n.getPageState()?['tab'], 1);

      n.undoRedo('/a');
      expect(n.getPageState()?['tab'], 0);

      // REDO
      n.undoRedo('/b');
      expect(n.getPageState()?['tab'], 1);

      n.undoRedo('/c');
      expect(n.getPageState()?['tab'], 2);
    });
  });

  // ---------------------------------------------------------------------------
  // PageStateRegistry — edge cases and value types
  // ---------------------------------------------------------------------------

  group('PageStateRegistry — edge cases', () {
    test('supports int values', () {
      final reg = PageStateRegistry();
      reg.set('count', 42);
      expect(reg.snapshot['count'], 42);
    });

    test('supports double values', () {
      final reg = PageStateRegistry();
      reg.set('progress', 0.75);
      expect(reg.snapshot['progress'], 0.75);
    });

    test('supports bool values', () {
      final reg = PageStateRegistry();
      reg.set('subscribed', true);
      reg.set('dismissed', false);
      expect(reg.snapshot['subscribed'], true);
      expect(reg.snapshot['dismissed'], false);
    });

    test('supports String values', () {
      final reg = PageStateRegistry();
      reg.set('query', 'braga');
      expect(reg.snapshot['query'], 'braga');
    });

    test('supports List values', () {
      final reg = PageStateRegistry();
      reg.set('selected', [1, 2, 3]);
      expect(reg.snapshot['selected'], [1, 2, 3]);
    });

    test('supports Map values (nested state)', () {
      final reg = PageStateRegistry();
      reg.set('form', {'name': 'Alice', 'age': 30});
      final snap = reg.snapshot['form'] as Map;
      expect(snap['name'], 'Alice');
      expect(snap['age'], 30);
    });

    test('supports null values (explicit null stored and retrieved)', () {
      final reg = PageStateRegistry();
      reg.set('optional', null);
      // Key is present in snapshot but value is null
      expect(reg.snapshot.containsKey('optional'), true);
      expect(reg.snapshot['optional'], isNull);
    });

    test('get() returns null for key that was never set OR saved', () {
      final reg = PageStateRegistry(savedState: {'a': 1});
      expect(reg.get('b'), isNull); // 'b' was never saved
    });

    test('type cast pattern — int tab index', () {
      final reg = PageStateRegistry(savedState: {'tab': 2});
      final tab = (reg.get('tab') as int?) ?? 0;
      expect(tab, 2);
    });

    test('type cast pattern — defaults to 0 when missing', () {
      final reg = PageStateRegistry(); // no saved state
      final tab = (reg.get('tab') as int?) ?? 0;
      expect(tab, 0); // uses default
    });

    test('type cast pattern — String field defaults to empty', () {
      final reg = PageStateRegistry();
      final query = (reg.get('search') as String?) ?? '';
      expect(query, '');
    });

    test('multiple set() calls accumulate all keys in snapshot', () {
      final reg = PageStateRegistry();
      reg.set('a', 1);
      reg.set('b', 'hello');
      reg.set('c', true);
      expect(reg.snapshot.length, 3);
    });

    test(
      'snapshot is stable — two calls return equal (but not identical) maps',
      () {
        final reg = PageStateRegistry();
        reg.set('tab', 0);
        final snap1 = reg.snapshot;
        final snap2 = reg.snapshot;
        expect(snap1, equals(snap2));
        // Each call returns a new unmodifiable view
        expect(identical(snap1, snap2), isFalse);
      },
    );

    test('restore() with empty map is a no-op', () {
      final reg = PageStateRegistry(savedState: {'tab': 1});
      reg.restore({});
      expect(reg.get('tab'), 1);
    });

    test('freshly constructed registry has empty snapshot', () {
      final reg = PageStateRegistry();
      expect(reg.snapshot, isEmpty);
    });
  });

  // ---------------------------------------------------------------------------
  // HistoryEntry — additional edge cases
  // ---------------------------------------------------------------------------

  group('HistoryEntry — additional edge cases', () {
    test('pageState default is const empty map (not null)', () {
      const e = HistoryEntry(path: '/x');
      expect(e.pageState, isNotNull);
      expect(e.pageState, isEmpty);
    });

    test('copyWithPageState is pure — does not share state between copies', () {
      const original = HistoryEntry(path: '/x', pageState: {'tab': 0});
      final copy1 = original.copyWithPageState({'tab': 1});
      final copy2 = original.copyWithPageState({'tab': 2});

      // Copies are independent
      expect(copy1.pageState['tab'], 1);
      expect(copy2.pageState['tab'], 2);
      // Original untouched
      expect(original.pageState['tab'], 0);
    });

    test('copyWith* preserve all other fields', () {
      const e = HistoryEntry(
        path: '/page',
        scrollPositions: {'page': 150.0},
        pageState: {'tab': 2},
      );

      // Updating scroll does NOT lose pageState
      final withScroll = e.copyWithScrollPositions({'page': 300.0});
      expect(withScroll.pageState['tab'], 2);
      expect(withScroll.path, '/page');

      // Updating pageState does NOT lose scrollPositions
      final withState = e.copyWithPageState({'tab': 3});
      expect(withState.scrollPositions['page'], 150.0);
      expect(withState.path, '/page');
    });
  });

  // ---------------------------------------------------------------------------
  // NavHistoryNotifier — boundary conditions
  // ---------------------------------------------------------------------------

  group('NavHistoryNotifier — page state boundary conditions', () {
    test(
      'savePageStateAt before any push is a no-op (index 0 out of range)',
      () {
        final c = makeContainer();
        addTearDown(c.dispose);
        final n = c.read(navHistoryProvider.notifier);

        // No pushes yet — entries is empty
        expect(() => n.savePageStateAt(0, {'tab': 1}), returnsNormally);
        expect(c.read(navHistoryProvider).entries, isEmpty);
      },
    );

    test('savePageStateAt exactly at last valid index', () {
      final c = makeContainer();
      addTearDown(c.dispose);
      final n = c.read(navHistoryProvider.notifier);

      n.push('/a');
      n.push('/b');
      n.push('/c'); // index 2 — last valid
      n.savePageStateAt(2, {'tab': 7});

      expect(c.read(navHistoryProvider).entries[2].pageState['tab'], 7);
    });

    test('savePageStateAt one beyond last valid index is a no-op', () {
      final c = makeContainer();
      addTearDown(c.dispose);
      final n = c.read(navHistoryProvider.notifier);

      n.push('/a');
      n.push('/b'); // last is index 1
      expect(() => n.savePageStateAt(2, {'tab': 1}), returnsNormally);
      // Entries unchanged
      expect(c.read(navHistoryProvider).entries[0].pageState, isEmpty);
      expect(c.read(navHistoryProvider).entries[1].pageState, isEmpty);
    });

    test(
      'page state at index 0 survives when max entries is reached and oldest is trimmed',
      () {
        final c = makeContainer();
        addTearDown(c.dispose);
        final n = c.read(navHistoryProvider.notifier);

        // Fill to maxEntries
        for (int i = 0; i < maxEntries; i++) {
          n.push('/p$i');
        }
        // Save state on the first entry (index 0)
        n.savePageStateAt(0, {'marker': 'first'});

        // Push one more — triggers trim (oldest removed)
        n.push('/extra');

        // The old index 0 (/p0) is now gone; index 0 is now /p1 with no state
        final entries = c.read(navHistoryProvider).entries;
        expect(entries.length, maxEntries);
        expect(entries.first.path, '/p1'); // /p0 trimmed
        expect(entries.first.pageState, isEmpty); // /p1 never had state
      },
    );

    test(
      'page state written at index N is not affected by writes to index M',
      () {
        final c = makeContainer();
        addTearDown(c.dispose);
        final n = c.read(navHistoryProvider.notifier);

        n.push('/a');
        n.push('/b');
        n.savePageStateAt(0, {'tab': 1}); // write to index 0
        n.savePageStateAt(1, {'tab': 2}); // write to index 1

        // Re-write index 1 — must not affect index 0
        n.savePageStateAt(1, {'tab': 99, 'extra': 'x'});

        expect(c.read(navHistoryProvider).entries[0].pageState['tab'], 1);
        expect(c.read(navHistoryProvider).entries[1].pageState['tab'], 99);
      },
    );

    test(
      'page state is empty on a fresh push even after a trimming operation',
      () {
        final c = makeContainer();
        addTearDown(c.dispose);
        final n = c.read(navHistoryProvider.notifier);

        n.push('/a');
        n.savePageStateAt(0, {'tab': 5});
        n.push('/b');

        // Undo then push new page — trims /b
        n.undoRedo('/a');
        n.push('/z');

        // /z is a fresh entry — no saved page state
        final entries = c.read(navHistoryProvider).entries;
        expect(entries.last.path, '/z');
        expect(entries.last.pageState, isEmpty);
      },
    );
  });
}

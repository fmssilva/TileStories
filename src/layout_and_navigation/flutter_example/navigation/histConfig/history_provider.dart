import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'history_entry.dart';
import 'history_state.dart';

// =============================================================================
// NAVIGATION HISTORY PROVIDER
// =============================================================================

/// Maximum number of history entries to keep in memory
/// Prevents unbounded growth for long browsing sessions
const maxEntries = 50;

/// Main provider for navigation history state
/// Manages the history stack with back/forward navigation and bookmarks
final navHistoryProvider =
    NotifierProvider<NavHistoryNotifier, NavHistoryState>(
      NavHistoryNotifier.new,
    );

class NavHistoryNotifier extends Notifier<NavHistoryState> {
  @override
  NavHistoryState build() => const NavHistoryState();

  // ═══════════════════════════════════════════════════════════════════════════
  // STAGED SAVES  (dispose-safe, no Riverpod state mutation)
  // ═══════════════════════════════════════════════════════════════════════════
  //
  // LayoutManager.dispose() fires inside Flutter's buildScope. Riverpod (in
  // debug mode) throws if `state =` is called during buildScope. To work around
  // this we write the data into a plain Dart Map here (no `state` mutation),
  // and flush it at the very start of push() / undoRedo(), which are always
  // called from a postFrameCallback — safely outside buildScope.
  //
  // Key: history entry index.  Value: the data to save.
  final Map<int, ({Map<String, double> scroll, Map<String, dynamic> pageState})>
  _staged = {};

  /// Save scroll + pageState data for [index].
  ///
  /// When dispose() fires during animation completion (outside Flutter's
  /// buildScope), we can write directly to Riverpod state.
  ///
  /// When dispose() fires during buildScope (debug-mode immediate transitions,
  /// e.g. Chrome), Riverpod throws an assertion — we catch it and fall back to
  /// staging.  Staged data is flushed at the start of the next push()/undoRedo().
  void stageSave(
    int index,
    Map<String, double> scrollPositions,
    Map<String, dynamic> pageState,
  ) {
    if (index < 0) return;
    try {
      _doSave(index, scrollPositions, pageState);
    } catch (_) {
      _staged[index] = (scroll: scrollPositions, pageState: pageState);
    }
  }

  void _doSave(
    int index,
    Map<String, double> scroll,
    Map<String, dynamic> pageState,
  ) {
    final currState = state;
    if (index < 0 || index >= currState.entries.length) return;
    final updated = List<HistoryEntry>.from(currState.entries);
    updated[index] = updated[index]
        .copyWithScrollPositions(scroll)
        .copyWithPageState(pageState);
    state = currState.copyWith(entries: updated);
  }

  void _flushStagedSaves() {
    if (_staged.isEmpty) return;
    final entries =
        Map<
          int,
          ({Map<String, double> scroll, Map<String, dynamic> pageState})
        >.from(_staged);
    _staged.clear();

    var currState = state;
    for (final kv in entries.entries) {
      final idx = kv.key;
      if (idx < 0 || idx >= currState.entries.length) continue;
      final updated = List<HistoryEntry>.from(currState.entries);
      updated[idx] = updated[idx]
          .copyWithScrollPositions(kv.value.scroll)
          .copyWithPageState(kv.value.pageState);
      currState = currState.copyWith(entries: updated);
    }
    state = currState;
  }

  /// Add a new forward entry to the history stack.
  ///
  /// Called by router redirect when isNavigatingProvider is true (user clicked a link).
  ///
  /// FORWARD TRIM: If the user went BACK several times (UNDO) and then navigates
  /// to a new page, we discard the now-stale "future" entries.
  /// Example: [Home, Panorama, Demo] with currentIndex=1 (Panorama).
  ///   push('/contact') → entries become [Home, Panorama, Contact] (Demo discarded).
  ///
  /// OVERFLOW TRIM: If we exceed maxEntries, the oldest entry is removed from
  /// the front of the list (currentIndex is adjusted accordingly).
  void push(String path) {
    _flushStagedSaves();
    final currHistState = state;
    final currIndex = currHistState.currentIndex;

    // Discard any "future" entries that are no longer reachable after this push.
    final toKeep = currIndex >= 0
        ? currHistState.entries.sublist(0, currIndex + 1)
        : <HistoryEntry>[];

    // Append new entry
    final updated = [...toKeep, HistoryEntry(path: path)];

    // If over limit, trim oldest entries
    final trimmed = updated.length > maxEntries
        ? updated.sublist(1, updated.length)
        : updated;

    state = currHistState.copyWith(
      entries: trimmed,
      currentIndex: trimmed.length - 1,
    );

    return;
  }

  /// Move the currentIndex back (UNDO) or forward (REDO) to match [path].
  ///
  /// Called by router redirect when isNavigatingProvider is false (browser BACK/FORWARD).
  /// Does NOT add new entries — just shifts the pointer in the existing stack.
  ///
  /// UNDO-priority: If [path] matches BOTH the previous AND next entry
  /// (e.g. history [A, B, A] with index at position 2), we always prefer UNDO
  /// so the user moves back toward B, not forward into a loop.
  void undoRedo(String path) {
    _flushStagedSaves();
    final currHistState = state;

    // Check if we want to UNDO (go back)
    // Priority: UNDO takes precedence if path matches both prev and next
    // (e.g. A → B → A: pressing UNDO from the second A should go back to B, not forward)
    final prevEntry = currHistState.previous;
    if (prevEntry?.path == path) {
      final newIdx = currHistState.currentIndex - 1;
      state = currHistState.copyWith(currentIndex: newIdx);
      return;
    }

    // Check REDO (go forward) — only if UNDO didn't match
    final nextEntry = currHistState.next;
    if (nextEntry?.path == path) {
      final newIdx = currHistState.currentIndex + 1;
      state = currHistState.copyWith(currentIndex: newIdx);
    }

    return;
  }

  // ═══════════════════════════════════════════════════════════════════════════
  // SCROLL POSITIONS
  // ═══════════════════════════════════════════════════════════════════════════

  // Save scroll positions to a specific history entry by index.
  //
  // Used by LayoutManager.dispose() — by the time dispose() runs, currentIndex
  // has already moved to the new page. We therefore record the page's own index
  // (captured in initState) and save to that specific slot, not to currentIndex.
  //
  // No-ops silently if the index is out of range.
  void saveScrollPositionsAt(int index, Map<String, double> positions) {
    final currHistState = state;
    if (index < 0 || index >= currHistState.entries.length) return;

    final updated = List<HistoryEntry>.from(currHistState.entries);
    updated[index] = updated[index].copyWithScrollPositions(positions);
    state = currHistState.copyWith(entries: updated);
  }

  // Get scroll positions for the current history entry.
  // Merges any pending staged save so the restored page always sees the
  // latest data even if the leaving page hasn't been flushed yet.
  // Returns null if history is empty (cold-start edge-case).
  Map<String, double>? getScrollPositions() {
    final currHistState = state;
    final idx = currHistState.currentIndex;
    if (idx < 0) return null;
    final saved = currHistState.entries[idx].scrollPositions;
    // Merge with any pending staged save for this index
    final staged = _staged[idx]?.scroll;
    final result = (staged != null && staged.isNotEmpty) ? staged : saved;
    return result;
  }

  // ═══════════════════════════════════════════════════════════════════════════
  // PAGE STATE
  // ═══════════════════════════════════════════════════════════════════════════

  // Save arbitrary page state to a specific history entry by index.
  //
  // Used by LayoutManager.dispose() — same timing rationale as
  // saveScrollPositionsAt: by dispose time currentIndex has moved forward.
  //
  // [state] can contain anything serialisable: tab indices, form values, etc.
  // No-ops silently if the index is out of range.
  void savePageStateAt(int index, Map<String, dynamic> pageState) {
    final currHistState = state;
    if (index < 0 || index >= currHistState.entries.length) return;

    final updated = List<HistoryEntry>.from(currHistState.entries);
    updated[index] = updated[index].copyWithPageState(pageState);
    state = currHistState.copyWith(entries: updated);
  }

  // Get the page state of the current entry.
  // Merges any pending staged save so the restored page always sees the
  // latest data even if the leaving page hasn't been flushed yet.
  // Returns null if history is empty (cold-start edge-case).
  Map<String, dynamic>? getPageState() {
    final currHistState = state;
    final idx = currHistState.currentIndex;
    if (idx < 0) return null;
    final saved = currHistState.entries[idx].pageState;
    // Merge with any pending staged save for this index
    final staged = _staged[idx]?.pageState;
    final result = (staged != null && staged.isNotEmpty) ? staged : saved;
    return result;
  }

  // ═══════════════════════════════════════════════════════════════════════════
  // NAVIGATION HELPERS for UNDO/REDO buttons
  // ═══════════════════════════════════════════════════════════════════════════

  /// Get the path to go back to (for UNDO button)
  /// Returns null if can't go back
  String? getBackPath() {
    return state.previous?.path;
  }

  /// Get the path to go forward to (for REDO button)
  /// Returns null if can't go forward
  String? getForwardPath() {
    return state.next?.path;
  }
}

// =============================================================================
// CONVENIENCE PROVIDERS
// =============================================================================

/// Whether back navigation is available — useful for enabling/disabling UNDO button
final canGoBackProvider = Provider<bool>((ref) {
  return ref.watch(navHistoryProvider).canGoBack;
});

/// Whether forward navigation is available — useful for enabling/disabling REDO button
final canGoForwardProvider = Provider<bool>((ref) {
  return ref.watch(navHistoryProvider).canGoForward;
});

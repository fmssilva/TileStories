import 'history_entry.dart';

///
/// Navigation History State
///
/// Immutable state that holds the complete navigation history.
/// - [entries] - List of all navigation history entries
/// - [currentIndex] - Index of the currently viewed page (-1 means empty)
class NavHistoryState {
  final List<HistoryEntry> entries;
  final int currentIndex; // -1 means empty

  const NavHistoryState({this.entries = const [], this.currentIndex = -1});

  // ═══════════════════════════════════════════════════════════════════════════
  // CHECKERS
  // ═══════════════════════════════════════════════════════════════════════════

  /// checkers, ex to help make the UNDO/REDO buttons active/clicable or not
  bool get canGoBack => currentIndex > 0;
  bool get canGoForward => currentIndex < entries.length - 1;

  // ═══════════════════════════════════════════════════════════════════════════
  // CONVENIENCE GETTERS
  // ═══════════════════════════════════════════════════════════════════════════

  /// Get the current entry the user is viewing
  HistoryEntry? get current =>
      (currentIndex >= 0 && currentIndex < entries.length)
      ? entries[currentIndex]
      : null;

  /// Get the previous entry (one step back)
  HistoryEntry? get previous =>
      (currentIndex > 0) ? entries[currentIndex - 1] : null;

  /// Get the next entry (one step forward for redo)
  HistoryEntry? get next =>
      (currentIndex < entries.length - 1) ? entries[currentIndex + 1] : null;

  // ═══════════════════════════════════════════════════════════════════════════
  // COPYWITH
  // ═══════════════════════════════════════════════════════════════════════════
  // Manual copyWith implementation (no Freezed needed, only 4 fields)
  NavHistoryState copyWith({List<HistoryEntry>? entries, int? currentIndex}) {
    return NavHistoryState(
      entries: entries ?? this.entries,
      currentIndex: currentIndex ?? this.currentIndex,
    );
  }
}

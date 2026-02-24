///
/// History Entry Model
///
/// Represents a single entry in the navigation history stack.
/// Stores everything needed to restore the user's exact state when they UNDO.
///
/// FIELDS:
/// - [path]            Route path (e.g., '/', '/explore')
/// - [scrollPositions] Scroll offset per named scroll area (key → pixels).
///                     Key 'page' is the outer scroll managed by LayoutManager.
///                     Additional keys are inner scroll areas registered by the page.
/// - [pageState]       Arbitrary page state (tab index, form values, etc.).
///                     Saved and restored by the page via PageStateRegistry.
class HistoryEntry {
  final String path;
  final Map<String, double> scrollPositions;
  final Map<String, dynamic> pageState;

  const HistoryEntry({
    required this.path,
    this.scrollPositions = const {},
    this.pageState = const {},
  });

  /// Returns a new entry with [positions] merged into [scrollPositions].
  HistoryEntry copyWithScrollPositions(Map<String, double> positions) =>
      HistoryEntry(
        path: path,
        scrollPositions: {...scrollPositions, ...positions},
        pageState: pageState,
      );

  /// Returns a new entry with [newPageState] merged into [pageState].
  HistoryEntry copyWithPageState(Map<String, dynamic> newPageState) =>
      HistoryEntry(
        path: path,
        scrollPositions: scrollPositions,
        pageState: {...pageState, ...newPageState},
      );

  @override
  String toString() =>
      'HistoryEntry($path, scrolls: $scrollPositions, pageState: $pageState)';
}

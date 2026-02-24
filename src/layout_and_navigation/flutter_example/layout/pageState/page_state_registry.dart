///
/// Page State Registry
///
/// Stores arbitrary key-value page state for a single page, enabling
/// restoration when the user returns via UNDO/REDO navigation.
///
/// WHAT "page state" means:
/// - Selected tab index: {'tab': 2}
/// - Search field text:  {'search': 'lisbon'}
/// - Form values:        {'name': 'Alice', 'age': 30}
/// - Any other UI state that the user would expect to be preserved on UNDO
///
/// PHILOSOPHY:
/// Mirrors [ScrollRegistry]. Pages don't manage state storage directly.
/// They read and write named values via the registry. LayoutManager handles
/// the entire lifecycle — creation, restoration, and persistence.
///
/// LIFECYCLE:
/// 1. LayoutManager creates PageStateRegistry with savedState from history
/// 2. Page reads initial values via get(key) → used as widget initial values
/// 3. Page writes new values via set(key, value) → called inside setState / notifier
/// 4. On LayoutManager dispose: snapshot() is saved to history (by LayoutManager)
///
/// USAGE (from a page widget):
/// ```dart
/// final pageState = PageStateRegistryProvider.of(context);
///
/// // Read saved value (e.g. for DefaultTabController initialIndex)
/// final savedTab = (pageState.get('tab') as int?) ?? 0;
///
/// // Write on change (e.g. in onTap callback)
/// pageState.set('tab', newIndex);
/// ```
class PageStateRegistry {
  /// Saved state from the previous visit (used to restore on UNDO/REDO).
  /// Mutable so that [restore] can populate it in the postFrameCallback,
  /// matching how [ScrollRegistry.restorePositions] works.
  final Map<String, dynamic> _savedState = {};

  /// Live state written by the page during this visit.
  /// Written via [set], read back via [snapshot] by LayoutManager on dispose.
  final Map<String, dynamic> _currentState = {};

  /// Create a registry, optionally pre-populated with [savedState].
  /// Pass an empty map (or omit) on first visit.
  /// The postFrameCallback will call [restore] with the real saved data.
  PageStateRegistry({Map<String, dynamic> savedState = const {}}) {
    _savedState.addAll(savedState);
  }

  /// Merges [savedState] into the internal saved-state map.
  ///
  /// Called by LayoutManager's postFrameCallback once the history entry is
  /// known. Mirrors [ScrollRegistry.restorePositions].
  ///
  /// Note: page widgets that read restored values MUST read from [get] inside
  /// their own [State.initState] or [State.didChangeDependencies] after the
  /// provider is available, NOT inside [build] — because [build] has already
  /// run by the time this fires. For tab/form restoration, use a
  /// [StatefulWidget] that reads from the registry in its own initState.
  void restore(Map<String, dynamic> savedState) {
    _savedState.addAll(savedState);
  }

  /// Returns the saved value for [key] from the previous visit, or null if
  /// this is a fresh visit or the key was never saved.
  ///
  /// Pages use this to initialise widgets:
  ///   final savedTab = (pageState.get('tab') as int?) ?? 0;
  dynamic get(String key) => _savedState[key];

  /// Saves [value] under [key] for the current visit.
  ///
  /// Pages call this when the user changes state (tab switch, form input, etc.).
  /// The value will be persisted to history when LayoutManager disposes this page.
  void set(String key, dynamic value) {
    _currentState[key] = value;
  }

  /// Returns all state written during this visit.
  /// Called by LayoutManager on dispose to persist state to history.
  Map<String, dynamic> get snapshot => Map.unmodifiable(_currentState);
}

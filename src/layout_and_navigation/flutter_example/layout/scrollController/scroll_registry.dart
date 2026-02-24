import 'package:flutter/material.dart';

///
/// Scroll Registry
///
/// Manages [ScrollController]s for a page, providing automatic:
/// - Controller creation on demand with position restoration
/// - Scroll position caching while the user scrolls
/// - Controller disposal
///
/// PHILOSOPHY:
/// Pages don't manage ScrollControllers directly. They request them by ID
/// from the registry, which handles the entire lifecycle automatically.
///
/// LIFECYCLE:
/// 1. LayoutManager creates ScrollRegistry with savedPositions from history
/// 2. Page requests controllers via controller(id)
/// 3. Registry creates controller with initialScrollOffset from savedPositions
/// 4. A listener caches the current position on every scroll event
/// 5. On LayoutManager dispose: cachedPositions are saved to history (by LayoutManager)
/// 6. Registry.dispose() disposes all controllers
///
/// USAGE (from a page widget):
/// ```dart
/// final registry = ScrollRegistryProvider.of(context);
/// final scroll = registry.controller('tab-a');
/// return SingleChildScrollView(controller: scroll, child: Content());
/// ```
class ScrollRegistry {
  /// Map of scroll IDs → controllers
  final Map<String, ScrollController> _controllers = {};

  /// Scroll positions cached while the user scrolls (updated on every scroll event)
  final Map<String, double> _cachedPositions = {};

  /// Saved positions from the previous visit (used to restore on UNDO/REDO)
  final Map<String, double> _savedPositions;

  /// Create a registry with the saved scroll positions for this page.
  /// Pass an empty map on first visit; pass the history entry's positions on UNDO/REDO.
  ScrollRegistry({required Map<String, double> savedPositions})
    : _savedPositions = savedPositions;

  /// Get or create a controller for a given scroll ID.
  ///
  /// On first call for an ID: creates a controller with initialScrollOffset
  /// set to the saved position (so the page renders already scrolled to the right spot).
  /// On subsequent calls: returns the existing controller.
  ScrollController controller(String id) {
    if (_controllers.containsKey(id)) {
      return _controllers[id]!;
    }

    // Restore previous scroll position via initialScrollOffset
    final initialOffset = _savedPositions[id] ?? 0.0;
    final ctrl = ScrollController(initialScrollOffset: initialOffset);
    _controllers[id] = ctrl;

    // Cache the position on every scroll event so we can save it on dispose
    ctrl.addListener(() {
      if (ctrl.hasClients) {
        _cachedPositions[id] = ctrl.position.pixels;
      }
    });

    return ctrl;
  }

  /// Returns all currently cached scroll positions.
  /// Called by LayoutManager before disposing, to persist positions to history.
  Map<String, double> get cachedPositions => Map.unmodifiable(_cachedPositions);

  /// Restores scroll positions by jumping all existing controllers to their saved offsets.
  ///
  /// Called by LayoutManager after the first frame when history has been updated
  /// (redirect deferred its push/undoRedo, so scroll restoration must also be deferred).
  /// Uses jumpTo() because controllers are already created — initialScrollOffset
  /// only applies at construction time.
  void restorePositions(Map<String, double> positions) {
    for (final entry in positions.entries) {
      final ctrl = _controllers[entry.key];
      if (ctrl != null && ctrl.hasClients) {
        ctrl.jumpTo(entry.value);
      }
    }
  }

  /// Disposes all managed controllers.
  /// LayoutManager calls this after saving cachedPositions to history.
  void dispose() {
    for (final ctrl in _controllers.values) {
      ctrl.dispose();
    }
    _controllers.clear();
  }
}

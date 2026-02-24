import 'package:flutter/widgets.dart';

///
/// Nav Observer
///
/// A [NavigatorObserver] that hooks into GoRouter route changes to maintain
/// the current route state for nav-widget active-tab highlighting.
///
/// NOTE ON HISTORY (push vs undoRedo):
/// GoRouter's [NavigatorObserver] does NOT have access to Riverpod providers,
/// so it cannot read [isNavigatingProvider] to distinguish user-initiated
/// navigation from browser BACK/FORWARD.
/// History push/undoRedo is therefore handled in [router_config.dart] via
/// GoRouter's [redirect] callback, which runs with access to [ref] and can
/// read + consume the [isNavigatingProvider] flag set by nav widgets before
/// calling context.go().
///
/// This observer's sole responsibility is deduplication and forwarding the
/// destination path to [onRouteChanged] for current-route tracking.
///
/// TIMING:
/// [NavigatorObserver.didPush] is called during Navigator's mount/restoreState,
/// which happens inside the widget-tree build phase. Riverpod forbids provider
/// mutations during build. We therefore defer [onRouteChanged] with
/// [WidgetsBinding.addPostFrameCallback] so it fires after the frame is painted,
/// when the tree is idle and provider mutations are allowed.
///
/// DEDUPLICATION:
/// GoRouter fires multiple observer callbacks for the same logical navigation
/// (e.g. didPush + didReplace). We guard with [_lastProcessedPath] so
/// [onRouteChanged] is called exactly once per destination.
///
/// USAGE (in router_config.dart):
/// ```dart
/// NavObserver(
///   onRouteChanged: (path) => ref.read(currentRouteProvider.notifier).update(path),
/// )
/// ```
class NavObserver extends NavigatorObserver {
  /// Called once per route change with the new destination path.
  /// Always fires after the current frame is painted (postFrameCallback).
  final void Function(String path) onRouteChanged;

  /// Dedup guard — prevents firing twice for the same transition
  String? _lastProcessedPath;

  NavObserver({required this.onRouteChanged});

  // ─────────────────────────────────────────────────────────────────────────
  // NavigatorObserver overrides
  // ─────────────────────────────────────────────────────────────────────────

  @override
  void didPush(Route route, Route? previousRoute) {
    _handleRoute(route);
  }

  @override
  void didPop(Route route, Route? previousRoute) {
    _handleRoute(previousRoute);
  }

  @override
  void didReplace({Route? newRoute, Route? oldRoute}) {
    _handleRoute(newRoute);
  }

  @override
  void didRemove(Route route, Route? previousRoute) {
    _handleRoute(previousRoute);
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Core logic
  // ─────────────────────────────────────────────────────────────────────────

  void _handleRoute(Route? route) {
    final path = route?.settings.name;
    if (path == null || path.isEmpty) return;

    // Dedup: skip if already processed this path in this transition
    if (_lastProcessedPath == path) return;
    _lastProcessedPath = path;

    // Defer the provider mutation until after the current frame is built.
    // NavigatorObserver callbacks fire during Navigator.restoreState /
    // didChangeDependencies, which is inside the widget build phase.
    // Riverpod throws if we mutate a provider there.
    WidgetsBinding.instance.addPostFrameCallback((_) {
      // Reset the dedup guard AFTER the callback fires, not before.
      // This allows the same path to be re-reported on a future navigation.
      _lastProcessedPath = null;
      onRouteChanged(path);
    });
    // Ensure the scheduler knows a frame is pending so that pump() in tests
    // (and the real engine in production) will actually fire the callback.
    WidgetsBinding.instance.ensureVisualUpdate();
  }
}

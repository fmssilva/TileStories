import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'nav_item.dart';
import 'nav_config.dart';
import '../histConfig/is_navigating_provider.dart';
import '../../components/feedback/error_page.dart';
import '../histConfig/route_observer.dart';
import '../histConfig/history_provider.dart';
import 'current_route_provider.dart';

/// ROUTER CONFIGURATION
///
/// WHY go_router?
/// - Web support: Browser URLs work correctly
/// - Type safety: Compile-time route validation
/// - Deep linking: Share URLs that open specific pages
/// - Route guards: Protect pages requiring authentication
/// - Nested navigation: Support complex navigation flows

/// Create and configure the go_router instance.
/// All routes from [navigationConfig] are auto-generated.
/// [ref] gives access to Riverpod providers for history and route tracking.
GoRouter createRouter(WidgetRef ref) {
  return GoRouter(
    initialLocation: '/',

    // ─────────────────────────────────────────────────────────────────────
    // NAVIGATION HISTORY HOOK via redirect
    // ─────────────────────────────────────────────────────────────────────
    //
    // WHY redirect and not a NavigatorObserver?
    // NavigatorObserver has no access to Riverpod providers or to the
    // isNavigatingProvider flag — it can only see route names, not intent.
    // redirect fires before every navigation and has full access to [ref].
    //
    // HOW IT WORKS:
    // 1. Nav widgets (tabs, hamburger, back/forward buttons) set
    //    isNavigatingProvider = true immediately before calling context.go().
    // 2. Browser BACK/FORWARD never sets the flag — it arrives as false.
    // 3. redirect reads + consumes the flag (atomically resets to false).
    // 4. true  → user clicked a link        → push() adds a new history entry
    // 5. false → browser back/forward        → undoRedo() moves the index
    //
    // We always return null (no actual redirect), using this purely as an
    // observer hook that has access to both the flag and Riverpod [ref].
    //
    // TIMING NOTE: go_router fires redirect during its route-resolution phase,
    // which may overlap with Flutter's build cycle on the initial load.
    // Riverpod's debug mode throws if a provider is mutated during build.
    // We defer the mutation with addPostFrameCallback so it always fires after
    // the current frame is committed — safe for both initial and subsequent navs.
    //
    // SCROLL/PAGESTATE RESTORATION COMPATIBILITY:
    // LayoutManager.initState() reads scroll/pageState via getScrollPositions()
    // and getPageState(). Since these fire in the same build frame as redirect
    // (BEFORE the postFrameCallback runs), LayoutManager also uses
    // didChangeDependencies + addPostFrameCallback to apply restoration
    // AFTER history has been updated.
    redirect: (context, state) {
      final path = state.uri.toString();

      // Read + consume the isNavigating flag from Riverpod.
      // Nav widgets set this to true before calling context.go().
      // Browser BACK/FORWARD never sets it, so it arrives as false.
      // We consume (read + reset to false) atomically to prevent stale state.
      final isNavigating = ref.read(isNavigatingProvider.notifier).consume();

      WidgetsBinding.instance.addPostFrameCallback((_) {
        final historyNotifier = ref.read(navHistoryProvider.notifier);
        final historyState = ref.read(navHistoryProvider);

        if (historyState.currentIndex < 0) {
          historyNotifier.push(path);
        } else if (isNavigating) {
          historyNotifier.push(path);
        } else {
          historyNotifier.undoRedo(path);
        }
      });

      return null; // No redirect — we're only observing
    },

    // ─────────────────────────────────────────────────────────────────────
    // CURRENT ROUTE TRACKING via NavigatorObserver
    // ─────────────────────────────────────────────────────────────────────
    //
    // NavObserver tracks the active route so nav widgets can highlight
    // the active tab.
    // THIS IS NOT FOR HISTORY TO CALL PUSH OR UNDO/REDO — THAT IS handled in redirect above.
    observers: [
      NavObserver(
        onRouteChanged: (path) =>
            ref.read(currentRouteProvider.notifier).update(path),
      ),
    ],

    // ─────────────────────────────────────────────────────────────────────
    // ROUTES
    // ─────────────────────────────────────────────────────────────────────
    //
    // ShellRoute wraps all routes in a common shell. The builder here is a
    // pass-through (returns child unchanged) because layout is handled by each
    // page's own LayoutManager. The ShellRoute exists to give all routes a
    // shared Navigator and Overlay scope, which some widgets (e.g. tooltips,
    // dropdowns) require.
    routes: [
      ShellRoute(
        builder: (context, state, child) => child,
        routes: _generateGoRoutes(navigationConfig),
      ),
    ],

    // ─────────────────────────────────────────────────────────────────────
    // ERROR PAGE — shown when user navigates to an unknown route
    // ─────────────────────────────────────────────────────────────────────
    errorBuilder: (context, state) {
      return ErrorPage(
        error: state.error.toString(),
        path: state.uri.toString(),
      );
    },
  );
}

/// Recursively generate GoRoute list from NavItem configuration.
///
/// go_router requires nested child routes to use **relative** paths
/// (e.g., 'child-a' not '/demo-nav1/child-a').  We strip the parent path
/// prefix here so NavItem can keep convenient absolute paths for URL building.
List<GoRoute> _generateGoRoutes(
  List<NavItem> navItems, {
  String parentPath = '',
}) {
  return navItems.map((item) {
    // Compute the relative path for go_router nesting.
    // Top-level items keep their full path (parentPath is '').
    // Child items strip the parent prefix so go_router sees 'child-a',
    // not '/demo-nav1/child-a'.
    final relativePath = parentPath.isEmpty
        ? item.path
        : item.path.substring(parentPath.length).replaceFirst('/', '');

    return GoRoute(
      path: relativePath,
      builder: (context, state) => item.builder(context),
      routes: item.children.isNotEmpty
          ? _generateGoRoutes(item.children, parentPath: item.path)
          : [],
    );
  }).toList();
}

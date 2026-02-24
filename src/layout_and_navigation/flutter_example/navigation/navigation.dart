// ================================================================
// Navigation System - Barrel Export File
// ================================================================
//
// Single import point for the entire navigation system.
//
// Usage:
//   import 'package:grande_panorama_ar/navigation/navigation.dart';
//
// ================================================================

// -------------------- Widgets --------------------
export 'widgets/nav_tabs_row.dart';
export 'widgets/hamburger/hamburger.dart';
export 'widgets/hamburger/nav_accordion.dart';

// -------------------- Models --------------------
export 'navConfig/nav_item.dart';
export 'histConfig/history_entry.dart';
export 'histConfig/history_state.dart';

// -------------------- Providers --------------------
export 'widgets/hamburger/show_hamburger_provider.dart';
export 'histConfig/history_provider.dart'; // navHistoryProvider, canGoBackProvider, canGoForwardProvider
export 'navConfig/current_route_provider.dart'; // currentRouteProvider

// -------------------- Config --------------------
export 'navConfig/nav_config.dart';
export 'navConfig/router_config.dart';

// -------------------- Observers --------------------
export 'histConfig/route_observer.dart'; // NavObserver

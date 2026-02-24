import 'package:flutter_riverpod/flutter_riverpod.dart';

///
/// Current Route Provider
///
/// Tracks the currently active route path so navigation widgets
/// (NavTabsRow, NavAccordion) can highlight the active tab/item.
///
/// Updated by NavObserver after every route change.
///
/// USAGE:
/// ```dart
/// final currentRoute = ref.watch(currentRouteProvider);
/// final isActive = currentRoute == item.path;
/// ```
final currentRouteProvider = NotifierProvider<_CurrentRouteNotifier, String>(
  _CurrentRouteNotifier.new,
);

class _CurrentRouteNotifier extends Notifier<String> {
  @override
  String build() => '/';

  /// Called by NavObserver after every route change
  void update(String path) => state = path;
}

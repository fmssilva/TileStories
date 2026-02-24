import 'package:flutter/widgets.dart';
import 'page_state_registry.dart';

///
/// Page State Registry Provider (InheritedWidget)
///
/// Makes [PageStateRegistry] available to all widgets below [LayoutManager]
/// in the widget tree. Pages call [PageStateRegistryProvider.of(context)] to
/// get the registry, read saved state, and write new state.
///
/// Mirrors the design of [ScrollRegistryProvider].
///
/// USAGE:
/// ```dart
/// // In a page widget's build method:
/// final pageState = PageStateRegistryProvider.of(context);
/// final savedTab = (pageState.get('tab') as int?) ?? 0;
/// ```
class PageStateRegistryProvider extends InheritedWidget {
  final PageStateRegistry registry;

  const PageStateRegistryProvider({
    super.key,
    required this.registry,
    required super.child,
  });

  /// Returns the nearest [PageStateRegistry] in the widget tree.
  /// Throws an assertion error if called outside a [LayoutManager] subtree.
  static PageStateRegistry of(BuildContext context) {
    final provider = context
        .dependOnInheritedWidgetOfExactType<PageStateRegistryProvider>();
    assert(
      provider != null,
      'PageStateRegistryProvider.of() called outside a LayoutManager subtree.',
    );
    return provider!.registry;
  }

  @override
  bool updateShouldNotify(PageStateRegistryProvider oldWidget) =>
      // The registry object is the same instance for the life of a LayoutManager,
      // but its internal _savedState may have been populated via restore() after
      // the first build. Always notify so that didChangeDependencies re-fires on
      // children after LayoutManager calls setState() following restore().
      true;
}

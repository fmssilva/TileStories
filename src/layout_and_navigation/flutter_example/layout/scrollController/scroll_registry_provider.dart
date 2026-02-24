import 'package:flutter/material.dart';
import 'scroll_registry.dart';

///
/// Scroll Registry Provider (InheritedWidget)
///
/// Makes [ScrollRegistry] available to all widgets below [LayoutManager]
/// in the widget tree. Pages call [ScrollRegistryProvider.of(context)] to
/// get the registry and request scroll controllers by ID.
///
/// See [ScrollRegistry] for the full lifecycle documentation.
class ScrollRegistryProvider extends InheritedWidget {
  final ScrollRegistry registry;

  const ScrollRegistryProvider({
    required this.registry,
    required super.child,
    super.key,
  });

  /// Get the [ScrollRegistry] from context.
  ///
  /// Throws assertion error if [LayoutManager] is not in the widget tree.
  static ScrollRegistry of(BuildContext context) {
    final provider = context
        .dependOnInheritedWidgetOfExactType<ScrollRegistryProvider>();
    assert(
      provider != null,
      'No ScrollRegistryProvider found in context. '
      'Make sure LayoutManager is an ancestor of this widget.',
    );
    return provider!.registry;
  }

  @override
  bool updateShouldNotify(ScrollRegistryProvider oldWidget) =>
      registry != oldWidget.registry;
}

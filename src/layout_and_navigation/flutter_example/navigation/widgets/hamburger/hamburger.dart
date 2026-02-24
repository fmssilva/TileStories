import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../navConfig/nav_config.dart';
import 'nav_accordion.dart';

// =============================================================================
// HAMBURGER MENU BUTTON
// =============================================================================

///
/// Hamburger
///
/// A hamburger menu icon button that opens a drawer with navigation accordion.
///
/// This widget provides a simple way to add a hamburger menu to any page.
/// When tapped, it opens a drawer containing the full navigation structure
/// using NavDrawer which internally uses NavAccordion.
///
/// FEATURES:
/// - Opens drawer with full navigation
/// - Automatically closes drawer after navigation
/// - Uses brand colors for consistent styling
/// - Accessible with tooltip
///
class Hamburger extends ConsumerWidget {
  const Hamburger({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    return Semantics(
      container: true,
      button: true,
      label: 'Navigation menu',
      hint: 'Opens navigation drawer with all menu items',
      enabled: true,
      child: IconButton(
        icon: const Icon(Icons.menu),
        tooltip: 'Open navigation menu',
        onPressed: () => _openDrawer(context),
      ),
    );
  }

  /// Open the navigation drawer
  void _openDrawer(BuildContext context) {
    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      builder: (context) => DraggableScrollableSheet(
        initialChildSize: 0.9,
        minChildSize: 0.5,
        maxChildSize: 0.9,
        builder: (context, scrollController) =>
            NavDrawer(items: navigationConfig),
      ),
    );
  }
}

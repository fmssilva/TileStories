import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../design/design_system.dart';
import '../../navConfig/nav_item.dart';
import '../../navConfig/current_route_provider.dart';
import 'base_accordion.dart';

// =============================================================================
// NAVIGATION ACCORDION - THIN WRAPPER AROUND BASE_ACCORDION
// =============================================================================

///
/// Nav Accordion
///
/// An ultra-thin wrapper around BaseAccordion.
/// ALL accordion logic (filtering, sorting, navigation, i18n) is in BaseAccordion.
///
/// RESPONSIBILITIES:
/// - Watch current route provider
/// - Pass items directly to BaseAccordion
/// - Pass onNavigate callback
///
/// THAT'S IT! Everything else is handled by BaseAccordion.
///
/// USAGE:
/// ```dart
/// NavAccordion(
///   items: navigationConfig,
///   onNavigate: () => Navigator.pop(context), // Close drawer
/// )
/// ```
class NavAccordion extends ConsumerWidget {
  /// Navigation items to display (from navigationConfig)
  final List<NavItem> items;

  /// Callback when navigation occurs (e.g., to close drawer)
  final VoidCallback? onNavigate;

  const NavAccordion({super.key, required this.items, this.onNavigate});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    // Watch the current route to highlight active item
    final currentRoute = ref.watch(currentRouteProvider);

    // Pass everything to BaseAccordion - it handles all the logic
    return BaseAccordion(
      items: items,
      currentRoute: currentRoute,
      onNavigate: onNavigate,
    );
  }
}

// =============================================================================
// NAVIGATION DRAWER WITH ACCORDION
// =============================================================================

///
/// Nav Drawer
///
/// A complete drawer widget with navigation accordion.
/// Includes header with branding and close button, scrollable accordion content.
///
/// USAGE:
/// ```dart
/// Scaffold(
///   drawer: NavDrawer(items: navigationConfig),
///   ...
/// )
/// ```
class NavDrawer extends ConsumerWidget {
  /// Navigation items to display
  final List<NavItem> items;

  const NavDrawer({super.key, required this.items});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    return Drawer(
      child: Container(
        decoration: BoxDecoration(
          gradient: LinearGradient(
            begin: Alignment.topCenter,
            end: Alignment.bottomCenter,
            colors: [
              context.surface,
              BrandColors.deepBlue50.withValues(alpha: 0.1),
            ],
          ),
        ),
        child: SafeArea(
          child: Column(
            children: [
              // Drawer header with close button
              _buildDrawerHeader(context),

              const Divider(height: 1),

              // Navigation accordion (scrollable)
              Expanded(
                child: SingleChildScrollView(
                  padding: const EdgeInsets.all(Spacing.md),
                  child: NavAccordion(
                    items: items,
                    onNavigate: () {
                      // Close drawer after navigation
                      Navigator.of(context).pop();
                    },
                  ),
                ),
              ),

              // Drawer footer (optional)
              _buildDrawerFooter(context),
            ],
          ),
        ),
      ),
    );
  }

  /// Build drawer header with branding and close button
  Widget _buildDrawerHeader(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(Spacing.lg),
      decoration: const BoxDecoration(
        gradient: LinearGradient(
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
          colors: [BrandColors.deepBlue500, BrandColors.azulejoBlue500],
        ),
      ),
      child: Row(
        children: [
          // App icon
          const Icon(Icons.map, size: 32, color: BrandColors.gold200),
          const SizedBox(width: Spacing.md),

          // App name
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  'TileStories',
                  style: context.titleLarge.copyWith(
                    color: Colors.white,
                    fontWeight: FontWeight.w700,
                  ),
                ),
                Text(
                  'Grande Panorama',
                  style: context.bodySmall.copyWith(color: BrandColors.gold200),
                ),
              ],
            ),
          ),

          // Close button
          Semantics(
            container: true,
            button: true,
            label: 'Close navigation menu',
            hint: 'Double tap to close the navigation drawer',
            enabled: true,
            child: IconButton(
              icon: const Icon(Icons.close, color: Colors.white),
              onPressed: () => Navigator.of(context).pop(),
              tooltip: 'Close menu',
            ),
          ),
        ],
      ),
    );
  }

  /// Build drawer footer with version or links
  Widget _buildDrawerFooter(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(Spacing.md),
      decoration: BoxDecoration(
        border: Border(
          top: BorderSide(
            color: context.outline.withValues(alpha: 0.2),
            width: 1,
          ),
        ),
      ),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Text(
            'Version 1.0.0',
            style: context.bodySmall.copyWith(
              color: context.onSurface.withValues(alpha: 0.5),
            ),
          ),
        ],
      ),
    );
  }
}

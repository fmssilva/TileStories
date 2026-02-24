import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../design/design_system.dart';
import '../../utils/i18n/extensions/context_extensions.dart';
import '../navConfig/nav_config.dart';
import '../navConfig/nav_item.dart';
import '../histConfig/is_navigating_provider.dart';
import '../navConfig/current_route_provider.dart';
import 'hamburger/show_hamburger_provider.dart';
import 'hamburger/base_accordion.dart';

// =============================================================================
// NAVIGATION TABS ROW
// =============================================================================

///
/// Nav Tabs Row
///
/// Renders top-level navigation tabs in a horizontal row with responsive behavior.
/// Each tab can expand to show its children in a dropdown accordion.
///
/// FEATURES:
/// - Responsive: Adapts to available width
/// - Mini-accordions: Each tab with children shows chevron and dropdown
/// - Overflow handling: "More" button for tabs that don't fit
/// - Hamburger integration: Shows hamburger when < 2 tabs fit
/// - Active state: Highlights current route
///
/// RESPONSIVE BEHAVIOR:
/// - Available width >= all tabs: Show all tabs
/// - Available width < all tabs: Show tabs that fit + "More" dropdown
/// - Available width < 2 tabs + More (3 total): Set showHamburger = true, return empty
///
/// USAGE:
/// ```dart
/// LayoutBuilder(
///   builder: (context, constraints) => NavTabsRow(
///     availableWidth: constraints.maxWidth,
///   ),
/// )
/// ```
///
class NavTabsRow extends ConsumerStatefulWidget {
  /// Available width for rendering tabs
  final double availableWidth;

  const NavTabsRow({super.key, required this.availableWidth});

  @override
  ConsumerState<NavTabsRow> createState() => _NavTabsRowState();
}

class _NavTabsRowState extends ConsumerState<NavTabsRow> {
  /// Width for the "More" button
  static const double moreButtonWidth = 80.0;

  /// Padding around tabs
  static const double tabPadding = Spacing.md;

  /// Cache for tab widths to avoid repeated TextPainter calculations
  /// Key format: "{label}_{hasChildren}_{locale}"
  final Map<String, double> _widthCache = {};

  /// Track last locale to clear cache on locale change
  String? _lastLocale;

  /// Focus node for keyboard navigation
  final FocusNode _focusNode = FocusNode();

  /// Currently focused tab index (-1 for More button)
  int _focusedIndex = 0;

  @override
  void dispose() {
    _focusNode.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    // Clear cache if locale changed
    final currentLocale = Localizations.localeOf(context).toString();
    if (_lastLocale != null && _lastLocale != currentLocale) {
      _widthCache.clear();
    }
    _lastLocale = currentLocale;

    // Get current route to highlight active tab
    final currentRoute = ref.watch(currentRouteProvider);

    // Get visible navigation items (top-level only)
    final visibleItems =
        navigationConfig
            .where((item) => item.metadata?.showInNav ?? true)
            .toList()
          ..sort((a, b) {
            final orderA = a.metadata?.order ?? 0;
            final orderB = b.metadata?.order ?? 0;
            return orderA.compareTo(orderB);
          });

    // Calculate how many tabs can fit
    final fitsCalculation = _calculateTabFits(
      context: context,
      availableWidth: widget.availableWidth,
      items: visibleItems,
    );

    // Determine if hamburger should be shown
    final shouldShowHamburger =
        fitsCalculation.visibleCount < 2 ||
        (fitsCalculation.visibleCount == 2 && !fitsCalculation.needsMore);

    // Update hamburger visibility IMMEDIATELY (not in postFrameCallback)
    final currentHamburgerState = ref.read(showHamburgerProvider);
    if (shouldShowHamburger != currentHamburgerState) {
      // Use scheduleMicrotask to update in the same frame, avoiding postFrameCallback delay
      Future.microtask(() {
        if (mounted) {
          ref
              .read(showHamburgerProvider.notifier)
              .setVisible(shouldShowHamburger);
        }
      });
    }

    // If less than 2 tabs fit, OR only 2 tabs fit without room for More, show hamburger instead
    if (shouldShowHamburger) {
      return const SizedBox.shrink(); // Return empty widget
    }

    // Split items into visible and overflow
    final visibleTabs = visibleItems
        .take(fitsCalculation.visibleCount)
        .toList();
    final overflowTabs = visibleItems
        .skip(fitsCalculation.visibleCount)
        .toList();

    // Total number of focusable items (tabs + More button if present)
    final totalFocusable =
        visibleTabs.length + (overflowTabs.isNotEmpty ? 1 : 0);

    return Focus(
      focusNode: _focusNode,
      onKeyEvent: (node, event) {
        if (event is! KeyDownEvent) {
          return KeyEventResult.ignored;
        }

        if (event.logicalKey == LogicalKeyboardKey.arrowLeft) {
          setState(() {
            _focusedIndex = (_focusedIndex - 1) % totalFocusable;
            if (_focusedIndex < 0) _focusedIndex = totalFocusable - 1;
          });
          return KeyEventResult.handled;
        } else if (event.logicalKey == LogicalKeyboardKey.arrowRight) {
          setState(() {
            _focusedIndex = (_focusedIndex + 1) % totalFocusable;
          });
          return KeyEventResult.handled;
        } else if (event.logicalKey == LogicalKeyboardKey.enter ||
            event.logicalKey == LogicalKeyboardKey.space) {
          // Activate the focused item
          if (_focusedIndex < visibleTabs.length) {
            _handleTabTap(context, visibleTabs[_focusedIndex]);
          } else {
            // Activate More button
            _showMoreMenu(context, overflowTabs, currentRoute);
          }
          return KeyEventResult.handled;
        }

        return KeyEventResult.ignored;
      },
      child: Container(
        height: 48,
        padding: const EdgeInsets.symmetric(horizontal: Spacing.sm),
        decoration: BoxDecoration(
          color: context.surface,
          border: Border(
            bottom: BorderSide(
              color: context.outline.withValues(alpha: 0.2),
              width: 1,
            ),
          ),
        ),
        child: Row(
          children: [
            // Visible tabs
            ...visibleTabs.asMap().entries.map((entry) {
              final item = entry.value;
              // Tab is active if:
              // 1. Current route exactly matches item.path, OR
              // 2. Current route is a child of this item (starts with item.path + '/')
              // BUT: "/" (home) only matches exactly "/" (not "/demo-nav1")
              final isExactMatch = currentRoute == item.path;
              final isChildRoute =
                  item.path != '/' && currentRoute.startsWith('${item.path}/');
              final isActive = isExactMatch || isChildRoute;

              return _buildTab(
                context: context,
                item: item,
                isActive: isActive,
                isFocused: _focusedIndex == entry.key,
              );
            }),

            // More button if there are overflow tabs
            if (overflowTabs.isNotEmpty)
              _buildMoreButton(
                context: context,
                overflowItems: overflowTabs,
                currentRoute: currentRoute,
                isFocused: _focusedIndex == visibleTabs.length,
              ),
          ],
        ),
      ),
    );
  }

  /// Calculate how many tabs can fit in available width
  _TabFitsCalculation _calculateTabFits({
    required BuildContext context,
    required double availableWidth,
    required List<NavItem> items,
  }) {
    if (items.isEmpty) {
      return _TabFitsCalculation(visibleCount: 0, needsMore: false);
    }

    // Subtract the container's own horizontal padding so tab estimates
    // don't cause a layout overflow (padding: Spacing.sm * 2 = 16px).
    final usableWidth = availableWidth - (Spacing.sm * 2);

    // Estimate width for each tab
    double usedWidth = 0;
    int visibleCount = 0;

    for (final item in items) {
      final estimatedWidth = _estimateTabWidth(context, item);

      if (usedWidth + estimatedWidth <= usableWidth) {
        usedWidth += estimatedWidth;
        visibleCount++;
      } else {
        // Check if we can fit "More" button with at least 2 visible tabs
        if (visibleCount >= 2 && usedWidth + moreButtonWidth <= usableWidth) {
          return _TabFitsCalculation(
            visibleCount: visibleCount,
            needsMore: true,
          );
        } else if (visibleCount >= 3) {
          // Remove last tab to make room for More button (keep at least 2 tabs + More)
          return _TabFitsCalculation(
            visibleCount: visibleCount - 1,
            needsMore: true,
          );
        } else {
          // Not enough space for 2 tabs + More button minimum
          return _TabFitsCalculation(
            visibleCount: visibleCount,
            needsMore: false,
          );
        }
      }
    }

    // All tabs fit
    return _TabFitsCalculation(visibleCount: visibleCount, needsMore: false);
  }

  /// Estimate width required for a tab
  double _estimateTabWidth(BuildContext context, NavItem item) {
    // Create cache key
    final hasChildren = item.children
        .where((child) => child.metadata?.showInNav ?? true)
        .isNotEmpty;
    final locale = Localizations.localeOf(context).toString();
    final cacheKey = '${item.label}_${hasChildren}_$locale';

    // Check cache first
    if (_widthCache.containsKey(cacheKey)) {
      return _widthCache[cacheKey]!;
    }

    // Translate the label
    final label = ref.tr(item.label);

    final textPainter = TextPainter(
      text: TextSpan(
        text: label,
        style: context.bodyMedium.copyWith(fontWeight: FontWeight.w500),
      ),
      textDirection: TextDirection.ltr,
    )..layout();

    // Base width: text + padding + chevron (if has children) + margin
    // tabPadding * 2 = the Container's horizontal padding (12px each side)
    // Spacing.xs * 2 = the outer Padding widget's horizontal padding (4px each side)
    double width = textPainter.width + (tabPadding * 2) + (Spacing.xs * 2);

    if (hasChildren) {
      width += 4 + 18; // SizedBox(width: Spacing.xs) + Icon(size: 18)
    }

    width += Spacing.sm; // Add spacing between tabs

    // Cache the result
    _widthCache[cacheKey] = width;

    return width;
  }

  /// Build a single tab
  Widget _buildTab({
    required BuildContext context,
    required NavItem item,
    required bool isActive,
    bool isFocused = false,
  }) {
    final hasChildren = item.children
        .where((child) => child.metadata?.showInNav ?? true)
        .isNotEmpty;

    final label = ref.tr(item.label);

    return Semantics(
      container: true,
      button: true,
      label: label,
      hint: hasChildren
          ? 'Navigation tab with submenu. Double tap to expand menu'
          : 'Navigation tab. Double tap to navigate',
      selected: isActive,
      focused: isFocused,
      enabled: true,
      child: Padding(
        padding: const EdgeInsets.symmetric(horizontal: Spacing.xs),
        child: Material(
          color: Colors.transparent,
          child: InkWell(
            onTap: () => _handleTabTap(context, item),
            borderRadius: RadiusTokens.radiusSm,
            child: Container(
              padding: const EdgeInsets.symmetric(
                horizontal: tabPadding,
                vertical: Spacing.sm,
              ),
              decoration: BoxDecoration(
                color: isActive
                    ? BrandColors.deepBlue500.withValues(alpha: 0.1)
                    : Colors.transparent,
                borderRadius: RadiusTokens.radiusSm,
                border: isFocused
                    ? Border.all(color: BrandColors.deepBlue500, width: 2)
                    : isActive
                    ? const Border(
                        bottom: BorderSide(
                          color: BrandColors.deepBlue500,
                          width: 2,
                        ),
                      )
                    : null,
              ),
              child: Row(
                mainAxisSize: MainAxisSize.min,
                children: [
                  Text(
                    label,
                    style: context.bodyMedium.copyWith(
                      fontWeight: isActive ? FontWeight.w600 : FontWeight.w500,
                      color: isActive
                          ? BrandColors.deepBlue500
                          : context.onSurface,
                    ),
                  ),
                  if (hasChildren) ...[
                    const SizedBox(width: Spacing.xs),
                    Icon(
                      Icons.arrow_drop_down,
                      size: 18,
                      color: isActive
                          ? BrandColors.deepBlue500
                          : context.onSurface.withValues(alpha: 0.6),
                    ),
                  ],
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }

  /// Handle tab tap
  void _handleTabTap(BuildContext context, NavItem item) {
    final hasChildren = item.children
        .where((child) => child.metadata?.showInNav ?? true)
        .isNotEmpty;

    if (hasChildren) {
      // Show dropdown with children
      _showTabDropdown(context, item);
    } else {
      // Set the flag BEFORE context.go so the router redirect sees it
      ref.read(isNavigatingProvider.notifier).set(true);
      context.go(item.path);
    }
  }

  /// Show dropdown menu for tab with children
  void _showTabDropdown(BuildContext context, NavItem item) {
    final visibleChildren =
        item.children
            .where((child) => child.metadata?.showInNav ?? true)
            .toList()
          ..sort((a, b) {
            final orderA = a.metadata?.order ?? 0;
            final orderB = b.metadata?.order ?? 0;
            return orderA.compareTo(orderB);
          });

    if (visibleChildren.isEmpty) {
      // Set the flag BEFORE context.go so the router redirect sees it
      ref.read(isNavigatingProvider.notifier).set(true);
      context.go(item.path);
      return;
    }

    // Show accordion dropdown using BaseAccordion positioned below tab
    _showAccordionOverlay(
      context: context,
      items: [item], // Wrap in list to show item with its children
      expandInitialItems: true, // Auto-expand so children show immediately
    );
  }

  /// Build "More" button for overflow tabs
  Widget _buildMoreButton({
    required BuildContext context,
    required List<NavItem> overflowItems,
    required String currentRoute,
    bool isFocused = false,
  }) {
    return Semantics(
      container: true,
      button: true,
      label: 'More navigation options',
      hint:
          'Opens menu with ${overflowItems.length} additional navigation items',
      focused: isFocused,
      enabled: true,
      child: Padding(
        padding: const EdgeInsets.symmetric(horizontal: Spacing.xs),
        child: Material(
          color: Colors.transparent,
          child: InkWell(
            onTap: () => _showMoreMenu(context, overflowItems, currentRoute),
            borderRadius: RadiusTokens.radiusSm,
            child: Container(
              padding: const EdgeInsets.symmetric(
                horizontal: tabPadding,
                vertical: Spacing.sm,
              ),
              decoration: isFocused
                  ? BoxDecoration(
                      borderRadius: RadiusTokens.radiusSm,
                      border: Border.all(
                        color: BrandColors.deepBlue500,
                        width: 2,
                      ),
                    )
                  : null,
              child: Row(
                mainAxisSize: MainAxisSize.min,
                children: [
                  Text(
                    'More',
                    style: context.bodyMedium.copyWith(
                      fontWeight: FontWeight.w500,
                    ),
                  ),
                  const SizedBox(width: Spacing.xs),
                  Icon(
                    Icons.more_horiz,
                    size: 18,
                    color: context.onSurface.withValues(alpha: 0.6),
                  ),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }

  /// Show popup menu with overflow tabs
  void _showMoreMenu(
    BuildContext context,
    List<NavItem> overflowItems,
    String currentRoute,
  ) {
    if (overflowItems.isEmpty) return;

    // Show accordion overlay with overflow items
    _showAccordionOverlay(
      context: context,
      items: overflowItems,
      expandInitialItems: false, // Don't auto-expand for More menu
    );
  }

  /// Show accordion overlay positioned below the clicked tab
  /// Uses Overlay and OverlayEntry for precise positioning
  void _showAccordionOverlay({
    required BuildContext context,
    required List<NavItem> items,
    bool expandInitialItems = false,
  }) {
    // Get current route for highlighting
    final currentRoute = ref.read(currentRouteProvider);

    // Find the button's position to place dropdown below it
    final RenderBox? renderBox = context.findRenderObject() as RenderBox?;
    if (renderBox == null) return;

    final offset = renderBox.localToGlobal(Offset.zero);
    final size = renderBox.size;

    // Create overlay entry
    late OverlayEntry overlayEntry;
    overlayEntry = OverlayEntry(
      builder: (overlayContext) => Stack(
        children: [
          // Barrier to dismiss on click outside
          Positioned.fill(
            child: GestureDetector(
              onTap: () => overlayEntry.remove(),
              behavior: HitTestBehavior.translucent,
              child: Container(color: Colors.transparent),
            ),
          ),

          // Positioned accordion below the tab
          Positioned(
            top: offset.dy + size.height + 4, // 4px spacing below tab
            left: offset.dx,
            child: Material(
              elevation: 8,
              borderRadius: RadiusTokens.radiusMd,
              shadowColor: Colors.black.withValues(alpha: 0.3),
              child: Container(
                constraints: const BoxConstraints(
                  minWidth: 200,
                  maxWidth: 400,
                  maxHeight: 500,
                ),
                decoration: BoxDecoration(
                  color: context.surface,
                  borderRadius: RadiusTokens.radiusMd,
                  border: Border.all(
                    color: context.outline.withValues(alpha: 0.2),
                    width: 1,
                  ),
                ),
                child: SingleChildScrollView(
                  child: BaseAccordion(
                    items: items,
                    currentRoute: currentRoute,
                    expandInitialItems: expandInitialItems,
                    onNavigate: () {
                      // Close overlay after navigation
                      overlayEntry.remove();
                    },
                  ),
                ),
              ),
            ),
          ),
        ],
      ),
    );

    // Insert overlay
    Overlay.of(context).insert(overlayEntry);
  }
}

/// Helper class for tab fits calculation result
class _TabFitsCalculation {
  final int visibleCount;
  final bool needsMore;

  _TabFitsCalculation({required this.visibleCount, required this.needsMore});
}

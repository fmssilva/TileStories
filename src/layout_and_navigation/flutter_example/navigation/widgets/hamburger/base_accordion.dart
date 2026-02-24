import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../../design/design_system.dart';
import '../../../utils/i18n/extensions/context_extensions.dart';
import '../../navConfig/nav_item.dart';
import '../../histConfig/is_navigating_provider.dart';
// =============================================================================
// BASE ACCORDION - SINGLE SOURCE OF TRUTH FOR ALL ACCORDIONS
// =============================================================================

///
/// Base Accordion
///
/// A complete accordion component that handles ALL navigation accordion logic:
/// - Filtering by metadata.showInNav
/// - Sorting by metadata.order
/// - Expand/collapse animations
/// - Keyboard navigation (Arrow Up/Down, Enter, Space, Escape)
/// - Focus management with visual indicators
/// - Accessibility with full Semantics support
/// - Hierarchical rendering with proper indentation
/// - Navigation with GoRouter
/// - i18n translation
/// - Hover and active states
/// - Brand colors and design tokens
///
/// This is the SINGLE SOURCE OF TRUTH for accordion behavior.
/// All navigation dropdowns and menus use this component.
///
/// NOT GENERIC - Works specifically with NavItem.
/// All NavItem-specific logic (filtering, sorting, translation) is INSIDE this component.
///
/// USAGE:
/// ```dart
/// BaseAccordion(
///   items: navigationConfig,
///   currentRoute: '/explore',
///   onNavigate: () => Navigator.pop(context), // Close drawer/menu
///   expandInitialItems: true, // Auto-expand items with children
/// )
/// ```
class BaseAccordion extends ConsumerStatefulWidget {
  /// List of top-level navigation items
  final List<NavItem> items;

  /// Currently active route (for highlighting)
  final String? currentRoute;

  /// Callback when navigation occurs (to close drawer/menu)
  final VoidCallback? onNavigate;

  /// Whether to automatically expand top-level items that have children
  /// Useful for tab dropdowns where we want to show children immediately
  final bool expandInitialItems;

  /// Whether to show expandable/collapsible chevrons
  final bool showChevrons;

  /// Whether to show focus indicators for keyboard navigation
  final bool showFocusIndicators;

  /// Custom decoration for container
  final BoxDecoration? containerDecoration;

  const BaseAccordion({
    super.key,
    required this.items,
    this.currentRoute,
    this.onNavigate,
    this.expandInitialItems = false,
    this.showChevrons = true,
    this.showFocusIndicators = true,
    this.containerDecoration,
  });

  @override
  ConsumerState<BaseAccordion> createState() => _BaseAccordionState();
}

class _BaseAccordionState extends ConsumerState<BaseAccordion> {
  /// Tracks which items are expanded (by their path)
  final Set<String> _expandedIds = {};

  /// Focus node for keyboard navigation
  final FocusNode _focusNode = FocusNode();

  /// Currently focused item index (flat list)
  int _focusedIndex = 0;

  /// Flat list of all visible items (for keyboard navigation)
  List<NavItem> _flatItems = [];

  @override
  void initState() {
    super.initState();
    // Auto-expand initial items if requested
    if (widget.expandInitialItems) {
      _expandInitialItemsWithChildren();
    }
  }

  /// Auto-expand top-level items that have visible children
  /// Called on mount when expandInitialItems=true
  void _expandInitialItemsWithChildren() {
    final visibleItems = _getFilteredSortedItems(widget.items);
    for (final item in visibleItems) {
      final hasChildren = _getFilteredSortedItems(item.children).isNotEmpty;
      if (hasChildren) {
        _expandedIds.add(item.path);
      }
    }
  }

  @override
  void dispose() {
    _focusNode.dispose();
    super.dispose();
  }

  /// Get filtered and sorted items
  /// - Filters by metadata.showInNav (default: true)
  /// - Sorts by metadata.order (ascending)
  List<NavItem> _getFilteredSortedItems(List<NavItem> items) {
    return items.where((item) => item.metadata?.showInNav ?? true).toList()
      ..sort((a, b) {
        final orderA = a.metadata?.order ?? 0;
        final orderB = b.metadata?.order ?? 0;
        return orderA.compareTo(orderB);
      });
  }

  @override
  Widget build(BuildContext context) {
    // Filter and sort items
    final visibleItems = _getFilteredSortedItems(widget.items);

    // Build flat list for keyboard navigation
    _flatItems = _buildFlatList(visibleItems);

    return Focus(
      focusNode: _focusNode,
      onKeyEvent: (node, event) {
        if (event is! KeyDownEvent) return KeyEventResult.ignored;

        // Arrow Down - move focus to next item
        if (event.logicalKey == LogicalKeyboardKey.arrowDown) {
          setState(() {
            _focusedIndex = (_focusedIndex + 1) % _flatItems.length;
          });
          return KeyEventResult.handled;
        }
        // Arrow Up - move focus to previous item
        else if (event.logicalKey == LogicalKeyboardKey.arrowUp) {
          setState(() {
            _focusedIndex = (_focusedIndex - 1) % _flatItems.length;
            if (_focusedIndex < 0) _focusedIndex = _flatItems.length - 1;
          });
          return KeyEventResult.handled;
        }
        // Enter or Space - activate focused item
        else if (event.logicalKey == LogicalKeyboardKey.enter ||
            event.logicalKey == LogicalKeyboardKey.space) {
          if (_flatItems.isNotEmpty && _focusedIndex < _flatItems.length) {
            final item = _flatItems[_focusedIndex];
            _handleItemTap(item);
          }
          return KeyEventResult.handled;
        }
        // Escape - close accordion
        else if (event.logicalKey == LogicalKeyboardKey.escape) {
          widget.onNavigate?.call();
          return KeyEventResult.handled;
        }

        return KeyEventResult.ignored;
      },
      child: Container(
        decoration:
            widget.containerDecoration ??
            BoxDecoration(
              color: context.surface,
              borderRadius: RadiusTokens.radiusMd,
              border: Border.all(
                color: context.outline.withValues(alpha: 0.2),
                width: 1,
              ),
            ),
        child: ClipRRect(
          borderRadius: RadiusTokens.radiusMd,
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: visibleItems
                .map(
                  (item) => _buildAccordionItem(
                    context: context,
                    item: item,
                    level: 0,
                  ),
                )
                .toList(),
          ),
        ),
      ),
    );
  }

  /// Build flat list of all visible items (including expanded children)
  List<NavItem> _buildFlatList(List<NavItem> items) {
    final flat = <NavItem>[];
    for (final item in items) {
      flat.add(item);
      if (_expandedIds.contains(item.path)) {
        final children = _getFilteredSortedItems(item.children);
        flat.addAll(_buildFlatList(children));
      }
    }
    return flat;
  }

  /// Recursively build accordion item with its children
  Widget _buildAccordionItem({
    required BuildContext context,
    required NavItem item,
    required int level,
  }) {
    final children = _getFilteredSortedItems(item.children);
    final hasChildren = children.isNotEmpty;
    final isExpanded = _expandedIds.contains(item.path);
    final isSelected = widget.currentRoute == item.path;
    final isFocused =
        _flatItems.isNotEmpty &&
        _focusedIndex < _flatItems.length &&
        _flatItems[_focusedIndex].path == item.path;

    // Calculate indentation based on hierarchy level
    final indentation = Spacing.lg * level;

    return Column(
      mainAxisSize: MainAxisSize.min,
      children: [
        // Item row with tap handler
        _buildItemRow(
          context: context,
          item: item,
          level: level,
          indentation: indentation,
          hasChildren: hasChildren,
          isExpanded: isExpanded,
          isSelected: isSelected,
          isFocused: isFocused,
        ),

        // Children (if expanded and has children)
        if (hasChildren && isExpanded)
          AnimatedSize(
            duration: const Duration(milliseconds: 300),
            curve: Curves.easeInOut,
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: children
                  .map(
                    (child) => _buildAccordionItem(
                      context: context,
                      item: child,
                      level: level + 1,
                    ),
                  )
                  .toList(),
            ),
          ),
      ],
    );
  }

  /// Build individual item row with styling
  Widget _buildItemRow({
    required BuildContext context,
    required NavItem item,
    required int level,
    required double indentation,
    required bool hasChildren,
    required bool isExpanded,
    required bool isSelected,
    required bool isFocused,
  }) {
    // Translate label using i18n
    final label = ref.tr(item.label);

    // No badges for NavItem (can add later if needed)
    final badge = null;
    final children = _getFilteredSortedItems(item.children);

    return Semantics(
      container: true,
      button: true,
      label: label,
      hint: hasChildren
          ? (isExpanded
                ? 'Expanded. Double tap to collapse ${children.length} items'
                : 'Collapsed. Double tap to expand ${children.length} items')
          : 'Double tap to select',
      expanded: hasChildren ? isExpanded : null,
      selected: isSelected,
      focused: isFocused,
      enabled: true,
      inMutuallyExclusiveGroup: true,
      child: Material(
        color: Colors.transparent,
        child: InkWell(
          // Row tap: navigate to page (even if it has children)
          onTap: () => _handleItemNavigation(item),
          hoverColor: _getHoverColor(context, level, isSelected),
          splashColor: context.primary.withValues(alpha: 0.1),
          borderRadius: RadiusTokens.radiusSm,
          child: Container(
            padding: EdgeInsets.only(
              left: Spacing.md + indentation,
              right: Spacing.md,
              top: Spacing.sm,
              bottom: Spacing.sm,
            ),
            decoration: BoxDecoration(
              // Selected state background
              color: isSelected
                  ? context.primary.withValues(alpha: 0.1)
                  : Colors.transparent,

              // Focus indicator border
              border: isFocused && widget.showFocusIndicators
                  ? Border.all(color: BrandColors.deepBlue500, width: 2)
                  : Border(
                      bottom: BorderSide(
                        color: context.outline.withValues(alpha: 0.1),
                        width: 0.5,
                      ),
                    ),
              borderRadius: isFocused && widget.showFocusIndicators
                  ? RadiusTokens.radiusSm
                  : null,
            ),
            child: Row(
              children: [
                // Chevron icon (if has children and showChevrons is true)
                // Separate clickable area for expand/collapse
                if (hasChildren && widget.showChevrons)
                  GestureDetector(
                    onTap: () => _handleChevronTap(item),
                    // Add padding around chevron for easier tapping
                    child: Padding(
                      padding: const EdgeInsets.all(Spacing.xs),
                      child: AnimatedRotation(
                        turns: isExpanded
                            ? 0.25
                            : 0.0, // 90 degrees when expanded
                        duration: const Duration(milliseconds: 200),
                        child: Icon(
                          Icons.chevron_right,
                          size: 20,
                          color: _getIconColor(context, level, isSelected),
                        ),
                      ),
                    ),
                  )
                else if (widget.showChevrons)
                  const SizedBox(
                    width: 20 + (Spacing.xs * 2),
                  ), // Placeholder spacing

                if (widget.showChevrons) const SizedBox(width: Spacing.xs),

                // Item label
                Expanded(
                  child: Text(
                    label,
                    style: _getTextStyle(context, level, isSelected),
                  ),
                ),

                // Optional badge
                if (badge != null && badge.isNotEmpty)
                  Container(
                    margin: const EdgeInsets.only(left: Spacing.sm),
                    padding: const EdgeInsets.symmetric(
                      horizontal: Spacing.sm,
                      vertical: 2,
                    ),
                    decoration: BoxDecoration(
                      color: context.primary.withValues(alpha: 0.15),
                      borderRadius: RadiusTokens.radiusSm,
                    ),
                    child: Text(
                      badge,
                      style: context.labelSmall.copyWith(
                        color: context.primary,
                        fontWeight: FontWeight.w600,
                      ),
                    ),
                  ),
              ],
            ),
          ),
        ),
      ),
    );
  }

  /// Handle tap on an item - Navigate to the page
  void _handleItemNavigation(NavItem item) {
    ref.read(isNavigatingProvider.notifier).set(true);
    context.go(item.path);
    widget.onNavigate?.call();
  }

  /// Handle tap on chevron - Toggle expansion
  void _handleChevronTap(NavItem item) {
    final children = _getFilteredSortedItems(item.children);
    final hasChildren = children.isNotEmpty;

    if (hasChildren) {
      setState(() {
        if (_expandedIds.contains(item.path)) {
          _expandedIds.remove(item.path);
        } else {
          _expandedIds.add(item.path);
        }
      });
    }
  }

  /// Handle tap on an item (legacy - for keyboard navigation)
  void _handleItemTap(NavItem item) {
    final children = _getFilteredSortedItems(item.children);
    final hasChildren = children.isNotEmpty;

    if (hasChildren) {
      setState(() {
        if (_expandedIds.contains(item.path)) {
          _expandedIds.remove(item.path);
        } else {
          _expandedIds.add(item.path);
        }
      });
    } else {
      ref.read(isNavigatingProvider.notifier).set(true);
      context.go(item.path);
      widget.onNavigate?.call();
    }
  }

  /// Get hover color based on hierarchy level and selection state
  Color _getHoverColor(BuildContext context, int level, bool isSelected) {
    if (isSelected) {
      return context.primary.withValues(alpha: 0.15);
    }
    // Higher levels get more subtle hover effects
    final opacity = 0.08 - (level * 0.01);
    return context.onSurface.withValues(alpha: opacity.clamp(0.02, 0.08));
  }

  /// Get icon color based on level and selection state
  Color _getIconColor(BuildContext context, int level, bool isSelected) {
    if (isSelected) return context.primary;

    // Deeper levels get lighter icons
    return context.onSurface.withValues(
      alpha: 0.7 - (level * 0.1).clamp(0.3, 0.7),
    );
  }

  /// Get text style based on hierarchy level and selection
  TextStyle _getTextStyle(BuildContext context, int level, bool isSelected) {
    // Base style depends on level
    TextStyle? baseStyle;
    switch (level) {
      case 0:
        baseStyle = context.titleMedium; // Top level - larger, bolder
        break;
      case 1:
        baseStyle = context.bodyLarge; // Second level
        break;
      case 2:
        baseStyle = context.bodyMedium; // Third level
        break;
      default:
        baseStyle = context.bodySmall; // Deep nesting - smaller
    }

    // Apply selection styling
    return baseStyle.copyWith(
      color: isSelected
          ? context.primary
          : context.onSurface.withValues(
              alpha: 0.9 - (level * 0.1).clamp(0.6, 0.9),
            ),
      fontWeight: isSelected
          ? FontWeight.w600
          : (level == 0 ? FontWeight.w500 : FontWeight.w400),
    );
  }
}

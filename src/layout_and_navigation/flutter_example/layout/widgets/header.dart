import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../design/design_system.dart';
import '../../navigation/navigation.dart';
import '../../navigation/histConfig/is_navigating_provider.dart';
import 'icons_group.dart';

/// @index-widget layout-header
/// @layer layout
/// @description Main header/app bar for web/desktop layouts
/// @ui-type ConsumerWidget
/// @depends-on AppLogo, AppName, NavTabsRow, IconsGroup
/// @domain-usage Top-level layout component for web/desktop
///
/// HEADER COMPONENT
/// ================
///
/// Main horizontal bar at the top of the app containing:
/// - App logo and name (left side)
/// - Navigation tabs (center, flexible width)
/// - Icons (language, theme, hamburger) (right side)
///
/// RESPONSIBILITIES:
/// - Calculate available width for NavTabsRow
/// - Position all header elements consistently
/// - Maintain proper spacing and alignment
/// - Ensure proper overflow behavior for dropdowns
///
/// LAYOUT STRUCTURE:
/// [Logo] [Name] [Spacer(flex:1)] [NavTabsRow(width)] [Spacer(flex:1)] [Icons] [Padding]
///
/// WIDTH CALCULATION:
/// Fixed elements:
/// - Logo: 48px
/// - Name: ~150px (estimated)
/// - Spacer1 (flex): 16px minimum
/// - Spacer2 (flex): 16px minimum
/// - Icons: ~120px (2-3 icon buttons at ~40px each)
/// - Right padding: 16px
/// Total fixed: ~350px
/// Available for tabs = totalWidth - fixed elements
///
/// Z-INDEX / OVERFLOW:
/// - Header uses Stack internally if needed for dropdowns
/// - Dropdowns use Overlay (global positioning) so they appear above everything
///
/// USAGE:
/// ```dart
/// LayoutSlots(
///   header: const Header(),
///   body: MyContent(),
/// )
/// ```
class Header extends ConsumerWidget implements PreferredSizeWidget {
  const Header({super.key});

  @override
  Size get preferredSize => const Size.fromHeight(64.0);

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    return Container(
      height: 64.0,
      decoration: BoxDecoration(
        color: Theme.of(context).colorScheme.surface,
        border: Border(
          bottom: BorderSide(color: Theme.of(context).dividerColor, width: 1.0),
        ),
      ),
      child: Row(
        children: [
          // Left side: Logo + Name + History Navigation
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: Spacing.md),
            child: Row(
              mainAxisSize: MainAxisSize.min,
              children: [
                AppLogo(
                  size: 48.0,
                  onTap: () {
                    ref.read(isNavigatingProvider.notifier).set(true);
                    context.go('/');
                  },
                ),
                const SizedBox(width: Spacing.sm), // 8px
                AppName(
                  style: Theme.of(context).textTheme.titleLarge,
                  onTap: () {
                    ref.read(isNavigatingProvider.notifier).set(true);
                    context.go('/');
                  },
                ),
                const SizedBox(width: Spacing.md), // 16px
                // History Navigation Buttons
                _buildHistoryButtons(ref, context),
              ],
            ),
          ),

          // Center: Navigation Tabs (flex — takes all remaining space between left and right)
          Expanded(
            child: Center(
              child: LayoutBuilder(
                builder: (context, constraints) {
                  return NavTabsRow(availableWidth: constraints.maxWidth);
                },
              ),
            ),
          ),

          // Right side: Icons (Language, Theme, Hamburger)
          const Padding(
            padding: EdgeInsets.only(right: Spacing.md),
            child: IconsGroup(),
          ),
        ],
      ),
    );
  }

  /// Build back/forward navigation buttons for UNDO/REDO functionality
  Widget _buildHistoryButtons(WidgetRef ref, BuildContext context) {
    final history = ref.watch(navHistoryProvider);
    final canGoBack = history.canGoBack;
    final canGoForward = history.canGoForward;
    final backPath = ref.read(navHistoryProvider.notifier).getBackPath();
    final forwardPath = ref.read(navHistoryProvider.notifier).getForwardPath();

    return Container(
      decoration: BoxDecoration(
        border: Border.all(color: Theme.of(context).dividerColor, width: 1.0),
        borderRadius: BorderRadius.circular(8.0),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          // UNDO Button (Back)
          IconButton(
            icon: const Icon(Icons.arrow_back, size: 20),
            onPressed: canGoBack && backPath != null
                ? () {
                    // No extra flag → redirect detects UNDO and calls undoRedo()
                    context.go(backPath);
                  }
                : null,
            tooltip: 'UNDO - Go back (⬅️)',
            iconSize: 20,
            visualDensity: VisualDensity.compact,
            padding: const EdgeInsets.all(8.0),
          ),
          // Divider
          Container(
            width: 1,
            height: 24,
            color: Theme.of(context).dividerColor,
          ),
          // REDO Button (Forward)
          IconButton(
            icon: const Icon(Icons.arrow_forward, size: 20),
            onPressed: canGoForward && forwardPath != null
                ? () {
                    // No extra flag → redirect detects REDO and calls undoRedo()
                    context.go(forwardPath);
                  }
                : null,
            tooltip: 'REDO - Go forward (➡️)',
            iconSize: 20,
            visualDensity: VisualDensity.compact,
            padding: const EdgeInsets.all(8.0),
          ),
        ],
      ),
    );
  }
}

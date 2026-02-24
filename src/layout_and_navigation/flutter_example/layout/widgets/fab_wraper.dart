import 'package:flutter/material.dart';
import '../platform_info.dart';

/// Modes for FAB positioning
///
/// - cornerVertical: Stack FAB vertically in corner (portrait mode)
/// - sideHorizontal: Stack FAB horizontally on side (landscape mode)
/// - adaptive: Automatically choose based on orientation
enum FabMode { cornerVertical, sideHorizontal, adaptive }

/// Adaptive FAB wrapper that changes layout based on orientation.
///
/// STUDENT NOTE: This widget wraps a FAB and adds a chevron toggle button.
/// The FAB can be expanded/collapsed, and the layout direction changes
/// based on screen orientation (vertical in portrait, horizontal in landscape).
///
/// WHY ADAPTIVE?
/// - Portrait: Vertical stack fits naturally in corner
/// - Landscape: Horizontal stack saves vertical space
/// - Consistent UX across orientations
///
/// FEATURES:
/// - Expandable/collapsible with animation
/// - Chevron button indicates direction and state
/// - Adapts to portrait/landscape automatically
///
/// USAGE:
/// ```dart
/// AdaptiveFab(
///   mode: FabMode.adaptive,
///   child: MyFAB(),
/// )
/// ```
class FabWraperAdaptative extends StatefulWidget {
  final Widget child; // The actual FAB content
  final FabMode mode; // Positioning mode

  const FabWraperAdaptative({
    super.key,
    required this.child,
    this.mode = FabMode.adaptive,
  });

  @override
  State<FabWraperAdaptative> createState() => _FabWraperAdaptativeState();
}

class _FabWraperAdaptativeState extends State<FabWraperAdaptative> {
  bool _isOpen = false; // Is the FAB expanded?

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    // Determine effective mode (resolve 'adaptive' to actual mode)
    final FabMode effectiveMode = widget.mode == FabMode.adaptive
        ? (PlatformInfo.isLandscape(context)
              ? FabMode.sideHorizontal
              : FabMode.cornerVertical)
        : widget.mode;

    // Choose chevron icon based on mode and state
    IconData chevron;
    if (effectiveMode == FabMode.cornerVertical) {
      // Vertical mode: up/down chevron
      chevron = _isOpen ? Icons.keyboard_arrow_down : Icons.keyboard_arrow_up;
    } else {
      // Horizontal mode: left/right chevron
      chevron = _isOpen
          ? Icons.keyboard_arrow_right
          : Icons.keyboard_arrow_left;
    }

    // Chevron toggle button
    final chevronFab = FloatingActionButton.small(
      onPressed: () => setState(() => _isOpen = !_isOpen),
      backgroundColor: theme.colorScheme.secondary,
      child: Icon(chevron),
    );

    // Animated child visibility
    final animatedChild = AnimatedOpacity(
      opacity: _isOpen ? 1.0 : 0.0,
      duration: const Duration(milliseconds: 200),
      child: _isOpen ? widget.child : const SizedBox.shrink(),
    );

    // Return vertical or horizontal layout based on effective mode
    if (effectiveMode == FabMode.cornerVertical) {
      // Vertical stack: child above chevron
      return Column(
        mainAxisSize: MainAxisSize.min,
        children: [animatedChild, const SizedBox(height: 6), chevronFab],
      );
    } else {
      // Horizontal row: child before chevron
      return Row(
        mainAxisSize: MainAxisSize.min,
        children: [animatedChild, const SizedBox(width: 6), chevronFab],
      );
    }
  }
}

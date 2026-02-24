import 'package:flutter/widgets.dart';
import 'package:flutter/services.dart';

/// Pure data holder for layout configuration pieces.
///
/// STUDENT NOTE: This is a simple data class that holds all the pieces
/// of a page layout. It has NO logic - only fields to configure the layout.
///
/// WHY SEPARATE DATA FROM LOGIC?
/// - Easy to understand: just fields, no complex behavior
/// - Easy to test: can create different configurations easily
/// - Easy to pass around: domains configure, LayoutManager renders
///
/// USAGE:
/// ```dart
/// LayoutSlots(
///   body: MyPageContent(),
///   header: MyHeader(),
///   fab: MyFAB(),
///   scrollable: true,
/// )
/// ```
class LayoutSlots {
  // Main content of the page
  final Widget body;

  // Optional header (AppBar) - typically shown on web
  final PreferredSizeWidget? header;

  // Optional floating action button
  final Widget? fab;

  // Optional footer (BottomNavigationBar) - typically shown on mobile app
  final Widget? footer;

  // Layout behavior options
  final bool scrollable; // Should body be scrollable?
  final bool safeArea; // Respect device safe areas (notch, etc.)?
  final bool resizeForKeyboard; // Resize when keyboard appears?
  final Color? backgroundColor; // Custom background color
  final bool isLoading; // Show loading overlay?

  // Screen orientation lock
  final DeviceOrientation? lockedOrientation;

  // System UI mode (normal, immersive, etc.)
  final SystemUiMode? systemUiMode;

  // Show back-to-top button for long scrollable content
  final bool showBackToTop;

  const LayoutSlots({
    required this.body,
    this.header,
    this.fab,
    this.footer,
    this.scrollable = true,
    this.safeArea = true,
    this.resizeForKeyboard = true,
    this.backgroundColor,
    this.isLoading = false,
    this.lockedOrientation,
    this.systemUiMode,
    this.showBackToTop = false,
  });
}

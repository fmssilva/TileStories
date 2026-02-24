import 'package:flutter/widgets.dart';
import 'package:flutter/services.dart';
import 'layout_slots.dart';
import 'platform_info.dart';
import 'widgets/header.dart';
import 'widgets/footer_app.dart';
import 'widgets/fab_wraper.dart';

/// Common layout presets for different page types.
///
/// STUDENT NOTE: This provides pre-configured LayoutSlots for common scenarios.
/// Instead of manually configuring every field, domains can use these presets
/// and customize only what's different.
///
/// WHY PRESETS?
/// - Consistency: Similar pages use same layout
/// - Convenience: Less boilerplate in domain pages
/// - Flexibility: Can still override specific options
/// - Best practices: Presets encode UX decisions
///
/// PATTERN:
/// Each preset is a static method that returns a configured LayoutSlots.
/// Domains pass their body widget and optional customizations.
///
/// USAGE:
/// ```dart
/// LayoutManager(
///   slots: LayoutPresets.defaultPageBrowser(
///     context: context,
///     body: MyContent(),
///     fab: MyFAB(),
///   ),
/// )
/// ```
class LayoutPresets {
  /// Standard page preset that adapts to platform.
  ///
  /// WEB/DESKTOP: Shows header (AppBar) with navigation
  /// MOBILE APP: Shows footer (BottomNavigationBar) instead
  ///
  /// FEATURES:
  /// - Scrollable by default
  /// - Safe area enabled
  /// - Optional FAB with adaptive positioning
  /// - Platform-appropriate navigation (header vs footer)
  ///
  /// USE FOR: Most standard content pages
  static LayoutSlots defaultPageBrowser({
    required BuildContext context,
    required Widget body,
    Widget? fab,
    FabMode fabMode = FabMode.adaptive,
  }) {
    // Wrap FAB in adaptive wrapper if provided
    final wrappedFab = fab != null
        ? FabWraperAdaptative(mode: fabMode, child: fab)
        : null;

    // Choose header or footer based on platform
    if (PlatformInfo.isApp(context)) {
      // Mobile app: Use bottom navigation
      return LayoutSlots(
        body: body,
        footer: const FooterApp(),
        fab: wrappedFab,
      );
    } else {
      // Web/desktop: Use header navigation
      return LayoutSlots(header: const Header(), body: body, fab: wrappedFab);
    }
  }

  /// Immersive page preset for full-screen content.
  ///
  /// FEATURES:
  /// - No header or footer (clean full-screen)
  /// - No safe area (uses full screen real estate)
  /// - Optional scrolling
  /// - Optional FAB with adaptive positioning
  ///
  /// USE FOR: Panorama viewer, media viewer, immersive experiences
  static LayoutSlots defaultPageApp({
    required Widget body,
    Widget? fab,
    FabMode fabMode = FabMode.adaptive,
    bool scrollable = false,
  }) {
    final wrappedFab = fab != null
        ? FabWraperAdaptative(mode: fabMode, child: fab)
        : null;

    return LayoutSlots(
      body: body,
      fab: wrappedFab,
      scrollable: scrollable,
      safeArea: false, // Use full screen
    );
  }

  /// Fullscreen preset for boot/splash screens.
  ///
  /// FEATURES:
  /// - Immersive sticky mode (hides system UI)
  /// - No scrolling
  /// - No safe area
  /// - Optional FAB
  ///
  /// USE FOR: Splash screens, onboarding, fullscreen presentations
  static LayoutSlots fullscreen({
    required Widget body,
    Widget? fab,
    FabMode fabMode = FabMode.adaptive,
  }) {
    final wrappedFab = fab != null
        ? FabWraperAdaptative(mode: fabMode, child: fab)
        : null;

    return LayoutSlots(
      body: body,
      fab: wrappedFab,
      scrollable: false,
      safeArea: false,
      systemUiMode: SystemUiMode.immersiveSticky, // Hide system bars
    );
  }

  /// Minimal preset for focused flows.
  ///
  /// FEATURES:
  /// - Header shown (for branding/context)
  /// - No footer
  /// - Scrollable
  /// - Safe area enabled
  /// - Optional FAB
  ///
  /// USE FOR: Login, signup, focused task flows
  static LayoutSlots minimal({
    required BuildContext context,
    required Widget body,
    Widget? fab,
    FabMode fabMode = FabMode.adaptive,
  }) {
    final wrappedFab = fab != null
        ? FabWraperAdaptative(mode: fabMode, child: fab)
        : null;

    return LayoutSlots(
      header: const Header(),
      body: body,
      fab: wrappedFab,
      scrollable: true,
      safeArea: true,
    );
  }
}

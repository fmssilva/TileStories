import 'package:flutter/widgets.dart';
import 'package:flutter/foundation.dart';

/// Helper class to detect platform and device characteristics.
///
/// STUDENT NOTE: This provides simple boolean checks to adapt UI
/// based on platform (web vs native app) and device properties
/// (orientation, screen size).
///
/// WHY NEEDED?
/// - Different platforms have different UX patterns
/// - Web typically uses header navigation
/// - Mobile apps typically use bottom navigation
/// - Landscape needs different FAB positioning
///
/// USAGE:
/// ```dart
/// if (PlatformInfo.isApp(context)) {
///   return MobileLayout();
/// } else {
///   return WebLayout();
/// }
/// ```
class PlatformInfo {
  /// Is this running as a native app (Android/iOS)?
  static bool isApp(BuildContext context) {
    return !kIsWeb;
  }

  /// Is this running in a web browser?
  static bool isBrowser(BuildContext context) {
    return !isApp(context);
  }

  /// Is the device in landscape orientation?
  static bool isLandscape(BuildContext context) {
    return MediaQuery.of(context).orientation == Orientation.landscape;
  }

  /// Is the device in portrait orientation?
  static bool isPortrait(BuildContext context) {
    return !isLandscape(context);
  }

  /// Is this a phone-sized screen? (width < 600)
  static bool isPhone(BuildContext context) {
    return MediaQuery.of(context).size.width < 600;
  }

  /// Is this a tablet-sized or larger screen? (width >= 600)
  static bool isTablet(BuildContext context) {
    return !isPhone(context);
  }
}

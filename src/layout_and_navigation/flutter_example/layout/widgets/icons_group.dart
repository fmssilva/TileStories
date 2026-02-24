import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../utils/i18n/widgets/language_switcher.dart';
import '../../navigation/navigation.dart';
import '../../design/design_system.dart';

/// @index-widget layout-icons-group
/// @layer layout
/// @description Groups all header icon buttons (language, theme, hamburger menu)
/// @ui-type ConsumerWidget
/// @depends-on LanguageSwitcher, ThemeSwitcher, Hamburger, showHamburgerProvider
/// @domain-usage Used in Header to encapsulate right-side icons
///
/// ICONS GROUP COMPONENT
/// =====================
///
/// Encapsulates all header icons in a reusable component.
/// This follows the React pattern of component composition.
///
/// RESPONSIBILITIES:
/// - Render LanguageSwitcher and ThemeSwitcher consistently
/// - Show Hamburger button when showHamburgerProvider is true
/// - Provide clean API for width measurement
/// - Maintain consistent spacing between icons
///
/// DESIGN:
/// - Icons: Language, Theme, Hamburger (conditional)
/// - Spacing: 4px between icons (Spacing.xs)
/// - Layout: Row with MainAxisSize.min (shrink to fit)
class IconsGroup extends ConsumerWidget {
  const IconsGroup({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    // Watch hamburger visibility state
    final showHamburger = ref.watch(showHamburgerProvider);

    return Row(
      mainAxisSize: MainAxisSize.min,
      children: [
        // Language switcher (PT/EN with flags)
        const LanguageSwitcher(),

        const SizedBox(width: Spacing.xs), // 4px spacing
        // Theme toggle (light/dark mode)
        const ThemeSwitcher(),

        // Hamburger menu button (shown when navigation tabs don't fit)
        if (showHamburger) ...[
          const SizedBox(width: Spacing.xs),
          const Hamburger(),
        ],
      ],
    );
  }
}

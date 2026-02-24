import 'package:flutter/material.dart';

/// Simple footer component for mobile app layouts.
///
/// STUDENT NOTE: This is a placeholder BottomNavigationBar for demonstration.
/// Shows typical mobile app navigation with icons and labels.
/// Real implementation would connect to actual navigation state.
///
/// FEATURES (dummy):
/// - 5 navigation items (Home, Explore, Search, Saved, Profile)
/// - Icons and labels for each
/// - Fixed bottom navigation bar
///
/// USAGE:
/// ```dart
/// LayoutSlots(
///   footer: const FooterApp(),
///   body: MyContent(),
/// )
/// ```
class FooterApp extends StatelessWidget {
  const FooterApp({super.key});

  @override
  Widget build(BuildContext context) {
    return BottomNavigationBar(
      type: BottomNavigationBarType.fixed,
      items: const [
        BottomNavigationBarItem(icon: Icon(Icons.home), label: 'Home'),
        BottomNavigationBarItem(icon: Icon(Icons.explore), label: 'Explore'),
        BottomNavigationBarItem(icon: Icon(Icons.search), label: 'Search'),
        BottomNavigationBarItem(icon: Icon(Icons.bookmark), label: 'Saved'),
        BottomNavigationBarItem(icon: Icon(Icons.person), label: 'Profile'),
      ],
      onTap: (int index) {},
    );
  }
}

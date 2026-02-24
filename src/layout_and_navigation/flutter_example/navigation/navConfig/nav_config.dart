import 'nav_item.dart';
import '../../utils/i18n/models/translatable_string.dart';
import '../../domains/home/pages/home_page.dart';
import '../../domains/panorama/pages/panorama_page.dart';
import '../test/demo_pages/demo_nav1_page.dart';
import '../test/demo_pages/demo_nav1_child_a_page.dart';
import '../test/demo_pages/demo_nav1_child_b_page.dart';
import '../test/demo_pages/demo_nav1_child_c_page.dart';
import '../test/demo_pages/demo_nav2_page.dart';
import '../test/demo_pages/demo_nav3_page.dart';
import '../test/demo_pages/demo_nav4_page.dart';

/// NAVIGATION CONFIGURATION

final List<NavItem> navigationConfig = [
  // ============================================================================
  // HOME - Welcome page with hero section
  // ============================================================================
  NavItem(
    id: 'home',
    path: '/',
    label: const TranslatableString(pt: 'Início', en: 'Home'),
    labelShort: const TranslatableString(pt: 'Início', en: 'Home'),
    builder: (context) => const HomePage(),
    metadata: const NavMetadata(
      showInNav: true,
      requiresAuth: false,
      order: 1, // First item in navigation
      showInBreadcrumb: true,
    ),
  ),

  // ============================================================================
  // PANORAMA - Main panorama viewer
  // ============================================================================
  NavItem(
    id: 'panorama',
    path: '/panorama',
    label: const TranslatableString(pt: 'Panorama', en: 'Panorama'),
    labelShort: const TranslatableString(pt: 'Panorama', en: 'Panorama'),
    builder: (context) => const PanoramaPage(),
    metadata: const NavMetadata(
      showInNav: true,
      requiresAuth: false,
      order: 2, // Second item in navigation
      showInBreadcrumb: true,
    ),
  ),

  // ============================================================================
  // DEMO NAVIGATION 1 - Testing hierarchical navigation and breadcrumbs
  // ============================================================================
  NavItem(
    id: 'demo-nav1',
    path: '/demo-nav1',
    label: const TranslatableString(pt: 'Demo Nav 1', en: 'Demo Nav 1'),
    labelShort: const TranslatableString(pt: 'Demo1', en: 'Demo1'),
    builder: (context) => const DemoNav1Page(),
    metadata: const NavMetadata(
      showInNav: true,
      requiresAuth: false,
      order: 5, // After About
      showInBreadcrumb: true,
    ),
    // Add children to create hierarchy
    children: [
      NavItem(
        id: 'demo-nav1-child-a',
        path: '/demo-nav1/child-a',
        label: const TranslatableString(pt: 'Child A', en: 'Child A'),
        builder: (context) => const DemoNav1ChildAPage(),
        metadata: const NavMetadata(
          showInNav: true, // Show in accordion/tabs
          requiresAuth: false,
          order: 51,
          showInBreadcrumb: true,
        ),
      ),
      NavItem(
        id: 'demo-nav1-child-b',
        path: '/demo-nav1/child-b',
        label: const TranslatableString(pt: 'Child B', en: 'Child B'),
        builder: (context) => const DemoNav1ChildBPage(),
        metadata: const NavMetadata(
          showInNav: true, // Show in accordion/tabs
          requiresAuth: false,
          order: 52,
          showInBreadcrumb: true,
        ),
      ),
      NavItem(
        id: 'demo-nav1-child-c',
        path: '/demo-nav1/child-c',
        label: const TranslatableString(pt: 'Child C', en: 'Child C'),
        builder: (context) => const DemoNav1ChildCPage(),
        metadata: const NavMetadata(
          showInNav: true, // Show in accordion/tabs
          requiresAuth: false,
          order: 53,
          showInBreadcrumb: true,
        ),
      ),
    ],
  ),

  // ============================================================================
  // DEMO NAVIGATION 2 - Testing tab overflow
  // ============================================================================
  NavItem(
    id: 'demo-nav2',
    path: '/demo-nav2',
    label: const TranslatableString(pt: 'Demo Nav 2', en: 'Demo Nav 2'),
    labelShort: const TranslatableString(pt: 'Demo2', en: 'Demo2'),
    builder: (context) => const DemoNav2Page(),
    metadata: const NavMetadata(
      showInNav: true,
      requiresAuth: false,
      order: 6,
      showInBreadcrumb: true,
    ),
  ),

  // ============================================================================
  // DEMO NAVIGATION 3 - Testing tab overflow
  // ============================================================================
  NavItem(
    id: 'demo-nav3',
    path: '/demo-nav3',
    label: const TranslatableString(pt: 'Demo Nav 3', en: 'Demo Nav 3'),
    labelShort: const TranslatableString(pt: 'Demo3', en: 'Demo3'),
    builder: (context) => const DemoNav3Page(),
    metadata: const NavMetadata(
      showInNav: true,
      requiresAuth: false,
      order: 7,
      showInBreadcrumb: true,
    ),
  ),

  // ============================================================================
  // DEMO NAVIGATION 4 - Testing tab overflow
  // ============================================================================
  NavItem(
    id: 'demo-nav4',
    path: '/demo-nav4',
    label: const TranslatableString(pt: 'Demo Nav 4', en: 'Demo Nav 4'),
    labelShort: const TranslatableString(pt: 'Demo4', en: 'Demo4'),
    builder: (context) => const DemoNav4Page(),
    metadata: const NavMetadata(
      showInNav: true,
      requiresAuth: false,
      order: 8,
      showInBreadcrumb: true,
    ),
  ),
];

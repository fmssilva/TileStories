/**
 * LAYOUT AND NAVIGATION - MAIN EXPORTS
 * =====================================
 * 
 * Central export point for all layout and navigation features.
 * 
 * Architecture:
 * - Config: Navigation structure (single source of truth)
 * - Context: Navigation state management
 * - Hooks: Clean API for components
 * - Components: UI elements
 * - Utils: Helper functions
 */

// ============================================================================
// COMPONENTS
// ============================================================================

export { Header, Footer, MainLayout, Breadcrumbs, MobileMenu, BackToTop } from './components';

// ============================================================================
// CONTEXT & PROVIDER
// ============================================================================

export { NavigationProvider, useNavigationContext } from './context';

// ============================================================================
// HOOKS
// ============================================================================

export { useNavigation, useBreadcrumbs, useMobileMenu, useHeaderScroll } from './hooks';

// ============================================================================
// CONFIGURATION
// ============================================================================

export { navigationConfig } from './config';

// ============================================================================
// UTILITIES
// ============================================================================

export {
    // Label extraction
    getNavItemLabel,
    // Navigation query helpers
    getMainNavItems,
    getFooterNavItems,
    findNavItemByPath,
    getAllNavItems,
    // Breadcrumb utilities
    buildBreadcrumbTrail,
    populateParentReferences,
    isPathActive,
    extractParam,
    buildPath,
    getItemDepth,
    findNavItemById,
    getSiblings,
    getNextSibling,
    getPreviousSibling,
    // Route generation
    generateRoutesFromConfig,
} from './utils';

// ============================================================================
// TYPES
// ============================================================================

export type {
    NavItem,
    NavItemMetadata,
    NavigationState,
    HistoryEntry,
    NavigationAPI,
    NavigateOptions,
    MobileMenuState,
    ScrollState,
    ScrollOptions,
    HeaderProps,
    FooterProps,
    MainLayoutProps,
    BreadcrumbsProps,
    MobileMenuProps,
    BackToTopProps,
} from './types';

/**
 * NAVIGATION & LAYOUT TYPES
 * =========================
 * 
 * TypeScript types for the navigation and layout system.
 * These are the foundation for the entire navigation architecture.
 */

import { ComponentType, ReactNode } from 'react';
import type { InlineTranslation } from '@/utils/language';

// ============================================================================
// NAVIGATION CONFIGURATION TYPES
// ============================================================================

/**
 * Metadata for navigation items
 * Controls visibility, authentication, and behavior
 */
export interface NavItemMetadata {
    /** Show in main navigation menu */
    showInNav?: boolean;
    /** Show in breadcrumb trail */
    showInBreadcrumb?: boolean;
    /** Requires authentication to access */
    requiresAuth?: boolean;
    /** Order in navigation (lower = first) */
    order?: number;
    /** Icon name or component */
    icon?: string | ReactNode;
    /** Preserve scroll position on navigation */
    preserveScroll?: boolean;
    /** Replace history entry instead of push */
    replaceHistory?: boolean;
    /** Return to this page after authentication */
    returnAfterAuth?: boolean;
}

/**
 * Navigation item representing a route/page
 * Forms a tree structure for nested navigation
 */
export interface NavItem {
    /** Unique identifier */
    id: string;
    /** Display label (full) - supports i18n with { pt: '...', en: '...' } */
    label: string | InlineTranslation;
    /** Short label for mobile/compact views - supports i18n */
    labelShort?: string | InlineTranslation;
    /** Route path (React Router format) */
    path: string;
    /** Component to render (lazy loaded) */
    component?: ComponentType;
    /** Visual hierarchy level */
    styleLevel: 'top' | 'second' | 'inner' | 'section';
    /** Child navigation items */
    children?: NavItem[];
    /** Additional metadata */
    metadata?: NavItemMetadata;
    /** Parent reference (auto-populated by utils) */
    parent?: NavItem;
}

// ============================================================================
// NAVIGATION HISTORY TYPES
// ============================================================================

/**
 * Single entry in navigation history
 * Tracks where user has been with metadata
 */
export interface HistoryEntry {
    /** Route path */
    path: string;
    /** When this navigation occurred */
    timestamp: number;
    /** Scroll position at time of navigation */
    scrollPosition: number;
    /** Custom state data */
    state?: unknown;
}

/**
 * Navigation state managed by context
 * Central state for all navigation features
 */
export interface NavigationState {
    /** Current active path */
    currentPath: string;
    /** Breadcrumb trail for current page */
    breadcrumbTrail: NavItem[];
    /** Navigation history (last 50 entries) */
    navigationHistory: HistoryEntry[];
    /** Previous path before current */
    previousPath: string | null;
    /** Saved scroll positions by path */
    scrollPositions: Map<string, number>;
}

// ============================================================================
// HOOK RETURN TYPES
// ============================================================================

/**
 * Return type for useNavigation hook
 * Main API for programmatic navigation
 */
export interface NavigationAPI {
    /** Navigate to a path */
    goTo: (path: string, options?: NavigateOptions) => void;
    /** Go back one step */
    goBack: () => void;
    /** Go back or to home if no history */
    goBackOrHome: () => void;
    /** Navigate with return path for auth */
    navigateWithReturn: (path: string) => void;
    /** Return from auth to original page */
    returnFromAuth: () => void;
    /** Current active path */
    currentPath: string;
    /** Previous path */
    previousPath: string | null;
    /** Can go back in history */
    canGoBack: boolean;
}

/**
 * Options for navigation
 */
export interface NavigateOptions {
    /** Custom state to pass */
    state?: unknown;
    /** Replace current history entry */
    replace?: boolean;
    /** Don't scroll to top */
    preserveScroll?: boolean;
}

/**
 * Return type for useMobileMenu hook
 */
export interface MobileMenuState {
    /** Is mobile menu open */
    isOpen: boolean;
    /** Open the menu */
    open: () => void;
    /** Close the menu */
    close: () => void;
    /** Toggle menu state */
    toggle: () => void;
}

// ============================================================================
// SCROLL TYPES
// ============================================================================

/**
 * Scroll state for header behavior
 */
export interface ScrollState {
    /** Scrolled past threshold */
    isScrolled: boolean;
    /** Header should be visible */
    isVisible: boolean;
    /** Current scroll Y position */
    scrollY: number;
    /** Scroll direction */
    scrollDirection: 'up' | 'down' | null;
}

/**
 * Options for scroll behavior
 */
export interface ScrollOptions {
    /** Scroll threshold for "scrolled" state */
    scrollThreshold?: number;
    /** Distance before hiding header */
    hideThreshold?: number;
    /** Enable smart hide/show */
    enableHideShow?: boolean;
    /** Debounce delay in ms */
    debounceMs?: number;
}

// ============================================================================
// COMPONENT PROP TYPES
// ============================================================================

/**
 * Props for Header component
 */
export interface HeaderProps {
    /** Additional navigation items */
    children?: ReactNode;
    /** Custom logo component */
    logo?: ReactNode;
    /** Additional header actions (buttons, etc.) */
    actions?: ReactNode;
    /** Show scroll progress indicator */
    showProgress?: boolean;
}

/**
 * Props for Footer component
 */
export interface FooterProps {
    /** Additional footer content */
    children?: ReactNode;
    /** Show navigation links */
    showNavigation?: boolean;
    /** Custom copyright text */
    copyright?: string;
}

/**
 * Props for MainLayout component
 */
export interface MainLayoutProps {
    /** Page content */
    children: ReactNode;
    /** Show footer navigation */
    showFooterNavigation?: boolean;
    /** Custom header actions */
    headerActions?: ReactNode;
    /** Show breadcrumbs */
    showBreadcrumbs?: boolean;
}

/**
 * Props for Breadcrumbs component
 */
export interface BreadcrumbsProps {
    /** Custom separator */
    separator?: ReactNode;
    /** Maximum items to show */
    maxItems?: number;
    /** Show home icon instead of text */
    showHomeIcon?: boolean;
}

/**
 * Props for MobileMenu component
 */
export interface MobileMenuProps {
    /** Is menu open */
    isOpen: boolean;
    /** Close callback */
    onClose: () => void;
    /** Navigation items to display */
    items?: NavItem[];
}

/**
 * Props for BackToTop component
 */
export interface BackToTopProps {
    /** Smooth scroll duration in ms */
    scrollDuration?: number;
    /** Show scroll progress ring */
    showProgress?: boolean;
    /** Offset from edge in pixels */
    offset?: number;
    /** Min scroll before showing button */
    showAfter?: number;
}

// ============================================================================
// ADAPTIVE NAVIGATION TYPES
// ============================================================================

/**
 * Navigation display mode
 */
export type NavigationMode = 'tabs' | 'partial' | 'hamburger';

/**
 * Result from adaptive navigation calculation
 */
export interface AdaptiveNavigationState {
    /** Current display mode */
    mode: NavigationMode;
    /** Items to show as tabs */
    visibleItems: NavItem[];
    /** Items to show in "More" dropdown */
    overflowItems: NavItem[];
    /** Whether to show hamburger menu */
    showHamburger: boolean;
    /** Whether to show "More" button */
    showMore: boolean;
}

/**
 * Props for MoreMenu component
 */
export interface MoreMenuProps {
    /** Items to display in the menu */
    items: NavItem[];
    /** Is the menu open */
    isOpen: boolean;
    /** Toggle menu state */
    onToggle: () => void;
    /** Close menu */
    onClose: () => void;
    /** Current active path */
    activePath: string;
    /** DEPRECATED: Now uses fixed LAYOUT constants instead */
    componentsHeight?: number;
}

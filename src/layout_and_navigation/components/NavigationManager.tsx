/**
 * NAVIGATION MANAGER COMPONENT
 * ============================
 * 
 * Centralized component that manages the navigation display strategy.
 * 
 * Determines what to show based on available width:
 * - All tabs (when space permits)
 * - Some tabs + "More" button (partial overflow)
 * - Hamburger menu only (minimal space)
 * 
 * This component encapsulates ALL the logic for adaptive navigation,
 * keeping the Header component clean and focused on layout.
 * 
 * RESPONSIBILITIES:
 * - Calculate how many tabs fit in available width
 * - Decide when to show "More" button vs Hamburger
 * - Render appropriate navigation UI
 * - Handle state management for "More" dropdown
 * 
 * DOES NOT HANDLE:
 * - Width measurement (passed as prop from parent)
 * - Mobile menu rendering (delegated to MobileMenu component)
 * - Logo/branding (Header's responsibility)
 */

import { useState, useCallback, useEffect } from 'react';
import { Link, useLocation } from 'react-router-dom';
import { useLanguage } from '@/utils/language';
import { LAYOUT } from '@/design';
import { MoreMenu } from './MoreMenu';
import { getNavItemLabel } from '../utils';
import type { NavItem } from '../types';


// ============================================================================
// TYPES
// ============================================================================

export interface NavigationManagerProps {
    /** Navigation items to display */
    navItems: NavItem[];
    /** EXACT available width for navigation tabs (in pixels) */
    availableWidth: number;
    /** Exact height for components (in pixels) - matches componentsHeight from layout */
    componentsHeight: number;
    /** Callback when hamburger is clicked */
    onHamburgerClick: () => void;
    /** Additional children to render in nav section (deprecated - use iconsGroup instead) */
    children?: React.ReactNode;
}

interface NavigationState {
    mode: 'tabs' | 'partial' | 'hamburger';
    visibleItems: NavItem[];
    overflowItems: NavItem[];
    showMore: boolean;
}

// ============================================================================
// NAVIGATION MANAGER COMPONENT
// ============================================================================

export function NavigationManager({
    navItems,
    availableWidth,
    componentsHeight,
    children,
    onHamburgerClick,
}: NavigationManagerProps) {
    const { language } = useLanguage();
    const location = useLocation();
    const [isMoreMenuOpen, setIsMoreMenuOpen] = useState(false);
    const [navState, setNavState] = useState<NavigationState>({
        mode: 'tabs',
        visibleItems: navItems,
        overflowItems: [],
        showMore: false,
    });

    // Get constants from LAYOUT
    const { TAB_WIDTH_ESTIMATE, MORE_BUTTON_WIDTH, MIN_TABS_BEFORE_HAMBURGER, HAMBURGER_THRESHOLD } = LAYOUT;

    // ────────────────────────────────────────────────────────────────────────
    // CALCULATE NAVIGATION LAYOUT
    // ────────────────────────────────────────────────────────────────────────

    const calculateLayout = useCallback(() => {
        const totalItems = navItems.length;

        // CASE 1: Not enough space for minimum tabs → Show hamburger
        if (availableWidth < HAMBURGER_THRESHOLD) {
            setNavState({
                mode: 'hamburger',
                visibleItems: [],
                overflowItems: navItems,
                showMore: false,
            });
            return;
        }

        // CASE 2: Calculate how many tabs fit
        let spaceForTabs = availableWidth;
        let needsMoreButton = false;

        // Calculate total space needed for all tabs
        const spaceNeededForAllTabs = totalItems * TAB_WIDTH_ESTIMATE;

        // Check if we need "More" button
        if (spaceNeededForAllTabs > spaceForTabs) {
            // Can't fit all items - reserve space for "More"
            spaceForTabs -= MORE_BUTTON_WIDTH;
            needsMoreButton = true;
        }

        // Calculate how many items can be visible
        const maxVisibleItems = Math.max(
            needsMoreButton ? MIN_TABS_BEFORE_HAMBURGER : 1, // Minimum tabs if showing "More"
            Math.floor(spaceForTabs / TAB_WIDTH_ESTIMATE)
        );

        const numVisible = Math.min(maxVisibleItems, totalItems);
        const visibleItems = navItems.slice(0, numVisible);
        const overflowItems = navItems.slice(numVisible);

        // Determine final mode
        const mode = overflowItems.length > 0 ? 'partial' : 'tabs';

        setNavState({
            mode,
            visibleItems,
            overflowItems,
            showMore: overflowItems.length > 0,
        });
    }, [navItems, availableWidth, TAB_WIDTH_ESTIMATE, MORE_BUTTON_WIDTH, MIN_TABS_BEFORE_HAMBURGER, HAMBURGER_THRESHOLD]);

    // Recalculate when width or items change
    useEffect(() => {
        calculateLayout();
    }, [calculateLayout]);

    // ────────────────────────────────────────────────────────────────────────
    // RENDER
    // ────────────────────────────────────────────────────────────────────────

    // HAMBURGER MODE
    if (navState.mode === 'hamburger') {
        // Use FIXED hamburger icon size from LAYOUT
        const hamburgerSize = LAYOUT.HAMBURGER_ICON_SIZE;  // 28px fixed

        // Handler with stopPropagation to prevent click bubbling
        const handleHamburgerClick = (e: React.MouseEvent) => {
            e.stopPropagation();
            onHamburgerClick();
        };

        return (
            <div
                className="flex items-center justify-end gap-2 sm:gap-3 flex-shrink-0"
                style={{ width: availableWidth > 0 ? `${availableWidth}px` : 'auto' }}
            >
                <button
                    onClick={handleHamburgerClick}
                    className="p-2 rounded-md transition-colors hover:bg-white/20 relative"
                    style={{ zIndex: 10 }} // Ensure it's above other elements
                    aria-label="Open menu"
                    type="button"
                >
                    <svg
                        style={{
                            width: `${hamburgerSize}px`,
                            height: `${hamburgerSize}px`,
                        }}
                        className="text-white"
                        fill="none"
                        stroke="currentColor"
                        viewBox="0 0 24 24"
                    >
                        <path
                            strokeLinecap="round"
                            strokeLinejoin="round"
                            strokeWidth={2}
                            d="M4 6h16M4 12h16M4 18h16"
                        />
                    </svg>
                </button>
            </div>
        );
    }    // TABS MODE (full or partial)
    // NavigationManager only renders tabs - icons are handled by Header
    return (
        <div
            className="flex items-center gap-2 sm:gap-3 lg:gap-4 flex-shrink-0"
            style={{
                width: availableWidth > 0 ? `${availableWidth}px` : 'auto',
                maxWidth: availableWidth > 0 ? `${availableWidth}px` : 'none',
            }}
        >
            {navState.visibleItems.map((item) => (
                <NavLink
                    key={item.id}
                    to={item.path}
                    isActive={location.pathname === item.path}
                >
                    {getNavItemLabel(item, language, true)}
                </NavLink>
            ))}

            {/* More menu for overflow items */}
            {navState.showMore && (
                <MoreMenu
                    items={navState.overflowItems}
                    isOpen={isMoreMenuOpen}
                    onToggle={() => setIsMoreMenuOpen(!isMoreMenuOpen)}
                    onClose={() => setIsMoreMenuOpen(false)}
                    activePath={location.pathname}
                    componentsHeight={componentsHeight}
                />
            )}

            {children}
        </div>
    );
}

// ============================================================================
// NAV LINK COMPONENT (private to this module)
// ============================================================================

interface NavLinkProps {
    to: string;
    children: React.ReactNode;
    isActive: boolean;
}

function NavLink({ to, children, isActive }: NavLinkProps) {
    // Use FIXED sizes from LAYOUT constants (no more percentage calculations!)
    const fontSize = LAYOUT.NAV_TAB_FONT_SIZE;        // 20px
    const paddingX = LAYOUT.NAV_TAB_PADDING_X;        // 24px
    const paddingY = LAYOUT.NAV_TAB_PADDING_Y;        // 12px

    return (
        <Link
            to={to}
            className={`rounded-md font-medium transition-all duration-200 whitespace-nowrap ${isActive
                ? 'bg-white/25 text-white'
                : 'text-white/90 hover:bg-white/15 hover:text-white'
                }`}
            style={{
                fontSize: `${fontSize}px`,
                paddingLeft: `${paddingX}px`,
                paddingRight: `${paddingX}px`,
                paddingTop: `${paddingY}px`,
                paddingBottom: `${paddingY}px`,
            }}
        >
            {children}
        </Link>
    );
}

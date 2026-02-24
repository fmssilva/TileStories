/**
 * USE ADAPTIVE NAVIGATION HOOK
 * ============================
 * 
 * Intelligently adapts navigation display based on available space.
 * Measures container width and determines which nav items to show as tabs,
 * which to overflow into a "More" dropdown, or when to switch to hamburger menu.
 * 
 * Logic:
 * - Measure available width using ResizeObserver
 * - Calculate space needed for: logo, icons (3), nav items, "More" button
 * - Show as many tabs as possible
 * - When space < 2 tabs + More button: switch to hamburger
 * - Otherwise: show visible tabs + More button with overflow items
 */

import { useState, useEffect, useCallback } from 'react';
import type { NavItem, AdaptiveNavigationState, NavigationMode } from '../types';

// ============================================================================
// CONSTANTS
// ============================================================================

/** Estimated width per navigation tab (in pixels) */
const TAB_WIDTH_ESTIMATE = 120;

/** Width of the "More" button (in pixels) */
const MORE_BUTTON_WIDTH = 80;

/** Width for logo + site name (in pixels) */
const LOGO_WIDTH = 200;

/** Width for icons (language + theme + margin) (in pixels) */
const ICONS_WIDTH = 140;

/** Minimum space to stay in tabs mode (2 tabs + More button) */
const MIN_TABS_MODE_WIDTH = LOGO_WIDTH + ICONS_WIDTH + (2 * TAB_WIDTH_ESTIMATE) + MORE_BUTTON_WIDTH + 32;

/**
 * Hook for adaptive navigation
 * 
 * @param navItems - All navigation items
 * @param containerRef - Ref to the navigation container
 * @returns Adaptive navigation state
 * 
 * @example
 * const containerRef = useRef<HTMLElement>(null);
 * const { mode, visibleItems, overflowItems, showMore, showHamburger } = 
 *   useAdaptiveNavigation(navItems, containerRef);
 */
export function useAdaptiveNavigation(
    navItems: NavItem[],
    containerRef: React.RefObject<HTMLElement | null>
): AdaptiveNavigationState {
    const [state, setState] = useState<AdaptiveNavigationState>({
        mode: 'tabs',
        visibleItems: navItems,
        overflowItems: [],
        showHamburger: false,
        showMore: false,
    });

    const calculateLayout = useCallback(() => {
        if (!containerRef.current) return;

        const containerWidth = containerRef.current.offsetWidth;
        const totalItems = navItems.length;

        // Calculate available space for navigation items
        const availableNavSpace = containerWidth - LOGO_WIDTH - ICONS_WIDTH - 32; // 32 for padding

        console.log('[useAdaptiveNavigation] Calculating layout:', {
            containerWidth,
            logoWidth: LOGO_WIDTH,
            iconsWidth: ICONS_WIDTH,
            availableNavSpace,
            totalItems,
            minTabsModeWidth: MIN_TABS_MODE_WIDTH - LOGO_WIDTH - ICONS_WIDTH,
        });

        // Decide mode based on available space
        if (availableNavSpace < MIN_TABS_MODE_WIDTH - LOGO_WIDTH - ICONS_WIDTH) {
            // Not enough space for tabs mode - switch to hamburger
            console.log('[useAdaptiveNavigation] → HAMBURGER mode (not enough space)');
            setState({
                mode: 'hamburger',
                visibleItems: [],
                overflowItems: navItems,
                showHamburger: true,
                showMore: false,
            });
            return;
        }

        // Calculate how many tabs can fit
        // Reserve space for "More" button if we'll need it
        let spaceForTabs = availableNavSpace;
        let needsMoreButton = false;

        // Try to fit all items first
        const spaceNeeded = totalItems * TAB_WIDTH_ESTIMATE;

        if (spaceNeeded > spaceForTabs) {
            // Can't fit all items - reserve space for "More" button
            spaceForTabs -= MORE_BUTTON_WIDTH;
            needsMoreButton = true;
        }

        // Calculate how many items can be visible
        const maxVisibleItems = Math.max(
            needsMoreButton ? 2 : 1, // Minimum 2 tabs if showing More button
            Math.floor(spaceForTabs / TAB_WIDTH_ESTIMATE)
        );

        const numVisible = Math.min(maxVisibleItems, totalItems);
        const visibleItems = navItems.slice(0, numVisible);
        const overflowItems = navItems.slice(numVisible);

        // Final mode determination
        let mode: NavigationMode;
        if (overflowItems.length > 0) {
            mode = 'partial'; // Some items visible, some in More
        } else {
            mode = 'tabs'; // All items visible
        }

        console.log('[useAdaptiveNavigation] → Mode:', mode, {
            visibleCount: visibleItems.length,
            overflowCount: overflowItems.length,
            showMore: overflowItems.length > 0,
        });

        setState({
            mode,
            visibleItems,
            overflowItems,
            showHamburger: false,
            showMore: overflowItems.length > 0,
        });
    }, [navItems, containerRef]);

    // Set up ResizeObserver to watch container size changes
    useEffect(() => {
        const container = containerRef.current;
        if (!container) return;

        // Initial calculation
        calculateLayout();

        // Create ResizeObserver
        const resizeObserver = new ResizeObserver(() => {
            // Use RAF to debounce and optimize
            requestAnimationFrame(calculateLayout);
        });

        resizeObserver.observe(container);

        // Also listen to window resize as a fallback
        window.addEventListener('resize', calculateLayout);

        return () => {
            resizeObserver.disconnect();
            window.removeEventListener('resize', calculateLayout);
        };
    }, [calculateLayout, containerRef]);

    // Recalculate when nav items change
    useEffect(() => {
        calculateLayout();
    }, [navItems, calculateLayout]);

    return state;
}

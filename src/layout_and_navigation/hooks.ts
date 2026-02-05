// ============================================================================
// HEADER SCROLL BEHAVIOR HOOK
// ============================================================================

import { useState, useEffect, useCallback } from 'react';

interface ScrollState {
    isScrolled: boolean;
    isVisible: boolean;
    scrollY: number;
    scrollDirection: 'up' | 'down' | null;
}

interface UseHeaderScrollOptions {
    /** Scroll threshold to trigger "scrolled" state (default: 10px) */
    scrollThreshold?: number;
    /** Scroll distance to trigger hide/show behavior (default: 100px) */
    hideThreshold?: number;
    /** Enable smart hide/show behavior (default: true) */
    enableHideShow?: boolean;
    /** Debounce scroll events (default: 10ms) */
    debounceMs?: number;
}

/**
 * Custom hook to manage header scroll behavior
 * 
 * Features:
 * - Tracks scroll position and direction
 * - Manages header visibility with smart hide/show
 * - Provides scroll-based styling states
 * - Optimized with debouncing and RAF
 * 
 * @param options - Configuration options for scroll behavior
 * @returns ScrollState object with current scroll information
 */
export function useHeaderScroll(options: UseHeaderScrollOptions = {}) {
    const {
        scrollThreshold = 10,
        hideThreshold = 100,
        enableHideShow = true,
        debounceMs = 10,
    } = options;

    const [scrollState, setScrollState] = useState<ScrollState>({
        isScrolled: false,
        isVisible: true,
        scrollY: 0,
        scrollDirection: null,
    });

    const [lastScrollY, setLastScrollY] = useState(0);
    const [timeoutId, setTimeoutId] = useState<NodeJS.Timeout | null>(null);

    const updateScrollState = useCallback(() => {
        const currentScrollY = window.scrollY;
        const scrollDiff = currentScrollY - lastScrollY;

        // Determine scroll direction
        let direction: 'up' | 'down' | null = null;
        if (Math.abs(scrollDiff) > 5) { // Minimum threshold for direction detection
            direction = scrollDiff > 0 ? 'down' : 'up';
        }

        // Calculate visibility based on scroll behavior
        let isVisible = true;
        if (enableHideShow && currentScrollY > hideThreshold) {
            // Hide header when scrolling down, show when scrolling up
            if (direction === 'down' && scrollDiff > 0) {
                isVisible = false;
            } else if (direction === 'up' && scrollDiff < 0) {
                isVisible = true;
            } else {
                // Keep current state if no significant scroll change
                isVisible = scrollState.isVisible;
            }
        }

        // Special case: always show header when near top
        if (currentScrollY <= scrollThreshold) {
            isVisible = true;
        }

        const newState: ScrollState = {
            isScrolled: currentScrollY > scrollThreshold,
            isVisible,
            scrollY: currentScrollY,
            scrollDirection: direction,
        };

        setScrollState(newState);
        setLastScrollY(currentScrollY);
    }, [lastScrollY, scrollState.isVisible, scrollThreshold, hideThreshold, enableHideShow]);

    const debouncedUpdateScrollState = useCallback(() => {
        if (timeoutId) {
            clearTimeout(timeoutId);
        }

        const newTimeoutId = setTimeout(updateScrollState, debounceMs);
        setTimeoutId(newTimeoutId);
    }, [updateScrollState, timeoutId, debounceMs]);

    useEffect(() => {
        // Use RAF for smooth scroll handling
        let rafId: number;

        const handleScroll = () => {
            rafId = requestAnimationFrame(debouncedUpdateScrollState);
        };

        // Initial state
        updateScrollState();

        // Add scroll listener
        window.addEventListener('scroll', handleScroll, { passive: true });

        // Cleanup
        return () => {
            window.removeEventListener('scroll', handleScroll);
            if (rafId) {
                cancelAnimationFrame(rafId);
            }
            if (timeoutId) {
                clearTimeout(timeoutId);
            }
        };
    }, [debouncedUpdateScrollState, updateScrollState, timeoutId]);

    return scrollState;
}

// ============================================================================
// DEPRECATED: Use useHeaderScroll directly instead
// ============================================================================
// The useHeaderStyles hook has been simplified. Use useHeaderScroll + domain colors instead.
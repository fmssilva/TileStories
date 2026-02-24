/**
 * USE HEADER SCROLL HOOK
 * ======================
 * 
 * Track scroll position and direction for smart header behavior.
 * Provides scroll state for hide/show and styling effects.
 */

import { useState, useEffect, useCallback } from 'react';
import type { ScrollState, ScrollOptions } from '../types';

/**
 * Use Header Scroll
 * 
 * Tracks scroll position and provides state for header behavior.
 * Features:
 * - Scroll direction tracking
 * - Smart hide/show based on scroll
 * - Debounced for performance
 * - RAF optimized
 * 
 * @param options - Configuration for scroll behavior
 * 
 * @example
 * const { isScrolled, isVisible, scrollY } = useHeaderScroll({
 *   scrollThreshold: 10,
 *   hideThreshold: 150,
 *   enableHideShow: true
 * });
 */
export function useHeaderScroll(options: ScrollOptions = {}): ScrollState {
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

    const updateScrollState = useCallback(() => {
        const currentScrollY = window.scrollY;
        const scrollDiff = currentScrollY - lastScrollY;

        // Determine scroll direction (with minimum threshold)
        let direction: 'up' | 'down' | null = null;
        if (Math.abs(scrollDiff) > 5) {
            direction = scrollDiff > 0 ? 'down' : 'up';
        }

        // Calculate visibility based on scroll behavior
        let isVisible = true;
        if (enableHideShow && currentScrollY > hideThreshold) {
            // Hide header when scrolling down, show when scrolling up
            if (direction === 'down') {
                isVisible = false;
            } else if (direction === 'up') {
                isVisible = true;
            } else {
                // Keep current state if no significant scroll change
                isVisible = scrollState.isVisible;
            }
        }

        // Always show header when near top
        if (currentScrollY <= scrollThreshold) {
            isVisible = true;
        }

        setScrollState({
            isScrolled: currentScrollY > scrollThreshold,
            isVisible,
            scrollY: currentScrollY,
            scrollDirection: direction,
        });

        setLastScrollY(currentScrollY);
    }, [lastScrollY, scrollState.isVisible, scrollThreshold, hideThreshold, enableHideShow]);

    useEffect(() => {
        let rafId: number;
        let timeoutId: NodeJS.Timeout;

        const debouncedUpdate = () => {
            clearTimeout(timeoutId);
            timeoutId = setTimeout(updateScrollState, debounceMs);
        };

        const handleScroll = () => {
            rafId = requestAnimationFrame(debouncedUpdate);
        };

        // Initial state
        updateScrollState();

        // Add scroll listener
        window.addEventListener('scroll', handleScroll, { passive: true });

        // Cleanup
        return () => {
            window.removeEventListener('scroll', handleScroll);
            if (rafId) cancelAnimationFrame(rafId);
            if (timeoutId) clearTimeout(timeoutId);
        };
    }, [updateScrollState, debounceMs]);

    return scrollState;
}

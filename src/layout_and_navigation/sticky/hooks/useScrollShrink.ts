/**
 * USE SCROLL SHRINK HOOK
 * =======================
 *
 * Reusable hook for sticky elements that shrink/expand with scroll hysteresis.
 * Prevents flickering by using hysteresis thresholds and smooth transitions.
 *
 * FEATURES:
 * - Hysteresis to prevent rapid toggling (trembling)
 * - Configurable shrink/expand thresholds
 * - Smooth CSS transitions
 * - Throttled scroll handling for performance
 * - TypeScript support
 *
 * USAGE:
 * ```tsx
 * const isShrunk = useScrollShrink({
 *   shrinkThreshold: 50,  // Shrink when scrolled 50px past trigger
 *   expandThreshold: 30,  // Expand when scrolled back to 30px
 * });
 *
 * return (
 *   <div className={isShrunk ? 'compact' : 'expanded'}>
 *     Content that shrinks
 *   </div>
 * );
 * ```
 */

import { useState, useEffect, useCallback, useRef } from 'react';

// ═══════════════════════════════════════════════════════════════════════════
// TYPES
// ═══════════════════════════════════════════════════════════════════════════

export interface UseScrollShrinkOptions {
    /** Scroll position where element should start shrinking (in pixels) */
    shrinkThreshold?: number;

    /** Scroll position where element should expand again (in pixels) */
    expandThreshold?: number;

    /** Element to trigger on (defaults to window) */
    triggerElement?: HTMLElement | Window;

    /** Whether to trigger on scroll up vs down */
    triggerOnScrollUp?: boolean;
}

// ═══════════════════════════════════════════════════════════════════════════
// HOOK
// ═══════════════════════════════════════════════════════════════════════════

/**
 * Hook for sticky elements that shrink/expand with scroll hysteresis
 *
 * Prevents flickering by using different thresholds for shrink vs expand,
 * creating a "dead zone" that prevents rapid toggling.
 *
 * @param options - Configuration options
 * @returns isShrunk - Whether the element should be in shrunk state
 *
 * @example
 * // Basic usage with default thresholds
 * const isShrunk = useScrollShrink();
 *
 * @example
 * // Custom thresholds for hysteresis
 * const isShrunk = useScrollShrink({
 *   shrinkThreshold: 100,  // Shrink at 100px scroll
 *   expandThreshold: 50,   // Expand at 50px scroll (30px hysteresis)
 * });
 */
export function useScrollShrink(options: UseScrollShrinkOptions = {}): boolean {
    const {
        shrinkThreshold = 50,
        expandThreshold = 30,
        triggerElement,
        triggerOnScrollUp = false,
    } = options;

    const [isShrunk, setIsShrunk] = useState(false);
    const lastScrollY = useRef(0);
    const ticking = useRef(false);

    // Validate thresholds (expand should be less than shrink for hysteresis)
    const validShrinkThreshold = Math.max(shrinkThreshold, expandThreshold + 10);
    const validExpandThreshold = Math.min(expandThreshold, shrinkThreshold - 10);

    const handleScroll = useCallback(() => {
        if (ticking.current) return;

        ticking.current = true;

        requestAnimationFrame(() => {
            const element = triggerElement || window;
            const scrollY = element instanceof Window
                ? element.scrollY
                : element.scrollTop;

            const scrollDirection = scrollY > lastScrollY.current ? 'down' : 'up';
            lastScrollY.current = scrollY;

            // Only trigger on scroll down unless specified otherwise
            if (!triggerOnScrollUp && scrollDirection === 'up') {
                ticking.current = false;
                return;
            }

            if (scrollY > validShrinkThreshold && !isShrunk) {
                setIsShrunk(true);
                console.log(`📏 [useScrollShrink] SHRINK triggered at scrollY: ${scrollY}px (threshold: ${validShrinkThreshold}px)`);
            } else if (scrollY <= validExpandThreshold && isShrunk) {
                setIsShrunk(false);
                console.log(`📏 [useScrollShrink] EXPAND triggered at scrollY: ${scrollY}px (threshold: ${validExpandThreshold}px)`);
            }

            ticking.current = false;
        });
    }, [isShrunk, validShrinkThreshold, validExpandThreshold, triggerElement, triggerOnScrollUp]);

    useEffect(() => {
        const element = triggerElement || window;
        element.addEventListener('scroll', handleScroll, { passive: true });

        // Initial check
        handleScroll();

        return () => {
            element.removeEventListener('scroll', handleScroll);
        };
    }, [handleScroll, triggerElement]);

    return isShrunk;
}

// ═══════════════════════════════════════════════════════════════════════════
// UTILITY FUNCTIONS
// ═══════════════════════════════════════════════════════════════════════════

/**
 * Get CSS classes for smooth shrink transitions
 *
 * @param isShrunk - Whether element is shrunk
 * @param shrinkClass - CSS class to apply when shrunk
 * @param expandClass - CSS class to apply when expanded
 * @returns Combined CSS classes with transition
 */
export function getShrinkClasses(
    isShrunk: boolean,
    shrinkClass: string,
    expandClass: string
): string {
    return `transition-all duration-300 ease-out ${isShrunk ? shrinkClass : expandClass}`;
}

/**
 * Get inline styles for shrink transitions (alternative to CSS classes)
 *
 * @param isShrunk - Whether element is shrunk
 * @param shrinkStyles - Styles when shrunk
 * @param expandStyles - Styles when expanded
 * @returns Combined styles with transition
 */
export function getShrinkStyles(
    isShrunk: boolean,
    shrinkStyles: React.CSSProperties,
    expandStyles: React.CSSProperties
): React.CSSProperties {
    return {
        transition: 'all 300ms ease-out',
        ...isShrunk ? shrinkStyles : expandStyles,
    };
}
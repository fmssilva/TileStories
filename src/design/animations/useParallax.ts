/**
 * useParallax Hook
 * ================
 * Custom hook for subtle parallax scrolling effect
 * 
 * Features:
 * - Smooth parallax effect using transform instead of position (better performance)
 * - Configurable speed/intensity
 * - Respects reduced motion preference
 * - Uses RAF (RequestAnimationFrame) for 60fps performance
 * - Auto-cleanup on unmount
 * 
 * Usage:
 * ```tsx
 * const parallaxStyle = useParallax({ speed: 0.5 });
 * return <div style={parallaxStyle}>Background</div>
 * ```
 */

import { useState, useEffect, CSSProperties } from 'react';

export interface UseParallaxOptions {
    /**
     * Parallax speed multiplier
     * 0 = no parallax, 0.5 = half speed, 1 = same speed as scroll
     * @default 0.5
     */
    speed?: number;

    /**
     * Enable parallax effect
     * @default true
     */
    enabled?: boolean;
}

/**
 * Custom hook for parallax scrolling effect
 * Returns inline style object to apply to the element
 */
export function useParallax(options: UseParallaxOptions = {}): CSSProperties {
    const { speed = 0.5, enabled = true } = options;

    const [offsetY, setOffsetY] = useState(0);

    useEffect(() => {
        if (!enabled) return;

        // Check for reduced motion preference
        const prefersReducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
        if (prefersReducedMotion) return;

        let rafId: number;
        let lastScrollY = window.scrollY;

        const handleScroll = () => {
            const currentScrollY = window.scrollY;

            // Only update if scroll position changed
            if (currentScrollY !== lastScrollY) {
                lastScrollY = currentScrollY;

                rafId = requestAnimationFrame(() => {
                    setOffsetY(currentScrollY * speed);
                });
            }
        };

        // Initial call
        handleScroll();

        // Add scroll listener with passive flag for better performance
        window.addEventListener('scroll', handleScroll, { passive: true });

        return () => {
            window.removeEventListener('scroll', handleScroll);
            if (rafId) {
                cancelAnimationFrame(rafId);
            }
        };
    }, [speed, enabled]);

    return {
        transform: `translateY(${offsetY}px)`,
        willChange: 'transform', // Hint to browser for optimization
    };
}

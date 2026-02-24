/**
 * useScrollAnimation Hook
 * ======================
 * Custom hook for scroll-triggered animations using Intersection Observer API
 * 
 * Features:
 * - Fade-in animation when element enters viewport
 * - Slide-up animation for smooth appearance
 * - Configurable thresholds and delays
 * - Optimized with Intersection Observer (better than scroll listeners)
 * - Optional one-time trigger or continuous animations
 * - Supports reduced motion preference for accessibility
 * 
 * Usage:
 * ```tsx
 * const ref = useScrollAnimation<HTMLDivElement>({ 
 *   threshold: 0.2, 
 *   delay: 100 
 * });
 * 
 * return <div ref={ref} className="opacity-0 translate-y-4">Content</div>
 * ```
 */

import { useEffect, useRef, useState } from 'react';

export interface UseScrollAnimationOptions {
    /**
     * Percentage of element visible before triggering (0-1)
     * @default 0.1 (10% visible)
     */
    threshold?: number;

    /**
     * Delay before animation starts (ms)
     * @default 0
     */
    delay?: number;

    /**
     * Trigger animation only once (true) or every time element enters viewport (false)
     * @default true
     */
    once?: boolean;

    /**
     * Root margin for intersection observer (e.g., "0px 0px -100px 0px")
     * Useful for triggering animation before element enters viewport
     * @default "0px"
     */
    rootMargin?: string;
}

/**
 * Custom hook for scroll-triggered animations
 * Returns a ref to attach to the element you want to animate
 */
export function useScrollAnimation<T extends HTMLElement = HTMLDivElement>(
    options: UseScrollAnimationOptions = {}
) {
    const {
        threshold = 0.1,
        delay = 0,
        once = true,
        rootMargin = '0px'
    } = options;

    const elementRef = useRef<T>(null);
    const [isVisible, setIsVisible] = useState(false);
    const [hasAnimated, setHasAnimated] = useState(false);

    useEffect(() => {
        const element = elementRef.current;
        if (!element) return;

        // Check for reduced motion preference (accessibility)
        const prefersReducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;

        if (prefersReducedMotion) {
            // Skip animation if user prefers reduced motion
            setIsVisible(true);
            setHasAnimated(true);
            return;
        }

        const observer = new IntersectionObserver(
            (entries) => {
                entries.forEach((entry) => {
                    if (entry.isIntersecting) {
                        // Element entered viewport
                        if (delay > 0) {
                            setTimeout(() => {
                                setIsVisible(true);
                                if (once) setHasAnimated(true);
                            }, delay);
                        } else {
                            setIsVisible(true);
                            if (once) setHasAnimated(true);
                        }
                    } else if (!once && hasAnimated) {
                        // Element left viewport and we allow re-animation
                        setIsVisible(false);
                    }
                });
            },
            {
                threshold,
                rootMargin
            }
        );

        observer.observe(element);

        return () => {
            observer.disconnect();
        };
    }, [threshold, delay, once, hasAnimated, rootMargin]);

    return { ref: elementRef, isVisible };
}

/**
 * Helper function to get animation classes based on visibility
 * Use this with the hook to apply consistent animations
 */
export function getScrollAnimationClasses(
    isVisible: boolean,
    type: 'fade' | 'slide-up' | 'slide-left' | 'slide-right' | 'scale' = 'slide-up'
): string {
    const baseClasses = 'transition-all duration-700 ease-out';

    const animations = {
        'fade': isVisible
            ? 'opacity-100'
            : 'opacity-0',
        'slide-up': isVisible
            ? 'opacity-100 translate-y-0'
            : 'opacity-0 translate-y-8',
        'slide-left': isVisible
            ? 'opacity-100 translate-x-0'
            : 'opacity-0 translate-x-8',
        'slide-right': isVisible
            ? 'opacity-100 translate-x-0'
            : 'opacity-0 -translate-x-8',
        'scale': isVisible
            ? 'opacity-100 scale-100'
            : 'opacity-0 scale-95',
    };

    return `${baseClasses} ${animations[type]}`;
}

/**
 * Batch animation hook for animating multiple children sequentially
 * Useful for lists or grids where items appear one after another
 */
export function useStaggeredAnimation(
    baseDelay: number = 100,
    options: UseScrollAnimationOptions = {}
) {
    const containerRef = useRef<HTMLDivElement>(null);
    const [isVisible, setIsVisible] = useState(false);

    useEffect(() => {
        const container = containerRef.current;
        if (!container) return;

        const prefersReducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;

        if (prefersReducedMotion) {
            setIsVisible(true);
            return;
        }

        const observer = new IntersectionObserver(
            (entries) => {
                entries.forEach((entry) => {
                    if (entry.isIntersecting) {
                        setIsVisible(true);
                    }
                });
            },
            {
                threshold: options.threshold || 0.1,
                rootMargin: options.rootMargin || '0px'
            }
        );

        observer.observe(container);

        return () => {
            observer.disconnect();
        };
    }, [options.threshold, options.rootMargin]);

    const getItemClasses = (index: number) => {
        const delay = isVisible ? `${index * baseDelay}ms` : '0ms';
        return {
            className: `transition-all duration-700 ease-out ${isVisible ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-8'
                }`,
            style: { transitionDelay: delay }
        };
    };

    return { containerRef, isVisible, getItemClasses };
}

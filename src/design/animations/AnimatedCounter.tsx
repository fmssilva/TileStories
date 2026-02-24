/**
 * ANIMATED COUNTER COMPONENT
 * ==========================
 * 
 * Reusable animated number counter with scroll-triggered animation.
 * Smoothly animates from 0 to target value when element enters viewport.
 * 
 * Features:
 * - Scroll-triggered animation (once)
 * - Customizable duration and easing
 * - Number formatting with separators
 * - Prefix/suffix support (e.g., €, K, M)
 * - Uses <span> to avoid HTML hydration errors
 * 
 * Usage:
 * ```tsx
 * <AnimatedCounter target={3500} prefix="€" separator="," />
 * <AnimatedCounter target={12} suffix=" months" />
 * ```
 */

import { motion, useMotionValue, useTransform, animate } from 'framer-motion';
import { useEffect, useRef } from 'react';
import { useScrollAnimation } from './useScrollAnimation';

// ============================================================================
// TYPES
// ============================================================================

export interface AnimatedCounterProps {
    /** Target number to count to */
    target: number;
    /** Animation duration in seconds (default: 2) */
    duration?: number;
    /** Text to show before number (e.g., "€", "$") */
    prefix?: string;
    /** Text to show after number (e.g., "K", "M", " months") */
    suffix?: string;
    /** Thousands separator (default: ",") */
    separator?: string;
    /** Scroll visibility threshold (0-1, default: 0.5) */
    threshold?: number;
}

// ============================================================================
// COMPONENT
// ============================================================================

/**
 * AnimatedCounter Component
 * 
 * Animates a number from 0 to target when scrolled into view.
 * 
 * @example
 * // Basic usage
 * <AnimatedCounter target={100} />
 * 
 * @example
 * // With currency
 * <AnimatedCounter target={3500} prefix="€" separator="," />
 * 
 * @example
 * // With suffix
 * <AnimatedCounter target={12} suffix=" months" />
 */
export function AnimatedCounter({
    target,
    duration = 2,
    prefix = '',
    suffix = '',
    separator = ',',
    threshold = 0.5,
}: AnimatedCounterProps) {
    // Motion value for smooth animation
    const count = useMotionValue(0);

    // Transform count to formatted string
    const rounded = useTransform(count, (latest) => {
        const value = Math.round(latest);

        // Format number with separator (e.g., 3,500)
        const formatted = separator
            ? value.toString().replace(/\B(?=(\d{3})+(?!\d))/g, separator)
            : value.toString();

        return `${prefix}${formatted}${suffix}`;
    });

    // Track if animation has already run
    const hasAnimated = useRef(false);

    // Scroll-triggered visibility detection
    const { ref: scrollRef, isVisible } = useScrollAnimation<HTMLSpanElement>({
        threshold,
        once: true, // Only animate once
    });

    // Trigger animation when visible
    useEffect(() => {
        if (isVisible && !hasAnimated.current) {
            hasAnimated.current = true;

            const controls = animate(count, target, {
                duration,
                ease: 'easeOut',
            });

            // Cleanup on unmount
            return () => controls.stop();
        }
        return undefined;
    }, [isVisible, count, target, duration]);

    // IMPORTANT: Use <span> not <div> to avoid HTML hydration errors
    // when used inside <p> tags or other inline contexts
    return (
        <span ref={scrollRef} style={{ display: 'inline' }}>
            <motion.span>{rounded}</motion.span>
        </span>
    );
}

export default AnimatedCounter;

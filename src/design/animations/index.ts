/**
 * DESIGN ANIMATIONS - EXPORTS
 * ============================
 * 
 * Reusable animation components for the design system.
 * Import these for consistent, scroll-triggered animations.
 * 
 * Usage:
 * ```tsx
 * import { AnimatedCounter } from '@/design/animations';
 * ```
 */

export { AnimatedCounter } from './AnimatedCounter';
export type { AnimatedCounterProps } from './AnimatedCounter';

export { useScrollAnimation, getScrollAnimationClasses, useStaggeredAnimation } from './useScrollAnimation';
export type { UseScrollAnimationOptions } from './useScrollAnimation';

export { useParallax } from './useParallax';
export type { UseParallaxOptions } from './useParallax';

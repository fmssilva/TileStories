/**
 * Design System - Simplified exports for the three-tier system
 * =============================================================
 * 
 * TIER 1: Global foundations (this export)
 * TIER 2: Domain-specific (import from domain folders)  
 * TIER 3: Local component sizing (within components)
 */

// Global foundations - Tier 1
export { spacing, componentSizes, layout } from './size/sizes';
export {
    getSpacing,
    getIconSize,
    getInteractiveHeight,
    type SpacingKey,
    type IconSizeKey,
    type InteractiveSizeKey,
} from './size/sizeHelpers';

// Layout and component sizes
export {
    LAYOUT,
    COMPONENT_SIZES,
    isMobileViewport,
    isTabletViewport,
    isDesktopViewport,
    getHeaderHeight,
    getSectionPaddingY,
    type LayoutKey,
    type ComponentSizeKey,
} from './size/layout';

// Z-Index constants
export {
    Z_INDEX,
    getZIndex,
    getRelativeZIndex,
    type ZIndexKey,
    type ZIndexValue,
} from './zIndex';

// Global colors - Tier 1 
export * from './colors';

// Theme utilities - Tier 1
export { themeClasses, createThemeClasses, useThemeClasses, componentThemeClasses } from './themeClasses';

// Animation components
export { AnimatedCounter } from './animations';
export type { AnimatedCounterProps } from './animations';
export { useScrollAnimation, getScrollAnimationClasses, useStaggeredAnimation } from './animations/useScrollAnimation';
export type { UseScrollAnimationOptions } from './animations/useScrollAnimation';
export { useParallax } from './animations/useParallax';
export type { UseParallaxOptions } from './animations/useParallax';


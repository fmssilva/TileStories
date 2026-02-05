/**
 * Design System - Simplified exports for the three-tier system
 * =============================================================
 * 
 * TIER 1: Global foundations (this export)
 * TIER 2: Domain-specific (import from domain folders)  
 * TIER 3: Local component sizing (within components)
 */

// Global foundations - Tier 1
export { spacing, componentSizes, layout } from './sizes';
export {
    getSpacing,
    getIconSize,
    getInteractiveHeight,
    type SpacingKey,
    type IconSizeKey,
    type InteractiveSizeKey,
} from './sizeHelpers';

// Global colors - Tier 1 
export * from './colors';

// Theme utilities - Tier 1
export { themeClasses, createThemeClasses, useThemeClasses, componentThemeClasses } from './themeClasses';
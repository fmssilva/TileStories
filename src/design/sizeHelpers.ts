/**
 * SIZE HELPERS - Simple utilities for the size system
 * =====================================================
 * 
 * PHILOSOPHY: Keep it simple! Favor Tailwind classes over custom helpers.
 * Only provide utilities when you need dynamic values or complex logic.
 * 
 * Prefer: className="p-4 m-2 w-6 h-6"
 * Over:   className={getSpacing(4)} 
 * 
 * Use helpers only when:
 * ✅ Dynamic sizing based on props/state
 * ✅ Complex calculations 
 * ✅ Type safety for component APIs
 */

import { spacing, componentSizes } from './sizes';

// ============================================================================
// 1. TYPE SAFETY
// ============================================================================

export type SpacingKey = keyof typeof spacing;
export type IconSizeKey = keyof typeof componentSizes.icon;
export type InteractiveSizeKey = keyof typeof componentSizes.interactive;

// ============================================================================
// 2. ESSENTIAL HELPERS (Dynamic sizing only)
// ============================================================================

/**
 * Get spacing value for dynamic use cases
 * Prefer Tailwind classes: p-4, m-2, gap-6, etc.
 * Only use this when you need dynamic values
 */
export function getSpacing(key: SpacingKey): string {
    return spacing[key];
}

/**
 * Get icon size classes for dynamic components
 * Prefer Tailwind classes: w-4 h-4, w-6 h-6, etc.
 * Use this for props-based sizing: <Icon size="lg" />
 */
export function getIconSize(size: IconSizeKey): string {
    const value = componentSizes.icon[size];
    return `w-[${value}] h-[${value}]`;
}

/**
 * Get interactive element height for buttons, inputs
 * Use this for consistent interactive element sizing
 */
export function getInteractiveHeight(size: InteractiveSizeKey): string {
    const value = componentSizes.interactive[size];
    return `h-[${value}]`;
}

// ============================================================================
// 3. LAYOUT PRESETS (Common patterns only)
// ============================================================================

export const layoutPresets = {
    // Standard page container - most common pattern
    page: 'container mx-auto px-4',

    // Standard card pattern  
    card: 'p-6 rounded-lg',

    // Flex layouts with consistent gaps
    flexRow: 'flex items-center gap-4',
    flexCol: 'flex flex-col gap-4',
} as const;
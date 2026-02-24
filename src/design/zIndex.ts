/**
 * Z-INDEX SCALE FOR TILESTORIES
 * ==============================
 * 
 * Single source of truth for all z-index values across the application.
 * 
 * USAGE:
 * ```tsx
 * import { Z_INDEX } from '@/design/constants';
 * 
 * <header className={`fixed top-0 z-[${Z_INDEX.HEADER}]`} />
 * ```
 * 
 * SCALE PHILOSOPHY:
 * - Base layers (0-99): Normal content flow
 * - Content layers (100-999): Sticky UI elements
 * - Floating layers (1000-1999): Tooltips, buttons
 * - Map layers (2000-2999): Map components and controls
 * - Overlay layers (3000-3999): Modals, dialogs
 * - System layers (4000+): Critical UI (toasts, loading)
 * 
 * IMPORTANT:
 * - Never use arbitrary z-index values in components
 * - Always import and use these constants
 * - If you need a new z-index, add it here first
 * - Keep the scale organized and documented
 */

export const Z_INDEX = {
    // Explicitly behind other elements
    BELOW: -1,

    // Base OUTER PAGES, CONTAINERS... 
    BASE: 0,

    // MAP (leaflet...) maps like leaflet are normally high, so lets plan for that from the begining 
    /** 
     * WE DON'T DEFIN THIS BUT WE KNOW THAT:
     * Google Maps default z-index: ~200-500
     * Leaflet default z-index: ~400-600
     * We use 2000+ to avoid conflicts
     */

    // SO WE PUT CONTENT BUTTONS AND ALL COMPONENTS OVER MAPS
    // AND THEN IN EACH SECTION WE MANAGE LIKE CONTENT + 10, CONTENT + 20... to create relative layers
    CONTENT: 1000,

    // STICKY elements like headers, tabs, sections... 
    STICKY: 2000,

    // HEADER
    HEADER: 3000,

    // FLOATINGS - sticky ssections, dropdowns, mobile menu, notifications...
    FLOATING: 4000,

    // MODALS 
    // AND IN EACH MODAL WE CAN MANAGE LIKE MODAL + 10, MODAL + 20... to create relative layers
    MODAL: 5000,

    // SYSTEM LAYERS (4000+) - Critical UI
    SYSTEM: 6000,
} as const;

// ============================================================================
// TYPE DEFINITIONS
// ============================================================================

/** Type for all z-index keys */
export type ZIndexKey = keyof typeof Z_INDEX;

/** Type for z-index values */
export type ZIndexValue = typeof Z_INDEX[ZIndexKey];

// ============================================================================
// HELPER FUNCTIONS
// ============================================================================

/**
 * Get z-index value by key with type safety
 * 
 * @param key - Z-index key
 * @returns The z-index value
 * 
 * @example
 * const headerZ = getZIndex('HEADER'); // 300
 */
export function getZIndex(key: ZIndexKey): number {
    return Z_INDEX[key];
}

/**
 * Create a relative z-index based on a base layer
 * Useful for creating layers that should always be above/below another
 * 
 * @param baseKey - Base z-index key
 * @param offset - Offset to add (positive or negative)
 * @returns Calculated z-index value
 * 
 * @example
 * // Create a layer just below the modal
 * const behindModal = getRelativeZIndex('MODAL', -10); // 3090
 */
export function getRelativeZIndex(baseKey: ZIndexKey, offset: number): number {
    return Z_INDEX[baseKey] + offset;
}

/**
 * STICKY LAYERS CONFIGURATION
 * ============================
 * 
 * Single source of truth for all sticky elements in the application.
 * Supports hierarchical sticky elements with conditional activation.
 * 
 * CONCEPTS:
 * - **Flat Array Structure**: Easy to read and maintain
 * - **Parent References**: Each layer can reference a parent layer
 * - **Conditional Activation**: Layers can be conditionally active based on app state
 * - **Automatic Offset Calculation**: Only counts active layers in current hierarchy path
 * 
 * HIERARCHY EXAMPLE:
 * ```
 * work-plan-phase-tabs (always active when mounted)
 * ├── phase-1-detail-tabs (active only when phase 1 selected)
 * └── phase-2-detail-tabs (active only when phase 2 selected)
 * ```
 * 
 * STOP AT ELEMENT:
 * - Use CSS selector to make a layer stop being sticky
 * - When that element comes into view, the layer becomes relative
 * - Example: stopAtElement: '#section-b' means stop when #section-b appears
 * 
 * Z-INDEX STRATEGY:
 * - BackToTop: Z_INDEX.FLOATING (highest floating element)
 * - Root sticky layers: Z_INDEX.FLOATING - 10
 * - Child layers: Parent z-index - 10
 * - This prevents overlap and keeps clear visual hierarchy
 */

import { Z_INDEX } from '@/design';

// ═══════════════════════════════════════════════════════════════════════════
// TYPES
// ═══════════════════════════════════════════════════════════════════════════

/**
 * Context object passed to activeWhen function
 * Extend this interface to add app-specific state
 */
export interface StickyActivationContext {
    /** Current scroll position */
    scrollY: number;

    /** Add app-specific state here as needed */
    [key: string]: unknown;
}

export interface StickyLayerConfig {
    /** Unique identifier for this sticky layer */
    id: string;

    /** Human-readable label (for debugging) */
    label: string;

    /** Height of this sticky bar in pixels */
    height: number;

    /** Z-index value (higher layers should have higher values) */
    zIndex: number;

    /** 
     * Parent layer ID - this layer will only be considered if parent is active
     * Set to null for root-level layers
     */
    parent: string | null;

    /**
     * Optional condition for when this layer should be active
     * Receives context object with app state
     * Return true to activate, false to deactivate
     * If not provided, layer is always active (when parent is active)
     */
    activeWhen?: (context: StickyActivationContext) => boolean;

    /** CSS selector - stop being sticky when this element enters viewport */
    stopAtElement?: string;

    /** Tailwind background color classes (must be opaque!) */
    backgroundColor?: string;

    /** Show bottom border/shadow for visual separation */
    showSeparator?: boolean;
}

// ═══════════════════════════════════════════════════════════════════════════
// CONFIGURATION
// ═══════════════════════════════════════════════════════════════════════════

/**
 * STICKY LAYERS HIERARCHY
 * 
 * Define all sticky layers here with parent references.
 * The system will automatically calculate offsets based on active hierarchy path.
 * 
 * ADDING A NEW LAYER:
 * 1. Add entry to STICKY_LAYERS array
 * 2. Set parent: null for root level, or parent layer ID for child
 * 3. Set zIndex (root: FLOATING - 10, child: parent zIndex - 10)
 * 4. Optional: Add activeWhen condition for dynamic activation
 * 5. Optional: Set stopAtElement CSS selector
 * 
 * EXAMPLE HIERARCHIES:
 * 
 * Work Plan Page:
 * - work-plan-phase-tabs (parent: null, always active)
 *   - phase-1-detail-tabs (parent: 'work-plan-phase-tabs', active when phase 1 selected)
 *   - phase-2-detail-tabs (parent: 'work-plan-phase-tabs', active when phase 2 selected)
 * 
 * Home Page with Sections:
 * - home-epochs-tabs (parent: null, stops at #features-section)
 *   - epoch-1-detail (parent: 'home-epochs-tabs', active when epoch 1 viewed)
 */
export const STICKY_LAYERS: StickyLayerConfig[] = [
    // ─────────────────────────────────────────────────────────────────────────
    // GLOBAL STICKY ELEMENTS
    // ─────────────────────────────────────────────────────────────────────────

    // BackToTop button - highest z-index, always floating
    {
        id: 'back-to-top',
        label: 'Back to Top Button',
        height: 0, // No height, doesn't affect layout
        zIndex: Z_INDEX.STICKY, // Highest floating element
        parent: null,
        backgroundColor: 'transparent',
        showSeparator: false,
        // Always active (no activeWhen condition)
    },

    // Simple header example - demonstrates simplified API
    {
        id: 'simple-header',
        label: 'Simple Sticky Header Example',
        height: 80,
        zIndex: Z_INDEX.STICKY - 10,
        parent: null,
        backgroundColor: 'bg-blue-500',
        showSeparator: true,
    },

    // ─────────────────────────────────────────────────────────────────────────
    // WORK PLAN PAGE - ROOT LEVEL
    // ─────────────────────────────────────────────────────────────────────────

    {
        id: 'work-plan-phase-tabs',
        label: 'Work Plan Phase Navigation',
        height: 80,
        zIndex: Z_INDEX.STICKY - 10, // Below BackToTop
        parent: null, // Root level
        stopAtElement: '#work-plan-footer',
        backgroundColor: 'bg-white/95 dark:bg-gray-900/95',
        showSeparator: true,
        // Always active when component is mounted (no activeWhen condition)
    },

    // ─────────────────────────────────────────────────────────────────────────
    // DEMO STICKY SECTIONS (TEMPORARY - for testing)
    // ─────────────────────────────────────────────────────────────────────────

    // Test Case 1: Expand/Collapse
    {
        id: 'demo-sticky-1',
        label: 'Demo Test Case 1: Expand/Collapse',
        height: 60, // Approximate, will be dynamically measured
        zIndex: Z_INDEX.STICKY - 10,
        parent: null,
        backgroundColor: 'bg-blue-600',
        showSeparator: true,
    },

    // Test Case 2: Hierarchical Document Structure (3 levels)
    {
        id: 'demo-sticky-2-chapter',
        label: 'Demo Test Case 2: Chapter Level',
        height: 60, // Approximate, will be dynamically measured
        zIndex: Z_INDEX.STICKY - 20,
        parent: 'demo-sticky-1', // Child of Test Case 1
        backgroundColor: 'bg-purple-700',
        showSeparator: true,
    },

    {
        id: 'demo-sticky-2-section',
        label: 'Demo Test Case 2: Section Level',
        height: 70, // Approximate, will be dynamically measured
        zIndex: Z_INDEX.STICKY - 30,
        parent: 'demo-sticky-2-chapter', // Child of chapter
        backgroundColor: 'bg-green-600',
        showSeparator: true,
    },

    {
        id: 'demo-sticky-2-subsection',
        label: 'Demo Test Case 2: Subsection Level',
        height: 50, // Approximate, will be dynamically measured
        zIndex: Z_INDEX.STICKY - 40,
        parent: 'demo-sticky-2-section', // Child of section (grandchild of chapter)
        backgroundColor: 'bg-orange-600',
        showSeparator: true,
    },

    // Test Case 3: Variable Height Buttons
    {
        id: 'demo-sticky-3',
        label: 'Demo Test Case 3: Variable Height Buttons',
        height: 70, // Approximate, will be dynamically measured
        zIndex: Z_INDEX.STICKY - 50,
        parent: 'demo-sticky-2-subsection', // Child of subsection
        backgroundColor: 'bg-red-600',
        showSeparator: true,
    },

    // Test Case 4: Accordion/Nested Content
    {
        id: 'demo-sticky-4',
        label: 'Demo Test Case 4: Accordion',
        height: 60, // Approximate, will be dynamically measured
        zIndex: Z_INDEX.STICKY - 60,
        parent: 'demo-sticky-3', // Child of Test Case 3
        backgroundColor: 'bg-teal-600',
        showSeparator: true,
    },


];

// ═══════════════════════════════════════════════════════════════════════════
// HELPER FUNCTIONS
// ═══════════════════════════════════════════════════════════════════════════

/**
 * Get the hierarchy path for a layer (all ancestors from root to this layer)
 * 
 * @param layerId - ID of the layer
 * @returns Array of layer IDs from root to target, or empty array if not found
 * 
 * @example
 * // If: phase-1-detail-tabs has parent work-plan-phase-tabs (which has parent null)
 * getLayerPath('phase-1-detail-tabs')
 * // Returns: ['work-plan-phase-tabs', 'phase-1-detail-tabs']
 */
export function getLayerPath(layerId: string): string[] {
    const path: string[] = [];
    let currentId: string | null = layerId;

    // Traverse up the parent chain
    while (currentId) {
        const layer = STICKY_LAYERS.find(l => l.id === currentId);
        if (!layer) break;

        path.unshift(currentId); // Add to beginning
        currentId = layer.parent;
    }

    return path;
}

/**
 * Calculate the top offset for a sticky layer
 * This is the sum of heights of all ACTIVE layers above it in the hierarchy
 * 
 * @param layerId - ID of the layer to calculate offset for
 * @param activeLayers - Map of layer IDs to their active state
 * @param measuredHeights - Optional map of measured heights (overrides config)
 * @returns Offset in pixels from top of viewport
 * 
 * @example
 * // Active layers: work-plan-phase-tabs (80px), phase-1-detail-tabs (60px)
 * getStickyOffset('phase-1-detail-tabs', activeLayersMap)
 * // Returns: 80 (only parent tab height)
 */
export function getStickyOffset(
    layerId: string,
    activeLayers: Map<string, boolean> = new Map(),
    measuredHeights?: Map<string, number>
): number {
    const layerPath = getLayerPath(layerId);

    if (layerPath.length === 0) {
        console.warn(`[getStickyOffset] Sticky layer "${layerId}" not found in STICKY_LAYERS config`);
        return 0;
    }

    // Sum heights of all active ancestors (not including the layer itself)
    const ancestorIds = layerPath.slice(0, -1); // All except the target layer

    return ancestorIds.reduce((sum, ancestorId, index) => {
        const layer = STICKY_LAYERS.find(l => l.id === ancestorId);
        const isActive = activeLayers.get(ancestorId) ?? true;

        // Only add height if layer is active
        if (layer && isActive) {
            // Use measured height if available, otherwise use config height
            const height = measuredHeights?.get(ancestorId) ?? layer.height;
            // Add spacing between sticky elements (imported from LAYOUT.SPACE_BETWEEN_STICKY_ELEMENTS = 8px)
            const spacing = index < ancestorIds.length - 1 || ancestorIds.length > 0 ? 8 : 0;
            return sum + height + spacing;
        }
        return sum;
    }, 0);
}

/**
 * Get configuration for a specific sticky layer
 * 
 * @param layerId - ID of the layer
 * @returns Layer configuration or undefined if not found
 */
export function getStickyLayer(layerId: string): StickyLayerConfig | undefined {
    return STICKY_LAYERS.find(l => l.id === layerId);
}

/**
 * Get total height of all sticky layers (maximum possible)
 * Useful for calculating content padding or offsets
 * Note: This returns max possible height, not actual height (which depends on active layers)
 * 
 * @returns Total height in pixels
 */
export function getTotalStickyHeight(): number {
    return STICKY_LAYERS.reduce((sum, layer) => sum + layer.height, 0);
}

/**
 * Get all sticky layer IDs
 * Useful for validation and debugging
 * 
 * @returns Array of layer IDs
 */
export function getStickyLayerIds(): string[] {
    return STICKY_LAYERS.map(l => l.id);
}

/**
 * Get all child layers of a parent
 * 
 * @param parentId - Parent layer ID, or null for root-level layers
 * @returns Array of child layer configurations
 */
export function getChildLayers(parentId: string | null): StickyLayerConfig[] {
    return STICKY_LAYERS.filter(l => l.parent === parentId);
}

/**
 * Check if a layer should be considered based on its activeWhen condition
 * 
 * @param layerId - Layer ID to check
 * @param context - Current activation context with app state
 * @returns True if layer should be active, false otherwise
 */
export function shouldLayerBeActive(layerId: string, context: StickyActivationContext): boolean {
    const layer = getStickyLayer(layerId);
    if (!layer) return false;

    // If no activeWhen condition, layer is always active (when mounted)
    if (!layer.activeWhen) return true;

    // Otherwise check the condition
    return layer.activeWhen(context);
}

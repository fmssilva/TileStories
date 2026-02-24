/**
 * USE STICKY HOOK
 * ===============
 * 
 * Easy-to-use hook for making components sticky with automatic positioning.
 * Now includes optional scroll-shrink integration for flicker-free size changes.
 * 
 * USAGE:
 * ```tsx
 * // Basic sticky
 * const { ref, stickyClasses, stickyStyles } = useSticky('my-layer-id');
 * 
 * // With auto-shrink
 * const { ref, isShrunk, stickyClasses, stickyStyles } = useSticky('my-layer-id', {
 *   enableShrink: true,  // or { shrinkThreshold: 100, expandThreshold: 50 }
 *   autoMeasure: true,   // Auto-measure height on mount/resize
 * });
 * 
 * return (
 *   <div ref={ref} className={stickyClasses} style={stickyStyles}>
 *     <div className={isShrunk ? 'compact' : 'expanded'}>
 *       Content
 *     </div>
 *   </div>
 * );
 * ```
 * 
 * FEATURES:
 * - Automatic position registration
 * - Optional auto-height measurement with ResizeObserver
 * - Integrated scroll-shrink with hysteresis (no trembling)
 * - Returns ref for easy attachment
 * - Fully typed with TypeScript
 */

import { useEffect, useCallback, useRef } from 'react';
import { useStickyContext } from '../contexts/StickyContext';
import { getStickyLayer } from '../config/stickyConfig';
import { useScrollShrink, getShrinkClasses, type UseScrollShrinkOptions } from './useScrollShrink';

// ═══════════════════════════════════════════════════════════════════════════
// TYPES
// ═══════════════════════════════════════════════════════════════════════════

export interface UseStickyOptions {
    /** 
     * Enable scroll-based shrinking with hysteresis 
     * - true: Use default thresholds (shrink: 50px, expand: 30px)
     * - object: Custom thresholds { shrinkThreshold: 100, expandThreshold: 50 }
     * - false: Disable shrinking (default)
     */
    enableShrink?: boolean | Pick<UseScrollShrinkOptions, 'shrinkThreshold' | 'expandThreshold'>;

    /**
     * Automatically measure and report height on mount and resize
     * Uses ResizeObserver for efficient updates
     * Default: true
     */
    autoMeasure?: boolean;

    /**
     * Automatically register element position on mount
     * Default: true
     */
    autoRegisterPosition?: boolean;

    /**
     * CSS classes to apply when shrunk (if enableShrink is true)
     * Default: '' (you handle classes yourself using isShrunk)
     */
    shrinkClass?: string;

    /**
     * CSS classes to apply when expanded (if enableShrink is true)
     * Default: '' (you handle classes yourself using isShrunk)
     */
    expandClass?: string;
}

export interface UseStickyReturn {
    /** Ref to attach to your sticky element (required for auto-measurement/position) */
    ref: React.RefObject<HTMLDivElement | null>;

    /** Top offset in pixels where this layer should stick */
    offset: number;

    /** Whether this layer should be sticky (false when stopAtElement is reached) */
    isSticky: boolean;

    /** Whether the element is in shrunk state (based on scroll, not sticky state) */
    isShrunk: boolean;

    /** Full layer configuration from stickyConfig.ts */
    config: ReturnType<typeof getStickyLayer>;

    /** Tailwind classes to apply to the sticky element */
    stickyClasses: string;

    /** Inline styles to apply to the sticky element */
    stickyStyles: React.CSSProperties | undefined;

    /** CSS classes for shrink transitions (combined shrink/expand + transition) */
    shrinkClasses: string;

    /** Report measured height to sticky system (call when height changes manually) */
    reportHeight: (height: number) => void;

    /** Register element's position on page (call once when element is mounted manually) */
    registerPosition: (offsetTop: number) => void;
}

// ═══════════════════════════════════════════════════════════════════════════
// HOOK
// ═══════════════════════════════════════════════════════════════════════════

/**
 * Hook to make a component sticky with automatic positioning
 * 
 * Automatically registers the layer when mounted and unregisters when unmounted.
 * Provides classes and styles for easy application to your component.
 * 
 * NEW: Integrated scroll-shrink support with auto-measurement!
 * 
 * @param layerId - ID of the sticky layer (must exist in STICKY_LAYERS config)
 * @param options - Configuration options for auto-features
 * @returns Sticky state, styling, and ref
 * 
 * @example
 * // Basic usage with auto-measurement
 * function MyHeader() {
 *   const { ref, stickyClasses, stickyStyles } = useSticky('header');
 *   
 *   return (
 *     <header ref={ref} className={stickyClasses} style={stickyStyles}>
 *       Header content
 *     </header>
 *   );
 * }
 * 
 * @example
 * // With scroll-shrink integration (no trembling!)
 * function MyTabs() {
 *   const { ref, isShrunk, stickyClasses, stickyStyles } = useSticky('section-tabs', {
 *     enableShrink: { shrinkThreshold: 100, expandThreshold: 50 },
 *     autoMeasure: true,
 *   });
 *   
 *   return (
 *     <div ref={ref} className={stickyClasses} style={stickyStyles}>
 *       <div className={isShrunk ? 'compact-mode' : 'expanded-mode'}>
 *         Tabs content
 *       </div>
 *     </div>
 *   );
 * }
 */
export function useSticky(
    layerId: string,
    options: UseStickyOptions = {}
): UseStickyReturn {
    const {
        enableShrink = false,
        autoMeasure = true,
        autoRegisterPosition = true,
        shrinkClass = '',
        expandClass = '',
    } = options;

    // Parse shrink options
    const shrinkOptions = typeof enableShrink === 'object' ? enableShrink : {};
    const shrinkEnabled = !!enableShrink;

    // Create ref for auto-measurement and position registration
    const ref = useRef<HTMLDivElement>(null);

    const {
        getOffset,
        isSticky: getIsSticky,
        registerLayer,
        unregisterLayer,
        updateLayerHeight,
        registerElementPosition
    } = useStickyContext();

    // Register this layer when component mounts
    useEffect(() => {
        registerLayer(layerId);
        return () => unregisterLayer(layerId);
    }, [layerId, registerLayer, unregisterLayer]);

    // Auto-register position on mount
    useEffect(() => {
        if (autoRegisterPosition && ref.current) {
            const rect = ref.current.getBoundingClientRect();
            const offsetTop = rect.top + window.scrollY;
            registerElementPosition(layerId, offsetTop);
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []); // Only once on mount

    // Auto-measure height with ResizeObserver
    useEffect(() => {
        if (!autoMeasure || !ref.current) return;

        const element = ref.current;

        // Initial measurement
        const initialHeight = element.offsetHeight;
        updateLayerHeight(layerId, initialHeight);

        // Watch for size changes
        const resizeObserver = new ResizeObserver((entries) => {
            for (const entry of entries) {
                const height = entry.target instanceof HTMLElement
                    ? entry.target.offsetHeight
                    : 0;
                if (height > 0) {
                    updateLayerHeight(layerId, height);
                }
            }
        });

        resizeObserver.observe(element);

        return () => {
            resizeObserver.disconnect();
        };
    }, [autoMeasure, layerId, updateLayerHeight]);

    // Integrated scroll-shrink (optional)
    const isShrunk = useScrollShrink(
        shrinkEnabled
            ? {
                shrinkThreshold: shrinkOptions.shrinkThreshold ?? 50,
                expandThreshold: shrinkOptions.expandThreshold ?? 30,
            }
            : { shrinkThreshold: 0, expandThreshold: 0 } // Disabled config
    );

    // Get configuration for this layer
    const layer = getStickyLayer(layerId);
    const offset = getOffset(layerId);
    const shouldBeSticky = getIsSticky(layerId);

    // Callback to report measured height manually
    const reportHeight = useCallback((height: number) => {
        updateLayerHeight(layerId, height);
    }, [layerId, updateLayerHeight]);

    // Callback to register element position manually
    const registerPosition = useCallback((offsetTop: number) => {
        registerElementPosition(layerId, offsetTop);
    }, [layerId, registerElementPosition]);

    // Warn if layer not found in config (development only)
    if (!layer && process.env.NODE_ENV === 'development') {
        console.warn(
            `[useSticky] Layer "${layerId}" not found in STICKY_LAYERS config. ` +
            `Add it to src/layout_and_navigation/sticky/config/stickyConfig.ts`
        );
    }

    // Generate Tailwind classes
    const stickyClasses = shouldBeSticky
        ? 'sticky'
        : 'relative';

    // Generate inline styles (z-index and top position)
    const stickyStyles = shouldBeSticky
        ? {
            top: `${offset}px`,
            zIndex: layer?.zIndex ?? 100
        }
        : undefined;

    // Generate shrink classes
    const shrinkClasses = shrinkEnabled
        ? getShrinkClasses(isShrunk, shrinkClass, expandClass)
        : '';

    return {
        ref,
        offset,
        isSticky: shouldBeSticky,
        isShrunk: shrinkEnabled ? isShrunk : false,
        config: layer,
        stickyClasses,
        stickyStyles,
        shrinkClasses,
        reportHeight,
        registerPosition,
    };
}

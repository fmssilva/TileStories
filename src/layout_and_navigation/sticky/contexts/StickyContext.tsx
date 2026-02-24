/**
 * STICKY CONTEXT
 * ==============
 * 
 * Manages the global state of all sticky layers in the application.
 * Supports hierarchical sticky elements with conditional activation.
 * 
 * RESPONSIBILITIES:
 * - Track scroll position and app state
 * - Determine which layers should be sticky based on hierarchy and conditions
 * - Handle "stopAtElement" logic
 * - Provide offsets based on active hierarchy path
 * - Support conditional layer activation (activeWhen)
 * 
 * USAGE:
 * 1. Wrap your app with <StickyProvider> in App.tsx or MainLayout
 * 2. Use useStickyContext() hook in components or useSticky() for easier access
 */

import { createContext, useContext, useState, useEffect, ReactNode, useCallback } from 'react';
import {
    STICKY_LAYERS,
    getStickyOffset,
    shouldLayerBeActive,
    getLayerPath,
    type StickyActivationContext
} from '../config/stickyConfig';

// ═══════════════════════════════════════════════════════════════════════════
// TYPES
// ═══════════════════════════════════════════════════════════════════════════

interface StickyState {
    /** Map of layer ID to whether it should be sticky */
    activeLayers: Map<string, boolean>;

    /** Map of layer ID to measured height (overrides config height) */
    measuredHeights: Map<string, number>;

    /** Map of layer ID to the element's initial offset from top of page */
    elementOffsets: Map<string, number>;

    /** Current scroll position in pixels */
    scrollY: number;

    /** App-specific context for conditional activation */
    activationContext: StickyActivationContext;
}

interface StickyContextValue extends StickyState {
    /** Get the top offset for a sticky layer (considers active hierarchy) */
    getOffset: (layerId: string) => number;

    /** Check if layer should be sticky */
    isSticky: (layerId: string) => boolean;

    /** Register a layer as mounted */
    registerLayer: (layerId: string) => void;

    /** Unregister a layer */
    unregisterLayer: (layerId: string) => void;

    /** Update app-specific context (for conditional activation) */
    updateActivationContext: (context: Partial<StickyActivationContext>) => void;

    /** Update measured height for a layer (dynamic measurement) */
    updateLayerHeight: (layerId: string, height: number) => void;

    /** Get height for a layer (measured height overrides config) */
    getLayerHeight: (layerId: string) => number;

    /** Register element's position on page (for scroll-based sticky activation) */
    registerElementPosition: (layerId: string, offsetTop: number) => void;
}

// ═══════════════════════════════════════════════════════════════════════════
// CONTEXT
// ═══════════════════════════════════════════════════════════════════════════

const StickyContext = createContext<StickyContextValue | null>(null);

// ═══════════════════════════════════════════════════════════════════════════
// PROVIDER COMPONENT
// ═══════════════════════════════════════════════════════════════════════════

export function StickyProvider({ children }: { children: ReactNode }) {
    const [state, setState] = useState<StickyState>({
        activeLayers: new Map(),
        measuredHeights: new Map(),
        elementOffsets: new Map(),
        scrollY: 0,
        activationContext: {
            scrollY: 0,
        },
    });

    // ─────────────────────────────────────────────────────────────────────────
    // TRACK SCROLL POSITION
    // ─────────────────────────────────────────────────────────────────────────

    useEffect(() => {
        const handleScroll = () => {
            const newScrollY = window.scrollY;
            setState(prev => ({
                ...prev,
                scrollY: newScrollY,
                activationContext: {
                    ...prev.activationContext,
                    scrollY: newScrollY
                }
            }));
        };

        // Use passive listener for better performance
        window.addEventListener('scroll', handleScroll, { passive: true });
        handleScroll(); // Initial check

        return () => window.removeEventListener('scroll', handleScroll);
    }, []);

    // ─────────────────────────────────────────────────────────────────────────
    // UPDATE ACTIVE LAYERS BASED ON SCROLL AND CONDITIONS
    // ─────────────────────────────────────────────────────────────────────────

    useEffect(() => {
        const newActiveLayers = new Map(state.activeLayers);
        let hasChanged = false;

        STICKY_LAYERS.forEach(layer => {
            // Check if layer passes its activeWhen condition (if it has one)
            const meetsCondition = shouldLayerBeActive(layer.id, state.activationContext);

            if (!meetsCondition) {
                // Layer doesn't meet its condition, mark as inactive
                const wasSticky = newActiveLayers.get(layer.id);
                if (wasSticky !== false) {
                    newActiveLayers.set(layer.id, false);
                    hasChanged = true;
                }
                return;
            }

            // Layer meets condition, now check parent hierarchy
            const layerPath = getLayerPath(layer.id);
            const allAncestorsActive = layerPath.slice(0, -1).every(ancestorId => {
                return newActiveLayers.get(ancestorId) ?? true;
            });

            if (!allAncestorsActive) {
                // Parent is not active, so this layer can't be active
                const wasSticky = newActiveLayers.get(layer.id);
                if (wasSticky !== false) {
                    newActiveLayers.set(layer.id, false);
                    hasChanged = true;
                }
                return;
            }

            // Check stopAtElement logic OR scroll-based activation
            if (!layer.stopAtElement) {
                // No stop element - use scroll-based activation
                // Element should become sticky when user scrolls past its original position
                const elementOffset = state.elementOffsets.get(layer.id);

                if (elementOffset === undefined) {
                    // Element position not yet registered, remain not sticky
                    const wasSticky = newActiveLayers.get(layer.id);
                    if (wasSticky !== false) {
                        newActiveLayers.set(layer.id, false);
                        hasChanged = true;
                    }
                    return;
                }

                // Calculate the cumulative offset of all sticky parents above this element
                const parentOffset = getStickyOffset(layer.id, newActiveLayers, state.measuredHeights);

                // Element becomes sticky when user has scrolled past its original position
                // accounting for the sticky headers above it
                const shouldBeSticky = state.scrollY + parentOffset >= elementOffset;

                const wasSticky = newActiveLayers.get(layer.id);
                if (wasSticky !== shouldBeSticky) {
                    newActiveLayers.set(layer.id, shouldBeSticky);
                    hasChanged = true;
                    console.log(`🔄 [StickyContext] Layer "${layer.id}" ${shouldBeSticky ? 'BECAME STICKY' : 'BECAME UNSTICKY'} at scrollY: ${state.scrollY}px`);
                }
                return;
            }

            // Check if stop element is in view
            const stopElement = document.querySelector(layer.stopAtElement);
            if (!stopElement) {
                // Element not found, remain sticky
                const wasSticky = newActiveLayers.get(layer.id);
                if (wasSticky !== true) {
                    newActiveLayers.set(layer.id, true);
                    hasChanged = true;
                }
                return;
            }

            // Calculate if we should stop being sticky
            const rect = stopElement.getBoundingClientRect();
            const layerHeight = state.measuredHeights.get(layer.id) ?? layer.height;
            const offset = getStickyOffset(layer.id, newActiveLayers, state.measuredHeights) + layerHeight;

            // Stop being sticky when stopElement touches this layer
            const shouldBeSticky = rect.top > offset;

            const wasSticky = newActiveLayers.get(layer.id);
            if (wasSticky !== shouldBeSticky) {
                newActiveLayers.set(layer.id, shouldBeSticky);
                hasChanged = true;
            }
        });

        // Only update state if something changed (prevents infinite loops)
        if (hasChanged) {
            setState(prev => ({ ...prev, activeLayers: newActiveLayers }));
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [state.scrollY, state.activationContext, state.activeLayers]);
    // NOTE: Removed state.measuredHeights and state.elementOffsets from dependencies to prevent infinite loops
    // Height changes should NOT trigger recalculation of sticky state
    // Sticky state depends only on scroll position and element positions (which are read, not depended on)
    // ─────────────────────────────────────────────────────────────────────────
    // CONTEXT API METHODS
    // ─────────────────────────────────────────────────────────────────────────

    const registerLayer = useCallback((layerId: string) => {
        setState(prev => {
            const newMap = new Map(prev.activeLayers);
            if (!newMap.has(layerId)) {
                // Initially set to FALSE - layer should NOT be sticky until user scrolls past it
                newMap.set(layerId, false);
                return { ...prev, activeLayers: newMap };
            }
            return prev;
        });
    }, []);

    const unregisterLayer = useCallback((layerId: string) => {
        setState(prev => {
            const newMap = new Map(prev.activeLayers);
            if (newMap.has(layerId)) {
                newMap.delete(layerId);
                return { ...prev, activeLayers: newMap };
            }
            return prev;
        });
    }, []);

    const getOffset = useCallback((layerId: string): number => {
        return getStickyOffset(layerId, state.activeLayers, state.measuredHeights);
    }, [state.activeLayers, state.measuredHeights]);

    const isSticky = useCallback((layerId: string): boolean => {
        // Default to FALSE (not sticky) if layer hasn't been explicitly set
        return state.activeLayers.get(layerId) ?? false;
    }, [state.activeLayers]);

    const updateActivationContext = useCallback((contextUpdate: Partial<StickyActivationContext>) => {
        setState(prev => ({
            ...prev,
            activationContext: {
                ...prev.activationContext,
                ...contextUpdate
            }
        }));
    }, []);

    const updateLayerHeight = useCallback((layerId: string, height: number) => {
        console.log(`📏 [StickyContext] Layer "${layerId}" height measured:`, height, 'px');
        setState(prev => {
            const newMap = new Map(prev.measuredHeights);
            const currentHeight = newMap.get(layerId);
            if (currentHeight !== height) {
                newMap.set(layerId, height);
                return { ...prev, measuredHeights: newMap };
            }
            return prev;
        });
    }, []);

    const getLayerHeight = useCallback((layerId: string): number => {
        // Measured height takes precedence over config height
        const measured = state.measuredHeights.get(layerId);
        if (measured !== undefined) {
            return measured;
        }

        // Fallback to config height
        const layer = STICKY_LAYERS.find(l => l.id === layerId);
        return layer?.height ?? 0;
    }, [state.measuredHeights]);

    const registerElementPosition = useCallback((layerId: string, offsetTop: number) => {
        setState(prev => {
            const newMap = new Map(prev.elementOffsets);
            const currentOffset = newMap.get(layerId);
            // Only update if changed or not yet registered
            if (currentOffset !== offsetTop) {
                newMap.set(layerId, offsetTop);
                console.log(`📍 [StickyContext] Layer "${layerId}" position registered:`, offsetTop, 'px from top');
                return { ...prev, elementOffsets: newMap };
            }
            return prev;
        });
    }, []);

    // ─────────────────────────────────────────────────────────────────────────
    // RENDER
    // ─────────────────────────────────────────────────────────────────────────

    return (
        <StickyContext.Provider
            value={{
                ...state,
                getOffset,
                isSticky,
                registerLayer,
                unregisterLayer,
                updateActivationContext,
                updateLayerHeight,
                getLayerHeight,
                registerElementPosition,
            }}
        >
            {children}
        </StickyContext.Provider>
    );
}

// ═══════════════════════════════════════════════════════════════════════════
// HOOK
// ═══════════════════════════════════════════════════════════════════════════

/**
 * Hook to access sticky context
 * Must be used within StickyProvider
 * 
 * @throws Error if used outside StickyProvider
 * 
 * @example
 * const { isSticky, getOffset } = useStickyContext();
 */
export function useStickyContext(): StickyContextValue {
    const context = useContext(StickyContext);
    if (!context) {
        throw new Error('useStickyContext must be used within StickyProvider');
    }
    return context;
}

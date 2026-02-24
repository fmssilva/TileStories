/**
 * STICKY MANAGER - PUBLIC API
 * ============================
 * 
 * Central export point for the sticky system.
 * 
 * 🎯 NEW SIMPLIFIED API - One Hook Does It All!
 * ================================================
 * 
 * The sticky system now features auto-measurement, auto-positioning,
 * and integrated scroll-shrink with hysteresis (no trembling!).
 * 
 * QUICK START:
 * ```tsx
 * import { useSticky } from '@/layout_and_navigation/sticky';
 * 
 * function MyHeader() {
 *   const { ref, isShrunk, stickyClasses, stickyStyles } = useSticky('header', {
 *     enableShrink: true,  // Auto scroll-shrink with hysteresis
 *   });
 *   
 *   return (
 *     <header ref={ref} className={stickyClasses} style={stickyStyles}>
 *       <div className={isShrunk ? 'compact' : 'expanded'}>Content</div>
 *     </header>
 *   );
 * }
 * ```
 * 
 * DOCUMENTATION:
 * - Quick Reference: PROJECT_GUIDES/STICKY_QUICK_REFERENCE.md
 * - Full Guide: PROJECT_GUIDES/GUIDE_STICKY_SIMPLIFIED_API.md
 * - Summary: PROJECT_GUIDES/STICKY_SIMPLIFIED_API_SUMMARY.md
 * 
 * LEGACY USAGE (Still Supported):
 * ```tsx
 * // Provider
 * import { StickyProvider } from '@/layout_and_navigation/sticky';
 * 
 * // Component wrapper
 * import { StickyContainer } from '@/layout_and_navigation/sticky';
 * 
 * // Config helpers
 * import { getStickyOffset, getStickyLayer } from '@/layout_and_navigation/sticky';
 * ```
 */

// ═══════════════════════════════════════════════════════════════════════════
// PROVIDER & CONTEXT
// ═══════════════════════════════════════════════════════════════════════════

export { StickyProvider, useStickyContext } from './contexts/StickyContext';

// ═══════════════════════════════════════════════════════════════════════════
// HOOKS
// ═══════════════════════════════════════════════════════════════════════════

export { useSticky } from './hooks/useSticky';
export type { UseStickyReturn } from './hooks/useSticky';

export { useScrollShrink } from './hooks/useScrollShrink';
export type { UseScrollShrinkOptions } from './hooks/useScrollShrink';
export { getShrinkClasses, getShrinkStyles } from './hooks/useScrollShrink';

// ═══════════════════════════════════════════════════════════════════════════
// COMPONENTS
// ═══════════════════════════════════════════════════════════════════════════

export { StickyContainer } from './components/StickyContainer';

// ═══════════════════════════════════════════════════════════════════════════
// CONFIGURATION & HELPERS
// ═══════════════════════════════════════════════════════════════════════════

export {
    STICKY_LAYERS,
    getStickyOffset,
    getStickyLayer,
    getTotalStickyHeight,
    getStickyLayerIds,
    getChildLayers,
    getLayerPath,
    shouldLayerBeActive
} from './config/stickyConfig';

export type { StickyLayerConfig, StickyActivationContext } from './config/stickyConfig';

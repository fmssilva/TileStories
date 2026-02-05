// ============================================================================
// NATIVE ZOOM ENHANCER - AUTO-SNAP TO IDEAL ZOOM
// ============================================================================

/**
 * Enhanced native browser zoom with auto-snap functionality
 * 
 * Features:
 * ✅ Uses native browser zoom (pinch, scroll wheel, etc.)
 * ✅ Auto-snaps to ideal zoom level (100%) when close
 * ✅ Smooth transition when auto-snapping
 * ✅ No UI components - purely behavioral enhancement
 * 
 * UX Benefits:
 * - Natural zoom experience users expect
 * - Helps users find the optimal viewing zoom
 * - Reduces accidental over/under zooming
 * - Works with all native zoom methods (pinch, scroll, etc.)
 */

let isAutoSnapping = false;
const IDEAL_ZOOM = 1.0;
const SNAP_THRESHOLD = 0.15; // 15% tolerance around ideal zoom
const SNAP_DEBOUNCE = 300; // ms to wait before checking for auto-snap
const TRANSITION_DURATION = 200; // ms for smooth snap animation

/**
 * Initialize native zoom enhancement
 */
export function initializeNativeZoom() {
    let debounceTimer: number | null = null;
    let handleZoomChange: (() => void) | null = null;

    // Listen for zoom changes via visual viewport API (most reliable)
    if ('visualViewport' in window && window.visualViewport) {
        handleZoomChange = () => {
            if (isAutoSnapping) return;

            // Clear existing timer
            if (debounceTimer) {
                clearTimeout(debounceTimer);
            }

            // Debounce to avoid excessive checks during continuous zoom
            debounceTimer = window.setTimeout(() => {
                checkAndSnapToIdealZoom();
            }, SNAP_DEBOUNCE);
        };

        window.visualViewport.addEventListener('resize', handleZoomChange);
        window.visualViewport.addEventListener('scroll', handleZoomChange);
    }

    // Fallback for older browsers - listen to window resize
    const handleWindowResize = () => {
        if (isAutoSnapping) return;

        if (debounceTimer) {
            clearTimeout(debounceTimer);
        }

        debounceTimer = window.setTimeout(() => {
            checkAndSnapToIdealZoom();
        }, SNAP_DEBOUNCE);
    };

    window.addEventListener('resize', handleWindowResize);

    // Cleanup function
    return () => {
        if (debounceTimer) {
            clearTimeout(debounceTimer);
        }
        if (window.visualViewport && handleZoomChange) {
            window.visualViewport.removeEventListener('resize', handleZoomChange);
            window.visualViewport.removeEventListener('scroll', handleZoomChange);
        }
        window.removeEventListener('resize', handleWindowResize);
    };
}/**
 * Get current zoom level
 */
function getCurrentZoom(): number {
    // Try visual viewport first (most accurate)
    if ('visualViewport' in window && window.visualViewport) {
        return window.visualViewport.scale || 1.0;
    }

    // Fallback method using screen width
    return window.screen.width / window.innerWidth;
}

/**
 * Check if current zoom is close to ideal and snap if needed
 */
function checkAndSnapToIdealZoom() {
    const currentZoom = getCurrentZoom();
    const distanceFromIdeal = Math.abs(currentZoom - IDEAL_ZOOM);

    // Check if we're close enough to ideal zoom to trigger snap
    if (distanceFromIdeal <= SNAP_THRESHOLD && distanceFromIdeal > 0.02) {
        snapToIdealZoom();
    }
}

/**
 * Smoothly snap to ideal zoom level
 */
function snapToIdealZoom() {
    if (isAutoSnapping) return;

    isAutoSnapping = true;

    // Add smooth transition
    document.documentElement.style.transition = `transform ${TRANSITION_DURATION}ms ease-out`;

    // Reset zoom by manipulating the viewport meta tag
    const viewport = document.querySelector('meta[name="viewport"]');
    if (viewport) {
        const content = viewport.getAttribute('content') || '';

        // Temporarily force zoom to 1.0
        const newContent = content.replace(/initial-scale=[\d.]+/, 'initial-scale=1.0');
        viewport.setAttribute('content', newContent);

        // Restore original content after animation
        setTimeout(() => {
            viewport.setAttribute('content', content);
            document.documentElement.style.transition = '';
            isAutoSnapping = false;
        }, TRANSITION_DURATION + 50);
    } else {
        // Fallback - just reset the flag
        setTimeout(() => {
            document.documentElement.style.transition = '';
            isAutoSnapping = false;
        }, TRANSITION_DURATION);
    }
}
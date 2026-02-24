/**
 * LAYOUT & COMPONENT CONSTANTS
 * =============================
 * 
 * Specific dimensions for layouts and components.
 * Complements the global size system (sizes.ts) with concrete values.
 * 
 * SEPARATION OF CONCERNS:
 * - sizes.ts: Global spacing scale, component size foundations
 * - sizeHelpers.ts: Utility functions for dynamic sizing
 * - layout.ts: THIS FILE - Specific layout dimensions and component sizes
 */

// ============================================================================
// LAYOUT DIMENSIONS
// ============================================================================

export const LAYOUT = {
    // ───────────────────────────────────────────────────────────────────────
    // HEADER 
    // ───────────────────────────────────────────────────────────────────────
    // PC measurements (pixels)
    HEADER_HEIGHT: 80,
    MOBILE_HEADER_HEIGHT: 60,  // Smaller header for mobile
    GAP_LOGO_NAV: 72,      // Gap between logo and navigation
    GAP_NAV_ICONS: 32,     // Gap between navigation and icons
    GAP_ICONS_RIGHT: 16,   // Gap between icons and right edge of header
    CONTAINERS_HORIZONTAL_PADDING: 32, // Total horizontal padding (left + right)

    // FIXED SIZING FOR COMPONENTS (no more percentages!)
    // Logo & Name
    LOGO_HEIGHT: 58,
    LOGO_NAME_FONT_SIZE: 28,        // Font size for "TileStories" text
    LOGO_NAME_GAP: 12,              // Gap between logo icon and text

    // Nav Tabs & More Button
    NAV_TAB_FONT_SIZE: 20,          // Font size for nav tab text
    NAV_TAB_PADDING_X: 24,          // Horizontal padding for nav tabs
    NAV_TAB_PADDING_Y: 12,          // Vertical padding for nav tabs
    NAV_TAB_HEIGHT: 44,             // Total height for nav tab button (font + padding)

    // Icons (Theme Toggle & Language Selector)
    ICON_BUTTON_SIZE: 44,           // Square button size for icons
    ICON_SIZE: 32,                  // Size of icon/emoji inside button

    // Hamburger Menu
    HAMBURGER_ICON_SIZE: 28,        // Size of hamburger menu icon

    // NAV TABS 
    TAB_WIDTH_ESTIMATE: 120, // Estimated average width of a nav tab (in pixels), used for responsive calculations
    MORE_BUTTON_WIDTH: 100,  // Estimated width of the "More" button when tabs overflow (in pixels)
    MIN_TABS_BEFORE_HAMBURGER: 2, // Minimum number of visible tabs before switching to hamburger menu on small screens    
    HAMBURGER_THRESHOLD: (2 * 120) + 100, // MIN_TABS_BEFORE_HAMBURGER * TAB_WIDTH_ESTIMATE + MORE_BUTTON_WIDTH = 340


    // ───────────────────────────────────────────────────────────────────────
    // STICKY ELEMENTS
    // ───────────────────────────────────────────────────────────────────────
    SPACE_BETWEEN_STICKY_ELEMENTS: 8, // Minimum vertical space between stacked sticky elements (pixels)

    // ───────────────────────────────────────────────────────────────────────
    // FOOTER 
    // ───────────────────────────────────────────────────────────────────────
    FOOTER_HEIGHT: 200,


    // ───────────────────────────────────────────────────────────────────────
    // SECTION SPACING
    // ───────────────────────────────────────────────────────────────────────
    /** Standard vertical padding for sections (pixels) */
    SECTION_PADDING_Y: 64,
    /** Section padding on tablet (pixels) */
    SECTION_PADDING_Y_TABLET: 48,
    /** Section padding on mobile (pixels) */
    SECTION_PADDING_Y_MOBILE: 32,


    // ───────────────────────────────────────────────────────────────────────
    // CONTAINER WIDTHS
    // ───────────────────────────────────────────────────────────────────────
    /** Maximum content width for general content (pixels) */
    MAX_CONTENT_WIDTH: 1280,
    /** Maximum text width for optimal readability (pixels) */
    MAX_TEXT_WIDTH: 720,
    /** Maximum width for forms (pixels) */
    MAX_FORM_WIDTH: 640,

    // ───────────────────────────────────────────────────────────────────────
    // RESPONSIVE BREAKPOINTS (matches Tailwind)
    // ───────────────────────────────────────────────────────────────────────
    BREAKPOINTS: {
        SM: 640,   // sm: (pixels)
        MD: 768,   // md: (pixels)
        LG: 1024,  // lg: (pixels)
        XL: 1280,  // xl: (pixels)
        '2XL': 1536, // 2xl: (pixels)
    },
} as const;

// ============================================================================
// COMPONENT SIZES
// ============================================================================

export const COMPONENT_SIZES = {
    // ───────────────────────────────────────────────────────────────────────
    // BUTTONS
    // ───────────────────────────────────────────────────────────────────────
    BUTTON: {
        HEIGHT: {
            SM: 36,
            MD: 44,
            LG: 56,
        },
        PADDING_X: {
            SM: 16,
            MD: 24,
            LG: 32,
        },
    },

    // ───────────────────────────────────────────────────────────────────────
    // INPUTS
    // ───────────────────────────────────────────────────────────────────────
    INPUT: {
        HEIGHT: {
            SM: 36,
            MD: 44,
            LG: 52,
        },
    },

    // ───────────────────────────────────────────────────────────────────────
    // MODALS
    // ───────────────────────────────────────────────────────────────────────
    MODAL: {
        /** Modal max width as percentage of viewport */
        MAX_WIDTH_PERCENT: 95,
        /** Minimum margin from screen edges (pixels) */
        SCREEN_MARGIN: 16,
        /** Modal max height as percentage of viewport */
        MAX_HEIGHT_PERCENT: 90,

        WIDTHS: {
            SM: 400,
            MD: 600,
            LG: 800,
            XL: 1000,
        },
    },

    // ───────────────────────────────────────────────────────────────────────
    // BORDER RADIUS
    // ───────────────────────────────────────────────────────────────────────
    BORDER_RADIUS: {
        SM: 4,
        MD: 8,
        LG: 16,
        XL: 24,
        FULL: 9999,
    },
} as const;

// ============================================================================
// TYPE DEFINITIONS
// ============================================================================

export type LayoutKey = keyof typeof LAYOUT;
export type ComponentSizeKey = keyof typeof COMPONENT_SIZES;

// ============================================================================
// HELPER FUNCTIONS
// ============================================================================

/**
 * Check if viewport is mobile size
 */
export function isMobileViewport(): boolean {
    if (typeof window === 'undefined') return false;
    return window.innerWidth < LAYOUT.BREAKPOINTS.MD;
}

/**
 * Check if viewport is tablet size
 */
export function isTabletViewport(): boolean {
    if (typeof window === 'undefined') return false;
    return window.innerWidth >= LAYOUT.BREAKPOINTS.MD && window.innerWidth < LAYOUT.BREAKPOINTS.LG;
}

/**
 * Check if viewport is desktop size
 */
export function isDesktopViewport(): boolean {
    if (typeof window === 'undefined') return false;
    return window.innerWidth >= LAYOUT.BREAKPOINTS.LG;
}

/**
 * Get responsive header height based on viewport
 */
export function getHeaderHeight(): number {
    return isMobileViewport() ? LAYOUT.MOBILE_HEADER_HEIGHT : LAYOUT.HEADER_HEIGHT;
}

/**
 * Get responsive section padding based on viewport
 */
export function getSectionPaddingY(): number {
    if (isMobileViewport()) return LAYOUT.SECTION_PADDING_Y_MOBILE;
    if (isTabletViewport()) return LAYOUT.SECTION_PADDING_Y_TABLET;
    return LAYOUT.SECTION_PADDING_Y;
}


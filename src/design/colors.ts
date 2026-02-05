/**
 * GLOBAL COLOR SYSTEM - TIER 1 (Foundation)
 * ===========================================
 * 
 * This file contains ONLY the foundational colors that affect the entire application.
 * These should rarely change as they impact everything.
 * 
 * What goes here:
 * ✅ Brand identity colors (primary teal - matches logo)
 * ✅ Semantic colors (error, success, warning) 
 * ✅ Base neutrals (black, white, grays)
 * ✅ Light/Dark theme foundations
 * 
 * What does NOT go here:
 * ❌ Header-specific colors (those are domain-level)
 * ❌ Component hover states (those are local-level)  
 * ❌ Domain-specific themes (Pokemon, Admin, etc.)
 */

// ============================================================================
// 1. BRAND IDENTITY COLORS (Global Foundation)
// ============================================================================
// These define your brand and should match your logo/design system

export const globalColors = {
    // PRIMARY BRAND COLOR - Teal (matches logo and PWA icons)
    brand: {
        50: '#f0fdfa',
        100: '#ccfbf1',
        200: '#99f6e4',
        300: '#5eead4',
        400: '#2dd4bf',
        500: '#14b8a6',  // 🎯 PRIMARY BRAND COLOR (logo teal)
        600: '#0d9488',
        700: '#0f766e',
        800: '#115e59',
        900: '#134e4a',
        950: '#042f2e',
    },

    // NEUTRALS - For text, backgrounds, borders
    gray: {
        50: '#f8fafc',
        100: '#f1f5f9',
        200: '#e2e8f0',
        300: '#cbd5e1',
        400: '#94a3b8',
        500: '#64748b',  // 🎯 DEFAULT TEXT COLOR
        600: '#475569',
        700: '#334155',
        800: '#1e293b',
        900: '#0f172a',
        950: '#020617',
    },

    // SEMANTIC COLORS - For status/feedback (global across all domains)
    semantic: {
        success: '#22c55e',   // Green
        error: '#ef4444',     // Red  
        warning: '#f59e0b',   // Orange
        info: '#3b82f6',      // Blue
    },

    // PURE COLORS - For absolute values
    pure: {
        white: '#ffffff',
        black: '#000000',
        transparent: 'transparent',
    },
} as const;

// ============================================================================
// 2. THEME SYSTEM (Global Light/Dark)
// ============================================================================
// Foundation for light/dark mode across entire app

export const themeColors = {
    light: {
        // Backgrounds
        background: globalColors.pure.white,
        backgroundSoft: globalColors.gray[50],
        surface: globalColors.gray[100],

        // Text
        text: globalColors.gray[900],
        textSoft: globalColors.gray[700],
        textMuted: globalColors.gray[500],

        // Borders & Dividers
        border: globalColors.gray[200],
        borderSoft: globalColors.gray[100],

        // Primary brand in context
        primary: globalColors.brand[500],
        primaryForeground: globalColors.pure.white,

        // Semantic in context
        success: globalColors.semantic.success,
        error: globalColors.semantic.error,
        warning: globalColors.semantic.warning,
        info: globalColors.semantic.info,
    },

    dark: {
        // Backgrounds  
        background: globalColors.gray[950],
        backgroundSoft: globalColors.gray[900],
        surface: globalColors.gray[800],

        // Text
        text: globalColors.gray[50],
        textSoft: globalColors.gray[200],
        textMuted: globalColors.gray[400],

        // Borders & Dividers
        border: globalColors.gray[800],
        borderSoft: globalColors.gray[700],

        // Primary brand in context
        primary: globalColors.brand[400],  // Lighter for dark theme
        primaryForeground: globalColors.gray[900],

        // Semantic in context (same as light - semantic colors are universal)
        success: globalColors.semantic.success,
        error: globalColors.semantic.error,
        warning: globalColors.semantic.warning,
        info: globalColors.semantic.info,
    },
} as const;

// ============================================================================
// 3. UTILITY FUNCTIONS (Global)
// ============================================================================

export type Theme = 'light' | 'dark';

/**
 * Get theme colors for current mode
 */
export function getThemeColors(theme: Theme) {
    return themeColors[theme];
}

/**
 * Get brand color by shade
 */
export function getBrandColor(shade: keyof typeof globalColors.brand = 500) {
    return globalColors.brand[shade];
}

/**
 * Create CSS gradient from brand colors
 */
export function getBrandGradient(from: keyof typeof globalColors.brand = 600, to: keyof typeof globalColors.brand = 400): string {
    return `linear-gradient(135deg, ${globalColors.brand[from]} 0%, ${globalColors.brand[to]} 100%)`;
}
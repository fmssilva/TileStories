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
    // PRIMARY BRAND COLOR - Azulejo Blue (from traditional Portuguese tiles)
    primary: {
        50: '#EBF1F8',
        100: '#D7E3F1',
        200: '#AFC7E3',
        300: '#87ABD5',
        400: '#5F8FC7',
        500: '#3C5E95',  // 🎯 MAIN AZULEJO BLUE (traditional tile color)
        600: '#2F4B77',
        700: '#233859',
        800: '#17253B',
        900: '#0B121E',
    },

    // SECONDARY - Azulejo Cobalt (lighter accent blue)
    secondary: {
        50: '#EEF4F9',
        100: '#DDE9F3',
        200: '#BBD3E7',
        300: '#99BDDB',
        400: '#77A7CF',
        500: '#5081B6',  // 🎯 COBALT ACCENT
        600: '#406792',
        700: '#304D6E',
        800: '#203449',
        900: '#101A25',
    },

    // ACCENT COLORS - From azulejo decorations
    accent: {
        gold: {
            300: '#E8C96F',
            500: '#D4AF37',  // 🎯 GOLD (from tile decorations)
            600: '#B8962E',
            700: '#9C7D25',
        },
        terracotta: {
            300: '#E8673A',
            500: '#C1440E',  // 🎯 TERRACOTTA (earthquake/drama sections)
            600: '#9D360B',
            700: '#7A2908',
        },
        ivory: {
            300: '#FFFCF2',
            500: '#FFF8E7',  // 🎯 IVORY (traditional tile background)
            600: '#F5ECDB',
            700: '#EBE0CF',
        },
    },

    // NEUTRALS - For text, backgrounds, borders
    gray: {
        50: '#f8fafc',
        100: '#f1f5f9',
        200: '#e2e8f0',
        300: '#cbd5e1',
        400: '#94a3b8',
        500: '#64748b',
        600: '#475569',
        700: '#334155',
        800: '#1e293b',
        900: '#0f172a',
        950: '#020617',
    },

    // SEMANTIC COLORS - For status/feedback (global across all domains)
    semantic: {
        success: '#4CAF50',   // Keep green
        error: '#ef4444',     // Keep red  
        warning: '#f59e0b',   // Keep orange
        info: '#3C5E95',      // Use primary azulejo blue
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
        primary: globalColors.primary[500],
        primaryForeground: globalColors.pure.white,
        secondary: globalColors.secondary[500],
        accent: globalColors.accent.gold[500],

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
        primary: globalColors.primary[400],  // Lighter for dark theme
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
export function getPrimaryColor(shade: keyof typeof globalColors.primary = 500) {
    return globalColors.primary[shade];
}

/**
 * Create CSS gradient from brand colors
 */
export function getPrimaryGradient(from: keyof typeof globalColors.primary = 600, to: keyof typeof globalColors.primary = 400): string {
    return `linear-gradient(135deg, ${globalColors.primary[from]} 0%, ${globalColors.primary[to]} 100%)`;
}
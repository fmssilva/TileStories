// ============================================================================
// HEADER DOMAIN COLORS - TIER 2 (Domain-Specific)
// ============================================================================

/**
 * Header-specific colors following three-tier architecture.
 * 
 * Uses GLOBAL brand colors as foundation, but adds domain-specific variations
 * for header's unique needs (gradients, scroll states, navigation).
 * 
 * This is TIER 2 - Domain level colors that:
 * ✅ Import from global color system
 * ✅ Add header-specific variations (gradients, states)
 * ✅ Can be changed without affecting other domains
 * ❌ Don't redefine base brand colors
 */

import { globalColors, getThemeColors, getBrandGradient, type Theme } from '@/design/colors';

// ============================================================================
// HEADER DOMAIN CONFIGURATION
// ============================================================================

export const headerColors = {
    // Domain-specific gradients using global brand colors
    gradients: {
        // Primary header gradient with smoother white fade from left for logo visibility
        main: `linear-gradient(135deg, rgba(255,255,255,0.4) 0%, rgba(255,255,255,0.25) 15%, rgba(255,255,255,0.1) 25%, ${globalColors.brand[600]} 40%, ${globalColors.brand[400]} 100%)`,

        // Scrolled state (with opacity) - also smoother
        scrolled: (opacity: number = 0.95) => {
            const hex600 = globalColors.brand[600];
            const hex400 = globalColors.brand[400];
            const opacity255 = Math.round(opacity * 255).toString(16).padStart(2, '0');
            return `linear-gradient(135deg, rgba(255,255,255,0.3) 0%, rgba(255,255,255,0.2) 15%, rgba(255,255,255,0.08) 25%, ${hex600}${opacity255} 40%, ${hex400}${opacity255} 100%)`;
        },

        // Dark theme gradient
        dark: getBrandGradient(900, 700),  // Very dark to dark teal
    },

    // Add primary colors for backward compatibility
    primary: globalColors.brand,
} as const;

// ============================================================================
// HEADER THEME STATES
// ============================================================================

/**
 * Get header styles based on theme and scroll state
 */
export function getHeaderStyles(theme: Theme, isScrolled: boolean = false) {
    const colors = getThemeColors(theme);

    if (isScrolled) {
        return {
            background: headerColors.gradients.scrolled(),
            backdropFilter: 'blur(8px)',
            borderColor: colors.border,
            // ✅ FIX: Keep text white/light even when scrolled for better contrast on gradient
            textColor: globalColors.pure.white,
            shadow: 'shadow-lg',
        };
    }

    return {
        background: headerColors.gradients.main,
        backdropFilter: 'none',
        borderColor: 'transparent',
        textColor: globalColors.pure.white,
        shadow: 'shadow-none',
    };
}

/**
 * Get footer styles based on theme
 */
export function getFooterStyles(theme: Theme) {
    const colors = getThemeColors(theme);

    return {
        backgroundColor: colors.backgroundSoft,
        borderColor: colors.border,
        textColor: colors.text,
        linkColor: globalColors.brand[500],
        linkHoverColor: globalColors.brand[600],
    };
}

/**
 * Get navigation link styles based on theme and active state
 */
export function getNavLinkStyles(_theme: Theme, isActive: boolean) {
    return {
        color: isActive ? 'rgba(255, 255, 255, 1)' : 'rgba(255, 255, 255, 0.9)',
        hoverColor: 'rgba(255, 255, 255, 1)',
        backgroundColor: isActive ? 'rgba(255, 255, 255, 0.25)' : 'transparent',
        hoverBackgroundColor: isActive ? 'rgba(255, 255, 255, 0.35)' : 'rgba(255, 255, 255, 0.15)',
    };
}
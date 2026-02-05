/**
 * LAYOUT & NAVIGATION SIZES - TIER 2 (Domain-specific)
 * ======================================================
 * 
 * This file contains size patterns specific to the layout and navigation domain.
 * These build upon the global foundations from /design/sizes.ts
 * 
 * What goes here:
 * ✅ Header-specific sizing (height, logo size, nav gaps)
 * ✅ Footer layout patterns
 * ✅ Navigation spacing patterns
 * ✅ Domain-specific responsive behaviors
 * 
 * What does NOT go here:
 * ❌ Global spacing foundations (those are in /design/sizes.ts)
 * ❌ Component-specific micro-adjustments (those are local)
 */

import { spacing, componentSizes } from '@/design/sizes';

// ============================================================================
// 1. HEADER DOMAIN SIZING
// ============================================================================

export const headerSizes = {
    // Core header dimensions
    height: '4rem',                    // 64px - h-16
    padding: spacing[4],               // 16px - px-4

    // Logo sizing
    logo: {
        mobile: componentSizes.icon.lg,  // 24px on mobile
        desktop: componentSizes.icon.xl, // 32px on desktop
    },

    // Navigation patterns
    navigation: {
        gap: spacing[6],                 // 24px between nav items - space-x-6
        mobileGap: spacing[3],          // 12px on mobile - space-x-3
    },

    // Action buttons in header
    actions: {
        gap: spacing[2],                // 8px between action buttons - gap-2
        buttonPadding: spacing[2],      // 8px button padding - p-2
    },
} as const;

// ============================================================================
// 2. FOOTER DOMAIN SIZING
// ============================================================================

export const footerSizes = {
    // Core footer layout
    padding: {
        x: spacing[4],                  // 16px horizontal - px-4
        y: spacing[8],                  // 32px vertical - py-8
    },

    // Footer sections
    sections: {
        gap: spacing[8],                // 32px between sections - gap-8
        marginBottom: spacing[8],       // 32px bottom margin - mb-8
    },

    // Social links
    social: {
        gap: spacing[4],               // 16px between social icons - gap-4
        iconPadding: spacing[2],       // 8px icon padding - p-2
    },
} as const;

// ============================================================================
// 3. LAYOUT PRESETS (Domain-specific patterns)
// ============================================================================

export const layoutPresets = {
    // Header patterns
    header: {
        base: `h-16 px-4 flex items-center justify-between`,
        nav: 'hidden md:flex items-center space-x-6',
        mobileNav: 'md:hidden flex items-center gap-2',
        actions: 'flex items-center gap-2',
    },

    // Footer patterns  
    footer: {
        container: 'px-4 py-8',
        grid: 'grid grid-cols-1 md:grid-cols-3 gap-8 mb-8',
        social: 'flex gap-4',
    },

    // Page layout patterns
    page: {
        main: 'container mx-auto px-4',
        section: 'py-16 lg:py-24',
        hero: 'text-center mb-16',
    },
} as const;
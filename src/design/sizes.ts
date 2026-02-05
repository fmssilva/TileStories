/**
 * GLOBAL SIZE SYSTEM - TIER 1 (Foundation)
 * ==========================================
 * 
 * Following the three-tier architecture like our color system:
 * TIER 1: Global foundations (this file) 
 * TIER 2: Domain-specific sizing (e.g., header/sizes.ts)
 * TIER 3: Local component sizing (within components)
 * 
 * What goes here:
 * ✅ Spacing scale that matches Tailwind exactly
 * ✅ Basic component size foundations only
 * ✅ Layout fundamentals (breakpoints, containers)
 * 
 * What does NOT go here:
 * ❌ Domain-specific patterns (header height, hero spacing)
 * ❌ Complex component configurations
 * ❌ Helper functions (those go in sizeHelpers.ts)
 */

// ============================================================================
// 1. GLOBAL SPACING SCALE
// ============================================================================
// Matches Tailwind's spacing scale exactly - use Tailwind classes when possible

export const spacing = {
    1: '0.25rem',    // 4px  - Use p-1, m-1, etc.
    2: '0.5rem',     // 8px  - Use p-2, m-2, etc.
    3: '0.75rem',    // 12px - Use p-3, m-3, etc.
    4: '1rem',       // 16px - Use p-4, m-4, etc.
    6: '1.5rem',     // 24px - Use p-6, m-6, etc.
    8: '2rem',       // 32px - Use p-8, m-8, etc.
    12: '3rem',      // 48px - Use p-12, m-12, etc.
    16: '4rem',      // 64px - Use p-16, m-16, etc.
    20: '5rem',      // 80px - Use p-20, m-20, etc.
    24: '6rem',      // 96px - Use p-24, m-24, etc.
} as const;

// ============================================================================
// 2. COMPONENT SIZE FOUNDATIONS
// ============================================================================
// Basic size scales - components can use these or define their own

export const componentSizes = {
    // Icons
    icon: {
        sm: '1rem',      // 16px - w-4 h-4
        md: '1.25rem',   // 20px - w-5 h-5  
        lg: '1.5rem',    // 24px - w-6 h-6
        xl: '2rem',      // 32px - w-8 h-8
    },

    // Interactive elements
    interactive: {
        sm: '2rem',      // 32px - h-8
        md: '2.25rem',   // 36px - h-9
        lg: '2.5rem',    // 40px - h-10
    },
} as const;

// ============================================================================
// 3. LAYOUT FOUNDATIONS
// ============================================================================
// Global layout constants

export const layout = {
    // Container max-widths (matches Tailwind)
    maxWidth: {
        sm: '640px',     // max-w-sm
        md: '768px',     // max-w-md
        lg: '1024px',    // max-w-lg
        xl: '1280px',    // max-w-xl
        '2xl': '1536px', // max-w-2xl
    },

    // Responsive breakpoints (matches Tailwind)
    breakpoint: {
        sm: '640px',    // sm:
        md: '768px',    // md:
        lg: '1024px',   // lg:
        xl: '1280px',   // xl:
        '2xl': '1536px' // 2xl:
    },
} as const;
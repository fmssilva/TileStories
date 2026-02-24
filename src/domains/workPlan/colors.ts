/**
 * WORK PLAN DOMAIN - COLOR SYSTEM (TIER 2)
 * ==========================================
 * 
 * Domain-specific colors for the Work Plan page.
 * Follows the 3-tier color system (Global → Domain → Component).
 * 
 * Phase-specific colors:
 * - Phase 1 (MVP): Azulejo Blue #3C5E95
 * - Phase 2 (Core): Azulejo Cobalt #5081B6
 * - Phase 3 (Advanced): Azulejo Terracotta #C1440E
 * - Phase 4 (Optimization): Azulejo Gold #D4AF37
 */

import { globalColors, getPrimaryColor, getPrimaryGradient } from '@/design/colors';

/**
 * Get color for a specific development phase
 * @param phase Phase number (1-4)
 * @returns Hex color string
 */
export function getPhaseColor(phase: 1 | 2 | 3 | 4): string {
    const phaseColors = {
        1: globalColors.primary[500],        // MVP - Azulejo Blue
        2: globalColors.secondary[500],      // Core - Cobalt
        3: globalColors.accent.terracotta[500], // Advanced - Terracotta
        4: globalColors.accent.gold[500],    // Optimization - Gold
    };

    return phaseColors[phase];
}

/**
 * Get gradient for a specific development phase
 * @param phase Phase number (1-4)
 * @returns CSS gradient string
 */
export function getPhaseGradient(phase: 1 | 2 | 3 | 4): string {
    const gradients = {
        1: `linear-gradient(135deg, ${globalColors.primary[600]} 0%, ${globalColors.primary[400]} 100%)`,
        2: `linear-gradient(135deg, ${globalColors.secondary[500]} 0%, ${globalColors.secondary[400]} 100%)`,
        3: `linear-gradient(135deg, ${globalColors.accent.terracotta[600]} 0%, ${globalColors.accent.terracotta[500]} 100%)`,
        4: `linear-gradient(135deg, ${globalColors.accent.gold[600]} 0%, ${globalColors.accent.gold[500]} 100%)`,
    };

    return gradients[phase];
}

/**
 * Consolidated work plan colors object
 * Provides access to both global and phase-specific colors
 */
export const workPlanColors = {
    // Re-export global utilities for consistency
    primary: getPrimaryColor,
    primaryGradient: getPrimaryGradient,

    // Phase-specific utilities
    phaseColor: getPhaseColor,
    phaseGradient: getPhaseGradient,

    // Direct color references for convenience
    phases: {
        mvp: globalColors.primary[500],
        core: globalColors.secondary[500],
        advanced: globalColors.accent.terracotta[500],
        optimization: globalColors.accent.gold[500],
    },
} as const;

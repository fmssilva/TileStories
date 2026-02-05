/**
 * POKEMON DOMAIN COLORS - TIER 2 (Domain-Specific)
 * ==================================================
 * 
 * Pokemon-specific colors following three-tier architecture.
 * 
 * Uses GLOBAL semantic colors for status, but adds Pokemon-specific
 * blue theme and type colors that don't affect other domains.
 * 
 * This is TIER 2 - Domain level colors that:
 * ✅ Use global colors where appropriate
 * ✅ Add Pokemon-specific blue theme  
 * ✅ Include Pokemon type colors (unique to this domain)
 * ❌ Don't redefine global brand or semantic colors
 */

import { globalColors, getThemeColors, type Theme } from '@/design/colors';

// ============================================================================
// POKEMON DOMAIN COLORS
// ============================================================================

export const pokemonColors = {
    // Pokemon domain uses BLUE theme (different from global teal)
    primary: '#3b82f6',    // Pokemon blue
    secondary: '#facc15',  // Electric yellow

    // Pokemon type colors (unique to Pokemon domain)
    types: {
        normal: '#A8A878',
        fire: '#F08030',
        water: '#6890F0',
        electric: '#F8D030',
        grass: '#78C850',
        ice: '#98D8D8',
        fighting: '#C03028',
        poison: '#A040A0',
        ground: '#E0C068',
        flying: '#A890F0',
        psychic: '#F85888',
        bug: '#A8B820',
        rock: '#B8A038',
        ghost: '#705898',
        dragon: '#7038F8',
        dark: '#705848',
        steel: '#B8B8D0',
        fairy: '#EE99AC',
    },
} as const;

// ============================================================================
// POKEMON THEME HELPERS
// ============================================================================

/**
 * Pokemon theme configuration for consistency
 */
export const pokemonTheme = {
    primary: pokemonColors.primary,
    secondary: pokemonColors.secondary,
    types: pokemonColors.types,

    // Additional theme properties for component usage
    headerGradient: {
        css: `linear-gradient(135deg, #1e40af 0%, ${pokemonColors.primary} 100%)`,
    },
    button: {
        primary: pokemonColors.primary,
    },
    text: {
        primary: globalColors.pure.white,
        secondary: globalColors.gray[300],
    },
} as const;

/**
 * Get Pokemon card styles based on theme
 */
export function getPokemonCardStyles(theme: Theme) {
    const colors = getThemeColors(theme);

    return {
        // Card background uses global theme colors
        background: colors.surface,
        border: colors.border,
        text: colors.text,
        textSecondary: colors.textMuted,

        // Header uses Pokemon domain blue (not global teal)
        headerBackground: `linear-gradient(135deg, #1e40af 0%, ${pokemonColors.primary} 100%)`,
        headerText: globalColors.pure.white,

        // Status uses global semantic colors
        success: colors.success,
        error: colors.error,
        loading: colors.textMuted,
    };
}

/**
 * Get Pokemon type color
 */
export function getPokemonTypeColor(type: string): string {
    return pokemonColors.types[type as keyof typeof pokemonColors.types] || pokemonColors.primary;
}

/**
 * Get Pokemon gradient (domain-specific, not using global brand colors)
 */
export function getPokemonGradient(): string {
    return `linear-gradient(135deg, #1e40af 0%, ${pokemonColors.primary} 100%)`;
}
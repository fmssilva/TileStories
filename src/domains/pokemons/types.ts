/**
 * POKEMON DOMAIN TYPES
 * =====================
 * 
 * TypeScript definitions for Pokemon data from PokeAPI.
 */

// Pokemon basic info from PokeAPI
export interface Pokemon {
    id: number;
    name: string;
    height: number;
    weight: number;
    sprites: {
        front_default: string | null;
        front_shiny: string | null;
        other: {
            'official-artwork': {
                front_default: string | null;
            };
        };
    };
    types: Array<{
        type: {
            name: string;
        };
    }>;
    stats: Array<{
        base_stat: number;
        stat: {
            name: string;
        };
    }>;
}

// Simplified Pokemon for card display
export interface PokemonCard {
    id: number;
    name: string;
    image: string;
    types: string[];
    height: number;
    weight: number;
}
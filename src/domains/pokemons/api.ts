/**
 * POKEMON API - RTK Query Integration
 * ===================================
 * 
 * Domain-specific API slice for Pokemon data using PokeAPI.
 * All Pokemon-related API calls are managed here.
 */

import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';
import { Pokemon, PokemonCard } from './types';

// Pokemon API slice
export const pokemonApi = createApi({
    reducerPath: 'pokemonApi',

    baseQuery: fetchBaseQuery({
        baseUrl: 'https://pokeapi.co/api/v2/',
    }),

    tagTypes: ['Pokemon'],

    endpoints: (builder) => ({
        // Get a specific Pokemon by ID or name
        getPokemon: builder.query<Pokemon, string | number>({
            query: (id) => `pokemon/${id}`,
            providesTags: (_result, _error, id) => [{ type: 'Pokemon', id }],
        }),

        // Get random Pokemon for demo purposes
        getRandomPokemon: builder.query<Pokemon, void>({
            query: () => {
                const randomId = Math.floor(Math.random() * 150) + 1; // Original 150 Pokemon
                return `pokemon/${randomId}`;
            },
            providesTags: ['Pokemon'],
        }),
    }),
});

// Export hooks for components
export const {
    useGetPokemonQuery,
    useGetRandomPokemonQuery,
    useLazyGetPokemonQuery
} = pokemonApi;

// Transform Pokemon data for card display
export function transformPokemonForCard(pokemon: Pokemon): PokemonCard {
    return {
        id: pokemon.id,
        name: pokemon.name,
        image: pokemon.sprites.other['official-artwork'].front_default ||
            pokemon.sprites.front_default ||
            '',
        types: pokemon.types.map(t => t.type.name),
        height: pokemon.height,
        weight: pokemon.weight,
    };
}
/**
 * POKEMON DOMAIN - Public API
 * ============================
 * 
 * Central export point for Pokemon domain functionality.
 */

// Types
export type { Pokemon, PokemonCard as PokemonCardData } from './types';

// API and hooks
export {
    pokemonApi,
    useGetPokemonQuery,
    useGetRandomPokemonQuery,
    useLazyGetPokemonQuery,
    transformPokemonForCard
} from './api';

// Components
export { PokemonCard } from './PokemonCard';
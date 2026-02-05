/**
 * POKEMON CARD - Minimalist Design with Domain Colors
 * ====================================================
 * 
 * Simple, clean Pokemon card component with domain-specific styling.
 */

import { useGetRandomPokemonQuery, transformPokemonForCard } from './api';
import { pokemonTheme, getPokemonTypeColor } from './colors';
import { useInlineTranslation } from '@/utils/language';

export function PokemonCard() {
    const { data: pokemon, error, isLoading, refetch } = useGetRandomPokemonQuery();

    // Inline translations
    const randomPokemon = useInlineTranslation('Pokémon Aleatório', 'Random Pokemon');
    const failedToLoad = useInlineTranslation('Falha ao carregar Pokémon', 'Failed to load Pokemon');
    const tryAgain = useInlineTranslation('Tentar novamente', 'Try again');
    const getNewPokemon = useInlineTranslation('Obter novo Pokémon', 'Get new Pokemon');
    const height = useInlineTranslation('Altura', 'Height');
    const weight = useInlineTranslation('Peso', 'Weight');

    if (isLoading) {
        return (
            <div className="bg-card border border-border rounded-lg shadow-sm overflow-hidden">
                {/* Title bar with gradient */}
                <div
                    className="px-4 py-3"
                    style={{ background: pokemonTheme.headerGradient.css }}
                >
                    <h2 className="font-bold text-white text-lg">{randomPokemon}</h2>
                </div>

                {/* Loading content */}
                <div className="p-4 animate-pulse">
                    <div className="flex items-center space-x-4">
                        <div className="w-20 h-20 bg-muted rounded-lg"></div>
                        <div className="space-y-2 flex-1">
                            <div className="h-4 bg-muted rounded w-3/4"></div>
                            <div className="h-3 bg-muted rounded w-1/2"></div>
                        </div>
                    </div>
                </div>
            </div>
        );
    }

    if (error || !pokemon) {
        return (
            <div className="bg-card border border-destructive rounded-lg shadow-sm overflow-hidden">
                {/* Title bar with gradient */}
                <div
                    className="px-4 py-3"
                    style={{ background: pokemonTheme.headerGradient.css }}
                >
                    <h2 className="font-bold text-white text-lg">{randomPokemon}</h2>
                </div>

                {/* Error content */}
                <div className="p-4 text-center text-destructive">
                    <p className="font-medium">{failedToLoad}</p>
                    <button
                        onClick={() => refetch()}
                        className="mt-2 text-sm hover:underline"
                        style={{ color: pokemonTheme.button.primary }}
                    >
                        {tryAgain}
                    </button>
                </div>
            </div>
        );
    }

    const pokemonData = transformPokemonForCard(pokemon);

    return (
        <div className="bg-card border border-border rounded-lg shadow-sm hover:shadow-md transition-shadow overflow-hidden">
            {/* Title bar with Pokemon domain gradient */}
            <div
                className="px-4 py-3 flex items-center justify-between"
                style={{ background: pokemonTheme.headerGradient.css }}
            >
                <h2 className="font-bold text-white text-lg">{randomPokemon}</h2>

                {/* Refresh Button in header */}
                <button
                    onClick={() => refetch()}
                    className="text-white hover:text-yellow-200 transition-colors p-1 rounded"
                    title={getNewPokemon}
                >
                    🎲
                </button>
            </div>

            {/* Pokemon content */}
            <div className="p-4">
                <div className="flex items-center space-x-4">
                    {/* Pokemon Image */}
                    <div className="w-20 h-20 flex-shrink-0">
                        {pokemonData.image ? (
                            <img
                                src={pokemonData.image}
                                alt={pokemonData.name}
                                className="w-full h-full object-contain"
                            />
                        ) : (
                            <div className="w-full h-full bg-muted rounded-lg flex items-center justify-center text-muted-foreground">
                                ?
                            </div>
                        )}
                    </div>

                    {/* Pokemon Info */}
                    <div className="flex-1">
                        <h3
                            className="font-semibold capitalize text-lg mb-2"
                            style={{ color: pokemonTheme.text.primary }}
                        >
                            {pokemonData.name}
                        </h3>

                        <div className="flex flex-wrap gap-1 mb-3">
                            {pokemonData.types.map((type) => (
                                <span
                                    key={type}
                                    className="px-2 py-1 rounded-full text-xs font-medium capitalize text-white"
                                    style={{ backgroundColor: getPokemonTypeColor(type) }}
                                >
                                    {type}
                                </span>
                            ))}
                        </div>

                        <div
                            className="text-sm space-y-1"
                            style={{ color: pokemonTheme.text.secondary }}
                        >
                            <div>{height}: {pokemonData.height / 10}m</div>
                            <div>{weight}: {pokemonData.weight / 10}kg</div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
}
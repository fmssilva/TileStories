/**
 * HomePage Component
 */

import { useState } from 'react';

import HeroSection from './HeroSection';
import { PokemonCard } from '@/domains/pokemons/PokemonCard';
import { SearchSection } from './SearchSection';
import { ComparePage } from '@/domains/comparing';

type SearchMethod = 'exploration' | 'symptoms';

export function HomePage() {
    const [searchMethod, setSearchMethod] = useState<SearchMethod>('exploration');

    const handleScrollToSearch = (targetMethod?: SearchMethod) => {
        // Set the search method if specified
        if (targetMethod) {
            setSearchMethod(targetMethod);
        }

        // Scroll to search section
        const searchSection = document.getElementById('search-tabs-section');
        if (searchSection) {
            searchSection.scrollIntoView({
                behavior: 'smooth',
                block: 'start',
                inline: 'nearest'
            });
        }
    };

    return (
        <>
            <HeroSection onScrollToSearch={handleScrollToSearch} />

            {/* Search Features */}
            <section id="search-tabs-section" className="container mx-auto px-4 py-16 lg:py-24 scroll-mt-4">
                <SearchSection defaultMethod={searchMethod} />
                <ComparePage />
            </section>

            {/* Reviews and Social Proof (removed placeholder) */}

            {/* Small demo card (Pokémon) — placeholder component to verify home rendering */}
            <section className="container mx-auto px-4 py-8">
                <h3 className="text-lg font-semibold mb-4">Demo Card</h3>
                <div className="max-w-xs">
                    <PokemonCard />
                </div>
            </section>

        </>
    );
}
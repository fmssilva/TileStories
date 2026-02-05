/**
 * HomePage Component - TileStories Landing Page
 */

import HeroSection from './HeroSection';

export function HomePage() {
    const handleScrollToSearch = () => {
        // Scroll to search section (placeholder for future features)
        const searchSection = document.getElementById('features-section');
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

            {/* TODO: Add TileStories sections:
                - Problem Statement (Why AR for museums?)
                - Grande Panorama de Lisboa showcase
                - AR Demo/Screenshots
                - Historical epochs overview
                - Call to action
            */}

        </>
    );
}
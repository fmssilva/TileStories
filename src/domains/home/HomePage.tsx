/**
 * HomePage Component - TileStories Landing Page
 */

import HeroSection from './HeroSection';
import ProblemStatement from './ProblemStatement';
import PanoramaShowcase from './PanoramaShowcase';

export function HomePage() {
    const handleScrollToSearch = () => {
        // Scroll to features section
        const featuresSection = document.getElementById('features-section');
        if (featuresSection) {
            featuresSection.scrollIntoView({
                behavior: 'smooth',
                block: 'start',
                inline: 'nearest'
            });
        }
    };

    return (
        <>
            {/* Hero Section - Main entry point */}
            <HeroSection onScrollToSearch={handleScrollToSearch} />

            {/* Problem Statement - Why AR for museums? */}
            <ProblemStatement />

            {/* Grande Panorama Showcase - Historical significance */}
            <PanoramaShowcase />

            {/* TODO: Add remaining TileStories sections:
                - AR Demo/Screenshots
                - Historical epochs overview (4 time periods)
                - Features summary (150+ buildings, etc.)
                - Call to action
            */}

        </>
    );
}
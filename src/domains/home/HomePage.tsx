/**
 * HomePage Component - TileStories Landing Page
 */

import HeroSection from './HeroSection';
import ProblemStatement from './ProblemStatement';
import PanoramaShowcase from './PanoramaShowcase';
import ARDemoSection from './ARDemoSection';
import EpochsSection from './EpochsSection';
import FeaturesSection from './FeaturesSection';
import SupportSection from './SupportSection';
import CTASection from './CTASection';
import { DemoSticky } from '@/layout_and_navigation/sticky/components/DemoSticky';

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

            {/* TEMPORARY: Demo Sticky System Testing */}
            <DemoSticky />

            {/* Problem Statement - Why AR for museums? */}
            <ProblemStatement />

            {/* Grande Panorama Showcase - Historical significance */}
            <PanoramaShowcase />

            {/* AR Demo Screenshots - See the app in action */}
            <ARDemoSection />

            {/* Historical Epochs - 4 time periods */}
            <EpochsSection />

            {/* Features Summary - Comprehensive capabilities */}
            <FeaturesSection />

            {/* Support Section - Help fund the project */}
            <SupportSection />

            {/* Call to Action - Download, visit, subscribe */}
            <CTASection />
        </>
    );
}
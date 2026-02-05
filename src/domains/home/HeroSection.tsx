/**
 * HeroSection Component - TileStories Landing Page Hero
 * 
 * Purpose: Capture visitor attention with compelling visuals and clear value proposition
 * Target: Museum visitors, tourists, students researching Lisbon history
 * 
 * SEO: H1 optimized for "AR museum Lisbon", "Grande Panorama de Lisboa"
 * Design: Azulejo-inspired gradient overlay with responsive typography
 */

import React, { useEffect, useState } from 'react';
import heroImage from './hero_img.png';
import { useInlineTranslation } from '@/utils/language';
import { Button } from '@/components/ui';

interface HeroSectionProps {
    onScrollToSearch?: () => void;
}

const HeroSection: React.FC<HeroSectionProps> = ({ onScrollToSearch }) => {
    const t = useInlineTranslation;
    const [isLoaded, setIsLoaded] = useState(false);

    useEffect(() => {
        // Trigger animation after component mounts
        setIsLoaded(true);
    }, []);

    // Smooth scroll to features section
    const handleScrollToFeatures = () => {
        if (onScrollToSearch) {
            onScrollToSearch();
        } else {
            const featuresSection = document.getElementById('features-section');
            if (featuresSection) {
                featuresSection.scrollIntoView({
                    behavior: 'smooth',
                    block: 'start',
                    inline: 'nearest'
                });
            }
        }
    };

    return (
        <section
            id="hero"
            aria-labelledby="hero-heading"
            className="relative overflow-hidden min-h-screen flex items-center justify-center"
        >
            {/* Background Image with Azulejo Gradient Overlay */}
            <div className="absolute inset-0">
                {/* Hero Background Image */}
                <img
                    src={heroImage}
                    alt={t(
                        { pt: 'Grande Panorama de Lisboa - Museu Nacional do Azulejo', en: 'Grande Panorama de Lisboa - National Azulejo Museum' }
                    )}
                    className="w-full h-full object-cover object-center"
                    loading="eager"
                    width={1920}
                    height={1080}
                />

                {/* Azulejo-inspired gradient overlay - BUILD_PLAN spec */}
                <div
                    className="absolute inset-0"
                    style={{
                        background: `linear-gradient(
                            180deg, 
                            rgba(60, 94, 149, 0.85) 0%,
                            rgba(80, 129, 182, 0.75) 50%,
                            rgba(212, 175, 55, 0.3) 100%
                        )`
                    }}
                />
            </div>

            {/* Main Content */}
            <div className="relative w-full max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-16 sm:py-20 lg:py-24">
                <div className="max-w-4xl mx-auto text-center">

                    {/* H1: Primary SEO Target - "Explore Lisbon's Lost Skyline Through AR" */}
                    <h1
                        id="hero-heading"
                        className={`text-4xl sm:text-5xl md:text-6xl lg:text-7xl
                            font-extrabold mb-6 tracking-tight leading-tight 
                            text-white transition-all duration-700 delay-100 
                            ${isLoaded ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-4'}`}
                        style={{ textShadow: '0 2px 4px rgba(0,0,0,0.3)' }}
                    >
                        {t(
                            { pt: 'Explore o Horizonte Perdido de Lisboa Através de AR', en: 'Explore Lisbon\'s Lost Skyline Through AR' }
                        )}
                    </h1>

                    {/* Subheadline */}
                    <p
                        className={`text-lg sm:text-xl md:text-2xl lg:text-3xl
                            text-azulejo-ivory-300 leading-relaxed mb-4 max-w-3xl mx-auto
                            transition-all duration-700 delay-200
                            ${isLoaded ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-4'}`}
                    >
                        {t(
                            {
                                pt: 'Experimente o Grande Panorama de Lisboa como nunca antes. Aponte o seu telemóvel para esta obra-prima do século XVIII e veja a história ganhar vida.',
                                en: 'Experience the Grande Panorama de Lisboa like never before. Point your phone at this 18th-century masterpiece and watch history come alive.'
                            }
                        )}
                    </p>

                    {/* Supporting Details */}
                    <p
                        className={`text-sm sm:text-base md:text-lg
                            text-azulejo-ivory-50 opacity-90 mb-8 max-w-2xl mx-auto
                            transition-all duration-700 delay-300
                            ${isLoaded ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-4'}`}
                    >
                        {t(
                            {
                                pt: '150+ edifícios · 4 épocas históricas · Simulação do terramoto de 1755',
                                en: '150+ buildings · 4 historical epochs · 1755 earthquake simulation'
                            }
                        )}
                    </p>

                    {/* CTA Button */}
                    <div
                        className={`flex flex-col sm:flex-row gap-4 justify-center items-center
                            transition-all duration-700 delay-400
                            ${isLoaded ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-4'}`}
                    >
                        <Button
                            onClick={handleScrollToFeatures}
                            variant="primary"
                            size="lg"
                            className="bg-azulejo-blue-500 hover:bg-azulejo-blue-600 text-white 
                                px-8 py-4 text-lg font-semibold rounded-lg shadow-lg 
                                hover:shadow-xl transition-all duration-300 hover:scale-105"
                        >
                            {t(
                                { pt: 'Inicie a Sua Jornada AR', en: 'Start Your AR Journey' }
                            )}
                        </Button>

                        <Button
                            onClick={handleScrollToFeatures}
                            variant="secondary"
                            size="lg"
                            className="bg-white/20 hover:bg-white/30 backdrop-blur-sm text-white 
                                border-2 border-white/50 px-8 py-4 text-lg font-semibold rounded-lg
                                hover:border-white transition-all duration-300"
                        >
                            {t(
                                { pt: 'Saber Mais', en: 'Learn More' }
                            )}
                        </Button>
                    </div>
                </div>
            </div>

            {/* Scroll Indicator */}
            <button
                onClick={handleScrollToFeatures}
                className="absolute bottom-8 left-1/2 -translate-x-1/2 
                    animate-bounce opacity-70 hover:opacity-100 transition-all duration-300 
                    hover:scale-110 focus:outline-none focus:ring-2 focus:ring-azulejo-gold-500 
                    focus:ring-offset-2 rounded-lg p-2 group"
                aria-label={t(
                    { pt: 'Rolar para baixo', en: 'Scroll down' }
                )}
            >
                <div className="flex flex-col items-center gap-2">
                    <span className="text-sm text-white font-medium">
                        {t(
                            { pt: 'Explorar', en: 'Explore' }
                        )}
                    </span>
                    <svg
                        className="w-6 h-6 text-white transition-colors"
                        fill="none"
                        stroke="currentColor"
                        viewBox="0 0 24 24"
                    >
                        <path
                            strokeLinecap="round"
                            strokeLinejoin="round"
                            strokeWidth={2}
                            d="M19 14l-7 7m0 0l-7-7m7 7V3"
                        />
                    </svg>
                </div>
            </button>

            {/* Schema.org Structured Data for SEO */}
            <script
                type="application/ld+json"
                dangerouslySetInnerHTML={{
                    __html: JSON.stringify({
                        "@context": "https://schema.org",
                        "@type": "WebApplication",
                        "name": "TileStories",
                        "description": "AR app for exploring the Grande Panorama de Lisboa at Museu Nacional do Azulejo. 150+ buildings, 4 historical epochs, 1755 earthquake simulation.",
                        "applicationCategory": "EducationalApplication",
                        "operatingSystem": "iOS, Android",
                        "offers": {
                            "@type": "Offer",
                            "price": "0",
                            "priceCurrency": "EUR"
                        },
                        "about": {
                            "@type": "Museum",
                            "name": "Museu Nacional do Azulejo",
                            "address": {
                                "@type": "PostalAddress",
                                "streetAddress": "Rua da Madre de Deus, 4",
                                "addressLocality": "Lisbon",
                                "postalCode": "1900-312",
                                "addressCountry": "PT"
                            }
                        }
                    }, null, 2)
                }}
            />
        </section>
    );
};

export default HeroSection;

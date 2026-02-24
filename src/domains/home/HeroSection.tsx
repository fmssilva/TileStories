/**
 * HeroSection Component - TileStories Landing Page Hero
 * 
 * Purpose: Capture visitor attention with compelling visuals and clear value proposition
 * Target: Museum visitors, tourists, students researching Lisbon history
 * 
 * SEO: H1 optimized for "AR museum Lisbon", "Grande Panorama de Lisboa"
 * Design: Azulejo-inspired gradient overlay with responsive typography
 * Phase 5: Subtle parallax effect on background for depth and polish
 */

import React, { useEffect, useState } from 'react';
import heroImage from './hero_img.png';
import { useInlineTranslation, useLanguage } from '@/utils/language';
import { useParallax, LAYOUT } from '@/design';
import { Button, VideoModal, ComingSoonBadge } from '@/components/ui';

interface HeroSectionProps {
    onScrollToSearch?: () => void;
}

const HeroSection: React.FC<HeroSectionProps> = () => {
    const t = useInlineTranslation;
    const { language } = useLanguage();
    const [isLoaded, setIsLoaded] = useState(false);
    const [showVideoModal, setShowVideoModal] = useState(false);

    // Subtle parallax effect on background (Phase 5: Polish & Performance)
    const parallaxStyle = useParallax({ speed: 0.3 }); // Slower than scroll for depth

    useEffect(() => {
        // Trigger animation after component mounts
        setIsLoaded(true);
    }, []);

    // Scroll to AR Demo section
    const handleScrollToARDemo = () => {
        const arDemoSection = document.getElementById('ar-demo-heading')?.parentElement;
        if (arDemoSection) {
            arDemoSection.scrollIntoView({
                behavior: 'smooth',
                block: 'start',
                inline: 'nearest'
            });
        }
    };

    // Scroll to Problem Statement (Why AR?) section
    const handleScrollToProblemStatement = () => {
        const problemSection = document.getElementById('problem-statement');
        if (problemSection) {
            problemSection.scrollIntoView({
                behavior: 'smooth',
                block: 'start',
                inline: 'nearest'
            });
        }
    };

    return (
        <section
            id="hero"
            aria-labelledby="hero-heading"
            className="relative overflow-hidden flex items-center justify-center"
            style={{
                minHeight: `calc(100vh - ${LAYOUT.HEADER_HEIGHT}px)`,
                marginTop: `${LAYOUT.HEADER_HEIGHT}px`
            }}
        >
            {/* Background Image with Azulejo Gradient Overlay */}
            <div className="absolute inset-0">
                {/* Hero Background Image with Parallax Effect (Phase 5) */}
                <img
                    src={heroImage}
                    alt={t(
                        { pt: 'Grande Panorama de Lisboa - Museu Nacional do Azulejo', en: 'Grande Panorama de Lisboa - National Azulejo Museum' }
                    )}
                    className="w-full h-full object-cover object-center"
                    loading="eager"
                    width={1920}
                    height={1080}
                    style={parallaxStyle}
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

                    {/* Logo with Name - Bilingual with Coming Soon Badge */}
                    <div className={`mb-8 flex justify-center transition-all duration-700 ${isLoaded ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-4'}`}>
                        <div className="relative inline-block">
                            {/* Logo Image - switches based on language */}
                            <img
                                src={language === 'pt' ? '/images/Logo_with_name.png' : '/images/Logo_with_name_eng.png'}
                                alt={language === 'pt' ? 'TileStories - Explorando o Grande Panorama de Lisboa' : 'TileStories - Exploring the Grande Panorama de Lisboa'}
                                className="h-24 w-auto sm:h-32 md:h-40 drop-shadow-2xl"
                            />

                            {/* Coming Soon Badge - positioned in top-right corner, more outside with slight overlap */}
                            <div className="absolute bottom-[140%] left-[118%] translate-x-1/4 -translate-y-1/4 p-1">
                                <ComingSoonBadge
                                    variant="corner"
                                    launchText={t({ pt: 'Verão 2026', en: 'Summer 2026' })}
                                    className="scale-[1.4] sm:scale-[0.9] lg:scale-[1.15] origin-bottom-left"
                                />
                            </div>
                        </div>
                    </div>

                    {/* H1: Primary SEO Target - "Explore Lisbon's Lost Skyline Through AR" */}
                    <h1
                        id="hero-heading"
                        className={`text-4xl sm:text-5xl md:text-6xl lg:text-7xl
                            font-extrabold mb-4 tracking-tight leading-tight 
                            text-white transition-all duration-700 delay-100 
                            ${isLoaded ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-4'}`}
                        style={{ textShadow: '0 2px 4px rgba(0,0,0,0.3)' }}
                    >
                        {t(
                            { pt: 'Explore o Horizonte Perdido de Lisboa Através de AR', en: 'Explore Lisbon\'s Lost Skyline Through AR' }
                        )}
                    </h1>

                    {/* Coming Soon Inline Badge */}
                    <div
                        className={`flex justify-center mb-6 transition-all duration-700 delay-150
                            ${isLoaded ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-4'}`}
                    >
                        <ComingSoonBadge
                            variant="inline"
                            launchText={t({ pt: 'Lançamento Verão 2026', en: 'Launching Summer 2026' })}
                        />
                    </div>

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

                    {/* CTA Buttons - Enhanced with vibrant colors and animations */}
                    <div
                        className={`flex flex-col sm:flex-row gap-4 justify-center items-center
                            transition-all duration-700 delay-400
                            ${isLoaded ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-4'}`}
                    >
                        <button
                            onClick={handleScrollToARDemo}
                            className="group relative overflow-hidden bg-gradient-to-r from-azulejo-blue-500 to-azulejo-cobalt-500 
                                hover:from-azulejo-blue-600 hover:to-azulejo-cobalt-600 
                                text-white px-8 py-4 text-lg font-bold rounded-xl shadow-2xl 
                                hover:shadow-azulejo-blue-500/50 transition-all duration-300 
                                hover:scale-110 hover:-translate-y-1 
                                border-2 border-white/20 backdrop-blur-sm
                                h-16 flex items-center justify-center"
                        >
                            {/* Shine effect */}
                            <div className="absolute inset-0 -translate-x-full group-hover:translate-x-full 
                                         transition-transform duration-700 ease-out
                                         bg-gradient-to-r from-transparent via-white/30 to-transparent" />
                            <span className="relative z-10">
                                {t(
                                    { pt: 'Inicie a Sua Jornada AR', en: 'Start Your AR Journey' }
                                )}
                            </span>
                        </button>

                        <button
                            onClick={() => setShowVideoModal(true)}
                            className="group relative overflow-hidden bg-gradient-to-r from-azulejo-gold-500 to-yellow-500 
                                hover:from-azulejo-gold-600 hover:to-yellow-600 
                                text-white px-8 py-4 text-lg font-bold rounded-xl shadow-2xl 
                                hover:shadow-azulejo-gold-500/50 transition-all duration-300 
                                hover:scale-110 hover:-translate-y-1
                                border-2 border-white/20 backdrop-blur-sm
                                h-16 flex items-center justify-center gap-2"
                        >
                            {/* Shine effect */}
                            <div className="absolute inset-0 -translate-x-full group-hover:translate-x-full 
                                         transition-transform duration-700 ease-out
                                         bg-gradient-to-r from-transparent via-white/30 to-transparent" />
                            <svg
                                className="w-6 h-6 drop-shadow-lg relative z-10"
                                fill="currentColor"
                                viewBox="0 0 20 20"
                            >
                                <path d="M6.3 2.841A1.5 1.5 0 004 4.11V15.89a1.5 1.5 0 002.3 1.269l9.344-5.89a1.5 1.5 0 000-2.538L6.3 2.84z" />
                            </svg>
                            <span className="relative z-10">
                                {t(
                                    { pt: 'Ver Demonstração', en: 'Watch Demo' }
                                )}
                            </span>
                        </button>

                        <Button
                            onClick={handleScrollToProblemStatement}
                            variant="secondary"
                            size="lg"
                            className="bg-white/10 hover:bg-white/20 backdrop-blur-md text-white 
                                border-2 border-white/70 hover:border-white px-8 py-4 text-lg font-bold rounded-xl
                                shadow-xl hover:shadow-2xl transition-all duration-300
                                hover:scale-110 hover:-translate-y-1
                                h-16"
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
                onClick={handleScrollToProblemStatement}
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
                        "@graph": [
                            {
                                "@type": "WebApplication",
                                "@id": "https://tilestories.app/#webapp",
                                "name": "TileStories",
                                "alternateName": "TileStories AR",
                                "description": "Augmented reality app for exploring the Grande Panorama de Lisboa at Museu Nacional do Azulejo. Discover 150+ buildings across 4 historical epochs with interactive 1755 earthquake simulation.",
                                "url": "https://tilestories.app",
                                "applicationCategory": "EducationalApplication",
                                "operatingSystem": "Web, iOS, Android",
                                "browserRequirements": "Requires JavaScript. Requires HTML5.",
                                "offers": {
                                    "@type": "Offer",
                                    "price": "0",
                                    "priceCurrency": "EUR",
                                    "availability": "https://schema.org/InStock"
                                },
                                "featureList": [
                                    "Augmented Reality Experience",
                                    "150+ Historical Buildings",
                                    "4 Historical Epochs (1500-1755)",
                                    "1755 Earthquake Simulation",
                                    "Multilingual Support (PT/EN)",
                                    "Interactive 3D Models"
                                ],
                                "inLanguage": ["pt-PT", "en-US"],
                                "author": {
                                    "@type": "Organization",
                                    "@id": "https://tilestories.app/#organization",
                                    "name": "TileStories - FCT NOVA Thesis Project"
                                }
                            },
                            {
                                "@type": "Museum",
                                "@id": "https://www.museudoazulejo.gov.pt/#museum",
                                "name": "Museu Nacional do Azulejo",
                                "alternateName": "National Azulejo Museum",
                                "url": "https://www.museudoazulejo.gov.pt",
                                "description": "The National Azulejo Museum showcases the history of Portuguese decorative tiles from the 15th century to the present, including the iconic 23-meter Grande Panorama de Lisboa.",
                                "address": {
                                    "@type": "PostalAddress",
                                    "streetAddress": "Rua da Madre de Deus, 4",
                                    "addressLocality": "Lisbon",
                                    "addressRegion": "Lisboa",
                                    "postalCode": "1900-312",
                                    "addressCountry": "PT"
                                },
                                "geo": {
                                    "@type": "GeoCoordinates",
                                    "latitude": "38.7339",
                                    "longitude": "-9.1016"
                                },
                                "telephone": "+351-218-100-340",
                                "openingHoursSpecification": [
                                    {
                                        "@type": "OpeningHoursSpecification",
                                        "dayOfWeek": ["Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"],
                                        "opens": "10:00",
                                        "closes": "18:00"
                                    }
                                ],
                                "hasMap": "https://www.google.com/maps/place/Museu+Nacional+do+Azulejo",
                                "touristType": ["Families", "Students", "Art Enthusiasts", "History Buffs"]
                            },
                            {
                                "@type": "TouristAttraction",
                                "@id": "https://tilestories.app/#attraction",
                                "name": "Grande Panorama de Lisboa AR Experience",
                                "description": "Augmented reality experience of Lisbon's pre-1755 earthquake skyline through the 23-meter Grande Panorama de Lisboa azulejo panel. Explore 150+ buildings across 4 centuries of history.",
                                "isAccessibleForFree": true,
                                "publicAccess": true,
                                "touristType": ["Families", "Students", "Technology Enthusiasts", "History Buffs"],
                                "availableLanguage": ["Portuguese", "English"],
                                "containedInPlace": {
                                    "@id": "https://www.museudoazulejo.gov.pt/#museum"
                                }
                            }
                        ]
                    }, null, 2)
                }}
            />

            {/* Video Modal - Phase 3 - AR demo video */}
            <VideoModal
                isOpen={showVideoModal}
                onClose={() => setShowVideoModal(false)}
                videoUrl="/videos/Lisbon_AR_video.mp4"
                title={t({ pt: 'Demonstração AR - TileStories', en: 'AR Demo - TileStories' })}
                description={t({
                    pt: 'Veja como a realidade aumentada traz o Grande Panorama de Lisboa à vida.',
                    en: 'See how augmented reality brings the Grande Panorama de Lisboa to life.',
                })}
            />
        </section>
    );
};

export default HeroSection;

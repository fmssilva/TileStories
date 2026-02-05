import React, { useEffect, useState } from 'react';
import heroImage from './hero_img.png';
import { useInlineTranslation } from '@/utils/language';
import { Logo } from '@/branding';

// Global feedback trigger interface
declare global {
    interface Window {
        triggerFeedback?: {
            quick: (context?: string) => void;
            nps: (context?: string) => void;
        };
    }
}

type SearchMethod = 'exploration' | 'symptoms';

interface HeroSectionProps {
    onScrollToSearch?: (targetMethod?: SearchMethod) => void;
}

const HeroSection: React.FC<HeroSectionProps> = ({ onScrollToSearch }) => {
    const [isLoaded, setIsLoaded] = useState(false);

    useEffect(() => {
        // Trigger animation after component mounts
        setIsLoaded(true);
    }, []);

    // Smooth scroll to search tabs section
    const handleScrollToSearch = () => {
        if (onScrollToSearch) {
            onScrollToSearch();
        } else {
            // Fallback to default behavior
            const searchSection = document.getElementById('search-tabs-section');
            if (searchSection) {
                searchSection.scrollIntoView({
                    behavior: 'smooth',
                    block: 'start',
                    inline: 'nearest'
                });
            }
        }
    };

    // Scroll to search tabs section with AI Diagnosis tab active
    const handleScrollToAIDiagnosis = () => {
        if (onScrollToSearch) {
            onScrollToSearch('symptoms');
        } else {
            // Fallback to default behavior
            const searchSection = document.getElementById('search-tabs-section');
            if (searchSection) {
                searchSection.scrollIntoView({
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
            className="relative overflow-hidden bg-gray-50 dark:bg-gray-900 
              min-h-screen 
              sm:min-h-[calc(100vh-4rem)] 
              md:min-h-[calc(100vh-5rem)] 
              lg:min-h-[calc(100vh-6rem)] 
              flex items-center justify-center"
        >
            {/* Background Image with Overlay - SEO Optimized */}
            <div className="absolute inset-0">
                {/* Background Image - Keyword-rich alt text */}
                <img
                    src={heroImage}
                    alt="Museu do Azulejo — TileStories AR hero image"
                    className="w-full h-full object-cover object-center"
                    loading="eager"
                    width={1920}
                    height={1080}
                />

                {/* Reduced opacity gradient overlay for better image visibility */}
                <div className="absolute inset-0 bg-gradient-to-r 
                      from-white/85 via-white/75 to-white/60 
                      dark:from-gray-900/85 dark:via-gray-900/75 dark:to-gray-900/60"></div>

                {/* Subtle animated gradient accents with brand colors - Reduced for performance */}
                <div className="absolute inset-0 opacity-10 motion-safe:animate-pulse-slow">
                    <div className="absolute top-1/4 left-1/4 w-96 h-96 bg-[#1976D2]/40 rounded-full blur-3xl"></div>
                    <div className="absolute bottom-1/4 right-1/4 w-96 h-96 bg-[#4CAF50]/40 rounded-full blur-3xl" style={{ animationDelay: '1s' }}></div>
                </div>
            </div>

            {/* Main Content */}
            <div className="relative w-full max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 
                  py-8 sm:py-12 md:py-16 lg:py-20 xl:py-24
                  flex flex-col justify-center min-h-full">
                <div className="max-w-6xl mx-auto lg:mx-0 text-center lg:text-left">

                    {/* Logo and Website Name - SEO Context */}
                    <div className={`flex items-center justify-center lg:justify-start gap-3 sm:gap-4 mb-4 sm:mb-6 lg:mb-8 transition-all duration-700 ${isLoaded ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-4'}`}>
                        <Logo
                            variant="icon"
                            size="xl"
                            className="w-12 h-12 sm:w-16 sm:h-16 md:w-20 md:h-20 lg:w-24 lg:h-24"
                        />
                        <h2 className="text-3xl sm:text-4xl md:text-5xl lg:text-6xl xl:text-7xl font-extrabold">
                            <span className="text-[#4CAF50]">Tile</span>
                            <span className="text-[#1976D2]">Stories</span>
                        </h2>
                    </div>

                    {/* SEO-Optimized H1 - Primary Keywords */}
                    <h1
                        id="hero-heading"
                        className={`text-xl sm:text-2xl md:text-3xl lg:text-4xl xl:text-5xl 2xl:text-6xl
                          font-bold mb-4 sm:mb-6 lg:mb-8 tracking-tight leading-tight 
                          text-gray-900 dark:text-gray-100 transition-all duration-700 delay-100 
                          max-w-none ${isLoaded ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-4'}`}
                    >
                        {useInlineTranslation(
                            'TileStories — experiências AR para o Museu do Azulejo em Lisboa',
                            'TileStories — AR experiences for the Museu do Azulejo in Lisbon'
                        )}
                    </h1>

                    {/* SEO-Optimized Subtitle + Context Paragraph */}
                    <div className="space-y-4 mb-6 sm:mb-8 lg:mb-10">
                        {/* Main Subtitle */}
                        <p className={`text-base sm:text-lg md:text-xl lg:text-2xl xl:text-3xl 2xl:text-4xl
                              text-gray-700 dark:text-gray-300 leading-relaxed font-medium transition-all duration-700 delay-200 
                              max-w-none ${isLoaded ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-4'}`}>
                            {useInlineTranslation(
                                'Explore azulejos, histórias e exposições com realidade aumentada no Museu do Azulejo.',
                                'Explore tiles, stories and exhibits with augmented reality at the Museu do Azulejo.'
                            )}
                            <span className="block mt-3 text-[#1976D2] dark:text-blue-400 font-semibold text-lg sm:text-xl md:text-2xl">
                                {useInlineTranslation(
                                    'Guias interativos AR para cada azulejo.',
                                    'Interactive AR guides for each tile.'
                                )}
                            </span>
                        </p>

                    </div>

                    {/* Trust Indicators - SEO Enhanced with Keywords */}
                    <div className={`flex flex-col sm:flex-row flex-wrap justify-center lg:justify-start 
                          items-stretch gap-3 sm:gap-4 lg:gap-6 xl:gap-8 transition-all duration-700 delay-400 
                          max-w-none ${isLoaded ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-4'}`}>

                        {/* Savings Card - Price-focused keywords */}
                        <button
                            onClick={handleScrollToSearch}
                            className="flex items-center gap-2 sm:gap-3 px-3 sm:px-4 lg:px-6 xl:px-8 py-2 sm:py-3 lg:py-4
                              bg-white/90 dark:bg-gray-800/90 backdrop-blur-sm rounded-lg sm:rounded-xl lg:rounded-2xl
                              shadow-md hover:shadow-lg transition-all duration-300 hover:-translate-y-0.5 
                              w-full sm:flex-1 lg:flex-none lg:w-auto xl:w-auto min-w-0 cursor-pointer hover:bg-white dark:hover:bg-gray-700 
                              active:scale-95 transform"
                        >
                            <div className="w-8 h-8 sm:w-10 sm:h-10 lg:w-12 lg:h-12 bg-blue-100 dark:bg-blue-900/30 rounded-lg lg:rounded-xl
                                  flex items-center justify-center text-lg sm:text-xl lg:text-2xl flex-shrink-0">
                                💰
                            </div>
                            <div className="min-w-0">
                                <div className="text-sm sm:text-base lg:text-lg xl:text-xl font-bold text-gray-900 dark:text-gray-100">
                                    {useInlineTranslation('Poupe até 60%', 'Save up to 60%')}
                                </div>
                                <div className="text-xs sm:text-sm lg:text-base xl:text-lg text-gray-600 dark:text-gray-400">
                                    {useInlineTranslation('Preços transparentes', 'Transparent pricing')}
                                </div>
                            </div>
                        </button>

                        {/* AI Diagnosis Button - Symptom checker keywords */}
                        <button
                            onClick={handleScrollToAIDiagnosis}
                            className="flex items-center gap-2 sm:gap-3 px-3 sm:px-4 lg:px-6 xl:px-8 py-2 sm:py-3 lg:py-4
                              bg-white/90 dark:bg-gray-800/90 backdrop-blur-sm rounded-lg sm:rounded-xl lg:rounded-2xl
                              shadow-md hover:shadow-lg transition-all duration-300 hover:-translate-y-0.5 
                              w-full sm:flex-1 lg:flex-none lg:w-auto xl:w-auto cursor-pointer hover:bg-white dark:hover:bg-gray-700 
                              active:scale-95 transform min-w-0"
                        >
                            <div className="w-8 h-8 sm:w-10 sm:h-10 lg:w-12 lg:h-12 bg-gradient-to-br from-purple-100 to-blue-100 dark:from-purple-900/30 dark:to-blue-900/30 rounded-lg lg:rounded-xl flex items-center justify-center text-lg sm:text-xl lg:text-2xl flex-shrink-0">
                                ✨
                            </div>
                            <div className="min-w-0">
                                <div className="text-sm sm:text-base lg:text-lg xl:text-xl font-bold text-gray-900 dark:text-gray-100">
                                    {useInlineTranslation('Diagnóstico IA', 'AI Diagnosis')}
                                </div>
                                <div className="text-xs sm:text-sm lg:text-base xl:text-lg text-gray-600 dark:text-gray-400">
                                    {useInlineTranslation('Recomendações instantâneas', 'Instant recommendations')}
                                </div>
                            </div>
                        </button>

                        {/* Reviews Button - Trust signals */}
                        <button
                            onClick={handleScrollToSearch}
                            className="flex items-center gap-2 sm:gap-3 px-3 sm:px-4 lg:px-6 xl:px-8 py-2 sm:py-3 lg:py-4
                              bg-white/90 dark:bg-gray-800/90 backdrop-blur-sm rounded-lg sm:rounded-xl lg:rounded-2xl
                              shadow-md hover:shadow-lg transition-all duration-300 hover:-translate-y-0.5 
                              w-full sm:flex-1 lg:flex-none lg:w-auto xl:w-auto group cursor-pointer min-w-0 hover:bg-white dark:hover:bg-gray-700 
                              active:scale-95 transform"
                        >
                            <div className="w-8 h-8 sm:w-10 sm:h-10 lg:w-12 lg:h-12 bg-indigo-100 dark:bg-indigo-900/30 rounded-lg lg:rounded-xl
                                  flex items-center justify-center text-lg sm:text-xl lg:text-2xl flex-shrink-0 
                                  group-hover:bg-indigo-200 dark:group-hover:bg-indigo-800/50 transition-colors">
                                ⭐
                            </div>
                            <div className="min-w-0">
                                <div className="text-sm sm:text-base lg:text-lg xl:text-xl font-bold text-gray-900 dark:text-gray-100 
                                      group-hover:text-indigo-700 dark:group-hover:text-indigo-300 transition-colors">
                                    {useInlineTranslation('Avaliações Verificadas', 'Verified Reviews')}
                                </div>
                                <div className="text-xs sm:text-sm lg:text-base text-gray-600 dark:text-gray-400 
                                      group-hover:text-indigo-600 dark:group-hover:text-indigo-400 transition-colors">
                                    {useInlineTranslation('Comentários reais de pacientes', 'Real patient feedback')}
                                </div>
                            </div>
                        </button>
                    </div>
                </div>
            </div>

            {/* CRITICAL: Schema.org Structured Data for Google/AI */}
            <script
                type="application/ld+json"
                dangerouslySetInnerHTML={{
                    __html: JSON.stringify({
                        "@context": "https://schema.org",
                        "@type": ["WebApplication", "TouristAttraction"],
                        "name": "TileStories - Museu do Azulejo AR",
                        "description": "TileStories brings augmented reality storytelling to the Museu do Azulejo in Lisbon. Explore azulejo history with guided AR experiences.",
                        "url": "https://tilestories.pt",
                        "areaServed": ["Portugal", "Lisbon"],
                        "potentialAction": {
                            "@type": "SearchAction",
                            "target": {
                                "@type": "EntryPoint",
                                "urlTemplate": "https://tilestories.pt/?q={search_term_string}"
                            },
                            "query-input": "required name=search_term_string"
                        }
                    }, null, 2)
                }}
            />

            {/* Scroll indicator with click functionality */}
            <button
                onClick={handleScrollToSearch}
                className="absolute bottom-4 sm:bottom-6 lg:bottom-8 left-1/2 -translate-x-1/2 
                 animate-bounce opacity-70 hover:opacity-100 transition-all duration-300 
                 hover:scale-110 focus:outline-none focus:ring-2 focus:ring-blue-500 
                 focus:ring-offset-2 rounded-lg p-2 group"
                aria-label="Scroll to search section - Procurar dentista/tratamento"
            >
                <div className="flex flex-col items-center gap-1 sm:gap-2">
                    <span className="text-xs sm:text-sm text-gray-600 dark:text-gray-800 
                           group-hover:text-gray-800 dark:group-hover:text-gray-200 
                           font-medium transition-colors">
                        {useInlineTranslation('Procurar tratamento', 'Start Search')}
                    </span>
                    <svg className="w-5 h-5 sm:w-6 sm:h-6 lg:w-7 lg:h-7 text-gray-600 dark:text-gray-400 
                           group-hover:text-gray-800 dark:group-hover:text-gray-200 
                           transition-colors"
                        fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
                            d="M19 14l-7 7m0 0l-7-7m7 7V3" />
                    </svg>
                </div>
            </button>
        </section>
    );
};

export default HeroSection;

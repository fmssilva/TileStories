/**
 * WORK PLAN HERO SECTION
 * =======================
 * 
 * Investor-facing hero section with professional, serious tone.
 * Presents the 12-month roadmap, investment amount, and key metrics.
 * 
 * Layout: Full-width gradient background, centered content
 * Design: Azulejo blue gradient with clear typography hierarchy
 * CTA: Scroll to overview section for details
 */

import { useInlineTranslation, useLanguage } from '@/utils/language';
import { useParallax, LAYOUT } from '@/design';
import { projectMetadata } from './utils';
import { Link } from 'react-router-dom';

export function WorkPlanHero() {
    const { language } = useLanguage();

    // Parallax effect for professional depth (Phase 5)
    const parallaxStyle = useParallax({ speed: 0.5 });

    const headline = useInlineTranslation(
        'Plano de Desenvolvimento',
        'Development Plan'
    );

    const subheadline = useInlineTranslation(
        'Plano de Desenvolvimento de 12 Meses',
        '12-Month Development Plan'
    );

    const exploreButton = useInlineTranslation(
        'Explorar Detalhes',
        'Explore Details'
    );

    const contactButton = useInlineTranslation(
        'Entrar em Contato',
        'Get in Touch'
    );

    const handleScrollToOverview = () => {
        const overviewSection = document.getElementById('overview');
        if (overviewSection) {
            overviewSection.scrollIntoView({
                behavior: 'smooth',
                block: 'start',
                inline: 'nearest'
            });
        }
    };

    return (
        <section
            className="relative overflow-hidden"
            aria-labelledby="hero-heading"
            style={{
                minHeight: `calc(100vh - ${LAYOUT.HEADER_HEIGHT}px)`,
                marginTop: `${LAYOUT.HEADER_HEIGHT}px`
            }}
        >
            {/* Background Video with Parallax */}
            <div className="absolute inset-0">
                <video
                    autoPlay
                    loop
                    muted
                    playsInline
                    className="w-full h-full object-cover object-center"
                    style={parallaxStyle}
                >
                    <source src="/videos/Lisbon_AR_vide_4_views.mp4" type="video/mp4" />
                    {/* Fallback image if video fails to load */}
                    <img
                        src="/images/Lisbon_Panoram_1.png"
                        alt="Historical Lisbon Panorama"
                        className="w-full h-full object-cover object-center"
                    />
                </video>

                {/* Enhanced Gradient Overlay for better text contrast */}
                <div
                    className="absolute inset-0"
                    style={{
                        background: `linear-gradient(
                            135deg, 
                            rgba(28, 45, 72, 0.75) 0%, 
                            rgba(47, 75, 119, 0.70) 30%,
                            rgba(60, 94, 149, 0.65) 60%,
                            rgba(80, 129, 182, 0.60) 100%
                        )`
                    }}
                />

                {/* Radial gradient for center focus */}
                <div
                    className="absolute inset-0"
                    style={{
                        background: `radial-gradient(
                            ellipse at center,
                            rgba(0, 0, 0, 0) 0%,
                            rgba(0, 0, 0, 0.25) 70%,
                            rgba(0, 0, 0, 0.4) 100%
                        )`
                    }}
                />
            </div>

            {/* Content */}
            <div className="relative max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 text-center text-white 
                          py-16 sm:py-20 lg:py-28 flex flex-col justify-center"
                style={{ minHeight: `calc(100vh - ${LAYOUT.HEADER_HEIGHT}px - 8rem)` }}>

                {/* Enhanced Heading with better animation */}
                <div className="mb-8">
                    <h1
                        id="hero-heading"
                        className="text-4xl sm:text-5xl lg:text-7xl xl:text-8xl font-bold mb-6 
                                   drop-shadow-2xl tracking-tight leading-tight
                                   animate-fade-in-up"
                        style={{
                            textShadow: '0 6px 20px rgba(0, 0, 0, 0.6), 0 3px 8px rgba(0, 0, 0, 0.4), 0 1px 3px rgba(0, 0, 0, 0.3)'
                        }}
                    >
                        {headline}
                    </h1>

                    <p className="text-xl sm:text-2xl lg:text-3xl xl:text-4xl font-light mb-4 
                                 text-white/95 drop-shadow-lg max-w-4xl mx-auto leading-relaxed"
                        style={{
                            textShadow: '0 3px 12px rgba(0, 0, 0, 0.5), 0 1px 4px rgba(0, 0, 0, 0.3)'
                        }}>
                        {subheadline}
                    </p>
                </div>

                {/* Enhanced Summary Stats with better glassmorphism */}
                <div className="flex flex-wrap justify-center gap-4 sm:gap-6 text-base sm:text-lg lg:text-xl mb-14">
                    <div className="px-6 py-3.5 bg-white/20 backdrop-blur-lg rounded-full 
                                   border-2 border-white/30 shadow-xl
                                   hover:bg-white/30 hover:scale-105 hover:border-white/40
                                   transition-all duration-300 cursor-default">
                        <span className="font-bold text-azulejo-gold-300">€{projectMetadata.totalCostWithBuffer}</span>
                        <span className="font-light ml-2">investment</span>
                    </div>

                    <div className="hidden sm:flex items-center text-white/30 text-3xl font-thin">•</div>

                    <div className="px-6 py-3.5 bg-white/20 backdrop-blur-lg rounded-full 
                                   border-2 border-white/30 shadow-xl
                                   hover:bg-white/30 hover:scale-105 hover:border-white/40
                                   transition-all duration-300 cursor-default">
                        <span className="font-bold text-azulejo-gold-300">{projectMetadata.phaseCount}</span>
                        <span className="font-light ml-2">phases</span>
                    </div>

                    <div className="hidden sm:flex items-center text-white/30 text-3xl font-thin">•</div>

                    <div className="px-6 py-3.5 bg-white/20 backdrop-blur-lg rounded-full 
                                   border-2 border-white/30 shadow-xl
                                   hover:bg-white/30 hover:scale-105 hover:border-white/40
                                   transition-all duration-300 cursor-default">
                        <span className="font-light">MVP in</span>
                        <span className="font-bold text-azulejo-gold-300 ml-2">{projectMetadata.mvpMonth}</span>
                        <span className="font-light ml-2">months</span>
                    </div>
                </div>

                {/* Enhanced CTA Buttons with consistent sizing */}
                <div className="flex flex-col sm:flex-row gap-5 justify-center items-center">
                    <button
                        onClick={handleScrollToOverview}
                        className="group inline-flex items-center justify-center gap-3 
                                 px-10 py-4 min-w-[220px]
                                 bg-white/95 hover:bg-white text-azulejo-blue-800
                                 font-bold text-lg rounded-xl
                                 shadow-2xl hover:shadow-[0_20px_50px_rgba(255,255,255,0.3)]
                                 transform hover:scale-105 active:scale-100
                                 transition-all duration-300
                                 border-2 border-white/40 hover:border-white"
                    >
                        <span>{exploreButton}</span>
                        <svg
                            className="w-5 h-5 transform transition-transform duration-300 group-hover:translate-y-1"
                            fill="none"
                            stroke="currentColor"
                            viewBox="0 0 24 24"
                            aria-hidden="true"
                        >
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={3} d="M19 9l-7 7-7-7" />
                        </svg>
                    </button>

                    <Link
                        to="/contact"
                        className="group inline-flex items-center justify-center gap-3 
                                 px-10 py-4 min-w-[220px]
                                 bg-gradient-to-r from-azulejo-gold-500 to-azulejo-gold-600
                                 hover:from-azulejo-gold-400 hover:to-azulejo-gold-500
                                 text-white font-bold text-lg rounded-xl
                                 shadow-2xl hover:shadow-[0_20px_50px_rgba(212,175,55,0.4)]
                                 transform hover:scale-105 active:scale-100
                                 transition-all duration-300
                                 border-2 border-azulejo-gold-400 hover:border-azulejo-gold-300"
                    >
                        <svg
                            className="w-5 h-5"
                            fill="none"
                            stroke="currentColor"
                            viewBox="0 0 24 24"
                            aria-hidden="true"
                        >
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
                                d="M3 8l7.89 5.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" />
                        </svg>
                        <span>{contactButton}</span>
                        <svg
                            className="w-5 h-5 transition-transform duration-300 group-hover:translate-x-1"
                            fill="none"
                            stroke="currentColor"
                            viewBox="0 0 24 24"
                            aria-hidden="true"
                        >
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={3} d="M9 5l7 7-7 7" />
                        </svg>
                    </Link>
                </div>

            </div>

            {/* Schema.org Structured Data for SEO */}
            <script
                type="application/ld+json"
                dangerouslySetInnerHTML={{
                    __html: JSON.stringify({
                        "@context": "https://schema.org",
                        "@type": "Article",
                        "headline": language === 'pt'
                            ? "Plano de Desenvolvimento TileStories: 12 Meses para a Excelência"
                            : "TileStories Development Plan: 12 Months to Excellence",
                        "description": language === 'pt'
                            ? "Plano detalhado de 12 meses com 4 fases de desenvolvimento para criar a experiência AR do Grande Panorama de Lisboa. Investimento de €3,500, MVP em 6 meses, entrega faseada minimiza riscos."
                            : "Detailed 12-month plan with 4 development phases to create the AR experience for Lisbon's Grande Panorama. €3,500 investment, MVP in 6 months, phased delivery minimizes risks.",
                        "author": {
                            "@type": "Organization",
                            "name": "TileStories",
                            "url": "https://tilestories.app"
                        },
                        "publisher": {
                            "@type": "Organization",
                            "name": "TileStories - FCT NOVA Thesis Project",
                            "logo": {
                                "@type": "ImageObject",
                                "url": "https://tilestories.app/Logo.png"
                            }
                        },
                        "datePublished": "2025-02-06",
                        "dateModified": "2025-02-06",
                        "inLanguage": language === 'pt' ? "pt-PT" : "en-US",
                        "keywords": [
                            "TileStories development plan",
                            "AR museum investment",
                            "cultural heritage technology",
                            "Grande Panorama de Lisboa",
                            "digital museum innovation",
                            "phased software development",
                            "MVP cultural app"
                        ],
                        "about": {
                            "@type": "SoftwareApplication",
                            "name": "TileStories AR Platform",
                            "applicationCategory": "EducationalApplication",
                            "operatingSystem": "Web, iOS, Android",
                            "offers": {
                                "@type": "Offer",
                                "price": "0",
                                "priceCurrency": "EUR"
                            }
                        },
                        "mainEntity": {
                            "@type": "PlanAction",
                            "name": language === 'pt'
                                ? "Desenvolvimento de Plataforma AR para Museus"
                                : "AR Platform Development for Museums",
                            "description": language === 'pt'
                                ? "4 fases: MVP (€541), Core (€617), WOW (€1,022), Excellence (€967). Total: €3,147 + margem €314."
                                : "4 phases: MVP (€541), Core (€617), WOW (€1,022), Excellence (€967). Total: €3,147 + buffer €314.",
                            "instrument": {
                                "@type": "MonetaryAmount",
                                "value": projectMetadata.totalCostWithBuffer,
                                "currency": "EUR"
                            },
                            "expectedDuration": {
                                "@type": "Duration",
                                "value": "P12M"
                            }
                        },
                        "hasPart": [
                            {
                                "@type": "HowToStep",
                                "name": language === 'pt' ? "Fase 1: MVP" : "Phase 1: MVP",
                                "text": language === 'pt'
                                    ? "Fundação da plataforma com navegação básica, perfis de usuário, mapa interativo. Meses 1-3, €541."
                                    : "Platform foundation with basic navigation, user profiles, interactive map. Months 1-3, €541.",
                                "position": 1
                            },
                            {
                                "@type": "HowToStep",
                                "name": language === 'pt' ? "Fase 2: Core" : "Phase 2: Core",
                                "text": language === 'pt'
                                    ? "Recursos essenciais com guias de áudio, sistema de conquistas, analytics. Meses 4-6, €617."
                                    : "Core features with audio guides, achievement system, analytics. Months 4-6, €617.",
                                "position": 2
                            },
                            {
                                "@type": "HowToStep",
                                "name": language === 'pt' ? "Fase 3: WOW" : "Phase 3: WOW",
                                "text": language === 'pt'
                                    ? "Recursos inovadores com simulação de terramoto, tour 360°, integração de clima. Meses 7-9, €1,022."
                                    : "Innovative features with earthquake simulation, 360° tour, weather integration. Months 7-9, €1,022.",
                                "position": 3
                            },
                            {
                                "@type": "HowToStep",
                                "name": language === 'pt' ? "Fase 4: Excelência" : "Phase 4: Excellence",
                                "text": language === 'pt'
                                    ? "Polimento e otimização com PWA, compartilhamento social, SEO avançado. Meses 10-12, €967."
                                    : "Polish and optimization with PWA, social sharing, advanced SEO. Months 10-12, €967.",
                                "position": 4
                            }
                        ]
                    })
                }}
            />
        </section>
    );
}

export default WorkPlanHero;

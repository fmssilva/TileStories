/**
 * PanoramaShowcase Component - The Grande Panorama de Lisboa
 * 
 * Purpose: Introduce the specific artifact and its historical significance
 * Content Source: Google Arts & Culture article + site_notes.md
 * 
 * Design: Enhanced 60/40 split with elegant historical aesthetic
 * SEO: H2 "The Grande Panorama de Lisboa" with rich historical keywords
 * Phase 5: Scroll-triggered animations for image and text
 */
import { useInlineTranslation } from '@/utils/language';
import { useScrollAnimation, getScrollAnimationClasses } from '@/design';
import { ImageCarousel } from '@/components/ui';

// Real museum images for the carousel
const panoramaImages = [
    '/images/real_imgs/Grand_Panorama.png',
    '/images/real_imgs/Grand_panorama_2.png',
    '/images/real_imgs/Grand_Pan_3.png',
    '/images/real_imgs/Grand_Pan_4.png',
    '/images/real_imgs/Grand_Pan_5.png',
    '/images/real_imgs/close_up.png',
    '/images/real_imgs/most_jeron.png',
    '/images/real_imgs/most_jeron_2.png',
    '/images/real_imgs/museu.png',
    '/images/real_imgs/museu_2.png',
];

export function PanoramaShowcase() {
    const t = useInlineTranslation;

    const { ref: sectionRef, isVisible } = useScrollAnimation<HTMLDivElement>({
        threshold: 0.1,
        once: true
    });

    const historicalFacts = [
        {
            icon: '📅',
            text: t({
                pt: 'Criado ~1700, mostrando Lisboa pré-terramoto (1755)',
                en: 'Created ~1700, pre-earthquake Lisbon (1755)'
            })
        },
        {
            icon: '📏',
            text: t({
                pt: '23 metros de comprimento, mostrando o rio Tejo e mais de 150 edifícios',
                en: '23 meters long, showing the Tagus River and 150+ buildings'
            })
        },
        {
            icon: '🏛️',
            text: t({
                pt: 'Localizado no Museu Nacional do Azulejo',
                en: 'Located at Museu Nacional do Azulejo'
            })
        }
    ];

    const specialFeatures = [
        {
            icon: '🎨',
            text: t({
                pt: 'Única vista panorâmica de Lisboa antes do terramoto de 1755',
                en: 'Only panoramic view of Lisbon before the 1755 earthquake'
            })
        },
        {
            icon: '🏰',
            text: t({
                pt: 'Mostra edifícios que já não existem',
                en: 'Shows buildings that no longer exist'
            })
        },
        {
            icon: '✨',
            text: t({
                pt: 'Obra-prima da arte de azulejo portuguesa',
                en: 'Masterpiece of Portuguese azulejo art'
            })
        }
    ];

    const arFeatures = [
        {
            icon: '📱',
            text: t({
                pt: 'Aponte o seu dispositivo para o panorama',
                en: 'Point your device at the panorama'
            })
        },
        {
            icon: '🏛️',
            text: t({
                pt: 'Toque nos edifícios para revelar as suas histórias',
                en: 'Tap buildings to reveal their stories'
            })
        },
        {
            icon: '⏳',
            text: t({
                pt: 'Compare 4 épocas históricas',
                en: 'Compare 4 historical epochs'
            })
        },
        {
            icon: '🌊',
            text: t({
                pt: 'Experimente a simulação do terramoto de 1755',
                en: 'Experience the 1755 earthquake simulation'
            })
        }
    ];

    return (
        <section
            ref={sectionRef}
            id="panorama-showcase"
            className="relative py-20 sm:py-24 lg:py-32 bg-gradient-to-br from-slate-900 via-slate-800 to-slate-900 dark:from-gray-950 dark:via-gray-900 dark:to-gray-950 overflow-hidden"
        >
            {/* Azulejo pattern overlay */}
            <div className="absolute inset-0 opacity-[0.03]">
                <div className="absolute inset-0" style={{
                    backgroundImage: `url("data:image/svg+xml,%3Csvg width='80' height='80' viewBox='0 0 80 80' xmlns='http://www.w3.org/2000/svg'%3E%3Cg fill='none' fill-rule='evenodd'%3E%3Cg fill='%23ffffff' fill-opacity='1'%3E%3Cpath d='M50 50c0-5.523 4.477-10 10-10s10 4.477 10 10-4.477 10-10 10c0-5.523-4.477-10-10-10zm-20 0c0 5.523-4.477 10-10 10s-10-4.477-10-10 4.477-10 10-10c0 5.523 4.477 10 10 10zM0 30c0-5.523 4.477-10 10-10s10 4.477 10 10-4.477 10-10 10-10-4.477-10-10zm60 0c0 5.523 4.477 10 10 10s10-4.477 10-10-4.477-10-10-10-10 4.477-10 10z'/%3E%3C/g%3E%3C/g%3E%3C/svg%3E")`,
                    backgroundSize: '80px 80px'
                }}></div>
            </div>

            {/* Decorative elements */}
            <div className="absolute top-0 left-0 w-96 h-96 bg-azulejo-blue-500/5 rounded-full blur-3xl"></div>
            <div className="absolute bottom-0 right-0 w-96 h-96 bg-azulejo-gold-500/5 rounded-full blur-3xl"></div>

            <div className="container mx-auto px-4 sm:px-6 lg:px-8 relative z-10">
                {/* Header */}
                <div className={`text-center mb-16 sm:mb-20 ${getScrollAnimationClasses(isVisible, 'slide-up')}`}>
                    <div className="inline-block mb-6">
                        <span className="inline-flex items-center gap-2 px-5 py-2.5 rounded-full bg-azulejo-gold-500/10 border border-azulejo-gold-500/20 text-azulejo-gold-400 text-sm font-semibold backdrop-blur-sm">
                            <svg className="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
                                <path d="M10 2a6 6 0 00-6 6v3.586l-.707.707A1 1 0 004 14h12a1 1 0 00.707-1.707L16 11.586V8a6 6 0 00-6-6zM10 18a3 3 0 01-3-3h6a3 3 0 01-3 3z" />
                            </svg>
                            {t({ pt: 'Tesouro Nacional', en: 'National Treasure' })}
                        </span>
                    </div>

                    <h2 className="text-4xl sm:text-5xl lg:text-6xl font-bold mb-6 text-white">
                        {t({ pt: 'O Grande Panorama de Lisboa', en: 'The Grande Panorama de Lisboa' })}
                    </h2>

                    <p className="text-lg sm:text-xl text-slate-300 max-w-3xl mx-auto leading-relaxed">
                        {t({
                            pt: 'Uma janela para o passado. 23 metros de azulejos que capturam Lisboa antes do grande terramoto de 1755.',
                            en: 'A window to the past. 23 meters of tiles capturing Lisbon before the great 1755 earthquake.'
                        })}
                    </p>
                </div>

                {/* Main Content Grid - Centered alignment */}
                <div className="grid grid-cols-1 lg:grid-cols-5 gap-10 lg:gap-16 items-center">
                    {/* Left Column - Image Carousel (60%) - Centered vertically */}
                    <div className={`lg:col-span-3 ${getScrollAnimationClasses(isVisible, 'slide-left')}`}
                        style={{ transitionDelay: '150ms' }}>
                        <div className="relative group">
                            {/* Glow effect */}
                            <div className="absolute -inset-1 bg-gradient-to-r from-azulejo-gold-500 via-azulejo-blue-500 to-azulejo-gold-500 rounded-3xl blur-2xl opacity-20 group-hover:opacity-30 transition-opacity duration-700"></div>

                            <div className="relative overflow-hidden rounded-2xl ring-1 ring-white/10 shadow-2xl">
                                <div className="aspect-[16/9] w-full bg-slate-950">
                                    <ImageCarousel
                                        images={panoramaImages}
                                        interval={4000}
                                        alt={t({
                                            pt: 'Grande Panorama de Lisboa - Museu Nacional do Azulejo',
                                            en: 'Grande Panorama de Lisboa - National Azulejo Museum'
                                        })}
                                        showDots={true}
                                    />
                                </div>

                                {/* Image overlay badge */}
                                <div className="absolute top-4 left-4">
                                    <span className="inline-flex items-center gap-2 px-3 py-1.5 rounded-lg bg-black/60 backdrop-blur-md text-white text-xs font-semibold border border-white/10">
                                        <span className="relative flex h-2 w-2">
                                            <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-azulejo-gold-400 opacity-75"></span>
                                            <span className="relative inline-flex rounded-full h-2 w-2 bg-azulejo-gold-500"></span>
                                        </span>
                                        {t({ pt: 'Vista Interativa', en: 'Interactive View' })}
                                    </span>
                                </div>
                            </div>

                            {/* Caption */}
                            <div className="mt-6 flex items-start gap-3 text-slate-400">
                                <svg className="w-5 h-5 mt-0.5 flex-shrink-0" fill="currentColor" viewBox="0 0 20 20">
                                    <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z" clipRule="evenodd" />
                                </svg>
                                <p className="text-sm leading-relaxed">
                                    {t({
                                        pt: 'Painel de azulejo de 23 metros por Gabriel del Barco (~1700), mostrando Lisboa pré-terramoto',
                                        en: '23-meter azulejo panel by Gabriel del Barco (~1700), showing pre-earthquake Lisbon'
                                    })}
                                </p>
                            </div>
                        </div>
                    </div>

                    {/* Right Column - Details (40%) - Centered vertically */}
                    <div className={`lg:col-span-2 space-y-10 ${getScrollAnimationClasses(isVisible, 'slide-right')}`}
                        style={{ transitionDelay: '250ms' }}>

                        {/* Historical Context */}
                        <div className="relative">
                            <div className="absolute -left-4 top-0 bottom-0 w-1 bg-gradient-to-b from-azulejo-blue-500 via-azulejo-gold-500 to-transparent rounded-full"></div>

                            <h3 className="text-2xl lg:text-3xl font-bold text-white mb-5 flex items-center gap-3">
                                <span className="flex items-center justify-center w-10 h-10 rounded-xl bg-azulejo-blue-500/20 text-azulejo-blue-400 border border-azulejo-blue-500/30">
                                    📜
                                </span>
                                {t({ pt: 'Contexto Histórico', en: 'Historical Context' })}
                            </h3>

                            <div className="space-y-3">
                                {historicalFacts.map((fact, index) => (
                                    <div key={index} className="flex items-start gap-4 group">
                                        <div className="flex-shrink-0 w-10 h-10 rounded-xl bg-slate-800/50 border border-slate-700/50 flex items-center justify-center text-xl group-hover:scale-110 group-hover:bg-slate-700/50 transition-all duration-300">
                                            {fact.icon}
                                        </div>
                                        <p className="text-slate-300 leading-snug pt-2">
                                            {fact.text}
                                        </p>
                                    </div>
                                ))}
                            </div>
                        </div>

                        {/* What Makes It Special */}
                        <div className="relative">
                            <div className="absolute -left-4 top-0 bottom-0 w-1 bg-gradient-to-b from-azulejo-gold-500 via-azulejo-blue-500 to-transparent rounded-full"></div>

                            <h3 className="text-2xl lg:text-3xl font-bold text-white mb-5 flex items-center gap-3">
                                <span className="flex items-center justify-center w-10 h-10 rounded-xl bg-azulejo-gold-500/20 text-azulejo-gold-400 border border-azulejo-gold-500/30">
                                    ⭐
                                </span>
                                {t({ pt: 'O Que o Torna Especial', en: 'What Makes It Special' })}
                            </h3>

                            <div className="space-y-3">
                                {specialFeatures.map((feature, index) => (
                                    <div key={index} className="flex items-start gap-4 group">
                                        <div className="flex-shrink-0 w-10 h-10 rounded-xl bg-slate-800/50 border border-slate-700/50 flex items-center justify-center text-xl group-hover:scale-110 group-hover:bg-slate-700/50 transition-all duration-300">
                                            {feature.icon}
                                        </div>
                                        <p className="text-slate-300 leading-snug pt-2">
                                            {feature.text}
                                        </p>
                                    </div>
                                ))}
                            </div>
                        </div>

                        {/* AR Enhancement - Featured Box (sem botão) */}
                        <div className="relative group">
                            {/* Glow effect */}
                            <div className="absolute -inset-0.5 bg-gradient-to-r from-azulejo-blue-500 to-azulejo-gold-500 rounded-2xl blur-lg opacity-20 group-hover:opacity-30 transition-opacity duration-500"></div>

                            <div className="relative bg-gradient-to-br from-slate-800/90 to-slate-900/90 backdrop-blur-sm rounded-2xl p-6 lg:p-8 border border-slate-700/50 overflow-hidden">
                                {/* Decorative corner */}
                                <div className="absolute top-0 right-0 w-32 h-32 bg-gradient-to-br from-azulejo-blue-500/10 to-transparent rounded-bl-[4rem]"></div>

                                <div className="relative z-10">
                                    <div className="flex items-center gap-3 mb-5">
                                        <div className="flex items-center justify-center w-12 h-12 rounded-xl bg-gradient-to-br from-azulejo-blue-500 to-azulejo-blue-600 shadow-lg shadow-azulejo-blue-500/20">
                                            <svg className="w-6 h-6 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
                                            </svg>
                                        </div>
                                        <h3 className="text-xl lg:text-2xl font-bold text-white">
                                            {t({ pt: 'Experiência AR', en: 'AR Experience' })}
                                        </h3>
                                    </div>

                                    <div className="space-y-2.5">
                                        {arFeatures.map((feature, index) => (
                                            <div key={index} className="flex items-start gap-3 group/item">
                                                <span className="flex-shrink-0 w-6 h-6 rounded-lg bg-azulejo-blue-500/20 flex items-center justify-center text-sm group-hover/item:bg-azulejo-blue-500/30 transition-colors">
                                                    {feature.icon}
                                                </span>
                                                <span className="text-slate-300 text-sm lg:text-base leading-snug">
                                                    {feature.text}
                                                </span>
                                            </div>
                                        ))}
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </section>
    );
}

export default PanoramaShowcase;
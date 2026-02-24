/**
 * EPOCHS SECTION
 * ==============
 * Highlights the 4 historical time periods featured in the app
 * 
 * Layout: 2x2 grid (responsive to 1 column on mobile)
 * Design: Each epoch uses specific azulejo color from palette
 * Phase 2: Hover effects only
 * Phase 3: Click to open modal with detailed timeline
 * Phase 5: Scroll-triggered animations for epoch cards
 */
import { useState } from 'react';
import { useInlineTranslation } from '@/utils/language';
import { useScrollAnimation, getScrollAnimationClasses } from '@/design';
import { EpochDetailModal } from './EpochDetailModal';

interface Epoch {
    id: number;
    period: string;
    title: {
        pt: string;
        en: string;
    };
    description: {
        pt: string;
        en: string;
    };
    borderColorClass: string;
    bgColorClass: string;
    gradientFrom: string;
    gradientTo: string;
    icon: string;
}

export type { Epoch };

const epochsData: Epoch[] = [
    {
        id: 1,
        period: '~1700',
        title: {
            pt: 'Glória Pré-Terramoto',
            en: 'Pre-Earthquake Glory',
        },
        description: {
            pt: 'Veja Lisboa no seu auge antes do catastrófico terramoto. Ruas estreitas, palácios ornamentados e igrejas majestosas.',
            en: 'See Lisbon at its peak before the catastrophic earthquake. Narrow streets, ornate palaces, and majestic churches.',
        },
        borderColorClass: 'border-azulejo-blue-500',
        bgColorClass: 'bg-azulejo-blue-50',
        gradientFrom: 'from-azulejo-blue-500',
        gradientTo: 'to-azulejo-blue-600',
        icon: '🏛️',
    },
    {
        id: 2,
        period: '1755',
        title: {
            pt: 'O Grande Terramoto',
            en: 'The Great Earthquake',
        },
        description: {
            pt: 'Experiencie o evento que mudou Lisboa para sempre. Magnitude estimada de 9.0, seguida de tsunami e incêndios devastadores.',
            en: 'Experience the event that changed Lisbon forever. Estimated magnitude 9.0, followed by tsunami and devastating fires.',
        },
        borderColorClass: 'border-azulejo-terracotta',
        bgColorClass: 'bg-orange-50',
        gradientFrom: 'from-orange-500',
        gradientTo: 'to-red-600',
        icon: '⚡',
    },
    {
        id: 3,
        period: '1760-1800s',
        title: {
            pt: 'Reconstrução Pombalina',
            en: 'Pombaline Reconstruction',
        },
        description: {
            pt: 'Assista a cidade ressurgir das cinzas com planeamento urbano moderno. Ruas largas, edifícios resistentes a terramotos.',
            en: 'Watch the city rise from the ashes with modern urban planning. Wide streets, earthquake-resistant buildings.',
        },
        borderColorClass: 'border-azulejo-cobalt-500',
        bgColorClass: 'bg-blue-50',
        gradientFrom: 'from-azulejo-cobalt-500',
        gradientTo: 'to-azulejo-cobalt-600',
        icon: '🏗️',
    },
    {
        id: 4,
        period: 'Presente',
        title: {
            pt: 'Lisboa Moderna',
            en: 'Modern Day',
        },
        description: {
            pt: 'Compare a vista histórica com o horizonte atual de Lisboa. Descubra o que permaneceu e o que mudou.',
            en: 'Compare the historical view to today\'s Lisbon skyline. Discover what remained and what changed.',
        },
        borderColorClass: 'border-azulejo-gold-500',
        bgColorClass: 'bg-yellow-50',
        gradientFrom: 'from-azulejo-gold-500',
        gradientTo: 'to-azulejo-gold-600',
        icon: '🌆',
    },
];

export default function EpochsSection() {
    const t = useInlineTranslation;
    const [selectedEpoch, setSelectedEpoch] = useState<Epoch | null>(null);

    const { ref: sectionRef, isVisible } = useScrollAnimation<HTMLDivElement>({
        threshold: 0.1,
        once: true
    });

    return (
        <section
            ref={sectionRef}
            className="relative py-20 sm:py-24 lg:py-32 bg-gradient-to-br from-slate-900 via-slate-800 to-slate-900 dark:from-gray-950 dark:via-gray-900 dark:to-gray-950 overflow-hidden"
            aria-labelledby="epochs-heading"
        >
            {/* Decorative background */}
            <div className="absolute inset-0 opacity-[0.02]">
                <div className="absolute inset-0" style={{
                    backgroundImage: `url("data:image/svg+xml,%3Csvg width='60' height='60' viewBox='0 0 60 60' xmlns='http://www.w3.org/2000/svg'%3E%3Cpath d='M30 0L60 30L30 60L0 30z' fill='%23ffffff' fill-opacity='1'/%3E%3C/svg%3E")`,
                    backgroundSize: '60px 60px'
                }}></div>
            </div>

            {/* Glowing orbs */}
            <div className="absolute top-20 left-10 w-96 h-96 bg-azulejo-blue-500/5 rounded-full blur-3xl"></div>
            <div className="absolute bottom-20 right-10 w-96 h-96 bg-azulejo-gold-500/5 rounded-full blur-3xl"></div>

            <div className="container mx-auto px-4 sm:px-6 lg:px-8 relative z-10">
                {/* Header */}
                <div className={`text-center mb-16 sm:mb-20 ${getScrollAnimationClasses(isVisible, 'slide-up')}`}>
                    <div className="inline-block mb-6">
                        <span className="inline-flex items-center gap-2 px-5 py-2.5 rounded-full bg-gradient-to-r from-azulejo-blue-500/10 to-azulejo-gold-500/10 border border-azulejo-blue-500/20 text-azulejo-blue-400 text-sm font-semibold backdrop-blur-sm">
                            <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
                            </svg>
                            {t({ pt: 'Linha do Tempo', en: 'Timeline' })}
                        </span>
                    </div>

                    <h2
                        id="epochs-heading"
                        className="text-4xl sm:text-5xl lg:text-6xl font-bold mb-6 text-white"
                    >
                        {t({
                            pt: 'Viaje Através de 4 Épocas',
                            en: 'Journey Through 4 Epochs',
                        })}
                    </h2>

                    <p className={`text-lg sm:text-xl text-slate-300 max-w-3xl mx-auto leading-relaxed ${getScrollAnimationClasses(isVisible, 'slide-up')}`}
                        style={{ transitionDelay: '100ms' }}>
                        {t({
                            pt: 'De glória a devastação, de reconstrução a modernidade. Testemunhe a transformação de Lisboa.',
                            en: 'From glory to devastation, from reconstruction to modernity. Witness Lisbon\'s transformation.',
                        })}
                    </p>
                </div>

                {/* Timeline connector - decorative line */}
                <div className="hidden lg:block absolute left-1/2 top-[280px] bottom-[120px] w-0.5 bg-gradient-to-b from-azulejo-blue-500 via-azulejo-gold-500 to-transparent opacity-30 -translate-x-1/2 z-0"></div>

                {/* Epochs Grid */}
                <div className="grid grid-cols-1 md:grid-cols-2 gap-8 lg:gap-10 max-w-6xl mx-auto">
                    {epochsData.map((epoch, index) => (
                        <div
                            key={epoch.id}
                            onClick={() => setSelectedEpoch(epoch)}
                            className={`
                                group relative
                                bg-slate-800/50 dark:bg-gray-800/50
                                backdrop-blur-sm
                                border border-slate-700/50 dark:border-gray-700/50
                                rounded-2xl
                                overflow-hidden
                                cursor-pointer
                                transform hover:-translate-y-2
                                transition-all duration-500
                                ${getScrollAnimationClasses(isVisible, 'slide-up')}
                            `}
                            style={{ transitionDelay: `${200 + index * 100}ms` }}
                            role="article"
                            aria-labelledby={`epoch-${epoch.id}-title`}
                            tabIndex={0}
                            onKeyDown={(e) => {
                                if (e.key === 'Enter' || e.key === ' ') {
                                    e.preventDefault();
                                    setSelectedEpoch(epoch);
                                }
                            }}
                        >
                            {/* Colored accent bar on top */}
                            <div className={`h-1.5 bg-gradient-to-r ${epoch.gradientFrom} ${epoch.gradientTo}`}></div>

                            {/* Glow effect */}
                            <div className={`absolute -inset-0.5 bg-gradient-to-r ${epoch.gradientFrom} ${epoch.gradientTo} rounded-2xl opacity-0 group-hover:opacity-20 blur-xl transition-opacity duration-500 -z-10`}></div>

                            {/* Content */}
                            <div className="p-8 lg:p-10">
                                {/* Icon & Period badge */}
                                <div className="flex items-center justify-between mb-6">
                                    <div className={`flex items-center justify-center w-16 h-16 rounded-xl bg-gradient-to-br ${epoch.gradientFrom} ${epoch.gradientTo} shadow-lg transform group-hover:scale-110 group-hover:rotate-3 transition-all duration-500`}>
                                        <span className="text-3xl" aria-hidden="true">
                                            {epoch.icon}
                                        </span>
                                    </div>

                                    <div className={`px-4 py-2 rounded-lg bg-gradient-to-r ${epoch.gradientFrom} ${epoch.gradientTo} bg-opacity-10 border ${epoch.borderColorClass} border-opacity-30`}>
                                        <span className="text-sm font-bold text-white uppercase tracking-wider">
                                            {epoch.period}
                                        </span>
                                    </div>
                                </div>

                                {/* Title */}
                                <h3
                                    id={`epoch-${epoch.id}-title`}
                                    className="text-2xl lg:text-3xl font-bold text-white mb-4 group-hover:text-azulejo-gold-300 transition-colors duration-300"
                                >
                                    {t(epoch.title)}
                                </h3>

                                {/* Description */}
                                <p className="text-slate-300 leading-relaxed mb-6">
                                    {t(epoch.description)}
                                </p>

                                {/* Learn More CTA */}
                                <div className="flex items-center gap-2 text-azulejo-blue-400 group-hover:text-azulejo-gold-400 font-semibold text-sm group-hover:gap-3 transition-all duration-300">
                                    <span>{t({ pt: 'Explorar Época', en: 'Explore Epoch' })}</span>
                                    <svg
                                        className="w-5 h-5 transform group-hover:translate-x-1 transition-transform"
                                        fill="none"
                                        stroke="currentColor"
                                        viewBox="0 0 24 24"
                                    >
                                        <path
                                            strokeLinecap="round"
                                            strokeLinejoin="round"
                                            strokeWidth={2}
                                            d="M17 8l4 4m0 0l-4 4m4-4H3"
                                        />
                                    </svg>
                                </div>
                            </div>

                            {/* Timeline dot connector */}
                            <div className="hidden lg:block absolute top-1/2 -translate-y-1/2 z-20">
                                {index % 2 === 0 ? (
                                    // Left side cards - dot on right
                                    <div className={`absolute -right-[43px] w-4 h-4 rounded-full bg-gradient-to-br ${epoch.gradientFrom} ${epoch.gradientTo} ring-4 ring-slate-900 shadow-lg`}></div>
                                ) : (
                                    // Right side cards - dot on left
                                    <div className={`absolute -left-[43px] w-4 h-4 rounded-full bg-gradient-to-br ${epoch.gradientFrom} ${epoch.gradientTo} ring-4 ring-slate-900 shadow-lg`}></div>
                                )}
                            </div>

                            {/* Decorative corner element */}
                            <div className={`absolute bottom-0 right-0 w-32 h-32 bg-gradient-to-tl ${epoch.gradientFrom} ${epoch.gradientTo} opacity-5 rounded-tl-[4rem]`}></div>
                        </div>
                    ))}
                </div>

                {/* Timeline summary - bottom */}
                <div className={`mt-16 sm:mt-20 text-center ${getScrollAnimationClasses(isVisible, 'slide-up')}`}
                    style={{ transitionDelay: '600ms' }}>
                    <div className="inline-flex items-center gap-8 px-8 py-4 rounded-full bg-slate-800/50 backdrop-blur-sm border border-slate-700/50">
                        {epochsData.map((epoch, index) => (
                            <div key={epoch.id} className="flex items-center gap-3">
                                <div className={`w-3 h-3 rounded-full bg-gradient-to-br ${epoch.gradientFrom} ${epoch.gradientTo} shadow-lg`}></div>
                                <span className="text-sm text-slate-400 hidden sm:inline">{epoch.period}</span>
                                {index < epochsData.length - 1 && (
                                    <svg className="w-4 h-4 text-slate-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
                                    </svg>
                                )}
                            </div>
                        ))}
                    </div>
                </div>
            </div>

            {/* Epoch Detail Modal */}
            <EpochDetailModal
                isOpen={!!selectedEpoch}
                onClose={() => setSelectedEpoch(null)}
                epoch={selectedEpoch}
            />
        </section>
    );
}
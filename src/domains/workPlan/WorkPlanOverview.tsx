/**
 * WORK PLAN OVERVIEW SECTION
 * ===========================
 * 
 * Quick snapshot of key project metrics in 3-column grid.
 * Provides investors with immediate understanding of timeline, budget, and scope.
 * 
 * Layout: 3-column grid (responsive to 1-column mobile)
 * Design: Modern glass morphism cards with gradient accents and micro-interactions
 * Phase 5: Animated counters for engaging number presentation
 */

import { useInlineTranslation } from '@/utils/language';
import { projectMetadata } from './utils';
import { AnimatedCounter } from '@/design/animations';
import { useScrollAnimation, getScrollAnimationClasses } from '@/design';

interface OverviewCard {
    icon: string;
    title: string;
    details: string;
    metric?: string;
    accentColor: string;
}

export function WorkPlanOverview() {
    const timelineTitle = useInlineTranslation('12 Meses', '12 Months');
    const timelineDetails = useInlineTranslation(
        `MVP Mês ${projectMetadata.mvpMonth} | Excelência Mês ${projectMetadata.finalDeliveryMonth}`,
        `MVP Month ${projectMetadata.mvpMonth} | Excellence Month ${projectMetadata.finalDeliveryMonth}`
    );

    const budgetDetails = useInlineTranslation(
        `€${projectMetadata.totalCost} desenvolvimento + €${projectMetadata.buffer} buffer`,
        `€${projectMetadata.totalCost} development + €${projectMetadata.buffer} buffer`
    );

    const phasesTitle = useInlineTranslation(
        `${projectMetadata.phaseCount} Fases`,
        `${projectMetadata.phaseCount} Phases`
    );

    const sectionTitle = useInlineTranslation(
        'Visão Geral do Projeto',
        'Project Overview'
    );

    const sectionSubtitle = useInlineTranslation(
        'Planeamento estratégico para sucesso garantido',
        'Strategic planning for guaranteed success'
    );



    const timelineLabel = useInlineTranslation('Cronograma', 'Timeline');
    const budgetLabel = useInlineTranslation('Orçamento', 'Budget');
    const deliverablesLabel = useInlineTranslation('Entregas', 'Deliverables');


    // Extract numeric values for animation
    const totalBudget = projectMetadata.totalCostWithBuffer;

    // Scroll animation
    const { ref: sectionRef, isVisible } = useScrollAnimation<HTMLDivElement>({
        threshold: 0.1,
        once: true
    });

    const cards: OverviewCard[] = [
        {
            icon: '📅',
            title: timelineTitle,
            details: timelineDetails,
            metric: '12',
            accentColor: 'from-blue-500 to-azulejo-blue-600',
        },
        {
            icon: '💰',
            title: '', // Will use AnimatedCounter
            details: budgetDetails,
            accentColor: 'from-azulejo-gold-500 to-amber-600',
        },
        {
            icon: '🎯',
            title: phasesTitle,
            details: 'MVP → Core → Advanced → Optimization',
            metric: String(projectMetadata.phaseCount),
            accentColor: 'from-emerald-500 to-teal-600',
        },
    ];

    return (
        <section
            id="overview"
            ref={sectionRef}
            className="py-20 sm:py-28 bg-gradient-to-b from-gray-50 via-white to-azulejo-ivory-50 
                     dark:from-gray-900 dark:via-gray-900 dark:to-gray-800
                     relative overflow-hidden"
            aria-labelledby="overview-heading"
        >
            {/* Decorative background elements */}
            <div className="absolute inset-0 overflow-hidden pointer-events-none">
                <div className="absolute -top-40 -right-40 w-80 h-80 bg-azulejo-blue-200/20 dark:bg-azulejo-blue-800/10 rounded-full blur-3xl" />
                <div className="absolute -bottom-40 -left-40 w-80 h-80 bg-azulejo-gold-200/20 dark:bg-azulejo-gold-800/10 rounded-full blur-3xl" />
            </div>

            <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 relative z-10">
                {/* Section Header */}
                <div className={`text-center mb-16 ${getScrollAnimationClasses(isVisible, 'slide-up')}`}>
                    <h2
                        id="overview-heading"
                        className="text-3xl sm:text-4xl lg:text-5xl font-bold mb-4 
                                 text-gray-900 dark:text-white tracking-tight"
                    >
                        {sectionTitle}
                    </h2>
                    <p className="text-lg sm:text-xl text-gray-600 dark:text-gray-300 max-w-2xl mx-auto font-light">
                        {sectionSubtitle}
                    </p>
                </div>

                {/* Cards Grid */}
                <div className="grid grid-cols-1 md:grid-cols-3 gap-6 lg:gap-8">
                    {cards.map((card, index) => (
                        <div
                            key={index}
                            className={`group relative ${getScrollAnimationClasses(isVisible, 'slide-up')}`}
                            style={{ transitionDelay: `${(index + 1) * 100}ms` }}
                        >
                            {/* Card Container */}
                            <div className="relative h-full p-8 rounded-2xl
                                          bg-white/80 dark:bg-gray-800/80 backdrop-blur-xl
                                          border-2 border-gray-200/50 dark:border-gray-700/50
                                          hover:border-transparent
                                          shadow-lg hover:shadow-2xl
                                          transition-all duration-500 
                                          hover:-translate-y-3
                                          overflow-hidden">

                                {/* Animated gradient border on hover */}
                                <div className={`absolute inset-0 bg-gradient-to-br ${card.accentColor} 
                                              opacity-0 group-hover:opacity-100 transition-opacity duration-500 
                                              rounded-2xl`}
                                    style={{ padding: '2px' }}>
                                    <div className="w-full h-full bg-white dark:bg-gray-800 rounded-2xl" />
                                </div>

                                {/* Top accent line */}
                                <div className={`absolute top-0 left-0 right-0 h-1 bg-gradient-to-r ${card.accentColor} 
                                              transform scale-x-0 group-hover:scale-x-100 transition-transform duration-500 
                                              origin-left`} />

                                {/* Content */}
                                <div className="relative z-10 flex flex-col h-full">
                                    {/* Icon with gradient background */}
                                    <div className="mb-6">
                                        <div className={`inline-flex items-center justify-center w-16 h-16 sm:w-20 sm:h-20 
                                                       rounded-2xl bg-gradient-to-br ${card.accentColor}
                                                       shadow-lg group-hover:shadow-xl
                                                       transform group-hover:scale-110 group-hover:rotate-3
                                                       transition-all duration-500`}>
                                            <span className="text-3xl sm:text-4xl filter drop-shadow-lg"
                                                aria-hidden="true">
                                                {card.icon}
                                            </span>
                                        </div>
                                    </div>

                                    {/* Title with animated counter for budget card */}
                                    <h3 className="text-2xl sm:text-3xl font-bold mb-3 
                                                 text-gray-900 dark:text-white
                                                 group-hover:text-transparent group-hover:bg-clip-text 
                                                 group-hover:bg-gradient-to-r group-hover:${card.accentColor}
                                                 transition-all duration-500">
                                        {index === 1 ? (
                                            <AnimatedCounter
                                                target={totalBudget}
                                                duration={2}
                                                prefix="€"
                                            />
                                        ) : (
                                            card.title
                                        )}
                                    </h3>

                                    {/* Details */}
                                    <p className="text-base sm:text-lg text-gray-600 dark:text-gray-300 
                                                leading-relaxed flex-grow">
                                        {card.details}
                                    </p>

                                    {/* Bottom decorative element */}
                                    <div className="mt-6 pt-4 border-t border-gray-200 dark:border-gray-700 
                                                  opacity-0 group-hover:opacity-100 transition-opacity duration-500">
                                        <div className="flex items-center justify-between">
                                            <span className="text-sm font-medium text-gray-500 dark:text-gray-400">
                                                {index === 0 && timelineLabel}
                                                {index === 1 && budgetLabel}
                                                {index === 2 && deliverablesLabel}
                                            </span>
                                            <svg
                                                className={`w-5 h-5 text-gray-400 transform group-hover:translate-x-1 
                                                          transition-transform duration-300`}
                                                fill="none"
                                                stroke="currentColor"
                                                viewBox="0 0 24 24"
                                            >
                                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
                                                    d="M9 5l7 7-7 7" />
                                            </svg>
                                        </div>
                                    </div>
                                </div>

                                {/* Decorative corner accent */}
                                <div className={`absolute -bottom-8 -right-8 w-32 h-32 bg-gradient-to-br ${card.accentColor} 
                                              opacity-10 rounded-full blur-2xl
                                              transform scale-0 group-hover:scale-100
                                              transition-transform duration-700`} />
                            </div>
                        </div>
                    ))}
                </div>

                {/* Bottom CTA hint */}
                <div className={`mt-16 text-center ${getScrollAnimationClasses(isVisible, 'slide-up')}`}
                    style={{ transitionDelay: '400ms' }}>
                    <p className="text-base sm:text-lg text-gray-600 dark:text-gray-400 font-light">
                        {useInlineTranslation(
                            'Explore os detalhes de cada fase abaixo',
                            'Explore the details of each phase below'
                        )}
                    </p>
                    <div className="mt-4 flex justify-center">
                        <svg
                            className="w-6 h-6 text-azulejo-blue-500 animate-bounce"
                            fill="none"
                            stroke="currentColor"
                            viewBox="0 0 24 24"
                        >
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
                                d="M19 9l-7 7-7-7" />
                        </svg>
                    </div>
                </div>
            </div>
        </section>
    );
}

export default WorkPlanOverview;

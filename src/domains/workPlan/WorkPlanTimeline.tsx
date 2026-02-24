/**
 * WORK PLAN TIMELINE SECTION
 * ===========================
 * 
 * Interactive timeline showing 4 development phases across 12 months.
 * Horizontal layout on desktop, vertical on mobile.
 * Clickable phase markers scroll to detail sections.
 * 
 * Layout: Horizontal timeline (desktop), vertical (mobile)
 * Design: Phase-specific colors with progress line
 * Interaction: Click phase circle to scroll to details
 * Phase 5: Scroll-spy highlights active phase based on scroll position
 */

import { useState, useEffect } from 'react';
import { useInlineTranslation, useLanguage } from '@/utils/language';
import { phases } from './utils';
import { getPhaseColor } from './colors';

export function WorkPlanTimeline() {
    const { language } = useLanguage();
    const [activePhase, setActivePhase] = useState<number>(1);

    const timelineTitle = useInlineTranslation(
        'Cronograma Interativo',
        'Interactive Timeline'
    );

    const monthsLabel = useInlineTranslation('Meses', 'Months');

    // Scroll-spy: Track which phase section is currently visible
    useEffect(() => {
        const handleScroll = () => {
            const scrollPosition = window.scrollY + 200; // Offset for better UX

            // Check each phase section's position
            for (let i = phases.length; i >= 1; i--) {
                const phaseElement = document.getElementById(`phase-${i}`);
                if (phaseElement) {
                    const rect = phaseElement.getBoundingClientRect();
                    const offsetTop = window.scrollY + rect.top;

                    if (scrollPosition >= offsetTop) {
                        setActivePhase(i);
                        break;
                    }
                }
            }
        };

        // Initial check
        handleScroll();

        // Add scroll listener with passive flag for performance
        window.addEventListener('scroll', handleScroll, { passive: true });

        return () => {
            window.removeEventListener('scroll', handleScroll);
        };
    }, []);

    const handlePhaseClick = (phaseId: number) => {
        const phaseElement = document.getElementById(`phase-${phaseId}`);
        if (phaseElement) {
            phaseElement.scrollIntoView({
                behavior: 'smooth',
                block: 'start',
                inline: 'nearest'
            });
        }
    };

    return (
        <section
            className="py-16 sm:py-20 bg-azulejo-ivory-50 dark:bg-gray-900"
            aria-labelledby="timeline-heading"
        >
            <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
                <h2
                    id="timeline-heading"
                    className="text-3xl sm:text-4xl font-bold text-center mb-12 text-gray-900 dark:text-white"
                >
                    {timelineTitle}
                </h2>

                {/* Desktop: Horizontal Timeline */}
                <div className="hidden lg:block">
                    <div className="relative">
                        {/* Progress Line */}
                        <div className="absolute top-8 left-0 right-0 h-1 bg-gray-300 dark:bg-gray-700" />

                        {/* Phase Markers */}
                        <div className="relative flex justify-between items-start">
                            {phases.map((phase) => (
                                <div
                                    key={phase.id}
                                    className="relative z-10 flex flex-col items-center cursor-pointer group"
                                    onClick={() => handlePhaseClick(phase.id)}
                                    role="button"
                                    tabIndex={0}
                                    onKeyDown={(e) => {
                                        if (e.key === 'Enter' || e.key === ' ') {
                                            handlePhaseClick(phase.id);
                                        }
                                    }}
                                    aria-label={`${language === 'pt' ? phase.title_pt : phase.title_en}. ${monthsLabel} ${phase.months}. €${phase.cost}`}
                                >
                                    {/* Circle Marker */}
                                    <div
                                        className={`w-16 h-16 rounded-full flex items-center justify-center
                                                 text-white font-bold text-xl border-4 border-white dark:border-gray-900
                                                 shadow-lg transition-all duration-300 
                                                 ${activePhase === phase.id ? 'scale-125 opacity-100' : 'opacity-70 group-hover:opacity-100'}`}
                                        style={{ backgroundColor: getPhaseColor(phase.id) }}
                                    >
                                        {phase.id}
                                    </div>

                                    {/* Phase Info */}
                                    <div className="mt-6 text-center max-w-[200px]">
                                        <p className="font-semibold text-gray-900 dark:text-white mb-1">
                                            {language === 'pt' ? phase.title_pt : phase.title_en}
                                        </p>
                                        <p className="text-sm text-gray-600 dark:text-gray-400">
                                            {monthsLabel} {phase.months}
                                        </p>
                                        <p
                                            className="text-sm font-medium mt-1"
                                            style={{ color: getPhaseColor(phase.id) }}
                                        >
                                            €{phase.cost}
                                        </p>
                                    </div>
                                </div>
                            ))}
                        </div>
                    </div>
                </div>

                {/* Mobile: Vertical Timeline */}
                <div className="lg:hidden space-y-6">
                    {phases.map((phase, index) => (
                        <div key={phase.id} className="relative">
                            {/* Vertical Line (except for last item) */}
                            {index < phases.length - 1 && (
                                <div
                                    className="absolute left-6 top-14 bottom-0 w-1 bg-gray-300 dark:bg-gray-700"
                                    aria-hidden="true"
                                />
                            )}

                            {/* Phase Item */}
                            <div
                                className="flex items-start gap-4 cursor-pointer"
                                onClick={() => handlePhaseClick(phase.id)}
                                role="button"
                                tabIndex={0}
                                onKeyDown={(e) => {
                                    if (e.key === 'Enter' || e.key === ' ') {
                                        handlePhaseClick(phase.id);
                                    }
                                }}
                            >
                                {/* Circle Marker */}
                                <div
                                    className={`relative z-10 w-12 h-12 rounded-full flex items-center justify-center
                                             text-white font-bold flex-shrink-0 shadow-md
                                             transition-all duration-300
                                             ${activePhase === phase.id ? 'scale-110 opacity-100' : 'opacity-70'}`}
                                    style={{ backgroundColor: getPhaseColor(phase.id) }}
                                >
                                    {phase.id}
                                </div>

                                {/* Phase Info */}
                                <div className="flex-1 pt-1">
                                    <p className="font-semibold text-gray-900 dark:text-white text-lg">
                                        {language === 'pt' ? phase.title_pt : phase.title_en}
                                    </p>
                                    <p className="text-sm text-gray-600 dark:text-gray-400 mt-1">
                                        {monthsLabel} {phase.months}
                                    </p>
                                    <p
                                        className="text-sm font-medium mt-1"
                                        style={{ color: getPhaseColor(phase.id) }}
                                    >
                                        €{phase.cost}
                                    </p>
                                </div>
                            </div>
                        </div>
                    ))}
                </div>
            </div>
        </section>
    );
}

export default WorkPlanTimeline;

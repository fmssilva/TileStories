/**
 * PHASE DETAIL LAYOUT - UNIFIED COMPONENT
 * ========================================
 * 
 * Reusable layout component for all 4 development phases.
 * Provides consistent formatting with Phase4-style card design:
 * - Icon + border-2 cards for all sections
 * - 2x2 grid layout (Deliverables, Tech Stack, Milestones, Investment)
 * - Feature showcase image/video section
 * - Value proposition banner
 * - Integrated clickable tech stack with cost modals
 * 
 * Benefits:
 * - Reduces code duplication by ~70%
 * - Ensures consistent UX across all phases
 * - Easy to maintain and extend
 * - Preserves phase-specific customizations
 */

import { useState } from 'react';
import { useInlineTranslation, useLanguage } from '@/utils/language';
import { VideoModal } from '@/components/ui';
import { getPhaseById } from './utils';
import { getPhaseColor } from './colors';
import CostDetailModal from './CostDetailModal';
import { getCostIdForTechStack } from './techStackMapping';

/**
 * Props interface for PhaseDetailLayout
 */
interface PhaseDetailLayoutProps {
    /** Phase number (1-4) */
    phaseId: 1 | 2 | 3 | 4;

    /** Feature showcase image path */
    featureImage: string;

    /** Feature image alt text (Portuguese) */
    featureImageAlt_pt: string;

    /** Feature image alt text (English) */
    featureImageAlt_en: string;

    /** Feature caption shown on image overlay (Portuguese) */
    featureCaption_pt: string;

    /** Feature caption shown on image overlay (English) */
    featureCaption_en: string;

    /** Optional custom highlight banner (e.g., Phase3 earthquake simulation) */
    customHighlight?: React.ReactNode;

    /** Enable video demo section (Phase1 only) */
    showVideoSection?: boolean;

    /** Video URL if showVideoSection is true */
    videoUrl?: string;
}

/**
 * Unified Phase Detail Layout Component
 * 
 * Renders a consistent layout for all development phases with:
 * - Phase header
 * - Optional custom highlight banner
 * - 2x2 card grid (Deliverables, Tech Stack, Milestones, Investment Summary)
 * - Feature showcase image
 * - Optional video section
 * - Value proposition banner
 * - Cost detail modal
 */
export function PhaseDetailLayout({
    phaseId,
    featureImage,
    featureImageAlt_pt,
    featureImageAlt_en,
    featureCaption_pt,
    featureCaption_en,
    customHighlight,
    showVideoSection = false,
    videoUrl,
}: PhaseDetailLayoutProps) {
    const { language } = useLanguage();
    const phase = getPhaseById(phaseId)!;

    // State management
    const [selectedCostId, setSelectedCostId] = useState<string | null>(null);
    const [showVideo, setShowVideo] = useState(false);

    // Translations
    const deliverablesTitle = useInlineTranslation('Entregas', 'Deliverables');
    const techStackTitle = useInlineTranslation('Tecnologias', 'Tech Stack');
    const milestonesTitle = useInlineTranslation('Marcos', 'Milestones');
    const investmentTitle = useInlineTranslation('Investimento', 'Investment');

    // Icons for each card
    const icons = {
        deliverables: '✨',
        techStack: '🛠️',
        milestones: '📍',
        investment: '💎',
    };

    return (
        <div id={`phase-${phaseId}`} className="space-y-8">
            {/* Phase Header */}
            <div className="flex items-center gap-4">
                <div
                    className="w-16 h-16 rounded-full flex items-center justify-center
                             text-white font-bold text-2xl shadow-lg"
                    style={{ backgroundColor: getPhaseColor(phaseId) }}
                >
                    {phaseId}
                </div>
                <div>
                    <h3 className="text-3xl sm:text-4xl font-bold text-gray-900 dark:text-white">
                        {language === 'pt' ? phase.title_pt : phase.title_en}
                    </h3>
                    <p className="text-lg text-gray-600 dark:text-gray-400 mt-1">
                        {language === 'pt' ? phase.subtitle_pt : phase.subtitle_en}
                    </p>
                </div>
            </div>

            {/* Optional Custom Highlight (e.g., Phase3 earthquake banner) */}
            {customHighlight && <div>{customHighlight}</div>}

            {/* 2x2 Card Grid Layout */}
            <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-10">
                {/* Card 1: Deliverables */}
                <div
                    className="p-6 rounded-xl bg-white dark:bg-gray-950
                                 border-2 hover:shadow-lg transition-shadow duration-300"
                    style={{ borderColor: getPhaseColor(phaseId) }}
                >
                    <h3
                        className="text-2xl font-bold mb-4 flex items-center gap-2"
                        style={{ color: getPhaseColor(phaseId) }}
                    >
                        <span className="text-2xl" aria-hidden="true">
                            {icons.deliverables}
                        </span>
                        {deliverablesTitle}
                    </h3>
                    <ul className="space-y-2.5">
                        {(language === 'pt' ? phase.deliverables_pt : phase.deliverables_en).map(
                            (deliverable: string, index: number) => (
                                <li
                                    key={index}
                                    className="flex items-start gap-2 text-sm text-gray-700 dark:text-gray-300"
                                >
                                    <span
                                        className="mt-1 w-1.5 h-1.5 rounded-full flex-shrink-0"
                                        style={{ backgroundColor: getPhaseColor(phaseId) }}
                                        aria-hidden="true"
                                    />
                                    <span>{deliverable}</span>
                                </li>
                            )
                        )}
                    </ul>
                </div>

                {/* Card 2: Tech Stack */}
                <div
                    className="p-6 rounded-xl bg-white dark:bg-gray-950
                                 border-2 hover:shadow-lg transition-shadow duration-300"
                    style={{ borderColor: getPhaseColor(phaseId) }}
                >
                    <h3
                        className="text-2xl font-bold mb-4 flex items-center gap-2"
                        style={{ color: getPhaseColor(phaseId) }}
                    >
                        <span className="text-2xl" aria-hidden="true">
                            {icons.techStack}
                        </span>
                        {techStackTitle}
                    </h3>
                    <div className="grid grid-cols-2 gap-3">
                        {phase.techStack.map((tech, index) => {
                            const costId = getCostIdForTechStack(tech.name, phaseId);
                            return (
                                <button
                                    key={index}
                                    onClick={() => costId && setSelectedCostId(costId)}
                                    disabled={!costId}
                                    className={`flex flex-col px-3 py-2.5
                                             bg-azulejo-ivory-50 dark:bg-gray-900 rounded-lg
                                             border border-gray-200 dark:border-gray-700
                                             transition-all duration-200
                                             ${costId
                                            ? 'hover:bg-azulejo-gold-50 dark:hover:bg-gray-800 hover:border-azulejo-gold-400 hover:scale-105 hover:shadow-lg cursor-pointer'
                                            : 'cursor-default'
                                        }
                                             focus:outline-none focus:ring-2 focus:ring-azulejo-gold-500 focus:ring-offset-2 dark:focus:ring-offset-gray-900`}
                                    aria-label={
                                        costId
                                            ? `${tech.name} - €${tech.cost}. ${language === 'pt' ? 'Clique para ver detalhes' : 'Click for details'}`
                                            : `${tech.name} - €${tech.cost}`
                                    }
                                >
                                    <div className="flex items-center justify-between w-full">
                                        <span className="text-sm font-medium text-gray-900 dark:text-white text-left">
                                            {tech.name}
                                        </span>
                                        {costId && (
                                            <svg
                                                className="w-4 h-4 text-azulejo-gold-500 flex-shrink-0"
                                                fill="none"
                                                stroke="currentColor"
                                                viewBox="0 0 24 24"
                                                aria-hidden="true"
                                            >
                                                <path
                                                    strokeLinecap="round"
                                                    strokeLinejoin="round"
                                                    strokeWidth={2}
                                                    d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
                                                />
                                            </svg>
                                        )}
                                    </div>
                                    <span className="text-xs text-gray-500 dark:text-gray-400 text-left mt-1">
                                        €{tech.cost}
                                    </span>
                                </button>
                            );
                        })}
                    </div>
                </div>

                {/* Card 3: Milestones */}
                <div
                    className="p-6 rounded-xl bg-white dark:bg-gray-950
                                 border-2 hover:shadow-lg transition-shadow duration-300"
                    style={{ borderColor: getPhaseColor(phaseId) }}
                >
                    <h3
                        className="text-2xl font-bold mb-4 flex items-center gap-2"
                        style={{ color: getPhaseColor(phaseId) }}
                    >
                        <span className="text-2xl" aria-hidden="true">
                            {icons.milestones}
                        </span>
                        {milestonesTitle}
                    </h3>
                    <div className="space-y-3">
                        {phase.milestones.map((milestone, index) => (
                            <div key={index} className="flex gap-3">
                                <div
                                    className="w-10 h-10 rounded-full flex items-center justify-center
                                                 text-white font-bold text-xs flex-shrink-0"
                                    style={{ backgroundColor: getPhaseColor(phaseId) }}
                                >
                                    M{milestone.month}
                                </div>
                                <p className="text-sm text-gray-700 dark:text-gray-300 pt-1.5">
                                    {language === 'pt'
                                        ? milestone.description_pt
                                        : milestone.description_en}
                                </p>
                            </div>
                        ))}
                    </div>
                </div>

                {/* Card 4: Investment Summary */}
                <div
                    className="p-6 rounded-xl text-white"
                    style={{
                        background: `linear-gradient(135deg, ${getPhaseColor(phaseId)} 0%, ${getPhaseColor(
                            phaseId
                        )}dd 100%)`
                    }}
                >
                    <h3 className="text-2xl font-bold mb-4 flex items-center gap-2">
                        <span className="text-2xl" aria-hidden="true">
                            {icons.investment}
                        </span>
                        {investmentTitle}
                    </h3>
                    <div className="space-y-4">
                        <div>
                            <p className="text-3xl font-bold">€{phase.cost}</p>
                            <p className="text-sm opacity-90 mt-1">
                                {language === 'pt' ? `Fase ${phaseId} de 4` : `Phase ${phaseId} of 4`}
                            </p>
                        </div>
                        <div className="pt-4 border-t border-white/20">
                            <p className="text-base font-medium leading-relaxed">
                                {language === 'pt' ? phase.value_pt : phase.value_en}
                            </p>
                        </div>
                    </div>
                </div>
            </div>

            {/* Feature Showcase Image/Video */}
            <div className="relative rounded-xl overflow-hidden shadow-xl group">
                {featureImage.endsWith('.mp4') || featureImage.endsWith('.webm') ? (
                    // Video element
                    <video
                        autoPlay
                        loop
                        muted
                        playsInline
                        className="w-full h-auto transition-transform duration-500 group-hover:scale-105"
                    >
                        <source src={featureImage} type="video/mp4" />
                    </video>
                ) : (
                    // Image element
                    <img
                        src={featureImage}
                        alt={language === 'pt' ? featureImageAlt_pt : featureImageAlt_en}
                        className="w-full h-auto transition-transform duration-500 group-hover:scale-105"
                    />
                )}
                <div className="absolute bottom-0 left-0 right-0 bg-gradient-to-t from-black/80 via-black/40 to-transparent p-6">
                    <p className="text-white font-semibold text-lg">
                        {language === 'pt' ? featureCaption_pt : featureCaption_en}
                    </p>
                </div>
            </div>

            {/* Optional Video Demo Section (Phase1 only) */}
            {showVideoSection && videoUrl && (
                <div className="bg-gradient-to-br from-azulejo-blue-50 to-azulejo-cobalt-50 dark:from-gray-900 dark:to-gray-850 
                          p-8 rounded-xl border-2 border-azulejo-blue-200 dark:border-azulejo-blue-800">
                    <div className="grid grid-cols-1 lg:grid-cols-2 gap-8 items-center">
                        {/* Text Content */}
                        <div>
                            <h3 className="text-2xl font-bold mb-4 text-azulejo-blue-900 dark:text-white">
                                {language === 'pt'
                                    ? 'Veja o MVP em Ação'
                                    : 'See the MVP in Action'}
                            </h3>
                            <p className="text-gray-700 dark:text-gray-300 mb-6 leading-relaxed">
                                {language === 'pt'
                                    ? 'Explore como a realidade aumentada traz o Grande Panorama de Lisboa à vida. Veja demonstrações das funcionalidades principais do MVP.'
                                    : 'Explore how augmented reality brings the Grande Panorama de Lisboa to life. See demonstrations of the MVP\'s core features.'}
                            </p>
                            <button
                                onClick={() => setShowVideo(true)}
                                className="group relative overflow-hidden inline-flex items-center gap-3 
                                         px-6 py-3 bg-azulejo-blue-500 hover:bg-azulejo-blue-600 
                                         text-white font-semibold rounded-lg shadow-lg hover:shadow-xl
                                         transition-all duration-300 hover:scale-105"
                            >
                                {/* Shine effect */}
                                <div className="absolute inset-0 -translate-x-full group-hover:translate-x-full 
                                             transition-transform duration-700 ease-out
                                             bg-gradient-to-r from-transparent via-white/30 to-transparent" />
                                <svg
                                    className="w-5 h-5 relative z-10"
                                    fill="currentColor"
                                    viewBox="0 0 20 20"
                                >
                                    <path d="M6.3 2.841A1.5 1.5 0 004 4.11V15.89a1.5 1.5 0 002.3 1.269l9.344-5.89a1.5 1.5 0 000-2.538L6.3 2.84z" />
                                </svg>
                                <span className="relative z-10">
                                    {language === 'pt' ? 'Assistir Vídeo Demo' : 'Watch Demo Video'}
                                </span>
                            </button>
                        </div>

                        {/* Video Thumbnail */}
                        <div
                            onClick={() => setShowVideo(true)}
                            className="relative rounded-lg overflow-hidden shadow-xl cursor-pointer group"
                        >
                            <div className="aspect-video bg-gradient-to-br from-azulejo-blue-400 to-azulejo-cobalt-500 
                                          flex items-center justify-center relative">
                                {/* Play button overlay */}
                                <div className="absolute inset-0 bg-black/20 group-hover:bg-black/30 
                                             transition-colors duration-300 flex items-center justify-center">
                                    <div className="w-20 h-20 rounded-full bg-white/90 group-hover:bg-white 
                                                 flex items-center justify-center shadow-2xl
                                                 transform group-hover:scale-110 transition-all duration-300">
                                        <svg
                                            className="w-10 h-10 text-azulejo-blue-600 ml-1"
                                            fill="currentColor"
                                            viewBox="0 0 20 20"
                                        >
                                            <path d="M6.3 2.841A1.5 1.5 0 004 4.11V15.89a1.5 1.5 0 002.3 1.269l9.344-5.89a1.5 1.5 0 000-2.538L6.3 2.84z" />
                                        </svg>
                                    </div>
                                </div>
                                {/* Video icon */}
                                <div className="text-8xl opacity-20 text-white">
                                    🎬
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            )}

            {/* Video Modal */}
            {showVideoSection && videoUrl && (
                <VideoModal
                    isOpen={showVideo}
                    onClose={() => setShowVideo(false)}
                    videoUrl={videoUrl}
                    title={language === 'pt' ? `Demonstração Fase ${phaseId} - ${phase.title_pt}` : `Phase ${phaseId} Demo - ${phase.title_en}`}
                    description={language === 'pt'
                        ? 'Veja as funcionalidades principais em ação.'
                        : 'See the core features in action.'}
                />
            )}

            {/* Cost Detail Modal */}
            <CostDetailModal
                isOpen={selectedCostId !== null}
                onClose={() => setSelectedCostId(null)}
                costId={selectedCostId || ''}
            />
        </div>
    );
}

export default PhaseDetailLayout;

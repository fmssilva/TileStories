/**
 * PHASE TABS VIEW COMPONENT
 * ==========================
 * 
 * Tabbed interface for viewing all 4 development phases
 * Replaces the previous long-scroll layout with organized tabs
 * Includes timeline visualization for context
 * 
 * Layout: Timeline visualization → Tab navigation → Active phase content
 * Design: Phase-colored tabs with smooth transitions
 * Feature: Sticky tabs that compress when scrolling using global sticky system
 */

import { useState, useRef, useEffect } from 'react';
import { useInlineTranslation, useLanguage } from '@/utils/language';
import { useSticky } from '@/layout_and_navigation/sticky';
import { Tabs, TabsList, TabsTrigger, TabsContent } from '@/components/ui';
import { phases } from './utils';
import { getPhaseColor } from './colors';
import { Phase1Detail } from './Phase1Detail';
import { Phase2Detail } from './Phase2Detail';
import { Phase3Detail } from './Phase3Detail';
import { Phase4Detail } from './Phase4Detail';

export function PhaseTabsView() {
    const { language } = useLanguage();
    const [activeTab, setActiveTab] = useState<string>('phase-1');
    const sectionRef = useRef<HTMLElement>(null);
    const backgroundRef = useRef<HTMLDivElement>(null);
    const tabsListRef = useRef<HTMLDivElement>(null);

    // Use the global sticky system
    const { isSticky, stickyClasses, stickyStyles } = useSticky('work-plan-phase-tabs');

    // Debug: Log sticky state changes and measure actual button heights
    useEffect(() => {
        console.log('🎯 [PhaseTabsView] Sticky state changed:', isSticky);

        if (isSticky && backgroundRef.current && tabsListRef.current) {
            // Log dimensions after render
            requestAnimationFrame(() => {
                const bgHeight = backgroundRef.current?.offsetHeight;
                const bgWidth = backgroundRef.current?.offsetWidth;
                const tabsHeight = tabsListRef.current?.offsetHeight;
                const tabsWidth = tabsListRef.current?.offsetWidth;

                // Get background padding
                const bgStyles = window.getComputedStyle(backgroundRef.current!);
                const bgPaddingTop = parseFloat(bgStyles.paddingTop);
                const bgPaddingBottom = parseFloat(bgStyles.paddingBottom);
                const totalBgPadding = bgPaddingTop + bgPaddingBottom;

                // Measure individual tab buttons
                const tabButtons = tabsListRef.current?.querySelectorAll('[role="tab"]');
                const buttonHeights: number[] = [];
                tabButtons?.forEach((button, index) => {
                    const rect = button.getBoundingClientRect();
                    const styles = window.getComputedStyle(button as Element);
                    buttonHeights.push(rect.height);
                    if (index === 0) {
                        console.log('� [PhaseTabsView] First TabsTrigger button:', {
                            offsetHeight: (button as HTMLElement).offsetHeight,
                            boundingHeight: rect.height,
                            padding: styles.padding,
                            border: styles.border,
                            transform: styles.transform,
                            boxShadow: styles.boxShadow
                        });
                    }
                });
                const maxButtonHeight = Math.max(...buttonHeights);

                console.log('�📦 [PhaseTabsView] Background div:', {
                    height: bgHeight,
                    width: bgWidth,
                    paddingTop: bgPaddingTop,
                    paddingBottom: bgPaddingBottom,
                    totalPadding: totalBgPadding,
                    contentAreaHeight: bgHeight ? bgHeight - totalBgPadding : 'unknown'
                });
                console.log('📊 [PhaseTabsView] TabsList container:', {
                    containerHeight: tabsHeight,
                    containerWidth: tabsWidth,
                    gap: window.getComputedStyle(tabsListRef.current!).gap
                });
                console.log('🔘 [PhaseTabsView] Tab Buttons:', {
                    buttonCount: buttonHeights.length,
                    allButtonHeights: buttonHeights,
                    maxButtonHeight: maxButtonHeight,
                    tallestButton: `${maxButtonHeight}px`
                });
                console.log('⚠️ [PhaseTabsView] Coverage Analysis:', {
                    backgroundTotalHeight: bgHeight,
                    backgroundContentArea: bgHeight ? bgHeight - totalBgPadding : 'unknown',
                    tabsListContainer: tabsHeight,
                    actualButtonHeight: maxButtonHeight,
                    coverageIssue: bgHeight && maxButtonHeight ? (bgHeight - totalBgPadding) < maxButtonHeight : 'unknown',
                    shortfall: bgHeight && maxButtonHeight ? maxButtonHeight - (bgHeight - totalBgPadding) : 'unknown'
                });
            });
        }
    }, [isSticky]);

    const sectionTitle = useInlineTranslation(
        'Fases de Desenvolvimento',
        'Development Phases'
    );

    const monthsLabel = useInlineTranslation('Meses', 'Months');

    // Handle tab change with scroll to section top
    const handleTabChange = (newTab: string) => {
        setActiveTab(newTab);

        // Scroll to the top of the section when changing tabs
        if (sectionRef.current) {
            const headerOffset = 80; // Account for header height + some padding
            const sectionTop = sectionRef.current.getBoundingClientRect().top + window.scrollY - headerOffset;

            window.scrollTo({
                top: sectionTop,
                behavior: 'smooth'
            });
        }
    };

    return (
        <section
            ref={sectionRef}
            className="py-16 sm:py-20 bg-gradient-to-b from-azulejo-ivory-100 via-azulejo-ivory-50 to-white dark:from-gray-900 dark:via-gray-850 dark:to-gray-900"
            aria-labelledby="phases-heading"
        >
            <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
                {/* Section Title */}
                <div className="text-center mb-12">
                    <h2
                        id="phases-heading"
                        className="text-3xl sm:text-4xl font-bold mb-6 text-gray-900 dark:text-white"
                    >
                        {sectionTitle}
                    </h2>
                </div>

                {/* Sticky Tabs Container - Now using global sticky system */}
                <Tabs value={activeTab} onValueChange={handleTabChange} className="w-full">
                    <div
                        ref={backgroundRef}
                        className={`transition-all duration-300 mb-8 flex flex-col
                                   ${isSticky ? 'bg-white/95 dark:bg-gray-900/95 backdrop-blur-md shadow-lg rounded-xl -mx-4 px-4 py-8' : ''}
                                   ${stickyClasses}`}
                        style={stickyStyles}
                    >
                        {/* Phase Navigation - Compact Minimalist Tabs (Sticky Mode) */}
                        {isSticky ? (
                            <TabsList
                                ref={tabsListRef}
                                variant="buttons"
                                size="compact"
                                className="w-full grid grid-cols-4 gap-2 max-w-4xl mx-auto"
                            >
                                {phases.map((phase) => (
                                    <TabsTrigger
                                        key={phase.id}
                                        value={`phase-${phase.id}`}
                                        layout="vertical"
                                        className="p-3 text-center transition-all duration-200 data-[state=active]:scale-105"
                                        style={{
                                            backgroundColor: activeTab === `phase-${phase.id}` ? getPhaseColor(phase.id) : 'transparent',
                                            borderColor: getPhaseColor(phase.id),
                                            color: activeTab === `phase-${phase.id}` ? 'white' : getPhaseColor(phase.id)
                                        }}
                                        aria-label={`View ${language === 'pt' ? phase.title_pt : phase.title_en}`}
                                    >
                                        <div className="font-bold text-xl">{phase.id}</div>
                                        <div className="text-xs font-medium mt-1">{phase.months}m</div>
                                    </TabsTrigger>
                                ))}
                            </TabsList>
                        ) : (
                            <>
                                {/* Phase Navigation - Large Desktop (Non-Sticky) */}
                                <div className="hidden lg:block mb-12">
                                    <div className="relative">
                                        {/* Progress Line */}
                                        <div className="absolute top-12 left-0 right-0 h-1 bg-gray-300 dark:bg-gray-700" />

                                        {/* Phase Buttons */}
                                        <div className="relative flex justify-between items-start">
                                            {phases.map((phase) => (
                                                <button
                                                    key={phase.id}
                                                    onClick={() => handleTabChange(`phase-${phase.id}`)}
                                                    className="relative z-10 flex flex-col items-center cursor-pointer group 
                                                         transition-all duration-300 hover:-translate-y-2"
                                                    aria-label={`View ${language === 'pt' ? phase.title_pt : phase.title_en}`}
                                                    aria-pressed={activeTab === `phase-${phase.id}`}
                                                >
                                                    {/* Larger Circle Button */}
                                                    <div
                                                        className={`w-24 h-24 rounded-full flex items-center justify-center
                                                             text-white font-bold text-3xl border-4 border-white dark:border-gray-900
                                                             shadow-xl transition-all duration-300
                                                             ${activeTab === `phase-${phase.id}`
                                                                ? 'scale-110 shadow-2xl ring-4 ring-opacity-30'
                                                                : 'scale-100 group-hover:scale-105 group-hover:shadow-2xl'}`}
                                                        style={{
                                                            backgroundColor: getPhaseColor(phase.id),
                                                            ...(activeTab === `phase-${phase.id}` ? { boxShadow: `0 0 0 4px ${getPhaseColor(phase.id)}33` } : {})
                                                        }}
                                                    >
                                                        {phase.id}
                                                    </div>

                                                    {/* Phase Info */}
                                                    <div className="mt-6 text-center max-w-[220px]">
                                                        <p className={`font-bold text-base mb-1 transition-colors duration-300
                                                                 ${activeTab === `phase-${phase.id}` ? '' : 'text-gray-700 dark:text-gray-300'}`}
                                                            style={{ color: activeTab === `phase-${phase.id}` ? getPhaseColor(phase.id) : undefined }}>
                                                            {language === 'pt' ? phase.title_pt : phase.title_en}
                                                        </p>
                                                        <p className="text-sm text-gray-600 dark:text-gray-400 font-medium">
                                                            {monthsLabel} {phase.months}
                                                        </p>
                                                        <p
                                                            className="text-base font-bold mt-1"
                                                            style={{ color: getPhaseColor(phase.id) }}
                                                        >
                                                            €{phase.cost}
                                                        </p>
                                                    </div>
                                                </button>
                                            ))}
                                        </div>
                                    </div>
                                </div>

                                {/* Phase Navigation - Mobile (Non-Sticky) */}
                                <TabsList variant="buttons" size="default" className="lg:hidden grid grid-cols-2 sm:grid-cols-4 gap-3 mb-8">
                                    {phases.map((phase) => (
                                        <TabsTrigger
                                            key={phase.id}
                                            value={`phase-${phase.id}`}
                                            layout="vertical"
                                            className="p-4 text-center transition-all duration-300 data-[state=active]:scale-105"
                                            style={{
                                                backgroundColor: activeTab === `phase-${phase.id}` ? getPhaseColor(phase.id) : 'transparent',
                                                borderWidth: '3px',
                                                borderColor: getPhaseColor(phase.id),
                                                color: activeTab === `phase-${phase.id}` ? 'white' : getPhaseColor(phase.id)
                                            }}
                                        >
                                            <div className="font-bold text-2xl mb-1">{phase.id}</div>
                                            <div className="text-xs font-semibold">{phase.months}m</div>
                                        </TabsTrigger>
                                    ))}
                                </TabsList>
                            </>
                        )}
                    </div>

                    {/* Phase Content - Card Container with Visual Boundaries */}
                    <div className="relative">
                        {/* Active Phase Color Indicator - Left Border */}
                        <div
                            className="absolute left-0 top-0 bottom-0 w-1.5 rounded-l-xl transition-colors duration-300"
                            style={{
                                backgroundColor: getPhaseColor(
                                    parseInt(activeTab.split('-')[1] || '1') as 1 | 2 | 3 | 4
                                )
                            }}
                        />

                        {/* Content Container */}
                        <div className="bg-white dark:bg-gray-950 rounded-xl shadow-2xl border border-gray-200 dark:border-gray-800
                                  p-8 sm:p-10 lg:p-12 ml-1.5">
                            {/* Tab Content */}
                            <TabsContent value="phase-1" className="mt-0">
                                <Phase1Detail />
                            </TabsContent>

                            <TabsContent value="phase-2" className="mt-0">
                                <Phase2Detail />
                            </TabsContent>

                            <TabsContent value="phase-3" className="mt-0">
                                <Phase3Detail />
                            </TabsContent>

                            <TabsContent value="phase-4" className="mt-0">
                                <Phase4Detail />
                            </TabsContent>
                        </div>
                    </div>
                </Tabs>
            </div>
        </section>
    );
}

export default PhaseTabsView;

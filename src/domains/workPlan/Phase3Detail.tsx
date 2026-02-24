/**
 * PHASE 3: ADVANCED FEATURES DETAIL SECTION
 * ==========================================
 * 
 * Advanced Features Phase - Now using unified PhaseDetailLayout component
 * 
 * Features:
 * - Consistent Phase4-style card formatting
 * - Clickable tech stack with cost modals
 * - Custom earthquake simulation highlight banner
 * - Responsive design
 */

import { useInlineTranslation, useLanguage } from '@/utils/language';
import PhaseDetailLayout from './PhaseDetailLayout';
import { getPhaseColor } from './colors';

export function Phase3Detail() {
    const { language } = useLanguage();
    const highlightTitle = useInlineTranslation(
        'Destaque: Simulação de Terramotos',
        'Highlight: Earthquake Simulation'
    );

    // Custom highlight banner for earthquake simulation
    const earthquakeHighlight = (
        <div
            className="p-8 rounded-2xl text-white text-center"
            style={{
                background: `linear-gradient(135deg, ${getPhaseColor(3)} 0%, ${getPhaseColor(
                    3
                )}dd 100%)`
            }}
        >
            <h3 className="text-2xl font-bold mb-3">{highlightTitle}</h3>
            <p className="text-lg max-w-3xl mx-auto">
                {language === 'pt'
                    ? 'Recurso inovador que permite aos visitantes visualizar o impacto de eventos históricos em azulejos portugueses. Combina tecnologia AR com narrativas históricas para criar experiências educacionais envolventes.'
                    : 'Innovative feature allowing visitors to visualize the impact of historical events on Portuguese tiles. Combines AR technology with historical narratives to create engaging educational experiences.'}
            </p>
        </div>
    );

    return (
        <PhaseDetailLayout
            phaseId={3}
            featureImage="/images/earthquake.png"
            featureImageAlt_pt="Demonstração AR - Simulação do terramoto de 1755"
            featureImageAlt_en="AR Demo - 1755 earthquake simulation"
            featureCaption_pt="✨ Recursos Avançados: Simulação do terramoto de 1755"
            featureCaption_en="✨ Advanced Features: 1755 earthquake simulation"
            customHighlight={earthquakeHighlight}
        />
    );
}

export default Phase3Detail;


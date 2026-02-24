/**
 * PHASE 2: CORE DETAIL SECTION
 * =============================
 * 
 * Core Enhancement Phase - Now using unified PhaseDetailLayout component
 * 
 * Features:
 * - Consistent Phase4-style card formatting
 * - Clickable tech stack with cost modals
 * - Lisbon_AR_video.mp4 demo video
 * - Responsive design
 */

import PhaseDetailLayout from './PhaseDetailLayout';

export function Phase2Detail() {
    return (
        <PhaseDetailLayout
            phaseId={2}
            featureImage="/videos/Lisbon_AR_video.mp4"
            featureImageAlt_pt="Demonstração AR - Ícones interativos sobre o panorama histórico"
            featureImageAlt_en="AR Demo - Interactive icons over historical panorama"
            featureCaption_pt="🏛️ Experiência Principal: Navegação educacional com ícones AR"
            featureCaption_en="🏛️ Core Experience: Educational navigation with AR icons"
        />
    );
}

export default Phase2Detail;


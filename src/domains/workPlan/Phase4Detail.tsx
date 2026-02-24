/**
 * PHASE 4: OPTIMIZATION & POLISH DETAIL SECTION
 * ==============================================
 * 
 * Optimization Phase - Now using unified PhaseDetailLayout component
 * 
 * Features:
 * - Consistent Phase4-style card formatting
 * - Clickable tech stack with cost modals
 * - Lisbon_AR_vide_4_views.mp4 final product demo
 * - Responsive design
 */

import PhaseDetailLayout from './PhaseDetailLayout';

export function Phase4Detail() {
    return (
        <PhaseDetailLayout
            phaseId={4}
            featureImage="/videos/Lisbon_AR_vide_4_views.mp4"
            featureImageAlt_pt="Grande Panorama de Lisboa - Produto final otimizado"
            featureImageAlt_en="Grande Panorama de Lisboa - Final optimized product"
            featureCaption_pt="🎨 Produto Final: Experiência completa e polida do Grande Panorama"
            featureCaption_en="🎨 Final Product: Complete and polished Grande Panorama experience"
        />
    );
}

export default Phase4Detail;


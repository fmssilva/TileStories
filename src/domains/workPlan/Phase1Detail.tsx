/**
 * PHASE 1: MVP DETAIL SECTION
 * ============================
 * 
 * Foundation Phase - Now using unified PhaseDetailLayout component
 * 
 * Features:
 * - Consistent Phase4-style card formatting
 * - Clickable tech stack with cost modals
 * - MostJeron.mp4 video demo instead of static image
 * - Responsive design
 */

import PhaseDetailLayout from './PhaseDetailLayout';

export function Phase1Detail() {
    return (
        <PhaseDetailLayout
            phaseId={1}
            featureImage="/videos/MostJeron.mp4"
            featureImageAlt_pt="Demonstração AR - Interação com azulejos históricos"
            featureImageAlt_en="AR Demo - Interacting with historical azulejo tiles"
            featureCaption_pt="🎯 MVP: Interação AR com azulejos históricos"
            featureCaption_en="🎯 MVP: AR Interaction with Historical Tiles"
            showVideoSection={false}
        />
    );
}

export default Phase1Detail;


/**
 * WORK PLAN DOMAIN - DATA UTILITIES
 * ===================================
 * 
 * Phase data and cost breakdown for the 12-month development plan.
 * Data source: App_Plan.md
 * 
 * Total Investment: €3,461 (rounded to €3,500 with buffer)
 */

import type { Phase, CostCategory } from './types';

/**
 * All 4 development phases with complete details
 */
export const phases: Phase[] = [
    // ========================================================================
    // PHASE 1: MVP FUNCIONAL (Months 1-3)
    // ========================================================================
    {
        id: 1,
        title_pt: 'Fase 1 - MVP Funcional',
        title_en: 'Phase 1 - Functional MVP',
        subtitle_pt: 'Prove It Works',
        subtitle_en: 'Prove It Works',
        months: '1-3',
        cost: 541,
        deliverables_pt: [
            'AR básico com 30 edifícios identificáveis',
            'Timeline temporal simples (3 épocas)',
            'Info cards com história e localização',
        ],
        deliverables_en: [
            'Basic AR with 30 identifiable buildings',
            'Simple timeline (3 epochs)',
            'Info cards with history and location',
        ],
        value_pt: 'Em vez de ver parede estática, consigo identificar instantaneamente qualquer edifício, saber a sua história, e descobrir se ainda existe hoje em Lisboa.',
        value_en: 'Instead of seeing a static wall, I can instantly identify any building, learn its history, and discover if it still exists in Lisbon today.',
        techStack: [
            { name: 'Apple Developer', cost: 99 },
            { name: 'Google Play', cost: 25 },
            { name: 'GitHub Copilot Pro+ (3 meses)', cost: 117 },
            { name: 'Assets ícones UI', cost: 100 },
            { name: 'Imagens históricas stock', cost: 200 },
        ],
        milestones: [
            { month: 1, description_pt: 'Setup técnico + MoU museu + fotografias HD painel', description_en: 'Technical setup + museum MoU + HD panel photos' },
            { month: 2, description_pt: 'AR tracking básico + 10 edifícios com info cards + Google Maps', description_en: 'Basic AR tracking + 10 buildings with info cards + Google Maps' },
            { month: 3, description_pt: '30 edifícios + timeline temporal + sistema zoom + beta testing', description_en: '30 buildings + temporal timeline + zoom system + beta testing' },
        ],
    },

    // ========================================================================
    // PHASE 2: EXPERIÊNCIA CORE (Months 4-6)
    // ========================================================================
    {
        id: 2,
        title_pt: 'Fase 2 - Experiência Core',
        title_en: 'Phase 2 - Core Experience',
        subtitle_pt: 'Make It Engaging',
        subtitle_en: 'Make It Engaging',
        months: '4-6',
        cost: 617,
        deliverables_pt: [
            'Perfis académicos personalizados',
            'Guia áudio profissional (20 clips)',
            '5 circuitos temáticos',
            '5 edifícios Unity 3D premium',
            'Gamificação (badges, quiz, leaderboard)',
        ],
        deliverables_en: [
            'Personalized academic profiles',
            'Professional audio guide (20 clips)',
            '5 thematic circuits',
            '5 premium Unity 3D buildings',
            'Gamification (badges, quiz, leaderboard)',
        ],
        value_pt: 'A app sabe que sou estudante de arquitetura e mostra-me análises de proporções e estilos. O guia áudio vai-me contando histórias enquanto sigo o circuito recomendado.',
        value_en: 'The app knows I\'m an architecture student and shows me proportion and style analyses. The audio guide tells me stories as I follow the recommended circuit.',
        techStack: [
            { name: 'Copilot Pro+ (3 meses)', cost: 117 },
            { name: 'Voice actor profissional (Fiverr)', cost: 250 },
            { name: 'Unity assets 3D base (5 edifícios)', cost: 200 },
            { name: 'Texturas/materiais custom', cost: 50 },
        ],
        milestones: [
            { month: 4, description_pt: 'Perfis académicos personalizados + guia áudio (20 clips)', description_en: 'Personalized academic profiles + audio guide (20 clips)' },
            { month: 5, description_pt: '5 circuitos temáticos + gamificação (badges, quiz, leaderboard)', description_en: '5 thematic circuits + gamification (badges, quiz, leaderboard)' },
            { month: 6, description_pt: 'Unity integration + 5 edifícios 3D premium interativos', description_en: 'Unity integration + 5 premium interactive 3D buildings' },
        ],
    },

    // ========================================================================
    // PHASE 3: ADVANCED FEATURES (Months 7-9)
    // ========================================================================
    {
        id: 3,
        title_pt: 'Fase 3 - Recursos Avançados',
        title_en: 'Phase 3 - Advanced Features',
        subtitle_pt: 'Make It Unforgettable',
        subtitle_en: 'Make It Unforgettable',
        months: '7-9',
        cost: 1022,
        deliverables_pt: [
            'Simulação Terramoto 1755 (3min imersivo)',
            'GPT-4 Q&A conversacional',
            '100 edifícios completos (expansão de 30→100)',
            '360° interior views (5 edifícios)',
            'Analytics & heatmaps para tese',
        ],
        deliverables_en: [
            '1755 Earthquake simulation (3min immersive)',
            'GPT-4 conversational Q&A',
            '100 complete buildings (expansion 30→100)',
            '360° interior views (5 buildings)',
            'Analytics & heatmaps for thesis',
        ],
        value_pt: 'Vivo o terramoto de 1755 - sinto o chão tremer, vejo edifícios cair em tempo real, ouço o caos. Depois posso fazer perguntas à app e recebo resposta inteligente.',
        value_en: 'I experience the 1755 earthquake - feel the ground shake, see buildings collapse in real time, hear the chaos. Then I can ask the app questions and get intelligent answers.',
        techStack: [
            { name: 'Copilot Pro+ (3 meses)', cost: 117 },
            { name: 'Unity assets terramoto (particles, destruction)', cost: 95 },
            { name: 'Assets Lisboa 1755 (edifícios, props)', cost: 300 },
            { name: 'Sound effects profissionais', cost: 50 },
            { name: 'OpenAI API (GPT-4o-mini)', cost: 15 },
            { name: 'Google TTS premium', cost: 45 },
            { name: '360° assets/renders', cost: 200 },
            { name: 'Imagens & Conteúdo histórico', cost: 200 },
        ],
        milestones: [
            { month: 7, description_pt: 'Simulação terramoto 1755 completa (5 fases, physics, áudio)', description_en: 'Complete 1755 earthquake simulation (5 phases, physics, audio)' },
            { month: 8, description_pt: 'GPT-4 Q&A conversacional + expansão para 100 edifícios', description_en: 'GPT-4 conversational Q&A + expansion to 100 buildings' },
            { month: 9, description_pt: 'Sistema analytics (heatmaps, A/B testing) + 360° views (5 edifícios)', description_en: 'Analytics system (heatmaps, A/B testing) + 360° views (5 buildings)' },
        ],
    },

    // ========================================================================
    // PHASE 4: OPTIMIZATION & POLISH (Months 10-12)
    // ========================================================================
    {
        id: 4,
        title_pt: 'Fase 4 - Otimização e Acabamento',
        title_en: 'Phase 4 - Optimization & Polish',
        subtitle_pt: 'Make It Shine',
        subtitle_en: 'Make It Shine',
        months: '10-12',
        cost: 967,
        deliverables_pt: [
            '150 edifícios finalizados (cobertura total)',
            'Multilíngue (Português, Inglês, Espanhol)',
            'Acessibilidade completa (WCAG AA)',
            'Optimização (<100MB app, <3s loading)',
            'Publicação App Store + Google Play',
            'Documentação tese (100 páginas, 1000+ visitantes)',
        ],
        deliverables_en: [
            '150 finalized buildings (full coverage)',
            'Multilingual (Portuguese, English, Spanish)',
            'Full accessibility (WCAG AA)',
            'Optimization (<100MB app, <3s loading)',
            'App Store + Google Play publication',
            'Thesis documentation (100 pages, 1000+ visitors)',
        ],
        value_pt: 'App está em inglês para mim (turista). Funciona super rápido mesmo no meu telemóvel de 2021. Minha avó com visão reduzida consegue usar com narração de voz.',
        value_en: 'App is in English for me (tourist). Works super fast even on my 2021 phone. My grandmother with reduced vision can use it with voice narration.',
        techStack: [
            { name: 'Copilot Pro+ (3 meses)', cost: 117 },
            { name: 'Traduções profissionais (EN/ES)', cost: 200 },
            { name: 'Incentivos beta testers', cost: 100 },
            { name: 'Video demo profissional', cost: 50 },
            { name: 'Assets finais (50 edifícios restantes)', cost: 300 },
            { name: 'Contingência/imprevistos', cost: 200 },
        ],
        milestones: [
            { month: 10, description_pt: 'Optimização (<100MB, <3s loading) + testes extensivos (50 beta testers)', description_en: 'Optimization (<100MB, <3s loading) + extensive testing (50 beta testers)' },
            { month: 11, description_pt: '150 edifícios completos + acessibilidade WCAG AA + multilíngue (PT/EN/ES)', description_en: '150 complete buildings + WCAG AA accessibility + multilingual (PT/EN/ES)' },
            { month: 12, description_pt: 'Publicação App Store/Google Play + documentação tese + analytics 1000+ users', description_en: 'App Store/Google Play launch + thesis documentation + 1000+ users analytics' },
        ],
    },
];

/**
 * Cost breakdown by category across all phases
 */
export const costBreakdown: CostCategory[] = [
    {
        category_pt: 'Copilot Pro+',
        category_en: 'Copilot Pro+',
        phase1: 117,
        phase2: 117,
        phase3: 117,
        phase4: 117,
        total: 468,
    },
    {
        category_pt: 'Store Fees',
        category_en: 'Store Fees',
        phase1: 124,
        phase2: 0,
        phase3: 0,
        phase4: 0,
        total: 124,
    },
    {
        category_pt: 'Assets 3D & Audio',
        category_en: 'Assets 3D & Audio',
        phase1: 100,
        phase2: 250,
        phase3: 645,
        phase4: 300,
        total: 1295,
    },
    {
        category_pt: 'Serviços IA (GPT, TTS)',
        category_en: 'AI Services (GPT, TTS)',
        phase1: 0,
        phase2: 0,
        phase3: 60,
        phase4: 0,
        total: 60,
    },
    {
        category_pt: 'Traduções & Localization',
        category_en: 'Translations & Localization',
        phase1: 0,
        phase2: 0,
        phase3: 0,
        phase4: 200,
        total: 200,
    },
    {
        category_pt: 'Imagens & Conteúdo',
        category_en: 'Images & Content',
        phase1: 200,
        phase2: 50,
        phase3: 200,
        phase4: 0,
        total: 450,
    },
    {
        category_pt: 'Testing & Video',
        category_en: 'Testing & Video',
        phase1: 0,
        phase2: 0,
        phase3: 0,
        phase4: 150,
        total: 150,
    },
    {
        category_pt: 'Contingência',
        category_en: 'Contingency',
        phase1: 0,
        phase2: 0,
        phase3: 0,
        phase4: 200,
        total: 200,
    },
];

/**
 * Calculate total cost across all phases
 */
export function getTotalCost(): number {
    return phases.reduce((sum, phase) => sum + phase.cost, 0);
}

/**
 * Get phase by ID
 */
export function getPhaseById(id: 1 | 2 | 3 | 4): Phase | undefined {
    return phases.find(phase => phase.id === id);
}

/**
 * Project metadata
 */
export const projectMetadata = {
    totalCost: 3147,
    totalCostWithBuffer: 3500,
    buffer: 314,
    duration: 12, // months
    phaseCount: 4,
    mvpMonth: 6,
    finalDeliveryMonth: 12,
} as const;

/**
 * TECH STACK TO COST ID MAPPING
 * ==============================
 * 
 * Maps tech stack item names to their corresponding cost detail IDs.
 * Centralized mapping used by all Phase detail components.
 */

/**
 * Get cost detail ID for a tech stack item name
 */
export function getCostIdForTechStack(techName: string, phaseId: number): string {
    const mappings: Record<string, string> = {
        // Phase 1
        'Apple Developer': 'apple-developer',
        'Google Play': 'google-play',
        'GitHub Copilot Pro+ (3 meses)': `copilot-phase${phaseId}`,
        'Assets ícones UI': 'assets-icons-ui',
        'Imagens históricas stock': 'historical-images',

        // Phase 2
        'Copilot Pro+ (3 meses)': `copilot-phase${phaseId}`,
        'Voice actor profissional (Fiverr)': 'voice-actor',
        'Unity assets 3D base (5 edifícios)': 'unity-assets',
        'Texturas/materiais custom': 'custom-textures',

        // Phase 3
        'Unity assets terramoto (particles, destruction)': 'earthquake-assets',
        'Assets Lisboa 1755 (edifícios, props)': 'lisbon-1755-assets',
        'Sound effects profissionais': 'sound-effects',
        'OpenAI API (GPT-4o-mini)': 'openai-api',
        'Google TTS premium': 'google-tts',
        '360° assets/renders': '360-assets',
        'Imagens & Conteúdo histórico': 'historical-content',

        // Phase 4
        'Traduções profissionais (EN/ES)': 'professional-translations',
        'Incentivos beta testers': 'beta-tester-incentives',
        'Video demo profissional': 'demo-video',
        'Assets finais (50 edifícios restantes)': 'final-assets',
        'Contingência/imprevistos': 'contingency',
    };

    return mappings[techName] || '';
}

/**
 * Get cost detail ID for a financial breakdown category
 */
export function getCostIdForCategory(categoryName: string): string {
    // Since category rows span multiple phases, we'll show the first instance
    const categoryMappings: Record<string, string> = {
        'Copilot Pro+': 'copilot-phase1',
        'Store Fees': 'apple-developer',
        'Assets 3D & Audio': 'assets-icons-ui',
        'Serviços IA (GPT, TTS)': 'openai-api',
        'Traduções & Localization': 'professional-translations',
        'Translations & Localization': 'professional-translations',
        'Imagens & Conteúdo': 'historical-images',
        'Images & Content': 'historical-images',
        'Testing & Video': 'beta-tester-incentives',
        'Contingência': 'contingency',
        'Contingency': 'contingency',
    };

    return categoryMappings[categoryName] || '';
}

/**
 * COST DETAILS DATA
 * ==================
 * 
 * Detailed information about each cost item for display in modals.
 * Extracted from App_Plan_Details.md
 * 
 * Structure:
 * - Tech stack items (Apple Developer, Copilot, etc.)
 * - Cost categories (Store Fees, Assets, AI Services, etc.)
 */

export interface CostDetail {
    id: string; // Unique identifier
    name_pt: string;
    name_en: string;
    cost: number;
    category_pt: string;
    category_en: string;
    description_pt: string;
    description_en: string;
    justification_pt: string;
    justification_en: string;
    advantages_pt: string[];
    advantages_en: string[];
    alternatives_pt?: string;
    alternatives_en?: string;
}

/**
 * All cost details mapped by ID
 */
export const costDetailsMap: Record<string, CostDetail> = {
    // ========================================================================
    // PHASE 1 TECH STACK
    // ========================================================================
    'apple-developer': {
        id: 'apple-developer',
        name_pt: 'Apple Developer',
        name_en: 'Apple Developer',
        cost: 99,
        category_pt: 'Store Fees',
        category_en: 'Store Fees',
        description_pt: 'Conta de desenvolvedor Apple necessária para publicar apps na App Store',
        description_en: 'Apple developer account required to publish apps on the App Store',
        justification_pt: 'Essencial para distribuir a app para dispositivos iOS (iPhone/iPad). Sem esta conta, não é possível publicar na App Store oficial, limitando severamente o alcance do projeto.',
        justification_en: 'Essential to distribute the app for iOS devices (iPhone/iPad). Without this account, it\'s impossible to publish on the official App Store, severely limiting the project\'s reach.',
        advantages_pt: [
            'Acesso a milhões de utilizadores iOS',
            'Ferramentas de desenvolvimento profissionais (Xcode, TestFlight)',
            'Analytics e crash reports integrados',
            'Distribuição oficial e confiável',
            'Suporte técnico da Apple'
        ],
        advantages_en: [
            'Access to millions of iOS users',
            'Professional development tools (Xcode, TestFlight)',
            'Integrated analytics and crash reports',
            'Official and trusted distribution',
            'Apple technical support'
        ],
        alternatives_pt: 'Não publicar para iOS (perderia 50% do mercado português de smartphones)',
        alternatives_en: 'Not publishing for iOS (would lose 50% of Portuguese smartphone market)',
    },

    'google-play': {
        id: 'google-play',
        name_pt: 'Google Play',
        name_en: 'Google Play',
        cost: 25,
        category_pt: 'Store Fees',
        category_en: 'Store Fees',
        description_pt: 'Taxa única para conta de desenvolvedor Google Play',
        description_en: 'One-time fee for Google Play developer account',
        justification_pt: 'Necessária para publicar na Google Play Store (Android). Pagamento único que permite publicar apps ilimitadas. Android representa ~50% do mercado português.',
        justification_en: 'Required to publish on Google Play Store (Android). One-time payment allowing unlimited app publications. Android represents ~50% of Portuguese market.',
        advantages_pt: [
            'Acesso ao maior mercado de smartphones do mundo',
            'Pagamento único (não anual como Apple)',
            'Google Play Console com analytics detalhados',
            'Testes A/B integrados',
            'Distribuição global instantânea'
        ],
        advantages_en: [
            'Access to the world\'s largest smartphone market',
            'One-time payment (not annual like Apple)',
            'Google Play Console with detailed analytics',
            'Integrated A/B testing',
            'Instant global distribution'
        ],
        alternatives_pt: 'APK direto (inseguro, sem updates automáticos, violaria política do museu)',
        alternatives_en: 'Direct APK (insecure, no automatic updates, would violate museum policy)',
    },

    'copilot-phase1': {
        id: 'copilot-phase1',
        name_pt: 'GitHub Copilot Pro+ (3 meses)',
        name_en: 'GitHub Copilot Pro+ (3 months)',
        cost: 117,
        category_pt: 'Copilot Pro+',
        category_en: 'Copilot Pro+',
        description_pt: 'Assistente de IA para programação durante os primeiros 3 meses',
        description_en: 'AI programming assistant for the first 3 months',
        justification_pt: 'Acelera desenvolvimento em 30-40% segundo estudos (GitHub, 2024). Para uma tese de 1 ano com escopo ambicioso, esta aceleração é crítica para cumprir prazos.',
        justification_en: 'Accelerates development by 30-40% according to studies (GitHub, 2024). For a 1-year thesis with ambitious scope, this acceleration is critical to meet deadlines.',
        advantages_pt: [
            'Acelera desenvolvimento 30-40% (estudos GitHub)',
            'Reduz bugs com sugestões testadas',
            'Aprende padrões do projeto',
            'Gera testes automáticos',
            'Documentação inline',
            'Poupa ~100h de desenvolvimento'
        ],
        advantages_en: [
            'Accelerates development by 30-40% (GitHub studies)',
            'Reduces bugs with tested suggestions',
            'Learns project patterns',
            'Generates automatic tests',
            'Inline documentation',
            'Saves ~100h of development'
        ],
        alternatives_pt: 'Programação manual (3-4 meses extra, risco de não concluir tese a tempo)',
        alternatives_en: 'Manual programming (3-4 extra months, risk of not completing thesis on time)',
    },

    'assets-icons-ui': {
        id: 'assets-icons-ui',
        name_pt: 'Assets ícones UI',
        name_en: 'UI icon assets',
        cost: 100,
        category_pt: 'Assets 3D & Audio',
        category_en: 'Assets 3D & Audio',
        description_pt: 'Pacote profissional de ícones para interface da app',
        description_en: 'Professional icon pack for app interface',
        justification_pt: 'Interface profissional transmite credibilidade ao museu e utilizadores. Ícones custom-made levariam ~40h de design. Pacote premium (SF Symbols Pro + Material Design) custa €100 mas poupa tempo e garante consistência.',
        justification_en: 'Professional interface conveys credibility to museum and users. Custom-made icons would take ~40h of design. Premium pack (SF Symbols Pro + Material Design) costs €100 but saves time and ensures consistency.',
        advantages_pt: [
            'Consistência visual iOS + Android',
            'Poupa ~40h de design',
            'Ícones otimizados (SVG, múltiplos tamanhos)',
            'Acessibilidade garantida',
            'Updates gratuitos',
            'Licença comercial incluída'
        ],
        advantages_en: [
            'Visual consistency iOS + Android',
            'Saves ~40h of design',
            'Optimized icons (SVG, multiple sizes)',
            'Guaranteed accessibility',
            'Free updates',
            'Commercial license included'
        ],
        alternatives_pt: 'Ícones gratuitos (qualidade inconsistente, requerem atribuição, sem suporte)',
        alternatives_en: 'Free icons (inconsistent quality, require attribution, no support)',
    },

    'historical-images': {
        id: 'historical-images',
        name_pt: 'Imagens históricas stock',
        name_en: 'Historical stock images',
        cost: 200,
        category_pt: 'Imagens & Conteúdo',
        category_en: 'Images & Content',
        description_pt: 'Banco de imagens históricas de Lisboa (gravuras, pinturas, fotografias antigas)',
        description_en: 'Historical image bank of Lisbon (engravings, paintings, old photographs)',
        justification_pt: '30 edifícios precisam de contexto visual histórico. Museu tem algumas imagens, mas não todas. Licenças comerciais de arquivos (Biblioteca Nacional, coleções privadas) custam €5-10/imagem.',
        justification_en: '30 buildings need historical visual context. Museum has some images, but not all. Commercial licenses from archives (National Library, private collections) cost €5-10/image.',
        advantages_pt: [
            'Licenças comerciais legais',
            'Alta resolução (impressão + digital)',
            'Curadoria profissional',
            'Metadados históricos incluídos',
            'Sem preocupações de direitos autorais'
        ],
        advantages_en: [
            'Legal commercial licenses',
            'High resolution (print + digital)',
            'Professional curation',
            'Historical metadata included',
            'No copyright concerns'
        ],
        alternatives_pt: 'Domínio público apenas (cobre ~50% dos edifícios, qualidade variável)',
        alternatives_en: 'Public domain only (covers ~50% of buildings, variable quality)',
    },

    // ========================================================================
    // PHASE 2 TECH STACK
    // ========================================================================
    'copilot-phase2': {
        id: 'copilot-phase2',
        name_pt: 'Copilot Pro+ (3 meses)',
        name_en: 'Copilot Pro+ (3 months)',
        cost: 117,
        category_pt: 'Copilot Pro+',
        category_en: 'Copilot Pro+',
        description_pt: 'Continuação do assistente IA para meses 4-6',
        description_en: 'Continuation of AI assistant for months 4-6',
        justification_pt: 'Fase 2 inclui features complexas (Unity 3D, guia áudio, gamificação). Copilot essencial para integração Flutter-Unity e otimização de performance.',
        justification_en: 'Phase 2 includes complex features (Unity 3D, audio guide, gamification). Copilot essential for Flutter-Unity integration and performance optimization.',
        advantages_pt: [
            'Expertise em Unity + Flutter integration',
            'Otimização automática de código',
            'Debugging acelerado',
            'Mantém consistência com Fase 1'
        ],
        advantages_en: [
            'Expertise in Unity + Flutter integration',
            'Automatic code optimization',
            'Accelerated debugging',
            'Maintains consistency with Phase 1'
        ],
    },

    'voice-actor': {
        id: 'voice-actor',
        name_pt: 'Voice actor profissional (Fiverr)',
        name_en: 'Professional voice actor (Fiverr)',
        cost: 250,
        category_pt: 'Assets 3D & Audio',
        category_en: 'Assets 3D & Audio',
        description_pt: 'Narração profissional para guia áudio (20 clips, ~15 minutos total)',
        description_en: 'Professional narration for audio guide (20 clips, ~15min total)',
        justification_pt: 'Voz humana profissional cria conexão emocional 3x superior a TTS (estudos UX). Custo Fiverr: €250 para 15min narração PT/EN por voice actor com portfólio cultural.',
        justification_en: 'Professional human voice creates 3x superior emotional connection than TTS (UX studies). Fiverr cost: €250 for 15min PT/EN narration by voice actor with cultural portfolio.',
        advantages_pt: [
            'Qualidade profissional de museu',
            'Entonação emocional correta',
            'Pronúncia histórica precisa',
            'Versões PT + EN incluídas',
            'Edição profissional',
            'Uso ilimitado (licença comercial)'
        ],
        advantages_en: [
            'Museum-grade professional quality',
            'Correct emotional intonation',
            'Precise historical pronunciation',
            'PT + EN versions included',
            'Professional editing',
            'Unlimited use (commercial license)'
        ],
        alternatives_pt: 'Google TTS grátis (robótico, sem emoção, prejudica imersão)',
        alternatives_en: 'Free Google TTS (robotic, no emotion, hurts immersion)',
    },

    'unity-assets': {
        id: 'unity-assets',
        name_pt: 'Unity assets 3D base (5 edifícios)',
        name_en: 'Unity 3D base assets (5 buildings)',
        cost: 200,
        category_pt: 'Assets 3D & Audio',
        category_en: 'Assets 3D & Audio',
        description_pt: 'Modelos 3D base profissionais para 5 edifícios principais',
        description_en: 'Professional base 3D models for 5 main buildings',
        justification_pt: 'Modelar 5 edifícios do zero levaria ~80h. Assets Unity Asset Store (Medieval Castle, Gothic Cathedral, etc.) custam €40 cada, adaptados em Blender (~10h). Trade-off: €200 + 10h vs 80h trabalho.',
        justification_en: 'Modeling 5 buildings from scratch would take ~80h. Unity Asset Store assets (Medieval Castle, Gothic Cathedral, etc.) cost €40 each, adapted in Blender (~10h). Trade-off: €200 + 10h vs 80h work.',
        advantages_pt: [
            'Poupa ~70h de modelação 3D',
            'Qualidade profissional garantida',
            'Otimizados para mobile (low-poly)',
            'Texturas PBR incluídas',
            'Documentação completa',
            'Suporte do criador'
        ],
        advantages_en: [
            'Saves ~70h of 3D modeling',
            'Guaranteed professional quality',
            'Optimized for mobile (low-poly)',
            'PBR textures included',
            'Complete documentation',
            'Creator support'
        ],
        alternatives_pt: 'Modelar tudo do zero (inviável em 3 meses + outras features)',
        alternatives_en: 'Model everything from scratch (unfeasible in 3 months + other features)',
    },

    'custom-textures': {
        id: 'custom-textures',
        name_pt: 'Texturas/materiais custom',
        name_en: 'Custom textures/materials',
        cost: 50,
        category_pt: 'Assets 3D & Audio',
        category_en: 'Assets 3D & Audio',
        description_pt: 'Texturas personalizadas para adaptar modelos 3D a edifícios de Lisboa',
        description_en: 'Custom textures to adapt 3D models to Lisbon buildings',
        justification_pt: 'Assets genéricos precisam de texturas específicas Lisboa (pedra lioz, azulejos). Pacotes Substance Source ou TextureHaven Pro fornecem materiais PBR editáveis.',
        justification_en: 'Generic assets need Lisbon-specific textures (lioz stone, tiles). Substance Source or TextureHaven Pro packages provide editable PBR materials.',
        advantages_pt: [
            'Autenticidade histórica',
            'Materiais PBR realistas',
            'Editáveis em Substance Painter',
            '4K resolution',
            'Licença comercial'
        ],
        advantages_en: [
            'Historical authenticity',
            'Realistic PBR materials',
            'Editable in Substance Painter',
            '4K resolution',
            'Commercial license'
        ],
    },

    // ========================================================================
    // PHASE 3 TECH STACK
    // ========================================================================
    'copilot-phase3': {
        id: 'copilot-phase3',
        name_pt: 'Copilot Pro+ (3 meses)',
        name_en: 'Copilot Pro+ (3 months)',
        cost: 117,
        category_pt: 'Copilot Pro+',
        category_en: 'Copilot Pro+',
        description_pt: 'Continuação do assistente IA para meses 7-9 (features avançadas)',
        description_en: 'Continuation of AI assistant for months 7-9 (advanced features)',
        justification_pt: 'Fase mais técnica (physics earthquake, GPT integration, analytics). Copilot crítico para debugging complexo e otimizações.',
        justification_en: 'Most technical phase (earthquake physics, GPT integration, analytics). Copilot critical for complex debugging and optimizations.',
        advantages_pt: [
            'Expertise em integrações complexas',
            'Sugestões de otimização performance',
            'Geração de testes unitários',
            'Documentação automática'
        ],
        advantages_en: [
            'Expertise in complex integrations',
            'Performance optimization suggestions',
            'Unit test generation',
            'Automatic documentation'
        ],
    },

    'earthquake-assets': {
        id: 'earthquake-assets',
        name_pt: 'Unity assets terramoto (particles, destruction)',
        name_en: 'Earthquake Unity assets (particles, destruction)',
        cost: 95,
        category_pt: 'Assets 3D & Audio',
        category_en: 'Assets 3D & Audio',
        description_pt: 'Particle systems (fumo, fogo, poeira) e physics de destruição',
        description_en: 'Particle systems (smoke, fire, dust) and destruction physics',
        justification_pt: 'Sistemas de partículas realistas levam ~30h a criar. Pacotes Asset Store especializados (Realistic FX, Destruction Tools) custam €30-50 cada.',
        justification_en: 'Realistic particle systems take ~30h to create. Specialized Asset Store packages (Realistic FX, Destruction Tools) cost €30-50 each.',
        advantages_pt: [
            'Efeitos realistas profissionais',
            'Performance otimizada mobile',
            'Configuráveis e reutilizáveis',
            'Documentação + exemplos'
        ],
        advantages_en: [
            'Professional realistic effects',
            'Mobile-optimized performance',
            'Configurable and reusable',
            'Documentation + examples'
        ],
    },

    'lisbon-1755-assets': {
        id: 'lisbon-1755-assets',
        name_pt: 'Assets Lisboa 1755 (edifícios, props)',
        name_en: 'Lisbon 1755 assets (buildings, props)',
        cost: 300,
        category_pt: 'Assets 3D & Audio',
        category_en: 'Assets 3D & Audio',
        description_pt: 'Modelos low-poly de 20 edifícios principais pré-terramoto + props (carruagens, barcos)',
        description_en: 'Low-poly models of 20 main pre-earthquake buildings + props (carriages, boats)',
        justification_pt: 'Simulação terramoto precisa de cidade completa (~20 edifícios simplificados). Assets históricos custam ~€15 cada. Alternativa: modelar tudo (120h inviável).',
        justification_en: 'Earthquake simulation needs complete city (~20 simplified buildings). Historical assets cost ~€15 each. Alternative: model everything (120h unfeasible).',
        advantages_pt: [
            'Set completo coerente',
            'Históricidade verificada',
            'Low-poly para performance',
            'Props contextuais incluídos'
        ],
        advantages_en: [
            'Complete coherent set',
            'Verified historicity',
            'Low-poly for performance',
            'Contextual props included'
        ],
    },

    'sound-effects': {
        id: 'sound-effects',
        name_pt: 'Sound effects profissionais',
        name_en: 'Professional sound effects',
        cost: 50,
        category_pt: 'Assets 3D & Audio',
        category_en: 'Assets 3D & Audio',
        description_pt: 'Biblioteca de efeitos sonoros (terramoto, colapsos, tsunami, incêndios)',
        description_en: 'Sound effects library (earthquake, collapses, tsunami, fires)',
        justification_pt: 'Som espacial imersivo crítico para experiência terramoto. Pacotes profissionais (Epidemic Sound, AudioJungle) incluem licença comercial.',
        justification_en: 'Immersive spatial sound critical for earthquake experience. Professional packages (Epidemic Sound, AudioJungle) include commercial license.',
        advantages_pt: [
            'Qualidade cinema/documentário',
            'Espacialização 3D',
            'Licença comercial perpétua',
            'Formatos otimizados mobile'
        ],
        advantages_en: [
            'Cinema/documentary quality',
            '3D spatialization',
            'Perpetual commercial license',
            'Mobile-optimized formats'
        ],
    },

    'openai-api': {
        id: 'openai-api',
        name_pt: 'OpenAI API (GPT-4o-mini)',
        name_en: 'OpenAI API (GPT-4o-mini)',
        cost: 15,
        category_pt: 'Serviços IA (GPT, TTS)',
        category_en: 'AI Services (GPT, TTS)',
        description_pt: 'Créditos API para chatbot conversacional (estimativa 3 meses)',
        description_en: 'API credits for conversational chatbot (3-month estimate)',
        justification_pt: 'GPT-4o-mini custa €0.15/1M tokens input + €0.60/1M output. Estimativa: 100k perguntas em 3 meses = ~€15. Rate limiting: 3 perguntas/utilizador/visita.',
        justification_en: 'GPT-4o-mini costs €0.15/1M tokens input + €0.60/1M output. Estimate: 100k questions in 3 months = ~€15. Rate limiting: 3 questions/user/visit.',
        advantages_pt: [
            'Respostas contextuais inteligentes',
            'Conhecimento histórico profundo',
            'Adaptação a perfil utilizador',
            'Custo baixíssimo por consulta'
        ],
        advantages_en: [
            'Intelligent contextual answers',
            'Deep historical knowledge',
            'User profile adaptation',
            'Very low cost per query'
        ],
        alternatives_pt: 'FAQ estático (limitado, sem personalização)',
        alternatives_en: 'Static FAQ (limited, no personalization)',
    },

    'google-tts': {
        id: 'google-tts',
        name_pt: 'Google TTS premium',
        name_en: 'Google TTS premium',
        cost: 45,
        category_pt: 'Serviços IA (GPT, TTS)',
        category_en: 'AI Services (GPT, TTS)',
        description_pt: 'Text-to-Speech premium para respostas GPT (voz neural)',
        description_en: 'Premium Text-to-Speech for GPT answers (neural voice)',
        justification_pt: 'Google TTS Neural €16/1M caracteres. Estimativa 3 meses: 100k respostas × ~200 chars = 20M chars = €45. Vozes neurais > standard (naturalidade 2x).',
        justification_en: 'Google TTS Neural €16/1M characters. 3-month estimate: 100k answers × ~200 chars = 20M chars = €45. Neural voices > standard (2x naturalness).',
        advantages_pt: [
            'Vozes neurais naturais',
            'Suporte PT-PT nativo',
            'Baixa latência (<1s)',
            'Sem limitações de caracteres'
        ],
        advantages_en: [
            'Natural neural voices',
            'Native PT-PT support',
            'Low latency (<1s)',
            'No character limitations'
        ],
    },

    '360-assets': {
        id: '360-assets',
        name_pt: '360° assets/renders',
        name_en: '360° assets/renders',
        cost: 200,
        category_pt: 'Imagens & Conteúdo',
        category_en: 'Images & Content',
        description_pt: 'Imagens 360° ou renders para 5 edifícios (interior views)',
        description_en: '360° images or renders for 5 buildings (interior views)',
        justification_pt: 'Opção A: Fotografias 360° reais (€0, se permitido museu). Opção B: Renders Blender 360° (20h trabalho). Opção C: Assets stock €40 cada. Orçamento prevê Opção C.',
        justification_en: 'Option A: Real 360° photos (€0, if museum permits). Option B: Blender 360° renders (20h work). Option C: Stock assets €40 each. Budget assumes Option C.',
        advantages_pt: [
            'Qualidade profissional garantida',
            'Resolução 4K+ imersiva',
            'Editáveis se necessário',
            'Licença comercial'
        ],
        advantages_en: [
            'Guaranteed professional quality',
            'Immersive 4K+ resolution',
            'Editable if needed',
            'Commercial license'
        ],
    },

    'historical-content': {
        id: 'historical-content',
        name_pt: 'Imagens & Conteúdo histórico',
        name_en: 'Historical images & content',
        cost: 200,
        category_pt: 'Imagens & Conteúdo',
        category_en: 'Images & Content',
        description_pt: 'Expansão de 30→100 edifícios: 70 imagens adicionais + pesquisa histórica',
        description_en: 'Expansion 30→100 buildings: 70 additional images + historical research',
        justification_pt: '70 edifícios novos precisam de contexto visual. Licenças €3-5/imagem (menor qualidade que Fase 1). Inclui também acesso a bibliotecas digitais.',
        justification_en: '70 new buildings need visual context. Licenses €3-5/image (lower quality than Phase 1). Also includes digital library access.',
        advantages_pt: [
            'Cobertura completa 100 POIs',
            'Licenças legais comerciais',
            'Suporte à escrita da tese',
            'Curadoria histórica'
        ],
        advantages_en: [
            'Complete 100 POI coverage',
            'Legal commercial licenses',
            'Thesis writing support',
            'Historical curation'
        ],
    },

    // ========================================================================
    // PHASE 4 TECH STACK
    // ========================================================================
    'copilot-phase4': {
        id: 'copilot-phase4',
        name_pt: 'Copilot Pro+ (3 meses)',
        name_en: 'Copilot Pro+ (3 months)',
        cost: 117,
        category_pt: 'Copilot Pro+',
        category_en: 'Copilot Pro+',
        description_pt: 'Fase final: optimização, polish, documentação tese',
        description_en: 'Final phase: optimization, polish, thesis documentation',
        justification_pt: 'Últimos 3 meses focam em refactoring, optimização e documentação. Copilot gera documentação técnica automaticamente, acelerando escrita da tese.',
        justification_en: 'Last 3 months focus on refactoring, optimization and documentation. Copilot automatically generates technical documentation, accelerating thesis writing.',
        advantages_pt: [
            'Refactoring automático',
            'Geração de documentação',
            'Detecção de code smells',
            'Sugestões de otimização'
        ],
        advantages_en: [
            'Automatic refactoring',
            'Documentation generation',
            'Code smell detection',
            'Optimization suggestions'
        ],
    },

    'professional-translations': {
        id: 'professional-translations',
        name_pt: 'Traduções profissionais (EN/ES)',
        name_en: 'Professional translations (EN/ES)',
        cost: 200,
        category_pt: 'Traduções & Localization',
        category_en: 'Translations & Localization',
        description_pt: 'Tradução profissional de todo o conteúdo para inglês e espanhol',
        description_en: 'Professional translation of all content to English and Spanish',
        justification_pt: '~50,000 palavras de conteúdo (fichas edifícios, UI, áudio). Tradutores profissionais especializados em património cultural: €0.10/palavra PT→EN, €0.05/palavra PT→ES.',
        justification_en: '~50,000 words of content (building cards, UI, audio). Professional translators specialized in cultural heritage: €0.10/word PT→EN, €0.05/word PT→ES.',
        advantages_pt: [
            'Qualidade profissional (não Google Translate)',
            'Terminologia histórica correta',
            'Revisão por nativos',
            'Alcance internacional (museu)',
            'Turistas compreendem perfeitamente'
        ],
        advantages_en: [
            'Professional quality (not Google Translate)',
            'Correct historical terminology',
            'Native speaker review',
            'International reach (museum)',
            'Tourists understand perfectly'
        ],
        alternatives_pt: 'Google Translate grátis (erros históricos graves, prejudica credibilidade museu)',
        alternatives_en: 'Free Google Translate (serious historical errors, hurts museum credibility)',
    },

    'beta-tester-incentives': {
        id: 'beta-tester-incentives',
        name_pt: 'Incentivos beta testers',
        name_en: 'Beta tester incentives',
        cost: 100,
        category_pt: 'Testing & Video',
        category_en: 'Testing & Video',
        description_pt: 'Vouchers/agradecimentos para 50 beta testers',
        description_en: 'Vouchers/acknowledgments for 50 beta testers',
        justification_pt: 'Testes extensivos precisam de 50 utilizadores diversos. Incentivo €2/pessoa (café museu) motiva participação e feedback detalhado.',
        justification_en: 'Extensive testing needs 50 diverse users. €2/person incentive (museum cafe) motivates participation and detailed feedback.',
        advantages_pt: [
            'Feedback qualitativo profundo',
            'Diversidade de perfis (idade, tech-savvy)',
            'Testes no ambiente real (museu)',
            'Dados para tese (estatísticas)'
        ],
        advantages_en: [
            'Deep qualitative feedback',
            'Profile diversity (age, tech-savvy)',
            'Real environment testing (museum)',
            'Thesis data (statistics)'
        ],
    },

    'demo-video': {
        id: 'demo-video',
        name_pt: 'Video demo profissional',
        name_en: 'Professional demo video',
        cost: 50,
        category_pt: 'Testing & Video',
        category_en: 'Testing & Video',
        description_pt: 'Vídeo promocional 1-2min para App Store e apresentações',
        description_en: '1-2min promotional video for App Store and presentations',
        justification_pt: 'App Store exige video preview. Freelancer Fiverr: €50 para edição profissional 2min (screencaps fornecidas). Aumenta conversão downloads 2-3x.',
        justification_en: 'App Store requires video preview. Fiverr freelancer: €50 for 2min professional editing (screencaps provided). Increases download conversion 2-3x.',
        advantages_pt: [
            'Aumento conversão App Store (2-3x)',
            'Uso em apresentações tese',
            'Material para museu promover',
            'Portfolio profissional'
        ],
        advantages_en: [
            'App Store conversion increase (2-3x)',
            'Use in thesis presentations',
            'Material for museum promotion',
            'Professional portfolio'
        ],
    },

    'final-assets': {
        id: 'final-assets',
        name_pt: 'Assets finais (50 edifícios restantes)',
        name_en: 'Final assets (remaining 50 buildings)',
        cost: 300,
        category_pt: 'Imagens & Conteúdo',
        category_en: 'Images & Content',
        description_pt: 'Completar cobertura 150 edifícios (faltam 50 após Fase 3)',
        description_en: 'Complete 150 building coverage (50 missing after Phase 3)',
        justification_pt: 'Grande Panorama tem ~150 edifícios identificáveis. Cobertura total = valor científico máximo. €6/edifício (imagem + pesquisa básica).',
        justification_en: 'Grande Panorama has ~150 identifiable buildings. Total coverage = maximum scientific value. €6/building (image + basic research).',
        advantages_pt: [
            'Cobertura total 100% do painel',
            'Valor científico máximo (tese)',
            'Diferenciação vs apps existentes',
            'Catalogação completa património'
        ],
        advantages_en: [
            '100% panel coverage',
            'Maximum scientific value (thesis)',
            'Differentiation vs existing apps',
            'Complete heritage cataloging'
        ],
    },

    'contingency': {
        id: 'contingency',
        name_pt: 'Contingência/imprevistos',
        name_en: 'Contingency/unforeseen',
        cost: 200,
        category_pt: 'Contingência',
        category_en: 'Contingency',
        description_pt: 'Reserva para custos imprevistos (bugs, re-work, extras)',
        description_en: 'Reserve for unforeseen costs (bugs, re-work, extras)',
        justification_pt: 'Projetos software têm ~10-15% imprevistos (estudos IEEE). €200 = 6.4% do total, conservador. Cobre: re-submissions store, APIs extras, hardware teste.',
        justification_en: 'Software projects have ~10-15% unforeseen costs (IEEE studies). €200 = 6.4% of total, conservative. Covers: store re-submissions, extra APIs, test hardware.',
        advantages_pt: [
            'Proteção contra atrasos',
            'Flexibilidade para melhorias',
            'Sem stress financeiro',
            'Permite experimentação'
        ],
        advantages_en: [
            'Protection against delays',
            'Flexibility for improvements',
            'No financial stress',
            'Allows experimentation'
        ],
    },
};

/**
 * Get cost detail by ID
 */
export function getCostDetail(id: string): CostDetail | undefined {
    return costDetailsMap[id];
}

/**
 * Get cost details by category
 */
export function getCostDetailsByCategory(category: string): CostDetail[] {
    return Object.values(costDetailsMap).filter(
        detail => detail.category_en === category || detail.category_pt === category
    );
}

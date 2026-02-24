/**
 * RISK MITIGATION SECTION
 * ========================
 * 
 * 3-column grid with elevated cards, colored accents and confident summary banner.
 */

import { useInlineTranslation, useLanguage } from '@/utils/language';
import { getPhaseColor } from './colors';
import { projectMetadata } from './utils';

export function RiskMitigation() {
    const { language } = useLanguage();

    const title = useInlineTranslation('Mitigação de Riscos', 'Risk Mitigation');
    const subtitle = useInlineTranslation(
        'Estratégias proativas para garantir o sucesso do projeto',
        'Proactive strategies to ensure project success'
    );

    const technicalTitle = useInlineTranslation('Riscos Técnicos', 'Technical Risks');
    const marketTitle = useInlineTranslation('Riscos de Mercado', 'Market Risks');
    const financialTitle = useInlineTranslation('Riscos Financeiros', 'Financial Risks');

    const technicalRisks = [
        {
            risk_pt: 'Complexidade da simulação de terramotos',
            risk_en: 'Earthquake simulation complexity',
            mitigation_pt: 'Prototipagem antecipada, consulta com especialistas em física',
            mitigation_en: 'Early prototyping, consultation with physics experts',
        },
        {
            risk_pt: 'Integração de APIs externas (mapas, clima)',
            risk_en: 'External API integration (maps, weather)',
            mitigation_pt: 'Fallback para dados locais, caching robusto',
            mitigation_en: 'Fallback to local data, robust caching',
        },
        {
            risk_pt: 'Performance em dispositivos móveis',
            risk_en: 'Mobile device performance',
            mitigation_pt: 'Testes em múltiplos dispositivos, otimização progressiva',
            mitigation_en: 'Multi-device testing, progressive optimization',
        },
    ];

    const marketRisks = [
        {
            risk_pt: 'Baixa adoção inicial de usuários',
            risk_en: 'Low initial user adoption',
            mitigation_pt: 'MVP validado com usuários reais, marketing direcionado',
            mitigation_en: 'MVP validated with real users, targeted marketing',
        },
        {
            risk_pt: 'Competição de plataformas maiores',
            risk_en: 'Competition from larger platforms',
            mitigation_pt: 'Foco em nicho (azulejos portugueses), recursos únicos',
            mitigation_en: 'Focus on niche (Portuguese tiles), unique features',
        },
        {
            risk_pt: 'Mudanças em tendências culturais',
            risk_en: 'Shifts in cultural trends',
            mitigation_pt: 'Arquitetura modular, capacidade de pivotagem rápida',
            mitigation_en: 'Modular architecture, fast pivot capability',
        },
    ];

    const financialRisks = [
        {
            risk_pt: 'Ultrapassagem de orçamento',
            risk_en: 'Budget overrun',
            mitigation_pt: `Margem de segurança de €${projectMetadata.buffer}, desenvolvimento iterativo`,
            mitigation_en: `€${projectMetadata.buffer} safety buffer, iterative development`,
        },
        {
            risk_pt: 'Atrasos em marcos de entrega',
            risk_en: 'Milestone delivery delays',
            mitigation_pt: 'Cronograma com folga, revisões mensais de progresso',
            mitigation_en: 'Schedule with slack, monthly progress reviews',
        },
        {
            risk_pt: 'Custos imprevistos de infraestrutura',
            risk_en: 'Unexpected infrastructure costs',
            mitigation_pt: 'Fornecedores gratuitos (Vercel, Supabase tier grátis)',
            mitigation_en: 'Free-tier providers (Vercel, Supabase free tier)',
        },
    ];

    const RiskCard = ({
        title,
        icon,
        risks,
        accentColor,
    }: {
        title: string;
        icon: string;
        risks: Array<{
            risk_pt: string;
            risk_en: string;
            mitigation_pt: string;
            mitigation_en: string;
        }>;
        accentColor: string;
    }) => (
        <div
            className="group relative h-full rounded-2xl border border-gray-200/80 dark:border-gray-800/80 
                       bg-white/95 dark:bg-gray-950/95 shadow-lg 
                       hover:shadow-2xl hover:-translate-y-2 transition-all duration-500 overflow-hidden"
        >
            {/* Top gradient bar */}
            <div
                className="absolute top-0 left-0 right-0 h-1.5"
                style={{ background: `linear-gradient(90deg, ${accentColor}, ${accentColor}99)` }}
            />

            <div className="relative p-6 sm:p-7">
                <div className="flex items-center gap-3 mb-5">
                    <div
                        className="w-12 h-12 rounded-2xl flex items-center justify-center text-2xl 
                                   shadow-md transition-transform duration-300 group-hover:scale-110"
                        style={{
                            background: `radial-gradient(circle at 30% 30%, #ffffff, ${accentColor}33)`,
                        }}
                    >
                        <span aria-hidden="true">{icon}</span>
                    </div>
                    <h3 className="text-lg sm:text-xl font-semibold text-gray-900 dark:text-white">
                        {title}
                    </h3>
                </div>

                <div className="space-y-4">
                    {risks.map((item, index) => (
                        <div
                            key={index}
                            className="relative pl-4 border-l-2 transition-all duration-300 group-hover:pl-5"
                            style={{ borderColor: `${accentColor}80` }}
                        >
                            {/* Bullet dot */}
                            <span
                                className="absolute -left-1 top-2 w-2 h-2 rounded-full"
                                style={{ backgroundColor: accentColor }}
                                aria-hidden="true"
                            />
                            <p className="text-sm font-medium text-gray-900 dark:text-white mb-1">
                                {language === 'pt' ? item.risk_pt : item.risk_en}
                            </p>
                            <p className="text-xs sm:text-sm text-gray-600 dark:text-gray-400 leading-relaxed">
                                <span className="font-semibold">
                                    {language === 'pt' ? 'Mitigação:' : 'Mitigation:'}
                                </span>{' '}
                                {language === 'pt' ? item.mitigation_pt : item.mitigation_en}
                            </p>
                        </div>
                    ))}
                </div>
            </div>
        </div>
    );

    return (
        <section
            className="relative py-20 sm:py-24 
                       bg-gradient-to-b from-azulejo-ivory-100 via-white to-azulejo-ivory-100 
                       dark:from-gray-950 dark:via-gray-900 dark:to-gray-900 overflow-hidden"
            aria-labelledby="risk-heading"
        >
            {/* Background accents */}
            <div className="pointer-events-none absolute inset-0">
                <div className="absolute -top-24 -left-24 w-64 h-64 rounded-full bg-azulejo-blue-300/20 dark:bg-azulejo-blue-800/20 blur-3xl" />
                <div className="absolute -bottom-24 -right-24 w-64 h-64 rounded-full bg-azulejo-gold-300/24 dark:bg-azulejo-gold-800/24 blur-3xl" />
            </div>

            <div className="relative max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
                {/* Header */}
                <div className="text-center mb-10 sm:mb-12">
                    <h2
                        id="risk-heading"
                        className="text-3xl sm:text-4xl lg:text-5xl font-bold mb-3 
                                   text-gray-900 dark:text-white tracking-tight"
                    >
                        {title}
                    </h2>
                    <div className="flex items-center justify-center gap-3 mb-4">
                        <div className="h-px w-14 bg-gradient-to-r from-azulejo-blue-500 to-azulejo-blue-300" />
                        <div className="h-10 w-10 rounded-2xl bg-azulejo-blue-600 text-white flex items-center justify-center shadow-lg shadow-azulejo-blue-500/40">
                            🛡️
                        </div>
                        <div className="h-px w-14 bg-gradient-to-l from-azulejo-blue-500 to-azulejo-blue-300" />
                    </div>
                    <p className="text-sm sm:text-base text-gray-600 dark:text-gray-300 max-w-2xl mx-auto">
                        {subtitle}
                    </p>
                </div>

                {/* Card wrapper with blue accent bar on the left */}
                <div className="relative">
                    <div
                        className="absolute left-0 top-4 bottom-4 w-1.5 rounded-l-2xl"
                        style={{
                            background: 'linear-gradient(to bottom, #3C5E95, #5081B6)',
                        }}
                    />

                    <div className="ml-1.5 rounded-2xl border border-gray-200/80 dark:border-gray-800 
                                    bg-white/96 dark:bg-gray-950/96 shadow-2xl 
                                    p-6 sm:p-8 lg:p-10">
                        {/* 3-column grid */}
                        <div className="grid grid-cols-1 md:grid-cols-3 gap-6 lg:gap-8">
                            <RiskCard
                                title={technicalTitle}
                                icon="⚙️"
                                risks={technicalRisks}
                                accentColor={getPhaseColor(1)}
                            />
                            <RiskCard
                                title={marketTitle}
                                icon="📊"
                                risks={marketRisks}
                                accentColor={getPhaseColor(2)}
                            />
                            <RiskCard
                                title={financialTitle}
                                icon="💰"
                                risks={financialRisks}
                                accentColor={getPhaseColor(4)}
                            />
                        </div>

                        {/* Summary banner */}
                        <div className="mt-8 rounded-xl bg-gradient-to-r from-azulejo-blue-600 via-azulejo-blue-500 to-azulejo-gold-500 
                                        text-white px-6 sm:px-8 py-5 sm:py-6 flex flex-col sm:flex-row items-center justify-between gap-4 shadow-lg">
                            <div className="flex items-center gap-3">
                                <div className="flex h-10 w-10 items-center justify-center rounded-full bg-white/15">
                                    <svg
                                        className="h-6 w-6"
                                        viewBox="0 0 24 24"
                                        fill="none"
                                        xmlns="http://www.w3.org/2000/svg"
                                        aria-hidden="true"
                                    >
                                        <path
                                            d="M9 12.75L11.25 15L15 9.75"
                                            stroke="currentColor"
                                            strokeWidth="1.8"
                                            strokeLinecap="round"
                                            strokeLinejoin="round"
                                        />
                                        <path
                                            d="M21 12C21 16.9706 16.9706 21 12 21C7.02944 21 3 16.9706 3 12C3 7.02944 7.02944 3 12 3C16.9706 3 21 7.02944 21 12Z"
                                            stroke="currentColor"
                                            strokeWidth="1.8"
                                        />
                                    </svg>
                                </div>
                                <p className="text-sm sm:text-base font-semibold">
                                    {language === 'pt'
                                        ? 'Riscos mapeados com planos de mitigação concretos em todas as fases.'
                                        : 'Risks mapped with concrete mitigation plans across every phase.'}
                                </p>
                            </div>
                            <p className="text-xs sm:text-sm text-white/80">
                                {language === 'pt'
                                    ? 'Atualizado a cada revisão mensal do plano.'
                                    : 'Reviewed and updated at each monthly plan review.'}
                            </p>
                        </div>
                    </div>
                </div>
            </div>
        </section>
    );
}

export default RiskMitigation;

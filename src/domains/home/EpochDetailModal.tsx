/**
 * EPOCH DETAIL MODAL
 * ==================
 * Detailed historical information modal for each of the 4 time periods
 * 
 * Opens when user clicks on an epoch card
 * Displays: Timeline, key events, historical facts, images
 * 
 * Phase 3: Basic structure with placeholder content
 * Phase 4: Add real historical images and comprehensive content
 */

import { Modal } from '@/components/ui';
import { useLanguage } from '@/utils/language/hooks';
import type { Epoch } from './EpochsSection';

interface EpochDetailModalProps {
    isOpen: boolean;
    onClose: () => void;
    epoch: Epoch | null;
}

// Detailed content for each epoch
const epochDetails: Record<
    number,
    {
        keyEvents: { pt: string[]; en: string[] };
        facts: { pt: string[]; en: string[] };
        impact: { pt: string; en: string };
    }
> = {
    1: {
        // Pre-Earthquake Glory (~1700)
        keyEvents: {
            pt: [
                '1680-1700: Construção do Grande Panorama por Gabriel del Barco',
                '1698: Lisboa floresce como centro comercial do Atlântico',
                '1700: População atinge ~200.000 habitantes',
                'Ruas estreitas medievais dominam o centro histórico',
            ],
            en: [
                '1680-1700: Construction of the Grande Panorama by Gabriel del Barco',
                '1698: Lisbon flourishes as Atlantic trade center',
                '1700: Population reaches ~200,000 inhabitants',
                'Narrow medieval streets dominate the historic center',
            ],
        },
        facts: {
            pt: [
                '150+ edifícios identificados no Panorama',
                'Palácios ornamentados em estilo barroco',
                'Igrejas majestosas com torres góticas',
                '14km de costa do Rio Tejo representados',
            ],
            en: [
                '150+ buildings identified in the Panorama',
                'Ornate baroque-style palaces',
                'Majestic churches with gothic towers',
                '14km of Tagus River coastline represented',
            ],
        },
        impact: {
            pt: 'O Grande Panorama é o único registo visual completo de Lisboa antes do terramoto, tornando-se uma janela única para o passado glorioso da cidade.',
            en: 'The Grande Panorama is the only complete visual record of Lisbon before the earthquake, making it a unique window into the city\'s glorious past.',
        },
    },
    2: {
        // The Great Earthquake (1755)
        keyEvents: {
            pt: [
                '1 de Novembro de 1755, 9:40 AM: Terramoto magnitude ~9.0',
                '15 minutos depois: Tsunami de 6 metros atinge a costa',
                'Incêndios devastadores durante 5 dias',
                '85% dos edifícios destruídos',
                '~60.000 mortes (30% da população)',
            ],
            en: [
                'November 1, 1755, 9:40 AM: Earthquake magnitude ~9.0',
                '15 minutes later: 6-meter tsunami hits the coast',
                'Devastating fires for 5 days',
                '85% of buildings destroyed',
                '~60,000 deaths (30% of population)',
            ],
        },
        facts: {
            pt: [
                'Um dos terramotos mais mortíferos da história',
                'Ondas sísmicas sentidas em toda a Europa',
                'Inspirou estudos científicos modernos de sismologia',
                'Dia de Todos os Santos: igrejas lotadas',
            ],
            en: [
                'One of the deadliest earthquakes in history',
                'Seismic waves felt across Europe',
                'Inspired modern scientific studies of seismology',
                'All Saints Day: churches were full',
            ],
        },
        impact: {
            pt: 'O terramoto mudou Lisboa para sempre, marcando o fim da era medieval e o início de um planeamento urbano moderno na reconstrução.',
            en: 'The earthquake changed Lisbon forever, marking the end of the medieval era and the beginning of modern urban planning in reconstruction.',
        },
    },
    3: {
        // Pombaline Reconstruction (1760s-1800s)
        keyEvents: {
            pt: [
                '1756: Marquês de Pombal inicia reconstrução',
                '1758: Plano urbanístico revolucionário aprovado',
                'Ruas largas e ortogonais substituem becos medievais',
                'Edifícios resistentes a terramotos com "gaiola pombalina"',
                '1775: Baixa Pombalina praticamente concluída',
            ],
            en: [
                '1756: Marquis of Pombal begins reconstruction',
                '1758: Revolutionary urban plan approved',
                'Wide orthogonal streets replace medieval alleys',
                'Earthquake-resistant buildings with "Pombaline cage"',
                '1775: Baixa Pombalina largely completed',
            ],
        },
        facts: {
            pt: [
                'Primeiro exemplo de planeamento urbano antisísmico',
                'Ruas de 40 metros de largura (vs 5m anteriores)',
                'Grelha ortogonal: inovação para a época',
                'Arquitetura neoclássica simétrica',
            ],
            en: [
                'First example of anti-seismic urban planning',
                '40-meter wide streets (vs 5m previously)',
                'Orthogonal grid: innovation for the time',
                'Symmetrical neoclassical architecture',
            ],
        },
        impact: {
            pt: 'A reconstrução pombalina transformou Lisboa numa cidade moderna, servindo de modelo para planeamento urbano em todo o mundo.',
            en: 'Pombaline reconstruction transformed Lisbon into a modern city, serving as a model for urban planning worldwide.',
        },
    },
    4: {
        // Modern Day (Present)
        keyEvents: {
            pt: [
                '1974: Revolução dos Cravos e democracia',
                '1986: Portugal entra na União Europeia',
                '1998: Expo 98 e modernização do Parque das Nações',
                '2010s: Boom turístico e gentrificação',
                '2024: Lisboa como capital tecnológica europeia',
            ],
            en: [
                '1974: Carnation Revolution and democracy',
                '1986: Portugal joins the European Union',
                '1998: Expo 98 and modernization of Parque das Nações',
                '2010s: Tourism boom and gentrification',
                '2024: Lisbon as European tech capital',
            ],
        },
        facts: {
            pt: [
                'População: ~500.000 (área metropolitana: 2.8M)',
                'Mistura de arquitetura histórica e moderna',
                'Preservação da Baixa Pombalina como Património UNESCO',
                'Expansão para Parque das Nações e novos bairros',
            ],
            en: [
                'Population: ~500,000 (metro area: 2.8M)',
                'Mix of historical and modern architecture',
                'Preservation of Baixa Pombalina as UNESCO Heritage',
                'Expansion to Parque das Nações and new districts',
            ],
        },
        impact: {
            pt: 'Lisboa moderna mantém o legado pombalino enquanto abraça inovação, equilibrando tradição e progresso numa cidade vibrante.',
            en: 'Modern Lisbon maintains the Pombaline legacy while embracing innovation, balancing tradition and progress in a vibrant city.',
        },
    },
};

export function EpochDetailModal({ isOpen, onClose, epoch }: EpochDetailModalProps) {
    const { language } = useLanguage();

    // Translation helper function
    const t = (text: { pt: string; en: string } | string) => {
        if (typeof text === 'string') return text;
        return language === 'pt' ? text.pt : text.en;
    };

    if (!epoch) return null;

    const details = epochDetails[epoch.id];
    if (!details) return null;

    // Get the current language to access array content
    const keyEvents = language === 'pt' ? details.keyEvents.pt : details.keyEvents.en;
    const facts = language === 'pt' ? details.facts.pt : details.facts.en;

    return (
        <Modal isOpen={isOpen} onClose={onClose} size="lg">
            <Modal.Content>
                {/* Header with icon and title */}
                <div className={`flex items-center gap-4 mb-6 rounded-lg ${epoch.bgColorClass} dark:bg-gray-800 border-l-4 ${epoch.borderColorClass}`}>
                    <span className="text-5xl" aria-hidden="true">
                        {epoch.icon}
                    </span>
                    <div>
                        <div className="text-sm font-semibold text-azulejo-blue-600 dark:text-azulejo-blue-400 uppercase tracking-wide">
                            {epoch.period}
                        </div>
                        <h3 className="text-3xl font-bold text-azulejo-blue-900 dark:text-white">
                            {t(epoch.title)}
                        </h3>
                    </div>
                </div>

                {/* Description */}
                <p className="text-lg text-gray-700 dark:text-gray-300 leading-relaxed mb-8">
                    {t(epoch.description)}
                </p>

                {/* Key Events Timeline */}
                <div className="mb-8">
                    <h4 className="text-xl font-bold text-azulejo-blue-900 dark:text-white mb-4 flex items-center gap-2">
                        <span className="text-2xl">📅</span>
                        {t({ pt: 'Eventos-Chave', en: 'Key Events' })}
                    </h4>
                    <ul className="space-y-3">
                        {keyEvents.map((event: string, index: number) => (
                            <li key={index} className="flex items-start gap-3">
                                <span className="flex-shrink-0 w-2 h-2 rounded-full bg-azulejo-blue-500 dark:bg-azulejo-blue-400 mt-2" />
                                <span className="text-gray-700 dark:text-gray-300">{event}</span>
                            </li>
                        ))}
                    </ul>
                </div>

                {/* Historical Facts */}
                <div className="mb-8">
                    <h4 className="text-xl font-bold text-azulejo-blue-900 dark:text-white mb-4 flex items-center gap-2">
                        <span className="text-2xl">💡</span>
                        {t({ pt: 'Factos Históricos', en: 'Historical Facts' })}
                    </h4>
                    <ul className="grid grid-cols-1 md:grid-cols-2 gap-3">
                        {facts.map((fact: string, index: number) => (
                            <li key={index} className="flex items-start gap-2 p-3 bg-gray-50 dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700">
                                <span className="text-azulejo-blue-500 dark:text-azulejo-blue-400 font-bold">✓</span>
                                <span className="text-gray-700 dark:text-gray-300 text-sm">{fact}</span>
                            </li>
                        ))}
                    </ul>
                </div>

                {/* Impact */}
                <div className="p-6 bg-azulejo-blue-50 dark:bg-gray-800 rounded-lg border-l-4 border-azulejo-blue-500 dark:border-azulejo-blue-400">
                    <h4 className="text-lg font-bold text-azulejo-blue-900 dark:text-white mb-3 flex items-center gap-2">
                        <span className="text-2xl">🎯</span>
                        {t({ pt: 'Impacto Histórico', en: 'Historical Impact' })}
                    </h4>
                    <p className="text-gray-700 dark:text-gray-300 leading-relaxed italic">
                        {t(details.impact)}
                    </p>
                </div>

                {/* Phase 4 TODO: Add historical images carousel */}
            </Modal.Content>
        </Modal>
    );
}

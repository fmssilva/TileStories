/**
 * COST DETAIL MODAL
 * ==================
 * 
 * Modal component that displays detailed information about a specific cost item.
 * Shows justification, advantages, alternatives, and visual elements.
 * 
 * Usage:
 * <CostDetailModal
 *   isOpen={isOpen}
 *   onClose={() => setIsOpen(false)}
 *   costId="apple-developer"
 * />
 */

import { Modal } from '@/components/ui';
import { useLanguage } from '@/utils/language';
import { getCostDetail } from './costDetails';

interface CostDetailModalProps {
    isOpen: boolean;
    onClose: () => void;
    costId: string;
}

export function CostDetailModal({ isOpen, onClose, costId }: CostDetailModalProps) {
    const { language } = useLanguage();
    const costDetail = getCostDetail(costId);

    if (!costDetail) {
        return null;
    }

    const name = language === 'pt' ? costDetail.name_pt : costDetail.name_en;
    const category = language === 'pt' ? costDetail.category_pt : costDetail.category_en;
    const description = language === 'pt' ? costDetail.description_pt : costDetail.description_en;
    const justification = language === 'pt' ? costDetail.justification_pt : costDetail.justification_en;
    const advantages = language === 'pt' ? costDetail.advantages_pt : costDetail.advantages_en;
    const alternatives = language === 'pt' ? costDetail.alternatives_pt : costDetail.alternatives_en;

    return (
        <Modal isOpen={isOpen} onClose={onClose} size="lg">
            <Modal.Header sticky>
                <h2 className="text-2xl font-bold text-azulejo-blue-900 dark:text-white">
                    {name}
                </h2>
            </Modal.Header>

            <Modal.Content>
                <div className="space-y-6">
                    {/* Cost Header */}
                    <div className="flex items-center justify-between p-4 bg-gradient-to-r from-azulejo-gold-100 to-azulejo-gold-50 dark:from-azulejo-gold-900/30 dark:to-azulejo-gold-800/20 rounded-lg border-l-4 border-azulejo-gold-500">
                        <div>
                            <p className="text-sm text-gray-600 dark:text-gray-400 mb-1">
                                {language === 'pt' ? 'Investimento' : 'Investment'}
                            </p>
                            <p className="text-3xl font-bold text-azulejo-gold-700 dark:text-azulejo-gold-400">
                                €{costDetail.cost}
                            </p>
                        </div>
                        <div className="text-right">
                            <p className="text-sm text-gray-600 dark:text-gray-400 mb-1">
                                {language === 'pt' ? 'Categoria' : 'Category'}
                            </p>
                            <span className="inline-block px-3 py-1 bg-azulejo-blue-100 dark:bg-azulejo-blue-900/30 text-azulejo-blue-800 dark:text-azulejo-blue-300 text-sm font-medium rounded-full">
                                {category}
                            </span>
                        </div>
                    </div>

                    {/* Description */}
                    <div>
                        <h3 className="text-lg font-semibold mb-2 text-gray-900 dark:text-white flex items-center gap-2">
                            <span className="text-xl">📋</span>
                            {language === 'pt' ? 'O que é?' : 'What is it?'}
                        </h3>
                        <p className="text-gray-700 dark:text-gray-300 leading-relaxed">
                            {description}
                        </p>
                    </div>

                    {/* Justification */}
                    <div className="p-4 bg-azulejo-ivory-50 dark:bg-gray-800/50 rounded-lg border border-gray-200 dark:border-gray-700">
                        <h3 className="text-lg font-semibold mb-2 text-gray-900 dark:text-white flex items-center gap-2">
                            <span className="text-xl">💡</span>
                            {language === 'pt' ? 'Porquê este investimento?' : 'Why this investment?'}
                        </h3>
                        <p className="text-gray-700 dark:text-gray-300 leading-relaxed">
                            {justification}
                        </p>
                    </div>

                    {/* Advantages */}
                    <div>
                        <h3 className="text-lg font-semibold mb-3 text-gray-900 dark:text-white flex items-center gap-2">
                            <span className="text-xl">✨</span>
                            {language === 'pt' ? 'Vantagens' : 'Advantages'}
                        </h3>
                        <ul className="space-y-2">
                            {advantages.map((advantage, index) => (
                                <li
                                    key={index}
                                    className="flex items-start gap-3 text-gray-700 dark:text-gray-300"
                                >
                                    <span className="flex-shrink-0 w-5 h-5 rounded-full bg-green-100 dark:bg-green-900/30 text-green-600 dark:text-green-400 flex items-center justify-center text-xs font-bold mt-0.5">
                                        ✓
                                    </span>
                                    <span className="leading-relaxed">{advantage}</span>
                                </li>
                            ))}
                        </ul>
                    </div>

                    {/* Alternatives (if provided) */}
                    {alternatives && (
                        <div className="p-4 bg-red-50 dark:bg-red-900/10 rounded-lg border border-red-200 dark:border-red-800/30">
                            <h3 className="text-lg font-semibold mb-2 text-gray-900 dark:text-white flex items-center gap-2">
                                <span className="text-xl">⚠️</span>
                                {language === 'pt' ? 'Alternativas (não recomendadas)' : 'Alternatives (not recommended)'}
                            </h3>
                            <p className="text-gray-700 dark:text-gray-300 leading-relaxed">
                                {alternatives}
                            </p>
                        </div>
                    )}

                    {/* Footer */}
                    <div className="pt-4 border-t border-gray-200 dark:border-gray-700">
                        <p className="text-sm text-gray-500 dark:text-gray-400 text-center">
                            {language === 'pt'
                                ? 'Investimento justificado e transparente para o sucesso do projeto'
                                : 'Justified and transparent investment for project success'}
                        </p>
                    </div>
                </div>
            </Modal.Content>
        </Modal>
    );
}

export default CostDetailModal;

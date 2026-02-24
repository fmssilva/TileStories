/**
 * FINANCIAL BREAKDOWN SECTION
 * ============================
 * 
 * Detailed cost breakdown by category across all 4 phases
 * Responsive table showing infrastructure, development, design, testing costs
 * 
 * Layout: Responsive table (horizontal scroll on mobile)
 * Design: Modern card with glassmorphism, sticky header and summary highlights
 */

import { useState } from 'react';
import { useInlineTranslation, useLanguage } from '@/utils/language';
import { costBreakdown, getTotalCost, projectMetadata } from './utils';
import { getPhaseColor } from './colors';
import { CostDetailModal } from './CostDetailModal';
import { getCostIdForCategory } from './techStackMapping';

export function FinancialBreakdown() {
    const { language } = useLanguage();
    const [selectedCostId, setSelectedCostId] = useState<string | null>(null);

    const title = useInlineTranslation('Análise Financeira', 'Financial Analysis');
    const subtitle = useInlineTranslation(
        'Investimento transparente, fase a fase e por categoria.',
        'Transparent investment, by phase and category.'
    );
    const categoryLabel = useInlineTranslation('Categoria', 'Category');
    const totalLabel = useInlineTranslation('Total', 'Total');
    const bufferLabel = useInlineTranslation('Margem de Segurança', 'Safety Buffer');
    const investmentLabel = useInlineTranslation('Investimento Total', 'Total Investment');

    const handleRowClick = (costId: string | null) => {
        if (!costId) return;
        setSelectedCostId(costId);
    };

    return (
        <section
            className="relative py-20 sm:py-24 
                       bg-gradient-to-b from-gray-50 via-white to-azulejo-ivory-100 
                       dark:from-gray-950 dark:via-gray-900 dark:to-gray-850"
            aria-labelledby="financial-heading"
        >
            {/* Background accents */}
            <div className="pointer-events-none absolute inset-0">
                <div className="absolute -top-32 left-1/2 -translate-x-1/2 w-72 h-72 rounded-full 
                                bg-azulejo-blue-200/25 dark:bg-azulejo-blue-800/20 blur-3xl" />
                <div className="absolute -bottom-40 -right-32 w-64 h-64 rounded-full 
                                bg-azulejo-gold-200/25 dark:bg-azulejo-gold-800/20 blur-3xl" />
            </div>

            <div className="relative max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
                {/* Header */}
                <div className="text-center mb-10 sm:mb-12">
                    <h2
                        id="financial-heading"
                        className="text-3xl sm:text-4xl lg:text-5xl font-bold 
                                   text-gray-900 dark:text-white tracking-tight mb-3"
                    >
                        {title}
                    </h2>
                    <div className="flex items-center justify-center gap-3 mb-4">
                        <div className="h-px w-12 bg-gradient-to-r from-azulejo-gold to-azulejo-gold/40" />
                        <div className="h-10 w-10 rounded-2xl bg-azulejo-gold text-white flex items-center justify-center shadow-lg">
                            💰
                        </div>
                        <div className="h-px w-12 bg-gradient-to-l from-azulejo-gold to-azulejo-gold/40" />
                    </div>
                    <p className="text-sm sm:text-base text-gray-600 dark:text-gray-300 max-w-2xl mx-auto">
                        {subtitle}
                    </p>
                </div>

                {/* Card container */}
                <div className="relative">
                    {/* Gold accent bar */}
                    <div
                        className="absolute left-0 top-0 bottom-0 w-1.5 rounded-l-2xl"
                        style={{ background: 'linear-gradient(to bottom, #D4AF37, #F2D27A)' }}
                    />

                    <div className="ml-1.5 rounded-2xl border border-gray-200/80 dark:border-gray-800 
                                    bg-white/95 dark:bg-gray-950/95 shadow-2xl 
                                    p-6 sm:p-8 lg:p-10">
                        {/* Table wrapper */}
                        <div className="rounded-2xl border border-gray-200 dark:border-gray-800 overflow-hidden">
                            <div className="overflow-x-auto">
                                <table className="w-full text-sm">
                                    <thead className="bg-gradient-to-r from-azulejo-ivory-100 via-white to-azulejo-ivory-100 
                                                     dark:from-gray-900 dark:via-gray-850 dark:to-gray-900">
                                        <tr className="border-b border-gray-200 dark:border-gray-800">
                                            <th className="px-4 sm:px-5 py-4 text-left font-semibold text-xs sm:text-sm uppercase tracking-wide text-gray-500 dark:text-gray-400">
                                                {categoryLabel}
                                            </th>
                                            {[1, 2, 3, 4].map((phase) => (
                                                <th
                                                    key={phase}
                                                    className="px-4 sm:px-5 py-4 text-right font-semibold text-xs sm:text-sm uppercase tracking-wide"
                                                    style={{ color: getPhaseColor(phase as 1 | 2 | 3 | 4) }}
                                                >
                                                    {language === 'pt'
                                                        ? `Fase ${phase}`
                                                        : `Phase ${phase}`}
                                                </th>
                                            ))}
                                            <th className="px-4 sm:px-5 py-4 text-right font-semibold text-xs sm:text-sm uppercase tracking-wide text-gray-700 dark:text-gray-200">
                                                {totalLabel}
                                            </th>
                                        </tr>
                                    </thead>

                                    <tbody className="bg-white dark:bg-gray-950 divide-y divide-gray-100 dark:divide-gray-850">
                                        {costBreakdown.map((category, index) => {
                                            const categoryName =
                                                language === 'pt'
                                                    ? category.category_pt
                                                    : category.category_en;
                                            const costId = getCostIdForCategory(categoryName);
                                            const clickable = Boolean(costId);

                                            return (
                                                <tr
                                                    key={index}
                                                    onClick={() => handleRowClick(costId)}
                                                    role={clickable ? 'button' : undefined}
                                                    tabIndex={clickable ? 0 : undefined}
                                                    onKeyDown={(e) => {
                                                        if (
                                                            clickable &&
                                                            (e.key === 'Enter' || e.key === ' ')
                                                        ) {
                                                            e.preventDefault();
                                                            handleRowClick(costId);
                                                        }
                                                    }}
                                                    className={`
                                                        transition-all duration-250
                                                        ${
                                                            clickable
                                                                ? 'cursor-pointer hover:bg-azulejo-gold-50/60 dark:hover:bg-gray-900'
                                                                : 'hover:bg-azulejo-ivory-50/70 dark:hover:bg-gray-900'
                                                        }
                                                    `}
                                                    aria-label={
                                                        clickable
                                                            ? `${categoryName}. ${
                                                                  language === 'pt'
                                                                      ? 'Clique para ver detalhes'
                                                                      : 'Click for details'
                                                              }`
                                                            : categoryName
                                                    }
                                                >
                                                    <td className="px-4 sm:px-5 py-3.5 text-gray-900 dark:text-white font-medium whitespace-nowrap">
                                                        <div className="flex items-center gap-2">
                                                            <span>{categoryName}</span>
                                                            {clickable && (
                                                                <span className="inline-flex items-center rounded-full px-2 py-0.5 text-[10px] font-semibold bg-azulejo-gold-100 text-azulejo-gold-800 dark:bg-azulejo-gold-900/60 dark:text-azulejo-gold-200">
                                                                    {language === 'pt'
                                                                        ? 'Ver detalhes'
                                                                        : 'View details'}
                                                                </span>
                                                            )}
                                                        </div>
                                                    </td>
                                                    <td className="px-4 sm:px-5 py-3.5 text-right text-gray-700 dark:text-gray-300 whitespace-nowrap">
                                                        €{category.phase1}
                                                    </td>
                                                    <td className="px-4 sm:px-5 py-3.5 text-right text-gray-700 dark:text-gray-300 whitespace-nowrap">
                                                        €{category.phase2}
                                                    </td>
                                                    <td className="px-4 sm:px-5 py-3.5 text-right text-gray-700 dark:text-gray-300 whitespace-nowrap">
                                                        €{category.phase3}
                                                    </td>
                                                    <td className="px-4 sm:px-5 py-3.5 text-right text-gray-700 dark:text-gray-300 whitespace-nowrap">
                                                        €{category.phase4}
                                                    </td>
                                                    <td className="px-4 sm:px-5 py-3.5 text-right font-semibold text-gray-900 dark:text-white whitespace-nowrap">
                                                        €{category.total}
                                                    </td>
                                                </tr>
                                            );
                                        })}
                                    </tbody>

                                    {/* Totais por fase */}
                                    <tfoot className="bg-azulejo-ivory-100/80 dark:bg-gray-900/80">
                                        <tr className="border-t border-gray-200 dark:border-gray-800">
                                            <td className="px-4 sm:px-5 py-4 text-xs sm:text-sm font-semibold text-gray-800 dark:text-gray-100 uppercase tracking-wide">
                                                {totalLabel}
                                            </td>
                                            <td className="px-4 sm:px-5 py-4 text-right text-sm font-semibold text-gray-900 dark:text-white">
                                                €
                                                {costBreakdown.reduce(
                                                    (sum, cat) => sum + cat.phase1,
                                                    0
                                                )}
                                            </td>
                                            <td className="px-4 sm:px-5 py-4 text-right text-sm font-semibold text-gray-900 dark:text-white">
                                                €
                                                {costBreakdown.reduce(
                                                    (sum, cat) => sum + cat.phase2,
                                                    0
                                                )}
                                            </td>
                                            <td className="px-4 sm:px-5 py-4 text-right text-sm font-semibold text-gray-900 dark:text-white">
                                                €
                                                {costBreakdown.reduce(
                                                    (sum, cat) => sum + cat.phase3,
                                                    0
                                                )}
                                            </td>
                                            <td className="px-4 sm:px-5 py-4 text-right text-sm font-semibold text-gray-900 dark:text-white">
                                                €
                                                {costBreakdown.reduce(
                                                    (sum, cat) => sum + cat.phase4,
                                                    0
                                                )}
                                            </td>
                                            <td className="px-4 sm:px-5 py-4 text-right text-lg sm:text-xl font-bold text-azulejo-blue-800 dark:text-azulejo-blue-200">
                                                €{getTotalCost()}
                                            </td>
                                        </tr>
                                    </tfoot>
                                </table>
                            </div>
                        </div>

                        {/* Summary tiles */}
                        <div className="grid grid-cols-1 sm:grid-cols-3 gap-4 mt-8">
                            <div className="p-5 sm:p-6 rounded-xl bg-azulejo-ivory-50 dark:bg-gray-900/80 border border-gray-200/80 dark:border-gray-800/80 text-center shadow-sm">
                                <p className="text-xs sm:text-sm text-gray-600 dark:text-gray-400 mb-1">
                                    {language === 'pt' ? 'Desenvolvimento' : 'Development'}
                                </p>
                                <p className="text-2xl font-bold text-gray-900 dark:text-white">
                                    €{getTotalCost()}
                                </p>
                            </div>
                            <div className="p-5 sm:p-6 rounded-xl bg-azulejo-ivory-50 dark:bg-gray-900/80 border border-gray-200/80 dark:border-gray-800/80 text-center shadow-sm">
                                <p className="text-xs sm:text-sm text-gray-600 dark:text-gray-400 mb-1">
                                    {bufferLabel}
                                </p>
                                <p className="text-2xl font-bold text-gray-900 dark:text-white">
                                    €{projectMetadata.buffer}
                                </p>
                            </div>
                            <div className="p-5 sm:p-6 rounded-xl bg-gradient-to-br from-azulejo-gold-500 to-azulejo-gold-600 text-center shadow-lg border border-azulejo-gold-400">
                                <p className="text-xs sm:text-sm text-white/85 mb-1">
                                    {investmentLabel}
                                </p>
                                <p className="text-2xl font-bold text-white">
                                    €{projectMetadata.totalCostWithBuffer}
                                </p>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            {/* Modal Detalhe */}
            <CostDetailModal
                isOpen={selectedCostId !== null}
                onClose={() => setSelectedCostId(null)}
                costId={selectedCostId || ''}
            />
        </section>
    );
}

export default FinancialBreakdown;

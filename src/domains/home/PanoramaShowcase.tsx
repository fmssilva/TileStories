/**
 * PanoramaShowcase Component - The Grande Panorama de Lisboa
 * 
 * Purpose: Introduce the specific artifact and its historical significance
 * Content Source: Google Arts & Culture article + site_notes.md
 * 
 * Design: 60/40 split layout (image left, details right)
 * SEO: H2 "The Grande Panorama de Lisboa" with rich historical keywords
 */

import { useInlineTranslation } from '@/utils/language';
import { Button } from '@/components/ui';
import heroImage from './hero_img.png'; // Placeholder - replace with actual panorama image in Phase 4

export function PanoramaShowcase() {
    const t = useInlineTranslation;

    return (
        <section
            id="panorama-showcase"
            className="py-16 sm:py-20 lg:py-24 bg-white dark:bg-gray-800"
        >
            <div className="container mx-auto px-4 sm:px-6 lg:px-8">
                {/* Section Heading - H2 for SEO */}
                <h2 className="text-3xl sm:text-4xl lg:text-5xl font-bold text-center mb-16 text-gray-900 dark:text-white">
                    {t({ pt: 'O Grande Panorama de Lisboa', en: 'The Grande Panorama de Lisboa' })}
                </h2>

                {/* 60/40 Split Layout */}
                <div className="grid grid-cols-1 lg:grid-cols-5 gap-8 lg:gap-12 items-center">

                    {/* Left Column - Image (60%) */}
                    <div className="lg:col-span-3 relative group">
                        <div className="relative overflow-hidden rounded-2xl shadow-2xl">
                            <img
                                src={heroImage}
                                alt={t({
                                    pt: 'Grande Panorama de Lisboa - Painel de azulejo de 23 metros por Gabriel del Barco, mostrando Lisboa pré-terramoto',
                                    en: 'Grande Panorama de Lisboa - 23-meter azulejo panel by Gabriel del Barco, showing pre-earthquake Lisbon'
                                })}
                                className="w-full h-auto object-cover"
                                loading="lazy"
                            />

                            {/* Hover Overlay */}
                            <div className="absolute inset-0 bg-azulejo-blue-900/0 group-hover:bg-azulejo-blue-900/70 
                                transition-all duration-300 flex items-center justify-center">
                                <div className="opacity-0 group-hover:opacity-100 transition-all duration-300 transform 
                                    translate-y-4 group-hover:translate-y-0">
                                    <span className="text-white text-2xl font-bold flex items-center gap-2">
                                        {t({ pt: 'Explorar em AR', en: 'Explore in AR' })}
                                        <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
                                                d="M13 7l5 5m0 0l-5 5m5-5H6" />
                                        </svg>
                                    </span>
                                </div>
                            </div>
                        </div>

                        {/* Caption */}
                        <p className="text-sm text-gray-600 dark:text-gray-400 mt-4 italic text-center">
                            {t({
                                pt: 'Painel de azulejo de 23 metros por Gabriel del Barco (~1700), mostrando Lisboa pré-terramoto',
                                en: '23-meter azulejo panel by Gabriel del Barco (~1700), showing pre-earthquake Lisbon'
                            })}
                        </p>
                    </div>

                    {/* Right Column - Details (40%) */}
                    <div className="lg:col-span-2 space-y-6">

                        {/* Historical Context */}
                        <div>
                            <h3 className="text-2xl font-bold text-azulejo-blue-600 dark:text-azulejo-blue-400 mb-4">
                                {t({ pt: 'Contexto Histórico', en: 'Historical Context' })}
                            </h3>
                            <ul className="space-y-3 text-gray-700 dark:text-gray-300">
                                <li className="flex items-start gap-3">
                                    <span className="text-azulejo-gold-500 text-xl flex-shrink-0">•</span>
                                    <span>
                                        {t({
                                            pt: 'Criado ~1700, mostrando Lisboa pré-terramoto (1755)',
                                            en: 'Created ~1700, pre-earthquake Lisbon (1755)'
                                        })}
                                    </span>
                                </li>
                                <li className="flex items-start gap-3">
                                    <span className="text-azulejo-gold-500 text-xl flex-shrink-0">•</span>
                                    <span>
                                        {t({
                                            pt: '23 metros de comprimento, mostrando o rio Tejo e mais de 150 edifícios',
                                            en: '23 meters long, showing the Tagus River and 150+ buildings'
                                        })}
                                    </span>
                                </li>
                                <li className="flex items-start gap-3">
                                    <span className="text-azulejo-gold-500 text-xl flex-shrink-0">•</span>
                                    <span>
                                        {t({
                                            pt: 'Localizado no Museu Nacional do Azulejo',
                                            en: 'Located at Museu Nacional do Azulejo'
                                        })}
                                    </span>
                                </li>
                            </ul>
                        </div>

                        {/* What Makes It Special */}
                        <div>
                            <h3 className="text-2xl font-bold text-azulejo-blue-600 dark:text-azulejo-blue-400 mb-4">
                                {t({ pt: 'O Que o Torna Especial', en: 'What Makes It Special' })}
                            </h3>
                            <ul className="space-y-3 text-gray-700 dark:text-gray-300">
                                <li className="flex items-start gap-3">
                                    <span className="text-azulejo-terracotta-500 text-xl flex-shrink-0">✓</span>
                                    <span>
                                        {t({
                                            pt: 'Única vista panorâmica de Lisboa antes do terramoto de 1755',
                                            en: 'Only panoramic view of Lisbon before the 1755 earthquake'
                                        })}
                                    </span>
                                </li>
                                <li className="flex items-start gap-3">
                                    <span className="text-azulejo-terracotta-500 text-xl flex-shrink-0">✓</span>
                                    <span>
                                        {t({
                                            pt: 'Mostra edifícios que já não existem',
                                            en: 'Shows buildings that no longer exist'
                                        })}
                                    </span>
                                </li>
                                <li className="flex items-start gap-3">
                                    <span className="text-azulejo-terracotta-500 text-xl flex-shrink-0">✓</span>
                                    <span>
                                        {t({
                                            pt: 'Obra-prima da arte de azulejo portuguesa',
                                            en: 'Masterpiece of Portuguese azulejo art'
                                        })}
                                    </span>
                                </li>
                            </ul>
                        </div>

                        {/* AR Enhancement */}
                        <div className="bg-azulejo-blue-50 dark:bg-azulejo-blue-900/20 rounded-xl p-6">
                            <h3 className="text-xl font-bold text-azulejo-blue-700 dark:text-azulejo-blue-300 mb-3">
                                {t({ pt: 'Melhoria com AR', en: 'AR Enhancement' })}
                            </h3>
                            <ul className="space-y-2 text-gray-700 dark:text-gray-300 text-sm">
                                <li className="flex items-start gap-2">
                                    <span className="text-azulejo-blue-500">→</span>
                                    <span>
                                        {t({
                                            pt: 'Aponte o seu dispositivo para o panorama',
                                            en: 'Point your device at the panorama'
                                        })}
                                    </span>
                                </li>
                                <li className="flex items-start gap-2">
                                    <span className="text-azulejo-blue-500">→</span>
                                    <span>
                                        {t({
                                            pt: 'Toque nos edifícios para revelar as suas histórias',
                                            en: 'Tap buildings to reveal their stories'
                                        })}
                                    </span>
                                </li>
                                <li className="flex items-start gap-2">
                                    <span className="text-azulejo-blue-500">→</span>
                                    <span>
                                        {t({
                                            pt: 'Compare 4 épocas históricas',
                                            en: 'Compare 4 historical epochs'
                                        })}
                                    </span>
                                </li>
                                <li className="flex items-start gap-2">
                                    <span className="text-azulejo-blue-500">→</span>
                                    <span>
                                        {t({
                                            pt: 'Experimente a simulação do terramoto de 1755',
                                            en: 'Experience the 1755 earthquake simulation'
                                        })}
                                    </span>
                                </li>
                            </ul>
                        </div>

                        {/* CTA Button */}
                        <div className="pt-4">
                            <Button
                                variant="secondary"
                                size="lg"
                                className="w-full sm:w-auto bg-azulejo-cobalt-500 hover:bg-azulejo-cobalt-600 
                                    text-white px-8 py-3 font-semibold rounded-lg shadow-md 
                                    hover:shadow-lg transition-all duration-300"
                            >
                                {t({ pt: 'Explorar o Guia Interativo', en: 'Explore the Interactive Guide' })}
                            </Button>
                        </div>
                    </div>
                </div>
            </div>
        </section>
    );
}

export default PanoramaShowcase;

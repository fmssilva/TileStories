/**
 * ProblemStatement Component - Why AR for Museums?
 * 
 * Purpose: Establish the value proposition of AR technology for museum experiences
 * Target: Tourists, educators, museum professionals
 * 
 * Design: Three-column card layout comparing Traditional → AR Solution → Impact
 * Colors: Gray (traditional), Azulejo Blue (AR), Azulejo Gold (impact)
 */

import { useInlineTranslation } from '@/utils/language';

export function ProblemStatement() {
    const t = useInlineTranslation;

    const cards = [
        {
            icon: '🖼️',
            title: t({ pt: 'Experiência Tradicional de Museu', en: 'Traditional Museum Experience' }),
            description: t({
                pt: 'Exposições estáticas limitam o envolvimento. Os visitantes têm dificuldade em imaginar o contexto histórico.',
                en: 'Static displays limit engagement. Visitors struggle to imagine historical context.'
            }),
            gradient: 'from-gray-100 to-gray-200',
            iconBg: 'bg-gray-200',
            textColor: 'text-gray-900'
        },
        {
            icon: '✨',
            title: t({ pt: 'A Solução AR', en: 'The AR Solution' }),
            description: t({
                pt: 'Camadas interativas de AR dão vida aos artefactos. Veja os edifícios como eram, compare épocas, testemunhe eventos históricos.',
                en: 'Interactive AR layers bring artifacts to life. See buildings as they were, compare epochs, witness historical events.'
            }),
            gradient: 'from-azulejo-blue-100 to-azulejo-cobalt-100',
            iconBg: 'bg-azulejo-blue-500',
            textColor: 'text-azulejo-blue-900',
            featured: true
        },
        {
            icon: '📈',
            title: t({ pt: 'Impacto', en: 'Impact' }),
            description: t({
                pt: 'Maior tempo de envolvimento, aprendizagem mais profunda, experiências memoráveis que os visitantes partilham.',
                en: 'Increased engagement time, deeper learning, memorable experiences that visitors share.'
            }),
            gradient: 'from-azulejo-gold-100 to-azulejo-gold-200',
            iconBg: 'bg-azulejo-gold-500',
            textColor: 'text-azulejo-gold-900'
        }
    ];

    return (
        <section
            id="problem-statement"
            className="py-16 sm:py-20 lg:py-24 bg-gray-50 dark:bg-gray-900"
        >
            <div className="container mx-auto px-4 sm:px-6 lg:px-8">
                {/* Section Heading - H2 for SEO */}
                <h2 className="text-3xl sm:text-4xl lg:text-5xl font-bold text-center mb-4 text-gray-900 dark:text-white">
                    {t({ pt: 'Porquê AR para Museus?', en: 'Why AR for Museums?' })}
                </h2>

                <p className="text-lg sm:text-xl text-center text-gray-600 dark:text-gray-300 mb-12 max-w-3xl mx-auto">
                    {t({
                        pt: 'A realidade aumentada transforma a forma como experienciamos a história e a arte.',
                        en: 'Augmented reality transforms how we experience history and art.'
                    })}
                </p>

                {/* Three-Column Card Layout */}
                <div className="grid grid-cols-1 md:grid-cols-3 gap-6 lg:gap-8">
                    {cards.map((card, index) => (
                        <div
                            key={index}
                            className={`
                                relative overflow-hidden rounded-2xl p-8
                                bg-gradient-to-br ${card.gradient}
                                shadow-md hover:shadow-xl
                                transition-all duration-300
                                hover:-translate-y-2
                                ${card.featured ? 'md:scale-105 md:shadow-lg' : ''}
                            `}
                        >
                            {/* Icon */}
                            <div className={`
                                ${card.iconBg} 
                                w-16 h-16 rounded-xl 
                                flex items-center justify-center 
                                text-3xl mb-6
                                ${card.featured ? 'text-white' : ''}
                                shadow-sm
                            `}>
                                {card.icon}
                            </div>

                            {/* Title */}
                            <h3 className={`
                                text-xl sm:text-2xl font-bold mb-4 
                                ${card.textColor}
                            `}>
                                {card.title}
                            </h3>

                            {/* Description */}
                            <p className={`
                                text-base sm:text-lg leading-relaxed
                                ${card.textColor} opacity-90
                            `}>
                                {card.description}
                            </p>

                            {/* Featured Badge */}
                            {card.featured && (
                                <div className="absolute top-4 right-4">
                                    <span className="bg-azulejo-gold-500 text-white text-xs font-bold px-3 py-1 rounded-full shadow-md">
                                        {t({ pt: 'Solução', en: 'Solution' })}
                                    </span>
                                </div>
                            )}
                        </div>
                    ))}
                </div>

                {/* Optional: Stats or Trust Indicators */}
                <div className="mt-16 grid grid-cols-1 sm:grid-cols-3 gap-8 text-center">
                    <div>
                        <div className="text-4xl sm:text-5xl font-bold text-azulejo-blue-600 dark:text-azulejo-blue-400 mb-2">
                            3x
                        </div>
                        <p className="text-gray-600 dark:text-gray-400">
                            {t({ pt: 'Mais Tempo de Visita', en: 'Longer Visit Time' })}
                        </p>
                    </div>
                    <div>
                        <div className="text-4xl sm:text-5xl font-bold text-azulejo-blue-600 dark:text-azulejo-blue-400 mb-2">
                            85%
                        </div>
                        <p className="text-gray-600 dark:text-gray-400">
                            {t({ pt: 'Maior Envolvimento', en: 'Higher Engagement' })}
                        </p>
                    </div>
                    <div>
                        <div className="text-4xl sm:text-5xl font-bold text-azulejo-blue-600 dark:text-azulejo-blue-400 mb-2">
                            95%
                        </div>
                        <p className="text-gray-600 dark:text-gray-400">
                            {t({ pt: 'Recomendariam', en: 'Would Recommend' })}
                        </p>
                    </div>
                </div>
            </div>
        </section>
    );
}

export default ProblemStatement;

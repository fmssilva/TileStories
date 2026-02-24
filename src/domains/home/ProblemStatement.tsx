/**
 * ProblemStatement Component - Why AR for Museums?
 * 
 * Purpose: Establish the value proposition of AR technology for museum experiences
 * Target: Tourists, educators, museum professionals
 * 
 * Design: Three-column card layout comparing Traditional → AR Solution → Impact
 * Colors: Gray (traditional), Azulejo Blue (AR), Azulejo Gold (impact)
 * Phase 5: Scroll-triggered fade-in animations for cards
 */

import { useInlineTranslation } from '@/utils/language';
import { useScrollAnimation, getScrollAnimationClasses } from '@/design';

export function ProblemStatement() {
    const t = useInlineTranslation;

    const { ref: sectionRef, isVisible } = useScrollAnimation<HTMLDivElement>({
        threshold: 0.1,
        once: true
    });

    const cards = [
        {
            icon: '🏛️',
            title: t({ pt: 'Museus Tradicionais', en: 'Traditional Museums' }),
            description: t({
                pt: 'Vitrines estáticas. Placas com texto. Imaginação limitada ao que está à frente dos olhos.',
                en: 'Static displays. Text plaques. Imagination limited to what\'s before your eyes.'
            }),
            gradient: 'from-slate-50 via-slate-100 to-slate-50',
            iconBg: 'bg-slate-200/80',
            textColor: 'text-slate-800',
            iconColor: 'grayscale opacity-60'
        },
        {
            icon: '✨',
            title: t({ pt: 'Realidade Aumentada', en: 'Augmented Reality' }),
            description: t({
                pt: 'Edifícios reconstroem-se no tempo. Épocas sobrepõem-se. A história acontece diante de si.',
                en: 'Buildings reconstruct through time. Epochs overlap. History unfolds before you.'
            }),
            gradient: 'from-azulejo-blue-500 via-azulejo-cobalt-500 to-azulejo-blue-600',
            iconBg: 'bg-white/20 backdrop-blur-sm',
            textColor: 'text-white',
            iconColor: '',
            featured: true,
            shine: true
        },
        {
            icon: '🎯',
            title: t({ pt: 'Resultado', en: 'Result' }),
            description: t({
                pt: 'Visitantes fascinados. Memórias duradouras. Histórias que se partilham.',
                en: 'Captivated visitors. Lasting memories. Stories worth sharing.'
            }),
            gradient: 'from-azulejo-gold-400 via-azulejo-gold-500 to-azulejo-gold-600',
            iconBg: 'bg-white/25 backdrop-blur-sm',
            textColor: 'text-white',
            iconColor: '',
            glow: true
        }
    ];

    return (
        <section
            ref={sectionRef}
            id="problem-statement"
            className="relative py-20 sm:py-24 lg:py-32 bg-gradient-to-b from-white via-slate-50 to-white dark:from-gray-900 dark:via-gray-800 dark:to-gray-900 overflow-hidden"
        >
            {/* Decorative background pattern */}
            <div className="absolute inset-0 opacity-[0.03] dark:opacity-[0.02]">
                <div className="absolute inset-0" style={{
                    backgroundImage: `url("data:image/svg+xml,%3Csvg width='60' height='60' viewBox='0 0 60 60' xmlns='http://www.w3.org/2000/svg'%3E%3Cpath d='M30 0L60 30L30 60L0 30z' fill='%23334155' fill-opacity='1'/%3E%3C/svg%3E")`,
                    backgroundSize: '60px 60px'
                }}></div>
            </div>

            <div className="container mx-auto px-4 sm:px-6 lg:px-8 relative z-10">
                {/* Header */}
                <div className={`text-center mb-16 sm:mb-20 ${getScrollAnimationClasses(isVisible, 'slide-up')}`}>
                    <div className="inline-block mb-4">
                        <span className="inline-flex items-center gap-2 px-4 py-2 rounded-full bg-azulejo-blue-100 dark:bg-azulejo-blue-900/30 text-azulejo-blue-700 dark:text-azulejo-blue-300 text-sm font-semibold border border-azulejo-blue-200 dark:border-azulejo-blue-800">
                            <span className="relative flex h-2 w-2">
                                <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-azulejo-blue-400 opacity-75"></span>
                                <span className="relative inline-flex rounded-full h-2 w-2 bg-azulejo-blue-500"></span>
                            </span>
                            {t({ pt: 'Tecnologia Inovadora', en: 'Innovative Technology' })}
                        </span>
                    </div>

                    <h2 className="text-4xl sm:text-5xl lg:text-6xl font-bold mb-6 bg-gradient-to-r from-azulejo-blue-600 via-azulejo-cobalt-600 to-azulejo-blue-600 bg-clip-text text-transparent dark:from-azulejo-blue-400 dark:via-azulejo-cobalt-400 dark:to-azulejo-blue-400">
                        {t({ pt: 'Do Estático ao Extraordinário', en: 'From Static to Extraordinary' })}
                    </h2>

                    <p className="text-lg sm:text-xl text-gray-600 dark:text-gray-300 max-w-3xl mx-auto leading-relaxed">
                        {t({
                            pt: 'Transforme cada visita numa viagem no tempo. A realidade aumentada não mostra apenas história — faz com que a viva.',
                            en: 'Transform every visit into a journey through time. Augmented reality doesn\'t just show history — it lets you live it.'
                        })}
                    </p>
                </div>

                {/* AR Demo Video */}
                <div className={`mb-16 sm:mb-20 max-w-5xl mx-auto ${getScrollAnimationClasses(isVisible, 'scale')}`}
                    style={{ transitionDelay: '150ms' }}>
                    <div className="relative group">
                        {/* Glow effect */}
                        <div className="absolute -inset-1 bg-gradient-to-r from-azulejo-blue-500 via-azulejo-gold-500 to-azulejo-blue-500 rounded-3xl blur-2xl opacity-20 group-hover:opacity-30 transition-opacity duration-500"></div>

                        <div className="relative rounded-2xl overflow-hidden shadow-2xl ring-1 ring-black/5 dark:ring-white/10">
                            <video
                                autoPlay
                                loop
                                muted
                                playsInline
                                className="w-full h-auto"
                                poster="/images/MostJeron.png"
                            >
                                <source src="/videos/MostJeron.mp4" type="video/mp4" />
                                {t({
                                    pt: 'Seu navegador não suporta a tag de vídeo.',
                                    en: 'Your browser does not support the video tag.'
                                })}
                            </video>

                            {/* Gradient overlay */}
                            <div className="absolute bottom-0 left-0 right-0 bg-gradient-to-t from-black/90 via-black/50 to-transparent p-6 sm:p-8">
                                <p className="text-white text-base sm:text-lg font-semibold tracking-wide flex items-center justify-center gap-3">
                                    <span className="inline-block w-1.5 h-1.5 rounded-full bg-azulejo-gold-400 animate-pulse"></span>
                                    {t({
                                        pt: 'Veja Lisboa renascer através dos séculos',
                                        en: 'Watch Lisbon reborn through the centuries'
                                    })}
                                </p>
                            </div>
                        </div>
                    </div>
                </div>

                {/* Cards */}
                <div className="grid grid-cols-1 lg:grid-cols-3 gap-6 lg:gap-8 max-w-7xl mx-auto mb-16 sm:mb-20">
                    {cards.map((card, index) => (
                        <div
                            key={index}
                            className={`
                                group relative overflow-hidden rounded-3xl p-8 lg:p-10
                                bg-gradient-to-br ${card.gradient}
                                shadow-xl hover:shadow-2xl
                                ${getScrollAnimationClasses(isVisible, 'slide-up')}
                                hover:-translate-y-2
                                transition-all duration-500
                                ${card.featured ? 'lg:scale-105 lg:shadow-2xl' : ''}
                            `}
                            style={{ transitionDelay: `${200 + index * 100}ms` }}
                        >
                            {/* Shine effect for featured card */}
                            {card.shine && (
                                <div className="absolute inset-0 opacity-0 group-hover:opacity-100 transition-opacity duration-700">
                                    <div className="absolute inset-0 bg-gradient-to-r from-transparent via-white/10 to-transparent -skew-x-12 translate-x-[-200%] group-hover:translate-x-[200%] transition-transform duration-1000"></div>
                                </div>
                            )}

                            {/* Glow effect */}
                            {card.glow && (
                                <div className="absolute -inset-0.5 bg-gradient-to-r from-azulejo-gold-400 to-azulejo-gold-600 rounded-3xl opacity-0 group-hover:opacity-20 blur-xl transition-opacity duration-500"></div>
                            )}

                            <div className="relative z-10">
                                {/* Icon */}
                                <div className={`
                                    ${card.iconBg} 
                                    w-16 h-16 rounded-2xl 
                                    flex items-center justify-center 
                                    text-4xl mb-6
                                    shadow-lg
                                    transform group-hover:scale-110 group-hover:rotate-3
                                    transition-all duration-500
                                    ${card.iconColor}
                                `}>
                                    {card.icon}
                                </div>

                                {/* Title */}
                                <h3 className={`
                                    text-2xl lg:text-3xl font-bold mb-4 
                                    ${card.textColor}
                                    leading-tight
                                `}>
                                    {card.title}
                                </h3>

                                {/* Description */}
                                <p className={`
                                    text-base lg:text-lg leading-relaxed
                                    ${card.textColor} ${card.featured ? 'opacity-95' : 'opacity-90'}
                                `}>
                                    {card.description}
                                </p>

                                {/* Arrow indicator for featured */}
                                {card.featured && (
                                    <div className="mt-6 flex items-center gap-2 text-white/90 text-sm font-semibold">
                                        <span>{t({ pt: 'A nossa solução', en: 'Our solution' })}</span>
                                        <svg className="w-5 h-5 transform group-hover:translate-x-1 transition-transform" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 8l4 4m0 0l-4 4m4-4H3" />
                                        </svg>
                                    </div>
                                )}
                            </div>

                            {/* Connection line for middle card */}
                            {index === 1 && (
                                <>
                                    <div className="hidden lg:block absolute -left-8 top-1/2 w-8 h-0.5 bg-gradient-to-r from-transparent to-azulejo-blue-400/50"></div>
                                    <div className="hidden lg:block absolute -right-8 top-1/2 w-8 h-0.5 bg-gradient-to-l from-transparent to-azulejo-gold-400/50"></div>
                                </>
                            )}
                        </div>
                    ))}
                </div>

                {/* Stats */}
                <div className={`grid grid-cols-1 sm:grid-cols-3 gap-8 max-w-4xl mx-auto ${getScrollAnimationClasses(isVisible, 'slide-up')}`}
                    style={{ transitionDelay: '500ms' }}>
                    {[
                        { value: '3x', label: t({ pt: 'Mais Tempo de Visita', en: 'Longer Visit Time' }) },
                        { value: '85%', label: t({ pt: 'Maior Envolvimento', en: 'Higher Engagement' }) },
                        { value: '95%', label: t({ pt: 'Recomendariam', en: 'Would Recommend' }) }
                    ].map((stat, index) => (
                        <div key={index} className="text-center group">
                            <div className="inline-flex flex-col items-center justify-center w-32 h-32 rounded-full bg-gradient-to-br from-azulejo-blue-100 to-azulejo-gold-100 dark:from-azulejo-blue-900/30 dark:to-azulejo-gold-900/30 mb-4 ring-4 ring-white dark:ring-gray-800 shadow-lg group-hover:scale-110 transition-transform duration-300">
                                <div className="text-4xl sm:text-5xl font-bold bg-gradient-to-br from-azulejo-blue-600 to-azulejo-gold-600 bg-clip-text text-transparent">
                                    {stat.value}
                                </div>
                            </div>
                            <p className="text-gray-600 dark:text-gray-400 font-medium">
                                {stat.label}
                            </p>
                        </div>
                    ))}
                </div>
            </div>
        </section>
    );
}

export default ProblemStatement;

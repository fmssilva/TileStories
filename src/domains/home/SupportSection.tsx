/**
 * SUPPORT SECTION
 * ===============
 * Call-to-action encouraging users to support the project development
 * Positioned after Features to capitalize on demonstrated value
 * 
 * Design Philosophy (based on Kickstarter/Patreon/NN Group research):
 * - Clear mission statement (most important factor - 3.6x)
 * - Financial transparency (how funds will be used)
 * - Concrete numbers (€3,500, 12 months, 4 phases)
 * - Social proof & progress indicators
 * - Community focus ("Join us" not "Give us money")
 * 
 * Layout: Hero message → Project stats → Timeline preview → CTA
 */
import { useInlineTranslation } from '@/utils/language';
import { useScrollAnimation, getScrollAnimationClasses } from '@/design';
import { Link } from 'react-router-dom';
import { projectMetadata } from '../workPlan/utils';
import { AnimatedCounter } from '@/design/animations';

export function SupportSection() {
    const t = useInlineTranslation;

    const { ref: sectionRef, isVisible } = useScrollAnimation<HTMLDivElement>({
        threshold: 0.1,
        once: true,
    });

    const deliverables = [
        {
            icon: '📱',
            title: t({ pt: 'Aplicação AR Completa', en: 'Complete AR Application' }),
            description: t({
                pt: 'App iOS e Android com reconhecimento de imagem e modelos 3D interativos',
                en: 'iOS & Android app with image recognition and interactive 3D models',
            })
        },
        {
            icon: '🗺️',
            title: t({ pt: 'Mapa Histórico Interativo', en: 'Interactive Historical Map' }),
            description: t({
                pt: 'Navegação entre 4 épocas com 150+ edifícios identificados',
                en: 'Navigate through 4 epochs with 150+ identified buildings',
            })
        },
        {
            icon: '⚡',
            title: t({ pt: 'Simulação do Terramoto', en: 'Earthquake Simulation' }),
            description: t({
                pt: 'Experiência imersiva do terramoto de 1755 que remodelou Lisboa',
                en: 'Immersive experience of the 1755 earthquake that reshaped Lisbon',
            })
        },
        {
            icon: '📚',
            title: t({ pt: 'Conteúdo Educativo', en: 'Educational Content' }),
            description: t({
                pt: 'Narrativas históricas e informações curadas por especialistas do museu',
                en: 'Historical narratives and information curated by museum experts',
            })
        }
    ];

    return (
        <section
            ref={sectionRef}
            className="relative py-20 sm:py-24 lg:py-32 bg-gradient-to-br from-azulejo-gold-500 via-azulejo-gold-600 to-azulejo-gold-500 dark:from-azulejo-gold-700 dark:via-azulejo-gold-800 dark:to-azulejo-gold-700 overflow-hidden"
            aria-labelledby="support-heading"
        >
            {/* Animated background elements */}
            <div className="absolute inset-0 overflow-hidden">
                <div className="absolute top-0 right-0 w-[600px] h-[600px] bg-white/10 rounded-full blur-3xl translate-x-1/3 -translate-y-1/3 animate-pulse"></div>
                <div className="absolute bottom-0 left-0 w-[600px] h-[600px] bg-white/10 rounded-full blur-3xl -translate-x-1/3 translate-y-1/3 animate-pulse" style={{ animationDelay: '1s' }}></div>
                <div className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[800px] h-[800px] bg-gradient-to-br from-white/5 to-transparent rounded-full blur-3xl"></div>
            </div>

            {/* Pattern overlay */}
            <div className="absolute inset-0 opacity-[0.03]">
                <div className="absolute inset-0" style={{
                    backgroundImage: `url("data:image/svg+xml,%3Csvg width='60' height='60' viewBox='0 0 60 60' xmlns='http://www.w3.org/2000/svg'%3E%3Cpath d='M30 0L60 30L30 60L0 30z' fill='%23ffffff' fill-opacity='1'/%3E%3C/svg%3E")`,
                    backgroundSize: '60px 60px'
                }}></div>
            </div>

            <div className="container mx-auto px-4 sm:px-6 lg:px-8 relative z-10">
                {/* Header */}
                <div className={`text-center mb-16 sm:mb-20 ${getScrollAnimationClasses(isVisible, 'slide-up')}`}>
                    <div className="inline-flex items-center justify-center w-24 h-24 bg-white/20 backdrop-blur-xl rounded-3xl mb-8 shadow-2xl border-2 border-white/40 transform hover:scale-110 transition-transform duration-500">
                        <span className="text-6xl">🤝</span>
                    </div>

                    <h2
                        id="support-heading"
                        className="text-4xl sm:text-5xl lg:text-6xl font-bold text-white mb-6 drop-shadow-2xl"
                    >
                        {t({
                            pt: 'Ajude a Dar Vida à História',
                            en: 'Help Bring History to Life',
                        })}
                    </h2>

                    <p className="text-lg sm:text-xl text-white/95 max-w-4xl mx-auto drop-shadow-lg leading-relaxed font-medium">
                        {t({
                            pt: 'O TileStories é um projeto de tese de mestrado que precisa do seu apoio para se tornar realidade. Com um plano claro de desenvolvimento e investimento transparente, estamos a criar uma plataforma que revoluciona como experienciamos o património cultural.',
                            en: 'TileStories is a master\'s thesis project that needs your support to become reality. With a clear development plan and transparent investment, we\'re creating a platform that revolutionizes how we experience cultural heritage.',
                        })}
                    </p>
                </div>

                {/* Project Stats Grid */}
                <div className={`grid grid-cols-1 sm:grid-cols-3 gap-6 lg:gap-8 mb-16 max-w-5xl mx-auto ${getScrollAnimationClasses(isVisible, 'scale')}`}
                    style={{ transitionDelay: '150ms' }}>
                    {[
                        {
                            value: projectMetadata.duration,
                            label: t({ pt: 'Meses de Desenvolvimento', en: 'Months Development' }),
                            icon: '📅',
                            suffix: ''
                        },
                        {
                            value: projectMetadata.totalCostWithBuffer,
                            label: t({ pt: 'Investimento Total', en: 'Total Investment' }),
                            icon: '💰',
                            suffix: '€'
                        },
                        {
                            value: projectMetadata.phaseCount,
                            label: t({ pt: 'Fases de Desenvolvimento', en: 'Development Phases' }),
                            icon: '🎯',
                            suffix: ''
                        }
                    ].map((stat, index) => (
                        <div
                            key={index}
                            className="group relative bg-white/20 backdrop-blur-xl rounded-3xl p-8 shadow-2xl border border-white/30 hover:bg-white/30 hover:scale-105 hover:shadow-3xl transition-all duration-500"
                        >
                            {/* Glow effect */}
                            <div className="absolute -inset-0.5 bg-white/20 rounded-3xl opacity-0 group-hover:opacity-100 blur-xl transition-opacity duration-500"></div>

                            <div className="relative z-10 text-center">
                                <div className="text-4xl mb-4 transform group-hover:scale-110 transition-transform duration-300">
                                    {stat.icon}
                                </div>
                                <p className="text-5xl lg:text-6xl font-black text-white mb-3 drop-shadow-lg">
                                    {stat.suffix === '€' && stat.suffix}
                                    <AnimatedCounter target={stat.value} />
                                    {stat.suffix !== '€' && stat.suffix}
                                </p>
                                <p className="text-white/95 font-semibold text-base lg:text-lg">
                                    {stat.label}
                                </p>
                            </div>
                        </div>
                    ))}
                </div>

                {/* What Your Support Enables */}
                <div className={`bg-white/15 backdrop-blur-2xl rounded-3xl p-8 sm:p-12 border-2 border-white/30 shadow-2xl mb-16 ${getScrollAnimationClasses(isVisible, 'slide-up')}`}
                    style={{ transitionDelay: '300ms' }}>
                    <div className="text-center mb-10">
                        <div className="inline-flex items-center gap-3 px-6 py-3 bg-white/20 backdrop-blur-sm rounded-full border border-white/30 mb-6">
                            <svg className="w-6 h-6 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 10V3L4 14h7v7l9-11h-7z" />
                            </svg>
                            <span className="text-white font-bold text-sm uppercase tracking-wider">
                                {t({ pt: 'Entregáveis', en: 'Deliverables' })}
                            </span>
                        </div>

                        <h3 className="text-3xl sm:text-4xl font-bold text-white mb-4">
                            {t({
                                pt: 'O Que o Seu Apoio Permite',
                                en: 'What Your Support Enables',
                            })}
                        </h3>
                    </div>

                    <div className="grid grid-cols-1 md:grid-cols-2 gap-6 lg:gap-8">
                        {deliverables.map((item, index) => (
                            <div
                                key={index}
                                className="group flex items-start gap-4 p-6 bg-white/10 backdrop-blur-sm rounded-2xl border border-white/20 hover:bg-white/20 hover:scale-105 transition-all duration-300"
                            >
                                <div className="flex-shrink-0 w-14 h-14 bg-white/25 backdrop-blur-sm rounded-xl flex items-center justify-center text-3xl shadow-lg group-hover:scale-110 group-hover:rotate-6 transition-all duration-300">
                                    {item.icon}
                                </div>
                                <div>
                                    <h4 className="text-xl font-bold text-white mb-2">
                                        {item.title}
                                    </h4>
                                    <p className="text-white/90 leading-relaxed">
                                        {item.description}
                                    </p>
                                </div>
                            </div>
                        ))}
                    </div>
                </div>

                {/* Call to Action */}
                <div className={`text-center ${getScrollAnimationClasses(isVisible, 'scale')}`}
                    style={{ transitionDelay: '450ms' }}>
                    <Link
                        to="/work-plan"
                        className="group relative inline-flex items-center gap-4 px-12 py-6 
                                 bg-white hover:bg-white/95
                                 text-azulejo-gold-700 dark:text-azulejo-gold-800
                                 font-black text-xl lg:text-2xl rounded-2xl
                                 shadow-2xl hover:shadow-3xl
                                 transform hover:scale-110
                                 transition-all duration-500
                                 overflow-hidden"
                    >
                        {/* Animated background */}
                        <div className="absolute inset-0 bg-gradient-to-r from-azulejo-gold-50 via-white to-azulejo-gold-50 opacity-0 group-hover:opacity-100 transition-opacity duration-500"></div>

                        {/* Shine effect */}
                        <div className="absolute inset-0 -translate-x-full group-hover:translate-x-full 
                                     transition-transform duration-1000 ease-out
                                     bg-gradient-to-r from-transparent via-azulejo-gold-200/50 to-transparent"></div>

                        {/* Pulse rings */}
                        <div className="absolute inset-0 rounded-2xl">
                            <div className="absolute inset-0 rounded-2xl bg-white/30 animate-ping opacity-0 group-hover:opacity-100"></div>
                        </div>

                        <span className="relative z-10 flex items-center gap-4">
                            <span className="flex items-center justify-center w-12 h-12 bg-azulejo-gold-100 rounded-xl group-hover:rotate-12 transition-transform duration-300">
                                📋
                            </span>
                            <span>{t({ pt: 'Ver Plano Detalhado', en: 'View Detailed Plan' })}</span>
                        </span>

                        <svg
                            className="relative z-10 w-6 h-6 transform group-hover:translate-x-2 transition-transform duration-300"
                            fill="none"
                            viewBox="0 0 24 24"
                            stroke="currentColor"
                        >
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={3} d="M17 8l4 4m0 0l-4 4m4-4H3" />
                        </svg>
                    </Link>

                    <div className="mt-8 flex items-center justify-center gap-2 text-white/90">
                        <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                        </svg>
                        <p className="text-base sm:text-lg font-medium max-w-2xl">
                            {t({
                                pt: 'Detalhes completos: cronograma de 12 meses, breakdown financeiro e formas de apoiar',
                                en: 'Complete details: 12-month timeline, financial breakdown, and ways to support',
                            })}
                        </p>
                    </div>

                    {/* Trust indicators */}
                    <div className="mt-12 flex flex-wrap items-center justify-center gap-8 text-white/80">
                        <div className="flex items-center gap-2">
                            <svg className="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z" />
                            </svg>
                            <span className="font-semibold">{t({ pt: 'Transparente', en: 'Transparent' })}</span>
                        </div>
                        <div className="flex items-center gap-2">
                            <svg className="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
                            </svg>
                            <span className="font-semibold">{t({ pt: 'Plano Claro', en: 'Clear Plan' })}</span>
                        </div>
                        <div className="flex items-center gap-2">
                            <svg className="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z" />
                            </svg>
                            <span className="font-semibold">{t({ pt: 'Comunidade', en: 'Community' })}</span>
                        </div>
                    </div>
                </div>
            </div>
        </section>
    );
}

export default SupportSection;
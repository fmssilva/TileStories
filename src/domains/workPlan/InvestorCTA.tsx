/**
 * INVESTOR CTA SECTION
 * ====================
 * 
 * Call-to-action for potential investors
 * Gold gradient banner with contact information
 * 
 * Layout: Centered CTA with email contact
 * Color: Gold gradient (#D4AF37)
 */

import { useInlineTranslation } from '@/utils/language';
import { projectMetadata } from './utils';
import { useScrollAnimation } from '@/design';
import { AnimatedCounter } from '@/design/animations';
import { Link } from 'react-router-dom';

export function InvestorCTA() {
    const title = useInlineTranslation(
        'Pronto para Investir no Futuro do Patrimônio Cultural Digital?',
        'Ready to Invest in the Future of Digital Cultural Heritage?'
    );

    const description = useInlineTranslation(
        'TileStories representa uma oportunidade única de combinar impacto cultural com inovação tecnológica. Com um plano claro de 12 meses e investimento de €3,500, estamos prontos para lançar uma plataforma completa de patrimônio digital.',
        'TileStories represents a unique opportunity to combine cultural impact with technological innovation. With a clear 12-month plan and €3,500 investment, we are ready to launch a comprehensive digital heritage platform.'
    );

    const ctaButton = useInlineTranslation('Entre em Contato', 'Get in Touch');

    // Scroll animation for the entire section
    const { ref: sectionRef, isVisible } = useScrollAnimation<HTMLDivElement>({
        threshold: 0.2,
        once: true,
    });

    return (
        <section
            ref={sectionRef}
            className="py-20 sm:py-28 bg-gradient-to-br from-azulejo-gold-600 via-azulejo-gold-500 to-azulejo-terracotta-500 
                     dark:from-azulejo-gold-700 dark:via-azulejo-gold-600 dark:to-azulejo-terracotta-600
                     relative overflow-hidden">

            {/* Animated background pattern */}
            <div className="absolute inset-0 opacity-10">
                <div className="absolute top-0 left-0 w-96 h-96 bg-white rounded-full blur-3xl -translate-x-1/2 -translate-y-1/2"></div>
                <div className="absolute bottom-0 right-0 w-96 h-96 bg-white rounded-full blur-3xl translate-x-1/2 translate-y-1/2"></div>
            </div>

            <div className={`max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 text-center relative z-10
                           transition-all duration-700 ${isVisible ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-8'}`}>
                {/* Enhanced Icon with animation */}
                <div className="flex justify-center mb-8">
                    <div className={`w-24 h-24 bg-white/25 backdrop-blur-md rounded-full 
                                   flex items-center justify-center shadow-2xl border-2 border-white/30
                                   transition-all duration-700 delay-100
                                   ${isVisible ? 'scale-100 rotate-0' : 'scale-50 rotate-180'}`}>
                        <span className="text-6xl" aria-hidden="true">
                            🤝
                        </span>
                    </div>
                </div>

                {/* Title with stagger animation */}
                <h2 className={`text-3xl sm:text-5xl lg:text-6xl font-bold text-white mb-8 drop-shadow-lg
                              transition-all duration-700 delay-200
                              ${isVisible ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-8'}`}>
                    {title}
                </h2>

                {/* Description with stagger animation */}
                <p className={`text-lg sm:text-xl lg:text-2xl text-white/95 mb-12 max-w-3xl mx-auto 
                             drop-shadow leading-relaxed
                             transition-all duration-700 delay-300
                             ${isVisible ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-8'}`}>
                    {description}
                </p>

                {/* Enhanced CTA Button */}
                <Link
                    to="/contact"
                    className={`group inline-flex items-center gap-3 px-10 py-5 
                             bg-white/95 hover:bg-white text-azulejo-gold-700 dark:text-azulejo-gold-600
                             font-bold text-xl rounded-xl shadow-2xl
                             hover:shadow-3xl hover:scale-110
                             transition-all duration-500
                             relative overflow-hidden
                             ${isVisible ? 'opacity-100 scale-100' : 'opacity-0 scale-90'}`}
                    style={{ transitionDelay: '400ms' }}
                    aria-label={`${ctaButton}: ${useInlineTranslation('ir para página de contacto', 'go to contact page')}`}
                >
                    {/* Animated shine effect */}
                    <div className="absolute inset-0 -translate-x-full group-hover:translate-x-full 
                                 transition-transform duration-1000 ease-out
                                 bg-gradient-to-r from-transparent via-white/50 to-transparent" />

                    {/* Pulse effect */}
                    <div className="absolute inset-0 rounded-xl bg-white/20 animate-ping opacity-0 group-hover:opacity-100"></div>

                    <span className="relative z-10 flex items-center gap-2">
                        <span>✉️</span>
                        {ctaButton}
                    </span>
                    <span className="relative z-10 transition-transform duration-300 group-hover:translate-x-2"
                        aria-hidden="true">→</span>
                </Link>

                {/* Animated Stats Row with counters */}
                <div className={`grid grid-cols-3 gap-6 mt-16 max-w-3xl mx-auto
                               transition-all duration-700 delay-600
                               ${isVisible ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-8'}`}>
                    <div className="group p-6 bg-white/15 backdrop-blur-md rounded-2xl shadow-xl 
                                  border border-white/20 hover:bg-white/25 hover:scale-110 
                                  transition-all duration-300">
                        <p className="text-4xl lg:text-5xl font-bold text-white mb-2">
                            <AnimatedCounter target={projectMetadata.duration} suffix="" />
                        </p>
                        <p className="text-sm lg:text-base text-white/90 font-medium">
                            {useInlineTranslation('Meses', 'Months')}
                        </p>
                    </div>
                    <div className="group p-6 bg-white/15 backdrop-blur-md rounded-2xl shadow-xl 
                                  border border-white/20 hover:bg-white/25 hover:scale-110 
                                  transition-all duration-300">
                        <p className="text-4xl lg:text-5xl font-bold text-white mb-2">
                            €<AnimatedCounter target={projectMetadata.totalCostWithBuffer} suffix="" />
                        </p>
                        <p className="text-sm lg:text-base text-white/90 font-medium">
                            {useInlineTranslation('Investimento', 'Investment')}
                        </p>
                    </div>
                    <div className="group p-6 bg-white/15 backdrop-blur-md rounded-2xl shadow-xl 
                                  border border-white/20 hover:bg-white/25 hover:scale-110 
                                  transition-all duration-300">
                        <p className="text-4xl lg:text-5xl font-bold text-white mb-2">
                            <AnimatedCounter target={projectMetadata.phaseCount} suffix="" />
                        </p>
                        <p className="text-sm lg:text-base text-white/90 font-medium">
                            {useInlineTranslation('Fases', 'Phases')}
                        </p>
                    </div>
                </div>
            </div>
        </section>
    );
}

export default InvestorCTA;

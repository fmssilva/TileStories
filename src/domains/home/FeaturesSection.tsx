/**
 * FEATURES SECTION
 * ================
 * Quick overview of app capabilities for scanning visitors
 * 
 * Layout: 3-column grid (responsive)
 * Design: Simple cards with emoji icons, checkmark style
 * Phase 2: Static display
 * Phase 3: Add hover animations, possibly video demos on click
 * Phase 5: Scroll-triggered animations for feature cards
 */
import { useInlineTranslation } from '@/utils/language';
import { useScrollAnimation, getScrollAnimationClasses } from '@/design';

interface Feature {
    id: number;
    icon: string;
    title: {
        pt: string;
        en: string;
    };
    description: {
        pt: string;
        en: string;
    };
    highlight?: boolean; // Para destacar features principais
}

const featuresData: Feature[] = [
    {
        id: 1,
        icon: '🏛️',
        title: {
            pt: '150+ Edifícios Identificados',
            en: '150+ Identified Buildings',
        },
        description: {
            pt: 'Toque em qualquer estrutura para descobrir seu nome, propósito e destino histórico',
            en: 'Tap any structure to learn its name, purpose, and historical fate',
        },
        highlight: true,
    },
    {
        id: 2,
        icon: '⏳',
        title: {
            pt: '4 Épocas Históricas',
            en: '4 Historical Epochs',
        },
        description: {
            pt: 'Deslize entre períodos temporais para ver a evolução de Lisboa através dos séculos',
            en: 'Slide between time periods to see Lisbon\'s evolution through the centuries',
        },
        highlight: true,
    },
    {
        id: 3,
        icon: '⚡',
        title: {
            pt: 'Simulação do Terramoto',
            en: 'Earthquake Simulation',
        },
        description: {
            pt: 'Experiencie o terramoto de 1755 que remodelou a cidade numa recriação interativa',
            en: 'Experience the 1755 earthquake that reshaped the city in an interactive recreation',
        },
        highlight: true,
    },
    {
        id: 4,
        icon: '🗺️',
        title: {
            pt: 'Mapa Interativo',
            en: 'Interactive Map',
        },
        description: {
            pt: 'Navegue pelo panorama com uma interface de mapa fácil de usar e zoom suave',
            en: 'Navigate the panorama with an easy-to-use map interface and smooth zoom',
        },
    },
    {
        id: 5,
        icon: '📚',
        title: {
            pt: 'Conteúdo Educativo',
            en: 'Educational Content',
        },
        description: {
            pt: 'Aprenda com historiadores especialistas e curadores do Museu do Azulejo',
            en: 'Learn from expert historians and curators at the Museu do Azulejo',
        },
    },
    {
        id: 6,
        icon: '🌍',
        title: {
            pt: 'Suporte Multilíngue',
            en: 'Multilingual Support',
        },
        description: {
            pt: 'Disponível em Português e Inglês para visitantes locais e internacionais',
            en: 'Available in Portuguese and English for local and international visitors',
        },
    },
];

export default function FeaturesSection() {
    const t = useInlineTranslation;

    const { ref: sectionRef, isVisible } = useScrollAnimation<HTMLDivElement>({
        threshold: 0.1,
        once: true
    });

    return (
        <section
            ref={sectionRef}
            id="features-section"
            className="relative py-20 sm:py-24 lg:py-32 bg-gradient-to-b from-white via-slate-50 to-white dark:from-gray-950 dark:via-gray-900 dark:to-gray-950 overflow-hidden"
            aria-labelledby="features-heading"
        >
            {/* Decorative background elements */}
            <div className="absolute inset-0 overflow-hidden pointer-events-none">
                <div className="absolute top-0 right-0 w-96 h-96 bg-azulejo-blue-100/30 dark:bg-azulejo-blue-900/10 rounded-full blur-3xl"></div>
                <div className="absolute bottom-0 left-0 w-96 h-96 bg-azulejo-gold-100/30 dark:bg-azulejo-gold-900/10 rounded-full blur-3xl"></div>
            </div>

            {/* Grid pattern overlay */}
            <div className="absolute inset-0 opacity-[0.02] dark:opacity-[0.03]">
                <div className="absolute inset-0" style={{
                    backgroundImage: `url("data:image/svg+xml,%3Csvg width='40' height='40' viewBox='0 0 40 40' xmlns='http://www.w3.org/2000/svg'%3E%3Cg fill='%23334155' fill-opacity='1' fill-rule='evenodd'%3E%3Cpath d='M0 40L40 0H20L0 20M40 40V20L20 40'/%3E%3C/g%3E%3C/svg%3E")`,
                    backgroundSize: '40px 40px'
                }}></div>
            </div>

            <div className="container mx-auto px-4 sm:px-6 lg:px-8 relative z-10">
                {/* Header */}
                <div className={`text-center mb-16 sm:mb-20 ${getScrollAnimationClasses(isVisible, 'slide-up')}`}>
                    <div className="inline-block mb-6">
                        <span className="inline-flex items-center gap-2 px-5 py-2.5 rounded-full bg-gradient-to-r from-azulejo-blue-100 to-azulejo-blue-50 dark:from-azulejo-blue-900/30 dark:to-azulejo-blue-800/20 border border-azulejo-blue-200 dark:border-azulejo-blue-700/50 text-azulejo-blue-700 dark:text-azulejo-blue-300 text-sm font-semibold shadow-sm">
                            <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
                            </svg>
                            {t({ pt: 'Funcionalidades', en: 'Features' })}
                        </span>
                    </div>

                    <h2
                        id="features-heading"
                        className="text-4xl sm:text-5xl lg:text-6xl font-bold mb-6 bg-gradient-to-r from-azulejo-blue-600 via-azulejo-cobalt-600 to-azulejo-blue-600 bg-clip-text text-transparent dark:from-azulejo-blue-400 dark:via-azulejo-cobalt-400 dark:to-azulejo-blue-400"
                    >
                        {t({
                            pt: 'O Que Irá Descobrir',
                            en: 'What You\'ll Discover',
                        })}
                    </h2>

                    <p className={`text-lg sm:text-xl text-gray-600 dark:text-gray-300 max-w-3xl mx-auto leading-relaxed ${getScrollAnimationClasses(isVisible, 'slide-up')}`}
                        style={{ transitionDelay: '100ms' }}>
                        {t({
                            pt: 'Uma experiência completa que combina história, tecnologia e aprendizagem interativa',
                            en: 'A complete experience combining history, technology, and interactive learning',
                        })}
                    </p>
                </div>

                {/* Features Grid */}
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 lg:gap-8 max-w-7xl mx-auto">
                    {featuresData.map((feature, index) => (
                        <div
                            key={feature.id}
                            className={`
                                group relative overflow-hidden
                                bg-white dark:bg-gray-900
                                rounded-2xl
                                border-2 ${feature.highlight ? 'border-azulejo-blue-200 dark:border-azulejo-blue-800' : 'border-gray-200 dark:border-gray-800'}
                                hover:border-azulejo-blue-400 dark:hover:border-azulejo-blue-600
                                shadow-lg hover:shadow-2xl
                                transform hover:-translate-y-2
                                transition-all duration-500
                                ${getScrollAnimationClasses(isVisible, 'scale')}
                            `}
                            style={{ transitionDelay: `${200 + index * 75}ms` }}
                            role="article"
                            aria-labelledby={`feature-${feature.id}-title`}
                        >
                            {/* Gradient overlay on hover */}
                            <div className="absolute inset-0 bg-gradient-to-br from-azulejo-blue-50/0 via-azulejo-blue-50/50 to-azulejo-gold-50/50 dark:from-azulejo-blue-900/0 dark:via-azulejo-blue-900/20 dark:to-azulejo-gold-900/20 opacity-0 group-hover:opacity-100 transition-opacity duration-500"></div>

                            {/* Highlight badge for featured items */}
                            {feature.highlight && (
                                <div className="absolute top-4 right-4 z-10">
                                    <div className="px-3 py-1 rounded-full bg-gradient-to-r from-azulejo-gold-500 to-azulejo-gold-600 text-white text-xs font-bold shadow-md">
                                        {t({ pt: 'Destaque', en: 'Featured' })}
                                    </div>
                                </div>
                            )}

                            <div className="relative z-10 p-8">
                                {/* Icon */}
                                <div className="mb-6">
                                    <div className="inline-flex items-center justify-center w-20 h-20 rounded-2xl bg-gradient-to-br from-azulejo-blue-100 to-azulejo-blue-50 dark:from-azulejo-blue-900/50 dark:to-azulejo-blue-800/50 group-hover:from-azulejo-blue-500 group-hover:to-azulejo-blue-600 dark:group-hover:from-azulejo-blue-600 dark:group-hover:to-azulejo-blue-700 shadow-lg group-hover:shadow-xl transform group-hover:scale-110 group-hover:rotate-6 transition-all duration-500">
                                        <span className="text-4xl group-hover:scale-110 transition-transform duration-300">
                                            {feature.icon}
                                        </span>
                                    </div>
                                </div>

                                {/* Title */}
                                <h3
                                    id={`feature-${feature.id}-title`}
                                    className="text-xl lg:text-2xl font-bold text-gray-900 dark:text-white mb-4 group-hover:text-azulejo-blue-600 dark:group-hover:text-azulejo-blue-400 transition-colors duration-300"
                                >
                                    {t(feature.title)}
                                </h3>

                                {/* Description */}
                                <p className="text-gray-600 dark:text-gray-400 leading-relaxed">
                                    {t(feature.description)}
                                </p>

                                {/* Checkmark indicator */}
                                <div className="mt-6 flex items-center gap-2 text-azulejo-blue-600 dark:text-azulejo-blue-400 text-sm font-semibold opacity-0 group-hover:opacity-100 transform translate-y-2 group-hover:translate-y-0 transition-all duration-300">
                                    <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                                    </svg>
                                    <span>{t({ pt: 'Incluído', en: 'Included' })}</span>
                                </div>
                            </div>

                            {/* Decorative corner */}
                            <div className="absolute bottom-0 right-0 w-24 h-24 bg-gradient-to-tl from-azulejo-blue-100/30 dark:from-azulejo-blue-900/30 to-transparent rounded-tl-[3rem] opacity-0 group-hover:opacity-100 transition-opacity duration-500"></div>
                        </div>
                    ))}
                </div>

                {/* Bottom stats/highlights */}
                <div className={`mt-16 sm:mt-20 grid grid-cols-1 sm:grid-cols-3 gap-8 max-w-4xl mx-auto ${getScrollAnimationClasses(isVisible, 'slide-up')}`}
                    style={{ transitionDelay: '650ms' }}>
                    <div className="text-center group">
                        <div className="inline-flex items-center justify-center w-16 h-16 rounded-full bg-gradient-to-br from-azulejo-blue-100 to-azulejo-blue-50 dark:from-azulejo-blue-900/30 dark:to-azulejo-blue-800/20 mb-4 group-hover:scale-110 transition-transform duration-300">
                            <svg className="w-8 h-8 text-azulejo-blue-600 dark:text-azulejo-blue-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.747 0 3.332.477 4.5 1.253v13C19.832 18.477 18.247 18 16.5 18c-1.746 0-3.332.477-4.5 1.253" />
                            </svg>
                        </div>
                        <p className="text-2xl font-bold text-gray-900 dark:text-white mb-2">
                            {t({ pt: 'Rica em História', en: 'History Rich' })}
                        </p>
                        <p className="text-sm text-gray-600 dark:text-gray-400">
                            {t({ pt: 'Conteúdo curado por especialistas', en: 'Expert-curated content' })}
                        </p>
                    </div>

                    <div className="text-center group">
                        <div className="inline-flex items-center justify-center w-16 h-16 rounded-full bg-gradient-to-br from-azulejo-gold-100 to-azulejo-gold-50 dark:from-azulejo-gold-900/30 dark:to-azulejo-gold-800/20 mb-4 group-hover:scale-110 transition-transform duration-300">
                            <svg className="w-8 h-8 text-azulejo-gold-600 dark:text-azulejo-gold-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9.663 17h4.673M12 3v1m6.364 1.636l-.707.707M21 12h-1M4 12H3m3.343-5.657l-.707-.707m2.828 9.9a5 5 0 117.072 0l-.548.547A3.374 3.374 0 0014 18.469V19a2 2 0 11-4 0v-.531c0-.895-.356-1.754-.988-2.386l-.548-.547z" />
                            </svg>
                        </div>
                        <p className="text-2xl font-bold text-gray-900 dark:text-white mb-2">
                            {t({ pt: 'Intuitiva', en: 'Intuitive' })}
                        </p>
                        <p className="text-sm text-gray-600 dark:text-gray-400">
                            {t({ pt: 'Fácil de usar para todas as idades', en: 'Easy to use for all ages' })}
                        </p>
                    </div>

                    <div className="text-center group">
                        <div className="inline-flex items-center justify-center w-16 h-16 rounded-full bg-gradient-to-br from-azulejo-cobalt-100 to-azulejo-cobalt-50 dark:from-azulejo-cobalt-900/30 dark:to-azulejo-cobalt-800/20 mb-4 group-hover:scale-110 transition-transform duration-300">
                            <svg className="w-8 h-8 text-azulejo-cobalt-600 dark:text-azulejo-cobalt-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 10V3L4 14h7v7l9-11h-7z" />
                            </svg>
                        </div>
                        <p className="text-2xl font-bold text-gray-900 dark:text-white mb-2">
                            {t({ pt: 'Envolvente', en: 'Engaging' })}
                        </p>
                        <p className="text-sm text-gray-600 dark:text-gray-400">
                            {t({ pt: 'Experiência imersiva e interativa', en: 'Immersive interactive experience' })}
                        </p>
                    </div>
                </div>
            </div>
        </section>
    );
}
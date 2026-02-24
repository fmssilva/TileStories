/**
 * AR DEMO SECTION
 * ================
 * Showcases the AR experience through screenshots
 * Phase 2: Static grid layout with 4 demo images
 * Phase 3: Add click to open modal with video demos
 * Phase 5: Scroll-triggered animations for demo cards
 */
import { useState } from 'react';
import { useLanguage } from '@/utils/language';
import { useScrollAnimation, getScrollAnimationClasses } from '@/design';
import { Modal, VideoModal, ComingSoonBadge, SequentialVideoPlayer } from '@/components/ui';
import type { InlineTranslation } from '@/utils/language';

// Videos for the full demo - plays sequentially
const fullDemoVideos = [
    '/videos/Lisbon_AR_vide_4_views.mp4',
    '/videos/Lisbon_AR_video.mp4',
    '/videos/MostJeron.mp4',
];

interface ARDemoItem {
    id: number;
    imageSrc: string;
    videoSrc?: string;
    imageAlt: {
        pt: string;
        en: string;
    };
    caption: {
        pt: string;
        en: string;
    };
}

export type { ARDemoItem };

const arDemoData: ARDemoItem[] = [
    {
        id: 1,
        imageSrc: '/images/Lisbon_Mosaic_AR_1.png',
        videoSrc: '/videos/Lisbon_AR_vide_4_views.mp4',
        imageAlt: {
            pt: 'Vista AR de azulejo com sobreposição de informações interativas',
            en: 'AR view of azulejo tile with interactive information overlay',
        },
        caption: {
            pt: 'Aponte para Explorar o Passado',
            en: 'Point to Explore the Past',
        },
    },
    {
        id: 2,
        imageSrc: '/images/Lisbon_Panorama_AR_1.png',
        videoSrc: '/videos/Lisbon_AR_video.mp4',
        imageAlt: {
            pt: 'Panorama com ícones de realidade aumentada para navegação histórica',
            en: 'Panorama with AR icons for historical navigation',
        },
        caption: {
            pt: 'Descubra edifícios com ícones interativos',
            en: 'Discover buildings with interactive icons',
        },
    },
    {
        id: 3,
        imageSrc: '/images/MostJeron.png',
        videoSrc: '/videos/MostJeron.mp4',
        imageAlt: {
            pt: 'Demonstração AR do Mosteiro dos Jerónimos',
            en: 'AR demonstration of Jerónimos Monastery',
        },
        caption: {
            pt: 'Viaje no tempo com realidade aumentada',
            en: 'Travel through time with interactive timeline',
        },
    },
    {
        id: 4,
        imageSrc: '/images/earthquake.png',
        imageAlt: {
            pt: 'Simulação do terramoto de Lisboa de 1755',
            en: 'Lisbon 1755 earthquake simulation',
        },
        caption: {
            pt: 'Experimente o terramoto de 1755',
            en: 'Experience the 1755 earthquake',
        },
    },
];

// Representative icons for each AR feature
const demoIcons = ['📱', '🏛️', '🕰️', '🌊'];

export default function ARDemoSection() {
    const { language } = useLanguage();
    const [selectedDemo, setSelectedDemo] = useState<ARDemoItem | null>(null);
    const [showFullDemoVideo, setShowFullDemoVideo] = useState(false);

    const { ref: sectionRef, isVisible } = useScrollAnimation<HTMLDivElement>({
        threshold: 0.1,
        once: true
    });

    const t = (text: InlineTranslation): string => {
        return language === 'pt' ? text.pt : text.en;
    };

    return (
        <section
            ref={sectionRef}
            className="relative py-20 sm:py-24 lg:py-32 bg-gradient-to-b from-white via-azulejo-blue-50/30 to-white dark:from-gray-900 dark:via-gray-850 dark:to-gray-900 overflow-hidden"
            aria-labelledby="ar-demo-heading"
        >
            {/* Decorative background elements */}
            <div className="absolute inset-0 overflow-hidden pointer-events-none">
                <div className="absolute top-20 left-10 w-72 h-72 bg-azulejo-blue-200/20 dark:bg-azulejo-blue-500/10 rounded-full blur-3xl"></div>
                <div className="absolute bottom-20 right-10 w-96 h-96 bg-azulejo-gold-200/20 dark:bg-azulejo-gold-500/10 rounded-full blur-3xl"></div>
            </div>

            <div className="container mx-auto px-4 sm:px-6 lg:px-8 relative z-10">
                {/* Header */}
                <div className={`text-center mb-16 sm:mb-20 ${getScrollAnimationClasses(isVisible, 'slide-up')}`}>
                    <div className="inline-block mb-6">
                        <span className="inline-flex items-center gap-2 px-5 py-2.5 rounded-full bg-gradient-to-r from-azulejo-blue-100 to-azulejo-blue-50 dark:from-azulejo-blue-900/30 dark:to-azulejo-blue-800/20 border border-azulejo-blue-200 dark:border-azulejo-blue-700/50 text-azulejo-blue-700 dark:text-azulejo-blue-300 text-sm font-semibold shadow-sm">
                            <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 10l4.553-2.276A1 1 0 0121 8.618v6.764a1 1 0 01-1.447.894L15 14M5 18h8a2 2 0 002-2V8a2 2 0 00-2-2H5a2 2 0 00-2 2v8a2 2 0 002 2z" />
                            </svg>
                            {t({ pt: 'Demonstração Interativa', en: 'Interactive Demo' })}
                        </span>
                    </div>

                    <h2
                        id="ar-demo-heading"
                        className="text-4xl sm:text-5xl lg:text-6xl font-bold mb-6 bg-gradient-to-r from-azulejo-blue-600 via-azulejo-cobalt-600 to-azulejo-blue-600 bg-clip-text text-transparent dark:from-azulejo-blue-400 dark:via-azulejo-cobalt-400 dark:to-azulejo-blue-400"
                    >
                        {t({
                            pt: 'Veja a RA em Ação',
                            en: 'See AR in Action',
                        })}
                    </h2>

                    <p className={`text-lg sm:text-xl text-gray-600 dark:text-gray-300 max-w-3xl mx-auto leading-relaxed ${getScrollAnimationClasses(isVisible, 'slide-up')}`}
                        style={{ transitionDelay: '100ms' }}>
                        {t({
                            pt: 'Descubra como a realidade aumentada transforma cada visita numa experiência inesquecível',
                            en: 'Discover how augmented reality transforms every visit into an unforgettable experience',
                        })}
                    </p>
                </div>

                {/* AR Demo Grid */}
                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6 lg:gap-8 max-w-7xl mx-auto mb-16">
                    {arDemoData.map((demo, index) => (
                        <div
                            key={demo.id}
                            onClick={() => setSelectedDemo(demo)}
                            className={`
                                group relative overflow-hidden rounded-2xl
                                bg-white dark:bg-gray-800
                                shadow-lg hover:shadow-2xl
                                border border-gray-200 dark:border-gray-700
                                cursor-pointer
                                transform hover:-translate-y-2
                                transition-all duration-500
                                ${getScrollAnimationClasses(isVisible, 'scale')}
                            `}
                            style={{ transitionDelay: `${200 + index * 100}ms` }}
                            role="button"
                            tabIndex={0}
                            onKeyDown={(e) => {
                                if (e.key === 'Enter' || e.key === ' ') {
                                    e.preventDefault();
                                    setSelectedDemo(demo);
                                }
                            }}
                            aria-label={t(demo.caption)}
                        >
                            {/* Glow effect on hover */}
                            <div className="absolute -inset-0.5 bg-gradient-to-r from-azulejo-blue-500 to-azulejo-gold-500 rounded-2xl opacity-0 group-hover:opacity-20 blur-xl transition-opacity duration-500"></div>

                            {/* Image Container */}
                            <div className="relative aspect-[4/3] overflow-hidden bg-gradient-to-br from-azulejo-blue-50 to-azulejo-cobalt-50 dark:from-gray-700 dark:to-gray-600">
                                {/* Background icon */}
                                <div className="absolute inset-0 flex items-center justify-center">
                                    <div className="text-8xl opacity-10 group-hover:opacity-20 group-hover:scale-110 transition-all duration-500">
                                        {demoIcons[index]}
                                    </div>
                                </div>

                                <img
                                    src={demo.imageSrc}
                                    alt={t(demo.imageAlt)}
                                    loading="lazy"
                                    className="relative z-10 w-full h-full object-cover opacity-40 group-hover:opacity-60 group-hover:scale-110 transition-all duration-500"
                                />

                                {/* Coming Soon Badge */}
                                <div className="absolute top-3 right-3 z-20">
                                    <ComingSoonBadge
                                        variant="inline"
                                        launchText={t({ pt: 'Em Breve', en: 'Coming Soon' })}
                                        className="text-xs backdrop-blur-md"
                                    />
                                </div>

                                {/* Hover Overlay */}
                                <div className="absolute inset-0 bg-gradient-to-t from-azulejo-blue-900/80 via-azulejo-blue-900/40 to-transparent opacity-0 group-hover:opacity-100 transition-opacity duration-500 flex items-center justify-center z-10">
                                    <div className="transform scale-0 group-hover:scale-100 transition-transform duration-500 bg-white/90 dark:bg-gray-800/90 backdrop-blur-sm rounded-full p-4 shadow-xl">
                                        <svg
                                            className="w-8 h-8 text-azulejo-blue-600 dark:text-azulejo-blue-400"
                                            fill="none"
                                            stroke="currentColor"
                                            viewBox="0 0 24 24"
                                        >
                                            <path
                                                strokeLinecap="round"
                                                strokeLinejoin="round"
                                                strokeWidth={2}
                                                d="M14.752 11.168l-3.197-2.132A1 1 0 0010 9.87v4.263a1 1 0 001.555.832l3.197-2.132a1 1 0 000-1.664z"
                                            />
                                            <path
                                                strokeLinecap="round"
                                                strokeLinejoin="round"
                                                strokeWidth={2}
                                                d="M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
                                            />
                                        </svg>
                                    </div>
                                </div>

                                {/* Number indicator */}
                                <div className="absolute top-3 left-3 z-20 w-8 h-8 rounded-lg bg-white/90 dark:bg-gray-800/90 backdrop-blur-sm flex items-center justify-center text-sm font-bold text-azulejo-blue-600 dark:text-azulejo-blue-400 shadow-md">
                                    {demo.id}
                                </div>
                            </div>

                            {/* Caption */}
                            <div className="relative z-10 p-5 bg-white dark:bg-gray-800">
                                <p className="text-sm lg:text-base text-azulejo-blue-800 dark:text-azulejo-blue-300 text-center font-semibold leading-snug">
                                    {t(demo.caption)}
                                </p>
                            </div>
                        </div>
                    ))}
                </div>

                {/* Watch Full Demo Button */}
                <div className={`text-center ${getScrollAnimationClasses(isVisible, 'slide-up')}`}
                    style={{ transitionDelay: '600ms' }}>
                    <button
                        onClick={() => setShowFullDemoVideo(true)}
                        className="group relative overflow-hidden
                            bg-gradient-to-r from-azulejo-blue-500 via-azulejo-blue-600 to-azulejo-blue-500
                            hover:from-azulejo-blue-600 hover:via-azulejo-blue-700 hover:to-azulejo-blue-600
                            dark:from-azulejo-blue-600 dark:via-azulejo-blue-700 dark:to-azulejo-blue-600
                            dark:hover:from-azulejo-blue-700 dark:hover:via-azulejo-blue-800 dark:hover:to-azulejo-blue-700
                            text-white 
                            px-10 py-5 text-base lg:text-lg font-bold rounded-xl
                            shadow-xl shadow-azulejo-blue-500/30
                            hover:shadow-2xl hover:shadow-azulejo-blue-600/40
                            transition-all duration-500
                            hover:scale-105
                            inline-flex items-center gap-4"
                    >
                        {/* Animated shine effect */}
                        <div className="absolute inset-0 -translate-x-full group-hover:translate-x-full 
                                     transition-transform duration-1000 ease-out
                                     bg-gradient-to-r from-transparent via-white/20 to-transparent" />

                        <div className="relative z-10 flex items-center gap-4">
                            <div className="flex items-center justify-center w-12 h-12 rounded-full bg-white/20 group-hover:bg-white/30 transition-colors duration-300">
                                <svg
                                    className="w-6 h-6 group-hover:scale-110 transition-transform duration-300"
                                    fill="currentColor"
                                    viewBox="0 0 20 20"
                                >
                                    <path d="M6.3 2.841A1.5 1.5 0 004 4.11V15.89a1.5 1.5 0 002.3 1.269l9.344-5.89a1.5 1.5 0 000-2.538L6.3 2.84z" />
                                </svg>
                            </div>
                            <span>
                                {t({
                                    pt: 'Ver Demonstração Completa',
                                    en: 'Watch Full Demo Video',
                                })}
                            </span>
                            <svg className="w-5 h-5 group-hover:translate-x-1 transition-transform" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 8l4 4m0 0l-4 4m4-4H3" />
                            </svg>
                        </div>
                    </button>
                </div>
            </div>

            {/* Demo Detail Modal */}
            {selectedDemo && (
                <>
                    {selectedDemo.videoSrc ? (
                        <VideoModal
                            isOpen={!!selectedDemo}
                            onClose={() => setSelectedDemo(null)}
                            videoUrl={selectedDemo.videoSrc}
                            title={t(selectedDemo.caption)}
                            description={t(selectedDemo.imageAlt)}
                            autoPlay={true}
                            loop={true}
                        />
                    ) : (
                        <Modal
                            isOpen={!!selectedDemo}
                            onClose={() => setSelectedDemo(null)}
                            size="lg"
                        >
                            <Modal.Header>
                                <h2 className="text-2xl font-bold text-azulejo-blue-900 dark:text-white">
                                    {t(selectedDemo.caption)}
                                </h2>
                            </Modal.Header>
                            <Modal.Content>
                                <div className="space-y-4">
                                    <img
                                        src={selectedDemo.imageSrc}
                                        alt={t(selectedDemo.imageAlt)}
                                        className="w-full rounded-lg shadow-md"
                                    />
                                    <div className="prose max-w-none">
                                        <p className="text-gray-700 dark:text-gray-300 leading-relaxed">
                                            {t(selectedDemo.imageAlt)}
                                        </p>
                                        <p className="text-azulejo-blue-600 dark:text-azulejo-blue-400 font-medium mt-4">
                                            {t({
                                                pt: 'Esta funcionalidade estará disponível na aplicação móvel completa. Baixe o aplicativo para experimentar em realidade aumentada!',
                                                en: 'This feature will be available in the full mobile app. Download the app to experience it in augmented reality!',
                                            })}
                                        </p>
                                    </div>
                                </div>
                            </Modal.Content>
                        </Modal>
                    )}
                </>
            )}

            {/* Full Demo Video Modal */}
            <Modal
                isOpen={showFullDemoVideo}
                onClose={() => setShowFullDemoVideo(false)}
            >
                <Modal.Header>
                    <h2 className="text-2xl font-bold text-azulejo-blue-900 dark:text-white">
                        {t({
                            pt: 'Demonstração AR - TileStories',
                            en: 'AR Demo - TileStories',
                        })}
                    </h2>
                </Modal.Header>
                <Modal.Content>
                    <div className="space-y-4">
                        <SequentialVideoPlayer
                            videos={fullDemoVideos}
                            autoPlay={true}
                            showControls={true}
                            showProgress={true}
                        />
                        <p className="text-gray-700 dark:text-gray-300 text-sm text-center">
                            {t({
                                pt: 'Os vídeos são reproduzidos sequencialmente em loop contínuo. Use os controles para navegar entre os vídeos.',
                                en: 'Videos play sequentially in a continuous loop. Use the controls to navigate between videos.',
                            })}
                        </p>
                    </div>
                </Modal.Content>
            </Modal>
        </section>
    );
}
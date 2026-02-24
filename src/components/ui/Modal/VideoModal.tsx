/**
 * VIDEO MODAL COMPONENT
 * =====================
 * Modal specifically for embedding videos (YouTube, Vimeo, local MP4)
 * 
 * Features:
 * - YouTube/Vimeo embed support with autoplay
 * - Local HTML5 video support (.mp4, .webm, .ogg)
 * - Responsive 16:9 aspect ratio
 * - Auto-pause on close
 * - Loading states
 * - Accessible video controls
 * 
 * Usage:
 * // YouTube/Vimeo
 * <VideoModal
 *   isOpen={isOpen}
 *   onClose={() => setIsOpen(false)}
 *   videoUrl="https://www.youtube.com/embed/VIDEO_ID"
 *   title="Watch Demo Video"
 * />
 * 
 * // Local video
 * <VideoModal
 *   isOpen={isOpen}
 *   onClose={() => setIsOpen(false)}
 *   videoUrl="/videos/demo.mp4"
 *   title="Watch Demo Video"
 * />
 */

import { useEffect, useRef } from 'react';
import { Modal } from './Modal';
import { ComingSoonBadge } from '../ComingSoonBadge';
import { useInlineTranslation } from '@/utils/language';

interface VideoModalProps {
    isOpen: boolean;
    onClose: () => void;
    videoUrl: string;
    title?: string;
    description?: string;
    autoPlay?: boolean;
    loop?: boolean;
}

/**
 * Check if URL is a local video file
 */
function isLocalVideo(url: string): boolean {
    return url.endsWith('.mp4') || url.endsWith('.webm') || url.endsWith('.ogg') || url.startsWith('/videos/');
}

/**
 * Extract video ID from YouTube or Vimeo URL and convert to embed format
 */
function getEmbedUrl(url: string): string {
    // YouTube patterns
    if (url.includes('youtube.com/watch')) {
        const videoId = url.split('v=')[1]?.split('&')[0];
        return `https://www.youtube.com/embed/${videoId}?autoplay=1&rel=0`;
    }

    if (url.includes('youtu.be/')) {
        const videoId = url.split('youtu.be/')[1]?.split('?')[0];
        return `https://www.youtube.com/embed/${videoId}?autoplay=1&rel=0`;
    }

    // Vimeo patterns
    if (url.includes('vimeo.com/')) {
        const videoId = url.split('vimeo.com/')[1]?.split('?')[0];
        return `https://player.vimeo.com/video/${videoId}?autoplay=1`;
    }

    // Already an embed URL or direct video
    return url.includes('autoplay') ? url : `${url}?autoplay=1`;
}

export function VideoModal({
    isOpen,
    onClose,
    videoUrl,
    title,
    description,
    autoPlay = true,
    loop = false
}: VideoModalProps) {
    const videoRef = useRef<HTMLVideoElement>(null);
    const t = useInlineTranslation;

    // Check if videoUrl is placeholder/missing
    const isPlaceholder = !videoUrl || videoUrl.includes('placeholder') || videoUrl === '#';
    const isLocal = isLocalVideo(videoUrl);
    const embedUrl = !isLocal ? getEmbedUrl(videoUrl) : videoUrl;

    // Auto-pause video when modal closes
    useEffect(() => {
        if (!isOpen && videoRef.current) {
            videoRef.current.pause();
        }
    }, [isOpen]);

    return (
        <Modal isOpen={isOpen} onClose={onClose} size="xl">
            {title && (
                <Modal.Header>
                    <h2 className="text-2xl font-bold text-azulejo-blue-900 dark:text-white">
                        {title}
                    </h2>
                </Modal.Header>
            )}

            <Modal.Content noPadding>
                {/* Video Container with 16:9 aspect ratio */}
                <div className="relative w-full bg-gradient-to-br from-azulejo-blue-500 to-azulejo-cobalt-500" style={{ paddingTop: '56.25%' }}>
                    {isPlaceholder ? (
                        /* Placeholder with Coming Soon */
                        <div className="absolute top-0 left-0 w-full h-full rounded-lg flex flex-col items-center justify-center p-8">
                            <div className="text-center text-white">
                                <div className="text-6xl mb-6">🎬</div>
                                <h3 className="text-3xl font-bold mb-4">
                                    {t({ pt: 'Vídeo em Breve', en: 'Video Coming Soon' })}
                                </h3>
                                <ComingSoonBadge
                                    variant="inline"
                                    launchText={t({ pt: 'Verão 2026', en: 'Summer 2026' })}
                                />
                                <p className="mt-6 text-azulejo-ivory-200 max-w-md mx-auto">
                                    {t({
                                        pt: 'Estamos a preparar conteúdo exclusivo para demonstrar a experiência AR completa.',
                                        en: 'We\'re preparing exclusive content to demonstrate the full AR experience.'
                                    })}
                                </p>
                            </div>
                        </div>
                    ) : isLocal ? (
                        /* Local HTML5 Video Player */
                        <video
                            ref={videoRef}
                            className="absolute top-0 left-0 w-full h-full rounded-lg object-cover"
                            controls
                            autoPlay={autoPlay}
                            loop={loop}
                            playsInline
                        >
                            <source src={videoUrl} type="video/mp4" />
                            <p className="text-white p-4">
                                {t({
                                    pt: 'Seu navegador não suporta a reprodução de vídeo.',
                                    en: 'Your browser does not support video playback.'
                                })}
                            </p>
                        </video>
                    ) : (
                        /* Embedded video (YouTube/Vimeo) */
                        <iframe
                            className="absolute top-0 left-0 w-full h-full rounded-lg"
                            src={embedUrl}
                            title={title || 'Video player'}
                            allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
                            allowFullScreen
                        />
                    )}
                </div>

                {/* Optional Description */}
                {description && (
                    <div className="p-6">
                        <p className="text-gray-700 dark:text-gray-300 leading-relaxed">{description}</p>
                    </div>
                )}
            </Modal.Content>
        </Modal>
    );
}

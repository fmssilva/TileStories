/**
 * SEQUENTIAL VIDEO PLAYER COMPONENT
 * ==================================
 * 
 * Plays multiple videos sequentially in a continuous loop
 * 
 * Features:
 * - Auto-advances to next video when current ends
 * - Continuous loop (restarts from first video after last)
 * - Progress indicator showing current video
 * - Smooth transitions between videos
 * - Manual navigation controls
 * 
 * Usage:
 * <SequentialVideoPlayer
 *   videos={['/video1.mp4', '/video2.mp4', '/video3.mp4']}
 *   autoPlay={true}
 * />
 */

import { useState, useRef, useEffect } from 'react';
import { Z_INDEX } from '@/design';

interface SequentialVideoPlayerProps {
    videos: string[];
    autoPlay?: boolean;
    className?: string;
    showControls?: boolean;
    showProgress?: boolean;
    /**
     * Z-index for layering
     * @default Z_INDEX.CONTENT (1000)
     */
    zIndex?: number;
}

export function SequentialVideoPlayer({
    videos,
    autoPlay = true,
    className = '',
    showControls = true,
    showProgress = true,
    zIndex = Z_INDEX.CONTENT,
}: SequentialVideoPlayerProps) {
    const [currentIndex, setCurrentIndex] = useState(0);
    const [isPlaying, setIsPlaying] = useState(autoPlay);
    const videoRef = useRef<HTMLVideoElement>(null);

    // Handle video end - move to next video
    const handleVideoEnd = () => {
        const nextIndex = (currentIndex + 1) % videos.length;
        setCurrentIndex(nextIndex);
    };

    // Manual controls
    const playPause = () => {
        if (videoRef.current) {
            if (isPlaying) {
                videoRef.current.pause();
            } else {
                videoRef.current.play();
            }
            setIsPlaying(!isPlaying);
        }
    };

    const nextVideo = () => {
        setCurrentIndex((prevIndex) => (prevIndex + 1) % videos.length);
    };

    const previousVideo = () => {
        setCurrentIndex((prevIndex) => (prevIndex - 1 + videos.length) % videos.length);
    };

    const goToVideo = (index: number) => {
        setCurrentIndex(index);
    };

    // Auto-play when video changes
    useEffect(() => {
        if (videoRef.current && autoPlay) {
            videoRef.current.play().catch((error) => {
                console.warn('Auto-play prevented:', error);
                setIsPlaying(false);
            });
        }
    }, [currentIndex, autoPlay]);

    if (videos.length === 0) {
        return (
            <div className="w-full h-full bg-gray-900 flex items-center justify-center text-white">
                <p>No videos available</p>
            </div>
        );
    }

    return (
        <div className={`relative w-full h-full bg-black ${className}`}>
            {/* Video Player */}
            <video
                ref={videoRef}
                className="w-full h-full object-contain"
                src={videos[currentIndex]}
                onEnded={handleVideoEnd}
                autoPlay={autoPlay}
                playsInline
                controls={showControls}
            >
                <source src={videos[currentIndex]} type="video/mp4" />
                Your browser does not support the video tag.
            </video>

            {/* Progress Indicator */}
            {showProgress && videos.length > 1 && (
                <div
                    className="absolute bottom-4 left-1/2 -translate-x-1/2"
                    style={{ zIndex: zIndex + 10 }}
                >
                    <div className="bg-black/75 backdrop-blur-sm rounded-full px-6 py-3 flex items-center gap-3">
                        {/* Previous Button */}
                        <button
                            onClick={previousVideo}
                            className="text-white hover:text-azulejo-gold-400 transition-colors"
                            aria-label="Previous video"
                        >
                            <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
                                <path d="M8.445 14.832A1 1 0 0010 14v-2.798l5.445 3.63A1 1 0 0017 14V6a1 1 0 00-1.555-.832L10 8.798V6a1 1 0 00-1.555-.832l-6 4a1 1 0 000 1.664l6 4z" />
                            </svg>
                        </button>

                        {/* Progress Dots */}
                        <div className="flex gap-2">
                            {videos.map((_, index) => (
                                <button
                                    key={index}
                                    onClick={() => goToVideo(index)}
                                    className={`transition-all duration-300 rounded-full
                                             ${index === currentIndex
                                            ? 'bg-azulejo-gold-500 w-8 h-2.5'
                                            : 'bg-white/40 hover:bg-white/60 w-2.5 h-2.5'
                                        }`}
                                    aria-label={`Go to video ${index + 1}`}
                                />
                            ))}
                        </div>

                        {/* Next Button */}
                        <button
                            onClick={nextVideo}
                            className="text-white hover:text-azulejo-gold-400 transition-colors"
                            aria-label="Next video"
                        >
                            <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
                                <path d="M4.555 5.168A1 1 0 003 6v8a1 1 0 001.555.832L10 11.202V14a1 1 0 001.555.832l6-4a1 1 0 000-1.664l-6-4A1 1 0 0010 6v2.798l-5.445-3.63z" />
                            </svg>
                        </button>

                        {/* Video Counter */}
                        <span className="text-white text-sm font-semibold ml-2">
                            {currentIndex + 1} / {videos.length}
                        </span>
                    </div>
                </div>
            )}

            {/* Play/Pause Overlay (only when custom controls are shown) */}
            {!showControls && (
                <button
                    onClick={playPause}
                    className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 
                             bg-black/50 hover:bg-black/75 text-white rounded-full p-4
                             transition-all duration-200 hover:scale-110"
                    aria-label={isPlaying ? 'Pause' : 'Play'}
                >
                    {isPlaying ? (
                        <svg className="w-8 h-8" fill="currentColor" viewBox="0 0 20 20">
                            <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zM7 8a1 1 0 012 0v4a1 1 0 11-2 0V8zm5-1a1 1 0 00-1 1v4a1 1 0 102 0V8a1 1 0 00-1-1z" clipRule="evenodd" />
                        </svg>
                    ) : (
                        <svg className="w-8 h-8" fill="currentColor" viewBox="0 0 20 20">
                            <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM9.555 7.168A1 1 0 008 8v4a1 1 0 001.555.832l3-2a1 1 0 000-1.664l-3-2z" clipRule="evenodd" />
                        </svg>
                    )}
                </button>
            )}
        </div>
    );
}

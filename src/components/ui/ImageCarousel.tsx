/**
 * IMAGE CAROUSEL COMPONENT
 * ========================
 * 
 * Auto-playing image carousel with smooth fade transitions
 * 
 * Features:
 * - Automatic slide transitions
 * - Faster first transition to show users it's auto-advancing
 * - Intersection Observer: starts autoplay only when visible
 * - Smooth crossfade between images
 * - Navigation dots
 * - Manual pause/play button (hover does NOT pause)
 * - Manual navigation arrows (on hover)
 * - Responsive design
 * 
 * Usage:
 * <ImageCarousel
 *   images={imageUrls}
 *   interval={5000}
 *   alt="Description"
 * />
 */

import { useState, useEffect, useCallback, useRef } from 'react';
import { Z_INDEX } from '@/design';

interface ImageCarouselProps {
    images: string[];
    interval?: number; // milliseconds between transitions
    firstInterval?: number; // milliseconds before first transition (defaults to interval/2)
    alt?: string;
    className?: string;
    showDots?: boolean;
    /**
     * Z-index for layering
     * @default Z_INDEX.CONTENT (1000)
     */
    zIndex?: number;
}

export function ImageCarousel({
    images,
    interval = 4000,
    firstInterval,
    alt = 'Carousel image',
    className = '',
    showDots = true,
    zIndex = Z_INDEX.CONTENT,
}: ImageCarouselProps) {
    const [currentIndex, setCurrentIndex] = useState(0);
    const [isPaused, setIsPaused] = useState(false);
    const [isFirstTransition, setIsFirstTransition] = useState(true);
    const [hasStarted, setHasStarted] = useState(false);
    const containerRef = useRef<HTMLDivElement>(null);

    // Use faster interval for first transition if not specified
    const effectiveFirstInterval = firstInterval ?? Math.floor(interval / 2);

    // Auto-advance to next image
    const nextImage = useCallback(() => {
        setCurrentIndex((prevIndex) => (prevIndex + 1) % images.length);
        setIsFirstTransition(false);
    }, [images.length]);

    // Previous image (for manual navigation)
    const previousImage = useCallback(() => {
        setCurrentIndex((prevIndex) => (prevIndex - 1 + images.length) % images.length);
        setIsFirstTransition(false);
    }, [images.length]);

    // Go to specific image
    const goToImage = useCallback((index: number) => {
        setCurrentIndex(index);
        setIsFirstTransition(false);
    }, []);

    // Toggle pause state
    const togglePause = useCallback(() => {
        setIsPaused((prev) => !prev);
    }, []);

    // Intersection Observer to detect when carousel becomes visible
    useEffect(() => {
        if (!containerRef.current) return;

        const observer = new IntersectionObserver(
            (entries) => {
                entries.forEach((entry) => {
                    if (entry.isIntersecting && !hasStarted) {
                        // Carousel just became visible for the first time
                        setHasStarted(true);
                        setIsFirstTransition(true);
                    }
                });
            },
            { threshold: 0.1 }
        );

        observer.observe(containerRef.current);

        return () => observer.disconnect();
    }, [hasStarted]);

    // Auto-play effect with faster first transition
    useEffect(() => {
        if (isPaused || images.length <= 1 || !hasStarted) return;

        const delay = isFirstTransition ? effectiveFirstInterval : interval;
        const timer = setInterval(nextImage, delay);

        return () => clearInterval(timer);
    }, [currentIndex, isPaused, interval, effectiveFirstInterval, isFirstTransition, nextImage, images.length, hasStarted]);

    if (images.length === 0) {
        return <div className="w-full h-full bg-gray-200 dark:bg-gray-700 flex items-center justify-center">
            <p className="text-gray-500 dark:text-gray-400">No images available</p>
        </div>;
    }

    return (
        <div
            ref={containerRef}
            className={`relative w-full h-full overflow-hidden group ${className}`}
        >
            {/* Images with crossfade */}
            <div className="relative w-full h-full">
                {images.map((image, index) => (
                    <div
                        key={index}
                        className={`absolute inset-0 transition-opacity duration-1000 ${index === currentIndex ? 'opacity-100' : 'opacity-0'
                            }`}
                    >
                        <img
                            src={image}
                            alt={`${alt} ${index + 1}`}
                            className="w-full h-full object-cover"
                            loading={index === 0 ? 'eager' : 'lazy'}
                        />
                    </div>
                ))}
            </div>

            {/* Navigation Arrows - visible on hover */}
            {images.length > 1 && (
                <>
                    <button
                        onClick={previousImage}
                        className="absolute left-4 top-1/2 -translate-y-1/2 
                                 bg-black/50 hover:bg-black/75 text-white 
                                 p-3 rounded-full opacity-0 group-hover:opacity-100 
                                 transition-opacity duration-300
                                 focus:outline-none focus:ring-2 focus:ring-white"
                        style={{ zIndex: zIndex + 10 }}
                        aria-label="Previous image"
                    >
                        <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
                        </svg>
                    </button>

                    <button
                        onClick={nextImage}
                        className="absolute right-4 top-1/2 -translate-y-1/2 
                                 bg-black/50 hover:bg-black/75 text-white 
                                 p-3 rounded-full opacity-0 group-hover:opacity-100 
                                 transition-opacity duration-300
                                 focus:outline-none focus:ring-2 focus:ring-white"
                        style={{ zIndex: zIndex + 10 }}
                        aria-label="Next image"
                    >
                        <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
                        </svg>
                    </button>
                </>
            )}

            {/* Pause/Play Button - visible on hover, positioned at top-right */}
            {images.length > 1 && (
                <button
                    onClick={togglePause}
                    className="absolute top-4 right-4 
                             bg-black/50 hover:bg-black/75 text-white 
                             p-2.5 rounded-full opacity-0 group-hover:opacity-100 
                             transition-opacity duration-300
                             focus:outline-none focus:ring-2 focus:ring-white"
                    style={{ zIndex: zIndex + 10 }}
                    aria-label={isPaused ? 'Play carousel' : 'Pause carousel'}
                >
                    {isPaused ? (
                        // Play icon
                        <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
                            <path d="M6.3 2.841A1.5 1.5 0 004 4.11V15.89a1.5 1.5 0 002.3 1.269l9.344-5.89a1.5 1.5 0 000-2.538L6.3 2.84z" />
                        </svg>
                    ) : (
                        // Pause icon
                        <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
                            <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zM7 8a1 1 0 012 0v4a1 1 0 11-2 0V8zm5-1a1 1 0 00-1 1v4a1 1 0 102 0V8a1 1 0 00-1-1z" clipRule="evenodd" />
                        </svg>
                    )}
                </button>
            )}

            {/* Navigation Dots */}
            {showDots && images.length > 1 && (
                <div
                    className="absolute bottom-4 left-1/2 -translate-x-1/2 flex gap-2"
                    style={{ zIndex: zIndex + 10 }}
                >
                    {images.map((_, index) => (
                        <button
                            key={index}
                            onClick={() => goToImage(index)}
                            className={`w-2.5 h-2.5 rounded-full transition-all duration-300 
                                     ${index === currentIndex
                                    ? 'bg-white w-8'
                                    : 'bg-white/50 hover:bg-white/75'}`}
                            aria-label={`Go to image ${index + 1}`}
                        />
                    ))}
                </div>
            )}
        </div>
    );
}

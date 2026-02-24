/**
 * BACK TO TOP COMPONENT
 * =====================
 * 
 * Smart scroll-to-top button with progress indicator.
 * Appears after scrolling down, smooth scroll animation.
 */

import { useState, useEffect, useCallback } from 'react';
import { useTheme } from '@/design/theme';
import { Z_INDEX } from '@/design';
import { useInlineTranslation } from '@/utils/language';
import { globalColors } from '@/design/colors';
import type { BackToTopProps } from '../types';

/**
 * Back To Top Button
 * 
 * Shows scroll progress and smooth scrolls to top.
 * 
 * @example
 * <BackToTop showProgress showAfter={300} />
 */
export function BackToTop({
    scrollDuration = 800,
    showProgress = true,
    offset = 16,
    showAfter = 200,
}: BackToTopProps = {}) {
    const { theme } = useTheme();
    const [scrollProgress, setScrollProgress] = useState(0);
    const [isVisible, setIsVisible] = useState(false);
    const [isScrolling, setIsScrolling] = useState(false);

    const scrollToTopLabel = useInlineTranslation('Voltar ao topo', 'Scroll to top');

    // Update scroll progress and visibility
    const updateScrollState = useCallback(() => {
        const scrollY = window.scrollY;
        const documentHeight = document.documentElement.scrollHeight - window.innerHeight;
        const progress = documentHeight > 0 ? (scrollY / documentHeight) * 100 : 0;

        setScrollProgress(progress);
        setIsVisible(scrollY > showAfter);
    }, [showAfter]);

    // Smooth scroll to top
    const scrollToTop = useCallback(() => {
        const startY = window.scrollY;
        const startTime = performance.now();

        setIsScrolling(true);

        const animateScroll = (currentTime: number) => {
            const elapsed = currentTime - startTime;
            const progress = Math.min(elapsed / scrollDuration, 1);

            // Easing function
            const easeInOutCubic = (t: number): number =>
                t < 0.5 ? 4 * t * t * t : (t - 1) * (2 * t - 2) * (2 * t - 2) + 1;

            const easedProgress = easeInOutCubic(progress);
            const currentY = startY * (1 - easedProgress);

            window.scrollTo(0, currentY);

            if (progress < 1) {
                requestAnimationFrame(animateScroll);
            } else {
                setIsScrolling(false);
            }
        };

        requestAnimationFrame(animateScroll);
    }, [scrollDuration]);

    // Handle keyboard
    const handleKeyDown = useCallback((e: React.KeyboardEvent) => {
        if (e.key === 'Enter' || e.key === ' ') {
            e.preventDefault();
            scrollToTop();
        }
    }, [scrollToTop]);

    // Set up scroll listener
    useEffect(() => {
        let rafId: number;

        const handleScroll = () => {
            rafId = requestAnimationFrame(updateScrollState);
        };

        window.addEventListener('scroll', handleScroll, { passive: true });
        updateScrollState();

        return () => {
            window.removeEventListener('scroll', handleScroll);
            if (rafId) cancelAnimationFrame(rafId);
        };
    }, [updateScrollState]);

    // Don't render if not visible
    if (!isVisible) return null;

    // Theme colors
    const colors = {
        background: theme === 'light' ? '#ffffff' : '#1e293b',
        border: theme === 'light' ? '#e2e8f0' : '#475569',
        icon: theme === 'light' ? globalColors.primary[600] : globalColors.primary[400],
        progressRing: globalColors.primary[500],
    };

    return (
        <button
            onClick={scrollToTop}
            onKeyDown={handleKeyDown}
            disabled={isScrolling}
            className="fixed group transition-all duration-300 hover:scale-110 active:scale-95 focus:outline-none focus:ring-2 focus:ring-primary focus:ring-offset-2 disabled:opacity-50 rounded-full border shadow-lg backdrop-blur-sm"
            style={{
                zIndex: Z_INDEX.STICKY,
                top: `${offset}px`,
                right: `${offset}px`,
                backgroundColor: colors.background,
                borderColor: colors.border,
            }}
            aria-label={scrollToTopLabel}
            title={scrollToTopLabel}
        >
            <div className="relative flex items-center justify-center w-12 h-12">
                {/* Progress Ring */}
                {showProgress && (
                    <svg
                        className="absolute inset-0 w-12 h-12 -rotate-90"
                        viewBox="0 0 48 48"
                    >
                        <circle
                            cx="24"
                            cy="24"
                            r="20"
                            fill="none"
                            stroke={colors.border}
                            strokeWidth="2"
                            opacity="0.3"
                        />
                        <circle
                            cx="24"
                            cy="24"
                            r="20"
                            fill="none"
                            stroke={colors.progressRing}
                            strokeWidth="2"
                            strokeLinecap="round"
                            strokeDasharray={`${2 * Math.PI * 20}`}
                            strokeDashoffset={`${2 * Math.PI * 20 * (1 - scrollProgress / 100)}`}
                            style={{ transition: 'stroke-dashoffset 0.1s linear' }}
                        />
                    </svg>
                )}

                {/* Arrow Icon */}
                <svg
                    className="w-5 h-5 transition-transform duration-200 group-hover:-translate-y-0.5"
                    style={{ color: colors.icon }}
                    fill="none"
                    stroke="currentColor"
                    viewBox="0 0 24 24"
                    strokeWidth="2.5"
                >
                    <path strokeLinecap="round" strokeLinejoin="round" d="M5 10l7-7m0 0l7 7m-7-7v18" />
                </svg>
            </div>
        </button>
    );
}

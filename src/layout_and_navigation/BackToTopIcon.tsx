// ============================================================================
// BACK TO TOP COMPONENT - SIMPLE BEHIND-HEADER APPROACH
// ============================================================================

/**
 * Smart back-to-top button with clean UX:
 * 
 * ✅ Fixed position behind header (lower z-index)
 * ✅ Automatically revealed when header slides up
 * ✅ No complex visibility logic - pure CSS layering
 * ✅ Smooth scroll animation with progress indicator
 * ✅ Theme-adaptive styling
 * ✅ Keyboard accessible
 */

import { useState, useEffect, useCallback } from 'react';
import { useTheme } from '@/domains/theme';
import { useInlineTranslation } from '@/utils/language';
import { headerColors } from './colors';

interface BackToTopProps {
    /** Smooth scroll duration in ms (default: 800ms) */
    scrollDuration?: number;
    /** Show scroll progress ring (default: true) */
    showProgress?: boolean;
    /** Offset from edge in pixels (default: 24px) */
    offset?: number;
}

export function BackToTop({
    scrollDuration = 800,
    showProgress = true,
    offset = 24
}: BackToTopProps) {
    const { theme } = useTheme();
    const [scrollProgress, setScrollProgress] = useState(0);
    const [isScrolling, setIsScrolling] = useState(false);

    // Inline translations
    const scrollToTopLabel = useInlineTranslation('Voltar ao topo', 'Scroll to top');
    const backToTopLabel = useInlineTranslation('Voltar ao topo', 'Back to top');

    // Calculate scroll progress for the progress ring
    const updateScrollState = useCallback(() => {
        const scrollY = window.scrollY;
        const documentHeight = document.documentElement.scrollHeight - window.innerHeight;
        const progress = documentHeight > 0 ? (scrollY / documentHeight) * 100 : 0;

        setScrollProgress(progress);
    }, []);

    // Smooth scroll to top with animation
    const scrollToTop = useCallback(() => {
        const startY = window.scrollY;
        const startTime = performance.now();

        setIsScrolling(true);

        const animateScroll = (currentTime: number) => {
            const elapsed = currentTime - startTime;
            const progress = Math.min(elapsed / scrollDuration, 1);

            // Easing function for smooth animation
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

    // Handle keyboard access
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
        updateScrollState(); // Initial call

        return () => {
            window.removeEventListener('scroll', handleScroll);
            if (rafId) cancelAnimationFrame(rafId);
        };
    }, [updateScrollState]);

    // Theme-based colors using teal header colors
    const brandColors = headerColors.primary; // Get brand colors from header
    const colors = {
        background: theme === 'light'
            ? `${brandColors[50]}f2`  // Very light teal with opacity
            : `${brandColors[900]}f2`, // Very dark teal with opacity
        border: theme === 'light'
            ? brandColors[200]
            : brandColors[700],
        icon: theme === 'light'
            ? brandColors[600]
            : brandColors[400],
        shadow: theme === 'light'
            ? '0 10px 25px rgba(20, 184, 166, 0.15), 0 4px 6px rgba(20, 184, 166, 0.1)' // Teal shadow
            : '0 10px 25px rgba(0, 0, 0, 0.4), 0 4px 6px rgba(0, 0, 0, 0.2)',
        progressRing: theme === 'light'
            ? brandColors[500]
            : brandColors[400],
    };

    return (
        <div
            className="fixed group"
            style={{
                top: `${offset}px`,
                right: `${offset}px`,
                zIndex: 40, // Lower than header (z-50) so header covers it
            }}
        >
            {/* Main Button */}
            <button
                onClick={scrollToTop}
                onKeyDown={handleKeyDown}
                disabled={isScrolling}
                className="relative flex items-center justify-center w-12 h-12 rounded-full backdrop-blur-md border transition-all duration-300 hover:scale-110 active:scale-95 focus:outline-none focus:ring-2 focus:ring-offset-2 disabled:opacity-50"
                style={{
                    backgroundColor: colors.background,
                    borderColor: colors.border,
                    boxShadow: colors.shadow,
                }}
                aria-label={scrollToTopLabel}
                title={backToTopLabel}
            >
                {/* Progress Ring (if enabled) */}
                {showProgress && (
                    <svg
                        className="absolute inset-0 w-12 h-12 -rotate-90"
                        viewBox="0 0 48 48"
                    >
                        {/* Background circle */}
                        <circle
                            cx="24"
                            cy="24"
                            r="20"
                            fill="none"
                            stroke={colors.border}
                            strokeWidth="2"
                            opacity="0.3"
                        />
                        {/* Progress circle */}
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
                            style={{
                                transition: 'stroke-dashoffset 0.1s linear',
                                filter: 'drop-shadow(0 0 2px rgba(59, 130, 246, 0.3))'
                            }}
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

                {/* Loading indicator when scrolling */}
                {isScrolling && (
                    <div className="absolute inset-0 flex items-center justify-center">
                        <div
                            className="w-3 h-3 rounded-full animate-spin"
                            style={{
                                border: `2px solid ${colors.border}`,
                                borderTopColor: colors.icon,
                            }}
                        />
                    </div>
                )}
            </button>

            {/* Tooltip on hover (desktop only) */}
            <div className="hidden md:block absolute bottom-full right-0 mb-2 px-3 py-1 text-sm font-medium rounded-md opacity-0 group-hover:opacity-100 transition-opacity duration-200 pointer-events-none whitespace-nowrap"
                style={{
                    backgroundColor: colors.background,
                    borderColor: colors.border,
                    color: colors.icon,
                    boxShadow: colors.shadow,
                }}
            >
                {backToTopLabel}
                <div className="absolute top-full right-4 w-0 h-0 border-l-4 border-r-4 border-t-4 border-transparent"
                    style={{ borderTopColor: colors.background }}
                />
            </div>
        </div>
    );
}
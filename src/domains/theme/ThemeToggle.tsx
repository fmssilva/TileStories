/**
 * THEME TOGGLE COMPONENT
 * ======================
 * 
 * A clean, accessible button component for switching between themes.
 * Provides visual feedback and proper accessibility features.
 */

import { useTheme } from './useTheme';
import { THEME_ICONS, useThemeLabels } from './types';
import { useLanguage } from '@/utils/language';

type ThemeToggleSize = 'sm' | 'md' | 'lg';

interface ThemeToggleProps {
    /**
     * Size of the toggle button
     */
    size?: ThemeToggleSize;

    /**
     * Additional CSS classes
     */
    className?: string;

    /**
     * Show theme label text
     */
    showLabel?: boolean;
}

export function ThemeToggle({
    size = 'md',
    className = '',
    showLabel = false
}: ThemeToggleProps) {
    const { mode, toggleTheme } = useTheme();
    const themeLabels = useThemeLabels();
    const { language } = useLanguage();

    // Inline translations with interpolation
    const toggleTooltip = language === 'pt'
        ? `Alterar tema (atual: ${themeLabels[mode]})`
        : `Switch theme (current: ${themeLabels[mode]})`;

    const toggleAriaLabel = language === 'pt'
        ? `Alterar tema. Tema atual: ${themeLabels[mode]}`
        : `Switch theme. Current theme: ${themeLabels[mode]}`;    // Get the appropriate icon for current theme
    const currentIcon = THEME_ICONS[mode];
    const currentLabel = themeLabels[mode];

    // Size mappings for the button
    const sizeClasses = {
        sm: 'w-6 h-6 text-sm',
        md: 'w-8 h-8 text-base',
        lg: 'w-10 h-10 text-lg',
    };

    const buttonSizeClass = sizeClasses[size];

    return (
        <button
            onClick={toggleTheme}
            className={`
        inline-flex items-center justify-center gap-2
        rounded-md
        bg-background hover:bg-muted
        border border-border
        text-foreground hover:text-primary
        transition-all duration-200
        focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring
        ${buttonSizeClass}
        p-2
        ${className}
      `}
            title={toggleTooltip}
            aria-label={toggleAriaLabel}
        >
            {/* Theme icon */}
            <span className="text-current" role="img" aria-hidden="true">
                {currentIcon}
            </span>

            {/* Optional label */}
            {showLabel && (
                <span className="hidden sm:inline font-medium">
                    {currentLabel}
                </span>
            )}
        </button>
    );
}

/**
 * Simplified theme toggle with just icon
 */
export function ThemeToggleIcon({ size = 'md', className = '' }: Omit<ThemeToggleProps, 'showLabel'>) {
    return <ThemeToggle size={size} className={className} showLabel={false} />;
}

/**
 * Theme toggle with label
 */
export function ThemeToggleWithLabel({ size = 'md', className = '' }: Omit<ThemeToggleProps, 'showLabel'>) {
    return <ThemeToggle size={size} className={className} showLabel={true} />;
}
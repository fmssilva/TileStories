/**
 * THEME TOGGLE COMPONENT
 * ======================
 * 
 * A clean, accessible button for toggling between light and dark themes.
 * Simplified: just toggles between sun (light) and moon (dark).
 */

import { useTheme } from './useTheme';
import { THEME_ICONS, useThemeLabels } from './types';
import { useLanguage } from '@/utils/language';
import { LAYOUT } from '@/design';

type ThemeToggleSize = 'sm' | 'md' | 'lg';

interface ThemeToggleProps {
    /**
     * Size of the toggle button (deprecated, use iconSize instead)
     */
    size?: ThemeToggleSize;

    /**
     * Icon button size in pixels (overrides size prop)
     */
    iconSize?: number;

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
    iconSize,
    className = '',
    showLabel = false
}: ThemeToggleProps) {
    const { mode, toggleTheme } = useTheme();
    const themeLabels = useThemeLabels();
    const { language } = useLanguage();

    // Next theme that will be switched to
    const nextMode = mode === 'light' ? 'dark' : 'light';
    const nextLabel = themeLabels[nextMode];

    // Inline translations with interpolation
    const toggleTooltip = language === 'pt'
        ? `Alterar para ${nextLabel}`
        : `Switch to ${nextLabel}`;

    const toggleAriaLabel = language === 'pt'
        ? `Alterar tema. Atual: ${themeLabels[mode]}`
        : `Switch theme. Current: ${themeLabels[mode]}`;

    // Get the appropriate icon for current theme
    const currentIcon = THEME_ICONS[mode];

    // Size mappings for the button (used when iconSize not provided)
    const sizeClasses = {
        sm: 'w-8 h-8 text-lg',
        md: 'w-10 h-10 text-xl',
        lg: 'w-12 h-12 text-2xl',
    };

    const buttonSizeClass = sizeClasses[size];

    // If iconSize is provided, use inline styles with FIXED icon size from LAYOUT
    const buttonStyle = iconSize ? {
        width: `${iconSize}px`,
        height: `${iconSize}px`,
        fontSize: `${LAYOUT.ICON_SIZE}px`, // Fixed icon size (24px), not percentage!
    } : undefined;

    return (
        <button
            onClick={toggleTheme}
            className={`
                inline-flex items-center justify-center gap-2
                rounded-lg
                bg-transparent hover:bg-white/10
                border-none
                text-white/90 hover:text-white
                transition-all duration-200 hover:brightness-110
                focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-white/30
                ${iconSize ? '' : buttonSizeClass}
                ${className}
            `}
            style={buttonStyle}
            title={toggleTooltip}
            aria-label={toggleAriaLabel}
        >
            {/* Theme icon */}
            <span className="text-current transition-transform duration-200" role="img" aria-hidden="true">
                {currentIcon}
            </span>

            {/* Optional label */}
            {showLabel && (
                <span className="hidden sm:inline font-medium text-sm">
                    {themeLabels[mode]}
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
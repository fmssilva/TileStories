/**
 * ICONS GROUP COMPONENT
 * =====================
 * 
 * Manages all header icon buttons (LanguageSelector, ThemeToggle, and optional actions).
 * This component encapsulates the right-side icons section of the header.
 * 
 * RESPONSIBILITIES:
 * - Render LanguageSelector and ThemeToggle in consistent layout
 * - Accept custom actions
 * - Work within exact width constraints passed by Header
 * - Never overflow or shrink
 * 
 * The Header component measures this component's actual width and uses it
 * for layout calculations.
 */

import { forwardRef } from 'react';
import { LanguageSelector } from '@/components/LanguageSelector';
import { ThemeToggleIcon } from '@/design/theme';

export interface IconsGroupProps {
    /** Additional action buttons to include */
    actions?: React.ReactNode;
    /** Exact height for icons (in pixels) - matches componentsHeight from layout */
    componentsHeight: number;
    /** Additional CSS classes */
    className?: string;
}

/**
 * IconsGroup Component
 * 
 * Groups LanguageSelector and ThemeToggle icons together.
 * Uses forwardRef so Header can measure its actual DOM width.
 * Icons match componentsHeight exactly for precise sizing.
 * 
 * @example
 * <IconsGroup ref={iconsRef} componentsHeight={68} />
 */
export const IconsGroup = forwardRef<HTMLDivElement, IconsGroupProps>(
    ({ actions, componentsHeight, className = '' }, ref) => {
        // Icons are exactly componentsHeight (direct measurement, no calculations!)
        const iconSize = componentsHeight;

        return (
            <div
                ref={ref}
                className={`flex items-center gap-2 sm:gap-3 flex-shrink-0 ${className}`}
            >
                {actions}
                <LanguageSelector iconSize={iconSize} />
                <ThemeToggleIcon iconSize={iconSize} />
            </div>
        );
    }
);

IconsGroup.displayName = 'IconsGroup';

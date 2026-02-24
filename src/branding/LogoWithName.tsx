/**
 * LOGO WITH NAME COMPONENT
 * ========================
 * 
 * Brand identity component that combines logo icon with app name.
 * Used in header for consistent branding display.
 * 
 * Uses direct logoHeight measurement for simple, predictable sizing.
 */

import { Link } from 'react-router-dom';
import { Logo } from './Logo';
import { LAYOUT } from '@/design';
import SiteName from './SiteName';

export interface LogoWithNameProps {
    /** Exact height for the logo icon (in pixels) */
    logoHeight: number;
    /** Exact height for the logo name text (in pixels) - DEPRECATED, now uses fixed LAYOUT.LOGO_NAME_FONT_SIZE */
    logoNameHeight?: number;
    /** Additional CSS classes */
    className?: string;
    /** Custom logo element (optional override) */
    logoElement?: React.ReactNode;
}

/**
 * LogoWithName Component
 * 
 * Renders logo icon + "TileStories" text with exact sizing.
 * Now uses fixed font size from LAYOUT constants.
 * 
 * @example
 * <LogoWithName logoHeight={58} />
 */
export function LogoWithName({
    logoHeight,
    logoNameHeight, // DEPRECATED - kept for backward compatibility
    className = '',
    logoElement,
}: LogoWithNameProps) {
    // Logo is square - use exact logoHeight
    const logoSize = logoHeight;

    // Use fixed font size from LAYOUT constants (no more calculations!)
    const fontSize = LAYOUT.LOGO_NAME_FONT_SIZE;
    const gap = LAYOUT.LOGO_NAME_GAP;

    // Suppress unused warning for deprecated param
    void logoNameHeight;

    return (
        <div className={`flex items-center flex-shrink-0 ${className}`} style={{ gap: `${gap}px` }}>
            <Link
                to="/"
                className="flex items-center transition-opacity hover:opacity-80"
                style={{ gap: `${gap}px` }}
            >
                {logoElement || (
                    <div
                        className="flex-shrink-0"
                        style={{
                            width: `${logoSize}px`,
                            height: `${logoSize}px`,
                        }}
                    >
                        <Logo variant="icon" size="xl" className="w-full h-full" />
                    </div>
                )}

                <SiteName fontSize={fontSize} />
            </Link>
        </div>
    );
}

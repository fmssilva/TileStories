/**
 * Logo Component - Brand identity system with theme awareness
 * 
 * This component automatically chooses the right logo variant based on:
 * - Current theme (light/dark)
 * - Size requirements (full logo vs icon only)
 * - Usage context (header, footer, etc.)
 * 
 * Located in /branding because it's brand-specific, not a generic UI component
 * 
 * NOTE: Uses PNG files for better quality. SVG only used for favicon.
 */

import { appConfig } from '@/config/app';

// Logo paths - Using PNG for better visual quality (SVG is low quality)
// Co-located in public folder for direct access
const logoIcon = '/assets/Logo.png';
const logoFull = '/assets/Logo_with_name.png';

type LogoSize = 'xs' | 'sm' | 'md' | 'lg' | 'xl';

interface LogoProps {
    variant?: 'full' | 'icon';
    size?: LogoSize;
    theme?: 'light' | 'dark' | 'auto';
    className?: string;
    alt?: string;
}

export function Logo({
    variant = 'full',
    size = 'md',
    className = '',
    alt = appConfig.displayName
}: LogoProps) {
    // Determine which logo to use
    const getLogoSrc = () => {
        // For icon variant, use icon-only PNG
        if (variant === 'icon') {
            return logoIcon;
        }

        // For full variant, use logo with name
        return logoFull;
    };

    const logoSrc = getLogoSrc();

    // Simple size mapping to Tailwind classes
    const sizeClasses = {
        xs: 'w-4 h-4',
        sm: 'w-6 h-6',
        md: 'w-8 h-8',
        lg: 'w-10 h-10',
        xl: 'w-12 h-12'
    };

    const sizeClass = sizeClasses[size];

    return (
        <div className={`flex-shrink-0 ${sizeClass} ${className}`}>
            <img
                src={logoSrc}
                alt={alt}
                className="w-full h-full object-contain"
                draggable={false}
            />
        </div>
    );
}

// Convenience exports for common use cases
export const LogoIcon = (props: Omit<LogoProps, 'variant'>) =>
    <Logo {...props} variant="icon" />;

export const LogoFull = (props: Omit<LogoProps, 'variant'>) =>
    <Logo {...props} variant="full" />;
/**
 * Logo Component - Brand identity system with theme awareness
 * 
 * This component automatically chooses the right logo variant based on:
 * - Current theme (light/dark)
 * - Size requirements (full logo vs icon only)
 * - Usage context (header, footer, etc.)
 * 
 * Located in /branding because it's brand-specific, not a generic UI component
 */

import { appConfig } from '@/config/app';
import { useTheme } from '@/design/theme';

// Logo paths - co-located assets for better cohesion
import logoLight from './assets/logo.svg';
import logoDark from './assets/logo-dark.svg';
import logoIcon from './assets/logo-icon.svg';

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
    theme = 'auto',
    className = '',
    alt = appConfig.displayName
}: LogoProps) {
    const { theme: currentTheme } = useTheme();

    // Determine which logo to use based on theme
    const getLogoSrc = () => {
        // For icon variant, always use the icon version
        if (variant === 'icon') {
            return logoIcon;
        }

        // For full variant, choose based on theme
        const effectiveTheme = theme === 'auto' ? currentTheme : theme;

        if (effectiveTheme === 'dark') {
            return logoDark;
        } else {
            return logoLight;
        }
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
/**
 * BUTTON COMPONENT - Enhanced with asChild support
 * 
 * Modern button component with flexible rendering:
 * - Standard button: <Button onClick={...}>Click me</Button>
 * - Custom element: <Button asChild><Link to="/page">Go</Link></Button>
 * - Clean design following our size and color guides
 */

import { ReactNode, ReactElement, cloneElement, isValidElement } from 'react';
import { getThemeColors, globalColors, type Theme } from '@/design/colors';
import { useTheme } from '@/domains/theme';

interface BaseButtonProps {
    children: ReactNode;
    variant?: 'primary' | 'secondary' | 'outline' | 'ghost' | 'destructive';
    size?: 'sm' | 'md' | 'lg';
    disabled?: boolean;
    className?: string;
}

interface ButtonAsButton extends BaseButtonProps {
    asChild?: false;
    onClick?: () => void;
}

interface ButtonAsChild extends BaseButtonProps {
    asChild: true;
    onClick?: never;
}

type ButtonProps = ButtonAsButton | ButtonAsChild;

export function Button({
    children,
    variant = 'primary',
    size = 'md',
    disabled = false,
    asChild = false,
    className = '',
    ...props
}: ButtonProps) {
    const { theme } = useTheme();

    // Simple size mapping - use Tailwind classes directly
    const sizeClasses = {
        sm: 'h-8 px-3 text-sm',      // 32px height
        md: 'h-9 px-4 text-sm',      // 36px height  
        lg: 'h-10 px-6 text-base',   // 40px height
    };

    const sizeConfig = {
        className: sizeClasses[size],
    };

    // Get button styles based on variant and theme (LOCAL LEVEL)
    const getVariantStyles = (theme: Theme, variant: string) => {
        const themeColors = getThemeColors(theme);

        switch (variant) {
            case 'primary':
                return {
                    backgroundColor: themeColors.primary,
                    color: themeColors.primaryForeground,
                    borderColor: themeColors.primary,
                    hover: {
                        backgroundColor: theme === 'light'
                            ? globalColors.brand[600]  // Darker teal
                            : globalColors.brand[300], // Lighter teal
                    }
                };

            case 'secondary':
                return {
                    backgroundColor: themeColors.surface,
                    color: themeColors.text,
                    borderColor: themeColors.border,
                    hover: {
                        backgroundColor: themeColors.backgroundSoft,
                    }
                };

            case 'outline':
                return {
                    backgroundColor: 'transparent',
                    color: themeColors.text,
                    borderColor: themeColors.border,
                    hover: {
                        backgroundColor: themeColors.surface,
                        borderColor: themeColors.primary,
                    }
                };

            case 'ghost':
                return {
                    backgroundColor: 'transparent',
                    color: themeColors.text,
                    borderColor: 'transparent',
                    hover: {
                        backgroundColor: themeColors.surface,
                    }
                };

            case 'destructive':
                return {
                    backgroundColor: themeColors.error,
                    color: globalColors.pure.white,
                    borderColor: themeColors.error,
                    hover: {
                        backgroundColor: globalColors.semantic.error,
                    }
                };

            default:
                return {
                    backgroundColor: themeColors.primary,
                    color: themeColors.primaryForeground,
                    borderColor: themeColors.primary,
                    hover: { backgroundColor: themeColors.primary }
                };
        }
    };

    const variantStyles = getVariantStyles(theme, variant);

    // Common button classes and styles
    const buttonClasses = `
        inline-flex items-center justify-center
        rounded-md font-medium border
        transition-all duration-200
        focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-offset-2
        disabled:pointer-events-none disabled:opacity-50
        hover:scale-105 active:scale-95
        ${sizeConfig.className}
        ${className}
    `;

    const buttonStyles = {
        backgroundColor: variantStyles.backgroundColor,
        color: variantStyles.color,
        borderColor: variantStyles.borderColor,
    };

    const buttonEvents = {
        onMouseEnter: (e: React.MouseEvent<HTMLElement>) => {
            if (!disabled && variantStyles.hover.backgroundColor) {
                e.currentTarget.style.backgroundColor = variantStyles.hover.backgroundColor;
            }
            if (!disabled && variantStyles.hover.borderColor) {
                e.currentTarget.style.borderColor = variantStyles.hover.borderColor;
            }
        },
        onMouseLeave: (e: React.MouseEvent<HTMLElement>) => {
            if (!disabled) {
                e.currentTarget.style.backgroundColor = variantStyles.backgroundColor;
                e.currentTarget.style.borderColor = variantStyles.borderColor;
            }
        }
    };

    // If asChild is true, clone the child element with button props
    if (asChild && isValidElement(children)) {
        const childElement = children as ReactElement<any>;
        return cloneElement(childElement, {
            className: `${buttonClasses} ${childElement.props.className || ''}`,
            style: { ...buttonStyles, ...(childElement.props.style || {}) },
            disabled,
            ...buttonEvents,
            ...childElement.props, // Child props override button props
        });
    }

    // Standard button implementation
    const onClick = 'onClick' in props ? props.onClick : undefined;

    return (
        <button
            onClick={onClick}
            disabled={disabled}
            className={buttonClasses}
            style={buttonStyles}
            {...buttonEvents}
        >
            {children}
        </button>
    );
}
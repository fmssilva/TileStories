/**
 * ALERT COMPONENT
 * ===============
 * 
 * Inline alert/notification component for contextual feedback
 * 
 * Features:
 * - 4 variants (info, success, warning, error)
 * - Optional icon
 * - Optional close button
 * - Dark mode support
 * - Smooth animations
 * 
 * Usage:
 * ```tsx
 * <Alert variant="success">Your changes have been saved!</Alert>
 * <Alert variant="error" onClose={() => setShowError(false)}>
 *   An error occurred. Please try again.
 * </Alert>
 * ```
 */

import { ReactNode } from 'react';
import { cn } from '@/utils';
import type { FeedbackVariant } from './types';

interface AlertProps {
    /** Alert content */
    children: ReactNode;
    /** Visual variant */
    variant?: FeedbackVariant;
    /** Optional title */
    title?: string;
    /** Show close button */
    onClose?: () => void;
    /** Additional CSS classes */
    className?: string;
}

export function Alert({ children, variant = 'info', title, onClose, className }: AlertProps) {
    const variantStyles = {
        info: {
            container: 'bg-blue-50 dark:bg-blue-900/20 border-blue-200 dark:border-blue-800',
            icon: 'text-blue-600 dark:text-blue-400',
            title: 'text-blue-900 dark:text-blue-100',
            text: 'text-blue-800 dark:text-blue-200',
        },
        success: {
            container: 'bg-green-50 dark:bg-green-900/20 border-green-200 dark:border-green-800',
            icon: 'text-green-600 dark:text-green-400',
            title: 'text-green-900 dark:text-green-100',
            text: 'text-green-800 dark:text-green-200',
        },
        warning: {
            container: 'bg-yellow-50 dark:bg-yellow-900/20 border-yellow-200 dark:border-yellow-800',
            icon: 'text-yellow-600 dark:text-yellow-400',
            title: 'text-yellow-900 dark:text-yellow-100',
            text: 'text-yellow-800 dark:text-yellow-200',
        },
        error: {
            container: 'bg-red-50 dark:bg-red-900/20 border-red-200 dark:border-red-800',
            icon: 'text-red-600 dark:text-red-400',
            title: 'text-red-900 dark:text-red-100',
            text: 'text-red-800 dark:text-red-200',
        },
    };

    const icons = {
        info: (
            <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
                <path
                    fillRule="evenodd"
                    d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z"
                    clipRule="evenodd"
                />
            </svg>
        ),
        success: (
            <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
                <path
                    fillRule="evenodd"
                    d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z"
                    clipRule="evenodd"
                />
            </svg>
        ),
        warning: (
            <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
                <path
                    fillRule="evenodd"
                    d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z"
                    clipRule="evenodd"
                />
            </svg>
        ),
        error: (
            <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
                <path
                    fillRule="evenodd"
                    d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z"
                    clipRule="evenodd"
                />
            </svg>
        ),
    };

    const styles = variantStyles[variant];

    return (
        <div
            className={cn(
                'rounded-lg border p-4',
                'animate-in fade-in duration-200',
                styles.container,
                className
            )}
            role="alert"
        >
            <div className="flex items-start gap-3">
                {/* Icon */}
                <div className={cn('flex-shrink-0 mt-0.5', styles.icon)}>{icons[variant]}</div>

                {/* Content */}
                <div className="flex-1 min-w-0">
                    {title && (
                        <h3 className={cn('text-sm font-semibold mb-1', styles.title)}>{title}</h3>
                    )}
                    <div className={cn('text-sm', styles.text)}>{children}</div>
                </div>

                {/* Close button */}
                {onClose && (
                    <button
                        onClick={onClose}
                        className={cn(
                            'flex-shrink-0 ml-2 p-1 rounded-md',
                            'hover:bg-black/5 dark:hover:bg-white/10',
                            'transition-colors',
                            styles.icon
                        )}
                        aria-label="Close alert"
                    >
                        <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path
                                strokeLinecap="round"
                                strokeLinejoin="round"
                                strokeWidth={2}
                                d="M6 18L18 6M6 6l12 12"
                            />
                        </svg>
                    </button>
                )}
            </div>
        </div>
    );
}

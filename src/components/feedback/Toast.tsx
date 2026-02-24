/**
 * TOAST COMPONENT
 * ===============
 * 
 * Individual toast notification component
 * 
 * Features:
 * - Auto-dismiss with countdown
 * - Slide-in animation
 * - Close button
 * - 4 variants (info, success, warning, error)
 * - Dark mode support
 * 
 * Note: Use ToastProvider and useToast hook instead of using this directly
 */

import { useEffect, useState } from 'react';
import { cn } from '@/utils';
import type { Toast as ToastType } from './types';

interface ToastProps extends ToastType {
    onClose: (id: string) => void;
}

export function Toast({ id, title, message, variant, duration = 5000, onClose }: ToastProps) {
    const [isLeaving, setIsLeaving] = useState(false);

    useEffect(() => {
        const timer = setTimeout(() => {
            setIsLeaving(true);
            setTimeout(() => onClose(id), 200); // Wait for animation
        }, duration);

        return () => clearTimeout(timer);
    }, [id, duration, onClose]);

    const handleClose = () => {
        setIsLeaving(true);
        setTimeout(() => onClose(id), 200);
    };

    const variantStyles = {
        info: {
            container: 'bg-blue-50 dark:bg-blue-900/90 border-blue-200 dark:border-blue-800',
            icon: 'text-blue-600 dark:text-blue-400',
            title: 'text-blue-900 dark:text-blue-100',
            text: 'text-blue-800 dark:text-blue-200',
        },
        success: {
            container: 'bg-green-50 dark:bg-green-900/90 border-green-200 dark:border-green-800',
            icon: 'text-green-600 dark:text-green-400',
            title: 'text-green-900 dark:text-green-100',
            text: 'text-green-800 dark:text-green-200',
        },
        warning: {
            container: 'bg-yellow-50 dark:bg-yellow-900/90 border-yellow-200 dark:border-yellow-800',
            icon: 'text-yellow-600 dark:text-yellow-400',
            title: 'text-yellow-900 dark:text-yellow-100',
            text: 'text-yellow-800 dark:text-yellow-200',
        },
        error: {
            container: 'bg-red-50 dark:bg-red-900/90 border-red-200 dark:border-red-800',
            icon: 'text-red-600 dark:text-red-400',
            title: 'text-red-900 dark:text-red-100',
            text: 'text-red-800 dark:text-red-200',
        },
    };

    const icons = {
        info: '🔵',
        success: '✅',
        warning: '⚠️',
        error: '❌',
    };

    const styles = variantStyles[variant];

    return (
        <div
            className={cn(
                'pointer-events-auto w-full max-w-sm overflow-hidden rounded-lg border shadow-lg',
                styles.container,
                isLeaving ? 'animate-out slide-out-to-right duration-200' : 'animate-in slide-in-from-right duration-300'
            )}
            role="alert"
        >
            <div className="p-4">
                <div className="flex items-start gap-3">
                    {/* Icon */}
                    <span className="text-xl flex-shrink-0" aria-hidden="true">
                        {icons[variant]}
                    </span>

                    {/* Content */}
                    <div className="flex-1 min-w-0">
                        {title && (
                            <p className={cn('text-sm font-semibold mb-1', styles.title)}>
                                {title}
                            </p>
                        )}
                        <p className={cn('text-sm', styles.text)}>{message}</p>
                    </div>

                    {/* Close button */}
                    <button
                        onClick={handleClose}
                        className={cn(
                            'flex-shrink-0 ml-2 p-1 rounded-md',
                            'hover:bg-black/5 dark:hover:bg-white/10',
                            'transition-colors',
                            styles.icon
                        )}
                        aria-label="Close notification"
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
                </div>
            </div>
        </div>
    );
}

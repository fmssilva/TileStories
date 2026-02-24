/**
 * SPINNER COMPONENT
 * =================
 * 
 * Simple, reusable loading spinner
 * 
 * Features:
 * - 3 sizes (sm, md, lg)
 * - Respects theme colors
 * - Accessible with ARIA labels
 * - Smooth animation
 * 
 * Usage:
 * ```tsx
 * <Spinner size="md" />
 * <Spinner size="lg" label="Loading data..." />
 * ```
 */

import { cn } from '@/utils';

interface SpinnerProps {
    /** Size of the spinner */
    size?: 'sm' | 'md' | 'lg';
    /** Optional accessible label */
    label?: string;
    /** Additional CSS classes */
    className?: string;
}

export function Spinner({ size = 'md', label = 'Loading...', className }: SpinnerProps) {
    const sizeClasses = {
        sm: 'w-4 h-4 border-2',
        md: 'w-8 h-8 border-3',
        lg: 'w-12 h-12 border-4',
    };

    return (
        <div className={cn('inline-flex items-center justify-center', className)} role="status">
            <div
                className={cn(
                    'animate-spin rounded-full',
                    'border-gray-300 dark:border-gray-600',
                    'border-t-azulejo-blue-500 dark:border-t-azulejo-blue-400',
                    sizeClasses[size]
                )}
                aria-hidden="true"
            />
            <span className="sr-only">{label}</span>
        </div>
    );
}

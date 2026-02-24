/**
 * PROGRESS BAR COMPONENT
 * ======================
 * 
 * Visual progress indicator for tasks with known duration
 * 
 * Features:
 * - Smooth animated transitions
 * - Multiple variants (info, success, warning, error)
 * - Optional label display
 * - Accessible with ARIA attributes
 * 
 * Usage:
 * ```tsx
 * <ProgressBar value={60} max={100} />
 * <ProgressBar value={uploadedBytes} max={totalBytes} variant="success" showLabel />
 * ```
 */

import { cn } from '@/utils';
import type { FeedbackVariant } from './types';

interface ProgressBarProps {
    /** Current progress value */
    value: number;
    /** Maximum value (default: 100) */
    max?: number;
    /** Visual variant */
    variant?: FeedbackVariant;
    /** Show percentage label */
    showLabel?: boolean;
    /** Additional CSS classes */
    className?: string;
}

export function ProgressBar({
    value,
    max = 100,
    variant = 'info',
    showLabel = false,
    className,
}: ProgressBarProps) {
    const percentage = Math.min(100, Math.max(0, (value / max) * 100));

    const variantClasses = {
        info: 'bg-azulejo-blue-500 dark:bg-azulejo-blue-400',
        success: 'bg-green-500 dark:bg-green-400',
        warning: 'bg-yellow-500 dark:bg-yellow-400',
        error: 'bg-red-500 dark:bg-red-400',
    };

    return (
        <div className={cn('w-full', className)}>
            {showLabel && (
                <div className="flex justify-between items-center mb-2">
                    <span className="text-sm font-medium text-gray-700 dark:text-gray-300">
                        Progress
                    </span>
                    <span className="text-sm font-medium text-gray-700 dark:text-gray-300">
                        {percentage.toFixed(0)}%
                    </span>
                </div>
            )}
            <div
                className="w-full h-2 bg-gray-200 dark:bg-gray-700 rounded-full overflow-hidden"
                role="progressbar"
                aria-valuenow={value}
                aria-valuemin={0}
                aria-valuemax={max}
            >
                <div
                    className={cn(
                        'h-full rounded-full transition-all duration-300 ease-out',
                        variantClasses[variant]
                    )}
                    style={{ width: `${percentage}%` }}
                />
            </div>
        </div>
    );
}

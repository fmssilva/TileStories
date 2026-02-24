/**
 * STICKY CONTAINER COMPONENT
 * ==========================
 * 
 * Wrapper component that makes children sticky with proper positioning,
 * z-index, and opaque background to hide content scrolling behind.
 * 
 * FEATURES:
 * - Automatic positioning (calculates offset from config)
 * - Opaque background (prevents see-through)
 * - Shadow when sticky (visual depth)
 * - Border separator (optional)
 * - Height enforcement (from config)
 * 
 * USAGE:
 * ```tsx
 * <StickyContainer layerId="header">
 *   <YourHeaderContent />
 * </StickyContainer>
 * ```
 */

import { ReactNode } from 'react';
import { useSticky } from '../hooks/useSticky';
import { cn } from '@/utils/cn';

// ═══════════════════════════════════════════════════════════════════════════
// TYPES
// ═══════════════════════════════════════════════════════════════════════════

interface StickyContainerProps {
    /** ID of sticky layer (must match one in stickyConfig.ts) */
    layerId: string;

    /** Content to make sticky */
    children: ReactNode;

    /** Additional CSS classes */
    className?: string;

    /** Override default background from config */
    backgroundColor?: string;

    /** Apply padding to content (default: true) */
    applyPadding?: boolean;
}

// ═══════════════════════════════════════════════════════════════════════════
// COMPONENT
// ═══════════════════════════════════════════════════════════════════════════

/**
 * Sticky container wrapper component
 * 
 * Makes its children sticky with automatic positioning based on sticky hierarchy.
 * Applies opaque background, shadow, and border as configured.
 * 
 * @example
 * // Basic usage
 * <StickyContainer layerId="header">
 *   <Header />
 * </StickyContainer>
 * 
 * @example
 * // With custom background and no padding
 * <StickyContainer 
 *   layerId="section-tabs" 
 *   backgroundColor="bg-blue-50"
 *   applyPadding={false}
 * >
 *   <TabNavigation />
 * </StickyContainer>
 */
export function StickyContainer({
    layerId,
    children,
    className,
    backgroundColor,
    applyPadding = true
}: StickyContainerProps) {
    const { isSticky, config, stickyClasses, stickyStyles } = useSticky(layerId);

    // Warn if layer not found in config (already warned by useSticky)
    if (!config) {
        // Return children without wrapper if config not found
        return <>{children}</>;
    }

    return (
        <div
            className={cn(
                // Position and z-index from hook
                stickyClasses,

                // Opaque background (prevents see-through)
                backgroundColor || config.backgroundColor || 'bg-white dark:bg-gray-900',

                // Separator (border/shadow)
                config.showSeparator && 'border-b border-gray-200 dark:border-gray-700',

                // Shadow when sticky (adds depth)
                isSticky && 'shadow-md',

                // Smooth shadow transition
                'transition-shadow duration-200',

                // Custom classes
                className
            )}
            style={{
                ...stickyStyles,
                height: `${config.height}px`,
            }}
        >
            {/* Flex container to center content vertically (optional padding) */}
            <div className={cn(
                'h-full flex items-center',
                applyPadding && 'px-4 sm:px-6 lg:px-8'
            )}>
                {children}
            </div>
        </div>
    );
}

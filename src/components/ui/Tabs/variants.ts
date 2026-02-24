/**
 * TABS COMPONENT VARIANTS
 * ========================
 * 
 * CVA (class-variance-authority) variant definitions for Tabs components
 * Separates styling logic from component logic for better maintainability
 */

import { cva } from 'class-variance-authority';

// ============================================================================
// TABSLIST VARIANTS
// ============================================================================

/**
 * TabsList Variants
 * Controls the container style for tab triggers
 */
export const tabsListVariants = cva(
    // Base styles (always applied)
    'inline-flex items-center justify-center',
    {
        variants: {
            /** Visual style of the tabs container */
            variant: {
                default: 'rounded-lg bg-azulejo-ivory-100 dark:bg-gray-900 p-1',
                line: 'border-b border-gray-200 dark:border-gray-800',
                pills: 'gap-2 bg-transparent',
                buttons: 'gap-2 bg-transparent flex-wrap',
            },
            /** Height/vertical spacing */
            size: {
                compact: 'h-9',
                default: 'h-auto',
                spacious: 'h-12',
            },
            /** Should tabs take full width */
            fullWidth: {
                true: 'w-full',
                false: 'w-auto',
            },
        },
        defaultVariants: {
            variant: 'default',
            size: 'default',
            fullWidth: false,
        },
    }
);

// ============================================================================
// TABSTRIGGER VARIANTS
// ============================================================================

/**
 * TabsTrigger Variants
 * Controls the style of individual tab buttons
 */
export const tabsTriggerVariants = cva(
    // Base styles (always applied)
    [
        'inline-flex justify-center whitespace-nowrap',
        'font-medium transition-all duration-200',
        'focus-visible:outline-none focus-visible:ring-2',
        'focus-visible:ring-azulejo-blue focus-visible:ring-offset-2',
        'disabled:pointer-events-none disabled:opacity-50',
        'dark:ring-offset-gray-950',
    ],
    {
        variants: {
            /** Visual style matching TabsList variant */
            variant: {
                default: [
                    'rounded-md px-4 py-2.5',
                    'data-[state=active]:bg-white data-[state=active]:text-gray-900',
                    'data-[state=active]:shadow-sm',
                    'dark:data-[state=active]:bg-gray-950 dark:data-[state=active]:text-white',
                    'hover:bg-white/50 dark:hover:bg-gray-950/50',
                    'text-gray-600 dark:text-gray-400', // Inactive state color
                ],
                line: [
                    'px-4 py-2 border-b-2 border-transparent -mb-[2px]', // Fixed border alignment
                    'data-[state=active]:border-azulejo-blue',
                    'data-[state=active]:text-azulejo-blue dark:data-[state=active]:text-azulejo-blue',
                    'hover:text-gray-900 dark:hover:text-gray-100',
                    'text-gray-600 dark:text-gray-400', // Inactive state color
                ],
                pills: [
                    'rounded-full px-4 py-2',
                    'data-[state=active]:bg-azulejo-blue data-[state=active]:text-white',
                    'hover:bg-gray-100 dark:hover:bg-gray-800',
                    'text-gray-600 dark:text-gray-400', // Inactive state color
                ],
                buttons: [
                    'rounded-lg px-5 py-2.5 border border-gray-300 dark:border-gray-700',
                    'data-[state=active]:border-azulejo-blue data-[state=active]:bg-azulejo-blue/10',
                    'data-[state=active]:text-azulejo-blue dark:data-[state=active]:text-azulejo-blue',
                    'hover:border-gray-400 dark:hover:border-gray-600',
                    'bg-white dark:bg-gray-950', // Background for better contrast
                    'text-gray-600 dark:text-gray-400', // Inactive state color
                ],
            },
            /** Text size and padding */
            size: {
                compact: 'text-xs px-3 py-1.5',
                default: 'text-sm',
                spacious: 'text-base px-6 py-3',
            },
            /** Content layout direction - controls how children are arranged */
            layout: {
                horizontal: 'flex-row items-center gap-2',
                vertical: 'flex-col items-center gap-0.5',
                'icon-left': 'flex-row items-center gap-2',
                'icon-right': 'flex-row-reverse items-center gap-2',
                'icon-top': 'flex-col items-center gap-1',
            },
            /** Active tab color theme (works best with 'line' and 'buttons' variants) */
            colorScheme: {
                default: '',
                blue: [
                    'data-[state=active]:text-blue-600 dark:data-[state=active]:text-blue-400',
                    'data-[state=active]:border-blue-600 dark:data-[state=active]:border-blue-400',
                ],
                green: [
                    'data-[state=active]:text-green-600 dark:data-[state=active]:text-green-400',
                    'data-[state=active]:border-green-600 dark:data-[state=active]:border-green-400',
                ],
                purple: [
                    'data-[state=active]:text-purple-600 dark:data-[state=active]:text-purple-400',
                    'data-[state=active]:border-purple-600 dark:data-[state=active]:border-purple-400',
                ],
                red: [
                    'data-[state=active]:text-red-600 dark:data-[state=active]:text-red-400',
                    'data-[state=active]:border-red-600 dark:data-[state=active]:border-red-400',
                ],
            },
            /** Visual depth/effect */
            appearance: {
                flat: '',
                '3d': [
                    'shadow-md data-[state=active]:shadow-lg',
                    'data-[state=active]:transform data-[state=active]:translate-y-[-2px]',
                    'hover:shadow-lg transition-transform',
                ],
                metal: [
                    'bg-gradient-to-b from-gray-100 to-gray-200',
                    'dark:from-gray-800 dark:to-gray-900',
                    'border border-gray-300 dark:border-gray-700',
                    'data-[state=active]:from-gray-200 data-[state=active]:to-gray-300',
                    'dark:data-[state=active]:from-gray-700 dark:data-[state=active]:to-gray-800',
                    'shadow-inner',
                ],
            },
        },
        defaultVariants: {
            variant: 'default',
            size: 'default',
            layout: 'horizontal',
            colorScheme: 'default',
            appearance: 'flat',
        },
        compoundVariants: [
            // Pills variant should ignore colorScheme since it uses background
            {
                variant: 'pills',
                colorScheme: ['blue', 'green', 'purple', 'red'],
                className: 'data-[state=active]:bg-azulejo-blue data-[state=active]:text-white',
            },
        ],
    }
);

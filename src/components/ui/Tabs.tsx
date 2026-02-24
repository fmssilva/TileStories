/**
 * ENHANCED TABS COMPONENT
 * =======================
 * 
 * Reusable tabs component built on Radix UI Tabs with multiple visual variants
 * Provides accessible, keyboard-navigable tab interface with flexible styling
 * 
 * FEATURES:
 * - Multiple variants: default, line, pills, buttons
 * - Multiple appearances: flat, 3D, metal
 * - Size options: compact, default, spacious
 * - Color schemes: default, blue, green, purple, red
 * - Full TypeScript support with variants
 * - Context-based variant inheritance (set once on TabsList, applies to all TabsTrigger children)
 * - Smooth content transitions with fade/slide animations
 * 
 * BASIC USAGE:
 * ```tsx
 * <Tabs defaultValue="tab1">
 *   <TabsList>
 *     <TabsTrigger value="tab1">Tab 1</TabsTrigger>
 *     <TabsTrigger value="tab2">Tab 2</TabsTrigger>
 *   </TabsList>
 *   <TabsContent value="tab1">Content 1</TabsContent>
 *   <TabsContent value="tab2">Content 2</TabsContent>
 * </Tabs>
 * ```
 * 
 * ENHANCED USAGE (variant inheritance - no repetition needed):
 * ```tsx
 * <Tabs defaultValue="overview">
 *   <TabsList variant="pills" size="spacious" appearance="3d">
 *     <TabsTrigger value="overview">Overview</TabsTrigger>
 *     <TabsTrigger value="analytics">Analytics</TabsTrigger>
 *   </TabsList>
 *   <TabsContent value="overview">...</TabsContent>
 * </Tabs>
 * ```
 */

import * as React from 'react';
import * as TabsPrimitive from '@radix-ui/react-tabs';
import { cva, type VariantProps } from 'class-variance-authority';
import { cn } from '@/utils';
import { Z_INDEX } from '@/design';

// ============================================================================
// CONTEXT FOR VARIANT INHERITANCE
// ============================================================================

interface TabsContextValue {
    variant?: VariantProps<typeof tabsListVariants>['variant'];
    size?: VariantProps<typeof tabsListVariants>['size'];
    colorScheme?: VariantProps<typeof tabsTriggerVariants>['colorScheme'];
    appearance?: VariantProps<typeof tabsTriggerVariants>['appearance'];
}

const TabsContext = React.createContext<TabsContextValue>({});

const useTabsContext = () => React.useContext(TabsContext);

// ============================================================================
// BASE PRIMITIVES (Radix UI)
// ============================================================================

const Tabs = TabsPrimitive.Root;

const TabsContent = React.forwardRef<
    React.ElementRef<typeof TabsPrimitive.Content>,
    React.ComponentPropsWithoutRef<typeof TabsPrimitive.Content>
>(({ className, ...props }, ref) => (
    <TabsPrimitive.Content
        ref={ref}
        className={cn(
            'mt-6 ring-offset-white',
            'focus-visible:outline-none focus-visible:ring-2',
            'focus-visible:ring-azulejo-blue focus-visible:ring-offset-2',
            'dark:ring-offset-gray-950',
            // Smooth animations for content transitions
            'data-[state=active]:animate-in data-[state=active]:fade-in-50',
            'data-[state=active]:slide-in-from-bottom-1 data-[state=active]:duration-300',
            'data-[state=inactive]:animate-out data-[state=inactive]:fade-out-50',
            className
        )}
        {...props}
    />
));
TabsContent.displayName = TabsPrimitive.Content.displayName;

// ============================================================================
// ENHANCED VARIANTS WITH CVA (class-variance-authority)
// ============================================================================

/**
 * TabsList Variants
 * Controls the container style for tab triggers
 */
const tabsListVariants = cva(
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

/**
 * TabsTrigger Variants
 * Controls the style of individual tab buttons
 */
const tabsTriggerVariants = cva(
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

// ============================================================================
// ENHANCED COMPONENTS
// ============================================================================

interface TabsListProps
    extends React.ComponentPropsWithoutRef<typeof TabsPrimitive.List>,
    VariantProps<typeof tabsListVariants> {
    /**
     * Z-index for layering
     * @default Z_INDEX.CONTENT (1000)
     */
    zIndex?: number;
    /** Visual depth/effect to apply to all child TabsTriggers */
    appearance?: 'flat' | '3d' | 'metal';
    /** Active tab color theme to apply to all child TabsTriggers */
    colorScheme?: 'default' | 'blue' | 'green' | 'purple' | 'red';
}

const TabsList = React.forwardRef<
    React.ElementRef<typeof TabsPrimitive.List>,
    TabsListProps
>(({ className, variant, size, fullWidth, zIndex = Z_INDEX.CONTENT, appearance, colorScheme, children, ...props }, ref) => {
    // Memoize context value for performance
    const contextValue = React.useMemo(
        () => ({ variant, size, appearance, colorScheme }),
        [variant, size, appearance, colorScheme]
    );

    return (
        <TabsContext.Provider value={contextValue}>
            <TabsPrimitive.List
                ref={ref}
                className={cn(tabsListVariants({ variant, size, fullWidth }), className)}
                style={{ zIndex }}
                {...props}
            >
                {children}
            </TabsPrimitive.List>
        </TabsContext.Provider>
    );
});
TabsList.displayName = TabsPrimitive.List.displayName;

interface TabsTriggerProps
    extends React.ComponentPropsWithoutRef<typeof TabsPrimitive.Trigger>,
    Partial<VariantProps<typeof tabsTriggerVariants>> {
    /**
     * Content layout direction - controls how children are arranged
     * 
     * @example
     * // Text only (horizontal is default)
     * <TabsTrigger value="tab1">Settings</TabsTrigger>
     * 
     * // Stacked content (e.g., number + subtitle)
     * <TabsTrigger layout="vertical" value="phase1">
     *   <div>1</div>
     *   <div>3 months</div>
     * </TabsTrigger>
     * 
     * // Icon with label (left)
     * <TabsTrigger layout="icon-left" value="profile">
     *   <UserIcon />
     *   <span>Profile</span>
     * </TabsTrigger>
     * 
     * // Icon with label (right)
     * <TabsTrigger layout="icon-right" value="settings">
     *   <span>Settings</span>
     *   <GearIcon />
     * </TabsTrigger>
     * 
     * // Image above label
     * <TabsTrigger layout="icon-top" value="product1">
     *   <img src="..." />
     *   <span>Product Name</span>
     * </TabsTrigger>
     */
    layout?: 'horizontal' | 'vertical' | 'icon-left' | 'icon-right' | 'icon-top';
}

const TabsTrigger = React.forwardRef<
    React.ElementRef<typeof TabsPrimitive.Trigger>,
    TabsTriggerProps
>(({ className, variant, size, layout, colorScheme, appearance, ...props }, ref) => {
    // Get defaults from context (set by TabsList)
    const context = useTabsContext();

    // Props override context values
    const finalVariant = variant ?? context.variant;
    const finalSize = size ?? context.size;
    const finalLayout = layout; // Layout is not inherited from context - always explicit
    const finalColorScheme = colorScheme ?? context.colorScheme;
    const finalAppearance = appearance ?? context.appearance;

    return (
        <TabsPrimitive.Trigger
            ref={ref}
            className={cn(
                tabsTriggerVariants({
                    variant: finalVariant,
                    size: finalSize,
                    layout: finalLayout,
                    colorScheme: finalColorScheme,
                    appearance: finalAppearance
                }),
                className
            )}
            {...props}
        />
    );
});
TabsTrigger.displayName = TabsPrimitive.Trigger.displayName;

// ============================================================================
// EXPORTS
// ============================================================================

export { Tabs, TabsList, TabsTrigger, TabsContent };
export type { TabsListProps, TabsTriggerProps };


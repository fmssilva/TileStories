/**
 * TABSLIST COMPONENT
 * ==================
 * 
 * Container for tab triggers (buttons)
 * Provides variant inheritance to child TabsTrigger components
 */

import * as React from 'react';
import * as TabsPrimitive from '@radix-ui/react-tabs';
import { cn } from '@/utils';
import { Z_INDEX } from '@/design';
import { tabsListVariants } from './variants';
import { TabsContext } from './context';
import type { TabsListProps, TabsContextValue } from './types';

/**
 * TabsList Component
 * 
 * Container for tab triggers. Provides context for variant inheritance.
 * Set variant/size/appearance once, applies to all child TabsTrigger components.
 * 
 * @example
 * // Basic usage
 * <TabsList>
 *   <TabsTrigger value="tab1">Tab 1</TabsTrigger>
 *   <TabsTrigger value="tab2">Tab 2</TabsTrigger>
 * </TabsList>
 * 
 * @example
 * // With variants (inherited by all children)
 * <TabsList variant="pills" size="spacious" appearance="3d">
 *   <TabsTrigger value="tab1">Tab 1</TabsTrigger>
 *   <TabsTrigger value="tab2">Tab 2</TabsTrigger>
 * </TabsList>
 */
export const TabsList = React.forwardRef<
    React.ElementRef<typeof TabsPrimitive.List>,
    TabsListProps
>(({ className, variant, size, fullWidth, zIndex = Z_INDEX.CONTENT, appearance, colorScheme, children, ...props }, ref) => {
    // Memoize context value for performance
    // Only include defined properties to satisfy exactOptionalPropertyTypes
    const contextValue = React.useMemo(() => {
        const value: TabsContextValue = {};
        if (variant !== undefined) value.variant = variant;
        if (size !== undefined) value.size = size;
        if (appearance !== undefined) value.appearance = appearance;
        if (colorScheme !== undefined) value.colorScheme = colorScheme;
        return value;
    }, [variant, size, appearance, colorScheme]);

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

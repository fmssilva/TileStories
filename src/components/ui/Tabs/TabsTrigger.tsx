/**
 * TABSTRIGGER COMPONENT
 * ======================
 * 
 * Individual tab button component
 * Inherits variant/size/appearance from parent TabsList
 */

import * as React from 'react';
import * as TabsPrimitive from '@radix-ui/react-tabs';
import { cn } from '@/utils';
import { tabsTriggerVariants } from './variants';
import { useTabsContext } from './context';
import type { TabsTriggerProps } from './types';

/**
 * TabsTrigger Component
 * 
 * Individual tab button. Inherits variant/size/appearance from TabsList context.
 * Supports flexible layouts for different content structures (text, icons, stacked).
 * 
 * @example
 * // Simple text tab
 * <TabsTrigger value="settings">Settings</TabsTrigger>
 * 
 * @example
 * // Stacked content (number + subtitle)
 * <TabsTrigger layout="vertical" value="phase1">
 *   <div className="font-bold text-xl">1</div>
 *   <div className="text-xs">3 months</div>
 * </TabsTrigger>
 * 
 * @example
 * // Icon with label
 * <TabsTrigger layout="icon-left" value="profile">
 *   <UserIcon />
 *   <span>Profile</span>
 * </TabsTrigger>
 */
export const TabsTrigger = React.forwardRef<
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

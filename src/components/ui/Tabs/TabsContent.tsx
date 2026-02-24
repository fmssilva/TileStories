/**
 * TABS CONTENT COMPONENT
 * =======================
 * 
 * Content panel for each tab
 * Displays content associated with the active tab
 */

import * as React from 'react';
import * as TabsPrimitive from '@radix-ui/react-tabs';
import { cn } from '@/utils';

/**
 * TabsContent Component
 * 
 * Content panel that displays when its associated tab is active.
 * Includes smooth fade-in/slide animations.
 * 
 * @example
 * <TabsContent value="overview">
 *   <h3>Overview</h3>
 *   <p>This is the overview content...</p>
 * </TabsContent>
 */
export const TabsContent = React.forwardRef<
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

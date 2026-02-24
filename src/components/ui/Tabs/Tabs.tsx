/**
 * TABS ROOT COMPONENT
 * ===================
 * 
 * Root container for the tabs system
 * Simple wrapper around Radix UI Tabs.Root
 */

import * as TabsPrimitive from '@radix-ui/react-tabs';

/**
 * Tabs Root Component
 * 
 * Container for the entire tabs system. Manages the active tab state.
 * 
 * @example
 * <Tabs defaultValue="tab1" onValueChange={handleChange}>
 *   <TabsList>
 *     <TabsTrigger value="tab1">Tab 1</TabsTrigger>
 *     <TabsTrigger value="tab2">Tab 2</TabsTrigger>
 *   </TabsList>
 *   <TabsContent value="tab1">Content 1</TabsContent>
 *   <TabsContent value="tab2">Content 2</TabsContent>
 * </Tabs>
 */
export const Tabs = TabsPrimitive.Root;

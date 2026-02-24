/**
 * TABS CONTEXT
 * =============
 * 
 * React Context for variant inheritance in Tabs component
 * Allows TabsList to pass props to all child TabsTrigger components
 * Reduces code repetition and ensures consistency
 */

import * as React from 'react';
import type { TabsContextValue } from './types';

// ============================================================================
// CONTEXT
// ============================================================================

/**
 * Context for passing variant/size/appearance from TabsList to TabsTrigger
 * This enables clean syntax like:
 * 
 * <TabsList variant="pills" size="spacious">
 *   <TabsTrigger value="tab1">Tab 1</TabsTrigger>  // Inherits pills + spacious
 *   <TabsTrigger value="tab2">Tab 2</TabsTrigger>  // Inherits pills + spacious
 * </TabsList>
 */
export const TabsContext = React.createContext<TabsContextValue>({});

/**
 * Hook to access Tabs context
 * Used by TabsTrigger to inherit variant/size/appearance from TabsList
 */
export const useTabsContext = () => React.useContext(TabsContext);

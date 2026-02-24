/**
 * TABS COMPONENT SYSTEM
 * ======================
 * 
 * Reusable tabs component built on Radix UI with multiple visual variants.
 * Provides accessible, keyboard-navigable tab interface with flexible styling.
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
 * ARCHITECTURE:
 * - Tabs.tsx - Root container component
 * - TabsList.tsx - Container for tab buttons (provides context)
 * - TabsTrigger.tsx - Individual tab button (consumes context)
 * - TabsContent.tsx - Content panel for each tab
 * - variants.ts - CVA variant definitions
 * - types.ts - TypeScript interfaces
 * - context.ts - React Context for variant inheritance
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

// ============================================================================
// COMPONENT EXPORTS
// ============================================================================

export { Tabs } from './Tabs';
export { TabsList } from './TabsList';
export { TabsTrigger } from './TabsTrigger';
export { TabsContent } from './TabsContent';

// ============================================================================
// TYPE EXPORTS
// ============================================================================

export type {
    TabsContextValue,
    TabsListProps,
    TabsTriggerProps,
} from './types';

// ============================================================================
// VARIANT EXPORTS (for advanced use cases)
// ============================================================================

export { tabsListVariants, tabsTriggerVariants } from './variants';
export { TabsContext, useTabsContext } from './context';

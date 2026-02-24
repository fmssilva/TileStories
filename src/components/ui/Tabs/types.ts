/**
 * TABS COMPONENT TYPES
 * =====================
 * 
 * TypeScript type definitions for the Tabs component system
 * Centralizes all interfaces and type exports
 */

import * as React from 'react';
import * as TabsPrimitive from '@radix-ui/react-tabs';

// ============================================================================
// CONTEXT TYPES
// ============================================================================

/**
 * Context value for variant inheritance
 * Allows TabsList to pass variant/size/appearance to all child TabsTrigger components
 * Reduces repetition - set once on TabsList, applies to all children
 */
export interface TabsContextValue {
    /** Visual style of the tabs container */
    variant?: 'default' | 'line' | 'pills' | 'buttons';

    /** Height/vertical spacing */
    size?: 'compact' | 'default' | 'spacious';

    /** Active tab color theme */
    colorScheme?: 'default' | 'blue' | 'green' | 'purple' | 'red';

    /** Visual depth/effect */
    appearance?: 'flat' | '3d' | 'metal';
}
// ============================================================================
// COMPONENT PROP TYPES
// ============================================================================

/**
 * Props for TabsList component
 * Controls the container style for tab triggers
 */
export interface TabsListProps
    extends React.ComponentPropsWithoutRef<typeof TabsPrimitive.List> {
    /** Visual style of the tabs container */
    variant?: 'default' | 'line' | 'pills' | 'buttons';

    /** Height/vertical spacing */
    size?: 'compact' | 'default' | 'spacious';

    /** Should tabs take full width */
    fullWidth?: boolean;

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

/**
 * Props for TabsTrigger component
 * Controls the style of individual tab buttons
 */
export interface TabsTriggerProps
    extends React.ComponentPropsWithoutRef<typeof TabsPrimitive.Trigger> {
    /** Visual style matching TabsList variant */
    variant?: 'default' | 'line' | 'pills' | 'buttons';

    /** Text size and padding */
    size?: 'compact' | 'default' | 'spacious';

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

    /** Active tab color theme */
    colorScheme?: 'default' | 'blue' | 'green' | 'purple' | 'red';

    /** Visual depth/effect */
    appearance?: 'flat' | '3d' | 'metal';
}

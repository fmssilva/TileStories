# Sticky Manager Guide

## 🎯 Overview
Centralized system for managing sticky elements with automatic positioning, z-index coordination, and hierarchical support.

**Location**: `src/layout_and_navigation/sticky/`

## 📚 Documentation

- **[Simplified API Guide](./GUIDE_STICKY_SIMPLIFIED_API.md)** - ⭐ **START HERE** - New one-hook solution
- This guide - Advanced features and architecture

## ✨ What's New - Simplified API

The sticky system now features a **revolutionary one-hook solution** that eliminates 90% of boilerplate:

```tsx
// OLD WAY - Manual everything (15+ lines)
const ref = useRef<HTMLDivElement>(null);
const { stickyClasses, stickyStyles, reportHeight, registerPosition } = useSticky('header');
useEffect(() => { /* manual position */ }, []);
useEffect(() => { /* manual height */ }, []);
// ... more boilerplate

// NEW WAY - One hook does it all (1 line!)
const { ref, isShrunk, stickyClasses, stickyStyles } = useSticky('header', {
    enableShrink: true,  // Auto scroll-shrink with hysteresis (no trembling)
    autoMeasure: true,   // Auto-measure with ResizeObserver
});
```

**🚀 See [GUIDE_STICKY_SIMPLIFIED_API.md](./GUIDE_STICKY_SIMPLIFIED_API.md) for full details!**

---

## Core Concepts

### 1. Hierarchical Architecture
- **Flat array with parent references** - not nested trees
- **Conditional activation** - layers activate based on app state
- **Auto-offset calculation** - only counts active ancestor heights
- **Z-index coordination** - prevents overlap conflicts

### 2. Key Files
```
sticky/
├── config/stickyConfig.ts      # Single source of truth - all layers defined here
├── contexts/StickyContext.tsx  # Global state management
├── hooks/useSticky.ts          # Hook for custom sticky implementations
├── components/StickyContainer.tsx  # Wrapper component (preferred method)
└── index.ts                    # Public API exports
```

## Quick Start

### Step 1: Add Layer to Config
```typescript
// src/layout_and_navigation/sticky/config/stickyConfig.ts
export const STICKY_LAYERS: StickyLayerConfig[] = [
  {
    id: 'my-sticky-element',
    label: 'My Sticky Element',
    height: 60,                           // Height in pixels
    zIndex: Z_INDEX.STICKY - 10,          // Use Z_INDEX constants
    parent: null,                         // null = root level, or parent layer ID
    backgroundColor: 'bg-white dark:bg-gray-900',
    showSeparator: true,
    stopAtElement: '#footer',             // Optional: CSS selector to stop sticking
    activeWhen: (context) => context.scrollY > 100  // Optional: condition
  }
];
```

### Step 2: Use StickyContainer (Recommended)
```tsx
import { StickyContainer } from '@/layout_and_navigation/sticky';

<StickyContainer layerId="my-sticky-element">
  <YourContent />
</StickyContainer>
```

### Alternative: Use Hook Directly
```tsx
import { useSticky } from '@/layout_and_navigation/sticky';

const { stickyClasses, stickyStyles, isSticky } = useSticky('my-sticky-element');

<div className={stickyClasses} style={stickyStyles}>
  {isSticky && <CompactView />}
  {!isSticky && <FullView />}
</div>
```

## Configuration Details

### Layer Properties

| Property          | Type           | Required | Description                               |
| ----------------- | -------------- | -------- | ----------------------------------------- |
| `id`              | string         | ✅        | Unique identifier                         |
| `label`           | string         | ✅        | Human-readable name (debugging)           |
| `height`          | number         | ✅        | Height in pixels (0 for no layout impact) |
| `zIndex`          | number         | ✅        | Z-index value (use Z_INDEX constants)     |
| `parent`          | string \| null | ✅        | Parent layer ID or null for root          |
| `backgroundColor` | string         | ❌        | Tailwind classes (must be opaque)         |
| `showSeparator`   | boolean        | ❌        | Show border/shadow separator              |
| `stopAtElement`   | string         | ❌        | CSS selector - stop when element in view  |
| `activeWhen`      | function       | ❌        | `(context) => boolean` condition          |

### Z-Index Strategy
```typescript
import { Z_INDEX } from '@/design';

// Z_INDEX.STICKY = 3000 (floating elements)
// Root sticky layers: Z_INDEX.STICKY - 10 = 2990
// Child layers: Parent zIndex - 10
// BackToTop button: Z_INDEX.STICKY (highest)
```

### Hierarchical Layers

**Parent-Child Example**:
```typescript
[
  // Root level
  {
    id: 'section-tabs',
    parent: null,
    zIndex: Z_INDEX.STICKY - 10,
    height: 60
  },
  // Child only active when parent is active
  {
    id: 'subsection-tabs',
    parent: 'section-tabs',
    zIndex: Z_INDEX.STICKY - 20,  // Lower than parent
    height: 50,
    activeWhen: (ctx) => ctx.selectedTab === 'details'
  }
]
```

**Offset Calculation**: Child top position = sum of active ancestor heights

### Conditional Activation

Update activation context to trigger conditions:
```tsx
import { useStickyContext } from '@/layout_and_navigation/sticky';

const { updateActivationContext } = useStickyContext();

// Update when app state changes
useEffect(() => {
  updateActivationContext({ 
    selectedTab: currentTab,
    selectedPhase: activePhase 
  });
}, [currentTab, activePhase]);
```

**Define condition in config**:
```typescript
{
  id: 'phase-1-tabs',
  parent: 'phase-tabs',
  activeWhen: (context) => context.selectedPhase === 1
}
```

## Common Patterns

### Pattern 1: Simple Sticky Header
```typescript
// Config
{ id: 'header', label: 'Header', height: 80, zIndex: Z_INDEX.STICKY - 10, parent: null }

// Component
<StickyContainer layerId="header">
  <Header />
</StickyContainer>
```

### Pattern 2: Sticky Tabs that Stop at Footer
```typescript
// Config
{ 
  id: 'tabs', 
  height: 60, 
  zIndex: Z_INDEX.STICKY - 10,
  parent: null,
  stopAtElement: '#page-footer'  // Becomes relative when footer appears
}
```

### Pattern 3: Conditional Nested Tabs
```typescript
// Root tabs always active
{ id: 'main-tabs', parent: null, height: 60, zIndex: 2990 }

// Detail tabs only when tab 1 selected
{ 
  id: 'tab-1-details', 
  parent: 'main-tabs', 
  height: 50, 
  zIndex: 2980,
  activeWhen: (ctx) => ctx.activeMainTab === 1 
}
```

### Pattern 4: Custom Styling with Hook
```tsx
const { isSticky, offset, config } = useSticky('my-layer');

return (
  <div 
    className={cn(
      isSticky ? 'sticky shadow-lg' : 'relative',
      'bg-gradient-to-r from-blue-500 to-purple-500'
    )}
    style={{ 
      top: isSticky ? `${offset}px` : undefined,
      zIndex: config?.zIndex 
    }}
  >
    Content
  </div>
);
```

## Important Rules

1. **Always add layers to `STICKY_LAYERS` config first** - before using them
2. **Use Z_INDEX constants** - import from `@/design`, never hardcode
3. **Parent z-index must be higher than children** - subtract 10 for each level
4. **Background must be opaque** - use `bg-white/95` not `bg-white/50` to hide content behind
5. **Height must match actual height** - used for offset calculations
6. **Only update activationContext when needed** - prevents unnecessary re-renders
7. **Provider must wrap app** - `<StickyProvider>` in main.tsx or App.tsx

## Debugging

### Check if layer is registered:
```tsx
import { STICKY_LAYERS } from '@/layout_and_navigation/sticky';
console.log(STICKY_LAYERS.find(l => l.id === 'my-layer'));
```

### Check current active layers:
```tsx
const { activeLayers } = useStickyContext();
console.log([...activeLayers.entries()]);
```

### Get layer offset:
```tsx
const { getOffset } = useStickyContext();
console.log('Offset:', getOffset('my-layer'));
```

## Troubleshooting

| Issue                  | Solution                                                        |
| ---------------------- | --------------------------------------------------------------- |
| Layer not sticking     | Check if registered in STICKY_LAYERS config                     |
| Wrong offset           | Verify parent heights and active state                          |
| Content visible behind | Use opaque background (e.g., `bg-white` not `bg-white/50`)      |
| Z-index conflicts      | Follow z-index strategy (parent > child, use Z_INDEX constants) |
| Infinite loops         | Don't include activeLayers in useEffect deps                    |
| activeWhen not working | Call updateActivationContext when state changes                 |

## Advanced: Helper Functions

```tsx
import { 
  getStickyLayer,      // Get layer config by ID
  getStickyOffset,     // Calculate offset for layer
  getLayerPath,        // Get full ancestor path
  getChildLayers,      // Get children of a layer
  shouldLayerBeActive  // Check if layer meets conditions
} from '@/layout_and_navigation/sticky';
```

## Migration from Manual Sticky

**Before**:
```tsx
const [isSticky, setIsSticky] = useState(false);
useEffect(() => {
  const handleScroll = () => setIsSticky(window.scrollY > 100);
  window.addEventListener('scroll', handleScroll);
  return () => window.removeEventListener('scroll', handleScroll);
}, []);

<div className={isSticky ? 'sticky top-20' : 'relative'} style={{ zIndex: 3000 }}>
```

**After**:
```tsx
// Add to config once
{ id: 'my-element', height: 60, zIndex: Z_INDEX.STICKY - 10, parent: null }

// Use in component
<StickyContainer layerId="my-element">
```

**Benefits**: No manual scroll listeners, automatic positioning, coordinated z-index, hierarchical support.

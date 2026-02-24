# Sticky System - Simplified API Guide

## 🎯 Overview

The sticky system now features a **one-stop declarative hook** that eliminates all boilerplate code. Everything is automated: position registration, height measurement, scroll-shrink integration, and trembling prevention.

---

## 📋 Before vs After Comparison

### ❌ OLD WAY (Manual Everything)

```tsx
function MyHeader() {
    const ref = useRef<HTMLDivElement>(null);
    const { isSticky, stickyClasses, stickyStyles, reportHeight, registerPosition } = useSticky('my-header');
    const isShrunk = useScrollShrink({ shrinkThreshold: 100, expandThreshold: 50 });
    
    // Manual position registration
    useEffect(() => {
        if (ref.current) {
            const rect = ref.current.getBoundingClientRect();
            const offsetTop = rect.top + window.scrollY;
            registerPosition(offsetTop);
        }
    }, [registerPosition]); // ⚠️ Can cause re-renders
    
    // Manual height measurement
    useEffect(() => {
        if (ref.current) {
            const height = ref.current.offsetHeight;
            reportHeight(height);
        }
    }, []); // ⚠️ Doesn't update on resize
    
    return (
        <div ref={ref} className={stickyClasses} style={stickyStyles}>
            <div className={isShrunk ? 'compact' : 'expanded'}>
                Content
            </div>
        </div>
    );
}
```

**Problems:**
- 🔴 Manual ref management
- 🔴 Multiple useEffect hooks
- 🔴 Repetitive boilerplate in every component
- 🔴 No automatic resize handling
- 🔴 Dependency issues causing re-renders
- 🔴 Separate hook for scroll-shrink

---

### ✅ NEW WAY (Automated Everything)

```tsx
function MyHeader() {
    const { 
        ref,           // Auto-managed ref
        isSticky,      // Sticky state
        isShrunk,      // Scroll-shrink state (with hysteresis)
        stickyClasses, // CSS classes
        stickyStyles,  // Inline styles
    } = useSticky('my-header', {
        enableShrink: { 
            shrinkThreshold: 100, 
            expandThreshold: 50 
        },
        autoMeasure: true,          // ResizeObserver auto-measurement
        autoRegisterPosition: true, // Auto position registration
    });
    
    return (
        <div ref={ref} className={stickyClasses} style={stickyStyles}>
            <div className={isShrunk ? 'compact' : 'expanded'}>
                Content
            </div>
        </div>
    );
}
```

**Benefits:**
- ✅ Single hook call
- ✅ Zero manual useEffect hooks
- ✅ Auto-measurement with ResizeObserver
- ✅ Auto position registration
- ✅ Integrated scroll-shrink with hysteresis
- ✅ No trembling/flickering
- ✅ Fully typed TypeScript
- ✅ Reusable across all components

---

## 🚀 API Reference

### `useSticky(layerId, options)`

#### Parameters

**`layerId`** (string, required)
- Must match a layer ID in `stickyConfig.ts`
- Example: `'my-header'`, `'section-tabs'`, `'work-plan-phase-tabs'`

**`options`** (object, optional)

| Option                 | Type                | Default | Description                                   |
| ---------------------- | ------------------- | ------- | --------------------------------------------- |
| `enableShrink`         | `boolean \| object` | `false` | Enable scroll-based shrinking with hysteresis |
| `autoMeasure`          | `boolean`           | `true`  | Auto-measure height with ResizeObserver       |
| `autoRegisterPosition` | `boolean`           | `true`  | Auto-register element position on mount       |
| `shrinkClass`          | `string`            | `''`    | CSS classes when shrunk                       |
| `expandClass`          | `string`            | `''`    | CSS classes when expanded                     |

#### Shrink Options (when `enableShrink` is object)

```tsx
enableShrink: {
    shrinkThreshold: 100,  // Shrink at 100px scroll
    expandThreshold: 50,   // Expand at 50px scroll (50px hysteresis)
}
```

**Hysteresis Explanation:**
- The gap between thresholds (100 - 50 = 50px) is the "dead zone"
- Prevents rapid toggling when scrolling near the threshold
- Eliminates trembling/flickering

#### Return Values

```tsx
{
    ref: RefObject<HTMLDivElement>,        // Attach to your element
    offset: number,                        // Top offset in pixels
    isSticky: boolean,                     // Sticky state
    isShrunk: boolean,                     // Shrink state (if enabled)
    config: StickyLayerConfig,             // Layer config
    stickyClasses: string,                 // 'sticky' or 'relative'
    stickyStyles: CSSProperties,           // { top, zIndex }
    shrinkClasses: string,                 // Combined shrink classes
    reportHeight: (height) => void,        // Manual height reporting
    registerPosition: (offsetTop) => void, // Manual position registration
}
```

---

## 💡 Usage Examples

### Example 1: Basic Sticky Header

```tsx
function Header() {
    const { ref, stickyClasses, stickyStyles } = useSticky('header');
    
    return (
        <header ref={ref} className={stickyClasses} style={stickyStyles}>
            My Header
        </header>
    );
}
```

---

### Example 2: Sticky with Scroll Shrink

```tsx
function ShrinkingHeader() {
    const { ref, isShrunk, stickyClasses, stickyStyles } = useSticky('header', {
        enableShrink: true, // Use default thresholds (50px/30px)
    });
    
    return (
        <header 
            ref={ref} 
            className={`${stickyClasses} transition-all duration-300`} 
            style={stickyStyles}
        >
            <div className={isShrunk ? 'h-16 text-lg' : 'h-24 text-3xl'}>
                Logo
            </div>
        </header>
    );
}
```

---

### Example 3: Custom Shrink Thresholds

```tsx
function CustomShrink() {
    const { ref, isShrunk, stickyClasses, stickyStyles } = useSticky('tabs', {
        enableShrink: {
            shrinkThreshold: 200,  // Shrink after 200px scroll
            expandThreshold: 100,  // Expand when back to 100px (100px hysteresis)
        },
    });
    
    return (
        <div ref={ref} className={stickyClasses} style={stickyStyles}>
            {isShrunk ? 'Compact Tabs' : 'Expanded Tabs'}
        </div>
    );
}
```

---

### Example 4: With Custom Shrink Classes

```tsx
function StyledShrink() {
    const { 
        ref, 
        isShrunk,
        stickyClasses, 
        stickyStyles,
        shrinkClasses, // Auto-generated with transition
    } = useSticky('header', {
        enableShrink: true,
        shrinkClass: 'bg-blue-600 py-2 px-4',    // Compact styles
        expandClass: 'bg-blue-500 py-6 px-8',    // Expanded styles
    });
    
    return (
        <header 
            ref={ref} 
            className={`${stickyClasses} ${shrinkClasses}`} 
            style={stickyStyles}
        >
            Content
        </header>
    );
}
```

---

### Example 5: Manual Control (Advanced)

```tsx
function ManualControl() {
    const { 
        ref, 
        reportHeight,        // Manual height update
        registerPosition,    // Manual position update
        stickyClasses, 
        stickyStyles 
    } = useSticky('advanced', {
        autoMeasure: false,          // Disable auto-measurement
        autoRegisterPosition: false, // Disable auto-registration
    });
    
    useEffect(() => {
        // Custom measurement logic
        if (ref.current) {
            const customHeight = calculateCustomHeight(ref.current);
            reportHeight(customHeight);
        }
    }, [/* dependencies */]);
    
    return (
        <div ref={ref} className={stickyClasses} style={stickyStyles}>
            Advanced Usage
        </div>
    );
}
```

---

## 🔧 Configuration (stickyConfig.ts)

Add your sticky layers to `stickyConfig.ts`:

```typescript
export const STICKY_LAYERS: StickyLayerConfig[] = [
    {
        id: 'my-header',
        label: 'Site Header',
        height: 80,
        zIndex: Z_INDEX.STICKY - 10,
        parent: null,
        backgroundColor: 'bg-white/95 dark:bg-gray-900/95',
        showSeparator: true,
    },
    {
        id: 'section-tabs',
        label: 'Section Navigation',
        height: 60,
        zIndex: Z_INDEX.STICKY - 20,
        parent: 'my-header', // Child of header
        backgroundColor: 'bg-gray-50/95 dark:bg-gray-800/95',
        showSeparator: true,
    },
];
```

---

## 📊 Performance Optimizations

### Built-in Optimizations

1. **ResizeObserver** - Efficient height measurement
   - Only triggers on actual size changes
   - Batched by the browser
   - Better than polling with setInterval

2. **RequestAnimationFrame** - Smooth scroll handling
   - Throttles scroll events to 60fps
   - Prevents layout thrashing
   - Minimal CPU usage

3. **Passive Listeners** - Better scroll performance
   ```tsx
   window.addEventListener('scroll', handler, { passive: true });
   ```

4. **Dependency Optimization** - Prevents unnecessary re-renders
   - Memoized callbacks with `useCallback`
   - Stable refs with `useRef`
   - Controlled state updates

---

## 🐛 Troubleshooting

### Issue: Element not becoming sticky

**Solution:** Ensure layer ID is registered in `stickyConfig.ts`

```tsx
// Check console for warning:
// "[useSticky] Layer 'my-id' not found in STICKY_LAYERS config"
```

---

### Issue: Height not updating

**Solution:** Ensure `autoMeasure: true` (default)

```tsx
const { ref } = useSticky('my-layer', {
    autoMeasure: true, // ✅ Enables ResizeObserver
});
```

---

### Issue: Trembling/flickering when shrinking

**Solution:** Increase hysteresis gap

```tsx
const { ref, isShrunk } = useSticky('my-layer', {
    enableShrink: {
        shrinkThreshold: 100,
        expandThreshold: 30, // 70px gap (larger = more stable)
    },
});
```

---

### Issue: Position registered incorrectly

**Solution:** Ensure element is rendered before registration

```tsx
// Auto-registration happens after mount, so element must be visible
// If using conditional rendering, consider manual registration:

const { ref, registerPosition } = useSticky('my-layer', {
    autoRegisterPosition: false,
});

useEffect(() => {
    if (isVisible && ref.current) {
        const rect = ref.current.getBoundingClientRect();
        registerPosition(rect.top + window.scrollY);
    }
}, [isVisible]);
```

---

## 📝 Console Logs

### Initial Load
```
📍 [StickyContext] Layer "my-header" position registered: 100px from top
📏 [StickyContext] Layer "my-header" height measured: 80px
```

### During Scroll
```
📏 [useScrollShrink] SHRINK triggered at scrollY: 262px (threshold: 100px)
🔄 [StickyContext] Layer "my-header" BECAME STICKY at scrollY: 1100px
```

---

## 🎓 Best Practices

### ✅ Do This

```tsx
// Simple, clean, one hook call
const { ref, isShrunk, stickyClasses, stickyStyles } = useSticky('header', {
    enableShrink: true,
});

return <header ref={ref} className={stickyClasses} style={stickyStyles}>...</header>;
```

### ❌ Don't Do This

```tsx
// Unnecessary manual management
const ref = useRef<HTMLDivElement>(null);
const { stickyClasses, stickyStyles, reportHeight } = useSticky('header', {
    autoMeasure: false, // Why disable automation?
});

useEffect(() => {
    if (ref.current) {
        reportHeight(ref.current.offsetHeight); // Unnecessary manual measurement
    }
}, []);
```

---

## 🚀 Migration Guide

### Step 1: Update imports

```tsx
// Before
import { useSticky } from '@/layout_and_navigation/sticky';
import { useScrollShrink } from '@/layout_and_navigation/sticky';

// After (just one import)
import { useSticky } from '@/layout_and_navigation/sticky';
```

### Step 2: Replace hook usage

```tsx
// Before (multiple hooks, manual refs, effects)
const ref = useRef<HTMLDivElement>(null);
const { stickyClasses, stickyStyles, reportHeight, registerPosition } = useSticky('id');
const isShrunk = useScrollShrink({ ... });

useEffect(() => { /* manual registration */ }, []);
useEffect(() => { /* manual measurement */ }, []);

// After (single hook, auto everything)
const { ref, isShrunk, stickyClasses, stickyStyles } = useSticky('id', {
    enableShrink: true,
});
```

### Step 3: Remove manual effects

```tsx
// Delete these - they're now automated!
// ❌ useEffect(() => registerPosition(...), []);
// ❌ useEffect(() => reportHeight(...), []);
```

---

## 🎉 Summary

The simplified `useSticky` hook provides:

- ✅ **One-line integration** - Just attach the ref
- ✅ **Zero boilerplate** - No manual effects or refs
- ✅ **Auto-everything** - Position, height, shrink, all automated
- ✅ **No trembling** - Built-in hysteresis prevents flicker
- ✅ **High performance** - ResizeObserver + RAF throttling
- ✅ **Fully typed** - Complete TypeScript support
- ✅ **Easy debugging** - Comprehensive console logging

**Result:** Sticky headers that "just work" with minimal code! 🚀

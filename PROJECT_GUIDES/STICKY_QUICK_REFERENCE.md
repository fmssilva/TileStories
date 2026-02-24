# Sticky System - Quick Reference Card

## 🚀 One-Line Setup

```tsx
const { ref, isShrunk, stickyClasses, stickyStyles } = useSticky('my-layer', {
    enableShrink: true,
});

return <header ref={ref} className={stickyClasses} style={stickyStyles}>...</header>;
```

---

## 📋 Common Patterns

### Basic Sticky
```tsx
const { ref, stickyClasses, stickyStyles } = useSticky('layer-id');
```

### With Shrink (Default Thresholds)
```tsx
const { ref, isShrunk, stickyClasses, stickyStyles } = useSticky('layer-id', {
    enableShrink: true,
});
```

### Custom Shrink Thresholds
```tsx
const { ref, isShrunk, stickyClasses, stickyStyles } = useSticky('layer-id', {
    enableShrink: {
        shrinkThreshold: 100,  // Shrink at 100px
        expandThreshold: 50,   // Expand at 50px (50px hysteresis)
    },
});
```

### Manual Control (Advanced)
```tsx
const { ref, reportHeight, registerPosition } = useSticky('layer-id', {
    autoMeasure: false,
    autoRegisterPosition: false,
});
```

---

## 🎨 Styling Patterns

### Conditional Classes
```tsx
<div className={isShrunk ? 'h-16 text-sm' : 'h-24 text-2xl'}>
```

### Transitions
```tsx
<div className="transition-all duration-300">
```

### Combined
```tsx
<div className={`${stickyClasses} transition-all duration-300`}>
    <div className={isShrunk ? 'compact-mode' : 'expanded-mode'}>
        Content
    </div>
</div>
```

---

## 🔧 Configuration (stickyConfig.ts)

```typescript
{
    id: 'my-layer',
    label: 'My Layer',
    height: 80,
    zIndex: Z_INDEX.STICKY - 10,
    parent: null,  // or parent layer ID
    backgroundColor: 'bg-white/95',
    showSeparator: true,
}
```

---

## 📊 Options Reference

| Option                 | Type                | Default | Description             |
| ---------------------- | ------------------- | ------- | ----------------------- |
| `enableShrink`         | `boolean \| object` | `false` | Enable scroll shrinking |
| `autoMeasure`          | `boolean`           | `true`  | Auto-measure height     |
| `autoRegisterPosition` | `boolean`           | `true`  | Auto-register position  |
| `shrinkClass`          | `string`            | `''`    | CSS when shrunk         |
| `expandClass`          | `string`            | `''`    | CSS when expanded       |

---

## 🎯 Return Values

```typescript
{
    ref,           // Attach to element
    offset,        // Top offset (px)
    isSticky,      // Sticky state
    isShrunk,      // Shrink state
    config,        // Layer config
    stickyClasses, // CSS classes
    stickyStyles,  // Inline styles
    shrinkClasses, // Shrink classes
    reportHeight,  // Manual height
    registerPosition, // Manual position
}
```

---

## 📝 Console Logs

### Normal
```
📍 Layer position registered: 1098px from top
📏 Layer height measured: 132px
📏 SHRINK triggered at scrollY: 262px
🔄 Layer BECAME STICKY at scrollY: 1100px
```

### Problems
```
❌ Multiple position registrations
❌ Rapid height changes
❌ Rapid shrink/expand toggling
```

---

## ⚡ Performance Tips

1. **Use passive listeners** (already done)
2. **Increase hysteresis gap** for stability
3. **Use CSS transitions** for smoothness
4. **Keep content static** when possible

---

## 🐛 Troubleshooting

| Issue          | Solution                   |
| -------------- | -------------------------- |
| Not sticky     | Check layer ID in config   |
| Height wrong   | Ensure `autoMeasure: true` |
| Trembling      | Increase hysteresis gap    |
| Position wrong | Check element visibility   |

---

## 📚 Full Documentation

- **[Simplified API Guide](./GUIDE_STICKY_SIMPLIFIED_API.md)**
- **[Implementation Summary](./STICKY_SIMPLIFIED_API_SUMMARY.md)**
- **[Main Sticky Guide](./GUIDE_STICKY_MANAGER.md)**

---

## 💡 Example Component

```tsx
import { useSticky } from '@/layout_and_navigation/sticky';

function MyHeader() {
    const { ref, isShrunk, stickyClasses, stickyStyles } = useSticky('header', {
        enableShrink: { shrinkThreshold: 100, expandThreshold: 50 },
    });
    
    return (
        <header 
            ref={ref} 
            className={`${stickyClasses} transition-all duration-300`}
            style={stickyStyles}
        >
            <div className={`
                ${isShrunk 
                    ? 'bg-blue-600 py-2 px-4' 
                    : 'bg-blue-500 py-6 px-8'
                }
                text-white
            `}>
                <h1 className={isShrunk ? 'text-lg' : 'text-3xl'}>
                    Logo
                </h1>
            </div>
        </header>
    );
}
```

**That's it! One hook, zero boilerplate.** 🚀

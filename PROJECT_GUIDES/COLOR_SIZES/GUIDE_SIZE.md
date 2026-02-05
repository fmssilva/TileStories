# SIZE SYSTEM GUIDE FOR AI AGENTS
**Three-Tier Size Architecture - Clean & Simple**

---

## 🏗️ ARCHITECTURE OVERVIEW

This project uses a **simplified three-tier size system** similar to our color architecture:

```
TIER 1: Global Foundations    → src/design/sizes.ts & sizeHelpers.ts
TIER 2: Domain-Specific       → src/[domain]/sizes.ts  
TIER 3: Local Component       → Direct Tailwind classes in components
```

**PHILOSOPHY**: Favor Tailwind classes over custom helpers. Keep it simple!

---

## 🎯 DECISION TREE: WHAT TO USE WHEN

### ✅ USE TAILWIND CLASSES (PREFERRED)
```tsx
// Spacing
className="p-4 m-6 gap-8 space-x-3"

// Sizing  
className="w-6 h-6 h-10 max-w-lg"

// Layout
className="container mx-auto px-4 py-16"
```

### 🔧 USE HELPERS (Only When Dynamic)
```tsx
// Props-based sizing
<Icon size="lg" />        // Uses getIconSize()
<Button size="md" />      // Uses simple size mapping

// Dynamic calculations
style={{ padding: getSpacing(size) }}
```

### 🚫 AVOID COMPLEX ABSTRACTIONS
```tsx
// ❌ DON'T create complex helpers
// ❌ DON'T use sizePresets for simple cases  
// ❌ DON'T over-engineer simple spacing
```

---

## 📖 IMPORT PATTERNS

### Global Foundations (Tier 1)
```typescript
// Only for dynamic sizing
import { getSpacing, getIconSize } from '@/design/sizeHelpers';
import { spacing, componentSizes } from '@/design/sizes';
```

### Domain Patterns (Tier 2)  
```typescript
// For domain-specific patterns
import { headerSizes, footerSizes } from '@/layout_and_navigation/sizes';
```

### Local Components (Tier 3)
```tsx
// No imports needed - use Tailwind directly
className="p-4 h-10 w-8 gap-6"
```

---

## 📏 SIZING STANDARDS

### Spacing Scale (Matches Tailwind)
```
1 = 4px   → p-1, m-1, gap-1
2 = 8px   → p-2, m-2, gap-2  
3 = 12px  → p-3, m-3, gap-3
4 = 16px  → p-4, m-4, gap-4
6 = 24px  → p-6, m-6, gap-6
8 = 32px  → p-8, m-8, gap-8
12 = 48px → p-12, m-12, gap-12
16 = 64px → p-16, m-16, gap-16
```

### Icon Sizes
```
sm = 16px → w-4 h-4
md = 20px → w-5 h-5
lg = 24px → w-6 h-6  
xl = 32px → w-8 h-8
```

### Interactive Elements
```
sm = 32px → h-8  (small buttons)
md = 36px → h-9  (default buttons)
lg = 40px → h-10 (large buttons)
```

---

## 🛠️ IMPLEMENTATION PATTERNS

### 1. Basic Components
```tsx
// ✅ CORRECT: Simple, readable
function Button({ size = 'md' }) {
    const sizeClasses = {
        sm: 'h-8 px-3 text-sm',
        md: 'h-9 px-4 text-base', 
        lg: 'h-10 px-6 text-lg'
    };
    
    return <button className={sizeClasses[size]}>...</button>;
}
```

### 2. Layout Components
```tsx
// ✅ CORRECT: Direct Tailwind classes
function Header() {
    return (
        <header className="h-16 px-4">
            <div className="flex items-center justify-between space-x-6">
                <Logo className="w-8 h-8" />
                <nav className="flex space-x-6">...</nav>
            </div>
        </header>
    );
}
```

### 3. Page Layouts
```tsx
// ✅ CORRECT: Standard page pattern
function HomePage() {
    return (
        <main className="container mx-auto px-4">
            <section className="py-16 lg:py-24">
                <div className="max-w-4xl mx-auto space-y-12">
                    <h1 className="text-4xl lg:text-6xl">...</h1>
                    <div className="grid gap-8 md:grid-cols-2">...</div>
                </div>
            </section>
        </main>
    );
}
```

### 4. Dynamic Sizing (When Needed)
```tsx
// ✅ CORRECT: Only when props determine size
function Icon({ size = 'md' }) {
    const sizeClass = getIconSize(size); // Returns w-[20px] h-[20px]
    return <svg className={sizeClass}>...</svg>;
}
```

---

## 📐 COMMON PATTERNS

### Page Structure
```tsx
<main className="container mx-auto px-4">
    <section className="py-16 lg:py-24">     // Hero section
        <div className="max-w-4xl mx-auto">  // Content width
            <h1 className="mb-6">...</h1>    // Standard spacing
            <p className="mb-12">...</p>     // Larger spacing
        </div>
    </section>
    
    <section className="py-12">             // Regular section
        <div className="grid gap-8 lg:grid-cols-3">
            // Cards with consistent gap
        </div>
    </section>
</main>
```

### Card Components
```tsx
<div className="p-6 rounded-lg border">     // Standard card
    <h3 className="mb-4">...</h3>          // Title spacing  
    <p className="mb-6">...</p>            // Content spacing
    <div className="flex gap-4">...</div>  // Action spacing
</div>
```

### Navigation
```tsx
// Header nav
<nav className="flex space-x-6">
    <a className="px-3 py-2">Link</a>
</nav>

// Footer nav  
<nav className="flex flex-wrap gap-6">
    <a className="hover:text-primary">Link</a>
</nav>
```

---

## ⚡ QUICK REFERENCE

### Most Common Classes
```css
/* Spacing */
p-4 px-4 py-4 m-4 mx-4 my-4
gap-4 space-x-4 space-y-4

/* Sizing */  
w-full h-full w-8 h-8 w-fit h-fit
max-w-lg max-w-4xl max-w-6xl

/* Layout */
container mx-auto px-4 py-16
flex grid items-center justify-between
```

### When to Use Helpers
```tsx
// Dynamic props
<Icon size={iconSize} />           // → getIconSize()
<Button size={buttonSize} />       // → size mapping object

// Computed values
style={{ margin: getSpacing(level) }}
```

---

## 🎯 AI AGENT CHECKLIST

When creating new components:

- [ ] **Start with Tailwind classes** - don't reach for helpers first
- [ ] **Use consistent spacing** - p-4, gap-6, space-x-3, etc.  
- [ ] **Follow page patterns** - container mx-auto px-4, py-16 lg:py-24
- [ ] **Size icons consistently** - w-6 h-6, w-8 h-8 for common sizes
- [ ] **Make buttons proper height** - h-8, h-9, h-10 for sm/md/lg
- [ ] **Use responsive patterns** - py-16 lg:py-24, gap-6 lg:gap-8
- [ ] **Only use helpers for dynamic sizing** - when size comes from props
- [ ] **Keep it simple** - readable classes are better than complex abstractions

---

## 🏆 SUCCESS EXAMPLES

### Good: Simple and Clear
```tsx
<div className="p-6 space-y-4">
    <h2 className="text-2xl font-bold">Title</h2>
    <div className="grid gap-6 md:grid-cols-2">
        {items.map(item => (
            <div key={item.id} className="p-4 border rounded-lg">
                {item.content}
            </div>
        ))}
    </div>
</div>
```

### Good: Dynamic When Needed
```tsx
function Avatar({ size = 'md' }) {
    const sizes = {
        sm: 'w-8 h-8',
        md: 'w-10 h-10', 
        lg: 'w-12 h-12'
    };
    
    return <img className={`rounded-full ${sizes[size]}`} />;
}
```

---

**Remember**: Keep it simple, readable, and maintainable. Tailwind classes are your first choice! 🚀
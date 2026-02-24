# SIZE & Z-INDEX SYSTEM GUIDE FOR AI AGENTS
**Complete Guide - Three-Tier Architecture**

---

## 🏗️ SYSTEM ARCHITECTURE

### Three-Tier Approach (Both Size & Z-Index)
```
TIER 1: Global Foundations    → src/design/size/* & src/design/zIndex.ts
TIER 2: Component Defaults    → Props with sensible defaults in components
TIER 3: Component Usage       → Direct Tailwind classes + prop overrides
```

**PHILOSOPHY**: Centralized constants + Tailwind classes + Flexible props

---

## 📏 SIZE SYSTEM

### File Structure
```
src/design/size/
├── sizes.ts        - Global foundations (spacing, component sizes, breakpoints)
├── layout.ts       - Specific values (LAYOUT, COMPONENT_SIZES constants)
└── sizeHelpers.ts  - Utility functions (only for dynamic sizing)
```

### Decision Tree: What to Use

**✅ PREFER: Tailwind Classes** (90% of cases)
```tsx
className="p-4 m-6 gap-8 h-10 w-6 max-w-lg"
```

**🔧 USE: COMPONENT_SIZES Constants** (when you need pixel values)
```tsx
import { COMPONENT_SIZES, LAYOUT } from '@/design';

// Modal widths
width: COMPONENT_SIZES.MODAL.WIDTHS.MD  // 600px

// Button heights
height: COMPONENT_SIZES.BUTTON.HEIGHT.MD  // 44px

// Layout dimensions
height: LAYOUT.HEADER_HEIGHT  // 64px
```

**🚫 AVOID: Over-engineering** (don't create helpers for static values)

### Key Constants

```typescript
// LAYOUT - Specific dimensions
LAYOUT.HEADER_HEIGHT = 64                    // Desktop header
LAYOUT.HEADER_HEIGHT_MOBILE = 56            // Mobile header
LAYOUT.SECTION_PADDING_Y = 64               // Section spacing
LAYOUT.MAX_CONTENT_WIDTH = 1280             // Content max width

// COMPONENT_SIZES - Component dimensions
COMPONENT_SIZES.BUTTON.HEIGHT = { SM: 36, MD: 44, LG: 56 }
COMPONENT_SIZES.INPUT.HEIGHT = { SM: 36, MD: 44, LG: 52 }
COMPONENT_SIZES.MODAL.WIDTHS = { SM: 400, MD: 600, LG: 800, XL: 1000 }
COMPONENT_SIZES.MODAL.MAX_WIDTH_PERCENT = 95
COMPONENT_SIZES.MODAL.MAX_HEIGHT_PERCENT = 90
COMPONENT_SIZES.BORDER_RADIUS = { SM: 4, MD: 8, LG: 16, XL: 24 }
```

---

## 🎚️ Z-INDEX SYSTEM

### File: `src/design/zIndex.ts`

**Six-Tier Scale** (Simplified & Clean)
```typescript
Z_INDEX = {
  BELOW: -1,           // Explicitly behind elements
  BASE: 0,             // Page containers, outer wrappers
  CONTENT: 1000,       // Main content, buttons, cards
  HEADER: 2000,        // Fixed header
  FLOATING: 3000,      // Dropdowns, tooltips, mobile menu
  MODAL: 4000,         // Modal dialogs
  SYSTEM: 5000,        // Toasts, critical system UI
}
```

### Decision Tree: Z-Index Values

**✅ CONTENT (1000)** - Default for most components
- Buttons, cards, sections, images, text overlays
- Use: `zIndex = Z_INDEX.CONTENT` (default prop value)
- Relative layering: `Z_INDEX.CONTENT + 10` for stacking within sections

**✅ HEADER (2000)** - Fixed navigation
- Main header, sticky navigation bars
- Use: `zIndex = Z_INDEX.HEADER`

**✅ FLOATING (3000)** - Above header, below modals
- Dropdowns, mobile menus, tooltips, badges, BackToTop button
- Use: `zIndex = Z_INDEX.FLOATING`

**✅ MODAL (4000)** - Modal dialogs
- All modal dialogs, overlays, confirmations
- Use: `zIndex = Z_INDEX.MODAL` (default for Modal component)
- Relative layering: `Z_INDEX.MODAL + 10` for nested modals

**✅ SYSTEM (5000)** - Critical system UI
- Toast notifications, loading screens, error boundaries
- Use: `zIndex = Z_INDEX.SYSTEM`

### Relative Layering Pattern
```typescript
// Within sections - use small offsets for stacking
<div style={{ zIndex: Z_INDEX.CONTENT + 10 }}>     // Content layer
  <div className="relative z-10">                   // Above background
    <div className="relative z-20">                 // Above sibling
    </div>
  </div>
</div>

// Within modals - stack modal elements
<Modal zIndex={Z_INDEX.MODAL}>
  <CloseButton style={{ zIndex: Z_INDEX.MODAL + 50 }} />  // Above modal content
  <LoadingOverlay style={{ zIndex: Z_INDEX.MODAL + 50 }} />
</Modal>
```

---

## 🎯 COMPONENT IMPLEMENTATION PATTERNS

### 1. Standard Component with Z-Index Prop

```tsx
import { Z_INDEX } from '@/design';

interface ButtonProps {
  children: ReactNode;
  size?: 'sm' | 'md' | 'lg';
  /**
   * Z-index for button layering
   * @default Z_INDEX.CONTENT (1000)
   */
  zIndex?: number;
}

export function Button({
  children,
  size = 'md',
  zIndex = Z_INDEX.CONTENT,
}: ButtonProps) {
  const sizeClasses = {
    sm: 'h-8 px-3 text-sm',      // 32px height
    md: 'h-9 px-4 text-base',    // 36px height
    lg: 'h-10 px-6 text-lg',     // 40px height
  };

  return (
    <button
      className={`rounded-lg transition ${sizeClasses[size]}`}
      style={{ zIndex }}
    >
      {children}
    </button>
  );
}
```

### 2. Modal with Size Constants

```tsx
import { Z_INDEX, COMPONENT_SIZES } from '@/design';

interface ModalProps {
  isOpen: boolean;
  onClose: () => void;
  size?: 'sm' | 'md' | 'lg' | 'xl';
  /**
   * Z-index for modal
   * @default Z_INDEX.MODAL (4000)
   */
  zIndex?: number;
}

export function Modal({
  isOpen,
  onClose,
  size = 'md',
  zIndex = Z_INDEX.MODAL,
}: ModalProps) {
  const sizeClasses = {
    sm: 'max-w-md',      // 448px (Tailwind)
    md: 'max-w-2xl',     // 672px (Tailwind)
    lg: 'max-w-4xl',     // 896px (Tailwind)
    xl: 'max-w-6xl',     // 1152px (Tailwind)
  };

  return (
    <div
      className="fixed inset-0"
      style={{ zIndex }}
    >
      {/* Backdrop - below modal */}
      <div
        className="absolute inset-0 bg-black/50"
        style={{ zIndex: zIndex - 10 }}
        onClick={onClose}
      />
      
      {/* Modal content */}
      <div className={`relative ${sizeClasses[size]}`}>
        {/* Close button - above modal content */}
        <button
          style={{ zIndex: zIndex + 50 }}
          onClick={onClose}
        >
          ✕
        </button>
        
        {/* Content */}
      </div>
    </div>
  );
}
```

### 3. Header Component

```tsx
import { Z_INDEX, LAYOUT } from '@/design';

export function Header() {
  return (
    <header
      className="fixed top-0 left-0 right-0"
      style={{
        zIndex: Z_INDEX.HEADER,
        height: LAYOUT.HEADER_HEIGHT,
      }}
    >
      <nav className="container mx-auto px-4 flex items-center justify-between h-full">
        <Logo className="w-10 h-10" />
        <div className="flex gap-6">
          {/* Nav items */}
        </div>
      </nav>
    </header>
  );
}
```

### 4. Dropdown/Floating Menu

```tsx
import { Z_INDEX } from '@/design';

export function Dropdown({ isOpen }: { isOpen: boolean }) {
  return isOpen && (
    <div
      className="absolute right-0 mt-2 w-56 rounded-lg shadow-xl border bg-white"
      style={{ zIndex: Z_INDEX.FLOATING }}
    >
      {/* Dropdown content */}
    </div>
  );
}
```

### 5. Toast Provider

```tsx
import { Z_INDEX } from '@/design';

export function ToastProvider({ children }: { children: ReactNode }) {
  return (
    <>
      {children}
      {createPortal(
        <div
          className="fixed top-0 right-0 p-4 flex flex-col gap-3"
          style={{ zIndex: Z_INDEX.SYSTEM }}
        >
          {/* Toast notifications */}
        </div>,
        document.body
      )}
    </>
  );
}
```

---

## 📋 IMPLEMENTATION CHECKLIST

### For Every New Component:

**Size Implementation:**
- [ ] Use Tailwind classes for static sizing (`h-8`, `p-4`, `gap-6`)
- [ ] Create size mapping object for dynamic sizing (sm/md/lg props)
- [ ] Use COMPONENT_SIZES constants only when pixel values needed
- [ ] Follow responsive patterns (`py-16 lg:py-24`)

**Z-Index Implementation:**
- [ ] Import Z_INDEX from `@/design`
- [ ] Add `zIndex?: number` prop to interface
- [ ] Set appropriate default value in props destructuring
- [ ] Apply via `style={{ zIndex }}` (NOT className)
- [ ] Add JSDoc comment explaining default value

**Default Values by Component Type:**
- [ ] Button/Card/Content → `zIndex = Z_INDEX.CONTENT`
- [ ] Modal/Dialog → `zIndex = Z_INDEX.MODAL`
- [ ] Header/Nav → `zIndex = Z_INDEX.HEADER`
- [ ] Dropdown/Menu/Tooltip → `zIndex = Z_INDEX.FLOATING`
- [ ] Toast/System → `zIndex = Z_INDEX.SYSTEM`

---

## 🎓 COMMON PATTERNS

### Page Layout
```tsx
<main className="container mx-auto px-4">
  <section className="py-16 lg:py-24">
    <div className="max-w-4xl mx-auto space-y-12">
      <h1 className="text-4xl lg:text-6xl">Title</h1>
      <div className="grid gap-8 md:grid-cols-2">
        {/* Content */}
      </div>
    </div>
  </section>
</main>
```

### Card with Relative Layering
```tsx
<div className="relative p-6 rounded-lg border">
  {/* Background decoration */}
  <div className="absolute inset-0 opacity-10 -z-10">
    {/* Pattern */}
  </div>
  
  {/* Content layer */}
  <div className="relative z-10">
    <h3>Title</h3>
    <p>Content</p>
  </div>
</div>
```

### Section with Background & Content
```tsx
<section
  className="relative py-16"
  style={{ zIndex: Z_INDEX.CONTENT }}
>
  {/* Background elements */}
  <div className="absolute inset-0 opacity-20">
    {/* Pattern/gradient */}
  </div>
  
  {/* Content above background */}
  <div className="relative z-10 max-w-7xl mx-auto px-4">
    {/* Section content */}
  </div>
</section>
```

---

## ⚠️ CRITICAL RULES

### Z-Index Rules:
1. **NEVER** use hardcoded values (`z-50`, `z-40`, etc.)
2. **ALWAYS** use Z_INDEX constants from `@/design`
3. **ALWAYS** apply via `style={{ zIndex }}`, NOT className
4. Use **relative z-10/z-20** only for internal component layering
5. For relative offsets, use: `Z_INDEX.CONTENT + 10` (not arbitrary values)

### Size Rules:
1. **PREFER** Tailwind classes over custom values
2. **USE** COMPONENT_SIZES constants when pixel values needed
3. **AVOID** creating helpers for static values
4. **KEEP** component size mappings simple (sm/md/lg objects)

---

## 🚀 QUICK REFERENCE

### Imports
```typescript
import { Z_INDEX, LAYOUT, COMPONENT_SIZES } from '@/design';
```

### Default Z-Index Values
```typescript
Button → Z_INDEX.CONTENT (1000)
Modal → Z_INDEX.MODAL (4000)
Header → Z_INDEX.HEADER (2000)
Dropdown → Z_INDEX.FLOATING (3000)
Toast → Z_INDEX.SYSTEM (5000)
```

### Common Size Classes
```css
/* Heights */
h-8 h-9 h-10           /* Buttons */
h-16                    /* Header */

/* Spacing */
p-4 p-6 gap-4 gap-6    /* Padding/gaps */
py-16 lg:py-24         /* Section padding */

/* Widths */
max-w-md max-w-2xl max-w-4xl max-w-6xl  /* Modals */
max-w-4xl              /* Content */
```

### Helper Functions (Rare Use)
```typescript
// Only for truly dynamic sizing
getSpacing(key: SpacingKey): string
getIconSize(size: IconSizeKey): string
getRelativeZIndex(baseKey: ZIndexKey, offset: number): number
```

---

## ✅ SUCCESS EXAMPLES

### Example 1: Button Component
```tsx
import { Z_INDEX } from '@/design';

interface ButtonProps {
  size?: 'sm' | 'md' | 'lg';
  zIndex?: number;
}

export function Button({ size = 'md', zIndex = Z_INDEX.CONTENT }: ButtonProps) {
  const sizeClasses = {
    sm: 'h-8 px-3 text-sm',
    md: 'h-9 px-4 text-base',
    lg: 'h-10 px-6 text-lg',
  };

  return (
    <button
      className={`rounded-lg ${sizeClasses[size]}`}
      style={{ zIndex }}
    >
      Click me
    </button>
  );
}
```

### Example 2: FloatingBadge Component
```tsx
import { Z_INDEX } from '@/design';

interface FloatingBadgeProps {
  zIndex?: number;
}

export function FloatingBadge({ zIndex = Z_INDEX.FLOATING }: FloatingBadgeProps) {
  return (
    <div
      className="absolute top-4 right-4"
      style={{ zIndex }}
    >
      <div className="p-3 rounded-full bg-gold shadow-lg">
        {/* Badge content with internal layering */}
        <div className="relative z-10">Coming Soon</div>
        <div className="absolute inset-0 -z-10 blur-xl bg-black/20" />
      </div>
    </div>
  );
}
```

---

**Remember**: Clean constants + Tailwind classes + Flexible props = Maintainable code! 🎯

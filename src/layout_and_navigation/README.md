# Layout and Navigation System

A comprehensive, production-ready navigation and layout system for React applications with TypeScript support.

## ✨ Features

- 🧭 **Smart Navigation** with history tracking and UNDO/REDO
- 📍 **Scroll Restoration** - Automatically saves and restores scroll positions
- 📊 **Hierarchical Sticky Positioning** - Multi-level sticky elements with auto-measurement
- 📱 **Adaptive Responsive** - Automatically adapts navigation based on available space
- 🌐 **i18n Ready** - Built-in internationalization support
- 🎯 **Type-Safe** - Comprehensive TypeScript definitions
- 🔄 **Breadcrumbs** - Auto-generated from navigation hierarchy
- 📦 **Modular** - Clean separation between navigation and layout concerns

## 📁 Project Structure

```
src/layout_and_navigation/
│
├── core/                          ← Core functionality (NEW!)
│   ├── navigation/                ← Navigation concerns (routing, history, breadcrumbs)
│   ├── layout/                    ← Layout concerns (positioning, responsive, sticky)
│   └── shared/                    ← Shared types and utilities
│
├── docs/                          ← Documentation
│   ├── ARCHITECTURE.md            ← System architecture overview
│   ├── NAVIGATION_GUIDE.md        ← Using navigation features
│   ├── LAYOUT_GUIDE.md            ← Layout system usage  
│   ├── STICKY_GUIDE.md            ← Sticky positioning guide
│   └── FLUTTER_PATTERNS.md        ← Flutter example analysis
│
├── examples/                      ← Usage examples
│   ├── BasicNavigation.example.tsx
│   ├── StickyHeaders.example.tsx
│   └── ResponsiveLayout.example.tsx
│
├── components/                    ← LEGACY (being reorganized)
├── hooks/                         ← LEGACY (being reorganized)
├── config/                        ← LEGACY (being reorganized)
├── utils/                         ← LEGACY (being reorganized)
│
├── flutter_example/               ← Reference implementation
├── sticky/                        ← MOVING TO: core/layout/sticky/
│
├── FOLDER_STATUS.md               ← **READ THIS FIRST!**
├── README.md                      ← This file
└── index.ts                       ← Main public API
```

## 🚀 Quick Start

### 1. Set Up Providers

```tsx
// App.tsx
import { BrowserRouter } from 'react-router-dom';
import { NavigationProvider } from '@/layout_and_navigation';

function App() {
  return (
    <BrowserRouter>
      <NavigationProvider>
        <YourAppContent />
      </NavigationProvider>
    </BrowserRouter>
  );
}
```

### 2. Use Main Layout

```tsx
// pages/MyPage.tsx
import { MainLayout } from '@/layout_and_navigation';

export function MyPage() {
  return (
    <MainLayout showBreadcrumbs>
      <div className="container">
        <h1>My Page Content</h1>
      </div>
    </MainLayout>
  );
}
```

### 3. Programmatic Navigation

```tsx
import { useNavigation } from '@/layout_and_navigation';

function MyComponent() {
  const { goTo, goBack, canGoBack } = useNavigation();
  
  return (
    <div>
      <button onClick={() => goTo('/products')}>
        View Products
      </button>
      {canGoBack && (
        <button onClick={goBack}>
          Go Back
        </button>
      )}
    </div>
  );
}
```

### 4. Sticky Elements

```tsx
import { useSticky } from '@/layout_and_navigation/sticky';

function MyHeader() {
  const { ref, stickyClasses, stickyStyles } = useSticky('my-header', {
    enableShrink: true,
    autoMeasure: true
  });
  
  return (
    <header ref={ref} className={stickyClasses} style={stickyStyles}>
      <h1>My Header</h1>
    </header>
  );
}
```

## 📖 Key Concepts

### Navigation vs Layout

This system clearly separates two distinct concerns:

**Navigation** (`core/navigation/`)
- **What:** Where to go, how to get there, where you've been
- **Includes:** Routing, history, breadcrumbs, path matching
- **Use when:** Adding routes, tracking history, building breadcrumb trails

**Layout** (`core/layout/`)
- **What:** How to display UI, where to position elements
- **Includes:** Sticky positioning, responsive behavior, scroll handling
- **Use when:** Positioning headers, handling screen sizes, scroll effects

### UNDO/REDO Navigation

The system automatically tracks your navigation history:

```typescript
// Navigation history is automatic!
const { canGoBack, goBack, previousPath } = useNavigation();

// Scroll positions are automatically saved and restored
// when using browser back/forward buttons
```

**How it works:**
1. Every navigation (PUSH) adds to history stack
2. Scroll position saved before navigation
3. Browser back (POP) restores scroll position
4. Maximum 50 entries to prevent memory issues

### Hierarchical Sticky Positioning

Create multi-level sticky elements that stack correctly:

```typescript
// In stickyConfig.ts
export const STICKY_LAYERS = [
  {
    id: 'header',
    height: 80,
    parent: null,  // Root level
  },
  {
    id: 'sub-nav',
    height: 60,
    parent: 'header',  // Sticks below header
  }
];
```

**Benefits:**
- Auto-calculates offsets based on hierarchy
- Handles z-index automatically
- Supports conditional activation
- Scroll-based sticky (becomes sticky when scrolled past)
- Auto-measurement with ResizeObserver

### Adaptive Navigation

Navigation automatically adapts to available space:

**Modes:**
- **Tabs** - All items visible as tabs
- **Partial** - Some tabs + "More" dropdown
- **Hamburger** - All items in mobile menu

The system measures available width and switches modes automatically.

## 📚 Documentation

### Essential Reading

1. **[FOLDER_STATUS.md](./FOLDER_STATUS.md)** - Comprehensive folder analysis, current status, and reorganization plan
2. **Architecture** - System overview and data flow (coming soon in docs/)
3. **Navigation Guide** - Using navigation features (coming soon)
4. **Layout Guide** - Layout system usage (coming soon)
5. **Sticky Guide** - Sticky positioning guide (coming soon)

### Current Documentation

- `PROJECT_GUIDES/STICKY_QUICK_REFERENCE.md` - Quick sticky system reference
- `PROJECT_GUIDES/GUIDE_STICKY_SIMPLIFIED_API.md` - Detailed sticky API guide
- `flutter_example/` - Reference implementation showing better patterns

## 🏗️ Current Status: **In Reorganization**

**⚠️ Important:** This folder is currently being reorganized to better separate concerns.

**What's changing:**
- Moving from flat structure to modular `core/` structure
- Separating navigation concerns from layout concerns
- Extracting history management into dedicated module
- Better aligned with Flutter example patterns

**What's stable:**
- All current functionality works
- Public API remains backward compatible
- No breaking changes to existing code

**Read:** [FOLDER_STATUS.md](./FOLDER_STATUS.md) for full details on the reorganization.

## 🎯 Design Principles

### 1. Separation of Concerns
- Navigation = routing, history, where to go
- Layout = positioning, responsive, how to display
- Clear boundaries between modules

### 2. Single Responsibility
- Each module has one clear purpose
- Components do one thing well
- Easy to understand and test

### 3. Configuration Over Code
- `navigationConfig` drives everything
- `STICKY_LAYERS` defines hierarchy
- Add features by configuration, not code

### 4. Type Safety First
- Comprehensive TypeScript throughout
- No `any` types
- Interfaces for all public APIs

### 5. Developer Experience
- Self-documenting code
- Helpful error messages
- Clear examples
- Progressive complexity (simple → advanced)

## 📦 Module Overview

### Core Navigation (`core/navigation/`)

**Responsibilities:**
- Route configuration and generation
- Navigation history with UNDO/REDO
- Scroll position restoration
- Breadcrumb trail generation
- Path matching utilities

**Key files:**
- `routing/routeConfig.ts` - Navigation structure
- `history/historyManager.ts` - UNDO/REDO logic
- `hooks/useNavigation.ts` - Main navigation hook
- `components/Breadcrumbs.tsx` - Breadcrumb UI

### Core Layout (`core/layout/`)

**Responsibilities:**
- Component positioning (sticky, fixed)
- Responsive UI adaptation
- Scroll behavior and locking
- Layout wrappers and primitives

**Key subdirectories:**
- `sticky/` - Complete sticky positioning system
- `adaptive/` - Responsive navigation
- `scroll/` - Scroll tracking and locking
- `components/` - Layout UI components

### Sticky System (`core/layout/sticky/`)

**Complete subsystem for sticky positioning:**
- Config-driven hierarchy
- Auto-measurement
- Scroll-based activation
- Hysteresis for stability
- Conditional layers

**Key files:**
- `config/stickyConfig.ts` - Layer definitions
- `hooks/useSticky.ts` - Simplified API (ONE hook!)
- `contexts/StickyContext.tsx` - Global state
- `components/DemoSticky.tsx` - Test suite

## 🔧 Configuration

### Navigation Config

```typescript
// core/navigation/routing/routeConfig.ts
export const navigationConfig: NavItem[] = [
  {
    id: 'home',
    label: { pt: 'Início', en: 'Home' },
    path: '/',
    component: HomePage,
    styleLevel: 'top',
    metadata: {
      showInNav: true,
      showInBreadcrumb: true,
      order: 1,
    }
  }
];
```

### Sticky Config

```typescript
// core/layout/sticky/config/stickyConfig.ts
export const STICKY_LAYERS: StickyLayerConfig[] = [
  {
    id: 'header',
    label: 'Main Header',
    height: 80,
    zIndex: Z_INDEX.STICKY - 10,
    parent: null,
    backgroundColor: 'bg-white dark:bg-gray-900',
    showSeparator: true,
  }
];
```

## 🧪 Testing

**Test structure:**
```
core/
├── navigation/__tests__/
│   ├── useNavigation.test.ts
│   └── historyManager.test.ts
└── layout/__tests__/
    ├── useSticky.test.ts
    └── adaptiveNavigation.test.ts
```

*Note: Test files coming soon as part of reorganization*

## 🚦 Migration Status

**Phase 1: Preparation** ✅ DONE
- Created new folder structure
- Documented current state
- Created migration plan

**Phase 2: Move Files** 🔄 IN PROGRESS
- Moving files to new locations
- Updating internal imports
- Maintaining backward compatibility

**Phase 3: Split Large Files** ⏳ NEXT
- Extract history from NavigationContext
- Split types.ts by module
- Separate concerns in helpers

**Phase 4: Update Project** ⏳ FUTURE
- Update all imports
- Remove deprecations
- Final testing

## 🤝 Contributing

### Adding a New Page

1. Import component (lazy loaded)
2. Add to `navigationConfig` array
3. That's it! Routes, nav, breadcrumbs auto-update

### Adding a Sticky Layer

1. Add entry to `STICKY_LAYERS` in `stickyConfig.ts`
2. Set parent relationship
3. Use `useSticky` hook in component
4. Done!

### Best Practices

- Keep navigation concerns separate from layout
- Use configuration over code
- Document with JSDoc comments
- Add usage examples
- Write tests for new features

## 📄 License

Part of TileStories project - Master's thesis

## 🙏 Acknowledgments

- **Flutter Example** - Inspired better separation patterns
- **React Router** - Foundation for routing
- **TypeScript** - Type safety throughout

## 📞 Support

For questions or issues:
1. Check [FOLDER_STATUS.md](./FOLDER_STATUS.md) for detailed analysis
2. Review documentation in `docs/` folder
3. Check examples in `examples/` folder
4. Review Flutter example for patterns

---

**Last Updated:** February 24, 2026  
**Status:** Active Development (Reorganization Phase)  
**Stability:** Production-ready (functionality stable, structure improving)

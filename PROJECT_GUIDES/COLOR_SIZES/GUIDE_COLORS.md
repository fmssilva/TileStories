# COLORS GUIDE - AI AGENT REFERENCE

> **Quick Reference**: How to handle colors when creating features, modifying components, or implementing designs.

## 🎯 THREE-TIER SYSTEM OVERVIEW

```
🌍 TIER 1 (Global)    → Foundation colors, brand identity, semantic meanings
    » see the image example of the home page colors to be used: C:\Users\franc\Desktop\TileStories\PROJECT_GUIDES\COLOR_SIZES\SITE_COLORS.png

    and so lets make that style the main theme across our app... and so confirm we have the good colors bellow to use
🏢 TIER 2 (Domain)    → Feature-specific colors (header, pokemon, etc.)
🎯 TIER 3 (Local)     → Component-specific states and variations
```

---

## 🚀 QUICK START - DECISION TREE

### 🤔 **What are you implementing?**

#### 📱 **Standard UI Elements** (buttons, inputs, cards, text)
```typescript
// ✅ USE: Theme colors from global system
import { getThemeColors } from '@/design/colors';

const colors = getThemeColors(theme);
// colors.primary, colors.text, colors.border, colors.background, etc.
```

#### 🏢 **Feature-Specific Elements** (header, pokemon cards, user profiles)
```typescript
// ✅ USE: Domain color system  
import { getHeaderStyles } from '@/layout_and_navigation/header/colors';
import { getPokemonCardStyles } from '@/domains/pokemons/colors';

const headerStyles = getHeaderStyles(theme, isScrolled);
const pokemonStyles = getPokemonCardStyles(theme);
```

#### 🎨 **One-off Styling** (special states, unique components)
```typescript
// ✅ USE: Local colors with global foundation
import { globalColors } from '@/design/colors';

const urgentColor = globalColors.semantic.error; // Use semantic meanings
const successColor = globalColors.semantic.success;
```

#### 🎨 **CSS Classes** (for Tailwind-based components)
```typescript
// ✅ USE: Theme-aware CSS classes
import { themeClasses } from '@/design/themeClasses';

// Ready-to-use class combinations
className={themeClasses.surface}          // "bg-white dark:bg-slate-800"
className={themeClasses.text}             // "text-gray-900 dark:text-gray-50"
className={themeClasses.border}           // "border-gray-200 dark:border-slate-700"

// Interactive elements
className={themeClasses.interactive.base} // Button/clickable surfaces
className={themeClasses.interactive.input} // Input fields
```

---

## 📁 FILE STRUCTURE & IMPORTS

### 🌍 **Global Colors** (`src/design/colors.ts`)
```typescript
// WHAT'S AVAILABLE:
export const globalColors = {
    brand: { 500: '#14b8a6' },      // Primary teal (logo color)
    gray: { 500: '#64748b' },       // Neutral grays
    semantic: {
        success: '#22c55e',          // Status colors
        error: '#ef4444',
        warning: '#f59e0b',
        info: '#3b82f6'
    }
};

export const themeColors = {
    light: { primary: '#14b8a6', text: '#0f172a' },
    dark: { primary: '#2dd4bf', text: '#f8fafc' }
};

// UTILITY FUNCTIONS:
export function getThemeColors(theme: 'light' | 'dark')
export function getBrandColor(shade: 50-950)
export function getBrandGradient(from: shade, to: shade)
```

### 🏢 **Domain Colors**

#### Header Domain (`src/layout_and_navigation/header/colors.ts`)
```typescript
export const headerColors = {
    gradients: {
        main: 'linear-gradient(...)',         // Header background
        scrolled: (opacity) => 'gradient...' // Scrolled state
    },
    primary: globalColors.brand              // Brand colors reference
};

export function getHeaderStyles(theme, isScrolled)
export function getFooterStyles(theme)
export function getNavLinkStyles(theme, isActive)
```

#### Pokemon Domain (`src/domains/pokemons/colors.ts`)
```typescript
export const pokemonColors = {
    primary: '#3b82f6',                      // Pokemon blue (not global teal)
    types: { fire: '#F08030', water: '#6890F0' }
};

export const pokemonTheme = {
    headerGradient: { css: 'gradient...' },
    button: { primary: pokemonColors.primary },
    text: { primary: '#ffffff' }
};

export function getPokemonCardStyles(theme)
export function getPokemonTypeColor(type)
```

---

## ⚡ COMMON PATTERNS

### 🎨 **Pattern 0: Tailwind Classes (NEW - Recommended for most components)**
```typescript
import { themeClasses } from '@/design/themeClasses';

const TableRow = () => {
    return (
        <tr className={`${themeClasses.surface} ${themeClasses.border} hover:${themeClasses.backgroundSoft}`}>
            <td className={`${themeClasses.backgroundSoft} ${themeClasses.text}`}>
                Content
            </td>
        </tr>
    );
};
```

### 🎨 **Pattern 1: Theme-Adaptive Component**
```typescript
import { getThemeColors } from '@/design/colors';

const MyComponent = () => {
    const { theme } = useTheme();
    const colors = getThemeColors(theme);

    return (
        <div style={{
            backgroundColor: colors.background,
            color: colors.text,
            borderColor: colors.border
        }}>
    );
};
```

### 🏢 **Pattern 2: Domain-Specific Feature**
```typescript
import { getHeaderStyles } from '@/layout_and_navigation/header/colors';

const Header = () => {
    const { theme } = useTheme();
    const [isScrolled, setIsScrolled] = useState(false);
    
    const headerStyles = getHeaderStyles(theme, isScrolled);

    return (
        <header style={{
            background: headerStyles.background,
            color: headerStyles.textColor
        }}>
    );
};
```

### 🎯 **Pattern 3: Component with Status States**
```typescript
import { globalColors, getThemeColors } from '@/design/colors';

const StatusBadge = ({ status }) => {
    const colors = getThemeColors(theme);
    
    const statusColor = {
        success: globalColors.semantic.success,
        error: globalColors.semantic.error,
        warning: globalColors.semantic.warning,
        default: colors.text
    }[status] || colors.text;

    return <span style={{ color: statusColor }}>
};
```

---

## 🔧 IMPLEMENTATION RULES

### ✅ **DO**

```typescript
✅ Import appropriate tier for your use case
✅ Use getThemeColors() for standard UI elements  
✅ Use domain colors for feature-specific elements
✅ Test in both light and dark themes
✅ Use semantic color meanings (success, error, warning)
✅ Follow TypeScript types for color shades (50, 100, 200...950)
```

### ❌ **DON'T**

```typescript
❌ Hardcode hex colors: style={{ color: '#374151' }}
❌ Skip theme system: use globalColors.brand[500] directly in components
❌ Mix domain colors: don't use pokemonColors in header components
❌ Create new color constants: const BLUE = '#3b82f6'
❌ Ignore dark mode: always test both themes
❌ Break the hierarchy: components should use theme colors, not global directly
```

---

## 🛠️ CREATING NEW FEATURES

### 🆕 **Adding a Standard Component**
```typescript
// 1. Import theme colors
import { getThemeColors } from '@/design/colors';

// 2. Get theme-appropriate colors
const colors = getThemeColors(theme);

// 3. Use semantic properties
backgroundColor: colors.surface,    // Not colors.gray[100]
color: colors.text,                // Not colors.black  
borderColor: colors.border         // Not colors.gray[200]
```

### 🏢 **Creating a New Domain**
```typescript
// 1. Create domain color file: src/domains/admin/colors.ts
import { globalColors, getThemeColors, type Theme } from '@/design/colors';

export const adminColors = {
    primary: '#f97316',              // Domain-specific orange theme
    secondary: '#84cc16'             // Supporting color
};

export function getAdminStyles(theme: Theme) {
    const colors = getThemeColors(theme);
    return {
        background: colors.surface,   // Use global theme
        primary: adminColors.primary  // Add domain flavor
    };
}

// 2. Export from domain index file
// 3. Use in domain components
```

### 🎨 **Handling Special Cases**
```typescript
// One-off colors with semantic foundation
const urgencyColors = {
    low: globalColors.semantic.info,      // Blue
    medium: globalColors.semantic.warning, // Orange  
    high: globalColors.semantic.error     // Red
};

// Dynamic color with theme awareness
const getDynamicColor = (value, theme) => {
    const colors = getThemeColors(theme);
    return value > 80 ? colors.success : colors.error;
};
```

---

## 🔍 QUICK TROUBLESHOOTING

### ❓ **"What color should I use for...?"**

- **Background**: `colors.background` or `colors.surface`
- **Text**: `colors.text` or `colors.textMuted`  
- **Borders**: `colors.border` or `colors.borderSoft`
- **Primary actions**: `colors.primary`
- **Success states**: `globalColors.semantic.success`
- **Errors**: `globalColors.semantic.error`

### ❓ **"Should I create domain colors?"**

**YES** if:
- Multiple components share the same color scheme  
- Colors are feature-specific (not general UI)
- Feature has unique visual identity

**NO** if:
- Only one component needs it
- Standard theme colors work fine
- It's a simple one-off variation

### ❓ **"Component not updating with theme?"**

Check if you're:
- Using `getThemeColors(theme)` 
- Passing theme prop correctly
- Using theme colors (not global colors directly)

---

## 📋 QUICK REFERENCE

### **Imports**
```typescript
// Global system (most common)
import { getThemeColors, globalColors } from '@/design/colors';

// Header domain  
import { getHeaderStyles } from '@/layout_and_navigation/header/colors';

// Pokemon domain
import { getPokemonCardStyles } from '@/domains/pokemons/colors';
```

### **Color Properties**
```typescript
// Theme colors (use these most often)
colors.background, colors.surface, colors.text, colors.textMuted
colors.border, colors.primary, colors.success, colors.error

// Global semantic (for status/feedback)
globalColors.semantic.success, globalColors.semantic.error

// Brand colors (when you need exact shades)  
globalColors.brand[500] // Primary teal
globalColors.gray[500]  // Neutral gray
```

---

**Remember**: Start with theme colors, use domain colors for features, create local colors sparingly. When in doubt, use the theme system! 🎨
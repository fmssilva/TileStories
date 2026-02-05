/**
 * Theme CSS Classes Utilities
 * ============================
 * 
 * Provides ready-to-use CSS class strings for common theme patterns.
 * This makes it easy to replace hardcoded `dark:` classes with centralized theme management.
 * 
 * Instead of: `"bg-white dark:bg-gray-800"`
 * Use: `themeClasses.surface`
 */

/**
 * Standard theme class combinations for common UI elements
 */
export const themeClasses = {
    // Backgrounds
    background: 'bg-white dark:bg-slate-950',
    backgroundSoft: 'bg-azulejo-ivory-300 dark:bg-slate-900',
    surface: 'bg-white dark:bg-slate-800',

    // Text colors  
    text: 'text-gray-900 dark:text-gray-50',
    textSoft: 'text-gray-700 dark:text-gray-200',
    textMuted: 'text-gray-600 dark:text-gray-400',

    // Borders
    border: 'border-gray-200 dark:border-slate-700',
    borderSoft: 'border-gray-100 dark:border-slate-600',

    // Interactive elements
    interactive: {
        // Button/clickable surfaces
        base: 'bg-gray-100 dark:bg-gray-700 hover:bg-gray-200 dark:hover:bg-gray-600',
        text: 'text-gray-700 dark:text-gray-300',

        // Input fields
        input: 'bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-100 border-gray-300 dark:border-gray-600',

        // Dropdown/select options
        option: 'hover:bg-azulejo-blue-50 dark:hover:bg-gray-600 text-gray-900 dark:text-gray-100',
    },

    // Brand-specific - Using azulejo colors
    primary: 'bg-azulejo-blue-500 text-white',
    primarySoft: 'bg-azulejo-blue-50 dark:bg-azulejo-blue-900/30 text-azulejo-blue-800 dark:text-azulejo-blue-200',
} as const;

/**
 * Creates dynamic theme classes based on theme mode
 * Use when you need programmatic class generation
 */
export function createThemeClasses(theme: 'light' | 'dark') {
    const isDark = theme === 'dark';

    return {
        background: isDark ? 'bg-slate-950' : 'bg-white',
        backgroundSoft: isDark ? 'bg-slate-900' : 'bg-gray-50',
        surface: isDark ? 'bg-slate-800' : 'bg-white',

        text: isDark ? 'text-gray-50' : 'text-gray-900',
        textSoft: isDark ? 'text-gray-200' : 'text-gray-700',
        textMuted: isDark ? 'text-gray-400' : 'text-gray-600',

        border: isDark ? 'border-slate-700' : 'border-gray-200',
        borderSoft: isDark ? 'border-slate-600' : 'border-gray-100',
    };
}

/**
 * Hook for getting theme-aware CSS classes
 */
export function useThemeClasses() {
    // Note: This would need to import useTheme from the theme domain
    // For now, providing static classes that work with Tailwind's dark: modifier
    return themeClasses;
}

// Common class combinations for specific components
export const componentThemeClasses = {
    // Modal/overlay components
    modal: {
        backdrop: 'bg-black/50',
        content: themeClasses.surface,
        header: themeClasses.backgroundSoft,
    },

    // Card components
    card: {
        base: `${themeClasses.surface} ${themeClasses.border} shadow-lg`,
        header: themeClasses.backgroundSoft,
    },

    // Form components
    form: {
        input: `${themeClasses.interactive.input} focus:ring-2 focus:ring-azulejo-blue-500`,
        label: themeClasses.textSoft,
    },

    // Table components
    table: {
        header: themeClasses.backgroundSoft,
        row: `${themeClasses.surface} hover:${themeClasses.backgroundSoft.replace('bg-', 'bg-')}`,
        cell: themeClasses.border,
    },
} as const;
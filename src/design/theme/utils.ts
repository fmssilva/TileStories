/**
 * THEME UTILITIES
 * ===============
 * 
 * Utility functions for theme management, detection, and manipulation.
 * Simplified: detects system preference on init, then just toggles light/dark.
 */

import { ThemeMode } from './types';

/**
 * Get the system's preferred theme
 * @returns 'light' | 'dark'
 */
export function getSystemTheme(): 'light' | 'dark' {
    if (typeof window === 'undefined') return 'light';

    return window.matchMedia('(prefers-color-scheme: dark)').matches
        ? 'dark'
        : 'light';
}

/**
 * Resolve theme mode to actual theme
 * @param mode - The theme mode ('light' or 'dark')
 * @returns The resolved theme ('light' | 'dark')
 */
export function resolveTheme(mode: ThemeMode): 'light' | 'dark' {
    return mode; // Direct return since we removed 'system' mode
}

/**
 * Apply theme to the document
 * @param theme - The theme to apply ('light' | 'dark')
 */
export function applyTheme(theme: 'light' | 'dark'): void {
    if (typeof document === 'undefined') return;

    const root = document.documentElement;

    // Remove existing theme classes
    root.classList.remove('light', 'dark');

    // Add the new theme class
    root.classList.add(theme);
}

/**
 * Toggle between light and dark themes
 * @param currentMode - The current theme mode
 * @returns The opposite theme mode
 */
export function getNextTheme(currentMode: ThemeMode): ThemeMode {
    return currentMode === 'light' ? 'dark' : 'light';
}

/**
 * Save theme to localStorage
 * @param mode - The theme mode to save
 */
export function saveTheme(mode: ThemeMode): void {
    if (typeof window === 'undefined') return;

    try {
        localStorage.setItem('app-theme', mode);
    } catch (error) {
        console.warn('Failed to save theme to localStorage:', error);
    }
}

/**
 * Load theme from localStorage or detect from system
 * @returns The saved theme mode or system-detected theme
 */
export function loadTheme(): ThemeMode {
    if (typeof window === 'undefined') return 'light';

    try {
        const saved = localStorage.getItem('app-theme') as ThemeMode;

        // If we have a saved preference, use it
        if (saved && ['light', 'dark'].includes(saved)) {
            return saved;
        }

        // Otherwise, detect from system and save it
        const systemTheme = getSystemTheme();
        saveTheme(systemTheme);
        return systemTheme;
    } catch (error) {
        console.warn('Failed to load theme from localStorage:', error);
        return getSystemTheme();
    }
}

/**
 * Set up system theme change listener
 * @param callback - Function to call when system theme changes
 * @returns Cleanup function to remove the listener
 */
export function setupSystemThemeListener(callback: () => void): () => void {
    if (typeof window === 'undefined') return () => { };

    const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');

    // Modern browsers
    if (mediaQuery.addEventListener) {
        mediaQuery.addEventListener('change', callback);
        return () => mediaQuery.removeEventListener('change', callback);
    }

    // Legacy browsers (IE/older Safari)
    if (mediaQuery.addListener) {
        mediaQuery.addListener(callback);
        return () => mediaQuery.removeListener?.(callback);
    }

    // Fallback
    return () => { };
}
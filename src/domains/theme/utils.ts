/**
 * THEME UTILITIES
 * ===============
 * 
 * Utility functions for theme management, detection, and manipulation.
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
 * @param mode - The theme mode ('light', 'dark', 'system')
 * @returns The resolved theme ('light' | 'dark')
 */
export function resolveTheme(mode: ThemeMode): 'light' | 'dark' {
    if (mode === 'system') {
        return getSystemTheme();
    }
    return mode;
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
 * Get the next theme in the cycle
 * @param currentMode - The current theme mode
 * @returns The next theme mode in the cycle
 */
export function getNextTheme(currentMode: ThemeMode): ThemeMode {
    const modes: ThemeMode[] = ['light', 'dark', 'system'];
    const currentIndex = modes.indexOf(currentMode);
    const nextIndex = (currentIndex + 1) % modes.length;
    return modes[nextIndex] as ThemeMode;
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
 * Load theme from localStorage
 * @returns The saved theme mode or 'system' as default
 */
export function loadTheme(): ThemeMode {
    if (typeof window === 'undefined') return 'system';

    try {
        const saved = localStorage.getItem('app-theme') as ThemeMode;
        return saved && ['light', 'dark', 'system'].includes(saved) ? saved : 'system';
    } catch (error) {
        console.warn('Failed to load theme from localStorage:', error);
        return 'system';
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
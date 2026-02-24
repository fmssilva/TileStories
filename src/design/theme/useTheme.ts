/**
 * THEME HOOK - useTheme
 * =====================
 * 
 * React hook for managing theme state and providing theme switching functionality.
 * Simplified: detects system preference on first load, then just toggles light/dark.
 */

import { useState, useEffect, useCallback } from 'react';
import { ThemeMode, ThemeState } from './types';
import {
    resolveTheme,
    applyTheme,
    getNextTheme,
    saveTheme,
    loadTheme,
} from './utils';

/**
 * Theme hook providing complete theme management functionality
 * @returns Object with current theme state and control functions
 */
export function useTheme() {
    // Initialize theme state
    const [state, setState] = useState<ThemeState>(() => {
        const savedMode = loadTheme(); // This will auto-detect system if no saved preference
        const resolved = resolveTheme(savedMode);

        return {
            mode: savedMode,
            resolvedTheme: resolved,
        };
    });

    /**
     * Set a specific theme mode
     * @param mode - The theme mode to set ('light' or 'dark')
     */
    const setTheme = useCallback((mode: ThemeMode) => {
        const resolved = resolveTheme(mode);

        setState({
            mode,
            resolvedTheme: resolved,
        });

        // Apply theme to document
        applyTheme(resolved);

        // Save to localStorage
        saveTheme(mode);
    }, []);

    /**
     * Toggle between light and dark themes
     */
    const toggleTheme = useCallback(() => {
        const nextMode = getNextTheme(state.mode);
        setTheme(nextMode);
    }, [state.mode, setTheme]);

    // Apply initial theme
    useEffect(() => {
        applyTheme(state.resolvedTheme);
    }, [state.resolvedTheme]);

    // Return theme state and controls
    return {
        // Current state
        mode: state.mode,
        theme: state.resolvedTheme,

        // Actions
        setTheme,
        toggleTheme,

        // Utility getters
        isLight: state.resolvedTheme === 'light',
        isDark: state.resolvedTheme === 'dark',
    };
}
/**
 * THEME HOOK - useTheme
 * =====================
 * 
 * React hook for managing theme state and providing theme switching functionality.
 * Handles system theme detection, localStorage persistence, and theme application.
 */

import { useState, useEffect, useCallback } from 'react';
import { ThemeMode, ThemeState } from './types';
import {
    getSystemTheme,
    resolveTheme,
    applyTheme,
    getNextTheme,
    saveTheme,
    loadTheme,
    setupSystemThemeListener
} from './utils';

/**
 * Theme hook providing complete theme management functionality
 * @returns Object with current theme state and control functions
 */
export function useTheme() {
    // Initialize theme state
    const [state, setState] = useState<ThemeState>(() => {
        const savedMode = loadTheme();
        const resolved = resolveTheme(savedMode);

        return {
            mode: savedMode,
            resolvedTheme: resolved,
        };
    });

    /**
     * Set a specific theme mode
     * @param mode - The theme mode to set
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
     * Toggle to the next theme in the cycle
     */
    const toggleTheme = useCallback(() => {
        const nextMode = getNextTheme(state.mode);
        setTheme(nextMode);
    }, [state.mode, setTheme]);

    /**
     * Update resolved theme when system changes (for 'system' mode)
     */
    const updateSystemTheme = useCallback(() => {
        if (state.mode === 'system') {
            const newResolved = getSystemTheme();
            if (newResolved !== state.resolvedTheme) {
                setState(prev => ({
                    ...prev,
                    resolvedTheme: newResolved,
                }));
                applyTheme(newResolved);
            }
        }
    }, [state.mode, state.resolvedTheme]);

    // Set up effects
    useEffect(() => {
        // Apply initial theme
        applyTheme(state.resolvedTheme);

        // Set up system theme listener for 'system' mode
        const cleanup = setupSystemThemeListener(updateSystemTheme);

        return cleanup;
    }, [state.resolvedTheme, updateSystemTheme]);

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
        isSystem: state.mode === 'system',
    };
}
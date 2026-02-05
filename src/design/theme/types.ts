/**
 * THEME TYPES & CONSTANTS
 * ========================
 * 
 * Defines the types and constants used throughout the theme system.
 */

import { useInlineTranslation } from '@/utils/language';

// Theme modes supported by the application
export type ThemeMode = 'light' | 'dark' | 'system';

// Theme state interface
export interface ThemeState {
    mode: ThemeMode;
    resolvedTheme: 'light' | 'dark'; // The actual theme being applied
}

// Theme constants
export const THEME_MODES: ThemeMode[] = ['light', 'dark', 'system'];

export const THEME_STORAGE_KEY = 'app-theme';

// Hook to get localized theme labels
export const useThemeLabels = (): Record<ThemeMode, string> => {
    const lightTheme = useInlineTranslation('Tema Claro', 'Light Theme');
    const darkTheme = useInlineTranslation('Tema Escuro', 'Dark Theme');
    const systemDefault = useInlineTranslation('Padrão do Sistema', 'System Default');

    return {
        light: lightTheme,
        dark: darkTheme,
        system: systemDefault,
    };
};

// Theme labels for UI (deprecated - use useThemeLabels hook instead)
export const THEME_LABELS: Record<ThemeMode, string> = {
    light: 'Light Theme',
    dark: 'Dark Theme',
    system: 'System Default',
};

// Theme icons (using emoji for simplicity)
export const THEME_ICONS: Record<ThemeMode, string> = {
    light: '☀️',
    dark: '🌙',
    system: '💻',
};
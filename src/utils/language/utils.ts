import { Language } from './types';

export const STORAGE_KEY = 'tilestories-language';

export const i18nConfig = {
    defaultLanguage: 'pt' as Language,
    supportedLanguages: ['en', 'pt'] as Language[]
};

export function getStoredLanguage(): Language | null {
    if (typeof window === 'undefined') return null;

    try {
        const stored = localStorage.getItem(STORAGE_KEY);
        if (stored && i18nConfig.supportedLanguages.includes(stored as Language)) {
            return stored as Language;
        }
    } catch (error) {
        console.warn('Failed to read language from localStorage:', error);
    }

    return null;
}

export function storeLanguage(language: Language): void {
    if (typeof window === 'undefined') return;

    try {
        localStorage.setItem(STORAGE_KEY, language);
    } catch (error) {
        console.warn('Failed to store language in localStorage:', error);
    }
}
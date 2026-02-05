import { useContext } from 'react';
import { LanguageContext } from './context';
import { LanguageContextType, InlineTranslation } from './types';

export function useLanguage(): LanguageContextType {
    const context = useContext(LanguageContext);
    if (context === undefined) {
        throw new Error('useLanguage must be used within a LanguageProvider');
    }
    return context;
}

// Inline translation hook: useInlineTranslation('Portuguese', 'English') or useInlineTranslation({ pt: '...', en: '...' })
export function useInlineTranslation(
    ptText: string | InlineTranslation,
    enText?: string
): string {
    const { language } = useLanguage();
    if (typeof ptText === 'object') {
        return language === 'pt' ? ptText.pt : ptText.en;
    }
    if (!enText) return ptText;
    return language === 'pt' ? ptText : enText;
}


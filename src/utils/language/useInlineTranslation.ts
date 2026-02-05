/**
 * SIMPLE INLINE TRANSLATION SYSTEM
 * =================================
 * 
 * This system allows you to write text directly inline in components 
 * instead of maintaining separate translation files. Perfect for SEO 
 * and content management.
 * 
 * Usage:
 * const text = useInlineTranslation('Portuguese text here', 'English text here')
 * 
 * Or with object syntax:
 * const text = useInlineTranslation({ pt: 'Texto em português', en: 'English text' })
 */

import { useLanguage } from './hooks';
import { InlineTranslation } from './types';

/**
 * Hook for inline translations - allows hardcoded text in components
 * @param ptText Portuguese text (or translation object)
 * @param enText English text (optional if using object syntax)
 */
export function useInlineTranslation(
    ptText: string | InlineTranslation,
    enText?: string
): string {
    const { language } = useLanguage();

    if (typeof ptText === 'object') {
        return language === 'pt' ? ptText.pt : ptText.en;
    }

    // If only Portuguese text provided, return it (backward compatibility)
    if (!enText) return ptText;

    return language === 'pt' ? ptText : enText;
}
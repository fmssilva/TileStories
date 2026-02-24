import { useState, useRef, useEffect } from 'react';
import { useLanguage, useInlineTranslation } from '@/utils/language';
import { Z_INDEX, LAYOUT } from '@/design';

interface LanguageSelectorProps {
    /** Icon button size in pixels (optional, defaults to 44px from LAYOUT) */
    iconSize?: number;
}

export function LanguageSelector({ iconSize }: LanguageSelectorProps = {}) {
    const [isOpen, setIsOpen] = useState(false);
    const { language, setLanguage } = useLanguage();
    const dropdownRef = useRef<HTMLDivElement>(null);

    // Use fixed sizes from LAYOUT constants
    const buttonSize = iconSize || LAYOUT.ICON_BUTTON_SIZE;  // 44px default
    const emojiSize = LAYOUT.ICON_SIZE;                      // 24px fixed

    // Inline translations for language selector
    const portugueseText = useInlineTranslation('Português', 'Portuguese');
    const englishText = useInlineTranslation('Inglês', 'English');
    const changeLanguageText = useInlineTranslation('Alterar idioma', 'Change language');

    const languages = [
        { code: 'pt' as const, name: portugueseText, flag: '🇵🇹' },
        { code: 'en' as const, name: englishText, flag: '🇺🇸' }
    ];

    const currentLanguage = languages.find(lang => lang.code === language);

    useEffect(() => {
        const handleClickOutside = (event: MouseEvent) => {
            if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
                setIsOpen(false);
            }
        };

        document.addEventListener('mousedown', handleClickOutside);
        return () => {
            document.removeEventListener('mousedown', handleClickOutside);
        };
    }, []);

    const handleLanguageSelect = (langCode: 'pt' | 'en') => {
        setLanguage(langCode);
        setIsOpen(false);
    };

    return (
        <div className="relative" ref={dropdownRef}>
            {/* Simplified button - only flag */}
            <button
                onClick={() => setIsOpen(!isOpen)}
                className="flex items-center justify-center text-white hover:bg-white/10 rounded-md transition-all duration-200 hover:scale-105"
                style={{
                    width: `${buttonSize}px`,
                    height: `${buttonSize}px`,
                    fontSize: `${emojiSize}px`, // Fixed emoji size
                    fontFamily: 'system-ui, -apple-system, "Segoe UI", "Segoe UI Emoji", "Apple Color Emoji", "Noto Color Emoji", sans-serif',
                }}
                aria-label={changeLanguageText}
                title={currentLanguage?.name}
            >
                <span style={{ fontSize: 'inherit', lineHeight: 1 }}>{currentLanguage?.flag}</span>
            </button>

            {isOpen && (
                <div
                    className="absolute right-0 mt-2 w-48 bg-white dark:bg-gray-800 rounded-lg shadow-lg border border-gray-200 dark:border-gray-700"
                    style={{ zIndex: Z_INDEX.FLOATING }}
                >
                    <div className="py-1">
                        {languages.map((lang) => (
                            <button
                                key={lang.code}
                                onClick={() => handleLanguageSelect(lang.code)}
                                className={`flex items-center w-full px-4 py-2 text-sm text-left hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors ${language === lang.code
                                    ? 'bg-blue-50 dark:bg-blue-900/30 text-blue-700 dark:text-blue-300'
                                    : 'text-gray-700 dark:text-gray-300'
                                    }`}
                            >
                                <span className="mr-3 text-lg">{lang.flag}</span>
                                <span>{lang.name}</span>
                                {language === lang.code && (
                                    <span className="ml-auto text-blue-600 dark:text-blue-400">✓</span>
                                )}
                            </button>
                        ))}
                    </div>
                </div>
            )}
        </div>
    );
}
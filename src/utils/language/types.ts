export type Language = 'en' | 'pt';

export interface LanguageContextType {
    language: Language;
    setLanguage: (language: Language) => void;
}

export interface InlineTranslation {
    pt: string;
    en: string;
}
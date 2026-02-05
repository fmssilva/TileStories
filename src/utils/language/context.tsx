import { createContext, useState, ReactNode } from 'react';
import { Language, LanguageContextType } from './types';
import { getStoredLanguage, storeLanguage, i18nConfig } from './utils';

const LanguageContext = createContext<LanguageContextType | undefined>(undefined);

export { LanguageContext };

interface LanguageProviderProps {
    children: ReactNode;
}

export function LanguageProvider({ children }: LanguageProviderProps) {
    const [language, setLanguageState] = useState<Language>(() => {
        const stored = getStoredLanguage();
        return stored || i18nConfig.defaultLanguage;
    });

    const setLanguage = (newLanguage: Language) => {
        setLanguageState(newLanguage);
        storeLanguage(newLanguage);
    };

    const contextValue: LanguageContextType = {
        language,
        setLanguage
    };

    return (
        <LanguageContext.Provider value={contextValue}>
            {children}
        </LanguageContext.Provider>
    );
}
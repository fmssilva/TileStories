// ============================================================================
// APPLICATION ENTRY POINT - CLINIC COMPARE PLATFORM
// ============================================================================

/**
 * This file serves as the main entry point for Clinic Compare.
 * It demonstrates modern React patterns and serves as a foundation for 
 * healthcare provider comparison with transparent pricing.
 */

import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { BrowserRouter } from 'react-router-dom';
import { Provider } from 'react-redux';

import App from './App';
import './index.css';
import { appConfig } from '@/config/app';
import { store } from '@/store';
import { LanguageProvider } from '@/utils/language';

// Set dynamic document title from centralized configuration
const setDocumentTitle = () => {
    document.title = appConfig.displayName;
};

// Get the root DOM element - we assert it exists with "!" since it's guaranteed in our HTML
const rootElement = document.getElementById('root')!;

/**
 * React 18+ createRoot for concurrent rendering features
 * Provides better performance and user experience compared to legacy ReactDOM.render
 */
const root = createRoot(rootElement);

// Set title before rendering
setDocumentTitle();

// Render the application with modern providers and best practices
root.render(
    <StrictMode>
        {/* StrictMode helps catch bugs and warns about deprecated features in development */}
        <LanguageProvider>
            {/* Language Provider for internationalization */}
            <Provider store={store}>
                {/* Redux Provider for global state management */}
                <BrowserRouter>
                    {/* BrowserRouter enables client-side routing for the entire application */}
                    <App />
                </BrowserRouter>
            </Provider>
        </LanguageProvider>
    </StrictMode>
);
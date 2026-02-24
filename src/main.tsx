// ============================================================================
// APPLICATION ENTRY POINT - TILESTORIES AR PLATFORM
// ============================================================================

/**
 * This file serves as the main entry point for TileStories.
 * AR experiences for the Grande Panorama de Lisboa at the 
 * Museu Nacional do Azulejo.
 */

import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { BrowserRouter } from 'react-router-dom';

import App from './App';
import './index.css';
import { appConfig } from '@/config/app';
import { LanguageProvider } from '@/utils/language';
import { NavigationProvider } from '@/layout_and_navigation';
import { StickyProvider } from '@/layout_and_navigation/sticky';

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
            {/* BrowserRouter enables client-side routing for the entire application */}
            <BrowserRouter>
                {/* NavigationProvider enables navigation context features */}
                <NavigationProvider>
                    {/* StickyProvider manages hierarchical sticky elements */}
                    <StickyProvider>
                        <App />
                    </StickyProvider>
                </NavigationProvider>
            </BrowserRouter>
        </LanguageProvider>
    </StrictMode>
);
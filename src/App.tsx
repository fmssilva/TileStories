// ============================================================================
// TILESTORIES - AR EXPERIENCES FOR MUSEU DO AZULEJO
// ============================================================================

/**
 * This is the root component of TileStories.
 * An AR experience platform for exploring the Grande Panorama de Lisboa
 * at the Museu Nacional do Azulejo.
 */

import { Routes } from 'react-router-dom';
import { useEffect, Suspense } from 'react';
import { MainLayout, navigationConfig, generateRoutesFromConfig } from '@/layout_and_navigation';
import { initializeNativeZoom } from '@/design/nativeZoom';


/**
 * App Component - Main application router and layout provider
 * 
 * Architecture decisions:
 * - Uses React Router for client-side navigation
 * - Auto-generates routes from navigationConfig (single source of truth)
 * - Wraps content in MainLayout for consistent structure
 * - Separates routing logic from layout concerns
 * - Follows domain-centered organization
 * 
 * Routes are now auto-generated from src/layout_and_navigation/config/navigation.ts
 * To add a new page:
 * 1. Create the page component in src/domains/[domain-name]/
 * 2. Add a NavItem to navigationConfig
 * 3. Routes, nav menu, and breadcrumbs update automatically!
 */
function App() {
    // Initialize native zoom enhancement on app start
    useEffect(() => {
        const cleanup = initializeNativeZoom();
        return cleanup; // Cleanup on unmount
    }, []);

    return (
        <div className="min-h-screen bg-background text-foreground">
            {/* Main application wrapper with full height and theme-aware colors */}
            {/* Note: Scroll management is handled by NavigationContext */}
            <MainLayout>
                <Suspense fallback={
                    <div className="flex items-center justify-center min-h-screen">
                        <div className="text-center">
                            <div className="w-16 h-16 border-4 border-azulejo-blue-200 border-t-azulejo-blue-600 rounded-full animate-spin mx-auto mb-4"></div>
                            <p className="text-lg text-gray-600 dark:text-gray-400">Loading...</p>
                        </div>
                    </div>
                }>
                    <Routes>
                        {/* Auto-generated routes from navigation config */}
                        {generateRoutesFromConfig(navigationConfig)}
                    </Routes>
                </Suspense>
            </MainLayout>
        </div>
    );
}

export default App;
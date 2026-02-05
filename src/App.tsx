// ============================================================================
// TILESTORIES - AR EXPERIENCES FOR MUSEU DO AZULEJO
// ============================================================================

/**
 * This is the root component of TileStories.
 * An AR experience platform for exploring the Grande Panorama de Lisboa
 * at the Museu Nacional do Azulejo.
 */

import { Routes, Route } from 'react-router-dom';
import { useEffect } from 'react';
import { HomePage } from '@/domains/home/HomePage';
import { MainLayout } from '@/layout_and_navigation/MainLayout';
import { NotFound } from '@/components/error';
import { initializeNativeZoom } from '@/utils/nativeZoom';


/**
 * App Component - Main application router and layout provider
 * 
 * Architecture decisions:
 * - Uses React Router for client-side navigation
 * - Wraps content in MainLayout for consistent structure
 * - Separates routing logic from layout concerns
 * - Follows domain-centered organization
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
            <MainLayout>
                <Routes>
                    <Route path="/" element={<HomePage />} />
                    <Route path="*" element={<NotFound />} />
                </Routes>
            </MainLayout>
        </div>
    );
}

export default App;
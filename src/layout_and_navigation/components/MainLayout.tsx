/**
 * MAIN LAYOUT COMPONENT
 * =====================
 * 
 * Main layout wrapper that composes all navigation components.
 * Wraps all pages with header, footer, and navigation features.
 */

import { Suspense } from 'react';
import { Header } from './Header';
import { Footer } from './Footer';
import { BackToTop } from './BackToTop';
import { Breadcrumbs } from './Breadcrumbs';
import { Spinner } from '@/components/feedback';
import type { MainLayoutProps } from '../types';

/**
 * Main Layout
 * 
 * Wraps page content with consistent navigation structure.
 * 
 * @example
 * <MainLayout showBreadcrumbs>
 *   <YourPage />
 * </MainLayout>
 */
export function MainLayout({
    children,
    showFooterNavigation = true,
    headerActions,
    showBreadcrumbs = false,
}: MainLayoutProps) {

    return (
        <div className="min-h-screen flex flex-col">
            {/* Header */}
            <Header actions={headerActions} showProgress />

            {/* Breadcrumbs (optional) */}
            {showBreadcrumbs && <Breadcrumbs />}

            {/* Back to Top Button */}
            <BackToTop showProgress showAfter={200} />

            {/* Main Content */}
            <main className="flex-1">
                <Suspense fallback={
                    <div className="flex items-center justify-center min-h-[50vh]">
                        <Spinner size="lg" />
                    </div>
                }>
                    {children}
                </Suspense>
            </main>

            {/* Footer */}
            <Footer showNavigation={showFooterNavigation} />
        </div>
    );
}

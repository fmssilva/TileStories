// ============================================================================
// MAIN LAYOUT COMPONENT - MODERN 2026 UX
// ============================================================================

/**
 * MainLayout - Provides consistent structure across all pages
 * 
 * 🎓 FOR STUDENTS - This is the "wrapper" for your entire website:
 * 
 * ┌─────────────────────────┐
 * │        Header          │  ← Always visible (sticky)
 * ├─────────────────────────┤
 * │                        │
 * │     Your Page          │  ← HomePage, AboutPage, etc.
 * │     Content            │    
 * │     Goes Here          │
 * │                        │
 * ├─────────────────────────┤
 * │        Footer          │  ← Always visible
 * └─────────────────────────┘
 * 
 * Benefits:
 * ✅ Add new pages without rebuilding header/footer
 * ✅ Consistent navigation across all pages
 * ✅ Modern scroll behavior (header hides/shows)
 * ✅ Back-to-top button included automatically
 * 
 * Features modernized for 2026 UX best practices:
 * - Sticky header with smart scroll behavior
 * - Back-to-top button positioned behind header (reveals when header hides)
 * - Backdrop blur and transparency effects
 * - Domain-centered color system
 * - Mobile-first responsive design
 * - Theme-adaptive styling
 * - Proper semantic HTML layout
 */

import { ReactNode } from 'react';
import { Header, Footer } from './index';
import { BackToTop } from './BackToTopIcon';

interface MainLayoutProps {
    children: ReactNode;
    /** Show social links in footer */
    showSocialLinks?: boolean;
    /** Show navigation in footer */
    showFooterNavigation?: boolean;
    /** Custom header actions */
    headerActions?: ReactNode;
}

export function MainLayout({
    children,
    showSocialLinks = false,
    showFooterNavigation = true,
    headerActions
}: MainLayoutProps) {
    return (
        <div className="min-h-screen flex flex-col">
            {/* Modern Header with Sticky Scroll Behavior */}
            <Header actions={headerActions} />

            {/* Back to Top Button - Fixed behind header, revealed when header slides up */}
            <BackToTop
                showProgress={true}
                scrollDuration={800}
                offset={16}
            />

            {/* Main Content Area - Account for fixed header */}
            <main className="flex-1 pt-16">
                <div className="min-h-full">
                    {children}
                </div>
            </main>

            {/* Modern Footer */}
            <Footer
                showSocialLinks={showSocialLinks}
                showNavigation={showFooterNavigation}
            >
                {/* Optional: Custom footer content can be added here */}
            </Footer>

        </div>
    );
}
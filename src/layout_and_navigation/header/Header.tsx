// ============================================================================
// MODERN HEADER COMPONENT - 2026 UX BEST PRACTICES
// ============================================================================

/**
 * Enhanced header component with modern UX features:
 * 
 * ✅ Sticky positioning with smart show/hide
 * ✅ Backdrop blur with transparency when scrolling
 * ✅ Domain-specific color system
 * ✅ Smooth animations and transitions
 * ✅ Mobile-responsive design
 * ✅ Theme-adaptive styling
 * ✅ Progressive enhancement
 */

import { ReactNode } from 'react';
import { useLocation, Link } from 'react-router-dom';
import { Logo } from '@/branding';
import { ThemeToggleIcon, useTheme } from '@/domains/theme';
import { useHeaderScroll } from '../hooks';
import { getHeaderStyles, getNavLinkStyles } from '../colors';
import { LanguageSelector } from '../../components/LanguageSelector';
import { useInlineTranslation } from '@/utils/language';

interface HeaderProps {
    /** Additional navigation items */
    children?: ReactNode;
    /** Custom logo component */
    logo?: ReactNode;
    /** Additional header actions */
    actions?: ReactNode;
}

export function Header({ children, logo, actions }: HeaderProps) {
    const { theme } = useTheme();
    const location = useLocation();

    // Inline translations for navigation
    const homeText = useInlineTranslation('Início', 'Home');
    const forClinicsText = useInlineTranslation('Para Clínicas', 'For Clinics');
    const aboutText = useInlineTranslation('Para Pacientes', 'For Patients');

    // Simplified scroll handling - use the optimized hook
    const scrollState = useHeaderScroll({
        scrollThreshold: 10,
        hideThreshold: 150,
        enableHideShow: true,
        debounceMs: 16,
    });

    // Get header styles from domain colors (now with consistent white text)
    const headerStyles = getHeaderStyles(theme, scrollState.isScrolled);

    // Simplified header classes
    const headerClasses = [
        'fixed top-0 left-0 right-0 z-50',
        'transition-all duration-300 ease-in-out',
        'border-b',
        scrollState.isVisible ? 'translate-y-0' : '-translate-y-full',
        scrollState.isScrolled ? 'backdrop-blur-md shadow-sm' : '',
    ].filter(Boolean).join(' ');

    return (
        <header
            className={headerClasses}
            style={{
                background: headerStyles.background,
                backdropFilter: headerStyles.backdropFilter,
                borderBottomColor: headerStyles.borderColor,
            }}
        >
            <div className="container mx-auto px-4">
                <nav className="flex items-center justify-between h-16">
                    {/* Logo and Brand */}
                    <div className="flex items-center space-x-3">
                        {logo || (
                            <>
                                <div className="flex-shrink-0 transition-opacity duration-200 hover:opacity-80 w-10 h-10 sm:w-11 sm:h-11">
                                    <Logo
                                        variant="icon"
                                        size="xl"
                                        className="w-full h-full"
                                    />
                                </div>
                                <div className="font-bold text-xl sm:text-2xl transition-colors duration-200">
                                    <span className="text-[#4CAF50]">Clinic</span>
                                    <span className="text-[#1976D2]">Compare</span>
                                </div>
                            </>
                        )}
                    </div>

                    {/* Navigation Menu - Desktop */}
                    <div className="hidden md:flex items-center space-x-6">
                        {/* Default Navigation Links */}
                        <NavLink to="/" theme={theme} isActive={location.pathname === '/'}>
                            {homeText}
                        </NavLink>
                        <NavLink to="/about" theme={theme} isActive={location.pathname === '/about'}>
                            {aboutText}
                        </NavLink>
                        <NavLink to="/para-clinicas" theme={theme} isActive={location.pathname === '/para-clinicas'}>
                            {forClinicsText}
                        </NavLink>

                        {/* Custom navigation items */}
                        {children}

                        {/* Header Actions */}
                        <div className="flex items-center space-x-2 ml-4">
                            {actions}
                            <LanguageSelector />
                            <ThemeToggleIcon size="md" />
                        </div>
                    </div>

                    {/* Mobile Menu */}
                    <div className="md:hidden flex items-center gap-2">
                        <LanguageSelector />
                        <MobileMenuButton theme={theme} />
                        <ThemeToggleIcon size="sm" />
                    </div>
                </nav>
            </div>

            {/* Scroll Progress Indicator (optional) */}
            {scrollState.isScrolled && (
                <div
                    className="absolute bottom-0 left-0 h-0.5 transition-all duration-300"
                    style={{
                        width: `${Math.min((scrollState.scrollY / (document.documentElement.scrollHeight - window.innerHeight)) * 100, 100)}%`,
                        background: 'linear-gradient(90deg, rgba(255,255,255,0.8) 0%, rgba(255,255,255,0.4) 100%)',
                    }}
                />
            )}
        </header>
    );
}

// ============================================================================
// NAVIGATION LINK COMPONENT
// ============================================================================

interface NavLinkProps {
    to: string;
    children: ReactNode;
    theme: 'light' | 'dark';
    isActive?: boolean;
    onClick?: () => void;
}

function NavLink({ to, children, theme, isActive = false, onClick }: NavLinkProps) {
    const linkStyles = getNavLinkStyles(theme, isActive);

    return (
        <Link
            to={to}
            onClick={onClick}
            className="relative px-3 py-2 rounded-md text-sm font-medium transition-all duration-200 hover:scale-105"
            style={{
                color: linkStyles.color,
                backgroundColor: linkStyles.backgroundColor,
            }}
            onMouseEnter={(e) => {
                if (!isActive) {
                    e.currentTarget.style.color = linkStyles.hoverColor;
                    e.currentTarget.style.backgroundColor = linkStyles.hoverBackgroundColor;
                }
            }}
            onMouseLeave={(e) => {
                if (!isActive) {
                    e.currentTarget.style.color = linkStyles.color;
                    e.currentTarget.style.backgroundColor = linkStyles.backgroundColor;
                }
            }}
        >
            {children}
            {isActive && (
                <div
                    className="absolute bottom-0 left-1/2 transform -translate-x-1/2 w-1 h-1 rounded-full"
                    style={{ backgroundColor: linkStyles.color }}
                />
            )}
        </Link>
    );
}

// ============================================================================
// MOBILE MENU BUTTON
// ============================================================================

interface MobileMenuButtonProps {
    theme: 'light' | 'dark';
}

function MobileMenuButton({ theme }: MobileMenuButtonProps) {
    const iconColor = theme === 'light' ? 'rgb(17, 24, 39)' : 'rgb(243, 244, 246)';

    return (
        <button
            className="p-2 rounded-md transition-all duration-200 hover:scale-110 active:scale-95"
            style={{
                backgroundColor: theme === 'light'
                    ? 'rgba(59, 130, 246, 0.1)'
                    : 'rgba(96, 165, 250, 0.1)',
            }}
            aria-label="Open menu"
        >
            <div className="w-6 h-6 flex flex-col justify-center space-y-1">
                <div
                    className="w-full h-0.5 transition-all duration-200"
                    style={{ backgroundColor: iconColor }}
                />
                <div
                    className="w-full h-0.5 transition-all duration-200"
                    style={{ backgroundColor: iconColor }}
                />
                <div
                    className="w-full h-0.5 transition-all duration-200"
                    style={{ backgroundColor: iconColor }}
                />
            </div>
        </button>
    );
}
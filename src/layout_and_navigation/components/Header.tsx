/**
 * HEADER COMPONENT
 * ================
 * 
 * Main site header with smart scroll behavior.
 * 
 * ARCHITECTURE (Separation of Concerns):
 * - Header = Central Orchestrator
 *   - Measures all child components using DOM refs
 *   - Calculates exact dimensions for each section
 *   - Distributes space based on priorities
 *   - Enforces hard boundaries with exact pixel dimensions
 * 
 * - Logo + Title = Fixed size element (measured via DOM)
 * - IconsGroup = Fixed size element (LanguageSelector + ThemeToggle, measured via DOM)
 * - Spacer = Hard-coded gaps between sections
 * - NavigationManager = Receives exact available width, adapts within boundaries
 * 
 * DATA FLOW:
 * 1. Header container width measured via ResizeObserver
 * 2. Logo section width measured via ref.current.offsetWidth
 * 3. IconsGroup width measured via ref.current.offsetWidth
 * 4. Spacer widths calculated based on available space
 * 5. NavigationManager receives remaining width
 * 6. All components render with exact dimensions
 * 
 * Features:
 * - Sticky positioning with hide/show on scroll
 * - Responsive desktop/mobile navigation
 * - Theme toggle and language selector
 * - Backdrop blur when scrolled
 * - Progress indicator
 * - Dynamic gradient transition based on logo width
 */
import { LogoWithName } from '@/branding';
import { useTheme } from '@/design/theme';
import { Z_INDEX, LAYOUT } from '@/design';
import { useHeaderScroll, useMobileMenu } from '../hooks';
import { MobileMenu } from './MobileMenu';
import { NavigationManager } from './NavigationManager';
import { IconsGroup } from './IconsGroup';
import { Spacer } from './Spacer';
import { navigationConfig } from '../config';
import { getMainNavItems } from '../utils';
import { globalColors } from '@/design/colors';
import type { HeaderProps } from '../types';
import { useRef, useState, useEffect } from 'react';

/**
 * Header Component
 * 
 * Responsive header with smart scroll behavior.
 * Acts as central orchestrator for all header layout calculations.
 * 
 * @example
 * <Header showProgress actions={<CustomButton />} />
 */
export function Header({
    children,
    logo,
    actions,
    showProgress = false,
}: HeaderProps = {}) {
    const { theme } = useTheme();
    const navItems = getMainNavItems(navigationConfig);
    const mobileMenu = useMobileMenu();

    // ═══════════════════════════════════════════════════════════════════════
    // REFS - For measuring actual DOM widths
    // ═══════════════════════════════════════════════════════════════════════
    const headerRef = useRef<HTMLElement>(null);
    const navContainerRef = useRef<HTMLDivElement>(null);
    const logoSectionRef = useRef<HTMLDivElement>(null);
    const iconsGroupRef = useRef<HTMLDivElement>(null);

    // ═══════════════════════════════════════════════════════════════════════
    // STATE - Calculated dimensions
    // ═══════════════════════════════════════════════════════════════════════
    const [logoWidth, setLogoWidth] = useState(0);
    const [iconsWidth, setIconsWidth] = useState(0);
    const [navigationWidth, setNavigationWidth] = useState(0);
    const [containerWidth, setContainerWidth] = useState(0);

    // ═══════════════════════════════════════════════════════════════════════
    // LAYOUT CONSTANTS (from LAYOUT config)
    // ═══════════════════════════════════════════════════════════════════════
    const {
        HEADER_HEIGHT: PC_HEADER_HEIGHT,
        LOGO_HEIGHT: PC_LOGO_HEIGHT,
        GAP_LOGO_NAV: PC_GAP_LOGO_NAV,
        GAP_NAV_ICONS: PC_GAP_NAV_ICONS,
        GAP_ICONS_RIGHT: PC_GAP_ICONS_RIGHT,
        CONTAINERS_HORIZONTAL_PADDING: PC_CONTAINERS_HORIZONTAL_PADDING,
        ICON_BUTTON_SIZE: PC_ICON_BUTTON_SIZE,
    } = LAYOUT;

    // ═══════════════════════════════════════════════════════════════════════
    // MEASUREMENT LOGIC - Central orchestrator
    // ═══════════════════════════════════════════════════════════════════════
    useEffect(() => {
        const measureLayout = () => {
            // Step 1: Measure container width
            if (!navContainerRef.current) return;
            const containerW = navContainerRef.current.offsetWidth;
            setContainerWidth(containerW);

            // Step 2: Measure logo section actual width
            if (logoSectionRef.current) {
                const logoW = logoSectionRef.current.offsetWidth;
                setLogoWidth(logoW);
            }

            // Step 3: Measure icons group actual width
            if (iconsGroupRef.current) {
                const iconsW = iconsGroupRef.current.offsetWidth;
                setIconsWidth(iconsW);
            }

            // Step 4: Calculate available width for navigation
            // Formula: Container - Logo - Icons - Gaps - Padding
            if (logoWidth > 0 && iconsWidth > 0) {
                const availableForNav = containerW - logoWidth - iconsWidth - PC_GAP_LOGO_NAV - PC_GAP_NAV_ICONS - PC_GAP_ICONS_RIGHT - PC_CONTAINERS_HORIZONTAL_PADDING;
                setNavigationWidth(Math.max(0, availableForNav));
            }
        };

        measureLayout();

        // Re-measure on resize
        const resizeObserver = new ResizeObserver(measureLayout);
        if (navContainerRef.current) {
            resizeObserver.observe(navContainerRef.current);
        }
        if (logoSectionRef.current) {
            resizeObserver.observe(logoSectionRef.current);
        }
        if (iconsGroupRef.current) {
            resizeObserver.observe(iconsGroupRef.current);
        }

        return () => resizeObserver.disconnect();
    }, [logoWidth, iconsWidth, PC_GAP_LOGO_NAV, PC_GAP_NAV_ICONS, PC_GAP_ICONS_RIGHT, PC_CONTAINERS_HORIZONTAL_PADDING]);

    // ═══════════════════════════════════════════════════════════════════════
    // SCROLL BEHAVIOR
    // ═══════════════════════════════════════════════════════════════════════
    const { isScrolled, isVisible, scrollY } = useHeaderScroll({
        scrollThreshold: 10,
        hideThreshold: 150,
        enableHideShow: true,
    });

    // ═══════════════════════════════════════════════════════════════════════
    // DYNAMIC GRADIENT CALCULATION
    // ═══════════════════════════════════════════════════════════════════════
    // Calculate gradient transition point based on logo width percentage
    const logoPercentage = containerWidth > 0 ? (logoWidth / containerWidth) * 100 : 25;

    // Add small buffer zone for smooth transition (5% of container width)
    const transitionBuffer = 18;
    const whiteEnd = logoPercentage;
    const transitionStart = logoPercentage + 2;
    const blueStart = logoPercentage + transitionBuffer;

    const gradientStyle = isScrolled
        ? `linear-gradient(120deg, 
            rgba(255,255,255,0.3) 0%, 
            rgba(255,255,255,0.2) ${whiteEnd * 0.6}%, 
            rgba(255,255,255,0.08) ${transitionStart}%, 
            ${globalColors.primary[600]}f2 ${blueStart}%, 
            ${globalColors.primary[400]}f2 100%)`
        : `linear-gradient(120deg, 
            rgba(255,255,255,0.4) 0%, 
            rgba(255,255,255,0.25) ${whiteEnd * 0.6}%, 
            rgba(255,255,255,0.1) ${transitionStart}%, 
            ${globalColors.primary[600]} ${blueStart}%, 
            ${globalColors.primary[400]} 100%)`;

    const borderColor = isScrolled
        ? (theme === 'light' ? '#e2e8f0' : '#475569')
        : 'transparent';

    // ═══════════════════════════════════════════════════════════════════════
    // STYLING
    // ═══════════════════════════════════════════════════════════════════════
    const headerClasses = [
        'fixed top-0 left-0 right-0',
        'transition-all duration-300 ease-in-out',
        'border-b',
        isVisible ? 'translate-y-0' : '-translate-y-full',
    ].filter(Boolean).join(' ');

    // ═══════════════════════════════════════════════════════════════════════
    // RENDER
    // ═══════════════════════════════════════════════════════════════════════
    return (
        <>
            <header
                ref={headerRef}
                className={headerClasses}
                style={{
                    zIndex: Z_INDEX.HEADER,
                    background: gradientStyle,
                    backdropFilter: isScrolled ? 'blur(8px)' : 'none',
                    borderBottomColor: borderColor,
                }}
            >
                <div ref={navContainerRef} className="w-full max-w-screen-2xl mx-auto px-4 sm:px-6 lg:px-8">
                    <nav
                        className="flex items-center justify-between"
                        style={{
                            height: `${PC_HEADER_HEIGHT}px`,
                        }}
                    >
                        {/* ═══════════════════════════════════════════════════ */}
                        {/* LOGO + TITLE SECTION (Fixed size, measured) */}
                        {/* ═══════════════════════════════════════════════════ */}
                        <div ref={logoSectionRef}>
                            {logo || <LogoWithName logoHeight={PC_LOGO_HEIGHT} />}
                        </div>

                        {/* ═══════════════════════════════════════════════════ */}
                        {/* SPACER - Gap between Logo and Navigation */}
                        {/* ═══════════════════════════════════════════════════ */}
                        <Spacer width={PC_GAP_LOGO_NAV} />

                        {/* ═══════════════════════════════════════════════════ */}
                        {/* NAVIGATION SECTION (Receives exact width) */}
                        {/* ═══════════════════════════════════════════════════ */}
                        <NavigationManager
                            navItems={navItems}
                            availableWidth={navigationWidth}
                            onHamburgerClick={mobileMenu.toggle}
                            componentsHeight={PC_ICON_BUTTON_SIZE}
                        >
                            {children}
                        </NavigationManager>

                        {/* ═══════════════════════════════════════════════════ */}
                        {/* SPACER - Gap between Navigation and Icons */}
                        {/* ═══════════════════════════════════════════════════ */}
                        <Spacer width={PC_GAP_NAV_ICONS} />

                        {/* ═══════════════════════════════════════════════════ */}
                        {/* ICONS GROUP (Fixed size, measured) */}
                        {/* ═══════════════════════════════════════════════════ */}
                        <IconsGroup
                            ref={iconsGroupRef}
                            actions={actions}
                            componentsHeight={PC_ICON_BUTTON_SIZE}
                        />
                    </nav>
                </div>

                {/* Scroll Progress Indicator */}
                {showProgress && isScrolled && (
                    <div
                        className="absolute bottom-0 left-0 h-0.5 bg-gradient-to-r from-primary to-primary/50 transition-all duration-300"
                        style={{
                            width: `${Math.min((scrollY / (document.documentElement.scrollHeight - window.innerHeight)) * 100, 100)}%`,
                        }}
                    />
                )}
            </header>

            {/* Mobile Menu */}
            <MobileMenu isOpen={mobileMenu.isOpen} onClose={mobileMenu.close} />
        </>
    );
}
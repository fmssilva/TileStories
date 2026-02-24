/**
 * MOBILE MENU COMPONENT
 * =====================
 * 
 * Modern slide-in mobile navigation menu with floating card design.
 * Features rounded corners, shadow, and lighter appearance.
 */

import { Link, useLocation } from 'react-router-dom';
import { useState, useEffect } from 'react';
import { Z_INDEX } from '@/design';
import { navigationConfig } from '../config';
import { getMainNavItems, getNavItemLabel } from '../utils';
import { useInlineTranslation, useLanguage } from '@/utils/language';
import type { MobileMenuProps } from '../types';
import { SiteName } from '@/branding';

/**
 * Mobile Menu
 * 
 * Modern floating menu for mobile devices.
 * 
 * @example
 * const { isOpen, close } = useMobileMenu();
 * <MobileMenu isOpen={isOpen} onClose={close} />
 */
export function MobileMenu({ isOpen, onClose, items }: MobileMenuProps) {
    const location = useLocation();
    const { language } = useLanguage();
    const navItems = items || getMainNavItems(navigationConfig);
    const closeLabel = useInlineTranslation('Fechar menu', 'Close menu');
    const menuLabel = useInlineTranslation('Menu', 'Menu');

    // Track closing state for smooth transitions
    const [isClosing, setIsClosing] = useState(false);
    const [shouldRender, setShouldRender] = useState(isOpen);

    // Handle open/close transitions
    useEffect(() => {
        if (isOpen) {
            setShouldRender(true);
            setIsClosing(false);
        } else if (shouldRender) {
            // Start closing animation
            setIsClosing(true);
            // Remove from DOM after transition completes (300ms)
            const timer = setTimeout(() => {
                setShouldRender(false);
                setIsClosing(false);
            }, 300);
            return () => clearTimeout(timer);
        }
        return undefined; // Explicit return for code paths that don't need cleanup
    }, [isOpen, shouldRender]);

    // Handle backdrop click
    const handleBackdropClick = (e: React.MouseEvent) => {
        e.stopPropagation();
        onClose();
    };

    // Handle close button click
    const handleCloseClick = (e: React.MouseEvent) => {
        e.stopPropagation();
        onClose();
    };

    // Don't render if not needed
    if (!shouldRender) return null;

    const isVisible = isOpen && !isClosing;

    return (
        <>
            {/* Backdrop with blur */}
            <div
                className={`fixed inset-0 bg-black/50 backdrop-blur-sm md:hidden transition-opacity duration-300 ${isVisible ? 'opacity-100' : 'opacity-0'
                    }`}
                style={{
                    zIndex: Z_INDEX.FLOATING,
                    pointerEvents: isClosing ? 'none' : 'auto' // Disable clicks during closing
                }}
                onClick={handleBackdropClick}
                aria-hidden="true"
            />

            {/* Menu Panel - Portuguese Tile Inspired */}
            <div
                className={`
                fixed top-5 right-5 
                w-80 max-w-[85vw]
                bg-gradient-to-br from-background via-background to-muted/80
                md:hidden 
                transition-all duration-300 ease-out
                shadow-2xl
                ${isVisible ? 'translate-x-0' : 'translate-x-full'}
            `}
                style={{
                    zIndex: Z_INDEX.FLOATING + 10,
                    pointerEvents: isClosing ? 'none' : 'auto' // Disable clicks during closing
                }}
            >

                {/* Header with azulejo accent */}
                <div className="relative p-4 border-b-2 border-[#1e4d8b]/20">
                    {/* Decorative top bar */}
                    <div className="absolute top-0 left-0 right-0 h-1 bg-gradient-to-r from-[#1e4d8b] via-[#2563a8] to-[#d4a04c]"></div>

                    <div className="flex items-center justify-between">
                        <div className="flex items-center gap-1">
                            <h2 className="text-lg font-bold text-foreground">{menuLabel}</h2>
                        </div>
                        <button
                            onClick={handleCloseClick}
                            className="p-2 rounded-lg hover:bg-muted/80 transition-all duration-200 hover:rotate-90 group"
                            aria-label={closeLabel}
                        >
                            <svg className="w-5 h-5 text-muted-foreground group-hover:text-foreground" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2.5} d="M6 18L18 6M6 6l12 12" />
                            </svg>
                        </button>
                    </div>
                </div>

                {/* Navigation Links */}
                <nav className="p-4 space-y-1 flex-1 overflow-y-auto relative">
                    {navItems.map((item, index) => {
                        const isActive = location.pathname === item.path;
                        return (
                            <Link
                                key={item.id}
                                to={item.path}
                                onClick={onClose}
                                className={`
                                group relative block px-5 py-4 rounded-lg font-medium
                                transition-all duration-200
                                ${isActive
                                        ? 'bg-gradient-to-r from-[#1e4d8b] to-[#2563a8] text-white shadow-lg shadow-[#1e4d8b]/20'
                                        : 'hover:bg-muted/60 text-foreground/90 hover:text-foreground hover:translate-x-1'
                                    }
                            `}
                                style={{
                                    animationDelay: `${index * 50}ms`,
                                    animation: isOpen ? 'slideInRight 0.3s ease-out forwards' : 'none'
                                }}
                            >
                                {/* Tile-inspired accent */}
                                <div className={`
                                absolute left-0 top-1/2 -translate-y-1/2 w-1 h-8 rounded-r-full
                                transition-all duration-200
                                ${isActive
                                        ? 'bg-[#d4a04c] opacity-100'
                                        : 'bg-[#1e4d8b] opacity-0 group-hover:opacity-100'
                                    }
                            `}></div>

                                <span className="relative z-10 flex items-center gap-3">
                                    {/* Optional: Add icon dot */}
                                    <span className={`
                                    w-1.5 h-1.5 rounded-full transition-all
                                    ${isActive ? 'bg-[#d4a04c]' : 'bg-[#1e4d8b]/40 group-hover:bg-[#d4a04c]'}
                                `}></span>
                                    {getNavItemLabel(item, language)}
                                </span>
                            </Link>
                        );
                    })}
                </nav>

                {/* Bottom decorative footer */}
                <div className="p-5 border-t border-border/50 relative">
                    <div className="h-1 bg-gradient-to-r from-[#d4a04c] via-[#2563a8] to-[#1e4d8b] rounded-full mb-4"></div>
                    <SiteName fontSize={18} />
                </div>
            </div>
        </>
    );
}

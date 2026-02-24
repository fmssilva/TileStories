/**
 * MORE MENU COMPONENT
 * ===================
 * 
 * Dropdown menu for overflow navigation items.
 * Appears when not all nav items fit in the header.
 * 
 * Features:
 * - Click-outside to close
 * - Keyboard navigation (Escape to close)
 * - Proper positioning below the "More" button
 * - Smooth animations
 * - Accessible markup
 */

import { useEffect, useRef } from 'react';
import { Link } from 'react-router-dom';
import { useLanguage } from '@/utils/language';
import { getNavItemLabel } from '../utils';
import { Z_INDEX, LAYOUT } from '@/design';
import type { MoreMenuProps } from '../types';

/**
 * More Menu Component
 * 
 * Dropdown showing overflow navigation items.
 * 
 * @example
 * <MoreMenu 
 *   items={overflowItems}
 *   isOpen={isOpen}
 *   onToggle={() => setIsOpen(!isOpen)}
 *   onClose={() => setIsOpen(false)}
 *   activePath={location.pathname}
 * />
 */
export function MoreMenu({
    items,
    isOpen,
    onToggle,
    onClose,
    activePath,
    componentsHeight, // DEPRECATED - kept for backward compatibility
}: MoreMenuProps) {
    const { language } = useLanguage();
    const menuRef = useRef<HTMLDivElement>(null);

    // Use FIXED sizes from LAYOUT constants (no more percentage calculations!)
    const fontSize = LAYOUT.NAV_TAB_FONT_SIZE;        // 20px - same as nav tabs
    const paddingX = LAYOUT.NAV_TAB_PADDING_X;        // 24px - same as nav tabs
    const paddingY = LAYOUT.NAV_TAB_PADDING_Y;        // 12px - same as nav tabs
    const iconSize = 16;                              // Fixed icon size for chevron

    // Suppress unused warning for deprecated param
    void componentsHeight;

    // Close on click outside
    useEffect(() => {
        if (!isOpen) return;

        const handleClickOutside = (event: MouseEvent) => {
            if (menuRef.current && !menuRef.current.contains(event.target as Node)) {
                onClose();
            }
        };

        // Small delay to avoid immediate close on toggle click
        const timeoutId = setTimeout(() => {
            document.addEventListener('mousedown', handleClickOutside);
        }, 0);

        return () => {
            clearTimeout(timeoutId);
            document.removeEventListener('mousedown', handleClickOutside);
        };
    }, [isOpen, onClose]);

    // Close on Escape key
    useEffect(() => {
        if (!isOpen) return;

        const handleEscape = (event: KeyboardEvent) => {
            if (event.key === 'Escape') {
                onClose();
            }
        };

        document.addEventListener('keydown', handleEscape);
        return () => document.removeEventListener('keydown', handleEscape);
    }, [isOpen, onClose]);

    if (items.length === 0) return null;

    return (
        <div className="relative" ref={menuRef}>
            {/* More Button */}
            <button
                onClick={onToggle}
                className={`rounded-md font-medium transition-all duration-200 whitespace-nowrap ${isOpen
                    ? 'bg-white/25 text-white'
                    : 'text-white/90 hover:bg-white/15 hover:text-white'
                    }`}
                style={{
                    fontSize: `${fontSize}px`,
                    paddingLeft: `${paddingX}px`,
                    paddingRight: `${paddingX}px`,
                    paddingTop: `${paddingY}px`,
                    paddingBottom: `${paddingY}px`,
                }}
                aria-expanded={isOpen}
                aria-haspopup="true"
            >
                More
                <svg
                    className={`inline-block ml-1 transition-transform duration-200 ${isOpen ? 'rotate-180' : ''
                        }`}
                    style={{
                        width: `${iconSize}px`,
                        height: `${iconSize}px`,
                    }}
                    fill="none"
                    stroke="currentColor"
                    viewBox="0 0 24 24"
                >
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
                </svg>
            </button>

            {/* Dropdown Menu */}
            {isOpen && (
                <div
                    className="absolute right-0 mt-2 w-56 rounded-lg shadow-xl border 
                              bg-white dark:bg-slate-800 border-gray-200 dark:border-slate-700
                              animate-in fade-in slide-in-from-top-2 duration-200"
                    style={{ zIndex: Z_INDEX.FLOATING }}
                    role="menu"
                    aria-orientation="vertical"
                >
                    <div className="py-2">
                        {items.map(item => {
                            const isActive = activePath === item.path;
                            return (
                                <Link
                                    key={item.id}
                                    to={item.path}
                                    onClick={onClose}
                                    className={`block px-4 py-2.5 text-sm font-medium transition-colors duration-150
                                              ${isActive
                                            ? 'bg-primary/10 text-primary dark:bg-primary/20 dark:text-primary-foreground'
                                            : 'text-gray-700 dark:text-gray-200 hover:bg-gray-100 dark:hover:bg-slate-700'
                                        }`}
                                    role="menuitem"
                                >
                                    {getNavItemLabel(item, language, false)}
                                </Link>
                            );
                        })}
                    </div>
                </div>
            )}
        </div>
    );
}

/**
 * USE MOBILE MENU HOOK
 * ====================
 * 
 * Manage mobile menu state (open/close/toggle).
 * Handles body scroll locking when menu is open.
 */

import { useState, useEffect, useCallback, useRef } from 'react';
import type { MobileMenuState } from '../types';

/**
 * Use Mobile Menu
 * 
 * Manages mobile menu open/close state and body scroll locking.
 * 
 * @example
 * const { isOpen, open, close, toggle } = useMobileMenu();
 * 
 * <button onClick={toggle}>Menu</button>
 * <MobileMenu isOpen={isOpen} onClose={close} />
 */
export function useMobileMenu(): MobileMenuState {
    const [isOpen, setIsOpen] = useState(false);
    const scrollPositionRef = useRef(0);

    /**
     * Lock body scroll when menu is open
     * Prevents background page from scrolling
     */
    useEffect(() => {
        if (isOpen) {
            // Save current scroll position
            scrollPositionRef.current = window.scrollY;

            // Lock scroll
            document.body.style.position = 'fixed';
            document.body.style.top = `-${scrollPositionRef.current}px`;
            document.body.style.width = '100%';

            return () => {
                // Restore scroll
                const scrollY = scrollPositionRef.current;
                document.body.style.position = '';
                document.body.style.top = '';
                document.body.style.width = '';
                window.scrollTo(0, scrollY);
            };
        }
        return undefined;
    }, [isOpen]);

    /**
     * Close menu on escape key
     */
    useEffect(() => {
        const handleEscape = (e: KeyboardEvent) => {
            if (e.key === 'Escape' && isOpen) {
                setIsOpen(false);
            }
        };

        window.addEventListener('keydown', handleEscape);
        return () => window.removeEventListener('keydown', handleEscape);
    }, [isOpen]);

    const open = useCallback(() => setIsOpen(true), []);
    const close = useCallback(() => setIsOpen(false), []);
    const toggle = useCallback(() => setIsOpen(prev => !prev), []);

    return {
        isOpen,
        open,
        close,
        toggle,
    };
}

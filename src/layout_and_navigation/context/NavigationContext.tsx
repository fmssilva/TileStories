/**
 * NAVIGATION CONTEXT
 * ==================
 * 
 * Central state management for navigation system.
 * 
 * Manages:
 * - Current path and navigation history
 * - Breadcrumb trails
 * - Scroll position restoration
 * - Navigation metadata
 * 
 * This is the "brain" that coordinates all navigation features.
 */

import { createContext, useContext, useState, useEffect, useRef, ReactNode, useCallback } from 'react';
import { useLocation, useNavigationType } from 'react-router-dom';
import { navigationConfig } from '../config';
import { findNavItemByPath, buildBreadcrumbTrail } from '../utils';
import type { NavigationState } from '../types';

// ============================================================================
// CONTEXT DEFINITION
// ============================================================================

interface NavigationContextType extends NavigationState {
    /** Add entry to navigation history */
    addToHistory: (path: string, state?: unknown) => void;
    /** Get the path to return to (for after-auth flows) */
    getReturnPath: () => string | null;
    /** Save scroll position for a path */
    saveScrollPosition: (path: string, position: number) => void;
    /** Get saved scroll position for a path */
    getScrollPosition: (path: string) => number | null;
}

const NavigationContext = createContext<NavigationContextType | null>(null);

// ============================================================================
// PROVIDER COMPONENT
// ============================================================================

interface NavigationProviderProps {
    children: ReactNode;
}

/**
 * Navigation Provider
 * 
 * Wrap your app (inside BrowserRouter) with this provider
 * to enable navigation context features.
 * 
 * @example
 * <BrowserRouter>
 *   <NavigationProvider>
 *     <App />
 *   </NavigationProvider>
 * </BrowserRouter>
 */
export function NavigationProvider({ children }: NavigationProviderProps) {
    const location = useLocation();
    const navigationType = useNavigationType();

    // Initialize state
    const [state, setState] = useState<NavigationState>({
        currentPath: location.pathname,
        breadcrumbTrail: [],
        navigationHistory: [],
        previousPath: null,
        scrollPositions: new Map<string, number>(),
    });

    // Track last scroll Y for history
    const [lastScrollY, setLastScrollY] = useState(0);

    // Track navigation to prevent scroll fighting
    const navigationIdRef = useRef<string>('');
    const hasScrolledRef = useRef<boolean>(false);

    // Update state when location changes
    useEffect(() => {
        // Create unique navigation ID
        const navId = `${navigationType}-${location.pathname}-${Date.now()}`;
        const isNewNavigation = navigationIdRef.current !== navId;

        if (isNewNavigation) {
            navigationIdRef.current = navId;
            hasScrolledRef.current = false;
        }


        const currentNavItem = findNavItemByPath(navigationConfig, location.pathname);
        const trail = buildBreadcrumbTrail(currentNavItem);

        setState(prev => {
            // Only add to history if it's a PUSH navigation (not POP/REPLACE)
            const newHistory = navigationType === 'PUSH'
                ? [...prev.navigationHistory, {
                    path: prev.currentPath,
                    timestamp: Date.now(),
                    scrollPosition: lastScrollY,
                    state: location.state,
                }].slice(-50) // Keep last 50 entries to prevent memory issues
                : prev.navigationHistory;

            return {
                ...prev,
                previousPath: prev.currentPath,
                currentPath: location.pathname,
                breadcrumbTrail: trail,
                navigationHistory: newHistory,
            };
        });

        // Handle scroll restoration
        if (navigationType === 'POP') {
            // User hit back/forward button - restore scroll position
            const savedPosition = state.scrollPositions.get(location.pathname);

            if (savedPosition !== undefined && !hasScrolledRef.current) {
                hasScrolledRef.current = true;
                // Use setTimeout to ensure DOM is ready
                setTimeout(() => {
                    window.scrollTo(0, savedPosition);
                }, 0);
            }
        } else {
            // New navigation (PUSH/REPLACE) - scroll to top ONCE
            if (!hasScrolledRef.current) {
                hasScrolledRef.current = true;

                // Immediate scroll
                window.scrollTo(0, 0);

                // Check if scroll actually happened
                setTimeout(() => {
                    if (window.scrollY !== 0) {
                        // Try one more time
                        window.scrollTo(0, 0);
                    }
                }, 100);
            }
        }
    }, [location, navigationType, state.scrollPositions]);

    // Track scroll position for current page
    useEffect(() => {
        const handleScroll = () => {
            setLastScrollY(window.scrollY);
        };

        window.addEventListener('scroll', handleScroll, { passive: true });
        return () => window.removeEventListener('scroll', handleScroll);
    }, []);

    // Context methods
    const addToHistory = useCallback((path: string, customState?: unknown) => {
        setState(prev => ({
            ...prev,
            navigationHistory: [...prev.navigationHistory, {
                path,
                timestamp: Date.now(),
                scrollPosition: window.scrollY,
                state: customState,
            }].slice(-50),
        }));
    }, []);

    const getReturnPath = useCallback((): string | null => {
        if (state.navigationHistory.length === 0) return null;
        const lastEntry = state.navigationHistory[state.navigationHistory.length - 1];
        return lastEntry ? lastEntry.path : null;
    }, [state.navigationHistory]);

    const saveScrollPosition = useCallback((path: string, position: number) => {
        setState(prev => {
            const newPositions = new Map(prev.scrollPositions);
            newPositions.set(path, position);
            return { ...prev, scrollPositions: newPositions };
        });
    }, []);

    const getScrollPosition = useCallback((path: string): number | null => {
        return state.scrollPositions.get(path) ?? null;
    }, [state.scrollPositions]);

    const contextValue: NavigationContextType = {
        ...state,
        addToHistory,
        getReturnPath,
        saveScrollPosition,
        getScrollPosition,
    };

    return (
        <NavigationContext.Provider value={contextValue}>
            {children}
        </NavigationContext.Provider>
    );
}

// ============================================================================
// HOOK TO USE CONTEXT
// ============================================================================

/**
 * Use Navigation Context
 * 
 * Access navigation state and methods from any component.
 * Must be used inside NavigationProvider.
 * 
 * @throws Error if used outside NavigationProvider
 */
export function useNavigationContext(): NavigationContextType {
    const context = useContext(NavigationContext);

    if (!context) {
        throw new Error('useNavigationContext must be used within NavigationProvider');
    }

    return context;
}

/**
 * USE NAVIGATION HOOK
 * ===================
 * 
 * Main hook for programmatic navigation.
 * Clean API that wraps React Router and Navigation Context.
 * 
 * Use this instead of useNavigate directly for:
 * - Automatic scroll position saving
 * - History tracking
 * - Return-after-auth flows
 * - Convenient helper methods
 */

import { useNavigate, useLocation } from 'react-router-dom';
import { useNavigationContext } from '../context';
import type { NavigationAPI, NavigateOptions } from '../types';

/**
 * Main navigation hook
 * 
 * Provides clean API for all navigation operations.
 * 
 * @example
 * const { goTo, goBack, canGoBack } = useNavigation();
 * 
 * // Navigate to a page
 * goTo('/products');
 * 
 * // Go back
 * if (canGoBack) {
 *   goBack();
 * } else {
 *   goBackOrHome(); // Goes home if no history
 * }
 * 
 * // Auth flow
 * navigateWithReturn('/login'); // Saves current page
 * // ...after login...
 * returnFromAuth(); // Returns to saved page
 */
export function useNavigation(): NavigationAPI {
    const navigate = useNavigate();
    const location = useLocation();
    const navContext = useNavigationContext();

    /**
     * Navigate to a path
     * 
     * @param path - Target path
     * @param options - Navigation options
     */
    const goTo = (path: string, options?: NavigateOptions) => {
        // Save current scroll position unless explicitly disabled
        if (!options?.preserveScroll) {
            navContext.saveScrollPosition(location.pathname, window.scrollY);
        }

        const navigateOptions: { state?: unknown; replace?: boolean } = {};
        if (options?.state !== undefined) {
            navigateOptions.state = options.state;
        }
        if (options?.replace !== undefined) {
            navigateOptions.replace = options.replace;
        }

        navigate(path, navigateOptions);
    };

    /**
     * Go back one step in history
     * Saves current scroll position before navigating
     */
    const goBack = () => {
        navContext.saveScrollPosition(location.pathname, window.scrollY);
        navigate(-1);
    };

    /**
     * Go back if history exists, otherwise go home
     * Useful for "back" buttons that should always work
     */
    const goBackOrHome = () => {
        const returnPath = navContext.getReturnPath();
        if (returnPath) {
            navigate(-1);
        } else {
            navigate('/');
        }
    };

    /**
     * Navigate with return path for auth flows
     * Saves current location to return to after auth
     * 
     * @example
     * // User tries to access protected page
     * navigateWithReturn('/login');
     * // After login, call returnFromAuth()
     */
    const navigateWithReturn = (path: string) => {
        navigate(path, {
            state: { returnTo: location.pathname }
        });
    };

    /**
     * Return from auth to original page
     * Uses saved returnTo path or goes home
     */
    const returnFromAuth = () => {
        const state = location.state as { returnTo?: string } | null;
        const returnTo = state?.returnTo || '/';
        navigate(returnTo, { replace: true });
    };

    return {
        goTo,
        goBack,
        goBackOrHome,
        navigateWithReturn,
        returnFromAuth,
        currentPath: navContext.currentPath,
        previousPath: navContext.previousPath,
        canGoBack: navContext.navigationHistory.length > 0,
    };
}

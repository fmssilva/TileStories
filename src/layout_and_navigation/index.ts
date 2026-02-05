// ============================================================================
// LAYOUT AND NAVIGATION - DOMAIN EXPORTS
// ============================================================================

/**
 * Centralized exports for layout and navigation components
 * Following domain-centered architecture patterns
 */

// Core layout components
export { Header } from './header/Header';
export { Footer } from './footer/Footer';
export { BackToTop } from './BackToTopIcon';

// Hooks and utilities
export { useHeaderScroll } from './hooks';

// Color system and styling utilities
export {
    headerColors,
    getHeaderStyles,
    getFooterStyles,
    getNavLinkStyles
} from './colors';// Types (if needed in future)
// export type { HeaderProps, FooterProps } from './types';
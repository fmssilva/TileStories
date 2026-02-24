/**
 * MODAL COMPONENT - Production-Ready Implementation
 * ==================================================
 * 
 * A comprehensive, accessible modal component with composition pattern.
 * 
 * ✅ KEY FEATURES:
 * ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
 * 
 * Core Functionality:
 * • Portal rendering outside DOM hierarchy
 * • Focus trap with keyboard navigation
 * • Focus restoration on close
 * • Body scroll locking (including iOS support)
 * • ESC key to close (configurable)
 * • Backdrop click to close (configurable)
 * • Visible close button (configurable)
 * 
 * Advanced Features:
 * • Loading state with overlay
 * • Close confirmation callback (onBeforeClose)
 * • Initial focus control
 * • Multiple animation variants
 * • Backdrop blur & opacity control
 * • Mobile fullscreen mode
 * • Transition callbacks
 * 
 * Layout & Sizing (NEW & IMPROVED):
 * • Flexible size presets (sm, md, lg, xl, full, auto, content-fit)
 * • Custom viewport padding control (spacing around modal)
 * • Max width and max height overrides
 * • Content-fit mode for alerts and small dialogs
 * • Proper scroll handling with overflow detection
 * • Scroll indicators (top/bottom gradients)
 * 
 * Accessibility:
 * • Full ARIA compliance
 * • Keyboard navigation
 * • Screen reader support
 * • Focus management
 * 
 * ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
 * 
 * 📖 USAGE EXAMPLES:
 * 
 * Basic Modal:
 * ```tsx
 * <Modal isOpen={isOpen} onClose={() => setIsOpen(false)}>
 *   <Modal.Header><h2>Title</h2></Modal.Header>
 *   <Modal.Content>Content here</Modal.Content>
 *   <Modal.Footer>Actions</Modal.Footer>
 * </Modal>
 * ```
 * 
 * Small Alert Dialog (content-fit):
 * ```tsx
 * <Modal 
 *   isOpen={isOpen} 
 *   onClose={onClose}
 *   size="content-fit"
 *   maxWidth="400px"
 * >
 *   <Modal.Content>
 *     <div className="text-center py-4">
 *       <p>Are you sure you want to delete this item?</p>
 *     </div>
 *   </Modal.Content>
 *   <Modal.Footer>
 *     <div className="flex gap-2 justify-end">
 *       <button onClick={onClose}>Cancel</button>
 *       <button onClick={handleDelete}>Delete</button>
 *     </div>
 *   </Modal.Footer>
 * </Modal>
 * ```
 * 
 * Custom Viewport Padding:
 * ```tsx
 * <Modal 
 *   isOpen={isOpen} 
 *   onClose={onClose}
 *   viewportPadding="2rem" // Space around modal
 *   maxHeight="80vh" // Limit height
 * >
 *   <Modal.Content>Long scrollable content</Modal.Content>
 * </Modal>
 * ```
 * 
 * With Scroll Indicators:
 * ```tsx
 * <Modal isOpen={isOpen} onClose={onClose}>
 *   <Modal.Header sticky>Title</Modal.Header>
 *   <Modal.Content showScrollIndicator>
 *     Very long content that needs scrolling...
 *   </Modal.Content>
 *   <Modal.Footer sticky>Actions</Modal.Footer>
 * </Modal>
 * ```
 */

import { ReactNode, useEffect, useCallback, useRef, useState } from 'react';
import { createPortal } from 'react-dom';
import { cn } from '@/utils';
import { Z_INDEX } from '@/design';

// ═══════════════════════════════════════════════════════════════════════════
// TYPES
// ═══════════════════════════════════════════════════════════════════════════

type ModalSize = 'sm' | 'md' | 'lg' | 'xl' | 'full' | 'auto' | 'content-fit';
type AnimationType = 'fade' | 'slide-up' | 'slide-down' | 'scale' | 'none';
type BackdropBlur = 'none' | 'sm' | 'md' | 'lg';

interface ModalProps {
    /** Controls modal visibility */
    isOpen: boolean;
    /** Callback when modal should close */
    onClose: () => void;
    /** Modal content (use Modal.Header, Modal.Content, Modal.Footer) */
    children: ReactNode;

    // Size & Layout
    /** Modal size preset */
    size?: ModalSize;
    /** Custom max width (e.g., "500px", "50rem") - overrides size preset */
    maxWidth?: string;
    /** Custom max height (e.g., "600px", "80vh") - default: 90vh */
    maxHeight?: string;
    /** Padding around modal within viewport (e.g., "1rem", "32px") - default: 1rem (16px) */
    viewportPadding?: string;

    // Styling
    /** Additional CSS classes for modal container */
    className?: string;
    /** Animation variant */
    animation?: AnimationType;
    /** Animation duration in ms (default: 200) */
    animationDuration?: number;
    /** Backdrop opacity (0-100, default: 60) */
    backdropOpacity?: number;
    /** Backdrop blur intensity */
    backdropBlur?: BackdropBlur;

    // Behavior
    /** Whether clicking backdrop closes the modal (default: true) */
    closeOnBackdropClick?: boolean;
    /** Whether ESC key closes the modal (default: true) */
    closeOnEscape?: boolean;
    /** Show close button in top-right corner (default: true) */
    showCloseButton?: boolean;
    /** Force fullscreen on mobile devices */
    fullscreenMobile?: boolean;

    // Advanced Features
    /** Show loading overlay (default: false) */
    isLoading?: boolean;
    /** Custom loading content (default: spinner with text) */
    loadingContent?: ReactNode;
    /** Callback to check if modal can close (return false to prevent) */
    onBeforeClose?: () => boolean | Promise<boolean>;
    /** CSS selector for element to focus on open */
    initialFocus?: string;
    /** Callback after modal opens */
    onAfterOpen?: () => void;
    /** Callback after modal closes */
    onAfterClose?: () => void;

    // Accessibility
    /** Accessible label for screen readers */
    ariaLabel?: string;
    /** Z-index for modal (default: Z_INDEX.MODAL = 3100) */
    zIndex?: number;
}

interface ModalContentProps {
    children: ReactNode;
    /** Additional CSS classes */
    className?: string;
    /** Disable default padding */
    noPadding?: boolean;
    /** Show scroll indicators (gradients) when content overflows */
    showScrollIndicator?: boolean;
}

interface ModalHeaderProps {
    children: ReactNode;
    /** Keep header sticky at top while content scrolls */
    sticky?: boolean;
    /** Additional CSS classes */
    className?: string;
}

interface ModalFooterProps {
    children: ReactNode;
    /** Keep footer sticky at bottom while content scrolls */
    sticky?: boolean;
    /** Additional CSS classes */
    className?: string;
}

// ═══════════════════════════════════════════════════════════════════════════
// HOOKS
// ═══════════════════════════════════════════════════════════════════════════

/**
 * Focus trap - keeps keyboard navigation inside modal
 */
function useFocusTrap(
    isOpen: boolean,
    containerRef: React.RefObject<HTMLDivElement | null>,
    initialFocus?: string
) {
    useEffect(() => {
        if (!isOpen || !containerRef.current) return;

        const container = containerRef.current;
        const focusableElements = container.querySelectorAll<HTMLElement>(
            'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
        );

        const firstElement = focusableElements[0];
        const lastElement = focusableElements[focusableElements.length - 1];

        // Focus specific element or first focusable element
        const elementToFocus = initialFocus
            ? container.querySelector<HTMLElement>(initialFocus)
            : firstElement;

        elementToFocus?.focus();

        const handleTab = (e: KeyboardEvent) => {
            if (e.key !== 'Tab') return;

            if (e.shiftKey) {
                // Shift + Tab
                if (document.activeElement === firstElement) {
                    e.preventDefault();
                    lastElement?.focus();
                }
            } else {
                // Tab
                if (document.activeElement === lastElement) {
                    e.preventDefault();
                    firstElement?.focus();
                }
            }
        };

        container.addEventListener('keydown', handleTab);
        return () => container.removeEventListener('keydown', handleTab);
    }, [isOpen, containerRef, initialFocus]);
}

/**
 * Body scroll lock - prevents background scrolling
 */
function useScrollLock(isOpen: boolean) {
    useEffect(() => {
        if (!isOpen) return;

        const originalOverflow = document.body.style.overflow;
        const originalPaddingRight = document.body.style.paddingRight;

        // Calculate scrollbar width to prevent layout shift
        const scrollbarWidth = window.innerWidth - document.documentElement.clientWidth;

        document.body.style.overflow = 'hidden';
        if (scrollbarWidth > 0) {
            document.body.style.paddingRight = `${scrollbarWidth}px`;
        }

        // Prevent iOS scroll
        const preventScroll = (e: TouchEvent) => {
            const target = e.target as HTMLElement;
            const modalContent = target.closest('[data-modal-scrollable]');
            if (!modalContent) {
                e.preventDefault();
            }
        };

        document.addEventListener('touchmove', preventScroll, { passive: false });

        return () => {
            document.body.style.overflow = originalOverflow;
            document.body.style.paddingRight = originalPaddingRight;
            document.removeEventListener('touchmove', preventScroll);
        };
    }, [isOpen]);
}

/**
 * Focus restoration - returns focus to trigger element on close
 */
function useFocusRestore(isOpen: boolean) {
    const previousActiveElement = useRef<HTMLElement | null>(null);

    useEffect(() => {
        if (isOpen) {
            previousActiveElement.current = document.activeElement as HTMLElement;
        } else if (previousActiveElement.current) {
            previousActiveElement.current.focus();
            previousActiveElement.current = null;
        }
    }, [isOpen]);
}

// ═══════════════════════════════════════════════════════════════════════════
// MAIN MODAL COMPONENT
// ═══════════════════════════════════════════════════════════════════════════

export function Modal({
    isOpen,
    onClose,
    children,
    size = 'lg',
    maxWidth,
    maxHeight = '90vh',
    viewportPadding = '1rem',
    className = '',
    closeOnBackdropClick = true,
    closeOnEscape = true,
    ariaLabel = 'Dialog',
    zIndex = Z_INDEX.MODAL,
    showCloseButton = true,
    isLoading = false,
    loadingContent,
    onBeforeClose,
    initialFocus,
    animation = 'scale',
    animationDuration = 200,
    backdropOpacity = 60,
    backdropBlur = 'sm',
    fullscreenMobile = false,
    onAfterOpen,
    onAfterClose,
}: ModalProps) {
    const modalRef = useRef<HTMLDivElement>(null);

    // Apply hooks
    useScrollLock(isOpen);
    useFocusRestore(isOpen);
    useFocusTrap(isOpen, modalRef, initialFocus);

    // Transition callbacks
    useEffect(() => {
        if (isOpen) {
            onAfterOpen?.();
        } else {
            onAfterClose?.();
        }
    }, [isOpen, onAfterOpen, onAfterClose]);

    // ─────────────────────────────────────────────────────────────────────────
    // CLOSE HANDLER
    // ─────────────────────────────────────────────────────────────────────────

    const handleClose = useCallback(async () => {
        if (onBeforeClose) {
            const canClose = await onBeforeClose();
            if (!canClose) return;
        }
        onClose();
    }, [onBeforeClose, onClose]);

    // ─────────────────────────────────────────────────────────────────────────
    // ESC KEY HANDLER
    // ─────────────────────────────────────────────────────────────────────────

    const handleEscapeKey = useCallback(
        (event: KeyboardEvent) => {
            if (closeOnEscape && event.key === 'Escape') {
                handleClose();
            }
        },
        [closeOnEscape, handleClose]
    );

    useEffect(() => {
        if (isOpen && closeOnEscape) {
            document.addEventListener('keydown', handleEscapeKey);
            return () => document.removeEventListener('keydown', handleEscapeKey);
        }
        return undefined;
    }, [isOpen, closeOnEscape, handleEscapeKey]);

    // ─────────────────────────────────────────────────────────────────────────
    // BACKDROP CLICK HANDLER
    // ─────────────────────────────────────────────────────────────────────────

    const handleBackdropClick = () => {
        if (closeOnBackdropClick) {
            handleClose();
        }
    };

    // ─────────────────────────────────────────────────────────────────────────
    // SIZE & STYLE CONFIGURATION
    // ─────────────────────────────────────────────────────────────────────────

    const sizeClasses = {
        'sm': 'max-w-md',           // ~448px
        'md': 'max-w-2xl',          // ~672px
        'lg': 'max-w-4xl',          // ~896px
        'xl': 'max-w-6xl',          // ~1152px
        'full': 'max-w-[95vw]',     // Almost full width
        'auto': 'max-w-fit',        // Fits content, but can grow
        'content-fit': 'w-auto',    // Minimal width, wraps content tightly
    };

    const animationClasses = {
        'fade': 'animate-in fade-in',
        'slide-up': 'animate-in slide-in-from-bottom-4',
        'slide-down': 'animate-in slide-in-from-top-4',
        'scale': 'animate-in zoom-in-95',
        'none': '',
    };

    const backdropBlurClasses = {
        'none': '',
        'sm': 'backdrop-blur-sm',
        'md': 'backdrop-blur-md',
        'lg': 'backdrop-blur-lg',
    };

    // Determine width behavior
    const widthClass = size === 'content-fit'
        ? 'w-auto min-w-[300px]'  // Minimal but readable
        : `w-full ${sizeClasses[size]}`;

    // Custom max width override
    const maxWidthStyle = maxWidth ? { maxWidth } : {};


    // ─────────────────────────────────────────────────────────────────────────
    // RENDER
    // ─────────────────────────────────────────────────────────────────────────

    if (!isOpen) return null;

    const modalContent = (
        <div
            className="fixed inset-0 overflow-hidden animate-in fade-in"
            style={{
                zIndex,
                animationDuration: `${animationDuration}ms`
            }}
        >
            {/* Backdrop */}
            <div
                className={cn(
                    'absolute inset-0 transition-opacity',
                    backdropBlurClasses[backdropBlur]
                )}
                style={{
                    backgroundColor: `rgba(0, 0, 0, ${backdropOpacity / 100})`
                }}
                onClick={handleBackdropClick}
                aria-hidden="true"
            />

            {/* Modal Container - Centered with padding */}
            <div
                className="absolute inset-0 flex items-center justify-center"
                style={{ padding: viewportPadding }}
                onClick={handleBackdropClick}
            >
                {/* Modal Dialog */}
                <div
                    ref={modalRef}
                    className={cn(
                        // Base styles
                        'relative',
                        widthClass,
                        'bg-white dark:bg-gray-900',
                        'rounded-2xl shadow-2xl',
                        'border border-gray-200 dark:border-gray-700',
                        'overflow-hidden',
                        'flex flex-col',
                        // CRITICAL: min-h-0 allows flex children to shrink and enable scrolling
                        'min-h-0',
                        // Animation
                        animationClasses[animation],
                        // Mobile fullscreen
                        fullscreenMobile && 'max-md:!max-w-none max-md:!max-h-screen max-md:!rounded-none max-md:!m-0',
                        // Focus
                        'focus:outline-none',
                        // Custom classes
                        className
                    )}
                    style={{
                        animationDuration: `${animationDuration}ms`,
                        maxHeight: fullscreenMobile ? undefined : maxHeight,
                        ...maxWidthStyle
                    }}
                    role="dialog"
                    aria-modal="true"
                    aria-label={ariaLabel}
                    onClick={(e) => e.stopPropagation()}
                    tabIndex={-1}
                >
                    {/* Close Button */}
                    {showCloseButton && (
                        <button
                            onClick={handleClose}
                            className={cn(
                                'absolute top-4 right-4',
                                'p-2 rounded-lg',
                                'text-gray-500 hover:text-gray-700',
                                'dark:text-gray-400 dark:hover:text-gray-200',
                                'hover:bg-gray-100 dark:hover:bg-gray-800',
                                'transition-colors',
                                'focus:outline-none focus:ring-2 focus:ring-blue-500',
                                'z-50'
                            )}
                            aria-label="Close modal"
                            type="button"
                        >
                            <svg
                                className="w-5 h-5"
                                fill="none"
                                stroke="currentColor"
                                viewBox="0 0 24 24"
                            >
                                <path
                                    strokeLinecap="round"
                                    strokeLinejoin="round"
                                    strokeWidth={2}
                                    d="M6 18L18 6M6 6l12 12"
                                />
                            </svg>
                        </button>
                    )}

                    {/* Loading Overlay */}
                    {isLoading && (
                        <div
                            className="absolute inset-0 bg-white/80 dark:bg-gray-900/80 flex items-center justify-center rounded-2xl z-50"
                        >
                            {loadingContent || (
                                <div className="flex flex-col items-center gap-3">
                                    <div className="w-12 h-12 border-4 border-gray-300 dark:border-gray-600 border-t-blue-500 rounded-full animate-spin" />
                                    <p className="text-sm text-gray-600 dark:text-gray-400">Loading...</p>
                                </div>
                            )}
                        </div>
                    )}

                    {children}
                </div>
            </div>
        </div>
    );

    return createPortal(modalContent, document.body);
}

// ═══════════════════════════════════════════════════════════════════════════
// MODAL HEADER
// ═══════════════════════════════════════════════════════════════════════════

Modal.Header = function ModalHeader({ children, sticky = false, className = '' }: ModalHeaderProps) {
    return (
        <div
            className={cn(
                'bg-white dark:bg-gray-900',
                'border-b border-gray-200 dark:border-gray-700',
                'px-6 py-4',
                sticky && 'sticky top-0 z-10',
                className
            )}
        >
            {children}
        </div>
    );
};

// ═══════════════════════════════════════════════════════════════════════════
// MODAL CONTENT
// ═══════════════════════════════════════════════════════════════════════════

Modal.Content = function ModalContent({
    children,
    className = '',
    noPadding = false,
    showScrollIndicator = false,
}: ModalContentProps) {
    const contentRef = useRef<HTMLDivElement>(null);
    const wrapperRef = useRef<HTMLDivElement>(null);
    const [scrollState, setScrollState] = useState({
        hasScroll: false,
        isScrolledToBottom: false,
        isScrolledToTop: true,
    });

    useEffect(() => {
        const element = contentRef.current;
        if (!element) return;

        const checkScroll = () => {
            const hasScroll = element.scrollHeight > element.clientHeight;
            const isScrolledToTop = element.scrollTop < 10;
            const isScrolledToBottom =
                Math.abs(element.scrollHeight - element.scrollTop - element.clientHeight) < 10;

            setScrollState({
                hasScroll,
                isScrolledToBottom,
                isScrolledToTop,
            });
        };

        checkScroll();
        element.addEventListener('scroll', checkScroll);

        // Also check on resize and content changes
        const resizeObserver = new ResizeObserver(checkScroll);
        resizeObserver.observe(element);

        return () => {
            element.removeEventListener('scroll', checkScroll);
            resizeObserver.disconnect();
        };
    }, [children]);

    return (
        <div ref={wrapperRef} className="flex-1 min-h-0 overflow-hidden relative flex flex-col">
            {/* Top scroll indicator */}
            {showScrollIndicator && scrollState.hasScroll && !scrollState.isScrolledToTop && (
                <div className="absolute top-0 left-0 right-0 h-8 bg-gradient-to-b from-white dark:from-gray-900 to-transparent pointer-events-none z-10" />
            )}

            {/* Scrollable content */}
            <div
                ref={contentRef}
                className={cn(
                    'flex-1 min-h-0 overflow-y-auto overflow-x-hidden',
                    // Styled scrollbar
                    'scrollbar-thin scrollbar-thumb-gray-400 dark:scrollbar-thumb-gray-600',
                    'scrollbar-track-gray-100 dark:scrollbar-track-gray-800',
                    'scrollbar-thumb-rounded-full scrollbar-track-rounded-full',
                    !noPadding && 'p-6',
                    className
                )}
                data-modal-scrollable
            >
                {children}
            </div>

            {/* Bottom scroll indicator */}
            {showScrollIndicator && scrollState.hasScroll && !scrollState.isScrolledToBottom && (
                <div className="absolute bottom-0 left-0 right-0 h-8 bg-gradient-to-t from-white dark:from-gray-900 to-transparent pointer-events-none z-10" />
            )}
        </div>
    );
};

// ═══════════════════════════════════════════════════════════════════════════
// MODAL FOOTER
// ═══════════════════════════════════════════════════════════════════════════

Modal.Footer = function ModalFooter({ children, sticky = false, className = '' }: ModalFooterProps) {
    return (
        <div
            className={cn(
                'bg-white dark:bg-gray-900',
                'border-t border-gray-200 dark:border-gray-700',
                'px-6 py-4',
                sticky && 'sticky bottom-0 z-10',
                className
            )}
        >
            {children}
        </div>
    );
};
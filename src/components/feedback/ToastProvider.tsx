/**
 * TOAST PROVIDER
 * ==============
 * 
 * Context provider for toast notifications
 * 
 * Manages a stack of toast notifications and renders them in a fixed container
 * Use with the useToast hook to show toasts from anywhere in your app
 * 
 * Usage:
 * ```tsx
 * // In App.tsx or root layout
 * <ToastProvider>
 *   <YourApp />
 * </ToastProvider>
 * 
 * // In any component
 * const toast = useToast();
 * toast.success('Operation completed!');
 * ```
 */

import { createContext, ReactNode, useContext, useState, useCallback } from 'react';
import { createPortal } from 'react-dom';
import { Toast } from './Toast';
import type { Toast as ToastType, ToastOptions } from './types';
import { Z_INDEX } from '@/design';

interface ToastContextType {
    toasts: ToastType[];
    addToast: (options: ToastOptions) => void;
    removeToast: (id: string) => void;
}

const ToastContext = createContext<ToastContextType | undefined>(undefined);

export function useToastContext() {
    const context = useContext(ToastContext);
    if (!context) {
        throw new Error('useToastContext must be used within ToastProvider');
    }
    return context;
}

interface ToastProviderProps {
    children: ReactNode;
}

export function ToastProvider({ children }: ToastProviderProps) {
    const [toasts, setToasts] = useState<ToastType[]>([]);

    const addToast = useCallback((options: ToastOptions) => {
        const id = Math.random().toString(36).substr(2, 9);
        const newToast: ToastType = {
            id,
            ...(options.title && { title: options.title }),
            message: options.message,
            variant: options.variant || 'info',
            duration: options.duration || 5000,
        };
        setToasts((prev) => [...prev, newToast]);
    }, []);

    const removeToast = useCallback((id: string) => {
        setToasts((prev) => prev.filter((toast) => toast.id !== id));
    }, []);

    const contextValue: ToastContextType = {
        toasts,
        addToast,
        removeToast,
    };

    return (
        <ToastContext.Provider value={contextValue}>
            {children}
            {createPortal(
                <div
                    className="fixed top-0 right-0 flex flex-col gap-3 p-4 pointer-events-none"
                    style={{ zIndex: Z_INDEX.SYSTEM }}
                    aria-live="polite"
                    aria-atomic="false"
                >
                    {toasts.map((toast) => (
                        <Toast key={toast.id} {...toast} onClose={removeToast} />
                    ))}
                </div>,
                document.body
            )}
        </ToastContext.Provider>
    );
}

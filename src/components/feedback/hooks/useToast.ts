/**
 * USE TOAST HOOK
 * ==============
 * 
 * Hook to trigger toast notifications from anywhere in your app
 * 
 * Must be used within a ToastProvider
 * 
 * Usage:
 * ```tsx
 * function MyComponent() {
 *   const toast = useToast();
 * 
 *   const handleSave = async () => {
 *     try {
 *       await saveData();
 *       toast.success('Data saved successfully!');
 *     } catch (error) {
 *       toast.error('Failed to save data');
 *     }
 *   };
 * 
 *   return <button onClick={handleSave}>Save</button>;
 * }
 * ```
 */

import { useToastContext } from '../ToastProvider';
import type { ToastOptions } from '../types';

export function useToast() {
    const { addToast } = useToastContext();

    return {
        /** Show an info toast */
        info: (message: string, options?: Omit<ToastOptions, 'message' | 'variant'>) => {
            addToast({ message, variant: 'info', ...options });
        },

        /** Show a success toast */
        success: (message: string, options?: Omit<ToastOptions, 'message' | 'variant'>) => {
            addToast({ message, variant: 'success', ...options });
        },

        /** Show a warning toast */
        warning: (message: string, options?: Omit<ToastOptions, 'message' | 'variant'>) => {
            addToast({ message, variant: 'warning', ...options });
        },

        /** Show an error toast */
        error: (message: string, options?: Omit<ToastOptions, 'message' | 'variant'>) => {
            addToast({ message, variant: 'error', ...options });
        },

        /** Show a custom toast with all options */
        custom: (options: ToastOptions) => {
            addToast(options);
        },
    };
}

/**
 * FEEDBACK TYPES
 * ==============
 * 
 * Shared TypeScript types for the feedback system
 */

// Feedback variant types
export type FeedbackVariant = 'info' | 'success' | 'warning' | 'error';

// Toast notification types
export interface Toast {
    id: string;
    title?: string;
    message: string;
    variant: FeedbackVariant;
    duration?: number; // milliseconds
}

export interface ToastOptions {
    title?: string;
    message: string;
    variant?: FeedbackVariant;
    duration?: number;
}

// Async state types
export interface AsyncState<T> {
    data: T | null;
    loading: boolean;
    error: Error | null;
}

export interface AsyncOptions {
    onSuccess?: () => void;
    onError?: (error: Error) => void;
}

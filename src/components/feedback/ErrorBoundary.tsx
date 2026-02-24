/**
 * ERROR BOUNDARY COMPONENT
 * ========================
 * 
 * React Error Boundary to catch and handle errors gracefully
 * 
 * Features:
 * - Catches JavaScript errors in child component tree
 * - Displays fallback UI instead of crashing the app
 * - Logs error details for debugging
 * - Optional custom fallback component
 * - Reset functionality
 * 
 * Usage:
 * ```tsx
 * <ErrorBoundary>
 *   <YourComponent />
 * </ErrorBoundary>
 * 
 * // With custom fallback
 * <ErrorBoundary fallback={<CustomErrorUI />}>
 *   <YourComponent />
 * </ErrorBoundary>
 * ```
 */

import { Component, ReactNode } from 'react';
import { Alert } from './Alert';

interface ErrorBoundaryProps {
    children: ReactNode;
    /** Custom fallback UI to show when an error occurs */
    fallback?: ReactNode;
    /** Callback when an error is caught */
    onError?: (error: Error, errorInfo: React.ErrorInfo) => void;
}

interface ErrorBoundaryState {
    hasError: boolean;
    error: Error | null;
}

export class ErrorBoundary extends Component<ErrorBoundaryProps, ErrorBoundaryState> {
    constructor(props: ErrorBoundaryProps) {
        super(props);
        this.state = {
            hasError: false,
            error: null,
        };
    }

    static getDerivedStateFromError(error: Error): ErrorBoundaryState {
        return {
            hasError: true,
            error,
        };
    }

    override componentDidCatch(error: Error, errorInfo: React.ErrorInfo) {
        console.error('ErrorBoundary caught an error:', error, errorInfo);
        this.props.onError?.(error, errorInfo);
    }

    handleReset = () => {
        this.setState({
            hasError: false,
            error: null,
        });
    };

    override render() {
        if (this.state.hasError) {
            if (this.props.fallback) {
                return this.props.fallback;
            }

            // Default fallback UI
            return (
                <div className="container mx-auto px-4 py-8 max-w-2xl">
                    <Alert variant="error" title="Something went wrong">
                        <p className="mb-4">
                            An unexpected error occurred. We apologize for the inconvenience.
                        </p>
                        {this.state.error && (
                            <details className="mt-4">
                                <summary className="cursor-pointer font-semibold mb-2">
                                    Technical details
                                </summary>
                                <pre className="text-xs bg-red-900/20 dark:bg-red-900/40 p-3 rounded overflow-auto">
                                    {this.state.error.toString()}
                                </pre>
                            </details>
                        )}
                        <button
                            onClick={this.handleReset}
                            className="mt-4 px-4 py-2 bg-red-600 hover:bg-red-700 text-white rounded-lg transition-colors"
                        >
                            Try again
                        </button>
                    </Alert>
                </div>
            );
        }

        return this.props.children;
    }
}

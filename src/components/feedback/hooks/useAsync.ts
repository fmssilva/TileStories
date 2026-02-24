/**
 * USE ASYNC HOOK
 * ==============
 * 
 * Hook for managing async operations with loading, error, and data states
 * 
 * Features:
 * - Automatic loading state management
 * - Error handling
 * - Success callbacks
 * - TypeScript generic support
 * 
 * Usage:
 * ```tsx
 * function MyComponent() {
 *   const { execute, loading, error, data } = useAsync<User>();
 * 
 *   const fetchUser = async () => {
 *     await execute(async () => {
 *       const response = await fetch('/api/user');
 *       return response.json();
 *     });
 *   };
 * 
 *   if (loading) return <Spinner />;
 *   if (error) return <Alert variant="error">{error.message}</Alert>;
 *   if (data) return <div>{data.name}</div>;
 * 
 *   return <button onClick={fetchUser}>Load User</button>;
 * }
 * ```
 */

import { useState, useCallback } from 'react';
import type { AsyncState, AsyncOptions } from '../types';

export function useAsync<T = unknown>(options?: AsyncOptions) {
    const [state, setState] = useState<AsyncState<T>>({
        data: null,
        loading: false,
        error: null,
    });

    const execute = useCallback(
        async (asyncFunction: () => Promise<T>) => {
            setState({ data: null, loading: true, error: null });

            try {
                const data = await asyncFunction();
                setState({ data, loading: false, error: null });
                options?.onSuccess?.();
                return data;
            } catch (error) {
                const errorObj = error instanceof Error ? error : new Error('Unknown error');
                setState({ data: null, loading: false, error: errorObj });
                options?.onError?.(errorObj);
                throw errorObj;
            }
        },
        [options]
    );

    const reset = useCallback(() => {
        setState({ data: null, loading: false, error: null });
    }, []);

    return {
        ...state,
        execute,
        reset,
    };
}

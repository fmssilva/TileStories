/**
 * GLOBAL STORE CONFIGURATION
 * ===========================
 * 
 * Minimal global Redux store setup using RTK.
 * Individual domains register their own API slices.
 */

import { configureStore } from '@reduxjs/toolkit';
import { pokemonApi } from '@/domains/pokemons';

// Global store configuration
export const store = configureStore({
    reducer: {
        // Domain API slices
        [pokemonApi.reducerPath]: pokemonApi.reducer,
    },

    middleware: (getDefaultMiddleware) =>
        getDefaultMiddleware({
            serializableCheck: {
                ignoredActions: ['persist/PERSIST', 'persist/REHYDRATE'],
            },
        }).concat(pokemonApi.middleware),

    devTools: process.env.NODE_ENV !== 'production',
});

// Store types for TypeScript
export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
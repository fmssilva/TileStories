/**
 * Application Configuration - Environment-based app settings
 * 
 * Centralizes application configuration by processing environment variables
 * and providing computed values for consistent usage across the app.
 * 
 * Responsibilities:
 * - Site identity (name, branding)
 * - Environment detection (dev/prod)
 * - API endpoint configuration
 * - Feature flag management
 */

export const appConfig = {
    // Site Identity
    name: import.meta.env.VITE_APP_NAME || 'Clinic Compare',
    description: import.meta.env.VITE_APP_DESCRIPTION || 'Compare healthcare clinics and find the best prices for your medical needs',

    // Environment Detection
    isDevelopment: import.meta.env.VITE_NODE_ENV === 'development',
    isProduction: import.meta.env.VITE_NODE_ENV === 'production',

    // API Configuration
    apiUrl: import.meta.env.VITE_API_URL,

    // Feature Flags
    enableDebugLogs: import.meta.env.VITE_ENABLE_DEBUG_LOGS === 'true',

    // Computed Site Identity Values
    get displayName() {
        return this.name;
    },

    get shortName() {
        // Extract first letter of each word for logo/favicon
        return this.name
            .split(' ')
            .map(word => word.charAt(0))
            .join('')
            .toUpperCase()
            .slice(0, 3); // Max 3 characters
    }
} as const;

// Type for better TypeScript support
export type AppConfig = typeof appConfig;
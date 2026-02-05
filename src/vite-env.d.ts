/// <reference types="vite/client" />

interface ImportMetaEnv {
    readonly VITE_APP_NAME: string
    readonly VITE_NODE_ENV: string
    readonly VITE_API_URL?: string
    readonly VITE_ENABLE_DEBUG_LOGS?: string
    // Add more environment variables here as needed
}

interface ImportMeta {
    readonly env: ImportMetaEnv
}
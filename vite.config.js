import path from 'path';
import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
// https://vite.dev/config/
export default defineConfig({
    plugins: [
        react({
            babel: {
                plugins: [['babel-plugin-react-compiler']],
            },
        }),
    ],
    server: {
        port: 5173,
        host: true,
        open: true, // Auto-open browser in development
    },
    resolve: {
        alias: {
            '@': path.resolve(__dirname, './src'),
            '@/domains': path.resolve(__dirname, './src/domains'),
            '@/components': path.resolve(__dirname, './src/components'),
            '@/lib': path.resolve(__dirname, './src/lib'),
            '@/branding': path.resolve(__dirname, './src/branding'),
            '@/config': path.resolve(__dirname, './src/config'),
            '@/utils': path.resolve(__dirname, './src/utils'),
            '@/layout': path.resolve(__dirname, './src/layout_and_navigation'),
            '@/design': path.resolve(__dirname, './src/design'),
            '@/assets': path.resolve(__dirname, './src/assets'),
        }
    },
    build: {
        // Optimize chunk size and splitting for production
        chunkSizeWarningLimit: 2500,
        target: 'esnext',
        minify: 'esbuild',
        rollupOptions: {
            output: {
                manualChunks: {
                    // Separate vendor libraries into their own chunks
                    'react-vendor': ['react', 'react-dom'],
                    'redux-vendor': ['@reduxjs/toolkit', 'react-redux'],
                    'ui-vendor': ['lucide-react', '@radix-ui/react-slot'],
                    'router-vendor': ['react-router-dom'],
                    // Feature domains can be added here as they grow
                },
            },
        },
    },
    // CSS preprocessing
    css: {
        postcss: './postcss.config.js',
    },
});

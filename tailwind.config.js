/** @type {import('tailwindcss').Config} */
export default {
    content: ['./index.html', './src/**/*.{js,ts,jsx,tsx}'],
    darkMode: 'class', // Enable class-based dark mode for 2026 UX standards
    theme: {
        extend: {
            // Custom border radius using CSS variables
            borderRadius: {
                lg: 'var(--radius)',
                md: 'calc(var(--radius) - 2px)',
                sm: 'calc(var(--radius) - 4px)',
            },

            // State-of-the-art color system - Azulejo-inspired for TileStories
            colors: {
                // Azulejo Blue - Primary brand color from traditional Portuguese tiles
                'azulejo-blue': {
                    50: '#EBF1F8',
                    100: '#D7E3F1',
                    200: '#AFC7E3',
                    300: '#87ABD5',
                    400: '#5F8FC7',
                    500: '#3C5E95', // Main azulejo blue
                    600: '#2F4B77',
                    700: '#233859',
                    800: '#17253B',
                    900: '#0B121E',
                },

                // Azulejo Cobalt - Secondary brand color
                'azulejo-cobalt': {
                    50: '#EDF4FB',
                    100: '#DBE9F7',
                    200: '#B7D3EF',
                    300: '#93BDE7',
                    400: '#6FA7DF',
                    500: '#5081B6', // Cobalt blue
                    600: '#3F6691',
                    700: '#2F4C6D',
                    800: '#1F3248',
                    900: '#101924',
                },

                // Azulejo Gold - Accent for tile decorations
                'azulejo-gold': {
                    50: '#FBF7E8',
                    100: '#F7EFD1',
                    200: '#EFDFA3',
                    300: '#E7CF75',
                    400: '#DFBF47',
                    500: '#D4AF37', // Gold accent
                    600: '#A88C2C',
                    700: '#7E6921',
                    800: '#544616',
                    900: '#2A230B',
                },

                // Azulejo Terracotta - Accent for drama/earthquake sections
                'azulejo-terracotta': {
                    50: '#FAECE8',
                    100: '#F5D9D1',
                    200: '#EBB3A3',
                    300: '#E18D75',
                    400: '#D76747',
                    500: '#C1440E', // Terracotta
                    600: '#9A360B',
                    700: '#742808',
                    800: '#4D1B06',
                    900: '#270D03',
                },

                // Azulejo Ivory - Accent for tile backgrounds
                'azulejo-ivory': {
                    50: '#FFFEFB',
                    100: '#FFFCF7',
                    200: '#FFF9EF',
                    300: '#FFF8E7', // Ivory background
                    400: '#FFF3D7',
                    500: '#FFEEC7',
                    600: '#E6D5B3',
                    700: '#CCBB9F',
                    800: '#B3A28B',
                    900: '#998977',
                },

                // Semantic color aliases for easy reference
                primary: '#3C5E95', // Azulejo blue
                secondary: '#5081B6', // Cobalt
                accent: '#D4AF37', // Gold

                // Semantic colors via CSS variables (2026 standard)
                background: 'hsl(var(--background))',
                foreground: 'hsl(var(--foreground))',
                card: {
                    DEFAULT: 'hsl(var(--card))',
                    foreground: 'hsl(var(--card-foreground))',
                },
                popover: {
                    DEFAULT: 'hsl(var(--popover))',
                    foreground: 'hsl(var(--popover-foreground))',
                },
                primary: {
                    DEFAULT: 'hsl(var(--primary))',
                    foreground: 'hsl(var(--primary-foreground))',
                },
                secondary: {
                    DEFAULT: 'hsl(var(--secondary))',
                    foreground: 'hsl(var(--secondary-foreground))',
                },
                muted: {
                    DEFAULT: 'hsl(var(--muted))',
                    foreground: 'hsl(var(--muted-foreground))',
                },
                accent: {
                    DEFAULT: 'hsl(var(--accent))',
                    foreground: 'hsl(var(--accent-foreground))',
                },
                destructive: {
                    DEFAULT: 'hsl(var(--destructive))',
                    foreground: 'hsl(var(--destructive-foreground))',
                },
                border: 'hsl(var(--border))',
                input: 'hsl(var(--input))',
                ring: 'hsl(var(--ring))',
            },

            // Modern spacing scale
            spacing: {
                '18': '4.5rem',
                '88': '22rem',
                '128': '32rem',
            },

            // Custom background images and gradients for azulejo aesthetic
            backgroundImage: {
                'hero-gradient': 'linear-gradient(135deg, #2F4B77 0%, #5F8FC7 100%)',
                'gold-gradient': 'linear-gradient(135deg, #D4AF37 0%, #DFBF47 100%)',
                'panorama-overlay': 'linear-gradient(180deg, rgba(11, 18, 30, 0.7) 0%, rgba(60, 94, 149, 0.3) 100%)',
                'azulejo-pattern': 'linear-gradient(135deg, #3C5E95 0%, #5081B6 50%, #D4AF37 100%)',
            },

            // Typography improvements
            fontFamily: {
                sans: ['Inter', 'system-ui', 'sans-serif'],
                mono: ['JetBrains Mono', 'Consolas', 'monospace'],
            },

            // Animation and transitions
            animation: {
                'fade-in': 'fadeIn 0.5s ease-in-out',
                'slide-up': 'slideUp 0.3s ease-out',
                'pulse-subtle': 'pulse 2s cubic-bezier(0.4, 0, 0.6, 1) infinite',
            },

            keyframes: {
                fadeIn: {
                    '0%': { opacity: '0' },
                    '100%': { opacity: '1' },
                },
                slideUp: {
                    '0%': { transform: 'translateY(10px)', opacity: '0' },
                    '100%': { transform: 'translateY(0)', opacity: '1' },
                },
            },
        },
    },
    plugins: [
        require('tailwind-scrollbar')({ nocompatible: true }),
    ],
};
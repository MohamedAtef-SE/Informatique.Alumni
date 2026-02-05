/** @type {import('tailwindcss').Config} */
export default {
    content: [
        "./index.html",
        "./src/**/*.{js,ts,jsx,tsx}",
    ],
    theme: {
        extend: {
            colors: {
                primary: {
                    DEFAULT: '#0f172a', // Slate 900
                    light: '#1e293b',   // Slate 800
                    foreground: '#f8fafc',
                },
                secondary: {
                    DEFAULT: '#334155', // Slate 700
                    foreground: '#f1f5f9',
                },
                accent: {
                    DEFAULT: '#d4af37', // Metallic Gold
                    hover: '#b5952f',
                    foreground: '#0f172a',
                },
                background: '#020617', // Slate 950
                muted: {
                    DEFAULT: '#64748b',
                    foreground: '#94a3b8',
                },
                success: '#10b981',
                warning: '#f59e0b',
                error: '#ef4444',
            },
            fontFamily: {
                sans: ['Inter', 'system-ui', 'sans-serif'],
                heading: ['Outfit', 'Inter', 'system-ui', 'sans-serif'],
            },
            keyframes: {
                'fade-in': {
                    '0%': { opacity: '0' },
                    '100%': { opacity: '1' },
                },
                'slide-up': {
                    '0%': { transform: 'translateY(20px)', opacity: '0' },
                    '100%': { transform: 'translateY(0)', opacity: '1' },
                },
                'scale-in': {
                    '0%': { transform: 'scale(0.95)', opacity: '0' },
                    '100%': { transform: 'scale(1)', opacity: '1' },
                }
            },
            animation: {
                'fade-in': 'fade-in 0.5s ease-out',
                'slide-up': 'slide-up 0.6s ease-out',
                'scale-in': 'scale-in 0.3s ease-out',
            },
            boxShadow: {
                'glass': '0 8px 32px 0 rgba(0, 0, 0, 0.37)',
                'neon': '0 0 10px rgba(212, 175, 55, 0.5)',
            }
        },
    },
    plugins: [],
}

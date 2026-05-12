import type { Config } from 'tailwindcss'

export default {
  content: ['./index.html', './src/**/*.{ts,tsx}'],
  theme: {
    extend: {
      colors: {
        brand: {
          navy: '#0B1E3F',
          navyLight: '#122952',
          teal: '#14B8A6',
          tealDark: '#0F766E',
        },
        card: {
          mint: '#D7F3E8',
          aqua: '#C5EDE4',
          peach: '#FCE3DA',
          lavender: '#E8E4FB',
          blue: '#D6EAF8',
        },
        status: {
          unpaid: '#F59E0B',
          overdue: '#EF4444',
          paid: '#10B981',
          closed: '#6B7280',
        },
      },
      fontFamily: {
        sans: ['Inter', 'ui-sans-serif', 'system-ui', '-apple-system', 'sans-serif'],
      },
      boxShadow: {
        card: '0 2px 8px rgba(11,30,63,0.08)',
        'card-lg': '0 8px 24px rgba(11,30,63,0.12)',
      },
    },
  },
  plugins: [],
} satisfies Config

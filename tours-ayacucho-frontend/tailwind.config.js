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
          50:  '#e0fafa',
          100: '#b3f2f2',
          200: '#80e8e8',
          300: '#4ddede',
          400: '#26d4d4',
          500: '#00bfbf',
          600: '#009999',
          700: '#007373',
          800: '#004d4d',
          900: '#002626',
        },
        teal: {
          400: '#2dd4bf',
          500: '#14b8a6',
          600: '#0d9488',
        },
        gold: {
          400: '#fbbf24',
          500: '#f59e0b',
          600: '#d97706',
        },
        dark: {
          800: '#0f1923',
          900: '#070d14',
        }
      },
      fontFamily: {
        sans: ['Inter', 'system-ui', 'sans-serif'],
        display: ['Outfit', 'system-ui', 'sans-serif'],
      },
      backgroundImage: {
        'hero-gradient': 'linear-gradient(135deg, #070d14 0%, #0f2744 40%, #00474f 100%)',
        'card-gradient': 'linear-gradient(145deg, rgba(0,191,191,0.08), rgba(0,191,191,0.02))',
        'teal-gradient': 'linear-gradient(135deg, #00bfbf, #0d9488)',
        'gold-gradient': 'linear-gradient(135deg, #f59e0b, #d97706)',
      },
      animation: {
        'fade-in': 'fadeIn 0.5s ease-in-out',
        'slide-up': 'slideUp 0.4s ease-out',
        'slide-down': 'slideDown 0.3s ease-out',
        'pulse-slow': 'pulse 3s cubic-bezier(0.4, 0, 0.6, 1) infinite',
        'float': 'float 6s ease-in-out infinite',
      },
      keyframes: {
        fadeIn: {
          '0%': { opacity: '0' },
          '100%': { opacity: '1' },
        },
        slideUp: {
          '0%': { transform: 'translateY(20px)', opacity: '0' },
          '100%': { transform: 'translateY(0)', opacity: '1' },
        },
        slideDown: {
          '0%': { transform: 'translateY(-10px)', opacity: '0' },
          '100%': { transform: 'translateY(0)', opacity: '1' },
        },
        float: {
          '0%, 100%': { transform: 'translateY(0px)' },
          '50%': { transform: 'translateY(-10px)' },
        }
      },
      boxShadow: {
        'teal': '0 4px 30px rgba(0, 191, 191, 0.3)',
        'teal-lg': '0 8px 50px rgba(0, 191, 191, 0.4)',
        'card': '0 4px 24px rgba(0, 0, 0, 0.4)',
        'card-hover': '0 8px 40px rgba(0, 191, 191, 0.25)',
      }
    },
  },
  plugins: [],
}

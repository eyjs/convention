/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{vue,js,ts,jsx,tsx}",
  ],
  theme: {
    // ===== 타이포그래피 시스템 (Mobile-First) =====
    fontSize: {
      // 기본 폰트 크기 (rem 단위, 줄간격 포함)
      'xs': ['0.75rem', { lineHeight: '1.5' }],       // 12px
      'sm': ['0.875rem', { lineHeight: '1.6' }],      // 14px
      'base': ['0.9375rem', { lineHeight: '1.7' }],   // 15px (Mobile 기본)
      'lg': ['1rem', { lineHeight: '1.75' }],         // 16px
      'xl': ['1.125rem', { lineHeight: '1.75' }],     // 18px
      '2xl': ['1.25rem', { lineHeight: '1.6' }],      // 20px
      '3xl': ['1.5rem', { lineHeight: '1.5' }],       // 24px
      '4xl': ['1.875rem', { lineHeight: '1.4' }],     // 30px
      '5xl': ['2.25rem', { lineHeight: '1.3' }],      // 36px
      '6xl': ['3rem', { lineHeight: '1.2' }],         // 48px
    },
    extend: {
      colors: {
        // CSS 변수를 사용한 동적 primary 컬러
        primary: {
          50: 'rgb(var(--color-primary-50) / <alpha-value>)',
          100: 'rgb(var(--color-primary-100) / <alpha-value>)',
          200: 'rgb(var(--color-primary-200) / <alpha-value>)',
          300: 'rgb(var(--color-primary-300) / <alpha-value>)',
          400: 'rgb(var(--color-primary-400) / <alpha-value>)',
          500: 'rgb(var(--color-primary-500) / <alpha-value>)',
          600: 'rgb(var(--color-primary-600) / <alpha-value>)',
          700: 'rgb(var(--color-primary-700) / <alpha-value>)',
          800: 'rgb(var(--color-primary-800) / <alpha-value>)',
          900: 'rgb(var(--color-primary-900) / <alpha-value>)',
        },
        // 다크 그레이는 고정
        dark: {
          50: '#f8fafc',
          100: '#f1f5f9',
          200: '#e2e8f0',
          300: '#cbd5e1',
          400: '#94a3b8',
          500: '#64748b',
          600: '#475569',
          700: '#334155',
          800: '#1e293b',
          900: '#0f172a',
        }
      },
      fontFamily: {
        sans: [
          'Pretendard Variable',
          'Pretendard',
          '-apple-system',
          'BlinkMacSystemFont',
          'system-ui',
          'Roboto',
          'Helvetica Neue',
          'Segoe UI',
          'Apple SD Gothic Neo',
          'Noto Sans KR',
          'Malgun Gothic',
          'sans-serif',
        ],
      },
      boxShadow: {
        'card': '0 2px 8px rgba(0, 0, 0, 0.08)',
        'card-hover': '0 4px 12px rgba(0, 0, 0, 0.12)',
        'float': '0 8px 24px rgba(0, 0, 0, 0.15)',
      },
      animation: {
        'fade-in': 'fadeIn 0.3s ease-out',
        'slide-up': 'slideUp 0.3s ease-out',
        'slide-down': 'slideDown 0.3s ease-out',
        'scale-in': 'scaleIn 0.2s ease-out',
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
        slideDown: {
          '0%': { transform: 'translateY(-10px)', opacity: '0' },
          '100%': { transform: 'translateY(0)', opacity: '1' },
        },
        scaleIn: {
          '0%': { transform: 'scale(0.95)', opacity: '0' },
          '100%': { transform: 'scale(1)', opacity: '1' },
        },
      },
      // ===== Typography Plugin 설정 (Prose 클래스용) =====
      typography: ({ theme }) => ({
        DEFAULT: {
          css: {
            // Mobile base
            fontSize: '15px',
            lineHeight: '1.8',
            maxWidth: 'none',
            color: theme('colors.gray.900'),

            // Headings
            h1: {
              fontSize: '1.5rem',        // 24px
              lineHeight: '1.4',
              fontWeight: '700',
              marginTop: '0',
              marginBottom: '0.75em',
            },
            h2: {
              fontSize: '1.25rem',       // 20px
              lineHeight: '1.5',
              fontWeight: '600',
              marginTop: '1.5em',
              marginBottom: '0.75em',
            },
            h3: {
              fontSize: '1.125rem',      // 18px
              lineHeight: '1.5',
              fontWeight: '600',
              marginTop: '1.5em',
              marginBottom: '0.5em',
            },

            // Paragraphs & Lists
            p: {
              marginTop: '0',
              marginBottom: '1em',
            },
            'ul, ol': {
              marginTop: '1em',
              marginBottom: '1em',
              paddingLeft: '1.5em',
            },
            li: {
              marginTop: '0.25em',
              marginBottom: '0.25em',
            },

            // Links
            a: {
              color: theme('colors.blue.600'),
              textDecoration: 'underline',
              fontWeight: '500',
              '&:hover': {
                color: theme('colors.blue.700'),
              },
            },

            // Code & Pre
            code: {
              color: theme('colors.gray.900'),
              backgroundColor: theme('colors.gray.100'),
              padding: '0.2em 0.4em',
              borderRadius: '0.25rem',
              fontSize: '0.875em',
              fontWeight: '500',
            },
            'code::before': {
              content: '""',
            },
            'code::after': {
              content: '""',
            },
            pre: {
              backgroundColor: theme('colors.gray.100'),
              color: theme('colors.gray.900'),
              padding: '1em',
              borderRadius: '0.5rem',
              overflowX: 'auto',
              fontSize: '0.875em',
              lineHeight: '1.7',
            },
            'pre code': {
              backgroundColor: 'transparent',
              padding: '0',
              fontSize: 'inherit',
              color: 'inherit',
            },

            // Blockquotes
            blockquote: {
              borderLeftWidth: '4px',
              borderLeftColor: theme('colors.gray.300'),
              paddingLeft: '1em',
              marginTop: '1.5em',
              marginBottom: '1.5em',
              fontStyle: 'normal',
              color: theme('colors.gray.700'),
            },

            // Images
            img: {
              marginTop: '1.5em',
              marginBottom: '1.5em',
              borderRadius: '0.5rem',
            },

            // Tables
            table: {
              width: '100%',
              marginTop: '1.5em',
              marginBottom: '1.5em',
            },
            'thead th': {
              fontWeight: '600',
              borderBottomWidth: '2px',
              borderBottomColor: theme('colors.gray.300'),
              paddingTop: '0.5em',
              paddingBottom: '0.5em',
            },
            'tbody td': {
              borderBottomWidth: '1px',
              borderBottomColor: theme('colors.gray.200'),
              paddingTop: '0.5em',
              paddingBottom: '0.5em',
            },
          },
        },
        sm: {
          css: {
            fontSize: '14px',
            lineHeight: '1.7',
          },
        },
        lg: {
          css: {
            fontSize: '16px',
            lineHeight: '2',
          },
        },
      }),
    },
  },
  plugins: [
    require('@tailwindcss/typography'),
  ],
}

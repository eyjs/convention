/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{vue,js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        'ifa-green': '#00B894',
        'ifa-dark-green': '#00A085',
        'ifa-light': '#E8F5F3',
        'ifa-gray': '#636E72',
        'ifa-light-gray': '#DDD',
        'schedule-timeline': '#00B894'
      },
      fontFamily: {
        'sans': ['-apple-system', 'BlinkMacSystemFont', 'Segoe UI', 'Roboto', 'sans-serif']
      },
      screens: {
        'mobile': '375px',
        'tablet': '768px'
      }
    },
  },
  plugins: [],
}

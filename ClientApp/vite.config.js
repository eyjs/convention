import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import path from 'path'

export default defineConfig({
  plugins: [vue()],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
    },
  },
  server: {
    port: 3000,
    host: true,
    proxy: {
      '/api': {
        target: 'http://localhost:5000',
        changeOrigin: true,
        secure: false,
      },
      '/chathub': {
        target: 'http://localhost:5000',
        changeOrigin: true,
        secure: false,
        ws: true,
      },
    },
  },
  build: {
    outDir: 'dist',
    assetsDir: 'assets',
    sourcemap: false,
    minify: 'esbuild',
    rollupOptions: {
      output: {
        manualChunks(id) {
          if (
            id.includes('node_modules/quill') ||
            id.includes('node_modules/parchment')
          ) {
            return 'quill'
          }
          if (id.includes('node_modules/xlsx')) {
            return 'xlsx'
          }
          if (
            id.includes('node_modules/jspdf') ||
            id.includes('node_modules/html2canvas')
          ) {
            return 'pdf-export'
          }
          if (id.includes('node_modules/apexcharts') || id.includes('node_modules/vue3-apexcharts')) {
            return 'apexcharts'
          }
          if (id.includes('node_modules/vue-datepicker-next')) {
            return 'datepicker'
          }
          if (id.includes('node_modules/lucide-vue-next')) {
            return 'lucide-icons'
          }
          if (id.includes('node_modules/country-state-city')) {
            return 'country-state-city'
          }
          if (id.includes('node_modules/dayjs')) {
            return 'dayjs'
          }
        },
      },
    },
  },
  css: {
    postcss: './postcss.config.cjs',
  },
  base: '/',
})

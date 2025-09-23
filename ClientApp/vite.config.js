import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

export default defineConfig({
  plugins: [vue()],
  server: {
    port: 3000,
    host: true,
    proxy: {
      '/api': {
        target: process.env.ASPNETCORE_URLS?.split(';')[0] || 'http://localhost:5000',
        changeOrigin: true,
        secure: false,
      }
    }
  },
  build: {
    outDir: 'dist',
    assetsDir: 'assets',
    sourcemap: false,
    minify: 'terser',
    rollupOptions: {
      output: {
        manualChunks: {
          vendor: ['vue', 'vue-router', 'pinia'],
          utils: ['axios', 'date-fns', 'lucide-vue-next']
        }
      }
    }
  },
  css: {
    postcss: './postcss.config.js'
  },
  base: './' // 상대 경로 사용 (배포 환경 고려)
})

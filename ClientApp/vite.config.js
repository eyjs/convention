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
    minify: false,
    rollupOptions: {
      output: {
        manualChunks(id) {
          // Quill 관련 모듈만 별도 청크로 분리 (순환참조 방지)
          if (id.includes('node_modules/quill') || id.includes('node_modules/parchment')) {
            return 'quill'
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

import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import { fileURLToPath, URL } from 'node:url'

export default defineConfig({
  plugins: [vue()],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url))
    }
  },
  server: {
    port: 5173,
    host: true,
  },
  // GitHub Pages 部署需要指定 repo 名稱作為 base path
  base: process.env.NODE_ENV === 'production' ? '/changfu-erp/' : '/',
})

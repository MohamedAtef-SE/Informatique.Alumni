import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/api': {
        target: 'https://localhost:44386',
        changeOrigin: true,
        secure: false,
      },
      '/swagger': {
        target: 'https://localhost:44386',
        changeOrigin: true,
        secure: false,
      },
      // Proxying identity server if needed, usually on same port in ABP template
      '/.well-known': {
        target: 'https://localhost:44386',
        changeOrigin: true,
        secure: false,
      }
    }
  }
})

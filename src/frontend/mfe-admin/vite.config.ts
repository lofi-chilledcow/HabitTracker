import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import federation from '@originjs/vite-plugin-federation'

export default defineConfig({
  plugins: [
    react(),
    federation({
      name: 'mfe-admin',
      filename: 'remoteEntry.js',
      exposes: {
        './AdminApp': './src/AdminApp',
      },
      shared: {
        react: { singleton: true, requiredVersion: '^18.3.1' },
        'react-dom': { singleton: true, requiredVersion: '^18.3.1' },
        'react-router-dom': { singleton: true, requiredVersion: '^6.27.0' },
      },
    }),
  ],
  server: { port: 3003 },
  preview: { port: 3003 },
  build: {
    target: 'esnext',
    minify: false,
  },
})

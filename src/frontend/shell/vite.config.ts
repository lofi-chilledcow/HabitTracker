import { defineConfig, loadEnv } from 'vite'
import react from '@vitejs/plugin-react'
import federation from '@originjs/vite-plugin-federation'

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '')

  const mfeAuthUrl = env.VITE_MFE_AUTH_URL || 'http://localhost:3001'
  const mfeHabitsUrl = env.VITE_MFE_HABITS_URL || 'http://localhost:3002'
  const mfeAdminUrl = env.VITE_MFE_ADMIN_URL || 'http://localhost:3003'
  const mfeCompetitionUrl = env.VITE_MFE_COMPETITION_URL || 'http://localhost:3004'

  return {
    plugins: [
      react(),
      federation({
        name: 'shell',
        remotes: {
          'mfe-auth': `${mfeAuthUrl}/assets/remoteEntry.js`,
          'mfe-habits': `${mfeHabitsUrl}/assets/remoteEntry.js`,
          'mfe-admin': `${mfeAdminUrl}/assets/remoteEntry.js`,
          'mfe-competition': `${mfeCompetitionUrl}/assets/remoteEntry.js`,
        },
        shared: {
          react: { singleton: true, requiredVersion: '^18.3.1' },
          'react-dom': { singleton: true, requiredVersion: '^18.3.1' },
          'react-router-dom': { singleton: true, requiredVersion: '^6.27.0' },
        },
      }),
    ],
    server: { port: 3000 },
    preview: { port: 3000 },
    build: {
      target: 'esnext',
      minify: false,
    },
  }
})

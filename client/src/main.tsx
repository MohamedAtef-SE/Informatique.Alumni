import { createRoot } from 'react-dom/client'
import { Suspense } from 'react';
import { AuthProvider } from 'react-oidc-context'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { oidcConfig } from './services/auth'
import './index.css'
import './i18n'; // Initialize i18n
import App from './App.tsx'

const queryClient = new QueryClient()

createRoot(document.getElementById('root')!).render(
  <QueryClientProvider client={queryClient}>
    <AuthProvider {...oidcConfig}>
      <Suspense fallback={<div className="flex h-screen items-center justify-center">Loading...</div>}>
        <App />
      </Suspense>
    </AuthProvider>
  </QueryClientProvider>,
)

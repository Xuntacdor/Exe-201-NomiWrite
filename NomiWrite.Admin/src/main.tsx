import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import App from './App.tsx'
import { AdminPreferencesProvider } from './lib/adminPreferences.tsx'

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <AdminPreferencesProvider><App /></AdminPreferencesProvider>
  </StrictMode>,
)

import { lazy, Suspense } from 'react'
import { Routes, Route, Navigate } from 'react-router-dom'
import Nav from './components/Nav'
import ProtectedRoute from './components/ProtectedRoute'
import { RemoteErrorBoundary } from './components/RemoteErrorBoundary'

const AuthApp = lazy(() => import('mfe-auth/AuthApp'))
const HabitsApp = lazy(() => import('mfe-habits/HabitsApp'))
const AdminApp = lazy(() => import('mfe-admin/AdminApp'))
const CompetitionApp = lazy(() => import('mfe-competition/CompetitionApp'))

function Loading() {
  return (
    <div className="flex items-center justify-center min-h-[50vh] text-gray-400 text-sm">
      Loading...
    </div>
  )
}

export default function App() {
  return (
    <>
      <Nav />
      <main>
        <Routes>
          <Route path="/" element={<Navigate to="/habits" replace />} />

          <Route
            path="/auth/*"
            element={
              <RemoteErrorBoundary name="mfe-auth">
                <Suspense fallback={<Loading />}>
                  <AuthApp />
                </Suspense>
              </RemoteErrorBoundary>
            }
          />

          <Route
            path="/habits/*"
            element={
              <ProtectedRoute>
                <RemoteErrorBoundary name="mfe-habits">
                  <Suspense fallback={<Loading />}>
                    <HabitsApp />
                  </Suspense>
                </RemoteErrorBoundary>
              </ProtectedRoute>
            }
          />

          <Route
            path="/admin/*"
            element={
              <ProtectedRoute>
                <RemoteErrorBoundary name="mfe-admin">
                  <Suspense fallback={<Loading />}>
                    <AdminApp />
                  </Suspense>
                </RemoteErrorBoundary>
              </ProtectedRoute>
            }
          />

          <Route
            path="/competition/*"
            element={
              <ProtectedRoute>
                <RemoteErrorBoundary name="mfe-competition">
                  <Suspense fallback={<Loading />}>
                    <CompetitionApp />
                  </Suspense>
                </RemoteErrorBoundary>
              </ProtectedRoute>
            }
          />
        </Routes>
      </main>
    </>
  )
}

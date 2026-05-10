import { NavLink, Navigate, Outlet, Route, Routes, useLocation } from 'react-router-dom'
import { Activity, BarChart3, CalendarCheck, LogOut, ShieldCheck, UserCircle } from 'lucide-react'
import { useAuth } from '../shared/auth/AuthProvider'
import { LoginPage } from '../features/auth/LoginPage'
import { RegisterPage } from '../features/auth/RegisterPage'
import { TodayPage } from '../features/habits/TodayPage'
import { HabitsPage } from '../features/habits/HabitsPage'
import { HabitEditorPage } from '../features/habits/HabitEditorPage'
import { CompetitionPage } from '../features/competition/CompetitionPage'
import { AdminUsersPage } from '../features/admin/AdminUsersPage'
import { LoadingState } from '../shared/ui/State'

function ShellLayout() {
  const { user, logout } = useAuth()

  return (
    <div className="app-shell">
      <aside className="sidebar">
        <div className="brand">
          <Activity size={24} />
          <span>HabitTracker</span>
        </div>
        <nav className="nav">
          <NavItem to="/habits/today" icon={<CalendarCheck size={18} />} label="Today" />
          <NavItem to="/habits" icon={<UserCircle size={18} />} label="Habits" />
          <NavItem to="/competition" icon={<BarChart3 size={18} />} label="Leaderboard" />
          {user?.role === 'Admin' ? <NavItem to="/admin/users" icon={<ShieldCheck size={18} />} label="Users" /> : null}
        </nav>
        <div className="sidebar-footer">
          <div>
            <strong>{user?.username}</strong>
            <span>{user?.role}</span>
          </div>
          <button className="icon-button" onClick={() => void logout()} aria-label="Log out" title="Log out">
            <LogOut size={18} />
          </button>
        </div>
      </aside>
      <main className="main">
        <Outlet />
      </main>
    </div>
  )
}

function NavItem({ to, icon, label }: { to: string; icon: React.ReactNode; label: string }) {
  return (
    <NavLink to={to} className={({ isActive }) => (isActive ? 'nav-link active' : 'nav-link')}>
      {icon}
      <span>{label}</span>
    </NavLink>
  )
}

function RequireAuth({ children }: { children: React.ReactNode }) {
  const { user, isBootstrapping } = useAuth()
  const location = useLocation()

  if (isBootstrapping) return <LoadingState label="Restoring session" />
  if (!user) return <Navigate to="/auth/login" replace state={{ from: location.pathname }} />
  return <>{children}</>
}

function RequireAdmin({ children }: { children: React.ReactNode }) {
  const { user } = useAuth()
  if (user?.role !== 'Admin') return <Navigate to="/habits/today" replace />
  return <>{children}</>
}

function PublicOnly({ children }: { children: React.ReactNode }) {
  const { user, isBootstrapping } = useAuth()
  if (isBootstrapping) return <LoadingState label="Restoring session" />
  if (user) return <Navigate to="/habits/today" replace />
  return <>{children}</>
}

export function App() {
  return (
    <Routes>
      <Route path="/auth/login" element={<PublicOnly><LoginPage /></PublicOnly>} />
      <Route path="/auth/register" element={<PublicOnly><RegisterPage /></PublicOnly>} />
      <Route path="/" element={<RequireAuth><ShellLayout /></RequireAuth>}>
        <Route index element={<Navigate to="/habits/today" replace />} />
        <Route path="habits/today" element={<TodayPage />} />
        <Route path="habits" element={<HabitsPage />} />
        <Route path="habits/new" element={<HabitEditorPage mode="create" />} />
        <Route path="habits/:id/edit" element={<HabitEditorPage mode="edit" />} />
        <Route path="competition" element={<CompetitionPage />} />
        <Route path="admin/users" element={<RequireAdmin><AdminUsersPage /></RequireAdmin>} />
      </Route>
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  )
}

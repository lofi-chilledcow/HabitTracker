import { NavLink, useNavigate } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext'

export default function Nav() {
  const { user, logout } = useAuth()
  const navigate = useNavigate()

  function handleLogout() {
    logout()
    navigate('/auth/login')
  }

  if (!user) return null

  return (
    <nav className="bg-white border-b border-gray-200 px-6 py-3 flex items-center gap-6">
      <span className="text-sm font-bold text-indigo-600 mr-2">HabitTracker</span>
      <NavLink
        to="/habits"
        className={({ isActive }) =>
          `text-sm ${isActive ? 'text-indigo-600 font-medium' : 'text-gray-600 hover:text-gray-900'}`
        }
      >
        Habits
      </NavLink>
      <NavLink
        to="/competition"
        className={({ isActive }) =>
          `text-sm ${isActive ? 'text-indigo-600 font-medium' : 'text-gray-600 hover:text-gray-900'}`
        }
      >
        Competition
      </NavLink>
      <NavLink
        to="/admin"
        className={({ isActive }) =>
          `text-sm ${isActive ? 'text-indigo-600 font-medium' : 'text-gray-600 hover:text-gray-900'}`
        }
      >
        Admin
      </NavLink>
      <div className="ml-auto flex items-center gap-3">
        <span className="text-sm text-gray-500">{user.username}</span>
        <button
          onClick={handleLogout}
          className="text-sm text-gray-500 hover:text-gray-800"
        >
          Sign out
        </button>
      </div>
    </nav>
  )
}

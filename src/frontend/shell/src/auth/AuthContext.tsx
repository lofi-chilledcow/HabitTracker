import {
  createContext,
  useContext,
  useState,
  useEffect,
  useCallback,
  type ReactNode,
} from 'react'
import {
  getStoredUsername,
  getStoredEmail,
  getToken,
  clearTokens,
} from './tokens'

export interface AuthUser {
  username: string
  email: string
}

interface AuthContextValue {
  user: AuthUser | null
  logout: () => void
}

const AuthContext = createContext<AuthContextValue | null>(null)

function loadUser(): AuthUser | null {
  const token = getToken()
  const username = getStoredUsername()
  const email = getStoredEmail()
  if (token && username && email) return { username, email }
  return null
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(loadUser)

  // Listen for login events dispatched by mfe-auth after a successful login
  useEffect(() => {
    function onLogin(e: Event) {
      const { username, email } = (e as CustomEvent<AuthUser>).detail
      setUser({ username, email })
    }
    function onLogout() {
      setUser(null)
    }
    window.addEventListener('habittracker:auth:login', onLogin)
    window.addEventListener('habittracker:auth:logout', onLogout)
    return () => {
      window.removeEventListener('habittracker:auth:login', onLogin)
      window.removeEventListener('habittracker:auth:logout', onLogout)
    }
  }, [])

  const logout = useCallback(() => {
    clearTokens()
    setUser(null)
    window.dispatchEvent(new CustomEvent('habittracker:auth:logout'))
  }, [])

  return (
    <AuthContext.Provider value={{ user, logout }}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used inside AuthProvider')
  return ctx
}

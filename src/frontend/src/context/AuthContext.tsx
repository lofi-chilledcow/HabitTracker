import { createContext, useContext, useState, useCallback, type ReactNode } from 'react'
import { login as apiLogin, register as apiRegister } from '../api/auth'
import { clearTokens } from '../api/axios'
import type { AuthResponse } from '../types'

interface AuthState {
  username: string
  email: string
}

interface AuthContextValue {
  user: AuthState | null
  login: (email: string, password: string) => Promise<void>
  register: (username: string, email: string, password: string) => Promise<void>
  logout: () => void
}

const AuthContext = createContext<AuthContextValue | null>(null)

function userFromAuth(auth: AuthResponse): AuthState {
  return { username: auth.username, email: auth.email }
}

function loadStoredUser(): AuthState | null {
  const token = localStorage.getItem('access_token')
  const username = localStorage.getItem('username')
  const email = localStorage.getItem('email')
  if (token && username && email) return { username, email }
  return null
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthState | null>(loadStoredUser)

  const login = useCallback(async (email: string, password: string) => {
    const auth = await apiLogin(email, password)
    localStorage.setItem('username', auth.username)
    localStorage.setItem('email', auth.email)
    setUser(userFromAuth(auth))
  }, [])

  const register = useCallback(async (username: string, email: string, password: string) => {
    const auth = await apiRegister(username, email, password)
    localStorage.setItem('username', auth.username)
    localStorage.setItem('email', auth.email)
    setUser(userFromAuth(auth))
  }, [])

  const logout = useCallback(() => {
    clearTokens()
    localStorage.removeItem('username')
    localStorage.removeItem('email')
    setUser(null)
  }, [])

  return (
    <AuthContext.Provider value={{ user, login, register, logout }}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used inside AuthProvider')
  return ctx
}

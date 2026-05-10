import { createContext, useContext, useEffect, useMemo, useState } from 'react'
import { clearTokens, getAccessToken, request, storeTokens } from '../api/client'
import type { AuthResponse, AuthUser } from '../api/types'

type AuthContextValue = {
  user: AuthUser | null
  isBootstrapping: boolean
  login: (identifier: string, password: string) => Promise<void>
  register: (input: RegisterInput) => Promise<void>
  logout: () => Promise<void>
}

type RegisterInput = {
  username: string
  email: string
  phoneNumber?: string
  password: string
}

const AuthContext = createContext<AuthContextValue | null>(null)

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(null)
  const [isBootstrapping, setIsBootstrapping] = useState(true)

  useEffect(() => {
    if (!getAccessToken()) {
      setIsBootstrapping(false)
      return
    }

    request<AuthUser>('/api/auth/me')
      .then(setUser)
      .catch(() => {
        clearTokens()
        setUser(null)
      })
      .finally(() => setIsBootstrapping(false))
  }, [])

  const value = useMemo<AuthContextValue>(
    () => ({
      user,
      isBootstrapping,
      async login(identifier, password) {
        const result = await request<AuthResponse>('/api/auth/login', {
          method: 'POST',
          body: { identifier, password },
          skipRefresh: true,
        })
        storeTokens(result.accessToken, result.refreshToken)
        setUser(result.user)
      },
      async register(input) {
        const result = await request<AuthResponse>('/api/auth/register', {
          method: 'POST',
          body: input,
          skipRefresh: true,
        })
        storeTokens(result.accessToken, result.refreshToken)
        setUser(result.user)
      },
      async logout() {
        const refreshToken = localStorage.getItem('habittracker.refreshToken')
        if (refreshToken) {
          await request<void>('/api/auth/logout', {
            method: 'POST',
            body: { refreshToken },
            skipRefresh: true,
          }).catch(() => undefined)
        }
        clearTokens()
        setUser(null)
      },
    }),
    [isBootstrapping, user],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth() {
  const context = useContext(AuthContext)
  if (!context) throw new Error('useAuth must be used within AuthProvider.')
  return context
}

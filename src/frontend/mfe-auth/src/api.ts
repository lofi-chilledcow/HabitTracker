import axios from 'axios'

const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
})

export interface LoginRequest {
  email: string
  password: string
}

export interface RegisterRequest {
  username: string
  email: string
  password: string
}

export interface AuthResponse {
  accessToken: string
  refreshToken: string
  username: string
  email: string
}

export async function login(data: LoginRequest): Promise<AuthResponse> {
  const res = await api.post<{ success: boolean; data: AuthResponse; errors: string[] | null }>(
    '/api/auth/login',
    data,
  )
  if (!res.data.success) throw new Error(res.data.errors?.[0] ?? 'Login failed')
  return res.data.data
}

export async function register(data: RegisterRequest): Promise<AuthResponse> {
  const res = await api.post<{ success: boolean; data: AuthResponse; errors: string[] | null }>(
    '/api/auth/register',
    data,
  )
  if (!res.data.success) throw new Error(res.data.errors?.[0] ?? 'Registration failed')
  return res.data.data
}

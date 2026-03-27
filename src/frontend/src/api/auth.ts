import axios from 'axios'
import type { ApiResponse, AuthResponse } from '../types'
import { storeTokens } from './axios'

const BASE_URL = import.meta.env.VITE_API_URL as string

export async function login(email: string, password: string): Promise<AuthResponse> {
  const { data } = await axios.post<ApiResponse<AuthResponse>>(
    `${BASE_URL}/api/auth/login`,
    { email, password },
  )
  if (!data.success || !data.data) throw new Error(data.errors?.[0] ?? 'Login failed')
  storeTokens(data.data.accessToken, data.data.refreshToken)
  return data.data
}

export async function register(
  username: string,
  email: string,
  password: string,
): Promise<AuthResponse> {
  const { data } = await axios.post<ApiResponse<AuthResponse>>(
    `${BASE_URL}/api/auth/register`,
    { username, email, password },
  )
  if (!data.success || !data.data) throw new Error(data.errors?.[0] ?? 'Registration failed')
  storeTokens(data.data.accessToken, data.data.refreshToken)
  return data.data
}

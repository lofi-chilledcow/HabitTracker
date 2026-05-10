import type { AuthResponse } from './types'

const API_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5000'
const ACCESS_TOKEN_KEY = 'habittracker.accessToken'
const REFRESH_TOKEN_KEY = 'habittracker.refreshToken'

export class ApiError extends Error {
  status: number

  constructor(status: number, message: string) {
    super(message)
    this.status = status
  }
}

let refreshPromise: Promise<AuthResponse> | null = null

export function getAccessToken() {
  return localStorage.getItem(ACCESS_TOKEN_KEY)
}

export function getRefreshToken() {
  return localStorage.getItem(REFRESH_TOKEN_KEY)
}

export function storeTokens(accessToken: string, refreshToken: string) {
  localStorage.setItem(ACCESS_TOKEN_KEY, accessToken)
  localStorage.setItem(REFRESH_TOKEN_KEY, refreshToken)
}

export function clearTokens() {
  localStorage.removeItem(ACCESS_TOKEN_KEY)
  localStorage.removeItem(REFRESH_TOKEN_KEY)
}

async function parseError(response: Response) {
  try {
    const body = await response.json()
    if (typeof body?.error === 'string') return body.error
    if (typeof body?.title === 'string') return body.title
  } catch {
    return response.statusText
  }
  return response.statusText
}

async function refreshTokens() {
  const refreshToken = getRefreshToken()
  if (!refreshToken) throw new ApiError(401, 'Session expired.')

  if (!refreshPromise) {
    refreshPromise = request<AuthResponse>('/api/auth/refresh', {
      method: 'POST',
      body: { refreshToken },
      skipRefresh: true,
    }).finally(() => {
      refreshPromise = null
    })
  }

  const result = await refreshPromise
  storeTokens(result.accessToken, result.refreshToken)
  return result
}

type RequestOptions = Omit<RequestInit, 'body'> & {
  body?: unknown
  skipRefresh?: boolean
}

export async function request<T>(path: string, options: RequestOptions = {}): Promise<T> {
  const headers = new Headers(options.headers)
  headers.set('Content-Type', 'application/json')

  const token = getAccessToken()
  if (token) headers.set('Authorization', `Bearer ${token}`)

  const response = await fetch(`${API_URL}${path}`, {
    ...options,
    headers,
    body: options.body === undefined ? undefined : JSON.stringify(options.body),
  })

  if (response.status === 401 && !options.skipRefresh && getRefreshToken()) {
    await refreshTokens()
    return request<T>(path, { ...options, skipRefresh: true })
  }

  if (!response.ok) {
    throw new ApiError(response.status, await parseError(response))
  }

  if (response.status === 204) return undefined as T
  return (await response.json()) as T
}

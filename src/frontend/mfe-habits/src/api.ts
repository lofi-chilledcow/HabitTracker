import axios from 'axios'
import { getToken } from './tokens'

const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
})

api.interceptors.request.use(config => {
  const token = getToken()
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

export interface Habit {
  id: number
  name: string
  description: string | null
  frequency: string
  createdAt: string
}

export interface CreateHabitRequest {
  name: string
  description?: string
  frequency: string
}

export interface UpdateHabitRequest {
  name: string
  description?: string
  frequency: string
}

export interface HabitCompletion {
  id: number
  habitId: number
  completedAt: string
}

async function unwrap<T>(promise: Promise<{ data: { success: boolean; data: T; errors: string[] | null } }>): Promise<T> {
  const res = await promise
  if (!res.data.success) throw new Error(res.data.errors?.[0] ?? 'Request failed')
  return res.data.data
}

export const getHabits = () =>
  unwrap(api.get<any>('/api/habits'))

export const getHabit = (id: number) =>
  unwrap(api.get<any>(`/api/habits/${id}`))

export const createHabit = (data: CreateHabitRequest) =>
  unwrap(api.post<any>('/api/habits', data))

export const updateHabit = (id: number, data: UpdateHabitRequest) =>
  unwrap(api.put<any>(`/api/habits/${id}`, data))

export const deleteHabit = (id: number) =>
  api.delete(`/api/habits/${id}`)

export const getTodaysCompletions = () =>
  unwrap(api.get<any>('/api/habit-completions/today'))

export const completeHabit = (habitId: number) =>
  unwrap(api.post<any>('/api/habit-completions', { habitId }))

export const deleteCompletion = (id: number) =>
  api.delete(`/api/habit-completions/${id}`)

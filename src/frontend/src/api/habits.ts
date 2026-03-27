import type { ApiResponse, Habit } from '../types'
import { api } from './axios'

export async function getHabits(): Promise<Habit[]> {
  const { data } = await api.get<ApiResponse<Habit[]>>('/api/habits')
  return data.data ?? []
}

export async function createHabit(
  name: string,
  description: string | null,
  frequency: string,
): Promise<Habit> {
  const { data } = await api.post<ApiResponse<Habit>>('/api/habits', {
    name,
    description,
    frequency,
  })
  if (!data.success || !data.data) throw new Error(data.errors?.[0] ?? 'Failed to create habit')
  return data.data
}

export async function updateHabit(
  id: string,
  name: string,
  description: string | null,
  frequency: string,
  isActive: boolean,
): Promise<Habit> {
  const { data } = await api.put<ApiResponse<Habit>>(`/api/habits/${id}`, {
    name,
    description,
    frequency,
    isActive,
  })
  if (!data.success || !data.data) throw new Error(data.errors?.[0] ?? 'Failed to update habit')
  return data.data
}

export async function deleteHabit(id: string): Promise<void> {
  await api.delete(`/api/habits/${id}`)
}

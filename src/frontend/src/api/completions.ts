import type { ApiResponse, HabitCompletion } from '../types'
import { api } from './axios'

export async function getTodaysCompletions(): Promise<HabitCompletion[]> {
  const { data } = await api.get<ApiResponse<HabitCompletion[]>>('/api/habit-completions/today')
  return data.data ?? []
}

export async function createCompletion(
  habitId: string,
  notes?: string,
): Promise<HabitCompletion> {
  const { data } = await api.post<ApiResponse<HabitCompletion>>('/api/habit-completions', {
    habitId,
    notes: notes ?? null,
  })
  if (!data.success || !data.data) throw new Error(data.errors?.[0] ?? 'Failed to record completion')
  return data.data
}

export async function deleteCompletion(id: string): Promise<void> {
  await api.delete(`/api/habit-completions/${id}`)
}

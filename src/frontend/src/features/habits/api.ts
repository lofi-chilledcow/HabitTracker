import { request } from '../../shared/api/client'
import type { Habit, HabitCompletion, HabitInput } from '../../shared/api/types'

export const habitsApi = {
  list: () => request<Habit[]>('/api/habits'),
  get: (id: string) => request<Habit>(`/api/habits/${id}`),
  create: (input: HabitInput) => request<Habit>('/api/habits', { method: 'POST', body: input }),
  update: (id: string, input: HabitInput) => request<Habit>(`/api/habits/${id}`, { method: 'PUT', body: input }),
  archive: (id: string) => request<void>(`/api/habits/${id}`, { method: 'DELETE' }),
  todayCompletions: () => request<HabitCompletion[]>('/api/completions/today'),
  history: (habitId: string) => request<HabitCompletion[]>(`/api/habits/${habitId}/completions`),
  mark: (habitId: string, date: string, notes?: string) =>
    request<HabitCompletion>(`/api/habits/${habitId}/completions/${date}`, { method: 'PUT', body: { notes } }),
  unmark: (habitId: string, date: string) =>
    request<void>(`/api/habits/${habitId}/completions/${date}`, { method: 'DELETE' }),
}

export function todayIso() {
  return new Date().toISOString().slice(0, 10)
}

export interface AuthResponse {
  accessToken: string
  refreshToken: string
  username: string
  email: string
}

export interface Habit {
  id: string
  name: string
  description: string | null
  frequency: string
  createdAt: string
  isActive: boolean
}

export interface HabitCompletion {
  id: string
  habitId: string
  userId: string
  completedDate: string
  notes: string | null
  createdAt: string
}

export interface ApiResponse<T> {
  success: boolean
  data: T | null
  errors: string[] | null
}

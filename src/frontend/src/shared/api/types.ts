export type Role = 'User' | 'Admin'

export type AuthUser = {
  id: string
  username: string
  email: string
  phoneNumber?: string | null
  role: Role
}

export type AuthResponse = {
  accessToken: string
  refreshToken: string
  user: AuthUser
}

export type Habit = {
  id: string
  name: string
  description?: string | null
  frequency: 'daily' | 'weekly'
  targetDaysPerWeek?: number | null
  isPublic: boolean
  createdAt: string
  updatedAt: string
  isActive: boolean
}

export type HabitInput = {
  name: string
  description?: string | null
  frequency: 'daily' | 'weekly'
  targetDaysPerWeek?: number | null
  isPublic: boolean
}

export type HabitCompletion = {
  id: string
  habitId: string
  completedDate: string
  notes?: string | null
  createdAt: string
}

export type LeaderboardEntry = {
  habitId: string
  name: string
  description?: string | null
  frequency: string
  targetDaysPerWeek?: number | null
  completionCount: number
  createdAt: string
}

export type AdminUser = AuthUser & {
  isActive: boolean
  createdAt: string
  updatedAt: string
}

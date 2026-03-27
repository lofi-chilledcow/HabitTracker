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

export interface UserSummary {
  id: number
  username: string
  email: string
  createdAt: string
}

export interface Stats {
  totalUsers: number
  totalHabits: number
  totalCompletions: number
}

export default api

import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import { getHabits } from '../api/habits'
import { getTodaysCompletions, createCompletion, deleteCompletion } from '../api/completions'
import type { Habit, HabitCompletion } from '../types'

export default function Dashboard() {
  const { user, logout } = useAuth()
  const [habits, setHabits] = useState<Habit[]>([])
  const [completions, setCompletions] = useState<HabitCompletion[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    async function load() {
      try {
        const [h, c] = await Promise.all([getHabits(), getTodaysCompletions()])
        setHabits(h.filter((h) => h.isActive))
        setCompletions(c)
      } catch {
        setError('Failed to load data')
      } finally {
        setLoading(false)
      }
    }
    void load()
  }, [])

  function completionFor(habitId: string): HabitCompletion | undefined {
    return completions.find((c) => c.habitId === habitId)
  }

  async function toggle(habit: Habit) {
    const existing = completionFor(habit.id)
    try {
      if (existing) {
        await deleteCompletion(existing.id)
        setCompletions((prev) => prev.filter((c) => c.id !== existing.id))
      } else {
        const c = await createCompletion(habit.id)
        setCompletions((prev) => [...prev, c])
      }
    } catch {
      setError('Failed to update completion')
    }
  }

  const today = new Date().toLocaleDateString('en-US', {
    weekday: 'long',
    month: 'long',
    day: 'numeric',
  })

  const done = habits.filter((h) => completionFor(h.id)).length

  return (
    <div className="min-h-screen bg-gray-50">
      <header className="bg-white border-b border-gray-200 px-6 py-4 flex items-center justify-between">
        <h1 className="text-lg font-bold text-gray-800">HabitTracker</h1>
        <div className="flex items-center gap-4">
          <Link to="/habits" className="text-sm text-indigo-600 hover:underline">
            Manage habits
          </Link>
          <span className="text-sm text-gray-500">{user?.username}</span>
          <button
            onClick={logout}
            className="text-sm text-gray-500 hover:text-gray-700"
          >
            Sign out
          </button>
        </div>
      </header>

      <main className="max-w-lg mx-auto px-4 py-8">
        <div className="mb-6">
          <p className="text-sm text-gray-500">{today}</p>
          <h2 className="text-2xl font-bold text-gray-800 mt-1">Today's habits</h2>
          {!loading && habits.length > 0 && (
            <p className="text-sm text-gray-500 mt-1">
              {done} / {habits.length} completed
            </p>
          )}
        </div>

        {error && (
          <div className="mb-4 rounded-lg bg-red-50 border border-red-200 px-4 py-3 text-sm text-red-700">
            {error}
          </div>
        )}

        {loading ? (
          <p className="text-sm text-gray-400">Loading…</p>
        ) : habits.length === 0 ? (
          <div className="text-center py-16 text-gray-400">
            <p className="mb-2">No habits yet.</p>
            <Link to="/habits" className="text-indigo-600 hover:underline text-sm">
              Create your first habit
            </Link>
          </div>
        ) : (
          <ul className="space-y-3">
            {habits.map((habit) => {
              const completed = !!completionFor(habit.id)
              return (
                <li
                  key={habit.id}
                  className="flex items-center gap-4 bg-white rounded-xl border border-gray-200 px-4 py-4"
                >
                  <button
                    onClick={() => void toggle(habit)}
                    className={`w-6 h-6 rounded-full border-2 flex-shrink-0 flex items-center justify-center transition-colors ${
                      completed
                        ? 'bg-indigo-600 border-indigo-600'
                        : 'border-gray-300 hover:border-indigo-400'
                    }`}
                  >
                    {completed && (
                      <svg className="w-3 h-3 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={3}>
                        <path strokeLinecap="round" strokeLinejoin="round" d="M5 13l4 4L19 7" />
                      </svg>
                    )}
                  </button>
                  <div className="flex-1 min-w-0">
                    <p className={`text-sm font-medium ${completed ? 'line-through text-gray-400' : 'text-gray-800'}`}>
                      {habit.name}
                    </p>
                    {habit.description && (
                      <p className="text-xs text-gray-400 truncate">{habit.description}</p>
                    )}
                  </div>
                  <span className="text-xs text-gray-400 flex-shrink-0">{habit.frequency}</span>
                </li>
              )
            })}
          </ul>
        )}
      </main>
    </div>
  )
}

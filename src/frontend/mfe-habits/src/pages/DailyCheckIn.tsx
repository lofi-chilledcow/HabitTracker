import { useState, useEffect } from 'react'
import { Link } from 'react-router-dom'
import { getHabits, getTodaysCompletions, completeHabit, deleteCompletion } from '../api'
import type { Habit, HabitCompletion } from '../api'

export default function DailyCheckIn() {
  const [habits, setHabits] = useState<Habit[]>([])
  const [completions, setCompletions] = useState<HabitCompletion[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    Promise.all([getHabits(), getTodaysCompletions()])
      .then(([h, c]) => {
        setHabits(h)
        setCompletions(c)
      })
      .catch(err => setError(err.message))
      .finally(() => setLoading(false))
  }, [])

  async function toggle(habit: Habit) {
    const existing = completions.find(c => c.habitId === habit.id)
    if (existing) {
      await deleteCompletion(existing.id)
      setCompletions(prev => prev.filter(c => c.id !== existing.id))
    } else {
      const newCompletion = await completeHabit(habit.id)
      setCompletions(prev => [...prev, newCompletion])
    }
  }

  if (loading) return <div className="p-6 text-sm text-gray-400">Loading...</div>
  if (error) return <div className="p-6 text-sm text-red-500">{error}</div>

  const completedIds = new Set(completions.map(c => c.habitId))

  return (
    <div className="max-w-2xl mx-auto p-6">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-xl font-bold text-gray-900">Today's Check-in</h1>
        <Link to="/habits/list" className="text-sm text-indigo-600 hover:underline">
          Manage habits
        </Link>
      </div>

      {habits.length === 0 ? (
        <div className="text-center py-12 text-gray-400 text-sm">
          <p>No habits yet.</p>
          <Link to="/habits/new" className="mt-2 inline-block text-indigo-600 hover:underline">
            Create your first habit
          </Link>
        </div>
      ) : (
        <ul className="space-y-3">
          {habits.map(habit => {
            const done = completedIds.has(habit.id)
            return (
              <li
                key={habit.id}
                className="flex items-center gap-4 bg-white border border-gray-200 rounded-xl px-4 py-3"
              >
                <button
                  onClick={() => toggle(habit)}
                  className={`w-6 h-6 rounded-full border-2 flex items-center justify-center flex-shrink-0 transition-colors ${
                    done
                      ? 'bg-indigo-600 border-indigo-600 text-white'
                      : 'border-gray-300 hover:border-indigo-400'
                  }`}
                >
                  {done && (
                    <svg className="w-3 h-3" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={3}>
                      <path strokeLinecap="round" strokeLinejoin="round" d="M5 13l4 4L19 7" />
                    </svg>
                  )}
                </button>
                <div>
                  <p className={`text-sm font-medium ${done ? 'line-through text-gray-400' : 'text-gray-900'}`}>
                    {habit.name}
                  </p>
                  {habit.description && (
                    <p className="text-xs text-gray-400 mt-0.5">{habit.description}</p>
                  )}
                </div>
                <span className="ml-auto text-xs text-gray-400 capitalize">{habit.frequency}</span>
              </li>
            )
          })}
        </ul>
      )}

      <p className="mt-6 text-center text-sm text-gray-400">
        {completedIds.size} / {habits.length} completed today
      </p>
    </div>
  )
}

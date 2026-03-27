import { useState, useEffect } from 'react'
import { Link } from 'react-router-dom'
import { getHabits, deleteHabit } from '../api'
import type { Habit } from '../api'

export default function HabitList() {
  const [habits, setHabits] = useState<Habit[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    getHabits()
      .then(setHabits)
      .catch(err => setError(err.message))
      .finally(() => setLoading(false))
  }, [])

  async function handleDelete(id: number) {
    if (!confirm('Delete this habit?')) return
    await deleteHabit(id)
    setHabits(prev => prev.filter(h => h.id !== id))
  }

  if (loading) return <div className="p-6 text-sm text-gray-400">Loading...</div>
  if (error) return <div className="p-6 text-sm text-red-500">{error}</div>

  return (
    <div className="max-w-2xl mx-auto p-6">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-xl font-bold text-gray-900">My Habits</h1>
        <Link
          to="/habits/new"
          className="bg-indigo-600 hover:bg-indigo-700 text-white text-sm font-medium rounded-lg px-4 py-2"
        >
          New habit
        </Link>
      </div>

      {habits.length === 0 ? (
        <div className="text-center py-12 text-gray-400 text-sm">No habits yet.</div>
      ) : (
        <ul className="space-y-3">
          {habits.map(habit => (
            <li
              key={habit.id}
              className="flex items-center gap-4 bg-white border border-gray-200 rounded-xl px-4 py-3"
            >
              <div className="flex-1 min-w-0">
                <p className="text-sm font-medium text-gray-900 truncate">{habit.name}</p>
                {habit.description && (
                  <p className="text-xs text-gray-400 mt-0.5 truncate">{habit.description}</p>
                )}
              </div>
              <span className="text-xs text-gray-400 capitalize flex-shrink-0">{habit.frequency}</span>
              <div className="flex items-center gap-2 flex-shrink-0">
                <Link
                  to={`/habits/${habit.id}/edit`}
                  className="text-xs text-indigo-600 hover:underline"
                >
                  Edit
                </Link>
                <button
                  onClick={() => handleDelete(habit.id)}
                  className="text-xs text-red-500 hover:underline"
                >
                  Delete
                </button>
              </div>
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}

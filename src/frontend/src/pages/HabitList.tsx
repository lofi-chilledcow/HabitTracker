import { useEffect, useState, type FormEvent } from 'react'
import { Link } from 'react-router-dom'
import { getHabits, createHabit, updateHabit, deleteHabit } from '../api/habits'
import type { Habit } from '../types'

interface HabitForm {
  name: string
  description: string
  frequency: string
}

const EMPTY_FORM: HabitForm = { name: '', description: '', frequency: 'Daily' }

export default function HabitList() {
  const [habits, setHabits] = useState<Habit[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [form, setForm] = useState<HabitForm>(EMPTY_FORM)
  const [editingId, setEditingId] = useState<string | null>(null)
  const [saving, setSaving] = useState(false)

  useEffect(() => {
    void load()
  }, [])

  async function load() {
    try {
      setHabits(await getHabits())
    } catch {
      setError('Failed to load habits')
    } finally {
      setLoading(false)
    }
  }

  function startEdit(habit: Habit) {
    setEditingId(habit.id)
    setForm({
      name: habit.name,
      description: habit.description ?? '',
      frequency: habit.frequency,
    })
  }

  function cancelEdit() {
    setEditingId(null)
    setForm(EMPTY_FORM)
  }

  async function handleSubmit(e: FormEvent) {
    e.preventDefault()
    setError(null)
    setSaving(true)
    try {
      if (editingId) {
        const existing = habits.find((h) => h.id === editingId)!
        const updated = await updateHabit(
          editingId,
          form.name,
          form.description || null,
          form.frequency,
          existing.isActive,
        )
        setHabits((prev) => prev.map((h) => (h.id === editingId ? updated : h)))
        setEditingId(null)
      } else {
        const created = await createHabit(form.name, form.description || null, form.frequency)
        setHabits((prev) => [...prev, created])
      }
      setForm(EMPTY_FORM)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to save habit')
    } finally {
      setSaving(false)
    }
  }

  async function handleDelete(id: string) {
    if (!confirm('Delete this habit?')) return
    try {
      await deleteHabit(id)
      setHabits((prev) => prev.filter((h) => h.id !== id))
    } catch {
      setError('Failed to delete habit')
    }
  }

  async function handleToggleActive(habit: Habit) {
    try {
      const updated = await updateHabit(
        habit.id,
        habit.name,
        habit.description,
        habit.frequency,
        !habit.isActive,
      )
      setHabits((prev) => prev.map((h) => (h.id === habit.id ? updated : h)))
    } catch {
      setError('Failed to update habit')
    }
  }

  return (
    <div className="min-h-screen bg-gray-50">
      <header className="bg-white border-b border-gray-200 px-6 py-4 flex items-center gap-4">
        <Link to="/" className="text-sm text-indigo-600 hover:underline">
          ← Dashboard
        </Link>
        <h1 className="text-lg font-bold text-gray-800">Manage habits</h1>
      </header>

      <main className="max-w-lg mx-auto px-4 py-8 space-y-8">
        {error && (
          <div className="rounded-lg bg-red-50 border border-red-200 px-4 py-3 text-sm text-red-700">
            {error}
          </div>
        )}

        {/* Add / edit form */}
        <div className="bg-white rounded-2xl border border-gray-200 p-6">
          <h2 className="text-sm font-semibold text-gray-700 mb-4">
            {editingId ? 'Edit habit' : 'New habit'}
          </h2>
          <form onSubmit={handleSubmit} className="space-y-3">
            <input
              type="text"
              required
              placeholder="Name"
              value={form.name}
              onChange={(e) => setForm((f) => ({ ...f, name: e.target.value }))}
              className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
            />
            <input
              type="text"
              placeholder="Description (optional)"
              value={form.description}
              onChange={(e) => setForm((f) => ({ ...f, description: e.target.value }))}
              className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
            />
            <select
              value={form.frequency}
              onChange={(e) => setForm((f) => ({ ...f, frequency: e.target.value }))}
              className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
            >
              <option>Daily</option>
              <option>Weekly</option>
            </select>
            <div className="flex gap-2">
              <button
                type="submit"
                disabled={saving}
                className="flex-1 rounded-lg bg-indigo-600 px-4 py-2 text-sm font-semibold text-white hover:bg-indigo-700 disabled:opacity-50"
              >
                {saving ? 'Saving…' : editingId ? 'Save changes' : 'Add habit'}
              </button>
              {editingId && (
                <button
                  type="button"
                  onClick={cancelEdit}
                  className="rounded-lg border border-gray-300 px-4 py-2 text-sm text-gray-600 hover:bg-gray-50"
                >
                  Cancel
                </button>
              )}
            </div>
          </form>
        </div>

        {/* Habit list */}
        {loading ? (
          <p className="text-sm text-gray-400">Loading…</p>
        ) : habits.length === 0 ? (
          <p className="text-sm text-gray-400 text-center py-8">No habits yet.</p>
        ) : (
          <ul className="space-y-3">
            {habits.map((habit) => (
              <li
                key={habit.id}
                className={`bg-white rounded-xl border px-4 py-4 flex items-start gap-3 ${
                  habit.isActive ? 'border-gray-200' : 'border-gray-100 opacity-60'
                }`}
              >
                <div className="flex-1 min-w-0">
                  <p className="text-sm font-medium text-gray-800">{habit.name}</p>
                  {habit.description && (
                    <p className="text-xs text-gray-400 mt-0.5">{habit.description}</p>
                  )}
                  <p className="text-xs text-gray-400 mt-1">{habit.frequency}</p>
                </div>
                <div className="flex items-center gap-2 flex-shrink-0">
                  <button
                    onClick={() => void handleToggleActive(habit)}
                    className={`text-xs px-2 py-1 rounded-md border ${
                      habit.isActive
                        ? 'border-green-200 text-green-700 bg-green-50 hover:bg-green-100'
                        : 'border-gray-200 text-gray-500 bg-gray-50 hover:bg-gray-100'
                    }`}
                  >
                    {habit.isActive ? 'Active' : 'Inactive'}
                  </button>
                  <button
                    onClick={() => startEdit(habit)}
                    className="text-xs px-2 py-1 rounded-md border border-gray-200 text-gray-600 hover:bg-gray-50"
                  >
                    Edit
                  </button>
                  <button
                    onClick={() => void handleDelete(habit.id)}
                    className="text-xs px-2 py-1 rounded-md border border-red-200 text-red-600 hover:bg-red-50"
                  >
                    Delete
                  </button>
                </div>
              </li>
            ))}
          </ul>
        )}
      </main>
    </div>
  )
}

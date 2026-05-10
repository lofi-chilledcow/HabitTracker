import { FormEvent, useEffect, useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Save } from 'lucide-react'
import { useNavigate, useParams } from 'react-router-dom'
import { ApiError } from '../../shared/api/client'
import type { HabitInput } from '../../shared/api/types'
import { ErrorState, LoadingState } from '../../shared/ui/State'
import { habitsApi } from './api'

export function HabitEditorPage({ mode }: { mode: 'create' | 'edit' }) {
  const { id } = useParams()
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const habit = useQuery({ queryKey: ['habit', id], queryFn: () => habitsApi.get(id!), enabled: mode === 'edit' && Boolean(id) })
  const [form, setForm] = useState<HabitInput>({ name: '', description: '', frequency: 'daily', targetDaysPerWeek: null, isPublic: false })
  const [error, setError] = useState('')

  useEffect(() => {
    if (habit.data) {
      setForm({
        name: habit.data.name,
        description: habit.data.description,
        frequency: habit.data.frequency,
        targetDaysPerWeek: habit.data.targetDaysPerWeek,
        isPublic: habit.data.isPublic,
      })
    }
  }, [habit.data])

  const save = useMutation({
    mutationFn: (input: HabitInput) => (mode === 'create' ? habitsApi.create(input) : habitsApi.update(id!, input)),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['habits'] })
      navigate('/habits')
    },
    onError: (err) => setError(err instanceof ApiError ? err.message : 'Could not save habit.'),
  })

  if (habit.isLoading) return <LoadingState label="Loading habit" />
  if (habit.isError) return <ErrorState message="Could not load habit." />

  function handleSubmit(event: FormEvent) {
    event.preventDefault()
    setError('')
    save.mutate({
      ...form,
      description: form.description?.trim() || null,
      targetDaysPerWeek: form.frequency === 'weekly' ? Number(form.targetDaysPerWeek) : null,
    })
  }

  return (
    <section className="page narrow">
      <header className="page-header">
        <div>
          <span className="eyebrow">{mode === 'create' ? 'New habit' : 'Edit habit'}</span>
          <h1>{mode === 'create' ? 'Create habit' : 'Update habit'}</h1>
        </div>
      </header>
      <form className="form surface" onSubmit={handleSubmit}>
        <label>
          <span>Name</span>
          <input value={form.name} onChange={(event) => setForm({ ...form, name: event.target.value })} required />
        </label>
        <label>
          <span>Description</span>
          <textarea value={form.description ?? ''} onChange={(event) => setForm({ ...form, description: event.target.value })} rows={4} />
        </label>
        <label>
          <span>Frequency</span>
          <select value={form.frequency} onChange={(event) => setForm({ ...form, frequency: event.target.value as HabitInput['frequency'], targetDaysPerWeek: null })}>
            <option value="daily">Daily</option>
            <option value="weekly">Weekly</option>
          </select>
        </label>
        {form.frequency === 'weekly' ? (
          <label>
            <span>Target days per week</span>
            <input type="number" min={1} max={7} value={form.targetDaysPerWeek ?? ''} onChange={(event) => setForm({ ...form, targetDaysPerWeek: Number(event.target.value) })} required />
          </label>
        ) : null}
        <label className="checkbox-row">
          <input type="checkbox" checked={form.isPublic} onChange={(event) => setForm({ ...form, isPublic: event.target.checked })} />
          <span>Show on public leaderboard</span>
        </label>
        {error ? <p className="form-error">{error}</p> : null}
        <button className="primary-button" type="submit" disabled={save.isPending}><Save size={18} />Save</button>
      </form>
    </section>
  )
}

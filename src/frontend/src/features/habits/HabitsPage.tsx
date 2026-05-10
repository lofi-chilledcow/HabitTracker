import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Edit3, Plus, Trash2 } from 'lucide-react'
import { Link } from 'react-router-dom'
import { habitsApi } from './api'
import { EmptyState, ErrorState, LoadingState } from '../../shared/ui/State'

export function HabitsPage() {
  const queryClient = useQueryClient()
  const habits = useQuery({ queryKey: ['habits'], queryFn: habitsApi.list })
  const archive = useMutation({
    mutationFn: habitsApi.archive,
    onSuccess: () => void queryClient.invalidateQueries({ queryKey: ['habits'] }),
  })

  if (habits.isLoading) return <LoadingState label="Loading habits" />
  if (habits.isError) return <ErrorState message="Could not load habits." />

  return (
    <section className="page">
      <header className="page-header">
        <div>
          <span className="eyebrow">Habits</span>
          <h1>Manage habits</h1>
        </div>
        <Link className="primary-button" to="/habits/new"><Plus size={18} />New habit</Link>
      </header>
      {(habits.data ?? []).length === 0 ? (
        <EmptyState title="No habits yet" detail="Create your first habit." />
      ) : (
        <div className="table-list">
          {(habits.data ?? []).map((habit) => (
            <article key={habit.id} className="table-row">
              <div>
                <strong>{habit.name}</strong>
                <span>{habit.description || 'No description'}</span>
              </div>
              <div className="row-meta">
                <span>{habit.frequency}</span>
                <span>{habit.isPublic ? 'Public' : 'Private'}</span>
              </div>
              <div className="row-actions">
                <Link className="icon-button" to={`/habits/${habit.id}/edit`} aria-label="Edit habit" title="Edit habit"><Edit3 size={17} /></Link>
                <button className="icon-button danger" onClick={() => archive.mutate(habit.id)} aria-label="Archive habit" title="Archive habit"><Trash2 size={17} /></button>
              </div>
            </article>
          ))}
        </div>
      )}
    </section>
  )
}

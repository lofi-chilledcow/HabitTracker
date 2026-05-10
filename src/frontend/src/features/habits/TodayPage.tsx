import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Check, Circle, Plus } from 'lucide-react'
import { Link } from 'react-router-dom'
import { habitsApi, todayIso } from './api'
import { EmptyState, ErrorState, LoadingState } from '../../shared/ui/State'
import type { HabitCompletion } from '../../shared/api/types'

export function TodayPage() {
  const queryClient = useQueryClient()
  const habits = useQuery({ queryKey: ['habits'], queryFn: habitsApi.list })
  const completions = useQuery({ queryKey: ['completions', 'today'], queryFn: habitsApi.todayCompletions })
  const date = todayIso()

  const toggle = useMutation({
    mutationFn: async ({ habitId, done }: { habitId: string; done: boolean }) =>
      done ? habitsApi.unmark(habitId, date) : habitsApi.mark(habitId, date),
    onMutate: async ({ habitId, done }) => {
      await queryClient.cancelQueries({ queryKey: ['completions', 'today'] })
      const previous = queryClient.getQueryData<HabitCompletion[]>(['completions', 'today']) ?? []
      const next = done
        ? previous.filter((completion) => completion.habitId !== habitId)
        : [...previous, { id: `optimistic-${habitId}`, habitId, completedDate: date, createdAt: new Date().toISOString() }]
      queryClient.setQueryData(['completions', 'today'], next)
      return { previous }
    },
    onError: (_err, _vars, context) => {
      queryClient.setQueryData(['completions', 'today'], context?.previous ?? [])
    },
    onSettled: () => {
      void queryClient.invalidateQueries({ queryKey: ['completions', 'today'] })
    },
  })

  if (habits.isLoading || completions.isLoading) return <LoadingState label="Loading today" />
  if (habits.isError || completions.isError) return <ErrorState message="Could not load today's habits." />

  const completionIds = new Set((completions.data ?? []).map((completion) => completion.habitId))
  const activeHabits = habits.data ?? []

  return (
    <section className="page">
      <header className="page-header">
        <div>
          <span className="eyebrow">Today</span>
          <h1>Daily check-in</h1>
        </div>
        <Link className="secondary-button" to="/habits/new"><Plus size={18} />New habit</Link>
      </header>
      {activeHabits.length === 0 ? (
        <EmptyState title="No habits yet" detail="Create a habit to start tracking today." />
      ) : (
        <div className="habit-list">
          {activeHabits.map((habit) => {
            const done = completionIds.has(habit.id)
            return (
              <button key={habit.id} className={done ? 'habit-row done' : 'habit-row'} onClick={() => toggle.mutate({ habitId: habit.id, done })}>
                <span className="check-icon">{done ? <Check size={18} /> : <Circle size={18} />}</span>
                <span>
                  <strong>{habit.name}</strong>
                  <small>{habit.frequency === 'weekly' ? `${habit.targetDaysPerWeek} days/week` : 'Daily'}</small>
                </span>
              </button>
            )
          })}
        </div>
      )}
    </section>
  )
}

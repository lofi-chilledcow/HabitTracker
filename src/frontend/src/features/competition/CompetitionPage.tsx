import { useQuery } from '@tanstack/react-query'
import { Trophy } from 'lucide-react'
import { request } from '../../shared/api/client'
import type { LeaderboardEntry } from '../../shared/api/types'
import { EmptyState, ErrorState, LoadingState } from '../../shared/ui/State'

export function CompetitionPage() {
  const leaderboard = useQuery({
    queryKey: ['leaderboard'],
    queryFn: () => request<LeaderboardEntry[]>('/api/competition/leaderboard?days=30&limit=50'),
  })

  if (leaderboard.isLoading) return <LoadingState label="Loading leaderboard" />
  if (leaderboard.isError) return <ErrorState message="Could not load leaderboard." />

  return (
    <section className="page">
      <header className="page-header">
        <div>
          <span className="eyebrow">Competition</span>
          <h1>Leaderboard</h1>
        </div>
      </header>
      {(leaderboard.data ?? []).length === 0 ? (
        <EmptyState title="No public habits yet" detail="Public habits appear here once they have completions." />
      ) : (
        <div className="leaderboard">
          {(leaderboard.data ?? []).map((entry, index) => (
            <article className="leader-row" key={entry.habitId}>
              <span className="rank">{index + 1}</span>
              <div>
                <strong>{entry.name}</strong>
                <span>{entry.description || entry.frequency}</span>
                <span>by {entry.username}</span>
              </div>
              <div className="score"><Trophy size={17} />{entry.completionCount}</div>
            </article>
          ))}
        </div>
      )}
    </section>
  )
}

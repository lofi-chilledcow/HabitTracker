import { Routes, Route, Navigate } from 'react-router-dom'
import Leaderboard from './pages/Leaderboard'

export default function CompetitionApp() {
  return (
    <Routes>
      <Route path="/competition" element={<Leaderboard />} />
      <Route path="*" element={<Navigate to="/competition" replace />} />
    </Routes>
  )
}

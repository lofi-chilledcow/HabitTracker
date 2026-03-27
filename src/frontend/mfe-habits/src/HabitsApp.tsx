import { Routes, Route, Navigate } from 'react-router-dom'
import DailyCheckIn from './pages/DailyCheckIn'
import HabitList from './pages/HabitList'
import CreateEditHabit from './pages/CreateEditHabit'

export default function HabitsApp() {
  return (
    <Routes>
      <Route path="/habits" element={<DailyCheckIn />} />
      <Route path="/habits/list" element={<HabitList />} />
      <Route path="/habits/new" element={<CreateEditHabit />} />
      <Route path="/habits/:id/edit" element={<CreateEditHabit />} />
      <Route path="*" element={<Navigate to="/habits" replace />} />
    </Routes>
  )
}

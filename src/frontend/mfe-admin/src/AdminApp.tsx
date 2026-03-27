import { Routes, Route, Navigate } from 'react-router-dom'
import AdminDashboard from './pages/AdminDashboard'
import UserManagement from './pages/UserManagement'

export default function AdminApp() {
  return (
    <Routes>
      <Route path="/admin" element={<AdminDashboard />} />
      <Route path="/admin/users" element={<UserManagement />} />
      <Route path="*" element={<Navigate to="/admin" replace />} />
    </Routes>
  )
}

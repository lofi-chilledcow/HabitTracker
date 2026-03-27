import { Link } from 'react-router-dom'

export default function AdminDashboard() {
  return (
    <div className="max-w-4xl mx-auto p-6">
      <h1 className="text-xl font-bold text-gray-900 mb-6">Admin Dashboard</h1>
      <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
        <Link
          to="/admin/users"
          className="bg-white border border-gray-200 rounded-xl p-6 hover:border-indigo-300 transition-colors"
        >
          <h2 className="text-sm font-semibold text-gray-900">User Management</h2>
          <p className="text-xs text-gray-400 mt-1">View and manage registered users</p>
        </Link>
      </div>
    </div>
  )
}

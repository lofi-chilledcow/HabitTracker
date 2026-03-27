import { Link } from 'react-router-dom'

export default function UserManagement() {
  return (
    <div className="max-w-4xl mx-auto p-6">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-xl font-bold text-gray-900">Users</h1>
        <Link to="/admin" className="text-sm text-indigo-600 hover:underline">
          Back to dashboard
        </Link>
      </div>
      <p className="text-sm text-gray-400">User management coming soon.</p>
    </div>
  )
}

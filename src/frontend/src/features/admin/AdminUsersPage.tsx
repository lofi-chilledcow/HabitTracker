import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useMemo, useState } from 'react'
import { Power, PowerOff, Search, ShieldCheck, UserCog, Users } from 'lucide-react'
import { request } from '../../shared/api/client'
import type { AdminUser } from '../../shared/api/types'
import { useAuth } from '../../shared/auth/AuthProvider'
import { EmptyState, ErrorState, LoadingState } from '../../shared/ui/State'

type UserFilter = 'all' | 'active' | 'disabled' | 'admins'

export function AdminUsersPage() {
  const queryClient = useQueryClient()
  const { user: currentUser } = useAuth()
  const [query, setQuery] = useState('')
  const [filter, setFilter] = useState<UserFilter>('all')
  const [actionError, setActionError] = useState<string | null>(null)
  const users = useQuery({ queryKey: ['admin-users'], queryFn: () => request<AdminUser[]>('/api/admin/users') })
  const status = useMutation({
    mutationFn: ({ id, isActive }: { id: string; isActive: boolean }) =>
      request<AdminUser>(`/api/admin/users/${id}/status`, { method: 'PATCH', body: { isActive } }),
    onMutate: () => setActionError(null),
    onSuccess: () => void queryClient.invalidateQueries({ queryKey: ['admin-users'] }),
    onError: (error) => setActionError(error instanceof Error ? error.message : 'Could not update user status.'),
  })
  const role = useMutation({
    mutationFn: ({ id, nextRole }: { id: string; nextRole: 'User' | 'Admin' }) =>
      request<AdminUser>(`/api/admin/users/${id}/role`, { method: 'PATCH', body: { role: nextRole } }),
    onMutate: () => setActionError(null),
    onSuccess: () => void queryClient.invalidateQueries({ queryKey: ['admin-users'] }),
    onError: (error) => setActionError(error instanceof Error ? error.message : 'Could not update user role.'),
  })

  const allUsers = users.data ?? []
  const visibleUsers = useMemo(() => {
    const normalizedQuery = query.trim().toLowerCase()
    return allUsers.filter((adminUser) => {
      const matchesQuery = normalizedQuery.length === 0
        || adminUser.username.toLowerCase().includes(normalizedQuery)
        || adminUser.email.toLowerCase().includes(normalizedQuery)
        || adminUser.phoneNumber?.toLowerCase().includes(normalizedQuery)
      const matchesFilter =
        filter === 'all'
        || (filter === 'active' && adminUser.isActive)
        || (filter === 'disabled' && !adminUser.isActive)
        || (filter === 'admins' && adminUser.role === 'Admin')

      return matchesQuery && matchesFilter
    })
  }, [allUsers, filter, query])

  const activeCount = allUsers.filter((adminUser) => adminUser.isActive).length
  const adminCount = allUsers.filter((adminUser) => adminUser.role === 'Admin').length
  const isMutating = status.isPending || role.isPending

  if (users.isLoading) return <LoadingState label="Loading users" />
  if (users.isError) return <ErrorState message="Could not load users." />

  return (
    <section className="page">
      <header className="page-header">
        <div>
          <span className="eyebrow">Admin</span>
          <h1>User management</h1>
        </div>
      </header>

      <div className="admin-summary" aria-label="User totals">
        <SummaryItem icon={<Users size={18} />} label="Users" value={allUsers.length} />
        <SummaryItem icon={<Power size={18} />} label="Active" value={activeCount} />
        <SummaryItem icon={<ShieldCheck size={18} />} label="Admins" value={adminCount} />
      </div>

      <div className="admin-toolbar">
        <label className="search-field">
          <Search size={17} />
          <input
            value={query}
            onChange={(event) => setQuery(event.target.value)}
            placeholder="Search username, email, or phone"
            aria-label="Search users"
          />
        </label>
        <select value={filter} onChange={(event) => setFilter(event.target.value as UserFilter)} aria-label="Filter users">
          <option value="all">All users</option>
          <option value="active">Active</option>
          <option value="disabled">Disabled</option>
          <option value="admins">Admins</option>
        </select>
      </div>

      {actionError ? <p className="form-error">{actionError}</p> : null}

      {visibleUsers.length === 0 ? (
        <EmptyState title="No users found" detail="Adjust the search or filter." />
      ) : (
        <div className="table-list">
          {visibleUsers.map((user) => {
            const isCurrentUser = user.id === currentUser?.id
            return (
              <article key={user.id} className={user.isActive ? 'table-row user-row' : 'table-row user-row disabled'}>
                <div>
                  <strong>{user.username}</strong>
                  <span>{user.email}</span>
                  {user.phoneNumber ? <span>{user.phoneNumber}</span> : null}
                </div>
                <div className="row-meta">
                  <span className={user.isActive ? 'status-pill active' : 'status-pill disabled'}>{user.isActive ? 'Active' : 'Disabled'}</span>
                  <span><ShieldCheck size={14} />{user.role}</span>
                  <span>Joined {new Date(user.createdAt).toLocaleDateString()}</span>
                </div>
                <div className="row-actions wide">
                  <button
                    className="secondary-button"
                    onClick={() => status.mutate({ id: user.id, isActive: !user.isActive })}
                    disabled={isMutating || isCurrentUser}
                    title={isCurrentUser ? 'You cannot disable your own account here' : undefined}
                  >
                    {user.isActive ? <PowerOff size={16} /> : <Power size={16} />}
                    {user.isActive ? 'Disable' : 'Enable'}
                  </button>
                  <button
                    className="secondary-button"
                    onClick={() => role.mutate({ id: user.id, nextRole: user.role === 'Admin' ? 'User' : 'Admin' })}
                    disabled={isMutating || isCurrentUser}
                    title={isCurrentUser ? 'You cannot change your own role here' : undefined}
                  >
                    <UserCog size={16} />
                    Make {user.role === 'Admin' ? 'User' : 'Admin'}
                  </button>
                </div>
              </article>
            )
          })}
        </div>
      )}
    </section>
  )
}

function SummaryItem({ icon, label, value }: { icon: React.ReactNode; label: string; value: number }) {
  return (
    <div className="summary-item">
      {icon}
      <span>{label}</span>
      <strong>{value}</strong>
    </div>
  )
}

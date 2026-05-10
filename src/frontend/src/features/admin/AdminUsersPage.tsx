import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { ShieldCheck } from 'lucide-react'
import { request } from '../../shared/api/client'
import type { AdminUser } from '../../shared/api/types'
import { ErrorState, LoadingState } from '../../shared/ui/State'

export function AdminUsersPage() {
  const queryClient = useQueryClient()
  const users = useQuery({ queryKey: ['admin-users'], queryFn: () => request<AdminUser[]>('/api/admin/users') })
  const status = useMutation({
    mutationFn: ({ id, isActive }: { id: string; isActive: boolean }) =>
      request<AdminUser>(`/api/admin/users/${id}/status`, { method: 'PATCH', body: { isActive } }),
    onSuccess: () => void queryClient.invalidateQueries({ queryKey: ['admin-users'] }),
  })
  const role = useMutation({
    mutationFn: ({ id, nextRole }: { id: string; nextRole: 'User' | 'Admin' }) =>
      request<AdminUser>(`/api/admin/users/${id}/role`, { method: 'PATCH', body: { role: nextRole } }),
    onSuccess: () => void queryClient.invalidateQueries({ queryKey: ['admin-users'] }),
  })

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
      <div className="table-list">
        {(users.data ?? []).map((user) => (
          <article key={user.id} className="table-row user-row">
            <div>
              <strong>{user.username}</strong>
              <span>{user.email}</span>
            </div>
            <div className="row-meta">
              <span>{user.isActive ? 'Active' : 'Disabled'}</span>
              <span><ShieldCheck size={14} />{user.role}</span>
            </div>
            <div className="row-actions wide">
              <button className="secondary-button" onClick={() => status.mutate({ id: user.id, isActive: !user.isActive })}>
                {user.isActive ? 'Disable' : 'Enable'}
              </button>
              <button className="secondary-button" onClick={() => role.mutate({ id: user.id, nextRole: user.role === 'Admin' ? 'User' : 'Admin' })}>
                Make {user.role === 'Admin' ? 'User' : 'Admin'}
              </button>
            </div>
          </article>
        ))}
      </div>
    </section>
  )
}

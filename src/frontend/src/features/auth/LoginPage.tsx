import { FormEvent, useState } from 'react'
import { Link, useLocation, useNavigate } from 'react-router-dom'
import { LogIn } from 'lucide-react'
import { ApiError } from '../../shared/api/client'
import { useAuth } from '../../shared/auth/AuthProvider'

export function LoginPage() {
  const [identifier, setIdentifier] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)
  const { login } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()
    setError('')
    setIsSubmitting(true)
    try {
      await login(identifier, password)
      navigate((location.state as { from?: string } | null)?.from ?? '/habits/today', { replace: true })
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Login failed.')
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <AuthFrame title="Welcome back" subtitle="Sign in with email, username, or phone.">
      <form className="form" onSubmit={handleSubmit}>
        <label>
          <span>Email, username, or phone</span>
          <input value={identifier} onChange={(event) => setIdentifier(event.target.value)} required autoFocus />
        </label>
        <label>
          <span>Password</span>
          <input type="password" value={password} onChange={(event) => setPassword(event.target.value)} required />
        </label>
        {error ? <p className="form-error">{error}</p> : null}
        <button className="primary-button" type="submit" disabled={isSubmitting}>
          <LogIn size={18} />
          {isSubmitting ? 'Signing in' : 'Sign in'}
        </button>
        <p className="auth-link">New here? <Link to="/auth/register">Create an account</Link></p>
      </form>
    </AuthFrame>
  )
}

export function AuthFrame({ title, subtitle, children }: { title: string; subtitle: string; children: React.ReactNode }) {
  return (
    <main className="auth-screen">
      <section className="auth-panel">
        <div>
          <span className="eyebrow">HabitTracker</span>
          <h1>{title}</h1>
          <p>{subtitle}</p>
        </div>
        {children}
      </section>
    </main>
  )
}

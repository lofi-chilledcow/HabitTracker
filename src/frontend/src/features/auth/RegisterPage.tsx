import { FormEvent, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { UserPlus } from 'lucide-react'
import { ApiError } from '../../shared/api/client'
import { useAuth } from '../../shared/auth/AuthProvider'
import { AuthFrame } from './LoginPage'

export function RegisterPage() {
  const [username, setUsername] = useState('')
  const [email, setEmail] = useState('')
  const [phoneNumber, setPhoneNumber] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)
  const { register } = useAuth()
  const navigate = useNavigate()

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()
    setError('')
    setIsSubmitting(true)
    try {
      await register({ username, email, phoneNumber: phoneNumber || undefined, password })
      navigate('/habits/today', { replace: true })
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Registration failed.')
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <AuthFrame title="Create your account" subtitle="Set up a private habit workspace.">
      <form className="form" onSubmit={handleSubmit}>
        <label>
          <span>Username</span>
          <input value={username} onChange={(event) => setUsername(event.target.value)} required autoFocus />
        </label>
        <label>
          <span>Email</span>
          <input type="email" value={email} onChange={(event) => setEmail(event.target.value)} required />
        </label>
        <label>
          <span>Phone</span>
          <input value={phoneNumber} onChange={(event) => setPhoneNumber(event.target.value)} />
        </label>
        <label>
          <span>Password</span>
          <input type="password" value={password} onChange={(event) => setPassword(event.target.value)} required />
        </label>
        {error ? <p className="form-error">{error}</p> : null}
        <button className="primary-button" type="submit" disabled={isSubmitting}>
          <UserPlus size={18} />
          {isSubmitting ? 'Creating' : 'Create account'}
        </button>
        <p className="auth-link">Already have one? <Link to="/auth/login">Sign in</Link></p>
      </form>
    </AuthFrame>
  )
}

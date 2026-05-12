export function validateRegistration(input: {
  username: string
  email: string
  phoneNumber?: string
  password: string
}) {
  const errors: string[] = []
  const username = input.username.trim()
  const email = input.email.trim()
  const phoneDigits = input.phoneNumber?.replace(/\D/g, '') ?? ''

  if (username.length < 3) errors.push('Username: Must be at least 3 characters.')
  if (/\s/.test(username)) errors.push('Username: Must not contain spaces.')

  if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
    errors.push('Email: Must be a valid email address.')
  }

  if (input.phoneNumber && phoneDigits.length < 10) {
    errors.push('Phone: Must include at least 10 digits.')
  }

  if (input.password.length < 8) errors.push('Password: Must be at least 8 characters.')
  if (!/[A-Z]/.test(input.password)) errors.push('Password: Must contain at least one uppercase letter.')
  if (!/[0-9]/.test(input.password)) errors.push('Password: Must contain at least one number.')

  return errors
}

export function validateLogin(input: { identifier: string; password: string }) {
  const errors: string[] = []

  if (!input.identifier.trim()) errors.push('Login: Email, username, or phone is required.')
  if (!input.password) errors.push('Password: Required.')

  return errors
}

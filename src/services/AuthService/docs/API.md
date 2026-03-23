# AuthService API

Authentication service for HabitTracker. Handles user registration, login, and JWT token management.

- **Base URL (dev):** `http://localhost:5039`
- **Content-Type:** `application/json`
- **Auth scheme:** Bearer JWT (`Authorization: Bearer <accessToken>`)

---

## Endpoints

| Method | Path | Description |
|--------|------|-------------|
| POST | `/api/auth/register` | Register a new user |
| POST | `/api/auth/login` | Log in and receive tokens |
| POST | `/api/auth/refresh` | Exchange a refresh token for a new access token |

---

## POST `/api/auth/register`

Creates a new user account with the default `User` role. Password is hashed with BCrypt before storage.

### Request body

```json
{
  "username": "johndoe",
  "email": "john@example.com",
  "password": "Secret123"
}
```

| Field | Type | Rules |
|-------|------|-------|
| `username` | string | Required. Min 3 characters. No spaces. |
| `email` | string | Required. Valid email format. |
| `password` | string | Required. Min 8 chars. At least one uppercase letter. At least one number. |

### Responses

#### `200 OK` — Registration successful

```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "dGhpcyBpcyBhIHJhbmRvbSByZWZyZXNoIHRva2Vu...",
  "username": "johndoe",
  "email": "john@example.com"
}
```

#### `400 Bad Request` — Validation failed

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Username": ["Username must be at least 3 characters."],
    "Password": [
      "Password must be at least 8 characters.",
      "Password must contain at least one uppercase letter.",
      "Password must contain at least one number."
    ]
  }
}
```

#### `409 Conflict` — Duplicate email or username

```json
{ "error": "Email is already registered." }
```

```json
{ "error": "Username is already taken." }
```

---

## POST `/api/auth/login`

Authenticates an existing user and issues a new access token and refresh token.

### Request body

```json
{
  "email": "john@example.com",
  "password": "Secret123"
}
```

| Field | Type | Rules |
|-------|------|-------|
| `email` | string | Required. Valid email format. |
| `password` | string | Required. |

### Responses

#### `200 OK` — Login successful

```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "dGhpcyBpcyBhIHJhbmRvbSByZWZyZXNoIHRva2Vu...",
  "username": "johndoe",
  "email": "john@example.com"
}
```

#### `400 Bad Request` — Validation failed

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Email": ["Email must be a valid email address."]
  }
}
```

#### `401 Unauthorized` — Invalid credentials

```json
{ "error": "Invalid email or password." }
```

> Response is deliberately vague to prevent user enumeration.

---

## POST `/api/auth/refresh`

Exchanges a valid refresh token for a new access token. The submitted token is immediately revoked and replaced (token rotation).

### Request body

```json
{
  "refreshToken": "dGhpcyBpcyBhIHJhbmRvbSByZWZyZXNoIHRva2Vu..."
}
```

| Field | Type | Rules |
|-------|------|-------|
| `refreshToken` | string | Required. Must be active (not expired or revoked). |

### Responses

#### `200 OK` — Token refreshed

```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "bmV3UmVmcmVzaFRva2VuVmFsdWVIZXJl...",
  "username": "johndoe",
  "email": "john@example.com"
}
```

> Store the new `refreshToken` immediately — the previous one is now invalid.

#### `401 Unauthorized` — Token invalid, expired, or already used

```json
{ "error": "Invalid or expired refresh token." }
```

---

## Schemas

### `AuthResponse`

Returned by all three endpoints on success.

| Field | Type | Description |
|-------|------|-------------|
| `accessToken` | string | Signed JWT. Valid for **60 minutes**. |
| `refreshToken` | string | Opaque random token (64-byte CSPRNG, base64). Valid for **30 days**. Single-use. |
| `username` | string | The authenticated user's username. |
| `email` | string | The authenticated user's email. |

### JWT Access Token Claims

| Claim | Description |
|-------|-------------|
| `sub` | User ID (GUID) |
| `email` | User's email address |
| `name` | Username |
| `role` | Role name (`User` or `Admin`) |
| `jti` | Unique token ID (GUID) |
| `iss` | `AuthService` |
| `aud` | `HabitTrackerApp` |
| `exp` | Expiry — 60 minutes from issue time |

---

## Status Code Reference

| Code | When |
|------|------|
| `200 OK` | Request succeeded |
| `400 Bad Request` | Input failed FluentValidation rules |
| `401 Unauthorized` | Invalid credentials or token |
| `409 Conflict` | Email or username already registered |

---

## Error Formats

Two distinct shapes depending on failure type.

### Validation error (`400`)

Produced by FluentValidation before the handler runs. Multiple messages per field are possible.

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "<FieldName>": ["<message>", "..."]
  }
}
```

### Application error (`401`, `409`)

Produced when business logic fails.

```json
{
  "error": "<message>"
}
```

---

## Validation Rules

### Register

| Field | Rules |
|-------|-------|
| `username` | Not empty · min 3 characters · no spaces |
| `email` | Not empty · valid email format |
| `password` | Not empty · min 8 characters · ≥1 uppercase letter · ≥1 number |

### Login

| Field | Rules |
|-------|-------|
| `email` | Not empty · valid email format |
| `password` | Not empty |

---

## Token Lifecycle

```
Register / Login
      │
      ├─ accessToken  (JWT, 60 min) ──► include as Bearer token on protected requests
      └─ refreshToken (opaque, 30 days)
                │
                │  when accessToken expires:
                ▼
        POST /api/auth/refresh
                │
         old token revoked
                │
                ├─ new accessToken
                └─ new refreshToken  ◄── store this; old one is now invalid
```

Refresh tokens are **single-use**. Submitting an already-revoked token returns `401`.

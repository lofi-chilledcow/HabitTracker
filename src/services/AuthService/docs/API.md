# AuthService API

AuthService owns registration, login, refresh tokens, current profile, and admin user management.

- Base URL: `http://localhost:5039`
- Gateway paths are the same under `http://localhost:5000`
- Protected endpoints require `Authorization: Bearer <accessToken>`

## Endpoints

| Method | Path | Auth | Purpose |
| --- | --- | --- | --- |
| POST | `/api/auth/register` | none | Create a user with role `User` |
| POST | `/api/auth/login` | none | Login with email, username, or phone |
| POST | `/api/auth/refresh` | none | Rotate refresh token and issue new tokens |
| POST | `/api/auth/logout` | none | Revoke a submitted refresh token |
| GET | `/api/auth/me` | user | Return current user profile |
| GET | `/api/admin/users` | admin | List users |
| PATCH | `/api/admin/users/{id}/status` | admin | Enable or disable a user |
| PATCH | `/api/admin/users/{id}/role` | admin | Change a user's role |

## Register

```json
{
  "username": "johndoe",
  "email": "john@example.com",
  "password": "Secret123",
  "phoneNumber": "(555) 123-4567"
}
```

Rules:

- `username` is required, at least 3 characters, and cannot contain spaces.
- `email` is required and normalized to lowercase.
- `phoneNumber` is optional and stored as digits only.
- `password` is required, at least 8 characters, with one uppercase letter and one number.
- Duplicate email, username, or phone returns `409`.

## Login

```json
{
  "identifier": "john@example.com",
  "password": "Secret123"
}
```

`identifier` may be email, username, or phone number.

## Auth Response

```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "raw-refresh-token",
  "user": {
    "id": "7e4c5f0a-1a90-4d6e-bff3-7d274b91a400",
    "username": "johndoe",
    "email": "john@example.com",
    "phoneNumber": "5551234567",
    "role": "User"
  }
}
```

Refresh tokens are returned raw once, stored hashed in the database, and rotated on refresh.

## Current User

```text
GET /api/auth/me
```

Returns `UserProfileDto`.

## Logout

```json
{
  "refreshToken": "raw-refresh-token"
}
```

Returns `204` whether or not the token was already revoked.

## Admin Users

Admin routes require the `AdminOnly` policy.

```text
GET /api/admin/users
```

Returns:

```json
[
  {
    "id": "7e4c5f0a-1a90-4d6e-bff3-7d274b91a400",
    "username": "johndoe",
    "email": "john@example.com",
    "phoneNumber": "5551234567",
    "role": "User",
    "isActive": true,
    "createdAt": "2026-05-10T19:00:00Z",
    "updatedAt": "2026-05-10T19:00:00Z"
  }
]
```

Set status:

```text
PATCH /api/admin/users/{id}/status
```

```json
{
  "isActive": false
}
```

Set role:

```text
PATCH /api/admin/users/{id}/role
```

```json
{
  "role": "Admin"
}
```

Valid roles are `User` and `Admin`.

## JWT Claims

| Claim | Meaning |
| --- | --- |
| `sub` | User id |
| `email` | Email |
| `name` | Username |
| `role` | `User` or `Admin` |
| `jti` | Token id |

## Status Codes

| Code | Meaning |
| --- | --- |
| `200` | Request succeeded |
| `204` | Logout succeeded |
| `400` | Validation failed |
| `401` | Invalid credentials or token |
| `403` | Authenticated but not allowed |
| `404` | Admin target user not found |
| `409` | Duplicate registration field |

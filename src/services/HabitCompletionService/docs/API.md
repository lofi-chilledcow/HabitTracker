# HabitCompletionService API

Tracks habit completions for HabitTracker users.

- **Base URL (dev):** `http://localhost:5225`
- **Content-Type:** `application/json`
- **Authentication:** Bearer JWT required on all endpoints (`Authorization: Bearer <accessToken>`)
- **User identity:** `userId` is always read from the JWT `sub` claim — it is never accepted as a request parameter

---

## Endpoints

| Method | Path | Description |
|--------|------|-------------|
| POST | `/api/habit-completions` | Mark a habit as complete |
| GET | `/api/habit-completions/habit/{habitId}` | Get all completions for a habit |
| GET | `/api/habit-completions/today` | Get the caller's completions for today |
| DELETE | `/api/habit-completions/{id}` | Delete a completion |

---

## POST `/api/habit-completions`

Records a completion for the authenticated user. `userId` is extracted from the JWT `sub` claim and is never supplied by the caller.

### Request body

```json
{
  "habitId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "completedDate": "2026-03-23T00:00:00Z",
  "notes": "Felt great today"
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `habitId` | uuid | Yes | ID of the habit being marked complete |
| `completedDate` | datetime (UTC) | No | Date of completion. Time component is truncated to date. Defaults to today (UTC) if omitted |
| `notes` | string \| null | No | Optional notes. Max 1000 characters |

### Responses

#### `201 Created`

Includes a `Location` header pointing to the habit's completions list.

```
Location: /api/habit-completions/habit/3fa85f64-5717-4562-b3fc-2c963f66afa6
```

```json
{
  "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "habitId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "userId": "d4e5f6a7-b8c9-0123-def0-456789abcdef",
  "completedDate": "2026-03-23T00:00:00Z",
  "notes": "Felt great today",
  "createdAt": "2026-03-23T08:30:00Z"
}
```

#### `401 Unauthorized`

Missing or invalid JWT, or `sub` claim is absent/not a valid GUID. Empty body.

---

## GET `/api/habit-completions/habit/{habitId}`

Returns all completions for a given habit across all users, ordered newest first.

### Path parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `habitId` | uuid | ID of the habit |

### Responses

#### `200 OK`

Returns an empty array `[]` if no completions exist.

```json
[
  {
    "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "habitId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "userId": "d4e5f6a7-b8c9-0123-def0-456789abcdef",
    "completedDate": "2026-03-23T00:00:00Z",
    "notes": "Felt great today",
    "createdAt": "2026-03-23T08:30:00Z"
  }
]
```

#### `401 Unauthorized`

Missing or invalid JWT. Empty body.

---

## GET `/api/habit-completions/today`

Returns all completions recorded today (UTC date) for the authenticated user.

### Responses

#### `200 OK`

Returns an empty array `[]` if the user has no completions today.

```json
[
  {
    "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "habitId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "userId": "d4e5f6a7-b8c9-0123-def0-456789abcdef",
    "completedDate": "2026-03-23T00:00:00Z",
    "notes": null,
    "createdAt": "2026-03-23T07:15:00Z"
  }
]
```

#### `401 Unauthorized`

Missing or invalid JWT, or `sub` claim is absent/not a valid GUID. Empty body.

---

## DELETE `/api/habit-completions/{id}`

Permanently deletes a completion. Users can only delete their own records — the handler matches on both `id` and the `userId` from the JWT.

### Path parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `id` | uuid | ID of the completion to delete |

### Responses

#### `204 No Content`

Completion deleted. Empty body.

#### `401 Unauthorized`

Missing or invalid JWT, or `sub` claim is absent/not a valid GUID. Empty body.

#### `404 Not Found`

No completion with that ID exists, or it belongs to a different user. Empty body.

---

## Schemas

### `HabitCompletionDto`

Returned by POST and both GET endpoints.

| Field | Type | Description |
|-------|------|-------------|
| `id` | uuid | Unique identifier of the completion record |
| `habitId` | uuid | ID of the completed habit |
| `userId` | uuid | ID of the user who completed the habit |
| `completedDate` | datetime (UTC) | Date the habit was completed (time truncated to midnight UTC) |
| `notes` | string \| null | Optional notes. Max 1000 characters |
| `createdAt` | datetime (UTC) | When this record was inserted |

### `CreateHabitCompletionDto`

| Field | Type | Required |
|-------|------|----------|
| `habitId` | uuid | Yes |
| `completedDate` | datetime \| null | No — defaults to today (UTC) |
| `notes` | string \| null | No |

---

## Status Code Reference

| Code | When |
|------|------|
| `201 Created` | Completion successfully recorded |
| `200 OK` | Successful read |
| `204 No Content` | Completion successfully deleted |
| `401 Unauthorized` | Invalid/missing JWT, or `sub` claim missing or malformed |
| `404 Not Found` | Completion not found, or belongs to a different user |

---

## Error Format

All error responses return an **empty body**. No JSON error payload is returned.

---

## Authentication Notes

Obtain a token from AuthService:

```
POST http://localhost:5039/api/auth/login
```

Include it on every request:

```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

The JWT must have been issued by `AuthService` with audience `HabitTrackerApp`. Tokens expire after **60 minutes** — use `/api/auth/refresh` to rotate.

# HabitService API

Habit management service for HabitTracker. Provides full CRUD for habits.

- **Base URL (dev):** `http://localhost:5110`
- **Content-Type:** `application/json`
- **Authentication:** None (not yet enforced)

---

## Endpoints

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/habits` | List all habits |
| GET | `/api/habits/{id}` | Get a single habit |
| POST | `/api/habits` | Create a habit |
| PUT | `/api/habits/{id}` | Update a habit |
| DELETE | `/api/habits/{id}` | Delete a habit |

---

## GET `/api/habits`

Returns all habits.

### Responses

#### `200 OK`

```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "Morning Run",
    "description": "Run 5km before breakfast",
    "frequency": "Daily",
    "createdAt": "2026-03-22T10:00:00Z",
    "isActive": true
  },
  {
    "id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
    "name": "Monthly Budget Check",
    "description": "Review spending and set savings targets",
    "frequency": "Monthly",
    "createdAt": "2026-03-22T10:00:00Z",
    "isActive": false
  }
]
```

Returns an empty array `[]` when no habits exist.

---

## GET `/api/habits/{id}`

Returns a single habit by its GUID.

### Path parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `id` | uuid | Habit ID |

### Responses

#### `200 OK`

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Morning Run",
  "description": "Run 5km before breakfast",
  "frequency": "Daily",
  "createdAt": "2026-03-22T10:00:00Z",
  "isActive": true
}
```

#### `404 Not Found`

Empty body.

---

## POST `/api/habits`

Creates a new habit. New habits default to `isActive: true`.

### Request body

```json
{
  "name": "Morning Run",
  "description": "Run 5km before breakfast",
  "frequency": "Daily"
}
```

| Field | Type | Required | Rules |
|-------|------|----------|-------|
| `name` | string | Yes | Max 200 characters |
| `description` | string \| null | No | Max 1000 characters |
| `frequency` | string | Yes | Max 50 characters. Suggested values: `Daily`, `Weekly`, `Monthly` |

### Responses

#### `201 Created`

Includes a `Location` header pointing to the new resource.

```
Location: /api/habits/3fa85f64-5717-4562-b3fc-2c963f66afa6
```

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Morning Run",
  "description": "Run 5km before breakfast",
  "frequency": "Daily",
  "createdAt": "2026-03-22T10:00:00Z",
  "isActive": true
}
```

---

## PUT `/api/habits/{id}`

Replaces all mutable fields on an existing habit. All fields are required — partial updates are not supported.

### Path parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `id` | uuid | Habit ID |

### Request body

```json
{
  "name": "Morning Run",
  "description": "Run 10km before breakfast",
  "frequency": "Daily",
  "isActive": true
}
```

| Field | Type | Required | Rules |
|-------|------|----------|-------|
| `name` | string | Yes | Max 200 characters |
| `description` | string \| null | No | Max 1000 characters |
| `frequency` | string | Yes | Max 50 characters |
| `isActive` | boolean | Yes | Use `false` to soft-disable a habit |

### Responses

#### `200 OK`

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Morning Run",
  "description": "Run 10km before breakfast",
  "frequency": "Daily",
  "createdAt": "2026-03-22T10:00:00Z",
  "isActive": true
}
```

#### `404 Not Found`

Empty body.

---

## DELETE `/api/habits/{id}`

Permanently deletes a habit.

### Path parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `id` | uuid | Habit ID |

### Responses

#### `204 No Content`

Empty body. Habit deleted.

#### `404 Not Found`

Empty body.

---

## Schemas

### `HabitDto`

Returned by all endpoints on success.

| Field | Type | Description |
|-------|------|-------------|
| `id` | uuid | Unique identifier (GUID, sequential) |
| `name` | string | Habit name. Max 200 characters |
| `description` | string \| null | Optional description. Max 1000 characters |
| `frequency` | string | How often the habit recurs (e.g. `Daily`, `Weekly`, `Monthly`) |
| `createdAt` | datetime | UTC timestamp of creation (ISO 8601) |
| `isActive` | boolean | Whether the habit is currently active |

### `CreateHabitDto`

| Field | Type | Required |
|-------|------|----------|
| `name` | string | Yes |
| `description` | string \| null | No |
| `frequency` | string | Yes |

### `UpdateHabitDto`

| Field | Type | Required |
|-------|------|----------|
| `name` | string | Yes |
| `description` | string \| null | No |
| `frequency` | string | Yes |
| `isActive` | boolean | Yes |

---

## Status Code Reference

| Code | When |
|------|------|
| `200 OK` | Successful read or update |
| `201 Created` | Habit successfully created |
| `204 No Content` | Habit successfully deleted |
| `404 Not Found` | No habit with the given ID exists |

---

## Error Format

`404` responses return an empty body with no JSON payload.

No input validation is currently enforced — invalid or missing fields will result in a `400` from the framework's model binding.

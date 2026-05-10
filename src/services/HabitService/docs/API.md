# HabitService API

HabitService owns user habits, habit completions, and public competition data.

- Base URL: `http://localhost:5110`
- Gateway paths are the same under `http://localhost:5000`
- Protected endpoints require `Authorization: Bearer <accessToken>`

## Endpoints

| Method | Path | Auth | Purpose |
| --- | --- | --- | --- |
| GET | `/api/habits` | user | List current user's active habits |
| GET | `/api/habits/{id}` | user | Get one owned habit |
| POST | `/api/habits` | user | Create a habit for the current user |
| PUT | `/api/habits/{id}` | user | Update one owned habit |
| DELETE | `/api/habits/{id}` | user | Archive one owned habit |
| GET | `/api/completions/today` | user | List current user's completions for today |
| GET | `/api/habits/{habitId}/completions` | user | List completion history for one owned habit |
| PUT | `/api/habits/{habitId}/completions/{date}` | user | Mark an owned active habit complete |
| DELETE | `/api/habits/{habitId}/completions/{date}` | user | Unmark an owned active habit |
| GET | `/api/competition/leaderboard` | none | List public active habit leaderboard |

## Habit Request

```json
{
  "name": "Read",
  "description": "Read before bed",
  "frequency": "daily",
  "targetDaysPerWeek": null,
  "isPublic": true
}
```

Rules:

- `name` is required and max 200 characters.
- `frequency` must be `daily` or `weekly`.
- Weekly habits require `targetDaysPerWeek` from 1 to 7.
- Daily habits must not include `targetDaysPerWeek`.
- `userId` is never accepted from the frontend; it comes from the JWT.

## Habit Response

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Read",
  "description": "Read before bed",
  "frequency": "daily",
  "targetDaysPerWeek": null,
  "isPublic": true,
  "createdAt": "2026-05-10T19:00:00Z",
  "updatedAt": "2026-05-10T19:00:00Z",
  "isActive": true
}
```

## Completion Request

`date` route values use `yyyy-MM-dd`.

```json
{
  "notes": "Done"
}
```

`PUT` is idempotent. If the completion already exists for the same habit and date, notes are updated and no duplicate row is inserted.

`DELETE` is idempotent for an owned active habit. It returns `204` even if the completion was already missing.

## Completion Response

```json
{
  "id": "4b3c8f1d-8c9a-4e7a-8507-c4d2d5af02e9",
  "habitId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "completedDate": "2026-05-10",
  "notes": "Done",
  "createdAt": "2026-05-10T19:00:00Z"
}
```

## Leaderboard

```text
GET /api/competition/leaderboard?days=30&limit=50
```

Only public active habits are returned. Private and archived habits are excluded.

```json
[
  {
    "habitId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "Read",
    "description": "Read before bed",
    "frequency": "daily",
    "targetDaysPerWeek": null,
    "completionCount": 12,
    "createdAt": "2026-05-10T19:00:00Z"
  }
]
```

## Status Codes

| Code | Meaning |
| --- | --- |
| `200` | Request succeeded |
| `201` | Habit created |
| `204` | Delete/archive/unmark succeeded |
| `400` | Validation failed |
| `401` | Missing or invalid access token |
| `404` | Owned resource was not found |

# HabitTracker — Claude Code Guidelines

## API Design Standards

### Response envelope

All API responses use a consistent `{ success, data, errors }` envelope:

```json
{
  "success": true,
  "data": { },
  "errors": null
}
```

```json
{
  "success": false,
  "data": null,
  "errors": ["Email is already registered."]
}
```

| Field | Type | Description |
|-------|------|-------------|
| `success` | bool | `true` when the request succeeded, `false` otherwise |
| `data` | object \| null | The response payload on success, `null` on failure |
| `errors` | string[] \| null | List of error messages on failure, `null` on success |

### Status codes

| Code | When to use |
|------|-------------|
| `200 OK` | Successful read or update |
| `201 Created` | Resource successfully created |
| `204 No Content` | Successful delete (no body) |
| `400 Bad Request` | Validation failure |
| `401 Unauthorized` | Missing, invalid, or expired token |
| `409 Conflict` | Duplicate resource (e.g. email already registered) |

No other status codes should be introduced without updating this document.

## Git Commit Standards

Commit after each completed feature using the conventional commit format:

```
<type>(<scope>): <short description>
```

### Types

| Type | When to use |
|------|-------------|
| `feat` | New feature or endpoint |
| `fix` | Bug fix |
| `chore` | Config, tooling, dependencies |
| `docs` | Documentation only |
| `refactor` | Code change with no behaviour change |
| `test` | Adding or updating tests |

### Scopes

Use the service or area being changed: `auth`, `habits`, `completions`, `infra`, `shared`.

### Examples

```
feat(auth): add JWT login endpoint
feat(completions): add today's completions query
fix(habits): fix delete returning 404 on valid ID
chore(auth): add DotNetEnv package
docs(completions): add API.md and OpenAPI spec
refactor(auth): extract JWT generation into JwtTokenService
```

### Rules

- One commit per completed feature or fix — do not batch unrelated changes
- Description is lowercase, imperative mood, no trailing period
- Keep the description under 72 characters

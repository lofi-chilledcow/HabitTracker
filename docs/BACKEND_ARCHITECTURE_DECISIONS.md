# Backend Architecture Decisions

## Purpose

This document records the backend architecture decisions for the HabitTracker rewrite. Use it while coding so the rewrite stays focused and does not drift into unnecessary complexity.

Related docs:

- `docs/REWRITE_PLAN.md`
- `docs/DATABASE_DESIGN.md`
- `docs/BACKEND_REWRITE_PLAN.md`

## Decision Summary

| Topic | Decision |
| --- | --- |
| API style | REST-first |
| Backend organization | Light CQRS with MediatR-style handlers |
| Authorization | Server-enforced role policies plus ownership checks |
| Events | No event-driven architecture in the first rewrite |
| Testing | Test-driven for security, ownership, and business rules |
| Persistence | EF Core migrations are the source of truth |
| Services | AuthService + HabitService behind ApiGateway |
| Completions | Merge into HabitService for the first rewrite |

## API Style: REST-First

The rewrite should use REST endpoints for externally visible API contracts.

Good resource shapes:

```text
POST   /api/auth/register
POST   /api/auth/login
POST   /api/auth/refresh
POST   /api/auth/logout
GET    /api/auth/me

GET    /api/habits
POST   /api/habits
GET    /api/habits/{id}
PUT    /api/habits/{id}
DELETE /api/habits/{id}

GET    /api/completions/today
PUT    /api/habits/{habitId}/completions/{date}
DELETE /api/habits/{habitId}/completions/{date}

GET    /api/competition/leaderboard
GET    /api/admin/users
```

This is REST-first, but not "dumb CRUD." Every endpoint must enforce product rules.

Examples:

- `POST /api/habits` creates a habit for the current JWT user, never for a client-supplied `UserId`.
- `GET /api/habits` returns only the current user's active habits.
- `PUT /api/habits/{habitId}/completions/{date}` first verifies habit ownership.
- `DELETE /api/habits/{id}` archives a habit instead of hard deleting it.

## CQRS Scope: Light CQRS

Use CQRS as a code organization pattern, not as a distributed architecture.

Recommended flow:

```text
Controller
  -> Command or Query
  -> Handler
  -> EF Core DbContext
  -> DTO
```

Use commands for operations that change state:

- `RegisterCommand`
- `LoginCommand`
- `RefreshTokenCommand`
- `LogoutCommand`
- `CreateHabitCommand`
- `UpdateHabitCommand`
- `ArchiveHabitCommand`
- `MarkHabitCompleteCommand`
- `UnmarkHabitCompleteCommand`

Use queries for read operations:

- `GetCurrentUserQuery`
- `GetMyHabitsQuery`
- `GetHabitByIdQuery`
- `GetTodaysCompletionsQuery`
- `GetHabitCompletionHistoryQuery`
- `GetLeaderboardQuery`
- `GetAdminUsersQuery`

Do not add full CQRS infrastructure in the first rewrite:

- No separate read database.
- No event sourcing.
- No projection workers.
- No message bus.
- No eventual consistency for core habit tracking.

The app needs strong ownership and correctness more than distributed read/write separation.

## Role-Based Access Control

Use simple role-based access control.

Roles:

| Role | Capabilities |
| --- | --- |
| `User` | Manage own habits and completions |
| `Admin` | Access admin APIs, user management, monitoring, and admin reports |

One role per user is enough for the first rewrite.

Do not add `Permissions`, `RolePermissions`, or many-to-many `UserRoles` unless the product grows into roles like `Coach`, `Moderator`, or `GroupOwner`.

## Authorization Model

Authorization has two layers:

1. Role policy checks.
2. Resource ownership checks.

Role policies:

| Policy | Rule |
| --- | --- |
| `UserOnly` | Authenticated user |
| `AdminOnly` | Authenticated user with `Admin` role |

Ownership checks:

- Habits are loaded by `Id` and current `UserId`.
- Completions are changed only after the habit is verified to belong to the current user.
- Normal user APIs never accept `UserId` from the request body.
- Admin override behavior must use separate admin endpoints.

For ownership failures, return `404` instead of `403` so the API does not reveal that another user's resource exists.

## User Flow

Normal user flow:

```text
Register or login
  -> AuthService issues JWT with user id and role
  -> User creates habit
  -> HabitService stores UserId from JWT
  -> User lists habits
  -> HabitService filters by UserId
  -> User marks habit complete
  -> HabitService verifies habit belongs to UserId
  -> HabitService inserts or returns completion for the date
```

Admin flow:

```text
Admin logs in
  -> JWT includes Admin role
  -> Shell may show admin navigation
  -> Backend enforces AdminOnly
  -> Admin can list users, manage status/roles, and view monitoring
```

Competition flow:

```text
User sets habit IsPublic = true
  -> Competition endpoint reads only public active habits
  -> Private habits are never returned
  -> Archived habits are never returned
```

## Event-Driven Architecture Decision

Do not use event-driven architecture in the first rewrite.

The first rewrite should be synchronous:

```text
Frontend
  -> ApiGateway
  -> AuthService or HabitService
  -> SQL Server
```

Event-driven design may be useful later for:

- notification emails
- weekly summaries
- badges or achievements
- audit logs
- async leaderboard recalculation
- external integrations

Do not introduce queues, event buses, or background projection workers until the core REST flows are stable and tested.

## Testing Decision

Use practical test-driven development for backend rules.

This does not mean every line must be written test-first. It means the important behavior gets tests before the rewrite is considered complete.

Highest-priority tests:

- User cannot list another user's habits.
- User cannot update another user's habit.
- User cannot complete another user's habit.
- Archived habits are hidden from normal lists.
- Archived habits cannot be completed.
- Same habit/date cannot create duplicate completions.
- Mark-complete is idempotent.
- Admin endpoints reject normal users.
- Admin endpoints accept admin users.
- Refresh token rotation revokes old tokens.

Testing style:

- Handler tests for business rules.
- Controller or integration tests for authorization policies.
- EF relational tests for unique constraints when possible.
- Avoid only using EF InMemory for database constraint behavior because it does not enforce relational constraints the same way SQL Server does.

## Response Contract Decision

Use:

- Raw DTOs for successful responses.
- `ProblemDetails` or `ValidationProblemDetails` for errors.

Do not use mixed response envelopes.

Good success:

```json
{
  "id": "f65f2b15-c77e-4ef6-9be5-473df375c187",
  "name": "Read",
  "frequency": "daily"
}
```

Good validation error shape:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "name": ["Name is required."]
  }
}
```

## Service Boundary Decision

Keep the first rewrite simple:

```text
ApiGateway
AuthService
HabitService
```

Merge completions into HabitService because completions depend directly on habit ownership. This avoids needing cross-service calls for the most common user action: checking off a habit.

Reconsider a separate HabitCompletionService only if:

- completion traffic becomes independently high,
- completion logic needs separate deployment,
- or the project goal is specifically to practice distributed service boundaries.

## Database Source Of Truth

EF Core migrations should be the implementation source of truth after the rewrite starts.

The existing hand-written SQL script can remain as historical reference, but it should not define the active schema if it disagrees with EF.

Rules:

- Map schemas explicitly in EF.
- Generate migrations from models.
- Review migrations before applying.
- Do not maintain parallel schema definitions that drift.

## What To Avoid In The First Rewrite

Avoid:

- event sourcing
- separate read models/databases
- message brokers
- generic repository layers over EF unless they add real value
- many-to-many user roles
- permission tables
- hard delete for habits
- accepting `UserId` from frontend habit APIs
- mixed API response envelopes
- frontend-only role checks

These can be added later if the product needs them. They are not foundation requirements now.

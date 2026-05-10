# HabitTracker Backend Rewrite Plan

## Purpose

This document turns the database design into a backend implementation plan. Use it as the reference when rewriting the .NET services.

Read these first:

- `docs/REWRITE_PLAN.md`
- `docs/DATABASE_DESIGN.md`
- `docs/BACKEND_ARCHITECTURE_DECISIONS.md`
- `docs/IMPLEMENTATION_CHECKLIST.md`

## Backend Verdict

Keep the broad shape, but rewrite the foundation:

- Keep ASP.NET Core Web API.
- Keep JWT auth with refresh tokens.
- Keep YARP API Gateway.
- Keep EF Core and SQL Server.
- Keep CQRS-style handlers where they help.
- Rewrite the database mappings, ownership checks, response contracts, and authorization policies.

Recommended simplification:

- Keep `AuthService`.
- Keep `HabitService`.
- Merge `HabitCompletionService` into `HabitService` for the rewrite.

The separate completion service is not wrong in theory, but right now it adds complexity where the app most needs strong consistency: habit ownership, duplicate completion prevention, and daily check-in queries.

## Target Services

| Service | Port | Responsibility |
| --- | --- | --- |
| ApiGateway | 5000 | Single frontend API entry point |
| AuthService | 5039 | Register, login, refresh, logout, current user, roles |
| HabitService | 5110 | Habits, completions, summaries, competition data |

Optional later:

| Service | When to add |
| --- | --- |
| MonitorService | Add only when admin monitoring needs real query endpoints |
| HabitCompletionService | Add only if completions must scale/deploy independently |

## Shared Backend Rules

### API Response Style

Use raw DTOs for successful responses.

Examples:

```json
{
  "id": "7e4c5f0a-1a90-4d6e-bff3-7d274b91a400",
  "name": "Read",
  "frequency": "daily"
}
```

```json
[
  {
    "id": "7e4c5f0a-1a90-4d6e-bff3-7d274b91a400",
    "name": "Read",
    "frequency": "daily"
  }
]
```

Use ASP.NET Core `ProblemDetails` for errors.

Do not mix raw DTOs with `{ success, data, errors }` envelopes.

### Authentication

JWT access tokens should include:

- `sub`: user id
- `email`: email
- `name`: username
- role claim: `User` or `Admin`
- `jti`: token id

Every service that accepts authenticated requests validates:

- signing key
- issuer
- audience
- lifetime

### Authorization

Define policies:

| Policy | Rule |
| --- | --- |
| `UserOnly` | Authenticated users |
| `AdminOnly` | Authenticated users with role `Admin` |

Server-side authorization is mandatory. Frontend route hiding is only UX.

### Current User Access

Add a small service/helper in authenticated services:

```csharp
public interface ICurrentUser
{
    Guid UserId { get; }
    string? Email { get; }
    string? Username { get; }
    bool IsAdmin { get; }
}
```

Handlers should receive `UserId` from this helper or from controller-created commands. They should never trust a frontend-supplied `UserId`.

### Validation

Use FluentValidation for request DTOs:

- Email format and length.
- Optional phone number must contain at least 10 digits.
- Password minimum length.
- Habit name required and max length.
- Frequency must be `daily` or `weekly`.
- `TargetDaysPerWeek` must be 1-7 for weekly habits.
- Completion date must be a date, not arbitrary local time.

### Time

Use UTC for timestamps.

Use `DateOnly` in C# for completion dates if practical. If staying with `DateTime`, always normalize with `.Date` before persisting.

## Project Structure

Recommended backend structure:

```text
src/services/
  ApiGateway/
  AuthService/
  HabitService/
tests/services/
  AuthService.Tests/
  HabitService.Tests/
```

Within each service:

```text
Controllers/
Data/
Models/
DTOs/
Commands/
Queries/
Services/
Validation/
Infrastructure/
```

`Infrastructure/` can hold cross-cutting helpers like `CurrentUser`, exception handling middleware, and ProblemDetails configuration.

## ApiGateway Plan

### Keep

- YARP reverse proxy.
- CORS config for local MFE ports.
- Serilog request logging.

### Change

- Route only to services that exist after rewrite.
- Remove HabitCompletionService route if completions are merged into HabitService.
- Ensure the frontend only calls the gateway base URL.

### Target Routes

| Gateway path | Destination |
| --- | --- |
| `/api/auth/{**catch-all}` | AuthService |
| `/api/habits/{**catch-all}` | HabitService |
| `/api/completions/{**catch-all}` | HabitService |
| `/api/competition/{**catch-all}` | HabitService |
| `/api/admin/{**catch-all}` | AuthService or HabitService depending on endpoint |

Prefer habit-related admin endpoints in HabitService and user-related admin endpoints in AuthService.

### Gateway Done Criteria

- All frontend API calls go through `http://localhost:5000`.
- No frontend code calls downstream service ports directly.
- Gateway route config matches OpenAPI docs.

## AuthService Rewrite Plan

### Owns

Schema: `auth`

Tables:

- `auth.Roles`
- `auth.Users`
- `auth.RefreshTokens`

### Models

`User`:

- `Id`
- `Username`
- `Email`
- `PhoneNumber`
- `PasswordHash`
- `RoleId`
- `Role`
- `IsActive`
- `CreatedAt`
- `UpdatedAt`

`Role`:

- `Id`
- `Name`
- `Description`
- `CreatedAt`

`RefreshToken`:

- `Id`
- `UserId`
- `TokenHash`
- `ExpiresAt`
- `RevokedAt`
- `CreatedAt`

### EF Mapping

Use explicit schema mapping:

```csharp
modelBuilder.HasDefaultSchema("auth");
```

Add:

- unique email index
- unique username index
- unique filtered phone number index
- unique role name index
- unique refresh token hash index

### Endpoints

| Method | Path | Auth | Purpose |
| --- | --- | --- | --- |
| POST | `/api/auth/register` | none | Create user account |
| POST | `/api/auth/login` | none | Issue access and refresh tokens by email, username, or phone |
| POST | `/api/auth/refresh` | none | Rotate refresh token and issue new access token |
| POST | `/api/auth/logout` | user | Revoke current refresh token |
| GET | `/api/auth/me` | user | Return current user profile |
| GET | `/api/admin/users` | admin | List users |
| PATCH | `/api/admin/users/{id}/status` | admin | Enable/disable user |
| PATCH | `/api/admin/users/{id}/role` | admin | Change role |

### DTOs

`AuthResponse`:

```csharp
public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    UserProfileDto User);
```

`UserProfileDto`:

```csharp
public record UserProfileDto(
    Guid Id,
    string Username,
    string Email,
    string? PhoneNumber,
    string Role);
```

### Important Rules

- Store refresh token hashes only.
- Normalize email before lookup and storage.
- Normalize phone numbers to digits only before lookup and storage.
- Registration creates users with role `User`.
- Login uses one `identifier` field: email, username, or phone number.
- Admin users should be created by seed/config/script, not public registration.
- Login rejects inactive users.
- Refresh rejects inactive users.
- Logout revokes the submitted refresh token.

### AuthService Test Gate

Required tests:

1. Register creates user with `User` role.
2. Register rejects duplicate email.
3. Register rejects duplicate username.
4. Register rejects duplicate phone number.
5. Login returns token pair for valid email credentials.
6. Login returns token pair for valid username credentials.
7. Login returns token pair for valid phone credentials.
8. Login rejects wrong password.
9. Login rejects inactive user.
10. Refresh rotates token and revokes old token.
11. Refresh rejects reused revoked token.
12. Logout revokes token.
13. `/me` returns current user profile.
14. Admin endpoints reject normal users.
15. Admin endpoints accept admin users.

## HabitService Rewrite Plan

### Owns

Schema: `habit`

Tables:

- `habit.Habits`
- `habit.HabitCompletions`

Potential read access:

- `auth.Users` for competition display names if using one DB and cross-schema reads.

If avoiding cross-schema reads, competition display names can come from denormalized username snapshots or an AuthService call later. For the first rewrite, cross-schema read is acceptable in the single SQL Server deployment.

### Models

`Habit`:

- `Id`
- `UserId`
- `Name`
- `Description`
- `Frequency`
- `TargetDaysPerWeek`
- `IsPublic`
- `IsActive`
- `CreatedAt`
- `UpdatedAt`
- `Completions`

`HabitCompletion`:

- `Id`
- `HabitId`
- `Habit`
- `UserId`
- `CompletedDate`
- `Notes`
- `CreatedAt`

### EF Mapping

Use explicit schema mapping:

```csharp
entity.ToTable("Habits", "habit");
entity.ToTable("HabitCompletions", "habit");
```

Required indexes:

- `Habits`: `(UserId, IsActive)`
- `Habits`: `(IsPublic, IsActive)`
- `HabitCompletions`: unique `(HabitId, CompletedDate)`
- `HabitCompletions`: `(UserId, CompletedDate)`
- `HabitCompletions`: `HabitId`

Required constraints:

- `Frequency in ('daily', 'weekly')`
- `TargetDaysPerWeek between 1 and 7` when present

### Habit Endpoints

| Method | Path | Auth | Purpose |
| --- | --- | --- | --- |
| GET | `/api/habits` | user | List current user's active habits |
| GET | `/api/habits/{id}` | user | Get current user's habit |
| POST | `/api/habits` | user | Create habit for current user |
| PUT | `/api/habits/{id}` | user | Update current user's habit |
| DELETE | `/api/habits/{id}` | user | Archive current user's habit |
| PATCH | `/api/habits/{id}/visibility` | user | Toggle public/private |

### Completion Endpoints

Recommended shape:

| Method | Path | Auth | Purpose |
| --- | --- | --- | --- |
| GET | `/api/completions/today` | user | Today's completions for current user |
| GET | `/api/habits/{habitId}/completions` | user | Completion history for one owned habit |
| PUT | `/api/habits/{habitId}/completions/{date}` | user | Mark habit completed for date |
| DELETE | `/api/habits/{habitId}/completions/{date}` | user | Unmark habit completed for date |

Use `PUT` for mark-complete because the operation is naturally idempotent.

### Summary and Competition Endpoints

| Method | Path | Auth | Purpose |
| --- | --- | --- | --- |
| GET | `/api/habits/summary` | user | Current user's habit/completion summary |
| GET | `/api/habits/{id}/streak` | user | Streak for owned habit |
| GET | `/api/competition/leaderboard` | optional/user | Public leaderboard |
| GET | `/api/competition/habits` | optional/user | Public habits |

Product decision:

- If competition is public marketing/community content, allow anonymous reads.
- If it is app-internal, require auth.

Either way, competition endpoints must only return public active habits.

### DTOs

`HabitDto`:

```csharp
public record HabitDto(
    Guid Id,
    string Name,
    string? Description,
    string Frequency,
    byte? TargetDaysPerWeek,
    bool IsPublic,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);
```

`CreateHabitRequest`:

```csharp
public record CreateHabitRequest(
    string Name,
    string? Description,
    string Frequency,
    byte? TargetDaysPerWeek,
    bool IsPublic);
```

`UpdateHabitRequest`:

```csharp
public record UpdateHabitRequest(
    string Name,
    string? Description,
    string Frequency,
    byte? TargetDaysPerWeek,
    bool IsPublic);
```

`HabitCompletionDto`:

```csharp
public record HabitCompletionDto(
    Guid Id,
    Guid HabitId,
    DateOnly CompletedDate,
    string? Notes,
    DateTime CreatedAt);
```

### Important Rules

- Create habit uses current user id from JWT.
- List habits filters by current user id.
- Get/update/archive habit loads by both habit id and current user id.
- Completion operations first verify habit ownership.
- Completing inactive habits is rejected.
- Mark-complete is idempotent.
- Archive does not delete completions.
- Hard delete should not exist in normal user API.

### HabitService Test Gate

Required habit tests:

1. Create habit stores current user id.
2. List habits returns only current user's active habits.
3. Get habit rejects another user's habit.
4. Update habit rejects another user's habit.
5. Archive habit sets `IsActive = false`.
6. Archived habits are hidden from normal list.
7. Visibility toggle only affects current user's habit.
8. Invalid frequency is rejected.
9. Weekly habit validates target days.

Required completion tests:

1. Mark completion succeeds for owned active habit.
2. Mark completion rejects another user's habit.
3. Mark completion rejects inactive habit.
4. Mark completion is idempotent for same habit/date.
5. Unique constraint prevents duplicate habit/date rows.
6. Unmark completion deletes only current user's completion.
7. Today's completions returns only current user's rows.
8. Habit completion history rejects another user's habit.

Required competition tests:

1. Leaderboard includes public active habits only.
2. Leaderboard excludes private habits.
3. Leaderboard excludes archived habits.
4. Competition counts are date-window correct.

## Monitoring Plan

Do not let monitoring block the core rewrite.

Phase 1:

- Keep Serilog file/console logging.
- Use ASP.NET Core health checks.
- Expose `/health` per service.

Phase 2:

- Add `monitor.RequestLogs` only if the admin dashboard needs database-backed request stats.
- Add `monitor.HealthChecks` snapshots only if historical health views are needed.

Admin monitoring endpoints should be `AdminOnly`.

## Exception Handling

Add consistent exception handling per service:

- Validation errors: `400 Bad Request` with `ValidationProblemDetails`.
- Unauthorized: `401`.
- Forbidden: `403`.
- Not found or not owned: `404`.
- Duplicate/conflict: `409` if not using idempotent behavior.
- Unexpected errors: `500` with generic message and logged exception.

For ownership failures, prefer `404` instead of `403` to avoid revealing another user's resource exists.

## Migration Strategy

For a clean local rewrite:

1. Stop treating `src/database/scripts/001_CreateDatabase.sql` as active source of truth.
2. Update EF models and DbContexts.
3. Remove old generated migrations that no longer represent the target schema.
4. Generate new initial migrations.
5. Apply migrations to a clean database.
6. Seed roles.
7. Seed optional local dev users only in development.

The current local app has no real users/data, so no data-preservation migration is required. If this app ever has real production data later, create explicit migration scripts and data transforms instead of using a destructive reset.

## Suggested Implementation Order

### Slice 1: Backend Skeleton Cleanup

1. Decide whether to remove or ignore HabitCompletionService.
2. Update solution references if services are removed.
3. Add shared current-user helper to AuthService and HabitService where needed.
4. Add common ProblemDetails/exception behavior.

Gate:

- Services build.
- Existing tests either pass or are intentionally replaced.

### Slice 2: AuthService Rewrite

1. Update auth models.
2. Map `auth` schema.
3. Store refresh token hashes.
4. Add `/me` and `/logout`.
5. Add role policies.
6. Add admin user endpoints.
7. Rewrite tests.

Gate:

- AuthService tests pass.
- JWT includes required claims.
- Normal user cannot access admin endpoint.

### Slice 3: HabitService Data Rewrite

1. Add owned `Habit` model.
2. Add `HabitCompletion` model.
3. Map `habit` schema and constraints.
4. Generate migration.
5. Add seed-free startup.

Gate:

- EF migration matches `docs/DATABASE_DESIGN.md`.
- HabitService builds.

### Slice 4: Habit CRUD

1. Rewrite commands/queries to include current user id.
2. Add validators.
3. Implement archive instead of hard delete.
4. Update controller routes.

Gate:

- User isolation tests pass.
- CRUD endpoints match docs.

### Slice 5: Completions

1. Move completion endpoints into HabitService.
2. Implement idempotent mark-complete with `PUT`.
3. Implement unmark.
4. Implement today's completions.
5. Implement habit completion history.

Gate:

- Duplicate and ownership tests pass.
- Daily check-in contract is ready for frontend.

### Slice 6: Gateway

1. Remove old completion route if service is merged.
2. Add `/api/completions`, `/api/competition`, and admin routes.
3. Verify all service URLs and IIS configs.

Gate:

- Gateway forwards every documented route.
- IIS impact has been reviewed with the owner before CI/CD changes.

IIS reminder:

- If `HabitCompletionService` is merged into `HabitService`, local IIS may need app pool, site/application, deployed folder, and gateway route updates.
- Do not assume CI/CD changes are enough; the local IIS server may need manual cleanup.

### Slice 7: OpenAPI and Docs

1. Update service-level `docs/API.md`.
2. Regenerate or update OpenAPI specs.
3. Mark old docs that no longer apply.

Gate:

- Frontend can be implemented from docs without reading backend code.

## Do Not Start Frontend Rewrite Until

- AuthService tests are green.
- HabitService ownership tests are green.
- Completion duplicate tests are green.
- Gateway routes are final.
- API response style is final.

This avoids rebuilding UI against unstable contracts.

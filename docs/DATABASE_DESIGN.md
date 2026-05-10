# HabitTracker Database Design

## Status

This is the target database contract for the rewrite. Auth and habit EF migrations now follow this foundation; future feature slices should keep this document aligned before frontend work depends on a contract.

## Database Goals

- Every habit is owned by exactly one user.
- Users can only manage their own habits unless they are admins.
- Habit completions are unique per habit and date.
- Public competition data only comes from public habits.
- Auth, habit tracking, and monitoring data are separated by schema.
- The database supports local SQL Server and IIS deployment without Docker.

## Schemas

| Schema | Purpose |
| --- | --- |
| `auth` | Users, roles, refresh tokens |
| `habit` | Habits, completions, habit summaries |
| `monitor` | Request logs and health snapshots |

## Naming Rules

- Use `uniqueidentifier` primary keys.
- Use UTC timestamps.
- Use `CreatedAt` and `UpdatedAt` where rows are user-managed.
- Use `IsActive` for soft delete/archive.
- Table names are plural.
- Foreign key columns use `{Entity}Id`.

## auth.Roles

Stores role names used for authorization.

| Column | Type | Required | Notes |
| --- | --- | --- | --- |
| `Id` | uniqueidentifier | yes | Primary key |
| `Name` | nvarchar(50) | yes | Unique. Expected values: `User`, `Admin` |
| `Description` | nvarchar(500) | no | Human-readable description |
| `CreatedAt` | datetime2 | yes | UTC |

Indexes and constraints:

- Unique index on `Name`.

## auth.Users

Stores application users.

| Column | Type | Required | Notes |
| --- | --- | --- | --- |
| `Id` | uniqueidentifier | yes | Primary key |
| `Username` | nvarchar(100) | yes | Unique |
| `Email` | nvarchar(255) | yes | Unique |
| `PhoneNumber` | nvarchar(32) | no | Normalized digits only. Unique when present |
| `PasswordHash` | nvarchar(max) | yes | BCrypt hash |
| `RoleId` | uniqueidentifier | yes | FK to `auth.Roles.Id` |
| `IsActive` | bit | yes | Allows account disable |
| `CreatedAt` | datetime2 | yes | UTC |
| `UpdatedAt` | datetime2 | yes | UTC |

Indexes and constraints:

- Unique index on `Email`.
- Unique index on `Username`.
- Unique filtered index on `PhoneNumber` where `PhoneNumber is not null`.
- Index on `RoleId`.

Notes:

- Normalize email before uniqueness checks.
- Normalize phone numbers to digits only before uniqueness checks.
- Login accepts email, username, or phone number plus password.
- Do not expose `PasswordHash` in any API response.

## auth.RefreshTokens

Stores refresh token metadata.

| Column | Type | Required | Notes |
| --- | --- | --- | --- |
| `Id` | uniqueidentifier | yes | Primary key |
| `UserId` | uniqueidentifier | yes | FK to `auth.Users.Id` |
| `TokenHash` | nvarchar(128) | yes | SHA-256 hash of refresh token, never raw token |
| `ExpiresAt` | datetime2 | yes | UTC |
| `RevokedAt` | datetime2 | no | Set during logout or rotation |
| `CreatedAt` | datetime2 | yes | UTC |

Indexes and constraints:

- Unique index on `TokenHash`.
- Index on `UserId`.
- Index on `ExpiresAt`.

Rules:

- Refresh token rotation revokes the old token and creates a new token.
- Expired or revoked tokens cannot be reused.
- Logout revokes the active refresh token.

## habit.Habits

Stores user-owned habits.

| Column | Type | Required | Notes |
| --- | --- | --- | --- |
| `Id` | uniqueidentifier | yes | Primary key |
| `UserId` | uniqueidentifier | yes | Owning user id |
| `Name` | nvarchar(200) | yes | Habit name |
| `Description` | nvarchar(1000) | no | Optional |
| `Frequency` | nvarchar(20) | yes | `daily` or `weekly` |
| `TargetDaysPerWeek` | tinyint | no | Required for weekly habits |
| `IsPublic` | bit | yes | Allows competition visibility |
| `IsActive` | bit | yes | Soft delete/archive |
| `CreatedAt` | datetime2 | yes | UTC |
| `UpdatedAt` | datetime2 | yes | UTC |

Indexes and constraints:

- Index on `(UserId, IsActive)`.
- Index on `(IsPublic, IsActive)`.
- Optional unique index on `(UserId, Name)` for active habits only if duplicate names should be blocked.
- Check constraint: `Frequency in ('daily', 'weekly')`.
- Check constraint: `TargetDaysPerWeek between 1 and 7` when not null.

Ownership rule:

- `UserId` is always taken from the JWT subject claim on create.
- Frontend must never send `UserId` for habit creation or update.

Foreign key choice:

- Recommended for this local single-database app: FK from `habit.Habits.UserId` to `auth.Users.Id`.
- If strict microservice database ownership is required later, remove cross-schema FK but keep the required `UserId` column and enforce existence in service logic.

## habit.HabitCompletions

Stores completed dates for habits.

| Column | Type | Required | Notes |
| --- | --- | --- | --- |
| `Id` | uniqueidentifier | yes | Primary key |
| `HabitId` | uniqueidentifier | yes | FK to `habit.Habits.Id` |
| `UserId` | uniqueidentifier | yes | Redundant owner id for fast user queries |
| `CompletedDate` | date | yes | Date completed, no time component |
| `Notes` | nvarchar(1000) | no | Optional completion note |
| `CreatedAt` | datetime2 | yes | UTC |

Indexes and constraints:

- Unique index on `(HabitId, CompletedDate)`.
- Index on `(UserId, CompletedDate)`.
- Index on `HabitId`.

Rules:

- A habit can only have one completion per date.
- `UserId` must match the owning `UserId` from `habit.Habits`.
- Completing an inactive habit should be rejected.
- Deleting a completion should hard delete the completion row.
- Archiving a habit should not delete historical completions unless the product explicitly asks for permanent delete.

## monitor.RequestLogs

Stores request telemetry when database-backed monitoring is required.

| Column | Type | Required | Notes |
| --- | --- | --- | --- |
| `Id` | uniqueidentifier | yes | Primary key |
| `ServiceName` | nvarchar(100) | yes | Source service |
| `Endpoint` | nvarchar(500) | yes | Request path |
| `Method` | nvarchar(10) | yes | HTTP method |
| `StatusCode` | int | yes | HTTP status |
| `DurationMs` | int | yes | Request duration |
| `UserId` | uniqueidentifier | no | Current user if authenticated |
| `CreatedAt` | datetime2 | yes | UTC |

Indexes:

- Index on `(ServiceName, CreatedAt)`.
- Index on `(UserId, CreatedAt)`.

## monitor.HealthChecks

Stores health check snapshots if needed for the admin dashboard.

| Column | Type | Required | Notes |
| --- | --- | --- | --- |
| `Id` | uniqueidentifier | yes | Primary key |
| `ServiceName` | nvarchar(100) | yes | Service name |
| `Status` | nvarchar(20) | yes | `healthy`, `degraded`, `unhealthy` |
| `ResponseMs` | int | yes | Health check duration |
| `Details` | nvarchar(1000) | no | Optional diagnostic detail |
| `CheckedAt` | datetime2 | yes | UTC |

Indexes and constraints:

- Index on `(ServiceName, CheckedAt)`.
- Check constraint: `Status in ('healthy', 'degraded', 'unhealthy')`.

## API Ownership Rules Driven By Database

### Create Habit

- Auth required.
- Server extracts `UserId` from JWT.
- Insert into `habit.Habits`.
- Ignore or reject any client-supplied `UserId`.

### List Habits

- Auth required.
- Normal users only see `where UserId = currentUserId and IsActive = 1`.
- Admin views must use explicit admin endpoints.

### Update Habit

- Auth required.
- Load habit by `Id` and `UserId`.
- Return `404` if not found or not owned by current user.
- Admin override should be separate and policy-protected.

### Archive Habit

- Auth required.
- Set `IsActive = 0`.
- Keep completions.

### Complete Habit

- Auth required.
- Load habit by `Id`, `UserId`, and `IsActive = 1`.
- Insert completion with `HabitId`, current `UserId`, and `CompletedDate`.
- If `(HabitId, CompletedDate)` already exists, return conflict or idempotent success. Choose one behavior and keep it consistent.

Recommended behavior:

- Mark complete should be idempotent: if already completed, return the existing completion.
- Explicit duplicate POST from another flow should not create a second row.

### Uncomplete Habit

- Auth required.
- Delete completion by `HabitId`, `UserId`, and `CompletedDate`, or by completion `Id` and `UserId`.

## EF Core Mapping Requirements

Each DbContext must map schema names explicitly:

```csharp
modelBuilder.HasDefaultSchema("auth");
```

or:

```csharp
entity.ToTable("Habits", "habit");
```

Do not rely on SQL scripts that drift from EF migrations. After the rewrite starts, EF migrations should be the implementation source of truth.

## Minimum Test Cases

Database and handler tests should prove:

1. User A cannot list User B's habits.
2. User A cannot update User B's habit.
3. User A cannot complete User B's habit.
4. Same habit/date cannot produce duplicate completions.
5. Archived habits are hidden from normal list results.
6. Archived habits cannot be completed.
7. Public competition queries only include `IsPublic = 1` and `IsActive = 1`.
8. Admin-only queries require admin role.

## Migration Strategy For Rewrite

For this local/dev rewrite, use a clean break. The current local app has no real users/data, so data preservation is not required.

1. Delete/reset the existing local database when ready to apply the new schema.
2. Create new EF models matching this document.
3. Delete old generated migrations if they no longer represent the target schema.
4. Generate a new initial migration.
5. Apply to a clean local database.
6. Seed roles.
7. Add test users only through dev-only seed code or local scripts.

If this app ever has real production data later, do not use this destructive reset strategy. Write explicit data migration scripts instead.

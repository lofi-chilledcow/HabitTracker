# HabitTracker Rewrite Plan

## Purpose

Rewrite HabitTracker around a clean foundation before fixing UI bugs or adding features. The current repo proves the direction, but the implemented database, API contracts, and frontend assumptions are not aligned enough to safely extend.

Use `docs/IMPLEMENTATION_CHECKLIST.md` as the master execution checklist while implementing the rewrite.

The rewrite should preserve the product goal:

- Users can register and sign in.
- Each user owns their own habits.
- Users can complete habits by date.
- Public habits can appear in competition views.
- Admin users can access admin-only views and management APIs.
- The app can run locally on IIS with microfrontends and .NET services.

The current local IIS deployment is disposable and has no real users/data. The rewrite may reset the database, hard replace the frontend, and remove/merge prototype services. IIS and CI/CD should still be updated deliberately after the code structure is stable.

## Current Foundation Problems

1. Habit ownership is missing in the implemented HabitService model. Habits do not currently store `UserId`, so the service cannot enforce per-user habit isolation.
2. The hand-written SQL script and EF migrations disagree on schemas, IDs, table names, and columns.
3. Habit completions can be created for any `HabitId` without verifying that the habit exists or belongs to the current user.
4. Duplicate completions for the same habit/date are not prevented by a unique database constraint.
5. API response shapes are inconsistent. Some frontend code expects an envelope, while backend services return raw DTOs.
6. Role-based access control exists in JWT claims but is not consistently enforced by frontend routes or backend policies.
7. Frontend auth state is split across MFEs and shell-local token helpers, causing session-state drift.

## Rewrite Principles

- Make database ownership rules impossible to ignore.
- Treat EF migrations as the database source of truth.
- Keep service boundaries simple until the core app works.
- Enforce authorization on the server first; frontend checks are only UX.
- Keep API contracts boring and consistent.
- Keep the MFE shell responsible for session state and routing.
- Build vertical slices end to end instead of finishing all backend work before any frontend work.

## Target Architecture

### Backend

Use one SQL Server database with schemas:

- `auth` for users, roles, refresh tokens.
- `habit` for habits and completions.
- `monitor` for request logs and health snapshots.

Recommended service layout for the rewrite:

| Service | Responsibility |
| --- | --- |
| ApiGateway | Single client entry point and route proxying |
| AuthService | Registration, login, refresh, logout, current profile |
| HabitService | Habit CRUD, completion tracking, summaries, competition data |

HabitCompletionService can be merged into HabitService for the rewrite. Splitting completions into a separate service adds ownership and consistency complexity without much product value yet. If service-boundary practice is a hard requirement, keep it separate only after the habit ownership model is correct.

### Frontend

Keep the MFE structure, but define contracts up front:

| MFE | Responsibility |
| --- | --- |
| shell | Auth state, layout, protected routes, remote loading |
| mfe-auth | Login and registration screens |
| mfe-habits | Habit list, create/edit form, daily check-in |
| mfe-competition | Public leaderboard and public habit progress |
| mfe-admin | Admin dashboard and user/service management |

The shell should own the session. Remotes should call shared auth/session helpers or receive session data through a clear boundary.

## Phase Plan

### Phase 1: Database Contract

Deliverables:

- Final schema documented in `docs/DATABASE_DESIGN.md`.
- EF models mapped to `auth`, `habit`, and `monitor` schemas.
- One clean initial migration per owning service or one clean database migration strategy.
- Seed roles: `User`, `Admin`.
- Unique constraints and indexes for ownership-sensitive queries.

Done when:

- A user can own habits at the database level.
- A habit completion cannot duplicate the same habit/date.
- The implemented EF schema matches the documented schema.

### Phase 2: Auth Foundation

Deliverables:

- Register, login, refresh, logout, and `/api/auth/me`.
- JWT contains user id, email, username, and role.
- Refresh tokens are stored as hashes, not raw token values.
- Role policies are configured server-side.

Done when:

- A logged-in user can fetch their current profile.
- Admin-only endpoints reject normal users.
- Refresh token rotation works and old tokens are revoked.

### Phase 3: Habit Foundation

Deliverables:

- Create/list/get/update/archive habits for the current user.
- Server extracts `UserId` from JWT, never from request body.
- `IsPublic` and `TargetDaysPerWeek` are supported.
- Archive habits with `IsActive = false`; do not hard delete by default.

Done when:

- User A cannot see or mutate User B's habits.
- Habit list only returns the current user's active habits by default.
- Admin access is explicit and policy-based.

### Phase 4: Completion Foundation

Deliverables:

- Mark/unmark habit completion by date.
- Enforce habit ownership before completion changes.
- Return today's completions for the current user.
- Return habit history for charts/calendar.

Done when:

- Duplicate completion for the same habit/date is rejected.
- User A cannot complete User B's habit.
- Daily check-in can work from two endpoints: user's habits and today's completions.

### Phase 5: API Gateway and Contracts

Deliverables:

- Gateway routes to AuthService and HabitService.
- One response style is chosen and used everywhere.
- OpenAPI docs match implementation.
- IIS-impacting service changes are identified before CI/CD updates.

Recommended response style:

- Use raw DTOs for success responses.
- Use ASP.NET Core `ProblemDetails` for errors.

Done when:

- Frontend API clients do not need per-endpoint response-shape workarounds.
- Swagger/OpenAPI accurately describes runtime behavior.
- The owner has been reminded about any required local IIS changes.

### Phase 6: MFE Shell and Auth

Deliverables:

- Shell owns auth state.
- Session survives refresh if refresh token is valid.
- `/admin/*` requires admin role.
- `/competition` can be public or protected by deliberate product choice.

Done when:

- Login updates shell state.
- Page reload does not lose user state.
- Admin nav/routes are hidden and blocked for non-admin users.

### Phase 7: Habit MFE

Deliverables:

- Habit list.
- Create/edit/archive forms.
- Daily check-in.
- Basic completion history.

Done when:

- All habit UI calls the gateway only.
- UI behavior matches server ownership rules.

### Phase 8: Competition and Admin

Deliverables:

- Public leaderboard based on public habits.
- Admin dashboard using server-side admin policies.
- Monitoring endpoints backed by `monitor` schema or structured logs.

Done when:

- Competition does not leak private habits.
- Admin APIs are not reachable by normal users.

## Key Decisions To Lock Before Coding

1. Use GUID primary keys consistently.
2. Use EF migrations as the source of truth.
3. Use schemas in EF mappings.
4. Store `UserId` on `habit.Habits`.
5. Store `UserId` redundantly on `habit.HabitCompletions` for query speed and ownership checks.
6. Use `ProblemDetails` for API errors.
7. Merge HabitCompletionService into HabitService unless there is a strong reason to keep it separate.
8. Store refresh token hashes, not raw refresh tokens.
9. Do not accept `UserId` from frontend request bodies.
10. Make soft delete/archive the default for habits.
11. Hard replace `src/frontend`; do not preserve old frontend code.
12. Reset the local database; no data migration is required for the current prototype.

## First Implementation Slice

Start with the database and backend only:

1. Replace current habit model with owned habit model.
2. Add completion model under the same habit schema.
3. Add EF schema mapping and constraints.
4. Create a clean migration.
5. Update command/query handlers to accept current user id.
6. Add tests proving user isolation and duplicate-completion rejection.

Only after this slice is green should the MFE UI bugs be fixed.

# HabitTracker Implementation Checklist

## Purpose

This is the master execution checklist for the HabitTracker rewrite. Work through it in order. Each slice should leave the app in a buildable, understandable state.

Reference docs:

- `docs/REWRITE_PLAN.md`
- `docs/DATABASE_DESIGN.md`
- `docs/BACKEND_ARCHITECTURE_DECISIONS.md`
- `docs/BACKEND_REWRITE_PLAN.md`
- `docs/FRONTEND_REWRITE_PLAN.md`
- `docs/REACT_ARCHITECTURE.md`
- `docs/CICD_DEPLOYMENT_NOTES.md`

## Ground Rules

- Do not modify CI/CD until the CI/CD slice.
- Do not modify IIS assumptions silently.
- Call out any IIS-impacting change before implementation.
- Keep EF migrations as the database source of truth.
- Keep backend response style consistent: raw DTOs plus `ProblemDetails`.
- Keep frontend auth/session owned by the shell.
- Prefer small vertical slices over broad rewrites.

## Phase 0: Decisions To Lock

- [x] Confirm whether `HabitCompletionService` will be merged into `HabitService`.
- [x] Confirm frontend rebuild strategy: hard replace `src/frontend`.
- [x] Confirm database reset strategy: disposable local database, no data migration needed.
- [ ] Confirm competition route auth mode: public or authenticated.
- [x] Confirm whether database migrations will be manual during rewrite.
- [ ] Confirm whether refresh tokens stay in localStorage temporarily or move to secure cookies later.

Recommended defaults:

- Merge `HabitCompletionService` into `HabitService`.
- Hard delete/recreate `src/frontend`.
- Reset/delete the existing local database; no data preservation required.
- Keep competition public-read but only public active habits appear.
- Keep migrations manual until schema stabilizes.
- Centralize token handling in shell; improve storage later if backend supports secure cookies.

## Phase 1: Backend Safety Baseline

- [x] Run backend builds.
- [x] Run existing backend tests.
- [x] Record current services and ports.
- [ ] Record current IIS app pools and deployed folders if local server access is needed.
- [x] Do not change CI/CD.

Gate:

- [x] Current backend builds are understood.
- [x] Current test status is known.
- [x] Current deploy workflow is preserved.

## Phase 2: Database And EF Foundation

- [x] Delete/reset existing local database when ready to apply the new schema.
- [x] Update AuthService EF models to match `auth` schema design.
- [x] Update AuthService DbContext schema mapping.
- [x] Add `IsActive` and `UpdatedAt` to users.
- [x] Change refresh tokens to store `TokenHash`.
- [x] Update role seed to seed `User` and `Admin`.
- [x] Update HabitService EF models to include owned `Habit`.
- [x] Add `HabitCompletion` model into HabitService if merging services.
- [x] Map `habit.Habits`.
- [x] Map `habit.HabitCompletions`.
- [x] Add required indexes and unique constraints.
- [x] Add frequency and target-days validation constraints.
- [x] Generate clean migrations.
- [x] Review generated migrations before applying.
- [x] Apply migrations to a clean local/dev database.

Gate:

- [x] EF schema matches `docs/DATABASE_DESIGN.md`.
- [x] Habits have required `UserId`.
- [x] Completions have unique `(HabitId, CompletedDate)`.
- [x] Roles are seeded.
- [ ] No frontend changes depend on unfinished backend contracts.

## Phase 3: AuthService Rewrite

- [x] Implement normalized email registration.
- [x] Register users with `User` role.
- [x] Reject duplicate email.
- [x] Reject duplicate username.
- [x] Reject duplicate phone number when phone is provided.
- [x] Support login by email, username, or phone number.
- [x] Reject login for inactive users.
- [x] Generate JWT with user id, email, username, role, and jti.
- [x] Store refresh token hashes only.
- [x] Implement refresh token rotation.
- [x] Implement logout.
- [x] Implement `/api/auth/me`.
- [x] Add `AdminOnly` policy.
- [x] Add admin user list endpoint.
- [x] Add admin user enable/disable endpoint.
- [x] Add admin role-change endpoint if needed.
- [ ] Add/update AuthService OpenAPI docs.

Tests:

- [x] Register creates user with `User` role.
- [x] Register rejects duplicate email.
- [x] Register rejects duplicate username.
- [x] Register rejects duplicate phone number.
- [x] Login succeeds with valid credentials.
- [x] Login succeeds with username credentials.
- [x] Login succeeds with phone credentials.
- [x] Login rejects invalid credentials.
- [x] Login rejects inactive user.
- [x] Refresh rotates token.
- [x] Refresh rejects revoked token reuse.
- [x] Logout revokes token.
- [x] `/me` returns profile.
- [x] Admin endpoints reject normal users.
- [x] Admin endpoints accept admins.

Gate:

- [x] AuthService builds.
- [x] AuthService tests pass.
- [x] Auth API response contracts are final enough for frontend auth.

## Phase 4: HabitService Rewrite

- [x] Add current-user helper.
- [x] Create habit using JWT user id.
- [x] List only current user's active habits.
- [x] Get habit by `Id` and current `UserId`.
- [x] Update habit by `Id` and current `UserId`.
- [x] Archive habit with `IsActive = false`.
- [x] Toggle public/private for owned habit.
- [x] Validate habit name.
- [x] Validate frequency.
- [x] Validate weekly target days.
- [ ] Add/update HabitService OpenAPI docs.

Tests:

- [x] Create habit stores current user id.
- [x] List habits excludes other users' habits.
- [x] List habits excludes archived habits.
- [x] Get rejects another user's habit.
- [x] Update rejects another user's habit.
- [x] Archive affects only current user's habit.
- [x] Public/private toggle affects only current user's habit.
- [x] Invalid frequency is rejected.
- [x] Weekly target days are validated.

Gate:

- [x] HabitService builds.
- [x] Habit ownership tests pass.
- [x] Habit CRUD contracts are final enough for frontend.

## Phase 5: Completion Flow

- [x] Move completion commands/queries into HabitService if service is merged.
- [x] Implement `GET /api/completions/today`.
- [x] Implement `GET /api/habits/{habitId}/completions`.
- [x] Implement idempotent `PUT /api/habits/{habitId}/completions/{date}`.
- [x] Implement `DELETE /api/habits/{habitId}/completions/{date}`.
- [x] Ensure completion write verifies habit ownership.
- [x] Reject completion for inactive habits.
- [x] Ensure duplicate same habit/date cannot create two rows.

Tests:

- [x] Mark completion succeeds for owned active habit.
- [x] Mark completion rejects another user's habit.
- [x] Mark completion rejects inactive habit.
- [x] Mark completion is idempotent for same habit/date.
- [x] Unique constraint prevents duplicate rows.
- [x] Unmark deletes only current user's completion.
- [x] Today endpoint returns only current user's completions.
- [x] History endpoint rejects another user's habit.

Gate:

- [x] Completion tests pass.
- [x] Daily check-in backend contract is ready.

## Phase 6: Competition And Admin Backend

- [x] Implement `GET /api/competition/leaderboard`.
- [x] Ensure competition only returns public active habits.
- [x] Exclude private habits.
- [x] Exclude archived habits.
- [ ] Add admin dashboard summary endpoint if needed.
- [ ] Add admin health endpoint if needed.
- [ ] Keep monitoring DB work optional until admin UI needs it.

Tests:

- [x] Leaderboard includes public active habits.
- [x] Leaderboard excludes private habits.
- [x] Leaderboard excludes archived habits.
- [x] Admin summary endpoints require admin role.

Gate:

- [x] Competition data does not leak private habits.
- [x] Admin backend is server-protected.

## Phase 7: ApiGateway

- [ ] Update routes for AuthService.
- [x] Update routes for HabitService.
- [x] Route `/api/completions/**` to HabitService if completions are merged.
- [x] Route `/api/competition/**` to HabitService.
- [x] Route `/api/admin/**` to owning service or split clearly.
- [ ] Remove old HabitCompletionService route only after service is actually removed.
- [ ] Verify CORS for frontend dev ports.

IIS-impact reminder:

- [ ] Tell owner before removing or merging a service.
- [ ] Confirm whether local IIS still has `HabitTracker-HabitCompletionService`.
- [ ] Confirm whether app pool/site/folder cleanup is needed.
- [x] Confirm gateway route changes before deployment.

Gate:

- [x] Gateway forwards all documented routes.
- [x] IIS-impacting changes are explicitly reviewed with owner.

## Phase 8: Frontend Architecture Setup

- [x] Decide `src/frontend-v2` or hard replace.
- [ ] Hard delete/recreate `src/frontend`.
- [ ] Create shell app.
- [ ] Create shared package/module.
- [ ] Add React Router.
- [ ] Add TanStack Query.
- [ ] Add shell-owned AuthProvider.
- [ ] Add shared API client.
- [ ] Add ProblemDetails parsing.
- [ ] Add shared UI primitives.
- [ ] Add MFE remote placeholders.

Gate:

- [ ] Shell runs.
- [ ] Placeholder remotes load.
- [ ] Auth/session is centralized.
- [ ] No per-MFE token helpers.

## Phase 9: Frontend Auth

- [ ] Implement login page.
- [ ] Implement register page.
- [ ] Implement `/api/auth/me` bootstrap.
- [ ] Implement refresh-on-401.
- [ ] Implement logout.
- [ ] Implement protected routes.
- [ ] Implement admin role routes.
- [ ] Implement app navigation.

Tests:

- [ ] Login success redirects to Today.
- [ ] Login failure shows error.
- [ ] Protected route redirects anonymous user.
- [ ] Admin route rejects non-admin user.
- [ ] Refresh failure logs user out.

Gate:

- [ ] User can sign in and reload without frontend state drift.
- [ ] Admin route behavior matches role.

## Phase 10: Frontend Habits

- [ ] Implement Today page.
- [ ] Fetch habits.
- [ ] Fetch today's completions.
- [ ] Mark complete with `PUT`.
- [ ] Unmark with `DELETE`.
- [ ] Add optimistic UI with rollback.
- [ ] Implement habit list.
- [ ] Implement create habit.
- [ ] Implement edit habit.
- [ ] Implement archive habit.
- [ ] Implement public/private toggle.
- [ ] Add empty/loading/error states.

Tests:

- [ ] Today renders habits and completion state.
- [ ] Mark complete calls correct endpoint.
- [ ] Unmark calls correct endpoint.
- [ ] Create habit validates required name.
- [ ] Archive removes habit from active list after success.

Gate:

- [ ] User can manage and complete habits end to end.

## Phase 11: Frontend Competition And Admin

- [ ] Implement leaderboard page.
- [ ] Verify private habits do not appear.
- [ ] Implement admin dashboard.
- [ ] Implement user management page.
- [ ] Implement health page if backend endpoint exists.
- [ ] Add responsive mobile behavior.

Gate:

- [ ] Competition is safe.
- [ ] Admin UI requires admin role.

## Phase 12: CI/CD Update

Pause before starting this phase.

Reminder:

- [ ] Tell owner this is the CI/CD update step.
- [ ] Confirm local IIS service/app pool changes.
- [ ] Confirm whether `HabitCompletionService` was merged.
- [ ] Confirm old local app is disposable and no data preservation is needed.
- [ ] Confirm frontend deployment path.
- [ ] Confirm database migration policy.

Possible backend CI/CD updates:

- [ ] Remove HabitCompletionService build/test/publish/deploy if service is removed.
- [ ] Update test project list.
- [ ] Update publish outputs.
- [ ] Update app pool stop/start commands.
- [ ] Update gateway deployment if routes changed.

Possible frontend CI/CD updates:

- [ ] Add Node setup or verify self-hosted Node.
- [ ] Install frontend dependencies.
- [ ] Run TypeScript checks.
- [ ] Run frontend tests.
- [ ] Build shell and MFEs.
- [ ] Deploy static frontend files to IIS.
- [ ] Preserve or replace frontend `web.config` deliberately.

Gate:

- [ ] Workflow still deploys backend successfully.
- [ ] Frontend static files deploy successfully if added.
- [ ] IIS app pools and deployed folders match actual services.

## Phase 13: IIS Verification

- [ ] Verify AuthService site/app pool.
- [ ] Verify HabitService site/app pool.
- [ ] Verify ApiGateway site/app pool.
- [ ] Verify HabitCompletionService is either still valid or intentionally removed.
- [ ] Verify frontend IIS site if added.
- [ ] Verify `.env` exists at expected path.
- [ ] Verify gateway routes.
- [ ] Verify health endpoints.
- [ ] Run smoke test through IIS/gateway.

Smoke tests:

- [ ] Register/login.
- [ ] `/api/auth/me`.
- [ ] Create habit.
- [ ] List habits.
- [ ] Mark today complete.
- [ ] View Today page.
- [ ] Admin route rejects normal user.

## Done Criteria For Rewrite

- [ ] Database schema matches documentation.
- [ ] Backend ownership tests pass.
- [ ] Backend role tests pass.
- [ ] Gateway routes match docs.
- [ ] Frontend uses shell-owned auth.
- [ ] Frontend calls gateway only.
- [ ] CI/CD matches actual service/frontend structure.
- [ ] IIS is aligned with deployed services.
- [ ] End-to-end habit flow works.

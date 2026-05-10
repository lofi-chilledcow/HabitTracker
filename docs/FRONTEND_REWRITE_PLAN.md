# HabitTracker Frontend Rewrite Plan

## Purpose

This document defines the frontend rewrite plan before deleting or rebuilding the current UI. Use it as the reference for architecture, page flow, UI design, state management, and MFE boundaries.

Related docs:

- `docs/REWRITE_PLAN.md`
- `docs/BACKEND_ARCHITECTURE_DECISIONS.md`
- `docs/BACKEND_REWRITE_PLAN.md`
- `docs/REACT_ARCHITECTURE.md`
- `docs/CICD_DEPLOYMENT_NOTES.md`
- `docs/IMPLEMENTATION_CHECKLIST.md`

## Frontend Verdict

Rewrite the frontend cleanly.

The current UI has useful rough page ideas, but the implementation has too much foundation drift:

- Auth state is duplicated across shell and remotes.
- Token helpers differ by MFE.
- API response assumptions do not match the backend rewrite plan.
- Route protection is only "logged in" and does not model roles.
- Admin and competition pages are mostly placeholders.
- Habit IDs are typed inconsistently.
- The UI styling is fragmented across Tailwind and inline CSS.

Keep the product intent and MFE deployment model. Rebuild the code.

## Frontend Architecture Decision

Use a shell-led MFE architecture:

```text
shell
  owns auth/session
  owns app layout/navigation
  owns protected route decisions
  loads feature remotes

mfe-auth
  login/register only

mfe-habits
  daily check-in, habit management, history

mfe-competition
  leaderboard and public habit progress

mfe-admin
  admin dashboard, users, monitoring
```

The shell is the application frame. Remotes are feature modules.

## Technology

Recommended stack:

- React
- TypeScript
- Vite
- Vite Module Federation
- React Router
- TanStack Query for server state
- A single shared API client
- Tailwind CSS or a small local design system
- Lucide React for icons

Avoid adding Redux at the start. The app mostly needs server state, not a large client-side state machine.

## MFE Boundary Rules

### Shell Owns

- Auth session bootstrap.
- Current user profile.
- Access token and refresh flow.
- Top-level layout.
- Main navigation.
- Protected route wrappers.
- Admin role route checks.
- Remote loading and error boundaries.

### Remotes Own

- Feature-specific pages.
- Feature-specific forms.
- Feature-specific data queries and mutations.
- Feature route internals under their base path.

### Remotes Must Not Own

- Token storage.
- Refresh token rotation.
- Global auth state.
- Top-level nav.
- Global route protection.
- Different API response assumptions.

## Shared Frontend Package

Create a shared frontend package or module used by all MFEs.

Possible path:

```text
src/frontend/packages/shared/
```

Shared package responsibilities:

- API client factory.
- Auth/session types.
- DTO types generated or hand-maintained from backend docs.
- Query key helpers.
- Common UI primitives.
- Error helpers for `ProblemDetails`.
- Date formatting helpers.

If a shared package feels heavy at first, create the same boundary under shell and expose it through module federation. The key is one source of truth, not five token helpers.

## Session Model

Recommended session behavior:

1. User opens app.
2. Shell checks whether a refresh token/session exists.
3. Shell calls `/api/auth/me` if an access token is present.
4. If access token is expired, shell calls `/api/auth/refresh`.
5. Shell stores current user in `AuthProvider`.
6. Remotes receive auth context through shared hooks or shell-provided module.

Session state:

```ts
type AuthUser = {
  id: string
  username: string
  email: string
  phoneNumber?: string | null
  role: 'User' | 'Admin'
}
```

Login form contract:

```ts
type LoginRequest = {
  identifier: string // email, username, or phone number
  password: string
}
```

The login UI should label the first field as `Email, username, or phone` and let the backend decide which lookup path applies.

Token storage decision:

- First rewrite: keep tokens in memory where practical.
- If page refresh persistence is required, prefer refresh token in secure cookie when backend supports it.
- If using localStorage temporarily, isolate it in one shell-owned module and document the security tradeoff.

Do not let each remote read/write localStorage independently.

## API Client Rules

All frontend API calls go through the gateway:

```text
VITE_API_URL=http://localhost:5000
```

Do not call service ports directly.

API client behavior:

- Attach access token.
- Handle `401` by attempting one refresh.
- Queue concurrent requests during refresh.
- On refresh failure, clear session and route to login.
- Parse `ProblemDetails` consistently.
- Return raw DTOs for success.

Do not unwrap `{ success, data, errors }` because the backend rewrite uses raw DTOs plus `ProblemDetails`.

## Route Map

### Public Routes

| Route | Page | Notes |
| --- | --- | --- |
| `/auth/login` | Login | Redirect to `/habits/today` if already signed in |
| `/auth/register` | Register | Redirect to `/habits/today` after success |
| `/competition` | Leaderboard | Public or protected, depending final product decision |

### User Routes

| Route | Page | Purpose |
| --- | --- | --- |
| `/habits/today` | Today | Daily check-in |
| `/habits` | Habit List | Manage active habits |
| `/habits/new` | New Habit | Create habit |
| `/habits/:id/edit` | Edit Habit | Update habit settings |
| `/habits/:id` | Habit Detail | History, streak, settings entry |
| `/habits/:id/history` | Habit History | Calendar/list of completions |

Default signed-in route:

```text
/habits/today
```

### Admin Routes

| Route | Page | Purpose |
| --- | --- | --- |
| `/admin` | Admin Dashboard | System overview |
| `/admin/users` | User Management | View, disable, role changes |
| `/admin/health` | Service Health | Health checks |
| `/admin/logs` | Request Logs | Optional later |

Admin routes require user role `Admin` in the shell and backend. Frontend route checks are UX only.

## Navigation Model

Use a quiet app shell, not a marketing page.

Desktop:

- Left sidebar or top nav.
- Primary links: Today, Habits, Competition.
- Admin link only for admins.
- User menu with profile and sign out.

Mobile:

- Bottom navigation for Today, Habits, Competition.
- Admin under user menu or hidden unless admin.

Recommended nav labels:

- Today
- Habits
- Competition
- Admin

## Page Designs

### Login

Purpose:

- Let existing users sign in quickly.

Elements:

- Email input.
- Password input.
- Submit button.
- Link to register.
- Inline validation and server error display.

Behavior:

- On success, shell stores session and redirects to `/habits/today`.
- On failure, show concise error.

### Register

Purpose:

- Create standard `User` account.

Elements:

- Username input.
- Email input.
- Password input.
- Submit button.
- Link to login.

Behavior:

- On success, user is logged in and sent to `/habits/today`.

### Today

Purpose:

- Fast daily habit check-in.

This is the primary app screen.

Elements:

- Date header.
- Completion count: `3 of 5`.
- List of active habits.
- Large accessible completion toggle per habit.
- Empty state with create habit action.
- Link/button to manage habits.

Behavior:

- Load `GET /api/habits`.
- Load `GET /api/completions/today`.
- Mark complete with `PUT /api/habits/{habitId}/completions/{date}`.
- Unmark with `DELETE /api/habits/{habitId}/completions/{date}`.
- Use optimistic UI with rollback on failure.

Design goal:

- Calm, fast, one-screen workflow.
- No dashboard clutter on the check-in screen.

### Habit List

Purpose:

- Manage active habits.

Elements:

- List/table of habits.
- Frequency badge.
- Public/private indicator.
- Streak or recent completion summary if available.
- Create button.
- Row actions: edit, archive.

Behavior:

- Archive, do not hard delete.
- Confirm archive with a modal/dialog.

### New/Edit Habit

Purpose:

- Create or update habit settings.

Fields:

- Name.
- Description.
- Frequency: daily or weekly.
- Target days per week for weekly habits.
- Public toggle.

Behavior:

- Validate client-side before submit.
- Backend remains source of truth.
- Save redirects to habit list or detail.

### Habit Detail

Purpose:

- See one habit's progress.

Elements:

- Habit name and status.
- Streak.
- Completion calendar or recent history.
- Public/private status.
- Edit action.
- Archive action.

This page can be phase 2 if the first UI slice needs to stay small.

### Competition

Purpose:

- Show public habit activity without leaking private data.

Elements:

- Leaderboard.
- User display name.
- Public completion counts.
- Current streak or weekly count.

Behavior:

- Reads `/api/competition/leaderboard`.
- Does not depend on private habit APIs.

### Admin Dashboard

Purpose:

- Give admins a compact operational overview.

Elements:

- Total users.
- Active users.
- Total active habits.
- Completion count today.
- Service health.
- Links to users and health pages.

Design:

- Dense, scan-friendly, not a marketing dashboard.

### User Management

Purpose:

- Admin manages users.

Elements:

- User table.
- Search/filter.
- Role column.
- Status column.
- Actions: disable/enable, change role.

Behavior:

- All actions call admin endpoints.
- Normal users cannot access route or backend endpoint.

## UI Design Direction

The app should feel like a quiet productivity tool:

- Clean layout.
- High readability.
- Fast repeated actions.
- Few decorative elements.
- Accessible controls.
- Consistent spacing.
- No oversized hero sections inside the app.
- No card-inside-card layouts.
- Avoid one-color monotony.

Suggested tone:

- Calm.
- Focused.
- Encouraging without being childish.

Suggested visual system:

- Neutral base: white/off-white backgrounds, gray text.
- Primary accent: green or teal for completion.
- Secondary accent: blue or indigo for navigation/actions.
- Warning/destructive: red for archive/disable.
- Status colors: green, amber, red.

Do not make everything purple/indigo. Use accent colors by purpose.

## Component Inventory

Shared UI primitives:

- `Button`
- `IconButton`
- `TextField`
- `TextArea`
- `Select`
- `Switch`
- `Badge`
- `Dialog`
- `Toast`
- `EmptyState`
- `Spinner`
- `ErrorState`
- `PageHeader`
- `AppShell`
- `ProtectedRoute`
- `RoleRoute`

Feature components:

- `HabitCompletionToggle`
- `HabitCard` or `HabitRow`
- `HabitForm`
- `CompletionCalendar`
- `StreakStat`
- `LeaderboardTable`
- `UserTable`
- `ServiceHealthList`

Use icons from Lucide React where useful:

- Check
- Plus
- Pencil
- Archive
- Trash only if permanent delete exists
- Lock
- User
- LogOut
- Shield
- Calendar
- Trophy

## Data Fetching Plan

Use TanStack Query.

Query keys:

```ts
['auth', 'me']
['habits']
['habits', habitId]
['habits', habitId, 'completions']
['completions', 'today']
['competition', 'leaderboard']
['admin', 'users']
['admin', 'health']
```

Mutation invalidation:

- Create habit invalidates `['habits']`.
- Update habit invalidates `['habits']` and `['habits', habitId]`.
- Archive habit invalidates `['habits']`.
- Mark/unmark completion invalidates `['completions', 'today']`, `['habits', habitId, 'completions']`, and summary/streak queries when added.

## Frontend Testing Plan

Test the flows that protect the app experience:

- Login success redirects to Today.
- Login failure shows error.
- Protected route redirects anonymous user.
- Admin route rejects non-admin user.
- Today page renders habits and completion state.
- Mark complete calls correct `PUT` endpoint.
- Unmark calls correct `DELETE` endpoint.
- Create habit validates required name.
- Archive habit removes it from list after success.
- API client refreshes once on `401`.
- API client logs out on refresh failure.

Use:

- React Testing Library for components.
- MSW for API mocking.
- Playwright later for end-to-end local IIS/gateway flows.

## Implementation Slices

### Slice 1: Frontend Skeleton

1. Delete and recreate `src/frontend`.
2. Create shell app.
3. Add shared package/module.
4. Add app shell layout.
5. Add routing and remote loading placeholders.

Gate:

- Shell runs.
- Placeholder routes load.
- Build succeeds.

### Slice 2: Auth

1. Implement API client.
2. Implement AuthProvider.
3. Implement login/register pages.
4. Implement `/api/auth/me` bootstrap.
5. Implement protected routes.
6. Implement admin route guard.

Gate:

- Login/register flow works against backend.
- Refresh/reload behavior works.
- Admin route respects role.

### Slice 3: Habit Check-In

1. Implement Today page.
2. Fetch habits and today's completions.
3. Implement mark/unmark completion.
4. Add empty state.

Gate:

- User can complete and uncomplete today's habits.
- UI state matches backend after refresh.

### Slice 4: Habit Management

1. Implement habit list.
2. Implement create/edit form.
3. Implement archive.
4. Implement public/private toggle.

Gate:

- User can manage own habits end to end.
- Archived habits disappear from active list.

### Slice 5: Competition

1. Implement leaderboard page.
2. Add public habit display if backend supports it.
3. Add loading/empty/error states.

Gate:

- Private habits never appear.
- Page works for chosen auth mode.

### Slice 6: Admin

1. Implement admin dashboard.
2. Implement user table.
3. Implement user status/role actions.
4. Add service health page if backend exists.

Gate:

- Admin can manage users.
- Normal user cannot access admin UI or API.

### Slice 7: Polish and Accessibility

1. Keyboard navigation.
2. Focus states.
3. Responsive mobile layout.
4. Loading skeletons.
5. Error and empty states.
6. Visual consistency pass.

Gate:

- Desktop and mobile screenshots are clean.
- No text overflow.
- Forms and toggles are accessible.

## Delete vs Rebuild Strategy

Decision:

Hard delete and recreate `src/frontend`.

The current local app is disposable and has no real users/data. Do not preserve old frontend code unless a specific file is intentionally copied as reference.

Rules:

1. Do not delete all of `src`; backend services live under `src/services`.
2. Delete/recreate only `src/frontend`.
3. Keep repo-level docs.
4. Build the new shell-led MFE structure from scratch.
5. Do not mix old and new MFE token/session logic.

## Do Not Start Frontend Implementation Until

- Backend response style is final.
- Auth endpoints are final.
- Habit ownership endpoints are final.
- Completion endpoints are final.
- Gateway routes are final.

The frontend rewrite should follow stable contracts, not chase backend churn.

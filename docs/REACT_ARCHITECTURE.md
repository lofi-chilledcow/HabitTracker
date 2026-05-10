# React Architecture

## Purpose

This document defines the React architecture for the frontend rewrite. It complements `docs/FRONTEND_REWRITE_PLAN.md` with concrete folder boundaries, state rules, and module ownership.

Related docs:

- `docs/FRONTEND_REWRITE_PLAN.md`
- `docs/BACKEND_ARCHITECTURE_DECISIONS.md`
- `docs/BACKEND_REWRITE_PLAN.md`

## Architecture Summary

Use a shell-led MFE React architecture:

```text
Shell owns app architecture.
Remotes own feature screens.
Shared package owns contracts, API clients, utilities, and UI primitives.
TanStack Query owns server state.
React local state owns form and UI state.
```

Do not add Redux at the start.

## Target Folder Shape

```text
src/frontend/
  shell/
    src/
      app/
        App.tsx
        router.tsx
        AppShell.tsx
        providers.tsx
      auth/
        AuthProvider.tsx
        ProtectedRoute.tsx
        RoleRoute.tsx
      layout/
        Sidebar.tsx
        TopBar.tsx
        MobileNav.tsx
      remotes/
        RemoteErrorBoundary.tsx
        remoteRoutes.tsx

  packages/
    shared/
      src/
        api/
          client.ts
          authApi.ts
          habitsApi.ts
          competitionApi.ts
          adminApi.ts
        auth/
          types.ts
          session.ts
        components/
          Button.tsx
          IconButton.tsx
          Dialog.tsx
          Badge.tsx
          EmptyState.tsx
          Spinner.tsx
          ErrorState.tsx
        errors/
          problemDetails.ts
        query/
          keys.ts
        utils/
          dates.ts

  mfe-auth/
    src/
      AuthApp.tsx
      pages/
        LoginPage.tsx
        RegisterPage.tsx

  mfe-habits/
    src/
      HabitsApp.tsx
      pages/
        TodayPage.tsx
        HabitListPage.tsx
        HabitFormPage.tsx
        HabitDetailPage.tsx
        HabitHistoryPage.tsx
      components/
        HabitToggle.tsx
        HabitForm.tsx
        HabitRow.tsx
        CompletionCalendar.tsx

  mfe-competition/
    src/
      CompetitionApp.tsx
      pages/
        LeaderboardPage.tsx
      components/
        LeaderboardTable.tsx

  mfe-admin/
    src/
      AdminApp.tsx
      pages/
        AdminDashboardPage.tsx
        UsersPage.tsx
        HealthPage.tsx
      components/
        UserTable.tsx
        ServiceHealthList.tsx
```

The current `src/frontend` can be hard deleted and recreated for the rewrite. Do not delete `src/services` or `src/database`.

## Shell Responsibilities

The shell owns:

- Browser router.
- Global providers.
- Auth bootstrap.
- Session refresh.
- Current user state.
- App layout.
- Navigation.
- Protected route decisions.
- Admin role route decisions.
- Remote loading.
- Remote error boundaries.

The shell should not own feature page internals.

## Remote Responsibilities

Remotes own feature screens under their route base:

| Remote | Base Route | Responsibility |
| --- | --- | --- |
| `mfe-auth` | `/auth/*` | Login and register |
| `mfe-habits` | `/habits/*` | Daily check-in and habit management |
| `mfe-competition` | `/competition/*` | Leaderboard and public habit data |
| `mfe-admin` | `/admin/*` | Admin dashboard and management pages |

Remotes should not create their own global auth model or token refresh logic.

## Shared Package Responsibilities

The shared package owns cross-MFE contracts:

- API client.
- API DTO types.
- Auth/session types.
- Query keys.
- ProblemDetails parsing.
- Date formatting.
- Common UI primitives.

If a piece of code is copied into more than one MFE, it probably belongs in shared.

## State Ownership

| State Type | Owner |
| --- | --- |
| Auth/session | Shell `AuthProvider` |
| Current user | Shell `AuthProvider` |
| Server data | TanStack Query |
| Form inputs | Local component state or form library |
| Dialog open/closed | Local component state |
| Toasts | Shell-level toast provider |
| Feature filters | URL search params or local state |
| Cross-MFE state | Avoid unless truly needed |

## Data Fetching

Use TanStack Query for server data.

Query keys should come from shared:

```ts
export const queryKeys = {
  me: ['auth', 'me'] as const,
  habits: ['habits'] as const,
  habit: (habitId: string) => ['habits', habitId] as const,
  habitCompletions: (habitId: string) =>
    ['habits', habitId, 'completions'] as const,
  todaysCompletions: ['completions', 'today'] as const,
  leaderboard: ['competition', 'leaderboard'] as const,
  adminUsers: ['admin', 'users'] as const,
}
```

Mutations should invalidate precise related queries, not the entire app.

## API Client

All API calls use the gateway:

```text
VITE_API_URL=http://localhost:5000
```

API client responsibilities:

- Attach access token.
- Retry once after refresh on `401`.
- Prevent refresh stampedes.
- Clear session on refresh failure.
- Parse `ProblemDetails`.
- Return raw DTOs.

No remote should create its own token helper.

## Routing

Shell-level routes:

```text
/auth/*
/habits/*
/competition/*
/admin/*
```

Remote-level routes:

```text
mfe-auth:
  /login
  /register

mfe-habits:
  /today
  /
  /new
  /:id
  /:id/edit
  /:id/history

mfe-competition:
  /

mfe-admin:
  /
  /users
  /health
```

Route protection belongs in shell:

- Anonymous users can access `/auth/*`.
- Authenticated users can access `/habits/*`.
- Competition route is public or authenticated based on final product decision.
- Admin route requires `Admin` role.

## Component Design

Prefer small, boring components.

Good component boundaries:

- Page component owns data fetching and mutation wiring.
- Feature component owns feature-specific rendering.
- Shared primitive owns styling and accessibility.

Example:

```text
TodayPage
  fetches habits and completions
  owns optimistic mutation wiring
  renders HabitToggle rows

HabitToggle
  receives checked/loading/error props
  renders accessible toggle button
```

Avoid:

- page-sized shared components
- business logic buried in UI primitives
- duplicated API calls inside low-level components
- global state for one-page concerns

## Styling

Use one consistent styling strategy.

Recommended:

- Tailwind CSS for layout and styling.
- Shared UI primitives for consistency.
- Lucide React for icons.

Rules:

- No card-inside-card layouts.
- No decorative gradient blobs.
- No giant hero pages inside the app.
- Use stable dimensions for toggles, nav items, and repeated rows.
- Ensure mobile layouts are designed, not accidental.
- Ensure text never overflows buttons or cards.

## MFE Communication

Preferred communication:

- Shared API contracts.
- Shared auth/session hooks.
- URL routing.
- TanStack Query invalidation inside each feature.

Avoid window events for normal app communication. They are hard to trace and were part of the current frontend drift.

Use browser events only for exceptional integration cases.

## Testing Strategy

Test at three levels:

1. Shared API client tests.
2. Component/page tests with mocked API.
3. End-to-end smoke tests after backend and gateway stabilize.

Required tests:

- AuthProvider bootstraps current user.
- API client refreshes once on `401`.
- ProtectedRoute redirects anonymous users.
- RoleRoute rejects non-admin users.
- Today page marks and unmarks completions with correct endpoints.
- Habit form validates required fields.
- Admin users page is not reachable by normal users.

Use MSW for API mocks.

## What To Avoid

Avoid:

- Redux for initial rewrite.
- Per-MFE token helpers.
- Per-MFE API response parsing rules.
- Window events for auth state.
- Duplicated shared components.
- Direct service-port API calls.
- UI-only authorization.
- Mixing old and new frontend modules.

## Implementation Rule

Build the new React architecture only after backend API contracts are stable enough to avoid churn:

- Auth endpoints final.
- Habit endpoints final.
- Completion endpoints final.
- Gateway routes final.
- Response format final.

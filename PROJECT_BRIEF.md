# HabitTracker — Project Brief

> **This file is the single source of truth for the project.**
> Feed this to Claude Code at the start of any session:
> `@PROJECT_BRIEF.md` or "Read PROJECT_BRIEF.md and follow the current step."

---

## What Is This App?

A habit tracking app where users can sign up, log in, create daily or weekly habits, track completions, and share progress on a public competition page. An admin dashboard monitors app health.

---

## Tech Stack

| Layer          | Technology                              |
|----------------|-----------------------------------------|
| Frontend       | React + Webpack 5 Module Federation     |
| Backend        | C# / ASP.NET Core Web API              |
| Database       | SQL Server                              |
| Auth           | JWT (access + refresh tokens)           |
| Hosting        | Windows Server / IIS                    |
| Proxy / LB     | IIS Application Request Routing (ARR)   |
| CI/CD          | GitHub Actions (YAML)                   |
| Frontend Tests | Jest + React Testing Library            |
| Backend Tests  | xUnit + Moq                            |

---

## Architecture

```
                  ┌──────────────┐
                  │  IIS (ARR)   │  ← Reverse Proxy + Load Balancer
                  │  Port 80/443 │
                  └──────┬───────┘
                         │
          ┌──────────────┼──────────────┐
          │              │              │
   ┌──────▼──────┐ ┌────▼─────┐ ┌──────▼──────┐
   │ React Shell  │ │ Auth API │ │ Habit API   │
   │ (MFE Host)   │ │ C# :5001 │ │ C# :5002   │
   └──────────────┘ └────┬─────┘ └──────┬──────┘
                         │              │
                  ┌──────▼──────────────▼──────┐
                  │        SQL Server          │
                  │  Schemas: auth | habit |   │
                  │           monitor          │
                  └────────────────────────────┘
```

### Microfrontends (React)

| MFE              | Route             | Purpose                          |
|------------------|-------------------|----------------------------------|
| Shell            | `/`               | Host app, layout, nav, routing   |
| mfe-auth         | `/auth/*`         | Login, register pages            |
| mfe-habits       | `/habits/*`       | Dashboard, habit CRUD, calendar  |
| mfe-competition  | `/competition/*`  | Public leaderboard, shared habits|
| mfe-admin        | `/admin/*`        | Monitoring dashboard (admin only)|

### Microservices (C# ASP.NET Web API)

| Service       | Port | Owns Schema | Responsibility                    |
|---------------|------|-------------|-----------------------------------|
| AuthService   | 5001 | `auth`      | Register, login, JWT, user mgmt   |
| HabitService  | 5002 | `habit`     | Habits CRUD, completions, streaks |

Both services write to `monitor` schema for request logging.

---

## Database Schemas

### `auth.Users`
UserId (PK), Email (unique), PasswordHash, DisplayName, Role ('user'|'admin'), IsActive, CreatedAt, UpdatedAt

### `auth.RefreshTokens`
TokenId (PK), UserId (FK), Token, ExpiresAt, CreatedAt, RevokedAt

### `habit.Habits`
HabitId (PK), UserId (FK), Name, Description, Frequency ('daily'|'weekly'), TargetDaysPerWeek, IsPublic, IsActive, CreatedAt, UpdatedAt

### `habit.HabitCompletions`
CompletionId (PK), HabitId (FK), CompletedDate (unique per habit), CreatedAt

### `monitor.RequestLogs`
LogId (PK), ServiceName, Endpoint, Method, StatusCode, DurationMs, UserId, CreatedAt

### `monitor.HealthChecks`
CheckId (PK), ServiceName, Status, ResponseMs, Details, CheckedAt

---

## API Endpoints

### Auth Service (port 5001)
```
POST   /api/auth/register        — Create account
POST   /api/auth/login            — Get JWT tokens
POST   /api/auth/refresh          — Refresh access token
POST   /api/auth/logout           — Revoke refresh token
GET    /api/auth/me               — Get current user profile
```

### Habit Service (port 5002)
```
GET    /api/habits                — List user's habits
POST   /api/habits                — Create habit
PUT    /api/habits/{id}           — Update habit
DELETE /api/habits/{id}           — Archive habit (soft delete)
PUT    /api/habits/{id}/visibility — Toggle IsPublic

POST   /api/habits/{id}/complete  — Mark habit done for a date
DELETE /api/habits/{id}/complete/{date} — Unmark completion

GET    /api/habits/summary        — User's completion history
GET    /api/habits/{id}/streak    — Current streak for a habit

GET    /api/competition           — Public habits + completions
```

### Both Services
```
GET    /health                    — Health check endpoint
```

---

## Folder Structure

```
HabitTracker/
├── src/
│   ├── frontend/
│   │   ├── shell/                # MFE host app
│   │   ├── mfe-auth/             # Auth microfrontend
│   │   ├── mfe-habits/           # Habits microfrontend
│   │   ├── mfe-competition/      # Competition microfrontend
│   │   └── mfe-admin/            # Admin monitoring microfrontend
│   ├── services/
│   │   ├── AuthService/          # C# Web API
│   │   │   ├── Controllers/
│   │   │   ├── Models/
│   │   │   ├── DTOs/
│   │   │   ├── Services/
│   │   │   └── Data/
│   │   └── HabitService/         # C# Web API
│   │       ├── Controllers/
│   │       ├── Models/
│   │       ├── DTOs/
│   │       ├── Services/
│   │       └── Data/
│   └── database/
│       └── scripts/              # SQL migration scripts
├── tests/
│   ├── frontend/                 # Jest tests per MFE
│   └── backend/                  # xUnit test projects
│       ├── AuthService.Tests/
│       └── HabitService.Tests/
├── infra/
│   ├── iis/                      # IIS config, ARR rules, URL rewrite
│   └── deploy/                   # Deployment scripts (PowerShell)
├── .github/
│   └── workflows/
│       └── ci-cd.yml             # GitHub Actions pipeline
├── PROJECT_BRIEF.md              # ← This file
└── README.md
```

---

## Build Steps (In Order)

Follow these steps sequentially. Each step should be completed and tested before moving to the next.

### Step 1: Project Setup ⬜
- [ ] Create folder structure
- [ ] Initialize git repo
- [ ] Add PROJECT_BRIEF.md
- [ ] Add .gitignore (Node, .NET, SQL)
- [ ] Save database script to `src/database/scripts/001_CreateDatabase.sql`
- [ ] Run SQL script against local SQL Server

### Step 2: Auth Service ⬜
- [ ] Create ASP.NET Web API project in `src/services/AuthService/`
- [ ] Add EF Core with SQL Server provider
- [ ] Create models: User, RefreshToken
- [ ] Create DbContext mapping to `auth` schema
- [ ] Implement AuthService (register, login, refresh, logout)
- [ ] Implement JWT token generation + validation
- [ ] Create AuthController with all endpoints
- [ ] Add request logging middleware (writes to `monitor.RequestLogs`)
- [ ] Add health check endpoint
- [ ] Write xUnit tests in `tests/backend/AuthService.Tests/`

### Step 3: Habit Service ⬜
- [ ] Create ASP.NET Web API project in `src/services/HabitService/`
- [ ] Add EF Core with SQL Server provider
- [ ] Create models: Habit, HabitCompletion
- [ ] Create DbContext mapping to `habit` schema
- [ ] Implement HabitService (CRUD, completions, streaks, competition)
- [ ] Implement JWT validation (validate tokens from Auth Service)
- [ ] Create HabitsController, CompletionsController
- [ ] Add request logging middleware
- [ ] Add health check endpoint
- [ ] Write xUnit tests in `tests/backend/HabitService.Tests/`

### Step 4: React Shell + Auth MFE ⬜
- [ ] Initialize shell app with Webpack 5 + Module Federation
- [ ] Set up shared dependencies (react, react-dom, react-router)
- [ ] Create shell layout (nav, sidebar, routing)
- [ ] Initialize mfe-auth with Webpack Module Federation
- [ ] Build login page + register page
- [ ] Wire auth API calls + store JWT in memory
- [ ] Create AuthContext for shared auth state
- [ ] Write Jest tests for auth components

### Step 5: Habits MFE ⬜
- [ ] Initialize mfe-habits with Module Federation
- [ ] Build dashboard page (today's habits + checkboxes)
- [ ] Build habit form (create/edit with public toggle)
- [ ] Build completion history / calendar view
- [ ] Wire habit API calls
- [ ] Write Jest tests

### Step 6: Competition MFE ⬜
- [ ] Initialize mfe-competition with Module Federation
- [ ] Build leaderboard page
- [ ] Show public habits with streaks and completion counts
- [ ] Write Jest tests

### Step 7: Admin MFE ⬜
- [ ] Initialize mfe-admin with Module Federation
- [ ] Build monitoring dashboard (admin role only)
- [ ] Show: service health, request stats, error rates, active users
- [ ] Wire to monitor schema data via Habit Service (or new endpoint)
- [ ] Write Jest tests

### Step 8: IIS + ARR Setup ⬜
- [ ] Install IIS, ARR, URL Rewrite on Windows Server VM
- [ ] Create IIS site for React shell (static files)
- [ ] Configure URL Rewrite rules:
  - `/api/auth/*` → localhost:5001
  - `/api/habits/*` → localhost:5002
  - `/api/competition/*` → localhost:5002
- [ ] Configure ARR load balancing (even with single instance — ready to scale)
- [ ] Enable SSL (self-signed cert for home lab)
- [ ] Save all config to `infra/iis/`

### Step 9: CI/CD Pipeline ⬜
- [ ] Create GitHub Actions workflow `.github/workflows/ci-cd.yml`
- [ ] Stage 1: Build + test (frontend Jest, backend xUnit)
- [ ] Stage 2: Publish (React build, C# publish)
- [ ] Stage 3: Deploy to server (WinRM or self-hosted runner)
- [ ] Save deployment scripts to `infra/deploy/`

---

## Design Decisions

| Decision                     | Choice              | Reason                                      |
|------------------------------|----------------------|---------------------------------------------|
| Habits per user (not shared) | One-to-many          | Users own their habits, no cross-user edits |
| Competition via IsPublic flag| Flag on Habits table  | Minimal, no extra tables needed             |
| Single DB, multiple schemas  | auth, habit, monitor | Service boundaries without DB overhead      |
| IIS ARR for proxy + LB       | Native to Windows    | No extra software on Windows Server         |
| Module Federation for MFE    | Webpack 5            | Most common, well-documented approach       |
| JWT in memory (not localStorage) | AuthContext      | More secure, avoids XSS risks              |

---

## Future Phases (Not Now)

**Phase 2 — Engagement:** Cheers/likes table, Followers table, notifications
**Phase 3 — Groups:** Groups + GroupMembers tables, challenges, invite system

---

## How To Use This File With Claude Code

Start each Claude Code session with:
```
Read PROJECT_BRIEF.md. I'm on Step [X]. Do the next task.
```

Or for specific work:
```
Read PROJECT_BRIEF.md. Create the AuthService project following Step 2.
```

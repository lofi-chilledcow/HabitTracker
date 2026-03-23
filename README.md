# HabitTracker

A habit tracking API that lets users sign up, create daily or weekly habits, and track completions over time. Built as a set of independently deployable .NET 8 microservices behind a YARP API Gateway.

---

## Architecture

```
                        ┌─────────────────────────────┐
                        │         API Gateway          │
                        │      localhost:5000           │
                        │    (Yarp.ReverseProxy)       │
                        └──────────┬──────────────────-┘
                                   │
               ┌───────────────────┼───────────────────┐
               │                   │                   │
               ▼                   ▼                   ▼
    ┌──────────────────┐ ┌──────────────────┐ ┌──────────────────────────┐
    │   AuthService    │ │   HabitService   │ │  HabitCompletionService  │
    │  localhost:5039  │ │  localhost:5110  │ │     localhost:5225       │
    └────────┬─────────┘ └────────┬─────────┘ └───────────┬─────────────┘
             │                    │                        │
             └────────────────────┴────────────────────────┘
                                  │
                          ┌───────▼────────┐
                          │   SQL Server   │
                          │ HabitTracker_  │
                          │     Dev        │
                          └────────────────┘
```

All client traffic enters through the API Gateway on port 5000. Each downstream service owns its own database schema. AuthService issues JWTs that HabitService and HabitCompletionService validate directly.

---

## Services

| Service | Port | Description |
|---------|------|-------------|
| ApiGateway | 5000 | YARP reverse proxy — single entry point for all clients |
| AuthService | 5039 | User registration, JWT login, token refresh |
| HabitService | 5110 | Habit CRUD (JWT-protected) |
| HabitCompletionService | 5225 | Completion tracking per habit and per day (JWT-protected) |

---

## Project Structure

```
HabitTracker/
├── .env.example
├── .env                   # local secrets — never commit
├── CLAUDE.md
├── README.md
└── src/
    └── services/
        ├── ApiGateway/
        ├── AuthService/
        ├── HabitService/
        └── HabitCompletionService/
```

---

## Environment Setup

### 1. Copy the example env file

```bash
cp .env.example .env
```

Edit `.env` and fill in your values:

```env
HABITTRACKER_DB_PASSWORD=your_password_here
HABITTRACKER_JWT_SECRET=your_32_character_minimum_secret_here
```

| Variable | Used by | Description |
|----------|---------|-------------|
| `HABITTRACKER_DB_PASSWORD` | AuthService, HabitService, HabitCompletionService | SQL Server password for `HabitTracker_Dev` |
| `HABITTRACKER_JWT_SECRET` | AuthService | JWT signing secret — minimum 32 characters |

> **Never commit `.env`** — it is listed in `.gitignore`. Each service loads it automatically at startup via `DotNetEnv`.

---

## Running Locally

Start all four services in separate terminals. Run each command from the repo root.

### API Gateway

```bash
cd src/services/ApiGateway
dotnet run
# Listening on http://localhost:5000
```

### AuthService

```bash
cd src/services/AuthService
dotnet run
# Listening on http://localhost:5039
# Swagger UI: http://localhost:5039/swagger
```

### HabitService

```bash
cd src/services/HabitService
dotnet run
# Listening on http://localhost:5110
# Swagger UI: http://localhost:5110/swagger
```

### HabitCompletionService

```bash
cd src/services/HabitCompletionService
dotnet run
# Listening on http://localhost:5225
# Swagger UI: http://localhost:5225/swagger
```

> The API Gateway must be running for end-to-end requests. Each downstream service can also be called directly during development.

---

## API Gateway Routes

All routes below are relative to the gateway base URL `http://localhost:5000`.

| Gateway path | Forwards to | Service |
|--------------|-------------|---------|
| `/api/auth/**` | `http://localhost:5039` | AuthService |
| `/api/habits/**` | `http://localhost:5110` | HabitService |
| `/api/habit-completions/**` | `http://localhost:5225` | HabitCompletionService |

---

## API Reference

### AuthService — `/api/auth`

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| `POST` | `/api/auth/register` | None | Register a new user |
| `POST` | `/api/auth/login` | None | Login and receive access + refresh tokens |
| `POST` | `/api/auth/refresh` | None | Exchange a refresh token for a new access token |

### HabitService — `/api/habits`

All endpoints require a valid JWT in the `Authorization: Bearer <token>` header.

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/api/habits` | List all habits |
| `GET` | `/api/habits/{id}` | Get a habit by ID |
| `POST` | `/api/habits` | Create a habit |
| `PUT` | `/api/habits/{id}` | Update a habit |
| `DELETE` | `/api/habits/{id}` | Delete a habit |

### HabitCompletionService — `/api/habit-completions`

All endpoints require a valid JWT in the `Authorization: Bearer <token>` header.

| Method | Path | Description |
|--------|------|-------------|
| `POST` | `/api/habit-completions` | Record a completion |
| `GET` | `/api/habit-completions/habit/{habitId}` | List completions for a habit |
| `GET` | `/api/habit-completions/today` | List today's completions for the current user |
| `DELETE` | `/api/habit-completions/{id}` | Delete a completion |

### Response envelope

Every response uses a consistent envelope:

```json
{ "success": true,  "data": { },   "errors": null }
{ "success": false, "data": null,  "errors": ["Email is already registered."] }
```

| Status | Meaning |
|--------|---------|
| `200` | Successful read or update |
| `201` | Resource created |
| `204` | Successful delete (no body) |
| `400` | Validation failure |
| `401` | Missing, invalid, or expired token |
| `409` | Duplicate resource |

---

## Git Commit Standards

Commits follow [Conventional Commits](https://www.conventionalcommits.org/):

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

`auth` · `habits` · `completions` · `infra` · `shared`

### Examples

```
feat(auth): add JWT login endpoint
feat(completions): add today's completions query
fix(habits): fix delete returning 404 on valid ID
chore(infra): add DotNetEnv package
docs(completions): add API.md and OpenAPI spec
refactor(auth): extract JWT generation into JwtTokenService
```

### Rules

- One commit per completed feature or fix — do not batch unrelated changes
- Description is lowercase, imperative mood, no trailing period
- Keep the description under 72 characters

---

## Git Workflow

This project uses **trunk-based development**. All work is committed directly to `main`.

- No long-lived feature branches
- Commits are small and independently deployable
- CI runs on every push to `main`
- If a change is risky, it is gated behind a feature flag rather than a branch

This keeps history linear, eliminates merge conflicts, and means `main` is always the source of truth for what is deployed.

---

## Deployment Roadmap

Planned deployment phases in order. Each phase builds on the previous one.

### Phase 1 — IIS on a Windows VM

The initial production target is a single Windows VM running IIS. Each service is hosted as a separate IIS site backed by a .NET 8 Windows service or in-process host.

**Why IIS over Docker:**
The target VM is Windows-only without Docker Desktop or WSL2 available. IIS is a first-class host for ASP.NET Core on Windows, has no container runtime overhead, and integrates directly with Windows auth and certificate management. Docker can be revisited if deployment targets change.

Planned steps:
1. Provision VM, install IIS + .NET 8 Hosting Bundle + SQL Server
2. Create one IIS site per service, each bound to its own port
3. Set environment variables via IIS application pool or Windows environment
4. Configure IIS as a reverse proxy in front of the API Gateway (optional, for port 80/443)
5. Verify end-to-end routing through the gateway

### Phase 2 — CI/CD with GitHub Actions

Automate build, test, and deployment on every push to `main`.

Planned pipeline:
1. `build` job — `dotnet restore` + `dotnet build` for all services
2. `test` job — run unit tests (depends on `build`)
3. `deploy` job — publish each service and robocopy artifacts to the VM over SSH (depends on `test`)
4. Trigger IIS app pool recycle via `appcmd` after artifact copy

Secrets (`HABITTRACKER_DB_PASSWORD`, `HABITTRACKER_JWT_SECRET`, VM credentials) will be stored in GitHub Actions repository secrets.

### Phase 3 — Unit Tests

Add an xUnit test project per service covering:

- Command and query handlers (business logic)
- Validators (FluentValidation rules)
- JWT generation and validation (AuthService)

Tests run in-memory — no database required. Integration tests that hit SQL Server are a later addition.

### Phase 4 — Frontend React App

A React SPA living under `src/frontend/`. Planned scope:

- Auth flow (register, login, token refresh)
- Habit list and CRUD forms
- Daily completion check-off view
- Calls the API Gateway exclusively — no direct service calls

Hosting: served as static files from IIS alongside the API, or from a separate IIS site on port 3000.

# HabitTracker — Dev Log

A step-by-step journal documenting how I use Claude Code in VS Code to build this app from scratch. Each entry captures the exact prompt used, what Claude did, and what the outcome was — so this log can serve as both a reference and a reproducible guide.

---

## Entry Template

```
## Step N — [Step Name]
**Date:** YYYY-MM-DD
**Status:** ⬜ In Progress | ✅ Done | ❌ Blocked

**Prompt used:**
> Paste the exact message sent to Claude Code here.

**What Claude Code does:**
- Bullet list of actions taken (files created, commands run, decisions made)

**Expected result:**
- What should exist / work after this step is complete

**Actual result:**
- What actually happened (fill in after running)

**Notes / Decisions:**
- Any architecture choices, surprises, or things to remember
```

---

## Step 1 — Project Setup
**Date:** 2026-03-05
**Status:** ⬜ In Progress

**Prompt used:**
> Read PROJECT_BRIEF.md. I'm on Step 1. Do all the tasks.
> *(with follow-up: "on the backend server midtier, using C# and .NET, combining the Repository pattern for data abstraction with CQRS")*

**What Claude Code does:**
- Creates full folder structure under `src/`, `tests/`, `infra/`, `.github/` as defined in PROJECT_BRIEF.md
- Adds `.gitkeep` files so empty directories are tracked by git
- Creates `.gitignore` covering Node/frontend, .NET/backend, and SQL artifacts
- Creates `src/database/scripts/001_CreateDatabase.sql` with all schemas and tables (`auth`, `habit`, `monitor`)
- Runs the SQL script against local SQL Server to initialize the database
- Updates Step 1 checkboxes in PROJECT_BRIEF.md to ✅

**Backend folder structure uses CQRS + Repository pattern:**
```
src/services/{Service}/
├── Controllers/
├── Models/
├── DTOs/
├── Commands/
│   └── Handlers/
├── Queries/
│   └── Handlers/
├── Repositories/
└── Data/
```

**Expected result:**
- All project folders exist and are committed to git
- `.gitignore` prevents `node_modules/`, `bin/`, `obj/`, secrets from being tracked
- SQL Server has a `HabitTracker` database with schemas: `auth`, `habit`, `monitor`
- All 6 tables exist: `auth.Users`, `auth.RefreshTokens`, `habit.Habits`, `habit.HabitCompletions`, `monitor.RequestLogs`, `monitor.HealthChecks`

**Actual result:**
*(fill in after running)*

**Notes / Decisions:**
- Backend uses CQRS + Repository pattern (not plain service layer) — commands for writes, queries for reads, repositories for data access abstraction
- Single SQL Server database with multiple schemas (not separate DBs per service) — keeps infrastructure simple while maintaining logical service boundaries
- git repo was already initialized; PROJECT_BRIEF.md and DEVLOG.md already created before this step ran

---

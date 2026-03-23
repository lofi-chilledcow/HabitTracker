# HabitTracker
A simple habit tracking app that lets users sign up, create daily or weekly habits, and track completion over time.

## Getting Started

### 1. Configure environment variables

Copy the example env file and fill in your values:

```bash
cp .env.example .env
```

| Variable | Used by | Description |
|---|---|---|
| `HABITTRACKER_DB_PASSWORD` | AuthService, HabitService | SQL Server password for `HabitTracker_Dev` |
| `HABITTRACKER_JWT_SECRET` | AuthService | JWT signing secret (32+ characters) |

Export the variables before running any service (or use your IDE's launch profile):

```bash
# Linux / macOS
export HABITTRACKER_DB_PASSWORD=your_password_here
export HABITTRACKER_JWT_SECRET=your_32_character_minimum_secret_here

# PowerShell
$env:HABITTRACKER_DB_PASSWORD="your_password_here"
$env:HABITTRACKER_JWT_SECRET="your_32_character_minimum_secret_here"
```

> **Never commit `.env`** — it is already listed in `.gitignore`.

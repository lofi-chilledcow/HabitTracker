# HabitService

A .NET 8 Web API microservice for managing habits. Built with the CQRS pattern via MediatR and Entity Framework Core with SQL Server.

---

## Architecture

HabitService follows a **CQRS (Command Query Responsibility Segregation)** architecture, separating read operations (queries) from write operations (commands). All requests flow through MediatR, which decouples the HTTP layer from business logic.

```
HTTP Request
    └── Controller
            └── IMediator.Send(...)
                    ├── Query → QueryHandler → DbContext (read-only)
                    └── Command → CommandHandler → DbContext (write)
```

### Why CQRS here

- Queries use `AsNoTracking()` — no change tracking overhead for reads
- Commands own their transaction scope via `SaveChangesAsync`
- Handlers are independently testable without touching the controller
- Adding new operations means adding a new Command or Query file — no existing code changes

---

## Project Structure

```
HabitService/
├── Commands/                        # Write-side: what the system does
│   ├── CreateHabitCommand.cs
│   ├── UpdateHabitCommand.cs
│   ├── DeleteHabitCommand.cs
│   └── Handlers/
│       ├── CreateHabitCommandHandler.cs
│       ├── UpdateHabitCommandHandler.cs
│       └── DeleteHabitCommandHandler.cs
│
├── Queries/                         # Read-side: what the system returns
│   ├── GetAllHabitsQuery.cs
│   ├── GetHabitByIdQuery.cs
│   └── Handlers/
│       ├── GetAllHabitsQueryHandler.cs
│       └── GetHabitByIdQueryHandler.cs
│
├── Controllers/
│   └── HabitsController.cs          # Thin HTTP layer — delegates to MediatR
│
├── Data/
│   └── HabitDbContext.cs            # EF Core DbContext
│
├── Models/
│   └── Habit.cs                     # Domain entity
│
├── DTOs/
│   └── HabitDto.cs                  # Request/response shapes (records)
│
├── Migrations/                      # EF Core migration history
├── Program.cs                       # Service registration and middleware
├── appsettings.json                 # Connection string and logging config
└── HabitService.csproj
```

---

## Data Model

### `Habit` entity

| Field         | Type       | Notes                          |
|---------------|------------|--------------------------------|
| `Id`          | `int`      | Primary key, auto-increment    |
| `Name`        | `string`   | Required, max 200 chars        |
| `Description` | `string?`  | Optional, max 1000 chars       |
| `Frequency`   | `string`   | Required, max 50 chars (e.g. `Daily`, `Weekly`) |
| `CreatedAt`   | `DateTime` | Defaults to `GETUTCDATE()`     |
| `IsActive`    | `bool`     | Defaults to `true`             |

---

## API Endpoints

Base URL: `http://localhost:5110/api/habits`

### GET `/api/habits`
Returns all habits.

**Response `200 OK`**
```json
[
  {
    "id": 1,
    "name": "Morning Run",
    "description": "Run 5km before breakfast",
    "frequency": "Daily",
    "createdAt": "2026-03-21T08:00:00Z",
    "isActive": true
  }
]
```

---

### GET `/api/habits/{id}`
Returns a single habit by ID.

**Response `200 OK`**
```json
{
  "id": 1,
  "name": "Morning Run",
  "description": "Run 5km before breakfast",
  "frequency": "Daily",
  "createdAt": "2026-03-21T08:00:00Z",
  "isActive": true
}
```

**Response `404 Not Found`** — habit does not exist.

---

### POST `/api/habits`
Creates a new habit.

**Request body**
```json
{
  "name": "Morning Run",
  "description": "Run 5km before breakfast",
  "frequency": "Daily"
}
```

| Field         | Required | Notes               |
|---------------|----------|---------------------|
| `name`        | Yes      |                     |
| `description` | No       |                     |
| `frequency`   | Yes      | e.g. `Daily`, `Weekly`, `Monthly` |

**Response `201 Created`** — returns the created habit with its assigned `id`.

---

### PUT `/api/habits/{id}`
Updates an existing habit.

**Request body**
```json
{
  "name": "Morning Run",
  "description": "Run 5km before breakfast",
  "frequency": "Daily",
  "isActive": true
}
```

**Response `200 OK`** — returns the updated habit.
**Response `404 Not Found`** — habit does not exist.

---

### DELETE `/api/habits/{id}`
Deletes a habit.

**Response `204 No Content`** — successfully deleted.
**Response `404 Not Found`** — habit does not exist.

---

## CQRS Flow

Each request maps to exactly one MediatR message type:

| HTTP Method | Endpoint           | MediatR Message           | Handler                        |
|-------------|--------------------|---------------------------|--------------------------------|
| GET         | `/api/habits`      | `GetAllHabitsQuery`       | `GetAllHabitsQueryHandler`     |
| GET         | `/api/habits/{id}` | `GetHabitByIdQuery`       | `GetHabitByIdQueryHandler`     |
| POST        | `/api/habits`      | `CreateHabitCommand`      | `CreateHabitCommandHandler`    |
| PUT         | `/api/habits/{id}` | `UpdateHabitCommand`      | `UpdateHabitCommandHandler`    |
| DELETE      | `/api/habits/{id}` | `DeleteHabitCommand`      | `DeleteHabitCommandHandler`    |

Handlers are registered automatically via:
```csharp
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
```

---

## Dependencies

| Package                                   | Version | Purpose                    |
|-------------------------------------------|---------|----------------------------|
| `MediatR`                                 | 12.4.1  | CQRS mediator pipeline     |
| `Microsoft.EntityFrameworkCore.SqlServer` | 9.0.3   | SQL Server ORM provider    |
| `Microsoft.EntityFrameworkCore.Tools`     | 9.0.3   | EF CLI migration tooling   |
| `Microsoft.EntityFrameworkCore.Design`    | 9.0.3   | Design-time EF support     |
| `Swashbuckle.AspNetCore`                  | 7.3.1   | Swagger/OpenAPI UI         |

---

## Running Locally

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- [dotnet-ef CLI tool](https://learn.microsoft.com/en-us/ef/core/cli/dotnet)
- SQL Server accessible at `192.168.1.226,1433`

Install the EF CLI tool if not already installed:
```bash
dotnet tool install --global dotnet-ef
```

### 1. Clone and navigate

```bash
git clone <repo-url>
cd src/services/HabitService
```

### 2. Configure the connection string

The default connection string in `appsettings.json` points to the dev SQL Server:
```
Server=192.168.1.226,1433;Database=HabitTracker_Dev;User Id=sa;Password=...;TrustServerCertificate=True
```

To override locally without editing `appsettings.json`, use `appsettings.Development.json` or an environment variable:
```bash
export ConnectionStrings__DefaultConnection="Server=localhost;Database=HabitTracker_Dev;..."
```

### 3. Apply the database migration

```bash
dotnet ef database update
```

This creates the `Habits` table in the target database. To roll back:
```bash
dotnet ef database update 0
```

### 4. Run the service

```bash
dotnet run
```

The service starts on:
- HTTP: `http://localhost:5110`
- HTTPS: `https://localhost:7006`

### 5. Open Swagger UI

```
http://localhost:5110/swagger
```

Use the Swagger UI to explore and test all endpoints interactively.

---

## Adding a New Operation

To add a new command or query, follow this pattern:

1. **Add the message** in `Commands/` or `Queries/`:
   ```csharp
   public record DeactivateHabitCommand(int Id) : IRequest<bool>;
   ```

2. **Add the handler** in `Commands/Handlers/` or `Queries/Handlers/`:
   ```csharp
   public class DeactivateHabitCommandHandler(HabitDbContext db)
       : IRequestHandler<DeactivateHabitCommand, bool>
   {
       public async Task<bool> Handle(DeactivateHabitCommand request, CancellationToken ct)
       {
           var habit = await db.Habits.FindAsync(request.Id, ct);
           if (habit is null) return false;
           habit.IsActive = false;
           await db.SaveChangesAsync(ct);
           return true;
       }
   }
   ```

3. **Call it from the controller:**
   ```csharp
   await mediator.Send(new DeactivateHabitCommand(id), cancellationToken);
   ```

MediatR discovers the handler automatically — no registration needed.

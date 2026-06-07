# AGENTS.md — Monthly Expense Tracker (Backend)

## Project Overview

A server-side REST API for personal expense tracking. Users record income/expense transactions, set monthly budgets per category, and export reports. Built with Clean Architecture + CQRS. Data stored in PostgreSQL.

## Tech Stack

| Layer | Technology | Version |
|-------|-----------|---------|
| Runtime | .NET / ASP.NET Core | net10.0 |
| Language | C# | 12 |
| ORM | Entity Framework Core | 10.0.8 |
| Database | PostgreSQL (via Npgsql) | 10.0.2 |
| CQRS | MediatR | 14.1.0 |
| Mapping | AutoMapper | 12.0.1 |
| Validation | FluentValidation | 12.1.1 |
| Auth | JWT Bearer (Microsoft) | 10.0.8 |
| Password Hashing | BCrypt.Net-Next | 4.2.0 |
| Logging | Serilog | 10.0.0 |
| API Docs | Swashbuckle / Swagger | 10.2.1 |
| Testing | xUnit + Moq + FluentAssertions | 2.9.3 / 4.20.72 / 8.10.0 |

## Dev Commands

```powershell
# Restore
dotnet restore ExpenseTracker.slnx

# Build
dotnet build ExpenseTracker.slnx

# Run (development, port 5141)
dotnet run --project src/API/ExpenseTracker.API.csproj

# Tests
dotnet test ExpenseTracker.slnx
```

Prerequisites: .NET 10.0 SDK, PostgreSQL running on `localhost:5432`. App auto-applies EF migrations on startup.

## Core Logic Summary

- **Transactions**: Income/Expense records with soft delete, linked to a Category
- **Budgets**: Per-category monthly spending limits, upserted (one per category per month)
- **Reports**: Aggregated summaries, category breakdowns, trend lines, top categories
- **Export**: Monthly report as file (currently CSV placeholder, QuestPDF planned)
- **Business rules enforced in MediatR command/query handlers**

## Key Constraints

1. **Do NOT modify Domain entities** unless the data model changes — they are the core of the application
2. **Do NOT bypass MediatR** — all business logic lives in handlers, not controllers
3. **Do NOT disable soft delete** — Transactions and Categories use `IsDeleted` + `DeletedAt`; always filter `.Where(x => !x.IsDeleted)`
4. **Do NOT change PostgreSQL enum type** — `TransactionType` (`Income`, `Expense`) is mapped as a native PG enum
5. **Do NOT use `DateTimeOffset`** — all timestamps are `DateTime.UtcNow`; no timezone logic exists
6. **Do NOT hardcode secrets** — JWT `Key`, connection strings, CORS origins come from `appsettings.json` / environment
7. **Do NOT remove the `ExpenseTracker.slnx` solution file structure** — the 4-project clean architecture layout is required
8. **Auth is required** — all endpoints except `POST /api/v1/auth/register`, `/login`, `/refresh` require `[Authorize]`
9. **Default categories** (Vietnamese locale) are seeded per-user — these must never be deleted or renamed
10. **Max 50 categories per user** — enforced in the `CreateCategoryCommand` handler
11. **Branch Management**: Before adding any features or fix bugs, always work on
a new git branch. Never commit directly on main. Bug branches must follow naming
convention bug/[desc], feature branches follow naming convention feature/[desc], never commit and push directly to main

## Additional Documentation

Detailed technical documentation is in `.agents/docs/`:

- [Architecture](./.agents/docs/architecture.md) — Clean Architecture layers, CQRS, middleware pipeline, DI setup
- [Data Model](./.agents/docs/data_model.md) — Entities, relationships, EF Core config, constraints, soft delete pattern
- [Auth](./.agents/docs/auth.md) — JWT + refresh token flow, registration/login, token rotation
- [Date Logic](./.agents/docs/date_logic.md) — `DateOnly` usage, UTC convention, budget/report date calculations

# Architecture

## Project Structure (Clean Architecture)

```
backend/
├── ExpenseTracker.slnx              # Solution file (new .slnx format, VS 2025+)
├── src/
│   ├── Domain/                      # Innermost layer — no dependencies
│   │   ├── Entities/                # User, Transaction, Category, MonthlyBudget, RefreshToken
│   │   ├── Enums/                   # TransactionType (Income, Expense)
│   │   └── Exceptions/              # DomainException
│   ├── Application/                 # Use cases — depends on Domain
│   │   ├── Common/                  # Interfaces, Behaviors, Mappings, Exceptions
│   │   ├── DTOs/                    # Request/Response records
│   │   └── Features/                # MediatR Commands + Queries grouped by feature
│   ├── Infrastructure/              # Data access, auth — depends on Application
│   │   ├── Persistence/             # AppDbContext, Migrations, Seed
│   │   └── Auth/                    # JwtTokenService, CurrentUserService
│   └── API/                         # Presentation — depends on Application + Infrastructure
│       ├── Controllers/             # REST controllers (route prefix: api/v1/)
│       └── Middleware/              # ExceptionHandling, RequestLogging
└── tests/
    ├── UnitTests/                   # xUnit + Moq (empty — no test source yet)
    └── IntegrationTests/            # xUnit + Microsoft.AspNetCore.Mvc.Testing (empty)
```

### Dependency Flow

`API → Application → Domain` and `Infrastructure → Application`. Domain has **zero dependencies**.

## CQRS with MediatR

Every operation is either a **Command** (write) or **Query** (read), implemented as a MediatR `IRequest<TResponse>` handler.

```
Controller → MediatR.Send(command/query) → Handler → AppDbContext → Response
```

- Handlers contain **all business logic**. Controllers are thin — they parse HTTP and call `MediatR.Send()`.
- Pipeline behaviors wrap handler execution:
  1. `LoggingBehavior` — logs start/end/failure of each request
  2. `ValidationBehavior` — runs all FluentValidation validators before handler; throws `ValidationException` on failure

## Middleware Pipeline (order in Program.cs)

```
ExceptionHandlingMiddleware     # Catches exceptions → RFC 7807 ProblemDetails
RequestLoggingMiddleware        # Logs method, path, status, duration
CORS ("AllowFrontend")
Authentication (JWT Bearer)
Authorization
MapControllers
```

### Exception → HTTP Status mapping

| Exception | Status | Title |
|-----------|--------|-------|
| `ValidationException` | 400 | Validation Error |
| `UnauthorizedException` | 401 | Unauthorized |
| `DomainException` | 409 | Business Rule Violation |
| `KeyNotFoundException` | 404 | Not Found |
| All others | 500 | Internal Server Error |

Note: `AuthController` has local try/catch that duplicates some of this middleware. When editing auth endpoints, check both the handler and the controller's catch blocks.

## Dependency Injection

DI registration is split:

- **Application layer**: `Application.DependencyInjection` — registers MediatR, FluentValidation, AutoMapper, pipeline behaviors
- **Infrastructure layer**: `Infrastructure.DependencyInjection` — registers `AppDbContext`, `ITokenService`, `ICurrentUserService`
- **API layer**: `Program.cs` — registers controllers, Swagger, JWT auth, CORS, Serilog, calls both DI modules

## Startup Sequence

1. `Program.cs`: Build host, configure services
2. `WebApplication`: Configure middleware pipeline
3. On first request: `dbContext.Database.MigrateAsync()` (auto-applies EF migrations)
4. After migration: calls `DefaultCategoriesSeeder.SeedAsync()` per user

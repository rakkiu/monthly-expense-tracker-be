# Date & Time Logic

## Core Convention

**All timestamps are UTC.** There is no timezone conversion, no `DateTimeOffset`, no user timezone preference. This is a deliberate design choice for simplicity.

## Types Used

| Field Type | Used For | Example |
|-----------|----------|---------|
| `DateTime` (UTC) | `CreatedAt`, `UpdatedAt`, `DeletedAt`, `ExpiresAt` | `DateTime.UtcNow` |
| `DateOnly` | `TransactionDate`, `MonthlyBudget.MonthYear` | `new DateOnly(2026, 6, 15)` |

## Transaction Dates

- Stored as `DateOnly` — no time component
- Serialized as `yyyy-MM-dd` in API responses (via AutoMapper)
- Client must send `TransactionDate` as a date string (e.g., `"2026-06-15"`)
- DTO uses `string` for `TransactionDate`, parsed to `DateOnly` in the handler

## Budget Dates

- `MonthlyBudget.MonthYear` is always the **first day of the month** (`yyyy-MM-01`)
- Clients send `MonthYear` as `"yyyy-MM"` format (e.g., `"2026-06"`)
- Handler parses: `DateOnly.Parse($"{command.MonthYear}-01")`

## Query Date Handling

All list/query endpoints that filter by date accept nullable `month` and `year` integers:

- Default: `DateTime.UtcNow.Month` / `DateTime.UtcNow.Year`
- Budget queries: use `new DateOnly(year, month, 1)` to filter `MonthYear`
- Transaction queries: use `transaction.TransactionDate.Month == month && transaction.TransactionDate.Year == year`
- Transaction date range: `transaction.TransactionDate >= start && transaction.TransactionDate <= end`

## Report Date Logic

### Monthly Summary
- Compares requested month vs previous month for percentage change
- Previous month calculated as: `new DateOnly(year, month, 1).AddMonths(-1)`

### Trend
- `GET /api/v1/reports/trend?months=6` (max 6)
- Start date: `DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-months + 1))`
- Groups transactions by month, returns `TrendItem(Date, Amount)` for each month

### Top Categories
- Queries transactions for the requested month/year
- Groups by `CategoryId`, sums `Amount`, takes top 5 ordered by total descending

## Refresh Token Expiry

```csharp
// Access token
Expires = DateTime.UtcNow.AddMinutes(15)

// Refresh token
ExpiresAt = DateTime.UtcNow.AddDays(7)
```

The `RefreshToken` entity has computed properties:

```csharp
IsExpired => DateTime.UtcNow >= ExpiresAt
IsActive  => !IsRevoked && !IsExpired
```

## Important Notes

- **No recurrence engine** yet — `IsRecurring` + `RecurringId` fields exist but no logic processes them
- **No timezone handling** anywhere — adding it would require changing all `DateTime.UtcNow` usages and adding user timezone preference storage
- **JSON serialization**: `DateOnly` serializes as `"yyyy-MM-dd"` by default with .NET 10
- **All date comparisons are calendar-date based**, not culture-aware

# Data Model

## Entities

### User
```
Id              Guid        PK
Email           string      Unique, auto-lowercased
PasswordHash    string      BCrypt hash
FullName        string
IsActive        bool        Default: true
CreatedAt       DateTime    UTC
UpdatedAt       DateTime    UTC
```

### Transaction
```
Id              Guid        PK
UserId          Guid        FK → User
CategoryId      Guid        FK → Category
Amount          decimal(15,2)
Type            TransactionType  PostgreSQL enum (Income, Expense)
Description     string?     Nullable
TransactionDate DateOnly    No time component
IsRecurring     bool
RecurringId     Guid?       For future recurring-expense logic
IsDeleted       bool        Soft delete flag
DeletedAt       DateTime?   UTC
CreatedAt       DateTime    UTC
UpdatedAt       DateTime    UTC
```

### Category
```
Id              Guid        PK
UserId          Guid        FK → User
Name            string
Type            TransactionType
Color           string      Hex, default "#6B7280"
Icon            string?     Material icon name
IsDefault       bool        System-seeded, cannot edit/delete
IsDeleted       bool        Soft delete flag
CreatedAt       DateTime    UTC
```

### MonthlyBudget
```
Id              Guid        PK
UserId          Guid        FK → User
CategoryId      Guid        FK → Category
Amount          decimal(15,2)
MonthYear       DateOnly    Always first day of month (yyyy-MM-01)
CreatedAt       DateTime    UTC
UpdatedAt       DateTime    UTC
```

### RefreshToken
```
Id              Guid        PK
UserId          Guid        FK → User
Token           string      Unique
ExpiresAt       DateTime    UTC
IsRevoked       bool
CreatedAt       DateTime    UTC
```

## Relationships

```
User ──1:N── Transaction
User ──1:N── Category
User ──1:N── MonthlyBudget
User ──1:N── RefreshToken
Category ──1:N── Transaction
Category ──1:N── MonthlyBudget
```

- Deleting a User cascades to all children
- Deleting a Category with Restrict on Transaction — code first reassigns to "Other" category before deleting

## EF Core Configuration

- **PostgreSQL enum**: `modelBuilder.HasPostgresEnum<TransactionType>()` — maps C# enum to native PG type
- **Decimal precision**: `HasPrecision(15, 2)` on all `Amount` fields
- **Filtered indexes**: `HasFilter("\"IsDeleted\" = false")` on Transaction and Category tables

### Unique Constraints

| Entity | Constraint |
|--------|-----------|
| User | `Email` (unique index) |
| Category | `(UserId, Name, Type)` — unique per user |
| MonthlyBudget | `(UserId, CategoryId, MonthYear)` — one budget per category per month |
| RefreshToken | `Token` (unique index) |

## Soft Delete Pattern

`Transaction` and `Category` use soft delete:

```
// Apply in all queries:
.Where(x => !x.IsDeleted)

// Delete operation:
entity.IsDeleted = true;
entity.DeletedAt = DateTime.UtcNow;
```

The database has filtered indexes to exclude soft-deleted rows.

### Category Deletion Special Case

When deleting a Category, the handler first reassigns all transactions in that category to a "Other" category (same user + same type), then soft-deletes the category. This prevents orphaned transactions.

## Seeding

`DefaultCategoriesSeeder.cs` seeds 12 default categories per user on every application startup (idempotent — checks existence by `(UserId, Name, Type)`):

| Name | Type | Color |
|------|------|-------|
| Ăn uống | Expense | #EF4444 |
| Di lại | Expense | #F97316 |
| Mua sắm | Expense | #EAB308 |
| Hóa đơn & Tiện ích | Expense | #22C55E |
| Giải trí | Expense | #3B82F6 |
| Sức khỏe | Expense | #8B5CF6 |
| Giáo dục | Expense | #EC4899 |
| Khác | Expense | #6B7280 |
| Lương | Income | #22C55E |
| Làm thêm | Income | #3B82F6 |
| Đầu tư | Income | #8B5CF6 |
| Thu nhập khác | Income | #6B7280 |

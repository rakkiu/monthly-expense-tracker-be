# Authentication & Authorization

## JWT Configuration

- **Scheme**: `JwtBearerDefaults.AuthenticationScheme`
- **Access token expiry**: 15 minutes (`DateTime.UtcNow.AddMinutes(15)`)
- **Refresh token expiry**: 7 days (`DateTime.UtcNow.AddDays(7)`)
- **Issuer / Audience**: Configurable via `appsettings.json` (`Jwt:Issuer`, `Jwt:Audience`)
- **Signing key**: Symmetric (HMAC-SHA256), from `appsettings.json` (`Jwt:Key`)

### JWT Claims

```csharp
ClaimTypes.NameIdentifier = User Guid
ClaimTypes.Email          = Email (lowercased on register)
ClaimTypes.Name           = FullName
```

## Endpoints

| Method | Path | Auth | Cookie Set |
|--------|------|------|-----------|
| POST | `/api/v1/auth/register` | No | No |
| POST | `/api/v1/auth/login` | No | Yes (refreshToken) |
| POST | `/api/v1/auth/refresh` | No | Yes (refreshToken) |
| POST | `/api/v1/auth/logout` | Yes | Clears cookie |
| PATCH | `/api/v1/auth/change-password` | Yes | No |

## Registration Flow

1. Validate `RegisterRequest` (FluentValidation: name 2-50 chars, email format, password 6+ chars, passwords match)
2. Check email uniqueness (case-insensitive, stored lowercase)
3. Hash password with BCrypt (`BCrypt.Net.BCrypt.HashPassword(password)`)
4. Create `User` + seed 12 default categories via `DefaultCategoriesSeeder`
5. Generate JWT + refresh token
6. Return `AuthResponse(AccessToken, ExpiresAt, UserDto)`

## Login Flow

1. Find user by email (lowercased)
2. Verify password with `BCrypt.Net.BCrypt.Verify(password, user.PasswordHash)`
3. If inactive or bad password: throw `UnauthorizedException` → 401
4. Generate JWT + refresh token
5. Set `refreshToken` cookie (HttpOnly, Secure, SameSite=Strict)
6. Return `AuthResponse`

## Refresh Token Rotation

1. Read `refreshToken` from cookie
2. Find matching `RefreshToken` in DB by token value
3. Verify `IsActive` (not revoked and not expired)
4. Revoke old token (`IsRevoked = true`)
5. Generate new JWT + new refresh token
6. Set new `refreshToken` cookie
7. Return new `AuthResponse`

## Logout Flow

1. Find refresh token from cookie
2. Revoke it in DB
3. Clear the cookie

## CurrentUserService

Registered as `ICurrentUserService` — scoped, uses `IHttpContextAccessor`:

```csharp
UserId   → ClaimTypes.NameIdentifier parsed as Guid
Email    → ClaimTypes.Email
IsAuthenticated → true when identity is authenticated
```

Used by handlers to scope all queries/commands to the authenticated user.

## Security Notes

- **No email verification** — accounts are active immediately
- **No role-based authorization** — just `[Authorize]` vs public
- **No rate limiting** — not implemented
- **No lockout policy** — repeated failed logins are not tracked
- **Refresh tokens stored in DB** — enables server-side revocation
- **Cookie name**: `refreshToken` (case-sensitive)

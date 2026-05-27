# Local Postgres Identity Auth And Storage Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace Supabase local development dependencies with Docker Compose PostgreSQL, ASP.NET Core Identity, secure JWT plus refresh-token-cookie auth, and API-served local file storage.

**Architecture:** Keep PostgreSQL as the single local database and use the existing `ApplicationDbContext` as the Identity-backed EF context. The API owns authentication, role claims, refresh-token rotation, and local file storage; Angular stores only the short-lived access token in memory and uses an `HttpOnly` refresh cookie to restore sessions.

**Tech Stack:** ASP.NET Core, ASP.NET Core Identity, EF Core/Npgsql, Docker Compose, JWT bearer auth, Angular, xUnit, Testcontainers PostgreSQL.

---

## File Structure

- Create: `docker-compose.yml` for local PostgreSQL.
- Modify: `.env.example` to document local DB/JWT/storage settings.
- Modify: `src/Ecommerce.Api/appsettings.Development.json` to point at local Docker PostgreSQL and local storage.
- Modify: `src/Ecommerce.Infrastructure/Ecommerce.Infrastructure.csproj` to add Identity EF package if missing.
- Modify: `src/Ecommerce.Infrastructure/Persistence/ApplicationDbContext.cs` to inherit from Identity context and include refresh tokens.
- Create: `src/Ecommerce.Infrastructure/Identity/ApplicationUser.cs`.
- Create: `src/Ecommerce.Infrastructure/Identity/ApplicationRole.cs`.
- Create: `src/Ecommerce.Infrastructure/Identity/RefreshToken.cs`.
- Create: `src/Ecommerce.Infrastructure/Identity/JwtOptions.cs`.
- Create: `src/Ecommerce.Infrastructure/Identity/RefreshTokenOptions.cs`.
- Create: `src/Ecommerce.Infrastructure/Identity/IJwtTokenService.cs`.
- Create: `src/Ecommerce.Infrastructure/Identity/JwtTokenService.cs`.
- Create: `src/Ecommerce.Infrastructure/Identity/IRefreshTokenService.cs`.
- Create: `src/Ecommerce.Infrastructure/Identity/RefreshTokenService.cs`.
- Modify: `src/Ecommerce.Infrastructure/DependencyInjection.cs` to register Identity, token services, and storage implementation.
- Create: `src/Ecommerce.Infrastructure/Storage/LocalStorageOptions.cs`.
- Create: `src/Ecommerce.Infrastructure/Storage/LocalFileStorageService.cs`.
- Modify: `src/Ecommerce.Api/Program.cs` to use local JWT validation, static files, and secure auth cookie behavior.
- Create: `src/Ecommerce.Api/Controllers/AuthController.cs`.
- Create: `src/Ecommerce.Api/Auth/AuthContracts.cs`.
- Modify: `src/Ecommerce.Infrastructure/Persistence/Seed/CatalogSeed.cs` or create `src/Ecommerce.Infrastructure/Persistence/Seed/IdentitySeed.cs` for a development admin user.
- Create/rename: `apps/admin/src/app/core/services/auth.service.ts`.
- Modify: `apps/admin/src/app/core/interceptors/auth-token.interceptor.ts`.
- Modify: `apps/admin/src/app/core/guards/admin-auth.guard.ts`.
- Modify: `apps/admin/src/app/features/auth/login.page.ts`.
- Modify: `apps/admin/src/app/features/auth/register.page.ts` or remove route if local registration is not supported.
- Add/modify API integration tests in `tests/Ecommerce.Api.Tests`.
- Add/modify Angular unit tests in `apps/admin/src/app`.

---

### Task 1: Add Docker Compose PostgreSQL For Local Development

**Files:**
- Create: `docker-compose.yml`
- Modify: `.env.example`
- Modify: `src/Ecommerce.Api/appsettings.Development.json`
- Test: manual Docker startup check

- [ ] **Step 1: Create local PostgreSQL compose file**

Create `docker-compose.yml`:

```yaml
services:
  postgres:
    image: postgres:16-alpine
    container_name: toyshops-postgres
    environment:
      POSTGRES_DB: toyshops
      POSTGRES_USER: toyshops
      POSTGRES_PASSWORD: toyshops
    ports:
      - "5432:5432"
    volumes:
      - toyshops-postgres-data:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U toyshops -d toyshops"]
      interval: 5s
      timeout: 5s
      retries: 10

volumes:
  toyshops-postgres-data:
```

- [ ] **Step 2: Update env documentation**

Add or update `.env.example` with:

```text
ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=toyshops;Username=toyshops;Password=toyshops
Jwt__Issuer=ToyShops
Jwt__Audience=ToyShops.Admin
Jwt__SigningKey=replace-with-at-least-32-random-characters
Jwt__AccessTokenMinutes=15
RefreshTokens__Days=14
Storage__LocalRoot=wwwroot/uploads
Storage__PublicBasePath=/uploads
```

- [ ] **Step 3: Update development appsettings**

Set local development config in `src/Ecommerce.Api/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=toyshops;Username=toyshops;Password=toyshops"
  },
  "Jwt": {
    "Issuer": "ToyShops",
    "Audience": "ToyShops.Admin",
    "SigningKey": "development-only-signing-key-change-before-production",
    "AccessTokenMinutes": 15
  },
  "RefreshTokens": {
    "Days": 14
  },
  "Storage": {
    "LocalRoot": "wwwroot/uploads",
    "PublicBasePath": "/uploads"
  }
}
```

- [ ] **Step 4: Verify compose starts**

Run:

```powershell
docker compose up -d
docker compose ps
```

Expected: `toyshops-postgres` is healthy.

---

### Task 2: Add Identity Entities And EF Model

**Files:**
- Modify: `src/Ecommerce.Infrastructure/Ecommerce.Infrastructure.csproj`
- Create: `src/Ecommerce.Infrastructure/Identity/ApplicationUser.cs`
- Create: `src/Ecommerce.Infrastructure/Identity/ApplicationRole.cs`
- Create: `src/Ecommerce.Infrastructure/Identity/RefreshToken.cs`
- Modify: `src/Ecommerce.Infrastructure/Persistence/ApplicationDbContext.cs`
- Test: `dotnet build ToyShops.sln`

- [ ] **Step 1: Add Identity EF package**

Run:

```powershell
dotnet add src\Ecommerce.Infrastructure\Ecommerce.Infrastructure.csproj package Microsoft.AspNetCore.Identity.EntityFrameworkCore --version 9.0.0
```

- [ ] **Step 2: Create Identity user and role types**

Create `ApplicationUser.cs`:

```csharp
using Microsoft.AspNetCore.Identity;

namespace Ecommerce.Infrastructure.Identity;

public sealed class ApplicationUser : IdentityUser<Guid>
{
}
```

Create `ApplicationRole.cs`:

```csharp
using Microsoft.AspNetCore.Identity;

namespace Ecommerce.Infrastructure.Identity;

public sealed class ApplicationRole : IdentityRole<Guid>
{
}
```

- [ ] **Step 3: Create refresh token entity**

Create `RefreshToken.cs`:

```csharp
namespace Ecommerce.Infrastructure.Identity;

public sealed class RefreshToken
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTimeOffset ExpiresAtUtc { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? RevokedAtUtc { get; private set; }
    public Guid? ReplacedByTokenId { get; private set; }

    public bool IsActive(DateTimeOffset nowUtc) => RevokedAtUtc is null && ExpiresAtUtc > nowUtc;

    public static RefreshToken Create(Guid userId, string tokenHash, DateTimeOffset expiresAtUtc)
    {
        return new RefreshToken
        {
            UserId = userId,
            TokenHash = tokenHash,
            ExpiresAtUtc = expiresAtUtc
        };
    }

    public void Revoke(Guid? replacedByTokenId = null)
    {
        RevokedAtUtc = DateTimeOffset.UtcNow;
        ReplacedByTokenId = replacedByTokenId;
    }
}
```

- [ ] **Step 4: Update ApplicationDbContext**

Change `ApplicationDbContext` to inherit from Identity:

```csharp
using Ecommerce.Application.Common.Interfaces;
using Ecommerce.Domain.Entities;
using Ecommerce.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Studio> Studios => Set<Studio>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<ProductSpecification> ProductSpecifications => Set<ProductSpecification>();
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        modelBuilder.Entity<RefreshToken>(builder =>
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.TokenHash).HasMaxLength(128).IsRequired();
            builder.HasIndex(x => x.TokenHash).IsUnique();
            builder.HasIndex(x => x.UserId);
        });
    }
}
```

- [ ] **Step 5: Build**

Run:

```powershell
dotnet build ToyShops.sln
```

Expected: build succeeds.

---

### Task 3: Configure Identity, JWT, And Admin Policy

**Files:**
- Create: `src/Ecommerce.Infrastructure/Identity/JwtOptions.cs`
- Create: `src/Ecommerce.Infrastructure/Identity/RefreshTokenOptions.cs`
- Modify: `src/Ecommerce.Infrastructure/DependencyInjection.cs`
- Modify: `src/Ecommerce.Api/Program.cs`
- Test: API build

- [ ] **Step 1: Add options types**

Create `JwtOptions.cs`:

```csharp
namespace Ecommerce.Infrastructure.Identity;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public string SigningKey { get; init; } = string.Empty;
    public int AccessTokenMinutes { get; init; } = 15;
}
```

Create `RefreshTokenOptions.cs`:

```csharp
namespace Ecommerce.Infrastructure.Identity;

public sealed class RefreshTokenOptions
{
    public const string SectionName = "RefreshTokens";
    public int Days { get; init; } = 14;
}
```

- [ ] **Step 2: Register Identity in Infrastructure**

Update `DependencyInjection.AddInfrastructure` to include:

```csharp
services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 8;
        options.Lockout.MaxFailedAccessAttempts = 5;
    })
    .AddRoles<ApplicationRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
services.Configure<RefreshTokenOptions>(configuration.GetSection(RefreshTokenOptions.SectionName));
```

- [ ] **Step 3: Replace Supabase JWT auth in Program.cs**

Configure JWT bearer using local `JwtOptions`:

```csharp
var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException("Jwt configuration is required.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
```

Keep the existing `AdminOnly` policy checking `role`, `app_role`, or `user_role` equals `admin`.

- [ ] **Step 4: Build API**

Run:

```powershell
dotnet build src\Ecommerce.Api\Ecommerce.Api.csproj
```

Expected: build succeeds.

---

### Task 4: Implement Token Services

**Files:**
- Create: `src/Ecommerce.Infrastructure/Identity/IJwtTokenService.cs`
- Create: `src/Ecommerce.Infrastructure/Identity/JwtTokenService.cs`
- Create: `src/Ecommerce.Infrastructure/Identity/IRefreshTokenService.cs`
- Create: `src/Ecommerce.Infrastructure/Identity/RefreshTokenService.cs`
- Modify: `src/Ecommerce.Infrastructure/DependencyInjection.cs`
- Test: `tests/Ecommerce.Api.Tests/AuthEndpointsTests.cs`

- [ ] **Step 1: Add failing integration test for login JWT claims**

Create `tests/Ecommerce.Api.Tests/AuthEndpointsTests.cs` with a test that posts valid admin credentials, expects `200 OK`, expects an `accessToken`, and decodes the JWT to assert `user_role=admin`.

- [ ] **Step 2: Add JWT token service**

Create `IJwtTokenService.cs`:

```csharp
using System.Security.Claims;

namespace Ecommerce.Infrastructure.Identity;

public interface IJwtTokenService
{
    string CreateAccessToken(ApplicationUser user, IReadOnlyCollection<string> roles);
}
```

Create `JwtTokenService.cs` using `JwtSecurityTokenHandler`, `SigningCredentials`, and claims:

```csharp
new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
new Claim("user_role", role)
```

- [ ] **Step 3: Add refresh token service**

Create `IRefreshTokenService.cs` with methods:

```csharp
Task<(string RawToken, RefreshToken Entity)> CreateAsync(Guid userId, CancellationToken cancellationToken);
Task<RefreshToken?> FindActiveAsync(string rawToken, CancellationToken cancellationToken);
Task RevokeAsync(RefreshToken token, Guid? replacedByTokenId, CancellationToken cancellationToken);
```

Create `RefreshTokenService.cs` that:

- generates 32+ random bytes via `RandomNumberGenerator`
- stores SHA-256 hash, not raw token
- uses `RefreshTokenOptions.Days` for expiry

- [ ] **Step 4: Register services**

Add:

```csharp
services.AddScoped<IJwtTokenService, JwtTokenService>();
services.AddScoped<IRefreshTokenService, RefreshTokenService>();
```

- [ ] **Step 5: Run failing test**

Run:

```powershell
dotnet test tests\Ecommerce.Api.Tests\Ecommerce.Api.Tests.csproj --filter AuthEndpointsTests
```

Expected: fails until `AuthController` is implemented.

---

### Task 5: Implement Auth API Endpoints

**Files:**
- Create: `src/Ecommerce.Api/Auth/AuthContracts.cs`
- Create: `src/Ecommerce.Api/Controllers/AuthController.cs`
- Test: `tests/Ecommerce.Api.Tests/AuthEndpointsTests.cs`

- [ ] **Step 1: Create auth contracts**

Create:

```csharp
namespace Ecommerce.Api.Auth;

public sealed record LoginRequest(string Email, string Password);
public sealed record AuthUserResponse(Guid Id, string Email, IReadOnlyList<string> Roles);
public sealed record AuthResponse(string AccessToken, DateTimeOffset ExpiresAtUtc, AuthUserResponse User);
```

- [ ] **Step 2: Implement login endpoint**

`POST /api/auth/login` should:

- find user by email using `UserManager<ApplicationUser>`
- validate password
- get roles
- issue access token
- create refresh token
- set refresh cookie
- return `AuthResponse`

Cookie settings:

```csharp
new CookieOptions
{
    HttpOnly = true,
    Secure = !Environment.IsDevelopment(),
    SameSite = SameSiteMode.Lax,
    Path = "/api/auth",
    Expires = refreshToken.ExpiresAtUtc
}
```

- [ ] **Step 3: Implement refresh endpoint**

`POST /api/auth/refresh` should:

- read refresh cookie
- find active token hash
- load user and roles
- create replacement refresh token
- revoke old token with replacement id
- set new refresh cookie
- return new access token

- [ ] **Step 4: Implement logout endpoint**

`POST /api/auth/logout` should:

- revoke current refresh token if present
- delete refresh cookie
- return `204 NoContent`

- [ ] **Step 5: Implement me endpoint**

`GET /api/auth/me` should require auth and return current user id, email, and roles.

- [ ] **Step 6: Run auth tests**

Run:

```powershell
dotnet test tests\Ecommerce.Api.Tests\Ecommerce.Api.Tests.csproj --filter AuthEndpointsTests
```

Expected: auth tests pass against PostgreSQL Testcontainers.

---

### Task 6: Seed Development Admin User

**Files:**
- Create: `src/Ecommerce.Infrastructure/Persistence/Seed/IdentitySeed.cs`
- Modify: `src/Ecommerce.Api/Program.cs`
- Test: run API and login manually or through integration test

- [ ] **Step 1: Add development identity seed**

Create `IdentitySeed.SeedDevelopmentAdminAsync` that:

- creates role `admin` if missing
- creates `admin@example.com` if missing
- sets password `Admin123!`
- adds the user to `admin`

- [ ] **Step 2: Call identity seed in development**

In `Program.cs` development block, call the seed after schema creation:

```csharp
await IdentitySeed.SeedDevelopmentAdminAsync(scope.ServiceProvider);
```

- [ ] **Step 3: Verify manually**

Run:

```powershell
docker compose up -d
dotnet run --project src\Ecommerce.Api\Ecommerce.Api.csproj
```

Then login:

```powershell
Invoke-RestMethod -Method Post -Uri http://localhost:5000/api/auth/login -ContentType application/json -Body '{"email":"admin@example.com","password":"Admin123!"}'
```

Expected: response includes access token.

---

### Task 7: Replace Supabase Storage With Local File Storage

**Files:**
- Create: `src/Ecommerce.Infrastructure/Storage/LocalStorageOptions.cs`
- Create: `src/Ecommerce.Infrastructure/Storage/LocalFileStorageService.cs`
- Modify: `src/Ecommerce.Infrastructure/DependencyInjection.cs`
- Modify: `src/Ecommerce.Api/Program.cs`
- Test: existing product image API tests plus new local storage test

- [ ] **Step 1: Add local storage options**

Create:

```csharp
namespace Ecommerce.Infrastructure.Storage;

public sealed class LocalStorageOptions
{
    public string LocalRoot { get; init; } = "wwwroot/uploads";
    public string PublicBasePath { get; init; } = "/uploads";
}
```

- [ ] **Step 2: Implement local file storage**

`LocalFileStorageService.UploadAsync` should:

- normalize and reject unsafe paths containing `..`
- create directories under `wwwroot/uploads`
- write stream to disk
- return public path: `/uploads/{objectPath}`

`DeleteAsync` should:

- map public path or object path back to disk
- delete if exists

- [ ] **Step 3: Register local storage**

Register local storage:

```csharp
services.AddScoped<IStorageService, LocalFileStorageService>();
```

Use local file storage for this implementation; remove the Supabase storage implementation.

- [ ] **Step 4: Serve static files**

In `Program.cs`, add:

```csharp
app.UseStaticFiles();
```

Place it before auth middleware.

- [ ] **Step 5: Run product image API tests**

Run:

```powershell
dotnet test tests\Ecommerce.Api.Tests\Ecommerce.Api.Tests.csproj --filter UploadProductImage
```

Expected: tests pass.

---

### Task 8: Replace Angular Supabase Auth With API Auth

**Files:**
- Create/rename: `apps/admin/src/app/core/services/auth.service.ts`
- Modify: `apps/admin/src/app/core/interceptors/auth-token.interceptor.ts`
- Modify: `apps/admin/src/app/core/guards/admin-auth.guard.ts`
- Modify: `apps/admin/src/app/features/auth/login.page.ts`
- Modify: `apps/admin/src/app/features/auth/login.page.spec.ts`
- Test: Angular auth unit tests

- [ ] **Step 1: Write failing tests for memory-only auth**

Extend auth service specs to assert:

```typescript
expect(localStorage.getItem).not.toHaveBeenCalled();
expect(sessionStorage.getItem).not.toHaveBeenCalled();
```

and verify login sends credentials:

```typescript
expect(http.post).toHaveBeenCalledWith('/api/auth/login', jasmine.any(Object), { withCredentials: true });
```

- [ ] **Step 2: Implement API auth service**

Use signals:

```typescript
readonly accessToken = signal('');
readonly userEmail = signal('');
readonly isAuthenticated = signal(false);
```

Methods:

- `signIn(email, password)`
- `refreshSession()`
- `signOut()`
- `clearSession()`

Do not use `localStorage` or `sessionStorage`.

- [ ] **Step 3: Update interceptor**

Behavior:

- attach `Authorization: Bearer <accessToken>` when present
- send `withCredentials: true` for `/api/auth/*`
- on `401`, call `refreshSession()` once and retry original request once
- if refresh fails, clear session

- [ ] **Step 4: Update guard**

Guard should:

- allow if already authenticated
- otherwise call refresh once
- redirect to `/login` if refresh fails

- [ ] **Step 5: Run Angular tests**

Run:

```powershell
npm.cmd --workspace apps/admin test -- --watch=false
```

Expected: admin Angular tests pass.

---

### Task 9: Add PostgreSQL-Backed Auth Integration Tests

**Files:**
- Modify: `tests/Ecommerce.Api.Tests/TestApiFactory.cs`
- Create/modify: `tests/Ecommerce.Api.Tests/AuthEndpointsTests.cs`
- Modify: `tests/Ecommerce.Api.Tests/AdminCatalogEndpointsTests.cs`

- [ ] **Step 1: Seed Identity users in TestApiFactory**

Seed:

- `admin@example.com` with role `admin`
- `customer@example.com` with role `customer` or no admin role

Use real `UserManager` and `RoleManager`.

- [ ] **Step 2: Add login tests**

Test:

- valid admin credentials return access token
- response sets refresh cookie with `HttpOnly`
- invalid credentials return `401`

- [ ] **Step 3: Add refresh tests**

Test:

- refresh returns a new access token
- refresh rotates the DB token
- reused old refresh token is rejected
- revoked refresh token is rejected

- [ ] **Step 4: Add logout tests**

Test:

- logout revokes current refresh token in PostgreSQL
- logout clears refresh cookie
- subsequent refresh fails

- [ ] **Step 5: Add admin authorization tests**

Test against a protected endpoint:

- no token returns `401`
- non-admin token returns `403`
- admin token returns success

- [ ] **Step 6: Run API integration tests**

Run:

```powershell
dotnet test tests\Ecommerce.Api.Tests\Ecommerce.Api.Tests.csproj
```

Expected: all API integration tests pass against PostgreSQL Testcontainers.

---

### Task 10: Remove Supabase Local Development Surface

**Files:**
- Modify: `README.md`
- Modify: `apps/admin/public/runtime-config.js`
- Remove or deprecate: `src/Ecommerce.Infrastructure/Auth/SupabaseJwtOptions.cs`
- Remove legacy remote storage implementation files
- Modify: package references if Supabase JS is no longer used

- [ ] **Step 1: Update README local setup**

Document:

```powershell
docker compose up -d
dotnet run --project src\Ecommerce.Api\Ecommerce.Api.csproj
npm.cmd --workspace apps/admin start
```

Document seeded admin:

```text
admin@example.com
Admin123!
```

- [ ] **Step 2: Remove frontend Supabase runtime dependency**

Remove `__supabaseUrl` and `__supabaseAnonKey` requirements from admin runtime config.

- [ ] **Step 3: Remove unused Supabase package if admin no longer imports it**

Run:

```powershell
npm.cmd --workspace apps/admin uninstall @supabase/supabase-js
```

Only do this after all imports are removed.

- [ ] **Step 4: Final verification**

Run:

```powershell
dotnet test ToyShops.sln
npm.cmd --workspace apps/admin test -- --watch=false
```

Expected: all backend and admin frontend tests pass.

---

## Security Requirements

- Refresh tokens are never returned to Angular JavaScript.
- Refresh tokens are stored only in `HttpOnly` cookies.
- Refresh tokens are stored in the database as hashes only.
- Refresh tokens rotate on every refresh.
- Reused refresh tokens are rejected.
- Access tokens are short-lived and stored only in Angular memory.
- Angular must not use `localStorage` or `sessionStorage` for auth tokens.
- Admin authorization depends on server-issued role claims, not client-side route checks.
- Local static file storage must reject path traversal attempts.

## Verification Checklist

- `docker compose up -d` starts local PostgreSQL.
- API starts against local PostgreSQL.
- Development admin can log in.
- JWT contains admin role claim.
- Admin endpoints reject unauthenticated and non-admin users.
- Refresh flow survives browser reload without `localStorage`.
- Logout revokes refresh token.
- Product image uploads write to local static folder.
- API integration tests run against PostgreSQL Testcontainers.
- Angular auth tests prove no local/session storage token persistence.

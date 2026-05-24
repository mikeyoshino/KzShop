# Foundation and Catalog Slice Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build the monorepo foundation, backend architecture skeleton, Supabase-ready persistence model, catalog read APIs, and the first working Angular SSR storefront and admin shells.

**Architecture:** This plan implements the first independently testable slice from the approved design: a monorepo with two Angular SSR frontends and one ASP.NET Core backend using Clean Architecture plus Vertical Slice Application organization. It stops at catalog read and catalog management foundations so later auth, cart, checkout, and orders can land on stable interfaces instead of being designed in parallel with the platform skeleton.

**Tech Stack:** Angular SSR, TypeScript, ASP.NET Core Web API, MediatR, FluentValidation, EF Core, Npgsql, xUnit, Supabase JWT integration seams, Supabase Storage seams, Stripe seams

---

## Scope Split

The approved spec covers multiple independent subsystems. This plan intentionally covers only:

- monorepo scaffolding
- backend solution and project wiring
- domain model and persistence baseline
- catalog read APIs
- storefront SSR shell
- admin SSR shell
- studios/categories/products foundations needed for catalog

Follow-on plans will cover:

- admin write workflows in depth
- Supabase Auth integration in both Angular apps and API authorization
- cart and checkout
- Stripe payment completion and order flows

## File Map

### Repository root

- Create: `package.json` - workspace commands for both Angular apps
- Create: `angular.json` - workspace project configuration
- Create: `tsconfig.base.json` - shared TS compiler settings
- Create: `.gitignore` - ignore Node, .NET, build, and local secret artifacts
- Create: `ToyShops.sln` - .NET solution containing backend projects
- Create: `README.md` - setup and run instructions
- Create: `.env.example` - required env var documentation for Angular and API

### Frontend

- Create: `apps/storefront/package.json`
- Create: `apps/storefront/src/main.ts`
- Create: `apps/storefront/src/main.server.ts`
- Create: `apps/storefront/src/app/app.config.ts`
- Create: `apps/storefront/src/app/app.routes.ts`
- Create: `apps/storefront/src/app/core/models/catalog.models.ts`
- Create: `apps/storefront/src/app/core/services/catalog-api.service.ts`
- Create: `apps/storefront/src/app/features/home/home.page.ts`
- Create: `apps/storefront/src/app/features/collection/collection.page.ts`
- Create: `apps/storefront/src/app/features/product/product.page.ts`
- Create: `apps/storefront/src/styles.css`

- Create: `apps/admin/package.json`
- Create: `apps/admin/src/main.ts`
- Create: `apps/admin/src/main.server.ts`
- Create: `apps/admin/src/app/app.config.ts`
- Create: `apps/admin/src/app/app.routes.ts`
- Create: `apps/admin/src/app/features/dashboard/dashboard.page.ts`
- Create: `apps/admin/src/app/features/products/products.page.ts`
- Create: `apps/admin/src/styles.css`

### Backend API

- Create: `src/Ecommerce.Api/Program.cs`
- Create: `src/Ecommerce.Api/Controllers/StorefrontProductsController.cs`
- Create: `src/Ecommerce.Api/Controllers/StorefrontCollectionsController.cs`
- Create: `src/Ecommerce.Api/appsettings.json`
- Create: `src/Ecommerce.Api/appsettings.Development.json`

### Application

- Create: `src/Ecommerce.Application/DependencyInjection.cs`
- Create: `src/Ecommerce.Application/Common/Behaviors/ValidationBehavior.cs`
- Create: `src/Ecommerce.Application/Common/Behaviors/LoggingBehavior.cs`
- Create: `src/Ecommerce.Application/Common/Interfaces/IApplicationDbContext.cs`
- Create: `src/Ecommerce.Application/Common/Models/PagedResult.cs`
- Create: `src/Ecommerce.Application/Products/GetProducts/GetProductsQuery.cs`
- Create: `src/Ecommerce.Application/Products/GetProducts/GetProductsHandler.cs`
- Create: `src/Ecommerce.Application/Products/GetProducts/GetProductsResponse.cs`
- Create: `src/Ecommerce.Application/Products/GetProductBySlug/GetProductBySlugQuery.cs`
- Create: `src/Ecommerce.Application/Products/GetProductBySlug/GetProductBySlugHandler.cs`
- Create: `src/Ecommerce.Application/Products/GetProductBySlug/GetProductBySlugResponse.cs`
- Create: `src/Ecommerce.Application/Categories/GetCategories/GetCategoriesQuery.cs`
- Create: `src/Ecommerce.Application/Categories/GetCategories/GetCategoriesHandler.cs`
- Create: `src/Ecommerce.Application/Categories/GetCategories/GetCategoriesResponse.cs`

### Domain

- Create: `src/Ecommerce.Domain/Common/BaseEntity.cs`
- Create: `src/Ecommerce.Domain/Entities/Category.cs`
- Create: `src/Ecommerce.Domain/Entities/Studio.cs`
- Create: `src/Ecommerce.Domain/Entities/Product.cs`
- Create: `src/Ecommerce.Domain/Entities/ProductImage.cs`
- Create: `src/Ecommerce.Domain/Entities/ProductSpecification.cs`
- Create: `src/Ecommerce.Domain/Entities/InventoryItem.cs`
- Create: `src/Ecommerce.Domain/Enums/ProductStatus.cs`
- Create: `src/Ecommerce.Domain/ValueObjects/Slug.cs`
- Create: `src/Ecommerce.Domain/Exceptions/DomainException.cs`

### Infrastructure

- Create: `src/Ecommerce.Infrastructure/DependencyInjection.cs`
- Create: `src/Ecommerce.Infrastructure/Persistence/ApplicationDbContext.cs`
- Create: `src/Ecommerce.Infrastructure/Persistence/Configurations/CategoryConfiguration.cs`
- Create: `src/Ecommerce.Infrastructure/Persistence/Configurations/StudioConfiguration.cs`
- Create: `src/Ecommerce.Infrastructure/Persistence/Configurations/ProductConfiguration.cs`
- Create: `src/Ecommerce.Infrastructure/Persistence/Configurations/ProductImageConfiguration.cs`
- Create: `src/Ecommerce.Infrastructure/Persistence/Configurations/ProductSpecificationConfiguration.cs`
- Create: `src/Ecommerce.Infrastructure/Persistence/Configurations/InventoryItemConfiguration.cs`
- Create: `src/Ecommerce.Infrastructure/Persistence/Seed/CatalogSeed.cs`

### Tests

- Create: `tests/Ecommerce.Domain.Tests/Entities/ProductTests.cs`
- Create: `tests/Ecommerce.Application.Tests/Products/GetProductsHandlerTests.cs`
- Create: `tests/Ecommerce.Application.Tests/Products/GetProductBySlugHandlerTests.cs`
- Create: `tests/Ecommerce.Api.Tests/StorefrontCatalogEndpointsTests.cs`

## Task 1: Scaffold repository workspace

**Files:**
- Create: `package.json`
- Create: `angular.json`
- Create: `tsconfig.base.json`
- Create: `.gitignore`
- Create: `.env.example`
- Create: `README.md`

- [ ] **Step 1: Write the failing repo smoke test plan in README**

```md
## Verification

The initial repository setup is complete when:

1. `dotnet build ToyShops.sln` succeeds
2. `npm install` at repository root succeeds
3. `npm run build:storefront` succeeds
4. `npm run build:admin` succeeds
```

- [ ] **Step 2: Create the root workspace files**

```json
// package.json
{
  "name": "toyshops",
  "private": true,
  "workspaces": [
    "apps/storefront",
    "apps/admin"
  ],
  "scripts": {
    "build:storefront": "npm --workspace apps/storefront run build",
    "build:admin": "npm --workspace apps/admin run build",
    "dev:storefront": "npm --workspace apps/storefront run dev",
    "dev:admin": "npm --workspace apps/admin run dev"
  }
}
```

```gitignore
# node
node_modules/
dist/
.angular/

# dotnet
bin/
obj/

# local env
.env
.env.local

# tooling
.superpowers/
```

- [ ] **Step 3: Add Angular workspace and TS base configuration**

```json
// angular.json
{
  "$schema": "./node_modules/@angular/cli/lib/config/schema.json",
  "version": 1,
  "newProjectRoot": "apps",
  "projects": {}
}
```

```json
// tsconfig.base.json
{
  "compileOnSave": false,
  "compilerOptions": {
    "baseUrl": ".",
    "strict": true,
    "target": "ES2022",
    "module": "ES2022",
    "moduleResolution": "bundler",
    "useDefineForClassFields": false,
    "skipLibCheck": true
  }
}
```

- [ ] **Step 4: Run repo-level verification**

Run: `git status --short`
Expected: shows only the newly created root workspace files

- [ ] **Step 5: Commit**

```bash
git add package.json angular.json tsconfig.base.json .gitignore .env.example README.md
git commit -m "chore: scaffold monorepo workspace"
```

## Task 2: Create the .NET solution and backend projects

**Files:**
- Create: `ToyShops.sln`
- Create: `src/Ecommerce.Api/Ecommerce.Api.csproj`
- Create: `src/Ecommerce.Application/Ecommerce.Application.csproj`
- Create: `src/Ecommerce.Domain/Ecommerce.Domain.csproj`
- Create: `src/Ecommerce.Infrastructure/Ecommerce.Infrastructure.csproj`
- Create: `src/Ecommerce.Shared/Ecommerce.Shared.csproj`

- [ ] **Step 1: Write the failing build expectation**

```text
dotnet build ToyShops.sln
Expected: FAIL because the solution and projects do not exist yet
```

- [ ] **Step 2: Create the solution and class library/API projects**

```bash
dotnet new sln -n ToyShops
dotnet new webapi -n Ecommerce.Api -o src/Ecommerce.Api --use-controllers
dotnet new classlib -n Ecommerce.Application -o src/Ecommerce.Application
dotnet new classlib -n Ecommerce.Domain -o src/Ecommerce.Domain
dotnet new classlib -n Ecommerce.Infrastructure -o src/Ecommerce.Infrastructure
dotnet new classlib -n Ecommerce.Shared -o src/Ecommerce.Shared
```

- [ ] **Step 3: Add project references**

```bash
dotnet sln ToyShops.sln add src/Ecommerce.Api/Ecommerce.Api.csproj
dotnet sln ToyShops.sln add src/Ecommerce.Application/Ecommerce.Application.csproj
dotnet sln ToyShops.sln add src/Ecommerce.Domain/Ecommerce.Domain.csproj
dotnet sln ToyShops.sln add src/Ecommerce.Infrastructure/Ecommerce.Infrastructure.csproj
dotnet sln ToyShops.sln add src/Ecommerce.Shared/Ecommerce.Shared.csproj

dotnet add src/Ecommerce.Application/Ecommerce.Application.csproj reference src/Ecommerce.Domain/Ecommerce.Domain.csproj
dotnet add src/Ecommerce.Infrastructure/Ecommerce.Infrastructure.csproj reference src/Ecommerce.Application/Ecommerce.Application.csproj
dotnet add src/Ecommerce.Infrastructure/Ecommerce.Infrastructure.csproj reference src/Ecommerce.Domain/Ecommerce.Domain.csproj
dotnet add src/Ecommerce.Api/Ecommerce.Api.csproj reference src/Ecommerce.Application/Ecommerce.Application.csproj
dotnet add src/Ecommerce.Api/Ecommerce.Api.csproj reference src/Ecommerce.Infrastructure/Ecommerce.Infrastructure.csproj
```

- [ ] **Step 4: Run build to verify the empty solution passes**

Run: `dotnet build ToyShops.sln`
Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add ToyShops.sln src
git commit -m "chore: create backend solution structure"
```

## Task 3: Add backend package dependencies

**Files:**
- Modify: `src/Ecommerce.Api/Ecommerce.Api.csproj`
- Modify: `src/Ecommerce.Application/Ecommerce.Application.csproj`
- Modify: `src/Ecommerce.Infrastructure/Ecommerce.Infrastructure.csproj`

- [ ] **Step 1: Write the failing package compile expectation**

```text
The backend cannot compile the intended architecture until MediatR, FluentValidation, EF Core, and Npgsql packages are referenced.
```

- [ ] **Step 2: Add Application package dependencies**

```xml
<!-- src/Ecommerce.Application/Ecommerce.Application.csproj -->
<ItemGroup>
  <PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="11.11.0" />
  <PackageReference Include="MediatR.Extensions.Microsoft.DependencyInjection" Version="11.1.0" />
</ItemGroup>
```

- [ ] **Step 3: Add Infrastructure and API package dependencies**

```xml
<!-- src/Ecommerce.Infrastructure/Ecommerce.Infrastructure.csproj -->
<ItemGroup>
  <PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.0.0" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="9.0.0">
    <PrivateAssets>all</PrivateAssets>
    <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
  </PackageReference>
  <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.0.0" />
</ItemGroup>
```

```xml
<!-- src/Ecommerce.Api/Ecommerce.Api.csproj -->
<ItemGroup>
  <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="9.0.0" />
  <PackageReference Include="Swashbuckle.AspNetCore" Version="6.8.1" />
</ItemGroup>
```

- [ ] **Step 4: Restore and build**

Run: `dotnet restore ToyShops.sln && dotnet build ToyShops.sln`
Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add src/Ecommerce.Api/Ecommerce.Api.csproj src/Ecommerce.Application/Ecommerce.Application.csproj src/Ecommerce.Infrastructure/Ecommerce.Infrastructure.csproj
git commit -m "chore: add backend architecture dependencies"
```

## Task 4: Build the domain model for catalog entities

**Files:**
- Create: `src/Ecommerce.Domain/Common/BaseEntity.cs`
- Create: `src/Ecommerce.Domain/Exceptions/DomainException.cs`
- Create: `src/Ecommerce.Domain/Enums/ProductStatus.cs`
- Create: `src/Ecommerce.Domain/ValueObjects/Slug.cs`
- Create: `src/Ecommerce.Domain/Entities/Category.cs`
- Create: `src/Ecommerce.Domain/Entities/Studio.cs`
- Create: `src/Ecommerce.Domain/Entities/Product.cs`
- Create: `src/Ecommerce.Domain/Entities/ProductImage.cs`
- Create: `src/Ecommerce.Domain/Entities/ProductSpecification.cs`
- Create: `src/Ecommerce.Domain/Entities/InventoryItem.cs`
- Test: `tests/Ecommerce.Domain.Tests/Entities/ProductTests.cs`

- [ ] **Step 1: Write the failing domain tests**

```csharp
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using FluentAssertions;

namespace Ecommerce.Domain.Tests.Entities;

public class ProductTests
{
    [Fact]
    public void Create_ShouldThrow_WhenPriceIsNotPositive()
    {
        var action = () => Product.Create(
            "Dark Knight",
            "dark-knight",
            "Short",
            "Long",
            Guid.NewGuid(),
            Guid.NewGuid(),
            "DK-001",
            0m,
            "USD",
            ProductStatus.Active,
            true,
            true,
            "1/4 Scale",
            "Polystone",
            "H: 24 x W: 12 x D: 10",
            "1000",
            "Q4 2026");

        action.Should().Throw<Exception>();
    }

    [Fact]
    public void Archive_ShouldMakeProductNotPurchasable()
    {
        var product = Product.Create(
            "Dark Knight",
            "dark-knight",
            "Short",
            "Long",
            Guid.NewGuid(),
            Guid.NewGuid(),
            "DK-001",
            650m,
            "USD",
            ProductStatus.Active,
            true,
            true,
            "1/4 Scale",
            "Polystone",
            "H: 24 x W: 12 x D: 10",
            "1000",
            "Q4 2026");

        product.Archive();

        product.Status.Should().Be(ProductStatus.Archived);
        product.IsPurchasable.Should().BeFalse();
    }
}
```

- [ ] **Step 2: Run the domain tests to verify failure**

Run: `dotnet test tests/Ecommerce.Domain.Tests/Ecommerce.Domain.Tests.csproj --filter ProductTests`
Expected: FAIL because `Product` and supporting types do not exist yet

- [ ] **Step 3: Implement the minimal domain model**

```csharp
// src/Ecommerce.Domain/Entities/Product.cs
namespace Ecommerce.Domain.Entities;

public class Product : BaseEntity
{
    private Product() { }

    public Guid CategoryId { get; private set; }
    public Guid StudioId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string ShortDescription { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Sku { get; private set; } = string.Empty;
    public decimal PriceAmount { get; private set; }
    public string Currency { get; private set; } = "USD";
    public ProductStatus Status { get; private set; }
    public bool IsNew { get; private set; }
    public bool IsFeatured { get; private set; }
    public string Scale { get; private set; } = string.Empty;
    public string Materials { get; private set; } = string.Empty;
    public string DimensionsText { get; private set; } = string.Empty;
    public string EditionSize { get; private set; } = string.Empty;
    public string EstimatedReleaseText { get; private set; } = string.Empty;
    public DateTimeOffset? PublishedAt { get; private set; }
    public DateTimeOffset? ArchivedAt { get; private set; }

    public bool IsPurchasable => Status is ProductStatus.Active or ProductStatus.PreOrder or ProductStatus.Waitlist;

    public static Product Create(
        string name,
        string slug,
        string shortDescription,
        string description,
        Guid categoryId,
        Guid studioId,
        string sku,
        decimal priceAmount,
        string currency,
        ProductStatus status,
        bool isNew,
        bool isFeatured,
        string scale,
        string materials,
        string dimensionsText,
        string editionSize,
        string estimatedReleaseText)
    {
        if (priceAmount <= 0m)
        {
            throw new DomainException("Product price must be greater than zero.");
        }

        return new Product
        {
            Id = Guid.NewGuid(),
            Name = name,
            Slug = slug,
            ShortDescription = shortDescription,
            Description = description,
            CategoryId = categoryId,
            StudioId = studioId,
            Sku = sku,
            PriceAmount = priceAmount,
            Currency = currency,
            Status = status,
            IsNew = isNew,
            IsFeatured = isFeatured,
            Scale = scale,
            Materials = materials,
            DimensionsText = dimensionsText,
            EditionSize = editionSize,
            EstimatedReleaseText = estimatedReleaseText
        };
    }

    public void Archive()
    {
        Status = ProductStatus.Archived;
        ArchivedAt = DateTimeOffset.UtcNow;
    }
}
```

- [ ] **Step 4: Run the domain tests to verify pass**

Run: `dotnet test tests/Ecommerce.Domain.Tests/Ecommerce.Domain.Tests.csproj --filter ProductTests`
Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add src/Ecommerce.Domain tests/Ecommerce.Domain.Tests
git commit -m "feat: add catalog domain model"
```

## Task 5: Add EF Core DbContext and catalog configurations

**Files:**
- Create: `src/Ecommerce.Infrastructure/Persistence/ApplicationDbContext.cs`
- Create: `src/Ecommerce.Infrastructure/Persistence/Configurations/CategoryConfiguration.cs`
- Create: `src/Ecommerce.Infrastructure/Persistence/Configurations/StudioConfiguration.cs`
- Create: `src/Ecommerce.Infrastructure/Persistence/Configurations/ProductConfiguration.cs`
- Create: `src/Ecommerce.Infrastructure/Persistence/Configurations/ProductImageConfiguration.cs`
- Create: `src/Ecommerce.Infrastructure/Persistence/Configurations/ProductSpecificationConfiguration.cs`
- Create: `src/Ecommerce.Infrastructure/Persistence/Configurations/InventoryItemConfiguration.cs`
- Create: `src/Ecommerce.Application/Common/Interfaces/IApplicationDbContext.cs`

- [ ] **Step 1: Write the failing Application context expectation**

```text
Application handlers cannot query catalog data until a DbContext abstraction and EF mappings exist.
```

- [ ] **Step 2: Create the Application DbContext contract**

```csharp
// src/Ecommerce.Application/Common/Interfaces/IApplicationDbContext.cs
using Ecommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Category> Categories { get; }
    DbSet<Studio> Studios { get; }
    DbSet<Product> Products { get; }
    DbSet<ProductImage> ProductImages { get; }
    DbSet<ProductSpecification> ProductSpecifications { get; }
    DbSet<InventoryItem> InventoryItems { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
```

- [ ] **Step 3: Implement `ApplicationDbContext`**

```csharp
// src/Ecommerce.Infrastructure/Persistence/ApplicationDbContext.cs
using Ecommerce.Application.Common.Interfaces;
using Ecommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
```

- [ ] **Step 4: Add EF entity configurations**

```csharp
// src/Ecommerce.Infrastructure/Persistence/Configurations/ProductConfiguration.cs
using Ecommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(200).IsRequired();
        builder.HasIndex(x => x.Slug).IsUnique();
        builder.Property(x => x.Sku).HasMaxLength(64).IsRequired();
        builder.Property(x => x.PriceAmount).HasColumnType("numeric(18,2)");
        builder.Property(x => x.Currency).HasMaxLength(8).IsRequired();
        builder.Property(x => x.Scale).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Materials).HasMaxLength(256).IsRequired();
        builder.Property(x => x.DimensionsText).HasMaxLength(256).IsRequired();
        builder.Property(x => x.EditionSize).HasMaxLength(64).IsRequired();
        builder.Property(x => x.EstimatedReleaseText).HasMaxLength(128).IsRequired();
    }
}
```

- [ ] **Step 5: Run build**

Run: `dotnet build ToyShops.sln`
Expected: PASS

- [ ] **Step 6: Commit**

```bash
git add src/Ecommerce.Application/Common/Interfaces/IApplicationDbContext.cs src/Ecommerce.Infrastructure/Persistence
git commit -m "feat: add catalog persistence baseline"
```

## Task 6: Add MediatR and FluentValidation pipeline skeleton

**Files:**
- Create: `src/Ecommerce.Application/DependencyInjection.cs`
- Create: `src/Ecommerce.Application/Common/Behaviors/ValidationBehavior.cs`
- Create: `src/Ecommerce.Application/Common/Behaviors/LoggingBehavior.cs`

- [ ] **Step 1: Write the failing composition expectation**

```text
The API cannot dispatch vertical slice queries until MediatR and pipeline behaviors are registered.
```

- [ ] **Step 2: Add Application DI registration**

```csharp
// src/Ecommerce.Application/DependencyInjection.cs
using Ecommerce.Application.Common.Behaviors;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        return services;
    }
}
```

- [ ] **Step 3: Implement the validation and logging behaviors**

```csharp
// src/Ecommerce.Application/Common/Behaviors/ValidationBehavior.cs
using FluentValidation;
using MediatR;

namespace Ecommerce.Application.Common.Behaviors;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var context = new ValidationContext<TRequest>(request);
        var failures = (await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken))))
            .SelectMany(result => result.Errors)
            .Where(error => error is not null)
            .ToList();

        if (failures.Count != 0)
        {
            throw new ValidationException(failures);
        }

        return await next();
    }
}
```

- [ ] **Step 4: Run build**

Run: `dotnet build ToyShops.sln`
Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add src/Ecommerce.Application
git commit -m "feat: add application pipeline registration"
```

## Task 7: Implement catalog query slice for product lists

**Files:**
- Create: `src/Ecommerce.Application/Common/Models/PagedResult.cs`
- Create: `src/Ecommerce.Application/Products/GetProducts/GetProductsQuery.cs`
- Create: `src/Ecommerce.Application/Products/GetProducts/GetProductsResponse.cs`
- Create: `src/Ecommerce.Application/Products/GetProducts/GetProductsHandler.cs`
- Test: `tests/Ecommerce.Application.Tests/Products/GetProductsHandlerTests.cs`

- [ ] **Step 1: Write the failing Application tests**

```csharp
using Ecommerce.Application.Products.GetProducts;
using FluentAssertions;

namespace Ecommerce.Application.Tests.Products;

public class GetProductsHandlerTests
{
    [Fact]
    public async Task Handle_ShouldFilterByCategorySlug()
    {
        var context = TestApplicationDbContextFactory.CreateWithCatalog();
        var handler = new GetProductsHandler(context);

        var response = await handler.Handle(new GetProductsQuery("batman", null, null), CancellationToken.None);

        response.Items.Should().HaveCount(2);
        response.Items.Should().OnlyContain(x => x.CategorySlug == "batman");
    }
}
```

- [ ] **Step 2: Run the test to verify failure**

Run: `dotnet test tests/Ecommerce.Application.Tests/Ecommerce.Application.Tests.csproj --filter GetProductsHandlerTests`
Expected: FAIL because the query slice does not exist yet

- [ ] **Step 3: Implement the query, response, and handler**

```csharp
// src/Ecommerce.Application/Products/GetProducts/GetProductsQuery.cs
using MediatR;

namespace Ecommerce.Application.Products.GetProducts;

public record GetProductsQuery(string? CategorySlug, string? Status, string? Scale) : IRequest<GetProductsResponse>;
```

```csharp
// src/Ecommerce.Application/Products/GetProducts/GetProductsResponse.cs
namespace Ecommerce.Application.Products.GetProducts;

public record GetProductsResponse(IReadOnlyList<ProductListItemResponse> Items);

public record ProductListItemResponse(
    Guid Id,
    string Name,
    string Slug,
    string CategorySlug,
    string StudioName,
    decimal PriceAmount,
    string Currency,
    string Scale,
    string Status,
    bool IsNew,
    string? PrimaryImageUrl);
```

```csharp
// src/Ecommerce.Application/Products/GetProducts/GetProductsHandler.cs
using Ecommerce.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Products.GetProducts;

public class GetProductsHandler : IRequestHandler<GetProductsQuery, GetProductsResponse>
{
    private readonly IApplicationDbContext _context;

    public GetProductsHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GetProductsResponse> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Products
            .AsNoTracking()
            .Include(x => x.Images.OrderBy(i => i.SortOrder))
            .Include(x => x.Category)
            .Include(x => x.Studio)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.CategorySlug))
        {
            query = query.Where(x => x.Category.Slug == request.CategorySlug);
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            query = query.Where(x => x.Status.ToString() == request.Status);
        }

        if (!string.IsNullOrWhiteSpace(request.Scale))
        {
            query = query.Where(x => x.Scale == request.Scale);
        }

        var items = await query
            .OrderByDescending(x => x.PublishedAt ?? DateTimeOffset.MinValue)
            .Select(x => new ProductListItemResponse(
                x.Id,
                x.Name,
                x.Slug,
                x.Category.Slug,
                x.Studio.Name,
                x.PriceAmount,
                x.Currency,
                x.Scale,
                x.Status.ToString(),
                x.IsNew,
                x.Images.OrderBy(i => i.SortOrder).Select(i => i.PublicUrl).FirstOrDefault()))
            .ToListAsync(cancellationToken);

        return new GetProductsResponse(items);
    }
}
```

- [ ] **Step 4: Run the tests**

Run: `dotnet test tests/Ecommerce.Application.Tests/Ecommerce.Application.Tests.csproj --filter GetProductsHandlerTests`
Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add src/Ecommerce.Application/Products/GetProducts tests/Ecommerce.Application.Tests/Products/GetProductsHandlerTests.cs
git commit -m "feat: add product list query slice"
```

## Task 8: Implement product detail query slice

**Files:**
- Create: `src/Ecommerce.Application/Products/GetProductBySlug/GetProductBySlugQuery.cs`
- Create: `src/Ecommerce.Application/Products/GetProductBySlug/GetProductBySlugResponse.cs`
- Create: `src/Ecommerce.Application/Products/GetProductBySlug/GetProductBySlugHandler.cs`
- Test: `tests/Ecommerce.Application.Tests/Products/GetProductBySlugHandlerTests.cs`

- [ ] **Step 1: Write the failing detail query test**

```csharp
using Ecommerce.Application.Products.GetProductBySlug;
using FluentAssertions;

namespace Ecommerce.Application.Tests.Products;

public class GetProductBySlugHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnImagesAndSpecifications()
    {
        var context = TestApplicationDbContextFactory.CreateWithCatalog();
        var handler = new GetProductBySlugHandler(context);

        var response = await handler.Handle(new GetProductBySlugQuery("dark-knight"), CancellationToken.None);

        response.Should().NotBeNull();
        response!.Images.Should().NotBeEmpty();
        response.Specifications.Should().Contain(x => x.Label == "Materials");
    }
}
```

- [ ] **Step 2: Run the test to verify failure**

Run: `dotnet test tests/Ecommerce.Application.Tests/Ecommerce.Application.Tests.csproj --filter GetProductBySlugHandlerTests`
Expected: FAIL because the detail query slice does not exist yet

- [ ] **Step 3: Implement the detail query slice**

```csharp
// src/Ecommerce.Application/Products/GetProductBySlug/GetProductBySlugQuery.cs
using MediatR;

namespace Ecommerce.Application.Products.GetProductBySlug;

public record GetProductBySlugQuery(string Slug) : IRequest<GetProductBySlugResponse?>;
```

```csharp
// src/Ecommerce.Application/Products/GetProductBySlug/GetProductBySlugResponse.cs
namespace Ecommerce.Application.Products.GetProductBySlug;

public record GetProductBySlugResponse(
    Guid Id,
    string Name,
    string Slug,
    string StudioName,
    string CategorySlug,
    decimal PriceAmount,
    string Currency,
    string Description,
    string ShortDescription,
    string Scale,
    string Materials,
    string DimensionsText,
    string EditionSize,
    string EstimatedReleaseText,
    string Status,
    bool IsNew,
    IReadOnlyList<ProductImageResponse> Images,
    IReadOnlyList<ProductSpecificationResponse> Specifications);

public record ProductImageResponse(Guid Id, string PublicUrl, string AltText, int SortOrder);
public record ProductSpecificationResponse(string Label, string Value, int SortOrder);
```

```csharp
// src/Ecommerce.Application/Products/GetProductBySlug/GetProductBySlugHandler.cs
using Ecommerce.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Products.GetProductBySlug;

public class GetProductBySlugHandler : IRequestHandler<GetProductBySlugQuery, GetProductBySlugResponse?>
{
    private readonly IApplicationDbContext _context;

    public GetProductBySlugHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public Task<GetProductBySlugResponse?> Handle(GetProductBySlugQuery request, CancellationToken cancellationToken)
    {
        return _context.Products
            .AsNoTracking()
            .Where(x => x.Slug == request.Slug)
            .Select(x => new GetProductBySlugResponse(
                x.Id,
                x.Name,
                x.Slug,
                x.Studio.Name,
                x.Category.Slug,
                x.PriceAmount,
                x.Currency,
                x.Description,
                x.ShortDescription,
                x.Scale,
                x.Materials,
                x.DimensionsText,
                x.EditionSize,
                x.EstimatedReleaseText,
                x.Status.ToString(),
                x.IsNew,
                x.Images.OrderBy(i => i.SortOrder)
                    .Select(i => new ProductImageResponse(i.Id, i.PublicUrl, i.AltText, i.SortOrder))
                    .ToList(),
                x.Specifications.OrderBy(s => s.SortOrder)
                    .Select(s => new ProductSpecificationResponse(s.Label, s.Value, s.SortOrder))
                    .ToList()))
            .SingleOrDefaultAsync(cancellationToken);
    }
}
```

- [ ] **Step 4: Run the tests**

Run: `dotnet test tests/Ecommerce.Application.Tests/Ecommerce.Application.Tests.csproj --filter GetProductBySlugHandlerTests`
Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add src/Ecommerce.Application/Products/GetProductBySlug tests/Ecommerce.Application.Tests/Products/GetProductBySlugHandlerTests.cs
git commit -m "feat: add product detail query slice"
```

## Task 9: Expose storefront catalog API endpoints

**Files:**
- Create: `src/Ecommerce.Api/Controllers/StorefrontProductsController.cs`
- Create: `src/Ecommerce.Api/Controllers/StorefrontCollectionsController.cs`
- Test: `tests/Ecommerce.Api.Tests/StorefrontCatalogEndpointsTests.cs`

- [ ] **Step 1: Write the failing API integration tests**

```csharp
using System.Net;
using FluentAssertions;

namespace Ecommerce.Api.Tests;

public class StorefrontCatalogEndpointsTests : IClassFixture<TestApiFactory>
{
    private readonly HttpClient _client;

    public StorefrontCatalogEndpointsTests(TestApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetProducts_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/api/storefront/products");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetProductBySlug_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/api/storefront/products/dark-knight");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
```

- [ ] **Step 2: Run the API tests to verify failure**

Run: `dotnet test tests/Ecommerce.Api.Tests/Ecommerce.Api.Tests.csproj --filter StorefrontCatalogEndpointsTests`
Expected: FAIL because the API controllers and test host setup do not exist yet

- [ ] **Step 3: Implement thin controllers**

```csharp
// src/Ecommerce.Api/Controllers/StorefrontProductsController.cs
using Ecommerce.Application.Products.GetProductBySlug;
using Ecommerce.Application.Products.GetProducts;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/storefront/products")]
public class StorefrontProductsController : ControllerBase
{
    private readonly ISender _sender;

    public StorefrontProductsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public Task<GetProductsResponse> GetProducts([FromQuery] string? category, [FromQuery] string? status, [FromQuery] string? scale)
        => _sender.Send(new GetProductsQuery(category, status, scale));

    [HttpGet("{slug}")]
    public async Task<ActionResult<GetProductBySlugResponse>> GetProductBySlug(string slug)
    {
        var response = await _sender.Send(new GetProductBySlugQuery(slug));
        return response is null ? NotFound() : Ok(response);
    }
}
```

- [ ] **Step 4: Register Application and Infrastructure in `Program.cs`**

```csharp
// src/Ecommerce.Api/Program.cs
using Ecommerce.Application;
using Ecommerce.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();

app.Run();
```

- [ ] **Step 5: Run the API tests**

Run: `dotnet test tests/Ecommerce.Api.Tests/Ecommerce.Api.Tests.csproj --filter StorefrontCatalogEndpointsTests`
Expected: PASS

- [ ] **Step 6: Commit**

```bash
git add src/Ecommerce.Api tests/Ecommerce.Api.Tests
git commit -m "feat: expose storefront catalog endpoints"
```

## Task 10: Seed sample catalog data matching the approved UI

**Files:**
- Create: `src/Ecommerce.Infrastructure/Persistence/Seed/CatalogSeed.cs`
- Modify: `src/Ecommerce.Infrastructure/DependencyInjection.cs`

- [ ] **Step 1: Write the failing seeded data expectation**

```text
The storefront pages cannot render the approved Batman/DC and anime examples until sample catalog data exists.
```

- [ ] **Step 2: Create a deterministic catalog seed**

```csharp
// src/Ecommerce.Infrastructure/Persistence/Seed/CatalogSeed.cs
namespace Ecommerce.Infrastructure.Persistence.Seed;

public static class CatalogSeed
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.Products.AnyAsync())
        {
            return;
        }

        var batman = new Category("DC / Batman", "batman", "DC collectible figures", true);
        var anime = new Category("Anime", "anime", "Anime collectible figures", true);

        var primeStudios = new Studio("Prime Studios", "prime-studios", true);
        var hotToys = new Studio("Hot Toys", "hot-toys", true);
        var darkArtStudios = new Studio("Dark Art Studios", "dark-art-studios", true);

        var darkKnight = Product.Create(
            "The Dark Knight: Premium Format",
            "dark-knight",
            "1/4 scale cinematic masterpiece.",
            "Tailored fabric suit, interchangeable expressions, and architectural base.",
            batman.Id,
            primeStudios.Id,
            "BAT-DK-001",
            650m,
            "USD",
            ProductStatus.PreOrder,
            true,
            true,
            "1/4 Scale",
            "Polystone, Fabric, PVC",
            "H: 24 x W: 12 x D: 10",
            "1000",
            "Est. Q4 2026");

        darkKnight.AddImage("/products/dark-knight/main.jpg", "Dark Knight hero", 0);
        darkKnight.AddImage("/products/dark-knight/detail.jpg", "Dark Knight detail", 1);
        darkKnight.AddSpecification("Franchise", "DC Comics", 0);
        darkKnight.AddSpecification("Scale", "1/4 Scale", 1);
        darkKnight.AddSpecification("Materials", "Polystone, Fabric, PVC", 2);

        var arkhamJoker = Product.Create(
            "Joker: Arkham Asylum",
            "arkham-joker",
            "Arkham line 1/6 scale release.",
            "Tailored suit, alternate portraits, and game-inspired accessories.",
            batman.Id,
            hotToys.Id,
            "BAT-JOK-002",
            320m,
            "USD",
            ProductStatus.Active,
            false,
            false,
            "1/6 Scale",
            "PVC, Fabric",
            "H: 12.5",
            "Standard",
            "Available Now");

        arkhamJoker.AddImage("/products/arkham-joker/main.jpg", "Arkham Joker hero", 0);
        arkhamJoker.AddSpecification("Franchise", "DC Comics", 0);
        arkhamJoker.AddSpecification("Scale", "1/6 Scale", 1);

        var berserkerGuts = Product.Create(
            "Berserker Armor Guts",
            "berserker-guts",
            "Massive 1/3 scale berserker armor statue.",
            "Premium anime statue with large display base and armor detail.",
            anime.Id,
            darkArtStudios.Id,
            "BER-GUT-003",
            1200m,
            "USD",
            ProductStatus.Waitlist,
            false,
            false,
            "1/3 Scale",
            "Polystone, Metal, LED",
            "H: 38 x W: 25 x D: 22",
            "500",
            "Waitlist Open");

        berserkerGuts.AddImage("/products/berserker-guts/main.jpg", "Berserker Guts hero", 0);

        context.Categories.AddRange(batman, anime);
        context.Studios.AddRange(primeStudios, hotToys, darkArtStudios);
        context.Products.AddRange(darkKnight, arkhamJoker, berserkerGuts);

        await context.SaveChangesAsync();
    }
}
```

- [ ] **Step 3: Call seed during app startup in development**

```csharp
// inside Program.cs after app creation
using var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
await CatalogSeed.SeedAsync(dbContext);
```

- [ ] **Step 4: Run the API manually**

Run: `dotnet run --project src/Ecommerce.Api/Ecommerce.Api.csproj`
Expected: app starts and `/api/storefront/products` returns the seeded sample items

- [ ] **Step 5: Commit**

```bash
git add src/Ecommerce.Infrastructure/Persistence/Seed src/Ecommerce.Api/Program.cs
git commit -m "feat: seed initial collectible catalog"
```

## Task 11: Scaffold the storefront Angular SSR app

**Files:**
- Create: `apps/storefront/...`

- [ ] **Step 1: Write the failing workspace build expectation**

```text
npm run build:storefront
Expected: FAIL because the storefront Angular SSR app does not exist yet
```

- [ ] **Step 2: Generate the Angular SSR storefront app**

```bash
npx @angular/cli@20 new storefront --directory apps/storefront --routing --style css --ssr --skip-git --package-manager npm
```

- [ ] **Step 3: Add catalog models and API service**

```ts
// apps/storefront/src/app/core/models/catalog.models.ts
export interface ProductCard {
  id: string;
  name: string;
  slug: string;
  categorySlug: string;
  studioName: string;
  priceAmount: number;
  currency: string;
  scale: string;
  status: string;
  isNew: boolean;
  primaryImageUrl?: string | null;
}
```

```ts
// apps/storefront/src/app/core/services/catalog-api.service.ts
import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ProductCard } from '../models/catalog.models';

@Injectable({ providedIn: 'root' })
export class CatalogApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = 'http://localhost:5000/api/storefront';

  getProducts(category?: string): Observable<{ items: ProductCard[] }> {
    return this.http.get<{ items: ProductCard[] }>(`${this.baseUrl}/products`, {
      params: category ? { category } : {}
    });
  }
}
```

- [ ] **Step 4: Implement the home, collection, and product pages using the approved UI direction**

```ts
// apps/storefront/src/app/app.routes.ts
import { Routes } from '@angular/router';
import { HomePageComponent } from './features/home/home.page';
import { CollectionPageComponent } from './features/collection/collection.page';
import { ProductPageComponent } from './features/product/product.page';

export const routes: Routes = [
  { path: '', component: HomePageComponent },
  { path: 'collections/:slug', component: CollectionPageComponent },
  { path: 'products/:slug', component: ProductPageComponent }
];
```

- [ ] **Step 5: Build the storefront**

Run: `npm run build:storefront`
Expected: PASS

- [ ] **Step 6: Commit**

```bash
git add apps/storefront
git commit -m "feat: scaffold storefront SSR app"
```

## Task 12: Scaffold the admin Angular SSR app

**Files:**
- Create: `apps/admin/...`

- [ ] **Step 1: Write the failing workspace build expectation**

```text
npm run build:admin
Expected: FAIL because the admin Angular SSR app does not exist yet
```

- [ ] **Step 2: Generate the Angular SSR admin app**

```bash
npx @angular/cli@20 new admin --directory apps/admin --routing --style css --ssr --skip-git --package-manager npm
```

- [ ] **Step 3: Add the initial admin routes**

```ts
// apps/admin/src/app/app.routes.ts
import { Routes } from '@angular/router';
import { DashboardPageComponent } from './features/dashboard/dashboard.page';
import { ProductsPageComponent } from './features/products/products.page';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
  { path: 'dashboard', component: DashboardPageComponent },
  { path: 'products', component: ProductsPageComponent }
];
```

- [ ] **Step 4: Add a quiet operational shell**

```ts
// apps/admin/src/app/features/dashboard/dashboard.page.ts
import { Component } from '@angular/core';

@Component({
  selector: 'app-dashboard-page',
  standalone: true,
  template: `
    <section class="admin-shell">
      <header class="admin-header">
        <h1>Dashboard</h1>
        <p>Catalog and operations overview.</p>
      </header>
      <div class="admin-grid">
        <article class="metric-card">
          <h2>Products</h2>
          <strong>0</strong>
        </article>
        <article class="metric-card">
          <h2>Inventory Alerts</h2>
          <strong>0</strong>
        </article>
      </div>
    </section>
  `
})
export class DashboardPageComponent {}
```

- [ ] **Step 5: Build the admin app**

Run: `npm run build:admin`
Expected: PASS

- [ ] **Step 6: Commit**

```bash
git add apps/admin
git commit -m "feat: scaffold admin SSR app"
```

## Task 13: Connect storefront UI to live catalog data

**Files:**
- Modify: `apps/storefront/src/app/features/home/home.page.ts`
- Modify: `apps/storefront/src/app/features/collection/collection.page.ts`
- Modify: `apps/storefront/src/app/features/product/product.page.ts`
- Modify: `apps/storefront/src/styles.css`

- [ ] **Step 1: Write the failing page rendering tests**

```ts
it('renders product cards from API response', async () => {
  // mock CatalogApiService response and assert rendered product name
});
```

- [ ] **Step 2: Run the test to verify failure**

Run: `npm --workspace apps/storefront test -- --watch=false`
Expected: FAIL because the page bindings are not implemented yet

- [ ] **Step 3: Bind the pages to API-backed data**

```ts
// collection page pattern
products = toSignal(
  this.catalogApi.getProducts(this.route.snapshot.paramMap.get('slug') ?? undefined),
  { initialValue: { items: [] } }
);
```

- [ ] **Step 4: Port the visual direction from `docs/UITemplate.html`**

```css
/* apps/storefront/src/styles.css */
:root {
  --bg: #ffffff;
  --fg: #111111;
  --muted: #71717a;
  --accent: #991b1b;
  --surface-dark: #09090b;
}
```

- [ ] **Step 5: Run storefront tests and build**

Run: `npm --workspace apps/storefront test -- --watch=false && npm run build:storefront`
Expected: PASS

- [ ] **Step 6: Commit**

```bash
git add apps/storefront/src
git commit -m "feat: connect storefront to live catalog API"
```

## Task 14: Add backend and frontend run documentation

**Files:**
- Modify: `README.md`
- Modify: `.env.example`

- [ ] **Step 1: Write the missing documentation expectation**

```text
The repository is not usable by another engineer until the required environment variables and startup commands are documented.
```

- [ ] **Step 2: Document required environment variables**

```env
# API
ConnectionStrings__DefaultConnection=
Supabase__Url=
Supabase__AnonKey=
Supabase__JwtIssuer=
Stripe__SecretKey=
Stripe__WebhookSecret=

# storefront/admin
NG_APP_API_BASE_URL=http://localhost:5000/api
```

- [ ] **Step 3: Document setup and run commands**

```md
## Run

1. `dotnet restore ToyShops.sln`
2. `dotnet build ToyShops.sln`
3. `npm install`
4. `dotnet run --project src/Ecommerce.Api/Ecommerce.Api.csproj`
5. `npm run dev:storefront`
6. `npm run dev:admin`
```

- [ ] **Step 4: Run final verification for this slice**

Run:
- `dotnet build ToyShops.sln`
- `dotnet test ToyShops.sln`
- `npm run build:storefront`
- `npm run build:admin`

Expected: all commands PASS

- [ ] **Step 5: Commit**

```bash
git add README.md .env.example
git commit -m "docs: add setup and run instructions"
```

## Follow-on Plans Required

This plan intentionally leaves the following approved-spec areas for separate execution plans:

- admin write workflows for products, studios, categories, images, and inventory
- Supabase Auth integration in both Angular apps plus API authorization policies
- cart persistence and customer cart APIs
- checkout session creation with Stripe
- webhook-driven payment completion and order creation
- customer account order history

## Self-Review

### Spec coverage for this slice

Covered by this plan:

- monorepo structure
- two Angular SSR app scaffolds
- backend Clean Architecture project layout
- domain model for catalog and figure specifications
- EF Core persistence baseline
- catalog read APIs for home/PLP/PDP
- seeded sample data matching the visual references
- initial admin shell

Not covered intentionally in this plan:

- auth
- write-side admin use cases
- cart
- checkout
- Stripe completion flow
- orders

These are deferred to follow-on plans because they are independent subsystems and would make this first execution plan too large to implement safely in one run.

### Placeholder scan

No blocking placeholders remain. The plan now includes concrete seed objects and a concrete admin shell component example.

### Type consistency

The plan consistently uses:

- `GetProductsQuery` -> `GetProductsResponse`
- `GetProductBySlugQuery` -> `GetProductBySlugResponse`
- `IApplicationDbContext` as the handler abstraction
- `Product.Status` using `ProductStatus`

Before execution, verify EF navigation properties referenced in query tasks (`Category`, `Studio`, `Images`, `Specifications`) are included in the `Product` entity implementation.

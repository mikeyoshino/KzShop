# Admin Dashboard Reporting Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add persisted internal order data and a real admin dashboard reporting endpoint, then replace the admin dashboard placeholder metrics with API-backed inventory, order, and revenue data.

**Architecture:** Extend the existing domain and EF Core model with a minimal `Order`/`OrderItem` aggregate, seed realistic internal orders, and expose a single admin reporting query through MediatR and `/api/admin/dashboard/metrics`. Update the Angular admin client and dashboard page to consume that single response so the dashboard stays a thin authenticated consumer of backend reporting data.

**Tech Stack:** ASP.NET Core, MediatR, Entity Framework Core, xUnit, FluentAssertions, Angular standalone components, RxJS, Jasmine/Karma.

---

### Task 1: Lock the backend dashboard contract with API tests

**Files:**
- Modify: `tests/Ecommerce.Api.Tests/AdminCatalogEndpointsTests.cs`
- Modify: `tests/Ecommerce.Api.Tests/TestApiFactory.cs`

- [ ] **Step 1: Write the failing test**

Add an API test that calls `GET /api/admin/dashboard/metrics` and asserts:
- `200 OK`
- revenue and order count fields exist
- recent orders are returned in descending recency order
- critical inventory items are returned

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test tests/Ecommerce.Api.Tests/Ecommerce.Api.Tests.csproj --filter AdminCatalogEndpointsTests`

Expected: FAIL because the endpoint does not exist yet.

- [ ] **Step 3: Write minimal implementation**

Add just enough controller/query wiring and test seed data support for the endpoint to respond.

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test tests/Ecommerce.Api.Tests/Ecommerce.Api.Tests.csproj --filter AdminCatalogEndpointsTests`

Expected: PASS.

### Task 2: Add the order domain model with application test coverage

**Files:**
- Create: `src/Ecommerce.Domain/Enums/OrderStatus.cs`
- Create: `src/Ecommerce.Domain/Entities/Order.cs`
- Create: `src/Ecommerce.Domain/Entities/OrderItem.cs`
- Create: `tests/Ecommerce.Domain.Tests/Entities/OrderTests.cs`

- [ ] **Step 1: Write the failing test**

Add domain tests for:
- creating an order with copied item pricing data
- computing subtotal from order items
- recognizing revenue only for `Paid`, `Processing`, and `Shipped`

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test tests/Ecommerce.Domain.Tests/Ecommerce.Domain.Tests.csproj --filter OrderTests`

Expected: FAIL because the order types do not exist yet.

- [ ] **Step 3: Write minimal implementation**

Add `OrderStatus`, `Order`, and `OrderItem` with the smallest behavior required by the tests.

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test tests/Ecommerce.Domain.Tests/Ecommerce.Domain.Tests.csproj --filter OrderTests`

Expected: PASS.

### Task 3: Persist and seed orders

**Files:**
- Modify: `src/Ecommerce.Application/Common/Interfaces/IApplicationDbContext.cs`
- Modify: `src/Ecommerce.Infrastructure/Persistence/ApplicationDbContext.cs`
- Create: `src/Ecommerce.Infrastructure/Persistence/Configurations/OrderConfiguration.cs`
- Create: `src/Ecommerce.Infrastructure/Persistence/Configurations/OrderItemConfiguration.cs`
- Modify: `src/Ecommerce.Infrastructure/Persistence/Seed/CatalogSeed.cs`
- Modify: `tests/Ecommerce.Api.Tests/TestApiFactory.cs`

- [ ] **Step 1: Write the failing test**

Extend API test seed usage or add an application-level seed-oriented test that expects dashboard reporting data to include persisted orders and stable recent-order ordering.

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test tests/Ecommerce.Api.Tests/Ecommerce.Api.Tests.csproj --filter Dashboard`

Expected: FAIL because orders are not part of the context or seed yet.

- [ ] **Step 3: Write minimal implementation**

Add `DbSet<Order>` and `DbSet<OrderItem>`, EF configurations, and idempotent seed data with a small realistic recent order history using existing products.

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test tests/Ecommerce.Api.Tests/Ecommerce.Api.Tests.csproj --filter Dashboard`

Expected: PASS.

### Task 4: Build the dashboard reporting query

**Files:**
- Create: `src/Ecommerce.Application/Dashboard/GetAdminDashboardMetrics/GetAdminDashboardMetricsQuery.cs`
- Create: `src/Ecommerce.Application/Dashboard/GetAdminDashboardMetrics/GetAdminDashboardMetricsResponse.cs`
- Create: `src/Ecommerce.Application/Dashboard/GetAdminDashboardMetrics/GetAdminDashboardMetricsHandler.cs`
- Create: `tests/Ecommerce.Application.Tests/Dashboard/GetAdminDashboardMetricsHandlerTests.cs`

- [ ] **Step 1: Write the failing test**

Add application tests that assert:
- `periodRevenue` uses only recognized orders in the last 30 days
- `recognizedRevenue` uses all recognized orders
- order status counts are correct
- critical inventory only includes active products with available quantity `< 5`
- recent orders are limited to 5 and ordered by `CreatedAtUtc` descending

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test tests/Ecommerce.Application.Tests/Ecommerce.Application.Tests.csproj --filter GetAdminDashboardMetricsHandlerTests`

Expected: FAIL because the query slice does not exist yet.

- [ ] **Step 3: Write minimal implementation**

Add the query, DTOs, and handler using `IApplicationDbContext` only.

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test tests/Ecommerce.Application.Tests/Ecommerce.Application.Tests.csproj --filter GetAdminDashboardMetricsHandlerTests`

Expected: PASS.

### Task 5: Expose the admin dashboard endpoint

**Files:**
- Create: `src/Ecommerce.Api/Controllers/AdminDashboardController.cs`
- Modify: `tests/Ecommerce.Api.Tests/AdminCatalogEndpointsTests.cs`

- [ ] **Step 1: Write the failing test**

Strengthen the admin API test to assert the exact response fields returned by `/api/admin/dashboard/metrics`.

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test tests/Ecommerce.Api.Tests/Ecommerce.Api.Tests.csproj --filter AdminCatalogEndpointsTests`

Expected: FAIL until the controller returns the expected payload shape.

- [ ] **Step 3: Write minimal implementation**

Add `AdminDashboardController` that sends the MediatR query and returns `Ok(response)`.

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test tests/Ecommerce.Api.Tests/Ecommerce.Api.Tests.csproj --filter AdminCatalogEndpointsTests`

Expected: PASS.

### Task 6: Add admin frontend models and service support

**Files:**
- Modify: `apps/admin/src/app/core/models/catalog.models.ts`
- Modify: `apps/admin/src/app/core/services/catalog-api.service.ts`
- Modify: `apps/admin/src/app/core/services/catalog-api.service.spec.ts`

- [ ] **Step 1: Write the failing test**

Add frontend service tests that assert `getAdminDashboardMetrics()` issues a request to `/api/admin/dashboard/metrics` and maps the returned payload type.

- [ ] **Step 2: Run test to verify it fails**

Run: `npm.cmd --workspace apps/admin test -- --watch=false --include src/app/core/services/catalog-api.service.spec.ts`

Expected: FAIL because the service method and models do not exist yet.

- [ ] **Step 3: Write minimal implementation**

Add typed dashboard response interfaces and `getAdminDashboardMetrics()` to the admin catalog service.

- [ ] **Step 4: Run test to verify it passes**

Run: `npm.cmd --workspace apps/admin test -- --watch=false --include src/app/core/services/catalog-api.service.spec.ts`

Expected: PASS.

### Task 7: Replace dashboard placeholder data with real reporting data

**Files:**
- Modify: `apps/admin/src/app/features/dashboard/dashboard.page.ts`
- Modify: `apps/admin/src/app/features/dashboard/dashboard.page.spec.ts`

- [ ] **Step 1: Write the failing test**

Update dashboard specs to assert:
- revenue and status counts come from API data
- recent orders render API-provided order numbers, customer names, totals, and statuses
- critical inventory renders API-provided available quantity
- the dashboard no longer depends on hardcoded `recentOrders`

- [ ] **Step 2: Run test to verify it fails**

Run: `npm.cmd --workspace apps/admin test -- --watch=false --include src/app/features/dashboard/dashboard.page.spec.ts`

Expected: FAIL because the page still uses placeholder arrays and computed values.

- [ ] **Step 3: Write minimal implementation**

Replace dashboard placeholder state with a signal backed by `getAdminDashboardMetrics()`. Keep the chart static if necessary, but drive all headline metrics, recent orders, and inventory alerts from the API.

- [ ] **Step 4: Run test to verify it passes**

Run: `npm.cmd --workspace apps/admin test -- --watch=false --include src/app/features/dashboard/dashboard.page.spec.ts`

Expected: PASS.

### Task 8: Full verification

**Files:**
- Verify only

- [ ] **Step 1: Run backend verification**

Run: `dotnet test ToyShops.sln`

Expected: all .NET tests pass.

- [ ] **Step 2: Run admin frontend verification**

Run: `npm.cmd --workspace apps/admin test -- --watch=false`

Expected: all admin frontend tests pass.

- [ ] **Step 3: Summarize residual risk**

Call out that the chart may still be visual-only if left static, while all other dashboard metrics are real.

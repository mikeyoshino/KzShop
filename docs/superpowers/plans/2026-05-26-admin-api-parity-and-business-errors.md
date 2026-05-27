# Admin API Parity And Business Errors Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make the admin app cover the available admin API workflows, including authentication wiring, and handle business failures through stable numeric error codes the UI can consume.

**Architecture:** Keep the existing MediatR + `Result<T>` backend flow and standardize admin failure payloads at the controller base so every admin endpoint emits the same `{ code, message }` shape. On the Angular side, centralize API error parsing in the admin client layer, then update pages to use typed business errors instead of generic strings while filling missing product/category loading and auth workflow gaps.

**Tech Stack:** ASP.NET Core, MediatR, xUnit, FluentAssertions, Angular standalone components, RxJS, Jasmine/Karma.

---

### Task 1: Lock the API error contract with tests

**Files:**
- Modify: `tests/Ecommerce.Api.Tests/AdminCatalogEndpointsTests.cs`

- [ ] **Step 1: Write the failing test**

Add assertions that duplicate studio creation and missing product image upload return JSON payloads with numeric `code` values matching `BusinessErrorCode`.

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test tests/Ecommerce.Api.Tests/Ecommerce.Api.Tests.csproj --filter AdminCatalogEndpointsTests`

Expected: failure because current API emits string enum names.

- [ ] **Step 3: Write minimal implementation**

Update admin controller failure mapping so every business failure payload uses numeric `code`, including the inline validation branch in product image upload.

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test tests/Ecommerce.Api.Tests/Ecommerce.Api.Tests.csproj --filter AdminCatalogEndpointsTests`

Expected: PASS.

### Task 2: Lock the admin client error parsing with tests

**Files:**
- Modify: `apps/admin/src/app/core/services/catalog-api.service.ts`
- Create: `apps/admin/src/app/core/services/catalog-api.service.spec.ts`
- Modify: `apps/admin/src/app/core/models/catalog.models.ts`

- [ ] **Step 1: Write the failing test**

Add tests that an HTTP error payload like `{ code: 1, message: "Duplicate name." }` is surfaced to callers as a typed business error, not a collapsed generic string.

- [ ] **Step 2: Run test to verify it fails**

Run: `npm.cmd --workspace apps/admin test -- --watch=false --include src/app/core/services/catalog-api.service.spec.ts`

Expected: failure because no typed error parsing exists yet.

- [ ] **Step 3: Write minimal implementation**

Add typed business error models and a helper in the admin catalog service to normalize backend failures into consistent numeric-code objects.

- [ ] **Step 4: Run test to verify it passes**

Run: `npm.cmd --workspace apps/admin test -- --watch=false --include src/app/core/services/catalog-api.service.spec.ts`

Expected: PASS.

### Task 3: Complete product editor/category workflow with tests

**Files:**
- Modify: `apps/admin/src/app/features/product-form/product-form.page.ts`
- Modify: `apps/admin/src/app/features/product-form/product-form.page.spec.ts`

- [ ] **Step 1: Write the failing test**

Add tests that edit mode loads categories and product details, and create/update/archive/inventory/image failures display business-code-aware messages.

- [ ] **Step 2: Run test to verify it fails**

Run: `npm.cmd --workspace apps/admin test -- --watch=false --include src/app/features/product-form/product-form.page.spec.ts`

Expected: failure because categories and typed failure handling are incomplete.

- [ ] **Step 3: Write minimal implementation**

Load categories into the product form, normalize page messages from business errors, and ensure all product operations refresh state correctly.

- [ ] **Step 4: Run test to verify it passes**

Run: `npm.cmd --workspace apps/admin test -- --watch=false --include src/app/features/product-form/product-form.page.spec.ts`

Expected: PASS.

### Task 4: Verify auth/admin workflow pages consume the shared error shape

**Files:**
- Modify: `apps/admin/src/app/core/services/supabase-auth.service.ts`
- Modify: `apps/admin/src/app/features/auth/login.page.ts`
- Modify: `apps/admin/src/app/features/auth/register.page.ts`
- Modify: `apps/admin/src/app/features/categories/categories.page.ts`
- Modify: `apps/admin/src/app/features/studios/studios.page.ts`
- Modify: related `*.spec.ts` files as needed

- [ ] **Step 1: Write the failing test**

Add or extend specs so auth pages and admin management pages surface actionable error messages and success states while continuing to use the shared auth/interceptor flow.

- [ ] **Step 2: Run test to verify it fails**

Run: `npm.cmd --workspace apps/admin test -- --watch=false --include src/app/features/auth/login.page.spec.ts --include src/app/features/auth/register.page.spec.ts --include src/app/features/categories/categories.page.spec.ts --include src/app/features/studios/studios.page.spec.ts`

Expected: failure where current pages only use generic fallback strings or miss needed data loading.

- [ ] **Step 3: Write minimal implementation**

Keep Supabase auth flow intact, improve page-level error handling, and wire shared business error parsing into categories and studios management.

- [ ] **Step 4: Run test to verify it passes**

Run: `npm.cmd --workspace apps/admin test -- --watch=false --include src/app/features/auth/login.page.spec.ts --include src/app/features/auth/register.page.spec.ts --include src/app/features/categories/categories.page.spec.ts --include src/app/features/studios/studios.page.spec.ts`

Expected: PASS.

### Task 5: Final verification

**Files:**
- Verify only

- [ ] **Step 1: Run backend verification**

Run: `dotnet test ToyShops.sln`

Expected: all .NET tests pass.

- [ ] **Step 2: Run admin frontend verification**

Run: `npm.cmd --workspace apps/admin test -- --watch=false`

Expected: admin frontend tests pass.

- [ ] **Step 3: Summarize residual risk**

Call out any remaining placeholder dashboard metrics or unimplemented backend capabilities not exposed by current controllers.

# Admin Catalog Management Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement admin catalog write workflows (studios, categories, products, inventory, images) with Supabase Storage-backed media handling.

**Architecture:** Extend the existing Clean Architecture + Vertical Slice backend with admin write commands/queries and thin admin controllers. Build corresponding Angular admin routes/pages/forms to operate on those APIs. Keep business rules in Domain + Application validators; keep external storage logic in Infrastructure.

**Tech Stack:** ASP.NET Core, MediatR, FluentValidation, EF Core/Npgsql, Supabase Storage, Angular SSR, xUnit

---

## Scope Split

This plan covers:

- backend write-side slices for studios/categories/products/inventory/product-images
- Supabase Storage integration for backend-managed uploads
- admin API endpoints
- admin frontend operational pages/forms for catalog management
- tests for critical handlers and admin endpoints

This plan does not cover:

- auth/authorization wiring
- cart/checkout/orders
- Stripe payment completion flows

## File Map

### Application

- Create: `src/Ecommerce.Application/Common/Interfaces/IStorageService.cs`
- Create: `src/Ecommerce.Application/Studios/CreateStudio/CreateStudioCommand.cs`
- Create: `src/Ecommerce.Application/Studios/CreateStudio/CreateStudioHandler.cs`
- Create: `src/Ecommerce.Application/Studios/CreateStudio/CreateStudioValidator.cs`
- Create: `src/Ecommerce.Application/Studios/CreateStudio/CreateStudioResponse.cs`
- Create: `src/Ecommerce.Application/Studios/SearchStudios/SearchStudiosQuery.cs`
- Create: `src/Ecommerce.Application/Studios/SearchStudios/SearchStudiosHandler.cs`
- Create: `src/Ecommerce.Application/Studios/SearchStudios/SearchStudiosResponse.cs`
- Create: `src/Ecommerce.Application/Categories/CreateCategory/CreateCategoryCommand.cs`
- Create: `src/Ecommerce.Application/Categories/CreateCategory/CreateCategoryHandler.cs`
- Create: `src/Ecommerce.Application/Categories/CreateCategory/CreateCategoryValidator.cs`
- Create: `src/Ecommerce.Application/Categories/CreateCategory/CreateCategoryResponse.cs`
- Create: `src/Ecommerce.Application/Categories/UpdateCategory/UpdateCategoryCommand.cs`
- Create: `src/Ecommerce.Application/Categories/UpdateCategory/UpdateCategoryHandler.cs`
- Create: `src/Ecommerce.Application/Categories/UpdateCategory/UpdateCategoryValidator.cs`
- Create: `src/Ecommerce.Application/Categories/UpdateCategory/UpdateCategoryResponse.cs`
- Create: `src/Ecommerce.Application/Products/CreateProduct/*`
- Create: `src/Ecommerce.Application/Products/UpdateProduct/*`
- Create: `src/Ecommerce.Application/Products/ArchiveProduct/*`
- Create: `src/Ecommerce.Application/Products/GetAdminProductById/*`
- Create: `src/Ecommerce.Application/Inventory/AdjustStock/*`
- Create: `src/Ecommerce.Application/ProductImages/UploadProductImage/*`
- Create: `src/Ecommerce.Application/ProductImages/ReorderProductImages/*`
- Create: `src/Ecommerce.Application/ProductImages/DeleteProductImage/*`

### Infrastructure

- Create: `src/Ecommerce.Infrastructure/Storage/StorageOptions.cs`
- Create: `src/Ecommerce.Infrastructure/Storage/SupabaseStorageService.cs`
- Modify: `src/Ecommerce.Infrastructure/DependencyInjection.cs`
- Modify: `src/Ecommerce.Infrastructure/Persistence/ApplicationDbContext.cs`

### API

- Create: `src/Ecommerce.Api/Controllers/AdminStudiosController.cs`
- Create: `src/Ecommerce.Api/Controllers/AdminCategoriesController.cs`
- Create: `src/Ecommerce.Api/Controllers/AdminProductsController.cs`
- Create: `src/Ecommerce.Api/Controllers/AdminInventoryController.cs`
- Create: `src/Ecommerce.Api/Controllers/AdminProductImagesController.cs`
- Modify: `src/Ecommerce.Api/Program.cs`

### Admin Frontend

- Create: `apps/admin/src/app/features/studios/studios.page.ts`
- Create: `apps/admin/src/app/features/categories/categories.page.ts`
- Create: `apps/admin/src/app/features/product-form/product-form.page.ts`
- Modify: `apps/admin/src/app/app.routes.ts`
- Modify: `apps/admin/src/app/core/models/catalog.models.ts`
- Modify: `apps/admin/src/app/core/services/catalog-api.service.ts`
- Modify: `apps/admin/src/styles.css`

### Tests

- Create: `tests/Ecommerce.Application.Tests/Studios/CreateStudioHandlerTests.cs`
- Create: `tests/Ecommerce.Application.Tests/Categories/CreateCategoryHandlerTests.cs`
- Create: `tests/Ecommerce.Application.Tests/Products/CreateProductHandlerTests.cs`
- Create: `tests/Ecommerce.Application.Tests/Inventory/AdjustStockHandlerTests.cs`
- Create: `tests/Ecommerce.Application.Tests/ProductImages/ReorderProductImagesHandlerTests.cs`
- Create: `tests/Ecommerce.Api.Tests/AdminCatalogEndpointsTests.cs`

## Task 1: Add storage abstraction and infrastructure implementation

**Files:**
- Create: `src/Ecommerce.Application/Common/Interfaces/IStorageService.cs`
- Create: `src/Ecommerce.Infrastructure/Storage/StorageOptions.cs`
- Create: `src/Ecommerce.Infrastructure/Storage/SupabaseStorageService.cs`
- Modify: `src/Ecommerce.Infrastructure/DependencyInjection.cs`
- Modify: `.env.example`

- [ ] **Step 1: Add `IStorageService` application contract**
- [ ] **Step 2: Add storage options class for Supabase URL/key/bucket**
- [ ] **Step 3: Implement `SupabaseStorageService` with upload/delete methods**
- [ ] **Step 4: Register storage options/service in infrastructure DI**
- [ ] **Step 5: Add required storage env vars to `.env.example`**
- [ ] **Step 6: Run `dotnet build ToyShops.sln`**
- [ ] **Step 7: Commit with `feat: add supabase storage infrastructure`**

## Task 2: Implement studio write/search slices

**Files:**
- Create: `src/Ecommerce.Application/Studios/CreateStudio/*`
- Create: `src/Ecommerce.Application/Studios/SearchStudios/*`
- Create: `tests/Ecommerce.Application.Tests/Studios/CreateStudioHandlerTests.cs`

- [ ] **Step 1: Add failing tests for duplicate slug/name and successful create**
- [ ] **Step 2: Implement `CreateStudio` command, handler, validator, response**
- [ ] **Step 3: Implement `SearchStudios` query and response**
- [ ] **Step 4: Run targeted tests for studio handlers**
- [ ] **Step 5: Commit with `feat: add studio management slices`**

## Task 3: Implement category create/update slices

**Files:**
- Create: `src/Ecommerce.Application/Categories/CreateCategory/*`
- Create: `src/Ecommerce.Application/Categories/UpdateCategory/*`
- Create: `tests/Ecommerce.Application.Tests/Categories/CreateCategoryHandlerTests.cs`

- [ ] **Step 1: Add failing tests for create/update and uniqueness validation**
- [ ] **Step 2: Implement category create command/validator/handler/response**
- [ ] **Step 3: Implement category update command/validator/handler/response**
- [ ] **Step 4: Run targeted category tests**
- [ ] **Step 5: Commit with `feat: add category management slices`**

## Task 4: Implement product write and admin detail slices

**Files:**
- Create: `src/Ecommerce.Application/Products/CreateProduct/*`
- Create: `src/Ecommerce.Application/Products/UpdateProduct/*`
- Create: `src/Ecommerce.Application/Products/ArchiveProduct/*`
- Create: `src/Ecommerce.Application/Products/GetAdminProductById/*`
- Create: `tests/Ecommerce.Application.Tests/Products/CreateProductHandlerTests.cs`

- [ ] **Step 1: Add failing tests for create/update/archive behavior**
- [ ] **Step 2: Implement create product slice**
- [ ] **Step 3: Implement update product slice including spec row updates**
- [ ] **Step 4: Implement archive product slice**
- [ ] **Step 5: Implement admin product detail query slice**
- [ ] **Step 6: Run targeted product tests**
- [ ] **Step 7: Commit with `feat: add product write slices`**

## Task 5: Implement inventory and product image slices

**Files:**
- Create: `src/Ecommerce.Application/Inventory/AdjustStock/*`
- Create: `src/Ecommerce.Application/ProductImages/UploadProductImage/*`
- Create: `src/Ecommerce.Application/ProductImages/ReorderProductImages/*`
- Create: `src/Ecommerce.Application/ProductImages/DeleteProductImage/*`
- Create: `tests/Ecommerce.Application.Tests/Inventory/AdjustStockHandlerTests.cs`
- Create: `tests/Ecommerce.Application.Tests/ProductImages/ReorderProductImagesHandlerTests.cs`

- [ ] **Step 1: Add failing tests for stock adjustment and image reorder invariants**
- [ ] **Step 2: Implement adjust stock slice**
- [ ] **Step 3: Implement upload image slice using `IStorageService`**
- [ ] **Step 4: Implement reorder image slice with deterministic contiguous sort orders**
- [ ] **Step 5: Implement delete image slice with storage delete side effect**
- [ ] **Step 6: Run targeted inventory/image tests**
- [ ] **Step 7: Commit with `feat: add inventory and image slices`**

## Task 6: Expose admin API endpoints

**Files:**
- Create: `src/Ecommerce.Api/Controllers/AdminStudiosController.cs`
- Create: `src/Ecommerce.Api/Controllers/AdminCategoriesController.cs`
- Create: `src/Ecommerce.Api/Controllers/AdminProductsController.cs`
- Create: `src/Ecommerce.Api/Controllers/AdminInventoryController.cs`
- Create: `src/Ecommerce.Api/Controllers/AdminProductImagesController.cs`
- Modify: `src/Ecommerce.Api/Program.cs`
- Create: `tests/Ecommerce.Api.Tests/AdminCatalogEndpointsTests.cs`

- [ ] **Step 1: Add failing API integration tests for admin endpoints**
- [ ] **Step 2: Implement thin admin controllers forwarding to MediatR**
- [ ] **Step 3: Add multipart endpoint for product image upload**
- [ ] **Step 4: Run admin endpoint tests**
- [ ] **Step 5: Commit with `feat: expose admin catalog endpoints`**

## Task 7: Build admin UI for studios/categories/products

**Files:**
- Create: `apps/admin/src/app/features/studios/studios.page.ts`
- Create: `apps/admin/src/app/features/categories/categories.page.ts`
- Create: `apps/admin/src/app/features/product-form/product-form.page.ts`
- Modify: `apps/admin/src/app/app.routes.ts`
- Modify: `apps/admin/src/app/core/models/catalog.models.ts`
- Modify: `apps/admin/src/app/core/services/catalog-api.service.ts`
- Modify: `apps/admin/src/app/features/products/products.page.ts`
- Modify: `apps/admin/src/styles.css`

- [ ] **Step 1: Add admin service methods for new endpoints**
- [ ] **Step 2: Implement studios page with search/create**
- [ ] **Step 3: Implement categories page with list/create/update**
- [ ] **Step 4: Implement product create/edit page with studio autocomplete and inline studio create**
- [ ] **Step 5: Add product spec row editor and inventory adjustment controls**
- [ ] **Step 6: Add product image upload/reorder/delete controls**
- [ ] **Step 7: Run `npm run build:admin`**
- [ ] **Step 8: Commit with `feat: add admin catalog management ui`**

## Task 8: Final verification and docs alignment

**Files:**
- Modify: `README.md`
- Modify: `.env.example`

- [ ] **Step 1: Update README with storage bucket/key setup notes**
- [ ] **Step 2: Run full verification**

Run:
- `dotnet build ToyShops.sln`
- `dotnet test ToyShops.sln`
- `npm run build:storefront`
- `npm run build:admin`

Expected: all pass.

- [ ] **Step 3: Commit with `docs: update admin catalog setup notes`**

## Self-Review

Spec coverage:
- Includes all requested admin catalog workflows and real Supabase Storage integration.
- Defers auth and commerce features per approved scope.

No placeholders:
- All tasks have concrete file targets and verification commands.

Type consistency:
- Uses existing naming conventions: feature folders with Command/Handler/Validator/Response, plus Query/Handler/Response where applicable.


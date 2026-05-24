# Toy Shop E-commerce Design

Date: 2026-05-24

## Overview

This project is a monorepo e-commerce platform for collectible figures and art statues. It uses two separate Angular SSR frontends from day one, a single ASP.NET Core Web API backend, and Supabase for auth, Postgres, and storage. The implementation will follow Clean Architecture for backend boundaries and Vertical Slice Architecture for application use cases.

The first delivery scope is a full foundation that includes:

- public storefront
- separate admin frontend
- catalog and collection browsing
- product detail pages with specification-heavy figure data
- Supabase auth integration
- cart and checkout flows
- Stripe as the first payment provider
- product, category, studio, image, inventory, and order management in admin

Redis and Hangfire are explicitly out of scope for this phase.

## Goals

- Match the provided `docs/UITemplate.html` and reference screens for home, product list, and product detail
- Keep business logic out of Angular and controllers
- Centralize use-case logic in the Application layer with MediatR
- Enforce business rules in Domain entities and value objects
- Use Supabase JWTs for identity, but keep backend authorization in ASP.NET Core
- Model collectible figures with enough structured specification data for premium art/statue products

## Monorepo Structure

Top-level structure:

```text
apps/
  storefront/
  admin/
src/
  Ecommerce.Api/
  Ecommerce.Application/
  Ecommerce.Domain/
  Ecommerce.Infrastructure/
  Ecommerce.Shared/
docs/
  UITemplate.html
```

### Frontends

- `apps/storefront`: Angular SSR app for customer-facing browsing, auth, cart, checkout, and account order history
- `apps/admin`: Angular SSR app for operational management of catalog, images, inventory, and orders

### Backend

- `src/Ecommerce.Api`: HTTP layer only
- `src/Ecommerce.Application`: use cases organized by vertical slices
- `src/Ecommerce.Domain`: entities, value objects, enums, domain exceptions, and business rules
- `src/Ecommerce.Infrastructure`: EF Core, Supabase auth/storage integration, Stripe integration
- `src/Ecommerce.Shared`: shared cross-project primitives if needed

## UI Scope Derived From Provided Screens

The supplied images and `UITemplate.html` are sufficient to define the initial storefront catalog experience:

- Home page:
  - hero feature section
  - franchise/category section
  - latest allocations/new products
  - editorial/supporting content block
  - dark operational footer
- Product list page:
  - breadcrumb
  - category title and product count
  - left filter rail
  - sort selector
  - product grid
- Product detail page:
  - image gallery with thumbnails
  - status and new badges
  - price and preorder messaging
  - specification block
  - primary purchase CTA

The images also imply a premium figure-specific data model:

- studio/manufacturer
- scale
- status such as preorder/in-stock/waitlist
- multiple ordered images
- description and short description
- structured specification fields
- optional additional specification rows

The images do not fully define discounts, reviews, CMS blocks, variants, or analytics. Those remain out of scope for phase 1.

## Frontend Boundaries

### Storefront App

Primary routes:

- `/`
- `/collections/:slug`
- `/products/:slug`
- `/cart`
- `/login`
- `/register`
- `/account/orders`
- `/checkout`

Responsibilities:

- server-side render public catalog pages
- bootstrap Supabase Auth session
- call backend APIs for catalog, cart, checkout, and orders
- render data only; do not enforce business rules

### Admin App

Primary routes:

- `/login`
- `/dashboard`
- `/products`
- `/products/new`
- `/products/:id`
- `/categories`
- `/studios`
- `/inventory`
- `/orders`

Responsibilities:

- manage studios with autocomplete and inline creation
- create, edit, archive, and publish products
- upload and reorder product images
- manage stock quantities
- inspect orders

The admin app should be visually quieter and more operational than the storefront. It should not reuse the storefront’s editorial surface patterns.

## Backend Architecture

### Ecommerce.Api

Responsibilities:

- configure ASP.NET Core pipeline
- authenticate Supabase JWTs
- authorize customer/admin access
- expose thin controllers or minimal HTTP endpoints that forward directly to MediatR
- configure CORS, Swagger, middleware, and error handling

Constraints:

- no business logic in controllers
- no direct EF Core workflow logic in controllers

### Ecommerce.Application

This is the primary logic layer. It owns use-case orchestration, validation, and workflow decisions.

Common pieces:

- `Behaviors/ValidationBehavior.cs`
- `Behaviors/LoggingBehavior.cs`
- `Behaviors/TransactionBehavior.cs`
- `Interfaces/IApplicationDbContext.cs`
- `Interfaces/ICurrentUserService.cs`
- `Interfaces/IStorageService.cs`
- `Interfaces/IPaymentService.cs`

Initial vertical slices:

- `Products/`
  - `CreateProduct/`
  - `UpdateProduct/`
  - `ArchiveProduct/`
  - `GetProducts/`
  - `GetProductBySlug/`
  - `GetAdminProductById/`
- `Categories/`
  - `CreateCategory/`
  - `UpdateCategory/`
  - `GetCategories/`
- `Studios/`
  - `CreateStudio/`
  - `SearchStudios/`
  - `GetStudios/`
- `ProductImages/`
  - `UploadProductImage/`
  - `ReorderProductImages/`
  - `DeleteProductImage/`
- `Inventory/`
  - `AdjustStock/`
  - `GetInventoryStatus/`
- `Cart/`
  - `AddToCart/`
  - `RemoveFromCart/`
  - `UpdateCartItemQuantity/`
  - `GetMyCart/`
- `Checkout/`
  - `CreateCheckoutSession/`
  - `CompleteCheckout/`
- `Orders/`
  - `GetMyOrders/`
  - `GetOrderById/`
  - `GetAdminOrders/`
- `Payments/`
  - `HandleStripeWebhook/`

Each use case keeps its command or query, handler, validator, and response DTO together.

### Ecommerce.Domain

The Domain layer protects business invariants. It is not just persistence models.

Planned entities:

- `Product`
- `ProductImage`
- `ProductSpecification`
- `Category`
- `Studio`
- `Cart`
- `CartItem`
- `Order`
- `OrderItem`
- `Customer`
- `Payment`
- `InventoryItem`

Planned value objects:

- `Money`
- `Slug`
- `Address`

Planned enums:

- `OrderStatus`
- `PaymentStatus`
- `ProductStatus`

Business rule examples:

- product price must be greater than zero
- archived products are not purchasable
- stock cannot go below zero
- cart quantity must remain valid
- order cannot become paid twice

### Ecommerce.Infrastructure

Responsibilities:

- EF Core persistence with Npgsql against Supabase Postgres
- application DbContext and entity configurations
- Supabase JWT current-user resolution
- Supabase Storage integration for product images
- Stripe integration for checkout session creation and webhook completion
- dependency injection wiring

Out of scope for this phase:

- Redis caching
- Hangfire background jobs

## Domain Model

### Category

Fields:

- `Id`
- `Name`
- `Slug`
- `Description`
- `IsActive`

### Studio

Fields:

- `Id`
- `Name`
- `Slug`
- `IsActive`

Notes:

- no website field
- studio is a managed entity
- admin product forms use studio autocomplete with inline create when no match exists

### Product

Core fields:

- `Id`
- `Name`
- `Slug`
- `ShortDescription`
- `Description`
- `CategoryId`
- `StudioId`
- `Sku`
- `PriceAmount`
- `Currency`
- `Status`
- `IsNew`
- `IsFeatured`
- `Scale`
- `Materials`
- `DimensionsText`
- `EditionSize`
- `EstimatedReleaseText`
- `PublishedAt`
- `ArchivedAt`

Reasoning:

- figure art products are spec-heavy and image-led
- common premium figure fields should be first-class product properties
- these fields directly support the product detail screen shown in the provided reference images

### ProductImage

Fields:

- `Id`
- `ProductId`
- `StoragePath`
- `PublicUrl`
- `AltText`
- `SortOrder`

Notes:

- images are ordered
- storefront and admin both depend on deterministic image ordering

### ProductSpecification

Fields:

- `Id`
- `ProductId`
- `Label`
- `Value`
- `SortOrder`

Notes:

- used for custom rows beyond the fixed specification fields on `Product`
- product detail page renders standard product fields first, then extra specification rows

### InventoryItem

Fields:

- `Id`
- `ProductId`
- `OnHandQuantity`
- `ReservedQuantity`

### Customer

Fields:

- `Id`
- `SupabaseUserId`
- `Email`
- `FirstName`
- `LastName`

### Cart

Fields:

- `Id`
- `CustomerId`

### CartItem

Fields:

- `Id`
- `CartId`
- `ProductId`
- `Quantity`
- `UnitPriceAmount`
- `Currency`

Notes:

- cart is persisted in Postgres in phase 1
- cart state is not browser-only and not Redis-backed

### Order

Fields:

- `Id`
- `CustomerId`
- `OrderNumber`
- `Status`
- `PaymentStatus`
- `SubtotalAmount`
- `Currency`
- `ShippingAddress`
- `BillingAddress`
- `PlacedAt`
- `StripeCheckoutSessionId`
- `StripePaymentIntentId`

### OrderItem

Fields:

- `Id`
- `OrderId`
- `ProductId`
- `ProductNameSnapshot`
- `UnitPriceAmount`
- `Quantity`

### Payment

Fields:

- `Id`
- `OrderId`
- `Provider`
- `ProviderReference`
- `Status`
- `Amount`
- `Currency`

## Auth and Authorization

Identity source:

- Supabase Auth

Backend enforcement:

- ASP.NET Core validates Supabase JWTs
- authorization policies determine customer vs admin access

Initial roles:

- `Customer`
- `Admin`

Guiding rule:

- frontend routes can hide or show UI
- backend is the actual security boundary

API route segmentation:

- `/api/storefront/...`
- `/api/admin/...`
- `/api/payments/webhooks/stripe`

## Key User and System Flows

### Catalog Browsing

1. Storefront SSR route requests catalog data from API
2. API dispatches product/category queries via MediatR
3. Query handlers read from EF Core context
4. Storefront renders home, collection, and product detail pages

### Studio Autocomplete in Admin

1. Admin types a studio name in product create/edit form
2. Admin app queries backend studio search endpoint
3. Matching studios are returned
4. Admin selects an existing studio or triggers inline studio creation
5. Product command persists the selected or newly created studio reference

### Product Image Upload

1. Admin opens product editor
2. Admin uploads one or more images
3. Backend-controlled storage flow writes images to Supabase Storage
4. Backend persists `ProductImage` rows with sort order
5. Admin can reorder images and save updated order

### Cart

1. Authenticated customer adds a product to cart
2. Storefront sends command to API
3. Application validates product availability and quantity
4. Cart is persisted in Postgres
5. Storefront renders live cart state from API responses

### Checkout and Payment

1. Authenticated customer starts checkout
2. API creates Stripe checkout session through payment service
3. Customer completes payment on Stripe
4. Stripe webhook calls backend endpoint
5. Backend validates webhook and completes payment workflow
6. Order and payment state are updated

## Validation and Rule Placement

Request validation:

- FluentValidation in Application slice validators

Business rules:

- Domain entities and value objects

Workflow orchestration:

- Application handlers

Persistence and external integrations:

- Infrastructure

Presentation concerns:

- Angular only

Examples:

- `Price > 0` -> Domain
- `Order cannot be paid twice` -> Domain
- `Create Stripe checkout session` -> Application + Infrastructure
- `Validate product create payload` -> FluentValidation
- `Upload image to Supabase Storage` -> Infrastructure

## Filtering, Sorting, and Catalog Query Expectations

Phase 1 PLP behavior:

- filters:
  - status
  - scale
  - category
- sorting:
  - newest first
  - likely price and name later if needed

Studio filtering may be added later if the catalog size warrants it, but it is not required to deliver the initial UI based on the reference screens.

## Testing Strategy

Backend:

- domain unit tests for business invariants
- application tests for handlers and validators
- API integration tests for important public/admin flows

Frontend:

- Angular component/page tests for critical screens and forms
- SSR smoke tests for major routes

First priority tests:

- product create/edit validation
- studio autocomplete and inline create behavior
- home/PLP/PDP catalog queries
- cart add/update/remove
- checkout session creation
- Stripe webhook completion path

## Non-Goals for Phase 1

- Redis-backed cart or cache
- Hangfire background jobs
- discounts and promotions
- reviews and ratings
- CMS/editorial management system
- analytics dashboards
- product variants beyond the current single-item model

## Recommended Implementation Order

1. Monorepo scaffolding
2. Backend solution and project wiring
3. Domain model and persistence baseline
4. Catalog read APIs for storefront
5. Storefront SSR shell based on the supplied template
6. Admin SSR shell
7. Product/category/studio management
8. Image upload and ordering
9. Inventory management
10. Supabase-authenticated cart
11. Checkout with Stripe
12. Order views

## Final Decisions Captured

- two separate Angular SSR frontends from the start
- one monorepo
- single ASP.NET Core Web API backend
- Clean Architecture plus Vertical Slice Architecture
- Supabase for auth, Postgres, and storage
- Stripe as the first payment provider
- no Redis or Hangfire in phase 1
- `Studio` is a managed entity with autocomplete and no website field
- product detail page uses both fixed specification fields and custom extra specification rows

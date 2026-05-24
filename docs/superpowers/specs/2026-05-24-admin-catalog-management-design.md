# Admin Catalog Management Design

Date: 2026-05-24

## Overview

This spec defines the next delivery slice for ToyShops: operational catalog management through the admin app, backed by real ASP.NET Core write APIs and Supabase Storage for product media. This slice builds on the completed foundation and catalog read-side work. It adds the missing write-side workflows needed to manage studios, categories, products, specifications, inventory, and images.

This slice intentionally does not include Supabase Auth or admin authorization yet. It also does not include cart, checkout, Stripe payment execution, orders, Redis, or Hangfire.

## Goals

- make the catalog operational from the admin app instead of relying on seed data
- keep business logic in the backend Application and Domain layers
- keep controllers thin and Angular limited to UI behavior
- integrate real Supabase Storage for product images now
- preserve the current Clean Architecture and Vertical Slice structure

## Scope

Included in this slice:

- studio management
- category management
- product create, update, and archive
- product specification row editing
- inventory adjustment
- product image upload, reorder, and delete using Supabase Storage
- admin UI screens and forms for those workflows

Explicitly excluded from this slice:

- Supabase Auth integration
- admin authorization policies
- cart
- checkout
- Stripe payment completion
- orders
- discounts
- reviews
- CMS features

## System Boundary

The current storefront read side already exists and should remain the consumer of catalog data. This slice fills the operational gap by making catalog content editable from the admin app.

The intended system shape remains:

- `apps/admin` handles operational UI only
- `src/Ecommerce.Api` exposes thin admin HTTP endpoints
- `src/Ecommerce.Application` owns use-case orchestration
- `src/Ecommerce.Domain` enforces catalog and inventory invariants
- `src/Ecommerce.Infrastructure` handles EF Core persistence and Supabase Storage integration

## Backend Write Architecture

### Application slices

Add or flesh out these vertical slices:

- `Studios/`
  - `CreateStudio/`
  - `SearchStudios/`
  - `GetStudios/`
- `Categories/`
  - `CreateCategory/`
  - `UpdateCategory/`
  - `GetCategories/`
- `Products/`
  - `CreateProduct/`
  - `UpdateProduct/`
  - `ArchiveProduct/`
  - `GetAdminProductById/`
- `Inventory/`
  - `AdjustStock/`
- `ProductImages/`
  - `UploadProductImage/`
  - `ReorderProductImages/`
  - `DeleteProductImage/`

Each slice keeps command or query, handler, validator, and response DTO together.

### API shape

Admin endpoints should be grouped under:

- `/api/admin/studios`
- `/api/admin/categories`
- `/api/admin/products`
- `/api/admin/inventory`
- `/api/admin/product-images`

Controllers remain transport-only. They receive HTTP input, map it to MediatR commands or queries, and return the result.

### Business rules

The write side must enforce these rules:

- studio names and slugs must be unique
- category names and slugs must be unique
- product price must stay greater than zero
- archived products are not sellable
- stock cannot go below zero
- every product must reference a valid category and studio
- image sort order must remain deterministic after reorder and delete
- image metadata lives in Postgres even though binary files live in Supabase Storage

## Domain and Data Model

The existing catalog model remains the base. This slice uses it more fully rather than expanding it significantly.

### Product status

The active status model remains:

- `PreOrder`
- `Active`
- `Waitlist`
- `Archived`

No `Draft` or `Published` layer is added in this slice.

### Product fields used by admin

The admin product form should manage:

- `Name`
- `Slug`
- `ShortDescription`
- `Description`
- `StudioId`
- `CategoryId`
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

### Product specifications

Standard fields stay directly on `Product`.

Additional flexible specification rows stay in `ProductSpecification`:

- `Id`
- `ProductId`
- `Label`
- `Value`
- `SortOrder`

This keeps the PDP clean while still supporting figure-specific catalog depth.

### Inventory

Inventory remains tracked via `InventoryItem`. In this slice, the admin UI should use an adjustment workflow rather than direct uncontrolled overwrite. The backend should validate that final stock does not go negative.

### Studios

`Studio` stays a managed table with:

- `Id`
- `Name`
- `Slug`
- `IsActive`

No website field is added.

## Admin UI Workflows

The admin app should remain operational and quiet in tone. It is not a marketing surface.

### Routes

Add or complete these admin routes:

- `/studios`
- `/categories`
- `/products`
- `/products/new`
- `/products/:id`
- `/inventory` if separated, though inventory can also live inside product edit

### Studios workflow

The studios page should provide:

- searchable list
- create studio form or modal
- simple management for the studio directory used by products

Studio selection inside the product form should support autocomplete:

- search existing studios while typing
- select an existing studio
- if no suitable match exists, allow inline create

### Categories workflow

The categories page should provide:

- list existing categories
- create category
- update category fields such as name, slug, and description if allowed by the current model

This should be a compact table-and-form workflow.

### Products workflow

The products area should provide:

- product list with basic search and status filtering
- create product form
- edit product form
- archive action

The product form should include the core figure fields, plus:

- studio autocomplete with inline create
- category select
- extra specification row editor
- inventory summary and adjustment control
- ordered image management

### Specifications workflow

Inside the product form, admins should be able to manage extra specification rows as label/value/sort-order entries. Standard specification fields remain fixed product fields and should not be duplicated into the flexible spec list.

### Inventory workflow

Inventory should be editable in a controlled way:

- show current on-hand quantity
- allow quantity adjustment
- validate the result server-side

This can live on the product edit screen for now instead of requiring a separate dedicated inventory module.

### Images workflow

The product edit experience should support:

- upload one or more images
- preview ordered gallery
- reorder images
- delete images
- edit alt text if practical within the same edit surface

## Storage Architecture

This slice includes real Supabase Storage integration now.

### Upload pattern

The chosen upload pattern is backend-managed:

1. admin selects a file in Angular
2. admin app sends a multipart request to the backend
3. backend validates the product and file constraints
4. backend uploads the binary to Supabase Storage
5. backend writes the `ProductImage` record in Postgres
6. backend returns image metadata to the admin app

The frontend does not upload directly to Supabase in this slice.

### Storage conventions

Use a dedicated catalog media bucket.

Recommended object path shape:

- `products/{productId}/{guid}-{sanitized-filename}`

Persist these metadata fields:

- `StoragePath`
- `PublicUrl`
- `AltText`
- `SortOrder`

### Storage rules

- validate file size and content type before upload
- generate unique file names on the server
- delete the storage object when an image record is deleted
- do not move storage objects during reorder; reorder only changes metadata

## Data Flow

The intended admin/product flow is:

1. create the product record first
2. load the product edit screen using `GetAdminProductById`
3. upload images against an existing product id
4. reorder or delete images as needed
5. adjust inventory from the same operational surface
6. storefront continues to consume product and image data from existing catalog read APIs

This keeps product identity stable before media paths are generated and avoids unnecessary temporary upload flows.

## Error Handling

Write-side validation belongs in FluentValidation and domain methods.

Expected failure modes include:

- duplicate studio or category slug
- missing studio or category reference
- invalid product price
- negative resulting stock
- unsupported image content type
- oversized upload
- product not found for image or inventory actions

The API should return clear validation or not-found responses. Angular should present operationally useful feedback without embedding business logic.

## Testing Strategy

### Backend

Add tests for:

- domain rules added or tightened by write flows
- application handlers for studio creation
- category creation and update
- product create, update, and archive
- inventory adjustment
- product image upload metadata behavior
- image reorder and delete behavior
- admin API integration endpoints

### Frontend

Add focused admin tests for:

- studio autocomplete behavior
- product form submission
- specification row editing
- image list management interactions where practical

The target is confidence in the core workflows, not exhaustive UI snapshot coverage.

## Delivery Order

Recommended implementation order for this slice:

1. backend write commands for studios, categories, products, and inventory
2. Supabase Storage integration and product image commands
3. admin API endpoints
4. admin Angular pages and forms
5. verification and docs touch-up if needed

## Expected Result

When this slice is complete:

- the catalog can be managed from the admin app
- products can be created, edited, archived, and stocked through real backend workflows
- product images can be uploaded and organized in Supabase Storage
- storefront pages continue to read updated catalog data without further changes
- auth remains deferred, but operational catalog management is no longer blocked by seed-only data


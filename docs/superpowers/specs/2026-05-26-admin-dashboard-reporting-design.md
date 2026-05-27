# Admin Dashboard Reporting Design

Date: 2026-05-26

## Overview

This spec defines the next admin slice for ToyShops: replacing placeholder dashboard metrics with real backend-driven inventory, order, and revenue reporting. The repo already has catalog and inventory management. It does not yet have an implemented order domain or reporting slice, so this work adds the minimum internal order model needed to support operational dashboard metrics.

This slice is intentionally narrow. It adds real persisted order and revenue data for admin reporting without pulling in checkout, payment capture, customer order history, or a full commerce workflow.

## Goals

- replace dashboard placeholder metrics with real API-backed data
- keep reporting inside the existing admin backend surface under `/api/admin`
- add the smallest internal order model that supports real revenue and recent-order reporting
- preserve the repo's current business error handling pattern using `Result<T>` and numeric `BusinessErrorCode`
- keep the Angular admin dashboard as a thin consumer of backend reporting data

## Non-Goals

- checkout flow
- payment processor integration
- customer-facing order views
- refunds, returns, discounts, taxes, or shipping calculations
- full order lifecycle management UI

## System Boundary

This slice adds internal reporting data for the admin dashboard only.

- `src/Ecommerce.Domain` gains order entities and status rules
- `src/Ecommerce.Infrastructure` persists and seeds order data
- `src/Ecommerce.Application` exposes a reporting query
- `src/Ecommerce.Api` exposes an authenticated admin reporting endpoint
- `apps/admin` renders the returned metrics in the existing dashboard view

Storefront behavior remains unchanged.

## Data Model

### Order

Add a minimal `Order` aggregate with these fields:

- `Id`
- `OrderNumber`
- `CustomerName`
- `CustomerEmail`
- `Status`
- `Currency`
- `SubtotalAmount`
- `CreatedAtUtc`
- `PaidAtUtc` nullable

The aggregate owns a collection of `OrderItem` rows.

### OrderItem

Add `OrderItem` with these fields:

- `Id`
- `OrderId`
- `ProductId`
- `ProductName`
- `Sku`
- `Quantity`
- `UnitPriceAmount`
- `LineTotalAmount`

`ProductName` and `Sku` are copied onto the order item so reporting remains stable even if catalog data changes later.

### Order Status

Add a minimal order status enum:

- `Pending`
- `Paid`
- `Processing`
- `Shipped`
- `Cancelled`

For revenue reporting in this slice, recognized revenue is the sum of orders in `Paid`, `Processing`, or `Shipped` state. `Cancelled` does not contribute. `Pending` does not contribute until paid.

## Persistence

### ApplicationDbContext

Extend `ApplicationDbContext` with:

- `DbSet<Order>`
- `DbSet<OrderItem>`

### EF Configuration

Add configurations for `Order` and `OrderItem` that:

- enforce required fields
- set reasonable string lengths
- configure one-to-many `Order -> OrderItems`
- index `OrderNumber`
- index `CreatedAtUtc`
- index `Status`
- index `ProductId` on `OrderItem`

### Development Seed

Extend development seed data with a small but realistic order history using the existing seeded products.

Seed enough variety to support dashboard metrics:

- several recent orders across multiple statuses
- at least one cancelled order
- at least one paid/processing/shipped order for revenue totals
- recent order timestamps covering roughly the last 7-30 days

This seed must be idempotent like the existing catalog seed.

## Reporting Endpoint

Add a new authenticated admin endpoint:

- `GET /api/admin/dashboard/metrics`

The endpoint returns a single response object containing all dashboard data needed in one request.

### Response Shape

The response should include:

- `periodRevenue`
- `recognizedRevenue`
- `orderCount`
- `pendingOrderCount`
- `processingOrderCount`
- `shippedOrderCount`
- `cancelledOrderCount`
- `waitlistCount`
- `preOrderCount`
- `criticalInventoryCount`
- `criticalInventoryItems`
- `recentOrders`

### Critical Inventory Item Shape

Each critical inventory row should include:

- `productId`
- `productName`
- `studioName`
- `primaryImageUrl`
- `availableQuantity`

### Recent Order Shape

Each recent order row should include:

- `orderId`
- `orderNumber`
- `createdAtUtc`
- `customerName`
- `totalAmount`
- `currency`
- `status`

## Query Semantics

### Revenue

For this slice:

- `periodRevenue` is the total of recognized orders within the recent reporting window
- `recognizedRevenue` is the total of all recognized orders in the dataset

Use a simple fixed recent window of the last 30 days. The UI can label this clearly. No user-selectable ranges are added yet.

### Inventory Alerts

Critical inventory means products with:

- status `Active`
- available quantity below a fixed threshold

Use threshold `< 5` to match the current dashboard behavior.

Available quantity continues to come from:

- `InventoryItem.OnHandQuantity - InventoryItem.ReservedQuantity`

### Waitlist And PreOrder Counts

These remain catalog-derived counts from the product table, not order-derived counts.

### Recent Orders

Return the most recent 5 orders sorted by `CreatedAtUtc` descending.

## Application Architecture

Add a new query slice, for example:

- `src/Ecommerce.Application/Dashboard/GetAdminDashboardMetrics/`

This slice contains:

- query
- handler
- response DTOs

The handler reads from products, inventory, and orders using `IApplicationDbContext` and returns a single DTO.

No business exception path is expected for the happy path reporting query. If the dataset is empty, return zero values and empty lists.

## API Controller

Add a new controller or endpoint grouping under admin:

- `AdminDashboardController`

The controller should:

- require the existing admin authorization policy through the shared admin controller base
- send the MediatR query
- return `200 OK` with the reporting DTO

No special error mapping beyond the shared admin base is needed for this read endpoint unless unexpected validation is introduced.

## Admin UI

### Service Layer

Add a typed method in `CatalogApiService`:

- `getAdminDashboardMetrics()`

This returns the full dashboard response from `/api/admin/dashboard/metrics`.

### Dashboard Page

Replace placeholder data in `dashboard.page.ts` with real values from the reporting endpoint:

- real revenue metrics
- real inventory alert count and rows
- real recent orders
- real waitlist and preorder counts

The chart can remain a placeholder visual for now unless there is enough time to derive it from seeded data. The important requirement for this slice is that headline metrics, inventory alerts, and order/revenue tables are real.

### Dashboard Text

Any labels implying unsupported precision should be adjusted. For example, if the chart remains static, the rest of the dashboard must not claim it is live-calculated from the backend.

## Error Handling

This slice must continue using the repo convention:

- business failures are represented with `BusinessErrorCode`
- API failures use structured payloads with numeric `code`
- controllers do not inspect exception messages

The dashboard reporting query should normally return a successful zero-state response rather than business failures when data is absent.

## Testing Strategy

### Backend

Add tests for:

- order aggregate creation and totals if domain behavior is introduced
- dashboard reporting query totals and filtering semantics
- critical inventory selection
- recent order ordering
- admin dashboard endpoint response contract

### Frontend

Add or update admin dashboard specs for:

- dashboard uses the admin dashboard metrics endpoint
- revenue and counts render from API data
- recent orders render from API data
- critical inventory rows render from API data

## Delivery Order

1. add order entities and enum
2. wire EF Core persistence and configurations
3. extend seed data with orders
4. add dashboard reporting query and DTOs
5. add admin dashboard endpoint
6. update Angular models and service
7. replace placeholder dashboard data with API data
8. run backend and admin frontend verification

## Expected Result

When this slice is complete:

- the admin dashboard reads real inventory, order, and revenue data from backend endpoints
- metrics are based on persisted internal reporting data, not placeholder arrays
- critical inventory and recent orders are operationally useful
- the repo remains aligned with its existing application layering and business error conventions

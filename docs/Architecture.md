# Architecture

## Goal

Create a Blazor Web App that keeps storefront interactivity portable across server prerendering and client-side WebAssembly execution, while isolating Supabase and Stripe behind server-side services.

## Solution Layout

```text
KzShop.sln
src/
  KzShop.Web/
  KzShop.Web.Client/
  KzShop.Shared/
```

## Projects

### `KzShop.Web`

Server host for the Blazor Web App.

Responsibilities:

- Host root app, static assets, API endpoints, and middleware.
- Configure Razor components and interactive render modes.
- Own Stripe secret-key operations.
- Own Stripe webhook endpoint and signature verification.
- Own Supabase service-role operations.
- Provide server implementations for catalog, cart, order, checkout, and auth-related services.

Expected folders:

```text
src/KzShop.Web/
  Components/
    App.razor
    Layout/
  Endpoints/
    CatalogEndpoints.cs
    CartEndpoints.cs
    CheckoutEndpoints.cs
    WebhookEndpoints.cs
  Infrastructure/
    Supabase/
    Stripe/
  Options/
```

### `KzShop.Web.Client`

Interactive storefront UI.

Responsibilities:

- Contain routable pages for Auto/WebAssembly rendering.
- Render home, category/product listing, product detail, cart, checkout return, and auth pages.
- Call HTTP APIs for catalog, checkout, and authenticated data.
- Use JS interop only for local storage and DOM-specific UI behavior.

Expected folders:

```text
src/KzShop.Web.Client/
  Layout/
  Pages/
    Home.razor
    Category.razor
    ProductDetail.razor
    Cart.razor
    CheckoutReturn.razor
    Auth/
  Components/
    Catalog/
    Cart/
    Shared/
  Services/
  wwwroot/
    css/app.css
    js/archive-ui.js
```

### `KzShop.Shared`

UI-independent contracts and domain values.

Responsibilities:

- DTOs shared by API and client.
- Enums for stable domain values.
- Route constants.
- Validation contracts and simple value helpers.

Expected folders:

```text
src/KzShop.Shared/
  Catalog/
  Cart/
  Checkout/
  Auth/
  Constants/
```

## Render Mode

Default recommendation:

- Use `InteractiveAuto` globally for routes and head outlet.
- Put Auto-rendered pages and layout files in the `.Client` project.
- Use HTTP APIs for data and commands so client-rendered components work after WebAssembly takes over.
- Do not store cart, checkout, or auth-critical state in server circuits.

Strict no-SignalR alternative:

- Use `InteractiveWebAssembly` globally.
- Treat `KzShop.Web` as API/payment/webhook host plus static asset server.

## Dependency Direction

Allowed dependencies:

- `KzShop.Web` references `KzShop.Shared`.
- `KzShop.Web.Client` references `KzShop.Shared`.
- `KzShop.Shared` references no app project.

Do not reference `KzShop.Web` from `KzShop.Web.Client`.

## Data Flow

Catalog reads:

```text
Blazor page -> Client catalog service -> GET /api/products -> Server catalog service -> Supabase
```

Cart:

```text
Anonymous user -> local storage cart
Authenticated user -> local storage cart + Supabase cart sync
```

Checkout:

```text
Cart page -> POST /api/checkout/sessions -> server validates products and totals -> order PendingPayment -> Stripe Checkout Session -> redirect to Stripe
```

Payment completion:

```text
Stripe -> POST /api/stripe/webhook -> verify signature -> update order status
```

## Domain Enums

Keep these in `KzShop.Shared` and map them explicitly to database strings or integers:

```csharp
public enum ProductStatus
{
    InStock = 1,
    PreOrder = 2,
    Waitlist = 3,
    Archived = 4
}

public enum ProductCategory
{
    BatmanDc = 1,
    Anime = 2,
    Other = 3
}

public enum FigureScale
{
    OneThird = 1,
    OneFourth = 2,
    OneSixth = 3,
    NonScale = 4
}

public enum OrderStatus
{
    Draft = 1,
    PendingPayment = 2,
    Paid = 3,
    Failed = 4,
    Cancelled = 5,
    Fulfilled = 6
}
```

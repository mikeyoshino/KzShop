# KzShop Blazor Ecommerce Planning Spec

Date: 2026-05-23

## Goal

Build KzShop as a premium collectible figure ecommerce site for Hot Toys, statues, anime figures, DC/Batman products, and other display collectibles. The application should match the visual direction and motion behavior in `docs/UITemplate.html`, then evolve it into a real Blazor Web App backed by Supabase and Stripe Checkout.

## Current Project State

- The repository currently contains `docs/UITemplate.html`.
- The template is a single-page mock application with:
  - Fixed header with logo, category nav, mobile menu, search icon, cart icon, and cart badge.
  - Home page hero, cinematic strip, franchise/category cards, latest allocations grid, editorial trust section, and footer.
  - Product listing page with breadcrumbs, filters, sort control, product grid, hover quick action, and empty state.
  - Product detail page with gallery, product facts, status badges, price, CTA, trust labels, and specification table.
  - Cart page with line items, quantity updates, totals, and secure checkout CTA.
  - JavaScript behavior for page fade transitions, toast notifications, mobile menu toggling, cart state, cart badge updates, scroll-to-top navigation, and header shadow on scroll.

## Research Summary

Microsoft's current Blazor Web App guidance defines four render modes: Static Server, Interactive Server, Interactive WebAssembly, and Interactive Auto. `InteractiveAuto` initially renders interactively on the server using the Blazor Server hosting model, then downloads and caches the WebAssembly bundle for client-side rendering on later visits. It does not dynamically switch an already-rendered component on the page, and components using Auto must be built from a separate `.Client` project.

Because of that, `InteractiveAuto` is not the same as "HTTP instead of SignalR" for the first interactive visit. The architecture should use HTTP APIs as the data boundary so components work when running on WebAssembly, while still accepting that the first Auto visit can use server interactivity. If a strict "no SignalR/circuit dependency" requirement becomes mandatory, use global `InteractiveWebAssembly` instead of `InteractiveAuto`.

Stripe Checkout is the recommended payment approach for this project because it creates a server-side Checkout Session and redirects the customer to a Stripe-hosted payment page. Stripe recommends fulfilling orders from webhooks, especially `checkout.session.completed`, rather than trusting only the browser redirect.

Supabase can provide fast development for Postgres, Auth, Storage, and Data API access. The C# Supabase client is community-maintained, not official, so the application should hide Supabase behind repository/service interfaces and keep the option to replace it later. Supabase tables exposed through the Data API must use Row Level Security and least-privilege grants.

Sources:
- Microsoft Blazor render modes: https://learn.microsoft.com/en-us/aspnet/core/blazor/components/render-modes?view=aspnetcore-10.0
- Microsoft Blazor Web API calls: https://learn.microsoft.com/en-us/aspnet/core/blazor/call-web-api?view=aspnetcore-10.0
- Stripe Checkout lifecycle: https://docs.stripe.com/payments/checkout/how-checkout-works
- Stripe Checkout Session API: https://docs.stripe.com/api/checkout/sessions/create?lang=dotnet
- Supabase C# client reference: https://supabase.com/docs/reference/csharp/introduction/
- Supabase C# Data API setup: https://supabase.com/docs/reference/csharp/installing

## Recommended Architecture

Use a .NET Blazor Web App solution with global `InteractiveAuto` only if first-load SignalR is acceptable. Configure the app so UI components call application services that can operate in both server and client render locations:

- Server project: hosts the Blazor Web App, minimal API endpoints, Stripe secret operations, webhook endpoints, Supabase service-role operations, and SSR/prerender support.
- Client project: contains routable interactive pages and UI components that need WebAssembly/Auto rendering.
- Shared project: contains DTOs, enums, validation contracts, route constants, and UI-independent models.

Data access should flow through HTTP endpoints for all browser-executed operations. Server-rendered paths may call application services directly during SSR, but those services must follow the same interfaces and authorization rules as the HTTP API.

Recommended solution layout:

```text
KzShop.sln
src/
  KzShop.Web/
    Program.cs
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
  KzShop.Web.Client/
    Program.cs
    Routes.razor
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
    wwwroot/
      js/archive-ui.js
      css/app.css
  KzShop.Shared/
    Catalog/
    Cart/
    Checkout/
    Auth/
    Constants/
docs/
  UITemplate.html
  BlazorEcommercePlanningSpec.md
AGENTS.md
```

## Render Mode Decision

Recommended default:

- Create a Blazor Web App with client interactivity enabled and use `InteractiveAuto` globally for routes and head outlet.
- Keep all interactive pages and layouts that need Auto under the `.Client` project.
- Use HTTP APIs for catalog, cart, checkout, and authenticated user data so client-side rendering does not depend on server memory.
- Do not store cart or checkout state in a SignalR circuit. Use local storage for anonymous cart persistence and Supabase for authenticated carts/orders.

Strict no-SignalR alternative:

- Use global `InteractiveWebAssembly`.
- Treat the server project as an API and payment/webhook host.
- Accept slower first load than Auto, but avoid server circuit interactivity for the storefront.

The implementation should document which mode is chosen in `AGENTS.md` so future agents do not accidentally introduce server-circuit-only state.

## Page Scope

### Shared Layout

- Header: logo, Home, New Drops, Batman / DC, Anime, All Figures, search icon, cart icon with count, and mobile menu.
- Footer: Archive brand text, customer care links, newsletter field, secure checkout labels.
- Transitions: route content should use the same fade-in animation from the template: `opacity 0 -> 1` and slight `translateY(10px) -> 0`.
- Header shadow should appear after vertical scroll.
- Mobile menu should lock body scroll while open.

### Home Page

Home page must contain:

- Feature hero product with:
  - New Drop badge.
  - Edition size.
  - Product name.
  - Short detail copy.
  - Scale detail.
  - Buy/pre-order button leading to product detail.
- Cinematic strip repeating trust messages.
- Category/franchise list:
  - DC / Batman.
  - Anime.
  - Other. The template currently uses Gaming; rename or map this to Other unless the catalog taxonomy keeps Gaming as a child category.
- Latest Allocations section:
  - Recently added stock and new pre-orders.
  - Product cards with image, new/status badges, studio, name, price, and scale.
- Editorial standard/trust section from the template.

### Category / Product Listing Page

Each category route should render a product list:

- `/category/batman-dc`
- `/category/anime`
- `/category/other`
- `/figures`
- `/new-drops`

Required controls:

- Status filter: In Stock, Pre-order, Waitlist.
- Scale filter: 1/3 Scale, 1/4 Scale, 1/6 Scale, Non-Scale.
- Studio filter: Prime Studios, Hot Toys, Dark Art Studios, Tamashii Nations, and future values from database.
- Sort: Newest, Price High to Low, Price Low to High, Name.
- Apply filters button for parity with the template.

Filter state should be reflected in query string parameters so URLs are shareable.

### Product Detail Page

Product page must contain:

- Breadcrumbs.
- Image gallery with thumbnails.
- Status and new badges.
- Studio, product title, SKU/display ID, price.
- Deposit note for pre-order products.
- Description.
- CTA:
  - `Pre-order Now` for pre-order.
  - `Add to Cart` for in-stock.
  - `Join Waitlist` for waitlist.
- Toast notification after adding a product to cart.
- Trust labels such as Secure Ship and Authentic.
- Specification table: franchise, scale, materials, dimensions, estimated release, edition size.

### Cart Page

Cart page must be implemented, not delegated to Stripe:

- Empty state.
- Line items with image, studio, scale, product name, status, quantity controls, remove button, item total.
- Order summary with subtotal, shipping/taxes calculated at checkout note, estimated total.
- Secure checkout button.
- Pre-order deposit messaging when cart includes a pre-order item.

Anonymous cart should live in browser local storage. Authenticated cart should sync to Supabase after login.

### Checkout Page / Stripe Flow

The app should have a checkout review route before redirecting to Stripe:

- `/checkout`: collect or confirm customer email, shipping destination summary, and order review.
- POST `/api/checkout/sessions`: validate cart server-side, create an internal order with status `PendingPayment`, create a Stripe Checkout Session, and return the Stripe redirect URL.
- Stripe-hosted checkout handles payment details.
- `/checkout/success?session_id=...`: show pending confirmation until webhook marks order paid.
- `/checkout/cancelled`: return the user to cart without clearing cart.
- POST `/api/stripe/webhook`: verify Stripe signature and process `checkout.session.completed`.

The server must calculate prices from Supabase/database records, not from client-submitted cart totals.

## Domain Model

Use enums instead of magic strings for all stable categorical values.

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

Core records:

- Product: id, slug, name, description, studio, franchise, category, status, scale, price, currency, edition size, materials, dimensions, estimated release, is new, created date.
- ProductImage: id, product id, url, alt text, sort order, image role.
- CartItem: product id, quantity.
- Order: id, user id nullable, email, status, currency, subtotal, tax, shipping, total, Stripe session id, Stripe payment intent id, created date.
- OrderLine: order id, product id, product snapshot name, unit price, quantity, status snapshot, scale snapshot.
- Profile: Supabase auth user id, display name, email, timestamps.

## Supabase Plan

Use Supabase Postgres as the source of truth for catalog, profiles, orders, and authenticated cart records.

Recommended table names:

- `products`
- `product_images`
- `profiles`
- `carts`
- `cart_items`
- `orders`
- `order_lines`

Security:

- Enable Row Level Security on all tables.
- Public/anonymous users can read active products and images only.
- Authenticated users can read and update their own profile, cart, orders, and order lines.
- Only server-side code using service-role credentials can write catalog data, create paid order records, or update payment status from Stripe webhooks.
- Never expose the Supabase service-role key to the `.Client` project.

## Stripe Plan

Use Stripe Checkout in hosted mode for lower implementation risk.

Server responsibilities:

- Read cart item IDs and quantities.
- Fetch product records from Supabase.
- Reject unavailable products and invalid quantities.
- Build Stripe line items using server-side product names and prices.
- Store an internal pending order before creating the Checkout Session.
- Set `client_reference_id` or metadata to the internal order ID.
- Redirect user to `Session.Url`.
- Verify webhook signatures.
- Mark order paid only from verified webhook events.

Do not accept price, currency, status, or product names from the browser as trusted checkout input.

## UI And Animation Requirements

Preserve the template's visual language:

- White, zinc, black, and deep red accent `#991b1b`.
- `Oswald` for display headings and compact uppercase labels.
- `Inter` for body text.
- Sharp rectangular UI, minimal rounding, dense product cards, strong typography.
- Card hover image zoom with `duration-700`.
- Page fade transition matching `.fade-in`.
- Toast notification animation from bottom-right using `translate-y-full`, `opacity-0`, and `duration-300`.
- Cinematic strip with continuous horizontal slide animation. The current template references `animate-[slide_30s_linear_infinite]` but does not define `@keyframes slide`; implementation must define it.
- Mobile menu open/close behavior and body scroll lock.
- Header shadow on scroll.

Use Tailwind CSS through the ASP.NET build pipeline, not the CDN used by the template.

## JavaScript Boundary

Keep JavaScript small and UI-specific:

- `archive-ui.js` should expose functions for scroll header state, toast display, body scroll locking, and optional route transition helpers.
- Business rules, cart totals, filters, and checkout decisions belong in C# services/components.
- Use JS interop only where browser APIs are necessary, such as local storage and DOM transition helpers.

## Clean Architecture And Coding Rules

Create `AGENTS.md` at the repository root before implementation begins. It should instruct future agents and developers to follow:

- SOLID principles.
- Clean Architecture boundaries.
- DRY without premature abstraction.
- No magic strings for domain values; use enums, constants, route names, and option classes.
- No secrets in source code.
- No Supabase service-role key in client code.
- Server-side validation for checkout.
- Stripe webhook as the payment source of truth.
- Preserve the template's animation and visual details unless explicitly changed.
- No test project is required for this phase per user instruction, but manual verification and `dotnet build` are still required.

## Configuration

Expected configuration keys:

```text
Supabase:Url
Supabase:AnonKey
Supabase:ServiceRoleKey
Stripe:SecretKey
Stripe:WebhookSecret
Stripe:PublishableKey
Checkout:SuccessUrl
Checkout:CancelUrl
```

Use user secrets for local development and environment variables in deployment.

## Implementation Phases

### Phase 1: Project Foundation

- Create Blazor Web App solution with WebAssembly/Auto interactivity support.
- Add Tailwind build setup.
- Add shared enums, constants, DTOs, and route names.
- Add `AGENTS.md`.
- Port layout, footer, base styles, fonts, and JavaScript helpers from `UITemplate.html`.

### Phase 2: Catalog UI With Mock Data

- Implement Home, Category/Product Listing, and Product Detail pages using in-memory mock data matching the template products.
- Preserve route transitions, hover states, toast notification, mobile menu, and header scroll behavior.
- Implement filters and sort against mock data.

### Phase 3: Cart

- Implement cart service, local storage persistence, cart badge, cart page, quantity controls, remove behavior, and pre-order messaging.
- Keep cart calculations in C#.

### Phase 4: Supabase Integration

- Add Supabase configuration and repository interfaces.
- Add catalog read repository.
- Add authenticated profile/cart/order repositories.
- Add SQL schema and RLS policy scripts under `docs/database/` or `infra/supabase/`.
- Replace mock catalog data with Supabase-backed data.

### Phase 5: Auth

- Add login, signup, logout, and session handling.
- Keep public catalog readable without login.
- Sync anonymous cart to authenticated cart after login.

### Phase 6: Checkout

- Add checkout review page.
- Add server checkout endpoint.
- Add Stripe Checkout Session creation.
- Add success and cancelled routes.
- Add webhook endpoint and order status update flow.

### Phase 7: Verification And Polish

- Run `dotnet build`.
- Manually verify desktop and mobile layouts for home, PLP, PDP, cart, checkout, success, and cancelled pages.
- Verify transition animation, toast, mobile menu, cart persistence, filters, sort, and checkout redirect in Stripe test mode.
- Verify Stripe webhook handling through Stripe CLI or dashboard event replay.

## Open Decisions

1. Confirm render mode:
   - Recommended: `InteractiveAuto` with HTTP APIs and no server-circuit state.
   - Alternative: `InteractiveWebAssembly` if avoiding SignalR entirely is required.
2. Confirm product taxonomy:
   - Use `Other` as requested, or keep `Gaming` from the template as a visible category.
3. Confirm whether guest checkout is allowed:
   - Recommended: allow guest checkout with email, then link orders to an account after login if the email matches.
4. Confirm currency:
   - The template uses USD. Keep USD unless the shop needs THB or multi-currency.
5. Confirm product images:
   - Use placeholder images during development, then Supabase Storage or external CDN for real product photography.

## Acceptance Criteria

- Home page visually matches Image #1 and `UITemplate.html` structure.
- Category page visually matches Image #2 and supports status, scale, studio, and sort controls.
- Product detail page visually matches Image #3 and supports add-to-cart/pre-order behavior.
- Cart page is implemented with persistent cart state and checkout CTA.
- Checkout review page creates a Stripe Checkout Session through the server.
- Order paid status is updated only from verified Stripe webhook events.
- Supabase is used for catalog, auth, and order persistence.
- Domain constants use enums or constants instead of repeated magic strings.
- Template animations and notifications are preserved.
- `AGENTS.md` exists and documents project engineering rules.

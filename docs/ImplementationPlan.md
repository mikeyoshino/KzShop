# KzShop Blazor Ecommerce Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build the KzShop collectible figure storefront as a Blazor Web App matching `docs/UITemplate.html`, with cart, Supabase-backed catalog/auth/order persistence, and Stripe Checkout.

**Architecture:** Use `KzShop.Web` for server hosting, APIs, Supabase service-role access, and Stripe; use `KzShop.Web.Client` for interactive storefront pages; use `KzShop.Shared` for DTOs, enums, route constants, and contracts. Prefer `InteractiveAuto` with HTTP APIs and no server-circuit business state.

**Tech Stack:** .NET Blazor Web App, C#, Razor components, Tailwind CSS, JavaScript interop, Supabase Postgres/Auth/Storage, Stripe Checkout.

---

## Task 1: Scaffold Solution

**Files:**
- Create: `KzShop.sln`
- Create: `src/KzShop.Web/KzShop.Web.csproj`
- Create: `src/KzShop.Web.Client/KzShop.Web.Client.csproj`
- Create: `src/KzShop.Shared/KzShop.Shared.csproj`

- [ ] Create a Blazor Web App with WebAssembly/Auto interactivity enabled.
- [ ] Add `KzShop.Shared` as a class library.
- [ ] Add project references so both web projects reference `KzShop.Shared`.
- [ ] Confirm `KzShop.Web.Client` does not reference `KzShop.Web`.
- [ ] Run `dotnet build`.
- [ ] Commit with message `chore: scaffold blazor solution`.

## Task 2: Add Shared Domain Contracts

**Files:**
- Create: `src/KzShop.Shared/Catalog/ProductStatus.cs`
- Create: `src/KzShop.Shared/Catalog/ProductCategory.cs`
- Create: `src/KzShop.Shared/Catalog/FigureScale.cs`
- Create: `src/KzShop.Shared/Checkout/OrderStatus.cs`
- Create: `src/KzShop.Shared/Constants/AppRoutes.cs`
- Create: `src/KzShop.Shared/Catalog/ProductSummaryDto.cs`
- Create: `src/KzShop.Shared/Catalog/ProductDetailDto.cs`
- Create: `src/KzShop.Shared/Cart/CartItemDto.cs`
- Create: `src/KzShop.Shared/Cart/CartDto.cs`
- Create: `src/KzShop.Shared/Checkout/CreateCheckoutSessionRequest.cs`
- Create: `src/KzShop.Shared/Checkout/CreateCheckoutSessionResponse.cs`

- [ ] Add enums exactly as defined in `docs/Architecture.md`.
- [ ] Add route constants for `/`, `/new-drops`, `/category/batman-dc`, `/category/anime`, `/category/other`, `/figures`, `/cart`, `/checkout`, `/checkout/success`, and `/checkout/cancelled`.
- [ ] Add DTOs for catalog cards, product detail, cart items, cart summaries, and checkout session creation.
- [ ] Run `dotnet build`.
- [ ] Commit with message `feat: add shared storefront contracts`.

## Task 3: Configure Tailwind And Base Styling

**Files:**
- Create: `src/KzShop.Web.Client/tailwind.config.js`
- Create: `src/KzShop.Web.Client/package.json`
- Modify: `src/KzShop.Web.Client/wwwroot/css/app.css`

- [ ] Configure Tailwind content paths for Razor, HTML, and C# files.
- [ ] Add `Inter` and `Oswald` font family names to Tailwind theme.
- [ ] Add accent colors `#991b1b` and `#7f1d1d`.
- [ ] Add scrollbar styles, `.fade-in`, and `@keyframes slide`.
- [ ] Replace CDN-only assumptions from `docs/UITemplate.html` with build-pipeline CSS.
- [ ] Run Tailwind build command defined in `package.json`.
- [ ] Run `dotnet build`.
- [ ] Commit with message `feat: configure storefront styling`.

## Task 4: Port Shared Layout

**Files:**
- Modify: `src/KzShop.Web/Components/App.razor`
- Create or modify: `src/KzShop.Web.Client/Layout/MainLayout.razor`
- Create: `src/KzShop.Web.Client/Components/Shared/SiteHeader.razor`
- Create: `src/KzShop.Web.Client/Components/Shared/SiteFooter.razor`
- Create: `src/KzShop.Web.Client/wwwroot/js/archive-ui.js`

- [ ] Configure global render mode according to `docs/Architecture.md`.
- [ ] Port header markup from `docs/UITemplate.html`.
- [ ] Add nav links for Home, New Drops, Batman / DC, Anime, and All Figures.
- [ ] Port footer markup from `docs/UITemplate.html`.
- [ ] Add JS interop for body scroll lock, header shadow, and toast.
- [ ] Verify mobile menu opens, closes, and locks page scroll.
- [ ] Run `dotnet build`.
- [ ] Commit with message `feat: port archive layout`.

## Task 5: Build Catalog UI With Mock Data

**Files:**
- Create: `src/KzShop.Web.Client/Services/MockCatalogService.cs`
- Create: `src/KzShop.Web.Client/Pages/Home.razor`
- Create: `src/KzShop.Web.Client/Pages/Category.razor`
- Create: `src/KzShop.Web.Client/Pages/ProductDetail.razor`
- Create: `src/KzShop.Web.Client/Components/Catalog/ProductCard.razor`
- Create: `src/KzShop.Web.Client/Components/Catalog/StatusBadge.razor`
- Create: `src/KzShop.Web.Client/Components/Catalog/ProductFilters.razor`

- [ ] Add mock products from `docs/UITemplate.html`.
- [ ] Build home page sections listed in `docs/ui/TemplateMapping.md`.
- [ ] Build category/product listing page with status, scale, studio, and sort controls.
- [ ] Build product detail page with gallery, CTA, trust labels, and specification table.
- [ ] Preserve hover zoom, route fade, badges, and typography.
- [ ] Run `dotnet build`.
- [ ] Commit with message `feat: build catalog storefront pages`.

## Task 6: Build Cart

**Files:**
- Create: `src/KzShop.Web.Client/Services/CartService.cs`
- Create: `src/KzShop.Web.Client/Services/BrowserStorageService.cs`
- Create: `src/KzShop.Web.Client/Pages/Cart.razor`
- Create: `src/KzShop.Web.Client/Components/Cart/CartLineItem.razor`
- Create: `src/KzShop.Web.Client/Components/Cart/CartSummary.razor`

- [ ] Persist anonymous cart in local storage.
- [ ] Update cart badge after add, remove, and quantity changes.
- [ ] Show toast after adding a product.
- [ ] Implement empty cart state.
- [ ] Implement line item quantity controls, remove button, subtotal, and estimated total.
- [ ] Add pre-order deposit messaging when applicable.
- [ ] Run `dotnet build`.
- [ ] Commit with message `feat: add persistent storefront cart`.

## Task 7: Add API Endpoints And Supabase Infrastructure

**Files:**
- Create: `src/KzShop.Web/Endpoints/CatalogEndpoints.cs`
- Create: `src/KzShop.Web/Infrastructure/Supabase/SupabaseOptions.cs`
- Create: `src/KzShop.Web/Infrastructure/Supabase/SupabaseCatalogRepository.cs`
- Create: `src/KzShop.Web/Infrastructure/Supabase/SupabaseOrderRepository.cs`
- Create: `docs/database/schema.sql`
- Create: `docs/database/seed.sql`

- [ ] Add typed options for Supabase config keys from `docs/Configuration.md`.
- [ ] Add catalog API endpoints from `docs/api/ApiContracts.md`.
- [ ] Add Supabase repository interfaces and implementations.
- [ ] Add SQL schema and seed data matching `docs/database/SupabaseSchema.md`.
- [ ] Replace mock catalog reads with HTTP API calls.
- [ ] Run `dotnet build`.
- [ ] Commit with message `feat: add supabase catalog api`.

## Task 8: Add Auth And Cart Sync

**Files:**
- Create: `src/KzShop.Web.Client/Pages/Auth/Login.razor`
- Create: `src/KzShop.Web.Client/Pages/Auth/Register.razor`
- Create: `src/KzShop.Web.Client/Services/AuthSessionService.cs`
- Create: `src/KzShop.Web/Endpoints/CartEndpoints.cs`

- [ ] Add login, signup, logout, and session handling.
- [ ] Keep catalog routes public.
- [ ] Sync anonymous cart to authenticated cart after login.
- [ ] Ensure authenticated users can only read and update their own cart.
- [ ] Run `dotnet build`.
- [ ] Commit with message `feat: add auth and cart sync`.

## Task 9: Add Stripe Checkout

**Files:**
- Create: `src/KzShop.Web/Endpoints/CheckoutEndpoints.cs`
- Create: `src/KzShop.Web/Endpoints/WebhookEndpoints.cs`
- Create: `src/KzShop.Web/Infrastructure/Stripe/StripeOptions.cs`
- Create: `src/KzShop.Web/Infrastructure/Stripe/StripeCheckoutService.cs`
- Create: `src/KzShop.Web.Client/Pages/Checkout.razor`
- Create: `src/KzShop.Web.Client/Pages/CheckoutReturn.razor`

- [ ] Add Stripe config options from `docs/Configuration.md`.
- [ ] Implement `POST /api/checkout/sessions`.
- [ ] Server-validate products, statuses, quantities, and totals.
- [ ] Create pending order before Stripe session creation.
- [ ] Redirect user to Stripe-hosted Checkout.
- [ ] Implement success and cancelled return pages.
- [ ] Implement Stripe webhook signature verification and `checkout.session.completed` processing.
- [ ] Run `dotnet build`.
- [ ] Commit with message `feat: add stripe checkout flow`.

## Task 10: Manual Verification

**Files:**
- Modify docs only if implementation discoveries require clarification.

- [ ] Run `dotnet build`.
- [ ] Verify home page desktop and mobile layout.
- [ ] Verify category filters, sort, empty state, and product cards.
- [ ] Verify product detail gallery, CTA labels, add-to-cart toast, and specifications.
- [ ] Verify cart persistence, quantity changes, remove item, empty state, and checkout CTA.
- [ ] Verify checkout redirect in Stripe test mode.
- [ ] Verify webhook event updates order status.
- [ ] Commit final fixes with message `fix: polish storefront verification issues`.

# KzShop Agent Instructions

These instructions apply to the entire repository.

## Product Goal

KzShop is a premium ecommerce storefront for Hot Toys, statues, anime figures, DC/Batman collectibles, and other display figures. The app must preserve the visual language and animation behavior from `docs/UITemplate.html` while becoming a production-shaped Blazor Web App backed by Supabase and Stripe Checkout.

## Source Of Truth

Read these before implementation:

- `docs/BlazorEcommercePlanningSpec.md`
- `docs/ImplementationPlan.md`
- `docs/Architecture.md`
- `docs/ui/TemplateMapping.md`
- `docs/api/ApiContracts.md`
- `docs/database/SupabaseSchema.md`
- `docs/payments/StripeFlow.md`
- `docs/Configuration.md`
- `docs/UITemplate.html`

## Architecture Rules

- Use a Blazor Web App with a server project, a `.Client` project, and a shared project.
- Recommended render mode is `InteractiveAuto` with HTTP APIs and no server-circuit-owned business state.
- If strict no-SignalR behavior is required, switch the storefront to `InteractiveWebAssembly` and keep the server as API/payment/webhook host.
- Keep browser-executed data access behind HTTP APIs.
- Keep Stripe secret operations, webhook handling, and Supabase service-role operations in the server project only.
- Do not expose service-role secrets, Stripe secret keys, or webhook secrets to the `.Client` project.

## Coding Rules

- Follow SOLID and Clean Architecture boundaries.
- Keep files focused by responsibility.
- Prefer explicit DTOs, interfaces, enums, and option classes over loose dictionaries or repeated strings.
- No magic strings for domain values. Use enums for `ProductStatus`, `ProductCategory`, `FigureScale`, and `OrderStatus`.
- No secrets in source code or committed config.
- Do not trust client-submitted price, currency, product name, status, or totals.
- Server-side checkout must recalculate totals from stored product records.
- Stripe webhook events are the source of truth for paid order state.
- The user has requested no test project for this phase. Still run `dotnet build` and perform manual verification before claiming completion.

## UI Rules

- Preserve `docs/UITemplate.html` behavior unless the user explicitly changes it.
- Use Tailwind through the ASP.NET build pipeline, not the CDN script from the template.
- Keep the visual style sharp, dense, premium, and mostly rectangular.
- Use `Oswald` for display labels/headings and `Inter` for body text.
- Preserve the page fade, product image hover zoom, toast animation, mobile menu body scroll lock, and header shadow on scroll.
- Define the missing `@keyframes slide` needed by the cinematic strip.
- JavaScript should be limited to browser/DOM concerns: local storage, toast, scroll state, body scroll lock, and transition helpers.

## Verification Rules

Before finalizing implementation work:

- Run `dotnet build`.
- Manually verify home, category listing, product detail, cart, checkout review, success, and cancelled pages.
- Verify desktop and mobile layouts.
- Verify filters, sort, cart persistence, quantity changes, toast notifications, mobile menu, and route transition animation.
- Verify Stripe Checkout redirect with test mode.
- Verify Stripe webhook handling with Stripe CLI or dashboard event replay when payment work is implemented.

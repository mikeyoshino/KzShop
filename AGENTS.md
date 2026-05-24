# KzShop Agent Instructions

These instructions apply to the entire repository.

## Product Goal

KzShop is a premium ecommerce storefront for Hot Toys, statues, anime figures, DC/Batman collectibles, and other display figures. The app must preserve the visual language and animation behavior from `docs/UITemplate.html` while becoming a production-shaped commerce app.

## Source Of Truth

Read these before implementation when they are relevant to the requested work:

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

- Follow SOLID and Clean Architecture boundaries.
- Keep files focused by responsibility.
- Prefer explicit DTOs, interfaces, enums, and option classes over loose dictionaries or repeated strings.
- No secrets in source code or committed config.
- Do not expose service-role secrets, Stripe secret keys, or webhook secrets to browser-executed code.
- Do not trust client-submitted price, currency, product name, status, or totals.
- Server-side checkout must recalculate totals from stored product records.
- Stripe webhook events are the source of truth for paid order state.

## Business Error Handling (Mandatory)

Do not use `InvalidOperationException` or other generic exceptions for expected business logic failures.

Required approach:

1. Represent business failures using enum error codes.
2. Return structured results from application handlers, for example `Result<T>` with `BusinessErrorCode`.
3. Map business error codes to HTTP status codes in API controllers.
4. Use exceptions only for unexpected system failures such as infrastructure faults, programming defects, or unrecoverable conditions.

### Required outcomes

- Tests must assert business error codes, not exception message strings, for business-rule failures.
- Controllers must not parse exception text to decide HTTP responses.
- New slices must follow this convention by default.

### Minimum business error codes

- `DuplicateSlug`
- `DuplicateName`
- `DuplicateSku`
- `CategoryNotFound`
- `StudioNotFound`
- `ProductNotFound`
- `InventoryNotFound`
- `InvalidStatusTransition`
- `ValidationFailed`

Add new enum values when new business failure types are introduced.

## Angular File Convention (Mandatory)

For Angular UI work, every new component or feature must include standard Angular files, not only a single page file.

Required minimum per component:

1. `*.component.ts` or `*.page.ts` if this repo keeps page naming.
2. `*.component.html` unless inline template is explicitly justified.
3. `*.component.css`/`*.component.scss` or a shared style reference.
4. `*.component.spec.ts` test file.

Rules:

- Do not ship new Angular components without a corresponding `.spec.ts`.
- Prefer separate template/style files for medium and large components.
- Keep naming consistent inside each feature folder, for example `feature-name/`.

## UI Rules

- Preserve `docs/UITemplate.html` behavior unless the user explicitly changes it.
- Keep the visual style sharp, dense, premium, and mostly rectangular.
- Use `Oswald` for display labels/headings and `Inter` for body text.
- Preserve page fade, product image hover zoom, toast animation, mobile menu body scroll lock, and header shadow on scroll where applicable.
- JavaScript should be limited to browser/DOM concerns such as local storage, toast, scroll state, body scroll lock, and transition helpers.

## Verification Rules

Before finalizing implementation work:

- Run the relevant backend and frontend tests for the touched areas.
- Run `dotnet build` for backend changes.
- Verify desktop and mobile layouts for UI changes.
- Verify filters, sort, cart persistence, quantity changes, toast notifications, mobile menu, and route transition animation when those areas are touched.
- Verify Stripe Checkout redirect and webhook handling when payment work is implemented.

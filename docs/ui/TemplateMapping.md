# UI Template Mapping

This document maps `docs/UITemplate.html` to Blazor components. Preserve visual behavior and motion unless the user explicitly changes it.

## Global Style

Template sources:

- Tailwind CDN config.
- Google fonts: `Inter` and `Oswald`.
- Font Awesome icons.
- Custom scrollbar.
- `.fade-in` animation.

Blazor target:

- Tailwind build pipeline in `KzShop.Web.Client`.
- Font loading in app head or CSS import.
- Prefer a package or static icon strategy for icons. Match the visual result of Font Awesome if changing icon source.
- Define `.fade-in` and `@keyframes fadeIn`.
- Define missing `@keyframes slide` for cinematic strip.

## Layout

### Header

Template IDs/classes:

- `#main-header`
- `#mobile-menu`
- `#cart-count`

Blazor components:

- `Components/Shared/SiteHeader.razor`
- `wwwroot/js/archive-ui.js`

Behavior:

- Fixed top header.
- Header shadow after scroll position is greater than `20`.
- Mobile hamburger opens full-screen mobile menu.
- Mobile menu close button hides menu.
- Body scroll is locked while menu is open.
- Cart badge is hidden at zero and shows total quantity above zero.

### Footer

Blazor component:

- `Components/Shared/SiteFooter.razor`

Content:

- Archive brand block.
- Customer Care links.
- Newsletter email field.
- Secure checkout label with payment icons.

## Home Page

Blazor page:

- `Pages/Home.razor`

Components:

- `Components/Catalog/ProductCard.razor`
- `Components/Catalog/StatusBadge.razor`

Sections:

1. Hero
   - Full viewport-ish black hero.
   - Background product image with opacity.
   - Left gradient overlay.
   - New Drop badge.
   - Edition size.
   - Product name and secondary grey line.
   - Short copy with red left border.
   - Pre-order button to PDP.

2. Cinematic strip
   - Border top and bottom.
   - Repeating uppercase text.
   - Continuous horizontal animation.

3. Franchises
   - Section title and View All link.
   - Three image cards.
   - Required visible categories: DC / Batman, Anime, Other.
   - Template has Gaming; map Gaming visuals to Other unless taxonomy changes.
   - Hover image scale and overlay darkening.

4. Latest Allocations
   - Grey section background.
   - Latest product cards.
   - New/status badges.
   - Studio, product name, price, scale.

5. Editorial
   - "The Archive Standard" label.
   - "Curated for the Discerning Eye."
   - Trust blocks for Guaranteed Authentic and Collector Grade Shipping.
   - Framed image on the right.

## Product Listing Page

Blazor page:

- `Pages/Category.razor`

Components:

- `Components/Catalog/ProductFilters.razor`
- `Components/Catalog/ProductCard.razor`
- `Components/Catalog/StatusBadge.razor`

Routes:

- `/new-drops`
- `/category/batman-dc`
- `/category/anime`
- `/category/other`
- `/figures`

Template behavior:

- Breadcrumbs.
- Page title and item count.
- Left sidebar filters.
- Product grid.
- Sort control.
- Empty state.
- Product card hover quick action.

Required filters:

- Status: In Stock, Pre-order, Waitlist.
- Scale: 1/3 Scale, 1/4 Scale, 1/6 Scale, Non-Scale.
- Studio: dynamic from available products.

Required sorting:

- Newest.
- Price High to Low.
- Price Low to High.
- Name.

State:

- Reflect filters and sort in query string parameters.

## Product Detail Page

Blazor page:

- `Pages/ProductDetail.razor`

Components:

- `Components/Catalog/StatusBadge.razor`

Route:

- `/products/{slug}`

Template behavior:

- Breadcrumbs.
- Thumbnail gallery.
- Main image.
- Status and New badges.
- Studio label.
- Display ID/SKU badge.
- Product title.
- Price.
- Pre-order deposit note when status is PreOrder.
- Description.
- CTA button:
  - PreOrder: `Pre-order Now`
  - InStock: `Add to Cart`
  - Waitlist: `Join Waitlist`
- CTA disabled for Waitlist unless waitlist signup is implemented.
- Toast after add to cart.
- Secure Ship and Authentic labels.
- Specification table.

## Cart Page

Blazor page:

- `Pages/Cart.razor`

Components:

- `Components/Cart/CartLineItem.razor`
- `Components/Cart/CartSummary.razor`

Template behavior:

- Title: `Requisition Log`.
- Empty state with return button.
- Line item list with remove button.
- Quantity controls.
- Item total.
- Order summary.
- Secure checkout CTA.
- Terms/pre-order note.

Implementation additions:

- Persist cart in local storage.
- Sync authenticated cart to Supabase.
- Keep prices from catalog records, not local storage values.

## Checkout Pages

Blazor pages:

- `Pages/Checkout.razor`
- `Pages/CheckoutReturn.razor`

Routes:

- `/checkout`
- `/checkout/success`
- `/checkout/cancelled`

Required behavior:

- `/checkout` reviews cart and email before Stripe redirect.
- Success page shows order state from server.
- Cancelled page returns user to cart without clearing cart.

## Toast

Template function:

- `showToast(message)`

Blazor target:

- JS function in `archive-ui.js`.

Behavior:

- Fixed bottom-right.
- Dark background.
- Check icon.
- Starts with `translate-y-full opacity-0`.
- Animates in after a short delay.
- Animates out after 3 seconds.
- Removes DOM node after animation.

## Route Transition

Template behavior:

- On navigation, container removes and re-adds `.fade-in`.
- Window scrolls to top.

Blazor target:

- Use route/layout hook or JS interop helper to apply `.fade-in` to the main content wrapper.
- Scroll to top on navigation.

## Visual QA Checklist

- Home resembles Image #1.
- Category listing resembles Image #2.
- Product detail resembles Image #3.
- Footer and header match across pages.
- No text overlaps on mobile or desktop.
- Product cards keep stable aspect ratios.
- Buttons keep text inside bounds.
- Mobile menu covers viewport and closes correctly.

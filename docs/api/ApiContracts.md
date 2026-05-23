# API Contracts

All browser-executed catalog, cart, checkout, and authenticated data access should go through HTTP APIs. Server endpoints must validate authorization and never trust client-submitted prices or totals.

Base path: `/api`

## Catalog

### `GET /api/products`

Returns product cards for category pages, latest allocations, new drops, and all figures.

Query parameters:

| Name | Type | Required | Notes |
| --- | --- | --- | --- |
| `category` | string | no | `batman-dc`, `anime`, `other` |
| `status` | string[] | no | `in-stock`, `pre-order`, `waitlist` |
| `scale` | string[] | no | `1-3`, `1-4`, `1-6`, `non-scale` |
| `studio` | string[] | no | Studio slug or display name normalized server-side |
| `newOnly` | bool | no | Used by New Drops |
| `sort` | string | no | `newest`, `price-desc`, `price-asc`, `name` |
| `page` | int | no | Default `1` |
| `pageSize` | int | no | Default `24`, max `60` |

Response:

```json
{
  "items": [
    {
      "id": "00000000-0000-0000-0000-000000000001",
      "slug": "the-dark-knight-premium-format",
      "name": "The Dark Knight: Premium Format",
      "studio": "Prime Studios",
      "franchise": "DC Comics",
      "category": "BatmanDc",
      "status": "PreOrder",
      "scale": "OneFourth",
      "price": 650.00,
      "currency": "USD",
      "isNew": true,
      "primaryImageUrl": "https://placehold.co/800x1000/1a1a1a/ffffff?text=Dark+Knight+1/4",
      "primaryImageAlt": "Dark Knight 1/4"
    }
  ],
  "page": 1,
  "pageSize": 24,
  "totalItems": 1
}
```

### `GET /api/products/{slug}`

Returns product detail for PDP.

Response:

```json
{
  "id": "00000000-0000-0000-0000-000000000001",
  "slug": "the-dark-knight-premium-format",
  "name": "The Dark Knight: Premium Format",
  "description": "A cinematic masterpiece capturing the brooding essence of Gotham's protector.",
  "studio": "Prime Studios",
  "franchise": "DC Comics",
  "category": "BatmanDc",
  "status": "PreOrder",
  "scale": "OneFourth",
  "price": 650.00,
  "currency": "USD",
  "editionSize": 1000,
  "materials": "Polystone, Fabric, PVC",
  "dimensions": "H: 24\" x W: 12\" x D: 10\"",
  "estimatedRelease": "Est. Q4 2026",
  "isNew": true,
  "images": [
    {
      "url": "https://placehold.co/800x1000/1a1a1a/ffffff?text=Dark+Knight+1/4",
      "alt": "Dark Knight 1/4",
      "sortOrder": 1
    }
  ]
}
```

Errors:

- `404 Not Found` if product slug does not exist or product is archived.

### `GET /api/catalog/filter-options`

Returns filter options derived from active products.

Response:

```json
{
  "statuses": ["InStock", "PreOrder", "Waitlist"],
  "scales": ["OneThird", "OneFourth", "OneSixth", "NonScale"],
  "studios": ["Prime Studios", "Hot Toys", "Dark Art Studios", "Tamashii Nations"]
}
```

## Cart

Cart APIs are only needed for authenticated cart sync. Anonymous cart lives in local storage.

### `GET /api/cart`

Requires authenticated user.

Response:

```json
{
  "items": [
    {
      "productId": "00000000-0000-0000-0000-000000000001",
      "slug": "the-dark-knight-premium-format",
      "name": "The Dark Knight: Premium Format",
      "studio": "Prime Studios",
      "scale": "OneFourth",
      "status": "PreOrder",
      "quantity": 1,
      "unitPrice": 650.00,
      "currency": "USD",
      "imageUrl": "https://placehold.co/800x1000/1a1a1a/ffffff?text=Dark+Knight+1/4"
    }
  ],
  "subtotal": 650.00,
  "currency": "USD"
}
```

### `PUT /api/cart`

Requires authenticated user. Replaces the authenticated user's cart with the submitted product IDs and quantities.

Request:

```json
{
  "items": [
    {
      "productId": "00000000-0000-0000-0000-000000000001",
      "quantity": 1
    }
  ]
}
```

Response: same shape as `GET /api/cart`.

Validation:

- Quantity must be between `1` and `10`.
- Product must exist and must not be archived.
- Server recalculates prices.

## Checkout

### `POST /api/checkout/sessions`

Creates an internal pending order and a Stripe Checkout Session.

Request:

```json
{
  "email": "customer@example.com",
  "items": [
    {
      "productId": "00000000-0000-0000-0000-000000000001",
      "quantity": 1
    }
  ],
  "successUrl": "https://localhost:5001/checkout/success?session_id={CHECKOUT_SESSION_ID}",
  "cancelUrl": "https://localhost:5001/checkout/cancelled"
}
```

Response:

```json
{
  "orderId": "10000000-0000-0000-0000-000000000001",
  "checkoutUrl": "https://checkout.stripe.com/c/pay/cs_test_..."
}
```

Validation:

- Email is required for guest checkout.
- Cart must include at least one item.
- Product IDs and quantities are the only trusted client cart fields.
- Product names, prices, statuses, currency, and totals are loaded server-side.
- Waitlist and archived products cannot be checked out.

Errors:

- `400 Bad Request` for invalid cart or email.
- `409 Conflict` if product status changed and item is no longer purchasable.
- `500 Internal Server Error` only for unexpected failures after logging a safe error.

## Stripe Webhook

### `POST /api/stripe/webhook`

Receives Stripe events. This endpoint must read the raw request body and verify the Stripe signature with `Stripe:WebhookSecret`.

Supported events:

- `checkout.session.completed`: mark order `Paid`, store Stripe session ID and payment intent ID.
- `checkout.session.expired`: mark order `Cancelled` if it is still `PendingPayment`.

Response:

```json
{
  "received": true
}
```

Rules:

- Do not require a logged-in user.
- Do not skip signature verification.
- Use the internal order ID from Stripe metadata or `client_reference_id`.
- Make processing idempotent. If the order is already `Paid`, return success without changing totals.

## Checkout Return

### `GET /api/orders/by-checkout-session/{sessionId}`

Used by the success page to display order state after redirect.

Response:

```json
{
  "orderId": "10000000-0000-0000-0000-000000000001",
  "status": "Paid",
  "total": 650.00,
  "currency": "USD"
}
```

Rules:

- If the webhook has not processed yet, return `PendingPayment`.
- Do not mark paid from this endpoint.

# Stripe Checkout Flow

Use Stripe Checkout in hosted mode. KzShop owns the cart, order records, and product catalog. Stripe owns payment collection.

## Required Configuration

- `Stripe:SecretKey`
- `Stripe:WebhookSecret`
- `Stripe:PublishableKey`
- `Checkout:SuccessUrl`
- `Checkout:CancelUrl`

## Flow

1. Customer reviews cart in KzShop.
2. Customer opens `/checkout`.
3. KzShop collects or confirms email.
4. Client posts product IDs and quantities to `POST /api/checkout/sessions`.
5. Server loads products from Supabase.
6. Server rejects invalid, archived, waitlist-only, or unavailable products.
7. Server calculates subtotal, tax, shipping, and total.
8. Server creates an order with status `PendingPayment`.
9. Server creates Stripe Checkout Session in `payment` mode.
10. Server stores Stripe Checkout Session ID on the order.
11. Client redirects browser to `checkoutUrl`.
12. Stripe collects payment.
13. Stripe redirects browser to success or cancelled URL.
14. Stripe sends webhook to `POST /api/stripe/webhook`.
15. Server verifies signature.
16. Server marks the order `Paid` only after `checkout.session.completed`.

## Checkout Session Rules

Use server-side product data for line items:

- Product name.
- Unit amount.
- Currency.
- Quantity.

Use `client_reference_id` or metadata for the internal order ID.

Recommended metadata:

```text
order_id
user_id
cart_item_count
```

Do not put sensitive data in Stripe metadata.

## Pre-Orders

Initial implementation can charge full listed product price for pre-orders unless the user later requests deposit-only checkout.

If deposit checkout is added later:

- Add explicit product deposit fields.
- Add order line fields for deposit amount and remaining balance.
- Update customer-facing copy.
- Update Stripe line items to charge deposit amount.

## Webhook Handling

Endpoint:

```text
POST /api/stripe/webhook
```

Required events:

- `checkout.session.completed`
- `checkout.session.expired`

Rules:

- Read raw request body.
- Verify Stripe signature with `Stripe:WebhookSecret`.
- Return success for duplicate already-processed events.
- Never mark an order paid from the browser success redirect.
- Log unknown events and return success unless a retry is needed.

## Order Status Transitions

Allowed transitions:

```text
Draft -> PendingPayment
PendingPayment -> Paid
PendingPayment -> Cancelled
PendingPayment -> Failed
Paid -> Fulfilled
```

Disallowed transitions:

```text
Paid -> PendingPayment
Paid -> Cancelled
Fulfilled -> Paid
```

## Success Page

Route:

```text
/checkout/success?session_id={CHECKOUT_SESSION_ID}
```

Behavior:

- Query server for order by checkout session ID.
- Show `PendingPayment` if webhook has not arrived.
- Show paid confirmation only after server order status is `Paid`.
- Do not clear cart until order is `Paid`.

## Cancelled Page

Route:

```text
/checkout/cancelled
```

Behavior:

- Tell customer checkout was cancelled.
- Keep cart intact.
- Provide button back to cart.

## Manual Verification

- Create a Stripe test checkout session.
- Complete payment with a Stripe test card.
- Confirm browser reaches success page.
- Confirm webhook changes order status to `Paid`.
- Confirm cancelled checkout keeps cart contents.
- Replay webhook and confirm idempotent handling.

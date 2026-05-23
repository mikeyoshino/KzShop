# Supabase Schema

Use Supabase Postgres for catalog, profile, authenticated cart, order, and order line persistence. Enable Row Level Security on all app tables.

## Enum Storage

Store enum-like values as text in the database for readability, and map them explicitly to C# enums.

Allowed values:

- Product status: `in_stock`, `pre_order`, `waitlist`, `archived`
- Product category: `batman_dc`, `anime`, `other`
- Figure scale: `one_third`, `one_fourth`, `one_sixth`, `non_scale`
- Order status: `draft`, `pending_payment`, `paid`, `failed`, `cancelled`, `fulfilled`

## Tables

### `products`

| Column | Type | Required | Notes |
| --- | --- | --- | --- |
| `id` | `uuid` | yes | Primary key, default `gen_random_uuid()` |
| `slug` | `text` | yes | Unique, URL-safe |
| `name` | `text` | yes | Display name |
| `description` | `text` | yes | PDP description |
| `studio` | `text` | yes | Prime Studios, Hot Toys, etc. |
| `franchise` | `text` | yes | DC Comics, Berserk, etc. |
| `category` | `text` | yes | Enum text |
| `status` | `text` | yes | Enum text |
| `scale` | `text` | yes | Enum text |
| `price_cents` | `integer` | yes | Store money as integer cents |
| `currency` | `text` | yes | Default `USD` |
| `edition_size` | `integer` | no | Limited edition count |
| `materials` | `text` | no | Specification |
| `dimensions` | `text` | no | Specification |
| `estimated_release` | `text` | no | Example `Est. Q4 2026` |
| `is_new` | `boolean` | yes | Default `false` |
| `created_at` | `timestamptz` | yes | Default `now()` |
| `updated_at` | `timestamptz` | yes | Default `now()` |

Indexes:

- Unique index on `slug`.
- Index on `category`.
- Index on `status`.
- Index on `scale`.
- Index on `studio`.
- Index on `created_at desc`.

### `product_images`

| Column | Type | Required | Notes |
| --- | --- | --- | --- |
| `id` | `uuid` | yes | Primary key |
| `product_id` | `uuid` | yes | FK to `products.id` |
| `url` | `text` | yes | Public image URL |
| `alt_text` | `text` | yes | Accessible alt text |
| `sort_order` | `integer` | yes | Gallery order |
| `image_role` | `text` | yes | `primary`, `gallery`, `detail` |
| `created_at` | `timestamptz` | yes | Default `now()` |

Indexes:

- Index on `product_id`.
- Unique index on `product_id, sort_order`.

### `profiles`

| Column | Type | Required | Notes |
| --- | --- | --- | --- |
| `id` | `uuid` | yes | Primary key, same as Supabase auth user ID |
| `email` | `text` | yes | User email |
| `display_name` | `text` | no | Optional display name |
| `created_at` | `timestamptz` | yes | Default `now()` |
| `updated_at` | `timestamptz` | yes | Default `now()` |

### `carts`

| Column | Type | Required | Notes |
| --- | --- | --- | --- |
| `id` | `uuid` | yes | Primary key |
| `user_id` | `uuid` | yes | FK to `profiles.id` |
| `created_at` | `timestamptz` | yes | Default `now()` |
| `updated_at` | `timestamptz` | yes | Default `now()` |

Indexes:

- Unique index on `user_id`.

### `cart_items`

| Column | Type | Required | Notes |
| --- | --- | --- | --- |
| `id` | `uuid` | yes | Primary key |
| `cart_id` | `uuid` | yes | FK to `carts.id` |
| `product_id` | `uuid` | yes | FK to `products.id` |
| `quantity` | `integer` | yes | Check `quantity between 1 and 10` |
| `created_at` | `timestamptz` | yes | Default `now()` |
| `updated_at` | `timestamptz` | yes | Default `now()` |

Indexes:

- Unique index on `cart_id, product_id`.

### `orders`

| Column | Type | Required | Notes |
| --- | --- | --- | --- |
| `id` | `uuid` | yes | Primary key |
| `user_id` | `uuid` | no | Nullable for guest checkout |
| `email` | `text` | yes | Checkout email |
| `status` | `text` | yes | Order status enum text |
| `currency` | `text` | yes | Default `USD` |
| `subtotal_cents` | `integer` | yes | Server-calculated |
| `tax_cents` | `integer` | yes | Default `0` until tax is implemented |
| `shipping_cents` | `integer` | yes | Default `0` until shipping is implemented |
| `total_cents` | `integer` | yes | Server-calculated |
| `stripe_checkout_session_id` | `text` | no | Stripe session ID |
| `stripe_payment_intent_id` | `text` | no | Stripe payment intent ID |
| `created_at` | `timestamptz` | yes | Default `now()` |
| `updated_at` | `timestamptz` | yes | Default `now()` |

Indexes:

- Index on `user_id`.
- Index on `email`.
- Unique index on `stripe_checkout_session_id` where not null.

### `order_lines`

| Column | Type | Required | Notes |
| --- | --- | --- | --- |
| `id` | `uuid` | yes | Primary key |
| `order_id` | `uuid` | yes | FK to `orders.id` |
| `product_id` | `uuid` | yes | FK to `products.id` |
| `product_name` | `text` | yes | Snapshot at checkout |
| `studio` | `text` | yes | Snapshot at checkout |
| `status` | `text` | yes | Snapshot at checkout |
| `scale` | `text` | yes | Snapshot at checkout |
| `unit_price_cents` | `integer` | yes | Snapshot at checkout |
| `quantity` | `integer` | yes | Check `quantity between 1 and 10` |
| `line_total_cents` | `integer` | yes | Snapshot at checkout |
| `created_at` | `timestamptz` | yes | Default `now()` |

Indexes:

- Index on `order_id`.
- Index on `product_id`.

## RLS Policy Intent

### Public Catalog

- `anon` and `authenticated` can select products where `status <> 'archived'`.
- `anon` and `authenticated` can select product images for visible products.

### Profiles

- Authenticated users can select and update only their own profile.
- Profile inserts should happen through a trusted server flow or Supabase auth trigger.

### Carts

- Authenticated users can select, insert, update, and delete their own cart and cart items.
- Cart item access must join through the user's cart.

### Orders

- Authenticated users can select orders where `orders.user_id = auth.uid()`.
- Guest users cannot query order history without a server-mediated lookup.
- Only the server service role can insert or update orders and order lines.

### Catalog Writes

- Product and product image writes are service-role only.

## Seed Products

Initial seed should include the six products from `docs/UITemplate.html`:

- The Dark Knight: Premium Format
- Berserker Armor Guts
- Cyber Ninja V
- Joker: Arkham Asylum
- Goku: Ultra Instinct
- Iron Man Mark LXXXV

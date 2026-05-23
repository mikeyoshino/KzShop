# Configuration

Use user secrets for local development and environment variables in hosted environments. Do not commit real secrets.

## Required Keys

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

## Ownership

### Client-Safe

These may be exposed to browser code when needed:

```text
Supabase:Url
Supabase:AnonKey
Stripe:PublishableKey
```

### Server-Only

These must remain in `KzShop.Web` only:

```text
Supabase:ServiceRoleKey
Stripe:SecretKey
Stripe:WebhookSecret
```

## Local Development Example

Use `dotnet user-secrets` from the server project directory:

```bash
dotnet user-secrets set "Supabase:Url" "https://example.supabase.co"
dotnet user-secrets set "Supabase:AnonKey" "replace-with-anon-key"
dotnet user-secrets set "Supabase:ServiceRoleKey" "replace-with-service-role-key"
dotnet user-secrets set "Stripe:SecretKey" "sk_test_replace"
dotnet user-secrets set "Stripe:WebhookSecret" "whsec_replace"
dotnet user-secrets set "Stripe:PublishableKey" "pk_test_replace"
dotnet user-secrets set "Checkout:SuccessUrl" "https://localhost:5001/checkout/success?session_id={CHECKOUT_SESSION_ID}"
dotnet user-secrets set "Checkout:CancelUrl" "https://localhost:5001/checkout/cancelled"
```

## Environment Variables

Use double underscores for nested config:

```text
Supabase__Url
Supabase__AnonKey
Supabase__ServiceRoleKey
Stripe__SecretKey
Stripe__WebhookSecret
Stripe__PublishableKey
Checkout__SuccessUrl
Checkout__CancelUrl
```

## Option Classes

Create typed options in the server project:

```csharp
public sealed class SupabaseOptions
{
    public required string Url { get; init; }
    public required string AnonKey { get; init; }
    public required string ServiceRoleKey { get; init; }
}

public sealed class StripeOptions
{
    public required string SecretKey { get; init; }
    public required string WebhookSecret { get; init; }
    public required string PublishableKey { get; init; }
}

public sealed class CheckoutOptions
{
    public required string SuccessUrl { get; init; }
    public required string CancelUrl { get; init; }
}
```

## Validation

At startup:

- Validate required option values are present.
- Validate `Checkout:SuccessUrl` contains `{CHECKOUT_SESSION_ID}`.
- Validate server-only keys are never passed to client DTOs or rendered into the page.

## Deployment Notes

- Use HTTPS URLs for checkout success and cancel routes.
- Configure Stripe webhook endpoint URL in the Stripe Dashboard.
- Configure Supabase Auth site URL and redirect URLs to match deployed domain.
- Configure CORS only for domains that need direct browser access.

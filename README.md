# ToyShops

ToyShops is a monorepo for an ecommerce platform with:

- Angular SSR storefront and admin apps
- ASP.NET Core Web API backend
- shared solution and workspace setup for local development

## Prerequisites

- .NET SDK
- Node.js
- npm

## Setup

1. Copy `.env.example` to `.env`.
2. Set the required API values:
   - `ConnectionStrings__DefaultConnection`
   - `Supabase__Url`
   - `Supabase__AnonKey`
   - `Supabase__JwtIssuer`
   - `Storage__Url`
   - `Storage__Key`
   - `Storage__Bucket`
   - `Stripe__SecretKey`
   - `Stripe__WebhookSecret`
3. Keep the documented frontend API target in `.env` aligned with the local API launch profile:
   - `NG_APP_API_BASE_URL=http://localhost:5297/api`
4. Restore and install dependencies:
   - `dotnet restore ToyShops.sln`
   - `dotnet build ToyShops.sln`
   - `npm install`

Current note: the storefront and admin apps currently resolve `/api/storefront` relative to the app origin in code. The `.env` entry is documented now for the later explicit frontend environment wiring.

## Run

1. Start the API:

```bash
dotnet run --project src/Ecommerce.Api/Ecommerce.Api.csproj
```

2. Start the storefront:

```bash
npm run dev:storefront
```

3. Start the admin app:

```bash
npm run dev:admin
```

## Verification

Use these commands to verify this slice:

```bash
dotnet build ToyShops.sln
dotnet test ToyShops.sln
npm run build:storefront
npm run build:admin
```

Note: `dotnet test ToyShops.sln` is intended to run the solution test projects. Keep the test projects in the solution so this command remains meaningful.

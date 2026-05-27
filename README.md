# ToyShops

ToyShops is a monorepo for an ecommerce platform with:

- Angular SSR storefront and admin apps
- ASP.NET Core Web API backend
- shared solution and workspace setup for local development

## Prerequisites

- .NET SDK
- Node.js
- npm
- Docker Desktop

## Setup

1. Copy `.env.example` to `.env`.
2. Set the required API values:
   - `ConnectionStrings__DefaultConnection`
   - `Jwt__Issuer`
   - `Jwt__Audience`
   - `Jwt__SigningKey`
   - `Jwt__AccessTokenMinutes`
   - `RefreshTokens__Days`
   - `Storage__LocalRoot`
   - `Storage__PublicBasePath`
   - `Stripe__SecretKey`
   - `Stripe__WebhookSecret`
3. Start local PostgreSQL:
   - `docker compose up -d`
4. Keep the documented frontend API target in `.env` aligned with the local API launch profile:
   - `NG_APP_API_BASE_URL=http://localhost:5297/api`
5. Restore and install dependencies:
   - `dotnet restore ToyShops.sln`
   - `dotnet build ToyShops.sln`
   - `npm install`

Current note: the storefront and admin apps currently resolve `/api/storefront` relative to the app origin in code. The `.env` entry is documented now for the later explicit frontend environment wiring.

Development admin seed:

- Email: `admin@example.com`
- Password: `Admin123!`
- Email: `mikeyoshinos@gmail.com`
- Password: `Mikey123`

Local Swagger admin auth:

1. Open `/swagger` while the API is running in Development.
2. Run `POST /api/auth/login` with the development admin credentials above.
3. Copy `accessToken` from the response.
4. Click `Authorize` in Swagger and paste the token.

## Run

1. Start local PostgreSQL:

```bash
docker compose up -d
```

2. Start the API:

```bash
dotnet run --project src/Ecommerce.Api/Ecommerce.Api.csproj
```

3. Start the storefront:

```bash
npm run dev:storefront
```

4. Start the admin app:

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

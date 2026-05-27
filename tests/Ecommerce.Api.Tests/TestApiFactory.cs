using System.Net.Http.Headers;
using System.Net.Http.Json;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using Ecommerce.Application.Common.Interfaces;
using Ecommerce.Infrastructure.Identity;
using Ecommerce.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Testcontainers.PostgreSql;
using Xunit;

namespace Ecommerce.Api.Tests;

public class TestApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("toyshops_api_tests")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    async Task IAsyncLifetime.InitializeAsync()
    {
        await _postgres.StartAsync();
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", _postgres.GetConnectionString());
        Environment.SetEnvironmentVariable("Jwt__Issuer", "ToyShops.Tests");
        Environment.SetEnvironmentVariable("Jwt__Audience", "ToyShops.Admin.Tests");
        Environment.SetEnvironmentVariable("Jwt__SigningKey", "test-signing-key-with-at-least-32-characters");
        Environment.SetEnvironmentVariable("Jwt__AccessTokenMinutes", "15");
        Environment.SetEnvironmentVariable("RefreshTokens__Days", "14");
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _postgres.DisposeAsync().AsTask();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _postgres.GetConnectionString(),
                ["Jwt:Issuer"] = "ToyShops.Tests",
                ["Jwt:Audience"] = "ToyShops.Admin.Tests",
                ["Jwt:SigningKey"] = "test-signing-key-with-at-least-32-characters",
                ["Jwt:AccessTokenMinutes"] = "15",
                ["RefreshTokens:Days"] = "14"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<ApplicationDbContext>));
            services.RemoveAll(typeof(IDbContextOptionsConfiguration<ApplicationDbContext>));
            services.RemoveAll(typeof(ApplicationDbContext));

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(_postgres.GetConnectionString()));
            services.RemoveAll(typeof(IStorageService));
            services.AddSingleton<IStorageService, FakeStorageService>();
            services.AddSingleton<TestCatalogSeeder>();
            services.AddHostedService(provider => provider.GetRequiredService<TestCatalogSeeder>());
        });
    }

    public Task<HttpClient> CreateAdminClientAsync()
    {
        return CreateAuthenticatedClientAsync("admin@example.com", "Admin123!");
    }

    public async Task<HttpClient> CreateAuthenticatedClientAsync(string email, string password)
    {
        var client = CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email,
            password
        });

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<AuthResponseContract>();
        if (string.IsNullOrWhiteSpace(payload?.AccessToken))
        {
            throw new InvalidOperationException("Test login did not return an access token.");
        }

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", payload.AccessToken);
        return client;
    }

    private sealed record AuthResponseContract(string AccessToken);
}

internal sealed class TestCatalogSeeder : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public TestCatalogSeeder(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.EnsureCreated();
        TestApiFactorySeed.Seed(dbContext);

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        await TestApiFactorySeed.SeedIdentityAsync(userManager, roleManager);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

internal static class TestApiFactorySeed
{
    public static async Task SeedIdentityAsync(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager)
    {
        foreach (var role in new[] { "admin", "customer" })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new ApplicationRole { Name = role });
            }
        }

        var admin = await EnsureUserAsync(userManager, "admin@example.com", "Admin123!");
        if (!await userManager.IsInRoleAsync(admin, "admin"))
        {
            await userManager.AddToRoleAsync(admin, "admin");
        }

        var customer = await EnsureUserAsync(userManager, "customer@example.com", "Customer123!");
        if (!await userManager.IsInRoleAsync(customer, "customer"))
        {
            await userManager.AddToRoleAsync(customer, "customer");
        }
    }

    private static async Task<ApplicationUser> EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string password)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is not null)
        {
            return user;
        }

        user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(x => x.Description)));
        }

        return user;
    }

    public static void Seed(ApplicationDbContext dbContext)
    {
        var existingProductSlugs = dbContext.Products
            .Select(x => x.Slug)
            .ToHashSet();

        var category = dbContext.Categories.FirstOrDefault(x => x.Slug == "dc-batman")
            ?? dbContext.Categories.FirstOrDefault(x => x.Slug == "batman")
            ?? new Category("DC / Batman", "dc-batman", "DC collectibles", true);
        var studio = dbContext.Studios.FirstOrDefault(x => x.Slug == "prime-studios")
            ?? new Studio("Prime Studios", "prime-studios", true);

        if (!dbContext.Categories.Any(x => x.Id == category.Id))
        {
            dbContext.Categories.Add(category);
        }

        if (!dbContext.Studios.Any(x => x.Id == studio.Id))
        {
            dbContext.Studios.Add(studio);
        }

        if (!existingProductSlugs.Contains("dark-knight"))
        {
            var product = Product.Create(
                "The Dark Knight: Premium Format",
                "dark-knight",
                "1/4 scale cinematic masterpiece.",
                "Tailored fabric suit and interchangeable expressions.",
                category.Id,
                studio.Id,
                "BAT-DK-001",
                650m,
                "USD",
                ProductStatus.PreOrder,
                true,
                true,
                "1/4 Scale",
                "Polystone, Fabric, PVC",
                "H: 24 x W: 12 x D: 10",
                "1000",
                "Est. Q4 2026");

            product.AddImage("/products/dark-knight/main.jpg", "Dark Knight hero", 0);
            product.AddSpecification("Franchise", "DC Comics", 0);
            dbContext.Products.Add(product);
            dbContext.InventoryItems.Add(new InventoryItem(product.Id, 8));
        }

        if (!existingProductSlugs.Contains("batman-beyond"))
        {
            var criticalProduct = Product.Create(
                "Batman Beyond: Modern Suit",
                "batman-beyond",
                "Futuristic 1/6 scale display piece.",
                "Sleek neon-accent suit with animated series styling.",
                category.Id,
                studio.Id,
                "BAT-BEY-002",
                420m,
                "USD",
                ProductStatus.Active,
                true,
                false,
                "1/6 Scale",
                "Polystone, PVC",
                "H: 14 x W: 8 x D: 7",
                "750",
                "In stock");

            criticalProduct.AddImage("/products/batman-beyond/main.jpg", "Batman Beyond hero", 0);
            criticalProduct.AddSpecification("Franchise", "DC Comics", 0);
            dbContext.Products.Add(criticalProduct);
            dbContext.InventoryItems.Add(new InventoryItem(criticalProduct.Id, 2));
        }

        if (!existingProductSlugs.Contains("nightwing-bludhaven"))
        {
            var healthyProduct = Product.Create(
                "Nightwing: Bludhaven Vigil",
                "nightwing-bludhaven",
                "Acrobatic premium statue.",
                "Dynamic rooftop pose with escrima sticks.",
                category.Id,
                studio.Id,
                "NIGHT-003",
                380m,
                "USD",
                ProductStatus.Active,
                false,
                true,
                "1/8 Scale",
                "Polystone, PVC",
                "H: 12 x W: 7 x D: 6",
                "900",
                "In stock");

            healthyProduct.AddImage("/products/nightwing/main.jpg", "Nightwing hero", 0);
            healthyProduct.AddSpecification("Franchise", "DC Comics", 0);
            dbContext.Products.Add(healthyProduct);
            dbContext.InventoryItems.Add(new InventoryItem(healthyProduct.Id, 12));
        }

        dbContext.SaveChanges();

        if (!dbContext.Orders.Any())
        {
            var productsBySlug = dbContext.Products
                .ToDictionary(x => x.Slug);

            var orderInputs = new[]
            {
                new { Slug = "dark-knight", Quantity = 1, Status = OrderStatus.Paid },
                new { Slug = "batman-beyond", Quantity = 1, Status = OrderStatus.Cancelled },
                new { Slug = "dark-knight", Quantity = 1, Status = OrderStatus.Pending },
                new { Slug = "nightwing-bludhaven", Quantity = 1, Status = OrderStatus.Processing },
                new { Slug = "batman-beyond", Quantity = 1, Status = OrderStatus.Shipped }
            };

            foreach (var orderInput in orderInputs)
            {
                var product = productsBySlug[orderInput.Slug];
                var order = Order.Create(
                    $"ORD-TST-{Guid.NewGuid():N}"[..16],
                    "Api Test Customer",
                    "api-test@example.com",
                    DateTimeOffset.UtcNow,
                    [(product.Id, product.Name, product.Sku, orderInput.Quantity, product.PriceAmount, product.Currency)],
                    orderInput.Status);
                dbContext.Orders.Add(order);
            }
        }

        dbContext.SaveChanges();
    }
}

internal sealed class FakeStorageService : IStorageService
{
    public Task<string> UploadAsync(string objectPath, Stream content, string contentType, CancellationToken cancellationToken = default)
    {
        return Task.FromResult($"/storage/{objectPath}");
    }

    public Task DeleteAsync(string objectPath, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}

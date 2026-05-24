using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using Ecommerce.Application.Common.Interfaces;
using Ecommerce.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Ecommerce.Api.Tests;

public class TestApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"toyshops-api-tests-{Guid.NewGuid()}";

    public TestApiFactory()
    {
        Environment.SetEnvironmentVariable(
            "ConnectionStrings__DefaultConnection",
            "Host=localhost;Port=5432;Database=toyshops-tests;Username=postgres;Password=postgres");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Port=5432;Database=toyshops-tests;Username=postgres;Password=postgres"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<ApplicationDbContext>));
            services.RemoveAll(typeof(IDbContextOptionsConfiguration<ApplicationDbContext>));
            services.RemoveAll(typeof(ApplicationDbContext));

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));
            services.RemoveAll(typeof(IStorageService));
            services.AddSingleton<IStorageService, FakeStorageService>();
            services.AddSingleton<TestCatalogSeeder>();
            services.AddHostedService(provider => provider.GetRequiredService<TestCatalogSeeder>());
        });
    }

}

internal sealed class TestCatalogSeeder : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public TestCatalogSeeder(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.EnsureCreated();
        TestApiFactorySeed.Seed(dbContext);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

internal static class TestApiFactorySeed
{
    public static void Seed(ApplicationDbContext dbContext)
    {
        if (dbContext.Products.Any())
        {
            return;
        }

        var category = new Category("DC / Batman", "batman", "DC collectibles", true);
        var studio = new Studio("Prime Studios", "prime-studios", true);

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

        dbContext.Categories.Add(category);
        dbContext.Studios.Add(studio);
        dbContext.Products.Add(product);
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

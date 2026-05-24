using Ecommerce.Application.Common.Interfaces;
using Ecommerce.Application.Products.GetProducts;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ecommerce.Application.Tests.Products;

public class GetProductsHandlerTests
{
    [Fact]
    public async Task Handle_ShouldFilterByCategorySlug()
    {
        await using var context = TestApplicationDbContextFactory.CreateWithCatalog();
        var handler = new GetProductsHandler(context);

        var response = await handler.Handle(new GetProductsQuery("batman", null, null), CancellationToken.None);

        response.Items.Should().HaveCount(2);
        response.Items.Should().OnlyContain(x => x.CategorySlug == "batman");
    }

    [Fact]
    public async Task Handle_ShouldApplyDefaultPublicStatusFilter()
    {
        await using var context = TestApplicationDbContextFactory.CreateWithCatalog();
        var handler = new GetProductsHandler(context);

        var response = await handler.Handle(new GetProductsQuery(null, null, null), CancellationToken.None);

        response.Items.Should().HaveCount(3);
        response.Items.Should().NotContain(x => x.Status == ProductStatus.Archived.ToString());
    }

    [Fact]
    public async Task Handle_ShouldFilterByStatus_CaseInsensitive()
    {
        await using var context = TestApplicationDbContextFactory.CreateWithCatalog();
        var handler = new GetProductsHandler(context);

        var response = await handler.Handle(new GetProductsQuery(null, "pReOrDeR", null), CancellationToken.None);

        response.Items.Should().ContainSingle();
        response.Items[0].Slug.Should().Be("dark-knight-1-4");
    }

    [Fact]
    public async Task Handle_ShouldFilterByScale_CaseInsensitive()
    {
        await using var context = TestApplicationDbContextFactory.CreateWithCatalog();
        var handler = new GetProductsHandler(context);

        var response = await handler.Handle(new GetProductsQuery(null, null, "1/6 scale"), CancellationToken.None);

        response.Items.Should().ContainSingle();
        response.Items[0].Slug.Should().Be("arkham-joker");
    }

    [Fact]
    public async Task Handle_ShouldSelectPrimaryImageByLowestSortOrder()
    {
        await using var context = TestApplicationDbContextFactory.CreateWithCatalog();
        var handler = new GetProductsHandler(context);

        var response = await handler.Handle(new GetProductsQuery(null, "preorder", null), CancellationToken.None);

        response.Items.Should().ContainSingle();
        response.Items[0].PrimaryImageUrl.Should().Be("dark-knight-main.jpg");
    }
}

internal static class TestApplicationDbContextFactory
{
    public static TestApplicationDbContext CreateWithCatalog()
    {
        var options = new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        var context = new TestApplicationDbContext(options);

        var batmanCategory = new Category("DC / Batman", "batman", "DC collectibles", true);
        var animeCategory = new Category("Anime", "anime", "Anime collectibles", true);

        var primeStudios = new Studio("Prime Studios", "prime-studios", true);
        var hotToys = new Studio("Hot Toys", "hot-toys", true);
        var darkArtStudios = new Studio("Dark Art Studios", "dark-art-studios", true);

        var darkKnight = Product.Create(
            "Dark Knight 1/4",
            "dark-knight-1-4",
            "Short",
            "Long",
            batmanCategory.Id,
            primeStudios.Id,
            "BAT-DK-001",
            650m,
            "USD",
            ProductStatus.PreOrder,
            true,
            true,
            "1/4 Scale",
            "Polystone",
            "H: 24 x W: 12 x D: 10",
            "1000",
            "Q4 2026");

        var arkhamJoker = Product.Create(
            "Arkham Joker",
            "arkham-joker",
            "Short",
            "Long",
            batmanCategory.Id,
            hotToys.Id,
            "BAT-JOK-002",
            320m,
            "USD",
            ProductStatus.Active,
            false,
            false,
            "1/6 Scale",
            "PVC",
            "H: 12.5",
            "Standard",
            "Available");

        var berserkerGuts = Product.Create(
            "Berserker Guts",
            "berserker-guts",
            "Short",
            "Long",
            animeCategory.Id,
            darkArtStudios.Id,
            "ANI-GUT-003",
            1200m,
            "USD",
            ProductStatus.Waitlist,
            false,
            false,
            "1/3 Scale",
            "Polystone",
            "H: 38 x W: 25 x D: 22",
            "500",
            "Waitlist");

        var archivedFigure = Product.Create(
            "Legacy Batman",
            "legacy-batman",
            "Short",
            "Long",
            batmanCategory.Id,
            hotToys.Id,
            "BAT-OLD-004",
            200m,
            "USD",
            ProductStatus.Archived,
            false,
            false,
            "1/6 Scale",
            "PVC",
            "H: 12",
            "Standard",
            "Archived");

        darkKnight.AddImage("dark-knight-detail.jpg", "detail", 1);
        darkKnight.AddImage("dark-knight-main.jpg", "main", 0);
        arkhamJoker.AddImage("arkham-joker-main.jpg", "main", 0);
        berserkerGuts.AddImage("berserker-main.jpg", "main", 0);

        context.Categories.AddRange(batmanCategory, animeCategory);
        context.Studios.AddRange(primeStudios, hotToys, darkArtStudios);
        context.Products.AddRange(darkKnight, arkhamJoker, berserkerGuts, archivedFigure);
        context.SaveChanges();

        return context;
    }
}

internal sealed class TestApplicationDbContext : DbContext, IApplicationDbContext
{
    public TestApplicationDbContext(DbContextOptions<TestApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Studio> Studios => Set<Studio>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<ProductSpecification> ProductSpecifications => Set<ProductSpecification>();
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
}

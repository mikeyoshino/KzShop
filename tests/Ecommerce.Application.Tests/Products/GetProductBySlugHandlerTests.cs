using Ecommerce.Application.Common.Interfaces;
using Ecommerce.Application.Products.GetProductBySlug;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ecommerce.Application.Tests.Products;

public class GetProductBySlugHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnProductDetail_WithOrderedImagesAndSpecifications()
    {
        await using var context = ProductBySlugTestApplicationDbContextFactory.CreateWithCatalog();
        var handler = new GetProductBySlugHandler(context);

        var response = await handler.Handle(new GetProductBySlugQuery("dark-knight-1-4"), CancellationToken.None);

        response.Should().NotBeNull();
        response!.Slug.Should().Be("dark-knight-1-4");
        response.StudioName.Should().Be("Prime Studios");
        response.CategorySlug.Should().Be("batman");

        response.Images.Should().HaveCount(3);
        response.Images.Select(x => x.PublicUrl).Should().ContainInOrder(
            "dark-knight-main.jpg",
            "dark-knight-detail.jpg",
            "dark-knight-base.jpg");
        response.Images.Select(x => x.SortOrder).Should().ContainInOrder(0, 1, 2);

        response.Specifications.Should().HaveCount(3);
        response.Specifications.Select(x => x.Label).Should().ContainInOrder(
            "Franchise",
            "Scale",
            "Materials");
        response.Specifications.Select(x => x.SortOrder).Should().ContainInOrder(0, 1, 2);
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenSlugIsUnknown()
    {
        await using var context = ProductBySlugTestApplicationDbContextFactory.CreateWithCatalog();
        var handler = new GetProductBySlugHandler(context);

        var response = await handler.Handle(new GetProductBySlugQuery("missing-slug"), CancellationToken.None);

        response.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenProductIsNotPublic()
    {
        await using var context = ProductBySlugTestApplicationDbContextFactory.CreateWithCatalog();
        var handler = new GetProductBySlugHandler(context);

        var response = await handler.Handle(new GetProductBySlugQuery("legacy-batman"), CancellationToken.None);

        response.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldNormalizeSlugInput()
    {
        await using var context = ProductBySlugTestApplicationDbContextFactory.CreateWithCatalog();
        var handler = new GetProductBySlugHandler(context);

        var response = await handler.Handle(new GetProductBySlugQuery("  DARK-KNIGHT-1-4  "), CancellationToken.None);

        response.Should().NotBeNull();
        response!.Slug.Should().Be("dark-knight-1-4");
    }
}

internal static class ProductBySlugTestApplicationDbContextFactory
{
    public static ProductBySlugTestApplicationDbContext CreateWithCatalog()
    {
        var options = new DbContextOptionsBuilder<ProductBySlugTestApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        var context = new ProductBySlugTestApplicationDbContext(options);

        var batmanCategory = new Category("DC / Batman", "batman", "DC collectibles", true);
        var primeStudios = new Studio("Prime Studios", "prime-studios", true);

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

        darkKnight.AddImage("dark-knight-base.jpg", "Base details", 2);
        darkKnight.AddImage("dark-knight-main.jpg", "Main shot", 0);
        darkKnight.AddImage("dark-knight-detail.jpg", "Detail shot", 1);

        darkKnight.AddSpecification("Materials", "Polystone, Fabric, PVC", 2);
        darkKnight.AddSpecification("Franchise", "DC Comics", 0);
        darkKnight.AddSpecification("Scale", "1/4 Scale", 1);

        var archivedFigure = Product.Create(
            "Legacy Batman",
            "legacy-batman",
            "Short",
            "Long",
            batmanCategory.Id,
            primeStudios.Id,
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

        context.Categories.Add(batmanCategory);
        context.Studios.Add(primeStudios);
        context.Products.AddRange(darkKnight, archivedFigure);
        context.SaveChanges();

        return context;
    }
}

internal sealed class ProductBySlugTestApplicationDbContext : DbContext, IApplicationDbContext
{
    public ProductBySlugTestApplicationDbContext(DbContextOptions<ProductBySlugTestApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Studio> Studios => Set<Studio>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<ProductSpecification> ProductSpecifications => Set<ProductSpecification>();
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<OrderItem>().Property<Guid>("Id");
        modelBuilder.Entity<OrderItem>().HasKey("Id");
    }
}

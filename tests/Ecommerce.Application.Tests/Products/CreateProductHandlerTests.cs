using Ecommerce.Application.Common.Interfaces;
using Ecommerce.Application.Common.Models;
using Ecommerce.Application.Products.ArchiveProduct;
using Ecommerce.Application.Products.CreateProduct;
using Ecommerce.Application.Products.GetAdminProductById;
using Ecommerce.Application.Products.UpdateProduct;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ecommerce.Application.Tests.Products;

public class CreateProductHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateProduct_WithSpecifications()
    {
        await using var context = ProductWriteTestApplicationDbContextFactory.CreateWithCatalog();
        var handler = new CreateProductHandler(context);
        var command = new CreateProductCommand(
            "Batman Beyond 1/4",
            "batman-beyond-1-4",
            "Short text",
            "Long text",
            ProductWriteTestData.BatmanCategoryId,
            ProductWriteTestData.PrimeStudiosId,
            "BAT-BEY-003",
            899m,
            "usd",
            ProductStatus.PreOrder,
            true,
            false,
            "1/4 Scale",
            "Polystone",
            "H: 24 x W: 16 x D: 15",
            "999",
            "Q2 2027",
            [
                new CreateProductSpecificationInput("Franchise", "DC Comics", 0),
                new CreateProductSpecificationInput("Character", "Batman Beyond", 1)
            ]);

        var response = await handler.Handle(command, CancellationToken.None);
        response.IsSuccess.Should().BeTrue();
        response.Value.Should().NotBeNull();

        response.Value!.Name.Should().Be("Batman Beyond 1/4");
        response.Value.Slug.Should().Be("batman-beyond-1-4");
        response.Value.Sku.Should().Be("BAT-BEY-003");
        response.Value.Status.Should().Be(ProductStatus.PreOrder.ToString());

        var createdProduct = await context.Products.SingleAsync(x => x.Id == response.Value.Id);
        createdProduct.Currency.Should().Be("USD");

        var specs = await context.ProductSpecifications
            .AsNoTracking()
            .Where(x => x.ProductId == response.Value.Id)
            .OrderBy(x => x.SortOrder)
            .ToListAsync();

        specs.Should().HaveCount(2);
        specs.Select(x => x.Label).Should().ContainInOrder("Franchise", "Character");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenSlugAlreadyExists()
    {
        await using var context = ProductWriteTestApplicationDbContextFactory.CreateWithCatalog();
        var handler = new CreateProductHandler(context);
        var command = new CreateProductCommand(
            "Different Name",
            "dark-knight-1-4",
            "Short text",
            "Long text",
            ProductWriteTestData.BatmanCategoryId,
            ProductWriteTestData.PrimeStudiosId,
            "BAT-NEW-123",
            600m,
            "USD",
            ProductStatus.Active,
            false,
            false,
            "1/4 Scale",
            "Polystone",
            "H: 24",
            "500",
            "Q1 2027",
            []);

        var response = await handler.Handle(command, CancellationToken.None);
        response.IsFailure.Should().BeTrue();
        response.ErrorCode.Should().Be(BusinessErrorCode.DuplicateSlug);
    }
}

public class UpdateProductHandlerTests
{
    [Fact]
    public async Task Handle_ShouldUpdateProduct_AndReplaceSpecificationRows()
    {
        await using var context = ProductWriteTestApplicationDbContextFactory.CreateWithCatalog();
        var handler = new UpdateProductHandler(context);
        var command = new UpdateProductCommand(
            ProductWriteTestData.DarkKnightProductId,
            "Dark Knight Returns 1/4",
            "dark-knight-returns-1-4",
            "Updated short",
            "Updated long",
            ProductWriteTestData.BatmanCategoryId,
            ProductWriteTestData.PrimeStudiosId,
            "BAT-DKR-004",
            999m,
            "usd",
            ProductStatus.Waitlist,
            false,
            true,
            "1/4 Scale",
            "Mixed Media",
            "H: 26 x W: 18 x D: 16",
            "750",
            "Waitlist open",
            [
                new UpdateProductSpecificationInput("Franchise", "DC Comics", 0),
                new UpdateProductSpecificationInput("Edition", "750", 1),
                new UpdateProductSpecificationInput("Weight", "18kg", 2)
            ]);

        var response = await handler.Handle(command, CancellationToken.None);
        response.IsSuccess.Should().BeTrue();
        response.Value.Should().NotBeNull();

        response.Value!.Slug.Should().Be("dark-knight-returns-1-4");
        response.Value.Status.Should().Be(ProductStatus.Waitlist.ToString());

        var updatedProduct = await context.Products.SingleAsync(x => x.Id == ProductWriteTestData.DarkKnightProductId);
        updatedProduct.Name.Should().Be("Dark Knight Returns 1/4");
        updatedProduct.Currency.Should().Be("USD");
        updatedProduct.Sku.Should().Be("BAT-DKR-004");

        var specs = await context.ProductSpecifications
            .AsNoTracking()
            .Where(x => x.ProductId == ProductWriteTestData.DarkKnightProductId)
            .OrderBy(x => x.SortOrder)
            .ToListAsync();

        specs.Should().HaveCount(3);
        specs.Select(x => x.Label).Should().ContainInOrder("Franchise", "Edition", "Weight");
        specs.Should().NotContain(x => x.Label == "Character");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenProductDoesNotExist()
    {
        await using var context = ProductWriteTestApplicationDbContextFactory.CreateWithCatalog();
        var handler = new UpdateProductHandler(context);
        var command = new UpdateProductCommand(
            Guid.NewGuid(),
            "Name",
            "name",
            "Short",
            "Long",
            ProductWriteTestData.BatmanCategoryId,
            ProductWriteTestData.PrimeStudiosId,
            "SKU-1",
            100m,
            "USD",
            ProductStatus.Draft,
            false,
            false,
            "1/6",
            "PVC",
            "H: 12",
            "Standard",
            "TBD",
            []);

        var response = await handler.Handle(command, CancellationToken.None);
        response.IsFailure.Should().BeTrue();
        response.ErrorCode.Should().Be(BusinessErrorCode.ProductNotFound);
    }
}

public class ArchiveProductHandlerTests
{
    [Fact]
    public async Task Handle_ShouldArchiveProduct()
    {
        await using var context = ProductWriteTestApplicationDbContextFactory.CreateWithCatalog();
        var handler = new ArchiveProductHandler(context);
        var command = new ArchiveProductCommand(ProductWriteTestData.DarkKnightProductId);

        var response = await handler.Handle(command, CancellationToken.None);
        response.IsSuccess.Should().BeTrue();
        response.Value.Should().NotBeNull();

        response.Value!.Status.Should().Be(ProductStatus.Archived.ToString());

        var archivedProduct = await context.Products.SingleAsync(x => x.Id == ProductWriteTestData.DarkKnightProductId);
        archivedProduct.Status.Should().Be(ProductStatus.Archived);
        archivedProduct.ArchivedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenProductDoesNotExist()
    {
        await using var context = ProductWriteTestApplicationDbContextFactory.CreateWithCatalog();
        var handler = new ArchiveProductHandler(context);
        var command = new ArchiveProductCommand(Guid.NewGuid());

        var response = await handler.Handle(command, CancellationToken.None);
        response.IsFailure.Should().BeTrue();
        response.ErrorCode.Should().Be(BusinessErrorCode.ProductNotFound);
    }
}

public class GetAdminProductByIdHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnAdminProductDetail_WithSpecsAndInventory()
    {
        await using var context = ProductWriteTestApplicationDbContextFactory.CreateWithCatalog();
        var handler = new GetAdminProductByIdHandler(context);

        var response = await handler.Handle(new GetAdminProductByIdQuery(ProductWriteTestData.DarkKnightProductId), CancellationToken.None);

        response.Should().NotBeNull();
        response!.Slug.Should().Be("dark-knight-1-4");
        response.Status.Should().Be(ProductStatus.PreOrder.ToString());
        response.InventoryOnHand.Should().Be(15);
        response.InventoryReserved.Should().Be(3);
        response.InventoryAvailable.Should().Be(12);

        response.Specifications.Should().HaveCount(2);
        response.Specifications.Select(x => x.Label).Should().ContainInOrder("Franchise", "Character");

        response.Images.Should().HaveCount(2);
        response.Images.Select(x => x.PublicUrl).Should().ContainInOrder("dark-knight-main.jpg", "dark-knight-detail.jpg");
    }
}

internal static class ProductWriteTestData
{
    public static readonly Guid BatmanCategoryId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid PrimeStudiosId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid DarkKnightProductId = Guid.Parse("33333333-3333-3333-3333-333333333333");
}

internal static class ProductWriteTestApplicationDbContextFactory
{
    public static ProductWriteTestApplicationDbContext CreateWithCatalog()
    {
        var options = new DbContextOptionsBuilder<ProductWriteTestApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        var context = new ProductWriteTestApplicationDbContext(options);

        var batmanCategory = new Category("Batman", "batman", "DC collectibles", true);
        SetEntityId(batmanCategory, ProductWriteTestData.BatmanCategoryId);

        var primeStudios = new Studio("Prime Studios", "prime-studios", true);
        SetEntityId(primeStudios, ProductWriteTestData.PrimeStudiosId);

        var darkKnight = Product.Create(
            "Dark Knight 1/4",
            "dark-knight-1-4",
            "Short",
            "Long",
            ProductWriteTestData.BatmanCategoryId,
            ProductWriteTestData.PrimeStudiosId,
            "BAT-DK-001",
            650m,
            "USD",
            ProductStatus.PreOrder,
            true,
            false,
            "1/4 Scale",
            "Polystone",
            "H: 24 x W: 12 x D: 10",
            "1000",
            "Q4 2026");
        SetEntityId(darkKnight, ProductWriteTestData.DarkKnightProductId);

        darkKnight.AddSpecification("Franchise", "DC Comics", 0);
        darkKnight.AddSpecification("Character", "Batman", 1);
        darkKnight.AddImage("dark-knight-main.jpg", "main", 0);
        darkKnight.AddImage("dark-knight-detail.jpg", "detail", 1);

        var inventory = new InventoryItem(ProductWriteTestData.DarkKnightProductId, 15);
        inventory.Reserve(3);

        context.Categories.Add(batmanCategory);
        context.Studios.Add(primeStudios);
        context.Products.Add(darkKnight);
        context.InventoryItems.Add(inventory);
        context.SaveChanges();
        context.ChangeTracker.Clear();

        return context;
    }

    private static void SetEntityId(object entity, Guid id)
    {
        var idProperty = entity.GetType().GetProperty("Id");
        idProperty!.SetValue(entity, id);
    }
}

internal sealed class ProductWriteTestApplicationDbContext : DbContext, IApplicationDbContext
{
    public ProductWriteTestApplicationDbContext(DbContextOptions<ProductWriteTestApplicationDbContext> options)
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

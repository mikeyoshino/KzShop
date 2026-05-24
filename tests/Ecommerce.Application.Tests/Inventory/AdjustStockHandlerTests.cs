using Ecommerce.Application.Common.Interfaces;
using Ecommerce.Application.Common.Models;
using Ecommerce.Application.Inventory.AdjustStock;
using Ecommerce.Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ecommerce.Application.Tests.Inventory;

public class AdjustStockHandlerTests
{
    [Fact]
    public async Task Handle_ShouldAdjustOnHand_WhenResultingQuantityIsValid()
    {
        await using var context = InventoryTestApplicationDbContextFactory.CreateWithInventory();
        var handler = new AdjustStockHandler(context);
        var command = new AdjustStockCommand(
            InventoryTestData.DarkKnightProductId,
            -2);

        var response = await handler.Handle(command, CancellationToken.None);

        response.IsSuccess.Should().BeTrue();
        response.Value.Should().NotBeNull();
        response.Value!.OnHandQuantity.Should().Be(13);
        response.Value.ReservedQuantity.Should().Be(3);
        response.Value.AvailableQuantity.Should().Be(10);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenProductDoesNotExist()
    {
        await using var context = InventoryTestApplicationDbContextFactory.CreateWithInventory();
        var handler = new AdjustStockHandler(context);

        var response = await handler.Handle(new AdjustStockCommand(Guid.NewGuid(), 1), CancellationToken.None);

        response.IsFailure.Should().BeTrue();
        response.ErrorCode.Should().Be(BusinessErrorCode.ProductNotFound);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenInventoryDoesNotExist()
    {
        await using var context = InventoryTestApplicationDbContextFactory.CreateWithProductOnly();
        var handler = new AdjustStockHandler(context);

        var response = await handler.Handle(new AdjustStockCommand(InventoryTestData.DarkKnightProductId, 1), CancellationToken.None);

        response.IsFailure.Should().BeTrue();
        response.ErrorCode.Should().Be(BusinessErrorCode.InventoryNotFound);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenAdjustmentWouldDropBelowReserved()
    {
        await using var context = InventoryTestApplicationDbContextFactory.CreateWithInventory();
        var handler = new AdjustStockHandler(context);
        var command = new AdjustStockCommand(
            InventoryTestData.DarkKnightProductId,
            -13);

        var response = await handler.Handle(command, CancellationToken.None);

        response.IsFailure.Should().BeTrue();
        response.ErrorCode.Should().Be(BusinessErrorCode.ValidationFailed);
    }
}

internal static class InventoryTestData
{
    public static readonly Guid BatmanCategoryId = Guid.Parse("51111111-1111-1111-1111-111111111111");
    public static readonly Guid PrimeStudiosId = Guid.Parse("52222222-2222-2222-2222-222222222222");
    public static readonly Guid DarkKnightProductId = Guid.Parse("53333333-3333-3333-3333-333333333333");
}

internal static class InventoryTestApplicationDbContextFactory
{
    public static InventoryTestApplicationDbContext CreateWithInventory()
    {
        var context = CreateBaseContext();
        SeedProductCatalog(context);

        var inventory = new InventoryItem(InventoryTestData.DarkKnightProductId, 15);
        inventory.Reserve(3);
        context.InventoryItems.Add(inventory);

        context.SaveChanges();
        context.ChangeTracker.Clear();

        return context;
    }

    public static InventoryTestApplicationDbContext CreateWithProductOnly()
    {
        var context = CreateBaseContext();
        SeedProductCatalog(context);
        context.SaveChanges();
        context.ChangeTracker.Clear();
        return context;
    }

    private static InventoryTestApplicationDbContext CreateBaseContext()
    {
        var options = new DbContextOptionsBuilder<InventoryTestApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new InventoryTestApplicationDbContext(options);
    }

    private static void SeedProductCatalog(InventoryTestApplicationDbContext context)
    {
        var category = new Category("Batman", "batman", "DC collectibles", true);
        SetEntityId(category, InventoryTestData.BatmanCategoryId);

        var studio = new Studio("Prime Studios", "prime-studios", true);
        SetEntityId(studio, InventoryTestData.PrimeStudiosId);

        var product = Product.Create(
            "Dark Knight 1/4",
            "dark-knight-1-4",
            "Short",
            "Long",
            InventoryTestData.BatmanCategoryId,
            InventoryTestData.PrimeStudiosId,
            "BAT-DK-001",
            650m,
            "USD",
            Domain.Enums.ProductStatus.PreOrder,
            true,
            false,
            "1/4 Scale",
            "Polystone",
            "H: 24 x W: 12 x D: 10",
            "1000",
            "Q4 2026");
        SetEntityId(product, InventoryTestData.DarkKnightProductId);

        context.Categories.Add(category);
        context.Studios.Add(studio);
        context.Products.Add(product);
    }

    private static void SetEntityId(object entity, Guid id)
    {
        var idProperty = entity.GetType().GetProperty("Id");
        idProperty!.SetValue(entity, id);
    }
}

internal sealed class InventoryTestApplicationDbContext : DbContext, IApplicationDbContext
{
    public InventoryTestApplicationDbContext(DbContextOptions<InventoryTestApplicationDbContext> options)
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

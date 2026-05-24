using Ecommerce.Application.Common.Interfaces;
using Ecommerce.Application.Common.Models;
using Ecommerce.Application.ProductImages.DeleteProductImage;
using Ecommerce.Application.ProductImages.ReorderProductImages;
using Ecommerce.Application.ProductImages.UploadProductImage;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ecommerce.Application.Tests.ProductImages;

public class ReorderProductImagesHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReorderImages_WithDeterministicContiguousSortOrder()
    {
        await using var context = ProductImagesTestApplicationDbContextFactory.CreateWithProductAndImages();
        var handler = new ReorderProductImagesHandler(context);
        var command = new ReorderProductImagesCommand(
            ProductImagesTestData.DarkKnightProductId,
            [
                ProductImagesTestData.DetailImageId,
                ProductImagesTestData.PoseImageId,
                ProductImagesTestData.MainImageId
            ]);

        var response = await handler.Handle(command, CancellationToken.None);

        response.IsSuccess.Should().BeTrue();
        response.Value.Should().NotBeNull();

        var orderedImages = await context.ProductImages
            .AsNoTracking()
            .Where(x => x.ProductId == ProductImagesTestData.DarkKnightProductId)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Id)
            .ToListAsync();

        orderedImages.Should().HaveCount(3);
        orderedImages.Select(x => x.Id).Should().ContainInOrder(
            ProductImagesTestData.DetailImageId,
            ProductImagesTestData.PoseImageId,
            ProductImagesTestData.MainImageId);
        orderedImages.Select(x => x.SortOrder).Should().ContainInOrder(0, 1, 2);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenCommandDoesNotContainAllProductImages()
    {
        await using var context = ProductImagesTestApplicationDbContextFactory.CreateWithProductAndImages();
        var handler = new ReorderProductImagesHandler(context);
        var command = new ReorderProductImagesCommand(
            ProductImagesTestData.DarkKnightProductId,
            [
                ProductImagesTestData.MainImageId,
                ProductImagesTestData.DetailImageId
            ]);

        var response = await handler.Handle(command, CancellationToken.None);

        response.IsFailure.Should().BeTrue();
        response.ErrorCode.Should().Be(BusinessErrorCode.ValidationFailed);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenProductDoesNotExist()
    {
        await using var context = ProductImagesTestApplicationDbContextFactory.CreateWithProductAndImages();
        var handler = new ReorderProductImagesHandler(context);
        var command = new ReorderProductImagesCommand(Guid.NewGuid(), []);

        var response = await handler.Handle(command, CancellationToken.None);

        response.IsFailure.Should().BeTrue();
        response.ErrorCode.Should().Be(BusinessErrorCode.ProductNotFound);
    }
}

public class UploadProductImageHandlerTests
{
    [Fact]
    public async Task Handle_ShouldUploadImage_AndCreateRow()
    {
        await using var context = ProductImagesTestApplicationDbContextFactory.CreateWithProductOnly();
        var storage = new FakeStorageService { UploadResult = "products/63333333/new-image.jpg" };
        var handler = new UploadProductImageHandler(context, storage);
        await using var content = new MemoryStream([1, 2, 3, 4]);

        var response = await handler.Handle(
            new UploadProductImageCommand(
                ProductImagesTestData.DarkKnightProductId,
                "new-image.jpg",
                "image/jpeg",
                content,
                "front view"),
            CancellationToken.None);

        response.IsSuccess.Should().BeTrue();
        response.Value.Should().NotBeNull();
        response.Value!.SortOrder.Should().Be(0);

        storage.UploadedPaths.Should().ContainSingle()
            .Which.Should().Contain($"{ProductImagesTestData.DarkKnightProductId}");

        var image = await context.ProductImages.SingleAsync(x => x.Id == response.Value.Id);
        image.PublicUrl.Should().Be("products/63333333/new-image.jpg");
        image.AltText.Should().Be("front view");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenStorageUploadFails()
    {
        await using var context = ProductImagesTestApplicationDbContextFactory.CreateWithProductOnly();
        var storage = new FakeStorageService { UploadException = new HttpRequestException("upload failed") };
        var handler = new UploadProductImageHandler(context, storage);
        await using var content = new MemoryStream([1]);

        var response = await handler.Handle(
            new UploadProductImageCommand(
                ProductImagesTestData.DarkKnightProductId,
                "new-image.jpg",
                "image/jpeg",
                content,
                "front view"),
            CancellationToken.None);

        response.IsFailure.Should().BeTrue();
        response.ErrorCode.Should().Be(BusinessErrorCode.StorageUploadFailed);
    }
}

public class DeleteProductImageHandlerTests
{
    [Fact]
    public async Task Handle_ShouldDeleteImageAndStorageObject_AndNormalizeSortOrder()
    {
        await using var context = ProductImagesTestApplicationDbContextFactory.CreateWithProductAndImages();
        var storage = new FakeStorageService();
        var handler = new DeleteProductImageHandler(context, storage);

        var response = await handler.Handle(
            new DeleteProductImageCommand(
                ProductImagesTestData.DarkKnightProductId,
                ProductImagesTestData.DetailImageId),
            CancellationToken.None);

        response.IsSuccess.Should().BeTrue();
        storage.DeletedPaths.Should().ContainSingle()
            .Which.Should().Be("products/63333333/detail.jpg");

        var images = await context.ProductImages
            .AsNoTracking()
            .Where(x => x.ProductId == ProductImagesTestData.DarkKnightProductId)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Id)
            .ToListAsync();

        images.Should().HaveCount(2);
        images.Select(x => x.SortOrder).Should().ContainInOrder(0, 1);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenStorageDeleteFails()
    {
        await using var context = ProductImagesTestApplicationDbContextFactory.CreateWithProductAndImages();
        var storage = new FakeStorageService { DeleteException = new HttpRequestException("delete failed") };
        var handler = new DeleteProductImageHandler(context, storage);

        var response = await handler.Handle(
            new DeleteProductImageCommand(
                ProductImagesTestData.DarkKnightProductId,
                ProductImagesTestData.DetailImageId),
            CancellationToken.None);

        response.IsFailure.Should().BeTrue();
        response.ErrorCode.Should().Be(BusinessErrorCode.StorageDeleteFailed);
        (await context.ProductImages.AnyAsync(x => x.Id == ProductImagesTestData.DetailImageId))
            .Should().BeTrue();
    }
}

internal static class ProductImagesTestData
{
    public static readonly Guid BatmanCategoryId = Guid.Parse("61111111-1111-1111-1111-111111111111");
    public static readonly Guid PrimeStudiosId = Guid.Parse("62222222-2222-2222-2222-222222222222");
    public static readonly Guid DarkKnightProductId = Guid.Parse("63333333-3333-3333-3333-333333333333");
    public static readonly Guid MainImageId = Guid.Parse("64444444-4444-4444-4444-444444444444");
    public static readonly Guid DetailImageId = Guid.Parse("65555555-5555-5555-5555-555555555555");
    public static readonly Guid PoseImageId = Guid.Parse("66666666-6666-6666-6666-666666666666");
}

internal static class ProductImagesTestApplicationDbContextFactory
{
    public static ProductImagesTestApplicationDbContext CreateWithProductAndImages()
    {
        var context = CreateBaseContext();
        var product = SeedProduct(context);

        product.AddImage("products/63333333/main.jpg", "main", 5);
        product.AddImage("products/63333333/detail.jpg", "detail", 11);
        product.AddImage("products/63333333/pose.jpg", "pose", 20);
        SetImageIds(product);

        context.SaveChanges();
        context.ChangeTracker.Clear();

        return context;
    }

    public static ProductImagesTestApplicationDbContext CreateWithProductOnly()
    {
        var context = CreateBaseContext();
        _ = SeedProduct(context);
        context.SaveChanges();
        context.ChangeTracker.Clear();
        return context;
    }

    private static ProductImagesTestApplicationDbContext CreateBaseContext()
    {
        var options = new DbContextOptionsBuilder<ProductImagesTestApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new ProductImagesTestApplicationDbContext(options);
    }

    private static Product SeedProduct(ProductImagesTestApplicationDbContext context)
    {
        var category = new Category("Batman", "batman", "DC collectibles", true);
        SetEntityId(category, ProductImagesTestData.BatmanCategoryId);

        var studio = new Studio("Prime Studios", "prime-studios", true);
        SetEntityId(studio, ProductImagesTestData.PrimeStudiosId);

        var product = Product.Create(
            "Dark Knight 1/4",
            "dark-knight-1-4",
            "Short",
            "Long",
            ProductImagesTestData.BatmanCategoryId,
            ProductImagesTestData.PrimeStudiosId,
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
        SetEntityId(product, ProductImagesTestData.DarkKnightProductId);

        context.Categories.Add(category);
        context.Studios.Add(studio);
        context.Products.Add(product);

        return product;
    }

    private static void SetImageIds(Product product)
    {
        var images = product.Images
            .OrderBy(x => x.SortOrder)
            .ToList();

        SetEntityId(images[0], ProductImagesTestData.MainImageId);
        SetEntityId(images[1], ProductImagesTestData.DetailImageId);
        SetEntityId(images[2], ProductImagesTestData.PoseImageId);
    }

    private static void SetEntityId(object entity, Guid id)
    {
        var idProperty = entity.GetType().GetProperty("Id");
        idProperty!.SetValue(entity, id);
    }
}

internal sealed class ProductImagesTestApplicationDbContext : DbContext, IApplicationDbContext
{
    public ProductImagesTestApplicationDbContext(DbContextOptions<ProductImagesTestApplicationDbContext> options)
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

internal sealed class FakeStorageService : IStorageService
{
    public string UploadResult { get; set; } = string.Empty;
    public Exception? UploadException { get; set; }
    public Exception? DeleteException { get; set; }
    public List<string> UploadedPaths { get; } = [];
    public List<string> DeletedPaths { get; } = [];

    public Task<string> UploadAsync(string objectPath, Stream content, string contentType, CancellationToken cancellationToken = default)
    {
        if (UploadException is not null)
        {
            throw UploadException;
        }

        UploadedPaths.Add(objectPath);
        return Task.FromResult(string.IsNullOrWhiteSpace(UploadResult) ? objectPath : UploadResult);
    }

    public Task DeleteAsync(string objectPath, CancellationToken cancellationToken = default)
    {
        if (DeleteException is not null)
        {
            throw DeleteException;
        }

        DeletedPaths.Add(objectPath);
        return Task.CompletedTask;
    }
}

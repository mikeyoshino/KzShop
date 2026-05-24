using Ecommerce.Application.Common.Interfaces;
using Ecommerce.Application.Studios.CreateStudio;
using Ecommerce.Application.Studios.SearchStudios;
using Ecommerce.Domain.Entities;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ecommerce.Application.Tests.Studios;

public class CreateStudioHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateStudio_WhenNameAndSlugAreUnique()
    {
        await using var context = StudioTestApplicationDbContextFactory.CreateWithStudios();
        var handler = new CreateStudioHandler(context);
        var command = new CreateStudioCommand(" Queen Studios ", " QUEEN-STUDIOS ", true);

        var response = await handler.Handle(command, CancellationToken.None);

        response.Name.Should().Be("Queen Studios");
        response.Slug.Should().Be("queen-studios");
        response.IsActive.Should().BeTrue();

        var createdStudio = await context.Studios.SingleAsync(x => x.Id == response.Id);
        createdStudio.Name.Should().Be("Queen Studios");
        createdStudio.Slug.Should().Be("queen-studios");
        createdStudio.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenSlugAlreadyExists()
    {
        await using var context = StudioTestApplicationDbContextFactory.CreateWithStudios();
        var handler = new CreateStudioHandler(context);
        var command = new CreateStudioCommand("Another Studio", "prime-studios", true);

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Studio slug already exists.");
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenNameAlreadyExists()
    {
        await using var context = StudioTestApplicationDbContextFactory.CreateWithStudios();
        var handler = new CreateStudioHandler(context);
        var command = new CreateStudioCommand("  Prime Studios  ", "prime-studios-2", false);

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Studio name already exists.");
    }

    [Fact]
    public async Task Handle_ShouldThrowMeaningfulMessage_WhenSaveFailsWithUniqueNameViolation()
    {
        await using var context = new ThrowOnSaveStudioDbContext(
            new DbContextOptionsBuilder<StudioTestApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
                .Options,
            new DbUpdateException("Cannot insert duplicate key row in object 'dbo.Studios' with unique index 'IX_Studios_Name'."));

        var handler = new CreateStudioHandler(context);
        var command = new CreateStudioCommand("Queen Studios", "queen-studios", true);

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Studio name already exists.");
    }
}

public class SearchStudiosHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnStudios_FilteredBySearchTerm()
    {
        await using var context = StudioTestApplicationDbContextFactory.CreateWithStudios();
        var handler = new SearchStudiosHandler(context);

        var response = await handler.Handle(new SearchStudiosQuery("prime"), CancellationToken.None);

        response.Items.Should().ContainSingle();
        response.Items[0].Name.Should().Be("Prime Studios");
        response.Items[0].Slug.Should().Be("prime-studios");
    }
}

public class CreateStudioValidatorTests
{
    [Theory]
    [InlineData("prime-studios")]
    [InlineData("prime-studios-2")]
    [InlineData("a")]
    public void Validate_ShouldNotHaveError_WhenSlugHasValidFormat(string slug)
    {
        var validator = new CreateStudioValidator();

        var result = validator.TestValidate(new CreateStudioCommand("Prime Studios", slug, true));

        result.ShouldNotHaveValidationErrorFor(x => x.Slug);
    }

    [Theory]
    [InlineData("Prime-Studios")]
    [InlineData("prime studios")]
    [InlineData("prime_studios")]
    [InlineData("-prime-studios")]
    [InlineData("prime-studios-")]
    public void Validate_ShouldHaveError_WhenSlugFormatIsInvalid(string slug)
    {
        var validator = new CreateStudioValidator();

        var result = validator.TestValidate(new CreateStudioCommand("Prime Studios", slug, true));

        result.ShouldHaveValidationErrorFor(x => x.Slug);
    }
}

internal static class StudioTestApplicationDbContextFactory
{
    public static StudioTestApplicationDbContext CreateWithStudios()
    {
        var options = new DbContextOptionsBuilder<StudioTestApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        var context = new StudioTestApplicationDbContext(options);
        context.Studios.AddRange(
            new Studio("Prime Studios", "prime-studios", true),
            new Studio("Hot Toys", "hot-toys", true));
        context.SaveChanges();

        return context;
    }
}

internal class StudioTestApplicationDbContext : DbContext, IApplicationDbContext
{
    public StudioTestApplicationDbContext(DbContextOptions<StudioTestApplicationDbContext> options)
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

internal sealed class ThrowOnSaveStudioDbContext : IApplicationDbContext, IAsyncDisposable
{
    private readonly StudioTestApplicationDbContext _innerContext;
    private readonly Exception _exceptionToThrow;

    public ThrowOnSaveStudioDbContext(DbContextOptions<StudioTestApplicationDbContext> options, Exception exceptionToThrow)
    {
        _innerContext = new StudioTestApplicationDbContext(options);
        _exceptionToThrow = exceptionToThrow;
    }

    public DbSet<Category> Categories => _innerContext.Categories;
    public DbSet<Studio> Studios => _innerContext.Studios;
    public DbSet<Product> Products => _innerContext.Products;
    public DbSet<ProductImage> ProductImages => _innerContext.ProductImages;
    public DbSet<ProductSpecification> ProductSpecifications => _innerContext.ProductSpecifications;
    public DbSet<InventoryItem> InventoryItems => _innerContext.InventoryItems;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        throw _exceptionToThrow;
    }

    public ValueTask DisposeAsync()
    {
        return _innerContext.DisposeAsync();
    }
}

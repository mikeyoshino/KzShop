using Ecommerce.Application.Categories.CreateCategory;
using Ecommerce.Application.Categories.UpdateCategory;
using Ecommerce.Application.Common.Interfaces;
using Ecommerce.Domain.Entities;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ecommerce.Application.Tests.Categories;

public class CreateCategoryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateCategory_WhenNameAndSlugAreUnique()
    {
        await using var context = CategoryTestApplicationDbContextFactory.CreateWithCategories();
        var handler = new CreateCategoryHandler(context);
        var command = new CreateCategoryCommand(" Marvel ", " MARVEL ", " Licensed collectibles ", true);

        var response = await handler.Handle(command, CancellationToken.None);

        response.Name.Should().Be("Marvel");
        response.Slug.Should().Be("marvel");
        response.Description.Should().Be("Licensed collectibles");
        response.IsActive.Should().BeTrue();

        var createdCategory = await context.Categories.SingleAsync(x => x.Id == response.Id);
        createdCategory.Name.Should().Be("Marvel");
        createdCategory.Slug.Should().Be("marvel");
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenSlugAlreadyExists()
    {
        await using var context = CategoryTestApplicationDbContextFactory.CreateWithCategories();
        var handler = new CreateCategoryHandler(context);
        var command = new CreateCategoryCommand("Comics", "statues", "desc", true);

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Category slug already exists.");
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenNameAlreadyExists()
    {
        await using var context = CategoryTestApplicationDbContextFactory.CreateWithCategories();
        var handler = new CreateCategoryHandler(context);
        var command = new CreateCategoryCommand("  Statues  ", "statues-2", "desc", true);

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Category name already exists.");
    }
}

public class UpdateCategoryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldUpdateCategory_WhenNameAndSlugAreUnique()
    {
        await using var context = CategoryTestApplicationDbContextFactory.CreateWithCategories();
        var handler = new UpdateCategoryHandler(context);
        var existing = await context.Categories.SingleAsync(x => x.Slug == "figures");
        var command = new UpdateCategoryCommand(existing.Id, " Action Figures ", " ACTION-FIGURES ", " Updated desc ", false);

        var response = await handler.Handle(command, CancellationToken.None);

        response.Name.Should().Be("Action Figures");
        response.Slug.Should().Be("action-figures");
        response.Description.Should().Be("Updated desc");
        response.IsActive.Should().BeFalse();

        var updated = await context.Categories.SingleAsync(x => x.Id == existing.Id);
        updated.Name.Should().Be("Action Figures");
        updated.Slug.Should().Be("action-figures");
        updated.Description.Should().Be("Updated desc");
        updated.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenUpdatingToDuplicateSlug()
    {
        await using var context = CategoryTestApplicationDbContextFactory.CreateWithCategories();
        var handler = new UpdateCategoryHandler(context);
        var existing = await context.Categories.SingleAsync(x => x.Slug == "figures");
        var command = new UpdateCategoryCommand(existing.Id, "Figures", "statues", "desc", true);

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Category slug already exists.");
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenUpdatingToDuplicateName()
    {
        await using var context = CategoryTestApplicationDbContextFactory.CreateWithCategories();
        var handler = new UpdateCategoryHandler(context);
        var existing = await context.Categories.SingleAsync(x => x.Slug == "figures");
        var command = new UpdateCategoryCommand(existing.Id, "Statues", "figures-new", "desc", true);

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Category name already exists.");
    }
}

public class CategoryValidatorsTests
{
    [Theory]
    [InlineData("collectibles")]
    [InlineData("collectibles-2")]
    [InlineData("a")]
    public void CreateValidator_ShouldNotHaveError_WhenSlugHasValidFormat(string slug)
    {
        var validator = new CreateCategoryValidator();

        var result = validator.TestValidate(new CreateCategoryCommand("Collectibles", slug, "desc", true));

        result.ShouldNotHaveValidationErrorFor(x => x.Slug);
    }

    [Theory]
    [InlineData("Collectibles")]
    [InlineData("collectibles toys")]
    [InlineData("collectibles_toys")]
    public void CreateValidator_ShouldHaveError_WhenSlugFormatIsInvalid(string slug)
    {
        var validator = new CreateCategoryValidator();

        var result = validator.TestValidate(new CreateCategoryCommand("Collectibles", slug, "desc", true));

        result.ShouldHaveValidationErrorFor(x => x.Slug);
    }
}

internal static class CategoryTestApplicationDbContextFactory
{
    public static CategoryTestApplicationDbContext CreateWithCategories()
    {
        var options = new DbContextOptionsBuilder<CategoryTestApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        var context = new CategoryTestApplicationDbContext(options);
        context.Categories.AddRange(
            new Category("Statues", "statues", "Premium statues", true),
            new Category("Figures", "figures", "Action figures", true));
        context.SaveChanges();

        return context;
    }
}

internal class CategoryTestApplicationDbContext : DbContext, IApplicationDbContext
{
    public CategoryTestApplicationDbContext(DbContextOptions<CategoryTestApplicationDbContext> options)
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

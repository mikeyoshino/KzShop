using Ecommerce.Application.Categories.CreateCategory;
using Ecommerce.Application.Categories.UpdateCategory;
using Ecommerce.Application.Common.Interfaces;
using Ecommerce.Application.Common.Models;
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
        response.IsSuccess.Should().BeTrue();
        response.Value.Should().NotBeNull();

        response.Value!.Name.Should().Be("Marvel");
        response.Value.Slug.Should().Be("marvel");
        response.Value.Description.Should().Be("Licensed collectibles");
        response.Value.IsActive.Should().BeTrue();

        var createdCategory = await context.Categories.SingleAsync(x => x.Id == response.Value.Id);
        createdCategory.Name.Should().Be("Marvel");
        createdCategory.Slug.Should().Be("marvel");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenSlugAlreadyExists()
    {
        await using var context = CategoryTestApplicationDbContextFactory.CreateWithCategories();
        var handler = new CreateCategoryHandler(context);
        var command = new CreateCategoryCommand("Comics", "statues", "desc", true);

        var response = await handler.Handle(command, CancellationToken.None);
        response.IsFailure.Should().BeTrue();
        response.ErrorCode.Should().Be(BusinessErrorCode.DuplicateSlug);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenNameAlreadyExists()
    {
        await using var context = CategoryTestApplicationDbContextFactory.CreateWithCategories();
        var handler = new CreateCategoryHandler(context);
        var command = new CreateCategoryCommand("  Statues  ", "statues-2", "desc", true);

        var response = await handler.Handle(command, CancellationToken.None);
        response.IsFailure.Should().BeTrue();
        response.ErrorCode.Should().Be(BusinessErrorCode.DuplicateName);
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
        response.IsSuccess.Should().BeTrue();
        response.Value.Should().NotBeNull();

        response.Value!.Name.Should().Be("Action Figures");
        response.Value.Slug.Should().Be("action-figures");
        response.Value.Description.Should().Be("Updated desc");
        response.Value.IsActive.Should().BeFalse();

        var updated = await context.Categories.SingleAsync(x => x.Id == existing.Id);
        updated.Name.Should().Be("Action Figures");
        updated.Slug.Should().Be("action-figures");
        updated.Description.Should().Be("Updated desc");
        updated.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUpdatingToDuplicateSlug()
    {
        await using var context = CategoryTestApplicationDbContextFactory.CreateWithCategories();
        var handler = new UpdateCategoryHandler(context);
        var existing = await context.Categories.SingleAsync(x => x.Slug == "figures");
        var command = new UpdateCategoryCommand(existing.Id, "Figures", "statues", "desc", true);

        var response = await handler.Handle(command, CancellationToken.None);
        response.IsFailure.Should().BeTrue();
        response.ErrorCode.Should().Be(BusinessErrorCode.DuplicateSlug);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUpdatingToDuplicateName()
    {
        await using var context = CategoryTestApplicationDbContextFactory.CreateWithCategories();
        var handler = new UpdateCategoryHandler(context);
        var existing = await context.Categories.SingleAsync(x => x.Slug == "figures");
        var command = new UpdateCategoryCommand(existing.Id, "Statues", "figures-new", "desc", true);

        var response = await handler.Handle(command, CancellationToken.None);
        response.IsFailure.Should().BeTrue();
        response.ErrorCode.Should().Be(BusinessErrorCode.DuplicateName);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenCategoryDoesNotExist()
    {
        await using var context = CategoryTestApplicationDbContextFactory.CreateWithCategories();
        var handler = new UpdateCategoryHandler(context);
        var command = new UpdateCategoryCommand(Guid.NewGuid(), "Name", "name", "desc", true);

        var response = await handler.Handle(command, CancellationToken.None);
        response.IsFailure.Should().BeTrue();
        response.ErrorCode.Should().Be(BusinessErrorCode.CategoryNotFound);
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

    [Fact]
    public void UpdateValidator_ShouldHaveError_WhenIdIsEmpty()
    {
        var validator = new UpdateCategoryValidator();

        var result = validator.TestValidate(new UpdateCategoryCommand(Guid.Empty, "Name", "name", "desc", true));

        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void UpdateValidator_ShouldHaveError_WhenNameExceedsMaxLength()
    {
        var validator = new UpdateCategoryValidator();
        var name = new string('n', 121);

        var result = validator.TestValidate(new UpdateCategoryCommand(Guid.NewGuid(), name, "name", "desc", true));

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void UpdateValidator_ShouldHaveError_WhenSlugExceedsMaxLength()
    {
        var validator = new UpdateCategoryValidator();
        var slug = new string('a', 161);

        var result = validator.TestValidate(new UpdateCategoryCommand(Guid.NewGuid(), "Name", slug, "desc", true));

        result.ShouldHaveValidationErrorFor(x => x.Slug);
    }

    [Fact]
    public void UpdateValidator_ShouldHaveError_WhenDescriptionExceedsMaxLength()
    {
        var validator = new UpdateCategoryValidator();
        var description = new string('d', 501);

        var result = validator.TestValidate(new UpdateCategoryCommand(Guid.NewGuid(), "Name", "name", description, true));

        result.ShouldHaveValidationErrorFor(x => x.Description);
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
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<OrderItem>().Property<Guid>("Id");
        modelBuilder.Entity<OrderItem>().HasKey("Id");
    }
}

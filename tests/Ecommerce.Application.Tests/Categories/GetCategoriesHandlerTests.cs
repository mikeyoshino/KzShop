using Ecommerce.Application.Categories.GetCategories;
using FluentAssertions;
using Xunit;

namespace Ecommerce.Application.Tests.Categories;

public class GetCategoriesHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnAllCategories_OrderedByName_WhenSearchIsEmpty()
    {
        await using var context = CategoryTestApplicationDbContextFactory.CreateWithCategories();
        var handler = new GetCategoriesHandler(context);

        var response = await handler.Handle(new GetCategoriesQuery(null), CancellationToken.None);

        response.Items.Should().HaveCount(2);
        response.Items.Select(x => x.Name).Should().ContainInOrder("Figures", "Statues");
    }

    [Fact]
    public async Task Handle_ShouldFilterByNameOrSlug_WhenSearchProvided()
    {
        await using var context = CategoryTestApplicationDbContextFactory.CreateWithCategories();
        var handler = new GetCategoriesHandler(context);

        var response = await handler.Handle(new GetCategoriesQuery("stat"), CancellationToken.None);

        response.Items.Should().ContainSingle();
        response.Items[0].Slug.Should().Be("statues");
    }
}

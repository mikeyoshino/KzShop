using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using Ecommerce.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace Ecommerce.Domain.Tests.Entities;

public class ProductTests
{
    [Fact]
    public void Create_ShouldThrow_WhenPriceIsNotPositive()
    {
        var action = () => Product.Create(
            "Dark Knight",
            "dark-knight",
            "Short",
            "Long",
            Guid.NewGuid(),
            Guid.NewGuid(),
            "DK-001",
            0m,
            "USD",
            ProductStatus.Active,
            true,
            true,
            "1/4 Scale",
            "Polystone",
            "H: 24 x W: 12 x D: 10",
            "1000",
            "Q4 2026");

        action.Should().Throw<DomainException>()
            .WithMessage("Product price must be greater than zero.");
    }

    [Fact]
    public void Archive_ShouldMakeProductNotPurchasable()
    {
        var product = Product.Create(
            "Dark Knight",
            "dark-knight",
            "Short",
            "Long",
            Guid.NewGuid(),
            Guid.NewGuid(),
            "DK-001",
            650m,
            "USD",
            ProductStatus.Active,
            true,
            true,
            "1/4 Scale",
            "Polystone",
            "H: 24 x W: 12 x D: 10",
            "1000",
            "Q4 2026");

        product.Archive();

        product.Status.Should().Be(ProductStatus.Archived);
        product.IsPurchasable.Should().BeFalse();
    }
}

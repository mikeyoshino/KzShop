using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace Ecommerce.Domain.Tests.Entities;

public class OrderTests
{
    [Fact]
    public void Create_ShouldCopyItemPricingData()
    {
        var productId = Guid.NewGuid();
        var order = Order.Create(
            "ORD-1",
            "Bruce Wayne",
            "bruce@wayneenterprises.com",
            DateTimeOffset.UtcNow,
            [
                (productId, "Batman Bust", "BAT-001", 2, 125.50m, "usd")
            ],
            OrderStatus.Pending);

        var item = order.Items.Single();

        item.ProductId.Should().Be(productId);
        item.ProductName.Should().Be("Batman Bust");
        item.ProductSku.Should().Be("BAT-001");
        item.Quantity.Should().Be(2);
        item.UnitPriceAmount.Should().Be(125.50m);
        item.Currency.Should().Be("USD");
    }

    [Fact]
    public void SubtotalAmount_ShouldSumLineTotals()
    {
        var order = Order.Create(
            "ORD-1",
            "Bruce Wayne",
            "bruce@wayneenterprises.com",
            DateTimeOffset.UtcNow,
            [
                (Guid.NewGuid(), "A", "SKU-A", 2, 100m, "USD"),
                (Guid.NewGuid(), "B", "SKU-B", 1, 250m, "USD")
            ],
            OrderStatus.Pending);

        order.SubtotalAmount.Should().Be(450m);
    }

    [Theory]
    [InlineData(OrderStatus.Pending, false)]
    [InlineData(OrderStatus.Cancelled, false)]
    [InlineData(OrderStatus.Paid, true)]
    [InlineData(OrderStatus.Processing, true)]
    [InlineData(OrderStatus.Shipped, true)]
    public void IsRevenueRecognized_ShouldMatchRecognizedStatuses(OrderStatus status, bool expected)
    {
        var order = Order.Create(
            "ORD-1",
            "Bruce Wayne",
            "bruce@wayneenterprises.com",
            DateTimeOffset.UtcNow,
            [
                (Guid.NewGuid(), "A", "SKU-A", 1, 100m, "USD")
            ],
            status);

        order.IsRevenueRecognized.Should().Be(expected);
    }

    [Fact]
    public void Create_ShouldThrow_WhenItemsAreNull()
    {
        var act = () => Order.Create("ORD-1", "Bruce Wayne", "bruce@wayneenterprises.com", DateTimeOffset.UtcNow, null!, OrderStatus.Pending);
        act.Should().Throw<Ecommerce.Domain.Exceptions.DomainException>()
            .WithMessage("Order items are required.");
    }

    [Fact]
    public void Create_ShouldThrow_WhenItemsAreEmpty()
    {
        var act = () => Order.Create("ORD-1", "Bruce Wayne", "bruce@wayneenterprises.com", DateTimeOffset.UtcNow, [], OrderStatus.Pending);
        act.Should().Throw<Ecommerce.Domain.Exceptions.DomainException>()
            .WithMessage("Order must contain at least one item.");
    }

    [Fact]
    public void Create_ShouldThrow_WhenStatusIsInvalid()
    {
        var act = () => Order.Create(
            "ORD-1",
            "Bruce Wayne",
            "bruce@wayneenterprises.com",
            DateTimeOffset.UtcNow,
            [
                (Guid.NewGuid(), "A", "SKU-A", 1, 100m, "USD")
            ],
            (OrderStatus)999);

        act.Should().Throw<Ecommerce.Domain.Exceptions.DomainException>()
            .WithMessage("Order status is invalid.");
    }
}

using Ecommerce.Domain.Common;
using Ecommerce.Domain.Enums;
using Ecommerce.Domain.Exceptions;

namespace Ecommerce.Domain.Entities;

public class Order : BaseEntity
{
    private readonly List<OrderItem> _items = [];

    private Order()
    {
    }

    public OrderStatus Status { get; private set; }
    public string OrderNumber { get; private set; } = string.Empty;
    public string CustomerName { get; private set; } = string.Empty;
    public string CustomerEmail { get; private set; } = string.Empty;
    public string Currency { get; private set; } = "USD";
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? PaidAtUtc { get; private set; }

    public IReadOnlyCollection<OrderItem> Items => _items;

    public decimal SubtotalAmount => _items.Sum(x => x.LineTotalAmount);

    public bool IsRevenueRecognized => Status is OrderStatus.Paid or OrderStatus.Processing or OrderStatus.Shipped;

    public static Order Create(
        string orderNumber,
        string customerName,
        string customerEmail,
        DateTimeOffset createdAtUtc,
        IEnumerable<(Guid ProductId, string ProductName, string ProductSku, int Quantity, decimal UnitPriceAmount, string Currency)> items,
        OrderStatus status)
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
        {
            throw new DomainException("Order number is required.");
        }

        if (string.IsNullOrWhiteSpace(customerName))
        {
            throw new DomainException("Customer name is required.");
        }

        if (string.IsNullOrWhiteSpace(customerEmail))
        {
            throw new DomainException("Customer email is required.");
        }

        if (items is null)
        {
            throw new DomainException("Order items are required.");
        }

        if (!Enum.IsDefined(status))
        {
            throw new DomainException("Order status is invalid.");
        }

        var itemList = items.ToList();
        if (itemList.Count == 0)
        {
            throw new DomainException("Order must contain at least one item.");
        }

        var order = new Order
        {
            Id = Guid.NewGuid(),
            Status = status,
            OrderNumber = orderNumber.Trim(),
            CustomerName = customerName.Trim(),
            CustomerEmail = customerEmail.Trim(),
            CreatedAtUtc = createdAtUtc
        };

        foreach (var item in itemList)
        {
            order._items.Add(new OrderItem(
                item.ProductId,
                item.ProductName,
                item.ProductSku,
                item.Quantity,
                item.UnitPriceAmount,
                item.Currency));
        }

        order.Currency = order._items[0].Currency;
        if (status is OrderStatus.Paid or OrderStatus.Processing or OrderStatus.Shipped)
        {
            order.PaidAtUtc = createdAtUtc;
        }

        return order;
    }
}

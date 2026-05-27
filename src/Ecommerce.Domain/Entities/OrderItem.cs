using Ecommerce.Domain.Exceptions;

namespace Ecommerce.Domain.Entities;

public class OrderItem
{
    private OrderItem()
    {
    }

    public OrderItem(Guid productId, string productName, string productSku, int quantity, decimal unitPriceAmount, string currency)
    {
        if (productId == Guid.Empty)
        {
            throw new DomainException("Product is required.");
        }

        if (string.IsNullOrWhiteSpace(productName))
        {
            throw new DomainException("Product name is required.");
        }

        if (string.IsNullOrWhiteSpace(productSku))
        {
            throw new DomainException("Product SKU is required.");
        }

        if (quantity <= 0)
        {
            throw new DomainException("Quantity must be greater than zero.");
        }

        if (unitPriceAmount < 0m)
        {
            throw new DomainException("Unit price cannot be negative.");
        }

        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new DomainException("Currency is required.");
        }

        ProductId = productId;
        ProductName = productName.Trim();
        ProductSku = productSku.Trim();
        Quantity = quantity;
        UnitPriceAmount = unitPriceAmount;
        Currency = currency.Trim().ToUpperInvariant();
    }

    public Guid ProductId { get; private set; }

    public string ProductName { get; private set; } = string.Empty;

    public string ProductSku { get; private set; } = string.Empty;

    public int Quantity { get; private set; }

    public decimal UnitPriceAmount { get; private set; }

    public string Currency { get; private set; } = "USD";

    public decimal LineTotalAmount => Quantity * UnitPriceAmount;
}

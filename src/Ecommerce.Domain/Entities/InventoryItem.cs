using Ecommerce.Domain.Common;
using Ecommerce.Domain.Exceptions;

namespace Ecommerce.Domain.Entities;

public class InventoryItem : BaseEntity
{
    private InventoryItem()
    {
    }

    public InventoryItem(Guid productId, int onHandQuantity)
    {
        if (onHandQuantity < 0)
        {
            throw new DomainException("On-hand quantity cannot be negative.");
        }

        ProductId = productId;
        OnHandQuantity = onHandQuantity;
    }

    public Guid ProductId { get; private set; }

    public int OnHandQuantity { get; private set; }

    public int ReservedQuantity { get; private set; }

    public int AvailableQuantity => OnHandQuantity - ReservedQuantity;

    public void AdjustOnHand(int newOnHandQuantity)
    {
        if (newOnHandQuantity < 0)
        {
            throw new DomainException("On-hand quantity cannot be negative.");
        }

        if (newOnHandQuantity < ReservedQuantity)
        {
            throw new DomainException("On-hand quantity cannot be lower than reserved quantity.");
        }

        OnHandQuantity = newOnHandQuantity;
    }

    public void Reserve(int quantity)
    {
        if (quantity <= 0)
        {
            throw new DomainException("Reserve quantity must be greater than zero.");
        }

        if (AvailableQuantity < quantity)
        {
            throw new DomainException("Insufficient available stock to reserve.");
        }

        ReservedQuantity += quantity;
    }

    public void Release(int quantity)
    {
        if (quantity <= 0)
        {
            throw new DomainException("Release quantity must be greater than zero.");
        }

        if (quantity > ReservedQuantity)
        {
            throw new DomainException("Cannot release more than reserved quantity.");
        }

        ReservedQuantity -= quantity;
    }
}

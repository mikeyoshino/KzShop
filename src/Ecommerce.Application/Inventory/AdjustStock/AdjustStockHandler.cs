using Ecommerce.Application.Common.Interfaces;
using Ecommerce.Application.Common.Models;
using Ecommerce.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Inventory.AdjustStock;

public class AdjustStockHandler : IRequestHandler<AdjustStockCommand, Result<AdjustStockResponse>>
{
    private readonly IApplicationDbContext _context;

    public AdjustStockHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<AdjustStockResponse>> Handle(AdjustStockCommand request, CancellationToken cancellationToken)
    {
        var hasProduct = await _context.Products
            .AsNoTracking()
            .AnyAsync(x => x.Id == request.ProductId, cancellationToken);
        if (!hasProduct)
        {
            return Result.Failure<AdjustStockResponse>(BusinessErrorCode.ProductNotFound, "Product was not found.");
        }

        var inventory = await _context.InventoryItems
            .SingleOrDefaultAsync(x => x.ProductId == request.ProductId, cancellationToken);
        if (inventory is null)
        {
            return Result.Failure<AdjustStockResponse>(BusinessErrorCode.InventoryNotFound, "Inventory item was not found.");
        }

        var resultingOnHand = inventory.OnHandQuantity + request.QuantityDelta;
        if (resultingOnHand < 0 || resultingOnHand < inventory.ReservedQuantity)
        {
            return Result.Failure<AdjustStockResponse>(
                BusinessErrorCode.ValidationFailed,
                "Resulting on-hand quantity must be zero or greater and cannot be lower than reserved stock.");
        }

        try
        {
            inventory.AdjustOnHand(resultingOnHand);
        }
        catch (DomainException ex)
        {
            return Result.Failure<AdjustStockResponse>(BusinessErrorCode.ValidationFailed, ex.Message);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(new AdjustStockResponse(
            inventory.ProductId,
            inventory.OnHandQuantity,
            inventory.ReservedQuantity,
            inventory.AvailableQuantity));
    }
}

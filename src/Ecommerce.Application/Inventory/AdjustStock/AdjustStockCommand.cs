using Ecommerce.Application.Common.Models;
using MediatR;

namespace Ecommerce.Application.Inventory.AdjustStock;

public record AdjustStockCommand(
    Guid ProductId,
    int QuantityDelta) : IRequest<Result<AdjustStockResponse>>;

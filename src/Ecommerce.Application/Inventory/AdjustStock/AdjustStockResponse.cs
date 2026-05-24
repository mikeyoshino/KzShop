namespace Ecommerce.Application.Inventory.AdjustStock;

public record AdjustStockResponse(
    Guid ProductId,
    int OnHandQuantity,
    int ReservedQuantity,
    int AvailableQuantity);

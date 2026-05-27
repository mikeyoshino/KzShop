namespace Ecommerce.Application.Dashboard.GetAdminDashboardMetrics;

public record GetAdminDashboardMetricsResponse(
    decimal PeriodRevenue,
    decimal RecognizedRevenue,
    int OrderCount,
    IReadOnlyList<CriticalInventoryItemResponse> CriticalInventoryItems,
    IReadOnlyList<RecentOrderResponse> RecentOrders);

public record CriticalInventoryItemResponse(
    Guid ProductId,
    string ProductName,
    string StudioName,
    string? PrimaryImageUrl,
    int AvailableQuantity);

public record RecentOrderResponse(
    Guid OrderId,
    string OrderNumber,
    DateTimeOffset CreatedAtUtc,
    string CustomerName,
    decimal TotalAmount,
    string Currency,
    string Status);

using Ecommerce.Application.Common.Interfaces;
using Ecommerce.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Application.Dashboard.GetAdminDashboardMetrics;

public class GetAdminDashboardMetricsHandler : IRequestHandler<GetAdminDashboardMetricsQuery, GetAdminDashboardMetricsResponse>
{
    private readonly IApplicationDbContext _context;

    public GetAdminDashboardMetricsHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GetAdminDashboardMetricsResponse> Handle(GetAdminDashboardMetricsQuery request, CancellationToken cancellationToken)
    {
        var periodStartUtc = DateTimeOffset.UtcNow.AddDays(-30);

        var criticalInventoryItems = await (
            from inventory in _context.InventoryItems.AsNoTracking()
            join product in _context.Products.AsNoTracking() on inventory.ProductId equals product.Id
            join studio in _context.Studios.AsNoTracking() on product.StudioId equals studio.Id
            join image in _context.ProductImages.AsNoTracking() on product.Id equals image.ProductId into imageGroup
            let availableQuantity = inventory.OnHandQuantity - inventory.ReservedQuantity
            where product.Status == ProductStatus.Active && availableQuantity < 5
            orderby availableQuantity, product.Name
            select new CriticalInventoryItemResponse(
                product.Id,
                product.Name,
                studio.Name,
                imageGroup.OrderBy(x => x.SortOrder).Select(x => x.PublicUrl).FirstOrDefault(),
                availableQuantity))
            .ToListAsync(cancellationToken);

        var recentOrders = await _context.Orders
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAtUtc)
            .ThenByDescending(x => x.Id)
            .Take(5)
            .Select(x => new RecentOrderResponse(
                x.Id,
                x.OrderNumber,
                x.CreatedAtUtc,
                x.CustomerName,
                x.Items.Sum(i => i.UnitPriceAmount * i.Quantity),
                x.Currency,
                x.Status.ToString()))
            .ToListAsync(cancellationToken);

        var recognizedRevenue = await _context.Orders
            .AsNoTracking()
            .Where(order =>
                order.Status == OrderStatus.Paid
                || order.Status == OrderStatus.Processing
                || order.Status == OrderStatus.Shipped)
            .Select(order => order.Items.Sum(item => item.UnitPriceAmount * item.Quantity))
            .SumAsync(cancellationToken);

        var periodRevenue = await _context.Orders
            .AsNoTracking()
            .Where(order =>
                (order.Status == OrderStatus.Paid
                || order.Status == OrderStatus.Processing
                || order.Status == OrderStatus.Shipped)
                && order.CreatedAtUtc >= periodStartUtc)
            .Select(order => order.Items.Sum(item => item.UnitPriceAmount * item.Quantity))
            .SumAsync(cancellationToken);

        var orderCount = await _context.Orders
            .AsNoTracking()
            .CountAsync(cancellationToken);

        return new GetAdminDashboardMetricsResponse(
            periodRevenue,
            recognizedRevenue,
            orderCount,
            criticalInventoryItems,
            recentOrders);
    }
}

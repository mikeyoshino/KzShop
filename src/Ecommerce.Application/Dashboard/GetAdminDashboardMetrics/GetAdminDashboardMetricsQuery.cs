using MediatR;

namespace Ecommerce.Application.Dashboard.GetAdminDashboardMetrics;

public record GetAdminDashboardMetricsQuery() : IRequest<GetAdminDashboardMetricsResponse>;

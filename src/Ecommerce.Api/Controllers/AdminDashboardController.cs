using Ecommerce.Application.Dashboard.GetAdminDashboardMetrics;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers;

[Route("api/admin/dashboard")]
public class AdminDashboardController : AdminApiControllerBase
{
    private readonly ISender _sender;

    public AdminDashboardController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("metrics")]
    public Task<GetAdminDashboardMetricsResponse> GetMetrics(CancellationToken cancellationToken)
    {
        return _sender.Send(new GetAdminDashboardMetricsQuery(), cancellationToken);
    }
}

using Ecommerce.Application.Inventory.AdjustStock;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers;

[Route("api/admin/inventory")]
public class AdminInventoryController : AdminApiControllerBase
{
    private readonly ISender _sender;

    public AdminInventoryController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("adjust")]
    public async Task<ActionResult<AdjustStockResponse>> AdjustStock(
        [FromBody] AdjustStockCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return MapFailure(result);
        }

        return Ok(result.Value);
    }
}

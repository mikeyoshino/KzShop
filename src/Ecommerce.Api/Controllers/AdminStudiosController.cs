using Ecommerce.Application.Studios.CreateStudio;
using Ecommerce.Application.Studios.SearchStudios;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers;

[Route("api/admin/studios")]
public class AdminStudiosController : AdminApiControllerBase
{
    private readonly ISender _sender;

    public AdminStudiosController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public Task<SearchStudiosResponse> Search([FromQuery] string? search, CancellationToken cancellationToken)
    {
        return _sender.Send(new SearchStudiosQuery(search), cancellationToken);
    }

    [HttpPost]
    public async Task<ActionResult<CreateStudioResponse>> Create(
        [FromBody] CreateStudioCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return MapFailure(result);
        }

        return CreatedAtAction(nameof(Search), new { search = result.Value!.Slug }, result.Value);
    }
}

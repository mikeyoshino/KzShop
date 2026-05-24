using Ecommerce.Application.Categories.CreateCategory;
using Ecommerce.Application.Categories.UpdateCategory;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers;

[Route("api/admin/categories")]
public class AdminCategoriesController : AdminApiControllerBase
{
    private readonly ISender _sender;

    public AdminCategoriesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    public async Task<ActionResult<CreateCategoryResponse>> Create(
        [FromBody] CreateCategoryCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return MapFailure(result);
        }

        return CreatedAtAction(nameof(Create), new { id = result.Value!.Id }, result.Value);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UpdateCategoryResponse>> Update(
        Guid id,
        [FromBody] UpdateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new UpdateCategoryCommand(id, request.Name, request.Slug, request.Description, request.IsActive),
            cancellationToken);

        if (result.IsFailure)
        {
            return MapFailure(result);
        }

        return Ok(result.Value);
    }

    public sealed record UpdateCategoryRequest(string Name, string Slug, string Description, bool IsActive);
}

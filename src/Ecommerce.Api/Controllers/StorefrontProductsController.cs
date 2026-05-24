using Ecommerce.Application.Products.GetProductBySlug;
using Ecommerce.Application.Products.GetProducts;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/storefront/products")]
public class StorefrontProductsController : ControllerBase
{
    private readonly ISender _sender;

    public StorefrontProductsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public Task<GetProductsResponse> GetProducts(
        [FromQuery] string? category,
        [FromQuery] string? status,
        [FromQuery] string? scale,
        CancellationToken cancellationToken)
    {
        return _sender.Send(new GetProductsQuery(category, status, scale), cancellationToken);
    }

    [HttpGet("{slug}")]
    public async Task<ActionResult<GetProductBySlugResponse>> GetProductBySlug(string slug, CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetProductBySlugQuery(slug), cancellationToken);
        return response is null ? NotFound() : Ok(response);
    }
}

using Ecommerce.Application.Products.ArchiveProduct;
using Ecommerce.Application.Products.CreateProduct;
using Ecommerce.Application.Products.GetAdminProductById;
using Ecommerce.Application.Products.GetProducts;
using Ecommerce.Application.Products.UpdateProduct;
using Ecommerce.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers;

[Route("api/admin/products")]
public class AdminProductsController : AdminApiControllerBase
{
    private readonly ISender _sender;

    public AdminProductsController(ISender sender)
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

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GetAdminProductByIdResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetAdminProductByIdQuery(id), cancellationToken);
        return response is null ? NotFound() : Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<CreateProductResponse>> Create(
        [FromBody] CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new CreateProductCommand(
                request.Name,
                request.Slug,
                request.ShortDescription,
                request.Description,
                request.CategoryId,
                request.StudioId,
                request.Sku,
                request.PriceAmount,
                request.Currency,
                request.Status,
                request.IsNew,
                request.IsFeatured,
                request.Scale,
                request.Materials,
                request.DimensionsText,
                request.EditionSize,
                request.EstimatedReleaseText,
                request.Specifications),
            cancellationToken);

        if (result.IsFailure)
        {
            return MapFailure(result);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UpdateProductResponse>> Update(
        Guid id,
        [FromBody] UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new UpdateProductCommand(
                id,
                request.Name,
                request.Slug,
                request.ShortDescription,
                request.Description,
                request.CategoryId,
                request.StudioId,
                request.Sku,
                request.PriceAmount,
                request.Currency,
                request.Status,
                request.IsNew,
                request.IsFeatured,
                request.Scale,
                request.Materials,
                request.DimensionsText,
                request.EditionSize,
                request.EstimatedReleaseText,
                request.Specifications),
            cancellationToken);

        if (result.IsFailure)
        {
            return MapFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpPost("{id:guid}/archive")]
    public async Task<ActionResult<ArchiveProductResponse>> Archive(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ArchiveProductCommand(id), cancellationToken);
        if (result.IsFailure)
        {
            return MapFailure(result);
        }

        return Ok(result.Value);
    }

    public sealed record CreateProductRequest(
        string Name,
        string Slug,
        string ShortDescription,
        string Description,
        Guid CategoryId,
        Guid StudioId,
        string Sku,
        decimal PriceAmount,
        string Currency,
        ProductStatus Status,
        bool IsNew,
        bool IsFeatured,
        string Scale,
        string Materials,
        string DimensionsText,
        string EditionSize,
        string EstimatedReleaseText,
        IReadOnlyList<CreateProductSpecificationInput> Specifications);

    public sealed record UpdateProductRequest(
        string Name,
        string Slug,
        string ShortDescription,
        string Description,
        Guid CategoryId,
        Guid StudioId,
        string Sku,
        decimal PriceAmount,
        string Currency,
        ProductStatus Status,
        bool IsNew,
        bool IsFeatured,
        string Scale,
        string Materials,
        string DimensionsText,
        string EditionSize,
        string EstimatedReleaseText,
        IReadOnlyList<UpdateProductSpecificationInput> Specifications);
}

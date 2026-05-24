using Ecommerce.Application.ProductImages.DeleteProductImage;
using Ecommerce.Application.ProductImages.ReorderProductImages;
using Ecommerce.Application.ProductImages.UploadProductImage;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers;

[Route("api/admin/products/{productId:guid}/images")]
public class AdminProductImagesController : AdminApiControllerBase
{
    private readonly ISender _sender;

    public AdminProductImagesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult<UploadProductImageResponse>> Upload(
        Guid productId,
        [FromForm] UploadProductImageRequest request,
        CancellationToken cancellationToken)
    {
        if (request.File is null || request.File.Length == 0)
        {
            return BadRequest(new { code = "ValidationFailed", message = "Image file is required." });
        }

        await using var stream = request.File.OpenReadStream();
        var result = await _sender.Send(
            new UploadProductImageCommand(
                productId,
                request.File.FileName,
                request.File.ContentType,
                stream,
                request.AltText ?? string.Empty),
            cancellationToken);

        if (result.IsFailure)
        {
            return MapFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpPut("reorder")]
    public async Task<ActionResult<ReorderProductImagesResponse>> Reorder(
        Guid productId,
        [FromBody] ReorderProductImagesRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new ReorderProductImagesCommand(productId, request.OrderedImageIds),
            cancellationToken);

        if (result.IsFailure)
        {
            return MapFailure(result);
        }

        return Ok(result.Value);
    }

    [HttpDelete("{productImageId:guid}")]
    public async Task<ActionResult<DeleteProductImageResponse>> Delete(
        Guid productId,
        Guid productImageId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new DeleteProductImageCommand(productId, productImageId),
            cancellationToken);

        if (result.IsFailure)
        {
            return MapFailure(result);
        }

        return Ok(result.Value);
    }

    public sealed record UploadProductImageRequest(IFormFile? File, string? AltText);

    public sealed record ReorderProductImagesRequest(IReadOnlyList<Guid> OrderedImageIds);
}

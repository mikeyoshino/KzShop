using Ecommerce.Application.Common.Models;
using MediatR;

namespace Ecommerce.Application.ProductImages.ReorderProductImages;

public record ReorderProductImagesCommand(
    Guid ProductId,
    IReadOnlyList<Guid> OrderedImageIds) : IRequest<Result<ReorderProductImagesResponse>>;

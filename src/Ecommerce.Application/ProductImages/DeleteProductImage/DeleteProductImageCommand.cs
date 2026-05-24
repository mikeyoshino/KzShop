using Ecommerce.Application.Common.Models;
using MediatR;

namespace Ecommerce.Application.ProductImages.DeleteProductImage;

public record DeleteProductImageCommand(
    Guid ProductId,
    Guid ProductImageId) : IRequest<Result<DeleteProductImageResponse>>;

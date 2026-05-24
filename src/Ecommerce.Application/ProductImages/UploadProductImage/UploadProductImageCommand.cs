using Ecommerce.Application.Common.Models;
using MediatR;

namespace Ecommerce.Application.ProductImages.UploadProductImage;

public record UploadProductImageCommand(
    Guid ProductId,
    string FileName,
    string ContentType,
    Stream Content,
    string AltText) : IRequest<Result<UploadProductImageResponse>>;

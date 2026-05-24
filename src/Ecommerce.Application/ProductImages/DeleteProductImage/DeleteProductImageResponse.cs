namespace Ecommerce.Application.ProductImages.DeleteProductImage;

public record DeleteProductImageResponse(
    Guid ProductId,
    Guid DeletedImageId,
    int RemainingImageCount);

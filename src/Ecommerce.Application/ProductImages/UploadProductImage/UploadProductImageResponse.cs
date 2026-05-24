namespace Ecommerce.Application.ProductImages.UploadProductImage;

public record UploadProductImageResponse(
    Guid Id,
    Guid ProductId,
    string PublicUrl,
    string AltText,
    int SortOrder);

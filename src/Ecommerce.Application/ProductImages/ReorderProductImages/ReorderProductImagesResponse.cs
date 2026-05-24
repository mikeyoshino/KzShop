namespace Ecommerce.Application.ProductImages.ReorderProductImages;

public record ReorderProductImagesResponse(
    Guid ProductId,
    IReadOnlyList<ReorderedProductImageResponse> Images);

public record ReorderedProductImageResponse(
    Guid Id,
    int SortOrder);

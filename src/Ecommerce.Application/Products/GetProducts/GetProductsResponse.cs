namespace Ecommerce.Application.Products.GetProducts;

public record GetProductsResponse(IReadOnlyList<ProductListItemResponse> Items);

public record ProductListItemResponse(
    Guid Id,
    string Name,
    string Slug,
    string CategorySlug,
    string StudioName,
    decimal PriceAmount,
    string Currency,
    string Scale,
    string Status,
    bool IsNew,
    string? PrimaryImageUrl);

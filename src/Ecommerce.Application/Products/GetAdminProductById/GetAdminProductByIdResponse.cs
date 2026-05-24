namespace Ecommerce.Application.Products.GetAdminProductById;

public record GetAdminProductByIdResponse(
    Guid Id,
    string Name,
    string Slug,
    string ShortDescription,
    string Description,
    Guid CategoryId,
    string CategoryName,
    Guid StudioId,
    string StudioName,
    string Sku,
    decimal PriceAmount,
    string Currency,
    string Status,
    bool IsNew,
    bool IsFeatured,
    string Scale,
    string Materials,
    string DimensionsText,
    string EditionSize,
    string EstimatedReleaseText,
    DateTimeOffset? PublishedAt,
    DateTimeOffset? ArchivedAt,
    int InventoryOnHand,
    int InventoryReserved,
    int InventoryAvailable,
    IReadOnlyList<AdminProductImageResponse> Images,
    IReadOnlyList<AdminProductSpecificationResponse> Specifications);

public record AdminProductImageResponse(
    Guid Id,
    string PublicUrl,
    string AltText,
    int SortOrder);

public record AdminProductSpecificationResponse(
    Guid Id,
    string Label,
    string Value,
    int SortOrder);

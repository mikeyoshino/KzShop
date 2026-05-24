namespace Ecommerce.Application.Products.GetProductBySlug;

public record GetProductBySlugResponse(
    Guid Id,
    string Name,
    string Slug,
    string StudioName,
    string CategorySlug,
    decimal PriceAmount,
    string Currency,
    string Description,
    string ShortDescription,
    string Scale,
    string Materials,
    string DimensionsText,
    string EditionSize,
    string EstimatedReleaseText,
    string Status,
    bool IsNew,
    IReadOnlyList<ProductDetailImageResponse> Images,
    IReadOnlyList<ProductDetailSpecificationResponse> Specifications);

public record ProductDetailImageResponse(
    Guid Id,
    string PublicUrl,
    string AltText,
    int SortOrder);

public record ProductDetailSpecificationResponse(
    string Label,
    string Value,
    int SortOrder);
